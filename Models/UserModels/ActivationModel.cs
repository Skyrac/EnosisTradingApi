using System;

namespace API.Models
{
    public class ActivationModel
    {
        public string user_id { get; set; }
        public Guid user_token { get; set; }
        public string activation_key { get; set; }
    }
}
