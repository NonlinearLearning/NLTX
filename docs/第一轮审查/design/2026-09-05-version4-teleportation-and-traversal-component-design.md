# TeleportationAndTraversal Component Design

## 1. 设计元数据

subsystemId: TeleportationAndTraversal
taskNumber: 14
sourceReport: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-teleportation-and-traversal-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-teleportation-and-traversal-component-design.md
designScope: component-only
designStatus: decision-required
evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
selectionMethod: current-session-explicit-report; no research-directory scan
sourceReportMetadata: subsystemId/taskNumber/reportPath matched; reportPath confirmed in source report Integration Handoff

本文件只把当前会话已经产生的 TeleportationAndTraversal 研究报告整理为组件级设计。输入报告路径与报告内 Integration Handoff 的 reportPath 一致，因此没有发生输入文件路径替换，也没有读取其他研究报告。

designStatus: decision-required 的原因是：Pylon registry 的跨域 owner、Portal identity 的归属、两个冷却状态是否能够在某些实体上共存，以及部分默认值/生命周期仍不能从 Version4 基线唯一确定。组件候选可以供架构评审使用，但不能被视为已经锁定的实现合同。

## 2. 设计范围与排除范围

### 2.1 本文件回答的问题

本文件只回答以下组件级问题：

- 哪些持续状态应由一个内聚 Component 保存；
- 每个字段的类型、默认值、状态分类和不变量；
- Component 所在的实体、世界或实体集合范围；
- Component 的创建、初始化、更新和清理生命周期；
- 端点、配对关系、registry、subject cooldown 之间的组合关系；
- 实体 ID、TileEntity ID、网络 ID、Tile 坐标和外部引用的字段归类；
- 当前 NLTX 状态与候选组件的覆盖关系；
- 哪些组件 owner 仍需整合裁决。

### 2.2 组件范围

最终组件清单保留五个概念：

1. PylonRegistryComponent：世界范围的已提交 Pylon 端点集合、revision 和 registry 刷新缓存。
2. PortalEndpointComponent：单个 Portal 端点的几何与宿主支持事实。
3. PortalLinkStateComponent：Portal 端点之间的配对关系。
4. TeleportCooldownStateComponent：旅行主体的通用离散旅行冷却状态。
5. PortalTraversalCooldownStateComponent：Portal 穿越特有的主体冷却、最近端点和分组关系。

其中 PortalEndpointComponent 与 PortalTraversalCooldownStateComponent 在当前 NLTX 已有局部实现，状态为 partial；其他目标形状或目标路径仍是 status: proposed，除非表格明确说明已有映射。

### 2.3 排除范围

以下内容不在本文件中设计为组件或运行时结构：

- Pylon TileEntity 的宿主、Tile 合法性、多 Tile 损坏清理、掉落和 Tile 销毁；这些是 WorldStorage / WorldInteraction 的结构状态。
- Portal Projectile 的完整生命周期和放置几何；本文件只保存端点实体所需的持续事实。
- Player/NPC 的位置、速度、实体生命周期和主体身份；这些状态仍由其所属实体领域拥有，本文件不复制镜像字段。
- 玩家资格计算所需的 NPC 数量、Danger、Temple 进度、Biome 和世界事实；这些是外部只读事实，不成为组件字段。
- 短生命周期的请求、资格结果、落点候选和旅行结果消息；当前没有证据证明它们需要作为跨 Tick 持久组件保存。
- 网络包、存档格式、客户端表现、颜色粒子、声音、日志和 UI；它们不是权威组件状态。
- LocationComponent、SpatialReferenceComponent 的位置空间字段；旅行组件只建立写入边界关系，不重复持有位置。
- 任何完整运行时类实现、文件创建步骤或迁移操作。

### 2.4 设计原则

- 世界 registry、单个端点、端点关系和主体冷却具有不同实体范围、生命周期和失效条件，因此不合并成一个巨型组件。
- registry 当前列表与用于差异比较的上一份列表虽然分类不同，但由同一世界 owner 创建、替换、清理，且共同维护差异不变量；在没有独立消费者证据时保持在同一个世界组件中。
- Portal 几何与 Portal 配对关系拆开：端点失效不应要求几何字段和 peer 关系拥有相同的更新粒度。
- registry 刷新冷却不能放入任何主体旅行冷却组件。
- 表现兼容字段不因为被旅行入口写入就自动成为 Teleportation 权威组件。
- 任何跨 WorldStorage、Teleportation、PlayerGameplay、NpcAndTownSimulation 或 SpatialSimulation 的字段、ID、关系，均保留 crossSubsystemOwner: integration-review。

## 3. 组件设计依据

### 3.1 来源优先级

| 来源 | 实际读取内容 | 在本文件中的作用 | 状态规则 |
|---|---|---|---|
| Version4 | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs、PortalHelper.cs、TETeleportationPylon.cs、NetTeleportPylonModule.cs、Terraria\Main.cs、Player.cs、NPC.cs、Collision.cs、TeleportPylonInfo.cs | 真实字段、实际默认初始化、实体范围、生命周期和写入事实 | 首要基线；只把实际存在成员标为 confirmed 或 partial |
| 完整可编译参考 | 同路径的 Pylon、Portal、网络模块、TileEntity 和 Player 文件 | 仅补充 Version4 已存在文件中缺失/空实现的成员差异 | full-reference-supplemented、version-drift，不能升级 Version4 缺失成员 |
| tModLoader v2026.07 | index.html、class_mod_pylon.html、class_mod_tile_entity.html | 交叉核对公开生命周期、Pylon 资格阶段和 TileEntity 宿主语义 | 不能替代 Version4 私有实现 |
| Space Station 14 | PortalComponent.cs、PortalTimeoutComponent.cs、TeleportLocationsComponent.cs、SharedPortalSystem.cs、SharedTeleportLocationsSystem.cs 及服务端对应文件 | 只参考组件内聚、关系字段、网络快照粒度和冷却状态的组织方式 | 无 Terraria 直接对应行为证据 |
| 当前 NLTX | src\Teleportation、src\WorldStorage、src\Share\Entity\Components、dome\src\...\Teleportation、Test 和 dome\Test | 确认当前已存在类型、字段和测试覆盖 | 现有实现标 existing 或 partial；无引用标 missing |

### 3.2 已重新定位的 Version4 事实

| 事实 | 实际证据 | 组件级含义 | evidenceStatus |
|---|---|---|---|
| Pylon registry 持有当前/旧列表、刷新冷却和 SceneMetrics | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:15-23 | 当前/上一列表和 registry 刷新状态属于世界范围；SceneMetrics 不应进入组件字段 | confirmed |
| Pylon registry 从 TileEntity.ByPosition 重建当前列表并广播差异 | TeleportPylonsSystem.cs:47-71 | registry 保存已提交端点快照；TileEntity 本体不被复制为 Travel component | confirmed |
| Pylon registry reset 和玩家加入快照 | TeleportPylonsSystem.cs:76-90 | world reset 清空 registry；join 读取快照但不改变组件 owner | confirmed |
| Pylon TeleportPylonInfo 只有 Tile 位置和 Pylon 类型字段 | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonInfo.cs:6-9 | Version4 网络/列表身份以位置和类型为主；NLTX 的 TileEntityId 是补充候选而非已确认等价身份 | confirmed |
| Portal 配对缓存和 Player/NPC cooldown 数组由 PortalHelper 静态持有 | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:12-24 | 配对关系与主体冷却不是一个字段集合；冷却不能并入世界 registry | confirmed |
| Portal refresh 清空配对、递减两类 cooldown、扫描 1000 个 Projectile | PortalHelper.cs:64-100 | endpoint/link 生命周期与主体 cooldown 共享旧入口但不共享 Component owner | confirmed |
| Portal traversal 写入实体位置、速度、颜色和 cooldown | PortalHelper.cs:109-215 | 组件只保存持续事实；位置、速度和表现字段不在本文件重复建模 | confirmed |
| Portal 几何辅助在 Version4 中存在空实现 | PortalHelper.cs:250-260、282-291 | 出口几何、支撑细节和默认值不能由完整参考静默补入 Version4 | version-drift |
| Player 有 Portal physics 和 Pylon style 字段 | D:\TRbackup\Version4\Terraria\Player.cs:2376-2382、3124-3134、15292-15318 | 这些是 PlayerGameplay 兼容/表现状态，不自动迁移成 Travel component | confirmed |
| Player Teleport 写位置并设置 Portal/Pylon 兼容字段 | Player.cs:21731-21785 | LocationComponent 与 Player 兼容字段保持原 owner；本设计不复制位置 | confirmed |
| NPC 有 Portal color、Teleport 和 Portal slot reset | D:\TRbackup\Version4\Terraria\NPC.cs:6441、67306-67330、8125-8130、79460-79468 | NPC 具有独立主体冷却/兼容字段生命周期；不能复用 Player-only 字段 | confirmed |

### 3.3 版本差异与交叉证据

| 主题 | 直接证据 | 设计处理 |
|---|---|---|
| TeleportPylonInfo.Equals | Version4 TeleportPylonInfo.cs:6-9 的 Equals 为空实现；完整参考同路径实现位置与类型比较 | registry diff 的唯一性和重复项语义标 version-drift；组件不声称 Version4 已有完整 equality 行为 |
| Pylon 请求和资格成员 | Version4 TeleportPylonsSystem.cs 未发现完整参考中的 HandleTeleportRequest、附近 Pylon 和资格成员；完整参考同路径 :114-350 存在 | 资格字段不落入组件；Pylon registry 只保存端点事实；完整资格行为是组件外 evidence-gap |
| Player Portal 调用链 | Version4 Player.cs 未找到 TryPortalJumping；Version4 NPC.cs:79460-79468 有 NPC 调用；完整参考 Player.cs 有对应调用 | 不为 Player 组件添加未经 Version4 确认的 Portal 触发字段；标 version-drift |
| TileEntity 宿主 | Version4 TETeleportationPylon.cs:5-11、26-85、108-150 负责结构校验、损坏清理、放置和类型读取 | Pylon host 不创建 Teleportation component；registry 只消费已提交的端点记录 |
| 网络模块 | Version4 NetTeleportPylonModule.cs:7-28 只有三类包枚举、增删序列化，Deserialize 为空；完整参考 :28-87 补充请求序列化/分派 | 包字段不是组件身份；网络请求状态不进入权威组件 |

### 3.4 tModLoader 页面证据

本地首页为 D:\TRbackup\tmodloader-api-docs-stable\index.html，标题为 tModLoader: Main Page，页面版本字段为 v2026.07。类型页实际标题和锚点如下：

| 页面和锚点 | 原文支持的边界 | 组件级用途 |
|---|---|---|
| D:\TRbackup\tmodloader-api-docs-stable\class_mod_pylon.html:700-714 | ModPylon 说明 ValidTeleportCheck 的阶段顺序，从玩家接近、目的地 NPC 数、Danger、Temple、Biome 到附近 Pylon 和最终位置修改 | 资格阶段不是端点组件字段；Pylon 组件只保存端点事实 |
| class_mod_pylon.html#a20282cb665f2cea48d22361c291878c0，页面区段 :1308-1333 | ValidTeleportCheck_AnyDanger 是资格阶段之一 | Danger 不写入 Pylon registry 或主体 cooldown |
| class_mod_pylon.html#aa8bf18a82194c5ed1ab819f84f6070d4，页面区段 :1344-1384 | ValidTeleportCheck_BiomeRequirements 读取 Pylon 信息和 SceneMetrics | SceneMetrics 不能作为 Pylon component 字段 |
| class_mod_pylon.html#a38ce636c2242724bd79de5d5b80923e9，页面区段 :1391-1441 | 目的地 post-check 是扩展资格边界 | Hook 不构成持续组件 |
| class_mod_pylon.html#a52fccf10176d043fe9ef054cc31ea720，页面区段 :1444-1500 | 附近 Pylon post-check 与目的地检查分开 | 端点集合与主体关系不能合并为一个实体组件 |
| class_mod_pylon.html#a40c779c732f239e1f79d63d3494b298a，页面区段 :1504-1539 | NPC count 资格接收 TeleportPylonInfo 和必要 NPC 数 | NPC 数不属于 registry 组件持久字段 |
| D:\TRbackup\tmodloader-api-docs-stable\class_mod_tile_entity.html#a8415b71088157d5fcf4ed024cffe868d，页面区段 :505-540 | IsTileValidForEntity 负责宿主 Tile 存活语义；Tile 被销毁时需要显式清理 TileEntity | Pylon TileEntity 生命周期留在结构 owner，不能由 Travel component 代管 |

### 3.5 Space Station 14 组织参考

Space Station 14 无 Terraria 直接对应证据；以下只参考组件粒度、关系字段、状态范围和失败时不写入的位置边界，不复制其代码、命名、目录结构或领域语义。

| 实际文件 | 读取到的组件级模式 | 对本设计的限制 |
|---|---|---|
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Teleportation\Components\PortalComponent.cs | Portal 配置字段集中在端点组件，link、随机和距离限制是组件数据 | Terraria 的 Portal 几何字段可以集中，但不能由此推出同名字段或相同行为 |
| ...\PortalTimeoutComponent.cs | 旅行后保存 EnteredPortal 以防止立即反向穿越 | 支持把最近端点关系视为独立状态；不证明 Terraria Version4 已有该字段 |
| ...\TeleportLocationsComponent.cs | 可网络同步的传送点集合和 NetEntity 关系字段 | 网络 ID 与权威端点身份分离；不能让包字段直接成为 Pylon identity |
| ...\SharedPortalSystem.cs | 端点组件、link 查询、cooldown 和坐标状态彼此可组合 | 只采纳“端点与关系拆开”的粒度，不引入外部运行时结构 |
| ...\SharedTeleportLocationsSystem.cs | 安全位置失败时不写入主体坐标，并把 delay 状态单独保存 | 支持失败不修改位置/冷却的组件不变量 |
| ...\Content.Server\Teleportation\PortalSystem.cs | 服务端日志字段不混入共享传送状态 | 日志、UI、音频和网络表现不进入权威组件 |

## 4. Version4 成员到 Component 归属表

表中 proposed Component 是组件设计归属，不表示对应源码已经创建。not-component 表示该成员有证据但不应成为 Teleportation 组件字段。

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| _pylons | TeleportPylonsSystem | 当前有效 Pylon 端点集合 | 权威快照候选 | world init、刷新、reset | PylonRegistryComponent.CurrentPylons | confirmed | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:15、47-62 |
| _pylonsOld | TeleportPylonsSystem | registry 差异比较的上一集合 | 派生比较缓存 | 与当前列表交换、清理 | PylonRegistryComponent.PreviousPylons | confirmed | TeleportPylonsSystem.cs:17、63-71 |
| _cooldownForUpdatingPylonsList | TeleportPylonsSystem | registry 刷新节流 | registry 缓存/生命周期状态 | Tick 递减、refresh、reset | PylonRegistryComponent.RefreshCooldownTicksRemaining | confirmed | TeleportPylonsSystem.cs:19、25-35、76-80 |
| _sceneMetrics | TeleportPylonsSystem | Pylon 资格可能读取的世界统计 | 外部事实，不是端点状态 | 世界/资格读取 | not-component | partial | TeleportPylonsSystem.cs:23；tModLoader class_mod_pylon.html:1344-1384 |
| TeleportPylonInfo.PositionInTiles | TeleportPylonInfo | Pylon 的 Tile 坐标 | registry 快照字段 | 端点有效期间 | PylonRegistryComponent.CurrentPylons[].Position | confirmed | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonInfo.cs:6-8 |
| TeleportPylonInfo.TypeOfPylon | TeleportPylonInfo | Pylon 类型 | registry 快照字段 | 端点有效期间 | PylonRegistryComponent.CurrentPylons[].Kind | confirmed | TeleportPylonInfo.cs:8-9 |
| PylonRegistryEntry.TileEntityId | NLTX PylonRegistryEntry | NLTX 候选 TileEntity 身份 | 关系/持久化候选 | 结构实体生命周期 | PylonRegistryComponent.CurrentPylons[].TileEntityId | unresolved | D:\TRbackup\NLTX\src\WorldStorage\PylonRegistryEntry.cs:3-7；Version4 TeleportPylonInfo.cs:6-9 无此字段 |
| PylonRegistryState.CurrentPylons | NLTX PylonRegistryState | 当前 Pylon 列表 | 权威候选 | world lifetime | PylonRegistryComponent.CurrentPylons | partial | D:\TRbackup\NLTX\src\WorldStorage\PylonRegistryState.cs:3-11 |
| PylonRegistryState.PreviousPylons | NLTX PylonRegistryState | 上一 Pylon 列表 | 差异缓存 | registry replace/reset | PylonRegistryComponent.PreviousPylons | partial | PylonRegistryState.cs:5-6 |
| PylonRegistryState.RefreshCooldownTicksRemaining | NLTX PylonRegistryState | registry 刷新剩余 ticks | registry 状态 | Tick/reset | PylonRegistryComponent.RefreshCooldownTicksRemaining | partial | PylonRegistryState.cs:8-10 |
| PylonRegistryState.Revision | NLTX PylonRegistryState | registry revision | 权威版本 | replace/reset | PylonRegistryComponent.Revision | partial | PylonRegistryState.cs:10-11 |
| FoundPortals | Version4 PortalHelper | owner 与 pair index 到 Projectile 的配对缓存 | 派生缓存 | 每次 Portal refresh 重建 | PortalLinkStateComponent.PeerEndpoint 的外部证据；不保留全局数组字段 | partial | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:12、57-99 |
| PortalCooldownForPlayers | Version4 PortalHelper | Player Portal traversal cooldown 数组 | 权威主体冷却候选 | 初始化、Tick 递减、成功穿越、slot 生命周期 | PortalTraversalCooldownStateComponent.RemainingTicks | confirmed | PortalHelper.cs:14、74-80、126-129、191-197 |
| PortalCooldownForNPCs | Version4 PortalHelper | NPC Portal traversal cooldown 数组 | 权威主体冷却候选 | 初始化、Tick 递减、成功穿越、NPC slot reset | PortalTraversalCooldownStateComponent.RemainingTicks | confirmed | PortalHelper.cs:16、81-87、128-129、198-207；NPC.cs:8125-8130 |
| anyPortalAtAll | Version4 PortalHelper | 是否存在完整 Portal pair 的快速判断 | 派生缓存 | Portal refresh 重建 | not-component | confirmed | PortalHelper.cs:24、68、94-97、113-116 |
| Angle | NLTX PortalEndpointComponent | Portal 端点角度 | 端点权威几何 | endpoint create/update/remove | PortalEndpointComponent.Angle | partial | D:\TRbackup\NLTX\src\Teleportation\PortalEndpointComponent.cs:6-12 |
| Direction | NLTX PortalEndpointComponent | Portal 端点方向 | 几何/兼容字段 | endpoint lifetime | PortalEndpointComponent.Direction | version-drift | NLTX PortalEndpointComponent.cs:8-10；dome counterpart uses sbyte and constructor default 1 |
| Form | NLTX PortalEndpointComponent | Portal 形式/Projectile form | 端点权威几何分类 | endpoint lifetime | PortalEndpointComponent.Form | partial | NLTX PortalEndpointComponent.cs:8-12；Version4 PortalHelper.cs:90-99、216-247 |
| PortalEntity | NLTX PortalEndpointComponent | 端点关联的 EntityReference | 实体关系 | endpoint create/remove | PortalEndpointComponent.PortalEntity | partial | NLTX PortalEndpointComponent.cs:11-17；EntityReference.cs:5-9 |
| SupportTile | NLTX PortalEndpointComponent | Portal 支撑 Tile 坐标 | 宿主关系 | endpoint validation/remove | PortalEndpointComponent.SupportTile | partial | NLTX PortalEndpointComponent.cs:12；Version4 PortalHelper.cs:334-418 |
| PortalNetworkState.FirstPortal/SecondPortal | NLTX PortalNetworkState | 一对 Portal 端点引用 | 关系状态 | pair create/invalidate | PortalLinkStateComponent.PeerEndpoint 与端点组合 | partial | D:\TRbackup\NLTX\src\Teleportation\PortalNetworkState.cs:6-14 |
| PortalNetworkState.FirstPortalSupport/SecondPortalSupport | NLTX PortalNetworkState | 两个端点支持 Tile | 端点宿主关系 | pair create/invalidate | PortalEndpointComponent.SupportTile | partial | PortalNetworkState.cs:8-14 |
| PortalNetworkState.IsComplete | NLTX PortalNetworkState | 两个端点是否同时存在 | 派生值 | relation refresh | PortalLinkStateComponent.IsPairComplete | partial | PortalNetworkState.cs:10 |
| PortalNetworkState.LastPortalColorIndex | NLTX PortalNetworkState | Portal 颜色兼容值 | 表现/兼容字段 | traversal result | not-component；继续由主体领域决定 | partial | PortalNetworkState.cs:11；Player.cs:2376 |
| PortalNetworkState.UpdatedAtTick | NLTX PortalNetworkState | pair state 更新时间 | 缓存/审计候选 | pair refresh | not-component until owner is decided | unresolved | PortalNetworkState.cs:14 |
| TeleportCooldownState.RemainingTicks | NLTX TeleportCooldownState | 通用旅行冷却 ticks | 主体权威状态候选 | start、Tick、clear、主体清理 | TeleportCooldownStateComponent.RemainingTicks | partial | D:\TRbackup\NLTX\src\Teleportation\TeleportCooldownState.cs:3-8 |
| TeleportCooldownState.Source | NLTX TeleportCooldownState | 冷却来源 | 主体权威状态候选 | cooldown start/clear | TeleportCooldownStateComponent.Source | partial | TeleportCooldownState.cs:6-8；TeleportSource.cs:3-10 |
| TeleportCooldownState.StartedAtTick | NLTX TeleportCooldownState | 冷却开始 tick | 审计/兼容字段 | cooldown start/clear | TeleportCooldownStateComponent.StartedAtTick | partial | TeleportCooldownState.cs:6-8 |
| PortalTraversalCooldownStateComponent.LastPortal | NLTX dome | 最近穿越端点关系 | Portal traversal 状态 | successful traversal/expiry | PortalTraversalCooldownStateComponent.LastPortal | partial | dome\src\Terraria.Dome.Simulation\Teleportation\PortalTraversalCooldownStateComponent.cs:5-13 |
| PortalTraversalCooldownStateComponent.SubjectKind | NLTX dome | Player/NPC/Projectile 主体分类 | 主体范围辅助字段 | component attach/remove | PortalTraversalCooldownStateComponent.SubjectKind | partial | dome\src\Terraria.Dome.Simulation\Teleportation\PortalSubjectKind.cs:3-9 |
| LocationComponent.X/Y | NLTX Entity ECS | 实体世界位置 | 外部权威实体状态 | entity lifecycle/update | not-component in this subsystem; referenced owner only | confirmed | D:\TRbackup\NLTX\src\Share\Entity\Components\LocationComponent.cs:3-13 |
| SpatialReferenceComponent fields | NLTX Entity ECS | 父实体、偏移和空间 | 外部空间状态 | entity attach/detach | not-component in this subsystem | confirmed | D:\TRbackup\NLTX\src\Share\Entity\Components\SpatialReferenceComponent.cs:5-19 |
 
## 5. Component 定义


### 5.1 TeleportCooldownStateComponent

#### 职责

保存旅行主体的通用离散旅行冷却来源和开始时间。该组件不保存 registry 刷新节奏，也不默认覆盖 Portal 专用最近端点关系。

componentId: TT-COMP-04
name: TeleportCooldownStateComponent
status: proposed
currentNltxCoverage: partial
componentOwner: Teleportation candidate
crossSubsystemOwner: integration-review
entityScope: one travel subject entity; candidate Player/NPC scope
lifecycle: cooldown start -> tick decrement -> expiry/clear -> subject teardown cleanup
candidatePath: D:\TRbackup\NLTX\src\Teleportation\TeleportCooldownStateComponent.cs
pathStatus: proposed

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| RemainingTicks | int | 0 | 权威主体状态 | 不得为负数；大于零才表示冷却有效；冷却完成后必须归零 | partial | D:\TRbackup\NLTX\src\Teleportation\TeleportCooldownState.cs:3-8 |
| Source | TeleportSource | TeleportSource.None | 权威来源分类 | RemainingTicks == 0 时是否必须为 None 尚未由当前 owner 确认；不能把 registry refresh 当作 source | partial | TeleportCooldownState.cs:6-8；TeleportSource.cs:3-10 |
| StartedAtTick | long? | null | 审计/兼容字段 | 非空时应对应本次冷却开始；时钟来源和恢复语义未锁定 | partial | TeleportCooldownState.cs:6-8 |
| IsOnCooldown | bool derived from RemainingTicks > 0 | false | 派生值 | 不单独存储；必须与 ticks 一致 | partial | TeleportCooldownState.cs:5 |

#### 字段不变量

- 该组件的 scope 是旅行主体，不是 World；Player/NPC 的实体身份来自外部实体 owner，不在组件内重复保存。
- Source 只标记旅行语义；PylonRegistryComponent.RefreshCooldownTicksRemaining 永远不写入本组件。
- StartedAtTick 是当前 NLTX 审计字段，但 Version4 Portal 代码使用独立数组和 Tick 递减，没有证明同一时间戳语义；兼容时标 version-drift。
- 如果同一主体同时存在 Pylon Travel cooldown 和 Portal traversal cooldown，是否允许两个组件共存由 BD-COMP-04 决定；本设计不通过字段合并隐式裁决。
- 组件不保存位置、速度、目标坐标或成功结果；这些字段属于实体空间状态或短生命周期结果。

#### 生命周期

- 创建：主体第一次需要该通用旅行状态时附加，默认无冷却；是否改为主体出生时固定附加未确认。
- 初始化：开始一次适用的离散旅行后写入 source、ticks 和可选 start tick。
- 更新：ticks 递减或由成功/失败策略清理；更新者必须保持单一 owner。
- 清理：ticks 到零时清理来源和开始时间；主体移除时清理全部状态。
- 失败：资格失败、落点失败或过期目标不应消耗或改变该组件，除非兼容基线明确要求失败冷却；当前证据不足，标 decision-required。

#### Entity/World 范围

组件挂在旅行主体实体上，候选包括 Player 和 NPC。它不挂在 Pylon、Portal、World 或 Section 上。主体类别不通过新增 SubjectKind 字段复制，而通过实体组合/外部 owner 识别。

#### ID 与关系字段

- 本组件不保存 PlayerEntityId 或 NpcEntityId 字段；主体实体本身是关联载体。
- TeleportSource 是领域枚举，不是网络 ID。
- StartedAtTick 的 WorldTick owner 和持久化语义为 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

- src/Teleportation/TeleportCooldownState.cs：status: existing，但它是普通状态类而非当前已接入的 ECS Component，覆盖标为 partial。
- dome/src/Terraria.Dome.Simulation/Teleportation/PortalTraversalCooldownStateComponent.cs：status: existing，但它保存 Portal 专用字段，不足以证明本通用组件已实现。
- Test 与 dome\Test 搜索未发现对这些 Teleportation 类型的行为验证引用，故不能升级为 confirmed。

#### 证据

- NLTX 通用 cooldown：D:\TRbackup\NLTX\src\Teleportation\TeleportCooldownState.cs:3-8。
- Version4 Player cooldown 数组：D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:14、74-80、126-129。
- Version4 NPC slot 清理：D:\TRbackup\Version4\Terraria\NPC.cs:8125-8130。
- 失败不得部分提交的设计依据来自研究报告，但当前没有已运行 verifier，故 verificationStatus: not-run。

### 5.2 PortalTraversalCooldownStateComponent

#### 职责

保存 Portal 穿越特有的最近端点关系、主体分类和冷却分组。它与通用离散旅行冷却分开，是因为 Portal 反向穿越防护的失效条件包含端点关系，而 registry refresh 又属于世界范围。

componentId: TT-COMP-05
name: PortalTraversalCooldownStateComponent
status: partial
currentNltxCoverage: existing in dome, absent from root runtime wiring
componentOwner: Teleportation candidate
crossSubsystemOwner: integration-review
entityScope: one Portal-traversal subject entity
lifecycle: Portal traversal success -> active timeout/cooldown -> expiry -> subject slot cleanup
currentPath: D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Teleportation\PortalTraversalCooldownStateComponent.cs
pathStatus: existing; root adoption proposed

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| RemainingTicks | int | 0 | 权威主体状态 | 大于零表示 Portal traversal 暂时受阻；不得与 registry refresh ticks 共享存储 | partial | dome PortalTraversalCooldownStateComponent.cs:5-13；Version4 PortalHelper.cs:74-87 |
| LastPortal | Entity? current dome type; EntityReference? normalization unresolved | null | Portal 关系状态 | 非空引用必须指向仍有效的 Portal endpoint；实体删除时必须清理 | partial | dome PortalTraversalCooldownStateComponent.cs:7-10；SS14 PortalTimeoutComponent.cs only as organization reference |
| SubjectKind | PortalSubjectKind | Unknown | 主体范围辅助状态 | 值域必须覆盖实际主体；Version4 通过 Player/NPC 分支而不是该枚举表达 | version-drift | dome PortalSubjectKind.cs:3-9；Version4 PortalHelper.cs:122-129、191-207 |
| CooldownGroup | int? | null | 分组/兼容状态 | 非空时必须与 Portal 关系 owner 一致；Version4 未证明其存在 | unresolved | dome PortalTraversalCooldownStateComponent.cs:7-13 |
| IsCoolingDown | bool derived from RemainingTicks > 0 | false | 派生值 | 不单独存储；与 ticks 一致 | partial | dome PortalTraversalCooldownStateComponent.cs:12 |

#### 字段不变量

- LastPortal 与 RemainingTicks 的组合用于防止立即沿同一关系反向穿越；Version4 直接证据只确认 per-player/per-NPC cooldown 数组，没有确认 LastPortal 字段，因此该语义不能写成 Version4 baseline。
- SubjectKind 不是新的身份 ID；它不能取代 PlayerEntityId、NpcEntityId 或 Projectile identity。
- CooldownGroup 在没有 Version4 或 NLTX 根目录 owner 证据时保持 unresolved，不能依据 dome 的字段单方面固化为必需字段。
- Portal 专用 cooldown 与通用旅行 cooldown 是否并存、覆盖或互斥由 BD-COMP-04 裁决。
- PortalTraversalCooldownStateComponent 不保存 Portal 的 Angle、SupportTile、PeerEndpoint 或实体位置。

#### 生命周期

- 创建：主体第一次参与 Portal traversal 时附加或由主体生命周期提供空状态。
- 初始化：成功穿越后保存最近端点候选和 cooldown ticks；当前 Version4 只确认数组写入，不确认 LastPortal 保存。
- 更新：每个世界 tick 递减；端点失效、主体 slot 重用或超时结束时清理关系字段。
- 清理：NPC slot reset 必须清理 NPC 对应状态；Version4 直接证据为 NPC.cs:8125-8130 调用 PortalHelper.ResetNPCSlotData。
- 失败：入口碰撞、出口空间检查或配对失效时，不应提前写入 cooldown；当前 Version4 对失败写集的完整语义仍为 evidence-gap。

#### Entity/World 范围

组件挂在 Player、NPC 或其他已被允许穿越 Portal 的主体实体上，不挂在 Portal endpoint 或 World 上。Version4 Player 调用链缺失而 NPC 调用链存在，因此 Player 组合为 version-drift，不是 confirmed。

#### ID 与关系字段

- LastPortal 的最终表示需要在 EntityReference、dome Entity 和网络实体 ID 之间选择，crossSubsystemOwner: integration-review。
- SubjectKind 是行为分类，不是主体持久化 ID。
- CooldownGroup 如果保留，必须说明其与 PortalGroup、NetworkId 和世界范围的关系；当前为 BD-COMP-04 的未决字段。

#### 当前 NLTX 映射

- dome/src/Terraria.Dome.Simulation/Teleportation/PortalTraversalCooldownStateComponent.cs：status: existing，字段形状已存在，当前覆盖为 partial。
- 根目录没有同名组件；根目录 TeleportCooldownState 只覆盖 RemainingTicks、Source、StartedAtTick，不能静默合并。
- Test 和 dome\Test 未发现针对该组件的直接行为验证引用，不能声明生命周期已验证。

#### 证据

- Version4 Portal cooldown arrays 和写入分支：D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:12-16、64-107、126-129、191-207。
- Version4 NPC slot reset：D:\TRbackup\Version4\Terraria\NPC.cs:8125-8130。
- Version4 Player 调用缺口：Player.cs 未找到 TryPortalJumping；NPC 对应方法为 NPC.cs:79460-79468，状态 version-drift。
- SS14 PortalTimeoutComponent.cs 只证明“最近端点可以是独立组件状态”的组织参考，不证明 Terraria 字段存在。

## 6. Entity 与 Component 组合

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| World entity | PylonRegistryComponent | 无 | 任何主体 cooldown component | registry 是世界范围；不能把世界 registry 放到 Player/NPC 或 Portal entity |
| Pylon TileEntity / 结构宿主 | 无 Teleportation component；通过 registry entry 被引用 | 无 | PortalEndpointComponent、主体 cooldown components | Version4 TETeleportationPylon 的结构合法性、放置、损坏和掉落属于宿主 owner，不是 Travel component |
| 有效 Portal endpoint entity | PortalEndpointComponent | PortalLinkStateComponent | PylonRegistryComponent、主体 cooldown components | 几何/支撑必须独立于 peer 关系；未配对 endpoint 仍是有效 endpoint 状态 |
| 已配对 Portal endpoint entity | PortalEndpointComponent、PortalLinkStateComponent | 无 | 无 | link 关系只在 peer 有效时存在；几何字段不因配对重建而复制 |
| Player entity | 由 PlayerGameplay 自有位置/兼容组件承载位置和 Portal physics | TeleportCooldownStateComponent、PortalTraversalCooldownStateComponent | PylonRegistryComponent | Player 可拥有主体级 cooldown；是否两个 cooldown 同时存在需 BD-COMP-04，不能隐式合并 |
| NPC entity | 由 NpcAndTownSimulation 自有位置/兼容组件承载位置和 Portal color | TeleportCooldownStateComponent、PortalTraversalCooldownStateComponent | PylonRegistryComponent | NPC slot reset 有独立清理路径；不能复用 Player-only Pylon style 字段 |
| Projectile entity 尚未成为有效 Portal endpoint | 无 PortalLinkStateComponent | 可有未决的 endpoint 兼容数据 | PortalLinkStateComponent | 不完整配对不能产生 peer 关系；endpoint 组件是否提前附加需结构生命周期裁决 |
| World Section | 无本子系统 Component | 无 | 任何主体 cooldown component | Section 是空间/复制范围，不是 Portal pair 或旅行冷却 owner；Section ID 作为外部关系字段处理 |

组合约束：

- PortalLinkStateComponent 只能出现在拥有有效 PortalEndpointComponent 的实体上；反向关系是否强制成对同步为 BD-COMP-03。
- TeleportCooldownStateComponent 和 PortalTraversalCooldownStateComponent 不能共享同一个 RemainingTicks 存储。允许共存、优先级覆盖或互斥仍需 BD-COMP-04。
- PylonRegistryComponent 的 RefreshCooldownTicksRemaining 不是任何实体组合的主体 cooldown。
- 位置和速度组件不在本清单中复制；旅行相关组件只保存关系、冷却和端点/registry 事实。
- Pylon registry entry 可以含 TileEntityId 候选，但这不表示 Pylon TileEntity 必须组合任一 Teleportation component。

## 7. 组件拆分与合并决策

### 7.1 保持合并：Pylon 当前快照与差异缓存

CurrentPylons 是权威快照，PreviousPylons 是差异缓存，分类不同，但两者具有相同的世界 owner、替换时机、reset 生命周期和 diff 不变量。Version4 在 TeleportPylonsSystem.cs:47-71 中交换并比较两份列表；NLTX dome 组件在 PylonRegistryComponent.cs:18-32 中共同替换。当前不单独创建 PylonRegistryDiffCacheComponent，避免两个 world component 之间产生隐含同步。

### 7.2 拆分：Portal endpoint 与 Portal link

Angle、Direction、Form、SupportTile 只描述一个 endpoint；PeerEndpoint、PortalGroup 描述两个 endpoint 的关系。两者的失效条件不同：支撑 Tile 损坏可以使一个 endpoint 失效，配对关系需要清除 peer；因此不把 PortalNetworkState 原样保留为双端巨型组件。

### 7.3 拆分：registry refresh 与主体冷却

Version4 的 _cooldownForUpdatingPylonsList 是世界 registry refresh 节奏，PortalCooldownForPlayers/NPCs 是主体 traversal cooldown。两者更新者、scope、失败影响和清理生命周期不同，必须分开。PylonRegistryComponent 只保留 registry refresh 字段，两个主体冷却候选使用独立组件。

### 7.4 保持拆分：通用旅行冷却与 Portal 专用冷却

TeleportCooldownState 保存 source 和 start tick；dome PortalTraversalCooldownStateComponent 保存最近 Portal、主体分类和 cooldown group。由于 Portal cooldown 具有关联 endpoint 的额外不变量，暂不合并。是否在最终实现中用一个多 lane 组件表达，交由 BD-COMP-04，当前设计不强行选择。

### 7.5 不新增：表现/兼容字段组件

Player.lastPortalColorIndex、Player.lastTeleportPylonStyleUsed、portalPhysicsFlag、_portalPhysicsTime 和 NPC.lastPortalColorIndex 已由 Player/NPC 领域拥有。它们的可观察效果来自 Version4 Player.Teleport / NPC.Teleport 和更新逻辑，但当前证据不足以证明它们应从实体领域迁移到 Teleportation component。将它们重复建模会产生双写风险，因此暂不创建新组件。

### 7.6 不新增：短生命周期请求和结果数据

当前研究报告提出的 intent、资格结果、落点结果和旅行结果是交互或快照数据，而不是已证实需要跨 Tick 保存的持续组件。除非后续证据证明请求会跨 Tick、需要恢复或需要审计，否则不将它们升级为 Component。这样可以避免把外部协议字段、碰撞计算临时值和主体位置镜像塞入权威状态。

## 8. 不单独创建 Component 的对象

| 对象 | 不单独创建的理由 |
|---|---|
| 单个 Pylon | 是结构宿主/实例；TileEntity 的尺寸、Tile 合法性、损坏清理、掉落和放置生命周期不属于 Teleportation component。registry 只保存其已提交端点记录。 |
| 单个 Portal | 是 endpoint entity 实例；端点几何放入 PortalEndpointComponent，peer 关系放入 PortalLinkStateComponent，不再创建一个聚合 Portal component 包含两端和主体状态。 |
| 单个 cooldown 字段 | 是组件中的策略状态，不是独立领域对象；但 registry refresh、通用旅行和 Portal traversal 三种语义仍需字段/组件隔离。 |
| ValidTeleportCheck Hook | 是公开资格扩展点，不是持续状态；其 NPC/Danger/Biome 输入不属于端点 component。tModLoader 证据只交叉确认阶段语义。 |
| NetTeleportPylonModule | 是协议边界对象；Version4 Deserialize 为空，完整参考的请求分派也不能成为组件字段。网络包中的 Tile 坐标和类型不能直接成为权威 identity。 |
| PortalHelper 的全局配对数组 | 是 Version4 派生缓存实现，不是必须持久化的组件；最终 link 关系应由 endpoint 关系表达，是否保留查找缓存未决。 |
| Collision.CheckAABBvLineCollision / Collision.TileCollision | 是空间计算能力，不是旅行状态；落点临时结果不应写入端点或主体 component。 |
| Portal 颜色、粒子、声音和聊天提示 | 是表现/外部观察字段；当前 Player/NPC 兼容字段保留在其 owner，不能建立重复 Travel component。 |
| PylonRegistryEntry | 是 registry 集合中的值记录，不是独立实体 Component；它的 TileEntityId owner 仍需整合审查。 |
| PortalNetworkState 原样聚合对象 | 同时混合端点引用、支撑 Tile、完整性、颜色和更新时间，生命周期及 owner 不一致；拆分后映射到端点/link/兼容边界。 |


## 9. 当前 NLTX 组件覆盖

| 当前路径 | 当前状态 | 已覆盖字段/语义 | 与目标组件的关系 | 缺口 |
|---|---|---|---|---|
| D:\TRbackup\NLTX\src\Teleportation\PortalEndpointComponent.cs | existing / partial | Angle、Direction、Form、PortalEntity、SupportTile | 直接映射 PortalEndpointComponent | 没有 active/support 有效位、完整生命周期和最终 identity scope |
| D:\TRbackup\NLTX\src\Teleportation\PortalNetworkState.cs | existing / partial | 双端引用、两个 support Tile、IsComplete、LastPortalColorIndex、UpdatedAtTick | 拆分映射到 PortalEndpointComponent、PortalLinkStateComponent 和主体兼容 owner | 聚合对象混合 scope；更新时间和颜色 owner 未决 |
| D:\TRbackup\NLTX\src\Teleportation\TeleportCooldownState.cs | existing / partial | RemainingTicks、Source、StartedAtTick、IsOnCooldown | 覆盖 TeleportCooldownStateComponent 的基础字段 | 没有实体组合、唯一 writer、失败语义和恢复语义 |
| D:\TRbackup\NLTX\src\Teleportation\TeleportSource.cs | existing | None、Portal、Pylon、Mechanism、Other | TeleportCooldownStateComponent.Source 的值类型 | Source 与实际 cooldown lane 的 owner 未锁定 |
| D:\TRbackup\NLTX\src\WorldStorage\PylonRegistryState.cs | existing / partial | Current、Previous、refresh cooldown、Revision、Count、HasPendingRefresh | 覆盖 PylonRegistryComponent 基础形状 | 当前类型不是 ECS Component；entry identity 和 World owner 未最终裁决 |
| D:\TRbackup\NLTX\src\WorldStorage\PylonRegistryEntry.cs | existing | Position、Kind、TileEntityId、IsValid、CanTeleport | registry Component 的集合元素 | 与 Version4 Point16 + TeleportPylonType 的映射和重复规则未锁定 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Teleportation\PylonRegistryComponent.cs | existing / partial | 只读集合、Replace、AdvanceTick、Clear、Revision | Pylon registry 的局部实现参考 | 只在 dome 工作区，根目录没有接入证明 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Teleportation\PortalEndpointStateComponent.cs | existing / partial | Owner、PairIndex、PortalGroup、Angle、Form、Direction、SupportedByTile、Active、PortalColorIndex | endpoint/link 的并行材料 | 与根目录 endpoint 类型、Direction 类型和 owner scope 不一致 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Teleportation\PortalLinkStateComponent.cs | existing / partial | PortalGroup、PeerEndpoint、IsPairComplete | PortalLinkStateComponent 的局部形状 | Arch.Core.Entity 不能直接作为根目录公共 identity；双向清理未验证 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Teleportation\PortalTraversalCooldownStateComponent.cs | existing / partial | RemainingTicks、LastPortal、SubjectKind、CooldownGroup、IsCoolingDown | PortalTraversalCooldownStateComponent 的局部形状 | 根目录没有接线，Version4 没有证明 LastPortal/CooldownGroup |
| D:\TRbackup\NLTX\src\Share\Entity\Components\LocationComponent.cs | existing | X、Y 位置 | 外部位置 owner；不复制到 Teleportation | 缺少旅行写入边界，但不在本 Component-only 文档内定义 |
| D:\TRbackup\NLTX\src\Share\Entity\Components\SpatialReferenceComponent.cs | existing | ParentEntity、offset、Space、world-space 派生值 | 外部空间 owner；不复制到 Teleportation | EntityReference scope 与跨域 owner 未决 |
| D:\TRbackup\NLTX\Test、dome\Test | missing focused coverage | 未发现对上述 Teleportation/Pylon/Portal 类型的直接行为验证引用 | 不影响组件字段设计，但阻止 confirmed | verificationStatus: not-run，无当前测试证据 |

当前覆盖结论：根目录已有类型可以支撑局部字段设计，但没有证据证明五个组件形成完整组合、生命周期和行为闭环。nltxStatus: partial 保持不变。

## 10. 组件级 evidence-gap

| gapId | 未确认内容 | 已查来源 | 对组件设计的影响 | 状态 |
|---|---|---|---|---|
| EG-COMP-01 | PylonRegistryEntry.TileEntityId 是否是权威持久化身份，或仅为 NLTX 记录 | Version4 TeleportPylonInfo.cs:6-9、NLTX PylonRegistryEntry.cs:3-10 | PylonRegistryComponent 的 entry identity 不能锁定 | unresolved |
| EG-COMP-02 | Version4 Pylon Equals 和 registry diff 的重复/删除语义 | Version4 TeleportPylonInfo.cs:6-9；完整参考同路径 | PreviousPylons 缓存能确定，但唯一性不确定 | version-drift |
| EG-COMP-03 | 两个 NLTX TileCoordinate 类型的最终 owner | src/WorldStorage/TileCoordinate.cs、src/WorldInteraction/Tiles/TileCoordinate.cs 及 endpoint/entry 引用 | SupportTile 与 Pylon Position 的字段类型不能统一宣布 | unresolved |
| EG-COMP-04 | Portal endpoint 的 active/support 字段是否应成为根目录组件字段 | Version4 Projectile/Portal 支撑逻辑；NLTX 根组件与 dome component 差异 | PortalEndpointComponent 仍为 partial | partial |
| EG-COMP-05 | Portal peer identity 的根目录表示：EntityReference、dome Entity 还是网络实体引用 | PortalNetworkState.cs:6-14、dome link component、SS14 only as pattern | PortalLinkStateComponent.PeerEndpoint 类型不能锁定 | unresolved |
| EG-COMP-06 | PortalGroup、pair index、Projectile owner 和 Portal identity 的关系 | Version4 PortalHelper.cs:88-99、189-205；dome endpoint/link | link 关系字段的唯一 identity 未确认 | unresolved |
| EG-COMP-07 | Version4 是否存在 Player Portal traversal 调用链 | Version4 Player.cs 未找到 TryPortalJumping；Version4 NPC.cs:79460-79468；完整参考 Player | Player 与 PortalTraversalCooldownStateComponent 的组合不能标 confirmed | version-drift |
| EG-COMP-08 | LastPortal 与 CooldownGroup 是否是 Terraria 目标行为必需字段 | Version4 cooldown arrays；dome component；SS14 timeout only | Portal 专用 cooldown 的最小字段仍需裁决 | unresolved |
| EG-COMP-09 | 通用 TeleportCooldownState 与 Portal 专用 cooldown 是否共存或互斥 | NLTX root state、dome state、Version4 separate arrays | 两个 cooldown Component 的组合未锁定 | decision-required |
| EG-COMP-10 | PortalNetworkState.UpdatedAtTick 的 owner 和持久化意义 | NLTX PortalNetworkState.cs:14，未发现测试/消费者 | 不创建独立更新时间组件 | unresolved |
| EG-COMP-11 | registry、link、cooldown 的持久化/网络快照是否需要权威 Component 字段 | Version4 网络模块仅确认 Pylon add/remove 包；NLTX 无闭环测试 | 不把网络/存档字段加入组件；保留 integration review | partial |
| EG-COMP-12 | 组件组合和字段不变量当前没有 focused 行为验证 | Test、dome\Test 搜索无直接引用 | 所有设计状态不能提升为已验证 | missing |

## 11. 未决组件 owner

每项都保留候选方案，不在本会话宣布最终 owner。所有跨子系统候选均使用 crossSubsystemOwner: integration-review。

### BD-COMP-01：PylonRegistryComponent 的 owner

- 冲突字段：CurrentPylons、PreviousPylons、Revision、TileEntityId。
- 候选 A：WorldStorage 拥有世界 registry；Teleportation 只读已提交快照。
- 候选 B：Teleportation 拥有 registry，因为 Pylon 资格会消费其端点。
- 组成影响：A 保持 PylonRegistryComponent 在 WorldStorage；B 会把 TileEntity/registry 生命周期拉入 Teleportation，并增加结构边界耦合。
- 当前不能裁决：Version4 的 TeleportPylonsSystem 同时做列表刷新、差异广播和资格入口，NLTX PylonRegistryState 位于 WorldStorage；两者尚未形成最终公共 owner。
- 状态：decision-required；crossSubsystemOwner: integration-review。

### BD-COMP-02：Pylon/Portal identity 和 Tile 坐标 owner

- 冲突字段：PylonRegistryEntry.TileEntityId、Position、PortalEntity、SupportTile、PeerEndpoint。
- 候选 A：保留结构 identity、实体 identity、网络 identity 和 Tile 坐标四类字段分离。
- 候选 B：用统一 EntityReference 包装全部关系。
- 组成影响：A 会保留多个明确字段和转换边界；B 可能把 TileEntity、Portal entity 和网络实体错误合并。
- 当前不能裁决：根目录已经存在多个 TileCoordinate 和 EntityReference scope，但 Version4 Pylon 网络字段只有位置/类型，Portal endpoint 的跨域 identity 未闭合。
- 状态：decision-required；crossSubsystemOwner: integration-review。

### BD-COMP-03：Portal link 的关系形状

- 冲突字段：PeerEndpoint、PortalGroup、PairIndex、FirstPortal/SecondPortal、IsComplete。
- 候选 A：每个 endpoint 一个 PortalLinkStateComponent，通过 peer 关系表达配对。
- 候选 B：World entity 持有双端 Portal pair 集合，endpoint 只保存几何。
- 组成影响：A 减少双端聚合和局部失效复制；B 便于全局扫描但会引入 world cache 与 endpoint 状态同步。
- 当前不能裁决：Version4 FoundPortals 是 world/static cache，NLTX 同时有双端 PortalNetworkState 和 endpoint/link 组件草图；没有现成权威选择。
- 状态：decision-required；crossSubsystemOwner: integration-review。

### BD-COMP-04：通用与 Portal cooldown 的组合

- 冲突字段：两个 RemainingTicks、Source、StartedAtTick、LastPortal、SubjectKind、CooldownGroup。
- 候选 A：通用旅行 cooldown 与 Portal 专用 cooldown 分成两个 Component，允许在同一主体上共存。
- 候选 B：合并成一个按 source/lane 区分的主体 Component。
- 组成影响：A 保留不同生命周期和关系不变量，但需要明确优先级；B 减少组合数量，但会把 Portal 最近端点和 Pylon/Mechanism 来源混入同一状态。
- 当前不能裁决：NLTX root 与 dome 采用不同字段形状，Version4 Pylon 请求链缺失，无法从 baseline 证明 lane 语义。
- 状态：decision-required；crossSubsystemOwner: integration-review。

### BD-COMP-05：表现/兼容字段 owner

- 冲突字段：Player.lastPortalColorIndex、Player.lastTeleportPylonStyleUsed、portalPhysicsFlag、_portalPhysicsTime，NPC.lastPortalColorIndex。
- 候选 A：保留在 PlayerGameplay/NpcAndTownSimulation 的主体组件，Teleportation 只提供旅行事实。
- 候选 B：创建共享的 Teleportation 兼容 Component。
- 组成影响：A 避免双写和表现状态回流；B 便于按旅行源统一读取，但会引入 Player/NPC/表现 owner 交叉依赖。
- 当前不能裁决：Version4 Player.Teleport 与 NPC.Teleport 直接写这些字段，当前 NLTX 没有等价的共享组件证据。
- 状态：decision-required；crossSubsystemOwner: integration-review。

## 12. 最终 Component 清单

下表是本文件的最终五项组件清单。status: proposed 表示目标组件尚未在对应根目录路径创建；status: partial 表示当前已有局部形状，但组合、owner 或生命周期仍不完整。

| componentId | Component | status | componentOwner 候选 | crossSubsystemOwner | entityScope | candidate/current path | 主要字段 |
|---|---|---|---|---|---|---|---|
| TT-COMP-01 | PylonRegistryComponent | proposed | WorldStorage | integration-review | World | src/WorldStorage/PylonRegistryComponent.cs，status: proposed；dome counterpart status: existing | CurrentPylons、PreviousPylons、RefreshCooldownTicksRemaining、Revision |
| TT-COMP-02 | PortalEndpointComponent | partial | Teleportation candidate | integration-review | 单个 Portal endpoint entity | src/Teleportation/PortalEndpointComponent.cs，status: existing | Angle、Direction、Form、PortalEntity、SupportTile |
| TT-COMP-03 | PortalLinkStateComponent | proposed | Teleportation candidate | integration-review | 单个 endpoint 的可选 peer 关系 | src/Teleportation/PortalLinkStateComponent.cs，status: proposed；dome counterpart status: existing | PortalGroup、PeerEndpoint、IsPairComplete |
| TT-COMP-04 | TeleportCooldownStateComponent | proposed | Teleportation candidate | integration-review | 单个 Player/NPC travel subject | src/Teleportation/TeleportCooldownStateComponent.cs，status: proposed | RemainingTicks、Source、StartedAtTick、IsOnCooldown |
| TT-COMP-05 | PortalTraversalCooldownStateComponent | partial | Teleportation candidate | integration-review | 单个 Portal traversal subject | dome current path status: existing；root adoption status: proposed | RemainingTicks、LastPortal、SubjectKind、CooldownGroup、IsCoolingDown |

清单外的明确结论：

- PylonRegistryEntry 是 PylonRegistryComponent 内的集合值，不是第六个 Component。
- PortalNetworkState 不作为第六个 Component 保留；它被拆映射到 endpoint、link 和主体兼容字段。
- TeleportSource 是字段值类型，不是 Component。
- LocationComponent 与 SpatialReferenceComponent 是外部实体/空间状态，不在本子系统复制。
- LastPortalColorIndex、Pylon style、Portal physics timer/flag 暂不创建新的 Teleportation Component。
- 所有新路径和新组件名都必须继续标记 status: proposed，直到实际实现和独立验证完成；本文件不改变源码状态。

## 13. 最终声明

本文件是 Component-only Design。

本文件不定义 System、Query、Command、Event、Adapter、Projection、调度顺序、运行时实现、测试计划或迁移计划。

所有 status: proposed 的 Component 都只是设计提案。本文件不声明代码已创建、已迁移、行为等价或验证通过。

本文件仅整理当前会话明确产生的研究报告，并重新读取了 Version4 关键源码、完整参考同路径成员、tModLoader v2026.07 本地页面、Space Station 14 最小相关组件材料和当前 NLTX 组件/测试覆盖。verificationStatus: not-run 保持不变；本轮没有运行构建或测试，没有修改 Version4、完整参考、tModLoader、Space Station 14、NLTX src、Test、dome\src、dome\Test、原研究报告或其他设计文档。

未决组件级 evidence-gap 共 12 项（EG-COMP-01 至 EG-COMP-12），未决组件 owner 决策共 5 项（BD-COMP-01 至 BD-COMP-05）。这些缺口和裁决不能由本 Component-only Design 自行消除。



### 5.3 PylonRegistryComponent

#### 职责

保存当前世界中已经由结构边界提交的 Pylon 端点快照，并保存 registry 差异计算所需的上一快照、刷新节奏和 revision。它不保存 Pylon TileEntity 的完整结构状态，也不保存任何主体旅行冷却。

componentId: TT-COMP-01
name: PylonRegistryComponent
status: proposed
componentOwner: WorldStorage candidate
crossSubsystemOwner: integration-review
entityScope: one world entity / one loaded world
lifecycle: world initialization -> endpoint refresh/replace -> world reset/unload
candidatePath: D:\TRbackup\NLTX\src\WorldStorage\PylonRegistryComponent.cs
pathStatus: proposed

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| CurrentPylons | IReadOnlyList<PylonRegistryEntry> | empty | 权威快照 | 外部不能通过只读视图修改内部集合；每个 entry 必须通过结构有效性来源产生；重复 identity 规则仍 unresolved | partial | Version4 TeleportPylonsSystem.cs:15、47-62；NLTX PylonRegistryState.cs:5-7 |
| PreviousPylons | IReadOnlyList<PylonRegistryEntry> | empty | 差异缓存 | 只用于与 CurrentPylons 比较；不能被旅行资格当作当前有效端点；替换时形成上一版本快照 | partial | Version4 TeleportPylonsSystem.cs:17、63-71；NLTX PylonRegistryState.cs:5-6 |
| RefreshCooldownTicksRemaining | int | 0 | registry 生命周期状态/缓存 | 不得阻止或代替主体 Travel cooldown；不能为负数；0 表示允许刷新候选 | partial | Version4 TeleportPylonsSystem.cs:19、25-35、76-80；NLTX PylonRegistryState.cs:8-10 |
| Revision | uint | 0 | 权威版本 | 每次接受新的当前快照时单调增加；reset 行为和溢出策略未被 Version4 唯一确认 | partial | NLTX PylonRegistryState.cs:10-11；dome PylonRegistryComponent.cs:18-48 |
| Count | int | derived CurrentPylons.Count | 派生值 | 不单独存储；必须与当前快照长度一致 | partial | NLTX PylonRegistryState.cs:7；dome PylonRegistryComponent.cs:15 |
| HasPendingRefresh | bool | derived RefreshCooldownTicksRemaining == 0 | 派生值 | 保持当前 NLTX 语义；名称与“pending”直觉相反，不能自行改义 | partial | NLTX PylonRegistryState.cs:8；dome PylonRegistryComponent.cs:16 |

#### 字段不变量

- CurrentPylons 是组件唯一的当前 Pylon 集合；PreviousPylons 不能被解释为第二个可旅行目标集合。
- PylonRegistryEntry.IsValid 和 CanTeleport 是 entry 级别的当前 NLTX 约束，但 Version4 的 TeleportPylonInfo 没有同名有效位；两者不是已经证明的等价语义。
- Position、Kind、TileEntityId 的联合 identity 尚未锁定。Version4 直接记录 PositionInTiles 与 TypeOfPylon；NLTX 额外记录 TileEntityId，必须保留 owner 决策。
- RefreshCooldownTicksRemaining 只能影响 registry 刷新生命周期；它不能挂接到 Player、NPC 或 Portal entity。
- registry 组件不应保存 SceneMetrics、NPC 数量、Danger、Biome 或任何玩家资格结果。
- PreviousPylons 与 CurrentPylons 保持在同一组件，是因为两者由同一世界 owner 共同替换/清理，并共同维护 diff；如果未来出现独立读者，再以决策记录拆出缓存组件。

#### 生命周期

- 创建：世界实体初始化或世界加载时创建空组件；当前默认列表为空，revision 默认值按 NLTX 现有模型处理。
- 初始化：结构边界提交有效 Pylon 记录后替换当前集合，并把旧当前集合转为上一集合。
- 更新：刷新节奏字段按世界 tick 更新；当前快照只有在完整替换后才改变，不能逐 entry 半提交。
- 清理：世界 reset/unload 时清空当前和上一集合，重置刷新状态；Version4 Reset 的直接证据为 TeleportPylonsSystem.cs:76-80。
- 失败：结构扫描失败或 entry identity 无法确定时，不能用未验证数据覆盖当前快照；保留旧快照的策略仍需整合裁决。

#### Entity/World 范围

组件挂在每个 loaded world 的世界实体上，不挂在单个 Pylon TileEntity、Player、NPC 或 Portal entity 上。一个世界只能有一个该组件实例；多世界/跨世界语义未由 Version4 证据确认。

#### ID 与关系字段

- PylonRegistryEntry.Position 是 Tile 坐标值，不是网络 ID。
- PylonRegistryEntry.TileEntityId 是 NLTX 当前持有的 TileEntity 关系/持久化候选，不能直接当作 Version4 的 TeleportPylonInfo identity。
- Pylon type/kind 是领域值，不是 NetworkId。
- WorldEntityId、TileEntityId、NetworkId 和 TileCoordinate 的跨域 owner 必须写 crossSubsystemOwner: integration-review。
- 当前仓库同时存在 Terraria.WorldStorage.TileCoordinate 与 Terraria.WorldInteraction.Tiles.TileCoordinate，类型统一方式为 evidence-gap，不能在本文件擅自宣布最终 owner。

#### 当前 NLTX 映射

- src/WorldStorage/PylonRegistryState.cs：status: partial，已有当前/上一列表、刷新 ticks 和 revision，但不是名为 Component 的类型。
- src/WorldStorage/PylonRegistryEntry.cs：status: existing，已有位置、kind、TileEntityId 和有效位；与 Version4 位置/类型字段的对应关系为 partial。
- dome/src/Terraria.Dome.Simulation/Teleportation/PylonRegistryComponent.cs：status: existing，已有受保护替换、Tick 推进和 clear 行为；不能因此宣称根目录 NLTX 已接入运行时。

#### 证据

- Version4 直接证据：D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:15-90。
- Version4 Pylon value evidence：D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonInfo.cs:6-9。
- Version4 equality 版本差异：同文件 Equals 为空；完整参考同路径的 Equals 比较位置和类型，状态为 version-drift。
- tModLoader 宿主边界：D:\TRbackup\tmodloader-api-docs-stable\class_mod_tile_entity.html#a8415b71088157d5fcf4ed024cffe868d。

### 5.4 PortalEndpointComponent

#### 职责

保存单个 Portal endpoint entity 的几何、方向、形式、关联 entity 和支撑 Tile 事实。它不保存 peer 端点关系，不保存主体 cooldown，也不保存 Player/NPC 的位置或速度。

componentId: TT-COMP-02
name: PortalEndpointComponent
status: partial
componentOwner: Teleportation candidate
crossSubsystemOwner: integration-review
entityScope: one Portal endpoint entity / Portal projectile host
lifecycle: endpoint creation -> geometry/support refresh -> endpoint invalidation/removal
currentPath: D:\TRbackup\NLTX\src\Teleportation\PortalEndpointComponent.cs
pathStatus: existing

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| Angle | float | 0f struct default | 权威几何 | 必须表示当前 endpoint 的方向角；角度归一化范围未由 NLTX/Version4 组件字段唯一确认 | partial | D:\TRbackup\NLTX\src\Teleportation\PortalEndpointComponent.cs:6-12；Version4 PortalHelper.cs:134-145 |
| Direction | int | 0 struct default | 几何/兼容字段 | 只能取被 endpoint 几何使用的有效方向；有效值域未锁定 | version-drift | NLTX PortalEndpointComponent.cs:8-10；dome counterpart uses sbyte and constructor default 1 |
| Form | int | 0 struct default | 端点分类 | 必须与端点 Projectile/Portal form 解释一致；Version4 ai[0] 的完整值域未由当前 Version4 组件层锁定 | partial | NLTX PortalEndpointComponent.cs:8-12；Version4 PortalHelper.cs:90-99、216-247 |
| PortalEntity | EntityReference | current struct default; semantic empty value unresolved | 实体关系 | 引用必须携带正确 EntityReferenceScope；不能把 Guid 当作网络 ID 或 TileEntityId | partial | NLTX PortalEndpointComponent.cs:11-17；EntityReference.cs:5-9 |
| SupportTile | Terraria.WorldInteraction.Tiles.TileCoordinate | struct default; (0,0) 是否为合法 sentinel unresolved | 宿主关系 | 支撑 Tile 无效时 endpoint 不得继续被视为可用；坐标类型 owner 未决 | partial | NLTX PortalEndpointComponent.cs:12；Version4 PortalHelper.cs:334-418 |

#### 字段不变量

- PortalEndpointComponent 只保存一个端点，不保存两个端点的集合，也不反向保存 Player/NPC 旅行主体。
- SupportTile 的有效性与 Portal 支撑检查有关，但 Version4 SupportedTilesAreFine 的事实来自 Projectile/Tile，而不是当前 NLTX 组件已验证字段。
- PortalEntity 是实体关系引用；关系 scope、持久化生命周期和网络映射尚未统一，必须保留 crossSubsystemOwner: integration-review。
- Direction 的根目录 NLTX 类型与 dome 类型不一致，是 version-drift，不能在设计中静默改成 sbyte 或默认值 1。
- 端点失效必须使自身端点状态不可用；peer 关系由 PortalLinkStateComponent 独立失效，不能依赖两个组件字段同步写入。

#### 生命周期

- 创建：Portal Projectile/endpoint 被确认有效时附加；Version4 的源头证据为 Projectile.cs:16049-16057 和 PortalHelper.TryPlacingPortal。
- 初始化：读取端点位置、角度、方向、form 和支撑 Tile；当前 NLTX 没有完整初始化接线，状态为 partial。
- 更新：Portal endpoint 几何或支撑 Tile 变化时替换字段；不能以 peer 关系是否存在来伪造支撑有效性。
- 清理：Projectile 不再 active、支撑 Tile 损坏或 Portal 被移除时清理组件；Version4 对支撑检查和 Projectile 生命周期的直接事实位于 Projectile.cs:30355-30379、PortalHelper.cs:334-418。
- 失败：几何或支撑证据不完整时保留不可用/待验证状态，不写入主体位置。

#### Entity/World 范围

每个 Portal endpoint entity 一个组件实例。它不挂在 World singleton 上，也不挂在 Player/NPC 上。一个 endpoint 是否由 Projectile entity 直接承载，或由独立 Portal entity 代理，当前 owner 为 integration-review。

#### ID 与关系字段

- PortalEntity 是 EntityReference 候选，不等同于 Projectile 数组 slot、NetworkId 或 PortalGroup。
- SupportTile 是外部 Tile 坐标关系，不等同于 TileEntityId。
- PortalIdentity、实体 GUID、Projectile slot、网络实体 ID 的映射仍为 BD-COMP-02。

#### 当前 NLTX 映射

- src/Teleportation/PortalEndpointComponent.cs：status: partial，已经有五个端点字段，但没有 active/support 有效位和完整生命周期接线。
- dome/src/Terraria.Dome.Simulation/Teleportation/PortalEndpointStateComponent.cs：status: existing，已有 Owner、PairIndex、PortalGroup、Angle、Form、Direction、SupportedByTile 和 Active；它是并行工作区材料，不等于根目录当前实现。
- src/Teleportation/PortalNetworkState.cs 的两个端点引用和支持 Tile 应映射到本组件的多个实例，而不是继续保留一个双端聚合组件。

#### 证据

- 根目录当前组件：D:\TRbackup\NLTX\src\Teleportation\PortalEndpointComponent.cs:6-20。
- Version4 endpoint 配对扫描：D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:88-99。
- Version4 endpoint 旅行读取：PortalHelper.cs:134-145。
- Version4 支撑检查：PortalHelper.cs:334-418。

### 5.5 PortalLinkStateComponent

#### 职责

保存一个 Portal endpoint 与其 peer endpoint 的关系事实，以及关系分组和可派生的完整性。它把“端点自身几何”与“两个端点如何配对”分开，避免端点几何变化时复制整个网络状态。

componentId: TT-COMP-03
name: PortalLinkStateComponent
status: proposed
currentNltxCoverage: partial
componentOwner: Teleportation candidate
crossSubsystemOwner: integration-review
entityScope: one Portal endpoint entity carrying an optional peer relation
lifecycle: endpoint available -> pair established -> pair invalidated/cleared
candidatePath: D:\TRbackup\NLTX\src\Teleportation\PortalLinkStateComponent.cs
pathStatus: proposed

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| PortalGroup | int | 0 struct default | 关系分类 | 相同配对的 endpoint 使用同一关系分组；分组的 owner 和跨世界范围未锁定 | partial | dome PortalLinkStateComponent.cs:5-10；Version4 PortalHelper.cs:88-99 |
| PeerEndpoint | EntityReference? candidate normalized from current Entity? | null | 权威关系 | 非空时 peer 必须存在、有效且指向不同 endpoint；双方是否必须互相引用仍需裁决 | unresolved | dome PortalLinkStateComponent.cs:7-10 使用 Entity?；NLTX PortalNetworkState.cs:8、12 使用 EntityReference? |
| IsPairComplete | bool derived from PeerEndpoint.HasValue | false | 派生值 | 不单独存储；不能在 peer 为空时为 true | partial | dome PortalLinkStateComponent.cs:10；NLTX PortalNetworkState.cs:10 |

#### 字段不变量

- PortalLinkStateComponent 不保存 Angle、Direction、Form 或 SupportTile；这些属于端点组件。
- PeerEndpoint 是关系字段，不是当前 endpoint 的 PortalEntity 自身引用。
- IsPairComplete 只能由 peer 关系派生；不能用 World-level anyPortalAtAll 直接写入每个 endpoint。
- PortalGroup 的持久化 ID、网络 ID 和 owner/player 组合不能混为同一个字段。
- pair invalidation 必须清空或使 peer 关系不可用；不得保留指向已经移除 endpoint 的有效关系。

#### 生命周期

- 创建：endpoint 已存在且配对事实被确认后附加或初始化为空关系。
- 初始化：写入 peer 和关系分组；当前 NLTX 只有 dome 的局部字段，根目录没有完整关系生命周期。
- 更新：任一端点失效、Projectile 被移除或配对重建时更新关系字段。
- 清理：endpoint 移除时清除自身关系，并让 peer 进入未配对状态；清理行为的双向写入 owner 未确认。
- 失败：无法证明 peer 有效时保持 PeerEndpoint = null，不推断可旅行关系。

#### Entity/World 范围

组件挂在单个 Portal endpoint entity 上。一个 endpoint 最多有一个 peer 关系；一个 World 可拥有多个 endpoint，但 World 不直接持有一个新的双端权威组件。是否需要 world-level lookup cache 属于 BD-COMP-03，不列入最终组件清单。

#### ID 与关系字段

- PeerEndpoint 使用 EntityReference 是候选归一化形式，当前 dome 的 Arch.Core.Entity 不能直接跨项目复制。
- PortalGroup 是关系值，不是 NetworkId、TileEntityId 或 PlayerEntityId。
- pair identity、owner/player identity、Projectile slot、网络实体 ID 的最终 owner 为 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

- dome/src/Terraria.Dome.Simulation/Teleportation/PortalLinkStateComponent.cs：status: existing，已有 PortalGroup、PeerEndpoint 和 IsPairComplete。
- src/Teleportation/PortalNetworkState.cs：status: partial，已有双端聚合、支持 Tile、完整性和更新时间；目标是拆成端点实例和 link 关系，不能原样作为单个 endpoint component。
- 根目录没有 src/Teleportation/PortalLinkStateComponent.cs；目标路径和根目录接入均为 status: proposed。

#### 证据

- Version4 配对缓存：D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:57-99。
- Version4 配对消费：PortalHelper.cs:126-145。
- NLTX dome link：D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Teleportation\PortalLinkStateComponent.cs:5-10。
- 完整参考的 AddPortal、GetPortalEdges 和 GetPortalOutingPoint 实现只用于说明版本差异，不能升级 Version4 空实现的证据等级。
