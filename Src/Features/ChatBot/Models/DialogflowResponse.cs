namespace Quizlet_App_Server.Src.Features.ChatBot.Models
{
    public class DialogflowResponse
    {
        public string FulfillmentText { get; set; }

        public DialogflowResponse(string message)
        {
            FulfillmentText = message;
        }
    }

}
