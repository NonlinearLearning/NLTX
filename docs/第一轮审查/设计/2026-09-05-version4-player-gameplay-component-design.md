# PlayerGameplay Component Design

## 1. 设计元数据

subsystemId: PlayerGameplay
taskNumber: 11
sourceReport: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-player-gameplay-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-player-gameplay-component-design.md
designScope: component-only
designStatus: decision-required
evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
selectionMethod: current-session explicit research report; no research-directory scan

本文件的输入报告由当前会话明确提供，文件名以 -public-decomposition.md 结尾，因此输出路径由固定规则推导为本文件路径。没有读取或选择其他研究报告。

designStatus 为 decision-required，是因为 Version4 能确认大量玩家字段和生命周期入口，但本会话不能单独确认生命/死亡事实、物品实例引用、坐骑/钓鱼能力以及实体/持久化/会话标识的最终跨子系统 owner。

## 2. 设计范围与排除范围

本设计只描述挂在一个玩家实体上的状态 Component：字段、默认值、不变量、状态分类、生命周期、实体范围、组合和证据。所有名称、路径和新增值类型都是设计层名称，不表示源文件已经创建。

范围内的玩家状态：

- 玩家实体身份与兼容槽位关系；
- 输入意图的帧内快照；
- 活跃、死亡、重生和观察状态；
- 生命、法力和玩家本地资源；
- 装备槽、背包槽、银行和选中槽位；
- 物品使用计时和玩家本地使用状态；
- Buff 槽、恢复/免疫状态、召唤容量和玩家能力容量；
- 出生点/返回路线、坐骑、休息和钓鱼资格等可选玩家状态。

以下内容不在本文件中定义最终 owner：伤害计算、死亡惩罚和掉落事务、Item 实例与容器事务、位置/速度/碰撞、Projectile 实例与 owner、坐骑实体运动、浮标和 catch 事务、传送位置提交、存档 I/O、网络 session/slot 和客户端表现。它们只在字段归属或证据表中作为边界事实出现。

本文件不把共享的 LocationComponent、PlayerHandle、EntityUuid、网络标识或持久化标识重新吸收为 PlayerGameplay 私有定义。共享类型均保留 crossSubsystemOwner: integration-review。

## 3. 组件设计依据

### 3.1 来源优先级与直接证据

| 来源 | 实际读取内容 | 本设计用途 | evidenceStatus |
|---|---|---|---|
| D:\TRbackup\Version4\Terraria\Player.cs | Player 声明、字段、Update(int)、ResetEffects、装备/状态更新入口、ItemCheckWrapped、ItemCheck、Spawn、Hurt、KillMe、掉落和保存入口 | Version4 权威字段、默认值线索、生命周期和写入事实 | confirmed / partial |
| D:\TRbackup\Version4\Terraria\Entity.cs | Entity.whoAmI、位置、速度和方向字段 | 兼容索引与实体实例字段分离 | confirmed |
| D:\TRbackup\Version4\Terraria\Main.cs | Main 玩家数组和 11422-11442 的 active 玩家更新循环 | 玩家实体范围和 Tick 触发事实 | confirmed |
| D:\TRbackup\Version4\Terraria\MessageBuffer.cs | case 13 输入/位置/速度/坐骑/返回路线写入、case 16 生命写入、玩家连接状态写入 | 入站字段的写入事实；不把网络字段当作核心身份 | confirmed |
| D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs | Player、Name、ServerSideCharacter、LastPlayed、play timer | 玩家文件持有关系、保存时间和会话统计边界 | confirmed / partial |
| D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | world load/save、I/O lock、错误状态和临时世界状态 | 世界范围与玩家文件范围不混合 | confirmed |
| D:\TRbackup\tmodloader-api-docs-stable\class_mod_player.html | stable v2026.07 的 Initialize、ResetEffects、PreUpdate、SetControls、PreUpdateMovement、PostUpdateEquips、PreKill、SaveData、SendClientChanges | 公开生命周期和扩展边界交叉验证，不替代 Version4 私有实现 | partial |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Inventory\InventoryComponent.cs | 模板、槽位、容器和位移数据 | 只参考 Component 内聚和可选槽位粒度 | partial |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Movement\Components\InputMoverComponent.cs | 输入时间、当前 Tick 输入、方向和相对实体字段 | 只参考输入快照与移动状态分离 | partial |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Movement\Components\ActiveInputMoverComponent.cs | 独立 active marker 与输入组件分离 | 只参考可选状态和缓存不与权威输入混合 | partial |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\GameTicking\PlayerSpawnCompleteEvent.cs | 玩家 session 与 spawned entity 的分离字段 | 只参考玩家会话身份与实体身份分离 | partial |
| D:\TRbackup\NLTX\CONTEXT.md | EntityUuid、PlayerHandle、whoAmI、LocationComponent、DirectionKind 术语 | 当前 NLTX 身份和共享值类型约束 | confirmed |

### 3.2 Version4 重新定位的关键事实

- D:\TRbackup\Version4\Terraria\Player.cs:43 声明 Player : Entity, IFixLoadedData；D:\TRbackup\Version4\Terraria\Entity.cs:6-20 说明 whoAmI、位置、速度和方向首先属于实体/兼容层事实，而不是持久化身份。
- D:\TRbackup\Version4\Terraria\Main.cs:11422-11431 遍历 255 个玩家槽位，只对 active 玩家调用 player[i].Update(i)。这确认组件的主要范围是单个玩家实体，但不确认新的运行时结构。
- D:\TRbackup\Version4\Terraria\MessageBuffer.cs:652-738 直接写入输入、方向、位置、速度、坐骑、返回路线、休息和物品使用相关字段；MessageBuffer.cs:771-787 直接写入生命和 dead。这证明现有写入点，不证明其应成为核心组件的最终 owner。
- Player.cs:1013-1097 直接声明装备、Buff、背包、银行和虚空仓库相关字段；Player.cs:1134-1148 声明死亡、死亡计时、重生计时、观察槽位和保存时间；Player.cs:1219-1284 声明输入和使用意图；Player.cs:1355-1381 声明生命/法力/恢复；Player.cs:1552 声明 mount；Player.cs:1895-1901 声明出生点和返回路线。
- Player.cs:14789 是玩家更新入口；Player.cs:10272、10804、10847、11239、6865、9547 分别提供效果重置、免疫、生命恢复、法力恢复、装备和套装字段写入的事实入口。本文件只把这些作为成员证据。
- Player.cs:19603-19641 的 ItemCheckWrapped 调用 ItemCheck；Player.cs:21798 的 Spawn 写入出生后的生命、免疫、dead 和 active；Player.cs:22208 的 Hurt 读取免疫和生命；Player.cs:22572-22681 的 KillMe 写入死亡事实、死亡计数、死亡位置、重生计时并触发掉落边界。
- Player.cs:26261-26345 的 DropCoins/DropItems 遍历玩家背包和装备；Player.cs:26394-26418 的 SavePlayer 接受 PlayerFileData，而 Version4 当前 InternalSavePlayerFile/Serialize 在该文件中为空实现。具体序列化字段顺序因此只能标记 partial。
- D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs:10-38 持有 Player、派生 Name、ServerSideCharacter 和 LastPlayed；52-100 管理 play timer。D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:658-811 与 878-935 分别证明世界加载和保存的世界范围边界，不应并入玩家 Component。

### 3.3 外部资料的限制

本地 tModLoader 首页页眉为 tModLoader v2026.07。已核对的实际页面和成员锚点包括：

- class_mod_player.html#a68a5502a00c3e3c8f8f79c316637bcd4：Initialize；
- class_mod_player.html#a4aa636ee49c91545612dafb6c4e15a99：ResetEffects；
- class_mod_player.html#a4e7ec0fe894253b5bef5630be67ed42f：PreUpdate；
- class_mod_player.html#a981918672130f6284403f1468e88fc9a：PreKill；
- class_mod_player.html#aeaf390061d5840291f92c6fd929dc599：SaveData；
- class_mod_player.html#a7893e4b2639c4bf7d6e8872ce346fd38：SendClientChanges；
- class_mod_player.html#ac1a398dd8b8feb257c94efa5680b1b76：PostUpdateEquips；
- class_mod_player.html#a5293087b6fee40f0c032ef901800ed69：PostUpdateRunSpeeds；
- class_mod_player.html#a6f1888c345019158a7c25485b0208b26：ModifyFishingAttempt；
- class_mod_player.html#a53501a140db66e497fad2f3d3920d437：PreItemCheck。

这些页面只交叉验证公开生命周期、装备效果、物品使用和同步边界，不能覆盖 Version4 私有算法、保存格式或网络写入权限。

Space Station 14 没有提供可直接对应 Terraria 玩家行为的证据；以下边界仅由 Version4 真实代码、当前 NLTX 状态和项目约束决定。SS14 读取的 InventoryComponent、InputMoverComponent、ActiveInputMoverComponent 和 PlayerSpawnCompleteEvent 仅用于参考 Component 粒度、输入快照与实体/session 分离，不复制其代码、命名、目录结构或领域语义。

## 4. Version4 成员到 Component 归属表

下表中的 proposed Component 是目标设计名，不表示当前 Version4 或当前 NLTX 已拥有该 Component。方法行只作为状态写集、生命周期或边界证据，未转换为本文件中的运行时结构。

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| whoAmI | Entity | 玩家数组/协议兼容索引 | 兼容 | 实例创建到槽位释放 | PlayerIdentityComponent (status: proposed) | confirmed | D:\TRbackup\Version4\Terraria\Entity.cs:6-20 |
| active | Player | 玩家实例是否在当前世界有效 | 权威 | 创建、进入世界、离开世界、清理 | PlayerLifecycleComponent (status: proposed) | confirmed | Player.cs:478；Main.cs:11422-11431；MessageBuffer.cs:745-765 |
| name | Player | 角色显示名/存档名 | 权威、持久化候选 | 创建、加载、保存 | PlayerIdentityComponent (status: proposed) | confirmed | Player.cs:505；PlayerFileData.cs:20-38 |
| team、host、difficulty | Player | 队伍、主机角色和角色难度 | 权威；session 相关字段跨域 | 连接、加载、Tick、保存 | PlayerIdentityComponent (status: proposed) | partial | Player.cs:478、480、978、1154 |
| controlLeft、controlRight、controlUp、controlDown、controlJump、controlUseItem | Player | 当前输入按钮 | 快照/意图 | 网络输入到当前状态消费 | InputIntentComponent (status: proposed) | confirmed | Player.cs:1219-1229；MessageBuffer.cs:665-670 |
| controlUseTile、controlTorch、controlDash、controlDownHold、autoReuseAllWeapons | Player | 交互、辅助和持续输入 | 快照/意图 | 当前输入帧 | InputIntentComponent (status: proposed) | confirmed | Player.cs:1231-1241、1270-1276；MessageBuffer.cs:726-729 |
| releaseJump、releaseUp、releaseUseItem、releaseUseTile、releaseLeft、releaseRight、releaseDown、releaseDash | Player | 按键释放边沿 | 快照/意图 | 当前输入帧，消费后失效 | InputIntentComponent (status: proposed) | confirmed | Player.cs:1245-1264 |
| direction、gravDir、selectedItemState | Player | 朝向、重力方向和选中槽请求 | 意图/兼容快照；方向需整合 | 网络输入到实体状态 | InputIntentComponent (status: proposed) | partial | Player.cs:1389、2960；MessageBuffer.cs:671-682、685 |
| dead、deadTime、respawnTimer、spectating | Player | 死亡和观察生命周期 | 权威 | 死亡、等待重生、观察、重生/清理 | PlayerLifecycleComponent (status: proposed) | confirmed | Player.cs:1134-1146；KillMe:22572-22651；Spawn:21798-21851 |
| numberOfDeathsPVE、numberOfDeathsPVP、lastDeathPostion、lastDeathTime、showLastDeath | Player | 死亡统计和最后死亡快照 | 权威/快照；时间字段持久化候选 | 死亡写入，加载/保存可选 | PlayerLifecycleComponent (status: proposed) | confirmed | Player.cs:507-523；KillMe:22593-22603 |
| statLife、statLifeMax、statLifeMax2 | Player | 当前生命、基础最大生命和有效最大生命 | 权威/派生 | 创建、装备效果、伤害、重生 | PlayerVitalComponent (status: proposed) | confirmed | Player.cs:1357-1362；Hurt:22208；Spawn:21828-21848 |
| statMana、statManaMax、statManaMax2、statDefense | Player | 当前法力、法力上限和防御 | 权威/派生；防御 owner 跨域 | 创建、装备效果、消耗/伤害、重生 | PlayerVitalComponent (status: proposed) | confirmed / unresolved | Player.cs:1355-1367；Hurt:22254-22263 |
| lifeRegen、lifeRegenCount、lifeRegenTime、manaRegen、manaRegenCount、manaRegenDelay、manaRegenBuff | Player | 恢复累加器与恢复资格 | 权威临时/派生 | 每个玩家更新边界重置或累加 | PlayerRegenerationAndImmunityComponent (status: proposed) | confirmed | Player.cs:1369-1381；UpdateLifeRegen:10847；UpdateManaRegen:11239 |
| immune、immuneNoBlink、immuneTime、hurtCooldowns | Player | 免疫窗口及冷却数组 | 权威临时；伤害 owner 跨域 | 受伤、免疫递减、重生清理 | PlayerRegenerationAndImmunityComponent (status: proposed) | confirmed | Player.cs:968-972、2475；UpdateImmunity:10804；Hurt:22221-22225 |
| armor、dye、miscEquips、miscDyes、hideVisibleAccessory | Player | 装备、染料、杂项槽和隐藏标记 | 权威；Item 引用跨域 | 创建、加载、装备变更、死亡掉落、保存 | PlayerEquipmentComponent (status: proposed) | confirmed | Player.cs:1013-1021、1208-1210；UpdateEquips:6865；DropItems:26304-26345 |
| inventory、inventoryChestStack、bank、bank2、bank3、bank4、voidVaultInfo、trashItem | Player | 主背包、银行、堆叠资格、虚空仓库和垃圾槽 | 权威；容器/Item 实例跨域 | 创建、加载、使用/消费、死亡掉落、保存 | PlayerInventoryComponent (status: proposed) | confirmed | Player.cs:1083-1097；DropCoins:26261-26302；SavePlayer:26394-26418 |
| selectedItemState、selectedItem、changeItem、pendingItemReuse | Player | 选中槽、切换请求和待重用状态 | 权威意图/兼容快照 | 输入、使用、槽位变更 | PlayerInventoryComponent 与 PlayerUseComponent (status: proposed)；边界未决 | partial | Player.cs:1005-1009、2960；MessageBuffer.cs:685 |
| buffType、buffTime、buffImmune | Player | Buff 槽、剩余时间和免疫类型 | 权威/派生免疫 | 加载、获得、递减、死亡/重生清理 | PlayerBuffComponent (status: proposed) | confirmed | Player.cs:1027-1035；UpdateBuffs:4300；ResetEffects:10272 |
| itemAnimation、itemAnimationMax、itemTime、itemTimeMax、toolTime | Player | 物品使用与工具计时 | 权威临时 | 使用开始、逐 Tick、完成/取消 | PlayerUseComponent (status: proposed) | confirmed | Player.cs:2393-2401；ItemCheckWrapped:19603-19641；ItemCheck:23435 |
| channel、altFunctionUse、lastItemUseAttemptSuccess、delayUseItem、heldProj | Player | 持续使用、备用用法、结果和持有投射物索引 | 权威意图/兼容关系 | 输入、使用、投射物关联、完成/取消 | PlayerUseComponent (status: proposed) | confirmed / partial | Player.cs:1274-1292、1318、2393-2401；MessageBuffer.cs:731 |
| maxMinions、numMinions、slotsMinions、maxTurrets、maxTurretsOld | Player | 召唤容量、已用容量和炮台容量 | 派生缓存/容量事实 | 装备与 Buff 效果重算、召唤实体变化 | PlayerAbilityComponent (status: proposed) | confirmed | Player.cs:832-836、2244-2246；UpdateMaxTurrets:25840；ResetEffects:10024、10446 |
| fishingSkill、cratePotion、sonarPotion、accFishingLine、accFishingBobber、accTackleBox、accLavaFishing | Player | 钓鱼资格和装备派生能力 | 派生资格；catch owner 跨域 | 装备/Buff 重算、钓鱼尝试、清理 | PlayerFishingCapabilityComponent (status: proposed) | confirmed / partial | Player.cs:818-830；UpdateEquips:6865；tModLoader class_mod_player.html#a6f1888c345019158a7c25485b0208b26 |
| SpawnX、SpawnY、PotionOfReturnOriginalUsePosition、PotionOfReturnHomePosition | Player | 个人出生点和返回路线 | 权威/持久化候选；传送提交跨域 | 加载、设置出生点、使用返回物品、重生清理 | PlayerSpawnPointComponent (status: proposed) | confirmed | Player.cs:1895-1901；MessageBuffer.cs:708-716；Spawn:21854-21860 |
| mount、cMount、MountFishronSpecialCounter | Player | 坐骑实例/类型关系和玩家侧计时 | 关系/临时；坐骑 owner 跨域 | 装备/Buff、attach/detach、死亡/重生 | PlayerMountComponent (status: proposed) | confirmed / unresolved | Player.cs:1552、2346、2384；MessageBuffer.cs:700-707 |
| sitting、sleeping | Player | 休息/睡眠辅助状态 | 权威临时；空间/世界边界跨域 | 进入、更新、离开、死亡/重生 | PlayerRestComponent (status: proposed) | confirmed / unresolved | Player.cs:2286-2288；MessageBuffer.cs:720-725；Main.cs:11431-11438 |
| Update(int) | Player | 玩家 Tick 中的多域字段写集 | 行为入口证据，不是 Component | 活跃玩家每 Tick | 多个 proposed Component | confirmed | Player.cs:14789；Main.cs:11422-11431 |
| ResetEffects、UpdateImmunity、UpdateLifeRegen、UpdateManaRegen | Player | 派生效果、免疫和恢复字段的入口 | 行为入口证据，不是 Component | 玩家更新边界或受伤/重生边界 | PlayerVitalComponent、PlayerBuffComponent、PlayerRegenerationAndImmunityComponent (status: proposed) | confirmed | Player.cs:10272、10804、10847、11239 |
| UpdateEquips、UpdateArmorSets | Player | 装备和套装效果对派生字段的写入 | 行为入口证据，不是 Component | 装备效果重算 | PlayerEquipmentComponent、PlayerAbilityComponent 等 (status: proposed) | confirmed | Player.cs:6865、9547 |
| ItemCheckWrapped、ItemCheck | Player | 物品使用资格、计时和跨域结果入口 | 行为入口证据，不是 Component | 输入到使用完成/取消 | PlayerUseComponent、PlayerInventoryComponent (status: proposed) | confirmed | Player.cs:19603-19641、23435 |
| Spawn、Hurt、KillMe | Player | 重生、受伤和死亡状态变更入口 | 行为入口证据；死亡/伤害 owner 未决 | 边界到状态提交 | PlayerLifecycleComponent、PlayerVitalComponent (status: proposed) | confirmed / unresolved | Player.cs:21798、22208、22572 |
| DropCoins、DropItems、SavePlayer | Player | 掉落和保存边界 | 外部边界证据；不把事务吞入玩家 Component | 死亡掉落、显式保存 | PlayerInventoryComponent、PlayerLifecycleComponent (status: proposed) | confirmed / partial | Player.cs:26261-26345、26394-26418 |

## 5. Component 定义

以下 Component 均是 status: proposed。proposedPath 只表示按领域优先的候选文件位置，不表示该文件已存在。小型 PlayerGameplay 领域保持平面路径；没有创建 Shared/Components、Common 或 Misc 聚合目录。

### 5.1 PlayerIdentityComponent

#### 职责

保存玩家实体实例与玩家持久身份之间的最小身份关系，以及角色名、难度和队伍等玩家身份事实。不保存网络包对象、账户连接对象或整个玩家实体字段。

componentId: PG-COMP-01
name: PlayerIdentityComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerIdentityComponent.cs (status: proposed)
componentOwner: PlayerGameplay (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: entity creation -> identity load/bind -> active session -> disconnect/unbind -> entity destruction

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| entityId | EntityUuid (status: proposed; crossSubsystemOwner: integration-review) | 零值表示未分配 | 权威实体实例身份 | 分配后非零；只绑定当前实体实例；销毁后不可复用为当前实例 | partial | NLTX CONTEXT.md:7-23,71-74；Version4 只证明 Player : Entity |
| persistentPlayerId | PersistentPlayerId (status: proposed; crossSubsystemOwner: integration-review) | 未确认 | 权威持久化身份候选 | 不随重生实体实例改变；不能与 entityId 或槽位互换 | missing | PlayerFileData.cs:10-38 未确认此 ID 字段 |
| characterName | string | 空字符串 | 权威、持久化候选 | 名称格式和唯一性沿用 Version4；空值不构成身份 | confirmed | Player.cs:505 |
| teamId | int | 0（CLR 默认；语义初始化仍需确认） | 权威 | 只表示队伍，不承担账户或网络身份 | partial | Player.cs:978 |
| difficulty | byte；PlayerDifficulty (status: proposed) 为受控领域映射 | 0（数值到语义映射未锁定） | 权威、持久化候选 | 只能取 Version4 支持的值；死亡掉落规则不能由字段自行推断 | partial | Player.cs:1154；KillMe:22662-22673 |
| isHost | bool | false | 权威 session 角色快照 | 不得作为实体身份或持久化主键 | partial | Player.cs:480 |
| legacyPlayerSlot | LegacyPlayerSlot? (status: proposed; crossSubsystemOwner: integration-review) | null；Version4 兼容哨兵为 -1 | 兼容字段/typed projection | 只在数组/协议作用域内有效；槽位复用不能自动解析新实体 | confirmed / partial | Entity.cs:8；MessageBuffer.cs:222-228,659-660；CONTEXT.md:19-26 |

#### 字段不变量

- entityId、persistentPlayerId、legacyPlayerSlot 必须明确区分。
- legacyPlayerSlot 只允许作为兼容索引映射存在，不进入持久化身份比较。
- PlayerFileData.Player 的对象引用只是 Version4 文件边界关系，不证明 PlayerFileData 应附着到实体。

#### 生命周期

实体创建时分配 entityId；加载或连接时尝试绑定持久化身份和兼容槽位；断开或实体销毁时清理当前实例映射。持久化身份未确认前不得以空值猜测或复用 entityId。

#### Entity/World 范围

只属于一个玩家实体。世界名称、世界 ID、世界文件锁和 WorldFile 临时状态不属于本 Component。

#### ID 与关系字段

EntityUuid 是服务器权威实体实例身份；PersistentPlayerId 是跨存档/会话候选身份；LegacyPlayerSlot 是数组/网络兼容索引。PlayerHandle 是当前 NLTX 的 typed projection/handle，不能成为本 Component 的权威主键。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerIdentityState.cs 为 partial；dome\src\Terraria.Dome.Simulation\Player\Components\PlayerIdentityComponent.cs 也为 partial。现有文件包含角色名、队伍、难度、连接或 slot 等局部模型，但不能证明 EntityUuid、持久化 ID 和 session ID 已完整分离。

#### 证据

主要证据为 Player.cs:43,478,480,505,978,1154、Entity.cs:6-20、MessageBuffer.cs:222-228,745-765、PlayerFileData.cs:10-38 和 CONTEXT.md:7-26。持久化账户 UUID 是 EVIDENCE-GAP-PG-01。

### 5.2 InputIntentComponent

#### 职责

保存一个玩家输入帧的意图快照。不保存位置、速度、碰撞结果或 Item 实例；方向字段只作为已验证的输入/兼容边界，公共方向类型的最终 owner 仍需整合。

componentId: PG-COMP-02
name: InputIntentComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\InputIntentComponent.cs (status: proposed)
componentOwner: PlayerGameplay (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: create with player entity -> replace from accepted input snapshot -> expire/replace at input boundary -> clear on disconnect or destruction

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| moveLeft、moveRight、moveUp、moveDown | bool | false | 快照/意图 | 只表示当前输入帧，不能等同于移动结果 | confirmed | Player.cs:1219-1225；MessageBuffer.cs:665-668 |
| jump、useItem、useTile | bool | false | 快照/意图 | 由输入边界写入；消费后不延长生命周期 | confirmed | Player.cs:1227-1231；MessageBuffer.cs:669-670,728-729 |
| controlTorch、controlDash、controlDownHold | bool | false | 快照/意图 | 不携带外部协议位图本身 | confirmed | Player.cs:1235-1241,1270；MessageBuffer.cs:727-729 |
| releaseJump、releaseUp、releaseUseItem、releaseUseTile、releaseLeft、releaseRight、releaseDown、releaseDash | bool | false | 快照/意图 | 只描述边沿，不得解释为持续按键 | confirmed | Player.cs:1245-1264 |
| alternateUseMode | int；ItemUseMode (status: proposed) 为领域映射 | 0 | 快照/意图 | 无效值不得进入权威使用状态 | confirmed / partial | Player.cs:1284；PlayerUseState 当前映射 |
| requestedDirection | DirectionKind (status: proposed; crossSubsystemOwner: integration-review) | 未锁定 | 兼容快照/意图 | 不使用裸 int 作为公共契约；不能改写 LocationComponent | partial | MessageBuffer.cs:671；CONTEXT.md:45-48 |
| issuedAtTick | SimulationTick? (status: proposed) | null | 快照 | 只用于输入新旧判断，不作为持久化时间 | partial | src\Player\InputIntentComponent.cs:19-32 |
| sequence | uint | 0 | 兼容/快照 | 序号回退或重复不能使旧意图覆盖新意图 | partial | src\Player\InputIntentComponent.cs:24-32 |
| source | InputIntentSource (status: partial) | None | 兼容/快照 | 只标识输入来源，不承担 session 身份 | partial | src\Player\InputIntentComponent.cs:21,32 |

#### 字段不变量

- InputIntent 不是位置、速度、方向结果或授权结果。
- 同一玩家实体同一输入边界只接受一个可审计的当前快照；重复序号不得隐式重放。
- requestedDirection 与 LocationComponent 分离；方向类型共享 owner 标记为 integration-review。

#### 生命周期

玩家实体创建时置空/默认；收到合法输入后替换当前快照；输入边界结束后过期或由下一快照覆盖；离开世界时清除。Version4 MessageBuffer 直接写 Player 字段的现状只作为兼容证据。

#### Entity/World 范围

只属于一个玩家实体，不拥有世界或 Section 输入队列。

#### ID 与关系字段

不拥有实体 ID、网络 slot 或 session ID。输入来源若需绑定 session，只使用带作用域的外部关系，并保留 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\InputIntentComponent.cs 为 partial 现状证据；其布尔字段、tick、序号和 source 已覆盖部分目标状态，但 Version4 的 controlUseTile、释放边沿、方向和网络权限边界尚未完整闭合。

#### 证据

主要证据为 Player.cs:1219-1292、MessageBuffer.cs:652-738、SS14 InputMoverComponent.cs:10-90 的输入快照粒度参考，以及 class_mod_player.html#a891a9922ddd7f63b9d4e56e4231f3838 的公开 SetControls 边界说明。

### 5.3 PlayerLifecycleComponent

#### 职责

保存玩家实例的活跃、死亡、重生和观察生命周期，以及死亡统计和最后死亡快照。不宣布伤害算法、死亡惩罚或掉落事务的最终 owner。

componentId: PG-COMP-03
name: PlayerLifecycleComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerLifecycleComponent.cs (status: proposed)
componentOwner: PlayerGameplay (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: created with player -> alive/active -> dead or spectating -> respawning -> active or destroyed

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| phase | PlayerLifecyclePhase (status: proposed) | Alive（目标模型默认） | 权威状态 | Alive、Dead、Respawning、Spectating 互斥 | partial | src\Player\PlayerValueTypes.cs:42-48；Player.cs:1134,910 |
| isActive | bool | false | 权威/兼容 | inactive 实体不能被当作当前世界玩家 | confirmed / partial | Player.cs:478；Main.cs:11422-11431 |
| isDead | bool | false | 权威/兼容 | 死亡时生命/重生字段关系与 Version4 保持 | confirmed | Player.cs:1134；KillMe:22649-22651 |
| deadElapsedTicks | int | 0 | 权威临时 | 非负，只在死亡/观察阶段累加 | confirmed | Player.cs:1136；Player.cs:10059-10066 |
| respawnRemainingTicks | int | 0 | 权威临时 | 非负；为零不自动表示允许重生 | confirmed | Player.cs:1140-1146；KillMe:22649-22651 |
| spectatingTarget | LegacyPlayerSlot? (status: proposed; crossSubsystemOwner: integration-review) | null；Version4 兼容哨兵为 -1 | 兼容快照 | 槽位失效必须显式失败 | partial | Player.cs:1138；CONTEXT.md:19-26,62-66 |
| wasPvpDeath、pveDeathCount、pvpDeathCount | bool、int、int | false、0、0 | 权威/持久化候选 | 只在一次有效死亡提交中增加 | confirmed | Player.cs:507-509；KillMe:22593-22600 |
| lastDeathPosition | WorldPosition (status: proposed; crossSubsystemOwner: integration-review) | 零位置 | 快照/持久化候选 | 不成为当前 LocationComponent | confirmed / partial | Player.cs:519、KillMe:22601；CONTEXT.md:33-35 |
| lastDeathTime、showLastDeath | DateTime、bool | CLR 默认、false | 快照/兼容 | 时间来自明确时钟边界，Component 不自行读取时钟 | confirmed / partial | Player.cs:521-523、KillMe:22601-22603 |

#### 字段不变量

- isActive、isDead 和 phase 不能形成三套可任意覆盖的权威状态；目标只保留一个权威相位，其余为兼容/派生字段。
- 一次死亡只增加一次对应计数，并只产生一个可审计的死亡快照。
- respawnRemainingTicks 只表达等待量，不表达出生点资格、世界安全位置或传送提交。

#### 生命周期

创建/加载后进入活跃或待进入世界状态；死亡时保存死亡快照并进入等待；重生成功时清除死亡计时、恢复活跃标记；销毁时清理当前实例关系。Spawn 与 KillMe 的外部掉落和位置副作用不复制到 Component。

#### Entity/World 范围

只属于一个玩家实体。出生点 tile、世界安全出生资格和世界保存状态是外部关系或跨域事实。

#### ID 与关系字段

观察目标只能是带作用域的 LegacyPlayerSlot/PlayerHandle 关系；它不是持久化身份。死亡原因若由 CombatAndStatus 提交，关系字段标记 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerLifecycleState.cs 与 dome\src\Terraria.Dome.Simulation\Player\Components\PlayerLifecycleComponent.cs 为 partial。已有字段覆盖死亡/重生计时和出生位置局部模型，但不能证明死亡事实与跨域掉落已闭合。

#### 证据

主要证据为 Player.cs:1134-1148,21798-21860,22572-22681、Main.cs:11422-11431、MessageBuffer.cs:745-765 和 tModLoader class_mod_player.html#a0a73016c97f920cc3022e4c45fc9470c 的公开 UpdateDead 边界。

### 5.4 PlayerVitalComponent

#### 职责

保存玩家当前生命/法力及其基础/有效上限。防御和死亡结算虽有玩家字段证据，但最终 owner 与 CombatAndStatus 存在冲突，因此这里只保留候选字段关系和 integration-review 标记。

componentId: PG-COMP-04
name: PlayerVitalComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerVitalComponent.cs (status: proposed)
componentOwner: PlayerGameplay / CombatAndStatus (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: initialize on creation/load -> change on resource effects and damage/heal -> reset/reseed on respawn -> clear on destruction

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| life | int | 100 | 权威 | 0 <= life <= effectiveLifeMaximum | confirmed | Player.cs:1361；Spawn:21828-21848 |
| baseLifeMaximum | int | 100 | 权威/持久化候选 | 不低于 Version4 最小有效生命约束 | confirmed | Player.cs:1357；MessageBuffer.cs:778-783 |
| effectiveLifeMaximum | int | 100 | 派生 | 不低于有效最小值，由基础值与效果计算得到 | confirmed / partial | Player.cs:1359；Hurt:22229 |
| mana | int | 0 | 权威 | 0 <= mana <= effectiveManaMaximum | confirmed / partial | Player.cs:1363；MessageBuffer.cs:1794-1805 |
| baseManaMaximum | int | 0 | 权威/持久化候选 | 不以网络输入直接扩大上限 | partial | Player.cs:1365 |
| effectiveManaMaximum | int | 0 | 派生 | 不小于零；消耗不得使 mana 为负 | confirmed / partial | Player.cs:1367；Hurt:22254-22262 |
| defense | int | 0（有效默认未锁定） | 派生/跨域候选 | 只在伤害结算边界读取，不承担生命 owner | confirmed / unresolved | Player.cs:1355；Hurt:22229 |

#### 字段不变量

- 当前资源不得超过对应有效上限；网络收到的生命值必须经过 Version4 已有最小值约束。
- base*Maximum 与 effective*Maximum 不得互相镜像写入；有效值是派生值。
- defense、免疫和死亡判断之间存在跨域不变量，当前文档不宣布单独 owner。

#### 生命周期

创建/加载建立基础和当前资源；装备/Buff 变化影响有效上限；受伤/治疗改变当前资源；重生恢复资源并清理死亡临时状态；实体销毁时清理。

#### Entity/World 范围

只属于玩家实体。世界难度、全局伤害倍率和 NPC/Projectile 伤害来源不属于本 Component。

#### ID 与关系字段

不拥有伤害来源 ID；伤害来源只能以跨域只读关系进入，最终 owner 标记 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerVitalState.cs 为 partial，覆盖当前生命、基础/有效上限、法力和防御；没有闭合 Version4 网络输入、死亡 owner 或恢复/免疫所有权。

#### 证据

主要证据为 Player.cs:1355-1381、Player.cs:10804-11290、MessageBuffer.cs:771-787,1794-1805。tModLoader 的公开 ModifyHurt 项在本地页为无链接重载项，因此该交叉证据保持 partial。

### 5.5 PlayerEquipmentComponent

#### 职责

保存玩家装备、染料、杂项装备和隐藏标记的槽位关系。槽位属于玩家实体；Item 实例、Item 定义和装备效果不在本 Component 内。

componentId: PG-COMP-05
name: PlayerEquipmentComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerEquipmentComponent.cs (status: proposed)
componentOwner: PlayerGameplay / ItemContainerAndEconomy (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: allocate fixed slot shape -> load/attach item references -> equip/unequip changes -> death/drop boundary -> clear on destruction

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| armorSlots | ItemEntityRef[20] (status: proposed; crossSubsystemOwner: integration-review) | 20 个空引用 | 权威关系 | 槽位数保持 Version4 形状；实例不能重复占有互斥槽位 | confirmed / partial | Player.cs:1013；PlayerEquipmentState.cs:5-12 |
| dyeSlots | ItemEntityRef[10] (status: proposed; crossSubsystemOwner: integration-review) | 10 个空引用 | 权威关系 | 与装备槽关系分离，不改写 Item 身份 | confirmed / partial | Player.cs:1015；PlayerEquipmentState.cs:6,14 |
| miscEquipmentSlots | ItemEntityRef[5] (status: proposed; crossSubsystemOwner: integration-review) | 5 个空引用 | 权威关系 | 只保存杂项装备关系 | confirmed / partial | Player.cs:1017；PlayerEquipmentState.cs:7,16 |
| miscDyeSlots | ItemEntityRef[5] (status: proposed; crossSubsystemOwner: integration-review) | 5 个空引用 | 权威关系 | 与杂项装备关系分离 | confirmed / partial | Player.cs:1019；PlayerEquipmentState.cs:8,18 |
| hiddenAccessorySlots | bool[10] | 10 个 false | 权威/兼容 | 隐藏不删除装备关系 | confirmed | Player.cs:1208；MessageBuffer.cs:3251 |
| currentLoadoutIndex | int | 0（合法范围未锁定） | 权威选择 | 只能指向存在的 loadout | partial | Player.cs:2491；PlayerEquipmentState.cs:22,40 |

#### 字段不变量

- 装备槽保存 Item 引用，不复制完整 Item 权威状态。
- Item 实例的容器归属、转移、消费和掉落由 ItemContainerAndEconomy 的整合裁决。
- 装备效果产生的生命上限、召唤容量和钓鱼资格是派生结果，不回写成装备槽字段。

#### 生命周期

固定槽位在玩家实体初始化时建立；加载时恢复引用；装备变更时更新关系；死亡掉落时进入跨域提交；实体销毁时解除关系但不自行销毁 Item 实例。

#### Entity/World 范围

只属于玩家实体。银行、世界 Item、NPC 商店和世界掉落不属于本 Component。

#### ID 与关系字段

ItemEntityRef 是跨子系统实体关系，必须标记 crossSubsystemOwner: integration-review；不能用数组索引、whoAmI 或 Item 类型值替代实体引用。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerEquipmentState.cs 为 partial，已有装备、染料、杂项槽、loadout 和隐藏标记；D:\TRbackup\NLTX\src\Items\EquipmentComponent.cs 等文件只作为相邻 Item 证据。

#### 证据

主要证据为 Player.cs:1013-1021,1208-1210,2491、UpdateEquips:6865、UpdateArmorSets:9547 和 DropItems:26304-26345。

### 5.6 PlayerInventoryComponent

#### 职责

保存玩家主背包、银行、虚空仓库、垃圾槽和选中槽位的容器关系。不保存 Item 实例内部字段，也不宣布交易、消费、金币或掉落事务 owner。

componentId: PG-COMP-06
name: PlayerInventoryComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerInventoryComponent.cs (status: proposed)
componentOwner: PlayerGameplay / ItemContainerAndEconomy (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: create container relationships -> load -> inventory/bank operations -> death/drop/save boundary -> clear on destruction

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| mainInventory | ItemEntityRef[59] (status: proposed; crossSubsystemOwner: integration-review) | 59 个空引用 | 权威关系 | 选中索引有效；实例不产生隐式重复拥有 | confirmed / partial | Player.cs:1083；PlayerInventoryState.cs:7-12,36-38 |
| inventoryChestStackEligibility | bool[59] | 59 个 false | 权威/兼容 | 与主背包同长 | confirmed | Player.cs:1085；PlayerInventoryState.cs:5,26 |
| bank、bank2、bank3、bank4 | PlayerContainerRef (status: proposed; crossSubsystemOwner: integration-review) | 未锁定 | 权威关系 | 银行容器与主背包分离 | confirmed / partial | Player.cs:1089-1095；PlayerInventoryState.cs:14-20 |
| voidVaultState | VoidVaultState (status: proposed; crossSubsystemOwner: integration-review) | IsAvailable=false, IsOpen=false | 权威/兼容 | 可用性与开启状态分离 | partial | Player.cs:1097,2993；PlayerInventoryState.cs:22 |
| trashItem | ItemEntityRef (status: proposed; crossSubsystemOwner: integration-review) | 空引用 | 权威关系 | 不是独立实体域；清空时释放引用 | confirmed / partial | Player.cs:1021；DropItems:26308-26309 |
| selectedSlotIndex、lastHotbarSlotIndex | int | 0 | 权威选择/兼容 | 索引在主背包范围内 | partial | Player.cs:2960；PlayerInventoryState.cs:28-30 |
| bufferedSelectedSlotIndex、overriddenSelectedSlotIndex | int? | null | 快照/意图 | 无效索引不得覆盖有效选中槽 | partial | PlayerInventoryState.cs:32-34 |

#### 字段不变量

- 所有 Item 容器关系必须有明确容器 owner 和转移结果；不能以局部数组写入代替事务。
- selectedSlotIndex 与 PlayerUseComponent 的 held-item 读取必须共享同一槽位事实。
- DropCoins/DropItems 证明死亡会读取容器，但不证明死亡掉落事务属于本 Component。

#### 生命周期

创建时建立固定主背包形状并连接银行关系；加载/恢复时恢复容器内容；使用、消费、拾取和交易时由跨域提交改变关系；死亡掉落或保存时产生边界快照；实体销毁时解除当前实体引用。

#### Entity/World 范围

主背包和玩家银行关系属于玩家实体；世界掉落、Chest 实体、商店库存和经济账本不属于本 Component。

#### ID 与关系字段

ItemEntityRef、PlayerContainerRef 都是跨子系统关系值，必须 crossSubsystemOwner: integration-review。不得使用裸数组下标作为持久化容器 ID。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerInventoryState.cs 为 partial，已有主背包、银行、虚空仓库、垃圾槽和选中槽；src\Items 的 InventoryComponent、ContainerContentsComponent 等相邻文件不被本设计吸收。

#### 证据

主要证据为 Player.cs:1083-1097,26261-26345,26394-26418、PlayerInventoryState.cs:3-38 和 SS14 InventoryComponent.cs:8-54 的槽位/容器粒度参考。

### 5.7 PlayerUseComponent

#### 职责

保存玩家当前物品使用的计时、通道和结果状态。不保存 Item 定义、Projectile 实体状态或使用结果事务。

componentId: PG-COMP-07
name: PlayerUseComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerUseComponent.cs (status: proposed)
componentOwner: PlayerGameplay / ItemContainerAndEconomy (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: initialize -> accepted use request -> animation/use timing -> completed/cancelled -> reuse delay -> clear on death/destruction

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| animationRemainingTicks、animationDurationTicks | int、int | 0、0 | 权威临时 | 剩余值不大于有效持续时间 | confirmed / partial | Player.cs:2393-2395；PlayerItemUseState.cs:5-7 |
| useRemainingTicks、useDurationTicks | int、int | 0、0 | 权威临时 | 结束时产生一次完成/取消结果 | confirmed / partial | Player.cs:2397-2399；PlayerItemUseState.cs:9-11 |
| toolTime | int | 0 | 权威临时 | 只用于工具使用时序 | confirmed | Player.cs:2401 |
| reuseDelayRemainingTicks、hasPendingReuse | int、bool | 0、false | 权威临时 | 未到期不得重复消费同一请求 | partial | PlayerItemUseState.cs:13-17；Player.cs:1007 |
| isChanneling、isUseDelayed | bool、bool | false、false | 权威状态/快照 | 通道只在有效使用期间成立 | confirmed / partial | Player.cs:1318,1292；PlayerItemUseState.cs:15,23 |
| heldProjectile | LegacyProjectileSlot? (status: proposed; crossSubsystemOwner: integration-review) | null；Version4 兼容值为 -1 | 兼容关系/快照 | 槽位失效不得绑定新投射物 | confirmed / partial | Player.cs:1035；PlayerItemUseState.cs:19；CONTEXT.md:22-26 |
| lastUseAttemptSucceeded | bool | false | 快照 | 不代替 Item 事务结果 | confirmed | Player.cs:1274；MessageBuffer.cs:731 |

#### 字段不变量

- animation、use 和 reuse delay 的单位统一为模拟 Tick，不混入墙钟时间。
- 失败使用不得扣除 Item 或法力；成功消费与跨域提交不能由 lastUseAttemptSucceeded 单独推断。
- heldProjectile 是兼容关系，不是玩家拥有的投射物数组。

#### 生命周期

默认空闲；合法意图建立使用计时；计时递减直到完成/取消；重用延迟结束后回到可用；死亡、断开或实体销毁时清除临时使用状态。

#### Entity/World 范围

只属于玩家实体。Item 定义、Projectile 生命周期、世界掉落和消费账本排除在外。

#### ID 与关系字段

heldProjectile 使用有作用域的兼容槽位，并标记 crossSubsystemOwner: integration-review；不能作为投射物实体身份。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerItemUseState.cs 为 partial，覆盖动画/使用/重用计时、通道和 held projectile；src\Items\ItemUseComponent.cs 是相邻 Item 侧状态，不替代玩家侧使用状态。

#### 证据

主要证据为 Player.cs:1005-1009,1274-1292,2393-2401、ItemCheckWrapped:19603-19641、ItemCheck:23435 及 tModLoader class_mod_player.html#a53501a140db66e497fad2f3d3920d437 的公开 PreItemCheck 边界。

### 5.8 PlayerAbilityComponent

#### 职责

保存玩家能力容量的玩家侧数值，当前重点是召唤物和炮台容量。容量是装备/Buff/玩家状态的派生结果；召唤实体、投射物实体和炮台实例不属于此 Component。

componentId: PG-COMP-08
name: PlayerAbilityComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerAbilityComponent.cs (status: proposed)
componentOwner: PlayerGameplay / ProjectileSimulation (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: initialize base capacity -> derive from equipment/buffs -> update used capacity from external entity facts -> clear on destruction

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| maximumMinionSlots | float | 1（Version4 maxMinions） | 派生 | 非负；不得低于已用容量而无 trim 结果 | confirmed / partial | Player.cs:832；ResetEffects:10446；PlayerSummonCapacityState.cs:5 |
| usedMinionSlots | float | 0 | 派生缓存 | 不超过最大容量，除待处理清理窗口外 | partial | Player.cs:836；PlayerSummonCapacityState.cs:7,17 |
| minionCount | int | 0 | 派生缓存 | 非负；由外部召唤实体集合计算 | confirmed / partial | Player.cs:834；PlayerSummonCapacityState.cs:9 |
| maximumTurrets、previousMaximumTurrets | int、int | 1、1 | 派生/快照 | 非负；下降时必须有超额处理关系 | confirmed / partial | Player.cs:2244-2246；PlayerSummonCapacityState.cs:11-13 |
| featureFlags | MinionFeatureFlags (status: proposed; crossSubsystemOwner: integration-review) | None | 派生能力标志 | 只表示资格，不表示召唤实体存在 | partial | PlayerSummonCapacityState.cs:15；Version4 分散 minion flag 字段 |

#### 字段不变量

- maximum 与 used 不得由两个独立 Component 镜像维护。
- 容量下降不自动销毁外部实体；超额处理 owner 留给 BD-COMP-03。
- 每个 minion flag 不拆成一级 Component，以避免高频结构变化和脆弱同步。

#### 生命周期

玩家创建时建立基础容量；装备/Buff 改变时重算派生值；外部召唤实体变化时刷新已用容量；玩家死亡或实体销毁时清理玩家侧容量关系。

#### Entity/World 范围

只属于玩家实体；Minion/Projectile 实例、炮台位置和世界上限不属于本 Component。

#### ID 与关系字段

不保存每个召唤实体列表；如需关系，只使用带作用域的玩家 owner 引用并标记 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerSummonCapacityState.cs 为 partial，覆盖容量字段和派生属性；没有证据证明它已与 Version4 的召唤实体数量和清理语义闭合。

#### 证据

主要证据为 Player.cs:832-836,2244-2246,10024-10025,10446-10447,25840 和 PlayerSummonCapacityState.cs:3-21。

### 5.9 PlayerBuffComponent

#### 职责

保存玩家 Buff 槽及 Buff 类型免疫表。Buff 定义、效果规则和伤害/恢复结算不属于此 Component；最终 owner 需要 CombatAndStatus 与 PlayerGameplay 整合裁决。

componentId: PG-COMP-09
name: PlayerBuffComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerBuffComponent.cs (status: proposed)
componentOwner: PlayerGameplay / CombatAndStatus (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: initialize fixed buff slots -> add/refresh/remove/decrement -> derive immunity lookup -> clear or preserve by Version4 semantics

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| slots | BuffSlot[44] (status: proposed; crossSubsystemOwner: integration-review) | 44 个空槽 | 权威 | 一个槽位最多一个 Buff；剩余时间不为正才为空 | confirmed / partial | Player.cs:1027-1031；PlayerValueTypes.cs:93-96 |
| buffType | ContentId<BuffDefinition>[44] (status: proposed; crossSubsystemOwner: integration-review) | 空类型 | 权威关系 | 类型和剩余时间成对存在 | partial | Player.cs:1029；PlayerBuffState.cs:5-15 |
| buffTime | int[44] | 44 个 0 | 权威 | 递减不可越界 | confirmed / partial | Player.cs:1031；PlayerBuffState.cs:5-12 |
| immuneBuffTypes | IReadOnlySet<ContentId<BuffDefinition>> | 空集合 | 派生 | 只影响 Buff 资格，不代替伤害免疫计时 | partial | Player.cs:1033；PlayerBuffState.cs:14-21 |

#### 字段不变量

- Buff 类型和剩余时间共同存在；不把单个 Buff 拆成实体级 Component。
- immuneBuffTypes 与 PlayerRegenerationAndImmunityComponent 的受伤免疫严格分离。
- Buff 清理与死亡/重生语义未完全确认，不补造默认规则。

#### 生命周期

固定槽位随玩家创建；加载时恢复；运行期间添加、刷新、递减、删除；死亡/重生保留规则由 Version4 证据与整合裁决确定；实体销毁时清理。

#### Entity/World 范围

只属于玩家实体。Buff 定义目录和世界范围 Buff 事实不属于本 Component。

#### ID 与关系字段

Buff 定义引用是内容目录关系，不是实体身份；当前 owner 与 CombatAndStatus 共享，标记 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerBuffState.cs 为 partial，覆盖槽位和免疫集合局部模型；完整 Buff 规则不在本设计范围。

#### 证据

主要证据为 Player.cs:1027-1035,4300,10272 和 PlayerBuffState.cs:3-21。

### 5.10 PlayerRegenerationAndImmunityComponent

#### 职责

保存生命/法力恢复累加器、延迟和受伤免疫窗口。它与生命资源 Component 分开，因为字段失效条件和跨域 owner 不同。

componentId: PG-COMP-10
name: PlayerRegenerationAndImmunityComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerRegenerationAndImmunityComponent.cs (status: proposed)
componentOwner: CombatAndStatus / PlayerGameplay (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: initialize/reset derived values -> accumulate/decrement at player boundary -> clear/reseed on damage/death/respawn -> destroy with entity

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| lifeRegenRate、lifeRegenAccumulator、lifeRegenElapsed | int、int、float | 0、0、0 | 权威临时/派生 | 累加器不跨玩家实例；负恢复伤害提交可观察 | confirmed / partial | Player.cs:1369-1373；UpdateLifeRegen:10847 |
| manaRegenRate、manaRegenAccumulator、manaRegenDelay | int、int、float | 0、0、0 | 权威临时/派生 | delay 非负；恢复不越过法力上限 | confirmed / partial | Player.cs:1375-1379；UpdateManaRegen:11239 |
| manaRegenRateBonus、manaRegenDelayBonus、hasManaRegenBuff | int、float、bool | 0、0、false | 派生 | 只在当前效果有效期内存在 | confirmed / partial | Player.cs:674-676,1381 |
| generalImmunityRemainingTicks、cooldownImmunityRemainingTicks | int、int[ImmunityCooldownID.Count] | 0、全零 | 权威临时 | 计时不为负；cooldown 不互相覆盖 | confirmed / partial | Player.cs:968-972,2475；UpdateImmunity:10804 |
| suppressImmunityBlink | bool | false | 派生/表现相关缓存 | 不改变免疫事实 | confirmed / partial | Player.cs:970；PlayerRegenerationAndImmunityState.cs:29 |

#### 字段不变量

- 恢复累加器不得直接写入世界或 Item 状态；资源提交必须经过资源不变量。
- 免疫计时和 Buff 免疫类型不是同一集合。
- 时钟和随机性由外部边界提供；Component 只保存结果状态。

#### 生命周期

创建或加载后置默认；玩家状态变化时更新；伤害、死亡和重生可能清除或重置窗口；销毁时清理。具体保留规则是 BD-COMP-01 的一部分。

#### Entity/World 范围

只属于玩家实体；全局难度和 CombatAndStatus 的伤害来源不属于本 Component。

#### ID 与关系字段

不保存来源实体 ID；来源与结算关系均标记 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerRegenerationAndImmunityState.cs 为 partial，覆盖恢复和免疫字段；最终 owner 尚未与 Combat 领域模型闭合。

#### 证据

主要证据为 Player.cs:968-972,1369-1381,2475、UpdateImmunity:10804、UpdateLifeRegen:10847、UpdateManaRegen:11239。

### 5.11 PlayerSpawnPointComponent

#### 职责

保存玩家个人出生点和返回路线的玩家侧关系。不保存世界安全出生算法、传送位置提交或世界 tile 所有权。

componentId: PG-COMP-11
name: PlayerSpawnPointComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerSpawnPointComponent.cs (status: proposed)
componentOwner: PlayerGameplay / TeleportationAndTraversal (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: create empty -> load personal route -> update on spawn/return item -> clear or retain by Version4 semantics -> destroy with entity

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| personalSpawnTile | TileCoordinate? (status: proposed; crossSubsystemOwner: integration-review) | null | 权威关系/持久化候选 | 坐标在声明世界范围内；不成为当前玩家位置 | partial | Player.cs:1895-1897；PlayerSpawnPointState.cs:5-13 |
| spawnX、spawnY | int、int | -1、-1 | 兼容 | 负值表示未指定；使用前校验世界范围 | confirmed | Player.cs:1895-1897；Spawn:21854-21860 |
| returnOriginalUsePosition | WorldPosition? (status: proposed; crossSubsystemOwner: integration-review) | null | 权威快照/持久化候选 | 只有与返回目标成对才构成路线 | confirmed / partial | Player.cs:1899；MessageBuffer.cs:710-716 |
| returnHomePosition | WorldPosition? (status: proposed; crossSubsystemOwner: integration-review) | null | 权威快照/持久化候选 | 不单独构成合法目标 | confirmed / partial | Player.cs:1901；PlayerSpawnPointState.cs:7-13 |

#### 字段不变量

- 个人出生点、当前实体位置和返回路线是三个不同概念。
- 坐标提交失败不得静默替换为世界原点或旧槽位实体位置。
- Tile 坐标/世界位置是共享值，不能在本 Component 中宣布最终 owner。

#### 生命周期

创建为空；加载或玩家设置时恢复；使用返回物品或重生时读取；清空/更新遵循 Version4 规则；实体销毁时解除关系。

#### Entity/World 范围

字段挂在玩家实体，但目标坐标关系属于世界/空间边界；不创建 World Component。

#### ID 与关系字段

TileCoordinate、WorldPosition 标记 crossSubsystemOwner: integration-review；不使用 whoAmI 标识出生点。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerSpawnPointState.cs 为 partial，覆盖个人出生点和返回路线；dome 局部出生/重生模型不能证明 Version4 完整传送与持久化语义。

#### 证据

主要证据为 Player.cs:1895-1901,21798-21860、MessageBuffer.cs:708-716 和 PlayerSpawnPointState.cs:3-13。

### 5.12 PlayerMountComponent

#### 职责

保存玩家侧坐骑附着关系和玩家侧计时/能力状态。坐骑类型定义、车辆实体、移动解析和附着提交属于跨域边界。

componentId: PG-COMP-12
name: PlayerMountComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerMountComponent.cs (status: proposed)
componentOwner: MountAndVehicleSimulation / PlayerGameplay (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: empty -> attach -> active mounted state -> detach/death/respawn -> empty

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| mountType | ContentId<MountDefinition>? (status: proposed; crossSubsystemOwner: integration-review) | null | 权威关系 | inactive 时不得读取有效坐骑类型 | partial | Player.cs:1552；PlayerMountState.cs:5-8 |
| isActive | bool | false | 权威 | 与 mountType 一致；attach/detach 成对可审计 | confirmed / partial | MessageBuffer.cs:700-707；PlayerMountState.cs:7,35 |
| flightRemainingTicks、fatigue、maximumFatigue | int、float、float | 0、0、0 | 权威临时/派生 | 非负；能力计时不跨坐骑实例泄漏 | partial | PlayerMountState.cs:9-13；Version4 Mount 内部状态未完整读取 |
| abilityCharge、abilityCooldownRemainingTicks、abilityDurationRemainingTicks | int、int、int | 0、0、0 | 权威临时 | 非负；能力使用依赖 active mount | partial | PlayerMountState.cs:15-25 |
| isDismountLocked、isDismountRequested、isMinecart | bool、bool、bool | false、false、false | 权威/快照 | 车辆和普通坐骑语义不互相覆盖 | partial | PlayerMountState.cs:27-41；Player.cs:1552 |

#### 字段不变量

- mountType 是关系引用，不是坐骑实体状态副本。
- 玩家死亡时必须解除或转交附着关系；不能由本 Component 修改车辆位置。
- 与 PlayerRestComponent 的同时有效性需要明确，默认按互斥候选处理。

#### 生命周期

无坐骑创建；合法附着结果后进入 active；能力计时在关系有效期内更新；detach、死亡、重生或销毁时清理。

#### Entity/World 范围

玩家实体范围；坐骑实体、车辆运动和世界轨道不属于本 Component。

#### ID 与关系字段

坐骑实体引用和类型定义均为跨子系统关系，标记 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerMountState.cs 为 partial，有玩家侧坐骑状态；最终 owner 仍需与 MountAndVehicleSimulation 对齐。

#### 证据

主要证据为 Player.cs:1552,2346,2384、MessageBuffer.cs:700-707 和 PlayerMountState.cs:3-41。

### 5.13 PlayerRestComponent

#### 职责

保存玩家坐下/睡眠的玩家侧状态和锚点关系。座椅、床、空间占用和世界锚点不属于本 Component。

componentId: PG-COMP-13
name: PlayerRestComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerRestComponent.cs (status: proposed)
componentOwner: PlayerGameplay / SpatialSimulation / WorldSession (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: none -> sitting/sleeping -> wake/clear -> clear on death/disconnect/destruction

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| mode | PlayerRestMode (status: proposed; crossSubsystemOwner: integration-review) | None | 权威 | None、Sitting、Sleeping 互斥 | partial | PlayerRestState.cs:5,17-19；MessageBuffer.cs:720-725 |
| anchorTile | TileCoordinate? (status: proposed; crossSubsystemOwner: integration-review) | null | 权威关系 | 锚点失效时必须退出或显式失败 | partial | PlayerRestState.cs:7；Player.cs:2286-2288 |
| requiredFacing | DirectionKind (status: proposed; crossSubsystemOwner: integration-review) | Right（当前 NLTX 候选；Version4 需复核） | 派生/资格 | 不使用裸 int 公共契约 | partial | PlayerRestState.cs:9；CONTEXT.md:45-48 |
| stackIndex、sleepElapsedTicks | int、int | 0、0 | 权威临时 | 非负；完全入睡阈值保持现有模型语义 | partial | PlayerRestState.cs:11-17 |
| seatFeatures | RestSeatFeatures (status: proposed; crossSubsystemOwner: integration-review) | None | 资格快照 | 只表达座位能力，不拥有座位实体 | partial | PlayerRestState.cs:15-19 |

#### 字段不变量

- 休息模式与玩家位置/碰撞结果分开；锚点不是位置 Component。
- 坐下和睡眠不能并列成为两套权威布尔字段。
- 外部座椅/床关系失效必须可观察。

#### 生命周期

创建为 None；成功绑定锚点后进入休息；收到唤醒、移动、死亡或断开条件时清理；销毁时清理关系。

#### Entity/World 范围

玩家实体范围，锚点是跨域 Tile/实体关系。

#### ID 与关系字段

锚点和方向值类型均标记 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerRestState.cs 为 partial，提供玩家侧休息字段，但不能证明空间占用和世界锚点 owner 已确定。

#### 证据

主要证据为 Player.cs:2286-2288、MessageBuffer.cs:720-725、Main.cs:11431-11438 和 PlayerRestState.cs:3-20。

### 5.14 PlayerFishingCapabilityComponent

#### 职责

保存玩家装备/Buff 推导出的钓鱼资格和能力标志。浮标实体、饵料 Item、catch 事务和 NPC/Item 结果不属于本 Component。

componentId: PG-COMP-14
name: PlayerFishingCapabilityComponent
status: proposed
proposedPath: D:\TRbackup\NLTX\src\Player\PlayerFishingCapabilityComponent.cs (status: proposed)
componentOwner: FishingAndCatchSimulation / PlayerGameplay (candidate)
crossSubsystemOwner: integration-review
entityScope: one player entity
lifecycle: initialize defaults -> derive from equipment/buffs -> read during fishing attempt -> clear/rederive after equipment change or destruction

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| baseSkill | int | 0 | 权威/派生输入 | 非负；不把有效技能混入 Item 或浮标实体 | confirmed / partial | Player.cs:818；PlayerFishingCapabilityState.cs:5 |
| allowsCrates、hasSonar、hasFishingLineProtection、hasBobberBonus、hasTackleBoxBonus、canFishInLava | bool | false | 派生资格 | 只在当前装备/Buff 条件成立时有效 | confirmed / partial | Player.cs:820-830；PlayerFishingCapabilityState.cs:7-17 |
| bobberOverrideType | ContentId<ProjectileDefinition>? (status: proposed; crossSubsystemOwner: integration-review) | null | 派生关系 | 只指定能力覆盖类型，不拥有浮标实体 | partial | PlayerFishingCapabilityState.cs:19 |
| effectiveFishingLevel | int | 0 | 派生缓存 | 装备变化后必须重算 | partial | PlayerFishingCapabilityState.cs:21-27；Player.cs:818,6933-6935 |

#### 字段不变量

- 资格 Component 不包含浮标实例、饵料容器或 catch 结果。
- effectiveFishingLevel 不能在装备变化后保留无效缓存。
- bobberOverrideType 是内容关系而非 Projectile owner。

#### 生命周期

玩家创建为默认资格；装备/Buff 变化时重算；钓鱼资格读取只影响外部尝试结果，不在此 Component 产生 catch；实体销毁时清理。

#### Entity/World 范围

只属于玩家实体。水域、浮标、饵料和 catch 事务属于外部边界。

#### ID 与关系字段

Projectile 定义引用和浮标实体引用标记 crossSubsystemOwner: integration-review。

#### 当前 NLTX 映射

D:\TRbackup\NLTX\src\Player\PlayerFishingCapabilityState.cs 为 partial，已有玩家资格字段；完整浮标/catch 责任不属于本设计。

#### 证据

主要证据为 Player.cs:818-830,6933-6935,8301-8329、PlayerFishingCapabilityState.cs:3-27 和 tModLoader class_mod_player.html#a6f1888c345019158a7c25485b0208b26 的公开 ModifyFishingAttempt 锚点。

## 6. Entity 与 Component 组合

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| 活跃玩家实体 | PlayerIdentityComponent、InputIntentComponent、PlayerLifecycleComponent、PlayerVitalComponent、PlayerEquipmentComponent、PlayerInventoryComponent、PlayerUseComponent | PlayerAbilityComponent、PlayerBuffComponent、PlayerRegenerationAndImmunityComponent、PlayerSpawnPointComponent、PlayerMountComponent、PlayerRestComponent、PlayerFishingCapabilityComponent | 无结构互斥；phase 必须是活跃兼容状态 | 共同描述玩家实例，但每个 Component 有独立字段生命周期和 owner 候选 |
| 死亡或观察中的玩家实体 | PlayerIdentityComponent、PlayerLifecycleComponent | PlayerVitalComponent、PlayerInventoryComponent、PlayerEquipmentComponent、PlayerSpawnPointComponent、PlayerBuffComponent | PlayerUseComponent 的 active 使用状态与死亡互斥；PlayerMountComponent active attach 通常互斥 | 保留可恢复/持久化状态，同时清理不应跨死亡泄漏的临时状态 |
| 重生中的玩家实体 | PlayerIdentityComponent、PlayerLifecycleComponent、PlayerSpawnPointComponent | PlayerVitalComponent、PlayerInventoryComponent、PlayerEquipmentComponent | PlayerRestComponent、PlayerMountComponent active 状态候选互斥 | 出生点关系与生命恢复生命周期不同 |
| 带有效坐骑关系的玩家实体 | PlayerIdentityComponent、PlayerLifecycleComponent、PlayerMountComponent | InputIntentComponent、PlayerVitalComponent、PlayerEquipmentComponent、PlayerUseComponent | PlayerRestComponent 的 Sitting/Sleeping 与 active mount 候选互斥 | 只保存坐骑关系和玩家侧能力，不复制车辆运动 |
| 带休息锚点的玩家实体 | PlayerIdentityComponent、PlayerLifecycleComponent、PlayerRestComponent | PlayerVitalComponent、InputIntentComponent | PlayerMountComponent active 状态候选互斥 | 休息有自己的锚点、方向和睡眠计时 |
| 具备钓鱼资格的玩家实体 | PlayerIdentityComponent、PlayerEquipmentComponent、PlayerInventoryComponent、PlayerFishingCapabilityComponent | PlayerUseComponent、PlayerAbilityComponent | 无；浮标实体不是本玩家必需 Component | 玩家资格与浮标/catch 实例生命周期不同 |
| 仅作为持久化恢复目标的玩家记录 | 不创建运行时 Entity Component | 持久化快照由外部边界读取 | 不把 PlayerFileData 或 WorldFile 作为玩家实体 Component | 文件路径、云保存、锁和错误恢复不属于玩家实体状态 |
| 玩家实体的空间部分 | 既有 LocationComponent（不由本设计新增） | PlayerRestComponent、PlayerMountComponent 的关系字段 | 不创建第二套 Player 位置 Component | CONTEXT.md 规定 LocationComponent 为统一位置名称 |

## 7. 组件拆分与合并决策

### 7.1 拆分决策

| 决策 | 结论 | 理由 |
|---|---|---|
| 身份与生命周期 | 拆为 PlayerIdentityComponent 与 PlayerLifecycleComponent | 实例身份、持久化身份、slot 关系和死亡/重生相位具有不同生命周期 |
| 输入意图与物品使用 | 拆为 InputIntentComponent 与 PlayerUseComponent | 输入是短期帧快照，使用计时跨多个 Tick 并有完成/取消不变量 |
| 生命资源与恢复/免疫 | 拆为 PlayerVitalComponent 与 PlayerRegenerationAndImmunityComponent | 当前资源/上限与恢复累加器/免疫窗口的失效条件和 owner 不同 |
| 装备与背包 | 拆为 PlayerEquipmentComponent 与 PlayerInventoryComponent | 槽位集合、容器关系和选中槽访问模式不同 |
| Buff 与能力容量 | 拆为 PlayerBuffComponent 与 PlayerAbilityComponent | Buff 是带时间的槽位事实，召唤/炮台容量是派生缓存 |
| 出生点与生命周期 | 拆为 PlayerSpawnPointComponent 与 PlayerLifecycleComponent | 出生点可跨多次死亡持久存在，死亡相位是实例生命周期 |
| 坐骑、休息、钓鱼资格 | 保持独立可选 Component | 都是可选状态，外部关系和失效条件分别不同 |

### 7.2 合并决策

| 合并对象 | 结论 | 理由 |
|---|---|---|
| buffType 与 buffTime | 同一 PlayerBuffComponent | 类型和剩余时间形成不可分割的槽位不变量 |
| 主背包选中槽与背包关系 | 同一 PlayerInventoryComponent | selectedSlotIndex 必须直接解释为主背包引用 |
| 召唤容量与炮台容量 | 同一 PlayerAbilityComponent | 都是玩家能力容量，具有相同的装备/Buff 派生生命周期 |
| 单个输入键、单个装备槽、单个 Buff | 不创建独立 Component | 没有独立实体生命周期或 owner |

### 7.3 设计约束

- 每个 Component 只保存同一内聚概念的状态，不保存日志、时钟、网络 DTO、存档流或 UI 字段。
- 派生值必须写明来源和失效条件；没有证据时保持 partial/unresolved，不升级为 baseline。
- EntityUuid、持久化 ID、网络/slot ID、账户/session ID 和 typed handle 不互换。
- 目录只是领域导航；不能用文件名或目录顺序表达执行先后。

## 8. 不单独创建 Component 的对象

| 对象 | 处理 | 原因与证据 |
|---|---|---|
| 单个玩家字段 | 保留为所属 Component 的字段 | 没有独立生命周期、创建/销毁或 owner |
| 单个输入键或释放键 | 保留在 InputIntentComponent | 共同构成一帧输入意图 |
| 单个 Buff | 保留在 PlayerBuffComponent 槽位 | Version4 是 buffType[]/buffTime[] 并行槽 |
| 单个装备槽或银行槽 | 保留在装备/背包槽位集合 | 需要容器关系和索引不变量 |
| whoAmI、网络 slot、单个 PlayerHandle | 保留为兼容或 typed projection 关系 | CONTEXT.md 明确它们不取代 EntityUuid |
| PlayerFileData、WorldFile | 不挂到玩家实体 | 承担文件边界、路径、云保存、锁和错误恢复 |
| Item、Mount、Projectile 完整实例 | 不复制到玩家 Component | 玩家只保存必要关系或兼容引用 |
| UI、网络消息和存档快照 | 不创建为权威玩家 Component | 它们是边界视图或快照，不能反向成为玩家事实 |
| Update、ItemCheck、Hurt、KillMe 等方法 | 不转换为 Component | 它们是 Version4 行为证据和写集入口，本文件只映射字段归属 |

## 9. 当前 NLTX 组件覆盖

| 当前文件 | 当前状态 | 目标 Component | 覆盖判断 |
|---|---|---|---|
| D:\TRbackup\NLTX\src\Player\InputIntentComponent.cs | status: partial | InputIntentComponent | 已有输入布尔、tick、序号和来源；Version4 全部输入边沿与方向权限未闭合 |
| D:\TRbackup\NLTX\src\Player\PlayerIdentityState.cs | status: partial | PlayerIdentityComponent | 有角色名、队伍、难度、host、连接和 slot 局部状态；缺少已裁决的身份分离 |
| D:\TRbackup\NLTX\src\Player\PlayerLifecycleState.cs | status: partial | PlayerLifecycleComponent | 有 phase、死亡/重生计时、观察 slot 和死亡统计；死亡事实 owner 未闭合 |
| D:\TRbackup\NLTX\src\Player\PlayerVitalState.cs | status: partial | PlayerVitalComponent | 有生命/法力/上限/防御；网络最小值、伤害和死亡边界仍是部分证据 |
| D:\TRbackup\NLTX\src\Player\PlayerEquipmentState.cs | status: partial | PlayerEquipmentComponent | 有装备、染料、杂项、loadout 和隐藏标记；Item 实例 owner 为 integration-review |
| D:\TRbackup\NLTX\src\Player\PlayerInventoryState.cs | status: partial | PlayerInventoryComponent | 有背包、银行、虚空仓库、垃圾槽和选中槽；容器事务未闭合 |
| D:\TRbackup\NLTX\src\Player\PlayerItemUseState.cs | status: partial | PlayerUseComponent | 有动画/使用/重用计时、通道和 held projectile；跨 Item/Projectile 提交未闭合 |
| D:\TRbackup\NLTX\src\Player\PlayerBuffState.cs | status: partial | PlayerBuffComponent | 有槽位和免疫集合；Buff 规则 owner 未闭合 |
| D:\TRbackup\NLTX\src\Player\PlayerRegenerationAndImmunityState.cs | status: partial | PlayerRegenerationAndImmunityComponent | 有恢复/免疫字段；与 CombatAndStatus owner 未裁决 |
| D:\TRbackup\NLTX\src\Player\PlayerSpawnPointState.cs | status: partial | PlayerSpawnPointComponent | 有个人出生点和返回路线；传送/世界坐标 owner 未闭合 |
| D:\TRbackup\NLTX\src\Player\PlayerMountState.cs | status: partial | PlayerMountComponent | 有坐骑能力和附着状态；坐骑实体与移动 owner 未闭合 |
| D:\TRbackup\NLTX\src\Player\PlayerRestState.cs | status: partial | PlayerRestComponent | 有休息模式、锚点、方向和睡眠计时；空间锚点 owner 未闭合 |
| D:\TRbackup\NLTX\src\Player\PlayerFishingCapabilityState.cs | status: partial | PlayerFishingCapabilityComponent | 有钓鱼资格派生字段；浮标、饵料和 catch 不属于当前覆盖 |
| D:\TRbackup\NLTX\src\Player\PlayerSummonCapacityState.cs | status: partial | PlayerAbilityComponent | 有 minion/turret 容量；与 Projectile/召唤实体 used capacity 关系未闭合 |
| D:\TRbackup\NLTX\src\Player\PlayerValueTypes.cs | status: partial | 多个 Component 候选值类型 | 已有 Item/slot/tick/坐标/内容引用候选；EntityUuid、持久化 ID 和共享方向 owner 仍需整合 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Player\Components\PlayerIdentityComponent.cs | status: partial | PlayerIdentityComponent | 有 PlayerHandle、assigned slot、account UUID；不能当作最终跨域身份裁决 |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Player\Components\PlayerLifecycleComponent.cs | status: partial | PlayerLifecycleComponent | 有 active/dead/dead time/respawn/spawn；仍是局部 dome 模型 |
| D:\TRbackup\NLTX\src\Share\Entity\Components\LocationComponent.cs | status: existing | 外部共享空间关系 | 按 CONTEXT.md 保持统一位置名称；不在本设计中复制或改名 |

上述 existing/partial 只描述当前 NLTX 文件状态；目标 Component 定义仍全部是 status: proposed，不能写成已迁移或行为等价。

## 10. 组件级 evidence-gap

| ID | 缺口 | 受影响 Component | 当前证据与影响 |
|---|---|---|---|
| EVIDENCE-GAP-PG-01 | 持久化玩家 ID、账户 UUID、运行时实体 ID、网络/session ID 的完整来源和映射未由 Version4 PlayerFileData 直接确认 | PlayerIdentityComponent | PlayerFileData.cs:10-38 只确认 Player、Name、ServerSideCharacter、LastPlayed；不能补造 PersistentPlayerId |
| EVIDENCE-GAP-PG-02 | 生命/死亡事实由 PlayerGameplay 还是 CombatAndStatus 最终提交，以及 DeathPenalty/掉落提交如何闭合 | PlayerLifecycleComponent、PlayerVitalComponent、PlayerRegenerationAndImmunityComponent | Hurt/KillMe 同时读写多域状态；改变 owner 会改变 Component 组合 |
| EVIDENCE-GAP-PG-03 | Item 实例/容器关系、装备效果、消费、金币和掉落事务的共同提交边界 | PlayerEquipmentComponent、PlayerInventoryComponent、PlayerUseComponent | DropCoins/DropItems 给出读取事实，但没有独立事务 owner 证据 |
| EVIDENCE-GAP-PG-04 | 玩家序列化字段、版本分支和恢复失败保留旧状态规则 | PlayerIdentityComponent、PlayerLifecycleComponent、PlayerVitalComponent、PlayerInventoryComponent | Player.cs:26394-26418 中当前 InternalSavePlayerFile/Serialize 为空；完整参考不能扩展 Version4 基线 |
| EVIDENCE-GAP-PG-05 | 入站输入的权限、序号、重复包和 Tick 归属，以及直接写 Player 字段后的最终权威边界 | InputIntentComponent、PlayerUseComponent | MessageBuffer.cs:652-738 证明直接写入，但不证明新的输入状态契约 |
| EVIDENCE-GAP-PG-06 | direction、LocationComponent、坐骑移动、休息锚点和出生/返回坐标的共享值类型 owner | InputIntentComponent、PlayerSpawnPointComponent、PlayerMountComponent、PlayerRestComponent | CONTEXT.md 规定统一命名和 ID 分离，但未替代跨子系统整合裁决 |

这些缺口不阻止候选 Component 文档生成，但使 designStatus 保持 decision-required；没有任何缺口被默认为 baseline。

## 11. 未决组件 owner

### BD-COMP-01：生命、免疫和死亡的 owner

- 冲突字段：statLife、statLifeMax2、immune、hurtCooldowns、dead、respawnTimer、死亡快照和死亡计数。
- 候选 A：PlayerGameplay 拥有玩家资源与生命周期；CombatAndStatus 只提供伤害结果。
- 候选 B：CombatAndStatus 拥有生命/免疫/死亡事实；PlayerGameplay 只拥有重生相位和玩家本地快照。
- 影响：A 保留 PlayerVitalComponent/PlayerRegenerationAndImmunityComponent 为玩家核心 Component；B 会把它们拆为跨域组件，并改变 PlayerLifecycleComponent 的字段组合。
- 不能在本会话宣布最终 owner：Version4 Hurt/KillMe 将伤害、免疫、死亡、掉落和网络/表现副作用混在 Player 中，研究报告已标记该跨域 handoff 未闭合。

### BD-COMP-02：Item 实例引用和容器 owner

- 冲突字段：armor、inventory、bank*、trashItem、selectedItemState、heldProj 以及死亡掉落读取。
- 候选 A：PlayerGameplay 拥有玩家侧槽位关系，ItemContainerAndEconomy 拥有实例/事务。
- 候选 B：ItemContainerAndEconomy 直接拥有所有槽位，PlayerGameplay 只保存选中/使用快照。
- 影响：A 保留两个玩家 Component；B 会缩小 PlayerGameplay 的 Equipment/Inventory 组合并增加关系引用。
- 不能在本会话宣布最终 owner：Version4 的槽位、Item 实例和掉落在同一 Player 方法中读写，无法从现有成员单独推导事务边界。

### BD-COMP-03：坐骑、休息、钓鱼和能力资格的 owner

- 冲突字段：mount、sitting、sleeping、fishingSkill、maxMinions/maxTurrets 以及与外部实体的关系。
- 候选 A：PlayerGameplay 保存玩家资格/关系 Component，邻接领域保存实体和提交结果。
- 候选 B：对应邻接领域拥有完整 capability Component，PlayerGameplay 只保留玩家输入或只读资格快照。
- 影响：A 产生本文件的可选 Component；B 会将 PlayerMountComponent、PlayerFishingCapabilityComponent 或 PlayerAbilityComponent 降级为跨域快照。
- 不能在本会话宣布最终 owner：研究报告只确认 Version4 玩家字段和局部 NLTX 模型，没有完整 attach/catch/summon 实例调用链的共同 owner 证据。

### BD-COMP-04：身份、持久化、网络和 session 关系

- 冲突字段：EntityUuid、PersistentPlayerId、PlayerHandle、whoAmI/legacy slot、account UUID 和 session/network slot。
- 候选 A：Entity identity registry 拥有 EntityUuid，PlayerGameplay 只持久化玩家 ID 和受控 typed handle。
- 候选 B：WorldSession/NetworkSession 按 slot 建立玩家主关系，PlayerGameplay 仅保存角色身份。
- 影响：A 要求 PlayerIdentityComponent 不存裸网络字段；B 要求所有 slot 映射通过外部关系解析。两者都禁止 whoAmI 成为持久化主键。
- 不能在本会话宣布最终 owner：PlayerFileData 只暴露 Player/Name/LastPlayed，NLTX PlayerHandle 是局部实现，跨领域 Registry/Session 归属仍未整合。

## 12. 最终 Component 清单

| componentId | name | status | entityScope | componentOwner | crossSubsystemOwner | 当前覆盖 |
|---|---|---|---|---|---|---|
| PG-COMP-01 | PlayerIdentityComponent | proposed | one player entity | PlayerGameplay candidate | integration-review | partial |
| PG-COMP-02 | InputIntentComponent | proposed | one player entity | PlayerGameplay candidate | integration-review | partial |
| PG-COMP-03 | PlayerLifecycleComponent | proposed | one player entity | PlayerGameplay candidate | integration-review | partial |
| PG-COMP-04 | PlayerVitalComponent | proposed | one player entity | PlayerGameplay / CombatAndStatus candidate | integration-review | partial |
| PG-COMP-05 | PlayerEquipmentComponent | proposed | one player entity | PlayerGameplay / ItemContainerAndEconomy candidate | integration-review | partial |
| PG-COMP-06 | PlayerInventoryComponent | proposed | one player entity | PlayerGameplay / ItemContainerAndEconomy candidate | integration-review | partial |
| PG-COMP-07 | PlayerUseComponent | proposed | one player entity | PlayerGameplay / ItemContainerAndEconomy candidate | integration-review | partial |
| PG-COMP-08 | PlayerAbilityComponent | proposed | one player entity | PlayerGameplay / ProjectileSimulation candidate | integration-review | partial |
| PG-COMP-09 | PlayerBuffComponent | proposed | one player entity | PlayerGameplay / CombatAndStatus candidate | integration-review | partial |
| PG-COMP-10 | PlayerRegenerationAndImmunityComponent | proposed | one player entity | CombatAndStatus / PlayerGameplay candidate | integration-review | partial |
| PG-COMP-11 | PlayerSpawnPointComponent | proposed | one player entity | PlayerGameplay / TeleportationAndTraversal candidate | integration-review | partial |
| PG-COMP-12 | PlayerMountComponent | proposed | one player entity | MountAndVehicleSimulation / PlayerGameplay candidate | integration-review | partial |
| PG-COMP-13 | PlayerRestComponent | proposed | one player entity | PlayerGameplay / SpatialSimulation / WorldSession candidate | integration-review | partial |
| PG-COMP-14 | PlayerFishingCapabilityComponent | proposed | one player entity | FishingAndCatchSimulation / PlayerGameplay candidate | integration-review | partial |

Component 数量：14。所有目标定义和候选路径均为 status: proposed；当前 NLTX 文件在第 9 节单独标记为 existing 或 partial，不改变目标设计的提案状态。

## 13. 最终声明

本文件是 Component-only Design。
本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 status: proposed 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。
