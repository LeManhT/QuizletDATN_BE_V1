using Microsoft.AspNetCore.SignalR;
using Quizlet_App_Server.Src.DTO;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Src.Features.Social.Service;

namespace Quizlet_App_Server.Src
{
    public class PostHub : Hub
    {
        private readonly PostService _postService;
        private readonly ILogger<PostHub> _logger;

        public PostHub(PostService postService, ILogger<PostHub> logger)
        {
            _postService = postService;
            _logger = logger;
            _logger.LogInformation("PostHub initialized.");
        }

        public async Task CreatePost(PostDTO post)
        {
            try
            {
                var savedPost = await _postService.SavePostAsync(post);
                await Clients.All.SendAsync("ReceivePost", savedPost);
                _logger.LogInformation("New post created: {PostId}", savedPost.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating post: {Message}", ex.Message);
            }
        }

        public async Task<Comment?> AddComment(string postId, CommentDTO comment)
        {
            try
            {
                var savedComment = await _postService.AddCommentAsync(postId, comment);
                if (savedComment == null)
                {
                    _logger.LogWarning("Failed to add comment for post: {PostId}", postId);
                    return null; // Trả về null nếu không lưu được comment
                }

                await Clients.All.SendAsync("ReceiveComment", savedComment);
                _logger.LogInformation("New comment added: {CommentId}", savedComment.Id);
                return savedComment; // Trả về comment đã lưu
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding comment: {Message}", ex.Message);
                return null; // Trả về null nếu có lỗi
            }
        }


        public async Task LikePost(string postId, string userId)
        {
            try
            {
                var likeResult = await _postService.LikePostAsync(postId, userId);
                await Clients.All.SendAsync("PostLiked", postId, userId);
                _logger.LogInformation("Post {PostId} liked by {UserId}", postId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error liking post: {Message}", ex.Message);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        }
    }
}
