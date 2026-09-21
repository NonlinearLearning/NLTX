# Server-Required Field and Property Migration Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将当前迁移方向中真正属于 server-authoritative state 的字段和属性逐项归属到
Definition、Component、Snapshot、System、Command 或 Server host，并把所有 `Protocol`
相关字段明确排除在迁移范围之外。

**Architecture:** 以 World、Player、NPC、Projectile、Item、WorldObjects 和 Server host
为独立领域模块。不可变规则进入 Definition，实体或世界的可变值进入 Component，跨 tick
可恢复的值进入 Snapshot，写入通过 typed command 和确定性 commit 完成；兼容层只把旧输入
转换成值状态。每一项字段必须有唯一 owner、来源、默认值、可变性、生命周期、持久化判定
和当前状态，禁止重新构造 `MainFieldsComponent`、`MainStateManager` 或其他 God Object。

**Tech Stack:** Documentation-only; no source changes, no new functions, no test/build/verifier
execution. The intended implementation boundary is `Terraria.Dome.Simulation`,
`Terraria.Dome.Server`, `Terraria.WorldFile.V319`, and `Terraria.WorldCompatibility`; the
`Protocol` and transport trees are explicitly outside this plan.

---

## Scope Change Record

- Original request: 为当前服务器 ECS 迁移方向建立字段/属性执行计划，逐项核对字段，设计各个
  ECS 组件，只保留服务器必要状态。
- Added constraint: **不迁移任何 `Protocol` 相关的字段或属性。**
- This document only covers server-authoritative state. “服务器需要知道”不等于“字段可以
  放进 Simulation”；文件路径、会话生命周期和权限可以留在 Server host，wire/layout
  值不能进入本计划。
- `Protocol` 不是本计划的组件、模块、owner、证据或完成条件。
- No C# implementation, no function expansion, no project-file change, no ledger/CSV/JSON
  change, and no test/build/verifier execution are included in this task.
- Existing migration documents may仍然提到协议投影；那些内容在本计划中只作为背景风险，不能
  使协议字段重新进入迁移清单。

## 0. 本计划的硬结论

下面四句话是所有后续字段核对的优先级最高规则：

1. **`Protocol` 不参与字段迁移 owner 设计。** 不为 packet DTO、消息编号、wire layout、
   bit flag、编码长度、协议游标或复制帧建立 ECS owner。
2. **`Protocol` 不是组件。** 不创建 `ProtocolComponent`、`NetworkFieldsComponent`、
   `ReplicationStateComponent` 或任何把传输格式包装成运行时状态的替代类型。
3. **`Protocol` 不属于完成条件。** 协议编解码、客户端可见字段、同步帧数量和 wire parity
   不能提升本计划任何字段的状态，也不能授权删除 legacy server state。
4. **本计划只核对 server-authoritative state。** 只留下会影响服务器规则、世界模拟、实体
   生命周期、权限、持久化或 deterministic replay 的值；其他字段必须明确写成
   `excluded`、`host-only` 或 `deferred`。

“不迁移 Protocol 字段”不表示删除身份、权限或实体寻址。`WorldId`、Player handle、NPC
实体 handle、Projectile 实体 handle、Item 实例 handle、Chest identity 和内部 revision
仍然是运行时/持久化所需的业务身份；它们不能被误删，也不能被重新解释为 wire 字段。

## 1. 当前进度快照

### 1.1 当前结论

当前主线是 `N6 / in_progress_with_deferred_findings`，整体迁移仍为 `partial`。现有
窄 owner 证明了若干 server slice 已经有承载，不代表旧 `Main`、`Player`、`NPC`、
`Projectile`、`Item` 或 `WorldGen` 的完整行为已经等价。本文件不把“字段有同名承载”
当作完成，也不把客户端或协议映射当作服务器字段证据。

| 方向 | 当前基线 | 能说明什么 | 不能说明什么 |
| --- | --- | --- | --- |
| Flowstate | `N6 / in_progress_with_deferred_findings` | 仍有明确的 deferred/blocker | 不能宣称收敛或 release-ready |
| Main 语义清单 | `members=696`、`migratedScope=695`、`identityExcluded=1`、`accepted-narrow=126` | 已有一套 Roslyn 级成员与窄 owner 计数 | `126` 不是 `126/696` 的迁移比例，也不是全量行为完成 |
| 核心能力台账 | 11 个能力族，9 个 evidenced，NPC 与 Containers/Signs/TileEntities 为 partial，weighted core score `92` | 已有风险加权的服务器能力证据 | 不是 Terraria parity、不是全字段分数、不是删除许可 |
| WorldGen inventory | `684` methods、`233` fields；methods 为 `162` partial、`522` unmapped；fields 为 `233` unmapped | 生成状态仍是最大的未收敛字段族之一 | 不能用 registry 数量代替生成行为等价 |
| WorldGen 源规模 | 约 `1,901,533` bytes、`73,355` lines | 需要按 pass/状态/随机流分段 | 不能一次性把旧静态字段复制成一个对象 |
| WorldGen differential | `5,040,000` tiles；`3,190,404` mismatch（约 `63.30%`）；extended-state mismatch `1,046,843`；`canRemoveLegacyWorldGen=false` | 当前删除门明确关闭 | 不能删除 legacy WorldGen，也不能把局部 pass 的绿色证据外推为全局 parity |
| physical deletion ledger | `535` rows：`427 ClientOnly`、`60 SharedDefinition`、`44 ServerRelevant`、`4 ReplacedWithEvidence`、`0 Unknown` | 分类账完整，ServerRelevant 尚有延期 | 分类完成不等于替换完成；44 行不允许物理删除 |
| Player | PB-001 至 PB-011 仍有 partial/blocked；旧 58 slots、runtime 40 slots、persistence 990 slots 不等价 | 核心输入、移动、生命、库存有窄 owner | 不能用同名 alias 掩盖布局差异，专用机制仍未闭合 |
| NPC | 当前已有生命周期、target、home、slot、death/invasion 等窄切片；源侧约 158 个 `AI_###` markers | 可继续沿 source-backed behavior family 收敛 | 完整 AI、Boss phase、invasion/event、Buff/DoT、town service、worm movement 尚未完成 |
| Item | 148 个字段/属性有归属报告；更宽的 mapping 表含 237 条初始 ownership | Definition、实例、库存、世界物品已有多个 owner | ownership 不是行为完成；完整 SetDefaults、prefix、shop、shimmer 仍 partial/deferred |
| 当前 P9 抓手 | Pyramid wall-frame evaluation 是下一 bounded direction；neighbor boundary 已有独立窄片 | 可以继续拆 immutable neighbor/mask/value/command slice | 不能把该方向写成已完成，也不能用它关闭 WorldGen 删除门 |

### 1.2 证据口径漂移必须显式保留

当前资料存在不同时间/不同扫描口径的数字。执行时只能记录来源和日期，不能混加：

- WorldGen 当前 inventory 是 `684/233`，而删除门内嵌历史值仍可能出现
  `partialMethodCount=125`、`unmappedMethodCount=559`、`unmappedFieldCount=233`。
- 旧 responsibility ledger 曾记录 `425 ClientOnly`，当前 CSV 记录为 `427 ClientOnly`；
  当前 ledger 的可执行基线是 CSV 的 `535 = 427 + 60 + 44 + 4`。
- Main 的 `accepted-narrow=126` 是当前 resolver/证据状态，不与旧报告中的 `115`、`118`
  或词法侧 `685` 直接相加。
- Item 的 `148/151` 是字段/属性报告口径；`237` 是更宽的初始 mapping 行数，可能含方法
  和不同归属层，不应作为完成度分母。

每一条新记录必须带有 source snapshot、源文件 hash 或现有证据路径；没有这些信息时状态
只能是 `unmapped`、`deferred` 或 `not-run`。

## 2. 迁移痛点与拖慢因素

下面按“为什么会拖慢”而不是按旧类名排列。每一项都需要在执行过程中有明确的消解动作。

| 优先级 | 痛点 | 直接证据/现象 | 拖慢路径 | 处置方向 |
| --- | --- | --- | --- | --- |
| P0 | 范围混入 Protocol、客户端和 Simulation | `Main`、`MessageBuffer`、`NetMessage`、复制/传输树同时出现在旧报告 | owner 争论变成 wire 设计，字段被重复建模，完成度失真 | 先执行本文件 Protocol exclusion audit；任何 wire 字段直接排除 |
| P0 | God Object 读写耦合 | `Main` 同时含世界尺寸、tick、随机、实体数组、UI、资源、网络和队列 | 一个字段的读写者跨多个域，改一个 owner 会破坏多个调用链 | 用字段卡锁定唯一 owner；跨域只经 snapshot/query/command |
| P0 | WorldGen 全局随机流和顺序敏感 | `Main.rand`、pass reset、`TileRunner`、frame、liquid side effects 相互影响 | 局部结果正确但全局 tile/state 仍偏离，无法安全删除 legacy | 先冻结 pass order、stream owner、checkpoint，再做结构/Tile 差分 |
| P0 | 差分规模太大 | `3,190,404` tile mismatch 与 `1,046,843` extended-state mismatch | 无法从单一失败点判断是输入、顺序、frame、liquid 还是结构问题 | 按 stage、field、command、random checkpoint 分层，不接受总体猜测 |
| P0 | 数组/槽位语义不等价 | Player 旧布局 50/4/4=58，当前 runtime 40，持久化 990；实体旧数组还有 slot reuse | 结构看似已有，但索引、空槽、容量、顺序、恢复结果不一样 | 保留 legacy layout contract 作为独立 deferred，不新增同名 alias |
| P1 | 静态表数量大且语义细 | tile solid、merge、frame、buff、projectile、NPC 分类各自有默认/边界/版本 | 把一个表合并到现代 registry 会引入额外类型和错误默认值 | 每表独立 registry、来源锚点、默认值、越界规则和 consumer |
| P1 | NPC AI 族爆炸 | 当前记录约 158 个 `AI_###` source markers | 用一个 `ai[]` 或巨大 `NpcBehaviorSystem` 既无法复现也无法审核 | 按 behavior family 建 typed state；未覆盖族保持 deferred |
| P1 | 生命周期是跨域状态机 | active、timeLeft、death、loot、invasion、spawn、segment/home 相互写入 | 只迁移字段不迁移 transition 会产生 zombie、重复掉落或错误计数 | lifecycle component + typed transition command + commit 顺序 |
| P1 | 规则、实例状态和派生值混放 | Item、Player equipment/buff、NPC definition/state 原本混在类里 | 定义被实例污染，实例被静态表污染，重启恢复不稳定 | Definition/Component/Snapshot 三分，派生值不持久化 |
| P1 | 持久化边界与运行时边界不一致 | WLD metadata、Dome snapshot、Player persistent state 有不同版本/容量 | “能读出来”不代表运行时 owner 正确；未知版本容易被默认值覆盖 | Import 只输出 value-only request/snapshot，版本差异显式 unknown |
| P1 | 主线程队列和延迟工作含义不明 | `DelayedProcesses`、`DelayedProcessesInGame`、`_mainThreadActions` 混有 `Action`/`IEnumerator` | 直接塞 ECS 会把执行时机、闭包、客户端副作用带进状态 | 只保留 typed intent；无法证明 server authority 的留在 host/deferred |
| P1 | Server authority 与输入边界容易混淆 | 本地键鼠字段、输入 frame、服务器接受的控制意图位于不同层 | 把设备状态当成 Simulation 状态，或让未授权输入直接写实体 | Server 只接收归一化、带 authority/tick 的业务意图 |
| P2 | 物理删除门过于保守但必须保守 | 44 `ServerRelevant` rows deferred；WorldGen gate false | 迁移速度受证据链而非新增类数量限制 | “延期”是合法结果；未闭合字段不做静默删除 |
| P2 | 窄 verifier 容易被误读 | 局部 owner 可 green，aggregate parity 仍失败 | 进度汇报把局部完成说成整体完成，下一批基线被污染 | 报告 `completed_partial`，保留总 gate 状态 |
| P2 | 共享 worktree 与构建输出竞争 | 当前 checkout 有大量用户改动和并发诊断输出 | 并发读写 Build/bin/obj 会生成非因果失败，拖慢重跑 | 本轮不触碰构建；未来验证采用串行、隔离、鲜明 evidence path |
| P2 | 文档和清单分散 | Main、NPC、Item、WorldGen、deletion ledger 的数字和口径不同 | 反复搜索、重复分类、错误复用旧结论 | 以本计划 field ID/状态为导航，原始清单只作为证据来源 |

### 2.1 最高优先级的实际拖慢项

如果只能先处理五件事，顺序应为：

1. 彻底冻结 Protocol exclusion，清理“复制/协议已存在所以字段已迁移”的误判。
2. 冻结 WorldGen 的输入、pass 顺序、随机 stream 和 Tile/extended-state 差分分层。
3. 解决 Player 的 58/40/990 布局分裂，不再通过 alias 隐藏容量差异。
4. 把 NPC 的生命周期、AI family、事件/掉落拆成可审计的小 owner。
5. 将 44 个 ServerRelevant deletion rows 逐条挂回 replacement evidence 或保持 deferred。

## 3. 服务器必要字段的判定规则

### 3.1 纳入条件

字段或属性只有在至少满足下列一项，并且不是 Protocol/wire 字段时，才可以进入服务器
必要清单：

- 改变权威世界规则、时间、天气、事件、生成、液体、Tile/Wall 或结构结果。
- 改变实体生命周期、位置、碰撞、生命、伤害、AI、目标、库存、装备、Buff、掉落或交互。
- 决定服务端权限、会话生命周期、玩家绑定、超时、断开清理或服务器启动恢复。
- 是服务器持久化需要的值，并能从旧存档/当前状态明确投影为 value-only state。
- 是服务端规则计算所需的不可变 Definition/Registry 值，且有真实服务器 consumer。

### 3.2 不纳入条件

以下情况不得进入 Simulation Component，也不得为了“覆盖旧声明”创建占位字段：

- 只被客户端绘制、镜头、天空、背景、粒子、音频、UI、tooltip 或本地诊断使用。
- 只是 packet DTO、message ID、wire cursor、payload length、bit flags、压缩 scratch、
  同步节流字段或客户端投影字段。
- 只是文件句柄、线程对象、原始路径缓存、窗口状态、设备输入对象或闭包队列。
- 只有名字相似，没有 source consumer、默认值、生命周期或持久化证据。
- 能通过更小的派生 query 得到，不需要存储为可写全局字段。

### 3.3 状态词汇

| 状态 | 本计划含义 |
| --- | --- |
| `accepted-narrow` | 已有明确 server owner 与窄证据；只覆盖表中标明的值域/分支 |
| `partial` | 有组件/定义/命令形状或局部行为，但默认值、完整 consumer、恢复或边界未闭合 |
| `deferred` | 已确认属于服务器责任，但尚无足够行为证据；不能以同名字段填空 |
| `host-only` | 服务器需要，但只属于 Server startup/session/persistence 编排，不进入 Simulation |
| `excluded` | 客户端、Protocol、传输、UI、表现或无 server authority 的字段 |
| `legacy-reference` | 只作为旧来源/Oracle/导入参考，绝不是运行时 owner |
| `not-run` | 本轮没有执行任何验证或构建；不等于失败，也不等于通过 |

## 4. Protocol 绝对排除清单

本节中的条目是“核对是否被错误纳入”的反向清单。勾选表示已确认排除，不表示需要
迁移；任何后续计划都必须保持这些行的 `excluded` 状态。

- [ ] `src/Terraria.Dome.Protocol.V1456/**` 中所有字段、属性、DTO、codec state。
- [ ] `src/Terraria.Dome.Server/Protocol/**` 中所有 packet/message/session-wire 字段。
- [ ] `src/Terraria.Dome.Transport/**` 中 frame、payload、cursor、length、sequence、
  compression 和 framing 字段。
- [ ] `MessageBuffer.whoAmI` 等连接身份的 wire 表达；运行时的 session identity 另行保留，
  但不复制 wire 字段。
- [ ] `messageId`、message kind、packet type、send mode、receive mode、payload length、
  bit position、bit mask、byte cursor、reader/writer 状态。
- [ ] `SyncNPC`、`SyncProjectile`、`SyncItem`、`SyncPlayer`、`SyncChestItem` 等同步帧
  对应的字段集合。
- [ ] `NpcReplicationComponent`、`ProjectileNetworkUpdateComponent`、
  `ProjectileNetworkIdentityComponent`、`NpcReplicationSnapshot`、任何只服务复制的
  cursor/revision 字段。
- [ ] `NetworkPlayerSlice`、`PlayerReplicationState`、`SessionReplicationState`、
  section visibility cursor 和 PVS 编码字段。
- [ ] wire-specific `netUpdate`、`netSpam`、`skippedSyncs`、`streamCounter`、
  `netOffset`、`netStream` 以及按玩家同步的 scratch 状态。
- [ ] `NetMessage` 的压缩缓存、broadcast recipient、同步帧 flags 和编码临时字段。
- [ ] 客户端 projection 中的可见性、顺序、客户端 slot、客户端回放编号和显示状态。
- [ ] 任何以“将来可能需要发包”为理由新增的 Component/Property/Definition 字段。
- [ ] 任何使用协议完成、协议 loopback 或 wire parity 作为迁移完成证据的字段记录。

核对结果必须写成：`Protocol owner = none; migration state = excluded; not a completion gate`。

## 5. 总体架构与组件设计约束

### 5.1 目标数据流

```text
WLD/legacy value import
    -> WorldBootstrapRequest / PlayerRestoreRequest
    -> WorldMetadata + WorldGrid + WorldRule/Seed state
    -> entity Definition + runtime Components
    -> System reads immutable snapshots and Components
    -> typed simulation command (tick + simulation sequence + authority)
    -> deterministic commit
    -> server-owned runtime snapshot / persistence value
```

这里的 snapshot 是服务器内部的不可变读模型或持久化边界，不是 wire snapshot。任何需要
传输的表示都不属于本计划。

### 5.2 Module、Interface、Seam、Adapter 约束

每个组件族都按深模块思路设计：

- 一个 Module 只对外暴露它真正需要的 Interface；字段的默认值、顺序、非法值、生命周期
  和性能约束都是 Interface 的一部分。
- Component 是运行时状态的值模块，不是把旧类字段原样搬过来的浅薄容器。
- Definition/Registry 是不可变规则模块；同一个类型的规则只存一份，实例不能覆盖共享规则。
- Snapshot 是跨 tick、持久化或恢复的 seam；Snapshot 不暴露 Arch entity，不携带传输格式。
- Command 是状态改变的 seam；System 只产生 intent，Commit 才改变权威 store/grid。
- Adapter 只位于 WLD/legacy import 或 Server host seam；它不能成为 Simulation 的全局状态。
- 只有真正存在变化点时才保留 Adapter；不要为了未来的协议版本预先建立 wire adapter。
- 深模块的删除测试是：移除该模块后，复杂度是否会散落到多个调用方；如果只是转发字段，
  不要创建新模块。

### 5.3 每条字段卡必须填写的列

后续真正执行任何字段迁移前，必须为下方清单中的每个 ID 填完这些列。缺列即保持
`deferred` 或 `not-run`：

| 列 | 必填内容 |
| --- | --- |
| Legacy name | 旧字段/属性原名，保留大小写和下划线 |
| Source anchor | 文件、声明行、版本/hash 或现有证据路径 |
| Type shape | 原始类型、数组/列表/nullable/枚举/值对象形状 |
| Static/instance | 静态定义、世界实例、实体实例、按 tick 临时值或宿主值 |
| Default | 构造、Reset、导入、缺省版本和非法输入的实际值 |
| Mutability | 谁能写、何时写、是否单调、是否可撤销 |
| Lifetime | bootstrap、tick、entity lifetime、world lifetime、restart、host lifetime |
| Owner | 唯一 Definition/Component/Snapshot/System/Command/Host owner |
| Persistence | 必须保存、派生不保存、版本未知或不适用 |
| Exclusion | 若不迁移，写明 client/Protocol/host/unsupported 原因 |
| Status | `accepted-narrow`/`partial`/`deferred`/`host-only`/`excluded`/`not-run` |

### 5.4 禁止的形状

- 禁止 `MainFieldsComponent`、`GlobalServerData`、`MiscState`、`DataManager`、
  `Manager`、`Helper`、`Utility` 等隐藏 owner 的聚合对象。
- 禁止把所有旧数组变成一个可写 dictionary，再用索引名称伪装 slot parity。
- 禁止把 `Action`、`IEnumerator`、闭包、线程对象、socket、reader/writer 放进 ECS state。
- 禁止使用一个全局 `UnifiedRandom` 代替所有 worldgen/event/frame stream。
- 禁止把 Definition 中的值复制到 Component 后允许两边任意修改。
- 禁止把客户端输入设备、渲染缓存、音频句柄、Protocol frame 放入服务器状态。
- 禁止用字段同名、类型相同或编译通过作为迁移完成证据。

## 6. 组件总体设计清单

以下不是当前新增类型的要求，而是后续逐项实现时的组件契约。`现有窄 owner` 表示可以
复用；`目标模块` 表示尚未闭合时的设计方向。任何组件都必须只拿服务器必要字段。

### 6.1 World 模块

| Module / Component | 只保存什么 | 明确不保存什么 | 主要读者/写入边界 | 当前状态 |
| --- | --- | --- | --- | --- |
| `WorldMetadata` | WorldId、名称、尺寸、边界、section 几何、出生点、surface/rock layer、seed variant、generator version、mode | 文件路径、文件句柄、packet flags、客户端背景/视野 | `WorldBootstrap` 导入；World systems 只读；持久化 value snapshot | partial，多个字段 accepted-narrow |
| `WorldGrid` | 稠密 Tile/Wall/liquid 值、合法边界、section revision | 每 Tile 一个 entity、渲染缓存、wire layout | `WorldGridSnapshot` 读；Tile/Liquid commit 写 | partial |
| `WorldClock` | day/night、time、moon phase、pause、rate、tick cursor | visual-only clock、客户端帧计数 | `WorldClockSystem` 计算 transition | accepted-narrow/partial |
| `WorldClockSnapshot` | 可恢复的权威时钟值和版本 | 客户端动画时间、传输序号 | save/restart/import | partial |
| `WorldTimeRateSnapshot` | 权威 dayRate 与有界派生 update rate | `desiredWorldTilesUpdateRate` 的 wire/visual 表达 | rate policy 读 | accepted-narrow |
| `WorldRuleState` | seed/rule flags、PVP/game rule、hardmode 前置规则 | 纹理、音效、菜单或 wire flags | weather/progression/generation systems 读 | partial |
| `WorldProgressionState` | hardmode、boss/event progression、invasion/slime/meteor/lantern state | event message fields、UI warning text | progression systems 通过 commands 改 | partial |
| `WorldWeatherState` | rain state/amount/time、wind physics/counter、server weather transitions | cloud rendering、ambient effects | weather system + snapshot | partial |
| `WorldEnvironmentTransition` | 一次 tick 的 weather/time/progression transition | 客户端通知帧 | transition commit | partial |
| `WorldGenerationRequest` | 冻结 seed、world bounds、secret seed/rule inputs、difficulty、profile | generator object、thread flag、status text | bootstrap -> pipeline | partial |
| `WorldGenerationState` | generation stage、active/loading、stage cursor、bounded failure/recovery state | UI progress text、协议阶段编号 | pipeline lifecycle | partial |
| `GenerationCursorComponent` | stage-local cursor、iteration、source sequence | 全局静态计数器、客户端 cursor | pass system -> command buffer | partial |
| `LegacyPassRandomState` | stage/pass seed、stream version、cursor/checkpoint | `Main.rand` 全局实例、客户端 random | pass-local deterministic stream | deferred/partial |
| `WorldGenerationStageSnapshot` | stage boundary 的 immutable inputs/outputs | wire snapshot fields | stage handoff | partial |
| `WorldGridSnapshot` | stage/read/persistence 所需的 tile value copy 或 sparse bounded view | rendering/protocol projection | query/system read | partial |
| `TileReadSnapshot` | 单次查询的 active/type/wall/liquid/frame 值 | 可写 Tile 引用、全局缓存 | query seam | partial |
| `WorldGenerationCommandBuffer` | 有来源、优先级、simulation sequence 的 Tile/Wall/Liquid/Structure intents | packet commands、Action/IEnumerator | deterministic commit | partial |
| `LiquidWorldStateComponent` | liquid capacities、dirty sections、propagation budget、known liquid kinds | wire liquid encoding、client alpha | liquid systems | partial |
| `LiquidWorkItemComponent` | 坐标、kind、amount、source/priority、bounded work state | network cursor、packet queue | liquid input/propagation/commit | partial |

### 6.2 Player 模块

| Module / Component | 只保存什么 | 不保存什么 | 当前状态 |
| --- | --- | --- | --- |
| `PlayerIdentityComponent` | Player handle、持久化 profile key、实体关联身份 | `whoAmI` 的 wire 复制格式、packet slot | identity 保留，非本计划 Protocol owner |
| `PlayerAuthorityComponent` | 当前 session 的授权关系、command acceptance、permission、disconnect cleanup key | message ID、reader/writer、wire sequence | accepted narrow/host seam |
| `PlayerLifecycleComponent` | active、dead、respawn ticks、spawn phase、despawn/reconnect state | client connected frame、presentation state | verified narrow/partial |
| `PlayerTransformComponent` | position、velocity、facing、gravity direction | camera/screen/visual interpolation | partial/基础路径已有 |
| `PlayerMovementStateComponent` | grounded、wet/lava/honey/shimmer contact、fall/dash/jump transition、collision intent | 键盘/鼠标对象、渲染帧 | partial |
| `PlayerInputComponent` | 经服务器接受的 normalized move/jump/use/tile intent、tick/authority context | raw device state、packet layout、wire bit flags | partial；只保留业务意图 |
| `HealthComponent` | current/max life、damage/death transition 所需值 | health bar/UI color | verified narrow |
| `ManaComponent` | current/max mana、regen、potion/use delay | visual effect timer | verified/partial |
| `PlayerEnvironmentContactComponent` | drowning/breath、liquid contact、tile/environment contacts | scene lighting/ambient state | deferred/partial |
| `PlayerSpawnStateComponent` | spawn point、spawn validation、pending respawn/clear-area state | menu spawn screen | deferred/partial |
| `PlayerInventoryComponent` | runtime item slots、selected slot、stack revision、server ownership | old 58-slot alias、packet slot layout | partial；40 runtime 与 990 persistence 需分开 |
| `EquipmentComponent` | loadout、equipment item references、server stat contributors | dye/armor rendering projection | delegated/partial |
| `BuffCollectionComponent` | buff type、remaining ticks、source、immunity-relevant instance state | buff icon/tooltip/packet representation | partial/deferred |
| `PlayerStatModifierComponent` | equipment/buff/mount/food 对生命、防御、移动等的权威修正 | client-only stat display | partial |
| `PlayerTeleportStateComponent` | validated teleport intent、origin/destination、cooldown、commit phase | teleport visual effect/packet | deferred |
| `PlayerCooldownStateComponent` | item use、potion、hurt、dodge、interaction cooldown | animation-only counters | partial/deferred |
| `PlayerDeathDropStateComponent` | death cause、drop policy、once-only drop marker、pending world item intents | tombstone packet/visual death effect | deferred |

### 6.3 NPC 模块

| Module / Component | 只保存什么 | 不保存什么 | 当前状态 |
| --- | --- | --- | --- |
| `NpcDefinitionComponent` | type、base stats、category、difficulty、town/boss/chase capability、slot cost | network identity encoding、frame bytes | partial |
| `NpcAuthorityComponent` | server ownership、spawn source、damage authority、interaction authority | replication frame fields | partial |
| `NpcBehaviorStateComponent` | typed AI family state、behavior phase、movement intent、local behavior counters | 公共 `float[] ai` 兼容数组、client animation | partial |
| `NpcLifecycleComponent` | active、timeLeft、death/despawn reason、replacement eligibility、revision | sync cadence/net spam | partial，若干窄 slice 已有 |
| `NpcTransformComponent` | position、velocity、facing、collision flags | oldPos/oldRot render history | partial |
| `NpcTargetComponent` | player/NPC target handle、validity、selection reason | encoded target index | partial |
| `NpcCombatComponent` | life, damage, defense, immunity, friendly/hostile, hit/death markers | hit sound/visual effect | partial |
| `NpcHomeComponent` | homeless、home coordinates、home timeout、door/return intent | housing UI/dialogue text | partial |
| `NpcHomePublicationComponent` | authoritative home baseline and change marker for server systems | outbound sync cursor | partial |
| `NpcSpawnCycleStateComponent` | no-spawn-cycle intent、spawn eligibility/consumption state | message-based spawn notification | partial/deferred |
| `NpcInvasionParticipationComponent` | invasion group/points/wave contribution、death contribution | invasion packet flags | partial |
| `NpcSegmentLinkComponent` | root/parent/child relationship、segment order、shared-life intent | encoded segment slot | partial |
| `NpcLootStateComponent` | death loot source、once-only emission、drop authority | client loot animation | partial |
| `NpcActivityStateComponent` | CheckActive keep-alive, inactivity, active-player contribution, bounded range state | screen/PVS visibility cursor | partial |

NPC 中的完整 `AI_###` family、Boss phase、完整 invasion/event、Buff/DoT、town services/
housing/dialogue、CheckActive integration、worm movement/shared life 和完整 spawn scheduler
必须各自建立 owner；不能以 `NpcBehaviorStateComponent` 存一个大数组宣称完成。

### 6.4 Projectile 模块

| Module / Component | 只保存什么 | 不保存什么 | 当前状态 |
| --- | --- | --- | --- |
| `ProjectileDefinitionComponent` | type、base behavior、damage class、hostile/friendly capability、lifetime defaults | frame/render/network tables | partial |
| `ProjectileTransformComponent` | position、velocity、direction、bounds | rotation-only drawing state | partial |
| `ProjectileLifecycleComponent` | active、timeLeft、spawn/despawn reason、update budget | replication cadence | accepted narrow/partial |
| `ProjectileOwnerComponent` | player/NPC/world owner handle、authority relation | owner wire index | partial |
| `ProjectileCollisionComponent` | tile collide、water/liquid interaction、slope/collision policy | scene metrics/visual collision | partial |
| `ProjectileCombatComponent` | damage、original damage、knockback、friendly/hostile、penetration、hit result | hit sound/particle | partial |
| `ProjectileBehaviorStateComponent` | typed behavior family state、bounded AI counters、reflection/bounce intent | `ai[]`/`localAI[]` public compatibility arrays | partial |
| `ProjectileCooldownComponent` | restrike/local hit cooldown、attack cooldown、owner hit gate | network spam timers | deferred/partial |
| `ProjectileSpawnStateComponent` | validated spawn request、child spawn source、sentry/minion/trap capability when server consumer exists | spawn packet | partial/deferred |

仅保留 ownership、active/lifetime、位置/速度、collision、damage、hostility、penetration、
AI 所需状态和 spawn/despawn。`alpha`、light、rotation-only presentation、frame、oldPos、
oldRot、drawLayer、hide、Name、sound、Protocol projection 全部不是本模块字段。

### 6.5 Item 模块

| Module / Component | 只保存什么 | 不保存什么 | 当前状态 |
| --- | --- | --- | --- |
| `ItemDefinition` | ItemType、尺寸、堆叠上限、经济、稀有度、分类和模式规则 | 实例 stack、favorited、tooltip、颜色、wire fields | partial |
| `ItemCombatDefinition` | damage、knockback、crit、armor penetration、damage class、tag damage | combat text/sound | partial |
| `ItemEquipmentDefinition` | defense、equipment slot、life/mana/stat modifiers、mount/sentry capability | armor/dye draw state | partial |
| `ItemUseDefinition` | use style/time/animation、channel、auto reuse、shoot/ammo/place intent | client hand/animation frame | partial |
| `ItemPlacementDefinition` | create tile/wall、place style、tile boost、tool power、wall use time | tile placement packet layout | partial |
| `ItemRecoveryDefinition` | heal life/mana、buff type/time、potion delay、food/flask server effect | buff icon/color | partial |
| `ItemPrefixDefinition` | prefix stat deltas and eligibility | prefix display text | deferred/partial |
| `ItemInstanceStateComponent` | prefix、variant、paint/coating if authoritative、favorited、new/shiny only when persistence requires | tooltip/appearance cache | partial |
| `ItemStackComponent` | ItemType、quantity、stack invariants | global slot layout, protocol index | partial |
| `ItemWorldStateComponent` | active、position/velocity、pickup delay、owner/retention、revision | render hitbox, PVS cursor | partial |
| `InventoryComponent` | player-owned runtime slots and selected slot | legacy 58-slot alias | partial |
| `WorldItemComponent` | world item lifecycle and drop/pickup authority | client sync frame | partial |
| `ShopOfferDefinition` | only if a server-authoritative shop domain is approved: offer, price, stock, refresh | UI shop array and packet fields | deferred |

`Definition` 与 `Component` 不能混用：`type/maxStack/damage` 是 definition；`stack/prefix/
favorited/active/position` 是 instance/world state。Item 的 `stack` 虽然在旧 mapping 的
初始归属中可能被写成 Definition，执行时必须按实例语义复核，不能照抄旧归属表。

### 6.6 WorldObjects 模块

| Module / Component | 服务器必要字段 | 不保存什么 | 当前状态 |
| --- | --- | --- | --- |
| `ChestStateComponent` | chest identity、tile coordinates、active/valid、owner/lock、open state、contents、revision | open/close packet fields、client animation | partial |
| `SignStateComponent` | sign identity、coordinates、active、authoritative text、revision、edit authority | sign message layout、font/render state | partial |
| `TileEntityStateComponent` | entity type、coordinates、active/valid、linked entity、owner/lock、persistent values、revision | tile-entity packet DTO | partial/deferred |
| `DoorStateComponent` | tile anchor、open/closed/locked、mechanism authority、mutation revision | frame packet and animation | deferred/partial |
| `WireNetworkComponent` | wire/actuator/logic state、bounded traversal budget、mechanism intent | wire packet encoding、scratch broadcast list | partial |
| `ActuatorStateComponent` | tile coordinates、enabled/disabled, permission, last committed state | visual wire color | deferred |
| `TileObjectStateComponent` | object anchor/style/validity, footprint, persistence marker | client frame/render cache | partial |
| `TrainingDummyStateComponent` | dummy identity、tile anchor、active、linked NPC/entity、ownership、hitbox and revision | message-87 field layout and request/response fields | bounded partial |

World object 的交互入口必须是服务器授权的 domain command；Protocol request/response 字段
不进入组件，不能把一个 message handler 当作状态 owner。

### 6.7 Server host 模块

Server host 可保留 server 必要业务状态，但不把它提升成 Simulation Component：

| Host module | 允许保存 | 明确排除 |
| --- | --- | --- |
| `ServerLifecycleState` | booting/ready/stopping、world loaded、tick host state | packet state、wire cursor |
| `ServerSessionState` | session identity、bound Player handle、permission、liveness、timeout、disconnect cleanup | message ID、payload、transport frame |
| `ServerCommandIngress` | 已授权业务 command 的有界队列、tick/authority metadata | raw reader/writer、client device object |
| `WorldBootstrap` | load/import phase、failure/recovery decision、world ready state | UI status text、protocol handshake fields |
| `WorldSaveCoordinator` | save request、version、atomic save state、backup retention、recovery state | file handle、packet notification |
| `ServerLaunchOptions` | listen/server mode、capacity、paths、bounded host options | Simulation world rules、client menu state |

`dedServ`、`netMode`、`_targetNetMode`、`MaxTimeout`、`netPlayCounter`、`maxNetPlayers`、
address/port、menu flags等属于 host-only 或 excluded；“服务器使用”不等于“迁移到 ECS”。

## 7. Main.cs 服务器必要字段/属性逐项核对清单

### 7.1 使用说明

本节把当前 Main 迁移矩阵中的 server-required 子集拆成单独行。它不是把 696 个符号全部
塞回新的 `Main`，也不把客户端或 Protocol 声明算作待迁移项。

- `[ ]` 表示本计划要求未来为该行完成字段卡；不表示本轮已完成。
- `accepted-narrow` 只表示现有窄 owner 可复用；仍须保留本行的默认值、可变性、生命周期
  和持久化说明。
- `partial`、`deferred` 不得通过新建同名字段提升。
- `host-only` 表示字段可以在 Server host 出现，但不得进入 Simulation state。
- `excluded` 表示明确不迁移；排除项仍列出以防止错误回流。
- 所有表格中的 `Owner` 都是服务器域 owner；不允许填 Protocol/transport/replication
  类型作为 owner。

### 7.2 世界元数据、尺寸和坐标

| ID | 核对 | 旧字段/属性 | 服务器语义 | Owner | 形状/生命周期 | 持久化 | 当前状态与下一步 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| M-WM-001 | [x] | `worldName` | 世界实例名称，影响加载/保存和世界识别 | `WorldMetadata.Name` | string；world lifetime | 必须 | partial；缺省名和恢复边界仍 open |
| M-WM-002 | [ ] | `worldID` | 世界身份关联；不是待删除的普通业务字段 | `WorldMetadata.WorldId` | identity；world lifetime | 必须 | identity-preserved；不计入迁移分母 |
| M-WM-003 | [x] | `leftWorld` | 世界左边界坐标 | `WorldMetadata.LeftWorld` + `WorldCoordinateRules` | scalar；derived from width | 派生/按格式 | accepted-narrow；禁止静态读取 |
| M-WM-004 | [x] | `rightWorld` | 世界右边界坐标 | `WorldMetadata.RightWorld` + `WorldCoordinateRules` | scalar；derived from width | 派生/按格式 | accepted-narrow；极值恢复仍 partial |
| M-WM-005 | [ ] | `topWorld` | 世界上边界坐标 | `WorldMetadata` + `WorldCoordinateRules` | scalar；derived | 派生/按格式 | partial；补生成/交互使用方 |
| M-WM-006 | [ ] | `bottomWorld` | 世界下边界坐标 | `WorldMetadata` + `WorldCoordinateRules` | scalar；derived | 派生/按格式 | partial；补非法尺寸拒绝 |
| M-WM-007 | [x] | `maxTilesX` | 世界 Tile 宽度和稠密 grid 容量 | `WorldMetadata.Width` + `WorldGrid.Width` | int；world lifetime | 必须 | accepted-narrow；resize 被禁止 |
| M-WM-008 | [x] | `maxTilesY` | 世界 Tile 高度和稠密 grid 容量 | `WorldMetadata.Height` + `WorldGrid.Height` | int；world lifetime | 必须 | accepted-narrow；上下界已校验 |
| M-WM-009 | [x] | `sectionWidth` | section 宽度，供服务器分区/存储使用 | `WorldGrid.SectionWidth` | const/value；world lifetime | 按格式 | accepted-narrow；与实体/对象查询边界分开 |
| M-WM-010 | [x] | `sectionHeight` | section 高度，供服务器分区/存储使用 | `WorldGrid.SectionHeight` | const/value；world lifetime | 按格式 | accepted-narrow；不能拿客户端视野替代 |
| M-WM-011 | [x] | `maxSectionsX` | X 方向 section 容量 | `WorldMetadata.MaxSectionsX` + `WorldGrid` | derived int；world lifetime | 派生 | accepted-narrow；零尺寸由 metadata/grid 拒绝 |
| M-WM-012 | [x] | `maxSectionsY` | Y 方向 section 容量 | `WorldMetadata.MaxSectionsY` + `WorldGrid` | derived int；world lifetime | 派生 | accepted-narrow；section index 有界校验 |
| M-WM-013 | [ ] | `spawnTileX` | 世界出生点 X | `WorldMetadata` + `WorldSpawnPolicy` | mutable during bootstrap/events | 必须 | partial；补 fallback/clear-area 规则 |
| M-WM-014 | [ ] | `spawnTileY` | 世界出生点 Y | `WorldMetadata` + `WorldSpawnPolicy` | mutable during bootstrap/events | 必须 | partial；补 surface/solid 判定 |
| M-WM-015 | [ ] | `worldSurface` | 地表高度，生成/刷怪/出生规则输入 | `WorldMetadata` + `TerrainProfileComponent` | derived/frozen per world | 必须或可重建 | accepted-narrow；补 pass provenance |
| M-WM-016 | [ ] | `rockLayer` | 岩层高度，生成/刷怪/矿物规则输入 | `WorldMetadata` + `TerrainProfileComponent` | derived/frozen per world | 必须或可重建 | accepted-narrow；已接入 generation request，补恢复 |

### 7.3 种子、世界模式和生成规则

| ID | 核对 | 旧字段/属性 | 服务器语义 | Owner | 形状/生命周期 | 持久化 | 当前状态与下一步 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| M-SEED-001 | [ ] | `drunkWorld` | Remix/Drunk 生成和规则选择 | `WorldRuleState` + `WorldGenerationRequest` | bool；world lifetime | 必须 | deferred；补逐字段 source contract |
| M-SEED-002 | [ ] | `getGoodWorld` | Good World 规则输入 | `WorldMetadata.IsGoodWorld` + generation request | bool?；world lifetime | 必须/版本化 | accepted-narrow/partial；旧版本 unknown 保留 |
| M-SEED-003 | [ ] | `tenthAnniversaryWorld` | 十周年生成/规则分支 | `WorldRuleState` secret-seed flags | bool；world lifetime | 必须 | deferred；禁止 variant 字符串 alias |
| M-SEED-004 | [ ] | `dontStarveWorld` | DontStarve 生成/环境分支 | `WorldRuleState` + generation policies | bool；world lifetime | 必须 | partial；完整生成与天气消费 deferred |
| M-SEED-005 | [ ] | `notTheBeesWorld` | NotTheBees 规则/生成/陷阱分支 | `WorldRuleState` + `TrapGenerationGatePolicy` | bool；world lifetime | 必须 | partial；只保留已证实分支 |
| M-SEED-006 | [ ] | `remixWorld` | Remix 的生成、地形和事件分支 | `WorldRuleState` + terrain/generation definitions | bool；world lifetime | 必须 | partial；完整 pass order deferred |
| M-SEED-007 | [ ] | `noTrapsWorld` | 陷阱生成禁用规则 | `WorldMetadata` + `WorldRuleState` | bool；world lifetime | 必须 | accepted-narrow；生成副作用仍 deferred |
| M-SEED-008 | [ ] | `zenithWorld` | Zenith 组合规则 | `WorldRuleState` typed secret seeds | bool；world lifetime | 必须 | deferred；逐分支记录优先级 |
| M-SEED-009 | [ ] | `skyblockWorld` | Skyblock 世界规则/生成输入 | `WorldMetadata.IsSkyblockWorld` + request | bool；world lifetime | 必须 | partial；完整 generation parity deferred |
| M-SEED-010 | [ ] | `vampireSeed` | Vampirism seed 行为输入 | `WorldRuleState` typed variant | bool；world lifetime | 必须/版本化 | deferred；不得只映射名称 |
| M-SEED-011 | [ ] | `infectedSeed` | 感染世界规则输入 | `WorldRuleState` + infection policy | bool；world lifetime | 必须/版本化 | deferred；补 biome/感染 consumer |
| M-SEED-012 | [ ] | `teamBasedSpawnsSeed` | 团队出生/刷怪规则输入 | `WorldRuleState` + spawn policy | bool；world lifetime | 必须 | deferred；补 server-only team rule |
| M-SEED-013 | [ ] | `dualDungeonsSeed` | 双地牢生成/出生规则输入 | `WorldRuleState` + dungeon definitions | bool；world lifetime | 必须 | deferred；补 dungeon placement |
| M-SEED-014 | [ ] | `DefaultSeed` | 缺省世界种子或 seed parser default | `WorldSeed` definition/policy | immutable constant | 不单独保存 | partial；区分默认值与实例 seed |
| M-SEED-015 | [ ] | `WorldGeneratorVersion` | 生成器版本和恢复兼容选择 | `WorldMetadata` + `WorldGenerationRequest` | immutable value；world lifetime | 必须 | partial；必须进入 generation provenance |
| M-SEED-016 | [ ] | `gameMode` | 世界/服务器规则模式 | `WorldRuleState` / `WorldMetadata` | enum/value；world lifetime | 必须 | accepted-narrow/partial；补模式规则 |
| M-SEED-017 | [ ] | `GameModeInfo` | 模式派生参数和服务器限制 | `WorldRuleState` definition/query | immutable derived | 按规则 | partial；不存 UI 描述对象 |
| M-SEED-018 | [ ] | `Difficulty` | 难度对实体生命/伤害/生成的权威输入 | `WorldRuleState` + difficulty policy | enum/value；world lifetime | 必须 | partial；与 client display 分开 |
| M-SEED-019 | [ ] | `_gameModeDifficultyOverride` | 临时 difficulty multiplier | `WorldRuleState` scoped override | nullable scalar；bounded runtime | 通常不保存 | partial；finite/positive gate 已有方向 |

### 7.4 时钟、时间速率和 tick 状态

| ID | 核对 | 旧字段/属性 | 服务器语义 | Owner | 形状/生命周期 | 持久化 | 当前状态与下一步 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| M-CLK-001 | [x] | `dayLength` | 白天周期长度 | `WorldClock.DayLengthTicks` | immutable rule/value | 按格式或可重建 | accepted-narrow；版本默认值已集中 |
| M-CLK-002 | [x] | `nightLength` | 夜晚周期长度 | `WorldClock.NightLengthTicks` | immutable rule/value | 按格式或可重建 | accepted-narrow；跨日边界已集中 |
| M-CLK-003 | [x] | `dayTime` | 当前是否白天 | `WorldClock.IsDayTime` / `WorldClockQuery.IsDayTime` | bool；tick derived | 派生 | accepted-narrow；禁止独立双写 |
| M-CLK-004 | [x] | `time` | 当前日内 tick/time | `WorldClock.TimeOfDay` / `WorldClockQuery.TimeOfDay` | mutable scalar；tick | 必须/恢复 | accepted-narrow；溢出和暂停有校验 |
| M-CLK-005 | [x] | `moonPhase` | 月相和事件规则输入 | `WorldClock.MoonPhase` / `WorldClockQuery.MoonPhase` | enum/int；tick/day | 必须或可重建 | accepted-narrow；跨日计算已集中 |
| M-CLK-006 | [ ] | `GlobalTimeWrappedHourly` | 小时边界事件触发 marker | `WorldClockTransition` | transient bool/event | 不保存 | partial；只保留 server consumer |
| M-CLK-007 | [x] | `GlobalTimerPaused` | 全局世界时钟暂停规则 | `WorldClock.IsPaused` / `WorldClockQuery.IsPaused` | mutable bool；runtime | 依规则 | accepted-narrow；暂停恢复已接入 |
| M-CLK-008 | [x] | `dayRate` | 权威日夜时间倍率 | `WorldTimeRateSnapshot.Rate` | bounded scalar；runtime | 必须或 default | accepted-narrow；不与 update rate 混同 |
| M-CLK-009 | [x] | `desiredWorldTilesUpdateRate` | 服务器 Tile/world update rate 策略 | `WorldUpdateRatePolicy.GetRate` | derived bounded value | 不直接保存 | accepted-narrow；上限/冻结规则已定 |
| M-CLK-010 | [x] | `GameUpdateCount` | 服务器 tick cursor/统计输入 | `WorldGameUpdateCountProjection` + `WorldGameUpdateCountQuery` | monotonic counter；host/world | 可选 checkpoint | accepted-narrow；不得回流为全局静态业务字段 |
| M-CLK-011 | [ ] | `timeForVisualEffects` | 只供视觉插值的时间 | 无 server owner | projection-only | 不保存 | excluded；不得以时钟字段迁入 Simulation |

### 7.5 天气、降雨和风

| ID | 核对 | 旧字段/属性 | 服务器语义 | Owner | 形状/生命周期 | 持久化 | 当前状态与下一步 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| M-WEA-001 | [x] | `raining` | 是否下雨，影响世界规则/NPC/环境 | `WorldWeatherState.IsRaining` | mutable bool；runtime | 需要时 | accepted-narrow；重启持久化仍 partial |
| M-WEA-002 | [x] | `rainTime` | 降雨剩余/累计时间 | `WorldWeatherState.RainTime` | mutable counter；tick | 必须或恢复 | accepted-narrow；重启持久化仍 partial |
| M-WEA-003 | [x] | `maxRaining` | 降雨最大强度 | `WorldWeatherState.MaximumRainStrength` | bounded scalar；runtime | 可重建/保存 | accepted-narrow；随机冷却仍 partial |
| M-WEA-004 | [x] | `oldMaxRaining` | 雨量变化前基线 | `WorldEnvironmentTransition.PreviousMaximumRainStrength` | transient baseline；tick | 不保存 | accepted-narrow；仅用于服务端 transition 差异 |
| M-WEA-005 | [x] | `windSpeedCurrent` | 当前服务器风速，影响沙尘/世界行为 | `WorldRuleState.WindSpeedCurrent` / `WorldWeatherState.WindSpeedCurrent` | mutable scalar；tick | 必须或重建 | accepted-narrow；边界校验与推进已接入，重启恢复仍 partial |
| M-WEA-006 | [x] | `windSpeedTarget` | 风速目标 | `WorldRuleState.WindSpeedTarget` / `WorldWeatherState.WindSpeedTarget` | mutable scalar；tick | 必须或重建 | accepted-narrow；边界校验与命令接入已完成，seeded draw 仍 partial |
| M-WEA-007 | [ ] | `windCounter` | 风更新计时/随机推进状态 | `WorldWeatherState` | mutable counter；tick | 需要重启一致性 | partial；记录 stream ownership |
| M-WEA-008 | [ ] | `extremeWindCounter` | 极端风状态计时 | `WorldWeatherState` | mutable counter；event lifetime | 需要时 | partial；补结束 transition |
| M-WEA-009 | [ ] | `windPhysics` | 是否启用风物理规则 | `WorldRuleState`/weather policy | bool；world/runtime | 依规则 | partial；排除视觉强度 |
| M-WEA-010 | [ ] | `windPhysicsStrength` | 风物理服务器强度 | `WorldWeatherState` | bounded scalar；tick | 可重建 | partial；区分 visual wind |
| M-WEA-011 | [ ] | `lowWindLimit` | 风速 clamp 下限/环境规则 | `WorldWeatherDefinition` | immutable scalar | 不单独保存 | partial；补 source default |
| M-WEA-012 | [ ] | `cloudAlpha` | 云层渲染透明度 | 无 server owner | visual scalar | 不保存 | excluded；不进入天气组件 |

### 7.6 世界进度、事件、侵袭和 Slime Rain

| ID | 核对 | 旧字段/属性 | 服务器语义 | Owner | 形状/生命周期 | 持久化 | 当前状态与下一步 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| M-PRG-001 | [ ] | `hardMode` | 世界进度总开关 | `WorldProgressionState` | mutable bool；world lifetime | 必须 | accepted-narrow/partial |
| M-PRG-002 | [ ] | `bloodMoon` | Blood Moon 活动状态 | `WorldProgressionState` | event state | 必须或恢复 | partial；补 start/end side effects |
| M-PRG-003 | [ ] | `eclipse` | Eclipse 活动状态 | `WorldProgressionState` | event state | 必须或恢复 | partial |
| M-PRG-004 | [x] | `pumpkinMoon` | Pumpkin Moon 活动状态 | `WorldProgressionState.IsPumpkinMoon` | event state | 必须或恢复 | accepted-narrow；start/end side effects仍 partial |
| M-PRG-005 | [x] | `snowMoon` | Frost Moon 活动状态 | `WorldProgressionState.IsSnowMoon` | event state | 必须或恢复 | accepted-narrow；start/end side effects仍 partial |
| M-PRG-006 | [ ] | `invasionType` | 当前 invasion 类型 | `WorldProgressionState` | enum/int；event lifetime | 必须 | accepted-narrow/partial |
| M-PRG-007 | [ ] | `invasionX` | invasion 位置/方向坐标 | `WorldProgressionState` | scalar；event lifetime | 必须或恢复 | accepted-narrow/partial |
| M-PRG-008 | [ ] | `invasionSize` | 当前 invasion 剩余规模 | `WorldProgressionState` | counter；event lifetime | 必须 | accepted-narrow/partial |
| M-PRG-009 | [ ] | `invasionDelay` | invasion spawn/update delay | `WorldProgressionState` | counter；tick | 必须/恢复 | accepted-narrow/partial |
| M-PRG-010 | [x] | `invasionWarn` | invasion warning 计时/规则状态 | `WorldProgressionState.InvasionWarningTicks` + `WorldInvasionWarningSystem` | transient counter | 通常不保存 | accepted-narrow；非 UI 文本，行为输入接线仍 partial |
| M-PRG-011 | [ ] | `invasionSizeStart` | invasion 初始规模 | `WorldProgressionState` | immutable per event | 可按事件恢复 | partial |
| M-PRG-012 | [ ] | `invasionProgress` | invasion 当前进度 | `WorldProgressionState` | counter；event lifetime | 必须 | accepted-narrow/partial |
| M-PRG-013 | [ ] | `invasionProgressMax` | invasion 进度上限 | `WorldProgressionState` | counter；event lifetime | 必须或可重建 | accepted-narrow/partial |
| M-PRG-014 | [ ] | `invasionProgressWave` | invasion 波次/进度派生值 | `WorldProgressionState` + query | derived int；tick | 不单独保存 | accepted-narrow；不能靠同步字段证明 |
| M-PRG-015 | [x] | `slimeRainTime` | Slime Rain 剩余时间 | `WorldProgressionState.SlimeRainTimeTicks` | counter；event lifetime | 必须或恢复 | accepted-narrow；重启持久化仍 partial |
| M-PRG-016 | [x] | `slimeRain` | Slime Rain 活动状态 | `WorldProgressionQuery.IsSlimeRaining` | bool；event lifetime | 必须 | accepted-narrow；重启持久化仍 partial |
| M-PRG-017 | [x] | `slimeRainKillCount` | Slime Rain kill progress | `WorldProgressionState.SlimeRainKillCount` / `WorldProgressionQuery.SlimeRainKillCount` | counter；event lifetime | 必须 | partial；状态边界已完成，NPC death 增量 consumer 尚未接入 |
| M-PRG-018 | [x] | `slimeWarningTime` | Slime warning 计时 | `WorldProgressionState.SlimeRainWarningTicks` | transient counter | 通常不保存 | accepted-narrow；不含 warning message |
| M-PRG-019 | [x] | `slimeWarningDelay` | Slime warning delay policy | `WorldSlimeRainWarningPolicy.DefaultDelayTicks` | immutable/scoped | 不单独保存 | accepted-narrow |
| M-PRG-020 | [x] | `isThereAWorldSurface` | 是否存在可用世界表面 | `WorldSurfaceQuery.IsThereAWorldSurface` | derived bool；query | 不保存 | accepted-narrow；Legacy `worldSurface > 50.0` |
| M-PRG-021 | [x] | `lanternNight` | Lantern Night 规则状态 | `WorldProgressionState.IsLanternNight` / `WorldProgressionQuery.IsLanternNight` | event state | 必须或可重建 | partial；restart persistence 与 presentation 分开 |
| M-PRG-022 | [x] | `spawnMeteor` | Meteor scheduled/eligible state | `WorldProgressionState.IsMeteorScheduled` / `WorldProgressionQuery.IsMeteorScheduled` | mutable bool/state | 必须 | partial；generation side effect deferred |
| M-PRG-023 | [ ] | `worldCleared` | 世界清理/生成完成标记 | `WorldGenerationState` 或 host lifecycle | bool；bootstrap lifetime | 可选 | deferred；没有 consumer 不伪迁移 |

### 7.7 WorldGrid、Tile、Wall 和 Liquid 状态

| ID | 核对 | 旧字段/属性 | 服务器语义 | Owner | 形状/生命周期 | 持久化 | 当前状态与下一步 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| M-GRID-001 | [ ] | `tile` | 稠密 Tile/Wall 权威值存储 | `WorldGrid` / `WorldTile` | dense storage；world lifetime | 必须 | partial；禁止复制为每 Tile entity |
| M-GRID-002 | [ ] | `liquid` | 稠密液体值/类型存储 | `WorldGrid` + `LiquidWorldStateComponent` | dense value；world lifetime | 必须 | partial；补 -1/-2 side effects |
| M-GRID-003 | [ ] | `liquidBuffer` | 液体传播工作集/缓冲 | `LiquidWorkItemComponent` + liquid queue | bounded work state；tick | 不保存或 checkpoint | partial；禁止匿名全局 list |
| M-GRID-004 | [ ] | `liquidAlpha` | 若被服务器规则使用的液体透明/状态值 | `LiquidWorldStateComponent` only if consumer exists | scalar；runtime | 按 consumer | deferred；纯视觉用途直接 excluded |
| M-GRID-005 | [ ] | `maxLiquidTypes` | legacy 液体容量边界 | `LegacyLiquidTypeCapacity` definition | const `15`; process/compat | 不作为行为状态 | accepted-narrow；不宣称 15 种行为已实现 |
| M-GRID-006 | [ ] | `waterStyle` | 水体环境规则/生成选择 | `WorldRuleState` + liquid/environment definition | enum/value；world lifetime | 依世界格式 | partial；渲染 style 不进入 |
| M-GRID-007 | [ ] | `Setting_UseReducedMaxLiquids` | 服务器 liquid budget 模式 | `LiquidWorldStateComponent`/policy | bool；runtime | 可选 | deferred；补真实 consumer |
| M-GRID-008 | [ ] | `tileFrame` | Tile frame 的 server world mutation state | `TileFrameCommand` + `TileFrameSystem` | command-local/commit | 不保存 scratch | partial；完整 framing deferred |
| M-GRID-009 | [ ] | `tileFrameCounter` | frame update budget/counter | `TileFrameBudget`/frame commit | bounded transient | 不保存 | partial；禁止静态 scratch |
| M-GRID-010 | [ ] | `sectionManager` | section index/revision/access state | `WorldGrid` section store | world state; section lifetime | 必须/版本化 | partial；不接收 PVS cursor |
| M-GRID-011 | [ ] | `tileFrameImportant` | frame-important server geometry classification | `TileFrameImportantRegistry` | immutable table | 不保存 | accepted-narrow；只承接静态分类 |
| M-GRID-012 | [ ] | `wallLargeFrames` | wall frame-size 规则，若 server framing consumer 需要 | `LegacyLargeFrameWallRegistry` | immutable sparse table | 不保存 | accepted-narrow；不承接渲染 |

### 7.8 实体存储、容量和派生统计

| ID | 核对 | 旧字段/属性 | 服务器语义 | Owner | 形状/生命周期 | 持久化 | 当前状态与下一步 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| M-ENT-001 | [ ] | `player` | Player 实体 store | `PlayerStore` | bounded store；world/server lifetime | 通过 snapshot | partial；补 slot/reuse |
| M-ENT-002 | [ ] | `npc` | NPC 实体 store | `NpcStore` | bounded store；world lifetime | 通过 snapshot | partial；补 active/tombstone |
| M-ENT-003 | [ ] | `projectile` | Projectile 实体 store | `ProjectileStore` | bounded store；tick/world lifetime | 通过 snapshot | partial；补 spawn/despawn |
| M-ENT-004 | [ ] | `item` | World item store | `WorldItemStore` | bounded store；world lifetime | 通过 snapshot | partial；补 pickup race |
| M-ENT-005 | [ ] | `chest` | Chest world-object store | Chest domain store/snapshot | bounded store；world lifetime | 必须 | partial；40-slot/revision |
| M-ENT-006 | [ ] | `sign` | Sign world-object store | Sign domain state/snapshot | bounded store；world lifetime | 必须 | partial；text/authority |
| M-ENT-007 | [ ] | `maxPlayers` | Simulation player capacity | `SimulationEntityLimits`/`WorldEntityLimits` | immutable config；server lifetime | host config | accepted-narrow；与 maxNetPlayers 分开 |
| M-ENT-008 | [ ] | `maxNPCs` | NPC capacity | `SimulationEntityLimits` | immutable config；server lifetime | host/world config | accepted-narrow；补 rejection |
| M-ENT-009 | [ ] | `maxProjectiles` | Projectile capacity | `SimulationEntityLimits` | immutable config；server lifetime | host/world config | accepted-narrow |
| M-ENT-010 | [ ] | `maxItems` | World item capacity | `SimulationEntityLimits` | immutable config；server lifetime | host/world config | accepted-narrow |
| M-ENT-011 | [ ] | `maxChests` | Chest capacity | `SimulationEntityLimits` | immutable config；world lifetime | world config | accepted-narrow |
| M-ENT-012 | [ ] | `gore` | 仅视觉 gore 实例 | 无 server owner | client/visual lifetime | 不保存 | excluded；不创建 GoreComponent |
| M-ENT-013 | [ ] | `rain` | 仅视觉雨滴实例 | 无 server owner | client/visual lifetime | 不保存 | excluded；天气规则另核对 |
| M-ENT-014 | [ ] | `dust` | 粒子实例 | 无 server owner | client/visual lifetime | 不保存 | excluded |
| M-ENT-015 | [ ] | `star` | 星空实例 | 无 server owner | client/visual lifetime | 不保存 | excluded |
| M-ENT-016 | [ ] | `cloud` | 云实例/缓存 | 无 server owner | client/visual lifetime | 不保存 | excluded |
| M-ENT-017 | [ ] | `combatText` | 战斗文字展示缓存 | 无 server owner | client/UI lifetime | 不保存 | excluded；伤害结果另由 combat state 表达 |
| M-ENT-018 | [ ] | `ActivePlayersCount` | 当前 active player 派生统计 | current tick query | derived; tick | 不保存 | partial；禁止可写 static cache |
| M-ENT-019 | [ ] | `SleepingPlayersCount` | sleeping player 统计/世界规则输入 | player query/snapshot | derived; tick | 不保存 | partial |
| M-ENT-020 | [ ] | `AnyActiveBossNPC` | boss 存在性派生查询 | NPC definition/lifecycle query | derived; tick | 不保存 | partial |
| M-ENT-021 | [ ] | `HadAnActiveInteractableProjectile` | 当前 tick 交互 projectile 统计 | Projectile query | derived; tick | 不保存 | partial |
| M-ENT-022 | [ ] | `checkForSpawns` | spawn scheduler 是否应运行 | `NpcSpawnCycleState`/world policy | mutable tick state | 通常不保存 | partial；补 tick order |
| M-ENT-023 | [ ] | `ProjectileUpdateLoopIndex` | projectile update phase cursor | `SimulationTickContext` | transient counter | 不保存 | partial；非 wire cursor |
| M-ENT-024 | [ ] | `lastItemUpdate` | world item update cursor/last slot | `WorldItemSystem` local state | tick-local | 不保存 | deferred；按 store order 重建 |
| M-ENT-025 | [ ] | `maxItemUpdates` | item update budget | `WorldItemUpdateBudgetPolicy` | immutable/bounded | 不保存 | deferred；需证明 server consumer |

### 7.9 服务器使用的 Definition/Registry 字段

静态表不是实体组件；下列每个表都要独立核对容量、默认值、越界值、来源和 server
consumer。已形成 exact registry 的行仍只承接该表，不外推到相邻语义。

| ID | 核对 | 旧字段/属性 | 目标 Definition/Registry | 服务器必要用途 | 当前状态 |
| --- | --- | --- | --- | --- | --- |
| M-DEF-001 | [ ] | `projHostile` | `LegacyProjectileHostileRegistry` | Projectile target/damage eligibility | accepted-narrow/partial |
| M-DEF-002 | [ ] | `projHook` | `LegacyProjectileHookRegistry` | hook ownership/collision behavior | accepted-narrow/partial |
| M-DEF-003 | [ ] | `projPet` | `LegacyPetProjectileRegistry` | pet spawn/category rule | accepted-narrow；不替代 minion AI |
| M-DEF-004 | [ ] | `projFrames` | `LegacyProjectileFrameRegistry` | 只有 server behavior 明确消费时保留 | accepted-narrow；不承接动画 |
| M-DEF-005 | [ ] | `npcFrameCount` | `LegacyNpcFrameRegistry` | 只有行为/碰撞需要时保留 | accepted-narrow；不承接绘制 |
| M-DEF-006 | [ ] | `slimeRainNPC` | `LegacySlimeRainNpcRegistry` | Slime Rain NPC category | accepted-narrow；不等于完整 event |
| M-DEF-007 | [ ] | `npcCatchable` | `LegacyCatchableNpcRegistry` | 捕捉资格 | partial |
| M-DEF-008 | [ ] | `pvpBuff` | `LegacyPvpBuffRegistry`/combat rule | PVP 伤害/状态资格 | partial；不迁移 message relay 字段 |
| M-DEF-009 | [ ] | `persistentBuff` | `LegacyPersistentBuffRegistry` | reset/save 时 persistence rule | accepted-narrow |
| M-DEF-010 | [ ] | `meleeBuff` | `LegacyMeleeBuffRegistry` | melee buff replacement rule | accepted-narrow |
| M-DEF-011 | [ ] | `debuff` | `LegacyDebuffRegistry` | debuff classification | accepted-narrow/partial |
| M-DEF-012 | [ ] | `vanityPet` | `LegacyVanityPetBuffRegistry` | vanity pet classification | accepted-narrow；pet behavior deferred |
| M-DEF-013 | [ ] | `lightPet` | `LegacyLightPetBuffRegistry` | light pet classification | accepted-narrow；不含光照渲染 |
| M-DEF-014 | [ ] | `buffNoSave` | `LegacyBuffNoSaveRegistry` | reset/persistence exclusion | accepted-narrow/partial |
| M-DEF-015 | [ ] | `buffNoTimeDisplay` | `LegacyBuffNoTimeDisplayRegistry` | server effect display classification only if consumer exists | accepted-narrow；无 UI owner |
| M-DEF-016 | [ ] | `tileSolid` | `LegacySolidTileRegistry` | collision/placement/WorldGen | accepted-narrow；modern extra set 不混入 |
| M-DEF-017 | [ ] | `tileSolidTop` | `LegacySolidTopTileRegistry` | platform/placement collision | accepted-narrow |
| M-DEF-018 | [ ] | `tileContainer` | `LegacyTileContainerRegistry` | chest/container placement | accepted-narrow |
| M-DEF-019 | [ ] | `tileSign` | `LegacySignTileRegistry` | sign validity/placement | accepted-narrow |
| M-DEF-020 | [ ] | `tileRope` | `TileRopeQuery`/legacy rope registry | rope endpoint/placement behavior | accepted-narrow |
| M-DEF-021 | [ ] | `tileLavaDeath` | `TileDefinition.LavaDestroysTile` | liquid damage/destruction | accepted-narrow |
| M-DEF-022 | [ ] | `tileWaterDeath` | `TileDefinition.WaterDestroysTile` | water contact destruction | accepted-narrow |
| M-DEF-023 | [ ] | `tileFlame` | `TileDefinition` server hazard rule | fire/hazard interaction if consumer exists | partial/deferred |
| M-DEF-024 | [ ] | `tileLighted` | `LegacyLightedTileRegistry` only if server consumer exists | generation/authoritative hazard classification | accepted-narrow; client light excluded |
| M-DEF-025 | [ ] | `tileObsidianKill` | `LegacyObsidianKillTileRegistry` | liquid conversion/destruction rule | accepted-narrow |
| M-DEF-026 | [ ] | `tileOreFinderPriority` | `LegacyOreFinderPriorityRegistry` | server tool/query result | accepted-narrow |
| M-DEF-027 | [ ] | `tileMergeDirt` | `LegacyMergeDirtTileRegistry` | tile merge eligibility | accepted-narrow |
| M-DEF-028 | [ ] | `tileMerge` | `LegacyTileMergeRegistry` | server frame/merge algorithm | accepted-narrow；算法另行核对 |
| M-DEF-029 | [ ] | `tileBrick` | `LegacyBrickTileRegistry` | brick classification/WorldGen | accepted-narrow |
| M-DEF-030 | [ ] | `wallHouse` | `LegacyHouseWallRegistry` | housing wall eligibility | accepted-narrow；housing behavior deferred |
| M-DEF-031 | [ ] | `wallLargeFrames` | `LegacyLargeFrameWallRegistry` | wall frame-size value | accepted-narrow；不承接渲染 |
| M-DEF-032 | [ ] | `tileNoAttach` | `TileDefinitionRegistry.IsNoAttach`/legacy set | placement attachment rule | accepted-narrow |
| M-DEF-033 | [ ] | `tileCut` | legacy cut target registry | server tool/world mutation | accepted-narrow |
| M-DEF-034 | [ ] | `tileHammer` | legacy hammer target registry | server tool/world mutation | accepted-narrow |
| M-DEF-035 | [ ] | `tileAxe` | legacy axe target registry | server tool/tree mutation | accepted-narrow |
| M-DEF-036 | [ ] | `tileMoss` | `MossTileTypeRegistry` | moss generation/placement | accepted-narrow |
| M-DEF-037 | [ ] | `tileStone` | `LegacyTileRunnerTargetRegistry` | TileRunner target classification | accepted-narrow |
| M-DEF-038 | [ ] | `tileDungeon` | dungeon tile definitions | dungeon replacement/eligibility | accepted-narrow |
| M-DEF-039 | [ ] | `wallDungeon` | dungeon wall definitions | dungeon replacement/eligibility | accepted-narrow |
| M-DEF-040 | [ ] | `tileBouncy` | `LegacyBouncyTileRegistry` | authoritative bounce rule | accepted-narrow |
| M-DEF-041 | [ ] | `tileAlch` | `LegacyAlchemicalTileRegistry` | harvest eligibility | accepted-narrow |
| M-DEF-042 | [ ] | `tilePile` | `LegacyPileTileRegistry` | pile placement/target | accepted-narrow |
| M-DEF-043 | [ ] | `tileSand` | `ConversionSandTileRegistry` | sand conversion/generation | accepted-narrow |
| M-DEF-044 | [ ] | `tileCracked` | `TileSolidityOverrideQuery` | cracked brick solidity | accepted-narrow |
| M-DEF-045 | [ ] | `wallLight` | `LegacyWallLightRegistry` | server wall environment query when consumed | accepted-narrow |
| M-DEF-046 | [ ] | `tileBlendAll` | no Simulation owner unless a server rule consumer is proven | render blend only in current use | excluded/deferred |
| M-DEF-047 | [ ] | `tileShine` | no Simulation owner | shine/render value | excluded |
| M-DEF-048 | [ ] | `tileShine2` | no Simulation owner | shine/render membership | excluded |
| M-DEF-049 | [ ] | `tileGlowMask` | no Simulation owner | glow/render mask | excluded |
| M-DEF-050 | [ ] | `anglerQuestItemNetIDs` | quest domain only if server quest domain approved | quest selection/value, not wire IDs | deferred |
| M-DEF-051 | [ ] | `townNPCCanSpawn` | `LegacyTownNpcSpawnCandidateRegistry` | candidate universe only | partial/deferred |

### 7.10 加载、持久化、随机和延迟队列

| ID | 核对 | 旧字段/属性 | 目标 owner | 是否进入 Simulation | 当前状态与动作 |
| --- | --- | --- | --- | --- | --- |
| M-OPS-001 | [ ] | `WorldFileMetadata` | `LegacyWorldMetadata`/`WorldBootstrap` | 只以 value snapshot 进入 | partial；版本差异显式保留 |
| M-OPS-002 | [ ] | `WorldPath` | `WorldBootstrap`/`WorldSaveCoordinator` | 否，host-only | host-only；Simulation 不访问文件 |
| M-OPS-003 | [ ] | `saveTime` | `WorldSaveCoordinator` | 否或只记录 save transition value | host-only；不作为世界规则 |
| M-OPS-004 | [ ] | `AutogenProgress` | `WorldBootstrap` generation status | 否，status/UI 不入 state | host-only/excluded |
| M-OPS-005 | [ ] | `WorldRollingBackupsCountToKeep` | `WorldSaveCoordinator` bounded policy | 否 | accepted-narrow host-only；保留有界策略 |
| M-OPS-006 | [ ] | `rand` | domain-scoped deterministic random stream | 不保留 global field | deferred；拆 world/event/generation streams |
| M-OPS-007 | [ ] | `TileFrameSeed` | frame/generation scoped seed only if server consumer proven | 不复制 client startup random | deferred；不能直接 alias WorldSeed |
| M-OPS-008 | [ ] | `_tempSeededRandom` | pass-local seeded stream | 否，scope 到 pass | partial/deferred；记录 seed/version/cursor |
| M-OPS-009 | [ ] | `_drawRand` | draw-local deterministic stream | 否，scope 到 operation | deferred；不建 global singleton |
| M-OPS-010 | [ ] | `DelayedProcesses` | typed server command/intent queue if authoritative | 不放 `IEnumerator` | partial；逐项确认 server authority |
| M-OPS-011 | [ ] | `DelayedProcessesInGame` | typed tick queue if authoritative | 不放 legacy coroutine | partial；补 tick/ordering |
| M-OPS-012 | [ ] | `_mainThreadActions` | Server lifecycle queue | 否，host-only | host-only/deferred；不将 `Action` 放 ECS |
| M-OPS-013 | [ ] | `OnEnginePreload` | host lifecycle event | 否 | excluded/host-only |
| M-OPS-014 | [ ] | `OnEngineLoad` | host lifecycle event | 否 | excluded/host-only |
| M-OPS-015 | [ ] | `OnTickForThirdPartySoftwareOnly` | host callback | 否 | excluded |
| M-OPS-016 | [ ] | `OnTickForInternalCodeOnly` | host callback | 否 | excluded |

### 7.11 WorldObjects 入口和服务器 Host 字段

| ID | 核对 | 旧字段/属性 | 目标 owner | 是否进入 Simulation | 当前状态与动作 |
| --- | --- | --- | --- | --- | --- |
| M-OBJ-001 | [ ] | `chest` | Chest state/store/snapshot | 是，value state | partial；补 lock/contents/revision |
| M-OBJ-002 | [ ] | `sign` | Sign state/store/snapshot | 是，value state | partial；补 text/authority |
| M-OBJ-003 | [ ] | `sectionManager` | `WorldGrid` section state | 是，非 PVS cursor | partial |
| M-OBJ-004 | [ ] | `WireNetwork` | `WireNetworkComponent` | 是，server mechanism state | partial；补 budget/commit |
| M-OBJ-005 | [ ] | `sittingManager` | Player/NPC sitting state only if behavior consumer exists | 只留 typed state | deferred；不复制 manager |
| M-OBJ-006 | [ ] | `sleepingManager` | Player sleep authority state | 只留 typed state | partial/deferred |
| M-OBJ-007 | [ ] | `TeleportPylonsSystem` | validated server teleport domain | 只留 domain state | deferred；不建立 message field |
| M-OBJ-008 | [ ] | `door`/door caches | `DoorStateComponent` | 是，world object state | deferred/partial |
| M-OBJ-009 | [ ] | `TileEntity` caches | `TileEntityStateComponent` | 是，persistent value state | partial/deferred |
| M-HOST-001 | [ ] | `dedServ` | `ServerLaunchOptions`/host lifecycle | 否，host-only | host-only |
| M-HOST-002 | [ ] | `netMode` | server host mode policy | 否，host-only | host-only；不是 world rule |
| M-HOST-003 | [ ] | `_targetNetMode` | server startup transition | 否 | host-only |
| M-HOST-004 | [ ] | `MaxTimeout` | session liveness policy | 否，Server host | host-only |
| M-HOST-005 | [ ] | `netPlayCounter` | session/host lifecycle counter | 否 | host-only；非 Simulation tick |
| M-HOST-006 | [ ] | `verboseNetplay` | host diagnostics/config | 否 | excluded/host-only |
| M-HOST-007 | [ ] | `stopTimeOuts` | host shutdown/liveness override | 否 | host-only |
| M-HOST-008 | [ ] | `maxNetPlayers` | host connection limit | 否；不等于 maxPlayers | deferred-host |
| M-HOST-009 | [ ] | `defaultIP` | host bind config | 否 | host-only |
| M-HOST-010 | [ ] | `getIP`/`getPort` | host endpoint lookup | 否 | host-only |
| M-HOST-011 | [ ] | `menuServer` | client/menu state | 否 | excluded |
| M-HOST-012 | [ ] | `menuMultiplayer` | client/menu state | 否 | excluded |
| M-HOST-013 | [ ] | `npcStreamSpeed` | old client/network stream throttle | 否 | deferred-host；无 Simulation consumer |
| M-HOST-014 | [ ] | `Assets`/content repositories | resource loading | 否 | excluded；Simulation 不引用 |

### 7.12 Main 明确排除项逐项回查

下列项目只用于防回流检查；它们没有 server ECS owner。若未来发现某个同名值确实改变
权威规则，必须新增独立 source-backed 字段卡，不能直接解除本行排除。

- [ ] `treeBGSet0`、`treeBGSet1`、`treeBGSet2`、`treeBGSet3`：天空/背景选择缓存，excluded。
- [ ] `corruptBG`、`jungleBG`、`snowBG`、`hallowBG`、`crimsonBG`、`mushroomBG`、
  `underworldBG`、`desertBG`、`oceanBG`：背景渲染值，excluded。
- [ ] `treeX`、`treeStyle`、`caveBackX`、`caveBackY`、`maxStars`、`numStars`、
  `maxClouds`、`numClouds`：presentation/cache，excluded。
- [ ] `graphics`、`GameViewMatrix`、`Camera`、`screenPosition`、`screenWidth`、
  `screenHeight`：图形/镜头，excluded。
- [ ] `musicPitch`、`musicFade`、`ambient*`、`ParticleSystem_*`、`shimmerAlpha`、
  `BlackFadeIn`：音频/粒子/效果，excluded。
- [ ] `mouseX`、`mouseY`、`mouseRight`、`mouseRightRelease`、`keyState`、gamepad/local
  cursor、`MouseScreen`：设备输入对象，excluded；只允许 normalized intent 进入 server。
- [ ] `SmartCursorWanted_*`、`SmartInteractTileCoords*`、`TileInteraction*`、
  `cursorOverride`、`mouseColor*`：客户端交互/光标状态，excluded。
- [ ] `MenuUI`、`InGameUI`、`gameMenu`、`motd`、`statusText`、`helpText`、
  `AnnouncementBox*`、`HoverItem`、`showItemText`：UI/文本，excluded。
- [ ] `currentNPCShowingChatBubble`、`chatMonitor`、`Pings`、`LocalGolfState`：社交/表现，
  excluded；若未来有服务器领域，另建 domain，不回填 Main。
- [ ] `fpsTimer`、`dedServFPS`、`dedServCount*`、`renderCount`、`updatesCountedForFPS`、
  `NoPooling`、`CollectGen0EveryFrame`：诊断/平台，excluded。
- [ ] 所有 `packet`、`message`、`payload`、`reader`、`writer`、`wire`、`bit`、`cursor`、
  `Sync*`、`NetMessage` 对应字段：Protocol exclusion，excluded。

## 8. Player.cs 服务器必要字段/属性逐项核对清单

Player 的完整 archive declaration inventory 远大于服务器核心。这里仅留下影响服务器
移动、生命、权限、库存、装备、状态效果、交互、重生和持久化的字段；渲染/社交/设备/协议
字段逐项放到本节末尾的排除清单。PB-001 至 PB-011 的 partial/blocked 结果必须保留，
不能因核心字段已有 owner 就把专用机制改写成 complete。

### 8.1 身份、生命周期和基础运动

| ID | 核对 | 旧字段/属性 | 目标 owner | 类型/生命周期 | 持久化 | 状态/执行备注 |
| --- | --- | --- | --- | --- | --- | --- |
| PLY-001 | [ ] | `active` | `PlayerLifecycleComponent.IsActive` | bool；entity lifetime | 按角色/世界 | verified narrow；补重连和失活清理 |
| PLY-002 | [ ] | `dead` | `PlayerLifecycleComponent` | bool；death interval | 必须 | verified narrow；补全部死亡分支 |
| PLY-003 | [ ] | `respawnTimer` | `PlayerLifecycleComponent.RespawnTicks` | counter；death interval | 可恢复 | verified narrow；补暂停/重启 |
| PLY-004 | [ ] | `deadTime` | `PlayerLifecycleComponent` only if source consumer exists | counter；death interval | 按 source | deferred；不能与 respawnTimer 合并猜测 |
| PLY-005 | [ ] | `spawnX` | `PlayerSpawnStateComponent` | tile coordinate；profile/world | 必须或重建 | partial；补 spawn source |
| PLY-006 | [ ] | `spawnY` | `PlayerSpawnStateComponent` | tile coordinate；profile/world | 必须或重建 | partial；补 valid spawn query |
| PLY-007 | [ ] | `position` | `TransformComponent.Position` | value；tick/entity | 需要恢复 | verified narrow；补 collision correction |
| PLY-008 | [ ] | `velocity` | `VelocityComponent` | value；tick/entity | 需要恢复 | verified narrow；补 slope/liquid |
| PLY-009 | [ ] | `direction` | `FacingComponent` | signed direction；tick | 通常不保存 | verified narrow；必须由 server intent 更新 |
| PLY-010 | [ ] | `gravDir` | `PlayerMovementStateComponent` | scalar；tick/entity | 视规则 | partial；补反重力/特殊移动 |
| PLY-011 | [ ] | `fallStart` | `PlayerMovementStateComponent` | coordinate/counter；movement interval | 不一定 | partial；补 fall damage boundary |
| PLY-012 | [ ] | `fallStart2` | `PlayerMovementStateComponent` | coordinate/counter；movement interval | 不一定 | deferred；先证明与 fallStart 的差异 |
| PLY-013 | [ ] | `tileCollide` | movement/collision state | bool；tick | 不保存 | partial；只保留 server collision rule |
| PLY-014 | [ ] | `noFallDmg` | `PlayerStatModifierComponent`/movement rule | bool；effect lifetime | 依 effect | deferred；补来源和清除 |
| PLY-015 | [ ] | `onGround`/grounded state | `PhysicsStateComponent` | derived bool；tick | 不保存 | partial；不得从 visual floor state 推断 |
| PLY-016 | [ ] | `wet` | `PlayerEnvironmentContactComponent` | bool；tick | 通常不保存 | partial；补 liquid source |
| PLY-017 | [ ] | `honeyWet` | `PlayerEnvironmentContactComponent` | bool；tick | 通常不保存 | deferred |
| PLY-018 | [ ] | `lavaWet` | `PlayerEnvironmentContactComponent` | bool；tick | 通常不保存 | deferred |
| PLY-019 | [ ] | `shimmering` | environment/transform state | bool；effect lifetime | 依规则 | deferred；不要与 visual shimmer alpha 合并 |
| PLY-020 | [ ] | `slideDir` | movement state | signed direction；tick | 不保存 | deferred |
| PLY-021 | [ ] | `pulley` | movement/rope state | bool/value；interaction lifetime | 视恢复 | deferred |

### 8.2 服务器接收的规范化输入与控制状态

这些行只表达服务器已经接受的业务意图。raw keyboard/mouse/gamepad、输入 frame layout、
message type 和 bit flags 均不迁移。

| ID | 核对 | 旧字段/属性 | 目标 owner | 服务器保留内容 | 状态 |
| --- | --- | --- | --- | --- | --- |
| PLY-IN-001 | [ ] | `controlLeft` | `PlayerInputComponent.MoveAxis` | normalized left intent | verified narrow |
| PLY-IN-002 | [ ] | `controlRight` | `PlayerInputComponent.MoveAxis` | normalized right intent | verified narrow |
| PLY-IN-003 | [ ] | `controlUp` | `PlayerInputComponent` | normalized up intent | partial |
| PLY-IN-004 | [ ] | `controlDown` | `PlayerInputComponent` | normalized down intent | partial |
| PLY-IN-005 | [ ] | `controlJump` | `PlayerInputComponent.Jump` | normalized jump intent | verified narrow |
| PLY-IN-006 | [ ] | `controlUseItem` | `PlayerInputComponent.UseItem` | accepted use intent | verified narrow |
| PLY-IN-007 | [ ] | `controlUseTile` | `PlayerInputComponent.UseTile` | validated tile intent | partial |
| PLY-IN-008 | [ ] | `controlHook` | player interaction input | validated hook intent | deferred |
| PLY-IN-009 | [ ] | `controlMount` | mount authority input | validated mount intent | partial/deferred |
| PLY-IN-010 | [ ] | `controlQuickHeal` | item-use command intent | validated heal intent | partial |
| PLY-IN-011 | [ ] | `controlQuickMana` | item-use command intent | validated mana intent | partial |
| PLY-IN-012 | [ ] | `controlThrow` | item-use command intent | validated throw intent | deferred |
| PLY-IN-013 | [ ] | `controlDash` | movement ability input | validated dash intent | deferred |
| PLY-IN-014 | [ ] | `controlInv` | server inventory command intent | inventory action only | deferred; menu state excluded |
| PLY-IN-015 | [ ] | `controlSmart` | tile interaction intent | server rule request only | deferred |
| PLY-IN-016 | [ ] | `releaseLeft` | input edge state | release transition | partial |
| PLY-IN-017 | [ ] | `releaseRight` | input edge state | release transition | partial |
| PLY-IN-018 | [ ] | `releaseUp` | input edge state | release transition | deferred |
| PLY-IN-019 | [ ] | `releaseDown` | input edge state | release transition | deferred |
| PLY-IN-020 | [ ] | `releaseJump` | input edge state | release transition | partial |
| PLY-IN-021 | [ ] | `releaseUseItem` | input edge state | release transition | partial |
| PLY-IN-022 | [ ] | `releaseHook` | input edge state | release transition | deferred |
| PLY-IN-023 | [ ] | `releaseUseTile` | input edge state | release transition | deferred |
| PLY-IN-024 | [ ] | `PlayerInput`/input batch | `PlayerInputComponent` + authority gate | tick, normalized intent, authority | partial | 不保留原始设备/帧格式 |
| PLY-IN-025 | [ ] | input sequence | server command ordering metadata | simulation ordering only | partial | 不得当作 Protocol sequence |

### 8.3 生命、法力、受伤和环境生存

| ID | 核对 | 旧字段/属性 | 目标 owner | 服务器语义 | 状态 |
| --- | --- | --- | --- | --- | --- |
| PLY-VIT-001 | [ ] | `statLifeMax` | `HealthComponent.Maximum` | life cap | verified/partial |
| PLY-VIT-002 | [ ] | `statLife` | `HealthComponent.Current` | current life | verified narrow |
| PLY-VIT-003 | [ ] | `statManaMax` | `ManaComponent.Maximum` | mana cap | verified/partial |
| PLY-VIT-004 | [ ] | `statMana` | `ManaComponent.Current` | current mana | verified narrow |
| PLY-VIT-005 | [ ] | `lifeRegen` | stat modifier/health regen state | authoritative regen value | partial |
| PLY-VIT-006 | [ ] | `lifeRegenCount` | health regen accumulator | tick accumulator | deferred |
| PLY-VIT-007 | [ ] | `manaRegen` | `ManaComponent` | mana regen rule | partial |
| PLY-VIT-008 | [ ] | `manaRegenCount` | `ManaComponent` | tick accumulator | deferred |
| PLY-VIT-009 | [ ] | `breath` | environment contact state | remaining breath | deferred |
| PLY-VIT-010 | [ ] | `breathMax` | environment rule/definition | breath cap | deferred |
| PLY-VIT-011 | [ ] | `breathCounter` | drowning state | drowning cadence | deferred |
| PLY-VIT-012 | [ ] | `drowning` | `PlayerEnvironmentContactComponent` | drowning active state | deferred |
| PLY-VIT-013 | [ ] | `immune` | combat immunity state | damage immunity | partial |
| PLY-VIT-014 | [ ] | `immuneTime` | `PlayerCooldownStateComponent` | immunity ticks | partial |
| PLY-VIT-015 | [ ] | hurt cooldown fields | `PlayerCooldownStateComponent` | per-source/ability cooldowns | deferred |
| PLY-VIT-016 | [ ] | `hurtCooldowns` | typed hit cooldown state | bounded cooldown map/slots | deferred |
| PLY-VIT-017 | [ ] | `potionDelay` | `PlayerPotionStateComponent` | potion use lockout | partial |
| PLY-VIT-018 | [ ] | `wellFed` | `WellFedStateComponent` | food effect active | partial |
| PLY-VIT-019 | [ ] | `wellFed2` | `WellFedStateComponent` | stronger food effect | partial |
| PLY-VIT-020 | [ ] | `wellFedTimer` | `WellFedStateComponent` | remaining food ticks | partial |
| PLY-VIT-021 | [ ] | `shadowDodge` | defensive ability state | one-use dodge availability | deferred |
| PLY-VIT-022 | [ ] | `brainOfConfusionDodge` | defensive ability state | one-use dodge availability | deferred |
| PLY-VIT-023 | [ ] | `ninjaDodge` | defensive ability state | one-use dodge availability | deferred |

### 8.4 Inventory、装备和 Buff

| ID | 核对 | 旧字段/属性 | 目标 owner | 服务器保留内容 | 状态 |
| --- | --- | --- | --- | --- | --- |
| PLY-INV-001 | [ ] | `inventory[59]` | `InventoryComponent` runtime slots | item instances and quantities | partial；当前 runtime 40 槽，不得假称 59 |
| PLY-INV-002 | [ ] | `bank` | persistent inventory domain | bank values if supported | deferred |
| PLY-INV-003 | [ ] | `bank2` | persistent inventory domain | bank values if supported | deferred |
| PLY-INV-004 | [ ] | `bank3` | persistent inventory domain | bank values if supported | deferred |
| PLY-INV-005 | [ ] | `bank4` | persistent inventory domain | bank values if supported | deferred |
| PLY-INV-006 | [ ] | `selectedItem` | `SelectedItemComponent` | selected runtime slot | partial/accepted narrow |
| PLY-INV-007 | [ ] | `HeldItem` | derived inventory query | current usable item | partial；不复制对象引用 |
| PLY-INV-008 | [ ] | `trashItem` | inventory state | server-owned trash slot if supported | deferred |
| PLY-INV-009 | [ ] | `voidVaultInfo`/void storage | persistent inventory domain | value-only slots if supported | deferred |
| PLY-INV-010 | [ ] | `armor[20]` | `EquipmentLoadoutComponent` | equipment item references | partial/delegated |
| PLY-INV-011 | [ ] | `dye[10]` | equipment appearance state only if server rule needs it | authoritative dye effect if any | deferred；render excluded |
| PLY-INV-012 | [ ] | `miscEquips[5]` | equipment state collection | server-use misc equipment | partial/deferred |
| PLY-INV-013 | [ ] | `miscDyes` | appearance adapter unless rule consumer exists | no default Simulation owner | excluded/deferred |
| PLY-INV-014 | [ ] | loadout selection | `EquipmentLoadoutComponent` | active loadout | partial |
| PLY-INV-015 | [ ] | equipment revision | `EquipmentStateCollectionComponent.Revision` | server mutation ordering | partial |
| PLY-INV-016 | [ ] | `buffType[]` | `BuffCollectionComponent` | active buff type | partial/delegated |
| PLY-INV-017 | [ ] | `buffTime[]` | `BuffCollectionComponent` | remaining effect ticks | partial/deferred |
| PLY-INV-018 | [ ] | `buffImmune[]` | status effect immunity state | authoritative immunity | deferred |
| PLY-INV-019 | [ ] | buff source | status effect instance state | source entity/effect provenance | deferred |
| PLY-INV-020 | [ ] | buff revision/count | `BuffCollectionComponent` | bounded collection mutation | partial |
| PLY-INV-021 | [ ] | `pet` | player summon state | active pet authority | deferred |
| PLY-INV-022 | [ ] | `lightPet` | player summon state | active light-pet authority | deferred |
| PLY-INV-023 | [ ] | `mount` | `PlayerMountStateComponent` | mounted state/type | partial |
| PLY-INV-024 | [ ] | `mountType` | `PlayerMountStateComponent` + mount definition | mount capability | partial |
| PLY-INV-025 | [ ] | `sentry`/sentry count | `PlayerSentryStateComponent` | server turret ownership/count | partial |
| PLY-INV-026 | [ ] | `stealth` | `PlayerStealthStateComponent` | authoritative stealth amount/state | partial |

### 8.5 Item use、交互、传送和 WorldObjects

| ID | 核对 | 旧字段/属性 | 目标 owner | 服务器语义 | 状态 |
| --- | --- | --- | --- | --- | --- |
| PLY-USE-001 | [ ] | `itemAnimation` | `ItemUseStateComponent` | use progress if behavior needs it | partial |
| PLY-USE-002 | [ ] | `itemAnimationMax` | `ItemUseStateComponent` | use duration | partial |
| PLY-USE-003 | [ ] | `itemTime` | `ItemUseStateComponent` | item cooldown | partial |
| PLY-USE-004 | [ ] | `itemTimeMax` | `ItemUseStateComponent` | item cooldown cap | partial |
| PLY-USE-005 | [ ] | `ItemTimeIsZero` | item-use query | derived readiness | partial |
| PLY-USE-006 | [ ] | `ItemAnimationJustStarted` | item-use transition query | derived edge | deferred |
| PLY-USE-007 | [ ] | `itemRotation` | only if affects authoritative hit/use direction | no visual-only rotation | deferred |
| PLY-USE-008 | [ ] | `itemLocation` | only if server use geometry needs it | no draw location | deferred |
| PLY-USE-009 | [ ] | `itemWidth`/`itemHeight` | item-use collision/input geometry | server geometry only | deferred |
| PLY-USE-010 | [ ] | `chest` | `PlayerInteractionComponent` | target chest handle/interaction state | partial |
| PLY-USE-011 | [ ] | `sign` | `PlayerInteractionComponent` | target sign handle/interaction state | partial |
| PLY-USE-012 | [ ] | `talkNPC` | NPC interaction state | target NPC handle | deferred |
| PLY-USE-013 | [ ] | `tileInteractAttempted` | validated tile interaction state | rejected/accepted interaction marker | partial |
| PLY-USE-014 | [ ] | `tileTargetX` | interaction command target | tile coordinate | partial |
| PLY-USE-015 | [ ] | `tileTargetY` | interaction command target | tile coordinate | partial |
| PLY-USE-016 | [ ] | `teleporting` | `PlayerTeleportStateComponent` | teleport lifecycle | deferred |
| PLY-USE-017 | [ ] | `teleportTime` | `PlayerTeleportStateComponent` | teleport cooldown/progress | deferred |
| PLY-USE-018 | [ ] | `teleportStyle` | server teleport rule only if behavior uses it | typed style, no effect object | deferred |
| PLY-USE-019 | [ ] | teleport origin/destination | teleport command/state | validated coordinates | deferred |
| PLY-USE-020 | [ ] | `sleeping` | `PlayerSleepComponent` | server sleep state | partial |
| PLY-USE-021 | [ ] | `sitting` | player interaction/movement state | server sitting rule | deferred |

### 8.6 Player 专用机制 deferred 清单

这些是服务器可能需要的行为域，但本计划不以在 Player 上添加同名字段作为解决方案。每个
行为族都要先建立独立 Definition/Component/System/Command 边界；本轮状态仍为
`deferred/blocked`。

- [ ] `mount`、mount type、mount movement、dismount、mount collision 和 mount resource：
  `PlayerMountStateComponent` + mount definitions；完整 family deferred。
- [ ] `wings`、`wingTime`、wing resource、flight/fatigue、hover/vertical flight：独立
  movement resource；不能将 frame time 或 visual wing state当作 server state。
- [ ] `grappling[]`、hook anchors、rope attach/release：独立 rope/grapple domain；数组
  不能直接进入通用 Player component。
- [ ] Fishing：rod、bait consumption、bobber ownership、fishing progress、liquid rule；
  当前 PB-005 等专用边界仍 deferred。
- [ ] Golf：ball ownership、stroke/score、world collision、golf state；`LocalGolfState`
  本身 excluded，不能把本地状态复制进 server。
- [ ] Emotes：只保留会改变服务器规则的 interaction intent；表情动画、选择 UI、社交帧
  excluded。
- [ ] Drowning、`breath`、wet collision、slope movement：PB-004 deferred，必须接入
  WorldGrid/Liquid 权威值后再提升。
- [ ] Dodge：ShadowDodge、BrainOfConfusionDodge、NinjaDodge 必须保留一次性消费/冷却/来源，
  当前 PB-007 blocked。
- [ ] Paladin/ally defense：`CanDefendWithPaladinsShield` 的资格、范围、冷却、死亡清理，
  当前 PB-008 blocked。
- [ ] Death drop：`DropTombstone`、死亡掉落、once-only marker、world item creation，
  当前 PB-009 blocked。
- [ ] NPC banner/melee rules：`HasNPCBannerBuff`、melee hit cooldown、banner source，
  当前 PB-011 blocked。
- [ ] Jellyfish：`TakeDamageFromJellyfish` 的 liquid/接触 damage 规则，当前 PB-011 blocked。
- [ ] 其他 `DashMovement`、`WallClimbMovement`、`WallslideMovement`、`CarpetMovement`、
  `DoubleJumpVisuals`、`StickyMovement`：逐族建 owner；不加大 Player God Object。

### 8.7 Player 明确排除项

- [ ] `VisualPosition`、`BaseHeight`、`Directions`、`ShouldNotDraw` 和 camera modifier：
  client presentation，excluded。
- [ ] player texture/frame/color/alpha、dust、sound、camera、map/UI、social cosmetics：
  excluded。
- [ ] raw keyboard、mouse、gamepad、local cursor、输入设备 profile：excluded；只保留已授权
  normalized intent。
- [ ] `NetworkPlayerSlice`、Player replication cursor、packet order、wire slot、payload
  flags、client-visible field：Protocol exclusion，excluded。
- [ ] `HeldItem` 的绘制/tooltip/animation projection：excluded；只有服务器使用结果保留为
  derived query。
- [ ] `PlayerList`、`ActivePlayerFileData` 等文件/宿主集合：Server/Compatibility host-only，
  不进入 Player entity component。

## 9. NPC.cs 服务器必要字段/属性逐项核对清单

NPC 字段必须先区分“NPC 实例”“刷怪上下文”“世界事件资源”和“客户端/Protocol scratch”。
Spawner 的 zone 值不是 NPC 实例字段，Boss/invasion 计数不是 `NpcBehaviorStateComponent`
字段；这些值要迁到相应的 world snapshot 或 spawn query。当前约 158 个 `AI_###` source
markers 仍未形成完整行为 owner，以下任何 `partial` 都不得升级为 complete。

### 9.1 Spawner 上下文：只进入不可变 spawn snapshot

| ID | 核对 | 旧字段 | 目标 owner | 服务器用途 | 状态 |
| --- | --- | --- | --- | --- | --- |
| NPC-SP-001 | [ ] | `spawnSpaceX` | `NpcSpawnSnapshot` | spawn candidate 空间 | partial |
| NPC-SP-002 | [ ] | `spawnSpaceY` | `NpcSpawnSnapshot` | spawn candidate 空间 | partial |
| NPC-SP-003 | [ ] | `fairyLog` | spawn rule snapshot | event/spawn rule | deferred |
| NPC-SP-004 | [ ] | `numberOfActivePlayers` | player readiness snapshot | active-player count | partial |
| NPC-SP-005 | [ ] | `reachedInvasionBossCap` | invasion spawn policy | boss cap | partial |
| NPC-SP-006 | [ ] | `pX` | spawn query input | player anchor X | partial |
| NPC-SP-007 | [ ] | `pY` | spawn query input | player anchor Y | partial |
| NPC-SP-008 | [ ] | `luck` | spawn policy input | luck modifier | deferred |
| NPC-SP-009 | [ ] | `dayTime` | `WorldClockSnapshot` | day/night eligibility | accepted-narrow |
| NPC-SP-010 | [ ] | `raining` | `WorldWeatherState` | weather eligibility | accepted-narrow |
| NPC-SP-011 | [ ] | `townNPCs` | spawn world snapshot | town NPC count | partial |
| NPC-SP-012 | [ ] | `skyMob` | spawn eligibility policy | sky spawn category | partial |
| NPC-SP-013 | [ ] | `noWorms` | spawn policy | worm prohibition | deferred |
| NPC-SP-014 | [ ] | `noGroundWorms` | spawn policy | ground worm prohibition | deferred |
| NPC-SP-015 | [ ] | `invaders` | `WorldProgressionState`/spawn policy | invasion eligibility | partial |
| NPC-SP-016 | [ ] | `spawnFriendly` | spawn authority input | friendly spawn gate | partial |
| NPC-SP-017 | [ ] | `ignoreSafeWalls` | spawn policy | safe-wall bypass | deferred |
| NPC-SP-018 | [ ] | `waterTile` | `WorldGrid` environment query | water spawn condition | partial |
| NPC-SP-019 | [ ] | `nearGranite` | biome query | granite biome condition | deferred |
| NPC-SP-020 | [ ] | `nearMarble` | biome query | marble biome condition | deferred |
| NPC-SP-021 | [ ] | `spawnSpider` | spawn policy | spider cave condition | deferred |
| NPC-SP-022 | [ ] | `surfaceSpawn` | terrain/spawn query | surface eligibility | partial |
| NPC-SP-023 | [ ] | `spawnUndergroundDesert` | biome/spawn query | underground desert eligibility | deferred |
| NPC-SP-024 | [ ] | `hardDungeon` | dungeon world snapshot | hard dungeon eligibility | deferred |
| NPC-SP-025 | [ ] | `deeperThanRockLayer` | terrain profile query | depth eligibility | partial |
| NPC-SP-026 | [ ] | `underGround` | terrain profile query | underground eligibility | partial |
| NPC-SP-027 | [ ] | `isOcean` | world coordinate query | ocean eligibility | partial |
| NPC-SP-028 | [ ] | `isBeach` | world coordinate query | beach eligibility | partial |
| NPC-SP-029 | [ ] | `isSpawningInWindDirection` | weather/spawn policy | wind-direction rule | deferred |
| NPC-SP-030 | [ ] | `skyBehindPlayer` | spawn environment query | sky backdrop rule only if server-used | deferred |
| NPC-SP-031 | [ ] | `livingTree` | biome/structure query | living-tree eligibility | deferred |
| NPC-SP-032 | [ ] | `dualDungeonsSpawnRules` | world rule snapshot | dual-dungeon rule | deferred |
| NPC-SP-033 | [ ] | `inDualDungeon` | dungeon query | current dungeon side | deferred |
| NPC-SP-034 | [ ] | `tresspassingDualDungeon` | dungeon policy | boundary violation rule | deferred |
| NPC-SP-035 | [ ] | `inRemixStartingArea` | world coordinate policy | Remix start area | deferred |
| NPC-SP-036 | [ ] | `offensiveToTim` | spawn rule | Tim hostility rule | deferred |
| NPC-SP-037 | [ ] | `playerHasStartingHealth` | player readiness snapshot | spawn suppression | deferred |
| NPC-SP-038 | [ ] | `ZoneCorrupt` | biome zone snapshot | corrupt zone | partial |
| NPC-SP-039 | [ ] | `ZoneCrimson` | biome zone snapshot | crimson zone | partial |
| NPC-SP-040 | [ ] | `ZoneHallow` | biome zone snapshot | hallow zone | partial |
| NPC-SP-041 | [ ] | `ZoneJungle` | biome zone snapshot | jungle zone | partial |
| NPC-SP-042 | [ ] | `ZoneSnow` | biome zone snapshot | snow zone | partial |
| NPC-SP-043 | [ ] | `ZoneGlowshroom` | biome zone snapshot | glowshroom zone | deferred |
| NPC-SP-044 | [ ] | `ZoneMeteor` | biome zone snapshot | meteor zone | deferred |
| NPC-SP-045 | [ ] | `ZoneGraveyard` | biome zone snapshot | graveyard zone | deferred |
| NPC-SP-046 | [ ] | `ZoneDungeon` | biome zone snapshot | dungeon zone | partial |
| NPC-SP-047 | [ ] | `ZoneLihzhardTemple` | biome zone snapshot | temple zone | deferred |
| NPC-SP-048 | [ ] | `ZoneGranite` | biome zone snapshot | granite zone | deferred |
| NPC-SP-049 | [ ] | `ZoneMarble` | biome zone snapshot | marble zone | deferred |
| NPC-SP-050 | [ ] | `ZoneSandstorm` | weather/biome snapshot | sandstorm zone | deferred |
| NPC-SP-051 | [ ] | `ZoneTowerSolar` | event zone snapshot | solar tower zone | deferred |
| NPC-SP-052 | [ ] | `ZoneTowerVortex` | event zone snapshot | vortex tower zone | deferred |
| NPC-SP-053 | [ ] | `ZoneTowerNebula` | event zone snapshot | nebula tower zone | deferred |
| NPC-SP-054 | [ ] | `ZoneTowerStardust` | event zone snapshot | stardust tower zone | deferred |
| NPC-SP-055 | [ ] | `ZoneOldOneArmy` | event zone snapshot | Old One Army zone | deferred |
| NPC-SP-056 | [ ] | `ZoneWaterCandle` | biome/item effect query | water candle spawn modifier | deferred |
| NPC-SP-057 | [ ] | `ZonePeaceCandle` | biome/item effect query | peace candle spawn modifier | deferred |
| NPC-SP-058 | [ ] | `ZoneShadowCandle` | biome/item effect query | shadow candle spawn modifier | deferred |
| NPC-SP-059 | [ ] | `defaultTarget` | `NpcTargetComponent` initial target | default target selection | partial |

### 9.2 NPC 生命周期、定义和战斗状态

| ID | 核对 | 旧字段 | 目标 owner | 服务器语义 | 状态 |
| --- | --- | --- | --- | --- | --- |
| NPC-LC-001 | [ ] | `active` | `NpcLifecycleComponent.IsActive` | entity active state | partial/accepted narrow |
| NPC-LC-002 | [ ] | `CanBeReplacedByOtherNPCs` | lifecycle/slot policy | slot replacement eligibility | accepted-narrow |
| NPC-LC-003 | [ ] | `homelessDespawn` | `NpcLifecycleComponent` | town NPC escape gate | accepted-narrow |
| NPC-LC-004 | [ ] | `dontCountMe` | `NpcActivityStateComponent` | slot/count exclusion | accepted-narrow |
| NPC-LC-005 | [ ] | `despawnEncouraged` | `NpcLifecycleComponent` | despawn pressure | accepted-narrow |
| NPC-LC-006 | [ ] | `timeLeft` | `NpcLifecycleComponent` | lifetime/despawn ticks | partial |
| NPC-DEF-001 | [ ] | `type` | `NpcDefinitionComponent` | content type | partial |
| NPC-DEF-002 | [ ] | `waterMovementSpeed` | `NpcDefinition` | water movement rule | deferred |
| NPC-DEF-003 | [ ] | `lavaMovementSpeed` | `NpcDefinition` | lava movement rule | deferred |
| NPC-DEF-004 | [ ] | `honeyMovementSpeed` | `NpcDefinition` | honey movement rule | deferred |
| NPC-DEF-005 | [ ] | `shimmerMovementSpeed` | `NpcDefinition` | shimmer movement rule | deferred |
| NPC-DEF-006 | [ ] | `teleportStyle` | `NpcDefinition`/behavior capability | teleport behavior selector | deferred |
| NPC-DEF-007 | [ ] | `nameOver` | no server owner unless interaction uses stable name key | name/presentation | excluded/deferred |
| NPC-DEF-008 | [ ] | `SpawnedFromStatue` | `NpcAuthorityComponent` | spawn provenance/loot rule | accepted-narrow |
| NPC-DEF-009 | [ ] | `altTexture` | no Simulation owner | texture selection | excluded |
| NPC-DEF-010 | [ ] | `townNpcVariationIndex` | NPC town variant state | server variant selection | partial |
| NPC-DEF-011 | [ ] | `rarity` | `NpcDefinition` | category/loot/slot rule if consumed | partial |
| NPC-DEF-012 | [ ] | `takenDamageMultiplier` | `NpcCombatComponent`/difficulty policy | incoming damage multiplier | accepted-narrow |
| NPC-DEF-013 | [ ] | `npcSlots` | slot accounting definition | weighted capacity cost | partial |
| NPC-DEF-014 | [ ] | `shimmerTransparency` | no server owner unless gameplay uses it | visual transparency | excluded |
| NPC-DEF-015 | [ ] | `damage` | `NpcCombatComponent`/definition | contact/attack damage | partial |
| NPC-DEF-016 | [ ] | `defDamage` | `NpcDefinition` | base damage | partial |
| NPC-DEF-017 | [ ] | `defDefense` | `NpcDefinition` | base defense | partial |
| NPC-DEF-018 | [ ] | `defLifeMax` | `NpcDefinition` | base maximum life | partial |
| NPC-DEF-019 | [ ] | `coldDamage` | `NpcDefinition`/combat rule | cold damage capability | deferred |
| NPC-DEF-020 | [ ] | `trapImmune` | `NpcAuthorityComponent`/combat policy | trap damage immunity | accepted-narrow |
| NPC-DEF-021 | [ ] | `life` | `HealthComponent`/NPC combat | current life | partial |
| NPC-DEF-022 | [ ] | `lifeMax` | `HealthComponent.Maximum` | current life cap | partial |
| NPC-DEF-023 | [ ] | `difficulty` | NPC difficulty state | scaling input | partial |
| NPC-DEF-024 | [ ] | `statsAreScaledForThisManyPlayers` | NPC combat state | scaling baseline | deferred |
| NPC-DEF-025 | [ ] | `friendly` | NPC combat faction | friendly/hostile rule | partial |
| NPC-DEF-026 | [ ] | `boss` | NPC definition capability | boss eligibility | partial |
| NPC-DEF-027 | [ ] | `chaseable` | behavior/target eligibility | target qualification | accepted-narrow |
| NPC-DEF-028 | [ ] | `dontTakeDamage` | combat state | invulnerability gate | partial |
| NPC-DEF-029 | [ ] | `dontTakeDamageFromHostiles` | combat state | hostile-NPC immunity | accepted-narrow |
| NPC-DEF-030 | [ ] | `knockBackResist` | combat definition/state | knockback resolution | deferred |
| NPC-DEF-031 | [ ] | `value` | loot/economy definition | coin/drop value | deferred |
| NPC-DEF-032 | [ ] | `extraValue` | loot/economy definition | extra drop value | deferred |
| NPC-DEF-033 | [ ] | `townNPC` | `LegacyNpcTownRegistry` + definition | town classification | accepted-narrow |
| NPC-DEF-034 | [ ] | `lavaImmune` | lava contact authority | lava damage immunity | accepted-narrow |
| NPC-DEF-035 | [ ] | `reflectsProjectiles` | behavior/combat capability | projectile reflection eligibility | accepted-narrow |

### 9.3 NPC 行为、移动和目标

| ID | 核对 | 旧字段/属性 | 目标 owner | 服务器语义 | 状态 |
| --- | --- | --- | --- | --- | --- |
| NPC-AI-001 | [ ] | `teleportTime` | `NpcBehaviorStateComponent` | behavior cooldown | partial |
| NPC-AI-002 | [ ] | `aiAction` | typed behavior state | current behavior action | partial |
| NPC-AI-003 | [ ] | `aiStyle` | `NpcDefinition`/behavior registry | behavior family key | partial |
| NPC-AI-004 | [ ] | `ai` | typed family state, not `float[]` | behavior inputs/state | partial；158 AI markers remain |
| NPC-AI-005 | [ ] | `localAI` | typed family-local state | behavior-local state | deferred；不复制数组 |
| NPC-AI-006 | [ ] | `justHit` | NPC combat transition | recent hit marker | partial |
| NPC-AI-007 | [ ] | `directionY` | `NpcTransformComponent` | vertical facing | partial |
| NPC-AI-008 | [ ] | `oldDirectionY` | behavior transition state | previous vertical facing | deferred |
| NPC-AI-009 | [ ] | `oldTarget` | target transition state | previous target | deferred |
| NPC-AI-010 | [ ] | `rotation` | only if collision/behavior needs it | authoritative orientation | deferred；draw rotation excluded |
| NPC-AI-011 | [ ] | `noGravity` | movement capability | gravity rule | partial |
| NPC-AI-012 | [ ] | `noTileCollide` | collision capability | tile collision rule | partial |
| NPC-AI-013 | [ ] | `collideX` | movement transition | X collision result | partial |
| NPC-AI-014 | [ ] | `collideY` | movement transition | Y collision result | partial |
| NPC-AI-015 | [ ] | `spriteDirection` | only if target/facing uses it | authoritative facing | deferred |
| NPC-AI-016 | [ ] | `behindTiles` | collision/visibility only if server rule uses it | tile interaction rule | deferred |
| NPC-AI-017 | [ ] | `stepSpeed` | movement definition | step movement rule | deferred |
| NPC-AI-018 | [ ] | `gfxOffY` | no server owner | render offset | excluded |
| NPC-AI-019 | [ ] | `teleporting` | behavior/lifecycle state | teleport transition | deferred |
| NPC-AI-020 | [ ] | `stairFall` | movement state | stair/fall rule | deferred |
| NPC-AI-021 | [ ] | `oldPos` | no server owner unless physics history proven | render history | excluded/deferred |
| NPC-AI-022 | [ ] | `oldRot` | no server owner | render history | excluded |
| NPC-AI-023 | [ ] | `setFrameSize` | no server owner | frame presentation | excluded |
| NPC-TGT-001 | [ ] | `target` | `NpcTargetComponent` | target handle | partial |
| NPC-TGT-002 | [ ] | `targetRect` | target/geometry query | target hitbox snapshot | partial |
| NPC-TGT-003 | [ ] | `playerInteraction` | `NpcInteractionComponent` | interaction authority | partial |
| NPC-TGT-004 | [ ] | `lastInteraction` | interaction state | expiry/repeat gate | deferred |
| NPC-TGT-005 | [ ] | `releaseOwner` | interaction authority | releasing player handle | partial |

### 9.4 NPC home、门、呼吸和交互

| ID | 核对 | 旧字段/属性 | 目标 owner | 服务器语义 | 状态 |
| --- | --- | --- | --- | --- | --- |
| NPC-HOME-001 | [ ] | `homeless` | `NpcHomeComponent` | home eligibility | accepted-narrow |
| NPC-HOME-002 | [ ] | `lookForHomeTimeout` | `NpcHomeComponent` | return/search timeout | accepted-narrow |
| NPC-HOME-003 | [ ] | `homeTileX` | `NpcHomeComponent` | home anchor X | accepted-narrow |
| NPC-HOME-004 | [ ] | `homeTileY` | `NpcHomeComponent` | home anchor Y | accepted-narrow |
| NPC-HOME-005 | [ ] | `housingCategory` | housing definition/query | category eligibility | deferred |
| NPC-HOME-006 | [ ] | `oldHomeless` | `NpcHomePublicationComponent` | publication baseline | accepted-narrow |
| NPC-HOME-007 | [ ] | `oldHomeTileX` | `NpcHomePublicationComponent` | previous home X | accepted-narrow |
| NPC-HOME-008 | [ ] | `oldHomeTileY` | `NpcHomePublicationComponent` | previous home Y | accepted-narrow |
| NPC-HOME-009 | [ ] | `closeDoor` | `DoorStateComponent`/NPC intent | door action intent | deferred |
| NPC-HOME-010 | [ ] | `doorX` | NPC home/door interaction | door anchor X | deferred |
| NPC-HOME-011 | [ ] | `doorY` | NPC home/door interaction | door anchor Y | deferred |
| NPC-HOME-012 | [ ] | `friendlyRegen` | NPC combat/environment state | friendly regen rule | deferred |
| NPC-HOME-013 | [ ] | `breath` | NPC environment state | remaining breath | deferred |
| NPC-HOME-014 | [ ] | `breathCounter` | NPC environment state | drowning cadence | deferred |
| NPC-HOME-015 | [ ] | `nextDialogue` | no server owner without dialogue domain | dialogue scheduling | excluded/deferred |

### 9.5 NPC segment、loot、Buff/DoT 和命中状态

| ID | 核对 | 旧字段/属性 | 目标 owner | 服务器语义 | 状态 |
| --- | --- | --- | --- | --- | --- |
| NPC-SEG-001 | [ ] | `realLife` | `NpcSegmentLinkComponent` | root/shared-life handle | partial |
| NPC-SEG-002 | [ ] | segment index/parent | `NpcSegmentLinkComponent` | ordered worm chain | partial |
| NPC-SEG-003 | [ ] | segment active/death state | `NpcLifecycleComponent` + segment system | cascading lifecycle | deferred |
| NPC-LOOT-001 | [ ] | `catchItem` | `NpcLootStateComponent` | 服务器捕捉/掉落业务 source、one-shot loot intent | deferred；仅迁移业务语义，wire/sync representation excluded |
| NPC-BUF-001 | [ ] | `buffType` | `BuffCollectionComponent` | active effect type | deferred |
| NPC-BUF-002 | [ ] | `buffTime` | `BuffCollectionComponent` | effect remaining ticks | deferred |
| NPC-BUF-003 | [ ] | `buffImmune` | status-effect immunity state | effect immunity | deferred |
| NPC-BUF-004 | [ ] | `canDisplayBuffs` | no server owner | display permission | excluded |
| NPC-BUF-005 | [ ] | `midas` | NPC status effect state | gold transformation effect | deferred |
| NPC-BUF-006 | [ ] | `ichor` | NPC status effect state | defense reduction effect | deferred |
| NPC-BUF-007 | [ ] | `brokenArmor` | NPC status effect state | armor reduction effect | deferred |
| NPC-BUF-008 | [ ] | `onFire` | NPC status effect state | fire DoT | deferred |
| NPC-BUF-009 | [ ] | `onFire2` | NPC status effect state | fire DoT variant | deferred |
| NPC-BUF-010 | [ ] | `onFire3` | NPC status effect state | fire DoT variant | deferred |
| NPC-BUF-011 | [ ] | `onFrostBurn` | NPC status effect state | frost fire DoT | deferred |
| NPC-BUF-012 | [ ] | `onFrostBurn2` | NPC status effect state | frost fire DoT variant | deferred |
| NPC-BUF-013 | [ ] | `poisoned` | NPC status effect state | poison DoT | deferred |
| NPC-BUF-014 | [ ] | `venom` | NPC status effect state | venom DoT | deferred |
| NPC-BUF-015 | [ ] | `tipsy` | NPC status effect state | stat modifier | deferred |
| NPC-BUF-016 | [ ] | `bleeding` | NPC status effect state | regen suppression | deferred |
| NPC-BUF-017 | [ ] | `hemorrhage` | NPC status effect state | hemorrhage effect | deferred |
| NPC-BUF-018 | [ ] | `markedByScytheWhip` | NPC status effect state | whip mark | deferred |
| NPC-BUF-019 | [ ] | `markedByEelWhip` | NPC status effect state | whip mark | deferred |
| NPC-BUF-020 | [ ] | `shadowFlame` | NPC status effect state | shadow flame DoT | deferred |
| NPC-BUF-021 | [ ] | `soulDrain` | NPC status effect state | soul drain effect | deferred |
| NPC-BUF-022 | [ ] | `shimmering` | NPC environment state | shimmer transform rule | deferred |
| NPC-BUF-023 | [ ] | `lifeRegen` | `HealthComponent`/status modifier | effective regen | deferred |
| NPC-BUF-024 | [ ] | `lifeRegenCount` | status effect accumulator | regen tick accumulator | deferred |
| NPC-BUF-025 | [ ] | `lifeRegenExpectedLossPerSecond` | status effect policy | expected DoT loss | deferred |
| NPC-BUF-026 | [ ] | `confused` | NPC status effect state | targeting/movement effect | deferred |
| NPC-BUF-027 | [ ] | `loveStruck` | NPC status effect state | interaction effect | deferred |
| NPC-BUF-028 | [ ] | `stinky` | NPC status effect state | interaction effect | deferred |
| NPC-BUF-029 | [ ] | `dryadWard` | NPC status effect state | defense ward | deferred |
| NPC-BUF-030 | [ ] | `immortal` | NPC combat state | death prevention | deferred |
| NPC-BUF-031 | [ ] | `canGhostHeal` | NPC combat state | healing rule | deferred |
| NPC-BUF-032 | [ ] | `javelined` | NPC status effect state | projectile mark | deferred |
| NPC-BUF-033 | [ ] | `tentacleSpiked` | NPC status effect state | contact effect | deferred |
| NPC-BUF-034 | [ ] | `bloodButchered` | NPC status effect state | bleed effect | deferred |
| NPC-BUF-035 | [ ] | `celled` | NPC status effect state | cell effect | deferred |
| NPC-BUF-036 | [ ] | `dryadBane` | NPC status effect state | dryad bane effect | deferred |
| NPC-BUF-037 | [ ] | `daybreak` | NPC status effect state | daybreak mark | deferred |
| NPC-BUF-038 | [ ] | `betsysCurse` | NPC status effect state | defense curse | deferred |
| NPC-BUF-039 | [ ] | `oiled` | NPC status effect state | fire interaction modifier | deferred |
| NPC-BUF-040 | [ ] | `electricEelCounter` | NPC status effect state | bounded contact counter | deferred |
| NPC-BUF-041 | [ ] | `catchableNPCTempImmunityCounter` | NPC interaction state | capture immunity cooldown | deferred |
| NPC-BUF-042 | [ ] | `immune` | NPC combat state | hit immunity | partial |
| NPC-BUF-043 | [ ] | `soundDelay` | no server owner | sound cooldown | excluded |

### 9.6 NPC world event/Boss/Spawn 资源重新归属

这些名称来自 NPC 源区，但不是 NPC 实例字段。它们必须逐项从 NPC owner 移到 World
progression、Boss encounter、spawn budget 或 event registry；本计划不允许把它们堆进
`NpcDefinitionComponent`。

- [ ] `MoonLordAttacksArray` -> Boss encounter definition；不可变攻击规则，deferred。
- [ ] `MoonLordAttacksArray2` -> Boss encounter definition；不可变攻击规则，deferred。
- [ ] `MoonLordFightingDistance` -> Boss encounter policy；战斗距离，deferred。
- [ ] `MoonLordCountdown` -> Boss encounter state；跨 tick 倒计时，deferred。
- [ ] `MaxMoonLordCountdown` -> Boss encounter definition；常量，deferred。
- [ ] `NaturalMoonlordCountdownTime` -> Boss encounter policy；deferred。
- [ ] `ItemMoonlordCountdownTime` -> Boss encounter policy；deferred。
- [ ] `goldCritterChance` -> World/Spawn rule；deferred。
- [ ] `totalInvasionPoints` -> `WorldProgressionState`；deferred。
- [ ] `waveKills` -> `WorldProgressionState`；deferred。
- [ ] `waveNumber` -> `WorldProgressionState`；deferred。
- [ ] `golemBoss`、`plantBoss`、`crimsonBoss`、`deerclopsBoss`、`mechQueen` -> Boss encounter
  state；deferred。
- [ ] `brainOfGravity`、`empressRageMode`、`cavernMonsterType` -> event/behavior state；deferred。
- [ ] `downedBoss1`、`downedBoss2`、`downedBoss3`、`downedQueenBee`、`downedSlimeKing`、
  `downedGoblins`、`downedFrost`、`downedPirates`、`downedClown` -> world progression flags；
  deferred。
- [ ] `downedPlantBoss`、`downedGolemBoss`、`downedMartians`、`downedFishron`、
  `downedHalloweenTree`、`downedHalloweenKing`、`downedChristmasIceQueen`、
  `downedChristmasTree`、`downedChristmasSantank` -> world progression flags；deferred。
- [ ] `downedAncientCultist`、`downedMoonlord`、`downedTowerSolar`、`downedTowerVortex`、
  `downedTowerNebula`、`downedTowerStardust`、`downedEmpressOfLight`、`downedQueenSlime`、
  `downedDeerclops` -> world progression flags；deferred。
- [ ] `downedMechBossAny`、`downedMechBoss1`、`downedMechBoss2`、`downedMechBoss3` -> derived
  progression query + persisted flags；deferred。
- [ ] `ShieldStrengthTowerSolar`、`ShieldStrengthTowerVortex`、`ShieldStrengthTowerNebula`、
  `ShieldStrengthTowerStardust` -> event shield state；deferred。
- [ ] `LunarShieldPowerNormal`、`TowerActiveSolar`、`TowerActiveVortex`、`TowerActiveNebula`、
  `TowerActiveStardust`、`LunarApocalypseIsUp` -> event state; deferred。
- [ ] `npcsFoundForCheckActive` -> `NpcActivityState`/spawn query scratch；deferred，不能做全局
  可写数组。
- [ ] `lazyNPCOwnedProjectileSearchArray` -> query-local workspace；deferred，不能持久化。
- [ ] `spawnSlotProtected` -> spawn budget state；deferred。
- [ ] `ShimmeredTownNPCs`、`savedTaxCollector`、`savedGoblin`、`savedWizard`、`savedMech`、
  `savedAngler`、`savedStylist`、`savedBartender`、`savedGolfer` -> town/event progression；
  deferred。
- [ ] `boughtCat`、`boughtDog`、`boughtBunny` -> world purchase progression；deferred。
- [ ] `unlockedSlimeBlueSpawn`、`unlockedSlimeGreenSpawn`、`unlockedSlimeOldSpawn`、
  `unlockedSlimePurpleSpawn`、`unlockedSlimeRainbowSpawn`、`unlockedSlimeRedSpawn`、
  `unlockedSlimeYellowSpawn`、`unlockedSlimeCopperSpawn` -> spawn progression；deferred。
- [ ] `unlockedMerchantSpawn`、`unlockedDemolitionistSpawn`、`unlockedPartyGirlSpawn`、
  `unlockedDyeTraderSpawn`、`unlockedTruffleSpawn`、`unlockedArmsDealerSpawn`、
  `unlockedNurseSpawn`、`unlockedPrincessSpawn` -> town progression；deferred。
- [ ] `combatBookWasUsed`、`combatBookVolumeTwoWasUsed`、`peddlersSatchelWasUsed` -> world rule/
  item progression；deferred。
- [ ] `taxCollector`、`freeCake`、`travelNPC` -> event/town state；deferred。
- [ ] `fireFlyFriendly`、`fireFlyChance`、`fireFlyMultiple`、`butterflyChance`、`stinkBugChance`
  -> spawn definition/policy；deferred。
- [ ] `gravity`、`safeRangeX`、`safeRangeY`、`activeRangeX`、`activeRangeY` -> spawn/activity
  definition；deferred，禁止作为全局杂项 singleton。
- [ ] `noSpawnCycle` -> `NpcSpawnCycleStateComponent`；partial，必须接入 lifecycle/spawn。
- [ ] `activeTime`、`defaultSpawnRate`、`defaultMaxSpawns` -> spawn budget policy；deferred。
- [ ] `kingSlimePointCacheSize`、`kingSlimePointCacheSizeMax`、`kingSlimePointCache` -> bounded
  query-local cache；deferred，不能成为 NPC component。
- [ ] `EoCKilledToday`、`WoFKilledToday` -> world event progression；deferred。
- [ ] `ignorePlayerInteractions` -> NPC interaction authority; deferred。
- [ ] `ladyBugGoodLuckTime`、`ladyBugBadLuckTime`、`ladyBugRainTime`、
  `maximumAmountOfTimesLadyBugRainCanStack` -> world weather/luck state; deferred。
- [ ] `offSetDelayTime` -> event/spawn policy; deferred。

### 9.7 NPC 属性与 Protocol/表现排除

- [ ] `CanTalk`、`CanBeTalkedTo`：若未来建立 town service，只进入 interaction query；当前
  partial/deferred，不复制本地化文本。
- [ ] `HasValidTarget`、`HasPlayerTarget`：server target query，可保留为 derived property；
  不使用 encoded target index。
- [ ] `HasNPCTarget`、`SupportsNPCTargets`：target capability/query，当前 partial。
- [ ] `IsShimmerVariant`：typed town variant rule，当前 partial；不复制透明度。
- [ ] `TypeName`、`FullName`、`GivenOrTypeName`：localization/presentation，excluded/deferred。
- [ ] `HasGivenName`、`GivenName`：若服务器持久化名字需要，进入 `NpcGivenNameComponent`；
  不把本地化标题放入 Simulation。
- [ ] `TranslatedTargetIndex`：旧槽位编码属性，Protocol-only，excluded。
- [ ] `sWidth`、`sHeight`：客户端窗口尺寸，excluded。
- [ ] `IsABestiaryIconDummy`、`IsAPortraitDummy`、`ForcePartyHatOn`：client/presentation，excluded。
- [ ] `dripping`、`drippingSlime`、`drippingSparkleSlime`、`HitSound`、`DeathSound`、
  `color`、`alpha`、`hide`、`scale`、`frameCounter`、`frame`、`lastPortalColorIndex`：
  presentation/effect，excluded。
- [ ] `netUpdate`、`netSpam`、`skippedSyncs`、`streamCounter`、`netUpdatePendingSpamCooldown`、
  `netUpdatePendingFullSpamCooldown`、`netSpamPacketLimit`、`netSpamTicksPerPacket`、
  `netSpamTicksPerPacketForBosses`、`netAlways`、`spawnNeedsSyncing`、`netStream`、
  `playerNetSyncState`、`netOffset`、`catchItem` 的 wire/同步部分：Protocol exclusion，
  不迁移；`realLife` 仅保留 segment identity/state。

## 10. Projectile.cs 服务器必要字段/属性逐项核对清单

Projectile 只保留服务器用于归属、生成、移动、碰撞、伤害、生命周期和行为的值。旧的
`ai[]`/`localAI[]` 如果确实承载行为，必须按 behavior family 拆成 typed state；不能
因为某些 `ai` 值曾被放进同步帧，就把同步帧字段纳入本计划。

### 10.1 实例、生命周期和归属

| ID | 核对 | 旧字段/属性 | 目标 owner | 服务器语义 | 状态 |
| --- | --- | --- | --- | --- | --- |
| PROJ-LC-001 | [ ] | `identity` | Projectile entity handle/identity store | 实例身份、持久化/生命周期关联 | identity-preserved；不作为 Protocol 字段迁移 |
| PROJ-LC-002 | [ ] | `active` | `ProjectileLifecycleComponent` | active/inactive | accepted-narrow |
| PROJ-LC-003 | [ ] | `timeLeft` | `ProjectileLifetimeComponent` | remaining lifetime | accepted-narrow |
| PROJ-LC-004 | [ ] | `numUpdates` | update policy/state | sub-update count | partial |
| PROJ-LC-005 | [ ] | `extraUpdates` | update policy/definition | extra simulation updates | partial |
| PROJ-LC-006 | [ ] | `type` | `ProjectileDefinitionComponent` | content type | partial |
| PROJ-LC-007 | [ ] | `owner` | `ProjectileOwnerComponent` | owning Player/NPC/world handle | partial |
| PROJ-LC-008 | [ ] | `npcProj` | owner/capability state | NPC-origin projectile | partial |
| PROJ-LC-009 | [ ] | `minion` | summon capability state | minion authority | deferred |
| PROJ-LC-010 | [ ] | `minionSlots` | summon resource state | minion capacity consumption | deferred |
| PROJ-LC-011 | [ ] | `sentry` | sentry capability/state | persistent turret authority | deferred/partial |
| PROJ-LC-012 | [ ] | `trap` | trap capability/state | world trap authority | deferred |
| PROJ-LC-013 | [ ] | spawn source | `ProjectileSpawnStateComponent` | source entity and spawn reason | partial |
| PROJ-LC-014 | [ ] | child spawn relation | typed child spawn state | server-owned parent/child lifecycle | deferred |

### 10.2 运动、碰撞和行为

| ID | 核对 | 旧字段/属性 | 目标 owner | 服务器语义 | 状态 |
| --- | --- | --- | --- | --- | --- |
| PROJ-MV-001 | [ ] | `position` | `ProjectileTransformComponent` | authoritative position | accepted-narrow |
| PROJ-MV-002 | [ ] | `velocity` | `VelocityComponent` | authoritative velocity | accepted-narrow |
| PROJ-MV-003 | [ ] | `direction` | `ProjectileTransformComponent` | movement/facing direction | partial |
| PROJ-MV-004 | [ ] | `width` | `ColliderComponent`/definition | collision width | partial |
| PROJ-MV-005 | [ ] | `height` | `ColliderComponent`/definition | collision height | partial |
| PROJ-MV-006 | [ ] | `aiStyle` | `ProjectileDefinition`/behavior registry | behavior family key | partial |
| PROJ-MV-007 | [ ] | `scale` | definition or collider state | server hitbox scale when consumed | partial |
| PROJ-MV-008 | [ ] | `stepSpeed` | behavior definition | movement step rule | deferred |
| PROJ-MV-009 | [ ] | `tileCollide` | `ProjectileCollisionComponent` | solid collision gate | partial |
| PROJ-MV-010 | [ ] | `ignoreWater` | collision/liquid policy | liquid collision gate | deferred |
| PROJ-MV-011 | [ ] | `reflected` | behavior/combat state | reflection state | deferred |
| PROJ-MV-012 | [ ] | `correctSlopeCollision` | collision policy | slope correction rule | deferred |
| PROJ-MV-013 | [ ] | `ai` | typed behavior state | authoritative family state | partial；禁止公共 float array |
| PROJ-MV-014 | [ ] | `localAI` | typed behavior-local state | local behavior state | deferred；禁止公共 float array |
| PROJ-MV-015 | [ ] | behavior counters | `ProjectileBehaviorStateComponent` | bounded per-instance counters | partial |
| PROJ-MV-016 | [ ] | `bounce`/reflection count | `ProjectileBehaviorStateComponent` | bounce budget | deferred |
| PROJ-MV-017 | [ ] | liquid interaction state | `ProjectileCollisionComponent` | water/lava/shimmer behavior | deferred |

### 10.3 战斗、命中和冷却

| ID | 核对 | 旧字段/属性 | 目标 owner | 服务器语义 | 状态 |
| --- | --- | --- | --- | --- | --- |
| PROJ-CBT-001 | [ ] | `damage` | `ProjectileCombatComponent` | effective damage | partial |
| PROJ-CBT-002 | [ ] | `originalDamage` | `ProjectileCombatComponent`/definition | pre-modifier damage | partial |
| PROJ-CBT-003 | [ ] | `knockBack` | `ProjectileCombatComponent` | knockback value | partial |
| PROJ-CBT-004 | [ ] | `friendly` | combat faction state | player-friendly target rule | partial |
| PROJ-CBT-005 | [ ] | `hostile` | combat faction state | hostile target rule | partial |
| PROJ-CBT-006 | [ ] | `penetrate` | `ProjectilePenetrationComponent` | remaining penetration | partial/accepted narrow |
| PROJ-CBT-007 | [ ] | `localNPCImmunity` | typed hit-immunity resource | NPC hit gate | deferred |
| PROJ-CBT-008 | [ ] | `playerImmune` | typed hit-immunity resource | player hit gate | deferred |
| PROJ-CBT-009 | [ ] | `immune`/hit immunity | combat cooldown state | hit exclusion | deferred |
| PROJ-CBT-010 | [ ] | restrike delay | `ProjectileCooldownComponent` | repeat-hit delay | partial |
| PROJ-CBT-011 | [ ] | owner hit check | owner-target query | self/owner hit exclusion | partial |
| PROJ-CBT-012 | [ ] | area damage intent | typed damage command | server area effect | deferred |
| PROJ-CBT-013 | [ ] | on-hit status intent | combat command/effect | status effect application | deferred |
| PROJ-CBT-014 | [ ] | despawn damage/status intent | lifecycle effect command | authoritative despawn effect | deferred |
| PROJ-CBT-015 | [ ] | `WipableTurret` | sentry eligibility policy | turret wipe rule | deferred |
| PROJ-CBT-016 | [ ] | `OwnerMinionAttackTargetNPC` | owner target query | minion target selection | deferred |
| PROJ-CBT-017 | [ ] | `CareForAttackCD` | owner attack cooldown state | minion/sentry attack cooldown | deferred |

### 10.4 Projectile 属性和明确排除项

- [ ] `MaxUpdates` -> update policy 的 derived value；只保留 server tick meaning，partial。
- [ ] `OwnedBySomeone` -> ownership query；没有 owner 时返回确定性 false，partial。
- [ ] `NetSectionCoordinates` -> section geometry query 若被服务器空间分区消费；任何 wire
  坐标/同步 cursor 仍 excluded。
- [ ] `Name`、`Opacity` -> content/presentation，excluded。
- [ ] `alpha`、`light`、`rotation`、`frame`、`frameCounter`、`oldPos`、`oldRot`、
  `drawLayer`、`hide` -> presentation/animation，excluded；只有碰撞需要的方向值进入 transform。
- [ ] `netUpdate`、`netSpam`、`netSyncSkippedForPlayer`、`NetSectionCoordinates` 的 wire
  投影、replication cursor、packet flags -> Protocol exclusion，excluded。
- [ ] local immunity/player immunity 的业务冷却可以进入 typed combat state；任何按包同步
  的 immunity 数组或字段不迁移。
- [ ] `ProjectileNetworkIdentityComponent`、`ProjectileNetworkUpdateComponent` 等现有
  网络/复制类型不作为本计划 owner；若保留，仅作为外部历史兼容边界，不计入本计划完成度。

## 11. Item.cs 字段/属性逐项核对清单

Item 的字段/属性报告口径为 `148`（140 fields + 8 properties）；更宽 mapping 另有
`237` 条初始 ownership 行，可能含方法和不同责任层。下面只核对字段/属性，不迁移或扩写
`SetDefaults`、`Prefix`、`Refresh` 等方法。每一行都必须重新判断它是 Definition、实例状态、
服务器系统所需的派生值、host/compatibility 边界，还是客户端/unsupported deferred。

### 11.1 Item 基础定义、经济和拾取规则

- [ ] `ITM-001` `width` -> `ItemDefinition`；类型尺寸，immutable；server hit/use geometry；`partial`。
- [ ] `ITM-002` `height` -> `ItemDefinition`；类型尺寸，immutable；server hit/use geometry；`partial`。
- [ ] `ITM-003` `coinGrabRange` -> item pickup policy/compatibility input；服务器拾取范围；`deferred`。
- [ ] `ITM-004` `manaGrabRange` -> item pickup policy；法力拾取范围；`partial/deferred`。
- [ ] `ITM-005` `lifeGrabRange` -> item pickup policy；生命拾取范围；`deferred`。
- [ ] `ITM-006` `treasureGrabRange` -> item pickup policy；宝藏拾取范围；`deferred`。
- [ ] `ITM-007` `_nameOverride` -> `ItemIdentityDefinition` 或实例名称覆盖；服务器只在交易/持久化需要时保留；`deferred`。
- [ ] `ITM-008` `luckPotionDuration1` -> recovery definition；固定 effect duration；`partial`。
- [ ] `ITM-009` `luckPotionDuration2` -> recovery definition；固定 effect duration；`partial`。
- [ ] `ITM-010` `luckPotionDuration3` -> recovery definition；固定 effect duration；`partial`。
- [ ] `ITM-011` `flaskTime` -> potion/flask effect policy；server effect duration；当前 `deferred`，不得只放常量占位。
- [ ] `ITM-012` `copper` -> `ItemPriceSystem` immutable conversion；`accepted-narrow`。
- [ ] `ITM-013` `silver` -> `ItemPriceSystem` immutable conversion；`accepted-narrow`。
- [ ] `ITM-014` `gold` -> `ItemPriceSystem` immutable conversion；`accepted-narrow`。
- [ ] `ITM-015` `platinum` -> `ItemPriceSystem` immutable conversion；`accepted-narrow`。
- [ ] `ITM-016` `goldCritterRarityColor` -> visual rarity color；`excluded`。
- [ ] `ITM-017` `CommonMaxStack` -> `ItemDefinition.StackLimit` default；immutable definition；`partial`。
- [ ] `ITM-018` `potionDelay` -> recovery/use policy；server potion lockout default；`partial`。
- [ ] `ITM-019` `restorationDelay` -> recovery/use policy或兼容输入；server delay；`deferred`。
- [ ] `ITM-020` `eggnogDelay` -> recovery/use policy或兼容输入；server delay；`deferred`。
- [ ] `ITM-021` `mushroomDelay` -> recovery/use policy或兼容输入；server delay；`deferred`。
- [ ] `ITM-022` `questItem` -> `ItemIdentityDefinition`；quest inventory/drop rule；`partial`。
- [ ] `ITM-023` `headType` -> equipment definition registry；head slot type rule；`partial`。
- [ ] `ITM-024` `bodyType` -> equipment definition registry；body slot type rule；`partial`。
- [ ] `ITM-025` `legType` -> equipment definition registry；leg slot type rule；`partial`。
- [ ] `ITM-026` `staff` -> tool/placement compatibility registry only if server consumer exists；`deferred`。
- [ ] `ITM-027` `claw` -> tool/placement compatibility registry only if server consumer exists；`deferred`。
- [ ] `ITM-028` `flame` -> item hazard/placement definition；只保留 server fire rule；`deferred`。
- [ ] `ITM-029` `mech` -> item summon/mechanism definition；server use rule；`deferred`。
- [ ] `ITM-030` `tileWand` -> `ItemPlacementDefinition`；tile-wand rule；`partial/deferred`。
- [ ] `ITM-031` `wornArmor` -> visual equipment state；`excluded`，不进入 server component。
- [ ] `ITM-032` `tooltipContext` -> tooltip/UI；`excluded`。
- [ ] `ITM-033` `tooltipSlot` -> tooltip/UI；`excluded`。
- [ ] `ITM-034` `dye` -> instance dye only if an authoritative rule consumes it；render-only value `excluded`。
- [ ] `ITM-035` `fishingPole` -> `ItemGatheringDefinition`；fishing capability；`deferred`。
- [ ] `ITM-036` `bait` -> `ItemGatheringDefinition`；bait capability/consumption；`deferred`。
- [ ] `ITM-037` `makeNPC` -> item summon/spawn definition；server spawn intent；`deferred`。
- [ ] `ITM-038` `expertOnly` -> `ItemDefinition` mode eligibility；`partial`。
- [ ] `ITM-039` `expert` -> `ItemDefinition` mode/variant rule；`partial`。
- [ ] `ITM-040` `isAShopItem` -> shop offer classification；server shop domain only；`deferred`。
- [ ] `ITM-041` `hairDye` -> player appearance/effect definition；若只改外观则 `excluded`，否则 `deferred`。
- [ ] `ITM-042` `paint` -> instance paint state only when persistence/gameplay needs it；`partial/deferred`。
- [ ] `ITM-043` `paintCoating` -> instance coating state only when gameplay needs it；`partial/deferred`。
- [ ] `ITM-044` `type` -> `ItemDefinition.ItemType` + `ItemStackComponent.ItemType`；content key，不是身份排除；`partial`。
- [ ] `ITM-045` `favorited` -> `ItemInstanceStateComponent.IsFavorited`；persistence/account rule；`partial`。
- [ ] `ITM-046` `holdStyle` -> holding/render style；`excluded`。

### 11.2 Item 使用、库存、放置和战斗定义

- [ ] `ITM-047` `useStyle` -> `ItemUseDefinition`；server action selection；`partial`。
- [ ] `ITM-048` `channel` -> `ItemUseDefinition`；continuous-use rule；`partial`。
- [ ] `ITM-049` `accessory` -> `ItemEquipmentDefinition`；equip eligibility；`partial`。
- [ ] `ITM-050` `useAnimation` -> use definition/state；只保存 server use timing；`partial`。
- [ ] `ITM-051` `useTime` -> use definition/state；server cooldown；`partial`。
- [ ] `ITM-052` `stack` -> `ItemStackComponent.Quantity`；instance mutable quantity；`partial`，纠正旧 mapping 的 Definition 归类。
- [ ] `ITM-053` `maxStack` -> `ItemDefinition.StackLimit`；immutable cap；`partial`。
- [ ] `ITM-054` `pick` -> `ItemToolDefinition.PickPower`；server tile interaction；`partial`。
- [ ] `ITM-055` `axe` -> `ItemToolDefinition.AxePower`；server tree interaction；`partial`。
- [ ] `ITM-056` `hammer` -> `ItemToolDefinition.HammerPower`；server tile interaction；`partial`。
- [ ] `ITM-057` `tileBoost` -> placement/tool definition；server reach/placement modifier；`partial`。
- [ ] `ITM-058` `createTile` -> `ItemPlacementDefinition`；tile creation target；`partial`。
- [ ] `ITM-059` `createWall` -> `ItemPlacementDefinition`；wall creation target；`partial`。
- [ ] `ITM-060` `placeStyle` -> `ItemPlacementDefinition`；style/footprint selector；`partial`。
- [ ] `ITM-061` `damage` -> `ItemCombatDefinition.Damage`；immutable base damage；`partial`。
- [ ] `ITM-062` `knockBack` -> `ItemCombatDefinition.Knockback`；immutable combat metadata；`partial`。
- [ ] `ITM-063` `healLife` -> `ItemRecoveryDefinition`；server healing amount；`partial`。
- [ ] `ITM-064` `healMana` -> `ItemRecoveryDefinition`；server mana recovery amount；`partial`。
- [ ] `ITM-065` `potion` -> recovery/use classification；server potion rule；`partial`。
- [ ] `ITM-066` `consumable` -> use/stack mutation rule；server consumption；`partial`。
- [ ] `ITM-067` `autoReuse` -> use definition；server repeat permission；`partial`。
- [ ] `ITM-068` `useTurn` -> use/movement rule；server facing/turn effect only if consumer exists；`deferred`。
- [ ] `ITM-069` `color` -> render color；`excluded`。
- [ ] `ITM-070` `alpha` -> render/appearance alpha；若没有 server hitbox consumer，`excluded`。
- [ ] `ITM-071` `glowMask` -> render glow mask；`excluded`。
- [ ] `ITM-072` `scale` -> definition scale only if server geometry consumes it；`partial/deferred`。
- [ ] `ITM-073` `UseSound` -> sound asset/style；`excluded`。
- [ ] `ITM-074` `useSoundPitch` -> sound presentation；`excluded`。
- [ ] `ITM-075` `defense` -> `ItemEquipmentDefinition.Defense`；server equipment stats；`partial`。
- [ ] `ITM-076` `headSlot` -> equipment definition slot；`partial`。
- [ ] `ITM-077` `bodySlot` -> equipment definition slot；`partial`。
- [ ] `ITM-078` `legSlot` -> equipment definition slot；`partial`。
- [ ] `ITM-079` `handOnSlot` -> equipment definition slot；`partial`。
- [ ] `ITM-080` `handOffSlot` -> equipment definition slot；`partial`。
- [ ] `ITM-081` `backSlot` -> equipment definition slot；`partial`。
- [ ] `ITM-082` `frontSlot` -> equipment definition slot；`partial`。
- [ ] `ITM-083` `shoeSlot` -> equipment definition slot；`partial`。
- [ ] `ITM-084` `waistSlot` -> equipment definition slot；`partial`。
- [ ] `ITM-085` `wingSlot` -> equipment definition slot；flight capability only；`deferred`。
- [ ] `ITM-086` `shieldSlot` -> equipment definition/ally defense capability；`deferred`。
- [ ] `ITM-087` `neckSlot` -> equipment definition slot；`partial`。
- [ ] `ITM-088` `faceSlot` -> equipment definition slot；`partial`。
- [ ] `ITM-089` `balloonSlot` -> equipment definition slot；`deferred`。
- [ ] `ITM-090` `beardSlot` -> appearance slot；`excluded` unless server rule is proven。
- [ ] `ITM-091` `voiceSlot` -> voice presentation；`excluded`。
- [ ] `ITM-092` `stringColor` -> presentation color；`excluded`。
- [ ] `ITM-093` `ToolTip` -> UI/tooltip object；`excluded`。
- [ ] `ITM-094` `BestiaryNotes` -> bestiary text；`excluded`。
- [ ] `ITM-095` `rare` -> `ItemDefinition.Rarity`；loot/economy rule；`partial`。
- [ ] `ITM-096` `shoot` -> use/combat projectile intent；`ItemUseDefinition`；`partial`。
- [ ] `ITM-097` `shootSpeed` -> use/combat projectile speed；`partial`。
- [ ] `ITM-098` `ammo` -> ammo category definition；`partial`。
- [ ] `ITM-099` `notAmmo` -> ammo exclusion definition；`partial`。
- [ ] `ITM-100` `useAmmo` -> ammo consumption rule；`ItemUseDefinition` + `ItemAmmoConsumptionSystem`；`partial`。

### 11.3 Item 恢复、经济、专用能力和实例状态

- [ ] `ITM-101` `lifeRegen` -> `ItemEquipmentDefinition`/stat modifier；server regen contribution；`partial`。
- [ ] `ITM-102` `manaIncrease` -> equipment/recovery definition；server max mana contribution；`partial`。
- [ ] `ITM-103` `buyOnce` -> shop offer rule；server purchase state；`deferred`。
- [ ] `ITM-104` `mana` -> use/recovery definition；server mana cost；`partial`。
- [ ] `ITM-105` `noUseGraphic` -> client use presentation；`excluded`。
- [ ] `ITM-106` `noMelee` -> combat definition；server melee eligibility；`deferred`，不得误判为纯 UI。
- [ ] `ITM-107` `value` -> economy definition；server sell/drop value；`partial`。
- [ ] `ITM-108` `buy` -> economy/shop definition；server buy eligibility；`deferred`。
- [ ] `ITM-109` `social` -> equipment/social rule；只在服务器效果存在时保留；`deferred`。
- [ ] `ITM-110` `vanity` -> equipment appearance rule；纯外观 `excluded`，有 server effect 时 `deferred`。
- [ ] `ITM-111` `material` -> crafting/recipe definition；server crafting consumer尚未闭合；`deferred`。
- [ ] `ITM-112` `noWet` -> environment interaction definition；server wet rule；`deferred`。
- [ ] `ITM-113` `buffType` -> `ItemRecoveryDefinition.BuffType`；effect type；`partial`。
- [ ] `ITM-114` `buffTime` -> `ItemRecoveryDefinition.BuffDurationTicks`；effect duration；`partial`。
- [ ] `ITM-115` `mountType` -> mount capability definition；server mount selection；`partial/deferred`。
- [ ] `ITM-116` `cartTrack` -> minecart/track interaction domain；server placement/use rule；`deferred`。
- [ ] `ITM-117` `uniqueStack` -> stack invariant；不能与不兼容实例合并；`partial`。
- [ ] `ITM-118` `shopSpecialCurrency` -> shop/economy domain；server payment rule；`deferred`，不是 wire field。
- [ ] `ITM-119` `shopCustomPrice` -> shop/economy domain；server price；`deferred`，不是客户端价格帧。
- [ ] `ITM-120` `shootsEveryUse` -> use definition；projectile count/consumption rule；`partial`。
- [ ] `ITM-121` `chlorophyteExtractinatorConsumable` -> `ItemExtractinatorDefinition`；server extraction eligibility；`partial`。
- [ ] `ITM-122` `DD2Summon` -> event summon definition；server event spawn intent；`deferred`。
- [ ] `ITM-123` `crit` -> `ItemCombatDefinition.CriticalChance`；server combat metadata；`partial`。
- [ ] `ITM-124` `armorPenetration` -> `ItemCombatDefinition.ArmorPenetration`；server combat metadata；`partial`。
- [ ] `ITM-125` `bonusTagDamage` -> combat definition；server summon/tag damage；`deferred`。
- [ ] `ITM-126` `prefix` -> `ItemInstanceStateComponent.PrefixId`；instance mutable value；`partial`。
- [ ] `ITM-127` `melee` -> combat definition damage class；`partial`。
- [ ] `ITM-128` `magic` -> combat definition damage class；`partial`。
- [ ] `ITM-129` `ranged` -> combat definition damage class；`partial`。
- [ ] `ITM-130` `summon` -> combat definition damage class；`partial`。
- [ ] `ITM-131` `sentry` -> sentry equipment/use capability；server turret behavior；`deferred`。
- [ ] `ITM-132` `reuseDelay` -> use definition/state；server cooldown；`partial`。
- [ ] `ITM-133` `newAndShiny` -> `ItemInstanceStateComponent.IsNewAndShiny` only if persistent rule requires；`partial`。
- [ ] `ITM-134` `hasVanityEffects` -> equipment definition；server effect capability only；`deferred`。
- [ ] `ITM-135` `foodWidth` -> recovery/use definition constant；server hit/use geometry if consumed；`deferred`。
- [ ] `ITM-136` `foodHeight` -> recovery/use definition constant；server hit/use geometry if consumed；`deferred`。
- [ ] `ITM-137` `WALL_PLACEMENT_USETIME` -> placement definition constant；server use timing；`partial`。
- [ ] `ITM-138` `_phaseColors` -> visual projectile/item phase colors；`excluded`。
- [ ] `ITM-139` `PickupReplacementTime` -> world item pickup policy；server replacement lifetime；`partial`。
- [ ] `ITM-140` `SlotsRemainingBeforeEmergencyStackingInMultiplayer` -> inventory policy；server emergency stacking threshold；`deferred`，不要解释为 Protocol capacity。

### 11.4 Item 属性

- [ ] `ITM-P-001` `active` -> `ItemWorldStateComponent.IsActive` + empty-stack invariant；server world lifecycle；`partial`。
- [ ] `ITM-P-002` `PaintOrCoating` -> derived instance query；只有 paint/coating 为 server rule 时保留；`partial/deferred`。
- [ ] `ITM-P-003` `OriginalRarity` -> `ItemDefinition.Rarity`；不回查 legacy `ContentSamples`；`partial`。
- [ ] `ITM-P-004` `OriginalDamage` -> `ItemDefinition` base damage；不回查旧全局样本；`partial`。
- [ ] `ITM-P-005` `OriginalDefense` -> `ItemDefinition` base defense；不回查旧全局样本；`partial`。
- [ ] `ITM-P-006` `Variant` -> `ItemVariantDefinition` + instance variant state；`ItemVariantSystem`；`partial`。
- [ ] `ITM-P-007` `IsACoin` -> derived economy query；由 ItemDefinition/stack 计算；`partial`。
- [ ] `ITM-P-008` `IsAir` -> derived empty-item query；由 ItemType/Quantity invariant 计算；`partial`。

### 11.5 Item 布局与排除复核

- [ ] `InventoryItemSlotsStart=0`、`InventoryItemSlotsCount=50`：旧布局 contract；当前 runtime
  `InventoryComponent` 为 40 槽，状态必须 `deferred`，不能新增 alias。
- [ ] `InventoryCoinSlotsStart=50`、`InventoryCoinSlotsCount=4`：旧布局 contract；不能由当前
  40 槽或持久化 990 槽推导等价。
- [ ] `InventoryAmmoSlotsStart=54`、`InventoryAmmoSlotsCount=4`：旧布局 contract；必须单独
  记录 ammo layout 与 server inventory owner，当前 `deferred`。
- [ ] `InventorySlotsTotal=58`：旧布局总数；不是当前 runtime/persistence 容量，当前 `deferred`。
- [ ] `PlayerPersistentState.ItemSlotCount=990`：服务器持久化布局，不能反向当作 runtime inventory。
- [ ] `PlayerPersistentStateMapper` 的连接可见槽位投影：属于外部兼容/传输边界，本计划不迁移
  其字段或布局；服务器只核对输入/输出的 value state。
- [ ] tooltip、draw hitbox、color/glow、sound、UI、Bestiary、animation、appearance cache：
  明确 `excluded`，不得为了覆盖 148 行而添加 server 字段。
- [ ] `ContentSamples`、`ItemID.Sets` 的全局可变引用：不得成为 Simulation owner；只可由
  source-backed immutable Definition/Registry 适配。

## 12. WorldObjects 字段/属性逐项核对清单

### 12.1 Chest

- [ ] `Chest.identity` -> `ChestIdentityComponent`；保留实体寻址/持久化关联；不是 Protocol field。
- [ ] `Chest.x` -> `ChestComponent.TileX`；world grid anchor；必须保存。
- [ ] `Chest.y` -> `ChestComponent.TileY`；world grid anchor；必须保存。
- [ ] `Chest.item[]` -> `ChestInventoryComponent`；内容与数量不变量；必须保存；禁止复用 Player inventory layout。
- [ ] `Chest.maxItems` -> `ChestDefinition`/bounded capacity；当前 40 槽窄片需与旧来源核对。
- [ ] `Chest.name` -> `ChestStateComponent`；服务器需要时保存名称；不含 UI label cache。
- [ ] `Chest.active`/valid -> `ChestComponent` lifecycle；invalid/destroyed 必须单调处理。
- [ ] `Chest.openBy`/opener -> `ChestAccessComponent`；授权 Player handle、范围和占用状态。
- [ ] `Chest.locked` -> `ChestLockDefinition` + state；锁定规则、权限和持久化。
- [ ] `Chest.revision` -> domain revision；只用于服务器并发/持久化冲突；不是网络 revision。
- [ ] `Chest.section` -> WorldGrid section association；不迁移 PVS/section packet cursor。
- [ ] chest create/destroy/open/close state -> typed commands/systems；不把 request/response 字段存入组件。
- [ ] bank/shop chest distinction -> explicit `ChestKind`/definition；不能以客户端菜单状态推断。
- [ ] chest frame/animation/render fields -> `excluded`。

### 12.2 Sign

- [ ] `Sign.identity` -> sign entity identity；持久化关联；保留但不复制 wire identity。
- [ ] `Sign.x` -> `SignComponent.TileX`；world anchor；必须保存。
- [ ] `Sign.y` -> `SignComponent.TileY`；world anchor；必须保存。
- [ ] `Sign.text` -> `SignComponent.Text`；服务器交互/持久化需要时保存；不是 message field。
- [ ] `Sign.active`/valid -> sign lifecycle；invalid/destroyed state。
- [ ] `Sign.revision` -> domain revision；服务器持久化/冲突控制。
- [ ] sign edit authority -> `PlayerInteractionComponent` + authorization policy；只保留业务权限。
- [ ] sign attachment/frame geometry -> `SignValidationQuery`/TileReadSnapshot；不复制客户端 frame。
- [ ] sign UI/font/color/scroll state -> `excluded`。

### 12.3 TileEntity、Door、Wire、Actuator 和训练假人

- [ ] TileEntity identity -> `TileEntityStateComponent`；实体寻址和持久化关联；不是 Protocol owner。
- [ ] TileEntity type -> `TileEntityDefinition`；稳定类型规则；未知类型 fail-closed/deferred。
- [ ] TileEntity tile X/Y -> state component；world anchor；必须保存。
- [ ] TileEntity active/valid -> lifecycle state；destroy/reload 语义必须明确。
- [ ] TileEntity linked NPC/entity -> typed entity handle；linked entity stale 时有清理规则。
- [ ] TileEntity owner/lock -> authorization state；不得用 session/packet owner 代替。
- [ ] TileEntity revision -> domain revision；持久化冲突控制，不是 network revision。
- [ ] TileEntity persistent values -> value-only state；不保存 object reference/closure。
- [ ] door anchor X/Y -> `DoorStateComponent`；Tile mutation coordinate。
- [ ] door open/closed -> `DoorStateComponent`；server collision/interaction state。
- [ ] door locked/permission -> door authority policy；必须能由 server command 改变。
- [ ] door frame/animation -> `excluded`。
- [ ] wire mask/state -> `WireNetworkComponent`；server mechanism state。
- [ ] actuator enabled/disabled -> `ActuatorStateComponent`；Tile mutation rule。
- [ ] wire traversal queue -> bounded typed worklist；禁止 legacy static scratch/packet queue。
- [ ] pump/liquid transfer state -> liquid commands/work items；与 wire topology分离。
- [ ] pressure plate/switch state -> `MechanismActivationCommand`/component；只保留 active rule。
- [ ] Teleport pylon anchor/destination -> validated server teleport domain；deferred。
- [ ] TrainingDummy identity -> `TrainingDummyStateComponent`；持久化/实体关联。
- [ ] TrainingDummy tile anchor -> state component；server geometry。
- [ ] TrainingDummy active/valid -> state component；生命周期。
- [ ] TrainingDummy linked NPC/entity -> typed handle；stale link cleanup。
- [ ] TrainingDummy owner/lock -> authority state；业务权限而非请求字段。
- [ ] TrainingDummy hitbox -> server collision snapshot；不保存 draw bounds。
- [ ] TrainingDummy revision -> domain revision；服务器 state conflict。
- [ ] TrainingDummy deterministic update state -> typed state；message-87 的 message shape 不进入。
- [ ] TrainingDummy request/response/message fields -> `excluded`；当前只有 bounded source contract，不能扩大为完整 family parity。

## 13. WorldGen.cs 服务器必要字段/属性逐项核对清单

WorldGen 是当前最明显的拖慢方向。当前结构化 inventory 为 `684` 个 methods、`233` 个
fields；methods 为 `162` partial、`522` unmapped，fields 仍为 `233` unmapped。下面只
核对字段/属性的 server state 归属；方法本身不在本清单内，也不因为某个 pass 已有 typed
owner 就关闭全局删除门。

### 13.1 Secret seed 和规则字段

| ID | 核对 | 旧字段 | 目标 owner | 服务器用途 | 状态 |
| --- | --- | --- | --- | --- | --- |
| WGEN-S-001 | [ ] | `AllSecretSeeds` | `SecretSeedDefinitionRegistry` + rule snapshot | seed definition set | partial |
| WGEN-S-002 | [ ] | `paintEverythingGray` | world rule snapshot | generation rule | partial |
| WGEN-S-003 | [ ] | `paintEverythingNegative` | world rule snapshot | generation rule | partial |
| WGEN-S-004 | [ ] | `coatEverythingEcho` | world rule snapshot | tile/coating rule | partial |
| WGEN-S-005 | [ ] | `coatEverythingIlluminant` | world rule snapshot | tile/coating rule | partial |
| WGEN-S-006 | [ ] | `noSurface` | terrain/generation rule | surface generation gate | partial |
| WGEN-S-007 | [ ] | `extraLivingTrees` | tree generation policy | tree count rule | partial |
| WGEN-S-008 | [ ] | `extraFloatingIslands` | structure generation policy | island count rule | partial |
| WGEN-S-009 | [ ] | `errorWorld` | world rule snapshot | Error World rule | partial |
| WGEN-S-010 | [ ] | `graveyardBloodmoonStart` | progression/environment policy | starting event rule | deferred |
| WGEN-S-011 | [ ] | `surfaceIsInSpace` | terrain profile | surface environment rule | deferred |
| WGEN-S-012 | [ ] | `rainsForAYear` | weather/progression state | persistent weather rule | deferred |
| WGEN-S-013 | [ ] | `biggerAbandonedHouses` | structure definition/policy | house generation | partial |
| WGEN-S-014 | [ ] | `randomSpawn` | spawn policy | player/NPC spawn choice | deferred |
| WGEN-S-015 | [ ] | `addTeleporters` | structure/teleport pylon policy | world object placement | deferred |
| WGEN-S-016 | [ ] | `startInHardmode` | progression state | hardmode bootstrap | deferred |
| WGEN-S-017 | [ ] | `noInfection` | biome/infection policy | infection gate | partial |
| WGEN-S-018 | [ ] | `hallowOnTheSurface` | biome surface policy | hallow surface rule | deferred |
| WGEN-S-019 | [ ] | `worldIsInfected` | biome/infection state | infection mode | partial |
| WGEN-S-020 | [ ] | `surfaceIsMushrooms` | biome surface policy | mushroom surface rule | partial |
| WGEN-S-021 | [ ] | `surfaceIsDesert` | biome surface policy | desert surface rule | partial |
| WGEN-S-022 | [ ] | `pooEverywhere` | tile generation policy | generation recipe | deferred |
| WGEN-S-023 | [ ] | `noSpiderCaves` | cave generation policy | spider cave gate | deferred |
| WGEN-S-024 | [ ] | `actuallyNoTraps` | trap generation policy | trap gate | partial |
| WGEN-S-025 | [ ] | `rainbowStuff` | generation/biome policy | rainbow recipe | deferred |
| WGEN-S-026 | [ ] | `digExtraHoles` | cave generation policy | hole count/rule | deferred |
| WGEN-S-027 | [ ] | `roundLandmasses` | terrain generation policy | landmass shape | deferred |
| WGEN-S-028 | [ ] | `extraLiquid` | liquid generation policy | liquid amount/rule | partial |
| WGEN-S-029 | [ ] | `portalGunInChests` | structure/item placement policy | chest content rule | deferred |
| WGEN-S-030 | [ ] | `worldIsFrozen` | world generation/finalize policy | frozen world mutation | partial |
| WGEN-S-031 | [ ] | `halloweenGen` | seasonal generation policy | pumpkin/season rule | partial |
| WGEN-S-032 | [ ] | `endlessHalloween` | seasonal progression policy | persistent event rule | deferred |
| WGEN-S-033 | [ ] | `endlessChristmas` | seasonal progression policy | persistent event rule | deferred |
| WGEN-S-034 | [ ] | `vampirism` | secret-seed runtime projection | server rule input | partial |
| WGEN-S-035 | [ ] | `teamBasedSpawns` | spawn policy | team spawn rule | partial |
| WGEN-S-036 | [ ] | `dualDungeons` | dungeon generation policy | dual dungeon rule | partial |
| WGEN-S-037 | [ ] | `Localization` | no Simulation owner | text/localization | excluded |
| WGEN-S-038 | [ ] | `_code` | no Simulation owner | unlock code/text | excluded |
| WGEN-S-039 | [ ] | `_sound` | no Simulation owner | sound asset | excluded |
| WGEN-S-040 | [ ] | `_plaintext` | no Simulation owner | UI text | excluded |
| WGEN-S-041 | [ ] | `TextThatWasUsedToUnlock` | host/UI boundary | unlock presentation text | excluded |
| WGEN-S-042 | [ ] | `activeSecretSeedCount` | `SecretSeedRuleSnapshotSet` | active rule count | partial |
| WGEN-S-043 | [ ] | `_enabled` | `SecretSeedRuleSnapshotSet` | enabled rule state | partial |

### 13.2 Secret seed 和 Skyblock 属性

- [ ] `WGEN-SP-001` `paintEverythingGrayJustTheSurface` -> `SecretSeedVariationQuery`；server
  generation predicate；partial。
- [ ] `WGEN-SP-002` `paintEverythingGrayJustTreasure` -> `SecretSeedVariationQuery`；server
  generation predicate；partial。
- [ ] `WGEN-SP-003` `paintEverythingGrayUseWhite` -> `SecretSeedVariationQuery`；rule input；partial。
- [ ] `WGEN-SP-004` `paintEverythingNegativeJustUnderground` -> variation query；partial。
- [ ] `WGEN-SP-005` `paintEverythingNegativeJustSomeThings` -> variation query；partial。
- [ ] `WGEN-SP-006` `coatEverythingJustInnerBlocks` -> variation query；partial。
- [ ] `WGEN-SP-007` `coatEverythingEchoJustSomeThings` -> variation query；partial。
- [ ] `WGEN-SP-008` `coatEverythingIlluminantJustRandomSpots` -> variation query；partial。
- [ ] `WGEN-SP-009` `coatEverythingIlluminantJustSomeThings` -> variation query；partial。
- [ ] `WGEN-SP-010` `noSurfaceNoFloatingIslands` -> variation query；generation gate；partial。
- [ ] `WGEN-SP-011` `noSurfaceNoLivingTrees` -> variation query；generation gate；partial。
- [ ] `WGEN-SP-012` `noSurfaceNoPyramids` -> variation query；generation gate；partial。
- [ ] `WGEN-SP-013` `noSurfaceNoSwordShrines` -> variation query；generation gate；partial。
- [ ] `WGEN-SP-014` `extraLivingTreesReducedAmount` -> variation query；count multiplier；partial。
- [ ] `WGEN-SP-015` `extraFloatingIslandsNormalAmount` -> variation query；count multiplier；partial。
- [ ] `WGEN-SP-016` `extraFloatingIslandsReducedAmount` -> variation query；count multiplier；partial。
- [ ] `WGEN-SP-017` `errorWorldBalancedChests` -> variation query；chest generation rule；partial。
- [ ] `WGEN-SP-018` `noSpiderCavesActuallyNoSpiderCaves` -> variation query；cave gate；partial。
- [ ] `WGEN-SP-019` `noSpiderCavesILiedMoreSpiderCaves` -> variation query；cave rule；partial。
- [ ] `WGEN-SP-020` `actuallyNoTrapsForRealIMeanIt` -> variation query；trap gate；partial。
- [ ] `WGEN-SP-021` `surfaceIsDesertNormalFunction` -> variation query；biome rule；partial。
- [ ] `WGEN-SP-022` `surfaceIsDesertSwapDesertAndSnowBiomes` -> variation query；biome rule；partial。
- [ ] `WGEN-SP-023` `SecretSeed.Enabled` -> immutable rule property；不能作为可写 static；partial。
- [ ] `WGEN-SP-024` `GenerateBiggerAbandonedHouses` -> generation policy query；partial。
- [ ] `WGEN-SP-025` `GenerateRainbowGlowsticks` -> generation policy query；partial。
- [ ] `WGEN-SP-026` `Skyblock.denyFloatingIslands` -> `SkyblockPolicyQuery`；partial。
- [ ] `WGEN-SP-027` `Skyblock.denyAllGeneration` -> `SkyblockPolicyQuery`；partial。
- [ ] `WGEN-SP-028` `Skyblock.denySomeGeneration` -> `SkyblockPolicyQuery`；partial。
- [ ] `WGEN-SP-029` `Skyblock.spawnSolidifier` -> world generation state/policy；partial。
- [ ] `WGEN-SP-030` `Skyblock.spawnShimmerPool` -> world generation state/policy；partial。
- [ ] `WGEN-SP-031` `TransformingWorld` -> `WorldTransformationStateSnapshot`；partial。
- [ ] `WGEN-SP-032` `genRand` -> `GenerationRandomState`；scoped value，不是 global field；partial。
- [ ] `WGEN-SP-033` `oceanLevel` -> `OceanLevelQuery`/terrain profile；partial。

| ID | 核对 | 旧字段 | 目标 owner | 服务器用途 | 状态 |
| --- | --- | --- | --- | --- | --- |
| WGEN-SK-001 | [ ] | `noAltars` | `SkyblockRuleSnapshot` | generation scan rule | partial |
| WGEN-SK-002 | [ ] | `noDungeon` | `SkyblockRuleSnapshot` | dungeon generation gate | partial |
| WGEN-SK-003 | [ ] | `noTemple` | `SkyblockRuleSnapshot` | temple generation gate | partial |
| WGEN-SK-004 | [ ] | `noHellstone` | `SkyblockRuleSnapshot` | ore generation gate | partial |
| WGEN-SK-005 | [ ] | `noFossils` | `SkyblockRuleSnapshot` | fossil generation gate | partial |
| WGEN-SK-006 | [ ] | `noLifeCrystals` | `SkyblockRuleSnapshot` | life crystal generation gate | partial |
| WGEN-SK-007 | [ ] | `noHellforge` | `SkyblockRuleSnapshot` | hellforge generation gate | partial |
| WGEN-SK-008 | [ ] | `lowTiles` | `TilePresenceScanResult` | active tile ratio | partial |
| WGEN-SK-009 | [ ] | `hasTile` | `TilePresenceScanResult` | type presence set | partial |
| WGEN-SK-010 | [ ] | `hasWall` | `TilePresenceScanResult` | wall presence set | partial |
| WGEN-SK-011 | [ ] | `currentActiveTiles` | `TilePresenceScanResult`/counter | active tile count | partial |

### 13.3 Saved ore tier 和树配置

| ID | 核对 | 旧字段 | 目标 owner | 服务器用途 | 状态 |
| --- | --- | --- | --- | --- | --- |
| WGEN-ORE-001 | [ ] | `Copper` | `OreDefinition` | pre-hardmode ore type | partial |
| WGEN-ORE-002 | [ ] | `Iron` | `OreDefinition` | pre-hardmode ore type | partial |
| WGEN-ORE-003 | [ ] | `Silver` | `OreDefinition` | pre-hardmode ore type | partial |
| WGEN-ORE-004 | [ ] | `Gold` | `OreDefinition` | pre-hardmode ore type | partial |
| WGEN-ORE-005 | [ ] | `Cobalt` | `SavedOreTierDefaults` | hardmode ore tier | partial |
| WGEN-ORE-006 | [ ] | `Mythril` | `SavedOreTierDefaults` | hardmode ore tier | partial |
| WGEN-ORE-007 | [ ] | `Adamantite` | `SavedOreTierDefaults` | hardmode ore tier | partial |
| WGEN-TREE-001 | [ ] | `GemTree_Ruby` | `LegacyTreeProfileDefinition` | tree profile | partial |
| WGEN-TREE-002 | [ ] | `GemTree_Diamond` | `LegacyTreeProfileDefinition` | tree profile | partial |
| WGEN-TREE-003 | [ ] | `GemTree_Topaz` | `LegacyTreeProfileDefinition` | tree profile | partial |
| WGEN-TREE-004 | [ ] | `GemTree_Amethyst` | `LegacyTreeProfileDefinition` | tree profile | partial |
| WGEN-TREE-005 | [ ] | `GemTree_Sapphire` | `LegacyTreeProfileDefinition` | tree profile | partial |
| WGEN-TREE-006 | [ ] | `GemTree_Emerald` | `LegacyTreeProfileDefinition` | tree profile | partial |
| WGEN-TREE-007 | [ ] | `GemTree_Amber` | `LegacyTreeProfileDefinition` | tree profile | partial |
| WGEN-TREE-008 | [ ] | `VanityTree_Sakura` | `LegacyTreeProfileDefinition` | tree profile | partial |
| WGEN-TREE-009 | [ ] | `VanityTree_Willow` | `LegacyTreeProfileDefinition` | tree profile | partial |
| WGEN-TREE-010 | [ ] | `Tree_Ash` | `LegacyTreeProfileDefinition` | tree profile | partial |
| WGEN-TREE-011 | [ ] | `TreeTileType` | `TreeDefinition` | tree tile rule | partial |
| WGEN-TREE-012 | [ ] | `TreeHeightMin` | `TreeDefinition` | tree height rule | partial |
| WGEN-TREE-013 | [ ] | `TreeHeightMax` | `TreeDefinition` | tree height rule | partial |
| WGEN-TREE-014 | [ ] | `TreeTopPaddingNeeded` | `TreeDefinition` | footprint rule | partial |
| WGEN-TREE-015 | [ ] | `GroundTest` | `TreeCheckSettingsQuery` | ground eligibility | partial |
| WGEN-TREE-016 | [ ] | `WallTest` | `TreeCheckSettingsQuery` | wall eligibility | partial |
| WGEN-TREE-017 | [ ] | `SaplingTileType` | `TreeDefinition` | sapling rule | partial |
| WGEN-TREE-018 | [ ] | `IsGroundValid` | tree eligibility query | derived ground result | partial |

### 13.4 Tile merge、frame、计数和房屋 scratch

| ID | 核对 | 旧字段 | 目标 owner | 服务器用途 | 状态 |
| --- | --- | --- | --- | --- | --- |
| WGEN-FR-001 | [ ] | `CullTop` | `TileMergeCullMask` | frame neighbor cull | partial |
| WGEN-FR-002 | [ ] | `CullBottom` | `TileMergeCullMask` | frame neighbor cull | partial |
| WGEN-FR-003 | [ ] | `CullLeft` | `TileMergeCullMask` | frame neighbor cull | partial |
| WGEN-FR-004 | [ ] | `CullRight` | `TileMergeCullMask` | frame neighbor cull | partial |
| WGEN-FR-005 | [ ] | `CullTopLeft` | `TileMergeCullMask` | diagonal cull | partial |
| WGEN-FR-006 | [ ] | `CullTopRight` | `TileMergeCullMask` | diagonal cull | partial |
| WGEN-FR-007 | [ ] | `CullBottomLeft` | `TileMergeCullMask` | diagonal cull | partial |
| WGEN-FR-008 | [ ] | `CullBottomRight` | `TileMergeCullMask` | diagonal cull | partial |
| WGEN-FR-009 | [ ] | `tileReframeCount` | `TileFrameBudget` | bounded frame recursion | partial |
| WGEN-COUNT-001 | [ ] | `tileCounts` | `TileTypeCountSnapshot` | world tile counts | partial |
| WGEN-COUNT-002 | [ ] | `totalEvil` | infection scan state | evil weighted total | partial |
| WGEN-COUNT-003 | [ ] | `totalBlood` | infection scan state | crimson weighted total | partial |
| WGEN-COUNT-004 | [ ] | `totalGood` | infection scan state | hallow weighted total | partial |
| WGEN-COUNT-005 | [ ] | `totalSolid` | infection scan state | solid denominator | partial |
| WGEN-COUNT-006 | [ ] | `totalEvil2` | infection accumulator | stage total | partial |
| WGEN-COUNT-007 | [ ] | `totalBlood2` | infection accumulator | stage total | partial |
| WGEN-COUNT-008 | [ ] | `totalGood2` | infection accumulator | stage total | partial |
| WGEN-COUNT-009 | [ ] | `totalSolid2` | infection accumulator | stage total | partial |
| WGEN-COUNT-010 | [ ] | `tEvil` | infection column state | column total | partial |
| WGEN-COUNT-011 | [ ] | `tBlood` | infection column state | column total | partial |
| WGEN-COUNT-012 | [ ] | `tGood` | infection column state | column total | partial |
| WGEN-COUNT-013 | [ ] | `totalX` | `TileCountSchedulingState` | column cursor | partial |
| WGEN-COUNT-014 | [ ] | `totalD` | `TileCountSchedulingState` | cadence cursor | partial |
| WGEN-COUNT-015 | [ ] | `numTileCount` | tile count capacity state | current count | partial |
| WGEN-COUNT-016 | [ ] | `maxTileCount` | tile count capacity policy | scan cap | partial |
| WGEN-COUNT-017 | [ ] | `CountedTiles` | immutable visited snapshot | scan dedupe | partial |
| WGEN-COUNT-018 | [ ] | `trapDiag` | diagnostic-only local state | trap metric if consumed | deferred |
| WGEN-COUNT-019 | [ ] | `gem` | `GemTileRandomPolicy` | gem selection | partial |
| WGEN-COUNT-020 | [ ] | `mossType` | `MossSelectionPolicy` | moss selection | partial |
| WGEN-COUNT-021 | [ ] | `neonMossType` | `MossSelectionPolicy` | neon moss selection | partial |

| ID | 核对 | 旧字段 | 目标 owner | 服务器用途 | 状态 |
| --- | --- | --- | --- | --- | --- |
| WGEN-HOUSE-001 | [ ] | `TownManager` | housing/structure context | room/housing authority | partial |
| WGEN-HOUSE-002 | [ ] | `maxRoomTiles` | housing room policy | scan capacity | partial |
| WGEN-HOUSE-003 | [ ] | `maxRoomSize` | housing room policy | room size bound | partial |
| WGEN-HOUSE-004 | [ ] | `roomTiles` | room scan snapshot | visited room tiles | partial |
| WGEN-HOUSE-005 | [ ] | `numRoomTiles` | room scan result | room tile count | partial |
| WGEN-HOUSE-006 | [ ] | `roomX1` | room bounds snapshot | left bound | partial |
| WGEN-HOUSE-007 | [ ] | `roomX2` | room bounds snapshot | right bound | partial |
| WGEN-HOUSE-008 | [ ] | `roomY1` | room bounds snapshot | top bound | partial |
| WGEN-HOUSE-009 | [ ] | `roomY2` | room bounds snapshot | bottom bound | partial |
| WGEN-HOUSE-010 | [ ] | `canSpawn` | housing/spawn decision | spawn eligibility | partial |
| WGEN-HOUSE-011 | [ ] | `houseTile` | housing tile policy | home anchor tile | partial |
| WGEN-HOUSE-012 | [ ] | `bestX` | housing candidate result | best room X | deferred |
| WGEN-HOUSE-013 | [ ] | `bestY` | housing candidate result | best room Y | deferred |
| WGEN-HOUSE-014 | [ ] | `hiScore` | housing candidate result | room score | deferred |
| WGEN-HOUSE-015 | [ ] | `roomTorch` | room needs result | torch requirement | partial |
| WGEN-HOUSE-016 | [ ] | `roomDoor` | room needs result | door requirement | partial |
| WGEN-HOUSE-017 | [ ] | `roomChair` | room needs result | chair requirement | partial |
| WGEN-HOUSE-018 | [ ] | `roomTable` | room needs result | table requirement | partial |
| WGEN-HOUSE-019 | [ ] | `roomHasStinkbug` | housing spawn gate | stinkbug room gate | partial |
| WGEN-HOUSE-020 | [ ] | `roomHasEchoStinkbug` | housing spawn gate | echo-stinkbug room gate | partial |
| WGEN-HOUSE-021 | [ ] | `LastFoundHouse` | host/housing result snapshot | last successful house | deferred |
| WGEN-HOUSE-022 | [ ] | `currentlyTryingToUseAlternateHousingSpot` | housing recursion state | reentrancy gate | partial |
| WGEN-HOUSE-023 | [ ] | `sharedRoomX` | housing result | shared room coordinate | deferred |
| WGEN-HOUSE-024 | [ ] | `_roomCheckStack` | bounded room query workspace | recursive scan stack | partial；不得成为 global singleton |
| WGEN-HOUSE-025 | [ ] | `roomCheckFailureReason` | room result enum | deterministic rejection | partial |

### 13.5 WorldGen 运行时、合并、生成生命周期和随机状态

| ID | 核对 | 旧字段 | 目标 owner | 服务器用途 | 状态 |
| --- | --- | --- | --- | --- | --- |
| WGEN-RUN-001 | [ ] | `crimson` | `WorldEvilSelectionPolicy` | evil/biome selection | partial |
| WGEN-RUN-002 | [ ] | `generatingRandomEvil` | generation stage state | random evil selection phase | partial |
| WGEN-RUN-003 | [ ] | `WorldGenParam_Evil` | `WorldGenerationRequest` | explicit evil parameter | partial |
| WGEN-RUN-004 | [ ] | `_transformingWorld` | transformation snapshot | transformation state | partial |
| WGEN-RUN-005 | [ ] | `spawnEye` | progression event state | boss spawn intent | partial |
| WGEN-RUN-006 | [ ] | `spawnHardBoss` | progression event state | boss spawn intent | partial |
| WGEN-RUN-007 | [ ] | `shadowOrbSmashed` | progression state | orb progression flag | partial |
| WGEN-RUN-008 | [ ] | `shadowOrbCount` | progression state | orb count | partial |
| WGEN-RUN-009 | [ ] | `altarCount` | progression state | altar count | partial |
| WGEN-RUN-010 | [ ] | `builtHouseWithNoFurniture` | housing result state | generation outcome | deferred；当前仅 reset/无 consumer |
| WGEN-RUN-011 | [ ] | `builtHouseWithNoLight` | housing result state | generation outcome | deferred；当前仅 reset/无 consumer |
| WGEN-RUN-012 | [ ] | `spawnMeteor` | progression state | meteor schedule | partial |
| WGEN-RUN-013 | [ ] | `loadFailed` | `WorldLoadRecoveryPolicy` | load retry/failure | host-only/partial |
| WGEN-RUN-014 | [ ] | `worldCleared` | generation lifecycle result | clear completion | deferred |
| WGEN-RUN-015 | [ ] | `worldBackup` | host persistence recovery | backup availability | host-only |
| WGEN-RUN-016 | [ ] | `lastMaxTilesX` | `PreviousWorldBoundsSnapshot` | resize/restart detection | partial |
| WGEN-RUN-017 | [ ] | `lastMaxTilesY` | `PreviousWorldBoundsSnapshot` | resize/restart detection | partial |
| WGEN-RUN-018 | [ ] | `npcSpawnDelay` | `TownNpcSpawnCadencePolicy` | town spawn cadence | partial |
| WGEN-RUN-019 | [ ] | `npcSpawnPeriod` | `TownNpcSpawnCadencePolicy` | town spawn cadence | partial |
| WGEN-RUN-020 | [ ] | `prioritizedTownNPCType` | town spawn selector | server spawn candidate | partial |
| WGEN-RUN-021 | [ ] | `meteorShowerCount` | meteor progression policy | meteor schedule count | partial |
| WGEN-RUN-022 | [ ] | `generatingWorld` | `WorldGenerationLifecycleSnapshot` | generation active state | partial |
| WGEN-RUN-023 | [ ] | `isGeneratingOrLoadingWorld` | `WorldGenerationLifecycleSnapshot` | generation/load state | partial |
| WGEN-RUN-024 | [ ] | `generatingWorldOnThisThread` | host thread state | thread-local generation marker | excluded/host-only |
| WGEN-RUN-025 | [ ] | `remixWorldGen` | `WorldRuleSnapshotComponent` | generation mode | partial |
| WGEN-RUN-026 | [ ] | `everythingWorldGen` | `WorldRuleSnapshotComponent` | generation mode | partial |
| WGEN-RUN-027 | [ ] | `noTrapsWorldGen` | `TrapGenerationGatePolicy` | trap gate | partial |
| WGEN-RUN-028 | [ ] | `drunkWorldGen` | `WorldRuleSnapshotComponent` | generation mode | partial |
| WGEN-RUN-029 | [ ] | `getGoodWorldGen` | `WorldRuleSnapshotComponent` | generation mode | partial |
| WGEN-RUN-030 | [ ] | `tenthAnniversaryWorldGen` | `WorldRuleSnapshotComponent` | generation mode | deferred |
| WGEN-RUN-031 | [ ] | `dontStarveWorldGen` | `WorldRuleSnapshotComponent` | generation mode | partial |
| WGEN-RUN-032 | [ ] | `notTheBees` | `WorldRuleSnapshotComponent` | generation mode | partial |
| WGEN-RUN-033 | [ ] | `skyblockWorldGen` | `WorldRuleSnapshotComponent` | generation mode | partial |
| WGEN-RUN-034 | [ ] | `placingTraps` | trap generation phase state | trap mutation phase | deferred |
| WGEN-RUN-035 | [ ] | `ItemSpawnProtectionTime` | item spawn protection policy | world item pickup protection | partial |
| WGEN-RUN-036 | [ ] | `hardModeWorldUpdates` | world update policy | post-hardmode update gate | partial |
| WGEN-RUN-037 | [ ] | `growGrassUnderground` | world update policy | grass update gate | partial |
| WGEN-RUN-038 | [ ] | `_isRainingBoulders` | boulder event state | raining boulder event | partial |
| WGEN-RUN-039 | [ ] | `fossilBreak` | `FossilBreakPolicy` | one-shot fossil mutation gate | partial |
| WGEN-RUN-040 | [ ] | `grassSpread` | `GrassSpreadRecursionState` | bounded recursion state | partial |

| ID | 核对 | 旧字段 | 目标 owner | 服务器用途 | 状态 |
| --- | --- | --- | --- | --- | --- |
| WGEN-MERGE-001 | [ ] | `mergeUp` | `TileMergeQuery` | merge neighbor mapping | partial |
| WGEN-MERGE-002 | [ ] | `mergeDown` | `TileMergeQuery` | merge neighbor mapping | partial |
| WGEN-MERGE-003 | [ ] | `mergeLeft` | `TileMergeQuery` | merge neighbor mapping | partial |
| WGEN-MERGE-004 | [ ] | `mergeRight` | `TileMergeQuery` | merge neighbor mapping | partial |
| WGEN-MERGE-005 | [ ] | `stopDrops` | `WorldDropPolicyQuery` | suppress world drops | partial |
| WGEN-MERGE-006 | [ ] | `destroyObject` | object destruction guard | nested mutation guard | partial |
| WGEN-MERGE-007 | [ ] | `maxWallOut2` | `WallSpreadBudgetPolicy` | wall spread output budget | partial |
| WGEN-MERGE-008 | [ ] | `tileSolidBackup` | compatibility/local workspace | temporary table backup | excluded from Simulation |
| WGEN-MERGE-009 | [ ] | `_preventInfiniteRopeFraming` | housing/frame rule definition | recursion protection | partial |
| WGEN-MERGE-010 | [ ] | `BUBBLES_SOLID_STATE_FOR_HOUSING` | `TileHousingRuleSnapshot` | housing solidity rule | partial |
| WGEN-MERGE-011 | [ ] | `ExploitDestroyQueue` | bounded tile mutation worklist | server tile destruction intent | partial；不包含 wire dispatch |

| ID | 核对 | 旧字段 | 目标 owner | 服务器用途 | 状态 |
| --- | --- | --- | --- | --- | --- |
| WGEN-COUNT-022 | [ ] | `cactusWaterWidth` | terrain/cactus policy | cactus water geometry | partial |
| WGEN-COUNT-023 | [ ] | `cactusWaterHeight` | terrain/cactus policy | cactus water geometry | partial |
| WGEN-COUNT-024 | [ ] | `cactusWaterLimit` | terrain/cactus policy | cactus water budget | partial |
| WGEN-COUNT-025 | [ ] | `SmallConsecutivesFound` | clump metrics policy | clump scan result | partial |
| WGEN-COUNT-026 | [ ] | `SmallConsecutivesEliminated` | clump metrics policy | clump mutation count | partial |
| WGEN-COUNT-027 | [ ] | `catTailDistance` | `CatTailDistancePolicy` | cat-tail placement distance | partial |
| WGEN-COUNT-028 | [ ] | `heartPos` | `CrimsonHeartPositionSnapshot` | generated heart positions | partial |
| WGEN-COUNT-029 | [ ] | `heartCount` | `CrimsonHeartPositionSnapshot` | heart count/capacity | partial |
| WGEN-COUNT-030 | [ ] | `strip_w` | frame refresh local workspace | bounded strip width | deferred |
| WGEN-COUNT-031 | [ ] | `strip_h` | frame refresh local workspace | bounded strip height | deferred |
| WGEN-COUNT-032 | [ ] | `bitStrip` | frame refresh local workspace | bit scratch | deferred；禁止 singleton |

补齐 inventory 中的独立边界值字段：

- [ ] `WGEN-DIST-001` `oceanDistance` -> `WorldGenerationRequest.DistanceDefaults`；ocean boundary；partial。
- [ ] `WGEN-DIST-002` `beachDistance` -> `WorldGenerationRequest.DistanceDefaults`；beach boundary；partial。
- [ ] `WGEN-DIST-003` `shimmerSafetyDistance` -> generation/structure safety policy；partial。
- [ ] `WGEN-DIST-004` `InfectionAndGrassSpreadOuterWorldBuffer` -> infection/grass scan policy；partial。
- [ ] `WGEN-RULE-001` `AllowedToSpreadInfections` -> `WorldUpdatePolicySnapshot`；post-worldgen infection rule；partial。
- [ ] `WGEN-ENV-001` `lavaCount` -> `TileCountEnvironmentCounters`；server environment count；partial。
- [ ] `WGEN-ENV-002` `iceCount` -> `TileCountEnvironmentCounters`；server environment count；partial。
- [ ] `WGEN-ENV-003` `sandCount` -> `TileCountEnvironmentCounters`；server environment count；partial。
- [ ] `WGEN-ENV-004` `rockCount` -> `TileCountEnvironmentCounters`；server environment count；partial。
- [ ] `WGEN-ENV-005` `shroomCount` -> `TileCountEnvironmentCounters`；server environment count；partial。
- [ ] `WGEN-SIZE-001` `WorldSizeSmallX` -> world-size definition input；未消费常量先 deferred。
- [ ] `WGEN-SIZE-002` `WorldSizeSmallY` -> world-size definition input；未消费常量先 deferred。
- [ ] `WGEN-SIZE-003` `WorldSizeMediumX` -> world-size definition input；未消费常量先 deferred。
- [ ] `WGEN-SIZE-004` `WorldSizeMediumY` -> world-size definition input；未消费常量先 deferred。
- [ ] `WGEN-SIZE-005` `WorldSizeLargeX` -> world-size definition input；未消费常量先 deferred。
- [ ] `WGEN-SIZE-006` `WorldSizeLargeY` -> world-size definition input；未消费常量先 deferred。

### 13.6 WorldGen 客户端/宿主/表现排除字段

这些字段来自 WorldGen inventory，但不是服务器必要状态；必须逐项保持排除，不能为了
达到 `233` 个 field 的覆盖而创建空组件：

- [ ] `Manifest` -> 版本/hash/host compatibility metadata；不进入 Simulation state。
- [ ] `treeBG1`、`treeBG2`、`treeBG3`、`treeBG4` -> background presentation；excluded。
- [ ] `corruptBG`、`jungleBG`、`snowBG`、`hallowBG`、`crimsonBG`、`desertBG`、`oceanBG`、
  `mushroomBG`、`underworldBG` -> background presentation；excluded。
- [ ] `mysticLogsEvent` -> host logging/audio event；excluded。
- [ ] `_generator` -> legacy generator object/reference；pipeline composition only，excluded。
- [ ] `BackgroundsCache` -> presentation/cache；excluded。
- [ ] `_SpawnThunderStorm_SafeSpots` -> visual/effect safe-point cache；若未证明改变权威天气，
  excluded；不得放入 WorldGenerationState。
- [ ] `drunkWorldGenText` -> UI/status text；excluded。
- [ ] `_coatingColors` -> client Color list；excluded。
- [ ] `TreeTops` 中仅供背景/渲染的部分 -> client presentation；server 不保存。

### 13.7 WorldGen 当前执行方向：Pyramid wall-frame evaluation

这部分是当前正在迁移的方向，但仍是 `completed_partial` 之前的计划，不是已经完成的
全量 WorldGen 迁移：

- [ ] 复用已有 `LegacyWallFrameNeighborQuery`，不再建立第二套 neighbor predicate。
- [ ] 冻结 truncating tile registry、strict-interior guard、source bit order 和 invisible-wall
  mode；输入只能是 immutable `TileReadSnapshot`。
- [ ] 建立 `Framing.WallFrame` lookup/value definition；记录完整 lookup 默认值和非法 wall
  type 行为。
- [ ] 将 reset-frame random 变成 Pyramid pass scoped stream；记录 `(seed, streamVersion,
  stage, cursor)`，不读 `Main.rand`。
- [ ] 用 typed wall-frame evaluation result 表达 neighbor mask、value、rejection reason，
  不将数组或临时 random state挂到 World singleton。
- [ ] 按 Pyramid source order 产生 wall-frame mutation command；重复目标和 source sequence
  必须保留，commit 只能发生在唯一 Tile/Wall commit seam。
- [ ] 结构 tunnel/features、client SceneMetrics、aggregate pass order、global RNG/WLD
  parity、legacy deletion 均不属于该窄片，保持 deferred。
- [ ] 不将 wall-frame command 转换为 packet/frame payload；本计划只保留服务器 Tile/Wall
  变更意图。

## 14. 组件接口、读集、写集和提交边界

本节把“组件怎么设计”落实为可以逐项核对的接口契约。这里的 Interface 不只指 C# 类型
签名，还包括允许读者、默认值、写入时机、失败方式、持久化语义和性能边界。当前不编写
函数；执行时先完成这些契约卡，再决定是否需要最小实现。

### 14.1 World 组件契约卡

| 组件 | Interface 必须承诺 | 允许写入者/写集 | 允许读取者/读集 | 必须拒绝 |
| --- | --- | --- | --- | --- |
| `WorldMetadata` | 尺寸、边界、spawn、surface/rock、seed variant、generator version 的值域、默认值和版本 | bootstrap/import、明确的 world transition | clock/rule/generation/entity systems | file handle、path、client view、packet flag |
| `WorldGrid` | `(x,y)` 合法性、dense tile value、section ownership、revision 单调性 | Tile/Liquid/Structure commit | immutable grid/tile snapshots、collision、generation | arbitrary system direct mutation、每 Tile entity、render cache |
| `WorldClock` | day/night/time/moon/pause/rate 的一致性和跨日规则 | `WorldClockSystem` transition | weather/progression/spawn systems | client visual time、多个可写 clock |
| `WorldRuleState` | seed/mode/PVP/hardmode/secret rule 的互斥、优先级和版本 | bootstrap、rule command/commit | world/event/generation systems | localized text、unlock UI、wire bits |
| `WorldProgressionState` | boss/event/invasion/slime/meteor state 的生命周期和持久化分类 | typed progression commands | spawn/loot/environment systems | warning message fields、client display marker |
| `WorldWeatherState` | rain/wind amount, target, counter and bounded transitions | weather system | environment/spawn/world update systems | cloud alpha、sound、particle、visual-only wind |
| `WorldGenerationRequest` | 一次生成所需的冻结输入完整且可重放 | bootstrap only | pipeline/stage systems | hidden static `Main`/legacy generator reads |
| `WorldGenerationState` | stage/cursor/failure/recovery 的显式状态 | pipeline lifecycle commit | generation orchestration | thread object、UI progress string |
| `GenerationCursorComponent` | stage-local cursor 和 sequence scope | one generation stage | same stage and handoff snapshot | global counter reused by unrelated pass |
| `LegacyPassRandomState` | seed、stream version、stage、cursor、checkpoint | pass owner only | current pass only | global mutable `Main.rand` |
| `WorldGenerationCommandBuffer` | source/priority/simulation sequence/coordinate ordering and bounded capacity | generation systems | single commit system | packet payload, broadcast recipient, anonymous Action |
| `LiquidWorldStateComponent` | known liquid kinds、capacity、dirty sections、propagation budget | liquid systems/commit | liquid and Tile systems | wire liquid encoding、visual alpha |
| `LiquidWorkItemComponent` | coordinate/kind/amount/source/retry budget with deterministic ordering | liquid input/propagation | liquid commit | unbounded queue、network frame cursor |

### 14.2 Player 组件契约卡

| 组件 | Interface 必须承诺 | 允许写入者/写集 | 禁止混入 |
| --- | --- | --- | --- |
| `PlayerIdentityComponent` | entity/profile identity 稳定且可关联 | bootstrap/session binding | wire slot、packet identity encoding |
| `PlayerAuthorityComponent` | session-to-player binding、权限、命令接受和清理 | Server authority seam | raw message、transport state |
| `PlayerLifecycleComponent` | active/dead/respawn 状态机和清理顺序 | lifecycle/respawn commands | UI connected state |
| `PlayerTransformComponent` | position/velocity/facing 的有限值和更新顺序 | movement commit | camera/screen/interpolation cache |
| `PlayerMovementStateComponent` | grounded/liquid/fall/dash/jump server rules | movement system | keyboard object、render frame |
| `PlayerInputComponent` | normalized business intent、tick、authority 和 edge state | authorized command ingress | raw input frame、wire bit layout |
| `HealthComponent` | current/max life、damage/death transition | damage/heal commit | health bar/presentation |
| `ManaComponent` | current/max mana、regen、potion/use delay | combat/use commit | mana effect visual |
| `PlayerEnvironmentContactComponent` | wet/lava/honey/shimmer/breath authority | environment system | ambient visual state |
| `PlayerSpawnStateComponent` | validated spawn candidate、pending clear/respawn | spawn/respawn system | menu spawn UI |
| `PlayerInventoryComponent` | runtime slot count, item instance ownership, selected slot and revision | inventory command system | legacy 58-slot alias、packet slot array |
| `EquipmentComponent` | loadout/equipment references and server stat source | equipment commit | dye/armor render values |
| `BuffCollectionComponent` | typed effect entries、remaining ticks、immunity and source | status-effect commit | buff icon/tooltip/packet state |
| `PlayerStatModifierComponent` | deterministic accumulation order and reset behavior | equipment/buff/mount systems | client-only stat display |
| `PlayerTeleportStateComponent` | validated destination、phase、cooldown and authority | teleport domain | teleport effect/packet |
| `PlayerCooldownStateComponent` | bounded cooldowns for use/hurt/dodge/interaction | corresponding domain system | network spam/cadence state |
| `PlayerDeathDropStateComponent` | once-only death/drop intent and cleanup | death/drop commit | tombstone message/visual effect |

### 14.3 NPC 组件契约卡

| 组件 | Interface 必须承诺 | 允许写入者/写集 | 禁止混入 |
| --- | --- | --- | --- |
| `NpcDefinitionComponent` | type/default stats/category/capability are immutable per definition | definition registry/bootstrap | net id encoding、texture/frame |
| `NpcAuthorityComponent` | spawn source、damage/interaction ownership | spawn/authority systems | replication state |
| `NpcBehaviorStateComponent` | family-specific typed state and bounded phase transitions | behavior system only | public `float[] ai`、all 158 families in one object |
| `NpcLifecycleComponent` | active/timeLeft/death/despawn/replacement transitions | lifecycle/slot systems | netSpam/skipped sync |
| `NpcTransformComponent` | position/velocity/collision-facing finite values | movement commit | oldPos/oldRot presentation history |
| `NpcTargetComponent` | valid target handle and deterministic tie-break | target selection/routing | encoded target index |
| `NpcCombatComponent` | life/damage/defense/faction/immune/hit result | combat/damage commit | hit sound/particle |
| `NpcHomeComponent` | home/timeout/door-return authority | home systems | dialogue/localized name |
| `NpcHomePublicationComponent` | server home baseline/change state | home publication system | outbound cursor |
| `NpcSpawnCycleStateComponent` | timeout marker and one-time consumption | spawn/lifecycle systems | message notification |
| `NpcInvasionParticipationComponent` | group/point/wave contribution | event/progression systems | invasion packet flags |
| `NpcSegmentLinkComponent` | root/parent/child/order/shared-life relation | segment lifecycle/follow systems | encoded segment slot |
| `NpcLootStateComponent` | source, once-only emission and drop intents | death/loot system | client loot animation |
| `NpcActivityStateComponent` | CheckActive keep-alive/inactivity/range state | activity/lifecycle systems | PVS/screen cursor |

### 14.4 Projectile、Item 和 WorldObjects 组件契约卡

| 领域 | Interface 组合 | 允许写集 | 禁止混入 |
| --- | --- | --- | --- |
| Projectile | definition + transform + owner + collision + combat + behavior + lifetime + cooldown | spawn, movement, damage, despawn commits | frame/alpha/light/oldPos、network update fields |
| Item definition | identity + combat + equipment + use + placement + recovery + prefix | immutable registry/compiler | stack/favorited/active/tooltip |
| Item instance | stack + instance + ownership + world state | item/inventory/world-item commands | global slot layout、client appearance |
| Inventory/Equipment | bounded slots、loadout、selected item、revision | transfer/equip/use systems | old 50/4/4 alias、wire slot index |
| Chest | identity + anchor + contents + access + lock + revision | chest create/open/transfer/close/destroy commands | request/response field、open packet |
| Sign | identity + anchor + text + active + revision + edit authority | validated sign edit/delete commands | font/scroll/wire text layout |
| TileEntity | type + anchor + linked handle + active + owner/lock + value state + revision | object placement/mutation commit | object reference、packet DTO |
| Door/Wire/Actuator | topology/anchor + active rule + bounded work + mutation revision | mechanism activation and Tile commit | wire color/render/broadcast scratch |
| TrainingDummy | identity + anchor + active + NPC link + owner/lock + hitbox + revision | activation/deactivation/hit state commit | message-87 request/response layout |

## 15. 分阶段执行计划

每个阶段都是一个独立的低耦合工作包。执行时只处理列出的 field ID 和写集，不顺手扩展
到相邻行为族。当前仅写入了本计划，下面是未来实现顺序，不代表任何阶段已经完成。

### Task 0：冻结服务器范围和字段卡模板

**Files / evidence:**

- Read: `progress.md`
- Read: `docs/flowstate/README.md`
- Read: `docs/flowstate/plan/2026-08-22-server-ecs-convergence.md`
- Read: `docs/flowstate/task/2026-08-22-server-ecs-convergence.md`
- Read: `docs/migrations/main-field-property-ecs-migration.md`
- Read: `docs/migrations/player-legacy-behavior-map.md`
- Read: `docs/migrations/npc-field-property-ecs-migration.md`
- Read: `docs/migrations/item-field-property-ecs-migration.md`
- Read: `docs/migrations/worldgen-field-property-migration.md`
- Read: `docs/migrations/version4-physical-deletion-ledger.csv`

**Checklist:**

- [ ] 建立唯一字段 ID，不用同名率或类名相似度判定完成。
- [ ] 为 Main、Player、NPC、Projectile、Item、WorldGen、Chest/Sign/Wiring/TileEntity
  的每个 server-required row 填 Source、Type、Static/Instance、Default、Mutability、
  Lifetime、Owner、Persistence、Status。
- [ ] 将所有 Protocol/transport/replication/client projection row 设为 `excluded`，并
  删除它们在 Simulation owner 候选中的位置。
- [ ] 保留身份字段的 runtime/persistence 用途，但不把 wire identity 当作字段迁移成果。
- [ ] 将不同日期/口径的 inventory 数字并列保存，禁止混算。

**Output:** only a field-card/owner decision record. No C# type, method or project reference is
added in this task.

### Task 1：建立 World 核心状态 seam

**Target files/modules:**

- `src/Terraria.Dome.Simulation/World/WorldMetadata.cs`
- `src/Terraria.Dome.Simulation/World/WorldGrid.cs`
- `src/Terraria.Dome.Simulation/World/WorldClock.cs`
- `src/Terraria.Dome.Simulation/World/WorldRuleState.cs`
- `src/Terraria.Dome.Simulation/World/WorldProgressionState.cs`
- `src/Terraria.Dome.Simulation/World/WorldRuntimeSnapshot.cs`
- `src/Terraria.Dome.Simulation/World/WorldGridSnapshot.cs`
- `src/Terraria.Dome.Server/Startup/WorldBootstrap.cs`

**Field set:** `M-WM-*`, `M-SEED-*`, `M-CLK-*`, `M-WEA-*`, `M-PRG-*`, `M-GRID-001..010`。

**Execution order:**

- [ ] 先固化 world metadata/bounds/spawn/seed/mode 的输入形状和非法尺寸处理。
- [ ] 再固化 clock/rate；derived values 不重复存储为可写字段。
- [ ] 再固化 weather/progression transition；每个跨 tick 值明确 reset/end 行为。
- [ ] 最后让 WorldGrid/section/liquid/frame 只接受 commit seam 的 typed mutation。
- [ ] 不把 `WorldPath`、client view、Protocol projection 或 render style放入 World component。

**Stop condition:** 只要一个字段同时有两个可写 owner、持久化默认值不明、或读写依赖旧
`Main`，本 task 停在 `partial/deferred`。

### Task 2：建立 bounded stores 和实体生命周期

**Target modules:** `PlayerStore`、`NpcStore`、`ProjectileStore`、`WorldItemStore`、Chest/
Sign/TileEntity stores、`SimulationEntityLimits`。

**Field set:** `M-ENT-*`、`PLY-001..021`、`NPC-LC-*`、`PROJ-LC-*`、world-object identity/
lifecycle rows。

- [ ] 为每个 store 明确 capacity、allocation、active、tombstone、reuse 和 revision 语义。
- [ ] Player/NPC/Projectile/Item 的 identity/handle 与 persistence association 分开记录。
- [ ] slot reuse 不能借助 `Dictionary` 名称伪装成旧数组等价；旧布局差异单独留在 deferred。
- [ ] 所有 despawn/death/destroy 结果必须有一次性 commit 语义。
- [ ] 不为 gore、rain、dust、star、cloud、combatText 创建服务器组件。

### Task 3：闭合 Player 服务器核心

**Target modules:** `Player/Components/*`、`Components/TransformComponent.cs`、
`Components/VelocityComponent.cs`、`Combat/Components/*`、`Items/InventoryComponent.cs`、
`Inventory/Components/*`、`Player/Systems/*`、`Players/PlayerPersistentState.cs`。

**Field set:** PLY identity/lifecycle/movement/input/vitals/inventory/use/interaction rows。

- [ ] 先保证 position/velocity/life/mana/active/dead/respawn 的最小权威链。
- [ ] 再接 normalized input；raw device and frame shape remain excluded。
- [ ] 再接 inventory/equipment/buff state；明确 runtime 40、legacy 58、persistence 990
  三种布局各自的 owner 和转换，不能 alias。
- [ ] 再处理 interaction/teleport/spawn；每个请求只留下 validated business intent。
- [ ] PB-001..PB-011 的专用机制继续列为 blocked/deferred，不能用字段增加掩盖。

### Task 4：闭合 NPC 生命周期、目标和基础战斗

**Target modules:** `Npc/Components/*`、`Npc/Systems/*`、`Npc/Definitions/*`、
`Npc/Snapshots/*`、`Npc/NpcStore.cs`。

**Field set:** `NPC-SP-*`、`NPC-LC-*`、`NPC-DEF-*`、`NPC-AI-001..023`、`NPC-TGT-*`,
`NPC-HOME-*`、`NPC-SEG-*`、`NPC-LOOT`（including `catchItem`）、`NPC-BUF-*`。

- [ ] 先完成 active/timeLeft/death/despawn/slot accounting 的 transition 方向。
- [ ] 再完成 deterministic target validity/tie-break 和 transform/collision input。
- [ ] 再按 AI family 分批接入 typed state；不建立公共 `float[]` 大容器。
- [ ] home/town、segment、loot、Buff/DoT 各自有独立 owner；不把 world event fields 放回 NPC。
- [ ] 完整 Boss phase、invasion/event、town services/housing/dialogue、worm shared-life、
  CheckActive 全 integration 和 spawn scheduler 保持 deferred，直到逐族有证据。

### Task 5：闭合 Projectile 服务器核心

**Target modules:** `Projectile/Definitions/*`、`Projectile/Components/*`、
`Projectile/Systems/*`、`Projectile/ProjectileStore.cs`、generic transform/collider/combat
components。

**Field set:** `PROJ-LC-*`、`PROJ-MV-*`、`PROJ-CBT-*`。

- [ ] 先实现 spawn/owner/active/lifetime/position/velocity 的最小链。
- [ ] 再实现 tile collision/friendly/hostile/damage/penetration 的服务器规则。
- [ ] 再按 behavior family 拆 AI/local state、reflection、liquid 和 cooldown。
- [ ] `identity` 保持实体关联；network identity/update/replication fields 不在本 task。
- [ ] presentation and Protocol rows remain excluded even when a snapshot with similar values exists。

### Task 6：闭合 Item 定义、实例和世界物品

**Target modules:** `Items/Definitions/*`、`Items/Components/*`、`Items/Systems/*`、
`Items/Snapshots/*`、`Items/Compatibility/*`、`InventoryComponent.cs`、`WorldItemStore.cs`。

**Field set:** `ITM-001..ITM-140`、`ITM-P-001..008`、Player inventory rows。

- [ ] 先将 type/stack/maxStack/damage/use/place/equipment/recovery 分为 definition vs instance。
- [ ] 再建立 stack/favorited/prefix/variant/active/position/pickup/revision 的 instance state。
- [ ] 再按 inventory transfer/use/equipment/drop/pickup/placement 的命令边界接入。
- [ ] shop/prefix/shimmer/DD2/sentry/Angler 等未闭合 server domain 保持 deferred。
- [ ] tooltip/color/glow/sound/UI/animation 继续 excluded；不为字段覆盖创建空 owner。

### Task 7：闭合 WorldObjects 和 mechanism

**Target modules:** `WorldObjects/Chest/*`、`WorldObjects/Sign/*`、TileEntity definitions/state、
`DoorStateComponent`、`WireNetworkComponent`、`ActuatorStateComponent`、TrainingDummy owners。

**Field set:** Chest/Sign/TileEntity/Door/Wire/Actuator/TrainingDummy rows。

- [ ] 所有对象先有 identity、anchor、active/valid、revision、persistence classification。
- [ ] 再加入 contents/text/link/lock/owner/permission 等真正的 server value。
- [ ] 交互只经过 validated business command；request/response/message fields remain excluded。
- [ ] wire/liquid/door mutation 共用 deterministic commit，但不共用一个杂项状态对象。
- [ ] TrainingDummy 保持 bounded source contract；不推导整个 message family 已完成。

### Task 8：WorldGen 分段收敛

**Target modules:** `src/Terraria.Dome.Simulation/WorldGeneration/` 下的
`Components`、`Definitions`、`Systems`、typed commands 和 snapshots。

**Field set:** all `WGEN-*` fields/properties, with client/host exclusions preserved。

**执行顺序：**

1. [ ] 先冻结 `WorldGenerationRequest`、`WorldSeedComponent`、`WorldRuleSnapshotComponent`、
   bounds/profile、lifecycle 和 `GenerationCursorComponent`。
2. [ ] 再冻结 pass-scoped random state；记录 seed/version/stage/cursor，移除隐式 `Main.rand`
   读取。
3. [ ] 再处理 TileReadSnapshot、TileChangeCommand、LiquidChangeCommand、Structure command
   和唯一 commit 顺序。
4. [ ] 按 terrain -> cave -> biome -> ore -> tree -> structure -> liquid/frame 的 dependency
   顺序推进；每个 pass 的 source order 和 side effects 单独留记录。
5. [ ] 当前 next bounded direction 是 Pyramid wall-frame evaluation：复用 neighbor owner，
   先 immutable mask/value，再 random semantics，再 typed command/commit。
6. [ ] TileRunner traversal、`-1/-2` liquid side effects、Pyramid tunnel/features、aggregate
   ordering、WLD snapshot differential 和 global deletion gate 保持独立 deferred。

**Stop condition:** aggregate differential 仍为非零时，不得把任何局部 owner 解释为
`canRemoveLegacyWorldGen=false`；任何历史性相反表述均不构成当前删除许可。

### Task 9：Server host 与持久化边界

**Target modules:** `src/Terraria.Dome.Server/Startup/`、`Import/`、`Persistence/`、
`src/Terraria.WorldFile.V319/Model/`、`src/Terraria.WorldCompatibility/`。

**Field set:** `M-OPS-*`、`M-HOST-*`、WLD metadata and persistence-needed Player/WorldObjects fields。

- [ ] 文件路径/句柄/备份/加载失败只在 host/adapter；导入输出 value-only request/snapshot。
- [ ] session identity、permission、liveness、timeout、disconnect cleanup 只在 host business state。
- [ ] `maxNetPlayers` 与 Simulation `maxPlayers` 分开；前者不进入 world entity capacity。
- [ ] save/recovery state 不混入 WorldClock/WorldGen/Entity components。
- [ ] 不把任何 host state 转写成 packet/wire field，Protocol 继续 excluded。

### Task 10：Deferred ledger 和物理删除门

**Target evidence:** `docs/migrations/version4-physical-deletion-ledger.csv`、
`docs/migrations/version4-physical-deletion-ledger.md`、WorldGen deletion metadata、各领域
field-card records。

- [ ] 44 条 `ServerRelevant` 逐行补 replacement evidence，未补齐的继续 `deferred`。
- [ ] 4 条 `ReplacedWithEvidence` 不自动扩大到相邻字段；逐行核对 owner/command/snapshot。
- [ ] ClientOnly/Protocol rows 不因没有 Simulation owner而被计为 server migration failure。
- [ ] `canRemoveLegacyWorldGen` 保持 false，直到 tile、extended state、metadata、command
  sequence、random checkpoint 和 restart/persistence boundary 都闭合。
- [ ] 只有所有 server-required 字段的默认值、写入链、生命周期和持久化边界具备证据后，才
  重新评估旧字段/文件的物理删除；本计划本身不执行删除。

## 16. 完成判定与反误报清单

### 16.1 单字段完成条件

一个字段只有在以下项目全部满足时，才能从 `deferred/partial` 提升为 `accepted-narrow`
或 `complete`：

- [ ] source anchor 和版本/hash 已冻结。
- [ ] 类型形状、static/instance 语义和默认值已明确。
- [ ] 唯一 owner 已确定，且没有第二个可写 owner。
- [ ] 所有服务器读取者和写入者已列出；旧 `Main`/旧实体对象不再是隐式读写源。
- [ ] 生命周期（bootstrap/tick/entity/world/restart/host）已明确。
- [ ] invalid/default/reset/rollback/duplicate/destroy 分支已明确。
- [ ] 需要持久化的值有 versioned value-only representation；派生/临时值明确不保存。
- [ ] 若属于 Definition，已与实例 state 分离；若属于 Component，未把共享规则复制进去。
- [ ] 若属于 command，写集和 commit 顺序明确；没有系统直接修改其他系统容器。
- [ ] 明确确认它不是 Protocol、transport、client projection、render/UI/audio/device 字段。

### 16.2 整个方向不得出现的误报

- [ ] 不把 `accepted-narrow=126` 写成 Main 已迁移 `126/696` 或全量百分比。
- [ ] 不把 weighted core score `92` 写成字段迁移率或 Terraria parity。
- [ ] 不把有 `Definition` 类、同名 property、注释或初始 mapping 写成行为完成。
- [ ] 不把 current WorldGen pass 的局部命令正确写成 aggregate WorldGen parity。
- [ ] 不把 current inventory 的 40 槽、persistence 的 990 槽写成旧 58 槽 parity。
- [ ] 不把 `worldID`、entity handle、Chest identity、Player profile key 因身份统计规则而删除。
- [ ] 不把 `Protocol`/replication/transport 字段放入组件表，也不把 Protocol 证据计入完成条件。
- [ ] 不把 client-only 字段由于位于旧 `Main`/`Player`/`NPC`/`Item` 就强行迁入服务器。
- [ ] 不把 `deferred` 写成“未发现”；deferred 必须有明确 owner 候选和未闭合原因。
- [ ] 不把 `not-run` 写成失败或通过；本轮没有执行验证。

## 17. 本轮交付边界：只写 Markdown，不测试

本轮只交付本文件中的范围冻结、当前进度、痛点分析、组件契约和逐项核对清单：

- 不编写 C#。
- 不新增或扩展函数、方法、事件处理器、system implementation 或 command implementation。
- 不修改 `.csproj`、`Directory.Build.*`、CSV、JSON、`progress.md`、Flowstate manifest 或
  现有 migration source。
- 不执行 test、build、verifier、regression、docs verifier 或差分 gate。
- 不触碰共享 `Build/bin`、`Build/obj`、`Build/diagnostics` 的生成输出。
- 所有新清单行在本轮的执行状态保留为现有证据状态或 `not-run`；本文件不制造新的通过
  证据，不关闭 WorldGen/deletion gate。

后续若要按本计划实现，必须另开实现批次，并在实现批次开始时重新确认范围；尤其不能因为
后续需要输出客户端状态，就把本文件排除的 `Protocol` 字段重新纳入。

## 18. 参考入口

- 当前状态卡：`progress.md`
- Flowstate 入口：`docs/flowstate/README.md`
- 当前主计划：`docs/flowstate/plan/2026-08-22-server-ecs-convergence.md`
- 当前任务图：`docs/flowstate/task/2026-08-22-server-ecs-convergence.md`
- Main 字段矩阵：`docs/migrations/main-field-property-ecs-migration.md`
- Main server responsibility：`docs/migrations/main-server-responsibility-ledger.md`
- Player 行为地图：`docs/migrations/player-legacy-behavior-map.md`
- Player 字段研究：`docs/research/2026-08-28-player-field-property-migration.md`
- NPC 字段矩阵：`docs/migrations/npc-field-property-ecs-migration.md`
- Item 字段报告：`docs/migrations/item-field-property-ecs-migration.md`
- Item 逐成员映射：`docs/migrations/item-ecs-member-mapping.md`
- WorldGen 字段说明：`docs/migrations/worldgen-field-property-migration.md`
- WorldGen source inventory：`docs/worldgen/worldgen-source-inventory.json`
- WorldGen parity：`docs/worldgen/worldgen-parity-report.md`
- Physical deletion ledger：`docs/migrations/version4-physical-deletion-ledger.csv`

## 19. 可并行子任务拆分与汇合顺序

本节把前面的领域清单拆成可以独立交付的子任务。13 份独立工作稿已实际创建在
`docs/plans/parallel/`；工作稿的存在只表示拆分已落地，不表示对应核对任务已经开始，
也不改变本文件的 `partial` 状态。为了避免多个执行者同时改同一个 Markdown 造成
冲突，每个并行子任务都必须先写自己的独立工作稿；只有最后的 `P99` 汇合任务可以把
结果合并回本文件。

### 19.1 并行边界和统一交付规则

所有子任务都必须遵守以下统一规则：

- 本轮子任务是字段、属性、组件、生命周期和边界的 Markdown 规划/审计，不编写 C#，不
  新增函数、System、Command implementation 或 DTO。
- 每个字段必须单独保留 `ID`、旧名称、source anchor、source hash、exact type、
  static/instance、default、mutability、lifetime、server meaning、唯一 owner、
  persistence、exclusion、status 和 next action。
- 一个并行子任务只允许写自己的工作稿路径；禁止直接修改本文件、`progress.md`、
  `docs/flowstate/**`、CSV、JSON、`.csproj`、`Directory.Build.*` 或其他子任务的工作稿。
- 工作稿中的新增核对项统一使用 `[ ]`。已有证据可以保留 `accepted-narrow`、`partial`、
  `deferred`、`host-only`、`excluded`、`legacy-reference` 或 `not-run`，但不能把工作稿
  的存在解释为行为完成。
- `Protocol`、transport、replication、wire layout、packet cursor、payload length、
  message ID、同步节流和客户端投影不进入任何 Simulation owner。需要记录时只能作为
  `excluded` 反向核对项出现。
- 业务身份不因 Protocol 排除而删除：`WorldId`、Player handle、NPC entity handle、
  Projectile entity handle、Item instance handle、Chest identity 和内部业务 revision
  仍须保留为运行时/持久化语义。
- 未来若子任务扩展到 C#，每个子任务必须使用独立 worktree 和独立 `Build/bin`、
  `Build/obj`；本节本身不启动 dotnet，因此本轮没有构建临界区。
- 本轮不执行 test、build、verifier、regression、差分 gate 或文档 gate。子任务输出只能
  引用既有证据；没有新验证时状态写 `not-run`。

统一的字段卡格式如下。各领域可以增加领域专用列，但不得删除这些基础列：

| ID | 旧声明 | Source anchor/hash | Exact type | Static/instance | Default | Mutability | Lifetime | Server meaning | Owner | Persistence | Exclusion | Status | Next action |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `SUB-XXX` | 一个旧字段或属性 | 文件与行号、源 hash | 完整 C# 类型 | static/instance | 声明默认值和语义 reset 值 | readonly/derived/mutable | bootstrap/tick/entity/world/restart/host | 权威业务含义 | 唯一组件/Definition/query/host owner | 保存/派生/临时/host | Protocol/client/UI 等排除理由 | 本计划状态词 | 下一条核对动作 |

### 19.2 波次依赖图

`P00` 先冻结范围和字段卡格式。完成后，第一波的各领域任务互不写入同一文件，可以并行
执行。第一波完成后，第二波的持久化和跨域 seam 任务也可以并行执行；它们都只读取第一波
产物，不修改第一波产物。最后由 `P99` 串行合并并做重复项、Protocol 回流和状态口径复核。

```text
P00 范围/字段卡/Protocol 硬边界
  |
  +--> P01 World/Main       ----+
  +--> P02 Player           -----|
  +--> P03 NPC              -----|
  +--> P04 Projectile       -----|
  +--> P05 Item             -----|----> P10 持久化/身份汇合 ----+
  +--> P06 WorldObjects     -----|                              |
  +--> P07 WorldGen         -----|----> P11 seam/command 汇合 ---+--> P99 最终合并
  +--> P08 Server host      -----|                              |
  +--> P09 Protocol audit   ----+-------------------------------+
```

`P01..P09` 的并行前提是“读集可以重叠，写集不能重叠”。如果执行环境不能提供独立工作稿
或 worktree，则保持同样的波次顺序，但由一个执行者串行完成，不得多人同时编辑总计划。

### 19.3 子任务总表

| ID | 子任务 | 领域/字段范围 | 组件设计输出 | 独立工作稿（已创建） | 依赖 | 可并行波次 | 本轮状态 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `P00` | 范围、字段卡和状态词冻结 | 全部领域；特别是 Protocol、身份、deferred 口径 | 统一字段卡、排除规则、状态词和证据入口 | [P00 工作稿](parallel/2026-08-31-P00-scope-and-boundary.md) | 无 | 0 | `[ ] not-run` |
| `P01` | World/Main 声明补录 | `M-*`、遗漏 Main property、静态 Tile registry、模式/天气/侵袭查询 | WorldMetadata、WorldRuleState、WorldClock、WorldWeatherState、WorldProgressionState、定义 registry | [P01 工作稿](parallel/2026-08-31-P01-world-main.md) | P00 | 1 | `[ ] not-run` |
| `P02` | Player 声明拆分 | Player 组合行、专用机制、库存/装备/Buff/环境/死亡槽位 | Player identity/authority/lifecycle/transform/movement/inventory/equipment/buff/interaction/death-drop/mount seams | [P02 工作稿](parallel/2026-08-31-P02-player.md) | P00 | 1 | `[ ] not-run` |
| `P03` | NPC 声明和 AI family | NPC 遗漏字段/属性、158 个 AI marker、spawn/home/event/loot | NpcDefinition、NpcLifecycle、NpcTarget、NpcHome、NpcBehaviorFamily、NpcEvent/Loot state | [P03 工作稿](parallel/2026-08-31-P03-npc.md) | P00 | 1 | `[ ] not-run` |
| `P04` | Projectile 声明拆分 | `maxAI`、`ai`/`localAI` slots、行为/生命周期；网络字段只做排除 | ProjectileDefinition、Transform、Owner、Collision、Combat、BehaviorFamily、Lifetime、Cooldown | [P04 工作稿](parallel/2026-08-31-P04-projectile.md) | P00 | 1 | `[ ] not-run` |
| `P05` | Item declaration closure | `ITM-001..ITM-140`、`ITM-P-001..008` 每一行的声明级元数据 | ItemDefinition、ItemInstance、Equipment、Use、Recovery、Placement、WorldItem、Economy | [P05 工作稿](parallel/2026-08-31-P05-item.md) | P00 | 1 | `[ ] not-run` |
| `P06` | WorldObjects 字段库存 | Chest、Sign、TileEntity、Door、Wire、Actuator、TrainingDummy | identity/anchor/lifecycle/contents/text/link/lock/permission/revision 及 typed commands | [P06 工作稿](parallel/2026-08-31-P06-world-objects.md) | P00 | 1 | `[ ] not-run` |
| `P07` | WorldGen 声明和分类 | inventory fields/properties、scratch、host/client 分类、Pyramid bounded slice | rule/seed/profile/pass state、random checkpoint、Tile/Liquid/Structure command seams | [P07 工作稿](parallel/2026-08-31-P07-worldgen.md) | P00 | 1 | `[ ] not-run` |
| `P08` | Server host 边界 | `M-OPS-*`、`M-HOST-*`、session/load/save/recovery/capacity | ServerSessionState、WorldBootstrap、WorldSaveCoordinator、ServerLaunchOptions | [P08 工作稿](parallel/2026-08-31-P08-server-host.md) | P00 | 1 | `[ ] not-run` |
| `P09` | Protocol 反向审计 | Protocol/transport/replication/wire 相关声明全量排除 | 只输出 `excluded` 反向清单，不产生 Simulation owner | [P09 工作稿](parallel/2026-08-31-P09-protocol-exclusion.md) | P00 | 1 | `[x] completed_partial` |
| `P10` | 身份、持久化和删除门汇合 | 第一波所有 server-required rows、WLD metadata、44 ServerRelevant rows | value-only snapshots、identity/revision boundary、restore/overflow/rollback matrix | [P10 工作稿](parallel/2026-08-31-P10-persistence-identity.md) | P01..P09 | 2 | `[x] completed_partial` |
| `P11` | Component/System/Command seam 汇合 | 第一波所有 owner、读集、写集、commit 顺序 | 组件契约、typed command、snapshot、唯一写入者和跨域依赖图 | [P11 工作稿](parallel/2026-08-31-P11-seams-and-commit.md) | P01..P09 | 2 | `[x] completed_partial` |
| `P99` | 总计划合并和遗漏复核 | 所有工作稿、现有第 7-18 节和引用入口 | 合并后的总计划、重复/遗漏/Protocol 回流报告 | [P99 收口任务](parallel/2026-08-31-P99-merge-and-omission-review.md)；写集仅为本文件 | P09..P11 | 3 | `[x] completed_partial` |

### 19.4 P00：范围、字段卡和 Protocol 硬边界

**目标：** 在所有领域开始前，冻结“什么算服务器必要字段”和“什么绝不能迁移”，避免
各领域分别解释导致重复建模。

**读取范围：** `progress.md`、`docs/flowstate/README.md`、当前 active plan/task、本文
第 0-5 节、Main responsibility ledger、physical deletion ledger 的分类词汇。

**必须产出：**

- 每个子任务都采用相同字段卡列，不能用只有 `旧字段 -> 新组件` 的短 mapping 代替。
- 明确 `server-authoritative`、`host-only`、`derived`、`legacy-reference`、`excluded`、
  `deferred` 和 `not-run` 的区别。
- 明确身份字段保留规则，尤其是 `worldID`、entity handle、Chest identity 和业务
  revision 不属于可删除的 Protocol 字段。
- 为 P09 提供 Protocol 排除词表和 false-positive 处理：看到 `whoAmI`、`owner`、
  `identity`、`revision` 时必须再判断它是业务身份还是 wire 编码，不能按名称自动排除。

**禁止：** 不为 `ProtocolComponent`、`ReplicationStateComponent`、packet DTO 或任何
网络 cursor 设计替代 owner；不改领域清单。

### 19.5 P01：World/Main 声明补录和组件设计

**目标：** 把 Main 的遗漏声明拆成可审计的 World owner，同时把静态表、派生属性、transient
display projection 分开。

**必须逐项覆盖的遗漏声明：**

- `offLimitBorderTiles`：`public static readonly int`、默认 `40`、World boundary policy。
- `dungeonX`、`dungeonY`：`public static int`、声明默认 `0`，并单独记录运行时 `-1/-1`
  unset 语义；不得与 Dungeon bounds 合并。
- `tileTable`、`tileBlockLight`、`tileNoSunLight`、`tileSpelunker`、`tileLargeFrames`、
  `tileNoFail`：按 `TileID.Count` 的 exact array type、全 false/0 默认、显式 true/false
  覆盖、registry owner 和越界 fail-closed 逐表核对。
- `invasionProgressIcon`：`static int` transient/display projection；不能以图标字段
  作为 invasion authority。若要保留权威 wave/score，只能另列 typed WorldProgressionState。
- `invasionProgressWave`：如果只作为显示投影，标 `excluded`；任何 gameplay wave 必须与
  Protocol packet wave 独立核对，不能把 packet 字段迁入。
- `GameMode`：`static int` property，含 null metadata 时返回 `0`、setter validity gate；
  `IsJourneyMode`、`NoFunctionalSurface`、`masterMode`、`expertMode` 和
  `IsRainingForever` 必须分别作为 derived property 行，而不是合并为一个 `gameMode` 行。

**组件边界：**

- `WorldMetadata` 只拥有 world identity、dimensions、bounds、spawn、seed/version metadata；
  不拥有 client view 或 Protocol projection。
- `WorldRuleState`/`WorldRuleQuery` 拥有 GameMode、difficulty、secret seed 和 derived mode
  queries；derived 值不得产生第二个可写缓存。
- 每个 Tile static table 使用独立 immutable `Definition/Registry`；不要把所有 bool/byte
  表合成一个无语义的 `TileData`。
- `WorldProgressionState` 拥有 invasion/boss/event authority；icon、message、packet wave
  等 projection 留在排除项。

**完成条件：** 每个上述声明均有 source anchor/hash、默认值、owner、持久化/派生判定和
`accepted-narrow`/`partial`/`excluded` 状态；不以 registry 集合证据宣称完整 WorldGen
或 Main parity。

### 19.6 P02：Player 声明拆分和三套布局矩阵

**目标：** 将当前 Player 组合行拆成 declaration-level 核对，尤其处理旧数组、runtime
container 和 persistence layout 的不等价。

**必须逐项覆盖的字段族：**

- 移动/专用能力：`mount`、`wings`、`wingsLogic`、`wingTime`、`wingTimeMax`、`grappling`、
  `grapCount`、`fallStart`、`fallStart2`、`meleeScaleGlove`。
- Fishing/Golf：`fishingSkill`、`golferScoreAccumulated`；若只用于结果显示则记录
  `excluded`，若影响规则则进入专用 domain，不能塞回通用 Player state。
- Luck/dodge/shield：`luck`、`luckMinimumCap`、`luckMaximumCap`、`luckPotion`、
  `oldLuckPotion`、`shadowDodge`、`shadowDodgeCount`、`shadowDodgeTimer`、`shield`、
  `defendedByPaladin`、`hasPaladinShield`、`shieldRaised`、`shieldParryTimeLeft`。
- 生命周期/使用：`respawnTimer`、`attackCD`、`itemAnimation`、`itemAnimationMax`、
  `itemTime`、`itemTimeMax`、`toolTime`；每个计时器都要有 reset、暂停、死亡和重连边界。
- 环境：`breathCD`、`breath`、`breathMax`、`lavaCD`、`lavaMax`、`lavaTime`、
  `adjWaterSource`、`adjHoney`、`adjLava`；接触 derived state 与可恢复资源必须分开。
- Inventory/equipment：`inventory`、`inventoryChestStack`、`armor`、`dye`、`miscEquips`、
  `miscDyes`、`trashItem`、`bank`、`bank2`、`bank3`、`bank4`、`voidVaultInfo`、
  `maxBuffs`、`buffType`、`buffTime`、`buffImmune`。
- Spawn/death/drop：`SpawnX`、`SpawnY`、`spawnX`/`spawnY` 兼容命名、`dead`、`deadTime`、
  `respawnTimer`、death cause、drop policy、once-only drop marker 和 pending world-item
  intent 必须在同一张 transition matrix 中对照；不能只留两个出生坐标。

**必须单独交付的布局矩阵：**

| 布局 | 精确边界 | 处理方式 | 禁止的误判 |
| --- | --- | --- | --- |
| Legacy inventory | source `inventory` 声明为 `Item[59]`；逻辑分区另核对 50 item + 4 coin + 4 ammo 的 58-slot contract | 记录物理数组长度、逻辑 slot range、空槽和保留索引的差异 | 不能用 `Item[59]` 自动宣称旧 58-slot parity |
| Runtime inventory | 当前 `InventoryComponent` 为 40 slots | 记录 runtime owner、selected slot、transfer/merge/revision | 不能新增一个旧 58-slot alias |
| Persistence inventory | `PlayerPersistentState.ItemSlotCount=990` | 记录 value-only restore、版本、overflow、duplicate/empty policy | 不能把 990 槽反向当作 runtime layout |
| Equipment/loadout | `armor[20]`、`dye[10]`、`miscEquips[5]`、`miscDyes[5]` | 记录 functional/vanity、slot range、stat calculation order | 不能用表现数组覆盖功能装备状态 |
| Bank/Void/Trash | 四个 `Chest` bank、`voidVaultInfo`、独立 `trashItem` | 各自 owner、持久化、容量和失效恢复 | 不能合并成一个 generic inventory |
| Buff | `maxBuffs=44`、`buffType[44]`、`buffTime[44]`、`buffImmune[BuffID.Count]` | 容量、排序、免疫表和 effect lifetime 分开 | 不能把 buff 数组变为 Protocol slot 或无界列表 |

**组件边界：** `PlayerInventoryComponent` 只拥有 runtime item instances；
`PlayerPersistentInventoryState` 只表达存档值；`EquipmentLoadoutComponent`、
`BuffCollectionComponent`、`PlayerEnvironmentContactComponent`、`PlayerMountStateComponent`
和 `PlayerDeathDropStateComponent` 分开，避免重新构造 Player God Object。

### 19.7 P03：NPC 遗漏声明和 AI family 矩阵

**目标：** 把 NPC 的静态容量、分类属性、世界查询和实体行为分离，禁止用 `float[] ai` 或
一个巨大 `NpcBehaviorStateComponent` 覆盖 158 个 AI marker。

**必须逐项覆盖的字段/属性：**

- `_givenName`、`CommonMasterBossLifeReduction`、`KickOutLookForHomeTimeout`、`maxAI`、
  `maxBuffs`、`nameOverDistance`、`nameOverIncrement`、`NPC_TARGETS_START`、
  `RevengeManager`、`SPAWN_SLOT_PROTECTION_TIME`。
- `CountsAsACritter`、`DownedAnyPreHardmodeBoss`、`isLikeATownNPC`、`IsMechQueenUp`、
  `ShieldStrengthTowerMax`、`TooWindyForButterflies`、
  `TreatedAsABossForRainbowBoulders`、`WhoAmIToTargetingIndex`。
- `WhoAmIToTargetingIndex` 必须拆为“业务 target handle derived value”和“wire target index”
  两行：前者可保留，后者 `excluded`，不能因为名称相同而复制网络编码。

**AI family 表必须至少包含：**

| AI family | Typed state | Default | Phase | Random state | Transition | Terminal condition | Owner | Persistence | Deferred reason |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 一个 `AI_###` 家族 | 独立 typed fields/resource | source default | spawn/tick/death | stream/seed owner | command/commit | despawn/phase end | 唯一 behavior system | 保存/重建/临时 | 未闭合的 consumer 或顺序 |

P03 需要将所有 marker 归入 family，并为每个 family 列 reader、writer、随机调用顺序、
target/home/event/loot 依赖。只登记 marker 名称而不写 family 行，不能算完成。

**组件边界：** `NpcDefinition` 放不变分类与难度规则；`NpcLifecycleComponent` 放 active、
timeLeft、death/despawn；`NpcTargetComponent` 放业务 target handle；`NpcHomeComponent`
放居住/门/呼吸规则；`NpcBehaviorFamilyState` 按家族拆分；`WorldProgressionState` 或
`WorldEventResourceState` 承接 boss/event 查询，不把静态查询属性复制到每个 NPC。

### 19.8 P04：Projectile AI/localAI 声明拆分

**目标：** 只迁移实际服务 Projectile 行为的 typed state，不迁移复制调度字段。

**当前 source reconciliation：** 当前 checkout 的 oracle
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/Projectile.cs` 在 `:130` 声明
`public static int maxAI = 3`，在 `:132` 和 `:134` 声明 `float[] ai/localAI`。因此本计划
按当前 source hash `8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`
只建立 slot `0..2` 的核对行。任何旧笔记中的 `maxAI=4` 都必须标记为 stale source
reconciliation；在没有新的 source snapshot 前，不得凭空添加 slot `3`。

**必须逐项覆盖：**

- `maxAI`：static mutable capacity declaration；不是实体行为状态本身；记录默认 `3`、
  生命周期和 family decomposition 责任。
- `ai[0]`、`ai[1]`、`ai[2]`：每个 slot 单独列 behavior family、默认 `0f`、读者、写者、
  transition、terminal condition、persistence 和 `deferred` 原因。
- `localAI[0]`、`localAI[1]`、`localAI[2]`：按 source consumer 区分 server behavior
  state 与 audio/render/local scratch；不能因数组属于实例就全部进入 Simulation。
- `ai[3]`、`localAI[3]`：作为 negative inventory 行记录“当前 source 不存在”，状态为
  `excluded`/`legacy-reference`，防止旧统计口径把不存在的 slot 当迁移字段。

**明确排除：** `netUpdate`、`netUpdate2`、`netSpam`、`netSyncSkippedForPlayer`、
`NetworkUpdateReady`、`ProjectileReplicationSnapshot`、session cursor 和 replication
scheduling 都属于 Protocol/replication reference-only。本任务不得为它们创建
`ProjectileNetworkUpdateComponent` 或任何等价 owner，也不得将它们计入 server field
migration 完成度。

**组件边界：** `ProjectileBehaviorStateComponent` 只承载经过 family 归类的行为值；
`ProjectileLifecycleComponent`、`ProjectileCollisionComponent`、`ProjectileCombatComponent`
和 `ProjectileCooldownComponent` 各自拥有自己的写集。raw `float[]` 只能作为 legacy
reference，不能成为新的公共兼容组件。

### 19.9 P05：Item declaration-level closure

**目标：** Item 名称清单目前基本完整，本任务不重复发明新名称，而是逐一为现有
`ITM-001..ITM-140` 和 `ITM-P-001..008` 补齐声明级证据，使“有一行 mapping”不再被误读为
“有完整 owner”。

**每个 ITM 行必须补齐：** source anchor/hash、exact type、default、static/instance、
readonly/derived/mutable、definition/instance lifetime、server consumer、唯一 owner、
business persistence、excluded projection、状态和下一步。不得用 `ITM-001..140` 的范围
行替代 140 个单独核对项，也不得将 `ITM-P` 属性块当作已闭合行为。

**必须保留的行为分组：**

- `SetDefaults` 和 `ItemID.Sets`：immutable definition 与实例 state 分离；没有真实
  consumer 的表保持 `deferred`，不创建空 registry。
- prefix eligibility/effect、equipment calculation、loadout stat order：分别列定义、
  实例 prefix 和计算系统的读写顺序。
- shop/catalog/discount/price、shimmer、DD2、sentry、Angler、fishing、crafting/Recipe：
  保留 server domain 候选和未闭合原因；不能用客户端 catalog 或 Protocol projection 代替。
- `noWet`、Potion/Flask effect、item placement、world item pickup/merge/owner/revision：
  按 command、commit、生命周期和恢复行为拆开。
- tooltip/color/glow/sound/UI/animation/appearance-only 字段：逐项写 `excluded`，不创建
  Simulation owner。

**组件边界：** 采用
`Definition -> Component -> System -> Snapshot -> business persistence`。末端明确写
`business persistence`，不写 Protocol projection；库存 slot、Item instance handle、
  stack quantity、prefix、active、world position 和 owner reservation 不能混为一个字段。

### 19.10 P06：WorldObjects 完整字段库存

**目标：** 将当前对象级 bullet checklist 补成 declaration-level inventory；业务 identity、
anchor、contents、text、permission、revision 可以保留，但 request/response/message layout
必须排除。

**Chest 必须逐项覆盖：** `identity`、`index/handle`、`x`、`y`、`active`、`valid`、
`maxItems`、`contents`、content slot index、`owner`、`lock`、open state、`revision`、
`name`、empty slot policy、duplicate policy、overflow policy、restore、destroy 和 stale
link behavior。来源至少锚定 `Chest.cs:61-75` 的 `maxItems`、`item`、`x`、`y`、`index`、
`bankChest`、`name`，并区分 `frameCounter/frame` 的 presentation 排除。

**Sign 必须逐项覆盖：** `identity`、`x`、`y`、`active/deleted`、`text`、text default、
text length、edit authority、revision、persistence、reload/delete behavior。来源至少锚定
`Sign.cs:3-11`；`text` 是业务内容，不能改写成 message payload。

**TileEntity 必须逐项覆盖：** entity type、identity/ID、`Point16` anchor、active/valid、
linked entity、owner、lock、persistent value schema、revision、placement、destroy、restore
和 cross-entity link invalidation。来源至少锚定 `TileEntity.cs:11-37` 的 manager、ID、Position、
type、RequiresUpdates；manager/update list 不能直接复制成一个全局 ECS singleton。

**Door/Wire/Actuator 必须逐项覆盖：** door anchor、open/closed、locked、permission、
mechanism authority、mutation revision；wire topology、anchor、enabled/disabled、wire
state、actuator state、bounded traversal、retry policy、mutation sequence、commit ordering
和 rollback。来源锚定 `Wiring.cs:19-69` 的 legacy scratch 以及 `Wiring.cs:405-435`、
`:871-1040`、`:1490-1530` 的 actuator/door/traversal consumers；这些 scratch queue 不是
目标 owner。

**TrainingDummy 必须逐项覆盖：** identity、anchor、active、linked NPC/entity、owner、lock、
hitbox、damage state、activation/deactivation、revision、persistence 和 stale link cleanup。
来源至少锚定 `TETrainingDummy.cs:11-25`、`:35-76`；`npc=-1` 是业务 link sentinel，
而 message 87 的字段布局、request/response 和 packet shape 全部 `excluded`。

**组件边界：** `ChestInventoryComponent`、`ChestAccessComponent`、`SignComponent`、
`TileEntityStateComponent`、`DoorStateComponent`、`WireNetworkComponent`、
`ActuatorStateComponent` 和 `TrainingDummyStateComponent` 分开；交互只输出 validated
business command，统一 deterministic commit 但不共用一个杂项状态对象。

### 19.11 P07：WorldGen 声明、scratch 分类和当前 bounded direction

**目标：** 对 WorldGen inventory 做 field/property/constant/derived/scratch/host/client
分层，避免把源文件规模或方法数量直接当作迁移字段完成度。

**计数口径必须单列：** `method`、`field`、`property`、`property block`、`constant`、
`derived value`、`scratch state`、`host state`、`client-only state` 不得混加。当前
`worldgen-source-inventory.json` 的 `684 methods/233 fields` 与旧报告的 `230 fields + 46
property blocks` 是不同口径，P07 必须保留两者来源并说明不能相加。

**必须逐项分类：** `Manifest`、`mysticLogsEvent`、`_generator`、`BackgroundsCache`、
`_SpawnThunderStorm_SafeSpots`、`drunkWorldGenText`、`_coatingColors`、`TreeTops`。每行
都要有 `WorldGen.cs` source anchor/hash、exact type、default、lifetime、consumer、owner、
persistence 和 exclusion reason。当前建议为 `Manifest` compatibility/host metadata、
`mysticLogsEvent` logging/audio excluded、`_generator` pipeline reference excluded、
`BackgroundsCache`/`_coatingColors`/渲染型 `TreeTops` excluded；安全点只有证明影响权威
天气/生成才可 deferred，不得默认进入 Simulation。

**组件边界：** `WorldGenerationRequest`、`WorldSeedComponent`、`WorldRuleSnapshotComponent`、
pass-scoped random state、`TileReadSnapshot`、`TileChangeCommand`、`LiquidChangeCommand`、
Structure command 和唯一 commit sequence 分开。Pyramid wall-frame evaluation 只作为
bounded slice，不能关闭 aggregate WorldGen deletion gate。

### 19.12 P08：Server host/session 边界

**目标：** 将服务器确实需要、但不属于 Simulation 的编排字段单独归档，避免“服务器使用”
被误读成“必须迁移为 ECS component”。

**必须逐项核对：** 文件 path/handle、backup/recovery、load failure、session identity、
bound Player handle、permission、liveness、timeout、disconnect cleanup、server mode、
capacity、save version 和 recovery state。`dedServ`、`netMode`、`_targetNetMode`、
`MaxTimeout`、`netPlayCounter`、`maxNetPlayers`、address/port、menu flags 等必须分别标为
`host-only` 或 `excluded`，并说明 `maxNetPlayers` 不等于 Simulation `maxPlayers`。

**组件边界：** `ServerSessionState`、`ServerCommandIngress`、`WorldBootstrap`、
`WorldSaveCoordinator`、`ServerLaunchOptions` 只拥有 host business state；导入层向
Simulation 输出 value-only request/snapshot；不把文件句柄、线程对象、闭包队列、raw
reader/writer 或任何 Protocol state 转写到 Simulation。

### 19.13 P09：Protocol 反向排除审计

**目标：** 与领域任务并行建立负向证据，防止某领域为了“字段完整”把网络字段再次纳入。

**必须逐项复核：** `src/Terraria.Dome.Protocol.V1456/**`、`src/Terraria.Dome.Server/Protocol/**`、
`src/Terraria.Dome.Transport/**` 的 DTO、codec state、message ID、packet kind、send/receive
mode、payload length、bit position/mask、byte cursor、reader/writer state、compression、
framing、replication cursor、sync cadence、`netUpdate`/`netSpam`/skipped-player arrays、
message 87 layout 和所有 `SyncNPC`/`SyncProjectile`/`SyncItem`/`SyncPlayer`/`SyncChestItem`
字段。

**身份误报处理：** `MessageBuffer.whoAmI` 的 wire representation excluded，但 Server
session identity 可以是 host-only；`Projectile.identity`、`NPC.netID`、Player handle、
Chest identity 和业务 revision 保留为业务身份，不能因为存在同名网络投影就删除。

**输出限制：** P09 只能写 excluded matrix 和误报说明，不能创建任何 Protocol owner、
replication component、network snapshot 或 codec function，也不能把排除项计入完成分母。

### 19.14 P10：持久化、身份和删除门汇合

**依赖：** 读取 P01-P09 的工作稿，等待每个领域明确哪些值必须跨 restart/reload 保存，
哪些值可 derived/rebuild，哪些值只在 host 存活。

**必须交付三套矩阵：**

1. `World`：WorldId、dimensions、spawn、mode/rule、progression、weather、Tile/Wall/Liquid、
   WorldObjects identity/content/revision 的 save/restore/invalid/unknown-version 行为。
2. `Player`：runtime inventory 40、legacy logical 58/source array 59、persistence 990、
   selected slot、armor/loadout、bank/bank2/bank3/bank4、Void Vault、trash、dye/misc、buff、
   restore collision、overflow、duplicate/empty slot policy。
3. 删除门：44 条 `ServerRelevant` 逐行关联 replacement owner/command/snapshot/evidence；
   未闭合项保持 `deferred`，`canRemoveLegacyWorldGen=false` 不变。

`P10` 只处理 business persistence。任何 persistence projection 若依赖 packet/wire layout，
必须拆出 value-only 存档语义和 Protocol `excluded` 语义，不能把存档与网络格式合并。

### 19.15 P11：Component/System/Command seam 汇合

**依赖：** 读取 P01-P09 的 owner 候选，确认同一字段只有一个可写 owner。

**每个域必须输出：**

- component 的输入不变量、默认值、生命周期和可写字段；
- system 的 read set、write set、tick/order 和跨域 query；
- command 的 source、authority、payload value-only 形状、目标实体/坐标、commit sequence、
  duplicate/retry/rollback 行为；
- snapshot 的持久化值、派生值和临时值边界；
- stale link、destroy、slot reuse、capacity overflow、invalid input 和 restart/recovery
  的处理责任。

**禁止的汇合形状：** `MainFieldsComponent`、`MainStateManager`、通用 `Data`/`Helper`/
`Manager` God Object、跨域直接写入其他容器、用 `float[]`/旧数组作为公共 owner、用
Protocol/replication snapshot 代替 business snapshot。

### 19.16 P99：总计划最终合并和遗漏复核

`P99` 是唯一允许修改本文件的执行单元，必须串行完成以下核对；完成合并也不等于迁移
完成：

- [x] 检查 P01-P08 的每个领域工作稿是否都包含字段名、属性名、source anchor/hash、
  exact type、default、mutability、lifetime、owner、persistence、exclusion、status 和
  next action。
- [x] 将 P02 的 mount/wings/grapple/fishing/golf/luck/shield、三套 inventory layout、
  buff arrays、spawn/death/drop 行逐项挂回 Player 清单。
- [x] 将 P03 的 NPC omitted declarations、properties 和 AI family matrix 逐项挂回 NPC
  清单；不以一个 AI 数组行替代多个 family。
- [x] 将 P04 的 `maxAI` 和当前 source 的 `ai/localAI` slot range 与旧报告 reconciliation；
  不添加当前 source 不存在的 slot，也不纳入 net scheduling。
- [x] 将 P05 的所有 `ITM-001..140`、`ITM-P-001..008` metadata closure 挂回 Item 清单；
  不以范围文字伪造逐项 coverage。
- [x] 将 P06 的 Chest/Sign/TileEntity/Door/Wire/Actuator/TrainingDummy 字段库存挂回
  WorldObjects；message/request/response/wire layout 保持 excluded。
- [x] 将 P07 的 WorldGen count reconciliation 和七类遗漏声明挂回 WorldGen；保留
  `ServerRelevant deferred=44` 与 `canRemoveLegacyWorldGen=false`。
- [x] 将 P08 的 host-only 列表与 `maxPlayers`/`maxNetPlayers` 边界挂回 Server host；不让
  host state 变成 Simulation 或 Protocol state。
- [x] 使用 P09 做一次全文 Protocol owner/字段回流扫描；任何误纳入项回退为 `excluded`，
  但不删除业务 identity。
- [x] 保留总体 `partial`、`not-run` 和所有 deferred/blocker；不把工作稿数量、字段行数、
  registry 数量或局部 owner 数量写成 parity、release-ready 或物理删除许可。

**P99 的写集仅为：** 本文件中新增/修订的并行执行章节和已被工作稿明确支持的字段卡。
不得顺手修改其他迁移文档、manifest、CSV、JSON、源码或 Build 输出。合并后仍需另开
实现批次；本节不授权执行任何测试、构建、verifier 或代码迁移。

### P99 执行证据（2026-09-01）

本次收口已读取 P00-P11 工作稿、本文第 0-19 节及
`docs/migrations/version4-physical-deletion-ledger.csv`，并只修改本文。核对结果如下：

- P00-P08 的字段卡仍保留 source anchor/hash、类型、默认值、可变性、生命周期、owner、
  persistence、exclusion、status 和 next action；P03/P04/P05 中明确未闭合的行为继续为
  `partial`/`deferred`，没有被工作稿数量提升为 parity。
- P09 的 source-anchor 反向审计已覆盖 Protocol V1456、Server Protocol、Transport、
  replication cursor/snapshot、MessageBuffer/NetMessage、同步帧及 B248 网络调度；全文
  结论固定为 `Protocol owner = none; migration state = excluded; not a completion gate`。
- P10 已提供 World/Player/Entity/WorldObjects 的 value-only persistence、identity、revision、
  stale-link、duplicate、overflow、unknown-version、restore/rollback 矩阵；ledger 的 44 条
  `ServerRelevant` 逐行仍为 `deferred`，没有用相邻字段或 registry 证据替代，
  `canRemoveLegacyWorldGen=false` 保持关闭。
- P11 已提供 10 条跨域 seam 的 read/write/validation/commit/retry/rollback/persistence
  矩阵，并拒绝重复 owner、隐式 static 写入、raw array alias、跨容器直接写入和 derived
  write-back；未闭合项保持 `partial`/`deferred`。
- 业务 `WorldId`、Player/entity handle、`NPC.netID`、`Projectile.identity`、Item instance
  handle、Chest identity/content 和 business revision 均保留；packet slot、cursor、wire
  revision、sync cadence、replication snapshot、`netUpdate`/`netSpam` 与 B248 scheduling
  不进入业务 owner 或 persistence。

**合并边界：** P09、P10、P11 状态为 `completed_partial`；P99 仅表示文档合并和遗漏复核
完成，不表示运行时迁移、测试、构建、verifier、release readiness 或物理删除完成。总体
状态仍为 `partial`，所有未闭合字段和删除门仍需后续授权批次处理。
