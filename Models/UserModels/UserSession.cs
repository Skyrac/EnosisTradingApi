namespace API.Models.UserModels
{
    public class UserSession
    {
        public string email { get; set; }
        public string token { get; set; }
        public string ip { get; set; }
    }
}
