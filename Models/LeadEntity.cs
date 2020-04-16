using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class LeadEntity : CosmoModel
    {
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
        ///In Punkten
        public float commission { get; set; }
        //Two Letter ISO Region Code
        [Required]
        public string country_code { get; set; }
        [Required]
        [Url]
        public string url { get; set; }
        [Required]
        public Category category { get; set; }
        [Required]
        public string program_id { get; set; }
    }
    
    public enum LeadType
    {
        CPL,
        CPS,
        CPA,
        PPI,
        COD
    }

    public enum Category
    {
        Default,
        Recommended
    }
}
