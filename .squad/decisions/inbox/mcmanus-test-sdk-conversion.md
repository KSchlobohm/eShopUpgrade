### M1-T3: Convert eShopLegacyMVC.Test to SDK-style project format
**By:** McManus (Developer)
**Task:** M1-T3
**Status:** Completed

**Decision:** Performed manual SDK-style csproj conversion for eShopLegacyMVC.Test (same approach as M1-T1 and M1-T2, since upgrade-assistant tool crashes on this environment).

**What was done:**
1. Replaced old-style csproj with SDK-style (`Microsoft.NET.Sdk`)
2. TargetFramework kept as `net472` (unchanged)
3. Converted 10 NuGet packages from `packages.config` to `<PackageReference>`
4. Added `Microsoft.NET.Test.Sdk 17.3.2` — required for `dotnet test` to discover and run tests in SDK-style projects
5. Added `EntityFramework 6.2.0` as `<PackageReference>` — was a direct assembly reference in old csproj but missing from packages.config; test code uses EF types directly
6. Kept explicit `<Reference>` for: Microsoft.CSharp, System.ComponentModel.DataAnnotations, System.Configuration, System.Messaging, System.Web
7. Preserved ProjectReferences to eShopLegacy.Common and eShopLegacyMVC
8. Set `GenerateAssemblyInfo=false` to preserve existing `Properties\AssemblyInfo.cs`
9. Fixed app.config binding redirect for System.Threading.Tasks.Extensions (4.2.0.0 → 4.2.0.1)
10. Removed `packages.config`, cleaned `bin`/`obj`

**Validation:** Full solution (all 4 projects) builds successfully. All 31 tests pass via `dotnet test`.

**Impact:** M1 (SDK-style conversion) is now complete for all eligible projects. M1-T4 (Hockney verification) can proceed.
