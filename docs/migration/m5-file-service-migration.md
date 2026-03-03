# FileService Migration to ASP.NET Core

## Current State
`eShopLegacyMVC/Services/FileService.cs` provides file operations (list, download, upload) using:
- `ConfigurationManager.AppSettings` for configuration (BasePath, service account credentials)
- `HttpFileCollectionBase` (System.Web) for file uploads
- `WindowsIdentity.Impersonate` for accessing network file shares with service account credentials
- Static `Create()` factory method (not DI-friendly)
- P/Invoke to `advapi32.dll` `LogonUser` for authentication

`DocumentsController.cs` uses:
- `FileService.Create()` — static factory
- `MimeMapping.GetMimeMapping(filename)` — System.Web API for MIME type lookup
- `Request.Files` — System.Web HttpFileCollectionBase

## Challenge
1. **`HttpFileCollectionBase`** does not exist in ASP.NET Core. Replace with `IFormFileCollection`.
2. **`ConfigurationManager.AppSettings`** does not exist in ASP.NET Core. Replace with `IConfiguration` or `IOptions<FileServiceConfiguration>`.
3. **Static `Create()` factory** — anti-pattern for ASP.NET Core. Use DI registration instead.
4. **`MimeMapping.GetMimeMapping()`** — not available. Replace with `FileExtensionContentTypeProvider`.
5. **`Request.Files`** — replace with `Request.Form.Files`.
6. **`WindowsIdentity.Impersonate`** — available on .NET 10 (Windows only). Keep as-is but consider platform guards.
7. **P/Invoke `LogonUser`** — Windows-only. Available on .NET 10 with `[SupportedOSPlatform("windows")]`.

## Migration Plan

### FileService changes
```csharp
using Microsoft.AspNetCore.Http;

public class FileService
{
    private readonly FileServiceConfiguration _configuration;

    public FileService(IOptions<FileServiceConfiguration> options)
    {
        _configuration = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    // Replace HttpFileCollectionBase with IFormFileCollection
    public void UploadFile(IFormFileCollection files)
    {
        // ... impersonation logic stays the same ...
        for (var i = 0; i < files.Count; i++)
        {
            var file = files[i];
            var filename = Path.GetFileName(file.FileName);
            var path = Path.Combine(_configuration.BasePath, filename);
            using (var fs = File.Create(path))
            {
                file.CopyTo(fs); // IFormFile.CopyTo
            }
        }
    }
}
```

### DocumentsController changes
```csharp
public class DocumentsController : Controller
{
    private readonly FileService _fileService;
    private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public DocumentsController(FileService fileService)
    {
        _fileService = fileService;
    }

    public FileResult Download(string filename)
    {
        var file = _fileService.DownloadFile(filename);
        _contentTypeProvider.TryGetContentType(filename, out var contentType);
        return File(file, contentType ?? "application/octet-stream", filename);
    }

    [HttpPost]
    public IActionResult UploadDocument()
    {
        _fileService.UploadFile(Request.Form.Files);
        return RedirectToAction("Index");
    }
}
```

### DI Registration (Program.cs)
```csharp
builder.Services.Configure<FileServiceConfiguration>(
    builder.Configuration.GetSection("Files"));
builder.Services.AddScoped<FileService>();
```

## Actions
- [ ] Remove static Create() factory from FileService
- [ ] Replace constructor to accept IOptions<FileServiceConfiguration>
- [ ] Replace HttpFileCollectionBase with IFormFileCollection
- [ ] Update DocumentsController to inject FileService via DI
- [ ] Replace MimeMapping with FileExtensionContentTypeProvider
- [ ] Replace Request.Files with Request.Form.Files
- [ ] Register FileService and FileServiceConfiguration in DI
- [ ] Build and verify

## Verification
- No `System.Web.HttpFileCollectionBase` references
- No `ConfigurationManager.AppSettings` in FileService
- No `MimeMapping` usage
- FileService registered in DI
- Upload/Download compile and work

## References
- [File uploads in ASP.NET Core](https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads)
- [Options pattern in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options)
