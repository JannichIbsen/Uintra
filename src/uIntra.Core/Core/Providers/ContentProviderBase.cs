using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Uintra.Core
{
    public abstract class ContentProviderBase 
    {
        private readonly UmbracoHelper _umbracoHelper;

        protected ContentProviderBase(UmbracoHelper umbracoHelper)
        {
            _umbracoHelper = umbracoHelper;
        }

        protected virtual IPublishedContent GetContent(IEnumerable<string> xPath) =>
            _umbracoHelper.TypedContentSingleAtXPath(XPathHelper.GetXpath(xPath));

        protected virtual IEnumerable<IPublishedContent> GetDescendants(IEnumerable<string> xPath) =>
            _umbracoHelper.TypedContentAtXPath(XPathHelper.GetDescendantsXpath(xPath));
    }
}