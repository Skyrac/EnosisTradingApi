using API.Models;
using API.Models.PostbackModels;
using API.Models.UserModels;
using API.Repository;
using API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("users")]
    public class UserController : Controller
    {
        private ICosmosDatabase<UserTaskEntity> _userTaskContext;
        private ICosmosDatabase<UserEntity> _userContext;
        private ICosmosDatabase<StakeEntity> _stakingContext;

        public static List<MyLeadPostback> posts = new List<MyLeadPostback>();

        private readonly ILogger<PostbackController> _logger;

        private const int MIN_STAKE = 10000;
        private const double MIN_INTEREST = 0.0001f;
        private const double INTEREST_INCREASE = 0.00012f;
        private const double INTEREST_DECREASE = 0.00012f;

        public UserController(ILogger<PostbackController> logger, ICosmosDatabase<UserTaskEntity> userTaskContext, ICosmosDatabase<UserEntity> userContext, ICosmosDatabase<StakeEntity> miningContext)
        {
            _logger = logger;
            _userTaskContext = userTaskContext;
            _userContext = userContext;
            _stakingContext = miningContext;
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
                    user = new UserEntity() { name = registration.name, user_token = Guid.NewGuid().ToString(), activation_key = activationKey, email = registration.email, password = registration.password, login_ip = HttpContext.Connection.RemoteIpAddress.ToString(), language = registration.language, handy = registration.phone, last_interest = DateTime.Now, interest = MIN_INTEREST };

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

        [Route("refresh")]
        [HttpPut]
        public async Task<IActionResult> Refresh([FromBody] UserReferenceModel request)
        {
            if (ModelState.IsValid)
            {
                var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.id = '{1}' AND {0}.user_token = '{2}'", nameof(UserEntity), request.user_id, request.user_token));
                if (user != null && user != default(UserEntity))
                {
                    var tasks = (await _userTaskContext.GetItemsAsync(string.Format("SELECT * FROM {0} WHERE {0}.user = '{1}' and {0}.is_checked = false", nameof(UserTaskEntity), request.user_id))).ToList();
                    await CheckSurfbar(user);
                    if (tasks.Count() > 0)
                    {
                        foreach (var task in tasks)
                        {
                            task.is_checked = true;
                            user.free_points += task.reward;
                        }

                        await _userTaskContext.BulkUpdateAsync(tasks);
                        await _userContext.UpdateItemAsync(user.id, user);
                    }
                    return Ok(UserModel.FromEntity(user, InfoStatus.Info));
                }
                return BadRequest(new ResponseModel() { status = InfoStatus.Error, text = "no_user_found" });
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Error, text = "invalid_request" });
        }

        private async Task CheckSurfbar(UserEntity user)
        {
            var webRequest = WebRequest.Create(string.Format("https://www.ebesucher.de/api/visitor_exchange.json/surflink/Skyrac.{0}/earnings/{1}-{2}", user.id, user.last_surf_claim, (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds));
            webRequest.Credentials = new NetworkCredential("Skyrac", "5J8vk4YQpn5pblTCVO9taVxy3j3rTJqmEIDE238SJOenUiiEik");
            var response = await webRequest.GetResponseAsync();
            if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
            {

                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.
                    Console.WriteLine(responseFromServer);
                    if (!string.IsNullOrEmpty(responseFromServer))
                    {
                        user.last_surf_claim = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        var surfTasks = JsonConvert.DeserializeObject<SurfModel[]>(responseFromServer);
                        if (surfTasks != null && surfTasks.Length > 0)
                        {
                            var newTasks = new List<UserTaskEntity>();
                            foreach (var surfTask in surfTasks)
                            {
                                var convertedValue = (double)Math.Round(double.Parse(surfTask.value, new CultureInfo("en")
                                {
                                    NumberFormat = { NumberDecimalSeparator = "." }
                                }), 2);
                                var newTask = new UserTaskEntity("", user.id, "eBesucher", "1", 0.000021f * convertedValue, "eBesucher") { is_checked = true };
                                newTasks.Add(newTask);
                                user.free_points += newTask.reward;
                            }
                            await _userTaskContext.BulkInsertAsync(newTasks);
                            await _userContext.UpdateItemAsync(user.id, user);
                        }
                    }
                }
            }
            response.Close();
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
                    if (user.staked_points >= MIN_STAKE)
                    {
                        var deltaInterest = DateTime.Now.Subtract(user.last_interest);
                        var days = deltaInterest.TotalDays;
                        if (days > 1)
                        {
                            for (var i = 1; i < Math.Floor(days); i++)
                            {
                                user.interest = Math.Max(0, user.interest - INTEREST_DECREASE);
                            }
                        }
                        if (deltaInterest.Days >= 1)
                        {
                            user.last_interest = DateTime.Now;
                            user.interest = Math.Max(MIN_INTEREST, Math.Round(user.interest + INTEREST_INCREASE, 6));
                            var earned_interest = user.staked_points * user.interest;
                            user.free_points += earned_interest;
                            await _stakingContext.AddItemAsync(new StakeEntity() { user = user.id, date = DateTime.Now, points = earned_interest });
                        }
                    }
                    user.user_token = Guid.NewGuid().ToString();
                    await _userContext.UpdateItemAsync(user.id, user);
                    return Ok(UserModel.FromEntity(user, InfoStatus.Info));
                }
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "wrong_entries" });
        }


        [Route("stake")]
        [HttpPost]
        public async Task<IActionResult> Stake([FromBody] UserStakeModel stakeObj)
        {
            if (ModelState.IsValid)
            {
                var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.id = '{1}' AND {0}.user_token = '{2}'", nameof(UserEntity), stakeObj.user_id, stakeObj.user_token));
                if (user != null && user != default(UserEntity))
                {
                    if (stakeObj.amount < MIN_STAKE)
                    {
                        return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "invalid_invest_value" });
                    }
                    else if (user.free_points >= stakeObj.amount)
                    {
                        user.free_points -= stakeObj.amount;
                        user.staked_points += stakeObj.amount;
                        await _stakingContext.AddItemAsync(new StakeEntity() { date = DateTime.Now, points = stakeObj.amount, user = user.id });
                        await _userContext.UpdateItemAsync(user.id, user);
                        return Ok(UserModel.FromEntity(user, InfoStatus.Info));
                    }
                    else
                    {
                        return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "not_enough_monies" });
                    }
                }
                return BadRequest(new ResponseModel() { status = InfoStatus.Error, text = "no_user_found" });
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Error, text = "invalid_request" });
        }

        [Route("withdraw")]
        [HttpPost]
        public async Task<IActionResult> Withdraw([FromBody] UserStakeModel stakeObj)
        {
            if (ModelState.IsValid)
            {
                var user = await _userContext.GetItemByQueryAsync(string.Format("SELECT * FROM {0} WHERE {0}.id = '{1}' AND {0}.user_token = '{2}'", nameof(UserEntity), stakeObj.user_id, stakeObj.user_token));
                if (user != null && user != default(UserEntity))
                {
                    if (stakeObj.amount < 0)
                    {
                        return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "invalid_withdraw_value" });
                    }
                    else if (user.staked_points >= stakeObj.amount)
                    {
                        user.free_points += stakeObj.amount;
                        user.staked_points -= stakeObj.amount;
                        user.interest = MIN_INTEREST;
                        await _stakingContext.AddItemAsync(new StakeEntity() { date = DateTime.Now, points = -stakeObj.amount, user = user.id });
                        await _userContext.UpdateItemAsync(user.id, user);
                        return Ok(UserModel.FromEntity(user, InfoStatus.Info));
                    }
                    else
                    {
                        return BadRequest(new ResponseModel() { status = InfoStatus.Warning, text = "not_enough_stake" });
                    }
                }
                return BadRequest(new ResponseModel() { status = InfoStatus.Error, text = "no_user_found" });
            }
            return BadRequest(new ResponseModel() { status = InfoStatus.Error, text = "invalid_request" });
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
