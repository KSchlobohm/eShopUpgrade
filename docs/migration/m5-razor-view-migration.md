# Razor View Migration — ASP.NET MVC 5 to ASP.NET Core

## Current State
Views are in `eShopLegacyMVC/Views/` with subdirectories:
- `Account/` — Login, Register views
- `AspNetSession/` — Session demo view
- `Catalog/` — Index, Details, Create, Edit, Delete views
- `Documents/` — File listing, upload views
- `Shared/` — _Layout.cshtml, _LoginPartial.cshtml, Error.cshtml
- `UserInfo/` — User info view
- `Web.config` — Razor configuration (namespaces, base page type)
- `_ViewStart.cshtml` — Sets layout

The views use:
- `@Html.ActionLink()`, `@Html.BeginForm()`, `@Html.AntiForgeryToken()`
- `@Scripts.Render("~/bundles/...")` and `@Styles.Render("~/Content/css")`
- `@ViewBag` for passing data
- `@model` directives with types from `eShopLegacyMVC.Models` and `eShopLegacyMVC.ViewModel`
- `@Html.Partial()` for partial views

## Challenge
1. **Bundle references**: `@Scripts.Render()` and `@Styles.Render()` don't exist in ASP.NET Core. Replace with `<script>` and `<link>` tags.
2. **Views/Web.config**: Not used in ASP.NET Core. Replaced by `_ViewImports.cshtml`.
3. **@Html.ActionLink()**: Still works in Core but Tag Helpers (`<a asp-controller="..." asp-action="...">`) are preferred.
4. **@Html.AntiForgeryToken()**: Replaced by `<form asp-antiforgery="true">` or automatic with `<form>` tag helper.
5. **@Html.BeginForm()**: Replaced by `<form asp-controller="..." asp-action="...">` tag helper.
6. **@Html.Partial()**: Replaced by `<partial name="..." />` tag helper.
7. **Namespace imports**: Need `_ViewImports.cshtml` to import tag helpers and namespaces.

## Migration Plan

### Step 1: Create _ViewImports.cshtml
```cshtml
@using eShopLegacyMVC
@using eShopLegacyMVC.Models
@using eShopLegacyMVC.ViewModel
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

### Step 2: Update _ViewStart.cshtml
```cshtml
@{
    Layout = "_Layout";
}
```
(Should work as-is, but verify the layout file name matches.)

### Step 3: Update _Layout.cshtml
- Replace `@Scripts.Render("~/bundles/jquery")` with `<script src="~/js/jquery-3.3.1.js"></script>`
- Replace `@Styles.Render("~/Content/css")` with `<link rel="stylesheet" href="~/css/site.css" />`
- Replace `@Html.Partial("_LoginPartial")` with `<partial name="_LoginPartial" />`
- Replace `@RenderSection("scripts", required: false)` — this actually works the same in Core ✅
- Replace `@RenderBody()` — works the same ✅

### Step 4: Update individual views
Most `@Html.ActionLink` and `@Html.BeginForm` calls can remain as they are (they work in Core). Tag helpers are preferred but not required. Focus on:
- Removing references to `System.Web` types
- Fixing model binding attribute changes (`[Bind]`)
- Updating any `HttpContext.Current` usage

### Step 5: Delete Views/Web.config
Not needed in ASP.NET Core. Functionality replaced by `_ViewImports.cshtml`.

## Actions
- [ ] Create Views/_ViewImports.cshtml with tag helpers and namespace imports
- [ ] Update Views/_ViewStart.cshtml if needed
- [ ] Update Views/Shared/_Layout.cshtml — replace bundle references
- [ ] Move Content/ to wwwroot/css/ and Scripts/ to wwwroot/js/
- [ ] Verify all Catalog views compile (Index, Details, Create, Edit, Delete)
- [ ] Verify Account views compile (Login, Register)
- [ ] Delete Views/Web.config

## Verification
- No `@Scripts.Render` or `@Styles.Render` calls remain
- `_ViewImports.cshtml` exists with `@addTagHelper` directive
- Views/Web.config deleted
- Views render without errors at runtime

## References
- [Migrate Razor views](https://learn.microsoft.com/aspnet/core/migration/proper-to-2x/#migrate-views)
- [Tag Helpers in ASP.NET Core](https://learn.microsoft.com/aspnet/core/mvc/views/tag-helpers/intro)
