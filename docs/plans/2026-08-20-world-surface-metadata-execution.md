# World Surface Metadata Implementation Plan


**Goal:** Preserve an authoritative V319 WLD world-surface `double` through import, immutable Dome
metadata, and both persistence layers without enabling unsupported Type 226 behavior.

**Architecture:** WLD decoding remains responsible for reading the source value. Compatibility
projects it into Simulation's optional metadata contract; Simulation treats absence as unknown.
The embedded world format and outer Dome state format each use version-gated appended fields so
old snapshots remain readable and never acquire guessed surface values.

**Tech Stack:** .NET 10, C#, executable verifier projects, binary `BinaryReader`/`BinaryWriter`,
V319 WLD compatibility model, deterministic serial Release builds.

---

### Task 1: Add the World Import RED

**Files:**
- Modify: `Test/Terraria.Dome.WorldImport.Verification/Program.cs`
- Read: `src/Terraria.WorldFile.V319/Model/LegacyWorldMetadata.cs`
- Read: `src/Terraria.WorldCompatibility/Model/CompatibilityWorldMetadata.cs`

**Step 1: Write the failing test**

Add a fixture with `LegacyWorldMetadata.WorldSurface = 123.5` and a compatibility snapshot that
projects it through `CompatibilityToDomeProjection`. Assert the resulting
`snapshot.World.Metadata.WorldSurface` is exactly `123.5`.

**Step 2: Run the RED**

```powershell
dotnet run --project Test\Terraria.Dome.WorldImport.Verification\Terraria.Dome.WorldImport.Verification.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected: non-zero compile failure because `WorldMetadata.WorldSurface` is not yet exposed and the
Compatibility projection has no source field.

**Step 3: Record evidence**

Create `Build/diagnostics/main-migration/task-8-world-surface-metadata/<runId>/world-import-red.txt`
and save the complete output. Do not implement production code before this failure is recorded.

### Task 2: Add the Persistence RED

**Files:**
- Modify: `Test/Terraria.Dome.Persistence.Verification/Program.cs`
- Read: `src/Terraria.Dome.Server/Persistence/WorldPersistenceFormat.cs`
- Read: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`

**Step 1: Write the failing tests**

Add a v18 round-trip fixture using `WorldMetadata(..., worldSurface: 123.5)` and assert exact
fractional equality after `DomeStatePersistenceFormat.Write`/`Read`. Add an embedded world-format
round-trip assertion. Add a v17 layout fixture and assert its restored metadata surface is `null`.

**Step 2: Run the RED**

```powershell
dotnet run --project Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected: non-zero compile failure from the new metadata constructor/property or, once the import
RED has supplied that API, a precise round-trip failure because both formats currently omit the
field.

**Step 3: Record evidence**

Save complete output as `persistence-red.txt` in the same fresh run directory.

### Task 3: Implement the Import Contract

**Files:**
- Modify: `src/Terraria.WorldCompatibility/Model/CompatibilityWorldMetadata.cs`
- Modify: `src/Terraria.WorldCompatibility/Projection/CompatibilityToDomeProjection.cs`
- Modify: `src/Terraria.Dome.Simulation/World/WorldMetadata.cs`

**Step 1: Add the optional source-preserving field**

Add `double? WorldSurface` to the Compatibility record and `WorldMetadata` constructor/property.
Pass `metadata.WorldSurface` from the legacy model through `CompatibilityWorldMetadata.From` and
then into the Dome projection. Preserve a present `double` exactly, because the V319 reader has no
source-backed range normalization; preserve a null value as unknown.

**Step 2: Run the focused import GREEN**

Run the WorldImport verifier from Task 1. Expected: exit `0`; the fractional value remains exact.

### Task 4: Implement Embedded World Persistence

**Files:**
- Modify: `src/Terraria.Dome.Server/Persistence/WorldPersistenceFormat.cs`
- Modify: `Test/Terraria.Dome.Persistence.Verification/Program.cs`

**Step 1: Append a version-gated surface field**

Advance the embedded world format version. Write a presence bit followed by `double` when present;
read it only for the new version. Keep legacy v1/v2 readers unchanged in field interpretation and
return `null` for their metadata surface.

**Step 2: Run the focused persistence test**

Run the persistence verifier. Expected: embedded world round-trip passes, and old embedded fixtures
continue to restore `null`.

### Task 5: Implement Outer Dome State Persistence

**Files:**
- Modify: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Modify: `Test/Terraria.Dome.Persistence.Verification/Program.cs`

**Step 1: Advance the outer format safely**

Set `CurrentFormatVersion` to `18`. Append the presence bit and `double` after the complete
existing progression record so a v17 payload is an exact prefix. Read it only when
`formatVersion >= 18`; v1-v17 must preserve their existing field order and restore `null`.

**Step 2: Verify old-layout compatibility**

Run the hand-built v17 fixture and strict trailing-data checks. Expected: v17 reads successfully,
surface is `null`, and unexpected bytes are still rejected.

### Task 6: Run the Batch Evidence Gates

**Files:**
- Create: `Build/diagnostics/main-migration/task-8-world-surface-metadata/<runId>/` artifacts
- Modify: `progress.md` only after all gates pass
- Modify: `docs/plans/2026-08-19-main-server-ecs-migration-execution.md` only to record accepted scope

**Step 1: Run affected verifiers serially**

Run WorldImport, Persistence, Wiring, Wiring/Liquid/Chest loopback, MainBoundary, and the serial
root Release build with `-p:UseSharedCompilation=false -p:MSBuildNodeReuse=false` and
`-p:FixtureHostBuild=false` for the solution build. Save one output file per command.

**Step 2: Check the final source tree**

Run `git diff --check`, verify no source changed after the final gate, and save `worktree.txt`,
`reference-hash.txt`, `diff-check.txt`, and command outputs under a fresh timestamped directory.

**Step 3: Record the exact boundary**

Document accepted: WLD -> Compatibility -> Dome metadata -> embedded/outer persistence preservation.
Document deferred: Type 226 runtime integration, guessed world-surface derivation, and all tile
definition parity not covered by an authoritative source chain.

**Step 4: Commit**

Stage only this batch's source, verifier, plan/design, and progress changes. Do not stage or revert
unrelated existing worktree changes.
