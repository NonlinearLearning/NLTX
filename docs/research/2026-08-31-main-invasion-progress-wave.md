# `Main.invasionProgressWave` transient projection boundary

**Date:** 2026-08-31  
**Status:** `completed_partial`  
**Batch:** `main-field-property/invasion-progress-wave-20260831-01`

## Decision

`Main.invasionProgressWave` is carried into the existing immutable
`WorldInvasionProgressResult.Wave` projection. It is not added to
`WorldProgressionState`, a persistence snapshot, or a new ECS singleton. The projection accepts
the value at the same boundary where the legacy `ReportInvasionProgress` call receives it, while
the existing overload remains equivalent to the regular invasion wave `0` default.

This is a display/protocol-adjacent transient boundary only. Wave scheduling, NPC wave-counter
ownership, display lifetime, outbound publication, persistence, full protocol loopback, and
complete invasion parity remain deferred. The legacy `Main.cs` deletion gate is unchanged.

## Source authority

The Version4 source declares and assigns the field at:

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:1289` — static
  `invasionProgressWave` declaration.
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:12254-12262` —
  `ReportInvasionProgress(..., progressWave)` stores the supplied value and separately resets
  `invasionProgressDisplayLeft` to `160`.
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:13082` — a newly
  started invasion initializes the display wave to `0`.
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64796` — regular
  invasion progress reports wave `0`.
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:65088` and
  `:65218` — Pumpkin and Snow invasion reports pass their `waveNumber`.

The instrumented source mirror contains the same call shape at
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/Main.cs:1994,48020-48022`
and `.../NPC.cs:79441,79770,79906`.

## Typed owner and contract

The owner is
`src/Terraria.Dome.Simulation/World/Systems/WorldInvasionProgressProjectionSystem.cs`.
`Resolve(WorldProgressionState, int progressWave)` preserves the supplied wave for a valid
invasion state. Invalid or unavailable progression returns `IsAvailable = false` and `Wave = 0`;
it never fabricates a wave or mutates the progression state. The existing
`Resolve(WorldProgressionState)` overload delegates with `progressWave: 0`.

The V1456 wire type already has a `Wave` member and the codec preserves the final wave byte in
`src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs:3620-3701`. This batch therefore
does not alter message-78 encoding, publication, or client display lifetime; it only prevents the
source value from being lost at the typed projection boundary.

The Main inventory resolver maps the symbol as follows:

```text
Main.invasionProgressWave
  -> WorldInvasionProgressResult.Wave
  serialization = transient-projection
  verification = Terraria.Dome.WorldRules.Verification
```

No persistence field, serialized version marker, or ECS state component was introduced.

## Verification evidence

Fresh evidence is under
`Build/diagnostics/main-field-property/invasion-progress-wave-20260831-01/`:

- WorldRules focused verifier build and run pass; the run includes valid-wave preservation,
  legacy no-wave default `0`, and unavailable-state wave suppression.
- `Terraria.Dome.Simulation` Release build exits `0` with zero warnings/errors.
- Main inventory verifier build and run pass with
  `members=696`, `migratedScope=695`, `identityExcluded=1`, and `accepted-narrow=126`.
- Style, diff, JSON, Flowstate manifest, and Flowstate docs checks are recorded in the same
  directory; each final gate exits `0`.

## Deferred boundary

The following are intentionally not claimed by this slice:

- invasion wave scheduling and NPC wave-counter ownership;
- `invasionProgressDisplayLeft`/client display lifetime;
- outbound message-78 publication and complete protocol loopback;
- persistence or restart continuity for the transient wave;
- complete regular, Pumpkin, Snow, and Old invasion behavior;
- deletion of the legacy `Main.cs` oracle or promotion to full Main parity.
