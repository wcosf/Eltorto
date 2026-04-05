using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;

namespace Eltorto.Application.Services;

public class TestimonialService : ITestimonialService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TestimonialService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        return _mapper.Map<TestimonialDto>(testimonial);
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

    public async Task<TestimonialDto> ApproveAsync(int id, CancellationToken cancellationToken = default)
    {
        var testimonial = await _unitOfWork.Testimonials.GetByIdAsync(id, cancellationToken);
        if (testimonial == null)
        {
            throw new KeyNotFoundException($"Testimonial with id {id} not found");
        }

        testimonial.IsApproved = true;
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