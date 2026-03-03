### 2026-03-03: McManus — MSMQ Replacement (M2-T1) in eShopLegacyMVC
**By:** McManus (Developer)
**Task:** M2-T1
**Status:** Completed

**Decision:** Use Experimental.System.Messaging v1.1.0 (not v1.2.0) for the MSMQ replacement in eShopLegacyMVC.

**Why:** Version 1.2.0 targets net8.0 only and is incompatible with net472 projects. Version 1.1.0 targets netstandard2.0, which is compatible with .NET Framework 4.7.2. Since the web project is still a legacy csproj targeting net472, v1.1.0 is the correct choice.

**What was done:**
1. Removed `<Reference Include="System.Messaging" />` from eShopLegacyMVC.csproj
2. Added Experimental.System.Messaging v1.1.0 as assembly reference with HintPath
3. Added package entry to eShopLegacyMVC/packages.config
4. Updated `using System.Messaging;` → `using Experimental.System.Messaging;` in CatalogController.cs
5. Package DLL copied to `packages/Experimental.System.Messaging.1.1.0/` from NuGet cache

**Impact on M2-T2:** The test project (eShopLegacyMVC.Test) is SDK-style (net472) and still has `<Reference Include="System.Messaging" />`. It doesn't appear to use MSMQ types directly in test code, so M2-T2 may just need the reference removed (no package addition needed unless tests reference MSMQ types).

**Impact on M4 (retarget to net10.0):** When the web project is retargeted to net10.0, Experimental.System.Messaging will need to be upgraded from v1.1.0 to v1.2.0 (which targets net8.0, compatible with net10.0).

**Validation:** Full solution (all 4 projects) builds successfully.
