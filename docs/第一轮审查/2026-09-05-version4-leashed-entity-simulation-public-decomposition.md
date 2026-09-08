# LeashedEntitySimulation 独立只读审查与 ECS 拆分报告

> taskNumber: `03`  
> subsystemId: `LeashedEntitySimulation`  
> layer: `authoritative-simulation`  
> reportType: `public-decomposition`  
> verificationStatus: `not-run`  
> reportStatus: `research-deliverable`

## 1. 执行摘要

Version4 中存在一个独立的 `LeashedEntitySimulation` 权威责任面。它不是单个 critter、单个风筝、单个锚点 TileEntity，也不是 `ProjectileSimulation` 的普通 projectile 变体。`D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs` 同时拥有类型注册、运行时实体索引、按 world section 的活动集合、生成/销毁、逐帧更新和独立 `NetModule` 同步流；`Main`、`WorldGen`、`RemoteClient` 和锚点 TileEntity 是其直接调用者或外部触发边界。

Version4 的主文件是一个有删减的源码基线：发送路径和更新骨架仍可见，但 `NetModule.Deserialize`、`SectionEntityList.Add/Remove`、`LeashedEntity.Remove`、`StreamNetUpdates`、静态构造函数和若干行为实现存在空体或缺失。完整可编译参考源码仅用于补足同文件、同签名、同调用邻域的明确缺口；这些补证不能被写成 Version4 当前已执行的事实。

本报告建议将责任面拆成以下能力：

- `Definition` 与 `Registry`：只保存稳定类型定义、原型和创建策略；
- `State`、`Motion`、`Lifecycle`、`SectionIndex` 与 `AnchorRelation`：分别承载实例权威状态、运动状态、生命周期、空间索引和锚点关系；
- `SectionActivationSystem`、`Spawn/Despawn`、`UpdateSystem`：显式表达提交顺序；
- `Query`：只读查询，不直接写权威状态；
- `ReplicationSnapshot`、`ReplicationAdapter`、`PersistenceAdapter`、`ClientProjection`：隔离网络、存档和客户端表现副作用。

以上所有新类型、目标路径和接口均为 `status: proposed`。当前 NLTX 已有少量 Leash 结构草图和协议解析类型，但没有证据证明 registry、锚点关系、section 生命周期、server-authoritative update/spawn/despawn 和 focused verifier 已闭合，因此本报告将 authoritative implementation 状态判为 `missing`；局部模型/协议草图本身另记为 `partial`，不是迁移完成。

本轮未运行编译、测试、覆盖率检查或其他 compile-capable 命令；本报告所有验证字段统一为 `verificationStatus: not-run`。它不证明行为等价、API 兼容、编译通过或当前 NLTX 已实现该系统。

## 2. 子系统范围和不负责的内容

### 2.1 负责范围

`LeashedEntitySimulation` 的建议责任边界如下：

| 责任 | Version4 证据 | 边界结论 |
| --- | --- | --- |
| 类型注册与原型查找 | `D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:42-90`，`Registry.RegisterAll`、`Register` | registry 是类型定义/创建策略的 owner；单个 prototype 不是一级子系统 |
| 实例身份与集合 | `LeashedEntity.cs:177-194`，`ByWhoAmI`、`BySection`、`ActiveSectionList`、`whoAmI`、`Type` | 实例状态和索引属于该责任面；索引不能反向成为行为 owner |
| section 激活/停用响应 | `ActiveSections.cs:12-43`；完整参考 `LeashedEntity.cs:340-350` | Leash 消费 section 激活事实并控制本地 Spawn/Despawn；section 全局事实的最终 owner 需整合裁决 |
| 生成、销毁与锚点关系 | 完整参考 `LeashedEntity.cs:377-410,425-440`；锚点完整参考 `TELeashedEntityAnchor.cs:44-61` | 锚点触发命令；Leashed registry 承担实例生命周期 |
| 行为更新 | `LeashedEntity.cs:263-289`；完整参考行为类型 | `Walker/Flyer/Jumper` 是 movement strategy/behavior boundary，不各自升格为一级子系统 |
| 流式同步 | `LeashedEntity.cs:18-39,162-175`；完整参考 `:511-526` | server snapshot 经 Adapter 发送；客户端接收后只能更新 projection/镜像 |

### 2.2 不负责的内容

| 对象或能力 | 归属建议 | 本报告处理方式 |
| --- | --- | --- |
| `TELeashedEntityAnchor`、`TEKiteAnchor`、`TECritterAnchor` 的 TileEntity 注册、tile 合法性和 item 宿主 | `WorldStorage` / `WorldInteractionAndStructures`，最终 owner `integration-review` | 只记录其触发关系、存档关系和销毁边界，不重新设计 TileEntity 子系统 |
| `ActiveSections` 的全局 section 真相、`RemoteClient.TileSections` 和连接超时 | `NetworkSessionAndSectionStreaming` / `WorldStorage`，最终 owner `integration-review` | 只定义 Leashed 所需的只读 Port 和事件输入 |
| `NetManager`、`MessageBuffer`、`NetMessage` 的通用传输 | `NetworkSessionAndSectionStreaming` | 只定义 `ReplicationAdapter` seam，不把低层网络类型渗入核心状态 |
| projectile damage、collision、owner、lifetime 和普通 projectile registry | `ProjectileSimulation` | 只保留“可复用运动/投影机制”的边界挑战，不把 Leashed 实体塞入 projectile 巨型组件 |
| NPC 定义、Item 目录、`ContentSamples`、物品消耗 | `ContentCatalog` / `NpcAndTownSimulation` / `ItemContainerAndEconomy` | 只以 `npcType`、`projectileType`、item compatibility 作为外部事实输入 |
| `DrawNPCDirect`、`Main.DrawKite` 和屏幕可见性 | 客户端 presentation | 只定义 `ClientProjection`，不把 draw 作为权威模拟写入 |
| 通用 `EntityId`、`PersistentEntityId`、`NetworkId`、`WorldSectionId`、`TileCoordinate` | 跨子系统整合 | 只能提出候选，统一标记 `crossSubsystemOwner: integration-review` |

## 3. 证据优先级和来源角色

本次按公共协议采用 `Version4 → 完整可编译参考源码 → tModLoader API 文档 → Space Station 14 ECS 参考` 的优先级。实际读取的规则和材料包括：

| 来源 | 实际读取内容 | 角色 |
| --- | --- | --- |
| `D:\TRbackup\NLTX\AGENTS.md` | 仓库范围、ECS 文件组织、C#、副作用隔离、Build/验证约束 | 约束；本轮只写报告，未运行编译 |
| `D:\TRbackup\NLTX\progress.md` | 当前无活跃迁移批次，源码和验证文件不得被上下文清理改写 | 工作区上下文 |
| `D:\TRbackup\NLTX\docs\第一轮审查\设计\2026-09-05-version4-public-decomposition-common-protocol.md` | 证据状态、四类内容分区、报告最低结构、Integration Handoff | 报告协议 |
| `D:\TRbackup\NLTX\docs\第一轮审查\设计\2026-09-05-version4-leashed-entity-simulation-public-decomposition.md` | `taskNumber: 03`、固定范围、必读证据和不拆分对象 | 子系统任务包 |
| `D:\TRbackup\NLTX\架构设计\ECS文件组织设计约束.md` | 按领域组织、单核心 public type、禁止通用聚合目录 | proposed 路径约束 |
| `D:\TRbackup\NLTX\约束\Google-CSharp-Style-Guide-约束.md` | 命名和文件组织要求 | proposed 代码形状约束 |
| `D:\TRbackup\NLTX\约束\非函数式编码副作用隔离规范.md` | I/O、时钟、随机数、网络、日志和持久化隔离 | System/Adapter 边界约束 |
| `D:\TRbackup\NLTX\docs\Version4权威游戏模拟子系统全量审查报告-2026-09-05.md` | 全量责任面、LeashedEntitySimulation 的既有归类和 NLTX 状态摘要 | 既有审查索引，不替代源码 |
| `D:\TRbackup\NLTX\docs\迁移参考表\Version4子系统索引.json` | `LeashedEntitySimulation` 的责任和排除候选 | 既有索引 |
| `D:\TRbackup\NLTX\docs\迁移参考表\Version4源码覆盖.tsv` | `LeashedEntity.cs`、LeashedEntities prototypes 和两个锚点文件的 primary owner/coverage | 文件级覆盖索引 |
| `D:\TRbackup\NLTX\docs\Version4与完整源码差异附录-2026-09-05.md` | LeashedEntity 被列为独立责任面，且完整参考不扩大 Version4 分母 | 差异边界 |

`docs/flowstate/README.md` 在本工作区不存在；本轮没有用它推断架构结论。

## 4. Version4 真实代码证据表

以下只记录 Version4 中实际读取到的类型、字段、方法和调用邻域。`confirmed` 代表该结构/调用确实存在；当同一文件的行为因空体或缺成员而不能闭合时，记录为 `partial` 或 `missing`。

| evidenceId | 绝对路径与实际行号 | 类型/方法 | 读者与写者 | 生命周期与副作用 | evidenceStatus |
| --- | --- | --- | --- | --- | --- |
| `v4-leash-net-send-001` | `D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:13-39` | `LeashedEntity.NetModule.Deserialize`、`Sync` | `Sync` 被实体添加/section 同步路径使用；`Deserialize` 被 `NetManager.Read` 调用 | `Sync` 创建 packet，写 message type、`whoAmI`、`Type`、full anchor 坐标和 `entity.NetSend`，再定向或按 section 广播；`Deserialize` 当前为空体 | `partial` |
| `v4-leash-registry-002` | `D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:42-90` | `Registry.RegisterAll`、`Register`、`Register<T>` | `Main.Initialize_AlmostEverything` 写入注册表；`CreateLeashedEntity`/接收路径应读类型定义 | 先放 `null` 占位，再按固定顺序注册 1 个 kite 和 18 个 critter prototype，`Type` 来自 `Prototypes.Count` | `confirmed` |
| `v4-leash-section-003` | `D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:94-175` | `SectionEntityList`、`CompactIfNecesary`、`Activate`、`Deactivate`、`Sync` | `GetSection` 创建/读取；更新循环读 `list/count`；section 事件调用 Activate/Deactivate；网络 section 事件调用 Sync | 固定初始数组长度 32；压缩空槽并修正 `sectionSlot`；Activate 调 `Spawn(false)`，Deactivate 调 `Despawn()`；Sync 为每项 full sync；Version4 缺 `Add/Remove` | `partial` |
| `v4-leash-index-004` | `D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:177-194` | `BySection`、`ActiveSectionList`、`ByWhoAmI`、`SectionCoordinates` | 生命周期写索引；更新、查询和广播读索引 | 三套集合分别表达空间索引、活跃 section 工作集和旧 slot 身份；`SectionCoordinates` 从 anchor 坐标派生 | `confirmed` |
| `v4-leash-clear-005` | `D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:196-220` | `Clear(bool keepActiveSections)` | `WorldGen` 在世界清理阶段调用；`ActiveSections` 被可选读取 | 清空 section、slot 和活跃列表，重设容量；`keepActiveSections` 时根据全局 section 活跃状态重激活 | `confirmed` |
| `v4-leash-update-006` | `D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:234-289` | `UpdateEntities`、`RecheckActiveSections`、`_UpdateEntities` | `Main` 每帧写入行为副作用；读取 `ActiveSectionList` 和实体 `active` | 每帧先重检/停用 section，再对 active 实体调 `Update` 和 `StreamNetUpdates`；inactive 实体交给 `Remove`；`Remove` 和 `StreamNetUpdates` 当前为空体 | `partial` |
| `v4-leash-section-sync-007` | `D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:291-297` | `SyncEntitiesInSection` | `RemoteClient.NetSectionActivated` 订阅者调用；读取 section list | 对一个客户端发送该 section 内所有实体的 full sync | `confirmed` |
| `v4-leash-instance-api-008` | `D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:298-306` | `NewInstance`、`Spawn`、`Despawn`、`Update`、`Draw`、`NetSend`、`NetReceive` | Registry/anchor/section lifecycle 和客户端表现分别调用 | 定义策略扩展点；Version4 `NewInstance` 退化为 `new LeashedEntity()`，实体基础运动字段不在该文件中 | `partial` |
| `v4-main-009` | `D:\TRbackup\Version4\Terraria\Main.cs:3338-3349` | `Initialize_AlmostEverything` 片段 | `Main` 写入 registry 初始化；后续实体使用 registry | `TileEntity.InitializeAll`、`Projectile.InitializeStaticThings` 后调用 `LeashedEntity.Registry.RegisterAll()` | `confirmed` |
| `v4-main-update-010` | `D:\TRbackup\Version4\Terraria\Main.cs:11567-11577` | 主更新循环片段 | `Main` 调用 `LeashedEntity.UpdateEntities` | leashed update 位于该主循环的 item 更新之后；不能仅凭片段推断与所有其他系统的全局顺序 | `confirmed` |
| `v4-world-clear-011` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:6662-6667` | 世界清理片段 | `WorldGen` 写 `ActiveSections.Reset` 和 `LeashedEntity.Clear` | 先清全局 section 活跃时间，再清 leashed 索引 | `confirmed` |
| `v4-active-sections-012` | `D:\TRbackup\Version4\Terraria.DataStructures\ActiveSections.cs:6-50` | `ActiveSections`、`CheckSection`、`IsSectionActive`、`Reset` | Player/RemoteClient 等触发 Check；Leashed 静态订阅者读取激活事件 | section 尺寸为 200×150 tile；`SectionInactiveTime=60`；Check 更新 `LastActiveTime` 并在 inactive→active 时触发 `SectionActivated` | `confirmed` |
| `v4-remote-section-013` | `D:\TRbackup\Version4\Terraria\RemoteClient.cs:65,138-208,241-247` | `NetSectionActivated`、`CheckSection`、`CheckSection_ForClient`、`IsSectionActive` | 网络 session 写客户端 section cache；Leashed 订阅事件；`NetMessage` 写 tile section | 服务端全局 `ActiveSections.CheckSection` 先更新；新客户端 section 触发 `NetSectionActivated`，随后发送 tile section；远端 section 有独立 60 tick cache | `confirmed` |
| `v4-network-registration-014` | `D:\TRbackup\Version4\Terraria.Initializers\NetworkInitializer.cs:10-29`、`Terraria.Net\NetManager.cs:28-65,68-125` | `NetworkInitializer.Load`、`NetManager.Register/GetId/Read/Broadcast/SendToClient` | initializer 写 module 注册序；`CreatePacket` 读 `GetId`；`MessageBuffer` 读 `NetManager.Read` | Leashed module 是当前注册序列第 14 项，0-based module ID 为 13；该 ID 依赖注册顺序；`NetManager` 将低层 packet 副作用提交给 socket | `confirmed` |
| `v4-message-buffer-015` | `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:2518-2526,3310-3319` | NetModule case 82、tile item case 156 | 网络接收写 `NetManager.Read` 或锚点 `InsertItem` | case 82 将 module frame 交给 Leashed `Deserialize`；case 156 读取 anchor 坐标/item 并写入 `TELeashedEntityAnchorWithItem.InsertItem` | `confirmed` |
| `v4-anchor-host-016` | `D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELeashedEntityAnchor.cs:5-12`、`TELeashedEntityAnchorWithItem.cs:6-36` | anchor base and item host | TileEntity manager/world/tile placement 读写；`InsertItem` 调 `RespawnLeashedEntity` | 当前基类的 placement、removed、world-load、respawn 都为空体；item host 保留 `itemType`、掉落和插入签名，但持久化读写为空体 | `partial` |
| `v4-anchor-registration-017` | `D:\TRbackup\Version4\Terraria.DataStructures\TileEntitiesManager.cs:30-45,47-85` | `RegisterAll`、`Register`、`GenerateInstance`、`NetPlaceEntity` | TileEntity manager 写 `_types`；tile placement/网络读注册类型 | 注册 `TEKiteAnchor` 和 `TECritterAnchor`；manager 只负责 TileEntity 类型，不拥有 Leashed registry | `confirmed` |
| `v4-tileentity-lifecycle-018` | `D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs:80-145` | `Add`、`Kill`、`Remove`、`InitializeAll` | TileEntity manager 写 `ByID/ByPosition/UpdateEntities`；`Remove` 调实体 `OnRemoved` | TileEntity 移除完成后调用 `OnRemoved`；锚点的 OnRemoved 是 Leashed 关系的触发边界 | `confirmed` |
| `v4-anchor-tile-hooks-019` | `D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:4517,4549`、`WorldGen.cs:52726-52729,52897-52904` | placement/break/kill hooks | tile placement、tile break 和 tile destroy 写 anchor 生命周期 | 723/724 分别绑定 kite/critter anchor；break 调 item drop；destroy 调对应 TileEntity `Kill` | `confirmed` |
| `v4-world-load-020` | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:3391-3392,3440-3443` | TileEntity save/load callback | WorldFile 遍历 TileEntity；anchor `OnWorldLoaded` 是 respawn 边界 | 当前可见路径遍历 `TileEntity.ByID` 并调用 `OnWorldLoaded`；Version4 anchor OnWorldLoaded 为空，完整参考补证其恢复实体行为 | `partial` |

### 4.1 Version4 的关键真实顺序

下列顺序只表达已由源码调用关系支持的局部顺序，不把文件排列当成运行时调度顺序：

```text
Main.Initialize_AlmostEverything
  -> LeashedEntity.Registry.RegisterAll

NetworkInitializer.Load
  -> NetManager.Register<LeashedEntity.NetModule>

RemoteClient.CheckSection_ForClient
  -> ActiveSections.CheckSection
  -> ActiveSections.SectionActivated
  -> LeashedEntity section activation subscriber [完整参考明确；Version4 静态订阅构造函数缺失]

RemoteClient.CheckSection_ForClient
  -> RemoteClient.NetSectionActivated
  -> LeashedEntity.SyncEntitiesInSection
  -> SectionEntityList.Sync
  -> LeashedEntity.NetModule.Sync(full: true, toClient)

Main update loop
  -> LeashedEntity.UpdateEntities
  -> RecheckActiveSections
  -> SectionEntityList.Deactivate [若过期]
  -> _UpdateEntities
  -> entity.Update
  -> entity.StreamNetUpdates [完整参考补证；Version4 为空]
  -> Remove [若 entity.active == false；Version4 为空]

WorldGen world clear
  -> ActiveSections.Reset
  -> LeashedEntity.Clear
```

## 5. 完整参考源码补证表

完整参考目录为 `D:\TRbackup\无任何删减通过编译`。它只补足 Version4 已存在类型/成员的明确缺口，不扩大 Version4 覆盖基线。以下是“参考补证”，不是“Version4 当前实现”。

| supplementId | 完整参考路径与行号 | 补证内容 | 对 Version4 的解释 | evidenceStatus |
| --- | --- | --- | --- | --- |
| `ref-leash-net-receive-001` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs:13-119` | `Deserialize` 解析 Remove/FullSync/PartialSync；full sync 按 slot 扩展 `ByWhoAmI`、从 `Registry.Get(type).NewInstance()` 创建并应用 `NetReceive`；partial sync 检查 type | Version4 的 `Deserialize` 空体，故只能确认 protocol seam，不能确认当前 Version4 客户端 apply 行为 | `partial` |
| `ref-leash-registry-002` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs:121-177` | `Registry.Get(int)` 返回 `Prototypes[type]` | 与 Version4 `Register` 的类型 ID 逻辑同文件同调用设计相符 | `confirmed` |
| `ref-leash-section-003` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs:179-283` | `Add` 支持数组扩容并写 `sectionSlot`；`Remove` 清槽并增加 `emptySlots`；Activate/Deactivate 仅在非 client 调 Spawn/Despawn | 补足 Version4 section list 的缺失 mutation 和 server/client 守卫 | `partial` |
| `ref-leash-static-owner-004` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs:340-350` | 初始化 `BySection`、`ActiveSectionList`、`ByWhoAmI`，订阅 `ActiveSections.SectionActivated` 和 `RemoteClient.NetSectionActivated` | 解释 Update/Sync 调用链如何接入 section 事件；Version4 缺少该静态构造函数，是关键 evidence-gap | `partial` |
| `ref-leash-add-005` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs:377-411` | `AddNewEntity` 选择空 slot/尾部 slot，写 anchor、active、whoAmI，加入 section；服务器活跃 section 触发 Spawn，server 发送 full sync | 提供锚点恢复、网络 full sync 和 slot 分配的补证顺序 | `confirmed` |
| `ref-leash-remove-006` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs:425-441` | `Remove` 清 active、`ByWhoAmI`、section slot；服务器发送 `NetModule.Remove` | Version4 `Remove` 空体，当前 V4 删除/断线广播未闭合 | `partial` |
| `ref-leash-instance-state-007` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs:285-338` | 完整参考含 `position`、`velocity`、`direction`、`width`、`height`、`Center`、`Size` 和 `StreamingRate=1024` | 证明这些是行为/网络状态候选，但 Version4 文件未包含它们，不能静默加入 V4 事实 | `partial` |
| `ref-leash-stream-008` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs:511-520` | server 且 `((GameUpdateCount + whoAmI) & 0x3FF)==0` 时发送 partial sync | 补证 1024 tick 错位流式更新；需 focused verifier 后才能承诺兼容 | `partial` |
| `ref-leash-draw-009` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs:528-549` | `DrawEntities` 按 active section 和屏幕扩张矩形调用实体 Draw | 只建立客户端 presentation 的证据；当前 Version4 直接调用者未检索到，调用顺序为 evidence-gap | `partial` |
| `ref-leash-instance-010` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs:551-559` | `NewInstance` 用运行时类型创建非公开实例并复制 `Type` | Version4 `new LeashedEntity()` 会丢失派生类型，属于高风险删减点 | `partial` |
| `ref-anchor-base-011` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Tile_Entities\TELeashedEntityAnchor.cs:7-63` | placement 网络消息、OnRemoved→Despawn、OnWorldLoaded→Respawn、Respawn→Create→AddNewEntity | 锚点是生命周期触发宿主；关系实例仍由 Leashed registry 管理 | `confirmed` |
| `ref-anchor-item-012` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Tile_Entities\TELeashedEntityAnchorWithItem.cs:8-80` | `itemType` short 持久化；服务端掉落守卫；world-load `FitsItem` 校验；placement 的 client message 156/server InsertItem 分支 | Version4 相同成员存在但多个体为空；不可声称当前 V4 item 持久化或客户端守卫已闭合 | `partial` |
| `ref-derived-anchor-013` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Tile_Entities\TEKiteAnchor.cs:23-74`、`D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Tile_Entities\TECritterAnchor.cs:25-115` | tile 723/724 校验；kite item compatibility；critter `makeNPC` 映射和 prototype lookup | 证明 derived anchor 是定义映射/宿主，不是 Leashed registry owner | `partial` |
| `ref-behavior-014` | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\LeashedCritter.cs:47-249`、`D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\WalkerLeashedCritter.cs:71-194`、`D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs:42-172`、`D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs:44-297`、`D:\TRbackup\无任何删减通过编译\Terraria.GameContent.LeashedEntities\LeashedKite.cs:51-281` | critter/kite 的 defaults、运动、随机状态、网络序列化、dummy 转换和 draw | behavior state 应从生命周期容器中分离；Version4 stub 不足以证明完整运动等价 | `partial` |

### 5.1 完整参考揭示的行为与网络状态

`LeashedCritter.NetSend` 的 full 负载包括 `npcType`、`Size`，随后是相对 anchor 的 packed position、facing direction、random state、wait time、state 和相对 anchor 的 target X/Y；某些 prototype 还有 full-only extension。`LeashedKite.NetSend` 的 full 负载包括 projectile type，随后是 position、packed velocity、rotation byte、wind/cloud/time 状态等。

这说明 `NetModule.Sync` 当前接受的是可变 `LeashedEntity` 实例，并直接调用其 `NetSend`，不是不可变快照。提议的 `LeashedEntityReplicationSnapshot` 必须在核心模拟与低层 writer 之间建立 seam；它不是对现有行为的已完成替换。

## 6. tModLoader 公开 API 交叉验证表

本地文档目录为 `D:\TRbackup\tmodloader-api-docs-stable`，页面版本为 `tModLoader v2026.07`。这些文档只用于交叉验证公开边界，不替代 Version4 私有行为。

| apiEvidenceId | 文档文件、页面标题、成员锚点 | 公开语义 | 可用于本报告的边界 | evidenceStatus |
| --- | --- | --- | --- | --- |
| `tml-mod-packet-001` | `class_mod_packet.html:90,94`；`ModPacket Class Reference` | `ModPacket` 用于 server/client 间任意数据同步并继承 `BinaryWriter` | 支持把 `ReplicationAdapter` 与序列化 writer 隔离；不能证明 Terraria 私有 `LeashedEntity.NetModule` 的负载 | `confirmed` |
| `tml-mod-packet-send-002` | `class_mod_packet.html:101`，`Send`；锚点 `#a54393cf0ee63ef5c497a6850d4e8b688`；详细语义 `:139-142` | client 调用发送到 server；server 可发全部、指定或排除 client；client→server 通常需要 server relay | 支持 proposed network direction、指定客户端 full sync 和 server relay 设计 | `confirmed` |
| `tml-mod-tile-entity-003` | `class_mod_tile_entity.html:93,97`；`ModTileEntity Class Reference` | TileEntity 与 tile 紧耦合；`TileEntity.Update` 只在 SP/server 调用，不在 client 调用 | 支持把 anchor host 作为 server-side lifecycle boundary；不能把 tModLoader 生命周期当成 Version4 私有实现 | `confirmed` |
| `tml-mod-tile-entity-hooks-004` | `class_mod_tile_entity.html:110,116-123,126-148` | 公开 `GenerateInstance`、placement、`IsTileValidForEntity`、`Kill`、`NetPlaceEntityAttempt`、`NetReceive`、`NetSend`、`OnKill` | 交叉验证 proposed `AnchorPort` 应覆盖合法性、placement、删除和网络方向 | `confirmed` |
| `tml-tile-entity-net-005` | `class_tile_entity.html:367-389`、`:403-419` | `NetReceive` 只在 client 调用，且可能应用到替换旧实例；`NetSend` 只在 server 调用 | 支持 server send/client projection 的方向；不能证明 Leashed entity 的私有 full/partial 协议 | `confirmed` |
| `tml-tile-entity-save-006` | `class_tile_entity.html:665-689`，`WriteExtraData` | TileEntity extra data 有独立 writer seam | 支持把 anchor item/关系持久化放入 `PersistenceAdapter`，而不是把 BinaryWriter 放进组件 | `confirmed` |
| `tml-net-message-007` | `class_net_message.html:139-149`，`SendData` | vanilla `MessageID` 消息的 server/client 发送入口，描述 relay 与 `MessageBuffer.GetData` 接收 | 仅用于比较 tile placement message 的公开方向；不能替代 Version4 `NetMessage`/`MessageBuffer` 证据 | `confirmed` |
| `tml-leashed-api-008` | `index.html`、`annotated.html:1351,1381` 精确检索 `LeashedEntity` 与 `ModLeashedEntity` 未命中；仅出现 `ModPacket`、`ModTileEntity` | 没有证据证明存在公开 `ModLeashedEntity` API | 报告不声称公开 API，也不以 tModLoader 推断 Leashed 行为 | `missing` |

## 7. Space Station 14 最小相关 ECS 参考表

Space Station 14 无直接对应 Terraria `LeashedEntity` 的证据；以下仅用于参考 Component、System、Event、关系清理以及 spawn/despawn 的结构粒度，不能推断 Terraria 行为，也不复制其命名、目录或领域语义。

| ss14EvidenceId | 实际读取文件与类型/方法 | 结构参考 | 不可推断的内容 | evidenceStatus |
| --- | --- | --- | --- | --- |
| `ss14-anchorable-001` | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Construction\Components\AnchorableComponent.cs:8-18,35-117`；`AnchorableComponent`、attempt/completed events | Component 保存锚定配置/状态，事件表达尝试与完成阶段 | 不推断 Terraria anchor 规则或 tile 语义 | `confirmed` |
| `ss14-anchorable-system-002` | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Construction\EntitySystems\AnchorableSystem.cs:25,41-54,70-73,130-203,210-379`；`AnchorableSystem` | System 订阅事件，做验证、状态转换、碰撞查询和显式提交 | 不推断 Version4 section 或 leash 的顺序 | `confirmed` |
| `ss14-spawn-query-003` | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Spawning\EntitySystemExtensions.cs:10-52`；`SpawnIfUnobstructed` | spawn 资格查询与 SpawnEntity 副作用分离 | 不复制其 collision 规则 | `confirmed` |
| `ss14-relation-cleanup-004` | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Teleportation\Systems\LinkedEntitySystem.cs:12-33,46-69,105-134`；`ComponentShutdown`、`TryLink`、`TryUnlink` | 对称关系、shutdown cleanup、空关系删除应有显式边界 | 不推断 Terraria anchor relation 是否对称或可恢复 | `confirmed` |
| `ss14-station-anchor-005` | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Shuttles\Components\StationAnchorComponent.cs:5-10`；`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Shuttles\Systems\StationAnchorSystem.cs:9-85` | Component 保存跨域状态，System 通过事件驱动外部提交 | 不把 station 语义或类型名带入 NLTX | `confirmed` |

## 8. 成员、字段、方法、读写者和生命周期盘点

### 8.1 Version4 成员分类

| 成员/区域 | 分类 | 写入者 | 读取者 | 生命周期 | 设计处理 |
| --- | --- | --- | --- | --- | --- |
| `Registry.Prototypes` | authority definition store | `RegisterAll/Register` | `Registry.Get`、实例创建、网络 full receive | 内容初始化至进程结束 | `DefinitionRegistry`；不得与实例状态合并 |
| `ByWhoAmI` | authority instance index + legacy network slot map | `AddNewEntity/Remove`（完整参考补证） | network receive、查询、remove | spawn 到 remove；clear 时重置 | 身份 map 与稳定实体 ID 分开，`whoAmI` 只作兼容候选 |
| `BySection[,]` | spatial index/cache | `GetSection`、section add/remove | activation、update、full sync | world session 生命周期 | `SectionIndexComponent/Query`；不拥有定义或行为 |
| `ActiveSectionList` | active work-set/cache | section `Activate/Deactivate` | `RecheckActiveSections/_UpdateEntities/DrawEntities` | world/session 生命周期 | `SectionActivationSystem` 的派生工作集 |
| `SectionEntityList.list/count/emptySlots` | section bucket/cache | Add/Remove/Compact | update/sync/activation | entity 在 section 中期间 | proposed 索引；压缩不能改变实体稳定身份 |
| `sectionSlot` | bucket-local derived index | Add/Compact | Remove | entity 在 bucket 中期间 | 不作为持久化 ID，不传给客户端 |
| `active` | instance lifecycle authority | Add/Spawn/behavior/anchor remove | update/remove/section lifecycle | inactive→active→removed | `LeashedEntityLifecycleComponent`；所有写者需集中到 command/system |
| `whoAmI` | legacy network slot | AddNewEntity/网络映射 | Sync/Remove/decoder | slot 分配到 remove | `crossSubsystemOwner: integration-review` |
| `Type` | runtime definition ID | Registry registration | Sync、NewInstance、behavior dispatch | definition registry 生命周期 | domain definition ID 候选；跨内容协议时需整合 owner |
| `AnchorPosition` | anchor coordinate relation fact | AddNewEntity | section derivation、full sync、behavior target | entity 生命周期 | 与 runtime anchor reference、persistent coordinate 分层 |
| `SectionCoordinates` | derived spatial value | 不直接写；由 anchor 坐标计算 | section index、broadcast filter | 每次访问派生 | Query/adapter 计算，不作为独立权威字段 |
| `position/velocity/direction/width/height` | behavior/motion state（完整参考才有） | behavior Update/Spawn/NetReceive | Draw、NetSend、movement strategy | active instance 生命周期 | `MotionComponent`；Version4 文件缺失，列为证据缺口 |
| `LeashedCritter` fields | behavior state | `SetDefaults/Spawn/Update/NetReceive` | behavior helpers、Draw、NetSend | critter instance 生命周期 | behavior-specific components/strategy，不塞进通用 state |
| `LeashedKite` fields | kite behavior state | `SetDefaults/Update/NetReceive` | Draw、NetSend | kite instance 生命周期 | kite-specific behavior component/strategy |
| `NetModule.Sync` | protocol side effect | server update/add/section sync | NetManager/socket | packet construction时短生命周期 | `ReplicationAdapter`；输入应改为 snapshot seam |
| `NetModule.Deserialize` | client input/apply boundary | network receive | ByWhoAmI/Registry/NetReceive | packet 到达时 | parser 与 apply command 分开；Version4 当前为空 |
| `Draw`/`DrawEntities` | client projection | client frame/presentation | sprite/NPC/projectile dummy | frame 生命周期 | `ClientProjection`；不得写 simulation state |

### 8.2 生命周期和副作用

| 阶段 | 纯计算/查询 | 权威写入 | 外部副作用 | 失败/重试注意 |
| --- | --- | --- | --- | --- |
| 注册 | 定义顺序和 Type 分配 | prototype registry | 无或内容初始化日志 | 注册顺序是协议 ID 依赖，必须拒绝重复/顺序漂移 |
| Anchor placement | tile 合法性、item compatibility | TileEntity/anchor relation、spawn command | client placement message 或 server tile placement | client 输入不能直接写 Leashed state；server 需验证坐标与 item |
| World load | anchor→definition 可恢复性 | relation、entity command | 存档读取和必要的 full projection | 无效 item/prototype 需确定丢弃、保留或 tombstone 策略 |
| Section activation | `IsSectionActive` 查询 | active work-set、Spawn/Despawn state | server entity spawn/despawn；客户端 full sync | 同一事件重复到达必须幂等；激活前后的 spawn 顺序需锁定 |
| Per-tick update | target、motion、dirty 判定 | motion/behavior/lifecycle | server partial sync | 时钟、随机数、网络均不得隐藏在纯 Query 中 |
| Anchor removal | existence/relationship query | lifecycle inactive、despawn/remove command | item drop、Remove packet | anchor removal 与 entity remove 不能互相递归；应有一次性提交 ID |
| World clear | 是否保留 active section | clear indexes/relations | session/network reset | `ActiveSections.Reset` 与 Leashed clear 顺序应保持，重建需幂等 |

## 9. 权威状态所有权表

| 状态 | Version4 事实 owner | proposed owner | 其他读者/写者 | 结论 |
| --- | --- | --- | --- | --- |
| definition registry | `LeashedEntity.Registry` | `LeashedEntityRegistry` (`status: proposed`) | anchor mapping、network full receive | Leashed 独占候选 |
| 实例 active/type/anchor 生命周期 | `LeashedEntity` 实例及其集合 | `LeashedEntityStateComponent` + lifecycle system (`status: proposed`) | anchor、section system、replication | Leashed 独占候选 |
| motion/behavior fields | 派生类实例（完整参考补证） | `LeashedEntityMotionComponent` 与 behavior strategy (`status: proposed`) | update、snapshot、client projection | Leashed 独占候选，具体字段需实现阶段核对 |
| section bucket | `BySection`/`SectionEntityList` | `LeashedEntitySectionComponent` + section query (`status: proposed`) | WorldStorage/Network section visibility | 这是 index/cache，不是全局 section 真相 |
| global section activity | `ActiveSections.LastActiveTime`；客户端另有 `RemoteClient.TileSectionsCheckTime` | `ILeashedEntitySectionActivityPort` (`status: proposed`) | `RemoteClient`、WorldStorage、Leashed section system | `crossSubsystemOwner: integration-review` |
| anchor tile/item | `TileEntity`/anchor derived types | 外部 `AnchorFact` 输入与 `LeashedEntityAnchorRelationComponent` 消费边界 (`status: proposed`) | WorldStorage、WorldInteraction、placement | anchor host 不转移为 Leashed registry owner |
| stable runtime identity | Version4 没有独立 persistent ID；`whoAmI` 是 int slot | `LeashedEntityRuntimeId` candidate (`status: proposed`) | storage、network、queries | `crossSubsystemOwner: integration-review` |
| `whoAmI`/module slot | Version4 network compatibility slot | adapter-side `LegacyLeashedEntitySlot` candidate (`status: proposed`) | NetModule、client decoder | 不应作为唯一权威 identity |
| persistent relation ID | Version4 证据未证明 | `PersistentEntityId`/anchor coordinate candidate (`status: proposed`) | PersistenceAndRecovery、WorldStorage | `crossSubsystemOwner: integration-review` |
| replication snapshot/version | Version4 直接读可变实例；无 revision | `LeashedEntityReplicationSnapshot` (`status: proposed`) | NetworkSession、client projection | `crossSubsystemOwner: integration-review` |

## 10. proposed 组件拆分

下表全部是设计候选，不表示文件已创建或类型已实现。所有目标类型和路径均为 `status: proposed`；其中标有 `crossSubsystemOwner: integration-review` 的类型不能由本报告单独定 owner。

| proposedId | 类型与目标路径 | 关键字段/职责 | 不应持有 | status |
| --- | --- | --- | --- | --- |
| `design-definition-001` | `LeashedEntityDefinition`；`dome/src/Terraria.Dome.Simulation/Leash/Definitions/LeashedEntityDefinition.cs` | `DefinitionId`、behavior kind、content kind、default size、anchor compatibility、replication kind | runtime active、slot、socket、tile store | `proposed` |
| `design-registry-002` | `LeashedEntityRegistry`；`dome/src/Terraria.Dome.Simulation/Leash/Definitions/LeashedEntityRegistry.cs` | 固定注册顺序、唯一 ID 检查、只读 lookup、实例 factory/strategy | entity instances、section activity、network writer | `proposed` |
| `design-state-003` | `LeashedEntityStateComponent`；候选路径 `dome/src/Terraria.Dome.Simulation/Leash/LeashedEntityStateComponent.cs`（当前工作区已有草图，不修改） | runtime identity、definition ID、active/lifecycle、spawned | motion details、legacy wire bytes、anchor TileEntity object | `proposed` |
| `design-motion-004` | `LeashedEntityMotionComponent`；`dome/src/Terraria.Dome.Simulation/Leash/Components/LeashedEntityMotionComponent.cs` | position、velocity、direction、size、target、net offset | registry membership、I/O、random service instance | `proposed` |
| `design-section-005` | `LeashedEntitySectionComponent`；`dome/src/Terraria.Dome.Simulation/Leash/Components/LeashedEntitySectionComponent.cs` | derived section、bucket slot、membership state、section revision seen | global `ActiveSections` truth、persistent ID | `proposed` |
| `design-anchor-relation-006` | `LeashedEntityAnchorRelationComponent`；`dome/src/Terraria.Dome.Simulation/Leash/Components/LeashedEntityAnchorRelationComponent.cs` | anchor runtime reference candidate、anchor tile coordinate、persistent relation candidate、relation state | owning TileEntity data、item inventory mutation | `proposed` |
| `design-lifecycle-007` | `LeashedEntityLifecycleComponent`；`dome/src/Terraria.Dome.Simulation/Leash/Components/LeashedEntityLifecycleComponent.cs` | `Inactive/Active/Despawning/Removed`、transition sequence、spawned flag | direct socket/persistence effects | `proposed` |
| `design-section-activation-008` | `LeashedEntitySectionActivationSystem`；`dome/src/Terraria.Dome.Simulation/Leash/Systems/LeashedEntitySectionActivationSystem.cs` | consume section activated/deactivated facts；emit idempotent spawn/despawn commands | section global truth、network bytes | `proposed` |
| `design-update-009` | `LeashedEntityUpdateSystem`；`dome/src/Terraria.Dome.Simulation/Leash/Systems/LeashedEntityUpdateSystem.cs` | query active entities、调用 behavior strategy、写 motion/dirty result | tile I/O、socket、draw、persistence | `proposed` |
| `design-spawn-command-010` | `LeashedEntitySpawnCommand`；`dome/src/Terraria.Dome.Simulation/Leash/Commands/LeashedEntitySpawnCommand.cs` | anchor fact、definition ID、request/transaction ID、expected relation revision | implicit global singleton mutation | `proposed` |
| `design-despawn-command-011` | `LeashedEntityDespawnCommand`；`dome/src/Terraria.Dome.Simulation/Leash/Commands/LeashedEntityDespawnCommand.cs` | runtime ID、reason、expected lifecycle revision、remove-after-despawn policy | client projection mutation | `proposed` |
| `design-registry-query-012` | `LeashedEntityRegistryQuery`；`dome/src/Terraria.Dome.Simulation/Leash/Queries/LeashedEntityRegistryQuery.cs` | 纯读 definition/strategy lookup | component、registry、network 写入 | `proposed` |
| `design-section-query-013` | `LeashedEntitySectionQuery`；`dome/src/Terraria.Dome.Simulation/Leash/Queries/LeashedEntitySectionQuery.cs` | 纯读 section membership/relevance，输入 session/section fact | active flag、socket、world storage mutation | `proposed` |
| `design-snapshot-014` | `LeashedEntityReplicationSnapshot`；`dome/src/Terraria.Dome.Simulation/Leash/Snapshots/LeashedEntityReplicationSnapshot.cs` | immutable full/partial payload model、definition/anchor/runtime slot、motion/behavior state、revision candidate | `BinaryWriter`、mutable ECS refs、socket | `proposed`; `crossSubsystemOwner: integration-review` |
| `design-replication-adapter-015` | `LeashedEntityReplicationAdapter`；`dome/src/Terraria.Dome.Simulation/Leash/Adapters/LeashedEntityReplicationAdapter.cs` | snapshot→legacy module 13 and message 0/1/2；client decode→validated apply command/projection | authoritative update logic、registry ownership | `proposed` |
| `design-persistence-adapter-016` | `LeashedEntityPersistenceAdapter`；`dome/src/Terraria.Dome.Simulation/Leash/Adapters/LeashedEntityPersistenceAdapter.cs` | anchor relation、definition/item mapping、stable ID、recovery validation | binary format details inside components | `proposed`; `crossSubsystemOwner: integration-review` |
| `design-client-projection-017` | `LeashedEntityClientProjection`；`dome/src/Terraria.Dome.Simulation/Leash/Projection/LeashedEntityClientProjection.cs` | full/partial state mirror、visible entity draw input、remove/tombstone | server authoritative components、spawn commands | `proposed` |

## 11. System、Query、Command、Adapter、Projection 边界

### 11.1 契约表

| 边界 | 输入 | 输出/提交 | Seam | Depth | Leverage | Locality | 失败与重试 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `LeashedEntityRegistry` (`status: proposed`) | ordered definitions、content compatibility | read-only definition/behavior factory | `RegistryQuery` | high | high | high | duplicate/unknown ID 在初始化时拒绝；不可悄悄重排网络 ID |
| `SectionActivationSystem` (`status: proposed`) | section activation facts、current lifecycle | idempotent Spawn/Despawn commands | `ILeashedEntitySectionActivityPort` (`status: proposed`) | high | high | medium | 重复激活不重复 Spawn；过期停用不删除 durable relation |
| `UpdateSystem` (`status: proposed`) | active state、motion、behavior definition、clock/random ports | component mutation + replication dirty result | `ILeashedEntityClockPort`、`ILeashedEntityRandomPort` (`status: proposed`) | high | high | high | 单实体行为失败需定义 quarantine/remove；不能吞掉全局异常 |
| `SpawnCommand` executor (`status: proposed`) | validated anchor + definition | entity state、section membership、Spawn event | `ILeashedEntityAnchorPort` (`status: proposed`) | high | high | medium | relation revision 冲突拒绝或重读；提交必须幂等 |
| `DespawnCommand` executor (`status: proposed`) | runtime ID、reason、expected revision | lifecycle/remove + optional Remove snapshot | same anchor/relation port | high | high | medium | 已 despawn/removed 视为幂等成功；未找到记录输出可审计结果 |
| `RegistryQuery/SectionQuery` (`status: proposed`) | immutable state view | read-only result | ECS query API | medium | medium | high | 不写状态、不触发 lazy creation，不生成网络副作用 |
| `ReplicationAdapter` (`status: proposed`) | immutable snapshot + session relevance | bytes/frames or validated client apply command | `ILeashedEntityReplicationPort` (`status: proposed`) | high | high | low | malformed/truncated/type mismatch 拒绝；重复 full/partial 需幂等/版本规则 |
| `PersistenceAdapter` (`status: proposed`) | durable anchor relation snapshot | load commands/save records | `ILeashedEntityPersistencePort` (`status: proposed`) | high | high | low | 读档缺字段/无效 item 使用显式 policy；保存提交失败不能半写权威状态 |
| `ClientProjection` (`status: proposed`) | accepted full/partial projection | draw-ready view/tombstone | presentation port | medium | medium | medium | 客户端断包不反写 server；无 projection 不影响 server simulation |

### 11.2 接口草图（全部为 proposed）

以下仅是非编译契约示意；每个类型、路径和签名均为 `status: proposed`，不能直接当作当前 NLTX API。

```text
// status: proposed
interface ILeashedEntitySectionActivityPort
{
  // status: proposed
  SectionActivityFact Read(IntegrationReviewWorldSectionId section);
}

// status: proposed; crossSubsystemOwner: integration-review
interface ILeashedEntityAnchorPort
{
  // status: proposed
  AnchorFact ReadAnchor(IntegrationReviewTileCoordinate coordinate);
  // status: proposed
  bool IsCompatible(AnchorFact anchor, LeashedEntityDefinition definition);
}

// status: proposed
interface ILeashedEntityReplicationPort
{
  // status: proposed
  ReplicationSendResult Send(LeashedEntityReplicationSnapshot snapshot,
                             ReplicationAudience audience);
}

// status: proposed
interface ILeashedEntityPersistencePort
{
  // status: proposed
  LeashedEntityPersistenceRecord? Load(IntegrationReviewPersistentEntityId id);
  // status: proposed
  PersistenceCommitResult Save(LeashedEntityPersistenceRecord record);
}
```

这里的 `IntegrationReview*` 只是为了显式标记跨子系统值对象尚未定 owner；它们不是建议在当前报告中创建的共享基础类型。最终整合会话应决定使用现有 `EntityReference`、`SectionCoordinate`、`TileCoordinate` 或新值对象。

## 12. 调用方向和 proposed System 顺序

### 12.1 外部调用方向

```text
WorldInteraction / TileEntity host
  -> validated AnchorFact
  -> LeashedEntitySpawnCommand / LeashedEntityDespawnCommand
  -> LeashedEntity lifecycle owner

WorldStorage / NetworkSession section activity
  -> SectionActivityFact
  -> LeashedEntitySectionActivationSystem
  -> Spawn/Despawn command executor

LeashedEntityUpdateSystem
  -> behavior strategy / motion calculation
  -> immutable ReplicationSnapshot
  -> LeashedEntityReplicationAdapter
  -> NetworkSessionAndSectionStreaming

NetworkSessionAndSectionStreaming
  -> protocol decode
  -> validated client projection input
  -> LeashedEntityClientProjection

PersistenceAndRecovery
  <-> LeashedEntityPersistenceAdapter
```

### 12.2 建议但尚未裁决的调度顺序

下面是 `status: proposed` 的调度契约，不是当前 NLTX 已有 scheduler：

```text
1. ConsumeSectionActivityFacts
2. ApplyValidatedAnchorCommands
3. ExecuteLeashedEntitySpawnCommands
4. ExecuteLeashedEntityDespawnCommands
5. UpdateLeashedEntityBehaviorAndMotion
6. RebuildSectionIndexAndDirtySet
7. BuildImmutableReplicationSnapshots
8. ReplicateToRelevantClients
9. ApplyClientProjectionAndDraw
```

约束：

- `Spawn` 必须在 active section 的第一次 server update 之前完成，或明确采用延迟到下一 tick 的策略；不能由文件顺序隐式决定。
- `Despawn`/Remove 必须先使实体不再进入后续 update，再清除 slot/index，并在需要时产生一次 Remove projection。
- `Update` 产生 snapshot 后，Adapter 不得重新读取正在变化的 mutable instance 以修改 snapshot 内容。
- `ClientProjection` 永远在 server authoritative commit 之后，只能读投影输入。
- 与 `ProjectileSimulation`、`SpatialSimulation`、`NetworkSessionAndSectionStreaming` 的最终相对顺序属于 `blocking-decision`。

## 13. 持久化、网络和客户端投影边界

### 13.1 持久化

Version4 的 WorldFile 会保存/加载 TileEntity 集合并在 `WorldFile.cs:3440-3443` 调用每个 TileEntity 的 `OnWorldLoaded`。完整参考的 anchor item host 在 `TELeashedEntityAnchorWithItem.cs:10-23` 将 `itemType` 以 short 读写，并在 world load 先执行 `FitsItem` 再恢复实体。Version4 对应方法为空，因此当前 V4 不能证明 anchor item 或 Leashed 关系已正确持久化。

proposed `LeashedEntityPersistenceAdapter` 只应保存可恢复的 durable facts：

- anchor tile coordinate 或最终裁决的 persistent anchor ID；
- definition/type 与 item/NPC/projectile content mapping；
- 最终裁决的 stable runtime/persistent entity ID；
- relation revision、有效性和恢复策略。

瞬时 `position/velocity/random cursor/wait/state` 是否进入存档必须由行为兼容要求决定，不能因完整参考字段存在就自动持久化。保存失败、旧版本缺字段、invalid item 和 duplicate relation 都需要显式结果，不应在 Adapter 中吞异常。

### 13.2 网络

Version4 wire facts：


- `NetworkInitializer.Load` 将 `LeashedEntity.NetModule` 注册在第 14 个注册位置；`NetManager.Register` 从 0 递增，因此当前 module ID 与 `dome/src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs:44` 的 `LeashedEntityModuleId = 13` 一致。
- `NetModule.Sync` 的 message type 为 full=`1`、partial=`2`；完整参考还定义 Remove=`0`。
- full 带 `whoAmI`、`Type`、anchor X/Y 和 subtype full payload；partial 不带 anchor 坐标。
- full reference 的 `StreamNetUpdates` 以 1024 tick 为周期，并以 `whoAmI` 错位；Version4 此方法为空体。
- `MessageBuffer.cs:2525` 将 case 82 交给 `NetManager.Read`；完整参考 `Deserialize` 才进一步做 full/partial/remove apply。

proposed Adapter 应按以下方向工作：

```text
authority state
  -> immutable snapshot
  -> legacy module encoder (module 13, message 0/1/2)
  -> relevant connected client

client frame
  -> bounded decoder
  -> validate type/slot/anchor/revision
  -> projection apply or validated command
```

低层 `BinaryReader/BinaryWriter`、module ID、socket 和 client list 不能进入 `LeashedEntityStateComponent`。full/partial 的版本、乱序、重复和断线重连语义尚未由 Version4 完整证据闭合。

### 13.3 客户端投影

完整参考 `DrawEntities`（`LeashedEntity.cs:528-549`）根据 active section 和屏幕扩张矩形调用实体 `Draw`；`LeashedCritter` 使用 NPC dummy，`LeashedKite` 使用 projectile dummy。它们是 presentation projection，不是 `NPC` 或 `Projectile` 权威集合的写入者。当前 Version4 直接调用 `DrawEntities` 的调用者尚未在本轮证据中找到，因此 draw 入口与主更新顺序标记为 `evidence-gap`。

## 14. 当前 NLTX 映射

任务包中的固定元数据写作 `currentNltxStatus: missing`，当前工作区虽然实际存在结构草图和协议解析文件，但仍没有闭合的 authoritative registry、section lifecycle、anchor relation、spawn/despawn/update chain 或 focused verifier。因此本报告采用 `nltxStatus: missing`；表中局部文件使用 `partial` 说明其存在的模型片段，不把片段升级为实现。

| 当前路径 | 实际内容 | 缺少的闭环 | 状态 |
| --- | --- | --- | --- |
| `D:\TRbackup\NLTX\src\WorldInteraction\TileEntities\LeashedEntityAnchorComponent.cs:3-7` | `ItemType`、`HasItem` | 无 Leashed registry/lifecycle link、验证和提交端口 | `partial` |
| `D:\TRbackup\NLTX\src\WorldInteraction\TileEntities\TileEntityAnchorComponent.cs:5-9` | `Origin` | 无持久化关系和 spawn/despawn command | `partial` |
| `D:\TRbackup\NLTX\src\WorldStorage\TileEntityStore.cs:3-13` | ID/anchor dictionaries、revision/count 属性 | 无操作、事务、Leashed relation 和 recovery | `partial` |
| `D:\TRbackup\NLTX\src\WorldStorage\TileEntityRecord.cs:3-9` | ID/type/anchor/update flag | 无 Leashed-specific record 或 authoritative relationship | `partial` |
| `D:\TRbackup\NLTX\src\WorldStorage\WorldSectionState.cs:3-22`、`SectionCoordinate.cs:3` | section flags/count/revision、坐标值对象 | 未证明 Leashed active section owner 或事件桥 | `partial` |
| `D:\TRbackup\NLTX\src\Share\Entity\Components\EntityIdentityComponent.cs:5-17` | Guid UUID identity | 与 legacy slot、persistent ID、network ID 未建立映射 | `partial` |
| `D:\TRbackup\NLTX\src\Share\Entity\Components\SpatialReferenceComponent.cs:5-20`、`src\Relationships\EntityReference.cs:5-10` | parent reference、offset、space、Guid relation | 未证明 anchor 一对一生命周期或 Leashed owner | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashAnchorStateComponent.cs:6-15` | tile、optional `Entity`、style、active、section 派生 | 无 authoritative anchor adapter、relation cleanup 和 persistence | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashBehaviorRefComponent.cs:3-8` | behavior、anchor style、NPC/projectile type、aquatic | 无 registry、factory、行为执行链 | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityLifecycle.cs:3-9` | Inactive/Active/Despawning/Removed 枚举 | 无 transition owner/commands/verifier | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityStateComponent.cs:5-15` | instance/type/anchor/tile/active/lifecycle/spawned | 无完整 motion、registry、section owner、server system | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashSectionIndexComponent.cs:5-11` | section/slot/active/last activation tick | 无 section event consumer、index mutation和 cleanup | `partial` |
| `dome/src/Terraria.Dome.Protocol.V1456/Packets/LeashedEntityModulePacket.cs:3-10` | immutable packet envelope | 未连接 authority apply；仍是 protocol representation | `partial` |
| `dome/src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs:44,569-645,4172-4297` | module 13 decode、full/partial/remove、kite/critter fields、长度校验 | 无 authoritative registry/apply/client projection owner | `partial` |
| `dome/src/Terraria.Dome.Protocol.V1456/Session/TerrariaSession.cs:393-397` | 调用 Leashed module decoder | 无模拟状态提交或投影消费闭环 | `partial` |
| `dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileAttachmentKind.cs` | 包含 `Leashed` attachment kind | 不能据此把 Leashed 生命周期、registry、anchor、section owner 归入 projectile | `partial` |

当前 `Test` 目录用 `rg -l -i "leash|leashed|TELeashedEntity|LeashedEntity" Test` 未找到 focused verifier；结果为 `NO_TEST_MATCH`。`src` 的同关键词检索只直接找到 `LeashedEntityAnchorComponent.cs`。因此不能把现有组件字段或 decoder 当作已实现的模拟闭环。

## 15. focused verifier 设计和建议命令

本节只设计后续验证，不代表已执行。统一 `verificationStatus: not-run`，不创建测试文件、不运行测试。

### 15.1 建议场景

| verifierId | 场景 | 必须断言 | 当前预期 |
| --- | --- | --- | --- |
| `verifier-registry-001` | 注册顺序、重复注册、unknown type | Type 1..19 稳定；缺失/重复被显式拒绝；prototype 不变为实例 | focused verifier missing；`not-run` |
| `verifier-anchor-spawn-002` | kite/critter anchor 插入、invalid item、world load restore | server 验证后产生一次 spawn；关系指向正确 anchor；invalid item policy 可审计；client input 不直接写 state | missing；`not-run` |
| `verifier-section-activation-003` | section active→inactive→active、重复事件、边界 section | 60 tick 规则/最终 owner明确；active 时 Spawn 一次；inactive 时 Despawn 不删除 durable relation；重激活可恢复 | missing；`not-run` |
| `verifier-update-004` | active update、inactive cleanup、behavior exception | 只更新 active entity；Update 后 dirty/snapshot 读同一版本；inactive 最终 Remove；失败隔离策略明确 | missing；`not-run` |
| `verifier-network-full-005` | full sync 新客户端/空 slot/类型和 anchor mismatch | module 13、message 1、type/anchor 检查、幂等 apply、projection 创建 | decoder 有静态代码但无 focused apply verifier；`not-run` |
| `verifier-network-partial-006` | partial sync、1024 tick cadence、乱序/重复/断线重连 | partial 不覆盖 identity/anchor；版本策略不回退；断线以 full snapshot 恢复；旧 slot 不误更新新实体 | missing；`not-run` |
| `verifier-network-remove-007` | Remove、unknown slot、已删除实体 | server cleanup 与 client tombstone 一次性；unknown/duplicate remove 幂等 | missing；`not-run` |
| `verifier-persistence-008` | 保存/加载、截断记录、invalid item、重复 anchor | durable relation 恢复；瞬时状态策略稳定；失败不半写；world load 后 spawn 顺序稳定 | missing；`not-run` |
| `verifier-disconnect-009` | client section 离开、重新进入、连接断开 | 离开只停止投影/streaming，不误删 server entity；重入 full sync 完整；socket failure 不污染 state | missing；`not-run` |
| `verifier-boundary-010` | projectile attachment、spatial section、anchor TileEntity 交叉调用 | Leashed owner 不被 projectile 巨型组件反向接管；Query 无写入；Projection 无权威写入；command 顺序可观察 | missing；`not-run` |
| `verifier-malformed-011` | 截断 7-bit int、trailing bytes、invalid subtype/length | Adapter 拒绝并返回可审计错误；不创建半初始化实体 | decoder 有长度检查证据，未独立运行；`not-run` |

### 15.2 建议命令形状

实现 focused verifier 和受影响项目后，才可按仓库契约从仓库根目录使用以下形状；本轮没有执行：

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\<affected-leashed-verifier-project>.csproj `
  --no-build --no-restore `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
```

`<affected-leashed-verifier-project>.csproj` 是待实现阶段解析的候选路径，不是当前文件。执行时还需要记录项目、退出码、warning/error 数量和 `Build/bin/` 工件路径；当前没有这些结果。

## 16. 本次验证状态

| 检查 | 本轮状态 | 证据 |
| --- | --- | --- |
| Version4 静态源码重新读取 | 已执行，只读 | 上述 Version4 evidence table 的绝对路径和实际行号 |
| 完整参考源码补证 | 已执行，只读 | `LeashedEntity.cs`、anchor、derived anchor、behavior 文件表 |
| tModLoader 文档检索 | 已执行，只读 | `tModLoader v2026.07` 页面和成员锚点表 |
| SS14 最小结构参考 | 已执行，只读 | Component/System/Spawn/Relation 文件表 |
| NLTX 当前结构检索 | 已执行，只读 | `src`、`dome/src`、`Test` 的静态内容 |
| focused verifier | 未执行 | `verificationStatus: not-run`；Test 无 Leash focused match |
| `dotnet restore/build/test/run/publish/pack/watch/msbuild` | 未执行 | 本轮明确不运行 compile-capable 命令 |
| coverage verifier | 未执行 | `Build/Tools/Test-Version4SubsystemCoverage.ps1` 仅作为规则参考 |

## 17. 不拆分项

以下对象保留在其上层边界，不单独升格为一级子系统：

| 对象 | 不拆分理由 |
| --- | --- |
| 单个 leashed critter prototype | 它是 `Registry` 中的 definition/behavior strategy；生命周期由 registry、section、entity state 和 update 共同形成 |
| 单个 `LeashedKite` prototype | 它是 kite-specific strategy/definition，不拥有全局 slot、section activation 或网络 session |
| 单个 `TELeashedEntityAnchor` | 它是 TileEntity host/relation trigger；其 `OnRemoved`、`OnWorldLoaded` 和 item placement 触发 Leashed command，但不拥有 `ByWhoAmI`/`BySection` |
| `TEKiteAnchor`/`TECritterAnchor` | 它们是 content compatibility、tile validity 和 factory mapping；不是独立 Leashed lifecycle |
| 单个 `NetModule` | 它是 protocol Adapter；模块 ID、writer、socket 是传输机制，不是模拟状态 owner |
| 单个 `SectionEntityList` | 它是按 section 的 spatial index/work-set/cache；多个 section、active list、实体 update 和 lifecycle 才构成责任面 |
| 单个网络包 | 它是 snapshot/projection transport 的一次实例；full/partial/remove 协议应由 Adapter 统一管理 |
| 单个渲染对象/NPC dummy/projectile dummy | 它是 client presentation projection；不拥有 server authority 或 persistent relation |
| `WalkerLeashedCritter`、`FlyerLeashedCritter`、`JumperLeashedCritter` | 它们是 movement behavior strategies，共享 Leashed lifecycle/registry；拆成一级子系统会复制 owner 和调度边界 |
| `LeashedEntity` 基类中的每个虚方法 | 它们是扩展点/策略回调，不能仅凭方法名创建独立 owner |

## 18. 兼容策略和行为保持风险

### 18.1 需要保留或显式版本化的兼容事实

1. 注册顺序：`null` 占位、kite 为 Type 1、18 个 critter 依序为 Type 2..19；注册顺序不能由目录顺序或哈希顺序决定。
2. section 几何：Version4 `ActiveSections` 使用 200×150 tile 区段；边界、clamp 和 60 tick inactive window 需由最终 section owner 提供稳定语义。
3. slot 兼容：旧 `whoAmI` 参与 full/partial/remove 协议；它不应自动等同于持久化 ID 或 ECS entity handle。
4. full/partial/remove：module 13 与消息类型 0/1/2、7-bit encoded int、full anchor 坐标和 subtype payload 的 wire shape 需要由 golden packet verifier 锁定。
5. stream cadence：完整参考的 `(GameUpdateCount + whoAmI) & 0x3FF` 是 1024 tick 错位发送策略；Version4 为空体，实施时必须先决定是兼容还是 revision/dirty 替代。
6. section list mutation：数组扩容、`sectionSlot`、empty-slot compact 和 active work-set 必须保持身份稳定，不得将 slot 误当作 durable identity。
7. server/client 方向：Update、Spawn、Despawn、item drop 和 NetSend 为 server/SP 方向；NetReceive 和 draw 为 client 方向；任何例外都必须有验证证据。

### 18.2 风险

| riskId | 风险 | 原因 | 需要的控制 |
| --- | --- | --- | --- |
| `risk-v4-stub-001` | 把完整参考补证误写成 Version4 当前行为 | V4 多个成员为空体/缺失 | 报告、实现和 verifier 分别标记 source fact、supplement、proposed |
| `risk-identity-002` | slot 重用导致旧 partial 更新新实例 | `whoAmI` 是可重用列表 slot | stable ID + slot generation/revision，最终 owner `integration-review` |
| `risk-anchor-003` | anchor 删除与 entity remove 顺序产生悬挂关系或重复 Remove | TileEntity.OnRemoved 与 Leashed.Update 分离 | 一次性 command/transition revision、幂等 verifier |
| `risk-section-004` | client section cache 被当成 server authority | ActiveSections 与 RemoteClient 各有时间状态 | 明确 section activity Port 和 server/client ownership |
| `risk-network-005` | mutable instance 在写包期间继续变化 | V4 `Sync` 直接调用 `entity.NetSend` | immutable snapshot、版本和发送队列 |
| `risk-behavior-006` | 运动/随机/dummy 状态丢失导致行为漂移 | V4 缺 position 等字段，完整参考才有实现 | behavior fixture、golden snapshot、deterministic clock/random ports |
| `risk-projectile-007` | kite 被错误并入普通 projectile owner | 当前 dome 仅出现 `AttachmentKind.Leashed` | 以 lifecycle/registry/anchor/section 写集划界，最终 owner整合审查 |
| `risk-persistence-008` | world load 后 item/anchor 未恢复但 entity 被创建 | V4 anchor persistence/on-load 方法为空 | recovery policy、invalid-content verifier、原子提交 |
| `risk-module-009` | module ID 随 initializer 顺序漂移 | `NetManager.Register` 按注册序递增 | 锁定 protocol manifest 或显式 compatibility mapping |

## 19. 未决问题、evidence-gap 和 blocking-decision

### 19.1 普通 evidence-gap

| gapId | 缺口 | 影响 | 当前处理 |
| --- | --- | --- | --- |
| `gap-v4-deserialize-001` | Version4 `NetModule.Deserialize` 为空 | 当前 V4 客户端 apply 行为无法由主基线闭合 | 以完整参考补证协议形状，状态 `partial` |
| `gap-v4-static-subscription-002` | Version4 缺少完整参考的静态构造函数 | section event 到 Leashed Activate/Sync 的运行时订阅无法直接确认 | 只写完整参考关系，标记 `partial` |
| `gap-v4-section-mutation-003` | V4 `SectionEntityList.Add/Remove` 缺失 | index mutation 和 slot 回收无法确认 | 完整参考同签名补证，状态 `partial` |
| `gap-v4-remove-stream-004` | V4 `Remove`、`StreamNetUpdates` 为空 | remove packet 和 partial cadence 无当前 V4 执行证据 | 不声称已发送，状态 `partial` |
| `gap-v4-motion-005` | V4 `LeashedEntity` 文件缺 position/velocity/size 等完整参考字段 | motion state owner 和网络 payload 不完整 | 将其列为 proposed behavior state，等待成员级核对 |
| `gap-v4-draw-caller-006` | 当前 V4 直接 `DrawEntities` 调用者未定位 | render/update 相对顺序未知 | 只确认完整参考 draw 方法，调用链 `unresolved` |
| `gap-v4-anchor-persistence-007` | V4 anchor extra data/on-load 方法为空 | item/关系恢复行为未闭合 | 不把完整参考的持久化实现写成 V4 事实 |
| `gap-nltx-verifier-008` | Test 无 Leash focused verifier | 无独立激活、销毁、断线、协议 apply 证据 | `focused verifier: missing; verificationStatus: not-run` |
| `gap-tml-public-api-009` | 文档索引未找到 `ModLeashedEntity` | 没有公开 Leashed API 可直接复用 | 明确标记 `missing`，不做反向推断 |

### 19.2 必须留给整合会话的 blocking-decision

这些问题会改变 owner、提交边界或系统顺序，本报告不自行裁决。

| decisionId | 未决问题 | 选项 A | 选项 B | 选项 C | 影响 |
| --- | --- | --- | --- | --- | --- |
| `BD-LeashedIdentityOwner` | persistent/runtime/network/legacy slot 的最终 owner 是谁 | `LeashedEntity` 持有稳定 runtime ID，`whoAmI` 仅 Adapter projection | `WorldStorage` 持有 persistent ID，Leashed 只引用 | 继续以 legacy slot 为权威并增加 generation | 决定所有组件字段、存档 key、full/partial apply 和 slot reuse 规则 |
| `BD-LeashedAnchorRelation` | TileEntity 与实体关系的形态 | 一对一 runtime link + durable tile coordinate | 只保存 persistent anchor coordinate，load 时重建 runtime link | 独立 relation record 允许暂时 unresolved | 决定删除/重建/断线/世界加载时的提交顺序 |
| `BD-LeashedSectionOwner` | active section 真相的 owner | `NetworkSessionAndSectionStreaming` 提供 activity facts | `WorldStorage` 提供 world section state | Leashed 自己持有 authoritative cache | 决定 section event、60 tick window、缓存失效和跨域依赖 |
| `BD-LeashedPartialSync` | partial sync 的 dirty/version contract | 保留 1024 tick + slot skew | 以 behavior dirty set/monotonic revision 发送 | 可见 section 变化即 full snapshot | 决定 snapshot 字段、带宽、乱序和重连策略 |
| `BD-LeashedSchedulerOrder` | 与 Projectile/Spatial/Network 的调度先后 | Spatial/index 更新后 Leashed behavior，再 replication | Leashed 先计算，再由 Spatial 消费位置 | 由统一 RuntimeComposition 明确 phase barrier | 决定 position/section relevance 和 snapshot 时点 |
| `BD-LeashedLegacySlot` | `whoAmI` 是否仍是权威状态 | 仅旧 wire projection | 权威 state 保留 slot 直到 remove | slot 与 stable ID 双向 durable 映射 | 决定协议兼容、slot reuse 和客户端错误防护 |
| `BD-LeashedPersistencePayload` | 哪些实体状态持久化 | 仅 anchor/definition/item/relation | 加 motion/behavior state | 只保存 checkpoint/recovery cursor | 决定 world load 行为和行为漂移风险 |

## 20. Integration Handoff

```text
subsystemId: LeashedEntitySimulation
taskNumber: 03
reportPath: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-leashed-entity-simulation-public-decomposition.md

evidenceStatus: partial
nltxStatus: missing
verificationStatus: not-run

confirmedOwners:
- Version4 LeashedEntity.Registry 的固定类型注册顺序和 prototype definition 入口
- Version4 LeashedEntity 的 ByWhoAmI、BySection、ActiveSectionList 及 section list 生命周期骨架
- Version4 LeashedEntity 的 UpdateEntities -> RecheckActiveSections -> _UpdateEntities 局部更新方向
- Version4 NetModule.Sync 的 full/partial writer 入口、按 section relevance 广播和定向发送边界
- Version4 Main 的 registry 初始化/每帧驱动以及 WorldGen 的 clear 触发边界
- Version4 的 LeashedEntity responsibility 面；锚点 TileEntity 仅为 host/relation trigger，不拥有 registry

proposedTypes:
- proposed Definition: LeashedEntityDefinition
- proposed Registry: LeashedEntityRegistry
- proposed Components: LeashedEntityStateComponent, LeashedEntityMotionComponent, LeashedEntitySectionComponent, LeashedEntityAnchorRelationComponent, LeashedEntityLifecycleComponent
- proposed Systems: LeashedEntitySectionActivationSystem, LeashedEntityUpdateSystem
- proposed Queries: LeashedEntityRegistryQuery, LeashedEntitySectionQuery
- proposed Commands: LeashedEntitySpawnCommand, LeashedEntityDespawnCommand
- proposed Snapshot: LeashedEntityReplicationSnapshot
- proposed Adapters: LeashedEntityReplicationAdapter, LeashedEntityPersistenceAdapter
- proposed Projection: LeashedEntityClientProjection

sharedTypesForIntegrationReview:
- EntityId/runtime entity handle/persistent entity ID
- legacy whoAmI slot and slot generation
- NetworkId/module 13/message envelope
- TileCoordinate/WorldSectionId/SectionCoordinate
- EntityReference and anchor relation value
- replication snapshot/version/dirty contract
- WorldTime/clock and deterministic random cursor

crossSubsystemReaders:
- NetworkSessionAndSectionStreaming reads snapshots and section relevance
- WorldStorage/PersistenceAndRecovery reads durable anchor relation and recovery records
- WorldInteraction/TileEntity host reads lifecycle result and supplies anchor/item facts
- SpatialSimulation may read position/section projection for spatial relevance
- Client presentation reads LeashedEntityClientProjection

crossSubsystemWriters:
- WorldInteraction/TileEntity host may submit validated spawn/despawn triggers
- NetworkSessionAndSectionStreaming may submit decoded, validated client input or section activity facts
- WorldStorage/PersistenceAndRecovery may submit world-load/recovery commands
- RuntimeComposition may determine phase and dispatch order
- ProjectileSimulation may expose reusable movement/attachment mechanisms, but must not own Leashed lifecycle

orderingConstraints:
- registry initialization must complete before any definition lookup or anchor spawn
- anchor validation must precede SpawnCommand commit
- section activity must be consumed before section-local Spawn/Despawn decisions
- Spawn must precede the first active update; Despawn must prevent later update before index removal
- Update must precede immutable snapshot build; snapshot build must precede network send
- server authoritative commit must precede client projection
- WorldGen clear keeps ActiveSections.Reset before LeashedEntity.Clear as observed in Version4
- final relative order with ProjectileSimulation, SpatialSimulation and NetworkSessionAndSectionStreaming is integration-review

boundaryChallenges:
- keep LeashedEntity independent from ProjectileSimulation while allowing kite/projectile-like motion mechanisms to be reused
- keep anchor TileEntity as host/trigger while Leashed owns entity registry and lifecycle
- treat BySection/ActiveSectionList as indexes/work sets, not alternate authorities
- replace mutable NetSend input with immutable snapshot at the Adapter seam
- decide whether section activity is external fact or Leashed-owned cache

evidenceGaps:
- Version4 Deserialize, Remove, StreamNetUpdates, Add/Remove, static event subscriptions and anchor restore bodies are empty or missing
- complete-reference behavior fields and methods are not all present in Version4
- Version4 direct DrawEntities caller and full rendering order are unresolved
- current NLTX has no closed registry/lifecycle/system/anchor relation/apply chain
- no Leash focused verifier exists; all current verification is not-run

blockingDecisions:
- final owner of persistent/runtime/network/legacy identity
- anchor relation representation and deletion/recovery semantics
- authoritative section activity owner
- partial sync dirty/version/cadence contract
- scheduler order against Projectile/Spatial/Network systems
- persistence payload and legacy whoAmI authority policy

notImplemented:
- no LeashedEntity registry implementation in current NLTX
- no authoritative Leashed spawn/despawn/update/section activation systems in current NLTX
- no validated anchor relation command executor in current NLTX
- no Leashed replication apply/projection owner connected to the existing decoder
- no Leashed persistence adapter or focused verifier

verifierPlan:
- implement registry order/type safety verifier
- verify anchor insertion, invalid content, world load and removal idempotency
- verify section activation/deactivation and spawn/despawn ordering
- verify full/partial/remove module 13 protocol, malformed payloads, stale/repeated packets and reconnect
- verify stable identity versus whoAmI slot reuse
- verify immutable snapshot, server/client authority, persistence recovery and cross-subsystem boundaries
- run only after implementation, using the repository serial command contract; this report records no execution
```

## 21. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。

本轮只新增本报告文件，未修改 `src/`、`Test/`、`dome/src/`、Version4、完整参考源码、既有索引或其他报告；未启动子代理，未运行构建或测试。
