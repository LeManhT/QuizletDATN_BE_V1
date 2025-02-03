using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Src.Models.OtherFeature.Cipher;

namespace Quizlet_App_Server.Src.DataSettings
{
    [System.Serializable]
    public class AppConfigResource
    {
        public bool IsOk;
        public AesConfig Aes;
        public JwtConfig Jwt;
        public UserStoreDatabaseSetting UserStoreDatabaseSetting;

        public AppConfigResource() { }
        public void SetDefaultConfig(WebApplicationBuilder builder)
        {
            this.Aes = new();
            this.Aes = builder.Configuration.GetSection("Aes")
                        .Get<AesConfig>();

            this.Jwt = new();
            this.Jwt.Issuer = builder.Configuration.GetValue<string>("Jwt:Issuer");
            this.Jwt.Audience = builder.Configuration.GetValue<string>("Jwt:Audience");
            this.Jwt.Key = builder.Configuration.GetValue<string>("Jwt:Key");
            this.Jwt.TokenValidityMins = builder.Configuration.GetValue<int>("Jwt:TokenValidityMins");

            this.UserStoreDatabaseSetting = new();
            this.UserStoreDatabaseSetting = builder.Configuration.GetSection("UserStoreDatabaseSetting")
                                            .Get<UserStoreDatabaseSetting>();
        }

        [System.Serializable]
        public class JwtConfig
        {
            public string Issuer;
            public string Audience;
            public string Key;
            public int TokenValidityMins;
        }
    }
}
