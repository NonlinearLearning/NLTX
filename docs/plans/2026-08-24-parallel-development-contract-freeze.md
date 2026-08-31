# 并行开发公共契约冻结

> Flowstate execution plan: N4 / `graph` strategy / contract-freeze batch。
> 本计划只冻结跨方向接口和执行边界；具体实现必须由后续 `fst-iterate` task 承担。
> 本轮过程上下文预算为常规任务的 10%，验证预算为常规全量回归的 30%；两者都不降低
> 安全契约、拒绝路径或证据可追溯性。

## 目的

本文件冻结 Chest、Sign、TileEntity、Wiring、Liquid、Server 和 V1456 Protocol 并行开发前
必须共同遵守的四组公共契约。它是 `docs/plans/2026-08-18-wiring-liquid-chest-ecs-migration-execution-proposal.md`
的前置接口约束，不代表任何未有证据的旧版行为已经迁移完成。

冻结原则是：Simulation 持有权威状态和确定性提交，Server 持有 session、PVS、所有权校验
和断线清理，Protocol 只编解码，不持有 Arch 可变实体。跨域只传值类型命令和不可变快照。

## 1. 身份契约

### 1.1 身份类型和有效范围

| 身份 | 权威 owner | 当前类型/来源 | 冻结规则 |
| --- | --- | --- | --- |
| 玩家运行时身份 | Simulation | `PlayerHandle` | 正整数；只在 Simulation 内部寻址，不等同 V1456 slot。 |
| 玩家持久身份 | Server/持久化 | `PlayerIdentityComponent.CanonicalAccountUuid` | UUID 是账户身份；slot 只作为当前连接路由，断线后不得成为恢复依据。 |
| NPC | Simulation | `NpcHandle` + replication identity | handle、定义/网络类型和 replication ID 分开；NPC 所有权只能由 Simulation 提交。 |
| Projectile | Simulation | replication ID + `PlayerHandle` owner + 可选 UUID | owner、网络 identity、UUID 不得互相替代；UUID 仅在定义要求时存在。 |
| World Item | Simulation | replication ID + `ItemOwnershipComponent` | owner/container/source/revision 是独立字段；客户端不能写入 owner。 |
| Chest | Simulation | `ChestId` | 正整数；坐标索引唯一；删除后发 tombstone，不能在同一 live snapshot epoch 复用 ID。 |
| Sign | Simulation | `SignId` | 当前兼容实现允许 `0`，后续不得与 Chest/TileEntity ID 混用；坐标唯一性和删除 tombstone 必须显式定义。 |
| TileEntity | Simulation | `TileEntityPersistentState.Id` | 正整数；类型表决定 payload 解释；未知或 opaque 类型只能保留/转发，不能被通用 mutation 当作已知实体。 |

### 1.2 owner、session 和 UUID

1. `PlayerHandle` 表示当前 Simulation 实例中的运行时玩家，不是认证身份。
2. Server 必须将 `session identity -> PlayerHandle -> CanonicalAccountUuid` 绑定在会话创建时，
   并在断线清理时原子解除；任何命令的 actor 都来自绑定，不信任 payload 内自报 slot/owner。
3. Chest opener、Training Dummy 的 NPC link、Projectile owner 和 Item reservation 都是
   server-owned 状态。客户端只能提交意图，不能提交新的 owner 或 link。
4. 持久化恢复按 UUID/稳定对象 ID 恢复；网络 slot、连接顺序和 transient Arch entity handle
   不得写入恢复契约。
5. 两个对象域的 ID 不默认全局唯一。跨域引用必须使用带类型的值（例如 `NpcHandle`），不能
   用裸 `int` 猜测其语义。

## 2. 状态契约

### 2.1 Immutable snapshot

Snapshot 是提交后的只读事实，不是 ECS storage 的视图。Snapshot 必须满足：

- 构造时复制输入集合；对外只暴露 `IReadOnlyList`/`IReadOnlyCollection` 或不可变值类型；
- 包含对象身份、坐标/section、server-owned 状态和 revision；
- 只在 deterministic commit 成功后生成；验证失败不得发布部分快照；
- replication cursor 只记录已发送的 `(object identity, revision)`，不能反向修改 Simulation。

冻结前必须修正 `ChestSnapshot.Slots` 暴露 `ItemStack[]` 的可变表面，改为防御性只读集合或
不可变值容器。`Network*Slice` 和持久化类型继续执行同样的复制规则。

### 2.2 Revision

- revision 是对象域内单调递增的非负 `long`；对象创建时确定起始值，所有 mutation、opener
  生命周期、rename、inventory、link 和定义变化都必须说明是否递增。
- revision 溢出是拒绝/故障，不允许回绕。
- persistence 保存最后提交 revision；恢复不得重置 revision，也不得把未提交 opener 恢复为
  活跃 owner。
- replication 只在 commit 后观察 revision；未变化 revision 不产生重复 delta。

### 2.3 Tick、source sequence 和 expected revision

所有可变命令至少携带以下元数据：

```text
TickNumber       当前 Simulation tick，单调递增并参与 deterministic replay
SourceSequence   同一输入来源/域内单调递增，用于稳定排序和重复检测
ExpectedRevision 调用方观察到的对象 revision；-1 只允许表示“未提供”，不能绕过已要求的 guard
Actor            可选的 server-bound PlayerHandle/session identity
```

排序规则由域定义，但必须首先按 `SourceSequence`，然后按稳定对象坐标/ID 排序；同一 tick
中重复命令不得产生第二次提交。`ExpectedRevision` 不匹配必须在 mutation 前拒绝，并且不能
改变任何 inventory、opener、liquid、wire 或 tile-entity 状态。

当前 Simulation chest transfer 已有 expected-revision guard；当前 Server 会从会话
`ChestReplicationCursor` 推导 legacy revision。V1456 `SyncPlayerChest` 的原始 8-byte shape
保持兼容，显式 revision 使用 `NetModules` module `15`、envelope version `1`，并通过同一
module 的 version negotiation 选择扩展版本；所有路径均有 verifier。

## 3. 执行契约

每个行为族必须采用同一条可追踪链：

```text
Protocol Decode
  -> Server Identity/PVS/Range/Budget Validation
  -> Simulation Domain Validation
  -> Pure System
  -> Typed Value Command
  -> Deterministic Commit
  -> Immutable Snapshot
  -> Section/PVS Replication
```

具体约束：

1. Protocol 只产生 typed intent；不直接调用 Simulation 内部可变对象。
2. Server 验证 session identity、坐标范围、可见 section、权限和每 tick budget；失败输入
     不进入 Simulation mutation。
3. System 只读取 snapshot/component 输入并产生 command。Wiring 只能产生
   `DoorTransitionCommand`、`TileChangeCommand` 或 `LiquidTransferCommand` 等值类型；不得
   直接调用 Liquid/Chest 或网络发送方法。
4. Commit 是唯一写入 `WorldGrid`、Chest、Sign、TileEntity、Liquid 队列和 owner 状态的地方；
   提交顺序和冲突规则必须确定且可重放。
5. Replication 只消费 immutable snapshot，并按 session 的 PVS/cursor 生成消息；客户端回写
   不能修改 server-owned revision、owner、liquid amount/type 或机制状态。
6. 不支持的旧行为必须返回明确 rejection/deferred 结果，不能用 no-op 伪装成功。

## 4. 证据契约

每个并行方向的交付物都必须有一条可独立复核的 evidence record，至少包含：

| 字段 | 要求 |
| --- | --- |
| Legacy anchor | 旧源码绝对/仓库相对路径、类型/成员/行号、SHA-256；协议则附 message ID 和字段顺序。 |
| Current owner | 当前 Simulation/Server/Protocol module、具体 type/system/commit owner。 |
| Input state | fixture、身份、tick、source sequence、expected revision、PVS/范围和输入 snapshot hash。 |
| Output state | commit result、revision、tombstone/拒绝原因、输出 snapshot hash；网络方向附逐字段/字节结果。 |
| Verifier | verifier 工程、场景名称、执行命令、exit code、warning/error 摘要。 |
| Build diagnostic | 新鲜的 `Build/diagnostics/<area>/<run-id>/` 路径，包含命令、源码 hash、场景和结论。 |
| Status | `verified`、`partial`、`excluded` 或 `deferred`；只对明确覆盖的切片负责。 |

验收必须至少覆盖：

- 两会话相同 expected revision 的竞争操作只有一个 commit；
- opener/owner 在范围、PVS、session identity 和断线清理后保持一致；
- persistence round-trip 保留稳定 ID、revision 和允许恢复的 owner/link；
- Wiring/Liquid/Chest 联合 loopback 的输入、命令、commit、snapshot、replication 全链路；
- 恶意/重复/乱序/越预算 payload 在 Simulation mutation 前被拒绝；
- 每个 deferred/excluded 行为都有原因和不会静默成功的兼容路径。

## 冻结判定

四组契约冻结后，任何并行分支不得自行新增身份语义、revision 起始/递增规则、command 排序、
snapshot 可变集合或 evidence 字段。需要变更时必须先更新本文件、相关 contract verifier
和受影响方向的 evidence manifest，再开始实现。

当前批次已关闭此前阻塞的两项：Sign tombstone 通过 V1456 `NetModules` module `15` 的
versioned deletion frame 投影，chest expected revision 通过同一 module 的 versioned envelope
承载，并有独立 version negotiation。legacy message 47 仍没有删除形状，因此不会伪装成普通
sign update；旧客户端只会看到未识别扩展，Server 仍保留 typed owner 状态。

延期项的独立实施提案见
[`docs/plans/2026-08-24-deferred-protocol-contracts-proposal.md`](2026-08-24-deferred-protocol-contracts-proposal.md)。
该提案只冻结协议边界和后续批次，不改变当前 `Deferred` / `Rejected` 状态，也不代表正式
客户端删除帧或独立 revision negotiation 已实现。

## 当前批次证据（2026-08-24）

本批次已关闭 Chest snapshot 的可变数组表面：`ChestSnapshot.Slots` 现在在构造时复制输入，
对外只暴露 `IReadOnlyList<ItemStack>`；源数组后续修改不会改变已发布快照。Sign 的当前
创建、删除和 persistence 路径也已由 focused verifier 固化为 `SignId=0` 合法、删除后
tombstone 保留、allocator watermark 跨文件 round-trip 且不复用；Protocol 现在对合法
tombstone 返回显式 `Deferred`，越界值返回 `Rejected`。

证据记录：

- Legacy anchor：`Terraria/Chest.cs` 与 `Terraria/Sign.cs` 的旧源码锚点仍以既有迁移清单为准；
  本批次未扩大旧行为声明。
- Current owner：`src/Terraria.Dome.Simulation/WorldObjects/ChestSnapshot.cs`、
  `DomeSimulation.CreateSign`、`DomeSimulation.TryDeleteSign` 和
  `DomeStatePersistenceFormat`。
- Input/output：`Test/Terraria.Dome.WiringLiquidChest.Contracts.Verification` 验证源数组
  修改不影响快照，并验证 Sign IDs `0 -> 1`。
- Verifier/build diagnostic：
  `Build/diagnostics/contracts/20260824-110500/verifier.log` 与
  `Build/diagnostics/contracts/20260824-111500/protocol-build.log`，以及
  `Build/diagnostics/contracts/20260824-113000/verifier.log` 与
  `Build/diagnostics/contracts/20260824-115500/protocol-build.log`；focused verifier、Protocol
  build、Server build 均 exit `0`，无 warning/error。一次并行 build 的 `CS2012` 仅为共享
  编译输出锁竞争，已按仓库规则串行重跑并通过。
- Status：Chest snapshot `verified`；Sign ID 起始/创建序列 `partial`；删除 tombstone 与 ID 不复用
  `verified`；typed/正式 Sign deletion frame `verified`；V1456 chest transfer expected revision
  transport `verified`：legacy wire 继续由 cursor 推导，显式 revision 使用 versioned module
  envelope，version negotiation、未知版本和 malformed payload 均有拒绝路径。

## 8. Protocol Sign tombstone batch checkpoint（2026-08-24）

新增 `SignTombstoneProjection`，将 V1456 message 47 无删除形状这一事实编码为 typed result：
合法 tombstone 为 `Deferred` 并保留原因，ID/revision/reason 越界为 `Rejected`。没有发送空文本、
假更新或静默丢弃。

- Owner：`Terraria.Dome.Protocol.V1456.Packets.SignTombstoneProjection`。
- Focused verifier：`Test/Terraria.Dome.World.Protocol.Verification`，exit `0`。
- Build diagnostic：`Build/diagnostics/contracts/20260824-175000/protocol-verifier.log`；Protocol
  build `Build/diagnostics/contracts/20260824-180000/protocol-build.log`，exit `0`，0 warning /
  0 error。
- Status：Sign tombstone wire projection `verified`；legacy message 47 保持兼容，正式删除使用
  versioned module extension。

## 9. Server Sign tombstone owner batch checkpoint（2026-08-24）

`DomeServer` 现在通过 `TryDeleteSign` 暴露 Simulation 的 typed deletion boundary，并通过
`CreateSignTombstoneProjections` 将每个 tombstone 映射为 Protocol 的 `Deferred` 或 `Rejected`
结果。Server 不发送伪造的 message 47 更新，也不会把删除成功吞成空结果。

- Owner：`DomeServer` Sign lifecycle and projection boundary。
- Focused verifier：`Test/Terraria.Dome.World.Server.Verification`，exit `0`。
- Build diagnostic：`Build/diagnostics/contracts/20260824-183000/server-verifier.log`；Server build
  `Build/diagnostics/contracts/20260824-184000/server-build.log`，exit `0`，0 warning / 0 error。
- Status：Server-side Sign tombstone ownership 与 V1456 client deletion frame `verified`。

## 10. V1456 chest revision batch checkpoint（2026-08-24）

`ChestTransferIntent` 现在保留两种兼容形态：原始 8-byte `SyncPlayerChest` payload 将
`ExpectedRevision` 解释为 `-1`，新增 17-byte 自描述扩展 payload 可携带显式 `Int64` revision：
原始 8-byte payload 后追加固定 marker `0xD1` 和 little-endian `Int64`。解码只接受
8-byte legacy 或 17-byte marker extension，未知 marker、非法 revision 和其他长度均拒绝。
Server 的 `SessionReplicationState.TryAuthorizeChestTransfer` 统一执行规则：`-1` 使用已发送
cursor，显式 revision 必须精确匹配 cursor，mismatch 在 Simulation mutation 前拒绝。

- Legacy anchor：V1456 message 34 `SyncPlayerChest`，原始字段顺序保持不变。
- Current owner：`ChestTransferIntent`、`TerrariaPacketCodec`、`SessionReplicationState`、
  `DomeServer`。
- Focused verifier：`Test/Terraria.Dome.WorldObjects.Verification`，exit `0`；最新证据为
  `Build/diagnostics/contracts/20260824-170500/worldobjects-verifier.log`。
- Build diagnostics：`Build/diagnostics/contracts/20260824-174000/worldobjects-verifier.log`、
  `Build/diagnostics/contracts/20260824-180000/protocol-build.log` 与
  `Build/diagnostics/contracts/20260824-184000/server-build.log`；均 exit `0`，0 warning /
  0 error。Verifier 覆盖 legacy/explicit shape、固定 marker、unknown marker、revision `< -1`
  和非法长度拒绝。
- Status：legacy cursor-derived revision、versioned envelope、version negotiation、marker
  extension 与 malformed rejection 均 `verified`；message 34 legacy shape 未改变。

## 5. Flowstate 执行契约

### 5.1 阶段与依赖

| Phase | 目标 | 依赖 | 状态 |
| --- | --- | --- | --- |
| P1 | 冻结身份、状态、执行、证据四组公共契约 | 无 | completed/partial |
| P2 | 登记每个并行方向的唯一 owner、写集和输入/输出边界 | P1 | completed |
| P3 | 按批次实现；每批只改一个责任族并产出独立 evidence record | P2 | completed |
| P4 | 运行压缩验证矩阵，更新 checkpoint 和 deferred 清单 | P3 | completed/verified |

### 5.2 批次规则

1. 一个 batch 只能有一个主要写集；跨写集改动必须先更新本文件和受影响 manifest。
2. batch 入口必须记录 `owner / inputs / outputs / dependencies / status`；不得以聊天上下文
   作为隐式接口。
3. batch 结束才允许进入下一批；失败时保留 RED 证据，不把失败项误标为成功。
4. 过程状态落 `.agent-workplace/state/`；提交文档只保留决策、边界、证据路径和下一节点，
   不复制长日志或完整历史。

### 5.3 上下文压缩规则（目标 10%）

每个方向只携带以下五类信息：

| 信息 | 必须内容 |
| --- | --- |
| Contract | 相关契约条款编号和版本 |
| Scope | 本批唯一责任族、明确排除项 |
| Inputs | 输入快照/命令及其约束摘要 |
| Evidence | 最新 verifier、build diagnostic、exit code |
| Next | 一个下一动作和阻塞原因 |

旧日志、重复背景、已关闭批次和未引用源码片段不进入工作上下文；需要追溯时只通过
evidence 路径回读。每次 checkpoint 的目标是让上述摘要不超过常规任务上下文的 10%，并以
`state/checkpoint.json` 作为唯一当前状态入口。

### 5.4 压缩验证矩阵（目标 30%）

30% 是风险加权的验证预算，不是任意删测：

| 层级 | 每批保留的最小检查 | 触发条件 |
| --- | --- | --- |
| A 必测 | 编译/Schema、一个 happy path、一个拒绝路径、`git diff --check` | 所有 batch |
| B 必测 | 身份/PVS/重复或乱序/expected revision 中与本批相关的两项 | 触及 authority 或命令 |
| C 抽样 | 持久化 round-trip、跨域 loopback、边界数值各选一项 | 触及对应写集 |
| D 全量 | 完整回归、完整 oracle differential、物理删除 gate | 发布或契约变更 |

不得用 A 层绿灯替代 D 层结论；未触发的层级记录为 `not-run`，而不是 `verified`。每批
至少保存命令、退出码、warning/error 摘要和新鲜 diagnostic 路径；安全拒绝路径和契约
冲突测试不得削减。

## 6. 本计划 DoD

- [x] 四组公共契约有唯一版本、owner 和变更入口。
- [x] 每个并行方向都有唯一写集和 batch 依赖；无隐式共享可变状态。
- [x] `.agent-workplace/state/contract-freeze-2026-08-24.json` 能在 10% 摘要内恢复当前节点。
- [x] 每批按 30% 风险加权矩阵保存可复核 evidence；未运行项与 deferred 项明确区分。
- [x] Chest snapshot、Sign 生命周期、V1456 expected revision envelope 和 Sign deletion
  frame 均有对应 verified evidence；未支持的 legacy client 行为仍显式保留兼容边界。
- [x] 本计划范围内的实现均通过 `fst-iterate` batch 落地；每个 batch 保留唯一写集、
  focused verifier 和 build evidence，未把 deferred 能力伪装成 complete。

**当前 Flowstate 状态：** `N4 / verified / P4 completed`。本批次的协议扩展、Sign deletion
frame、capability gate 和 Sign PVS/reconnect loopback 已关闭；full contract-release regression
仍是 release 级 deferred，后续行为扩展必须开启新的 batch 并重新生成 evidence。

## 10. Version4 源扩展矩阵（2026-08-24）

本节把冻结范围延伸到 `D:\TRbackup\Version4物理删除了某些文件\Terraria` 的实际源码责任族。
源码只作为行为锚点和排除依据，不作为可直接搬运的实现模板；任何新代码仍必须落在
Simulation/Server/Protocol 的既定 owner 内。

### 10.1 源文件基线

| Source | SHA-256 | 关键锚点 | 当前扩展结论 |
| --- | --- | --- | --- |
| `Terraria/Main.cs` | `844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D` | `UpdateWeather:12458`、`UpdateTimeRate:3567`、`QueueMainThreadAction:11541`、`UpdateTime:13388` | weather/clock 可拆 deterministic slice；任意 Action 和 random presentation 不可泛化 |
| `Terraria/NPC.cs` | `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17` | `AI:18642`、`TargetClosest` 调用族、`NewNPC` 调用族 | 只接受已冻结 definition/input/command 的 AI 家族；完整 `AI` 和 boss table deferred |
| `Terraria/Projectile.cs` | `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B` | owner 字段 `126`、`NewProjectile:10220`、owner-hit checks `11469+` | owner/identity/lifetime/collision 可分写集；类型特定命中盒和 client effects deferred |
| `Terraria/WorldGen.cs` | `A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D` | `GenerateWorld:10108`、Dungeon setup `10096+`、tile mutation families | 只有 source-derived query/command 可进入；完整随机生成和物理删除 gate 继续 blocked |

### 10.2 并行写集和依赖

| Lane | 唯一写集 | 依赖 | 可接受切片 | 明确排除 |
| --- | --- | --- | --- | --- |
| Main weather/clock | `Simulation/World/**`、WorldRules verifier | Tick phase + immutable clock | rain/wind bounds、pause、Lantern Night same-tick suppression | random wind/cloud、client ambience |
| NPC behavior/input | `Simulation/Npc/**`、NPC verifier | identity/target/player snapshot | one AI family、target validation、typed attack command | full `NPC.AI`、boss/event tables |
| Projectile lifecycle | `Simulation/Projectile/**`、Combat verifier | owner/identity/revision | allocator、finite motion、lifetime、tombstone reason | type-wide hitboxes、reflection、client effects |
| WorldGen structure | `Simulation/WorldGeneration/**`、WorldGen verifier | tile command/commit + RNG contract | pure geometry/query/placement intent | `GenerateWorld` parity、random pass replay、legacy deletion |
| Source ledger | `docs/research/**`、`docs/migrations/**` | lane evidence only | anchor/hash/status refresh | 修改其他 lane 的生产代码 |

规则：一个 lane 一个主要写集；跨 lane 只通过 immutable snapshot、typed command 或 evidence
manifest 交接。Source hash 变化时，所有依赖该 anchor 的 `verified` 自动降为 `partial`，
必须重新生成 evidence，不能沿用旧 exit code。

### 10.3 下一批代码入口

下一批优先选择 `Main weather/clock` 的 deterministic boundary：对照 `Main.UpdateWeather`
的雨强耦合风速公式和 `UpdateTimeRate` 的 pause/rate 输入，补齐一个 typed transition
verifier；不实现 `Main` 的 random wind counter、cloud presentation 或 arbitrary
`QueueMainThreadAction`。入口、输出和验收固定为：

```text
Version4 Main anchor
  -> immutable WorldClock/WorldRule input
  -> WorldWeatherSystem transition
  -> committed WorldRuleSnapshot
  -> WorldRules projection
```

该 batch 的最小 evidence 为：一个正常雨天推进、一个 pause/rate 拒绝或不变路径、一个
Lantern Night 冲突路径、一个 persistence continuation，以及 Simulation/Server serial
Release build。完整 Main tick regression 属于 D 层，仅在该契约扩展正式发布时触发。

### 10.5 Main weather/clock resolved-rate checkpoint（2026-08-24）

本批已将 Version4 `Main.UpdateTimeRate` 的解析结果接入 `WorldWeatherSystem`。天气不再
固定消费 `WorldClockSnapshot.TicksPerUpdate`，而是消费同一 tick 已解析的
`WorldTimeRateSystem` rate；rate 为 0 时雨/风不递减，Lantern Night 仍在同一提交边界抑制雨。

- Source anchors：Version4 `Main.cs:3567`（`UpdateTimeRate`）、`Main.cs:12458`
  （`UpdateWeather`）。
- Owner：`WorldTimeRateSystem` -> `WorldClockSystem` -> `WorldWeatherSystem`，写集为
  `src/Terraria.Dome.Simulation/World/**` 与 WorldRules verifier。
- Inputs：immutable `WorldClockSnapshot`、resolved `ticksToAdvance`、rain/wind typed
  requests、Lantern Night authority。
- Outputs：committed `WorldRuleState` 与 `WorldEnvironmentTransition`。
- RED：`Build/diagnostics/contracts/20260824-132000-weather-rate-red.log`，证明旧实现
  忽略 fast-forward rate。
- GREEN：`Build/diagnostics/contracts/20260824-133000-weather-rate-green.log`，exit `0`，
  包含 `PASS: weather consumes the resolved simulation time rate`。
- Build：Simulation `Build/diagnostics/contracts/20260824-133100-weather-rate-simulation-build.log`、
  Server 重跑 `Build/diagnostics/contracts/20260824-133200-weather-rate-server-build-rerun.log`，
  均为 0 warning / 0 error。并行 Server 首次运行的 `CS2012` 仅为输出锁竞争，已保留在
  `20260824-133100-weather-rate-server-build.log`，不作为源码失败结论。
- Diff：`Build/diagnostics/contracts/20260824-133300-diff-check.log`，exit `0`。
- Status：resolved weather rate boundary `verified`；完整 random wind/cloud、Main
  presentation 和 full tick regression 仍为 `deferred`。

### 10.4 源证据禁止事项

- 不以 Version4 文件行数、方法名相同或“能编译”证明迁移完成。
- 不把 `Main.UpdateTime` 内的 random spawn、NPC/Projectile 全表 AI 或 `WorldGen.GenerateWorld`
  直接搬进一个 universal system。
- 不把 client/UI/audio/cloud/particle 分支计入 server authority 完成度。
- 不为 legacy 缺少 wire shape 的行为发送空文本、零值或伪造成功帧；必须返回 typed
  `Deferred`/`Rejected` 并留下 source anchor。

## 7. Sign tombstone batch checkpoint（2026-08-24）

本批只修改 Sign 生命周期写集，新增 typed `DeleteSignCommand`、删除提交和
`SignTombstoneSnapshot`。删除要求 `ExpectedRevision` 精确匹配，提交后从 live snapshot 移除，
写入不可复用 ID 的 tombstone；重复删除、revision 冲突和 max revision 在 mutation 前拒绝。
持久化同时恢复 live signs 与 typed tombstones，已删除 sign 不会重新激活，allocator watermark
继续单调前进。

- Owner：`DomeSimulation` Sign lifecycle boundary。
- Contract inputs：`SignId`、`ExpectedRevision`。
- Contract outputs：live `SignSnapshot` removal + `SignTombstoneSnapshot`。
- Focused verifier：`Test/Terraria.Dome.WorldObjects.Verification`，exit `0`。
- Build diagnostic：`Build/diagnostics/contracts/20260824-120000-sign-tombstone-verifier.log`；
  `Build/diagnostics/contracts/20260824-120000-sign-tombstone-simulation-build.log`、
  `Build/diagnostics/contracts/20260824-123000/verifier.log` 与
  `Build/diagnostics/contracts/20260824-124500/server-build.log`，均 exit `0`，Server
  build 为 0 warning / 0 error。
- Status：Sign deletion tombstone、V1456 chest transfer explicit revision transport、独立协商
  envelope 和正式 Sign deletion frame 均 `verified`。
