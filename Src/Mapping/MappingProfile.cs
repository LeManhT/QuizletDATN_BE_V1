using AutoMapper;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Src.DTO;
using Quizlet_App_Server.Src.Features.Social.Models;

namespace Quizlet_App_Server.Src.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Message, MessageDTO>();
            CreateMap<MessageDTO, Message>()
                .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.MessageId, opt => opt.Ignore())
                 .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src =>
                src.Attachments != null
                    ? src.Attachments.Select(dto => new Attachment
                    {
                        Type = dto.Type,
                        Url = dto.Url,
                        FileName = dto.FileName,
                        FileSize = dto.FileSize
                    }).ToList()
                    : new List<Attachment>()));

            CreateMap<AttachmentDTO, Attachment>();
            CreateMap<Post, PostDTO>();
            CreateMap<PostDTO, Post>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());



            CreateMap<Comment, CommentDTO>();
            CreateMap<CommentDTO, Comment>();
        }
    }
}
