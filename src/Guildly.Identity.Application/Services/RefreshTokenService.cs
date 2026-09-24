using Guildly.Common.Results;
using Guildly.Identity.Application.Configurations;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Models;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Guildly.Identity.Application.Services;

internal class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenProvider _refreshTokenProvider;

    private readonly RefreshTokenConfiguration _refreshTokenConfiguration;
    
    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository, 
                               IRefreshTokenProvider refreshTokenProvider, 
                               IOptions<RefreshTokenConfiguration> refreshTokenConfiguration)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenProvider = refreshTokenProvider;
        _refreshTokenConfiguration = refreshTokenConfiguration.Value;
    }

    public CreateRefreshTokenResult CreateRefreshToken(
        IdentityUser user,
        DateTimeOffset createdAt,
        bool rememberMe)
    {
        var token = _refreshTokenProvider.GenerateRefreshToken(user);
        var tokenHash = _refreshTokenProvider.HashRefreshToken(token);

        var expiresAt = rememberMe 
            ? createdAt.AddDays(_refreshTokenConfiguration.RememberMeRefreshTokenLifeTime) 
            : createdAt.AddDays(_refreshTokenConfiguration.RefreshTokenLifeTime);
        
        var refreshToken = RefreshToken.Create(user.Id, tokenHash, rememberMe, createdAt, expiresAt);
        _refreshTokenRepository.Add(refreshToken);
        
        return new CreateRefreshTokenResult(token, expiresAt);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(
        string token, 
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = _refreshTokenProvider.HashRefreshToken(token);
        
        var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, asNoTracking, cancellationToken);
        
        return refreshToken;
    }

    public async Task<Result> RevokeTokenAsync(
        string refreshToken, 
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken = default)
    {
        var hash = _refreshTokenProvider.HashRefreshToken(refreshToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(hash, false, cancellationToken);
        if (token is null)
        {
            return IdentityApplicationErrors.InvalidRefreshToken;
        }
        
        var tokenIsActiveResult = token.EnsureIsActive(revokedAt);
        if (tokenIsActiveResult.IsFailure)
        {
            return tokenIsActiveResult;
        }
        
        token.Revoke(revokedAt);
        return Result.Success;
    }

    public async Task RevokeAllActiveTokensAsync(
        Guid userId, 
        DateTimeOffset revokedAt, 
        CancellationToken cancellationToken = default)
    {
        var tokens = await _refreshTokenRepository.GetActiveTokensAsync(userId, revokedAt, false, cancellationToken);

        foreach (var refreshToken in tokens)
        {
            refreshToken.Revoke(revokedAt);
        }
    }
}