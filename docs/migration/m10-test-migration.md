# Test Project Migration to .NET 10

## Current State
`eShopLegacyMVC.Test` targets .NET Framework 4.7.2 with:

### Test Files
| File | What It Tests | Framework Dependencies |
|------|--------------|----------------------|
| `Controllers/CatalogControllerTest.cs` | CatalogController CRUD | System.Web.Mvc (ActionResult, ViewResult, HttpStatusCodeResult, RedirectToRouteResult) |
| `Controllers/PicControllerTest.cs` | PicController image serving | System.Web (HttpContextBase, HttpServerUtilityBase, HttpRequestBase, ControllerContext) |
| `Services/FileServiceTest.cs` | FileService upload/download | System.Web (HttpFileCollectionBase, HttpPostedFileBase) |
| `Models/CatalogItemTest.cs` | CatalogItem validation | System.ComponentModel.DataAnnotations (works on Core) ✅ |
| `Models/CatalogDBContextTest.cs` | DbContext configuration | System.Data.Entity (EF6) |
| `DbSetExtensions.cs` | Test helpers | System.Data.Entity |

### NuGet Packages
- MSTest.TestFramework 2.2.10 → Update to 3.x
- MSTest.TestAdapter 2.2.10 → Update to 3.x
- Moq 4.20.72 → Compatible with .NET 10 ✅
- Castle.Core 5.1.1 → Compatible with .NET 10 ✅

## Challenge
1. **System.Web.Mvc types gone**: All test assertions against MVC 5 types (ViewResult, HttpStatusCodeResult, etc.) must use ASP.NET Core types.
2. **HttpContext mocking changed**: System.Web `HttpContextBase`, `HttpServerUtilityBase` → ASP.NET Core `DefaultHttpContext` or mocked `HttpContext`.
3. **HttpFileCollectionBase gone**: FileService tests use `Mock<HttpFileCollectionBase>` → use `Mock<IFormFileCollection>` and `Mock<IFormFile>`.
4. **EF6 test patterns**: `InMemoryCatalogDBContext` with `Database.SetInitializer<T>(null)` → EF Core InMemory provider.
5. **MSTest version**: 2.x → 3.x, mostly API-compatible but some attribute changes possible.

## Migration Plan

### CatalogControllerTest.cs
```csharp
// Before:
var result = _controller.Details(null) as HttpStatusCodeResult;
Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);

// After:
var result = _controller.Details(null) as BadRequestResult;
Assert.IsNotNull(result);
// Or check: Assert.IsInstanceOfType(result, typeof(BadRequestResult));
```

Key type changes:
- `HttpStatusCodeResult` → `StatusCodeResult` or specific types (`BadRequestResult`, `NotFoundResult`)
- `HttpNotFoundResult` → `NotFoundResult`
- `ViewResult` → `ViewResult` (same name, different namespace)
- `RedirectToRouteResult` → `RedirectToActionResult`

### PicControllerTest.cs
```csharp
// Before:
var _mockHttpContext = new Mock<HttpContextBase>();
var _mockServer = new Mock<HttpServerUtilityBase>();
_mockServer.Setup(s => s.MapPath(It.IsAny<string>())).Returns(webRoot);

// After: Inject IWebHostEnvironment instead
var mockEnv = new Mock<IWebHostEnvironment>();
mockEnv.Setup(e => e.WebRootPath).Returns(webRoot);
var controller = new PicController(_mockCatalogService.Object, mockEnv.Object);
```

### FileServiceTest.cs
```csharp
// Before:
var mockHttpFileCollection = new Mock<HttpFileCollectionBase>();
var mockHttpPostedFile = new Mock<HttpPostedFileBase>();

// After:
var mockFormFileCollection = new Mock<IFormFileCollection>();
var mockFormFile = new Mock<IFormFile>();
mockFormFile.Setup(f => f.FileName).Returns("test.txt");
mockFormFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[] { 0x01 }));
```

### CatalogDBContextTest.cs
```csharp
// Before:
Database.SetInitializer<InMemoryCatalogDBContext>(null);

// After: Use EF Core InMemory
var options = new DbContextOptionsBuilder<CatalogDBContext>()
    .UseInMemoryDatabase(databaseName: "TestDb")
    .Options;
using var context = new CatalogDBContext(options);
```

## Actions
- [ ] Retarget test csproj to net10.0
- [ ] Update MSTest to 3.x
- [ ] Fix CatalogControllerTest: replace MVC 5 types with Core types
- [ ] Fix PicControllerTest: replace System.Web mocks with Core equivalents
- [ ] Fix FileServiceTest: replace HttpFileCollectionBase with IFormFileCollection
- [ ] Fix CatalogDBContextTest: use EF Core InMemory provider
- [ ] Update DbSetExtensions for EF Core
- [ ] Run all tests

## Verification
- `dotnet test eShopLegacyMVC.Test` reports all tests passed
- No `System.Web` references in test project
- No `System.Data.Entity` references in test project
- All 20+ tests pass

## References
- [Testing in ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/)
- [MSTest v3 migration](https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-runner-intro)
- [EF Core InMemory provider](https://learn.microsoft.com/ef/core/testing/choosing-a-testing-strategy)
