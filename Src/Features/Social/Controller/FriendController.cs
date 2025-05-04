using Microsoft.AspNetCore.Mvc;
using Quizlet_App_Server.Src.Features.Social.Service;
using Quizlet_App_Server.Src.Models;

namespace Quizlet_App_Server.Src.Features.Social.Controller {
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FriendController : ControllerBase
{
    private readonly FriendService _friendService;

    public FriendController(FriendService friendService)
    {
        _friendService = friendService;
    }

    /// Gửi lời mời kết bạn
    [HttpPost]
    public async Task<IActionResult> SendFriendRequest([FromQuery] string senderId, [FromQuery] string receiverId)
    {
        var success = await _friendService.SendFriendRequestAsync(senderId, receiverId);
        if (!success) return BadRequest("Lời mời đã tồn tại hoặc có lỗi xảy ra.");
        return Ok(new { success = true, message = "Đã gửi lời mời kết bạn." });
    }

        [HttpPost]
        public async Task<IActionResult> AcceptFriendRequest([FromQuery] string requestId)
        {
            var success = await _friendService.AcceptFriendRequestAsync(requestId);
            if (!success) return NotFound("Lời mời kết bạn không tồn tại hoặc đã được xử lý.");
            return Ok(new { success = true, message = "Đã chấp nhận lời mời kết bạn." });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriendRequest([FromQuery] string requestId)
        {
            var success = await _friendService.RemoveFriendRequestAsync(requestId);
            if (!success) return NotFound("Lời mời kết bạn không tồn tại.");
            return Ok(new { success = true, message = "Đã từ chối lời mời kết bạn." });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend([FromQuery] string userId, [FromQuery] string friendId)
        {
            var success = await _friendService.RemoveFriendAsync(userId, friendId);
            if (!success) return NotFound("Người dùng này không có trong danh sách bạn bè.");
            return Ok(new { success = true, message = "Đã xóa bạn bè." });
        }

        /// Lấy danh sách bạn bè của người dùng
        [HttpGet]
        public async Task<ActionResult<List<UserRespone>>> GetUserFriends(string userId)
        {
            try
            {
                var friends = await _friendService.GetFriendsAsync(userId);
                var userResponses = friends.Select(user => new UserRespone(user)).ToList();
                return Ok(userResponses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        //[HttpDelete]
        //public async Task<IActionResult> CancelFriendRequest([FromQuery] string senderId, [FromQuery] string receiverId)
        //{
        //    var result = await _friendService.CancelFriendRequestAsync(senderId, receiverId);
        //    if (!result) return BadRequest("Friend request not found or already accepted.");

        //    return Ok("Friend request canceled successfully.");
        //}

        [HttpGet]
        public async Task<IActionResult> GetReceivedFriendRequests([FromQuery] string receiverId)
        {
            var requests = await _friendService.GetReceivedFriendRequestsAsync(receiverId);
            return Ok(new { success = true, requests });
        }

    }
}
