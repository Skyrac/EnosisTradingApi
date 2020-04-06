using API.Models;
using API.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace API.Controllers
{
    //ndkcdbu4nl3fTPZ9XbiLU6SG8B1BW0E4zT94m1dY2BVh0ljKf1tBijWYsQwV

    [ApiController]
    [Route("[controller]")]
    public class PostbackController : Controller
    {
        private ICosmoDatabase<UserTaskEntity> _userTaskContext;
        private ICosmoDatabase<UserEntity> _userContext;

        public static List<PostbackEntity> posts = new List<PostbackEntity>();

        private readonly ILogger<PostbackController> _logger;

        public PostbackController(ILogger<PostbackController> logger, ICosmoDatabase<UserTaskEntity> userTaskContext, ICosmoDatabase<UserEntity> userContext)
        {
            _logger = logger;
            _userTaskContext = userTaskContext;
            _userContext = userContext;
        }

        [HttpGet]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Get([Required, FromQuery] PostbackEntity entity)//[FromQuery] string clickid, [FromQuery] string affid, [FromQuery] string transaction, [FromQuery] string payout)
        {
            if(ModelState.IsValid)
            {
                await _userTaskContext.AddItemAsync(new UserTaskEntity(1, entity.click_id, entity.aff_id, entity.p_id, entity.status, entity.payout));
                await _userContext.AddItemAsync(new UserEntity());
                var val = await _userTaskContext.GetItemAsync("1");
                return Ok(new UserTaskEntity(1, entity.click_id, entity.aff_id, entity.p_id, entity.status, entity.payout));
            }

            return BadRequest();
        }
    }
}
