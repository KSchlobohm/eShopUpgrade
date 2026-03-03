---
name: "systemweb-adapters"
description: "Using System.Web adapters for incremental ASP.NET to ASP.NET Core migration."
domain: "dotnet-migration"
confidence: "high"
source: "https://github.com/dotnet/systemweb-adapters"
---

## System.Web Adapters for ASP.NET Core

The systemweb-adapters project provides a collection of adapters that help migrating from `System.Web.dll` based ASP.NET projects to ASP.NET Core projects.

### Packages

- `Microsoft.AspNetCore.SystemWebAdapters` — Subset of System.Web APIs backed by ASP.NET Core HttpContext
- `Microsoft.AspNetCore.SystemWebAdapters.CoreServices` — Services for ASP.NET Core apps during migration
- `Microsoft.AspNetCore.SystemWebAdapters.FrameworkServices` — Services for ASP.NET Framework apps during migration
- `Microsoft.AspNetCore.SystemWebAdapters.Abstractions` — Shared abstractions (session serialization, etc.)

### Setup Steps

1. Install `Microsoft.AspNetCore.SystemWebAdapters` to supporting libraries
   - Class libraries can target .NET Standard 2.0 for shared surface area
   - Cross-compile with .NET Framework if APIs are missing on Core side

2. Install `Microsoft.AspNetCore.SystemWebAdapters.CoreServices` to ASP.NET Core application

3. Install `Microsoft.AspNetCore.SystemWebAdapters.FrameworkServices` to ASP.NET Framework application

4. Register adapter services in ASP.NET Core app:
   ```csharp
   builder.Services.AddSystemWebAdapters();
   ```

5. Add middleware after routing but before endpoints:
   ```csharp
   app.UseSystemWebAdapters();
   ```

### Supported Targets

- **.NET 6.0+** — Full adapter implementation against ASP.NET Core HttpContext
- **.NET Standard 2.0** — Reference assembly for class libraries (no constructors)
- **.NET Framework 4.7.2** — Type-forwards to System.Web for unified library usage

### Known Limitations

- Some `NameValueCollection`-based APIs (like headers) don't support index-by-position (`Get(int)`, `.Keys`, `.GetEnumerator()`) because ASP.NET Core containers don't have positional indexing.
- If a member is not found, it is not currently supported on ASP.NET Core — no manual workaround.

### Key Docs

- [Incremental ASP.NET to ASP.NET Core Migration](https://learn.microsoft.com/aspnet/core/migration/inc/overview)
- [Get started guide](https://learn.microsoft.com/en-us/aspnet/core/migration/inc/start)
