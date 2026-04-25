namespace Eltorto.Api.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _serviceMock;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _serviceMock = new Mock<ICategoryService>();
        var loggerMock = new Mock<ILogger<CategoriesController>>();
        _controller = new CategoriesController(_serviceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<CategoryDto>());
        var result = await _controller.GetAll(CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_Exists_ReturnsOk()
    {
        var dto = new CategoryDto { Id = 1, Name = "Cakes" };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(dto);
        var result = await _controller.GetById(1, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task GetById_NotExists_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((CategoryDto?)null);
        var result = await _controller.GetById(99, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetBySlug_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetBySlugAsync("classic", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new CategoryDto());
        var result = await _controller.GetBySlug("classic", CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetBySlug_NotFound()
    {
        _serviceMock.Setup(s => s.GetBySlugAsync("missing", It.IsAny<CancellationToken>()))
                   .ReturnsAsync((CategoryDto?)null);
        var result = await _controller.GetBySlug("missing", CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetWithCakes_Ok()
    {
        _serviceMock.Setup(s => s.GetWithCakesAsync("classic", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new CategoryWithCakesDto());
        var result = await _controller.GetWithCakes("classic", CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetWithCakes_NotFound()
    {
        _serviceMock.Setup(s => s.GetWithCakesAsync("missing", It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new KeyNotFoundException());
        var result = await _controller.GetWithCakes("missing", CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var createDto = new CreateCategoryDto();
        var created = new CategoryDto { Id = 1 };
        _serviceMock.Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(created);
        var result = await _controller.Create(createDto, CancellationToken.None);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
    }

    [Fact]
    public async Task Create_InvalidOperation_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateCategoryDto>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("Duplicate"));
        var result = await _controller.Create(new CreateCategoryDto(), CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_IdMismatch_BadRequest()
    {
        var dto = new UpdateCategoryDto { Id = 2 };
        var result = await _controller.Update(1, dto, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_NotFound_404()
    {
        var dto = new UpdateCategoryDto { Id = 1 };
        _serviceMock.Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new KeyNotFoundException());
        var result = await _controller.Update(1, dto, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Valid_Ok()
    {
        var dto = new UpdateCategoryDto { Id = 1 };
        var updated = new CategoryDto { Id = 1 };
        _serviceMock.Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(updated);
        var result = await _controller.Update(1, dto, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_Success_NoContent()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);
        var result = await _controller.Delete(1, CancellationToken.None);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_404()
    {
        _serviceMock.Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new KeyNotFoundException());
        var result = await _controller.Delete(99, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_InvalidOp_BadRequest()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("Has cakes"));
        var result = await _controller.Delete(1, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }
}