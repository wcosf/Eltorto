using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Domain.Abstractions;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Eltorto.Application.Services;

public class TestimonialService : ITestimonialService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TestimonialService> _logger;

    public TestimonialService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<TestimonialService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<TestimonialDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(id, cancellationToken);
        return testimonial != null ? _mapper.Map<TestimonialDto>(testimonial) : null;
    }

    public async Task<IReadOnlyList<TestimonialListDto>> GetApprovedAsync(CancellationToken cancellationToken = default)
    {
        var testimonials = await _unitOfWork.Testimonials.GetApprovedAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TestimonialListDto>>(testimonials);
    }

    public async Task<IReadOnlyList<TestimonialListDto>> GetLatestAsync(int count, CancellationToken cancellationToken = default)
    {
        var testimonials = await _unitOfWork.Testimonials.GetLatestAsync(count, cancellationToken);
        return _mapper.Map<IReadOnlyList<TestimonialListDto>>(testimonials);
    }

    public async Task<IReadOnlyList<TestimonialListDto>> GetForHomePageAsync(int count, CancellationToken cancellationToken = default)
    {
        var testimonials = await _unitOfWork.Testimonials.GetForHomePageAsync(count, cancellationToken);
        return _mapper.Map<IReadOnlyList<TestimonialListDto>>(testimonials);
    }

    public async Task<PagedResultDto<TestimonialListDto>> GetPagedApprovedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var testimonials = await _unitOfWork.Testimonials.GetPagedApprovedAsync(page, pageSize, cancellationToken);
        var totalCount = await _unitOfWork.Testimonials.GetApprovedCountAsync(cancellationToken);

        return new PagedResultDto<TestimonialListDto>
        {
            Items = _mapper.Map<List<TestimonialListDto>>(testimonials),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<TestimonialListDto>> GetPagedAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var testimonials = await _unitOfWork.Testimonials.GetPagedAsync(page, pageSize, cancellationToken);
        var totalCount = await _unitOfWork.Testimonials.CountAsync(cancellationToken);

        return new PagedResultDto<TestimonialListDto>
        {
            Items = _mapper.Map<List<TestimonialListDto>>(testimonials),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TestimonialDto> CreateAsync(CreateTestimonialDto createDto, CancellationToken cancellationToken = default)
    {
        var testimonial = _mapper.Map<Testimonial>(createDto);

        await _unitOfWork.Testimonials.AddAsync(testimonial, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = _mapper.Map<TestimonialDto>(testimonial);

        using (ExecutionContext.SuppressFlow())
        {
            _ = Task.Run(() => NotifyReviewCreatedAsync(result));
        }

        return result;
    }

    private async Task NotifyReviewCreatedAsync(TestimonialDto testimonial)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var contact = await serviceProvider.GetRequiredService<IContactSettingsService>().GetAsync();
            var toEmail = contact?.Email;
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            try
            {
                var notifier = serviceProvider.GetRequiredService<IReviewNotifier>();
                await notifier.SendReviewCreatedAsync(testimonial, toEmail, cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[EMAIL] Review notification send was canceled (timeout) for review {ReviewId}", testimonial.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Failed to send review notification for review {ReviewId}", testimonial.Id);
        }
    }

    public async Task<TestimonialDto> UpdateAsync(UpdateTestimonialDto updateDto, CancellationToken cancellationToken = default)
    {
        var existingTestimonial = await _unitOfWork.Testimonials.GetByIdAsync(updateDto.Id, cancellationToken);
        if (existingTestimonial == null)
        {
            throw new KeyNotFoundException($"Testimonial with id {updateDto.Id} not found");
        }

        _mapper.Map(updateDto, existingTestimonial);
        await _unitOfWork.Testimonials.UpdateAsync(existingTestimonial, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TestimonialDto>(existingTestimonial);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(id, cancellationToken);
        if (testimonial == null)
        {
            throw new KeyNotFoundException($"Testimonial with id {id} not found");
        }

        await _unitOfWork.Testimonials.DeleteAsync(testimonial, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<TestimonialDto> ApproveAsync(int id, ApproveTestimonialDto approveDto, CancellationToken cancellationToken = default)
    {
        var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(id, cancellationToken);
        if (testimonial == null)
        {
            throw new KeyNotFoundException($"Testimonial with id {id} not found");
        }

        testimonial.IsApproved = approveDto.IsApproved;
        await _unitOfWork.Testimonials.UpdateAsync(testimonial, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TestimonialDto>(testimonial);
    }

    public async Task<TestimonialDto> AddResponseAsync(int id, string response, CancellationToken cancellationToken = default)
    {
        var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(id, cancellationToken);
        if (testimonial == null)
        {
            throw new KeyNotFoundException($"Testimonial with id {id} not found");
        }

        testimonial.Response = response;
        await _unitOfWork.Testimonials.UpdateAsync(testimonial, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TestimonialDto>(testimonial);
    }
}
