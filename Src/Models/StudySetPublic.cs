using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Models
{
    public class StudySetPublic
    {
        [BsonElement("id")] public string Id { get; set;} = string.Empty;
        [BsonElement("id_owner")] public string IdOwner { get; set;} = string.Empty;
        [BsonElement("name_owner")] public string NameOwner { get; set;} = string.Empty;
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("time_pushed")] public long? TimePushed { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("count_term")] public int? CountTerm { get => AllCards != null ? AllCards.Count : 0; }
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
        //[BsonElement("id_folder_owner")] public string IdFolderOwner { get; set; } = string.Empty;
        //[BsonElement("is_public")] public bool IsPublic { get; set; } = false;
        [BsonElement("all_cards")] public List<FlashCard> AllCards { get; set; } = new List<FlashCard>();
        public StudySetPublic(string idOwner, string nameOwner, StudySet set) 
        {
            this.IdOwner = idOwner;
            this.NameOwner = nameOwner;
            this.Id = set.Id;
            this.Name = set.Name;
            this.TimeCreated = set.TimeCreated;
            this.Description = set.Description;
            this.AllCards = set.Cards;
        }
    }
}
