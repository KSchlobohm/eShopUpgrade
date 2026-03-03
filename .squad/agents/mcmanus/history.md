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

📌 **M2-T2 completed:** Replaced System.Messaging with Experimental.System.Messaging v1.1.0 in eShopLegacyMVC.Test. Removed `<Reference Include="System.Messaging" />` from the test csproj and added `<PackageReference Include="Experimental.System.Messaging" Version="1.1.0" />`. No test source files directly reference System.Messaging types — the reference was only needed for transitive resolution from the eShopLegacyMVC project reference. No using statement changes required. Full solution (all 4 projects) builds successfully. All 31 tests pass.

**Gotchas:**
- No test source files in eShopLegacyMVC.Test use `System.Messaging` or `Experimental.System.Messaging` directly — the assembly reference existed solely for transitive type resolution from the web project.
- Since the test project is SDK-style, adding Experimental.System.Messaging as a `<PackageReference>` is straightforward (no manual DLL download needed, unlike the legacy web project in M2-T1).
- v1.1.0 (netstandard2.0) is required; v1.2.0 targets net8.0 only and won't work with net472.

📌 **M3-T2 completed:** Updated NuGet packages in library and web projects.
- **eShopLegacy.Common:** EntityFramework 6.0.0 → 6.2.0 (aligned with web project; EF Core migration deferred to M6).
- **eShopLegacy.Utilities:** Verified clean — no NuGet packages, no changes needed.
- **eShopLegacyMVC (legacy csproj):** Newtonsoft.Json 12.0.1 → 13.0.3 (CVE-2024-21907 security fix, no breaking changes). log4net 2.0.8 → 3.0.4 (targets net462/netstandard2.0, compatible with net472). Updated csproj HintPaths, packages.config, Web.config binding redirect.
- Did NOT touch: ASP.NET MVC/WebAPI/OWIN/Identity (M5-M8), EntityFramework in web project (M6), Autofac (M7), Application Insights (breaking changes, deferred).

**Gotchas:**
- Newtonsoft.Json 13.x assembly version is 13.0.0.0 (not 13.0.3.0) — binding redirects must use assembly version, not package version.
- log4net 3.0.4 targets net462 (not net45-full like 2.0.8) — the HintPath lib folder name changed from `net45-full` to `net462`.
- nuget.exe was not pre-installed in this environment — had to download it to handle packages.config restore for the legacy web project.

📌 **M3-T3 completed:** Updated test project packages.
- MSTest.TestFramework 2.2.10 → 3.7.3, MSTest.TestAdapter 2.2.10 → 3.7.3 (MSTest 3.x with net472 support).
- Microsoft.NET.Test.Sdk 17.3.2 → 17.12.0.
- Castle.Core 5.1.1 → 5.2.1 (minor update, no breaking changes).
- Moq 4.20.72 kept as-is (already latest compatible).
- Fixed Newtonsoft.Json binding redirect in app.config (12.0.0.0 → 13.0.0.0) to match web project update.

**Gotchas:**
- MSTest 3.x auto-includes MSTest.Analyzers as a transitive dependency — no issues but adds analyzer warnings to the build.
- The test project's app.config binding redirect for Newtonsoft.Json had to be updated to match the web project's new version, otherwise the build emits warning MSB3836.

**Validation:** Full solution builds with 0 errors, 0 warnings. All 31 tests pass via `dotnet test`.

📌 **M4-T1 completed:** Retargeted eShopLegacy.Common from net461 to multi-target `net461;net10.0`. Used multi-targeting because three projects (Utilities, Test, Web) still reference Common on net461/net472. Removed EntityFramework 6.2.0 PackageReference — no source files in Common use EF6 types (the reference was vestigial). Removed unused `using System.Web;` from CatalogBrand.cs and CatalogType.cs. Replaced BinaryFormatter in Serializing.cs with conditional compilation (`#if NETFRAMEWORK` keeps BinaryFormatter; net10.0 uses System.Text.Json). Made System.ComponentModel.DataAnnotations reference conditional (net461 only; built-in on net10.0). Full solution builds, all 31 tests pass.

**Key discovery — EF 6.5.1 supports .NET 6+:**
- EntityFramework 6.5.1 targets netstandard2.1 and .NET 6+
- This means EF6 CAN run on net10.0 without migrating to EF Core
- The team has an option to use EF 6.5.1 as a stepping stone before full EF Core migration in M6
- Common didn't use EF6 so the reference was simply removed

**Gotchas:**
- `System.Web` using directives in CatalogBrand.cs and CatalogType.cs were entirely unused — no actual System.Web API calls, safe to remove.
- BinaryFormatter is completely removed in .NET 9+ (not just obsolete). Conditional compilation is the cleanest approach for multi-target.
- `System.ComponentModel.DataAnnotations` needs explicit `<Reference>` on net461 SDK-style but is built-in on net10.0 — must be conditional.
- NETSDK1233 warning about .NET 10 in VS2022 17.14 is expected (preview SDK). Does not affect compilation.

📌 **M4-T2 completed:** Retargeted eShopLegacy.Utilities from net461 to multi-target `net461;net10.0`. Used multi-targeting (same pattern as Common in M4-T1) because the web project and test project still reference Utilities on net461/net472. Applied conditional compilation in WebHelper.cs: on NETFRAMEWORK, keeps `System.Web.HttpContext.Current.Request.UserAgent`; on net10.0, provides a static accessor pattern (`SetUserAgentAccessor()`) that returns empty string by default and can be wired up in M5 when the web project migrates to ASP.NET Core. Made `System.Web` reference conditional (net461 only). Full solution builds, all 31 tests pass.

**Gotchas:**
- WebHelper.UserAgent is actively used in `_Layout.cshtml` (`@WebHelper.UserAgent`) — cannot be removed, must be preserved on both targets.
- The net10.0 `SetUserAgentAccessor(Func<string>)` pattern allows M5 to hook this up via `IHttpContextAccessor` in ASP.NET Core startup without changing the static API surface.
- No new NuGet packages needed for the net10.0 target — the stub pattern avoids adding `Microsoft.AspNetCore.Http` as a dependency to this library project.

📌 **M4-T3 already completed in M4-T1:** BinaryFormatter replacement in Serializing.cs was done during the Common retarget (M4-T1). Conditional compilation (`#if NETFRAMEWORK` keeps BinaryFormatter; net10.0 uses System.Text.Json) was applied. Marked M4-T3 as completed in tasks.json since the work is already done.
