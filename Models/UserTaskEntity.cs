using Newtonsoft.Json;

namespace API.Models
{
    public class UserTaskEntity : CosmoModel
    {
        public int user { get; set; }
        public string click { get; set; }
        public string affiliate { get; set; }
        public string program { get; set; }
        public string status { get; set; }

        public float payout { get; set; }

        public UserTaskEntity(int user_id, string click_id, string aff_id, string prog_id, string status, float payout)
        {
            this.user = user_id;
            this.click = click_id;
            this.affiliate = aff_id;
            this.program = prog_id;
            this.status = status;
            this.payout = payout;
        }
    }
}
