# Version4 七大核心类型子系统盘点报告

> 盘点对象：`D:\TRbackup\Version4`。
>
> 本报告专门回答“`Entity`、`Player`、`NPC`、`Projectile`、`Item`、`Chest`、`Main` 分别承载了哪些子系统”。
> 这里的“子系统”按状态所有权、主要读写者、生命周期和副作用边界分组，不按文件名或字段名机械拆分。
>
> `dome` 已确认是旧实现，本报告不使用 `dome` 证明当前实现行为；它不参与当前 Version4 的证据链。

## 1. 证据与范围

### 1.1 文件规模

| 类型 | 文件 | 行数 | 方法声明近似数 | Version4 外部引用文件数（含文本命中前的源码检索） |
|---|---|---:|---:|---:|
| `Entity` | `Terraria/Entity.cs` | 213 | 4 | 22 |
| `Player` | `Terraria/Player.cs` | 26,856 | 387 | 96 |
| `NPC` | `Terraria/NPC.cs` | 79,688 | 354 | 68 |
| `Projectile` | `Terraria/Projectile.cs` | 54,799 | 231 | 33 |
| `Item` | `Terraria/Item.cs` | 48,927 | 80 | 63 |
| `Chest` | `Terraria/Chest.cs` | 1,299 | 35 | 16 |
| `Main` | `Terraria/Main.cs` | 14,319 | 153 | 156 |

“方法声明近似数”来自源码声明形态检索，不能代替编译器语义分析；它用于显示职责集中度，不是 API 计数承诺。

### 1.2 证据等级

- **confirmed**：Version4 中存在明确类型、字段、方法或调用关系证据。
- **partial**：名称和局部调用足以判断候选职责，但完整读写者、顺序、持久化或网络契约仍需继续核对。
- **missing**：当前来源没有足够证据，不把它当成已实现边界。

SS14 仅用于组织模式参考：其 `Content.Shared` 将组件与领域 System、Event、Container、Projection 分开；不用于确认 Terraria 字段语义。NLTX 的 `src` 组件也不能反向证明 Version4 已经采用这些边界。

## 2. 总体子系统地图

```text
Main / 全局调度与注册表
├── 世界状态、时间、天气、事件、侵袭
├── Entity arrays: Player[] / NPC[] / Projectile[] / WorldItem[] / Chest[]
├── 网络会话、World/Player 文件与内容数据库
├── Tile/液体/地图/光照/渲染/UI/音频全局入口
└── 游戏主循环：Update -> world/entity updates -> networking/presentation

Entity / 公共实体基类
├── 空间状态：position、velocity、width、height、direction
├── 液体接触：wet、shimmerWet、honeyWet、lavaWet、wetCount
└── 纯几何/空间派生：Center、Hitbox、Distance、Angle、Direction

Player / 角色与会话实体
├── 控制输入与移动/重力/液体/碰撞
├── 生命、法力、免疫、Buff、死亡、复活、墓碑
├── 库存、装备、手持物、弹药、物品使用与拾取
├── 建造/Tile 交互、钓鱼、坐骑、钩爪、翅膀、飞行
├── Player <-> NPC/Projectile/WorldItem/Chest 关系
├── 角色复制、保存、聊天、观战与客户端表现
└── 幸运、事件、环境、视觉特效等跨域耦合

NPC / 非玩家实体与 AI
├── 自然生成、城镇生成、事件/Boss/invasion 生成
├── AI 状态机、目标选择、寻路/传送/变形
├── 生命、防御、伤害、Buff/免疫、受击与死亡掉落
├── Tile/液体/重力/坡面/传送/碰撞
├── Bestiary、Banner、战利品与玩家交互
├── 网络流式同步与区段更新
└── 视觉、动画、声音和 Town NPC 状态

Projectile / 投射物实例
├── 类型定义、来源、Owner、identity 与槽位分配
├── AI/extra update/轨迹/方向/特殊行为
├── PVE/PVP 伤害、穿透、免疫、Buff、切 Tile
├── Tile/液体/风/高尔夫/绳索/鞭/鱼漂/哨兵等特例
├── 寿命、销毁、反射、嵌入和旧投射物回收
├── 网络同步、UUID/identity 查找、区段更新
└── 光照、动画、历史轨迹和命中特效

Item / 物品定义与实例
├── 类型默认值、变体、前缀、稀有度、价格
├── 武器、工具、弹药、药水、食物、钓鱼、坐骑、放置物
├── 堆叠、拾取、消耗、合成、商店和掉落
├── 装备/饰品/时装/染料/套装加成
├── Projectile/NPC/Tile/Creative/Buff 关联
├── 网络默认值、克隆、修复与反作弊
└── Tooltip、声音、颜色、绘制和表现投影

Chest / 世界容器、银行与商店
├── 坐标/索引/类型/容量/名称
├── 创建、放置、查找、锁定、占用、删除
├── Item[] 内容与 Resize
├── Bank、Shop、Travel Shop 特殊库存
├── 网络同步、存档加载兼容与客户端动画
└── 与 Player、Tile、WorldGen、NetMessage 的边界适配
```

## 3. `Entity` 子系统

证据：[`Entity.cs`](D:/TRbackup/Version4/Terraria/Entity.cs:6)。`Player`、`NPC`、`Projectile` 直接继承它；`Item` 和 `Chest` 不继承它，因此不能把物品/容器状态错误归入公共实体状态。

| 子系统 | 关键成员/方法 | 生命周期与副作用 | ECS 边界判断 | 状态 |
|---|---|---|---|---|
| 空间位置 | `position`；`TopLeft`、`Center` 等 | 创建至销毁，移动系统和传送写入 | `LocationComponent` | confirmed |
| 运动 | `velocity`、`oldVelocity` | 每 tick 或特殊行为更新 | `VelocityComponent`；历史速度另作缓存/表现 | confirmed/partial |
| 碰撞几何 | `width`、`height`、`Size`、`Hitbox` | 生成时设置，变体/缩放可调整 | `ColliderComponent`；碰撞策略不要塞入几何组件 | confirmed |
| 朝向 | `direction`、`oldDirection` | 输入/AI/行为写入 | 仅水平基础方向可共享；NPC 垂直方向、Projectile 轨迹方向分域 | confirmed |
| 液体接触 | `wet`、`shimmerWet`、`honeyWet`、`lavaWet`、`wetCount`、`AnyWet` | 碰撞/环境更新，影响移动和状态 | 不直接创建公共 `WetComponent`；按实体规则分域，公共处只保留查询/策略 | confirmed/partial |
| 空间纯查询 | `AngleTo`、`Distance`、`DirectionTo`、`DirectionFrom` | 无写入 | 适合纯 Query；不应缓存或持有全局状态 | confirmed |
| 兼容来源 | `IEntitySourceTarget` 实现 | 为 Item/NPC/Projectile 来源链提供关系 | Adapter/Source projection，不是实体组件行为 | partial |

### `Entity` 的拆分结论

- `Location`、`Velocity`、`Collider` 是最稳定的公共能力边界。
- `Center`、`Hitbox`、距离和方向是派生查询，不是可写组件字段。
- 液体字段虽然位于基类，但 Player、NPC、Projectile 的规则不同；不能因为字段同名就合并。
- 不创建 `BaseEntityComponent` 或 `SpatialComponent` 聚合组件，否则会把已经区分的访问模式重新耦合。

## 4. `Player` 子系统

证据主文件：[`Player.cs`](D:/TRbackup/Version4/Terraria/Player.cs)。该类同时承担角色权威状态、输入、物品流程、世界交互和客户端表现，是七类中最明显的多职责聚合体。

| 子系统 | Version4 证据 | 主要职责 | 建议边界 | 状态 |
|---|---|---|---|---|
| 会话/槽位生命周期 | `active`、`host`、`PlayerConnect`、`PlayerDisconnect`（约 291-298、478） | 连接、断线、槽位活动、主机资格 | `PlayerLifecycle` + Session Adapter | confirmed |
| 输入意图 | `controlLeft/right/up/down/jump/useItem/useTile`（约 1219 起），`UpdateControlHolds` | 读取键鼠/网络输入，形成意图 | `InputIntent`；输入不直接拥有 Velocity | confirmed |
| 移动与物理 | `HorizontalMovement`、`JumpMovement`、`DashMovement`、`WallClimbMovement`、`WingMovement`、`GrappleMovement`、`TileCollision` | 速度、重力、坡面、平台、绳索、飞行、钩爪 | Movement/Physics System；Player 只提供专属意图与能力 | confirmed |
| 液体/呼吸/环境 | `WetCollision`、`DryCollision`、`CheckDrowning`、`TryFloatingInFluid`；`breath`、`lavaTime`、`ignoreWater` | 液体接触、溺水、岩浆、环境移动效果 | Player Environment/Contact；不与 NPC/Projectile 的液体策略合并 | confirmed |
| 生命与法力资源 | `statLife`、`statLifeMax`、`statMana`、`statManaMax`、`HealEffect`、`ManaEffect`、`UpdateLifeRegen`、`UpdateManaRegen` | 当前/最大生命、资源回复、治疗反馈 | Health、Mana/Resource 分离；反馈为 Projection/Effect | confirmed |
| 伤害、免疫、死亡 | `Hurt`、`KillMe`、`UpdateImmunity`、`SetImmuneTimeForAllTypes`、`GetRespawnTime` | 伤害结算、免疫、死亡、复活计时、死亡副作用 | Combat + PlayerLifecycle；死亡事件不可藏在 Health 组件 | confirmed |
| Buff/状态效果 | `buffType[]`、`buffTime[]`、`buffImmune[]`、`AddBuff`、`DelBuff`、`ClearBuff`、`UpdateBuffs` | 效果容器、免疫、到期、宠物/增益派生 | StatusEffects 集合 + Player 专属策略；单条效果独立建模 | confirmed |
| 库存/槽位 | `inventory`、`armor`、`dye`、`miscEquips`、`miscDyes`、`trashItem`；`CountItem`、`ConsumeItem` | 物品组织、装备栏、染料栏、消耗和筛选 | Inventory、Equipment、Hands、Trash/Storage 分开 | confirmed |
| 拾取/堆叠/虚空仓库 | `CanAcceptItemIntoInventory`、`ItemSpace`、`GetItem`、`PickupItem`、`useVoidBag` | 世界物品进入库存、堆叠、空间判断、特殊仓库 | Item transfer Command + Inventory Query；不能让 Item 自己修改 Player | confirmed |
| 物品使用/攻击 | `ItemCheck`、`ItemCheck_TryStartUse`、`CheckMana`、`PickAmmo`、`GetWeaponDamage`、`StartChanneling` | 使用动画、法力支付、弹药、生成 Projectile、自动重复 | Use/Weapon/Resource/Projectile Spawn 系统 | confirmed |
| 装备与套装 | `UpdateEquips`、`UpdateArmorSets`、`GrantArmorBenefits`、`GrantPrefixBenefits`、`UpdateDyes` | 装备派生防御、前缀、套装奖励、视觉染色 | Equipment definition → derived combat/presentation state | confirmed |
| Tile/世界交互 | `LookForTileInteractions`、`TileInteractionsCheck/Use`、`IsInTileInteractionRange`、`BiomeTorchPlaceStyle` | 放置、挖掘、交互范围、智能光标、容器/机关交互 | Interaction Query + Command + World/Tile Adapter | confirmed |
| NPC/Projectile 关系 | `ApplyDamageToNPC`、`CollideWithNPCs`、`CanNPCBeHitByPlayerOrPlayerProjectile`、`RemoveAllGrapplingHooks` | 角色对 NPC 伤害、近战碰撞、钩爪/投射物关系 | Combat/Relationship System；不把 Target/Owner 塞入 Player 基础组件 | confirmed |
| Spawn/Teleport | `Spawn`、`Spawn_GetPositionAtWorldSpawn`、`Teleport`、`PlayerNoSpaceTeleport` | 出生点、安全区、传送效果、位置修正 | Spawn/Teleport Command + World Geometry Query | confirmed |
| 坐骑/载具/翅膀 | `WingMovement`、`GetWingStats`、`Mount` 相关字段、`HandleMount` | 坐骑、矿车、翅膀、飞行能力 | Ability/Vehicle Components；表现和模拟分离 | confirmed |
| 钓鱼/资源 | `UpdateFishingBobber`、`QuickMana`、食物/饥饿方法 | 鱼漂、资源消耗、食物和饥饿状态 | Fishing/Resource/Status 分域 | confirmed |
| 幸运/事件资格 | `RollLuck`、`UpdateLuck`、`RecalculateLuck`、`AddCoinLuck` | 幸运值、瓢虫、硬币等临时因子 | Luck Query/Modifier；不要作为通用 Player 核心组件 | confirmed |
| 网络/存档 | `SavePlayer`、`Serialize`、`Deserialize`、`PlayerConnect/Disconnect`、`NewMessage` | 文件存档、网络同步、聊天/消息 | Player Snapshot + Persistence/Network Adapter | confirmed |
| 表现/动画 | `PlayerFrame`、`UpdateVisibleAccessories`、`WingFrame`、`UpdateTeleportVisuals`、`UpdateSocialShadow` | 精灵帧、饰品可见性、翅膀特效、影子、粒子 | Presentation Projection；不能反向成为权威模拟状态 | confirmed |
| 社交/观战/UI | `SetTalkNPC`、`SetOrRequestSpectating`、`NewMessage`、交互锁定 | 对话、观战、UI 状态和聊天 | Session/UI Adapter | confirmed |

### `Player` 的拆分优先级

1. 先拆生命周期、输入意图、Health/Mana、Inventory/Equipment 四类权威状态。
2. 再将 `ItemCheck`、Tile 交互、拾取、攻击转换为 Command/System，而不是继续扩大 Player 组件。
3. 将动画、粒子、声音、可见饰品、阴影和 UI 从服务器权威边界移出。
4. `control*` 与 `release*` 不应统称为“服务器可直接使用的控制状态”：服务器可以消费经验证的输入命令，但输入采样、客户端预测和权威结果必须区分。

## 5. `NPC` 子系统

证据主文件：[`NPC.cs`](D:/TRbackup/Version4/Terraria/NPC.cs)。该类将生成调度、AI、战斗、掉落、网络和大量具体 NPC 行为集中在一起。

| 子系统 | Version4 证据 | 主要职责 | 建议边界 | 状态 |
|---|---|---|---|---|
| 自然生成与槽位 | `SpawnNPC`、`TrySpawnAnNPC`、`GetSpawnRate`、`GetSpawnArea`、`FindSpawnTile`、`GetAvailableNPCSlot`（约 185-1190、66954-67133） | 生成资格、位置、槽位、玩家附近生成 | Spawn System + Spawn Policy Query | confirmed |
| 城镇/事件/Boss 生成 | `SpawnAllowed_*`、`SpawnBoss`、`SpawnOnPlayer`、`SpawnWOF`、`SlimeRainSpawns`、`SpawnFaelings` | Town NPC、Boss、事件敌人、特殊生成 | 各事件/内容域 System，不放入通用 NPC 组件 | confirmed |
| 基础定义与变体 | `type`、`SetDefaultsFromNetId`、`SetDefaults_ForNetId`、`ResetForNewNPC`、`SetWorldSpecificMonstersByWorldID`、`ScaleStats` | 类型、世界种子、难度和玩家数缩放 | NPC Definition/Archetype + Spawn Projection | confirmed |
| AI 状态机 | `ai[]`、`localAI[]`、`aiStyle`、`aiAction`、`AI` 及大量 `AI_*` 方法（约 18642 起） | 行为状态、计时、特定敌人逻辑 | NpcAiState + 按行为族拆分的 Systems/Policies | confirmed |
| 目标选择 | `target`、`TargetClosest`、`GetTargetData`、`TryTrackingTarget` | 目标玩家、视线、距离、追踪缓存 | Targeting Relation + Target Query | confirmed |
| 生命/防御/攻击 | `life`、`lifeMax`、`damage`、`defense`、`StrikeNPC`、`checkArmorPenetration`、`GetAttackDamage_*` | 受击、伤害输出、防御、难度缩放 | Health/Defense/Attack Capability 分离 | confirmed |
| Buff/免疫/回复 | `buffType[]`、`buffTime[]`、`buffImmune[]`、`AddBuff`、`DelBuff`、`CheckLifeRegen`、`GetImmuneTime` | 状态效果、免疫、DOT、生命回复 | StatusEffects + NPC 专属抵抗/策略 | confirmed |
| 生命周期/消失 | `active`、`timeLeft`、`EncourageDespawn`、`DiscourageDespawn`、`CheckActive`、`checkDead` | 活动、自然消失、击杀、蠕虫段节和特殊终止 | NpcLifecycle；不要与 Player/Projectile 的 timeLeft 合并 | confirmed |
| 移动/重力/碰撞 | `UpdateCollision`、`Collision_MoveWhileDry/Wet`、`ApplyTileCollision`、`Collision_WalkDownSlopes`、`CheckDrowning` | 地形、坡面、液体、重力和传送 | Movement/Physics + NPC 环境策略 | confirmed |
| 变形/传送/附着物 | `Transform`、`TransformVisuals`、`Teleport`、`PopAllAttachedProjectilesAndTakeDamageForThem` | 形态变更、位置迁移、附着投射物 | Transform Command + Relationship/Projectile System | confirmed |
| 战利品/死亡 | `NPCLoot`、`DoDeathEvents`、`DropItemInstanced`、`GetItemSource_Loot`、`CountKillForBannersAndDropThem` | 掉落、Boss 事件、Banner、成就、死亡副作用 | Loot Rule/Resolver + Death Event + Item Spawn | confirmed |
| Bestiary/统计 | `IsNPCValidForBestiaryKillCredit`、`GetBestiaryCreditId`、击杀统计调用 | 图鉴解锁、击杀归因、统计 | Projection/Tracker，不应拥有 NPC 权威状态 | confirmed |
| Town/住房/交互 | `AI_007_TownEntities*`、`CheckDialogue`、`AnyInteractions`、`PlayerInteraction` | 城镇 NPC、住房、对话、玩家交互 | Town/Interaction System | confirmed |
| 网络复制 | `netUpdate`、`netSpam`、`UpdateNetworkCode`、`StreamUpdatesToNearbyPlayers`、`NetUpdate*` | 区段流式同步、节流、重同步 | NPC Replication Projection/Cursor | confirmed |
| 表现/动画/声音 | `FindFrame`、`UpdateAltTexture`、`IdleSounds`、`UpdateNPC_UpdateTrails`、`UpdateNPC_BuffApplyVFX` | 帧、拖尾、替换纹理、音效、Buff VFX | Presentation System | confirmed |
| 来源/关系适配 | `GetSpawnSource_*`、`GetItemSource_Loot`、`GetSpawnSource_ForProjectile` | 记录生成/伤害/掉落的来源链 | Source Adapter；关系字段需有类型和作用域 | confirmed |

### `NPC` 的拆分结论

`NPC.ai[]` 不是可直接共享的“通用 AI 组件”：不同 AI style 的状态转移、计时、目标和生命周期不一致。可以共享调度接口或纯策略，不共享无类型状态数组的所有权。

## 6. `Projectile` 子系统

证据主文件：[`Projectile.cs`](D:/TRbackup/Version4/Terraria/Projectile.cs)。关键基础字段集中在约 90-180 行；生成、伤害、更新、AI、网络和特殊行为横跨全文件。

| 子系统 | Version4 证据 | 主要职责 | 建议边界 | 状态 |
|---|---|---|---|---|
| 类型定义/默认值 | `type`、`SetDefaults`、`InitializeStaticThings`、`DefaultToWhip/Spear/Flail/Yoyo/Kite` | 类型能力、默认寿命、阵营、碰撞和特殊标志 | Projectile Definition/Archetype | confirmed |
| 生成与槽位 | `NewProjectile`、`GetNextSlot`、`FindOldestProjectile`、`ApplyStatsFromSource` | 槽位、Owner、来源、初始速度/AI | Spawn Command/System + typed Handle | confirmed |
| 归属与关系 | `owner`、`identity`、`GetByUUID`、`InheritSource`、`FindBannerToAssociateTo` | Owner、协议 identity、来源继承、Banner 关联 | Owner Relation + Protocol Identity Adapter | confirmed |
| 行为状态 | `ai[]`、`localAI[]`、`aiStyle`、`AI`、大量 `AI_*` | 每种投射物运行逻辑、计时和特殊状态 | ProjectileBehavior；按行为族分 System/Policy | confirmed |
| 额外更新/运动 | `extraUpdates` 相关字段、`HandleMovement`、`UpdatePosition`、`GetCollisionParams` | extra update、速度、液体运动、尺寸调整 | Movement System + Lifetime tick contract | confirmed/partial |
| 伤害结算 | `damage`、`originalDamage`、`knockBack`、`Damage`、`Damage_PVE/PVP` | PVE/PVP、目标迭代、命中特效 | ProjectileDamage + Combat Command | confirmed |
| 穿透/免疫 | `maxPenetrate`、`penetrate` 相关字段、`DecrementLocalImmuneTimeCounters`、`IsNPCIndexImmuneToProjectileType` | 命中额度、局部/按类型免疫、耗尽 | Penetration + HitImmunity 分离 | confirmed |
| Buff/命中特效 | `StatusNPC`、`StatusPvP`、`ApplyBuffTo`、`ApplyWhipDebuffs`、`TryDoingOnHitEffects` | 目标 Buff、粉末、鞭子、吸血/治疗 | Combat/Status Effect System | confirmed |
| 碰撞/切 Tile | `Colliding`、`CanCutTiles`、`CutTiles`、`ExplodeTiles`、`AI_197_HandleTileCollision` | 实体碰撞、切 Tile、爆炸和地形副作用 | Collision Policy/Result + Tile Command | confirmed |
| 寿命/销毁 | `active`、`timeLeft`、`Kill`、`ProjectileFixDesperation`、反射/销毁逻辑 | 到期、碰撞销毁、旧投射物回收、特殊保留 | ProjectileLifetime + EndReason | confirmed |
| 特殊实体能力 | `sentry`、`minion`、`bobber`、`counterweight`、`arrow`、`ownerHitCheck`、`reflected`、`stepSpeed` | 召唤物、哨兵、鱼漂、反射、链锯/钻头、计数器重物等 | 能力标签/专用组件；不要堆成神秘 bool 聚合 | confirmed |
| 网络同步 | `netUpdate`、`netUpdate2`、`netSpam`、`netSyncSkippedForPlayer`、`RecheckSectionsForSkippedUpdates` | 复制脏标记、节流、区段和跳过更新 | Projectile Network Projection/Cursor | confirmed |
| 表现/历史 | `rotation`、`gfxOffY`、`oldPos[]`、`ProjLight`、`UpdateEnchantmentVisuals`、颜色计算 | 轨迹、光照、动画、附魔视觉和命中特效 | Presentation/History Projection；暂不进入核心权威组件 | confirmed |

### `Projectile` 的关键结论

- `ProjectileDefinition` 只适合放生成时确定的静态能力；若继续增加 `IsTrap`、`IsBobber`、`IsCounterweight`、`IsSentry`、`IsMinion` 等 bool，会形成低可读性的“标志袋”。
- 更合适的是按能力拆为 `Trap`、`Bobber`、`Counterweight`、`Sentry`、`Minion` 等专用组件，或使用带语义的定义类型/能力集合；但是否迁移要以每个标志的读写者和生命周期为证据。
- `identity`、网络复制 ID、Owner、运行时槽位和实体 UUID 不应共用一个字段。
- `ProjectileDirection` 与公共水平 `Direction` 不应直接合并：前者表达轨迹向量/发射方向，后者表达稳定的基础朝向。

## 7. `Item` 子系统

证据主文件：[`Item.cs`](D:/TRbackup/Version4/Terraria/Item.cs)。其字段区约 18-337 行已经显示它同时承载定义、实例、装备、战斗、商店和表现数据；默认值方法从约 1,212 行延伸到 48,000 行以后。

| 子系统 | Version4 证据 | 主要职责 | 建议边界 | 状态 |
|---|---|---|---|---|
| 类型/定义 | `type`、`SetDefaults1-5`、`SetDefaults`、`ResetStats`、`netDefaults` | 类型基础属性、网络默认值、初始化 | ItemDefinition/Prototype | confirmed |
| 实例数量/堆叠 | `stack`、`maxStack`、`uniqueStack`、`CanStack`、`TurnToAir`、`Clone` | 实例数量、堆叠、克隆、清空 | Stackable Item + Item Instance lifecycle | confirmed |
| 前缀/变体 | `prefix`、`CanHavePrefixes`、`Prefix`、`CanRollPrefix`、`TryGetPrefixStatMultipliersForItem` | 前缀选择、属性倍率、重置 | Prefix Definition + Derived Stats | confirmed |
| 武器/攻击 | `damage`、`knockBack`、`crit`、`shoot`、`shootSpeed`、`DefaultToBow/MagicWeapon/RangedWeapon/ThrownWeapon`、`SetWeaponValues` | 武器能力、投射物发射、暴击和击退 | Weapon/ProjectileSpawn capability | confirmed |
| 工具/采集/放置 | `pick`、`axe`、`hammer`、`tileBoost`、`createTile`、`createWall`、`placeStyle`、`DefaultToPlaceableTile/Wall` | 采矿、砍伐、锤击、放置 Tile/墙 | Tool + Placement Definition | confirmed |
| 资源/消耗品 | `healLife`、`healMana`、`mana`、`potion`、`consumable`、`DefaultToFood/HealingPotion` | 生命/法力恢复、药水、食物、使用消耗 | Consumable/Resource Effect | confirmed |
| 弹药/钓鱼 | `ammo`、`useAmmo`、`bait`、`fishingPole`、`FitsAmmoSlot`、`CanFillEmptyAmmoSlot`、`DefaultToQuestFish` | 弹药匹配、鱼饵、钓鱼和任务鱼 | Ammo/Fishing capability | confirmed |
| 装备/饰品/时装 | `accessory`、`wornArmor`、`headSlot`、`bodySlot`、`legSlot`、`handOnSlot`、`wingSlot`、`dye`、`DefaultToHeadgear/Body/Legs/Accessory` | 装备槽、饰品、时装、染料 | Equipment Definition；槽位关系由 Player/Inventory 管理 | confirmed |
| Buff/宠物/坐骑 | `buffType`、`buffTime`、`mountType`、`DD2Summon`、`DefaultToVanitypet`、`DefaultToCapturedCritter` | 使用物品产生 Buff、宠物、坐骑或 NPC | Effect/Spawn Command，不放 Item 行为方法 | confirmed |
| 商店/经济 | `value`、`buy`、`isAShopItem`、`shopSpecialCurrency`、`shopCustomPrice`、`SetShopValues`、`buyPrice/sellPrice` | 买卖价格、商店资格、特殊货币 | Economy/Shop Definition + Transaction System | confirmed |
| 世界掉落/生成 | `NewItem`、`PickAnItemSlotToSpawnItemOn`、`CanPassivelyStackInWorld` | 生成 WorldItem、掉落槽位和世界堆叠 | Item Spawn Command + WorldItem entity | confirmed |
| 网络/兼容/安全 | `netDefaults`、`FixAgainstExploit`、`Refresh` | 网络字段恢复、反作弊修复、刷新 | Protocol Adapter + Validation | confirmed |
| Tooltip/表现 | `Name`、`RebuildTooltip`、`GetDrawHitbox`、`GetPhaseColor`、`UseSound`、`color`、`glowMask` | 名称、说明、颜色、声音、绘制 | Presentation Projection | confirmed |

### `Item` 的拆分结论

`ItemDefinition`、`Stackable`、`Weapon`、`Equipment`、`Consumable`、`Ammo` 等是不同变更原因和生命周期。共享的应是定义/策略接口，而不是把所有字段塞入一个“通用 ItemComponent”。

## 8. `Chest` 子系统

证据：[`Chest.cs`](D:/TRbackup/Version4/Terraria/Chest.cs:17)。相比 Player/NPC/Projectile，Chest 体量较小，但它仍混合了世界对象、库存容器、银行、商店、网络/存档和动画。

| 子系统 | Version4 证据 | 主要职责 | 建议边界 | 状态 |
|---|---|---|---|---|
| 世界对象身份 | `x`、`y`、`index`、`type` 间接由创建/Tile 绑定 | 世界坐标、数组索引、类型 | WorldObject identity + typed handle | confirmed |
| 容器内容 | `maxItems`、`item`、`Resize`、`FillWithEmptyInstances` | 槽位容量、物品内容、容量变更 | Container Contents；不要直接复用 Player Inventory 语义 | confirmed |
| 创建/注册/删除 | `CreateWorldChest`、`Assign`、`CreateOutOfArray`、`RemoveChest`、`CreateChest`、`DestroyChest` | 世界放置、注册表、删除和回收 | Placement/WorldObject Command | confirmed |
| 查找/占用/锁定 | `_chestsByCoords`、`_chestInUse`、`FindChest`、`UsingChest`、`IsLocked`、`GetCurrentlyOpenChests` | 坐标索引、玩家占用、锁定和打开状态 | Container Access/Interaction System | confirmed |
| Bank/Shop | `CreateBank`、`CreateShop`、`SetupShop`、`SetupTravelShop*`、`Main.shop` | 银行、普通商店、旅行商店库存与随机选择 | Bank/Shop Projection + Economy System | confirmed |
| 网络/存档兼容 | `NetMessage.SendChestContentsTo` 的调用链、`FixLoadedData` | 内容同步、加载后修复 | Chest Snapshot + Network/Persistence Adapter | confirmed/partial |
| 表现/动画 | `frameCounter`、`frame`、`eatingAnimationTime`、`UpdateChestFrames` | 箱子动画、打开/食用表现 | Presentation State | confirmed |

### `Chest` 的拆分结论

Chest 是“可容纳物品的世界对象”，不是 Player Inventory 的同义类型。可以共享 Container 的最小能力，但容量、槽位语义、银行/商店权限和世界坐标生命周期必须单独建模。

## 9. `Main` 子系统

证据：[`Main.cs`](D:/TRbackup/Version4/Terraria/Main.cs:84)。`Main` 不是普通实体，而是全局协调器和大量静态注册表的持有者。它是最需要通过 Adapter、Registry、WorldState 和主循环 System 拆开的类型。

| 子系统 | 关键证据 | 主要职责 | 建议边界 | 状态 |
|---|---|---|---|---|
| 实体注册表/数组 | `tile[,]`、`item[]`、`npc[]`、`projectile[]`、`chest[]`、`player[]`（约 928-982） | 旧数组槽位、遍历、索引复用 | Registry/Store；索引不是实体身份 | confirmed |
| 世界 Tile/地图 | `tile`、`Map`、`WorldSections`、`maxTilesX/Y`、区段 | 世界网格、地图、区段可见性和加载 | World/Tiles/Map 子系统 | confirmed |
| 时间/主循环 | `Update`、`DoUpdate`、`DoUpdateInWorld`、`UpdateTime`、`UpdateTimeRate`、`GameUpdateCount` | tick、暂停、时间速率、世界更新顺序 | Tick Scheduler + 显式 System order | confirmed |
| 世界天气/环境 | `dayTime`、`time`、`moonPhase`、`raining`、`windSpeed*`、`StartRain/StopRain`、`StartSlimeRain` | 日夜、月相、雨、风、史莱姆雨 | World Environment/Event Systems | confirmed |
| 事件/侵袭/Boss 状态 | `invasionType`、`invasionProgress*`、`StartInvasion`、`UpdateInvasion`、月事件方法 | 事件启动、进度、告警、结束条件 | Event/Invasion State + Systems | confirmed |
| 世界生成/加载 | `Initialize_AlmostEverything`、`Initialize_Entities`、`LoadWorlds`、`SetWorld`、`autoCreate`、`AutogenProgress` | 初始化、世界选择、生成、加载准备 | World Generation + Persistence Adapter | confirmed |
| 内容注册/数据库 | `ItemDropsDB`、`FishDropsDB`、`BestiaryDB`、`ItemDropSolver`、`PylonSystem`、`ShopHelper` | 内容数据库、掉落、鱼、图鉴、传送柱和商店 | Definition/Registry services | confirmed |
| Item/Projectile/NPC 静态缓存 | `projHostile[]`、`projHook[]`、`projPet[]`、Buff/Tile 属性数组、`npcFrameCount`、`projFrames` | 类型级能力表和绘制/行为缓存 | Definition registry/cache；不作为实体组件 | confirmed |
| 液体/光照/地图表现 | `liquid[]`、`liquidBuffer[]`、`liquidAlpha[]`、`Lighting` 相关状态 | 液体模拟、缓冲、透明度、光照与地图显示 | Liquid/Lighting/Map Presentation | confirmed |
| 网络/会话模式 | `netMode`、`menuMultiplayer`、`menuServer`、`SetNetPlayers`、`clientPlayer`、连接相关字段 | 客户端/服务器模式、会话、玩家数 | Session/Network Adapter | confirmed |
| 存档路径与回滚 | `SavePath`、`WorldPath`、`PlayerPath`、`WorldList`、`ActiveWorldFileData`、`WorldRollingBackupsCountToKeep` | 世界/玩家存档路径、云存档、回滚 | Persistence Service | confirmed |
| UI/输入/相机 | `MenuUI`、`InGameUI`、`mouseX/Y`、`mouseItem`、`Camera`、`screenPosition` | 菜单、鼠标、相机、UI 缩放 | Client Presentation/Input Adapter | confirmed |
| 渲染/粒子/音频 | `graphics`、`ParticleRenderer`、背景数组、`cloud[]`、音乐/环境字段 | 客户端渲染、粒子、背景、音频环境 | Presentation/Audio Systems | confirmed |
| 主线程与异步任务 | `DelayedProcesses`、`DelayedProcessesInGame`、`_mainThreadActions`、`QueueMainThreadAction` | 延迟任务、线程切换、资源生命周期 | Explicit Scheduler/Port；不能成为隐式全局业务状态 | confirmed |
| 配置/诊断/平台 | `LoadDedConfig`、`SetThreadExecutionState`、`LoadLibrary`、`verboseNetplay`、退出流程 | 服务器配置、平台调用、诊断与退出 | Platform/Diagnostics Adapter | confirmed |

### `Main` 的拆分结论

`Main` 同时拥有权威世界状态、实体存储、客户端表现、配置、网络会话和主循环。它不应被转化为“MainComponent”；应按 WorldState、EntityStore、Tick Scheduler、Session、Persistence、Content Registry、Presentation、Platform Adapter 分层。

## 10. 跨类型调用关系与系统顺序

Version4 的旧调用关系可以概括为：

```text
Main.Update / DoUpdate
  ├─ 时间、天气、世界事件
  ├─ Player.Update
  │    ├─ 输入/移动/碰撞
  │    ├─ Buff/资源/装备/ItemCheck
  │    └─ NPC/Projectile/Tile/Chest 交互
  ├─ NPC.UpdateNPC / AI / checkDead
  │    ├─ 目标、移动、战斗、Buff
  │    └─ 掉落、事件、网络
  ├─ Projectile.Update / AI / Damage / Kill
  │    ├─ Player/NPC/Tile 碰撞
  │    └─ 网络/光照/轨迹表现
  ├─ WorldItem / Chest / TileEntity 等世界对象
  └─ NetMessage / WorldFile / Presentation
```

建议的目标调度契约（这是设计建议，不是 Version4 已存在的 ECS 实现）：

1. 输入与外部命令接收；
2. Player/NPC/Projectile 行为系统产生意图或速度；
3. Movement/Physics/Tile/Liquid 计算并提交空间结果；
4. Combat/Status/Projectile collision 消费命中与伤害；
5. Player/NPC/Projectile 生命周期处理死亡、过期、销毁和清理；
6. Loot/WorldObject/Item spawn 处理结构变化；
7. Snapshot/Network/Save Projection 单向读取稳定状态；
8. Presentation/Audio/UI 使用投影，不回写权威模拟。

必须显式测试的先后关系包括：

- 行为产生速度后才能进行 Tile/Entity 碰撞；
- 命中与伤害结算后才能决定穿透耗尽和 Projectile 终止；
- Player/NPC 死亡状态改变后才能生成掉落或触发 Boss/事件副作用；
- 权威状态稳定后才能生成网络快照和存档投影；
- 网络/表现系统不得直接修改 Health、Location、Lifetime 等权威字段。

## 11. 对 NLTX `src` 组件模型的映射判定

| `src` 组件族 | Version4 证据 | 当前判定 |
|---|---|---|
| `LocationComponent` / `VelocityComponent` / `ColliderComponent` | `Entity.position/velocity/width/height`，[`Entity.cs`](D:/TRbackup/Version4/Terraria/Entity.cs:10) | 公共空间候选，语义最清楚 |
| `DirectionComponent` | `Entity.direction`、Player/NPC/Projectile 各自方向字段 | 只能共享稳定水平朝向；Projectile 轨迹和 NPC 垂直方向分域 |
| `HealthComponent` | Player `statLife/statLifeMax`（约 1357-1361）；NPC `life/lifeMax`（约 6341-6343） | Player/NPC 共享候选，但必须保留各自死亡/恢复语义 |
| `DefenseComponent` | Player `statDefense`；NPC `defense/defDefense` | 计算输入状态，可共享纯策略，不等于同一来源 |
| `ImmunityComponent` | Player `buffImmune`/免疫计时；NPC `immune[]`；Projectile 局部免疫 | 免疫键空间和生命周期不同，不能直接共用一个字段集 |
| `InputIntentComponent` | Player `control*`/`release*` | 服务器消费的是经验证的命令/意图，不能把客户端采样等同权威速度 |
| `Inventory/Hands/Equipment/Container` | Player inventory/armor/dye；Chest `item[]`/capacity | 能力相关但生命周期不同；容器能力可共享最小接口，库存布局不可泛化吞并 |
| `ProjectileDefinition/Behavior/Damage/Lifetime/Owner/Penetration` | Projectile 基础字段与 `AI`/`Damage`/`Kill`/`NewProjectile` | 拆分合理；网络 identity 与表现历史继续隔离 |
| `EntityIdentityComponent` | Version4 `whoAmI`、数组索引、Projectile `identity`、网络槽位 | 当前只能确认这些旧概念不等价；尚不能从 Version4 证明 `src` UUID 已接入 |
| `LiquidComponent` | Entity 液体字段；Main `liquid[]`/`liquidBuffer[]` | 实体接触状态与世界液体存储必须分开；公共组件语义仍需明确 |

## 12. 风险与未决证据

| 风险 | 当前证据 | 影响 | 下一步 |
|---|---|---|---|
| 巨型类型职责回流 | Player/NPC/Projectile/Item/Main 的方法和字段跨越多个域 | 机械拆文件后仍会循环依赖 | 先按读写者和生命周期建立 owner map |
| `Main` 全局数组被误当实体身份 | `Main.npc/projectile/player/chest` 和 `whoAmI` | 槽位复用、网络串 ID、重连错绑 | Registry + typed handle + protocol adapter |
| 公共组件与旧实现类型漂移 | `src`、旧 `dome`、Version4 类型名称并行 | 双写、命名空间混淆、迁移方向不明 | 选择唯一规范类型，保留单向兼容投影 |
| `timeLeft`/`active` 机械合并 | Player、NPC、Projectile 均有同名字段 | 终止原因、恢复路径和清理顺序丢失 | 三个独立生命周期组件/System |
| Projectile 神秘 bool 增长 | `sentry`、`minion`、`bobber`、`counterweight` 等 | 组合状态不可读、非法组合增加 | 按能力拆专用组件或语义定义类型 |
| 表现数据进入服务器核心 | `rotation`、`gfxOffY`、`oldPos`、动画/粒子字段 | 网络/模拟耦合、历史采样不一致 | Presentation Projection + 明确采样契约 |
| 组件缺少真实消费者 | `src` 多数领域组件只有定义；Version4 是旧 OOP 参考 | “组件已存在”被误报成“运行时已接入” | 为每个候选组件补创建/读/写/销毁/复制证据 |
| 反编译/占位方法影响语义确认 | Version4 个别方法体为空或简化 | 不能仅凭方法名确认完整算法 | 记录 partial，必要时查更完整原始版本 |

## 13. 报告结论与推荐顺序

### 已确认的第一层边界

- 公共空间：Location、Velocity、Collider，以及有限的水平 Direction。
- Player/NPC 战斗：Health、Defense、Immunity 分离；Player 与 NPC 的生命周期分离。
- Projectile：Definition、Behavior、Damage、Penetration、Owner、Lifetime、Network、Presentation 分离。
- Item：Definition、Stackable、Weapon、Equipment、Consumable、Ammo/Tool 等按能力分组。
- Chest：Container 内容与世界对象身份、银行/商店权限、交互和网络投影分离。
- Main：World、Entity Store、Tick、Session、Persistence、Content Registry、Presentation、Platform 分层。

### 不应立即实施的动作

- 不创建 `BaseEntityComponent`、`GenericLifetimeComponent`、`GenericNetworkComponent`。
- 不把 `Projectile.identity`、`whoAmI`、数组索引、网络复制 ID、持久化 ID、账户 ID 合并。
- 不把 Player/NPC/Projectile 的 AI 数组合并成 `AiComponent`。
- 不把表现历史、绘制偏移、旋转和网络脏标记直接放入共享权威组件。
- 不因为 `src` 中已经有类型定义，就宣称对应运行时已完成迁移。

### 推荐的证据补齐顺序

1. 对七个类型建立字段级 `Read By / Written By / Lifecycle / State Kind` 清单。
2. 先锁定 Version4 的创建、更新、销毁、网络、存档入口。
3. 以 `Entity` 的空间字段为第一批 focused verifier 对象。
4. 分别验证 Player、NPC、Projectile 生命周期，不合并 `active/timeLeft`。
5. 对 Item/Chest 建立容器、槽位、堆叠和商店权限的关系图。
6. 最后再决定 `src` 组件与未来实现的唯一规范类型及兼容策略。

## 14. 验证状态

- 已完成：Version4 七个核心文件的规模、字段族、方法族、外部引用和跨域入口只读盘点。
- 已完成：`src` 组件目录、拆分文档和 SS14 组件/System 组织模式的对照。
- 已完成：`dome` 作为旧实现的范围纠偏；本报告不使用它作为当前实现证据。
- 未执行：任何 C# 代码修改、编译、运行时行为迁移或 focused verifier。
- 因此，本报告是**子系统发现与拆分边界报告**，不是迁移完成报告，也不声称 Version4 已经 ECS 化。

## 15. 这些子系统共同组成什么系统

### 15.1 系统级结论

`Entity`、`Player`、`NPC`、`Projectile`、`Item`、`Chest`、`Main` 下的子系统共同组成的是：

> **以世界状态为中心、以实体交互为核心、由主循环驱动、支持内容定义、网络会话、存档恢复和客户端表现的 Terraria 类多人沙盒游戏系统。**

它不是单纯的“实体系统”，也不是单纯的“ECS 组件库”。从 Version4 的职责分布看，完整系统至少包含以下四个运行平面：

```text
                         ┌─────────────────────────┐
                         │ 客户端表现平面           │
                         │ UI / 输入 / 相机 / 渲染   │
                         │ 音频 / 动画 / 地图显示     │
                         └───────────┬─────────────┘
                                     │ 单向读取投影
┌─────────────────────────┐          v          ┌─────────────────────────┐
│ 外部边界与基础设施       │  ─── Commands ───>  │ 权威游戏模拟平面         │
│ 网络 / 会话 / 存档        │  <── Snapshots ───  │ 世界 / 实体 / 战斗 / AI   │
│ 配置 / 平台 / 诊断        │                    │ 移动 / 碰撞 / 生命周期    │
└─────────────────────────┘                    └───────────┬─────────────┘
                                                           │
                                                           v
                                      ┌──────────────────────────────────┐
                                      │ 内容与规则平面                    │
                                      │ Item/NPC/Projectile 定义          │
                                      │ 掉落 / 事件 / 世界生成 / Tile 规则 │
                                      └──────────────────────────────────┘
```

### 15.2 四个运行平面

#### A. 权威游戏模拟系统

这是整个游戏的核心。它回答“世界现在是什么状态、实体下一 tick 应该怎么变化”：

- **世界状态**：Tile、液体、时间、天气、月相、事件、侵袭和世界生成结果。
- **实体状态**：Player、NPC、Projectile、WorldItem、Chest 的位置、速度、碰撞几何、生命、关系和生命周期。
- **行为系统**：Player 输入、NPC AI、Projectile AI、物品使用、目标选择、移动和碰撞。
- **交互与战斗**：Player/NPC/Projectile 命中、伤害、免疫、Buff、穿透、掉落和物品生成。
- **生命周期**：生成、活动、死亡、复活、自然消失、投射物过期、容器创建和删除。

`Entity` 提供公共空间语义；`Player`、`NPC`、`Projectile` 是主要动态实体；`Item` 和 `Chest` 是与实体和世界交互的内容/容器对象；`Main` 负责把这些状态放进主循环和旧式注册表中。

#### B. 内容与规则系统

它回答“某个类型具有什么能力、在什么条件下发生什么”：

- Item/NPC/Projectile 的类型定义、默认属性、前缀、装备、武器和特殊能力；
- NPC 生成条件、AI style、Boss/invasion 规则；
- Projectile 的行为、伤害、穿透、鱼漂、哨兵、召唤物、陷阱等能力；
- ItemDropDatabase、FishDropRules、Bestiary、Creative、Biome、WorldGen 等规则数据库或规则集合；
- Tile、液体、碰撞、放置和交互资格判断。

内容系统应主要提供 Definition、Policy、Query 和规则数据；它不应直接拥有所有动态实体状态。

#### C. 外部边界与基础设施系统

它回答“如何与游戏外部世界交换状态”：

- 网络连接、消息缓冲、协议包、玩家/NPC/Projectile 同步；
- Player/World 文件、云存档、回滚、版本兼容和加载修复；
- 服务器配置、客户端/服务器模式、平台 API、主线程队列和诊断；
- 内容注册表、ID 表、静态缓存和资源加载。

这层应通过 Adapter、Snapshot、Command 和 Registry 与权威模拟交互。网络 ID、数组索引、存档 ID、账户 ID 和运行时实体句柄不能互相替代。

#### D. 客户端表现系统

它回答“如何把当前状态呈现给玩家”：

- Player/NPC/Projectile 的动画帧、旋转、拖尾、光照、旧位置和命中特效；
- Item/Chest 的 Tooltip、颜色、声音、打开动画和绘制盒；
- UI、地图、相机、背景、粒子、音乐、天气视觉和场景指标。

表现层应读取权威状态或快照投影，不能反向写入 Health、Location、Lifetime 等核心状态。`rotation`、`gfxOffY`、`oldPos` 等字段需要先证明采样和同步契约，不能因为它们在多个类中同名就并入公共模拟组件。

### 15.3 组成后的数据流

```text
输入 / 网络命令 / 世界生成配置
              │
              v
    ┌──────────────────┐
    │ 命令验证与意图层  │  Player input / interaction / spawn request
    └────────┬─────────┘
             v
    ┌──────────────────┐
    │ 行为与规则层      │  Player control / NPC AI / Projectile AI
    └────────┬─────────┘
             v
    ┌──────────────────┐
    │ 空间与接触层      │  Movement / Collision / Tile / Liquid
    └────────┬─────────┘
             v
    ┌──────────────────┐
    │ 战斗与效果层      │  Damage / Defense / Immunity / Buff / Penetration
    └────────┬─────────┘
             v
    ┌──────────────────┐
    │ 生命周期与结构层  │  Death / Respawn / Despawn / Expiry / Loot / Spawn
    └────────┬─────────┘
             ├──────────────> Snapshot / Network projection
             ├──────────────> Persistence projection
             └──────────────> Presentation projection
```

这条数据流说明了为什么不能按旧类中的字段顺序机械拆文件：一个 Player 的 `control`、`velocity`、碰撞、伤害、死亡、掉落和网络同步属于不同阶段；一个 Projectile 的 `damage`、`timeLeft`、`identity` 和 `oldPos` 也属于不同状态所有权。

### 15.4 从系统性质看，它是什么类型的游戏系统

从功能组合看，它同时具备：

1. **实时离散 tick 模拟系统**：`Main.Update` 驱动时间、实体、世界和事件更新。
2. **实体交互系统**：Player、NPC、Projectile、Item、Chest 通过空间、关系、命令和事件互相影响。
3. **规则驱动的内容系统**：Item/NPC/Projectile 类型定义与掉落、生成、事件、世界生成规则决定行为差异。
4. **持久世界系统**：世界 Tile、Chest、事件进度、Player 数据和回滚需要存档与恢复。
5. **多人网络系统**：连接、消息缓冲、实体同步、区段更新、复制节流和客户端快照。
6. **客户端表现系统**：UI、相机、地图、粒子、动画、音频和视觉历史将模拟状态呈现出来。

因此，最准确的系统级命名不是“Entity ECS 系统”，而是：

> **Terraria 风格的多人实时沙盒游戏运行时（World Simulation + Entity Interaction + Content Rules + Networking/Persistence + Presentation）。**

### 15.5 对 NLTX 架构的含义

如果 NLTX 的目标是逐步 ECS 化 Version4，目标应是把上述系统拆成明确的协作边界，而不是把七个旧类逐一替换成七个组件：

```text
World State / Tile / Liquid / Time / Events
        │
        ├── Entity Store + typed handles
        │       ├── Shared spatial components
        │       ├── Player domain components
        │       ├── NPC domain components
        │       ├── Projectile domain components
        │       └── Item / Chest world-object components
        │
        ├── Systems: Input / AI / Movement / Physics / Combat / Status / Lifecycle
        ├── Commands and Events: Spawn / Damage / Use / Interact / Despawn / Loot
        ├── Queries: Geometry / Targeting / Spawn eligibility / Inventory space
        ├── Projections: Network / Persistence / Bestiary / UI / Presentation
        └── Adapters: Legacy Terraria API / protocol / file / platform
```

这意味着 `src` 中的组件只是未来权威模拟系统的状态列；完整系统还需要真正的 Entity Store、创建/销毁命令、System 调度、Query、事件、快照和 Adapter。缺少这些边界时，组件文件本身不能被称作完整游戏系统。
