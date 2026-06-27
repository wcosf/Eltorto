using System.Net;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using FluentAssertions;

namespace Eltorto.IntegrationTests;

[Collection("IntegrationTests")]
public class SliderTests : IntegrationTestBase
{
    private async Task<SliderItemDto> CreateFreshSliderItemAsync(
        string title = "Тестовый слайд",
        int sortOrder = 99)
    {
        var token = await GetAdminTokenAsync();
        var createDto = new CreateSliderItemDto
        {
            ImageUrl = "https://example.com/test_slide.jpg",
            Title = title,
            Subtitle = "Тестовый подзаголовок",
            SortOrder = sortOrder
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/slider", createDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<SliderItemDto>())!;
    }

    // ========== GET ==========

    [Fact]
    public async Task GetAllSlides_ShouldReturnOrderedList()
    {
        var response = await Client.GetAsync("/api/slider");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var slides = await response.Content.ReadFromJsonAsync<List<SliderItemDto>>();
        slides.Should().NotBeNull();
        slides!.Count.Should().BeGreaterThan(0);

        slides.Should().BeInAscendingOrder(s => s.SortOrder);
        slides.Should().Contain(s => s.Title == "Первый слайд");
    }

    [Fact]
    public async Task GetSlideById_Existing_ReturnsOk()
    {
        var allResponse = await Client.GetAsync("/api/slider");
        var all = await allResponse.Content.ReadFromJsonAsync<List<SliderItemDto>>();
        var existing = all!.First();

        var response = await Client.GetAsync($"/api/slider/{existing.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var slide = await response.Content.ReadFromJsonAsync<SliderItemDto>();
        slide.Should().NotBeNull();
        slide!.Id.Should().Be(existing.Id);
        slide.Title.Should().Be(existing.Title);
    }

    [Fact]
    public async Task GetSlideById_NotExisting_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/slider/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ========== POST ==========

    [Fact]
    public async Task CreateSlide_AsAdmin_ReturnsCreated()
    {
        var token = await GetAdminTokenAsync();
        var createDto = new CreateSliderItemDto
        {
            ImageUrl = "https://example.com/new_slide.jpg",
            Title = "Новый слайд",
            Subtitle = "Подзаголовок нового слайда",
            SortOrder = 10
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/slider", createDto, token);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<SliderItemDto>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("Новый слайд");
        created.Subtitle.Should().Be("Подзаголовок нового слайда");
        created.SortOrder.Should().Be(10);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.PathAndQuery.Should().ContainEquivalentOf($"api/slider/{created.Id}");
    }

    [Fact]
    public async Task CreateSlide_WithoutToken_ReturnsUnauthorized()
    {
        var createDto = new CreateSliderItemDto
        {
            ImageUrl = "https://example.com/slide.jpg",
            Title = "Без токена"
        };
        var response = await Client.PostAsJsonAsync("/api/slider", createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateSlide_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var createDto = new CreateSliderItemDto
        {
            ImageUrl = "https://example.com/slide.jpg",
            Title = "От покупателя"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/slider", createDto, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== PUT ==========

    [Fact]
    public async Task UpdateSlide_AsAdmin_ReturnsOk()
    {
        var created = await CreateFreshSliderItemAsync("Для обновления", 50);
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateSliderItemDto
        {
            Id = created.Id,
            ImageUrl = "https://example.com/updated_slide.jpg",
            Title = "Обновлённый слайд",
            Subtitle = "Обновлённый подзаголовок",
            SortOrder = 60
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/slider/{created.Id}", updateDto, token);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 200, got {response.StatusCode}. Response: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<SliderItemDto>();
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Обновлённый слайд");
        updated.Subtitle.Should().Be("Обновлённый подзаголовок");
        updated.SortOrder.Should().Be(60);
    }

    [Fact]
    public async Task UpdateSlide_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateSliderItemDto
        {
            Id = 999999,
            ImageUrl = "https://example.com/slide.jpg",
            Title = "Несуществующий"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/slider/999999", updateDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSlide_WithoutToken_ReturnsUnauthorized()
    {
        var allResponse = await Client.GetAsync("/api/slider");
        var all = await allResponse.Content.ReadFromJsonAsync<List<SliderItemDto>>();
        var slide = all!.First();
        var updateDto = new UpdateSliderItemDto
        {
            Id = slide.Id,
            ImageUrl = "https://example.com/slide.jpg",
            Title = "Обновлённый"
        };
        var response = await Client.PutAsJsonAsync($"/api/slider/{slide.Id}", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateSlide_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var allResponse = await Client.GetAsync("/api/slider");
        var all = await allResponse.Content.ReadFromJsonAsync<List<SliderItemDto>>();
        var slide = all!.First();
        var updateDto = new UpdateSliderItemDto
        {
            Id = slide.Id,
            ImageUrl = "https://example.com/slide.jpg",
            Title = "Обновлённый"
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/slider/{slide.Id}", updateDto, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== DELETE ==========

    [Fact]
    public async Task DeleteSlide_Existing_ReturnsNoContent()
    {
        var created = await CreateFreshSliderItemAsync("Для удаления", 100);
        var token = await GetAdminTokenAsync();

        var response = await AuthorizedRequestAsync(HttpMethod.Delete, $"/api/slider/{created.Id}", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/slider/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSlide_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/slider/999999", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSlide_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.DeleteAsync("/api/slider/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteSlide_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/slider/1", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ========== POST ==========

    [Fact]
    public async Task ReorderSlides_AsAdmin_ReturnsNoContent()
    {
        var token = await GetAdminTokenAsync();

        var existingResponse = await Client.GetAsync("/api/slider");
        var existingSlides = await existingResponse.Content.ReadFromJsonAsync<List<SliderItemDto>>();
        if (existingSlides != null)
        {
            foreach (var slide in existingSlides)
            {
                await AuthorizedRequestAsync(HttpMethod.Delete, $"/api/slider/{slide.Id}", token: token);
            }
        }

        var slide1 = await CreateFreshSliderItemAsync("Первый", 1);
        var slide2 = await CreateFreshSliderItemAsync("Второй", 2);
        var slide3 = await CreateFreshSliderItemAsync("Третий", 3);

        var orderedIds = new List<int> { slide3.Id, slide2.Id, slide1.Id };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/slider/reorder", orderedIds, token);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAllResponse = await Client.GetAsync("/api/slider");
        var allSlides = await getAllResponse.Content.ReadFromJsonAsync<List<SliderItemDto>>();
        allSlides.Should().NotBeNull();
        allSlides!.Select(s => s.Id).Should().Equal(orderedIds);
    }

    [Fact]
    public async Task ReorderSlides_WithInvalidIds_ReturnsNoContentAndIgnores()
    {
        var token = await GetAdminTokenAsync();
        var slide1 = await CreateFreshSliderItemAsync("Первый", 1);
        var slide2 = await CreateFreshSliderItemAsync("Второй", 2);

        var orderedIds = new List<int> { slide2.Id, 999999, slide1.Id };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/slider/reorder", orderedIds, token);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAllResponse = await Client.GetAsync("/api/slider");
        var allSlides = await getAllResponse.Content.ReadFromJsonAsync<List<SliderItemDto>>();
        allSlides.Should().NotBeNull();

        var first = allSlides!.FirstOrDefault(s => s.Id == slide1.Id);
        var second = allSlides.FirstOrDefault(s => s.Id == slide2.Id);
        first.Should().NotBeNull();
        second.Should().NotBeNull();
        first.SortOrder.Should().NotBe(second.SortOrder);
    }

    [Fact]
    public async Task ReorderSlides_WithoutToken_ReturnsUnauthorized()
    {
        var orderedIds = new List<int> { 1, 2 };
        var response = await Client.PostAsJsonAsync("/api/slider/reorder", orderedIds);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReorderSlides_WithCustomerToken_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var orderedIds = new List<int> { 1, 2 };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/slider/reorder", orderedIds, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}