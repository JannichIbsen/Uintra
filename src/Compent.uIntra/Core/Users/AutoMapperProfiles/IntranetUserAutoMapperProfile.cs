using AutoMapper;
using uIntra.Core.User;
using uIntra.Users;

namespace Compent.uIntra.Core.Users
{
    public class IntranetUserAutoMapperProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<IntranetUser, ProfileViewModel>()
                .ForMember(dst => dst.EditingUser, o => o.MapFrom(user => user));

            Mapper.CreateMap<IntranetUser, ProfileEditModel>()
                .ForMember(dst => dst.MediaRootId, o => o.Ignore())
                .ForMember(dst => dst.NewMedia, o => o.Ignore())
                .ForMember(dst => dst.MemberNotifierSettings, o => o.Ignore());

            Mapper.CreateMap<IntranetUser, IntranetUserDTO>()
                .ForMember(dst => dst.DeleteMedia, o => o.Ignore())
                .ForMember(dst => dst.NewMedia, o => o.Ignore());

            base.Configure();
        }
    }
}