using System;
using Domain.Design;
using Lokad.Cqrs;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.HandlerClasses;

namespace ConsoleRunner.Routing
{
    class BusClient : IBusClient
    {
        readonly IQueueWriter _commandWriter;
        readonly IQueueWriter _eventWriter;

        public BusClient(IQueueWriter commandWriter, IQueueWriter eventWriter)
        {
            _commandWriter = commandWriter;
            _eventWriter = eventWriter;
        }

        public void Send(params IMessage[] messages)
        {
            foreach (var message in messages)
            {
                if (message is Define.Event)
                {
                    _eventWriter.PutMessage(message.BuildEnvelope());
                    return;
                }

                if (message is Define.Command)
                {
                    _commandWriter.PutMessage(message.BuildEnvelope());
                    return;
                }

                throw new InvalidOperationException(string.Format("Unknown message type {0}.", message.GetType().Name));
            }
        }
    }

    internal static class MessageExtensions
    {
        public static ImmutableEnvelope BuildEnvelope(this IMessage message)
        {
            var eb = new EnvelopeBuilder(Guid.NewGuid().ToString());
            eb.AddItem((object) message);

            return eb.Build();
        }
    }
}
