# Configuration Migration — web.config to appsettings.json

## Current State

### web.config — connectionStrings
```xml
<add name="CatalogDBContext" connectionString="Data Source=(localdb)\MSSQLLocalDB; Initial Catalog=Microsoft.eShopOnContainers.Services.CatalogDb; Integrated Security=True; MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
<add name="IdentityDBContext" connectionString="Data Source=(LocalDb)\MSSQLLocalDB; Initial Catalog=Microsoft.eShopOnContainers.Services.IdentityDb; Integrated Security=True" providerName="System.Data.SqlClient" />
```

### web.config — appSettings
```xml
<add key="UseMockData" value="false" />
<add key="UseCustomizationData" value="false" />
<add key="files:BasePath" value="\\bvtsrv2\Team\MikeRou\eShopFiles" />
<add key="files:ServiceAccountId" value="" />
<add key="files:ServiceAccountDomain" value="" />
<add key="files:ServiceAccountPassword" value="" />
<add key="weather:ApiKey" value="" />
<add key="NewItemQueuePath" value="FormatName:DIRECT=OS:servername\private$\newitems" />
```

### ConfigurationManager.AppSettings usage
| File | Keys Used |
|------|-----------|
| `Global.asax.cs` (RegisterContainer, ConfigDataBase) | `UseMockData` |
| `CatalogDBInitializer.cs` | `UseCustomizationData` |
| `CatalogController.cs` (QueueItemCreatedMessage) | `NewItemQueuePath` |
| `FileService.cs` (Create factory) | `Files:BasePath`, `Files:ServiceAccountUsername`, `Files:ServiceAccountDomain`, `Files:ServiceAccountPassword` |

## Challenge
1. **No ConfigurationManager in ASP.NET Core**: The `System.Configuration.ConfigurationManager` class exists as a compatibility shim but is not the recommended pattern.
2. **Hierarchical configuration**: ASP.NET Core uses `IConfiguration` with `builder.Configuration["key"]` or strongly-typed `IOptions<T>`.
3. **Connection strings**: Access via `builder.Configuration.GetConnectionString("name")`.
4. **Environment-specific config**: ASP.NET Core uses `appsettings.{Environment}.json` instead of web.config transforms.

## Migration Plan

### Step 1: Create appsettings.json
```json
{
  "ConnectionStrings": {
    "CatalogDBContext": "Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog=Microsoft.eShopOnContainers.Services.CatalogDb; Integrated Security=True; MultipleActiveResultSets=True;",
    "IdentityDBContext": "Data Source=(LocalDb)\\MSSQLLocalDB; Initial Catalog=Microsoft.eShopOnContainers.Services.IdentityDb; Integrated Security=True"
  },
  "UseMockData": false,
  "UseCustomizationData": false,
  "Files": {
    "BasePath": "\\\\bvtsrv2\\Team\\MikeRou\\eShopFiles",
    "ServiceAccountUsername": "",
    "ServiceAccountDomain": "",
    "ServiceAccountPassword": ""
  },
  "Weather": {
    "ApiKey": ""
  },
  "NewItemQueuePath": "FormatName:DIRECT=OS:servername\\private$\\newitems",
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Step 2: Replace ConfigurationManager calls
```csharp
// Before (Global.asax / Program.cs):
var mockData = bool.Parse(ConfigurationManager.AppSettings["UseMockData"]);

// After (Program.cs):
var useMockData = builder.Configuration.GetValue<bool>("UseMockData");
```

### Step 3: Use IOptions for FileServiceConfiguration
```csharp
builder.Services.Configure<FileServiceConfiguration>(
    builder.Configuration.GetSection("Files"));
```

### Step 4: Inject IConfiguration where needed
```csharp
public class CatalogController : Controller
{
    private readonly string _queuePath;

    public CatalogController(ICatalogService service, IConfiguration configuration)
    {
        this.service = service;
        _queuePath = configuration["NewItemQueuePath"];
    }
}
```

## Actions
- [ ] Create appsettings.json with all migrated settings
- [ ] Create appsettings.Development.json for dev overrides
- [ ] Replace ConfigurationManager.AppSettings in CatalogController
- [ ] Replace ConfigurationManager.AppSettings in CatalogDBInitializer
- [ ] Replace FileService.Create() with DI-based IOptions pattern
- [ ] Verify connection strings load correctly
- [ ] Build and verify

## Verification
- `appsettings.json` exists with all required settings
- No `ConfigurationManager.AppSettings` calls in any source file
- No `System.Configuration` using statements (except in compatibility scenarios)
- Configuration values load correctly at runtime

## References
- [Configuration in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration)
- [Options pattern](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options)
