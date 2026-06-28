using System.Net;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using FluentAssertions;

namespace Eltorto.IntegrationTests;

public class ContactSettingsTests : IntegrationTestBase
{
    // ========== GET ==========

    [Fact]
    public async Task GetContactSettings_ShouldReturnSettings()
    {
        var response = await Client.GetAsync("/api/contacts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var settings = await response.Content.ReadFromJsonAsync<ContactSettingsDto>();
        settings.Should().NotBeNull();
        settings!.Phone.Should().Be("+7 (123) 456-78-90");
        settings.AdditionalPhone.Should().Be("+7 (098) 765-43-21");
        settings.Email.Should().Be("info@eltorto.ru");
        settings.Address.Should().Be("г. Москва, ул. Примерная, д. 1");
        settings.MapUrl.Should().Be("https://maps.example.com/eltorto");
    }

    // ========== PUT ==========

    [Fact]
    public async Task UpdateContactSettings_AsAdmin_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateContactSettingsDto
        {
            Phone = "+7 (999) 111-22-33",
            AdditionalPhone = "+7 (888) 444-55-66",
            Email = "new@eltorto.ru",
            Address = "г. Санкт-Петербург, Невский пр., д. 1",
            MapUrl = "https://maps.example.com/new"
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/contacts", updateDto, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ContactSettingsDto>();
        updated.Should().NotBeNull();
        updated!.Phone.Should().Be(updateDto.Phone);
        updated.AdditionalPhone.Should().Be(updateDto.AdditionalPhone);
        updated.Email.Should().Be(updateDto.Email);
        updated.Address.Should().Be(updateDto.Address);
        updated.MapUrl.Should().Be(updateDto.MapUrl);
    }

    [Fact]
    public async Task UpdateContactSettings_WithoutToken_ReturnsUnauthorized()
    {
        var updateDto = new UpdateContactSettingsDto
        {
            Phone = "+7 (111) 222-33-44",
            Email = "test@test.ru"
        };

        var response = await Client.PutAsJsonAsync("/api/contacts", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateContactSettings_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var updateDto = new UpdateContactSettingsDto
        {
            Phone = "+7 (111) 222-33-44",
            Email = "test@test.ru"
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/contacts", updateDto, customerToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}