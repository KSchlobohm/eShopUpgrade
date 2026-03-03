# Target Framework Update — Library Projects to net10.0

## Current State
After SDK-style conversion (M1), the library projects still target .NET Framework:
- `eShopLegacy.Common` — `<TargetFramework>net461</TargetFramework>`
- `eShopLegacy.Utilities` — `<TargetFramework>net461</TargetFramework>`

### eShopLegacy.Common Source Files
- `Models/CatalogItem.cs` — Uses `System.ComponentModel.DataAnnotations` (available in .NET 10) ✅
- `Models/CatalogBrand.cs` — Has `using System.Web` but **doesn't actually use any System.Web types** ⚠️
- `Models/CatalogType.cs` — Has `using System.Web` but **doesn't actually use any System.Web types** ⚠️
- `Models/SessionDemoModel.cs` — Uses `System.ComponentModel.DataAnnotations` ✅
- `Utilities/Serializing.cs` — Uses `System.Runtime.Serialization.Formatters.Binary.BinaryFormatter` 🔴
- `ViewModel/PaginatedItemsViewModel.cs` — Pure C# class, no framework dependencies ✅

### eShopLegacy.Utilities Source Files
- `WebHelper.cs` — Uses `System.Web.HttpContext.Current.Request.UserAgent` 🔴

## Challenge
1. **System.Web usings in Common models**: `CatalogBrand.cs` and `CatalogType.cs` have `using System.Web` but don't use any types from it. These are safe to remove.
2. **BinaryFormatter in Serializing.cs**: `BinaryFormatter` is obsolete and **throws at runtime** in .NET 5+ unless explicitly opted in via `<EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>`. This is a security risk and should be replaced.
3. **System.Web.HttpContext.Current in WebHelper.cs**: `HttpContext.Current` does not exist in ASP.NET Core. The static accessor pattern must be redesigned.
4. **EntityFramework 6 on .NET 10**: EF 6.5.1 targets netstandard2.1 and works on .NET 10, but the `System.Data.Entity` namespace APIs are specific to EF6. This is a holding pattern until M6 (EF Core migration).

## Migration Plan

### eShopLegacy.Common
1. Change `<TargetFramework>net461</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`
2. Remove `using System.Web;` from `CatalogBrand.cs` and `CatalogType.cs` (unused imports)
3. Update EntityFramework to 6.5.1 (netstandard2.1 support)
4. Handle BinaryFormatter — see `m4-binaryformatter-replacement.md`
5. Build and verify

### eShopLegacy.Utilities
1. Change `<TargetFramework>net461</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`
2. Remove `<Reference Include="System.Web" />` from csproj
3. Redesign `WebHelper.cs` — see `m4-utilities-systemweb-removal.md`
4. Build and verify

## Actions
- [ ] Retarget eShopLegacy.Common to net10.0
- [ ] Remove unused `using System.Web` from CatalogBrand.cs and CatalogType.cs
- [ ] Update EntityFramework to 6.5.1
- [ ] Retarget eShopLegacy.Utilities to net10.0
- [ ] Redesign WebHelper.cs to not use System.Web
- [ ] Replace BinaryFormatter in Serializing.cs
- [ ] Build both projects

## Verification
- Both projects have `<TargetFramework>net10.0</TargetFramework>`
- `dotnet build` succeeds for both projects
- No `System.Web` references in either project (except through systemweb-adapters if used)
- No `BinaryFormatter` usage

## References
- `.squad/skills/dotnet-porting/SKILL.md`
- [Porting .NET Framework to .NET](https://learn.microsoft.com/dotnet/core/porting/)
