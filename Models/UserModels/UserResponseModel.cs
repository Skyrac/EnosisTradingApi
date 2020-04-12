using System;

namespace API.Models.UserModels
{
    public class UserResponseModel : ResponseModel
    {
        public string email { get; set; }

        public string name { get; set; }

        public Guid user_token { get; set; }
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

        public static UserResponseModel FromEntity(UserEntity user, InfoStatus status)
        {
            return new UserResponseModel()
            {
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
                passive_time = user.passive_time
            };
        }
    }
}
