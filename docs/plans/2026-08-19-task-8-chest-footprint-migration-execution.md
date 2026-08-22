# Task 8 Chest Footprint Migration Execution

## 1. Purpose and Decision

This work item advances one deferred part of Task 8: project an authoritative Dome chest at its
legacy tile footprint into the existing `CanKillTile` protection contract. It does not claim full
`WorldGen.CanKillTile`, full actuator parity, or complete Terraria world-object parity.

The immediate decision is intentionally narrow:

```text
WorldGrid tile coordinate, type, frame
  -> legacy chest origin query
  -> exact Dome chest lookup
  -> ChestDestructionRuleSystem
  -> isChestBlocked fact
  -> ActuatorCanKillTileRuleSystem
```

No layer may infer chest protection from a nearby chest, a chest entity ID, lock state, open state,
or a client packet. The legacy owner is coordinate-and-frame based; Dome currently indexes chests
by the exact coordinates supplied to `DomeSimulation.CreateChest`.

## 2. Frozen Inputs and Non-Claims

Use the no-deletion V1456 source only as the behavior oracle:

| Evidence | Required use |
| --- | --- |
| `D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs:63226-63334` | `CanKillTile` chest protection and origin formulas |
| `D:\TRbackup\无任何删减通过编译\Terraria\Chest.cs:648-665` | inventory-only `CanDestroyChest` predicate |
| `D:\TRbackup\无任何删减通过编译\Terraria.IO\WorldFile.cs:3357` | Type 88 creation-frame evidence |
| `src/Terraria.Dome.Simulation/WorldObjects/ChestComponent.cs` | authoritative slots |
| `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs` | current chest creation/index ownership |
| `src/Terraria.Dome.Simulation/Wiring/Systems/ActuatorCanKillTileRuleSystem.cs` | existing pure consumer contract |

The expected source formulas are:

```text
Type 21 or 467: originX = x - (frameX / 18 % 2)
                 originY = y - (frameY / 18)

Type 88:         originX = x - (frameX / 18 % 3)
                 originY = y - (frameY / 18)
```

These formulas are source facts, not an invitation to substitute remembered Terraria geometry.
The Type 88 `3 x 2` shape suggested by WorldFile must be modeled only where the recovered source
proves it. Do not replace the source's `frameY / 18` with modulo arithmetic without another
authoritative source reference.

This batch does not:

- enable the active-above actuator branch;
- implement door locks, boulder protection, protected trees, frame relations, or generic
  multi-tile protection;
- alter persistence format, packet framing, chest opening, or `CreateChest` semantics;
- make unsupported tile IDs or malformed frames permissive.

## 3. One Falsifiable Acceptance Proposition

For a supported, active chest tile with source-valid frame values, the projected legacy origin is
the only location queried in Dome's chest index. A chest at that origin blocks `CanKillTile` exactly
when `ChestDestructionRuleSystem.CanDestroy` rejects its slots; an empty or absent origin chest does
not block it. Unsupported, inactive, malformed, or non-authoritatively mapped inputs fail closed.

Every successful and rejected path must assert both the returned decision and the absence of an
unintended world/chest mutation.

## 4. Responsibility and Write Set

| Concern | Owner | Permitted dependency | Forbidden dependency |
| --- | --- | --- | --- |
| tile/frame to legacy origin | new focused Simulation query | `WorldTile` values only | `Main`, `WorldGen`, protocol, mutable grid |
| origin to chest | existing exact chest index or a narrow read-only lookup | chest collection/index | scan by proximity, client owner/open state |
| inventory decision | `ChestDestructionRuleSystem` | `ChestComponent` slots | tile frame, network state |
| protection projection | Wiring adapter/caller | origin query plus lookup result | direct mutation or hard-coded allow |
| final decision | `ActuatorCanKillTileRuleSystem` | explicit boolean facts | reading broad simulation globals |

The adapter is read-only. It must not create chests, normalize frames, repair tile state, allocate an
entity, or commit a tile change. A new type should be responsibility-specific, for example
`LegacyChestOriginQuery`; do not create a generic `Helper`, `Utility`, `Manager`, or `Data` type.

## 5. Execution Order

### Step 1: Recover and freeze source evidence

Before editing production code, record the exact line ranges, source hashes, version evidence, and
the current worktree in a fresh directory:

```powershell
$runId = Get-Date -Format "yyyyMMdd-HHmmss"
$evidence = "Build\diagnostics\main-migration\task-8-chest-footprint\$runId"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null

git status --short | Tee-Object "$evidence\worktree-before.txt"
Get-FileHash "D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs" -Algorithm SHA256 |
  Tee-Object "$evidence\worldgen-hash.txt"
Get-FileHash "D:\TRbackup\无任何删减通过编译\Terraria\Chest.cs" -Algorithm SHA256 |
  Tee-Object "$evidence\chest-hash.txt"
```

Save short source excerpts with their original paths and line numbers in
`$evidence\source-manifest.txt`. If an expected method is absent, stubbed, contradictory, or its
frame meaning cannot be established, stop that mapping as `Blocked by source-coordinate contract`.
Do not manufacture geometry from an external memory or a broad definition table.

### Step 2: Prove the current ownership boundary

Read `ChestMutationCommitSystem`, `ChestIndexSystem`, `ChestCreateCommand`, `DomeSimulation`, and
the WorldFile import path. Establish with existing tests or a small read-only inspection whether
`CreateChest(tileX, tileY)` means legacy top-left origin for imported worlds. If it merely means an
arbitrary test coordinate, implement only a pure origin query and leave runtime projection deferred.

Record one of these outcomes before writing a runtime adapter:

| Outcome | Permitted next action |
| --- | --- |
| exact index is proven to contain legacy origins | add read-only lookup projection |
| index location is not semantically proven | accept pure origin contract only |
| two legacy sources conflict | record conflict; request architecture decision |
| source formula unavailable | remain fail-closed; do not infer |

### Step 3: Create a real RED

Extend `Test/Terraria.Dome.Wiring.Verification/Program.cs` with assertions that fail against the
current source because the query/projection does not exist. Run it before implementation and save
the complete output as `$evidence\wiring-red.txt`.

The RED must cover the following matrix. Compilation failure for a deliberately missing contract is
acceptable only before the contract exists; once it exists, the RED must be behavioral.

| Fixture | Expected result |
| --- | --- |
| Type 21/467 tile at a non-origin horizontal frame | derives the exact formula origin |
| Type 88 tile at a non-origin horizontal frame | derives its exact formula origin |
| populated chest at exact projected origin | `isChestBlocked=true` |
| empty chest at exact projected origin | `isChestBlocked=false` |
| no chest at exact projected origin | `isChestBlocked=false` |
| populated chest adjacent but not at origin | `isChestBlocked=false` |
| unsupported tile type, inactive tile, invalid/negative frame | fail closed; no lookup/mutation |

### Step 4: Make the minimum production change

Add only the smallest pure query and, if Step 2 proves index semantics, the narrow read-only
projection. Preserve existing `ChestDestructionRuleSystem` and
`ActuatorCanKillTileRuleSystem` as their own decision owners.

The new code must preserve the repository C# rules: 2-space indentation, 100-column limit, one core
type per file, clear names, braces, and no unrelated formatting changes. Keep all unknown protection
facts true/blocked at the active-above actuator producer. Do not pass `canKillTile: true` merely
because chest mapping works; the other `CanKillTile` protections remain unowned.

### Step 5: Focused GREEN and affected regressions

After the final source edit, rerun the focused Wiring verifier first, then all affected domain
verifiers. Any source modification after a command invalidates its result and restarts this sequence.

```powershell
function Invoke-RecordedDotnet {
  param(
    [Parameter(Mandatory)] [string] $name,
    [Parameter(Mandatory)] [string[]] $arguments)

  & dotnet @arguments 2>&1 | Tee-Object "$evidence\$name.txt"
  $exitCode = $LASTEXITCODE
  if ($exitCode -ne 0) {
    throw "dotnet $name failed: $exitCode"
  }
}

Invoke-RecordedDotnet "wiring-final" @(
  "run", "--project", "Test\Terraria.Dome.Wiring.Verification\Terraria.Dome.Wiring.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "wiring-liquid-chest-loopback-final" @(
  "run", "--project", "Test\Terraria.Dome.WiringLiquidChest.Loopback.Verification\Terraria.Dome.WiringLiquidChest.Loopback.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "persistence-final" @(
  "run", "--project", "Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "player-physics-final" @(
  "run", "--project", "Test\Terraria.Dome.PlayerPhysics.Verification\Terraria.Dome.PlayerPhysics.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "npc-final" @(
  "run", "--project", "Test\Terraria.Dome.Npc.Verification\Terraria.Dome.Npc.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")
```

### Step 6: Boundary and repository gates

```powershell
Invoke-RecordedDotnet "main-boundary-final" @(
  "run", "--project", "Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "root-release-build-final" @(
  "build", "Terraria.Dome.sln", "-c", "Release", "-m:1",
  "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false",
  "-p:FixtureHostBuild=false")

git diff --check | Tee-Object "$evidence\diff-check-final.txt"
if ($LASTEXITCODE -ne 0) {
  throw "git diff --check failed: $LASTEXITCODE"
}
```

The valid conclusion is `Accepted` only when all recorded commands exit `0`, root Release reports
no warnings/errors, MainBoundary has no forbidden Simulation dependencies, and the source manifest
matches the implementation's claimed behavior. Otherwise write `Partial` or `Blocked`, including
the first decisive failure and unchanged deferred scope.

## 6. AI Continuation Protocol

Use this as the work instruction for each AI turn:

```text
Own exactly one falsifiable behavior slice from the active migration work item.
Read the source oracle, responsibility ledger, current implementation, and relevant verifier first.
Do not ask for human guidance for compilation defects, narrow test defects, missing local evidence,
or deterministic implementation choices inside the declared owner boundary.

First make or recover a real RED. Then implement the smallest authority path. Run focused GREEN,
affected regressions, MainBoundary, serial root Release, and diff check from repository root.
Store fresh command output under the stated evidence directory. If source changes after a gate, rerun
all invalidated gates. Update the main execution handbook and progress.md only after final GREEN.

Never claim broad legacy parity from a pure contract or one fixture. Preserve all unowned legacy
conditions as rejected/deferred. Escalate only for a missing/conflicting source oracle, mutually
exclusive authority semantics, a required public protocol/save-format decision, a forbidden
dependency crossing into Simulation, or three attempts with the same evidenced root cause.
```

## 7. Acceptance Record Template

Append this record to the main execution handbook and `progress.md` only after Step 6 succeeds:

```markdown
## Task 8 / chest footprint projection accepted

Source oracle: `<paths, lines, hashes>`. Accepted proposition: `<single sentence>`.
Authority path: `<tile/frame -> origin -> chest lookup -> inventory rule -> protection fact>`.
Focused coverage: `<accepted and rejected cases>`.
Evidence: `Build/diagnostics/main-migration/task-8-chest-footprint/<runId>/`.
Gates: Wiring `<exit>`, loopback `<exit>`, Persistence `<exit>`, PlayerPhysics `<exit>`,
NPC `<exit>`, MainBoundary `<exit>`, root Release `<exit; warnings/errors>`, diff check `<exit>`.
Deferred: `<all still-unowned protections and actuator runtime branch>`.
```

Until that record is backed by a fresh evidence directory, Task 8 stays `IN PROGRESS`.
