namespace Eltorto.Api.Tests.Controllers;

public class PagesControllerTests
{
    private readonly Mock<IPageService> _serviceMock;
    private readonly PagesController _controller;

    public PagesControllerTests()
    {
        _serviceMock = new Mock<IPageService>();
        var loggerMock = new Mock<ILogger<PagesController>>();
        _controller = new PagesController(_serviceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetBySlug_Ok()
    {
        _serviceMock.Setup(s => s.GetBySlugAsync("about", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new PageDto());
        var result = await _controller.GetBySlug("about", CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetBySlug_NotFound()
    {
        _serviceMock.Setup(s => s.GetBySlugAsync("missing", It.IsAny<CancellationToken>()))
                   .ReturnsAsync((PageDto?)null);
        var result = await _controller.GetBySlug("missing", CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_Ok()
    {
        var pageDto = new PageDto
        {
            Id = 1,
            Title = "О нас",
            Slug = "about"
        };
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageDto);

        var result = await _controller.GetById(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPage = Assert.IsType<PageDto>(okResult.Value);
        Assert.Equal(1, returnedPage.Id);
        Assert.Equal("about", returnedPage.Slug);
    }

    [Fact]
    public async Task GetBlocks_ReturnsBlocks()
    {
        var blocks = new List<ContentBlockDto> { new() };
        _serviceMock.Setup(s => s.GetPageContentBlocksAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(blocks);
        var result = await _controller.GetBlocks(1, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(blocks, ok.Value);
    }

    [Fact]
    public async Task AddBlock_Success_ReturnsCreated()
    {
        var createDto = new CreateContentBlockDto();
        var block = new ContentBlockDto { Id = 100 };
        _serviceMock.Setup(s => s.AddContentBlockAsync(1, createDto, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(block);
        var result = await _controller.AddBlock(1, createDto, CancellationToken.None);
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetBlockById), created.ActionName);
    }

    [Fact]
    public async Task AddBlock_PageNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.AddContentBlockAsync(99, It.IsAny<CreateContentBlockDto>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new KeyNotFoundException("Page not found"));
        var result = await _controller.AddBlock(99, new CreateContentBlockDto(), CancellationToken.None);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Page not found", notFound.Value?.ToString());
    }

    [Fact]
    public async Task UpdateBlock_IdMismatch_BadRequest()
    {
        var dto = new UpdateContentBlockDto { Id = 2 };
        var result = await _controller.UpdateBlock(1, dto, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetBlockById_PageNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((PageDto?)null);
        var result = await _controller.GetBlockById(10, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetBlockById_BlockNotFound_ReturnsNotFound()
    {
        var page = new PageDto
        {
            ContentBlocks = new List<ContentBlockDto> { new() { Id = 5 } }
        };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(page);
        var result = await _controller.GetBlockById(999, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ReorderBlocks_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.ReorderContentBlocksAsync(1, It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);
        var result = await _controller.ReorderBlocks(1, new List<int> { 1, 2 }, CancellationToken.None);
        Assert.IsType<NoContentResult>(result);
    }
}