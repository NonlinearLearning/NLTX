## [ERR-20260819-001] item_world_state_component_missing_system_using

**Logged**: 2026-08-19T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: backend

### Summary
The component source changed during the fix and ended up with two `System` directives.

### Error
```
CS0246: ArgumentOutOfRangeException could not be found.
CS0105: The using directive for System appeared previously in this namespace.
```

### Context
- Adding `FromReplicationSnapshot` used `ArgumentOutOfRangeException`.
- Re-reading before the next command showed that the component file already had a `using System;`
  directive and the added directive was redundant.

### Resolution
- Retain exactly one `using System;` before the namespace.

### Metadata
- Reproducible: yes
- Related Files: src/Terraria.Dome.Simulation/Items/Components/ItemWorldStateComponent.cs

---
## [ERR-20260819-003] scoped_temp_file_policy

**Logged**: 2026-08-19T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: tests

### Summary
The shell policy rejected removal of a single temporary diagnostics file created during the
source-table audit.

### Error
```text
PowerShell command rejected by execution policy.
```

### Context
- Target: `Build/diagnostics/tile-solid-source-snippets.txt`
- The file was created by the current audit and was removed with an exact repository patch instead.

### Resolution
- Use an explicit patch for a known temporary file when the shell deletion command is rejected.

### Metadata
- Reproducible: yes
- Related Files: Build/diagnostics/tile-solid-source-snippets.txt

---
## [ERR-20260819-004] powershell_interpolated_path_label

**Logged**: 2026-08-19T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: tests

### Summary
Two read-only audit helper commands failed because PowerShell treated a colon immediately after an
interpolated variable as part of the variable name.

### Error
```text
ParserError: Variable reference is not valid. ':' was not followed by a valid variable name character.
```

### Context
- The helper formatted `${path}:$lineNumber` as `"$path:$lineNumber"`.
- No repository source or verification result was affected.

### Resolution
- Use braced interpolation such as `"${path}:$lineNumber"` in PowerShell audit output.

### Metadata
- Reproducible: yes
- Related Files: none

---
## [ERR-20260819-005] powershell_literal_wildcard_path

**Logged**: 2026-08-19T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: tests

### Summary
One read-only repository search failed because a PowerShell `-LiteralPath` contained a wildcard.

### Error
```text
rg: Test\\Terraria.Dome.Items*: 文件名、目录名或卷标语法不正确。 (os error 123)
```

### Context
- The command passed `Test\\Terraria.Dome.Items*` as a literal path to `rg`.
- No source, build output, or verification result was affected.

### Resolution
- Search from the `Test` root with `rg` filtering, or enumerate matching directories before passing explicit paths.

### Metadata
- Reproducible: yes
- Related Files: none

---
## [ERR-20260819-006] stale_command_buffer_path

**Logged**: 2026-08-19T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: tests

### Summary
The Extractinator implementation plan assumed a `SimulationCommandBuffer.cs` path that does not
exist in the current worktree.

### Error
```text
Cannot find path 'src\\Terraria.Dome.Simulation\\Simulation\\SimulationCommandBuffer.cs'
because it does not exist.
```

### Context
- The failed command only read the planned source path.
- Existing command ownership must be located from `DomeSimulation` before implementation.

### Resolution
- Use `rg` over `src\\Terraria.Dome.Simulation` to locate the actual command buffer type, then
  correct the implementation plan before editing production code.

### Metadata
- Reproducible: yes
- Related Files: docs/plans/2026-08-19-item-extractinator-implementation.md

---

## [ERR-20260819-007] worldgen_guessed_type_paths

**Logged**: 2026-08-19T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: tests

### Summary
Two read-only WorldGeneration source inspections used guessed paths for existing types.

### Error
```text
Cannot find path 'src\\Terraria.Dome.Simulation\\WorldGeneration\\StructureDefinition.cs'.
Cannot find path 'src\\Terraria.Dome.Simulation\\WorldGeneration\\TileProtectionComponent.cs'.
```

### Context
- The types are currently located under `WorldGeneration\\Definitions` and
  `WorldGeneration\\Components`.
- `WorldTile` is located under `World`, rather than the `WorldModel` namespace directory.
- No source, build output, or verification result was modified by the failed reads.

### Resolution
- Use `rg --files` to locate the project type before reading a new WorldGeneration path.

### Metadata
- Reproducible: yes
- Related Files: src/Terraria.Dome.Simulation/WorldGeneration/Definitions/StructureDefinition.cs

---

## [ERR-20260820-001] parallel_dotnet_verification_lock

**Logged**: 2026-08-20T00:00:00+08:00
**Priority**: medium
**Status**: resolved
**Area**: tests

### Summary
Launching multiple .NET verification projects in parallel caused a compiler output lock.

### Error
```text
CS2012: Cannot open Build\\obj\\Terraria.Dome.Protocol.V1456\\Debug\\net10.0\\Terraria.Dome.Protocol.V1456.dll for writing because it is being used by another process.
```

### Context
- Four repository verification commands were started in one parallel tool call.
- The repository guidance requires serial .NET build/test commands with
  `UseSharedCompilation=false`.
- No source was changed by the failed parallel run.

### Resolution
- Re-run each verification project serially from the repository root.

### Metadata
- Reproducible: yes
- Related Files: src/Terraria.Dome.Protocol.V1456/Terraria.Dome.Protocol.V1456.csproj

---

## [ERR-20260820-002] platform_query_missing_linq_using

**Logged**: 2026-08-20T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: tests

### Summary
The new platform support query used `IReadOnlyCollection.Contains` without importing LINQ.

### Error
```text
CS1061: IReadOnlyCollection<ushort> does not contain a definition for Contains.
```

### Context
- The query was intentionally kept dependency-light and did not have a LINQ using.
- The failure occurred during the first build of `PlatformSupportQuery.cs`.

### Resolution
- Replaced the extension call with an explicit collection loop.

### Metadata
- Reproducible: yes
- Related Files: src/Terraria.Dome.Simulation/WorldGeneration/PlatformSupportQuery.cs

---

## [ERR-20260820-003] powershell_pipeline_after_foreach

**Logged**: 2026-08-20T00:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: tests

### Summary
Candidate enumeration placed a PowerShell pipeline directly after a foreach statement.

### Error
```text
ParserError: An empty pipe element is not allowed.
```

### Context
- The read-only WorldGen candidate scan attempted to pipe a foreach statement to `Format-Table`.
- No repository file was modified before the failure.

### Resolution
- Collect foreach output in a `$result` variable, then pipe that variable to `Format-Table`.

### Metadata
- Reproducible: yes
- Related Files: Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs

---

---

---
