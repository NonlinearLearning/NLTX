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
| Status | `[x] complete` |

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
| `P08-H-013` | `--max-net-players`/`MaximumConnections` | connection admission capacity | `int = 255`; validated `1..255` | `ServerLaunchOptions` -> `DomeServer`; host lifetime | config; host-only | `host-only` |
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

### 3.0 Version4 Netplay omission audit

The broader `Version4` `Netplay` declaration was checked separately from the 24-field P08
inventory. These declarations must not be silently treated as migrated:

| Legacy declaration | Disposition | Evidence / reason |
| --- | --- | --- |
| `ServerPassword` | `excluded` | Password bytes and request/response handling are Protocol-owned; no current host authentication policy exists. A future password feature needs a typed secret boundary, not a copied static string. |
| `BanFilePath` | `deferred` | It is a host path, but the current server has no ban-list loader or admission policy. Do not invent a path owner without a ban decision consumer. |
| `SaveOnServerExit` | `deferred` | Current shutdown persists only configured auxiliary state; no world-save-on-exit policy is wired to this flag. |
| `Disconnect` | `host-only` | Current `CancellationTokenSource` and `Dispose`/listener shutdown own the equivalent lifecycle; no legacy flag is copied. |
| `HasClients` | `derived` | `ActiveSessionCount > 0` is the current host-owned equivalent; no second mutable cache is introduced. |
| `UseUPNP`, `TcpListener`, `Connection`, `Clients`, `_serverThread`, broadcast state | `excluded` | Transport/device handles, socket arrays, threads and broadcast state are runtime resources or wire/transport concerns. |

## 3.1 Source-backed field-card completion

The inventory above is completed against the current declarations. Anchors use the declaration
line (or the owning call site) and the SHA-256 snapshot below; a missing declaration remains
`deferred` instead of being invented.

| IDs | Source anchor and exact/default evidence | Lifetime and owner decision |
| --- | --- | --- |
| H-001,H-005,H-014 | `Startup/WorldBootstrap.cs:11-117`; `Startup/WorldBootstrapRequest.cs:6-53`; `Startup/WorldEntityLimits.cs:5-19`. `worldPath` is required absolute `string`; generation defaults to `Generated`; `MaximumPlayers=255`, validated `1..255`. | Bootstrap/world lifetime; `WorldBootstrap` owns orchestration and `SimulationEntityLimits` owns the business capacity value. |
| H-003,H-016,H-017 | `Persistence/WorldSaveCoordinator.cs:7-115`; `Persistence/DomeStateSaveCoordinator.cs:7-83`. Backup default is `2`, bounded `0..32`; temp file is flushed with `WriteThrough`, then backups rotate and `File.Replace`/`Move` commits; failures return typed recovery results. | Save/load operation; save coordinators own format/version and recovery. Streams/temp paths are host-only. |
| H-004 | `Persistence/WorldRecoveryResult.cs:5-19`; `DomeStateRecoveryResult.cs:5-19`. Nullable snapshot plus `FailureReason` is the value-only failure contract. | Load operation; host chooses retry/restart, Simulation receives only a successful snapshot. |
| H-006,H-007,H-008,H-011 | `Protocol/TerrariaProtocolSessionHost.cs:89-119,264-307`; `DomeServer.cs:1129-1157,1418-1424`. Session is created after hello, account is resolved before world data, Player is created on accepted spawn, and destroy is queued in `finally`. | Session lifetime; host/session state owns identity, binding, authority, and cleanup. Raw packet/session-wire fields stay excluded. |
| H-012,H-015 | `Startup/ServerLaunchOptions.cs:8-75`; `Program.cs:11-18,46-50`. Required absolute `.wld` path and port `1..65535` (except `7778`) are normalized before `Start`. | Process lifetime; `ServerLaunchOptions` owns immutable launch config. |
| H-002,H-023,H-024 | Current server uses scoped `FileStream`, `CancellationToken`, UI/menu legacy values, and task/closure resources only at host/protocol boundaries. | Operation/client lifetime; no Simulation owner and no persistence. |
| H-010,H-021 | `Version4物理删除了某些文件/Terraria/Main.cs:1389` declares `public const int MaxTimeout = 120`; `TerrariaProtocolSessionHost.HandleAsync` applies a bounded 120-second idle deadline to hello and every subsequent frame read. `ServerHostState.SessionTimeoutSeconds` owns the host policy. | Session lifetime; host timeout policy is transient host state. |
| H-009 | `TerrariaProtocolSessionHost.cs:118,135` records activity after each accepted frame through `DomeServer.RecordSessionActivity`; the read deadline is per connection, while `LastSessionActivityUtc` is currently an aggregate host observation rather than a per-session map. | Session lifetime; host-only activity observation. A per-session liveness owner remains an explicit follow-up if reconnect/idle reporting requires it. |
| H-018 | The server process is intrinsically dedicated; `ServerHostState.Dedicated` defaults to `true` and is never represented as Simulation state. | Process lifetime; host-only mode invariant. |
| H-019,H-020,H-022 | No current server declaration with a source-backed runtime consumer was found. | Keep legacy network mode transitions/counters `excluded`/`legacy-reference`; they remain outside the host business contract. |
| H-013 | `Startup/ServerLaunchOptions.cs:7-10,53-66,91-96`; `DomeServer.cs:49-50,207-219,806-817`. `MaximumConnections` defaults to `255`; `--max-net-players` is validated before startup and admission counts host session tasks. | Host process/session lifetime; `ServerLaunchOptions` stores configuration and `DomeServer` enforces admission without Simulation state. |

Source snapshot hashes: `WorldBootstrap.cs`
`F6EAED28B4F1599F4AA8188440DA94DF70FC3B757AFC99BAD474BF81F6227289`,
`WorldBootstrapRequest.cs` `D01B9DC7F5E0F40753D0E0942C9E272659851174DFE950D58F68A9883FCC9755`,
`WorldEntityLimits.cs` `6BF49E552736C38BAF24CBF6B3D7E44BDFA32759CD273AD3BEF63B6F8823ADDC`,
`ServerLaunchOptions.cs` `1F515D3EC5009F4D407E2DF395CAACDDF1F5AAEE9B32D725F017C6AB58559012`,
`WorldSaveCoordinator.cs` `ABF63326F1FA13D934D205EECE8AA1932A22B69D767BF0CE8D0D2A4882E3DA50`,
`DomeStateSaveCoordinator.cs` `8813C7957011BAB66387CF63D00106F2A5F059C3042EE561185AA6B3A2032282`,
`TerrariaProtocolSessionHost.cs` `3D4AE16A1AC9C48140D6CEAEAA3230FCE023E45D573E1632190E2909DC7DE470`,
`DomeServer.cs` `21EA85829FBDD92DB77DAFD60F312FB36343F6040574F3CCF610096E9B61093F`. The declaration anchors
are authoritative for this working-tree document; hashes must be refreshed when the source snapshot
is frozen by the parent plan.

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

- [x] 对 `M-OPS-*`、`M-HOST-*` 逐项记录 exact source anchor/type/default/lifetime；缺证据保留 `deferred`。
- [x] 将每个字段分类为 host-only、Simulation business value、derived 或 excluded。
- [x] 单独核对 `maxNetPlayers`/`maxPlayers`，不允许容量 alias；`MaximumConnections` 是 host admission，`MaximumPlayers=255` 是 Simulation 上限。
- [x] 固化顺序：hello -> account -> world/tile import -> spawn validation -> Player attach -> authorized command acceptance -> active -> finally cleanup。
- [x] 固化 import/save：absolute path -> decode/normalize -> value snapshot；temp + flush -> backup rotation -> atomic replace/move；typed failure -> host retry/restart decision。
- [x] 将 host 输出限制为 value-only request/snapshot；拒绝文件、线程、闭包、raw network state。
- [x] 交给 P10 做 persistence boundary 汇合，交给 P09 做 Protocol 负向审计。

## 6. Acceptance and handoff

- [x] P08 24-row host/session/startup/save/recovery inventory 均有独立 owner、排除或明确
  `deferred` disposition；扩展的 Version4 `Netplay` omission audit 在第 3.0 节单列未纳入
  本 24-row inventory 的密码、ban 与退出保存策略。
- [x] `maxNetPlayers` 与 `maxPlayers` 的语义、配置和容量边界已分离。
- [x] host state 没有被写入 Simulation component，也没有被转写为 Protocol field。
- [x] 所有输出都是 value-only business input/snapshot，未加入 raw file/network object。
- [x] 只将本文件交给 P09/P10/P11/P99，不直接修改 parent plan、源码、CSV、JSON 或 manifest。

本任务不宣称 Server host 或 persistence 全部完成；未执行测试、构建、verifier、regression
或文档 gate。

Protocol owner = none; migration state = excluded; not a completion gate. Packet framing,
message IDs, payload bytes, replication cursors, and `SessionReplicationState` remain P09's
negative audit boundary even when the host uses them to deliver a session response.

## 7. Code migration evidence

- `ServerLaunchOptions.MaximumConnections` and `--max-net-players` now own host admission capacity.
- `Program` applies the parsed host value through `DomeServer.ConfigureMaximumConnections` before
  `Start`; `DomeServer.HandleClientAsync` rejects sessions once the host task limit is reached.
- `WorldEntityLimits.MaximumPlayers` remains the independent Simulation entity limit and is passed
  only through `SimulationEntityLimits`.
- `ServerHostState` now owns host-only `WorldPath`, `MaximumConnections`, `BackupRetention`,
  `Dedicated`, `SessionTimeoutSeconds`, `LastSessionActivityUtc`, ready/accepting state,
  active-session count, and recovery-failure summary; these
  values are exposed by `DomeServer` without entering Simulation components.
- `ServerHostState.cs` SHA-256: `95042B68B7A5EB13BD81488A9888AE8E2F59C5A810C535DD2EBF7812A9F23C07`.
- `TerrariaProtocolSessionHost.cs` SHA-256: `764EE7D9B5B42CC59559A7D5FBDAD42497A0CE07ACCA6F2E6E83322EF90047B2`.
- `DomeServer.cs` SHA-256: `C2D2CB881B5F16DE1DD878553C46508955BF3EF1E18B24499684B470D9E7BE00`.
- `DomeServer` now exposes host-only `WorldPath`, `BackupRetention`, `SessionTimeoutSeconds`,
  and `IsAcceptingConnections`,
  and explicit `ConfigureWorldPath`/`ConfigureBackupRetention` seams; `Start` records the bound
  port in host state and `Dispose` closes the accepting lifecycle.
- P08 host inventory has 24/24 rows with a concrete host owner or explicit exclusion/deferred
  disposition. Server-relevant timeout, liveness, and dedicated-mode fields have concrete host
  consumers; legacy network counters/mode transitions and client/UI/resource fields remain
  explicitly excluded without importing them into Simulation or Protocol state. The broader
  Netplay omission audit records `ServerPassword`, `BanFilePath`, and `SaveOnServerExit` as
  excluded or deferred rather than claiming they were migrated.
- No tests were added, per task instruction. Build verification is the only runtime gate for this
  migration; the current run is blocked by pre-existing `DomeSimulation.cs` ECS query errors
  (`CS1503`, 35 errors).
