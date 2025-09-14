using Api.Data;
using Api.Models;

namespace Api.Repositories
{
    public class ClientRepository(ApplicationDbContext context)
    {
	private readonly ApplicationDbContext _context = context;

	public string? GetClientUrlById(string id)
	    => _context.Clients
	    .Where(i => i.ProjectId == id)
	    .Select(i => i.Url)
	    .SingleOrDefault();

	public string? GetApiUrlById(string id)
	    => _context.Clients
	    .Where(i => i.ProjectId == id)
	    .Select(x => x.ApiUrl)
	    .SingleOrDefault();

	public List<Client> GetAll()
	    => _context.Clients
	    .Select(e => e)
	    .ToList();


	public void Create(Client client)
	{
	    _context.Clients.Add(client);
	    Save();
	}

	public void RemoveByProjectId(String id){

	    var project = _context.Clients
	    .Where(
	    	e => e.ProjectId == id
	    )
	    .Single();

	    _context.Clients
	    .Remove(
	    	project
	    );
	    
	    Save();
	}

	public void Save()
	    => _context.SaveChanges();
    }
}
