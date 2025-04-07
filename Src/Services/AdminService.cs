using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Src.Models.OtherFeature.Notification;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Services
{
    public class AdminService
    {
        protected readonly IMongoCollection<Admin> admin_collection;
        protected readonly IMongoCollection<User> user_collection;
        protected readonly IMongoClient client;
        private readonly IConfiguration config;
        public AdminService(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            admin_collection = database.GetCollection<Admin>(VariableConfig.Collection_Admin);
            user_collection = database.GetCollection<User>(VariableConfig.Collection_Users);

            this.client = mongoClient;
            this.config = config;
        }

        public Admin FindByLoginName(string loginName)
        {
            var filter = Builders<Admin>.Filter.Eq(x => x.LoginName, loginName);
            var existingAccount = admin_collection.Find(filter).FirstOrDefault();

            return existingAccount;
        }

        public List<User> GetUsers(int from, int to)
        {
            if (to < from) return null;

            List<User> users = user_collection.Find(user => true)
                                    .Skip(from)
                                    .Limit(to - from)
                                    .ToList();

            return users;
        }

        public bool SetSuspendUser(string userID, bool suspend)
        {
            User user = user_collection.Find(u => u.Id == userID).First();

            if(user == null) return false;

            var update = Builders<User>.Update.Set("is_suspend", suspend);
            var filter = Builders<User>.Filter.Eq(x => x.Id, user.Id);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            var result = user_collection.FindOneAndUpdate(filter, update, options);

            return true;
        }

        public UpdateResult PingNoticeUser(string userID, Notification notice)
        {
            notice.WasPushed = false;
            var update = Builders<User>.Update.PushEach("all_notices", new[] { notice }, position: 0);
            var filter = Builders<User>.Filter.Eq(x => x.Id, userID);


            return user_collection.UpdateOne(filter, update);
        }

        public UpdateResult PingNoticeAllUsers(Notification notice)
        {
            notice.WasPushed = false;
            var update = Builders<User>.Update.PushEach("all_notices", new[] { notice }, position: 0);
            var filter = Builders<User>.Filter.Empty;


            return user_collection.UpdateMany(filter, update);
        }

        public DeleteResult DeleteUser(string userID)
        {
            DeleteResult deleteResult = user_collection.DeleteOne(u => u.Id == userID);

            return deleteResult;
        }

        public int CountUsers()
        {
            var filter = Builders<User>.Filter.Empty;

            var result = user_collection.Find(filter).ToList();

            return result.Count;
        }

        public List<User> GetUsersByMonthOfTimeCreated(int month, int year)
        {
            if (month < 1 || month > 12) return new();

            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddSeconds(-1);

            long startUnix = TimeHelper.ToUnixTime(startDate);
            long endUnix = TimeHelper.ToUnixTime(endDate);

            var filter = Builders<User>.Filter.And(
                    Builders<User>.Filter.Gte(x => x.TimeCreated, startUnix),
                    Builders<User>.Filter.Lte(x => x.TimeCreated, endUnix)
                    );

            var result = user_collection.Find(filter);

            return result.ToList();
        }
    }
}
