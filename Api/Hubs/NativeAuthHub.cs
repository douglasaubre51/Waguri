namespace Api.Hubs
{
	public class NativeAuthHub : Hub
	{
		public override async Task OnConnectedAsync()
		{
			string conn_id = Context.ConnectionId;
			Console.WriteLine("client connected: "+conn_id);

			await GreetClient(conn_id);
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
	}
}
