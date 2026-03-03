# M0 — Baseline Verification Report

**Date:** 2026-03-03  
**Verified by:** Hockney (Tester)  
**Solution:** eShopLegacyMVC.sln  
**Target Framework:** .NET Framework 4.7.2 (libraries target 4.6.1)

---

## Build Results

| Configuration | Result | Warnings | Errors | Time |
|---------------|--------|----------|--------|------|
| Release       | ✅ Success | 0 | 0 | 8.08s |
| Debug         | ✅ Success | 0 | 0 | 5.74s |

**MSBuild version:** 17.14.40+3e7442088 for .NET Framework  
**Build tool:** Visual Studio 2022 Enterprise MSBuild

### Warnings
None. Clean baseline — zero compiler warnings in both configurations.

### Errors
None.

---

## Test Results

| Metric | Value |
|--------|-------|
| Test Runner | vstest.console.exe 18.3.0 (x64) |
| Test Framework | MSTest 2.2.10 |
| Total Tests | 31 |
| Passed | 31 |
| Failed | 0 |
| Skipped | 0 |
| Total Time | 1.89s |

### Test Breakdown by Category

**CatalogItem Model Validation (9 tests)** — All passed
- Constructor_SetsDefaultPictureName
- Name_Required_ValidationFails_WhenEmpty
- Price_Range_ValidationFails_WhenNegative
- Price_Range_ValidationFails_WhenTooLarge
- Price_Format_ValidationFails_WithTooManyDecimals
- AvailableStock_Range_ValidationFails_WhenNegative
- RestockThreshold_Range_ValidationFails_WhenNegative
- MaxStockThreshold_Range_ValidationFails_WhenNegative
- ValidModel_PassesAllValidations

**CatalogDBContext (5 tests)** — All passed
- CatalogDBContext_HasDbSetForCatalogItems
- CatalogDBContext_HasDbSetForCatalogBrands
- CatalogDBContext_HasDbSetForCatalogTypes
- CatalogDBContext_ModelCreation_DoesNotThrow
- CatalogDBContext_OnModelCreating_ConfiguresEntityRelationships

**Configuration (1 test)** — All passed
- Constructor_NullConfiguration_ThrowsArgumentNullException

**FileService (3 tests)** — All passed
- ListFiles_ReturnsAllFilesInBasePath
- DownloadFile_WithExistingFile_ReturnsFileContent
- DownloadFile_WithNonExistingFile_ThrowsFileNotFoundException

**File Upload (1 test)** — All passed
- UploadFile_WithHttpFilesCollection_DoesNotThrow

**CatalogController (7 tests)** — All passed
- Details_WithNullId_ReturnsBadRequest
- Details_WithInvalidId_ReturnsNotFound
- Create_Get_ReturnsViewResult_WithBrandsAndTypes
- Create_Post_WithInvalidModel_ReturnsView_WithBrandsAndTypes
- Edit_Get_WithNullId_ReturnsBadRequest
- Edit_Get_WithInvalidId_ReturnsNotFound
- Edit_Post_WithValidModel_RedirectsToIndex

**Delete Operations (2 tests)** — All passed
- Delete_Get_WithNullId_ReturnsBadRequest
- DeleteConfirmed_WithValidId_RedirectsToIndex

**PicController (3 tests)** — All passed
- Index_WithInvalidId_ReturnsBadRequest
- Index_WithNonExistingItem_ReturnsNotFound
- Index_WithValidItem_ReturnsFileResult

---

## Environment Notes

1. **NuGet restore requires `nuget.exe`** — `dotnet restore` alone does NOT restore packages.config-style packages. The solution uses traditional `packages.config` format, not PackageReference. Must use `nuget.exe restore` before MSBuild.
2. **MSBuild path:** `C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe`
3. **vstest.console path:** `C:\Program Files\Microsoft Visual Studio\18\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe`
4. **62 NuGet packages** restored across 4 projects.

---

## Baseline Summary

The solution is in a clean, healthy state:
- **Zero warnings, zero errors** across both Debug and Release configurations
- **All 31 tests pass** with no failures or skips
- **No flaky tests detected** — all pass deterministically

This establishes our verified baseline before any migration work begins. Any warnings or test failures introduced after this point are regressions from migration changes.
