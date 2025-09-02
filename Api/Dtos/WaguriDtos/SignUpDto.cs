using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.WaguriDtos
{
    public class SignUpDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string EmailId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password, ErrorMessage = "not a valid password!")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "password mismatch!")]
        public string CheckPassword { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
