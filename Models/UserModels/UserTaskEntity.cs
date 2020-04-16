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
        public string payout { get; set; }

        
        public UserTaskEntity() { }

        public UserTaskEntity(string click_id, string payout, string prog_id)
        {
            this.click = click_id;
            this.program = prog_id;
            this.payout = payout;
        }

        public UserTaskEntity(string click_id, string aff_id, string prog_id, string status, string payout)
        {
            this.click = click_id;
            this.affiliate = aff_id;
            this.program = prog_id;
            this.status = status;
            this.payout = payout;
        }
    }
}
