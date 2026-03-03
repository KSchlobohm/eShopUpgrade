# Team Roster

> Upgrading eShopLegacyMVC from .NET Framework 4.7.2 to .NET 10 — ASP.NET MVC 5, EF6, MSMQ, Autofac, Identity.

## Coordinator

| Name | Role | Notes |
|------|------|-------|
| Squad | Coordinator | Routes work, enforces handoffs and reviewer gates. Does not generate domain artifacts. |

## Members

| Name | Role | Charter | Status |
|------|------|---------|--------|
| Keaton | Lead | `.squad/agents/keaton/charter.md` | ✅ Active |
| McManus | .NET Dev | `.squad/agents/mcmanus/charter.md` | ✅ Active |
| Fenster | Researcher | `.squad/agents/fenster/charter.md` | ✅ Active |
| Hockney | Tester | `.squad/agents/hockney/charter.md` | ✅ Active |
| Scribe | Session Logger | `.squad/agents/scribe/charter.md` | 📋 Silent |
| Ralph | Work Monitor | — | 🔄 Monitor |

## Coding Agent

<!-- copilot-auto-assign: false -->

| Name | Role | Charter | Status |
|------|------|---------|--------|
| @copilot | Coding Agent | — | 🤖 Coding Agent |

### Capabilities

**🟢 Good fit — auto-route when enabled:**
- Bug fixes with clear reproduction steps
- Test coverage (adding missing tests, fixing flaky tests)
- Lint/format fixes and code style cleanup
- Dependency updates and version bumps
- Small isolated features with clear specs
- Boilerplate/scaffolding generation
- Documentation fixes and README updates

**🟡 Needs review — route to @copilot but flag for squad member PR review:**
- Medium features with clear specs and acceptance criteria
- Refactoring with existing test coverage
- API endpoint additions following established patterns
- Migration scripts with well-defined schemas

**🔴 Not suitable — route to squad member instead:**
- Architecture decisions and system design
- Multi-system integration requiring coordination
- Ambiguous requirements needing clarification
- Security-critical changes (auth, encryption, access control)
- Performance-critical paths requiring benchmarking
- Changes requiring cross-team discussion

## Project Context

- **Owner:** Ken Schlobohm
- **Stack:** .NET Framework 4.7.2, ASP.NET MVC 5, Web API 2, Entity Framework 6, Autofac, MSMQ, MSTest
- **Target:** .NET 10, ASP.NET Core, EF Core
- **Description:** Upgrading eShopLegacyMVC e-commerce app from .NET Framework 4.7.2 to modern .NET 10
- **Done Criteria:** Code compiles without warnings and the app starts successfully
- **Created:** 2026-03-03T21:38:12Z

## Key Resources

- [System.Web Adapters](https://github.com/dotnet/systemweb-adapters) — incremental migration from System.Web
- [.NET Porting Guide](https://learn.microsoft.com/en-us/dotnet/core/porting/) — official migration guidance
- [SDK-style csproj conversion](https://github.com/mjrousos/spec-kit/blob/mjrousos/brownfield-poc/templates/skills/dotnet-sdk-upgrading/SKILL.md) — dnx upgrade-assistant tool
- [MSMQ upgrading](https://github.com/mjrousos/spec-kit/blob/mjrousos/brownfield-poc/templates/skills/msmq-upgrading/SKILL.md) — Experimental.System.Messaging replacement
