
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Src.Features.Payment.Service;

namespace Quizlet_App_Server.Src.Features.Payment.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class MomoPaymentController : ControllerBase
    {
        private readonly MomoPaymentService _momoPaymentService;

        public MomoPaymentController(MomoPaymentService momoPaymentService)
        {
            _momoPaymentService = momoPaymentService;
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDto dto)
        {
            try
            {
                var response = await _momoPaymentService.CreatePaymentAsync(dto.Amount, dto.OrderInfo, dto.ReturnUrl, dto.NotifyUrl);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception hihiiii: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class PaymentRequestDto
    {
        public long Amount { get; set; }
        public string OrderInfo { get; set; }
        public string ReturnUrl { get; set; }
        public string NotifyUrl { get; set; }
    }

}