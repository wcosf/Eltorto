using System.Net;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using FluentAssertions;

namespace Eltorto.IntegrationTests;

public class FillingsTests : IntegrationTestBase
{
    private async Task<FillingDto> CreateFreshFillingAsync(string name = "Тестовая начинка")
    {
        var token = await GetAdminTokenAsync();
        var createDto = new CreateFillingDto
        {
            Name = name,
            Description = "Описание тестовой начинки",
            ImageUrl = "https://example.com/filling.jpg",
            HasCrossSection = true
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/fillings", createDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<FillingDto>())!;
    }

    // ========== GET ==========

    [Fact]
    public async Task GetAllFillings_ShouldReturnList()
    {
        var response = await Client.GetAsync("/api/fillings");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fillings = await response.Content.ReadFromJsonAsync<List<FillingDto>>();
        fillings.Should().NotBeNull();
        fillings!.Count.Should().BeGreaterThan(0);
        fillings.Should().Contain(f => f.Name == "Сливочный");
    }

    [Fact]
    public async Task GetAvailableFillings_ShouldReturnList()
    {
        var response = await Client.GetAsync("/api/fillings/available");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fillings = await response.Content.ReadFromJsonAsync<List<FillingDto>>();
        fillings.Should().NotBeNull();
        fillings!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetFillingById_Existing_ReturnsOk()
    {
        var allResponse = await Client.GetAsync("/api/fillings");
        var all = await allResponse.Content.ReadFromJsonAsync<List<FillingDto>>();
        var existing = all!.First();
        var response = await Client.GetAsync($"/api/fillings/{existing.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var filling = await response.Content.ReadFromJsonAsync<FillingDto>();
        filling.Should().NotBeNull();
        filling!.Id.Should().Be(existing.Id);
        filling.Name.Should().Be(existing.Name);
    }

    [Fact]
    public async Task GetFillingById_NotExisting_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/fillings/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFillingWithCakes_Existing_ReturnsOkWithCakes()
    {
        var allResponse = await Client.GetAsync("/api/fillings");
        var all = await allResponse.Content.ReadFromJsonAsync<List<FillingDto>>();
        var filling = all!.FirstOrDefault(f => f.Name == "Сливочный");
        filling.Should().NotBeNull();

        var response = await Client.GetAsync($"/api/fillings/{filling!.Id}/with-cakes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fillingWithCakes = await response.Content.ReadFromJsonAsync<FillingWithCakesDto>();
        fillingWithCakes.Should().NotBeNull();
        fillingWithCakes!.Id.Should().Be(filling.Id);
        fillingWithCakes.Cakes.Should().NotBeNull();
        fillingWithCakes.Cakes.Should().HaveCountGreaterThan(0);
        fillingWithCakes.Cakes.Should().Contain(c => c.Name == "Наполеон");
    }

    [Fact]
    public async Task GetFillingWithCakes_NotExisting_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/fillings/999999/with-cakes");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ========== POST ==========

    [Fact]
    public async Task CreateFilling_AsAdmin_ReturnsCreated()
    {
        var token = await GetAdminTokenAsync();
        var newFilling = new CreateFillingDto
        {
            Name = "Новая начинка",
            Description = "Описание новой начинки",
            ImageUrl = "https://example.com/new_filling.jpg",
            HasCrossSection = true
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/fillings", newFilling, token);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<FillingDto>();
        created.Should().NotBeNull();
        created!.Name.Should().Be("Новая начинка");
        created.Description.Should().Be("Описание новой начинки");
        created.ImageUrl.Should().Be("https://example.com/new_filling.jpg");
        created.HasCrossSection.Should().BeTrue();

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.PathAndQuery.Should().ContainEquivalentOf($"api/fillings/{created.Id}");
    }

    [Fact]
    public async Task CreateFilling_WithoutToken_ReturnsUnauthorized()
    {
        var newFilling = new CreateFillingDto
        {
            Name = "Начинка без токена",
            Description = "Описание",
            ImageUrl = "https://example.com/filling.jpg"
        };
        var response = await Client.PostAsJsonAsync("/api/fillings", newFilling);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateFilling_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var newFilling = new CreateFillingDto
        {
            Name = "Начинка от покупателя",
            Description = "Описание",
            ImageUrl = "https://example.com/filling.jpg"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/fillings", newFilling, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== PUT ==========

    [Fact]
    public async Task UpdateFilling_AsAdmin_ReturnsOk()
    {
        var created = await CreateFreshFillingAsync("Для обновления");
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateFillingDto
        {
            Id = created.Id,
            Name = "Обновлённая начинка",
            Description = "Обновлённое описание",
            ImageUrl = "https://example.com/updated_filling.jpg",
            HasCrossSection = false
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/fillings/{created.Id}", updateDto, token);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 200, got {response.StatusCode}. Response: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<FillingDto>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Обновлённая начинка");
        updated.Description.Should().Be("Обновлённое описание");
        updated.ImageUrl.Should().Be("https://example.com/updated_filling.jpg");
        updated.HasCrossSection.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateFilling_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateFillingDto
        {
            Id = 999999,
            Name = "Несуществующая",
            Description = "Описание",
            ImageUrl = "https://example.com/filling.jpg"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/fillings/999999", updateDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateFilling_WithoutToken_ReturnsUnauthorized()
    {
        var allResponse = await Client.GetAsync("/api/fillings");
        var all = await allResponse.Content.ReadFromJsonAsync<List<FillingDto>>();
        var filling = all!.First();
        var updateDto = new UpdateFillingDto
        {
            Id = filling.Id,
            Name = "Обновлённая",
            Description = "Описание",
            ImageUrl = "https://example.com/filling.jpg"
        };
        var response = await Client.PutAsJsonAsync($"/api/fillings/{filling.Id}", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateFilling_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var allResponse = await Client.GetAsync("/api/fillings");
        var all = await allResponse.Content.ReadFromJsonAsync<List<FillingDto>>();
        var filling = all!.First();
        var updateDto = new UpdateFillingDto
        {
            Id = filling.Id,
            Name = "Обновлённая",
            Description = "Описание",
            ImageUrl = "https://example.com/filling.jpg"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/fillings/{filling.Id}", updateDto, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== DELETE ==========

    [Fact]
    public async Task DeleteFilling_WithoutCakes_ReturnsNoContent()
    {
        var created = await CreateFreshFillingAsync("Для удаления");
        var token = await GetAdminTokenAsync();

        var response = await AuthorizedRequestAsync(HttpMethod.Delete, $"/api/fillings/{created.Id}", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/fillings/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteFilling_WithCakes_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        var allResponse = await Client.GetAsync("/api/fillings");
        var all = await allResponse.Content.ReadFromJsonAsync<List<FillingDto>>();
        var filling = all!.FirstOrDefault(f => f.Name == "Сливочный");
        filling.Should().NotBeNull();

        var response = await AuthorizedRequestAsync(HttpMethod.Delete, $"/api/fillings/{filling!.Id}", token: token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("cakes");
    }

    [Fact]
    public async Task DeleteFilling_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/fillings/999999", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteFilling_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.DeleteAsync("/api/fillings/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteFilling_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/fillings/1", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}