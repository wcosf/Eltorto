using System.Net;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using FluentAssertions;

namespace Eltorto.IntegrationTests;

public class ContentBlocksTests : IntegrationTestBase
{
    private async Task<PageDto> GetAboutPageAsync()
    {
        var response = await Client.GetAsync("/api/pages/by-slug/about");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<PageDto>())!;
    }

    private async Task<ContentBlockDto> CreateFreshBlockAsync(int pageId, string title = "Тестовый блок", int sortOrder = 99)
    {
        var token = await GetAdminTokenAsync();
        var createDto = new CreateContentBlockDto
        {
            Title = title,
            Text = "Текст тестового блока",
            ImageUrl = "https://example.com/block.jpg",
            SortOrder = sortOrder
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, $"/api/pages/{pageId}/blocks", createDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ContentBlockDto>())!;
    }

    // ========== GET ==========

    [Fact]
    public async Task GetPageBlocks_ExistingPage_ReturnsList()
    {
        var page = await GetAboutPageAsync();
        var response = await Client.GetAsync($"/api/pages/{page.Id}/blocks");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var blocks = await response.Content.ReadFromJsonAsync<List<ContentBlockDto>>();
        blocks.Should().NotBeNull();
        blocks!.Should().HaveCountGreaterThan(0);
        blocks.Should().Contain(b => b.Title == "Наша история");
    }

    [Fact]
    public async Task GetPageBlocks_NotExistingPage_ReturnsEmptyList()
    {
        var response = await Client.GetAsync("/api/pages/99999/blocks");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var blocks = await response.Content.ReadFromJsonAsync<List<ContentBlockDto>>();
        blocks.Should().NotBeNull();
        blocks!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBlockById_Existing_ReturnsOk()
    {
        var page = await GetAboutPageAsync();
        var blocksResponse = await Client.GetAsync($"/api/pages/{page.Id}/blocks");
        var blocks = await blocksResponse.Content.ReadFromJsonAsync<List<ContentBlockDto>>();
        var existingBlock = blocks!.First();

        var response = await Client.GetAsync($"/api/pages/blocks/{existingBlock.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var block = await response.Content.ReadFromJsonAsync<ContentBlockDto>();
        block.Should().NotBeNull();
        block!.Id.Should().Be(existingBlock.Id);
        block.Title.Should().Be(existingBlock.Title);
        block.Text.Should().Be(existingBlock.Text);
    }

    [Fact]
    public async Task GetBlockById_NotExisting_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/pages/blocks/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ========== POST ==========

    [Fact]
    public async Task AddBlock_AsAdmin_ReturnsCreated()
    {
        var page = await GetAboutPageAsync();
        var token = await GetAdminTokenAsync();
        var createDto = new CreateContentBlockDto
        {
            Title = "Новый блок",
            Text = "Текст нового блока",
            ImageUrl = "https://example.com/new.jpg",
            SortOrder = 10
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, $"/api/pages/{page.Id}/blocks", createDto, token);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ContentBlockDto>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("Новый блок");
        created.Text.Should().Be("Текст нового блока");
        created.ImageUrl.Should().Be("https://example.com/new.jpg");
        created.SortOrder.Should().Be(10);
        created.PageId.Should().Be(page.Id);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.PathAndQuery.Should().ContainEquivalentOf($"api/pages/blocks/{created.Id}");
    }

    [Fact]
    public async Task AddBlock_WithDuplicateTitle_ReturnsBadRequest()
    {
        var page = await GetAboutPageAsync();
        var token = await GetAdminTokenAsync();
        var createDto = new CreateContentBlockDto
        {
            Title = "Наша история",
            Text = "Текст",
            SortOrder = 1
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, $"/api/pages/{page.Id}/blocks", createDto, token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task AddBlock_ToNotExistingPage_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var createDto = new CreateContentBlockDto
        {
            Title = "Блок на несуществующей странице",
            Text = "Текст"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/pages/99999/blocks", createDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task AddBlock_WithoutToken_ReturnsUnauthorized()
    {
        var page = await GetAboutPageAsync();
        var createDto = new CreateContentBlockDto { Title = "Test", Text = "Text" };
        var response = await Client.PostAsJsonAsync($"/api/pages/{page.Id}/blocks", createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddBlock_WithCustomerToken_ReturnsForbidden()
    {
        var page = await GetAboutPageAsync();
        var customerToken = await CreateAndLoginCustomerAsync();
        var createDto = new CreateContentBlockDto { Title = "Test", Text = "Text" };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, $"/api/pages/{page.Id}/blocks", createDto, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== PUT ==========

    [Fact]
    public async Task UpdateBlock_AsAdmin_ReturnsOk()
    {
        var page = await GetAboutPageAsync();
        var created = await CreateFreshBlockAsync(page.Id, "Для обновления");
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateContentBlockDto
        {
            Id = created.Id,
            Title = "Обновлённый блок",
            Text = "Обновлённый текст",
            ImageUrl = "https://example.com/updated.jpg",
            SortOrder = 20
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/pages/blocks/{created.Id}", updateDto, token);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 200, got {response.StatusCode}. Response: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ContentBlockDto>();
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Обновлённый блок");
        updated.Text.Should().Be("Обновлённый текст");
        updated.ImageUrl.Should().Be("https://example.com/updated.jpg");
        updated.SortOrder.Should().Be(20);
        updated.PageId.Should().Be(page.Id);
    }

    [Fact]
    public async Task UpdateBlock_WithDuplicateTitle_ReturnsBadRequest()
    {
        var page = await GetAboutPageAsync();
        var first = await CreateFreshBlockAsync(page.Id, "Первый блок");
        var second = await CreateFreshBlockAsync(page.Id, "Второй блок");

        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateContentBlockDto
        {
            Id = second.Id,
            Title = "Первый блок",
            Text = "Новый текст",
            SortOrder = 1
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/pages/blocks/{second.Id}", updateDto, token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdateBlock_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateContentBlockDto
        {
            Id = 999999,
            Title = "Несуществующий",
            Text = "Текст"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/pages/blocks/999999", updateDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateBlock_WithoutToken_ReturnsUnauthorized()
    {
        var page = await GetAboutPageAsync();
        var blocks = await (await Client.GetAsync($"/api/pages/{page.Id}/blocks")).Content.ReadFromJsonAsync<List<ContentBlockDto>>();
        var block = blocks!.First();
        var updateDto = new UpdateContentBlockDto { Id = block.Id, Title = "Test", Text = "Text" };
        var response = await Client.PutAsJsonAsync($"/api/pages/blocks/{block.Id}", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateBlock_WithCustomerToken_ReturnsForbidden()
    {
        var page = await GetAboutPageAsync();
        var blocks = await (await Client.GetAsync($"/api/pages/{page.Id}/blocks")).Content.ReadFromJsonAsync<List<ContentBlockDto>>();
        var block = blocks!.First();
        var customerToken = await CreateAndLoginCustomerAsync();
        var updateDto = new UpdateContentBlockDto { Id = block.Id, Title = "Test", Text = "Text" };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/pages/blocks/{block.Id}", updateDto, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== DELETE ==========

    [Fact]
    public async Task DeleteBlock_Existing_ReturnsNoContent()
    {
        var page = await GetAboutPageAsync();
        var created = await CreateFreshBlockAsync(page.Id, "Для удаления");
        var token = await GetAdminTokenAsync();

        var response = await AuthorizedRequestAsync(HttpMethod.Delete, $"/api/pages/blocks/{created.Id}", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/pages/blocks/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBlock_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/pages/blocks/999999", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBlock_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.DeleteAsync("/api/pages/blocks/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteBlock_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/pages/blocks/1", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== POST ==========

    [Fact]
    public async Task ReorderBlocks_AsAdmin_ReturnsNoContent()
    {
        var page = await GetAboutPageAsync();
        var token = await GetAdminTokenAsync();
        var blocksResponse = await Client.GetAsync($"/api/pages/{page.Id}/blocks");
        var blocks = await blocksResponse.Content.ReadFromJsonAsync<List<ContentBlockDto>>();
        var orderedIds = blocks!.Select(b => b.Id).ToList();
        orderedIds.Reverse();

        var response = await AuthorizedRequestAsync(HttpMethod.Post, $"/api/pages/{page.Id}/blocks/reorder", orderedIds, token);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedResponse = await Client.GetAsync($"/api/pages/{page.Id}/blocks");
        var updatedBlocks = await updatedResponse.Content.ReadFromJsonAsync<List<ContentBlockDto>>();
        updatedBlocks.Should().NotBeNull();
        updatedBlocks!.Select(b => b.Id).Should().Equal(orderedIds);
    }

    [Fact]
    public async Task ReorderBlocks_WithInvalidIds_ReturnsNoContentAndIgnores()
    {
        var page = await GetAboutPageAsync();
        var token = await GetAdminTokenAsync();
        var blocksResponse = await Client.GetAsync($"/api/pages/{page.Id}/blocks");
        var blocks = await blocksResponse.Content.ReadFromJsonAsync<List<ContentBlockDto>>();
        var orderedIds = blocks!.Select(b => b.Id).ToList();
        orderedIds.Add(99999);

        var response = await AuthorizedRequestAsync(HttpMethod.Post, $"/api/pages/{page.Id}/blocks/reorder", orderedIds, token);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedResponse = await Client.GetAsync($"/api/pages/{page.Id}/blocks");
        var updatedBlocks = await updatedResponse.Content.ReadFromJsonAsync<List<ContentBlockDto>>();
        updatedBlocks.Should().NotBeNull();
        updatedBlocks!.Select(b => b.Id).Should().Equal(blocks.Select(b => b.Id));
    }

    [Fact]
    public async Task ReorderBlocks_WithoutToken_ReturnsUnauthorized()
    {
        var page = await GetAboutPageAsync();
        var orderedIds = new List<int> { 1, 2 };
        var response = await Client.PostAsJsonAsync($"/api/pages/{page.Id}/blocks/reorder", orderedIds);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReorderBlocks_WithCustomerToken_ReturnsForbidden()
    {
        var page = await GetAboutPageAsync();
        var customerToken = await CreateAndLoginCustomerAsync();
        var orderedIds = new List<int> { 1, 2 };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, $"/api/pages/{page.Id}/blocks/reorder", orderedIds, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}