using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Models
{
    [BsonIgnoreExtraElements]
    public class StudySet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("id_owner")] public string IdOwner { get; set; } = string.Empty;
        [BsonElement("id_folder_owner")] public string IdFolderOwner { get; set; } = string.Empty;
        [BsonElement("is_public")] public bool IsPublic { get; set; } = false;
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
        [BsonElement("count_term")]
        public int CountTerm
        {
            get
            {
                if (Cards == null) return 0;
                else return Cards.Count;
            }
        }
        [BsonElement("cards")] public List<FlashCard> Cards { get; set; } = new List<FlashCard>();
        public StudySet() { }
        public StudySet(string idOwner, StudySetDTO dto)
        {
            IdOwner = idOwner;
            Name = dto.Name;
            IdFolderOwner = dto.IdFolderOwner;
            IsPublic = dto.IsPublic;
            Description = dto.Description;
            if (dto.AllNewCards != null && dto.AllNewCards.Count > 0)
            {
                Cards = new();
                foreach (FlashCardDTO cardDTO in dto.AllNewCards)
                {
                    cardDTO.IdSetOwner = Id;
                    Cards.Add(new FlashCard(cardDTO));
                }
            }
        }
        public StudySet Clone(string newId = null)
        {
            StudySet setClone = MemberwiseClone() as StudySet;

            if (newId != null)
            {
                setClone.Id = newId;
                List<FlashCard> newListCard = new List<FlashCard>();
                foreach (var card in Cards)
                {
                    var cardClone = card.Clone(ObjectId.GenerateNewId().ToString());
                    cardClone.IdSetOwner = newId;
                    newListCard.Add(cardClone);
                }

                setClone.Cards = newListCard;
            }

            return setClone;
        }
        public void AddNewCard(FlashCardDTO cardDTO)
        {
            cardDTO.IdSetOwner = Id;
            Cards.Add(new FlashCard(cardDTO));
        }
        public void UpdateInfo(StudySetDTO dto)
        {
            Id = Id;
            TimeCreated = TimeCreated;
            Name = dto.Name;
            IdFolderOwner = dto.IdFolderOwner;
            IsPublic = dto.IsPublic;
            Description = dto.Description;
        }
    }

    [Serializable]
    public class StudySetDTO
    {
        public string Id { get; set; } = string.Empty;
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("id_folder_owner")] public string IdFolderOwner { get; set; } = string.Empty;
        [BsonElement("is_public")] public bool IsPublic { get; set; } = false;
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
        [BsonElement("all_new_cards")] public List<FlashCardDTO> AllNewCards { get; set; } = new List<FlashCardDTO>();
    }

    [Serializable]
    public class StudySetShareView
    {
        public string IdOwner { get; set; } = string.Empty;
        public string NameOwner { get; set; } = string.Empty;
        //public string AvatarOwner { get; set;} = string.Empty;
        public List<int> AvatarOwner { get; set; } = new List<int>();
        public string Name { get; set; } = string.Empty;
        public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        public int? CountTerm { get => Cards != null ? Cards.Count : 0; }
        public string Description { get; set; } = string.Empty;
        public List<FlashCard> Cards { get; set; } = new List<FlashCard>();

        public StudySetShareView(string idOwner, string nameOwner, /*List<int> avatarOwner,*/ StudySet set)
        {
            IdOwner = idOwner;
            NameOwner = nameOwner;
            //AvatarOwner = avatarOwner;
            Name = set.Name;
            TimeCreated = set.TimeCreated;
            Description = set.Description;
            Cards = set.Cards;
        }
    }
}
