# SDK-Style Project Conversion

## Current State
The solution has three non-web projects using legacy-style `.csproj` format:
- **eShopLegacy.Common** — Class library, .NET Framework 4.6.1, EntityFramework 6.0.0 via `packages.config`
- **eShopLegacy.Utilities** — Class library, .NET Framework 4.6.1, no NuGet packages, references System.Web and eShopLegacy.Common
- **eShopLegacyMVC.Test** — Test project, .NET Framework 4.7.2, MSTest 2.2.10, Moq 4.20.72, Castle.Core 5.1.1, EF 6.2.0, ASP.NET MVC 5 via `packages.config`

All three use verbose XML `<Project>` format with explicit `<Compile>` items, `<Reference>` items with `<HintPath>`, and `packages.config` for NuGet.

## Challenge
- Legacy csproj format is incompatible with `dotnet` CLI and modern .NET.
- `packages.config` must be converted to `<PackageReference>` items.
- Assembly references with `<HintPath>` pointing into `packages/` folder must become PackageReferences.
- The web project (eShopLegacyMVC) must **NOT** be converted — ASP.NET MVC/WebAPI projects cannot use SDK-style format while targeting .NET Framework.
- Conversion order matters: Common has no project dependencies, Utilities depends on Common, Test depends on Common and MVC.
- `AssemblyInfo.cs` properties may conflict with SDK-style auto-generated attributes.

## Migration Plan

### Topological Order
1. `eShopLegacy.Common` (leaf — no project dependencies)
2. `eShopLegacy.Utilities` (depends on Common)
3. `eShopLegacyMVC.Test` (depends on Common and MVC)

### Steps per Project

1. Verify the solution builds: `msbuild eShopLegacyMVC.sln /t:Rebuild`
2. Run upgrade-assistant:
   ```
   dnx upgrade-assistant upgrade <path-to-csproj> -o feature.sdkstyle --non-interactive -y
   ```
3. Delete `bin/` and `obj/` folders in the project directory
4. Validate the conversion:
   - Confirm `<Project Sdk="Microsoft.NET.Sdk">`
   - Confirm TargetFramework is **unchanged** (net461 or net472)
   - Confirm all PackageReferences preserved
   - Confirm ProjectReferences preserved
   - Confirm `packages.config` removed
5. Rebuild and fix any issues introduced by conversion

### Key Concerns
- **eShopLegacy.Common**: EntityFramework 6.0.0 PackageReference must be preserved
- **eShopLegacy.Utilities**: System.Web assembly reference — this is a Framework assembly, not a NuGet package. It will remain as a `<Reference>` or `<FrameworkReference>`. Address in M4.
- **eShopLegacyMVC.Test**: Has MSTest `.props`/`.targets` imports that the upgrade tool should handle. The duplicate System.Web.Mvc reference entries should be cleaned up. System.Messaging assembly reference needs attention (M2).

## Actions
- [ ] Build solution to verify baseline
- [ ] Convert eShopLegacy.Common with dnx upgrade-assistant
- [ ] Clean bin/obj, verify SDK-style, rebuild
- [ ] Convert eShopLegacy.Utilities with dnx upgrade-assistant
- [ ] Clean bin/obj, verify SDK-style and ProjectReference preserved, rebuild
- [ ] Convert eShopLegacyMVC.Test with dnx upgrade-assistant
- [ ] Clean bin/obj, verify all PackageReferences and ProjectReferences, rebuild
- [ ] Full solution rebuild and test run

## Verification
- All three non-web csproj files start with `<Project Sdk="Microsoft.NET.Sdk">`
- No `packages.config` in any converted project
- `msbuild eShopLegacyMVC.sln /t:Rebuild` succeeds
- All tests pass

## References
- `.squad/skills/dotnet-sdk-upgrading/SKILL.md`
- [Migrate from packages.config to PackageReference](https://learn.microsoft.com/nuget/consume-packages/migrate-packages-config-to-package-reference)
- [SDK-style project format](https://learn.microsoft.com/dotnet/core/project-sdk/overview)
