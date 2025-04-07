using Microsoft.AspNetCore.Mvc;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.Features.ChatBot.Models;

namespace Quizlet_App_Server.Src.Features.Social.Controller
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly UserService _userService;

        public WebhookController(UserService userService)
        {
            _userService = userService;
        }
        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] DialogflowRequest request)
        {
            try
            {
                string userQuery = request?.QueryResult?.QueryText;
                string subject = request?.QueryResult?.Parameters?["search_text"];

                if (string.IsNullOrEmpty(subject))
                {
                    return Ok(new DialogflowResponse("Bạn muốn tìm chủ đề gì?"));
                }

                var studySets = await _userService.FindStudySetsByTextAsync(subject);

                if (studySets == null || !studySets.Any())
                {
                    return Ok(new DialogflowResponse($"Không tìm thấy study set nào cho chủ đề {subject}"));
                }

                string resultText = "Tôi tìm thấy các study set sau:\n";
                foreach (var studySet in studySets)
                {
                    resultText += $"- {studySet.Name}: {studySet.Description}\n";
                }

                return Ok(new DialogflowResponse(resultText));
            }
            catch
            {
                return StatusCode(500, new DialogflowResponse("Có lỗi xảy ra, vui lòng thử lại!"));
            }
        }

    }
}
