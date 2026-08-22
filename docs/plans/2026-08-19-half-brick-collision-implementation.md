# Half-Brick Collision Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make an active, known, ordinary solid half-brick occupy only its source-backed lower
half during Dome runtime tile collision, without widening the slice to slopes or special tiles.

**Architecture:** `TileCollisionSystem` remains the only runtime collision owner. It will derive
an internal axis-aligned collision rectangle from a `WorldTile` after existing definition and
active/inactive validation. Full solid tiles retain their current unit rectangle; an unsloped
half-brick gets a unit width and half-unit height at the legacy lower-half position. Platform
top behavior remains in `PlatformCollisionRuleSystem`; slope geometry remains explicitly deferred.

**Tech Stack:** .NET 10, C#, Arch, executable console verifiers, serial Release gates.

---

### Task 1: Lock the half-brick runtime contract

**Files:**
- Modify: `Test/Terraria.Dome.PlayerPhysics.Verification/Program.cs`
- Reference: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Collision.cs:1633-1740`

**Step 1: Write the failing verifier**

Add a `VerifyHalfBrickCollisionGeometry` scenario that uses an active Type `1` tile with
`IsHalfBrick: true` and `Slope: 0`. Cover all of the following observable results:

```csharp
// Falling: the collider stops on the half-brick top and becomes grounded.
// Horizontal: a collider overlapping only the half-brick upper half passes through.
// Horizontal: a collider overlapping the lower physical half is blocked.
// Upward: collision occurs only when the target collider overlaps the lower physical half.
// Standing: a collider already aligned on the half-brick top remains grounded.
```

Retain the existing Type `1` full-block, Type `4` non-solid, Type `19` platform, unknown tile,
and slope-fallback assertions. Change the old combined `IsHalfBrick: true, Slope: 1` fixture only
if necessary to make its intentional slope-fallback purpose unambiguous.

**Step 2: Run the RED verifier**

From repository root, create a fresh directory beneath
`Build/diagnostics/main-migration/task-8-half-brick/<runId>/` and run:

```powershell
dotnet run --project Test\Terraria.Dome.PlayerPhysics.Verification\Terraria.Dome.PlayerPhysics.Verification.csproj \
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected: exit `1` because the current system treats the half-brick as a whole tile. Preserve
the complete output as `physics-red.txt`. Do not weaken an assertion to accommodate the current
whole-tile fallback.

### Task 2: Implement the narrow runtime geometry

**Files:**
- Modify: `src/Terraria.Dome.Simulation/Physics/Systems/TileCollisionSystem.cs`
- Test: `Test/Terraria.Dome.PlayerPhysics.Verification/Program.cs`

**Step 1: Add an internal geometry representation**

Introduce a private value-only rectangle or equivalent named methods in `TileCollisionSystem`.
It must expose the unit-grid left/right/top/bottom needed by the existing axis-separated movement
code. It is not a public WorldGeneration query and must not introduce a new generic geometry
module.

**Step 2: Derive geometry after definition validation**

For active, non-inactive, known tiles where `definition.BlocksLiquid && !definition.IsPlatform`:

```csharp
// Full ordinary solid tile: physical range [tileY, tileY + 1).
// Unsloped half-brick: physical lower-half range [tileY, tileY + 0.5).
```

Use this geometry consistently in horizontal overlap, upward collision, falling landing, and
standing checks. Preserve current out-of-world blocking and unknown active-tile rejection before
any transform, velocity, or grounded-state mutation.

**Step 3: Preserve out-of-scope behavior**

- A platform remains side/upward-passable and uses its current top-surface rule.
- A tile with `Slope != 0` must retain the currently accepted whole-tile fallback in this batch,
  including when `IsHalfBrick` is also true.
- Do not reuse `TileStateQuery`; it serves WorldGeneration/attachment semantics and deliberately
  excludes some runtime shapes.
- Do not alter Player input, NPC fall-through, protocol, persistence, or tile definition data.

**Step 4: Run focused GREEN**

Rerun the command from Task 1 serially and save `physics-final.txt`. Expected: exit `0`, with
the new half-brick line and all existing physics lines passing.

### Task 3: Run affected acceptance gates

**Files:**
- Modify after gates only: `progress.md`
- Modify after gates only:
  `docs/plans/2026-08-19-main-server-ecs-migration-execution.md`

**Step 1: Run serial affected regressions**

Use the same fresh evidence directory, one command at a time:

```powershell
dotnet run --project Test\Terraria.Dome.Npc.Verification\Terraria.Dome.Npc.Verification.csproj \
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false

dotnet run --project Test\Terraria.Dome.Liquid.Verification\Terraria.Dome.Liquid.Verification.csproj \
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false

dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj \
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false

dotnet build Terraria.Dome.sln -c Release -m:1 \
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:FixtureHostBuild=false
```

Record respectively `npc-final.txt`, `liquid-final.txt`, `main-boundary-final.txt`, and
`root-release-build-final.txt`. No protocol/TCP/persistence gate is required unless the source
change unexpectedly expands beyond runtime collision.

**Step 2: Verify source and write evidence**

Capture SHA-256 and line excerpts for the Version4 collision range in `source-manifest.txt`.
Run `git diff --check` and record it as `diff-check-final.txt`. Inspect the focused verifier to
confirm it covers the new lower-half geometry rather than only a boolean tile classification.

**Step 3: Update migration truth**

Only after all listed gates exit `0`, append the proposition, exact commands, exit codes, evidence
directory, accepted scope, and deferred slope/actuator/door/NPC-special-collision items to
`progress.md` and the Main ECS execution handbook. Task 8 remains `IN PROGRESS`; this batch does
not claim full `Terraria.Collision` parity.

**Step 4: Commit policy**

Do not create a mixed commit in the current dirty shared worktree. If the user later requests a
commit, stage only this plan, the collision system, the focused verifier, and the two migration
records after rechecking their current diff.
