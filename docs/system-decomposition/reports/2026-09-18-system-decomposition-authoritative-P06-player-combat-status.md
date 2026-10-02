# System Decomposition Report: authoritative P06

partitionId: P06  
taskId: AUTH-SYS-P06  
taskSetName: authoritative-system-decomposition  
sessionId: 791756dc1f954ff3afcd84b5ef3fe296  
designStatus: proposed  
evidenceStatus: partial  
verificationStatus: not-run  
sourceModified: false

## Scope and Evidence

本报告只处理 claim 输入 P06：`PlayerGameplay` 下 22 个叶子组、254 个字段、0 个属性。它提出后续 System/API 边界，不实现迁移，不证明行为等价或迁移成功。输入 ledger、claim prompt 和本报告路径如下：

| 用途 | 路径 |
|---|---|
| authoritative 输入 | `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P06-Player-Combat-Status.md` |
| claim 专属 prompt | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-11-version4-authoritative-20-partition-prompts\2026-09-11-version4-P06-player-combat-status-public-decomposition.md` |
| 唯一授权输出 | `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P06-player-combat-status.md` |

证据分层：

| 证据源 | 本次读取或查询 | 可支持的结论 | 限制 |
|---|---|---|---|
| Version4 源码 | `Terraria/Player.cs`、`MessageBuffer.cs`、`NetMessage.cs`、`Projectile.cs`；Player SHA-256 `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`；MessageBuffer `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE`；NetMessage `87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A`；Projectile `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B` | 实际读取到的分支、直接写入、同文件调用次序和外部可见副作用 | Version4 目录没有可用 Git HEAD；局部源码不穷尽反射、mod hook、间接写入及所有运行时入口 |
| CPG 只读查询 API | `.agents/skills/ecs-system/tools/CpgEvidence.ps1`；SQLite `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`；manifest SHA-256 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；`ImportStatus=complete`，967 shards、8,166,789 nodes、71,038,907 edges、1,317 diagnostics，`SourceSnapshotId=null` | 选定源码 shard 中的符号、成员使用候选、精确静态调用边和 callable fact 状态 | 索引未绑定当前源码快照；查询完成只表示所选索引范围完整，不证明全局调用闭包、动态分派、alias/callee effects、事件注册、序列化或调度顺序 |
| 当前 NLTX | `D:\TRbackup\NLTX\src\NSSLC` 中 `Component/Combat` 与 `Component/Player` 相关类型；工作树启动前已含其他路径的大量变更 | 当前可读的组件、System/Query 声明及静态源码引用 | 声明存在不证明唯一权威写者、生产入口、运行时注册或 P06 行为已迁移；本任务不修改这些文件 |
| Space Station 14 | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Damage\Systems\DamageableSystem.API.cs`、`DamageableSystem.cs`、`StatusEffectNew/StatusEffectSystem.API.cs` | 只参考将 API 暴露面与 System 实现分文件、由 System 协调组件和外部服务的组织方式 | 不据此推断 Terraria 的伤害、Buff、调度或网络语义 |

本次按 `CpgEvidence.ps1` 初始化只读 SQLite reader 后调用 `Find-CpgSymbols`、`Get-CpgTypeSurface`、`Get-CpgMemberUses`、`Find-CpgCallSites`、`Get-CpgCallableFacts`。`Terraria.Player` 的 type surface 查询返回 1,511 个成员（972 fields、467 methods、72 properties），选中 shard 状态为 `complete`。P06 的 254 个输入字段均可在源码声明清单中对齐；type surface 数量不是 P06 闭包。

`Get-CpgMemberUses(statLife)` 在所选 4 个 shard（Player、MessageBuffer、NetMessage、Projectile）返回 31 项；其中可见直接写和读写，但也含 `AccessMode=Unknown` / `EvidenceStatus=partial`。此查询不覆盖未选源码文件。`Find-CpgCallSites(Player.Hurt)` 在所选 3 个 shard（Player、MessageBuffer、Projectile）找到 5 个精确静态调用点，状态 `complete` 仅限该选择范围。`Get-CpgCallableFacts(Player.Hurt)` 为 `partial`，并报告 `CalleeEffectsNotExpanded`。因此下文将调用闭包和全部读写归属标成 `partial` 或 `unknown`，没有用零命中推断不存在关系。

源码事实锚点：

| 行为 | 当前 Version4 观察 | 状态 |
|---|---|---|
| Player tick | `Player.Update` 中 `ResetEffects` (`Player.cs:15207`) → `UpdateBuffs` (`:15239`) → `UpdateEquips` (`:15296`) → `UpdateLifeRegen` (`:15599`) → `UpdateManaRegen` (`:15601`) | 此方法体内相对顺序 `confirmed`；不代表 ECS scheduler 或跨 System 全局顺序 |
| 派生效果重置 | `ResetEffects` (`Player.cs:10272`) 重置 `statDefense` (`:10326`) 与 `lifeRegen` (`:10332`) 等派生值；同方法也推进 rapid attack、shimmer 等状态 | 局部写入 `confirmed`；效果重算的全部写者 `partial` |
| 受伤入口 | `Player.Hurt` (`Player.cs:22208`) 包含 shimmer dodge、免疫/防御判断、资源和 Buff 分支、生命提交、immune timer、击退/表现；Paladin shield 分支可递归 Hurt；致死路径关联 KillMe | 方法内源码事实 `confirmed`；callee effects `partial` |
| 状态效果入口 | `AddBuff` (`Player.cs:3541`)、`DelBuff` (`:3697`)、`UpdateBuffs` (`:4300`) 处理免疫、槽位、覆盖/替换和 tick；`AddBuff` 可继续调用删除/时间更新辅助方法 | 局部行为 `confirmed`；所有 buff ID 定义和外部效果闭包 `partial` |
| 生命回复 | `UpdateLifeRegen` (`Player.cs:10847`) 算出生命回复/伤害计数并调用 `HurtLifeRegen` (`:11238`)；当前 `HurtLifeRegen(int dmg){}` 是空体 | 调用与计数 `confirmed`；最终生命提交语义 `unknown` |
| 魔力回复 | `UpdateManaRegen` (`Player.cs:11239`) 更新 delay、regen、count，并在满足阈值时直接增加并钳制 `statMana` | 局部行为 `confirmed`；跨入口写者 `partial` |
| shimmer dodge | `AllowShimmerDodge` (`Player.cs:22568`) 当前为返回默认 `bool` 的空 stub | 该分支结果 `unknown`；不得按方法名补出行为 |
| DPS 记账 | `addDPS` (`Player.cs:26245`) 使用 `DateTime.Now` 初始化/更新时间和 damage；`ResetEffects` 另有清除条件 | 本地行为 `confirmed`；时钟替代和对外读者 `partial` |
| 网络状态 | `NetMessage.cs:513-514` 编码生命/max；`:957-958` 编码魔力/max；`MessageBuffer.cs:778-784` 写回生命/max/dead，`:1792-1795` 写回魔力/max | packet 局部事实 `confirmed`；authority、权限校验和完整协议闭包 `partial` |
| 存档 | `Player.SavePlayer` 委派到空的 `InternalSavePlayerFile` / `Serialize`；`Deserialize` 当前也为空体 | P06 成员的持久化集合及兼容规则 `unknown` |

## Prior Component Decomposition Reconciliation

P06 的 component ledger 和 review-round-2 设计/执行材料是候选拆分与历史计划，不是 System owner 证据。ledger 自述其只证明成员库存，并明确读者/写者、生命周期、网络、持久化和调度未闭合。旧执行文档记录若干 C01-C22 checkpoint 与历史 focused verifier；本任务没有运行或重验它们，也不把这些记录升级为当前迁移证据。

当前 `src/NSSLC` 已有 `Component/Combat/DamageResolutionSystem`、`DeathResolutionSystem`、`DamageEligibilityQuery`，以及玩家侧 `PlayerDamageEligibilityQuery`、`PlayerCombatProcSystem`、`PlayerDodgeCommitSystem`、`PlayerBarrierSystem`、`PlayerDpsTelemetrySystem`、`PlayerAccessoryEffectRebuildSystem`、`ManaStatusSystem` 和多个 component。可见通用 `DamageResolutionSystem.Resolve` 在拒绝时不写状态，在接受时执行简单 `max(1, amount-defense)` / critical 乘算并写 Health、cooldown、contribution；`DeathResolutionSystem` 从 Health 推导 death。该契约没有证明覆盖 Version4 的闪避、status/Buff、资源触发、递归 Paladin、完整减伤、动画/击退与网络效果。对 `src/NSSLC` 的静态引用检索只见通用 resolver 内部调用及若干 player System 的 verifier 调用，未找到这些通用 resolver 接入 P06 gameplay 的生产调用链；运行时注册/反射入口仍 `unknown`。

当前同域状态也有多个候选表示，例如 `PlayerVitalComponent`、`PlayerVitalState`、`PlayerVitalStateComponent` 与 Combat 下的 `HealthComponent`，以及 `PlayerBuffComponent`、`PlayerBuffSlotsComponent`、`StatusEffectsComponent`。其创建、映射、唯一 writer 和与旧 Player API 的适配关系没有由类型声明闭合，须在后续 integration review 选择一个 authority。

## Conceptual Behaviors

以下按稳定概念分组，不按 22 个叶子组各建一个 System。所有 System/API 名称都是本报告 proposed 候选。

| 行为 | 不变量与副作用 | Proposed 协作边界 |
|---|---|---|
| 装备/效果派生与 reset | 每 tick 先清派生值，再按既有输入重建；一次性标记、持久计时和派生能力不可互相覆盖；更新次序会影响受伤和回复读取 | `PlayerCombatCapabilityRebuildSystem` 协调只读装备/环境/状态快照并提交 P06 派生能力；物品和环境输入走 integration handoff |
| 玩家受击裁决与提交 | 拒绝、闪避、免疫、有效受伤和致死必须保留条件与先后；只有被接受的 hit 才能触发生命、免疫计时、proc、Buff、击退、音效/粒子等对应旧效果 | 一个 `PlayerCombatResolutionSystem` 保持同步协调/唯一 P06 hit commit；纯资格/数值 Query、显式 effect adapter 与已提交结果事件 |
| Buff/debuff 与状态 tick | 同类 buff 更新、免疫拒绝、槽替换、计时到期和状态派生效果顺序可观察；生命/魔力效果不得绕过资源提交边界 | 独立 `PlayerStatusEffectSystem` 管玩家 buff slot/timer 命令和 tick；definition/catalog 与外部状态共享 API `crossSubsystemOwner: integration-review` |
| 生命/魔力与 regen | resource current/max 有界；life regen 计算的最终生命伤害/恢复目前源为空体；mana regen 有 delay/count、输入修正和 cap | `PlayerVitalState` 作为单一资源快照候选；mana regen 可独立 System，life regen 仅提议计算阶段，提交效果 `partial/unknown` |
| 免疫/屏障/命中 proc | dodge 是否消耗、immune time、命中计数器与 proc 只能在旧条件成立时推进；帧动画不得成为免疫 authority | 受击协调器调用 immunity/dodge/barrier/proc API；表现数据单向 Projection；NPC/projectile 共用键与效果 owner 待 integration review |
| DPS 观测 | damage 累加、首次命中和最后命中时间保持一致；旧实现用 wall-clock `DateTime.Now`，不可静默换成 tick time | 独立 `PlayerDpsTelemetrySystem` 接收 committed damage，时钟/公开读模型为 Adapter/Projection；时间语义保持 `partial` |

## State Ownership and Write Closure

下表逐项覆盖本 claim 的 22 个叶子组和全部 254 个成员。组名/成员来自输入 ledger；proposed owner 是设计候选，不是现存权威归属。对每个跨 P 分区类型、ID、快照、事件、网络、存档及共享接口，最终 owner 均为 `crossSubsystemOwner: integration-review`。

| claim 叶子组 | 数量 | 完整成员清单 | Proposed owner / 状态 |
|---|---:|---|---|
| `PlayerStringAndAccessoryEffectState` | 14 | `extraAccessorySlots`, `extraAccessory`, `tankPet`, `tankPetReset`, `stringColor`, `counterWeight`, `vanityCounterWeight`, `magicString`, `yoyoString`, `yoyoGlove`, `rapidAttackBonus`, `stressBall`, `stressBallPrevious`, `staffOfRegrowthBonus` | 派生装备能力候选；Item/projectile/tank-pet 的输入、对象引用、重算唯一时序 `crossSubsystemOwner: integration-review`；`partial` |
| `PlayerCombatDamageProcState` | 15 | `lifeSteal`, `ghostDmg`, `eocDash`, `eocHit`, `infernoCounter`, `starCloakCooldown`, `onHitDodge`, `onHitRegen`, `onHitPetal`, `onHitTitaniumStorm`, `titaniumStormCooldown`, `hasTitaniumStormBuff`, `petalTimer`, `boneGloveTimer`, `phantomPhoneixCounter` | 受击提交后的 Proc System 候选；committed-hit 源、物品目标、Buff/投射物副作用 `crossSubsystemOwner: integration-review`；`partial` |
| `PlayerCombatDodgeAndImmunityState` | 5 | `blackBelt`, `brainOfConfusionItem`, `brainOfConfusionDodgeAnimationCounter`, `shadowDodge`, `shadowDodgeTimer` | 资格/消费交受击协调器；Item 引用与动画投影 handoff；`partial` |
| `PlayerCombatBarrierAndRegenState` | 4 | `iceBarrier`, `iceBarrierFrame`, `iceBarrierFrameCounter`, `palladiumRegen` | 屏障 capability 归免疫/受击边界候选；frame 是 presentation projection，palladium 输入来自装备；`partial` |
| `PlayerManaAndAfkStatus` | 10 | `manaSickTime`, `manaSickLessDmg`, `manaSickReduction`, `manaSick`, `afkCounter`, `AFKTimeNeededForNoWormSpawns`, `AFKTimeNeededForNoLuckyStars`, `afkCounterForKiting`, `manaRegenBonus`, `manaRegenDelayBonus` | mana sickness/regen modifier 由资源与状态 System 协作；AFK→NPC/spawn/lucky-star 消费者跨域，`crossSubsystemOwner: integration-review`；`partial` |
| `PlayerDebuffAndRecoveryStatus` | 16 | `chilled`, `dazed`, `frozen`, `stoned`, `ichor`, `webbed`, `tipsy`, `noBuilding`, `miscCounter`, `sandStorm`, `crimsonRegen`, `ghostHeal`, `ghostHurt`, `sticky`, `slippy`, `slippy2` | Status System 管 timer/slot 输入候选；Movement、Environment、Building、life regen/ghost damage 为跨域效果；`crossSubsystemOwner: integration-review`；`partial` |
| `PlayerDetectionAndCombatStatus` | 11 | `dangerSense`, `luckPotion`, `endurance`, `whipRangeMultiplier`, `whipUseTimeMultiplier`, `loveStruck`, `stinky`, `resistCold`, `electrified`, `dryadWard`, `panic` | Combat/status capability 候选；感知表现、环境抗性、whip/item 和 world effect 分别 handoff；`crossSubsystemOwner: integration-review`；`partial` |
| `PlayerSocialAndDefenseState` | 20 | `skinVariant`, `voiceVariant`, `voicePitchOffset`, `ghost`, `ghostFrame`, `ghostFrameCounter`, `_framesLeftEligibleForDeadmansChestDeathAchievement`, `pvpDeath`, `boneArmor`, `frostArmor`, `honey`, `crystalLeaf`, `crystalLeafCooldown`, `portableStoolInfo`, `preventAllItemPickups`, `dontHurtCritters`, `hasLucyTheAxe`, `dontHurtNature`, `defendedByPaladin`, `hasPaladinShield` | 混合表现、P04 death/achievement、P09 item permission、P08 honey/armor、P12 NPC/Paladin 协作；不可由单一 P06 owner 声称闭合；`crossSubsystemOwner: integration-review` |
| `PlayerFrameAndImmunityState` | 11 | `townNPCs`, `bodyFrameCounter`, `legFrameCounter`, `immune`, `immuneNoBlink`, `immuneTime`, `immuneAlphaDirection`, `immuneAlpha`, `_timeSinceLastImmuneGet`, `_immuneStrikes`, `maxRegenDelay` | immunity authority 候选与 frame/alpha projection 分开；town NPC relation handoff；写入者与 counter 触发闭包 `partial` |
| `PlayerVitalAndRegenState` | 13 | `statLifeMax`, `statLifeMax2`, `statLife`, `statMana`, `statManaMax`, `statManaMax2`, `lifeRegen`, `lifeRegenCount`, `lifeRegenTime`, `manaRegen`, `manaRegenCount`, `manaRegenDelay`, `manaRegenBuff` | 唯一 Vital owner + regen calculation candidate；`HurtLifeRegen` 空体使 life 提交效果 `unknown`；网络/持久化 `crossSubsystemOwner: integration-review` |
| `PlayerCombatModifierAndImmunityState` | 16 | `armorPenetration`, `meleeArmorPenetration`, `statDefense`, `noKnockback`, `shimmerImmune`, `spaceGun`, `gravDir`, `chaosState`, `strongBees`, `sporeSac`, `shinyStone`, `empressBrooch`, `volatileGelatin`, `volatileGelatinCounter`, `hasMagiluminescence`, `shadowArmor` | 拆分 combat modifiers、P07 gravity、shimmer/environment、P09 equipment 能力；字段消费与唯一重建者需跨域裁决；`crossSubsystemOwner: integration-review` |
| `PlayerAmmoAndAccessoryEffects` | 12 | `chloroAmmoCost80`, `huntressAmmoCost90`, `ammoCost80`, `ammoCost75`, `stickyBreak`, `magicQuiver`, `magmaStone`, `lavaRose`, `hasMoltenQuiver`, `phantasmTime`, `ammoBox`, `ammoPotion` | Item/projectile capability handoff，不由伤害 resolver 复制 item state；`crossSubsystemOwner: integration-review` |
| `PlayerElementalAndShimmerStatus` | 21 | `archery`, `poisoned`, `venom`, `blind`, `blackout`, `headcovered`, `frostBurn`, `onFrostBurn`, `onFrostBurn2`, `burned`, `shimmering`, `timeShimmering`, `shimmerTransparency`, `shimmerUnstuckHelper`, `suffocating`, `dripping`, `drippingSlime`, `drippingSparkleSlime`, `onFire`, `onFire2`, `onFire3` | Buff timer、World/Environment、Movement 与 Presentation 拆分；`AllowShimmerDodge` stub 的语义不推定；`crossSubsystemOwner: integration-review` |
| `PlayerSurvivalAndTransformationState` | 15 | `noItems`, `hungry`, `starving`, `heartyMeal`, `windPushed`, `wereWolf`, `wolfAcc`, `hideMerman`, `hideWolf`, `forceMerman`, `forceWerewolf`, `sunScorchCounter`, `accMerman`, `merman`, `trident` | 生存/变形派生候选；buff、装备、环境和外观 consumer owner 未闭合；`crossSubsystemOwner: integration-review`；`partial` |
| `PlayerDebuffStatusState` | 8 | `cursed`, `bleed`, `confused`, `brokenArmor`, `silence`, `slow`, `gross`, `tongued` | Status System 输入候选；共享 Buff definitions/effects/免疫注册 `crossSubsystemOwner: integration-review` |
| `PlayerAccessoryCombatModifierState` | 9 | `kbGlove`, `autoReuseGlove`, `meleeScaleGlove`, `kbBuff`, `remoteVisionForDrone`, `starCloakItem`, `starCloakItem_manaCloakOverrideItem`, `starCloakItem_starVeilOverrideItem`, `starCloakItem_beeCloakOverrideItem` | P09 Item relation 和 drone presentation 输入 handoff；`Terraria.Item` 运行时对象不可复制进 combat authority；`crossSubsystemOwner: integration-review` |
| `PlayerAccessoryResourceAndInvulnerabilityState` | 5 | `longInvince`, `pStone`, `PhilosopherStoneDurationMultiplier`, `manaFlower`, `moonLeech` | resource/damage eligibility capability 候选；静态 multiplier 属定义而非 player mutable component；Item consumer handoff；`partial` |
| `PlayerAccessoryDebuffAndDropState` | 8 | `vortexDebuff`, `trapDebuffSource`, `witheredArmor`, `witheredWeapon`, `slowOgreSpit`, `parryDamageBuff`, `ballistaPanic`, `JustDroppedAnItem` | status/damage input 与 item drop event 拆分；P09、P12 和 status definition 交接；`crossSubsystemOwner: integration-review` |
| `PlayerCombatDamageAndCritModifiers` | 14 | `meleeCrit`, `magicCrit`, `rangedCrit`, `meleeDamage`, `magicDamage`, `rangedDamage`, `rangedMultDamage`, `arrowDamageAdditiveStack`, `arrowDamage`, `bulletDamage`, `rocketDamage`, `minionDamage`, `minionKB`, `revolverCritChanceBonus` | 派生 capability snapshot 候选；读取端包含 Item、Projectile、NPC/weapon domain；`crossSubsystemOwner: integration-review` |
| `PlayerCombatSpeedRangeAndPermissionState` | 9 | `IsAllowedToHoldItems`, `meleeSpeed`, `summonerWeaponSpeedBonus`, `moveSpeed`, `pickSpeed`, `wallSpeed`, `tileSpeed`, `autoPaint`, `autoActuator` | 字段属于 Item、Movement、WorldInteraction 与 combat 的混合投影；不得由 Combat System 独占全部；`crossSubsystemOwner: integration-review` |
| `PlayerLuckAndCommerceEffects` | 13 | `discountEquipped`, `discountAvailable`, `hasLuckyCoin`, `boneGloveItem`, `goldRing`, `accDivingHelm`, `accFlipper`, `deadCellsPotionStation`, `hasLuck_LuckyCoin`, `hasLuck_LuckyHorseshoe`, `hasLuck_LuckyClover`, `hasLuck_WiltedClover`, `hasLuck_RavenFeather` | Luck capability 与 commerce/item effects 分开；商店/掉落/物品读取者 `crossSubsystemOwner: integration-review` |
| `PlayerDpsTelemetryState` | 5 | `dpsStart`, `dpsEnd`, `dpsLastHit`, `dpsDamage`, `dpsStarted` | 独立 telemetry owner/projection candidate；提交伤害事件、wall clock source、读者和 reset 入口 `partial` |
| **合计** | **254** | **22 个 claim 叶子组；254 个字段；0 个属性** | **仅为 proposed 分类；无字段被声明为已迁移** |

### 写入闭包与唯一提交点

当前 Version4 的 `Hurt` 同一同步入口协调免疫/闪避、减伤、生命、资源/状态、免疫计时、击退和表现；Paladin shield 可再入本地玩家 `Hurt`。 proposed 设计保留一个 per-player `PlayerCombatResolutionSystem` 作为接受伤害的协调边界，避免多个 System 各自提交生命或在拒绝后继续触发 proc。Buff、regen、modifier rebuild 和 telemetry 是协作 System，不能另设未同步的生命 writer。

这只是候选写入协议：`HurtLifeRegen`、`AllowShimmerDodge` 为空体，Buff/Item/Projectile/Net/serialization 的全部间接写者没有闭合，`crossSubsystemOwner: integration-review`。本报告不宣布已存在唯一 writer。

## Boundary Decision

| 决策 | 边界 | 理由 | 状态 |
|---|---|---|---|
| `keep` | 单个玩家 hit coordination/commit System | Version4 `Hurt` 将资格、减伤、资源、生命、免疫和外部效果按分支同步编排；拆成多个独立生命 writer 会破坏拒绝/递归/致死顺序 | `proposed`；callee/effect closure `partial` |
| `separate` | 玩家 buff/debuff slot 与 tick System | `AddBuff`、`DelBuff`、`UpdateBuffs` 有独立槽位/免疫/替换/timer 生命周期，不是 damage resolver 的纯计算细节 | `proposed`；definitions 与共享状态 `crossSubsystemOwner: integration-review` |
| `separate` | capability rebuild 与只读 combat Query | reset/装备/状态导出派生能力；纯 Query 不应读写 Component、Item、RNG 或网络 | `proposed`；输入/调度/owner `partial` |
| `partial` | Life/Mana regeneration | Mana 更新有明确本地算法；life regen 的 `HurtLifeRegen` 是空实现，不能重建最终生命变化 | mana 子边界 `proposed`；life commit `unknown` |
| `separate` | DPS telemetry | 计数/墙钟指标可由 committed damage 投影，不应与 Vital authority 共用写者；旧 `DateTime.Now` 语义需保留 | `proposed`；clock/readers/reset `partial` |
| `reject` | 每个输入叶子组单独建立一个 System，或一个囊括 PlayerGameplay 的 catch-all | 叶子表是库存切分而非调度边界；大 System 会吞入 Item、Movement、Environment、NPC、World 和 Presentation ownership | 不符合状态 owner / 副作用边界 |

## System API and Legacy Behavior Mapping

名称仅为提案，不表示 `src/NSSLC` 已有等价 API。API 以稳定 value snapshot、Command 和显式 Adapter 为边界；不传 `Terraria.Player`、可变 Item/Buff/NPC/Projectile 对象或 Query 的 out 参数。

| 旧入口 | Proposed API 组合 | 必须保留的观察/状态变化 | 状态 |
|---|---|---|---|
| `Player.Hurt(reason, damage, direction, pvp, quiet, crit, cooldownCounter, dodgeable)` (`Player.cs:22208`) | `PlayerDamageEligibilityQuery` → `PlayerDamageMitigationQuery` → 单一 `PlayerCombatResolutionSystem.ResolveHit(command)` → result/event → Network/Presentation/Item adapters | dodge/immune 的早退，防御算法/舍入、资源和 Buff 触发时点、生命与 immune timer、击退/反馈、Paladin 再入、拒绝不提交 hit effects | 源本地行为 `confirmed`；新 API `proposed`；准确减伤/间接 effects `partial` |
| `Player.KillMe(...)` 从受击致死分支进入 | Vital result → `PlayerLethalDamageCommitted` handoff → P04 lifecycle/death System | lethal threshold、creative/practice/dead guard、死亡副作用顺序交 P04；不得由 P06 复制第二套 death authority | `crossSubsystemOwner: integration-review`；P04 结论 `unknown` |
| `AddBuff` / `DelBuff` / `UpdateBuffs` | `PlayerStatusEffectSystem.Add/Remove/Tick` + Buff definition Query + packet Adapter | buff immunity、fed-state replacement、时间刷新、槽淘汰和每 tick decrement 顺序 | 本地事实 `confirmed`；新组合 `proposed`；完整 definition/effects `partial` |
| `ResetEffects` / `UpdateEquips` | `PlayerCombatCapabilityRebuildSystem.BeginTick/Rebuild(snapshot)` + immutable capability snapshot | reset-before-rebuild，保留 `Player.Update` 原顺序；区分 derived reset 与跨 tick counter | reset 顺序 `confirmed`；全域调度/API `partial` |
| `UpdateLifeRegen` / `HurtLifeRegen` | `PlayerLifeRegenQuery` → proposed `LifeRegenResult` → commit handoff | 保留各 debuff、equipment、时间 accumulator 与 count 的先后；最终扣/加生命不得从空方法体猜测 | computation `partial`；commit `unknown` |
| `UpdateManaRegen` | `PlayerManaRegenSystem.Tick(snapshot)` → Vital command/result | delay 变化、静止/抓钩/buff 输入、regen formula、count 阈值、魔力 cap 和返回读者 | 本地算法 `confirmed`；新组合 `proposed`；全局写者 `partial` |
| `Player.addDPS(dmg)` | `PlayerDpsTelemetrySystem.RecordCommittedDamage(event, ClockPort)` → telemetry snapshot Projection | 首次 hit 与累积 hit 的各时间字段，damage 累加、`dpsStarted` 清理条件；不能把 `DateTime.Now` 改 tick 而不作行为决策 | 旧本地语义 `confirmed`；clock/API `partial` |
| `NetMessage` life/mana encode 与 `MessageBuffer` decode | Protocol Adapter ↔ authoritative resource Command / immutable replication Projection | 字段宽度、读取顺序、life<=0 时 dead 派生、max life 限制、发送/接收 authority | packet 局部 `confirmed`；身份/权限/序列化闭包 `crossSubsystemOwner: integration-review` |
| `SavePlayer` / `Serialize` / `Deserialize` | Persistence Adapter + versioned snapshot codec | 保存字段、缺失值、版本兼容、失败恢复和 transient 排除 | 源方法为空体；具体映射 `unknown` |

Query 必须无写入与隐藏副作用；Command 只表达状态变化意图；System 是唯一有权提交其字段的边界；Network/Persistence/Presentation 只读已提交快照。旧 API 兼容 Adapter 的错误映射和事务/补偿语义未定义，仍为 `unknown`。

## Call and Dependency DAG

### Source-observed edges

```text
Main player tick
  -> Player.Update
     -> ResetEffects
     -> UpdateBuffs
     -> UpdateEquips
     -> UpdateLifeRegen
        -> HurtLifeRegen (empty body; final resource effect unknown)
     -> UpdateManaRegen
```

该顺序由 `Player.Update` 方法体确认，不是构建器注册顺序，也不证明迁移调度。CPG 精确 `Player.Hurt` 调用边（选中 shard）有 5 项，跨 `Player.cs`、`MessageBuffer.cs`、`Projectile.cs`；手工源码还确认 `MessageBuffer.cs:2956`、`Projectile.cs:13261` 的入口。`Hurt` 的 callable facts 为 `partial`，不从这 5 条外推完整入口闭包。

### Proposed dependency direction

```text
input/network commands + equipment/status/environment snapshots
  -> PlayerCombatCapabilityRebuildSystem
  -> immutable PlayerCombatSnapshot
       -> PlayerDamageEligibilityQuery
       -> PlayerDamageMitigationQuery
       -> PlayerCombatResolutionSystem (single accepted-hit commit)
            -> Vital / immunity / accepted proc changes
            -> committed result event
                 -> P04 lethal lifecycle handoff
                 -> status/item/projectile/network/presentation adapters

PlayerStatusEffectSystem tick -> status snapshot -> regen Queries
PlayerManaRegenSystem -> Vital command/commit
PlayerLifeRegenQuery -> result -> unknown commit until source gap resolved
committed damage event -> PlayerDpsTelemetrySystem -> telemetry Projection
```

箭头为 proposed dependency，不是现存 call graph。必须先确定 modifier/status snapshot 的同帧可见时间，再配置 `must-before`/`must-after` barrier；玩家 Tick 全局入口、事件订阅、并行阶段和 scheduler 注册均 `unknown`。跨 P12 NPC、Projectile、Network、Presentation 和 persistence 的事件所有权一律 `crossSubsystemOwner: integration-review`。

## Lifecycle and Side Effects

- **Create/reset:** 初始默认值来自 Version4 字段声明和 Player 构造/重置路径；需分清初始 sentinel、每 tick derived reset、lifecycle reset 与 buff 状态保留。迁移后的默认值映射没有实现证据，`partial`。
- **Per tick:** 当前源码可确认 reset → buff → equips → life regen → mana regen 的局部顺序。任一 tick 改序可能改变装备能力、debuff、计数器、当前资源和当帧 hit 查询结果。
- **Accepted/rejected hit:** 接受前的 dodge、免疫、减伤、mana/life effect、Hit commit、计时、proc、击退、音效/粒子和网络更新交错。拒绝时不应误触发 accepted-hit effects；确切副作用集合需按 branch 对照，`partial`。
- **Death/respawn:** P06 只提供 combat/vital 输入；`dead`、respawn/reset 和 lethal lifecycle 归 P04 integration review。Paladin shield 对本地玩家的嵌套 Hurt 需要重入/重复命令验证，当前并发/去重语义 `unknown`。
- **Buff lifecycle:** Add/replace/remove/immune/tick-expire 与 Debuff 状态影响 regen、防御、外观和其他玩家行为；definition registry、内容卸载和 slot 重建需要 StatusEffects/Content owner 定案。
- **Network:** life/mana 有独立消息字段和读写路径；server/client authority、竞态 packet、断线/重连恢复和版本兼容未闭合。
- **Persistence:** Player serializer/deserializer 为空 stub；不据此宣称任何 P06 成员可保存/可恢复。异常、损坏档案和旧版本字段行为为 `unknown`。
- **Clock/random/effects:** `DateTime.Now`、随机 proc/效果、音效/粒子与 Network 是非纯输入/副作用；需 port/adapter，异常后的部分提交及重试/补偿仍 `unknown`。
- **SS14 reference:** 只观察到 DamageableSystem API 与实现的拆分组织及状态效果 API 的明确暴露边界；不复制其 scheduler、net entity、DamageSpecifier 或 status semantics。

## Integration Handoff

以下一律 `crossSubsystemOwner: integration-review`，P06 不做最终裁决：

| 共享边界 | 交接决策 |
|---|---|
| P04 Player lifecycle / death / respawn | Vital depletion 到 death 的单一 handoff、递归受击中 lethal 的去重、reset 与死亡状态原子性；不能由 P06 再拥有 dead/respawn |
| P07 Mobility / P08 Environment and armor | `gravDir`、shimmer/temperature/suffocation、wet/slippy 类输入与免疫/防御修正的写者和阶段 |
| P09 Inventory/Equipment / Content | 装备能力、ammo/Item 对象引用、药剂、luck/commerce、Buff 定义 ID/替换规则；只传 typed ID/value snapshot |
| P10 Input / P11 Presentation | dodge intent、免疫帧/alpha、ghost/body frame、DPS read projection 的命令和只读快照方向 |
| P12 NPC / Projectile Combat | 玩家与 NPC/Projectile 共享 DamageRequest、attribution、cooldown key、damage result、Paladin target 与 committed hit owner |
| Network / Persistence | authority、packet encode/decode、Player/Entity ID 映射、字段版本、存档恢复/失败事务 |
| World/StatusEffects/Clock/Random | AFK spawn effect、buff catalog/注册与卸载、墙钟/游戏时钟差别、seeded random 和外部 effect ports |

任何 ID 类型不等同 runner `sessionId`；该会话 ID 仅用于本分区 Complete 结算。

## Migration Behavior Contract

本阶段只定义后续 verifier 的观察向量，不执行验证。

| 场景 | 比较向量 | 关键不变量 / 尚缺证据 |
|---|---|---|
| 受击被闪避、免疫或资格拒绝 | accepted/rejected、life/mana delta、免疫窗口、cooldown、proc/Buff/击退/网络副作用 | 同一输入/随机 seed 时被拒绝不得产生 accepted-hit side effects；`AllowShimmerDodge` 为空体，因此 shimmer dodge oracle `unknown` |
| 普通/暴击/防御伤害 | final damage、整数舍入、防御状态、资源触发顺序、生命、hit timer | 旧 `Hurt` 与通用 DamageResolution 算法需逐分支比对；不默认其公式等价 |
| lethal 与 Paladin shield | outer/recursive target、damage 次数、扣血顺序、KillMe 次数、effects/events | 重入、拒绝和 lethal 同帧的提交/去重为 `unknown`，需同一 deterministic source fixture |
| Buff 添加/覆盖/免疫/槽满/过期 | 槽内容、剩余 tick、被移除 slot、status 派生值、packet/effects 顺序 | 内容 definition、替换规则和卸载状态未闭合 |
| tick reset + equipment + regen | reset 后 modifier、Buff snapshot、life/mana regen/count/delay、当前资源 cap | 必须保持源码观察到的阶段顺序；Life commit oracle 被空 `HurtLifeRegen` 阻塞 |
| 死亡、重生、跨 world / reload | resource defaults、immune/proc/Buff reset、P04 lifecycle state、restore result | 存档序列化/反序列化为空体；不能定义成功恢复预期前置行为 |
| DPS telemetry | started/start/end/lastHit/damage 与旧时间源 | `DateTime.Now` 精度和时区/时钟来源语义需明确；墙钟不可直接替换成 tick counter |

### Proposed invariants

1. 每个 P06 authority 字段只有一个 committed writer；projection、adapter 和 Query 不写回。
2. 被拒绝/闪避的受击不会意外推进 accepted-hit proc、生命提交或通知。
3. 对同一输入、状态和随机/时间来源，旧入口与新组合的状态 delta、副作用顺序和错误结果一致。
4. `statLife` / `statMana` 始终按旧路径 cap；精确约束以源码分支为准，不凭组件默认值推断。
5. 迁移期间旧 API 只适配到单一 System commit，不允许双写或以双写掩盖差异。

上述为未来验收契约，不是已测结果或迁移成功声明。

## Evidence Gaps and Blocking Decisions

1. **完整成员读写闭包 `partial`:** CPG 只查了指定 shard；部分 `AccessMode=Unknown`，callee effects、alias、reflection/hook、mod entry 和全局所有 writer 未闭合。
2. **版本绑定 `unknown`:** CPG `SourceSnapshotId=null`，CPG manifest 没有确认与当前 Version4 工作目录逐文件同步；关键行为已回当前原源码复查，不能把索引当唯一事实。
3. **life regen commit `unknown`:** `HurtLifeRegen(int)` 为空体；不能声称生命 DoT/heal 已恢复或自行发明提交规则。
4. **shimmer dodge `unknown`:** `AllowShimmerDodge` 为空体；闪避条件/副作用没有可执行源码 oracle。
5. **持久化 `unknown`:** `InternalSavePlayerFile`、`Serialize`、`Deserialize` 为空体；254 字段保存集合、transient 规则、兼容迁移和异常恢复未知。
6. **DPS clock `partial`:** `DateTime.Now` 是 wall clock；是否可注入并保持观察语义，以及 reset/consumer 完整闭包未证。
7. **运行时注册与调度 `unknown`:** 没有本阶段找到 P06 System scheduler、query access declaration、event binding 和跨 System barrier 证据；方法顺序仅限 Player.Update 内。
8. **owner 重复与迁移入口 `partial`:** 当前 NLTX 有多种 Vital/Health/Buff component 形状和 player combat Systems；未看到完整旧 API adapter/生产调用闭环，不能选定实际 writer。
9. **跨分区事务 `crossSubsystemOwner: integration-review`:** P04 lifecycle、P07/P08 movement/environment、P09 items、P11 presentation、P12 NPC、P15 projectile、Network 与 Persistence 的提交/失败协议需共同确定。
10. **实现阶段阻断项:** 在 3、4、5 的未知未解决前，不应把 regen、shimmer dodge、load/save 的 proposed API 描述为行为完整；P12/P15 shared damage 的 key/source identity 和重入策略须在 integration review 决议。

## Verification Plan

`verificationStatus: not-run`。本会话仅执行只读源码检查、只读 CPG 查询和文档写入；没有运行 build、test、run、行为 verifier 或迁移执行，也没有修改生产源码、测试、项目文件、输入 ledger、prompt 或其他分区报告。仅新增本 claim 授权的 `outputReport`。后续 verifier 应依 `Migration Behavior Contract` 对旧/新状态 delta、拒绝与接受效果、tick 顺序、嵌套 Hurt、buff slot/timer、network 与 save/load 做对照；serializer 和空 stub 语义须先从权威源码/版本补齐，不能以本报告代替。
