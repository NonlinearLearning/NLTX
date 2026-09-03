# TeleportPylonsSystem 资格边界审计

## Source oracle

- Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria.GameContent\TeleportPylonsSystem.cs`
- SHA-256: `C1A6B6A163ACAB5E33E09EC6D4F49FB27881E47528844F1B652BE67B12777D1F`
- `Update`/cooldown: lines 25-36
- `HasPylonOfType`: lines 38-43
- list rebuild and add/remove broadcasts: lines 45-72
- `Reset` and join synchronization: lines 74-90
- client dust presentation: lines 92-157
- construction from `Main.Initialize_AlmostEverything`: `Main.cs:3772`
- update call: `Main.cs:13535`

## Qualification result

| Predicate | Result | Evidence |
|---|---|---|
| bounded owner surface | yes | one concrete system with explicit update/reset/join methods |
| server-only definition | no | rebuild scans tile entities and emits NetModule broadcasts; dust is client presentation |
| unique world-state owner | no | existing tree has pylon tile-entity compatibility data but no authoritative pylon collection/commit lifecycle |
| typed command/replay contract | no | source uses mutable lists and cooldown, without revisioned add/remove commands or replay identity |
| persistence/restart | partial | tile-entity payload can be projected, but collection ordering/cooldown and rebuild continuation are not modeled |
| protocol contract | partial | packet decode exists, but authoritative mutation and two-session ownership are absent |

## Decision

The family remains `unknown/deferred` for M-001. Do not add an ID-only pylon registry, generic
initializer, or treat `TeleportPylonModulePacket` decoding as lifecycle acceptance. A future child
must split server pylon discovery/commit from broadcast projection and client dust.

## Prerequisites

1. Typed pylon identity/position state sourced from tile-entity commits.
2. Deterministic rebuild, add/remove ordering and revision semantics.
3. Join snapshot and loopback projection ownership, including stale/duplicate rejection.
4. Explicit teleport eligibility/command authority if the system is to own teleport behavior.
5. Separate client-only dust/presentation boundary.
