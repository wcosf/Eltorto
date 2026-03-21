using Eltorto.Domain.Entities;

namespace Eltorto.Application.Interfaces.Repositories;

public interface ITestimonialRepository : IRepository<Testimonial>
{
    Task<IReadOnlyList<Testimonial>> GetApprovedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Testimonial>> GetLatestAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Testimonial>> GetPagedApprovedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Testimonial>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetApprovedCountAsync(CancellationToken cancellationToken = default);
}