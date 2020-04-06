namespace API.Models
{
    public class UserEntity : CosmoModel
    {
        public string email { get; set; }
        public string password { get; set; }
        public string handy { get; set; }
        public bool two_way_auth { get; set; }
        public string paypal { get; set; }
        public string login_ip { get; set; }
        public float bound_points { get; set; }
        public float free_points { get; set; }
    }
}
