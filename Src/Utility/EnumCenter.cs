namespace Quizlet_App_Server.Src.Utility
{
    public class EnumCenter
    {
    }

    public enum VerifyLoginResult
    {
        None            = -1,
        Success         = 0,
        
        // Fail
        InvalidUserName = 1,
        InvalidPassword = 2,
        SuspendTemp     = 3    // User.TimeSuspendTemp
    }
}
