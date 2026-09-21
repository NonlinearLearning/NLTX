# Player Combat Proc Domain Split Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Validate a narrow P06 combat-proc experiment by separating deterministic tick calculation from the single state-writing `PlayerCombatProcSystem`.

**Architecture:** `PlayerCombatProcTickQuery` receives an immutable value input and returns an immutable value result. `PlayerCombatProcSystem.AdvanceTick` is still the only component writer: it builds the input, calls the query, and commits every changed field. Hit idempotency, reset behavior, EOC dash commands, snapshots, runtime registration, persistence, networking, random effects, and original `Terraria.Player` integration remain outside this experiment.

**Tech Stack:** C# `net10.0`, focused console verifier, repository serial .NET wrapper.

---

### Task 1: Make The Tick Boundary Observable

**Files:**
- Modify: `Test/Terraria.PlayerCombatProcSplitVerification/Program.cs`

**Step 1: Write the failing test**

Add a direct `PlayerCombatProcTickInput` construction and call a missing
`PlayerCombatProcTickQuery.Advance`. Assert that the result clamps ghost damage,
life steal, dash target, counter wrap, and each cooldown. Keep the existing
system-level `AdvanceTick` assertions to prove the system commits that result.

**Step 2: Run the verifier to verify it fails**

From the isolated worktree root, inspect active `dotnet.exe` and `csc.exe` first.
Then run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  run .\Test\Terraria.PlayerCombatProcSplitVerification\Terraria.PlayerCombatProcSplitVerification.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
```

Expected: compilation fails because the tick input, result, and query types do
not yet exist. Record the command, exit code, and failure cause.

### Task 2: Add The Pure Tick Value Types And Query

**Files:**
- Create: `src/Player/PlayerCombatProcTickInput.cs`
- Create: `src/Player/PlayerCombatProcTickResult.cs`
- Create: `src/Player/PlayerCombatProcTickQuery.cs`

**Step 1: Write the minimal implementation**

Declare immutable `readonly record struct` input and result types. Input holds
the nine component fields read by the current tick logic plus `ExpertMode`.
Result holds the nine fields it changes. `Advance` must use only its input and
constants for ghost decay, life-steal recovery/cap, EOC dash/sentinel handling,
inferno wrap, and non-underflowing cooldown decrement.

**Step 2: Run the verifier to verify it progresses**

Run the same serial wrapper command. Expected: any remaining failure identifies
the missing integration only; no original Terraria runtime behavior is claimed.

### Task 3: Keep The System As The Sole Commit Owner

**Files:**
- Modify: `src/Player/PlayerCombatProcSystem.Tick.cs`

**Step 1: Integrate the query**

Replace direct tick arithmetic in `AdvanceTick` with construction of
`PlayerCombatProcTickInput`, `PlayerCombatProcTickQuery.Advance`, and explicit
assignment of each result field back to `_component`. Keep `BeginEocDash`,
`CommitEocDashHit`, and `RecordPhantomPhoneixLaunch` in the system.

**Step 2: Run the verifier to verify it passes**

Run the same serial wrapper command. Expected output:

```text
PASS: player combat proc split behavior
```

### Task 4: Verify The Affected Project And Artifacts

**Files:**
- Verify: `src/Player/Terraria.Player.csproj`
- Verify: `Test/Terraria.PlayerCombatProcSplitVerification/Terraria.PlayerCombatProcSplitVerification.csproj`

**Step 1: Build the affected production project**

Use the serial wrapper with `build .\src\Player\Terraria.Player.csproj` and the
same MSBuild safety properties. Record exit code, warning/error count, and
artifact path under `Build/bin/`.

**Step 2: Run the verifier without rebuilding**

Use the serial wrapper with `run` plus `--no-build --no-restore` for the focused
verifier. Confirm that its emitted artifact is under `Build/bin/` and record its
exit code and output.

### Task 5: Document And Isolate The Result

**Files:**
- Modify: `docs/plans/system-decomposition/2026-09-17-player-combat-proc-domain-split-design.md`
- Modify: `docs/plans/system-decomposition/2026-09-17-player-combat-proc-domain-split-implementation-plan.md`

**Step 1: Record scope and evidence limits**

State that P06 is source-inventory-confirmed only, identify the Version4
`Player.cs` SHA-256, list excluded persistence/networking/random/presentation
paths, and state that this is not a deletion gate or full migration claim.

**Step 2: Commit only experiment-owned files**

Do not stage `.learnings/ERRORS.md` or unrelated untracked documentation. Commit
the focused source, verifier, and plan/design updates on the disposable branch.
The final cleanup removes this worktree and branch only after the result is
reported.

### Execution Record

The plan was executed with one scope-isolation correction. The focused verifier
initially used a `ProjectReference` to `Terraria.Player.csproj`; it was changed
to explicit `Compile` entries for the twelve C02 source files so that the
focused evidence remains valid when the result is brought back to the main
worktree, whose wider Player tree contains unrelated Progression references.

Observed evidence:

- RED: serial `run --project` exited `1` with four expected `CS0246` errors for
  the missing tick input/result/query types.
- GREEN: serial verifier project build exited `0`, with 0 warnings and 0
  errors; the verifier run with `--no-build --no-restore` exited `0` and printed
  `PASS: player combat proc split behavior`.
- Production closure: serial build of `src/Player/Terraria.Player.csproj`
  exited `0`, with 0 warnings and 0 errors, producing
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`.
- The experiment does not settle the active P06 partition session and does not
  satisfy the Version4 deletion gate.
