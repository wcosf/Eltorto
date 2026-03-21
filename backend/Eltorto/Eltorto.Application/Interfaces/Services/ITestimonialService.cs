using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface ITestimonialService
{
    Task<TestimonialDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestimonialListDto>> GetApprovedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestimonialListDto>> GetLatestAsync(int count, CancellationToken cancellationToken = default);
    Task<PagedResultDto<TestimonialListDto>> GetPagedApprovedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResultDto<TestimonialListDto>> GetPagedAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<TestimonialDto> CreateAsync(CreateTestimonialDto createDto, CancellationToken cancellationToken = default);
    Task<TestimonialDto> UpdateAsync(UpdateTestimonialDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<TestimonialDto> ApproveAsync(int id, CancellationToken cancellationToken = default);
    Task<TestimonialDto> AddResponseAsync(int id, string response, CancellationToken cancellationToken = default);
}