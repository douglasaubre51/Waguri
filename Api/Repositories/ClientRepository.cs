using Api.Data;

namespace Api.Repositories
{
    public class ClientRepository(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        public string? GetClientUrlById(string id)
        {
            var url = _context.Clients
                .Where(i => i.ProjectId == id)
                .Select(i => i.Url).SingleOrDefault();

            Console.WriteLine("project url: " + url);

            return url;
        }
    }
}
