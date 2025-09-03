using Api.Dtos.ClientDtos;

namespace Api.Services
{
    public class ClientStorageService()
    {
        private readonly HttpClient _client = new();

        public async Task<bool> CreateUserOnClient(string url, UserDto user)
        {
            var response = await _client.PostAsJsonAsync<UserDto>(url, user);
            if (response.IsSuccessStatusCode is false)
                return false;

            return true;
        }
    }
}
