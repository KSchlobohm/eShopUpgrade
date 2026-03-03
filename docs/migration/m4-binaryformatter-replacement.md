# BinaryFormatter Replacement

## Current State
`eShopLegacy.Common/Utilities/Serializing.cs` uses `BinaryFormatter` for binary serialization:
```csharp
using System.Runtime.Serialization.Formatters.Binary;

public class Serializing
{
    public Stream SerializeBinary(object input)
    {
        var stream = new MemoryStream();
        var binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(stream, input);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public object DeserializeBinary(Stream stream)
    {
        var binaryFormatter = new BinaryFormatter();
        stream.Seek(0, SeekOrigin.Begin);
        return binaryFormatter.UnsafeDeserialize(stream, null);
    }
}
```

This is consumed by `eShopLegacyMVC/Controllers/WebApi/FilesController.cs`:
```csharp
var serializer = new Serializing();
var response = new HttpResponseMessage(HttpStatusCode.OK)
{
    Content = new StreamContent(serializer.SerializeBinary(brands))
};
```

The `FilesController` serializes a list of `BrandDTO` objects (marked `[Serializable]`) into a binary stream for the HTTP response.

## Challenge
- `BinaryFormatter` is **obsolete** in .NET 5 and **disabled by default** in .NET 7+.
- It is a known security vulnerability (deserialization attacks).
- `UnsafeDeserialize` is even more dangerous and is completely removed.
- Using `<EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>` is a temporary workaround but not recommended.
- The `FilesController` returns binary-serialized data via HTTP — this is an unusual pattern that should be modernized.

## Migration Plan

### Replace with System.Text.Json
1. Update `Serializing.cs` to use `System.Text.Json`:
```csharp
using System.IO;
using System.Text.Json;

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        public Stream SerializeBinary(object input)
        {
            var stream = new MemoryStream();
            JsonSerializer.Serialize(stream, input);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public T DeserializeBinary<T>(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return JsonSerializer.Deserialize<T>(stream);
        }
    }
}
```

2. Update `FilesController` to use JSON content type:
```csharp
var serializer = new Serializing();
var response = new HttpResponseMessage(HttpStatusCode.OK)
{
    Content = new StreamContent(serializer.SerializeBinary(brands))
};
response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
```

3. Remove `[Serializable]` attribute from `BrandDTO` (not needed for JSON serialization).

### Note on Breaking Changes
If any client depends on the binary format of the response, this is a **breaking change**. However, binary serialization over HTTP is a legacy anti-pattern. JSON is the standard replacement.

## Actions
- [ ] Replace BinaryFormatter with System.Text.Json in Serializing.cs
- [ ] Update DeserializeBinary to use generic type parameter
- [ ] Update FilesController to set JSON content type
- [ ] Remove [Serializable] from BrandDTO
- [ ] Remove `using System.Runtime.Serialization.Formatters.Binary`
- [ ] Build and verify

## Verification
- No `BinaryFormatter` references in any source file
- No `System.Runtime.Serialization.Formatters.Binary` using statements
- `Serializing.cs` compiles on net10.0
- `FilesController` returns valid JSON

## References
- [BinaryFormatter security guide](https://learn.microsoft.com/dotnet/standard/serialization/binaryformatter-security-guide)
- [Migrate from BinaryFormatter](https://learn.microsoft.com/dotnet/standard/serialization/binaryformatter-migration-guide/)
- [System.Text.Json overview](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/overview)
