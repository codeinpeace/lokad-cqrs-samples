#region (c) 2010-2011 Lokad CQRS - New BSD License 
// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/
#endregion

using System;
using ConsoleRunner.Diagnostics;
using ConsoleRunner.Routing;
using ConsoleRunner.Storage;
using Domain.Design;
using Domain.Messages;
using Domain.ScheduledTasks;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Lokad.Cqrs.Feature.StreamingStorage;

namespace ConsoleRunner
{
    public static class BuildBusForWorker
    {
        public static CqrsEngineBuilder Configure(string localStorage, string storageConnectionString)
        {
            var dev = AzureStorage.CreateConfig(storageConnectionString,
                c => c.ConfigureBlobClient(x => x.ReadAheadInBytes = 0x200000L));

            var builder = new CqrsEngineBuilder();

            JsonSerialization.SetForMessages(builder);

            builder.MessagesWithHandlersFromAutofac(d =>
                {
                    d.HandlerSample<Define.Handle<Define.Command>>(m => m.Handle(null));
                    d.InAssemblyOf<RunTaskCommand>();
                });

            builder.Azure(m =>
                {
                    m.AddAzureSender(dev, IdFor.CommandsQueue);
                    m.AddAzureProcess(dev, IdFor.CommandsQueue, x =>
                        {
                            x.DispatchAsCommandBatch(f => f.WhereMessagesAre<Define.Command>());
                            x.Quarantine(c => new Quarantine(c.Resolve<IStreamingRoot>()));
                            x.DecayPolicy(TimeSpan.FromSeconds(0.75));
                        });

                    m.AddAzureSender(dev, IdFor.EventsQueue);
                    m.AddAzureProcess(dev, IdFor.EventsQueue, x =>
                        {
                            x.DispatchAsEvents(f => f.WhereMessagesAre<Define.Event>());
                            x.Quarantine(c => new Quarantine(c.Resolve<IStreamingRoot>()));
                            x.DecayPolicy(TimeSpan.FromSeconds(0.75));
                        });
                });

            builder.Storage(m =>
                {
                    m.AtomicIsInAzure(dev, x =>
                        {
                            x.FolderForEntity(t => "template-view-" + t.Name.ToLowerInvariant());
                            x.NameForEntity((t, o) => o.ToString().ToLowerInvariant() + ".json");
                            x.FolderForSingleton("template-singleton");
                            x.NameForSingleton(t => t.Name.ToLowerInvariant() + ".json");
                            JsonSerialization.SetForStorage(x);
                        });
                    m.StreamingIsInAzure(dev);
                });

            builder.Advanced.RegisterQueueWriterFactory(c => new AzureQueueWriterFactory(dev, c.Resolve<IEnvelopeStreamer>()));

            builder.Advanced.ConfigureContainer(cb =>
                {
                    var queueWriterRegistry = cb.Resolve<QueueWriterRegistry>();
                    var factory = queueWriterRegistry.GetOrThrow(dev.AccountName);
                    var client = new BusClient(factory.GetWriteQueue(IdFor.CommandsQueue), factory.GetWriteQueue(IdFor.EventsQueue));

                    cb.Register<IBusClient>(client);

                    var local = new FileStreamingContainer(localStorage);
                    var remote = new BlobStreamingRoot(dev.CreateBlobClient());

                    cb.Register(new StorageProvider(local, remote));
                    cb.Register<IStreamingRoot>(remote);

                    builder.Setup.AddProcess(new ScanTaskBlob(client, remote));
                });

            return builder;
        }
    }
}