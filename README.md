# Waguri
For Native Clients

Login Api
Login is done by sending a GET request to api_endpoint:1 and connecting to NativeClient signalR hub.

1. HttpGet: "Login/{projectId}/{clientGuid}"

IMPORTANT:
For saving new WAGURI account to the Client DB, add a POST method on client:
2. HttpPost: "<base_client_url>/api/user/create"
with DTO(payload):
<string>Email
<string>FirstName
<string>LastName

SignalR hub connection url: "<base_url>/native-auth"
SignalR hub methods:
GetGreeting(string) method should be implemented in client side
<string>GetClientGuid method should be implemented in client side

