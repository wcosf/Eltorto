namespace Eltorto.Api.Tests.Controllers;

public class SliderControllerTests
{
    private readonly Mock<ISliderService> _sliderServiceMock;
    private readonly SliderController _controller;

    public SliderControllerTests()
    {
        _sliderServiceMock = new Mock<ISliderService>();
        Mock<ILogger<SliderController>> loggerMock = new();
        _controller = new SliderController(_sliderServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOrderedSlides()
    {
        var slides = new List<SliderItemDto>
        {
            new SliderItemDto { Id = 1, Title = "Слайд 1" },
            new SliderItemDto { Id = 2, Title = "Слайд 2" }
        };
        _sliderServiceMock.Setup(s => s.GetOrderedAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(slides);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(slides, okResult.Value);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var slide = new SliderItemDto { Id = 1, Title = "Слайд" };
        _sliderServiceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(slide);

        var result = await _controller.GetById(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(slide, okResult.Value);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _sliderServiceMock.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                         .ReturnsAsync((SliderItemDto?)null);

        var result = await _controller.GetById(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        var createDto = new CreateSliderItemDto { Title = "Новый слайд" };
        var created = new SliderItemDto { Id = 10, Title = "Новый слайд" };
        _sliderServiceMock.Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(created);

        var result = await _controller.Create(createDto, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(SliderController.GetById), createdResult.ActionName);
        Assert.Equal(created, createdResult.Value);
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var updateDto = new UpdateSliderItemDto { Id = 2, Title = "Обновлённый" };

        var result = await _controller.Update(1, updateDto, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Id mismatch", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Update_ValidDto_ReturnsOk()
    {
        var updateDto = new UpdateSliderItemDto { Id = 1, Title = "Обновлённый" };
        var updated = new SliderItemDto { Id = 1, Title = "Обновлённый" };
        _sliderServiceMock.Setup(s => s.UpdateAsync(updateDto, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(updated);

        var result = await _controller.Update(1, updateDto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updated, okResult.Value);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var updateDto = new UpdateSliderItemDto { Id = 1, Title = "Обновлённый" };
        _sliderServiceMock.Setup(s => s.UpdateAsync(updateDto, It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Update(1, updateDto, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        _sliderServiceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        _sliderServiceMock.Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Delete(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Reorder_ValidIds_ReturnsNoContent()
    {
        var orderedIds = new List<int> { 2, 1, 3 };
        _sliderServiceMock.Setup(s => s.ReorderAsync(orderedIds, It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

        var result = await _controller.Reorder(orderedIds, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}