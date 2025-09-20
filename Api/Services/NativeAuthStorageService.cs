namespace Api.Services{

	public class NativeAuthStorageService{
		
		private Dictionary<string,string> Connections = new ();
		
		public void Set(string authId, string connectionId)
			=> Connections.Add(authId, connectionId);

		public string Get(string authId)
			=> Connections[authId];

		public bool KeyExists(string authId)
			=> Connections.ContainsKey(authId);

		public void Remove(string authId)
			=> Connections.Remove(authId);
	}
}
