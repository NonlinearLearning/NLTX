# Creative：创造能力、权限与研究上报

[返回 Modules](../README.md) · [服务器总览](../../README.md)

## 作用与三个子协议

本目录有独立项目 [MinimalImplementation.Modules.Creative.csproj](MinimalImplementation.Modules.Creative.csproj)。
它处理模块 5 的创造能力值、模块 6 的玩家研究上报，以及模块 9 的服务端权限快照。
能力状态、输出表现和持久化子集各有自己的数据类型。

## 状态与组件怎样分工

| 部件 | 作用 |
| --- | --- |
| [CreativePowerStateOwner](CreativePowerStateOwner.cs) | 维护共享/逐玩家开关、滑块、权限、提交版本与接收者交付进度 |
| [CreativePowerWireMessage](CreativePowerWireMessage.cs)、[CreativePowerWireCodec](CreativePowerWireCodec.cs) | 表达并编解码能力编号和对应的值类型 |
| [CreativePowerSnapshot](CreativePowerSnapshot.cs) | 完整运行时能力和权限的读取快照 |
| [CreativeModuleHandler](CreativeModuleHandler.cs) | 组织能力应用、研究上报和权限快照入口 |
| [CreativeUnlockReportOwner](CreativeUnlockReportOwner.cs) | 为认证后的研究上报分配提交版本，产生转发报告 |
| [CreativePowerEffect](CreativePowerEffect.cs) | 一次能力提交对应的广播消息 |
| [CreativePermissionSnapshotBatch](CreativePermissionSnapshotBatch.cs) | 面向一个会话的权限快照批次 |
| [CreativePowerEffectDeliverySettlement](CreativePowerEffectDeliverySettlement.cs) | 根据每个输出的交付结果结算能力广播 |

研究报告 Owner 不保存玩家研究进度，也不扣背包物品。它维护报告顺序及认证后的玩家字段，
具体报告事实检查由调用重载与装配决定。不能把“报告已提交”理解为服务端完成了完整研究
库存事务。

## 一次能力修改如何执行

1. Host 对模块 5 认证并解码，使用当前绑定的玩家编号与游戏主机身份。
2. CreativePowerStateOwner.Apply 验证能力 ID 0–14、值种类和权限。
3. 滑块必须是有限值且在 0–1 内；整表逐玩家开关需要恰好 255 个状态。
4. Owner 更新对应状态和版本，返回 CreativePowerEffect。
5. Host 将 effect 转为有序出站意图，并建立交付批次。

权限判断使用 `CountsAsHostForGameplay`，需要与 HostToken 授权区分。客户端发送模块 9
不能修改权限；该模块是服务端发给客户端的快照。

## 为什么效果值和广播消息分开

同一个能力操作可能需要多个广播。`CaptureBroadcastMessages` 对逐玩家整表同步转换为
逐槽消息，适配原版客户端对自身槽的处理；按钮或逐玩家滑块可能不产生普通广播。
因此，网络输出数量不能直接当成状态更新数量。

交付进度按 SessionEpoch、接收者生命周期和版本维护。一次发送部分成功时，已经完成的
部分与仍待交付的部分分别记录；后续捕获保留未完成消息。旧会话回执不能确认新会话的
权限或效果。`RetireEpoch` 用于关闭时清理这些接收者状态。

## 权限同步怎样接到 Host

入服和周期同步通过 Handler 捕获权限批次，发送后由回执结算；Host 的适配器位于
[CompleteModuleHandlers](../../Host/CompleteModuleHandlers.cs)，周期入口在
[ModulePeriodicSyncOwner](../../Host/ModulePeriodicSyncOwner.cs)。
`LocallySent` 仍然只表示本地发送完成，不能当成客户端已经应用权限。

## 哪些内容写进世界文件

[CreativeWorldPowerState](CreativeWorldPowerState.cs) 与持久化层的
[CreativeWorldPowerSnapshot](../../Persistence/CreativeWorldPowerSnapshot.cs) 表达六项世界状态：
冻结时间、时间速率、冻结雨、冻结风、难度滑块、停止生物群落传播。
[CreativeWorldPowerDocumentCodec](../../Persistence/CreativeWorldPowerDocumentCodec.cs)
负责编码这个子集，Host 在加载和保存边界调用。

这一保存子集不等于整个运行时快照，也不包含全部在线逐玩家状态、研究进度和交付缓存。
能力值已提交或已保存，还需要继续追踪 World 等消费者，才能判断其游戏效果是否持续执行。

建议阅读：WireMessage → StateOwner.Apply → CreativeModuleHandler → Effect/交付结算 →
Host 注册 → 世界能力保存与实际效果消费者。
