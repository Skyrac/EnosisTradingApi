﻿using API.Models;
using API.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("affiliate")]
    public class AffiliateLeadController : Controller
    {

        public ICosmosDatabase<LeadEntity> _leadDatabase;

        public AffiliateLeadController(ICosmosDatabase<LeadEntity> leadDatabase)
        {
            _leadDatabase = leadDatabase;
        }

        [HttpPut]
        public async Task<IActionResult> PutLeadsAsync([Required, FromBody] IEnumerable<LeadEntity> leads)
        {
            if (ModelState.IsValid)
            {
                foreach (var lead in leads)
                {
                    lead.country_code = lead.country_code.ToLower();
                    // ({0}.country_code = '{1}' AND {0}.name = '{2}')
                    var alreadyExist = await _leadDatabase.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.country_code = '{1}' AND {0}.name = '{2}' AND {0}.id != '{3}'", nameof(LeadEntity), lead.country_code, lead.name, lead.id));
                    if(alreadyExist != null)
                    {
                        continue;
                    }
                    var dbLead = await _leadDatabase.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.id = '{1}'", nameof(LeadEntity), lead.id));
                    if (dbLead == null) {
                        await _leadDatabase.AddItemAsync(lead);
                    } else
                    {
                        await _leadDatabase.UpdateItemAsync(lead.id, lead);
                    }
                }
                return Ok(new ResponseModel() { status = InfoStatus.Info });
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
        }

        [HttpPost]
        public async Task<IActionResult> AddLeadsAsync([Required, FromBody] IEnumerable<LeadEntity> leads)
        {
            if (ModelState.IsValid)
            {
                foreach (var lead in leads)
                {
                    lead.country_code = lead.country_code.ToLower();
                    if ((await _leadDatabase.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.country_code = '{1}' AND {0}.name = '{2}'", nameof(LeadEntity), lead.country_code, lead.name))) == null)
                    {
                        await _leadDatabase.AddItemAsync(lead);
                    }
                }
                return Ok(new ResponseModel() { status = InfoStatus.Info });
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
        }

        [HttpGet]
        public async Task<IActionResult> GetLeadsAsync()
        {
            var leads = await _leadDatabase.GetItemsAsync(string.Format("SELECT * FROM {0}", nameof(LeadEntity)));
            return Ok(leads.ToList());
        }

        [Route("country/{country-code}")]
        [HttpGet]
        public async Task<IActionResult> GetLeadsByCountryAsync([FromRoute(Name = "country-code")] string country)
        {
            country = country.ToLower();
            var leads = await _leadDatabase.GetItemsAsync(string.Format("SELECT * FROM {0} WHERE {0}.country_code = '{1}'", nameof(LeadEntity), country));
            return Ok(leads.ToList());
        }

        [Route("country/{country-code}/{category}")]
        [HttpGet]
        public async Task<IActionResult> GetLeadsByCountryAsync([FromRoute(Name = "country-code")] string country, Category category)
        {
            country = country.ToLower();
            var leads = await _leadDatabase.GetItemsAsync(string.Format("SELECT * FROM {0} WHERE {0}.country_code = '{1}' AND {0}.category = {2}", nameof(LeadEntity), country, (int) category));
            return Ok(leads.ToList());
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLeads([Required, FromBody] IEnumerable<string> leads)
        {
            foreach(var lead in leads)
            {
                await _leadDatabase.DeleteItemAsync(lead);
            }
            return Ok(leads.ToList());
        }

        [HttpGet]
        [Route("{name}/country/{country}")]
        public async Task<IActionResult> GetLeadAsync(string name, string country)
        {
            country = country.ToLower();
            var lead = await _leadDatabase.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.country_code = '{1}' AND {0}.name = '{2}'", nameof(LeadEntity), country, name));
            if (lead == null)
            {
                return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
            }
            return Ok(lead);
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetLeadAsync(string id)
        {
            var lead = await _leadDatabase.GetItemAsync(id);
            if(lead == null)
            {
                return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
            }
            return Ok(lead);
        }
    }
}