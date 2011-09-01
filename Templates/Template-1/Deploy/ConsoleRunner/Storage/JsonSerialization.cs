using System;
using System.Collections.Generic;
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
                (o, t, s) => o.SerializeAndFormat(),
                (t, s) => TypeSerializer.DeserializeFromStream(typeof(object), s));
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
    }
}
