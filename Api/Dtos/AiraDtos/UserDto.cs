namespace Api.Dtos.AiraDtos
{
    public class UserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }

        public string ProjectId { get; set; } = string.Empty;
    }
}
