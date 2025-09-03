using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;

        public string ProjectId { get; set; } = string.Empty;
    }
}
