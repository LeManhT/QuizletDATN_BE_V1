using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Src.Models.OtherFeature.Notification;
using Quizlet_App_Server.Src.Services;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        protected readonly AdminService service;
        protected readonly IMongoCollection<Admin> collection;
        protected readonly IMongoClient client;
        public AdminController(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            collection = database.GetCollection<Admin>(VariableConfig.Collection_Admin);

            this.client = mongoClient;
            this.service = new(mongoClient, config);
        }
        [HttpPost]
        public ActionResult<Admin> SignUp([FromBody] AdminSignUp request)
        {
            Admin newAccount = new Admin()
            {
                LoginName = request.LoginName,
                LoginPassword = request.LoginPassword,
                UserName = request.LoginName,
                Email = request.Email
            };

            // validate account
            var existingDocument = service.FindByLoginName(newAccount.LoginName);

            if (existingDocument != null)
            {
                return BadRequest("Username already exists");
            }

            collection.InsertOne(newAccount);
            return Ok(newAccount);
        }
        [HttpPost]
        public ActionResult<Admin> Login(string loginName, string password)
        {
            Admin existingAccount = service.FindByLoginName(loginName);

            if (existingAccount == null)
            {
                return NotFound("Login name not found");
            }

            if(!existingAccount.LoginPassword.Equals(password))
            {
                return BadRequest("Password incorrect");
            }

            return Ok(existingAccount);
        }

        [HttpGet]
        public ActionResult<List<User>> GetUsers(int from, int to)
        {
            if(to < from)
            {
                return BadRequest("Input incorrect!");
            }

            List<User> result = service.GetUsers(from, to);
            return Ok(result);
        }

        [HttpGet]
        public ActionResult<Dictionary<string, int>> GetChartUsersCreateByMonth()
        {
            try
            {
                int year = 2024;
                Dictionary<string, int> result = new();

                result.Add("totalUsers", service.CountUsers());

                for (int i = 1; i <= 12; i++)
                {
                    result.Add(i.ToString(), service.GetUsersByMonthOfTimeCreated(i, year).Count);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult SetSuspendUser(string userID, bool suspend)
        {
            bool updated = service.SetSuspendUser(userID, suspend);

            if(!updated)
            {
                return BadRequest("User suspend not updated");
            }

            return Ok($"User suspend was be changed to: {suspend}");
        }

        [HttpPost]
        public ActionResult PingNoticeUser(string userID, Notification notice)
        {
            var updateResult = service.PingNoticeUser(userID, notice);

            if(updateResult.ModifiedCount <= 0)
            {
                return BadRequest("Push notification failed!");
            }

            return Ok("Push notification success!");
        }

        [HttpPost]
        public ActionResult PingNoticeForAllUsers(Notification notice)
        {
            var updateResult = service.PingNoticeAllUsers(notice);

            if (updateResult.ModifiedCount <= 0)
            {
                return BadRequest("Push notification failed!");
            }

            return Ok("Push notification success!");
        }

        [HttpDelete]
        public ActionResult DeleteUser(string userID)
        {
            var delteResult = service.DeleteUser(userID);

            if(delteResult.DeletedCount <= 0)
            {
                return BadRequest("User not found");
            }

            return Ok("User was be deleted");
        }
    }
}
