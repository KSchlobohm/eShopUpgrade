using System.IO;
#if NETFRAMEWORK
using System.Runtime.Serialization.Formatters.Binary;
#else
using System.Text.Json;
#endif

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        public Stream SerializeBinary(object input)
        {
            var stream = new MemoryStream();
#if NETFRAMEWORK
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, input);
#else
            JsonSerializer.Serialize(stream, input, input.GetType());
#endif
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public object DeserializeBinary(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
#if NETFRAMEWORK
            var binaryFormatter = new BinaryFormatter();
            return binaryFormatter.UnsafeDeserialize(stream, null);
#else
            using var reader = new StreamReader(stream, leaveOpen: true);
            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<JsonElement>(json);
#endif
        }
    }
}
