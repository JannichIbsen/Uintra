//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder v3.0.5.96
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.ModelsBuilder;
using Umbraco.ModelsBuilder.Umbraco;
using Compent.uIntra.Core.UmbracoModelsBuilders;

namespace Umbraco.Web.PublishedContentModels
{
	/// <summary>Bulletins Overview Page</summary>
	[PublishedContentModel("bulletinsOverviewPage")]
	public partial class BulletinsOverviewPage : BasePageWithGrid, IHomeNavigationComposition, INavigationComposition
	{
#pragma warning disable 0109 // new is redundant
		public new const string ModelTypeAlias = "bulletinsOverviewPage";
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
#pragma warning restore 0109

		public BulletinsOverviewPage(IPublishedContent content)
			: base(content)
		{ }

#pragma warning disable 0109 // new is redundant
		public new static PublishedContentType GetModelContentType()
		{
			return PublishedContentType.Get(ModelItemType, ModelTypeAlias);
		}
#pragma warning restore 0109

		public static PublishedPropertyType GetModelPropertyType<TValue>(Expression<Func<BulletinsOverviewPage, TValue>> selector)
		{
			return PublishedContentModelUtility.GetModelPropertyType(GetModelContentType(), selector);
		}

		///<summary>
		/// Is show in Home Navigation
		///</summary>
		[ImplementPropertyType("isShowInHomeNavigation")]
		public bool IsShowInHomeNavigation
		{
			get { return HomeNavigationComposition.GetIsShowInHomeNavigation(this); }
		}

		///<summary>
		/// Is hide from Left Navigation
		///</summary>
		[ImplementPropertyType("isHideFromLeftNavigation")]
		public bool IsHideFromLeftNavigation
		{
			get { return NavigationComposition.GetIsHideFromLeftNavigation(this); }
		}

		///<summary>
		/// Is hide from Sub Navigation
		///</summary>
		[ImplementPropertyType("isHideFromSubNavigation")]
		public bool IsHideFromSubNavigation
		{
			get { return NavigationComposition.GetIsHideFromSubNavigation(this); }
		}

		///<summary>
		/// Navigation Name
		///</summary>
		[ImplementPropertyType("navigationName")]
		public string NavigationName
		{
			get { return NavigationComposition.GetNavigationName(this); }
		}
	}
}
