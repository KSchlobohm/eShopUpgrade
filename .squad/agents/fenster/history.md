# Fenster — Project History

## Project Context
- **Project:** eShopLegacyMVC — .NET Framework 4.7.2 e-commerce app upgrading to .NET 10
- **Owner:** Ken Schlobohm
- **Stack:** ASP.NET MVC 5, Web API 2, Entity Framework 6, Autofac, MSMQ (System.Messaging), MSTest
- **Target:** .NET 10, ASP.NET Core, EF Core
- **Done Criteria:** Code compiles without warnings and app starts successfully

## Research Focus Areas
- .NET Framework → .NET 10 migration challenges for this specific codebase
- NuGet package compatibility (Autofac, log4net, Application Insights, etc.)
- ASP.NET MVC 5 → ASP.NET Core migration patterns
- Entity Framework 6 → EF Core migration guide
- systemweb-adapters capabilities and limitations
- MSMQ alternatives and Experimental.System.Messaging viability

## Learnings

<!-- Append entries below -->

### 2025-03-03: M3-T1 — Complete .NET 10 Package Compatibility Research

**Task:** Inventory all NuGet packages and determine .NET 10-compatible versions for every package in solution.

**Research Findings (62 unique packages analyzed):**

- **Remove (31 packages — 🔴 Critical):** ASP.NET MVC 5 (Mvc, WebPages, Razor, WebApi.*, Web.Optimization), OWIN/Katana (Microsoft.Owin.*, Owin), ASP.NET Identity v2 (Identity.Core/EntityFramework/Owin), Session state (SessionStateModule), obsolete build/infrastructure (CodeDom, Net.Compilers, Web.Infrastructure, TelemetryCorrelation), legacy DI (Autofac.Mvc5, Autofac.WebApi2), Application Insights interceptor, legacy frontend tools (WebGrease, Respond).

- **Replace (3 packages — 🟡 Medium Risk):**
  - Autofac 4.9.1 → Autofac 9.0.0 (+ add Autofac.Extensions.DependencyInjection 10.0.0 for ASP.NET Core integration)
  - Autofac.Mvc5, Autofac.WebApi2 → Autofac.Extensions.DependencyInjection 10.0.0

- **Update (5 packages — 🟡 Medium Risk):**
  - EntityFramework 6.2.0 → EF Core 9.0.0 (major rewrite: Linq changes, DbContext setup, migrations)
  - Microsoft.ApplicationInsights (2.9.1 → 3.0.0) and modules (2.9.0 → 3.0.0): Breaking changes due to OpenTelemetry rebuild. Telemetry processors, initializers rewritten.
  - log4net 2.0.8 → 3.3.0 (straightforward; targets .NET Standard 2.0)
  - Newtonsoft.Json 12.0.1 → 13.0.4 (security fix for CVE-2024-21907; no breaking changes 12→13)
  - Castle.Core 5.1.1 → 5.2.1 (supports .NET 10; no breaking changes)

- **Keep (23 packages — 🟢 Low Risk):**
  - System.* NuGet polyfills (Buffers, Memory, IO.Pipelines, Numerics.Vectors, Runtime.CompilerServices.Unsafe, Threading.Channels, Threading.Tasks.Extensions, Diagnostics.DiagnosticSource, Diagnostics.PerformanceCounter, IO.Compression.*): All target .NET Standard 2.0, compatible with .NET 10. Most are built-in but safe to keep explicit references.
  - Testing packages (Microsoft.NET.Test.Sdk 17.3.2, MSTest.TestFramework/Adapter 2.2.10, Moq 4.20.72): Full .NET 10 support.
  - Client-side libraries (jQuery, jQuery.Validation, bootstrap, popper.js, Modernizr, Microsoft.jQuery.Unobtrusive.Validation): Can stay in packages.config or migrate to npm.
  - Microsoft.Bcl.AsyncInterfaces 1.1.0, Pipelines.Sockets.Unofficial 1.0.7: Standard 2.0 targets.

**Critical Decision Points:**
1. **Application Insights v3.0.0 is breaking:** All processor/initializer patterns change. Requires code review before upgrade.
2. **EF Core 9.0.0 is not a drop-in replacement for EF 6:** Requires migration planning (M6). Query syntax changes, DbContext initialization, SaveChangesAsync conversions, Linq.to.Entities vs Linq.to.Objects edge cases.
3. **OWIN removal is total:** No compatibility layer available. All authentication middleware must be rewritten using ASP.NET Core authentication pipeline.
4. **New ASP.NET Core packages needed (not currently in solution):** Autofac.Extensions.DependencyInjection 10.0.0, Microsoft.AspNetCore.Identity 10.0.0, Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.0, Microsoft.EntityFrameworkCore 9.0.0, Microsoft.EntityFrameworkCore.SqlServer 9.0.0.

**Artifact:** Complete compatibility matrix with 7 detailed tables covering frameworks, DI, data access, logging, testing, client-side libraries, and system APIs. See `docs/migration/m3-package-compatibility.md`.

**Impact on McManus (Code):** Detailed action list for each package. 31 packages to remove, 3 to replace, 5 to update. Provides exact NuGet versions for M3-T2 and M3-T3.

### 2025-06-30: Comprehensive Upgrade Challenge Research
- **32 distinct challenges identified** across the solution (6 blockers, 19 needs-work, 7 straightforward)
- **Blockers:** ASP.NET MVC 5 (15+ files), Web API 2 (4 files), OWIN/Katana (3 files), ASP.NET Identity v2 (5 files), System.Web pervasive (20+ files), Global.asax lifecycle, BinaryFormatter (`eShopLegacy.Common/Utilities/Serializing.cs`)
- **BinaryFormatter is removed in .NET 9+** — hard blocker for `Serializing.cs` and `FilesController.cs`
- **58 NuGet packages analyzed:** 28 remove, 10 replace, 10 update, 5 client-side keep, 5 review
- **Dependency graph:** Common → Utilities → MVC → Test (bottom-up migration order)
- **eShopLegacyMVC is NOT eligible for SDK-style conversion tool** — must be side-by-side migration
- **Auth stack (Identity + OWIN + MVC5)** is the most tightly coupled area — all three must migrate together
- **`ApplicationUser.ZipCode`** makes synchronous HttpWebRequest in property getter — design smell
- **`System.Runtime.Remoting.Messaging`** imported in `BrandsController.cs` — not available in .NET Core
- **Session state** stores objects directly — ASP.NET Core requires byte serialization
- **Full report:** `.squad/research/upgrade-challenges.md`
