using MongoDB.Driver;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Service
{
    public class CommentService
    {
        private readonly IMongoCollection<Comment> _commentsCollection;
        private readonly IMongoClient client;
        private readonly IConfiguration configuration;
        private readonly IMongoCollection<Post> _postsCollection;

        public CommentService(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            _commentsCollection = database.GetCollection<Comment>("comments");
            _postsCollection = database.GetCollection<Post>("posts");
            client = mongoClient;
            configuration = config;
        }

        // Thêm bình luận
        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            await _commentsCollection.InsertOneAsync(comment);

            // Nếu là phản hồi, cập nhật RepliesCount của comment cha
            if (comment.ParentCommentId != null)
            {
                await _commentsCollection.UpdateOneAsync(
                    Builders<Comment>.Filter.Eq(c => c.Id, comment.ParentCommentId.ToString()),
                    Builders<Comment>.Update.Inc(c => c.RepliesCount, 1)
                ); 
            }

            // Cập nhật bài viết (post) với ID của comment mới
            var postUpdate = Builders<Post>.Update.Push("Comments", comment);
            await _postsCollection.UpdateOneAsync(
                Builders<Post>.Filter.Eq(p => p.Id, comment.PostId),
                postUpdate
            );

            return comment;
        }

        // Lấy danh sách comment gốc theo bài viết
        public async Task<List<Comment>> GetRootCommentsAsync(string postId)
        {
            return await _commentsCollection.Find(c => c.PostId == postId && c.ParentCommentId == null && !c.IsDeleted)
                                            .SortByDescending(c => c.CreatedAt)
                                            .ToListAsync();
        }

        // Lấy danh sách phản hồi theo comment cha
        public async Task<List<Comment>> GetRepliesAsync(string parentCommentId)
        {
            return await _commentsCollection.Find(c => c.ParentCommentId == parentCommentId && !c.IsDeleted)
                                            .SortBy(c => c.CreatedAt)
                                            .ToListAsync();
        }

        // Xóa bình luận (xóa mềm)
        public async Task<bool> DeleteCommentAsync(string commentId)
        {
            var result = await _commentsCollection.UpdateOneAsync(
                Builders<Comment>.Filter.Eq(c => c.Id, commentId),
                Builders<Comment>.Update.Set(c => c.IsDeleted, true)
            );

            return result.ModifiedCount > 0;
        }
    }
}
