using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Domain.Abstractions;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;
using System.Text.RegularExpressions;

namespace Eltorto.Application.Services;

public partial class CakeService : ICakeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly BulkPriceHistoryService _bulkPriceHistory;

    public CakeService(IUnitOfWork unitOfWork, IMapper mapper, BulkPriceHistoryService bulkPriceHistory)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _bulkPriceHistory = bulkPriceHistory;
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

    public async Task<PagedResultDto<CakeListDto>> GetPagedAsync(int page, int pageSize, string? category = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var cakes = await _unitOfWork.Cakes.GetPagedAsync(page, pageSize, category, search, cancellationToken);
        var totalCount = await _unitOfWork.Cakes.GetCountAsync(category, search, cancellationToken);

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

    public async Task UpdateImageUrlAsync(int id, string imageUrl, CancellationToken cancellationToken)
    {
        var cake = await _unitOfWork.Cakes.GetByIdAsync(id, cancellationToken);
        if (cake == null)
            throw new KeyNotFoundException($"Cake with id {id} not found");

        cake.ImageUrl = imageUrl;
        await _unitOfWork.Cakes.UpdateAsync(cake, cancellationToken);
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

    public async Task<string> GetNextCakeNameAsync(CancellationToken cancellationToken = default)
    {
        var names = await _unitOfWork.Cakes.GetAllNamesAsync(cancellationToken);

        var maxNumber = 0;
        foreach (var name in names)
        {
            var match = CakeNameNumberRegex().Match(name);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var number) && number > maxNumber)
            {
                maxNumber = number;
            }
        }

        return $"Торт № {maxNumber + 1}";
    }

    public async Task<int> BulkIncreasePriceAsync(BulkPriceIncreaseDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.PercentChange <= -100 || dto.PercentChange == 0 || dto.PercentChange > 500)
        {
            throw new InvalidOperationException("Процент изменения должен быть больше -100, не равен 0 и не превышать 500");
        }

        if (!string.IsNullOrEmpty(dto.CategorySlug))
        {
            var categoryExists = await _unitOfWork.Categories.ExistsBySlugAsync(dto.CategorySlug, cancellationToken);
            if (!categoryExists)
            {
                throw new KeyNotFoundException($"Category with slug '{dto.CategorySlug}' does not exist");
            }
        }

        var cakes = await _unitOfWork.Cakes.FindAsync(
            c => c.Price > 0 && (string.IsNullOrEmpty(dto.CategorySlug) || c.CategorySlug == dto.CategorySlug),
            cancellationToken);

        if (cakes.Count == 0)
        {
            return 0;
        }

        var factor = 1 + dto.PercentChange / 100m;
        var entries = new List<BulkPriceChangeEntry>(cakes.Count);

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            foreach (var cake in cakes)
            {
                var oldPrice = cake.Price!.Value;
                var newPrice = Math.Ceiling(oldPrice * factor);
                if (newPrice <= 0)
                {
                    newPrice = 0.01m;
                }

                entries.Add(new BulkPriceChangeEntry(cake.Id, oldPrice, newPrice));
                cake.Price = newPrice;
                await _unitOfWork.Cakes.UpdateAsync(cake, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        _bulkPriceHistory.Save(new BulkPriceChangeRecord(dto.CategorySlug, dto.PercentChange, entries));

        return cakes.Count;
    }

    public async Task<int> UndoLastBulkPriceChangeAsync(CancellationToken cancellationToken = default)
    {
        var record = _bulkPriceHistory.Peek();
        if (record == null)
        {
            return 0;
        }

        var ids = record.Entries.Select(e => e.CakeId).ToList();
        var cakes = await _unitOfWork.Cakes.FindAsync(c => ids.Contains(c.Id), cancellationToken);

        if (cakes.Count == 0)
        {
            _bulkPriceHistory.ClearIf(record);
            return 0;
        }

        var oldPricesById = record.Entries.ToDictionary(e => e.CakeId, e => e.OldPrice);

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            foreach (var cake in cakes)
            {
                cake.Price = oldPricesById[cake.Id];
                await _unitOfWork.Cakes.UpdateAsync(cake, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        _bulkPriceHistory.ClearIf(record);

        return cakes.Count;
    }

    public Task<bool> CanUndoBulkPriceChangeAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_bulkPriceHistory.HasChange);
    }

    [GeneratedRegex(@"^(?:Торт[а]?|Фото|Пирожные)\s*[№N#]?\s*(\d+)")]
    private static partial Regex CakeNameNumberRegex();
}
