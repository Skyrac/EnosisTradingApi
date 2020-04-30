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

        public static List<MyLeadPostback> posts = new List<MyLeadPostback>();

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
                registration.email = registration.email.ToLower();
                var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}' OR {0}.name = '{2}' OR {0}.login_ip = '{3}'", nameof(UserEntity), registration.email, registration.name, HttpContext.Connection.RemoteIpAddress.ToString()));
                if (user == null)
                {
                    var activationKey = Guid.NewGuid().ToString().Substring(0, 4);
                    user = new UserEntity() { name = registration.name, user_token = Guid.NewGuid().ToString(), activation_key = activationKey, email = registration.email, password = registration.password, login_ip = HttpContext.Connection.RemoteIpAddress.ToString(), language = registration.language, handy = registration.phone };

                    if (!string.IsNullOrEmpty(registration.referal))
                    {
                        var referal = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.referal_id = '{1}'", nameof(UserEntity), registration.referal));
                        if (referal == null)
                        {
                            return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "referal_not_found" });
                        }

                        user.referrer = referal.id;
                    }
                    await _userContext.AddItemAsync(user);
                    //Mailer.CreateMessage(user.email, Language.Translate(user.language, "title_finish_registration"), string.Format(Language.Translate(user.language, "content_finish_registration"), user.activation_key));
                    return Ok(UserModel.FromEntity(user, InfoStatus.Info));
                }
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "already_exist" });
        }

        [Route("resend")]
        [HttpPost]
        public async Task<IActionResult> ResendEmail([FromBody] UserReferenceModel activation)
        {
            if (ModelState.IsValid)
            {
                var user = await _userContext.GetItemAsync(activation.user_id);

                if (user != null && user.user_token.Equals(activation.user_token) && !user.is_active)
                {
                    Mailer.CreateMessage(user.email, Language.Translate(user.language, "title_finish_registration"), string.Format(Language.Translate(user.language, "content_finish_registration"), user.activation_key));
                    return Ok(new ResponseModel() { status = InfoStatus.Info, text = "email_sent" });
                }
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "unknown" });
        }

        [Route("activate")]
        [HttpPut]
        public async Task<IActionResult> ActivateAccount([FromBody] UserReferenceModel activation)
        {
            if (ModelState.IsValid)
            {
                var user = await _userContext.GetItemAsync(activation.user_id);

                if (user != null && user.user_token.Equals(activation.user_token) && !string.IsNullOrEmpty(activation.activation_key) && user.activation_key.Equals(activation.activation_key) && !user.is_active)
                {
                    user.is_active = true;
                    user.referal_id = string.Format("R{0}A", user.id);
                    await _userContext.UpdateItemAsync(user.id, user);
                    await _miningContext.AddItemAsync(new MineEntity() { last_check = DateTime.Now, power = 0.0f, start_date = DateTime.Now, mined_points = 0.0f, remaining_time = -717, user = user.id });
                    Mailer.CreateMessage(user.email, Language.Translate(user.language, "title_successfull_activation"), Language.Translate(user.language, "content_successfull_activation"));
                    return Ok(new ResponseModel() { status = InfoStatus.Info, text = "successful_activated" });
                }
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "invalid_activation_key" });
        }

        
        [Route("update")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserInformation([FromBody] UserModel updateInfos)
        {
            if (ModelState.IsValid)
            {
                var user = await _userContext.GetItemAsync(updateInfos.id);
                if (!string.IsNullOrEmpty(updateInfos.email))
                {
                    updateInfos.email = updateInfos.email.ToLower();
                    if (!updateInfos.email.Equals(user.email, StringComparison.OrdinalIgnoreCase))
                    {
                        var check = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}'", nameof(UserEntity), updateInfos.email));
                        if (check != null)
                        {
                            return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "email_exists" });
                        }
                    }
                }
                if (user != null && user.user_token.Equals(updateInfos.user_token))
                {
                    UserModel.Update(ref user, updateInfos);
                    await _userContext.UpdateItemAsync(user.id, user);
                    if (!user.is_active)
                    {
                        Mailer.CreateMessage(user.email, Language.Translate(user.language, "title_finish_registration"), string.Format(Language.Translate(user.language, "content_finish_registration"), user.activation_key));
                    } else
                    {
                        Mailer.CreateMessage(user.email, Language.Translate(user.language, "title_account_updated"), Language.Translate(user.language, "content_account_updated"));
                    }
                    return Ok(new ResponseModel() { status = InfoStatus.Info, text = "account_updated" });
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
                var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}' AND {0}.password = '{2}'", nameof(UserEntity), login.email.ToLower(), login.password));
                if (user != null && user != default(UserEntity))
                {
                    user.user_token = Guid.NewGuid().ToString();
                    await _userContext.UpdateItemAsync(user.id, user);
                    return Ok(UserModel.FromEntity(user, InfoStatus.Info));
                }
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "wrong_entries" });
        }

        [Route("forgot/{email}")]
        [HttpPost]
        public async Task<IActionResult> PasswordForgotten(string email)
        {
            email = email.ToLower();
            var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}'", nameof(UserEntity), email));
            if (user != null && user != default(UserEntity))
            {
                user.password_forgotten_key = Guid.NewGuid().ToString().Substring(0, 4);
                await _userContext.UpdateItemAsync(user.id, user);
                Mailer.CreateMessage(email, Language.Translate(user.language, "title_forgot_password"), string.Format(Language.Translate(user.language, "content_forgot_password"), user.name, user.password_forgotten_key));
            }
            return Ok(new ResponseModel() { status = InfoStatus.Info, text = "email_sent" });
        }

        [Route("forgot/{email}/{key}")]
        [HttpPost]
        public async Task<IActionResult> PasswordForgotten(string email, string key)
        {
            email = email.ToLower();
            if (!string.IsNullOrEmpty(key))
            {
                var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}' AND {0}.password_forgotten_key = '{2}'", nameof(UserEntity), email, key));
                if (user != null && user != default(UserEntity))
                {
                    return Ok(new ResponseModel() { status = InfoStatus.Info });
                }
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Error, text = "wrong_entries" });
        }

        [Route("mining-stats")]
        [HttpPut]
        public async Task<IActionResult> GetMiningStats([FromBody] UserReferenceModel userRef)
        {
            if(ModelState.IsValid)
            {
                var user = await _userContext.GetItemAsync(userRef.user_id);
                if(user != null && user.user_token == userRef.user_token)
                {
                    var power = await _miner.GetUserPowerAsync(user);
                    var model = UserModel.FromEntity(user);
                    model.power = power;
                    await _userContext.UpdateItemAsync(user.id, user);
                    return Ok(model);
                }
            }

            return BadRequest(new ResponseModel() { status = InfoStatus.Error, text = "invalid_user_ref" });
        }

        [Route("forgot/{email}/{key}/{password}")]
        [HttpPost]
        public async Task<IActionResult> PasswordForgotten(string email, string key, string password)
        {
            email = email.ToLower();
            var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.email = '{1}' AND {0}.password_forgotten_key = '{2}'", nameof(UserEntity), email, key));
            if (user != null && user != default(UserEntity))
            {
                user.password = password;
                user.password_forgotten_key = "";
                user.user_token = Guid.NewGuid().ToString();
                await _userContext.UpdateItemAsync(user.id, user);
                return Ok(UserModel.FromEntity(user, InfoStatus.Info));
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Error, text = "wrong_entries" });
        }
    }
}
