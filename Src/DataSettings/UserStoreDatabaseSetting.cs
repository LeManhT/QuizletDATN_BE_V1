
namespace Quizlet_App_Server.DataSettings
{
    public class UserStoreDatabaseSetting : IStoreDatabaseSetting
    {
        public string CollectionName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}
