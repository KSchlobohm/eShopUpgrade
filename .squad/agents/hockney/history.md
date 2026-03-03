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
