using System;

namespace API.Models
{
    public class MineEntity : CosmoModel
    {
        public string user { get; set; }
        public DateTime start_date { get; set; }

        public DateTime last_check { get; set; }
        /// <summary>
        /// Mining Contracts with -717 are Lifetime Contracts
        /// </summary>
        public int remaining_time { get; set; } = -717;
        public float power { get; set; }

        public float mined_points { get; set; }

    }
}
