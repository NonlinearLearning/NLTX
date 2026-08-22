# Authoritative WorldGrid Design

## Goal

Replace the fixed empty V1456 tile stream with an authoritative, bounded world
that provides real section snapshots, section visibility, and deterministic tile
changes without putting Terraria protocol or socket state into Simulation.

## Boundaries

`Terraria.Dome.Simulation` owns `WorldGrid`, tiles, section versions, immutable
section snapshots, and tile commands. It has no V1456 packet, socket, or session
slot dependency.

`Terraria.Dome.Server` owns session-to-player ownership and visible section
tracking. It uses player snapshots to subscribe sessions to sections, validates
client interaction intent before it reaches Simulation, and schedules replication.

`Terraria.Dome.Protocol.V1456` translates immutable section snapshots to message
10 `TileSection` frames. It must not mutate a world or retain an Arch entity.

## World Model

The initial dome world is bounded at the existing V1456 world-data dimensions:
4,200 tiles wide by 1,200 tiles high. Sections are 200 by 150 tiles, matching
the legacy server's `Netplay.GetSectionX`, `Netplay.GetSectionY`, and
`NetMessage.SendSection` protocol units.

Each tile records only the data required for this first slice: active state and
tile type. The grid keeps an independent monotonically increasing version for
each changed section. A section snapshot contains its coordinates, dimensions,
version, and tile values. Snapshots are immutable copies, so packet encoding
cannot observe a concurrent mutation.

## Tick and Replication Flow

```text
client V1456 intent
  -> Server session and visibility validation
  -> Simulation TileChangeCommand queue
  -> deterministic WorldGrid commit during Tick
  -> immutable WorldSectionSnapshot
  -> Server subscription and version comparison
  -> V1456 TileSection frame
```

The first implementation replaces initial fixed empty sections with snapshots
selected around the requested spawn position. It tracks section versions and
subscribed sections but leaves complete message-17 interaction decoding and
multi-session outbound writes to the next task, after a real world grid has
proved its contract.

## Safety Rules

- Simulation remains the single tile authority.
- Out-of-world coordinates and unknown section coordinates are rejected.
- A section is sent only once at its current version unless it becomes dirty.
- A tile mutation changes exactly one containing section version.
- V1456 TileSection encoding consumes a snapshot, not a mutable grid.
- Existing V1456 bootstrap flow and its frame order remain intact:
  `StatusTextSize`, zero or more `TileSection`, then `InitialSpawn`.

## Evidence

The units and flow are derived from
`D:\TRbackup\Version4物理删除了某些文件\Terraria\RemoteClient.cs` and
`D:\TRbackup\Version4物理删除了某些文件\Terraria\NetMessage.cs`:

- section dimensions are `200 x 150`;
- `SendSection` marks the session before transmitting tile data;
- world entities and container content are later synchronized after the section;
- message 17 is validated against world bounds and loaded sections before a
  server-side world mutation is broadcast.

The retained V1456 bootstrap verifier establishes the required compressed message
10 envelope and frame ordering.
