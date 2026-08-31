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
| Status | `[ ] not-run` |

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

## 5. Field ownership conflict checks

- [ ] `WorldMetadata`、`WorldRuleState`、`WorldClock`、`WorldWeatherState` 和
  `WorldProgressionState` 不互相保存同一可写值；derived query 不能反向写回源状态。
- [ ] Player inventory、equipment、bank、Void Vault、trash、Buff 和 world item store 的
  Item instance ownership 有唯一写入者；跨容器移动必须经过 command。
- [ ] NPC target handle、Projectile owner handle、Player handle 和 Chest opener 都是业务
  handle；encoded slot/index 由 P09 excluded，不进入 owner matrix。
- [ ] NPC global progression query 不复制到每个 entity；NPC-local phase state 不写入 World
  progression，除非明确 command/event contract。
- [ ] Projectile `ai/localAI` family state 与 lifecycle/combat/cooldown 分开；raw arrays和
  B248 scheduling 不作为公共 owner。
- [ ] Chest/Sign/TileEntity/Door/Wiring/TrainingDummy 的 identity、anchor、content/link、
  permission 和 revision 之间没有第二个隐式 static owner。
- [ ] WorldGen read snapshot、pass random state、Tile/Liquid/Structure command 和 commit
  sequence 不直接引用 legacy global mutable arrays。
- [ ] Server host session/permission/timeout 不被复制为 Simulation entity state；Simulation
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

- [ ] `MainFieldsComponent`、`MainStateManager` 或跨领域 `ServerData` God Object。
- [ ] 用 `float[]`、`Item[]`、旧 static array、旧 `Dictionary` 或 manager reference 直接
  作为新的业务 owner。
- [ ] 一个 System 直接写入 Player、NPC、Projectile、Item、WorldGrid、Chest 等多个容器，
  却没有 typed command/commit contract。
- [ ] 用 `Protocol`、`Transport`、`Replication`、`Network` 类型作为服务器业务 Component。
- [ ] 用 message ID、packet cursor、wire revision、sync cadence 或 client-visible snapshot
  充当 business persistence。
- [ ] 用一个 `revision` 同时表示业务冲突顺序和网络复制脏状态。

## 8. Ordered actions

- [ ] 收集 P01-P09 的 owner/read/write/deferred rows，不修改其工作稿。
- [ ] 为每个跨域交互建立 read/write/validation/commit/retry/rollback matrix。
- [ ] 找出重复 owner、隐式 static write、数组 alias、跨容器直接写入和 derived write-back。
- [ ] 将 identity、business revision、wire identity、network revision 分开归类。
- [ ] 将 P10 的 persistence/restart/overflow/stale-link 结果挂到对应 command/snapshot。
- [ ] 将 P09 的 exclusion matrix 应用到所有 seam owner，拒绝 Protocol 回流。
- [ ] 把未闭合项保留 `partial/deferred`，不得为了得到闭环而创建空 Component。

## 9. Acceptance and handoff

- [ ] World、Player、NPC、Projectile、Item、WorldObjects、Server host 都有明确 primary owner、
  query/snapshot 和 command/commit boundary。
- [ ] 所有跨域写入都有 authority、validation、ordering、rollback 和 persistence decision。
- [ ] 没有重复可写 owner、God Object、raw array owner 或 derived write-back。
- [ ] Protocol/replication/wire fields 没有进入 seam matrix；业务 identity/revision 没有误删。
- [ ] P11 只把本文件交给 P99，不直接修改 parent plan、源码、CSV、JSON 或 manifest。

本任务不实现 Component/System/Command，不宣称跨域 parity，不运行测试、构建、verifier、
regression 或文档 gate。
