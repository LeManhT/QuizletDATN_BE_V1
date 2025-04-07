namespace Quizlet_App_Server.Utility
{
    public class VariableConfig
    {
        public const int MaxTryLogin = 5;
        public const int BaseScore = 5;
        public const string DatabaseName = "AppQuizlet";
        public const string Collection_Users = "users";
        public const string Collection_UserSequence = "UserSequence";
        public const string Collection_Configure = "Configure";
        public const string Collection_Admin = "Admin";
        public const string Collection_StudySetPublic = "StudySetPublic";

        // publish setup
        public const string ResourceSupplierString = "https://bb06-171-224-180-162.ngrok-free.app";
        public const string IdPublish = "pub_v0_0_0";
    }
}
