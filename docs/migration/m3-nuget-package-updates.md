# NuGet Package Updates for .NET 10 Compatibility

## Current State
The solution uses the following NuGet packages (across all projects):

### eShopLegacy.Common
| Package | Version | .NET 10 Compatible? | Action |
|---------|---------|---------------------|--------|
| EntityFramework | 6.0.0 | Yes (6.5.1 has netstandard2.1) | Update to 6.5.1 |

### eShopLegacyMVC (packages.config — 59 packages)
| Package | Version | .NET 10 Compatible? | Action |
|---------|---------|---------------------|--------|
| Autofac | 4.9.1 | No | Replace in M7 (upgrade to 8.x or use built-in DI) |
| Autofac.Mvc5 | 4.0.2 | No | Remove in M7 |
| Autofac.WebApi2 | 4.3.1 | No | Remove in M7 |
| EntityFramework | 6.2.0 | Yes (update to 6.5.1) | Update, then replace with EF Core in M6 |
| log4net | 2.0.8 | Yes (has netstandard2.0) | Update to 3.x or keep |
| Newtonsoft.Json | 12.0.1 | Yes | Update to 13.0.3 |
| Microsoft.ApplicationInsights.* | 2.9.x | No (Web packages need System.Web) | Replace with AspNetCore package |
| Microsoft.AspNet.Identity.* | 2.2.3 | No | Replace in M8 |
| Microsoft.AspNet.Mvc | 5.2.7 | No | Replace in M5 |
| Microsoft.AspNet.WebApi.* | 5.2.7 | No | Replace in M5 |
| Microsoft.AspNet.WebPages | 3.2.7 | No | Remove (not needed in Core) |
| Microsoft.AspNet.Razor | 3.2.7 | No | Remove (Core has built-in Razor) |
| Microsoft.Owin.* | 4.2.2 | No | Remove in M8 |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | No | Replace with static files or WebOptimizer |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | 2.0.1 | No | Remove (not needed in SDK-style) |
| Microsoft.Net.Compilers | 4.2.0 | No | Remove (not needed in SDK-style) |
| Antlr | 3.5.0.2 | No | Remove (bundling dependency) |
| WebGrease | 1.6.0 | No | Remove (bundling dependency) |
| bootstrap | 4.3.1 | N/A (client-side) | Keep as static files |
| jQuery | 3.3.1 | N/A (client-side) | Keep as static files |

### eShopLegacyMVC.Test
| Package | Version | .NET 10 Compatible? | Action |
|---------|---------|---------------------|--------|
| MSTest.TestFramework | 2.2.10 | Yes (update to 3.x) | Update |
| MSTest.TestAdapter | 2.2.10 | Yes (update to 3.x) | Update |
| Moq | 4.20.72 | Yes | Keep or update |
| Castle.Core | 5.1.1 | Yes | Keep |
| Microsoft.AspNet.Mvc | 5.2.7 | No | Replace with Core MVC |
| EntityFramework | 6.2.0 | Yes | Replace with EF Core in M6 |

## Challenge
- Many web project packages are deeply tied to System.Web and have no .NET Core equivalent — they must be **removed and replaced**, not updated.
- The web project's packages.config has 59 entries. Many are transitive dependencies that will be resolved automatically in SDK-style projects.
- Client-side packages (jQuery, Bootstrap, Modernizr) need to be managed differently — NuGet is not the right tool for client-side libraries in .NET Core. Use wwwroot static files, LibMan, or npm.
- Application Insights has a completely different integration model on ASP.NET Core.

## Migration Plan
1. Research and document exact target versions for each keepable package
2. Update library project packages (EF6 → 6.5.1 in Common)
3. Update test project packages (MSTest → 3.x)
4. Defer web project package changes to M5-M9 (they are intertwined with the web migration)
5. Client-side packages will be handled as static files in M5

## Actions
- [ ] Create package compatibility matrix with exact target versions
- [ ] Update EntityFramework from 6.0.0 to 6.5.1 in eShopLegacy.Common
- [ ] Update MSTest from 2.2.10 to 3.x in eShopLegacyMVC.Test
- [ ] Verify Moq 4.20.72 works on net10.0
- [ ] Build and test after updates

## Verification
- All PackageReferences resolve to .NET 10-compatible versions
- `dotnet restore` succeeds without warnings about incompatible packages
- Solution builds and tests pass

## References
- `.squad/skills/dotnet-porting/SKILL.md` — Common Challenges table
- [NuGet package compatibility](https://learn.microsoft.com/dotnet/core/porting/third-party-deps)
- [.NET API Portability Analyzer](https://learn.microsoft.com/dotnet/standard/analyzers/portability-analyzer)
