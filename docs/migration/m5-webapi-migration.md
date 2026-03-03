# Web API Controller Migration — Web API 2 to ASP.NET Core

## Current State
Three Web API-style controllers:

### `Controllers/WebApi/BrandsController.cs`
- Extends `System.Web.Http.ApiController`
- Uses `IHttpActionResult`, `HttpResponseMessage`
- Has `using System.Runtime.Remoting.Messaging` (unused — leftover import)
- Uses `using eShopLegacy.Utilities` (WebHelper reference)
- CRUD operations for CatalogBrand

### `Controllers/WebApi/FilesController.cs`
- Extends `System.Web.Http.ApiController`
- Returns `HttpResponseMessage` with `StreamContent` (binary-serialized data via `Serializing` class)
- Uses `eShopLegacy.Utilities.Serializing` (BinaryFormatter — addressed in M4)

### `Controllers/Api/CatalogController.cs` (CatalogController2)
- Extends `System.Web.Mvc.Controller` (not ApiController)
- Has `[Route("api")]` attribute
- Returns `Json(new { Message = "Hello World!" })` — simple endpoint

## Challenge

### Namespace Changes
| Web API 2 | ASP.NET Core |
|-----------|-------------|
| `System.Web.Http.ApiController` | `Microsoft.AspNetCore.Mvc.ControllerBase` with `[ApiController]` |
| `IHttpActionResult` | `IActionResult` |
| `Ok(value)` | `Ok(value)` (same) |
| `NotFound()` | `NotFound()` (same) |
| `ResponseMessage(new HttpResponseMessage(...))` | `StatusCode(int)` or specific result |
| `System.Net.Http.HttpResponseMessage` | Return `IActionResult` instead |

### Key Issues
1. **BrandsController**: `System.Runtime.Remoting.Messaging` is not available on .NET Core — but it's an unused import. Remove it.
2. **FilesController**: Returns `HttpResponseMessage` with binary content — needs to return `FileStreamResult` or `ContentResult` in ASP.NET Core.
3. **CatalogController2**: Actually uses MVC `Controller`, not `ApiController`. The `[Route("api")]` attribute will work with ASP.NET Core routing.

## Migration Plan

### BrandsController
```csharp
using Microsoft.AspNetCore.Mvc;

namespace eShopLegacyMVC.Controllers.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandsController : ControllerBase
    {
        // IHttpActionResult → IActionResult
        // ResponseMessage(new HttpResponseMessage(HttpStatusCode.NotFound)) → NotFound()
        // ResponseMessage(new HttpResponseMessage(HttpStatusCode.OK)) → Ok()
    }
}
```

### FilesController
```csharp
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    public IActionResult Get()
    {
        var brands = _service.GetCatalogBrands()...;
        var serializer = new Serializing();
        var stream = serializer.SerializeBinary(brands);
        return File(stream, "application/json"); // or return Ok(brands) for JSON
    }
}
```

### CatalogController2
```csharp
using Microsoft.AspNetCore.Mvc;

[Route("api")]
public class CatalogController2 : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return Json(new { Message = "Hello World!" }); // Works the same in Core
    }
}
```

## Actions
- [ ] Port BrandsController: ApiController → ControllerBase, remove Remoting import
- [ ] Port FilesController: ApiController → ControllerBase, replace HttpResponseMessage
- [ ] Port CatalogController2: Replace System.Web.Mvc with Microsoft.AspNetCore.Mvc
- [ ] Update routing to work with ASP.NET Core endpoint routing
- [ ] Build and verify

## Verification
- No `System.Web.Http` references remain
- No `System.Runtime.Remoting` references remain
- All API endpoints compile and return correct types
- `dotnet build` succeeds

## References
- [Migrate from Web API to ASP.NET Core](https://learn.microsoft.com/aspnet/core/migration/webapi)
