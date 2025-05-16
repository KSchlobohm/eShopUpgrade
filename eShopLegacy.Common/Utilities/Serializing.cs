using System.IO;
#pragma warning disable SYSLIB0011 // Type or member is obsolete
using System.Runtime.Serialization.Formatters.Binary;
#pragma warning restore SYSLIB0011 // Type or member is obsolete

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        public Stream SerializeBinary(object input)
        {
            var stream = new MemoryStream();
            #pragma warning disable SYSLIB0011 // Type or member is obsolete
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, input);
            #pragma warning restore SYSLIB0011 // Type or member is obsolete
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public object DeserializeBinary(Stream stream)
        {
            #pragma warning disable SYSLIB0011 // Type or member is obsolete
            var binaryFormatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            return binaryFormatter.Deserialize(stream);
            #pragma warning restore SYSLIB0011 // Type or member is obsolete
        }
    }
}