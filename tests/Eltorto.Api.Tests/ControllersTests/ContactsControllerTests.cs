namespace Eltorto.Api.Tests.Controllers;

public class ContactsControllerTests
{
    private readonly Mock<IContactSettingsService> _serviceMock;
    private readonly ContactsController _controller;

    public ContactsControllerTests()
    {
        _serviceMock = new Mock<IContactSettingsService>();
        var loggerMock = new Mock<ILogger<ContactsController>>();
        _controller = new ContactsController(_serviceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsSettings()
    {
        var dto = new ContactSettingsDto { Phone = "123" };
        _serviceMock.Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(dto);
        var result = await _controller.Get(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task Get_NotFound()
    {
        _serviceMock.Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync((ContactSettingsDto?)null);
        var result = await _controller.Get(CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Ok()
    {
        var updateDto = new UpdateContactSettingsDto { Phone = "456" };
        var updated = new ContactSettingsDto { Phone = "456" };
        _serviceMock.Setup(s => s.UpdateAsync(updateDto, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(updated);
        var result = await _controller.Update(updateDto, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }
}