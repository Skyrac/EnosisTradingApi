using API.Utility;
using System;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UserEntity : CosmoModel
    {
        /// <summary>
        /// E-Mail must be Unique and verified
        /// Verification-Code: First Letter of E-Mail, First 2 Numbers of User_Id, Last Letter of Password 
        /// </summary>
        [Required]
        [EmailAddress]
        public string email { get; set; }

        public string name { get; set; }
        
        public string user_token { get; set; }
        public string password { get; set; }

        public string password_forgotten_key { get; set; }

        public string activation_key { get; set; }

        public bool is_active { get; set; } = false;

        [Phone]
        public string handy { get; set; }
        public bool two_way_auth { get; set; } = false;
        /// <summary>
        /// Paypal E-Mail address
        /// </summary>
        public string paypal { get; set; }
        public string login_ip { get; set; }
        /// <summary>
        /// Displays the amount of used bound points
        /// For the "usable bound points" we have to check all user mines collected points - used_bound_points
        /// </summary>
        public float used_bound_points { get; set; }
        public float free_points { get; set; }
        /// <summary>
        /// users own referal_id (=[R{user_id}A]
        /// </summary>
        public string referal_id { get; set; }
        /// <summary>
        /// user_id of the referrer
        /// </summary>
        public string referrer { get; set; }

        public DateTime passive_activation { get; set; }

        public DateTime last_check { get; set; }
        
        /// <summary>
        /// Remaining Time for passive farming
        /// </summary>
        public float passive_time { get; set; }

        public SystemLanguage language { get; set; }
    }
}
