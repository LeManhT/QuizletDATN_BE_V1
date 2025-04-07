using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Models
{
    [BsonIgnoreExtraElements]
    public class Folder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
        [BsonElement("count_sets")]
        public int CountSets
        {
            get
            {
                if (StudySets == null) return 0;
                else return StudySets.Count;
            }
        }
        [BsonElement("study_sets")] public List<StudySet> StudySets { get; set; } = new List<StudySet>();

        public Folder() { }
        public Folder(FolderDTO dto)
        {
            Name = dto.Name;
            Description = dto.Description;
            StudySets = dto.StudySets;
        }
        /*        public void AddNewSet(StudySetDTO newSet)
                {
                    newSet.IdFolderOwner = this.Id;
                    StudySets.Add(new StudySet(newSet));
                }*/
        public void AddNewSet(StudySet newSet)
        {
            newSet.IdFolderOwner = Id;
            StudySets.Add(newSet);
        }
    }

    [Serializable]
    public class FolderDTO
    {
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
        [BsonElement("study_sets")] public List<StudySet>? StudySets { get; set; } = new List<StudySet>();
    }

    [Serializable]
    public class FolderShareView
    {
        public string IdOwner = string.Empty;
        public string NameOwner { get; set; } = string.Empty;
        //public string AvatarOwner { get; set; } = string.Empty;
        public List<int> AvatarOwner { get; set; } = new List<int>();
        public string Name { get; set; } = string.Empty;
        public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        public string Description { get; set; } = string.Empty;
        public int? CountSet { get => StudySets != null ? StudySets.Count : 0; }
        public List<StudySet> StudySets { get; set; } = new List<StudySet>();

        public FolderShareView(string idOwner, string nameOwner/*, List<int> avatarOwner*/, Folder folder)
        {
            IdOwner = idOwner;
            NameOwner = nameOwner;
            //AvatarOwner = avatarOwner;
            Name = folder.Name;
            TimeCreated = folder.TimeCreated;
            Description = folder.Description;
            StudySets = folder.StudySets;
        }
    }
}
