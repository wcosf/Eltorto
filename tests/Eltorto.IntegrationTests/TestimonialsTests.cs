using System.Net;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using FluentAssertions;

namespace Eltorto.IntegrationTests;

[Collection("IntegrationTests")]
public class TestimonialsTests : IntegrationTestBase
{
    private async Task<TestimonialDto> CreateFreshTestimonialAsync(
        string author = "Тестовый автор",
        string text = "Тестовый отзыв",
        string? token = null)
    {
        if (token == null)
            token = await GetAdminTokenAsync();

        var createDto = new CreateTestimonialDto
        {
            Author = author,
            Email = $"{author.Replace(" ", "")}@test.ru",
            Text = text
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/testimonials", createDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<TestimonialDto>())!;
    }

    // ========== GET ==========

    [Fact]
    public async Task GetApprovedTestimonials_ShouldReturnOnlyApproved()
    {
        var response = await Client.GetAsync("/api/testimonials/approved");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var testimonials = await response.Content.ReadFromJsonAsync<List<TestimonialListDto>>();
        testimonials.Should().NotBeNull();
        testimonials.Should().NotBeEmpty();
        testimonials.Should().AllSatisfy(t => t.IsApproved.Should().BeTrue());
    }

    [Fact]
    public async Task GetLatestTestimonials_ShouldReturnLimitedCount()
    {
        var token = await GetAdminTokenAsync();
        await CreateFreshTestimonialAsync("Автор1", "Отзыв 1", token);
        await CreateFreshTestimonialAsync("Автор2", "Отзыв 2", token);
        await CreateFreshTestimonialAsync("Автор3", "Отзыв 3", token);
        await CreateFreshTestimonialAsync("Автор4", "Отзыв 4", token);

        var response = await Client.GetAsync("/api/testimonials/latest?count=3");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var testimonials = await response.Content.ReadFromJsonAsync<List<TestimonialListDto>>();
        testimonials.Should().NotBeNull();
        testimonials!.Count.Should().BeLessThanOrEqualTo(3);
        testimonials.Should().AllSatisfy(t => t.IsApproved.Should().BeTrue());
    }

    [Fact]
    public async Task GetPagedApprovedTestimonials_ReturnsPagedResult()
    {
        var response = await Client.GetAsync("/api/testimonials/paged/approved?page=1&pageSize=2");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResultDto<TestimonialListDto>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().NotBeNull();
        paged.TotalCount.Should().BeGreaterThan(0);
        paged.Page.Should().Be(1);
        paged.PageSize.Should().Be(2);
        paged.TotalPages.Should().BeGreaterThan(0);
        paged.Items.Should().AllSatisfy(t => t.IsApproved.Should().BeTrue());
    }

    [Fact]
    public async Task GetPagedAllTestimonials_AsAdmin_ReturnsAll()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Get, "/api/testimonials/paged/all?page=1&pageSize=10", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResultDto<TestimonialListDto>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().NotBeNull();
        paged.Items.Should().Contain(t => t.IsApproved);
        paged.Items.Should().Contain(t => !t.IsApproved);
    }

    [Fact]
    public async Task GetPagedAllTestimonials_AsCustomer_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Get, "/api/testimonials/paged/all", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPagedAllTestimonials_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/testimonials/paged/all");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTestimonialById_Existing_ReturnsOk()
    {
        var approvedResponse = await Client.GetAsync("/api/testimonials/approved");
        var approved = await approvedResponse.Content.ReadFromJsonAsync<List<TestimonialListDto>>();
        var existing = approved!.First();

        var response = await Client.GetAsync($"/api/testimonials/{existing.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var testimonial = await response.Content.ReadFromJsonAsync<TestimonialDto>();
        testimonial.Should().NotBeNull();
        testimonial!.Id.Should().Be(existing.Id);
    }

    [Fact]
    public async Task GetTestimonialById_NotExisting_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/testimonials/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ========== POST ==========

    [Fact]
    public async Task CreateTestimonial_AsAdmin_ReturnsCreated()
    {
        var token = await GetAdminTokenAsync();
        var created = await CreateFreshTestimonialAsync("Admin Автор", "Отзыв от админа", token);
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);
        created.Author.Should().Be("Admin Автор");
        created.IsApproved.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTestimonial_AsCustomer_ReturnsCreated()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var created = await CreateFreshTestimonialAsync("Customer Автор", "Отзыв от покупателя", customerToken);
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);
        created.Author.Should().Be("Customer Автор");
        created.IsApproved.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTestimonial_WithoutToken_ReturnsUnauthorized()
    {
        var createDto = new CreateTestimonialDto
        {
            Author = "Аноним",
            Text = "Отзыв без токена"
        };
        var response = await Client.PostAsJsonAsync("/api/testimonials", createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateTestimonial_AsAdmin_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        var created = await CreateFreshTestimonialAsync("Для обновления", "Старый текст", token);

        var updateDto = new UpdateTestimonialDto
        {
            Id = created.Id,
            Response = "Ответ администратора",
            IsApproved = true
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/testimonials/{created.Id}", updateDto, token);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 200, got {response.StatusCode}. Response: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TestimonialDto>();
        updated.Should().NotBeNull();
        updated!.Response.Should().Be("Ответ администратора");
        updated.IsApproved.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTestimonial_AsCustomer_ReturnsOk()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var created = await CreateFreshTestimonialAsync("Customer Обновление", "Текст", customerToken);

        var updateDto = new UpdateTestimonialDto
        {
            Id = created.Id,
            Response = null,
            IsApproved = false
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Put, $"/api/testimonials/{created.Id}", updateDto, customerToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TestimonialDto>();
        updated.Should().NotBeNull();
        updated!.IsApproved.Should().BeFalse();
        updated.Response.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTestimonial_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var updateDto = new UpdateTestimonialDto
        {
            Id = 999999,
            Response = "Ответ",
            IsApproved = true
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Put, "/api/testimonials/999999", updateDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTestimonial_WithoutToken_ReturnsUnauthorized()
    {
        var updateDto = new UpdateTestimonialDto { Id = 1, IsApproved = true };
        var response = await Client.PutAsJsonAsync("/api/testimonials/1", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ========== PATCH ==========

    [Fact]
    public async Task ApproveTestimonial_AsAdmin_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        var created = await CreateFreshTestimonialAsync("Для одобрения", "Текст", token);

        var response = await AuthorizedRequestAsync(HttpMethod.Patch, $"/api/testimonials/{created.Id}/approve", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var approved = await response.Content.ReadFromJsonAsync<TestimonialDto>();
        approved.Should().NotBeNull();
        approved!.IsApproved.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveTestimonial_AsCustomer_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var created = await CreateFreshTestimonialAsync("Для одобрения", "Текст", customerToken);

        var response = await AuthorizedRequestAsync(HttpMethod.Patch, $"/api/testimonials/{created.Id}/approve", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveTestimonial_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Patch, "/api/testimonials/999999/approve", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ========== PATCH ==========

    [Fact]
    public async Task AddResponse_AsAdmin_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        var created = await CreateFreshTestimonialAsync("Для ответа", "Текст", token);
        var responseText = "Спасибо за ваш отзыв!";

        var response = await AuthorizedRequestAsync(HttpMethod.Patch, $"/api/testimonials/{created.Id}/response", responseText, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TestimonialDto>();
        updated.Should().NotBeNull();
        updated!.Response.Should().Be(responseText);
    }

    [Fact]
    public async Task AddResponse_AsCustomer_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var created = await CreateFreshTestimonialAsync("Для ответа", "Текст", customerToken);

        var response = await AuthorizedRequestAsync(HttpMethod.Patch, $"/api/testimonials/{created.Id}/response", "Ответ", customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddResponse_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Patch, "/api/testimonials/999999/response", "Ответ", token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ========== DELETE ==========

    [Fact]
    public async Task DeleteTestimonial_AsAdmin_ReturnsNoContent()
    {
        var token = await GetAdminTokenAsync();
        var created = await CreateFreshTestimonialAsync("Для удаления", "Текст", token);

        var response = await AuthorizedRequestAsync(HttpMethod.Delete, $"/api/testimonials/{created.Id}", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/testimonials/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTestimonial_AsCustomer_ReturnsNoContent()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var created = await CreateFreshTestimonialAsync("Для удаления покупателем", "Текст", customerToken);

        var response = await AuthorizedRequestAsync(HttpMethod.Delete, $"/api/testimonials/{created.Id}", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/testimonials/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTestimonial_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Delete, "/api/testimonials/999999", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTestimonial_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.DeleteAsync("/api/testimonials/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}