﻿using System;
using System.Collections.Generic;
using Compent.Uintra.Core.Updater.Migrations._0._3._2._0.Steps;

namespace Compent.Uintra.Core.Updater.Migrations._0._3._2._0
{
    public class Migration : IMigration
    {
        private readonly IMigrationStepsResolver _migrationStepsResolver;

        public Version Version => new Version("0.3.2.0");

        public Migration(IMigrationStepsResolver migrationStepsResolver)
        {
            _migrationStepsResolver = migrationStepsResolver;
        }

        private T Resolve<T>() where T : class => _migrationStepsResolver.Resolve<T>();

        public IEnumerable<IMigrationStep> Steps
        {
            get
            {
                yield return Resolve<RemoveTranslationsStep>();
                yield return Resolve<HomePageHideUnnecessaryItemsStep>(); 
            }
        }
    }
}