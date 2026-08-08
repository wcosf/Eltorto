using System.Linq.Expressions;
using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Application.Services;
using Eltorto.Domain.Abstractions;
using Eltorto.Domain.Entities;
using Eltorto.Domain.Repositories;

namespace Eltorto.Api.Tests.ServicesTests;

public class CakeServiceTests
{
    [Fact]
    public async Task BulkIncreasePriceAsync_WithoutCategory_UpdatesAllPricedCakes()
    {
        var cake1 = new Cake { Id = 1, Name = "A", CategorySlug = "classic", Price = 100m };
        var cake2 = new Cake { Id = 2, Name = "B", CategorySlug = "wedding", Price = 200m };
        var cakes = new List<Cake> { cake1, cake2 };

        var cakeRepo = new Mock<ICakeRepository>();
        cakeRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Cake, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cakes);
        cakeRepo.Setup(r => r.UpdateAsync(It.IsAny<Cake>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var categoryRepo = new Mock<ICategoryRepository>();
        categoryRepo.Setup(r => r.ExistsBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(u => u.Cakes).Returns(cakeRepo.Object);
        unitOfWork.SetupGet(u => u.Categories).Returns(categoryRepo.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        unitOfWork.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>((operation, _) => operation());

        var service = new CakeService(unitOfWork.Object, Mock.Of<IMapper>());
        var dto = new BulkPriceIncreaseDto { CategorySlug = null, PercentIncrease = 10 };

        var updatedCount = await service.BulkIncreasePriceAsync(dto, CancellationToken.None);

        Assert.Equal(2, updatedCount);
        categoryRepo.Verify(r => r.ExistsBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(110m, cake1.Price);
        Assert.Equal(220m, cake2.Price);
    }
}
