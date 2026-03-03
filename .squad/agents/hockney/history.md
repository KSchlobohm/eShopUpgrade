# Hockney — Project History

## Project Context
- **Project:** eShopLegacyMVC — .NET Framework 4.7.2 e-commerce app upgrading to .NET 10
- **Owner:** Ken Schlobohm
- **Stack:** ASP.NET MVC 5, Web API 2, Entity Framework 6, Autofac, MSMQ (System.Messaging), MSTest
- **Target:** .NET 10, ASP.NET Core, EF Core
- **Done Criteria:** Code compiles without warnings and app starts successfully

## Test Infrastructure
- Test framework: MSTest 2.2.10
- Mock framework: Moq 4.20.72
- Test project: eShopLegacyMVC.Test

## Learnings

📌 **Team update (2026-03-03T21:49:00Z):** Fenster completed comprehensive upgrade challenges research (32 blockers, 58 packages analyzed). Keaton created 12-milestone migration plan with 53 tasks. Test project needs significant mock rewrites for System.Web → ASP.NET Core. M10 (test retarget) is planned with detailed guidance. — decided by Fenster & Keaton

<!-- Append entries below -->
