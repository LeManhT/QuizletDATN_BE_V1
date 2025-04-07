
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Quizlet_App_Server.Src.Features.Payment.Model;

namespace Quizlet_App_Server.Src.Features.Payment.Service
{
    public class MomoPaymentService
    {
        //private readonly ILogger<MomoPaymentService> _logger;
        private static readonly HttpClient client = new HttpClient();

        //    static async Task Main(string[] args)
        //{
        //    Guid myuuid = Guid.NewGuid();
        //    string myuuidAsString = myuuid.ToString();

        //    string accessKey = "F8BBA842ECF85";
        //    string secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";

        //    CreateMomoPaymentResponse request = new CreateMomoPaymentResponse();
        //    request.OrderInfo = "pay with MoMo";
        //    request.PartnerCode = "MOMO";
        //    request.IpnUrl = "https://webhook.site/b3088a6a-2d17-4f8d-a383-71389a6c600b";
        //    request.RedirectUrl = "https://webhook.site/b3088a6a-2d17-4f8d-a383-71389a6c600b";
        //    request.Amount = 5000;
        //    request.OrderId = myuuidAsString;
        //    request.RequestId = myuuidAsString;
        //    request.RequestType = "payWithMethod";
        //    request.PartnerName = "MoMo Payment";
        //    request.StoreId = "Test Store";
        //    request.AutoCapture = true;
        //    request.Lang = "vi";

        //    var rawSignature = "accessKey=" + accessKey + "&amount=" + request.Amount + "&ipnUrl=" + request.IpnUrl + "&orderId=" + request.OrderId + "&orderInfo=" + request.OrderInfo + "&partnerCode=" + request.PartnerCode + "&redirectUrl=" + request.RedirectUrl + "&requestId=" + request.RequestId + "&requestType=" + request.RequestType;
        //    request.Signature = getSignature(rawSignature, secretKey);

        //    StringContent httpContent = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
        //    var quickPayResponse = await client.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", httpContent);
        //    var contents = quickPayResponse.Content.ReadAsStringAsync().Result;
        //    System.Console.WriteLine(contents + "");
        //}

        public async Task<string> CreatePaymentAsync(long amount, string orderInfo, string returnUrl, string notifyUrl)
        {
            Console.WriteLine("CreatePayment");
            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = myuuid.ToString();

            string accessKey = "F8BBA842ECF85";
            string secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";

            // Cập nhật giá trị trường yêu cầu
            CreateMomoPaymentResponse request = new()
            {
                orderInfo = string.IsNullOrEmpty(orderInfo) ? "Payment for goods" : orderInfo,
                partnerCode = "MOMO",
                ipnUrl = string.IsNullOrEmpty(notifyUrl) ? "https://your-notify-url.com" : notifyUrl,
                redirectUrl = returnUrl,
                amount = amount,
                orderId = myuuidAsString,
                requestId = myuuidAsString,
                requestType = "payWithMethod",
                partnerName = "MoMo Payment",
                storeId = "Test Store",
                autoCapture = true,
                lang = "vi",
                extraData = ""
            };

            // Tạo chữ ký
            var rawSignature = "accessKey=" + accessKey +
                               "&amount=" + request.amount +
                               "&extraData=" + request.extraData +  // nếu extraData là chuỗi rỗng, cũng phải bao gồm nó
                               "&ipnUrl=" + request.ipnUrl +
                               "&orderId=" + request.orderId +
                               "&orderInfo=" + request.orderInfo +
                               "&partnerCode=" + request.partnerCode +
                               "&redirectUrl=" + request.redirectUrl +
                               "&requestId=" + request.requestId +
                               "&requestType=" + request.requestType;

            request.signature = getSignature(rawSignature, secretKey);
            StringContent httpContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            Console.WriteLine($"Request payload: {JsonSerializer.Serialize(request)}");

            try
            {
                var quickPayResponse = await client.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", httpContent);

                if (!quickPayResponse.IsSuccessStatusCode)
                {
                    string errorDetails = await quickPayResponse.Content.ReadAsStringAsync();
                    //_logger.LogError($"Error details: {errorDetails}");
                    throw new Exception($"Failed to create payment. StatusCode: {quickPayResponse.StatusCode}. Response: {errorDetails}");
                }

                // Kiểm tra phản hồi từ MoMo
                var responseContent = await quickPayResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Response from MoMo: {responseContent}");
                return responseContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                //_logger.LogError($"An error occurred: {ex.Message}");
                throw new Exception("Payment creation failed", ex);
            }
        }




        private static string getSignature(string text, string key)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] textBytes = encoding.GetBytes(text);
            byte[] keyBytes = encoding.GetBytes(key);
            byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

    }
}
