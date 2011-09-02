#region (c) 2010-2011 Lokad CQRS - New BSD License 
// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/
#endregion

using System;
using System.Diagnostics;
using System.Text;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Feature.StreamingStorage;
using ServiceStack.Text;

namespace ConsoleRunner.Diagnostics
{
    public sealed class Quarantine : IEnvelopeQuarantine
    {
        readonly IStreamingContainer _container;
        readonly MemoryQuarantine _quarantine = new MemoryQuarantine();

        public Quarantine(IStreamingRoot root)
        {
            _container = root.GetContainer(IdFor.ErrorBlob).Create();
        }

        public bool TryToQuarantine(EnvelopeTransportContext context, Exception ex)
        {
            var quarantined = _quarantine.TryToQuarantine(context, ex);

            try
            {
                var item = GetStreamingItem(context);
                var data = "";
                try
                {
                    data = item.ReadText();
                }
                catch (StreamingItemNotFoundException)
                {
                }

                var builder = new StringBuilder(data);
                if (builder.Length == 0)
                {
                    DescribeMessage(builder, context);
                }

                builder.AppendLine("[Exception]");
                builder.AppendLine(DateTime.UtcNow.ToString());
                builder.AppendLine(ex.ToString());
                builder.AppendLine();

                var text = builder.ToString();
                item.WriteText(text);
            }
            catch (Exception x)
            {
                Trace.WriteLine(x.ToString());
            }

            return quarantined;
        }

        IStreamingItem GetStreamingItem(EnvelopeTransportContext context)
        {
            var createdOnUtc = context.Unpacked.CreatedOnUtc;

            var file = string.Format("{0:yyyy-MM-dd-HH-mm}-{1}-engine.txt",
                createdOnUtc,
                context.Unpacked.EnvelopeId.ToLowerInvariant());

            return _container.GetItem(file);
        }

        public void TryRelease(EnvelopeTransportContext context)
        {
            _quarantine.TryRelease(context);
        }

        static void DescribeMessage(StringBuilder builder, EnvelopeTransportContext context)
        {
            builder.AppendLine(string.Format("{0,12}: {1}", "Queue", context.QueueName));
            builder.AppendLine(context.Unpacked.PrintToString(TypeSerializer.SerializeToString));
        }
    }
}