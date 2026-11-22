# Version4 玩家玩法与能力系统组件字段设计

## 1. 范围

本文是《[Version4 玩家玩法与能力系统组件拆分报告](Version4玩家玩法与能力系统组件拆分报告.md)》的字段级设计补充。它只定义该报告确认的 13 个组件的实例字段和只读属性，不包含 System、Query、Command、Adapter、行为方法、实现代码或测试计划。

字段从 `D:\TRbackup\Version4\Terraria\Player.cs` 及其直接协作类型收敛而来。收敛目标是保留权威状态、显式表示兼容边界，并排除客户端表现、旧静态全局对象和跨领域行为。`C:\Users\shan\Downloads\ECS\space-station-14-master` 仅作为“状态组件不混入行为”的组织参考，不提供 Terraria 字段语义。

## 2. 记法和字段约束

| 记法 | 含义 |
| --- | --- |
| `State` | 组件拥有的权威、可持久化或可由权威事件重建的字段。 |
| `Derived` | 当前 tick 或当前状态的只读派生属性；不持久化、不由网络包直接写入。 |
| `Compatibility` | 仅为旧存档、旧网络 slot 或单向迁移适配保留的字段；不得作为新规则的实体关系。 |
| `Transient` | 单次使用或当前 tick 的权威过程状态；不进入长期存档。 |

下列领域值类型是字段的设计类型，不表示本次生成 C# 代码：

| 类型 | 说明 |
| --- | --- |
| `EntityId` | 稳定的 ECS 实体标识；不用旧 `Player.whoAmI` 代替。 |
| `LegacyPlayerSlot` | 旧 `Main.player` 数组下标，仅供协议 Adapter 使用。 |
| `ItemEntityRef` | 物品实例实体引用；空引用表示空槽。 |
| `ItemContainerState` | 有固定槽位语义的物品容器；内部保存 `ItemEntityRef` 槽位。 |
| `ContentId<T>` | 对只读内容定义的标识，例如 Buff、坐骑、物品类型。 |
| `TileCoordinate` / `WorldPosition` | 世界 Tile 坐标和权威世界坐标；两者不混用。 |
| `SimulationTick` | 权威 tick 时间；替代规则状态中的 `DateTime.Now`。 |
| `Direction` | 面向方向枚举，而不是裸 `int`。 |

所有数组、集合和容器均为组件私有字段；外部只能取得只读视图。组件不保存 `Player`、`Item`、`Chest`、`Mount`、网络 socket、渲染器或 UI 对象引用。

## 3. 组件字段与属性

### 3.1 `PlayerIdentityState`

来源：`Player.cs:478-505, 978, 1154`。角色持久化 ID、账户 ID 与网络 session ID 在参考代码和现有证据中尚未闭合，因此本组件不虚构这些字段。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `CharacterName` | `string` | State | `name` | 角色显示名和角色存档身份文本。 |
| `TeamId` | `int` | State | `team` | 队伍关系键；具体队伍规则不在本组件。 |
| `Difficulty` | `PlayerDifficulty` | State | `difficulty` | 玩家难度，不用原始 `byte` 暴露规则语义。 |
| `IsHost` | `bool` | State | `host` | 旧会话主机标志。 |
| `ConnectionState` | `PlayerConnectionState` | State | `active` | `Inactive`、`Active`、`Disconnecting` 等会话状态。 |
| `LegacySlot` | `LegacyPlayerSlot?` | Compatibility | `whoAmI` / `Main.player[index]` | 仅供旧包和旧数组投影；新关系使用 `EntityId`。 |
| `IsActive` | `bool` | Derived | `active` | `ConnectionState == Active` 的只读属性。 |

### 3.2 `PlayerVitalState`

来源：`Player.cs:1355-1367`；本地 API 镜像明确 `statLife`、`statMana` 分别受 `statLifeMax2`、`statManaMax2` 限制。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `Life` | `int` | State | `statLife` | 当前生命；范围为 `0..EffectiveLifeMaximum`。 |
| `BaseLifeMaximum` | `int` | State | `statLifeMax` | 永久基础上限。 |
| `EffectiveLifeMaximum` | `int` | State | `statLifeMax2` | 装备、Buff 后的当前权威上限。 |
| `Mana` | `int` | State | `statMana` | 当前魔力；范围为 `0..EffectiveManaMaximum`。 |
| `BaseManaMaximum` | `int` | State | `statManaMax` | 永久基础上限。 |
| `EffectiveManaMaximum` | `int` | State | `statManaMax2` | 装备、Buff 后的当前权威上限。 |
| `Defense` | `int` | State | `statDefense` | 当前防御输入；伤害结算规则归 CombatAndStatus。 |
| `IsLifeDepleted` | `bool` | Derived | `statLife <= 0` | 只读生命阈值，不替代 `PlayerLifecycleState`。 |
| `LifeFraction` | `float` | Derived | `statLife / statLifeMax2` | 只读 UI/复制快照输入，分母为零时按零处理。 |
| `ManaFraction` | `float` | Derived | `statMana / statManaMax2` | 只读 UI/复制快照输入，分母为零时按零处理。 |

### 3.3 `PlayerRegenerationAndImmunityState`

来源：`Player.cs:674-676, 968-972, 986, 1369-1381, 2475`。`immuneAlpha`、闪烁方向等纯表现字段不属于本组件。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `LifeRegenRate` | `int` | State | `lifeRegen` | 当前生命再生速率单位。 |
| `LifeRegenAccumulator` | `int` | State | `lifeRegenCount` | 生命再生离散累积量。 |
| `LifeRegenElapsed` | `float` | State | `lifeRegenTime` | 再生状态持续时间。 |
| `ManaRegenRate` | `int` | State | `manaRegen` | 当前魔力再生速率单位。 |
| `ManaRegenAccumulator` | `int` | State | `manaRegenCount` | 魔力再生离散累积量。 |
| `ManaRegenDelay` | `float` | State | `manaRegenDelay` | 当前魔力再生延迟。 |
| `MaximumRegenDelay` | `float` | State | `maxRegenDelay` | 再生延迟上界。 |
| `ManaRegenRateBonus` | `int` | Derived | `manaRegenBonus` | 由装备/Buff 重建的当前 tick 修正。 |
| `ManaRegenDelayBonus` | `float` | Derived | `manaRegenDelayBonus` | 由装备/Buff 重建的当前 tick 修正。 |
| `HasManaRegenBuff` | `bool` | Derived | `manaRegenBuff` | Buff 聚合标志。 |
| `GeneralImmunityRemainingTicks` | `int` | State | `immune`, `immuneTime` | 通用无敌剩余时间；零表示未激活。 |
| `CooldownImmunityRemainingTicks` | `int[]` | State | `hurtCooldowns` | 以伤害冷却类型为索引的无敌剩余时间。 |
| `SuppressImmunityBlink` | `bool` | State | `immuneNoBlink` | 规则侧禁止闪烁的标志；具体闪烁表现不在此处。 |
| `HasGeneralImmunity` | `bool` | Derived | `immune && immuneTime > 0` | 通用无敌是否有效。 |
| `HasAnyImmunity` | `bool` | Derived | `immune` / `hurtCooldowns` | 任一冷却槽仍有效的只读汇总。 |

### 3.4 `PlayerBuffState`

来源：`Player.cs:1027-1033, 3541-3697, 4300`。Buff 类型与时间同槽位绑定，不能拆成互不关联的两个集合。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `Slots` | `BuffSlot[44]` | State | `buffType`, `buffTime`, `maxBuffs` | 每个 `BuffSlot` 同时含 `ContentId<BuffDefinition>` 与 `RemainingTicks`；空槽使用空 Buff ID。 |
| `ImmuneBuffTypes` | `BitSet<BuffDefinition>` | Derived | `buffImmune` | 由装备、世界规则和状态效果重建的免疫集合。 |
| `MaximumSlotCount` | `int` | Derived | `maxBuffs` | 固定为 44 的只读容量属性。 |
| `ActiveBuffCount` | `int` | Derived | `buffType` | 非空槽计数。 |
| `HasFreeSlot` | `bool` | Derived | `buffType` | 是否仍存在可放入新 Buff 的槽位。 |

### 3.5 `PlayerEquipmentState`

来源：`Player.cs:1013-1021, 2484-2490, 3799-3805` 与 `EquipmentLoadout.cs:6-63`。装备产生的能力汇总不储存在这里，而是重建为相邻 Combat、Movement、Fishing 和 Summon 能力视图。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `ArmorAndAccessorySlots` | `ItemEntityRef[20]` | State | `armor` | 护甲、饰品及社交槽的物品引用。 |
| `DyeSlots` | `ItemEntityRef[10]` | State | `dye` | 对应护甲/饰品的染料物品引用；染色结果属于表现投影。 |
| `MiscEquipmentSlots` | `ItemEntityRef[5]` | State | `miscEquips` | 宠物、光宠、矿车、坐骑、钩爪等设备槽。 |
| `MiscDyeSlots` | `ItemEntityRef[5]` | State | `miscDyes` | Misc 设备染料引用。 |
| `Loadouts` | `EquipmentLoadoutState[3]` | State | `Loadouts` | 每个 Loadout 含 20 个装备、10 个染料及 10 个隐藏可见性标志。 |
| `CurrentLoadoutIndex` | `int` | State | `CurrentLoadoutIndex` | 当前生效的 Loadout 下标，范围为 `0..2`。 |
| `HiddenAccessorySlots` | `bool[10]` | State | `hideVisibleAccessory` | 饰品视觉隐藏偏好；不保存渲染状态。 |
| `CurrentLoadout` | `EquipmentLoadoutView` | Derived | `CurrentLoadoutIndex`、`Loadouts` | 当前 Loadout 的只读视图。 |
| `HasValidLoadoutSelection` | `bool` | Derived | `CurrentLoadoutIndex` | 下标是否位于 `Loadouts` 范围内。 |

### 3.6 `PlayerInventoryState`

来源：`Player.cs:1083-1097`、`SelectedItemState`、本地 API 镜像对 59 个 inventory 槽及银行容器的说明。物品实例状态仍由 ItemGameplay 拥有；本组件只拥有持有关系和槽位语义。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `MainInventory` | `ItemContainerState` | State | `inventory[59]` | 59 个固定槽位：hotbar、背包、钱币、弹药和鼠标暂存槽。 |
| `PiggyBank` | `ItemContainerState` | State | `bank` | 个人银行容器。 |
| `Safe` | `ItemContainerState` | State | `bank2` | 个人保险箱容器。 |
| `DefendersForge` | `ItemContainerState` | State | `bank3` | 个人守卫熔炉容器。 |
| `VoidVault` | `ItemContainerState` | State | `bank4` | 虚空袋/虚空保险库容器。 |
| `VoidVaultFlags` | `VoidVaultState` | State | `voidVaultInfo` | 虚空容器可用、开关与兼容状态。 |
| `TrashItem` | `ItemEntityRef` | State | `trashItem` | 垃圾槽物品引用；空引用表示无物品。 |
| `ChestStackEligibility` | `bool[59]` | Compatibility | `inventoryChestStack` | 旧箱子堆叠逻辑的槽位标记，不作为物品规则来源。 |
| `SelectedSlotIndex` | `int` | State | `SelectedItemState.selected` | 当前实际选中槽。 |
| `LastHotbarSlotIndex` | `int` | State | `SelectedItemState.hotbar` | 最近选择的 hotbar 槽。 |
| `BufferedSelectedSlotIndex` | `int?` | Transient | `SelectedItemState.buffered` | 使用过程结束后再切换的待选槽。 |
| `OverriddenSelectedSlotIndex` | `int?` | Transient | `SelectedItemState.overridden` | 临时强制选择槽。 |
| `HeldItem` | `ItemEntityRef` | Derived | `inventory[selectedItem]` | 当前选中槽的只读物品引用。 |

### 3.7 `PlayerItemUseState`

来源：`Player.cs:991-1007, 1035, 1274-1292, 1318, 2393-2399, 23435-25851`。瞄准、按键和输入序号归 `PlayerInputIntent`，不放入持久使用状态。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `AnimationRemainingTicks` | `int` | State | `itemAnimation` | 当前使用动画剩余 tick。 |
| `AnimationDurationTicks` | `int` | State | `itemAnimationMax` | 当前使用动画总 tick。 |
| `UseRemainingTicks` | `int` | State | `itemTime` | 当前使用间隔剩余 tick。 |
| `UseDurationTicks` | `int` | State | `itemTimeMax` | 当前使用间隔总 tick。 |
| `ReuseDelayRemainingTicks` | `int` | State | `reuseDelay` | 自动/重复使用前的延迟。 |
| `IsChanneling` | `bool` | State | `channel` | 持续施法/使用状态。 |
| `HasPendingReuse` | `bool` | Transient | `pendingItemReuse` | 动画结束后等待执行的重复使用。 |
| `HeldProjectileSlot` | `LegacyProjectileSlot?` | Compatibility | `heldProj` | 旧投射物数组索引；新关系应转为 `EntityId`。 |
| `AlternateUseMode` | `ItemUseMode` | Transient | `altFunctionUse` | 主使用、替代使用或无替代使用。 |
| `IsUseDelayed` | `bool` | Transient | `delayUseItem` | 当前使用是否被规则延迟。 |
| `LastUseAttemptSucceeded` | `bool` | Transient | `lastItemUseAttemptSuccess` | 当前 tick 使用尝试结果。 |
| `IsUsingItem` | `bool` | Derived | `itemAnimation`, `itemTime` | 任一使用计时器仍大于零。 |
| `IsReadyForUse` | `bool` | Derived | 使用计时器、重用延迟 | 使用、动画和重用计时器均完成。 |

### 3.8 `PlayerLifecycleState`

来源：`Player.cs:507-523, 1134-1148, 22208-227xx`。旧代码中的 `DateTime` 改为权威 `SimulationTick`，由存档投影负责必要的时间格式转换。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `Phase` | `PlayerLifecyclePhase` | State | `dead`、`respawnTimer` | `Alive`、`Dead`、`Respawning`、`Spectating` 等互斥状态。 |
| `DeadElapsedTicks` | `int` | State | `deadTime` | 本次死亡后的经过时间。 |
| `RespawnRemainingTicks` | `int` | State | `respawnTimer` | 重生倒计时。 |
| `SpectatingTargetSlot` | `LegacyPlayerSlot?` | Compatibility | `spectating` | 旧观战目标数组下标；未观战为空。 |
| `WasPvpDeath` | `bool` | State | `pvpDeath` | 最近死亡是否由 PvP 导致。 |
| `PveDeathCount` | `int` | State | `numberOfDeathsPVE` | 累积 PvE 死亡次数。 |
| `PvpDeathCount` | `int` | State | `numberOfDeathsPVP` | 累积 PvP 死亡次数。 |
| `LastDeathWorldPosition` | `WorldPosition` | State | `lastDeathPostion` | 最近死亡位置。 |
| `LastDeathTick` | `SimulationTick?` | State | `lastDeathTime` | 最近死亡的权威 tick；空表示尚无死亡。 |
| `IsDead` | `bool` | Derived | `Phase` | `Phase` 是否为 `Dead` 或 `Respawning`。 |
| `CanRespawn` | `bool` | Derived | `Phase`、`RespawnRemainingTicks` | 生命周期允许且重生倒计时归零。 |

### 3.9 `PlayerSpawnPointState`

来源：`Player.cs:1895-1901, 21798-22083`。实际出生位置、速度、Tile 清理和无敌属于相邻 Movement、WorldStorage、Lifecycle 组件或命令，不能存入出生偏好状态。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `PersonalSpawnTile` | `TileCoordinate?` | State | `SpawnX`, `SpawnY` | 个人床/出生点；任一坐标无效时为空。 |
| `ReturnOriginalUsePosition` | `WorldPosition?` | State | `PotionOfReturnOriginalUsePosition` | 回归类物品记录的原始使用位置。 |
| `ReturnHomePosition` | `WorldPosition?` | State | `PotionOfReturnHomePosition` | 回归类物品记录的家园位置。 |
| `HasPersonalSpawn` | `bool` | Derived | `PersonalSpawnTile` | 是否具有有效个人出生 Tile。 |
| `HasReturnRoute` | `bool` | Derived | 两个 return position | 两端位置是否均已记录。 |

### 3.10 `PlayerRestState`

来源：`Player.cs:2286-2288`、`PlayerSleepingHelper.cs:7-145`、`PlayerSittingHelper.cs:6-94`、`ExtraSeatInfo.cs:3-5`。睡眠和坐姿合并为互斥 `Mode`，以阻止同一玩家同时坐下和睡眠。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `Mode` | `PlayerRestMode` | State | `isSleeping`, `isSitting` | `None`、`Sitting`、`Sleeping`，三者互斥。 |
| `AnchorTile` | `TileCoordinate?` | State | 由 helper 当前底部 Tile 推导 | 休息对象的权威 Tile 锚点。 |
| `RequiredFacing` | `Direction` | State | helper `targetDirection` | 维持休息状态所需的面向。 |
| `StackIndex` | `int` | Transient | `sleepingIndex`, `sittingIndex` | 同一锚点的临时堆叠序号；由 world rest manager 重建。 |
| `SleepElapsedTicks` | `int` | State | `timeSleeping` | 入睡经过 tick；仅在 `Mode == Sleeping` 时非零。 |
| `SeatFeatures` | `RestSeatFeatures` | State | `ExtraSeatInfo.IsAToilet` | 与规则有关的座椅特征，不直接保存 `ExtraSeatInfo`。 |
| `IsFullyAsleep` | `bool` | Derived | `FullyFallenAsleep` | `Mode == Sleeping && SleepElapsedTicks >= 120`。 |
| `IsResting` | `bool` | Derived | sleeping/sitting flags | `Mode != None`。 |

`visualOffsetOfBedBase`、`offsetForSeat`、旋转和显示位置只供表现计算，明确不成为权威字段。

### 3.11 `PlayerMountState`

来源：`Player.cs:1552`、`Mount.cs:321-367, 387-615`。坐骑定义中的速度、跳跃、纹理、音频回调、帧图和光照信息归 `MountDefinitionCatalog` 或客户端表现；组件仅保留当前实例进度。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `MountType` | `ContentId<MountDefinition>?` | State | `_type` | 当前坐骑定义；空表示未挂载。 |
| `IsActive` | `bool` | State | `_active` | 当前是否处于挂载状态。 |
| `FlightRemainingTicks` | `int` | State | `_flyTime` | 剩余飞行时间。 |
| `Fatigue` | `float` | State | `_fatigue` | 当前疲劳值。 |
| `MaximumFatigue` | `float` | State | `_fatigueMax` | 当前疲劳上限。 |
| `AbilityCharge` | `int` | State | `_abilityCharge` | 坐骑能力蓄力值。 |
| `AbilityCooldownRemainingTicks` | `int` | State | `_abilityCooldown` | 坐骑能力冷却。 |
| `AbilityDurationRemainingTicks` | `int` | State | `_abilityDuration` | 坐骑能力持续时间。 |
| `IsAbilityCharging` | `bool` | State | `_abilityCharging` | 当前是否蓄力。 |
| `IsAbilityActive` | `bool` | State | `_abilityActive` | 当前能力是否持续。 |
| `IsAimingAbility` | `bool` | State | `_aiming` | 当前是否瞄准坐骑能力。 |
| `WalkingGraceRemainingTicks` | `int` | State | `_walkingGraceTimeLeft` | 离地/行走宽限时间。 |
| `UsesSuperCartRules` | `bool` | State | `_shouldSuperCart` | 是否采用超级矿车规则。 |
| `IsMounted` | `bool` | Derived | `_active`, `_type` | `IsActive && MountType` 非空。 |
| `IsMinecart` | `bool` | Derived | `MountData.Minecart` | 从只读 mount definition 得到。 |
| `DismountsOnItemUse` | `bool` | Derived | `MountData.dismountsOnItemUse` | 从只读 mount definition 得到。 |

### 3.12 `PlayerFishingCapabilityState`

来源：`Player.cs:818-830, 1473`、`PlayerFishingConditions.cs:3-15`。该组件是装备/Buff 计算后的当前 tick 能力快照，不能保存浮标实体、钓鱼尝试、掉落或随机结果。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `BaseSkill` | `int` | Derived | `fishingSkill` | 装备和效果给出的基础钓鱼技能。 |
| `AllowsCrates` | `bool` | Derived | `cratePotion` | 箱子相关能力标志。 |
| `HasSonar` | `bool` | Derived | `sonarPotion` | 声呐能力标志。 |
| `HasFishingLineProtection` | `bool` | Derived | `accFishingLine` | 鱼线保护能力标志。 |
| `HasBobberBonus` | `bool` | Derived | `accFishingBobber` | 浮标能力标志。 |
| `HasTackleBoxBonus` | `bool` | Derived | `accTackleBox` | 鱼饵/渔具能力标志。 |
| `CanFishInLava` | `bool` | Derived | `accLavaFishing` | 熔岩钓鱼资格标志。 |
| `BobberOverrideType` | `ContentId<ProjectileDefinition>?` | Derived | `overrideFishingBobber` | 指定钓鱼浮标投射物类型；空表示不覆盖。 |
| `PolePower` | `int` | Derived | `PlayerFishingConditions.PolePower` | 当前鱼竿能力快照。 |
| `BaitPower` | `int` | Derived | `PlayerFishingConditions.BaitPower` | 当前鱼饵能力快照。 |
| `LevelMultiplier` | `float` | Derived | `PlayerFishingConditions.LevelMultipliers` | 当前规则乘数。 |
| `EffectiveFishingLevel` | `int` | Derived | `PlayerFishingConditions.FinalFishingLevel` | 钓鱼规则消费的最终等级。 |

### 3.13 `PlayerSummonCapacityState`

来源：`Player.cs:832-886, 2244-2246, 25840-25851`、`Projectile.cs`、`ArmorSetBonuses.cs`。容量的事实源是 owner 为当前玩家的 Projectile 关系；缓存必须可失效，不能取代该关系。

| 成员 | 目标类型 | 类别 | 旧字段映射 | 说明 |
| --- | --- | --- | --- | --- |
| `MaximumMinionSlots` | `float` | Derived | `maxMinions` | 由装备/Buff 汇总得到的 minion 容量。 |
| `UsedMinionSlots` | `float` | State | `slotsMinions` | owner projectile 聚合的已用容量缓存。 |
| `MinionCount` | `int` | State | `numMinions` | owner minion projectile 数量缓存。 |
| `MaximumTurrets` | `int` | Derived | `maxTurrets` | 由装备/Buff 汇总得到的 sentry 上限。 |
| `PreviousMaximumTurrets` | `int` | Transient | `maxTurretsOld` | 用于检测容量下降并触发清理的上一 tick 值。 |
| `MinionFeatureFlags` | `MinionFeatureFlags` | Derived | `pygmy` 到 `palworldFoxsparksMinion` 等 pet/minion flags | 由装备/Buff 重建的能力集合，不为每个布尔值创建组件。 |
| `RemainingMinionSlots` | `float` | Derived | `maxMinions - slotsMinions` | 负值表示需要容量修正。 |
| `HasMinionCapacity` | `bool` | Derived | 最大/已用容量 | `RemainingMinionSlots > 0`。 |
| `RequiresTurretTrim` | `bool` | Derived | 当前/上一 turret 上限 | 上限降低时为真，由 Projectile 领域执行清理。 |

## 4. 明确不进入字段设计的旧成员

| 旧成员类别 | 处置 |
| --- | --- |
| `position`、`velocity`、`width`、`height`、实体朝向 | 保留在 Entity、Movement、Physics 领域。 |
| `control*`、`release*`、鼠标、手柄和输入配置 | 转为当前 tick `PlayerInputIntent` 或客户端输入适配层。 |
| `headPosition`、`bodyFrame`、染料结果、阴影、粒子、声音、聊天、`netOffset` | 客户端表现投影；不得成为权威组件字段。 |
| `Item`、`Chest`、`Mount`、`Player` 对象引用 | 以实体引用、内容 ID、容器状态或只读定义视图替代。 |
| 浮标实体、FishingAttempt、FishingContext、掉落结果 | 分别属于 Projectile、短命交互输入、规则上下文和 ItemGameplay。 |
| `Main.rand`、`DateTime.Now`、网络 socket、存档 DTO | 分别通过随机/时钟端口或 Projection/Adapter 边界访问。 |

## 5. 字段证据状态

| 证据源 | 结论 |
| --- | --- |
| `D:\TRbackup\Version4\Terraria\Player.cs:478-2490` | 身份、资源、再生、Buff、装备、库存、使用、生命周期、出生、坐骑、钓鱼与召唤容量字段。 |
| `D:\TRbackup\Version4\Terraria.GameContent\PlayerSleepingHelper.cs:7-145` 与 `PlayerSittingHelper.cs:6-94` | Rest mode、锚点、堆叠、入睡 tick 和座椅特征。 |
| `D:\TRbackup\Version4\Terraria\Mount.cs:321-615` | 坐骑实例进度与坐骑定义数据的边界。 |
| `D:\TRbackup\Version4\Terraria\EquipmentLoadout.cs:6-63` | 三个 Loadout 的装备、染料、可见性字段。 |
| `D:\TRbackup\Version4\Terraria.DataStructures\PlayerFishingConditions.cs:3-15` | 钓鱼能力快照字段。 |
| `D:\TRbackup\tmodloader-api-docs-stable\class_player.html`，`tModLoader v2026.07` | 生命、魔力、背包、装备和 turret 行为的公开语义交叉核对。 |

本文不声称字段迁移、网络/存档兼容或运行时行为已实现；它仅固定后续实现的组件字段和属性边界。
