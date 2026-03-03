# Keaton — Project History

## Project Context
- **Project:** eShopLegacyMVC — .NET Framework 4.7.2 e-commerce app upgrading to .NET 10
- **Owner:** Ken Schlobohm
- **Stack:** ASP.NET MVC 5, Web API 2, Entity Framework 6, Autofac, MSMQ (System.Messaging), MSTest
- **Target:** .NET 10, ASP.NET Core, EF Core
- **Done Criteria:** Code compiles without warnings and app starts successfully
- **Solution:** 4 projects — eShopLegacyMVC (web), eShopLegacy.Common (library), eShopLegacy.Utilities (library), eShopLegacyMVC.Test (tests)

## Key Resources
- systemweb-adapters for incremental System.Web migration
- dnx upgrade-assistant for SDK-style csproj conversion (non-web projects only)
- Experimental.System.Messaging for MSMQ replacement
- Official .NET porting guide at learn.microsoft.com

## Learnings

<!-- Append entries below -->

### Migration Plan Structure (2026-03-03)
- Created 12-milestone plan (M0–M11) stored in `tasks.json` with schema in `tasks-schema.json`
- 18 challenge documents in `docs/migration/` covering every significant migration concern
- **Topological order is critical**: Common (leaf) → Utilities → Test → Web project
- **SDK-style conversion (M1) must happen before target framework changes (M4)** — these are distinct steps per the skill guidance
- **Web project is NEVER converted to SDK-style on Framework** — it goes directly to `Microsoft.NET.Sdk.Web` during the side-by-side ASP.NET Core migration in M5
- **MSMQ replacement (M2) is a clean namespace swap** — low risk, do it early
- **M5 (Web project migration) is the critical path** — 9 sub-tasks covering project setup, controllers, Web API, routing, views, services, telemetry
- **EF6 to EF Core (M6) and Identity (M8) are the two hardest data-layer migrations** — both require API surface rewrites
- **BinaryFormatter in Serializing.cs is a hidden blocker** — it throws at runtime on .NET 5+ and must be replaced before retargeting
- **System.Web usings in CatalogBrand.cs and CatalogType.cs are unused** — safe to remove, but easy to miss
- **WebHelper.cs (HttpContext.Current.Request.UserAgent) blocks Utilities retargeting** — must be redesigned or removed
- **System.Runtime.Remoting.Messaging in BrandsController is unused** — dead import, remove
- Dependency graph: Common(0 deps) → Utilities(Common) → MVC(Common,Utilities) → Test(Common,MVC)
