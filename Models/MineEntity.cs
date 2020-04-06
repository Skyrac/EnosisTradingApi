using System;

namespace API.Models
{
    public class MineEntity : CosmoModel
    {
        public string user { get; set; }
        public DateTime start_date { get; set; }
        public int remaining_time { get; set; }
        public float power { get; set; }

    }
}
