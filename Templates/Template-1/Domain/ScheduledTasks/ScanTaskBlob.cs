#region (c) 2010-2011 Lokad CQRS - New BSD License 
// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/
#endregion

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Domain.Design;
using Domain.Messages;
using Lokad.Cqrs;
using Lokad.Cqrs.Feature.StreamingStorage;

namespace Domain.ScheduledTasks
{
    public class ScanTaskBlob : IEngineProcess
    {
        readonly IBusClient _bus;
        readonly IStreamingRoot _storage;

        public ScanTaskBlob(IBusClient bus, IStreamingRoot storage)
        {
            _bus = bus;
            _storage = storage;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            var timeSpan = Happen();
                            token.WaitHandle.WaitOne(timeSpan);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                            token.WaitHandle.WaitOne(TimeSpan.FromMinutes(2));
                        }
                    }
                });
        }

        TimeSpan Happen()
        {
            var container = _storage.GetContainer(IdFor.SettingsContainer);
            container.Create();
            var tasksItem = container.GetItem(IdFor.TasksBlob);

            if (!tasksItem.GetInfo().HasValue)
                return TimeSpan.FromSeconds(60);

            _bus.Send(new RunTaskCommand(1));

            tasksItem.Delete();

            return TimeSpan.FromSeconds(60);
        }
    }
}
