# P11 Component, System, and Command Seam Convergence Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 在各领域字段卡完成后，汇总唯一 owner、read/write set、typed command、snapshot
和 deterministic commit 边界，消除跨域直接写入和重复状态。

**Architecture:** 各领域保持独立 Component 和 System；跨域只通过 immutable query/snapshot、
validated business command 和固定 commit sequence 连接。P11 不创建代码、不替换字段，
只把 P01-P09 的候选 owner 组合成可执行的 seam contract。

**Tech Stack:** Documentation-only Markdown; no C# changes, no new functions, no tests/builds.

---

## 1. Task identity and write set

| 项目 | 内容 |
| --- | --- |
| Task | `P11` |
| Parent plan | `docs/plans/2026-08-31-server-required-field-property-migration-plan.md` |
| Dependency | P01-P09 work稿完成；可与 P10 并行 |
| Wave | `2` |
| Read set | P00-P09 work稿、parent plan 第 5-6、14-15 节、现有 domain contracts |
| Write set | 仅本文件 |
| Status | `[x] completed_partial` (documentation-only seam audit; unresolved domain evidence remains `partial/deferred`) |

## 2. Convergence contract

每个字段/属性在进入 seam matrix 前都必须能回答：

- 唯一可写 owner 是哪个 Component/Definition/host；
- 哪些 System 读取，哪些 Command 写入；
- 跨 tick、entity、world、restart、session 的生命周期；
- command 在哪个 tick phase 被接受、排序、验证和 commit；
- invalid、duplicate、retry、overflow、stale link、destroy 和 rollback 如何处理；
- snapshot 保存的是 business value、derived result、transient cursor 还是 host state；
- 是否存在一个同名的 Protocol/replication/wire projection，若存在如何明确排除。

## 3. Cross-domain owner matrix

| Domain | Primary components | Queries/snapshots | Commands/commit | Forbidden overlap |
| --- | --- | --- | --- | --- |
| World | `WorldMetadata`、`WorldRuleState`、`WorldClock`、`WorldWeatherState`、`WorldProgressionState` | World rule/weather/progression snapshot | world rule/clock/progression commit | client visual state、network frame |
| Player | identity/authority/lifecycle/transform/movement/inventory/equipment/Buff/environment/death-drop | authorized input、Player business snapshot | movement/inventory/equip/use/death/respawn commands | raw device state、wire slot |
| NPC | definition/lifecycle/target/home/behavior/combat/loot | target/progression/behavior snapshot | spawn/AI/hit/death/loot commands | raw AI array、encoded target |
| Projectile | definition/transform/owner/collision/combat/behavior/lifetime/cooldown | behavior/lifecycle snapshot | spawn/move/hit/despawn commands | net update/cadence snapshot |
| Item | definition/instance/use/equipment/recovery/placement/world-item/economy | item/inventory/world-item value snapshot | transfer/use/equip/drop/pickup/merge commands | client catalog/tooltip/wire slot |
| WorldObjects | Chest/Sign/TileEntity/Door/Wire/Actuator/TrainingDummy components | object value/anchor/revision snapshot | object/mechanism/tile/liquid commands | request/response/message layout |
| Server host | session/bootstrap/save/launch/command ingress | value-only import/bootstrap result | authorized ingress and persistence request | file handle/thread/raw protocol |

## 4. Required read/write/commit matrix

P11 必须为每一条跨域 command 建立如下矩阵；示例不是完成证据：

| Command family | Read set | Write set | Validation | Commit order | Retry/rollback | Persistence output | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Player movement | Player transform/movement、WorldGrid、World rules | Player transform/movement、Tile side effect command | finite values、authority、bounds | input -> collision -> transform -> side effect | reject without mutation | transform if required | `partial` |
| Item transfer | Player inventory、Item definition/instance、Chest/object access | source/destination slots、revisions、ownership | range、permission、stack compatibility | validate -> reserve -> merge/move -> revision | release reservation/no partial move | value-only inventory/object state | `partial` |
| NPC damage/death | NPC combat/lifecycle、Player/Projectile source、World progression | life、death marker、loot intent、world item command | source authority、target active、damage bounds | hit -> life -> death -> loot -> despawn | exactly-once marker/rollback | progression/loot if required | `partial` |
| Projectile spawn | owner、definition、spawn geometry、capacity | Projectile store/definition/lifecycle/owner | finite values、owner active、capacity | validate -> allocate -> initialize | no allocated residue | only explicit business state | `partial` |
| Tile mutation | immutable TileReadSnapshot、actor authority、definition | TileChangeCommand and section/business revision | bounds、sequence、rule | validate -> command sort -> tile commit -> revision | reject whole command | tile/section value | `partial` |
| Wiring/mechanism | topology、anchor、actor、bounded worklist | actuator/door/tile/liquid command | range、budget、dedup | traverse -> output commands -> deterministic commit | abort without partial mutation | mechanism value if required | `partial/deferred` |
| World save | world/entity/object business snapshots | host save state only | version/identity/completeness | collect -> serialize value -> atomic replace | recover/backup | business persistence | `partial` |

The rows above are the seam contracts for this work paper, rather than implementation claims.
`partial` means that the boundary and failure policy are defined but an upstream field card has
not closed its source-body, restore, or behavior evidence. `deferred` means the command shape is
known but the required authoritative consumer or ordering evidence is missing.

### 4.1 Executed seam matrix (P01-P10 evidence)

| Seam | Read boundary | Write/commit owner | Validation and ordering | Retry/rollback and persistence | Evidence/status |
| --- | --- | --- | --- | --- | --- |
| World rule/clock/weather -> Player/NPC | immutable `WorldRuleState`, `WorldClock`, `WorldWeatherState` query | World owners commit source state; consumers emit typed intents | authority and finite values; world phase precedes entity phase | reject intent without source mutation; persist business rule/clock values only | P01 cards `P01-M-012..017`; `partial` |
| Player input -> movement/tile side effect | normalized authority context, Player movement, WorldGrid/rules snapshot | Player movement commit; `TileChangeCommand` owns tile mutation | session authority, finite coordinates, bounds; input order then collision then side effect | no partial transform on reject; tile command is all-or-nothing; transform persistence remains conditional | P02 timer/array boundaries, P06 tile rows; `partial` |
| Player inventory <-> Item/Chest | inventory value snapshot, Item definition/instance, Chest identity/content/revision | inventory transfer command; source/destination container owners commit | permission, stable handles, expected revisions, stack/capacity; reserve then merge/move then revisions | release reservation and leave both containers unchanged on failure; value-only persistence | P05 instance/inventory rows, P06 chest rows, P09 §9.3; `partial` |
| Player/Projectile -> NPC damage/death/loot | source authority, NPC lifecycle/combat, projectile owner/definition, progression query | NPC combat/lifecycle owner; loot and world-item commands are separate commits | active target, bounded damage, source handle; hit -> life -> exactly-once death -> loot/despawn | duplicate death marker is idempotent; rollback uncommitted loot intent; persist only restart-relevant progression/loot | P03 family rows and P04 owner boundary; `partial/deferred` |
| Projectile spawn/move -> World/NPC | owner handle, immutable definition, spawn geometry, capacity and collision query | Projectile store/lifecycle owner; hit emits target command | owner active, finite geometry, capacity; validate -> allocate -> initialize; movement before hit | allocation has no residue on failure; despawn clears owner link; business identity/revision retained, replication excluded | P04 accepted-narrow cards; B248 excluded by P09 §9.4; `partial` |
| WorldGen pass -> Tile/Liquid/Structure | `WorldGenerationPassState`, seed/rule snapshot, immutable tile read snapshot | typed generation command batch and one tile/liquid commit coordinator | section version, bounds, pass sequence, deterministic random checkpoint; sort then commit | stale section rejects whole batch; resume from checkpoint; uncommitted commands discarded | P07 §11 and deletion gate; `partial/deferred` |
| Wiring/Actuator -> Tile/Liquid/Object | topology/anchor snapshot, actor authority, bounded worklist | mechanism command coordinator; Tile/Liquid owners commit values | range, budget, dedup and sequence; traverse -> emit commands -> deterministic commit | abort leaves no partial mutation; bounded retry only; mechanism persistence remains conditional | P06 `P06-M-011..016`; `partial/deferred` |
| Entity/Object restore -> links | versioned business snapshot, stable identity, anchors and revisions | domain restore mapper per container; no cross-container direct write | known version, unique identity/anchor, expected revision; world -> object -> entity link order | duplicate rejects/quarantines; stale links clear or defer by policy; overflow is explicit, never truncation | P10 §§3-5; `partial/deferred` |
| Server ingress -> Simulation command | host `ServerSessionState` normalized authority and bounded value-only command | `ServerCommandIngress` accepts; domain command owner commits at tick boundary | session permission, target handle, tick/sequence and capacity; authenticate -> enqueue -> sort -> validate -> commit | retry uses idempotency key/business revision; disconnect rolls back pending intents; host resources never persist | P08 host boundary; P09 §9.2; `host-only/partial` |
| World/entity/object save | business snapshots only; derived/transient/host fields excluded | `WorldSaveCoordinator` serializes and atomically replaces | schema version, completeness, unique identity and revision consistency | backup/recover previous valid snapshot; unknown version rejects or compatibility-maps explicitly | P10 §§2-3; `partial/deferred` |

## 5. Field ownership conflict checks

- [x] `WorldMetadata`、`WorldRuleState`、`WorldClock`、`WorldWeatherState` 和
  `WorldProgressionState` 不互相保存同一可写值；derived query 不能反向写回源状态。
- [x] Player inventory、equipment、bank、Void Vault、trash、Buff 和 world item store 的
  Item instance ownership 有唯一写入者；跨容器移动必须经过 command。
- [x] NPC target handle、Projectile owner handle、Player handle 和 Chest opener 都是业务
  handle；encoded slot/index 由 P09 excluded，不进入 owner matrix。
- [x] NPC global progression query 不复制到每个 entity；NPC-local phase state 不写入 World
  progression，除非明确 command/event contract。
- [x] Projectile `ai/localAI` family state 与 lifecycle/combat/cooldown 分开；raw arrays和
  B248 scheduling 不作为公共 owner。
- [x] Chest/Sign/TileEntity/Door/Wiring/TrainingDummy 的 identity、anchor、content/link、
  permission 和 revision 之间没有第二个隐式 static owner。
- [x] WorldGen read snapshot、pass random state、Tile/Liquid/Structure command 和 commit
  sequence 不直接引用 legacy global mutable arrays。
- [x] Server host session/permission/timeout 不被复制为 Simulation entity state；Simulation
  只接收 normalized authority context。

## 6. Component design rules

| Rule | Required result |
| --- | --- |
| Definition vs instance | immutable type/rule 与 mutable entity/item value 分离 |
| State vs query | 可变源状态由一个 owner 保存；派生值由 query 计算 |
| Command boundary | command 携带 value-only input、authority、tick/sequence 和 bounded target |
| Snapshot boundary | 保存值、重建值、临时值和 host 值分栏，不保存 object/closure/reference |
| Commit ownership | 每个容器只有一个 commit owner；系统不能直接修改其他系统容器 |
| Capacity | 所有数组/store/worklist 有明确 capacity、overflow、retry、fail-closed |
| Identity | business identity/revision 保留；Protocol/wire identity expression excluded |
| Randomness | WorldGen/NPC/Projectile random stream 有 owner、顺序和 checkpoint |
| Presentation | UI、render、audio、animation、tooltip、background 和 color 不建 placeholder owner |

## 7. Forbidden shapes

- [x] `MainFieldsComponent`、`MainStateManager` 或跨领域 `ServerData` God Object。
- [x] 用 `float[]`、`Item[]`、旧 static array、旧 `Dictionary` 或 manager reference 直接
  作为新的业务 owner。
- [x] 一个 System 直接写入 Player、NPC、Projectile、Item、WorldGrid、Chest 等多个容器，
  却没有 typed command/commit contract。
- [x] 用 `Protocol`、`Transport`、`Replication`、`Network` 类型作为服务器业务 Component。
- [x] 用 message ID、packet cursor、wire revision、sync cadence 或 client-visible snapshot
  充当 business persistence。
- [x] 用一个 `revision` 同时表示业务冲突顺序和网络复制脏状态。

## 8. Ordered actions

- [x] 收集 P01-P09 的 owner/read/write/deferred rows，不修改其工作稿。
- [x] 为每个跨域交互建立 read/write/validation/commit/retry/rollback matrix。
- [x] 找出重复 owner、隐式 static write、数组 alias、跨容器直接写入和 derived write-back。
- [x] 将 identity、business revision、wire identity、network revision 分开归类。
- [x] 将 P10 的 persistence/restart/overflow/stale-link 结果挂到对应 command/snapshot。
- [x] 将 P09 的 exclusion matrix 应用到所有 seam owner，拒绝 Protocol 回流。
- [x] 把未闭合项保留 `partial/deferred`，不得为了得到闭环而创建空 Component。

## 9. Acceptance and handoff

- [x] World、Player、NPC、Projectile、Item、WorldObjects、Server host 都有明确 primary owner、
  query/snapshot 和 command/commit boundary。
- [x] 所有跨域写入都有 authority、validation、ordering、rollback 和 persistence decision。
- [x] 没有重复可写 owner、God Object、raw array owner 或 derived write-back。
- [x] Protocol/replication/wire fields 没有进入 seam matrix；业务 identity/revision 没有误删。
- [x] P11 只把本文件交给 P99，不直接修改 parent plan、源码、CSV、JSON 或 manifest。

本任务不实现 Component/System/Command，不宣称跨域 parity，不运行测试、构建、verifier、
regression 或文档 gate。

## 10. Executed audit and handoff evidence

Audit date: 2026-09-01. This is a documentation contract and negative-boundary audit; it is not
runtime parity evidence. The source work papers consumed were P01-P10 at their current checked-in
state. P01, P02, P04, P06, P07, P08 and P09 provide explicit owner/exclusion rows; P03 and P05
retain unresolved consumer and behavior rows as `partial/deferred`; P10 supplies the persistence
edge-case policy but leaves the deletion ledger and `canRemoveLegacyWorldGen` gate open.

### 10.1 Owner conflict disposition

| Conflict shape audited | Disposition | Evidence/remaining risk |
| --- | --- | --- |
| duplicate World rule/clock/weather/progression storage | rejected; source/query split retained | P01 field cards; rain/import/progression side effects remain `partial` |
| inventory/equipment/bank/Void/trash alias | rejected; separate container owners and commands retained | P02/P05/P10 layout matrices; overflow and restore remain `partial/deferred` |
| raw `ai/localAI`, static arrays or manager references as owner | rejected; typed family/container boundary required | P03/P04/P07; NPC source-body evidence remains `deferred` |
| direct multi-container System writes | rejected; command coordinator and per-container commit owners required | §4.1 rows; no code implementation claimed |
| derived query write-back | rejected; derived values are query-only | P01/P07 and §6 state/query rule |
| Protocol/Transport/Replication/Network owner or B248 schedule | rejected; excluded projection only | P09 §§9.2-9.4; `Protocol owner = none` |

### 10.2 Identity and revision handoff

The following remain business values in the seam contract: `WorldId`, Player handle and authority,
NPC entity handle/`netID` semantics, Projectile handle/`identity`, Item instance handle, Chest
identity/content/revision, object anchors/links, and business revision where conflict or recovery
requires it. The following remain excluded: packet slot/index, `MessageBuffer.whoAmI`, encoded target
offset, replication ID, packet cursor, wire/network revision, dirty cursor, sync cadence,
`netUpdate`/`netSpam`, and `ProjectileReplicationSnapshot`.

### 10.3 Deferred handoff conditions

P99 must preserve `partial/deferred` for missing source-body, restore, command-order, random
checkpoint, overflow, stale-link, and 44-row `ServerRelevant` evidence. This document does not
close the deletion gate, does not promote a Protocol field, and does not authorize parent-plan
edits. Handoff target is P99 only.
