# Utilities Project — System.Web Removal

## Current State
`eShopLegacy.Utilities/WebHelper.cs` contains:
```csharp
using System.Web;

namespace eShopLegacy.Utilities
{
    public class WebHelper
    {
        public static string UserAgent => HttpContext.Current.Request.UserAgent;
    }
}
```

This class is a static helper that reads the current HTTP request's User-Agent header via `System.Web.HttpContext.Current`. It is referenced by `eShopLegacyMVC/Controllers/WebApi/BrandsController.cs` (imported but usage is minimal — the `using eShopLegacy.Utilities;` is present but `WebHelper` isn't directly called in the visible code).

The project also has a direct `<Reference Include="System.Web" />` in the csproj.

## Challenge
- `HttpContext.Current` is a static accessor that **does not exist** in ASP.NET Core.
- ASP.NET Core uses dependency injection to provide `HttpContext` — there is no global static accessor.
- The `System.Web` assembly is not available on .NET 10.
- Options for migration:
  1. **Remove entirely** if WebHelper is not actively used
  2. **Redesign with DI** — accept `IHttpContextAccessor` as a dependency
  3. **Use systemweb-adapters** — the `Microsoft.AspNetCore.SystemWebAdapters` package provides a compatibility shim for `HttpContext.Current`, but this is intended as a transitional tool, not a permanent solution

## Migration Plan

### Recommended: Redesign with DI
1. Remove the static `UserAgent` property
2. Make `WebHelper` a service registered in DI:
```csharp
using Microsoft.AspNetCore.Http;

namespace eShopLegacy.Utilities
{
    public class WebHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserAgent =>
            _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? string.Empty;
    }
}
```
3. Register in DI: `builder.Services.AddHttpContextAccessor(); builder.Services.AddScoped<WebHelper>();`
4. Update callers to inject `WebHelper` instead of using static access

### Alternative: Remove if unused
If `WebHelper` is not actively called by any code path, remove it entirely and clean up the `using` statements in `BrandsController.cs`.

## Actions
- [ ] Determine if WebHelper.UserAgent is actively used in any code path
- [ ] If used: redesign as DI-based service with IHttpContextAccessor
- [ ] If unused: remove WebHelper.cs and clean up references
- [ ] Remove `<Reference Include="System.Web" />` from Utilities csproj
- [ ] Add `Microsoft.AspNetCore.Http.Abstractions` PackageReference if using IHttpContextAccessor
- [ ] Build and verify

## Verification
- No `System.Web` reference in eShopLegacy.Utilities.csproj
- No `HttpContext.Current` usage anywhere in the project
- `dotnet build eShopLegacy.Utilities` succeeds on net10.0

## References
- `.squad/skills/systemweb-adapters/SKILL.md`
- [Access HttpContext in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/http-context)
