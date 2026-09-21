# SpatialSimulation 子系统边界与 ECS 拆分研究报告

## 报告元数据

~~~text
reportId: version4-spatial-simulation-public-decomposition-2026-09-05
subsystemId: SpatialSimulation
taskNumber: 09
layer: authoritative-simulation
currentNltxStatus: partial
originalBoundary: 初始权威空间模拟；LiquidSimulation 从其中拆出
relatedSubsystems: PlayerGameplay, NpcAndTownSimulation, ProjectileSimulation, WorldInteractionAndStructures, LiquidSimulation, WorldStorage
evidenceStatus: partial
verificationStatus: not-run
sourcePrompt: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-spatial-simulation-public-decomposition.md
commonProtocol: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md
reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-spatial-simulation-public-decomposition.md
~~~

本报告是只读证据审查和设计提示，不创建或落地任何组件、System、Query、Command、Adapter、Projection、测试或项目文件。报告中的所有新类型、新路径和新签名都明确标记 status: proposed；跨子系统共享类型都明确标记 crossSubsystemOwner: integration-review。

## 1. 执行摘要

Version4 中不存在一个独立的统一空间物理 System。空间移动和碰撞分散在 Player、NPC、Projectile、Item 的更新过程中，由 Collision 提供一组静态碰撞、斜坡、台阶、平台、传送带、液体接触和空间资格方法。Main.DoUpdateInWorld 再以 Player、NPC、Projectile、Item、LeashedEntity、世界更新的顺序驱动这些调用。

基于 Version4 的真实代码，本报告确认 SpatialSimulation 的权威责任应收敛为以下四类能力：

- 确定性移动所需的空间解析，包括 Tile 碰撞、斜坡、平台、台阶和传送带规则；
- 碰撞接触和轴阻挡结果；
- CanHit、CanHitWithCheck、CanHitLine、SolidCollision、IsClearSpot 等空间资格查询；
- 读取 LiquidSimulation 提供的液体接触结果。

以下能力不属于 SpatialSimulation 的权威写集：液体数量、液体类型、液体传播、液体合并、液体提交、液体网络/存档写集，以及 Tile 结构变更。液体权威状态属于 LiquidSimulation，Tile 和结构写入属于 WorldStorage、WorldInteractionAndStructures 或相应的集成边界。

当前 NLTX 已存在一组空间和液体相关状态类型，也已存在部分 Dome 碰撞执行路径，但执行契约、只读 Tile 视图、唯一位置写者、液体接触边界和跨项目坐标类型尚未闭合。因此当前状态为 partial，不能据此宣称迁移完成、行为等价或 API 兼容。

本报告的核心设计方向是：将持续移动状态、碰撞形状、空间引用、运动历史、液体接触和碰撞策略分离；将 Collision 改造成读取显式输入并返回显式结果的 Query；将位置和速度提交放到明确的唯一写入边界；将静态隐藏状态、空 Tile 自动创建和 Tile 结构写入从空间计算中隔离出来。

## 2. 子系统范围和不负责内容

SpatialSimulation 是固定的 19 个权威游戏模拟子系统之一。本报告不改名、不合并、不拆分、不扩展该固定清单；若全量整合发现额外候选，只能记录为 boundary challenge 并交由 Integration Review 处理。

### 2.1 负责内容

| 能力 | SpatialSimulation 的边界 | 证据状态 |
|---|---|---|
| 确定性移动 | 读取实体移动状态、形状、策略和只读世界视图，计算候选位置和速度的空间解析结果 | confirmed；当前 NLTX 为 partial |
| Tile 碰撞 | 处理实体与固体 Tile、半砖、斜坡、平台、台阶和传送带的几何接触规则 | confirmed；当前 NLTX 为 partial |
| 空间资格 | 提供线段、AABB、Tile 路径和实体之间的可命中、可穿过、可站立或空间清空判断 | confirmed |
| 液体接触查询 | 消费 LiquidSimulation 的只读接触快照，形成实体侧的接触派生结果 | confirmed boundary；实现接入为 partial |
| 碰撞派生结果 | 返回阻挡轴、法线、接触 Tile、台阶结果等本 tick 派生数据 | proposed，status: proposed |

### 2.2 不负责内容

| 不负责内容 | 权威边界 | 说明 |
|---|---|---|
| 液体数量、类型和传播 | LiquidSimulation、WorldStorage | Tile.liquid 和液体类型是世界事实；SpatialSimulation 只能读取接触视图 |
| 液体合并、删除、buffer 和网络脏集 | LiquidSimulation | 由 Liquid.UpdateLiquid、AddWater 和 LiquidBuffer 维护 |
| Tile 类型、墙、激活、斜坡和半砖的结构变更 | WorldInteractionAndStructures、WorldStorage | 空间计算不得直接调用 Tile setter 或直接提交结构命令 |
| Player 输入、装备、坐骑、绳索和平台策略来源 | PlayerGameplay | SpatialSimulation 只接受已经形成的碰撞策略输入 |
| NPC AI、平台策略来源和 NPC 专用碰撞后行为 | NpcAndTownSimulation | NPC 域提供策略，空间域计算通用几何结果 |
| Projectile 生命周期、轨迹策略和 Tile 碰撞回调 | ProjectileSimulation | SpatialSimulation 不拥有 tileCollide、ignoreWater、correctSlopeCollision 等 Projectile 行为策略 |
| 伤害效果、受伤状态和 Tile 伤害提交 | 对应实体域、WorldInteractionAndStructures | AnyHurtingTiles、HurtTiles、CanTileHurt 可以提供资格或接触信息，但不拥有伤害效果提交 |
| 网络和客户端投影 | 网络/客户端集成层 | SpatialSimulation 只产生可投影的权威结果，不反向接受客户端投影作为权威状态 |

## 3. 证据优先级和来源角色

证据采用以下优先级，事实和设计严格分开：

1. D:\TRbackup\Version4 中实际存在的代码是 Version4 行为和写入关系的主要依据。报告中的 confirmed 事实只依据该目录中的实际符号和调用上下文。
2. D:\TRbackup\无任何删减通过编译 是完整参考源码，只用于补充成员、调用链和历史实现存在性。该目录与 Version4 行号发生漂移，相关条目统一标记 evidenceStatus: version-drift，不得覆盖 Version4 的具体行为事实。
3. D:\TRbackup\tmodloader-api-docs-stable 是公开 API 的边界交叉验证。文档版本为 v2026.07；它不能替代 Version4 私有实现、静态副作用或调用顺序。
4. C:\Users\shan\Downloads\ECS\space-station-14-master 是有限的 ECS 粒度、Query、System、Tile 读写边界参考。其底层 RobustToolbox 物理和 Transform 实现不在本地参考范围内，因此不存在对 NLTX 的直接实现对应证据。
5. 当前 NLTX 的 src、Test 和 dome/src 只用于核对已有状态模型和部分执行路径；现有源码存在不等于行为等价或验证通过。

本次已读取的来源之间未发现需要以 evidence-mismatch 记录的直接冲突；完整参考源码的行号漂移已单独按 evidenceStatus: version-drift 处理。若后续发现版本或语义冲突，应保留 evidence-mismatch，而不是用较新的参考源码覆盖 Version4 事实。

来源角色及状态如下：

| 来源 | 角色 | 允许推出的结论 | 不允许推出的结论 |
|---|---|---|---|
| Version4 真实源码 | 主行为证据 | 字段所有权线索、真实读写者、调用链、隐藏副作用 | 未出现的 ECS 组件已存在、迁移已完成 |
| 完整参考源码 | 补证 | 成员或调用链在另一份完整基线中存在 | Version4 的具体行号、版本专属行为 |
| tModLoader 文档 | API 边界交叉验证 | 公开扩展点、公开字段语义、Tile 公开属性语义 | 私有 Collision 实现、Version4 行为等价 |
| Space Station 14 | ECS 结构参考 | Component 粒度、显式 Query/Map 访问、读写分离的设计启发 | NLTX 的命名、目录、领域语义或物理实现 |
| 当前 NLTX | 现状证据 | 已有类型、已有部分执行路径、项目引用和缺口 | 完整运行时集成、测试通过、迁移完成 |

## 4. Version4 真实代码证据表

下表只列 D:\TRbackup\Version4 的事实。行号按本次读取的文件定位；未能确认具体范围的地方使用“约”，不把补证源码的行号混入 Version4 事实。

| 主题 | 实际文件和符号 | 事实、读写和生命周期 | 状态 |
|---|---|---|---|
| 实体空间基础状态 | D:\TRbackup\Version4\Terraria\Entity.cs:6、:8、:10-34 | Entity 聚合 whoAmI、position、velocity、oldPosition、oldVelocity、oldDirection、direction、width、height、wet、shimmerWet、honeyWet、wetCount、lavaWet | confirmed |
| 派生几何 | D:\TRbackup\Version4\Terraria\Entity.cs:36-183 | AnyWet、Center、边缘点、Hitbox、Size 等由实体空间状态推导；派生值不是独立权威写集 | confirmed |
| Collision 类型和隐式状态 | D:\TRbackup\Version4\Terraria\Collision.cs:9-73 | TileContactSide、TileContact、HurtTile 与 stair、stairFall、honey、shimmer、sloping、up、down、contacts、_cacheForConveyorBelts 共存于静态 Collision 类 | confirmed；hidden-shared-state |
| 几何资格查询 | D:\TRbackup\Version4\Terraria\Collision.cs:74-196 | CheckAABBvAABBCollision 和两种线碰撞方法以几何参数计算相交；这些方法可以成为纯 Query 的候选，但当前类仍与静态状态共处 | confirmed；设计隔离为 proposed |
| 实体/Tiles 空间资格 | D:\TRbackup\Version4\Terraria\Collision.cs:197-316、:307 起、:414、:608 | CanHit(Entity, Entity)、几何重载、Tile 路径检查、CanHitWithCheck、CanHitLine、HitLine 共同承担空间资格和碰撞线路判断 | confirmed；边界拆分为 proposed |
| 液体接触查询 | D:\TRbackup\Version4\Terraria\Collision.cs:803、:829、:883、:944-1120 | EmptyTile、DrownCollision、IsWorldPointSolid、GetWaterLine、WetCollision、LavaCollision 读取 Tile 空间/液体状态并参与实体湿状态与运动决策 | confirmed |
| 液体查询的静态写入 | D:\TRbackup\Version4\Terraria\Collision.cs:1001-1007、:1047-1055 | WetCollision 会写入 Collision.honey 和 Collision.shimmer；这些不是显式返回值，存在调用顺序和重入风险 | confirmed；hidden-shared-state |
| 斜坡和移动辅助 | D:\TRbackup\Version4\Terraria\Collision.cs:1121-1471 | WalkDownSlope、SlopeCollision 处理斜坡；SlopeCollision 会写入 stair、stairFall、sloping | confirmed；hidden-shared-state |
| Tile 接触和移动 | D:\TRbackup\Version4\Terraria\Collision.cs:1472 起、:1633-1797 | BuildTileContacts、TileCollision 计算 Tile 接触和位置/速度修正；TileCollision 写入 up、down（:1633-1639） | confirmed；hidden-shared-state |
| 空 Tile 自动创建 | D:\TRbackup\Version4\Terraria\Collision.cs 中 GetWaterLine 约 :962-974、WalkDownSlope 约 :1150、TileCollision 约 :1746、:1766、StepDown 约 :2760-2764 | 特定空 Tile 情况会直接执行 new Tile()；查询或碰撞计算因此可能产生世界数组写入或对象替换副作用 | confirmed；side-effect risk |
| 其他空间计算 | D:\TRbackup\Version4\Terraria\Collision.cs:1798、:1887、:1944、:2027-2111 | TileCollisionInStepsOf16、IsClearSpotTest、FindCollisionTile、SolidCollision、WaterCollision 构成分步移动、清空空间和固体/水判断 | confirmed |
| Tile 伤害和固体区域 | D:\TRbackup\Version4\Terraria\Collision.cs:2348、:2355、:2448、:2677-2801 | AnyHurtingTiles、HurtTiles、CanTileHurt、SolidTiles、StepDown、StepUp 混合接触、伤害资格和移动辅助，不能全部归入一个通用状态组件 | confirmed |
| 传送带 | D:\TRbackup\Version4\Terraria\Collision.cs:3386-3523 | StepConveyorBelt 使用 _cacheForConveyorBelts，并通过 TileCollision 影响实体位置 | confirmed；cache/ordering risk |
| Tile 世界事实 | D:\TRbackup\Version4\Terraria\Tile.cs:6-24 | type、wall、liquid、sTileHeader、bTileHeader、bTileHeader2、bTileHeader3、frameX、frameY 是 Tile 基础状态 | confirmed |
| Tile 液体和几何属性 | D:\TRbackup\Version4\Terraria\Tile.cs:56-62、:220-319、:335-447 | 液体常量、liquidType、nactive、斜坡、half-brick、lava、honey、shimmer、water 等属性从 Tile 状态推导 | confirmed |
| Player 斜坡/湿/干碰撞 | D:\TRbackup\Version4\Terraria\Player.cs:14124-14321 | SlopeDownMovement 调用 Collision.WalkDownSlope 并写回 position、velocity；WetCollision、TryFloatingInFluid、DryCollision 选择移动模式、浮力和 Tile 碰撞 | confirmed |
| Player 移动提交 | D:\TRbackup\Version4\Terraria\Player.cs 约 :17445、:17451、:17459、:17570-17602 | Player 保存 oldPosition，处理 StepDown/StepUp，根据湿状态和实体策略调用湿/干碰撞，最终写回位置和速度 | confirmed |
| NPC 碰撞状态 | D:\TRbackup\Version4\Terraria\NPC.cs 约 :6371-6373、:78625-78685 | collideX、collideY 是 NPC 行为状态；UpdateCollision 清理并重新计算碰撞、旧速度、湿/干移动、斜坡、台阶和传送带 | confirmed |
| NPC 移动提交 | D:\TRbackup\Version4\Terraria\NPC.cs:78698-78743 | Collision_MoveWhileDry 根据速度变化设置 collideX、collideY，保存 oldPosition、oldDirection 并执行 position += velocity；ApplyTileCollision 再处理 Tile 结果 | confirmed |
| Projectile 碰撞策略 | D:\TRbackup\Version4\Terraria\Projectile.cs:194、:202、:250、:15513 | tileCollide、ignoreWater、correctSlopeCollision 和 HandleMovement 共同决定 Projectile-specific 移动和碰撞策略 | confirmed |
| Projectile 位置更新 | D:\TRbackup\Version4\Terraria\Projectile.cs 约 :15043、:15642-15819、:17995-18035 | Projectile 保存 oldPosition；高速分步、湿状态、Tile 碰撞和斜坡修正后，UpdatePosition 依据 wetVelocity 或 velocity 写入位置 | confirmed |
| Item 空间消费者 | D:\TRbackup\Version4\Terraria\Item.cs 前部和约 :258、:48768-48778 | Item 持有尺寸、位置、速度和 noWet；WorldItem 生成路径调用 Collision.WetCollision，是相邻的小型消费者 | confirmed |
| 世界 Tick 顺序 | D:\TRbackup\Version4\Terraria\Main.cs:11411-11620 | DoUpdateInWorld 中依次更新 Player（:11422-11447）、NPC（:11505-11525）、Projectile（:11530-11550）、Item（:11553-11571）、LeashedEntity、时间、WorldGen.UpdateWorld 和 Server 更新 | confirmed |
| 液体权威流程 | D:\TRbackup\Version4\Terraria\Liquid.cs:14-54、:56-69、:1015-1212；LiquidBuffer.cs:3、:11-31 | UpdateLiquid、AddWater、buffer、网络变更集合和 Tile 写集负责液体传播、合并、删除和提交 | confirmed；owner is LiquidSimulation |
| 世界结构写入 | D:\TRbackup\Version4\Terraria\WorldGen.cs:38、:1190-1196、:1422-1425、:1521-1552、:1690-1698、:1946-1999、:59400-59435 | WorldGen.UpdateWorld 及 PlaceTile/KillTile 相关路径直接写 Main.tile 的 type、wall、liquid、slope、halfBrick、active 等 | confirmed |

### 4.1 Version4 状态结论

Entity.position 是实体左上角世界坐标，velocity 是每次更新使用的速度，oldPosition、oldVelocity、oldDirection 是行为历史状态；Center、Hitbox 和边缘点是派生几何属性。实体继承聚合了 Player、NPC、Projectile、Item 的共同空间字段，但这不意味着 ECS 应创建一个巨型通用 Physics 组件。

Collision 目前同时承担几何计算、Tile 读取、液体接触、静态上下文、缓存和部分写入。WetCollision、TileCollision、SlopeCollision 的静态写入必须在拆分时显式化，否则同一 tick 中不同实体的调用顺序就会继续隐式决定结果。空 Tile 的自动创建是尤其重要的写入副作用：即使调用者把方法当作查询，它也可能触发世界状态改变。

## 5. 完整参考源码补证表

完整参考路径为 D:\TRbackup\无任何删减通过编译。其源码可补充“成员或调用链存在”的证据，但行号与 Version4 有明显漂移，因此本节每一项都带 evidenceStatus: version-drift。

| 主题 | 完整参考源码证据 | 只能用于何种补证 | 限制 |
|---|---|---|---|
| Collision 资格和移动 | D:\TRbackup\无任何删减通过编译\Terraria\Collision.cs:403-426、:520-536、:634、:828、:1558-1565 | 补证 CanHit、CanHitWithCheck、CanHitLine、HitLine、GetWaterLine 的公开成员/调用链存在 | 不覆盖 Version4 的具体实现和静态写入 |
| Collision 液体/斜坡/Tile | D:\TRbackup\无任何删减通过编译\Terraria\Collision.cs:1645、:1729、:1765、:1872、:2381、:2807-2844、:3707、:3773、:4357 | 补证 WetCollision、LavaCollision、WalkDownSlope、SlopeCollision、TileCollision、SolidCollision、StepDown、StepUp、StepConveyorBelt 仍属于同一类责任 | 不可据此移动 Version4 的行号或宣布行为相同 |
| Liquid | D:\TRbackup\无任何删减通过编译\Terraria\Liquid.cs:1015、:1194；D:\TRbackup\无任何删减通过编译\Terraria\LiquidBuffer.cs | 补证 UpdateLiquid、AddWater 和 buffer 组成液体写集 | 不可替代 Version4 液体数量/类型和网络行为核对 |
| Main 顺序 | D:\TRbackup\无任何删减通过编译\Terraria\Main.cs:17999、:18018-18200 | 补证世界更新仍有实体循环和世界更新阶段 | 不覆盖 Version4 实际数组和调用顺序 |
| NPC | D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:91880、:94377、:94450、:94471 | 补证 UpdateNPC、UpdateCollision、干移动和 Tile 应用调用链 | 不覆盖 Version4 的专用状态细节 |
| Player | D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:23773、:23793、:23861、约 :23960-23970 | 补证斜坡、湿碰撞、干碰撞和 Player Tile collision 边界 | 行号和实现版本漂移 |
| Projectile | D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:16280、:19208 | 补证 HandleMovement 和 UpdatePosition 的责任存在 | 不覆盖 Version4 的 Projectile-specific 条件 |
| Item | D:\TRbackup\无任何删减通过编译\Terraria\Item.cs:49515 | 补证 WorldItem 路径使用湿碰撞 | 不替代 Version4 Item 读取 |
| WorldGen | D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs:72688 | 补证 UpdateWorld 是世界结构/液体更新入口之一 | 不覆盖 Version4 的 Tile 写集 |
| Tile | D:\TRbackup\无任何删减通过编译\Terraria\Tile.cs:6、:260-281 | 补证 Tile、liquidType 和几何/液体成员存在 | 行号相对 Version4 明显漂移 |

本节统一结论：evidenceStatus: version-drift。完整参考源码是交叉检查材料，不是 Version4 独有行为基线。

## 6. tModLoader 公开 API 交叉验证表

本节读取的首页为 D:\TRbackup\tmodloader-api-docs-stable\index.html:8，版本标记在 :29，为 v2026.07。文档只验证公开边界和扩展接缝，不替代 Version4 私有代码。

| 公共页面 | 实际成员/锚点 | 交叉验证用途 | 状态和限制 |
|---|---|---|---|
| D:\TRbackup\tmodloader-api-docs-stable\class_entity.html:200-231 | oldPosition、oldVelocity、position、velocity、wet、whoAmI | position 是世界坐标左上角，velocity 是世界坐标每 tick 速度；whoAmI 是具体实体数组索引，不是通用持久化 ID | confirmed；仅公共语义 |
| D:\TRbackup\tmodloader-api-docs-stable\class_player.html:4757-4803 | height、oldPosition、oldVelocity、position、velocity、wet、whoAmI | 验证 Player 暴露空间字段；oldPosition 对 Projectile.extraUpdates 有额外更新语义 | confirmed；不能替代 Version4 Player 私有流程 |
| D:\TRbackup\tmodloader-api-docs-stable\class_collision-members.html:98-160 | CanHit、CanHitLine、CanHitWithCheck、GetWaterLine、LavaCollision、SlopeCollision、SolidCollision、StepDown、TileCollision、WetCollision | 验证这些方法是公开可见的 Collision 边界和扩展使用面 | confirmed；页面没有 Version4 静态副作用细节 |
| D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html:488-499、:516-521、约 :451-453 | PreUpdate、PreUpdateMovement、SendClientChanges、PostUpdate | 验证 Player 输入/运动修改和网络同步存在明确扩展接缝；PreUpdateMovement 可在基于 velocity 修改 position 前改变 velocity | confirmed；不是新的 NLTX owner 决策 |
| D:\TRbackup\tmodloader-api-docs-stable\class_mod_n_p_c.html:160、:228、:311-317、:335-350 | CanFallThroughPlatforms、ModifyCollisionData、PostAI、PreAI、ReceiveExtraAI、SendExtraAI | 验证 NPC 平台策略、碰撞数据扩展、AI 生命周期和网络扩展应留在 NPC 域或适配边界 | confirmed；不推断 Version4 私有实现 |
| D:\TRbackup\tmodloader-api-docs-stable\class_mod_projectile.html:1051-1114、:117-118、:220-230、:265 | SendExtraAI、ReceiveExtraAI、CanDamage、OnTileCollide、PreAI、PostAI、TileCollideStyle | 验证 Projectile-specific 碰撞策略和 Tile 碰撞回调需要独立接缝 | confirmed；不覆盖 Version4 条件顺序 |
| D:\TRbackup\tmodloader-api-docs-stable\struct_tile.html:206、:214、:250-280、:666-729 | HasTile、IsActuated、LiquidAmount、LiquidType、SkipLiquid、Slope | LiquidAmount 范围为 0..255；LiquidType 先要求有液体；actuator Tile 不应当被当作固体；坡度是 Tile 几何事实 | confirmed；公开 API 语义交叉验证 |

公开 API 的结果支持“实体策略、空间查询和世界 Tile 视图分离”的边界，但不提供 Version4 Collision 的静态 flags、空 Tile 自动创建或调用顺序证据。whoAmI 的公共语义尤其说明：它可以作为运行时数组索引引用，不能直接被当作跨存档的稳定实体 ID。

## 7. Space Station 14 最小相关 ECS 参考表

读取的参考仓库为 C:\Users\shan\Downloads\ECS\space-station-14-master。本地 RobustToolbox 子模块为空，未读取到 Robust.Shared.Physics、PhysicsComponent、FixturesComponent、MapGridComponent、TransformComponent、SharedPhysicsSystem 或 SharedTransformSystem 的实现。因此以下只能作为结构参考，并明确 Space Station 14 无直接对应证据。

| 实际读取文件 | 类型/方法 | 参考用途 | 对 NLTX 的限制 |
|---|---|---|---|
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Movement\Components\MobCollisionComponent.cs | MobCollisionComponent | 说明移动碰撞能力可以作为独立组件 | 不复制其命名、字段或领域语义 |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Movement\Components\MovementSpeedModifierComponent.cs | MovementSpeedModifierComponent | 说明速度修饰可以和基础移动状态分离 | 不说明 NLTX 的速度权威 owner |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Movement\Systems\SharedMobCollisionSystem.cs | SharedMobCollisionSystem、EntityQuery/TryComp 访问 | 参考 System 通过显式组件查询和依赖访问完成碰撞流程 | 无法提供 Version4 碰撞行为等价证据 |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Movement\Systems\SharedMoverController.cs | SharedMoverController | 参考输入到移动意图的接缝 | Player/NPC/Projectile 的 NLTX 策略仍需 Integration Review |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Movement\Systems\SharedSpriteMovementSystem.cs | SharedSpriteMovementSystem | 说明显示移动可以是移动状态的下游消费者 | 不把渲染状态纳入 SpatialSimulation |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Movement\Events\MoveInputEvent.cs | MoveInputEvent | 参考输入事件和模拟状态之间的边界 | 不替代 NLTX 的 PlayerGameplay 输入协议 |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Movement\Events\WeightlessMoveEvent.cs | WeightlessMoveEvent | 参考特殊移动策略作为事件/输入 | 不决定 NLTX 的重力方向模型 |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Maps\ContentTileDefinition.cs | ContentTileDefinition | 参考 Tile 定义与实体碰撞读取分离 | 不复制 Tile 目录结构或数据模型 |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Maps\TileSystem.cs、TurfSystem.cs | TileSystem、TurfSystem | 参考地图查询与 Tile 变更可以由不同 System 管理 | 本地底层地图实现缺失，不能证明直接映射 |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Physics\CollisionGroup.cs | CollisionGroup | 参考碰撞类别/策略作为显式数据 | 不将其当作 NLTX 最终策略枚举 |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Physics\PreventCollideComponent.cs、SharedPreventCollideSystem.cs | PreventCollideComponent、SharedPreventCollideSystem | 参考碰撞禁用关系和 System 边界 | 不决定 NLTX 的实体引用和关系 owner |

参考结论是：移动能力可以拆成多个组件，System 可通过显式 Query 访问 Transform、Physics 和 Map，Tile 查询与 Tile 变更应分离，接触结果可以通过事件或专用派生组件传递。但由于底层实现缺失，不能把 Space Station 14 的物理模型、目录结构、命名或领域语义复制到 NLTX。

## 8. 成员、字段、方法、读写者和生命周期盘点

| 盘点对象 | 权威事实/结果 | 主要读者 | 主要写者 | 生命周期和副作用 |
|---|---|---|---|---|
| Entity.position | 实体左上角世界位置 | Player/NPC/Projectile/Item 更新、几何派生 | 各实体更新流程；未来应由明确移动提交边界写入 | 每 tick 变化；是空间提交根 |
| Entity.velocity | 每次更新使用的世界速度 | Collision、实体 AI、位置更新 | 各实体移动和碰撞后处理 | 每 tick 变化；碰撞可修正 |
| Entity.oldPosition、oldVelocity、oldDirection | 行为历史和碰撞前状态 | 实体行为、插值/回调、Projectile extra update 语义 | 各实体更新流程 | 历史状态；不应被误当作世界事实 |
| Entity.width、height、派生 Hitbox | 碰撞形状和几何投影 | Collision、空间资格和伤害查询 | Player/NPC/Projectile/Item 域 | 尺寸会随实体状态变化；派生 Hitbox 不应独立持久化 |
| Entity.wet、honeyWet、shimmerWet、lavaWet、wetCount | 实体液体接触/影响状态 | Player/NPC/Projectile/Item 更新 | Collision/实体更新交织写入 | 每 tick 或连续状态；液体世界事实不归此字段拥有 |
| Collision.honey、shimmer、stair、stairFall、sloping、up、down | 当前碰撞调用上下文的隐式结果 | 后续 Collision/实体流程 | Collision 静态方法 | 静态共享、重入不安全、顺序敏感；应改为显式 result |
| Collision.contacts | Tile 接触集合或缓存 | Collision 后续计算和实体移动 | Collision | 不应成为跨 tick 权威缓存；生命周期需限定到一次解析 |
| _cacheForConveyorBelts | 传送带计算缓存 | StepConveyorBelt | Collision | 仅缓存；不得作为持久化或网络事实 |
| Tile.type、wall、header、frame | 世界 Tile 结构事实 | WorldStorage、WorldGen、Tile view、Collision | WorldGen/WorldInteraction/WorldStorage | 持久世界状态；结构写必须走世界边界 |
| Tile.liquid 和 liquid type | 液体数量/类型世界事实 | LiquidSimulation、只读 Tile view、液体接触读取 | Liquid.UpdateLiquid/AddWater/WorldStorage | 液体 tick 写集；不能由 SpatialSimulation 写入 |
| Tile slope/half-brick/actuated | Tile 几何和可碰撞事实 | TileNeighborhoodQuery/Collision | WorldInteraction/WorldStorage | 结构变更生命周期；SpatialSimulation 只读 |
| Player.collideX/collideY | Player/NPC-specific 碰撞行为状态 | 实体后续逻辑、网络或 AI | Player/NPC 移动流程 | 是否通用化尚未决定；需保持历史语义 |
| Projectile.tileCollide/ignoreWater/correctSlopeCollision | Projectile 碰撞策略输入 | Projectile HandleMovement、Collision | Projectile 域 | 生命周期随 Projectile；不是通用 Physics 权威状态 |
| Liquid.UpdateLiquid、AddWater、LiquidBuffer | 液体传播和提交 | WorldGen/Main/网络更新 | LiquidSimulation/WorldStorage | 独立液体 tick；产生 Tile 写集和网络变化 |
| Main.DoUpdateInWorld | Version4 调度入口 | Player/NPC/Projectile/Item/WorldGen | Main | 文件顺序不是未来 ECS 调度契约 |
| WorldGen.UpdateWorld | 世界结构和液体更新入口 | Main | WorldGen/Liquid/WorldStorage | 位于实体循环之后，但新调度顺序需显式确认 |

### 8.1 读写结论

目前最危险的读写混合点是 Collision：它既接受实体和 Tile 输入，又通过静态字段输出上下文结果，还可能创建 Tile，并由不同实体域直接写回位置、速度和湿状态。拆分时，纯几何计算、只读 Tile 访问、液体接触读取和实体位置提交必须成为可独立核对的阶段。

## 9. 权威状态所有权表

下表区分 Version4 当前事实、建议 owner 和尚未裁决的共享类型。建议 owner 不是已落地的实现。

| 状态 | Version4 事实 | 建议权威 owner | 跨子系统/状态 |
|---|---|---|---|
| 实体位置 | Player/NPC/Projectile/Item 各自更新并写回 position | 各实体域提供策略，统一提交方案待 Integration Review | 当前多写者 |
| 实体速度 | 各实体域和 Collision 后处理共同影响 velocity | MovementState 或现有 VelocityComponent，见 BD-01 | unresolved |
| 碰撞形状 | width/height 等在实体上 | CollisionShape，status: proposed | 由实体域提供输入 |
| 运动历史 | oldPosition、oldVelocity、oldDirection | MotionHistory，status: proposed | Projectile extra-update 语义需单独建模 |
| Tile 几何和结构 | Tile 保存 type、active、slope、half-brick、actuator 等 | WorldStorage 提供只读 Tile view | SpatialSimulation 只读 |
| 液体数量和类型 | Tile 和 Liquid 全局流程维护 | LiquidSimulation/WorldStorage | SpatialSimulation 只读 |
| 液体接触 | 实体湿状态由实体更新和 Collision 共同形成 | LiquidContact 派生结果，status: proposed；生成者见 BD-03 | crossSubsystemOwner: integration-review |
| 碰撞策略 | Player/NPC/Projectile/Item 各有专用条件 | 实体域提供 CollisionPolicy 输入，status: proposed | 不由通用空间域拥有来源 |
| 碰撞结果 | 当前通过静态 Collision 状态和实体字段传播 | CollisionResult/ContactResult 派生结果，status: proposed | TouchedTiles owner 见 BD-05 |
| TileCoordinate | 当前存在多个 NLTX 类型 | 统一共享 owner 待 Integration Review | crossSubsystemOwner: integration-review |
| WorldSectionId/SpatialSpaceId | 当前设计需要表达世界、区段和父子空间 | 共享 owner 待 Integration Review | crossSubsystemOwner: integration-review |
| EntityReference | 当前位于 src/Relationships/EntityReference.cs，被 CollisionResult 使用 | 共享关系 owner 待 Integration Review | crossSubsystemOwner: integration-review |
| 网络/存档 ID | Version4 whoAmI 是数组索引 | 由实体/网络/存档集成层定义稳定 ID | 不得直接使用 whoAmI 持久化 |

## 10. proposed 组件拆分

以下是可实现的设计草案，尚未创建文件，也不代表最终 owner。每一项都带 status: proposed。

| 稳定 ID | proposed 路径和类型 | 建议字段/约束 | owner 与边界 |
|---|---|---|---|
| movement-state | src/SpatialSimulation/Components/MovementState.cs，MovementState；status: proposed | position、velocity、重力方向/倍率、grounded、movement lock、collision state、motion phase | 若采用统一模型，可成为移动提交输入；与 BD-01 绑定 |
| collision-shape | src/SpatialSimulation/Components/CollisionShape.cs，CollisionShape；status: proposed | width、height、offset、shape kind、enabled；不保存碰撞结果 | SpatialSimulation 读取；实体域负责策略性尺寸变化 |
| spatial-reference | src/SpatialSimulation/Components/SpatialReference.cs，SpatialReference；status: proposed | world/section/parent relation、parent offset、空间引用、Tile 映射 | crossSubsystemOwner: integration-review；不在本节裁决共享 ID |
| motion-history | src/SpatialSimulation/Components/MotionHistory.cs，MotionHistory；status: proposed | previous position、previous velocity、recorded tick、history kind | 区分普通 tick 与 Projectile extra-update 历史；不做网络事实 |
| liquid-contact | src/SpatialSimulation/Components/LiquidContact.cs，LiquidContact；status: proposed | dominant liquid kind、in-liquid、water/lava/honey/shimmer flags、contact tick | 只保存接触派生结果；不保存液体数量、传播或写集；生成方式见 BD-03 |
| collision-policy | src/SpatialSimulation/Components/CollisionPolicy.cs，CollisionPolicy；status: proposed | Tile collide、platform fall-through、door/Aetherium/hoik/slope policy、实体碰撞开关 | 输入由 Player/NPC/Projectile 域形成；SpatialSimulation 不拥有专用行为来源 |
| collision-result | src/SpatialSimulation/Components/CollisionResult.cs，CollisionResult；status: proposed | touched entities、touched tiles、blocking normal、blocked axes、step-up、resolved tick | 当前 NLTX 已有相近类型，但最终共享字段和 owner 未裁决；TouchedTiles 见 BD-05 |

### 10.1 组件不变量

- MovementState 或现有位置/速度组件只能有一个权威写者；Query 不直接修改它。
- CollisionShape 描述几何输入，不混入本 tick 接触结果、网络状态或液体数量。
- SpatialReference 只表达关系和坐标空间；EntityReference、TileCoordinate、WorldSectionId、SpatialSpaceId 的共享 owner 必须经 Integration Review 确认。
- MotionHistory 表示历史采样，不得作为当前空间事实的第二份权威副本。
- LiquidContact 只能表达 LiquidSimulation 已形成或允许读取的接触派生值，不能反向拥有 Liquid Tile 写集。
- CollisionPolicy 是策略输入，不把 Player 装备、NPC AI 或 Projectile 生命周期复制进通用空间组件。

## 11. System、Query、Command、Adapter、Projection 边界

### 11.1 proposed Query 契约

| 稳定 ID | proposed 类型、路径和签名 | 读取 | 返回/写入 | 约束 |
|---|---|---|---|---|
| contact-query | src/SpatialSimulation/Queries/ContactQuery.cs，ContactQuery.Resolve(ReadOnlyTileView tileView, CollisionShape shape, MovementState movement, CollisionPolicy policy)；status: proposed | 显式只读 Tile view、形状、位置、速度、策略 | 返回 CollisionResult；status: proposed；不写实体、不写 Tile | 纯计算；不读隐式静态 flags、不创建 Tile |
| spatial-qualification-query | src/SpatialSimulation/Queries/SpatialQualificationQuery.cs，SpatialQualificationQuery.CanHit(...)、CanHitWithCheck(...)、CanHitLine(...)、SolidCollision(...)、IsClearSpot(...)；签名 status: proposed | 几何输入和只读 Tile view | 返回带 reason 的稳定资格结果；类型 status: proposed | 不暴露可变内部集合，不提交伤害或结构变化 |
| liquid-contact-query | src/SpatialSimulation/Queries/LiquidContactQuery.cs，LiquidContactQuery.Read(LiquidContactReadPort source, SpatialReference reference, CollisionShape shape)；status: proposed | LiquidSimulation 只读接触 view | 返回 LiquidContact；status: proposed | 不写 LiquidSimulation，不写液体数量/类型 |
| tile-neighborhood-query | src/SpatialSimulation/Queries/TileNeighborhoodQuery.cs，TileNeighborhoodQuery.Read(ReadOnlyTileView view, TileCoordinate center, Bounds bounds)；签名和依赖 status: proposed | WorldStorage 的只读 Tile geometry、platform、slope、actuated、liquid presence | 返回不可变 Tile neighborhood view；类型 status: proposed | 不执行 Tile mutation，不触发空 Tile 自动创建 |

### 11.2 proposed System 契约

| 稳定 ID | proposed 类型、路径和签名 | 读取 | 写入/副作用 | 调度边界 |
|---|---|---|---|---|
| movement-system | src/SpatialSimulation/Systems/MovementSystem.cs，MovementSystem.Update(MovementState state, CollisionShape shape, CollisionPolicy policy, SpatialReference reference, CollisionCommandBuffer commands)；status: proposed | 移动状态、形状、策略、空间引用 | 生成 CollisionCommand；status: proposed；不直接写世界数组 | 由显式模拟调度调用 |
| collision-resolution-system | src/SpatialSimulation/Systems/CollisionResolutionSystem.cs，CollisionResolutionSystem.Resolve(ContactQuery query, MovementIntent intent)；status: proposed | Query、移动意图和只读 Tile view | 生成轴阻挡、平台、斜坡、台阶和 contact result | 不执行 Player/NPC/Projectile 专用后处理 |
| liquid-contact-system | src/SpatialSimulation/Liquid/Systems/LiquidContactSystem.cs，LiquidContactSystem.Update(LiquidContactReadPort source, EntitySpatialQuery spatialQuery)；status: proposed | LiquidSimulation 接触 view、实体形状/空间引用 | 形成实体侧 LiquidContact 派生结果；类型 status: proposed | 不写 Tile liquid，不参与液体传播/提交 |
| movement-commit-system | src/SpatialSimulation/Systems/MovementCommitSystem.cs，MovementCommitSystem.Commit(CollisionCommand command, CollisionCommitPort port)；status: proposed | 已解析 Command、实体版本和提交端口 | 唯一写 position/velocity/motion history；失败结果显式返回 | 唯一写者或保留各域提交的选择见 BD-04 |
| spatial-projection-system | src/SpatialSimulation/Projections/SpatialProjectionSystem.cs，SpatialProjectionSystem.Project(SpatialSnapshot snapshot)；status: proposed | 权威位置、速度、碰撞和接触快照 | 网络、客户端、诊断投影；类型 SpatialSnapshot 为 status: proposed | 不反向写模拟状态 |

### 11.3 proposed Command、Port、Adapter

| 稳定 ID | proposed 类型、路径和签名 | 责任 | 写入/失败规则 |
|---|---|---|---|
| collision-command | src/SpatialSimulation/Commands/CollisionCommand.cs，CollisionCommand；status: proposed | 携带 entity reference、candidate position、candidate velocity、policy、tick、resolved movement/contact | 只描述待提交结果，不自行执行写入；EntityReference 标记 crossSubsystemOwner: integration-review |
| collision-command-buffer | src/SpatialSimulation/Commands/CollisionCommandBuffer.cs，CollisionCommandBuffer；status: proposed | 收集一个明确阶段产生的移动命令 | 不作为跨 tick 权威缓存；重复命令、版本冲突和顺序必须有显式结果 |
| collision-commit-port | src/SpatialSimulation/Ports/CollisionCommitPort.cs，CollisionCommitPort.Commit(CollisionCommand command)；签名 status: proposed | 唯一写实体位置、速度和运动历史；记录提交顺序 | 版本不匹配、实体缺失、空间失效时返回失败结果；不得静默覆盖 |
| tile-read-port | src/SpatialSimulation/Ports/TileReadPort.cs，TileReadPort.GetReadOnlyView(TileCoordinate coordinate)；签名 status: proposed | 由 WorldStorage 提供只读 Tile view | 返回 immutable view；TileCoordinate 标记 crossSubsystemOwner: integration-review；不创建空 Tile |
| liquid-contact-read-port | src/SpatialSimulation/Ports/LiquidContactReadPort.cs，LiquidContactReadPort.Read(EntityReference entity)；签名 status: proposed | LiquidSimulation 提供只读接触 view | SpatialSimulation 只读消费；EntityReference 标记 crossSubsystemOwner: integration-review |
| world-storage-tile-adapter | src/SpatialSimulation/Adapters/WorldStorageTileAdapter.cs，WorldStorageTileAdapter；status: proposed | 将 WorldStorage Tile 数据适配为只读空间视图 | 不把 WorldStorage 的可变数组暴露给 Collision |
| liquid-contact-adapter | src/SpatialSimulation/Adapters/LiquidContactAdapter.cs，LiquidContactAdapter；status: proposed | 将 LiquidSimulation 的接触 projection 适配为查询输入 | 不复制液体权威状态，不写 LiquidSimulation |
| spatial-replication-projection | src/SpatialSimulation/Projections/SpatialReplicationProjection.cs，SpatialReplicationProjection；status: proposed | 输出网络/客户端可读空间快照 | 只出站；不接受客户端投影作为权威写入 |

### 11.4 proposed 视图和值类型

下列类型是前述 Query、Port 和调度草案的显式数据契约，均为 status: proposed，尚未创建：

| 稳定 ID | proposed 路径和类型 | 作用 | 共享边界 |
|---|---|---|---|
| readonly-tile-view | src/SpatialSimulation/Ports/ReadOnlyTileView.cs，ReadOnlyTileView；status: proposed | 暴露 Tile 几何、可碰撞、actuated、斜坡、half-brick 和液体 presence 的不可变读取结果 | TileCoordinate 的 owner 为 crossSubsystemOwner: integration-review |
| liquid-contact-read-view | src/SpatialSimulation/Ports/LiquidContactReadView.cs，LiquidContactReadView；status: proposed | 暴露 LiquidSimulation 在明确 snapshot tick 形成的实体接触读取结果 | LiquidContact 生成方和传输形状为 crossSubsystemOwner: integration-review |
| tile-neighborhood-view | src/SpatialSimulation/Queries/TileNeighborhoodView.cs，TileNeighborhoodView；status: proposed | 承载 TileNeighborhoodQuery 返回的不可变邻域 | 不允许反向执行 WorldStorage 或 Tile mutation |
| spatial-qualification-result | src/SpatialSimulation/Queries/SpatialQualificationResult.cs，SpatialQualificationResult；status: proposed | 承载资格结果、reason 和 resolved tick | 只表达派生结果，不自动触发伤害或网络提交 |
| spatial-snapshot | src/SpatialSimulation/Projections/SpatialSnapshot.cs，SpatialSnapshot；status: proposed | 承载位置、速度、grounded、碰撞和液体接触的只读投影输入 | 不作为客户端反向写入的权威状态 |

## 12. 调用方向和 System 顺序

### 12.1 Version4 已确认的方向

Version4 的实际调用方向大致为：

~~~text
Main.DoUpdateInWorld
  -> Player.Update / NPC.UpdateNPC / Projectile.Update / Item.UpdateItem
      -> entity-specific movement
          -> Collision.WetCollision / DryCollision / TileCollision / SlopeCollision / Step...
              -> Tile / Main.tile
  -> WorldGen.UpdateWorld
      -> Liquid.UpdateLiquid / WorldGen tile mutation
~~~

Main.cs:11411-11620 显示 Player、NPC、Projectile、Item 的实体更新循环和随后世界更新阶段；这不是未来 ECS 的合法排序依据。特别是，当前 Collision 的静态字段和数组写入会使调用顺序隐藏地影响结果。

### 12.2 proposed 显式调度顺序

以下是 status: proposed 的调度草案，最终顺序必须由 Integration Review 确认：

~~~text
Phase A  Snapshot
  WorldStorage -> ReadOnlyTileView
  LiquidSimulation -> LiquidContactReadView
  Entity domains -> MovementState / CollisionPolicy inputs

Phase B  Derivation
  LiquidContactSystem -> entity LiquidContact
  TileNeighborhoodQuery -> immutable tile neighborhood
  ContactQuery / CollisionResolutionSystem -> CollisionResult

Phase C  Domain policy
  PlayerGameplay / NpcAndTownSimulation / ProjectileSimulation
    -> entity-specific movement intent and collision follow-up

Phase D  Commit
  MovementSystem -> CollisionCommand
  MovementCommitSystem -> CollisionCommitPort -> position / velocity / history

Phase E  Side effects and projection
  WorldInteractionAndStructures -> explicit tile commands
  SpatialProjectionSystem -> network / client / diagnostics
~~~

应保持的依赖方向是：

~~~text
WorldStorage (read view)       LiquidSimulation (read view)
          |                              |
          +----------> SpatialSimulation Queries
                                |
Player/NPC/Projectile policies -+
                                v
                         CollisionCommand
                                |
                         Commit boundary
                                v
                 entity state / network projection
~~~

SpatialSimulation 不应反向调用液体传播、WorldGen 结构写入或网络提交。Tile 结构命令可以由 WorldInteractionAndStructures 处理，但其与移动提交的先后、失败和重试必须进入 BD-06 的集成裁决。

### 12.3 顺序约束

- 同一实体在一个 tick 内必须先采样历史，再读取策略和世界视图，再解析碰撞，最后提交位置/速度；不能由文件顺序或数组顺序隐式决定。
- 同一实体不能让 Player/NPC/Projectile 专用流程和通用 MovementCommitSystem 交替写同一位置而没有版本或 ownership 检查。
- LiquidSimulation 的液体传播提交与实体液体接触读取必须有明确快照点；实体接触不能一半读取旧液体、一半读取新液体。
- 传送、坐骑、矿车、绳索、门、平台和 Projectile-specific 回调的先后未确定，不能在本报告中假定统一顺序。
- Collision Query 不得因为查询一个空 Tile 而隐式执行 Tile 创建；兼容阶段如果必须保留该行为，也要把写入转成显式命令并记录失败/重试语义。

## 13. 持久化、网络和客户端投影边界

### 13.1 权威持久化

持久化的世界事实包括 Tile type、wall、active、slope、half-brick、actuator、liquid 数量/类型以及 WorldStorage 管理的结构数据。SpatialSimulation 只能通过 TileReadPort 消费只读视图，不能把碰撞结果写回 Tile，也不能持有液体写集。

实体位置、速度和实体专用状态是否存档，由实体域和存档边界确定。oldPosition、oldVelocity、oldDirection 通常是运行时历史，除非外部协议明确需要，否则不应作为长期世界事实。MotionHistory 即使存在，也不能自动进入网络或存档。

### 13.2 网络

whoAmI 是实体数组索引，不能直接作为跨会话、跨服务器或存档的稳定 ID。EntityReference、SpatialSpaceId、WorldSectionId 和网络实体 ID 的统一 owner 尚未裁决，均需标记 crossSubsystemOwner: integration-review。

液体传播产生的网络变更集合属于 LiquidSimulation/WorldStorage；SpatialSimulation 可以读取一个与 tick 对齐的液体接触 projection，但不得自己生成液体网络写集。碰撞结果是否网络复制取决于 TouchedTiles 和 contact result 的用途，见 BD-05。

### 13.3 客户端和诊断投影

建议把位置、速度、grounded、阻挡轴和液体接触作为只读空间快照的字段，再由 SpatialProjectionSystem 输出客户端或诊断视图；该类型和路径均为 status: proposed。客户端回传的预测位置、插值结果或渲染缓存不能反向覆盖权威 MovementState。

## 14. 当前 NLTX 映射

### 14.1 已有状态和项目

当前已读取到的根 src 结构包括：

| 当前路径 | 当前内容 | 与目标边界的关系 |
|---|---|---|
| D:\TRbackup\NLTX\src\Physics\CollisionPolicyComponent.cs | Tile/entity collision flags、liquid/door/Aetherium/platform/hoik policy、slope mode | 已有策略模型；需要确认实体域输入和 SpatialSimulation 读取方向 |
| D:\TRbackup\NLTX\src\Physics\CollisionResultComponent.cs | touched entities/tiles、blocking normal、blocked axes、step-up、resolved tick | 已有结果模型；EntityReference 和 TileCoordinate 仍是共享 owner 议题 |
| D:\TRbackup\NLTX\src\Physics\PhysicsStateComponent.cs | acceleration、gravity direction/scale、grounded、movement lock | 已有运动状态候选；不等于已接入完整执行链 |
| D:\TRbackup\NLTX\src\Physics\MovementIntentComponent.cs | 移动意图 | 可作为实体域到空间解析的输入，但提交顺序仍未闭合 |
| D:\TRbackup\NLTX\src\Physics\CollisionAxisMask.cs、GravityDirection.cs、SlopeCollisionMode.cs | 策略/值类型 | 是已有局部模型，不宣称与 Version4 完全对应 |
| D:\TRbackup\NLTX\src\Share\Entity\Components\ColliderComponent.cs | 矩形尺寸、offset、shape kind、enabled | 与 proposed CollisionShape 存在映射候选；不要重复创建第二份权威形状 |
| D:\TRbackup\NLTX\src\Share\Entity\Components\LiquidComponent.cs | dominant liquid kind、in-liquid、water/lava/honey/shimmer flags、timer、resolved tick | 跨边界审查证据；不得因此把 LiquidSimulation 吞回 SpatialSimulation |
| D:\TRbackup\NLTX\src\Share\Entity\Components\MotionHistoryComponent.cs | previous position、previous velocity、history kind、recorded tick | 与 proposed MotionHistory 有映射候选；Projectile extra-update 仍需验证 |
| D:\TRbackup\NLTX\src\Share\Entity\Components\SpatialReferenceComponent.cs | parent entity、parent offset、relative/world space | 与 proposed SpatialReference 有映射候选；共享关系 owner 未定 |
| D:\TRbackup\NLTX\src\Share\Entity\Components\LocationComponent.cs、VelocityComponent.cs | 分别持有位置和速度 | 与统一 MovementState 存在 BD-01 决策冲突 |
| D:\TRbackup\NLTX\src\Share\Entity\Components\EntityIdentityComponent.cs | 实体身份状态 | 不把 whoAmI 直接映射为稳定持久化 ID |
| D:\TRbackup\NLTX\src\Share\Entity\Queries\EntityGeometryQuery.cs、EntitySpatialQuery.cs | 纯几何、距离和方向计算 | 说明已有 Query 候选；尚未形成 Tile/contact/commit 契约 |
| D:\TRbackup\NLTX\src\Physics\Terraria.Physics.csproj | target net10.0，引用 Relationships 和 WorldInteraction | 项目边界存在；未执行 build 或测试 |

### 14.2 Dome 当前执行路径

| 当前路径 | 读取到的事实 | 状态 |
|---|---|---|
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Movement\Systems\MovementSystem.cs:8-19 | 直接读取 Location/Velocity 并累加位置，没有接入 Collision Query、Tile view、Contact result 或显式 commit seam | partial；执行契约不闭合 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Physics\Systems\TileCollisionSystem.cs:28-50 | 有局部 Tile collision 执行实现，直接修改传入 transform、velocity、physics | partial；不能证明 Version4 等价或完整运行时接入 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Physics\Systems\GroundCollisionSystem.cs、BottomSlopeCollisionRuleSystem.cs、PlatformCollisionRuleSystem.cs、TopSlopeContactSystem.cs | 已有若干空间规则 System | partial；需确认统一读写边界和执行顺序 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldGrid.cs:82-86、:94-137、:150-188、:241-258 | Tile 读取、Tile/liquid 写入、liquid commit、Tile command commit 分开存在 | partial；可作为边界线索，不宣称完成隔离 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Simulation\DomeSimulation.cs:5002-5194 | 存在显式 Tick phase；Player collision 在 :5139-5152，NPC 在 :5162-5167，Projectile 在 :5170-5175，Liquid 在 :5182-5184，command commit 在 :5186-5189 | partial；仍需集成审查 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Simulation\DomeSimulation.cs:5779-5809 | Player 读取 Location/Velocity/Physics/ControlInput/Collider，调用 TileCollisionSystem 和 TopSlopeContactSystem | partial；存在接入，但不是完整 Version4 证明 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Simulation\DomeSimulation.cs:9235-9275 | NPC 读取 MovementIntent、Location、Velocity、Physics、Collider，调用 TileCollisionSystem | partial；NPC 专用后处理和提交边界未闭合 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidContactComponent.cs:5-59、LiquidCommitSystem.cs:9-60、DomeSimulation.cs:5247-5332 | 液体接触、传播、合并、提交、脏区和复制是独立流程 | confirmed boundary；不得并回 SpatialSimulation |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj | target net10.0，引用 Arch 2.1.0 和根 Entity ECS 项目 | confirmed project fact；未执行 build |

### 14.3 当前映射结论

NLTX 已有“状态模型”和“局部执行实现”，但仍缺少统一的只读 Tile view、LiquidContactReadPort、CollisionCommand/CommitPort、碰撞 Query 的纯度保证、跨项目 TileCoordinate 统一 owner，以及明确的 Player/NPC/Projectile 提交顺序。src/Share/Entity/Components/LiquidComponent.cs 与 Dome 的 Liquid 流程是反向边界证据：它们显示液体接触可以被实体读取，但不应改变 LiquidSimulation 的数量、类型、传播和提交权威。

当前发现两个 TileCoordinate：

- D:\TRbackup\NLTX\src\WorldStorage\TileCoordinate.cs；
- D:\TRbackup\NLTX\src\WorldInteraction\Tiles\TileCoordinate.cs。

其统一 owner 未决。D:\TRbackup\NLTX\src\Relationships\EntityReference.cs 已被 CollisionResult 使用，但关系引用的跨子系统最终 owner 也未决。此类重复或共享类型不得由本报告擅自合并或移动。

## 15. focused verifier 设计和建议命令

当前已存在 focused verifier 源码：

- D:\TRbackup\NLTX\dome\Test\Terraria.Dome.PlayerPhysics.Verification\Program.cs:9-21：入口和基础落地测试；
- D:\TRbackup\NLTX\dome\Test\Terraria.Dome.PlayerPhysics.Verification\Program.cs:23-36：反重力测试；
- D:\TRbackup\NLTX\dome\Test\Terraria.Dome.PlayerPhysics.Verification\Program.cs:38-47 起：Tile collision、half-brick、slope、platform、actuator、平台穿越等测试；
- D:\TRbackup\NLTX\dome\Test\Terraria.Dome.PlayerPhysics.Verification\Terraria.Dome.PlayerPhysics.Verification.csproj：目标 net10.0，引用 Dome Simulation 项目。

这些文件的存在只能说明已有验证设计和测试入口，不能写成测试通过。

未来若获得执行授权，应遵守仓库的 BUILD-CONCURRENCY-1：先检查活动的 dotnet.exe 和 csc.exe，确认没有其他 owner，再从仓库根目录通过串行包装脚本执行受影响项目。建议命令形状如下，当前未执行：

~~~powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 test .\dome\Test\Terraria.Dome.PlayerPhysics.Verification\Terraria.Dome.PlayerPhysics.Verification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false --no-build --no-restore
~~~

建议 verifier 未来覆盖的判定维度：

| 维度 | 应验证的关系 | 当前报告结论 |
|---|---|---|
| 纯 Query | 相同只读输入多次查询得到相同结果，且不产生 Tile 或静态 Collision 写入 | 未运行，not-run |
| Tile 碰撞 | 固体、half-brick、斜坡、平台、actuator 的空间结果和边界接触 | 已有源码入口；未运行 |
| 移动提交 | 位置、速度、历史只由明确提交边界更新；版本冲突可观察 | 当前契约缺口；未运行 |
| 液体接触 | 接触读取不会改变液体数量、类型、传播或提交集合 | 当前边界设计；未运行 |
| 高速和历史 | 分步移动、oldPosition、oldVelocity 以及 Projectile extra-update 语义 | evidence-gap；未运行 |
| 反重力 | 重力方向、速度轴、grounded 和斜坡规则保持一致 | 已有源码入口；未运行 |
| 副作用隔离 | 不调用隐式 new Tile()，不依赖 Collision 静态 flags，不写网络/存档集合 | 当前尚未形成完整 verifier；未运行 |

本轮不新增 verifier、不运行命令、不生成 Build/bin 或 Build/obj 输出。

## 16. 本次验证状态

~~~text
verificationStatus: not-run
buildStatus: not-run
testStatus: not-run
restoreStatus: not-run
coverageStatus: not-run
~~~

本轮只进行了文件和源码的只读审查，并写入本报告。没有运行 dotnet restore、dotnet build、dotnet test、coverage checker 或其他 compile-capable 命令，因此不存在可记录的退出码、警告/错误统计或新 artifact。报告不把既有 verifier 源码存在写成通过，也不把当前 Dome 局部执行路径写成完整实现。

## 17. 不拆分项

下列对象不应升级为一级 SpatialSimulation 子系统、独立权威组件或额外固定模拟子系统：

| 对象 | 不拆分理由 | 正确分类 |
|---|---|---|
| 单个碰撞轴 | 只是一次 CollisionResult 的维度；把 X/Y 或主次轴各自升格会复制提交和顺序责任 | 实例字段/结果值 |
| 单个 AABB 查询 | 是几何算法或 Query 的一次调用，不拥有世界状态、生命周期或提交权 | 纯 Query/算法 |
| 单个 Collider 字段 | width、height、offset 等共同描述一个碰撞形状；单字段没有独立 owner 或行为边界 | CollisionShape 的字段；组件为 status: proposed |
| LiquidRenderer | 只负责液体显示或客户端投影，不拥有液体传播或空间碰撞事实 | Adapter/Projection；不属于 SpatialSimulation 权威写集 |
| SceneMetrics | 屏幕、场景或诊断缓存不能成为权威碰撞事实 | 缓存/Projection |
| 传送带缓存 | _cacheForConveyorBelts 是计算缓存，不是跨 tick 的世界事实 | 受生命周期约束的局部缓存 |
| Center、Hitbox、边缘点 | 由位置、尺寸和 offset 派生，每次可重新计算 | 派生几何值 |
| Collision.honey、stair 等单个静态 flag | 是旧实现的隐式调用上下文，拆分的目标是消除其共享副作用，不是把每个 flag 变成新子系统 | 显式结果字段或兼容适配状态 |

## 18. 兼容策略和行为保持风险

兼容工作必须以“先显式记录旧行为，再逐步替换写入边界”为原则。以下风险不应在没有 focused verifier 和 Integration Review 的情况下被隐藏：

1. Collision.WetCollision、SlopeCollision、TileCollision 的静态 flags 可能被多个实体调用共享；直接改成并行或重排 System 可能改变坡度、湿状态、上下阻挡和台阶结果。
2. GetWaterLine、WalkDownSlope、TileCollision、StepDown 在空 Tile 情况下可能 new Tile()；把这些方法包装成“只读 Query”而不迁移该副作用，会造成世界状态差异。
3. Player、NPC、Projectile 的湿/干路径不同。Player 有浮力、装备、坐骑、输入和绳索等策略；NPC 有 collideX、collideY、AI 和平台行为；Projectile 有 tileCollide、ignoreWater、correctSlopeCollision、wetVelocity、extra update 和 Tile 回调。通用空间域不能复制这些专用策略。
4. oldPosition 和 oldVelocity 既可能是普通 tick 历史，也可能与 Projectile extra update 相关。统一成一个“上一帧”字段可能改变回调、插值和碰撞前速度观察结果。
5. Main.DoUpdateInWorld 的 Player → NPC → Projectile → Item → 世界更新顺序是当前事实，但不能简单当作 ECS 规则；如果新调度改变实体之间的可见状态、传送、门、平台或液体快照点，行为会漂移。
6. TileCoordinate 的重复定义和 EntityReference 的共享 owner 未统一前，移动结果、Tile view、网络投影和关系查询可能使用不兼容坐标/引用值。
7. TouchedTiles 是每 tick 的派生接触还是持久化/网络事实尚未决定；两种选择会改变内存生命周期、网络带宽和回放语义。
8. 液体接触读取若直接读取正在写入的 Main.tile，就会把 LiquidSimulation 的提交时序泄漏进 SpatialSimulation；必须定义快照或只读 projection 的时间点。
9. 传送带、斜坡、台阶、平台、actuator、half-brick 的组合规则存在行为顺序敏感性；不能用单一“固体碰撞”结果替换所有旧分支。
10. 任何“兼容阶段保留静态 flags”的方案都必须把保留范围限制在隔离 Adapter，并对重入、并发、失败恢复和跨实体污染提供验证，不得把静态字段继续暴露为新公共契约。

本节只提出风险和兼容策略方向，不证明任何新设计已达到行为等价。

## 19. 未决问题、evidence-gap 和 blocking-decision

### 19.1 Evidence gaps

| 稳定 ID | 缺口 | 影响 | 状态 |
|---|---|---|---|
| EG-01 | Version4 私有 Collision 方法的全部调用上下文、重入条件和静态字段清理时机尚未形成完整调用图 | 无法安全决定纯 Query 的最终切分和并发规则 | evidence-gap |
| EG-02 | 空 Tile 自动创建的所有触发条件及其对 Main.tile 数组的确切替换语义未做运行时验证 | 无法证明只读 Tile view 与旧行为完全一致 | evidence-gap |
| EG-03 | Player/NPC/Projectile/Item 全部位置写入点和后续读者未完成全量图谱 | 无法在本报告中裁决统一提交者 | evidence-gap |
| EG-04 | Projectile extra-update 与 MotionHistory 的完整生命周期尚未对照 focused verifier | 可能改变旧速度、位置历史和碰撞回调语义 | evidence-gap |
| EG-05 | NLTX 两个 TileCoordinate 的依赖图和外部 API 影响尚未裁决 | 不能安全合并、移动或指定共享 owner | unresolved |
| EG-06 | 液体接触快照和液体传播提交的 tick 对齐点尚未定义 | 可能出现实体看到半更新液体状态 | unresolved |
| EG-07 | 当前 Dome 局部碰撞路径尚未通过 focused verifier | 不能把源码接入写成通过或完整实现 | not-run |
| EG-08 | 本地 Space Station 14 缺失 RobustToolbox 物理底层实现 | 无直接对应物理实现证据 | missing |

### 19.2 Blocking decisions

以下决策必须交由 Integration Review，本报告不自行裁决。每项给出可选方案和主要影响。

| 稳定 ID | 决策问题 | 选项 | 影响 | 当前状态 |
|---|---|---|---|---|
| BD-01 | Position 与 Velocity 是否统一由 MovementState 持有 | A：统一 MovementState；B：继续分别由现有 LocationComponent/VelocityComponent 持有；C：实体域持有、SpatialSimulation 只消费快照 | A 简化提交契约但有迁移和共享组件影响；B 保持当前拆分但需保证双字段原子提交；C 最保守但 Query/Commit 适配复杂 | blocking-decision |
| BD-02 | TileCoordinate、WorldSectionId、SpatialSpaceId、EntityReference 的共享 owner | A：放入统一空间/关系基础层；B：按 WorldStorage、WorldInteraction、Relationships 分域并以 Adapter 连接；C：短期保留重复类型并建立显式转换 | A 易访问但扩大共享层；B 边界清晰但有转换成本；C 风险最低但容易形成不一致 | blocking-decision；crossSubsystemOwner: integration-review |
| BD-03 | LiquidContact 由谁生成 | A：SpatialSimulation 通过只读 LiquidContactQuery 生成并写实体派生结果；B：LiquidSimulation 生成接触结果，SpatialSimulation 只读取 projection；C：LiquidSimulation 给出原始视图，实体域自行派生 | A 便于统一实体碰撞视图但扩大空间域写入；B 保护液体 owner 但跨域时序更重要；C 保持域自治但结果可能分散 | blocking-decision |
| BD-04 | Player/NPC/Projectile 的位置写入方式 | A：统一 MovementCommitSystem；B：各实体域保留提交 System，统一使用 CollisionCommitPort；C：按实体类型分层，通用移动只提交候选结果 | A 写者唯一且易审计；B 保留旧行为但需防双写；C 平衡迁移成本和统一性但调度复杂 | blocking-decision |
| BD-05 | CollisionResult.TouchedTiles 生命周期 | A：每 tick 派生并在 tick 末丢弃；B：按需持久化/网络复制；C：只保留摘要，把完整 Tile 集留在诊断流 | A 内存和语义最简单；B 支持回放/客户端但增加协议和带宽；C 降低成本但损失诊断细节 | blocking-decision |
| BD-06 | 传送、坐骑、矿车、绳索、门、平台和 Projectile-specific policy 的提交顺序 | A：在实体域策略阶段完成后统一空间提交；B：保留各域旧顺序，用显式 phase barrier 连接；C：由 Integration Scheduler 对每类动作声明优先级 | A 简洁但迁移风险大；B 行为保持较好但边界复杂；C 可审计但需要新的调度契约 | blocking-decision |
| BD-07 | Version4 静态 flags 和空 Tile 自动创建如何兼容 | A：兼容阶段由隔离 Adapter 保留并记录；B：立即改为显式结果和 WorldStorage command；C：分阶段，先保留 flags、先迁移 Tile 创建为显式命令 | A 风险最低但延续隐式状态；B 纯度最好但回归风险高；C 平衡但需要双路径 verifier | blocking-decision |

## 20. Integration Handoff

~~~text
subsystemId: SpatialSimulation
taskNumber: 09
reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-spatial-simulation-public-decomposition.md

evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
~~~

### 20.1 confirmedOwners

- Version4 实体的当前空间位置、速度和历史写入仍由 Player、NPC、Projectile、Item 各自更新流程承担；统一 ECS owner 尚未裁决。
- Tile 的液体数量/类型、传播、合并、删除、buffer、网络变更集合和 Tile 写集由 Liquid 流程及 WorldStorage/WorldGen 相关边界承担；SpatialSimulation 不是 owner。
- Tile 的 type、wall、active、slope、half-brick、actuator 等世界事实由 WorldStorage/WorldInteractionAndStructures 相关边界承担；SpatialSimulation 只读。
- 通用碰撞、斜坡、平台、台阶、传送带和空间资格属于 SpatialSimulation 的候选权威计算责任，但当前 NLTX 尚未证明完整接入。
- Player 输入和专用移动策略、NPC AI 和平台策略、Projectile 生命周期和专用碰撞回调不属于 SpatialSimulation。

### 20.2 proposedTypes

以下类型全部为 status: proposed，尚未创建：

- MovementState；
- CollisionShape；
- SpatialReference；
- MotionHistory；
- LiquidContact；
- CollisionPolicy；
- CollisionResult；
- ContactQuery；
- SpatialQualificationQuery；
- LiquidContactQuery；
- TileNeighborhoodQuery；
- MovementSystem；
- CollisionResolutionSystem；
- LiquidContactSystem；
- MovementCommitSystem；
- SpatialProjectionSystem；
- CollisionCommand；
- CollisionCommandBuffer；
- CollisionCommitPort；
- TileReadPort；
- LiquidContactReadPort；
- WorldStorageTileAdapter；
- LiquidContactAdapter；
- SpatialReplicationProjection。

### 20.3 sharedTypesForIntegrationReview

以下共享类型不得由本报告指定最终 owner：

- TileCoordinate：当前至少有 src/WorldStorage/TileCoordinate.cs 和 src/WorldInteraction/Tiles/TileCoordinate.cs 两个定义；crossSubsystemOwner: integration-review；
- WorldSectionId：crossSubsystemOwner: integration-review；
- SpatialSpaceId：crossSubsystemOwner: integration-review；
- EntityReference：当前位于 src/Relationships/EntityReference.cs，crossSubsystemOwner: integration-review；
- CollisionResult.TouchedTiles 的元素和序列化形状：crossSubsystemOwner: integration-review；
- LiquidContact 的生成方和跨域传输形状：crossSubsystemOwner: integration-review。

### 20.4 crossSubsystemReaders

- PlayerGameplay 读取碰撞结果、grounded、平台/斜坡和液体接触以形成 Player 专用行为；
- NpcAndTownSimulation 读取碰撞轴、平台策略结果、斜坡/台阶结果和液体接触；
- ProjectileSimulation 读取 Tile 碰撞、斜坡、液体接触和碰撞前速度，以执行 tileCollide、ignoreWater、correctSlopeCollision 等专用策略；
- WorldInteractionAndStructures 读取或消费显式 Tile command，但不从 Collision 的隐式静态状态读取；
- LiquidSimulation 只向 SpatialSimulation 暴露只读接触或快照，不接受空间域对液体数量/类型的写入；
- 网络/客户端/诊断层读取 SpatialProjectionSystem 生成的只读快照。

### 20.5 crossSubsystemWriters

- PlayerGameplay、NpcAndTownSimulation、ProjectileSimulation 是实体专用策略和可能的移动提交参与者；最终位置写入模式由 BD-04 裁决；
- WorldStorage/WorldInteractionAndStructures 写入 Tile 结构；
- LiquidSimulation/WorldStorage 写入液体数量、类型、传播结果和液体网络/存档集合；
- 网络/客户端不应写入权威 SpatialSimulation 状态；客户端预测只能作为非权威输入或投影；
- SpatialSimulation proposed MovementCommitSystem 若被选择，只能通过显式 CollisionCommitPort 成为唯一位置/速度写入者，且该类型为 status: proposed。

### 20.6 orderingConstraints

- 必须先采样历史，再读取只读世界视图和实体策略，再计算碰撞，再提交位置/速度/历史；
- 液体接触读取必须绑定明确的 LiquidSimulation snapshot tick；
- Collision Query 不得写 Tile、液体、网络或存档；
- Tile 结构命令和移动命令的先后、失败和重试由 BD-06、BD-07 裁决；
- 不得依赖文件顺序、数组枚举顺序或静态 Collision flags 作为 ECS 调度契约；
- 同一实体同一 tick 的位置/速度只能由一个明确提交边界最终写入，或由版本检查保证多阶段写入可审计。

### 20.7 boundaryChallenges

- Version4 Collision 将纯计算、Tile 读取、液体查询、静态共享状态、缓存和潜在 Tile 写入混在一起；
- Player、NPC、Projectile 的移动和提交行为并不相同，通用 MovementSystem 不能吞并实体专用策略；
- LiquidComponent 已位于共享 Entity 目录，但液体权威状态必须留在 LiquidSimulation；
- 当前存在重复 TileCoordinate，且 EntityReference、section/space ID 的共享 owner 未统一；
- Dome 的局部执行路径直接修改 transform、velocity、physics，尚未形成完整 Query/Command/Commit 契约；
- 静态 flags、空 Tile 创建、斜坡/台阶/传送带和高速分步都可能受调用顺序影响；
- TouchedTiles、液体接触和碰撞历史的持久化/网络生命周期未裁决。

### 20.8 evidenceGaps

- EG-01 至 EG-08 全部保留；特别是完整调用图、空 Tile 写入语义、Projectile extra-update、双 TileCoordinate 依赖图和 Liquid snapshot tick 尚未补齐；
- 完整参考源码只提供 version-drift 补证，不能替代 Version4 行为核对；
- Space Station 14 缺少 RobustToolbox 物理底层，标记 missing，不能作为直接实现依据；
- 当前 focused verifier 未运行，所有验证结论均为 not-run。

### 20.9 blockingDecisions

- BD-01：Position/Velocity 归属；
- BD-02：坐标、空间和实体引用共享 owner，crossSubsystemOwner: integration-review；
- BD-03：LiquidContact 生成方；
- BD-04：Player/NPC/Projectile 位置提交者；
- BD-05：TouchedTiles 生命周期；
- BD-06：传送、坐骑、矿车、绳索、门、平台和 Projectile-specific policy 顺序；
- BD-07：静态 flags 和空 Tile 自动创建兼容策略。

### 20.10 notImplemented

- 未创建 MovementState、CollisionShape、SpatialReference、MotionHistory、LiquidContact、CollisionPolicy 或新的 CollisionResult；
- 未创建 Contact/Qualification/Liquid/Tile Query；
- 未创建 Movement、CollisionResolution、LiquidContact、MovementCommit 或 Projection System；
- 未创建 CollisionCommand、CommitPort、TileReadPort、LiquidContactReadPort、Adapter 或 Projection；
- 未修改根 src、dome/src、Test、项目文件或生产代码；
- 未执行迁移、行为替换、API 兼容层、存档迁移或网络协议变更；
- 未运行编译、测试、restore 或 coverage checker。

### 20.11 verifierPlan

- 先根据 BUILD-CONCURRENCY-1 检查活动 dotnet.exe/csc.exe，确认唯一 owner；
- 只针对 D:\TRbackup\NLTX\dome\Test\Terraria.Dome.PlayerPhysics.Verification\Terraria.Dome.PlayerPhysics.Verification.csproj 使用仓库串行包装脚本；
- 先确保受影响项目 artifact 位于 Build/bin/，再以 --no-build --no-restore 运行 focused verifier；
- 验证纯 Query 无静态 flags、Tile 写入或隐式 new Tile()；
- 验证固体、half-brick、斜坡、平台、actuator、平台穿越、反重力、高速分步、历史和液体接触；
- 验证重复调用、跨实体调用、版本冲突、命令顺序和失败行为；
- 验证 LiquidContact 读取不改变液体数量、类型、传播、提交或网络集合；
- 在 Integration Review 解决 BD-01 至 BD-07 后，再决定是否增加网络/存档/回放 verifier；
- 当前执行状态保持 verificationStatus: not-run。

## 21. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。
