using API.Utility;
using System;

namespace API.Models.UserModels
{
    public class UserModel : ResponseModel
    {
        public string id { get; set; }
        public string email { get; set; }

        public string name { get; set; }

        public string user_token { get; set; }
        public bool is_active { get; set; } = false;
        public string handy { get; set; }
        public bool two_way_auth { get; set; } = false;
        /// <summary>
        /// Paypal E-Mail address
        /// </summary>
        public string paypal { get; set; }
        public float used_bound_points { get; set; }
        public float free_points { get; set; }
        public string referal_id { get; set; }
        public DateTime passive_activation { get; set; }
        public double passive_time { get; set; }
        public SystemLanguage language { get; set; }

        public float power { get; set; }

        public static UserModel FromEntity(UserEntity user, InfoStatus status = InfoStatus.Info)
        {
            return new UserModel()
            {
                id = user.id,
                status = status,
                email = user.email,
                name = user.name,
                user_token = user.user_token,
                is_active = user.is_active,
                handy = user.handy,
                two_way_auth = user.two_way_auth,
                paypal = user.paypal,
                used_bound_points = user.used_bound_points,
                free_points = user.free_points,
                referal_id = user.referal_id,
                passive_activation = user.passive_activation,
                passive_time = user.passive_time,
                language = user.language
            };
        }
        public static void Update(ref UserEntity user, UserModel model)
        {
            if (!string.IsNullOrEmpty(model.email))
            {
                user.email = model.email;
            }
            if (!string.IsNullOrEmpty(model.handy))
            {
                user.handy = model.handy;
            }
            if (model.two_way_auth)
            {
                user.two_way_auth = model.two_way_auth;
            }
        }
    }


}
