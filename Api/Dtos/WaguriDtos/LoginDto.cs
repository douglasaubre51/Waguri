using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.WaguriDtos
{
    public class LoginDto
    {
        [Required]
        public string EmailId { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
