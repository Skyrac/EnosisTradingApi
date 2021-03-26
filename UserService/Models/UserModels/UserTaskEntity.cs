namespace API.Models
{
    public class UserTaskEntity : CosmoModel
    {        
        public string user { get; set; }
        /// <summary>
        /// click_id
        /// </summary>
        public string click { get; set; }

        public string program { get; set; }
        public string status { get; set; }
        public double payout { get; set; }
        public double reward { get; set; }
        public string wall { get; set; }
        public string trans_id { get; set; }
        public bool is_checked { get; set; }


        public UserTaskEntity() { }

        public UserTaskEntity(string click_id, double payout, string prog_id)
        {
            this.click = click_id;
            this.program = prog_id;
            this.payout = payout;
            this.reward = payout * 0.6;
        }

        public UserTaskEntity(string click_id, string user_id, string prog_id, string status, double payout, string wall, string transId) : this(click_id, payout, prog_id)
        {
            this.user = user_id;
            this.status = status;
            this.wall = wall;
            this.trans_id = transId;
        }
    }
}
