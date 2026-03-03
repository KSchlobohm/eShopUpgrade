# McManus — Project History

## Project Context
- **Project:** eShopLegacyMVC — .NET Framework 4.7.2 e-commerce app upgrading to .NET 10
- **Owner:** Ken Schlobohm
- **Stack:** ASP.NET MVC 5, Web API 2, Entity Framework 6, Autofac, MSMQ (System.Messaging), MSTest
- **Target:** .NET 10, ASP.NET Core, EF Core
- **Done Criteria:** Code compiles without warnings and app starts successfully

## Key Migration Skills
- `.squad/skills/dotnet-sdk-upgrading/SKILL.md` — dnx upgrade-assistant for non-web projects
- `.squad/skills/msmq-upgrading/SKILL.md` — Experimental.System.Messaging replacement
- `.squad/skills/systemweb-adapters/SKILL.md` — incremental System.Web migration
- `.squad/skills/dotnet-porting/SKILL.md` — general .NET porting guidance

## Learnings

📌 **Team update (2026-03-03T21:49:00Z):** Fenster completed comprehensive upgrade challenges research (32 blockers, 58 packages analyzed). Keaton created 12-milestone migration plan with 53 tasks. BinaryFormatter in Serializing.cs is hard blocker. Web project must migrate side-by-side as new ASP.NET Core project. Migration order: libraries → web → tests. — decided by Fenster & Keaton

📌 **M1-T1 completed:** Converted eShopLegacy.Common from legacy .csproj to SDK-style format targeting net461. The `upgrade-assistant` tool (both via `dnx` and direct) crashed with `System.TypeInitializationException` in `Microsoft.Build.Shared.XMakeElements` — likely an MSBuild version mismatch. Performed manual conversion instead: replaced old-style csproj with `Microsoft.NET.Sdk`, added `PackageReference` for EntityFramework 6.0.0, kept explicit `Reference` for `System.ComponentModel.DataAnnotations`, set `GenerateAssemblyInfo=false` to preserve existing AssemblyInfo.cs. Removed `packages.config`, cleaned `bin`/`obj`. Full solution (all 4 projects) builds successfully. No code changes required.

**Gotchas:**
- The upgrade-assistant tool (v1.0.749-preview1) is incompatible with the installed MSBuild 17.14 — manual conversion was necessary.
- SDK-style net461 projects automatically reference most framework assemblies, but `System.ComponentModel.DataAnnotations` still needs an explicit `<Reference>`.
- `System.Web` using directives in CatalogBrand.cs and CatalogType.cs compile fine because net461 SDK-style projects resolve them through framework assembly references.

<!-- Append entries below -->

📌 Team update (2026-03-03T22:07): Hockney completed M0 baseline verification — Build succeeded with 0 warnings, 0 errors. All 31 MSTest tests pass. Baseline report written to docs/migration/m0-baseline-report.md. Key note: nuget.exe restore required (not dotnet restore) for packages.config projects. Environment: VS2022 Enterprise MSBuild 17.14.40, vstest.console 18.3.0. — decided by Hockney
