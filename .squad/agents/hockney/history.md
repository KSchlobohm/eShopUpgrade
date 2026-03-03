# Hockney — Project History

## Project Context
- **Project:** eShopLegacyMVC — .NET Framework 4.7.2 e-commerce app upgrading to .NET 10
- **Owner:** Ken Schlobohm
- **Stack:** ASP.NET MVC 5, Web API 2, Entity Framework 6, Autofac, MSMQ (System.Messaging), MSTest
- **Target:** .NET 10, ASP.NET Core, EF Core
- **Done Criteria:** Code compiles without warnings and app starts successfully

## Test Infrastructure
- Test framework: MSTest 2.2.10
- Mock framework: Moq 4.20.72
- Test project: eShopLegacyMVC.Test

## Learnings

📌 **Team update (2026-03-03T21:49:00Z):** Fenster completed comprehensive upgrade challenges research (32 blockers, 58 packages analyzed). Keaton created 12-milestone migration plan with 53 tasks. Test project needs significant mock rewrites for System.Web → ASP.NET Core. M10 (test retarget) is planned with detailed guidance. — decided by Fenster & Keaton

📌 **M0 Baseline Verification (2026-03-03):** Build succeeded with 0 warnings, 0 errors in both Debug and Release. All 31 MSTest tests pass (0 failures, 0 skips). Key environment note: `dotnet restore` does NOT work for packages.config projects — must use `nuget.exe restore`. MSBuild path: VS2022 Enterprise. vstest.console path: VS 18 Enterprise. 62 NuGet packages restored. Baseline report written to `docs/migration/m0-baseline-report.md`. M0-T1 and M0-T2 marked completed in tasks.json.

<!-- Append entries below -->

📌 Team update (2026-03-03T22:07): McManus completed M1-T1 — manual SDK-style conversion of eShopLegacy.Common targeting net461. upgrade-assistant incompatible with MSBuild 17.14; manual conversion applied. EntityFramework 6.0.0 migrated to PackageReference. Full solution builds successfully. Same pattern to be used for M1-T2 (Utilities) and M1-T3 (Test). — decided by McManus

📌 **M1-T4 Verification (2026-03-03):** Full solution build + test pass after SDK-style conversion of Common, Utilities, and Test projects. Build: 0 errors, 0 warnings (matches M0 baseline). Tests: 31 passed, 0 failed, 0 skipped (matches M0 baseline). No regressions detected. `msbuild /t:Restore` works for SDK-style projects; `dotnet test --no-build` works for SDK-style test project. M1-T4 marked completed; M1 milestone marked completed in tasks.json.

📌 **M2-T3 Verification (2026-03-04):** MSMQ replacement fully verified. All checks passed:
- **No bare System.Messaging references:** Searched all .cs and .csproj files — zero matches for `using System.Messaging;` or `<Reference Include="System.Messaging" />`. Only `Experimental.System.Messaging` references found (correct).
- **Experimental.System.Messaging v1.1.0 properly referenced:** eShopLegacyMVC.csproj has assembly reference with HintPath + packages.config entry. eShopLegacyMVC.Test.csproj has PackageReference. CatalogController.cs uses `using Experimental.System.Messaging;`.
- **Clean build:** 0 errors, 0 warnings across all 4 projects (Release configuration). Matches M0 baseline.
- **Tests:** 31 passed, 0 failed, 0 skipped (matches M0 baseline). No regressions.
- **tasks.json updated:** M2-T3 → completed, M2-T2 normalized to "completed", M2 milestone → completed. M1 was already completed.

📌 **M3-T4 Verification (2026-03-04):** NuGet package updates fully verified. All checks passed:
- **Package updates verified:** M3-T2 updated EntityFramework 6.0.0→6.2.0 (Common), Newtonsoft.Json 12.0.1→13.0.3, log4net 2.0.8→3.0.4 (web). M3-T3 updated MSTest 2.2.10→3.7.3, Microsoft.NET.Test.Sdk 17.3.2→17.12.0, Castle.Core 5.1.1→5.2.1 (test).
- **Clean build:** 0 errors, 0 warnings across all 4 projects (Release configuration). Matches M0 baseline.
- **Tests:** 31 passed, 0 failed, 0 skipped (matches M0 baseline). No regressions.
- **NuGet restore notes:** 5 pre-existing vulnerability warnings (jQuery 3.3.1, jQuery.Validation 1.17.0, Identity.Owin 2.2.3) — these are not M3-related and will be addressed in downstream milestones.
- **tasks.json updated:** M3-T4 → completed, M3 milestone → completed.
