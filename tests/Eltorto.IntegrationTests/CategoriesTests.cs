using System.Net;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using FluentAssertions;

namespace Eltorto.IntegrationTests;

public class CategoriesTests : IntegrationTestBase
{
    private async Task<CategoryDto> CreateFreshCategoryAsync(string slug = "fresh", string name = "Свежая", int sortOrder = 50)
    {
        var token = await GetAdminTokenAsync();
        var createDto = new CreateCategoryDto { Slug = slug, Name = name, SortOrder = sortOrder };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/categories", createDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CategoryDto>())!;
    }

    // ========== GET ==========

    [Fact]
    public async Task GetCategories_ShouldReturnList_WithSeedData()
    {
        var response = await Client.GetAsync("/api/categories");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        categories.Should().NotBeNull();
        categories!.Count.Should().BeGreaterThan(0);
        categories.Should().Contain(c => c.Slug == "classic");
    }

    [Fact]
    public async Task GetCategoryBySlug_Existing_ReturnsOk()
    {
        var slug = "classic";
        var response = await Client.GetAsync($"/api/categories/by-slug/{slug}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        category.Should().NotBeNull();
        category!.Slug.Should().Be(slug);
        category.Name.Should().Be("Классические");
    }

    [Fact]
    public async Task GetCategoryBySlug_NotExisting_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/categories/by-slug/non-existing-slug");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCategoryWithCakes_Existing_ReturnsOkWithCakes()
    {
        var response = await Client.GetAsync("/api/categories/classic/with-cakes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categoryWithCakes = await response.Content.ReadFromJsonAsync<CategoryWithCakesDto>();
        categoryWithCakes.Should().NotBeNull();
        categoryWithCakes!.Slug.Should().Be("classic");
        categoryWithCakes.Cakes.Should().NotBeNull();
        categoryWithCakes.Cakes.Should().HaveCountGreaterThan(0);
        categoryWithCakes.Cakes.Should().Contain(c => c.Name == "Наполеон");
    }

    [Fact]
    public async Task GetCategoryWithCakes_NotExisting_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/categories/non-existing-slug/with-cakes");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ========== POST ==========

    [Fact]
    public async Task CreateCategory_AsAdmin_ReturnsCreated()
    {
        var token = await GetAdminTokenAsync();
        var newCategory = new CreateCategoryDto
        {
            Slug = "new-category",
            Name = "Новая категория",
            Description = "Описание новой категории",
            SortOrder = 10
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/categories", newCategory, token);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
        created.Should().NotBeNull();
        created!.Slug.Should().Be("new-category");
        created.Name.Should().Be("Новая категория");
        created.Description.Should().Be("Описание новой категории");
        created.SortOrder.Should().Be(10);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.PathAndQuery.Should().ContainEquivalentOf($"api/categories/{created.Id}");
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateSlug_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        var newCategory = new CreateCategoryDto
        {
            Slug = "classic",
            Name = "Дубликат",
            SortOrder = 1
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/categories", newCategory, token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateCategory_WithoutToken_ReturnsUnauthorized()
    {
        var newCategory = new CreateCategoryDto { Slug = "test", Name = "Test" };
        var response = await Client.PostAsJsonAsync("/api/categories", newCategory);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCategory_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var newCategory = new CreateCategoryDto { Slug = "test", Name = "Test" };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/categories", newCategory, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== PUT ==========

    [Fact]
    public async Task UpdateCategory_AsAdmin_ReturnsOk()
    {
        var created = await CreateFreshCategoryAsync("update-me", "Для обновления", 60);
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateCategoryDto
        {
            Id = created.Id,
            Slug = created.Slug,
            Name = "Обновлённая категория",
            Description = "Новое описание",
            SortOrder = 70
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/categories/{created.Id}", updateDto, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CategoryDto>();
        updated.Should().NotBeNull();
        updated!.Slug.Should().Be(created.Slug);
        updated.Name.Should().Be("Обновлённая категория");
        updated.Description.Should().Be("Новое описание");
        updated.SortOrder.Should().Be(70);
    }

    [Fact]
    public async Task UpdateCategory_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateCategoryDto { Id = 999999, Slug = "non-existing", Name = "Non-existing", SortOrder = 1 };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/categories/999999", updateDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_WithoutToken_ReturnsUnauthorized()
    {
        var updateDto = new UpdateCategoryDto { Id = 1, Slug = "test", Name = "Test" };
        var response = await Client.PutAsJsonAsync("/api/categories/1", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategory_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var updateDto = new UpdateCategoryDto { Id = 1, Slug = "test", Name = "Test" };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/categories/1", updateDto, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== DELETE ==========

    [Fact]
    public async Task DeleteCategory_WithoutCakes_ReturnsNoContent()
    {
        var token = await GetAdminTokenAsync();
        var created = await CreateFreshCategoryAsync("to-delete", "Для удаления", 100);

        var response = await AuthorizedRequestAsync(HttpMethod.Delete, $"/api/categories/{created.Id}", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/categories/by-slug/to-delete");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_WithCakes_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        var getResponse = await Client.GetAsync("/api/categories/by-slug/classic");
        var category = await getResponse.Content.ReadFromJsonAsync<CategoryDto>();

        var response = await AuthorizedRequestAsync(HttpMethod.Delete, $"/api/categories/{category!.Id}", token: token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("cakes");
    }

    [Fact]
    public async Task DeleteCategory_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/categories/999999", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.DeleteAsync("/api/categories/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCategory_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/categories/1", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}