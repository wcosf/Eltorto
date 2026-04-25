namespace Eltorto.Api.Tests.Controllers;

public class TestimonialsControllerTests
{
    private readonly Mock<ITestimonialService> _serviceMock;
    private readonly TestimonialsController _controller;

    public TestimonialsControllerTests()
    {
        _serviceMock = new Mock<ITestimonialService>();
        var loggerMock = new Mock<ILogger<TestimonialsController>>();
        _controller = new TestimonialsController(_serviceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetApproved_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetApprovedAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<TestimonialListDto>());
        var result = await _controller.GetApproved(CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var dto = new CreateTestimonialDto();
        var created = new TestimonialDto { Id = 1 };
        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(created);
        var result = await _controller.Create(dto, CancellationToken.None);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
    }

    [Fact]
    public async Task Approve_NotFound_404()
    {
        _serviceMock.Setup(s => s.ApproveAsync(1, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new KeyNotFoundException());
        var result = await _controller.Approve(1, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddResponse_Success_Ok()
    {
        _serviceMock.Setup(s => s.AddResponseAsync(1, "Thanks", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new TestimonialDto());
        var result = await _controller.AddResponse(1, "Thanks", CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }
}