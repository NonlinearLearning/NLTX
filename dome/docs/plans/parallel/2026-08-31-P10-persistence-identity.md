# P10 Persistence, Identity, and Deletion-Gate Convergence Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 在第一波领域工作稿完成后，统一业务持久化、身份、revision、restore/overflow
边界，并逐条核对 44 条 `ServerRelevant` 删除门记录。

**Architecture:** 持久化只承载 versioned value-only business state；运行时派生值、transient
worklist 和 host resource 明确分开。业务 identity 与 Protocol/wire identity 分离，删除门
只接受逐字段 replacement evidence，不接受相邻字段或局部 slice 的推断。

**Tech Stack:** Documentation-only Markdown; no C# changes, no new functions, no tests/builds.

---

## 1. Task identity and write set

| 项目 | 内容 |
| --- | --- |
| Task | `P10` |
| Parent plan | `docs/plans/2026-08-31-server-required-field-property-migration-plan.md` |
| Dependency | P01-P09 work稿完成后启动 |
| Wave | `2`，可与 P11 并行 |
| Read set | P00-P09 work稿、WLD model/compatibility、player persistence、world object restore、physical deletion ledger |
| Write set | 仅本文件 |
| Status | `[x] completed_partial` (documentation-only; unresolved rows remain `deferred`) |

## 2. Hard boundaries

- 只记录 business persistence；任何 packet/wire/replication projection 由 P09 标为 `excluded`。
- `WorldId`、Player handle、NPC entity handle、Projectile entity handle、Item instance handle、
  Chest identity 和 business revision 不能因 Protocol exclusion 被删除。
- derived value 不重复保存；transient tick state、random work cursor、render/UI state 和 host
  file handle 不进入 business snapshot。
- unknown version、invalid value、duplicate identity、stale link、overflow 和 rollback 都必须
  有显式处理，不以默认值静默覆盖。
- `ServerRelevant` 未逐行关联 replacement evidence 时保持 `deferred`；不能由 registry、同名
  component 或窄 verifier 自动提升。

## 3. World persistence matrix

| Domain | Value state | Runtime owner | Persistence decision | Required edge cases | Status |
| --- | --- | --- | --- | --- | --- |
| World identity | `WorldId`、name | `WorldMetadata` | required value-only | missing/duplicate identity | `identity-preserved` |
| Dimensions/bounds | width/height/bounds | `WorldMetadata`/boundary policy | required or deterministic rebuild | invalid size/overflow | `partial` |
| Spawn/dungeon | world spawn、dungeon anchor | spawn/dungeon components | required or explicit rebuild | `-1/-1` unset, negative/invalid coordinates | `partial` |
| Seed/rules | seed、mode、secret variants | `WorldSeedComponent`/rule snapshot | required | version drift/unknown variant | `partial` |
| Clock/weather | day/time/rate/rain/wind authority | `WorldClock`/`WorldWeatherState` | save only business values | paused/rate/reset/derived flags | `partial` |
| Progression | boss/event/invasion/meteor flags/counters | `WorldProgressionState` | required where restart changes rules | duplicate event, reset, one-shot transition | `partial/deferred` |
| Tile/Wall/Liquid | value state and section/world versions | `WorldGrid`/liquid state | required by WLD/business format | sequence, extended state, rollback | `deferred` |
| WorldObjects | identity/anchor/content/text/link/revision | object-specific components | required by object kind | duplicate coordinates, stale link, destroy/restore | `partial/deferred` |
| Generation | seed/version/stage/checkpoint | generation snapshot | only restart/recovery values | random cursor/pass order/global mismatch | `deferred` |

## 4. Player persistence and layout matrix

| Layer | Exact shape | Owner | Persistence rule | Required checks |
| --- | --- | --- | --- | --- |
| Legacy source | `inventory: Item[59]`; logical 50 item + 4 coin + 4 ammo contract is separate | compatibility adapter | preserve source mapping only | physical vs logical index reconciliation |
| Runtime | `InventoryComponent` 40 slots | runtime inventory | runtime snapshot only | selected slot, empty/air, stack merge, overflow |
| Persistence | `PlayerPersistentState.ItemSlotCount=990` | persistent value state | versioned value-only | 990 mapping, unknown slots, duplicate/empty policy |
| Equipment | armor/dye/misc arrays and loadout | `EquipmentLoadoutComponent` | functional values; appearance split | stat order, functional/vanity, invalid item |
| Bank | `bank`-`bank4` separate Chest values | bank domain | profile value-only | independent kind/identity/capacity |
| Void Vault | `voidVaultInfo` and storage values | Void Vault domain | versioned value-only | disabled/empty/overflow/recovery |
| Trash | `trashItem` separate slot | trash domain | explicit product decision | destructive transfer and restore |
| Buff | 44 type/time entries and immune definition | Buff domain | mostly rebuild/transient | order, expiry, immunity, reset |
| Death/drop | cause, marker, pending world-item intent | lifecycle/drop domain | only if restart semantics require | exactly-once emission/rollback/reconnect |

禁止使用 58-slot alias、990-slot runtime alias或把 bank/Void/trash 合成 generic inventory。

## 5. Identity and stale-link matrix

| Entity | Stable business identity | Link(s) | On duplicate | On stale/missing link | Wire handling |
| --- | --- | --- | --- | --- | --- |
| World | `WorldId` | Player/object association | reject/fail closed | abort world restore | wire encoding excluded |
| Player | Player entity/profile handle | session, item, NPC target | reject/rebind by policy | detach session/domain link | packet slot excluded |
| NPC | NPC entity handle/`netID` semantics | target/player/projectile | reject or allocate new handle | clear target | encoded index excluded |
| Projectile | projectile entity handle/`identity` | owner/parent | reject or new entity | despawn/clear owner | replication identity excluded |
| Item | item instance handle | inventory/equipment/world owner | reject duplicate | quarantine/drop policy | visible slot excluded |
| Chest | Chest identity/index | opener/player/object | reject duplicate coordinate | close/clear opener | message index excluded |
| TileEntity | `ID` + anchor | linked entity/player | reject duplicate ID/anchor | invalidate link | message layout excluded |

## 6. Deletion-gate ledger work

- [ ] 打开 `docs/migrations/version4-physical-deletion-ledger.csv` 的 44 条 `ServerRelevant` 行，
  每行填 replacement owner、command/snapshot、source anchor、persistence boundary 和 evidence path。
- [ ] 4 条 `ReplacedWithEvidence` 只接受对应字段的证据；不能扩大到相邻声明。
- [ ] `ClientOnly`、Protocol、transport 和 presentation 行不因没有 Simulation owner 被标成
  server migration failure。
- [ ] 任何 field card 缺 default、write chain、lifetime、restore、invalid/rollback 或 stale-link
  行为时保持 `deferred`。
- [ ] 保持 `canRemoveLegacyWorldGen=false`；tile、extended state、metadata、command sequence、
  random checkpoint、restart/persistence boundary 全闭合前不开放物理删除。

## 7. Ordered actions

- [ ] 收集 P01-P09 领域字段卡，去掉 Protocol/replication-only rows。
- [ ] 对 World、Player、NPC、Projectile、Item、WorldObjects 逐字段确定保存/派生/临时/host。
- [ ] 对 identity、handle、revision、stale link、duplicate、overflow、unknown version 定义统一拒绝/恢复策略。
- [ ] 分开 business persistence 与 wire projection；不把协议字段写进任何 persistence owner。
- [ ] 逐条处理 44 条 `ServerRelevant`，未闭合行继续 deferred。
- [ ] 将本文件交给 P99；P11 可并行读取本文件的 owner/persistence boundaries。

## 8. Acceptance and handoff

- [ ] World/Player/Entity/WorldObjects 三套 persistence matrix 完成。
- [ ] 三套 Player inventory layout 的差异保留，无 alias。
- [ ] 业务 identity/revision 仍保留，wire identity/cursor 全部 excluded。
- [ ] 44 条 ServerRelevant 每条都有独立证据状态；没有静默关闭 deletion gate。
- [ ] `canRemoveLegacyWorldGen=false`、`partial`、deferred 和 `not-run` 保持不变。
- [ ] 只将本文件交给 P99，不直接修改 parent plan、ledger、CSV、JSON、源码或 manifest。

本任务不执行数据迁移、存档写入、测试、构建、verifier、regression 或删除操作。

## 9. Converged persistence matrix (P01-P09 evidence)

The following matrix is the P10 decision record.  A value is persisted only when it is a
business input to restart/recovery; runtime projections, protocol fields and host resources are
reconstructed.  Every row has an explicit invalid, duplicate, stale-link, overflow, unknown
version and rollback rule.  `partial` means the owner exists but an end-to-end save fixture or
format proof is still absent; `deferred` means the source/consumer chain is not closed.

| Domain | Persisted value-only record | Runtime owner / restore target | Identity and revision rule | Invalid, duplicate, stale, overflow, unknown-version and rollback policy | Status / evidence |
| --- | --- | --- | --- | --- | --- |
| World | `WorldId`, name, dimensions, bounds, seed/rules, clock/weather/progression values that alter restart rules | `WorldMetadata`, `WorldRuleState`, `WorldClock`, `WorldWeatherState`, `WorldProgressionState` | `WorldId` is required and unique; business revision is monotonic per committed snapshot, never a network revision | Reject missing/duplicate `WorldId`; reject non-positive dimensions, coordinate overflow, invalid mode/variant and non-finite weather; unknown save version fails closed and selects prior backup; restore is transactional and rolls back uncommitted sections | `partial` (`P01`, WLD compatibility and `P08` save/recovery anchors) |
| World grid | Versioned tile/wall/liquid sections and section sequence/checkpoint only when the WLD contract proves restart dependence | `WorldGrid`/liquid state import boundary | Section identity is `(WorldId, section coordinate, sequence)`; no packet cursor is persisted | Reject duplicate section/sequence, bounds overflow and unknown section version; stale sections are quarantined; restore aborts the snapshot and keeps the last committed world | `deferred` (P01/P07 explicitly leave WLD section proof open) |
| Player profile | Profile handle, spawn, functional equipment, value-only inventory/bank/Void/trash, business progression values with proven restart semantics | `PlayerLifecycleComponent`, inventory/equipment/bank/Void/trash components | Player handle is stable within profile; item instance handles and container identities remain unique; profile revision is monotonic | Reject duplicate profile/item/container identity; stale world links detach; slot/count overflow rejects the section (no truncation); unknown version quarantines profile; restore uses last committed profile and rolls back partial imports | `partial/deferred` (`P02` inventory/equipment matrix; `P08` recovery result) |
| NPC entity | Only business identity and restart-required state if a future NPC save contract proves it; tick AI/random cursors are transient | NPC identity/state components | NPC entity handle/`netID` semantics are business lookup values; encoded target index is excluded | Duplicate handles reject spawn/restore; stale target links clear; array/AI overflow rejects entity; unknown version skips entity with diagnostic; failed entity restore rolls back entity batch | `deferred` (`P03` AI and persistence chains remain open) |
| Projectile entity | Only lifecycle/owner/identity values if restart semantics require them; no replication snapshot/cadence/B248 fields | Projectile lifecycle/owner components | Projectile handle/`identity` and owner handle remain business values; replication identity/revision excluded | Duplicate handle rejects or allocates a new entity by domain policy; stale owner despawns/clears owner; invalid lifetime/position rejects; unknown version drops entity batch; rollback is despawn-before-publish | `deferred` (`P04`, P09 B248 exclusion) |
| World objects | Chest identity, anchor, content, access/link and business revision; Sign/TileEntity identity, anchor, content/text and revision | Chest/Sign/TileEntity components and typed restore mappers | Chest identity and TileEntity `ID` are unique; anchor is part of identity; business revision orders conflict/recovery | Duplicate identity or coordinate rejects; stale opener/link closes and clears association; capacity/text/slot overflow rejects object; unknown object version quarantines object; restore publishes only after validation and rolls back object batch | `partial/deferred` (`P06` identity/revision rows and restore policies) |
| Generation recovery | Seed, stage/version and explicit checkpoint only where restart/recovery requires it | `WorldGenerationLifecycleSnapshot`, `GenerationRandomState` | Checkpoint belongs to `(WorldId, generation version, stage)`; random cursor is scoped, not global | Reject stage/version mismatch, cursor overflow and duplicate checkpoint; unknown version restarts generation from a safe boundary; failed pass discards uncommitted commands and restores prior checkpoint | `deferred` (`P07` random/restart evidence incomplete) |

### 9.1 Shared restore and commit contract

1. Decode into an isolated value-only candidate and validate format version before touching the
   live domain.  No default value may silently replace a missing or invalid field.
2. Validate identity uniqueness, revision monotonicity, bounds/capacity and all links against the
   candidate index.  A stale link is detached or quarantined according to the row above; it is not
   rebound by array position or a wire slot.
3. Commit in deterministic domain order (`World` -> `Player` -> entities -> `WorldObjects`) and
   publish one new business revision.  Derived values and transient worklists are rebuilt after
   commit and are never written back into the snapshot.
4. On any validation or commit failure, discard the candidate, retain the last committed snapshot,
   and return a typed recovery result.  Retry may reread a known-good backup; it must not merge
   partially restored sections or advance revision.
5. Protocol packet layout, transport/session slots, replication cursors/cadence and B248 network
   scheduling are projections owned by P09/P08 and are excluded from every persistence record.

## 10. ServerRelevant deletion-gate cross-check (44/44)

The ledger was read directly at `docs/migrations/version4-physical-deletion-ledger.csv` on
2026-09-01.  All 44 rows below have the same result: the removed Version3 declaration has no
field-level replacement owner, command/commit chain, persistence consequence and focused
verifier in the current evidence set.  They therefore remain `deferred`; no row is promoted by
name similarity, registry membership or a narrow world-generation verifier.

| # | Ledger `RelativePath` | Replacement evidence | P10 decision |
| ---: | --- | --- | --- |
| 1 | `Terraria.GameContent.Biomes.CaveHouse/DesertHouseBuilder.cs` | none (ledger fields all `DEFERRED`) | `deferred` |
| 2 | `Terraria.GameContent.Biomes.CaveHouse/GraniteHouseBuilder.cs` | none | `deferred` |
| 3 | `Terraria.GameContent.Biomes.CaveHouse/HouseBuilder.cs` | none | `deferred` |
| 4 | `Terraria.GameContent.Biomes.CaveHouse/IceHouseBuilder.cs` | none | `deferred` |
| 5 | `Terraria.GameContent.Biomes.CaveHouse/JungleHouseBuilder.cs` | none | `deferred` |
| 6 | `Terraria.GameContent.Biomes.CaveHouse/MarbleHouseBuilder.cs` | none | `deferred` |
| 7 | `Terraria.GameContent.Biomes.CaveHouse/MushroomHouseBuilder.cs` | none | `deferred` |
| 8 | `Terraria.GameContent.Biomes.CaveHouse/WoodHouseBuilder.cs` | none | `deferred` |
| 9 | `Terraria.GameContent.Biomes.Desert/DesertDescription.cs` | none | `deferred` |
| 10 | `Terraria.GameContent.Biomes.Desert/SurfaceMap.cs` | none | `deferred` |
| 11 | `Terraria.GameContent.Biomes/CorruptionPitBiome.cs` | none | `deferred` |
| 12 | `Terraria.GameContent.Biomes/SpikePitBiome.cs` | none | `deferred` |
| 13 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonDropTrap.cs` | none | `deferred` |
| 14 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonDropTrapSettings.cs` | none | `deferred` |
| 15 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonPillar.cs` | none | `deferred` |
| 16 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonPillarSettings.cs` | none | `deferred` |
| 17 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonPitTrap.cs` | none | `deferred` |
| 18 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonPitTrapSettings.cs` | none | `deferred` |
| 19 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonTileClump.cs` | none | `deferred` |
| 20 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonTileClumpSettings.cs` | none | `deferred` |
| 21 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonWindow.cs` | none | `deferred` |
| 22 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonWindowBasic.cs` | none | `deferred` |
| 23 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonWindowBasicSettings.cs` | none | `deferred` |
| 24 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonWindowMosaic.cs` | none | `deferred` |
| 25 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonWindowMosaicSettings.cs` | none | `deferred` |
| 26 | `Terraria.GameContent.Generation.Dungeon.Features/DungeonWindowSettings.cs` | none | `deferred` |
| 27 | `Terraria.GameContent.Generation.Dungeon.Rooms/BiomeDungeonRoomSettings.cs` | none | `deferred` |
| 28 | `Terraria.GameContent.Generation.Dungeon.Rooms/LivingTreeDungeonRoomSettings.cs` | none | `deferred` |
| 29 | `Terraria.GameContent.Generation.Dungeon.Rooms/RegularDungeonRoomSettings.cs` | none | `deferred` |
| 30 | `Terraria.GameContent.Generation.Dungeon.Rooms/WormlikeDungeonRoomSettings.cs` | none | `deferred` |
| 31 | `Terraria.GameContent.Generation.Dungeon/DungeonShapes.cs` | none | `deferred` |
| 32 | `Terraria.GameContent.Generation/ActionGrass.cs` | none | `deferred` |
| 33 | `Terraria.GameContent.Generation/ActionPlaceStatue.cs` | none | `deferred` |
| 34 | `Terraria.GameContent.Generation/ActionStalagtite.cs` | none | `deferred` |
| 35 | `Terraria.GameContent.Generation/ActionVines.cs` | none | `deferred` |
| 36 | `Terraria.GameContent.Generation/PassLegacy.cs` | none | `deferred` |
| 37 | `Terraria.GameContent.Generation/ShapeBranch.cs` | none | `deferred` |
| 38 | `Terraria.GameContent.Generation/ShapeRoot.cs` | none | `deferred` |
| 39 | `Terraria.GameContent.Generation/ShapeRunner.cs` | none | `deferred` |
| 40 | `Terraria.GameContent/FlexibleTileWand.cs` | none | `deferred` |
| 41 | `Terraria.GameContent/MinecartDiggerHelper.cs` | none | `deferred` |
| 42 | `Terraria.GameContent/ShimmerHelper.cs` | none | `deferred` |
| 43 | `Terraria.WorldBuilding/SimpleStructure.cs` | none | `deferred` |
| 44 | `Terraria.WorldBuilding/TileFont.cs` | none | `deferred` |

The deletion gate remains closed: `canRemoveLegacyWorldGen=false`.  This P10 document does not
edit the ledger or any source file; P99 is the only task permitted to merge these findings into
the parent migration plan.

## 11. Verification and handoff

- Ledger denominator check: PowerShell `Import-Csv` reports exactly 44 rows where
  `Classification == ServerRelevant`; all 44 have `Status == deferred`.
- P01-P09 owner-table review: no Protocol/Transport/Replication/wire owner is accepted as a
  business persistence owner; WorldId, Player/entity handles, Chest identity/content and business
  revision remain explicitly retained.
- No build, test, verifier, migration, deletion or parent-plan edit was performed (documentation
  scope required by this task).
- Handoff: P11 may consume the matrices above; P99 must review this file and remains the sole
  writer of `docs/plans/2026-08-31-server-required-field-property-migration-plan.md`.
