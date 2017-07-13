﻿using uIntra.Core.Installer;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace uIntra.Search.Installer
{
    public class SearchInstallationStep : IIntranetInstallationStep
    {
        public string PackageName => "uIntra.Search";
        public int Priority => 2;

        public void Execute()
        {
            CreateSearchResultPage();
            CreateContentPageUseInSearchTrueFalse();
            AddSearchToContentPage();
        }

        private void CreateSearchResultPage()
        {
            var contentService = ApplicationContext.Current.Services.ContentTypeService;

            var searchResultPage = contentService.GetContentType(SearchInstallationConstants.DocumentTypeAliases.SearchResultPage);
            if (searchResultPage != null) return;

            searchResultPage = CoreInstallationStep.GetBasePageWithGridBase(CoreInstallationConstants.DocumentTypeAliases.BasePageWithGrid);
            //TODO: Move static methods to service

            searchResultPage.Name = SearchInstallationConstants.DocumentTypeNames.SearchResultPage;
            searchResultPage.Alias = SearchInstallationConstants.DocumentTypeAliases.SearchResultPage;
            searchResultPage.Icon = SearchInstallationConstants.DocumentTypeIcons.SearchResultPage;

            contentService.Save(searchResultPage);
            CoreInstallationStep.AddAllowedChildNode(CoreInstallationConstants.DocumentTypeAliases.HomePage, SearchInstallationConstants.DocumentTypeAliases.SearchResultPage);
        }

        private void CreateContentPageUseInSearchTrueFalse()
        {
            CoreInstallationStep.CreateTrueFalseDataType(SearchInstallationConstants.DataTypeNames.ContentPageUseInSearch);
        }

        private void AddSearchToContentPage()
        {
            var contentService = ApplicationContext.Current.Services.ContentTypeService;
            var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
            var contentPage = contentService.GetContentType(CoreInstallationConstants.DocumentTypeAliases.ContentPage);
            if (contentPage == null) return;

            var useInSearchDataType = dataTypeService.GetDataTypeDefinitionByName(SearchInstallationConstants.DataTypeNames.ContentPageUseInSearch);
            var useInSearchProperty = new PropertyType(useInSearchDataType)
            {
                Name = SearchInstallationConstants.DocumentTypePropertyNames.UseInSearch,
                Alias = SearchInstallationConstants.DocumentTypePropertyAliases.UseInSearch
            };
            if (!contentPage.PropertyGroups.Contains("Search"))
            {
                contentPage.AddPropertyGroup("Search");
            }
            if (!contentPage.PropertyTypeExists(useInSearchProperty.Alias))
            {
                contentPage.AddPropertyType(useInSearchProperty, "Search");
            }
            contentService.Save(contentPage);
        }
    }

}
