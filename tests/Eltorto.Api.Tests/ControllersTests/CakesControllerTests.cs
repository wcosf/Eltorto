namespace Eltorto.Api.Tests.Controllers;

public class CakesControllerTests
{
    private readonly Mock<ICakeService> _cakeServiceMock;
    private readonly CakesController _controller;

    public CakesControllerTests()
    {
        _cakeServiceMock = new Mock<ICakeService>();
        Mock<ILogger<CakesController>> loggerMock = new();
        _controller = new CakesController(_cakeServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithCakes()
    {
        var cakes = new List<CakeListDto> { new() { Id = 1, Name = "Napoleon" } };
        _cakeServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(cakes);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(cakes, okResult.Value);
    }

    [Fact]
    public async Task GetPaged_ReturnsPagedResult()
    {
        var paged = new PagedResultDto<CakeListDto>
        {
            Items = new List<CakeListDto> { new() { Id = 1 } },
            Page = 1,
            PageSize = 12,
            TotalCount = 1
        };
        _cakeServiceMock.Setup(s => s.GetPagedAsync(1, 12, null, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(paged);

        var result = await _controller.GetPaged(1, 12, null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(paged, okResult.Value);
    }

    [Fact]
    public async Task GetFeatured_ReturnsOkWithList()
    {
        var list = new List<CakeListDto> { new() { Id = 2 } };
        _cakeServiceMock.Setup(s => s.GetFeaturedAsync(6, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(list);

        var result = await _controller.GetFeatured(6, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, okResult.Value);
    }

    [Fact]
    public async Task GetByCategory_ReturnsCakes()
    {
        var cakes = new List<CakeListDto> { new() };
        _cakeServiceMock.Setup(s => s.GetByCategoryAsync("classic", It.IsAny<CancellationToken>()))
                       .ReturnsAsync(cakes);

        var result = await _controller.GetByCategory("classic", CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetByFilling_ReturnsOk()
    {
        var cakes = new List<CakeListDto>();
        _cakeServiceMock.Setup(s => s.GetByFillingAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(cakes);

        var result = await _controller.GetByFilling(1, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var cake = new CakeDetailDto { Id = 5 };
        _cakeServiceMock.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(cake);

        var result = await _controller.GetById(5, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(cake, okResult.Value);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _cakeServiceMock.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((CakeDetailDto?)null);

        var result = await _controller.GetById(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        var createDto = new CreateCakeDto();
        var created = new CakeDetailDto { Id = 10 };
        _cakeServiceMock.Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(created);

        var result = await _controller.Create(createDto, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
        Assert.Equal(created, createdResult.Value);
    }

    [Fact]
    public async Task Create_InvalidOperation_ReturnsBadRequest()
    {
        var createDto = new CreateCakeDto();
        _cakeServiceMock.Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new InvalidOperationException("Error"));

        var result = await _controller.Create(createDto, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Error", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var updateDto = new UpdateCakeDto { Id = 2 };
        var result = await _controller.Update(1, updateDto, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_KeyNotFound_Returns404()
    {
        var updateDto = new UpdateCakeDto { Id = 1 };
        _cakeServiceMock.Setup(s => s.UpdateAsync(updateDto, It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Update(1, updateDto, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Valid_ReturnsOk()
    {
        var updateDto = new UpdateCakeDto { Id = 1 };
        var updated = new CakeDetailDto { Id = 1 };
        _cakeServiceMock.Setup(s => s.UpdateAsync(updateDto, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(updated);

        var result = await _controller.Update(1, updateDto, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updated, okResult.Value);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        _cakeServiceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);

        var result = await _controller.Delete(1, CancellationToken.None);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        _cakeServiceMock.Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Delete(99, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }
}