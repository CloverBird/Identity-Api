using Guildly.Common.Results;
using Guildly.Identity.Domain.Errors;

namespace Guildly.Identity.Domain.ValueObjects;

public record Email
{
    private const int MaxLength = 256;
    
    public string Value { get; }
    
    public string NormalizedValue { get; }
    
    private Email(string value, string normalizedValue)
    {
        Value = value;
        NormalizedValue = normalizedValue;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return IdentityDomainErrors.EmailRequired;
        }
        
        email = email.Trim();
        
        if (email.Length > MaxLength)
        {
            return IdentityDomainErrors.EmailTooLong;
        }

        if (!email.Contains("@"))
        {
            return IdentityDomainErrors.EmailInvalidFormat;
        }
        
        return new Email(email, email.ToUpperInvariant());
    }
}