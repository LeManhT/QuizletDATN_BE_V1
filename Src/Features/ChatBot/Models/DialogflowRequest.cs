namespace Quizlet_App_Server.Src.Features.ChatBot.Models
{
    public class DialogflowRequest
    {
        public QueryResult QueryResult { get; set; }
    }

    public class QueryResult
    {
        public string QueryText { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }

}
