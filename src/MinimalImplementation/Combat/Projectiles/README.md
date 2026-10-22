# Projectile：弹幕状态、运动与命中

[返回 Combat](../README.md) · [NPC](../Npcs/README.md)

## 作用与当前接入状态

本目录提供弹幕的同步、创建、AI、移动、碰撞、NPC 命中、追踪和终止领域实现。
默认服务器已注册 Projectile Tick，用于推进已有实例的 AI 与到期退休；
**当前默认入站路由没有普通 Packet 27/29 处理分支**。阅读本目录时，应将领域能力与
当前实际运行的客户端调用链分别核对。

## 状态和身份

[Projectile](Projectile.cs) 保存类型、位置、速度、AI0–2、伤害、击退、原始伤害、剩余穿透、
寿命、额外更新、液体状态及 Revision。由 [ProjectileManager](ProjectileManager.cs) 提交修改，
[ProjectileView](ProjectileView.cs) 用于外部读取。

| 身份或字段 | 用途 |
| --- | --- |
| Entity | 共享战斗身份中的实例标识，包含代次和作用域意义 |
| RuntimeSlot | 服务端本地运行槽 |
| Owner、OwnerEpoch | 所有者槽及对应连接，避免重连后继承旧实例 |
| ProtocolIdentity、ProtocolUuid | 客户端协议同步和追踪标识 |
| Revision、CommitVersion | 当前实例状态与提交版本 |

同一个数字在不同标识空间中含义不同。尤其不能把 RuntimeSlot 直接作为任意玩家上报的
协议身份，也不能只看 Owner 槽位而忽略 OwnerEpoch。

## 协作部件

| 部件 | 作用 |
| --- | --- |
| [ProjectileDefinitionCatalog](ProjectileDefinitionCatalog.cs)、[ProjectileAiProfileCatalog](ProjectileAiProfileCatalog.cs) | 明确支持的内容与 AI 事实 |
| [ProjectilePacketHandler](ProjectilePacketHandler.cs)、[ProjectilePacketPayloadCodec](ProjectilePacketPayloadCodec.cs) | 领域网络适配与载荷编解码 |
| [ProjectileManager](ProjectileManager.cs) | 同步、终止、所有者清理和实例管理 |
| [ProjectileManager.LocalArrow](ProjectileManager.LocalArrow.cs) | 本地箭矢创建的准备和提交 |
| [ProjectileAiService](ProjectileAiService.cs) | AI 和生命周期推进 |
| [ProjectileMovementService](ProjectileMovementService.cs) | 根据捕获的世界运动事实准备位移 |
| [ProjectileCollisionService](ProjectileCollisionService.cs) | 解析碰撞结果并协调状态变化 |
| [ProjectileNpcCombatService](ProjectileNpcCombatService.cs) | 用弹幕和 NPC 捕获事实准备命中 |
| [ProjectileTrackerManager](ProjectileTrackerManager.cs) | 玩家与活动弹幕引用的追踪关系 |

## 准备、提交与副效果

同步请求先规范化所有者、内容与协议身份，再准备带来源修订号的同步计划。
运动和命中同样先读取 ProjectileView、世界快照或 NpcView，再产生计划。
Manager 在提交前校验当前身份和版本；过期计划不能修改新实例。

箭矢运动使用明确的所有者、风、液体及世界效果事实，见
[ProjectileArrowMotionRules](ProjectileArrowMotionRules.cs)。返回的世界效果意图需要调用方
进一步协调 World 操作；碰撞或命中结果也需要沿结果类型继续查找后续提交与输出。
领域规则不直接完成 socket 发送。

## 默认 Tick 实际做什么

[ProjectileServerTickPhaseOwner](../../Host/ProjectileServerTickPhaseOwner.cs) 遍历现存实例，
调用 ProjectileAiService.Tick；已到期或本帧到期的弹幕通过 Manager.RetireExpired 退休。
这个 owner 没有自动串联本目录全部运动、碰撞和 NPC 命中服务。

同样，模块 12 标签状态存在，不意味着每个弹幕命中都会调用 TagEffectStateOwner.ApplyHit。
要判断某种武器能否完整运行，需要继续追踪默认 Host 的入站路由、事实捕获、每帧调用与
输出结算，而不是停在弹幕定义或编解码器。

## 追踪、重连与清理

ProjectileTrackerManager 维护玩家追踪引用，相关 Packet producer 将视图投影为输出。
入服快照会捕获已有追踪关系；会话关闭时 Host 按 Epoch 执行所有者清理。
旧会话清理和旧终止请求都应校验实例代次，避免删除相同槽位上的新弹幕。

建议阅读顺序：Projectile → ProjectileManager → 同步/终止 Preparation → AiService →
MovementService/CollisionService → NpcCombatService → TrackerManager，再核对 Host 接入。
