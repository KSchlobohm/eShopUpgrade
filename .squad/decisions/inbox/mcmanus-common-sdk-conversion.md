### McManus — Manual SDK-style conversion for eShopLegacy.Common
**Date:** 2026-03-03
**Task:** M1-T1

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
