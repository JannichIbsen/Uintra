﻿using System.Linq;
using System.Web.Mvc;
using Uintra.Core.Constants;
using Uintra.Core.Extensions;
using Uintra.Core.Media;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace Uintra.Core.Controls.FileUpload
{
    public abstract class FileUploadControllerBase : SurfaceController
    {
        private readonly UmbracoHelper _umbracoHelper;
        private readonly IImageHelper _imageHelper;

        protected FileUploadControllerBase(UmbracoHelper umbracoHelper, IImageHelper imageHelper)
        {
            _umbracoHelper = umbracoHelper;
            _imageHelper = imageHelper;
        }

        public virtual ActionResult Uploader(FileUploadSettings settings)
        {
            return View("~/App_Plugins/Core/Controls/FileUpload/FileUploadView.cshtml", settings);
        }

        public virtual ActionResult Editor(FileUploadSettings settings, string model)
        {
            var mediaIds = model.ToIntCollection();
            var media = _umbracoHelper.TypedMedia(mediaIds);
            var files = media.Select(MapToFileModel);

            var viewModel = new FileUploadEditViewModel
            {
                Settings = settings,
                Files = files
            };

            return View("~/App_Plugins/Core/Controls/FileUpload/FileUploadEditView.cshtml", viewModel);
        }

        protected virtual FileViewModel MapToFileModel(IPublishedContent content)
        {
            var mediaType = content.GetMediaType();
            var url = mediaType is MediaTypeEnum.Video ?
                content.GetPropertyValue<string>(UmbracoAliases.Video.ThumbnailUrlPropertyAlias) :
                content.Url;

            return new FileViewModel
            {
                Id = content.Id,
                Url = _imageHelper.GetImageWithPreset(url, UmbracoAliases.ImagePresets.Thumbnail),
                Extension = content.GetMediaExtension(),
                Type = mediaType,
                FileName = content.Name
            };
        }
    }
}