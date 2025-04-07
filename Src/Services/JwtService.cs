using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.DataSettings;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Src.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Quizlet_App_Server.Src.Services
{
    public class JwtService
    {
        protected readonly UserService userService;
        private readonly IConfiguration config;
        private AppConfigResource setting;
        public JwtService(UserService userService, IConfiguration config, AppConfigResource setting)
        {
            this.userService = userService;
            this.config = config;
            this.setting = setting;
        }

        public Dictionary<string, object> Authenticate(UserLoginRequest loginReq, out VerifyLoginResult verifyLoginResult, out User existingUser)
        {
            existingUser = null;
            verifyLoginResult = VerifyLoginResult.None;

            if (string.IsNullOrWhiteSpace(loginReq.LoginName) || string.IsNullOrWhiteSpace(loginReq.LoginPassword))
            {
                verifyLoginResult = VerifyLoginResult.InvalidPassword;
                return null;
            }

            existingUser = userService.FindByLoginName(loginReq.LoginName);
            if (existingUser == null)
            {
                verifyLoginResult = VerifyLoginResult.InvalidUserName;
                return null;
            }

            if (userService.CheckSuspendTemp(existingUser))
            {
                verifyLoginResult = VerifyLoginResult.SuspendTemp;
                return null;
            }
            else if (existingUser.TryLoginCount <= 0)
            {
                userService.ResetLoginCount(ref existingUser);
            }

            // password incorrect
            bool isCorrectPassword = userService.VerifyPassword(existingUser.Id, loginReq.LoginPassword);
            if (!isCorrectPassword)
            {
                existingUser = userService.FindById(existingUser.Id);
                verifyLoginResult = VerifyLoginResult.InvalidPassword;
                return null;
            }

            Dictionary<string, object> result = new();

            // jwt token
            var issuer = setting.Jwt.Issuer;
            var audience = setting.Jwt.Audience;
            var key = setting.Jwt.Key;
            var tokenValidityMins = setting.Jwt.TokenValidityMins;
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Name, loginReq.LoginName)
                }),
                Expires = tokenExpiryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    SecurityAlgorithms.HmacSha512Signature),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            result.Add("accessToken", accessToken);
            result.Add("user", existingUser);
            verifyLoginResult = VerifyLoginResult.Success;

            return result;
        }
    }
}
