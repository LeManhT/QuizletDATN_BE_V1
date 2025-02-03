using ThirdParty.Json.LitJson;

namespace Quizlet_App_Server.Models.Helper
{
    [System.Serializable]
    public class MessageRespone<T>
    {
        public Status status;
        public string message = string.Empty;
        public T data;
    }

    public enum Status
    {
        OK = 200,
        NOT_FOUND = 404,
        BAD_REQUEST = 400
    }
}
