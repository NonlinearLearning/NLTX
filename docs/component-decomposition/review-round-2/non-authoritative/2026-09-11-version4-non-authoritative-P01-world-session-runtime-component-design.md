# Version4 非权威 P01 世界会话与运行时：proposed 组件设计

partitionId: P01
sessionId: 12ecaf3ffa2e43d490f5f18d19cda8d3
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\01-world-session-runtime.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P01-world-session-runtime-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P01-world-session-runtime-component-execution.md
designStatus: proposed
executionStatus: completed (src2-only implementation checkpoint)
implementationStatus: completed
verificationStatus: independently-verified (serial build, focused verifier, and 149-member coverage audit passed; integration and behavior-equivalence verification remain)
completedComponents: [FrameActivityStateComponent, RuntimeBootstrapAndWorldRuleStateComponent, FrameTimingAndSchedulingStateComponent, FrameControlAndTimeSkipStateComponent, WorldSeedAndSpecialRuleBoundary, SessionReadinessAndWorldCatalogBoundary, HostLoadAndShutdownBoundary, DerivedWorldSessionQueriesAndDifficultyDefinitions, ManEaterProtectionIndexComponent, WorldSessionPersistenceAndReplicationBoundary]
currentComponent: none (src2 implementation and focused verification complete)
pendingComponents: []
lastCheckpointUtc: 2026-09-12T07:32:20.4258455Z
evidence-gap: P01 专属第一轮 public-decomposition 输出文件不存在；输入库存只确认声明/计数，逐成员读者、写者、生命周期、网络/存档提交和宿主语义仍有 partial/unresolved；任务包中的 `Terraria\\WorldFile.cs` 与实际 `Terraria.IO\\WorldFile.cs` 不一致；`约束/公共拆分约束.md` 未在声明路径找到；src2 serial build、focused verifier 和 149-member coverage audit 已通过；`WorldSessionReplicationProjection` 目前仅提供 typed committed-snapshot pass-through，不闭合 WorldData packet 7 客户端恢复；`WorldPersistenceAdapter` 是内存边界，不是生产文件 I/O。
blocking-decision: crossSubsystemOwner: integration-review；需整合会话锁定 WorldSession 根实体、Hardmode 事务、FrameActivity 聚合、WorldData 客户端恢复、宿主/平台隔离、公共 ID/快照以及 Version4 时间与网络/持久化提交顺序。

> 本文件仍是第二轮非权威设计提案。边界、owner、网络/存档语义和跨分区调度仍为 `proposed`；本会话已在 `src2` 保存对应的非权威实现，且串行 build、focused verifier 和 149 条成员覆盖审计已通过。这不表示生产 `src` 已迁移、行为等价、网络闭合或生产持久化完成。

## 当前实现状态

实现模式已实际写入 `D:\TRbackup\NLTX\src2`。P01 源码清单为：`src2/WorldSession/**/*.cs`（119 个 C# 文件）、`src2/WorldSession/Terraria.WorldSession.csproj`、`src2/WorldInteraction/Components/ManEaterProtectionIndexComponent.cs`、`src2/WorldInteraction/Components/ManEaterProtectionSystem.cs`、`src2/WorldSessionVerification/Program.cs` 和 `src2/WorldSessionVerification/Terraria.WorldSessionVerification.csproj`。`src2/WorldInteraction/Tiles/**` 属于其他共享 checkout 改动，本会话未修改或纳入 P01 owner 声明。

已保存的实现覆盖九个原计划 checkpoint，并增加最小 typed persistence/replication boundary；serial build、focused verifier 和 149 条成员覆盖审计均已通过。当前只剩 runner settlement；`verificationStatus` 记录为 `independently-verified`，不扩大为生产接入或行为等价结论。

## 1. 范围与排除

本分区覆盖输入报告中 12 个叶子子系统、149 条成员记录（104 fields、45 properties）：

- `MainFrameActivityState`
- `MainBootstrapAndWorldRules`
- `MainClockAndFrameScheduling`
- `MainFrameAndWorldRuleControl`
- `MainRandomAndSeedState`
- `MainSaveAndWorldSessionState`
- `MainWindowAndShutdownState`
- `MainTimeSkipState`
- `MainDerivedWorldAndSessionQueries`
- `SharedDifficultyAndRuleMetadata`
- `WorldSeedAndExploitRules`
- `SharedStartupAndRuntimeHostState`

本文件不裁决其他 P 分区的成员、最终 owner、公共 ID、网络协议、存档格式或全局调度顺序。核心世界时钟字段、天气事实、完整世界描述和跨域进度若不在 P01 输入表中，只作为边界依赖提及，不复制其他分区成员清单。`crossSubsystemOwner: integration-review`。

不在本轮执行：生产 C#、csproj、测试、源生成、迁移、ledger、lock、第一轮报告或其他会话文档的修改；任何 compile-capable 命令；行为回放和行为等价验证。

## 2. 输入与证据角色

### 2.1 已读取材料

| 来源 | 事实用途 | 状态 |
| --- | --- | --- |
| `D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\01-world-session-runtime.md` | P01 唯一成员库存，runner 已校验 149/149 | confirmed |
| `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-11-version4-non-authoritative-20-partition-prompts\2026-09-11-version4-non-authoritative-P01-world-session-runtime-public-decomposition.md` | P01 范围、证据焦点、输出协议 | confirmed |
| `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-session-public-decomposition.md` | 同领域历史第一轮研究，补充调用顺序和边界线索；不是本次 P01 专属第一轮输出 | existing-evidence / historical |
| `D:\TRbackup\Version4\Terraria\Main.cs` | `Main` 声明、主循环、帧活动、时间跳转、规则查询、世界目录和清理 | confirmed / partial |
| `D:\TRbackup\Version4\Terraria\WorldGen.cs` | 随机源、生成屏障、Hardmode 转换和世界更新调用者 | confirmed / partial |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs`、`WorldFileData.cs` | 存档/加载、活动世界元数据、失败诊断和临时 staging | confirmed |
| `D:\TRbackup\Version4\Terraria\NetMessage.cs`、`MessageBuffer.cs` | 世界数据出站和客户端 case 7 边界 | confirmed / unresolved |
| `D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs`、`GameDifficultyLevel.cs` | 难度定义和曲线数据 | confirmed |
| `D:\TRbackup\Version4\Terraria.GameContent\FixExploitManEaters.cs`、`SpecialSeedFeatures.cs` | 瞬态漏洞保护状态和特殊种子纯派生资格 | confirmed |
| `D:\TRbackup\Version4\Terraria\InitData.cs`、`Program.cs`、`Ref.cs`、`WindowsLaunch.cs`、`Terraria.Server\Game.cs` | 宿主限制、启动参数、引用桥、Windows 退出信号和服务器框架属性 | confirmed as source facts; semantic owner partial |
| `D:\TRbackup\tmodloader-api-docs-stable\index.html`、`class_mod_system.html` | `v2026.07` 公开加载、时间、世界更新、存档和网络方向交叉核对 | existing-evidence |
| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\GameTicking\SharedGameTicker.cs`、`Content.Server\GameTicking\GameTicker.RoundFlow.cs` | 只参考 round/readiness 状态与服务端协调器粒度 | existing-evidence; 无 Terraria 直接语义 |
| `D:\TRbackup\NLTX\src\WorldSession`、`src\SimulationRuleOverrides` | 当前 NLTX 状态和重复类型/快照骨架 | partial; 不等于已实现 |

任务包给出的 `Terraria\WorldFile.cs` 路径与实际 Version4 路径不一致；实际路径是 `Terraria.IO\WorldFile.cs`，记录为 `evidence-mismatch`，不静默修正为任务包路径。

### 2.2 关键 Version4 事实

- `Main.CurrentFrameFlags` 在 `Main.cs:86-96` 声明；`DoUpdateInWorld` 在 `Main.cs:11448-11529` 聚合/重置其中的帧事实，NPC、Projectile 和 WorldGen 读取这些值（`Main.cs:3171`、`NPC.cs:161,66574`、`Projectile.cs:29053,33994,38199`、`WorldGen.cs:59549`）。这是 transient frame state，不是持久世界规则。
- `Main.UpdateWorldPreparationState` 在 `Main.cs:1763-1767` 将内部枚举设为 `Ready`；实体资格在 `Main.cs:11400-11408` 还依赖 `WorldGen.isGeneratingOrLoadingWorld`。加载失败、生成屏障和清理路径另由 `WorldFile.cs:672-807`、`WorldGen.cs:4151,4165` 参与，不能由单个 `Ready` 写入推导完整生命周期。
- `Main.DoUpdate` 在 `Main.cs:11212-11354` 先更新时间/宿主和天气阶段，再由 `DoUpdateInWorld` 在 `Main.cs:11411-11620` 运行实体、`UpdateTime`、WorldGen 和服务器更新；时间提交位置是行为顺序约束，不能由文件顺序或新 ECS 文件顺序替代。
- `Main.GameMode` 在 `Main.cs:1318-1332` 读写 `ActiveWorldFileData`；`Difficulty` 在 `Main.cs:1369-1390` 组合模式、覆盖和 `getGoodWorld`，因此 `GameMode`、难度曲线和有效覆写不能无条件合并为一个持久 Component。
- `WorldGen.StartHardmode` 在 `WorldGen.cs:26084-26125` 先写 Hardmode，再经后台转换、I/O 锁和主线程 follow-up；`Main.hardMode` 只是一项已提交事实，转换事务 owner 仍是 `integration-review`。
- `Program.LaunchParameters`、`SavePath`、`LoadedEverything` 和 JIT 指针在 `Program.cs:23-35,117-186` 由启动和反射/线程流程写入；平台句柄、参数字典、资源句柄和线程进度不得模拟成世界 Component。
- `GameDifficultyData.LinearCurve` 的 `Key.input/output`、`keys` 和八条曲线是静态只读定义，`GameDifficultyLevel` 的五个值是难度标尺；它们适合作为 Definition + 纯 Query，不是每个世界实体的可变 Component。
- `FixExploitManEaters.IndexesProtected` 在 `FixExploitManEaters.cs:7-32` 被 `Update` 清空、`ProtectSpot` 写入、`SpotProtected` 读取；它是按帧/空间资格的 transient index，不能并入世界规则状态。
- `SpecialSeedFeatures` 的 12 个属性在 `SpecialSeedFeatures.cs:5-118` 只读取 `Main`/`WorldGen.SecretSeed` 并计算 bool；它们是 Query，不是缓存权威字段。

## 3. 当前 NLTX 状态

当前根 `src/WorldSession` 已有 `WorldDescriptorState`、`WorldRulesState`、`WorldClockState`、`WorldWeatherState`、`SessionReadinessState` 和 `WorldTickSnapshot` 的材料，但这些文件没有被本轮当作已迁移证明。`WorldSessionComponents.cs` 还把 readiness、difficulty、天气、事件进度和刷怪压力聚合在一个文件中；`WorldSession\\WorldGeneration` 存在同名 `WorldDescriptorState`、`WorldRulesState`、`WorldPreparationState` 等类型，形成 namespace/owner 冲突风险。`WorldProgression` 已有 Hardmode 状态候选，`SimulationRuleOverrides` 已有规则覆写快照候选。

当前 NLTX 状态统一为 `partial`：尚未由本任务验证唯一写入 owner、根实体装配、网络/存档闭合、System 顺序或行为等价。`dome` 中的 clock-first schedule 只能作为结构线索，不能覆盖 Version4 的实体更新前后顺序。

## 4. proposed 边界总表

| ID | proposed 类型/角色 | 只保存或处理的概念 | 生命周期/作用域 | 不收纳 |
| --- | --- | --- | --- | --- |
| P01-C01 | `FrameActivityStateComponent` | 当前完整世界 Tick 的玩家、睡眠玩家、Boss、交互 Projectile 聚合事实 | transient；一个 WorldSession root；每个允许实体更新的 Tick 重置 | 世界进度、NPC/Projectile 本体状态 |
| P01-C02 | `RuntimeBootstrapAndWorldRuleStateComponent` + `RuntimeBootstrapAdapter` | 启动版本/公告配置和特殊种子规则事实 | 启动载入；规则状态随活动世界生效；宿主输入只读导入 | 外部资源、命令 payload、跨域难度覆写 |
| P01-C03 | `FrameTimingAndSchedulingStateComponent` + `DeferredProcessPort` | 帧暂停/诊断计数和延迟处理调度 seam | Host/Runtime root；每帧或进程生命周期 | `GameTime`、Asset、NPC/音频内容定义 |
| P01-C04 | `WorldTimeSkipStateComponent` + `FrameControlCommand` | 日晷/月晷 fast-forward intent 和 cooldown | 活动世界；命令提交后由单一 System 消费 | UI 输入、网络 wire DTO、完整时钟字段 |
| P01-C05 | `WorldRandomPort` + `SpecialSeedEligibilityQuery` | RNG 外部端口、默认/自动生成种子输入、特殊种子纯资格计算 | RNG 按会话/生成阶段；Query 无状态 | `UnifiedRandom`、派生 bool 的持久缓存 |
| P01-C06 | `SessionReadinessComponent` + `WorldCatalogAdapter` | session phase、失败码、活动世界/玩家文件选择和路径边界 | world session lifecycle；目录/文件 adapter 管理 I/O | `WorldFileData` 第三方对象、玩家实体、存档 staging |
| P01-C07 | `RuntimeLoadProgressComponent` + host/window/shutdown adapters | 启动加载进度、平台窗口/退出、Server.Game 服务端口 | Process/host lifetime；客户端/服务器隔离 | 平台句柄、服务容器、通用 `Ref<T>` |
| P01-C08 | `WorldSessionQuery`、`DifficultyRuleDefinition`、`DifficultyQuery` | 派生路径、规则资格、几何、玩家关系和静态难度曲线 | 纯 Query/immutable definition；不写状态 | 派生值缓存、网络/存档视图作为权威 |
| P01-C09 | `ManEaterProtectionIndexComponent` + `ManEaterProtectionSystem` | 当前保护坐标集合 | transient world/spatial scope；Update 清空，Protect 写入 | 长期世界规则、持久化字段 |

上述跨分区类型和 owner 仍是设计对象；对应的非权威实现文件已保存到 `src2`，但不表示已经接入生产。共享 ID、快照和值对象标记 `crossSubsystemOwner: integration-review`。

## 5. ID、作用域和副作用规则

- `WorldSession` root entity、`WorldEntityId`、`PersistentWorldId`、`WorldId`、`UniqueId`、`NetworkId`、`EntityId`、`NpcEntityId`、`WorldSectionId`、文件路径键和账户/客户端 UUID 必须分开。P01 不宣布任何跨分区 ID 的最终 owner，统一 `crossSubsystemOwner: integration-review`。
- Component 只保存稳定的内聚状态；`GameTime`、`IAssetRepository`、`GraphicsDevice`、`GameServiceContainer`、`WindowStateController`、`HandlerRoutine`、`IntPtr` 和 `Dictionary<string,string>` 通过 proposed Adapter/Port 隔离。
- Query 只读取显式快照或状态，不更新 Component；时间、随机、日志、文件、网络、UI、线程和平台电源效果由 System 外壳执行。
- 持久化先由 `WorldSessionPersistenceAdapter` proposed 生成 typed snapshot，再由 `WorldSessionCommitSystem` proposed 提交；失败不把 `_temp*` 或 `LoadException` 写回核心 Component。
- 网络出站由 `WorldSessionReplicationProjection` proposed 读取 committed snapshot；入站只产生显式 Command。包 7 客户端恢复仍是 unresolved，不能把 wire DTO 当作权威组件。

## 6. 调度契约候选

以下是 P01 的 proposed 调度契约，不是最终全局顺序：

```text
HostBootstrapAdapter -> SessionReadinessSystem
SessionReadinessSystem -> RuntimeBootstrapImportSystem
RuntimeBootstrapImportSystem -> FrameActivityResetSystem
FrameActivityResetSystem -> entity systems that contribute activity
entity activity accumulation -> FrameActivityCommitSystem
TimeSkipCommandSystem -> WorldTimeSkipStateSystem
FrameTimingAndDeferredProcessSystem -> host/runtime projections
WorldSessionCommitSystem -> PersistenceAdapter and ReplicationProjection
```

Version4 兼容约束候选：`Player -> NPC/spawn -> Projectile -> WorldItem -> LeashedEntity -> UpdateTime -> WorldGen/invasion -> UpdateServer` 仍需由整合会话确认；P01 只能提出候选，不能改变其他分区 System 的最终顺序。`WorldData` 出站不得早于 committed snapshot，`Ready` 不得绕过 generation/loading barrier。

## 7. 断点检查表

每个 checkpoint 追加前必须同时更新本文件和 execution 文件的同步字段。九个原计划 checkpoint 和 typed persistence/replication boundary 已保存到 `src2`；serial build、focused verifier 和 149 条成员覆盖审计已完成，但源码仍是非权威实现，跨分区 owner 裁决仍未完成。

## 8. Checkpoint 1：FrameActivityStateComponent（implemented in src2, proposed production owner）

| 输入序号 | Version4 成员 | 已保存的 src2 角色 | 生命周期和边界 |
| ---: | --- | --- | --- |
| 1 | `Main.CurrentFrameFlags.ActivePlayersCount` | `FrameActivityStateComponent.ActivePlayerCount` | `FrameActivityCommitSystem` 在 frame 开始 reset、接收贡献并 commit；不持久化 |
| 2 | `Main.CurrentFrameFlags.SleepingPlayersCount` | `FrameActivityStateComponent.SleepingPlayerCount` | 同一 transient frame aggregate；不进入世界规则快照 |
| 3 | `Main.CurrentFrameFlags.AnyActiveBossNPC` | `FrameActivityStateComponent.AnyActiveBoss` | NPC contribution 输入后由 commit system 写入；跨分区贡献 owner 仍待整合 |
| 4 | `Main.CurrentFrameFlags.HadAnActiveInteractableProjectile` | `FrameActivityStateComponent.HadActiveInteractableProjectile` | Projectile contribution 输入后由 commit system 写入；清理顺序需 integration-review |

`FrameActivityContribution`、`FrameActivitySnapshot`、`FrameActivityCommitSystem` 和 `FrameActivityQuery` 已保存于 `src2/WorldSession/Components` 与 `src2/WorldSession/Queries`。Focused verifier 已通过 reset/contribution/commit 检查；Version4 全局实体阶段顺序仍未裁决。

## 9. Checkpoint 2：RuntimeBootstrapAndWorldRuleStateComponent（proposed）

### 8.1 成员归属

| 输入序号 | Version4 成员 | proposed 归属 | 状态/生命周期 | 读者、写者和副作用 |
| ---: | --- | --- | --- | --- |
| 7 | `Main.mapDelay` | `RuntimeBootstrapAndWorldRuleStateComponent.MapDelayTicks`（proposed） | host/runtime pacing；启动初始化、运行时调整；非世界持久化事实 | `Main` 主循环/地图更新读取；主线程写入；不与世界规则合并 |
| 8 | `Main.Assets` | `RuntimeHostAdapter`（proposed port-owned reference） | 外部资源服务引用；进程级 | 资源加载/客户端读取；Adapter 负责生命周期和释放；不进入 Component |
| 9 | `Main.GameAskedToQuit` | `ShutdownRequestState`（proposed command/host state） | 进程退出意图；一次性/进程级 | 主循环读取，退出入口写；不持久化、不网络复制 |
| 18-19 | `Main.versionNumber`, `versionNumber2` | `RuntimeBuildIdentity`（proposed immutable definition） | 构建/协议显示元数据；进程级 | 启动、日志、服务器提示读取；构建输入/启动边界提供；不作为世界状态 |
| 20-21 | `Main.AnnouncementBoxDisabled`, `AnnouncementBoxRange` | `AnnouncementPolicy`（proposed runtime config value） | 启动参数导入后稳定；配置/表现输入 | `FindAnnouncementBoxStatus` 和公告 UI/网络边界读取；`Program.LaunchParameters` 写入来源；不由 ECS 查询改写 |
| 22 | `Main.AutogenSeedName` | `WorldGenerationRequest`（proposed command payload） | 自动生成请求期间短命；不作为 Component | CLI/启动命令写，WorldGeneration adapter 消费；不得写进长期规则状态 |
| 23-35 | `Main.drunkWorld`, `getGoodWorld`, `tenthAnniversaryWorld`, `dontStarveWorld`, `notTheBeesWorld`, `remixWorld`, `noTrapsWorld`, `zenithWorld`, `skyblockWorld`, `vampireSeed`, `infectedSeed`, `teamBasedSpawnsSeed`, `dualDungeonsSeed` | `RuntimeBootstrapAndWorldRuleStateComponent.SecretSeedFlags`（proposed） | 活动 WorldSession 的权威规则输入；创建/加载/清理；持久化/网络语义需整合 | `SpecialSeedFeatures`、NPC、WorldGen、NetMessage 读取；唯一写者候选为 world load/generation commit；清理由 session teardown；`crossSubsystemOwner: integration-review` |
| 36 | `Main._gameModeDifficultyOverride` | `RuleOverrideSnapshot` 只读输入（proposed，归属 `SimulationRuleOverrides` 候选） | 帧内 effective override；非基础规则持久化字段 | `UpdateCreativeGameModeOverride` 写，`DifficultyQuery` 读取；不得写回基础 `GameMode`；`crossSubsystemOwner: integration-review` |
| 37 | `Main.destroyerHB` | `DestroyerHeadMarker`（proposed adjacent combat/event boundary） | 活动 Boss/事件运行时坐标；瞬态表现/玩法输入 | Destroyer/NPC/渲染读取；P01 不拥有，不并入 bootstrap/rules；`crossSubsystemOwner: integration-review` |

### 8.2 设计判断

`MainBootstrapAndWorldRules` 是混合库存组，不能机械生成一个包含所有 23 个字段的组件。启动配置、构建标识、宿主资源、退出请求、一次性自动生成 seed 和世界秘密种子各自有不同生命周期；只有秘密种子 flag 集合具备活动 WorldSession 的持续状态候选。`_gameModeDifficultyOverride` 是 effective view 输入，`destroyerHB` 是相邻 Boss 运行态，均明确排除。

`SpecialSeedFeatures` 的 12 个属性只读取这些 flags 以及 `WorldGen.SecretSeed`，因此 proposed 目标是 `SpecialSeedEligibilityQuery`，不缓存 `ShouldDropExtraGel` 等 bool。Query 的输出可供生成、NPC、战利品等系统使用，但不得修改 `RuntimeBootstrapAndWorldRuleStateComponent`。

### 8.3 接口和副作用契约

```text
proposed IRuntimeBootstrapPort
  ReadBuildIdentity() -> immutable RuntimeBuildIdentity
  ReadAnnouncementPolicy() -> AnnouncementPolicy
  ReadLaunchSeed() -> WorldGenerationRequest

proposed RuntimeBootstrapImportSystem
  reads: IRuntimeBootstrapPort, validated launch input
  writes: proposed host/config state and WorldSession seed flags only at load/generation commit
  effects: logging/config diagnostics at outer adapter boundary

proposed SpecialSeedEligibilityQuery
  reads: immutable seed flags + explicit world-rule snapshot
  writes: none
  returns: derived eligibility result
```

Failure semantics: invalid launch parameter or seed parsing is an explicit rejected request; it must not silently produce a partially updated rule component. Any fallback to random seed or default difficulty is a boundary policy and remains `integration-review` until Version4 and compatibility tests close it.

## 9. Checkpoint 3：FrameTimingAndSchedulingStateComponent（proposed）

### 9.1 成员归属

| 输入序号 | Version4 成员 | proposed 归属 | 状态/生命周期 | 读者、写者和副作用 |
| ---: | --- | --- | --- | --- |
| 48 | `Main.clientUUID` | `ClientIdentityAdapter`（proposed external/client identity port） | 客户端/连接生命周期；不是世界状态 | 网络会话/日志读取；平台或连接边界写入；`crossSubsystemOwner: integration-review` |
| 49 | `Main.GlobalTimeWrappedHourly` | `FrameTimingAndSchedulingStateComponent.WrappedHour`（proposed cache） | 每帧由 `gameTime` 派生；清理时失效；非持久化 | `DoUpdate` 写，表现/第三方 hook 读取；必须由显式 clock input 计算，不能作为世界时钟 owner |
| 50 | `Main.GlobalTimerPaused` | `FrameTimingAndSchedulingStateComponent.GlobalTimerPaused`（proposed runtime control state） | 进程/帧调度；暂停/恢复 | host/clock systems 读取，控制入口写；不能暗示世界 `gamePaused` 或实体 Tick 自动暂停 |
| 51 | `Main.gameTimeCache` | `GameTimePort`（proposed host adapter） | 外部引擎时钟缓存；帧内短期 | 主循环/客户端表现读取；Adapter 提供不可变 frame time；不进入 Component |
| 52 | `Main.ScreenShaderRef` | `ScreenShaderAssetAdapter`（proposed client adapter） | 客户端资源句柄；加载/卸载 | 渲染读取，资源系统写/释放；不进入模拟 Component |
| 53-54 | `Main.DelayedProcesses`, `DelayedProcessesInGame` | `DeferredProcessSchedulerPort`（proposed effect port） | 协程队列；进程/世界生命周期 | `DoUpdate` 枚举并移除完成项；写者为提交延迟工作者；必须明确一次/重复枚举和取消，不持久化 |
| 55 | `Main.npcStreamSpeed` | `NpcStreamSchedulingPolicy`（proposed external/shared policy） | 服务端网络/实体流调度配置 | NPC/network scheduler 读取；配置写；跨 P01 共享候选 `integration-review` |
| 56-58 | `Main.dedServFPS`, `dedServCount1`, `dedServCount2` | `DedicatedServerFrameMetrics`（proposed diagnostics state） | 服务端帧诊断/节流；运行期缓存 | dedicated server loop 写/读；日志/metrics projection 读；不影响权威世界时钟 |
| 59-60 | `Main.offLimitBorderTiles`, `maxMusic` | `RuntimeLimitsDefinition` / `AudioCapacityAdapter`（proposed definitions/adapters） | 固定配置/客户端容量；进程级 | 空间边界、音乐系统读取；只读定义或客户端 adapter；不进入 WorldSession state |

### 9.2 设计判断

该 13 成员组混合了客户端身份、引擎时钟、暂停控制、延迟工作、服务端调度、诊断计数、空间常量和音频容量。唯一适合 Component 的是可观察的进程/帧控制或诊断状态；`GameTime`、资产、协程集合和 client UUID 必须留在 Adapter/Port 边界。`DelayedProcesses` 的枚举是副作用：它可能调用外部工作、移除队列项并受调用时机影响，不能由纯 Query 隐式处理。

### 9.3 接口和副作用契约

```text
proposed IFrameTimingPort
  ReadFrameTime() -> immutable frame-time value
  ReadGlobalPause() -> bool

proposed IDeferredProcessScheduler
  Enqueue(process, lifecycle)
  Drain(frameScope) -> explicit execution results

proposed FrameTimingSystem
  reads: IFrameTimingPort
  writes: frame timing cache and diagnostics counters
  effects: invokes deferred processes at declared phase; records cancellation/failure
```

`GlobalTimeWrappedHourly` 是从引擎时间派生的缓存，不可用于替代 WorldSession `WorldClockState`。`dedServCount1/2` 和 `updatesCountedForFPS` 只可作为诊断观察；日志/指标不得反向推进模拟。外部时间、协程和资源失败/取消/重复语义需在实现阶段用 verifier 闭合。

## 10. Checkpoint 4：FrameControlAndTimeSkipStateComponent（proposed）

### 10.1 成员归属

| 输入序号 | Version4 成员 | proposed 归属 | 状态/生命周期 | 读者、写者和副作用 |
| ---: | --- | --- | --- | --- |
| 165 | `Main.hardMode` | `WorldProgression` 候选的 `HardmodeSnapshot`；不由 P01 单独注册 | 活动世界持久事实；Hardmode 事务提交后稳定；跨域 owner 未决 | NPC、任务、生成、存档和网络读取；`WorldGen.StartHardmode` 候选写者；`crossSubsystemOwner: integration-review` |
| 166 | `Main.maxQ` | `FrameControlAndTimeSkipStateComponent.MaxQueryEnabled`（proposed） | 运行时/表现控制；初始化与场景清理 | 场景或粒子查询读取；主循环/场景重置写；不持久化、不作为世界规则 |
| 167 | `Main.DiscoR` | `DiscoColorState`（proposed presentation state） | 客户端表现帧状态；颜色更新/清理 | 彩虹 NPC、Projectile、Dust 和 WorldItem 表现读取；表现系统单一写者；不网络复制 |
| 168 | `Main.DiscoB` | `DiscoColorState.Blue`（proposed） | 与 `DiscoR/DiscoG` 同一表现生命周期 | 同上；不得让玩法系统写入 |
| 169 | `Main.DiscoG` | `DiscoColorState.Green`（proposed） | 与 `DiscoR/DiscoB` 同一表现生命周期 | 同上；不得让玩法系统写入 |
| 170 | `Main.gamePaused` | `FrameControlAndTimeSkipStateComponent.GamePaused`（proposed） | 当前主循环暂停快照；每帧重算；非持久化 | `DoUpdate` 和表现/输入边界读取；暂停判定系统单一写者；不等同于 `GlobalTimerPaused` |
| 171 | `Main.ReHideCursor` | `CursorVisibilityCommand`（proposed host/client command） | 一次性窗口表现请求；消费后清除 | `FocusHelper`/平台鼠标 adapter 消费；连接回调或窗口系统提交；不进入世界快照 |
| 172 | `Main.updatesCountedForFPS` | `DedicatedServerFrameMetrics.UpdatesInCurrentWindow`（proposed） | 服务端诊断窗口；计数、输出、归零 | dedicated-server loop 写；metrics projection 读；日志输出是 effect，不影响模拟 |
| 173 | `Main.autoJoin` | `AutoJoinRequest`（proposed command state） | 启动/连接一次性意图；消费后清除 | `AutoJoin`/Netplay adapter 读取；命令入口写；不持久化、不复制 |
| 470 | `Main.fastForwardTimeToDawn` | `WorldTimeSkipStateComponent.FastForwardToDawn`（proposed intent state） | 活动世界时间跳转意图；命令提交后由时间系统消费并清除 | `Sundialing`/time-skip system 写；`UpdateTime`/network projection 读；不把完整时钟放入本组件 |
| 471 | `Main.sundialCooldown` | `WorldTimeSkipStateComponent.SundialCooldownTicks`（proposed） | 时间跳转冷却；每 Tick 递减或按 Version4 语义更新；非独立世界规则 | 日晷命令系统单一写者；UI/network 只读投影；需验证边界和重载清理 |
| 472 | `Main.fastForwardTimeToDusk` | `WorldTimeSkipStateComponent.FastForwardToDusk`（proposed intent state） | 活动世界时间跳转意图；命令提交后由时间系统消费并清除 | `Moondialing`/time-skip system 写；`UpdateTime`/network projection 读；不持久化为完整时钟 |
| 473 | `Main.moondialCooldown` | `WorldTimeSkipStateComponent.MoondialCooldownTicks`（proposed） | 时间跳转冷却；每 Tick 递减或按 Version4 语义更新；非独立世界规则 | 月晷命令系统单一写者；UI/network 只读投影；需验证边界和重载清理 |

### 10.2 设计判断和契约

`hardMode` 虽位于同一库存组，但它是世界进度事实，不应与暂停或客户端颜色合并。`WorldGen.StartHardmode` 先提交 Hardmode，再执行后台转换和主线程 follow-up；P01 只提出只读 `HardmodeSnapshot` seam，最终写入者、快照字段和网络/存档版本由 `crossSubsystemOwner: integration-review` 裁决。

`gamePaused` 是一次主循环的暂停结果，`GlobalTimerPaused` 是进程/时钟控制输入；两者必须保持独立。`DiscoR/DiscoB/DiscoG` 是表现颜色通道，`ReHideCursor` 是平台消费的一次性请求，`autoJoin` 是网络连接命令，`updatesCountedForFPS` 是诊断计数。它们都不能成为权威世界状态。

```text
proposed FrameControlSystem
  reads: pause inputs, host focus and current frame context
  writes: GamePaused, MaxQueryEnabled and presentation/runtime caches
  effects: emits CursorVisibilityCommand and consumes AutoJoinRequest

proposed HardmodeReadProjection
  reads: committed HardmodeSnapshot
  writes: none
  owner: crossSubsystemOwner: integration-review
```

Failure semantics: a rejected pause or auto-join command leaves the prior committed world state unchanged; cursor and diagnostic effects may report failure without retrying world simulation. No implementation or verifier was run.

## 11. Checkpoint 5：WorldSeedAndSpecialRuleBoundary（proposed）

### 11.1 成员归属

| 输入序号 | Version4 成员 | proposed 归属 | 状态/生命周期 | 读者、写者和副作用 |
| ---: | --- | --- | --- | --- |
| 251 | `Main.rand` | `WorldRandomPort`（proposed thread/session-scoped adapter） | 生成或世界 Tick 的 RNG 端口；按线程/会话建立与清理；不是 Component 字段 | WorldGen、NPC、Projectile 等显式请求随机值；宿主/生成边界提供；随机状态不可持久化为 P01 组件；`crossSubsystemOwner: integration-review` |
| 252 | `Main.moonType` | `WorldPresentationRuleState.MoonType`（proposed）或外部天气/天体 adapter | 活动世界/表现配置；加载、规则变更和清理语义未闭合 | 天体/天气/音频表现读取；唯一写者和是否持久化需 integration-review；不能与 RNG 合并 |
| 253 | `Main.UseExperimentalFeatures` | `ExperimentalFeaturePolicy`（proposed immutable runtime policy） | 进程启动配置；启动导入后稳定；不属于世界存档 | 内容/系统注册边界读取；启动参数或配置 adapter 写；不网络复制为世界事实 |
| 254 | `Main.DefaultSeed` | `DefaultSeedPolicy`（proposed launch/generation input） | 启动或世界生成请求期间短命；消费后不保留为权威规则 | 世界生成请求读取；seed parser/RNG adapter 消费；有效 seed 提交到世界元数据的边界仍为 `integration-review` |
| 3857 | `SpecialSeedFeatures.ShouldDropExtraGel` | `SpecialSeedEligibilityQuery.ShouldDropExtraGel`（proposed pure result） | 从已提交 seed flags 派生；无缓存、无独立生命周期 | 掉落/内容系统读取；Query 无写者；不得写回 seed state |
| 3858 | `SpecialSeedFeatures.ShouldDropExtraWood` | `SpecialSeedEligibilityQuery.ShouldDropExtraWood`（proposed pure result） | 同上 | 掉落/内容系统读取；Query 无副作用 |
| 3859 | `SpecialSeedFeatures.DungeonEntranceHasATree` | `SpecialSeedEligibilityQuery.DungeonEntranceHasATree`（proposed pure result） | 同上 | 世界生成/装饰系统读取；Query 无副作用 |
| 3860 | `SpecialSeedFeatures.DungeonEntranceIsBuried` | `SpecialSeedEligibilityQuery.DungeonEntranceIsBuried`（proposed pure result） | 依赖 seed feature 输入；不可缓存为独立规则 | 世界生成读取；Query 无副作用 |
| 3861 | `SpecialSeedFeatures.DungeonEntranceIsUnderground` | `SpecialSeedEligibilityQuery.DungeonEntranceIsUnderground`（proposed pure result） | 同上 | 世界生成读取；Query 无副作用 |
| 3862 | `SpecialSeedFeatures.NoDungeonGuardian` | `SpecialSeedEligibilityQuery.NoDungeonGuardian`（proposed pure result） | 同上；由 shimmer/seed flags 输入计算 | NPC/生成系统读取；Query 无副作用 |
| 3863 | `SpecialSeedFeatures.BossesKeepSpawning` | `SpecialSeedEligibilityQuery.BossesKeepSpawning`（proposed pure result） | 同上 | NPC/事件系统读取；Query 无副作用 |
| 3864 | `SpecialSeedFeatures.ShimmerSpawnHalfOfWorld` | `SpecialSeedEligibilityQuery.ShimmerSpawnHalfOfWorld`（proposed pure result） | 同上 | 世界生成/地块系统读取；Query 无副作用 |
| 3865 | `SpecialSeedFeatures.RainbowSandAndBlackSandWalls` | `SpecialSeedEligibilityQuery.RainbowSandAndBlackSandWalls`（proposed pure result） | 同上 | 世界生成/墙体系统读取；Query 无副作用 |
| 3866 | `SpecialSeedFeatures.SpawnOnBeach` | `SpecialSeedEligibilityQuery.SpawnOnBeach`（proposed pure result） | 同上 | 玩家生成系统读取；Query 无副作用 |
| 3867 | `SpecialSeedFeatures.SpawnOnBeachOnDungeonSide` | `SpecialSeedEligibilityQuery.SpawnOnBeachOnDungeonSide`（proposed pure result） | 依赖 `SpawnOnBeach` 和 seed input；不得单独缓存 | 玩家/世界生成系统读取；Query 无副作用 |
| 3868 | `SpecialSeedFeatures.Mechdusa` | `SpecialSeedEligibilityQuery.Mechdusa`（proposed pure result） | 同上 | Boss/生成系统读取；Query 无副作用 |
| 2423 | `FixExploitManEaters.IndexesProtected` | `ManEaterProtectionIndexComponent`（proposed transient spatial index） | 每次 `Update` 清空；`ProtectSpot` 添加；`SpotProtected` 查询；世界会话/空间帧范围 | Man Eater 修复系统写，碰撞/生成逻辑读取；不持久化、不网络复制；空间 owner `crossSubsystemOwner: integration-review` |

### 11.2 设计判断和契约

`Main.rand` 的 `[ThreadStatic] UnifiedRandom` 体现的是随机源存取位置，不是可直接注册的 ECS 状态。任何随机性都必须通过显式 `WorldRandomPort` 注入并记录会话/生成阶段；Query 不应隐式拉取全局 RNG。`moonType`、实验开关和默认 seed 的生命周期不同，分别保留在天体/表现规则、进程配置和生成请求边界。

`SpecialSeedFeatures` 的十二个属性是纯资格计算：它们读取主规则 flag、`onlyShimmerOceanWorlds` 或 `WorldGen.SecretSeed`，不应形成十二个可变组件字段。若未来为性能缓存，必须另行声明缓存 owner、失效条件和验证，不得把缓存当成权威状态。

`IndexesProtected` 的写入/清理顺序是其语义的一部分：先由 `ManEaterProtectionSystem.Update` 清空当前集合，保护操作再写入，`SpotProtected` 只读。保护坐标属于 transient spatial capability，不得被 `RuntimeBootstrapAndWorldRuleStateComponent` 或世界规则快照吸收。

```text
proposed WorldRandomPort
  input: session/phase seed context
  output: explicit random values or scoped RNG handle
  effects: owns randomness and reproducibility diagnostics; no component mutation

proposed SpecialSeedEligibilityQuery
  input: immutable SecretSeedFlags + explicit SecretSeedFeatureSnapshot
  output: twelve derived bools
  effects: none

proposed ManEaterProtectionSystem
  Update: clear transient index
  ProtectSpot: add validated tile/spot key
  SpotProtected: pure membership read
```

Failure semantics: invalid seed input rejects the generation command before rule commit; a failed RNG adapter does not silently reuse a previous session's state. Protection-index duplicates are idempotent membership writes, and cleanup failure blocks the next spatial frame until integration review defines recovery.
Failure semantics: invalid seed input rejects the generation command before rule commit; a failed RNG adapter does not silently reuse a previous session's state. Protection-index duplicates are idempotent membership writes, and cleanup failure blocks the next spatial frame until integration review defines recovery.

## 12. Checkpoint 6：SessionReadinessAndWorldCatalogBoundary（proposed）

### 12.1 成员归属

| 输入序号 | Version4 成员 | proposed 归属 | 状态/生命周期 | 读者、写者和副作用 |
| ---: | --- | --- | --- | --- |
| 422 | `Main._worldPreparationState` | `SessionReadinessComponent.Phase`（proposed） | 会话生命周期：AwaitingData -> Ready 或显式失败/卸载；Ready 不能绕过 generation/loading barrier | `SessionReadinessSystem` 单一写者；实体资格、菜单、宿主和生成系统读取；失败码/转换事件需投影；`crossSubsystemOwner: integration-review` |
| 423 | `Main.motd` | `SessionPresentationState.Motd`（proposed）或 host/network projection input | 服务器/菜单会话配置；启动/命令更新；不属于世界持久规则 | CLI、服务器提示、客户端公告读取；命令或启动 adapter 写；是否网络复制需整合 |
| 424 | `Main.gameMenu` | `SessionReadinessComponent.InMenu`（proposed runtime view) | 客户端/服务器循环控制；菜单进入/离开；非世界存档事实 | 主循环、输入、UI 和实体资格查询读取；菜单状态系统写；不替代 `Phase` |
| 425 | `Main.lockMenuBGChange` | `MenuPresentationPolicy.LockBackgroundChange`（proposed client presentation state） | 菜单表现期间短命；场景切换清理 | UI/background system 读取；菜单控制写；不网络复制、不持久化 |
| 426 | `Main.maxLoadWorld` | `WorldLoadBudgetPolicy`（proposed host/session policy） | 加载/生成期间限制；进程或一次 session；非世界数据 | WorldFile/load pipeline 读取；启动配置或 host adapter 写；不得作为实体 component |
| 427 | `Main.ActivePlayerFileData` | `PlayerFileSelectionAdapter`（proposed external file boundary） | 当前客户端/玩家文件选择；菜单、加载和退出切换 | 玩家持久化 adapter 管理对象生命周期；`Main`/UI 读取 typed view；不放入 WorldSession component |
| 428 | `Main.WorldList` | `WorldCatalogProjection` + `WorldCatalogAdapter`（proposed） | 目录扫描结果；刷新、排序、失效；非活动世界权威状态 | UI/CLI 读取；文件/catalog adapter 写；`WorldFileData` 对象不跨核心边界 |
| 429 | `Main.ActiveWorldFileData` | `ActiveWorldMetadataProjection` backed by a proposed session metadata snapshot | 活动世界选择/加载/卸载；元数据快照与实际存档对象分离 | persistence/session systems 读取；唯一 commit owner 未决；网络只读投影；`crossSubsystemOwner: integration-review` |
| 430 | `Main.WorldPath` | `WorldStoragePathPort`（proposed adapter value） | 本地世界存储配置；进程/用户 profile；路径改变需重新验证 | WorldCatalog/WorldPersistence adapter 使用；路径服务写；不进入核心组件 |
| 431 | `Main.CloudWorldPath` | `CloudWorldStoragePort`（proposed external adapter value） | 云存档目录/提供者配置；平台连接生命周期 | cloud persistence adapter 使用；平台服务提供；不作为 WorldSession state |
| 432 | `Main.PlayerPath` | `PlayerStoragePathPort`（proposed external adapter value） | 玩家存档目录配置；进程/用户 profile | player persistence adapter 使用；路径服务提供；不进入核心组件 |

### 12.2 设计判断和契约

`_worldPreparationState` 只表达一个内部 readiness 标记；Version4 的 `ShouldUpdateEntities` 还检查 `WorldGen.isGeneratingOrLoadingWorld`，所以 proposed `SessionReadinessSystem` 必须同时接受生成/加载屏障输入，并在失败或卸载时撤销实体资格。`Ready` 不应由文件顺序或单个字段写入自动推导。


`ActivePlayerFileData`、`WorldList` 和 `ActiveWorldFileData` 是 `WorldFileData`/`PlayerFileData` 外部对象或目录集合。核心状态只能保存稳定 typed metadata 或 selection ID；读写、临时 staging、失败诊断和路径 I/O 由 adapter 管理。`WorldPath`、`CloudWorldPath` 和 `PlayerPath` 是存储边界，不是实体字段。


```text
proposed SessionReadinessSystem
  reads: load/generation barrier, host lifecycle and validated session commands
  writes: Phase, InMenu and stable failure code
  effects: emits readiness/failure events and gates entity updates

proposed WorldCatalogAdapter
  reads/writes: local/cloud/player storage ports and file metadata
  output: immutable catalog and active-world metadata projections
  effects: file I/O, staging, diagnostics and retry policy

proposed ActiveWorldMetadataProjection
  reads: committed session metadata snapshot
  writes: none
```

Failure semantics: catalog scan errors produce a typed unavailable/failed projection and do not replace the active world snapshot. A readiness failure prevents entity updates until an explicit unload or retry command; paths are validated before any adapter I/O.

## 13. Checkpoint 7：HostLoadAndShutdownBoundary（proposed）

### 13.1 成员归属

| 输入序号 | Version4 成员 | proposed 归属 | 状态/生命周期 | 读者、写者和副作用 |
| ---: | --- | --- | --- | --- |
| 463 | `Main._windowMover` | `WindowHostAdapter`（proposed platform adapter） | 客户端窗口生命周期；初始化、调整、释放 | 窗口/平台边界拥有第三方 controller；UI/host 读取稳定能力；不进入 ECS |
| 464 | `Main.sittingManager` | `AnchoredEntityRelationAdapter`（proposed external relation adapter） | 活动实体锚定关系；世界加载/更新/清理 | 玩家/NPC 锚定逻辑读写；adapter 管理集合和清理；不把集合对象放入 session component |
| 465 | `Main.sleepingManager` | `AnchoredEntityRelationAdapter`（proposed sleeping relation channel） | 与 sitting channel 同一实体/世界生命周期 | 玩家/NPC 睡眠锚定逻辑读写；必须和活动计数提交顺序协调；`crossSubsystemOwner: integration-review` |
| 466 | `Main.oldStatusText` | `RuntimeLoadProgressProjection`（proposed presentation cache） | 生成/加载过程的去重显示缓存；阶段完成或失败时清理 | host console/UI projection 读取；load-progress system 写；不作为 session readiness authority |
| 467 | `Main.autoGenFileLocation` | `AutoGenerationStorageRequest`（proposed command/adapter value） | 自动生成请求期间短命；消费、成功或失败后清理 | world-generation/persistence adapter 读取；路径验证和文件 I/O 在 adapter；不持久化为 world rule |
| 468 | `Main.autoShutdown` | `ShutdownPolicy` + `ShutdownRequestCommand`（proposed host state/command） | 进程生命周期；启动配置到退出；一次性请求可消费 | host loop/Server adapter 读取；启动或命令入口写；退出 effect 显式执行；不网络复制 |
| 469 | `Main.previousExecutionState` | `PlatformPowerStateAdapter`（proposed Windows power-effect adapter） | 平台电源防休眠 token；申请/恢复/释放 | `WindowsLaunch`/host lifecycle adapter 读写；平台 API effect；不进入 ECS |
| 3137 | `InitData.MaxNPCs` | `HostCapacityDefinition.MaxNpcs`（proposed immutable definition） | 进程启动固定容量；内容/NPC 初始化读取 | NPC allocator/host capacity systems 读取；定义提供；跨分区 owner `crossSubsystemOwner: integration-review` |
| 3382 | `Program.IsXna` | `RuntimePlatformCapabilities.IsXna`（proposed immutable host definition） | 进程启动平台 capability；只读 | bootstrap/platform adapter 读取；检测边界写；不作为世界事实 |
| 3383 | `Program.IsMono` | `RuntimePlatformCapabilities.IsMono`（proposed immutable host definition） | 进程启动平台 capability；只读 | bootstrap/platform adapter 读取；检测边界写；不进入 session component |
| 3384 | `Program.LaunchParameters` | `LaunchParameterAdapter`（proposed validated input port） | 启动解析阶段；解析后提供不可变 typed values | `Program`/host bootstrap 读取；adapter 拥有原始字典和校验；`Dictionary<string,string>` 不跨边界 |
| 3385 | `Program.SavePath` | `UserStoragePathPort`（proposed external path adapter） | 用户 profile/进程生命周期；配置变更需重新验证 | world/player persistence adapters 读取；path service 写；不进入 ECS |
| 3386 | `Program.TerrariaSaveFolderPath` | `UserStoragePathDefinition`（proposed immutable definition） | 固定目录名；进程级 | path adapter 读取；不成为可变组件字段 |
| 3387 | `Program.ThingsToLoad` | `RuntimeLoadProgressComponent.TotalWorkUnits`（proposed process state） | 启动加载期间；初始化、增量更新、完成/失败清理 | `LoadProgressSystem` 单一写者；console/UI projection 读；不进入 world snapshot |
| 3388 | `Program.ThingsLoaded` | `RuntimeLoadProgressComponent.CompletedWorkUnits`（proposed process state） | 启动加载期间单调递增或失败重置 | load pipeline 写；metrics/progress projection 读；不得驱动 entity readiness 单独成立 |
| 3389 | `Program.LoadedEverything` | `RuntimeLoadProgressComponent.IsComplete`（proposed derived/process state） | 加载阶段完成标记；失败/退出时失效 | load coordinator 写；host/readiness gate 读；必须与 generation barrier 分开 |
| 3390 | `Program.JitForcedMethodCache` | `JitRuntimeAdapter`（proposed unsafe/runtime adapter） | 进程/JIT lifetime；初始化和释放由运行时管理 | bootstrap/runtime adapter 使用 `IntPtr`；不持久化、不注册组件 |
| 3428 | `Ref<T>.Value` | `ReferenceBridgeAdapter<T>`（proposed aliasing boundary） | 外部引用包装生命周期；赋值/读取/释放 | 调用方通过 adapter 访问；保留引用别名语义；禁止当作通用 ECS field |
| 3666 | `WindowsLaunch._handleRoutine` | `ShutdownSignalAdapter`（proposed platform callback adapter） | Windows 进程回调注册/注销；宿主生命周期 | platform callback 写入 shutdown command；host loop 消费；`HandlerRoutine` 不跨核心边界 |
| 3911 | `Terraria.Server.Game.Components` | `ServerFrameworkAdapter.ComponentServices`（proposed server adapter view） | Server.Game 进程生命周期；框架组件容器 | server host 读取；第三方 framework owns collection；不进入 ECS |
| 3912 | `Terraria.Server.Game.Content` | `ServerContentAdapter`（proposed external service boundary） | Server.Game 进程生命周期；内容加载/释放 | server host/content pipeline 读写；`ContentManager` 不跨核心边界 |
| 3913 | `Terraria.Server.Game.GraphicsDevice` | `ServerGraphicsAdapter`（proposed capability boundary） | 服务端/宿主平台生命周期；可能为空或不可用 | rendering/content boundary 读取；graphics service owns object；不进入 simulation component |
| 3914 | `Terraria.Server.Game.InactiveSleepTime` | `HostTimingPolicy.InactiveSleepTime`（proposed definition） | 宿主节流配置；进程生命周期 | host scheduler 读取；policy adapter 写；不改变 world clock |
| 3915 | `Terraria.Server.Game.IsActive` | `HostLifecycleQuery.IsActive`（proposed pure host query） | 进程/窗口生命周期派生 | host scheduler/adapter 读取；无 ECS writer；不能作为 world readiness |
| 3916 | `Terraria.Server.Game.IsFixedTimeStep` | `HostTimingPolicy.IsFixedTimeStep`（proposed definition） | 宿主调度配置；进程生命周期 | host scheduler 读取；不等同于 authoritative simulation tick contract |
| 3917 | `Terraria.Server.Game.IsMouseVisible` | `WindowPresentationAdapter.IsMouseVisible`（proposed client/host adapter value） | 窗口表现生命周期；焦点和退出清理 | window adapter 读写；不进入 session state |
| 3918 | `Terraria.Server.Game.LaunchParameters` | `ServerLaunchParameterAdapter`（proposed typed input port） | Server.Game 启动生命周期；只读解析视图 | server bootstrap 读取；framework owns source object；不复制为 ECS component |
| 3919 | `Terraria.Server.Game.Services` | `ServerServiceContainerAdapter`（proposed external service port） | server host 生命周期；注册/解析/释放 | framework/host 读写；`GameServiceContainer` 不跨核心边界 |
| 3920 | `Terraria.Server.Game.TargetElapsedTime` | `HostTimingPolicy.TargetElapsedTime`（proposed definition） | 宿主调度配置；进程生命周期 | host scheduler 读取；不得替代 world simulation clock |
| 3921 | `Terraria.Server.Game.Window` | `WindowHostAdapter.Window`（proposed platform view） | 窗口生命周期；创建/销毁 | UI/host adapter 使用；`GameWindow` 不进入 ECS |

### 13.2 设计判断和契约

本检查点的共同边界不是“加载状态组件”，而是把进程/平台/框架对象和 typed host state 分开。只有 `ThingsToLoad`、`ThingsLoaded`、`LoadedEverything` 适合形成有限的 `RuntimeLoadProgressComponent` 候选；它只描述进程加载进度，不能单独把 WorldSession 置为 Ready。`MaxNPCs`、平台 capability 和宿主 timing 是 immutable definitions；窗口、GraphicsDevice、ContentManager、Services、`Ref<T>`、句柄和回调全部停留在 adapter/port。

```text
proposed RuntimeLoadProgressSystem
  reads: load work-unit events and host lifecycle
  writes: TotalWorkUnits, CompletedWorkUnits, IsComplete
  effects: emits progress/failure projection; never writes WorldSession readiness directly

proposed HostShutdownSystem
  reads: ShutdownRequestCommand, auto-shutdown policy, platform signal adapter
  writes: process-lifetime shutdown phase
  effects: unregisters callbacks, restores platform power state, closes host services

proposed ServerFrameworkAdapter
  reads/writes: third-party Game, components, content, graphics, services and window APIs
  output: typed host capabilities only
```

Failure semantics: unavailable graphics or window capability is represented as an explicit host capability result; it does not fabricate an ECS object. Load progress failure emits a typed failure and invalidates completion. Shutdown callback duplication is idempotent, and platform restoration failure is reported at the adapter boundary without mutating world rules.
Failure semantics: unavailable graphics or window capability is represented as an explicit host capability result; it does not fabricate an ECS object. Load progress failure emits a typed failure and invalidates completion. Shutdown callback duplication is idempotent, and platform restoration failure is reported at the adapter boundary without mutating world rules.

## 14. Checkpoint 8：DerivedWorldSessionQueriesAndDifficultyDefinitions（proposed）

### 14.1 成员归属

| 输入序号 | Version4 成员 | proposed 归属 | 状态/生命周期 | 读者、写者和副作用 |
| ---: | --- | --- | --- | --- |
| 513 | `Main.SavePath` | `WorldSessionQuery.SavePath`（proposed read-only adapter view） | 由宿主存储配置派生；无独立写者 | persistence/UI/CLI 读取；路径 adapter 提供；不写 WorldSession component |
| 514 | `Main.GameMode` | `WorldSessionRuleQuery.GameMode`（proposed effective query over active metadata） | 从活动世界元数据读取；setter 必须改为显式规则命令 | world/session systems 读取；规则 commit system 写 metadata；`crossSubsystemOwner: integration-review` |
| 515 | `Main.IsJourneyMode` | `WorldSessionRuleQuery.IsJourneyMode`（proposed pure result） | 从有效 GameMode 派生 | systems/UI 读取；Query 无副作用 |
| 516 | `Main.NoFunctionalSurface` | `WorldSurfaceQuery.NoFunctionalSurface`（proposed pure result） | 从 `worldSurface`/world geometry 派生；每次查询或显式 cache | world generation/interaction reads；Query 无写者 |
| 517 | `Main.surviveHardcoreDeath` | `WorldSessionRuleQuery.SurviveHardcoreDeath`（proposed pure result） | 从 dont-starve、anniversary、good-world flags 派生 | player/death system reads；Query 无副作用 |
| 518 | `Main.onlyShimmerOceanWorlds` | `WorldSessionRuleQuery.OnlyShimmerOceanWorlds`（proposed pure result） | 从 seed flags 派生；与 SpecialSeed query 保持同一输入快照 | world generation/NPC reads；Query 无副作用 |
| 519 | `Main.masterMode` | `DifficultyQuery.IsMasterOrAbove`（proposed pure result） | 从有效 difficulty 数值派生 | combat/NPC/content reads；Query 无副作用 |
| 520 | `Main.expertMode` | `DifficultyQuery.IsExpertOrAbove`（proposed pure result） | 同上 | combat/NPC/content reads；Query 无副作用 |
| 521 | `Main.Difficulty` | `DifficultyQuery.EffectiveDifficulty`（proposed pure calculation） | 按 override -> GameMode -> good-world 增量顺序计算；每帧/规则快照查询 | systems/read models 读取；override/system 输入提供；不得缓存为无效的基础规则 |
| 522 | `Main.Achievements` | `AchievementServiceAdapter`（proposed external service boundary） | 进程/玩家服务生命周期；外部 manager 所有 | achievement/UI systems 读取/提交；adapter 隔离第三方 manager；不进世界组件 |
| 523 | `Main.UnpausedUpdateSeed` | `FrameRandomSeedProjection`（proposed frame cache/projection） | 非暂停更新时推进；重启/会话初始化；非持久世界规则 | deterministic frame consumers/diagnostics 读取；frame system 单一写者；`crossSubsystemOwner: integration-review` |
| 527 | `Main.GameUpdateCount` | `FrameSequenceQuery`（proposed read-only counter view） | 主循环递增；进程/运行时；不持久化为世界事实 | systems/diagnostics 读取；main loop/frame system 写；不作为 external ID |
| 528 | `Main.worldID` | `PersistentWorldIdQuery`（proposed typed ID view） | 活动世界元数据生命周期；非实体 ID | persistence/network projections 读取；metadata commit owner 未决；`crossSubsystemOwner: integration-review` |
| 529 | `Main.isThereAWorldSurface` | `WorldSurfaceQuery.HasFunctionalSurface`（proposed pure result） | 从 surface geometry 派生 | generation/interaction reads；无 writer |
| 530 | `Main.UnderworldLayer` | `WorldGeometryQuery.UnderworldLayer`（proposed pure result） | 从 `maxTilesY` 派生；世界尺寸改变时失效 | generation/physics reads；无 writer；不持久化派生值 |
| 532 | `Main.SceneMetrics` | `SceneMetricsAdapter` + `SceneMetricsQuery`（proposed client/camera projection） | 帧/相机上下文；可按玩家或相机切换 | audio/UI/lighting reads；scene system 更新外部 metrics；不进入 authoritative world component |
| 535 | `Main.LocalPlayer` | `LocalPlayerProjection`（proposed client entity projection） | 客户端 player selection 生命周期；随 `myPlayer`/实体连接变化 | UI/input/camera reads；player registry owns entity; no world persistence |
| 536 | `Main.npcShop` | `NpcShopSessionState`（proposed client interaction state） | 当前 NPC 商店会话；打开/关闭/切换 | UI/player interaction reads; shop interaction system writes; no world snapshot |
| 537 | `Main.playerPathName` | `PlayerPathProjection`（proposed persistence view） | 从 active player file selection 派生 | UI/save systems read；adapter supplies path；not entity/world component |
| 538 | `Main.worldPathName` | `WorldPathProjection`（proposed persistence view） | 从 active world metadata/file selection 派生 | UI/save/CLI read；adapter supplies path；not authoritative path state |
| 539 | `Main.IsItAHappyWindyDay` | `WeatherPresentationQuery.IsHappyWindyDay`（proposed pure view） | 从 committed weather/ambient inputs 派生；本分区不拥有天气 input | audio/scene presentation reads；weather owner writes inputs；`crossSubsystemOwner: integration-review` |
| 540 | `Main.IsItStorming` | `WeatherPresentationQuery.IsStorming`（proposed pure view） | 从 committed weather/ambient inputs 派生；本分区不拥有天气 input | audio/scene presentation reads；weather owner writes inputs；`crossSubsystemOwner: integration-review` |
| 1169 | `GameDifficultyData.LinearCurve.Key.input` | `DifficultyCurveKey.Input`（proposed immutable definition field） | 静态定义生命周期；构造时设置 | `DifficultyCurve` sampling reads；无 runtime writer |
| 1170 | `GameDifficultyData.LinearCurve.Key.output` | `DifficultyCurveKey.Output`（proposed immutable definition field） | 静态定义生命周期；构造时设置 | `DifficultyCurve` sampling reads；无 runtime writer |
| 1171 | `GameDifficultyData.LinearCurve.keys` | `DifficultyCurve.Keys`（proposed immutable ordered definition data） | 进程内容定义生命周期；构造后只读 | pure interpolation query reads；定义加载边界 owns construction |
| 1172 | `GameDifficultyData.EnemyMaxLifeMultiplier` | `DifficultyRuleDefinition.EnemyMaxLifeMultiplier`（proposed immutable definition） | 进程级内容定义；只读 | NPC/combat queries read; no component writer |
| 1173 | `GameDifficultyData.EnemyDamageMultiplier` | `DifficultyRuleDefinition.EnemyDamageMultiplier`（proposed immutable definition） | 同上 | combat query reads; no writer |
| 1174 | `GameDifficultyData.HostileProjectileDamageMultiplier` | `DifficultyRuleDefinition.HostileProjectileDamageMultiplier`（proposed immutable definition） | 同上 | projectile/combat query reads; no writer |
| 1175 | `GameDifficultyData.KnockbackToEnemiesMultiplier` | `DifficultyRuleDefinition.KnockbackToEnemiesMultiplier`（proposed immutable definition） | 同上 | combat query reads; no writer |
| 1176 | `GameDifficultyData.EnemyMoneyDropMultiplier` | `DifficultyRuleDefinition.EnemyMoneyDropMultiplier`（proposed immutable definition） | 同上 | loot/economy query reads; no writer |
| 1177 | `GameDifficultyData.TownNPCDamageMultiplier` | `DifficultyRuleDefinition.TownNpcDamageMultiplier`（proposed immutable definition） | 同上 | NPC/combat query reads; no writer |
| 1178 | `GameDifficultyData.DebuffTimeMultiplier` | `DifficultyRuleDefinition.DebuffTimeMultiplier`（proposed immutable definition） | 同上 | status-effect query reads; no writer |
| 1179 | `GameDifficultyData.LightningPlayerDamageScaling` | `DifficultyRuleDefinition.LightningPlayerDamageScaling`（proposed immutable definition） | 同上 | weather/combat query reads; no writer |
| 1180 | `GameDifficultyLevel.Journey` | `DifficultyLevelDefinition.Journey = 0.5`（proposed constant） | 进程级静态定义 | `DifficultyQuery` reads; no writer |
| 1181 | `GameDifficultyLevel.Classic` | `DifficultyLevelDefinition.Classic = 1`（proposed constant） | 同上 | `DifficultyQuery` reads; no writer |
| 1182 | `GameDifficultyLevel.Expert` | `DifficultyLevelDefinition.Expert = 2`（proposed constant） | 同上 | `DifficultyQuery` reads; no writer |
| 1183 | `GameDifficultyLevel.Master` | `DifficultyLevelDefinition.Master = 3`（proposed constant） | 同上 | `DifficultyQuery` reads; no writer |
| 1184 | `GameDifficultyLevel.Legendary` | `DifficultyLevelDefinition.Legendary = 4`（proposed constant） | 同上 | `DifficultyQuery` reads; no writer |

### 14.2 设计判断和契约

本检查点的 38 条成员全部是派生视图、客户端服务或静态定义，不应机械注册为一个“WorldSession derived component”。`GameMode` 的 setter 是隐藏写入口，迁移时必须改为显式规则命令并由单一 commit owner 校验有效值；`Difficulty` 需要保留 Version4 的 override -> mode -> good-world 优先级。`SavePath`、玩家/世界路径和 `worldID` 的类型/边界必须与外部存储和持久化 ID 区分。


`LinearCurve.Sample` 是纯计算，但插值端点和 key 顺序必须保留；曲线定义不应在运行时修改。`SceneMetrics`、`LocalPlayer`、`npcShop` 和 `Achievements` 属于客户端或外部服务投影；天气音乐两个 bool 依赖天气 owner，本分区只提供 Query 边界。


```text
proposed DifficultyQuery
  reads: active world rule snapshot, optional override, immutable difficulty definitions
  returns: effective difficulty, threshold flags and sampled multipliers
  writes: none

proposed WorldSessionQuery
  reads: typed session metadata, geometry snapshot and client projection inputs
  returns: path, ID, surface, layer, player/shop and weather views
  writes: none

proposed RuleCommandHandler
  reads: validated GameMode command
  writes: committed active-world metadata through one owner
  effects: persistence/network projections after commit
```

Failure semantics: invalid GameMode or malformed curve definitions reject the command/definition load without changing the committed snapshot. A missing client projection returns an explicit unavailable result rather than a fake `Player` or service object; pure queries do not retry or mutate state.
Failure semantics: invalid GameMode or malformed curve definitions reject the command/definition load without changing the committed snapshot. A missing client projection returns an explicit unavailable result rather than a fake `Player` or service object; pure queries do not retry or mutate state.

## 15. Checkpoint 9：ManEaterProtectionIndexComponent（proposed）

成员 2423 已在 Checkpoint 5 中完成来源、读写方向和生命周期映射；本检查点只收束其独立的空间能力边界，不重复注册或复制成员。

| 输入序号 | proposed 组件/系统 | 不变量 | 失败和清理 |
| ---: | --- | --- | --- |
| 2423 `FixExploitManEaters.IndexesProtected` | `ManEaterProtectionIndexComponent` + `ManEaterProtectionSystem` | `Update` 在空间帧开始清空；`ProtectSpot` 只添加经过验证的 spot key；`SpotProtected` 只读 membership；重复保护幂等 | 清理失败不得静默沿用上一帧索引；系统报告失败并阻止下一次空间提交，恢复策略由 `crossSubsystemOwner: integration-review` 决定 |

`ManEaterProtectionIndexComponent` 不是世界规则、存档字段或网络快照。其 seam 是一个短生命周期空间索引；生产实现必须验证清理先于保护写入、保护查询不产生写入，以及 world unload 时集合被释放。

### 15.1 已保存的持久化和复制边界

实现阶段已在 `src2/WorldSession/Runtime/` 保存 `WorldSessionCommittedSnapshot`、`WorldPersistenceAdapter`、`WorldPersistenceResult` 和 `WorldSessionReplicationProjection`。快照只承载 typed active-world metadata、secret-seed flags 和 Hardmode read projection；adapter 只接受已提交快照并提供显式 rollback，projection 只向下游复制，不把 wire DTO 或客户端恢复状态反写为权威 Component。该边界是最小非权威 contract，不等于已完成 Version4 文件格式、生产 I/O、网络 packet 7 或客户端恢复闭合。

## 16. 完整成员覆盖确认

P01 输入报告的 149 条成员均已获得 proposed 角色或明确的跨边界角色，来源序号集合如下：

- `1-4`；`7-9`；`18-37`；`48-60`；`165-173`；`251-254`。
- `422-432`；`463-473`；`513-523`、`527-530`、`532`、`535-540`；`1169-1184`。
- `2423`；`3137`；`3382-3390`；`3428`；`3666`；`3857-3868`；`3911-3921`。

这些集合计数为 149，且没有把其他 P 分区成员复制进来。`2423` 的详细来源映射在 Checkpoint 5，空间系统契约在 Checkpoint 9；它仍只有一个 proposed writer。成员 coverage checker、串行构建和 focused verifier 均已通过；生产接线和跨分区 owner 仍待整合。

## 17. Proposed Contract Handoff

### 17.1 角色契约

- Component：只保存一个内聚概念的权威或瞬态状态；P01 候选包括 frame activity、runtime rule input、frame control、readiness、load progress 和空间保护索引。所有目标类型均为 proposed。
- System：在显式阶段读取输入并提交状态转换；每个候选组件只有一个 proposed writer，跨分区 writer 标为 `crossSubsystemOwner: integration-review`。
- Query：只做可重复的派生、资格、阈值或投影读取；不得隐式修改状态、推进时钟或消耗 RNG。
- Command：表达暂停、自动加入、自动生成、时间跳转、退出、GameMode 和存储请求等意图；提交边界验证失败时不改变已提交快照。
- Adapter/Port：隔离随机、时钟、文件、网络、平台、第三方服务、窗口、资源、`IntPtr`、外部集合和框架类型。
- Projection/Snapshot：单向输出持久化、网络、客户端、日志或诊断视图；不得反向成为权威 Component。

### 17.2 ID、持久化、网络和客户端边界

`WorldSession` root entity ID、实体 ID、持久化 World ID、网络 ID、WorldSection ID、客户端/账户 UUID、文件路径键和外部服务句柄必须保持不同类型和生命周期。`worldID` 只提出 `PersistentWorldIdQuery`，不宣布 ECS entity ID 或 network ID 的 owner；这些公共值对象统一 `crossSubsystemOwner: integration-review`。

持久化方向为 `Component/committed metadata -> typed snapshot -> WorldPersistenceAdapter`；失败结果通过 typed error 返回，不把临时文件、异常对象或半写状态写回核心。网络出站方向为 `committed snapshot -> WorldSessionReplicationProjection -> wire adapter`；入站只产生 Command。包 7 的客户端恢复路径仍有 unresolved evidence，不能把 wire DTO 当作权威状态。

客户端方向为 `committed/session query -> LocalPlayerProjection, SceneMetricsProjection, presentation adapters`。窗口、音频、Achievement service、MOTD、颜色、光标和 NPC shop 是表现或交互边界；它们不进入权威世界快照。

### 17.3 Proposed system order

```text
HostBootstrapAdapter
  -> SessionReadinessSystem
  -> RuntimeBootstrapImportSystem
  -> FrameActivityResetSystem
  -> entity contribution systems
  -> FrameActivityCommitSystem
  -> TimeSkipCommandSystem / WorldTimeSkipStateSystem
  -> FrameTimingAndDeferredProcessSystem
  -> WorldSessionCommitSystem
  -> PersistenceAdapter and ReplicationProjection
  -> client/diagnostic projections
```

这是 P01 的 proposed 调度契约，不是跨分区最终顺序。Version4 的实体更新、`UpdateTime`、WorldGen/invasion、服务器更新、Hardmode 后台转换和网络发送槽位必须由 integration review 通过 focused trace 锁定；不能用文件名或目录顺序代替调度约束。

## 18. Evidence Gaps, Blocking Decisions and Verification

- `evidence-gap`：P01 专属第一轮 public-decomposition 输出文件不存在；输入库存只确认声明/计数，逐成员读者、写者、生命周期、网络/存档提交和宿主语义仍有 partial/unresolved；任务包中的 `Terraria\\WorldFile.cs` 与实际 `Terraria.IO\\WorldFile.cs` 不一致；`约束/公共拆分约束.md` 未在声明路径找到。
- `blocking-decision`：`crossSubsystemOwner: integration-review`；需整合会话锁定 WorldSession 根实体、Hardmode 事务、FrameActivity 聚合、WorldData 客户端恢复、宿主/平台隔离、公共 ID/快照以及 Version4 时间与网络/持久化提交顺序。
- focused verifier：`src2/WorldSessionVerification/Program.cs` 已覆盖 frame activity trace、readiness gate、time-skip compatibility、host boundary、difficulty curve/query、special-seed input、ManEater cleanup/index ordering 和 typed persistence/replication boundary；WorldData packet 7 golden bytes、one-writer static guard 和行为回放仍未闭合。
- 已实际执行并通过本会话的验证：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 对 `src2/WorldSessionVerification/Terraria.WorldSessionVerification.csproj` 串行 build（exit code 0，0 warning，0 error），确认两个 DLL 位于 `Build/bin/`；随后以 `--no-build --no-restore` 运行 focused verifier（exit code 0，输出 `P01 verifier passed.`）；149-member coverage audit 的实际序号集合与执行映射完全相等，禁止边界引用审计无命中。`verificationStatus: independently-verified` 仅表示这些 src2 局部证据，不能据此声称行为等价、网络闭合或生产持久化完成。

## 19. Integration Handoff

交给整合会话的最小裁决集：

1. 为 Hardmode、WorldSession root、active metadata、公共 IDs、snapshot value objects 和 FrameActivity 提交唯一 owner。
2. 关闭 `WorldFileData`/packet 7 客户端恢复证据，并确认持久化版本和网络字段顺序。
3. 把 readiness、load progress、generation barrier、host shutdown 和 Version4 entity/update-time/worldgen/server 顺序放入同一条可追踪调度链。
4. 决定 moonType、天气派生输入、NPC stream policy 和跨分区规则覆写的归属；保留 P01 的 adapter/query 边界直到裁决完成。
5. focused verifier、串行 build 和成员覆盖审计已经通过；仍需由后续整合会话决定是否按 proposed execution plan 接入生产代码。本文件不构成迁移许可、行为等价证明或跨分区 owner 裁决。

本文件所有跨分区 Component、System、Query、Command、Adapter、Projection、接口、路径和文件名仍是 proposed 设计对象。P01 实现模式已创建/修改的源码仅位于 `src2`；没有修改 `src`、Version4、第一轮报告或其他分区文档。runner 已使用当前 manual session ID 完成结算：`status=completed`、`exitCode=0`、`lockReleased=true`。
