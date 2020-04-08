using System;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class ActivationModel
    {
        [Required]
        public string user_id { get; set; }
        [Required]
        public Guid user_token { get; set; }
        [Required, StringLength(4)]
        public string activation_key { get; set; }
    }
}
