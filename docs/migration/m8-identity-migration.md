# Identity Migration — ASP.NET Identity v2 to ASP.NET Core Identity

## Current State

### IdentityModels.cs
- `ApplicationUser` extends `Microsoft.AspNet.Identity.EntityFramework.IdentityUser`
- Custom `ZipCode` property that does a synchronous `HttpWebRequest` to an internal service
- `GenerateUserIdentityAsync` creates claims identity
- `ApplicationDbContext` extends `IdentityDbContext<ApplicationUser>` (EF6-based)

### IdentityConfig.cs (App_Start)
- `ApplicationUserManager` extends `UserManager<ApplicationUser>` (ASP.NET Identity v2)
- Factory method `Create()` with `IdentityFactoryOptions` and `IOwinContext`
- Configures UserValidator, PasswordValidator, lockout settings, DataProtectorTokenProvider
- `ApplicationSignInManager` extends `SignInManager<ApplicationUser, string>`

### Startup.Auth.cs (App_Start)
- OWIN startup configures cookie authentication
- `app.CreatePerOwinContext` for DbContext, UserManager, SignInManager
- `app.UseCookieAuthentication` with `CookieAuthenticationOptions`
- `SecurityStampValidator` for session validation

### AccountController.cs
- Uses `HttpContext.GetOwinContext()` to get managers
- `SignInManager.PasswordSignInAsync` with `SignInStatus` enum
- `UserManager.CreateAsync` for registration
- `AuthenticationManager.SignOut` for logout
- `ChallengeResult` inner class for external login

### NuGet Packages
- `Microsoft.AspNet.Identity.Core` 2.2.3
- `Microsoft.AspNet.Identity.EntityFramework` 2.2.3
- `Microsoft.AspNet.Identity.Owin` 2.2.3
- `Microsoft.Owin` 4.2.2
- `Microsoft.Owin.Host.SystemWeb` 4.2.2
- `Microsoft.Owin.Security` 4.2.2
- `Microsoft.Owin.Security.Cookies` 4.2.2
- `Microsoft.Owin.Security.OAuth` 3.0.1
- `Owin` 1.0

## Challenge
1. **Completely different API surface**: ASP.NET Core Identity uses `Microsoft.AspNetCore.Identity` namespace.
2. **No OWIN**: ASP.NET Core has its own middleware pipeline — no `IAppBuilder`, `IOwinContext`, or `CreatePerOwinContext`.
3. **UserManager/SignInManager**: Same concepts but different namespaces and API signatures.
4. **SignInStatus enum → SignInResult class**: `SignInStatus.Success` → `result.Succeeded`.
5. **Cookie auth**: Configured via `builder.Services.ConfigureApplicationCookie()` instead of OWIN middleware.
6. **ApplicationUser.ZipCode**: Uses `HttpWebRequest` (obsolete) — migrate to `HttpClient`.
7. **Database schema**: ASP.NET Core Identity uses a different schema than Identity v2. May need migration scripts.

## Migration Plan

### Step 1: Add ASP.NET Core Identity Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0" />
```

### Step 2: Update IdentityModels.cs
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class ApplicationUser : IdentityUser
{
    // ZipCode property — replace HttpWebRequest with HttpClient
}

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
}
```

### Step 3: Configure Identity in Program.cs
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDBContext")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});
```

### Step 4: Update AccountController
```csharp
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
    {
        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded) return RedirectToLocal(returnUrl);
        if (result.IsLockedOut) return View("Lockout");

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> LogOff()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Catalog");
    }
}
```

### Step 5: Remove OWIN
Delete Startup.Auth.cs, OWIN Startup.cs, and all OWIN packages.

## Actions
- [ ] Add Microsoft.AspNetCore.Identity.EntityFrameworkCore
- [ ] Update ApplicationUser to use Core Identity IdentityUser
- [ ] Update ApplicationDbContext to use Core IdentityDbContext with EF Core
- [ ] Configure Identity services in Program.cs
- [ ] Update AccountController to inject SignInManager/UserManager
- [ ] Replace SignInStatus with SignInResult
- [ ] Replace AuthenticationManager.SignOut with SignOutAsync
- [ ] Replace HttpWebRequest in ZipCode with HttpClient
- [ ] Delete IdentityConfig.cs, Startup.Auth.cs, Startup.cs (OWIN)
- [ ] Remove all OWIN and ASP.NET Identity v2 packages
- [ ] Build and verify

## Verification
- No `Microsoft.AspNet.Identity.*` or `Microsoft.Owin.*` packages
- No `using Microsoft.AspNet.Identity` statements
- No `GetOwinContext()` calls
- Identity configured in Program.cs
- Login/Register/LogOff compile and work

## References
- [Migrate authentication and Identity to ASP.NET Core](https://learn.microsoft.com/aspnet/core/migration/identity)
- [ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
