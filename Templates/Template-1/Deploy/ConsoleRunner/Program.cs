#region (c) 2010-2011 Lokad CQRS - New BSD License 
// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lokad.Cqrs;

namespace ConsoleRunner
{
    class Program
    {
        static void Main()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            Console.WriteLine("Initializing host");

            var localStorage = Path.Combine(Directory.GetCurrentDirectory(), "LocalStorage");
            if (Directory.Exists(localStorage))
                Directory.Delete(localStorage, true);

            Directory.CreateDirectory(localStorage);

            string connectionString;
            if (!AzureSettingsProvider.TryGetString(IdFor.StorageConnectionValueName, out connectionString))
                throw new InvalidOperationException("Storage connection string not found.");

            var builder = BuildBusForWorker.Configure(localStorage, connectionString);

            using (var source = new CancellationTokenSource())
            {
                using (var host = builder.Build())
                {
                    Console.WriteLine("Starting host");
                    var task = host.Start(source.Token);

                    Console.WriteLine("Press enter to quit");
                    Console.ReadLine();

                    source.Cancel();
                    Console.WriteLine("Stopping...");

                    task.Wait();
                    Console.WriteLine("Stopped");
                }
            }
        }
    }
}
