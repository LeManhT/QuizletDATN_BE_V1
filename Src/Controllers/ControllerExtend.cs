using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.DataSettings;

namespace Quizlet_App_Server.Src.Controllers
{
    public class ControllerExtend<T> : ControllerBase
    {
        protected readonly AppConfigResource setting;
        protected readonly IMongoDatabase database;
        protected readonly IMongoCollection<T> collection;
        protected readonly IMongoClient client;
        public ControllerExtend(AppConfigResource setting, IMongoClient mongoClient)
        {
            database = mongoClient.GetDatabase(setting.UserStoreDatabaseSetting.DatabaseName);
            collection = database.GetCollection<T>(setting.UserStoreDatabaseSetting.CollectionName);

            client = mongoClient;
            this.setting = setting;
        }
    }
}
