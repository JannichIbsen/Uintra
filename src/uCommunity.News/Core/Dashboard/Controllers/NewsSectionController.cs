using System;
using System.Collections.Generic;
using System.Web.Http;
using uCommunity.Core.Extentions;
using Umbraco.Web.WebApi;

namespace uCommunity.News.Dashboard
{
    public class NewsSectionController : UmbracoAuthorizedApiController
    {
        private readonly INewsService<NewsBase, NewsModelBase> _newsService;

        public NewsSectionController(INewsService<NewsBase, NewsModelBase> newsService)
        {
            _newsService = newsService;
        }

        public IEnumerable<NewsBackofficeViewModel> GetAll()
        {
            var news = _newsService.GetAll(true);
            var result = news.Map<IEnumerable<NewsBackofficeViewModel>>();
            return result;
        }

        [HttpPost]
        public NewsBackofficeViewModel Create(NewsBackofficeCreateModel createModel)
        {
            var newsId = _newsService.Create(createModel.Map<News>());
            var createdModel = _newsService.Get(newsId);
            var result = createdModel.Map<NewsBackofficeViewModel>();
            return result;
        }

        [HttpPost]
        public NewsBackofficeViewModel Save(NewsBackofficeSaveModel saveModel)
        {
            _newsService.Save(saveModel.Map<News>());
            var updatedModel = _newsService.Get(saveModel.Id);
            var result = updatedModel.Map<NewsBackofficeViewModel>();
            return result;
        }

        [HttpDelete]
        public void Delete(Guid id)
        {
            _newsService.Delete(id);
        }
    }
}