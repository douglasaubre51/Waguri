using Api.Data;

namespace Api.Repositories
{
    public class ClientRepository(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        public string? GetClientUrlById(string id)
            => _context.Clients
                .Where(i => i.ProjectId == id)
                .Select(i => i.Url).SingleOrDefault();
        public string? GetApiUrlById(string id)
            => _context.Clients
            .Where(i => i.ProjectId == id)
            .Select(x => x.ApiUrl).SingleOrDefault();
    }
}
