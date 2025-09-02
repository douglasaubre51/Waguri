using Api.Data;

namespace Api.Repositories
{
    public class ClientRepository(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        public string? GetClientUrlById(string id)
        {
            return _context.Clients
                .Where(i => i.Id == id)
                .Select(i => i.Url).ToString();
        }
    }
}
