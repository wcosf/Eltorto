using System.Net;
using System.Net.Http.Headers;
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
        var refreshToken = ExtractRefreshTokenFromResponse(loginResponse);
        return (tokens!.AccessToken, refreshToken);
    }

    private static string ExtractRefreshTokenFromResponse(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var refreshCookie = cookies.FirstOrDefault(c => c.StartsWith("refresh_token="));
            if (refreshCookie != null)
                return refreshCookie.Split(';').First().Split('=', 2).Last();
        }
        return string.Empty;
    }

    private async Task<HttpResponseMessage> SendWithRefreshTokenAsync(
        HttpMethod method, string url, string refreshToken,
        string? accessToken = null, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (!string.IsNullOrEmpty(refreshToken))
            request.Headers.Add("Cookie", $"refresh_token={refreshToken}");
        if (body != null)
            request.Content = JsonContent.Create(body);
        return await Client.SendAsync(request);
    }

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
        tokens.UserName.Should().Be(userName);
        tokens.Roles.Should().Contain("Customer");
        tokens.Expiration.Should().BeAfter(DateTime.UtcNow);

        var refreshCookie = ExtractRefreshTokenFromResponse(response);
        refreshCookie.Should().NotBeNullOrEmpty();
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

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewAccessToken()
    {
        var (accessToken, refreshToken) = await RegisterAndLoginAsync($"refresh_{Guid.NewGuid():N}");

        var response = await SendWithRefreshTokenAsync(HttpMethod.Post, "/api/auth/refresh", refreshToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await response.Content.ReadFromJsonAsync<LoginResponse>();
        newTokens.Should().NotBeNull();
        newTokens!.AccessToken.Should().NotBeNullOrEmpty();
        newTokens.UserName.Should().NotBeNullOrEmpty();
        newTokens.Expiration.Should().BeAfter(DateTime.UtcNow);

        var newRefreshToken = ExtractRefreshTokenFromResponse(response);
        newRefreshToken.Should().NotBeNullOrEmpty();
        newRefreshToken.Should().NotBe(refreshToken);
    }

    [Fact]
    public async Task Refresh_WithUsedToken_ReturnsUnauthorized()
    {
        var (accessToken, refreshToken) = await RegisterAndLoginAsync($"refresh_used_{Guid.NewGuid():N}");

        var firstResponse = await SendWithRefreshTokenAsync(HttpMethod.Post, "/api/auth/refresh", refreshToken);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await SendWithRefreshTokenAsync(HttpMethod.Post, "/api/auth/refresh", refreshToken);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var error = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("Invalid or expired refresh token");
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ReturnsUnauthorized()
    {
        var (accessToken, refreshToken) = await RegisterAndLoginAsync($"refresh_revoked_{Guid.NewGuid():N}");

        var logoutResponse = await SendWithRefreshTokenAsync(
            HttpMethod.Post, "/api/auth/logout", refreshToken, accessToken);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshResponse = await SendWithRefreshTokenAsync(
            HttpMethod.Post, "/api/auth/refresh", refreshToken);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        var response = await SendWithRefreshTokenAsync(
            HttpMethod.Post, "/api/auth/refresh", "invalid_token_value");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("Invalid or expired refresh token");
    }

    [Fact]
    public async Task Logout_ValidToken_ReturnsNoContent()
    {
        var (accessToken, refreshToken) = await RegisterAndLoginAsync($"logout_{Guid.NewGuid():N}");

        var response = await SendWithRefreshTokenAsync(
            HttpMethod.Post, "/api/auth/logout", refreshToken, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshResponse = await SendWithRefreshTokenAsync(
            HttpMethod.Post, "/api/auth/refresh", refreshToken);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_ReturnsNoContent()
    {
        var (accessToken, _) = await RegisterAndLoginAsync($"logout_empty_{Guid.NewGuid():N}");

        var response = await SendWithRefreshTokenAsync(
            HttpMethod.Post, "/api/auth/logout", string.Empty, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsUserInfo()
    {
        var (accessToken, _) = await RegisterAndLoginAsync($"me_{Guid.NewGuid():N}");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userInfo = await response.Content.ReadFromJsonAsync<MeResponse>();
        userInfo.Should().NotBeNull();
        userInfo!.UserName.Should().NotBeNullOrEmpty();
        userInfo.Roles.Should().Contain("Customer");
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public record MeResponse(string UserName, string[] Roles);
}
