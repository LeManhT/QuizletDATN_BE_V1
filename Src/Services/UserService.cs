using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Models.Helper;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Src.Models.OtherFeature.Cipher;
using Quizlet_App_Server.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;

namespace Quizlet_App_Server.Services
{
    public class UserService
    {
        protected readonly IMongoCollection<User> collection;
        protected readonly IMongoCollection<Friend> _friendCollection;
        protected readonly IMongoClient client;
        private readonly IConfiguration config;

        public IMongoCollection<User> Collection => collection;

        public UserService(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            collection = database.GetCollection<User>(VariableConfig.Collection_Users);
            _friendCollection = database.GetCollection<Friend>("friends");

            this.client = mongoClient;
            this.config = config;
        }


        //public async Task<List<User>> GetSuggestedFriendsAsync(string userId)
        //{
        //    var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        //    if (user == null)
        //    {
        //        return new List<User>();
        //    }

        //    var friendIds = user.Friends;
        //    var suggestedFriends = await collection.Find(u => !friendIds.Contains(u.Id) && u.Id != userId).ToListAsync();

        //    return suggestedFriends;
        //}

        public async Task<List<User>> GetSuggestedFriendsAsync(string userId)
        {
            // Tìm danh sách bạn bè của user từ bảng Friend
            var friends = await _friendCollection.Find(f => f.UserIds.Contains(userId)).ToListAsync();

            // Lấy danh sách ID bạn bè (loại bỏ userId)
            var friendIds = friends.SelectMany(f => f.UserIds).Where(id => id != userId).ToHashSet();

            // Lọc danh sách gợi ý (không phải bạn bè và không phải chính user)
            var suggestedFriends = await collection
                .Find(u => !friendIds.Contains(u.Id) && u.Id != userId)
                .ToListAsync();

            return suggestedFriends;
        }


        public int GetNextID()
        {
            string id = "user_sequence";
            var database = client.GetDatabase(VariableConfig.DatabaseName);
            var sequenceCollection = database.GetCollection<UserSequence>(VariableConfig.Collection_UserSequence);

            var filter = Builders<UserSequence>.Filter.Eq(x => x.Id, id);
            var existingDocument = sequenceCollection.Find(filter).FirstOrDefault();

            if (existingDocument == null)
            {
                var defaultSequence = new UserSequence
                {
                    Id = id,
                    Value = 10000
                };

                sequenceCollection.InsertOne(defaultSequence);
                return defaultSequence.Value;
            }

            var update = Builders<UserSequence>.Update.Inc(x => x.Value, 1);
            var options = new FindOneAndUpdateOptions<UserSequence>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var result = sequenceCollection.FindOneAndUpdate<UserSequence>(filter, update, options);
            return result.Value;
        }

        public T GetConfigData<T>(string specialName) where T: Configurable
        {
            var database = client.GetDatabase(VariableConfig.DatabaseName);
            var configCollection = database.GetCollection<T>(VariableConfig.Collection_Configure);

            var filter = Builders<T>.Filter.Eq(x => x.SpecialName, specialName);
            var existingDocument = configCollection.Find(filter).FirstOrDefault();

            return existingDocument;
        }
        public User FindBySeqId(int seqId)
        {
            var filter = Builders<User>.Filter.Eq(x => x.SeqId, seqId);
            var existingUser = collection.Find(filter).FirstOrDefault();

            return existingUser;
        }
        public long DeleteAllUser()
        {
            var filter = Builders<User>.Filter.Empty;
            long deleteCount = collection.DeleteMany(filter).DeletedCount;

            return deleteCount;
        }
        public User FindById(string id)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(x => x.Id, id);
                var existingUser = collection.Find(filter).FirstOrDefault();

                return existingUser;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return null;
            }
        }
        public User FindByLoginName(string loginName)
        {
            var filter = Builders<User>.Filter.Eq(x => x.LoginName, loginName);
            var existingUser = collection.Find(filter).FirstOrDefault();

            return existingUser;
        }
        public User UpdateDocumentsUser(User existingUser)
        {
            var update = Builders<User>.Update.Set("documents", existingUser.Documents);
            var filter = Builders<User>.Filter.Eq(x => x.Id, existingUser.Id);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            var result = collection.FindOneAndUpdate(filter, update, options);

            return result;
        }
        public UpdateResult UpdateCollectionStorage(User existingUser)
        {
            var update = Builders<User>.Update.Set("collection_storage", existingUser.CollectionStorage);
            var filter = Builders<User>.Filter.Eq(x => x.Id, existingUser.Id);
            var result = collection.UpdateOne(filter, update);

            return result;
        }
        public User UpdateScore(User existingUser, int score)
        {
            existingUser.CollectionStorage.Score += score;
            var update = Builders<User>.Update.Set("collection_storage", existingUser.CollectionStorage);
            var filter = Builders<User>.Filter.Eq(x => x.Id, existingUser.Id);
            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };
            var result = collection.FindOneAndUpdate(filter, update, options);

            return result;
        }
        public User CompleteNewTask(User existingUser, int taskId)
        {
            Src.Models.Task task = existingUser.Achievement.TaskList.Find(t => t.Id== taskId);
            if (task == null) return null;

            existingUser.CompleteNewTask(task);
            var update = Builders<User>.Update
                .Set("collection_storage", existingUser.CollectionStorage)
                .Set("all_notices", existingUser.AllNotices);
            var filter = Builders<User>.Filter.Eq(x => x.Id, existingUser.Id);
            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };
            var result = collection.FindOneAndUpdate(filter, update, options);

            return result;
        }

        public User UpdateScore(string userId, int score)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

            User existingUser = collection.Find(filter).First();
            if (existingUser == null) return null;

            existingUser.CollectionStorage.Score += score;
            var update = Builders<User>.Update.Set("collection_storage", existingUser.CollectionStorage);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };
            var result = collection.FindOneAndUpdate(filter, update, options);

            return result;
        }
        public User UpdateUserValue(string userId, string key, object value, bool requireEncrypt = false)
        {
            var update = Builders<User>.Update.Set(key, value);
            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);
            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };
            var result = collection.FindOneAndUpdate(filter, update, options);

            return result;
        }
        public async Task<List<StudySet>> FindStudySetsByTextAsync(string text)
        {
            var filter = Builders<User>.Filter.ElemMatch(user => user.Documents.StudySets,
                studySet => studySet.Name.ToLower().Contains(text.ToLower()) ||
                            studySet.Description.ToLower().Contains(text.ToLower()));

            var users = await collection.Find(filter).ToListAsync();

            return users.SelectMany(user => user.Documents.StudySets
                .Where(studySet =>
                    studySet.Name.ToLower().Contains(text.ToLower()) ||
                    studySet.Description.ToLower().Contains(text.ToLower()))
            ).ToList();
        }

        public InfoPersonal UpdateInfoUser(string userId, string aesKey, InfoPersonal newInfo)
        {
            var updateDefinitionList = new List<UpdateDefinition<User>>();

            if (!newInfo.UserName.IsNullOrEmpty())
            {
                updateDefinitionList.Add(Builders<User>.Update.Set("user_name", newInfo.UserName));
            }
            if (!newInfo.Email.IsNullOrEmpty())
            {
                updateDefinitionList.Add(Builders<User>.Update.Set("email", newInfo.Email));
            }
/*            if (newInfo.Avatar != null)
            {
                updateDefinitionList.Add(Builders<User>.Update.Set("avatar", newInfo.Avatar));
            }*/
            if (!newInfo.DateOfBirth.IsNullOrEmpty())
            {
                updateDefinitionList.Add(Builders<User>.Update.Set("date_of_birth", newInfo.DateOfBirth));
            }
            if (newInfo.Setting != null)
            {
                updateDefinitionList.Add(Builders<User>.Update.Set("setting", newInfo.Setting));
            }

            var combinedUpdate = Builders<User>.Update.Combine(updateDefinitionList);



            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After 
            };

            var updatedUser = collection.FindOneAndUpdate(filter, combinedUpdate, options);
            return updatedUser.GetInfo(aesKey);
        }
        public User UpdateAchievement(string userId, Achievement newAchievement)
        {
            var update = Builders<User>.Update.Set("achievement", newAchievement);
            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedUser = collection.FindOneAndUpdate(filter, update, options);
            return updatedUser;
        }
        // To generate token
        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.LoginName),
            };
            var token = new JwtSecurityToken(config["Jwt:Issuer"],
                config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool VerifyPassword(string userId, string plainTxtPass)
        {
            var existingUser = FindById(userId);

            if (existingUser == null)
                return false;

            bool isCorrectPassword = BCrypt.Net.BCrypt.EnhancedVerify(plainTxtPass, existingUser.LoginPassword);

            if (!isCorrectPassword)
            {
                existingUser.TryLoginCount--;
                UpdateUserValue(userId, "try_login_count", existingUser.TryLoginCount);

                if (existingUser.TryLoginCount <= 0)
                {
                    long durationSuspend = 60 * 60; // in 1 hour
                    SetSuspendInDuration(userId, durationSuspend);
                }
            }
            return isCorrectPassword;
        }
        public string EncryptPassword(string plainTxtPassword)
        {
            string hashPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(plainTxtPassword);
            return hashPassword;
        }
        public void CheckVersionAchievement(ref User existingUser)
        {
            Achievement currentAchievement = existingUser.Achievement != null
                                ? existingUser.Achievement
                                : new Achievement();
            Achievement configAchievement = GetConfigData<Achievement>("Achievement");

            if (configAchievement.Version > currentAchievement.Version)
            {
                List<Src.Models.Task> newTasks = new List<Src.Models.Task>();

                foreach (var configTask in configAchievement.TaskList)
                {
                    var taskOfUser = currentAchievement.TaskList.Find(t => t.Id == configTask.Id);

                    if (taskOfUser == null)
                    {
                        newTasks.Add(configTask);
                    }
                    else if (taskOfUser.Condition != configTask.Condition)
                    {
                        taskOfUser.Condition = configTask.Condition;
                    }
                }

                currentAchievement.Version = configAchievement.Version;
                currentAchievement.TaskList.AddRange(newTasks);
                existingUser.Achievement = UpdateAchievement(existingUser.Id, currentAchievement).Achievement;

            }
        }

        public void ResetLoginCount(ref User existingUser)
        {
            existingUser = UpdateUserValue(existingUser.Id, "try_login_count", VariableConfig.MaxTryLogin);
        }
        public bool CheckSuspendTemp(User existingUser)
        {
            long curTime = TimeHelper.UnixTimeNow;

            if(curTime < existingUser.TimeSuspendTemp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SetSuspendInDuration(string userId, long second)
        {
            try
            {
                UpdateUserValue(userId, "time_suspend_temp", TimeHelper.UnixTimeNow + second);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
