#region Copyright (c) 2009-2011 LOKAD SAS. All rights reserved.

// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using ConsoleRunner.Diagnostics;
using ConsoleRunner.Routing;
using ConsoleRunner.Storage;
using Domain.Design;
using Domain.Messages;
using Domain.Tasks;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Lokad.Cqrs.Feature.StreamingStorage;
using ServiceStack.Text;

namespace ConsoleRunner
{
    public static class BuildBusForWorker
    {
        public static CqrsEngineBuilder Configure(string localStorage, string storageConnectionString)
        {
            var dev = AzureStorage.CreateConfig(storageConnectionString,
                c => c.ConfigureBlobClient(x => x.ReadAheadInBytes = 0x200000L));

            var builder = new CqrsEngineBuilder();

            JsonSerialization.Set(builder);

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
                            x.CustomSerializer(
                                (o, t, s) => o.SerializeAndFormat(),
                                (t, s) => TypeSerializer.DeserializeFromStream(typeof(object), s));
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