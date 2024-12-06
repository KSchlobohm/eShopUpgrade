using System.IO;
using System.Text.Json;

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        public string SerializeBinary(object input)
        {
            return JsonSerializer.Serialize(input);
        }

        public object DeserializeBinary(string json)
        {
            return JsonSerializer.Deserialize<object>(json);
        }
    }
}