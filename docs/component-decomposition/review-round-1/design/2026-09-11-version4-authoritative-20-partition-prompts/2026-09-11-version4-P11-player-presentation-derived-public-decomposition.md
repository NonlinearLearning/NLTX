# P11 玩家表现、动画、外观与派生属性 - public-decomposition 分区专属提示词

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

- promptId: P11-public-decomposition-20260911
- partitionId: P11
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers/Version4权威模拟系统字段属性逐成员源码声明-20分区\P11-Player-Presentation-Derived.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-P11-player-presentation-derived-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: PlayerGameplay
- leafSubsystemCount: 18
- fieldCount: 91
- propertyCount: 73
- memberCount: 164

## 专属范围锁定

本会话只审查输入分区报告中的 18 个叶子子系统。以下清单是本会话唯一的成员范围；不得读取或复制作其他 P 分区的成员清单：
- `PlayerPoseAndAnimationState`
- `PlayerNetworkCameraState`
- `PlayerShadowAndArmPresentation`
- `PlayerVisualAndShaderEffects`
- `PlayerFootballPresentationState`
- `PlayerAppearanceCustomizationState`
- `PlayerTraversalColorProjection`
- `PlayerAppearanceCompanionAndEffectProjection`
- `PlayerSpatialDerivedProperties`
- `PlayerIdentityAndDerivedProperties`
- `PlayerBiomeZoneProperties`
- `PlayerVerticalAndWeatherZoneProperties`
- `PlayerEventAndShoppingZoneProperties`
- `PlayerInteractionAndSelectionProperties`
- `PlayerAbilityAndPresentationProperties`
- `PlayerItemMountAndRuntimeProperties`
- `PlayerEyeAnimationState`
- `PlayerPresentationMessagesAndArms`

本分区的输出多为 Projection/Query；与 Player 权威状态、网络协议、客户端渲染和环境派生的 owner 需要 integration-review。

跨分区发现必须在报告中写成 `cross-subsystem finding`、`integration-risk` 或 `crossSubsystemOwner: integration-review`，不得把相邻分区成员重新纳入本分区。

## 分区专属目标

围绕“玩家表现、动画、外观与派生属性”完成独立只读的 Version4 成员审查和 ECS 拆分设计。先读取输入分区报告中的完整成员表，再回到 Version4 实际源码核对关键字段、属性、方法、初始化路径、读写者、写入者、生命周期和副作用；所有结论必须区分源码事实、当前 NLTX 状态、proposed 设计和验证结果。

本任务的输出是研究报告，不是运行时实现；报告必须保持公共提示词规定的英文稳定 ID、证据状态、Integration Handoff 和未完成声明。

## 专属证据焦点

- D:\TRbackup\Version4\Terraria\Player.cs：Pose、Animation、Shadow/Arm、外观、颜色、眼睛、坐姿/睡眠及派生属性。
- Version4 中 PlayerDrawLayers、Main、Lighting、SceneMetrics、UI/消息相关类型：确定表现读取方向。
- Player.cs 的 Biome/Zone、Ability/Presentation、Spatial/Identity/Item/Mount 属性：区分派生 Query 与权威字段。
- MessageBuffer.cs、NetMessage.cs、客户端同步字段：确定表现、网络快照和模拟状态边界。

如果专属焦点中的路径、类型或历史行号发生漂移，必须按公共协议重新定位并记录 `version-drift` 或 `evidence-mismatch`，不得静默沿用旧行号。

## 专属审查问题

- 哪些属性可以是纯派生 Query，哪些字段只是缓存，哪些属于客户端 Projection 或兼容字段。
- Pose、Animation、外观颜色、相机、Shadow/Arm 和消息输出是否具有独立生命周期。
- Biome/Zone、Spatial、Ability、Item/Mount runtime 属性的读取者、失效条件和缓存 owner。
- 表现状态是否可能反向影响模拟；若有反向输入必须转为显式意图或命令。
- 投影稳定性、网络快照、客户端重建、缓存失效和无渲染环境下的 focused verifier。

## 专属不拆分边界

- 单个动画帧、单个颜色值、单个 UI 消息、单个眼睛状态、单个派生属性和单个相机字段。
- 不要把表现字段重新放回 Player 权威状态组件，也不要把派生 Query 缓存默认持久化。

## 输出要求

将完整研究报告只写入：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-P11-player-presentation-derived-public-decomposition.md`
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、组件/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 `verificationStatus: not-run`。
- 完成前检查：本报告只包含本分区组名；没有复制其他 P 分区成员；没有把 proposed 类型写成已存在实现；没有宣布跨分区 owner。

## 会话执行结束条件

输出文件存在且只由本会话写入后，在最终消息中报告：P11、输出路径、实际读取的 Version4 证据、关键 evidence-gap、blocking-decision、erificationStatus，以及未修改生产代码。
