namespace Quizlet_App_Server.Src.Features.Payment.Model
{
    public class CreateMomoPaymentResponse
    {
        public string orderInfo { get; set; }         // Mô tả thông tin đơn hàng
        public string partnerCode { get; set; }       // Mã đối tác (từ MoMo)
        public string ipnUrl { get; set; }            // URL nhận callback từ MoMo
        public string redirectUrl { get; set; }       // URL chuyển hướng khi giao dịch hoàn thành
        public long amount { get; set; }              // Số tiền giao dịch
        public string orderId { get; set; }           // ID đơn hàng duy nhất
        public string requestId { get; set; }         // ID yêu cầu duy nhất
        public string requestType { get; set; }       // Loại yêu cầu thanh toán
        public string partnerName { get; set; }       // Tên đối tác (hiển thị)
        public string storeId { get; set; }           // ID cửa hàng (nếu có)
        public bool autoCapture { get; set; }         // Tự động hoàn thành thanh toán hay không
        public string lang { get; set; }              // Ngôn ngữ (vi hoặc en)
        public string signature { get; set; }         // Chữ ký bảo mật
        public string extraData { get; set; }         // Chữ ký bảo mật
    }

}


