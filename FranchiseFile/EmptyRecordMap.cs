namespace MaddenFranchise;

public readonly record struct EmptyRecordEntry(int? Previous, int Next);

/// <summary>
/// Minimal insertion-order-preserving int-keyed map, standing in for the JS `Map` used for
/// FranchiseFileTable's empty-record free list - order matters there (`Array.from(map).pop()`
/// grabs the most-recently-inserted entry), which a plain Dictionary doesn't guarantee.
/// </summary>
public sealed class EmptyRecordMap
{
    private readonly Dictionary<int, EmptyRecordEntry> _map = new();
    private readonly List<int> _order = new();

    public int Count => _map.Count;

    public EmptyRecordEntry? Get(int key) => _map.TryGetValue(key, out var v) ? v : null;

    public void Set(int key, EmptyRecordEntry value)
    {
        if (!_map.ContainsKey(key))
        {
            _order.Add(key);
        }
        _map[key] = value;
    }

    public void Delete(int key)
    {
        if (_map.Remove(key))
        {
            _order.Remove(key);
        }
    }

    public (int Key, EmptyRecordEntry Value)? Last()
    {
        if (_order.Count == 0)
        {
            return null;
        }
        int k = _order[_order.Count - 1];
        return (k, _map[k]);
    }
}
