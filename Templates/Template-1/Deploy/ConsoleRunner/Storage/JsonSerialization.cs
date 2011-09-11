using System;
using System.Collections.Generic;
using Domain.Design;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Feature.AtomicStorage;

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
                JsonSerializationHelper.SerializeAndFormat,
                JsonSerializationHelper.Deserialize);
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
                    stream => JsonSerializationHelper.Deserialize(type, stream),
                    JsonSerializationHelper.Serialize);
            }
        }
    }
}
