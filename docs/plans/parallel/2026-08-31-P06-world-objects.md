# P06 WorldObjects Declaration Inventory Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将 Chest、Sign、TileEntity、Door、Wiring/Actuator 和 TrainingDummy 的业务字段拆成
逐项 inventory，并把 legacy scratch、message layout 与服务器权威状态分开。

**Architecture:** WorldObjects 以 identity/anchor/lifecycle/content/permission/revision 为
核心。对象交互只产生 validated business command，Tile/Wiring/Liquid 的提交保持确定性；
静态索引和遍历 workspace 不直接复制为全局 ECS singleton。

**Tech Stack:** Documentation-only Markdown; no C# changes, no new functions, no tests/builds.

---

## 1. Task identity and write set

| 项目 | 内容 |
| --- | --- |
| Task | `P06` |
| Parent plan | `docs/plans/2026-08-31-server-required-field-property-migration-plan.md` |
| Dependency | `P00` |
| Wave | `1`，可与 P01-P05、P07-P09 并行 |
| Read set | Chest/Sign/TileEntity/TETrainingDummy/Wiring/Liquid source、member coverage、WorldObjects sections |
| Write set | 仅本文件 |
| Status | `[ ] not-run` |

## 2. Source snapshots

使用当前 checkout 中的 instrumented oracle，不复用未核实的其他 source hash：

```text
Terraria/Chest.cs                 SHA256 A19E6A6811315B4CD03E6C7EE72666E8D4356851EBAFE915220737B8870DC1C7
Terraria/Sign.cs                  SHA256 AA8B1D36046BF06E1D0ACCF21B75867A591BFE2D02AF3CEA9C58425796A1947A
Terraria/Wiring.cs                SHA256 704BFD8029E29FCF64BFDF4150A14D599D7489D3F58462F485B47A557BDF3FDE
Terraria.DataStructures/TileEntity.cs SHA256 BB9D1895629228A7DE51513BAE36A0B0D92D0F0BA8F6E4413133D3D1EE750D0F
Terraria.GameContent.Tile_Entities/TETrainingDummy.cs SHA256 88088FFA7B24A1F6D998E471C20C662A98C642EEDE04B53B720CC4964B287F21
```

若 manifest 使用另一份 legacy source hash，必须在 field card 中同时写 `source snapshot`
和 `manifest reference`，不能静默覆盖来源。

## 3. Chest declaration and business-state inventory

| ID | Legacy/business field | Source anchor | Exact shape/default | Owner candidate | Persistence | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `P06-C-001` | `identity`/array index | `Chest.cs:69` `index` | legacy `readonly int`; constructor index default `0` | `ChestIdentityComponent` | required | `identity-preserved` |
| `P06-C-002` | `index/handle` | `Chest.cs:69`, create methods | `int` handle; allocated by store | `ChestStore` | association required | `partial` |
| `P06-C-003` | `x` | `Chest.cs:67` | `readonly int`; constructor default `0` | `ChestComponent.TileX` | required | `partial` |
| `P06-C-004` | `y` | `Chest.cs:69 vicinity` | `readonly int`; constructor default `0` | `ChestComponent.TileY` | required | `partial` |
| `P06-C-005` | `active` | create/destroy consumers | derived `bool`; no standalone legacy field | `ChestLifecycleComponent` | rebuild/record destroy | `partial` |
| `P06-C-006` | `valid` | `Chest.cs:309-330`, tile validation consumers | derived `bool` | `ChestValidationQuery` | derived | `deferred` |
| `P06-C-007` | `maxItems` | `Chest.cs:61`, constructor `:206` | `int`; constructor default `40` | `ChestCapacityDefinition` | definition/value | `partial` |
| `P06-C-008` | `contents`/`item` | `Chest.cs:65` | `Item[]`; allocated by capacity | `ChestInventoryComponent` | required value-only | `partial` |
| `P06-C-009` | content slot index | `Chest.cs:65` item access | `int`; valid range `0..maxItems-1` | `ChestInventoryComponent` | value-only | `deferred` |
| `P06-C-010` | `owner`/opener | `Chest.cs:309-348`, `_chestInUse` consumers | target `PlayerHandle?`; no scalar legacy field | `ChestAccessComponent` | transient/rebuild | `deferred` |
| `P06-C-011` | `lock` | `Chest.cs:309-348` lock methods | target `bool`; no standalone field | `ChestLockState` | business value if enabled | `deferred` |
| `P06-C-012` | open state | `Chest.cs:400-470` open consumers | target `PlayerHandle?`/state enum; exact type to freeze | `ChestAccessComponent` | transient | `deferred` |
| `P06-C-013` | `revision` | mutation/persistence boundary | target monotonic business revision; not network revision | `ChestRevisionComponent` | required for conflict/recovery | `partial` |
| `P06-C-014` | `name` | `Chest.cs:75` | `string`; null/empty constructor behavior to record | `ChestStateComponent.Name` | required if business name persists | `partial` |
| `P06-C-015` | `bankChest`/ChestKind | `Chest.cs:71` | `bool`; constructor parameter default `false` | `ChestKindDefinition` | value/derived | `partial` |
| `P06-C-016` | empty slot policy | `Chest.cs:65`, fill/repair methods | `Item` air invariant | `ChestInventoryPolicy` | rule | `partial` |
| `P06-C-017` | duplicate policy | `FixLoadedData`/restore consumers | decision enum; no legacy scalar | `ChestRestorePolicy` | restore contract | `deferred` |
| `P06-C-018` | overflow policy | `CreateOutOfArray`/Resize consumers | decision enum; no legacy scalar | `ChestCapacityPolicy` | restore/import contract | `deferred` |
| `P06-C-019` | restore behavior | `Chest.cs:FixLoadedData` | value-only import result | `ChestRestoreSystem` | required | `partial` |
| `P06-C-020` | destroy behavior | `Chest.cs:CanDestroyChest`/`DestroyChest` | typed destroy command/result | `ChestMutationCommitSystem` | tombstone/contents policy | `partial` |
| `P06-C-021` | stale link behavior | opener/linked item/entity consumers | explicit invalidation transition | `ChestAccessSystem` | transient/rebuild | `deferred` |
| `P06-C-022` | `frameCounter`/`frame` | `Chest.cs:77-79` | `int`; implicit `0` | no Simulation owner unless collision rule consumes | none/presentation | `excluded` |
| `P06-C-023` | `eatingAnimationTime` | `Chest.cs:81` | `int`; implicit `0` | no Simulation owner | none/presentation | `excluded` |

`Chest.item` 的 slot 内容必须使用 Item value-only representation；不得直接复用 Player
inventory layout，也不得将 Chest `revision` 改写成网络 revision。

## 4. Sign declaration and business-state inventory

| ID | Legacy/business field | Source anchor | Exact shape/default | Owner candidate | Persistence | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `P06-S-001` | `identity`/array handle | `Sign.cs:3-5`, `ReadSign` | target `SignHandle`; legacy array index only | `SignIdentityComponent` | association required | `partial` |
| `P06-S-002` | `x` | `Sign.cs:7` | `int`; assigned from tile anchor | `SignComponent.TileX` | required | `partial` |
| `P06-S-003` | `y` | `Sign.cs:9` | `int`; assigned from tile anchor | `SignComponent.TileY` | required | `partial` |
| `P06-S-004` | `active`/deleted | `Sign.cs:13-29`, `TextSign` | derived `bool`; array/tile validity | `SignLifecycleComponent` | delete/tombstone | `partial` |
| `P06-S-005` | `text` | `Sign.cs:11`, `TextSign` | `string`; creation default `""` | `SignComponent.Text` | required value-only | `partial` |
| `P06-S-006` | text default | `Sign.cs:49-53` | `string.Empty` on create | `SignRestorePolicy` | required | `partial` |
| `P06-S-007` | text length rule | `Sign.cs:67-78`, persistence consumer | bounded string policy; exact limit to freeze | `SignValidationPolicy` | rule | `deferred` |
| `P06-S-008` | edit authority | `TextSign` caller/interaction path | target `PlayerHandle` + permission result | `SignAuthorizationPolicy` | transient | `deferred` |
| `P06-S-009` | `revision` | current Sign mutation boundary | target monotonic business revision | `SignRevisionComponent` | required | `deferred` |
| `P06-S-010` | reload behavior | `ReadSign` | value-only restore/rebind | `SignRestoreSystem` | required | `partial` |
| `P06-S-011` | delete behavior | `KillSign` | typed delete command; stale anchor invalidates sign | `SignMutationCommitSystem` | tombstone/rebuild | `partial` |
| `P06-S-012` | frame/attachment geometry | `ReadSign` tile frame reads | `TileReadSnapshot` query values | `SignValidationQuery` | derived | `partial` |

`Sign.text` 是业务内容；任何将文本放入 message/packet layout 的表示都归 P09 `excluded`。
字体、颜色、滚动、UI selection 和 display cache 不建立 Simulation owner。

## 5. TileEntity declaration and lifecycle inventory

| ID | Legacy declaration | Source anchor | Exact type/default | Owner candidate | Persistence | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `P06-TE-001` | `manager` | `TileEntity.cs:13` | static `TileEntitiesManager`; implicit null | `TileEntityDefinitionRegistry`/host composition | host/reference | `host-only` |
| `P06-TE-002` | `EntityCreationLock` | `TileEntity.cs:17` | static `object`; new lock object | no Simulation owner | none/host | `excluded` |
| `P06-TE-003` | `UpdateEntities` | `TileEntity.cs:19` | static `List<TileEntity>`; new list | bounded update query/scheduler | transient | `deferred` |
| `P06-TE-004` | `ByID` | `TileEntity.cs:21` | static `Dictionary<int, TileEntity>`; new dictionary | `TileEntityStore` index | rebuild | `partial` |
| `P06-TE-005` | `ByPosition` | `TileEntity.cs:23` | static `Dictionary<Point16, TileEntity>`; new dictionary | `TileEntitySpatialIndex` | rebuild | `partial` |
| `P06-TE-006` | `TileEntitiesNextID` | `TileEntity.cs:25` | static `int`; implicit `0` | bounded entity allocator | restore/restart policy | `partial` |
| `P06-TE-007` | `ID` | `TileEntity.cs:27` | instance `int`; assigned new ID | `TileEntityIdentityComponent` | required | `identity-preserved` |
| `P06-TE-008` | `Position` | `TileEntity.cs:29` | `Point16`; default zero | `TileEntityAnchorComponent` | required | `partial` |
| `P06-TE-009` | `type` | `TileEntity.cs:31` | `byte`; assigned from type | `TileEntityDefinition` reference | required | `partial` |
| `P06-TE-010` | `RequiresUpdates` | `TileEntity.cs:33` | `bool`; implicit `false` | `TileEntityLifecycleComponent` | value/definition | `partial` |
| `P06-TE-011` | `active`/`valid` | `IsTileValidForEntity` consumers | derived `bool`; no base scalar | `TileEntityValidationQuery` | rebuild | `deferred` |
| `P06-TE-012` | linked entity | derived from entity-specific classes | typed `EntityHandle?` | `TileEntityLinkComponent` | value-only handle | `deferred` |
| `P06-TE-013` | owner/lock | interaction consumers | target `PlayerHandle?`/`bool` | `TileEntityAccessComponent` | transient/business decision | `deferred` |
| `P06-TE-014` | persistent value schema | derived type-specific payload | versioned value-only record | entity-specific persistence mapper | required | `deferred` |
| `P06-TE-015` | `revision` | current object mutation boundary | target monotonic business revision | `TileEntityRevisionComponent` | required | `deferred` |
| `P06-TE-016` | placement | `TileEntity.cs:115-126` | typed placement command | `TileEntityPlacementSystem` | event/record | `partial` |
| `P06-TE-017` | destroy | `TileEntity.cs:128-163` | typed destroy command | `TileEntityMutationCommitSystem` | tombstone/rebuild | `partial` |
| `P06-TE-018` | cross-entity stale link | entity update/removal consumers | invalidation transition | `TileEntityLinkSystem` | rebuild | `deferred` |

`manager`、lock、static indexes 和 update list 不得作为单个 `TileEntityComponent` 的可变
业务字段；它们应拆成 registry/store/scheduler 或 host boundary。

## 6. Door, mechanism wiring and actuator inventory

这里的 `wire` 指世界机关 Wiring 拓扑，不指 Protocol/transport wire。所有网络含义由 P09
排除审计处理。

| ID | Business field | Source anchor | Exact shape/default | Owner candidate | Persistence | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `P06-M-001` | door anchor | `Wiring.cs:1490-1530` door consumers | target `Point16`/bounded coordinate | `DoorStateComponent` | required | `partial` |
| `P06-M-002` | door open/closed | `Wiring.cs:1490-1530` | target `bool`/state enum; default closed | `DoorStateComponent` | required value | `deferred` |
| `P06-M-003` | door locked | door interaction path | target `bool`; default false | `DoorAuthorityPolicy` | business value if supported | `deferred` |
| `P06-M-004` | door permission | door/player interaction path | target permission result/PlayerHandle | `DoorAuthorizationPolicy` | transient | `deferred` |
| `P06-M-005` | mechanism authority | `Wiring.cs:405-435` actuator path | validated actor/source context | `MechanismActivationSystem` | transient | `partial` |
| `P06-M-006` | mutation revision | Tile mutation boundary | target monotonic business revision | `DoorStateComponent`/Tile commit | required | `deferred` |
| `P06-M-007` | wiring topology | `Wiring.cs:19-29`, `HitWire` | Tile coordinate + color mask; exact type to freeze | `WireNetworkComponent` | world value/rebuild | `partial` |
| `P06-M-008` | wire enabled/disabled | `Tile.wire/wire2/wire3/wire4` consumers | per-color `bool` derived from tile state | `WireTopologyQuery` | derived from Tile | `partial` |
| `P06-M-009` | actuator state | `Wiring.cs:405-435`, `HitWireSingle` | target `bool`; default inactive | `ActuatorStateComponent` | world value | `partial` |
| `P06-M-010` | bounded traversal | `Wiring.cs:871-1013` | bounded typed worklist | `WireTraversalSystem` | transient | `partial` |
| `P06-M-011` | retry policy | `Wiring.cs:871-1013`, liquid handoff | bounded retry/abort enum | `MechanismRetryPolicy` | transient | `deferred` |
| `P06-M-012` | mutation sequence | Tile/Wiring commit boundary | monotonic business sequence | `TileChangeCommitSystem` | required for recovery | `partial` |
| `P06-M-013` | commit ordering | `Actuate`/door/liquid consumers | explicit order record | `MechanismCommitCoordinator` | replay metadata/value | `deferred` |
| `P06-M-014` | rollback behavior | invalid/over-budget traversal | typed rejected result, no partial mutation | `MechanismCommitCoordinator` | transient | `deferred` |
| `P06-M-015` | pump/liquid transfer | `Wiring.cs:499-549` | `LiquidTransferCommand` value-only | Liquid command owner | world Tile/Liquid value | `partial` |
| `P06-M-016` | pressure/switch activation | `Wiring.cs:551-693` | typed activation command | `MechanismActivationSystem` | transient/value as needed | `deferred` |

禁止把 `_wireList`、`_wireDirectionList`、`_wireSkip`、`_toProcess`、`_GatesCurrent`、
`_GatesNext`、`_GatesDone`、`_PixelBoxTriggers`、pump arrays、`_mechX/Y/Time` 或
`_teleport` 直接复制为一个静态 `WireNetworkComponent`。它们是 legacy workspace；只能被
重构为 bounded per-command/per-tick worklist。

## 7. TrainingDummy declaration and lifecycle inventory

| ID | Legacy/business field | Source anchor | Exact type/default | Owner candidate | Persistence | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `P06-TD-001` | entity identity | `TETrainingDummy.cs:6-13` + TileEntity base | inherited `int ID`; assigned allocator | `TrainingDummyIdentityComponent` | required | `identity-preserved` |
| `P06-TD-002` | anchor | `TETrainingDummy.cs:35-76` + base Position | `Point16`; inherited | `TrainingDummyStateComponent` | required | `partial` |
| `P06-TD-003` | `npc` link | `TETrainingDummy.cs:13`, constructor `:106-110` | `int`; default `-1` | `TrainingDummyLinkComponent` | value-only handle/rebuild | `partial` |
| `P06-TD-004` | `activationRetryCooldown` | `TETrainingDummy.cs:15` | `int`; implicit `0` | `TrainingDummyActivationState` | transient | `deferred` |
| `P06-TD-005` | active/valid | `TETrainingDummy.cs:35-76`, `ValidTile` | derived `bool` | `TrainingDummyLifecycleComponent` | rebuild | `partial` |
| `P06-TD-006` | owner/lock | interaction path | target `PlayerHandle?`/`bool`; no legacy scalar | `TrainingDummyAccessComponent` | transient/business decision | `deferred` |
| `P06-TD-007` | hitbox | `TETrainingDummy.cs:51-68` | `Rectangle`/typed bounds; server geometry only | `TrainingDummyCollisionState` | derived | `partial` |
| `P06-TD-008` | damage state | NPC/player hit interaction | typed bounded damage/activation state | `TrainingDummyDamageSystem` | transient/value decision | `deferred` |
| `P06-TD-009` | activation/deactivation | `TETrainingDummy.cs:35-76` and Activate/Deactivate methods | typed transition | `TrainingDummyActivationSystem` | event/rebuild | `partial` |
| `P06-TD-010` | revision | object mutation boundary | target monotonic business revision | `TrainingDummyRevisionComponent` | required | `deferred` |
| `P06-TD-011` | persistence | TileEntity restore path | versioned value-only record | `TrainingDummyPersistenceMapper` | required | `deferred` |
| `P06-TD-012` | stale NPC link | `Update` lines checking active/type/ai | link invalidation transition | `TrainingDummyLinkSystem` | rebuild | `partial` |
| `P06-TD-013` | `playerBoxes` | `TETrainingDummy.cs:8` | static `List<Rectangle>`; new list | query-local workspace | none | `excluded` |
| `P06-TD-014` | `playerBoxFilled` | `TETrainingDummy.cs:10` | static `bool`; false | per-tick query state | none | `excluded` |
| `P06-TD-015` | `npcSlotsFull` | `TETrainingDummy.cs:12` | static `bool`; false | bounded query result, not global state | none | `excluded` |

message 87、request/response 字段、packet/entity placement layout 只能进入 P09 的
`excluded` matrix；不能把 `npc` link 或 activation state 误写成网络字段。

## 8. Ordered actions

- [ ] 对 Chest/Sign/TileEntity/TrainingDummy 的每个 legacy declaration 记录 exact type/default。
- [ ] 对没有直接 legacy scalar 的 `active`、`valid`、owner、lock、revision、permission、
  stale link 建立明确的 target type decision 行，不用模糊的 `data`。
- [ ] 将 Chest contents、Sign text、TileEntity value schema 和 TrainingDummy link 分开保存。
- [ ] 为 Door/Wiring/Actuator 固化 anchor、state、permission、traversal budget、retry、
  mutation sequence 和 commit order。
- [ ] 将 legacy static indexes/queues/worklists 与业务 entity state 分离。
- [ ] 将 message/request/response/packet layout 全部交给 P09，不能在本文件建立网络 owner。
- [ ] 将每个对象的 stale link、duplicate、overflow、destroy、restore 和 rollback 写进字段卡。

## 9. Acceptance and handoff

- [ ] Chest 至少 23 个业务/排除行、Sign 至少 12 行、TileEntity 至少 18 行、Door/Wiring
  至少 16 行、TrainingDummy 至少 15 行都有独立字段卡。
- [ ] 所有对象保留业务 identity/anchor/content/text/link/permission/revision 语义。
- [ ] legacy static manager/index/scratch 不被复制成单一杂项组件。
- [ ] P09 可据此逐项排除 message 87、request/response、packet/open/update/delete layout。
- [ ] 只将本文件交给 P10/P11/P99，不直接修改 parent plan、源码、CSV、JSON 或 manifest。

本任务不宣称 Containers/Signs/TileEntities/Wiring full parity；未执行测试、构建、verifier、
regression 或文档 gate。
