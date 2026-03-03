# Decisions

> Canonical record of team decisions. Append-only. Scribe merges from `.squad/decisions/inbox/`.

---

### 2026-03-03T21:38:12Z: Project scope and done criteria
**By:** Ken Schlobohm (Owner)
**What:** Upgrade eShopLegacyMVC from .NET Framework 4.7.2 to .NET 10. Done when code compiles without warnings and app starts successfully.
**Why:** Modernization to current .NET platform.

### 2026-03-03T21:38:12Z: Key migration resources established
**By:** Ken Schlobohm (Owner)
**What:** Use systemweb-adapters for System.Web migration, dnx upgrade-assistant for SDK-style csproj conversion, Experimental.System.Messaging for MSMQ replacement. Follow official porting guide at learn.microsoft.com.
**Why:** Owner-provided guidance for migration approach.

### 2026-03-03T21:38:12Z: No external data sources
**By:** Ken Schlobohm (Owner)
**What:** None of the other folders on disk are sources for data related to this work. Only use this repo and the referenced online resources.
**Why:** Owner directive — avoid pulling in unrelated code or data.

### 2026-03-03T21:49:00Z: Fenster — Comprehensive Upgrade Challenges Research Complete
**By:** Fenster (Research Analyst)  
**Date:** 2026-03-03T21:49:00Z  
**Artifact:** `.squad/research/upgrade-challenges.md`

**Key findings for the team:**

1. **6 blockers identified:** ASP.NET MVC 5, Web API 2, OWIN/Katana, ASP.NET Identity v2, pervasive System.Web usage, Global.asax lifecycle, and BinaryFormatter (removed in .NET 9+).

2. **BinaryFormatter is a hard blocker** (`eShopLegacy.Common/Utilities/Serializing.cs`): Completely removed in .NET 9+. Must replace with JSON or MessagePack serialization before migration.

3. **58 NuGet packages analyzed:** 28 must be removed (Framework-only), 10 need replacements, 10 can be updated in-place.

4. **eShopLegacyMVC web project CANNOT use SDK-style conversion tool** — must be migrated side-by-side as a new ASP.NET Core project. The three library/test projects CAN use the upgrade-assistant tool.

5. **Recommended migration order:** (1) SDK-convert shared libraries, (2) retarget libraries to net10.0, (3) side-by-side web app migration, (4) test project migration, (5) cleanup.

6. **Auth stack is highest-risk area:** Identity v2 + OWIN + MVC5 cookie auth are tightly coupled and must be migrated together.

7. **Design smell flagged:** `ApplicationUser.ZipCode` property makes synchronous `HttpWebRequest` in a getter — should be refactored to async service regardless of migration.

**Impact on McManus (Code):** Full detailed challenge list with file paths and migration paths per challenge.
**Impact on Keaton (Architect):** Migration order and phase plan ready for architecture decisions.
**Impact on Hockney (Test):** Test project needs significant mock rewrites for System.Web → ASP.NET Core types.

### 2026-03-03T21:49:00Z: Keaton — Migration Plan for .NET Framework 4.7.2 → .NET 10
**By:** Keaton (Lead / Migration Architect)  
**Date:** 2026-03-03T21:49:00Z  
**Status:** Proposed — awaiting owner review

**Decision:** Adopt a 12-milestone incremental migration plan (M0–M11) stored in `tasks.json` with JSON schema validation via `tasks-schema.json`. Each milestone is independently verifiable and leaves the solution in a buildable state.

**Migration Order:**
1. **M0 — Baseline**: Verify current build
2. **M1 — SDK-style conversion**: Convert Common, Utilities, Test to SDK-style csproj (NOT the web project)
3. **M2 — MSMQ replacement**: System.Messaging → Experimental.System.Messaging
4. **M3 — NuGet updates**: Update packages to .NET 10-compatible versions
5. **M4 — Target framework**: Retarget libraries to net10.0 (handle BinaryFormatter, System.Web removal)
6. **M5 — Web project**: Side-by-side ASP.NET Core migration (critical path — 9 tasks)
7. **M6 — EF Core**: Entity Framework 6 → EF Core
8. **M7 — DI**: Autofac → built-in DI
9. **M8 — Identity**: ASP.NET Identity v2 → Core Identity
10. **M9 — Configuration**: web.config → appsettings.json, cleanup
11. **M10 — Tests**: Retarget and fix test project
12. **M11 — Final verification**: Zero warnings, app starts

**Key Architecture Decisions:**
- **No big-bang rewrite**: Every milestone boundary is buildable
- **Libraries first, web last**: Topological order minimizes risk
- **BinaryFormatter must be replaced** (Serializing.cs) — it's disabled in .NET 5+
- **Autofac replaced with built-in DI** — simpler than upgrading Autofac to Core
- **log4net replaceable** with Microsoft.Extensions.Logging (team's choice)
- **systemweb-adapters available** for transitional System.Web compatibility but not recommended as permanent solution

**Artifacts:**
- `tasks-schema.json` — JSON schema for migration plan
- `tasks.json` — Complete milestone/task plan with assignments
- `docs/migration/` — 18 challenge documents with step-by-step guidance

**Team Assignments:**
- **McManus**: Code migration execution
- **Fenster**: Package research and compatibility analysis
- **Hockney**: Testing and verification at each milestone
- **Keaton**: Architecture review, plan maintenance, sign-off

### 2026-03-03: McManus — Manual SDK-style conversion for eShopLegacy.Common
**By:** McManus (Developer)  
**Task:** M1-T1  
**Status:** Completed

**Decision:** Performed manual csproj conversion instead of using the upgrade-assistant tool.

**Why:** The `upgrade-assistant` tool (v1.0.749-preview1) crashes with `System.TypeInitializationException` in `Microsoft.Build.Shared.XMakeElements` when invoked via both `dnx` and direct command. This appears to be an MSBuild version incompatibility (MSBuild 17.14.40 installed).

**What was done:**
1. Replaced old-style csproj with SDK-style (`Microsoft.NET.Sdk`)
2. TargetFramework kept as `net461` (unchanged)
3. EntityFramework 6.0.0 converted from `packages.config` HintPath reference to `<PackageReference>`
4. `System.ComponentModel.DataAnnotations` kept as explicit `<Reference>` (not implicit in SDK-style)
5. `GenerateAssemblyInfo=false` set to avoid conflicts with existing `Properties\AssemblyInfo.cs`
6. `packages.config` removed
7. `bin`/`obj` cleaned

**Impact:** Same approach should be used for M1-T2 (Utilities) and M1-T3 (Test) since the tool is broken on this environment.

**Validation:** Full solution (all 4 projects) builds successfully after conversion.
