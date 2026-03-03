# Keaton — Project History

## Project Context
- **Project:** eShopLegacyMVC — .NET Framework 4.7.2 e-commerce app upgrading to .NET 10
- **Owner:** Ken Schlobohm
- **Stack:** ASP.NET MVC 5, Web API 2, Entity Framework 6, Autofac, MSMQ (System.Messaging), MSTest
- **Target:** .NET 10, ASP.NET Core, EF Core
- **Done Criteria:** Code compiles without warnings and app starts successfully
- **Solution:** 4 projects — eShopLegacyMVC (web), eShopLegacy.Common (library), eShopLegacy.Utilities (library), eShopLegacyMVC.Test (tests)

## Key Resources
- systemweb-adapters for incremental System.Web migration
- dnx upgrade-assistant for SDK-style csproj conversion (non-web projects only)
- Experimental.System.Messaging for MSMQ replacement
- Official .NET porting guide at learn.microsoft.com

## Learnings

<!-- Append entries below -->
