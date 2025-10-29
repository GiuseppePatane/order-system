using Address.Core.Domain;
using Shared.Core.Domain.Errors;
using Shouldly;

namespace Address.UnitTest.Domain;

public class AddressEntityTests
{
    private readonly Guid _userId = Guid.NewGuid();

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var street = "123 Main St";
        var city = "New York";
        var state = "NY";
        var postalCode = "10001";
        var country = "USA";

        // Act
        var result = AddressEntity.Create(_userId, street, city, state, postalCode, country);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.UserId.ShouldBe(_userId);
        result.Value.Street.ShouldBe(street);
        result.Value.City.ShouldBe(city);
        result.Value.State.ShouldBe(state);
        result.Value.PostalCode.ShouldBe(postalCode);
        result.Value.Country.ShouldBe(country);
        result.Value.IsDefault.ShouldBeFalse();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.CreatedAt.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithOptionalFields_ShouldReturnSuccess()
    {
        // Arrange
        var street2 = "Apt 4B";
        var label = "Home";

        // Act
        var result = AddressEntity.Create(
            _userId,
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "USA",
            street2,
            label,
            true);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Street2.ShouldBe(street2);
        result.Value.Label.ShouldBe(label);
        result.Value.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnValidationError()
    {
        // Act
        var result = AddressEntity.Create(Guid.Empty, "123 Main St", "New York", "NY", "10001", "USA");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("userId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidStreet_ShouldReturnValidationError(string invalidStreet)
    {
        // Act
        var result = AddressEntity.Create(_userId, invalidStreet, "New York", "NY", "10001", "USA");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("street");
    }

    [Fact]
    public void Create_WithTooLongStreet_ShouldReturnValidationError()
    {
        // Arrange
        var longStreet = new string('a', 201);

        // Act
        var result = AddressEntity.Create(_userId, longStreet, "New York", "NY", "10001", "USA");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("street");
        error.Reason.ShouldContain("200 characters");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCity_ShouldReturnValidationError(string invalidCity)
    {
        // Act
        var result = AddressEntity.Create(_userId, "123 Main St", invalidCity, "NY", "10001", "USA");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("city");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidState_ShouldReturnValidationError(string invalidState)
    {
        // Act
        var result = AddressEntity.Create(_userId, "123 Main St", "New York", invalidState, "10001", "USA");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("state");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPostalCode_ShouldReturnValidationError(string invalidPostalCode)
    {
        // Act
        var result = AddressEntity.Create(_userId, "123 Main St", "New York", "NY", invalidPostalCode, "USA");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("postalCode");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCountry_ShouldReturnValidationError(string invalidCountry)
    {
        // Act
        var result = AddressEntity.Create(_userId, "123 Main St", "New York", "NY", "10001", invalidCountry);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("country");
    }

    [Fact]
    public void Create_WithTooLongLabel_ShouldReturnValidationError()
    {
        // Arrange
        var longLabel = new string('a', 51);

        // Act
        var result = AddressEntity.Create(_userId, "123 Main St", "New York", "NY", "10001", "USA", null, longLabel);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("label");
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidStreet_ShouldUpdateSuccessfully()
    {
        // Arrange
        var address = CreateValidAddress();
        var newStreet = "456 Oak Ave";

        // Act
        var result = address.Update(street: newStreet);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        address.Street.ShouldBe(newStreet);
        address.UpdatedAt.ShouldNotBeNull();
        address.UpdatedAt.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_WithValidCity_ShouldUpdateSuccessfully()
    {
        // Arrange
        var address = CreateValidAddress();
        var newCity = "Los Angeles";

        // Act
        var result = address.Update(city: newCity);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        address.City.ShouldBe(newCity);
        address.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Update_WithTooLongStreet_ShouldReturnValidationError()
    {
        // Arrange
        var address = CreateValidAddress();
        var longStreet = new string('a', 201);

        // Act
        var result = address.Update(street: longStreet);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
    }

    [Fact]
    public void Update_WithMultipleFields_ShouldUpdateAllFields()
    {
        // Arrange
        var address = CreateValidAddress();

        // Act
        var result = address.Update(
            street: "789 Pine St",
            city: "Boston",
            state: "MA",
            postalCode: "02101",
            label: "Office");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        address.Street.ShouldBe("789 Pine St");
        address.City.ShouldBe("Boston");
        address.State.ShouldBe("MA");
        address.PostalCode.ShouldBe("02101");
        address.Label.ShouldBe("Office");
    }

    #endregion

    #region SetAsDefault Tests

    [Fact]
    public void SetAsDefault_ShouldSetIsDefaultToTrue()
    {
        // Arrange
        var address = CreateValidAddress();
        address.IsDefault.ShouldBeFalse();

        // Act
        address.SetAsDefault();

        // Assert
        address.IsDefault.ShouldBeTrue();
        address.UpdatedAt.ShouldNotBeNull();
        address.UpdatedAt.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UnsetAsDefault_ShouldSetIsDefaultToFalse()
    {
        // Arrange
        var address = CreateValidAddress(isDefault: true);
        address.IsDefault.ShouldBeTrue();

        // Act
        address.UnsetAsDefault();

        // Assert
        address.IsDefault.ShouldBeFalse();
        address.UpdatedAt.ShouldNotBeNull();
    }

    #endregion

    #region Helper Methods

    private AddressEntity CreateValidAddress(
        string street = "123 Main St",
        string city = "New York",
        string state = "NY",
        string postalCode = "10001",
        string country = "USA",
        bool isDefault = false)
    {
        var result = AddressEntity.Create(_userId, street, city, state, postalCode, country, isDefault: isDefault);
        return result.Value;
    }

    #endregion
}
