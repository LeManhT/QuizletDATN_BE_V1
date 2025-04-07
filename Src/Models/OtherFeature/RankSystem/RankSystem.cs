using MongoDB.Bson.Serialization.Attributes;

namespace Quizlet_App_Server.Src.Models.OtherFeature.RankSystem
{
    [BsonIgnoreExtraElements]
    public class RankSystem
    {
        public List<InforUserRanking> UserRanking { get; set; } = new List<InforUserRanking>();
    }

    [System.Serializable]
    public class RankResult
    {
        public int CurrentScore { get; set; } = -1;
        public int CurrentRank { get; set; } = -1;
        public RankSystem? RankSystem { get; set; }
    }
}
