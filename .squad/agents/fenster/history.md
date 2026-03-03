# Fenster — Project History

## Project Context
- **Project:** eShopLegacyMVC — .NET Framework 4.7.2 e-commerce app upgrading to .NET 10
- **Owner:** Ken Schlobohm
- **Stack:** ASP.NET MVC 5, Web API 2, Entity Framework 6, Autofac, MSMQ (System.Messaging), MSTest
- **Target:** .NET 10, ASP.NET Core, EF Core
- **Done Criteria:** Code compiles without warnings and app starts successfully

## Research Focus Areas
- .NET Framework → .NET 10 migration challenges for this specific codebase
- NuGet package compatibility (Autofac, log4net, Application Insights, etc.)
- ASP.NET MVC 5 → ASP.NET Core migration patterns
- Entity Framework 6 → EF Core migration guide
- systemweb-adapters capabilities and limitations
- MSMQ alternatives and Experimental.System.Messaging viability

## Learnings

<!-- Append entries below -->

### 2025-06-30: Comprehensive Upgrade Challenge Research
- **32 distinct challenges identified** across the solution (6 blockers, 19 needs-work, 7 straightforward)
- **Blockers:** ASP.NET MVC 5 (15+ files), Web API 2 (4 files), OWIN/Katana (3 files), ASP.NET Identity v2 (5 files), System.Web pervasive (20+ files), Global.asax lifecycle, BinaryFormatter (`eShopLegacy.Common/Utilities/Serializing.cs`)
- **BinaryFormatter is removed in .NET 9+** — hard blocker for `Serializing.cs` and `FilesController.cs`
- **58 NuGet packages analyzed:** 28 remove, 10 replace, 10 update, 5 client-side keep, 5 review
- **Dependency graph:** Common → Utilities → MVC → Test (bottom-up migration order)
- **eShopLegacyMVC is NOT eligible for SDK-style conversion tool** — must be side-by-side migration
- **Auth stack (Identity + OWIN + MVC5)** is the most tightly coupled area — all three must migrate together
- **`ApplicationUser.ZipCode`** makes synchronous HttpWebRequest in property getter — design smell
- **`System.Runtime.Remoting.Messaging`** imported in `BrandsController.cs` — not available in .NET Core
- **Session state** stores objects directly — ASP.NET Core requires byte serialization
- **Full report:** `.squad/research/upgrade-challenges.md`
