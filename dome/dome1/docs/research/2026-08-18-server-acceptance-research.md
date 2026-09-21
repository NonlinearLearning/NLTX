# Multiplayer ECS Server Acceptance Research

## Scope

This note compares first-party testing and replay mechanisms from mature multiplayer or
ECS-oriented projects and adapts the useful parts to the Terraria Dome migration. The goal
is an executable acceptance process for an AI-driven migration, not a larger checklist or a
manual percentage claim.

## Findings From Primary Sources

### OpenRA: record the exact input stream

OpenRA's `ReplayRecorder` stores the client identifier, the frame number and the original
order bytes. It delays creating the replay file until the game-start order, then writes
metadata when the recording is closed.

Source: [OpenRA ReplayRecorder.cs](https://github.com/OpenRA/OpenRA/blob/bleed/OpenRA.Game/Network/ReplayRecorder.cs)

Implication for Dome: a replay must contain the session/player identity, simulation tick or
sequence, and the exact decoded or raw input. A test that only calls a system method without
the network input envelope cannot prove protocol authority or ordering.

### RobustToolbox/Space Station 14: record state and messages per tick

The server replay manager computes PVS state, records the current game state, serializes the
queued network or server-only messages, and writes bounded tick batches asynchronously. Replay
metadata includes engine/content/serializer hashes, start and end tick, duration and file
statistics. The replay interface explicitly supports recording server-only messages that are
not visible to a client.

Sources:

- [Server ReplayRecordingManager.cs](https://github.com/space-wizards/RobustToolbox/blob/master/Robust.Server/Replays/ReplayRecordingManager.cs)
- [SharedReplayRecordingManager.cs](https://github.com/space-wizards/RobustToolbox/blob/master/Robust.Shared/Replays/SharedReplayRecordingManager.cs)
- [IReplayRecordingManager.cs](https://github.com/space-wizards/RobustToolbox/blob/master/Robust.Shared/Replays/IReplayRecordingManager.cs)
- [Client integration test tree](https://github.com/space-wizards/RobustToolbox/tree/master/Robust.Client.IntegrationTests)

Implication for Dome: each acceptance run should emit an authoritative snapshot hash per tick,
outbound frame records per session, and build/protocol/reference hashes. The replay must be
re-runnable without a live client. PVS decisions and rejected inputs belong in the evidence,
not only successful gameplay events.

### Microsoft .NET testing guidance: isolation and repeatability

Microsoft's unit-testing guidance emphasizes tests that are isolated, repeatable and
deterministic, with one behavior under test and no dependence on shared mutable state.

Source: [.NET unit testing best practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

Implication for Dome: use a fresh world/seed/session fixture for every scenario, and make the
seed, tick rate, input trace and expected result explicit. A green test that depends on stale
Build output or mutable global state is not an acceptance result.

### Playwright: preserve diagnostic traces for client workflows

Playwright's trace viewer is designed to retain screenshots, DOM snapshots, network activity
and console information for a failed browser workflow.

Source: [Playwright Trace Viewer](https://playwright.dev/docs/trace-viewer)

Implication for Dome's real-client smoke tests: retain packet traces, server logs, client
crash logs and the replay identifier together. A pass/fail line without the trace is difficult
for an AI to repair.

## Recommended Dome Acceptance Model

The existing weighted capability matrix should remain a dashboard only. It should not be the
primary gate because its command strings and status fields can become stale while the working
tree changes. A capability is accepted only after all required gates below pass.

| Gate | Evidence | Required result |
|---|---|---|
| R0 Build identity | git/reference/source hashes, SDK, configuration, output manifest | fresh Release build; no stale `--no-build` artifact is used as new evidence |
| R1 Deterministic replay | input trace, seed/world fixture, per-tick state hash | two clean runs produce identical state hashes and command ordering |
| R2 Authority and rejection | accepted and rejected protocol envelopes | forged owner, bad range, invisible section, malformed and rate-limited inputs leave state unchanged |
| R3 Multiplayer loopback | two or more sessions, outbound frame trace and PVS decisions | only eligible sessions receive the revision; disconnect cleans all session state |
| R4 Fault matrix | delay, duplication, reordering, truncation, loss and slow-reader runs | server remains authoritative, bounded and live; failures are isolated to the affected session |
| R5 Persistence/restart | save, reload, replay continuation and snapshot hashes | equivalent world state is recovered or the load is rejected without mutating the live world |
| R6 Real-client smoke | client/server packet trace, client log/crash log and replay | world entry, one representative action and disconnect/reconnect complete with no unexplained frame |
| R7 Soak/nightly | long run metrics and periodic hashes | no unbounded queue/entity/session growth and no hash drift under a fixed scenario |

R1-R3 are the minimum merge gate for a gameplay family. R4-R7 are milestone or nightly gates,
not reasons to call a family complete when R1-R3 are missing.

## AI Repair Driver

Drive the AI with one machine-readable scenario card at a time. Each card should contain:

```text
id
reference: Version4 file/method/message path
authority_owner: Simulation or Server type
input_trace: exact session inputs and tick/sequence numbers
fixture: seed, world snapshot, player/session setup
expected: state assertions, outbound frames, and rejection assertions
gates: R1..R7 required for this family
artifacts: replay, state hashes, packet trace, logs, diff
stop_conditions: missing oracle, unrelated dirty change, nondeterminism, repeated failure
```

The controller loop is:

1. Run a read-only preflight and verify that the reference, source and build identities are
   present.
2. Run the scenario before editing and capture the RED failure at a tick, sequence or frame.
3. Give the AI only the card and the failure bundle. It adds or repairs the smallest authority
   path and updates the scenario when behavior is intentionally changed.
4. Re-run R1, then R2/R3, then the affected suite and Release build. Generate a new evidence
   bundle with a unique run ID.
5. A separate evaluator checks the evidence and acceptance gates. The implementing AI cannot
   mark its own family evidenced by changing the manifest.
6. After two or three failed attempts with the same root cause, stop the card and classify it as
   an oracle gap, architecture gap or implementation bug. Do not keep prompting the AI with the
   same failure text.

The AI should be allowed to continue automatically through ordinary compile/test failures.
Human input is reserved for choosing the intended behavior when the reference is missing or
contradictory, not for explaining every compiler error.

## Immediate Migration Sequence

1. Freeze and hash a readable Version4 reference snapshot. The current reference directory
   exists, but physically deleted files mean any missing behavior must be marked `unknown`, not
   reconstructed from class names or packet catalogs.
2. Build a replay runner around the existing `Terraria.Dome.sln` and executable verifier
   projects. Make the runner emit input, per-tick snapshots, outbound frames and metadata.
3. Convert three high-value flows first: world entry, player movement/collision, and one tile or
   combat mutation. Do not start with a broad all-gameplay score.
4. Add two-session PVS and malformed-input cases to every flow. This is where client-only or
   optimistic authority implementations usually fail.
5. Add save/reload and real-client smoke traces. Only then regenerate the capability dashboard.

The result is a replay-backed acceptance system: AI work is driven by a reproducible failure,
and completion is a fresh, independently checked evidence bundle rather than a manually edited
score.
