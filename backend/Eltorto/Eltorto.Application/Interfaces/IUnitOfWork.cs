using Eltorto.Application.Interfaces.Repositories;

namespace Eltorto.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    ICakeRepository Cakes { get; }
    IFillingRepository Fillings { get; }
    ITestimonialRepository Testimonials { get; }
    IPageRepository Pages { get; }
    IOrderRepository Orders { get; }
    ISliderRepository Sliders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}