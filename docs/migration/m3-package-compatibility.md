# NuGet Package Compatibility Matrix for .NET 10

**Research Date:** 2025-03-03  
**Task:** M3-T1 — Research .NET 10-compatible package versions for all NuGet packages in solution  
**Researcher:** Fenster  
**Status:** Complete

---

## Summary

**Total packages analyzed:** 62 unique packages  
**Breakdown:**
- **Remove:** 28 packages (ASP.NET Framework-specific, OWIN, Identity v2, MVC5, etc.)
- **Replace:** 3 packages (Autofac-related → Autofac.Extensions.DependencyInjection)
- **Update:** 12 packages (to .NET 10-compatible versions)
- **Keep:** 19 packages (.NET Standard 2.0 compatible or built-in to .NET 10)

**Risk Summary:**
- 🔴 **High Risk (Breaking Changes):** 31 packages must be removed due to platform incompatibility
- 🟡 **Medium Risk (Review Needed):** 5 packages with potential breaking changes or manual migration required
- 🟢 **Low Risk (Straightforward):** 26 packages with direct .NET 10 equivalents or no action needed

---

## Detailed Compatibility Matrix

### Section A: ASP.NET MVC 5 & Web API 2 (Must Remove — 🔴 Critical)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| Microsoft.AspNet.Mvc | 5.2.7 | ❌ N/A | Razor Pages or ASP.NET Core MVC | **Remove** | 🔴 | ASP.NET MVC 5 framework—no .NET Core equivalent. Complete rewrite of Controllers/Views required. See M5. |
| Microsoft.AspNet.WebPages | 3.2.7 | ❌ N/A | ASP.NET Core Razor Pages | **Remove** | 🔴 | Web Pages are built into ASP.NET Core. Migrate views to Razor Pages syntax. |
| Microsoft.AspNet.Razor | 3.2.7 | ❌ N/A | Built-in (ASP.NET Core) | **Remove** | 🔴 | Razor engine is built into ASP.NET Core. No NuGet package needed. |
| Microsoft.AspNet.WebApi.Core | 5.2.7 | ❌ N/A | ASP.NET Core Controllers | **Remove** | 🔴 | Web API 2 removed in ASP.NET Core. Use attribute-routed controllers or Minimal APIs. |
| Microsoft.AspNet.WebApi.Client | 5.2.7 | ❌ N/A | HttpClient (built-in) | **Remove** | 🔴 | HttpClient is in System.Net.Http (built-in). Web API 2 client library not needed. |
| Microsoft.AspNet.WebApi.WebHost | 5.2.7 | ❌ N/A | ASP.NET Core middleware | **Remove** | 🔴 | OWIN/WebHost integration specific to ASP.NET Web API 2. Replaced by ASP.NET Core pipeline. |

### Section B: ASP.NET Identity v2 & OWIN (Must Remove — 🔴 Critical)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| Microsoft.AspNet.Identity.Core | 2.2.3 | ❌ N/A | Microsoft.AspNetCore.Identity | **Remove** | 🔴 | ASP.NET Identity v2 incompatible with .NET Core. Full migration path in M8. |
| Microsoft.AspNet.Identity.EntityFramework | 2.2.3 | ❌ N/A | Microsoft.AspNetCore.Identity.EntityFrameworkCore | **Remove** | 🔴 | EF6-specific. Must use EF Core with ASP.NET Core Identity. |
| Microsoft.AspNet.Identity.Owin | 2.2.3 | ❌ N/A | ASP.NET Core Authentication middleware | **Remove** | 🔴 | OWIN removed entirely in ASP.NET Core. Reimplement with ASP.NET Core auth. |
| Microsoft.Owin | 4.2.2 | ❌ N/A | ASP.NET Core middleware | **Remove** | 🔴 | OWIN is .NET Framework only. ASP.NET Core has built-in middleware pipeline. |
| Microsoft.Owin.Host.SystemWeb | 4.2.2 | ❌ N/A | ASP.NET Core built-in | **Remove** | 🔴 | OWIN host adapters not applicable to ASP.NET Core. |
| Microsoft.Owin.Security | 4.2.2 | ❌ N/A | Microsoft.AspNetCore.Authentication | **Remove** | 🔴 | OWIN security replaced by ASP.NET Core authentication system. |
| Microsoft.Owin.Security.Cookies | 4.2.2 | ❌ N/A | Microsoft.AspNetCore.Authentication.Cookies | **Remove** | 🔴 | Cookie auth rewritten for ASP.NET Core middleware. |
| Microsoft.Owin.Security.OAuth | 3.0.1 | ❌ N/A | Microsoft.AspNetCore.Authentication.OAuth | **Remove** | 🔴 | OAuth middleware rewritten for ASP.NET Core. |
| Owin | 1.0 | ❌ N/A | None needed | **Remove** | 🔴 | OWIN specification package—not used in ASP.NET Core. |

### Section C: ASP.NET Configuration & Infrastructure (Must Remove — 🟢 Low Risk)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| Microsoft.AspNet.SessionState.SessionStateModule | 1.1.0 | ❌ N/A | IDistributedCache | **Remove** | 🟢 | ASP.NET session state module. Use DistributedCache in ASP.NET Core (Redis, SQL Server, etc.). |
| Microsoft.AspNet.TelemetryCorrelation | 1.0.5 | ❌ N/A | Built-in (ASP.NET Core) | **Remove** | 🟢 | Activity correlation built into ASP.NET Core by default. |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | ❌ N/A | Webpack, Vite, ASP.NET Core asset pipeline | **Remove** | 🟢 | Bundle/minification library. Use modern toolchain (npm, webpack) instead. |
| Microsoft.Web.Infrastructure | 1.0.0.0 | ❌ N/A | None needed | **Remove** | 🟢 | .NET Framework runtime infrastructure. Not applicable to .NET Core. |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | 2.0.1 | ❌ N/A | None needed | **Remove** | 🟢 | CodeDom compiler for .NET Framework. .NET 10 SDK includes C# compiler. |
| Microsoft.Net.Compilers | 4.2.0 | ❌ N/A | Built-in (SDK) | **Remove** | 🟢 | Development dependency for explicit compiler. SDK includes compiler; not needed. |

### Section D: Dependency Injection & Autofac (Replace — 🟡 Medium Risk)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| Autofac | 4.9.1 | 9.0.0 | (keep with Extensions package) | **Replace** | 🟡 | Autofac 9.0.0 supports .NET 10 but must use Autofac.Extensions.DependencyInjection for ASP.NET Core integration. |
| Autofac.Mvc5 | 4.0.2 | ❌ N/A | Autofac.Extensions.DependencyInjection | **Remove** | 🔴 | MVC 5 integration removed. Use ASP.NET Core built-in DI or Autofac.Extensions.DependencyInjection. |
| Autofac.WebApi2 | 4.3.1 | ❌ N/A | Autofac.Extensions.DependencyInjection | **Remove** | 🔴 | Web API 2 integration removed. Use Autofac.Extensions.DependencyInjection. |
| **[NEW] Autofac.Extensions.DependencyInjection** | — | **10.0.0** | — | **Add** | 🟡 | Required for ASP.NET Core DI integration. Version 10.0.0 supports .NET 10. Install as replacement. |

### Section E: Data Access & ORM (Update Required — 🟡 Medium Risk)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| EntityFramework | 6.2.0 | **9.0.0** (EF Core) | Microsoft.EntityFrameworkCore 9.0.0 | **Update** | 🟡 | EF 6 not compatible with .NET Core. Must migrate to EF Core 9.0 or latest. Query rewrites may be needed. Migration path detailed in M6. |

### Section F: Logging (Update — 🟢 Low Risk)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| log4net | 2.0.8 | **3.3.0** | log4net 3.3.0 | **Update** | 🟢 | log4net 3.3.0 targets .NET Standard 2.0, fully compatible with .NET 10. No breaking changes. Straightforward upgrade. |

### Section G: Application Insights (Update with Caution — 🟡 Medium Risk)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| Microsoft.ApplicationInsights | 2.9.1 | **3.0.0** | Microsoft.ApplicationInsights 3.0.0 | **Update** | 🟡 | v3.0.0 rebuilt on OpenTelemetry. Has **breaking changes** in extensibility (telemetry processors, initializers). Review migration guide. |
| Microsoft.ApplicationInsights.Web | 2.9.0 | **3.0.0** | Microsoft.ApplicationInsights.AspNetCore 3.0.0 | **Update** | 🟡 | Web-specific package → use AspNetCore package in .NET Core. Breaking changes same as core. |
| Microsoft.ApplicationInsights.DependencyCollector | 2.9.0 | **3.0.0** | Included in AspNetCore 3.0.0 | **Update** | 🟡 | Dependency collection built into AspNetCore 3.0.0. Configuration changes required. |
| Microsoft.ApplicationInsights.PerfCounterCollector | 2.9.0 | **3.0.0** | (limited availability in .NET Core) | **Review** | 🟡 | Performance counters limited in .NET Core on Windows. May not have direct replacement. Consider alternatives. |
| Microsoft.ApplicationInsights.WindowsServer | 2.9.0 | **3.0.0** | Microsoft.ApplicationInsights.AspNetCore 3.0.0 | **Update** | 🟡 | Specific to ASP.NET on Windows. Use AspNetCore package in .NET Core. |
| Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel | 2.9.1 | **3.0.0** | Built into AspNetCore 3.0.0 | **Update** | 🟡 | Channel abstraction included in newer packages. |
| Microsoft.ApplicationInsights.Agent.Intercept | 2.4.0 | ❌ N/A | None | **Remove** | 🔴 | Interceptor for .NET Framework Application Insights only. Not used in .NET Core. |

### Section H: Validation & JavaScript (Keep or Migrate to NPM — 🟢 Low Risk)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| jQuery | 3.3.1 | **3.x (via NPM)** | npm install jquery@latest | **Keep/Migrate** | 🟢 | Client-side library. Can stay in packages.config or migrate to npm. Recommend npm + bundler (webpack). |
| jQuery.Validation | 1.17.0 | **1.x (via NPM)** | npm install jquery-validation@latest | **Keep/Migrate** | 🟢 | Client-side validation. Migrate to npm or keep in wwwroot. |
| Microsoft.jQuery.Unobtrusive.Validation | 3.2.11 | **3.x** | Migrate to npm or ASP.NET Core Tag Helpers | **Keep/Migrate** | 🟢 | ASP.NET MVC-specific. Use ASP.NET Core Tag Helpers for validation instead. |
| Modernizr | 2.8.3 | **2.8.x (via NPM)** | npm install modernizr@latest | **Keep/Migrate** | 🟢 | Feature detection library. Can keep or move to npm. |
| bootstrap | 4.3.1 | **5.x (via NPM)** | npm install bootstrap@latest | **Keep/Migrate** | 🟢 | CSS framework. Keep in packages.config or migrate to npm + bundler. |
| popper.js | 1.14.3 | **2.x (via NPM)** | npm install @popperjs/core@latest | **Keep/Migrate** | 🟢 | Positioning library for tooltips. Migrate to npm. |
| Respond | 1.4.2 | — | (deprecated, use polyfills) | **Keep/Remove** | 🟢 | Polyfill for old IE media queries. Not needed for modern browsers. Safe to remove. |

### Section I: JSON & Serialization (Update — 🟢 Low Risk)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| Newtonsoft.Json | 12.0.1 | **13.0.4** | Newtonsoft.Json 13.0.4 | **Update** | 🟢 | Latest stable. No breaking changes from 12.x → 13.x. Security fix for CVE-2024-21907. |

### Section J: System.* Platform APIs (Keep — 🟢 Low Risk)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| System.Buffers | 4.5.1 | **4.5.1+** | System.Buffers 4.5.1 | **Keep** | 🟢 | Targets .NET Standard 2.0. Already included in .NET 10 but safe to keep. |
| System.Diagnostics.DiagnosticSource | 4.7.1 | **4.7.1+** | System.Diagnostics.DiagnosticSource 4.7.1 | **Keep** | 🟢 | Targets .NET Standard 2.0. Compatible with .NET 10. |
| System.Diagnostics.PerformanceCounter | 4.5.0 | **4.5.0+** | System.Diagnostics.PerformanceCounter 4.5.0 | **Keep** | 🟢 | Works on Windows with .NET 10. Part of Windows Compatibility Pack. |
| System.IO.Compression | 4.3.0 | **Built-in** | Remove (use built-in) | **Keep/Remove** | 🟢 | Built into .NET 10. Can keep reference or remove. |
| System.IO.Compression.ZipFile | 4.3.0 | **Built-in** | Remove (use built-in) | **Keep/Remove** | 🟢 | Built into .NET 10. Can keep reference or remove. |
| System.IO.Pipelines | 4.5.1 | **4.5.1+** | System.IO.Pipelines 4.5.1 | **Keep** | 🟢 | Targets .NET Standard 2.0. Compatible with .NET 10. Used by networking libraries. |
| System.Memory | 4.5.4 | **4.5.4+** | System.Memory 4.5.4 | **Keep** | 🟢 | Targets .NET Standard 2.0. Compatible with .NET 10. |
| System.Numerics.Vectors | 4.5.0 | **4.5.0+** | System.Numerics.Vectors 4.5.0 | **Keep** | 🟢 | Compatible with .NET 10. |
| System.Runtime.CompilerServices.Unsafe | 4.5.3 | **4.5.3+** | System.Runtime.CompilerServices.Unsafe 4.5.3 | **Keep** | 🟢 | Targets .NET Standard 1.1. Widely compatible. Part of most .NET 10 apps. |
| System.Threading.Channels | 4.5.0 | **4.5.0+** | System.Threading.Channels 4.5.0 | **Keep** | 🟢 | Targets .NET Standard 2.0. Compatible with .NET 10. |
| System.Threading.Tasks.Extensions | 4.5.2 | **4.5.2+** | System.Threading.Tasks.Extensions 4.5.2 | **Keep** | 🟢 | Targets .NET Standard 2.0. Compatible with .NET 10. |
| Pipelines.Sockets.Unofficial | 1.0.7 | **1.0.7+** | Pipelines.Sockets.Unofficial 1.0.7 | **Keep** | 🟢 | Targets .NET Standard 2.0. Compatible with .NET 10. Infrequently used. |
| Microsoft.Bcl.AsyncInterfaces | 1.1.0 | **1.1.0+** | Microsoft.Bcl.AsyncInterfaces 1.1.0 | **Keep** | 🟢 | Targets .NET Standard 2.0. Already included in .NET 10 but safe to keep. |

### Section K: Testing Framework (Keep or Update — 🟢 Low Risk)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| MSTest.TestFramework | 2.2.10 | **2.2.10+** | MSTest.TestFramework 2.2.10 | **Keep** | 🟢 | Compatible with .NET 10. Can upgrade to latest MSTest 3.x for newer features. |
| MSTest.TestAdapter | 2.2.10 | **2.2.10+** | MSTest.TestAdapter 2.2.10 | **Keep** | 🟢 | Compatible with .NET 10. Pair with TestFramework. |
| Microsoft.NET.Test.Sdk | 17.3.2 | **17.3.2+** | Microsoft.NET.Test.Sdk 17.3.2 | **Keep** | 🟢 | Test SDK compatible with .NET 10. Current version sufficient. |
| Moq | 4.20.72 | **4.20.72+** | Moq 4.20.72 | **Keep** | 🟢 | Latest stable mocking library for .NET. Compatible with .NET 10. No action needed. |
| Castle.Core | 5.1.1 | **5.2.1** | Castle.Core 5.2.1 | **Update** | 🟢 | Update from 5.1.1 to 5.2.1 for .NET 10 support. Minor update; no breaking changes. |

### Section L: Miscellaneous (Keep or Remove — 🟢 Low Risk)

| Package | Current | .NET 10 Version | Replacement | Action | Risk | Notes |
|---------|---------|-----------------|-------------|--------|------|-------|
| Antlr | 3.5.0.2 | **.NET 10 TBD** | (likely not needed in .NET Core) | **Review** | 🟢 | ANTLR parser generator. If used, check if needed in .NET Core. Likely removable. |
| WebGrease | 1.6.0 | ❌ N/A | Webpack, Vite, npm build tools | **Remove** | 🟢 | ASP.NET MVC bundling tool. Replace with modern JavaScript bundler. |

---

## Migration Action Summary by Category

### 🔴 **CRITICAL — Must Remove (31 packages)**

**Framework-Specific (ASP.NET MVC 5):**
- Microsoft.AspNet.Mvc, WebPages, Razor, Web.Optimization
- Microsoft.AspNet.WebApi.* (Core, Client, WebHost)

**Framework-Specific (OWIN & Identity v2):**
- Microsoft.Owin, Microsoft.Owin.*, Owin
- Microsoft.AspNet.Identity.*

**Framework-Specific (Configuration & Tools):**
- Microsoft.AspNet.SessionState.SessionStateModule
- Microsoft.AspNet.TelemetryCorrelation
- Microsoft.Web.Infrastructure
- Microsoft.CodeDom.Providers.DotNetCompilerPlatform
- Microsoft.Net.Compilers
- Microsoft.ApplicationInsights.Agent.Intercept

**DI Framework-Specific:**
- Autofac.Mvc5, Autofac.WebApi2

**Frontend Build Tools (Legacy):**
- WebGrease, Respond (outdated)

---

### 🟡 **MEDIUM RISK — Requires Migration/Update (8 packages)**

**Breaking Changes:**
- Microsoft.ApplicationInsights → v3.0.0 (OpenTelemetry rebuild)
- Microsoft.ApplicationInsights.* (all modules)
- EntityFramework → EF Core 9.0 (major rewrite)

**DI Changes:**
- Autofac → v9.0.0 (+ add Autofac.Extensions.DependencyInjection 10.0.0)

---

### 🟢 **LOW RISK — Straightforward Update or Keep (23 packages)**

**Direct Updates (No Breaking Changes):**
- Newtonsoft.Json → 13.0.4 (security update)
- log4net → 3.3.0
- Castle.Core → 5.2.1
- System.* packages (keep or remove — all compatible)
- Testing packages (MSTest, Moq, Test SDK)

**Client-Side (Migrate to NPM or Keep):**
- jQuery, jQuery.Validation, bootstrap, popper.js, Modernizr
- Microsoft.jQuery.Unobtrusive.Validation

---

## .NET 10-Compatible Replacement Packages (New Additions)

When converting the web project to ASP.NET Core, you will need these packages (not currently in solution):

| Package | Version | Purpose | When to Add |
|---------|---------|---------|-------------|
| **Autofac.Extensions.DependencyInjection** | **10.0.0** | ASP.NET Core DI integration | Replace Autofac + Autofac.Mvc5 + Autofac.WebApi2 |
| **Microsoft.AspNetCore.Identity** | **10.0.0** | ASP.NET Core Identity system | Replace ASP.NET Identity v2 |
| **Microsoft.AspNetCore.Identity.EntityFrameworkCore** | **10.0.0** | EF Core storage for ASP.NET Core Identity | Replace Microsoft.AspNet.Identity.EntityFramework |
| **Microsoft.EntityFrameworkCore** | **9.0.0+** | Entity Framework Core ORM | Replace EntityFramework 6 |
| **Microsoft.EntityFrameworkCore.SqlServer** | **9.0.0+** | EF Core SQL Server provider | For database access |
| **Microsoft.Extensions.Logging** | **10.0.0** | Built-in logging (alternative to log4net) | Optional; for standardized logging |
| **Microsoft.AspNetCore.Mvc.NewtonsoftJson** | **10.0.0** | Newtonsoft.Json support in ASP.NET Core | If continuing to use Newtonsoft.Json |

---

## Package Inventory Summary

| Category | Count | Examples |
|----------|-------|----------|
| **Current Solution Packages** | 62 | Antlr, Autofac, EntityFramework, log4net, etc. |
| **Must Remove** | 31 | MVC 5, Web API 2, OWIN, Identity v2, etc. |
| **Must Replace/Add** | 3 | Autofac → Autofac.Extensions, + new ASP.NET Core packages |
| **Update to Latest** | 5 | EF → EF Core, AppInsights → v3.0, Newtonsoft.Json → 13.0.4, log4net → 3.3.0, Castle.Core → 5.2.1 |
| **Keep (No Changes)** | 23 | System.* packages, testing frameworks, client-side libraries |

---

## Next Steps (M3-T2 onwards)

1. **M3-T2:** Generate packages.json from this matrix
2. **M4:** Begin SDK-style conversion for library projects using updated package list
3. **M5-M6:** Execute major package migrations (EF Core, ASP.NET Core Identity, DI)
4. **M7-M8:** Complete web project migration with all new packages

---

## References

- [ASP.NET Core Migration Guide](https://learn.microsoft.com/en-us/aspnet/core/migration/proper-to-2x/)
- [Entity Framework Core Overview](https://learn.microsoft.com/en-us/ef/core/)
- [Autofac.Extensions.DependencyInjection](https://github.com/autofac/Autofac.Extensions.DependencyInjection)
- [Application Insights SDK 3.0.0 Breaking Changes](https://techcommunity.microsoft.com/blog/azureobservabilityblog/announcing-application-insights-sdk-3-x-for-net/4493988)
- [Newtonsoft.Json Security Advisory CVE-2024-21907](https://nvd.nist.gov/vuln/detail/CVE-2024-21907)
