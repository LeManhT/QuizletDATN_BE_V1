


namespace Quizlet_App_Server.Response
{
    public class MomoPaymentResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string PayUrl { get; set; }
        public string QrCodeUrl { get; set; }
        public string Deeplink { get; set; }
        public string OrderId { get; set; }
    }
}

