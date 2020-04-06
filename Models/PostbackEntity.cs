using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Models
{
    public class PostbackEntity
    {
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        [BindRequired]
        public string click_id { get; set; }

        [FromQuery]
        [BindProperty(SupportsGet = true)]
        [BindRequired]
        public string aff_id { get; set; }

        [FromQuery]
        [BindProperty(SupportsGet = true)]
        [BindRequired]
        public string transaction { get; set; }

        [FromQuery]
        [BindProperty(SupportsGet = true)]
        [BindRequired]
        public float payout { get; set; }

        [FromQuery]
        [BindProperty(SupportsGet = true)]
        [BindRequired]
        public string status { get; set; }


        [FromQuery]
        [BindProperty(SupportsGet = true)]
        [BindRequired]
        public string ip { get; set; }


        [FromQuery]
        [BindProperty(SupportsGet = true)]
        [BindRequired]
        public string p_id { get; set; }
    }
}
