# Dependency Injection Migration — Autofac to Built-in DI

## Current State

### Global.asax.cs — RegisterContainer()
```csharp
var builder = new ContainerBuilder();
builder.RegisterControllers(thisAssembly);           // Autofac.Mvc5
builder.RegisterApiControllers(thisAssembly);         // Autofac.WebApi2
builder.RegisterModule(new ApplicationModule(mockData));
var container = builder.Build();
DependencyResolver.SetResolver(new AutofacDependencyResolver(container));      // MVC
GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container); // WebAPI
```

### Modules/ApplicationModule.cs
```csharp
public class ApplicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        if (useMockData)
            builder.RegisterType<CatalogServiceMock>().As<ICatalogService>().SingleInstance();
        else
            builder.RegisterType<CatalogService>().As<ICatalogService>().InstancePerLifetimeScope();

        builder.RegisterType<CatalogDBContext>().InstancePerLifetimeScope();
        builder.RegisterType<CatalogDBInitializer>().InstancePerLifetimeScope();
        builder.RegisterType<CatalogItemHiLoGenerator>().SingleInstance();
    }
}
```

### NuGet Packages
- `Autofac` 4.9.1
- `Autofac.Mvc5` 4.0.2
- `Autofac.WebApi2` 4.3.1

## Challenge
- `Autofac.Mvc5` and `Autofac.WebApi2` are Framework-only packages.
- ASP.NET Core has a built-in DI container that handles most scenarios.
- The registration patterns map cleanly:
  - `RegisterType<T>().As<I>()` → `services.AddScoped<I, T>()`
  - `SingleInstance()` → `services.AddSingleton<T>()`
  - `InstancePerLifetimeScope()` → `services.AddScoped<T>()`
- Controller registration is automatic in ASP.NET Core (`AddControllersWithViews()`).
- The conditional `useMockData` registration is straightforward with `if/else` in Program.cs.

## Migration Plan

### Replace with built-in DI in Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

var useMockData = builder.Configuration.GetValue<bool>("UseMockData");

if (useMockData)
{
    builder.Services.AddSingleton<ICatalogService, CatalogServiceMock>();
}
else
{
    builder.Services.AddScoped<ICatalogService, CatalogService>();
}

builder.Services.AddDbContext<CatalogDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDBContext")));
builder.Services.AddScoped<CatalogDBInitializer>();
builder.Services.AddSingleton<CatalogItemHiLoGenerator>();
builder.Services.AddScoped<FileService>();

builder.Services.AddControllersWithViews();
```

### Cleanup
1. Remove `Autofac`, `Autofac.Mvc5`, `Autofac.WebApi2` PackageReferences
2. Delete `Modules/ApplicationModule.cs`
3. Remove `RegisterContainer()` from Global.asax.cs (already removed in M5)
4. Remove DependencyResolver and GlobalConfiguration setup

## Actions
- [ ] Add service registrations in Program.cs
- [ ] Remove Autofac packages from csproj
- [ ] Delete Modules/ApplicationModule.cs
- [ ] Remove Autofac-related code from Global.asax.cs (if not already removed)
- [ ] Build and verify controller DI resolution

## Verification
- No Autofac packages in any csproj
- No `using Autofac` statements in any source file
- All services registered in Program.cs via builder.Services
- Controllers receive dependencies via constructor injection
- `dotnet build` succeeds

## References
- [Dependency injection in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [Autofac ASP.NET Core integration](https://autofac.readthedocs.io/en/latest/integration/aspnetcore.html) (alternative: keep Autofac)
