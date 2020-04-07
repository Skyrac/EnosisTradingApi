namespace API.Models
{
    public class UserTaskEntity : CosmoModel
    {
        /// <summary>
        /// user_id
        /// </summary>
        public string user { get; set; }
        /// <summary>
        /// click_id
        /// </summary>
        public string click { get; set; }
        /// <summary>
        /// should be user_id?
        /// </summary>
        public string affiliate { get; set; }
        public string program { get; set; }
        public string status { get; set; }
        public float payout { get; set; }

        public UserTaskEntity(string user_id, string click_id, string aff_id, string prog_id, string status, float payout)
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
