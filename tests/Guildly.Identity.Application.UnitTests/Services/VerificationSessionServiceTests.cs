using Guildly.Common.Results;
using Guildly.Identity.Application.Configurations;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;
using Guildly.Identity.Domain.Repositories;
using Microsoft.Extensions.Options;
using Moq;

namespace Guildly.Identity.Application.UnitTests.Services;

public class VerificationSessionServiceTests
{
    private readonly VerificationSessionService _verificationSessionService;
    private readonly Mock<IVerificationSessionRepository> _verificationSessionRepositoryMock;
    private readonly Mock<IVerificationCodeService> _verificationCodeServiceMock;
    private readonly VerificationSessionConfiguration _configuration;
    
    public VerificationSessionServiceTests()
    {
        _verificationSessionRepositoryMock = new Mock<IVerificationSessionRepository>();
        _verificationCodeServiceMock = new Mock<IVerificationCodeService>();
        _configuration = new VerificationSessionConfiguration()
        {
            AllowedAttempts = 5,
            ExpirationTimeInMinutes = 10
        };
        
        _verificationSessionService = new VerificationSessionService(
            _verificationSessionRepositoryMock.Object, 
            _verificationCodeServiceMock.Object, 
            new OptionsWrapper<VerificationSessionConfiguration>(_configuration));
    }
    
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly VerificationPurpose _purpose = VerificationPurpose.ResetPassword;
    private readonly VerificationChannel _channel = VerificationChannel.Email;
    
    private static VerificationSession CreateSession(
        VerificationPurpose purpose = VerificationPurpose.RegisterEmail,
        VerificationChannel channel = VerificationChannel.Email,
        string? sentTo = null,
        string codeHash = "code-hash",
        DateTimeOffset? createdAt = null,
        DateTimeOffset? expiresAt = null)
    {
        var now = DateTimeOffset.UtcNow;

        sentTo ??= channel == VerificationChannel.Email
            ? "user@gmail.com"
            : "+380741739347";

        return VerificationSession.Create(
            Guid.NewGuid(),
            sentTo,
            purpose,
            channel,
            codeHash,
            expiresAt ?? now.AddMinutes(10),
            createdAt ?? now).Value;
    }

    [Fact]
    public async Task StartVerificationSession_CancelPreviousSessions_And_CreatesNew()
    {
        var sentTo = "user@gmail.com";
        var code = "123456";
        var codeHash = "code-hash";
        
        var oldSession = CreateSession(_purpose);
        var newSession = CreateSession(_purpose);
        
        _verificationSessionRepositoryMock
            .Setup(r => r.GetActiveVerificationSessions(_userId, _now, _purpose, null, false))
            .ReturnsAsync([oldSession]);

        _verificationSessionRepositoryMock
            .Setup(r => r.Add(It.IsAny<VerificationSession>()))
            .Callback<VerificationSession>(s => newSession = s);

        _verificationCodeServiceMock
            .Setup(s => s.GenerateVerificationCode())
            .Returns(code);

        _verificationCodeServiceMock
            .Setup(s => s.HashCode(code))
            .Returns(codeHash);
        
        var result = await _verificationSessionService.StartVerificationSessionAsync(
            _userId, sentTo, _purpose, _channel, _now);
        
        _verificationSessionRepositoryMock.Verify(
            r => r.GetActiveVerificationSessions(_userId, _now, _purpose, null, false));
        
        Assert.Equal(_now, oldSession.CancelledAt);
        
        _verificationCodeServiceMock.Verify(
            s => s.GenerateVerificationCode(),
            Times.Once);
        _verificationCodeServiceMock.Verify(
            s => s.HashCode(code),
            Times.Once);

        _verificationSessionRepositoryMock.Verify(
            r => r.Add(It.IsAny<VerificationSession>()),
            Times.Once());
        
        Assert.True(result.IsSuccess);
        Assert.Equal(code, result.Value.Code);
        
        Assert.Equal(_userId, newSession.UserId);
        Assert.Equal(sentTo,  newSession.SentTo);
        Assert.Equal(_purpose, newSession.Purpose);
        Assert.Equal(_channel, newSession.Channel);
        Assert.Equal(codeHash, newSession.CodeHash);
        Assert.Equal(_now.AddMinutes(_configuration.ExpirationTimeInMinutes), newSession.ExpiresAt);
        Assert.Equal(_now, newSession.CreatedAt);
    }

    [Fact]
    public async Task StartVerificationSession_Should_ReturnFailure_And_NotAddSession()
    { 
        _verificationSessionRepositoryMock
            .Setup(r => r.GetActiveVerificationSessions(_userId, _now, _purpose, null, false))
            .ReturnsAsync([]);

        var result = await _verificationSessionService.StartVerificationSessionAsync(
            _userId,
            "not-valid-email",
            _purpose,
            VerificationChannel.Email,
            _now);
        
        Assert.True(result.IsFailure);
        _verificationSessionRepositoryMock.Verify(
            r => r.Add(It.IsAny<VerificationSession>()),
            Times.Never());
    }
    
    [Fact]
    public async Task CancelActiveVerificationSessions_Should_CancelActiveVerificationSessions()
    {
        var sessions = new List<VerificationSession>()
        {
            CreateSession(_purpose),
            CreateSession(_purpose),
        };
        
        _verificationSessionRepositoryMock
            .Setup(r => r.GetActiveVerificationSessions(_userId, _now, _purpose, null, false))
            .ReturnsAsync(sessions);
        
        await  _verificationSessionService.CancelActiveVerificationSessionsAsync(_userId, _purpose, _now);

        _verificationSessionRepositoryMock.Verify(
            r => r.GetActiveVerificationSessions(_userId, _now, _purpose, null, false),
            Times.Once);
        
        Assert.Equal(_now, sessions[0].CancelledAt);
        Assert.Equal(_now, sessions[1].CancelledAt);
    }

    [Fact]
    public async Task ConfirmSession_ReturnsError_WhenSessionNotFound()
    {
        _verificationSessionRepositoryMock
            .Setup(r => r.GetLastVerificationSession(_userId, _purpose, _channel, false))
            .ReturnsAsync((VerificationSession)null!);
        
        var result = await _verificationSessionService.ConfirmSessionAsync(_userId, "code", _now, _purpose, _channel);
        
        _verificationSessionRepositoryMock.Verify(
            r => r.GetLastVerificationSession(_userId, _purpose, _channel, false),
            Times.Once);
        
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.ErrorType);
    }
    
    [Fact]
    public async Task ConfirmSession_ReturnsError_WhenSessionCannotBeConfirmed()
    {
        var session = CreateSession(_purpose, createdAt: _now, expiresAt: _now.AddMinutes(-15));
        _verificationSessionRepositoryMock
            .Setup(r => r.GetLastVerificationSession(_userId, _purpose, _channel, false))
            .ReturnsAsync(session);
        
        var result = await _verificationSessionService.ConfirmSessionAsync(_userId, "code", _now, _purpose, _channel);
        
        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationSessionExpired, result.Error);
    }
    
    [Fact]
    public async Task ConfirmSession_ReturnsError_AndRegisterFailedAttempt_WhenCodeIsInvalid()
    {
        var session = CreateSession(_purpose);
        _verificationSessionRepositoryMock
            .Setup(r => r.GetLastVerificationSession(_userId, _purpose, _channel, false))
            .ReturnsAsync(session);

        _verificationCodeServiceMock
            .Setup(s => s.VerifyCode("code", session.CodeHash))
            .Returns(false);
        
        var result = await _verificationSessionService.ConfirmSessionAsync(_userId, "code", _now, _purpose, _channel);
        
        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidConfirmationCode, result.Error);
        Assert.Equal(1, session.FailedAttempts);
        Assert.Null(session.UsedAt);
    }
    
    [Fact]
    public async Task ConfirmSession_MarksSessionAsUsed_WhenSucceed()
    {
        var session = CreateSession(_purpose);
        _verificationSessionRepositoryMock
            .Setup(r => r.GetLastVerificationSession(_userId, _purpose, _channel, false))
            .ReturnsAsync(session);

        _verificationCodeServiceMock
            .Setup(s => s.VerifyCode("code", session.CodeHash))
            .Returns(true);
        
        var result = await _verificationSessionService.ConfirmSessionAsync(_userId, "code", _now, _purpose, _channel);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(_now, result.Value.UsedAt);
    }
}