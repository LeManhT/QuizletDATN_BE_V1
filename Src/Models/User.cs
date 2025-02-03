using Microsoft.AspNetCore.Mvc.ViewEngines;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Libmongocrypt;
using Quizlet_App_Server.Src.Models.OtherFeature.Notification;
using Quizlet_App_Server.Src.Utility;
using Quizlet_App_Server.Utility;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace Quizlet_App_Server.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [BsonElement("seq_id")] public int SeqId { get; set; }
        [BsonElement("login_name")] public string LoginName { get; set; }
        [BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
        [BsonElement("is_suspend")] public bool IsSuspend { get; set; } = false;
        [BsonElement("user_name")] public string UserName { get; set; }
        [BsonElement("email")] public string Email { get; set; }
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
        public void UpdateInfo(InfoPersonal newInfo)
        {
            this.UserName = newInfo.UserName;
            this.Email = newInfo.Email;
            //this.Avatar = newInfo.Avatar;
            this.DateOfBirth = newInfo.DateOfBirth;
            this.Setting = newInfo.Setting;
        }

        public void UpdateStreak()
        {
            if (Streak == null) return;

            Streak.CurrentStreak++;

            foreach(var task in Achievement.TaskList)
            {
                if (!task.Type.Equals(TaskType.STREAK)) continue;

                if (task.Progress >= task.Condition) continue;

                bool wasCompleted = task.Status >= TaskStatus.Completed;
                task.Progress = Streak.CurrentStreak;

                if(!wasCompleted && task.Status >= TaskStatus.Completed)
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
            this.CollectionStorage.Score += task.Score ?? 0;

            InsertNewNotice(newNotice);
        }

        public void InsertNewNotice(Notification newNotice)
        {
            if (AllNotices == null) AllNotices = new();

            AllNotices.Insert(0, newNotice);

            /*// max = 5 notices
            if (AllNotices.Count > 5)
            {
                AllNotices.RemoveAt(5);
            }*/
        }

        public void UpdateScore(int value)
        {
            this.CollectionStorage.Score += value;
        }
        public void UpdateScore(int baseValue, int multiple)
        {
            int value = baseValue * multiple;
            this.CollectionStorage.Score += value;
        }
        public InfoPersonal GetInfo(string key)
        {
            string decryptUserName = UserName;
            string decryptEmail = Email;

            try
            {
                decryptUserName = AesHelper.DecryptData(this.UserName, key, this.IV);
                decryptEmail = AesHelper.DecryptData(this.Email, key, this.IV);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new InfoPersonal()
            {
                UserName = decryptUserName,
                Email = decryptEmail,
                //Avatar = this.Avatar,
                DateOfBirth = this.DateOfBirth,
                Setting = this.Setting
            };
        }
        public InforUserRanking GetInfoScore()
        {
            return new InforUserRanking()
            {
                Score = this.CollectionStorage.Score,
                SeqId = this.SeqId,
                UserName = this.UserName,
                Email = this.Email,
                //Avatar = this.Avatar,
                DateOfBirth = this.DateOfBirth
            };
        }

        public void EncryptInfo(string key)
        {
            this.UserName = AesHelper.EncryptDataToBase64(this.UserName, key, this.IV);
            this.Email = AesHelper.EncryptDataToBase64(this.Email, key, this.IV);
        }

        public void GenIV()
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateIV();
                byte[] iv = aes.IV;
                this.IV = Convert.ToBase64String(iv);
            }
        }
        public byte[] GetIVByteArr()
        {
            return Convert.FromBase64String(IV);
        }
        public User ToUserDecrypt(string key)
        {
            var info = GetInfo(key);
            this.UserName = info.UserName;
            this.Email = info.Email;

            return this;
        }
    }

    [System.Serializable]
    public class UserSignUp
    {
        public string LoginName { get; set; } = string.Empty;
        public string LoginPassword { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = "1999-01-01";
    }

    [System.Serializable]
    public class UserLoginRequest
    {
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        [BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
    }

    [System.Serializable]
    public class ChangePasswordRequest
    {
        [BsonElement("old_password")] public string OldPassword { get; set; } = string.Empty;
        [BsonElement("new_password")] public string NewPassword { get; set; } = string.Empty;
    }

    [System.Serializable]
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

        public UserRespone(User user)
        {
            this.Id = user.Id;
            this.SeqId = user.SeqId;
            this.LoginName = user.LoginName;
            this.UserName = user.UserName;
            this.Email = user.Email;
            //this.Avatar = user.Avatar;
            this.DateOfBirth = user.DateOfBirth;
            this.TimeCreated = user.TimeCreated;
            this.Documents = user.Documents;
            this.Setting = user.Setting;
        }
    }

    [System.Serializable]
    public class InfoPersonal
    {
        [BsonElement("user_name")] public string UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string Email { get; set; } = string.Empty;
        //[BsonElement("avatar")] public string Avatar { get; set; } = string.Empty;
        [BsonElement("date_of_birth")] public string DateOfBirth { get; set; } = string.Empty;
        [BsonElement("setting")] public UserSetting? Setting { get; set; } = new UserSetting();
        //[BsonElement("avatar")] public List<int>? Avatar { get; set; } = new List<int>();
    }

    [System.Serializable]
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

    [System.Serializable]
    public class UserCollection
    {
        [BsonElement("create_set_count")] public int CreateSetCount { get; set; } = 0;
        [BsonElement("score")] public int Score { get; set; } = 0;
    }
}
