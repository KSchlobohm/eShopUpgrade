### M3-T2 & M3-T3: NuGet Package Updates to Latest Compatible Versions
**By:** McManus (Developer)
**Tasks:** M3-T2, M3-T3
**Status:** Completed
**Date:** 2026-03-03

**What was done:**

**Library Projects (M3-T2):**
- eShopLegacy.Common: EntityFramework 6.0.0 → 6.2.0 (aligned with web project)
- eShopLegacy.Utilities: Verified clean, no packages to update
- eShopLegacyMVC (legacy csproj, packages.config):
  - Newtonsoft.Json 12.0.1 → 13.0.3 (security fix for CVE-2024-21907)
  - log4net 2.0.8 → 3.0.4 (latest stable, targets net462/netstandard2.0)
  - Updated csproj HintPaths, packages.config, Web.config binding redirect

**Test Project (M3-T3):**
- MSTest.TestFramework 2.2.10 → 3.7.3
- MSTest.TestAdapter 2.2.10 → 3.7.3
- Microsoft.NET.Test.Sdk 17.3.2 → 17.12.0
- Castle.Core 5.1.1 → 5.2.1
- Moq 4.20.72 kept (already compatible)
- Fixed Newtonsoft.Json binding redirect in app.config

**NOT touched (per plan):**
- ASP.NET MVC/WebAPI/OWIN/Identity packages (M5-M8)
- EntityFramework in web project (M6)
- Autofac packages (M7)
- Application Insights packages (breaking changes, deferred)
- jQuery/bootstrap/client-side packages (deferred)

**Why:**
- Newtonsoft.Json 13.0.3 patches CVE-2024-21907 — mandatory security update.
- log4net 3.0.4 provides .NET Standard 2.0 support needed for eventual .NET 10 migration.
- MSTest 3.7.3 is the modern test framework with better .NET 10 support.
- Castle.Core 5.2.1 adds .NET 10 compatibility.
- EntityFramework 6.2.0 aligns Common with the web project (EF Core migration is M6).

**Validation:** Full solution builds (0 errors, 0 warnings). All 31 tests pass.

**Impact on Hockney:** M3-T4 verification can proceed — solution is green.
**Impact on Keaton:** M3 milestone nearly complete, only M3-T4 (verification) remains.
