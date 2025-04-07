using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Quizlet_App_Server.Src.Models
{
    [BsonIgnoreExtraElements]
    public class Documents
    {
        [BsonElement("folders")] public List<Folder> Folders { get; set; } = new List<Folder>();
        [BsonElement("study_sets")] public List<StudySet> StudySets { get; set; } = new List<StudySet>();
        [BsonElement("flash_cards")] public List<FlashCard> FlashCards { get; set; } = new List<FlashCard>();

        public List<StudySet> GetAllSets()
        {
            List<StudySet> allSets = new List<StudySet>();
            foreach (Folder folder in Folders)
            {
                allSets.AddRange(folder.StudySets);
            }

            allSets.AddRange(StudySets);

            return allSets;
        }

        public List<FlashCard> GetAllCards()
        {
            List<FlashCard> allCards = new List<FlashCard>();
            List<StudySet> allSets = GetAllSets();
            foreach (StudySet set in allSets)
            {
                allCards.AddRange(set.Cards);
            }

            allCards.AddRange(allCards);
            allCards.AddRange(FlashCards);

            return allCards;
        }
        public List<StudySet> GetAllSetsInFolder(string idFolderOwner)
        {
            Folder folderOwner = Folders.Find(folder => folder.Id.Equals(idFolderOwner));

            if (folderOwner != null) return folderOwner.StudySets;
            else return null;
        }
        public List<FlashCard> GetAllCardsInSet(string idSetOwner)
        {
            StudySet setOwner = StudySets.Find(set => set.Id.Equals(idSetOwner));

            if (setOwner != null) return setOwner.Cards;
            else return null;
        }
        public Folder GetFolderOwnerOfSet(string idSet)
        {
            foreach (StudySet set in StudySets)
            {
                if (set.Id.Equals(idSet))
                    return null;
            }

            foreach (Folder folder in Folders)
            {
                if (folder.StudySets.Any(set => set.Id.Equals(idSet)))
                    return folder;
            }

            return null;
        }
    }
}
