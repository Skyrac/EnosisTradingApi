using System;

namespace API.Models
{
    public class StakeEntity : CosmoModel
    {
        public string user { get; set; }
        public DateTime date { get; set; }
        public float points { get; set; }

    }
}
