namespace Quizlet_App_Server.Src.DTO
{
    public class CreateGroupConversationRequest
    {
        public string Name { get; set; } // Tên nhóm
        public string CreatedBy { get; set; } // Người tạo
        public string Type { get; set; } = "personal";// Người tạo
        public List<string> Members { get; set; } // Danh sách thành viên
    }
}
