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

📌 **M1-T2 completed:** Converted eShopLegacy.Utilities from legacy .csproj to SDK-style format targeting net461. Manual conversion (same as M1-T1 — upgrade-assistant unavailable). Project had no NuGet packages (no packages.config), so no PackageReference entries needed. Kept explicit `<Reference Include="System.Web" />` since WebHelper.cs uses `HttpContext.Current`. Preserved ProjectReference to eShopLegacy.Common. Set `GenerateAssemblyInfo=false` to preserve existing AssemblyInfo.cs. Cleaned bin/obj. Full solution (all 4 projects) builds successfully.

**Gotchas:**
- Utilities has no packages.config at all — it's purely framework references + project reference. Simpler conversion than Common.
- `System.Web` must be kept as explicit `<Reference>` — SDK-style net461 projects don't auto-reference it.

<!-- Append entries below -->

📌 Team update (2026-03-03T22:07): Hockney completed M0 baseline verification — Build succeeded with 0 warnings, 0 errors. All 31 MSTest tests pass. Baseline report written to docs/migration/m0-baseline-report.md. Key note: nuget.exe restore required (not dotnet restore) for packages.config projects. Environment: VS2022 Enterprise MSBuild 17.14.40, vstest.console 18.3.0. — decided by Hockney

📌 **M1-T3 completed:** Converted eShopLegacyMVC.Test from legacy .csproj to SDK-style format targeting net472. Manual conversion (same approach as M1-T1 and M1-T2). Converted 10 NuGet packages from packages.config to PackageReference (Castle.Core 5.1.1, EntityFramework 6.2.0, Microsoft.AspNet.Mvc 5.2.7, Microsoft.AspNet.Razor 3.2.7, Microsoft.AspNet.WebPages 3.2.7, Microsoft.Web.Infrastructure 1.0.0.0, Moq 4.20.72, MSTest.TestAdapter 2.2.10, MSTest.TestFramework 2.2.10). Added Microsoft.NET.Test.Sdk 17.3.2 to enable `dotnet test` support. Preserved ProjectReferences to eShopLegacy.Common and eShopLegacyMVC. Kept explicit framework references for Microsoft.CSharp, System.ComponentModel.DataAnnotations, System.Configuration, System.Messaging, System.Web. Set GenerateAssemblyInfo=false. Fixed app.config System.Threading.Tasks.Extensions binding redirect. Removed packages.config, cleaned bin/obj. Full solution builds, all 31 tests pass via `dotnet test`.

**Gotchas:**
- `Microsoft.CSharp` must be kept as explicit `<Reference>` — SDK-style net472 projects don't auto-reference it, and the test code uses the `dynamic` keyword which requires `Microsoft.CSharp.RuntimeBinder`.
- EntityFramework 6.2.0 was in the old csproj as a direct assembly reference but NOT in packages.config — added it as PackageReference since test code directly uses EF types (CatalogDBContextTest.cs, DbSetExtensions.cs).
- The app.config binding redirect for System.Threading.Tasks.Extensions needed updating from version 4.2.0.0 to 4.2.0.1 to match the NuGet package version 4.5.4.
- `Microsoft.NET.Test.Sdk` was added (not in original project) to enable SDK-style test discovery with `dotnet test`.

📌 **M2-T1 completed:** Replaced System.Messaging with Experimental.System.Messaging v1.1.0 in eShopLegacyMVC. Removed `<Reference Include="System.Messaging" />` from eShopLegacyMVC.csproj, added Experimental.System.Messaging assembly reference with HintPath to packages folder, added entry to packages.config. Updated `using System.Messaging;` to `using Experimental.System.Messaging;` in CatalogController.cs. Full solution (all 4 projects) builds successfully.

**Gotchas:**
- Experimental.System.Messaging v1.2.0 targets net8.0 ONLY — it is NOT compatible with net472. Must use v1.1.0 (targets netstandard2.0) for .NET Framework 4.7.2 projects.
- Since eShopLegacyMVC is a legacy (non-SDK-style) csproj with packages.config, the package had to be installed manually: downloaded via temp SDK-style project, copied DLL to packages folder, added HintPath reference in csproj, and added entry to packages.config.
- The test project (eShopLegacyMVC.Test) still has a `<Reference Include="System.Messaging" />` — that's M2-T2, a separate task.
