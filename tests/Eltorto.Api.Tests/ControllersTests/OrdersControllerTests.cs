namespace Eltorto.Api.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _serviceMock;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _serviceMock = new Mock<IOrderService>();
        var loggerMock = new Mock<ILogger<OrdersController>>();
        _controller = new OrdersController(_serviceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreated()
    {
        var dto = new CreateOrderDto();
        var order = new OrderDto { Id = 1 };
        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(order);
        var result = await _controller.Create(dto, CancellationToken.None);
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetById), created.ActionName);
    }

    [Fact]
    public async Task Create_InvalidOp_BadRequest()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateOrderDto>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("error"));
        var result = await _controller.Create(new CreateOrderDto(), CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetById_Exists_Ok()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new OrderDto());
        var result = await _controller.GetById(1, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_NotFound_404()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((OrderDto?)null);
        var result = await _controller.GetById(1, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetByPhone_ReturnsList()
    {
        _serviceMock.Setup(s => s.GetByCustomerPhoneAsync("123", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<OrderDto>());
        var result = await _controller.GetByPhone("123", CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAll_Admin_Ok()
    {
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<OrderDto>());
        var result = await _controller.GetAll(CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPaged_Admin_Ok()
    {
        var paged = new PagedResultDto<OrderDto>
        {
            Items = new List<OrderDto>(),
            Page = 1,
            PageSize = 20,
            TotalCount = 0
        };
        _serviceMock.Setup(s => s.GetPagedAsync(1, 20, null, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(paged);
        var result = await _controller.GetPaged(1, 20, null, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_Success_Ok()
    {
        var statusDto = new UpdateOrderStatusDto { Status = "Processing" };
        _serviceMock.Setup(s => s.UpdateStatusAsync(1, "Processing", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new OrderDto());
        var result = await _controller.UpdateStatus(1, statusDto, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_NotFound_404()
    {
        _serviceMock.Setup(s => s.UpdateStatusAsync(1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new KeyNotFoundException());
        var result = await _controller.UpdateStatus(1, new UpdateOrderStatusDto(), CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }
}