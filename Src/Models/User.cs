using DnsClient;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Libmongocrypt;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Src.Models.OtherFeature.Notification;
using Quizlet_App_Server.Src.Utility;
using Quizlet_App_Server.Utility;
using System.Security.Cryptography;

namespace Quizlet_App_Server.Src.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [BsonElement("seq_id")] public int SeqId { get; set; }
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        [BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
        [BsonElement("is_suspend")] public bool IsSuspend { get; set; } = false;
        [BsonElement("user_name")] public string UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string Email { get; set; } = string.Empty;
        //[BsonElement("avatar")] public string Avatar { get; set; } = string.Empty;
        [BsonElement("date_of_birth")] public string DateOfBirth { get; set; } = "1999-01-01";
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("try_login_count")] public int TryLoginCount { get; set; } = VariableConfig.MaxTryLogin;
        [BsonElement("time_suspend_temp")] public long TimeSuspendTemp { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("all_notices")] public List<Notification>? AllNotices { get; set; } = new List<Notification>();
        [BsonElement("collection_storage")] public UserCollection CollectionStorage { get; set; } = new UserCollection();
        [BsonElement("documents")] public Documents Documents { get; set; } = new Documents();
        [BsonElement("streak")] public Streak Streak { get; set; } = new Streak();
        [BsonElement("achievement")] public Achievement Achievement { get; set; } = new Achievement();

        [BsonElement("setting")] public UserSetting Setting { get; set; } = new UserSetting();

        //[BsonElement("avatar")] public List<int> Avatar { get; set; } = new List<int>();

        [BsonElement("iv")] public string IV { get; private set; } = string.Empty;
        // Mạng xã hội
        [BsonElement("friends")] public List<string> Friends { get; set; } = new List<string>();
        [BsonElement("followers")] public List<string> Followers { get; set; } = new List<string>();
        [BsonElement("following")] public List<string> Following { get; set; } = new List<string>();

        // Chức năng bài viết
        [BsonElement("posts")] public List<string> Posts { get; set; } = new List<string>();

        // Chia sẻ study set
        [BsonElement("shared_studysets")] public List<string> SharedStudySets { get; set; } = new List<string>();

        // Bình luận
        [BsonElement("comments")] public List<string> Comments { get; set; } = new List<string>();

        // Đánh giá
        [BsonElement("ratings")] public List<Rating> Ratings { get; set; } = new List<Rating>();

        // Chat
        [BsonElement("chats")] public List<Chat> Chats { get; set; } = new List<Chat>();

        // Trạng thái hoạt động
        [BsonElement("is_online")] public bool IsOnline { get; set; } = false;



        public void UpdateInfo(InfoPersonal newInfo)
        {
            UserName = newInfo.UserName;
            Email = newInfo.Email;
            //this.Avatar = newInfo.Avatar;
            DateOfBirth = newInfo.DateOfBirth;
            Setting = newInfo.Setting;
        }

        public void UpdateStreak()
        {
            if (Streak == null) return;

            Streak.CurrentStreak++;

            foreach (var task in Achievement.TaskList)
            {
                if (!task.Type.Equals(TaskType.STREAK)) continue;

                if (task.Progress >= task.Condition) continue;

                bool wasCompleted = task.Status >= TaskStatus.Completed;
                task.Progress = Streak.CurrentStreak;

                if (!wasCompleted && task.Status >= TaskStatus.Completed)
                {
                    CompleteNewTask(task);
                }
            }
        }
        public void CompleteNewTask(Task task)
        {
            Notification newNotice = new Notification()
            {
                Id = task.Id,
                Title = $"Reached new milestone",
                Detail = $"Congratulation!! You reached {task.TaskName}",
                WasPushed = false
            };
            CollectionStorage.Score += task.Score ?? 0;
            if (AllNotices == null) AllNotices = new();

            AllNotices.Insert(0, newNotice);

            // max = 5 notices
            if (AllNotices.Count > 5)
            {
                AllNotices.RemoveAt(5);
            }
        }
        public void UpdateScore(int value)
        {
            CollectionStorage.Score += value;
        }
        public void UpdateScore(int baseValue, int multiple)
        {
            int value = baseValue * multiple;
            CollectionStorage.Score += value;
        }
        public InfoPersonal GetInfo(string key)
        {
            string decryptUserName = UserName;
            string decryptEmail = Email;
            try
            {
                decryptUserName = AesHelper.DecryptData(UserName, key, IV);
                decryptEmail = AesHelper.DecryptData(Email, key, IV);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return new InfoPersonal()
            {
                UserName = UserName,
                Email = Email,
                //Avatar = this.Avatar,
                DateOfBirth = DateOfBirth,
                Setting = Setting
            };
        }
        public InforUserRanking GetInfoScore()
        {
            return new InforUserRanking()
            {
                Score = CollectionStorage.Score,
                SeqId = SeqId,
                UserName = UserName,
                Email = Email,
                //Avatar = this.Avatar,
                DateOfBirth = DateOfBirth
            };
        }

        public void EncryptInfo(string key)
        {
            UserName = AesHelper.EncryptDataToBase64(UserName, key, IV);
            Email = AesHelper.EncryptDataToBase64(Email, key, IV);
        }

        public void GenIV()
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateIV();
                byte[] iv = aes.IV;
                IV = Convert.ToBase64String(iv);
            }
        }
        public byte[] GetIVByteArr()
        {
            return Convert.FromBase64String(IV);
        }
        public User ToUserDecrypt(string key)
        {
            var info = GetInfo(key);
            UserName = info.UserName;
            Email = info.Email;

            return this;
        }

    }

    [Serializable]
    public class UserSignUp
    {
        public string LoginName { get; set; } = string.Empty;
        public string LoginPassword { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = "1999-01-01";
    }

    [Serializable]
    public class UserLoginRequest
    {
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        [BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
    }


    [Serializable]
    public class UserLogin
    {
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        [BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
    }

    [Serializable]
    public class ChangePasswordRequest
    {
        [BsonElement("old_password")] public string OldPassword { get; set; } = string.Empty;
        [BsonElement("new_password")] public string NewPassword { get; set; } = string.Empty;
    }

    [Serializable]
    public class UserRespone
    {
        public string Id { get; set; } = string.Empty;
        [BsonElement("seq_id")] public int SeqId { get; set; }
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        //[BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
        [BsonElement("user_name")] public string UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string Email { get; set; } = string.Empty;
        //[BsonElement("avatar")] public string Avatar { get; set; } = string.Empty;
        [BsonElement("date_of_birth")] public string DateOfBirth { get; set; } = "1999-01-01";
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("documents")] public Documents Documents { get; set; } = new Documents();
        [BsonElement("setting")] public UserSetting Setting { get; set; } = new UserSetting();
        //[BsonElement("avatar")] public List<int> Avatar { get; set; } = new List<int>();
        // Mạng xã hội
        [BsonElement("friends")] public List<string> Friends { get; set; } = new List<string>();
        [BsonElement("followers")] public List<string> Followers { get; set; } = new List<string>();
        [BsonElement("following")] public List<string> Following { get; set; } = new List<string>();

        // Chức năng bài viết
        [BsonElement("posts")] public List<string> Posts { get; set; } = new List<string>();

        // Chia sẻ study set
        [BsonElement("shared_studysets")] public List<string> SharedStudySets { get; set; } = new List<string>();

        // Bình luận
        [BsonElement("comments")] public List<string> Comments { get; set; } = new List<string>();

        // Đánh giá
        [BsonElement("ratings")] public List<Rating> Ratings { get; set; } = new List<Rating>();

        // Chat
        [BsonElement("chats")] public List<Chat> Chats { get; set; } = new List<Chat>();

        // Trạng thái hoạt động
        [BsonElement("is_online")] public bool IsOnline { get; set; } = false;


        public UserRespone(User user)
        {
            Id = user.Id;
            SeqId = user.SeqId;
            LoginName = user.LoginName;
            UserName = user.UserName;
            Email = user.Email;
            //this.Avatar = user.Avatar;
            DateOfBirth = user.DateOfBirth;
            TimeCreated = user.TimeCreated;
            Documents = user.Documents;
            Setting = user.Setting;


            // Các trường bổ sung
            Friends = user.Friends;
            Followers = user.Followers;
            Following = user.Following;
            Posts = user.Posts;
            SharedStudySets = user.SharedStudySets;
            Comments = user.Comments;
            Ratings = user.Ratings;
            Chats = user.Chats;
            IsOnline = user.IsOnline;
        }
    }

    public class Rating
    {
        [BsonElement("studyset_id")] public string StudySetId { get; set; } = string.Empty;
        [BsonElement("rating_value")] public int RatingValue { get; set; }
    }

    [Serializable]
    public class InfoPersonal
    {
        [BsonElement("user_name")] public string? UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string? Email { get; set; } = string.Empty;
        //[BsonElement("avatar")] public string Avatar { get; set; } = string.Empty;
        [BsonElement("date_of_birth")] public string? DateOfBirth { get; set; } = string.Empty;
        [BsonElement("setting")] public UserSetting? Setting { get; set; } = new UserSetting();
        //[BsonElement("avatar")] public List<int>? Avatar { get; set; } = new List<int>();
    }

    [Serializable]
    public class InforUserRanking
    {
        [BsonElement("score")] public int Score { get; set; } = 0;
        [BsonElement("seq_id")] public int SeqId { get; set; } = 0;
        [BsonElement("user_name")] public string? UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string? Email { get; set; } = string.Empty;
        //[BsonElement("avatar")] public string Avatar { get; set; } = string.Empty;
        [BsonElement("date_of_birth")] public string? DateOfBirth { get; set; } = string.Empty;
        //[BsonElement("avatar")] public List<int>? Avatar { get; set; } = new List<int>();
    }

    [Serializable]
    public class UserCollection
    {
        [BsonElement("create_set_count")] public int CreateSetCount { get; set; } = 0;
        [BsonElement("score")] public int Score { get; set; } = 0;
    }
}
