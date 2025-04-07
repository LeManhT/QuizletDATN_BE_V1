using MongoDB.Bson;
using MongoDB.Driver;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Services
{
    public class SetPublicService
    {
        protected readonly IMongoCollection<StudySetPublic> collection;
        protected readonly IMongoClient client;
        private readonly IConfiguration config;
        public SetPublicService(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            collection = database.GetCollection<StudySetPublic>(VariableConfig.Collection_StudySetPublic);

            this.client = mongoClient;
            this.config = config;
        }

        public StudySetPublic InsertOne(string idOwner, string nameOwner, StudySet setInfo)
        {
            StudySetPublic newDocument = new StudySetPublic(idOwner, nameOwner, setInfo);
            StudySetPublic existingDocument = FindById(setInfo.Id);

            if(existingDocument == null)
            {
                collection.InsertOne(newDocument);
                return newDocument;
            }
            else
            {
                return existingDocument;
            }
        }

        public List<StudySetPublic> GetAll()
        {
            var filter = Builders<StudySetPublic>.Filter.Empty;
            var allDocuments = collection.Find(filter).ToList();

            if (allDocuments == null) allDocuments = new();

            return allDocuments;
        }

        public List<StudySetPublic> FindByName(string keyword)
        {
            var regex = new BsonRegularExpression($".*{keyword}.*", "i");
            var filter = Builders<StudySetPublic>.Filter.Regex("name", regex);
            var result = collection.Find(filter).ToList();

            if(result == null) result= new();

            return result;
        }

        public StudySetPublic FindById(string id)
        {
            var filter = Builders<StudySetPublic>.Filter.Eq(x => x.Id, id);
            var existingDocument = collection.Find(filter).FirstOrDefault();

            return existingDocument;
        }

        public long Remove(string id)
        {
            var filter = Builders<StudySetPublic>.Filter.Eq("_id", id);
            var deleteResult = collection.DeleteMany(filter);

            return deleteResult.DeletedCount;
        }
    }
}
