# Waguri
For Native Clients

Login Api<br>
Login is done by sending a GET request to api_endpoint:1 and connecting to NativeClient signalR hub.

1. HttpGet: "Login/{projectId}/{clientGuid}"

IMPORTANT:<br>
For saving new WAGURI account to the Client DB, add a POST method on client:<br>
2. HttpPost: "<base_client_url>/api/user/create"<br>
with DTO(payload):<br>
<string>Email<br>
<string>FirstName<br>
<string>LastName<br>

SignalR hub connection url: "<base_url>/native-auth"<br>
SignalR hub methods:<br>
GetGreeting(string) method should be implemented in client side<br>
<string>GetClientGuid method should be implemented in client side<br>

