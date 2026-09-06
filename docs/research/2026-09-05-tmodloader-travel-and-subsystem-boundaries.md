# tModLoader 与 Version4 子系统补充审查：旅行与传送

## 结论

在既有《Version4 权威游戏模拟子系统审查报告》的十四个初始责任面之外，当前代码和
本地 tModLoader API 文档共同证明了一个**应在下一轮纳入清单、但当前不继续拆小的候选
子系统**：`TeleportationAndTravel`（旅行与传送）。

它不是一个 Pylon、Portal、冷却字段或单个网络包的集合。它负责传送端点的可用性、旅行请求
的权威资格判定、位置迁移、传送冷却，以及由已提交旅行结果驱动的复制通知。它与
`WorldStorage`、`IntentAndInteraction`、`PlayerGameplay`、`NpcAndTownSimulation`、
`WorldGenerationAndEcology` 和 `ExternalBoundaries` 交接，但不接管这些系统的长期状态。

主审查报告尚未出现 `Teleportation`、`Pylon` 或 `Travel` 名称；本次检查发现 NLTX 已经有
单独的 `src/Teleportation/Terraria.Teleportation.csproj`，因此这是“报告遗漏的已识别领域”，
不是从扩展 API 臆测出的新目录需求。

## 候选：TeleportationAndTravel

### 子系统成立依据

| 判定条件 | 当前证据 | 审查判断 |
| --- | --- | --- |
| 独立责任 | 传送点发现、端点配对、资格判定、位置迁移和冷却均围绕一次旅行事务，而不是一般 Tile 操作或一般移动 | 成立 |
| 独立状态和生命周期 | NLTX 有独立 Teleportation 项目；`PortalNetworkState` 持有端点配对与更新时间，`TeleportCooldownState` 持有来源、剩余 tick 和起始 tick；WorldStorage 另有 Pylon 当前/上一快照、刷新冷却与修订号 | 成立，但尚无执行器 |
| 可辨识写集 | 成功旅行只能提交玩家位置和冷却；Pylon 注册表只能经 WorldStorage 的受控刷新/提交更新；Tile、TileEntity、NPC、世界事件只能作为读取条件 | 成立，必须强制隔离 |
| 独立阶段与验证 | Version4 在世界时间/事件更新期间单独调用 `PylonSystem.Update`；其列表刷新、变更 diff、加入玩家同步和旅行请求是可独立验证的生命周期 | 成立 |

### 应有边界

`TeleportationAndTravel` 的输入是已经验证身份的旅行意图，例如 `RequestTravel(player, source,
destination)`；它不接受客户端位置作为事实。它读取：

- `WorldStorage` 的 TileEntity/Tile 只读视图和 Pylon 注册表；
- `PlayerGameplay` 的玩家存活、交互距离和冷却状态；
- `NpcAndTownSimulation` 的附近 NPC 计数；
- `WorldSession` 的全局危险/事件和世界进度；
- `WorldGenerationAndEcology` 提供的目的地 biome/场景分类查询；
- `ContentCatalog` 的端点、物品或机制定义。

它在资格全部成立后仅输出一个原子的旅行提交：更新玩家世界位置、写入传送冷却，并产生可供
`ExternalBoundaries` 投影的旅行结果。Pylon 列表增删的网络通知是投影，不是该系统以网络包
直接改写权威状态的理由。

不应把下列对象提升为一级子系统：`PortalEndpointComponent`、`TeleportCooldownState`、
`PylonRegistryEntry`、单个 `ValidTeleportCheck_*` Hook、`NetTeleportPylonModule` 子包，或
Pylon 的视觉粒子。它们分别是状态、资格规则、协议或表现细节。

### Version4 代码证据

- `D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:13-23` 声明独立的
  `TeleportPylonsSystem`，持有当前/上一 Pylon 列表、刷新冷却和专用 `SceneMetrics`。
- 同文件 `:25-35` 让该状态通过独立 `Update` 生命周期刷新；`:45-71` 从
  `TileEntity.ByPosition` 构建注册表、比较旧新快照，并仅将增删差异广播出去；`:74-90` 明确
  reset 与玩家加入时的同步生命周期。
- `D:\TRbackup\Version4\Terraria\Main.cs:3364` 创建该系统，而 `:13109-13119` 将
  `PylonSystem.Update()` 放在世界时间与全局事件更新序列中。这证明它是运行时责任，而不是
  Tile 类型定义。
- `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETeleportationPylon.cs:26-79`
  表明宿主多 Tile 损坏时，TileEntity 删除、物品掉落和 Tile 销毁是一个结构变更过程；`:108-117`
  则将 TileEntity 存活性绑定到 Tile 类型与 frame。该结构属于 WorldStorage/WorldInteraction，
  旅行系统只读取其已提交结果。
- `D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetTeleportPylonModule.cs:7-25`
  定义 Pylon 增加、移除和玩家请求三类协议值。它证明有明确的适配面，但不能充当权威写入根。

### tModLoader API 交叉证据

- `D:\TRbackup\tmodloader-api-docs-stable\class_mod_pylon.html:700-714` 把 Pylon 传送描述为
  有序资格流程：玩家临近、目的地附近 NPC、全局危险、进度门控、biome 条件、附近源端点，
  然后才修改落点。它是一个跨领域的旅行判定，不是单一 Tile hook。
- 同文件 `:1331-1333` 要求危险判定从 Boss/事件等世界状态读取；`:1344-1384` 要求目的地的
  biome 判定读取目的地位置的 `SceneMetrics`，而非玩家当前位置。这支持将资格计算放在旅行
  子系统并通过只读查询取得跨领域事实。
- `D:\TRbackup\tmodloader-api-docs-stable\class_mod_tile_entity.html:538-540` 指出 TileEntity
  在世界加载和服务器放置时验证其宿主 Tile，而 Tile 被移除时必须显式清理实体。该 API 证据
  进一步支持“端点结构归 WorldStorage/WorldInteraction、旅行资格归 Travel”的所有权划分。

### 当前 NLTX 证据与缺口

- `src/Teleportation/Terraria.Teleportation.csproj:1-10` 是独立 `net10.0` 项目，目前仅依赖
  Relationships 与 WorldInteraction，说明目录已形成领域级编译边界。
- `src/Teleportation/PortalNetworkState.cs:6-14` 和
  `src/Teleportation/TeleportCooldownState.cs:3-8` 已表达端点配对与冷却生命周期。
- `src/WorldStorage/PylonRegistryState.cs:3-10`、
  `src/WorldStorage/PylonRegistryEntry.cs:3-10` 已表达 Pylon 快照、刷新门控、修订号及
  TileEntity 身份绑定。
- 但在当前 `src/` 与 `Test/` 搜索中，没有旅行请求、资格决策、位置提交、注册表刷新器、
  网络投影或 focused verifier。因此它应标为“已识别、未形成可运行闭环”，不能标为已实现。

## 相邻材料的去重结论

以下材料被审查，但不再新建一级子系统。它们是已有子系统的证据或只读查询，而非另一套
独立的权威写集。

| 材料 | 证据 | 归属和原因 |
| --- | --- | --- |
| `ModSystem` 的世界生成、清除、加载、存档与世界 tick hooks | `class_mod_system.html:120-122`、`:163-164`、`:186-192`、`:258-273`、`:322-335` | 分别补强已有 `WorldGenerationAndEcology`、`WorldSession`、`RuntimeComposition` 与 `ExternalBoundaries`；它是生命周期扩展面，不拥有另一个领域写集。 |
| `ModBiome` / `SceneMetrics` | `class_mod_biome.html:105-109` 的 biome 激活查询；Pylon 的目的地 `SceneMetrics` 证据见上文 | 是 `WorldGenerationAndEcology` 向 Spawn、Craft、Travel 等系统提供的确定性环境查询；Scene 特效本身是表现，不能升格。 |
| `ModTile` / `ModTileEntity` | `class_mod_tile_entity.html:374-375`、`:538-540`、`:670-686` | 宿主 Tile、实体存活、每 tick 更新和同步边界已归入 `WorldStorage` 与 `SpatialSimulation` 内的 `WorldInteractionAndStructures`；按每种 TileEntity 再拆会违反初次报告的子系统粒度。 |
| `ModItem` / `GlobalItem` / `Recipe` / 内容注册 | `class_recipe.html:99-103` 规定配方在加载期注册；`:124-154` 显示配方条件与消耗规则 | 静态定义与注册归 `ContentCatalog`，玩家物品扣除、产出与失败回滚归 `ItemContainerAndEconomy`。没有独立的第三个权威状态根。 |

## 对主审查报告的最小修订建议

本笔记不修改主报告。下一次集中修订时，可在“领域模拟”层增加一个 `TeleportationAndTravel`
责任面，并将其描述为：

> 负责端点网络与旅行资格的确定性判定，提交玩家位置迁移与冷却，并对已提交端点集合生成
> 复制投影；不拥有 Tile/TileEntity 存储、环境分类、NPC/事件状态或视觉效果。

这会使初始责任面从十四个变为十五个。只有在实现资格决策、原子提交和 focused verifier 后，
才有理由再讨论内部的 Portal、Pylon 或机制传送策略。
