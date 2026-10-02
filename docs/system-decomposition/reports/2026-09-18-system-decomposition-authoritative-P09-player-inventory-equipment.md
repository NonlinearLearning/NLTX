# System Decomposition Report: authoritative P09

partitionId: P09  
taskId: AUTH-SYS-P09  
taskSetName: authoritative-system-decomposition  
sessionId: 811f7b34369747f088123f4f0224b106  
designStatus: proposed  
evidenceStatus: partial  
verificationStatus: not-run  
sourceModified: false  
migrationStatus: not-claimed

## Scope and Evidence

本报告只处理已 claim 的 authoritative P09，覆盖 `PlayerGameplay` 下 8 个叶子组、105 个字段、0 个属性。它是 System/API 拆分建议，不是实现，也不提供行为等价或迁移结果证明。唯一输入、专属 prompt 和授权输出如下：

| 用途 | 路径 |
|---|---|
| 输入分区 | `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P09-Player-Inventory-Equipment.md` |
| claim prompt | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-11-version4-authoritative-20-partition-prompts\2026-09-11-version4-P09-player-inventory-equipment-public-decomposition.md` |
| 唯一输出 | `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P09-player-inventory-equipment.md` |
| 参考项目 | `C:\Users\shan\Downloads\ECS\space-station-14-master` |
| 目标源码 | `D:\TRbackup\Version4` |
| 当前迁移树 | `D:\TRbackup\NLTX\src\NSSLC` |

### Evidence layers

| 层 | 已读证据 | 可支持结论 | 限制 |
|---|---|---|---|
| Version4 源码 | `Terraria\Player.cs`、`EquipmentLoadout.cs`、`Chest.cs`、`MessageBuffer.cs`、`NetMessage.cs`、`Terraria.IO\PlayerFileData.cs` | 字段声明、可见分支、局部写入、网络读写和调用顺序 | 当前 checkout 中 `Serialize`、`Deserialize`、`InternalSavePlayerFile` 等为空或不完整；动态派发、事件、调度、完整保存格式和异常闭包仍不确定 |
| CPG 查询 API | `.agents\skills\ecs-system\tools\CpgEvidence.ps1`，只读 SQLite `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite` | 精确符号、选定源码路径中的静态 call-site、字段使用候选 | `ImportStatus=complete`、967 shards、8,166,789 nodes、71,038,907 edges、1,317 diagnostics；manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；`SourceSnapshotId=null`，未绑定当前源码文件；callable facts 不展开 callee effects，多个访问方向为 `Unknown` |
| 当前 NLTX | `src\NSSLC\Component\Player` 与 `src\NSSLC\Component\Items` 的组件、Query 和状态类型 | 既有候选数据形状、重复边界和可复用 Query | 组件文件存在不等于 System owner、接入或行为迁移；本分区没有找到闭合全部行为的唯一生产 System |
| Space Station 14 | `Content.Shared\Inventory\InventorySystem.cs`、`InventorySystem.Equip.cs`、`InventorySystem.Slots.cs`、`Hands\EntitySystems\SharedHandsSystem.Pickup.cs` | 结构参考：System 可由 partial 文件组织初始化、Query、命令和事件协作；容器插入/移除可发出明确事件 | 不推断 Terraria 语义，不复制 SS14 类型或 API |

### Source identity

本次读取的 Version4 文件 SHA-256：

| 文件 | SHA-256 |
|---|---|
| `D:\TRbackup\Version4\Terraria\Player.cs` | `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86` |
| `D:\TRbackup\Version4\Terraria\EquipmentLoadout.cs` | `2CF422ADB45CF2DB4B9E8EE5146CAB816B459E7C2498F3FAF49E5F3D0179046B` |
| `D:\TRbackup\Version4\Terraria\Chest.cs` | `14EAF2C87C3761E2585C71EA98D6DF2F653354207BFE726D771E5C353F2918B9` |
| `D:\TRbackup\Version4\Terraria\MessageBuffer.cs` | `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE` |
| `D:\TRbackup\Version4\Terraria\NetMessage.cs` | `87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A` |
| `D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs` | `87C429514B9A709968BA35F9DE0F668BFA2FB83861B0ABC828F0521516AB0E04` |

### Query API evidence

Queries were issued through `Initialize-CpgEvidence`, `Start-CpgEvidenceServer`, `Find-CpgSymbols`, `Find-CpgCallSites`, and `Get-CpgMemberUses`; the reader remained read-only.

| 查询对象 | 结果（选定 `Player.cs`、`MessageBuffer.cs`、`NetMessage.cs`、`Terraria.IO/PlayerFileData.cs`） |
|---|---|
| `inventory` | 133 uses: `Player.cs` 125、`MessageBuffer.cs` 4、`NetMessage.cs` 4；所有访问方向在此查询为 `Unknown` |
| `armor` / `dye` / `miscEquips` / `miscDyes` | 分别 39 / 12 / 9 / 9 uses；大多数为 `Unknown` |
| `buffType` / `buffTime` / `buffImmune` | 308 / 87 / 50 uses；大多数为 `Unknown` |
| `Loadouts` / `CurrentLoadoutIndex` | 10 / 4 uses；包含 `NetMessage.cs`，访问方向不能闭合 |
| `hideVisibleAccessory` | 14 uses，含 `MessageBuffer.cs` 与 `NetMessage.cs` |
| `hurtCooldowns` / `meleeNPCHitCooldown` | 8 / 3 uses，均需 Combat integration review |
| `TrySwitchingLoadout(int)` | `MessageBuffer.cs` 1 个 confirmed static call-site |
| `AddBuff(int,int,bool)` / `DelBuff(int)` | Player symbol 分别 15 / 51 个 call-site；这不等于所有动态入口 |
| `UpdateDyes`、`UpdateBuffs`、`UpdateArmorSets`、`PlayerFrame`、`SavePlayer`、`SerializedClone` | 选定范围的 call-site 查询为 `partial`/`NoMatchingFactInScannedScope`；按约定记为未知，不解释为无调用 |
| callable facts | 受 `CalleeEffectsNotExpanded`、`DirectClosureOnly` 和预算限制；不能从返回结果推出完整副作用闭包 |

## Prior Component Decomposition Reconciliation

既有 Component round-2 材料和当前 NLTX 类型只作为候选设计对账，不是运行时迁移事实。文件中的 `status: implemented-isolated-core` 仅表示数据形状曾被写入目标树，不能证明唯一写者、调度接入、网络闭合或行为等价。

| 当前候选 | 观察 | System 处理 |
|---|---|---|
| `PlayerInventoryComponent`、`PlayerInventorySlotsComponent`、`PlayerInventoryState` | 主背包、trash、59 槽 marker 和银行关系有重复表达；`PlayerInventoryState` 还带选中槽/热键派生状态 | proposed 以库存提交不变量为中心；选中槽等输入/派生状态不得再成为第二个库存 authority。具体保留哪一个类型交 `crossSubsystemOwner: integration-review` |
| `PlayerContainerRelationComponent`、`PlayerTrackedContainerLinkComponent`、Items 下 `ContainerComponent` | 玩家只应保存关系/引用；容器内容、容量、转移顺序属于 Items/WorldStorage | `PlayerContainerRelationSystem` 只提交关系；内容和容量通过 Query/Command 交接，不能复制到玩家系统 |
| `PlayerEquipmentComponent`、`PlayerEquipmentRelationComponent`、`PlayerEquipmentState`、Items 下 `EquipmentComponent`/`EquipmentRelationComponent` | armor/dye/misc 槽和 hidden/loadout 在多个类型中重叠，Item payload 和装备效果未闭合 | proposed 一个 `PlayerEquipmentCommitSystem` 写玩家槽位，`PlayerLoadoutSystem` 负责原子交换；Items 装备关系保留为跨分区边界 |
| `PlayerBuffSlotsComponent`、`PlayerBuffComponent`、`PlayerBuffState`、`PlayerBuffImmunityComponent` | Buff 槽和 immunity 有三套近似形状；`buffType`/`buffTime` 配对不变量未由 System 维护 | proposed `PlayerBuffResourceSystem`；只能有一个配对槽位 authority，免疫数组/集合由 definition 与 effect rebuild 协调 |
| `PlayerEquipmentEffectStateComponent`、`PlayerDefenseCapabilityComponent` | 部分 `armorEffect*`/社会效果和防御 capability 已有形状，但缺 reset、重建和消费闭包；当前组件漏掉 `shroomiteStealth` 等 P09 字段 | 作为候选输出/能力快照，不承诺实现；效果和 Combat/Environment owner 标 `crossSubsystemOwner: integration-review` |
| `PlayerVisibleEquipmentSelectionComponent`、`PlayerAppearanceSelectionComponent`、`PlayerEquipmentColorProjectionComponent` | 可见槽、hide bits、颜色投影分别存在，但读写关系和 renderer 接入未闭合 | proposed 单向 Projection；`hideVisibleAccessory` 的权威写入交给 Loadout/appearance command，不允许 Projection 回写装备 |
| `PlayerLoadoutStateComponent` | 三套 loadout 和当前索引已有候选状态，但 ItemEntityRef、网络和持久化没有提交者 | proposed `PlayerLoadoutSystem`，旧 `EquipmentLoadout.Swap` 的双向交换必须保持原子性 |
| `PlayerItemSpaceQuery` | 是只读候选资格判断，能处理个人槽、ammo、coin、void vault 的候选扫描；不执行 `GetItem` 的提交、日志、音效或提示 | 保留为 Query；`PlayerInventoryCommitSystem` 通过显式 command 消费其快照，不能把 Query 当成迁移完成 |

## Conceptual Behaviors

本分区按不变量和可观察行为组织，而不是按 105 个字段各建一个 System。

| 稳定行为 ID | 输入与不变量 | 可观察输出/副作用 | proposed 协作 |
|---|---|---|---|
| `player-inventory-commit` | 59 个个人槽、trash、stack marker；槽位索引、堆叠、favorited/unique/ammo/coin/void-vault 条件必须一次提交 | `GetItem` 返回剩余 Item 或空 Item；库存槽、日志、PopupText、音效、coin merge 和 void vault 结果 | `PlayerItemSpaceQuery` -> `PlayerInventoryCommitSystem` -> Item/Container effect ports |
| `player-container-relation` | `bank`、`bank2`、`bank3`、`bank4`、`voidVaultInfo` 是玩家到外部容器的关系，不等于容器内容 | 关系绑定/解除、容器查询和网络/保存快照 | `PlayerContainerRelationSystem`; contents/capacity/anchor `crossSubsystemOwner: integration-review` |
| `player-equipment-commit` | armor 20、dye 10、misc equipment/dye 各 5；Item 实例只能由 Item owner 定义 | 装备/卸下、slot validation、effects invalidation 和同步意图 | `PlayerEquipmentCommitSystem` + Equipment/Item command port |
| `player-buff-resource` | `buffType[j]` 与 `buffTime[j]` 成对；最大 44 槽；immunity 和 breath/lava 资源不可由 projection 写入 | Buff add/remove/tick、immunity reset、环境能力和 Combat capability 输出 | `PlayerBuffResourceSystem` + Buff definition/Environment/Combat queries |
| `player-equipment-effect-rebuild` | tick 开始清理可重建效果，再由装备/Buff/坐骑条件重建；不能把派生效果当持久 authority | armor effect flags、stealth、social flags、held projection 和 defense capability | `PlayerEquipmentEffectSystem`; effect consumer `integration-review` |
| `player-visible-equipment-projection` | 从已提交装备、dye、hide bits 和定义读取可见 head/body/legs/accessory 与 `c*` 颜色；Projection 单向 | 可见装备选择、颜色快照、renderer/network projection | `PlayerEquipmentProjectionSystem` + read-only `EquipmentDefinitionQuery` |
| `player-appearance-selection` | voice/hide bits/body/leg frame/jump 依赖输入、动画和 loadout；hide bits 与 loadout 交换必须一致 | appearance snapshot、frame/voice projection、network bitmask | `PlayerAppearanceProjectionSystem`; animation/network owner `integration-review` |
| `player-loadout-switch` | 目标索引有效且通过旧前置条件；当前与目标 armor/dye/hide 交换后再提交 index | 原子 loadout swap、CurrentLoadoutIndex、同步/失败结果 | `PlayerLoadoutSystem.Switch(LoadoutSwitchCommand)` |
| `player-defense-interaction` | shield raised/parry/cooldown/tile lock 与输入和 combat timing 一致；hurt/melee cooldown 不可由装备 projection 写 | shield state、parry window、tile interaction lock 和 Combat handoff | `PlayerDefenseInteractionSystem`; `hurtCooldowns`/`meleeNPCHitCooldown` `crossSubsystemOwner: integration-review` |
| `player-persistence-clone-boundary` | 保存/clone 必须保持 item、equipment、buff、loadout 的快照边界；流资源不能被实体 System 共享写 | save request、clone snapshot、失败/恢复结果 | `PlayerPersistenceAndCloneAdapter`; 当前字段序列化 `unknown` |

## State Ownership and Write Closure

下表完整覆盖 claim 输入的 105 个字段。owner 是 proposed，不是已实现或已证明的唯一写者。

| 输入叶子组 | 数量 | 完整成员清单 | proposed owner / 分类 / 证据 |
|---|---:|---|---|
| `PlayerInventoryAndContainerSlots` | 8 | `trashItem`, `inventory`, `inventoryChestStack`, `bank`, `bank2`, `bank3`, `bank4`, `voidVaultInfo` | `inventory`、`trashItem`、marker 归 `PlayerInventoryCommitSystem`；五个 bank/vault 字段只保存 relation，归 `PlayerContainerRelationSystem`。Item payload、容器容量/内容和写入顺序为 `crossSubsystemOwner: integration-review`；CPG 的 inventory 133 uses 未能闭合写者 |
| `PlayerEquipmentAndDyeSlots` | 4 | `armor`, `dye`, `miscEquips`, `miscDyes` | `PlayerEquipmentCommitSystem` 的 authoritative slot state；Item metadata/effects、Network payload、loadout transaction 为 `crossSubsystemOwner: integration-review` |
| `PlayerBuffAndResourceSlots` | 11 | `maxBuffs`, `buffType`, `buffTime`, `buffImmune`, `breathMax`, `breath`, `lavaMax`, `lavaTime`, `ignoreWater`, `lavaVision`, `lavaOpacity` | `maxBuffs` 是 definition/config Query，不是实体写状态；其余归 `PlayerBuffResourceSystem`，但 `ignoreWater`/`lavaVision`/`lavaOpacity` 是环境效果输出，最终消费者与重建顺序 `integration-review` |
| `PlayerEquipmentPresentationState` | 20 | `itemRotation`, `itemLocation`, `heldProj`, `armorEffectDrawShadow`, `armorEffectDrawShadowSubtle`, `armorEffectDrawOutlines`, `armorEffectDrawShadowLokis`, `armorEffectDrawShadowBasilisk`, `armorEffectDrawOutlinesForbidden`, `armorEffectDrawShadowEOCShield`, `socialShadowRocketBoots`, `socialGhost`, `shroomiteStealth`, `ashWoodBonus`, `socialIgnoreLight`, `stealthTimer`, `stealth`, `isDisplayDollOrInanimate`, `isHatRackDoll`, `lastVisualizedSelectedItem` | `PlayerEquipmentEffectSystem`/presentation projection candidate；`itemRotation`、`itemLocation`、`heldProj` 可能属于 Item/Animation；`lastVisualizedSelectedItem` 持有 Item payload，均为 `crossSubsystemOwner: integration-review` |
| `PlayerEquipmentSelectionSlots` | 21 | `head`, `body`, `legs`, `coat`, `handon`, `handoff`, `back`, `front`, `shoe`, `waist`, `shield`, `neck`, `face`, `balloon`, `backpack`, `tail`, `faceHead`, `faceFlower`, `faceMask`, `balloonFront`, `beard` | `PlayerEquipmentProjectionSystem` 只从 armor/definitions 计算并发布；旧 `PlayerFrame` 直接写 head/body/legs 的事实已读到，其他 accessor、renderer、network closure `partial/unknown` |
| `PlayerAppearanceSelectionState` | 6 | `jump`, `voiceOverride`, `hideVisibleAccessory`, `hideMisc`, `bodyFrame`, `legFrame` | `hideVisibleAccessory` 的 authoritative command owner 是 `PlayerLoadoutSystem`/appearance command，不能与 projection 双写；`jump`、frame 和 voice 由 Animation/Network integration 提供，当前分区只保留 projection boundary |
| `PlayerEquipmentColorProjection` | 20 | `cHead`, `cBody`, `cLegs`, `cHandOn`, `cHandOff`, `cBack`, `cFront`, `cShoe`, `cWaist`, `cShield`, `cNeck`, `cFace`, `cFaceHead`, `cFaceFlower`, `cFaceMask`, `cBalloon`, `cBalloonFront`, `cBackpack`, `cTail`, `cShieldFallback` | `PlayerEquipmentProjectionSystem` 单向输出；`UpdateDyes` 明确按 dye/armor/hide 写入，颜色定义与 renderer/network 使用为 `crossSubsystemOwner: integration-review` |
| `PlayerDefenseLoadoutAndCloneState` | 15 | `hasRaisableShield`, `shieldRaised`, `shieldParryTimeLeft`, `shield_parry_cooldown`, `_lockTileInteractionsTimer`, `hoveredChestIndex`, `hurtCooldowns`, `GetItemLogger`, `meleeNPCHitCooldown`, `Loadouts`, `CurrentLoadoutIndex`, `_visualCloneDummyData`, `_visualCloneStream`, `_visualCloneWriter`, `_visualCloneReader` | shield/parry/tile lock 归 `PlayerDefenseInteractionSystem`; `hoveredChestIndex` 属 World/Container handoff；cooldown arrays 属 Combat；`GetItemLogger` 属 Item effect adapter；loadout 归 `PlayerLoadoutSystem`；四个 clone stream 字段只归 `PlayerPersistenceAndCloneAdapter`，当前序列化内容 `unknown` |
| **合计** | **105** | **以上 8 组成员全部覆盖，0 properties** | **所有 owner 仍为 proposed；跨分区状态、ID、网络、持久化、调度和效果闭合保持 `integration-review` 或 `unknown`** |

### Direct and indirect write evidence

- `Player.Update` 当前源码在 `Player.cs:15207-15208` 调用 `ResetEffects`、`UpdateDyes`；随后 `Player.cs:15234-15239` 清空 `buffImmune` 并调用 `UpdateBuffs`；`Player.cs:15284-15296` 由 armor 读取 `head/body/legs` 并更新装备；`Player.cs:15347-15348` 调用 `UpdateArmorSets`。这是顺序证据，不是完整调度闭包。
- `UpdateDyes` 在 `Player.cs:4048-4077` 将 dye/armor/hide 读取为 `c*` 投影；`UpdateItemDye` 还读取 Item metadata，因此颜色输出不能成为装备或 Item payload 的第二 authority。
- `PlayerFrame` 在 `Player.cs:20053-20083` 从 armor 读取可见槽并调用 `UpdateVisibleAccessories`；该方法的所有间接写入和 renderer 消费未由 CPG 完整闭合。
- `TrySwitchingLoadout` 在 `Player.cs:3797-3807` 先检查本地/死亡/索引条件，再执行当前 loadout 与目标 loadout 的两次 `Swap`，最后写 `CurrentLoadoutIndex`。任何拆分都必须保持这三个提交步骤的原子性。
- `GetItem`、`FillAmmo`、`DoCoins` 在 `Player.cs:22930-23089` 直接修改 inventory，调用 `GetItemLogger`、音效、PopupText、coin merge 和 post-action；`PlayerItemSpaceQuery` 只能作为资格 Query，不可代表这些副作用。
- CPG 对 `inventory`、`armor`、`buffType`、`buffTime`、`buffImmune` 的大多数访问返回 `AccessMode=Unknown`/`NoAssignmentEvidence`；因此没有声明唯一写者已经证明。赋值左侧可识别的 `trashItem`、`breath`、`lavaTime`、`ignoreWater`、`CurrentLoadoutIndex` 等只证明局部直接写，不证明全局闭包。

## Boundary Role and Decision

### Proposed boundary decisions

| 决定 | System / 边界 | 选择依据 | 明确拒绝 |
|---|---|---|---|
| `separate` | `PlayerInventoryCommitSystem` | `GetItem` 的槽位扫描、堆叠、coin/ammo/void-vault 分支和日志/提示具有一个提交不变量；Query 先算资格，System 再写入 | 将 Item 完整行为复制到 Player 组件；让 `PlayerItemSpaceQuery` 直接改库存 |
| `separate` | `PlayerContainerRelationSystem` | bank/vault 是玩家到外部容器的关系；内容、容量和世界锚点有独立生命周期 | 把 `Chest.item[]`、容量、WorldStorage registry 纳入玩家系统 |
| `separate` | `PlayerEquipmentCommitSystem` | armor/dye/misc 槽共享装备提交和 Item 引用不变量 | 按单个槽位拆 4 个 System；把 Items `EquipmentComponent` 当玩家唯一 owner |
| `separate` | `PlayerBuffResourceSystem` | buff pair、免疫、呼吸/岩浆资源在 tick 和 command 中共同维护；效果只经 capability 输出 | 让每种 Buff 建独立写者；让 Environment/Combat 直接写槽数组 |
| `separate` | `PlayerEquipmentEffectSystem` | reset/rebuild 是可重建派生状态，需在装备和 Buff 后顺序执行 | 把 armor effect flags 当持久装备字段；由 renderer 回写 authority |
| `separate` | `PlayerEquipmentProjectionSystem` | visible IDs 与 `c*` 颜色均从权威装备/dye/definition 单向投影 | 由 `head/body/legs` 或 `c*` 反推 armor/dye；让 Projection 发送 command |
| `partial` with integration contract | `PlayerAppearanceProjectionSystem` | frame/voice/hide bits 依赖 Animation、Network 和 Loadout；P09 不足以宣布跨域唯一 owner | 直接合并 `PlayerAppearanceSelectionComponent` 和 renderer state 为双向组件 |
| `separate` | `PlayerLoadoutSystem` | 三套 armor/dye/hide 的交换有明确事务边界，旧入口只有 `MessageBuffer` 一处 confirmed call-site | 用多个独立数组写者；先写 CurrentLoadoutIndex 再交换槽位 |
| `separate` with integration contract | `PlayerDefenseInteractionSystem` | shield/parry/tile lock 是局部状态机；hurt/melee cooldown 和 chest hover 不属于同一 authority | 用一个“PlayerDefense”吞并 Combat cooldown、World chest 和 Item logger |
| `separate` | `PlayerPersistenceAndCloneAdapter` | Save/clone 涉及文件、流和异常资源，必须隔离副作用；当前格式不完整 | 让 ECS System 直接持有 BinaryReader/Writer 或静态 MemoryStream |

## System API and Legacy Behavior Mapping

以下 API 是提案，名称不表示目标树中已存在。只允许通过 immutable Query、显式 Command 和一次 commit 写入权威状态。

| Legacy entry / behavior | proposed composition | 输入/输出与关键兼容条件 |
|---|---|---|
| `Player.Update(int)` | `PlayerTickCoordinator`（只做调度） -> `PlayerEquipmentEffectSystem.ResetForTick` -> `PlayerEquipmentProjectionSystem.RebuildDyeProjection` -> `PlayerBuffResourceSystem.Advance` -> `PlayerEquipmentEffectSystem.Rebuild` -> `PlayerLoadoutSystem`/Defense projection | 保持源码已读顺序 `ResetEffects -> UpdateDyes -> clear buffImmune -> UpdateBuffs -> UpdateArmorSets`；scheduler barrier、外部 `UpdateEquips` 全闭包 `unknown` |
| `Player.GetItem(Item, GetItemSettings)` | `PlayerItemSpaceQuery.Evaluate` -> `PlayerInventoryCommitSystem.TryCommitPickup` -> `IItemEffectsPort` | 输出 `remainingItem`、slot delta、coin/ammo/void-vault decision、effect intents；旧 unique/favorited/coin/ammo 扫描与失败结果必须保留 |
| `Player.FillAmmo` | `PlayerInventoryCommitSystem.TryFillAmmo` | 只提交 54-57 ammo 槽；日志、声音、PopupText、`DoCoins` 由 effect port 依序发出 |
| `Player.DoCoins` | `PlayerInventoryCommitSystem.MergeCoins` | 保留 100-stack 升级、递归 merge 和 air reset；递归深度/异常结果 `unknown` |
| `Player.AddBuff` / `DelBuff` | `PlayerBuffResourceSystem.Apply` / `Remove` | 输入 buff definition/id、time、fromNet；输出 slot delta 与 effect capability invalidation；网络重放/重复 command 语义需验证 |
| `Player.UpdateBuffs` | `PlayerBuffResourceSystem.Advance` + `BuffDefinitionQuery` | 保留 `buffType[j] > 0 && buffTime[j] > 0` 配对和逐槽顺序；mount/environment/Combat effects 只通过 capability port |
| `Player.UpdateDyes` / `UpdateItemDye` | `PlayerEquipmentProjectionSystem.ProjectColors` | 读取 armor/dye/hide 和 Item metadata，输出 immutable `EquipmentColorSnapshot`；不写回装备或 dye |
| `Player.PlayerFrame` / `UpdateVisibleAccessories` | `PlayerEquipmentProjectionSystem.ProjectVisibleSlots` + `PlayerAppearanceProjectionSystem.ProjectFrames` | 先选择 armor vanity/functional slot，再发布 visible ids/frame；死亡、robe、mount 和 renderer 规则需保留，当前间接调用闭包 `partial` |
| `Player.TrySwitchingLoadout(int)` | `PlayerLoadoutSystem.TrySwitch(LoadoutSwitchCommand)` | 校验 `whoAmI`/本地状态/死亡/索引后，事务内交换 armor、dye、hide，最后提交 index；失败不得部分修改 |
| `Player.SavePlayer` | `PlayerPersistenceAndCloneAdapter.Save(PlayerPersistenceSnapshot)` | `Main.Achievements.Save`、map save、server-side 条件和异常包装是 effect boundary；字段编码/恢复格式 `unknown` |
| `Player.SerializedClone` | `PlayerPersistenceAndCloneAdapter.CloneVisualState` | 使用隔离 stream 的 proposed adapter；当前 `Serialize`/`Deserialize` 内容为空，不能声称 clone 字段闭合 |
| `MessageBuffer` loadout/accessory/buff handlers | `NetworkAdapter.Parse` -> validated Command -> corresponding System -> response Projection | `TrySwitchingLoadout` 的一处 call-site 已 confirmed；网络字段 bitmask、authority、重放、拒绝响应需 integration review |
| `NetMessage` player item/loadout/buff publishing | `NetworkProjection.BuildSnapshot` -> transport | 只读 committed snapshot；不要让 NetMessage 成为 authority 或绕过 System 写数组 |

### Query / Command / Adapter contracts

| 类型 | proposed contract | 约束 |
|---|---|---|
| Query | `PlayerItemSpaceQuery`、`EquipmentDefinitionQuery`、`BuffDefinitionQuery`、`PlayerInventorySnapshotQuery`、`PlayerDefenseSnapshotQuery` | 只读、无 lazy initialization、返回值快照；发现缓存写入时升级为 `partial` |
| Command | `PickupItemCommand`、`EquipItemCommand`、`ApplyBuffCommand`、`SwitchLoadoutCommand`、`SetShieldCommand`、`SetAppearanceCommand` | 由唯一 owner 校验并一次 commit；command payload 不拥有 Item/Container 定义 |
| Projection | `EquipmentColorSnapshot`、`VisibleEquipmentSnapshot`、`BuffCapabilitySnapshot`、`DefenseSnapshot`、`NetworkPlayerSnapshot` | 单向读取 authority；不得通过 setter、事件回写玩家状态 |
| Adapter | `ItemEffectPort`、`ContainerRelationPort`、`PlayerNetworkAdapter`、`PlayerPersistenceAndCloneAdapter`、`RendererSink` | 转换外部 ID/流/网络/渲染；不复制领域不变量，不在未验证输入上分配实体或写组件 |

## Call and Dependency DAG

### Confirmed or source-observed edges

```text
Player.Update
  -> ResetEffects
  -> UpdateDyes
  -> clear buffImmune[]
  -> UpdateBuffs
  -> read armor[0..2] into head/body/legs
  -> UpdateEquips
  -> UpdateArmorSets

MessageBuffer loadout packet
  -> Player.TrySwitchingLoadout(index)
  -> EquipmentLoadout.Swap(current)
  -> EquipmentLoadout.Swap(target)
  -> CurrentLoadoutIndex = index

GetItem
  -> FillAmmo (when candidate fits ammo)
  -> occupied-slot / empty-slot commits
  -> DoCoins
  -> void-vault fallback
  -> GetItemLogger / PopupText / SoundEngine / HandlePostAction

NetMessage
  -> inventory/armor/dye/misc/loadout/buff/accessory snapshot reads
  -> network transport
```

The first two sequences are directly visible in the listed source ranges; the third is visible in `Player.cs:22930-23089`. CPG call-site evidence confirms one static `MessageBuffer.cs` call to `TrySwitchingLoadout`; CPG returns `partial` for several `Update*` and persistence entry call-site queries. No zero result is interpreted as no relationship.

### Proposed target DAG

```text
definition/catalog Query
  -> validated inventory/equipment/buff/loadout command
  -> PlayerInventoryCommitSystem / PlayerEquipmentCommitSystem / PlayerContainerRelationSystem
  -> PlayerLoadoutSystem atomic swap (when requested)
  -> PlayerBuffResourceSystem reset/advance
  -> PlayerEquipmentEffectSystem rebuild
  -> PlayerEquipmentProjectionSystem and PlayerAppearanceProjectionSystem
  -> NetworkProjection / PersistenceAdapter / RendererSink
```

This is a proposed order, not a proven scheduler graph. A frame barrier is required between authority commit and projections. `Player.Update`'s current source order is a must-preserve constraint; the order relative to Main, Item, Combat, Environment, Network flush, persistence, and renderer scheduling is `unknown`.

### Unresolved edges

- `UpdateDyes`/`UpdateBuffs`/`UpdateArmorSets`/`PlayerFrame` inbound callers: query result `partial` in selected scope; enumerate scheduler, event and dynamic callers before implementation.
- Item metadata and `Item` methods called by `GetItem`, `UpdateItemDye`, `DoCoins`: callee effects and alias mutation not closed.
- Buff effect consumers, mount/environment writes, Combat damage immunity, and renderer reads: `crossSubsystemOwner: integration-review`.
- `MessageBuffer` and `NetMessage` protocol registration, authority split, retry, malformed packet behavior and client prediction: `unknown`.
- Persistence/clone serializer registration and field schema: `unknown`; current bodies do not provide a positive closure.
- Entity IDs, ItemInstanceId, legacy slot, runtime entity ID, persistent container ID and network slot are distinct concepts; mapping is `crossSubsystemOwner: integration-review`.

## Lifecycle and Side Effects

| 阶段 | 当前证据 | proposed owner / 缺口 |
|---|---|---|
| Create/defaults | Player field initializers create arrays, banks and loadouts; `Chest.CreateBank` creates bank instances; `EquipmentLoadout` creates item arrays | Creation should attach one authority set and validate lengths; constructor/rehydration ordering and empty Item initialization are only partially read |
| Activate/session bind | P09 fields are attached to a player entity/session, but session-to-entity and active slot are outside this partition | `PlayerInventoryCommitSystem` must receive an explicit entity/session handle; `sessionId`, network slot and persistent identity cannot be conflated; owner `integration-review` |
| Tick/update | `Player.Update` sequence above is source-confirmed locally | Coordinator and barriers proposed; full Main scheduler, client/server split and reentrancy unknown |
| Inventory/equipment command | `GetItem`/`FillAmmo`/`DoCoins` mutate arrays and emit logger/audio/text | Commit System owns atomic state delta; effect ports must run after accepted commit in source order; retry/idempotency unknown |
| Buff/resource tick | `UpdateBuffs` loops 44 slots and writes capability fields; `buffImmune` is cleared before it | Buff System owns pair and resource state; capability publication to Environment/Combat requires integration contract |
| Loadout switch | `MessageBuffer.cs` invokes `TrySwitchingLoadout`; `EquipmentLoadout.Swap` swaps arrays and hide flags | Loadout System transaction; invalid/dead/local restrictions and network acknowledgment require focused verification |
| Projection | `UpdateDyes` and `PlayerFrame` read equipment/dye/hide and write visible/color fields | Projection consumes immutable snapshot; renderer/network may read only; current projection lifecycle unknown |
| Save | `SavePlayer` saves achievements/map and conditionally invokes `InternalSavePlayerFile`; catches and rethrows after error display | Adapter owns file/map side effects; player field encoding missing in current source, status `unknown` |
| Clone | `SerializedClone` uses static stream/writer/reader around empty serializer methods | Adapter boundary only; static stream concurrency, reset, failure and field coverage unknown |
| Network receive/publish | `MessageBuffer` writes buff slots/accessory bits and calls loadout; `NetMessage` reads item/loadout/buff arrays for output | Parse/validate -> command -> commit; no direct array writes in adapter; packet schema and versioning `unknown` |
| Reset/destroy/unload | No complete P09-specific entity destroy/unload closure found in inspected scope | Define cleanup for Item relations, bank refs, Buff slots, projection snapshots and clone streams; multi-world/session cleanup `unknown` |
| Exception/retry | `GetItem` effects and `SavePlayer` exception wrapper visible; command retries not defined | Commit result must distinguish rejected, partially applied, and effect failure; rollback/duplicate command policy `unknown` |

### Side-effect inventory

`GetItem`/`FillAmmo`/`DoCoins` can call `PlayerGetItemLogger`, `SoundEngine.PlaySound`, `PopupText.NewText`, `GetItemSettings.HandlePostAction`, Item mutation methods, and recursive coin merge. `SavePlayer` can call achievements, map, protected file invocation and error presentation. `MessageBuffer`/`NetMessage` perform binary protocol I/O. These are adapters/effect ports, not hidden Query writes.

## Integration Handoff

All unresolved shared ownership is explicitly handed off; this partition does not finalize cross-partition owners.

| Handoff | Required decision/evidence | Current disposition |
|---|---|---|
| Item ownership and IDs | Item payload, stack/prefix/favorited semantics, ItemInstanceId vs RuntimeEntityId, transfer atomicity | `crossSubsystemOwner: integration-review`; blocking for inventory/equipment implementation |
| Containers and banks | `Chest` content/capacity, bank creation, void vault relation, world/storage anchor, transfer order | `crossSubsystemOwner: integration-review`; P09 only owns relation projection |
| Buff/effect boundary | Buff definitions, immunity rebuild, mount/environment effects, Combat capability and reset order | `crossSubsystemOwner: integration-review`; no second writer allowed |
| Equipment/loadout | armor/dye/misc slot owner, hidden accessory exchange, three-loadout persistence/network framing | proposed `PlayerEquipmentCommitSystem` + `PlayerLoadoutSystem`; final type reconciliation required |
| Appearance/renderer | visible slot IDs, c* colors, frame/voice/hide bits, renderer read contract | `crossSubsystemOwner: integration-review`; Projection must remain one-way |
| Defense/Combat | shield/parry vs hurt/melee cooldowns, tile interaction lock, hovered chest | shield local state proposed; cooldown arrays and chest hover handed to Combat/World integration |
| Network | inbound authority, packet validation, replay/idempotency, outbound snapshot ownership | `crossSubsystemOwner: integration-review`; current call-site closure partial |
| Persistence/clone | save format, server-side character branch, serializer schema, stream isolation and reload | `unknown`; current Version4 serializer bodies are empty |
| Scheduler/session | Main tick phase, barriers, session/entity binding, multi-world cleanup | `unknown`; use original session boundary and explicit entity handle |
| Current NLTX duplicate components | choose one authority per overlapping inventory/equipment/buff type; remove or adapt redundant shapes only after evidence | `crossSubsystemOwner: integration-review`; no source files changed in this report |

## Migration Behavior Contract

本节只规定后续授权迁移应观察什么；没有执行迁移，也没有证明新旧行为等价。

| 行为 | 输入 -> 输出 | 必须保持的顺序/错误/副作用 | focused observation |
|---|---|---|---|
| Inventory pickup | `PickupItemCommand` + immutable inventory/container snapshot -> accepted slot delta + remaining item | unique/favorited/coin/ammo/void-vault 分支；失败不改槽位；accepted commit 后按旧顺序发 logger/audio/text/post-action | 59 槽边界、堆叠、prefix、coin merge、void-vault fallback、重复 command |
| Equipment commit | equip/unequip command + Item definition -> slot delta + effect invalidation | armor/dye/misc length和槽位限制；Item payload不复制；commit原子 | slot index、重复装备、swap、Item relation、effect invalidation |
| Buff/resource | apply/remove/tick + definition -> paired slot/resource delta + capability snapshot | `buffType`/`buffTime` 配对，44 槽扫描顺序，immunity reset before effects；拒绝输入无部分写 | timer、免疫、breath/lava、环境效果、网络重放 |
| Loadout switch | validated index + current/target loadout -> swapped active set + index | 旧前置条件、current swap、target swap、index assignment 的事务顺序；失败不部分交换 | 3 loadout armor/dye/hide 原子性、index invalid、dead/local reject |
| Visible projection | committed equipment/dye/hide + definitions -> visible IDs/colors | Projection 无写回；robe/vanity/hide/shield fallback 和颜色覆盖规则保持 | visible IDs、`c*` colors、state-before/after 不变、renderer snapshot |
| Appearance/frame | movement/animation/loadout input -> frame/voice/hide snapshot | jump/bodyFrame/legFrame 的动画顺序和网络 bitmask 保持；owner 尚需整合 | frame transitions、voice/hide roundtrip、dead/mount cases |
| Defense interaction | shield input + combat/tile context -> shield/parry/lock delta | shield state machine与cooldown不越权；Combat arrays由Combat owner提交 | raise/lower、parry window、tile lock timeout、cooldown handoff |
| Network apply/publish | bytes/committed snapshot -> validated command/framed bytes | parse before commit；坏包不部分写；outbound只读 committed snapshot | packet roundtrip、authority、duplicate/replay、recipient set、ordering |
| Persistence/clone | save/clone request + committed snapshot -> file/clone result | 保存异常包装、server-side branch、stream reset；未知字段不得静默丢弃 | field coverage、reload, clone isolation, failure/retry, concurrent sessions |

## Evidence Gaps and Blocking Decisions

| 缺口/阻塞 | 状态 | 实施前必须完成 |
|---|---|---|
| CPG manifest 没有绑定当前 Version4 per-file snapshot；索引和当前源码可能漂移 | `partial` | 重新绑定同一 revision 或对每个关键 source span 做 hash/line reconciliation |
| CPG 对多数数组访问返回 `Unknown`，callable facts 不展开 alias/callee effects | `partial` | 针对 inventory/equipment/buff/loadout 建 focused read/write/alias 查询和源码复核 |
| `UpdateDyes`、`UpdateBuffs`、`UpdateArmorSets`、`PlayerFrame` 的 inbound/scheduler closure 不完整 | `unknown` | 枚举 Main、event、network、virtual/dynamic callers；没有负证据推断 |
| `Serialize`、`Deserialize`、`InternalSavePlayerFile` 当前为空或不完整 | `unknown` | 获取同一 Version4 接受版本的完整保存/clone schema；在此之前禁止宣称持久化闭合 |
| Item、Chest、Buff definition、Combat、Environment、Renderer、Network 的跨分区 owner 未定 | `crossSubsystemOwner: integration-review` | 共同决定 ID、事件、snapshot 和唯一写者；P09 不得单方面吸收相邻域 |
| NLTX 同时存在重复 inventory/equipment/buff state shapes，且未见闭合 System 接入 | `partial` | 做类型级 authority 选择和全树 writer search，再进入实现；本报告不删除或改写它们 |
| session/entity/persistent/network ID 关系未闭合 | `unknown` | 以 session-scoped entity handle 设计 API；明确重连、跨 world、保存恢复语义 |
| 异常、重试、取消、重复 network command 和多 world 隔离 | `unknown` | 定义 commit/result/error contract 和 cleanup barrier |

## Verification Plan

`verificationStatus: not-run`。本轮没有运行构建、测试、focused verifier、运行时行为验证或迁移检查；没有修改 `src/`、`Test/`、项目文件、输入报告、prompt 或其他分区报告。

后续在得到实现授权且完成 integration handoff 后，至少需要以下 focused verifier：

1. `PlayerInventoryCommitSystem`：59 槽与 54-57 ammo/coin 边界、stack/prefix/favorited/unique、trash、void-vault fallback、coin merge、拒绝输入无部分写、effect 顺序。
2. `PlayerEquipmentCommitSystem` 与 `PlayerLoadoutSystem`：20/10/5/5 长度、重复装备、三套 loadout 的 armor/dye/hide 原子交换、invalid/dead/local 拒绝、唯一写者扫描。
3. `PlayerBuffResourceSystem`：44 槽 pair invariant、timer、免疫清理与重建顺序、breath/lava 资源边界、Environment/Combat capability 一致性。
4. Projection：`UpdateDyes`/`PlayerFrame` 观察向量、visible IDs、`c*` colors、hide/robe/shield fallback；验证 projection 前后 authority 不变。
5. Network：MessageBuffer 输入、NetMessage 输出、坏包/重复包/重放、authority、recipient 和字段顺序；不允许 adapter 直接写 authority。
6. Persistence/clone：完整 save/reload schema、server-side branch、stream isolation、失败重试和所有 P09 字段覆盖；当前源代码不足时保持 `unknown`。
7. Scheduler/lifecycle：tick barrier、session/entity binding、create/reset/destroy/unload、multi-world、disconnect/reconnect 和 cleanup。

结论仍是 proposed 静态 System/API 报告。完成本报告只代表 P09 文档会话可以结算，不代表代码已迁移、行为已等价或结果已验证。
