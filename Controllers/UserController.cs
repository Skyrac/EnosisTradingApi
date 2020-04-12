using API.Models;
using API.Models.UserModels;
using API.Repository;
using API.Services;
using API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        [Route("register")]
        [HttpPost()]
        public async Task<IActionResult> StartRegistration([FromBody] UserLoginModel registration)
        {
            if (ModelState.IsValid)
            {
                var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}'", nameof(UserEntity), registration.email));
                if (user == null)
                {
                    var activationKey = Guid.NewGuid().ToString().Substring(0, 4);
                    user = new UserEntity() { name = registration.name, user_token = Guid.NewGuid(), activation_key = activationKey, email = registration.email, password = registration.password, login_ip = HttpContext.Connection.RemoteIpAddress.ToString() };

                    if (!string.IsNullOrEmpty(registration.referal))
                    {
                        var referal = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.referal_id = '{1}'", nameof(UserEntity), registration.referal));
                        if (referal == null)
                        {
                            return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
                        }

                        user.referrer = referal.id;
                    }
                    await _userContext.AddItemAsync(user);
                    Mailer.CreateMessage(registration.email, "Registrierung für Money Moon abschließen", string.Format("Dein Code für das aktivieren deines Accounts: {0}", activationKey));
                    return Ok(UserResponseModel.FromEntity(user, InfoStatus.Info));
                }
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
        }

        [Route("activate")]
        [HttpPut]
        public async Task<IActionResult> ActivateAccount([FromBody] ActivationModel activation)
        {
            if (ModelState.IsValid)
            {
                var user = await _userContext.GetItemAsync(activation.user_id);

                if (user != null && user.user_token.Equals(activation.user_token) && !string.IsNullOrEmpty(activation.activation_key) && user.activation_key.Equals(activation.activation_key) && !user.is_active)
                {
                    user.is_active = true;
                    user.referal_id = string.Format("R{0}A", user.id);
                    await _userContext.UpdateItemAsync(user.id, user);
                    Mailer.CreateMessage(user.email, "Registrierung erfolgreich abgeschlossen!", "Herzlich wilkommen in der Money Moon Rakete!\nBei Fragen scheue dich nicht mich persönlich anzuschreiben oder eines der Hilfevideo anzusehen.\n\n\nViel Spaß beim Geld verdienen :)");
                    return Ok(new ResponseModel() { status = InfoStatus.Info });
                }
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
        }

        [Route("login")]
        [HttpPut]
        public async Task<IActionResult> Login([FromBody] UserLoginModel login)
        {
            if (ModelState.IsValid)
            {
                var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}' AND {0}.password = '{2}'", nameof(UserEntity), login.email, login.password));
                if (user != null && user != default(UserEntity))
                {
                    user.user_token = Guid.NewGuid();
                    await _userContext.UpdateItemAsync(user.id, user);
                    return Ok(UserResponseModel.FromEntity(user, InfoStatus.Info));
                }
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Warning });
        }

        [Route("forgot/{email}")]
        [HttpPost]
        public async Task<IActionResult> PasswordForgotten(string email)
        {
            var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}'", nameof(UserEntity), email));
            if (user != null && user != default(UserEntity))
            {
                user.password_forgotten_key = Guid.NewGuid().ToString().Substring(0, 4);
                await _userContext.UpdateItemAsync(user.id, user);
                Mailer.CreateMessage(email, "Money Moon - Passwort Vergessen", string.Format("Servus {0},\nIch hoffe es geht dir gut?\n\nDein Code zum zurücksetzen deines Passworts lautet: {1}", user.name, user.password_forgotten_key));
            }
            return Ok(new ResponseModel() { status = InfoStatus.Info });
        }

        [Route("forgot/{email}/{key}")]
        [HttpPost]
        public async Task<IActionResult> PasswordForgotten(string email, string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}' AND {0}.password_forgotten_key = '{2}'", nameof(UserEntity), email, key));
                if (user != null && user != default(UserEntity))
                {
                    return Ok(new ResponseModel() { status = InfoStatus.Info });
                }
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Error });
        }

        [Route("forgot/{email}/{key}/{password}")]
        [HttpPost]
        public async Task<IActionResult> PasswordForgotten(string email, string key, string password)
        {
            var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}' AND {0}.password_forgotten_key = '{2}'", nameof(UserEntity), email, key));
            if (user != null && user != default(UserEntity))
            {
                user.password = password;
                user.password_forgotten_key = "";
                user.user_token = Guid.NewGuid();
                await _userContext.UpdateItemAsync(user.id, user);
                return Ok(UserResponseModel.FromEntity(user, InfoStatus.Info));
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Error });
        }
    }
}
