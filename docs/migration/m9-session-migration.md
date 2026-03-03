# Session Migration — System.Web Session to ASP.NET Core Session

## Current State

### AspNetSessionController.cs
```csharp
public ActionResult Index()
{
    var model = HttpContext.Session["DemoItem"];
    return View(model);
}

[HttpPost]
public ActionResult Index(SessionDemoModel demoModel)
{
    HttpContext.Session["DemoItem"] = demoModel;
    return View(demoModel);
}
```

### Global.asax.cs — Session_Start
```csharp
protected void Session_Start(Object sender, EventArgs e)
{
    HttpContext.Current.Session["MachineName"] = Environment.MachineName;
    HttpContext.Current.Session["SessionStartTime"] = DateTime.Now;
}
```

### web.config
```xml
<sessionState mode="InProc" />
```

### SessionDemoModel
```csharp
public class SessionDemoModel
{
    public int? IntSessionItem { get; set; }
    public string StringSessionItem { get; set; }
}
```

## Challenge
1. **ASP.NET Core Session is string/byte-based**: You cannot store arbitrary objects in `Session["key"]`. Only `string`, `int`, and `byte[]` are natively supported.
2. **No Session_Start event**: ASP.NET Core has no equivalent. Session initialization must happen in middleware or on first access.
3. **Serialization required**: To store complex objects like `SessionDemoModel`, you must serialize to JSON/bytes.
4. **Session middleware**: Must be explicitly added via `builder.Services.AddSession()` and `app.UseSession()`.

## Migration Plan

### Step 1: Configure Session in Program.cs
```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// In middleware pipeline:
app.UseSession();
```

### Step 2: Create Session Extension Methods
```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Http;

public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
```

### Step 3: Update AspNetSessionController
```csharp
public IActionResult Index()
{
    var model = HttpContext.Session.Get<SessionDemoModel>("DemoItem");
    return View(model);
}

[HttpPost]
public IActionResult Index(SessionDemoModel demoModel)
{
    HttpContext.Session.Set("DemoItem", demoModel);
    return View(demoModel);
}
```

### Step 4: Replace Session_Start
Create middleware or use `OnSessionStart` in session options. Or initialize on first access:
```csharp
// In a middleware or filter:
if (string.IsNullOrEmpty(context.Session.GetString("MachineName")))
{
    context.Session.SetString("MachineName", Environment.MachineName);
    context.Session.SetString("SessionStartTime", DateTime.Now.ToString("o"));
}
```

## Actions
- [ ] Add session services in Program.cs (AddDistributedMemoryCache, AddSession)
- [ ] Add UseSession() to middleware pipeline
- [ ] Create SessionExtensions helper for typed session access
- [ ] Update AspNetSessionController to use typed session methods
- [ ] Replace Session_Start with middleware or lazy initialization
- [ ] Build and verify

## Verification
- Session reads/writes work in AspNetSessionController
- No `HttpContext.Current.Session` usage
- Session stores and retrieves SessionDemoModel correctly
- `dotnet build` succeeds

## References
- [Session in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/app-state#session)
- `.squad/skills/systemweb-adapters/SKILL.md` — session serialization support
