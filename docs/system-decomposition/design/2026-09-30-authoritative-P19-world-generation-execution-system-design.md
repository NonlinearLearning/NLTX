# 权威 P19 世界生成 System 设计

documentKind: system-design  
partitionId: P19  
derivedFrom: docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P19-world-generation-execution.md  
sourceReportSessionId: 33c1c3a45612439da49380669be73114  
designStatus: proposed  
targetSystemCoverage: partial  
implementationStatus: partial
verificationStatus: core-slice-only
sourceModified: true
buildRun: true
testsRun: true

## 1. 目的与范围

本文把已结算的 P19 静态拆分报告细化为候选 System 边界、API 组合和协作契约，供后续实现准备使用。范围仍是 `WorldGenerationAndEcology` 下 10 个叶子组、47 个字段和 37 个属性，共 84 个成员。成员完整名单以来源报告为准。

本文提出 P19 的 System 边界，不是新的 runner 分区结算，也不修改来源报告。目标树已实现有序 pass-result commit 和单个已激活 pass 的 runner composition，详见执行文档；P19 的入口接线、完整生命周期、快照持久化和选项目录仍未形成端到端 owner，本文不表示行为等价或迁移成功。

排除最终 owner 决定：P17 地形与 `GenVars`、P18 seed 定义与解析语义、P20 action/shape payload、`Main`/`WorldFile` 世界加载保存边界、UI/回调/调度/网络。这些保留为 `crossSubsystemOwner: integration-review`。

## 2. 证据基线

### 2.1 Version4 与只读 CPG

Version4 `D:\TRbackup\Version4` 是行为主来源。相关文件哈希及完整证据清单见来源报告的 “Scope and Evidence”；其源码目录没有 Git 元数据。只读 CPG 数据库为 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`，manifest SHA-256 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，project fingerprint 为 `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`，`SourceSnapshotId` 为 null。

本轮通过只读 `CpgEvidence.ps1` 对关键关系做了复查：

| 查询 | 结果 | 设计用途与限制 |
| --- | --- | --- |
| `Find-CpgSymbols(GenerateWorld)` | `Terraria/WorldGen.cs` 与 `Terraria.WorldBuilding/WorldGenerator.cs` 的精确声明均为 `complete` | 解析静态入口身份；不证明所有动态入口。 |
| `Find-CpgCallSites(WorldGen.GenerateWorld)` | 所选 `Main.cs`、`WorldGen.cs`、`WorldFile.cs` 范围中 2 个 callsite，状态 `complete`，位置为 `WorldGen.cs` 与 `WorldFile.cs` | 两条选定静态调用边成立；不关闭其他 caller 或 callback。 |
| `Find-CpgCallSites(WorldGenerator.GenerateWorld)` | 所选 `WorldGen.cs` 与 `WorldGenerator.cs` 范围中 1 个 callsite，状态 `complete` | 确认静态 wrapper 到实例生成器的选定边。 |
| `Get-CpgMemberUses(PassResults)` | `WorldGenerator.cs`、`WorldGenSnapshot.cs`、`WorldManifest.cs` 选定范围 14 项，查询 `complete`；访问模式均未证明为唯一写入闭包，`AliasMutation` 为 unknown | `PassResults` 是属性，返回可变 `List`；不能仅凭属性声明或使用数量确定唯一 writer。 |
| `Get-CpgMemberUses(_currentPass)` | `WorldGenerator.cs` 4 项，查询 `complete`；2 项是 `assignment-left-span` 写入，2 项为 `Unknown` | 支持当前游标在选定 shard 内有两个直接写点；不证明所有 writer 或外部读取已闭合。 |
| `Get-CpgCallableFacts(RunPass)` | `partial`，仅 5 个 CFG 节点，含 `CalleeEffectsNotExpanded` | 源码是 stub；索引事实不能由完整镜像补成 Version4 行为。 |
| `Find-CpgCallSites(RunPass)` | `WorldGenerator.GenerateWorld` 到 `RunPass` 的一个选定 `CallTargets` 边为 `complete/confirmed` | 证明所选 shard 内 pass loop 的直接调用；不证明 RunPass 的行为。 |
| `Find-CpgCallSites(WorldGenerator.GenerateWorld)` | `WorldGen.GenerateWorld` 到实例生成器的一个选定边为 `complete/confirmed` | 该边只证明 wrapper 调用；`CreateNewWorld` 通过 delegate 排队的调用路径不在此静态调用结果内。 |
| `Find-CpgCallSites(WorldGen.GenerateWorld)` | `WorldGen.cs` 选定范围中一个直接 caller 为 `complete/confirmed` | 对应 `worldGenCallback`；查询不关闭 `CreateNewWorld` 到 callback 的 delegate 关系，源码观察仍单独记录。 |
| `Find-CpgSymbols/Find-CpgCallSites(ForceUpdateProgress)` | 符号与 `WorldGenerator.cs` 内两个选定调用点均 `complete/confirmed` | 与 Version4 源码逐处核对后，方法只更新 message、enabled 总权重和已完成 enabled 权重；selected callers 不代表完整动态调用闭包。 |

查询 `complete` 只表示所选索引范围在预算内完成。CPG 未绑定到当前 Version4 的每文件源码快照；查询结果与源码关系需逐处核对，缺口仍为 `partial` 或 `unknown`。

### 2.2 完整参考源码与组织参考

`D:\TRbackup\无任何删减通过编译` 是完整参考镜像。它补充观察了 Version4 中被清空方法的可能行为；文件哈希分别记录如下：

| 完整参考文件 | SHA-256 |
| --- | --- |
| `Terraria/WorldGen.cs` | `B9F7834CE1BC68C1DD9C656574A2272DB6F79E1407D934E1ADA33EDC3C930F82` |
| `Terraria.WorldBuilding/WorldGenerator.cs` | `56CA4E8F06B13368625CFB3C9B643DB757995B2F4BA1AC8043F9D0712AAB146F` |
| `Terraria.WorldBuilding/GenPass.cs` | `05D9EB92E08B4166A33B7753FAEA29F6622DB614B0AC52BD661133A561FCB6EE` |
| `Terraria.WorldBuilding/GenerationProgress.cs` | `58AF1723DE9EC0CAC3C0DE958BDEDC4951983451821E1899DAF53A9C19ED4CE8` |
| `Terraria.WorldBuilding/WorldGenConfiguration.cs` | `5D8B8F44CD2ACFB5281FEAB6E9E6135817B0A644103135D7ABA353E009A233D5` |
| `Terraria.WorldBuilding/Passes.cs` | `AA421E3D4E64B665D869DC2D038689970F4FD86032D419D24CE698DD1B0D67C9` |
| `Terraria.WorldBuilding/GenPassResult.cs` | `B2143778BD1E41DC33304201C749A88A57E059D6D5517C681EE791F6ED29A853` |
| `Terraria.WorldBuilding/WorldGenSnapshot.cs` | `6EC1EEEF313676D5EFC84AA639B044DCACE190B63304455501344BBFD4D7AB1B` |
| `Terraria.WorldBuilding/WorldManifest.cs` | `6D3988E51E4A26B9E1AF1D2BCBDADA53CD763B52B64B226FAEBBEDBC590C76A4` |
| `Terraria.WorldBuilding/WorldGenerationOptions.cs` | `381192E41560D77BC544046C65C48F807A9FDB85C21AA912C7111D4DB3232B52` |
| `Terraria.WorldBuilding/AWorldGenerationOption.cs` | `C321D36945BE1C27AE417AA1A2745622D00C7C66CC68BFC516ABB5279C76940E` |

目标 Version4 的两个对应文件哈希为 `WorldGenerator.cs` `917FFA37464C607EC4A205EAB0E0454A30AF606BCD15DE3500F160F969BF9774`、`GenPassResult.cs` `37937CE30EE15C0881B65088E7256C9F385C2608BF3D9C265578A51B4E12438A`。完整镜像 `WorldGenerator.RunPass` 的可见行为是：禁用 pass 返回带名称和 `Skipped` 的结果；启用 pass 启动计时、以 `_seed` 重置 `Main.rand`、调用 progress `Start`、执行 `pass.Apply` 和命名配置、捕获 pass 异常并报告、调用 progress `End`，最后记录耗时与 `WorldGen.genRand.Next()`。镜像 `GenPassResult.Matches` 以名称、`RandNext`、`Skipped` 比较；只有两边都有 hash 时才比较 hash。

这些仍是 reference-only 行为。Version4 的 `RunPass` 只返回新的占位结果；其 `GenPassResult` 声明只有 `DurationMs`、`Hash`、`Skipped`，没有 `Name`/`RandNext`，`ToString` 也是占位实现。当前 NSSLC 结果组件用 `PassId` 表达身份，并新增可选的 `RandomNextValue`；runner port 与注册 callback adapter 可将显式值带回提交系统，但尚无真实 callback 生成或验证该值。因此当前代码只保证显式输出值随有序结果提交，不保证完整镜像的 pass 结果比较、随机状态或地形执行效果；不得静默把镜像行为写成 Version4 行为合同。若后续决定采用该行为，必须先确定所接受的目标语义，再补结果映射和观察测试。

完整参考树还显示 `GenPass.Apply` 调用虚方法 `ApplyPass(progress, configuration)`；`WorldGen.AddPasses` 通过 `AddGenerationPass` 注册 `GenPass` 或具名委托，pass 委托会取名配置并直接更新 progress。`WorldGenConfiguration.GetPassConfiguration(name)` 从 pass 配置树按名称取值，缺项时返回空配置。`WorldGen.GenerateWorld` 依次载入 embedded config、调用 config hook、建 generator、clear/reset、添加并禁用 pass、执行 generator、Finish，最后在 `finally` 恢复临时状态；`worldGenCallback` 只在成功时保存，且在完成后更新 menu/sound 并调用 callback。`CreateNewWorld` 把 `worldGenCallback` 交给 `Task.Factory.StartNew`，这是 delegate 执行路径，CPG 的直接调用边未闭合。该组源码只用于提出目标 runner 所需的 pass operation、配置、progress 与 lifecycle 接口，不证明 Version4 清空方法的行为。

本轮重新使用只读 CPG Query API：`AddPasses` 符号和 `WorldGen.GenerateWorld` 在选定 `Terraria/WorldGen.cs` 范围内的一个直接调用点返回 `complete`；`GetPassConfiguration` 在选定 Version4 索引范围没有返回符号项，不能据此确认目标配置映射。直接源码还显示 Version4 的 `AddGenerationPass` overload 是空方法，因此 `AddPasses` 的注册效果仍为 `unknown`；完整参考项目的对应 overload 才会 append 到 generator。`WorldGenerator.RunPass` 的符号和其选定文件范围内一个直接调用点也返回 `complete`；`WorldGenerator.GenerateWorld` callable facts 为 `partial`，含 `CalleeEffectsNotExpanded` gap。数据库 `SourceSnapshotId` 为 null，因此这些索引结果仍需与当前源码逐处核对。

完整镜像中的 `Controller.SetGenerator`、`OnPaused`、`OnPassCompleted` 和快照转换/恢复实现也只能形成待核对行为清单，不得替代 Version4 中对应 stub 的行为基线。

组织方式参考 `C:\Users\shan\Downloads\ECS\space-station-14-master`：`Content.Shared/Hands/EntitySystems/SharedHandsSystem.cs` 以 partial 文件组合同一 Hands 能力，`Content.Shared/Containers/ExitContainerOnMoveSystem.cs` 独立订阅一项容器移动事件并调用既有 Climb/Container 服务。这只用于比较“同能力 partial”与“有独立入口的窄 System”两种组织方式，不构成 P19 运行时语义、writer 或调度证据。

### 2.3 当前 NSSLC

当前目标树已有 `WorldGenerationLifecycleComponent`、`WorldGenerationPlanComponent`、`WorldGenerationPassStateComponent`、`WorldGenerationProgressComponent`、`WorldGenerationPassSelectionComponent`、Controller 控制/暂停/hash/snapshot-policy/abort 组件、Manifest/PassResult 组件和 OptionSelection 组件。`WorldGenerationPassExecutionSystem` 维护 plan/pass state 的开始、完成、失败和 control transitions；本轮增加了有序结果提交、下一未提交 descriptor 选择、enabled plan/result prefix 的 weighted progress 汇总，以及通过 `IWorldGenerationPassRunner` 执行一个已激活 pass 的同步组合。该组合由调用方提供已激活状态、configuration snapshot、下一 stage/checkpoint 和 runner；System 负责 progress callback、结果身份填充、失败标记及成功后的有序提交。新增的 `RegisteredWorldGenerationPassRunner` 将精确 pass ID 分发到调用方注册的 callback，并在构造时复制 callback 集合、拒绝重复 ID；`WorldGenerationPassCatalog` 只把显式 registration 顺序转换为 plan、disabled ID 和 runner，不发现或实现旧 pass。当前仍没有真实 pass callback 注册、pass-specific config 投影或 `Main`、`WorldGen`、`WorldFile` caller；并发锁、暂停中的中断与取消恢复仍 unknown。`OceanBiomePassControlSystem` 等窄能力 System 和 `Controller/WorldGenerationAbortStateComponent.cs` 的存在不证明 P19 的 writer、调用路径或 abort 行为已闭合。

## 3. 概念行为

| 行为切片 | 输入与权威状态 | 可观察结果 | 目前的关键缺口 |
| --- | --- | --- | --- |
| 开始与结束一次生成 | world identity、seed、配置、progress/controller 输入；run flags 与共享世界状态 | 任务/回调结果、world save 条件、sound/menu/UI 和临时标志清理 | 调度失败、取消、重入、callback 异常及多 world 语义 unknown。 |
| 按序执行 pass | 有序 pass 集合、Enabled、当前结果数、progress | 当前 pass、单次结果提交、进度、后续 pass 顺序 | Version4 `RunPass` 是 stub；pass 注册/动态扩展未闭合。 |
| pause/hash/abort/reset 控制 | pause、pause-after-pass、abort、hash policy、snapshot index 与 pass cursor | 下一 pass 的可见性、暂停/继续、恢复结果和 UI/debug effect | Version4 多个 Controller 回调是 stub；锁的并发/重入合同不完整。 |
| manifest 与快照 | manifest/pass results、GenVars、tile snapshot、文件系统 | 持久化记录、可恢复 checkpoint、旧快照失效/删除 | schema/version、原子恢复、损坏文件和 rollback unknown；跨 P17。 |
| seed option 与配置 | option catalog、processed seed、server config、embedded config | option 选择、Enabled 变化回调、autogen 行为和 pass 配置 | callback subscribers、外部扩展及 P18 所有权未闭合。 |

## 4. System 边界与状态归属

能力名是本设计中的 proposed 概念身份，不预先承诺最终类名或独立 scheduler node。若后续证据显示某能力不需要独立 owner、调度或协作契约，应留在同一 System/partial 内，而不是为了成员分组增加类型。

| P19 成员组 | Proposed owner / boundary | 关键职责与约束 |
| --- | --- | --- |
| `WorldGenerationExecutionState` | run flags 与 `_generator` 由 `WorldGenerationLifecycleSystem` 协调；trap/metric members owner unknown | 接受开始/结束流程并协调准备、完成与 finally 清理；`SmallConsecutivesFound`、`SmallConsecutivesEliminated`、`placingTraps` 的 callers/writers 未闭合，不因 inventory 分组而归给 lifecycle；不拥有地形算法。 |
| `WorldGenerationProgressAndPassState` | `WorldGenerationPassExecutionSystem`；pass identity/weight definition 是输入 | 维护 progress 与当前 pass 的执行不变量；不按每个 pass 增加异步 command queue。`Enabled` 的完整 writer 与 option/pass-registration 关系仍待闭合。 |
| `WorldGenerationControllerPassState` | execution/control 的控制视图；snapshot index 由 persistence boundary 提供 | Passes/CurrentPass/LastCompletedPass 是只读投影；不另建第二套 pass cursor。 |
| `WorldGenerationControllerPauseAndHashState` | `WorldGenerationControlSystem`，初始与 pass commit 共用协调边界 | 表达 pause/resume/abort/hash/reset 意图；不得独立写 `_currentPass` 或 `PassResults`。 |
| `WorldGenerationGeneratorExecutionState` | `WorldGenerationPassExecutionSystem` | 维护有序 pass cursor、progress、result commit 与 run context；random/config/clock 为外部输入或 adapter。 |
| `WorldGenerationSnapshotState` | `WorldGenerationSnapshotPersistenceAdapter` | 负责路径、编码、文件生命周期和 restore 校验；不成为 P17 live tile/GenVars 的第二写者。 |
| `WorldGenerationManifestAndPassResults` | PassExecution 负责有序结果提交；`WorldManifestSerializerAdapter` 负责序列化边界 | 保持结果与 cursor 的一致提交；读写 WorldFile/快照格式不进入 pass owner。 |
| `WorldGenerationOptionBaseState` | `WorldGenerationOptionCatalog` + `WorldGenerationOptionSelectionSystem`；`_biomeRoot` / `_passRoot` 归 configuration adapter | 静态 option 定义/展示 metadata 与可变 Enabled/AutoGenEnabled selection 分开；`WorldGenConfiguration` roots 是生成配置输入，不属于 option catalog。definition 查询不得返回可写 alias。 |
| `WorldGenerationOptionRegistry` | `WorldGenerationOptionCatalog` 与注册 adapter | 注册与 enumeration 是 catalog 能力；duplicate/扩展/init ordering 保持未决，直到调用闭包完成。 |
| `WorldGenerationSupportTypes` | 保持为 payload/adapter/query 合同 | GenAction/ShapeData 交 P20；tree predicate 交 P17；OptionStorage 不单独生成 System。 |

### 4.1 写入协议

拟定不变量是：只有一个 `WorldGenerationPassExecutionSystem`（或同一运行时 owner 的 partial）推进当前 pass 并提交对应的有序结果；Control 只能通过明确的同步/提交 API 影响下一次执行。`PassResults.Count` 与 pass cursor 的关系必须由一个提交边界维护。静态证据尚未证明 Version4 当前代码只有一个 writer，因此这是 proposed 约束，不是既成事实。

`WorldGenerationControlSystem` 在锁/提交协议被证明前保持与 execution coordinator 同一同步域。不能把 Controller 的共享字段访问许可、单个 `_controlLock` 和业务优先顺序误当为一个事实。也不在没有调度器证据时添加 ECS phase、barrier、事件总线或并行执行假设。

### 4.2 持久化与恢复协议

Serializer/adapter 只把稳定 manifest/GenVars/tile snapshot 数据编码、读取和校验。恢复成功后，应由各自 live-world authority 提交状态；Snapshot adapter 不直接成为地形、NPC、随机源或全局世界状态的权威 writer。文件 schema、版本 gate、rollback/atomicity、快照历史迁移和 cache lifetime 均需 integration review。

## 5. 概念 API 组合

以下是完整行为组合草案。目标树已有局部 plan/pass-state API；本表不把这些局部 API 当作完整旧入口组合，也不要求逐项新增同名 Command/Query 类型。

| 旧入口/行为 | Proposed composition | 必须保留的观察合同 | 证据 |
| --- | --- | --- | --- |
| `Main` 世界创建 -> `WorldGen.CreateNewWorld` | legacy entry adapter -> `StartWorldGeneration` -> lifecycle owner -> scheduler adapter | active world/seed/UI 标记先于调度；回调与任务错误、取消、重复 start 的语义需补证 | Main 到 CreateNewWorld 源码边确认；调度失败行为 unknown。 |
| `worldGenCallback` | lifecycle coordinator -> pass execution -> success-only `SaveNewWorld` -> menu/sound/completion callback | 保存条件、sound/menu/callback 相对顺序和 bool 结果一致 | Version4 顺序可从源码确认；callback 异常/保存失败闭包 unknown。 |
| `WorldFile.LoadWorld` autogen | seed/option resolution -> selection commit -> lifecycle generate -> success-only save -> existing load/error continuation | copied seed 与 text seed 分支、Reset-then-Select、AutoGenEnabled 以及失败后的现有路径 | 两条 option/select callsite 与 WorldFile 生成调用在选定范围可见；其他调用未闭合。 |
| `WorldGen.GenerateWorld` | `PrepareRun` -> config adapter/hook -> generator/pass catalog -> reset/disable rules -> synchronous `ExecutePasses` -> finish -> `finally` cleanup | 保留语句顺序与临时状态清理；未知 hook/pass effect 不由 API 设计猜测 | Version4 高层语句顺序确认；callee effects partial。 |
| `WorldGenerator.GenerateWorld` / `RunPass` | `TryGetNextPass` 与 `BeginPass` 选定/激活 descriptor；`ExecuteActivePass` 调用显式 runner、接收 progress 和 measurements、补齐结果身份并提交；上层仍需提供 stage transition 与 pass catalog | abort/pause gate、单 pass 同步执行/提交在目标 API 中实现；whole-loop 控制锁、random reset、pass config dispatch、catalog 与 legacy caller 未接入 | Version4 loop 和一个选定 `RunPass` caller 边 confirmed；Version4 `RunPass` callable 为 partial/stub；完整参考镜像只能提供操作形状。 |
| Controller pause/reset/snapshot | control intent -> shared execution/control commit gate -> snapshot adapter -> live-world owners | 同步锁结果、返回值、恢复失败路径、UI 和重试必须按实际实现固定 | 多个方法体 stub 或 dynamic/callback closure open。 |
| `WorldManifest` 与 `WorldFile` 存档 | manifest projection -> versioned serializer/file adapter；snapshot 复用格式需经协议决定 | record offset、version gate、pass result order、nullable hash、load fallback 和 save exception | Serialize/Deserialize 调用位置确认；完整兼容未验证。 |
| `WorldGenerationOptions` | catalog read -> explicit selection/config command -> synchronous state-change notification adapter | registration uniqueness/order、seed normalization、Reset-before-Select、AutoGenEnabled、回调可见性 | 选定 callers 确认；subscriber 与插件闭包 unknown。 |
| support payload | P20 action API / P17 tree query / option storage adapter | payload 所有权与生命周期由相邻 owner 决定 | consumers 与目标组合尚未闭合。 |

`Options` 目前暴露可变 option instances，且 `Enabled` setter 同步触发 virtual hook 与 static event。因此现有对象枚举不能直接视为 immutable pure Query。pause/reset/restore/selection 属于改变状态的操作，不是 Query。

## 6. 依赖与副作用方向

| 提供边界 | P19 消费/提供 | 限制与开放决定 |
| --- | --- | --- |
| P17 world/terrain/GenVars | P19 传递显式 terrain/config snapshot；恢复交 live-world owner commit | terrain、tile、tree predicate、NPC cleanup 和 reset 的唯一权威 writer 未决。 |
| P18 seed/secret-seed | P19 接收选定 seed identity 与生成 option 输入 | seed matching/definition 的归属与 selection commit timing 未决。 |
| P20 GenAction/ShapeData | pass 执行消费 action chain 或产出明确 action intent | chain lifetime、输出 owner、失败和顺序未决。 |
| Main/WorldFile | 生命周期适配器接入 legacy start/load/save | 异步任务失败、save compatibility、callback 顺序和恢复路径需确认。 |
| filesystem/serializer | snapshot 与 manifest adapters | corruption、atomicity、schema migration、cache 和 world identity 未决。 |
| UI/log/sound/localization/assets | 单向 effect/presentation adapters | 同步可见性、客户端/服务器边界和 callback subscribers unknown。 |
| scheduler/thread/network | lifecycle 请求与完成结果适配 | 单次/多 world 并发、thread affinity、取消和 replication contract unknown。 |

不使用目录顺序或 System 注册顺序来表达 pass 次序。Legacy pass order 在业务执行器的有序数据/调用协议中保留；目标 scheduler DAG、barrier 和跨线程可见点须由实际框架与集成证据另定。

## 7. 主要未知与设计门禁

1. CPG 与 Version4 当前文件缺少 snapshot binding；`PassResults` 的可变 alias 和其他 writer 未闭合。
2. Version4 `RunPass`、`Controller.SetGenerator`、`OnPaused`、`OnPassCompleted`、`UpdatePreviousManifest`、`ReportException`、debug UI helper 与 Snapshot converter 中有 stub；完整镜像只能补充待确认行为清单。当前有通用 pass callback registry/dispatch adapter，但没有真实 pass callback 注册、配置投影或生产调用点。`GenPassResult` 在目标和镜像间的字段也不同；当前可选 `RandomNextValue` 由 callback 显式返回，随机源如何按 seed 重置、它与 `WorldGen.genRand.Next()` 是否同义仍未映射。progress 的 message 需要 legacy display name，而当前 descriptor 只有 ID。
3. pass population/注册顺序、特殊 seed 对 pass 的影响、选项 event subscribers、动态 hooks 与世界加载 caller 未闭合。
4. Snapshot restore 涉及 GenVars、tile、manifest、NPC 和文件；原子提交、损坏处理、rollback 与多世界路径隔离 unknown。
5. `generatingWorldOnThisThread` 为 thread-static，但多数 run state 与随机源/manifest/options 是共享静态状态；多世界并行支持 unknown。
6. P17、P18、P20、WorldFile/Main、UI/event/config、scheduler/network/persistence owner 必须由 integration review 决定；本设计不替其最终分配权威。

只要上述未知影响对应的 writer/API/scheduler/恢复合同，设计就保持 `proposed`。缺口只阻断依赖它的切片；不得用完整镜像、Component 存在、静态查询或文档完成来升级状态。

## 8. 设计验收条件

- 每个 P19 invariant 有唯一提交者；PassResults/cursor/control 的同步合同明确。
- Legacy 入口按概念行为映射到新的 owner 与适配器；状态变化、结果、事件、顺序、异常、范围与生命周期均列入后续观察向量。
- P17/P18/P20、world file、snapshot、UI/event、scheduler/thread/network handoff 均有 owner 和协议决定，或依赖该决定的实现继续 blocked。
- 快照/manifest adapter 不绕过 live-world authority 写共享状态；查询接口不暴露可变 alias 或隐藏副作用。
- 通过真实迁移入口的必需行为验证后才可声称迁移成功。

当前结论：`designStatus: proposed`，`targetSystemCoverage: partial`，`implementationStatus: partial`，`verificationStatus: core-slice-only`。有序结果提交、单 active pass runner composition、通用 callback dispatch 与显式 registration catalog 通过窄验证；本文仍不表示旧入口已接线、行为等价或迁移成功。
