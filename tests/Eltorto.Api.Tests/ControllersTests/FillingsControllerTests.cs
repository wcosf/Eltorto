namespace Eltorto.Api.Tests.Controllers;

public class FillingsControllerTests
{
    private readonly Mock<IFillingService> _fillingServiceMock;
    private readonly FillingsController _controller;

    public FillingsControllerTests()
    {
        _fillingServiceMock = new Mock<IFillingService>();
        Mock<ILogger<FillingsController>> loggerMock = new();
        _controller = new FillingsController(_fillingServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithFillings()
    {
        var fillings = new List<FillingDto>
        {
            new FillingDto { Id = 1, Name = "Сливочный" },
            new FillingDto { Id = 2, Name = "Шоколадный" }
        };
        _fillingServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(fillings);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(fillings, okResult.Value);
    }

    [Fact]
    public async Task GetAvailable_ReturnsOkWithList()
    {
        var availableFillings = new List<FillingDto>
        {
            new FillingDto { Id = 1, Name = "Сливочный" }
        };
        _fillingServiceMock.Setup(s => s.GetAvailableAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(availableFillings);

        var result = await _controller.GetAvailable(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(availableFillings, okResult.Value);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var filling = new FillingDto { Id = 1, Name = "Сливочный" };
        _fillingServiceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(filling);

        var result = await _controller.GetById(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(filling, okResult.Value);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _fillingServiceMock.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((FillingDto?)null);

        var result = await _controller.GetById(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetWithCakes_Valid_ReturnsOk()
    {
        var fillingWithCakes = new FillingWithCakesDto
        {
            Id = 1,
            Name = "Сливочный",
            Cakes = new List<CakeListDto>()
        };
        _fillingServiceMock.Setup(s => s.GetWithCakesAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(fillingWithCakes);

        var result = await _controller.GetWithCakes(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(fillingWithCakes, okResult.Value);
    }

    [Fact]
    public async Task GetWithCakes_NotFound_Returns404()
    {
        _fillingServiceMock.Setup(s => s.GetWithCakesAsync(99, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.GetWithCakes(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        var createDto = new CreateFillingDto { Name = "Новая начинка" };
        var created = new FillingDto { Id = 10, Name = "Новая начинка" };
        _fillingServiceMock.Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(created);

        var result = await _controller.Create(createDto, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(FillingsController.GetById), createdResult.ActionName);
        Assert.Equal(created, createdResult.Value);
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var updateDto = new UpdateFillingDto { Id = 2, Name = "Обновлённая" };

        var result = await _controller.Update(1, updateDto, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Id mismatch", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Update_ValidDto_ReturnsOk()
    {
        var updateDto = new UpdateFillingDto { Id = 1, Name = "Обновлённая" };
        var updated = new FillingDto { Id = 1, Name = "Обновлённая" };
        _fillingServiceMock.Setup(s => s.UpdateAsync(updateDto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(updated);

        var result = await _controller.Update(1, updateDto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updated, okResult.Value);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var updateDto = new UpdateFillingDto { Id = 1, Name = "Обновлённая" };
        _fillingServiceMock.Setup(s => s.UpdateAsync(updateDto, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Update(1, updateDto, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_InvalidOperation_ReturnsBadRequest()
    {
        var updateDto = new UpdateFillingDto { Id = 1, Name = "Дубликат" };
        _fillingServiceMock.Setup(s => s.UpdateAsync(updateDto, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new InvalidOperationException("Дубликат названия"));

        var result = await _controller.Update(1, updateDto, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        _fillingServiceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        _fillingServiceMock.Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Delete(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_InvalidOperation_ReturnsBadRequest()
    {
        _fillingServiceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new InvalidOperationException("Нельзя удалить, используется в тортах"));

        var result = await _controller.Delete(1, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Нельзя удалить", badRequest.Value?.ToString());
    }
}