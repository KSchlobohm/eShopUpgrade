# ASP.NET Core Project Setup — Side-by-Side Migration

## Current State
`eShopLegacyMVC` is an ASP.NET MVC 5 / Web API 2 application with:
- **Legacy csproj** with 59 NuGet packages via `packages.config`
- **Global.asax.cs**: Application_Start bootstraps Autofac DI, registers areas/filters/routes/bundles, configures database
- **Startup.cs**: OWIN startup that calls ConfigureAuth for cookie authentication
- **App_Start/**: RouteConfig, WebApiConfig, FilterConfig, BundleConfig, IdentityConfig, Startup.Auth
- **Web.config**: Connection strings, app settings, assembly redirects, HTTP modules, Entity Framework config

The project depends on `System.Web` throughout — controllers, HTTP context, session, routing, bundling, and more.

## Challenge
- ASP.NET MVC 5 is fundamentally incompatible with .NET 10. There is no upgrade path — it must be **rewritten** as ASP.NET Core.
- The csproj must change from legacy format to SDK-style with `Microsoft.NET.Sdk.Web`.
- The hosting model changes from `HttpApplication` (Global.asax) to `WebApplication` (Program.cs).
- All `System.Web.*` namespaces must be replaced with `Microsoft.AspNetCore.*` equivalents.
- The `packages.config` with 59 packages must be replaced with targeted ASP.NET Core PackageReferences.
- OWIN middleware must become ASP.NET Core middleware.

## Migration Plan

### Step 1: Create SDK-style csproj
Replace the entire csproj with:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\eShopLegacy.Common\eShopLegacy.Common.csproj" />
    <ProjectReference Include="..\eShopLegacy.Utilities\eShopLegacy.Utilities.csproj" />
  </ItemGroup>
  <ItemGroup>
    <!-- Add packages incrementally as migration proceeds -->
  </ItemGroup>
</Project>
```

### Step 2: Create Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services (DI registration — replaces Autofac in M7)
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
```

### Step 3: Remove legacy files
- Delete `Global.asax` and `Global.asax.cs` (replaced by Program.cs)
- Delete `Startup.cs` (OWIN startup — replaced by Program.cs)
- Delete `packages.config` (replaced by PackageReferences)
- Keep source files for incremental migration

### Step 4: Incremental controller migration
Port controllers one at a time, replacing `System.Web.Mvc` with `Microsoft.AspNetCore.Mvc`.

## Actions
- [ ] Replace eShopLegacyMVC.csproj with SDK-style Web project
- [ ] Create Program.cs with minimal WebApplication setup
- [ ] Delete Global.asax, Global.asax.cs
- [ ] Delete OWIN Startup.cs
- [ ] Delete packages.config
- [ ] Add required ASP.NET Core PackageReferences
- [ ] Verify project loads and restore succeeds

## Verification
- csproj uses `Microsoft.NET.Sdk.Web` SDK
- Program.cs exists with WebApplication builder
- `dotnet restore eShopLegacyMVC` succeeds
- No `Global.asax`, `Startup.cs` (OWIN), or `packages.config`

## References
- `.squad/skills/systemweb-adapters/SKILL.md`
- `.squad/skills/dotnet-porting/SKILL.md`
- [Migrate from ASP.NET to ASP.NET Core](https://learn.microsoft.com/aspnet/core/migration/proper-to-2x/)
- [Incremental ASP.NET migration](https://learn.microsoft.com/aspnet/core/migration/inc/overview)
