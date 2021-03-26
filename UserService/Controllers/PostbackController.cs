using API.Models;
using API.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("postback")]
    public class PostbackController : Controller
    {
        private ICosmosDatabase<UserTaskEntity> _userTaskContext;
        private ICosmosDatabase<UserEntity> _userContext;

        public static List<MyLeadPostback> posts = new List<MyLeadPostback>();

        private readonly ILogger<PostbackController> _logger;

        public PostbackController(ILogger<PostbackController> logger, ICosmosDatabase<UserTaskEntity> userTaskContext, ICosmosDatabase<UserEntity> userContext)
        {
            _logger = logger;
            _userTaskContext = userTaskContext;
            _userContext = userContext;
        }

        [Route("my-lead")]
        [HttpGet()]
        public async Task<IActionResult> PostbackMyLead([Required, FromQuery] MyLeadPostback entity)
        {
            if(ModelState.IsValid)
            {
                var payout = 0.0d;
                double.TryParse(entity.payout, out payout);
                await _userTaskContext.AddItemAsync(new UserTaskEntity(entity.click_id, entity.aff_id, entity.p_id, entity.status, payout, "my-lead", entity.transaction));
                var val = await _userTaskContext.GetItemAsync("1");
                return Ok();
            }

            return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
        }

        [Route("adklick")]
        [Route("")]
        [HttpGet()]
        [HttpPost()]
        [HttpPut()]
        public async Task<IActionResult> Postback([Required, FromQuery] NormalPostback entity)
        {
            if (ModelState.IsValid)
            {

                var payout = 0.0d;
                var status = "0";
                double.TryParse(entity.payment, out payout);
                if(!string.IsNullOrEmpty(entity.status))
                {
                    status = entity.status;
                }
                await _userTaskContext.AddItemAsync(new UserTaskEntity("", entity.sub_id, entity.program, entity.status, payout, entity.wall, entity.transId));
                return Ok("1");
            }

            return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
        }

        [Route("wannads")]
        [HttpGet()]
        public async Task<object> WannadsPostback([Required, FromQuery] WannadsPostback entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var task = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.transId = '{1}' AND {0}.wall = 'wannads'", nameof(UserTaskEntity), entity.transId));

                    if (task != null)
                    {
                        return "DUP";
                    }
                } catch (Exception ex)
                {
                    _logger.LogWarning(new EventId(), ex.Message);
                }

                var payout = 0.0d;
                var status = "0";
                double.TryParse(entity.payout, out payout);
                if (!string.IsNullOrEmpty(entity.status))
                {
                    status = entity.status;
                }
                await _userTaskContext.AddItemAsync(new UserTaskEntity("", entity.subId, entity.campaign_id, entity.status, payout, "wannads", entity.transId));
                return "OK";
            }

            return BadRequest();
        }
    }
}
