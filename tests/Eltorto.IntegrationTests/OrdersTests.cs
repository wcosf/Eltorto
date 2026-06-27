using System.Net;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using FluentAssertions;

namespace Eltorto.IntegrationTests;

public class OrdersTests : IntegrationTestBase
{
    private async Task<OrderDto> CreateOrderAsync(string? token = null)
    {
        if (token == null)
            token = await GetAdminTokenAsync();

        var cakesResponse = await Client.GetAsync("/api/cakes");
        var cakes = await cakesResponse.Content.ReadFromJsonAsync<List<CakeListDto>>();
        var cake = cakes!.FirstOrDefault(c => c.Name == "Наполеон");
        var fillingId = cake?.FillingId;

        var createDto = new CreateOrderDto
        {
            CustomerName = "Тест Заказчик",
            CustomerPhone = "+7 (999) 888-77-66",
            CustomerEmail = "test@example.com",
            CakeId = cake?.Id,
            FillingId = fillingId,
            Weight = 1.5m,
            DeliveryDate = DateTime.UtcNow.AddDays(5),
            DeliveryAddress = "г. Тест, ул. Тестовая, д. 1",
            Comment = "Тестовый заказ"
        };

        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/orders", createDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    // ========== POST ==========

    [Fact]
    public async Task CreateOrder_AsAdmin_ReturnsCreated()
    {
        var token = await GetAdminTokenAsync();
        var created = await CreateOrderAsync(token);
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);
        created.Status.Should().Be("New");
        created.CustomerName.Should().Be("Тест Заказчик");
        created.CakeName.Should().Be("Наполеон");
        created.FillingName.Should().Be("Сливочный");
    }

    [Fact]
    public async Task CreateOrder_AsCustomer_ReturnsCreated()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var created = await CreateOrderAsync(customerToken);
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateOrder_WithoutToken_ReturnsUnauthorized()
    {
        var createDto = new CreateOrderDto
        {
            CustomerName = "Тест",
            CustomerPhone = "+7 (999) 000-00-00"
        };
        var response = await Client.PostAsJsonAsync("/api/orders", createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithNonExistingCake_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        var createDto = new CreateOrderDto
        {
            CustomerName = "Тест",
            CustomerPhone = "+7 (999) 111-22-33",
            CakeId = 99999
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/orders", createDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("does not exist");
    }

    [Fact]
    public async Task CreateOrder_WithNonExistingFilling_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        var createDto = new CreateOrderDto
        {
            CustomerName = "Тест",
            CustomerPhone = "+7 (999) 111-22-33",
            FillingId = 99999
        };
        var response = await AuthorizedRequestAsync(HttpMethod.Post, "/api/orders", createDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("does not exist");
    }

    // ========== GET ==========

    [Fact]
    public async Task GetOrderById_AsAdmin_ReturnsOkWithNames()
    {
        var token = await GetAdminTokenAsync();
        var created = await CreateOrderAsync(token);

        var response = await AuthorizedRequestAsync(HttpMethod.Get, $"/api/orders/{created.Id}", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Id.Should().Be(created.Id);
        order.CakeName.Should().Be("Наполеон");
        order.FillingName.Should().Be("Сливочный");
    }

    [Fact]
    public async Task GetOrderById_AsCustomer_ReturnsOk()
    {
        var adminToken = await GetAdminTokenAsync();
        var created = await CreateOrderAsync(adminToken);

        var customerToken = await CreateAndLoginCustomerAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Get, $"/api/orders/{created.Id}", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetOrderById_NotExisting_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Get, "/api/orders/999999", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrderById_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/orders/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOrdersByPhone_AsAdmin_ReturnsList()
    {
        var token = await GetAdminTokenAsync();
        var phone = "+7 (999) 111-22-33";
        var response = await AuthorizedRequestAsync(HttpMethod.Get, $"/api/orders/by-phone/{phone}", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
        orders!.Should().HaveCountGreaterThan(0);
        orders.Should().AllSatisfy(o => o.CustomerPhone.Should().Be(phone));
        orders.Should().AllSatisfy(o => o.CakeName.Should().NotBeNull());
    }

    [Fact]
    public async Task GetOrdersByPhone_AsCustomer_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Get, "/api/orders/by-phone/123", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetOrdersByPhone_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/orders/by-phone/123");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllOrders_AsAdmin_ReturnsList()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Get, "/api/orders/admin/all", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
        orders!.Count.Should().BeGreaterThan(0);
        orders.Should().AllSatisfy(o => o.CakeName.Should().NotBeNull());
    }

    [Fact]
    public async Task GetAllOrders_AsCustomer_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Get, "/api/orders/admin/all", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllOrders_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/orders/admin/all");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPagedOrders_AsAdmin_ReturnsPagedResult()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Get, "/api/orders/admin/paged?page=1&pageSize=5", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResultDto<OrderDto>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().NotBeNull();
        paged.TotalCount.Should().BeGreaterThan(0);
        paged.Page.Should().Be(1);
        paged.PageSize.Should().Be(5);
        paged.TotalPages.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPagedOrders_WithStatusFilter_ReturnsFiltered()
    {
        var token = await GetAdminTokenAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Get, "/api/orders/admin/paged?status=New", token: token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResultDto<OrderDto>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().AllSatisfy(o => o.Status.Should().Be("New"));
    }

    [Fact]
    public async Task GetPagedOrders_AsCustomer_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var response = await AuthorizedRequestAsync(HttpMethod.Get, "/api/orders/admin/paged", token: customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPagedOrders_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/orders/admin/paged");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ========== PATCH ==========

    [Fact]
    public async Task UpdateOrderStatus_AsAdmin_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        var created = await CreateOrderAsync(token);
        var statusDto = new UpdateOrderStatusDto { Status = "Processing" };

        var response = await AuthorizedRequestAsync(HttpMethod.Patch, $"/api/orders/{created.Id}/status", statusDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<OrderDto>();
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("Processing");
    }

    [Fact]
    public async Task UpdateOrderStatus_WithInvalidStatus_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        var created = await CreateOrderAsync(token);
        var statusDto = new UpdateOrderStatusDto { Status = "InvalidStatus" };

        var response = await AuthorizedRequestAsync(HttpMethod.Patch, $"/api/orders/{created.Id}/status", statusDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("Invalid status");
    }

    [Fact]
    public async Task UpdateOrderStatus_NotFound_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var statusDto = new UpdateOrderStatusDto { Status = "Processing" };
        var response = await AuthorizedRequestAsync(HttpMethod.Patch, "/api/orders/999999/status", statusDto, token);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOrderStatus_AsCustomer_ReturnsForbidden()
    {
        var customerToken = await CreateAndLoginCustomerAsync();
        var statusDto = new UpdateOrderStatusDto { Status = "Processing" };
        var response = await AuthorizedRequestAsync(HttpMethod.Patch, "/api/orders/1/status", statusDto, customerToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithoutToken_ReturnsUnauthorized()
    {
        var statusDto = new UpdateOrderStatusDto { Status = "Processing" };
        var response = await Client.PatchAsJsonAsync("/api/orders/1/status", statusDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}