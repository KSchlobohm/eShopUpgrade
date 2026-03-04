# eShopLegacyMVC Upgrade Report: .NET Framework 4.7.2 → .NET 10

**Date:** 2026-03-03  
**Branch:** `kschlobohm/squad-upgrade`  
**Status:** ✅ Complete — Experimental  
**Owner:** Ken Schlobohm  

---

## Executive Summary

The eShopLegacyMVC e-commerce application was successfully migrated from .NET Framework 4.7.2 (ASP.NET MVC 5) to .NET 10 (ASP.NET Core) in a single session. The migration was executed by an AI team ("Squad") using a 12-milestone incremental plan with 53 tasks. All milestones are complete: the solution compiles with zero warnings, all 31 tests pass, and the application starts and serves HTTP 200 responses.

**By the numbers:**
- 17 commits across the branch
- 266 files changed, +16,088 / −2,049 lines
- 12 milestones, 53 tasks, all completed
- 31/31 tests passing on .NET 10
- 0 compiler warnings, 0 errors

---

## What Went Well

### 1. Incremental milestone approach prevented breakage
Every milestone boundary kept the solution in a buildable state. This meant errors were caught early and in isolation — never more than one milestone's worth of changes to debug at once. The topological ordering (libraries → web → tests) was critical: by the time we hit the complex web migration (M5), the libraries were already multi-targeting and proven.

### 2. Research-first strategy saved significant rework
Fenster's upfront research (32 challenges identified, 62 NuGet packages analyzed) prevented multiple wrong turns. The discovery that **EF6 6.5.1 supports .NET 6+** was a game-changer — it eliminated the need for a full EF Core migration (M6) and reduced risk dramatically. The catalog database layer stayed on EF6, avoiding weeks of potential data access rewriting.

### 3. Multi-targeting as a stepping stone
Using `net461;net10.0` dual targeting for the library projects (Common, Utilities) allowed the migration to proceed incrementally without breaking the existing .NET Framework consumers. This was essential because the web project and test project still depended on net461/net472 output during the middle milestones.

### 4. Conditional compilation handled System.Web gracefully
Rather than introducing the full systemweb-adapters package (which adds complexity), we used `#if NETFRAMEWORK` / `#else` blocks in targeted files:
- `Serializing.cs`: BinaryFormatter (net461) vs System.Text.Json (net10.0)
- `WebHelper.cs`: `HttpContext.Current` (net461) vs configurable accessor (net10.0)

This kept the changes minimal and explicit.

### 5. Test suite migrated cleanly
All 31 tests were adapted from System.Web.Mvc types to Microsoft.AspNetCore.Mvc equivalents (e.g., `HttpStatusCodeResult` → `BadRequestResult`, `RedirectToRouteResult` → `RedirectToActionResult`) and continue to pass on net10.0. The test infrastructure (MSTest + Moq) required no fundamental changes.

---

## What Went Poorly / Could Be Improved

### 1. The upgrade-assistant tool was unusable
The `upgrade-assistant` tool (v1.0.749-preview1) crashed with `System.TypeInitializationException` due to an MSBuild 17.14 version incompatibility. All three SDK-style csproj conversions (M1) had to be done manually. This added time and required careful attention to PackageReference conversion, binding redirects, and GenerateAssemblyInfo settings. **Recommendation:** Pin a known-good MSBuild/upgrade-assistant version pair in CI, or document the manual conversion steps as a fallback.

### 2. The M5-M9 web migration was too coarse
Milestones M5 through M9 (Web Project, EF, DI, Identity, Configuration) were planned as separate milestones but in practice had to be executed as a single atomic change. You cannot have a half-migrated ASP.NET Core project — the csproj, Program.cs, controllers, views, and DI must all change together. The original plan had 9+4+2+4+6 = 25 tasks across M5-M9, but they collapsed into one large commit (`91f49f7`). **Recommendation:** For future migrations, treat the web project migration as one milestone with internal phases, not five separate milestones. The "independently verifiable" property breaks down for tightly coupled web frameworks.

### 3. No intermediate build verification during the big M5-M9 migration
Because M5-M9 was atomic, there was no milestone boundary where Hockney could verify intermediate state. The agent had to build and fix iteratively within a single session. A CI pipeline with incremental checks would have caught issues faster. **Recommendation:** For the web migration phase, use feature flags or conditional compilation to enable incremental verification, or accept that this phase is a "big bang" within the otherwise incremental strategy.

### 4. Static files had to be restructured
ASP.NET Core serves static files from `wwwroot/` by default. The legacy project had `Content/`, `Scripts/`, `Images/`, `Pics/`, and `fonts/` in the project root. These all had to be moved to `wwwroot/`, which is a large diff and affects every view that references static assets. **Recommendation:** Consider configuring ASP.NET Core's static file middleware to serve from legacy paths as a transitional step, then move to `wwwroot/` in a cleanup milestone.

### 5. Log4net was kept but not modernized
The migration kept log4net 3.0.4 with `LogManager.GetLogger()` calls throughout. ASP.NET Core's built-in `Microsoft.Extensions.Logging` with `ILogger<T>` DI integration is the modern standard. Keeping log4net works but forgoes structured logging, log scopes, and the ILogger ecosystem. **Recommendation:** A follow-up task should replace log4net with ILogger, or at least wire log4net as a provider behind the logging abstraction.

---

## Commentary From the Team

### Keaton (Lead / Migration Architect)
> The EF6 6.5.1 discovery was the most impactful finding of the project. The original plan assumed we'd need EF Core (M6), which would have required rewriting the entire data access layer — `CatalogDBContext`, `CatalogDBInitializer`, `CatalogItemHiLoGenerator`, `CatalogService`, and all test mocks. By keeping EF6, we preserved the battle-tested data layer and cut the risk profile in half. I'd recommend the same strategy for any brownfield migration: **check if EF6 6.5.1 meets your needs before committing to EF Core**.

### McManus (.NET Developer)
> The hardest part wasn't any single file — it was the **blast radius of removing System.Web**. HttpContext.Current, Server.MapPath, ConfigurationManager.AppSettings, Request.Files, MimeMapping, HttpFileCollectionBase, Session["key"] as object — these are scattered across controllers, services, views, and tests. Each one needs a different replacement. A checklist of the 20 most common System.Web → ASP.NET Core translations would have saved me significant lookup time.
>
> Also: `ApplicationUser.ZipCode` makes a synchronous HTTP call in a property getter. That's a production bug waiting to happen regardless of framework version. It should be refactored to an async service method.

### Fenster (Research Analyst)
> Two things the owner should know:
> 1. **The `packages/` folder can likely be removed.** It contains NuGet packages for the old packages.config workflow. All projects now use PackageReference, which restores to the global NuGet cache. The `packages/` folder is ~60MB of dead weight. Verify no MSBuild targets reference it, then delete.
> 2. **jQuery 3.3.1 has known XSS vulnerabilities** (CVE-2020-11022, CVE-2020-11023). It wasn't in scope for this migration, but it should be updated to 3.7+ or replaced with a modern alternative. Same for jQuery.Validation 1.17.0.

### Hockney (QA / Test Lead)
> All 31 tests pass, but the test coverage has a gap: **no tests exercise the ASP.NET Core middleware pipeline**. The existing tests create controllers directly (no `WebApplicationFactory`, no integration tests). This means the DI wiring in Program.cs, the authentication middleware, session middleware, and static file serving are untested. For a production migration, I'd add at least:
> - A `WebApplicationFactory<Program>` smoke test that GETs `/` and asserts 200
> - An integration test for the authentication flow (login/logout)
> - A test that verifies static file serving from wwwroot

---

## Sprint Retrospective

*After the migration was complete, each team member reflected on the project — what they learned, what the report missed, and what they'd do differently.*

### Keaton (Lead / Migration Architect)

> The EF6 6.5.1 discovery was almost accidental. Fenster's original research still recommended EF Core 9.0.0; I caught the EF6 .NET 6+ support while retargeting Common in M4-T1. That one finding eliminated the highest-risk milestone and probably saved 40% of the total effort. **Lesson: always verify your assumptions about what's compatible before committing to a rewrite path.**
>
> I own the M5–M9 milestone fiction. Five milestones looked clean on paper, but ASP.NET Core's all-or-nothing startup model made independent verification impossible. I should have planned M5 as one milestone with internal checkpoints instead of five fake milestones.
>
> **Process observation:** The squad model worked well for M0–M4 — Fenster researched, McManus coded, Hockney verified, and I reviewed in parallel. But during M5–M9, the squad collapsed into McManus doing everything serially. The web migration phase is inherently single-threaded. Future migrations should staff accordingly.

### McManus (.NET Developer)

> What would've helped me most: a **System.Web → ASP.NET Core translation cheat sheet**. `HttpContext.Current` → `IHttpContextAccessor`, `Server.MapPath` → `IWebHostEnvironment.ContentRootPath`, `ConfigurationManager.AppSettings` → `IConfiguration`, `Request.Files[0]` → `IFormFile`, `MimeMapping.GetMimeMapping` → `FileExtensionContentTypeProvider` — I looked up each one individually. A pre-built list of the 20 most common patterns would have cut hours.
>
> The static file moves (`Content/`, `Scripts/`, `Pics/`, `fonts/` → `wwwroot/`) touched 47 view files and every asset reference. ASP.NET Core's static file middleware could have been configured to serve from legacy paths during transition, then moved to `wwwroot/` in a cleanup milestone. That would've kept the migration diff much smaller.

### Fenster (Research Analyst)

> I completely missed the static file restructuring burden. ASP.NET Core's `wwwroot/` convention wasn't flagged as a challenge, and it created a massive diff.
>
> The report says "0 warnings" but doesn't mention the `<NoWarn>CA1416</NoWarn>` suppression in the web csproj. That suppresses Windows-specific API warnings (from `Experimental.System.Messaging`). Not necessarily wrong, but it's hidden risk that should be disclosed.
>
> I also traced `ApplicationUser.ZipCode`'s synchronous HTTP call to its source: `WeatherService.cs` still has `.Result` calls on async `HttpClient` methods (lines 24, 27). That's a thread pool starvation risk under load on Kestrel. ZipCode was cleaned up during Identity migration, but the underlying pattern survived in WeatherService.
>
> **Process reflection:** Research-first worked, but I should feed findings *iteratively* during implementation — not just upfront. A "pre-flight check" before each major milestone would've caught the static file issue and the milestone atomicity problem.

### Hockney (QA / Test Lead)

> "31/31 tests pass" is technically true but **deeply misleading.** Those tests instantiate controllers directly — no `WebApplicationFactory`, no middleware pipeline, no authentication flow, no static file serving, no session mechanics.
>
> **Untested controllers:** BrandsController, FilesController, CatalogController2 (API), AccountController, DocumentsController, AspNetSessionController — all have ZERO tests.
>
> **Breaking API change:** The FilesController endpoint (`/api/files`) now returns JSON instead of BinaryFormatter-serialized data. If external consumers expected binary format, they'll break. No test validates this endpoint's contract.
>
> **The migration succeeded from a compiler perspective. Runtime behavior is an open question.**

---

## Artifacts in This Branch

| Path | Description |
|------|-------------|
| `tasks.json` | Migration plan: 12 milestones, 53 tasks with status tracking |
| `tasks-schema.json` | JSON schema for tasks.json validation |
| `docs/migration/` | 20 challenge/milestone documentation files |
| `.squad/` | AI team state: agents, decisions, logs, research, skills |
| `.squad/research/upgrade-challenges.md` | Comprehensive research: 32 challenges, 58 packages |
| `.squad/decisions.md` | Canonical decision ledger for all migration choices |
| `.squad/skills/` | 4 migration skill files (SDK upgrading, MSMQ, porting, systemweb-adapters) |
| `UPGRADE-REPORT.md` | This report |

---

## Commit History

```
8bf8acb M11: All milestones completed - .NET 10 migration verified
1bd2e61 M10: Migrate test project to .NET 10 with ASP.NET Core test APIs
91f49f7 Migrate eShopLegacyMVC from ASP.NET MVC 5 (.NET 4.7.2) to ASP.NET Core (.NET 10)
da6aeb7 M4-T2: Retarget eShopLegacy.Utilities to multi-target net461;net10.0
b067f8d docs(ai-team): Log M3 completion + M4-T1 retarget
c926b48 M4-T1: Retarget eShopLegacy.Common to multi-target net461;net10.0
62ccc76 refactor: Update NuGet packages to latest compatible versions
417711e docs(ai-team): Log M2 MSMQ replacement + M3-T1 package research
b35d5d5 M2-T2: Replace System.Messaging with Experimental.System.Messaging in test project
906ddb1 refactor: Replace System.Messaging with Experimental.System.Messaging
467f9a7 docs(ai-team): Log M1 SDK-style conversion milestone complete
7b27c92 refactor: Convert eShopLegacyMVC.Test to SDK-style project format
08648ca refactor: Convert eShopLegacy.Utilities to SDK-style project format
eb06139 docs(ai-team): Log M0 baseline + M1-T1 Common SDK conversion
cc17019 M1-T1: Convert eShopLegacy.Common to SDK-style project format
121b085 docs(ai-team): Add milestone-driven migration plan with 12 milestones and 53 tasks
84c525b docs(ai-team): Initialize squad team for .NET Framework to .NET 10 migration
```

---

## Recommended Follow-Up Work

1. **Delete the `packages/` folder** — no longer needed with PackageReference
2. **Update jQuery to 3.7+** — CVE-2020-11022, CVE-2020-11023
3. **Add integration tests** using `WebApplicationFactory<Program>`
4. **Replace log4net with ILogger** — adopt Microsoft.Extensions.Logging
5. **Refactor `ApplicationUser.ZipCode`** — synchronous HTTP in a getter is a production risk
6. **Remove the `net461` target from library projects** — now that all consumers are net10.0
7. **Fix WeatherService sync-over-async** — `.Result` calls on `HttpClient` (lines 24, 27) will deadlock under load on Kestrel
8. **Audit `CA1416` NoWarn suppression** — Windows-specific API warnings are suppressed in the web csproj; verify this is intentional
9. **Verify `/api/files` contract change** — BinaryFormatter → JSON is a breaking API change for external consumers
10. **Consider EF Core migration** — EF6 6.5.1 works, but EF Core offers LINQ improvements, compiled queries, and better async support
11. **Add Application Insights / OpenTelemetry** — the legacy App Insights integration was removed during migration
12. **Clean up `.squad/` artifacts** — useful for audit trail, but not needed in production branches
