# 类别 04：动态实体快照与生命周期消息执行文档

文档 ID：NETMSG-CAT-04
执行模型：gpt-6-luna，reasoning=max
分支：codex/network-message-04-entity-lifecycle
会话：network-message-04-entity-lifecycle
类别 owner：WorldItem、NPC、Projectile 快照、实体网络身份和生命周期结果

## 1. 目标

完成 Item/NPC/Projectile 相关 18 个消息的注册与领域 owner 映射。网络槽位、owner+identity、
UUID 和旧数组索引只作为兼容投影，不能直接成为新 ECS EntityId。所有 C2S 声明必须经过
发送者绑定、实体所有权、世界代次和生命周期校验。

## 2. 消息范围

21 SyncItem；22 ItemOwner；23 SyncNPC；27 SyncProjectile；29 KillProjectile；
39 ReleaseItemOwnership；70 BugCatching；71 BugReleasing；88 ItemTweaker；
90 InstancedItem；92 SyncExtraValue；130 FishOutNPC；142 SyncProjectileTrackers；
145 SyncItemsWithShimmer；148 SyncItemCannotBeTakenByEnemies；151 SyncItemDespawn；
160 ItemPosition。

## 3. 基线

当前已有 SteamItemPacketGatewayRegistration 负责 21/151，ProjectilePacketGatewayRegistration
负责 27/29，NetworkWorldItemOwner、ProjectileNetworkCommandOwner 和对应 verifier 已存在。
先确认这些注册是唯一 owner，并检查当前 packet 方向是否与 21/27/29 的源码事实一致；
不要另建一套 Item/Projectile handler 代替现有 owner。

23、27 的快照字段不能被解释成完整 NPC/Projectile 状态；27 的 owner/identity/UUID、
29 的 owner+identity 必须形成稳定网络身份。21/90/145/148/151 的可见性和实例化范围
不同，不能以一个广播策略覆盖。

## 4. 实施步骤

1. 生成 ID、方向、codec 和当前 registration inventory，标出已经实现、仅格式存在、
   opaque 或缺语义证据的条目。
2. 对 21、27、29、151 先固定最小权威路径：Actor 绑定、身份映射、过期代次、实体存在/
   复用槽、所有权和拒绝原因。
3. 对 22、39、148 建立 item reservation/ownership 投影，确保释放/保护不会误操作新复用
   的槽位。
4. 对 23、92、142 建立 NPC/Projectile 增量快照接口，明确稀疏 AI/附加值不是内部模型
   API；快照必须是 detached snapshot。
5. 对 70、71、88、130、160 只把客户端请求转为经过 owner 校验的 command，检查距离、
   捕捉/释放上下文、背包容量、实体 UUID 和频率。
6. 对 90、145 维护实例化/可见性区别；发送失败不回滚已提交的掉落或实体状态。
7. 处理槽位复用和跨会话旧引用，补最小注册适配，不改无关实体系统。

## 5. 非目标

不实现 NPC AI、Projectile 物理、伤害结算、完整物品背包；不把网络同步包直接写组件；
不做全量实体 golden 或长时负载。

## 6. 10% 核心测试

只执行两组核心测试：

1. 21/151 世界物品同步和销毁：提交、广播目标、槽位复用、旧引用拒绝。
2. 27/29 投射物身份：owner/identity/UUID、伪造 owner、终止后重复包和旧代次拒绝。

优先使用 NetworkWorldOwnerVerification、ProjectileGatewayVerification、
ProjectileNetworkCommandOwnerGatewayVerification；只运行选定 case。对 23/70/71/130
等未闭合条目至少做 codec/registration 静态证据，不以未跑行为测试冒充 complete。

## 7. 验收

报告必须列 18 个 ID 的唯一 owner、方向、身份字段和生命周期状态；附成功、伪造或过期
拒绝的输出，记录构建项目、exit code、warning/error、Build/bin 路径，以及与第 3/7/8
类共享实体 owner 的接口冲突。没有槽位复用证据时只能 partial。

