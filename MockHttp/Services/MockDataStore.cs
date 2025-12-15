using System.Collections.Concurrent;

namespace MockHttp.Services;

public class MockDataStore : IMockDataStore
{
    private readonly ConcurrentDictionary<Guid, string> _items = new();

    public IDictionary<Guid, string> GetAll() => _items;

    public Guid Add(string value)
    {
        var id = Guid.NewGuid();
        _items[id] = value;
        return id;
    }

    public bool Update(Guid id, string value)
    {
        if (!_items.ContainsKey(id)) return false;
        _items[id] = value;
        return true;
    }

    public bool Delete(Guid id) => _items.TryRemove(id, out _);
}
