using Guildly.Identity.Domain.Errors;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Domain.UnitTests.ValueObjects;

public class PhoneTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Phone_Create_Should_ReturnFailure_WhenPhoneIsEmpty(string? phone)
    {
        var result = Phone.Create(phone!);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.PhoneRequired, result.Error);
    }
    
    [Theory]
    [InlineData("+099")]
    [InlineData("   +099   ")] // check that spaces don't count
    public void Phone_Create_Should_ReturnFailure_WhenPhoneIsTooShort(string? phone)
    {
        var result = Phone.Create(phone!);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.PhoneTooShort, result.Error);
    }
    
    [Fact]
    public void Phone_Create_Should_ReturnFailure_WhenPhoneIsTooLong()
    {
        var result = Phone.Create("+" + new string('3', 32));

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.PhoneTooLong, result.Error);
    }

    [Theory]
    [InlineData("+3807817483s8")]
    [InlineData("s0991382759")]
    [InlineData("+0991+82759")]
    [InlineData("+0991/82759")]
    [InlineData("+0991-82759")]
    [InlineData("+0991.82759")]
    public void Phone_Create_Should_ReturnFailure_WhenPhoneContainsNotOnlyDigitsBesidePlusAtBeginning(string phone)
    {
        var result = Phone.Create(phone);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.PhoneInvalidFormat, result.Error);
    }

    [Fact]
    public void Phone_Create_Should_ReturnSuccess_AndTrimPhone_WhenPhoneIsValid()
    {
        var result = Phone.Create(" +380781748381 ");
        
        Assert.True(result.IsSuccess);
        Assert.Equal("+380781748381", result.Value.Value);
    }
}