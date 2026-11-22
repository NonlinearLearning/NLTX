# 14 — TeleportationAndTraversal 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 14
- subsystemId: TeleportationAndTraversal
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 从 PlayerGameplay、SpatialSimulation、WorldStorage 交界提升
- relatedSubsystems: SpatialSimulation, WorldStorage, PlayerGameplay, NpcAndTownSimulation, WorldSession, ExternalBoundaries
- reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-teleportation-and-traversal-public-decomposition.md

## 专属目标

围绕 TeleportationAndTraversal，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：旅行端点资格、目的地选择、位置迁移、冷却和旅行结果事实。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:25-90：Update、端点列表刷新、差异广播和玩家加入同步。
- 如存在：D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:64-107 UpdatePortalPoints、:109-226 TryGoingThroughPortals、:293-339 SyncPortalSections；必须实际确认路径和行号。
- D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETeleportationPylon.cs：TileEntity 宿主、损坏清理和结构交接。
- D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetTeleportPylonModule.cs：协议 Adapter；Main.cs:3364、:13109-13119：初始化和 Tick 更新。
- Player.cs、NPC.cs、Collision.cs：旅行资格、落点和位置/冷却写入调用点。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_mod_pylon.html:700-714、:1308-1339、:1353-1379、:1504-1539：有序资格、NPC/危险/biome 边界。
- class_mod_tile_entity.html:538-540：宿主 Tile 校验和删除清理；网络页面只作 Adapter 交叉验证。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- teleport、portal、door/traversal、map coordinates
- eligibility query、transactional movement、cooldown component、network snapshot

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 有 src/Teleportation/PortalNetworkState.cs、TeleportCooldownState.cs 和 src/WorldStorage/PylonRegistryState.cs，但无请求、资格、落点、提交或复制闭环。
- 已有状态记录不能直接标为 confirmed；必须检查 Test 中是否有真实行为验证。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- Travel intent、资格 Query、落点 Query、位置/速度迁移、冷却和端点注册表的 owner。
- Pylon/Portal TileEntity 归 WorldStorage/WorldInteraction，Travel 只读已提交端点。
- 失败时不能部分提交位置或冷却；网络和客户端只消费旅行结果快照。
- PlayerEntityId、NpcEntityId、WorldSectionId、NetworkId 和 portal/pylon identity 的共享 owner。
- 门户配对更新、晶塔列表刷新和实体旅行是否应由不同 System 处理。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 TravelIntent、EndpointSnapshot、TravelEligibilityQuery、LandingQuery、TravelCommitCommand/System、CooldownState、ReplicationProjection 的 proposed 设计。
- 报告必须给出 Portal/Pylon 两种路径、资格顺序、失败/重试/幂等和 focused verifier。

报告必须写入：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-teleportation-and-traversal-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 Pylon、单个 Portal、单个冷却字段、单个 ValidTeleportCheck Hook、单个 NetTeleportPylonModule、视觉粒子。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
