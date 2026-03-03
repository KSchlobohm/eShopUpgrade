# MSMQ Replacement — System.Messaging to Experimental.System.Messaging

## Current State
`System.Messaging` is used in `eShopLegacyMVC/Controllers/CatalogController.cs` (lines 3, 79-91) to send MSMQ messages when a new catalog item is created.

```csharp
using System.Messaging;  // line 3

private void QueueItemCreatedMessage(CatalogItem catalogItem)
{
    using (var queue = new MessageQueue(ConfigurationManager.AppSettings["NewItemQueuePath"]))
    {
        var message = new Message
        {
            Formatter = new XmlMessageFormatter(new[] { typeof(CatalogItem) }),
            Body = catalogItem,
            Label = "New catalog item"
        };
        queue.Send(message);
    }
}
```

The queue path is configured in `web.config`:
```xml
<add key="NewItemQueuePath" value="FormatName:DIRECT=OS:servername\private$\newitems" />
```

The test project (`eShopLegacyMVC.Test.csproj`) also has a `<Reference Include="System.Messaging" />` assembly reference.

## Challenge
- `System.Messaging` is a .NET Framework-only assembly — it does not exist on .NET Core/.NET 5+.
- Direct MSMQ APIs are not available on modern .NET.
- The `MessageQueue`, `Message`, and `XmlMessageFormatter` types must come from an alternative source.

## Migration Plan
1. Install the `Experimental.System.Messaging` NuGet package (latest non-preview version)
2. In `eShopLegacyMVC.csproj` (or `packages.config` if not yet SDK-style): remove `<Reference Include="System.Messaging" />`
3. In `CatalogController.cs`: change `using System.Messaging;` → `using Experimental.System.Messaging;`
4. In `eShopLegacyMVC.Test.csproj`: remove `<Reference Include="System.Messaging" />`
5. Build and verify — the API surface is identical, only the namespace changes

## Actions
- [ ] Find latest non-preview version of Experimental.System.Messaging on NuGet
- [ ] Add PackageReference to eShopLegacyMVC project
- [ ] Remove System.Messaging assembly reference from eShopLegacyMVC
- [ ] Update `using System.Messaging` to `using Experimental.System.Messaging` in CatalogController.cs
- [ ] Remove System.Messaging assembly reference from eShopLegacyMVC.Test
- [ ] Build and run tests

## Verification
- `grep -r "using System.Messaging" --include="*.cs"` returns zero results (should only find `Experimental.System.Messaging`)
- No `<Reference Include="System.Messaging" />` in any csproj
- Solution builds successfully
- CatalogController.Create POST action compiles

## References
- `.squad/skills/msmq-upgrading/SKILL.md`
- [Experimental.System.Messaging on NuGet](https://www.nuget.org/packages/Experimental.System.Messaging)
