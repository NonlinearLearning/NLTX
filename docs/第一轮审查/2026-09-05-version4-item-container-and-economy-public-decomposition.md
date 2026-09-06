# 17 — ItemContainerAndEconomy 独立只读审查与 ECS 设计提示词

本文件必须与以下公共协议共同使用，开始任务前先完整读取该文件：
D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-public-decomposition-common-protocol.md

公共协议中的只读、并行、证据优先级、行号复核、状态枚举、事实/设计隔离、ECS 文件组织、副作用隔离、报告结构和 Integration Handoff 规则全部适用于本任务。若本任务包与公共协议冲突，以公共协议为准。

本任务属于已经固定的 19 个游戏模拟子系统。不得改名、合并、拆分或扩展固定清单；如果当前全量索引出现额外候选，只能记录为 boundary-challenge，交由最终整合会话处理。

## 任务元数据

- taskNumber: 17
- subsystemId: ItemContainerAndEconomy
- layer: authoritative-simulation
- currentNltxStatus: partial
- originalBoundary: 初始物品/经济责任；Fishing、Spawn/Loot、Player、ContentCatalog 与其交接
- relatedSubsystems: ContentCatalog, PlayerGameplay, FishingAndCatchSimulation, SpawnLifecycleAndLoot, WorldStorage, CombatAndStatus
- reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-item-container-and-economy-public-decomposition.md

## 专属目标

围绕 ItemContainerAndEconomy，基于真实 Version4 代码确定权威状态、写入根、生命周期、调用方向和跨域提交边界，并提出可实现但不落地的 ECS 拆分设计。该子系统的责任是：Item 实例、库存/容器事务、制作、商店、货币和物品结果提交。

“拆分为组件”只表示 proposed design。不得创建实际组件、System、Query、Command、Adapter、Projection、测试或项目文件。

## Version4 必须深读和重新定位的专属证据

- D:\TRbackup\Version4\Terraria\Item.cs：stack/maxStack/prefix/shop 字段及 Prefix、SetDefaults、Use/消费路径，按实际行号重新定位。
- D:\TRbackup\Version4\Terraria\Player.cs：ItemCheck/物品使用、库存写入和装备结果；Chest.cs：容器/商店。
- D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:9-32：RemoteCraftRequest、待处理队列和网络模块。
- D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\*、CommonDrop.cs：声明式掉落规则；WorldFile.cs:1195、:1646 等 Chest 保存。
- 完整参考和 tModLoader Recipe/Item 文档只用于补证注册、条件和公开边界。

任务包中的路径和行号只是检索线索。必须重新读取文件、定位实际符号和调用上下文；报告使用实际行号，并标记 confirmed、partial、missing、unresolved、evidence-mismatch 或 version-drift。

## tModLoader 公开 API 交叉验证

- D:\TRbackup\tmodloader-api-docs-stable\class_recipe.html:99-154：配方注册、条件和消耗边界。
- 按索引检索 class_mod_item.html、class_global_item.html、容器/网络/存档页面；公开规则不能替代 Version4 事务证据。

不得用 tModLoader 文档替代 Version4 私有行为；必须记录实际 HTML 文件、页面标题、文档版本、成员锚点和仅用于何种边界交叉验证。

## Space Station 14 最小相关 ECS 参考

只读检索下列最小相关关键词和目录：
- inventory、container、stack、item、crafting、shop、currency、wallet
- transaction/reservation、component/system、event/command、persistence/network state

只记录实际读取的 Space Station 14 文件、类型/方法和参考用途（Component 粒度、System/Query 边界、Event/Command、关系、网络或持久化）。禁止复制代码、命名、目录结构和领域语义；若无直接对应证据，明确写无直接对应证据。

## 当前 NLTX 必须浅读核对

- 当前 NLTX src/Items 有 ItemInstance、Inventory、Container、Crafting、Commerce、Loot 等组件/目录，但原子多容器事务和执行链仍 partial。
- 必须检查现有组件是否重复拥有 stack、reservation、ownership 或货币；不得创建共享基础组件。

只能读取当前 src、Test、dome/src 和既有验证材料，不得修改；本轮并行审查不运行编译/测试命令，验证状态写为 not-run，历史真实输出只能写 existing-evidence。

## public-decomposition 专属问题

- Item definition 与 Item instance、container contents、reservation、equipment、world drop 和 currency 的状态所有权。
- 制作/购买/出售/掉落/金币损失的 Command、验证、提交、回滚和幂等边界。
- Fishing/Spawn/Loot 只提交结果请求，ItemContainerAndEconomy 是否唯一写库存/容器。
- 网络 crafting 请求、持久化容器快照和客户端 inventory Projection 如何隔离。
- Item 与 Entity、Player、Chest/TileEntity 的关系和持久化 ID 如何建模。

必须完成成员/字段/方法盘点、读者/写者/生命周期/副作用表、权威状态所有权表、访问模式分组、Component/System/Query/Command/Adapter/Projection 接口契约、System 顺序、跨域依赖、ECS ID/关系建模、不拆分项和行为保持风险。所有新类型、路径和签名均标记 status: proposed；跨子系统类型标记 crossSubsystemOwner: integration-review。

## 专属交付重点

- 提出 ItemInstance、ContainerContents、Reservation、CraftingTransaction、CommerceOffer、ItemCommitSystem、EconomyQuery、Network/Persistence Adapter 的 proposed 设计。
- 报告必须包含消费成功/失败、并发 reservation、回滚、掉落接收、金币边界和 focused verifier 计划。

报告必须写入：
D:\TRbackup\NLTX\docs\research\2026-09-05-version4-item-container-and-economy-public-decomposition.md

报告必须包含 Version4 事实、完整参考补证、tModLoader 交叉验证、Space Station 14 参考、当前 NLTX 状态、proposed 设计、not-run 验证、证据缺口、blocking-decision 和 Integration Handoff。报告只能使用中文说明与英文稳定 ID，不能宣称迁移完成、行为等价或 API 兼容。

## 明确不拆分的专属对象

- 单个 Item 字段、单个 Recipe、单个掉落规则、单个 Chest、单个金币实体、单个商店 UI。

请说明每个对象为什么只是实例、策略、规则、查询、缓存、Adapter 或 Projection，而不是一级子系统。

## 完成条件

完成只读证据审查并生成唯一指定报告后，在最终回复中简要说明报告路径、实际读取的主要证据、本次未运行构建/测试、evidence-gap、blocking-decision 以及未修改生产代码。
