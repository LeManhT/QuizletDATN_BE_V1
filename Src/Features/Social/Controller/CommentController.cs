using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Src.Controllers;
using Quizlet_App_Server.Src.DataSettings;
using Quizlet_App_Server.Src.DTO;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Src.Features.Social.Service;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentController : ControllerExtend<Comment>
    {
        private readonly CommentService _commentService;
        public CommentController(AppConfigResource setting, IMongoClient mongoClient, IConfiguration config)
           : base(setting, mongoClient)
        {
            _commentService = new CommentService(mongoClient, config);
        }


        // Thêm bình luận
        [HttpPost("add")]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            if (string.IsNullOrEmpty(request.PostId) || string.IsNullOrEmpty(request.AuthorId) || string.IsNullOrEmpty(request.Content))
            {
                return BadRequest("Missing required fields");
            }

            var newComment = new Comment
            {
                PostId = request.PostId,
                AuthorId = request.AuthorId,
                Content = request.Content,
                CreatedAt = TimeHelper.UnixTimeNow,
                ParentCommentId = request.ParentCommentId
            };

            var addedComment = await _commentService.AddCommentAsync(newComment);
            return Ok(addedComment);
        }

        // Lấy danh sách comment gốc
        [HttpGet("{postId}/root-comments")]
        public async Task<IActionResult> GetRootComments(string postId)
        {
            var rootComments = await _commentService.GetRootCommentsAsync(postId);
            return Ok(rootComments);
        }

        // Lấy danh sách phản hồi
        [HttpGet("{commentId}/replies")]
        public async Task<IActionResult> GetReplies(string commentId)
        {
            var replies = await _commentService.GetRepliesAsync(commentId);
            return Ok(replies);
        }

        // Xóa bình luận
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            var isDeleted = await _commentService.DeleteCommentAsync(commentId);

            if (!isDeleted)
            {
                return NotFound("Comment not found or already deleted");
            }

            return Ok("Comment deleted successfully");
        }
    }
}
