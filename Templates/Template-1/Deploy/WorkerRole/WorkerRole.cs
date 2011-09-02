#region (c) 2010-2011 Lokad CQRS - New BSD License 
// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/
#endregion

using System;
using System.Linq;
using System.Net;
using System.Threading;
using ConsoleRunner;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        readonly CancellationTokenSource _source = new CancellationTokenSource();

        CqrsEngineHost _host;

        public override void Run()
        {
            _host.Start(_source.Token);
            _source.Token.WaitHandle.WaitOne();
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 48;

            RoleEnvironment.Changing += RoleEnvironmentChanging;
            var cache = RoleEnvironment.GetLocalResource("LocalStorage");

            string connectionString;
            if (!AzureSettingsProvider.TryGetString(IdFor.StorageConnectionValueName, out connectionString))
                throw new InvalidOperationException("Storage connection string not found.");

            _host = BuildBusForWorker.Configure(cache.RootPath, connectionString).Build();

            return base.OnStart();
        }

        public override void OnStop()
        {
            _source.Cancel();
            base.OnStop();
        }

        static void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }
    }
}
