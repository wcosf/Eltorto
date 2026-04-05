using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;

namespace Eltorto.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        return category != null ? _mapper.Map<CategoryDto>(category) : null;
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetBySlugAsync(slug, cancellationToken);
        return category != null ? _mapper.Map<CategoryDto>(category) : null;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetOrderedAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CategoryDto>>(categories);
    }

    public async Task<CategoryWithCakesDto> GetWithCakesAsync(string slug, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetBySlugAsync(slug, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with slug '{slug}' not found");
        }

        var categoryDto = _mapper.Map<CategoryWithCakesDto>(category);
        var cakes = await _unitOfWork.Cakes.GetByCategoryAsync(slug, cancellationToken);
        categoryDto.Cakes = _mapper.Map<List<CakeListDto>>(cakes);

        return categoryDto;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createDto, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Categories.ExistsBySlugAsync(createDto.Slug, cancellationToken))
        {
            throw new InvalidOperationException($"Category with slug '{createDto.Slug}' already exists");
        }

        var category = _mapper.Map<Category>(createDto);
        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(UpdateCategoryDto updateDto, CancellationToken cancellationToken = default)
    {
        var existingCategory = await _unitOfWork.Categories.GetByIdAsync(updateDto.Id, cancellationToken);
        if (existingCategory == null)
        {
            throw new KeyNotFoundException($"Category with id {updateDto.Id} not found");
        }

        if (existingCategory.Slug != updateDto.Slug &&
            await _unitOfWork.Categories.ExistsBySlugAsync(updateDto.Slug, cancellationToken))
        {
            throw new InvalidOperationException($"Category with slug '{updateDto.Slug}' already exists");
        }

        _mapper.Map(updateDto, existingCategory);
        await _unitOfWork.Categories.UpdateAsync(existingCategory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(existingCategory);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with id {id} not found");
        }

        var cakesCount = await _unitOfWork.Cakes.CountAsync(c => c.CategorySlug == category.Slug, cancellationToken);
        if (cakesCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete category with {cakesCount} cakes. Remove cakes first.");
        }

        await _unitOfWork.Categories.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}