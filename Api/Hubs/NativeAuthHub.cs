namespace Api.Hubs;

public class NativeAuthHub(ConnectionStorageService connectionStorage) : Hub
{
    private readonly ConnectionStorageService _connectionStorage = connectionStorage;


    public override async Task OnConnectedAsync()
    {
	string connId = Context.ConnectionId;
	Console.WriteLine("client connected: "+connId);

	await GreetClient(connId);
    }

    public async Task GreetClient(string conn_id)
    {
	Console.WriteLine("sending greeting to client ... ");

	await Clients.All.SendAsync(
		"GetGreeting",
		"Wait till everyone joined!"
		);

	await Clients.Client(conn_id).SendAsync(
		"GetGreeting",
		"hola"
		);
    }

    public async Task CallGetClientGuid()
    {
	Console.WriteLine("storing clientguid");
	var tokenSource = new CancellationTokenSource();
	var cancellationToken = tokenSource.Token;

	var clientGuid = await Clients.Caller.InvokeAsync<string>("GetClientGuid",cancellationToken);
	Console.WriteLine($"connId: {Context.ConnectionId}\nclientGuid: {clientGuid}");

	_connectionStorage.Add(clientGuid,Context.ConnectionId);
    }
}
