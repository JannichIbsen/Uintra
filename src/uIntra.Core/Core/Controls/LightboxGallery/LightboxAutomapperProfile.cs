﻿using AutoMapper;
using uIntra.Core.Extentions;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace uIntra.Core.Controls.LightboxGallery
{
    public class LightboxAutoMapperProfile: Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<IPublishedContent, LightboxGalleryViewModel>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Url, o => o.MapFrom(s => s.Url))
                .ForMember(d => d.Extention, o => o.MapFrom(s => s.GetMediaExtention()))
                .ForMember(d => d.Type, o => o.MapFrom(s => s.GetMediaType()))
                .ForMember(d => d.Width, o => o.Ignore())
                .ForMember(d => d.Height, o => o.Ignore())
                .ForMember(d => d.PreviewUrl, o => o.Ignore())
                .ForMember(d => d.IsHidden, o => o.Ignore())
                .AfterMap((s, d) =>
                {
                    if (d.Type == MediaTypeEnum.Image)
                    {
                        d.Height = s.GetPropertyValue<int>(UmbracoAliases.Media.MediaHeight);
                        d.Width = s.GetPropertyValue<int>(UmbracoAliases.Media.MediaWidth);
                        d.PreviewUrl = s.GetCropUrl(UmbracoAliases.GalleryPreviewImageCrop);
                    }
                });
        }
    }
}