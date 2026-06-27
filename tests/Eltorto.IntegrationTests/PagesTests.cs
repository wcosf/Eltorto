using System.Net;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace Eltorto.IntegrationTests;

public class PagesTests : IntegrationTestBase
{
    // ========== GET ==========

    [Fact]
    public async Task GetPageBySlug_Existing_ReturnsOk()
    {
        var slug = "about";
        var response = await Client.GetAsync($"/api/pages/by-slug/{slug}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PageDto>();
        page.Should().NotBeNull();
        page!.Slug.Should().Be(slug);
        page.Title.Should().Be("О нас");
        page.ContentBlocks.Should().NotBeNull();
        page.ContentBlocks.Should().HaveCountGreaterThan(0);
        page.ContentBlocks.Should().Contain(b => b.Title == "Наша история");
    }

    [Fact]
    public async Task GetPageBySlug_NotExisting_ReturnsNotFound()
    {
        var slug = "non-existing-slug";
        var response = await Client.GetAsync($"/api/pages/by-slug/{slug}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPageById_Existing_ReturnsOk()
    {
        var slugResponse = await Client.GetAsync("/api/pages/by-slug/about");
        slugResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var pageFromSlug = await slugResponse.Content.ReadFromJsonAsync<PageDto>();
        var id = pageFromSlug!.Id;

        var response = await Client.GetAsync($"/api/pages/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PageDto>();
        page.Should().NotBeNull();
        page!.Id.Should().Be(id);
        page.Slug.Should().Be("about");
        page.Title.Should().Be("О нас");
        page.ContentBlocks.Should().NotBeNull();
        page.ContentBlocks.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetPageById_NotExisting_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/pages/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllPages_ReturnsList()
    {
        var response = await Client.GetAsync("/api/pages");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pages = await response.Content.ReadFromJsonAsync<List<PageDto>>();
        pages.Should().NotBeNull();
        pages!.Count.Should().BeGreaterThan(0);
        pages.Should().Contain(p => p.Slug == "about");
    }

    // ========== PUT ==========

    [Fact]
    public async Task UpdatePage_AsAdmin_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();

        var getResponse = await Client.GetAsync("/api/pages/by-slug/about");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var existing = await getResponse.Content.ReadFromJsonAsync<PageDto>();
        var id = existing!.Id;

        var updateDto = new UpdatePageDto
        {
            Title = "Обновлённый заголовок",
            MetaDescription = "Новое описание для SEO",
            Heading = "Новый заголовок H1",
            Subheading = "Новый подзаголовок",
            Content = "<p>Обновлённый контент страницы</p>"
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/pages/{id}", updateDto, token);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 200, got {response.StatusCode}. Response: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<PageDto>();
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Обновлённый заголовок");
        updated.MetaDescription.Should().Be("Новое описание для SEO");
        updated.Heading.Should().Be("Новый заголовок H1");
        updated.Subheading.Should().Be("Новый подзаголовок");
        updated.Content.Should().Be("<p>Обновлённый контент страницы</p>");
        updated.Slug.Should().Be("about");
    }

    [Fact]
    public async Task UpdatePage_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdatePageDto
        {
            Title = "Новый заголовок",
            MetaDescription = "Описание",
            Heading = "H1"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/pages/999999", updateDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePage_WithoutToken_ReturnsUnauthorized()
    {
        var updateDto = new UpdatePageDto
        {
            Title = "Новый заголовок",
            MetaDescription = "Описание",
            Heading = "H1"
        };
        var response = await Client.PutAsJsonAsync("/api/pages/1", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePage_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var updateDto = new UpdatePageDto
        {
            Title = "Новый заголовок",
            MetaDescription = "Описание",
            Heading = "H1"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/pages/1", updateDto, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}