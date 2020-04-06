namespace API.Models
{
    public class UserEntity : CosmoModel
    {
        /// <summary>
        /// E-Mail must be Unique and verified
        /// Verification-Code: First Letter of E-Mail, First 2 Numbers of User_Id, Last Letter of Password 
        /// </summary>
        public string email { get; set; }
        public string password { get; set; }
        public string handy { get; set; }
        public bool two_way_auth { get; set; }
        /// <summary>
        /// Paypal E-Mail address
        /// </summary>
        public string paypal { get; set; }
        public string login_ip { get; set; }
        public float bound_points { get; set; }
        public float free_points { get; set; }
        /// <summary>
        /// users own referal_id (=[R{user_id}A]
        /// </summary>
        public string referal_id { get; set; }
        /// <summary>
        /// user_id of the referrer
        /// </summary>
        public string referrer { get; set; }
    }
}
