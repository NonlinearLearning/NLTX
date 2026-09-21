# Player Combat Proc Domain Split Design

**Status:** Experimental branch only. This document does not claim a completed
Version4-to-ECS migration or behavioral equivalence with Terraria.Player.

## Goal

Exercise `ecs-system-domain-splitting` against a non-trivial P06 Player combat
slice by extracting deterministic per-tick calculation from the current
`PlayerCombatProcSystem` while preserving that System as the sole writer of
`PlayerCombatProcStateComponent`.

## Scope And Inputs

The authoritative inventory input is P06, `Player Combat Status`, which records
22 leaf subsystems and 254 Player fields. This experiment deliberately narrows
implementation to its `PlayerCombatDamageProcState` leaf because the current
NLTX source provides a focused, testable System boundary for its 15 state fields.

The current candidate sources are untracked in the main checkout and are
therefore copied only into this disposable worktree before the experiment:

- `src/Player/PlayerCombatProcSystem.cs`
- `src/Player/PlayerCombatProcStateComponent.cs`
- `src/Player/PlayerCommittedCombatHitEvent.cs`
- `src/Player/PlayerCombatProcHitEffects.cs`
- `src/Player/PlayerCombatProcQuery.cs`
- `src/Player/PlayerCombatProcSnapshot.cs`
- `src/PlayerCombatProcVerification/Program.cs`

Version4 source evidence establishes that the original fields span hit handling,
per-tick update, projectile interactions, visual effects, randomness, and
equipment rebuilding. The current NLTX System models only a state-oriented
subset. The experiment must preserve current NLTX behavior only; original
Version4 side effects remain outside scope.

## Evidence And Ownership

| Concern | Current evidence | Owner after experiment | Status |
| --- | --- | --- | --- |
| Committed-hit validation and idempotency | `PlayerCombatProcSystem` owns accepted event IDs | `PlayerCombatProcSystem` | confirmed in current source |
| Hit state commit | The System writes life-steal, ghost damage, and proc flags | `PlayerCombatProcSystem` | confirmed in current source |
| Tick transition | The System currently reads and writes nine timer/resource fields | Pure `PlayerCombatProcTickQuery` computes; System commits | proposed |
| Snapshot projection | `PlayerCombatProcQuery` copies component state | `PlayerCombatProcQuery` | confirmed in current source |
| Original Terraria effects | Version4 uses lighting, random effects, projectile paths, and player update flow | Not represented by this experiment | partial / excluded |

`PlayerCombatProcStateComponent` remains the authoritative holder of all 15
fields. The extracted query does not retain a component reference, mutable
collection, clock, random source, event subscription, or I/O dependency.

## Boundary Decision

Three options were evaluated.

1. Keep the System unchanged: preserves behavior but does not exercise a new
   boundary.
2. Split source files with `partial`: improves navigation but creates no
   independently testable rule boundary.
3. Extract a pure tick query and keep one System writer: isolates deterministic
   rules, keeps idempotency and lifecycle state with their current owner, and
   does not add scheduling nodes.

Option 3 is selected. A separate tick System is rejected because it would add a
second writer for the same component without evidence of a distinct lifecycle,
phase, or transaction boundary.

## Target Design

The experiment adds these public, immutable value types and one stateless query:

- `PlayerCombatProcTickInput`: the exact component values read by the tick rule
  plus `ExpertMode`.
- `PlayerCombatProcTickResult`: the exact component values changed by the tick
  rule.
- `PlayerCombatProcTickQuery`: maps one input to one result without mutation.

`PlayerCombatProcSystem.AdvanceTick` constructs the input from the component,
calls the query, and applies the result. It remains the only code that commits
the result to the component. `AcceptCommittedHit`, EOC dash commands,
`RecordPhantomPhoneixLaunch`, `ResetEffects`, and `Reset` remain in that System.

```text
committed hit / lifecycle command --> PlayerCombatProcSystem --> component

component snapshot + expert mode --> PlayerCombatProcTickQuery --> tick result
                                                        |
                                                        v
                                            PlayerCombatProcSystem commits

component --> PlayerCombatProcQuery --> immutable snapshot
```

## Invariants

- Duplicate committed-hit IDs never spend life steal or add ghost damage twice.
- The tick query has no external reads and cannot mutate the component.
- `AdvanceTick` preserves existing life-steal caps, ghost-damage decay, EOC
  sentinel handling, inferno wrapping, and cooldown decrement semantics.
- The public `PlayerCombatProcSystem` command surface and its scheduling remain
  unchanged.
- There is one component writer: `PlayerCombatProcSystem`.

## Verification And Rollback

A new focused verifier will compile only this System's source closure, so
unrelated `Terraria.Player` project failures cannot hide the experiment result.
The first test will assert the planned tick-query contract before its production
types exist, then run red. The implementation will be added minimally and the
same verifier will run green. The branch will also attempt the affected full
project build through `Build/Tools/Invoke-SerialDotnet.ps1`; any unrelated
pre-existing failure will be reported separately, not relabeled as success.

Rollback is a single commit revert: remove the query, input, result, and focused
verifier, then restore the prior `AdvanceTick` body. No persistence schema,
network protocol, runtime registration, or side-effect adapter changes are in
scope.

## Non-Goals

- No claim that all P06 members are migrated.
- No new Component, event bus, command queue, scheduler phase, or adapter.
- No claim that original projectile, presentation, random, persistence, or
  network behavior is covered.

## Execution Record

The experiment was executed in `D:\TRbackup\NLTX-ecs-system-split-p06-experiment-20260917`
on branch `codex/ecs-system-split-p06-experiment-20260917`, based on commit
`3a90e999d4da756ae9e45a864a272df748ae47cd` plus the existing design commit
`80efab1a811468b63c3c9d16aa74225cd34a1372`.

- The P06 runner validated `254/254` members and left the already-running
  manual P06 session untouched.
- The final source closure contains the state Component, the partial owner
  (`PlayerCombatProcSystem`, `HitCommit`, `Tick`, and `Lifecycle`), the
  immutable snapshot Query, and the new immutable tick input/result and pure
  tick Query.
- The focused verifier first failed with four expected missing-type errors,
  then built and ran with exit code `0` and the `PASS: player combat proc split
  behavior` marker.
- The focused project explicitly compiles the C02 source closure. This keeps
  its evidence independent from unrelated `Terraria.Player` project failures;
  the production project is built separately.

The resulting status is `focused-verified`, not `P06-complete`,
`runtime-integrated`, or `deletion-eligible`. See
`docs/system-decomposition/reports/2026-09-17-ecs-system-domain-splitting-p06-system-experiment-optimization-report.md`
for the full read/write contract, evidence ledger, limits, rollback, and
skill optimization findings.
