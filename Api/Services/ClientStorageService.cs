using Api.Dtos.ClientDtos;

namespace Api.Services;

public class ClientStorageService()
{
    private readonly HttpClient _client = new();

    public async Task<bool> CreateUserOnClient(string url, UserDto user)
    {
        var response = await _client.PostAsJsonAsync<UserDto>(url, user);
        if (response.IsSuccessStatusCode is false)
        {
            Console.WriteLine("status code: " + response.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<bool> CreateUserOnClient(string url, CreateUser user)
    {
        var response = await _client.PostAsJsonAsync<CreateUser>(url, user);
        if (response.IsSuccessStatusCode is false)
        {
            Console.WriteLine("status code: " + response.StatusCode);
            var message = await response.Content.ReadAsStringAsync();
            Console.WriteLine("error message: " + message);

            return false;
        }

        return true;
    }
}
