namespace Guildly.Identity.Application.Services;

public interface IVerificationCodeService
{
    string GenerateVerificationCode();
    
    string HashCode(string code);
    
    bool VerifyCode(string codeHas, string codeHash);
}