# Routing and Middleware Migration

## Current State

### RouteConfig.cs
```csharp
routes.MapMvcAttributeRoutes();
routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
routes.MapRoute(
    name: "Default",
    url: "{controller}/{action}/{id}",
    defaults: new { controller = "Catalog", action = "Index", id = UrlParameter.Optional }
);
```

### WebApiConfig.cs
```csharp
config.MapHttpAttributeRoutes();
config.Routes.MapHttpRoute(
    name: "DefaultApi",
    routeTemplate: "api/{controller}/{id}",
    defaults: new { id = RouteParameter.Optional }
);
```

### FilterConfig.cs
```csharp
filters.Add(new HandleErrorAttribute());
```

### BundleConfig.cs
Configures script/style bundles for jQuery, Bootstrap, Modernizr, and custom CSS using `System.Web.Optimization`.

### Global.asax.cs Application_Start
```csharp
GlobalConfiguration.Configure(WebApiConfig.Register);
AreaRegistration.RegisterAllAreas();
FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
RouteConfig.RegisterRoutes(RouteTable.Routes);
BundleConfig.RegisterBundles(BundleTable.Bundles);
```

## Challenge
- `System.Web.Routing` and `System.Web.Http` routing are replaced by ASP.NET Core endpoint routing.
- `System.Web.Optimization` bundling has no direct equivalent in ASP.NET Core. Options: WebOptimizer, LibMan, or manual wwwroot management.
- `HandleErrorAttribute` has a different implementation in ASP.NET Core.
- `AreaRegistration` is handled differently in ASP.NET Core (areas still supported but configured differently).
- The `.axd` ignore route is not needed in ASP.NET Core.

## Migration Plan

### Routing → Program.cs
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

// Attribute routing is enabled by default with AddControllersWithViews()
```

### Filters → Program.cs
```csharp
builder.Services.AddControllersWithViews(options =>
{
    // Error handling is typically done via middleware in Core
});

// Or use exception handling middleware:
app.UseExceptionHandler("/Home/Error");
```

### Bundling → Static Files
Replace `System.Web.Optimization` with:
1. Serve static files from `wwwroot/` via `app.UseStaticFiles()`
2. Reference CSS/JS directly in `_Layout.cshtml`:
   ```html
   <link rel="stylesheet" href="~/css/bootstrap.css" />
   <script src="~/js/jquery-3.3.1.js"></script>
   ```
3. Move client-side assets from Content/Scripts to wwwroot/css and wwwroot/js
4. Replace `@Styles.Render("~/Content/css")` and `@Scripts.Render("~/bundles/jquery")` with direct `<link>` and `<script>` tags

## Actions
- [ ] Configure endpoint routing in Program.cs
- [ ] Remove App_Start/RouteConfig.cs (or keep as reference)
- [ ] Remove App_Start/WebApiConfig.cs
- [ ] Replace FilterConfig with ASP.NET Core exception handling middleware
- [ ] Replace BundleConfig with static file serving
- [ ] Move Content/ and Scripts/ to wwwroot/
- [ ] Update _Layout.cshtml to reference static files directly

## Verification
- `/` routes to CatalogController.Index
- `/api/Brands` routes to BrandsController.Get
- `/items/{id}/pic` routes to PicController.Index (attribute route)
- Static CSS/JS files load in browser
- No `System.Web.Routing`, `System.Web.Http`, or `System.Web.Optimization` references

## References
- [Routing in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/routing)
- [Static files in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/static-files)
- [Migrate from ASP.NET bundling](https://learn.microsoft.com/aspnet/core/migration/proper-to-2x/#mvc-bundling-and-minification)
