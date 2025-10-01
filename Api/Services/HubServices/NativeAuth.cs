namespace Api.Services.HubServices;

public class NativeAuth(
	IHubContext<NativeAuthHub> hub,
	ConnectionStorageService connectionStorage
	)
{
    private readonly IHubContext<NativeAuthHub> _hub = hub;
    private readonly ConnectionStorageService _connectionStorage = connectionStorage;


    public void SendClientLoginAccess(string guid, string userId)
    {
	var connId = _connectionStorage.GetConnection(guid);
	if(connId is null)
	{
	    Console.WriteLine("connection does not exists!");
	    return;
	}

	_hub.Clients.Client(connId)
	    .SendAsync("LoginSuccess",userId);
    }

}
