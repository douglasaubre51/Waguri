using Api.Dtos.ClientDtos;

namespace Api.Services
{
    public class ClientStorageService(HttpClient client)
    {
        private readonly HttpClient _client = client;

        public async Task<bool> CreateUserOnClient(string url, UserDto user)
        {
            var response = await _client.PostAsJsonAsync<UserDto>(url, user);
            if (response.IsSuccessStatusCode is false)
                return false;

            return true;
        }
    }
}
