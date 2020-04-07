using System;

namespace API.Models.UserModels
{
    public class UserLoginResponseModel : ResponseModel
    {
        public string user_id { get; set; }
        public Guid user_token { get; set; }
    }
}
