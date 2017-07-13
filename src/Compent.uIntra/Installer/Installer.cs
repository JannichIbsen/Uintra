﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Compent.uIntra.SetupMigrations;
using uIntra.Bulletins.Installer;
using uIntra.Core.Installer;
using uIntra.Events.Installer;
using uIntra.Navigation.Installer;
using uIntra.News.Installer;
using uIntra.Notification.Installer;
using uIntra.Search.Installer;
using Umbraco.Core;

namespace Compent.uIntra.Installer
{
    public class Installer: ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            var installer = new IntranetInstaller();
            var umbracoContentMigration = new UmbracoContentMigration();

            installer.Install();
            InheritNavigationCompositions();
            umbracoContentMigration.Init();
        }

        private void InheritNavigationCompositions()
        {
            var homeNav = NavigationInstallationConstants.DocumentTypeAliases.HomeNavigationComposition;
            var nav = NavigationInstallationConstants.DocumentTypeAliases.NavigationComposition;

            CoreInstallationStep.InheritCompositionForPage(CoreInstallationConstants.DocumentTypeAliases.HomePage, nav);
            CoreInstallationStep.InheritCompositionForPage(CoreInstallationConstants.DocumentTypeAliases.ContentPage, nav);
            CoreInstallationStep.InheritCompositionForPage(CoreInstallationConstants.DocumentTypeAliases.ErrorPage, nav);

            CoreInstallationStep.InheritCompositionForPage(SearchInstallationConstants.DocumentTypeAliases.SearchResultPage, nav);
            CoreInstallationStep.InheritCompositionForPage(NotificationInstallationConstants.DocumentTypeAliases.NotificationPage, nav);

            CoreInstallationStep.InheritCompositionForPage(BulletinsInstallationConstants.DocumentTypeAliases.BulletinsOverviewPage, homeNav);
            CoreInstallationStep.InheritCompositionForPage(BulletinsInstallationConstants.DocumentTypeAliases.BulletinsOverviewPage, nav);

            CoreInstallationStep.InheritCompositionForPage(NewsInstallationConstants.DocumentTypeAliases.NewsOverviewPage, homeNav);
            CoreInstallationStep.InheritCompositionForPage(NewsInstallationConstants.DocumentTypeAliases.NewsOverviewPage, nav);

            CoreInstallationStep.InheritCompositionForPage(EventsInstallationConstants.DocumentTypeAliases.EventsOverviewPage, homeNav);
            CoreInstallationStep.InheritCompositionForPage(EventsInstallationConstants.DocumentTypeAliases.EventsOverviewPage, nav);

        }
    }
}