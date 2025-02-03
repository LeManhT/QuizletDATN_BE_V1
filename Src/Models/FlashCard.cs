using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Models
{
    [BsonIgnoreExtraElements]
    public class FlashCard
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("term")] public string Term { get; set; } = string.Empty;
        [BsonElement("definition")] public string Definition { get; set; } = string.Empty;
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        //[BsonElement("is_public")] public bool IsPublic { get; set; } = false;
        [BsonElement("id_set_owner")] public string IdSetOwner { get; set; } = string.Empty;
        [BsonElement("path_image")] public string PathImage { get; set; } = string.Empty;
        public FlashCard() { }
        public FlashCard(FlashCardDTO dto)
        {
            this.Term = dto.Term;
            this.Definition= dto.Definition;
            this.IdSetOwner= dto.IdSetOwner;
            this.PathImage = dto.PathImage;
        }

        public FlashCard Clone(string newId = null)
        {
            FlashCard cardClone = this.MemberwiseClone() as FlashCard;

            if(newId != null)
            {
                cardClone.Id = newId;
            }

            return cardClone;
        }
        public void UpdateInfo(FlashCardDTO cardDTO)
        {
            this.Id = this.Id;                      // not update id
            this.TimeCreated = this.TimeCreated;    // not update time created
            this.Term = cardDTO.Term;
            this.Definition = cardDTO.Definition;
            this.IdSetOwner = cardDTO.IdSetOwner;
        }
    }

    [Serializable]
    public class FlashCardDTO
    {
        public string Id { get; set; } = null;
        [BsonElement("term")] public string Term { get; set; } = string.Empty;
        [BsonElement("definition")] public string Definition { get; set; } = string.Empty;
        [BsonElement("id_set_owner")] public string IdSetOwner { get; set; } = string.Empty;
        [BsonElement("path_image")] public string PathImage { get; set; } = string.Empty;
        //[BsonElement("is_public")] public bool IsPublic { get; set; } = false;
    }
}
