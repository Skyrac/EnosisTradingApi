using API.Models;
using API.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

        [HttpPost]
        public async Task<IActionResult> AddLeadAsync([Required, FromBody] LeadEntity lead)
        {
            if (ModelState.IsValid && (await _leadDatabase.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.language = '{1}' AND {0}.name = '{2}'", nameof(LeadEntity), lead.language, lead.name))) == null)
            {
                await _leadDatabase.AddItemAsync(lead);
                return Ok(new ResponseModel() { status = Status.Success });
            }
            return BadRequest(new ResponseModel() { status = Status.Failed });
        }

        [HttpGet]
        public async Task<IActionResult> GetLeadsAsync()
        {
            var leads = await _leadDatabase.GetItemsAsync(string.Format("SELECT * FROM {0}", nameof(LeadEntity)));
            return Ok(leads);
        }

        [HttpGet]
        [Route("{name}/{language}")]
        public async Task<IActionResult> GetLeadAsync(string name, string language)
        {
            var lead = await _leadDatabase.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.language = '{1}' AND {0}.name = '{2}'", nameof(LeadEntity), language, name));
            if (lead == null)
            {
                return BadRequest(new ResponseModel() { status = Status.Failed });
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
                return BadRequest(new ResponseModel() { status = Status.Failed });
            }
            return Ok(lead);
        }
    }
}