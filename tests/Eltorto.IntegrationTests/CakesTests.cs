using System.Net;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using FluentAssertions;

namespace Eltorto.IntegrationTests;

public class CakesTests : IntegrationTestBase
{
    private async Task<CakeDetailDto> CreateFreshCakeAsync(
        string name = "Тестовый торт",
        string categorySlug = "classic",
        int? fillingId = null,
        bool isFeatured = false)
    {
        var token = await GetAdminTokenAsync();
        var createDto = new CreateCakeDto
        {
            Name = name,
            ImageUrl = "https://example.com/cake.jpg",
            ThumbnailUrl = "https://example.com/cake_thumb.jpg",
            CategorySlug = categorySlug,
            IsFeatured = isFeatured,
            Description = "Описание тестового торта",
            FillingId = fillingId
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/cakes", createDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CakeDetailDto>())!;
    }

    // ========== GET ==========

    [Fact]
    public async Task GetAllCakes_ShouldReturnList()
    {
        var response = await Client.GetAsync("/api/cakes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cakes = await response.Content.ReadFromJsonAsync<List<CakeListDto>>();
        cakes.Should().NotBeNull();
        cakes!.Count.Should().BeGreaterThan(0);
        cakes.Should().Contain(c => c.Name == "Наполеон");
    }

    [Fact]
    public async Task GetPagedCakes_WithDefaultParams_ReturnsPagedResult()
    {
        var response = await Client.GetAsync("/api/cakes/paged");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResultDto<CakeListDto>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().NotBeNull();
        paged.TotalCount.Should().BeGreaterThan(0);
        paged.Page.Should().Be(1);
        paged.PageSize.Should().Be(12);
        paged.TotalPages.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPagedCakes_WithCategoryFilter_ReturnsFiltered()
    {
        var response = await Client.GetAsync("/api/cakes/paged?category=classic");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResultDto<CakeListDto>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().AllSatisfy(c => c.CategorySlug.Should().Be("classic"));
    }

    [Fact]
    public async Task GetPagedCakes_WithInvalidPage_ReturnsEmpty()
    {
        var response = await Client.GetAsync("/api/cakes/paged?page=999&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResultDto<CakeListDto>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().BeEmpty();
        paged.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetFeaturedCakes_ReturnsFeatured()
    {
        await CreateFreshCakeAsync("Featured Cake", isFeatured: true);
        var response = await Client.GetAsync("/api/cakes/featured?count=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cakes = await response.Content.ReadFromJsonAsync<List<CakeListDto>>();
        cakes.Should().NotBeNull();
        cakes!.Should().AllSatisfy(c => c.IsFeatured.Should().BeTrue());
    }

    [Fact]
    public async Task GetCakesByCategory_Existing_ReturnsOk()
    {
        var response = await Client.GetAsync("/api/cakes/by-category/classic");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cakes = await response.Content.ReadFromJsonAsync<List<CakeListDto>>();
        cakes.Should().NotBeNull();
        cakes!.Should().AllSatisfy(c => c.CategorySlug.Should().Be("classic"));
    }

    [Fact]
    public async Task GetCakesByCategory_NotExisting_ReturnsEmptyList()
    {
        var response = await Client.GetAsync("/api/cakes/by-category/non-existing");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cakes = await response.Content.ReadFromJsonAsync<List<CakeListDto>>();
        cakes.Should().NotBeNull();
        cakes!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCakesByFilling_Existing_ReturnsOk()
    {
        var fillingsResponse = await Client.GetAsync("/api/fillings");
        var fillings = await fillingsResponse.Content.ReadFromJsonAsync<List<FillingDto>>();
        var filling = fillings!.FirstOrDefault(f => f.Name == "Сливочный");
        filling.Should().NotBeNull();

        var response = await Client.GetAsync($"/api/cakes/by-filling/{filling!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cakes = await response.Content.ReadFromJsonAsync<List<CakeListDto>>();
        cakes.Should().NotBeNull();
        cakes!.Should().AllSatisfy(c => c.FillingId.Should().Be(filling.Id));
    }

    [Fact]
    public async Task GetCakesByFilling_NotExisting_ReturnsEmptyList()
    {
        var response = await Client.GetAsync("/api/cakes/by-filling/99999");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cakes = await response.Content.ReadFromJsonAsync<List<CakeListDto>>();
        cakes.Should().NotBeNull();
        cakes!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCakeById_Existing_ReturnsOk()
    {
        var allResponse = await Client.GetAsync("/api/cakes");
        var all = await allResponse.Content.ReadFromJsonAsync<List<CakeListDto>>();
        var existing = all!.First();
        var response = await Client.GetAsync($"/api/cakes/{existing.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cake = await response.Content.ReadFromJsonAsync<CakeDetailDto>();
        cake.Should().NotBeNull();
        cake!.Id.Should().Be(existing.Id);
        cake.Name.Should().Be(existing.Name);
    }

    [Fact]
    public async Task GetCakeById_NotExisting_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/cakes/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ========== POST ==========

    [Fact]
    public async Task CreateCake_AsAdmin_ReturnsCreated()
    {
        var token = await GetAdminTokenAsync();
        var newCake = new CreateCakeDto
        {
            Name = "Новый торт",
            ImageUrl = "https://example.com/new.jpg",
            ThumbnailUrl = "https://example.com/new_thumb.jpg",
            CategorySlug = "classic",
            IsFeatured = true,
            Description = "Описание нового торта",
            FillingId = null
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/cakes", newCake, token);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CakeDetailDto>();
        created.Should().NotBeNull();
        created!.Name.Should().Be("Новый торт");
        created.CategorySlug.Should().Be("classic");
        created.IsFeatured.Should().BeTrue();

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.PathAndQuery.Should().ContainEquivalentOf($"api/cakes/{created.Id}");
    }

    [Fact]
    public async Task CreateCake_WithNonExistingCategory_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        var newCake = new CreateCakeDto
        {
            Name = "Торт без категории",
            ImageUrl = "https://example.com/cake.jpg",
            ThumbnailUrl = "https://example.com/thumb.jpg",
            CategorySlug = "non-existing-category",
            IsFeatured = false
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/cakes", newCake, token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("does not exist");
    }

    [Fact]
    public async Task CreateCake_WithNonExistingFilling_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        var newCake = new CreateCakeDto
        {
            Name = "Торт с несуществующей начинкой",
            ImageUrl = "https://example.com/cake.jpg",
            ThumbnailUrl = "https://example.com/thumb.jpg",
            CategorySlug = "classic",
            FillingId = 99999
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/cakes", newCake, token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("does not exist");
    }

    [Fact]
    public async Task CreateCake_WithoutToken_ReturnsUnauthorized()
    {
        var newCake = new CreateCakeDto
        {
            Name = "Торт без токена",
            ImageUrl = "https://example.com/cake.jpg",
            ThumbnailUrl = "https://example.com/thumb.jpg",
            CategorySlug = "classic"
        };
        var response = await Client.PostAsJsonAsync("/api/cakes", newCake);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCake_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var newCake = new CreateCakeDto
        {
            Name = "Торт от покупателя",
            ImageUrl = "https://example.com/cake.jpg",
            ThumbnailUrl = "https://example.com/thumb.jpg",
            CategorySlug = "classic"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/cakes", newCake, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== PUT ==========

    [Fact]
    public async Task UpdateCake_AsAdmin_ReturnsOk()
    {
        var created = await CreateFreshCakeAsync("Для обновления");
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateCakeDto
        {
            Id = created.Id,
            Name = "Обновлённый торт",
            ImageUrl = "https://example.com/updated.jpg",
            ThumbnailUrl = "https://example.com/updated_thumb.jpg",
            CategorySlug = "classic",
            IsFeatured = true,
            Description = "Обновлённое описание",
            FillingId = null
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/cakes/{created.Id}", updateDto, token);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 200, got {response.StatusCode}. Response: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CakeDetailDto>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Обновлённый торт");
        updated.IsFeatured.Should().BeTrue();
        updated.Description.Should().Be("Обновлённое описание");
    }

    [Fact]
    public async Task UpdateCake_WithNonExistingCategory_ReturnsBadRequest()
    {
        var created = await CreateFreshCakeAsync("Для обновления с ошибкой");
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateCakeDto
        {
            Id = created.Id,
            Name = "Обновлённый",
            ImageUrl = "https://example.com/updated.jpg",
            ThumbnailUrl = "https://example.com/updated_thumb.jpg",
            CategorySlug = "non-existing-category",
            IsFeatured = false
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/cakes/{created.Id}", updateDto, token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("does not exist");
    }

    [Fact]
    public async Task UpdateCake_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateCakeDto
        {
            Id = 999999,
            Name = "Несуществующий",
            ImageUrl = "https://example.com/cake.jpg",
            ThumbnailUrl = "https://example.com/thumb.jpg",
            CategorySlug = "classic"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/cakes/999999", updateDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCake_WithoutToken_ReturnsUnauthorized()
    {
        var updateDto = new UpdateCakeDto
        {
            Id = 1,
            Name = "Обновлённый",
            ImageUrl = "https://example.com/cake.jpg",
            ThumbnailUrl = "https://example.com/thumb.jpg",
            CategorySlug = "classic"
        };
        var response = await Client.PutAsJsonAsync("/api/cakes/1", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCake_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var updateDto = new UpdateCakeDto
        {
            Id = 1,
            Name = "Обновлённый",
            ImageUrl = "https://example.com/cake.jpg",
            ThumbnailUrl = "https://example.com/thumb.jpg",
            CategorySlug = "classic"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/cakes/1", updateDto, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== DELETE ==========

    [Fact]
    public async Task DeleteCake_Existing_ReturnsNoContent()
    {
        var created = await CreateFreshCakeAsync("Для удаления");
        var token = await GetAdminTokenAsync();

        var response = await AuthorizedRequestAsync(HttpMethod.Delete, $"/api/cakes/{created.Id}", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/cakes/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCake_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/cakes/999999", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCake_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.DeleteAsync("/api/cakes/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCake_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/cakes/1", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}