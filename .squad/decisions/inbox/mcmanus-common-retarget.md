### 2026-03-04: McManus — M4-T1: Retarget eShopLegacy.Common from net461 to net10.0
**By:** McManus (Developer)
**Task:** M4-T1
**Status:** Completed

**Decision:** Multi-target `net461;net10.0` instead of net10.0-only. Remove unused EntityFramework reference. Replace BinaryFormatter with System.Text.Json via conditional compilation.

**Why multi-target:**
Three projects reference eShopLegacy.Common (Utilities net461, Test net472, Web net472). Going net10.0-only would break the entire solution. Multi-targeting keeps the full solution building while adding net10.0 support. The net461 target will be dropped after M5 (web project migration).

**Key finding — EF 6.5.1 supports .NET 6+:**
EntityFramework 6.5.1 targets netstandard2.1 and .NET 6+. This means the team does NOT need to migrate to EF Core just for .NET 10 compatibility. EF6 6.5.1 can run on net10.0. However, since Common's source code does NOT use any EF6 types, the reference was removed entirely from this project. Other projects (web, test) that actually use EF6 can upgrade to 6.5.1 in M6 if the team decides against EF Core migration.

**What was done:**
1. Changed `<TargetFramework>net461</TargetFramework>` → `<TargetFrameworks>net461;net10.0</TargetFrameworks>`
2. Removed `EntityFramework 6.2.0` PackageReference — no source files in Common use EF6 types
3. Made `System.ComponentModel.DataAnnotations` reference conditional (net461 only; built-in on net10.0)
4. Removed unused `using System.Web;` from CatalogBrand.cs and CatalogType.cs
5. Replaced BinaryFormatter in Serializing.cs with conditional compilation: net461 keeps BinaryFormatter, net10.0 uses System.Text.Json
6. Removed App.config EF section (no longer needed)

**Impact on M4-T2 (Utilities retarget):** Utilities references Common. With multi-targeting, Utilities (net461) will pick up Common's net461 assembly. When Utilities is retargeted, the same multi-target pattern should be used.

**Impact on M4-T3 (BinaryFormatter replacement):** Partially addressed. The net10.0 code path now uses System.Text.Json. M4-T3 may still need to address the net461 path and update FilesController.cs to work with the new serialization.

**Impact on M6 (EF migration):** EF 6.5.1 is an option as a stepping stone — supports .NET 10 without rewriting to EF Core. Team should decide whether to use EF 6.5.1 as intermediate step or go directly to EF Core.

**Validation:** Full solution builds (0 errors). All 31 tests pass. Only warning is NETSDK1233 about .NET 10 preview support in VS2022 17.14 (expected).
