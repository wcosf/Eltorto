namespace Eltorto.Application.Services;

public sealed record BulkPriceChangeEntry(int CakeId, decimal OldPrice, decimal NewPrice);

public sealed record BulkPriceChangeRecord(string? CategorySlug, decimal Percent, IReadOnlyList<BulkPriceChangeEntry> Entries);

public sealed class BulkPriceHistoryService
{
    private readonly object _lock = new();
    private BulkPriceChangeRecord? _lastChange;

    public bool HasChange
    {
        get
        {
            lock (_lock)
            {
                return _lastChange != null;
            }
        }
    }

    public void Save(BulkPriceChangeRecord record)
    {
        lock (_lock)
        {
            _lastChange = record;
        }
    }

    public BulkPriceChangeRecord? Peek()
    {
        lock (_lock)
        {
            return _lastChange;
        }
    }

    public void ClearIf(BulkPriceChangeRecord record)
    {
        lock (_lock)
        {
            if (ReferenceEquals(_lastChange, record))
            {
                _lastChange = null;
            }
        }
    }
}
