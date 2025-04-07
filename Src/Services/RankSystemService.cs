using MongoDB.Driver;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Src.Models.OtherFeature.RankSystem;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Services
{
    public class RankSystemService
    {
        protected readonly IMongoCollection<User> userCollection;
        protected readonly IMongoClient client;
        private readonly IConfiguration config;
        public RankSystemService(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            userCollection = database.GetCollection<User>(VariableConfig.Collection_Users);

            this.client = mongoClient;
            this.config = config;
        }

        public RankResult GetRankResult(string userId)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);
            var existingUser = userCollection.Find(filter).FirstOrDefault();

            if (existingUser == null) return null;

            // sort
            var sortDefinition = Builders<User>.Sort.Descending(u => u.CollectionStorage.Score);
            var sortUsersList = userCollection.Find(Builders<User>.Filter.Empty).Sort(sortDefinition).ToList();

            if (sortUsersList == null) return null;

            // get rank user
            int indexUser = sortUsersList.FindIndex(u => u.Id.Equals(userId));
            // get top 20
            var top20Users = sortUsersList.Take(20).ToList();
            List<InforUserRanking> infoList = new();
            for(int i=0; i< top20Users.Count; i++)
            {
                InforUserRanking info = top20Users[i].GetInfoScore();
                infoList.Add(info);
            }

            return new RankResult()
            {
                CurrentScore = existingUser.CollectionStorage == null ? 0 : existingUser.CollectionStorage.Score,
                CurrentRank = indexUser,
                RankSystem = new RankSystem() { UserRanking = infoList }
            };
        }
    }
}
