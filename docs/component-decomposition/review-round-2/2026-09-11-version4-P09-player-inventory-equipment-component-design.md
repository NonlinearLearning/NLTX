# Version4 P09 玩家库存、装备、Buff 与防御装载组件拆分设计

partitionId: P09
sessionId: c040060f00534e5fac6f66456c4c1d80
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P09-Player-Inventory-Equipment.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P09-player-inventory-equipment-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P09-player-inventory-equipment-component-execution.md
designStatus: proposed
executionStatus: failed
implementationStatus: implemented
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: not-verified
completedComponents: [PlayerInventorySlotsComponent, PlayerContainerRelationComponent, PlayerEquipmentRelationComponent, PlayerBuffSlotsComponent, PlayerBuffImmunityComponent, PlayerResourceStateComponent, PlayerHeldItemPresentationStateComponent, PlayerEquipmentEffectStateComponent, PlayerDisplayEntityModeComponent, PlayerVisibleEquipmentSelectionComponent, PlayerAppearanceSelectionComponent, PlayerEquipmentColorProjectionComponent, PlayerDefenseStateComponent, PlayerInteractionLockStateComponent, PlayerLoadoutStateComponent]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T10:31:22Z
evidence-gap: P09 15 个允许范围内的 ECS 组件均已写入 src/Player 并完成文件存在性核对；Terraria.Player.csproj 串行构建被 P09 之外的既有 src/Player/Progression 缺失跨项目引用阻塞，verificationStatus 保持 not-verified。hoveredChestIndex、hurtCooldowns、meleeNPCHitCooldown、GetItemLogger、visual clone serialization workspace、heldProj/stealth、jump/bodyFrame/legFrame、Item/Container、网络/持久化和颜色 metadata 仍保留在对应跨域 owner 或 integration-review 边界。
blocking-decision: build-blocked-by-pre-existing-player-progression-references; runner settlement must be Fail

## 1. 范围与状态

本设计只覆盖 P09 权威报告中的 8 个叶子子系统、105 个字段和 0 个属性。它是 ECS 组件边界与迁移输入；本次会话已开始将可独立确定的组件边界落地为 C#，但不把组件源码视为行为等价证明、公共 API 兼容证明、网络闭合证明或持久化闭合证明。本轮不修改测试、权威报告或其他会话文档。

叶子组仍按权威报告顺序处理，但“一个叶子组一个检查点”不等于“一个巨型运行时组件”。每个检查点可以落成一个或多个紧密边界的 Component、System、Query、Command、Adapter 或 Projection；实际文件必须在实现阶段再次依据访问边界决定。

| 检查点 | 权威叶子子系统 | 成员 | 建议边界 | 当前证据 |
|---|---|---:|---|---|
| C01 | PlayerInventoryAndContainerSlots | 8 | 玩家侧槽位与容器关系；Item/Container 内容由 Items 域拥有 | partial |
| C02 | PlayerEquipmentAndDyeSlots | 4 | 玩家装备、染料和杂项装备关系 | partial |
| C03 | PlayerBuffAndResourceSlots | 11 | Buff 槽/免疫、呼吸/岩浆资源和环境投影 | partial |
| C04 | PlayerEquipmentPresentationState | 20 | 持有物、装备效果、潜行和展示模式投影 | partial |
| C05 | PlayerEquipmentSelectionSlots | 21 | 可见装备选择结果的兼容快照 | partial |
| C06 | PlayerAppearanceSelectionState | 6 | 外观偏好、语音和动画帧输入 | partial |
| C07 | PlayerEquipmentColorProjection | 20 | 染料与装备元数据的单向颜色投影 | partial |
| C08 | PlayerDefenseLoadoutAndCloneState | 15 | 防御交互、冷却、装备方案和克隆适配器 | partial |

## 2. 证据登记

| 来源 | 已核对事实 | 支持的结论 | 状态 |
|---|---|---|---|
| P09 权威报告 | P09 报告第 4.13.28、4.13.29、4.13.30、4.13.31、4.13.35、4.13.36、4.13.83、4.13.90 节；来源序号 708..1416 | 105 个成员、声明类型、类型、路径、行号、原始声明及叶子归属 | confirmed |
| Version4 Player 声明 | D:\TRbackup\Version4\Terraria\Player.cs:1013-1097, 1162-1216, 2298-2342, 2461-2501 | P09 全部字段的直接声明、默认值和数组容量 | confirmed |
| Version4 初始化 | D:\TRbackup\Version4\Terraria\Player.cs:26518-26555 | armor、inventory、四个 bank、dye、misc、trash 和 lastVisualizedSelectedItem 的实例化边界 | confirmed |
| Version4 Buff 生命周期 | D:\TRbackup\Version4\Terraria\Player.cs:3523-3715, 4300-5350, 10018-10021 | Buff 槽成对更新、免疫读取/写入、删除压缩、持久 Buff 清理和效果派生 | confirmed |
| Version4 装备/表现生命周期 | D:\TRbackup\Version4\Terraria\Player.cs:3797-3807, 4048-4220, 15284-15296, 20053-20270, 21072-21285, 21320-21490 | loadout 交换、染料计算、装备选择和表现 flags 的写者与重置顺序 | confirmed |
| Version4 库存行为 | D:\TRbackup\Version4\Terraria\Player.cs:14008-14016, 22811-23018, 26461-26516 | chest stack 标记清理、拾取优先级、void vault 路径、查询和物品写入副作用 | confirmed |
| Version4 容器/装备类型 | D:\TRbackup\Version4\Terraria\Chest.cs:17-163; D:\TRbackup\Version4\Terraria\EquipmentLoadout.cs:6-68 | Chest 内容/容量/银行标记及 loadout Swap 的关系边界 | confirmed |
| Version4 网络输入 | D:\TRbackup\Version4\Terraria\MessageBuffer.cs:327-373, 1766-1786, 1926-1942, 2518-2543, 3245-3253, 3354-3363 | Item slot、Buff、item rotation、stealth、QuickStack、loadout 和隐藏配饰的输入写者 | confirmed |
| Version4 网络输出 | D:\TRbackup\Version4\Terraria\NetMessage.cs:994-1002, 1617-1630, 1872-1885, 2640-2677 | Buff、Item 快照、隐藏 bitmask、玩家 join 同步和三个 loadout 的输出边界 | confirmed |
| Version4 文件元数据 | D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs:10-100 | Player owner、路径、ServerSideCharacter、play timer 边界；不证明字段序列化格式 | partial |
| Version4 持久化直接实现 | D:\TRbackup\Version4\Terraria\Player.cs:26406-26460 | 当前 Version4 checkout 的 InternalSavePlayerFile、Serialize、Deserialize 是空实现；不能据此虚构保存格式 | partial |
| 完整参考补证 | D:\TRbackup\客户端\Terraria\Player.cs:53759-53960 及后续 Serialize/Deserialize | 仅作为同路径同类型的 full-reference-supplemented 保存字段顺序参考；不替代 Version4 事实 | partial |
| 当前 NLTX 原型 | src/Player/PlayerInventoryComponent.cs, PlayerInventoryState.cs, PlayerEquipmentComponent.cs, PlayerEquipmentState.cs, PlayerBuffComponent.cs, PlayerBuffState.cs | 已有局部组件/状态模型，但尚未证明唯一 owner、网络、持久化和系统顺序闭合 | existing-evidence |
| 当前 NLTX Item 原型 | src/Items/InventoryComponent.cs, src/Items/ContainerComponent.cs, src/Items/Containers/ContainerContentsComponent.cs, src/Items/EquipmentComponent.cs, src/Items/Equipment/EquipmentRelationComponent.cs | 已存在重复或过宽的 inventory/container/equipment 表达，需要收敛而非双写 | existing-evidence |
| 当前 NLTX Combat 原型 | src/Combat/StatusEffectSlotsComponent.cs, src/Combat/HitCooldownComponent.cs, src/Combat/ImmunityComponent.cs | Combat 已有状态 owner 候选；hurtCooldowns 与 meleeNPCHitCooldown 不能由 P09 私自重复拥有 | existing-evidence |
| tModLoader stable mirror | D:\TRbackup\tmodloader-api-docs-stable\index.html、class_item.html、class_chest.html；首页版本 tModLoader v2026.07 | 公开 Item/Chest API 的边界交叉参考 | confirmed-public-boundary |
| SS14 ECS 只读参考 | C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Inventory、Containers、Stacks、Storage | 槽位关系、窄组件、System/Query 组织粒度 | organization-only |
| 流程缺口 | D:\TRbackup\NLTX\约束\公共拆分约束.md 不存在；Version4\Terraria\ItemSlot.cs 不存在，实际 UI 路径为 Terraria.UI\ItemSlot.cs | 不能声称缺失文件已读取；路径漂移与约束缺口保留在风险中 | missing/partial |

持久化结论必须区分两类事实：Version4 当前 checkout 只证明 Player 保存入口和克隆适配器存在，不证明具体文件字节格式；客户端完整参考可以用于提出待验证的 adapter 计划，但不能把其字段顺序当作本分区已确认的 Version4 事实。

## 3. 边界模型与不变量

### 3.1 所有权图

输入/网络命令 -> 资格 Query 与版本检查 -> PlayerInventory、Equipment、Buff、Defense Owner System -> ItemEntity、Container、Combat 的显式 Command 或只读 Query -> committed facts -> EquipmentEffect、Appearance、Color Projection -> 网络、持久化、UI、渲染 Adapter。

必须保持以下方向：

- Player 组件只保存玩家到 Item/Container 的关系、玩家自己的槽位规则和可验证的玩家状态；Item 的 type、stack、prefix、favorited、内容定义、持久化 Item ID 和经济行为由 Items 域或协议 Adapter 拥有。
- Chest 的 capacity、contents、位置、世界索引和 bank 标记由 Container/Items 域拥有。P09 的 bank、bank2、bank3、bank4 只是玩家侧命名关系/容器句柄；最终跨分区 owner 标记为 integration-review。
- armor、dye、miscEquips、miscDyes 与 Loadouts 保存 Item 关系，不复制 Item 的完整行为或战斗属性。装备变更只能经 EquipmentCommitSystem/Command 写入一个 authority owner。
- buffType 与 buffTime 必须按同一槽位一起移动、清空和复制；buffImmune 是独立的资格/效果集合，不能把剩余时间伪装成免疫。
- itemRotation、itemLocation、heldProj、装备效果绘制 flags、颜色 c*、可见 head/body 等都是表现或派生结果，Projection/Adapter 不得反写 armor/dye authority。
- 可重建的投影只有在声明输入、失效条件和写者后才缓存；每个缓存都有明确的 reset/invalidate 步骤。
- GetItemLogger、MemoryStream、BinaryWriter、BinaryReader、日志、音频、UI 和网络发送属于 effect boundary；不进入持续玩家权威 Component。
- 文件、目录和 Markdown 顺序不定义运行时顺序；调度依赖由显式 scheduler contract 和 verifier 固定。

### 3.2 建议的最小边界

| 检查点 | proposed 模块 | 权威 owner | 只读输入 | 输出/副作用 |
|---|---|---|---|---|
| C01 | PlayerInventorySlotsComponent、PlayerContainerRelationComponent、InventoryCommandSystem、InventoryQuery | 玩家槽位/关系 owner；Container 内容另属 Items | Item/Container Query、玩家意图 | Item transfer Command、inventory snapshot、网络/保存 adapter |
| C02 | PlayerEquipmentRelationComponent、EquipmentCommitSystem、EquipmentQuery | 玩家装备关系 owner；Item 实例另属 Items | Item definition/slot Query、loadout command | equipment facts、effect recalculation event、网络/保存 adapter |
| C03 | PlayerBuffSlotsComponent、PlayerBuffImmunityComponent、PlayerResourceStateComponent、BuffResourceSystem | Buff 槽和玩家资源 owner；状态效果效果值由 Combat/Ability 消费 | Buff catalog、difficulty、environment、equipment facts | effect facts、Buff replication、resource projection |
| C04 | PlayerHeldItemPresentationProjection、PlayerEquipmentEffectProjection、PlayerStealthState integration seam、PlayerDisplayEntityModeComponent | 仅 display mode/特定玩家运行态可由 P09 owner；stealth/held projectile 最终 owner 需 integration-review | equipment、item-use、projectile、movement、mount facts | immutable draw/use snapshot、renderer/audio/UI adapter |
| C05 | PlayerVisibleEquipmentSelectionProjection | 无独立 authority；由 C02/C06/移动/坐骑事实计算 | armor/dye/hidden/mount/body facts | head/body/accessory selection snapshot |
| C06 | PlayerAppearanceSelectionComponent、PlayerAppearanceSelectionSystem、PlayerFrameProjection | 外观偏好/语音/隐藏标记可由玩家外观 owner；jump/frame 最终与 P11/P02 integration-review | input、movement、equipment | network bitmask、animation/render snapshot |
| C07 | PlayerEquipmentColorProjection、EquipmentColorQuery | 无独立 authority | armor/dye/hidden/slot catalog | immutable color snapshot、renderer shader adapter |
| C08 | PlayerDefenseState、PlayerInteractionLockState、PlayerLoadoutState、PlayerItemAcquisitionLogAdapter、Combat integration seam、VisualCloneAdapter | shield/interaction/loadout 可由对应 owner；Combat cooldown 和 clone I/O 不由本组重复拥有 | input、equipment capability、combat facts | defense commands/events、loadout swap facts、log/clone effects |

### 3.3 显式调度契约

1. Spawn/rehydration 先创建 Entity、Item relation 和 Container relation；未完成 hydration 不运行拾取、装备或表现 Query。
2. InventoryCommandSystem 校验 slot/container capacity、版本和 Item/Container Query，提交 Item transfer；成功后发布 inventory committed fact。
3. EquipmentCommitSystem 消费装备/染料/隐藏和 loadout 命令，原子交换玩家关系；不得与旧组件双写。
4. ResetDerivedPlayerEffects 清理上一 tick 的装备效果、资源投影和表现 flags；随后 BuffResourceSystem 按槽位顺序计算效果和资源状态。
5. DefenseCommandSystem 在装备能力事实和输入事实已提交后处理 shield/interaction 命令；Combat resolution 写入 Combat 自己的 cooldown owner，P09 只通过显式 seam 消费或发布结果。
6. VisibleEquipmentSelectionProjection 读取已提交装备、隐藏、坐骑和身体事实；EquipmentColorProjection 读取最终可见槽位和染料事实；二者都输出不可变 snapshot。
7. Network/Persistence/Renderer/UI/Logger/Clone adapters 只消费 committed snapshot 或 command result。失败、重复、超时未知和恢复策略在 adapter contract 中记录。

## 4. 全量成员归属表

下表的来源序号、原始类型和 Version4 行号以 P09 权威报告为准。partial 表示已经定位声明或主要调用点，但还不能证明所有调用者、端别、协议和持久化路径闭合；integration-review 表示本分区不能单方面声明最终 owner。

### C01 PlayerInventoryAndContainerSlots

| 序号 | 成员（C# 类型） | 状态/候选归属 | 主要读者、写者、生命周期 | 证据 |
|---:|---|---|---|---|
| 712 | trashItem (Terraria.Item) | authoritative relation -> PlayerInventorySlots/Trash relation；Item payload 由 Items owner | spawn empty；GetItem/slot return 读取；trash command 写入/清空；网络/保存 adapter 待闭合 | Player.cs:1021, 22811-23018, 26554 |
| 743 | inventory (Terraria.Item[]) | authoritative slot relation -> PlayerInventorySlots；Item instances crossSubsystemOwner: integration-review | spawn 59 empty slots；pickup/use/ammo/coin/HasItem 大量读取；InventoryCommand 唯一写入；网络 slot snapshot | Player.cs:1083, 26518-26540, 22811-23018 |
| 744 | inventoryChestStack (bool[]) | transient operation marker/compat -> InventoryStackInteractionSystem | QuickStack/chest action 设置证据仍 partial；IsStackingItems 读取并清除；tick/action cleanup | Player.cs:1085, 14008-14016 |
| 746 | bank (Terraria.Chest) | player-to-container relation -> PlayerContainerRelationComponent；Chest contents crossSubsystemOwner | constructor CreateBank(-2)；bank contents/transfer/save/net 读取；rehydration/Container command 写入 | Player.cs:1089, 26538; Chest.cs:CreateBank |
| 747 | bank2 (Terraria.Chest) | player-to-container relation -> PlayerContainerRelationComponent；contents external | constructor CreateBank(-3)；transfer/save/net 读取；container hydration writes | Player.cs:1091, 26539; Chest.cs:CreateBank |
| 748 | bank3 (Terraria.Chest) | player-to-container relation -> PlayerContainerRelationComponent；contents external | constructor CreateBank(-4)；transfer/save/net 读取；container hydration writes | Player.cs:1093, 26539; Chest.cs:CreateBank |
| 749 | bank4 (Terraria.Chest) | player-to-container relation -> PlayerContainerRelationComponent；void vault contents external | constructor CreateBank(-5)；void bag queries/GetItem/save/net 读取；container hydration writes | Player.cs:1095, 3784-3790, 22881-23018 |
| 750 | voidVaultInfo (Terraria.BitsByte) | authoritative vault capability flags -> PlayerInventorySlots；encoded/decoded by persistence adapter | IsVoidVaultEnabled/useVoidBag/CanVoidVaultAccept reads; validated vault command writes; save adapter format partial | Player.cs:1097, 2993-3001, 14026-14035, 23085 |

### C02 PlayerEquipmentAndDyeSlots

| 序号 | 成员（C# 类型） | 状态/候选归属 | 主要读者、写者、生命周期 | 证据 |
|---:|---|---|---|---|
| 708 | armor (Terraria.Item[]) | authoritative player-to-item equipment relation -> PlayerEquipmentRelationComponent; Item payload external | constructor; UpdateArmorSets/PlayerFrame/UpdateVisibleAccessories read; EquipmentCommand and loadout Swap write; network/save adapters | Player.cs:1013, 15284-15296, 20053-20270; EquipmentLoadout.cs:29-45 |
| 709 | dye (Terraria.Item[]) | authoritative dye relation -> PlayerEquipmentRelationComponent; dye definition external | UpdateDyes/read, loadout Swap/write, network/save adapter | Player.cs:1015, 4048-4220; NetMessage.cs:2640-2677 |
| 710 | miscEquips (Terraria.Item[]) | authoritative misc equipment relation -> PlayerEquipmentRelationComponent | mount/held/effect queries read; EquipmentCommand/loadout boundary writes; network/save adapter | Player.cs:1017, 13717, 26345-26350 |
| 711 | miscDyes (Terraria.Item[]) | authoritative misc dye relation -> PlayerEquipmentRelationComponent | UpdateDyes reads indices 0..4; equipment command writes; network/save adapter | Player.cs:1019, 4063-4067, 26345-26350 |

### C03 PlayerBuffAndResourceSlots

| 序号 | 成员（C# 类型） | 状态/候选归属 | 主要读者、写者、生命周期 | 证据 |
|---:|---|---|---|---|
| 715 | maxBuffs (int) | immutable schema/config -> BuffSlotSchema; not per-entity mutable state | Buff array allocation and loops read; no runtime writer | Player.cs:1027, 3523-3715 |
| 716 | buffType (int[]) | authoritative slot identity -> PlayerBuffSlotsComponent | AddBuff/DelBuff/UpdateBuffs/net/save read/write; move with buffTime; spawn clear | Player.cs:1029, 3523-3715, 4300+; MessageBuffer.cs:1926-1942 |
| 717 | buffTime (int[]) | authoritative slot duration paired with buffType -> PlayerBuffSlotsComponent | AddBuff/DelBuff/UpdateBuffs decrement/refresh/net/save read/write | Player.cs:1031, 3523-3715, 4300+; NetMessage.cs:994-1002 |
| 718 | buffImmune (bool[]) | authoritative effect qualification -> PlayerBuffImmunityComponent | AddBuff/FindBuff/UpdateBuffs/ResetEffects read; equipment/status commit writes and reset/recompute | Player.cs:1033, 3523-3542, 4300+, 15231-15240 |
| 720 | breathMax (int) | authoritative resource capacity -> PlayerResourceStateComponent | resource/equipment/environment systems read/write; spawn default 200; reset/effect recompute | Player.cs:1037, 26518-26555; Player.cs:Update |
| 721 | breath (int) | authoritative resource value -> PlayerResourceStateComponent | water/air update reads and decrements/restores; damage/environment command writes | Player.cs:1039; Player.cs:Update |
| 722 | lavaMax (int) | authoritative resource capacity -> PlayerResourceStateComponent | lava/equipment effects read/write; spawn default zero; effect reset | Player.cs:1041; Player.cs:Update |
| 723 | lavaTime (int) | authoritative resource timer -> PlayerResourceStateComponent | lava immunity/environment reads and timer writes; reset on lifecycle | Player.cs:1043; Player.cs:Update |
| 724 | ignoreWater (bool) | derived capability projection -> BuffResourceSystem output; final owner integration-review with movement/environment | ResetEffects clears; UpdateBuffs/mount/equipment derives; water movement reads | Player.cs:1045, 4300+, 10272+ |
| 725 | lavaVision (bool) | derived capability projection -> BuffResourceSystem output | ResetEffects clears; buffs/equipment derive; rendering/environment reads | Player.cs:1047, 4300+, 10272+ |
| 726 | lavaOpacity (float) | derived presentation/resource projection -> PlayerResourceProjection | environment/render reads and interpolation; reset/effect writes; no persistence authority | Player.cs:1049, 4300+, 10272+ |

### C04 PlayerEquipmentPresentationState

| 序号 | 成员（C# 类型） | 状态/候选归属 | 主要读者、写者、生命周期 | 证据 |
|---:|---|---|---|---|
| 713 | itemRotation (float) | presentation snapshot -> PlayerHeldItemPresentationProjection | item-use and network input write; PlayerFrame/draw read; reset on item/lifecycle | Player.cs:1023, 23244-23245, 24700+; MessageBuffer.cs:1766-1786 |
| 714 | itemLocation (Vector2) | presentation snapshot -> PlayerHeldItemPresentationProjection | held-item placement writes in use/draw path; renderer/effect reads; reset/recompute each frame | Player.cs:1025, 24204-24699 |
| 719 | heldProj (int) | presentation/projectile link -> adapter; projectile authority external | use/ResetEffects writes; held item and renderer/projectile reads; invalidate on item end | Player.cs:1035, 14809, 24861 |
| 727 | armorEffectDrawShadow (bool) | derived presentation -> PlayerEquipmentEffectProjection | ResetEffects clears; SetArmorEffectVisuals writes; renderer reads | Player.cs:1051, 20245-20254, 21320-21490 |
| 728 | armorEffectDrawShadowSubtle (bool) | derived presentation -> PlayerEquipmentEffectProjection | ResetEffects clears; SetArmorEffectVisuals writes; renderer reads | Player.cs:1053, 20245-20254, 21320-21490 |
| 729 | armorEffectDrawOutlines (bool) | derived presentation -> PlayerEquipmentEffectProjection | ResetEffects clears; SetArmorEffectVisuals writes; renderer reads | Player.cs:1055, 20245-20254, 21320-21490 |
| 730 | armorEffectDrawShadowLokis (bool) | derived presentation -> PlayerEquipmentEffectProjection | ResetEffects clears; SetArmorEffectVisuals writes; renderer reads | Player.cs:1057, 20245-20254, 21320-21490 |
| 731 | armorEffectDrawShadowBasilisk (bool) | derived presentation -> PlayerEquipmentEffectProjection | ResetEffects clears; SetArmorEffectVisuals writes; renderer reads | Player.cs:1059, 20245-20254, 21320-21490 |
| 732 | armorEffectDrawOutlinesForbidden (bool) | derived presentation -> PlayerEquipmentEffectProjection | ResetEffects clears; SetArmorEffectVisuals writes; renderer reads | Player.cs:1061, 20245-20254, 21320-21490 |
| 733 | armorEffectDrawShadowEOCShield (bool) | derived presentation -> PlayerEquipmentEffectProjection | ResetEffects clears; SetArmorEffectVisuals writes; renderer reads | Player.cs:1063, 20245-20254, 21320-21490 |
| 734 | socialShadowRocketBoots (bool) | derived presentation -> PlayerEquipmentEffectProjection | ResetEffects clears; armor/effect path writes; renderer reads | Player.cs:1065, 20245-20255, 20350-20363 |
| 735 | socialGhost (bool) | derived presentation -> PlayerEquipmentEffectProjection | ResetEffects clears; armor effect path writes; renderer reads | Player.cs:1067, 20245-20255, 20255-20270 |
| 736 | shroomiteStealth (bool) | derived combat/presentation capability -> integration-review with Combat/Player ability | ResetEffects/equipment writes; damage/stealth calculations read; tick reset | Player.cs:1069, 10272+, 15426, 25875 |
| 737 | ashWoodBonus (bool) | derived equipment capability -> PlayerEquipmentEffectProjection | ResetEffects clears; armor/effect path writes; ability/combat reads | Player.cs:1071, 10324-10325, 20245+ |
| 738 | socialIgnoreLight (bool) | derived presentation capability -> PlayerEquipmentEffectProjection | ResetEffects clears; armor/effect path writes; renderer/light reads | Player.cs:1073, 20245-20255 |
| 739 | stealthTimer (int) | authoritative transient combat input/state -> integration-review with Combat/Player ability | item use, damage and tick paths write/decrement; stealth query reads | Player.cs:1075, 15377-15430, 22221-22333 |
| 740 | stealth (float) | authoritative transient combat state -> integration-review with Combat/Player ability | item use/damage/tick writes; damage/aggro/draw reads; reset on damage/death | Player.cs:1077, 15377-15480, 22221-22333 |
| 741 | isDisplayDollOrInanimate (bool) | player presentation mode authority -> PlayerDisplayEntityModeComponent | display entity setup writes; equipment/visibility/render queries read; spawn/despawn reset | Player.cs:1079, 3100-3102, 10776 |
| 742 | isHatRackDoll (bool) | player presentation mode authority -> PlayerDisplayEntityModeComponent | display entity setup writes; visibility/render queries read; spawn/despawn reset | Player.cs:1081, 3100-3102 |
| 745 | lastVisualizedSelectedItem (Terraria.Item) | compatibility snapshot -> PlayerHeldItemPresentationAdapter; full Item payload deferred | HeldItem.Clone writes at item visualization; draw reads; reset on construction; no authority | Player.cs:1087, 19653, 23830-23845, 26555 |

### C05 PlayerEquipmentSelectionSlots

| 序号 | 成员（C# 类型） | 状态/候选归属 | 主要读者、写者、生命周期 | 证据 |
|---:|---|---|---|---|
| 783 | head (int) | derived visible selection -> PlayerVisibleEquipmentSelectionProjection | PlayerFrame/UpdateVisibleAccessories write; renderer/armor effects read; reset/recompute | Player.cs:1164, 15284-15286, 20065-20123 |
| 784 | body (int) | derived visible selection -> PlayerVisibleEquipmentSelectionProjection | PlayerFrame writes from armor/sets; renderer/armor effects read | Player.cs:1166, 20065-20123 |
| 785 | legs (int) | derived visible selection -> PlayerVisibleEquipmentSelectionProjection | PlayerFrame writes from armor/sets; renderer/armor effects read | Player.cs:1168, 20065-20123 |
| 786 | coat (int) | derived visible selection -> PlayerVisibleEquipmentSelectionProjection | PlayerFrame/effect path writes; renderer reads; reset per frame | Player.cs:1170, 19377-19394 |
| 787 | handon (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | UpdateVisibleAccessory writes; renderer reads; reset per frame | Player.cs:1172, 19377-19394, 21198-21202 |
| 788 | handoff (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | UpdateVisibleAccessory writes; renderer reads; reset per frame | Player.cs:1174, 19377-19394, 21202 |
| 789 | back (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | visible accessory/mount/sitting writes; renderer reads; reset per frame | Player.cs:1176, 20148-20190, 21208-21217 |
| 790 | front (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | visible accessory/cape writes; renderer reads; reset per frame | Player.cs:1178, 20163-20174, 21217-21226 |
| 791 | shoe (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | accessory/mount/gender writes; renderer reads; reset per frame | Player.cs:1180, 21072-21150, 21230-21238 |
| 792 | waist (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | UpdateVisibleAccessory writes; renderer reads; reset per frame | Player.cs:1182, 21238-21242 |
| 793 | shield (sbyte) | derived defensive accessory selection -> PlayerVisibleEquipmentSelectionProjection | shield/eoc/visible accessory writes; defense renderer reads | Player.cs:1184, 21082-21100, 21242-21246 |
| 794 | neck (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | UpdateVisibleAccessory writes; renderer reads | Player.cs:1186, 21246-21250 |
| 795 | face (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | face layer classification writes; renderer reads | Player.cs:1188, 21250-21267 |
| 796 | balloon (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | balloon layer classification writes; renderer reads | Player.cs:1190, 21267-21275 |
| 797 | backpack (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | back-layer classification writes; renderer reads | Player.cs:1192, 20148-20190, 21208-21217 |
| 798 | tail (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | tail-layer classification writes; renderer reads | Player.cs:1194, 20148-20190, 21208-21217 |
| 799 | faceHead (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | face layer classification writes; renderer reads | Player.cs:1196, 21250-21260 |
| 800 | faceFlower (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | face-flower classification writes; renderer reads | Player.cs:1198, 21256-21264 |
| 801 | faceMask (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | face-mask classification writes; renderer reads | Player.cs:1200, 21250-21260 |
| 802 | balloonFront (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | balloon layer classification writes; renderer reads | Player.cs:1202, 21267-21275 |
| 803 | beard (sbyte) | derived accessory selection -> PlayerVisibleEquipmentSelectionProjection | UpdateVisibleAccessory writes; renderer reads | Player.cs:1204, 21275-21280 |

### C06 PlayerAppearanceSelectionState

| 序号 | 成员（C# 类型） | 状态/候选归属 | 主要读者、写者、生命周期 | 证据 |
|---:|---|---|---|---|
| 782 | jump (int) | animation/mobility input -> integration-review with P02/P11 | movement/input writes; frame/animation reads; reset on movement lifecycle | Player.cs:1162; PlayerFrame and movement paths |
| 804 | voiceOverride (sbyte) | appearance customization authority -> PlayerAppearanceSelectionComponent | player customization/network writes; voice adapter reads; spawn/load/save boundary partial | Player.cs:1206 |
| 805 | hideVisibleAccessory (bool[]) | authoritative appearance preference -> PlayerAppearanceSelectionComponent | network bitmask/loadout Swap writes; visible accessory projection reads; save/network adapter | Player.cs:1208, EquipmentLoadout.cs:29-45, MessageBuffer.cs:3245-3253, 3354-3363 |
| 806 | hideMisc (Terraria.BitsByte) | authoritative appearance preference -> PlayerAppearanceSelectionComponent | network/save/customization writes; renderer/effect queries read | Player.cs:1210, NetMessage.cs:2640-2677; full reference save supplement |
| 807 | bodyFrame (Rectangle) | derived animation snapshot -> integration-review with P11 | PlayerFrame/animation writes; renderer reads; reset/recompute | Player.cs:1214, 20053+ |
| 808 | legFrame (Rectangle) | derived animation snapshot -> integration-review with P11 | PlayerFrame/animation writes; renderer reads; reset/recompute | Player.cs:1216, 20053+ |

### C07 PlayerEquipmentColorProjection

| 序号 | 成员（C# 类型） | 状态/候选归属 | 主要读者、写者、生命周期 | 证据 |
|---:|---|---|---|---|
| 1326 | cHead (int) | derived color projection -> PlayerEquipmentColorProjection | UpdateDyes resets/calculates; shader/renderer reads; invalidate on equipment/dye change | Player.cs:2298, 4048-4220 |
| 1327 | cBody (int) | derived color projection -> PlayerEquipmentColorProjection | UpdateDyes resets/calculates; shader/renderer reads | Player.cs:2300, 4052-4061 |
| 1328 | cLegs (int) | derived color projection -> PlayerEquipmentColorProjection | UpdateDyes resets/calculates; robe rule; renderer reads | Player.cs:2302, 4056-4061 |
| 1329 | cHandOn (int) | derived color projection -> PlayerEquipmentColorProjection | armor item slot metadata calculates; renderer reads | Player.cs:2304, 4099-4103 |
| 1330 | cHandOff (int) | derived color projection -> PlayerEquipmentColorProjection | armor item slot metadata calculates; renderer reads | Player.cs:2306, 4103-4107 |
| 1331 | cBack (int) | derived color projection -> PlayerEquipmentColorProjection | back layer classification calculates; renderer reads | Player.cs:2308, 4109-4117 |
| 1332 | cFront (int) | derived color projection -> PlayerEquipmentColorProjection | front slot calculates; renderer reads | Player.cs:2310, 4122-4126 |
| 1333 | cShoe (int) | derived color projection -> PlayerEquipmentColorProjection | shoe special cases calculate; renderer reads | Player.cs:2312, 4129-4140 |
| 1334 | cWaist (int) | derived color projection -> PlayerEquipmentColorProjection | waist slot calculates; renderer reads | Player.cs:2314, 4142-4144 |
| 1335 | cShield (int) | derived color projection -> PlayerEquipmentColorProjection | shield slot/fallback calculates; renderer reads | Player.cs:2316, 4089-4091, 4146 |
| 1336 | cNeck (int) | derived color projection -> PlayerEquipmentColorProjection | neck slot calculates; renderer reads | Player.cs:2318, 4150 |
| 1337 | cFace (int) | derived color projection -> PlayerEquipmentColorProjection | face layer classification calculates; renderer reads | Player.cs:2320, 4152-4168 |
| 1338 | cFaceHead (int) | derived color projection -> PlayerEquipmentColorProjection | face-head classification calculates; renderer reads | Player.cs:2322, 4152-4160 |
| 1339 | cFaceFlower (int) | derived color projection -> PlayerEquipmentColorProjection | face-flower classification calculates; renderer reads | Player.cs:2324, 4160-4164 |
| 1340 | cFaceMask (int) | derived color projection -> PlayerEquipmentColorProjection | face-mask classification calculates; renderer reads | Player.cs:2326, 4156-4164 |
| 1341 | cBalloon (int) | derived color projection -> PlayerEquipmentColorProjection | balloon layer classification calculates; renderer reads | Player.cs:2328, 4179-4183 |
| 1342 | cBalloonFront (int) | derived color projection -> PlayerEquipmentColorProjection | balloon-front classification calculates; renderer reads | Player.cs:2330, 4179-4183 |
| 1346 | cBackpack (int) | derived color projection -> PlayerEquipmentColorProjection | back backpack layer calculates; renderer reads | Player.cs:2338, 4109-4113 |
| 1347 | cTail (int) | derived color projection -> PlayerEquipmentColorProjection | back tail layer calculates; renderer reads | Player.cs:2340, 4113-4117 |
| 1348 | cShieldFallback (int) | derived fallback projection -> PlayerEquipmentColorProjection | reset to -1; shield visibility may consume; never writes armor/dye | Player.cs:2342, 4052, 4089-4091, 21082-21096 |

### C08 PlayerDefenseLoadoutAndCloneState

| 序号 | 成员（C# 类型） | 状态/候选归属 | 主要读者、写者、生命周期 | 证据 |
|---:|---|---|---|---|
| 1402 | hasRaisableShield (bool) | authoritative defense capability -> PlayerDefenseState | equipment/ability path writes; shield command/render reads; reset each effect cycle | Player.cs:2461, 7007, 7053, 10587 |
| 1403 | shieldRaised (bool) | authoritative defense interaction state -> PlayerDefenseState | input/guard command writes; combat/frame reads; reset on lifecycle | Player.cs:2463, 19580-19600, 20802 |
| 1404 | shieldParryTimeLeft (int) | authoritative defense timer -> PlayerDefenseState | guard command writes/decrements; combat resolution reads; tick reset | Player.cs:2465, 19584-19598 |
| 1405 | shield_parry_cooldown (int) | authoritative defense timer -> PlayerDefenseState | guard command writes/decrements; combat resolution reads | Player.cs:2467, 19584-19598 |
| 1406 | _lockTileInteractionsTimer (int) | authoritative interaction lock -> PlayerInteractionLockState | tile interaction writes/decrements; tile Query reads; spawn/reset cleanup | Player.cs:2471, 17970-17981, 19664 |
| 1407 | hoveredChestIndex (int) | transient input/UI selection snapshot -> PlayerChestInteractionProjection | UI/input writes and reset; chest interaction Query reads; not persistent authority | Player.cs:2473, 19672 |
| 1408 | hurtCooldowns (int[]) | combat authority -> integration-review with Combat HitCooldownComponent | hurt/resolution writes; hurt eligibility reads; tick decrements; no P09 duplicate owner | Player.cs:2475, 10837-10841, 22090-22333 |
| 1409 | GetItemLogger (PlayerGetItemLogger) | effect adapter/static logger -> PlayerItemAcquisitionLogAdapter | GetItem writes log side effect; diagnostics reads; process lifetime, not entity state | Player.cs:2477, 22967-23000 |
| 1410 | meleeNPCHitCooldown (int[]) | combat authority -> integration-review with Combat | melee hit writes/reads; reset per NPC/expiry; no P09 duplicate owner | Player.cs:2481, 23922-23944 |
| 1411 | Loadouts (EquipmentLoadout[]) | authoritative loadout relation state -> PlayerLoadoutState; Item payload external | constructor creates 3 loadouts; Swap exchanges armor/dye/hide; drop/network/save adapters | Player.cs:2484-2489, 3797-3807; EquipmentLoadout.cs:6-68 |
| 1412 | CurrentLoadoutIndex (int) | authoritative loadout selection -> PlayerLoadoutState | TrySwitchingLoadout validates/writes; network reads/writes; spawn/save adapter | Player.cs:2491, 3797-3807; NetMessage.cs:2640-2677 |
| 1413 | _visualCloneDummyData (PlayerFileData) | static serialization adapter resource -> VisualCloneAdapter; deferred from Component | SerializedClone only; adapter owns setup/cleanup; no persistent entity state | Player.cs:2495, 26443-26455 |
| 1414 | _visualCloneStream (MemoryStream) | static serialization workspace -> VisualCloneAdapter; must become per-call/owned resource | SerializedClone seeks/reads/writes; dispose/lease behavior unresolved | Player.cs:2497, 26443-26455 |
| 1415 | _visualCloneWriter (BinaryWriter) | static serialization workspace -> VisualCloneAdapter; deferred | SerializedClone serialization effect; writer lifetime/flush owned by adapter | Player.cs:2499, 26443-26455 |
| 1416 | _visualCloneReader (BinaryReader) | static serialization workspace -> VisualCloneAdapter; deferred | SerializedClone deserialization effect; reader lifetime owned by adapter | Player.cs:2501, 26443-26455 |

## 5. 跨分区交接与不拆分项

### 5.1 integration-review 清单

- Item/Container：C01 的 inventory/trash/bank/bank2/bank3/bank4 只拥有玩家侧关系；ItemInstance、ItemStack、ContainerContents、容量、持久化 Item ID 和 transfer transaction 的最终 owner 由 Items 分区共同确认。不得同时让 PlayerInventoryComponent、InventoryComponent、ContainerComponent 和 ContainerContentsComponent 写同一内容。
- Equipment：C02 的四组数组和 C08 的 Loadouts 只拥有装备关系；现有 src/Player/PlayerEquipmentComponent、src/Items/EquipmentComponent、src/Items/Equipment/EquipmentRelationComponent 必须在实现前选出一个 authority path，其他类型只作为兼容读模型或删除候选。
- Combat：C04 的 shroomiteStealth/stealthTimer/stealth 与 C08 的 hurtCooldowns/meleeNPCHitCooldown 不得与 Combat 的 HitCooldownComponent、ImmunityComponent 或 Damage systems 双写；P09 只定义交接契约。
- Presentation：C04/C05/C06/C07 的字段是可重建快照或输入偏好，不应被 renderer/UI 反向写入。C06 的 bodyFrame/legFrame 和 jump 与 P11/P02 需整合。
- Persistence/network：Version4 直接序列化为空实现；所有保存/加载和 packet adapter 先补 focused evidence，再允许替换旧路径。网络 Item slot 的 type/stack/prefix/favorited 是 Item snapshot，不应塞入 Player component。

### 5.2 暂不拆分或暂不实现

- 不把每个 Item、每个 Chest slot、每个颜色或每个 buff 做成独立实体；它们的共同不变量和访问频率不支持这种原子化。
- 不把 maxBuffs、GetItemLogger、_visualClone* 当作玩家实体组件；它们分别是 schema/config、effect adapter 和序列化 workspace。
- 不把颜色 c*、head/body/legs/accessory selections 反向写入 armor/dye；只能从已提交事实计算。
- 不在本轮实现 Player 保存格式、网络协议变更、Item ID 映射或 Combat cooldown 迁移；这些是 execution plan 的后续门槛。

## 6. 验证门槛与结果

本设计阶段的 focused verifier 计划：

1. 成员闭包：P09 报告来源序号集合与设计矩阵集合相等，105 unique、0 duplicate，8 组计数为 8/4/11/20/21/6/20/15。
2. Owner 唯一性：每个 authority member 只有一个写入 System；Projection/Adapter 行没有 authority write；integration-review 行在没有跨分区决策时必须阻止迁移。
3. 槽位不变量：inventory 59、armor 20、dye 10、misc 5/5、buff 44、hidden 10；buffType/buffTime 的 slot index 永远同步；loadout Swap 不丢失关系。
4. 纯查询：visible selection 和 color projection 对相同输入确定性相同，且不修改 armor/dye/item/container/buff authority。
5. 效果边界：GetItem 的 stack/coin/log/UI/audio/network 顺序可审计；clone stream 在成功、异常和重复调用下资源不泄漏；网络重复包按版本或幂等规则处理。
6. 生命周期：spawn/rehydration/reset/tick/despawn 覆盖每个组件；旧 adapter 只在新 owner verifier 通过后移除。

实际结果：已通过 Build/Tools/Invoke-SerialDotnet.ps1 尝试串行编译 Terraria.Player.csproj。首次直接传递 -p: 属性被 PowerShell 解析为 wrapper 的模糊参数，退出码 1，未启动 dotnet；随后使用 wrapper 的 -DotnetArguments 数组实际执行构建，退出码 1、0 个警告、5 个错误。5 个错误全部来自现有 src/Player/Progression 文件对缺失 Terraria.Relationships、Terraria.Projectile、EntityReference 和 ProjectileIdentityComponent 的引用，未报告 P09 新增组件错误。Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll 路径存在，但因构建失败视为 stale/unverified artifact。未运行测试、网络复制、持久化往返或行为等价验证；仓库没有 P09 专用 focused verifier。文档自检不使用用户禁止的 git diff --check 命令。

## 7. 组件检查点

### C01 PlayerInventoryAndContainerSlots

状态：implemented；PlayerInventorySlotsComponent、PlayerContainerRelationComponent 和 PlayerEquipmentRelationComponent 已实现；组件源码未编译，focused verifier 未运行。

- 组件/边界：PlayerInventorySlotsComponent 保存 59 个玩家侧 ItemEntityRef 和 chest-stack operation marker；PlayerContainerRelationComponent 保存四个命名容器关系和 void-vault capability。ContainerContents/ItemStack/ItemDefinition 不迁入 Player。
- 已写入源码：src/Player/PlayerInventorySlotsComponent.cs。字段为 MainInventorySlots[59]、InventoryChestStackMarkers[59] 和 TrashItem，默认槽位为空引用、标记为 false、垃圾槽为 ItemEntityRef.None。
- 已写入源码：src/Player/PlayerContainerRelationComponent.cs。字段为 Bank、Bank2、Bank3、Bank4 和 VoidVaultState，容器关系默认为空引用；容器内容和容量不进入该组件。
- 已写入源码：src/Player/PlayerEquipmentRelationComponent.cs。字段为 ArmorSlots[20]、DyeSlots[10]、MiscEquipmentSlots[5] 和 MiscDyeSlots[5]，所有关系默认为 ItemEntityRef.None。
- 当前证据：三个源码文件存在且内容已核对；未编译、未运行测试或行为等价验证。Item payload、转移顺序、装备效果、loadout、网络和持久化仍由后续 owner/integration-review 负责。
- System/Query：InventoryCommandSystem 是唯一库存关系写者；InventoryAcceptanceQuery 只计算槽位、容量和顺序；VoidVaultAccessQuery 只读取 capability 与 container facts；InventorySnapshotProjection 输出网络/UI/保存输入。
- seam：ItemTransferCommand、ContainerQuery、PlayerInventoryCommitPort。GetItem 的 coin merge、音频、文本和日志由 adapter 执行，业务计算先返回可审计 action。
- 当前完成证据：P09 报告序号 712、743、744、746-750 均已映射；初始化、IsStackingItems、void vault、GetItem 和容器构造路径已登记。PlayerInventorySlotsComponent 与 PlayerContainerRelationComponent 已保存，focused verifier 尚未运行，组件源码状态为 implemented，整体 verificationStatus 保持 not-verified。

### C02 PlayerEquipmentAndDyeSlots

状态：implemented；PlayerEquipmentRelationComponent 已保存，组件源码未编译，focused verifier 未运行。

- 组件/边界：PlayerEquipmentRelationComponent 只保存 armor[20]、dye[10]、miscEquips[5]、miscDyes[5] 到 Item entity/definition 的玩家关系。ItemInstance、ItemStack、ItemDefinition 和战斗数值留在 Items/Combat 域。
- System/Query：EquipmentCommitSystem 是唯一关系写者；EquipmentQuery 只提供功能装备、外观装备、染料和 misc capability 的不可变视图；EquipmentLoadoutCommand 在校验索引和版本后原子执行当前关系与隐藏偏好交换。
- 关键不变量：EquipmentLoadout.Swap 必须同时交换 armor、dye 和 hide；装备提交后先发布 equipment committed fact，再允许 Buff、颜色和可见选择 projection 读取。不能把 PlayerEquipmentComponent、Items EquipmentComponent、EquipmentRelationComponent 同时作为 authority。
- 副作用边界：网络 slot snapshot、保存 DTO、掉落和日志由 Adapter 负责；装备系统不直接发送网络、不写文件、不创建 Item 内容。无法证明 Version4 保存格式时，保存迁移保持 blocked。
- 当前完成证据：P09 报告序号 708-711 已映射；Version4 Player 声明、初始化、UpdateDyes、PlayerFrame、UpdateArmorSets、EquipmentLoadout.Swap 及玩家 join slot 同步路径已登记。PlayerEquipmentRelationComponent 文件已存在并核对字段容量；focused verifier 尚未运行，组件源码状态为 implemented，整体 verificationStatus 保持 not-verified。

### C03 PlayerBuffAndResourceSlots

状态：implemented；PlayerBuffSlotsComponent、PlayerBuffImmunityComponent、PlayerResourceStateComponent、PlayerHeldItemPresentationStateComponent 和 PlayerEquipmentEffectStateComponent 已保存；组件源码未编译，focused verifier 未运行。

- 组件/边界：PlayerBuffSlotsComponent 保存 44 个成对的 buff type/time 槽；PlayerBuffImmunityComponent 保存 BuffID.Count 资格集合；PlayerResourceStateComponent 保存 breathMax/breath/lavaMax/lavaTime。maxBuffs 是 schema 常量，不是实体状态。
- 已写入源码：src/Player/PlayerBuffSlotsComponent.cs 保存 44 个 BuffSlot；src/Player/PlayerBuffImmunityComponent.cs 由内容目录显式提供免疫槽容量并初始化为 false。未复制 Combat 的伤害免疫或 cooldown。
- 已写入源码：src/Player/PlayerResourceStateComponent.cs 保存 BreathMax、Breath、LavaMax 和 LavaTime，默认值为 200、200、0、0；ignoreWater、lavaVision、lavaOpacity 不进入该 authority component。
- System/Query：BuffResourceSystem 唯一提交 Buff 槽和玩家资源的状态转换；BuffEligibilityQuery 只判断免疫/重复/替换规则；BuffEffectProjection 将 Buff 与装备效果输出给能力、坐骑、Combat 和环境系统。
- 关键不变量：Add、refresh、Del、compact、网络 apply 和清理必须同时移动或清空 buffType 与 buffTime；空槽不得产生效果。buffImmune 不能被当作伤害 cooldown；ResetEffects 的清空与本 tick 的重建顺序必须显式。
- 资源边界：breath/lava 的 tick、环境、伤害和恢复是玩家资源 authority；ignoreWater、lavaVision、lavaOpacity 是从 Buff/装备/环境事实计算的能力或表现 projection，不能成为第二份环境权威。
- 副作用边界：Buff replication、save/load DTO、mount/effect 事件和 UI/音频由 adapter 或 event port 处理；网络 case 50 的类型 payload 和 0 terminator 不能由 Projection 直接写入组件。
- 当前完成证据：P09 报告序号 715-718、720-726 已映射；FindBuffIndex、AddBuff、DelBuff、UpdateBuffs、ResetEffects、case 50 网络输入/输出和资源初始化路径已登记。三个组件文件已存在并核对字段/默认值；focused verifier 尚未运行，组件源码状态为 implemented，整体 verificationStatus 保持 not-verified。

### C04 PlayerEquipmentPresentationState

状态：implemented；PlayerHeldItemPresentationStateComponent、PlayerEquipmentEffectStateComponent、PlayerDisplayEntityModeComponent 和 PlayerVisibleEquipmentSelectionComponent 已保存；组件源码未编译，focused verifier 未运行。

- 组件/边界：PlayerHeldItemPresentationProjection 输出 itemRotation、itemLocation、heldProj 和 lastVisualizedSelectedItem 的不可变兼容快照；PlayerEquipmentEffectProjection 输出七个 armor draw flags、socialShadowRocketBoots、socialGhost、ashWoodBonus、socialIgnoreLight；PlayerDisplayEntityModeComponent 才能拥有 display doll/inanimate 与 hat rack mode。
- 已写入源码：src/Player/PlayerHeldItemPresentationStateComponent.cs 保存 ItemRotation 和 ItemLocation。heldProj 继续由现有 PlayerUseComponent.HeldProjectile 作为兼容链接，lastVisualizedSelectedItem 与完整 Item clone 保持 deferred，不新增第二份 owner。
- 已写入源码：src/Player/PlayerEquipmentEffectStateComponent.cs 保存 11 个装备表现效果布尔状态；没有复制 shroomiteStealth、stealthTimer 或 stealth 的 Combat/Ability authority。
- 所有权保留：shroomiteStealth、stealthTimer、stealth 会影响 Combat、伤害、aggro 和表现，暂不由 P09 新增第二个 authority；最终 owner 标为 integration-review。heldProj 只表示与 Projectile authority 的链接，不复制 Projectile 状态。
- System/Query：ResetDerivedPlayerEffects 先清理上一 tick flags；HeldItemPresentationSystem 读取 item-use、mount、movement 和 network input，输出持有物快照；EquipmentEffectProjectionSystem 读取已提交装备/Buff/appearance facts，输出 renderer snapshot。
- 关键不变量：held item visualization 的 Clone 只是兼容快照，不能把完整 Item 变成持续 Player component；Projection/renderer/audio/UI 不得反写 equipment、inventory 或 Item authority。heldProj、itemRotation 和 itemLocation 在 item end/despawn/reset 路径必须失效或重建。
- 副作用边界：case 41 网络输入、音频、粒子、灯光、渲染和 UI 是 adapter effects；表现 projection 只返回值或 snapshot，副作用执行顺序由外部端别 scheduler 固定。
- 当前完成证据：P09 报告序号 713-714、719、727-742、745 已映射；ResetEffects、UpdateDyes、PlayerFrame、SetArmorEffectVisuals、item use/draw 和 MessageBuffer case 41 路径已登记。713-714、727-735、737-738 已由组件保存，719/736/739/740/745 仍为现有兼容或 integration-review 边界；focused verifier 尚未运行，组件源码状态为 implemented，整体 verificationStatus 保持 not-verified。

### C05 PlayerEquipmentSelectionSlots

状态：implemented；PlayerVisibleEquipmentSelectionComponent 已保存，组件源码未编译，focused verifier 未运行。

- 组件/边界：PlayerVisibleEquipmentSelectionProjection 输出 head、body、legs、coat、handon、handoff、back、front、shoe、waist、shield、neck、face、balloon、backpack、tail、faceHead、faceFlower、faceMask、balloonFront、beard 21 项可见选择快照；不创建第二套装备关系组件。
- 输入与计算：纯 VisibleEquipmentSelectionQuery 读取已提交 armor、hiddenVisibleAccessory、hideMisc、mount、sitting、body/appearance、shield state、HeldItem 和性别/内容定义 facts，重建 PlayerFrame 与 UpdateVisibleAccessories 的优先级和 -1 默认。
- 关键不变量：功能 armor 与 vanity armor 的覆盖顺序、隐藏配饰、披风/背包/尾巴层分类、盾牌抬起和坐骑鞋槽规则必须明确；selection snapshot 不反向写 armor/dye/hidden，也不被 renderer/UI 作为输入 authority。
- 生命周期：每次 frame/appearance/equipment/mount commit 后失效并重算；死亡、展示实体和未解锁槽位使用显式默认值，不读取未初始化的旧快照。
- 已写入源码：src/Player/PlayerVisibleEquipmentSelectionComponent.cs 保存 21 个可见头身、手部、背部、鞋、腰、盾、面部、气球、尾巴和胡须选择字段；全部默认值为 -1。
- 当前实现边界：该组件是可重建选择快照，不拥有 armor、dye、hidden preference 或 renderer 写入；选择规则、坐骑/遮挡优先级和失效时机仍未验证。
- 当前完成证据：P09 报告序号 783-803 已映射；PlayerFrame、ResetVisibleAccessories、UpdateVisibleAccessories、UpdateVisibleAccessory、SetArmorEffectVisuals 的写入/读取路径已登记。组件源码已保存，focused verifier 尚未运行，整体 verificationStatus 保持 not-verified。

### C06 PlayerAppearanceSelectionState

状态：implemented；PlayerAppearanceSelectionComponent 已保存，组件源码未编译，focused verifier 未运行。

- 组件/边界：PlayerAppearanceSelectionComponent 保存 voiceOverride、hideVisibleAccessory[10] 和 hideMisc 这三个外观偏好 authority；PlayerAppearanceFrameProjection 输出 jump、bodyFrame、legFrame，除非 P02/P11 集成决定由其拥有动画状态。
- System/Query：AppearanceSelectionSystem 校验外观命令、loadout hide 交换和网络 bitmask；AppearanceQuery 只提供隐藏偏好；FrameProjection 只根据 movement/equipment/body facts 计算动画输出。
- 关键不变量：hideVisibleAccessory 的 10 个元素必须与 accessory slot 语义一致；hideMisc 的 BitsByte 不能被当作普通 bool 数组；EquipmentLoadout.Swap 必须把隐藏偏好与 armor/dye 一起提交。case 147 的 packet 输入只能转成命令，不直接成为 renderer 写入。
- 持久化边界：Version4 Player Serialize 为空，客户端完整参考只能作为 full-reference-supplemented 字段顺序线索；本轮不把未确认的文件格式或 PlayerFileData 元数据扩展成权威保存组件。
- 已写入源码：src/Player/PlayerAppearanceSelectionComponent.cs 保存 VoiceOverride、HiddenVisibleAccessories[10] 和 packed HideMiscBits；jump、bodyFrame、legFrame 保持 P02/P11 integration-review，不创建重复动画 writer。
- 当前完成证据：P09 报告序号 782、804-808 已映射；804-806 已由组件保存，782/807/808 仍是 P02/P11 动画交接；focused verifier 尚未运行，组件源码状态为 implemented，整体 verificationStatus 保持 not-verified。

### C07 PlayerEquipmentColorProjection

状态：implemented；PlayerEquipmentColorProjectionComponent 已写入，组件源码尚未编译或运行 focused verifier。

- 组件/边界：PlayerEquipmentColorProjection 是无 authority 的单向 projection，输出 cHead、cBody、cLegs、cHandOn、cHandOff、cBack、cFront、cShoe、cWaist、cShield、cNeck、cFace、cFaceHead、cFaceFlower、cFaceMask、cBalloon、cBalloonFront、cBackpack、cTail、cShieldFallback 20 项颜色快照。
- 输入与计算：EquipmentColorQuery 只读已提交 dye、armor、miscDyes、hideVisibleAccessory、visible equipment selection 和 Item armor slot metadata；先执行 UpdateDyes 的清零，再应用 robe、hidden、layer 分类和 shield fallback 规则。
- 关键不变量：cShieldFallback 每次计算先回到 -1；颜色 projection 不写 armor/dye，也不把 dye ID 当作 Item entity ID；颜色快照失效由 equipment/dye/hidden/mount/selection commit 显式触发。
- 副作用边界：shader、renderer、dust、UI 只消费不可变颜色快照；GameShaders 或外部内容 catalog 通过 Adapter 提供，纯颜色计算不直接访问全局渲染状态。
- 已写入源码：src/Player/PlayerEquipmentColorProjectionComponent.cs 保存 CHead、CBody、CLegs、CHandOn、CHandOff、CBack、CFront、CShoe、CWaist、CShield、CNeck、CFace、CFaceHead、CFaceFlower、CFaceMask、CBalloon、CBalloonFront、CBackpack、CTail 和 CShieldFallback；CShieldFallback 默认值为 -1。
- 当前完成证据：P09 报告序号 1326-1342、1346-1348 已映射；Version4 UpdateDyes/UpdateItemDye、visible accessory shield fallback 和颜色读取路径已登记。组件文件存在且字段已核对，focused verifier 尚未运行，整体 verificationStatus 保持 not-verified。

### C08 PlayerDefenseLoadoutAndCloneState

状态：implemented；PlayerDefenseStateComponent、PlayerInteractionLockStateComponent 和 PlayerLoadoutStateComponent 已写入，组件源码尚未编译或运行 focused verifier。

- 组件/边界：PlayerDefenseState 保存 hasRaisableShield、shieldRaised、shieldParryTimeLeft、shield_parry_cooldown；PlayerInteractionLockState 保存 tile interaction lock；PlayerLoadoutState 保存 3 个 loadout 关系和 CurrentLoadoutIndex；hoveredChestIndex 是 UI interaction projection。
- 跨域 owner：hurtCooldowns 和 meleeNPCHitCooldown 属于 Combat cooldown authority 候选，P09 不复制到 PlayerDefenseState；GetItemLogger 是 Item acquisition effect adapter；_visualCloneDummyData、_visualCloneStream、_visualCloneWriter、_visualCloneReader 是调用范围内的 serialization workspace，不能进入持续 ECS Component。
- System/Query：DefenseCommandSystem 处理 guard/parry 状态机和显式 timer tick；PlayerInteractionLockSystem 管理 tile interaction lock 的 acquire/release；LoadoutCommitSystem 在一个事务中交换 armor/dye/hide 并提交 index；VisualCloneAdapter 负责 clone 序列化资源的创建、flush、读取、异常清理和结果返回。
- 关键不变量：shieldRaised 变化只能通过 guard command；parry timer/cooldown 不得出现负值或重复扣减；loadout index 必须在 0..2，Swap 失败时保持旧关系；Combat cooldown、日志和 clone I/O 各自只有一个 owner。
- 副作用边界：chest hover、GetItem logger、网络 loadout packet、文件/内存流、日志和 UI 不进入权威组件；adapter 必须说明重复、失败、超时未知、资源释放和回滚语义。
- 已写入源码：src/Player/PlayerDefenseStateComponent.cs 保存 HasRaisableShield、ShieldRaised、ShieldParryTimeLeft 和 ShieldParryCooldown；src/Player/PlayerInteractionLockStateComponent.cs 保存 LockTileInteractionsTimer；src/Player/PlayerLoadoutStateComponent.cs 保存三个独立的 20/10/10 槽位 loadout、CurrentLoadoutIndex 和 HasValidLoadoutSelection，并在组件自身完成空槽初始化。
- deferred/blocked 成员：hoveredChestIndex 保持 UI/interaction projection；hurtCooldowns 与 meleeNPCHitCooldown 保持 Combat owner；GetItemLogger 保持 Item acquisition effect adapter；_visualCloneDummyData、_visualCloneStream、_visualCloneWriter、_visualCloneReader 保持调用范围内 serialization workspace。上述成员没有被伪造为 P09 持续组件。
- 当前完成证据：P09 报告序号 1402-1416 已映射；Version4 shield guard、tile lock、hover reset、hurt/melee cooldown、TrySwitchingLoadout、EquipmentLoadout.Swap 和 SerializedClone 路径已登记。三个组件文件存在且字段/初始化已核对，focused verifier 尚未运行，整体 verificationStatus 保持 not-verified。

本轮 8 个检查点均已完成设计层审阅；允许范围内的 15 个 ECS 组件源码均已写入 src/Player。顶部 completedComponents 只记录已经保存的实际组件类型；currentComponent 为 none、pendingComponents 为空，不表示对应 System、Query、Adapter、Projection、网络或持久化行为已完成。

## 8. 文档交付边界

- 允许写入：本 P09 的 design/execution 两份 Markdown，以及本次明确授权的 P09 ECS 组件源码。
- 禁止写入：其他分区文档、P09 权威报告、ledger/lock、Test、公共 prompt、System、Query、Command、Adapter、Projection 和其他会话产物。
- 结算状态：已使用原始 `partition=P09` 与 `sessionId=c040060f00534e5fac6f66456c4c1d80` 调用 runner `Fail`；返回 `status=failed`、`lockReleased=true`。`implementationStatus: implemented` 仅表示 15 个允许范围内的组件源码已保存；`verificationStatus: not-verified` 保留，因为 Player 项目被既有 Progression 引用错误阻塞。
