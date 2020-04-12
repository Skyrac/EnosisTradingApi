using API.Models;
using API.Repository;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private ICosmosDatabase<MineEntity> _miningContext;
        private IMine _miner;

        public static List<PostbackEntity> posts = new List<PostbackEntity>();

        private readonly ILogger<PostbackController> _logger;

        public PostbackController(ILogger<PostbackController> logger, ICosmosDatabase<UserTaskEntity> userTaskContext, ICosmosDatabase<UserEntity> userContext, ICosmosDatabase<MineEntity> miningContext, IMine miner)
        {
            _logger = logger;
            _userTaskContext = userTaskContext;
            _userContext = userContext;
            _miner = miner;
            _miningContext = miningContext;
        }

        [Route("my-lead")]
        [HttpGet()]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Get([Required, FromQuery] PostbackEntity entity)
        {
            if(ModelState.IsValid)
            {
                await _userTaskContext.AddItemAsync(new UserTaskEntity(null, entity.click_id, entity.aff_id, entity.p_id, entity.status, entity.payout));
                var val = await _userTaskContext.GetItemAsync("1");
                return Ok(new ResponseModel() { status = InfoStatus.Info });
            }

            return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
        }
    }
}
