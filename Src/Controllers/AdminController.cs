using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Src.Models.OtherFeature.Notification;
using Quizlet_App_Server.Src.Services;
using Quizlet_App_Server.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Quizlet_App_Server.Src.DataSettings;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Quizlet_App_Server.Src.Features.Social.Models;

namespace Quizlet_App_Server.Src.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        protected readonly AdminService service;
        protected readonly IMongoCollection<Admin> collection;
        protected readonly IMongoClient client;
        private readonly AppConfigResource _appConfigResource;

        public AdminController(IMongoClient mongoClient, IConfiguration config, AppConfigResource appConfigResource)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            collection = database.GetCollection<Admin>(VariableConfig.Collection_Admin);

            this.client = mongoClient;
            this.service = new(mongoClient, config);
            _appConfigResource = appConfigResource;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult<Admin> SignUp([FromBody] AdminSignUp request)
        {
            Admin newAccount = new Admin()
            {
                LoginName = request.LoginName,
                LoginPassword = request.LoginPassword, // Nên băm password
                UserName = request.LoginName,
                Email = request.Email
            };

            // Validate account
            var existingDocument = service.FindByLoginName(newAccount.LoginName);

            if (existingDocument != null)
            {
                return BadRequest("Username already exists");
            }

            collection.InsertOne(newAccount);
            return Ok(newAccount);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.LoginName) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Login name or password is empty");
            }

            Admin existingAccount = service.FindByLoginName(request.LoginName);

            if (existingAccount == null)
            {
                return NotFound("Login name not found");
            }

            if (!existingAccount.LoginPassword.Equals(request.Password))
            {
                return BadRequest("Password incorrect");
            }

            // Tạo JWT
            var token = GenerateJwtToken(existingAccount);
            return Ok(new { Token = token });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult<List<User>> GetUsers(int from, int to)
        {
            if (to < from)
            {
                return BadRequest("Input incorrect!");
            }

            List<User> result = service.GetUsers(from, to);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public ActionResult SetSuspendUser(string userID, bool suspend)
        {
            bool updated = service.SetSuspendUser(userID, suspend);

            if (!updated)
            {
                return BadRequest("User suspend not updated");
            }

            return Ok($"User suspend was be changed to: {suspend}");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult PingNoticeUser(string userID, Notification notice)
        {
            var updateResult = service.PingNoticeUser(userID, notice);

            if (updateResult.ModifiedCount <= 0)
            {
                return BadRequest("Push notification failed!");
            }

            return Ok("Push notification success!");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteUser(string userID)
        {
            var deleteResult = service.DeleteUser(userID);

            if (deleteResult.DeletedCount <= 0)
            {
                return BadRequest("User not found");
            }

            return Ok("User was be deleted");
        }

        private string GenerateJwtToken(Admin admin)
        {
            var jwtConfig = _appConfigResource.Jwt;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, admin.LoginName),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtConfig.Issuer,
                audience: jwtConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtConfig.TokenValidityMins),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UpdateUser([FromBody] UpdateUserRequest request)
        {
            if (string.IsNullOrEmpty(request.UserID))
            {
                return BadRequest("UserID is required");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateModel = new UserUpdateModel
            {
                LoginName = request.LoginName,
                LoginPassword = request.LoginPassword,
                UserName = request.UserName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth
            };

            bool updated = service.UpdateUser(request.UserID, updateModel);

            if (!updated)
            {
                return BadRequest("User not found or update failed");
            }

            return Ok("User updated successfully");
        }

        [HttpGet("attachments")]
        [Authorize(Roles = "Admin")]
        public ActionResult<List<MessageAttachmentDto>> GetAttachmentsByConversation(string conversationId)
        {
            var db = client.GetDatabase(VariableConfig.DatabaseName);
            var messageCollection = db.GetCollection<Message>("messages");

            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(x => x.ConversationId, conversationId),
                Builders<Message>.Filter.Eq(x => x.IsDeleted, false)
            );

            var result = messageCollection.Find(filter).ToList();

            var attachmentsWithMessageId = result
                .SelectMany(msg => (msg.Attachments ?? new List<Attachment>())
                    .Select(att => new MessageAttachmentDto
                    {
                        MessageId = msg.MessageId,
                        Attachment = att
                    }))
                .ToList();

            return Ok(attachmentsWithMessageId);
        }


        [HttpDelete("attachments")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAttachmentFromMessage(string messageId, string attachmentUrl)
        {
            var db = client.GetDatabase(VariableConfig.DatabaseName);
            var messageCollection = db.GetCollection<Message>("messages");

            var filter = Builders<Message>.Filter.Eq(x => x.MessageId, messageId);
            var update = Builders<Message>.Update.PullFilter("attachments",
                Builders<Attachment>.Filter.Eq("url", attachmentUrl));

            var result = messageCollection.UpdateOne(filter, update);

            if (result.ModifiedCount == 0)
                return NotFound("Attachment not found or already deleted.");

            return Ok("Attachment deleted successfully.");
        }


        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public ActionResult<List<Conversation>> GetAllConversations()
        {
            var db = client.GetDatabase(VariableConfig.DatabaseName);
            var conversationCollection = db.GetCollection<Conversation>("conversation");

            var filter = Builders<Conversation>.Filter.Eq(x => x.IsDeleted, false);
            var result = conversationCollection.Find(filter).SortByDescending(c => c.LastMessageTime).ToList();

            return Ok(result);
        }
    }

    public class LoginRequest
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
    }

    public class AdminSignUp
    {
        public string LoginName { get; set; }
        public string LoginPassword { get; set; }
        public string Email { get; set; }
    }

    public class UpdateUserRequest
    {
        [Required]
        public string UserID { get; set; }
        public string LoginName { get; set; }
        public string LoginPassword { get; set; }
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
    }

    public class MessageAttachmentDto
    {
        public string MessageId { get; set; }
        public Attachment Attachment { get; set; }
    }

}
