### M4-T2: Retarget eShopLegacy.Utilities to net10.0
**By:** McManus (Developer)
**Task:** M4-T2
**Status:** Completed

**Decision:** Multi-target `net461;net10.0` with conditional compilation for System.Web dependency. On NETFRAMEWORK, keep `HttpContext.Current.Request.UserAgent`. On net10.0, provide static accessor pattern (`SetUserAgentAccessor(Func<string>)`) that returns empty string by default.

**Why:** The web project (eShopLegacyMVC) and test project still consume Utilities on net461/net472. Multi-targeting maintains backward compatibility while enabling net10.0 compilation. The static accessor pattern preserves the `WebHelper.UserAgent` API surface and can be wired up via `IHttpContextAccessor` in M5 without changing callers.

**What was done:**
1. Changed `<TargetFramework>net461</TargetFramework>` to `<TargetFrameworks>net461;net10.0</TargetFrameworks>`
2. Made `<Reference Include="System.Web" />` conditional on net461 only
3. Applied `#if NETFRAMEWORK` / `#else` conditional compilation in WebHelper.cs
4. net10.0 path provides `SetUserAgentAccessor(Func<string>)` for future ASP.NET Core wiring (M5)

**Impact on M5:** When the web project migrates to ASP.NET Core, wire up `WebHelper.SetUserAgentAccessor()` in startup using `IHttpContextAccessor` to restore User-Agent header access.

**Validation:** Full solution builds (all 4 projects). All 31 tests pass.

---

### M4-T3: BinaryFormatter Already Replaced in M4-T1
**By:** McManus (Developer)
**Task:** M4-T3
**Status:** Completed (work done in M4-T1)

**Decision:** M4-T3 (BinaryFormatter replacement) was already completed as part of M4-T1 (Common retarget). Serializing.cs already uses conditional compilation: `#if NETFRAMEWORK` keeps BinaryFormatter, net10.0 uses System.Text.Json. No additional work needed. Marked as completed in tasks.json.
