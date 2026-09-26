using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;

namespace Guildly.Identity.Domain.UnitTests.Entities;

public class IdentityUserTests
{
    private static IdentityUser CreateUser(
        Guid? id = null,
        string email = "user@gmail.com",
        string passwordHash = "password-hash",
        UserRole role = UserRole.User,
        DateTimeOffset? createdAt = null,
        bool active = false)
    {
        var user = IdentityUser.Create(
            id ?? Guid.NewGuid(),
            email,
            passwordHash,
            role,
            createdAt ?? DateTimeOffset.UtcNow).Value;

        if (active) user.ConfirmEmail();

        return user;
    }
    
    [Fact]
    public void Create_Should_CreatePendingUser_WhenDataIsValid()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        var email = "user@gmail.com";
        var passwordHash = "password-hash";
        
        var result = IdentityUser.Create(
            id,
            email,
            passwordHash,
            UserRole.User,
            createdAt);

        Assert.True(result.IsSuccess);

        var user = result.Value;
        Assert.Equal(id, user.Id);
        Assert.Equal(email, user.Email.Value);
        Assert.False(user.EmailConfirmed);
        Assert.Null(user.Phone);
        Assert.False(user.PhoneConfirmed);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(UserStatus.PendingRegistration, user.Status);
        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal(createdAt, user.CreatedAt);
    }
    
    [Fact]
    public void Create_Should_ReturnFailure_WhenEmailIsInvalid()
    {
        var result = IdentityUser.Create(
            Guid.NewGuid(),
            "invalid-email",
            "password-hash",
            UserRole.User,
            DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.EmailInvalidFormat, result.Error);
    }
    
    [Fact]
    public void ConfirmEmail_Should_ConfirmEmailAndActivateUser()
    {
        var user = CreateUser();

        var result = user.ConfirmEmail();

        Assert.True(result.IsSuccess);
        Assert.True(user.EmailConfirmed);
        Assert.Equal(UserStatus.Active, user.Status);
    }
    
    [Fact]
    public void ConfirmEmail_Should_ReturnFailure_WhenUserIsBlocked()
    {
        var user = CreateUser();
        user.Block();

        var result = user.ConfirmEmail();

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.UserBlocked, result.Error);
        Assert.False(user.EmailConfirmed);
        Assert.Equal(UserStatus.Blocked, user.Status);
    }
    
    [Fact]
    public void Block_Should_SetStatusToBlocked()
    {
        var user = CreateUser(active: true);

        user.Block();

        Assert.Equal(UserStatus.Blocked, user.Status);
    }

    [Fact]
    public void ChangeEmail_Should_ChangeEmailAndConfirmIt()
    {
        var user = CreateUser();
        var newEmail = "new@gmail.com";
        
        var result = user.ChangeEmail(newEmail);

        Assert.True(result.IsSuccess);
        Assert.Equal(newEmail, user.Email.Value);
        Assert.True(user.EmailConfirmed);
    }
    
    [Fact]
    public void ChangeEmail_Should_ReturnFailure_WhenUserIsBlocked()
    {
        var user = CreateUser(active: true);
        var oldEmail = user.Email;

        user.Block();

        var result = user.ChangeEmail("new@gmail.com");

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.UserBlocked, result.Error);
        Assert.Equal(oldEmail, user.Email);
    }
    
    [Fact]
    public void ChangeEmail_Should_Throw_WhenEmailIsInvalid()
    {
        var user = CreateUser(active: true);
        var oldEmail = user.Email;

        Assert.Throws<InvalidOperationException>(() => user.ChangeEmail("invalid-email"));
        Assert.Equal(oldEmail, user.Email);
    }
    
    [Fact]
    public void ChangePhone_Should_ChangePhoneAndConfirmIt()
    {
        var user = CreateUser(active: true);
        var newPhone = "+380741739347";
        
        var result = user.ChangePhone(newPhone);

        Assert.True(result.IsSuccess);
        Assert.NotNull(user.Phone);
        Assert.Equal(newPhone, user.Phone.Value);
        Assert.True(user.PhoneConfirmed);
    }

    [Fact]
    public void ChangePhone_Should_ReturnFailure_WhenUserIsBlocked()
    {
        var user = CreateUser(active: true);
        user.Block();

        var result = user.ChangePhone("+380741739347");

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.UserBlocked, result.Error);
        Assert.Null(user.Phone);
        Assert.False(user.PhoneConfirmed);
    }
    
    [Fact]
    public void ChangePhone_Should_Throw_WhenPhoneIsInvalid()
    {
        var user = CreateUser(active: true);

        Assert.Throws<InvalidOperationException>(() => user.ChangePhone("invalid-phone"));
        Assert.Null(user.Phone);
        Assert.False(user.PhoneConfirmed);
    }
    
    [Fact]
    public void RemovePhone_Should_ClearPhoneAndConfirmation()
    {
        var user = CreateUser(active: true);
        user.ChangePhone("+380741739347");

        var result = user.RemovePhone();

        Assert.True(result.IsSuccess);
        Assert.Null(user.Phone);
        Assert.False(user.PhoneConfirmed);
    }
    
    [Fact]
    public void RemovePhone_Should_ReturnFailure_WhenUserIsBlocked()
    {
        var user = CreateUser(active: true);
        user.ChangePhone("+380741739347");
        user.Block();

        var result = user.RemovePhone();

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.UserBlocked, result.Error);
        Assert.NotNull(user.Phone);
        Assert.True(user.PhoneConfirmed);
    }
    
    [Fact]
    public void ConfirmPhone_Should_ReturnFailure_WhenPhoneIsNotProvided()
    {
        var user = CreateUser(active: true);

        var result = user.ConfirmPhone();

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.PhoneNotProvided, result.Error);
    }
    
    [Fact]
    public void ConfirmPhone_Should_ReturnFailure_WhenUserIsBlocked()
    {
        var user = CreateUser(active: true);
        user.Block();

        var result = user.ConfirmPhone();

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.UserBlocked, result.Error);
    }
    
    [Fact]
    public void GetContact_Should_ReturnEmail_WhenEmailChannelIsRequestedAndEmailConfirmed()
    {
        var user = CreateUser(active: true);

        var result = user.GetContact(VerificationChannel.Email);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Email.Value, result.Value);
    }

    [Fact]
    public void GetContact_Should_ReturnPhone_WhenSmsChannelIsRequestedAndPhoneConfirmed()
    {
        var user = CreateUser(active: true);
        var phone = "+380741739347";
        user.ChangePhone(phone);

        var result = user.GetContact(VerificationChannel.Sms);

        Assert.True(result.IsSuccess);
        Assert.Equal(phone, result.Value);
    }

    [Fact]
    public void GetContact_Should_ReturnFailure_WhenUserIsNotActive()
    {
        var user = CreateUser();

        var result = user.GetContact(VerificationChannel.Email);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.UserIsNotActive, result.Error);
    }

    [Fact]
    public void GetContact_Should_ReturnFailure_WhenPhoneIsNotProvided()
    {
        var user = CreateUser(active: true);

        var result = user.GetContact(VerificationChannel.Sms);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.UnsupportedVerificationChannel, result.Error);
    }
    
    [Fact]
    public void ChangePassword_Should_ChangePassword_WhenUserIsActive()
    {
        var user = CreateUser(active: true);
        var passwordHash = "new-password-hash";
        
        var result = user.ChangePassword(passwordHash);

        Assert.True(result.IsSuccess);
        Assert.Equal(passwordHash, user.PasswordHash);
    }
    
    [Fact]
    public void ChangePassword_Should_ReturnFailure_WhenUserIsNotActive()
    {
        var user = CreateUser();
        var oldPasswordHash = user.PasswordHash;

        var result = user.ChangePassword("new-password-hash");

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.UserIsNotActive, result.Error);
        Assert.Equal(oldPasswordHash, user.PasswordHash);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangePassword_Should_ReturnFailure_WhenPasswordHashIsEmpty(string? passwordHash)
    {
        var user = CreateUser(active: true);
        var oldPasswordHash = user.PasswordHash;

        var result = user.ChangePassword(passwordHash!);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.PasswordIsEmpty, result.Error);
        Assert.Equal(oldPasswordHash, user.PasswordHash);
    }
}