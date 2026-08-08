using Eltorto.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Eltorto.Application.Services;

public class CakePriceBackfillService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CakePriceBackfillService> _logger;

    public CakePriceBackfillService(IUnitOfWork unitOfWork, ILogger<CakePriceBackfillService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task BackfillAsync(CancellationToken cancellationToken = default)
    {
        var cakes = await _unitOfWork.Cakes.FindAsync(
            c => c.MinWeightKg == null && c.Price == null && c.Description != null && c.Description != "",
            cancellationToken);

        var updated = 0;
        foreach (var cake in cakes)
        {
            if (string.IsNullOrWhiteSpace(cake.Description))
                continue;

            cake.MinWeightKg = CakeDescriptionParser.ParseMinWeightKg(cake.Description);
            cake.Price = CakeDescriptionParser.ParsePrice(cake.Description);
            updated++;
        }

        if (updated == 0)
        {
            _logger.LogInformation("[BACKFILL] No cakes to backfill (MinWeightKg/Price)");
            return;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[BACKFILL] Backfilled MinWeightKg/Price for {Count} cakes", updated);
    }
}
