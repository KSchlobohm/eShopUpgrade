using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        [Obsolete("BinaryFormatter serialization is obsolete and should not be used. See https://aka.ms/binaryformatter for more information.")]
        public Stream SerializeBinary(object input)
        {
            var stream = new MemoryStream();
#pragma warning disable SYSLIB0011
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, input);
#pragma warning restore SYSLIB0011
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        [Obsolete("BinaryFormatter serialization is obsolete and should not be used. See https://aka.ms/binaryformatter for more information.")]
        public object DeserializeBinary(Stream stream)
        {
#pragma warning disable SYSLIB0011
            var binaryFormatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            return binaryFormatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
        }
    }
}