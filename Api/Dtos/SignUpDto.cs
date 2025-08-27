namespace Api.Dtos
{
    public record SignUpDto
    {
        public required string UserName { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Password { get; init; }

        public required string ProjectId { get; init; }
    }
}
