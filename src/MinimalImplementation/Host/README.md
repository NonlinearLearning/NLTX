# Host：装配、生命周期与跨系统入口

[返回总览](../README.md)。本目录连接领域系统、网络和文件能力，是理解最小服务器的第一站。

## 职责与关键部件

| 部件 | 作用 | 直接协作者 |
| --- | --- | --- |
| [P08BServerRuntime](P08BServerRuntime.cs) | 启动、世界发布、默认集成、保存和退出 | ServerComposition、加载/保存协调器 |
| [ServerComposition](ServerComposition.cs) | 创建各 Owner、Store、适配器及模块注册 | 各领域入口 |
| [ServerLoop](ServerLoop.cs) | 用有界队列串行执行同步提交动作 | 入站路由、Tick、保存捕获、发送结算 |
| [ServerConnectionHost](ServerConnectionHost.cs) | 管理连接、发送目标及输出顺序 | Protocol、OutboundTurn |
| [Version4InboundRouter](Version4InboundRouter.cs) | 从已解码消息分派到领域入口 | Session 认证、各 partial 路由 |
| [DefaultServerTickPhaseOwners](DefaultServerTickPhaseOwners.cs) | 为默认运行实例组合 Tick owners | Player、Combat、World、Inventory、Modules |
| [ServerOutboundDeliveryPump](ServerOutboundDeliveryPump.cs) | 在提交窗口外发送并将回执送回 | DeliverySettlement、ServerLoop |
| [ServerLog](../../NSSLC/Components/Diagnostics/README.md) | 输出时间、级别、事件名与单行诊断内容 | 运行、路由及诊断边界 |

## 启动时怎样协作

从 [Program](../Program.cs) 进入，运行时读取配置，创建装配对象并启动提交循环。
世界文档先经过加载、校验和候选转换，再进入提交窗口发布，随后恢复箱子、牌子、图格实体
及适用的世界模块。运行时再连接路由器、网络宿主、输出泵和 Tick 调度器。

默认 Tick 注册通过 ServerTickPhaseOwnerCatalog 检查缺失或重复阶段；路由器也检查必要回调和
编解码绑定。启动结果同时报告世界、监听、Tick、保存等状态，定位时不要只看一个 ready 字段。

## 入站路由文件分工

| 文件 | 阅读内容 |
| --- | --- |
| [主路由](Version4InboundRouter.cs) | 连接回调、Hello、玩家包、模块分派及未接通消息的拒绝 |
| [PlayerOutput](Version4InboundRouter.PlayerOutput.cs) | 玩家上传、出生坐标、背包输入及输出捕获 |
| [Sections](Version4InboundRouter.Sections.cs) | 首次入服、移动区块、区块请求 |
| [JoinSnapshots](Version4InboundRouter.JoinSnapshots.cs) | NPC、世界物品、创造能力等初始快照 |
| [Terrain](Version4InboundRouter.Terrain.cs) | 基础破坏、放置、普通树木采集和物品事务 |
| [WorldItems](Version4InboundRouter.WorldItems.cs) | 丢弃请求、所有权释放、世界物品更新与退休 |
| [Npcs](Version4InboundRouter.Npcs.cs) | NPC 受击、预测确认、代次校验和生命广播 |
| [Loadout](Version4InboundRouter.Loadout.cs) | 装备配置切换与背包状态衔接 |
| [TeamSpawn](Version4InboundRouter.TeamSpawn.cs) | 换队及额外出生区块同步 |
| [Close](Version4InboundRouter.Close.cs) | 弹幕、追踪关系和断线输出清理 |

这些 partial 文件属于同一个 Version4InboundRouter 类型，并不各自创建一份服务器状态。

## 提交与 I/O 的边界

网络回调在接收线程获得 envelope 和解码对象，再通过 SubmitAsync 进入 ServerLoop。
领域读写在同步提交动作中完成，输出先捕获为 intent/settlement，异步发送在提交窗口外执行。
发送完成后的结算再次回到 ServerLoop，以免从后台任务直接修改 Owner。

ServerLoop 是一个串行逻辑执行窗口，不要求始终运行在相同操作系统线程上。它用 IsCurrent
标识当前窗口，并禁止在同一窗口里提交另一个动作后等待自己，避免死锁。

对于同一操作的多份状态，串行执行本身不等于事务原子性。例如破坏地形并生成掉落，通过
[TileItemTransactionParticipant](TileItemTransactionParticipant.cs) 参与物品事务；
放置和丢弃则由各自的 InventoryItem…Preparation 携带背包校验与扣减。

地形和 NPC 受击入口使用 `HasQueuedSection` 检查目标区块的完整基线是否已经排入有序
发送流。区块账本记录 Queued/LocallySent 等本地传输状态，不能将其当成客户端应用确认。

## 每帧与发送阶段

Tick 依次覆盖 Player、NPC、Projectile、WorldItem、LeashedEntity、Time/World、Invasion、
PeriodicSync。Time 与 World 由 [WorldTickPhaseCompositeOwner](WorldTickPhaseCompositeOwner.cs)
共用一次世界提交。默认注册关闭复合 owner 内的入侵桥调用，由后面的 Invasion 阶段处理。

[ModulePeriodicSyncOwner](ModulePeriodicSyncOwner.cs) 当前会捕获 NPC 脏快照、世界物品投影、
图鉴、旗帜和创造权限。NPC 周期同步按每 6 游戏帧尝试一次，且避免同时挂起重复批次。
不要把这个类名理解为所有领域输出都已经在这里实现。

## 与世界和文件的衔接

- [GeneratedWorldDocumentFactory](GeneratedWorldDocumentFactory.cs)：将完整生成结果转换为当前存档文档。
- [P08BWorldSnapshotProjector](P08BWorldSnapshotProjector.cs)：将世界快照投影回保存文档。
- [WorldAttachmentHydrator](WorldAttachmentHydrator.cs)：恢复箱子、牌子、图格实体。
- [WorldAttachmentPersistenceProjector](WorldAttachmentPersistenceProjector.cs)：捕获附件的保存状态。
- [WorldDataPacketSupplementProvider](WorldDataPacketSupplementProvider.cs)：提供世界信息包的存档补充字段。
- [WorldSectionAttachmentProvider](WorldSectionAttachmentProvider.cs)：捕获区块内附件用于同步。

## 阅读时需要留意的接入状态

默认会话策略目前使用 Terraria319 标识并开启 ignoreProtocolVersion。跳过版本字符串比较，
不会自动修复任何包字段或存档编码差异。普通消息仍可能在主 switch 中被拒绝。

诊断 NPC 由 [NpcDiagnosticSpawn](NpcDiagnosticSpawn.cs) 和启动参数显式启用；当前是原生
25 血蓝史莱姆及 250 血向导。完整服务器能力不能用这对诊断实体的成功运行代替判断。

定位问题时先找请求路径、当前阶段和 LastRejection，再追到实际 Owner。网络可解码、领域
可处理、默认实例已接通，是三个需要分别检查的位置。
