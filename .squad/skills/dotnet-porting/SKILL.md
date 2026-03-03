---
name: "dotnet-porting"
description: "General guidance for porting .NET Framework apps to modern .NET."
domain: "dotnet-migration"
confidence: "high"
source: "https://learn.microsoft.com/en-us/dotnet/core/porting/"
---

## .NET Framework to .NET Porting Guide

Official Microsoft guidance for migrating from .NET Framework to modern .NET.

### Key Steps

1. **Analyze dependencies** — Use .NET Portability Analyzer or `try-convert` to identify compatibility
2. **Convert project format** — Move to SDK-style csproj (for non-web projects)
3. **Update NuGet packages** — Replace .NET Framework-only packages with .NET Core equivalents
4. **Address API incompatibilities** — Use Windows Compatibility Pack, systemweb-adapters, or rewrite
5. **Test** — Run existing tests, add migration-specific tests

### Key Resources

- [Overview](https://learn.microsoft.com/en-us/dotnet/core/porting/)
- [.NET Upgrade Assistant](https://learn.microsoft.com/en-us/dotnet/core/porting/upgrade-assistant-overview)
- [Breaking changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/)
- [Windows Compatibility Pack](https://learn.microsoft.com/en-us/dotnet/core/porting/windows-compat-pack)
- [ASP.NET to ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/migration/proper-to-2x/)

### Migration Order Best Practice

1. Shared libraries (no web dependencies) — convert to SDK-style, multi-target if needed
2. Test projects — convert to SDK-style, update test framework
3. Web application — side-by-side migration using systemweb-adapters
4. Remove legacy Framework targeting after full migration

### Common Challenges for This Repo

| Challenge | Impact | Mitigation |
|-----------|--------|------------|
| System.Web (HttpContext, Session, etc.) | 🔴 High | systemweb-adapters + incremental migration |
| MSMQ (System.Messaging) | 🔴 High | Experimental.System.Messaging package |
| Entity Framework 6 | 🟡 Medium | EF Core migration, may need query rewrites |
| Autofac DI | 🟡 Medium | Autofac has ASP.NET Core support package |
| ASP.NET Identity v2 | 🟡 Medium | ASP.NET Core Identity migration |
| OWIN middleware | 🟡 Medium | Replace with ASP.NET Core middleware |
| log4net | 🟢 Low | Works on .NET Core, or switch to built-in logging |
| Application Insights | 🟢 Low | Updated packages available for .NET Core |
| Web.config → appsettings.json | 🟢 Low | Configuration system migration |
