using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class LeadEntity : CosmoModel
    {
        [Required]
        public string language { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public LeadType type { get; set; }
        /// <summary>
        /// Immer von Min ausgehend
        /// </summary>
        [Required]
        public float commission { get; set; }
        [Required]
        public string devices { get; set; }
        [Required]
        public string countries { get; set; }
        [Required]
        [Url]
        public string url { get; set; }
        [Required]
        public string categories { get; set; }
    }

    public enum LeadType
    {
        CPL,
        CPS,
        CPA,
        PPI,
        COD
    }
}
