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

<!-- Append entries below -->
