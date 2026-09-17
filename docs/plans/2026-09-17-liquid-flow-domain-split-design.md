# Liquid Flow Domain Split Design

**Status:** Experimental branch only. This design does not claim a completed
Version4 migration, runtime scheduler integration, or liquid simulation parity.

## Goal

Validate `ecs-system-domain-splitting` on the P01 liquid-flow boundary by
extracting the deterministic state-update mapping from `LiquidFlowSystem` while
retaining that System as the only caller of `ILiquidFlowCommitPort`.

## Selection And Scope

P01 is the latest authoritative partition for Liquid, Wiring, Spatial, Death,
and Teleport. Its `LiquidSimulation` portion records 24 fields, including the
15-field `LiquidFlowBudgetAndPanicState` leaf. The P06 Player combat candidate
was not selected because a separate active worktree was already compiling an
experiment against that exact boundary. This is a collision-avoidance change to
the target, not a change to the user-requested experiment outcome.

The current WorldStorage source is untracked in `main`. Only this experimental
source closure will be copied into the disposable branch:

- `LiquidFlowSystem`
- `LiquidFlowTickInput` and `LiquidFlowTickResult`
- `LiquidFlowBudgetPolicy`, `LiquidFlowBudgetDecision`, and `LiquidFlowBudgetQuery`
- `LiquidFlowStateUpdate` and `LiquidFlowCommitResult`
- `ILiquidFlowCommitPort`

## Evidence

The P01 inventory identifies `LiquidFlowBudgetAndPanicStateComponent` as the
authority candidate with a `Liquid Flow System/CommitPort` seam. Version4
`Terraria.Liquid` shows that the original fields participate in queue processing,
tile mutation, panic recovery, logging, network publication, and random/world
state. Current NLTX deliberately represents a smaller state-oriented slice:

```text
LiquidFlowTickInput
  -> LiquidFlowBudgetQuery
  -> private CreateStateUpdate
  -> ILiquidFlowCommitPort.Commit
  -> LiquidFlowTickResult
```

The experiment preserves that current slice. It does not claim to reproduce
Version4 tile updates, buffer draining, publication, visual effects, or panic
recovery execution.

## Ownership And Data Flow

| Concern | Read/compute owner | Write/effect owner | Evidence status |
| --- | --- | --- | --- |
| Maximum/budget/panic policy | `LiquidFlowBudgetQuery` | none | confirmed |
| State-update mapping | new `LiquidFlowStateUpdateQuery` | none | proposed |
| Input validation and orchestration | `LiquidFlowSystem` | none | confirmed |
| Component mutation | none | `ILiquidFlowCommitPort` implementation | confirmed |
| Tick result projection | `LiquidFlowSystem` | none | confirmed |
| Version4 tile/network behavior | excluded | excluded | partial |

The query has no component reference, collection, port, clock, random source,
or I/O dependency. `LiquidFlowSystem` remains the single effect boundary for
this experimental path.

## Boundary Decision

1. Keep the existing private method: no new tested seam, so it does not test the
   skill's Query/Command/Adapter distinction.
2. Split the System into `partial` files: changes navigation only and does not
   expose a behavior boundary.
3. Extract `LiquidFlowStateUpdateQuery`: creates a pure, reusable calculation
   seam without a second writer or scheduling node.
4. Create another tick System: rejected because it would create an additional
   state-transition owner without lifecycle or phase evidence.

Option 3 is selected.

## Target Design

Add a stateless public `LiquidFlowStateUpdateQuery` with one method:

```csharp
public static LiquidFlowStateUpdate Create(
  in LiquidFlowTickInput input,
  in LiquidFlowBudgetDecision decision)
```

For a normal decision it preserves the input's state values and applies the
decision's quick-fall and panic-counter outputs. For an enter-panic decision it
sets active liquid count and panic counter to zero, enables panic mode, and
uses `PanicStartY`. `LiquidFlowSystem.Advance` continues to validate the input,
evaluate the budget query, call the state-update query, commit exactly once,
and return the existing result type.

## Invariants

- Input validation behavior and public `Advance` signature stay unchanged.
- The query cannot invoke `ILiquidFlowCommitPort` or mutate a component.
- Every successful `Advance` calls the commit port exactly once.
- Panic entry produces the same state update as the current private method.
- Non-panic updates preserve all non-decision input state values.
- No persistence, protocol, scheduler, registration, or adapter contract moves.

## Verification And Rollback

A focused executable verifier will compile the copied source closure directly.
Its first run will prove red through a runtime assertion for the absent query
type, while the existing orchestration assertions still compile. After the
production types are added, it will verify normal and panic results, one commit
per advance, commit rejection propagation, and input rejection before commit.
The serial wrapper will run both build/run and `--no-build --no-restore` paths.

Rollback removes the new query and restores the original private mapping method.
No persisted state or irreversible external effect is introduced.
