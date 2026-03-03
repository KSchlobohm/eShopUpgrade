---
name: "msmq-upgrading"
description: "Upgrading .NET Framework projects with MSMQ (System.Messaging) dependencies to .NET Core."
domain: "dotnet-migration"
confidence: "high"
source: "https://github.com/mjrousos/spec-kit/blob/mjrousos/brownfield-poc/templates/skills/msmq-upgrading/SKILL.md"
---

## Upgrading MSMQ Dependencies

The `System.Messaging` namespace, which provides access to Microsoft Message Queuing (MSMQ), is not supported in .NET Core.

When upgrading applications that rely on MSMQ, replace the System.Messaging dependency with the [Experimental.System.Messaging](https://www.nuget.org/packages/Experimental.System.Messaging) NuGet package. This third-party package re-compiles most of the original System.Messaging code to target .NET 8, enabling its use in .NET Core applications.

The package is a drop-in replacement except that the namespace has changed from `System.Messaging` to `Experimental.System.Messaging`. Therefore, you will need to update all `using` statements and fully qualified type references in your code.

## Steps to Upgrade

1. Remove any existing references to the `System.Messaging` library from the project.
2. Use NuGet tools to find the latest non-preview version of the `Experimental.System.Messaging` package and install it.
3. Update all `using` statements and fully qualified type references from `System.Messaging` to `Experimental.System.Messaging`.
4. Run a restore + build. If the solution does NOT build successfully, STOP and fix issues before proceeding.

## Where MSMQ Is Used in This Repo

- `eShopLegacyMVC/Controllers/CatalogController.cs` — uses `System.Messaging.MessageQueue`
- Configuration: `NewItemQueuePath` app setting in web.config
