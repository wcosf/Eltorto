using System.Net;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using FluentAssertions;

namespace Eltorto.IntegrationTests;

[Collection("IntegrationTests")]
public class AuthTests : IntegrationTestBase
{
    private async Task<(string AccessToken, string RefreshToken)> RegisterAndLoginAsync(
        string userName, string password = "Test123!")
    {
        var registerDto = new RegisterRequest
        {
            UserName = userName,
            Email = $"{userName}@test.ru",
            Password = password,
            FullName = "Test User"
        };
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginDto = new LoginRequest { UserName = userName, Password = password };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return (tokens!.AccessToken, tokens.RefreshToken);
    }

    // ========== POST ==========

    [Fact]
    public async Task Register_ValidUser_ReturnsCreated()
    {
        var userName = $"user_{Guid.NewGuid():N}";
        var registerDto = new RegisterRequest
        {
            UserName = userName,
            Email = $"{userName}@test.ru",
            Password = "Valid123!",
            FullName = "New User"
        };
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<MessageResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Contain("registered successfully");
    }

    [Fact]
    public async Task Register_DuplicateUserName_ReturnsBadRequest()
    {
        var userName = $"duplicate_{Guid.NewGuid():N}";
        var registerDto = new RegisterRequest
        {
            UserName = userName,
            Email = $"{userName}@test.ru",
            Password = "Valid123!",
            FullName = "First"
        };
        await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        var duplicateDto = new RegisterRequest
        {
            UserName = userName,
            Email = $"{userName}_2@test.ru",
            Password = "Valid123!",
            FullName = "Second"
        };
        var response = await Client.PostAsJsonAsync("/api/auth/register", duplicateDto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        var userName = $"weak_{Guid.NewGuid():N}";
        var registerDto = new RegisterRequest
        {
            UserName = userName,
            Email = $"{userName}@test.ru",
            Password = "123",
            FullName = "Weak"
        };
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        var userName = $"invalidemail_{Guid.NewGuid():N}";
        var registerDto = new RegisterRequest
        {
            UserName = userName,
            Email = "invalid-email",
            Password = "Valid123!",
            FullName = "Invalid"
        };
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ========== POST ==========

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        var userName = $"login_{Guid.NewGuid():N}";
        var password = "Login123!";
        await RegisterAndLoginAsync(userName, password);

        var loginDto = new LoginRequest { UserName = userName, Password = password };
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await response.Content.ReadFromJsonAsync<LoginResponse>();
        tokens.Should().NotBeNull();
        tokens!.AccessToken.Should().NotBeNullOrEmpty();
        tokens.RefreshToken.Should().NotBeNullOrEmpty();
        tokens.UserName.Should().Be(userName);
        tokens.Roles.Should().Contain("Customer");
        tokens.Expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var userName = $"invalidpwd_{Guid.NewGuid():N}";
        await RegisterAndLoginAsync(userName, "Valid123!");

        var loginDto = new LoginRequest { UserName = userName, Password = "WrongPassword!" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_NonExistingUser_ReturnsUnauthorized()
    {
        var loginDto = new LoginRequest { UserName = "nonexistent", Password = "anything" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("Invalid credentials");
    }

    // ========== POST ==========

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewAccessToken()
    {
        var (accessToken, refreshToken) = await RegisterAndLoginAsync($"refresh_{Guid.NewGuid():N}");

        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await response.Content.ReadFromJsonAsync<LoginResponse>();
        newTokens.Should().NotBeNull();
        newTokens!.AccessToken.Should().NotBeNullOrEmpty();
        newTokens.RefreshToken.Should().NotBeNullOrEmpty();
        newTokens.RefreshToken.Should().NotBe(refreshToken);
        newTokens.UserName.Should().Be(newTokens.UserName);
        newTokens.Expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Refresh_WithUsedToken_ReturnsUnauthorized()
    {
        var (accessToken, refreshToken) = await RegisterAndLoginAsync($"refresh_used_{Guid.NewGuid():N}");

        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
        var firstResponse = await Client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await Client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var error = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("Invalid or expired refresh token");
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ReturnsUnauthorized()
    {
        var (accessToken, refreshToken) = await RegisterAndLoginAsync($"refresh_revoked_{Guid.NewGuid():N}");

        var logoutRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
        var logoutResponse = await AuthorizedRequestAsync(HttpMethod.Post, "/api/auth/logout", logoutRequest, accessToken);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        var refreshRequest = new RefreshTokenRequest { RefreshToken = "invalid_token" };
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("Invalid or expired refresh token");
    }

    // ========== POST ==========

    [Fact]
    public async Task Logout_ValidToken_ReturnsNoContent()
    {
        var (accessToken, refreshToken) = await RegisterAndLoginAsync($"logout_{Guid.NewGuid():N}");

        var logoutRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/auth/logout", logoutRequest, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithEmptyRefreshToken_ReturnsBadRequest()
    {
        var (accessToken, _) = await RegisterAndLoginAsync($"logout_empty_{Guid.NewGuid():N}");

        var logoutRequest = new RefreshTokenRequest();
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/auth/logout", logoutRequest, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ========== POST ==========

    [Fact]
    public async Task ChangePassword_Valid_ReturnsOk()
    {
        var userName = $"changepwd_{Guid.NewGuid():N}";
        var oldPassword = "Old123!";
        var newPassword = "New456!";
        var (accessToken, _) = await RegisterAndLoginAsync(userName, oldPassword);

        var changeRequest = new ChangePasswordRequest
        {
            CurrentPassword = oldPassword,
            NewPassword = newPassword
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/auth/change-password", changeRequest, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MessageResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Contain("Password changed successfully");
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ReturnsBadRequest()
    {
        var userName = $"wrongold_{Guid.NewGuid():N}";
        var oldPassword = "Old123!";
        var (accessToken, _) = await RegisterAndLoginAsync(userName, oldPassword);

        var changeRequest = new ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword!",
            NewPassword = "New456!"
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/auth/change-password", changeRequest, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("Failed to change password");
    }

    [Fact]
    public async Task ChangePassword_WithoutToken_ReturnsUnauthorized()
    {
        var changeRequest = new ChangePasswordRequest
        {
            CurrentPassword = "Old123!",
            NewPassword = "New456!"
        };
        var response = await Client.PostAsJsonAsync("/api/auth/change-password", changeRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithCustomerToken_ReturnsOk()
    {
        var userName = $"customerpwd_{Guid.NewGuid():N}";
        var oldPassword = "Old123!";
        var (accessToken, _) = await RegisterAndLoginAsync(userName, oldPassword);

        var changeRequest = new ChangePasswordRequest
        {
            CurrentPassword = oldPassword,
            NewPassword = "New456!"
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/auth/change-password", changeRequest, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}