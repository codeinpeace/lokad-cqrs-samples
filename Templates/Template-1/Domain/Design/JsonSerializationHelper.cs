#region (c) 2010-2011 Lokad CQRS - New BSD License
// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Domain.Design
{
    public static class JsonSerializationHelper
    {
        static readonly IsoDateTimeConverter DateTimeConverter = new IsoDateTimeConverter
            {
                Culture = CultureInfo.InvariantCulture,
                DateTimeStyles = DateTimeStyles.RoundtripKind
            };

        public static void SerializeAndFormat(object value, Type type, Stream stream)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.Indented, DateTimeConverter);
            WriteText(stream, json);
        }

        public static void Serialize(object value, Stream stream)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.None, DateTimeConverter);
            WriteText(stream, json);
        }

        public static object Deserialize(Type type, Stream stream)
        {
            var json = ReadText(stream);
            var obj = JsonConvert.DeserializeObject(json, type);
            return obj;
        }

        static void WriteText(Stream stream, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            using (var ms = new MemoryStream(bytes))
            {
                ms.CopyTo(stream);
            }
        }

        static string ReadText(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                var bytes = ms.ToArray();
                var text = Encoding.UTF8.GetString(bytes);
                return text;
            }
        }
    }
}