using Microsoft.AspNetCore.Mvc;

namespace API.Models
{
    public class MyLeadPostback
    {
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string click_id { get; set; }

        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string aff_id { get; set; }

        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string transaction { get; set; }

        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string payout { get; set; }

        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string status { get; set; }


        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string ip { get; set; }


        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string p_id { get; set; }
    }
}
