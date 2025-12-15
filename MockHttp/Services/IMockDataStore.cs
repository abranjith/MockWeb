namespace MockHttp.Services;

public interface IMockDataStore
{
    IDictionary<Guid, string> GetAll();
    Guid Add(string value);
    bool Update(Guid id, string value);
    bool Delete(Guid id);
}
