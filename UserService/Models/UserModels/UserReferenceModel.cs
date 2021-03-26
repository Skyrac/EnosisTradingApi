using System;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UserReferenceModel
    {
        [Required]
        public string user_id { get; set; }
        public string user_token { get; set; }
        [MaxLength(4)]
        public string activation_key { get; set; }
    }
}
