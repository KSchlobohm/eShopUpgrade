---
name: "dotnet-sdk-upgrading"
description: "Upgrade .NET project files to use the latest .NET SDK-style project format."
domain: "dotnet-migration"
confidence: "high"
source: "https://github.com/mjrousos/spec-kit/blob/mjrousos/brownfield-poc/templates/skills/dotnet-sdk-upgrading/SKILL.md"
---

## Upgrading to .NET SDK-Style Projects

An important step to modernizing a .NET solution is to upgrade all non-web projects to SDK-style projects.

This guidance outlines the steps to perform this upgrade for a single project. If you have multiple projects to upgrade, determine the order they should be upgraded (according to topological project ordering available in analysis reports) and upgrade them one at a time using this process.

## Hard Constraints

* Do NOT change, add, remove, or upgrade TargetFramework / TargetFrameworks values in this scenario.
* Do NOT introduce new package versions unrelated to conversion.
* Only convert the project format using the upgrade-assistant tool (as described below) and fix build issues directly caused by the conversion.
* This skill only applies to **non-web projects**. ASP.NET WebForms, MVC, and WebAPI projects must be upgraded side-by-side and should _NOT_ be converted to SDK-style projects.

## Steps to Upgrade

1. Run a restore + build. If the solution does NOT build successfully, STOP and fix issues before starting any conversion.

2. Confirm that the project to upgrade is _NOT_ an ASP.NET web project. If it is, STOP and do NOT proceed with this skill. ASP.NET MVC, WebForms, and WebAPI projects should not use SDK-style projects as explained in [Microsoft documentation](https://learn.microsoft.com/nuget/consume-packages/migrate-packages-config-to-package-reference#package-compatibility-issues).

3. Run the following command to convert the project to SDK-style format using the old .NET Upgrade Assistant tool. Note that the Upgrade Assistant tool is deprecated and should not be used for other upgrade tasks, but it is still the recommended way to convert projects to SDK-style format.

   ```
   dnx upgrade-assistant upgrade <path-to-csproj> -o feature.sdkstyle --non-interactive -y
   ```

4. After conversion, delete the `obj` and `bin` folders in the project directory to ensure a clean build.

5. Validate that the conversion was successful:
   * Open the converted project file and confirm it uses SDK-style format.
   * Confirm that the target framework(s) are unchanged.
   * Confirm that all existing `PackageReference` items were preserved.
   * Confirm that any existing `ProjectReference` items were preserved.
   * Ensure that the `packages.config` file was removed.
   * Confirm that the `obj` and `bin` folders were deleted.
   * Rebuild the project and ensure it builds successfully. If it does not build, fix any issues that were introduced by the conversion.

## Applicable Projects in This Repo

- `eShopLegacy.Common` (class library, .NET Framework 4.6.1) — eligible
- `eShopLegacy.Utilities` (class library, .NET Framework 4.6.1) — eligible
- `eShopLegacyMVC.Test` (test project, .NET Framework 4.7.2) — eligible
- `eShopLegacyMVC` (ASP.NET MVC web app) — NOT eligible, must use side-by-side migration
