# Project Progress Summary

# 2026-08-25 World-item spawn allocator boundary

`WorldItemSpawnSystem` now rejects an allocator value of `int.MaxValue` before world-item creation
or allocator mutation, returning the existing typed command rejection instead of surfacing checked
overflow. Valid item spawn inputs remain green. Allocator widening, legacy identity ordering,
complete item lifecycle and client/network behavior remain deferred. Evidence:
`docs/research/2026-08-25-world-item-spawn-allocator-boundary.md` and
`Build/diagnostics/main-migration/task-11-world-item-spawn-allocator-boundary/20260825-260000/`.

# 2026-08-25 Wiring input sequence boundary

`WiringInputValidationSystem` now rejects `long.MaxValue` input sequences before queue insertion,
preventing `DomeSimulation`'s next wiring sequence allocator from wrapping. Existing player,
range and wire validation remains green. Complete wiring graph semantics, persistence/restart,
legacy random behavior and client/network effects remain deferred. Evidence:
`docs/research/2026-08-25-wiring-input-sequence-boundary.md` and
`Build/diagnostics/main-migration/task-11-wiring-input-sequence-boundary/20260825-250000/`.

# 2026-08-25 Liquid source sequence boundary

`LiquidInputSystem` now rejects `long.MaxValue` source sequences before queue insertion, preventing
`DomeSimulation`'s next liquid sequence allocator from wrapping. Valid bounded input and liquid
propagation/commit/replication remain green. Wiring sequence ownership, complete liquid parity,
legacy random behavior and client/network effects remain deferred. Evidence:
`docs/research/2026-08-25-liquid-source-sequence-boundary.md` and
`Build/diagnostics/main-migration/task-11-liquid-source-sequence-boundary/20260825-240000/`.

# 2026-08-25 Sign ID restore boundary

`DomeSimulation.RestoreSign` now rejects a persisted sign `SignId` of `int.MaxValue` before sign
registration or the next-ID allocator can wrap. Valid sign persistence, including ID zero, remains
green. This is a narrow identity-integrity guard; allocator widening, legacy identity ordering,
complete sign lifecycle and client/network behavior remain deferred. Evidence:
`docs/research/2026-08-25-sign-id-restore-boundary.md` and
`Build/diagnostics/main-migration/task-11-sign-id-restore-boundary/20260825-230000/`.

# 2026-08-25 Chest ID restore boundary

`DomeSimulation.RestoreChest` now rejects a persisted chest `ChestId` of `int.MaxValue` before
registration or the next-ID allocator can wrap. Valid chest persistence and ownership/transfer
verification remain green. This is a narrow identity-integrity guard; allocator widening, legacy
identity ordering, complete chest lifecycle and client/network behavior remain deferred. Evidence:
`docs/research/2026-08-25-chest-id-restore-boundary.md` and
`Build/diagnostics/main-migration/task-11-chest-id-restore-boundary/20260825-220000/`.

# 2026-08-25 NPC ID restore boundary

`DomeSimulation.RestoreNpc` now rejects a persisted NPC `ReplicationId` of `int.MaxValue` before
the next-handle allocator can wrap or an Arch entity can be created. Valid NPC persistence and
death/loot restart behavior remain green. This is a narrow identity-integrity guard; NPC tables,
AI families, spawn rules, allocator widening, legacy identity ordering and complete lifecycle/client
behavior remain deferred. Evidence:
`docs/research/2026-08-25-npc-id-restore-boundary.md` and
`Build/diagnostics/main-migration/task-10-npc-id-restore-boundary/20260825-210000/`.

# 2026-08-25 Tile-entity ID restore boundary

`DomeSimulation` now rejects a persisted tile-entity `Id` of `int.MaxValue` before the next-ID
allocator can wrap. Valid Training Dummy persistence and ownership restoration remain green. This
is a narrow persistence-integrity guard; allocator widening, legacy identity ordering, complete
tile-entity tables and client/network behavior remain deferred. Evidence:
`docs/research/2026-08-25-tile-entity-id-restore-boundary.md` and
`Build/diagnostics/main-migration/task-11-tile-entity-id-restore-boundary/20260825-200000/`.

# 2026-08-25 World-item replication-ID restore boundary

`DomeSimulation` now rejects a persisted world-item `ReplicationId` of `int.MaxValue` before the
next-ID allocator can wrap. Valid persistence snapshots still restore their world-item set. This
is a narrow persistence-integrity guard; allocator widening, legacy ID ordering, network/client
effects and complete item lifecycle parity remain deferred. Evidence:
`docs/research/2026-08-25-world-item-replication-id-boundary.md` and
`Build/diagnostics/main-migration/task-11-world-item-replication-id-boundary/20260825-190000/`.

# 2026-08-25 Item-use command sequence boundary

`ItemUseSystem` now rejects negative deterministic command sequences before health, mana, cooldown
or inventory mutation. Valid recovery/combat use and event sequencing remain unchanged; legacy input
cadence, item tables, client animation and network projection remain deferred. Evidence:
`docs/research/2026-08-25-item-use-sequence-boundary.md` and
`Build/diagnostics/main-migration/task-11-item-use-sequence-boundary/20260825-180000/`.

# 2026-08-25 NPC definition enum boundary

`NpcDefinition` now rejects undefined behavior, faction and category enum values before registry or
spawn use. Zero loot-table identity remains valid for immortal definitions such as the training dummy;
death publication keeps its separate positive loot-table requirement. Complete NPC tables, AI styles,
spawn rules and type-specific loot remain deferred. Evidence:
`docs/research/2026-08-25-npc-definition-enum-boundary.md` and
`Build/diagnostics/main-migration/task-10-npc-definition-enum-boundary/20260825-170000/`.

# 2026-08-25 World-item pickup-delay revision boundary

`WorldItemPickupDelaySystem` now rejects max item/world-state revisions before checked increment
while advancing a positive pickup delay. Valid delay decrement remains unchanged; reservation aging,
enemy pickup, slot reuse, persistence and network/client effects remain deferred. Evidence:
`docs/research/2026-08-25-world-item-pickup-delay-revision-boundary.md` and
`Build/diagnostics/main-migration/task-11-world-item-pickup-delay-revision-boundary/20260825-160000/`.

# 2026-08-25 World-item stack revision boundary

`WorldItemStackingSystem` now rejects receiver/donor item or embedded world-state revisions at
`long.MaxValue` before checked merge increments. Valid compatible stack merging and same-tick guards
remain unchanged; owner selection, pickup delay, shimmer/encumbrance, complete item definitions and
network/client effects remain deferred. Evidence:
`docs/research/2026-08-25-world-item-stack-revision-boundary.md` and
`Build/diagnostics/main-migration/task-11-world-item-stack-revision-boundary/20260825-150000/`.

# 2026-08-25 World-item motion revision boundary

`WorldItemMotionSystem` now rejects `long.MaxValue` item or embedded world-state revisions before
checked increment. Valid finite active movement and section assignment remain unchanged. Slot reuse,
pickup-delay/enemy-timer parity, collision/encumbrance, shimmer, network cadence and client effects
remain deferred. Evidence:
`docs/research/2026-08-25-world-item-motion-revision-boundary.md` and
`Build/diagnostics/main-migration/task-11-world-item-motion-revision-boundary/20260825-140000/`.

# 2026-08-25 World-item destroy revision boundary

`WorldItemDestroySystem` now rejects an active item when its item or world-state revision is
`long.MaxValue`, before checked increment can throw. Expected-revision matching and valid inactive
tombstone behavior remain unchanged; slot reuse, network cadence, enemy pickup timers, shimmer and
complete item tables remain deferred. Evidence:
`docs/research/2026-08-25-world-item-destroy-revision-boundary.md` and
`Build/diagnostics/main-migration/task-11-world-item-destroy-revision-boundary/20260825-130000/`.

# 2026-08-25 Projectile collision geometry boundary

`ProjectileCollisionSystem` now fails closed for non-finite transforms and non-positive/non-finite
collider dimensions before floor/interpolation/tile access. Valid swept solid-tile collision is
unchanged; type hitboxes, reflection/bounce, slopes/liquids and client effects remain deferred.
Evidence:
`docs/research/2026-08-25-projectile-collision-geometry-boundary.md` and
`Build/diagnostics/main-migration/task-10-projectile-collision-geometry-boundary/20260825-120000/`.

# 2026-08-25 Item placement sequence boundary

`ItemPlacementSystem` now rejects negative deterministic command sequences before emitting tile or
wall placement commands. Valid placement remains unchanged; world bounds, occupancy, framing, item
tables and client placement effects remain deferred. Evidence:
`docs/research/2026-08-25-item-placement-sequence-boundary.md` and
`Build/diagnostics/main-migration/task-11-item-placement-sequence-boundary/20260825-110000/`.

# 2026-08-22 Entity/event lifecycle gap matrix

Added a source-backed accounting matrix for player/NPC/projectile/item lifecycle, invasion/weather
events, static tables, random starts and client/presentation branches. Existing narrow owners remain
scoped to their accepted predicates; unresolved tables, global random ordering, NPC-driven invasion
spawns and client effects remain explicitly deferred. No production code or default table was added.
Evidence:
`docs/research/2026-08-22-entity-event-lifecycle-gap-matrix.md` and
`Build/diagnostics/main-migration/task-13-entity-event-lifecycle-gap/20260822-100000/`.

Last updated: 2026-08-22

完整历史记录已归档至
`docs/archive/progress-2026-08-22-full.md`。本文件只保留当前上下文所需的摘要，避免每次 AI
任务加载数千行历史记录。

## Current Architecture

- `Terraria.Dome.sln` 是当前构建入口，目标框架为 `net10.0`。
- `Terraria.Dome.Simulation` 承载 Arch ECS、组件、系统、命令和不可变快照。
- `Terraria.Dome.Server` 承载服务器循环、会话、权威状态和复制。
- `Terraria.Dome.Protocol.V1456` 承载 V1456 协议编解码与兼容投影。
- `Terraria.WorldFile.V319` 和 `Terraria.WorldCompatibility` 承载世界文件读取及兼容投影。

## Current Qualification

- MainBoundary、WorldObjects、Persistence、WorldRules、WorldClock、WorldImport、
  PlayerLifecycle、Combat 和 Completion 已有新鲜退出码为 0 的证据。
- PlayerAuthority 的 bootstrap 队列修复已有五次重复运行成功证据；后续变更仍需重新验证。
- 当前约 40% 的主迁移范围已完成资格审查，整体状态仍为 `incomplete`。
- TileEntity 完整行为、若干初始化责任族、完整 NPC/Projectile/Item 行为表和
  WorldGen 物理删除仍未完成或明确延期。

## Recent Boundaries

- Projectile motion and world-item pickup reject non-finite input before mutation.
- NPC target routing and target selection use fail-closed validation for encoded targets,
  identities, lifecycle state, stable ids, and positions.
- Server session outbound flushing uses the bounded writer queue without blocking the
  Simulation tick loop.

## Verification Policy

- Build and test commands run from the repository root with
  `-p:UseSharedCompilation=false`.
- Build outputs, generated files, packages, diagnostics, and other local artifacts belong
  under `Build/` and are excluded from source commits.
- New claims require fresh evidence under `Build/diagnostics/` and a source-backed boundary.

## Open Work

- Continue migration in small responsibility families with explicit source ownership.
- Re-run focused verifiers after each behavioral change and record only the current result here.
- Keep deferred capability families and physical-deletion gates separate from accepted slices.
