using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
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
    public class PostController : ControllerExtend<Post>
    {
        private readonly PostService postService;
        private readonly S3Service s3Service;
        private readonly ILogger<PostController> logger;

        public PostController(AppConfigResource setting, PostService _postService, S3Service _s3Service, IMongoClient mongoClient, IConfiguration config, ILogger<PostController> logger)
            : base(setting, mongoClient)
        {
            postService = _postService;
            s3Service = _s3Service;
            this.logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File không hợp lệ.");
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            using var stream = file.OpenReadStream();

            var fileUrl = await s3Service.UploadFileAsync(stream, fileName);

            return Ok(new { Url = fileUrl });
        }


        [HttpPost]
        public async Task<ActionResult> CreatePost([FromBody] PostDTO newPost)  
        {
            if (newPost == null || string.IsNullOrWhiteSpace(newPost.Content))
            {
                return BadRequest("Post content is required.");
            }

            newPost.CreatedAt = TimeHelper.UnixTimeNow; // Gắn thời gian tạo bài viết
            newPost.Likes = 0; // Khởi tạo danh sách likes trống


            // Kiểm tra danh sách ImageUrls và FileUrls
            if (newPost.ImageUrls == null)
            {
                newPost.ImageUrls = new List<string>();
            }

            if (newPost.FileUrls == null)
            {
                newPost.FileUrls = new List<string>();
            }


            bool isCreated = await postService.CreatePostAsync(newPost);

            if (!isCreated)
            {
                logger.LogError("Failed to create post : ", isCreated);
                return StatusCode(500, "Failed to create the post.");
            }

            return Ok(newPost);
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleFiles([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("Không có file nào được tải lên.");
            }

            List<string> uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                using var stream = file.OpenReadStream();

                var fileUrl = await s3Service.UploadFileAsync(stream, fileName);
                uploadedUrls.Add(fileUrl);
            }

            return Ok(new { ImageUrls = uploadedUrls });
        }

        [HttpPost]
        public async Task<IActionResult> CheckLikedPosts([FromBody] CheckLikedPostsRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId) || request.PostIds == null || !request.PostIds.Any())
            {
                return BadRequest("Invalid request parameters");
            }

            var likedPostIds = await postService.CheckLikedPostsAsync(request.UserId, request.PostIds);
            return Ok(likedPostIds);
        }

        // Add this request model if not already defined
        public class CheckLikedPostsRequest
        {
            public string UserId { get; set; }
            public List<string> PostIds { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> LikePost(string userId, string postId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(postId))
            {
                return BadRequest("User ID and Post ID are required.");
            }

            var post = await postService.FindPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            // Check if user already liked this post
            if (post.LikedByUsers.Contains(userId))
            {
                return Ok(new { message = "User already liked this post", isLike = true, likes = post.Likes });
            }

            // Use the updated service method
            bool success = await postService.LikePostAsync(postId, userId);

            if (!success)
            {
                return StatusCode(500, "Failed to like the post");
            }

            // Fetch the updated post to return the latest like count
            post = await postService.FindPostByIdAsync(postId);

            return Ok(new { message = "Post liked successfully", isLike = true, likes = post.Likes });
        }

        [HttpPost]
        public async Task<ActionResult> UnlikePost(string userId, string postId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(postId))
            {
                return BadRequest("User ID and Post ID are required.");
            }

            var post = await postService.FindPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            // Check if user hasn't liked this post
            if (!post.LikedByUsers.Contains(userId))
            {
                return Ok(new { message = "User hasn't liked this post", isLike = false, likes = post.Likes });
            }

            // Use the updated service method
            bool success = await postService.UnlikePostAsync(postId, userId);

            if (!success)
            {
                return StatusCode(500, "Failed to unlike the post");
            }

            // Fetch the updated post to return the latest like count
            post = await postService.FindPostByIdAsync(postId);

            return Ok(new { message = "Post unliked successfully", isLike = false, likes = post.Likes });
        }

        [HttpGet]
        public async Task<ActionResult<List<Post>>> GetPosts(int page, int pageSize)
        {
            var posts = await postService.GetPaginatedPostsAsync(page, pageSize);
            return Ok(posts);
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> FindPostById(string postId)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return BadRequest("Post ID must not be null or empty.");
            }

            var post = await postService.FindPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found.");
            }

            return Ok(post);
        }


        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(string postId, [FromBody] PostDTO updatedPost)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return BadRequest("Post ID must not be null or empty.");
            }

            if (updatedPost == null)
            {
                return BadRequest("Updated post data must not be null.");
            }

            var success = await postService.UpdatePostAsync(postId, updatedPost);
            if (!success)
            {
                return NotFound("Post not found or update failed.");
            }

            var updatedPostResult = await postService.GetPostByIdAsync(postId);
            return Ok(updatedPostResult);

            //return NoContent();
        }

    }
}
