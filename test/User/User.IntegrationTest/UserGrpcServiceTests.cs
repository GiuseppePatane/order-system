using Microsoft.EntityFrameworkCore;
using User.Infrastructure.EF;
using User.IntegrationTest.Fixtures;
using User.IntegrationTest.Infrastructure;
using User.Protos;
using Shouldly;
using Xunit.Abstractions;

namespace User.IntegrationTest;

/// <summary>
/// Integration tests for UserGrpcService
/// </summary>
[Collection("UserGrpc")]
public class UserGrpcServiceTests : IAsyncLifetime
{
    private readonly UserIntegrationTestFactory _factory;
    private UserService.UserServiceClient _grpcClient = null!;

    public UserGrpcServiceTests(ITestOutputHelper testOutputHelper)
    {
        _factory = new UserIntegrationTestFactory(testOutputHelper);
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _grpcClient = new UserService.UserServiceClient(_factory.Channel);
    }

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetUser_WithValidId_ShouldReturnUser()
    {
        // Arrange
        Guid userId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var users = TestDataGenerator.CreateUsers(1);
            context.Users.AddRange(users);
            context.SaveChanges();
            userId = users[0].Id;
        });

        // Act
        var request = new GetUserRequest { UserId = userId.ToString() };
        var response = await _grpcClient.GetUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(UserResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.UserId.ShouldBe(userId.ToString());
        response.Data.FirstName.ShouldNotBeEmpty();
        response.Data.LastName.ShouldNotBeEmpty();
        response.Data.Email.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ShouldReturnError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var request = new GetUserRequest { UserId = nonExistentId.ToString() };
        var response = await _grpcClient.GetUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(UserResponse.ResultOneofCase.Error);
        response.Error.ShouldNotBeNull();
        response.Error.Code.ShouldContain("NOT_FOUND");
    }

    [Fact]
    public async Task GetUsers_ShouldReturnPaginatedList()
    {
        // Arrange
        await _factory.SeedDatabaseAsync(context =>
        {
            var users = TestDataGenerator.CreateUsers(15);
            context.Users.AddRange(users);
            context.SaveChanges();
        });

        // Act
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 5
        };
        var response = await _grpcClient.GetUsersAsync(request);

        // Assert
        response.ResultCase.ShouldBe(GetUsersResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.Items.Count.ShouldBe(5);
        response.Data.PageNumber.ShouldBe(1);
        response.Data.PageSize.ShouldBe(5);
        response.Data.TotalCount.ShouldBe(15);
    }

    [Fact]
    public async Task GetUsers_WithSearchTerm_ShouldReturnFilteredUsers()
    {
        // Arrange
        string targetFirstName = "John";
        await _factory.SeedDatabaseAsync(context =>
        {
            // Create a specific user with known name
            var specificUser = Core.Domain.UserEntity.Create(targetFirstName, "Doe", "john.doe@test.com");
            if (specificUser.IsSuccess)
                context.Users.Add(specificUser.Value);

            // Add other random users
            var otherUsers = TestDataGenerator.CreateUsers(10);
            context.Users.AddRange(otherUsers);
            context.SaveChanges();
        });

        // Act
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 20,
            SearchTerm = targetFirstName
        };
        var response = await _grpcClient.GetUsersAsync(request);

        // Assert
        response.ResultCase.ShouldBe(GetUsersResponse.ResultOneofCase.Data);
        response.Data.Items.Count.ShouldBeGreaterThan(0);
        response.Data.Items.Any(u => u.FirstName.Contains(targetFirstName, StringComparison.OrdinalIgnoreCase))
            .ShouldBeTrue();
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldCreateUser()
    {
        // Arrange & Act
        var request = new CreateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test.user@example.com"
        };
        var response = await _grpcClient.CreateUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(UserResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.UserId.ShouldNotBeNullOrEmpty();

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var savedUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == "test.user@example.com");

        savedUser.ShouldNotBeNull();
        savedUser.FirstName.ShouldBe("Test");
        savedUser.LastName.ShouldBe("User");
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ShouldReturnError()
    {
        // Arrange
        var existingEmail = "duplicate@test.com";
        await _factory.SeedDatabaseAsync(context =>
        {
            var user = Core.Domain.UserEntity.Create("Existing", "User", existingEmail);
            if (user.IsSuccess)
            {
                context.Users.Add(user.Value);
                context.SaveChanges();
            }
        });

        // Act
        var request = new CreateUserRequest
        {
            FirstName = "New",
            LastName = "User",
            Email = existingEmail
        };
        var response = await _grpcClient.CreateUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(UserResponse.ResultOneofCase.Error);
        response.Error.ShouldNotBeNull();
        response.Error.Message.ShouldContain("Email");
    }

    [Fact]
    public async Task CreateUser_WithInvalidEmail_ShouldReturnError()
    {
        // Arrange & Act
        var request = new CreateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "invalid-email"
        };
        var response = await _grpcClient.CreateUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(UserResponse.ResultOneofCase.Error);
        response.Error.ShouldNotBeNull();
        response.Error.Message.ShouldContain("email");
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        Guid userId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var users = TestDataGenerator.CreateUsers(1);
            context.Users.AddRange(users);
            context.SaveChanges();
            userId = users[0].Id;
        });

        // Act
        var request = new UpdateUserRequest
        {
            UserId = userId.ToString(),
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@test.com"
        };
        var response = await _grpcClient.UpdateUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(UserResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var updatedUser = await context.Users.FindAsync(userId);

        updatedUser.ShouldNotBeNull();
        updatedUser.FirstName.ShouldBe("Updated");
        updatedUser.LastName.ShouldBe("Name");
        updatedUser.Email.ShouldBe("updated@test.com");
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var request = new UpdateUserRequest
        {
            UserId = nonExistentId.ToString(),
            FirstName = "Updated"
        };
        var response = await _grpcClient.UpdateUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(UserResponse.ResultOneofCase.Error);
        response.Error.ShouldNotBeNull();
        response.Error.Code.ShouldContain("NOT_FOUND");
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ShouldDeleteUser()
    {
        // Arrange
        Guid userId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var users = TestDataGenerator.CreateUsers(1);
            context.Users.AddRange(users);
            context.SaveChanges();
            userId = users[0].Id;
        });

        // Act
        var request = new DeleteUserRequest { UserId = userId.ToString() };
        var response = await _grpcClient.DeleteUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(DeleteUserResponse.ResultOneofCase.Data);
        response.Data.Success.ShouldBeTrue();
        response.Data.UserId.ShouldBe(userId.ToString());

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var deletedUser = await context.Users.FindAsync(userId);

        deletedUser.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteUser_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var request = new DeleteUserRequest { UserId = nonExistentId.ToString() };
        var response = await _grpcClient.DeleteUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(DeleteUserResponse.ResultOneofCase.Error);
        response.Error.ShouldNotBeNull();
        response.Error.Code.ShouldContain("NOT_FOUND");
    }
}