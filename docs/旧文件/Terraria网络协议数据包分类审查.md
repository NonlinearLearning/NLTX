# Terraria 网络协议数据包分类审查

## 范围与证据

审查对象是当前物理删减源码树中的既有 Terraria 二进制协议：

- `D:\TRbackup\Version4物理删除了某些文件\Terraria.ID\MessageID.cs`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NetMessage.cs`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\MessageBuffer.cs`

`MessageID.cs` 定义有效消息号 `1..161`，`Count = 162`。`NetMessage` 是主要发送编码器；
`MessageBuffer.GetData` 是主要接收解码与状态变更入口。

本报告只审查包的网络语义、方向、权威性与新 ECS 的边界。它不实现或修改协议。

## 分类词汇

| 标记 | 含义 | 重构要求 |
| --- | --- | --- |
| `C->S` | 客户端请求、输入或声明 | 服务端必须验证身份、范围、权限和前置状态 |
| `S->C` | 服务端权威快照、结果或指令 | 客户端只应用合法投影，不重新推演服务端结论 |
| `Relay` | 服务端验证来源后转发 | 先转换为 Command/事件，再从权威世界生成广播 |
| `Both` | 两端均可能发送，但语义或权限不同 | 编解码可共享，接收授权必须按方向区分 |
| `Legacy` | 已废弃、未知或当前不应作为新设计入口 | 保持兼容读取，不在新模型中扩展 |

## 顶层分类

```text
1. 会话与连接生命周期
2. 初始世界装载与区域流送
3. 玩家身份、输入和角色状态
4. 动态实体快照：Item、NPC、Projectile
5. 世界 Tile、液体、连线与 Tile Entity 修改
6. 容器、库存、装备、Buff 和经济交互
7. 战斗、伤害、死亡与实体生命周期结果
8. NPC、Boss、入侵、任务和世界事件
9. UI、聊天、社交、音效和视觉结果
10. NetModule 扩展通道、调试与遗留包
```

## 1. 会话与连接生命周期

| ID | MessageID 名称 | 方向 | 分类 | 审查结论 |
| ---: | --- | --- | --- |
| 1 | `Hello` | `C->S` | 协议握手 | 版本和会话协商入口；不能写入游戏实体。 |
| 2 | `Kick` | `S->C` | 会话终止 | 服务端原因文本；不属于 ECS。 |
| 3 | `PlayerInfo` | `C->S` 后 `Relay` | 玩家身份声明 | 名称、外观、难度等角色资料；服务端校验后建立玩家身份投影。 |
| 4 | `SyncPlayer` | `S->C` / `Relay` | 玩家静态资料 | 不是每 tick 控制包；与 13 分开处理。 |
| 6 | `RequestWorldData` | `C->S` | 世界装载请求 | 连接阶段请求。 |
| 7 | `WorldData` | `S->C` | 世界权威快照 | 世界尺寸、模式、时间和事件相关基础数据。 |
| 9 | `StatusTextSize` | `S->C` | 装载进度 | 纯会话/UI 信息。 |
| 12 | `PlayerSpawn` | `C->S` 后 `Relay` | 生成请求/结果 | 服务端必须验证玩家状态和生成条件。 |
| 14 | `PlayerActive` | `Both` | 玩家连接状态 | 激活/停用是会话状态，不能由客户端任意激活其他槽位。 |
| 37 | `RequestPassword` | `S->C` | 登录挑战 | 会话认证。 |
| 38 | `SendPassword` | `C->S` | 登录凭据 | 不进入游戏状态或日志。 |
| 49 | `InitialSpawn` | `S->C` | 初始生成指令 | 会话装载完成后的角色进入世界。 |
| 93 | `SocialHandshake` | `Both` | 平台社交握手 | 外部会话能力，不属于 ECS。 |
| 129 | `FinishedConnectingToServer` | `C->S` | 装载完成确认 | 决定何时可开始常规同步。 |
| 139 | `SetCountsAsHostForGameplay` | `S->C` | 权限状态 | 服务端设置游戏主持权限。 |
| 161 | `HostToken` | `S->C` | 主机令牌 | 会话/平台令牌，不进入实体组件。 |

## 2. 初始世界装载与区域流送

| ID | MessageID 名称 | 方向 | 分类 | 审查结论 |
| ---: | --- | --- | --- |
| 8 | `SpawnTileData` | `S->C` | 生成点区域数据 | 初始世界切片。 |
| 10 | `TileSection` | `S->C` | 区域 Tile 快照 | 大块世界状态流送。 |
| 11 | `TileFrameSection` | `Legacy` | 已废弃 | 枚举已标记废弃；兼容读取即可。 |
| 20 | `AreaTileChange` | `S->C` | 区域 Tile 增量 | 权威世界变更结果。 |
| 73 | `RequestTeleportationByServer` | `S->C` | 区域加载/传送协调 | 客户端应加载/确认目标区域，不可自行决定位置。 |
| 158 | `ExtraSpawnSectionLoaded` | `C->S` | 区域就绪确认 | 客户端确认额外 spawn section 已到达。 |
| 159 | `RequestSection` | `C->S` | 区域请求 | 服务端必须按可见性、权限和世界边界返回数据。 |

这些包对应世界存储和流送，不应以实体 Component 代替。ECS 系统应通过只读 World Snapshot
读取 Tile；网络层将 Tile Change Command 的已提交结果编码为 17、20、63、64 等包。

## 3. 玩家身份、输入和角色状态

| ID | MessageID 名称 | 方向 | 分类 | 主要语义 |
| ---: | --- | --- | --- |
| 5 | `SyncEquipment` | `Both` | 装备槽同步 | 外观/装备载荷；服务端需校验物品合法性。 |
| 13 | `PlayerControls` | `C->S` 后 `Relay` | 输入与运动快照 | 控制位、方向、位置、可选速度、坐骑、交互状态。 |
| 16 | `PlayerLifeMana` | `S->C` | 生命/魔力权威状态 | 客户端不能以该包裁定自身生命。 |
| 30 | `TogglePVP` | `C->S` 后 `Relay` | PvP 意图 | 服务端验证世界、队伍和冷却规则。 |
| 35 | `PlayerHeal` | `S->C` / `Relay` | 治疗结果 | 作为战斗结算结果传播。 |
| 36 | `SyncPlayerZone` | `C->S` 后 `Relay` | 区域感知状态 | 客户端报告区域；服务端不能盲信高价值玩法结果。 |
| 41 | `ItemRotationAndAnimation` | `C->S` 后 `Relay` | 持有物表现/使用状态 | 需与服务端物品使用状态一致。 |
| 43 | `ManaEffect` | `S->C` / `Relay` | 魔力消耗结果 | 应来自权威物品/能力结算。 |
| 45 | `TeamChange` | `Both` | 队伍状态 | 兼容旧入口；服务端裁定。 |
| 50 | `PlayerBuffs` | `S->C` | 玩家 Buff 快照 | 投影 `BuffCollectionComponent`，不是客户端自定义 Buff。 |
| 55 | `AddPlayerBuffPvP` | `C->S` 后 `Relay` | PvP Buff 请求/结果 | 应转换为受校验的 ApplyBuffCommand。 |
| 80 | `SyncPlayerChestIndex` | `S->C` | 玩家容器使用状态 | UI/容器结果，不是 Inventory 的完整权威存储。 |
| 84 | `PlayerStealth` | `C->S` 后 `Relay` | 潜行表现状态 | 服务端验证装备和当前动作。 |
| 99 | `MinionRestTargetUpdate` | `C->S` 后 `Relay` | Minion 休息目标 | 投影至 `TargetComponent`，服务端验证目标。 |
| 102 | `NebulaLevelupRequest` | `C->S` 后 `Relay` | 套装等级请求 | 必须由服务端检查 Buff/装备前提。 |
| 115 | `MinionAttackTargetUpdate` | `C->S` 后 `Relay` | Minion 攻击目标 | 目标选择意图，不是 NPC 伤害结果。 |
| 117 | `PlayerHurtV2` | `S->C` | 受伤结果 | 投影 DamageResolution 的结果。 |
| 118 | `PlayerDeathV2` | `S->C` | 死亡结果 | 投影 DeathSystem 的结果。 |
| 120 | `Emoji` | `C->S` 后 `Relay` | 表情/社交 | 非权威游戏状态。 |
| 125 | `SyncTilePicking` | `C->S` 后 `Relay` | 挖掘/选取表现 | 服务端需将其与真正 TileManipulation 区分。 |
| 134 | `UpdatePlayerLuckFactors` | `C->S` 后 `Relay` | 幸运因子 | 不可直接接受数值；应依据服务端环境和装备重算。 |
| 135 | `DeadPlayer` | `S->C` | 死亡状态 | 角色生命周期结果。 |
| 138 | `ClientSyncedInventory` | `C->S` | 库存同步确认 | 是兼容/会话同步信号，不能覆盖权威库存。 |
| 147 | `SyncLoadout` | `Both` | 装备预设 | 服务端需验证物品和槽位。 |
| 150 | `SpectatePlayer` | `C->S` / `S->C` | 观战目标 | 权限与目标可见性必须由服务端控制。 |
| 157 | `TeamChangeFromUI` | `C->S` 后 `Relay` | UI 队伍改动 | 与 45 合并为一个新 Command 入口。 |

### PlayerControls 13 的 ECS 边界

消息 13 的发送端在 `NetMessage.cs`，接收端在 `MessageBuffer.cs`。其字节投影为：

```text
playerIndex
4 个 BitsByte
selectedItem
position
[velocity]
[mountType]
[Potion of Return 两个位置]
[cameraTarget]
```

它应解码为短命的 `PlayerControlPacket`，经过身份与移动校验后写为：

```text
PlayerInputComponent
PlayerControlComponent
TransformComponent
MotionComponent
MountStateComponent
PlayerInteractionStateComponent
CameraTargetComponent
```

不能让消息 13 直接替换玩家的全部游戏状态。

## 4. 动态实体快照与生命周期

| ID | MessageID 名称 | 方向 | 分类 | 主要语义 |
| ---: | --- | --- | --- |
| 21 | `SyncItem` | `S->C` / `Relay` | 世界掉落物快照 | 位置、速度、数量、前缀、类型。 |
| 22 | `ItemOwner` | `S->C` | 世界物品占用 | 物品拾取保留权。 |
| 23 | `SyncNPC` | `S->C` / `Relay` | NPC 权威快照 | 类型、位置、速度、目标、方向、生命、稀疏 AI。 |
| 27 | `SyncProjectile` | `S->C` / `Relay` | 投射物权威快照 | owner/identity、类型、位置、速度、伤害、稀疏 AI。 |
| 29 | `KillProjectile` | `S->C` / `Relay` | 投射物销毁 | 由 `owner + identity` 定位。 |
| 39 | `ReleaseItemOwnership` | `S->C` | 世界物品生命周期 | 释放拾取保留权。 |
| 70 | `BugCatching` | `C->S` 后 `Relay` | 小动物捕获 | 服务端验证实体、距离和背包容量。 |
| 71 | `BugReleasing` | `C->S` 后 `Relay` | 小动物释放 | 服务端生成新 NPC/实体。 |
| 88 | `ItemTweaker` | `Both` | 世界物品调整 | 需要服务器权威物品规则。 |
| 90 | `InstancedItem` | `S->C` | 实例化掉落物 | 与 21 共用部分编码，但可见性不同。 |
| 92 | `SyncExtraValue` | `S->C` | NPC/实体附加值 | 独立的实体补充同步。 |
| 130 | `FishOutNPC` | `C->S` 后 `Relay` | 钓鱼生成 NPC | 服务端验证钓鱼上下文。 |
| 142 | `SyncProjectileTrackers` | `S->C` | 投射物附加追踪器 | 27 不承载的投射物扩展状态。 |
| 145 | `SyncItemsWithShimmer` | `S->C` | Shimmer 物品状态 | 与 21 共享物品快照形状。 |
| 148 | `SyncItemCannotBeTakenByEnemies` | `S->C` | 物品临时保护 | 掉落物状态补充。 |
| 151 | `SyncItemDespawn` | `S->C` | 掉落物销毁 | 实体生命周期结果。 |
| 160 | `ItemPosition` | `S->C` / `Relay` | 物品位置更新 | 小粒度位置同步。 |

### NPC 23 的 ECS 边界

```text
npcIndex
position
velocity
target
flags
[稀疏 ai[0..3]]
netId
[缩放玩家数]
[difficulty]
[生命值]
[releaseOwner]
```

映射到：

```text
EntityIdentityComponent
TransformComponent
MotionComponent
FacingComponent
TargetComponent
NpcDefinitionComponent
NpcAiComponent
HealthComponent
NpcSpawnStateComponent
```

`ai[0..3]` 是旧客户端的协议投影，不是新游戏模型的内部 API。

### Projectile 27 与 KillProjectile 29 的 ECS 边界

27 发送：

```text
identity, position, velocity, owner, type,
optional ai[0..2], bannerId, damage, knockBack, originalDamage, projUUID
```

29 发送：

```text
identity, owner
```

因此新模型必须具有稳定的：

```text
ProjectileNetworkIdentityComponent
  Owner
  Identity
  OptionalUuid
```

但它不要求新的 ECS EntityId 与旧投射物数组槽位相同。

## 5. 世界修改、Tile、连线与 Tile Entity

| ID | MessageID 名称 | 方向 | 分类 | 审查结论 |
| ---: | --- | --- | --- |
| 17 | `TileManipulation` | `C->S` 后 `Relay` | 单点世界修改命令 | 破坏、放置、墙、线、斜坡等；必须校验距离、工具、区域和权限。 |
| 19 | `ToggleDoorState` | `C->S` 后 `Relay` | 交互命令 | 服务端判定 Door/Tile 当前状态。 |
| 34 | `ChestUpdates` | `C->S` 后 `Relay` | 容器关联世界修改 | 包含放置、销毁、重命名等多子操作。 |
| 42 | `Unknown42` | `Legacy` | 未命名 | 只保留兼容路径。 |
| 48 | `LiquidUpdate` | `Legacy` | 已废弃 | 枚举建议改用 `NetLiquidModule`。 |
| 52 | `LockAndUnlock` | `C->S` 后 `Relay` | 容器/门锁操作 | 服务端检查钥匙、距离、状态。 |
| 59 | `HitSwitch` | `C->S` 后 `Relay` | 机关触发 | 转换为 World Interaction Command。 |
| 63 | `SyncTilePaintOrCoating` | `S->C` / `Relay` | Tile 外观状态 | 已提交世界结果。 |
| 64 | `SyncWallPaintOrCoating` | `S->C` / `Relay` | Wall 外观状态 | 已提交世界结果。 |
| 79 | `PlaceObject` | `C->S` 后 `Relay` | 多 Tile 对象放置 | 比 17 更高层的世界操作。 |
| 85 | `QuickStackChests` | `C->S` | 批量库存操作 | 服务端完成库存和容器原子结算。 |
| 86 | `TileEntitySharing` | `S->C` / `Both` | Tile Entity 数据 | 包含对象专用序列化。 |
| 87 | `TileEntityPlacement` | `C->S` 后 `Relay` | Tile Entity 放置 | 服务端验证 Tile 和空间。 |
| 89 | `ItemFrameTryPlacing` | `C->S` | Tile Entity 交互 | 服务端校验陈列框实体和物品。 |
| 105 | `GemLockToggle` | `C->S` 后 `Relay` | 世界机关状态 | 服务端权威。 |
| 108 | `WiredCannonShot` | `C->S` 后 `Relay` | 连线武器触发 | 服务端校验机关和弹药。 |
| 109 | `MassWireOperation` | `C->S` | 批量连线请求 | 高风险世界写操作。 |
| 110 | `MassWireOperationPay` | `S->C` | 批量连线结算 | 费用/结果。 |
| 121 | `TEDisplayDollDataSync` | `S->C` / `Relay` | 展示人偶 Tile Entity | 专有状态同步。 |
| 122 | `RequestTileEntityInteraction` | `C->S` | Tile Entity 交互请求 | 服务端根据实体、距离和权限处理。 |
| 123 | `WeaponsRackTryPlacing` | `C->S` | 武器架操作 | 服务端验证。 |
| 124 | `TEHatRackItemSync` | `S->C` / `Relay` | 帽架 Tile Entity | 专有状态同步。 |
| 128 | `LandGolfBallInCup` | `C->S` 后 `Relay` | 高尔夫世界交互 | 服务端验证轨迹/球/球洞。 |
| 133 | `FoodPlatterTryPlacing` | `C->S` | 食物盘放置 | 服务端验证。 |
| 146 | `ShimmerActions` | `S->C` | Shimmer 世界结果 | 客户端只播放/应用权威结果。 |
| 149 | `DeadCellsDisplayJarTryPlacing` | `C->S` | 模组 Tile Entity 放置 | 与 87 同类。 |
| 154 | `Ping` | `C->S` 后 `Relay` | 地图标记 | 需要范围和团队可见性校验。 |
| 155 | `SyncChestSize` | `S->C` | 容器结构结果 | 权威容器容量。 |
| 156 | `TELeashedEntityAnchorPlaceItem` | `C->S` | 拴系实体锚点 | 服务端验证。 |

## 6. 容器、库存、装备、Buff 与经济

| ID | MessageID 名称 | 方向 | 分类 | 审查结论 |
| ---: | --- | --- | --- |
| 31 | `RequestChestOpen` | `C->S` | 容器打开请求 | 服务端检查距离、占用和可访问性。 |
| 32 | `SyncChestItem` | `S->C` / `Relay` | 容器格子结果 | 服务端权威库存变更。 |
| 33 | `SyncPlayerChest` | `S->C` | 容器完整内容 | 客户端 UI 投影。 |
| 46 | `OpenSignRequest` | `C->S` | 标牌读取请求 | 服务端检查范围。 |
| 47 | `OpenSignResponse` | `S->C` | 标牌内容 | 权威文本。 |
| 51 | `MiscDataSync` | `Both` | 杂项角色数据 | 应在新协议逐步拆为具名子包；旧包保持适配。 |
| 53 | `AddNPCBuff` | `C->S` 后 `Relay` | NPC Buff 请求/结果 | 转换为 ApplyBuffCommand 并检查来源。 |
| 54 | `NPCBuffs` | `S->C` | NPC Buff 快照 | 投影 `BuffCollectionComponent`。 |
| 56 | `UniqueTownNPCInfoSyncRequest` | `C->S` | Town NPC 信息请求 | 服务端返回权威资料。 |
| 58 | `InstrumentSound` | `C->S` 后 `Relay` | 乐器表现 | 非核心权威状态。 |
| 69 | `ChestName` | `S->C` / `Relay` | 容器命名 | 服务端控制写入和广播。 |
| 72 | `TravelMerchantItems` | `S->C` | 商店库存 | 权威商店快照。 |
| 74 | `AnglerQuest` | `S->C` | 每日任务 | 世界/玩家任务状态。 |
| 75 | `AnglerQuestFinished` | `C->S` 后 `Relay` | 任务完成请求 | 服务端验证任务条件。 |
| 76 | `QuestsCountSync` | `S->C` | 任务计数 | 权威结果。 |
| 104 | `ShopOverride` | `S->C` | 商店覆盖 | 服务端控制价格/库存视图。 |
| 131 | `TamperWithNPC` | `C->S` 后 `Relay` | NPC 特殊交互 | 视为 NPC Interaction Command。 |
| 137 | `RequestNPCBuffRemoval` | `C->S` | NPC Buff 移除请求 | 服务端验证来源和可移除性。 |
| 152 | `ItemUseSound` | `S->C` / `Relay` | 物品使用表现 | 不应作为物品使用的权威触发。 |

## 7. 战斗、伤害、死亡与实体结果

| ID | MessageID 名称 | 方向 | 分类 | 审查结论 |
| ---: | --- | --- | --- |
| 24 | `UnusedMeleeStrike` | `Legacy` | 旧近战路径 | 不作为新战斗入口。 |
| 28 | `DamageNPC` | `C->S` 后 `Relay` | NPC 伤害报告/结果 | 高风险；服务端必须重算命中、伤害和免疫。 |
| 57 | `Unknown57` | `Legacy` | 未命名 | 兼容处理。 |
| 81 | `CombatTextInt` | `S->C` | 伤害数字表现 | 纯结果展示。 |
| 97 | `AchievementMessageNPCKilled` | `S->C` | 成就结果 | 不负责击杀判定。 |
| 98 | `AchievementMessageEventHappened` | `S->C` | 成就/事件结果 | 不负责事件判定。 |
| 103 | `MoonlordHorror` | `S->C` | Boss 视觉/结果 | 世界事件表现。 |
| 106 | `PoofOfSmoke` | `S->C` | 视觉结果 | 不改变权威世界。 |
| 112 | `SpecialFX` | `S->C` | 视觉/特殊效果 | 结果播放。 |
| 119 | `CombatTextString` | `S->C` | 战斗文本表现 | 结果展示。 |
| 132 | `PlayLegacySound` | `S->C` / `Relay` | 音效结果 | 不作为权威游戏逻辑。 |
| 153 | `NPCDebuffDamage` | `S->C` | DoT 伤害结果 | 应来自 Buff/Damage 系统。 |

新 ECS 的伤害入口统一为：

```text
输入/碰撞/能力
  -> DamageRequestedEvent
  -> DamageResolutionSystem
  -> ApplyDamageCommand
  -> HealthComponent 提交
  -> DamageAppliedEvent / EntityDiedEvent
  -> 28、35、81、117、118、119、153 等协议投影
```

客户端包不应直接写 `HealthComponent` 或删除 NPC/Projectile 实体。

## 8. NPC、Boss、入侵、任务与世界事件

| ID | MessageID 名称 | 方向 | 分类 | 审查结论 |
| ---: | --- | --- | --- |
| 40 | `SyncTalkNPC` | `S->C` / `Relay` | NPC 对话目标 | 服务端决定可交互 NPC。 |
| 60 | `Unknown60` | `Legacy` | 未命名 | 兼容处理。 |
| 61 | `SpawnBossUseLicenseStartEvent` | `C->S` 后 `Relay` | Boss/事件请求 | 服务器检查物品、世界阶段和冷却。 |
| 62 | `Unknown62` | `Legacy` | 未命名 | 兼容处理。 |
| 77 | `TemporaryAnimation` | `S->C` | 世界事件表现 | 只读视觉时间线。 |
| 78 | `InvasionProgressReport` | `S->C` | 入侵权威状态 | `WorldEventState` 投影。 |
| 91 | `SyncEmoteBubble` | `S->C` / `Relay` | NPC/玩家表情结果 | 非核心模拟。 |
| 100 | `TeleportNPCThroughPortal` | `S->C` | NPC 空间结果 | 服务端已完成传送。 |
| 101 | `UpdateTowerShieldStrengths` | `S->C` | 世界事件状态 | 四塔护盾的权威快照。 |
| 107 | `SmartTextMessage` | `S->C` | 文本事件 | UI/聊天结果。 |
| 111 | `ToggleParty` | `C->S` 后 `Relay` | 派对事件请求 | 服务端控制世界事件。 |
| 113 | `CrystalInvasionStart` | `C->S` 后 `Relay` | 事件启动请求 | 服务端检查前置条件。 |
| 114 | `CrystalInvasionWipeAllTheThingsss` | `S->C` | 事件清理结果 | 权威世界事件结果。 |
| 116 | `CrystalInvasionSendWaitTime` | `S->C` | 事件计时 | 世界事件快照。 |
| 126 | `SyncRevengeMarker` | `S->C` | 世界/死亡标记 | 权威标记。 |
| 127 | `RemoveRevengeMarker` | `S->C` | 世界/死亡标记移除 | 权威结果。 |
| 136 | `SyncCavernMonsterType` | `S->C` | 世界生态状态 | 客户端显示/生成辅助状态。 |
| 140 | `SetMiscEventValues` | `S->C` | 世界事件快照 | 应映射到独立 World Event State。 |
| 141 | `RequestLucyPopup` | `C->S` 后 `Relay` | 特殊物品交互 | 服务端校验物品与玩家。 |
| 143 | `CrystalInvasionRequestedToSkipWaitTime` | `C->S` | 事件跳过请求 | 服务端检查投票/权限。 |
| 144 | `RequestQuestEffect` | `C->S` | 任务效果请求 | 服务端判定。 |

这些状态不属于某一只 NPC。旧 `NPC.cs` 中的 Boss 索引、入侵计数、事件开关应迁移为
World Event State 或专门事件领域的资源，而不是 `NpcComponent`。

## 9. UI、聊天、社交、音效与调试

| ID | MessageID 名称 | 方向 | 分类 | 审查结论 |
| ---: | --- | --- | --- |
| 65 | `TeleportEntity` | `S->C` / `Relay` | 实体传送表现/结果 | 实际位置变更应先由服务端提交。 |
| 66 | `Unknown66` | `Legacy` | 未命名 | 兼容处理。 |
| 67 | `Unknown67` | `Legacy` | 未命名 | 兼容处理。 |
| 68 | `Unknown68` | `Legacy` | 未命名 | 兼容处理。 |
| 82 | `NetModules` | `Both` | 扩展模块复用通道 | 需二次按 module type 分类，见下一节。 |
| 94 | `DevCommands` | `C->S` | 调试命令 | 仅限授权开发会话。 |
| 95 | `MurderSomeoneElsesPortal` | `C->S` 后 `Relay` | 特殊实体交互 | 服务端验证所有权与距离。 |
| 96 | `TeleportPlayerThroughPortal` | `S->C` | 玩家传送结果 | 权威位置结果。 |
| 109 | `MassWireOperation` | `C->S` | 工具/世界操作 | 见世界修改分类。 |
| 120 | `Emoji` | `C->S` 后 `Relay` | 社交表现 | 不写入游戏模拟。 |
| 132 | `PlayLegacySound` | `S->C` / `Relay` | 音效 | 表现层。 |
| 154 | `Ping` | `C->S` 后 `Relay` | 地图标记 | 团队/可见性语义。 |

## 10. NetModules、未知与废弃包

### NetModules 82

`NetModules` 是一个二级协议容器。消息号 82 本身不能作为单一业务类别：需要先读取
模块类型，再转交对应的 `NetModule`。在新架构中应建立：

```text
NetModule packet
  -> ModuleType decoder
  -> 指定 Protocol Adapter
  -> Validate/Authorize
  -> Command 或只读 UI 事件
```

不能让通用 `MessageBuffer` 在没有模块白名单的情况下直接执行任意模块行为。

### 废弃与未知

以下枚举明确或事实标记为不应成为新设计入口：

```text
0 NeverCalled
11 TileFrameSection
15 Unknown15
24 UnusedMeleeStrike
25 Unused25
26 Unused26
42 Unknown42
44 Unknown44
48 LiquidUpdate
57 Unknown57
60 Unknown60
62 Unknown62
66 Unknown66
67 Unknown67
68 Unknown68
83 Unused83
```

处理原则：

```text
保留读取/转发兼容性
不为它们创建新的 ECS 组件或系统
若当前运行时实际命中，先记录字段和调用者，再决定是否映射到具名包
```

## 面向 ECS 重构的协议边界

### 输入与请求包

下列包应进入 `Protocol -> Command`，不能直接修改组件：

```text
13 PlayerControls
17 TileManipulation
19 ToggleDoorState
28 DamageNPC
31 RequestChestOpen
52 LockAndUnlock
59 HitSwitch
61 SpawnBossUseLicenseStartEvent
70 BugCatching
71 BugReleasing
75 AnglerQuestFinished
79 PlaceObject
85 QuickStackChests
87 TileEntityPlacement
89 ItemFrameTryPlacing
99 MinionRestTargetUpdate
108 WiredCannonShot
109 MassWireOperation
113 CrystalInvasionStart
115 MinionAttackTargetUpdate
122 RequestTileEntityInteraction
123 WeaponsRackTryPlacing
128 LandGolfBallInCup
130 FishOutNPC
131 TamperWithNPC
137 RequestNPCBuffRemoval
141 RequestLucyPopup
143 CrystalInvasionRequestedToSkipWaitTime
144 RequestQuestEffect
149 DeadCellsDisplayJarTryPlacing
156 TELeashedEntityAnchorPlaceItem
157 TeamChangeFromUI
159 RequestSection
```

### 权威快照和结果包

下列包应由 ECS 的已提交世界投影生成：

```text
7 WorldData
10 TileSection
16 PlayerLifeMana
20 AreaTileChange
21/90/145/148/151 Item synchronization
23 SyncNPC
27 SyncProjectile
29 KillProjectile
32/33 Chest contents
50 PlayerBuffs
54 NPCBuffs
78 InvasionProgressReport
86/121/124 Tile Entity state
101 UpdateTowerShieldStrengths
117 PlayerHurtV2
118 PlayerDeathV2
126/127 Revenge markers
135 DeadPlayer
140 SetMiscEventValues
142 SyncProjectileTrackers
146 ShimmerActions
155 SyncChestSize
```

## 风险与优先级

| 优先级 | 包或类别 | 风险 | 新架构要求 |
| --- | --- | --- | --- |
| P0 | 13、17、28、27、23、29 | 客户端伪造移动、世界写入、伤害或实体身份 | 明确 DTO、授权层、golden-byte 编解码测试 |
| P0 | 31-34、52、85、87、89、122-124 | 库存和 Tile Entity 双花/越权 | 服务端原子 Command 和版本/占用检查 |
| P1 | 50、53-55、117、118、153 | Buff、伤害、死亡状态漂移 | 统一 StatusEffects/Combat 的权威结果投影 |
| P1 | 61、78、101、113、114、116、140、143 | 世界事件状态被塞进 NPC 或客户端包 | 独立 WorldEventState 和服务端事件系统 |
| P1 | 82 NetModules | 二级分派绕过统一授权 | 每个 module 具名 decoder 与白名单 |
| P2 | 41、43、58、77、81、91、106、112、119、120、132、152 | 表现包误触发权威规则 | 保持 Presentation-only，不参与 simulation commit |
| P2 | Unknown/Legacy | 不可审计历史兼容点 | 运行时命中日志和隔离兼容适配器 |

## 审查结论

现有协议不是“Player/NPC/Projectile 三个大对象的同步协议”，而是多类网络语义的混合：

```text
客户端输入与交互请求
服务端世界与实体快照
服务端战斗/生命周期结果
容器和 Tile Entity 事务
世界事件状态
表现/UI/社交消息
扩展模块与遗留兼容
```

彻底重构时，`Player.cs`、`NPC.cs`、`Projectile.cs` 的替换条件不是保留全部旧字段，
而是为每个仍在使用的协议包提供一个稳定、经过授权的状态投影或 Command 入口。

特别是：

```text
13 不是完整 Player
23 不是完整 NPC
27 不是完整 Projectile
17 不是世界状态，而是世界修改请求
28 不是可信伤害结算
82 不是单一业务包，而是模块容器
```

这些边界应成为后续 ECS 重构的协议合同。
