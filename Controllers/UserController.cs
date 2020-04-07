using API.Models;
using API.Models.UserModels;
using API.Repository;
using API.Services;
using API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("users")]
    public class UserController : Controller
    {
        private ICosmosDatabase<UserTaskEntity> _userTaskContext;
        private ICosmosDatabase<UserEntity> _userContext;
        private ICosmosDatabase<MineEntity> _miningContext;
        private IMine _miner;

        public static List<PostbackEntity> posts = new List<PostbackEntity>();

        private readonly ILogger<PostbackController> _logger;

        public UserController(ILogger<PostbackController> logger, ICosmosDatabase<UserTaskEntity> userTaskContext, ICosmosDatabase<UserEntity> userContext, ICosmosDatabase<MineEntity> miningContext, IMine miner)
        {
            _logger = logger;
            _userTaskContext = userTaskContext;
            _userContext = userContext;
            _miner = miner;
            _miningContext = miningContext;
        }

        [Route("/activate")]
        [HttpPut]
        public async Task<IActionResult> ActivateAccount([FromBody] ActivationModel activation)
        {
            if (ModelState.IsValid)
            {
                var user = await _userContext.GetItemAsync(activation.user_id);

                if (user != null && user.user_token.Equals(activation.user_token) && !string.IsNullOrEmpty(activation.activation_key) && user.activation_key.Equals(activation.activation_key) && !user.is_active)
                {
                    user.is_active = true;
                    await _userContext.UpdateItemAsync(user.id, user);
                    Mailer.CreateMessage(user.email, "Registrierung erfolgreich abgeschlossen!", "Herzlich wilkommen in der Money Moon Rakete!\nBei Fragen scheue dich nicht mich persönlich anzuschreiben oder eines der Hilfevideo anzusehen.\n\n\nViel Spaß beim Geld verdienen :)");
                    return Ok("Account activated.");
                }
            }
            return BadRequest("Account activation failed.");
        }

        [Route("/login")]
        [HttpPut]
        public async Task<IActionResult> Login([FromBody] UserLoginModel login)
        {
            if (ModelState.IsValid)
            {
                var user = (await _userContext.GetItemsAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}' AND {0}.password = '{2}'", nameof(UserEntity), login.email, login.password))).First();
                if (user != null)
                {
                    user.user_token = Guid.NewGuid();
                    await _userContext.UpdateItemAsync(user.id, user);
                    return Ok(new UserLoginResponseModel() { user_id = user.id, status = "success", user_token = user.user_token });
                }
            }
            return BadRequest("Login failed");
        }

        [Route("/register")]
        [HttpPost()]
        public async Task<IActionResult> StartRegistration([FromBody] UserLoginModel registration)
        {
            if (ModelState.IsValid)
            {
                var user = (await _userContext.GetItemsAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}'", nameof(UserEntity), registration.email)))?.FirstOrDefault();
                if (user == null)
                {
                    var activationKey = Guid.NewGuid().ToString().Substring(0, 4);
                    user = new UserEntity() { user_token = Guid.NewGuid(), activation_key = activationKey, email = registration.email, password = registration.password, login_ip = HttpContext.Connection.RemoteIpAddress.ToString() };
                    await _userContext.AddItemAsync(user);
                    Mailer.CreateMessage(registration.email, "Registrierung für Money Moon abschließen", string.Format("Dein Code für das aktivieren deines Accounts: {0}", activationKey));
                    return Ok(new UserLoginResponseModel() { user_id = user.id, status = "success", user_token = user.user_token });
                }
            }
            return BadRequest("Registration Failed!");
        }


        [Route("{id}")]
        [HttpGet()]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> GetUserAsync(string id)
        {
            if(ModelState.IsValid)
            {
                var user = await _userContext.GetItemAsync(id);
                return Ok(user);
            }

            return BadRequest();
        }
    }
}
