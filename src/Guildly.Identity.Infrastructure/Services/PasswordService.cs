using Guildly.Identity.Application.Services;
using Microsoft.AspNetCore.Identity;
using IdentityUser = Guildly.Identity.Domain.Entities.IdentityUser;

namespace Guildly.Identity.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private readonly IPasswordHasher<IdentityUser> _passwordHasher;

    public PasswordService(IPasswordHasher<IdentityUser> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, hashedPassword, password);

        return result is (PasswordVerificationResult.Success or  PasswordVerificationResult.SuccessRehashNeeded);
    }
}