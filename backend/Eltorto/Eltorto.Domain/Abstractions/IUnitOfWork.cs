using Eltorto.Domain.Repositories;

namespace Eltorto.Domain.Abstractions;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    ICakeRepository Cakes { get; }
    IContactSettingsRepository ContactSettings { get; }
    IFillingRepository Fillings { get; }
    ITestimonialRepository Testimonials { get; }
    IPageRepository Pages { get; }
    IOrderRepository Orders { get; }
    ISliderRepository Sliders { get; }
    IContentBlockRepository ContentBlocks { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}