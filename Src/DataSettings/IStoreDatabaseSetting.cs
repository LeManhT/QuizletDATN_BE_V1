namespace Quizlet_App_Server.DataSettings
{
    public interface IStoreDatabaseSetting
    {
        string CollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
