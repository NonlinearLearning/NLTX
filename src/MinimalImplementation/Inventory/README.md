# Inventory：背包、箱子与世界物品

[返回总览](../README.md) · [玩家](../Player/README.md) · [地形](../World/README.md)

## 作用与状态归属

本目录维护玩家物品槽、箱子内容和地面物品，提供转移、丢弃、拾取、合成和快速堆叠的
规则与事务。PlayerOwner 维护玩家生命和位置；WorldOwner 维护地形。跨系统操作通过
准备对象与参与者协作，不直接把另一系统的可变状态放进物品存储。

## 状态和组件

| 部件 | 维护内容 | 关键区别 |
| --- | --- | --- |
| [InventoryStore](State/InventoryStore.cs) | 玩家背包槽和修订号 | 每个槽的物品与玩家本体状态分开 |
| [ChestStore](State/ChestStore.cs) | 箱子槽和代次 | 箱子编号、实例代次、槽版本分别检查 |
| [ChestLeaseStore](State/ChestLeaseStore.cs)、[ChestAccessService](State/ChestAccessService.cs) | 会话箱子访问租约 | 管理箱子访问权 |
| [WorldItemStore](State/WorldItemStore.cs) | 世界物品实例及修订号 | 槽位重用后旧 Handle 不再有效 |
| [WorldItemReservationStore](State/WorldItemReservationStore.cs) | 世界物品的归属 | 管理客户端模拟归属，与箱子租约不同 |
| [InstancedItemStore](State/InstancedItemStore.cs) | 面向特定玩家的物品状态 | 与普通公共世界物品分开管理 |
| [InventoryItemState](State/InventoryItemState.cs) | 类型、数量、前缀、收藏标记 | 是物品值，不包含世界位置 |
| [WorldItemSnapshot](State/WorldItemSnapshot.cs)、[WorldItemRuntime](State/WorldItemRuntime.cs) | 物品、位置、速度、寿命、拾取延迟与归属计时 | 网络表现和模拟都以这些状态为依据 |

## 规则、事务和输出怎样协作

| 层次 | 主要入口 | 职责 |
| --- | --- | --- |
| 内容事实 | [ItemContentFacts](Content/ItemContentFacts.cs)、[ItemPickupCatalog](Content/ItemPickupCatalog.cs) | 提供数量、拾取和具体类型规则需要的事实 |
| 计划 | [WorldItemPickupPlanner](Rules/WorldItemPickupPlanner.cs) 等 Rules | 根据捕获的状态计算变化，尚未提交 |
| 背包与箱子事务 | [InventoryTransactionCoordinator](State/InventoryTransactionCoordinator.cs) | 校验两侧来源并提交槽变化 |
| 世界物品事务 | [WorldItemTransactionCoordinator](State/WorldItemTransactionCoordinator.cs) | 组合物品、库存和外部参与者，统一校验后提交 |
| 丢弃与生命周期 | [WorldItemDropCoordinator](State/WorldItemDropCoordinator.cs)、[WorldItemLifecycleCoordinator](State/WorldItemLifecycleCoordinator.cs) | 验证丢弃收支、清理失效会话和世界状态 |
| 输出 | [WorldItemProjectionJournal](State/WorldItemProjectionJournal.cs)、[WorldItemProjectionProducer](Protocol/WorldItemProjectionProducer.cs) | 记录变化并转换为面向接收者的包意图 |

相关 Store 共享 [ServerStateCommitGate](State/ServerStateCommitGate.cs)。事务要同时检查
来源修订号、物品代次、归属与参与者；拿到计划不意味着已修改状态。发送又在提交之后，
通过结算保留尚未交付的变化。

## 默认原版客户端：丢弃流程

当前网络衔接在 [Version4InboundRouter.WorldItems.cs](../Host/Version4InboundRouter.WorldItems.cs)。

1. 客户端以 Packet 21、`ItemIndex = 400` 请求创建掉落物。
2. Host 记录待处理丢弃，等待相应 Packet 5 的背包扣除变化。
3. [InventoryItemDropPreparation](../Host/InventoryItemDropPreparation.cs) 与丢弃协调器验证
   物品类型、数量和库存变化，组合为一次扣除与创建。
4. 提交成功后产生物品投影；避免再次向物主回显同一库存扣除。

因此，诊断丢弃不能只观察 Packet 21；还需要看对应的库存变化是否到达、是否匹配和提交。

## 默认原版客户端：归属和拾取流程

[VanillaWorldItemOwnership](../Host/VanillaWorldItemOwnership.cs) 为可拾取物品选择当前有效
客户端，并通过 Packet 22 同步归属。客户端执行拾取后发送 Packet 151 退休世界物品，
再通过 Packet 5 上传背包变化。**这一路径不在 Packet 151 中再次给服务端背包加物品**，
否则会和客户端上传重复入账。

目录内还有 [WorldItemPickupCoordinator](State/WorldItemPickupCoordinator.cs) 与
[WorldItemOwnershipCoordinator](State/WorldItemOwnershipCoordinator.cs) 等领域入口。
它们表达的服务端事务能力，与默认原版客户端的实际协议调用链应分别阅读。

## 地形掉落和放置如何接入

Packet 17 经过认证与区块基线排队记录校验，World 规则先准备地形结果；掉落需要的
[WorldItemSpawn](State/WorldItemSpawn.cs) 与
[TileItemTransactionParticipant](../Host/TileItemTransactionParticipant.cs) 一起提交。
放置通过 [InventoryItemPlacementPreparation](../Host/InventoryItemPlacementPreparation.cs)
协调库存消耗。共同目标是地形变化和物品收支同时成立。

## 物品每帧更新与拾取计时

[WorldItemTickCoordinator](State/WorldItemTickCoordinator.cs) 使用规则和世界碰撞事实准备
物品更新。默认 [WorldItem 阶段](../Host/DefaultServerTickPhaseOwners.cs) 在完整运动批次
因空间事实不可用等原因无法执行时，使用 `PrepareOwnershipTimerTick` 继续推进归属及
拾取延迟计时。完整运动成功后不再补跑同一计时，以免一帧减两次。

这解释了为什么“有地面物品但一直捡不起”还应检查 NoGrabDelay、OwnTime、归属和运动
批次失败，而不仅是距离或背包容量。

## 其他能力与当前接入范围

- [QuickStack](QuickStack/QuickStackTransactionCoordinator.cs)：组合附近目的地、租约和转移计划；
  [Packet 85 入口](QuickStack/QuickStackPacket85Handler.cs) 已有 Host 路由。
- [Crafting](Crafting/CraftingTransactionCoordinator.cs)：组合材料计划、背包和箱子变化；
  [模块 11](Crafting/CraftingModule11Handler.cs) 经模块分发器接入。
- [InventoryLoadoutCoordinator](State/InventoryLoadoutCoordinator.cs)、[InventoryPaymentCoordinator](State/InventoryPaymentCoordinator.cs)：
  负责配装和支付相关物品变化。

默认 [ServerComposition](../Host/ServerComposition.cs) 的 QuickStack 分类配置和合成材料目录
当前为空。存在入口和事务结构不等于已装载完整原版内容表。世界保存也不会自动把所有
在线背包和地面物品写入 WorldFile，需检查实际保存投影。

排查入口可搜索日志 `server.item.drop_pending`、`dropped`、`owner_assigned`、`retired`
和 `motion.deferred`，再回到对应路由、事务结果和投影结算。
