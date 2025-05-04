using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using Quizlet_App_Server.Src.DTO;
using Quizlet_App_Server.Src.Features.Social.Controller;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Utility;
using System.Xml.Linq;

namespace Quizlet_App_Server.Src.Features.Social.Service
{
    public class PostService
    {
        private readonly IMongoCollection<Post> _postCollection;
        protected readonly IMongoClient client;
        private readonly IConfiguration config;
        private readonly ILogger<PostService> logger;
        public PostService(IMongoClient mongoClient, IConfiguration config, ILogger<PostService> logger)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            _postCollection = database.GetCollection<Post>("posts");
            client = mongoClient;
            this.config = config;
            this.logger = logger;
        }
        // Thêm bài viết mới
        public async Task<bool> CreatePostAsync(PostDTO post)
        {
            try
            {
                var mappedPost = MapPostDtoToPost(post);
                mappedPost.CreatedAt = TimeHelper.UnixTimeNow;

                await _postCollection.InsertOneAsync(mappedPost);
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to create post");
                return false;
            }
        }

        public async Task<Post> SavePostAsync(PostDTO postDto)
        {
            var mappedPost = MapPostDtoToPost(postDto);
            mappedPost.CreatedAt = TimeHelper.UnixTimeNow;

            await _postCollection.InsertOneAsync(mappedPost);
            return mappedPost;
        }
        // Lấy danh sách bài viết
        public async Task<List<Post>> GetPostsAsync()
        {
            return await _postCollection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Post>> GetPostsAsync(int pageNumber, int pageSize)
        {
            return await _postCollection.Find(_ => true)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }


        // Tìm bài viết theo ID
        public async Task<Post?> GetPostByIdAsync(string postId)
        {
            return await _postCollection.Find(post => post.Id == postId).FirstOrDefaultAsync();
        }

        // Thêm bình luận vào bài viết
        public async Task<Comment?> AddCommentAsync(string postId, CommentDTO commentDto)
        {
            var post = await GetPostByIdAsync(postId);
            if (post == null) return null;

            var newComment = new Comment
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Content = commentDto.Content,
                AuthorId = commentDto.AuthorId,
                CreatedAt = TimeHelper.UnixTimeNow
            };

            post.Comments.Add(newComment);

            var result = await _postCollection.ReplaceOneAsync(
                p => p.Id == postId,
                post
            );

            return result.ModifiedCount > 0 ? newComment : null;
        }
        // Add/modify these methods in the PostService class

        public async Task<bool> LikePostAsync(string postId, string userId)
        {
            var post = await _postCollection.Find(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null) return false;

            // Only add the user if they haven't liked the post yet
            if (!post.LikedByUsers.Contains(userId))
            {
                post.LikedByUsers.Add(userId);
                post.Likes = post.LikedByUsers.Count; // Update likes count based on the array

                var result = await _postCollection.ReplaceOneAsync(
                    p => p.Id == postId,
                    post
                );
                return result.ModifiedCount > 0;
            }

            // User already liked the post
            return false;
        }

        public async Task<bool> UnlikePostAsync(string postId, string userId)
        {
            var post = await _postCollection.Find(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null) return false;

            // Only remove if the user has liked the post
            if (post.LikedByUsers.Contains(userId))
            {
                post.LikedByUsers.Remove(userId);
                post.Likes = post.LikedByUsers.Count; // Update likes count based on the array

                var result = await _postCollection.ReplaceOneAsync(
                    p => p.Id == postId,
                    post
                );
                return result.ModifiedCount > 0;
            }

            // User hasn't liked the post
            return false;
        }
        // Quản lý bài viết của User
        //Mô tả: Lấy danh sách bài viết được tạo bởi một người dùng cụ thể.
        public async Task<List<Post>> GetPostsByUserAsync(string userId, int pageNumber, int pageSize)
        {
            return await _postCollection.Find(post => post.AuthorId == userId)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPaginatedPostsAsync(int page, int pageSize)
        {
            if (page <= 0 || pageSize <= 0)
            {
                throw new ArgumentException("Page and page size must be greater than zero.");
            }

            return await _postCollection.Find(_ => true)
                                    .Skip((page - 1) * pageSize)
                                    .Limit(pageSize)
                                    .ToListAsync();
        }


        public async Task<Post> FindPostByIdAsync(string postId)
        {
            if (string.IsNullOrEmpty(postId))
            {
                throw new ArgumentException("Post ID must not be null or empty.");
            }

            return await _postCollection.Find(post => post.Id == postId).FirstOrDefaultAsync();
        }

        public async Task<List<string>> CheckLikedPostsAsync(string userId, List<string> postIds)
        {
            if (string.IsNullOrEmpty(userId) || postIds == null || !postIds.Any())
            {
                return new List<string>();
            }

            var filter = Builders<Post>.Filter.And(
                Builders<Post>.Filter.In(p => p.Id, postIds),
                Builders<Post>.Filter.AnyEq(p => p.LikedByUsers, userId)
            );

            var likedPosts = await _postCollection.Find(filter).ToListAsync();
            return likedPosts.Select(p => p.Id).ToList();
        }

        public async Task<bool> UpdatePostAsync(string postId, PostDTO updatedPost)
        {
            var existingPost = await _postCollection.Find(Builders<Post>.Filter.Eq(p => p.Id, postId)).FirstOrDefaultAsync();
            if (existingPost == null)
            {
                return false;
            }

            existingPost.Content = updatedPost.Content;
            existingPost.Likes = updatedPost.Likes;

            var result = await _postCollection.ReplaceOneAsync(
                Builders<Post>.Filter.Eq(p => p.Id, postId),
                existingPost
            );

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        private Post MapPostDtoToPost(PostDTO postDto)
        {
            return new Post
            {
                Content = postDto.Content,
                AuthorId = postDto.AuthorId,
                CreatedAt = postDto.CreatedAt > 0 ? postDto.CreatedAt : TimeHelper.UnixTimeNow, 
                Comments = postDto.Comments ?? new List<Comment>(),
                Likes = postDto.Likes,
                ImageUrls = postDto.ImageUrls ?? new List<string>(),
                FileUrls = postDto.FileUrls ?? new List<string>()
            };
        }

    }
}