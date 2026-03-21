using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;

namespace Eltorto.Application.Services;

public class CakeService : ICakeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CakeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CakeDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cake = await _unitOfWork.Cakes.GetByIdAsync(id, cancellationToken);
        return cake != null ? _mapper.Map<CakeDetailDto>(cake) : null;
    }

    public async Task<IReadOnlyList<CakeListDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cakes = await _unitOfWork.Cakes.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CakeListDto>>(cakes);
    }

    public async Task<IReadOnlyList<CakeListDto>> GetByCategoryAsync(string categorySlug, CancellationToken cancellationToken = default)
    {
        var cakes = await _unitOfWork.Cakes.GetByCategoryAsync(categorySlug, cancellationToken);
        return _mapper.Map<IReadOnlyList<CakeListDto>>(cakes);
    }

    public async Task<IReadOnlyList<CakeListDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default)
    {
        var cakes = await _unitOfWork.Cakes.GetFeaturedAsync(count, cancellationToken);
        return _mapper.Map<IReadOnlyList<CakeListDto>>(cakes);
    }

    public async Task<IReadOnlyList<CakeListDto>> GetByFillingAsync(int fillingId, CancellationToken cancellationToken = default)
    {
        var cakes = await _unitOfWork.Cakes.GetByFillingAsync(fillingId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CakeListDto>>(cakes);
    }

    public async Task<PagedResultDto<CakeListDto>> GetPagedAsync(int page, int pageSize, string? category = null, CancellationToken cancellationToken = default)
    {
        var cakes = await _unitOfWork.Cakes.GetPagedAsync(page, pageSize, category, cancellationToken);
        var totalCount = await _unitOfWork.Cakes.GetCountAsync(category, cancellationToken);

        return new PagedResultDto<CakeListDto>
        {
            Items = _mapper.Map<List<CakeListDto>>(cakes),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CakeDetailDto> CreateAsync(CreateCakeDto createDto, CancellationToken cancellationToken = default)
    {
        var cake = _mapper.Map<Cake>(createDto);

        var categoryExists = await _unitOfWork.Categories.ExistsBySlugAsync(createDto.CategorySlug, cancellationToken);
        if (!categoryExists)
        {
            throw new InvalidOperationException($"Category with slug '{createDto.CategorySlug}' does not exist");
        }

        if (createDto.FillingId.HasValue)
        {
            var fillingExists = await _unitOfWork.Fillings.ExistsAsync(f => f.Id == createDto.FillingId.Value, cancellationToken);
            if (!fillingExists)
            {
                throw new InvalidOperationException($"Filling with id {createDto.FillingId} does not exist");
            }
        }

        await _unitOfWork.Cakes.AddAsync(cake, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CakeDetailDto>(cake);
    }

    public async Task<CakeDetailDto> UpdateAsync(UpdateCakeDto updateDto, CancellationToken cancellationToken = default)
    {
        var existingCake = await _unitOfWork.Cakes.GetByIdAsync(updateDto.Id, cancellationToken);
        if (existingCake == null)
        {
            throw new KeyNotFoundException($"Cake with id {updateDto.Id} not found");
        }

        _mapper.Map(updateDto, existingCake);

        var categoryExists = await _unitOfWork.Categories.ExistsBySlugAsync(updateDto.CategorySlug, cancellationToken);
        if (!categoryExists)
        {
            throw new InvalidOperationException($"Category with slug '{updateDto.CategorySlug}' does not exist");
        }

        await _unitOfWork.Cakes.UpdateAsync(existingCake, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CakeDetailDto>(existingCake);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cake = await _unitOfWork.Cakes.GetByIdAsync(id, cancellationToken);
        if (cake == null)
        {
            throw new KeyNotFoundException($"Cake with id {id} not found");
        }

        await _unitOfWork.Cakes.DeleteAsync(cake, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}