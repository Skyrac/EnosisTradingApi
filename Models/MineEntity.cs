using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class MineEntity : CosmoModel
    {
        public int user_id { get; set; }
        public DateTime start_date { get; set; }
        public float power { get; set; }

    }
}
