using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Feature.AtomicStorage;
using ServiceStack.Text;

namespace ConsoleRunner.Storage
{
    internal static class JsonSerialization
    {
        public static void SetForMessages(CqrsEngineBuilder builder)
        {
            builder.Advanced.CustomDataSerializer(t => new JsonDataSerializer(t));
        }

        public static void SetForStorage(DefaultAtomicStorageStrategyBuilder builder)
        {
            builder.CustomSerializer(
                FormattedSerializationHelper.SerializeAndFormat,
                JsonSerializer.DeserializeFromStream);
        }

        class JsonDataSerializer : AbstractDataSerializer
        {
            public JsonDataSerializer(ICollection<Type> knownTypes)
                : base(knownTypes)
            {
            }

            protected override Formatter PrepareFormatter(Type type)
            {
                var name = ContractEvil.GetContractReference(type);
                return new Formatter(name,
                    stream => JsonSerializer.DeserializeFromStream(type, stream),
                    (o, stream) => JsonSerializer.SerializeToStream(o, type, stream));
            }
        }

        static class FormattedSerializationHelper
        {
            public static void SerializeAndFormat(object value, Type type, Stream stream)
            {
                var s = value.SerializeAndFormat();
                var bytes = Encoding.UTF8.GetBytes(s);

                using (var ms = new MemoryStream(bytes))
                {
                    ms.CopyTo(stream);
                }
            }
        }
    }
}
