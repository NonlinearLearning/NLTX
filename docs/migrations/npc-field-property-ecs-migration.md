# NPC.cs 字段与属性 ECS 迁移清单

**审查日期：** 2026-08-28  
**来源报告：** [`docs/research/2026-08-28-field-property-migration-comparison.md`](../research/2026-08-28-field-property-migration-comparison.md)  
**只读来源：** `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`  
**目标边界：** `src/Terraria.Dome.Simulation/Npc`、共享 Combat/Movement 能力，以及 Server/Protocol 投影层

## 1. 结论

`NPC.cs` 不是一个可以整体搬入 Simulation 的实体类。它把实例状态、自然生成上下文、
AI 临时变量、Buff、Boss/入侵全局状态、网络节流、绘制缓存和兼容属性集中在同一个
God Object 中。本清单把这些成员拆成 `Definition -> Component -> System -> Snapshot ->
Persistence/Protocol` 五段，并保留没有行为证据的成员为 `partial`、`deferred` 或
`compatibility`。

- 报告字段基线：360 个字段声明；`netID` 是唯一排除的 NPC 身份字段，字段统计分母为 **359**。
- 本文补充属性审计：源文件另有 25 个属性/属性块；`WhoAmIToTargetingIndex` 是身份寻址属性，
  不进入迁移评分，其余 **24 个非身份属性**单独列入属性矩阵。
- 因此，本文不把“属性块漏计”伪装成报告的 30.9% 数字变化；**30.9% 仍是跨 11 个旧文件的
  报告口径**，NPC 本身的 30% 是保守的文件级估算，不是逐成员完成率。
- `ID`/`Id` 只用于统计排除，不删除身份契约。`type`、`target`、`releaseOwner`、`realLife`
  等普通业务字段仍保留，因为它们不是“身份字段”本身。

状态含义：

| 状态 | 含义 |
| --- | --- |
| `verified` | 当前 owner、生命周期和至少一个运行时/验证证据已确认。 |
| `partial` | 已有类型化承载或窄行为切片，但不是旧语义的全量等价。 |
| `deferred` | 已确认责任边界，尚未进入当前实现批次。 |
| `compatibility` | 只能留在兼容/协议/表现适配层，不能进入 Simulation 权威状态。 |
| `excluded` | 明确属于世界级资源、客户端表现或旧 API，不作为 NPC 实例字段迁移。 |

## 2. 当前已确认的目标 owner

| 旧字段族 | 当前 owner | 证据/边界 |
| --- | --- | --- |
| `active`、`timeLeft`、失活原因 | `NpcLifecycleComponent`、`NpcLifecycleSystem`、`NpcStateSnapshot` | killed/timed-out/segment tombstone 和 revision 有窄验证；完整 `checkDead` 分支仍 partial。 |
| `type`、默认生命/防御、阵营/类别 | `NpcDefinition`、`NpcDefinitionComponent`、`NpcDefinitionRegistry` | 定义注册和输入校验已验证；完整 `NPCID.Sets` 默认表未迁移。 |
| `target`、目标有效性 | `NpcTargetComponent`、`NpcTargetSelectionSystem` | 最近有效玩家、无效目标清除和稳定 tie-break 已验证；NPC-target 全量语义未完成。 |
| `aiStyle`、`ai[]`、`localAI[]` | `NpcBehaviorStateComponent`、`NpcBehaviorRegistry`、行为族 System | 不提供 `float[] Ai` 公共状态；普通追逐 verified，FlyingEye/ranged/segment partial。 |
| `homeTileX/Y`、`homeless`、`lookForHomeTimeout` | `NpcHomeComponent`、`NpcHomePublicationComponent`、`NpcHomeSystem`、`NpcHomeTimeoutSystem`；`NpcHomeSnapshot` 仅承载既有 home wire 子集 | home/无家、返回超时、零值可找房边界和 last-published baseline 已有窄验证；住房、商店、对话 deferred。 |
| `realLife`、段索引/父子关系 | `NpcSegmentComponent`、`NpcSegmentFollowSystem`、`NpcSegmentLifecycleSystem` | 根/父/子约束和顺序有验证；完整 worm movement/shared-life partial。 |
| `netUpdate`、`netSpam`、section | `NpcReplicationComponent`、`NpcReplicationSnapshot`、Protocol projector | 复制快照、revision、PVS 投影存在；完整旧同步节流和所有消息分支 partial。 |
| Buff/DoT 标志 | `BuffCollectionComponent`、StatusEffects systems | 仅有共享 typed 状态能力；NPC 全部旧 Buff 标志仍 deferred。 |
| 死亡掉落 | `NpcDeathEvent`、`NpcLootCommand`、`NpcLootSystem` | 普通确定性掉落和一次性发布 verified；完整 `NPCLoot`/Boss loot deferred。 |

## 3. 字段迁移矩阵（359 个非 ID 字段）

下表按来源声明区域分组；组内成员名是完整扫描结果的压缩表示，未列出的字段不存在于
该组之外。组状态是最保守成员状态：只要组内有一个未完成语义，就不能写成 `verified`。

| 来源区域 | 成员（按 `NPC.cs` 原名） | 目标 owner | 状态 | 迁移动作/缺口 |
| --- | --- | --- | --- | --- |
| `Spawner` 嵌套上下文（61） | `spawnSpaceX`, `spawnSpaceY`, `fairyLog`, `numberOfActivePlayers`, `reachedInvasionBossCap`, `pX`, `pY`, `luck`, `dayTime`, `raining`, `townNPCs`, `skyMob`, `noWorms`, `noGroundWorms`, `invaders`, `spawnFriendly`, `ignoreSafeWalls`, `waterTile`, `nearGranite`, `nearMarble`, `spawnSpider`, `surfaceSpawn`, `spawnUndergroundDesert`, `hardDungeon`, `deeperThanRockLayer`, `underGround`, `isOcean`, `isBeach`, `isSpawningInWindDirection`, `skyBehindPlayer`, `livingTree`, `dualDungeonsSpawnRules`, `inDualDungeon`, `tresspassingDualDungeon`, `inRemixStartingArea`, `offensiveToTim`, `playerHasStartingHealth`, `ZoneCorrupt`, `ZoneCrimson`, `ZoneHallow`, `ZoneJungle`, `ZoneSnow`, `ZoneGlowshroom`, `ZoneMeteor`, `ZoneGraveyard`, `ZoneDungeon`, `ZoneLihzhardTemple`, `ZoneGranite`, `ZoneMarble`, `ZoneSandstorm`, `ZoneTowerSolar`, `ZoneTowerVortex`, `ZoneTowerNebula`, `ZoneTowerStardust`, `ZoneOldOneArmy`, `ZoneWaterCandle`, `ZonePeaceCandle`, `ZoneShadowCandle`, `defaultTarget` | `NpcSpawnSnapshot`, `NpcSpawnEligibilitySystem`, world/biome queries | `partial` | 生成资格已有命令/提交边界；完整 biome、事件、秘密种子和刷怪表不能塞进 NPC 实体。 |
| 生命周期 | `active`, `CanBeReplacedByOtherNPCs`, `homelessDespawn`, `dontCountMe`, `despawnEncouraged`, `timeLeft` | `NpcLifecycleComponent`, `NpcEscapeSystem`, `NpcLifecycleSystem`, `NpcActivityRangePolicy`, `NpcActivitySlotContributionPolicy`, `NpcCheckActiveKeepAlivePolicy`, `NpcCheckActiveTimerRefreshPolicy`, `NpcCheckActiveDeactivationPolicy`（pixel-space、nearby-slot、active-player keep-alive、screen-range timer-refresh 与 timeout/deactivation query） | `partial` | active/timeLeft/死亡 revision、pixel-range geometry、nearby-slot contribution、active-player keep-alive、screen-range timer-refresh 和 timeout/deactivation query 已有窄证据；完整替换、距离、per-player counter、timer/deactivation integration 和旧 `CheckActive` 分支仍 deferred。 |
| 定义/战斗核心 | `waterMovementSpeed`, `lavaMovementSpeed`, `honeyMovementSpeed`, `shimmerMovementSpeed`, `teleportStyle`, `nameOver`, `SpawnedFromStatue`, `altTexture`, `townNpcVariationIndex`, `rarity`, `takenDamageMultiplier`, `npcSlots`, `shimmerTransparency`, `damage`, `defense`, `defDamage`, `defDefense`, `defLifeMax`, `coldDamage`, `trapImmune`, `life`, `lifeMax`, `difficulty`, `statsAreScaledForThisManyPlayers`, `friendly`, `boss`, `chaseable`, `dontTakeDamage`, `dontTakeDamageFromHostiles`, `knockBackResist`, `value`, `extraValue`, `townNPC`, `lavaImmune`, `reflectsProjectiles` | `NpcDefinition`, `NpcDefinitionComponent`, `NpcBehaviorStateComponent`, `HealthComponent`, `DefenseComponent`, `NpcAuthorityComponent`, `LegacyNpcTownRegistry`, `LegacyNpcCheckActiveKeepAliveRegistry`, authority/loot definitions | `partial` | 定义、生命、防御、阵营、类别和难度已有 owner；`townNPC` 的 39 个有效静态 source net-id（来自 32 个分支）现在由 `LegacyNpcTownRegistry` 驱动生命周期 inactivity gate；active-player `CheckActive` 的 17 个 static keep-alive type 由 `LegacyNpcCheckActiveKeepAliveRegistry` 独立持有；`chaseable`/`dontTakeDamage` 已有行为状态 owner 和纯资格查询，`trapImmune` 与 `lavaImmune` 已有显式 authority gate 及窄 consumer，`reflectsProjectiles` 已有动态行为状态 owner 和 projectile 纯资格查询；完整默认值、`CheckActive` 玩家/AI/事件分支、projectile/target 与 lava contact 的完整副作用、Boss 规则、掉落价值和其他免疫策略未闭合。 |
| 行为/移动 | `teleportTime`, `aiAction`, `aiStyle`, `ai`, `localAI`, `justHit`, `directionY`, `oldDirectionY`, `oldTarget`, `rotation`, `noGravity`, `noTileCollide`, `collideX`, `collideY`, `spriteDirection`, `behindTiles`, `stepSpeed`, `gfxOffY`, `teleporting`, `stairFall`, `oldPos`, `oldRot`, `setFrameSize` | `NpcBehaviorStateComponent`, `NpcMovementIntentSystem`, shared movement/collision | `partial` | 类型化行为状态禁止回退为 `float[]`；普通 chase verified，FlyingEye、158 个 `AI_###`、碰撞特例和历史轨迹 deferred。 |
| 目标/交互 | `target`, `targetRect`, `playerInteraction`, `lastInteraction`, `releaseOwner` | `NpcTargetComponent`、`NpcInteractionComponent`、`NpcSpawnStateComponent` | `partial` | 玩家目标选择、释放者输入边界和会话交互状态 apply/expiry 已有窄证据；NPC-target、256 槽交互数组和完整对话上下文 deferred。 |
| 城镇/住房 | `homeless`, `lookForHomeTimeout`, `homeTileX`, `homeTileY`, `housingCategory`, `oldHomeless`, `oldHomeTileX`, `oldHomeTileY`, `closeDoor`, `doorX`, `doorY`, `friendlyRegen`, `breath`, `breathCounter`, `nextDialogue` | `NpcHomeComponent`, `NpcHomePublicationComponent`, `NpcHomeSystem`, `NpcHomeTimeoutSystem`, Town/Interaction systems | `partial` | home/无家、返回超时、零值可找房边界和旧 home 发布基线有窄验证；`NpcHomeSnapshot` 不携带内部 timeout 或发布游标；门、住房评分、呼吸、水下城镇行为、完整对话和服务 deferred。 |
| 复制/分段 | `skippedSyncs`, `streamCounter`, `netUpdate`, `netUpdatePendingSpamCooldown`, `netUpdatePendingFullSpamCooldown`, `netSpamPacketLimit`, `netSpamTicksPerPacket`, `netSpamTicksPerPacketForBosses`, `netSpam`, `netAlways`, `spawnNeedsSyncing`, `netStream`, `playerNetSyncState`, `netOffset`, `realLife`, `catchItem` | `NpcReplicationComponent`, `NpcReplicationSnapshot`, `NpcSegmentComponent`, Protocol projector | `partial` | snapshot/revision/PVS 和段关系已建模；`skippedSyncs`/`streamCounter` 是旧 per-player 同步 scratch。旧槽位、节流 scratch、捕捉释放和完整字节 parity deferred。 |
| 状态效果 | `buffType`, `buffTime`, `buffImmune`, `canDisplayBuffs`, `midas`, `ichor`, `brokenArmor`, `onFire`, `onFire2`, `onFire3`, `onFrostBurn`, `onFrostBurn2`, `poisoned`, `venom`, `tipsy`, `bleeding`, `hemorrhage`, `markedByScytheWhip`, `markedByEelWhip`, `shadowFlame`, `soulDrain`, `shimmering`, `lifeRegen`, `lifeRegenCount`, `lifeRegenExpectedLossPerSecond`, `confused`, `loveStruck`, `stinky`, `dryadWard`, `immortal`, `canGhostHeal`, `javelined`, `tentacleSpiked`, `bloodButchered`, `celled`, `dryadBane`, `daybreak`, `betsysCurse`, `oiled`, `electricEelCounter`, `catchableNPCTempImmunityCounter`, `immune`, `soundDelay` | `BuffCollectionComponent`, `HitImmunityComponent`, StatusEffects/Combat systems | `deferred` | 共享 typed Buff/免疫能力存在，但 NPC 全量 DoT、特殊标记、音效和捕捉免疫尚无 parity 证据。 |
| 世界级 Boss/事件/生成资源 | `MoonLordAttacksArray`, `MoonLordAttacksArray2`, `MoonLordFightingDistance`, `MoonLordCountdown`, `MaxMoonLordCountdown`, `NaturalMoonlordCountdownTime`, `ItemMoonlordCountdownTime`, `goldCritterChance`, `totalInvasionPoints`, `waveKills`, `waveNumber`, `golemBoss`, `plantBoss`, `crimsonBoss`, `deerclopsBoss`, `mechQueen`, `brainOfGravity`, `empressRageMode`, `cavernMonsterType`, `downedBoss1`, `downedBoss2`, `downedBoss3`, `downedQueenBee`, `downedSlimeKing`, `downedGoblins`, `downedFrost`, `downedPirates`, `downedClown`, `downedPlantBoss`, `downedGolemBoss`, `downedMartians`, `downedFishron`, `downedHalloweenTree`, `downedHalloweenKing`, `downedChristmasIceQueen`, `downedChristmasTree`, `downedChristmasSantank`, `downedAncientCultist`, `downedMoonlord`, `downedTowerSolar`, `downedTowerVortex`, `downedTowerNebula`, `downedTowerStardust`, `downedEmpressOfLight`, `downedQueenSlime`, `downedDeerclops`, `ShieldStrengthTowerSolar`, `ShieldStrengthTowerVortex`, `ShieldStrengthTowerNebula`, `ShieldStrengthTowerStardust`, `LunarShieldPowerNormal`, `TowerActiveSolar`, `TowerActiveVortex`, `TowerActiveNebula`, `TowerActiveStardust`, `LunarApocalypseIsUp`, `downedMechBossAny`, `downedMechBoss1`, `downedMechBoss2`, `downedMechBoss3`, `npcsFoundForCheckActive`, `lazyNPCOwnedProjectileSearchArray`, `spawnSlotProtected`, `ShimmeredTownNPCs`, `savedTaxCollector`, `savedGoblin`, `savedWizard`, `savedMech`, `savedAngler`, `savedStylist`, `savedBartender`, `savedGolfer`, `boughtCat`, `boughtDog`, `boughtBunny`, `unlockedSlimeBlueSpawn`, `unlockedSlimeGreenSpawn`, `unlockedSlimeOldSpawn`, `unlockedSlimePurpleSpawn`, `unlockedSlimeRainbowSpawn`, `unlockedSlimeRedSpawn`, `unlockedSlimeYellowSpawn`, `unlockedSlimeCopperSpawn`, `unlockedMerchantSpawn`, `unlockedDemolitionistSpawn`, `unlockedPartyGirlSpawn`, `unlockedDyeTraderSpawn`, `unlockedTruffleSpawn`, `unlockedArmsDealerSpawn`, `unlockedNurseSpawn`, `unlockedPrincessSpawn`, `combatBookWasUsed`, `combatBookVolumeTwoWasUsed`, `peddlersSatchelWasUsed`, `taxCollector`, `freeCake`, `travelNPC`, `fireFlyFriendly`, `fireFlyChance`, `fireFlyMultiple`, `butterflyChance`, `stinkBugChance`, `gravity`, `safeRangeX`, `safeRangeY`, `activeRangeX`, `activeRangeY`, `noSpawnCycle`, `activeTime`, `defaultSpawnRate`, `defaultMaxSpawns`, `kingSlimePointCacheSize`, `kingSlimePointCacheSizeMax`, `kingSlimePointCache`, `EoCKilledToday`, `WoFKilledToday`, `ignorePlayerInteractions`, `ladyBugGoodLuckTime`, `ladyBugBadLuckTime`, `ladyBugRainTime`, `maximumAmountOfTimesLadyBugRainCanStack`, `offSetDelayTime` | `BossEncounterState`, `InvasionState`, `WorldProgressionState`, `NpcSpawnBudgetState`, registries | `excluded` / `deferred` | 这些是世界资源、静态表或调试 scratch，不属于 NPC 实例。应迁移到 WorldEvents/Boss/Spawn 资源；当前完整表和事件进度 deferred。 |
| `noSpawnCycle` world gate | `NpcSpawnCycleStateComponent` | `partial` | Legacy `NPC.CheckActive` sets the static marker on timeout and `SpawnNPC` consumes it exactly once; a separate `checkDead` write at `NPC.cs:64670` remains outside this slice, and the typed owner is not wired to lifecycle/spawn integration in the current batch. |
| 表现/兼容 | `IsABestiaryIconDummy`, `IsAPortraitDummy`, `ForcePartyHatOn`, `dripping`, `drippingSlime`, `drippingSparkleSlime`, `rarity`, `HitSound`, `DeathSound`, `color`, `alpha`, `hide`, `scale`, `altTexture`, `oldPos`, `oldRot`, `frameCounter`, `frame`, `setFrameSize`, `shimmerTransparency`, `_givenName`, `nextDialogue`, `netOffset`, `lastPortalColorIndex` | Client presentation adapter、Localization、Protocol compatibility | `compatibility` / `deferred` | 不得让 Simulation 读取纹理、音效、语言、绘制帧或 `Main`；`alpha`/`Opacity` 只有投影层转换。 |
| 常量/缓存/复仇 | `NPC_TARGETS_START`, `maxAI`, `nameOverIncrement`, `nameOverDistance`, `gravity`, `netSpamPacketLimit`, `netSpamTicksPerPacket`, `netSpamTicksPerPacketForBosses`, `maxBuffs`, `KickOutLookForHomeTimeout`, `breathMax`, `CommonMasterBossLifeReduction`, `SPAWN_SLOT_PROTECTION_TIME`, `RevengeManager`, `kingSlimePointCache`, `playerNetSyncState` | 各自领域的 `const`/readonly policy、Replication/World resource | `deferred` | 常量可在目标领域重建；缓存和 manager 不能作为 ECS 全局 God Object，必须改成显式资源或删除。 |

> 注：分组表可能在不同语义组中重复列出同一个来源名（例如 `oldPos`），因为它们在旧类中
> 同时参与移动和表现。后续 Roslyn 成员索引应为每个声明附加唯一 `MemberId`，不能仅按名称合并。

## 4. 属性迁移矩阵（排除身份属性）

这些属性不在研究报告的“属性块”字段计数中；这里单独审计它们的 getter/setter 语义。

| 属性 | 身份排除 | 当前目标/处理 | 状态 | 说明 |
| --- | --- | --- | --- | --- |
| `CanTalk` | 否 | `NpcInteractionSystem` + `NpcHome/Behavior` 查询 | `partial` | 依赖城镇类型、`aiStyle`、垂直速度和宠物表；完整对话/服务未完成。 |
| `CanBeTalkedTo` | 否 | `NpcInteractionSystem` | `partial` | 已有交互权威边界；旧 `Main.player`/NPC 表查找不进入 Simulation。 |
| `HasValidTarget` | 否 | `NpcTargetSelectionSystem` 的目标有效性查询 | `verified`（窄） | 有效玩家过滤和 stale target 清除已验证；NPC-target 分支仍 partial。 |
| `HasPlayerTarget` | 否 | `NpcTargetComponent.HasTarget` + server player query | `verified`（窄） | 不再把槽位整数当作唯一实体状态。 |
| `HasNPCTarget` | 否 | `NpcTargetRoutingSystem.HasNpcTarget` + `NpcTargetComponent` | `partial` | encoded 范围/翻译边界已验证；完整 NPC-target acquisition 仍未完成。 |
| `SupportsNPCTargets` | 否 | `NpcDefinition.SupportsNpcTargets` + `NpcTargetCapabilityRegistry` | `partial` | Version 1.4.5.6 表已注册；完整 acquisition/AI 行为仍未完成。 |
| `TranslatedTargetIndex` | 否 | `NpcTargetRoutingSystem.TryTranslateTargetIndex` + Protocol compatibility | `compatibility` | NPC/player family 和偏移边界已验证；仅为旧槽位编码服务。 |
| `IsShimmerVariant` | 否 | `NpcTownVariantSystem` + explicit transform capability | `partial` | variation index 纯条件已验证；Shimmer town 表和完整转换行为仍 deferred。 |
| `TypeName` | 否 | Localization/Protocol adapter | `compatibility` | `Lang.GetNPCNameValue` 不能成为 Simulation 依赖。 |
| `FullName` | 否 | Localization/interaction projection | `deferred` | 给定名和本地化标题未形成服务器行为契约。 |
| `HasGivenName` | 否 | `NpcGivenNameComponent` | `partial` | null 已归一化为空字符串并可确定性清除；`FullName` 的本地化标题仍 deferred。 |
| `GivenOrTypeName` | 否 | Localization projection | `deferred` | 同上，不复制旧 getter。 |
| `GivenName` | 否 | `NpcGivenNameComponent` | `partial` | setter 的 null -> empty 语义已迁移；磁盘持久化、网络文本和本地化投影仍 deferred。 |
| `sWidth`、`sHeight` | 否 | Server world/session bounds | `compatibility` | 固定客户端窗口尺寸，不是 NPC 状态。 |
| `DownedAnyPreHardmodeBoss` | 否 | `WorldProgressionState`/Boss resource | `excluded` | 全局进度不挂在 NPC 实体。 |
| `ShieldStrengthTowerMax` | 否 | `WorldEvents`/Lunar shield resource | `excluded` | 依赖 Moon Lord 全局状态。 |
| `Opacity` | 否 | Client rendering projector | `compatibility` | alpha 与渲染透明度的转换不能写入权威 NPC 状态。 |
| `TreatedAsABossForRainbowBoulders` | 否 | Definition capability + World interaction query | `deferred` | `NPCID.Sets` Boss 表和特殊交互未完成。 |
| `isLikeATownNPC` | 否 | `NpcDefinition.Category`/Town capability | `partial` | 特殊 type `453` 规则未迁移为完整定义。 |
| `IsMechQueenUp` | 否 | `BossEncounterState` | `excluded` | 旧 getter 会扫描并修正全局 NPC 槽位；目标是显式 Boss resource。 |
| `TooWindyForButterflies` | 否 | World weather query | `excluded` | 依赖 `Main.windSpeedTarget`，不属于 NPC。 |
| `CountsAsACritter` | 否 | `NpcDefinition` + combat classification query | `partial` | 需要完整类型表和 damage/life 默认值。 |
| `NetSectionCoordinates` | 否 | `NpcReplicationSnapshot.Section` / PVS | `verified`（窄） | 目标是快照 section，不调用旧 `Netplay`。 |
| `WhoAmIToTargetingIndex` | **是** | 不迁移；仅兼容编码说明 | `excluded` | `whoAmI + 300` 是 NPC 身份/槽位寻址属性。 |

## 5. 迁移批次与验收闸门

### Batch N0：成员索引冻结

1. 固定来源 SHA-256：`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`。
2. 用 Roslyn 生成 `npc-members.json`：记录声明 kind、可见性、静态性、默认值、来源行、
   唯一 `MemberId` 和身份排除原因。
3. 明确字段 359、属性 24 的统计边界；不把 `WhoAmIToTargetingIndex` 或 `netID` 混入分母。

### Batch N1：实例核心

- `active`/`timeLeft`/`life`/`lifeMax`/`damage`/`defense`/`friendly`/`boss` 绑定 Definition、
  Health、Defense、Lifecycle 和 replication snapshot。
- 旧 `type` 只作为定义输入；禁止在系统里读取旧 `NPC` 实例或固定数组槽位。
- 验收：spawn -> target -> damage -> death -> loot -> inactive revision 可确定性回放。

### Batch N2：行为与目标

- 将 `aiStyle`/`ai[]`/`localAI[]` 映射为有类型 `BehaviorId + state`；四槽 AI 只在协议投影层生成。
- 验收：普通 chase 必须通过；FloatingEye、ranged、segment 单独标记 partial，不能共享“AI 已完成”。

### Batch N3：城镇、分段、事件

- home/无家先保持最小骨架；段关系使用 `NpcSegmentComponent`；Boss/invasion 使用 World resource。
- 验收：home snapshot restore、segment parent/child order、事件 spawn authority 各有独立 verifier。

### Batch N4：状态效果和兼容清理

- Buff/DoT/免疫逐类建立 typed command、持久化和协议投影；表现属性移出 Simulation。
- 全部旧引用扫描为零后，才评估删除兼容适配；不以删除源码替代行为 parity。

### Batch N3.1：交互状态承载（已完成窄切片）

- `NpcInteractionComponent` 作为 NPC 实例的 typed owner，承载会话玩家、交互类型、目标位置、最后交互 tick 与活跃标志；不复制旧的 256 槽数组，也不把对话文本或服务库存放入 Simulation。
- `NpcInteractionSystem.Apply` 只接受已通过 `TryCreate` 的授权命令；`TryExpire` 使用显式 tick 和 timeout，保证会话失效是确定性的。
- `NpcSpawnCommitSystem` 为每个新 NPC 挂载空交互组件，避免运行时通过可选旧字段回退。
- 验收证据：`Npc.Composition.Verification` 构建通过（0 警告/0 错误），组合冒烟输出 PASS。
- 边界：完整 `CanTalk`/`CanBeTalkedTo` 对话服务、住房评分、商店库存和持久化文本仍为 `deferred`，因此总体迁移状态保持 `partial`。

### Batch N3.2：给定名存储语义（已完成窄切片）

- `NpcGivenNameComponent` 迁移旧 `GivenName` setter 的可证实语义：null 归一化为空字符串，
  `HasGivenName` 只表示非空存储值。
- 新 NPC 默认挂载空给定名组件；Simulation 不生成 `FullName` 或 `GivenOrTypeName`，避免引入 Localization 依赖。
- 验收证据：NPC composition verifier 覆盖 null、非空、清除三态；构建/冒烟保持通过。
- 边界：给定名磁盘持久化、网络文本、`Game.NPCTitle` 本地化拼接仍为 `deferred`。

### Batch N3.3：给定名状态快照（已完成窄切片）

- `NpcStateSnapshot` 现在携带 `GivenName`，创建快照时从 `NpcGivenNameComponent` 读取，恢复实体时重新挂载 typed component。
- 给定名只进入服务器权威状态快照；`NpcReplicationSnapshot` 和 SyncNPC 网络包不携带文本，避免把本地化/兼容字段泄漏到协议层。
- 验收证据：NPC protocol verifier 的 named-state round-trip 通过，快照恢复后 `GivenName == "Guide"`；构建 0 警告/0 错误。
- 边界：磁盘二进制格式、网络给定名消息、`FullName`/`GivenOrTypeName` 本地化拼接仍为 `deferred`。

持久化边界证据见 [`2026-08-28-npc-given-name-persistence-boundary.md`](../research/2026-08-28-npc-given-name-persistence-boundary.md)。
V36 格式当前没有 `NpcStateSnapshot` 段；在未完成独立版本化 name segment 设计前，不把 `GivenName`
塞入 `NpcReplicationSnapshot` 或 SyncNPC codec。

### Batch N3.4：兼容导入给定名（已完成窄切片）

- `CompatibilityToDomeProjection` 同时生成 `NpcReplicationSnapshot` 和 `NpcStateSnapshot`，把 legacy
  `CompatibilityNpcSnapshot.Name` 投影到 `GivenName`，避免从旧 `.wld` 导入时丢失名称。
- 该投影只影响服务器内存状态，不改变 `NpcReplicationSnapshot`、SyncNPC codec 或 V36 二进制布局。
- 验收证据：`Terraria.Dome.Persistence.Verification` 的 compatibility import 断言通过；构建 0 警告/0 错误，
  persistence round-trip、legacy default 和 NPC typed-state 检查均 PASS。
- 当前 V36 `WriteNpcs` 仍只写 replication snapshot，因此导入后再保存仍会丢失给定名；本批新增
  独立 `NpcGivenNamePersistenceFormat` sidecar（V1），但尚未接入主保存协调器。
- sidecar 已实现数量/名称长度/正数 ID/重复 ID/尾部数据校验；主文件集成、原子保存协调和恢复时
  与实体集合的 join 已由 `NpcGivenNameSaveCoordinator` 实现；主文件集成仍为 `deferred`。

### Batch N3.5：给定名 sidecar 恢复协调（已完成窄切片）

- `NpcGivenNameSaveCoordinator` 提供临时文件原子保存、损坏主文件的临时文件恢复，以及按
  authoritative `NpcStateSnapshot.Replication.ReplicationId` 合并名称。
- 未出现在 authoritative NPC 集合中的 sidecar 条目会被忽略；重复状态 ID 或非法条目会拒绝，
  不会创建幽灵 NPC。
- 验收证据：Persistence verifier 覆盖 sidecar round-trip、临时文件恢复和 authoritative-ID join，
  构建 0 警告/0 错误。
- 边界：`DomeStatePersistenceFormat` 主文件尚未调用该 coordinator；网络文本、Localization 和
  完整对话仍为 `deferred`。

### Batch N3.6：Server 生命周期接入（已完成窄切片）

- `DomeServer.ConfigureNpcGivenNamePersistence` 在启动前读取 sidecar，并通过 Simulation 的
  authoritative NPC replication ID 应用给定名。
- `DomeServer.Dispose` 从 `NpcStateSnapshot` 收集非空名称并使用 coordinator 原子写回；主 V36
  文件和 SyncNPC 协议保持不变。
- 当前 join 只作用于配置时已经存在的 NPC。动态生成 NPC 的名称继承需要独立 spawn-owner
  contract，不能按复用 ID 隐式猜测，故仍为 `deferred`。
- 验收证据：`Terraria.Dome.Server` Release 构建通过（0 警告/0 错误）；sidecar coordinator
  persistence verifier 通过。

### Batch N3.7：动态生成给定名输入契约（已完成窄切片）

- `SpawnNpcCommand` 增加可选 `GivenName` 输入；`NpcSpawnCommitSystem` 只接受长度不超过 200 的
  显式名称，并写入 `NpcGivenNameComponent`。缺省值保持空字符串，不从复用 replication ID 或
  其他 NPC 隐式继承。
- 验收证据：`Npc.Verification` 的显式 `Target Dummy` spawn 断言通过；`Npc.Composition.Verification`
  通过；两个构建均为 0 警告/0 错误。
- 该契约只定义 spawn 输入，不解决 legacy 动态名称来源、Localization 或主文件持久化；这些仍
  按来源证据分别处理。

### Batch N3.8：交互属性资格查询（已完成窄切片）

- `NpcInteractionSystem.CanTalk` / `CanBeTalkedTo` 提取 legacy getter 的纯条件：
  `isLikeATownNPC`、`aiStyle == 7`、`velocity.Y == 0`，以及 `CanTalk` 的显式 town-pet 排除。
- `isLikeATownNPC` 与 `isTownPet` 都是调用方提供的 typed/query 输入；Simulation 不读取 `Main`
  或 `NPCID.Sets` 静态表。
- 验收证据：composition verifier 覆盖有效、移动中、宠物、非城镇、错误 AI style 和 NaN 速度；
  构建/冒烟通过。
- 边界：type 453 等特殊 town 规则、完整 `CanTalk` 对话服务和宠物表仍为 `deferred`。

### Batch N3.9：定义级交互能力输入（已完成窄切片）

- `NpcDefinition` 增加显式 `IsLikeTownNpc` 与 `IsTownPet` 能力字段，默认值保持关闭，避免从
  faction/category 推断未核实的 legacy `NPCID.Sets` 语义。
- `NpcInteractionSystem` 增加以 `NpcDefinition` 为输入的 `CanTalk` / `CanBeTalkedTo` 重载；
  查询仍是纯函数，AI style、垂直速度和宠物排除通过定义能力进入，不读取 `Main` 或静态全局表。
- 已知 town-home fixture 明确标注 `AiStyle: 7` 与 `IsLikeTownNpc: true`；其他定义不隐式获得
  城镇可对话资格。
- verifier 同时固定 legacy 的差异：town pet 的 `CanTalk` 为 false，但 `CanBeTalkedTo` 仍可为 true；
  这两个查询不能被合并为同一个 capability。
- 验收证据：`Terraria.Dome.Npc.Composition.Verification` Release 构建与冒烟通过（0 警告/0 错误）；
  复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-191347/composition-n3.9-rerun2.log`。
- 边界：type 453 等特殊 town 规则、完整宠物表、Localization、对话/商店服务仍为 `deferred`，
  因此 NPC field/property migration 总状态保持 `partial`。

### Batch N3.10：NPC-target capability 输入（已完成窄切片）

- `NpcDefinition` 增加显式 `SupportsNpcTargets` capability，对应 legacy
  `SupportsNPCTargets => NPCID.Sets.UsesNewTargeting[type]` 的表查询结果。
- 默认值为 `false`；Version 1.4.5.6 类型表由 `NpcTargetCapabilityRegistry` 提供，其他版本或未登记
  类型仍 fail-closed。目标选择与 routing 继续使用实体句柄和稳定引用，兼容层才解释旧的
  `target >= 300` 编码。
- 验收证据：composition verifier 覆盖显式开启与默认关闭的 definition 输入；Release 构建和冒烟通过。
- 边界：完整 `NPCID.Sets.UsesNewTargeting` 表、NPC-target 选择/routing 行为和旧协议全部 parity
  仍为 `partial`/`deferred`。

### Batch N3.11：Rainbow-boulder Boss 分类输入（已完成窄切片）

- `NpcDefinition` 增加 `IsBoss` 与 `ShouldBeCountedAsBossForRainbowBoulders` 两个显式输入，并以
  `TreatedAsABossForRainbowBoulders` 计算 legacy getter 的短逻辑：Boss 实例直接成立，否则使用
  类型表结果。
- 不在 Simulation 读取 `NPCID.Sets`，也不把 Boss encounter 状态挂到 NPC 定义；表结果必须由定义
  注册阶段显式提供，默认关闭。
- 验收证据：composition verifier 覆盖 Boss、表 capability 和默认关闭三种情况；Release 构建和
  冒烟通过。复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-191850/composition-n3.11-rerun.log`。
- 边界：完整 `ShouldBeCountedAsBossForRainbowBoulders` 类型表、Boss encounter/world event 行为和
  其他 Boss 特殊交互仍为 `deferred`。

### Batch N3.12：Definition-owned NPC-target routing（已完成窄切片）

- `NpcTargetRoutingSystem` 增加以 `NpcDefinition` 为输入的 `TryResolve` 重载，路由能力直接读取
  `definition.SupportsNpcTargets`，不再要求调用方复制一个无来源的裸 bool。
- 旧 overload 保留为兼容入口；两者都继续执行 `300 <= encodedTarget < 300 + maximumNpcCount`、
  translated index、非默认实体和 active candidate 检查。
- 验收证据：`Npc.Verification` 覆盖 definition capability 开启/关闭与原有边界，Release 构建通过；
  复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-192218/npc-n3.12-rerun2.log`。
- 边界：Version 1.4.5.6 `UsesNewTargeting` 表已由 N3.17-N3.19 注册并绑定；NPC-target
  acquisition/AI、LOS 和完整协议 parity 仍为 `partial`/`deferred`。

### Batch N3.13：Encoded NPC-target 属性查询（已完成窄切片）

- `NpcTargetRoutingSystem.HasNpcTarget` 与 `TryTranslateNpcTargetIndex` 提取 legacy
  `HasNPCTarget` 和 `TranslatedTargetIndex` 的纯边界逻辑；范围计算使用 `long` 中间值，避免
  `300 + maximumNpcCount` 溢出。
- `TryResolve` 复用同一翻译入口，确保属性查询与实体路由不会出现不同的 encoded-target 规则。
- 验收证据：`Npc.Verification` 覆盖有效边界、玩家范围、上界、`int.MaxValue` 边界和翻译值；
  Release 构建通过。复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-192453/npc-n3.13-rerun.log`。
- 边界：NPC-target acquisition、AI-style 目标变更、LOS、静态类型表和完整协议仍为
  `partial`/`deferred`。

### Batch N3.14：Player-target 属性查询（已完成窄切片）

- `NpcTargetRoutingSystem.HasPlayerTarget` 提取 legacy `HasPlayerTarget` 的纯范围语义：
  `0 <= target < maximumPlayerCount`。
- 最大玩家数由调用方显式提供，Simulation 不读取 `Main.maxPlayers`；使用 `long` 中间值保持边界
  判断不受整数加法溢出影响。
- 验收证据：`Npc.Verification` 覆盖零值、最大合法值、上界、负值和极大整数输入；Release 构建通过。
  复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-192707/npc-n3.14.log`。
- 边界：玩家目标实体解析、旧 `Main.player` 槽位、目标获取/AI 和完整协议仍为 `partial`/`deferred`。

### Batch N3.15：统一 target index 翻译（已完成窄切片）

- `NpcTargetRoutingSystem.TryTranslateTargetIndex` 统一 legacy `TranslatedTargetIndex` 的兼容翻译：
  先判合法 NPC encoded target，再判合法 player target，并返回 target family；无效编码 fail-closed。
- NPC target 保持 `target - 300`，player target 保持原 index；兼容调用方不再自行复制偏移常量。
- 验收证据：`Npc.Verification` 覆盖 NPC/玩家翻译、family 标志、无效间隙值和原有边界；Release
  构建通过。复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-192853/npc-n3.15.log`。
- 边界：实体解析、NPC-target acquisition、AI/LOS 和完整网络消息仍为 `partial`/`deferred`；
  Version 1.4.5.6 `UsesNewTargeting` 表本身已由 N3.17/N3.18 覆盖。

### Batch N3.16：NPC target selection consumer 接入（已完成窄切片）

- `DomeSimulation.SelectNpcTargets` 现在构造 `NpcTargetCandidate` 并调用
  `NpcTargetSelectionSystem.SelectTarget`；active/living/finite candidate 过滤和 stable-id tie-break
  不再在 Simulation tick 中重复实现。
- typed `NpcTargetComponent` 结果随后显式投影到兼容 target component；两个组件的 enum 值通过显式
  转换对齐，Simulation 仍以实体句柄和稳定玩家 ID 为权威状态。
- 验收证据：`Npc.Verification` 的 runtime target/chase、无效候选过滤和死亡/掉落回归通过；Release
  构建日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-193535/npc-acquisition-runtime-rerun.log`。
- 边界：NPC-target acquisition、type-specific targeting、LOS、AI-style 目标变更、静态表和完整
  协议 parity 仍为 `partial`/`deferred`。

### Batch N3.17：UsesNewTargeting 类型表注册（已完成窄切片）

- 新增 `NpcTargetCapabilityRegistry.CreateVersion1456`，登记 legacy
  `NPCID.Sets.UsesNewTargeting` 的 1.4.5.6 source-backed 类型集合（33 个 ID）。
- 表查询通过 `Supports(definitionId)` 提供，不读取旧 `NPCID` 静态数组；实例是否启用仍由
  `NpcDefinition.SupportsNpcTargets` 显式冻结，避免把版本表隐式写入实体。
- 验收证据：`Npc.Verification` 覆盖已知表项、未登记普通类型和 type `453` 排除；Release 构建通过。
  复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-194035/npc-n3.17.log`。
- 边界：NPC-target acquisition、AI-style 目标变更、LOS、完整 routing consumer 和其他 `NPCID.Sets`
  表仍为 `partial`/`deferred`。

### Batch N3.18：Target capability 到 Definition registry 绑定（已完成窄切片）

- `NpcDefinitionRegistry` 接受版本化 capability registry；注册时按 DefinitionId 合并
  `SupportsNpcTargets`，通过不可变 `NpcDefinition.WithSupportsNpcTargets` 生成新的定义值。
- `DomeSimulation` 默认 definition registry 使用 `NpcTargetCapabilityRegistry.CreateVersion1456`，
  使后续 spawn/routing 读取已冻结的 definition capability，而不是运行期访问静态表。
- 验收证据：`Npc.Verification` 用 type `547` 验证表值从 registry 进入 definition；Release 构建和
  runtime verifier 通过。复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-194615/npc-n3.18.log`。
- 边界：mod/未知类型的完整注册策略、NPC-target acquisition、AI-style 目标变更、LOS 和完整协议
  仍为 `partial`/`deferred`。

### Batch N3.19：Target capability registry 完整性闸门（已完成窄切片）

- `NpcTargetCapabilityRegistry` 现在拒绝非正数和重复 DefinitionId，并暴露冻结集合的 `Count`；
  版本表不会再通过集合构造静默丢失异常数据。
- `CreateVersion1456` 的 33 个 source-backed ID 由 verifier 做数量、已知项、未知项和坏输入检查。
- 验收证据：`Npc.Verification` Release/runtime verifier 通过；日志：
  `Build/diagnostics/npc-complete/task-10-interaction/20260828-194809/npc-n3.19.log`。
- 边界：完整 NPC-target acquisition、type-specific AI、LOS、mod registry 合并策略和其他
  `NPCID.Sets` 表仍为 `partial`/`deferred`。

### Batch N3.20：Critter classification 纯查询（已完成窄切片）

- 新增 `NpcCombatClassificationSystem.CountsAsCritter`，提取 legacy `CountsAsACritter` 的纯条件：
  `maximumHealth <= 5`、`damage == 0`，并排除 DefinitionId `594` 与 `686`。
- 查询不读取 `Main`、静态数组或客户端状态；负生命值 fail-closed，其他完整 critter 行为仍不在本批。
- 验收证据：`Npc.Verification` 覆盖生命上界、伤害、两个排除类型和负值；Release/runtime verifier 通过。
  复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-195116/npc-n3.20.log`。
- 边界：完整 critter 类型表、捕捉/掉落/AI 行为和其他 combat classification 仍为 `partial`/`deferred`。

### Batch N3.21：HasValidTarget 路由查询（已完成窄切片）

- `NpcTargetRoutingSystem.HasValidTarget` 提取 legacy getter 的 player-first 顺序：先验证合法
  player target 的 entity/active/not-dead/not-ghost 状态，失败后仅在 `SupportsNpcTargets` 且 NPC
  encoded target 合法时回退 `TryResolve`。
- player 与 NPC candidate 都必须提供显式实体身份；Simulation 不读取 `Main.player`/`Main.npc` 槽位。
- 验收证据：`Npc.Verification` 覆盖有效 player、dead player、NPC fallback 和 capability 关闭；Release
  构建通过。复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-195355/npc-n3.21.log`。
- 边界：NPC-target acquisition、type-specific AI、LOS、完整协议和目标变更副作用仍为
  `partial`/`deferred`。

### Batch N3.22：Target capability 统一解析入口（已完成窄切片）

- `NpcTargetCapabilityRegistry.Supports(NpcDefinition)` 统一显式 Definition capability 与版本化
  `UsesNewTargeting` 表的合并规则；`NpcDefinitionRegistry` 改为复用该入口，避免 spawn/registry
  层复制 OR 逻辑。
- 显式 capability 可以覆盖未登记/扩展类型，版本表仍只负责已知 1.4.5.6 DefinitionId；结果在
  registry 构造阶段冻结到 Definition 值。
- 验收证据：`Npc.Verification` 覆盖表定义、显式开启定义和 registry 合并结果；Release/runtime
  verifier 通过。复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-195926/npc-n3.22.log`。
- 边界：NPC-target acquisition、type-specific routing/AI、LOS、mod 表版本协调和完整协议仍为
  `partial`/`deferred`。

### Batch N3.23：Shimmer variant 纯查询（已完成窄切片）

- 新增 `NpcTownVariantSystem.IsShimmerVariant`，提取 legacy `IsShimmerVariant` 的纯条件：
  `townNpcVariationIndex == 1` 且调用方提供的 `supportsShimmerTownTransform` 为 true。
- Simulation 不读取 `NPCID.Sets.ShimmerTownTransform`；表结果和 town transform 行为必须由上层
  Definition/兼容注册阶段显式提供。
- 验收证据：`Npc.Verification` 覆盖 variation index 1、其他 index、表 capability 关闭和负值；
  Release/runtime verifier 通过。复跑日志：`Build/diagnostics/npc-complete/task-10-interaction/20260828-200114/npc-n3.23.log`。
- 边界：1.4.5.6 的 29-type ShimmerTownTransform 表、NPC 转换、副作用和客户端表现仍为
  `partial`/`deferred`。

### Batch N3.24：Replacement lifecycle 输入承载（已完成窄切片）

- `SpawnNpcCommand` 增加可选 `CanBeReplaced` 输入，`NpcSpawnCommitSystem` 将其写入
  `NpcLifecycleComponent`；该状态随 `NpcStateSnapshot` 的 lifecycle 值自然保存/恢复。
- 这只迁移 legacy flag 的 typed owner 和 spawn 输入，不启用替换调度，也不从 NPC type 或槽位
  自动推断 replacement 资格。
- 验收证据：`Npc.Verification` 使用显式 `CanBeReplaced: true` 的 spawn 命令，确认 lifecycle
  component 保留该值；Release/runtime verifier 通过。复跑日志：
  `Build/diagnostics/npc-complete/task-10-interaction/20260828-200814/npc-n3.24.log`。
- 边界：replacement budget、AI 动态设置、槽位回收、距离/CheckActive 和完整 lifecycle parity
  仍为 `partial`/`deferred`。

### Batch N3.25：Chaseability 纯查询（已完成窄切片）

- 新增 `NpcCombatClassificationSystem.CanBeChasedBy`，提取 legacy `NPC.CanBeChasedBy` 的
  纯条件：active、chaseable、`lifeMax > 5`、非 friendly、非无敌，并支持显式的
  `ignoreDontTakeDamage` 与 immortal target-dummy 例外输入。
- 查询只接收 typed 状态和调用方例外，不读取 `Main`、`DebugOptions` 或固定 NPC 槽位；因此
  它是 target acquisition 的资格谓词，不等价于完整目标选择、LOS 或碰撞判断。
- 验收证据：`Npc.Verification` 覆盖正常目标、生命阈值、inactive、不可 chase、friendly、
  immortal 及两个例外分支；Simulation/verifier Release 构建与运行通过。日志：
  `Build/diagnostics/npc-complete/task-10-interaction/20260828-203000/npc-n3.25.log`。
- 边界：NPC-target acquisition、LOS/collision、type-specific AI、完整 DebugOptions 语义和
  其他 chaseability 副作用仍为 `partial`/`deferred`。

### Batch N3.26：动态 Chaseability typed owner（已完成窄切片）

- `NpcBehaviorStateComponent` 增加 `IsChaseable`，默认值为 `true`，承载 legacy `chaseable`
  的实例级、可被 AI 修改的状态；`CanBeChasedBy` verifier 通过该字段消费资格结果。
- 本批只建立行为状态 owner，不把动态标志错误地冻结进 `NpcDefinition`，也不扩展网络或磁盘
  格式；从 replication 重建时使用兼容默认值，完整 AI 动态修改、协议同步和持久化仍 deferred。
- 验收证据：`Npc.Verification` 覆盖 `IsChaseable=false` 的 fail-closed 路径；Simulation、NPC
  verifier 和 Server Release 重跑均为 0 warnings/0 errors。日志目录：
  `Build/diagnostics/npc-complete/task-10-interaction/20260828-210000/`。
- 边界：projectile/NPC-target eligibility consumer、LOS/collision、type-specific AI 及
  chaseability 的完整生命周期同步仍为 `partial`/`deferred`。

### Batch N3.27：动态伤害免疫 typed owner（已完成窄切片）

- `NpcBehaviorStateComponent` 增加 `DoesNotTakeDamage`，默认值为 `false`，承载 legacy
  `dontTakeDamage` 的实例级行为状态；`CanBeChasedBy` verifier 使用该字段确认默认拒绝，
  并保留显式 `ignoreDoesNotTakeDamage` 例外。
- 本批只建立行为状态 owner，不把动态免疫状态写入 `NpcDefinition`；网络/持久化格式和 AI
  修改调度未扩展，未知或未恢复状态继续采用兼容默认值。
- 验收证据：`Npc.Verification` 覆盖 typed `IsChaseable=false` 与
  `DoesNotTakeDamage=true` 的 fail-closed 路径；Simulation、NPC verifier、Server Release
  均通过。日志目录：`Build/diagnostics/npc-complete/task-10-interaction/20260828-211500/`。
- 边界：完整 projectile/NPC-target eligibility、AI 动态免疫副作用、LOS/collision、协议和
  持久化 parity 仍为 `partial`/`deferred`。

### Batch N3.28：Projectile NPC-target 资格消费（已完成窄切片）

- `ProjectileTargetEligibilitySystem.CanTargetNpc` 现在同时消费 projectile 的 friendly 资格、
  `NpcLifecycleComponent`、`NpcBehaviorStateComponent`、`NpcAuthorityComponent`、Definition
  faction 和 Health，统一调用 `NpcCombatClassificationSystem.CanBeChasedBy`。
- `ignoreDoesNotTakeDamage` 与 immortal target dummy 仍是调用方显式参数；普通 projectile
  不会隐式绕过 NPC 的动态免疫或无敌状态。
- 验收证据：`Terraria.Dome.Combat.Verification` 覆盖 friendly/hostile projectile、不可 chase
  NPC、伤害免疫例外；Combat verifier、Simulation 和 Server Release 均通过。日志目录：
  `Build/diagnostics/npc-complete/task-10-interaction/20260828-213000/`。
- 边界：DomeSimulation 全量 projectile 命中路径尚未切换到该 contract；NPC-target acquisition
  的距离/LOS/collision、type-specific AI、完整 immortal/debug 语义及协议/持久化仍为
  `partial`/`deferred`。

### Batch N3.29：玩家 Projectile 命中路径接入 NPC 资格（已完成窄切片）

- `DomeSimulation.DetectProjectileHits` 在玩家 projectile→NPC 的候选循环中调用
  `ProjectileTargetEligibilitySystem.CanTargetNpc`，不再只依赖 projectile friendly 标志；
  NPC 的 lifecycle、behavior、authority、faction 和 health 都从 ECS typed components 读取。
- training dummy 的 immortal 例外通过 definition id 显式开启，其他 immortal、town、不可
  chase 或动态伤害免疫 NPC 默认 fail-closed。
- 验收证据：Combat verifier 覆盖既存 projectile sweep/collision/loot 链路及 N3.28 eligibility；
  Combat verifier、Simulation 和 Server Release 均通过。日志目录：
  `Build/diagnostics/npc-complete/task-10-interaction/20260828-220000/`。
- 边界：NPC projectile→player 路径、完整 projectile AI、LOS/collision 特例、其他 legacy
  `CanBeChasedBy` 调用点、协议/持久化同步仍为 `partial`/`deferred`。

### Batch N3.30：NPC replication dirty intent（已完成窄切片）

- `NpcReplicationComponent` 增加 `MarkDirty`/`ClearDirty`，`DomeSimulation.UpdateNpcReplication`
  在 authoritative snapshot 发生变化并推进 revision 时显式标记 dirty，承载 legacy `netUpdate`
  的最小 typed update intent。
- 初始 spawn 的 dirty 状态保持为 true；本批不迁移 per-player `netSpam`、节流 cooldown、
  `netStream` 或 `spawnNeedsSyncing`，避免把传输 scratch 误当作实体状态。
- 验收证据：`Npc.Verification` 覆盖 dirty 状态清除后重新标记；Simulation、NPC verifier、Server
  Release 均通过。日志目录：
  `Build/diagnostics/npc-complete/task-10-interaction/20260828-223000/`。
- 边界：dirty 消费者、完整 PVS/节流策略、旧槽位协议 parity 和 per-player cursor 仍为
  `partial`/`deferred`。

### Batch N3.31：NPC target range 纯查询（已完成窄切片）

- `NpcTargetRoutingSystem.IsWithinTargetRange` 提取 legacy target acquisition 的有限距离边界：
  source/target 坐标与最大距离必须 finite，距离比较使用严格 `<`，负距离 fail-closed。
- 最大距离由调用方显式提供，Simulation 不硬编码或读取未迁移的 `NPCID.Sets.DangerDetectRange`
  表；该查询只负责距离门槛，不包含 `CanBeChasedBy`、LOS、碰撞或 type-specific routing。
- 验收证据：`Npc.Verification` 覆盖 3-4-5 边界、严格相等、NaN 坐标和负距离；NPC verifier 与
  Server Release 均通过。日志目录：
  `Build/diagnostics/npc-complete/task-10-interaction/20260828-230000/`。
- 边界：完整 danger-range 表、NPC-target acquisition、LOS/collision、AI-style 变更和其他
  target side effects 仍为 `partial`/`deferred`。

### Batch N3.32：NPC target selection 距离溢出拒绝（已完成窄切片）

- `NpcTargetSelectionSystem.SelectTarget` 在计算候选平方距离后拒绝非 finite 结果，避免有限但
  极端坐标的浮点溢出伪装成最近目标；正常距离和稳定 ID tie-break 语义保持不变。
- 验收证据：`Npc.Verification` 增加 `float.MaxValue` 坐标溢出场景并确认选择可表示的候选；NPC
  verifier、Simulation 和 Server Release 均为 0 warnings/0 errors。
- 边界：这只收紧目标选择器的数值边界，不实现 `CanBeChasedBy`、danger-range 表、LOS/collision、
  type-specific routing 或完整 target side effects，相关范围仍为 `partial`/`deferred`。

### Batch N3.33：TargetClosest 优先级纯查询（已完成窄切片）

- `NpcTargetSelectionSystem.TryCalculateTargetPriority` 提取 legacy `TryTrackingTarget` 的可验证
  数值部分：曼哈顿距离减去玩家 `aggro`，并在 NPC 有朝向且目标启用 `npcTypeNoAggro` 时加入
  `1000f` 惩罚；所有坐标、差值和结果必须 finite。
- 验收证据：`Npc.Verification` 覆盖 aggro、no-aggro 惩罚和 NaN 拒绝；该查询尚未接入运行时选择，
  因为玩家 aggro、NPC 类型表和朝向副作用尚无完整 typed owner。
- 边界：完整 TargetClosest consumer、tank-pet、碰撞/LOS、类型表、朝向/netUpdate 及 AI 行为仍为
  `partial`/`deferred`。

### Batch N3.34：Player aggro typed owner 与运行时消费（已完成窄切片）

- 新增 `PlayerTargetingStateComponent` 承载实例级 `Aggro`，Player 创建路径通过 `World.Add`
  挂载该组件；`DomeSimulation.SelectNpcTargets` 为每个有效玩家计算显式 target priority，
  `NpcTargetSelectionSystem` 消费该 priority 决定目标。
- `NpcTargetCandidate.TargetPriority` 保留兼容默认值；未提供 priority 的调用仍使用有限平方距离，
  避免破坏既有调用方。NPC 类型 `npcTypeNoAggro` 仍未硬编码，当前运行时只传入显式 `false`。
- 验收证据：`Npc.Verification` 覆盖 priority 改变目标选择；`Build/diagnostics/npc-complete/task-10-interaction/20260829-003500/`
  中 NPC verifier、Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：玩家 aggro 的装备/buff 动态写入、完整 `npcTypeNoAggro` 类型/实例表、tank-pet、碰撞/LOS、
  朝向/netUpdate 和其他 TargetClosest 副作用仍为 `partial`/`deferred`。

### Batch N3.35：Player npcTypeNoAggro typed 集合与运行时查询（已完成窄切片）

- `PlayerTargetingStateComponent` 增加可替换的 `FrozenSet<int>` 类型集合和
  `IsNoAggroNpc` 查询；`DomeSimulation.SelectNpcTargets` 按 NPC `DefinitionId` 和 NPC 当前
  facing 将结果传给 priority 查询，no-aggro 惩罚不再是隐含常量分支。
- 集合更新入口 `SetNoAggroNpcTypes` 只接受调用方显式提供的类型 ID；本批不伪造 legacy
  `Player.UpdateBiomes`/装备/Buff 计算，也不把客户端表直接引入 Simulation。
- 验收证据：`Npc.Verification` 覆盖类型集合命中/未命中；`Build/diagnostics/npc-complete/task-10-interaction/20260829-010500/`
  中 NPC verifier、Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：完整 1.4.5.6 no-aggro 默认表、动态装备/Buff 更新、tank-pet、碰撞/LOS、朝向/netUpdate
  及其他 TargetClosest 副作用仍为 `partial`/`deferred`。

### Batch N3.36：Item-3090 no-aggro capability registry（已完成窄切片）

- 新增 `NpcNoAggroCapabilityRegistry.CreateVersion1456Item3090`，登记 legacy `Player.cs`
  中物品 `3090` 写入 `npcTypeNoAggro` 的 23 个 DefinitionId，并提供只读查询。
- 该 registry 只描述 source-backed capability 集合；它不会自动写入所有 Player，也不会绕过
  每 tick 清空/重算语义。上层装备/Buff 系统仍需显式把结果传给 `SetNoAggroNpcTypes`。
- 验收证据：`Npc.Verification` 覆盖集合数量、首尾类型和未登记类型；`Build/diagnostics/npc-complete/task-10-interaction/20260829-014500/`
  中 NPC verifier、Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：完整装备/Buff 动态重算、其他 no-aggro 来源、tank-pet、碰撞/LOS、朝向/netUpdate 和
  TargetClosest 其他副作用仍为 `partial`/`deferred`。

### Batch N3.37：No-aggro 动态刷新契约（已完成窄切片）

- 新增 `PlayerNpcTargetingSystem.RefreshNoAggroCapabilities`，由调用方显式提供 item-3090 效果
  是否生效，并原子刷新 `PlayerTargetingStateComponent.NoAggroNpcTypes`；效果关闭时清空旧集合，
  不保留 stale no-aggro 状态。
- 该系统不自行扫描装备或 Buff，也不在 tick 中猜测来源；它只连接已验证的 capability registry
  与 Player typed owner，保留 legacy 每 tick 清空/重算的责任边界。
- 验收证据：`Npc.Verification` 覆盖开启应用和关闭清除；`Build/diagnostics/npc-complete/task-10-interaction/20260829-023000/`
  中 NPC verifier、Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：完整装备/Buff 触发调度、其他 no-aggro 来源、动态 aggro 计算、tank-pet、碰撞/LOS、
  朝向/netUpdate 和完整 TargetClosest 副作用仍为 `partial`/`deferred`。

### Batch N3.38：装备 item-3090 触发 no-aggro 刷新（已完成窄切片）

- `DomeSimulation.RecalculateEquipmentStats` 在装备统计重算后调用
  `PlayerNpcTargetingSystem.RefreshFromEquipment`；只有非 vanity 且 source slot 有效的 item `3090`
  才会激活 23-type no-aggro capability，移除或替换后集合会被清空。
- 该接入复用现有 `EquipmentStateCollectionComponent`/`InventoryComponent` owner，不读取旧 Player
  数组，也不把 vanity 装备误认为权威战斗效果；Buff 来源仍未接入。
- 验收证据：`Npc.Verification` 覆盖装备加入和移除后的应用/清除；`Build/diagnostics/npc-complete/task-10-interaction/20260829-040000/`
  中 NPC verifier、Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：完整装备槽位替换语义、Buff 动态来源、aggro 计算、其他 no-aggro 来源、tank-pet、碰撞/LOS、
  朝向/netUpdate 和完整 TargetClosest 副作用仍为 `partial`/`deferred`。

### Batch N3.39：动态 aggro 结果提交契约（已完成窄切片）

- `PlayerNpcTargetingSystem.SetAggro` 提供显式 typed 写入入口，供装备、Buff、隐身或其他上层
  resolver 提交已经计算出的 Player `Aggro` 结果；NPC targeting 继续只消费 `PlayerTargetingStateComponent`。
- 当前 `BuffCollectionComponent` 仅有通用 type/duration 语义，没有 source-backed stealth/aggro
  效果映射，因此本批不按未知 Buff type 猜测数值，也未伪造自动调度。
- 验收证据：`Npc.Verification` 覆盖显式 aggro 写入；后续批次再由具体 Buff/装备 resolver 接入。
- 边界：aggro 计算、Buff effect registry、动态 tick 调度、其他 no-aggro 来源、tank-pet、碰撞/LOS、
  朝向/netUpdate 和完整 TargetClosest 副作用仍为 `partial`/`deferred`。

### Batch N3.40：Stealth aggro 纯 resolver（已完成窄切片）

- `PlayerNpcTargetingSystem.CalculateAggro` 提取 legacy stealth 相关的可验证数值规则：隐身时
  aggro 下压到 `-750`，shroomite stealth 按 `(1-stealth)*750`、vortex stealth active 按
  `(1-stealth)*1200` 减少，并对非 finite 输入和整数边界 fail-closed/饱和处理。
- 该 resolver 只接受上层已确认的 `invisible`/stealth 状态，不从未知 Buff type 推导效果，也不
  自动改变 Player 状态；结果仍通过 `SetAggro` 显式提交。
- 验收证据：`Npc.Verification` 覆盖 shroomite/vortex reduction；`Build/diagnostics/npc-complete/task-10-interaction/20260829-061500/`
  中 NPC verifier、Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：stealth 状态 owner、Buff effect registry、tick 调度、装备/隐身来源合并、tank-pet、
  碰撞/LOS、朝向/netUpdate 和完整 TargetClosest 副作用仍为 `partial`/`deferred`。

### Batch N3.41：Player stealth typed owner 与 aggro 刷新入口（已完成窄切片）

- 新增 `PlayerStealthStateComponent`，承载 `IsInvisible`、`Stealth`、shroomite 和 vortex stealth
  状态；`Set` 对 stealth 做 `[0,1]` finite 校验。Player 创建路径默认挂载 `Stealth = 1`。
- `PlayerNpcTargetingSystem.RefreshAggro` 消费该 typed owner 和显式 base aggro，调用 N3.40 resolver
  生成并写回 `PlayerTargetingStateComponent.Aggro`；Buff/装备 resolver 仍由上层负责提供状态。
- 验收证据：`Npc.Verification` 覆盖 stealth state -> aggro refresh；`Build/diagnostics/npc-complete/task-10-interaction/20260829-073000/`
  中 NPC verifier、Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：stealth 状态动态 tick、Buff/装备来源合并、隐身触发调度、其他 aggro 来源、tank-pet、
  碰撞/LOS、朝向/netUpdate 和完整 TargetClosest 副作用仍为 `partial`/`deferred`。

### Batch N3.42：Stealth 状态显式 tick 转移（已完成窄切片）

- `PlayerNpcTargetingSystem.AdvanceStealth` 更新 `PlayerStealthStateComponent`：shroomite 静止时
  按 `0.015` 衰减、移动时按速度恢复、坐骑重置为 `1`；vortex active 按 `0.04` 衰减并在坐骑时
  关闭；`StealthTimer` 在使用物品时置为 `5` 并逐 tick 递减。
- 该状态机只消费调用方提供的使用动作、速度和坐骑状态，所有状态仍是 typed owner；没有自动推断
  装备/Buff 是否激活 stealth。
- 验收证据：`Npc.Verification` 覆盖静止 floor、移动恢复和坐骑重置；`Build/diagnostics/npc-complete/task-10-interaction/20260829-083000/`
  中 NPC verifier、Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：完整 stealth 装备/Buff 激活、velocity/mount source 接线、aggro 重算调度、其他 aggro 来源、
  tank-pet、碰撞/LOS、朝向/netUpdate 和完整 TargetClosest 副作用仍为 `partial`/`deferred`。

### Batch N3.43：Player tick 接入 stealth 状态推进（已完成窄切片）

- `DomeSimulation.Tick` 在玩家碰撞解析后调用 `AdvancePlayerStealth`，消费现有
  `VelocityComponent` 和 `ItemUseStateComponent`，驱动 `PlayerStealthStateComponent` 的显式状态转移。
- 当前没有 typed mount owner，因此调用方显式传入 `isMounted: false`；本批不把缺失的坐骑状态猜成
  可用能力，也不在每 tick 直接累减 `PlayerTargetingStateComponent.Aggro`。
- 验收证据：`Npc.Verification` 覆盖 item-use timer 状态；`Build/diagnostics/npc-complete/task-10-interaction/20260829-093000/`
  中 NPC verifier、Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：mount owner、stealth 装备/Buff 激活、aggro 重算调度、其他 aggro 来源、tank-pet、碰撞/LOS、
  朝向/netUpdate 和完整 TargetClosest 副作用仍为 `partial`/`deferred`。

### Batch N3.44：Player mount typed owner 接入 stealth tick（已完成窄切片）

- 新增 `PlayerMountStateComponent`，承载显式 `IsMounted`/`MountType`；`Set(-1)` 清除挂载，非负
  mount type 建立挂载状态。Player 创建路径默认挂载 `MountType = -1`。
- `DomeSimulation.AdvancePlayerStealth` 消费 mount owner，不再把 `isMounted` 硬编码为 false；完整
  mount 召唤、物理、速度和装备来源仍由后续系统负责。
- 验收证据：`Npc.Verification` 覆盖 mount type 设置/清除；`Build/diagnostics/npc-complete/task-10-interaction/20260829-103000/`
  中 NPC verifier、Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：mount 生命周期/召唤与协议、stealth 装备/Buff 激活、aggro 重算调度、其他 aggro 来源、
  tank-pet、碰撞/LOS、朝向/netUpdate 和完整 TargetClosest 副作用仍为 `partial`/`deferred`。

### Batch N3.45：Item summoning metadata 驱动 mount owner（已完成窄切片）

- `PlayerNpcTargetingSystem.TryApplyMountSummon` 消费已有 `ItemDefinition.Summoning.MountType`
  source-backed metadata；只有非负 mount type 才调用 `PlayerMountStateComponent.Set` 并建立挂载。
- `DomeSimulation.CommitItemUses` 在 item 使用成功、资源校验和消耗完成后应用该 summon contract；
  null 或负 mount type 返回未应用，不隐式卸载现有坐骑，也不把普通非 mount 物品解释为卸载命令。
- 验收证据：`Npc.Verification` 覆盖 mount summon 建立和 null no-op；
  `Build/diagnostics/npc-complete/task-10-interaction/20260829-113000/` 中 NPC verifier、
  Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：完整 mount lifecycle、取消/卸载命令、mount physics/速度、协议同步和 summon behavior
  仍为 `partial`/`deferred`；stealth 装备/Buff 激活、aggro 重算调度、tank-pet、碰撞/LOS、
  朝向/netUpdate 和完整 `TargetClosest` 副作用同样未闭合。

### Batch N3.46：Legacy mount control 的显式卸载契约（已完成窄切片）

- `PlayerNpcTargetingSystem.TryApplyMountControl` 消费兼容层的 `ushort? MountType`：非空值建立
  指定 mount，`null` 调用 `PlayerMountStateComponent.Set(-1)`，对应 legacy PlayerControls 中
  无 mount type 时的 dismount 分支。
- 当前批次只建立 Simulation 侧纯状态契约并由 verifier 覆盖建立/卸载；session 输入到世界命令的
  调度、权限和协议回写仍未接入，不能把该方法描述为完整网络 mount lifecycle。
- 验收证据：本批次 verifier、Simulation 和 Server Release 日志应记录在新的同一时间戳目录；
  完整 mount physics/速度、协议同步、stealth 装备/Buff 激活、aggro 重算调度及
  `TargetClosest` 副作用仍为 `partial`/`deferred`。

### Batch N3.47：PlayerControls mount type 接入 server-owned Simulation（已完成窄切片）

- `DomeNetworkUpdateBridge` 保留兼容投影中的 `ushort? MountType`，并将其带入
  `ApplyPlayerControlCommand`；mount/dismount 信息不再在 bridge 边界丢失。
- `DomeServer` 在玩家存在、active 且 item interaction 校验通过后调用
  `DomeSimulation.ApplyPlayerMountControl`，由 `PlayerMountStateComponent` 成为 server-owned
  Simulation 的坐骑状态 owner；null 继续表示显式卸载。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260829-130000/` 中 NPC verifier、
  Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- SessionReplication 聚焦回归同样通过：`Build/diagnostics/npc-complete/task-10-interaction/20260829-133000/session-replication-n3.47.log`
  保持 mount compatibility state 在旧 session 投影边界、会话存活和 server-owned movement 约束内。
- 边界：完整 session 权限/重放策略、mount 协议回写、物理/速度/碰撞、stealth 装备/Buff 激活、
  aggro 重算调度、tank-pet、LOS 和完整 `TargetClosest` 副作用仍为 `partial`/`deferred`。

### Batch N3.48：Mount state snapshot 与 PlayerControls protocol projection（已完成窄切片）

- `PlayerSnapshot` 与 `PlayerStateSnapshot` 现在携带可选 `MountType`，来源是
  `PlayerMountStateComponent`；未挂载仍投影为 null，避免把兼容字段伪装成默认 mount。
- `TerrariaPacketCodec.EncodePlayerControls` 支持可选 mount suffix：有值时设置 legacy mount bit
  并写入 `ushort`，无值时保持原有 14-byte control payload；Server 初始 player projection 消费
  authoritative state 的 mount type。
- 验收证据：Protocol compatibility、Npc verifier、Simulation 和 Server Release 均通过；
  `Build/diagnostics/npc-complete/task-10-interaction/20260829-150000/` 中对应日志为 PASS，
  构建为 0 warnings/0 errors，Npc round-trip rerun 另见 `npc-n3.48-rerun.log`。
- 边界：完整 mount lifecycle、physics/速度/碰撞、session replay/权限细节、协议双向状态回写、
  stealth 装备/Buff 激活、aggro 调度和完整 `TargetClosest` 副作用仍为 `partial`/`deferred`。

### Batch N3.49：Mount owner 的 authoritative snapshot projection（已完成窄切片）

- `DomeSimulation.ApplyPlayerMountControl` 写入 `PlayerMountStateComponent` 后，
  `CreateSnapshot` 与 `CreatePlayerStateSnapshot` 都从该 owner 投影 `MountType`；卸载状态以 null
  保持，不回退到兼容层缓存或默认值。
- Npc verifier 覆盖 server-owned Simulation API 的建立/卸载及两种 snapshot 读取路径，证明
  mount state 不只存在于纯 system 或协议输入层。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260829-170000/` 中 NPC verifier、
  Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。
- 边界：磁盘持久化/跨重启 restore、session replay 去重、mount physics/速度/碰撞、协议双向回写、
  stealth 装备/Buff 激活、aggro 调度及完整 `TargetClosest` 副作用仍为 `partial`/`deferred`。

### Batch N3.50：Mount type 生命周期值域校验（已完成窄切片）

- `PlayerMountStateComponent.Set` 拒绝小于 `-1` 或大于 `ushort.MaxValue` 的 mount type，使 item
  metadata、PlayerControls wire 值和 snapshot 投影共享同一可表示值域；`-1` 仍表示卸载。
- verifier 覆盖不可表示 mount type 的拒绝路径。玩家持久化格式的 source-backed 字段集合不包含
  mount state，因此本批不把瞬态 mount 写入账户存档；跨重启恢复仍明确为 deferred。
- 初次增量 verifier 暴露 stale Simulation DLL（源文件已含 guard，但旧 DLL 未重编）；随后使用
  `-t:Rebuild` 重新生成 Simulation 后重跑通过，避免把 stale artifact 当成语义证据。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260829-193000/` 中
  `simulation-rebuild.log`、`npc-n3.50-rerun.log` 和 `server-build.log` 均成功，构建为 0 warnings/0 errors。
- 边界：跨重启 mount restore、session replay、完整 mount physics/速度/碰撞、协议回写及 lifecycle
  side effects 仍为 `partial`/`deferred`。

### Batch N3.51：Inactive player 的 mount replay 生命周期门禁（已完成窄切片）

- `DomeSimulation.ApplyPlayerMountControl` 对不存在或 inactive player 拒绝 mount/dismount 输入，
  防止 stale session/replay 在死亡或失活实体上重新写入坐骑状态。
- verifier 通过真实 `QueuePlayerDamage -> Tick` 失活路径覆盖拒绝，并确认被拒绝的卸载不会改变
  authoritative mount snapshot。该门禁不替代完整 session replay nonce、重连顺序和协议确认机制。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260829-210000/` 中 NPC verifier、
  Simulation 和 Server Release 均 PASS，且为 0 warnings/0 errors。跨重启 restore、mount physics/
  速度/碰撞、协议回写及完整 lifecycle side effects 仍为 `partial`/`deferred`。

### Batch N3.52：Player death 清理 mount owner（已完成窄切片）

- legacy `Player.KillMe` 在进入 dead 状态前调用 mount dismount；Simulation death commit 现在在
  `PlayerLifecycleSystem` 成功开始死亡后调用 `PlayerMountStateComponent.Set(-1)`，避免死亡 snapshot
  继续携带 stale mount。
- verifier 通过真实 damage/death 路径确认 `PlayerSnapshot` 与 `PlayerStateSnapshot` 均投影为 null，
  且后续 inactive replay 仍被拒绝。复活、重连和完整 mount cleanup side effects 未扩展。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260829-230000/` 中 NPC verifier、
  Simulation Rebuild 和 Server Release 均 PASS，且为 0 warnings/0 errors。跨重启 restore、session
  replay ordering、mount physics/速度/碰撞、协议回写及完整 lifecycle side effects 仍为 `partial`/`deferred`。

### Batch N3.53：Mount owner 默认未挂载不变量（已完成窄切片）

- `PlayerMountStateComponent` 增加显式 parameterless constructor，保证任意 `new()` 的默认状态都是
  `IsMounted = false, MountType = -1`；这与 Player 创建、死亡清理和 snapshot null projection 统一。
- verifier 覆盖默认 struct、建立和清除状态；本批不扩展磁盘存档或完整 replay ordering。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-010000/` 中
  `simulation-rebuild.log`、`npc-n3.53.log` 和 `server-build.log` 均成功，且为 0 warnings/0 errors。
  mount physics/速度/碰撞、协议回写及完整 lifecycle side effects 仍为 `partial`/`deferred`。

### Batch N3.54：Player respawn 清理 mount owner（已完成窄切片）

- legacy `Player.Spawn` 清除 dead 状态；Simulation 的成功 respawn 分支现在再次调用
  `PlayerMountStateComponent.Set(-1)`，保证死亡后等待期间或恢复提交时都不会带回 stale mount。
- verifier 覆盖真实 damage/death/自动 respawn 路径，并确认复活玩家 active 且 snapshot 的
  `MountType` 仍为 null。该批不扩展跨重启存档或完整 mount physics/lifecycle side effects。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-030000/` 中
  `simulation-rebuild.log`、`npc-n3.54.log` 和 `server-build.log` 均成功，且为 0 warnings/0 errors。
  session replay ordering、协议回写、mount physics/速度/碰撞及完整生命周期仍为 `partial`/`deferred`。

### Batch N3.55：Mount owner 受控写入边界（已完成窄切片）

- `PlayerMountStateComponent.IsMounted` 与 `MountType` 改为公开只读、私有写入；唯一生产状态
  变更入口是 `Set`，因此 mount 创建、summon、PlayerControls、death 和 respawn 都强制经过相同的
  值域校验与 `-1` 未挂载归一化。
- Player 创建不再使用 object initializer 绕过 owner。verifier 既有默认/建立/清除/值域覆盖继续验证
  该边界；不扩展跨重启存档、replay sequencing 或完整 physics。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-050000/` 中
  `simulation-rebuild.log`、`npc-n3.55.log` 和 `server-build.log` 均成功，且为 0 warnings/0 errors。
  完整 mount lifecycle、协议回写和物理 side effects 仍为 `partial`/`deferred`。

### Batch N3.56：Mount compatibility session regression（已完成验证节点）

- 重新运行现有 SessionReplication verifier，确认 PlayerControls mount compatibility state、会话
  存活、replacement session、PVS movement 和 server-owned authority 约束均保持通过。
- 该回归只证明兼容输入边界没有破坏既有 session 行为，不证明 mount replay 的 nonce/排序、跨连接
  去重或协议确认闭环；这些仍属于后续 deferred contract。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-070000/session-replication-current.log`
  全部 PASS；本批未修改运行时代码。

### Batch N3.57：Mount type registry 边界与玩家生命周期回归（已完成验证节点）

- legacy `Mount.SetMount` 还依赖 `MountID.Count`/mount data 表；当前 Simulation 没有对应的
  source-backed typed registry，因此现有 `ushort` 检查只保证 wire/快照可表示域，不把每个数值宣称为
  可执行 mount type。完整类型表与能力表继续 deferred。
- PlayerLifecycle verifier 复跑通过，确认 mount owner 的默认未挂载、死亡/复活清理没有破坏玩家
  identity、lifecycle 和 account ownership 分层。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-090000/player-lifecycle-current.log`
  PASS；本批未修改运行时代码。

### Batch N3.58：Source-backed MountID value registry（已完成窄切片）

- 从 legacy `Terraria.ID.MountID.cs` 提取 `None = -1`、`Count = 64`，新增
  `MountTypeRegistry`；`PlayerMountStateComponent`、item definition compiler 和 legacy item adapter
  统一拒绝 `64+`，接受 `0..63` 与 `-1`。
- 本批只迁移 ID/value-domain contract，不猜测 `MountID.Sets` 的 Cart、CanDash、碰撞或速度能力；
  能力表与完整 mount behavior 继续 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-110000/` 中
  `simulation-rebuild.log`、`npc-n3.58.log` 和 `server-build.log` 均成功，且为 0 warnings/0 errors。
  持久化、replay ordering、协议回写及完整 lifecycle side effects 仍为 `partial`/`deferred`。

### Batch N3.59：Source-backed MountID capability registry（已完成窄切片）

- 从 legacy `Terraria.ID.MountID.Sets` 提取 `Cart`、`CanDash`、`DontHoldItems` 的精确集合，新增
  不可变 `MountCapabilityRegistry` 查询；该表与 `MountTypeRegistry` 的 `0..63` value domain 分离。
- 本批只建立 capability lookup contract，不把 Cart/冲刺/持物限制直接接入尚未迁移的 movement、
  collision 或 item-use behavior；`DismountsOnItemUse` 等 MountData 级字段仍无完整 source-backed 表。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-140000/` 中
  `simulation-rebuild.log`、`npc-n3.59.log`、`server-build.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的 capability 正反例均通过。持久化、replay ordering、
  协议回写及完整 mount lifecycle 仍为 `partial`/`deferred`。

### Batch N3.60：Source-backed MountData item-use capability（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 `MountData.dismountsOnItemUse` 初始化与
  `TryEarlyDismount` 消费点确认，mount type `55、56、61` 是精确的自动卸载能力集合；
  `MountCapabilityRegistry.DismountsOnItemUse` 已提供不可变查询。
- 本批只迁移 capability lookup，不把查询直接接入 Player item-use；完整 `CanDismount`、空间碰撞
  和卸载副作用仍缺少 Simulation 侧等价契约，继续保持 deferred。
- 聚焦 verifier 与 Simulation Rebuild 证据已生成于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-150000/`，均通过；此前增量 Server
  build 的 `PlayerStateProjection.cs:35` 错误由 stale artifact 触发，原始失败保留在
  `server-build.log`。使用串行 `-t:Rebuild` 后，Server Release 通过且 0 warnings/0 errors，证据为
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-153000/server-rebuild.log`。

### Batch N3.61：Source-backed transformation/hidden mount capabilities（已完成窄切片）

- 从 legacy `Terraria.ID.MountID.Sets` 提取 `IsTransformationMount` 与 `PlayerIsHidden`；两者的
  source-backed 集合均为 `52、54、55、56、61`，新增不可变 `MountCapabilityRegistry` 查询。
- 本批只建立 definition lookup contract，不把查询直接接入客户端表现、变身状态迁移或完整 mount
  lifecycle；这些 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-160000/` 中
  `simulation-rebuild.log`、`npc-n3.61.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的正反例均通过。

### Batch N3.62：Source-backed hook-compatible mount capability（已完成窄切片）

- 从 legacy `Terraria.ID.MountID.Sets.CanUseHooks` 提取精确集合 `54、57、58、59、60`，新增
  `MountCapabilityRegistry.CanUseHooks` 不可变查询。
- 本批只建立抓钩兼容性 definition lookup，不接入抓钩状态、碰撞或 mount movement；这些运行时
  side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-170000/` 中
  `simulation-rebuild.log`、`npc-n3.62.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的正反例均通过。

### Batch N3.63：Source-backed crowd-control dismount immunity（已完成窄切片）

- 从 legacy `Terraria.ID.MountID.Sets.DontDismountWhenCCed` 提取精确集合 `55、56、61`，新增
  `MountCapabilityRegistry.DoesNotDismountWhenCrowdControlled` 不可变查询。
- 本批只建立 crowd-control dismount capability lookup，不接入 CC 状态、空间碰撞检查或卸载副作用；
  完整 dismount contract 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-180000/` 中
  `simulation-rebuild.log`、`npc-n3.63.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的正反例均通过。

### Batch N3.64：Source-backed mount frame-preservation capabilities（已完成窄切片）

- 从 legacy `Terraria.ID.MountID.Sets` 提取 `DoesNotOverrideBodyFrames`、
  `DoesNotOverrideLegFrames`、`DoesNotOverrideBackpackDraw`；三组集合均为 `57、58、59、60`，
  在 registry 中保持独立语义查询。
- 本批只建立 definition lookup，不接入客户端 frame/render projection 或完整 mount presentation；
  相关表现层与生命周期 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-190000/` 中
  `simulation-rebuild.log`、`npc-n3.64.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的正反例均通过。

### Batch N3.65：Source-backed roller-skates mount capability（已完成窄切片）

- 从 legacy `Terraria.ID.MountID.Sets.IsRollerSkates` 提取精确集合 `57、58、59、60`，新增
  `MountCapabilityRegistry.IsRollerSkates` 不可变查询。
- 本批只建立 definition lookup，不接入滑行速度、动画、碰撞或客户端表现；相关 movement/render
  side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-200000/` 中
  `simulation-rebuild.log`、`npc-n3.65.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的正反例均通过。

### Batch N3.66：Source-backed extra-jump blocking mount capability（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 `MountData.blockExtraJumps` 初始化提取 true 集合
  `5、6、7、8、11、12、13、16、23、44、49、56、61`，并核对显式 false 条目 `9、46`；
  新增 `MountCapabilityRegistry.BlocksExtraJumps` 不可变查询。
- 本批只建立 per-mount definition lookup，不接入 jump/movement、碰撞或完整 mount physics；相关
  side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-210000/` 中
  `simulation-rebuild.log`、`npc-n3.66.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的正反例均通过。

### Batch N3.67：Source-backed hover mount capability（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 `MountData.usesHover` 初始化提取 true 集合
  `5、7、8、12、23、44、48、49、56、61`；其余 mount type 维持默认 false，新增
  `MountCapabilityRegistry.UsesHover` 不可变查询。
- 本批只建立 movement definition lookup，不接入悬停积分、飞行资源、碰撞或完整 mount physics；
  相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-220000/` 中
  `simulation-rebuild.log`、`npc-n3.67.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的正反例均通过。

### Batch N3.68：Source-backed mount flight-time definition（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 `MountData.flightTimeMax` 初始化提取数值：`0、2` 为 `160`；
  `5、7、8、12、23、44、48、56、61` 为 `320`；`50` 为 `80`；其余 mount type 默认 `0`。
  新增 `MountCapabilityRegistry.GetFlightTimeMax` 查询。
- 本批只迁移 per-mount 数值 definition，不接入飞行计时、疲劳、翅膀资源、碰撞或完整 hover physics；
  相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-230000/` 中
  `simulation-rebuild.log`、`npc-n3.68.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的多值断言均通过。

### Batch N3.69：Source-backed mount fatigue definition（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 `MountData.fatigueMax` 初始化提取数值：
  `5、7、8、12、23、44、49、56、61` 为 `320`；`9、46` 显式为 `0`，其余 mount type 默认 `0`。
  新增 `MountCapabilityRegistry.GetFatigueMax` 查询。
- 本批只迁移 per-mount 数值 definition，不接入疲劳恢复、hover 消耗、碰撞或完整飞行 physics；
  相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-000000/` 中
  `simulation-rebuild.log`、`npc-n3.69.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的多值断言均通过。

### Batch N3.70：Source-backed mount fall-damage definition（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 `MountData.fallDamage` 初始化提取数值，新增
  `MountCapabilityRegistry.GetFallDamageMultiplier`；未显式赋值的 mount type 按 `MountData` 默认 `0`。
- 本批只迁移 per-mount 物理 definition，不接入跌落伤害结算、碰撞或完整 mount physics；相关 side
  effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-010000/` 中
  `simulation-rebuild.log`、`npc-n3.70.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的多值断言均通过。

### Batch N3.71：Source-backed mount run-speed definition（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 direct `MountData.runSpeed` 初始化以及
  `SetAsMinecart`、`SetAsRollerSkate`、`SetAsChillet` helper 解析最终值，新增
  `MountCapabilityRegistry.GetRunSpeed`；未初始化条目按 `MountData` 默认 `0`。
- 本批只迁移基础 movement 数值 definition，不接入加速度、dash、速度积分、碰撞或完整 mount
  physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-020000/` 中
  `simulation-rebuild.log`、`npc-n3.71.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的 direct/helper 多值断言均通过。

### Batch N3.72：Source-backed mount dash-speed definition（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 direct `MountData.dashSpeed` 初始化以及
  `SetAsMinecart`、`SetAsRollerSkate`、`SetAsHorse`、`SetAsChillet` helper 解析最终值，新增
  `MountCapabilityRegistry.GetDashSpeed`；未初始化条目按 `MountData` 默认 `0`。
- 本批只迁移基础 dash 数值 definition，不接入 SuperCart 动态覆盖、冲刺状态机、速度积分、碰撞或
  完整 mount physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-030000/` 中
  `simulation-rebuild.log`、`npc-n3.72.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的 direct/helper 多值断言均通过。

### Batch N3.73：Source-backed mount acceleration definition（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 direct `MountData.acceleration` 初始化以及
  `SetAsMinecart`、`SetAsRollerSkate`、`SetAsHorse`、`SetAsChillet` helper 解析最终值，新增
  `MountCapabilityRegistry.GetAcceleration`；未初始化条目按 `MountData` 默认 `0`。
- 本批只迁移基础 acceleration 数值 definition，不接入 SuperCart 动态覆盖、速度积分、dash 状态机、
  碰撞或完整 mount physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-040000/` 中
  `simulation-rebuild.log`、`npc-n3.73.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的 direct/helper 多值断言均通过。

### Batch N3.74：Source-backed mount jump-height definition（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 direct `MountData.jumpHeight` 初始化以及
  `SetAsMinecart`、`SetAsRollerSkate`、`SetAsHorse`、`SetAsChillet` helper 解析最终值，新增
  `MountCapabilityRegistry.GetJumpHeight`；未初始化条目按 `MountData` 默认 `0`。
- 本批只迁移基础 jump 数值 definition，不接入 SuperCart 动态覆盖、jump 状态机、速度积分、碰撞或
  完整 mount physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-050000/` 中
  `simulation-rebuild.log`、`npc-n3.74.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的 direct/helper 多值断言均通过。

### Batch N3.75：Source-backed mount jump-speed definition（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 direct `MountData.jumpSpeed` 初始化以及
  `SetAsMinecart`、`SetAsRollerSkate`、`SetAsHorse`、`SetAsChillet` helper 解析最终值，新增
  `MountCapabilityRegistry.GetJumpSpeed`；未初始化条目按 `MountData` 默认 `0`。
- 本批只迁移基础 jump 数值 definition，不接入 `SuperCartJumpSpeed` 动态覆盖、jump 状态机、速度
  积分、碰撞或完整 mount physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-060000/` 中
  `simulation-rebuild.log`、`npc-n3.75.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的 direct/helper 多值断言均通过。

### Batch N3.76：Source-backed mount swim-speed definition（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 `MountData.swimSpeed` 初始化提取非默认值：`4=10`、`8=4`、
  `12=16`、`44=3`、`48=8`、`49=14`；其余 mount type 默认 `0`，新增
  `MountCapabilityRegistry.GetSwimSpeed` 查询。
- 本批只迁移基础水中移动数值 definition，不接入水中碰撞、加速度、液体状态或完整 mount physics；
  相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-070000/` 中
  `simulation-rebuild.log`、`npc-n3.76.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的多值断言均通过。

### Batch N3.77：Source-backed mount height-boost definition（已完成窄切片）

- 从 legacy `Terraria.Mount` 的 direct `MountData.heightBoost` 初始化以及
  `SetAsMinecart`、`SetAsHorse`、`SetAsChillet` helper 解析最终值，新增
  `MountCapabilityRegistry.GetHeightBoost`；未初始化条目按 `MountData` 默认 `0`。
- 本批只迁移坐标/碰撞基线数值 definition，不接入 hitbox 重算、空间检查或完整 mount physics；
  相关 side effects 继续保持 deferred。
- 首轮 verifier 发现 `SetAsHorse` 的 `40–42` 被错误归入 minecart height boost；修正为 source-backed
  `34` 后，最终验收证据为 `Build/diagnostics/npc-complete/task-10-interaction/20260831-090000/` 中
  `simulation-rebuild.log`、`npc-n3.77.log`、`server-rebuild.log` 和 `diff-check.log`，均成功；
  Simulation/Server 为 0 warnings/0 errors，聚焦 verifier 的 direct/helper 多值断言均通过。

### Batch N3.78：NPC AI style value-domain guard（已完成窄切片）

- 根据 legacy `NPC.aiStyle` 的默认值与初始化赋值均为非负整数，`NpcDefinition` 现在拒绝负
  `AiStyle`，防止无效行为路由进入 ECS；verifier 增加负值拒绝断言。
- 本批只强化定义输入契约，不宣称 158 个 `AI_###` 行为族或完整 AI parity 已迁移。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-100000/` 中
  `simulation-rebuild.log`、`npc-n3.78.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，聚焦 verifier 的负值拒绝断言通过。

### Batch N3.82：NPC AI style source upper-bound guard（已完成窄切片）

- 对 legacy `NPC.aiStyle` 的 615 个初始化赋值做 source scan，确认值域为 `0..127`；
  `NpcDefinition` 现在拒绝 `<0` 或 `>127`，verifier 覆盖 `128` 拒绝边界。
- 本批只强化 AI style 输入域，不宣称 158 个 `AI_###` 行为族或完整 AI parity 已迁移。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-150000/` 中
  `simulation-rebuild.log`、`npc-n3.82.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，`AiStyle=128` 拒绝边界通过。

### Batch N3.83：NPC net-id source upper-bound guard（已完成窄切片）

- 根据 legacy `Terraria.ID.NPCID.Count = 697`，实际 definition `netID` 索引域为 `1..696`；
  `NpcDefinition` 现在拒绝 `NetId <= 0` 或 `>696`，verifier 同时覆盖 `696` 接受与 `697` 拒绝边界。
- 负数/特殊网络 sentinel 不属于 `NpcDefinition` 的实体 definition contract，仍由兼容/协议层单独处理。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-160000/` 中
  `simulation-rebuild.log`、`npc-n3.83.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  补充 `20260831-170000/npc-n3.83-boundary.log` 验证 `NetId=696` 接受。Simulation/Server 为
  0 warnings / 0 errors，`NetId=697` 拒绝边界通过。

### Batch N3.84：NPC definition-component value-domain guard（已完成窄切片）

- `NpcDefinitionComponent` 之前可绕过 `NpcDefinition` 直接构造无效 `NetId`/枚举；现下沉
  `DefinitionId > 0`、`NetId 1..696`（`NPCID.Count`）及 `Faction/Category` 定义校验，verifier
  增加 component 直构 `NetId=697` 拒绝断言。
- 本批只闭合 component 输入边界，不宣称完整 NPCID 表或 AI parity 已迁移。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-180000/` 中
  `simulation-rebuild.log`、`npc-n3.84.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，component 直构边界断言通过；补充回归证据
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-190000/npc-current-regression.log`。

### Batch N3.85：NPC definition-component enum boundary regression（已完成窄切片）

- 补充 verifier 对 `NpcDefinitionComponent` 直构非法 `Faction=99` 的拒绝断言，覆盖 N3.84 下沉
  的枚举值域校验，避免仅验证 NetId 而遗漏 faction/category 边界。
- 本批只增加输入契约回归，不宣称完整 NPC faction/category 表或 AI parity 已迁移。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-200000/` 中
  `npc-n3.85.log` 与 `diff-check.log` 均成功；component enum 边界回归通过。本批仅修改 verifier
  与文档，生产 Simulation/Server 产物沿用 N3.84 的 Rebuild 证据。

### Batch N3.86：NPC definition-component category boundary regression（已完成窄切片）

- 补充 verifier 对 `NpcDefinitionComponent` 直构非法 `Category=99` 的拒绝断言，与 N3.85 的
  `Faction=99` 形成对称覆盖；生产实现未改变。
- 本批只增加输入契约回归，不宣称完整 NPC faction/category 表或 AI parity 已迁移。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-200000/` 中
  `npc-n3.86.log` 与 `diff-check.log` 均成功；category 边界回归通过。本批仅修改 verifier
  与文档，生产 Simulation/Server 产物沿用 N3.84 的 Rebuild 证据。

### Batch N3.87：Source-backed MountData early-dismount contract（已完成窄切片）

- 根据 legacy `Mount.TryEarlyDismount` 的执行条件，新增
  `PlayerNpcTargetingSystem.TryApplyEarlyDismount`：仅当当前 mount 的
  `dismountsOnItemUse` 为真且外部传入的 `canDismount` 为空间/碰撞检查成功时才清除 typed mount state。
- 本批只迁移早期卸载决策契约，不伪造 `CanDismount` 的 crowd-control、hitbox 变更或碰撞实现；这些
  side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-210000/` 中
  `simulation-rebuild-rerun.log`、`npc-n3.87-rerun.log`、`server-rebuild-rerun.log` 和
  `diff-check-rerun.log` 均成功；Simulation/Server 为 0 warnings / 0 errors，早期卸载的
  capability、空间检查正反例均通过。

### Batch N3.88：Source-backed MountData run-speed movement integration（已完成窄切片）

- `PlayerControlSystem` 现在对 mounted 玩家读取 `MountCapabilityRegistry.GetRunSpeed`，替代统一的
  `3.0f` 水平速度；未 mounted 玩家继续保持原有基础速度。
- 本批只接入 source-backed run-speed definition 到玩家控制层，不接入加速度、dash、特殊坐骑动态
  速度、碰撞或完整 mount physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-220000/` 中
  `simulation-rebuild.log`、`npc-n3.88.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，mounted 与未 mounted 的速度回归通过。

### Batch N3.90：Source-backed MountData acceleration, dash, jump and hover movement integration（已完成窄切片）

- `PlayerControlSystem` 对 mounted 玩家现在按 `GetAcceleration` 将水平速度趋近于
  `direction * GetRunSpeed`，松开方向键时也按同一 acceleration 逐步减速；未 mounted 路径保持
  原有直接速度赋值。
- 对支持 dash 的 mount，按输入读取 `GetDashSpeed` 作为目标速度；不支持 dash 时回落到 run speed。
- mounted 跳跃在 grounded 条件下读取 `GetJumpSpeed`，未 mounted 仍使用原有基础跳跃速度。
- `PlayerGravitySystem` 对 `UsesHover` 坐骑跳过基础重力步进，普通坐骑继续应用原有重力。
- 本批只接入基础 acceleration/dash/jump/hover definitions，不接入 flight-time resource、SuperCart
  动态覆盖、特殊坐骑速度、碰撞或完整 mount physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-000000/` 中
  `simulation-rebuild.log`、`npc-n3.90-rerun.log`、`server-rebuild.log` 和 `diff-check-rerun.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，mounted 加速、dash、跳跃、hover gravity gate 和松开
  减速回归通过。

### Batch N3.91：Source-backed MountData flight-time resource contract（已完成窄切片）

- `PlayerMountStateComponent` 现在在 mount 设置时从 `GetFlightTimeMax` 初始化
  `FlightTimeRemaining`，支持按 flying tick 消耗、显式恢复，以及卸载时清零。
- 本批只建立 typed flight-time resource contract，不接入翅膀输入、hover 状态机、疲劳消耗、碰撞或
  完整 mount physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-010000/` 中
  `simulation-rebuild.log`、`npc-n3.91.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，flight-time 初始化、消耗、恢复和卸载清零回归通过。

### Batch N3.92：Source-backed MountData fatigue resource contract（已完成窄切片）

- `PlayerMountStateComponent` 现在在 mount 设置时从 `GetFatigueMax` 初始化
  `FatigueRemaining`，支持按显式 fatiguing tick 消耗、恢复，以及卸载时清零。
- 本批只建立 typed fatigue resource contract，不接入 hover/飞行输入、疲劳恢复规则、碰撞或完整
  mount physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-020000/` 中
  `simulation-rebuild.log`、`npc-n3.92.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，fatigue 初始化、消耗、恢复和卸载清零回归通过。

### Batch N3.93：Source-backed MountData flight-input authority contract（已完成窄切片）

- `PlayerNpcTargetingSystem.TryConsumeFlightInput` 现在要求显式 `upPressed`，且 mount 必须支持
  `CanUseWings` 或 `UsesHover`，通过后才消耗 `FlightTimeRemaining`。
- 本批只建立输入权限与资源消耗边界，不接入上升速度、翅膀表现、hover 状态机、碰撞或完整 mount
  physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-030000/` 中
  `simulation-rebuild.log`、`npc-n3.93.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，flight input capability 与 budget 消耗边界回归通过。

### Batch N3.94：Source-backed MountData vertical flight movement integration（已完成窄切片）

- `PlayerControlSystem` 对 `Up` 输入接入 wings/hover capability 与 flight budget：预算成功消耗时，
  mounted 玩家获得 source-backed `GetJumpSpeed` 垂直速度；不满足 capability 或预算条件时不施加。
- 本批只接入垂直速度与资源消耗，不接入翅膀表现、完整飞行状态机、疲劳恢复、碰撞或 mount physics；
  相关 side effects 继续保持 deferred。

### Batch N3.95：Source-backed MountData flight/fatigue coupling contract（已完成窄切片）

- `TryConsumeFlightInput` 现在要求 flight 与 fatigue 两个预算同时可用，并在 eligible `Up` tick
  原子各消耗一格；任一预算耗尽时拒绝输入。
- 本批只建立资源耦合边界，不接入疲劳恢复、飞行退出条件、hover 状态机、碰撞或完整 mount physics；
  相关 side effects 继续保持 deferred。

### Batch N3.96：Source-backed MountData flight recharge and fatigue recovery contract（已完成窄切片）

- `PlayerMountStateComponent` 新增 `RechargeFlightTime` 与 `RecoverFatigue` 增量恢复 API，均向
  source-backed 最大值封顶，并拒绝负恢复量。
- 本批只建立有界资源恢复契约，不接入具体 tick 时序、装备/环境条件、hover 状态机、碰撞或完整
  mount physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-060000/` 中
  `simulation-rebuild.log`、`npc-n3.96-rerun.log`、`server-rebuild.log` 和 `diff-check-rerun.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，flight/fatigue 增量恢复、上限封顶和负值拒绝回归通过。

### Batch N3.97：Source-backed MountData flight/fatigue exhaustion contract（已完成窄切片）

- `PlayerMountStateComponent` 新增 `IsFlightExhausted` 与 `IsFatigueExhausted`，并验证预算归零后
  消耗 API 与 flight-input authority 均 fail-closed，不会继续递减。
- 本批只建立资源耗尽边界，不接入自动恢复、退出飞行状态机、碰撞或完整 mount physics；相关 side
  effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-070000/` 中
  `simulation-rebuild.log`、`npc-n3.97.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，flight/fatigue exhaustion 与 fail-closed 回归通过。

### Batch N3.98：Source-backed MountData grounded recovery authority（已完成窄切片）

- `PlayerNpcTargetingSystem.TryRecoverFlightResources` 现在要求 mounted、grounded 且恢复量为正，
  通过后同时恢复 flight 与 fatigue budget，并复用各自最大值封顶规则。
- 本批只建立 grounded recovery authority，不决定具体恢复速率、环境条件、hover 状态机、碰撞或完整
  mount physics；相关 side effects 继续保持 deferred。

### Batch N3.99：Source-backed MountData flight-input fatigue exhaustion regression（已完成窄切片）

- 补充 verifier：在 flight budget 仍为最大值但 fatigue budget 已耗尽时，`TryConsumeFlightInput`
  必须拒绝输入，闭合双预算 fail-closed 边界。
- 本批只增加资源耗尽回归，不接入自动恢复、飞行退出状态机、碰撞或完整 mount physics；相关 side
  effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-090000/` 中
  `npc-n3.99.log` 和 `diff-check.log` 均成功；双预算 fatigue exhaustion 回归通过，生产构建沿用
  N3.98 的 Simulation/Server 0 warnings / 0 errors 证据。

### Batch N3.100：Current mount resource contract regression（已完成窄切片）

- 对当前工作树中已下沉至 `PlayerMountStateComponent` 的 flight-input、exhaustion 与 recovery API
  进行 fresh verifier 回归，确认接口重排后仍保持既有边界。
- 本批只验证当前实现一致性，不扩大 mount physics 或 NPC AI parity 范围。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-100000/` 中
  `npc-current-regression.log` 与 `diff-check.log` 均成功；生产构建沿用最近通过的 N3.98/N3.99 证据。

### Batch N3.101：Legacy flight/fatigue recovery timing audit（已完成只读审计）

- 复核 legacy `Mount.Flight` 与 `Mount.FatigueRecovery` 后确认，fatigue 恢复并非简单 grounded
  tick：还依赖飞行结束、frame state、gravity/slow-fall 及 mount-specific 分支。
- 当前 ECS 仅保留 typed budget 与显式 recovery authority，未凭猜测接入自动时序；完整 flight/fatigue
  状态机继续标记 deferred。

### Batch N3.102：Source-backed MountData CanFly capability boundary（已完成窄切片）

- `MountCapabilityRegistry.CanFly` 现在要求 source-backed `flightTimeMax > 0`，并明确排除 legacy
  type `48`；`TryConsumeFlightInput` 在 wings/hover capability 之外同时受该边界约束。
- 本批只闭合 CanFly 输入资格，不接入 type `54` 的 dynamic `allowedToFly`、frame state、碰撞或完整
  flight physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-120000/` 中
  `simulation-rebuild.log`、`npc-n3.102.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，CanFly 正反例与 flight-input 资格回归通过。

### Batch N3.103：Source-backed MountData dynamic allowedToFly contract（已完成窄切片）

- `MountCapabilityRegistry.CanFly(int, bool allowedToFly)` 为 legacy type `54` 保留 dynamic
  `SelectiveFlyingMountData.allowedToFly` 输入；type `54` 仅在显式允许时可飞，其他类型继续使用静态
  `flightTimeMax`/type `48` 规则。
- 本批只冻结 dynamic input contract，不把未建模的 `allowedToFly` 状态接入玩家组件、翅膀表现、碰撞
  或完整 flight physics；相关 side effects 继续保持 deferred。

### Batch N3.104：Source-backed MountData flight frame-state capability boundary（已完成窄切片）

- `MountCapabilityRegistry.CanUseFlightFrame` 提取 legacy `Mount.Flight` 的 frame-state 表：type `56`
  允许 `2/3`，type `61` 允许 `2/3/4`，其他可飞 mount 允许 frame `4`。
- 本批只冻结 frame-state 查询，不把 frame 输入接入现有 flight consumption API，也不接入 gravity、
  slow-fall、翅膀表现、碰撞或完整 flight physics；相关 side effects 继续保持 deferred。

### Batch N3.105：Source-backed MountData frame-aware flight-input authority（已完成窄切片）

- `PlayerMountStateComponent` 与 `PlayerNpcTargetingSystem` 新增带 `frameState` 的
  `TryConsumeFlightInput` overload，先通过 `CanUseFlightFrame` 再执行既有双预算消耗；无 frame 参数
  的兼容入口保持不变。
- 本批只接入 frame-aware 输入资格，不接入 frame 状态机、gravity/slow-fall、翅膀表现、碰撞或完整
  flight physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-150000/` 中
  `simulation-rebuild.log`、`npc-n3.105.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，frame-aware flight-input 正反例回归通过。

### Batch N3.106：Current frame-aware mount contract regression（已完成窄切片）

- 对当前工作树的 frame-aware flight-input、CanFly、flight/fatigue exhaustion 与 recovery 接口执行
  fresh verifier 回归，确认同步重排后边界仍稳定。
- 本批只验证当前实现一致性，不扩大 dynamic `wingTimeMax`、frame/gravity 状态机或 NPC AI parity。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-160000/` 中
  `npc-current-regression.log` 与 `diff-check.log` 均成功；生产构建沿用最近通过的 N3.105 证据。

### Batch N3.107：Typed MountData flight frame-state owner（已完成窄切片）

- `PlayerMountStateComponent` 新增受限 `FlightFrameState` 与 `SetFlightFrameState`，仅接受可表示的
  `0..4`，并在 mount 设置/卸载边界重置为 `0`。
- 本批只闭合 frame-state 的 typed 表示性契约，不改变现有 flight consumption、frame 状态机、
  gravity/slow-fall、碰撞或完整 flight physics；相关 side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-170000/` 中
  `simulation-rebuild.log`、`npc-n3.107.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，flight frame-state typed owner 边界回归通过。
  
### Batch N3.108：NPC escape large-coordinate distance boundary（已完成窄切片）

- `NpcEscapeSystem` 现在使用 `double` 距离平方并在开方前比较最大距离，避免有限的大坐标在
  `float delta * delta` 中溢出为异常；超出范围统一产生 `OutOfRange` despawn command。
- 本批只修正 escape 数值边界，不宣称完整 legacy escape/despawn 规则或 AI parity。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-180000/` 中
  `simulation-rebuild.log`、`npc-n3.108.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，普通 escape、超范围和大坐标回归通过。

### Batch N3.109：NPC behavior registry deterministic ordering（已完成窄切片）

- `NpcBehaviorRegistry.RegisteredBehaviorIds` 现在按 `NpcBehaviorId` 数值排序，避免 dictionary 枚举
  顺序泄漏到 verifier、诊断或回放输出；注册与 fail-closed 语义不变。
- 本批只修正 registry 可重复性，不宣称新增 AI family 或完整行为 parity。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-190000/` 中
  `simulation-rebuild.log`、`npc-n3.109.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，registry deterministic ordering 回归通过。

### Batch N3.110：NPC escape vertical-facing boundary（已完成窄切片）

- `NpcEscapeSystem` 在仅有垂直位移时返回 neutral facing `0`，不再把零水平差误判为向左；水平
  逃逸方向保持 `-1/1`。
- 本批只修正 escape facing 边界，不宣称完整 legacy escape/despawn 规则或 AI parity。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-200000/` 中
  `simulation-rebuild.log`、`npc-n3.110.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，vertical escape neutral-facing 回归通过。

### Batch N3.111：Current NPC escape/registry regression（已完成窄切片）

- 对当前工作树的 escape numeric/facing 与 behavior registry deterministic ordering 进行 fresh
  verifier 回归，确认最近接口同步后仍保持既有边界。
- 本批只验证当前实现一致性，不扩大完整 escape/despawn 规则或 AI family parity。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-210000/` 中
  `npc-current-regression.log` 与 `diff-check.log` 均成功。

### Batch N3.112：Current NPC migration focused regression（已完成窄切片）

- 对当前工作树的 NPC definition、target/chase、escape numeric/facing、death/loot 和 behavior
  registry 边界执行 fresh verifier 回归，确认既有窄链路未回归。
- 本批只验证当前实现一致性，不扩大完整 AI family、Boss/event、LOS/collision 或客户端 parity。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-220000/` 中
  `npc-current-regression.log` 与 `diff-check.log` 均成功。
- 文档登记后的 fresh rerun 仍通过：`Build/diagnostics/npc-complete/task-10-interaction/20260901-230000/`
  中 `npc-current-regression.log`、`diff-check.log` 和 checkpoint JSON 解析均成功。

### Batch N3.113：NPC despawn encouragement typed owner（已完成窄切片）

- `NpcLifecycleComponent` 新增 `DespawnEncouraged`，并迁移 legacy `NPC.cs:7143-7163` 的
  `EncourageDespawn`/`DiscourageDespawn` 单向 `timeLeft` 阈值更新：前者只收紧计时器，后者只
  放宽计时器，同时更新对应标志。
- 本批只闭合生命周期字段/property 的 typed owner，不扩大完整 `CheckActive` 玩家范围、类型
  特例、网络消息、worm follow-up 或 AI parity。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260902-000000/` 中
  `simulation-rebuild.log`、`server-rebuild.log`、`npc-n3.113.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors。

### Batch N3.114：NPC `dontCountMe` lifecycle owner（已完成窄切片）

- `SpawnNpcCommand` 新增显式 `DoesNotCountMe` 输入，`NpcSpawnCommitSystem` 将其写入
  `NpcLifecycleComponent.DoesNotCountMe`；默认 spawn 仍为 `false`，显式 `true` 可在提交后保留。
- 该字段对应 legacy `NPC.cs:6063` 的 `dontCountMe` 实例字段；本批只建立输入到 typed owner
  的承载链，不凭类型分支推导完整表，也不接入 active-NPC slot accounting。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260902-010000/` 中
  `simulation-rebuild.log`、`server-rebuild.log`、`npc-n3.114.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors。

### Batch N3.115：Source-backed frame-aware hover gravity contract（已完成窄切片）

- `MountCapabilityRegistry.CanUseHoverFrame` 冻结 legacy `Mount.CanHover()` 的 frame 输入边界：
  普通 hover mount 保持 frame-independent，type `49` 仅在 frame `4` 激活。
- `PlayerMountStateComponent.IsHoverActive` 作为 typed owner，`PlayerGravitySystem` 仅依据该
  owner gate 基础重力；不引入未建模的自动 frame transition。
- 本批只冻结 frame/gravity 输入契约，不接入 collision、slow-fall、flight/fatigue recovery timing、
  wing input 或完整 mount physics；相关 side effects 继续保持 deferred。
- 来源证据：legacy `Terraria/Mount.cs:2477-2487` 的 `CanHover()`，以及
  `Mount.cs:2848-2998` 的 hover/flight gravity 分支。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260902-020000/` 中
  `simulation-rebuild.log`、`npc-n3.115.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors。

### Batch N3.116：NPC `takenDamageMultiplier` post-defense resolution（已完成窄切片）

- `NpcDefinition` 和 `NpcAuthorityComponent` 新增有界 `TakenDamageMultiplier`，默认 `1.0`，拒绝
  非 finite 或小于 `1.0` 的输入；`NpcSpawnCommitSystem` 将其写入 NPC authority owner。
- `DamageResolutionSystem` 保留 legacy 顺序：先计算防御后的 `double` 伤害，再应用倍率，最后转换为
  整数并提交生命扣减；因此奇数防御下不会提前截断倍率输入。
- 本批只迁移初始 definition/authority 与 `DamageNpcCommand` 的结算 consumer，不猜测 158 个 AI
  行为族何时动态切换 `1/3/10` 倍率；暴击、击退、`realLife` shared-life、完整 `StrikeNPC` 副作用和
  loot 规则继续保持 deferred。
- 来源证据：legacy `Terraria/NPC.cs:5977`、`15020`、`46737`、`46791`、`67498-67501`。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260902-060000/` 中
  `simulation-build.log` 与 `server-build.log` 均成功（0 warnings / 0 errors）；
  `20260902-080000/` 中 `verifier-build.log`、`verifier.log` 和 `diff-check.log` 均成功，
  multiplier owner、倍率结算和小数防御顺序回归通过；共享结算的下游 Combat verifier 证据位于
  `20260902-070000/combat-build.log` 与 `combat-verifier.log`。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-140000/` 中
  `simulation-rebuild.log`、`npc-n3.104.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，flight frame-state 正反例回归通过。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-130000/` 中
  `simulation-rebuild.log`、`npc-n3.103.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，type 54 dynamic `allowedToFly` 正反例回归通过。
- 本批为只读审计，无生产代码变更；后续需先冻结 frame/gravity 输入契约再实现。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-080000/` 中
  `simulation-rebuild.log`、`npc-n3.98.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，grounded recovery authority 与拒绝边界回归通过。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-050000/` 中
  `simulation-rebuild.log`、`npc-n3.95.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，flight/fatigue 原子耦合与拒绝边界回归通过。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260901-040000/` 中
  `simulation-rebuild.log`、`npc-n3.94-rerun.log`、`server-rebuild.log` 和 `diff-check-rerun.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，eligible mounted vertical input 与 hover gravity 对照回归通过。

### Batch N3.79：Mount fall-damage helper parity correction（已完成窄切片）

- 复核发现 N3.70 初始表遗漏 helper 覆盖：legacy `SetAsHorse` 的 `40–42` 与 `SetAsChillet` 的
  `62–63` 最终为 `0.5`，`SetAsRollerSkate` 的 `57–60` 最终为 `1.0`；已修正
  `GetFallDamageMultiplier` 并增加 verifier 正例。
- 本批只修正 definition lookup，不接入跌落伤害结算或完整 mount physics。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-110000/` 中
  `simulation-rebuild.log`、`npc-n3.79.log` 和 `diff-check.log` 均成功；此前并行 Server 编译的
  构造函数错误保留在 `20260831-110000/server-rebuild.log`，串行 Rebuild 后最终 Server 证据为
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-120000/server-rebuild.log`，
  0 warnings / 0 errors，helper 分支回归断言均通过。

### Batch N3.80：Mount horse-helper movement parity correction（已完成窄切片）

- helper parity 审计发现 `SetAsHorse` 的 `40–42` 在 `GetRunSpeed` 和 `GetJumpHeight` 中遗漏，错误回落
  为默认 `0`；已分别修正为 source-backed `3.0` 与 `6`，并增加 verifier 回归断言。
- 本批只修正 movement definition lookup，不接入速度积分、jump 状态机或完整 mount physics。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-130000/` 中
  `simulation-rebuild.log`、`npc-n3.80.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，horse helper 回归断言均通过。

### Batch N3.81：Source-backed roller-skate wing/track capabilities（已完成窄切片）

- 从 legacy `SetAsRollerSkate` 的 `CanUseWings` 与 `CanRideMinecartTracks` 赋值确认，两组集合均为
  `57、58、59、60`；新增 registry 独立查询。
- 本批只建立 movement capability lookup，不接入翅膀资源、轨道碰撞或完整 mount physics；相关
  side effects 继续保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260831-140000/` 中
  `simulation-rebuild.log`、`npc-n3.81.log`、`server-rebuild.log` 和 `diff-check.log` 均成功；
  Simulation/Server 为 0 warnings / 0 errors，roller-skate helper 正反例均通过。

### Batch N3.82：`SpawnedFromStatue` statue-spawn loot qualification（已完成窄切片）

- 原始 `NPC.cs:65333-65335` 在满足 `NPCID.Sets` statue-loot policy 时，
  对 `SpawnedFromStatue` 实例在 `NPCLoot` 前直接返回；当前 ECS 已有
  `NpcSpawnStateComponent.SpawnedFromStatue`，但死亡发布边界此前未消费该状态。
- `NpcDefinition.SuppressLootWhenSpawnedFromStatue` 作为显式 definition policy，
  `NpcDeathInput` 携带实例 statue 来源和 policy，`NpcDeathSystem` 在发布前执行 scoped
  suppression；普通 spawn 或 policy=false 保持原有掉落链路。
- 本批只迁移死亡到掉落的资格边界，不声称完整 `NPCID.Sets`、hardmode/rarity roll、
  特殊死亡投射物或 capture/release parity；这些继续 `partial/deferred`。
- 验收证据：本批 fresh 证据写入
  `Build/diagnostics/npc-complete/task-10-interaction/20260829-*/`，包括
  Simulation/Server serial build、NPC verifier 和 `git diff --check`。

### Batch N3.83：`npcSlots` weighted spawn-cap accounting（进行中）

- 原始 `NPC.cs:6051` 的 `npcSlots` 默认值为 `1f`；`NPC.cs:176-181` 在 invasion
  boss cap 中累加该权重，`NPC.cs:64460-64469` 又将其累加到玩家的
  `nearbyActiveNPCs`，因此旧语义不是整数 active-NPC 数。
- 当前批次新增 `NpcDefinition.NpcSlotCost`、`NpcAuthorityComponent.NpcSlotCost` 和
  `NpcSlotAccountingSystem`，由 spawn commit 将 definition 值写入权威 owner；
  `DomeSimulation` 的 tile-entity NPC spawn-cap 检查改用 active weighted sum。
- 只建立有限值、确定性 weighted accounting primitive，并让现有 tile-entity spawn-cap 使用该
  primitive；完整 `NPC.Spawner`、玩家距离窗口、
  invasion 类型表、slime-rain multiplier、spawn-rate 曲线和所有难度缩放仍保持
  `partial/deferred`。

### Batch N3.84：`npcSlots` weighted candidate eligibility budget（已完成窄切片）

- `NpcSpawnSnapshot` 继续兼容原有整数 `activeNpcCount` 调用，同时提供可选的
  `ActiveNpcSlots` weighted budget；`NpcSpawnCandidate.NpcSlotCost` 作为候选的 typed
  authority input，默认 `1.0f`。
- `NpcSpawnEligibilitySystem` 现在按候选 slot cost 累加并在超过最大预算时拒绝后续候选；
  非 finite/负值候选 fail-closed。旧整数调用仍保持一候选一 slot 的行为。
- 本批只扩展通用资格预算，不实现玩家局部 `nearbyActiveNPCs` 距离刷新、slime-rain
  multiplier、动态 max-spawn 曲线、invasion 表或完整 legacy candidate generation；这些
  继续 `partial/deferred`。

### Batch N3.117：`dontTakeDamageFromHostiles` hostile-NPC damage immunity（已完成窄切片）

- 原始 `Terraria/NPC.cs:6143` 声明 `dontTakeDamageFromHostiles`，初始化路径
  `NPC.cs:8206` 将其重置为 `false`；`NPC.cs:78542-78549` 在
  `GetHurtByOtherNPCs` 的 NPC-to-NPC 伤害入口最早返回，且调用点
  `NPC.cs:76820-76832` 与玩家/projectile 伤害路径分离。
- `SpawnNpcCommand.DoesNotTakeDamageFromHostiles` 经 `NpcSpawnCommitSystem` 写入
  `NpcBehaviorStateComponent.DoesNotTakeDamageFromHostiles`。`NpcStateSnapshot` 直接保存该
  typed behavior component，因此内存 persistence round-trip 不丢失值。
- `DamageNpcCommand` 增加显式 `NpcDamageSourceKind` 和 `SourceNpc`；
  `QueueHostileNpcDamage` 只构造 `HostileNpc` 命令，`CommitNpcDamageCommands` 在 authoritative
  settlement 重新校验 source handle、source active 状态和 `NpcFaction.Hostile`，再读取 target
  behavior owner。命中 immunity gate 时跳过伤害；普通 `QueueNpcDamage` 仍走 external 路径，
  不受该 gate 影响。未定义 source kind、缺失 source 或 stale source 均 fail-closed。
- 本批不声称迁移完整 `GetHurtByOtherNPCs`：rectangle scan、`NPCID.Sets` type sets、
  bee-specific branch、hit effects/immunity timers、AI transition（例如
  `NPC.cs:41460-41462`、`41566-41568`）以及 NPC-to-NPC combat family 继续
  `partial/deferred`；网络 replication 中的该 bool 也保持 deferred。
- 验收证据：`Build/diagnostics/npc-complete/task-10-interaction/20260830-hostile-damage/`
  中的 Simulation/NPC verifier/Combat verifier/Server serial build、verifier logs 和
  `diff-check.log`、`checkpoint-json-parse.log` 与 `summary.md`；覆盖 hostile source 被阻断、
  external damage 仍生效、town/stale/undefined source fail-closed，以及 behavior state
  persistence round-trip。

### Batch N3.118：`CanBeReplacedByOtherNPCs` slot allocator（已完成窄切片）

- 原始 `Terraria/NPC.cs:6297` 声明 `spawnSlotProtected`，
  `NPC.cs:67041-67048` 按 active 状态刷新保护倒计时；`NPC.cs:67051-67095` 在
  `NewNPC` 成功时覆盖数组槽位并重新初始化实例。`NPC.cs:67098-67130` 的
  `GetAvailableNPCSlot` 先寻找未占用槽位，再寻找
  `CanBeReplacedByOtherNPCs` 槽位；`NPC.cs:8158` 默认把该标志重置为 `false`，
  只有 `NPC.cs:29544`、`50878`、`66485` 的显式分支写入 `true`。
- `NpcSlotAllocator` 将 slot 选择收敛为确定性 typed policy：按 `1..MaximumNpcs`
  升序扫描，优先复用 inactive replication/tombstone，其次才选择
  `NpcLifecycleComponent.CanBeReplaced == true` 的 active owner；revision 为
  `long.MaxValue`、重复/越界 handle、负 revision 或无可用槽位时 fail-closed。
  slot `0`、reverse/start-index、type-specific slot 规则没有被伪造为已迁移语义。
- `DomeSimulation.CreateNpc` 与 queued `CommitNpcSpawnCommands` 共用
  `TryResolveNpcSlot`/`TryCommitNpcSpawn`，消除了 direct/queued 两套 allocator 语义。
  新实体先通过 definition、有限坐标、难度生命和占用校验，再替换旧 Arch entity；失败
  时旧槽位保持不变。显式 `RequestedReplicationId` 仍在碰撞或容量不满足时拒绝，不绕过
  server-authoritative identity gate。
- 复用 stable `NpcHandle` 时保留 `_npcReplications` 的 identity，revision 严格递增，
  同步新实体的 `NpcReplicationComponent.Revision`，并清除 `_publishedNpcDeaths` 中的旧
  death 标记，使 combat/PVS cursor 能观察到新的 active snapshot；`NpcCount` 不因槽位
  复用增长。Training Dummy ownership 在 handle 映射变更前通过
  `ClearTrainingDummyOwnershipForNpc` 清理，queued tile-entity spawn 仍在成功提交后建立
  新 link。
- TDD RED 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-slot-replacement-red/`：
  verifier build exit `0`，verifier run 以
  `InvalidOperationException: The configured NPC limit has been reached.` 失败，命中
  inactive-slot 缺失行为。GREEN 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-slot-replacement/`：
  Simulation/NPC verifier/Combat verifier/Server 均串行构建成功，NPC 与 Combat verifier
  run exit `0`。
- `spawnSlotProtected` 倒计时、完整 `NPC.Spawner` 候选生成、reverse/start-index 搜索、
  type-specific slot-0 规则、network byte parity，以及 projectile ownership、AI/event
  cleanup、客户端表现等替换副作用继续 `partial/deferred`；本批不声称完整 NPC lifecycle
  或 `NPC.cs` parity。

### Batch N3.119：`homelessDespawn` lifecycle owner（已完成窄切片）

- 原始 `Terraria/NPC.cs:6403-6407` 声明 `homeless`/`homelessDespawn`，
  `NPC.cs:8220` 在 `SetDefaults` 中将 `homelessDespawn` 重置为 `false`；
  `Terraria/WorldGen.cs:4828-4832` 在无家 town NPC 生成后写入 `true`，
  `WorldGen.cs:4853-4889` 读取它进入 `UnspawnHomelessNPC` 候选，
  `WorldGen.cs:5105-5108`、`5407-5410` 在取得住房后清除。
- `NpcLifecycleComponent.HomelessDespawn` 现在是 typed lifecycle owner，默认值为
  `false`，并提供显式 `MarkHomelessDespawn`/`ClearHomelessDespawn` 操作；
  `SpawnNpcCommand.HomelessDespawn` 经 `NpcSpawnCommitSystem` 原样写入，不从
  `NpcSpawnSource` 或 definition 猜测。
- focused NPC verifier 覆盖默认/显式 spawn、mark/clear 操作，以及
  `CreateNpcStateSnapshots` 的内存 state round-trip；该字段不加入当前
  `NpcReplicationSnapshot`，避免把 WorldGen 内部生命周期 scratch 伪装成网络契约。
- TDD RED 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-homeless-despawn-red/verifier-build-red.log`，
  verifier build 因缺少 `SpawnNpcCommand.HomelessDespawn` 与
  `NpcLifecycleComponent.HomelessDespawn` 失败；GREEN 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-homeless-despawn/`，
  包括 `verifier-rebuild.log`、`verifier.log`、`simulation-rebuild.log`、
  `server-rebuild.log` 和 `diff-check.log`。
- 本批不实现 `WorldGen.UnspawnHomelessNPC` 的玩家安全区/矩形计算、候选扫描与
  active/life/net message 提交，不接入住房搬迁或 world-save/network byte parity；
  完整 `homeless`/town 服务、`CheckActive`、Spawner 及 NPC lifecycle parity 继续
  `partial/deferred`。

### Batch N3.120：`lookForHomeTimeout` typed owner and zero-ready boundary（已完成窄切片）

- 原始 `Terraria/NPC.cs:6407` 声明 `lookForHomeTimeout`，`NPC.cs:8121` 在
  `SetDefaults` 中重置为 `0`；`NPC.cs:76728-76731` 对 active NPC 的正值每 tick
  递减并保持不低于 `0`。`Terraria/NPC.cs:6409` 将
  `KickOutLookForHomeTimeout` 定义为 `3600`。
- `Terraria/WorldGen.cs:4562-4565` 的 `moveRoom` 写入 `homeless = true` 与零超时，
  `WorldGen.cs:4573-4575` 的 `kickOut` 写入 `homeless = true` 与 `3600`；
  `WorldGen.cs:5273`、`59537` 的住房候选读取要求 `homeless` 且
  `lookForHomeTimeout == 0`。
- `NpcHomeComponent.LookForHomeTimeout` 追加在既有 positional constructor 参数末尾，
  拒绝负值并默认为零；`MarkKickedOut`、`MarkMovedRoom` 分别承载两个 source-backed
  写入状态，`IsReadyToLookForHome` 只表达 `IsHomeless && LookForHomeTimeout == 0`。
  `NpcHomeTimeoutSystem` 只递减正值，负值 fail-closed；不把 active、town、type 或房间
  几何条件推断进该组件。
- 现有 authoritative NPC lifecycle stage 仅为仍 active、health 正值且拥有
  `NpcHomeComponent` 的实体调用 timeout system。没有 Home component 的 NPC 不会被补出
  住房状态，inactive/despawned NPC 不再递减；`NpcStateSnapshot` 已有 Home component
  sidecar，内部 timeout 不加入当前网络或旧 `NpcHomeSnapshot` 字节契约。
- focused NPC verifier 覆盖默认零值、构造/运行时负值拒绝、kick-out `3600`、move-room
  零值、单 tick 递减、零值 readiness、无家状态门禁、无 Home NPC 隔离和 inactive lifecycle
  门禁；运行时验证输出为：
  `PASS: NPC home-search timeout owner, zero-ready boundary and active lifecycle tick`。
- TDD RED 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-home-search-timeout-red-rerun/verifier-build-red.log`，
  verifier build 以退出码 `1` 失败且错误集中在缺失的新 typed API；GREEN 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-home-search-timeout-green/`，
  其中 `simulation-build.log`、`verifier-build.log`、`verifier.log`、
  `server-build.log` 均退出码 `0`。
- 本批不实现 TownManager/住房扫描、玩家安全区、房间评分、relocation/unspawn commit、
  active/life/net message 完整提交、world-save/network byte parity、完整 `CheckActive`、
  NPC Spawner 或 town-service/AI family parity；这些继续 `partial/deferred`。

### Batch N3.121：旧 home 发布基线 owner（已完成窄切片）

- 原始 `Terraria/NPC.cs:6417-6421` 的 `oldHomeless`、`oldHomeTileX`、`oldHomeTileY` 是
  home 状态的旧发布基线；`NPC.cs:43033-43045` 的 `UpdateHomeTileState` 先比较当前
  `homeless/homeTileX/homeTileY`，再同时写入当前值和旧基线；`NPC.cs:76956-76965` 的
  active town NPC 网络阶段则比较基线，发送 `NpcHome` 后推进它。
- `NpcHomePublicationComponent` 现在独立承载 `LastPublishedHomeless`、
  `LastPublishedHomeTileX`、`LastPublishedHomeTileY`，默认 tuple 保持旧实现的
  `false,-1,-1`；`HasPendingPublication` 只比较三元 home tuple，
  `CapturePublishedState` 只复制已确认的当前值。
- `NpcHomePublicationSystem.ApplyHomeTileState` 保留旧方法的 compare-before-write 顺序，
  返回当前值是否变化并捕获新基线；`CapturePublishedState` 可在未来网络发送闸门通过后
  清除 pending 状态，但本批不发送任何帧。
- `NpcStateSnapshot` 现在可选携带 `HasHomePublication/HomePublication`，
  `DomeSimulation.CreateNpcStateSnapshots` 与 restore 会保留显式挂载的 sidecar；没有
  `NpcHomeComponent` 的实体不会推断出 Home 或 publication。该 sidecar 不进入
  `NpcReplicationSnapshot`、`NpcHomeSnapshot` 或 SyncNPC wire shape。
- focused NPC verifier 覆盖默认基线、三元组变化、compare-before-write、显式捕获和
  authoritative state snapshot round-trip；输出为：
  `PASS: NPC home publication baseline owner and authoritative state round-trip`。
  RED build 先以退出码 `1` 命中新 API 缺失，GREEN verifier build/run 均退出码 `0`。
- 本批不实现 TownManager household status、active/town/head eligibility、room 扫描与评分、
  网络发送调度、world-save byte parity、relocation/unspawn commit、完整 `CheckActive`、
  NPC Spawner 或 town-service/AI family parity；总体迁移保持 `partial/deferred`。

### Batch N3.122：`trapImmune` projectile eligibility owner（已完成窄切片）

- 原始 `Terraria/NPC.cs:6335` 声明 `trapImmune`，`NPC.cs:8251` 在 `SetDefaults` 中将其重置为
  `false`，type `662` 的 source-backed definition 分支在 `NPC.cs:17089` 显式开启；
  `Terraria/Projectile.cs:11626` 在 `Damage_PVE_Inner` 中以 `targetNPC.trapImmune && trap`
  拒绝陷阱 projectile。
- `NpcDefinition.IsTrapImmune` 是显式 definition capability，`NpcAuthorityComponent.IsTrapImmune`
  是 immutable authority owner，`NpcSpawnCommitSystem` 只从注册 definition 输入复制该值，不从
  DefinitionId 猜测。`ProjectileTargetEligibilitySystem.CanTargetNpc` 仅在 projectile 的
  `IsTrap` 与 authority capability 同时为真时拒绝，普通 projectile 与普通 NPC 保持原路径。
- Combat verifier 覆盖 type-662 形状的显式 definition、spawn commit owner、trap/非 trap 三种
  组合；输出为 `PASS: NPC trap immunity owner gates trap projectile eligibility`。
- 本批不实现完整 NPC type/default table、动态 AI 写入、陷阱碰撞或伤害 family、
  `CanReflectProjectile`/反射副作用、网络/存档字节 parity 或完整 NPC/Projectile parity；
  总体迁移保持 `partial/deferred`。

### Batch N3.123：`lavaImmune` lava-contact damage owner（已完成窄切片）

- 原始 `Terraria/NPC.cs:6381` 声明 `lavaImmune`，`NPC.cs:8216` 在 `SetDefaults` 中重置为
  `false`；静态默认表存在 `49` 个显式 `lavaImmune = true` 写入（代表性位置为
  `NPC.cs:8795,9233,9257,9285,9342,17088`），而 `NPC.cs:50723` 的动态 AI 写入仍因完整
  AI family 未迁移而 deferred。`NPC.cs:79305-79333` 的 `Collision_LavaCollision` 以该能力、
  `dontTakeDamage` 和 `immune[255]` 共同决定是否进入伤害分支。
- 当前 ECS 只建立可证明的 owner/contact 链：`NpcDefinition.IsLavaImmune` 是显式 definition
  capability，`NpcAuthorityComponent.IsLavaImmune` 是 authoritative owner；
  `NpcSpawnCommitSystem` 与注册 definition restore 都复制该值，不从 `DefinitionId` 猜测。
  `NpcLavaContactSystem` 仅对 active、存活、可受伤、未免疫且有限几何与 lava 液体 tile 重叠的
  NPC 生成 `DamageNpcCommand(SourceKind = Lava, Amount = 50)`；settlement 再校验 source
  identity、source handle 与 lava immunity，普通 NPC 通过 source-backed 30-tick immunity
  接受该有界伤害。
- focused NPC verifier 覆盖 source-default false、显式 true、spawn/restore owner、lava-only
  overlap、water/非有限几何/世界外坐标 fail-closed、普通 NPC 实际受伤及伪造 Lava command
  被拒绝；NPC protocol verifier 证明 snapshot/`SyncNPC` wire shape 未改变。
- TDD RED 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-lava-immunity-red/verifier-build-red.log`，
  退出码 `1` 且命中 9 个预期 missing-API 错误。Fresh GREEN/回归证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-lava-immunity-final-green-rerun/`，
  其中 Simulation/NPC/Combat/Server/NPC protocol build 与 verifier、`git diff --check`、
  checkpoint JSON parse 均退出码 `0`；完整日志和边界说明见同目录 `summary.md`。
- 本批不实现完整 `Collision_LavaCollision` 矩形/边界与特殊 type 语义、`lavaWet`、buff 24
  duration/remix、hit FX/声音/网络消息、全部静态 type/default 表、动态 AI 写入、完整液体/NPC
  physics、persistence/wire byte parity 或完整 NPC/AI parity；这些继续 `partial/deferred`，
  `canRemoveLegacyWorldGen` 仍为 `false`。

### Batch N3.124：`reflectsProjectiles` dynamic-state owner（已完成窄切片）

- 原始 `Terraria/NPC.cs:6439` 声明 `reflectsProjectiles`，`NPC.cs:8257` 在 `SetDefaults` 中将其
  重置为 `false`；`NPC.cs:18888,19128,20762,20966,25400,25632,26015,26247,26551,
  31163,31167,38170,38299,46736,46790` 展示了 AI 分支中的动态清除/设置，未发现可直接
  搬入 definition 的静态 capability 表。
- `Terraria/Projectile.cs:11758-11767` 的命中前分支以目标 NPC 的该状态和
  `CanBeReflected()` 共同决定是否进入反射；`Projectile.cs:18036-18050` 证明
  `active`、`friendly`、`!hostile`、正数运行时 damage 以及 type `728/955` 或 legacy AI style
  `1,2,8,21,24,28,29,131` 是 projectile 资格的可验证部分。
- `NpcBehaviorStateComponent.ReflectsProjectiles` 现在承载可被 AI 更新的实例状态，默认保持
  `false`；`ProjectileReflectionEligibilityPolicy.CanBeReflectedByNpc` 读取该 typed owner，
  并以运行时 damage、activity、friendly/hostile 和 source-backed type/style 白名单做
  fail-closed 资格判断。没有把它误归类为 `NpcDefinition`/immutable authority capability。
- focused Combat verifier 新增 `PASS: NPC reflects-projectiles behavior owner and projectile
  eligibility policy`，覆盖默认关闭、显式开启、type/style 白名单和 activity/flags/damage/
  unsupported fail-closed；Simulation/Combat/Server clean build 与 Combat verifier 均退出码
  `0`，NPC protocol verifier 仍输出 `PASS: NPC state snapshot round-trip and SyncNPC
  projection/codec fixtures`，证明没有新增 wire 字段。
- TDD RED 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-reflect-projectiles-red/`，
  首次 verifier build 退出码 `1` 且仅命中缺失的 typed state/policy API；GREEN 与串行 gate
  证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-reflect-projectiles-green/`。
- 本批不实现 `NPC.CanReflectProjectile` 的完整 hitbox/type-618 mouth 语义、
  `ReflectProjectile` 的随机速度与 `Main.player` 依赖、damage/penetration 变更、sound/dust、
  `netUpdate`/replication/persistence、完整 NPC projectile-hit settlement、全部动态 AI 写入
  或 AI family parity；这些继续 `partial/deferred`，`canRemoveLegacyWorldGen` 仍为 `false`。

### Batch N3.125：inactivity-preservation lifecycle owner（已完成窄切片）

- Legacy `Terraria/NPC.cs:64323-64336`、`64397-64435` 的两个读取现在由
  `LegacyNpcInactivityRegistry` 承载：61 个无条件类型保持 exact set；type `139` 只在显式
  观察到 active type `134` 时保留；`552..563` 与 `566..578` 只在显式观察到 active type
  `548` 时保留，而 `564/565` 位于这两个条件区间之间，仍是无条件类型。
- `NPC.cs:64323-64328` 的 type `668` 分支单独由
  `DoesNotDespawnToInactivityAndCountsNpcSlots` 承载，不能与普通 inactivity 集合混为同一
  规则。Registry 接受 NPC type 和调用方提供的只读 active-type 集合，验证 `0..696` 域并
  拒绝无效输入/null context；不从静态 Definition 猜测 companion，也不新增 NPC/protocol
  字段。
- `NPC.cs:7129-7142` 的 per-tick active observation 在
  `DomeSimulation.CreateActiveNpcTypes` 中对应为当前 active
  `NpcDefinitionComponent.NetId` 集合；两个既有 lifecycle 入口把 Registry 结果传入
  `NpcLifecycleSystem`。gate 只阻止正生命 NPC 的 inactivity timer 递减，lethal 与 immortal
  顺序保持原有语义。
- `Terraria.Dome.Npc.Verification` 覆盖 exact unconditional set、两组 companion gate、
  `564/565` gap、type `668`、无效边界、null context 及 lifecycle no-decrement/kill/immortal
  分支。TDD RED、GREEN、Simulation/Server 串行 build、NPC protocol shape、diff-check 与
  checkpoint parse 的新鲜证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-inactivity-lifecycle-green/`；
  source audit 见 `docs/research/2026-08-30-npc-inactivity-lifecycle-boundary.md`。
- 受影响范围仍只是一条可重放的生命周期读取策略和 timer gate；完整 `CheckActive` 的
  player rectangle、`Main`/AI/town/boss/event 分支、`npcsFoundForCheckActive` 调度、worm
  cleanup、slot 计数生命周期、网络/持久化投影和旧源码删除继续 `partial/deferred`，
  `canRemoveLegacyWorldGen` 仍为 `false`。

### Batch N3.126：static `townNPC` inactivity lifecycle owner（已完成窄切片）

- Legacy `Terraria/NPC.cs:6397` 声明 `townNPC`，`NPC.cs:8218` 在 `SetDefaults` 中将其重置为
  `false`；全文件只发现 32 个静态分支，展开后为 39 个有效 `townNPC = true` net-id：
  `17,18,19,20,22,37,38,54,107,108,124,142,160,178,207,208,209,227,228,229,353,368,369,441,550,588,633,637,638,656,663,670,678,679,680,681,682,683,684`。
- `NPC.cs:64442` 在完整 `CheckActive` 的首个 guard 中直接跳过 `townNPC` 的 inactivity
  处理。`LegacyNpcTownRegistry` 现在持有 exact set 并验证 `0..696` 域；不从
  `NpcFaction`、`NpcCategory`、`IsLikeTownNpc` 或动态 `townNPCCanSpawn` 候选表推断。
- `DomeSimulation` 将 `NpcDefinitionComponent.NetId` 的纯查询结果传入
  `NpcLifecycleSystem`。gate 只跳过正生命城镇 NPC 的 `TimeLeft` 递减；致死和 immortal
  顺序保持不变，且没有新增实体、快照、网络或存档字段。
- `Terraria.Dome.Npc.Verification` 覆盖 39 个有效 source ID（32 个分支）、type `453`/`690` 差异、域边界和
  lifecycle ordering；TDD RED、修正后的 GREEN verifier、Simulation/Server/NPC protocol builds、
  source exact-set audit 与 `git diff --check` 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-town-inactivity-lifecycle-{red,green}/`。
  修正后的权威日志为 `source-audit-corrected.log`、`verifier-build-corrected.log`、
  `verifier-corrected.log`、`simulation-build-corrected.log`、`server-build-corrected.log`、
  `npc-protocol-build-corrected.log` 和 `npc-protocol-corrected.log`；旧的未展开分支计数
  `source-audit.log` 仅保留作历史记录。
- 完整 `CheckActive` 的玩家矩形、AI/event/boss、type `690` AI、noSpawn/revenge、碰撞/恢复、
  住房服务、WorldGen unspawn、全量 static definitions 和 legacy 删除仍为
  `partial/deferred`，`canRemoveLegacyWorldGen` 仍为 `false`。

### Batch N3.127：type-690 `ai[0]` inactivity guard audit（deferred）

- `NPC.cs:64442` 的下一条生命周期读取是 `type == 690 && ai[0] == 0f`。`SetDefaults`
  (`NPC.cs:17388-17403`) 将 type 690 配置为 `aiStyle = 126`、`immortal = true`、
  `dontTakeDamage = true`，因此该 guard 依赖 mutable AI state，而非静态 type-only capability。
- 当前 `NpcLifecycleSystem` 已有显式 immortal 优先级，可覆盖广义 no-decrement 效果；但当前
  Simulation 没有完整 type-690 definition、AI-126 behavior 或 generic `ai[0]` owner，不能以
  immortal 分支宣称 type-690 parity。
- 本次只读审计不增加 AI slot、definition、snapshot、protocol 或 persistence 字段；未来只有
  在 AI-126 owner 建立后才重新评估。证据见
  `docs/research/2026-08-30-npc-type690-inactivity-boundary.md` 与
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-type690-inactivity-audit/type690-audit.log`。

### Batch N3.128：`CheckActive` pixel-range geometry owner（已完成窄切片）

- 新增 `NpcPixelBounds`、`NpcPixelRectangle`、`NpcActivityRangeDefinition`、
  `NpcPlayerActivity`、`NpcActivityRangeDecision` 和 `NpcActivityRangePolicy`，只承载
  `NPC.CheckActive` 的 pixel-space 几何输入与纯 decision。`Version1456` 固定 source-backed
  `activeRangeX = 4032`、`activeRangeY = 2520`、`sWidth = 1920`、`sHeight = 1200`。
- `NpcActivityRangePolicy.Evaluate` 按 legacy cast 顺序构造 active/screen 两个矩形，跳过
  inactive player，保留严格（边界接触不算）相交，并分别返回
  `HasActivePlayerInRange` 与 `ShouldRefreshInactivityTimer`。坐标/尺寸做有限值和正数校验，
  相交边界使用 `long` 中间值避免整数溢出；policy 不写 timer、不改 NPC/Player 状态，也不
  修改调用方集合。
- 该 owner 没有接入 `TransformComponent` 或 `WorldGrid`，因为当前 Simulation 仍缺少通用
  pixel-to-tile contract；也没有创建 `Main.player[0..254]` 镜像、`nearbyActiveNPCs`、
  `npcSlots`/slime-rain、boss/AI/type exceptions 或任何 deactivation side effect owner。
  因此完整 `CheckActive`、timer refresh integration、slot accounting、collision/recovery、
  unspawn、网络/持久化与完整 NPC/AI parity 继续 `partial/deferred`。
- TDD RED 首次 verifier build 退出码 `1`，仅报告上述缺失类型；GREEN 的 NPC verifier、
  Simulation Release 与 Server Release 串行构建/运行均退出码 `0`，证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-pixel-range-{red,green}/`。
  研究边界见 [`2026-08-30-npc-checkactive-pixel-range-owner.md`](../research/2026-08-30-npc-checkactive-pixel-range-owner.md)。
- 本批建立的是几何 input/query contract，不是完整生命周期迁移；`canRemoveLegacyWorldGen`
  仍为 `false`，总体结论仍为 **NPC field/property migration: partial**。

### Batch N3.129：`CheckActive` nearby-slot contribution owner（已完成窄切片）

- 新增 `NpcActivitySlotContributionInput`、`NpcActivitySlotContribution` 和
  `NpcActivitySlotContributionPolicy`，只承载 `NPC.CheckActive` 在活动玩家相交后对
  `nearbyActiveNPCs` 的资格与权重计算。输入显式携带 NPC active 状态、type、`lifeMax`、
  `releaseOwner`、`npcSlots` 映射值、Slime Rain 状态和已计算的 active-range 结果。
- policy 保留 source-backed 顺序：排除 type `25`、`30`、`33`；要求
  `releaseOwner == 255` 且 `lifeMax > 0`；仅在 Slime Rain 开启且
  `LegacySlimeRainNpcRegistry` 命中 type `1` 时应用 `0.65f`，其它类型保留基础权重。
  net-id、release-owner 和 slot cost 做边界/finite 校验，policy 不修改输入或任何玩家状态。
- 本批不创建 `Main.player[0..254]` 镜像、`nearbyActiveNPCs` 可写字段、全局/玩家 slot
  counter，也不接入 `PlayerStore`、`DomeSimulation`、`NpcLifecycleSystem`、timer refresh、
  deactivation、protocol 或 Slime Rain event。pixel-range owner 仍由独立的
  `NpcActivityRangePolicy` 提供，避免混入坐标单位转换。
- TDD RED verifier build 退出码 `1`，GREEN focused NPC verifier、Simulation、Protocol 和
  Server Release 串行构建/运行均退出码 `0`；最终复跑的五条命令也全部为 `0`，证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-slot-contribution-{red,green,final}/`。
  研究边界见
  [`2026-08-30-npc-checkactive-slot-contribution-owner.md`](../research/2026-08-30-npc-checkactive-slot-contribution-owner.md)。
- 本批建立的是 per-player contribution query contract，不是完整 `CheckActive` 或 Slime
  Rain 生命周期迁移；`canRemoveLegacyWorldGen` 仍为 `false`，总体结论仍为
  **NPC field/property migration: partial**。

### Batch N3.130：`npcSlots` invasion-boss cap typed owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:159-183` 的
  `NPC.Spawner` 构造逻辑。legacy 只统计 active NPC，并且只接受 exact 七个类型
  `315, 325, 327, 328, 344, 345, 346` 的 `npcSlots`；`defaultMaxSpawns` 默认是 `5`。
- 本批保留 source 的单精度阈值公式：
  `(int)(defaultMaxSpawns * (2f + 0.3f * activePlayerCount))`，全局上限为
  `activePlayerCount * perPlayerLimit`，weighted active total 达到或超过上限时
  `ReachedInvasionBossCap` 为 `true`。类型表由
  `LegacyNpcInvasionBossRegistry` 独立持有；无效类型、非 finite/负 slot cost、玩家数和
  默认上限越界均 fail-closed。
- `NpcInvasionBossSlotAccount` 与 `NpcInvasionBossCapDecision` 是 typed 输入/输出，
  `NpcInvasionBossCapPolicy` 负责纯计算；`NpcInvasionSpawnState` 承载 cap 状态，
  `NpcSpawnEligibilitySystem` 在 invasion candidate 分支拒绝已达到上限的生成请求。
  该链没有把 cap 误接到未经 source 证明的全局 `NpcEventSpawnSystem`。
- TDD RED 构建退出码 `1`，只报告 owner 尚不存在的 16 个类型/成员错误；GREEN focused
  NPC verifier 构建退出码 `0`（0 warning、0 error），运行退出码 `0`（17 PASS、0
  FAIL/ERROR）。证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-invasion-boss-cap-{red,green}/`，
  source audit 为 `20260830-invasion-boss-cap-green/source-audit.log`。
- `NPC.Spawner` 全量生成选择、event spawn、玩家局部 `nearbyActiveNPCs`、invasion table、
  动态 spawn-rate 曲线、网络/持久化投影和完整 NPC parity 仍为 `deferred`；legacy
  `NPC.cs` 未修改，`canRemoveLegacyWorldGen` 仍为 `false`，总体迁移仍为
  **NPC field/property migration: partial**。

### Batch N3.131：`CheckActive` active-player keep-alive typed owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64450-64523`，source
  SHA-256 为 `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`。本批只
  承载 active-player loop 内的 keep-alive 分支：Boss 和 exact static set
  `7,10,13,35,36,39,87,127,128,129,130,131,392,393,394,491,492` 会保留 NPC；type `399`
  始终保留但仅在 `ai[0] == 1f || ai[0] == 2f` 时刷新 `timeLeft`；types `583..585` 仅在
  夜间且 `ai[2] == 0f` 时保留并刷新。所有这些分支都受 active player loop gating，没
  有 active player 时不会生效。
- 新增 `LegacyNpcCheckActiveKeepAliveRegistry`、`NpcCheckActiveKeepAliveInput`、
  `NpcCheckActiveKeepAliveDecision` 和 `NpcCheckActiveKeepAlivePolicy`。registry 固定
  `NpcTypeCount = 697` 与 17 个 source 类型；policy 是 pure query，独立计算 keep-active
  和 timer-refresh，不从 faction/category 推断，不引入通用 `float[] ai`，并对 net-id 与
  `ai[0]/ai[2]` finite 值做 fail-closed 校验。
- TDD RED focused verifier build 退出码 `1`，只报告新 owner 缺失；GREEN 的 Simulation、
  Protocol、NPC verifier 和 Server Release 串行构建均为 exit `0`、0 warning、0 error，
  focused verifier 运行 exit `0`，包含 active-player、inactive NPC、Boss/static、399、
  583..585、重叠分支及 NaN/Infinity/type 边界。最终 fresh 复跑同样全部通过；证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-keepalive-{red,green,final}/`，
  source audit 为 `20260830-checkactive-keepalive-green/source-audit.log`；研究边界见
  [`2026-08-30-npc-checkactive-keepalive-owner.md`](../research/2026-08-30-npc-checkactive-keepalive-owner.md)。
- 本批不接入 `DomeSimulation`、`NpcLifecycleSystem`、timer/deactivation、player-slot mirror、
  `nearbyActiveNPCs`、generic AI、type-690 guard、protocol 或 persistence；完整 `CheckActive`、
  AI/event/boss parity、网络/存档和 legacy 删除继续 `partial/deferred`。central WorldGen
  active batch 不变，`canRemoveLegacyWorldGen` 仍为 `false`，总体结论仍为
  **NPC field/property migration: partial**。

### Batch N3.132：`CheckActive` screen-range timer-refresh typed owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64476-64480`，source
  SHA-256 仍为 `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`。在前置
  active-player loop 已获得 screen-range 命中后，legacy 对非
  `DoesntDespawnToInactivityAndCountsNPCSlots` 分支把 `timeLeft` 重置为 `activeTime`，并清除
  `despawnEncouraged`；type `668` 的 slot-counting 分支跳过这次 refresh。
- 新增 `NpcCheckActiveTimerRefreshInput`、`NpcCheckActiveTimerRefreshDecision` 和
  `NpcCheckActiveTimerRefreshPolicy`。policy 保留 active NPC gate、screen-range hit gate、
  type-668 bypass、`Version1456ActiveTime = 750` 和 reset 输出；无命中时原样返回输入的
  `TimeLeft`/`DespawnEncouraged`，不直接写 `NpcLifecycleComponent` 或玩家集合。
- TDD RED verifier build 以预期的 15 个缺失 owner 符号退出 `1`；随后 Simulation、Protocol、
  NPC verifier 和 Server Release 串行构建均 exit `0` 且 0 warning/0 error，focused verifier
  包含 refresh、无玩家、inactive、type-668、相等阈值、纯函数和非法 active-time 边界。
  证据位于 `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-timer-refresh-{red,green,final}/`，
  source boundary 见
  [`2026-08-30-npc-checkactive-timer-refresh-owner.md`](../research/2026-08-30-npc-checkactive-timer-refresh-owner.md)。
  在 concurrent N6 批次推进后，fresh rerun 仍通过，证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-timer-refresh-final-rerun-01/`。
- 本批不接入 `DomeSimulation`/`NpcLifecycleSystem`，不新增 fixed player-slot mirror、
  `nearbyActiveNPCs`、generic AI、type-690/type-boss/event 分支、`noSpawnCycle`、`active`/`life`、
  SyncNPC、revenge、worm cleanup、protocol 或 persistence。完整 `CheckActive`、timer/deactivation
  integration、网络/存档、NPC parity 和 legacy 删除继续 `partial/deferred`；central Flowstate
  active-batch state is preserved outside this slice and `canRemoveLegacyWorldGen=false` remains,
  整体仍为 **NPC field/property migration: partial**。

### Batch N3.133：`CheckActive` timeout deactivation and spawn-cycle gate typed owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64529-64545` 与
  `NPC.cs:66513-66519`，source SHA-256 仍为
  `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`。legacy 在
  `CheckActive` player-loop 之后先递减 `timeLeft`；当没有 keep-alive 或递减后 timer 非正时，
  设置静态 `noSpawnCycle=true`、`active=false`、`life=0`。下一次 `SpawnNPC` 在
  `RevengeManager.CheckRespawns()` 和 `Spawner.SpawnNPC()` 之前只消费这个 marker 一次并返回；
  `checkDead` 在 `NPC.cs:64670` 的另一处写入不属于本批。
- 新增 `NpcCheckActiveDeactivationInput`、`NpcCheckActiveDeactivationDecision`、
  `NpcCheckActiveDeactivationPolicy` 与 `NpcSpawnCycleStateComponent`。policy 消费已拆出的
  keep-alive/slot-counting 结果，保留一次 decrement、timer<=0 边界、active/life 输出和
  skip-next-cycle intent；inactive 与 type-668 slot-counting 输入原样返回。spawn-cycle
  component 以显式 mutable world gate 提供 mark/consume-once 语义，不把 static source flag
  伪装成 NPC entity 字段。
- TDD RED verifier build 以预期的 14 个缺失 owner 符号退出 `1`；GREEN 的 Simulation、
  Protocol、NPC verifier 和 Server Release 串行构建均 exit `0` 且 0 warning/0 error，focused
  verifier run exit `0`，包含 timeout、keep-alive、timer-zero、slot-counting、inactive、
  invalid lifecycle input、纯函数和 consume-once gate。source/hash/style/diff 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-deactivation-{red,green,final}/`；
  source boundary 见
  [`2026-08-30-npc-checkactive-deactivation-owner.md`](../research/2026-08-30-npc-checkactive-deactivation-owner.md)。
- 本批不接入 `DomeSimulation`、`NpcLifecycleSystem` 或 spawn pipeline，不新增 fixed
  player-slot mirror、`nearbyActiveNPCs`、SyncNPC、`RevengeManager.CacheEnemy`、worm cleanup、
  generic AI/type/event/boss 分支、protocol/persistence；完整 `CheckActive`、spawn/deactivation
  integration、网络/存档、NPC parity 和 legacy 删除继续 `partial/deferred`，
  `canRemoveLegacyWorldGen` 仍为 `false`，总体结论仍为 **NPC field/property migration: partial**。

### Batch N3.134：`checkDead` entry qualification typed owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64575-64577`，source SHA-256
  为 `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`。legacy 的入口
  guard 按固定顺序拒绝 inactive NPC、非 root worm segment（`realLife >= 0 && realLife != whoAmI`）
  和 `life > 0`；因此只有 active root 且 `life <= 0` 才进入后续 `checkDead` 分支。
- 新增 `NpcCheckDeadQualificationInput`、`NpcCheckDeadQualificationDecision`、
  `NpcCheckDeadQualificationRejectionReason` 和 `NpcCheckDeadQualificationPolicy`。policy
  是无副作用的 typed pure owner，保留 guard 顺序并区分 `Inactive`、`NonRootSegment`、
  `PositiveLife`；它不写 `NpcLifecycleComponent`、`NpcSegmentComponent`、`NpcSpawnCycleStateComponent`
  或任何事件/掉落/网络状态。
- TDD RED 的标准 NPC verifier build 因 19 个缺失 owner 符号退出 `1`；随后 focused source
  harness build/run 均 exit `0`（0 warning/0 error，`PASS: NPC checkDead entry qualification policy`）。
  在当前源码树上，Simulation 与 NPC verifier 的 fresh serial `Rebuild` 均 exit `0`，NPC
  verifier run 输出 21 个 PASS、0 个 FAIL/ERROR；完整输出、摘要与 source guard/hash 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkdead-qualification-final-rerun-01/`。
- 本批不实现 `checkDead` 的特殊 transform/spawn、Good World、`noSpawnCycle` 写入、城镇
  tombstone/公告、loot、事件进度、网络通知或最终 `active=false`；这些分支仍依赖全局状态，
  继续 `partial/deferred`。不接入 death pipeline，不宣称完整 `checkDead`、NPC parity 或
  legacy 删除 readiness；`canRemoveLegacyWorldGen`、44 个 deferred `ServerRelevant` rows
  和 central WorldGen active batch 保持不变。

### Batch N3.135：`CheckActive_WormSegments` typed chain owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64548-64570`，source
  SHA-256 仍为 `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`。legacy
  先要求触发 NPC 的 `aiStyle == 6`，再按 `(int)ai[0]` 投影下一个 segment；循环保留
  `num != whoAmI && num > 0 && num < Main.maxNPCs`，只处理 active 且 `aiStyle == 6` 的子段，
  并在 self、重复、缺失、inactive、非 worm 或越界链接处终止。
- 新增 `NpcCheckActiveWormSegmentState`、`NpcCheckActiveWormSegmentInput`、
  `NpcCheckActiveWormSegmentDecision` 和 `NpcCheckActiveWormSegmentPolicy`。policy 使用
  `Version1456MaxNpcCount = 200` 的 bounded typed handle domain，保留 source 的 float-to-int
  截断顺序与 deactivate-then-follow 语义，为每个合格子段发出
  `DespawnNpcCommand(..., NpcDespawnReason.SegmentRootRemoved)`；不修改输入、ECS world、
  `NpcLifecycleComponent`、网络或 `Main` 状态。
- TDD RED 的标准 NPC verifier build 退出码为 `1`，仅出现本批 16 个缺失 owner/API 符号；
  focused verifier GREEN build/run 均退出码 `0`，覆盖非 worm 触发、截断投影、首个 inactive/
  非 worm 子段终止、self/cycle guard、调用方输入不变、非法 identity/style/link 和 bounded
  despawn reason。证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-worm-segments-{red,green}/`；
  source boundary 见
  [`2026-08-30-npc-checkactive-worm-segment-owner.md`](../research/2026-08-30-npc-checkactive-worm-segment-owner.md)。
- 本批只闭合 `CheckActive_WormSegments` 的 typed chain query，不接入 `DomeSimulation`、
  `NpcLifecycleSystem`、`Main.npc`、message-23、worm 构造/shared-life/movement、loot、revenge、
  网络/客户端表现或完整 NPC/AI parity。既有 `NpcSegmentLifecycleSystem` 的 death-follow-up
  owner 保持显式 worm classification 与 `Killed` reason；总体迁移继续为
  **NPC field/property migration: partial**，`canRemoveLegacyWorldGen=false` 与 44 个 deferred
  `ServerRelevant` rows 不变。

### Batch N3.136：`checkDead` special transform/spawn typed owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64587-64628`，source
  SHA-256 仍为
  `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`；transition excerpt
  (`NPC.cs:64587-64628`) 的 UTF-8/LF-normalized SHA-256 为
  `90DC3CC1EB2D284D97546F7D5AF57300B1428DDAE11EFFDC45465842A4E43F75`。本批保留四组
  source-order 前置分支：`396/397` 的 `ai[0] != -2f` 状态恢复和 type-400 子 NPC 请求，
  `398` 的 `ai[0] = 2f`，`517/422/507/493` 的 `ai[2] = 1f`、`ai[1] = 0f`，以及
  `548` 的 `ai[1] = 1f`、`ai[0] = 0f`。
- 新增 `NpcCheckDeadSpecialTransitionInput`、`NpcCheckDeadSpecialTransitionState`、
  `NpcCheckDeadSpecialTransitionKind`、`NpcCheckDeadSpawnIntent`、
  `NpcCheckDeadSpecialTransitionDecision` 和 `NpcCheckDeadSpecialTransitionPolicy`。policy
  接收 N3.134 已限定的 `life <= 0` 输入，恢复 `lifeMax`、damage-immunity/net-update flags，
  并以 source `(int)base.Center.X/Y` 产生有界的 type-400 spawn intent；`396/397` 的外层
  分支即使已处于 `ai[0] = -2f` 也保持 return，但不重复 spawn，其他三组 branch 只有状态
  谓词命中时才返回。
- TDD RED verifier build 退出码为 `1`，只出现本批缺失 owner/API 符号；GREEN focused build/run
  均为 `0`，覆盖 396/397（含中心坐标截断和 child `ai[3]` intent）、398、517/422/507/493、
  548 的状态转移、terminal branch、unrelated type、输入不变和 finite/domain 边界。新鲜
  Simulation/Protocol/NPC verifier/Server Release 串行 gate、source/style/diff 证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkdead-special-transition-{red,green,final-rerun-03}/`；
  source boundary 见
  [`2026-08-30-npc-checkdead-special-transition-owner.md`](../research/2026-08-30-npc-checkdead-special-transition-owner.md)。
- 本批不接入 `DomeSimulation`、`NpcDeathSystem`、`NpcLifecycleSystem` 或 spawn allocator，
  不执行 type-400 allocation/child initialization、message-23、Good World、城镇 tombstone/
  announcement、loot、事件/invasion progression、最终 `active=false` 或
  `noSpawnCycle` 写入；完整 `checkDead`、death/loot/network/persistence parity、完整
  AI/NPC parity 和 legacy deletion 继续 `partial/deferred`。`canRemoveLegacyWorldGen=false`
  与 44 个 deferred `ServerRelevant` rows 不变。

### Batch N3.137：`checkDead` invasion progress typed owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64762-64798` 及
  `64830-64947`，完整 source SHA-256 仍为
  `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`。本批保留
  `GetNPCInvasionGroup` 的四个实际入侵组、组匹配后才推进的 guard，以及
  `216 -> 5`、`395/491/471 -> 10`、`472/387 -> 0`、其他入侵类型 `-> 1` 的点数映射。
- 新增 `NpcCheckDeadInvasionProgressInput`、`NpcCheckDeadInvasionProgressDecision` 和
  `NpcCheckDeadInvasionProgressPolicy`。policy 只产生不可变的扣减结果：正点数时将
  `invasionSize` 下限截到零，并按源码使用 `invasionSizeStart - remainingSize` 计算进度；
  不读取或写入 `Main`、ECS world、网络、loot 或事件服务，也不修改输入。
- TDD RED verifier build 退出码为 `1`，仅出现本批缺失 owner/API 符号；GREEN 隔离 Release
  build/run 均为 `0`，并包含 `PASS: NPC checkDead invasion progress policy`。证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-invasion-progress-{red,green}/`；
  source boundary 见
  [`2026-08-31-npc-checkdead-invasion-progress-owner.md`](../research/2026-08-31-npc-checkdead-invasion-progress-owner.md)。
- 本批尚未接入 `WorldProgressionState`、progress report/message-78、DD2/Frost Moon/
  Pumpkin Moon、loot、death event 或最终 `active=false`。完整 `checkDead`、事件/网络/
  persistence parity 和 NPC parity 继续 `partial/deferred`；`canRemoveLegacyWorldGen=false`
  与 44 个 deferred `ServerRelevant` rows 不变。

### Batch N3.138：`checkDead` `noSpawnCycle` intent typed owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:6053,64670`。在完成
  entry qualification 与前置分支后，legacy 无条件写入静态 `noSpawnCycle = true`；本批将
  该生产点单独表示为 typed intent，不把静态世界 scratch 变量塞入 NPC 实体。
- 新增 `NpcCheckDeadSpawnCycleInput`、`NpcCheckDeadSpawnCycleDecision` 和
  `NpcCheckDeadSpawnCyclePolicy`。qualified death 始终产生 mark intent，未通过 qualification
  时不产生 intent；重复标记只作为状态信息返回，实际存储和 consume-once 仍由现有
  `NpcSpawnCycleStateComponent` 负责。policy 不访问或修改 `Main`、ECS world、spawn allocator、
  网络、loot 或事件服务。
- TDD RED verifier build 退出码为 `1`，仅出现本批缺失 owner/API 符号；GREEN focused
  build/run 均为 `0`。最终串行 Simulation/NPC verifier/Server Release gate、verifier
  (`25 PASS`, `0 FAIL/ERROR`) 与 diff check 均为 `0`。证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-spawn-cycle-{red,green,final}/`；
  source boundary 见
  [`2026-08-31-npc-checkdead-spawn-cycle-owner.md`](../research/2026-08-31-npc-checkdead-spawn-cycle-owner.md)。
- 本批不接入 `DomeSimulation`、spawn scheduler、`CheckActive`、最终 `active=false` 或完整
  `checkDead`。完整 NPC/death/event/network parity 继续 `partial/deferred`；
  `canRemoveLegacyWorldGen=false` 与 44 个 deferred `ServerRelevant` rows 不变。

### Batch N3.139：`checkDead` Good World projectile intent typed owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64755-64758`。本批保留
  `Main.getGoodWorld && type == 631` 的固定生成契约：projectile type `99`、位置为 NPC
  `base.Center`、零速度、伤害 `70`、击退 `10f`、owner 为 `Main.myPlayer`。
- 新增 `NpcCheckDeadGoodWorldProjectileInput`、`NpcCheckDeadGoodWorldProjectileDecision` 和
  `NpcCheckDeadGoodWorldProjectilePolicy`。policy 只产生不可变 spawn intent，校验 finite center
  与 bounded owner，不访问 `Main`、随机、ECS world、allocator、网络、loot 或事件服务。
- TDD RED build 退出码为 `1`，仅出现本批缺失 owner/API 符号；GREEN focused build/run 均为
  `0`，并包含 `PASS: NPC checkDead Good World projectile intent policy`。证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-goodworld-projectile-{red,green}/`；
  source boundary 见
  [`2026-08-31-npc-checkdead-goodworld-projectile-owner.md`](../research/2026-08-31-npc-checkdead-goodworld-projectile-owner.md)。
- 本批不接入 projectile allocator、Good World runtime gate、网络复制、death/loot/event
  pipeline 或完整 `checkDead`；总体 NPC migration、WorldGen deletion gate 和 44 个 deferred
  `ServerRelevant` rows 保持 `partial/deferred`。

### Batch N3.140：`GetNPCInvasionGroup` source-backed registry extraction（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64830-64947`。本批将
  `checkDead` 使用的四个实际入侵组完整提炼为 32 个类型的不可变表；源码中的 `-1/-2/-3`
  非推进分类保持排除，避免扩大到不相关事件语义。
- 新增 `LegacyNpcInvasionGroupRegistry`，并让
  `NpcCheckDeadInvasionProgressPolicy` 使用该 registry。registry 保留 `0..696` bounded
  domain、fail-closed lookup 和只读 dictionary projection，不访问 `Main`、世界状态、网络、
  随机、loot 或事件服务。
- TDD RED build 退出码为 `1`，仅缺少 registry 符号；最终 Simulation/NPC verifier/Server
  串行门和 verifier 均为 `0`，共 `27 PASS`、`0 FAIL/ERROR`。证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-invasion-group-registry-{red,final}/`；
  source boundary 见
  [`2026-08-31-npc-invasion-group-registry-owner.md`](../research/2026-08-31-npc-invasion-group-registry-owner.md)。
- 本批只提炼静态类型表，不接入 `WorldProgressionState`、message-78、invasion event、loot
  或完整 `checkDead`；NPC parity、WorldGen deletion gate 和 44 个 deferred rows 继续
  `partial/deferred`。

### Batch N3.141：town tombstone projectile type typed owner（已完成窄切片）

- 来源为 `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:64815-64824`。本批保留
  `DropTombstoneTownNPC` 的 projectile type 选择：type `17/441` 使用 `Next(5)+527`；其他
  类型使用 `Next(6)`，样本 `0 -> 43`、样本 `1..5 -> 201..205`。
- 新增 `NpcTombstoneProjectileTypePolicy`。随机样本由调用方提供，policy 只负责 bounded
  type/sample 校验和纯映射，不访问 `Main.rand`、位置、文本、projectile allocator、网络或
  世界状态。
- TDD RED build 退出码为 `1`，仅出现本批缺失 policy 符号；最终 Simulation/NPC verifier/
  Server 串行门和 verifier 均为 `0`，共 `28 PASS`、`0 FAIL/ERROR`。证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-tombstone-projectile-{red,final}/`；
  source boundary 见
  [`2026-08-31-npc-tombstone-projectile-type-owner.md`](../research/2026-08-31-npc-tombstone-projectile-type-owner.md)。
- 本批不接入 tombstone 实体、死亡文本、随机源、projectile spawn、网络或完整城镇 NPC death
  pipeline；完整 `checkDead`、loot/event/network/persistence parity 继续 `partial/deferred`。

### Batch N3.142：invasion progress command bridge typed owner（已完成窄切片）

- 本批承接 N3.137 的纯 progress decision，将正点数映射为现有
  `WorldInvasionProgressCommand(Amount, Sequence)`；sequence 必须由上游 authority 提供，
  policy 不猜测 NPC death ordering。
- 新增 `NpcCheckDeadInvasionProgressCommandPolicy`。它拒绝 non-applicable/非正点数决策及
  负数或 terminal sequence，仅构造不可变 world command，不修改 `WorldProgressionState`、
  不发送网络、不触发事件。
- TDD RED build 退出码为 `1`，仅出现 bridge 符号缺失；最终 Simulation/NPC verifier/Server
  串行门和 verifier 均为 `0`，共 `29 PASS`、`0 FAIL/ERROR`。证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-invasion-progress-command-{red,final}/`；
  source/contract boundary 见
  [`2026-08-31-npc-checkdead-invasion-progress-command-owner.md`](../research/2026-08-31-npc-checkdead-invasion-progress-command-owner.md)。
- 实际 NPC death publication 接入、sequence allocator、active invasion gate、world mutation、
  transition event 和 message-78 继续由现有 world systems 负责，完整 `checkDead`/NPC parity
  保持 `partial/deferred`。

### Batch N3.143：NPC death publication invasion progress bridge（已完成窄切片）

- `DomeSimulation.PublishNpcDeaths()` 现在在确认 `NpcDespawnReason.Killed` 后，使用注册定义的
  `NetId`、当前 `WorldProgressionState` 和 `NpcCheckDeadInvasionProgressPolicy` 生成 typed
  `WorldInvasionProgressCommand`，再通过现有 world queue 交给下一 tick 的权威结算。
- 新增 group-1 的 source-shaped definition 26 作为真实 pipeline fixture；simulation-owned
  `_nextNpcDeathSequence` 为同一 tick 的多个死亡提供不重复 sequence。命令仍不直接修改世界状态，
  并由现有 active-invasion、duplicate-sequence 和 progression system 负责最终 gate/commit。
- verifier 覆盖：同 tick 两个 group-1 NPC 各贡献一点、死亡 tick 不提前结算、下一 tick 从 10 降至 8、
  mismatch group 和无 active invasion 均不推进。`20260831-invasion-progress-pipeline-final` 的
  Simulation/NPC verifier/Server/diff-check 全部 exit `0`；verifier 共 `29 PASS`、`0 FAIL/ERROR`。
- `_nextNpcDeathSequence` 尚未加入 persistence snapshot；跨重启的 sequence continuity、完整 invasion
  event/message parity、全量 `checkDead`/NPC parity 继续 `partial/deferred`。

## 6. 当前证据与未决项

已核对：

- `docs/research/2026-08-18-npc-migration-coverage.md`：普通生成、目标、行为注册、生命周期、
  普通掉落的 verified/partial/excluded 边界。
- `docs/research/2026-08-24-npc-behavior-family-matrix.csv`：普通追逐 Complete；FlyingEye、ranged、
  escape、town、segment、event partial；Boss deferred。
- `docs/research/2026-08-24-npc-behavior-registry-boundary.md`：未知 `BehaviorId` fail-closed，
  但不代表完整 AI family parity。
- legacy `NPC.TargetClosest`（`NPC.cs` lines 64192-64290）除活动/死亡/ghost 过滤外，还依赖
  玩家 `aggro`、按 NPC 类型的 `npcTypeNoAggro`、tank pet、碰撞和朝向/netUpdate 副作用；当前
  Simulation 没有这些输入的 typed owner，因此完整 target acquisition 继续 deferred，不以最近
  玩家查询冒充语义等价。
- `src/Terraria.Dome.Simulation/Npc/Components/*`、`NpcStateSnapshot`、`NpcReplicationSnapshot`：
  当前定义/生命周期/行为/生成/home/segment/复制的实际承载面。
- `docs/research/2026-08-23-npc-death-loot-qualification-boundary.md` 和 restart boundary：死亡到
  掉落只证明窄链路，完整 `NPCLoot`、Boss、事件和表仍 deferred。

仍未完成：完整 `NPCID.Sets` 与默认表、158 个 `AI_###` 行为族、Boss/invasion/world event、全量
Buff/DoT、城镇服务/住房/对话、捕捉释放、完整 legacy 网络消息和客户端表现。当前判定必须保持：

> **NPC field/property migration: partial.** 核心实例字段已重组为 Definition/Component/Snapshot，
> 但旧 `NPC.cs` 的全量字段语义、静态表和副作用尚未等价迁移；`netID` 与
> `WhoAmIToTargetingIndex` 仅按身份字段规则排除，不代表身份契约可以删除。

## 7. 复现命令

```powershell
$source = 'D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs'
Get-FileHash $source -Algorithm SHA256
(Get-Content $source).Count
Get-Content $source | Select-String -Pattern '^\s*(public|private|protected|internal)\s+.*;\s*(//.*)?$'
Get-Content $source | Select-String -Pattern '^\s*(public|private|protected|internal)\s+[^;]+(=>|\{\s*$)'

dotnet build src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -m:1
```

本次只读审计未修改源代码；仓库已有的大量未提交改动保持不动。编译命令是后续行为切片的
验证入口，不能把绿色编译单独当作字段语义等价证据。

## 8. 完成审计（2026-08-28）

| 要求 | 当前证据 | 结论 |
| --- | --- | --- |
| 实例字段不再依赖旧 `NPC` 对象 | `Npc.Boundary.Verification`：1070 个 Simulation 文件，legacy NPC 违规 0 | `verified`（边界） |
| 定义/生命周期/目标/普通行为/掉落有 typed owner | `Npc.Verification`：定义输入、目标选择、死亡 revision、确定性掉落 PASS | `verified`（窄链路） |
| `chaseable`/`dontTakeDamage` 有动态行为 owner 和资格谓词 | `NpcBehaviorStateComponent` + `NpcCombatClassificationSystem.CanBeChasedBy`；NPC verifier 覆盖阈值、阵营、无敌及显式例外，N3.25-N3.27 日志 PASS | `verified`（窄链路） |
| 玩家 projectile -> NPC 使用 typed 目标资格 | `ProjectileTargetEligibilitySystem.CanTargetNpc` 已接入 `DomeSimulation.DetectProjectileHits`；Combat verifier 覆盖 friendly/hostile、不可 chase、免疫及 training dummy 例外，N3.28-N3.29 日志 PASS | `verified`（窄链路） |
| NPC target range 保留有限值和严格距离边界 | `NpcTargetRoutingSystem.IsWithinTargetRange`：finite 输入、负距离拒绝、严格 `<`；NPC verifier 的 3-4-5、相等、NaN 场景 PASS（N3.31） | `verified`（窄链路） |
| NPC target selection 拒绝距离平方溢出 | `NpcTargetSelectionSystem` 跳过非 finite `DistanceSquared`；NPC verifier 覆盖 `float.MaxValue` 溢出候选并保持可表示候选，`Build/diagnostics/npc-complete/task-10-interaction/20260828-233000/` 三条日志 PASS | `verified`（数值边界） |
| TargetClosest aggro/no-aggro 优先级纯查询 | `NpcTargetSelectionSystem.TryCalculateTargetPriority` 覆盖 Manhattan 距离、aggro 减免、`1000f` no-aggro 惩罚和 NaN 拒绝；`Build/diagnostics/npc-complete/task-10-interaction/20260829-000500/` verifier/Simulation/Server PASS | `verified`（纯查询） |
| Player aggro typed owner 接入 NPC target selection | `PlayerTargetingStateComponent` + `NpcTargetCandidate.TargetPriority` 已接入 `DomeSimulation.SelectNpcTargets`；`20260829-003500/` 三条验证日志 PASS | `partial` |
| Player npcTypeNoAggro typed 集合接入 priority | `PlayerTargetingStateComponent.IsNoAggroNpc` 按 NPC DefinitionId 查询并接入 runtime priority；`20260829-010500/` 三条验证日志 PASS | `partial` |
| Item-3090 no-aggro source set 有版本化 registry | `NpcNoAggroCapabilityRegistry` 登记 23 个 source-backed DefinitionId；`20260829-014500/` 三条验证日志 PASS | `verified`（表集合） |
| No-aggro 动态刷新有显式契约 | `PlayerNpcTargetingSystem.RefreshNoAggroCapabilities` 支持开启应用/关闭清除；`20260829-023000/` 三条验证日志 PASS | `partial` |
| 装备 item-3090 触发 no-aggro 刷新 | `RecalculateEquipmentStats` 接入非 vanity/source-slot 校验和刷新；`20260829-040000/` 三条验证日志 PASS | `partial` |
| 动态 aggro 结果有显式提交契约 | `PlayerNpcTargetingSystem.SetAggro` 写入 typed Player targeting state；Npc verifier 覆盖 `-750` 结果 | `partial` |
| Stealth aggro 数值规则有纯 resolver | `CalculateAggro` 覆盖隐身、shroomite/vortex reduction 与 finite/饱和边界；`20260829-061500/` 三条验证日志 PASS | `partial` |
| Player stealth 状态有 typed owner 和刷新入口 | `PlayerStealthStateComponent` + `RefreshAggro` 接入 aggro state；`20260829-073000/` 三条验证日志 PASS | `partial` |
| Stealth 状态有显式 tick 转移 | `AdvanceStealth` 覆盖静止衰减、移动恢复、坐骑重置；`20260829-083000/` 三条验证日志 PASS | `partial` |
| Player tick 驱动 stealth 状态推进 | `DomeSimulation.AdvancePlayerStealth` 接入速度/ItemUse typed owner；`20260829-093000/` 三条验证日志 PASS | `partial` |
| Player mount typed owner 接入 stealth tick | `PlayerMountStateComponent` 替代硬编码 mounted=false；`20260829-103000/` 三条验证日志 PASS | `partial` |
| NPC replication dirty intent 有 typed owner | `NpcReplicationComponent.MarkDirty/ClearDirty` 及 snapshot revision 变化接入；NPC verifier PASS，但 per-session cursor、PVS/节流消费者仍未闭合 | `partial` |
| NPC home last-published baseline 有 typed owner | `NpcHomePublicationComponent`/`NpcHomePublicationSystem` 覆盖三元 tuple 比较、compare-before-write、捕获及 `NpcStateSnapshot` round-trip；`20260830-home-publication-baseline/` RED/GREEN verifier PASS | `verified`（窄链路） |
| NPC `trapImmune` 有 projectile eligibility owner | `NpcDefinition.IsTrapImmune` → `NpcAuthorityComponent.IsTrapImmune` → `NpcSpawnCommitSystem`，`ProjectileTargetEligibilitySystem` 仅拒绝 `IsTrap && IsTrapImmune`；`20260830-trap-immunity-green/` Combat verifier PASS | `verified`（窄链路） |
| NPC `lavaImmune` definition/authority/contact owner | `NpcDefinition.IsLavaImmune` → `NpcAuthorityComponent.IsLavaImmune` → `NpcLavaContactSystem` → `DamageNpcCommand(SourceKind = Lava)` → authoritative settlement；`20260830-lava-immunity-final-green-rerun/` NPC/Combat/Protocol verifier 与 Simulation/Server build PASS | `verified`（窄链路） |
| NPC `reflectsProjectiles` dynamic owner/eligibility query | `NpcBehaviorStateComponent.ReflectsProjectiles` → `ProjectileReflectionEligibilityPolicy.CanBeReflectedByNpc`；`20260830-reflect-projectiles-green/` Combat verifier、Simulation/Combat/Server clean build 与 NPC protocol shape check PASS | `verified`（窄链路） |
| NPC inactivity-preservation reads and timer gate | `LegacyNpcInactivityRegistry` exact set/companion gates/type-668 split → active typed net-id observation → `NpcLifecycleSystem` no-decrement gate；`20260830-inactivity-lifecycle-green/` NPC verifier、Simulation/Server build、NPC protocol shape、diff/checkpoint checks PASS | `verified`（生命周期窄链路） |
| NPC static `townNPC` read and timer gate | `LegacyNpcTownRegistry` exact 39-effective-type source set (32 branches) → typed `NpcDefinitionComponent.NetId` → `NpcLifecycleSystem` town gate；`20260830-town-inactivity-lifecycle-green/` corrected source audit、NPC verifier、Simulation/Server build、NPC protocol shape PASS | `verified`（生命周期窄链路） |
| 交互会话状态有 owner 和过期规则 | `Npc.Composition.Verification`：授权 session、apply、timeout expiry PASS | `verified`（窄链路） |
| `GivenName` null 归一化和内存恢复 | Composition + Protocol verifier：null/非空/清除及 state round-trip PASS | `verified`（内存） |
| 给定名不污染网络协议 | `Npc.Protocol.Verification`：SyncNPC projection/codec round-trip PASS；字段不在 `NpcReplicationSnapshot` | `verified`（隔离） |
| 给定名磁盘持久化 | V36 `DomeStatePersistenceFormat` 仅读写 `NpcReplicationSnapshot[]`，无 state/name 段 | `deferred` |
| 完整对话、Localization、住房、商店 | 无服务器权威契约；旧字段依赖 `Main`/`Language`/全局表 | `deferred` |
| 全量 AI/Buff/DoT/Boss/事件 parity | 行为族矩阵和字段矩阵仍标记 partial/deferred | `deferred` |

审计结论：本清单的证据足以证明核心实例字段已完成类型化重组和窄链路回放，但不足以证明
旧 `NPC.cs` 的全量语义等价。总状态必须保持 **partial**；后续批次只能从有 source-backed
contract 的 deferred 行进入，不得用“编译通过”替代行为证据。
