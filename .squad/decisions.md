# Decisions

> Canonical record of team decisions. Append-only. Scribe merges from `.squad/decisions/inbox/`.

---

### 2026-03-03T21:38:12Z: Project scope and done criteria
**By:** Ken Schlobohm (Owner)
**What:** Upgrade eShopLegacyMVC from .NET Framework 4.7.2 to .NET 10. Done when code compiles without warnings and app starts successfully.
**Why:** Modernization to current .NET platform.

### 2026-03-03T21:38:12Z: Key migration resources established
**By:** Ken Schlobohm (Owner)
**What:** Use systemweb-adapters for System.Web migration, dnx upgrade-assistant for SDK-style csproj conversion, Experimental.System.Messaging for MSMQ replacement. Follow official porting guide at learn.microsoft.com.
**Why:** Owner-provided guidance for migration approach.

### 2026-03-03T21:38:12Z: No external data sources
**By:** Ken Schlobohm (Owner)
**What:** None of the other folders on disk are sources for data related to this work. Only use this repo and the referenced online resources.
**Why:** Owner directive — avoid pulling in unrelated code or data.
