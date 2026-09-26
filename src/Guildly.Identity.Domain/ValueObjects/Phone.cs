using Guildly.Common.Results;
using Guildly.Identity.Domain.Errors;

namespace Guildly.Identity.Domain.ValueObjects;

public record Phone
{
    private const int MinLength = 7;
    private const int MaxLength = 32;
    
    public string Value { get; }

    private Phone(string value)
    {
        Value = value;
    }
    
    public static Result<Phone> Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return IdentityDomainErrors.PhoneRequired;
        
        phone = phone.Trim();

        if (phone.Length < MinLength)
            return IdentityDomainErrors.PhoneTooShort;

        if (phone.Length > MaxLength)
            return IdentityDomainErrors.PhoneTooLong;

        var charsToCheck = phone.StartsWith('+')
            ? phone[1..]
            : phone;

        if (charsToCheck.Any(x => !char.IsDigit(x)))
            return IdentityDomainErrors.PhoneInvalidFormat;
        
        return new Phone(phone);
    }
}