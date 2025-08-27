using Microsoft.AspNetCore.Identity;

namespace Api.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
    }
}
