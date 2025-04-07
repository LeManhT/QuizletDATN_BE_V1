using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models.Helper;
using Quizlet_App_Server.Src.Models.OtherFeature.Notification;
using Quizlet_App_Server.Src.Models.OtherFeature.RankSystem;
using Quizlet_App_Server.Src.Services;
using Quizlet_App_Server.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.Utility;
using Microsoft.Extensions.Options;
using Quizlet_App_Server.Src.Models.OtherFeature.Cipher;
using Quizlet_App_Server.Src.DataSettings;
using Quizlet_App_Server.Src.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Quizlet_App_Server.Src.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerExtend<User>
    {
        private readonly IConfiguration configuration;
        private readonly UserService service;
        private readonly RankSystemService rankSystemService;
        private readonly JwtService jwtService;
        public UserController(AppConfigResource setting
                            , IMongoClient mongoClient
                            , IConfiguration config)
            : base(setting, mongoClient)
        {
            configuration = config;
            service = new(mongoClient, config);
            rankSystemService = new(mongoClient, config);
            jwtService = new JwtService(service, config, setting);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        [HttpGet]
        public ActionResult<RankResult> GetRankResult(string userId)
        {
            var result = rankSystemService.GetRankResult(userId);

            if (result == null) return BadRequest("Request faild");

            return new ActionResult<RankResult>(result);
        }
        [HttpGet]
        public ActionResult<List<Notification>> GetAllCurrentNotices(string userId)
        {
            var existingUser = service.FindById(userId);
            if (existingUser == null)
            {
                return NotFound("User ID not found");
            }

            var result = existingUser.AllNotices;

            return new ActionResult<List<Notification>>(result);
        }

        [HttpGet]
        public ActionResult VerifyUser(string userId, string plainPassword)
        {
            bool ok = service.VerifyPassword(userId, plainPassword);

            if (ok)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

        // GET api/<UserController>/5
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Dictionary<string, object>>> Login([FromBody] UserLoginRequest loginRequest)
        {
            string keyResultType = "result_type";

            // find user
            var resultAuthenticate = jwtService.Authenticate(loginRequest, out VerifyLoginResult verifyLoginResult, out User existingUser);

            // fail
            if (resultAuthenticate == null)
            {
                Dictionary<string, object> resultFail = new Dictionary<string, object>();

                resultFail.Add(keyResultType, verifyLoginResult);
                resultFail.Add("message", verifyLoginResult.ToString());
                switch (verifyLoginResult)
                {
                    case VerifyLoginResult.None: break;
                    case VerifyLoginResult.Success: break;
                    case VerifyLoginResult.InvalidUserName: break;
                    case VerifyLoginResult.InvalidPassword:
                        resultFail.Add("try_login_remain", existingUser.TryLoginCount);
                        resultFail.Add("time_suspend_temp", existingUser.TimeSuspendTemp);
                        break;
                    case VerifyLoginResult.SuspendTemp:
                        resultFail.Add("try_login_remain", existingUser.TryLoginCount);
                        resultFail.Add("time_suspend_temp", existingUser.TimeSuspendTemp);
                        break;
                }

                return Unauthorized(resultFail);
            }

            // success
            resultAuthenticate.Add(keyResultType, verifyLoginResult);
            existingUser = resultAuthenticate["user"] as User;
            string accessToken = resultAuthenticate["accessToken"] as string;

            if (existingUser.IsSuspend)
            {
                return BadRequest($"Account has been suspended!");
            }
            if (!service.CheckSuspendTemp(existingUser))
            {
                service.ResetLoginCount(ref existingUser);
            }

            service.CheckVersionAchievement(ref existingUser);
            existingUser = existingUser.ToUserDecrypt(setting.Aes.Key);
            resultAuthenticate["user"] = existingUser;
            return Ok(resultAuthenticate);
        }
        // GET api/<UserController>/5
        [HttpPost]
        public ActionResult<User> GetInfoByID(string ID)
        {
            // find user
            var existingUser = service.FindById(ID);

            // login name incorrect
            if (existingUser == null)
            {
                return NotFound("User's ID not found");
            }

            return Ok(existingUser);
        }
        // POST api/<UserController>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult<User> SignUp([FromBody] UserSignUp request)
        {
            string hashPassword = service.EncryptPassword(request.LoginPassword);
            User newUser = new User()
            {
                LoginName = request.LoginName,
                LoginPassword = hashPassword,
                UserName = request.LoginName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth
            };

            // validate user
            var existingDocument = service.FindByLoginName(request.LoginName);

            if (existingDocument != null)
            {
                return BadRequest("Username already exists");
            }

            // insert new user's information
            newUser.SeqId = service.GetNextID();
            newUser.GenIV();
            newUser.EncryptInfo(setting.Aes.Key);
            newUser.Achievement = service.GetConfigData<Achievement>("Achievement");
            collection.InsertOne(newUser);

            var decryptInfor = newUser.ToUserDecrypt(setting.Aes.Key);
            return Ok(decryptInfor);
        }
        [HttpPost]
        public ActionResult<StreakRespone> DetectContinueStudy(string userId, long timeDetect)
        {
            var existingUser = service.FindById(userId);
            if (existingUser == null)
            {
                return NotFound("User ID not found");
            }

            timeDetect = TimeHelper.UnixTimeNow;
            // caculate streak
            if (existingUser.Streak == null || existingUser.Streak.LastTime <= 0)
            {
                existingUser.Streak = new Streak();
            }
            else
            {
                var lastTimeStudy = TimeHelper.ToDateTime(existingUser.Streak.LastTime);
                var newTimeDetect = TimeHelper.ToDateTime((int)timeDetect);

                lastTimeStudy = new DateTime(lastTimeStudy.Year, lastTimeStudy.Month, lastTimeStudy.Day);
                newTimeDetect = new DateTime(newTimeDetect.Year, newTimeDetect.Month, newTimeDetect.Day);

                TimeSpan timeOffset = newTimeDetect - lastTimeStudy;
                if (timeOffset.TotalDays == 1)
                {
                    existingUser.UpdateStreak();
                }
                else if (timeOffset.TotalDays > 1)
                {
                    existingUser.Streak.CurrentStreak = 1;
                }

            }

            // update last time caculate
            existingUser.Streak.LastTime = timeDetect;

            #region Update achivement
            int hour = TimeHelper.ToDateTime(timeDetect).Hour;
            if (hour >= 22 && hour < 24)
            {
                var taskLateNight = existingUser.Achievement.TaskList.Find(t => t.Id == 201);
                if (taskLateNight != null)
                {
                    bool wasCompleted = taskLateNight.Status >= Models.TaskStatus.Completed;
                    taskLateNight.Progress++;

                    if (!wasCompleted && taskLateNight.Status >= Models.TaskStatus.Completed)
                        //existingUser.UpdateScore(taskLateNight.Score ?? 0);
                        existingUser.CompleteNewTask(taskLateNight);
                }
            }
            else if (hour >= 4 && hour < 7)
            {
                var taskEarly = existingUser.Achievement.TaskList.Find(t => t.Id == 202);
                if (taskEarly != null)
                {
                    bool wasCompleted = taskEarly.Status >= Models.TaskStatus.Completed;
                    taskEarly.Progress++;

                    if (!wasCompleted && taskEarly.Status >= Models.TaskStatus.Completed)
                        //existingUser.UpdateScore(taskEarly.Score ?? 0);
                        existingUser.CompleteNewTask(taskEarly);
                }
            }

            var taskStudyCard = existingUser.Achievement.TaskList.Find(t => t.Id == 200);
            if (taskStudyCard != null)
            {
                bool wasCompleted = taskStudyCard.Status >= Models.TaskStatus.Completed;
                taskStudyCard.Progress++;

                if (!wasCompleted && taskStudyCard.Status >= Models.TaskStatus.Completed)
                    //existingUser.UpdateScore(taskStudyCard.Score ?? 0);
                    existingUser.CompleteNewTask(taskStudyCard);
            }
            #endregion
            // update in database
            var update = Builders<User>.Update
                .Set("streak", existingUser.Streak)
                .Set("achievement", existingUser.Achievement)
                .Set("collection_storage", existingUser.CollectionStorage)
                .Set("all_notices", existingUser.AllNotices);
            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };
            var updatedUser = collection.FindOneAndUpdate(filter, update, options);
            StreakRespone result = new StreakRespone()
            {
                Streak = updatedUser.Streak,
                Achievement = updatedUser.Achievement
            };

            return new ActionResult<StreakRespone>(result);
        }
        // PUT api/<UserController>/5
        [HttpPut]
        public ActionResult<User> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
        {
            // validate user
            bool isCorrectPassword = service.VerifyPassword(id, request.OldPassword);
            if (!isCorrectPassword)
            {
                return BadRequest("Old password incorrect");
            }

            try
            {
                var existingUser = service.FindById(id);
                string hashPassword = service.EncryptPassword(request.NewPassword);

                var update = Builders<User>.Update.Set("login_password", hashPassword);
                var filter = Builders<User>.Filter.Eq(x => x.Id, id);
                var result = collection.UpdateOne(filter, update);

                return Ok("Change password successful");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        public ActionResult<InfoPersonal> UpdateInfo(string userId, [FromBody] InfoPersonal req)
        {
            User userExisting = service.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            var result = service.UpdateInfoUser(userId, setting.Aes.Key, req);
            return new ActionResult<InfoPersonal>(result);
        }
        [HttpDelete]
        public ActionResult DeleteAllUser(string password)
        {
            if (!password.Equals("qzl_nice_app")) return BadRequest("Wrong password");

            long deleteCount = service.DeleteAllUser();
            return Ok($"Deleted: {deleteCount}");
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestedFriends([FromQuery] string userId)
        {
            var suggestedFriends = await service.GetSuggestedFriendsAsync(userId);
            return Ok(suggestedFriends);
        }
    }
}
