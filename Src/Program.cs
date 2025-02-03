using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Newtonsoft.Json;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Src.DataSettings;
using Quizlet_App_Server.Src.Models.OtherFeature.Cipher;
using Quizlet_App_Server.Utility;
using System.Text;

Console.WriteLine($"Start {VariableConfig.IdPublish}");
var builder = WebApplication.CreateBuilder(args);
AppConfigResource appConfigResource = new();

#region get appconfig resource
HttpClient resourceClient = new HttpClient();
resourceClient.BaseAddress = new Uri(VariableConfig.ResourceSupplierString);
var resourceRes = await resourceClient.GetAsync($"/get-data?message={VariableConfig.IdPublish}");
if (resourceRes.IsSuccessStatusCode)
{
    var content = resourceRes.Content.ReadAsStringAsync().Result;
    Console.WriteLine(content);

    AppConfigResource deserializedContent = JsonConvert.DeserializeObject<AppConfigResource>(content);

    appConfigResource = deserializedContent;
    appConfigResource.IsOk = true;
}
else
{
    Console.WriteLine("Error: Can not fetch resource!");

    appConfigResource.SetDefaultConfig(builder);
}
#endregion

// Add services to the container.
builder.Services.AddSingleton<AppConfigResource>(appConfigResource);
#region UserStoreDatabaseSetting
builder.Services.AddSingleton<IMongoClient>(
                            s => new MongoClient(appConfigResource.UserStoreDatabaseSetting.ConnectionString));
#endregion


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your JWT Access Token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {jwtSecurityScheme, Array.Empty<string>() }
    });
});

//JWT Authentication
#region JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = appConfigResource.Jwt.Issuer,
        ValidAudience = appConfigResource.Jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appConfigResource.Jwt.Key))
    };
});
#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
