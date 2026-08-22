# Server ECS Convergence Design

## Decision

This work converges the server-authoritative ECS migration. It does not copy
`Main.cs`, `WorldGen.cs`, `Player.cs`, or other legacy files into new assemblies.
A legacy responsibility is accepted only when its source rule, authoritative
owner, state shape, deterministic mutation path, persistence or protocol
consequence, and focused verifier agree.

The reference source is `D:\TRbackup\Version4物理删除了某些文件`. It is an
intentionally reduced artifact, not a complete semantic oracle. A behavior
absent from that tree must be recovered from an approved complete source/runtime
oracle or remain explicitly deferred. File deletion is never evidence that a
server behavior is migrated.

## Boundary Model

```text
Version4 source rule
  -> immutable input/snapshot or definition
  -> Simulation query/system
  -> typed command
  -> deterministic commit
  -> Simulation snapshot revision
  -> Server persistence and Protocol V1456 projection
  -> focused verifier plus loopback verification
```

`Terraria.Dome.Simulation` owns authoritative gameplay state and deterministic
state transitions. `Terraria.Dome.Server` owns sessions, queues, bootstrap,
persistence orchestration, and outbound ordering. `Terraria.Dome.Protocol.V1456`
only decodes client input and projects server-owned snapshots. `WorldFile.V319`
and `WorldCompatibility` own legacy WLD parsing and lossless value projection.

UI, graphics, audio, RGB, local input, asset loading, social presentation, and
other client-only source responsibilities stay outside Simulation.

## Execution Strategy

The proposal uses the following gates, in this order:

1. A reproducible build baseline.
2. Repair of claimed server paths that currently fail loopback verification.
3. Completion of source-backed Main and WLD authority boundaries.
4. Stage-by-stage WorldGen state and oracle recovery, with the old-entry
   deletion gate left closed until full differential evidence exists.
5. A Version3-to-Version4 deletion ledger that records every server-relevant
   deleted dependency as recovered, replaced, excluded, or unknown.

The solution is intentionally incremental. It avoids a generic
`InitializeAlmostEverything`, `Queue<Action>`, generic `IEnumerator` scheduler,
duplicate `WorldTile`, global mutable random stream, and silent casts/defaults
for unknown legacy values.

## Success Criteria

The project can claim a converged server ECS migration only when all of these
conditions hold:

- affected projects build with the repository output policy and no new warnings;
- all claimed server capability verifiers, including loopback paths, pass from a
  clean process run;
- every server-relevant Main responsibility is `accepted`, `deferred`, or
  `excluded` with a source-backed owner and reason;
- WLD time, weather, progression, and world state either round-trip losslessly
  or explicitly reject an unsupported legacy form;
- each WorldGen stage has stable source and ECS fingerprints for its supported
  profile;
- `worldgen-deletion-gate.json` remains false until source inventory, complete
  differential replay, and required regression gates all prove safe removal;
- Version4 physical deletions have an evidence-backed classification rather
  than being treated as completed migration.
