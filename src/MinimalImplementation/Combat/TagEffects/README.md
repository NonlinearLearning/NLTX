# TagEffects：玩家标签与 NPC 命中附加效果

[返回 Combat](../README.md) · [NPC](../Npcs/README.md) · [弹幕](../Projectiles/README.md)

## 作用与状态归属

标签效果在这里按玩家维护，并关联到特定 NPC 实例。它记录玩家当前效果类型、NPC 标签
持续时间和触发计时，为命中附加伤害或次级效果提供依据。它不拥有 NPC 的生命，也不拥有
弹幕位置；这些仍由 NpcManager、ProjectileManager 管理。

## 协作部件

| 部件 | 作用 |
| --- | --- |
| [TagEffectStateOwner](TagEffectStateOwner.cs) | 单写入者；处理模块命令、计时、命中、退休和脏快照 |
| [TagEffectDefinitionCatalog](TagEffectDefinitionCatalog.cs)、[TagEffectDefinition](TagEffectDefinition.cs) | 已迁移效果的内容定义、持续和触发规则 |
| [TagEffectCommand](TagEffectCommand.cs) | 网络状态操作的领域输入 |
| [TagEffectSnapshotView](TagEffectSnapshotView.cs) | 某版本玩家效果及 NPC 关联的外部视图 |
| [TagEffectHitFacts](TagEffectHitFacts.cs) | 调用方捕获的玩家、NpcView、伤害及弹幕分类事实 |
| [TagEffectHitResult](TagEffectHitResult.cs)、[TagEffectActions](TagEffectActions.cs) | 已计算伤害、暴击和待执行的次级效果意图 |
| [ITagEffectRandom](ITagEffectRandom.cs) | 触发规则的随机输入边界 |

Owner 内部的玩家状态绑定 PlayerSlot 和 SessionEpoch；NPC 计时还记录实体代次。
NPC 槽位被重用时，旧标签不会仅因编号相同而自然适用于新实例。

## 模块 12 如何协作

1. Packet 82 由模块分发器校验认证、阶段和模块编号。
2. Host 的 [TagEffectModuleHandlerAdapter](../../Host/CompleteModuleHandlers.cs) 解码并交给 Owner。
3. Owner 验证 Epoch、世界作用域、效果定义与操作类型，用认证玩家替换不可信的包内玩家字段。
4. 命令提交后返回广播命令，由 Host producer 产生网络意图。

Action 0 的 FullState 由服务端生成，不能由客户端上报用来覆写服务端状态；其余动作分别
处理当前效果和 NPC 相关变化。具体允许范围以 Handle 的分支与定义目录为准。

## 命中与计时如何协作

`ApplyHit` 使用 TagEffectHitFacts 中的当前 NPC 身份、弹幕归属和分类等事实计算效果，
提交相关标签状态，并返回修正后的伤害与次级效果意图。调用方仍需把这些结果接到 NPC
扣血或其他效果执行链。返回 TaggedAction、ProcAction 并不代表已生成其他实体。

`AdvanceTick` 推进计时，`RetireNpc` 清理指定实例的关联，`RetireSession` 清理原连接状态。
脏快照可以捕获、发送并按捕获版本确认，旧快照确认不会直接代表新状态也已交付。

## 当前默认接入边界

默认 Host 接通模块 12、入服状态捕获及会话退休。当前默认 Tick 注册没有显式驱动
TagEffectStateOwner.AdvanceTick，默认弹幕命中链也未完整接到 ApplyHit。
因此，模块同步成功不足以证明持续时间和所有战斗附加效果已经在服务器持续执行。

建议先读 StateOwner.Handle，再读 ApplyHit 和 AdvanceTick；最后核对
[装配](../../Host/ServerComposition.cs)、[入服快照](../../Host/Version4InboundRouter.JoinSnapshots.cs)
和 [会话清理](../../Host/Version4InboundRouter.Close.cs)。
