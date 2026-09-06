# 09 — SpatialSimulation 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议出现冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 09
- subsystemId: SpatialSimulation
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 初始权威空间模拟；LiquidSimulation 从其中拆出
- relatedSubsystems: PlayerGameplay, NpcAndTownSimulation, ProjectileSimulation, WorldInteractionAndStructures, LiquidSimulation, WorldStorage
- reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-spatial-simulation-public-decomposition.md

## 专属目标

围绕 SpatialSimulation，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：确定性移动、碰撞、空间资格查询和液体接触查询。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\Collision.cs：CanHit/CanHitWithCheck（约 :197-316）、WetCollision（:1001）、LavaCollision（:1085）、SolidCollision（:2027）、StepDown（:2736）及 TileCollision 调用。
- Collision.cs 与 Player.cs、NPC.cs、Projectile.cs、Item.cs 的移动、受伤、液体接触和碰撞调用点。
- D:\TRbackup\Version4\Terraria\Tile.cs、WorldGen.cs：Tile 几何/可碰撞状态及结构变更读取。
- Main.cs 世界 Tick 中玩家、NPC、Projectile 空间更新顺序；Liquid.cs 仅作为液体权威来源交叉引用。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_player.html：公开玩家空间/碰撞表面（必须以实际索引锚点为准）。
- 按索引检索 ModPlayer/ModNPC/ModProjectile 的移动、碰撞和 Tile 查询成员；API 只作边界交叉验证。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- physics、collision、movement、transform、fixture、grid/map query
- query system、component access、contact event、spatial partition

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX 有 src/Physics、src/Share/Entity/Components 和部分 dome 空间组件，状态模型存在但执行和 Query 契约不闭合。
- 必须将已有 LiquidComponent 作为跨边界证据审查，不得重新吞并 LiquidSimulation。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- 哪些是纯 Collision/qualification Query，哪些是拥有移动状态并提交位置的 System。
- Position、Velocity、Collider、MotionHistory、LiquidContact 和 TileCoordinate 是否应分离；写者如何唯一化。
- WorldStorage 提供只读 Tile 视图还是 SpatialSimulation 直接操作 Tile；结构变更必须走 WorldInteraction。
- 如何表达实体引用、空间/section、坐标和派生接触结果，不把缓存当权威事实。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 MovementState、CollisionShape、SpatialReference、ContactQuery、MovementSystem、CollisionCommand/Commit Port 的 proposed 设计。
- 报告必须区分液体接触 Query 与 LiquidSimulation 写集，并设计纯查询、边界碰撞、移动提交和副作用隔离 verifier。

报告必须写入：
D:\TRbackup\NLTX\docs\research\2026-09-05-version4-spatial-simulation-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个碰撞轴、单个 AABB 查询、单个 Collider 字段、LiquidRenderer、SceneMetrics/屏幕缓存。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
