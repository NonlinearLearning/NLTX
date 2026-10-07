# T2-B2 compile failure review (PUA)

## Failure observed

First `dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --nologo` returned exit `1`, with 6 warnings in the referenced `Terraria.WorldStorage` project and these 3 errors in `LoadedWorldSession.cs`:

```text
CS1501: “Create”方法没有采用 0 个参数的重载
CS1061: “WorldSessionRestoreState”未包含“Destroy”的定义
CS1061: “WorldSessionRestoreState”未包含“Destroy”的定义
```

The errors came from `LoadedWorldSession.World` shadowing the imported `Arch.Core.World` type in the same class. The T2 probe's `World.Create()` call established that the no-argument API exists; the production call resolved against the property/type scope instead. This was a C# name-resolution issue, not a missing Arch API or package.

## PUA diagnostic steps

- **Exact error and source context:** read the full build output and the constructor/Dispose lines that called `World.Create` and `World.Destroy`.
- **Search:** searched the committed T2 probe for `World.Create` and `World.Destroy`; the existing probe confirmed `World.Create()` and instance `Dispose()` usage. The second requested T1 verification path was absent in this branch, so no result was inferred from it.
- **Prerequisites:** `dotnet --version` returned `10.0.400`; Application assets resolved Arch `2.1.0`, target `net10.0`, `lib/net8.0/Arch.dll`.
- **Reversed assumption:** instead of assuming the API signature was wrong, checked whether the `World` property hid the type. Fully qualifying static calls as `Arch.Core.World.Create()` and `Arch.Core.World.Destroy(...)` confirmed that as the cause.
- **Changed direction:** fixed name resolution directly, then built the T2 probe project with the Application project reference to compile the production owner path. That compile returned exit `0`, 0 warnings, 0 errors. A runtime smoke was run before the later compile-only acceptance instruction and is historical only.

## Current policy

Follow the user's latest compile-only acceptance: do not run tests, `dotnet run`, direct executables, loads, ticks, cleanup, identity rejection, or other runtime verification. Continue with source closure and incremental `dotnet build` only. The requested change-control file was not present in the worktree, `HEAD`, or `main`; the direct user instruction governs the current gate.

## Simulation build prerequisite failure

The first `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo` returned exit `1` with `NETSDK1004`: `Build/obj/NSSLC.Tools.Simulation/project.assets.json` was missing. `Test-Path` returned `False`, confirming the project had not been restored in this worktree. This was not a C# source or Arch API error.

Changed direction: restored that affected project (`dotnet restore ... --nologo`, exit `0`), then rebuilt it incrementally (`dotnet build ... --no-restore --nologo`, exit `0`, 17 warnings / 0 errors). The warnings came from the unchanged `NSSLC.WorldGeneration` dependency. No runtime command was used to resolve this compile prerequisite.
