# Version4 P06 玩家战斗、伤害、防御、状态与资源组件拆分设计

partitionId: P06
sessionId: fd85b62742c142878ee572f953f944e2
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P06-Player-Combat-Status.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P06-player-combat-status-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P06-player-combat-status-component-execution.md
designStatus: proposed
executionStatus: failed
implementationStatus: partial
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: focused-c22-c01-c02-c03-c04-c05-passed-c06-c07-c08-c09-c10-c11-c12-c13-c14-c15-c16-c17-c18-c19-c20-c21-source-not-verified
completedComponents: [C22.PlayerDpsTelemetryCore, C01.PlayerStringAndAccessoryEffectState, C02.PlayerCombatDamageProcState, C03.PlayerCombatDodgeAndImmunityState, C04.PlayerCombatBarrierAndRegenState, C05.PlayerManaAndAfkStatus, C06.PlayerDebuffAndRecoveryStatus-source, C07.PlayerDetectionAndCombatStatus-source, C08.PlayerSocialAndDefenseState-source, C09.PlayerFrameAndImmunityState-authority-source, C10.PlayerVitalAndRegenState-source, C11.PlayerCombatModifierAndImmunityState-source, C12.PlayerAmmoAndAccessoryEffects-source, C13.PlayerElementalAndShimmerStatus-source, C14.PlayerSurvivalAndTransformationState-source, C15.PlayerDebuffStatusState-source, C16.PlayerAccessoryCombatModifierState-source, C17.PlayerAccessoryResourceAndInvulnerabilityState-source, C18.PlayerAccessoryDebuffAndDropState-source, C19.PlayerCombatDamageAndCritModifiers-source, C20.PlayerCombatSpeedRangeAndPermissionState-source, C21.PlayerLuckAndCommerceEffects-source]
currentComponent: C22.IntegrationHandoff
pendingComponents: [C09.PresentationAndTownHandoff, C22.NpcProjectileIntegration, C22.LifecycleIntegration, C22.ProjectionIntegration]
lastCheckpointUtc: 2026-09-12T09:16:41.6470356Z
evidence-gap: C01-C05 和 C22 本地 focused verifier 已通过；C06-C21 组件源码已保存但没有本次任务允许的 focused verifier，且 C09 叶子组仍因 presentation/Town handoff 为 partial。C10 与现有 Health/Mana 的唯一 writer、C02/C03/C04/C05/C06/C19/C22 的跨分区 damage/immunity/vital owner、P04 lifecycle、regen commit root、World spawn/eligibility、network/persistence DTO、runtime registration 和 projection registration 仍未闭合。
blocking-decision: integration-handoff
关键阻塞边界：PlayerGameplay 与 CombatAndStatus 的生命/免疫/资源 owner；ItemContainerAndEconomy 与 P06 的 Item 引用和装备效果 owner；ProjectileSimulation/NpcAndTownSimulation 的伤害输入和免疫键；Liquid/World/Movement 对 shimmer、重力、环境和速度字段的 owner；NetworkSession、Persistence、ClientPresentation 对快照和副作用的单向边界。

## 1. 范围和状态口径

本分区属于正式父级 PlayerGameplay，只覆盖权威报告列出的 22 个叶子子系统、254 个字段和 0 个属性。它描述玩家实体作为战斗参与者的权威状态、状态转换输入和对外投影边界，不创建或接管 NPC、Projectile、全局 Combat、库存内容、世界事件或客户端渲染的完整 owner。

本文件中的 Component、System、Query、Command、Adapter 和 Projection 仍以 proposed 设计为基线；当前已落地 C22 本地 core、C01-C05 的既有受限组件 checkpoint，以及 C07-C21 的组件源码 checkpoint，并在文末记录实际文件和证据。当前 NLTX 文件只作为 existing-evidence 或 partial 映射，不能被本文件的提案名称重新解释为全分区已完成实现。本轮不修改权威报告、索引、TSV、Version4 或完整参考源码。

设计原则：

- 按共同读写者、变更原因、生命周期和语义内聚度拆分，而不是按字段数量机械切文件。
- 一个权威字段只有一个 Owner System/CommitPort；Query 不写回，Projection 不反向驱动战斗。
- 一次性意图使用 Command，已经提交的事实使用 Event，网络、存档、日志、时钟和表现使用 Adapter/Projection。
- EntityReference、内容定义 ID、旧数组槽位、网络 ID、持久账户 ID 分开建模。
- 系统先后关系写入显式 scheduler contract，不依赖文件或目录顺序。

本轮不把以下内容当作已验证：行为等价、API 兼容、网络闭合、持久化闭合、proposed 文件已创建、verifier/build/test/运行时回放已通过。

## 2. 证据登记

| 来源 | 实际核对内容 | 可支持的结论 | evidenceStatus |
|---|---|---|---|
| P06 权威分区报告 | D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P06-Player-Combat-Status.md:7-723；22 个叶子组起点为 :89、:120、:152、:174、:195、:222、:255、:283、:320、:348、:378、:411、:440、:478、:510、:535、:561、:583、:608、:639、:665、:695 | 254 个字段、声明类型、默认值、原始分组和正式父级 | confirmed |
| Version4 Player 字段声明 | D:\TRbackup\Version4\Terraria\Player.cs:537-816、:904-976、:1029-1031、:1351-1410、:1712-1853、:2018-2032、:2475-2484、:26518-26570 | P06 字段的可见性、默认值、Buff/免疫数组和运行时形状 | confirmed |
| Version4 Player 生命周期 | D:\TRbackup\Version4\Terraria\Player.cs:9948-10074、:10272-10740、:14789-15617、:26518-26570 | 死亡重置、效果重置、玩家 Tick、初始化和清理入口 | confirmed for located paths |
| Version4 Player 伤害与资源 | D:\TRbackup\Version4\Terraria\Player.cs:10847-11318、:11960-12020、:22208-22379 | 生命/魔力积算、受伤资格、免疫窗口、生命扣减和死亡边界 | confirmed for located paths |
| Version4 Buff 与网络 | D:\TRbackup\Version4\Terraria\Player.cs:3541-3728、:4300-5350；D:\TRbackup\Version4\Terraria\MessageBuffer.cs:652、:771、:789、:1406、:1919、:1926、:2018、:2524、:3265；D:\TRbackup\Version4\Terraria\NetMessage.cs:994-998、:2338-2360、:2644-2653 | Buff 槽位操作及网络入口已定位；完整逐字段恢复闭包仍为 partial | partial |
| Version4 持久化边界 | D:\TRbackup\Version4\Terraria\Player.cs:26394-26460；D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs | SavePlayer、SerializedClone、文件元数据边界；直接 Serialize/Deserialize 当前为空实现 | partial; persistence blocked |
| tModLoader stable public mirror | D:\TRbackup\tmodloader-api-docs-stable\index.html；版本 tModLoader v2026.07；D:\TRbackup\tmodloader-api-docs-stable\class_player.html#ac31b2c55747dc1b540e5b60282f58623、#a62a65413a898e003e73d8e9e38f6d0d8、#a9b28a808c81cd0f90ae16efbe196f0d1、#aa0ceee81c432120bde935869d62f944e | AddBuff、AddImmuneTime、statLife、statMana 和 Hurt 的公开边界；不证明 Version4 私有顺序或存档格式 | confirmed-public-boundary |
| SS14 ECS 只读参考 | C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Wires\WiresComponent.cs:6-66；WiresSystem.cs:43-59、301、394、465、613；Content.Shared\Atmos\Components\MapAtmosphereComponent.cs:8-23 | Component 保存状态、System 处理更新/事件、世界状态与投影分离的组织参考 | organization-only |
| 当前 NLTX Combat 骨架 | dome\src\Terraria.Dome.Simulation\Components\Entity\HealthComponent.cs:3-14；Combat\Components\HealthRegenerationComponent.cs:5-27、DefenseComponent.cs:5-19、ImmunityComponent.cs:3-16、HitImmunityComponent.cs:5-155、ManaComponent.cs:5-26 | 已有生命、再生、防御、免疫和魔力局部模型；不代表 P06 全量覆盖 | existing-evidence/partial |
| 当前 NLTX Player/Status 骨架 | dome\src\Terraria.Dome.Simulation\Player\Components\PlayerDefenseStateComponent.cs:3-25、PlayerDodgeStateComponent.cs:3-38、PlayerCooldownStateComponent.cs:3-83、PlayerBuffImmunityStateComponent.cs:5-39；StatusEffects\Components\BuffCollectionComponent.cs:6-83 | 已有玩家防御、闪避、冷却、Buff 免疫和 Buff 集合；唯一 owner 与 P06 映射未闭合 | existing-evidence/partial |
| 当前 NLTX Systems/Projection | dome\src\Terraria.Dome.Simulation\Combat\Systems\DamageCalculationSystem.cs:6-52、DamageResolutionSystem.cs:8-47、PlayerVitalRegenSystem.cs:10-69；dome\src\Terraria.Dome.Server\Replication\PlayerStateProjection.cs:10-52；Server\Protocol\PlayerPersistentStateMapper.cs:9-169 | 已有显式 System、Command、生命/魔力/Buff projection；不能替代兼容验证 | existing-evidence/partial |

## 3. 总体边界模型

Player entity 组合以下能力：生命/资源、Buff/状态效果、伤害与免疫、防御与装备能力、环境/变身、速度/权限、幸运/商业效果和可选 telemetry。P06 不定义一个包含 254 个字段的 PlayerCombatStateComponent。

数据流：

Player/NPC/Projectile/World 输入
→ DamageRequest 或状态 Command
→ 资格 Query 和确定性计算 Query
→ 唯一 Player commit root
→ committed damage/status/vital facts
→ network/persistence/presentation projection

跨 PlayerGameplay、CombatAndStatus、ItemContainerAndEconomy、ProjectileSimulation、NpcAndTownSimulation、LiquidSimulation、Movement、Network、Persistence 和 Presentation 的 owner 在本文件中统一标记 crossSubsystemOwner: integration-review。

## 4. 检查点总览

| checkpoint | 权威叶子组 | 成员 | 初步 proposed 边界 | 主要状态类型 |
|---|---|---:|---|---|
| C01 | PlayerStringAndAccessoryEffectState | 14 | PlayerAccessoryStringEffectComponent + effect rebuild system | 装备效果输入 |
| C02 | PlayerCombatDamageProcState | 15 | PlayerCombatProcStateComponent + proc system | 命中触发/计时 |
| C03 | PlayerCombatDodgeAndImmunityState | 5 | PlayerDodgeAndImmunityStateComponent + eligibility query | 闪避/受击资格 |
| C04 | PlayerCombatBarrierAndRegenState | 4 | PlayerBarrierAndRegenComponent | 屏障/再生能力 |
| C05 | PlayerManaAndAfkStatus | 10 | PlayerManaActivityComponent + policy definition | 魔力病/AFK |
| C06 | PlayerDebuffAndRecoveryStatus | 16 | PlayerDebuffRecoveryStateComponent | 恢复/临时减益 |
| C07 | PlayerDetectionAndCombatStatus | 11 | PlayerCombatDetectionStateComponent | 探测/战斗输入 |
| C08 | PlayerSocialAndDefenseState | 20 | Social/ghost/interaction/defense seams | 社交/防御混合组 |
| C09 | PlayerFrameAndImmunityState | 11 | PlayerHitFrameImmunityComponent + frame projection | 无敌帧/表现帧 |
| C10 | PlayerVitalAndRegenState | 13 | Vital/LifeRegen/ManaRegen components | 生命/魔力 |
| C11 | PlayerCombatModifierAndImmunityState | 16 | Combat modifier + environment permission query | 防御/伤害/环境 |
| C12 | PlayerAmmoAndAccessoryEffects | 12 | Ammo/accessory effect component + query | 弹药/配饰 |
| C13 | PlayerElementalAndShimmerStatus | 21 | Elemental component + shimmer adapter | 元素/微光 |
| C14 | PlayerSurvivalAndTransformationState | 15 | Survival/transformation component | 生存/变身 |
| C15 | PlayerDebuffStatusState | 8 | Debuff component or status alias seam | 快速减益别名 |
| C16 | PlayerAccessoryCombatModifierState | 9 | Accessory combat capability component | 配饰战斗能力 |
| C17 | PlayerAccessoryResourceAndInvulnerabilityState | 5 | Resource protection component + definition | 资源/保护 |
| C18 | PlayerAccessoryDebuffAndDropState | 8 | Accessory debuff/drop intent seam | 减益/掉落意图 |
| C19 | PlayerCombatDamageAndCritModifiers | 14 | Damage/critical modifier component | 伤害/暴击 |
| C20 | PlayerCombatSpeedRangeAndPermissionState | 9 | Speed/permission component + query | 速度/权限 |
| C21 | PlayerLuckAndCommerceEffects | 13 | Luck/commerce state + query | 幸运/商业 |
| C22 | PlayerDpsTelemetryState | 5 | Combat telemetry component + clock port | 观测状态 |

## 5. 全局不变量和不拆分项

- 生命归零产生死亡转换输入，PlayerVitalStateComponent 不拥有死亡/复活/掉落。
- HealthComponent、ManaComponent、PlayerVital/Mana components 不能并行写同一玩家的同一资源。
- Buff type/time 必须成对提交；Buff immunity 不等于剩余时间；快速 debuff 别名只能有一个 writer。
- Item 字段只保存 typed reference/relation，不嵌入 Item 可变 payload；库存数量和物品定义归 Item/Container owner。
- immune/hurtCooldowns 是资格状态；immuneAlpha、frame counters 和透明度是 projection/表现状态。
- Query 只做确定性计算和资格判断，System 才写组件；Projection 不反写 authority。
- 不为单个 Buff、单个伤害事件、单个免疫标记、单个 UI 资源条或单个 DPS 样本创建组件。
- 不使用 Shared/Components/Common/Misc/Manager 作为泛化目录；目录和文件顺序不表达运行时顺序。

## 6. 当前状态声明

截至本次保存，C01-C22 的成员归属和执行计划已追加；C07-C21 的可独立确定组件源码已保存，其中 C09 仅完成 authority source，C01-C05 与 C22 本地 focused verifier 证据保持原记录。C09 presentation/Town handoff、C22 外部事件/生命周期/projection 接入、运行时注册、网络和持久化仍未实现或验证。没有修改权威报告、索引、TSV、Version4 或其他会话文档。

## 7. C01 PlayerStringAndAccessoryEffectState

成员覆盖：extraAccessorySlots、extraAccessory、tankPet、tankPetReset、stringColor、counterWeight、vanityCounterWeight、magicString、yoyoString、yoyoGlove、rapidAttackBonus、stressBall、stressBallPrevious、staffOfRegrowthBonus。输入报告为 14/14，属性 0/0。

源码事实：

- Version4 Player.cs:537-563 声明全部 14 个字段，其中 rapidAttackBonus 为 private，其余为 public；tankPet 默认 -1，extraAccessorySlots 默认 2。
- Player.cs:6349-6402 的 Counterweight 读取 yoyoGlove、counterWeight 和 vanityCounterWeight，说明这些字段共同参与投射物/配饰效果，但不拥有 Projectile 实体。
- Player.cs:8035、:8358-8413 的装备检查路径写入 vanityCounterWeight、stringColor、yoyoString、counterWeight、yoyoGlove、magicString 和 stressBall。
- Player.cs:6928-6932 比较 stressBall 与 stressBallPrevious 并更新上一帧值；Player.cs:10277-10292 衰减 rapidAttackBonus 并由 extraAccessory 及模式派生 extraAccessorySlots；Player.cs:10429-10437 清理大部分短期配饰效果；Player.cs:14955-14963 处理 tankPet 的一次性重置。

proposed 边界：

| 类型 | proposed 责任 | 状态分类 | 写入边界 |
|---|---|---|---|
| PlayerAccessoryStringEffectComponent | 保存玩家侧 yoyo/string/counterweight/stress/staff capability facts；Item payload 使用 typed reference | authoritative capability state | PlayerAccessoryEffectRebuildSystem |
| PlayerAccessoryEffectRebuildSystem | 在装备能力提交后重建字段并执行 transient reset | state transition | EquipmentCommitPort；不得直接改 Item |
| PlayerAccessoryCapabilityQuery | 读取配饰能力，给 Player/Projectile/Item 查询使用 | pure query | no write-back |
| PlayerVisualEffectProjection | 输出 stress/ice-like frame or string color 等表现快照 | projection | presentation adapter only |

不变量和风险：

- extraAccessorySlots 是由 extraAccessory 和游戏模式派生的值，不应由外部输入直接写；其默认声明值与 ResetEffects 基线需要单独 verifier。
- counterWeight、vanityCounterWeight、stringColor 只能保存定义/关系结果，不复制 Item 实例；ItemContainerAndEconomy/P09 是 integration-review owner。
- stressBall 与 stressBallPrevious 必须在同一 tick 的重建/观察顺序中保持可解释，重复装备重算不得重复触发效果。
- tankPet 是玩家到宠物/投射物能力的关系，不是 P06 的实体生命周期；PlayerProgression/Projectile 的消费交由 integration-review。
- rapidAttackBonus 使用 Tick 衰减，时间输入需由显式 simulation tick 提供，不能在 Query 中读取系统时钟。

当前 NLTX 映射和计划：

- 当前未找到与这 14 个 Version4 字段一一对应的单一组件；PlayerEquipmentModifierStateComponent 只保存 MeleeScaleGlove，PlayerEquipmentInventoryComponent/Item 能力由 P09 维护，属于 partial existing-evidence。
- proposed 目标路径为 dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerAccessoryStringEffectComponent.cs、Player/Combat/Systems/PlayerAccessoryEffectRebuildSystem.cs 和 Player/Combat/Queries/PlayerAccessoryCapabilityQuery.cs。
- focused verifier：默认值与 ResetEffects 基线、装备重建幂等、stressBall previous/current 边沿、counterweight 选择顺序、tankPet reset，以及无 Item payload 复制。

检查点结论：C01 设计完成；本次已保存本地 C01 core 并通过 focused verifier。P09 Item capability owner、P05/P15 tank pet consumer 和 P11 presentation 仍为 integration-review，不以 C01 本地验证宣称跨域接入完成。

### C01 本地实现 checkpoint

本次实现已将 C01 的本地、无外部 Item payload 依赖部分保存到根 `src`：

- `src/Player/PlayerAccessoryStringEffectComponent.cs` 保存 14 个 C01 字段；`TankPet` 保持 `-1` 默认值，`ExtraAccessorySlots` 保持 `2` 默认值。
- `src/Player/PlayerAccessoryEffectRebuildInput.cs` 使用不可变能力输入，不持有 `Terraria.Item`、Projectile 或 Buff runtime object。
- `src/Player/PlayerAccessoryEffectRebuildSystem.cs` 是唯一 writer，执行装备能力重建、ResetEffects 基线、`rapidAttackBonus` 的 `0.005f` tick 衰减、`stressBall` edge memory 和 `tankPet` 两阶段 reset。
- `src/Player/PlayerAccessoryCapabilityQuery.cs` 与 `PlayerAccessoryCapabilitySnapshot.cs` 只读输出能力快照，并保持 vanity counterweight 优先于普通 counterweight；不写回 Component。
- `src/PlayerAccessoryVerification/Program.cs` 与 `Terraria.PlayerAccessoryVerification.csproj` 建立 focused verifier，覆盖默认值、重复重建、transient reset、stressBall edge、counterweight precedence、tankPet reset、spawn reset 和 Query 无写入。

本 checkpoint 已完成 C01 本地验证：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 构建 `src/PlayerAccessoryVerification/Terraria.PlayerAccessoryVerification.csproj`，退出码 0，0 个警告、0 个错误；focused run 使用同一 wrapper 的 `run --project --no-build --no-restore`，退出码 0，输出 `PASS: player accessory string effects rebuild, reset, query and edge semantics`。产物为 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 和 `Build/bin/Terraria.PlayerAccessoryVerification/Debug/net10.0/Terraria.PlayerAccessoryVerification.dll`。P09 装备 owner、P05/P15 tank pet consumer 和 P11 presentation 仍保持 integration-review；本地实现没有注册 Projectile、复制 Item payload 或接管玩家生命周期。

## 8. C02 PlayerCombatDamageProcState

成员覆盖：lifeSteal、ghostDmg、eocDash、eocHit、infernoCounter、starCloakCooldown、onHitDodge、onHitRegen、onHitPetal、onHitTitaniumStorm、titaniumStormCooldown、hasTitaniumStormBuff、petalTimer、boneGloveTimer、phantomPhoneixCounter。输入报告为 15/15，属性 0/0。

源码事实：

- Version4 Player.cs:642-644、:688-690、:736-739、:798-816 声明全部字段；lifeSteal 默认 99999f，eocHit 默认 -1。
- Player.cs:4525 写入 hasTitaniumStormBuff，:4643 读取 infernoCounter，:12575-12646 管理 eocDash/eocHit 的冲刺命中窗口，说明这些字段由移动/命中流程共同消费。
- Player.cs:10373、:10441-10445 清理部分装备/on-hit flags；:14876-14903 更新 ghostDmg/lifeSteal；:14983-15029 更新 inferno、titaniumStormCooldown 和 starCloakCooldown；:15173-15183 更新 petalTimer/boneGloveTimer。
- Player.cs:15349 以 shadowDodge 和 onHitDodge 组合判断效果，说明 proc 状态必须读取已提交的闪避事实，不能独立重算伤害。

proposed 边界：

| 类型 | proposed 责任 | 状态分类 | 写入边界 |
|---|---|---|---|
| PlayerCombatProcStateComponent | 保存命中后触发标志、计时器、冲刺窗口和生命窃取运行态 | authoritative behavior state | PlayerCombatProcSystem |
| PlayerCombatProcSystem | 消费 committed hit/damage facts，更新计时和 proc state | state transition | PlayerCombatCommitPort |
| PlayerCombatProcQuery | 读取当前 proc capability，供 Projectile/Item/Combat 使用 | pure query | no write-back |
| PlayerCombatProcProjection | 输出需要复制或表现的受击/触发事实 | projection | Network/Presentation adapter |

不变量和风险：

- lifeSteal、ghostDmg 等效果必须只由已接受的命中事实驱动，不能因重复网络 DamageRequest 重复触发。
- eocDash/eocHit 的计时和实体命中关系不能以数组槽位替代稳定 EntityReference；Projectile/Movement owner 为 integration-review。
- onHit flags 与 shadowDodge、barrier、regen 的提交先后必须固定；被拒绝命中不应更新 proc。
- timer tick 使用显式 simulation tick；不得以 DateTime 或表现帧推进战斗状态。
- starCloak、boneGlove 等 Item 字段/能力只通过 ItemCapabilityQuery，Player 不拥有 Item payload。

当前 NLTX 映射和计划：

- 当前 PlayerCombatProcStateComponent 不存在；PlayerCooldownStateComponent 仅有 ShadowDodgeTimer 等通用冷却，Combat systems 有局部 damage resolution，属于 partial existing-evidence。
- proposed 目标路径为 dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerCombatProcStateComponent.cs、Player/Combat/Systems/PlayerCombatProcSystem.cs 和 Player/Combat/Queries/PlayerCombatProcQuery.cs。
- focused verifier：重复命中幂等、eocDash/eocHit 计时、onHit flag 只在 committed hit 后变更、lifeSteal upper bound、counter expiry、ResetEffects 清理和 proc projection 单向性。

检查点结论：C02 的本地、无外部 runtime 对象依赖 core 已实际保存并通过 focused verifier；跨分区 committed-hit caller、EOC target reference、生命周期、network/persistence 和 projection 接入仍保持 integration-review，不能据此宣称跨域迁移完成。

### C02 本地实现 checkpoint

本 checkpoint 实际保存了以下 `src` 源码：

- `src/Player/PlayerCombatProcStateComponent.cs` 保存 15 个 C02 字段；`LifeSteal` 保持 `99999f` 默认值，`EocHit` 保持 `-1` sentinel。
- `src/Player/PlayerCommittedCombatHitEvent.cs` 保存 `EventId`、稳定 `SourceId`、伤害、source revision、committed 阶段和 proc effect 输入；不持有 NPC、Projectile、Item 或 Buff runtime object。
- `src/Player/PlayerCombatProcHitEffects.cs` 保存已提交命中的 ghost/life-steal/on-hit effect 输入。
- `src/Player/PlayerCombatProcSystem.cs` 是唯一 writer，执行 committed/positive-damage 校验、空 ID 和负 revision 拒绝、EventId 去重、ghost/life-steal 更新、expert/普通 recovery cap、EOC 窗口、inferno/timer tick、ResetEffects 和生命周期 reset。
- `src/Player/PlayerCombatProcQuery.cs` 与 `src/Player/PlayerCombatProcSnapshot.cs` 只输出不可变快照，不修改 component。
- `src/PlayerCombatProcVerification/Program.cs` 的 reset 断言明确验证新生命周期清空 replay history 后可重新接受同一 EventId；这与跨生命周期去重边界一致。

验证证据：

- 红阶段：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 构建 `src/PlayerCombatProcVerification/Terraria.PlayerCombatProcVerification.csproj`，在生产类型尚不存在时退出码 1，6 个预期 CS0246 缺失类型错误。
- Build：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建同一项目，退出码 0，0 个警告、0 个错误；产物为 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 和 `Build/bin/Terraria.PlayerCombatProcVerification/Debug/net10.0/Terraria.PlayerCombatProcVerification.dll`。
- Focused verifier：通过同一 wrapper 以 `run --project --no-build --no-restore` 执行，退出码 0，输出 `PASS: player combat proc committed-hit, timer and query semantics`。

未验证和依赖影响：

- P12/P15 尚未提供真实 committed damage caller、稳定 target EntityReference、source revision 生成和跨分区 replay/去重接入；C02 只提供本地 seam。
- P04 lifecycle 尚未调用本地 `Reset`；NetworkSession、Persistence、ClientPresentation 尚未接入 `PlayerCombatProcSnapshot`，没有创建 DTO、adapter 或运行时注册。
- `starCloakCooldown` 的随机 dust/sound 表现和 `phantomPhoneixCounter` 的 Projectile 发射 owner 保持外部 integration-review；C02 不拥有这些副作用。

## 9. C03 PlayerCombatDodgeAndImmunityState

成员覆盖：blackBelt、brainOfConfusionItem、brainOfConfusionDodgeAnimationCounter、shadowDodge、shadowDodgeTimer。输入报告为 5/5，属性 0/0。

源码事实：

- Version4 Player.cs:712、:785-793、:812 声明五个字段；brainOfConfusionItem 是 Terraria.Item 引用，不能原样进入 ECS 组件。
- Player.cs:4513、:8694-8721 写入 shadowDodge、blackBelt 和 brainOfConfusionItem；Player.cs:22096-22113 的 ShadowDodge 方法设置无敌/闪避相关效果并将 animation counter 设为 300。
- Player.cs:9961 清零 brainOfConfusionDodgeAnimationCounter；:10438、:10593、:10633 清理 shadowDodge、Item 引用和 blackBelt；:10758-10760、:15177-15179 递减动画/闪避计时；:15349 结合 onHitDodge 读取 shadowDodge。

proposed 边界：

| 类型 | proposed 责任 | 状态分类 | 写入边界 |
|---|---|---|---|
| PlayerDodgeAndImmunityStateComponent | 保存 black belt/shadow dodge 能力、剩余时间和 typed ItemReference | authoritative combat capability | PlayerDodgeCommitSystem |
| PlayerDamageEligibilityQuery | 以 DamageRequest、cooldown key 和 dodge state 判断资格 | pure query | no write-back |
| PlayerDodgeCommitSystem | 在一次 Damage commit 中消费闪避并发布 committed dodge fact | state transition | PlayerDamageCommitPort |
| PlayerDodgePresentationProjection | 输出 brain dodge animation counter 等表现状态 | projection | ClientPresentation adapter |

不变量和风险：

- ShadowDodge 只能由一次已授权的 Dodge/IncomingDamage command 消费；重发不能重复消费或延长窗口。
- brainOfConfusionItem 只能是 ItemReference/能力关系，不能复制完整 Item。
- blackBelt、shadowDodge 和一般 immune/hurtCooldowns 的优先级需在统一 PlayerDamageEligibilityQuery 中固定。
- animation counter 不得参与权威受伤资格；表现投影不可反写 shadowDodge。

当前 NLTX 有 PlayerDodgeStateComponent、PlayerCooldownStateComponent、ApplyShadowDodgeCommand 和 Combat ImmunityComponent，属于局部 existing-evidence；尚未形成 P06 玩家闪避与全局 Combat 免疫的单一 writer。

proposed 路径：dome/src/Terraria.Dome.Simulation/Player/Combat/PlayerDodgeAndImmunityStateComponent.cs、Player/Combat/Queries/PlayerDamageEligibilityQuery.cs、Player/Combat/Systems/PlayerDodgeCommitSystem.cs。

focused verifier：重复 Dodge command、窗口消费、blackBelt 与一般 immunity 优先级、ItemReference 不泄漏、animation projection 不写回和死亡/重生清理。

检查点结论：C03 的本地 typed ItemReference、闪避提交和纯资格/投影边界已实际保存并通过 focused verifier；统一 Combat immunity owner、P15 damage caller、P04 lifecycle、兼容 adapter 和 network/persistence 接入仍保持 integration-review。

### C03 本地实现 checkpoint

本 checkpoint 实际保存了以下 `src` 源码：

- `src/Player/PlayerDodgeAndImmunityStateComponent.cs` 保存 5 个 C03 字段；`brainOfConfusionItem` 使用 `ItemEntityRef`，不复制 `Terraria.Item`。
- `src/Player/PlayerDodgeCapabilityInput.cs`、`PlayerDodgeActivationCommand.cs`、`PlayerDodgeCommitCommand.cs`、`PlayerDamageEligibilityInput.cs` 和 `PlayerDamageEligibilityRejectionReason.cs` 定义稳定值类型输入、command id 和 source revision。
- `src/Player/PlayerDodgeCommitSystem.cs` 是唯一 writer，执行能力重建、ShadowDodge activation 去重、committed damage 消费去重、timer tick、ResetEffects 和 lifecycle reset。
- `src/Player/PlayerDamageEligibilityQuery.cs` 与 `PlayerDamageEligibilityResult.cs` 只根据显式输入判断资格；一般免疫、空 source、非正伤害和 source cooldown 在消费前拒绝，动画 counter 不参与资格。
- `src/Player/PlayerDodgePresentationProjection.cs` 与 `PlayerDodgePresentationSnapshot.cs` 为单向纯投影。
- `src/PlayerDodgeVerification/Program.cs` 覆盖默认值、typed relation、重复 activation/damage、拒绝命中无写入、timer、projection purity 和 lifecycle reset。

验证证据：

- 红阶段：wrapper build 在生产类型尚不存在时退出码 1，12 个预期缺失类型/成员错误。
- Build：`Invoke-SerialDotnet.ps1` 串行构建 `src/PlayerDodgeVerification/Terraria.PlayerDodgeVerification.csproj`，退出码 0，0 个警告、0 个错误；产物为 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 和 `Build/bin/Terraria.PlayerDodgeVerification/Debug/net10.0/Terraria.PlayerDodgeVerification.dll`。
- Focused verifier：同一 wrapper 以 `run --project --no-build --no-restore` 执行，退出码 0，输出 `PASS: player dodge eligibility, committed consumption and projection semantics`。

未验证和依赖影响：统一 `ImmunityComponent` 与 P06 dodge state 的最终 owner、P15/P12 incoming damage caller、P04 lifecycle caller、旧 API adapter、network/persistence/projection registration 未接入；C04 是下一个组件。

## 10. C04 PlayerCombatBarrierAndRegenState

成员覆盖：iceBarrier、iceBarrierFrame、iceBarrierFrameCounter、palladiumRegen。输入报告为 4/4，属性 0/0。

源码事实：

- Version4 Player.cs:761、:789、:791、:796 声明四个字段。
- Player.cs:4521 写入 palladiumRegen；:4855-4864 按生命阈值更新 iceBarrier、frame counter 和 12 帧循环；:10439、:10444 清理 palladiumRegen/iceBarrier；:11156 读取 palladiumRegen 参与生命再生；:22657-22658 在受伤/相关清理路径重置状态。

proposed 边界：

- PlayerBarrierAndRegenComponent：只保存 iceBarrier、palladiumRegen 及权威屏障状态；
- PlayerBarrierFrameProjection：只输出 iceBarrierFrame/iceBarrierFrameCounter 的表现快照；
- PlayerBarrierSystem：在装备/生命快照提交后更新屏障资格和再生能力；
- PlayerLifeRegenSystem：消费 palladiumRegen capability，不把屏障表现帧当生命 authority。

不变量：

- iceBarrier 的生命阈值判断使用同一个已提交 PlayerVital snapshot；frame counter 每 tick 最多按 Version4 规则推进一次。
- palladiumRegen 只能影响 regen calculation input，不直接修改生命；生命写入仍属于唯一 vital commit root。
- 受伤/死亡/重生清理的顺序必须明确，不能由 projection 重置 authority。
- iceBarrierFrame 是表现数据，不能让客户端输入或 renderer 反写。

当前 NLTX 只有 HealthRegenerationComponent、PlayerVitalRegenSystem 和若干 Player equipment state，未找到完整 barrier owner，状态为 partial。proposed 路径：Player/Combat/PlayerBarrierAndRegenComponent.cs、Player/Combat/Systems/PlayerBarrierSystem.cs、Player/Combat/PlayerBarrierFrameProjection.cs。

focused verifier：生命半值边界、frame 循环、受伤清理、regen contribution、重生清理和 projection 单向性。

检查点结论：C04 的本地屏障阈值、再生能力输入和表现帧投影已实际保存并通过 focused verifier；生命写入、统一 regen commit root、P04 lifecycle、P09 Buff/equipment owner、network/persistence 和 presentation registration 仍保持 integration-review。

### C04 本地实现 checkpoint

本 checkpoint 实际保存了以下 `src` 源码：

- `src/Player/PlayerBarrierAndRegenComponent.cs` 保存 `iceBarrier`、`iceBarrierFrame`、`iceBarrierFrameCounter` 和 `palladiumRegen` 四个 C04 字段。
- `src/Player/PlayerBarrierCapabilityInput.cs` 使用已提交生命快照和能力事实，不持有 Player、Buff 或 UI runtime object。
- `src/Player/PlayerBarrierSystem.cs` 是唯一 writer：按 `CurrentLife * 2 <= EffectiveLifeMaximum` 和 Buff/能力有效性提交屏障，按 Version4 的 3 tick cadence 推进 12 帧循环，单独提交 palladium regeneration capability，并提供 ResetEffects/lifecycle reset；不写生命、不推进再生累计器。
- `src/Player/PlayerBarrierFrameProjection.cs` 与 `PlayerBarrierFrameSnapshot.cs` 只读输出表现帧，不反写 authority。
- `src/PlayerBarrierVerification/Program.cs` 覆盖默认值、半血/高于半血边界、frame counter、12 帧回绕、屏障与 palladium 独立清理、纯 projection 和 lifecycle reset。

验证证据：

- 红阶段：wrapper build 在生产类型不存在时退出码 1，10 个预期 CS0246 缺失类型错误。
- Build：`Invoke-SerialDotnet.ps1` 串行构建 `src/PlayerBarrierVerification/Terraria.PlayerBarrierVerification.csproj`，退出码 0，0 个警告、0 个错误；产物为 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 和 `Build/bin/Terraria.PlayerBarrierVerification/Debug/net10.0/Terraria.PlayerBarrierVerification.dll`。
- Focused verifier：同一 wrapper 以 `run --project --no-build --no-restore` 执行，退出码 0，输出 `PASS: player barrier threshold, regen capability and frame projection semantics`。

未验证和依赖影响：P09 Buff/equipment 的真实 palladium/ice barrier source、C10 唯一生命/再生 commit root、P04 death/respawn caller、Network/Persistence/Presentation registration 未接入；C05 是下一个组件。

## 11. C05 PlayerManaAndAfkStatus

成员覆盖：manaSickTime、manaSickLessDmg、manaSickReduction、manaSick、afkCounter、AFKTimeNeededForNoWormSpawns、AFKTimeNeededForNoLuckyStars、afkCounterForKiting、manaRegenBonus、manaRegenDelayBonus。输入报告为 10/10，属性 0/0。

源码事实：

- Version4 Player.cs:622-636、:674-676 声明字段；manaSickTime/manaSickLessDmg/两个 AFK 阈值是 static/readonly policy，而不是每个玩家独立 authority。
- Player.cs:4374-4375、:8570-8571 由装备/能力路径增加 manaRegenDelayBonus 和 manaRegenBonus；:4727-4728 根据 Buff 时间计算 manaSickReduction；:10571、:10627-10628 重置对应 transient/derived 状态。
- Player.cs:11264-11281 在 UpdateManaRegen 中消费 regen bonuses；:15215-15229 更新 AFK counters；:15532-15534 让 manaSickReduction 影响 magicDamage。Main.cs:406-407 也在玩家状态清理路径重置 AFK counters。

proposed 边界：

- PlayerManaActivityComponent：manaSick、manaSickReduction、afkCounter、afkCounterForKiting；
- ManaActivityPolicyDefinition：manaSickTime、manaSickLessDmg、AFK thresholds；
- PlayerManaRegenModifierComponent：manaRegenBonus、manaRegenDelayBonus，供 C10 的 mana regen owner 消费；
- PlayerActivityQuery：基于显式 input/tick 判断 AFK 资格，不写状态；
- ManaStatusSystem：消费 Buff/装备事实并更新 sickness/regen capability。

不变量：

- static/readonly policy 不注册为每个 Player 的可变 component。
- manaSickReduction 只能从已提交 Buff time/definition 计算，不能被网络客户端直接设置。
- AFK counters 的 tick/reset 依赖明确的 input edge 和调度阶段，不能在 Spawn Query 中写回。
- manaRegenBonus 只影响 regen calculation，不直接修改 statMana；mana sickness 对 damage modifier 的影响顺序须由 C10/C19 verifier 固定。

当前 NLTX 的 ManaComponent/PlayerVitalRegenSystem 只覆盖 current/max 和局部 regen，未覆盖 P06 的 sickness/AFK policy，状态 partial。proposed 路径为 Player/Combat/PlayerManaActivityComponent.cs、Definitions/ManaActivityPolicyDefinition.cs、Systems/ManaStatusSystem.cs、Queries/PlayerActivityQuery.cs。

focused verifier：policy constant/threshold、mana sickness ratio、bonus reset、AFK counter reset on input, pause and death、mana damage modifier ordering、重复 Buff command。

检查点结论：C05 组件边界已按设计落地；三个组件文件均已存在于 `D:\TRbackup\NLTX\src\Player`，focused verifier 已通过。ManaStatusSystem、PlayerActivityQuery 及跨分区 StatusEffects/World/C10/C19 交接仍未在本任务范围内实现，因此 C05 的组件实现完成不等于整个 C05 行为迁移闭合。

### C05 本地实现 checkpoint

实际源码文件：

- `D:\TRbackup\NLTX\src\Player\PlayerManaActivityComponent.cs`
- `D:\TRbackup\NLTX\src\Player\ManaActivityPolicyDefinition.cs`
- `D:\TRbackup\NLTX\src\Player\PlayerManaRegenModifierComponent.cs`

字段覆盖与边界：

- `PlayerManaActivityComponent` 保存 `manaSick`、`manaSickReduction`、`afkCounter` 和 `afkCounterForKiting`，并提供效果重置与生命周期重置契约。
- `ManaActivityPolicyDefinition` 保存不可变的 `manaSickTime`、`manaSickLessDmg`、`AFKTimeNeededForNoWormSpawns` 和 `AFKTimeNeededForNoLuckyStars` 定义；默认值分别为 `300`、`0.25f`、`300` 和 `10800`。
- `PlayerManaRegenModifierComponent` 保存 `manaRegenBonus` 和 `manaRegenDelayBonus`，只作为 C10 魔力回复计算输入，不直接写入魔力。

依赖与未闭合项：StatusEffects 仍是 mana sickness 的 Buff 事实来源；C10 是魔力回复提交根；C19 负责 damage modifier 顺序；World/Spawn 负责 AFK 资格相关环境事实；P09 负责装备输入。当前没有新增或修改这些非组件依赖，也没有把其行为塞入组件。

实际验证：

- Build：`pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerManaActivityVerification\Terraria.PlayerManaActivityVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`；退出码 `0`，警告 `0`，错误 `0`。
- Focused run：`pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 run --project .\src\PlayerManaActivityVerification\Terraria.PlayerManaActivityVerification.csproj --no-build --no-restore /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`；退出码 `0`，输出 `PASS: player mana sickness, regeneration modifiers, AFK activity and lifecycle semantics`。
- 产物：`D:\TRbackup\NLTX\Build\bin\Terraria.PlayerManaActivityVerification\Debug\net10.0\Terraria.PlayerManaActivityVerification.dll`。

未验证项：跨分区 writer/reader 唯一性、网络/持久化/表现投影注册、World spawn 交接和 C10/C19 行为闭合仍未验证。

## 12. C06 PlayerDebuffAndRecoveryStatus

成员覆盖：chilled、dazed、frozen、stoned、ichor、webbed、tipsy、noBuilding、miscCounter、sandStorm、crimsonRegen、ghostHeal、ghostHurt、sticky、slippy、slippy2。输入报告为 16/16，属性 0/0。

源码事实：

- Version4 Player.cs:656-670、:734、:741-749 声明字段；:4484-4500、:5958、:6032、:6087 等 Buff/effect 路径写入 debuff flags。
- Player.cs:10320、:10620-10626、:10687-10689 清理 noBuilding、主 debuff 和 regen/heal flags；:11016、:11136-11184 读取 crimsonRegen 参与生命再生和受击恢复；:11361-11497、:15565-15573、:15721 等路径用 sticky/dazed/chilled 等改变控制与移动。
- Player.cs:13327、:14378-14399 处理 sandStorm、sticky、slippy、slippy2 的环境/地面输入；:17985-17992 更新 miscCounter。

proposed 边界：

- PlayerDebuffRecoveryStateComponent：保存 chilled/dazed/frozen/stoned/ichor/webbed/tipsy/noBuilding/regen flags 等玩家权威状态；
- PlayerSurfaceMovementStateComponent：只保存 sticky/slippy/slippy2/sandStorm 的玩家接触/环境结果，最终 movement owner 为 integration-review；
- PlayerMiscCounterComponent：若 verifier 证明是 simulation phase state，则独立保存 0..299 counter，否则降级为 transient projection；
- PlayerDebuffRecoverySystem：消费 Buff/environment facts，更新状态并发布 status events；
- PlayerRecoveryQuery：计算 crimsonRegen/ghostHeal/ghostHurt 对资源恢复的资格，不写生命。

不变量：

- Buff-derived flags 不能与 BuffCollectionComponent 形成两个 writer；状态别名必须有明确失效/重建点。
- sticky/slippy 与位置/速度分离，不由 P06 直接写 Movement authority。
- miscCounter 的时间单位、wrap 和消费者必须先确认；不能把它误当作系统时钟或表现帧。
- ghostHeal/ghostHurt、crimsonRegen 只提供 recovery capability，不越过 PlayerVital commit root。

当前 NLTX 有 StatusEffects BuffCollection/BuffEffect、PlayerMovement/Environment components，但没有 P06 全量 debuff owner，状态 partial。proposed 路径：Player/Combat/PlayerDebuffRecoveryStateComponent.cs、Player/Combat/PlayerSurfaceMovementStateComponent.cs、Systems/PlayerDebuffRecoverySystem.cs、Queries/PlayerRecoveryQuery.cs。

focused verifier：Buff alias rebuild/clear、debuff movement gates、sticky/slippy environment input、miscCounter wrap、crimsonRegen recovery ordering、death/reset cleanup。

检查点结论：C06 设计完成；三个组件源码已保存，但由于本任务禁止新增 verifier、System、Query、Adapter 或测试，C06 仍为 source-implemented/not-verified，不能宣称行为迁移或 runtime integration 完成。

### C06 本地源码 checkpoint

实际源码文件：

- `D:\TRbackup\NLTX\src\Player\PlayerDebuffRecoveryStateComponent.cs`
- `D:\TRbackup\NLTX\src\Player\PlayerSurfaceMovementStateComponent.cs`
- `D:\TRbackup\NLTX\src\Player\PlayerMiscCounterComponent.cs`

字段覆盖与边界：

- `PlayerDebuffRecoveryStateComponent` 保存 `chilled`、`dazed`、`frozen`、`stoned`、`ichor`、`webbed`、`tipsy`、`noBuilding`、`crimsonRegen`、`ghostHeal` 和 `ghostHurt`，`ResetEffects` 清除 Buff-derived/恢复能力事实，`ResetForLifecycle` 重用该清理契约。
- `PlayerSurfaceMovementStateComponent` 保存 `sandStorm`、`sticky`、`slippy` 和 `slippy2`，只表达环境/接触结果，不拥有位置或速度。
- `PlayerMiscCounterComponent` 保存 `miscCounter`，只提供生命周期清零；没有猜测其消费者、时间单位或 wrap 规则。

依赖与未闭合项：StatusEffects 仍是减益别名的事实来源；Movement/Spatial/Liquid/World 提供表面输入；C10 持有生命提交根；ClientPresentation 只读表现结果。未创建 `PlayerDebuffRecoverySystem`、`PlayerRecoveryQuery` 或其他非组件类型，也未建立第二个 Buff writer。

验证状态：源码路径和 16 个字段归属已检查；本 checkpoint 没有合法的 C06 focused verifier，故 verificationStatus 为 `not-verified`。`miscCounter` wrap、Buff alias rebuild/clear、recovery ordering、death/reset 调度和跨分区 writer 唯一性仍待后续集成审查。

## 13. C07 PlayerDetectionAndCombatStatus

成员覆盖：dangerSense、luckPotion、endurance、whipRangeMultiplier、whipUseTimeMultiplier、loveStruck、stinky、resistCold、electrified、dryadWard、panic。输入报告为 11/11，属性 0/0。

源码事实：

- Version4 Player.cs:763-783 声明全部字段；:4616、:4629、:4696-4722、:4856、:7782、:7807、:8542、:8713-8717、:9611 等 Buff/装备路径写入。
- Player.cs:3461 消费 whipUseTimeMultiplier；:9986-9990、:10482-10485、:10592、:10696-10705 清理 detection/combat flags 并恢复 endurance/whip defaults。
- Player.cs:17904 以 luckPotion 影响 luck；:10973、:17166、:17918 消费 electrified/stinky；:22263 在 Hurt 中用 endurance 缩放伤害。

proposed 边界：

- PlayerCombatDetectionStateComponent：dangerSense、loveStruck、stinky、resistCold、electrified、dryadWard、panic；
- PlayerDamageMitigationInputComponent：endurance；
- PlayerWhipCapabilityComponent：whipRangeMultiplier、whipUseTimeMultiplier；
- PlayerLuckPotionStateComponent：luckPotion 作为 Luck query 输入，不重复拥有 luck；
- PlayerCombatDetectionSystem：由 Buff/equipment/environment facts 重建；
- PlayerCombatModifierQuery：向伤害和鞭使用提供不可变 snapshot。

不变量：

- endurance 的范围、钳制和与 armor/defense 的计算顺序必须由 Damage verifier 固定；Query 不写 health。
- luckPotion 只能作为 Luck 派生输入，不能在本组件复制 PlayerLuckStateComponent 的 Luck。
- whip range/use time 只提供能力事实，不拥有 Projectile 或 Item use lifecycle。
- electrified、dryadWard、panic 可能跨环境/事件/防御 owner，不能本组件宣布最终 owner。

当前 NLTX 有 PlayerLuckStateComponent、PlayerLuckCalculationPolicy 和局部 projectile/damage systems，但无 P06 detection aggregate，状态 partial。proposed 路径：Player/Combat/PlayerCombatDetectionStateComponent.cs、Player/Combat/PlayerDamageMitigationInputComponent.cs、Player/Combat/PlayerWhipCapabilityComponent.cs、Systems/PlayerCombatDetectionSystem.cs、Queries/PlayerCombatModifierQuery.cs。

focused verifier：luckPotion pure calculation、endurance bounds and damage order、whip multiplier defaults/reset、environment effect reset、electrified/dryad/panic reader purity。

### C07 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerCombatDetectionStateComponent.cs`、`src/Player/PlayerDamageMitigationInputComponent.cs`、`src/Player/PlayerWhipCapabilityComponent.cs`、`src/Player/PlayerLuckPotionStateComponent.cs`。
- 字段覆盖：11/11；`DangerSense`、`LoveStruck`、`Stinky`、`ResistCold`、`Electrified`、`DryadWard`、`Panic` 为默认 `false`；`LuckPotion` 为 `0`；`Endurance` 为 `0f`；`WhipRangeMultiplier` 与 `WhipUseTimeMultiplier` 为 `1f`。
- 初始化/重置：各组件仅提供内部 `ResetEffects`，清除临时效果并恢复 Version4 基线；未实现 Detection System、Luck Query、Damage Query、Projectile/Item lifecycle 或外部副作用。
- 依赖影响：C10/C19 vital/damage、PlayerLuck、Projectile、StatusEffects、World/Liquid 仍为 integration-review；未验证 endurance 钳制/伤害顺序、luck 派生、鞭子时序和环境标记 reader purity。
- 该 checkpoint 只证明组件源码已保存，不证明行为等价、网络/持久化闭合或运行时注册。

检查点结论：C07 源码已实现但未验证，11/11 字段已落地；System、Query 和跨分区 owner 仍未实现。

## 14. C08 PlayerSocialAndDefenseState

成员覆盖：skinVariant、voiceVariant、voicePitchOffset、ghost、ghostFrame、ghostFrameCounter、_framesLeftEligibleForDeadmansChestDeathAchievement、pvpDeath、boneArmor、frostArmor、honey、crystalLeaf、crystalLeafCooldown、portableStoolInfo、preventAllItemPickups、dontHurtCritters、hasLucyTheAxe、dontHurtNature、defendedByPaladin、hasPaladinShield。输入报告为 20/20，属性 0/0。

源码事实：

- Version4 Player.cs:904-958 声明全部字段；字段混合外观/死亡表现、战斗防御、环境恢复、交互权限和 Paladin team defense。
- Player.cs:3848-3971 的 Ghost 更新 ghost/ghostFrame/ghostFrameCounter；:10066-10067 在死亡路径进入 ghost；:15005-15008、:15087-15089、:21885-21887、:22583-22593 管理死亡/ghost/achievement/PvP death 状态。
- Player.cs:4509、:5848、:6051、:6794-6806、:8265、:8987-8990、:10348-10350、:10595、:10741-10747、:22189-22201、:22287-22361、:23983-23985 分别覆盖 honey/crystalLeaf/Paladin、社会/交互限制、portable stool、armor effects 和伤害分享。

proposed 边界：

- PlayerGhostStateComponent：ghost 及其死亡/观察生命周期；
- PlayerSocialAppearancePreferenceComponent：skinVariant、voiceVariant、voicePitchOffset，最终表现 owner 与 P11 integration-review；
- PlayerDefenseCapabilityComponent：boneArmor、frostArmor、defendedByPaladin、hasPaladinShield、crystalLeaf/cooldown；
- PlayerInteractionPolicyComponent：preventAllItemPickups、dontHurtCritters、hasLucyTheAxe、dontHurtNature；
- PlayerPortableStoolStateComponent：portableStoolInfo 的玩家交互状态；
- PlayerAchievementEligibilityState：achievement timer 只作为受控资格输入；
- PlayerSocialDefenseSystem/Query：各边界分别提交和读取，禁止混成一个大组件。

不变量：

- C08 不能把 20 个字段合成一个 SocialDefenseComponent；Ghost、表现、交互和战斗防御生命周期不同。
- ghost/ghost frames 不等于 PlayerLifecycle 的 active/dead authority；死亡系统只提供明确转换事实。
- honey/crystalLeaf/bone/frost/Paladin 只能提供 recovery/defense capability，不能直接越过 Combat damage commit。
- portableStoolInfo 的 IsInUse 会影响位置/高度，但 C08 不拥有 Position/Movement。
- pickup/nature/critters flags 是 interaction policy，不是 ItemContainer 或 World tile authority。

当前 NLTX 有 PlayerDefenseStateComponent、PlayerLifecycleComponent、PlayerInteractionComponent、PlayerEnvironmentContactComponent 等局部骨架；没有这 20 个字段的完整聚合，状态 partial。proposed 路径按 Player/Combat、Player/Interaction、Player/Presentation 领域分置，避免泛化目录。

focused verifier：ghost/death transition、frame counter、PvP death reset、Paladin damage sharing、crystalLeaf cooldown、portable stool transition、interaction rejection 和 achievement timer expiry。

### C08 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerGhostStateComponent.cs`、`src/Player/PlayerSocialAppearancePreferenceComponent.cs`、`src/Player/PlayerDefenseCapabilityComponent.cs`、`src/Player/PlayerInteractionPolicyComponent.cs`、`src/Player/PlayerPortableStoolStateComponent.cs`、`src/Player/PlayerAchievementEligibilityStateComponent.cs`。
- 字段覆盖：20/20。Ghost/PvP、社交外观、防御能力、交互策略、便携凳和成就资格分别保存；`portableStoolInfo` 已展开为 `HasAStool`、`IsInUse`、`HeightBoost`、`VisualYOffset`、`MapYOffset`，没有引入 Version4 `PortableStoolUsage` 类型。
- 默认/重置：布尔、计数和偏移均回到 `false`/`0`；防御能力组件的 `CrystalLeafCooldown` 也清零。组件不拥有死亡、位置、移动、库存拾取、NPC population 或表现输出行为。
- 依赖影响：P04 lifecycle、P09 pickup/equipment、P11 presentation、Movement/Spatial、Town/NPC 和 Combat damage 仍为 integration-review；未验证 Ghost/PvP transition、Paladin sharing、stool transition、interaction rejection 和 achievement expiry。
- 该 checkpoint 只证明组件源码已保存，不证明 PortableStoolUsage 行为等价、网络/持久化闭合或运行时注册。

检查点结论：C08 源码已实现但未验证，20/20 字段已落地；System、Query 和跨分区 owner 仍未实现。

## 15. C09 PlayerFrameAndImmunityState

成员覆盖：townNPCs、bodyFrameCounter、legFrameCounter、immune、immuneNoBlink、immuneTime、immuneAlphaDirection、immuneAlpha、_timeSinceLastImmuneGet、_immuneStrikes、maxRegenDelay。输入报告为 11/11，属性 0/0。

源码事实：

- Version4 Player.cs:961-986 声明字段；body/leg counters 在 :20473-20625、:20808-20961 由移动/动画路径更新。
- Player.cs:10808-10835 递减 immuneTime 并更新 immuneNoBlink/immuneAlpha；:12014-12030 的 GiveImmuneTimeForCollisionAttack 使用 _timeSinceLastImmuneGet/_immuneStrikes 防止连续碰撞无限延长无敌。
- Player.cs:15034-15037 更新 immune strike window；:15062-15063 由 mana ratio 计算 maxRegenDelay；:21846-21906、:22084-22089、:22221-22335、:22652-22655 处理 hurt/immune/alpha 的多个边界。
- townNPCs 在本分区声明但其读者跨 NPC/Town population；不能据此由 Player Combat owner 单独接管。

proposed 边界：

- PlayerHitFrameImmunityComponent：immune、immuneNoBlink、immuneTime、hurt cooldown relation、_timeSinceLastImmuneGet、_immuneStrikes；
- PlayerFrameProjection：bodyFrameCounter、legFrameCounter、immuneAlphaDirection、immuneAlpha；
- PlayerRegenDelayStateComponent：maxRegenDelay 作为 mana/resource delay 的派生输入；
- PlayerTownInteractionProjection：townNPCs 只作为跨域人口/交互快照，不宣布 owner；
- PlayerImmunityTickSystem 和 PlayerFrameProjectionSystem 分离。

不变量：

- immune/immuneTime/hurtCooldowns 是 combat qualification，不等于 alpha/animation state。
- _immuneStrikes 的窗口和碰撞免疫必须由一个 writer 维护；不能让 HitImmunityComponent 和 Player component 重复代表同一 Player immunity。
- maxRegenDelay 是根据已提交 mana snapshot 计算的派生值，不能作为独立资源 authority。
- body/leg counters 不能由客户端 renderer 反写；townNPCs 不能成为 P06 的第二份 NPC population state。

当前 NLTX 有 Combat ImmunityComponent/HitImmunityComponent、PlayerCooldownStateComponent、PlayerVitalRegenSystem 和局部 Player state，属于 partial；hurt cooldown key 尚未与 Version4 Player 字段闭合。proposed 路径：Player/Combat/PlayerHitFrameImmunityComponent.cs、Player/Presentation/PlayerFrameProjection.cs、Player/Combat/PlayerRegenDelayStateComponent.cs。

focused verifier：immune timer and collision strikes、cooldown key isolation、alpha projection、maxRegenDelay calculation、animation counter determinism、townNPCs read-only handoff。

### C09 本地源码 checkpoint

- implementationStatus: partial
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerHitFrameImmunityComponent.cs`、`src/Player/PlayerRegenDelayStateComponent.cs`。
- 已落地字段：`immune`、`immuneNoBlink`、`immuneTime`、`_timeSinceLastImmuneGet`、`_immuneStrikes` 和 `maxRegenDelay`，共 6/11；生命周期重置恢复 `false`/`0`/`0f` 基线。
- 未落地字段：`townNPCs`、`bodyFrameCounter`、`legFrameCounter`、`immuneAlphaDirection`、`immuneAlpha`。这些在设计中属于 Town/NPC handoff 或 presentation projection，本任务禁止创建 Projection/非组件代码，因此保留未完成状态。
- 依赖影响：Combat immunity、P04 lifecycle、P11 presentation、P13 Town/NPC、C10 vital/regen、P15 Projectile 仍未闭合；未验证 immune timer/strike window、cooldown isolation、alpha projection 和 `maxRegenDelay` 计算。

检查点结论：C09 authority 组件源码已实现但该叶子组仍为 partial，6/11 字段已落地；presentation projection 与 Town/NPC handoff 未实现。

## 16. C10 PlayerVitalAndRegenState

成员覆盖：statLifeMax、statLifeMax2、statLife、statMana、statManaMax、statManaMax2、lifeRegen、lifeRegenCount、lifeRegenTime、manaRegen、manaRegenCount、manaRegenDelay、manaRegenBuff。输入报告为 13/13，属性 0/0。

源码事实：

- Version4 Player.cs:1357-1381 声明生命/魔力当前值、基础/有效上限和两套 regen state；statLifeMax/statLife 默认 100，statManaMax 在构造路径为 20。
- Player.cs:10332、:10386-10392 重置 lifeRegen、statLifeMax2/statManaMax2 和 manaRegenBuff；:10847-11234 执行生命 regen 的减益、积算、随机和 Health 变化；:11239-11315 执行魔力 regen/delay/accumulator。
- Player.cs:15601-15606、:17668-17674 做每帧 clamp；:21830-21843、:21889-21899 处理复活/死亡前后的 vital；:22257-22260、:22324、:22335、:22379 在 Hurt 中处理 mana/life 和 regen delay。
- 当前 NLTX HealthComponent、ManaComponent、HealthRegenerationComponent 和 PlayerVitalRegenSystem 已存在，但 PlayerVitalRegenSystem 同时更新 HealthComponent 和 ManaComponent，不能直接与新 PlayerVital owner 并行。

proposed 边界：

- PlayerVitalStateComponent：statLife、statLifeMax、statLifeMax2、statMana、statManaMax、statManaMax2；
- PlayerLifeRegenStateComponent：lifeRegen、lifeRegenCount、lifeRegenTime；
- PlayerManaRegenStateComponent：manaRegen、manaRegenCount、manaRegenDelay、manaRegenBuff；
- PlayerVitalCommitSystem：唯一写入当前生命/魔力及上限 clamp 的提交根；
- PlayerLifeRegenSystem/PlayerManaRegenSystem：只通过 CommitPort 写回 vital；
- PlayerVitalSnapshotProjection：向网络/持久化/死亡系统提供不可变快照。

不变量：

- 0 <= statLife <= statLifeMax2，0 <= statMana <= statManaMax2；有效上限重算后必须有明确 clamp 时点。
- 生命归零只发布 PlayerVitalDepletedEvent，由 PlayerLifecycle/Death owner 处理死亡；P06 不复制 dead/respawn/drop。
- lifeRegenCount/manaRegenCount 的 120-unit 累加阈值、负 regen/HurtLifeRegen、mana delay 顺序必须通过 focused verifier 固定。
- DamageCommit、RegenSystem、RespawnSystem 和 network restore 不能形成多个 vital writer。

当前 NLTX 为 partial existing-evidence：HealthComponent 和 ManaComponent 的字段形状接近，但命名空间/owner 与 Player P06 仍不闭合；PlayerStateProjection 只输出部分 life/mana。proposed 路径为 Player/Combat/PlayerVitalStateComponent.cs、PlayerLifeRegenStateComponent.cs、PlayerManaRegenStateComponent.cs、Systems/PlayerVitalCommitSystem.cs、PlayerLifeRegenSystem.cs、PlayerManaRegenSystem.cs。

focused verifier：default/reset, upper/lower clamp, damage then regen order, negative regen, mana delay, accumulator thresholds, death boundary, respawn restore, duplicate command, network snapshot round-trip and persistence blocked case。

### C10 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerVitalStateComponent.cs`、`src/Player/PlayerLifeRegenStateComponent.cs`、`src/Player/PlayerManaRegenStateComponent.cs`。
- 字段覆盖：13/13。`StatLifeMax`、`StatLifeMax2`、`StatLife` 默认 `100`；`StatMana` 默认 `0`；`StatManaMax` 与 `StatManaMax2` 默认 `20`；regen 字段按生命/魔力分组件保存并以 `0`/`0f`/`false` 初始化。
- 重置：生命组件的 `ResetEffects` 只将有效上限恢复为基础上限；两类 regen 组件清理本帧派生值，生命周期重置清理累加器、延迟和 Buff 标记。未实现 Vital Commit/Regen System、snapshot、death transition 或 network/persistence adapter。
- 依赖影响：C03/C04/C09/C11/C19、P04 lifecycle、P09 equipment/Buff、Network/Session、Persistence 仍为 integration-review；未验证 clamp、damage/regen 顺序、120-unit accumulator、mana delay、death/respawn 和 Health/Mana 单 writer。

检查点结论：C10 源码已实现但未验证，13/13 字段已落地；生命/魔力提交系统和跨分区 owner 仍未实现。

## 17. C11 PlayerCombatModifierAndImmunityState

成员覆盖：armorPenetration、meleeArmorPenetration、statDefense、noKnockback、shimmerImmune、spaceGun、gravDir、chaosState、strongBees、sporeSac、shinyStone、empressBrooch、volatileGelatin、volatileGelatinCounter、hasMagiluminescence、shadowArmor。输入报告为 16/16，属性 0/0。

源码事实：

- Version4 `Player.cs:1351-1431` 确认三项防御/穿透整数、noKnockback、private `shimmerImmune`、spaceGun、默认值为 `1f` 的 gravDir，以及其余战斗能力标记的声明和可见性。
- `Player.cs:3366-3376` 的 `GetArmorPenetration` 将 armorPenetration 与 meleeArmorPenetration 按攻击类型合并；不能让各武器系统直接复制或改写 Player 防御快照。
- `Player.cs:10320-10585` 的 ResetEffects 将 armorPenetration、meleeArmorPenetration、statDefense、noKnockback、spaceGun、chaosState、strongBees、sporeSac、shinyStone、empressBrooch、volatileGelatin、hasMagiluminescence、shadowArmor 等装备/Buff 派生值清零或清除；statDefense 在 :15553-15556 还存在非负 clamp。
- `Player.cs:9162-9165` 由装备事实设置 shimmerImmune；`Player.cs:14165-14168`、:14820-14824、:17133-17147 和 :25630-25637 显示 shimmer/spaceGun/gravDir 分别参与环境和物品使用资格。它们不能被 P06 当作 Liquid、World 或 Movement 的完整 owner。
- `Player.cs:15349-15360`、:15553-15556、:15620-15633 和 :22208-22385 显示能力/防御修正先汇入 Player，再由 Update/Hurt 消费；其中 noKnockback 只控制受击后击退分支，不能替代伤害资格免疫。

proposed 边界：

- `PlayerCombatDefenseModifierComponent`：armorPenetration、meleeArmorPenetration、statDefense、noKnockback。唯一写者为 `PlayerCombatModifierCommitSystem`，输出只读 `CombatDefenseSnapshot` 和 `ArmorPenetrationQuery`。
- `PlayerCombatCapabilityComponent`：spaceGun、chaosState、strongBees、sporeSac、shinyStone、empressBrooch、volatileGelatin、volatileGelatinCounter、hasMagiluminescence、shadowArmor。它保存装备/Buff 已提交的能力事实，不保存 Item payload、Projectile 状态或 World 规则。
- `PlayerEnvironmentInteractionSnapshot`：shimmerImmune、gravDir。该快照由 `Movement/LiquidEnvironmentAdapter` 通过明确输入端口提供；P06 只消费资格查询，不声明重力、微光液体或位置的最终 owner。
- `PlayerCombatModifierCommitSystem`：在 ResetEffects 基线和装备/Buff/environment input 完成后一次提交 C11 authority；`PlayerCombatPermissionQuery` 只计算是否允许特定 combat/item action；`PlayerCombatModifierProjection` 只向网络/表现输出不可变 DTO。

接口契约：

- Interface：`IPlayerCombatModifierCommitPort.Commit(PlayerCombatModifierInput)`；Implementation：C11 commit system；Seam：equipment/status/environment adapters；Depth：一层；Leverage：高；Locality：Player combat。
- Interface：`IPlayerCombatPermissionQuery.Evaluate(CombatActionContext, CombatModifierSnapshot)`；Implementation：纯 Query；Seam：Projectile/Item/Movement callers；Depth：一层；Leverage：中；Locality：只读计算。
- `Query` 不写 armor/statDefense/ability counter；`System` 才能提交状态；环境 adapter 不能写 P06 的生命、位置或速度；Projection 不能反写 authority。

不变量与验证计划：

- 默认值：armorPenetration、meleeArmorPenetration、statDefense、volatileGelatinCounter 为 0；布尔标记为 false；gravDir 初始化为 `1f`。ResetEffects 后派生能力必须回到这些基线，但 gravDir 的运行时变更必须由 Movement/重力 owner 提交。
- melee armor penetration 只能在 melee 查询中叠加；一般穿透、近战穿透和 statDefense 不得相互复写。statDefense 的负值 clamp 只能有一个提交点。
- `shimmerImmune` 只影响 shimmer 资格，不能伪造 `shimmering` 或 `shimmerWet`；`spaceGun` 只作为物品使用/魔力延迟资格输入；`noKnockback` 不等于 immune。
- volatileGelatinCounter 的递减、边沿和装备移除清理必须在唯一 system 中完成；strongBees/sporeSac/shinyStone/empressBrooch/shadowArmor 只输出能力事实，由各消费者读取。
- focused verifier：ResetEffects 复位、近战/非近战穿透合并、statDefense clamp、noKnockback Hurt 分支、spaceGun 资格、shimmerImmune 环境交接、volatile counter tick/重复装备重算、gravDir 只读交接和 snapshot 单向性。

跨分区依赖与回滚：

- 依赖 C10 vital/damage commit、C12/C16/C19 accessory and damage modifiers、C13 shimmer/elemental、C20 movement/permission、P09 Item/Buff、P15 Projectile、Liquid/World/Movement。
- 在唯一 modifier writer、damage snapshot、gravDir owner 和 shimmer adapter 获得 integration-review 前，不迁移旧 readers；保留旧 API read adapter，禁止新旧字段双写。
- 若 verifier 发现 statDefense/穿透顺序变化、environment input 反向写 authority 或 capability counter 重复提交，撤销本批 reader migration，保留设计状态 `proposed`。

### C11 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerCombatDefenseModifierComponent.cs`、`src/Player/PlayerCombatCapabilityComponent.cs`、`src/Player/PlayerEnvironmentInteractionComponent.cs`。
- 字段覆盖：16/16。防御修正组件保存 armor penetration、defense 和 no-knockback；能力组件保存 space gun、chaos、蜂群、孢子、闪耀石、皇后胸针、volatile gelatin、magiluminescence 和 shadow armor；环境组件保存 `shimmerImmune` 与 `gravDir`。
- 默认/重置：整数与计数为 `0`，能力标记为 `false`，`GravDir` 为 `1f`；内部重置不修改位置、重力、微光液体或伤害目标。
- 依赖影响：C10/C19、C13、C20、P09、P15、Liquid/World/Movement、Network/Presentation 仍为 integration-review；未验证 penetration composition、statDefense clamp、spaceGun permission、shimmer handoff、volatile counter 和 gravDir ownership。

检查点结论：C11 组件源码已实现但未验证，16/16 字段已落地；Combat Modifier System、Permission Query 和环境 owner 仍未实现。

## 18. C12 PlayerAmmoAndAccessoryEffects

成员覆盖：chloroAmmoCost80、huntressAmmoCost90、ammoCost80、ammoCost75、stickyBreak、magicQuiver、magmaStone、lavaRose、hasMoltenQuiver、phantasmTime、ammoBox、ammoPotion。输入报告为 12/12，属性 0/0。

源码事实：

- Version4 `Player.cs:1391-1413` 确认四个弹药消耗标记、stickyBreak、四个箭袋/配饰能力、phantasmTime 与 ammoBox/ammoPotion 的声明和默认零值。
- `Player.cs:10388-10391`、:10448-10449、:10583-10588 在 ResetEffects 清理 C12 的派生布尔值；`Player.cs:10754-10757` 每 Tick 递减 phantasmTime；`Player.cs:13986-13990` 在黏性移动路径清除 stickyBreak。
- `Player.cs:26068-26082` 使用 hasMoltenQuiver/magicQuiver 改变投射物类型、伤害、速度和击退；:26113-26175 将 magicQuiver、ammoBox、ammoPotion 及四个 cost policy 接入 ammo consumption 资格；:26193-26199 才提交 consumable stack 变化。
- 装备/Buff 入口位于 `Player.cs` 的 armor/accessory 与 Buff effect 路径，结论只证明能力事实被重建并消费，不证明 Item payload、Projectile 生命周期或随机序列已适合直接迁移。

proposed 边界：

- `PlayerAmmoCostPolicyComponent`：chloroAmmoCost80、huntressAmmoCost90、ammoCost80、ammoCost75、ammoBox、ammoPotion。它只保存本帧/本次发射的消耗优惠事实。
- `PlayerRangedAccessoryCapabilityComponent`：magicQuiver、magmaStone、lavaRose、hasMoltenQuiver、phantasmTime。magicQuiver/hasMoltenQuiver 供 projectile request query 读取，magma/lava 供 combat/status adapter 消费，phantasmTime 由 timeout system 唯一递减。
- `PlayerStickyBreakStateComponent`：stickyBreak 作为移动/黏着边界的临时计数，不与 ammo policy 混写；最终黏着物理 owner 属于 Movement/World integration-review。
- `PlayerAmmoConsumptionQuery`：输入 `AmmoUseContext`、cost policy、`IRandomSource` 和 deterministic seed，返回 `ConsumeAmmoDecision`；Query 不改 Item stack、不创建 Projectile。
- `PlayerAmmoCommitSystem`：仅在发射事实已提交且决策为 consume 时向 `IItemStackCommitPort` 发出一次命令；`PlayerAccessoryCapabilityRebuildSystem` 负责 ResetEffects 后的能力提交；Projection 只输出 typed capability snapshot。

接口契约：

- Interface：`IPlayerAmmoConsumptionQuery.Decide(AmmoUseContext, AmmoPolicySnapshot, RandomSample)`；Implementation：纯 Query；Seam：Item/Projectile adapter；Depth：一层；Leverage：高；Locality：ammo decision。
- Interface：`IItemStackCommitPort.Consume(ItemReference, int, CommandId)`；Implementation：ItemContainer owner；C12 只能发 command，不能持有完整 `Terraria.Item` 或直接改 stack。
- Interface：`IPlayerProjectileCapabilityReader.Read(PlayerEntity)`；Implementation：只读 snapshot；Seam：ProjectileSimulation；它不得反写 magicQuiver 或 phantasmTime。

不变量与验证计划：

- 四个 cost 标记、ammoBox、ammoPotion 的概率决策必须使用显式随机样本；同一 command/revision 重放不得重复扣 stack，拒绝发射不得触发消费。
- magicQuiver 和 hasMoltenQuiver 只能改变 typed projectile request 的 capability/参数；Projectile 类型、实例和轨迹不进入 Player component。magmaStone/lavaRose 不接管 Liquid/World 状态。
- phantasmTime 只能由 timeout system 递减，装备重算不能把倒计时重复叠加；stickyBreak 的清理、边沿和 pause/death reset 独立验证。
- focused verifier：默认/ResetEffects、随机样本边界、四项概率、ammoBox/ammoPotion 组合、重复 command、stack 下溢保护、magic/ molten quiver request、phantasm expiry、stickyBreak reader purity 和 Item payload 隔离。

跨分区依赖与回滚：

- 依赖 P09 ItemContainer/Equipment、P15 Projectile、C11 combat capability、C13 elemental status、C16 accessory combat、C19 damage/crit、C20 speed/permission、Network/Presentation。
- 在 Item stack commit root、随机源、Projectile request schema 和 sticky movement owner 未裁决前，只迁移只读 Query；保留旧发射/消费 reader，禁止新旧扣减双写。
- 若发现随机调用顺序、stack 变化、Projectile 生成或 phantasm timeout 与 Version4 不一致，回滚本批 command adapter，保留能力快照设计，状态仍为 partial。

### C12 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerAmmoCostPolicyComponent.cs`、`src/Player/PlayerRangedAccessoryCapabilityComponent.cs`、`src/Player/PlayerStickyBreakStateComponent.cs`。
- 字段覆盖：12/12。弹药策略保存四个概率标记与 `ammoBox`/`ammoPotion`；远程配饰保存 quiver、magma/lava 和 `phantasmTime`；黏着边界单独保存 `stickyBreak`。
- 默认/重置：布尔标记为 `false`，计数为 `0`；装备效果 reset 不清除 `phantasmTime`，生命周期 reset 才清除倒计时，保持 timeout owner 可区分。
- 依赖影响：P09 ItemContainer/Equipment、P15 Projectile、C11/C13/C16/C19/C20、Network/Presentation 仍为 integration-review；未验证随机样本、stack 幂等、quiver request、phantasm expiry 和 sticky movement purity。

检查点结论：C12 组件源码已实现但未验证，12/12 字段已落地；弹药 Query/Commit、Projectile 和 Item owner 仍未实现。

## 19. C13 PlayerElementalAndShimmerStatus

成员覆盖：archery、poisoned、venom、blind、blackout、headcovered、frostBurn、onFrostBurn、onFrostBurn2、burned、shimmering、timeShimmering、shimmerTransparency、shimmerUnstuckHelper、suffocating、dripping、drippingSlime、drippingSparkleSlime、onFire、onFire2、onFire3。输入报告为 21/21，属性 0/0。

源码事实：

- Version4 `Player.cs:1712-1753` 确认 C13 的 20 个 bool、一个 int、一个 float 和一个 `ShimmerUnstuckHelper` 外部类型字段；所有公开状态都属于玩家运行时状态，helper 不是 ECS authority 数据。
- `Player.cs:4453-4457`、:5882-5958、:5993-5997 说明 archery、毒/盲/燃烧/滴落/微光/头部覆盖等标记由 Buff effect 路径派生；`Player.cs:10633-10653` 在 ResetEffects 清理这些别名状态。
- `Player.cs:10847-11234` 的 UpdateLifeRegen 直接消费 poisoned、venom、onFire*、frostBurn*、burned、suffocating 和 drippingSlime，证明这些标记是生命回复计算输入而不是独立生命 writer。
- `Player.cs:10293-10313` 对 shimmerTransparency 做表现/时间衰减；:15307-15310 更新 helper；:17132-17366 根据 Collision.shimmer/wet/honey 维护微光接触；:21744-21750 在 Teleport 清除 shimmering/shimmerWet/wet；:22212-22214 将 shimmering 作为受击闪避资格输入。
- archery 同时影响 `Player.cs:26087-26093` 的箭投射参数；C13 只提交能力事实，具体 projectile 创建和伤害仍由 C12/C19/P15 消费。

proposed 边界：

- `PlayerElementalStatusComponent`：archery、poisoned、venom、blind、blackout、headcovered、frostBurn、onFrostBurn、onFrostBurn2、burned、suffocating、onFire、onFire2、onFire3。它是 Buff/environment fact 的短生命周期派生状态。
- `PlayerSurfaceElementStatusComponent`：dripping、drippingSlime、drippingSparkleSlime。它表达接触/外观和 regen modifier 输入，不拥有 Tile、Liquid、wet 或玩家位置。
- `PlayerShimmerStateComponent`：shimmering、timeShimmering、shimmerTransparency，以及稳定的 `ShimmerUnstuckSnapshot`；`shimmerUnstuckHelper` 通过 `IShimmerUnstuckAdapter` 映射，不把 `Terraria.GameContent` helper 引用渗透核心组件。
- `PlayerElementalStatusRebuildSystem` 读取已提交 Buff type/time 和环境快照，唯一写入 C13 flags；`PlayerShimmerEnvironmentSystem` 通过 Liquid/World port 提交微光接触；`PlayerElementalQuery` 只输出 regen/projectile/damage modifier input；`PlayerShimmerProjection` 只输出透明度和视觉状态。

接口契约：

- Interface：`IPlayerElementalStatusCommitPort.Commit(BuffStatusSnapshot, SurfaceEnvironmentSnapshot)`；Implementation：status rebuild system；Seam：StatusEffects/World/Liquid；Depth：一层；Leverage：高；Locality：Player status。
- Interface：`IShimmerUnstuckAdapter.Update(PlayerEntity, ShimmerEnvironmentInput)` / `Clear(PlayerEntity)`；Implementation：外部 helper adapter；核心组件只接受稳定 DTO 和结果，不保留第三方引用。
- Interface：`IPlayerElementalQuery.GetEffects(PlayerElementalSnapshot)`；Implementation：纯 Query；Seam：C10 regen、C12 projectile、C19 damage、P11 presentation；Query 不写 Buff、vital 或 environment。

不变量与验证计划：

- Buff type/time 是唯一状态来源；C13 flags 可以在每个效果重建周期清零并从 snapshot 重算，不能由网络客户端单独设置，也不能与 BuffCollection 形成第二份 timer writer。
- lifeRegen 的减益顺序、drippingSlime 的叠加、burned/suffocating 的移动/死亡边界必须由 C10 focused verifier 固定；C13 不直接改 statLife。
- shimmering、timeShimmering、shimmerTransparency 和 shimmerUnstuck result 分离：环境接触决定 authority 状态，透明度是表现/时间投影；Teleport、death、unstuck clear 必须幂等。
- shimmer immunity、shimmer dodge、shimmerWet 与 wet/liquid owner 通过端口交接；C13 不修改 position、velocity、gravity 或 World tile。
- focused verifier：Buff rebuild/expiry、每个 elemental flag 的 reset、regen contribution ordering、archery projectile snapshot、shimmer enter/leave、transparency clamp、teleport/death clear、helper adapter failure 和 projection read-only。

跨分区依赖与回滚：

- 依赖 StatusEffects/BuffCollection、C10 vital/regen、C11 shimmer permission、C12 ammo/projectile、C14 survival、C15 debuff aliases、C19 damage、Liquid/World/Movement、P11 Presentation。
- 在 Buff alias owner、shimmer environment authority 和 helper adapter 的 failure/ retry 策略未裁决前，保留旧 status/environment readers；不同时写 Buff flags 与 C13 flags。
- 若发现类型/时间不同步、微光碰撞顺序变化、透明度反向驱动 authority 或第三方 helper 泄漏，回滚 adapter/reader migration，保留 proposed DTO boundary。

### C13 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerElementalStatusComponent.cs`、`src/Player/PlayerSurfaceElementStatusComponent.cs`、`src/Player/PlayerShimmerStateComponent.cs`。
- 字段覆盖：21/21。元素状态保存 14 个标记，表面状态保存 3 个滴落标记，微光状态保存 `shimmering`、`timeShimmering`、`shimmerTransparency` 及 helper 的稳定时间/保护状态。
- 默认/重置：元素和表面标记为 `false`；微光计时/透明度为 `0`；生命周期重置清空微光 unstuck scalar。没有引入 `Terraria.GameContent.ShimmerUnstuckHelper` 引用。
- 依赖影响：StatusEffects/BuffCollection、C10 regen、C11 shimmer permission、C12/C19 projectile/damage、C14/C15、Liquid/World/Movement、P11 Presentation 仍为 integration-review；未验证 Buff expiry、regen ordering、shimmer enter/leave、透明度边界和 adapter failure/retry。

检查点结论：C13 组件源码已实现但未验证，21/21 字段已落地；元素重建、微光环境 adapter、Query 和 projection 仍未实现。

## 20. C14 PlayerSurvivalAndTransformationState

成员覆盖：noItems、hungry、starving、heartyMeal、windPushed、wereWolf、wolfAcc、hideMerman、hideWolf、forceMerman、forceWerewolf、sunScorchCounter、accMerman、merman、trident。输入报告为 15/15，属性 0/0。

源码事实：

- Version4 `Player.cs:1755-1793` 确认生存资格、环境风、狼人/鱼人变身、sunScorchCounter 和三叉戟能力字段；这些字段混合了 Buff 派生事实、装备能力和环境/表现计数，不能合并成单一 survival blob。
- `Player.cs:5902-5905`、:6001-6005、:6060-6070、:6141-6174、:6868-6873、:8017-8041 和 :8767-9207 说明 noItems/windPushed/hungry/starving/heartyMeal、wolf/merman accessory 及 trident 来自 Buff/装备/当前物品事实。
- `Player.cs:10635-10658` 清理 noItems、饥饿状态和 wereWolf；`Player.cs:15240-15270` 依据 wet/environment 计算 merman、处理 accMerman/forceMerman/wolfAcc/forceWerewolf 的边沿；这证明 `merman` 是环境与能力交接结果，不是独立的 active/dead 或 Movement owner。
- `Player.cs:11519-11522` 消费 windPushed，:14841-14847 和 :17573-17578 消费 merman/trident 影响运动/液体；:17713-17724 更新 sunScorchCounter。P06 只发布 input/capability，不直接写 velocity、gravity、wet 或 position。
- `Player.cs:25283-25290`、:25692-25750 使用 noItems 限制物品动作；`UpdateDead` :9974-9994 清理 survival/environment flags。死亡重置和重生 owner 仍属于 PlayerLifecycle integration-review。

proposed 边界：

- `PlayerSurvivalNeedComponent`：noItems、hungry、starving、heartyMeal。它提供 item-use gate 与 C10 regen/damage modifier input，不能直接改生命或库存。
- `PlayerTransformationCapabilityComponent`：wereWolf、wolfAcc、hideMerman、hideWolf、forceMerman、forceWerewolf、accMerman、merman。它保存变身意图/当前资格快照，最终湿润、重力、动画和 hitbox 由 Movement/Liquid/Presentation owner 消费。
- `PlayerEnvironmentalPressureComponent`：windPushed、sunScorchCounter。风是环境输入，sunScorchCounter 是有界 temporal state；两者由 environment tick port 提交，不持有 World weather 或音频/特效对象。
- `PlayerTridentCapabilityComponent`：trident 作为当前物品的 typed capability，仅供 movement/item-use query；不把 `Terraria.Item` 或物理实现放入组件。
- `PlayerSurvivalTransformationSystem` 唯一提交 C14 派生状态；`PlayerTransformationQuery` 返回 wet/merman/trident 对 Movement 的只读资格；`PlayerSurvivalProjection` 输出表现/网络快照。

接口契约：

- Interface：`IPlayerSurvivalCommitPort.Commit(SurvivalStatusInput)`；Implementation：survival/transformation system；Seam：Buff/Equipment/World/Liquid；Depth：一层；Leverage：中高；Locality：Player lifecycle/status。
- Interface：`IPlayerMovementEnvironmentInput.ApplyWindAndTransformation(PlayerEntity, WindInput, TransformationSnapshot)`；Implementation：Movement owner；C14 只发布 command/input，不能在 Query 中改 velocity/gravity。
- Interface：`IPlayerItemUsePermissionQuery.CanUse(ItemUseContext, SurvivalNeedSnapshot)`；Implementation：纯 Query；`noItems` 和 `trident` 是资格输入，不是 ItemContainer state。

不变量与验证计划：

- hungry/starving/heartyMeal 的 Buff type/time 必须由 StatusEffects 成对提交；C14 只重建别名，不能绕过 Buff immunity/time。生存状态对 C10 的 regen/damage 影响必须通过 snapshot。
- merman 只有在 wet 且非 lava/不被 mount 规则阻断时才能提交；accMerman/forceMerman/wolfAcc 等装备事实在每帧 rebuild 后清理，避免跨帧残留。hide flags 只影响 presentation/appearance query。
- windPushed 的移动效果由 Movement 消费；sunScorchCounter 必须 clamp 到 Version4 的 0..300，并把音频、shader、粒子作为外部 effect port。
- trident 只表示当前 item capability；在 lava/wet/merman 组合下的运动资格通过纯 query 固定，不直接移动玩家。
- focused verifier：need reset/regen ordering、noItems item gate、wet/mount/mennan transform matrix、wolf/mennan reset边沿、wind input purity、sunScorch clamp/death behavior、trident item query、teleport/respawn cleanup 和 projection 单向性。

跨分区依赖与回滚：

- 依赖 C10 vital/regen、C13 elemental/shimmer、C15 debuff aliases、C20 speed/permission、P04 PlayerLifecycle、P09 Item/Buff、Liquid/World/Movement、P11 Presentation。
- 在 Movement/Liquid 对 merman/trident/wind 的 owner、survival death reset 和环境时间源未裁决前，保留旧 reader，不迁移直接运动写入。
- 若发现 hunger/transform 影响顺序、sunScorch 外部副作用、wet/merman 资格或 reset 结果变化，撤销本批 reader/adapter，保留 C14 proposed components。

### C14 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerSurvivalNeedComponent.cs`、`src/Player/PlayerTransformationCapabilityComponent.cs`、`src/Player/PlayerEnvironmentalPressureComponent.cs`、`src/Player/PlayerTridentCapabilityComponent.cs`。
- 字段覆盖：15/15。生存需求保存 4 个标记，变身能力保存 8 个标记，环境压力保存 `windPushed`/`sunScorchCounter`，三叉戟能力单独保存。
- 默认/重置：能力和需求标记为 `false`；`SunScorchCounter` 生命周期重置为 `0`，不在组件内执行 0..300 clamp、声音、粒子、位置或移动写入。
- 依赖影响：C10/C13/C15/C20、P04 lifecycle、P09 Item/Buff、Liquid/World/Movement、P11 Presentation 仍为 integration-review；未验证 hunger/regen 顺序、wet/merman matrix、wolf reset、wind input purity、sun scorch clamp 和 trident query。

检查点结论：C14 组件源码已实现但未验证，15/15 字段已落地；生存/变身 System、Movement/Liquid owner 和 projection 仍未实现。

## 21. C15 PlayerDebuffStatusState

成员覆盖：cursed、bleed、confused、brokenArmor、silence、slow、gross、tongued。输入报告为 8/8，属性 0/0。

源码事实：

- Version4 `Player.cs:1757-1803` 确认八个 debuff alias 的公开字段；其原始 Buff type/time 仍位于 Player BuffCollection，C15 不得再创建独立的剩余时间表。
- `Player.cs:4466-4506`、:5276-5292、:5902-5906 由 Buff effect 路径设置 bleed/confused/slow/silence/brokenArmor/gross/tongued/cursed；`Player.cs:10654-10674` 在 ResetEffects 清理同一批 alias。
- `Player.cs:10847-11235` 消费 bleed/tongued 参与生命回复和 HurtLifeRegen；`Player.cs:15553-15573` 消费 brokenArmor/slow 影响防御/速度；`Player.cs:15721-15723`、:16075-16077、:17557-17611 消费 tongued 影响移动/碰撞；`Player.cs:25283-25366` 消费 cursed/silence 影响物品与魔力使用。
- `Player.cs:22221-22335` 显示 C15 状态并不直接拥有免疫或生命提交；受击资格、damage commit 和 vital commit 仍由 C03/C09/C10 交接。

proposed 边界：

- `PlayerDebuffStatusAliasComponent`：八个字段全部作为 Buff-derived alias 保存一个当前 tick 的权威快照，不保存 Buff type/time、Item 或 NPC source。
- `PlayerDebuffAliasRebuildSystem`：从唯一 `BuffStatusSnapshot` 清理并重建 C15 flags；C06 `PlayerDebuffRecoveryStateComponent` 和 C13/C14 相关状态只能读取此快照或共享 status port，禁止各自再写 alias。
- `PlayerDebuffEffectQuery`：输出 `ItemUseGate`、`MovementGate`、`DefenseModifierInput` 和 `LifeRegenModifierInput`；Query 不直接写 moveSpeed/statDefense/lifeRegen/statLife。
- `PlayerDebuffStatusProjection`：将已提交 alias 转为网络/表现 DTO；Projection 不反向添加/删除 Buff。

接口契约：

- Interface：`IPlayerDebuffAliasRebuildPort.Rebuild(BuffStatusSnapshot)`；Implementation：唯一 alias rebuild system；Seam：StatusEffects；Depth：一层；Leverage：高；Locality：Player status。
- Interface：`IPlayerDebuffEffectQuery.Evaluate(PlayerDebuffAliasSnapshot)`；Implementation：纯 Query；Seam：C10/C11/C14/C20 and Item/Movement callers；Query 只返回 immutable inputs。
- Interface：`IBuffStatusReader.ReadCommitted(PlayerEntity)`；Implementation：BuffCollection owner；C15 可以读 type/time，但不能持有第二份 timer 或改变 immunity。

不变量与验证计划：

- 每个 alias 的来源、Buff type/time、过期时点和 reset 必须可追溯；Buff immunity 删除 Buff 后 alias 在同一明确阶段清除，不允许一帧残留或双写。
- cursed/silence 只能作为 item/mana permission input；slow/tongued 只能输出 Movement gate；brokenArmor 只能输出 defense modifier，最终防御值仍由 C11/C19 commit owner 计算。
- bleed/tongued 对 C10 regen 的顺序必须由 focused verifier 固定，gross/confused 的外部行为通过明确 effect port；C15 不直接调用 Hurt 或变更 statLife。
- focused verifier：八个 alias 的 type/time/expiry、ResetEffects/death clear、cursed/silence item gate、slow/tongued movement gate、brokenArmor defense input、bleed/tongued regen ordering、重复 Buff command、C06/C13/C14 reader-only 和 projection purity。

跨分区依赖与回滚：

- 依赖 C06 recovery、C10 vital/regen、C11 defense、C13 elemental、C14 survival/transformation、C20 speed/permission、StatusEffects/BuffCollection、Movement/World、ItemContainer。
- 在 C06 与 C15 的 alias owner、Buff removal 事务边界和 effect ordering 未裁决前，禁止创建第二个 `PlayerDebuffAliasRebuildSystem`；兼容层只读旧 Player flags。
- 若发现 Buff type/time 与 alias 不一致、Movement/regen 产生第二 writer 或 alias 反向写 authority，回滚新 rebuild registration，保留共享 snapshot seam。

### C15 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerDebuffStatusAliasComponent.cs`。
- 字段覆盖：8/8；`cursed`、`bleed`、`confused`、`brokenArmor`、`silence`、`slow`、`gross`、`tongued` 均保存为当前状态 alias。
- 默认/重置：八个 alias 均为 `false`，组件不保存 Buff type/time、Item/NPC source、免疫或生命资源，并只提供内部 `ResetEffects`。
- 依赖影响：C06 recovery、C10 vital/regen、C11 defense、C13/C14 status、C20 speed/permission、StatusEffects/BuffCollection、Movement/World、ItemContainer 仍为 integration-review；未验证 alias expiry、regen/movement/defense ordering 或唯一 alias writer。

检查点结论：C15 组件源码已实现但未验证，8/8 字段已落地；Buff alias rebuild、effect Query 和 projection 仍未实现。

## 22. C16 PlayerAccessoryCombatModifierState

成员覆盖：kbGlove、autoReuseGlove、meleeScaleGlove、kbBuff、remoteVisionForDrone、starCloakItem、starCloakItem_manaCloakOverrideItem、starCloakItem_starVeilOverrideItem、starCloakItem_beeCloakOverrideItem。输入报告为 9/9，属性 0/0。

源码事实：

- Version4 `Player.cs:1805-1821` 确认四个手套/击退标记、无人机视野标记以及四个 `Terraria.Item` 星披风来源引用。
- `Player.cs:4588-4592` 由 Buff 设置 kbBuff；:8023-8027、:8182-8187、:8231-8236、:8593-8598、:8777-8782、:8933-8948、:9093-9096、:9213-9220 由装备/当前物品设置能力和星披风来源；这说明 source selection 依赖装备扫描顺序和 override 语义。
- `Player.cs:10675-10683` 在 ResetEffects 清理布尔值并将四个 Item 引用设为 null；:12596-12605、:23426-23431、:25719-25725、:25864-25873 分别消费击退/尺寸/自动复用能力。
- 星披风引用只用于后续配饰效果选择；核心组件不能保留 `Terraria.Item` 可变对象、数组槽位或裸引用。它们必须转换为 typed `ItemReference`，并由 P09 ItemContainer 解析当前定义。

proposed 边界：

- `PlayerAccessoryMeleeCapabilityComponent`：kbGlove、autoReuseGlove、meleeScaleGlove、kbBuff；提供近战参数和 item-use capability，不直接写 Projectile 或 combat damage。
- `PlayerDroneVisionCapabilityComponent`：remoteVisionForDrone；只输出视野/Presentation/Drone query，不能拥有 drone entity 或 camera。
- `PlayerStarCloakSourceRelationComponent`：四个星披风字段转为 `ItemReference?` relation，并保存 source revision/override kind；不复制 Item payload。
- `PlayerAccessoryCombatRebuildSystem`：在 ResetEffects 后依照稳定 equipment enumeration 提交能力和 source relation；`PlayerAccessoryCombatQuery` 纯计算 knockback/scale/reuse/vision；`PlayerStarCloakProjection` 将 relation 转为对外 snapshot。

接口契约：

- Interface：`IPlayerAccessoryCombatCommitPort.Commit(AccessoryCombatInput)`；Implementation：rebuild system；Seam：P09 Equipment/Item and StatusEffects；Depth：一层；Leverage：高；Locality：Player accessory combat。
- Interface：`IItemReferenceResolver.Resolve(ItemReference, Revision)`；Implementation：ItemContainer owner；C16 只能读取 typed definition/capability，不接受外部对象直接写入 component。
- Interface：`IPlayerAccessoryCombatQuery.Evaluate(AccessoryCombatSnapshot, ItemUseContext)`；Implementation：纯 Query；Seam：C12/C19/P15/P11；Projection 只读 relation，不能选择或 mutate source。

不变量与验证计划：

- ResetEffects 后四个能力 bool 为 false、四个 source relation 为空；同一 equipment rebuild 必须幂等，装备槽位复用不得让旧 ItemReference 继续生效。
- kbGlove 与 kbBuff 的 knockback multiplier 只在统一 Query 合并；meleeScaleGlove 只改变尺寸输入；autoReuseGlove 只改变 item reuse qualification。remoteVisionForDrone 不复制 drone state。
- 星披风 source、mana override、star veil override、bee cloak override 需要明确选择优先级和 source revision；解析失败时返回无 capability，不使用陈旧 Item payload。
- focused verifier：equipment order/rebuild idempotence、ItemReference identity/slot reuse、glove multiplier composition、auto reuse/remote vision purity、four-way star cloak override priority、reset/death/reconnect cleanup 和 projection one-way。

跨分区依赖与回滚：

- 依赖 P09 ItemContainer/Equipment、StatusEffects、C12 ammo/accessory、C17 resource/invulnerability、C18 drop/debuff、C19 damage/crit、P15 Projectile、P11 Presentation/Drone。
- 在 ItemReference resolver、equipment enumeration、星披风 override owner 和 P15 consumer 未裁决前，只迁移只读 Query；保留旧 source reader，禁止旧 Item object 与新 relation 双写。
- 若发现 source priority、slot reuse、glove 参数或 projection 发生差异，撤销新 relation resolver/reader migration，保留 typed relation 设计。

### C16 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerAccessoryMeleeCapabilityComponent.cs`、`src/Player/PlayerDroneVisionCapabilityComponent.cs`、`src/Player/PlayerStarCloakSourceRelationComponent.cs`。
- 字段覆盖：9/9。手套/Buff 能力保存 4 个标记，无人机视野保存 1 个标记；四个 Version4 `Item` 源引用映射为已有 `ItemEntityRef`，空值使用 `ItemEntityRef.None`。
- 默认/重置：能力标记为 `false`，Item relation 均为空引用；组件不保存 Item payload、数组槽位、source revision 或 override 行为。
- 依赖影响：P09 Equipment/Item、C12/C19/P15/P11、StatusEffects 和网络/投影仍为 integration-review；未验证 equipment enumeration、slot reuse、击退/尺寸/复用查询和 star-cloak source selection。

检查点结论：C16 组件源码已实现但未验证，9/9 字段已落地；Accessory Combat rebuild、Item resolver、Query 和 projection 仍未实现。

## 23. C17 PlayerAccessoryResourceAndInvulnerabilityState

成员覆盖：longInvince、pStone、PhilosopherStoneDurationMultiplier、manaFlower、moonLeech。输入报告为 5/5，属性 0/0。

源码事实：

- Version4 `Player.cs:1823-1831` 确认四个可变装备/Buff 标记和一个 `static readonly` duration multiplier；静态定义不是每个 Player 的 ECS component state。
- `Player.cs:6025-6029` 由 Buff 设置 moonLeech；:8778-8787、:9208-9228 由装备设置 longInvince/pStone/manaFlower；`Player.cs:10684-10701` 与 UpdateDead :9991 清理可变标记。
- `Player.cs:15193-15202` 使用 pStone 和 `PhilosopherStoneDurationMultiplier = 0.75f` 计算 potion/restoration/mushroom delay，并在 :15303-15305 处理 pStone 边沿；:22100-22121、:22325-22334 使用 longInvince 改变免疫时间；:25540-25543 以 manaFlower 触发 QuickMana。
- moonLeech 的声明、Buff 设置和 reset 已确认，但其在本地 Version4 片段中的完整消费闭合不足；必须保持 `partial`，不能据空缺推断资源或伤害语义。

proposed 边界：

- `PlayerAccessoryResourceProtectionComponent`：longInvince、pStone、manaFlower、moonLeech。它只提供免疫时长、药水延迟和 QuickMana/资源资格输入。
- `PotionDelayPolicyDefinition`：`PhilosopherStoneDurationMultiplier` 作为 immutable definition/versioned policy，不注册为玩家可变 component，也不允许网络客户端覆盖。
- `PlayerAccessoryResourceSystem`：从 equipment/Buff snapshot 唯一提交四个可变 flags；`PlayerResourceProtectionQuery` 返回 delay/invulnerability/mana-use input；`PlayerQuickManaCommand` 交给 Mana/Item owner。
- `PlayerResourceProjection` 只输出对外资源 capability snapshot；它不能直接写 statMana、potion delay、immuneTime 或 Buff。

接口契约：

- Interface：`IPlayerAccessoryResourceCommitPort.Commit(ResourceAccessoryInput)`；Implementation：resource system；Seam：P09 Item/Equipment and StatusEffects；Depth：一层；Leverage：中高；Locality：Player resource capability。
- Interface：`IResourceProtectionQuery.Evaluate(ResourceAccessorySnapshot, ImmutablePolicyDefinition)`；Implementation：纯 Query；Seam：C09/C10/C19 and Item/Mana owner；Query 不写 current resource。
- Interface：`IQuickManaCommandPort.Request(PlayerEntity, CommandId)`；Implementation：Mana/Item owner；C17 只发意图，不能调用或复制 QuickMana 内部 Item payload。

不变量与验证计划：

- longInvince 只改变受伤后免疫 timer policy，不能绕过 damage eligibility 或将 immuneAlpha 变成 authority；pStone 只改变 delay calculation，不能直接改时间字段两次。
- static multiplier 保持 0.75f 且版本化；pStone on/off 边沿的 remaining delay 调整必须是幂等的。manaFlower 的 QuickMana 触发必须在一个 command root 内，资源消耗和 potion/Buff 变化由外部 owner 提交。
- moonLeech 在消费语义未补证前只输出 opaque status fact，不推断其会改 statMana 或 damage；失效/死亡时由 Buff alias transaction 清除。
- focused verifier：default/reset、pStone delay calculation and edge、longInvince immunity timer、manaFlower quick-mana command/replay、moonLeech unknown-consumer guard、definition immutability、resource clamp 和 projection purity。

跨分区依赖与回滚：

- 依赖 C09 immunity、C10 vital/mana、C15 debuff/Buff alias、C16 star cloak/accessory、C19 damage、P09 Item/Equipment、StatusEffects、Network/Presentation。
- 在 QuickMana/resource commit root、longInvince owner、moonLeech consumer 和 policy version compatibility 未裁决前，只迁移 capability Query；保留旧 access readers，禁止新旧资源双写。
- 若发现 pStone 调整非幂等、longInvince bypass 资格、QuickMana 重放重复消费或 moonLeech 语义被猜测，回滚 command/projection migration，保持 component proposed。

### C17 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerAccessoryResourceProtectionComponent.cs`。
- 字段覆盖：5/5。四个可变配饰资源/保护标记保存在组件中；`PhilosopherStoneDurationMultiplier` 作为类型上的 `static readonly` 不可变策略值保存，不作为玩家实例状态。
- 默认/重置：四个布尔能力标记为 `false`；倍率固定为 `0.75f`，不提供网络或客户端覆盖入口；组件不写 statMana、potion delay、immuneTime 或 Buff。
- 依赖影响：C03/C09/C10、P09 Item/Buff、StatusEffects、Network/Persistence 仍为 integration-review；未验证 QuickMana、无敌时长、药水延迟、静态策略版本和 projection 单向性。

检查点结论：C17 组件源码已实现但未验证，5/5 字段已落地；资源保护 System、Query、Command 和 projection 仍未实现。

## 24. C18 PlayerAccessoryDebuffAndDropState

成员覆盖：vortexDebuff、trapDebuffSource、witheredArmor、witheredWeapon、slowOgreSpit、parryDamageBuff、ballistaPanic、JustDroppedAnItem。输入报告为 8/8，属性 0/0。

源码事实：

- Version4 `Player.cs:1833-1847` 确认七个状态/来源标记和一个大小写特殊的掉落瞬态字段。
- `Player.cs:5997-6024` 由 Buff effect 路径设置 vortexDebuff、witheredArmor/witheredWeapon、ballistaPanic、slowOgreSpit、parryDamageBuff；`Player.cs:9993-10005` 和 :10663-10705 处理死亡/ResetEffects 清理，其中 trapDebuffSource 位于 UpdateDead 清理边界。
- `Player.cs:14866-14869`、:17548-17551 消费 vortexDebuff 改变重力/垂直运动；:15557-15563 消费 slowOgreSpit；:15620-15635 将 witheredArmor/witheredWeapon 合并进防御/伤害修正；这些都是 C20/C19/C11 的只读 modifier 输入，C18 不直接写 movement/damage。
- `Player.cs:22585-22588` 消费 trapDebuffSource 触发 achievement side effect；:23570-23575 读取 JustDroppedAnItem 门控 held-item effects，:23834-23837 将其清除。当前 Version4 片段未形成完整的 JustDroppedAnItem 写入闭环，标记为 partial。

proposed 边界：

- `PlayerAccessoryDebuffCapabilityComponent`：vortexDebuff、trapDebuffSource、witheredArmor、witheredWeapon、slowOgreSpit、parryDamageBuff、ballistaPanic。它保存 Buff/accessory 事实和 source category，不直接应用速度、防御、伤害或 achievement side effect。
- `PlayerItemDropTransientComponent`：JustDroppedAnItem 作为一次 item-drop committed event 的短暂 projection/compat flag；真正的 drop command、ItemContainer mutation 和 spawned item 由 P09 owner 完成。
- `PlayerAccessoryDebuffSystem`：唯一重建七个 capability flags；`PlayerAccessoryDebuffQuery` 输出 gravity/movement/damage/defense/drop inputs；`PlayerDropEventAdapter` 将已提交 drop fact 转为兼容 flag 和 achievement event。
- `PlayerAccessoryDebuffProjection` 只输出已提交事实，不能从客户端表现或 achievement 回写状态。

接口契约：

- Interface：`IPlayerAccessoryDebuffCommitPort.Commit(AccessoryDebuffInput)`；Implementation：status/accessory system；Seam：StatusEffects/P09; Depth：一层；Leverage：高；Locality：Player status。
- Interface：`IPlayerDropCommitPort.Commit(DropCommand)` / `IPlayerDropEventReader.ReadCommitted(DropEvent)`；Implementation：ItemContainer owner；C18 只能消费 event，不能制造 Item entity 或改 stack。
- Interface：`IPlayerAccessoryDebuffQuery.Evaluate(AccessoryDebuffSnapshot)`；Implementation：纯 Query；Seam：C11/C19/C20/P04/P15；Query 只返回 immutable modifiers and event inputs。

不变量与验证计划：

- vortexDebuff 的 gravity/velocity 影响必须通过 Movement input；witheredArmor/witheredWeapon 的 defense/damage 顺序由 C11/C19 snapshot 固定；slowOgreSpit 不能与 C15 slow 形成隐式覆盖顺序。
- parryDamageBuff/ballistaPanic 的消费者未闭合前只作为 capability fact；不能假设它们会触发 damage, projectile 或 NPC effects。
- trapDebuffSource 只允许在已提交 death/achievement event 上触发一次；JustDroppedAnItem 必须以 drop command id/revision 幂等，清除不能吞掉后续 item state。
- focused verifier：Buff alias rebuild/expiry、vortex movement input purity、withered modifier composition、slow precedence、parry/ballista unknown consumer guard、trap achievement once-only、drop command/event/replay、JustDropped reset and projection one-way。

跨分区依赖与回滚：

- 依赖 C11 defense/environment、C15 debuff aliases、C19 damage/crit、C20 movement/permission、P04 lifecycle/achievement、P09 ItemContainer/Drop、P15 Projectile/NPC and StatusEffects。
- 在 JustDropped 写入源、drop event owner、achievement port 和 modifier ordering 未裁决前，只迁移 capability Query；保留旧 drop/achievement readers，禁止新旧 event 双发。
- 若发现 vortex/withered 顺序变化、trap event 重复、drop flag 无法追溯或 unknown consumer 被错误实现，回滚新 adapter/event projection，保留 status component 设计。

### C18 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerAccessoryDebuffCapabilityComponent.cs`、`src/Player/PlayerItemDropTransientComponent.cs`。
- 字段覆盖：8/8。七个配饰/Buff 减益能力保存为 capability facts；`JustDroppedAnItem` 单独保存为一次性 transient/compat 状态。
- 默认/重置：所有能力和 transient 标记为 `false`；组件不创建 Item entity、drop command、achievement event，也不直接应用重力、速度、防御或伤害。
- 依赖影响：C11/C15/C19/C20、P04 lifecycle、P09 ItemContainer/Drop、P15 Projectile/NPC、StatusEffects、Movement/World 仍为 integration-review；未验证 vortex/withered ordering、slow precedence、trap once-only 和 drop replay/reset。

检查点结论：C18 组件源码已实现但未验证，8/8 字段已落地；Accessory Debuff System、drop event adapter、Query 和 projection 仍未实现。

## 25. C19 PlayerCombatDamageAndCritModifiers

成员覆盖：meleeCrit、magicCrit、rangedCrit、meleeDamage、magicDamage、rangedDamage、rangedMultDamage、arrowDamageAdditiveStack、arrowDamage、bulletDamage、rocketDamage、minionDamage、minionKB、revolverCritChanceBonus。输入报告为 14/14，属性 0/0。

源码事实：

- Version4 `Player.cs:1851-1877` 确认三类基础暴击、武器类别伤害倍率、ranged multiplicative modifier、arrow additive stack、弹药类别倍率、召唤物伤害/击退和 revolver crit bonus 的默认值。
- `Player.cs:3074-3078` 公开 bow/gun/specialist effective damage 公式；:25886-25918 的 `GetWeaponDamageMultiplier` 按 Item 类别选择倍率；这些公式依赖 Item definition，但 C19 不拥有 Item 或 Projectile payload。
- `Player.cs:10335-10346`、:10394-10397 在 ResetEffects 将 damage multiplier/crit/minionKB 回到基线；:4382-4850、:6052-6170、:6939-6950 和 armor/accessory paths 累加装备/Buff/环境贡献。
- `Player.cs:15231-15233` 加入当前武器 crit，:15414-15418 体现 stealth 对 melee damage/crit 的追加与上限；:15532-15535 应用 mana sickness，:15628-15635 应用 witheredWeapon。消费顺序必须由 C19 snapshot verifier 固定，不能把 C18/C15 影响重复应用。
- `Player.cs:12607-12609` 以随机源比较 meleeCrit；:23489-23501 修改 revolverCritChanceBonus；:25859-25883 消费 minionKB/kb modifiers。随机暴击和 revolver bonus 都需要显式 RandomSource/command revision，不能隐藏在纯 Query。

proposed 边界：

- `PlayerWeaponDamageModifierComponent`：meleeDamage、magicDamage、rangedDamage、rangedMultDamage、arrowDamageAdditiveStack、arrowDamage、bulletDamage、rocketDamage；保存类别倍率与 additive/multiplicative inputs，不保存 Item base damage。
- `PlayerSummonerDamageModifierComponent`：minionDamage、minionKB；同一召唤物 capability 的生命周期一致，输出 typed summon damage/knockback snapshot。
- `PlayerWeaponCritModifierComponent`：meleeCrit、magicCrit、rangedCrit、revolverCritChanceBonus；crit chance 是计算输入，实际随机 roll 和 hit commit 属于 Weapon/Projectile combat system。
- `PlayerCombatDamageModifierRebuildSystem`：ResetEffects 后按 equipment/Buff/status snapshot 唯一提交 C19；`PlayerDamageMultiplierQuery` 纯计算 effective damage；`PlayerCritQuery` 只返回 chance/roll input；`PlayerDamageProjection` 只输出不可变 snapshot。

接口契约：

- Interface：`IPlayerDamageModifierCommitPort.Commit(DamageModifierInput)`；Implementation：C19 rebuild system；Seam：C11/C15/C18/Equipment/StatusEffects；Depth：一层；Leverage：高；Locality：Player combat.
- Interface：`IPlayerDamageQuery.Calculate(WeaponDefinition, DamageModifierSnapshot)`；Implementation：纯 Query；Seam：P15 Projectile/NPC/Weapon；不读取或写入 Player mutable state。
- Interface：`ICritRollPort.Roll(CritContext, CritChance, RandomSample)`；Implementation：combat owner；C19 提供 chance，不拥有 global RNG、hit side effect 或 target state。
- Interface：`IPlayerDamageProjection.Read(PlayerEntity)`；Implementation：network/telemetry projection；不能反写 modifiers 或 create damage event。

不变量与验证计划：

- 默认值：三类 crit 为 4；melee/magic/ranged/rangedMult/arrow/bullet/rocket/minion damage 为 1；arrow additive stack、minionKB、revolver bonus 为 0。ResetEffects 后必须回到基线。
- bow effective damage 的 additive/multiplicative 顺序、gun/specialist 选择和 Item category resolution 通过纯 Query 固定；同一 modifier 不可在 C19、Projectile 或 NPC 再应用。
- witheredWeapon、mana sickness、stealth、C15 debuff 和 C18 accessory effects 只能通过 ordered modifier snapshot 输入；C19 不直接改 `statLife` 或目标 NPC。
- crit chance 的 clamp、revolver bonus 的 random update、重复 hit command 和 random seed/replay 必须可复现；minionKB 只影响 summon weapon knockback。
- focused verifier：defaults/reset、bow/gun/specialist formula、additive vs multiplicative order、category fallback、crit bounds/seed replay、revolver bonus transaction、withered/mana/debuff ordering、summon damage/KB and projection purity。

跨分区依赖与回滚：

- 依赖 C10 vital/regen、C11 defense/penetration、C15 debuff、C18 withered/accessory、C20 speed/permission、P09 Item/Equipment、P15 Projectile/NPC、Random/Network/Telemetry。
- 在 ordered damage snapshot、crit random owner、revolver transaction、Projectile/NPC commit root 和 network DTO 未裁决前，只迁移纯 Query；保留旧 damage readers，禁止新旧 multiplier 双算。
- 若发现 effective damage 公式、modifier order、crit replay、minion knockback 或 target commit 改变，回滚新 Query/reader adapter，保持 C19 proposed。

### C19 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerWeaponDamageModifierComponent.cs`、`src/Player/PlayerSummonerDamageModifierComponent.cs`、`src/Player/PlayerWeaponCritModifierComponent.cs`。
- 字段覆盖：14/14。武器伤害组件保存 8 个类别/弹药倍率，召唤物组件保存 `minionDamage`/`minionKB`，暴击组件保存 3 个基础暴击和 `revolverCritChanceBonus`。
- 默认/重置：三类暴击为 `4`；武器/召唤物伤害倍率为 `1f`；additive stack、召唤物击退和 revolver bonus 为 `0`。
- 依赖影响：C10/C11/C15/C18/C20、P09 Item/Equipment、P15 Projectile/NPC、Random/Network/Telemetry 仍为 integration-review；未验证 bow/gun 公式、modifier ordering、crit replay、revolver transaction 和 minion KB。

检查点结论：C19 组件源码已实现但未验证，14/14 字段已落地；Damage Modifier rebuild、damage/crit Query、随机源和 hit commit 仍未实现。

## 26. C20 PlayerCombatSpeedRangeAndPermissionState

成员覆盖：IsAllowedToHoldItems、meleeSpeed、summonerWeaponSpeedBonus、moveSpeed、pickSpeed、wallSpeed、tileSpeed、autoPaint、autoActuator。输入报告为 9/9，属性 0/0。

源码事实：

- Version4 `Player.cs:1849-1893` 确认持物许可、攻击/召唤物速度、移动速度、挖掘/墙/Tile 速度和自动化权限字段；默认 speed 为 1，IsAllowedToHoldItems 默认 true。
- `Player.cs:10276` 每次 ResetEffects 将 IsAllowedToHoldItems 设为 true；:10334-10347、:10510-10513、:10657 将 melee/summoner/move/pick/wall/tile 和 auto flags 回到基线。
- Buff/equipment 入口在 `Player.cs:4354-4562`、:6783-6994、:7043-7976、:8182-9032 等路径累加速度/权限；这些输入应在同一个 equipment/status rebuild snapshot 中合并，不能依靠字段或目录顺序。
- `Player.cs:3454-3470` 将 melee/summoner/tile/wall speed 送入 item animation；:15536-15548 将 melee/summoner speed 转为 use-time multiplier、对 tile/wall 做 3 倍 cap 和 reciprocal；:15594-15598 对 pickSpeed 做最小值 0.3，并在 :15610-15611 将 moveSpeed 消费到 movement acceleration。
- `Player.cs:6784-6791`、:8905-8932 设置 autoPaint/autoActuator；:23570-23575 消费 IsAllowedToHoldItems；Tile/Wall action 的最终修改权仍属于 WorldInteraction/Tile owner。

proposed 边界：

- `PlayerAttackSpeedModifierComponent`：meleeSpeed、summonerWeaponSpeedBonus；保存攻击 use-time 输入，`CapAttackSpeeds`/item animation 由 combat/item owner 统一消费。
- `PlayerMovementSpeedModifierSnapshot`：moveSpeed。它是 Movement 的 immutable input，不拥有 velocity、runAcceleration、position 或 gravity。
- `PlayerBuildAndMiningSpeedComponent`：pickSpeed、wallSpeed、tileSpeed；统一记录 cap/reciprocal 前的 modifier，最终 Tile/Wall interaction owner 负责提交工具动作。
- `PlayerItemPermissionComponent`：IsAllowedToHoldItems、autoPaint、autoActuator。它表达当前 tick 的持物/自动化资格，不拥有 ItemContainer 或 Tile mutation。
- `PlayerSpeedPermissionRebuildSystem`：ResetEffects 后唯一提交 C20；`PlayerSpeedQuery` 纯计算 capped/use-time/movement inputs；`PlayerInteractionPermissionQuery` 只判断资格；Projection 只输出 snapshot。

接口契约：

- Interface：`IPlayerSpeedPermissionCommitPort.Commit(SpeedPermissionInput)`；Implementation：rebuild system；Seam：Equipment/StatusEffects/Movement/WorldInteraction；Depth：一层；Leverage：高；Locality：Player capability。
- Interface：`IPlayerSpeedQuery.Calculate(WeaponOrToolDefinition, SpeedPermissionSnapshot)`；Implementation：纯 Query；Seam：Item/Movement/Tile/Wall owners；Query 不改 acceleration/animation。
- Interface：`IPlayerInteractionPermissionQuery.CanHoldOrAutomate(ItemActionContext, PermissionSnapshot)`；Implementation：纯 Query；`autoPaint/autoActuator` 只作为 capability input。

不变量与验证计划：

- 默认值和 reset 必须保持：melee/move/pick/wall/tile 为 1、summoner bonus 为 0、IsAllowedToHoldItems 为 true、auto flags 为 false；C15/C14 等 status gates 在明确阶段重新覆盖。
- melee/summoner speed cap 和 reciprocal 转换只执行一次；tile/wall cap 为 3，pickSpeed lower bound 为 0.3，不能被 Tile owner 再倒数或重复 cap。
- moveSpeed 只影响 Movement input；P06 不写 velocity、runAcceleration、position。IsAllowedToHoldItems 与 noItems/cursed/JustDropped 的优先级由 item-use query 固定。
- autoPaint/autoActuator 不能让客户端绕过 Tile/World permission；任何外部 I/O、Tile mutation、animation side effect 通过 port/adapter。
- focused verifier：defaults/reset、attack speed cap/inversion、summoner conversion、pick/tile/wall boundaries、movement snapshot purity、permission precedence、auto capability rejection、duplicate rebuild and projection one-way。

跨分区依赖与回滚：

- 依赖 C14 survival、C15 debuff、C18 drop, C19 damage/crit、P09 Item/Equipment、Movement、WorldInteraction/Tile、P11 Presentation。
- 在 Movement/Tile/Item owners、permission priority 和 speed conversion root 未裁决前，只迁移 Query；保留旧 speed reader，禁止新旧 speed/permission writer 并行。
- 若发现 cap/inversion、permission gate、movement acceleration 或 Tile side effect 顺序变化，回滚新 speed adapter/query，保留 C20 proposed boundary。

### C20 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerAttackSpeedModifierComponent.cs`、`src/Player/PlayerMovementSpeedModifierComponent.cs`、`src/Player/PlayerBuildAndMiningSpeedComponent.cs`、`src/Player/PlayerItemPermissionComponent.cs`。
- 字段覆盖：9/9。攻击、移动、建造/挖掘和持物/自动化权限分别保存；`IsAllowedToHoldItems` 默认 `true`，speed modifier 默认 `1f`，summoner bonus 和 auto flags 默认 `0`/`false`。
- 重置：`ResetEffects` 恢复 Version4 基线；组件不写 velocity、run acceleration、position、animation、ItemContainer 或 Tile。
- 依赖影响：C14/C15/C18/C19、P09 Item/Equipment、Movement、WorldInteraction/Tile、P11 Presentation 仍为 integration-review；未验证 speed cap/reciprocal、pick lower bound、permission precedence、auto capability rejection 和 projection one-way。

检查点结论：C20 组件源码已实现但未验证，9/9 字段已落地；速度/权限 System、Query、Tile/Movement owner 和 projection 仍未实现。

## 27. C21 PlayerLuckAndCommerceEffects

成员覆盖：discountEquipped、discountAvailable、hasLuckyCoin、boneGloveItem、goldRing、accDivingHelm、accFlipper、deadCellsPotionStation、hasLuck_LuckyCoin、hasLuck_LuckyHorseshoe、hasLuck_LuckyClover、hasLuck_WiltedClover、hasLuck_RavenFeather。输入报告为 13/13，属性 0/0。

源码事实：

- Version4 `Player.cs:2030-2054` 确认 13 个字段；`boneGloveItem` 是 `Terraria.Item` 可变对象引用，拆分后必须变为 typed `ItemReference`/capability relation，不能复制 Item payload。
- `Player.cs:6691-6702` 设置 lucky accessory flags；`Player.cs:8273-8275` 设置 `boneGloveItem`；`Player.cs:8604-8629` 设置 discount、lucky coin 和 gold ring；`Player.cs:7177-7180`、`:4326-4350` 分别设置 diving helm 与 flipper；这些设置均属于装备/Buff capability rebuild 输入。
- `Player.cs:10276-10347`、`:10490`、`:10724-10731` 的 ResetEffects/初始化路径清理或恢复相关字段。`discountAvailable` 在 `Player.cs:10724-10731` 的非商店阶段从 `discountEquipped` 派生，不能在两个独立系统中并行持久化写入。
- `Player.cs:3683-3688` 在 `AddBuff_DetermineBuffTimeToAdd` 中用 `deadCellsPotionStation` 延长特定 Buff 时长；它是 Buff timing policy input，不是第二个 Buff timer owner。
- `Player.cs:19996-20002` 读取 `goldRing` 扩展金币拾取范围；`Player.cs:17869-17928` 的 `UpdateLuck`/`RecalculateLuck` 使用已提交 luck inputs。幸运查询只能计算输入，不得直接生成掉落、修改库存或提交随机结果。

proposed 边界：

- `PlayerCommerceCapabilityComponent`：discountEquipped、hasLuckyCoin、goldRing，以及作为派生可用性对外发布的 `discountAvailable`。其中 `discountAvailable` 优先建模为当前商店上下文下的 derived snapshot；只有在兼容 DTO 明确要求字段存在时才生成投影，不将其作为第二份持久化 authority。
- `PlayerLuckCapabilityComponent`：hasLuck_LuckyCoin、hasLuck_LuckyHorseshoe、hasLuck_LuckyClover、hasLuck_WiltedClover、hasLuck_RavenFeather。它保存能力事实，不持有随机源、掉落表或世界状态。
- `PlayerWaterAndUtilityCapabilityComponent`：accDivingHelm、accFlipper、deadCellsPotionStation。它发布对 Liquid/Movement utility 与 Buff timing 的只读 policy inputs；不直接改 gravity、velocity、Buff type/time 或环境实体。
- `PlayerBoneGloveSourceRelationComponent`：`boneGloveItem` 转为 typed `ItemReference?` 和 source revision；P09 Item owner 负责解析当前 definition/capability，P06 不保存 `Terraria.Item` 对象。
- `PlayerLuckAndCommerceRebuildSystem` 是本检查点唯一的 capability commit writer；`PlayerLuckAndCommerceQuery` 计算 luck/discount/pickup inputs；`PlayerCommerceProjection`、`PlayerLuckProjection` 只输出不可变快照。

接口契约：

- Interface：`IPlayerLuckAndCommerceCommitPort.Commit(LuckCommerceInput)`；Implementation：唯一 rebuild system；Seam：P09 Equipment/Item、StatusEffects、Buff timing、Liquid/Movement；Depth：一层；Leverage：高；Locality：Player capability。
- Interface：`IPlayerLuckAndCommerceQuery.Evaluate(LuckCommerceSnapshot, CommerceContext)`；Implementation：纯 Query；输出 luck contribution、discount eligibility、coin pickup input 和 utility policy input；Query 不写 World、ItemContainer、Buff 或随机源。
- Interface：`IItemReferenceResolver.Resolve(ItemReference, Revision)`；Implementation：P09 Item owner；C21 只能消费 typed definition/capability，不能让外部 `Item` 实例反向写入组件。
- Interface：`IBuffTimingPolicyQuery.GetDurationInput(BuffDefinition, PlayerUtilitySnapshot)`；Implementation：Buff owner；`deadCellsPotionStation` 只提供 policy input，Buff owner 仍是 type/time 的唯一提交者。

不变量与验证计划：

- ResetEffects 后 capability flags、source relation 和临时 utility flags 必须回到默认值；装备重建必须幂等，不能因槽位复用残留旧 `ItemReference` 或 luck flag。
- `discountAvailable` 必须由明确的 shop/non-shop context 派生，不能与 `discountEquipped` 形成隐式双写；客户端投影不能成为折扣 authority。
- `hasLuckyCoin` 与 `hasLuck_LuckyCoin` 的关联必须通过显式规则/查询固定；其它 luck flags 只作为 capability inputs。掉落、金币堆叠、库存数量和随机 roll 由 World/Item/Combat owner 提交。
- `deadCellsPotionStation` 只能影响允许延长的 Buff definitions；Buff type/time、免疫和过期仍由 StatusEffects owner 原子提交。`accFlipper`/`accDivingHelm` 只发布 utility input，不能在 Query 中写水中移动或重力。
- focused verifier：13 字段来源/默认/reset、equipment rebuild 幂等、ItemReference identity/slot reuse、discount context derivation、coin pickup range、luck deterministic calculation、Dead Cells Buff timing policy、water utility input purity、drop/inventory/random side-effect isolation 和 projection one-way。

跨分区依赖与回滚：

- 依赖 C13 elemental/shimmer、C14 survival/transformation、C19 damage/crit、C20 speed/permission、P09 ItemContainer/Equipment、StatusEffects/Buff、Liquid/Movement、World drop/economy、Network/Presentation。
- 在 discount owner、luck random/drop owner、ItemReference resolver、Buff timing commit root 和 Liquid/Movement utility owner 未裁决前，只迁移只读 Query/Projection；保留旧 reader，禁止新旧 capability 双写。
- 若发现 discountAvailable 与 shop context 不一致、luck inputs 被重复应用、ItemReference 指向陈旧槽位、Buff duration 产生第二 writer 或 utility Query 改变外部运动，撤销本批 reader/adapter migration，保留 proposed component/seam。

### C21 本地源码 checkpoint

- implementationStatus: implemented
- verificationStatus: not-verified
- 实际源码：`src/Player/PlayerCommerceCapabilityComponent.cs`、`src/Player/PlayerLuckCapabilityComponent.cs`、`src/Player/PlayerWaterAndUtilityCapabilityComponent.cs`、`src/Player/PlayerBoneGloveSourceRelationComponent.cs`。
- 字段覆盖：13/13。商业能力保存 discount/lucky coin/gold ring；幸运组件保存五个 `hasLuck_*` 标记；水/工具组件保存 diving helm、flipper、Dead Cells policy；`boneGloveItem` 映射为 `ItemEntityRef` 关系。
- 默认/重置：所有布尔能力为 `false`，Bone Glove relation 为 `ItemEntityRef.None`；不直接修改掉落、库存、Buff type/time、Liquid、Movement 或随机源。
- 依赖影响：C07 luck input、C13/C14/C20、P09 Item/Equipment、StatusEffects/Buff、Liquid/Movement、World economy、Network/Presentation 仍为 integration-review；未验证 discount context、luck calculation、coin pickup、Dead Cells timing、ItemReference identity 和 projection purity。

检查点结论：C21 组件源码已实现但未验证，13/13 字段已落地；Luck/Commerce rebuild、Query、Item resolver、Buff timing 和 projection 仍未实现。

## 28. C22 PlayerDpsTelemetryState

成员覆盖：dpsStart、dpsEnd、dpsLastHit、dpsDamage、dpsStarted。输入报告为 5/5，属性 0/0。

源码事实：

- Version4 `Player.cs:2018-2026` 确认三项 `DateTime`、一个累计伤害整数和一个窗口开关；它们属于观测状态，不应并入生命、伤害修正或受击资格组件。
- `Player.cs:26241-26260` 的 `addDPS` 是当前已定位的状态写入入口：窗口未开始时以本次伤害初始化 `dpsStart/dpsEnd/dpsLastHit/dpsDamage`，已开始时累加 `dpsDamage` 并更新结束/最近命中时间；所有时间读取使用 `DateTime.Now`。
- `Player.cs:11971-11975` 在 NPC `StrikeNPC` 返回实际伤害后调用 `addDPS`；`Projectile.cs:12516-12519` 在 Projectile 对 NPC 的 `StrikeNPC`/`StrikeNPCNoInteraction` 完成后调用。当前证据支持“消费已提交/已结算的伤害结果”，不支持消费原始 attempted hit 或直接参与 damage formula。
- `Player.cs:7000-7004` 在 `accDreamCatcher` 缺失且窗口已开始时关闭窗口并写入 `dpsEnd`；该 reset/stop 边界依赖装备能力重建。网络和持久化字段闭合仍未证实，且 `Serialize`/`Deserialize` 在 `Player.cs:26417-26418、:26455-26458` 当前为空实现，不能声称已有 Version4 wire format。

proposed 边界：

- `PlayerDpsTelemetryComponent`：五个字段作为单一观测窗口状态；`dpsDamage` 只累计已提交伤害事件，不能成为生命或伤害 authority。已实现的 Component 不访问 `DateTime.Now`、日志、UI、网络或存档。
- `IPlayerTelemetryClock`：已实现为显式 `DateTimeOffset UtcNow` port；本地 core 不把 wall-clock 转换成 monotonic duration。旧 `DateTime.Now` 的兼容时区/精度规则仍由 integration-review 决定，不进入 Component。
- `PlayerDpsTelemetrySystem`：本地 core 的唯一写入 System，消费 `PlayerCommittedDamageEvent` 的 `EventId`、正 `Damage`、`CommittedAt` 和 source revision；负责窗口启动、累加、last-hit/end 更新、显式能力撤销 seam 和 reset。source revision 当前只作为事件契约字段携带，不在 P06 core 内推断跨源排序。
- `PlayerDpsTelemetryQuery`：已实现为纯计算当前窗口 duration、DPS、最近命中和是否活跃；零时长、负时间、溢出和重复 event policy 在本地 core 中分别以零 DPS、非负 duration、`int.MaxValue` 饱和和 EventId 至多一次处理，Query 不写回。
- `PlayerDpsTelemetryProjection`：单向输出 UI/network/diagnostic DTO；C22 不向 Combat、Vital、Crit、NPC 或 Projectile 回写。

接口契约：

- Interface：`IPlayerDpsTelemetryCommitPort.AcceptCommittedDamage(PlayerCommittedDamageEvent)`；Implementation：已实现的唯一本地 telemetry system；Seam：NPC/Projectile combat event bus and Player capability state，外部接入仍为 integration-review；Depth：一层；Leverage：中；Locality：Player combat observation。
- Interface：`IPlayerTelemetryClock.UtcNow`；Implementation：已实现的 clock port；系统之外的 wall clock/monotonic conversion 不进入 Query/Component。
- Interface：`IPlayerDpsTelemetryQuery.Snapshot(PlayerDpsTelemetryComponent, DateTimeOffset)`；Implementation：已实现的纯 Query；输出 immutable `PlayerDpsTelemetrySnapshot`，不改变窗口状态。
- Interface：`IPlayerDpsTelemetryProjection.Project(DpsSnapshot)`；Implementation：ClientPresentation/Network/Diagnostics adapters；只读消费，不能生成 damage event 或修改 C22。

不变量与验证计划：

- 只有实际提交的正/定义允许的伤害事实可计入 `dpsDamage`；attempted hit、被免疫/取消/重复投递的事件不得计入。事件必须以 stable `eventId`/revision 去重，重放不能重复累计。
- 窗口启动时五字段必须原子初始化；累计时 `dpsStart` 不漂移、`dpsLastHit` 与最后接受事件一致、`dpsEnd` 的含义固定为最后提交时间或明确的观察结束时间，不能在不同调用点混用。
- `accDreamCatcher` 撤销、死亡、重生、断线和显式 reset 的清理边界必须唯一；关闭窗口不能把 telemetry projection 当作 combat state mutation。
- 时钟精度、时区、单调性、未来时间和零时长行为必须通过 clock/query verifier 固定；`dpsDamage` 溢出策略、负伤害策略和网络序列化精度必须明确。
- focused verifier：committed NPC/Projectile damage only、window start/accumulate/end、duplicate event/replay、clock injection、zero/negative duration、overflow/negative damage、Dream Catcher capability removal、death/respawn reset、projection purity 和 telemetry-to-authority isolation。

跨分区依赖与回滚：

- 依赖 C02/C03/C09/C10/C19 的 committed damage/eligibility facts、P12 NPC combat、P15 Projectile、P09 equipment capability、P04 lifecycle、Network/ClientPresentation/Diagnostics、clock and persistence adapters。
- 在所有 committed damage event source、telemetry owner、旧 `addDPS` 兼容切换、reset policy 和 network/persistence DTO 未裁决前，本次只实现本地 event/query seam；保留旧 `addDPS` 行为作为兼容读写源，禁止新旧 telemetry writer 并行。当前代码没有接管旧 writer。
- 若发现 raw hit 被错误计入、Projectile/NPC 重复发事件、窗口时间语义不一致、reset 跨生命周期泄漏或 projection 反向写 authority，回滚新 event adapter/system registration，保留 C22 proposed component 和 verifier 计划。

检查点结论：C22 设计完成，5/5 字段已归属；`C22.PlayerDpsTelemetryCore` 已实现并通过 focused verifier。NPC/Projectile source、生命周期和 projection 仍为 integration-review，不能据此宣称 Version4 全调用链或网络/持久化兼容。

### C22 本地实现 checkpoint

本次实际实现只覆盖一个不依赖未裁决外部 owner 的最小闭环：

- `src/Player/PlayerDpsTelemetryComponent.cs` 保存五个 C22 观测字段；组件不读取系统时钟、不写战斗 authority。
- `src/Player/PlayerCommittedDamageEvent.cs` 携带稳定 `EventId`、正伤害、提交时间和 source revision。
- `src/Player/IPlayerTelemetryClock.cs`、`IPlayerDpsTelemetryCommitPort.cs` 和 `IPlayerDpsTelemetryQuery.cs` 明确时间、提交和纯查询边界。
- `src/Player/PlayerDpsTelemetrySystem.cs` 是本地 core 的唯一 writer，执行正伤害检查、事件 ID 去重、窗口启动/累加、`int` 饱和和显式停止/reset；停止先读取 clock，再提交关闭状态，避免时钟端口失败留下半提交窗口。
- `src/Player/PlayerDpsTelemetryQuery.cs` 与 `PlayerDpsTelemetrySnapshot.cs` 只计算非负窗口时长、DPS 和只读快照；负时间和零时长返回零时长/零 DPS。
- `src/PlayerDpsTelemetryVerification/Program.cs` 覆盖首次提交、累加、重复事件、无效伤害、注入时钟、停止、能力撤销、reset 后重放集合清理、早于窗口开始的观察时间、Query 前后组件字段不变、纯 Query 和溢出饱和。

本地验证没有接管 NPC/Projectile 的 committed event source，没有替换 Version4 `addDPS`，没有把 `accDreamCatcher`、死亡/重生/断线接入推测性生命周期回调，也没有创建 Network/Persistence/Presentation projection。`PlayerDpsTelemetrySystem.ReconcileCapability(false)` 只提供本地停止 seam；实际能力重建调用点仍由 P09/P04/integration-review 决定。

## 29. 全分区最终设计契约

### 29.1 成员覆盖与组件所有权

本分区完成 22 个叶子检查点，覆盖字段 254/254、属性 0/0、成员合计 254/254；没有将字段按数量机械合并为一个 `PlayerCombatStateComponent`。C01-C22 的候选 owner 如下：

| 检查点 | 成员数 | proposed owner | 主要写入/提交边界 |
|---|---:|---|---|
| C01 | 14 | PlayerAccessoryStringEffectComponent | accessory effect rebuild |
| C02 | 15 | PlayerCombatProcStateComponent | proc/timeout commit |
| C03 | 5 | PlayerDodgeAndImmunityStateComponent | eligibility commit |
| C04 | 4 | PlayerBarrierAndRegenComponent | barrier/regen capability commit |
| C05 | 10 | PlayerManaActivityComponent | mana activity policy |
| C06 | 16 | PlayerDebuffRecoveryStateComponent | recovery/status commit |
| C07 | 11 | PlayerCombatDetectionStateComponent | detection/combat input rebuild |
| C08 | 20 | PlayerSocialAndDefenseStateComponent plus integration seams | social/defense capability commit |
| C09 | 11 | PlayerHitFrameImmunityComponent | hit-frame/immunity commit; frame projection separate |
| C10 | 13 | PlayerVitalStateComponent, PlayerLifeRegenComponent, PlayerManaRegenComponent | one vital/resource commit root |
| C11 | 16 | PlayerCombatModifierAndImmunityStateComponent | modifier/environment permission snapshot |
| C12 | 12 | PlayerAmmoAndAccessoryEffectsComponent | ammo/accessory capability rebuild |
| C13 | 21 | PlayerElementalAndShimmerStatusComponent | elemental/shimmer input commit |
| C14 | 15 | PlayerSurvivalNeedComponent, PlayerTransformationCapabilityComponent, PlayerEnvironmentalPressureComponent, PlayerTridentCapabilityComponent | survival/transformation/environment input commit |
| C15 | 8 | PlayerDebuffStatusAliasComponent | Buff-derived alias rebuild |
| C16 | 9 | PlayerAccessoryMeleeCapabilityComponent, PlayerDroneVisionCapabilityComponent, PlayerStarCloakSourceRelationComponent | accessory source/capability rebuild |
| C17 | 5 | PlayerAccessoryResourceProtectionComponent plus PotionDelayPolicyDefinition | resource capability and immutable policy |
| C18 | 8 | PlayerAccessoryDebuffCapabilityComponent, PlayerItemDropTransientComponent | status rebuild and committed drop event adapter |
| C19 | 14 | PlayerWeaponDamageModifierComponent, PlayerSummonerDamageModifierComponent, PlayerWeaponCritModifierComponent | ordered damage/crit snapshot |
| C20 | 9 | PlayerAttackSpeedModifierComponent, PlayerMovementSpeedModifierSnapshot, PlayerBuildAndMiningSpeedComponent, PlayerItemPermissionComponent | speed/permission rebuild |
| C21 | 13 | PlayerCommerceCapabilityComponent, PlayerLuckCapabilityComponent, PlayerWaterAndUtilityCapabilityComponent, PlayerBoneGloveSourceRelationComponent | luck/commerce/utility rebuild |
| C22 | 5 | PlayerDpsTelemetryComponent | committed damage telemetry system |

`HealthComponent`/`ManaComponent` 与 C10 proposed vital components 不能并行写同一资源；重复语义必须在实施前由 owner decision 和 focused verifier 消解。`discountAvailable`、`JustDroppedAnItem`、frame/alpha 等字段可能是兼容/派生/投影状态，不能仅凭原始字段名认定为独立持久 authority。

### 29.2 Component/System/Query/Command/Adapter/Projection 合同

- Component 只保存一个内聚概念的权威状态；不持有 `Terraria.Item`、Buff collection、NPC/Projectile 可变对象、全局随机源、时钟、日志 writer、网络 socket 或 UI 对象。
- System 是唯一状态提交者；所有 reset、重建、超时、生命周期转换和事件接收必须在显式 commit port 中发生，并记录输入 revision、失败策略和重复处理规则。
- Query 只接收不可变 snapshot/definition/context，返回纯 calculation 或 eligibility 结果；不得写 ECS、发网络、改 Buff、创建 Item、推进时间或调用随机副作用。
- Command 表示一次性意图，例如 damage/status/drop/QuickMana/interaction；Command 必须有 source/entity、revision/idempotency key 和边界 owner，不能伪装成可变组件。
- Event 表示已提交事实，例如 committed damage、death/status/drop；Event 必须区分 attempted、accepted、committed、projected 阶段，Telemetry/Network/Presentation 只能消费允许的阶段。
- Adapter 转换 Item/Buff/World/Liquid/Network/Persistence/clock/random/logging 等外部类型；Adapter 不取得 P06 authority，也不绕过唯一 commit root。
- Projection/Snapshot 是单向、版本化、不可变输出；客户端、UI、网络和诊断不能反写 P06 authority。兼容字段只通过 read adapter 暴露，迁移窗口禁止旧新双写。

### 29.3 跨分区依赖表

| 依赖边界 | P06 输出 | 外部 owner 输入/责任 | 当前状态 |
|---|---|---|---|
| P04 PlayerLifecycle | vital/status/telemetry reset inputs | death/respawn/active/ghost lifecycle commit | integration-review |
| P05 Progression/Pets | accessory, pet and capability definitions | progression ownership and persistent unlocks | integration-review |
| P09 Inventory/Equipment | typed `ItemReference`, equipment snapshot | Item payload, slot identity, stack/inventory mutation | integration-review |
| P12 NPC Combat | committed NPC damage result | NPC target/life/kill commit and event id | integration-review |
| P15 Projectile | committed projectile hit result | projectile authority, hit deduplication, source revision | integration-review |
| StatusEffects/Buff | alias/policy inputs | Buff type/time/immunity and expiry transaction | integration-review |
| Liquid/Movement/WorldInteraction | speed, wet, gravity, water and permission inputs | position/velocity/gravity/Tile/World mutation | integration-review |
| World/Economy/Drop | luck/discount/pickup inputs | random drop, coin entity, inventory/economy mutation | integration-review |
| NetworkSession | versioned read-only snapshots | authoritative replication and client command validation | integration-review |
| Persistence | proposed DTO boundary | file/schema ownership; Version4 Serialize/Deserialize evidence absent | persistence-blocked |
| ClientPresentation/Diagnostics | projections and telemetry snapshots | UI/render/log sinks, no authority writes | integration-review |

### 29.4 Scheduler and ordering contract

文件、目录和组件注册顺序不定义运行时顺序。实现阶段必须注册显式 scheduler phases，并让 verifier 断言依赖关系：

1. `HydratePlayerIdentityAndBaseState`
2. `ReadNetworkAndWorldInputs`
3. `ResetDerivedPlayerCapabilities`
4. `CommitEquipmentAndBuffCapabilities`
5. `CommitEnvironmentAndTransformationInputs`
6. `FreezePlayerCombatSnapshot`
7. `EvaluateEligibilityAndPermissions`
8. `CalculateDamageAndCrit`
9. `CommitDamageVitalImmunityAndProcFacts`
10. `CommitDeathAndLifecycleTransitions`
11. `CommitRegenAndTimeouts`
12. `ConsumeCommittedEventsForTelemetry`
13. `ProjectNetworkPersistencePresentation`

这是 proposed contract，不是已确认的 Version4 调用顺序；迁移前必须以 focused verifier 回放 `Player.Update`、`ResetEffects`、`Hurt`、`UpdateLifeRegen`、`UpdateManaRegen`、NPC hit 和 Projectile hit 的实际调用关系。任何跨 phase 写 authority 的系统都应被拒绝。

### 29.5 Network and persistence boundaries

- Network DTO 只包含版本化、不可变、最小必要 snapshot；EntityReference、content ID、network ID、persistence ID 和旧数组槽位保持不同类型。客户端输入转换为 Command，经 authority validation 后才产生 committed Event。
- C03/C09 immunity、C10 vitals、C19 modifiers、C21 luck/commerce 和 C22 telemetry 不共用一个无版本的 Player blob；每个 DTO 字段必须标记 authority、derived 或 presentation role。
- Persistence adapter 只能读写持久 owner 定义的 DTO；不可把 `Terraria.Item`/Buff runtime object 或 `DateTime.Now` 隐式序列化进组件。Version4 `Serialize`/`Deserialize` 当前为空实现，因此本分区 persistence 为 `persistence-blocked`，不声称字段 wire compatibility。
- `SerializedClone` 也不能被当作完整存档格式证据；需要补证字段顺序、版本迁移、缺省值、异常和回滚策略后，才允许实现持久化 adapter。

### 29.6 Compatibility and side-effect isolation

- 兼容读取通过 adapter/projection；实施时每个批次只切换一个 writer，旧字段保留为 read-only compatibility source，禁止新旧双写。
- Item/Equipment：P06 只保存 typed reference/capability snapshot，Item owner 负责 payload、槽位、stack 和消费。
- Buff：P06 只提供 policy/alias input，Buff owner 成对提交 type/time/immunity/expiry；`deadCellsPotionStation` 不创建第二 timer。
- World/Liquid/Movement/Tile：P06 发布 input/query result，外部 owner 写 position、velocity、gravity、Tile、coin/drop entity 和环境副作用。
- Randomness：crit、luck、drop 和 replay 使用显式 RandomSource/seed/revision；纯 Query 不取得或推进 RNG。
- Clock：DPS 使用显式 clock port；不在 Component/Query 中读取 `DateTime.Now`，不混用 wall clock 与 monotonic duration。
- Logging/Network/Persistence/Presentation：通过 adapter/projection 单向输出，失败、重试、去重和版本策略必须可观测、可测试，不能反向改变 authority。

### 29.7 Focused verifier plan and expected evidence

| verifier | 必须证明 | 预期证据 |
|---|---|---|
| Member coverage | 254/254 字段、0/0 属性无漏项/重复 | report 对照脚本与逐组表 |
| Lifecycle/reset | default、ResetEffects、death/respawn/reconnect 边界 | source anchor + state transition tests |
| Unique writer | 每个 authority 只有一个 commit root，无新旧双写 | writer registry + event trace |
| Pure Query | 输入相同则输出相同，无外部写入 | query tests + side-effect audit |
| Modifier order | C10/C11/C15/C18/C19/C20 precedence 不重复 | ordered snapshot trace |
| Item/Buff boundary | ItemReference、Buff type/time、policy adapter 不渗透 | adapter contract tests |
| Damage/telemetry | committed-only、dedup、replay、clock injection | NPC/Projectile event tests |
| Network/persistence | DTO 版本、ID 类型、缺省值与 blocked status | schema review; no claim until evidence |
| Scheduler | phase dependency 不依赖文件顺序 | scheduler graph + execution trace |
| Projection | network/UI/diagnostics 单向、不可写 authority | projection mutation tests |

本次已完成 C22 本地 core 的实现和 focused verifier；全分区成员覆盖、跨分区事件接入、scheduler、网络和持久化验证仍未完成，`verificationStatus` 仅表示 `focused-c22-passed`，不表示全分区通过。

实际验证记录：

- Build：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建 `src/PlayerDpsTelemetryVerification/Terraria.PlayerDpsTelemetryVerification.csproj`，退出码 0，0 个警告、0 个错误；产物为 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 和 `Build/bin/Terraria.PlayerDpsTelemetryVerification/Debug/net10.0/Terraria.PlayerDpsTelemetryVerification.dll`。
- Focused verifier：同一项目通过 `Invoke-SerialDotnet.ps1` 以 `run --project --no-build --no-restore` 执行，退出码 0，输出 `PASS: player DPS telemetry committed-event lifecycle and pure query`；包含 stop 时钟提交顺序、capability removal、reset replay-set 清理、早于窗口开始时的零时长/零 DPS 和 Query 不修改五个组件字段断言。
- 未验证：P12/P15 的真实 committed-damage caller、旧 `addDPS` 替换/兼容、`accDreamCatcher`/死亡/重生/断线调用、projection、scheduler、网络和持久化。

### 29.8 Integration Handoff

以下决策必须由相应跨分区 owner 在实施前确认：

- C10 与既有 `HealthComponent`/`ManaComponent` 的唯一资源 writer、damage/death commit root。
- C03/C09/C17 的 immunity、hurt cooldown、frame/alpha 的 authority 与表现投影边界。
- C06/C15/C18 的 Buff alias、type/time/immunity、expiry 和 modifier ordering owner。
- P09 对 ItemReference、装备遍历顺序、slot reuse、stack mutation、drop/coin entity 的 resolver/commit owner。
- C13/C14/C20/C21 与 Liquid/Movement/WorldInteraction 对 wet、shimmer、gravity、speed、utility 和 permission 的交接。
- P12/P15 的 committed damage event ID、去重、replay 及 C22 telemetry 唯一消费路径。
- NetworkSession 与 Persistence 对 snapshot version、ID 类型、缺省值、`SerializedClone` 和 `Serialize`/`Deserialize` 证据的处理；当前 persistence 保持 blocked。

### 29.9 Implementation declaration

本分区本次创建了 C07-C21 的 46 个组件源码文件；C22 本地 core、C01-C05 的既有源码和 focused verifier 记录保持不变。没有创建或修改网络协议、存档格式、运行时注册、Version4 或其他分区文件。C09 presentation/Town handoff、C22 外部接入、运行时注册、网络和持久化仍为 integration-review。当前执行状态为 `executionStatus: failed`、`implementationStatus: partial`、`verificationStatus: source-only-c06-c21-not-verified`，不代表全分区行为等价；runner 已按原始 manual session 记录 Fail。

### 29.10 本次组件源码构建记录

- 命令：`pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Player\Terraria.Player.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`
- 项目：`src/Player/Terraria.Player.csproj`，目标框架 `net10.0`；退出码 `1`；`0` 个警告、`5` 个错误。
- 真实错误：既有 `src/Player/Progression/PlayerMinionCapacityCommitSystem.cs` 的 `Terraria.Relationships` 引用，以及既有 `src/Player/Progression/SubmitMinionCapacityDeltaCommand.cs` 的 `Terraria.Projectile`、`Terraria.Relationships`、`EntityReference` 和 `ProjectileIdentityComponent` 引用未在当前项目提供。
- 预期产物路径：`Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`；该文件存在但时间戳早于本次失败构建，未被本次构建刷新，不能作为成功编译证据。
- C07-C21 组件源码未新增上述非组件错误；没有再添加测试、verifier、System、Query、Command、Adapter、Projection、项目文件或构建脚本来绕过失败。
