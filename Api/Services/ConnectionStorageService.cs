namespace Api.Services;

public class ConnectionStorageService
{
    private Dictionary<string, string> Connections = new();

    public void Add(string guid, string connId)
    => Connections.Add(guid, connId);

    public void Remove(string guid)
    => Connections.Remove(guid);

    public bool Exist(string guid)
    => Connections.ContainsKey(guid);

    public string GetConnection(string guid)
    {
        Connections.TryGetValue(guid, out string connId);
        return connId;
    }
}
