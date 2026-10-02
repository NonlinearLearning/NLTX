# 权威 P18 World Seeds 与 Skyblock System 执行计划

documentKind: system-execution-plan
partitionId: P18
derivedFromDesign: docs/system-decomposition/design/2026-09-30-authoritative-P18-world-seeds-skyblock-system-design.md
derivedFromSystemReport: docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P18-world-seeds-skyblock-definitions.md
sourceReportSessionId: e4147654f9464b5193bb35f895cdc63f
executionStatus: partial-implementation
implementationStatus: partial
verificationStatus: partial
sourceModified: true
buildRun: true
testsRun: true

## 1. 执行边界

本文记录 P18 System 执行计划及一次局部实现。本轮修改 `src/NSSLC` 中的 Skyblock 候选 owner，并新增独立核心验证程序；没有修改 P18 输入台账或 runner 状态。实现与验证仅覆盖候选 API，不包含真实旧入口接线、全范围行为对照、跨分区 owner 评审或完整测试，因此不表示行为等价、API 兼容或迁移成功。

`sourceReportSessionId` 只标记已结算静态报告的 provenance，不是本文的 claim/session，也不能用于重新结算。任何实现须另行获得实施授权，并重新确认适用的分区范围、集成 owner 和构建/验证约束。

## 2. 基线与当前目标路径

完整 136 成员范围和 Version4 源码 hash 见[设计文档](../design/2026-09-30-authoritative-P18-world-seeds-skyblock-system-design.md)及[静态报告](../reports/2026-09-18-system-decomposition-authoritative-P18-world-seeds-skyblock-definitions.md)。CPG 查询基于只读 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；索引没有源码 snapshot ID，静态 `complete` 结果只覆盖选定 shard。

目标候选集中在 `src/NSSLC/Component/WorldSession/WorldGeneration`：

| 已有候选 | 当前可确认的范围 | 计划约束 |
| --- | --- | --- |
| `WorldSecretSeedRuntimeRegistryComponent` / `WorldSecretSeedRuntimeRegistryCommitSystem` | registry set/count 及 commit API 声明存在 | generation/version/idempotency 协议超出已确认旧行为；先证明 caller 需要，再决定保留 |
| secret seed derived Queries 与 option catalog Query | 显式输入和 result 类型存在 | 仍须对照 Version4 每项公式、短路、文化规则和调用时点；无接线证据 |
| `WorldSkyblockGenerationScanSystem` / rules Query / commit System | 已增加整图扫描与逐列累计的同步求值/提交组合；owner 保存上次派生规则，effect 次序集中提交 | 尚未确认真实 load/generation caller；零尺寸除法与 Query guard 有差异；tile reader 的 null 初始化和生产接线仍为 `unknown` |
| `WorldGenerationSecretSeedFlagsComponent` | 旗标 Component 候选存在 | canonical writer、世界身份和 restore/save owner 仍为 `integration-review` |
| `WorldFile` / `WorldGen.Reset` / `UpdateWorld` 接线 | 本次搜索未在目标树确认对应生产入口及候选 API 的调用 | 接入类、调用点及构建项目边界须在后续授权实现前重新搜索；本文不臆造文件路径 |

完整参考 `D:\TRbackup\无任何删减通过编译` 已用于核对 P18 相关秘密种子、配置/UI、Star 消费者和 Skyblock generation 源码；这不是完整工程的全仓差异审计，也不自动替代 Version4 的 P18 authoritative 输入。参考版的 `SecretSeedsTracker.AddSeedToTrack`、`SecretSeed.anySecretSeedIsActive`、`Skyblock.spawnSolidifier` 和 `spawnShimmerPool` 不在 136 成员清单内。若选择完整参考作为额外兼容基线，先更新范围并由 integration review 指定 settings/UI 与跨分区旗标 owner。用户提供的编译通过状态没有在本文任务中重跑。

此次静态复核还确认：Version4 与完整参考的匹配成功都会改写 `_plaintext` 和 `TextThatWasUsedToUnlock`；完整参考仅在启用状态转换时、且非 dedicated server 时登记 tracker，之后才可选播放声音。当前候选 match 结果带有文本，但 `EnableSecretSeedCommand` 只含 generation/version、variant 和 idempotency 字段，registry commit 没有接收匹配文本；`MatchAndPlaySound` 则以“匹配成功”而不是“新启用转换”为声音门槛。这是 S02 的未解决行为差异，当前 `implementationStatus` 不因此升级。

## 3. Gate 与阶段

| Gate | 工作 | 进入条件 | 退出条件 | 当前状态 |
| --- | --- | --- | --- | --- |
| G0 来源与范围冻结 | 固定 Version4、完整参考、CPG manifest、NSSLC 当前树的身份；决定完整参考差异是否在范围内 | 开始任何实现前 | 每个被迁移行为有选定来源；reference-only 差异登记完成 | 部分完成；差异需裁决 |
| G1 owner / 生命周期评审 | 确定 option flags、secret seed set、Skyblock incremental state 的唯一 writer 与作用域 | G0 记录可供跨域评审 | Reset/restore、load/unload、save/network 及 P16/P17/P19/P20 owner 契约明确 | 未完成；阻塞依赖这些契约的 writer |
| G2 行为 oracle | 从实际 Version4 入口建立输入、delta、效果、顺序和错误观察 | 对应路径有稳定来源和 owner 决定 | 场景能命中旧入口，并可识别是否调用候选新 API | 未运行 |
| G3 API / 接线设计 | 冻结 Definition、Query、commit API 与真实 target caller 的组合 | G0/G1 对应部分完成，G2 覆盖需要迁移的行为 | 无双写；两个 Skyblock 入口和 reset/restore 都有明确接入计划 | 未开始 |
| G4 分片实现 | 在 `src/NSSLC` 接入选定 slice，并保持旧入口可回退 | 对应 G3 通过且有实现授权 | 真实入口命中新 owner；副作用和顺序符合冻结契约 | 部分；仅完成 Skyblock 候选 owner 的局部 API，真实调用方未接入 |
| G5 行为验收 | 跑定向和跨分区验证 | G4 路径完整接线 | 必需观察向量通过且验证记录可复核 | 局部核心验证通过；旧入口对照与跨分区验证未运行 |
| G6 旧路径移除 | 清理兼容 facade/重复状态 | G5 通过，持久化/网络/恢复读者均有替代证据 | 无旧 caller/serializer 绕过 owner 的证据 | 禁止提前执行 |

Gate 不代表项目已通过，也不授权运行任何编译或测试操作。一个缺口只阻塞依赖它的 slice；不得以局部通过缩小完整行为验收范围。

## 4. 实施分片

| Slice | 范围与顺序 | 前置条件 | 完成判据 | 回退条件 |
| --- | --- | --- | --- | --- |
| P18-S00 来源冻结 | 复核 12 组成员、Version4 hash、CPG manifest 和完整参考差异；确定哪个源码集合是 oracle | 无 | 每项差异被标为 target、reference-only、approved change 或 unknown | 基线不清或把 reference-only 事实混作 Version4 时停止 |
| P18-S01 定义与 option catalog | 保留 option/secret seed/tree/landmass 静态注册顺序和 key；用 immutable Definition/Query 暴露只读值 | S00；选项事件和 selection owner 已指定 | catalog 不返回 live mutable alias；seed text/config 的匹配、默认、未匹配结果有来源对照 | 次序、匹配或 lazy/callback 语义改变 |
| P18-S02 secret seed 输入与 active set | 适配文本输入；单 owner 提交匹配文本、enable/disable/clear；从权威集合导出 active-count；隔离 sound/unlock-text effect；获准完整参考基线时另接 tracker/config/UI adapter | S00、G1 secret-seed scope；S01 definitions；tracker/config/UI 纳入范围时需 integration owner | 空白/未匹配、plaintext/code、culture、匹配文本写入、重复启停、clear-without-unregister、dedicated server、声音门槛及 tracker 保存顺序符合已选 oracle | 匹配结果丢文本、重复 enable 重播声音/重复保存、server authority 或 world scope 不明 |
| P18-S03 derived option/variation | 逐属性映射到显式 seed/world/registry snapshot；把随机输入安排在原调用点 | S00、Query purity 和 RNG contract 已确定 | 结果、短路、重复调用观察及 RNG 次数/顺序一致 | ambient state、cache 或一次额外随机抽取改变行为 |
| P18-S04 selection/reset projection | 从选择状态接入唯一 Reset coordinator；覆盖 `WorldGenerator.TryReset` 和 `WorldGenSnapshot.Restore` 所确认入口 | G1 flag owner、save/restore contract、P16/P17/P19/P20 review | Enabled -> Main/WorldGen flags -> secret-seed flags -> `Main.rand` reset 的顺序保持，且无并行 writer | restore、选项事件或 random state 顺序差异 |
| P18-S05 load-time Skyblock scan | 将 `WorldFile` 加载稳定点接入同步观察 -> Query -> 单 owner commit；保持内区间和 wall observation 语义 | G1 Skyblock owner/effects；WorldFile 真实 target adapter 已定位 | 加载/settling 后扫描一次；tile/wall 分类、scratch reset、坐标与网络效果顺序符合 oracle | 重复提交、扫描时点移动、坐标或消息次数变化 |
| P18-S06 generation-time Skyblock accumulation | 将 `UpdateWorld -> CountTiles(totalX)` 的逐列 tile/wall 累积及末列 `Calculate` 接入同一 owner；不以 S05 全图扫描替代 | G1 对 P19/P20 writer 和 phase 的评审；S05 accumulator/commit contract | 30 tick 推进、列号 wrap、末列触发、跨列累计和 Calculate reset 顺序一致 | 全图扫描覆盖增量值、丢失墙/tile、提前/重复 Calculate |
| P18-S07 catalog profiles 与阶段整合 | 将 tree/landmass 值接到真实 generation consumers；按既有入口逐段切换并保持兼容回退 | S01、生成 callback 输入及 P19/P20 consumers 已确认 | 定义值、profile membership、callback 输入及 tree random draw 有逐项对照 | 改变 profile 顺序、callback 点或随机流 |
| P18-S08 行为验收与清理 | 对每个切片先验证新 owner 命中，再覆盖跨分区 save/load、network、restore；旧 API 最后移除 | G4 所有选定路径已接线，G5 通过 | 必需行为场景通过；无 serializer/动态 reader 绕过 owner | 任何未知 caller/effect 或未通过观察向量时保留旧路径 |

分片顺序只表示实现依赖，不是 ECS scheduler 排序。S02/S03 等可在 owner 已明确且输入不冲突时独立推进；S04/S05/S06 的单一 writer 与提交顺序不可拆开验收。本次代码只补充 S05/S06 的候选 API；未完成 world load、30-tick `UpdateWorld`/`CountTiles` 调用接入，故这两个 slice 仍未达到完成判据。

## 5. 逐阶段执行约束

### A. 固定源码关系与版本差异

1. 用目标 Version4 的实际 declaration、callsite、读写点和副作用冻结行为；保留 CPG query path、`Status`、`Gaps` 和 manifest。
2. 已确认的直接关系包括 `Reset <- TryReset/Restore`、`ScanTiles <- WorldFile`、`Calculate <- ScanTiles/CountTiles`、`CountTiles <- UpdateWorld`。静态边仅限定在被扫描 shard；事件、调度、反射或其他入口保持 `unknown`。
3. 对照完整参考时单列 reference-only 行为。tracker 持久化/UI、`anySecretSeedIsActive` 的 Star 读取者及 `spawnSolidifier` / `spawnShimmerPool` 需先决定是否更新目标行为/成员范围；不能由版本目录名或“通过编译”代替语义审批。
4. 复核当前目标候选的所有 caller 和 project inclusion。若只存在声明，先找到可接入的实际入口，再实现组合；禁止仅增加更多 System 文件。

### B. 冻结 API 和观察向量

每个 `ConceptId` 记录相同 seed/world 输入下的返回值或错误、权威状态 delta、event/sound/tracker/network/save 效果及次数、求值/提交顺序、可见性、world/session/generation scope、重复和失败语义。匹配成功会写 plaintext 与 sanitized unlock text，不是纯 Query；Query 只接受显式快照；`Enabled` setter、registry commit 和 Skyblock effects 均属于修改职责。

在完成 caller 协议核对前，不把候选 registry 的 generation ID、runtime version、idempotency key 作为兼容要求；也不预设必须引入额外 command DTO、队列、event bus、barrier 或事务层。

### C. 逐 owner 接线

1. 先选定各 invariant 的唯一权威 writer。选项和生成旗标由 integration review 定 owner；秘密种子集合不能同时由旧 static set 和新 Component 双写。
2. secret seed input adapter 负责解析并返回完整匹配输入，不自行启停、改 Main flag、播放声音或更新 tracker。registry commit 必须保留成功匹配的 normalized plaintext 与 unlock text；声音只在 enabled 状态从 false 变 true 时发出。若批准完整参考基线，tracker adapter 必须在状态提交后、可选声音前按非 dedicated-server 条件执行；其 config/UI owner 由 integration review 指定。
3. 先让派生 Query 在旧决策点命中，再替换读取方；不批量预热 random/lazy getters。
4. Skyblock 将整图扫描和逐列增量输入送到同一 accumulator/commit contract。完成 `Calculate` 后重置累计状态、更新 `noDungeon` 坐标、按 transition 规则发消息。
5. 保留旧入口作为有边界的适配层，直到实际业务 caller、world load、reset/restore 和保存路径均已命中新组合。不可依赖文件顺序或类名假设 scheduler order。

### D. 生命周期、持久化和副作用

先确认多 world/session 和 unload/reload 语义，再决定 registry 与 scan state 是 Component 还是调用期 snapshot。恢复旧世界时不从新进程默认值推断持久化规则。网络/server authority、sound/tracker client gate、序列化 seed text 和重复加载各自留在既有 adapter/effect boundary；缺少实现证据的失败/重试行为标 `unknown`。

## 6. 后续行为验收矩阵

| Slice | 关键用例 | 比较观察 |
| --- | --- | --- |
| 选项与 Reset | 默认、匹配/不匹配、`Reset` 后选择、AutoGen 与 Enabled 分离、TryReset、snapshot restore | flags、事件次数、随机初始化时点和 generation state |
| secret seed 输入/启停 | 空白、标点/大小写及文化、plaintext、变换 code、未匹配、文本状态回写、重复匹配/enable/disable、clear 后重新启用、`playSound=false`、dedicated server、tracker config round-trip | result、normalized plaintext、unlock text、active count、sound/tracker/save 次数和顺序、UI 列表顺序/去重、definition 是否仍注册 |
| derived Queries | 固定 flag/seed/context、short-circuit 分支、需要随机输入的分支、重复调用 | 每个属性结果、随机次数与顺序、异常和 ambient state 读取 |
| Skyblock load scan | 40 边界、active tile/wall 组合、dungeon/temple wall、低于/等于/高于 10%、零/非法 world dimension | presence set、no-content flags、lowTiles、数组/计数清理、坐标清理及 message 7 次数 |
| Skyblock generation path | `UpdateWorld` 30 tick 门槛、列递增/wrap、`X == maxTilesX - 1`、扫描中途状态 | 列累计和 load scan 一致性、Calculate 次数、reset 时间和跨分区观察 |
| Catalog/profile | option registration/display order、Everything dependency、landmass Top/Position 往返、tree profile/delegate/random 输入 | keys/values/顺序、alias mutation 和 tree decision/draw 语义 |
| 保存与恢复 | seed text round-trip、反复加载、生成 snapshot restore、重复请求 | serialized value、flags/registry scope 和重复外部 effect |

除下节明确列出的四个局部核心场景外，矩阵用例仍为未来计划。真实行为测试必须命中新 owner 和真实新组合；仅编译、类型存在、静态映射或局部 verifier 通过不能升级为 `migration-success`。目标零尺寸处理与完整参考额外行为应按最终批准的 source baseline 分别建用例，不混作默认行为。

## 7. 核心验证记录（2026-10-01）

只运行独立的 `Test/Terraria.WorldSession.WorldGeneration.P18.CoreVerification`，覆盖约 10% 的核心风险场景：非 Skyblock 世界仍计算 no-content flags、加载扫描内区间边界、带显式 `WorldSurfaceY` 的逐列累计至末列并提交、跳过首列时拒绝操作、末列前拒绝提交。验证没有调用 Version4/完整参考的运行时代码，也不是新旧行为对照。

通过仓库串行 runner 执行：

```powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
$dotnetArguments = @(
  'run', '--project', 'D:\TRbackup\NLTX\Test\Terraria.WorldSession.WorldGeneration.P18.CoreVerification\Terraria.WorldSession.WorldGeneration.P18.CoreVerification.csproj',
  '--configuration', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

最终运行退出码 `0`，输出 `PASS: P18 core Skyblock rules, load scan, incremental scan, and commit boundary`，警告 `0`、错误 `0`。编译产物位于 `Build/bin/Terraria.WorldSession.WorldGeneration.P18.CoreVerification/Release/net10.0/`。未运行完整解决方案构建、旧路径集成测试、跨分区测试或其他 P18 用例。

## 8. 停工条件与验收

出现以下任一条件时停止依赖该项的 implementation slice，并保留旧路径：owner 未定、Version4/reference 基线混淆、`CountTiles` writer 未接入、目标 API 未被实际 caller 使用、Query 隐藏访问 ambient state、存在双写、save/network authority 未闭合，或 behavior test 未通过。

只有所有纳入范围的真实 caller 都调用新组合、观察向量通过且 integration owner 签收后，才可把对应行为状态升级。P18 全分区完成还需 136 个输入成员逐项追踪；旧路径移除是独立 gate，不从本执行计划或报告结算自动推出。

本执行记录使用只读 CPG Query API、Version4 源码、用户指定的完整参考源码和 SS14 结构参考；历史核心验证仅为上文所列范围。本轮补充完整参考调用关系和设计/执行文档时只读源码并使用 CPG Query API，没有修改生产源码，也没有运行 build/test。真实生产接线、owner 生命周期、zero-dimension 语义、完整参考基线裁决和全范围行为仍有缺口；状态保持 `implementationStatus: partial`、`verificationStatus: partial`，不称迁移成功。
