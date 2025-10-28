using Product.Core.Domain;
using Shared.Core.Domain.Errors;
using Shouldly;

namespace Product.UnitTest.Domain;

public class CategoryTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var name = "Electronics";
        var description = "Electronic devices and accessories";

        // Act
        var result = Category.Create(name, description);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe(name);
        result.Value.Description.ShouldBe(description);
        result.Value.IsActive.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        result.Value.Products.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldReturnValidationError(string invalidName)
    {
        // Act
        var result = Category.Create(invalidName, "Description");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("Name");
        error.Reason.ShouldContain("cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldStillSucceed()
    {
        // Arrange
        var name = "Electronics";
        var description = "";

        // Act
        var result = Category.Create(name, description);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Description.ShouldBe(description);
    }

    #endregion

    #region UpdateName Tests

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateSuccessfully()
    {
        // Arrange
        var category = CreateValidCategory();
        var newName = "Updated Electronics";

        // Act
        var result = category.UpdateName(newName);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        category.Name.ShouldBe(newName);
        category.UpdatedAt.ShouldNotBeNull();
        category.UpdatedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithInvalidName_ShouldReturnValidationError(string invalidName)
    {
        // Arrange
        var category = CreateValidCategory();
        var originalName = category.Name;

        // Act
        var result = category.UpdateName(invalidName);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        category.Name.ShouldBe(originalName); // Name should remain unchanged
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("Name");
    }

    #endregion

    #region UpdateDescription Tests

    [Fact]
    public void UpdateDescription_ShouldUpdateSuccessfully()
    {
        // Arrange
        var category = CreateValidCategory();
        var newDescription = "Updated description for electronics";

        // Act
        category.UpdateDescription(newDescription);

        // Assert
        category.Description.ShouldBe(newDescription);
        category.UpdatedAt.ShouldNotBeNull();
        category.UpdatedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void UpdateDescription_WithEmptyString_ShouldSucceed()
    {
        // Arrange
        var category = CreateValidCategory();

        // Act
        category.UpdateDescription("");

        // Assert
        category.Description.ShouldBe("");
        category.UpdatedAt.ShouldNotBeNull();
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var category = CreateValidCategory();
        category.IsActive.ShouldBeTrue();

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.ShouldBeFalse();
        category.UpdatedAt.ShouldNotBeNull();
        category.UpdatedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var category = CreateValidCategory();
        category.Deactivate();
        category.IsActive.ShouldBeFalse();

        // Act
        category.Activate();

        // Assert
        category.IsActive.ShouldBeTrue();
        category.UpdatedAt.ShouldNotBeNull();
        category.UpdatedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Deactivate_CalledMultipleTimes_ShouldRemainInactive()
    {
        // Arrange
        var category = CreateValidCategory();

        // Act
        category.Deactivate();
        var firstUpdate = category.UpdatedAt;
        Thread.Sleep(10); // Small delay to ensure different timestamps
        category.Deactivate();

        // Assert
        category.IsActive.ShouldBeFalse();
        category.UpdatedAt!.Value.ShouldBeGreaterThan(firstUpdate!.Value);
    }

    #endregion

    #region Integration Tests (Category with Products)

    [Fact]
    public void Category_ShouldHaveEmptyProductsCollectionWhenCreated()
    {
        // Arrange & Act
        var result = Category.Create("Electronics", "Electronic devices");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Products.ShouldNotBeNull();
        result.Value.Products.ShouldBeEmpty();
    }

    #endregion

    #region Pattern Matching Tests

    [Fact]
    public void Create_Result_CanBeMatchedSuccessfully()
    {
        // Arrange
        var name = "Electronics";
        var description = "Electronic devices";
        string matchedResult = string.Empty;

        // Act
        var result = Category.Create(name, description);
        result.Match(
            category => matchedResult = $"Success: {category.Name}",
            error => matchedResult = $"Error: {error.Message}"
        );

        // Assert
        matchedResult.ShouldBe($"Success: {name}");
    }

    [Fact]
    public void Create_WithInvalidData_CanBeMatchedToError()
    {
        // Arrange
        string matchedResult = string.Empty;

        // Act
        var result = Category.Create("", "Description");
        result.Match(
            category => matchedResult = $"Success: {category.Name}",
            error => matchedResult = $"Error: {error.Code}"
        );

        // Assert
        matchedResult.ShouldStartWith("Error:");
        matchedResult.ShouldContain("VALIDATION_ERROR");
    }

    [Fact]
    public void UpdateName_Result_CanBeMatchedSuccessfully()
    {
        // Arrange
        var category = CreateValidCategory();
        bool wasSuccessful = false;

        // Act
        var result = category.UpdateName("New Name");
        result.Match(
            success => wasSuccessful = true,
            error => wasSuccessful = false
        );

        // Assert
        wasSuccessful.ShouldBeTrue();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_WithVeryLongName_ShouldSucceed()
    {
        // Arrange
        var longName = new string('A', 1000);

        // Act
        var result = Category.Create(longName, "Description");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(longName);
    }

    [Fact]
    public void Create_WithSpecialCharactersInName_ShouldSucceed()
    {
        // Arrange
        var name = "Electronics & Accessories (2024) - Summer Sale!";

        // Act
        var result = Category.Create(name, "Description");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(name);
    }

    [Fact]
    public void UpdateName_MultipleTimes_ShouldKeepLatestValue()
    {
        // Arrange
        var category = CreateValidCategory();

        // Act
        category.UpdateName("Name 1");
        category.UpdateName("Name 2");
        var result = category.UpdateName("Name 3");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        category.Name.ShouldBe("Name 3");
    }

    #endregion

    #region Helper Methods

    private Category CreateValidCategory(
        string name = "Test Category",
        string description = "Test Description")
    {
        var result = Category.Create(name, description);
        return result.Value;
    }

    #endregion
}