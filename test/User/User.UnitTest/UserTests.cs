using User.Core.Domain;
using Xunit;


namespace User.UnitTest;

public class UserTests
{
    [Fact]
    public void Create_ValidUser_ReturnsSuccess()
    {
        var result =  UserEntity.Create("John", "Doe", "john.doe@example.com");
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("John", result.Value.FirstName);
        Assert.Equal("Doe", result.Value.LastName);
        Assert.Equal("john.doe@example.com", result.Value.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Create_InvalidEmail_ReturnsFailure(string? email)
    {
        var result = UserEntity.Create("John", "Doe", email ?? string.Empty);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Update_ValidData_ReturnsSuccessAndUpdatesFields()
    {
        var created = UserEntity.Create("Alice", "Smith", "alice@example.com");
        Assert.True(created.IsSuccess);
        var user = created.Value;

        var update = user.Update("Alicia", "Johnson", "alicia.johnson@example.com");
        Assert.True(update.IsSuccess);
        Assert.Equal("Alicia", user.FirstName);
        Assert.Equal("Johnson", user.LastName);
        Assert.Equal("alicia.johnson@example.com", user.Email);
    }

    [Fact]
    public void Update_InvalidFirstName_ReturnsFailure()
    {
        var created = UserEntity.Create("Alice", "Smith", "alice@example.com");
        Assert.True(created.IsSuccess);
        var user = created.Value;

        var update = user.Update(null!, "Smith", "alice@example.com");
        Assert.True(update.IsFailure);
    }
}

