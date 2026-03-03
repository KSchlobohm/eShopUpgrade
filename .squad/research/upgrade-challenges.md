# eShopLegacyMVC Upgrade Challenges — .NET Framework 4.7.2 → .NET 10

> Researched by Fenster (Research Analyst) — June 2025
> Sources: Codebase analysis, Microsoft docs, NuGet package registry, skill files

---

## Solution Structure & Dependency Graph

### Projects

| Project | Type | Target Framework | Description |
|---------|------|-----------------|-------------|
| `eShopLegacy.Common` | Class Library | .NET Framework 4.6.1 | Shared models, view models, serialization utilities |
| `eShopLegacy.Utilities` | Class Library | .NET Framework 4.6.1 | WebHelper using System.Web.HttpContext |
| `eShopLegacyMVC` | ASP.NET MVC 5 Web App | .NET Framework 4.7.2 | Main web application |
| `eShopLegacyMVC.Test` | MSTest Unit Test Project | .NET Framework 4.7.2 | Unit tests for controllers, models, services |

### Dependency Graph

```
eShopLegacy.Common (no project deps)
  ↑
eShopLegacy.Utilities (refs → Common)
  
eShopLegacyMVC (refs → Common, Utilities)
  ↑
eShopLegacyMVC.Test (refs → Common, eShopLegacyMVC)
```

### Build Order (topological)
1. `eShopLegacy.Common`
2. `eShopLegacy.Utilities`
3. `eShopLegacyMVC`
4. `eShopLegacyMVC.Test`

---

## Challenge Summary Table

| # | Challenge | Risk | Complexity | Files Affected | Migration Path |
|---|-----------|------|-----------|----------------|----------------|
| 1 | ASP.NET MVC 5 → ASP.NET Core | 🔴 Blocker | Complex | 15+ files | Side-by-side rewrite to ASP.NET Core MVC |
| 2 | ASP.NET Web API 2 → ASP.NET Core | 🔴 Blocker | Complex | 4 files | Merge into ASP.NET Core unified controller model |
| 3 | OWIN/Katana Middleware | 🔴 Blocker | Complex | 3 files | Replace with ASP.NET Core middleware pipeline |
| 4 | ASP.NET Identity v2 (OWIN-based) | 🔴 Blocker | Complex | 5 files | Migrate to ASP.NET Core Identity |
| 5 | System.Web.* Dependencies (pervasive) | 🔴 Blocker | Complex | 20+ files | systemweb-adapters + progressive rewrite |
| 6 | Global.asax / HttpApplication Lifecycle | 🔴 Blocker | Complex | 1 file | Rewrite as Program.cs / Startup pipeline |
| 7 | Autofac DI (MVC5 + WebApi2 integration) | 🟡 Needs Work | Moderate | 3 files | Autofac.Extensions.DependencyInjection for ASP.NET Core |
| 8 | Entity Framework 6 → EF Core | 🟡 Needs Work | Moderate | 5 files | EF Core with Fluent API rewrite |
| 9 | System.Messaging (MSMQ) | 🟡 Needs Work | Moderate | 1 file | Experimental.System.Messaging NuGet package |
| 10 | BinaryFormatter Serialization | 🔴 Blocker | Moderate | 1 file | Replace with System.Text.Json or other safe serializer |
| 11 | System.Web.Optimization (Bundling) | 🟡 Needs Work | Moderate | 2 files | WebOptimizer or static file middleware |
| 12 | Razor Views (MVC5 → ASP.NET Core) | 🟡 Needs Work | Moderate | 17 cshtml files | Update tag helpers, remove @Scripts/@Styles helpers |
| 13 | Web.config → appsettings.json | 🟡 Needs Work | Moderate | 4 config files | IConfiguration / appsettings.json |
| 14 | ConfigurationManager Usage | 🟡 Needs Work | Moderate | 5 files | IConfiguration DI injection |
| 15 | HttpContext.Current Static Access | 🟡 Needs Work | Moderate | 5 files | IHttpContextAccessor DI |
| 16 | Server.MapPath / HostingEnvironment | 🟡 Needs Work | Simple | 3 files | IWebHostEnvironment.ContentRootPath |
| 17 | System.Web.MimeMapping | 🟡 Needs Work | Simple | 1 file | FileExtensionContentTypeProvider |
| 18 | HttpFileCollectionBase (File Uploads) | 🟡 Needs Work | Simple | 1 file | IFormFile / IFormFileCollection |
| 19 | WindowsIdentity.Impersonate + P/Invoke | 🟡 Needs Work | Moderate | 1 file | Windows Compatibility Pack or redesign |
| 20 | Application Insights (Framework SDK) | 🟡 Needs Work | Simple | 2 files | Microsoft.ApplicationInsights.AspNetCore |
| 21 | log4net | 🟢 Straightforward | Simple | 3 files | log4net works on .NET Core, or switch to ILogger |
| 22 | Newtonsoft.Json | 🟢 Straightforward | Simple | 1 file | System.Text.Json or keep Newtonsoft (cross-platform) |
| 23 | WebClient (obsolete) | 🟢 Straightforward | Simple | 1 file | HttpClient |
| 24 | HttpWebRequest (obsolete) | 🟢 Straightforward | Simple | 1 file | HttpClient |
| 25 | System.Runtime.Remoting.Messaging | 🟡 Needs Work | Simple | 1 file | Remove or replace (CallContext not in .NET Core) |
| 26 | Non-SDK-Style Project Files | 🟡 Needs Work | Moderate | 4 csproj files | Convert to SDK-style csproj |
| 27 | Test Project Migration (MSTest) | 🟡 Needs Work | Moderate | 7 files | Update MSTest packages, remove System.Web test mocks |
| 28 | Database Initializer Pattern | 🟡 Needs Work | Moderate | 1 file | EF Core Migrations or custom seeding |
| 29 | Raw SQL Queries (Database.SqlQuery) | 🟡 Needs Work | Simple | 2 files | EF Core FromSqlRaw / SqlQueryRaw |
| 30 | OutputCache Attribute | 🟡 Needs Work | Simple | 1 file | Response caching middleware |
| 31 | Session State (InProc) | 🟡 Needs Work | Simple | 3 files | ASP.NET Core distributed session |
| 32 | ViewBag/Dynamic for SelectList | 🟢 Straightforward | Simple | 3 files | Same pattern works in ASP.NET Core |

---

## Detailed Challenges

### Challenge 1: ASP.NET MVC 5 → ASP.NET Core MVC
**Risk:** 🔴 Blocker
**Files affected:**
- `Controllers/CatalogController.cs` — inherits `System.Web.Mvc.Controller`
- `Controllers/AccountController.cs` — inherits `System.Web.Mvc.Controller`
- `Controllers/AspNetSessionController.cs`
- `Controllers/DocumentsController.cs`
- `Controllers/PicController.cs`
- `Controllers/UserInfoController.cs`
- `Controllers/Api/CatalogController.cs` — MVC `Controller` with `[Route("api")]`
- `App_Start/RouteConfig.cs` — `RouteCollection`, `MapMvcAttributeRoutes`
- `App_Start/FilterConfig.cs` — `GlobalFilterCollection`, `HandleErrorAttribute`

**Current usage:** All MVC controllers inherit from `System.Web.Mvc.Controller` and use MVC5 action results (`HttpStatusCodeResult`, `HttpNotFoundResult`, `ActionResult`, `ViewResult`, `RedirectToRouteResult`). Routes configured via `RouteConfig` with `RouteTable.Routes`.

**Migration path:**
1. Controllers inherit `Microsoft.AspNetCore.Mvc.Controller` instead
2. Replace `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`
3. Replace `HttpNotFound()` → `NotFound()`
4. Replace `new SelectList(...)` with `ViewBag` — works the same in Core
5. Replace `[Bind(Include=...)]` / `[Bind(Exclude=...)]` — `[Bind]` still exists but `Exclude` is removed; use view models instead
6. Route config moves to `app.MapControllerRoute()` in Program.cs
7. `AreaRegistration.RegisterAllAreas()` → `[Area]` attribute on controllers
8. `[ValidateAntiForgeryToken]` still works in ASP.NET Core
9. `Request.Url.Scheme` → `Request.Scheme`
10. `Server.MapPath("~/Pics")` → `IWebHostEnvironment.WebRootPath`

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/migration/proper-to-2x/
- https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions

**Complexity:** Complex — touches every controller and the routing infrastructure

---

### Challenge 2: ASP.NET Web API 2 → ASP.NET Core
**Risk:** 🔴 Blocker
**Files affected:**
- `Controllers/WebApi/BrandsController.cs` — inherits `System.Web.Http.ApiController`
- `Controllers/WebApi/FilesController.cs` — inherits `System.Web.Http.ApiController`
- `App_Start/WebApiConfig.cs` — `HttpConfiguration`, `MapHttpAttributeRoutes`
- `Global.asax.cs` — `GlobalConfiguration.Configure(WebApiConfig.Register)`

**Current usage:** Two Web API controllers using `ApiController` base class with `IHttpActionResult`, `ResponseMessage()`, `HttpResponseMessage`, and `System.Web.Http` attributes. Uses `System.Runtime.Remoting.Messaging.CallContext` in BrandsController (via `using System.Runtime.Remoting.Messaging`).

**Migration path:**
1. Remove `ApiController` base class — use `ControllerBase` with `[ApiController]` attribute
2. Replace `IHttpActionResult` → `IActionResult` or `ActionResult<T>`
3. Replace `ResponseMessage(new HttpResponseMessage(...))` → `StatusCode(...)` or typed results
4. Remove `WebApiConfig.cs` — routes defined via attribute routing or `app.MapControllers()`
5. Remove `System.Runtime.Remoting.Messaging` usage (not available in .NET Core)

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/migration/webapi

**Complexity:** Complex — different base class, return types, and configuration model

---

### Challenge 3: OWIN/Katana Middleware Pipeline
**Risk:** 🔴 Blocker
**Files affected:**
- `Startup.cs` — `[assembly: OwinStartupAttribute]`, `IAppBuilder`
- `App_Start/Startup.Auth.cs` — `IAppBuilder.UseCookieAuthentication`, `CreatePerOwinContext`
- `Controllers/AccountController.cs` — `HttpContext.GetOwinContext()`

**Current usage:** OWIN used as middleware host for authentication. `Startup.cs` is the OWIN entry point. `Startup.Auth.cs` configures cookie auth via OWIN. `AccountController` uses `GetOwinContext()` to resolve managers.

**Migration path:**
1. Delete `Startup.cs` (OWIN startup) — replaced by `Program.cs` with ASP.NET Core middleware
2. Replace `IAppBuilder` middleware with ASP.NET Core `IApplicationBuilder` pipeline
3. Replace `CreatePerOwinContext` → ASP.NET Core DI (register services in `builder.Services`)
4. Replace `HttpContext.GetOwinContext()` → direct DI injection of services
5. Remove all `Microsoft.Owin.*` packages

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/migration/proper-to-2x/
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/

**Complexity:** Complex — complete pipeline redesign

---

### Challenge 4: ASP.NET Identity v2 (OWIN-based) → ASP.NET Core Identity
**Risk:** 🔴 Blocker
**Files affected:**
- `App_Start/IdentityConfig.cs` — `ApplicationUserManager`, `ApplicationSignInManager`, `IdentityFactoryOptions`, `IOwinContext`
- `App_Start/Startup.Auth.cs` — OWIN-based identity registration
- `Models/IdentityModels.cs` — `ApplicationUser : IdentityUser`, `ApplicationDbContext : IdentityDbContext`
- `Models/AccountViewModels.cs` — Login/Register view models
- `Controllers/AccountController.cs` — full authentication flow

**Current usage:** ASP.NET Identity 2.2.3 with OWIN integration. `ApplicationUserManager` uses `IdentityFactoryOptions` + `IOwinContext` factory pattern. `SignInManager` configured via OWIN context. `ApplicationUser` has custom `ZipCode` property with synchronous HTTP call in getter. `ApplicationDbContext` uses `IdentityDbContext<ApplicationUser>` with EF6.

**Migration path:**
1. Replace `Microsoft.AspNet.Identity.*` → `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
2. `ApplicationDbContext` → inherit from `IdentityDbContext<ApplicationUser>` (using EF Core)
3. Remove factory pattern (`IdentityFactoryOptions`, `IOwinContext`) — use DI instead
4. `UserManager<T>` and `SignInManager<T, string>` → ASP.NET Core versions (no second type param)
5. `SignInManager.PasswordSignInAsync` returns `SignInResult` instead of `SignInStatus` enum
6. `AuthenticationManager.SignOut` → `HttpContext.SignOutAsync()`
7. Cookie auth configured via `builder.Services.ConfigureApplicationCookie()`
8. ⚠️ The `ApplicationUser.ZipCode` property makes a synchronous `HttpWebRequest` in the getter — this is a design smell that should be refactored regardless

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/migration/identity
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity

**Complexity:** Complex — identity is deeply woven through auth flow, models, and controllers

---

### Challenge 5: System.Web.* Dependencies (Pervasive)
**Risk:** 🔴 Blocker
**Files affected:**
- `Global.asax.cs` — `HttpApplication`, `HttpContext.Current`
- `Controllers/*.cs` — `System.Web.Mvc.*` throughout
- `Services/FileService.cs` — `HttpFileCollectionBase`
- `Models/Infrastructure/CatalogDBInitializer.cs` — `HostingEnvironment.ApplicationPhysicalPath`
- `Models/Infrastructure/PreconfiguredData.cs` — `using System.Web`
- `Models/CatalogBrand.cs`, `CatalogType.cs` — `using System.Web`
- `eShopLegacy.Utilities/WebHelper.cs` — `HttpContext.Current.Request.UserAgent`
- `Views/Shared/_Layout.cshtml` — `HttpContext.Current.Session[...]`
- `Views/Shared/_LoginPartial.cshtml` — `Request.IsAuthenticated`
- `App_Start/FilterConfig.cs` — `System.Web.Mvc.HandleErrorAttribute`
- `Controllers/DocumentsController.cs` — `MimeMapping.GetMimeMapping()`, `OutputCache`
- `App_Start/BundleConfig.cs` — `System.Web.Optimization.*`

**Current usage:** `System.Web` is used everywhere — controllers, views, services, helpers, and startup. The `HttpContext.Current` static accessor is used in `Global.asax.cs`, `WebHelper.cs`, and `_Layout.cshtml`.

**Migration path:**
1. Install `Microsoft.AspNetCore.SystemWebAdapters` for transitional compatibility (per skill file)
2. Replace `HttpContext.Current` → inject `IHttpContextAccessor`
3. Replace `Request.IsAuthenticated` → `User.Identity?.IsAuthenticated`
4. Replace `HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath`
5. Replace `MimeMapping.GetMimeMapping()` → `FileExtensionContentTypeProvider`
6. Replace `HttpFileCollectionBase` / `Request.Files` → `IFormFileCollection`
7. Remove unnecessary `using System.Web` where only used for model classes

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/migration/inc/overview
- https://github.com/dotnet/systemweb-adapters

**Complexity:** Complex — most pervasive dependency in the solution

---

### Challenge 6: Global.asax / HttpApplication Lifecycle
**Risk:** 🔴 Blocker
**Files affected:**
- `Global.asax` — the physical file
- `Global.asax.cs` — `MvcApplication : HttpApplication` with `Application_Start`, `Session_Start`, `Application_BeginRequest`

**Current usage:**
- `Application_Start()` — registers Autofac container, configures WebAPI, areas, filters, routes, bundles, and database
- `Session_Start()` — stores machine name and session start time in `HttpContext.Current.Session`
- `Application_BeginRequest()` — sets log4net properties for activity ID and request info
- `ActivityIdHelper` class — uses `Trace.CorrelationManager.ActivityId`
- `WebRequestInfo` class — uses `HttpContext.Current.Request.RawUrl` and `UserAgent`

**Migration path:**
1. Delete `Global.asax` and `Global.asax.cs`
2. Create `Program.cs` with ASP.NET Core hosting model
3. Move DI registration to `builder.Services.*`
4. Move middleware to `app.Use*()` pipeline
5. Replace `Session_Start` → session middleware with custom initialization
6. Replace `Application_BeginRequest` → custom middleware for logging correlation
7. `Trace.CorrelationManager.ActivityId` → `Activity.Current` or `ILogger` scopes
8. `WebRequestInfo` → middleware accessing `HttpContext.Request`

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/migration/proper-to-2x/#globalasax-file-replacement
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/startup

**Complexity:** Complex — startup is the backbone of the application

---

### Challenge 7: Autofac DI (MVC5 + WebApi2 Integration)
**Risk:** 🟡 Needs Work
**Files affected:**
- `Global.asax.cs` — `ContainerBuilder`, `RegisterControllers`, `RegisterApiControllers`, `AutofacDependencyResolver`, `AutofacWebApiDependencyResolver`
- `Modules/ApplicationModule.cs` — `Autofac.Module` with service registrations
- `packages.config` — `Autofac 4.9.1`, `Autofac.Mvc5 4.0.2`, `Autofac.WebApi2 4.3.1`

**Current usage:** Autofac registers all MVC controllers and Web API controllers. Uses `DependencyResolver.SetResolver()` for MVC and `GlobalConfiguration.Configuration.DependencyResolver` for Web API. `ApplicationModule` registers `CatalogService`, `CatalogDBContext`, `CatalogDBInitializer`, `CatalogItemHiLoGenerator`.

**Migration path:**
1. Replace `Autofac.Mvc5` + `Autofac.WebApi2` → `Autofac.Extensions.DependencyInjection` (supports ASP.NET Core)
2. In `Program.cs`: `builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())`
3. `ApplicationModule` can remain mostly unchanged (Autofac modules work with Core)
4. Remove `AutofacDependencyResolver` and `AutofacWebApiDependencyResolver`
5. Remove `RegisterControllers` / `RegisterApiControllers` — ASP.NET Core discovers controllers automatically
6. **Alternative:** Migrate to built-in `IServiceCollection` DI and remove Autofac entirely (simpler long-term)

**Documentation:**
- https://autofac.readthedocs.io/en/latest/integration/aspnetcore.html
- NuGet: `Autofac.Extensions.DependencyInjection` — latest supports .NET 8+ ✅

**Complexity:** Moderate — Autofac has good ASP.NET Core support

---

### Challenge 8: Entity Framework 6 → EF Core
**Risk:** 🟡 Needs Work
**Files affected:**
- `Models/CatalogDBContext.cs` — `DbContext` with `DbModelBuilder`, `EntityTypeConfiguration<T>`, `HasRequired`, `WithMany`, `HasForeignKey`
- `Models/CatalogItemHiLoGenerator.cs` — `db.Database.SqlQuery<Int64>("SELECT NEXT VALUE FOR...")`
- `Models/Infrastructure/CatalogDBInitializer.cs` — `CreateDatabaseIfNotExists<T>`, `Database.ExecuteSqlCommand`, `Database.SqlQuery`
- `Services/CatalogService.cs` — `db.Entry(catalogItem).State = EntityState.Modified`, `Include()`, LINQ
- `Models/IdentityModels.cs` — `IdentityDbContext<ApplicationUser>` (EF6 version)
- `eShopLegacy.Common/packages.config` — `EntityFramework 6.0.0`
- `eShopLegacyMVC/packages.config` — `EntityFramework 6.2.0`

**Current usage:** EF6 with Fluent API configuration using `EntityTypeConfiguration<T>`, `DbModelBuilder`, database initializers (`CreateDatabaseIfNotExists`), raw SQL queries via `Database.SqlQuery<T>`, and `Database.ExecuteSqlCommand`.

**Migration path:**
1. Replace `EntityFramework` 6.x → `Microsoft.EntityFrameworkCore.SqlServer`
2. `DbModelBuilder` → `ModelBuilder` (similar but different API)
3. `EntityTypeConfiguration<T>` → `IEntityTypeConfiguration<T>` or inline in `OnModelCreating`
4. `HasRequired<T>()` → `HasOne<T>().WithMany().HasForeignKey().IsRequired()`
5. `builder.Ignore(ci => ci.PictureUri)` → same API exists
6. `HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)` → `.ValueGeneratedNever()`
7. `Database.SqlQuery<T>()` → `FromSqlRaw()` or `SqlQueryRaw<T>()` (EF Core 8+)
8. `Database.ExecuteSqlCommand()` → `Database.ExecuteSqlRaw()`
9. `CreateDatabaseIfNotExists` → `Database.EnsureCreated()` or EF Core Migrations
10. `db.Entry(x).State = EntityState.Modified` → same in EF Core
11. Connection string format stays the same but provider changes to `Microsoft.Data.SqlClient`

**Documentation:**
- https://learn.microsoft.com/en-us/ef/core/
- https://learn.microsoft.com/en-us/ef/efcore-and-ef6/porting/
- https://learn.microsoft.com/en-us/ef/core/modeling/

**Complexity:** Moderate — EF Core has parity for most patterns used here

---

### Challenge 9: System.Messaging (MSMQ)
**Risk:** 🟡 Needs Work
**Files affected:**
- `Controllers/CatalogController.cs` — `using System.Messaging`, `MessageQueue`, `Message`, `XmlMessageFormatter`
- `Web.config` — `NewItemQueuePath` app setting

**Current usage:** `QueueItemCreatedMessage()` method creates an MSMQ message with XML serialization and sends it when a new catalog item is created.

**Migration path:** (per MSMQ skill file)
1. Remove `System.Messaging` assembly reference
2. Add NuGet package `Experimental.System.Messaging`
3. Change `using System.Messaging` → `using Experimental.System.Messaging`
4. API is otherwise identical (drop-in replacement)

**Documentation:**
- https://www.nuget.org/packages/Experimental.System.Messaging
- Skill file: `.squad/skills/msmq-upgrading/SKILL.md`

**Complexity:** Moderate — namespace change is mechanical but needs testing with actual MSMQ infrastructure

---

### Challenge 10: BinaryFormatter Serialization
**Risk:** 🔴 Blocker
**Files affected:**
- `eShopLegacy.Common/Utilities/Serializing.cs` — `BinaryFormatter.Serialize()`, `BinaryFormatter.UnsafeDeserialize()`
- `Controllers/WebApi/FilesController.cs` — calls `serializer.SerializeBinary(brands)`

**Current usage:** `Serializing` class uses `BinaryFormatter` for binary serialization/deserialization. Called from `FilesController` to serialize brand DTOs into HTTP response streams.

**Migration path:**
`BinaryFormatter` is **completely removed** in .NET 9+ (marked obsolete as error in .NET 8, removed in .NET 9). This is a hard blocker.
1. Replace `BinaryFormatter` with `System.Text.Json.JsonSerializer` or `MessagePack` or `protobuf-net`
2. For the `FilesController`, return JSON instead of binary (standard REST practice)
3. If binary format is truly needed, use `MessagePack-CSharp` or `protobuf-net`
4. Remove `[Serializable]` attribute from `BrandDTO` (not needed with modern serializers)

**Documentation:**
- https://learn.microsoft.com/en-us/dotnet/core/compatibility/serialization/9.0/binaryformatter-removal
- https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide

**Complexity:** Moderate — need to choose replacement serializer and update callers

---

### Challenge 11: System.Web.Optimization (Bundling & Minification)
**Risk:** 🟡 Needs Work
**Files affected:**
- `App_Start/BundleConfig.cs` — `ScriptBundle`, `StyleBundle`, `BundleCollection`, `BundleTable`
- `Views/Shared/_Layout.cshtml` — `@Styles.Render("~/Content/css")`, `@Scripts.Render("~/bundles/...")`
- `Views/AspNetSession/Index.cshtml` — `@Scripts.Render("~/bundles/jqueryval")`
- `packages.config` — `Microsoft.AspNet.Web.Optimization 1.1.3`, `WebGrease 1.6.0`, `Antlr 3.5.0.2`

**Current usage:** MVC5 bundling system registers jQuery, Bootstrap, Modernizr, and validation script bundles plus CSS bundle.

**Migration path:**
1. Remove `BundleConfig.cs`, `Microsoft.AspNet.Web.Optimization`, `WebGrease`, `Antlr`
2. Option A: Use `WebOptimizer` (`LigerShark.WebOptimizer.Core` NuGet) — closest equivalent
3. Option B: Use static files directly with `<link>` and `<script>` tags (simplest)
4. Option C: Use a build-time bundler (webpack, Vite) for more control
5. Replace `@Styles.Render()` and `@Scripts.Render()` → standard HTML tags or tag helpers
6. Static files served via `app.UseStaticFiles()`

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/client-side/bundling-and-minification
- https://github.com/nickvdyck/WebOptimizer

**Complexity:** Moderate — need to restructure static asset references across all views

---

### Challenge 12: Razor Views (MVC5 → ASP.NET Core Razor)
**Risk:** 🟡 Needs Work
**Files affected:** All 17 `.cshtml` files in `Views/`

**Key changes needed:**
1. `@Html.Partial("_LoginPartial")` → `<partial name="_LoginPartial" />` tag helper
2. `@Html.BeginForm(...)` → `<form asp-action="..." asp-controller="...">` tag helper (or keep helper)
3. `@Html.AntiForgeryToken()` → `@Html.AntiForgeryToken()` (same) or auto via `[AutoValidateAntiforgeryToken]`
4. `@Html.ActionLink(...)` → `<a asp-action="..." asp-controller="...">` tag helper
5. `@Styles.Render()` / `@Scripts.Render()` → raw `<link>` / `<script>` tags
6. `@RenderBody()` / `@RenderSection()` → same in ASP.NET Core Razor
7. `HttpContext.Current.Session[...]` in `_Layout.cshtml` → inject `IHttpContextAccessor` or use `Context.Session`
8. `Request.IsAuthenticated` in `_LoginPartial.cshtml` → `User.Identity?.IsAuthenticated`
9. `@using Microsoft.AspNet.Identity` → `@using Microsoft.AspNetCore.Identity`
10. `User.Identity.GetUserName()` → `User.Identity?.Name`
11. `Views/Web.config` → `_ViewImports.cshtml` with `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`
12. Remove `_ViewStart.cshtml` references to MVC5 layout engine (file structure is similar)
13. `asp-validation-summary="ModelOnly"` tag helper in `AspNetSession/Index.cshtml` — already uses Core tag helper syntax (mixed usage, likely a copy-paste)

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/
- https://learn.microsoft.com/en-us/aspnet/core/migration/proper-to-2x/#update-razor-pages

**Complexity:** Moderate — many files but changes are largely mechanical

---

### Challenge 13: Web.config → appsettings.json
**Risk:** 🟡 Needs Work
**Files affected:**
- `Web.config` — connection strings, appSettings, EF config, system.web, assembly bindings
- `Web.Debug.config` / `Web.Release.config` — XML transforms
- `Views/Web.config` — Razor engine configuration
- `App.config` (eShopLegacy.Common) — EF provider config

**Current config values that need migration:**
- Connection strings: `CatalogDBContext`, `IdentityDBContext`
- App settings: `UseMockData`, `UseCustomizationData`, `files:BasePath`, `files:ServiceAccountId/Domain/Password`, `weather:ApiKey`, `NewItemQueuePath`
- Entity Framework section (provider registration)
- Session state mode (`InProc`)
- HTTP modules (telemetry, App Insights)
- Assembly binding redirects (no longer needed)

**Migration path:**
1. Create `appsettings.json` with connection strings and app settings
2. Use `builder.Configuration` / `IConfiguration` instead of `ConfigurationManager`
3. Remove XML transforms → use `appsettings.Development.json` / `appsettings.Production.json`
4. Remove assembly binding redirects (not needed in .NET Core)
5. Remove `Views/Web.config` → replaced by `_ViewImports.cshtml`
6. EF Core provider configured in code, not XML

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/
- https://learn.microsoft.com/en-us/aspnet/core/migration/configuration

**Complexity:** Moderate — must map every config value correctly

---

### Challenge 14: ConfigurationManager Static Usage
**Risk:** 🟡 Needs Work
**Files affected:**
- `Global.asax.cs` — `ConfigurationManager.AppSettings["UseMockData"]`
- `Models/Infrastructure/CatalogDBInitializer.cs` — `ConfigurationManager.AppSettings["UseCustomizationData"]`
- `Controllers/CatalogController.cs` — `ConfigurationManager.AppSettings["NewItemQueuePath"]`
- `Services/FileService.cs` — `ConfigurationManager.AppSettings["Files:*"]` (4 values)

**Migration path:**
1. Replace `ConfigurationManager.AppSettings[key]` → inject `IConfiguration` and use `configuration["key"]`
2. Consider strongly-typed options: `services.Configure<FileServiceConfiguration>(config.GetSection("Files"))`
3. Remove `System.Configuration` reference

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options

**Complexity:** Moderate — need to inject IConfiguration into classes that currently use static access

---

### Challenge 15: HttpContext.Current Static Access
**Risk:** 🟡 Needs Work
**Files affected:**
- `Global.asax.cs` — `HttpContext.Current.Session["MachineName"]`, `HttpContext.Current.Session["SessionStartTime"]`
- `Global.asax.cs` (`WebRequestInfo`) — `HttpContext.Current?.Request?.RawUrl`, `HttpContext.Current?.Request?.UserAgent`
- `eShopLegacy.Utilities/WebHelper.cs` — `HttpContext.Current.Request.UserAgent`
- `Views/Shared/_Layout.cshtml` — `HttpContext.Current.Session["MachineName"]`, `HttpContext.Current.Session["SessionStartTime"]`

**Current usage:** Static `HttpContext.Current` accessor used for session data and request info in both code-behind and Razor views.

**Migration path:**
1. `HttpContext.Current` doesn't exist in ASP.NET Core
2. In controllers/middleware: use `HttpContext` property directly
3. In services: inject `IHttpContextAccessor`
4. In Razor views: use `@Context.Session` or `@Context.Request`
5. systemweb-adapters can provide a shim during incremental migration
6. `WebHelper.cs` → needs refactor to accept `HttpContext` parameter or use DI

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-context
- https://learn.microsoft.com/en-us/aspnet/core/migration/http-modules#httpcontext

**Complexity:** Moderate — scattered across multiple projects

---

### Challenge 16: Server.MapPath / HostingEnvironment.ApplicationPhysicalPath
**Risk:** 🟡 Needs Work
**Files affected:**
- `Controllers/PicController.cs` — `Server.MapPath("~/Pics")`
- `Models/Infrastructure/CatalogDBInitializer.cs` — `HostingEnvironment.ApplicationPhysicalPath` (3 usages), `AppDomain.CurrentDomain.BaseDirectory`

**Migration path:**
1. `Server.MapPath("~/Pics")` → `Path.Combine(webHostEnvironment.WebRootPath, "Pics")`
2. `HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath`
3. `AppDomain.CurrentDomain.BaseDirectory` → works in .NET Core but `IWebHostEnvironment.ContentRootPath` preferred
4. Inject `IWebHostEnvironment` into controllers and services

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments

**Complexity:** Simple — straightforward replacement

---

### Challenge 17: System.Web.MimeMapping
**Risk:** 🟡 Needs Work
**Files affected:**
- `Controllers/DocumentsController.cs` — `MimeMapping.GetMimeMapping(filename)`

**Migration path:**
1. Replace with `new FileExtensionContentTypeProvider().TryGetContentType(filename, out string contentType)`
2. From `Microsoft.AspNetCore.StaticFiles` package (included in framework)

**Documentation:**
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.staticfiles.fileextensioncontenttypeprovider

**Complexity:** Simple — one-line replacement

---

### Challenge 18: HttpFileCollectionBase (File Uploads)
**Risk:** 🟡 Needs Work
**Files affected:**
- `Services/FileService.cs` — `UploadFile(HttpFileCollectionBase files)` method
- `Controllers/DocumentsController.cs` — `Request.Files`

**Migration path:**
1. `HttpFileCollectionBase` → `IFormFileCollection` or `List<IFormFile>`
2. `Request.Files` → `Request.Form.Files`
3. `file.InputStream` → `file.OpenReadStream()`
4. `file.FileName` → `file.FileName` (same)
5. Method signature: `UploadFile(IFormFileCollection files)`

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads

**Complexity:** Simple — API differences are minor

---

### Challenge 19: WindowsIdentity.Impersonate + P/Invoke (LogonUser)
**Risk:** 🟡 Needs Work
**Files affected:**
- `Services/FileService.cs` — `WindowsIdentity.Impersonate()`, `[DllImport("advapi32.dll")] LogonUser()`

**Current usage:** File service uses Windows impersonation to access network file shares with service account credentials. Uses P/Invoke to call `advapi32.dll!LogonUser`.

**Migration path:**
1. `WindowsIdentity.Impersonate()` → available via `Microsoft.Windows.Compatibility` NuGet (Windows Compatibility Pack) or built-in on .NET 10 for Windows
2. P/Invoke to `advapi32.dll` → works on Windows .NET Core but not cross-platform
3. Consider: this pattern only works on Windows. If cross-platform is a goal, redesign with Azure Storage or another file service
4. For Windows-only deployment: add `<RuntimeIdentifier>win-x64</RuntimeIdentifier>` and it should work

**Documentation:**
- https://learn.microsoft.com/en-us/dotnet/core/porting/windows-compat-pack
- https://learn.microsoft.com/en-us/dotnet/api/system.security.principal.windowsidentity.runimpersonated

**Complexity:** Moderate — works on Windows .NET Core but limits cross-platform portability

---

### Challenge 20: Application Insights (Framework SDK)
**Risk:** 🟡 Needs Work
**Files affected:**
- `ApplicationInsights.config` — full XML config with telemetry modules
- `Web.config` — `TelemetryCorrelationHttpModule`, `ApplicationInsightsHttpModule`
- `packages.config` — 7 Application Insights packages (v2.9.x)

**Current packages:**
- `Microsoft.ApplicationInsights` 2.9.1
- `Microsoft.ApplicationInsights.Agent.Intercept` 2.4.0
- `Microsoft.ApplicationInsights.DependencyCollector` 2.9.0
- `Microsoft.ApplicationInsights.PerfCounterCollector` 2.9.0
- `Microsoft.ApplicationInsights.Web` 2.9.0
- `Microsoft.ApplicationInsights.WindowsServer` 2.9.0
- `Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel` 2.9.1
- `Microsoft.AspNet.TelemetryCorrelation` 1.0.5

**Migration path:**
1. Remove all `.Web` and `.WindowsServer` packages (Framework-specific)
2. Add `Microsoft.ApplicationInsights.AspNetCore` (single package for ASP.NET Core)
3. Delete `ApplicationInsights.config` — config done in code
4. In `Program.cs`: `builder.Services.AddApplicationInsightsTelemetry()`
5. Remove HTTP modules from Web.config
6. **Alternative:** Migrate to OpenTelemetry (recommended by Microsoft for new development)

**Documentation:**
- https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core
- https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable

**Complexity:** Simple — single package replaces 7+ packages

---

### Challenge 21: log4net
**Risk:** 🟢 Straightforward
**Files affected:**
- `Global.asax.cs` — `LogManager.GetLogger(...)`, `LogicalThreadContext.Properties[...]`
- `Controllers/CatalogController.cs` — `LogManager.GetLogger(...)`, `_log.Info(...)`
- `Controllers/PicController.cs` — `LogManager.GetLogger(...)`, `_log.Info(...)`
- `log4Net.xml` — log4net configuration
- `packages.config` — `log4net 2.0.8`

**Current usage:** log4net used for logging with rolling file appender. Uses `LogicalThreadContext.Properties` for correlation.

**Migration path:**
- **Option A (keep log4net):** log4net 2.0.17+ supports .NET Standard 2.0 / .NET Core. Update package version and it works ✅
- **Option B (migrate to ILogger):** Replace with ASP.NET Core built-in `ILogger<T>` + `Microsoft.Extensions.Logging`. More idiomatic for .NET Core.
- `LogicalThreadContext.Properties` → ILogger scopes or `Activity.Current.Tags`

**Documentation:**
- https://logging.apache.org/log4net/
- NuGet: `log4net` 3.0.3 — supports .NET 6+ ✅

**Complexity:** Simple

---

### Challenge 22: Newtonsoft.Json
**Risk:** 🟢 Straightforward
**Files affected:**
- `Services/WeatherService.cs` — `JsonConvert.DeserializeAnonymousType()`
- `packages.config` — `Newtonsoft.Json 12.0.1`

**Migration path:**
- **Option A (keep Newtonsoft):** `Newtonsoft.Json` 13.x supports .NET Core/.NET 10 ✅
- **Option B (migrate to System.Text.Json):** `System.Text.Json.JsonSerializer.Deserialize<T>()` — built-in, no extra package. Anonymous type deserialization needs explicit type.

**Documentation:**
- https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/migrate-from-newtonsoft

**Complexity:** Simple

---

### Challenge 23: WebClient (Obsolete)
**Risk:** 🟢 Straightforward
**Files affected:**
- `Services/WeatherService.cs` — `new WebClient()`, `client.DownloadData(url)`

**Migration path:**
1. Replace `WebClient` → `HttpClient` (injected via `IHttpClientFactory`)
2. `client.DownloadData(url)` → `await client.GetByteArrayAsync(url)`
3. Make method `async`

**Documentation:**
- https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient

**Complexity:** Simple

---

### Challenge 24: HttpWebRequest (Obsolete)
**Risk:** 🟢 Straightforward
**Files affected:**
- `Models/IdentityModels.cs` — `HttpWebRequest.Create(uri)` in `ApplicationUser.ZipCode` getter

**Current usage:** Synchronous HTTP request in a property getter to look up user zip code. This is a design issue regardless of migration.

**Migration path:**
1. Replace with `HttpClient` via DI
2. Move from synchronous property getter to async method
3. Consider caching the value or loading it during authentication

**Complexity:** Simple (technically) but design refactoring recommended

---

### Challenge 25: System.Runtime.Remoting.Messaging
**Risk:** 🟡 Needs Work
**Files affected:**
- `Controllers/WebApi/BrandsController.cs` — `using System.Runtime.Remoting.Messaging` (imported but appears unused in the visible code)

**Current usage:** The `using` statement imports `CallContext` from remoting, which is NOT available in .NET Core.

**Migration path:**
1. If unused: simply remove the `using` statement
2. If used elsewhere: replace `CallContext` with `AsyncLocal<T>`

**Documentation:**
- https://learn.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1

**Complexity:** Simple — likely just removing an unused import

---

### Challenge 26: Non-SDK-Style Project Files
**Risk:** 🟡 Needs Work
**Files affected:**
- `eShopLegacy.Common/eShopLegacy.Common.csproj` — old-style
- `eShopLegacy.Utilities/eShopLegacy.Utilities.csproj` — old-style
- `eShopLegacyMVC/eShopLegacyMVC.csproj` — old-style (web project — NOT eligible for SDK conversion per skill file)
- `eShopLegacyMVC.Test/eShopLegacyMVC.Test.csproj` — old-style

**Migration path:** (per SDK-upgrading skill file)
1. Convert `eShopLegacy.Common` → SDK-style via `dnx upgrade-assistant upgrade`
2. Convert `eShopLegacy.Utilities` → SDK-style via `dnx upgrade-assistant upgrade`
3. Convert `eShopLegacyMVC.Test` → SDK-style via `dnx upgrade-assistant upgrade`
4. `eShopLegacyMVC` — **NOT eligible** for SDK conversion tool. Must be migrated side-by-side as a new ASP.NET Core project.
5. After SDK conversion, update target frameworks to `net10.0`
6. Delete `packages.config` files (replaced by `<PackageReference>` in csproj)

**Documentation:**
- Skill file: `.squad/skills/dotnet-sdk-upgrading/SKILL.md`
- https://learn.microsoft.com/en-us/nuget/consume-packages/migrate-packages-config-to-package-reference

**Complexity:** Moderate — tool-assisted but web project needs manual migration

---

### Challenge 27: Test Project Migration
**Risk:** 🟡 Needs Work
**Files affected:**
- `eShopLegacyMVC.Test/eShopLegacyMVC.Test.csproj` — references System.Web, System.Web.Mvc, System.Messaging, EntityFramework
- `Controllers/CatalogControllerTest.cs` — uses `System.Web.Mvc.ViewResult`, `HttpStatusCodeResult`, `RedirectToRouteResult`
- `Controllers/PicControllerTest.cs` — uses `Mock<HttpContextBase>`, `Mock<HttpServerUtilityBase>`, `Mock<HttpRequestBase>`, `ControllerContext`, `System.Web.Routing.RouteData`
- `Services/FileServiceTest.cs` — uses `Mock<HttpFileCollectionBase>`, `Mock<HttpPostedFileBase>`
- `Models/CatalogDBContextTest.cs` — uses `System.Data.Entity`, EF6 APIs
- `DbSetExtensions.cs` — EF6 `IQueryable<T>` extensions

**Migration path:**
1. Update `MSTest.TestFramework` + `MSTest.TestAdapter` to latest versions (3.x supports .NET 10)
2. `Moq` 4.20+ supports .NET Core ✅
3. `Castle.Core` 5.x supports .NET Core ✅
4. Replace `System.Web.Mvc` test types → `Microsoft.AspNetCore.Mvc` equivalents
5. Replace `Mock<HttpContextBase>` → `DefaultHttpContext` or `Mock<HttpContext>` (from ASP.NET Core)
6. Replace `Mock<HttpFileCollectionBase>` → `Mock<IFormFileCollection>`
7. Replace EF6 `DbSet` mocking → EF Core in-memory provider (`UseInMemoryDatabase`) or SQLite
8. `InMemoryCatalogDBContext` → replace with EF Core `DbContextOptions` pattern
9. Remove `System.Messaging` reference from test project

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- https://learn.microsoft.com/en-us/ef/core/testing/

**Complexity:** Moderate — significant mock rewrites needed for System.Web types

---

### Challenge 28: Database Initializer Pattern
**Risk:** 🟡 Needs Work
**Files affected:**
- `Models/Infrastructure/CatalogDBInitializer.cs` — `CreateDatabaseIfNotExists<CatalogDBContext>`, `Seed()` method

**Current usage:** EF6 database initializer creates HiLo sequences via SQL scripts, seeds catalog types/brands/items, and optionally extracts pictures from a ZIP file. Uses `HostingEnvironment.ApplicationPhysicalPath` for file paths.

**Migration path:**
1. `CreateDatabaseIfNotExists` → `context.Database.EnsureCreated()` + custom seed method
2. Or use EF Core Migrations for production-quality schema management
3. Seed data → `HasData()` in `OnModelCreating`, or custom `IHostedService` for startup seeding
4. SQL scripts for HiLo sequences → `context.Database.ExecuteSqlRaw(script)`
5. `HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath`

**Documentation:**
- https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
- https://learn.microsoft.com/en-us/ef/core/managing-schemas/ensure-created

**Complexity:** Moderate — complex seed logic with CSV parsing and ZIP extraction

---

### Challenge 29: Raw SQL Queries (Database.SqlQuery)
**Risk:** 🟡 Needs Work
**Files affected:**
- `Models/CatalogItemHiLoGenerator.cs` — `db.Database.SqlQuery<Int64>("SELECT NEXT VALUE FOR catalog_hilo;")`
- `Models/Infrastructure/CatalogDBInitializer.cs` — `db.Database.SqlQuery<Int64>(...)` and `db.Database.ExecuteSqlCommand(...)`

**Migration path:**
1. `Database.SqlQuery<T>()` → `Database.SqlQueryRaw<T>()` (EF Core 8+)
2. `Database.ExecuteSqlCommand()` → `Database.ExecuteSqlRaw()`
3. Parameter syntax may need updating (EF Core uses string interpolation for parameterized queries)

**Documentation:**
- https://learn.microsoft.com/en-us/ef/core/querying/sql-queries

**Complexity:** Simple — API rename with same functionality

---

### Challenge 30: OutputCache Attribute
**Risk:** 🟡 Needs Work
**Files affected:**
- `Controllers/DocumentsController.cs` — `[OutputCache(VaryByParam = "filename", Duration = int.MaxValue)]`

**Migration path:**
1. `[OutputCache]` → `[ResponseCache]` attribute in ASP.NET Core
2. Add response caching middleware: `builder.Services.AddResponseCaching()`; `app.UseResponseCaching()`
3. `VaryByParam` → `VaryByQueryKeys`

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/performance/caching/middleware

**Complexity:** Simple

---

### Challenge 31: Session State
**Risk:** 🟡 Needs Work
**Files affected:**
- `Web.config` — `<sessionState mode="InProc" />`
- `Global.asax.cs` — `Session_Start` writes to `HttpContext.Current.Session`
- `Controllers/AspNetSessionController.cs` — `HttpContext.Session["DemoItem"]` get/set with object
- `Views/Shared/_Layout.cshtml` — `HttpContext.Current.Session["MachineName"]`

**Current usage:** InProc session state storing objects (machine name, start time, `SessionDemoModel`).

**Migration path:**
1. Add `builder.Services.AddDistributedMemoryCache()` + `builder.Services.AddSession()`
2. Add `app.UseSession()` middleware
3. ASP.NET Core session stores bytes only — `HttpContext.Session.SetString()` / `GetString()`
4. For complex objects: serialize to JSON → `SetString(key, JsonSerializer.Serialize(obj))`
5. `SessionDemoModel` stored in session → needs serialization
6. systemweb-adapters can help bridge session access patterns during migration

**Documentation:**
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state#session-state

**Complexity:** Simple — but object session storage needs serialization wrapper

---

### Challenge 32: ViewBag / Dynamic SelectList Pattern
**Risk:** 🟢 Straightforward
**Files affected:**
- `Controllers/CatalogController.cs` — `ViewBag.CatalogBrandId = new SelectList(...)`

**Migration path:** Works the same in ASP.NET Core. No changes needed.

**Complexity:** Simple

---

## NuGet Package Compatibility Matrix

### eShopLegacyMVC (main web project)

| Package | Current Version | .NET 10 Equivalent | Status |
|---------|----------------|--------------------|---------| 
| `Antlr` | 3.5.0.2 | ❌ Remove (bundling dependency) | 🔴 Remove |
| `Autofac` | 4.9.1 | `Autofac` 8.x | 🟢 Update |
| `Autofac.Mvc5` | 4.0.2 | `Autofac.Extensions.DependencyInjection` 10.x | 🟡 Replace |
| `Autofac.WebApi2` | 4.3.1 | `Autofac.Extensions.DependencyInjection` 10.x | 🟡 Replace |
| `bootstrap` | 4.3.1 | Client-side: keep or update | 🟢 Keep |
| `EntityFramework` | 6.2.0 | `Microsoft.EntityFrameworkCore.SqlServer` 10.x | 🟡 Replace |
| `jQuery` | 3.3.1 | Client-side: keep or update | 🟢 Keep |
| `jQuery.Validation` | 1.17.0 | Client-side: keep or update | 🟢 Keep |
| `log4net` | 2.0.8 | `log4net` 3.0.3 or `Microsoft.Extensions.Logging` | 🟢 Update |
| `Microsoft.ApplicationInsights` | 2.9.1 | `Microsoft.ApplicationInsights.AspNetCore` | 🟡 Replace |
| `Microsoft.ApplicationInsights.Agent.Intercept` | 2.4.0 | ❌ Remove | 🔴 Remove |
| `Microsoft.ApplicationInsights.DependencyCollector` | 2.9.0 | Included in AspNetCore pkg | 🔴 Remove |
| `Microsoft.ApplicationInsights.PerfCounterCollector` | 2.9.0 | Included in AspNetCore pkg | 🔴 Remove |
| `Microsoft.ApplicationInsights.Web` | 2.9.0 | ❌ Framework-only | 🔴 Remove |
| `Microsoft.ApplicationInsights.WindowsServer` | 2.9.0 | ❌ Framework-only | 🔴 Remove |
| `Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel` | 2.9.1 | ❌ Framework-only | 🔴 Remove |
| `Microsoft.AspNet.Identity.Core` | 2.2.3 | `Microsoft.AspNetCore.Identity` | 🟡 Replace |
| `Microsoft.AspNet.Identity.EntityFramework` | 2.2.3 | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 🟡 Replace |
| `Microsoft.AspNet.Identity.Owin` | 2.2.3 | ❌ Remove (Core Identity has no OWIN) | 🔴 Remove |
| `Microsoft.AspNet.Mvc` | 5.2.7 | Built into ASP.NET Core framework | 🔴 Remove |
| `Microsoft.AspNet.Razor` | 3.2.7 | Built into ASP.NET Core framework | 🔴 Remove |
| `Microsoft.AspNet.SessionState.SessionStateModule` | 1.1.0 | ❌ Remove (Core has built-in session) | 🔴 Remove |
| `Microsoft.AspNet.TelemetryCorrelation` | 1.0.5 | ❌ Remove | 🔴 Remove |
| `Microsoft.AspNet.Web.Optimization` | 1.1.3 | `LigerShark.WebOptimizer.Core` or remove | 🟡 Replace |
| `Microsoft.AspNet.WebApi.Client` | 5.2.7 | ❌ Remove | 🔴 Remove |
| `Microsoft.AspNet.WebApi.Core` | 5.2.7 | Built into ASP.NET Core framework | 🔴 Remove |
| `Microsoft.AspNet.WebApi.WebHost` | 5.2.7 | ❌ Remove | 🔴 Remove |
| `Microsoft.AspNet.WebPages` | 3.2.7 | Built into ASP.NET Core framework | 🔴 Remove |
| `Microsoft.Bcl.AsyncInterfaces` | 1.1.0 | Built into .NET runtime | 🔴 Remove |
| `Microsoft.CodeDom.Providers.DotNetCompilerPlatform` | 2.0.1 | ❌ Remove (Roslyn is default in .NET Core) | 🔴 Remove |
| `Microsoft.jQuery.Unobtrusive.Validation` | 3.2.11 | Client-side: keep | 🟢 Keep |
| `Microsoft.Net.Compilers` | 4.2.0 | ❌ Remove (dev dependency) | 🔴 Remove |
| `Microsoft.Owin` | 4.2.2 | ❌ Remove | 🔴 Remove |
| `Microsoft.Owin.Host.SystemWeb` | 4.2.2 | ❌ Remove | 🔴 Remove |
| `Microsoft.Owin.Security` | 4.2.2 | ❌ Remove | 🔴 Remove |
| `Microsoft.Owin.Security.Cookies` | 4.2.2 | Built into ASP.NET Core auth | 🔴 Remove |
| `Microsoft.Owin.Security.OAuth` | 3.0.1 | ❌ Remove | 🔴 Remove |
| `Microsoft.Web.Infrastructure` | 1.0.0.0 | ❌ Remove | 🔴 Remove |
| `Modernizr` | 2.8.3 | Client-side: optional keep | 🟢 Optional |
| `Newtonsoft.Json` | 12.0.1 | `Newtonsoft.Json` 13.x or `System.Text.Json` | 🟢 Update |
| `Owin` | 1.0 | ❌ Remove | 🔴 Remove |
| `Pipelines.Sockets.Unofficial` | 1.0.7 | ❌ Review if still needed | 🟡 Review |
| `popper.js` | 1.14.3 | Client-side: included in Bootstrap 5 | 🟢 Keep |
| `Respond` | 1.4.2 | Client-side: optional (IE polyfill) | 🟢 Optional |
| `System.Buffers` | 4.5.1 | Built into .NET runtime | 🔴 Remove |
| `System.Diagnostics.DiagnosticSource` | 4.7.1 | Built into .NET runtime | 🔴 Remove |
| `System.Diagnostics.PerformanceCounter` | 4.5.0 | Available in Windows Compat Pack | 🟡 Review |
| `System.IO.Compression` | 4.3.0 | Built into .NET runtime | 🔴 Remove |
| `System.IO.Compression.ZipFile` | 4.3.0 | Built into .NET runtime | 🔴 Remove |
| `System.IO.Pipelines` | 4.5.1 | Built into .NET runtime | 🔴 Remove |
| `System.Memory` | 4.5.4 | Built into .NET runtime | 🔴 Remove |
| `System.Numerics.Vectors` | 4.5.0 | Built into .NET runtime | 🔴 Remove |
| `System.Runtime.CompilerServices.Unsafe` | 4.5.3 | Built into .NET runtime | 🔴 Remove |
| `System.Threading.Channels` | 4.5.0 | Built into .NET runtime | 🔴 Remove |
| `System.Threading.Tasks.Extensions` | 4.5.2 | Built into .NET runtime | 🔴 Remove |
| `WebGrease` | 1.6.0 | ❌ Remove (bundling dependency) | 🔴 Remove |

### eShopLegacy.Common

| Package | Current Version | .NET 10 Equivalent | Status |
|---------|----------------|--------------------|---------| 
| `EntityFramework` | 6.0.0 | `Microsoft.EntityFrameworkCore.SqlServer` 10.x | 🟡 Replace |

### eShopLegacyMVC.Test

| Package | Current Version | .NET 10 Equivalent | Status |
|---------|----------------|--------------------|---------| 
| `Castle.Core` | 5.1.1 | `Castle.Core` 5.x | 🟢 Compatible |
| `Microsoft.AspNet.Mvc` | 5.2.7 | ❌ Remove (use ASP.NET Core MVC) | 🔴 Remove |
| `Microsoft.AspNet.Razor` | 3.2.7 | ❌ Remove | 🔴 Remove |
| `Microsoft.AspNet.WebPages` | 3.2.7 | ❌ Remove | 🔴 Remove |
| `Microsoft.Web.Infrastructure` | 1.0.0.0 | ❌ Remove | 🔴 Remove |
| `Moq` | 4.20.72 | `Moq` 4.20+ | 🟢 Compatible |
| `MSTest.TestAdapter` | 2.2.10 | `MSTest.TestAdapter` 3.x | 🟢 Update |
| `MSTest.TestFramework` | 2.2.10 | `MSTest.TestFramework` 3.x | 🟢 Update |
| `System.Runtime.CompilerServices.Unsafe` | 4.5.3 | Built into .NET runtime | 🔴 Remove |
| `System.Threading.Tasks.Extensions` | 4.5.4 | Built into .NET runtime | 🔴 Remove |

**Summary:** Of ~58 unique NuGet packages: **28 must be removed**, **10 need replacement**, **10 can be updated**, **5 are client-side (keep)**, **5 need review**.

---

## Recommended Migration Order

Based on the dependency graph, risk levels, and the skill files' guidance:

### Phase 1: SDK-Style Conversion (Non-Web Projects)
**Priority:** Do first — required before changing target frameworks
1. ✅ `eShopLegacy.Common` → SDK-style csproj (via upgrade-assistant)
2. ✅ `eShopLegacy.Utilities` → SDK-style csproj (via upgrade-assistant)
3. ✅ `eShopLegacyMVC.Test` → SDK-style csproj (via upgrade-assistant)
4. ⚠️ `eShopLegacyMVC` — NOT eligible for SDK conversion tool, handled in Phase 3

### Phase 2: Retarget Shared Libraries to .NET 10
**Priority:** Bottom-up by dependency graph
1. `eShopLegacy.Common` → change TFM to `net10.0`, replace `EntityFramework` → `Microsoft.EntityFrameworkCore`, fix `BinaryFormatter` removal
2. `eShopLegacy.Utilities` → change TFM to `net10.0`, replace `System.Web.HttpContext.Current` with DI-friendly pattern

### Phase 3: Side-by-Side Web App Migration
**Priority:** Most complex, do after libraries
1. Create new ASP.NET Core `Program.cs` entry point
2. Migrate configuration (Web.config → appsettings.json)
3. Set up ASP.NET Core DI (Autofac or built-in)
4. Migrate Entity Framework → EF Core (CatalogDBContext, Identity DB)
5. Migrate controllers (MVC + Web API → unified ASP.NET Core controllers)
6. Migrate OWIN auth → ASP.NET Core Identity
7. Migrate MSMQ → Experimental.System.Messaging
8. Migrate views (Razor MVC5 → Razor ASP.NET Core)
9. Replace bundling/minification
10. Migrate Application Insights → AspNetCore package
11. Update logging (log4net or ILogger)

### Phase 4: Test Project Migration
**Priority:** After web app compiles
1. Retarget `eShopLegacyMVC.Test` to `net10.0`
2. Update MSTest packages to 3.x
3. Rewrite System.Web mocks → ASP.NET Core test infrastructure
4. Update EF6 test patterns → EF Core in-memory provider

### Phase 5: Cleanup & Verification
1. Remove all `packages.config` files
2. Remove `Web.config`, `Global.asax`, `ApplicationInsights.config`
3. Remove `App_Start/` folder
4. Remove assembly binding redirects
5. Build with zero warnings
6. Run all tests
7. Verify application starts successfully

---

## Key Risks & Blockers Summary

| Risk Level | Count | Items |
|------------|-------|-------|
| 🔴 Blocker | 6 | MVC5 controllers, WebAPI2, OWIN, Identity, System.Web pervasive, Global.asax, BinaryFormatter |
| 🟡 Needs Work | 19 | Autofac, EF6, MSMQ, Bundling, Razor views, Config, Session, File uploads, Impersonation, App Insights, etc. |
| 🟢 Straightforward | 7 | log4net, Newtonsoft.Json, WebClient, HttpWebRequest, ViewBag, SelectList |

**Estimated total effort:** Complex migration — the web project requires side-by-side rewrite, not incremental conversion. The blockers (MVC5, OWIN, Identity, System.Web) are deeply interconnected and must be addressed together.

**Biggest risk:** The ASP.NET Identity + OWIN + MVC5 authentication stack is the most tightly coupled area. All three must be migrated simultaneously since they share context (`IOwinContext`, `HttpContext.GetOwinContext()`, cookie middleware).

**Positive factors:** The business logic (services, models) is well-separated from web concerns. The `ICatalogService` interface makes the service layer testable and portable. The shared libraries (`Common`, `Utilities`) are small and straightforward to convert first.
