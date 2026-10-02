# 权威 P18 World Seeds 与 Skyblock System 设计

| 字段 | 值 |
| --- | --- |
| `partitionId` | `P18` |
| `taskId` | `AUTH-SYS-P18` |
| `sourceReportSessionId` | `e4147654f9464b5193bb35f895cdc63f` |
| `designStatus` | `proposed` |
| `implementationStatus` | `partial` |
| `verificationStatus` | `partial` |
| `migrationStatus` | `deferred` |
| System 报告 | [`2026-09-18-system-decomposition-authoritative-P18-world-seeds-skyblock-definitions.md`](../reports/2026-09-18-system-decomposition-authoritative-P18-world-seeds-skyblock-definitions.md) |
| 配套执行计划 | [`2026-09-30-authoritative-P18-world-seeds-skyblock-system-execution.md`](../execution/2026-09-30-authoritative-P18-world-seeds-skyblock-system-execution.md) |
| Version4 目标源码 | `D:\TRbackup\Version4` |
| 完整参考源码 | `D:\TRbackup\无任何删减通过编译` |
| 目标代码树 | `D:\TRbackup\NLTX\src\NSSLC` |
| 组织参考 | `C:\Users\shan\Downloads\ECS\space-station-14-master` |

本文把已结算的 P18 静态报告整理为 proposed System 设计，并记录一次局部实现。报告 session 只作来源 provenance；本次代码只覆盖 Skyblock 候选 owner 的加载扫描、逐列累计、规则求值和提交组合，没有接入真实生产调用方，也没有完成 P18 迁移。

## 1. 目的与范围

P18 覆盖权威输入台账中的 12 组、86 个字段、50 个属性，共 136 个成员。方法和调用点仅用于解释这些成员的行为，不把范围扩成 `WorldGen` 全量拆分。

设计目标是按状态生命周期、不变量和同步行为路径确定 System 能力。静态定义不建成独立 System；Command、Query、Adapter 和 Projection 都是按真实协作需要选择的职责，不按类型数量拆分。

## 2. 证据基线

### 2.1 目标源码与 CPG Query API

Version4 是此分区的目标行为来源。目标源文件 SHA-256 和完整的静态报告见上方链接。只读 CPG 数据集 manifest SHA-256 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；它没有 `SourceSnapshotId`，不能证明索引与当前磁盘源码为同一快照。

本次补查的静态调用关系如下。`complete` 表示指定 shard 范围内查询完整，不表示运行时闭包或动态调用闭合。

| Query API | 结果 | 使用边界 |
| --- | --- | --- |
| `Find-CpgCallSites(WorldGen.Reset)` | `WorldGenerator.TryReset`、`WorldGenSnapshot.Restore` 两个 `CallTargets`，selected paths 状态 `complete` | 只确认两个选定 shard 中的静态调用点；不证明所有 reset/restore 入口 |
| `Find-CpgCallSites(Skyblock.ScanTiles)` | `WorldFile.cs` 一个 `CallTargets`，selected path 状态 `complete` | 对应加载后扫描入口；不证明其他动态入口不存在 |
| `Find-CpgCallSites(Skyblock.Calculate)` | `WorldGen.cs` 两个 `CallTargets`，selected path 状态 `complete` | 源码定位分别落在 `ScanTiles` 和 `CountTiles` 的末列分支 |
| `Find-CpgCallSites(WorldGen.CountTiles)` | `WorldGen.cs` 一个 `CallTargets`，selected path 状态 `complete` | 源码显示由 `UpdateWorld` 在 30 tick 计数分支推进列扫描 |
| `Find-CpgCallSites(SecretSeed.InitializeSecretSeeds)` / `FinalizeSecretSeeds` | 各一个选定 shard 内调用点，状态 `complete` | 源码分别对应 `WorldGen.Reset` 和生成结束路径 |
| `Find-CpgCallSites(SecretSeed.Enable)` | `WorldFileData.cs` 两处（源码行 265、396）、`Main.cs` 一处（行 2473）静态 `CallTargets`，查询状态 `complete` | 只闭合这两个 Version4 shard 的静态调用 |
| `Find-CpgCallSites(SecretSeed.CheckInputForSecretSeed)` | `Main.cs` 一处（源码行 2471）静态 `CallTargets`，查询状态 `complete` | 不含完整参考项目新增的 UI/config callers |
| `Get-CpgMemberUses(TextThatWasUsedToUnlock)` | 所选 `WorldGen.cs`、`WorldFileData.cs`、`Main.cs` 范围内，`WorldGen.cs:526` 一处 `Write`，状态 `complete` | 这是索引命中的 member-use，不证明其他 shard、动态访问或完整参考没有读取 |

静态调用边不能补齐事件订阅、反射、动态分派、外部程序集或运行时调度。原报告中的 `AccessMode: Unknown`、索引未覆盖和调用闭包缺口继续保持 `partial` / `unknown`，不从零命中推出“不存在”。

### 2.2 完整参考源码

完整参考项目由用户指定，并作为 Version4 的语义补充来源，不覆盖 P18 输入台账或目标行为基线。本次只核对 P18 相关字段、方法及其直接调用/消费点，不代表对完整工程做了全仓差异审计。已读源码身份如下：

| 完整参考文件 | SHA-256 | 用途 |
| --- | --- | --- |
| `D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs` | `B9F7834CE1BC68C1DD9C656574A2272DB6F79E1407D934E1ADA33EDC3C930F82` | 对照 `Reset`、秘密种子和 Skyblock 实现 |
| `D:\TRbackup\无任何删减通过编译\Terraria\Main.cs` | `E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F` | 检查全局旗标及入口差异 |
| `D:\TRbackup\无任何删减通过编译\Terraria\Star.cs` | `6217F55D7CF8BCA4F336FC0ECD708EA479EC7371893D745D1AAFB0668DB4DB47` | 检查额外的 active-secret-seed 读取者 |
| `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\SecretSeedsTracker.cs` | `65EC7FF9DE3DAC5320D4D6ED2A27E5B5DF16B873931353E107B2A3CE3B62376A` | 检查配置/UI projection、去重和保存效果 |
| `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.UI.States\UIWorldCreation.cs` | `DE5B5EC29745C1D4B7C9B48C1BCAE0495E5B4B36D5D556A18B2864AE634039A9` | 检查 tracker 初始化及输入查找调用 |
| `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.UI.States\UIWorldCreationAdvancedSecretSeedsList.cs` | `35D8E1ED2E16EBA3811EFF998E4B87A784123830FC285821FC3BB983A25FDD9F` | 检查列表读取和 enable/disable UI 调用 |
| `D:\TRbackup\无任何删减通过编译\Terraria.IO\WorldFile.cs` | `92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289` | 加载后 `ScanTiles` 入口；该文件 hash 与 Version4 对应文件相同 |
| `D:\TRbackup\无任何删减通过编译\Terraria.IO\WorldFileData.cs` | `DCBEFBDECF5A35C3CFB8F7EBC4FF6F1A9B70FA2FC78F57E612E881F7C0D2B47C` | 对照 seed 输入、重置和保存相关路径 |

Version4 与完整参考在秘密种子路径上的共同源码行为是：输入匹配成功后更新 `_plaintext` 和清洗后的 `TextThatWasUsedToUnlock`；启用只在 disabled -> enabled 转换时增加活动计数、置 enabled，并按 `playSound` 决定声音。完整参考还在同一状态转换内、声音之前，对非 dedicated server 调用 `SecretSeedsTracker.AddSeedToTrack`。tracker 接受配置字符串、调用同一匹配入口构造去重并排序后的 UI 列表、向设置写回 `SecretSeeds`，新增条目时调用 `Main.SaveSettings()`；`Main` 与 world-creation UI 是其配置和交互调用方。完整参考中的 `SecretSeed.anySecretSeedIsActive` 还被 `Star.cs` 两处读取。上述 tracker / Star 接线在 Version4 目标范围未确认，属于跨子系统差异，owner 仍为 `integration-review`。

完整参考的 `WorldGen.Skyblock` 另有 `spawnSolidifier` 和 `spawnShimmerPool`，并在生成函数中被读取；Version4 对应文件未找到这两个属性。共同存在的 `denyFloatingIslands` 不列作版本差异。新增属性、tracker/UI 配置闭包及 `anySecretSeedIsActive` 消费者不在 P18 的 136 成员输入范围内，不能静默并入 Version4 迁移，也不能将其缺失解释为实现错误。它们作为参考版差异保留，待基线和跨分区 owner 评审。

用户提供的完整参考路径名带有“通过编译”说明；本文只记录该用户提供的来源背景，没有在本轮重新编译，也没有独立核实那一编译结果。

| 关系边 | 证据位置 | 状态 |
| --- | --- | --- |
| Version4 `Main` 输入处理 -> `SecretSeed.CheckInputForSecretSeed` -> `_plaintext` / `TextThatWasUsedToUnlock` 写入 | CPG：`Main.cs` callsite；源码：`WorldGen.cs:499-527` | 选定 shard 内 confirmed；索引无 snapshot ID |
| Version4 `WorldFileData` -> `SecretSeed.Enable` | CPG：两处 callsite；源码：`WorldFileData.cs:265,396` | 选定 shard 内 confirmed |
| 完整参考 `SecretSeed.Enable` -> `SecretSeedsTracker.AddSeedToTrack` -> `Main.SaveSettings` | `WorldGen.cs:532-548`；`SecretSeedsTracker.cs:53-63` | 源码直接调用 confirmed；此工程不在 Version4 CPG 数据集 |
| 完整参考配置 -> tracker -> world-creation UI | `Main.cs:4402,4734`；`UIWorldCreation.cs:209`；secret seed list UI 的 `SeedsForInterface`、`Enable/Disable` 调用 | 源码直接读写 confirmed；UI 生命周期及完整调用闭包 partial |
| 完整参考 `SecretSeed.anySecretSeedIsActive` -> `Star` 行为 | `Star.cs:170,275` | 两个源码读取点 confirmed；跨分区 owner 为 `integration-review` |
| 当前候选 `Match` 结果 -> registry command | `WorldSecretSeedInputMatch` 有文本字段；`EnableSecretSeedCommand` 无文本字段 | 当前目标树未见组合接线，证据 partial；不得推断文本已提交 |

### 2.3 当前目标候选

`src/NSSLC/Component/WorldSession/WorldGeneration` 中已有秘密种子 registry、Skyblock scan/query/commit 和 option catalog 候选类型。但本次静态搜索没有确认这些候选已接入 `WorldFile`、`WorldGen.Reset`、`UpdateWorld` 或调度器。类型存在不等于生产路径已使用。

当前候选代码已提供整图 `ScanAndCommit` 与 `AccumulateColumnAndCommit` 两个同步组合。后者按 `CountTiles` 的 X 列次序累计所有 tile 的 wall、active tile 类型和数量，在最后一列求值并提交；可选 `WorldSurfaceY` 输入保留 `worldSurface + 1` 的行段读取顺序。owner 保存上一次提交的 rules，从其 `LowTiles` 推导状态转变，并在清理累计数据后按“清 dungeon 坐标、通知 lowTiles 变化”的顺序执行 effect port。规则 Query 在非 Skyblock 世界仍计算内容旗标，只将 `LowTiles` 置 false。

这些 API 尚未接入 `WorldFile`、`WorldGen.UpdateWorld` 或 `WorldGen.CountTiles` 的生产调用点；外部 grid reader 是否复现 null tile 初始化等边界仍为 `unknown`。Query 对 `WorldTileCount > 0` 保留防护，而 Version4 在零尺寸下直接做除法；该差异没有被本次核心验证覆盖，不能宣称行为等价。

秘密种子候选当前由 `WorldSecretSeedInputAdapter.Match` 返回匹配结果，结果中带有规范化 plaintext 与原始解锁文本；`EnableSecretSeedCommand` 只带 `GenerationId`、`RuntimeVersion`、`Variant` 和 `IdempotencyKey`，registry commit 更新 enabled-variant 与命令幂等元数据，不接收这两个文本字段，当前没有确认文本状态被提交或持久化。匹配策略还使用 invariant lowercase，而目标源码调用当前文化的 `ToLower()`。候选 `MatchAndPlaySound` 在匹配成功时即可播放声音；旧 `Enable` 只在未启用到启用的转换分支播放声音，所以不能把它当作等价的生产组合入口。以上是静态源码差异，尚未由行为测试验证。

本次只运行了五个局部核心场景：非 Skyblock 世界内容旗标、加载扫描 40 格边界、逐列累计与末列提交、列顺序拒绝、末列前提交拒绝。它们验证候选 API，不调用旧 Version4 路径，也不证明真实接线、完整行为等价或迁移成功。

### 2.4 组织参考

SS14 的 `SharedHandsSystem` 以 partial class 将相关 API、事件及多个初始化职责分布在文件中；`PullingSystem.CanPull` 会发布两个可取消事件。它们只说明 partial System 和 API 名称不能代替职责/效果分析，不证明 Terraria 行为或要求 NSSLC 采用同一事件模型。

## 3. 边界决策

| 能力 | 设计决定 | 为什么 |
| --- | --- | --- |
| 秘密种子输入与启用集合 | 一个同步 registry owner 负责匹配成功的文本状态、enable、disable、clear 和 active-count 不变量；静态定义与活动集合分开 | 目标匹配 API 本身回写 plaintext/unlock text；声音受状态转换门控；完整参考另有配置/UI 持久化效果，owner 待 integration review |
| Skyblock 世界状态 | 一个有序行为组合完成扫描/增量观察、规则求值和提交；不因目标文件分开而建多个 scheduled System | 加载整图扫描和 `UpdateWorld -> CountTiles` 增量路径最终共享 `Calculate` 的派生和重置规则 |
| 选项选择与生成旗标 | 选择提交与 `WorldGen.Reset` 投影分开设计；canonical writer 交 `integration-review` | P16/P17/P19/P20 共享旗标、恢复和生成期读者尚未闭合，禁止 P18 与 coordinator 双写 |
| 秘密种子/选项/树木/地貌定义 | 保持 immutable Definition/Catalog；仅在有复用读取契约时提供 Query | 这些数据本身不构成每组一个 System 的状态不变量 |
| 纯读取 | Query 接收显式 snapshot/context；不得读隐式 `Main`、`genRand` 或惰性可变 alias | derived options/variations 有短路、随机和 lazy data 语义，调用时点属于行为 |

不采用“一个 WorldGenerationSystem 包含所有能力”：它会合并 UI/config 选择、活动 registry、静态定义和 tile 扫描等不同生命周期。也不按 12 个输入组分别建 System；那会把成员分组误作调度边界。

## 4. 十二组成员归属

| 输入组 | 字段/属性 | 设计归属 | owner 状态 |
| --- | ---: | --- | --- |
| `WorldGenerationSecretSeedFlags` | 10 / 0 | 选择状态 snapshot 与 Reset 旗标投影 | `integration-review` |
| `WorldSecretSeedRegistryDefinitions` | 6 / 0 | 静态 immutable definitions | 无独立 System |
| `WorldSecretSeedVisualAndSurfaceRules` | 9 / 0 | 静态规则 definitions | 无独立 System |
| `WorldSecretSeedTerrainAndStructureRules` | 11 / 0 | 静态规则 definitions | 无独立 System |
| `WorldSecretSeedProgressionAndInfectionRules` | 12 / 0 | 静态规则 definitions；共享状态只读输入 | `integration-review` |
| `WorldSecretSeedSeasonalRules` | 3 / 0 | 静态规则 definitions；季节效果由既有 owner 负责 | 无独立 System |
| `WorldSecretSeedRuntimeRegistry` | 2 / 1 | 活动 variant 集合及其派生计数 | 一个 registry owner；生命周期待定 |
| `WorldSecretSeedDerivedOptions` | 0 / 2 | 显式 seed/world/random 输入上的推导 | Query 职责；调用时点待接入 |
| `WorldSecretSeedDerivedVariations` | 0 / 22 | 显式 snapshot 上的规则结果 | Query 职责；调用时点待接入 |
| `WorldSkyblockGenerationRules` | 11 / 3 | 扫描/增量状态、规则求值和顺序提交 | 单一同步组合；跨域 writer 待定 |
| `WorldSeedOptionCatalog` | 1 / 21 | option definitions 与 seed/config 查找 | immutable catalog；选择写入另有 owner |
| `WorldLandmassAndTreeProfiles` | 21 / 1 | landmass/tree profile definitions 与显式求值输入 | 无独立 System；生成 owner 调用 |

## 5. 概念 API 组合

### P18.SeedOptionSelection

`seed/config adapter -> WorldSeedOptionCatalogQuery -> selection commit -> generation reset coordinator`。
查找 API 只返回匹配结果；选择 owner 才能触发 `Enabled` setter、回调和静态事件。保持 registration order、匹配/默认/未匹配语义、`Reset` 再选择、`Enabled` 与 `AutoGenEnabled` 的区分。Reset 读取选择后写 `Main`/`WorldGen` 旗标，调用秘密种子初始化，再以世界 seed 重置 `Main.rand`；顺序仍是调用契约。

### P18.SecretSeedActivation 与 RuleEvaluation

`外部 seed text -> 输入适配/定义匹配 -> registry owner 记录匹配的 plaintext/unlock text -> enable/disable/clear -> 只读 snapshot`。匹配成功对定义状态有写入，故解析 API 不能按纯 Query 处理。保留空值/未匹配返回、当前文化大小写转换、规范化和 plaintext/code 查找顺序、仅匹配成功时回写文本、重复启停只改变一次计数、clear 不注销定义、声音只在新启用转换时播放。

完整参考基线若获准纳入，还需在启用状态已提交之后、声音之前，由有明确 authority 的 adapter 处理 dedicated-server 门控、tracker 去重、设置保存及 UI 投影；`SeedsForInterface` 当前暴露可变列表，迁移 API 不应将其直接变成 Query 的 live alias。tracker 与设置/UI owner 未定，标记 `crossSubsystemOwner: integration-review`。完整参考的 `anySecretSeedIsActive` 及 `Star` 读取者、Skyblock 两个额外 generation properties 同样只进入基线决策，不由 P18 单独创建跨分区 owner。

派生 option/variation 的 Query 必须在旧决策点调用，输入包含所有旗标和必要的随机决定。不得提前批量求值、缓存随机 getter、读取 ambient `Main`/`genRand` 或暴露 `Everything.Dependencies` 的可变列表。

### P18.SkyblockWorldState

两个旧入口都进入同一个行为 owner：

1. 加载路径：`WorldFile` 完成 tile load/liquid settling 后调用 `ScanTiles`；扫描 x/y 内区间 `[40, maxTiles-40)`，active tile 计数并记录类型，所有扫描 tile 都记录 wall，然后 `Calculate`。
2. 生成更新路径：`UpdateWorld` 每 30 tick 调用一次 `CountTiles(totalX)` 并推进列号；`CountTiles` 累积 tile/wall 与 active count，仅在最后一列调用 `Calculate`。
3. `Calculate` 派生 no-content/lowTiles，清零计数和 scan arrays，再按原顺序清空 dungeon coordinates，并且只在 `lowTiles` 转变时发送 world-data message。

Query 只从一个已完成 snapshot 推导规则。一个 owner 按调用点同步执行观察、求值、状态提交和 effect port；不把 scan/query/commit 类名直接解释成独立 scheduler 节点。空尺寸输入的除零与 guard 差异须先确定有效尺寸域。

### P18.WorldGenerationCatalogs

选项、seed 规则、landmass 与 tree profile 保持不可变定义或目录。保留 key、注册/展示顺序和值语义。Tree callbacks 在生成 owner 的 tile/wall/random 输入边界运行；`LandmassData.Top` 的 value-local setter 关系不得转成隐藏全局写。

## 6. 状态与副作用规则

| 状态/效果 | 唯一写入规则 | 未决边界 |
| --- | --- | --- |
| 选项 `Enabled` 与生成旗标 | 选择 owner 更新 Enabled；一个 Reset coordinator 投影到 `Main`/`WorldGen` | event subscribers、snapshot restore、跨分区 canonical owner |
| secret seed active set/count | registry owner 保持集合与计数一致；definitions 不随 clear 删除 | process/world/session scope、save/load 与同时世界 |
| unlock text、sound、tracker | owner 在匹配成功时记录文本；sound 在新启用转换时提交；若采用完整参考，再执行 tracker/config/UI effect | tracker 是否纳入、settings/UI canonical owner、dedicated-server authority 和 SaveSettings 重入/失败语义 |
| Skyblock scan scratch/output | 同一组合 owner 提交；Query 不写入 | CountTiles 的 generation writer、跨分区 schedule 与 scan lifecycle |
| dungeon coordinates/network | 由 Skyblock commit contract 统一提交 | dungeon coordinate owner 与 network authority |
| option、variation、tree 派生值 | Query 仅接受不可变显式输入 | 具体消费者闭包、随机 draw 时间和 callback effects |

## 7. 依赖方向

| 提供者 | 消费者 | 传递内容 | 禁止关系 |
| --- | --- | --- | --- |
| Seed/config adapters | option catalog Query / secret seed resolver | 原始文本和解析结果 | Adapter 不写多个 owner |
| Registry owner | derived Queries、generation consumers | 不可变活动集合 snapshot | Query 不更改 enabled set/count |
| Seed input result | registry commit | variant、normalized plaintext、sanitized unlock text | 不丢弃旧匹配过程对 seed definition 的文本状态写入 |
| Registry commit | tracker/config/UI adapter（仅在参考版基线批准后） | 已启用 seed 与原解锁文本 | Adapter 不反向拥有 P18 active set，也不暴露 tracker 的 live list |
| option selection owner | generation reset coordinator | 选中 option snapshot | P18 与 coordinator 双写 Main/WorldGen flags |
| world tile reader / `CountTiles` path | Skyblock owner | 有序 tile/wall observation | 单次全图 scan 不能替代增量 writer |
| Skyblock Query | Skyblock commit owner | no-content / lowTiles selection | Query 不清坐标、不发网络消息 |
| catalog Definitions | UI/config/generation consumers | immutable metadata/profile values | 不返回 live mutable option/list alias |

## 8. 阻塞项与接受条件

代码设计落地前须先解决：

1. 选择及 secret-seed active set 的 canonical scope/owner，包含 reset/restore、save/load 和 P16/P17/P19/P20 的读写协作；完整参考 tracker/config/UI 是否属于兼容基线也须裁决。
2. Skyblock `CountTiles` 增量写、加载整图扫描、dungeon coordinates 和 network effect 的唯一提交契约。
3. Version4 与完整参考之间的基线差异；除 `spawnSolidifier` / `spawnShimmerPool` 外，还须对 `anySecretSeedIsActive` 及 `SecretSeedsTracker` 的持久化/UI 闭包定范围，所有额外成员/效果保持 reference-only，或经新范围批准后再纳入。
4. 当前目标候选中的 generation ID、runtime version、idempotency map 是否有实际调用协议依据；没有则不应成为额外语义。
5. event subscriber、动态读取、profile callback、异常/重试和 unload 行为仍为 `unknown`，需在依赖结论前补证。

设计接受条件：136 个输入成员全部在第 4 节有归属；无未经批准的双写；每个 Query 有显式输入和可说明的求值时点；Skyblock 两条入口均进入同一提交规则；跨分区 owner 和参考版差异仍显式开放。满足这些条件只接受 proposed 设计，不等于实现或兼容验证。
