# Version4 玩家玩法与能力系统组件拆分报告

## 1. 结论与范围

本报告针对《[Version4 权威游戏模拟系统主要子系统](Version4权威游戏模拟系统主要子系统.md)》第 8 项“玩家玩法与能力”，将 `D:\TRbackup\Version4` 中以 `Terraria.Player` 为中心的玩法代码拆成可迁移的 ECS 边界。结论不是将 `Player` 拆成字段数量相近的多个类型，而是将其拆为 13 个按共同读写者、生命周期和不变量划分的状态组件，并由显式 System、Query、Command 和 Projection 协作。

各组件的字段与只读属性设计见《[Version4 玩家玩法与能力系统组件字段设计](Version4玩家玩法与能力系统组件字段设计.md)》。

```text
经验证的网络输入 / 服务端命令
  -> PlayerInputIntent
  -> ItemUse / Rest / Mount / Spawn / Fishing Systems
  -> CombatAndStatus 与 ItemGameplay 的命令和事件
  -> Player 组件状态 + WorldStorage / EntitySlotStore<Projectile>
  -> 网络、存档、客户端表现投影
```

本次只创建设计报告；不修改 `D:\TRbackup\Version4`、不改动当前 `src/`，也没有运行编译。参考目录 `D:\TRbackup\Version4参考` 不存在，经过只读核查后以存在的 `D:\TRbackup\Version4` 作为用户所称 Version4 参考实现。该路径纠正不代表版本等价或运行时兼容承诺。

### 1.1 证据状态

| 结论 | 状态 | 依据 |
| --- | --- | --- |
| 玩家每个世界 Tick 从 `Main.DoUpdateInWorld` 进入 `Player.Update(i)` | confirmed | `Terraria/Main.cs:11418-11442` |
| 玩家核心权威状态集中在 `Player.cs` | confirmed | `Terraria/Player.cs:478-2399`，其中生命/魔力 `1355-1377`、背包 `1013-1083`、死亡 `1134-1140` |
| 生命、魔力、背包、Buff 的公开语义 | confirmed | 本地 tModLoader `v2026.07` 镜像 `class_player.html` 的 `statLife`、`statMana`、`inventory`、`buffType` 成员锚点 |
| 玩家网络和存档是外部边界，不应反向拥有玩法状态 | confirmed | `MessageBuffer.cs:219-3245`、`NetMessage.cs:144-1617`、`Player.cs:26394-26417`、`PlayerFileData.cs:10-46` |
| 睡眠、坐姿、坐骑、钓鱼和召唤容量有独立行为簇 | confirmed | 下表列出的独立类型、字段与方法 |
| 参考 SS14 的组织模式可直接复用 | missing（有意不采用） | SS14 仅支持组件粒度和 System 边界；不得复制其命名或领域语义 |
| 每个旧字段的完整反序列化、包字段和 Mod hook 对应关系 | partial | 当前报告确认核心入口与同步集合；迁移前仍需逐字段建立协议映射和回归夹具 |

### 1.2 来源清单和纳入理由

| 来源 | 直接证据 | 在拆分中的角色 |
| --- | --- | --- |
| `Terraria/Player.cs` | 字段、`Update`、`UpdateBuffs`、`UpdateEquips`、`UpdateDead`、`UpdateLifeRegen`、`ItemCheck`、`Spawn`、`Hurt`、`KillMe` | 主成员盘点与旧写入入口 |
| `Terraria/Main.cs` | 逐 active player 调用 `Player.Update`；清理 sleeping anchors 并聚合熟睡人数 | 调度与世界级睡眠聚合 |
| `Terraria/Entity.cs` | `whoAmI`、位置、速度、尺寸 | 实体身份/运动能力依旧属于相邻 `Entity`、`Movement` 领域 |
| `Terraria/Mount.cs` | `SetMount`、`TryDismount`、`Dismount`、`UpdateEffects`、`UpdateFrame` | 坐骑实例状态和行为协作 |
| `Terraria.GameContent/PlayerSleepingHelper.cs`、`PlayerSittingHelper.cs` | anchor、距离、停止及更新方法 | 休息/坐姿的专属状态与空间资格 |
| `Terraria.DataStructures/FishingAttempt.cs`、`Terraria.GameContent.FishDropRules/FishingContext.cs` | 钓鱼尝试与掉落规则上下文 | 钓鱼过程数据不是背包或 Buff 的附属字段 |
| `Terraria/Projectile.cs` | 统计 `numMinions`、`slotsMinions` 和 turret 容量的消费者 | 召唤容量跨 Player/Projectile 的不变量 |
| `Terraria/PlayerSpawnContext.cs`、`PlayerDeathReason.cs` | 重生情景、伤害/死亡归因值对象 | 命令输入而非 Player 可变状态 |
| `Terraria/MessageBuffer.cs`、`NetMessage.cs` | 包 4、5、13、14、16、42、50、51、80、135、142、147 读写；`SyncOnePlayer` | 协议 Adapter/Projection |
| `Terraria.IO/PlayerFileData.cs` | 玩家存档元数据 | Persistence Projection |
| `Terraria/Collision.cs`、`WorldGen.cs` | 碰撞、可出生区域和 Tile 操作 | 只读资格 Query 或世界结构变更 Command |

`Item.cs`、`NPC.cs`、`WorldItem.cs`、`Chest.cs`、`Wiring.cs`、`Liquid.cs` 和具体投射物 AI 不是本模块的权威成员。它们分别属于内容/物品、NPC、世界对象、交互、液体和投射物系统；玩家 System 只能通过其公开 Query、Command 或事件边界使用它们。

### 1.2.1 相关代码闭包与处置

为使“相关所有代码”可审计，本次以 `Player`、第 8 项列出的玩法名，以及资源、Buff、库存、装备、出生、钓鱼、坐骑、minion/sentry 字段为种子，对 `D:\TRbackup\Version4` 的 992 个 C# 项目文件做了只读闭包检索。命中不自动等于归属：一个文件只有在声明玩家权威状态、执行该状态转换、构造该转换的值对象/规则，或承载其协议/存档边界时才被直接纳入。其余命中按相邻系统或表现层处置。

| 闭包类别 | 已审阅文件 | 处置和原因 |
| --- | --- | --- |
| 玩家权威聚合和调度 | `Terraria/Player.cs`、`Main.cs`、`Entity.cs` | 直接纳入。分别是旧聚合、逐 tick 入口和相邻实体/移动基类；基类字段不回流到 Player 组件。 |
| 装备、库存和能力规则 | `Item.cs`、`EquipmentLoadout.cs`、`DataStructures/ArmorSetBonus.cs`、`ArmorSetBonuses.cs`、`ID/PlayerItemSlotID.cs`、`GameContent/QuickStacking.cs`、`UI/ItemSorting.cs`、`Chest.cs`、`WorldItem.cs` | `EquipmentLoadout`、armor set 值对象和 slot ID 直接作为 `PlayerEquipmentState`/`PlayerInventoryState` 的迁移证据。`Item`、Chest、WorldItem、QuickStacking、ItemSorting 保留给 ItemGameplay、Container 或 UI；玩家模块只发库存事务。 |
| 资源、伤害、死亡和复活 | `DataStructures/PlayerDeathReason.cs`、`PlayerSpawnContext.cs`、`Collision.cs`、`WorldGen.cs`、`NPC.cs`、`Testing/DebugUtils.cs` | DeathReason/SpawnContext 直接作为命令值对象。Collision 和 world spawn 检查作为 Query，WorldGen 的清理作为 world command；NPC 是伤害来源/重生计时的相邻消费者，不能拥有玩家生命周期。DebugUtils 是测试工具而非运行时状态。 |
| 休息和坐骑 | `GameContent/PlayerSleepingHelper.cs`、`PlayerSittingHelper.cs`、`Mount.cs`、`Minecart.cs`、`DelegateMethods.cs`、`DataStructures/EntitySource_Mount.cs`、`ID/MountID.cs` | 前三者直接纳入。Minecart/DelegateMethods/EntitySource_Mount/MountID 分别是定义、表现回调、生成归因和内容 ID 边界；不把回调和纹理/音频数据塞进 `PlayerMountState`。 |
| 钓鱼 | `DataStructures/FishingAttempt.cs`、`PlayerFishingConditions.cs`、`GameContent/FishDropRules/FishingContext.cs`、`FishingConditions.cs`、`AFishingCondition.cs` | 直接纳入钓鱼能力的规则闭包。`PlayerFishingConditions` 是从装备/诱饵导出的过程数据，`FishingAttempt` 是短命尝试输入，`FishingContext`/conditions 是规则输入；三者均不成为持久 Player 状态。 |
| 召唤与哨兵 | `Projectile.cs`、`DataStructures/MinionSpawnInfo.cs`、`MinionSpawnFromInventoryItem.cs`、`ArmorSetBonuses.cs` | 直接纳入容量不变量证据。Projectile 是 owner 关系和已占槽的事实源；两个 Minion 值对象是生成命令输入，ArmorSetBonuses 是 max minion/turret 的派生来源。 |
| 网络、会话和存档 | `MessageBuffer.cs`、`NetMessage.cs`、`Netplay.cs`、`RemoteClient.cs`、`IO/PlayerFileData.cs`、`ID/PlayerItemSlotID.cs` | 直接纳入 Adapter/Projection。它们承载旧包/slot/文件格式，不能成为 gameplay System 的依赖或写入根。 |
| 输入、UI 与表现 | `GameInput/PlayerInput.cs`、`PlayerInputProfile.cs`、`GameContent/PlayerEyeHelper.cs`、`DataStructures/PlayerIntentionGuesser.cs`、`PlayerMovementAccsCache.cs`、`EntityShadowInfo.cs`、`Graphics.Shaders/*`、`UI/ItemSlot.cs` | 仅作输入适配、客户端偏好或表现/缓存证据。输入转换为当 tick `PlayerInputIntent`；缓存和渲染状态不持久化、不复制为服务器玩法权威。 |
| 事件、成就、环境与内容辅助 | `GameContent.Achievements/AchievementsHelper.cs`、`GameContent.Ambience/AmbienceServer.cs`、`GameContent/PressurePlateHelper.cs`、`PortalHelper.cs`、`TeleportHelpers.cs`、`ShimmerUnstuckHelper.cs`、`GameContent.Items/TagEffectState.cs` 及其 `WhipTagEffect*`，`GameContent.ItemDropRules/*`、`GameContent.Golf/GolfHelper.cs` | 只作为订阅事件、交互或投影的外部消费者。成就/环境/聊天/视觉不会反向写 Player 权威规则；Tag、掉落和高尔夫分别归物品、掉落、专用实体系统。 |
| 泛型命中但与第 8 项无直接所有权关系 | `DataStructures/TileEntity.cs`、`TileReachCheckSettings.cs`、`NPCAimedTarget.cs`、`GameContent.Bestiary/NPCWasNearPlayerTracker.cs`、`GameContent.ObjectInteractions/*`、`GameContent.Personalities/*`、`GameContent.Tile_Entities/TEDisplayDoll.cs`、`TEHatRack.cs`、`TELogicSensor.cs`、`NetModules/NetAmbienceModule.cs`、`Creative/CreativePowers.cs`、`SceneMetrics*.cs`、`Wiring.cs` | 已查看命中归因，不纳入本模块。它们的主要权威状态属于 TileEntity、NPC、世界交互、世界会话、内容定义或客户端投影；若玩家发起交互，只接受 `InteractionIntent`。 |

此表是文件级闭包处置，而不是宣称每一个 `Player.cs` 调用点已完成逐语句迁移。逐字段读写者、网络字段和存档字段仍需在第 6 节的每一个迁移批次中以 focused verifier 和协议映射闭合；在那之前，报告将这些细节明确维持为 `partial`。

### 1.3 SS14 只读参照的可迁移模式

在 `C:\Users\shan\Downloads\ECS\space-station-14-master` 中，`Content.Shared/Inventory/InventoryComponent.cs` 保存库存布局，而 `InventorySystem` 提供变更 API；`Mobs/Components/MobStateComponent.cs` 保存生命周期状态，而 `MobStateSystem` 提供 `IsAlive`、`IsDead` 等查询；`HandsComponent`、`InputMoverComponent` 和 `SharedInteractionSystem` 又将持有、输入和交互分开。这支持本报告的以下组织判断：

- 网络可见字段仍归状态组件所有，但写入由具名 System 控制；
- 生命周期、库存、输入和交互不是一个“角色状态”巨型组件；
- 跨实体/跨领域规则放在 System，纯资格判断放在 Query。

它们不证明 Terraria 代码的语义，也不引入 SS14 类型、名字、属性或目录结构。

## 2. 成员归属表

“旧成员”是迁移盘点锚点而不是建议保留的公共字段。`权威` 指服务器结果必须保存或可由权威事件重建的状态；`派生` 指本 tick 由装备/Buff/世界规则计算；`临时` 只能在 tick 或命令执行期间存在；`投影` 不得反向驱动规则。

| 目标组件 | 旧成员/行为锚点 | 状态 | 主写者和读者 | 生命周期 | 证据 | 状态 |
| --- | --- | --- | --- | --- | --- | --- |
| `PlayerIdentityState` | `active`、`host`、`name`、`team`、difficulty、玩家实体/账户/网络 ID 映射 | 权威身份与会话归属 | Session admission 写；关系、网络、存档读 | 连接至断开 | `Player.cs:478`；`NetMessage.cs:2644-2655` | partial：账户 ID/持久化 ID 尚未闭合 |
| `PlayerVitalState` | `statLife*`、`statMana*`、`statDefense` | 当前资源与最大值 | Resource/Combat 写；使用、HUD、死亡读 | 玩家全程 | `Player.cs:1355-1377`；本地 API `class_player.html#a9b28...`、`#aa0cee...` | confirmed |
| `PlayerRegenerationAndImmunityState` | `lifeRegen*`、`manaRegen*`、`manaRegenDelay`、`immune*`、`hurtCooldowns` | 权威计时器 + 每 tick 派生修正 | Regen/Immunity 写；伤害、使用读 | 存活 tick；死亡/重生重置 | `Player.cs:968-972`、`1369-1377`、`10804-11318` | confirmed |
| `PlayerBuffState` | `buffType`、`buffTime`、`buffImmune`、Buff 删除/添加 | 权威 Buff 槽与剩余时间 | Buff System 写；装备、战斗、使用读 | 加入至过期/清除 | `Player.cs:1027-1033`、`3541-3697`、`4300` | confirmed |
| `PlayerEquipmentState` | `armor`、`dye`、`miscEquips`、`miscDyes`、loadout、装备产生的临时能力 | 权威槽位；能力汇总派生 | Inventory transaction 写；Equipment Effect 读/写派生 | 存档；每 tick 重算效果 | `Player.cs:1013-1021`、`6820-6865`、`9547-9833` | confirmed |
| `PlayerInventoryState` | `inventory[59]`、bank 1-4、`trashItem`、selected slot、void vault | 权威物品容器和选中关系 | Inventory transaction 写；Use/Fishing/Drop 读 | 存档/连接全程 | `Player.cs:1083`、`26304`；本地 API inventory 说明 | confirmed |
| `PlayerItemUseState` | `itemAnimation*`、`itemTime*`、`reuseDelay`、`channel`、`pendingItemReuse`、`heldProj` | 权威的使用阶段计时器；瞄准为意图 | Item Use System 唯一写；ItemGameplay/Projection 读 | 每次使用到结束 | `Player.cs:991-1007`、`2393-2399`、`23435-25851` | confirmed |
| `PlayerLifecycleState` | `dead`、`deadTime`、`respawnTimer`、`pvpDeath`、死亡计数、last death | 权威生命周期和倒计时 | Death/Respawn 写；输入和复制读 | active 玩家实体存在期间 | `Player.cs:1134-1148`、`9948`、`22208-227xx` | confirmed |
| `PlayerSpawnPointState` | `SpawnX`、`SpawnY`、team spawn/return 位置 | 权威出生偏好；实际位置属于 Movement | Spawn point command 写；Spawn System 读 | 存档/世界会话 | `Player.cs:1895-1897`、`21798-22083` | confirmed |
| `PlayerRestState` | `sleeping`、`sitting`、anchor、fully asleep、方向 | 权威休息锚点及阶段 | Rest System 写；世界睡眠聚合、Movement/Projection 读 | 交互开始至停止 | `Player.cs:2286-2288`；Sleeping `71-145`；Sitting `36-94` | confirmed |
| `PlayerMountState` | `mount`、挂载类型/帧/疲劳/飞行状态 | 权威挂载实例；表现帧另投影 | Mount System 写；Physics/Item Use 读 | 装备/召唤至卸载 | `Player.cs:1552`；`Mount.cs:3089,4097,4721-4782` | confirmed |
| `PlayerFishingCapabilityState` | `fishingSkill`、crate/sonar/line/bobber/tackle/lava 能力、bobber override | 装备/Buff 生成的本 tick 派生能力 | Equipment/Buff 写；Fishing System 读 | 每 tick rebuild | `Player.cs:818-830`、`20988-21020` | confirmed |
| `PlayerSummonCapacityState` | `maxMinions`、`numMinions`、`slotsMinions`、`maxTurrets`、相关 pet/minion flags | max 为派生、used 为权威聚合/缓存 | Equipment/Buff 写 max；Capacity System 复算 used；Projectile 读 | 每 tick；Projectile 创建/销毁失效 | `Player.cs:832-886`、`2244-2246`、`25840-25851`；`Projectile.cs` | confirmed |

以下旧成员不应转成上述玩家玩法组件的权威字段：`headPosition`、`bodyFrame`、advanced shadows、染料渲染、粒子、声音、聊天 overhead、`netOffset` 和本地 UI 选择器属于客户端表现投影；`position`、`velocity`、`width`、`height` 归 `Entity`/`MovementPhysics`；`control*`/`release*` 归输入意图；伤害加成、暴击、击退、移动速度等装备和 Buff 聚合结果归 `CombatAndStatus` 或 `Movement` 的派生能力视图。这样避免把实体基类、表现、输入和游戏规则重新塞进 `PlayerGameplayComponent`。

## 3. 组件和模块接口契约

每个组件只保存一个内聚概念。接口均为迁移目标的最小语义面，不是从 `Player` 逐方法搬运的兼容 API。

| 模块 | 最小接口 / 输入 | 输出 / Command | Seam、深度、局部性 |
| --- | --- | --- | --- |
| `PlayerVitalState` | `IPlayerVitalView.Read(player)` | `ApplyHealthDelta`、`ApplyManaCost`，拒绝越界结果 | 无世界 I/O 的状态变换；可用边界值表测试生命/魔力上限 |
| `PlayerRegenerationAndImmunityState` | Vital、Buff、Effect snapshot、tick | `RegenerationApplied`、`ImmunityExpired` | `PlayerRegenerationSystem` 是唯一 tick 写者；伤害只发 `DamageResolved` |
| `PlayerBuffState` | `TryAdd(BuffId, duration)`、`Remove(BuffId)`、只读枚举 | `BuffChanged` | 维护 slot/type/time 同步不变量；以 `BuffDefinitionCatalog` 适配旧 BuffID |
| `PlayerEquipmentState` | `IEquipmentSlotView`、变更事务 | `EquipmentChanged`、`EffectRebuildRequested` | 槽位/Loadout 与物品实例隔离；测试非法槽、交换和装备卸载 |
| `PlayerInventoryState` | `IPlayerInventoryView`、`InventoryTransferCommand` | `TransferAccepted/Rejected`、overflow item command | ItemGameplay 是物品实例所有者；库存只保存持有关系和槽布局 |
| `PlayerItemUseState` | `PlayerUseIntent`、HeldItem view、Combat/Movement capability | `ConsumeItem`、`SpendMana`、`SpawnProjectile`、`WorldInteractionRequest` | Use System 单写计时器；相同固定输入得到相同命令序列 |
| `PlayerLifecycleState` | `DamageResolved`、disconnect/enter-world、tick | `PlayerDied`、`RespawnRequested`、`DropInventoryCommand` | Death System 拥有转换；死亡原因通过值对象传入，不保留到组件中 |
| `PlayerSpawnPointState` | bed/team/world spawn preference、world spatial query | `SetPosition`、必要的 `TileMutationCommand` | 出生合法性由 `PlayerSpawnLocationQuery` 判断；不直接读写 `Main.tile` |
| `PlayerRestState` | `RestIntent`、anchor spatial query、Movement state | `RestStarted/Stopped`、`RestAnchorChanged` | 休息和坐姿同为锚定状态，但保留 mode，避免两个可同时为真的组件状态 |
| `PlayerMountState` | mount item/effect、mount definition、movement contact | `MountChanged`、`DismountRequested`、movement modifier | 定义数据在 Catalog；Player 只保留当前挂载和计时，不持有纹理/音频回调 |
| `PlayerFishingCapabilityState` | Equipment/Buff effect snapshot | immutable `FishingCapabilityView` | 重建为派生快照；FishingSimulation 不可写回装备或 Buff |
| `PlayerSummonCapacityState` | capability snapshot、owner projectile query | `CapacityExceeded`、despawn candidates | 将 `max` 的来源和 `used` 的聚合分开；以 Projectile owner 关系作唯一事实源 |
| `PlayerIdentityState` | authenticated session / persistence mapping | player entity activated/deactivated | 四种 ID 分开：`EntityId`、slot index、network session ID、persistent account/character ID |

`PlayerInputIntent` 不是持久组件：它由协议 Adapter 在当前 tick 写入，包含方向、跳跃、使用、替代使用、瞄准、休息/交互请求和输入序号；`PlayerControlValidationQuery` 验证玩家 active、生命周期、序号、频率与权限后才允许消费。原 `control*`、`release*`、`tileInteractAttempted` 不直接由玩法 System 相互修改。

## 4. 行为、Query、Command 和执行顺序

### 4.1 System 的唯一写入边界

| System | 读取 | 唯一写入 | 产生的跨领域命令/事件 |
| --- | --- | --- | --- |
| `PlayerEquipmentEffectSystem` | Equipment、Inventory、Buff、内容定义 | effect snapshot、最大资源/容量派生值 | `EquipmentEffectsRebuilt` |
| `PlayerBuffSystem` | Buff、内容定义、tick | Buff 时间和 effect inputs | `BuffExpired`、`BuffChanged` |
| `PlayerRegenerationSystem` | Vital、Regen/Immunity、effect snapshot | 生命/魔力、计数器、免疫计时器 | `ResourceChanged` |
| `PlayerItemUseSystem` | validated input、inventory、use state、ability views | 使用阶段计时器 | item 消耗、资源支付、Projectile/Tile/Interaction commands |
| `PlayerRestSystem` | Rest intent、Rest state、world anchor query、movement | Rest state | `RestStarted/Stopped` |
| `PlayerMountSystem` | Mount state、use intents、mount definition、movement contacts | Mount state | movement modifier / dismount event |
| `FishingSimulationSystem` | Fishing capability、bobber projectile、FishingAttempt、world liquid/space query | 不写库存或 Buff | catch/loot/consume commands，或失败事件 |
| `PlayerSummonCapacitySystem` | Summon capacity、owner projectile query | used count/cache；必要时 capacity resolution state | despawn/deny spawn commands |
| `PlayerDeathSystem` | DamageResolved、Vital、Lifecycle、Inventory、Mount | Lifecycle、Vital 的终态 | inventory drop、mount dismount、tombstone、death event |
| `PlayerRespawnSystem` | Lifecycle、Spawn point、world spawn query | Lifecycle、Vital、movement position via command | `Respawned`、短暂无敌设置 |

### 4.2 纯 Query

| Query | 输入 | 输出 | 禁止事项 |
| --- | --- | --- | --- |
| `CanStartItemUseQuery` | use state、held item、resource/cooldown/capability | 接受或具体拒绝原因 | 不消耗物品或魔力 |
| `PlayerSpawnLocationQuery` | Spawn point、world rules、TileMap read view | 合法位置或失败原因 | 不清 Tile；旧 `Spawn_ForceClearArea` 必须变成显式 world command |
| `RestAnchorQuery` | Player position、TileMap、rest target | anchor、方向、距离资格 | 不改玩家旋转或 `Main` anchor manager |
| `FishingEligibilityQuery` | FishingCapability、FishingAttempt、液体/空间只读快照 | 资格和 modifiers | 不生成掉落 |
| `SummonCapacityQuery` | player EntityId、max capacity、owner projectile relation | used/remaining/over-limit | 不删除 Projectile |
| `PlayerLifeStateQuery` | Vital、Lifecycle | alive/dead/respawning | 不推进计时器 |

### 4.3 推荐的确定性 tick 顺序

```text
1. NetworkInputAdapter -> 验证 PlayerInputIntent
2. EquipmentEffectSystem -> BuffSystem -> RegenerationAndImmunitySystem
3. RestSystem / MountSystem -> MovementPhysics（相邻子系统）
4. ItemUseSystem -> FishingSimulationSystem -> SummonCapacitySystem
5. CombatAndStatus 结算 DamageResolved -> PlayerDeathSystem
6. PlayerRespawnSystem -> CommandBuffer 提交（Item、Projectile、Tile、位置）
7. ReplicationProjection / PersistenceProjection / ClientPresentationProjection
```

步骤 2 必须先于使用资格与容量检查，否则同 tick 装备/Buff 带来的魔力、能力和 minion slot 结果会滞后。步骤 5 在战斗结算后执行，禁止 `ItemUseSystem` 自己写 `dead`。步骤 6 统一提交结构变化，避免 `Spawn`、`DropItems`、`Projectile` 和 Tile 修改绕过 `WorldStorage`。网络和存档只读提交后的快照。

## 5. 边界、兼容与不拆分项

### 5.1 Adapter 与 Projection

| 边界 | 输入或读取 | 允许输出 | 不允许 |
| --- | --- | --- | --- |
| `PlayerNetworkInputAdapter` | 旧包 4/13/16/42/50/51 等 | 已验证 `PlayerInputIntent`、库存/交互命令 | 直接写任意组件字段 |
| `PlayerReplicationProjection` | 提交后的 Player snapshot | 映射旧包 4、5、13、14、16、42、50、80、135、142、147 | 根据发送失败修改生命、背包、Buff 或重生 |
| `PlayerPersistenceProjection` | 持久组件 snapshot | 读写 `PlayerFileData` 对应 DTO | 将 `PlayerFileData` 透传给 gameplay System |
| `PlayerPresentationProjection` | 只读 gameplay events/state | 动画、染料、音频、聊天、特效 | 将本地表现状态写回权威组件 |
| `LegacyPlayerAdapter`（迁移期） | ECS state | 只读旧 API 视图或单向同步 | 新旧状态双写；双写会形成两个权威源 |

`Main.player[slot]` 的 slot index、`Player.whoAmI`、网络会话 ID 和角色持久化 ID 不同。slot 仍可能是旧网络协议字段；因此 `EntitySlotStore<Player>` 或复制 Adapter 才处理 slot，玩法组件只能持有稳定 `EntityId` 或显式的关系 ID。

### 5.2 明确不拆分或暂缓的项

| 项 | 决定 | 原因 |
| --- | --- | --- |
| `statLife`、生命上限、防御、当前魔力 | 保留在 `PlayerVitalState` | 同一伤害/资源不变量，拆成四个组件会增加查询与原子更新复杂度 |
| 生命再生与无敌计时 | 与 Vital 分离为 `PlayerRegenerationAndImmunityState` | 它们有独立 tick 写者和失效条件，死亡/伤害会同时触发但非同一生命周期 |
| 睡眠和坐姿 | 合并为 `PlayerRestState` 的 mode | 同为唯一 anchor、方向、距离校验和互斥姿态；拆成两个组件会允许无效并存 |
| 坐骑通用实例状态 | 保留为 `PlayerMountState` | 当前 player 仅能有一个 active mount；不把每种 mount 拆组件 |
| 钓鱼尝试、浮标实体、掉落规则 | 不放入 Player 状态 | 尝试是短命命令数据，浮标是 Projectile，掉落属于 ItemGameplay；玩家只保存能力派生视图 |
| `numMinions`/`slotsMinions` | 允许短期 cache，但 Projectile owner query 为事实源 | 如果缓存，必须规定 projectile create/despawn 时失效并在 verifier 中交叉检查 |
| 表现、UI、音频、渲染历史 | 不进入权威 ECS | 客户端可缺失、重放或节流，不能改变服务器结果 |

### 5.3 迁移风险及控制

| 风险 | 后果 | 控制措施 |
| --- | --- | --- |
| 旧包按 slot 和数组索引同步 | 客户端错位或存档损坏 | 先建立每个旧包字段到 snapshot 字段的版本化映射；golden packet fixture 覆盖边界 slot |
| `ItemCheck` 混合 物品规则、特效、随机、投射物和 Tile 修改 | 顺序变更会改变消耗、伤害或生成结果 | 先提取 deterministic decision，再用 command buffer 隔离随机/生成/网络效果 |
| Buff/装备每 tick 重建能力 | 漏掉 reset 会遗留能力；先后错会使当 tick 不一致 | 建立 effect snapshot，明确 reset/rebuild 时机并写顺序测试 |
| 伤害、死亡、掉落、重生相互调用 | 死亡重复、无敌错误或物品双掉 | 以 `DamageResolved -> PlayerDied -> commands` 单向事件链替代互调 |
| 出生可能清理 Tile | 玩家能力模块越权修改世界 | `PlayerSpawnLocationQuery` 只读；清理以 WorldInteraction Command 经世界变更系统执行 |
| 旧代码捕获异常/直接用 `Main.rand`、`DateTime.Now` | 不可复现且难以权威回放 | 通过 `IRandomSource`、`IClock`、`IWorldReadView` 注入；记录随机消费的顺序 |

## 6. 实施批次与 focused verifier

建议每批只迁移一个一致性边界，旧代码通过单向 Adapter 读取新状态，完成比较后再删除旧字段。目标路径遵循领域优先而不是建立全局 `Components/` 目录：例如 `src/Player/PlayerVitalStateComponent.cs`、`src/Player/PlayerBuffStateComponent.cs`、`src/Player/PlayerItemUseSystem.cs`；通用运动/战斗/物品能力仍放在各自领域。每个公共类型一个同名文件，System 顺序由组合根声明，不依赖文件排序。

| 批次 | 迁移内容 | focused verifier | 通过条件 |
| --- | --- | --- | --- |
| 1 | Vital、Regen/Immunity、Lifecycle 的纯状态与事件 | 伤害、治疗、无敌、死亡/重生状态机 | 无越界生命/魔力；一次致死只发一次死亡事件 |
| 2 | Buff、Equipment 与 effect snapshot | Buff 添加/覆盖/到期；装备切换后 snapshot | Buff slot/type/time 一致；效果不会跨 tick 泄漏 |
| 3 | Inventory 与 ItemUse decision | 选中槽、冷却、魔力、自动复用、满背包 overflow | 相同输入只产生一次消耗和一次生成请求 |
| 4 | Rest、Mount、Spawn queries | anchor 距离、停止条件、出生合法性、重生无敌 | Query 无写入；出生 Tile 修改只通过 command |
| 5 | Fishing 与 Summon Capacity | 钓鱼资格、浮标回收；projectile owner 容量重算 | capacity cache 与 owner-query 一致，掉落不由 Player 直接生成 |
| 6 | Network/Persistence Projection | golden packet、旧存档读取/写入、断线/重连 | 旧 slot/字段映射稳定；发送失败不改变权威状态 |

本报告的实际验证结果是：已完成只读的源码、离线 API 与 SS14 组织模式审查；未迁移 C#，因此没有构建、运行时或网络回归结果。实施任一批次后，必须按根 `AGENTS.md` 的串行 .NET 命令约束，仅构建受影响项目，并记录命令、退出码、warning/error 数和 `Build/bin/` 产物路径后才能声称通过。

## 7. 可复查的检索记录

| source | version | query | evidence | gaps / stop reason |
| --- | --- | --- | --- | --- |
| `D:\TRbackup\Version4` | 本地参考源码，版本未从项目元数据断言 | `Player` 的资源、Buff、装备、库存、死亡、Spawn、ItemCheck、Mount、Fishing、minion 字段/入口 | `Player.cs:818-886, 968-1033, 1083, 1134-1148, 1355-1377, 1552, 1895-1897, 21798-22083, 22208-227xx, 23435-25851, 26304-26417` | 核心权威状态/入口充分；逐字段持久化/包映射待迁移批次补齐 |
| `D:\TRbackup\Version4` | 同上 | tick 和 sleeping aggregate | `Main.cs:11418-11442` | 已确认调度入口；完整全局 phase 由 RuntimeComposition 负责 |
| `D:\TRbackup\Version4` | 同上 | rest/mount/fishing/capacity 协作类型 | `PlayerSleepingHelper.cs:71-145`、`PlayerSittingHelper.cs:36-94`、`Mount.cs:3089,4097,4721-4782`、`FishingAttempt.cs:3`、`FishingContext.cs:6` | 满足领域边界判断 |
| `D:\TRbackup\tmodloader-api-docs-stable` | `tModLoader v2026.07`，`index.html` 标题 | `Player.statLife`、`statMana`、`inventory`、`buffType` | `class_player.html#a9b28a808c81cd0f90ae16efbe196f0d1`、`#aa0ceee81c432120bde935869d62f944e`、`#a590dce4227b207c8976811be19f2cf64`、`#a167e74e57e96b01416e55b17c21c7df3` | 只确认公开成员语义；不确认本地私有调用顺序 |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` | 当前本地 checkout，未断言提交 | `InventoryComponent/System`、`MobStateComponent/System`、Hands/Input/Interaction | 组件保存内聚状态，System 暴露具名行为/查询 | 只支持组织模式；不复制领域语义或命名 |
