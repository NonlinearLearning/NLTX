# P17 世界地形、生物群系与 GenVars 结构 - public-decomposition 分区专属提示词

> 本文件只包含当前分区的专属任务内容，不复制公共提示词正文。
> 使用时：先提供公共提示词，再附加本文件；公共协议中的通用规则、证据门槛、报告结构、禁止项和交接格式继续有效。

## 公共前置（仅引用）

- 公共提示词：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md`
- 必读技能：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
- ECS 证据协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
- tModLoader 检索协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
- 本文件不重复上述文件内容；执行前必须实际读取并遵守它们。
- 本任务包覆盖权威报告的 P01-P20 共 20 个并行分区；本文件的 `currentTaskCount: 20`、分区范围和唯一输出路径优先于公共协议中遗留的 19 会话数量说明。

## 任务元数据

- promptId: P17-public-decomposition-20260911
- partitionId: P17
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers/Version4权威模拟系统字段属性逐成员源码声明-20分区\P17-World-Terrain-Biomes-GenVars.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-P17-world-terrain-biomes-genvars-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: WorldGenerationAndEcology
- leafSubsystemCount: 14
- fieldCount: 143
- propertyCount: 3
- memberCount: 146

## 专属范围锁定

本会话只审查输入分区报告中的 14 个叶子子系统。以下清单是本会话唯一的成员范围；不得读取或复制作其他 P 分区的成员清单：
- `GenVarsConfigurationAndOreState`
- `GenVarsWorldLayerMetrics`
- `GenVarsSurfaceAndBiomeState`
- `GenVarsBeachAndOceanBoundaryState`
- `WorldGenBeachAndOceanBiomeState`
- `WorldGenUndergroundDesertStructureState`
- `WorldGenJungleStructureState`
- `GenVarsDungeonAndIslands`
- `GenVarsCaveTunnelAndOrePatchState`
- `GenVarsMushroomBiomeAndLogState`
- `GenVarsLakeAndOasisState`
- `GenVarsHellAndSpecialStructures`
- `GenVarsDungeonDerivedProperties`
- `WorldSavedOreTierState`

本分区保留地形/生态事实与定义的边界；生成执行、动作载荷和秘密种子策略由 P18-P20 交接。

跨分区发现必须在报告中写成 `cross-subsystem finding`、`integration-risk` 或 `crossSubsystemOwner: integration-review`，不得把相邻分区成员重新纳入本分区。

## 分区专属目标

围绕“世界地形、生物群系与 GenVars 结构”完成独立只读的 Version4 成员审查和 ECS 拆分设计。先读取输入分区报告中的完整成员表，再回到 Version4 实际源码核对关键字段、属性、方法、初始化路径、读写者、写入者、生命周期和副作用；所有结论必须区分源码事实、当前 NLTX 状态、proposed 设计和验证结果。

本任务的输出是研究报告，不是运行时实现；报告必须保持公共提示词规定的英文稳定 ID、证据状态、Integration Handoff 和未完成声明。

## 专属证据焦点

- D:\TRbackup\Version4\Terraria\WorldGen.cs：地层、海洋、沙漠、丛林、洞穴、地牢、地狱、湖泊和结构生成状态。
- D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs 或实际声明文件：配置、矿石、层级、Biome 和结构变量。
- Terraria.GameContent.Biomes\*、WorldBuilding\*、Tile/Wall/Liquid：只读到确认写集和定义边界。
- WorldFile.cs、WorldSession、WorldGeneration execution 和 WorldSeed option 的恢复/配置交接。

如果专属焦点中的路径、类型或历史行号发生漂移，必须按公共协议重新定位并记录 `version-drift` 或 `evidence-mismatch`，不得静默沿用旧行号。

## 专属审查问题

- GenVars 配置、层级、Biome、海岸边界、结构和矿石状态的共同读写者与生命周期。
- 地形/生态事实、定义 profile、生成 scratch、派生指标和持久化矿石层级如何区分。
- Biome 规则是 Definition/Query 还是 World 权威状态；结构规划如何提交 Tile/Wall/Liquid 变更。
- WorldGen 与 WorldStorage、Liquid、NPC spawn、SecretSeed、GenerationExecution 的依赖方向。
- 随机种子确定性、生成失败/重跑、版本迁移、恢复和大范围 Tile 写入的 focused verifier。

## 专属不拆分边界

- 单个 Biome、单个 Tile、单个矿石 tier、单个结构 profile、单个 GenVars 临时变量。
- 不要把所有 GenVars 字段按源码文件机械复制成单一 GenVarsComponent。

## 输出要求

将完整研究报告只写入：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-P17-world-terrain-biomes-genvars-public-decomposition.md`
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、组件/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 `verificationStatus: not-run`。
- 完成前检查：本报告只包含本分区组名；没有复制其他 P 分区成员；没有把 proposed 类型写成已存在实现；没有宣布跨分区 owner。

## 会话执行结束条件

输出文件存在且只由本会话写入后，在最终消息中报告：P17、输出路径、实际读取的 Version4 证据、关键 evidence-gap、blocking-decision、erificationStatus，以及未修改生产代码。
