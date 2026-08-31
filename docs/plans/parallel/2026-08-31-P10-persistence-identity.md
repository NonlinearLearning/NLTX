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
| Status | `[ ] not-run` |

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
