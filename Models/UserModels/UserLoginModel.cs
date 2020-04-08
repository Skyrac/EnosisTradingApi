using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UserLoginModel
    {
        [Required]
        [MinLength(5), EmailAddress]
        public string email { get; set; }
        [Required]
        [MinLength(6), MaxLength(20)]
        public string password { get; set; }
        public string name { get; set; }
    }
}
