# MVC Controller Migration — ASP.NET MVC 5 to ASP.NET Core

## Current State
Six MVC controllers in `eShopLegacyMVC/Controllers/`:

| Controller | System.Web Dependencies | Complexity |
|-----------|------------------------|------------|
| `CatalogController.cs` | `System.Web.Mvc`, `System.Messaging`, `System.Configuration` | High — MSMQ, ConfigurationManager |
| `AccountController.cs` | `System.Web.Mvc`, `System.Web` (HttpContext.GetOwinContext), OWIN, ASP.NET Identity v2 | High — full identity stack |
| `PicController.cs` | `System.Web.Mvc`, `Server.MapPath` | Medium — file system access |
| `AspNetSessionController.cs` | `System.Web.Mvc`, `HttpContext.Session` | Medium — session access |
| `DocumentsController.cs` | `System.Web.Mvc`, `System.Web` (MimeMapping, Request.Files) | Medium — file upload/download |
| `UserInfoController.cs` | `System.Web.Mvc` | Low — simple view return |

## Challenge

### Namespace Changes
| ASP.NET MVC 5 | ASP.NET Core |
|---------------|-------------|
| `System.Web.Mvc.Controller` | `Microsoft.AspNetCore.Mvc.Controller` |
| `System.Web.Mvc.ActionResult` | `Microsoft.AspNetCore.Mvc.IActionResult` |
| `System.Web.Mvc.ViewResult` | `Microsoft.AspNetCore.Mvc.ViewResult` |
| `System.Web.Mvc.SelectList` | `Microsoft.AspNetCore.Mvc.Rendering.SelectList` |
| `HttpStatusCodeResult` | `StatusCodeResult` or `StatusCode(int)` |
| `HttpNotFound()` | `NotFound()` |
| `new HttpStatusCodeResult(HttpStatusCode.BadRequest)` | `BadRequest()` |
| `[ValidateAntiForgeryToken]` | `[ValidateAntiForgeryToken]` (same) |
| `[Bind(Include = "...")]` | `[Bind("...")]` |
| `Request.Url.Scheme` | `Request.Scheme` |
| `this.Url.RouteUrl(name, values, scheme)` | `Url.RouteUrl(name, values, scheme)` |
| `Server.MapPath("~/Pics")` | `IWebHostEnvironment.WebRootPath + "/Pics"` |
| `System.Web.MimeMapping.GetMimeMapping()` | `FileExtensionContentTypeProvider` |
| `Request.Files` (HttpFileCollectionBase) | `Request.Form.Files` (IFormFileCollection) |
| `OutputCache` attribute | Response caching middleware |
| `HttpContext.Session["key"]` | `HttpContext.Session.GetString("key")` / `SetString` |

### Key Breaking Changes
1. **PicController**: `Server.MapPath("~/Pics")` → inject `IWebHostEnvironment` and use `WebRootPath`
2. **DocumentsController**: `MimeMapping.GetMimeMapping()` → `FileExtensionContentTypeProvider`; `Request.Files` → `IFormFileCollection`
3. **AspNetSessionController**: `HttpContext.Session["DemoItem"]` → ASP.NET Core session uses string-based `Get`/`Set` methods with serialization
4. **AccountController**: Entire OWIN/Identity stack replacement (handled in M8)
5. **CatalogController**: `ConfigurationManager.AppSettings` → `IConfiguration` (handled in M9)

## Migration Plan

### Port Order (least to most complex)
1. `UserInfoController` — trivial, just namespace change
2. `PicController` — inject IWebHostEnvironment, replace Server.MapPath
3. `AspNetSessionController` — update session API
4. `DocumentsController` — replace MimeMapping, Request.Files
5. `CatalogController` — update after MSMQ (M2) and config (M9) migrations
6. `AccountController` — defer heavy changes to M8 (Identity migration)

### Per-Controller Steps
1. Replace `using System.Web.Mvc` with `using Microsoft.AspNetCore.Mvc`
2. Replace return types and helper methods
3. Inject required services via constructor (IWebHostEnvironment, IConfiguration, etc.)
4. Replace `[Bind(Include = "...")]` with `[Bind("...")]`
5. Build and fix compilation errors

## Actions
- [ ] Port UserInfoController (namespace change only)
- [ ] Port PicController (inject IWebHostEnvironment, replace MapPath)
- [ ] Port AspNetSessionController (update session API)
- [ ] Port DocumentsController (replace MimeMapping, file upload API)
- [ ] Port CatalogController (update after MSMQ/config changes)
- [ ] Stub AccountController (full migration in M8)
- [ ] Build after each controller port

## Verification
- All controllers compile against `Microsoft.AspNetCore.Mvc`
- No `using System.Web.Mvc` statements remain
- No `System.Web` references in controllers (except AccountController if deferred)

## References
- [Migrate controllers from ASP.NET MVC to ASP.NET Core](https://learn.microsoft.com/aspnet/core/migration/proper-to-2x/)
- `.squad/skills/systemweb-adapters/SKILL.md`
