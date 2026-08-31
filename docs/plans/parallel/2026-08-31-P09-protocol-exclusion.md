# P09 Protocol Exclusion Audit Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 建立独立的 Protocol/transport/replication 负向清单，确保任何领域任务都不会把
协议字段、封包布局或复制调度误报为服务器 ECS 迁移字段。

**Architecture:** P09 只产生 exclusion evidence，不产生 owner。业务身份、权限和运行时
revision 通过语义分类与 wire representation 分开；能够影响服务器规则的值仍由领域任务
负责，传输格式永远留在本任务的 `excluded` matrix。

**Tech Stack:** Documentation-only Markdown; no C# changes, no new functions, no tests/builds.

---

## 1. Task identity and write set

| 项目 | 内容 |
| --- | --- |
| Task | `P09` |
| Parent plan | `docs/plans/2026-08-31-server-required-field-property-migration-plan.md` |
| Dependency | `P00` |
| Wave | `1`，可与 P01-P08 并行 |
| Read set | Protocol/transport trees、replication proposals、MessageBuffer/NetMessage references、parent plan 第 4 节及 P01-P08 field cards |
| Write set | 仅本文件 |
| Status | `[ ] not-run` |

## 2. Absolute exclusion statement

P09 不迁移任何 Protocol 相关字段或属性。以下内容只能有 `excluded` 行，不能有
Simulation Component、business Snapshot、System、Command 或 owner：

- packet/message DTO、codec state、message ID、message kind、packet type、send mode、receive mode；
- wire layout、payload length、bit position、bit mask、byte cursor、reader/writer state、
  compression、framing 和 protocol version field；
- replication snapshot、replication cursor、sync cadence、per-player skip state、network
  dirty flag、network spam/throttle 和 reconnect wire state；
- `SyncNPC`、`SyncProjectile`、`SyncItem`、`SyncPlayer`、`SyncChestItem` 等同步帧的字段；
- `NetworkPlayerSlice`、`PlayerReplicationState`、`SessionReplicationState`、
  `NpcReplicationComponent`、`ProjectileNetworkUpdateComponent`、
  `ProjectileNetworkIdentityComponent` 等只服务复制的类型；
- B248 network scheduling、`netUpdate`、`netUpdate2`、`netSpam`、
  `netSyncSkippedForPlayer`、`NetworkUpdateReady` 和 `ProjectileReplicationSnapshot`；
- message 87 以及 open/update/delete/request/response 的 wire layout。

`Protocol owner = none`、`migration state = excluded`、`not a completion gate` 是本任务的
固定结论。

## 3. Exclusion inventory

| ID | Source family | Field/property shape | Exclusion reason | Business value exception | Status |
| --- | --- | --- | --- | --- | --- |
| `P09-X-001` | Protocol V1456 DTO | packet/message fields | transport representation | none | `excluded` |
| `P09-X-002` | Server Protocol | session-wire fields | wire boundary | host session identity is separate | `excluded` |
| `P09-X-003` | Transport | frame/payload/cursor/length | encoding/framing | none | `excluded` |
| `P09-X-004` | Transport | sequence/compression/bit state | wire scheduling/encoding | business mutation sequence is separate | `excluded` |
| `P09-X-005` | MessageBuffer | `whoAmI` wire expression | connection slot encoding | host session identity may remain host-only | `excluded` |
| `P09-X-006` | Sync frames | NPC/Projectile/Item/Player/Chest fields | replication projection | source entity/contents may be business state | `excluded` |
| `P09-X-007` | Projectile B248 | `netUpdate`/`netSpam`/network readiness | replication scheduling | projectile lifecycle remains separate | `excluded` |
| `P09-X-008` | Replication snapshots | snapshot cursor/revision/cadence | network projection | business revision remains separate | `excluded` |
| `P09-X-009` | Tile/object messages | message 87/open/update/delete layout | packet shape | object identity/anchor/state remain business values | `excluded` |
| `P09-X-010` | Player mapping | wire slot/payload/visible slice | client projection | inventory value state remains domain-owned | `excluded` |
| `P09-X-011` | Network target index | NPC/projectile encoded target | wire index/offset | business target handle remains domain-owned | `excluded` |
| `P09-X-012` | Network identity | replication ID/UUID encoding | transport identity representation | entity identity remains business state | `excluded` |

## 4. Identity and revision false-positive matrix

名称相同不代表语义相同，必须逐项按下面规则处理：

| Name/shape | Business/runtime meaning | Wire meaning | Final disposition |
| --- | --- | --- | --- |
| `WorldId`/`worldID` | world identity and persistence association | encoded world/session field if any | business identity retained; wire encoding excluded |
| Player handle | entity/session business binding | packet slot/index | business handle retained; wire slot excluded |
| NPC entity handle/`NPC.netID` | entity identity/target lookup | encoded NPC index | business identity retained; wire expression excluded |
| Projectile entity handle/`Projectile.identity` | projectile identity and restore association | replication ID/packet field | business identity retained; replication field excluded |
| Item instance handle | item ownership/merge identity | visible item slot/packet ID | business handle retained; wire slot excluded |
| Chest identity | object lookup/persistence association | chest message ID/index | business identity retained; message representation excluded |
| business revision | conflict/recovery/replay order | network revision/dirty cursor | business revision retained only if domain needs it; network revision excluded |
| permission/authority | server authorization decision | sender slot/message metadata | business permission retained host/domain; sender encoding excluded |

特别注意：`MessageBuffer.whoAmI` 的 wire 表达不能被迁移；这不等于删除
`ServerSessionState` 的 session identity，也不等于删除 Player/NPC/Projectile/Chest 的业务
寻址。

## 5. B248 handling

`docs/plans/2026-08-31-projectile-b248-network-scheduling-continuation.md` 只作为
Protocol/replication reference 使用。P09 必须明确：

- [ ] B248 的 `netUpdate`、`netUpdate2`、`netSpam`、`NetworkUpdateReady` 和
  `ProjectileReplicationSnapshot` 不进入本次 server field migration。
- [ ] session cursor、reconnect cursor、per-player skipped state 和 packet cadence 不建立
  Projectile/Session Simulation owner。
- [ ] Projectile 的 active/timeLeft/position/behavior/business revision 如被领域任务需要，
  仍走 P04 的业务 owner，不引用 B248 的网络调度类型。
- [ ] 不把 B248 的任何“完成”状态计入本计划 field parity、deletion gate 或 release readiness。

## 6. Protocol gate for other tasks

P01-P08 在交付前必须回答：

- 是否任何 Owner 名称含 Protocol/Transport/Replication/Network，但没有业务语义？
- 是否任何字段的唯一 consumer 其实是 encoder/decoder/sync scheduler？
- 是否把 wire cursor、payload length、message ID、slot index 或 network revision 当成运行时
  state？
- 是否把 client-visible snapshot 当成 server business snapshot？
- 是否因排除 wire 表达而错误删除了业务 identity/permission/revision？

若答案为“是”，P09 将该行退回 `excluded` 或要求拆成业务值与 wire projection 两行；
不得直接合并。

## 7. Ordered actions

- [ ] 扫描 Protocol、transport、replication 参考入口，生成 source path/anchor/状态清单。
- [ ] 将所有排除行写出 exact type/shape、source anchor、exclusion reason 和 false-positive exception。
- [ ] 逐一回查 P01-P08 的 owner 表，拒绝 Protocol/replication owner。
- [ ] 确认 business identity、permission、entity handle、Chest content 和 revision 没有被误删。
- [ ] 将 P09 matrix 提供给 P10/P11/P99；不修改领域工作稿，不修改任何源码或 Protocol 文件。

## 8. Acceptance and handoff

- [ ] Protocol/transport/replication/wire 的范围有完整反向清单。
- [ ] `Protocol owner = none`、`migration state = excluded`、`not a completion gate` 保持不变。
- [ ] B248 不计入本次 server field migration。
- [ ] 业务身份与 wire 表达已经成对区分，没有 identity deletion 误报。
- [ ] 本文件只交给 P10/P11/P99，不能作为任何字段的迁移完成证据。

本任务不实现、不测试、不构建、不验证任何协议，也不创建协议组件、编码器或复制快照。
