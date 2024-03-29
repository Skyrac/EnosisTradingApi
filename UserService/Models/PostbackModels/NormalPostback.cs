﻿using Microsoft.AspNetCore.Mvc;

namespace API.Models
{
    public class NormalPostback
    {
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string sub_id { get; set; }
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string program { get; set; }
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string payment { get; set; }
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string reward { get; set; }
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string status { get; set; }
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string wall { get; set; }
        [FromQuery]
        [BindProperty(SupportsGet = true)]
        public string transId { get; set; }
    }
}
