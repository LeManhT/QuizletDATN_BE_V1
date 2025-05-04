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
using Quizlet_App_Server.Src;
using Quizlet_App_Server.Src.Features.ChatBot.Services;
using Quizlet_App_Server.Src.Features.Payment.Service;
using Quizlet_App_Server.Src.Features.Social.Service;
using Quizlet_App_Server.Src.Models.OtherFeature.Cipher;
using Quizlet_App_Server.Utility;
using System.Text;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.Mapping;

Console.WriteLine($"Start {VariableConfig.IdPublish}");
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAutoMapper(typeof(MappingProfile));
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

builder.Logging.ClearProviders(); // Xóa các providers mặc định
builder.Logging.AddConsole().AddDebug(); // Thêm console logger
builder.Services.AddScoped<MomoPaymentService>();
// Đăng ký ChatBotService
builder.Services.AddScoped<ChatBotService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddSingleton<S3Service>();
builder.Services.AddSingleton<FriendService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<ChatHistoryService>();
builder.Services.AddScoped<UserService>();
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


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(
            "http://127.0.0.1:5500",         // Nếu dùng trình duyệt trên máy cục bộ
            "http://10.0.2.2:5500",          // Trình giả lập Android
            "http://192.168.1.100:5500" // IP của máy chủ trong mạng LAN
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Hỗ trợ credentials
    });
});


// Thêm SignalR
builder.Services.AddSignalR();

var app = builder.Build();
app.UseCors("AllowSpecificOrigin");

//builder.Logging.ClearProviders(); 
builder.Logging.AddConsole().AddDebug();

app.UseRouting();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
    app.UseSwaggerUI();
//}
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quizlet App Server v1");
    c.RoutePrefix = string.Empty; // Đặt Swagger UI tại URL gốc
});

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub");
    endpoints.MapHub<PostHub>("/postHub");
    endpoints.MapHub<FriendHub>("/friendHub");
});

app.MapControllers();

app.Run();
