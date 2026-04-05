using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;

namespace Eltorto.Application.Services;

public class SliderService : ISliderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SliderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SliderItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sliderItem = await _unitOfWork.Sliders.GetByIdAsync(id, cancellationToken);
        return sliderItem != null ? _mapper.Map<SliderItemDto>(sliderItem) : null;
    }

    public async Task<IReadOnlyList<SliderItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sliderItems = await _unitOfWork.Sliders.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<SliderItemDto>>(sliderItems);
    }

    public async Task<IReadOnlyList<SliderItemDto>> GetOrderedAsync(CancellationToken cancellationToken = default)
    {
        var sliderItems = await _unitOfWork.Sliders.GetOrderedAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<SliderItemDto>>(sliderItems);
    }

    public async Task<SliderItemDto> CreateAsync(CreateSliderItemDto createDto, CancellationToken cancellationToken = default)
    {
        var sliderItem = _mapper.Map<SliderItem>(createDto);

        await _unitOfWork.Sliders.AddAsync(sliderItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SliderItemDto>(sliderItem);
    }

    public async Task<SliderItemDto> UpdateAsync(UpdateSliderItemDto updateDto, CancellationToken cancellationToken = default)
    {
        var existingSliderItem = await _unitOfWork.Sliders.GetByIdAsync(updateDto.Id, cancellationToken);
        if (existingSliderItem == null)
        {
            throw new KeyNotFoundException($"Slider item with id {updateDto.Id} not found");
        }

        _mapper.Map(updateDto, existingSliderItem);
        await _unitOfWork.Sliders.UpdateAsync(existingSliderItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SliderItemDto>(existingSliderItem);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var sliderItem = await _unitOfWork.Sliders.GetByIdAsync(id, cancellationToken);
        if (sliderItem == null)
        {
            throw new KeyNotFoundException($"Slider item with id {id} not found");
        }

        await _unitOfWork.Sliders.DeleteAsync(sliderItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderAsync(List<int> orderedIds, CancellationToken cancellationToken = default)
    {
        var allItems = await _unitOfWork.Sliders.GetAllAsync(cancellationToken);
        var itemsDict = allItems.ToDictionary(i => i.Id);

        for (int i = 0; i < orderedIds.Count; i++)
        {
            if (itemsDict.TryGetValue(orderedIds[i], out var item))
            {
                item.SortOrder = i;
                await _unitOfWork.Sliders.UpdateAsync(item, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}