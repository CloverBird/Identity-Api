using Guildly.Identity.Domain.Errors;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Domain.UnitTests.ValueObjects;

public class EmailTests
{
    private const int MaxLength = 256;
    
    [Fact]
    public void Email_Create_Should_ReturnFailure_WhenEmailDoesNotContainAt()
    {
        var result = Email.Create("invalid-email");

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.EmailInvalidFormat, result.Error);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Email_Create_Should_ReturnFailure_WhenEmailIsEmpty(string? email)
    {
        var result = Email.Create(email!);
        
        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.EmailRequired, result.Error);
    }
    
    [Fact]
    public void Email_Create_Should_ReturnFailure_WhenEmailIsTooLong()
    {
        var emailPart = "@gmail.com";
        // 257 symbols in total
        var result = Email.Create(new string('a', MaxLength - emailPart.Length + 1) + emailPart);
        
        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.EmailTooLong, result.Error);
    }

    [Fact]
    public void Email_Create_Should_ReturnSuccess_WhenEmailIsValid()
    {
        var result = Email.Create("valid@gmail.com");
        Assert.True(result.IsSuccess);
    }
}