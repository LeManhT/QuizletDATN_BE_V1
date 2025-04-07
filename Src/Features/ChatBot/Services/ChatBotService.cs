using MongoDB.Driver;
using Quizlet_App_Server.Utility;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Quizlet_App_Server.Src.Features.ChatBot.Controllers;
using Quizlet_App_Server.Src.Features.ChatBot.Models;

namespace Quizlet_App_Server.Src.Features.ChatBot.Services
{
    public class ChatBotService
    {
        private readonly IMongoCollection<ChatBotMessage> _chatBotCollection;
        private readonly IMongoClient client;
        private readonly IConfiguration configuration;
        private readonly ILogger<ChatBotController> logger;
        private readonly HttpClient httpClient;

        public ChatBotService(IMongoClient client, IConfiguration configuration, ILogger<ChatBotController> logger)
        {
            var database = client.GetDatabase(VariableConfig.DatabaseName);
            _chatBotCollection = database.GetCollection<ChatBotMessage>("chatBot");
            this.client = client;
            this.configuration = configuration;
            httpClient = new HttpClient();
            this.logger = logger;
        }
        // save message chat bot to DB
        public async Task saveChatBotMessageAsync(ChatBotMessage chatBotMessage)
        {

            await _chatBotCollection.InsertOneAsync(chatBotMessage);
        }

        // get list history chat 

        public async Task<List<ChatBotMessage>> getListChatHistoryAsync()
        {
            return await _chatBotCollection.Find(_ => true)
                .SortByDescending(chat => chat.Timestamp).ToListAsync();
        }

        // Gửi yêu cầu đến dịch vụ AI (ví dụ OnionGPT) => Phản hồi từ AI.
        //    public async Task<string?> GetAIResponseAsync(string userMessage)
        //    {
        //        //var apiKey = configuration[VariableConfig.API_GPT_SECRET_KEY]; 
        //        //var endpoint = configuration[VariableConfig.ONIONGPT_ENDPOINT];      

        //        var apiKey = configuration["OpenAI:ApiKey"];;
        //        var endpoint = "https://api.openai.com/v1/chat/completions";

        //        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(endpoint))
        //            throw new InvalidOperationException("OnionGPT configuration is missing!");

        //        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        //        var payload = new
        //        {
        //            model = "gpt-3.5-turbo", // Hoặc model bạn muốn sử dụng
        //            messages = new[]
        //        {
        //            new { role = "user", content = userMessage }
        //}
        //        };

        //        var content = new StringContent(
        //            JsonSerializer.Serialize(payload),
        //            Encoding.UTF8,
        //            "application/json"
        //        );

        //        // Gửi POST request
        //        var response = await httpClient.PostAsync(endpoint, content);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            logger?.LogError("AI API request failed. Status Code: {StatusCode}, Response: {Response}",
        //                response.StatusCode, errorContent);
        //            return null;
        //        }


        //        var responseBody = await response.Content.ReadAsStringAsync();
        //        var jsonDoc = JsonDocument.Parse(responseBody);

        //        return jsonDoc.RootElement.GetProperty("choices").GetString();
        //    }
    }
}
