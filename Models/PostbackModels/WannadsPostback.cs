using Microsoft.AspNetCore.Mvc;

namespace API.Models
{
    public class WannadsPostback
    {
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string subId { get; set; }
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string campaign_id { get; set; }
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string payout { get; set; }
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string status { get; set; }
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string transId { get; set; }
    }
}
