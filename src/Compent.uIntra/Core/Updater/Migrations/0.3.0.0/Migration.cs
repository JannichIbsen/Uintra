﻿using System;
using System.Collections.Generic;
using Compent.Uintra.Core.Updater.Migrations._0._3._0._0.Steps;

namespace Compent.Uintra.Core.Updater.Migrations._0._3._0._0
{
    public class Migration : IMigration
    {
        private readonly IMigrationStepsResolver _migrationStepsResolver;

        public Version Version => new Version("0.3.0.0");

        public Migration(IMigrationStepsResolver migrationStepsResolver)
        {
            _migrationStepsResolver = migrationStepsResolver;
        }

        private T Resolve<T>() where T : class => _migrationStepsResolver.Resolve<T>();

        public IEnumerable<IMigrationStep> Steps
        {
            get
            {
                yield return Resolve<GroupDocumentsTabStep>();
                yield return Resolve<NotificationSettingsMigrationStep>();
                yield return Resolve<TablePanelMigrationStep>();   
                yield return Resolve<DeleteGroupNavigationTabsMigrationStep>();        
                yield return Resolve<AddTranslationsStep>();
                yield return Resolve<AddFirstTimeLoginStep>();
                yield return Resolve<WelcomeMessageSetupStep>();
            }
        }
    }
}