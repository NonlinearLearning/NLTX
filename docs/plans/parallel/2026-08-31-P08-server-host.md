# P08 Server Host and Session Boundary Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将服务器确实需要但只属于启动、会话、导入、保存和恢复编排的字段独立归档，
避免把 host state 或连接信息错误迁移成 Simulation component。

**Architecture:** Server host 负责 process/session/lifecycle orchestration；Compatibility
负责把文件或外部输入归一化为 value-only request/snapshot；Simulation 只接收经过授权的
业务值。文件句柄、线程、原始路径、菜单状态和 Protocol state永远不进入 Simulation。

**Tech Stack:** Documentation-only Markdown; no C# changes, no new functions, no tests/builds.

---

## 1. Task identity and write set

| 项目 | 内容 |
| --- | --- |
| Task | `P08` |
| Parent plan | `docs/plans/2026-08-31-server-required-field-property-migration-plan.md` |
| Dependency | `P00` |
| Wave | `1`，可与 P01-P07、P09 并行 |
| Read set | parent plan 第 5-7、15-17 节、server bootstrap/session/save docs、WLD model/compatibility docs、AGENTS build boundary |
| Write set | 仅本文件 |
| Status | `[ ] not-run` |

## 2. Host field inventory

每个字段都要区分“服务器进程需要”与“Simulation 需要”，前者不自动成为 ECS owner。

| ID | Field/domain | Host semantic | Exact shape to record | Owner | Persistence | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `P08-H-001` | file path | world/player save source and destination | path value; no open handle | `WorldBootstrap`/adapter | config/host only | `host-only` |
| `P08-H-002` | file handle/stream | active I/O resource | `FileStream`/handle reference | host process only | never save | `excluded` |
| `P08-H-003` | backup retention | atomic save/backup policy | bounded integer/policy | `WorldSaveCoordinator` | host config | `host-only` |
| `P08-H-004` | load failure | import failure classification and recovery choice | typed result/error category | `WorldBootstrap` | recovery record | `host-only` |
| `P08-H-005` | world ready state | bootstrap phase and acceptance | enum/state value | `WorldBootstrap` | transient | `host-only` |
| `P08-H-006` | session identity | authenticated server session identity | host `SessionId` | `ServerSessionState` | session only | `host-only` |
| `P08-H-007` | bound Player handle | session-to-Player business binding | `PlayerHandle?` | `ServerSessionState` | session/reconnect policy | `host-only` |
| `P08-H-008` | permission | authorization scope | typed permission/capability | `ServerSessionState` | session/config | `host-only` |
| `P08-H-009` | liveness | last accepted activity/heartbeat state | monotonic tick/time value | `ServerSessionState` | transient | `host-only` |
| `P08-H-010` | timeout | disconnect decision boundary | bounded duration/ticks | `ServerSessionState` | host config | `host-only` |
| `P08-H-011` | disconnect cleanup | once-only unbind/despawn/rollback intent | typed cleanup state | `ServerSessionState` + domain commands | transient/event | `host-only` |
| `P08-H-012` | server mode | dedicated/server process mode | host enum/bool | `ServerLaunchOptions` | config | `host-only` |
| `P08-H-013` | `maxNetPlayers` | connection admission capacity | host `int` | `ServerLaunchOptions` | config | `host-only` |
| `P08-H-014` | `maxPlayers` | Simulation entity capacity | world/server rule `int` | `SimulationEntityLimits` | world/config | `partial` |
| `P08-H-015` | address/port | listener binding | host values | `ServerLaunchOptions` | config | `host-only` |
| `P08-H-016` | save version | persistence format version | typed version value | `WorldSaveCoordinator`/compatibility | required | `host-only/partial` |
| `P08-H-017` | recovery state | load/save retry and atomic replacement phase | typed host state | `WorldSaveCoordinator` | recovery record | `host-only` |
| `P08-H-018` | `dedServ` | legacy dedicated-server mode flag | source bool; host only | `ServerLaunchOptions` | config | `host-only` |
| `P08-H-019` | `netMode` | legacy process/network mode | source int; host boundary | host adapter | config/reference | `host-only/excluded` |
| `P08-H-020` | `_targetNetMode` | legacy mode transition | source int; host boundary | host adapter | transient | `host-only/excluded` |
| `P08-H-021` | `MaxTimeout` | legacy timeout constant/state | exact source type/default required | session policy | host config | `host-only` |
| `P08-H-022` | `netPlayCounter` | legacy network process counter | exact source type/default required | host session policy | transient | `host-only/excluded` |
| `P08-H-023` | menu flags/status | menu/UI lifecycle | legacy client values | no Simulation owner | none | `excluded` |
| `P08-H-024` | thread/closure queue | execution resource and callback state | thread/action/iterator reference | host process only | never save | `excluded` |

## 3. Boundary rules

- `maxNetPlayers` 是 host connection admission；`maxPlayers` 是 Simulation entity capacity，
  两者必须有独立 field card、默认值、来源和 overflow behavior。
- session identity、bound Player handle 和 permission 是 host business state；若 Simulation
  需要授权结果，只接收 normalized authority context，不接收 session transport object。
- 文件 path、stream/handle、backup object、thread、task、closure 和 raw parser 不进入任何
  World/Player/NPC/Projectile/Item/WorldObjects component。
- load/save failure、recovery decision 和 world-ready lifecycle 由 host 编排；World/Player
  snapshot 只包含 value-only business state。
- host 可以记录业务审计事件，但不能把事件编码为 message ID、packet cursor 或 replication
  field；这些内容由 P09 明确 `excluded`。

## 4. Host components and adapters

| Owner | Owns | Output to Simulation | Must not expose |
| --- | --- | --- | --- |
| `ServerLaunchOptions` | process mode、listen endpoint、host capacity、paths | immutable bootstrap options | menu/client/wire layout |
| `ServerSessionState` | session identity、Player binding、permission、liveness、timeout、cleanup | authorized business command context | raw message/reader/writer |
| `ServerCommandIngress` | bounded authorized command queue、tick/authority metadata | value-only commands | packet bytes、transport cursor |
| `WorldBootstrap` | load/import phases、failure/recovery、world ready | `WorldBootstrapRequest`/value snapshot | file handle、UI status、handshake field |
| `WorldSaveCoordinator` | save request/version/atomic replacement/backup/recovery | persistence value request/result | packet notification、socket state |
| `WorldCompatibilityAdapter` | old format decode/normalization | value-only World/Player/Object snapshot | legacy object reference、wire DTO |

## 5. Ordered actions

- [ ] 对 `M-OPS-*`、`M-HOST-*` 逐项记录 exact source anchor/type/default/lifetime。
- [ ] 将每个字段分类为 host-only、Simulation business value、derived 或 excluded。
- [ ] 单独核对 `maxNetPlayers`/`maxPlayers`，不允许容量 alias。
- [ ] 固化 session attach、permission check、command acceptance、disconnect cleanup 的顺序。
- [ ] 固化 import、save、backup、failure、retry、atomic replace 和 restart behavior。
- [ ] 将 host 输出限制为 value-only request/snapshot；拒绝文件、线程、闭包、raw network state。
- [ ] 交给 P10 做 persistence boundary 汇合，交给 P09 做 Protocol 负向审计。

## 6. Acceptance and handoff

- [ ] 所有 host/session/startup/save/recovery 字段均有独立 owner 和生命周期。
- [ ] `maxNetPlayers` 与 `maxPlayers` 的语义、配置和容量边界已分离。
- [ ] host state 没有被写入 Simulation component，也没有被转写为 Protocol field。
- [ ] 所有输出都是 value-only business input/snapshot，未加入 raw file/network object。
- [ ] 只将本文件交给 P09/P10/P11/P99，不直接修改 parent plan、源码、CSV、JSON 或 manifest。

本任务不宣称 Server host 或 persistence 全部完成；未执行测试、构建、verifier、regression
或文档 gate。
