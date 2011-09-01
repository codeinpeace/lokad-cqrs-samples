using System;
using System.Collections.Generic;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Serialization;
using ServiceStack.Text;

namespace ConsoleRunner.Storage
{
    internal static class JsonSerialization
    {
        public static void Set(CqrsEngineBuilder builder)
        {
            builder.Advanced.CustomDataSerializer(t => new JsonDataSerializer(t));
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
