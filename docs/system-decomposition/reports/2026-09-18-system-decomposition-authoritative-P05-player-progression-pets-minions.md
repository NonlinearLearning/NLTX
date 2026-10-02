# System Decomposition Report: authoritative P05

partitionId: P05  
taskId: AUTH-SYS-P05  
taskSetName: authoritative-system-decomposition  
sessionId: 65bb838ff91b4927a81ee6d3e1500515  
designStatus: proposed  
evidenceStatus: partial  
verificationStatus: not-run  
sourceModified: false

## Scope and Evidence

本报告只覆盖 claim 输入中的 P05：玩家进度、召唤物、宠物与伙伴。输入库存含 17 个叶子组、164 个字段、0 个属性；下面逐组列出完整成员。成员在 `Terraria.Player` 中的声明身份以输入台账为准，不因此确认当前 NLTX 的运行时 owner。

唯一输入、专属 prompt 与授权输出：

| 用途 | 路径 |
|---|---|
| authoritative 输入 | `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P05-Player-Progression-Pets-Minions.md` |
| claim 专属 prompt | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-11-version4-authoritative-20-partition-prompts\2026-09-11-version4-P05-player-progression-pets-minions-public-decomposition.md` |
| 唯一授权输出 | `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P05-player-progression-pets-minions.md` |

完整成员覆盖：

- `PlayerConsumedProgressionFlags` (6): `usedAegisCrystal`, `usedAegisFruit`, `usedArcaneCrystal`, `usedGalaxyPearl`, `usedGummyWorm`, `usedAmbrosia`。
- `PlayerFishingCapabilityState` (7): `fishingSkill`, `cratePotion`, `sonarPotion`, `accFishingLine`, `accFishingBobber`, `accTackleBox`, `accLavaFishing`。
- `PlayerMinionCapacityState` (3): `maxMinions`, `numMinions`, `slotsMinions`。
- `PlayerCoreMinionSummonFlags` (22): `pygmy`, `raven`, `slime`, `hornetMinion`, `impMinion`, `twinsMinion`, `spiderMinion`, `pirateMinion`, `sharknadoMinion`, `UFOMinion`, `DeadlySphereMinion`, `stardustMinion`, `stardustGuardian`, `stardustDragon`, `batsOfLight`, `babyBird`, `vampireFrog`, `stormTiger`, `smolstar`, `empressBlade`, `flinxMinion`, `abigailMinion`。
- `PlayerCrossoverMinionSummonFlags` (3): `deadCellsMushroomBoiMinion`, `palworldCattivaMinion`, `palworldFoxsparksMinion`。
- `PlayerMinionDamageTrackingState` (2): `highestStormTigerGemOriginalDamage`, `highestAbigailCounterOriginalDamage`。
- `PlayerUnlockProgressionState` (4): `unlockedBiomeTorches`, `ateArtisanBread`, `unlockedSuperCart`, `enabledSuperCart`。
- `PlayerQuestAndEventCounters` (3): `anglerQuestsFinished`, `golferScoreAccumulated`, `downedDD2EventAnyDifficulty`。
- `PlayerLegacyPetState` (21): `suspiciouslookingTentacle`, `crimsonHeart`, `lightOrb`, `blueFairy`, `redFairy`, `greenFairy`, `bunny`, `turtle`, `eater`, `penguin`, `HasGardenGnomeNearby`, `magicLantern`, `rabid`, `sunflower`, `wellFed`, `puppy`, `grinch`, `miniMinotaur`, `blackCat`, `spider`, `squashling`。
- `PlayerBossPetFlags` (16): `petFlagKingSlimePet`, `petFlagEyeOfCthulhuPet`, `petFlagEaterOfWorldsPet`, `petFlagBrainOfCthulhuPet`, `petFlagSkeletronPet`, `petFlagQueenBeePet`, `petFlagDestroyerPet`, `petFlagTwinsPet`, `petFlagSkeletronPrimePet`, `petFlagPlanteraPet`, `petFlagGolemPet`, `petFlagDukeFishronPet`, `petFlagLunaticCultistPet`, `petFlagMoonLordPet`, `petFlagFairyQueenPet`, `petFlagQueenSlimePet`。
- `PlayerSeasonalAndEventPetFlags` (9): `petFlagDD2Gato`, `petFlagDD2Ghost`, `petFlagDD2Dragon`, `petFlagPumpkingPet`, `petFlagEverscreamPet`, `petFlagIceQueenPet`, `petFlagMartianPet`, `petFlagDD2OgrePet`, `petFlagDD2BetsyPet`。
- `PlayerStandardNamedPetFlags` (13): `petFlagUpbeatStar`, `petFlagSugarGlider`, `petFlagBabyShark`, `petFlagLilHarpy`, `petFlagFennecFox`, `petFlagGlitteryButterfly`, `petFlagBabyImp`, `petFlagBabyRedPanda`, `petFlagPlantero`, `petFlagDynamiteKitten`, `petFlagBabyWerewolf`, `petFlagShadowMimic`, `petFlagVoltBunny`。
- `PlayerCrossoverPetFlags` (13): `petFlagBerniePet`, `petFlagGlommerPet`, `petFlagDeerclopsPet`, `petFlagPigPet`, `petFlagChesterPet`, `petFlagJunimoPet`, `petFlagBlueChickenPet`, `petFlagSpiffo`, `petFlagCaveling`, `petFlagDeadCellsSwarmBiter`, `petFlagPufferfish`, `petFlagChillet`, `petFlagChilletIgnis`。
- `PlayerWorldObjectPetFlags` (4): `petFlagDirtiestBlock`, `petFlagBoulderPet`, `petFlagRainbowBoulderPet`, `petFlagAxeFairyPet`。
- `PlayerCompanionState` (14): `companionCube`, `babyFaceMonster`, `snowman`, `dino`, `skeletron`, `hornet`, `zephyrfish`, `tiki`, `parrot`, `truffle`, `sapling`, `cSapling`, `wisp`, `lizard`。
- `PlayerMountAndMinecartEffects` (7): `onWrongGround`, `onTrack`, `cartRampTime`, `cartFlip`, `trackBoost`, `lastBoost`, `mount`。
- `PlayerAccessoryProgressionEffects` (17): `brokenMirrorBadLuck`, `flowerBoots`, `fairyBoots`, `hellfireTreads`, `moonLordLegs`, `deadMansSweater`, `arcticDivingGear`, `coolWhipBuff`, `cobWhipBuff`, `wearsRobe`, `magicCuffs`, `coldDash`, `sailDash`, `desertDash`, `desertBoots`, `eyeSpring`, `scope`。

证据身份与限制：

| 层 | 本次读取与身份 | 可支持内容 | 限制 |
|---|---|---|---|
| Version4 | `D:\TRbackup\Version4` 本地源码；目录不是 Git 仓库，故没有可记录的 Git HEAD。下表 hash 标识本次读取的关键文件。 | 已读取代码中的同步调用、直接字段重置/累加、投射物对 Player 状态的直接访问。 | 局部静态代码不闭合全部 caller、调度、事件订阅、动态分派或存档/网络行为。多个被清空的方法体是 `unknown`。 |
| CPG 查询 | 只读 `CpgEvidence.ps1`；数据库 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`；manifest SHA-256 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；project fingerprint `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`；import `complete`，967 shards、8,166,789 nodes、71,038,907 edges、1,317 diagnostics。 | 所选 shard 范围内的符号、直接 call-site 和字段访问候选。 | `SourceSnapshotId=null`，未绑定当前源码 hash。`complete` 仅表示该索引查询在范围/预算内完成，不代表运行时闭包。`Unknown` access mode、callee effects、alias、动态入口和调度顺序均未闭合。 |
| 当前 NLTX | NLTX HEAD `865ca66bfec2b8b455aabe3ed29ce0e60a1f9401`；检查目标 `D:\TRbackup\NLTX\src\NSSLC`。本次启动时该目录为工作区内容，以下代码形状不等同于已接入的生产 System。 | 已存在的 commit/rebuild/query 类型及 P05 当前候选实现形状。 | 类型文件和 verifier 文件存在不证明 ECS 注册、生产调用路径、版本兼容、行为等价或集成完成。 |
| SS14 参考 | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Research\Systems\SharedResearchSystem.cs`，SHA-256 `6B4FCFFB4B554E30792943E6D9803C2F7E7B04113F2E1D4D0B6D051D4EEF11B6`。 | 只作组织参考：`SharedResearchSystem` 是 `EntitySystem`，可以依赖其他 System 并承载行为 API。 | 不证明 Terraria 的持久化、事件、权威 owner 或调度语义。 |

关键 Version4 文件 SHA-256：

| 文件 | SHA-256 |
|---|---|
| `Terraria/Player.cs` | `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86` |
| `Terraria/Main.cs` | `66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520` |
| `Terraria/Projectile.cs` | `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B` |
| `Terraria/Mount.cs` | `2DED2B174731DDDD003BCD1B03F83853D0AD39183CD3683B4C5289459AEDCEFC` |
| `Terraria/Minecart.cs` | `6E35F5039BBA399C7D6EB758DA7CB2F1242C141B83E68BB1CF17BAE630EED33F` |
| `Terraria/NetMessage.cs` | `87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A` |
| `Terraria/MessageBuffer.cs` | `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE` |
| `Terraria.IO/WorldFile.cs` | `92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289` |

当前 NLTX P05 候选文件 SHA-256（工作区快照；不等同于 HEAD 已跟踪内容）：

| 文件 | SHA-256 |
|---|---|
| `src/NSSLC/Component/Player/Progression/PlayerProgressionCommitSystem.cs` | `2FC87D072AA8A74793CD8E833407CDBDF141AB32DBAF4D3DCB97C4B183FC100B` |
| `src/NSSLC/Component/Player/Progression/PlayerFishingCapabilityRebuildSystem.cs` | `5595DD32A2BFDE86766164E17DF55E877E97A67F1773F8551885962C336C9536` |
| `src/NSSLC/Component/Player/Progression/PlayerMinionCapacityCommitSystem.cs` | `206CE99FC846A0719CCD7753CE14CAFD137A21834089F4EB3E9B054BC429A37A` |
| `src/NSSLC/Component/Player/PlayerAccessoryEffectRebuildSystem.cs` | `0769FCF710EEF536477760D478061F62544C794F90572BD425BA9BEB37DCA573` |

本次用 `Find-CpgSymbols`、`Find-CpgCallSites`、`Get-CpgMemberUses`、`Get-CpgEvidenceHealth` 查询。Player update 方法符号可解析；选定范围中 `ResetEffects` 的 call-site query 为 `complete` 1 项。`UpdateBuffs`、`UpdateEquips`、`UpdateProjectileCaches` 的 call-site query 均为 `partial`、0 项且带 gap；同一调用在当前 `Player.cs` 可直接读到，因此查询零命中不作为“没有调用”的证据。字段样本中 `numMinions` 查询返回 3 条（2 `Unknown`、1 `Write`），`maxMinions` 23 条（20 `Unknown`、2 `ReadWrite`、1 `Write`），`slotsMinions` 3 条（含 `Unknown`、`ReadWrite`、`Write`）；`usedAegisCrystal` 和 `petFlagJunimoPet` 各有 `Unknown` 与写事实，`accFishingLine` 的选定访问事实全为写。`cartRampTime` 使用查询为 `partial` 且 0 条；`mount` 的精确 Player 字段查询找到符号，但 member-use 返回 `partial`、达到 250 条预算且 access mode 均为 `Unknown`。以上只说明受限 shard 内的事实，不能据此认定完整读写者或唯一写者。

## Prior Component Decomposition Reconciliation

先前 P05 Component 设计和执行文档仍是候选，不是本次运行时证据：
`docs/component-decomposition/review-round-2/2026-09-11-version4-P05-player-progression-pets-minions-component-design.md`、
`docs/component-decomposition/review-round-2/2026-09-11-version4-P05-player-progression-pets-minions-component-execution.md`。

| 现状 | 对 System 拆分的处理 |
|---|---|
| `PlayerProgressionCommitSystem` 已有 consumed-upgrade、unlock/preference、Angler/Golfer/DD2 的显式 commit 入口。 | 保留为持久进度的候选写入口，继续使用不同 command 和校验；命令来源授权、奖励顺序、存档提交和网络投影仍 `unknown`。本次在 `src/NSSLC` 没找到生产 caller，静态存在不代表接入。 |
| `PlayerFishingCapabilityRebuildSystem`、`FishingCapabilityQuery` 和 7 字段 snapshot 已存在。 | 可保留 rebuild 与只读 snapshot query 的边界；完整钓鱼尝试仍需杆/饵/水域/环境和 bobber Projectile 的外部输入，不能把这些状态复制进 Player snapshot。 |
| `PlayerMinionCapacityCommitSystem` 与 `RemainingMinionCapacityQuery` 已存在。commit 检查非负、有限 delta、最大 slots、token 幂等；Query 可在提交前读取剩余容量。 | 保留一个容量状态 owner 的候选，但查询后另发 delta 不能独自保证“检查与实体生成”原子性。Player/Projectile 的 admission、回滚和销毁顺序要交由 integration review 定义。现有方法也没有发现 P05 生产调用点。 |
| 多组 Pet/Minion/Companion capability Components 已存在；本次路径检索未找到 `PlayerPetCapabilityRebuildSystem` 的生产实现。 | 不按每个叶子组建独立 System，也不把所有旗标塞进一个 PetFlags 巨型 Component。需按同一 rebuild 输入、内容映射和实体生命周期归并；content catalog 与实体命令边界待确认。 |
| `PlayerAccessoryEffectRebuildSystem` 已存在，当前负责 `PlayerAccessoryStringEffectComponent` 的一个子集；其代码不等于 17 个 P05 accessory fields 的完整重建。 | 将其作为派生效果候选；不能把未映射字段、luck、movement、combat 及 environment 输出假设成同一个 owner。 |
| `PlayerMountVehicleIntegrationComponent` 和 Mount/Minecart 相关结构存在；prior execution 明确将原始 `Mount mount` 与车辆状态 owner 标为阻塞。 | P05 不接管 P03/Movement 的车辆状态，也不新增第二个 Mount/Minecart 写者。最终状态模型与 typed command/query 交 `crossSubsystemOwner: integration-review`。 |

目标目录内对 `PlayerProgressionCommitSystem.Commit`、`PlayerFishingCapabilityRebuildSystem.Rebuild`、`PlayerMinionCapacityCommitSystem`、`PlayerAccessoryEffectRebuildSystem` 的静态 caller 检索，只找到 accessory/minion 的 verifier `Program.cs` 引用及实现声明；未找到生产注册/调度接入。此结论仅限检索到的当前 `src/NSSLC` 文本范围，不证明其他生成或外部注册路径不存在。

## Conceptual Behaviors

以下按行为和生命周期切分，不按 17 个成员组机械建立 17 个 System。字段属于哪个行为仍须由真实读取、写入和存档协议确认。

| 稳定行为 ID | 范围 | 不变量与可观察结果 | 状态 |
|---|---|---|---|
| `player-progression-commit` | consumed flags、unlock/preference、quest/event counters | 单次升级消费/解锁及进度计数的有效提交；重复命令不能重复消费或重复计数，奖励与事件顺序必须维持。 | proposed；持久化与命令来源 unknown |
| `player-fishing-capability-rebuild` | 7 个 fishing 字段 | tick 中由 reset、装备和 buff 贡献构成的 fishing capability；Query 只读同一已提交 snapshot，不读写外部钓鱼状态。 | current source 显示 reset/累加路径；完整输入和刷新点 partial |
| `player-minion-capacity-admission` | `maxMinions`、`numMinions`、`slotsMinions` | 容量上限、已占用数量/slots、投射物加入/退出之间保持一致；超额时不能留下已计数却不存在的实体。 | Player/Projectile 共同修改，唯一 owner unknown |
| `player-minion-capability` | 核心/跨界 summon flags | 由玩家装备、buff、内容定义生成当前可用 summon capability；能力旗标与实体实际存在分开。 | rebuild rules/content registry partial |
| `player-minion-damage-tracking` | 两个 highest original damage 字段 | 记录特定 minion 伤害输入的高水位或显示数据；更新来源、归零边界及 Combat 归属需确认。 | proposed，write closure unknown |
| `player-pet-capability` | Legacy、Boss、Seasonal/Event、Standard、Crossover、WorldObject pet flags | 解锁/资格、当前启用状态和投射物存活状态须区分；不允许一次死亡清理意外丢失持久解锁。 | 语义及持久性 unknown |
| `player-companion-state` | 14 个 companion fields | 伙伴能力映射与伙伴实体、展示和清理保持一致；不以字段存在推断创建/销毁命令。 | entity lifecycle owner integration-review |
| `player-vehicle-runtime-effects` | 6 cart flags/counters 加 `Mount mount` | Mount 与 minecart 更新、碰撞/轨道和 Player 身体状态协作，保留 reset/step 顺序。 | `crossSubsystemOwner: integration-review` |
| `player-accessory-effect-rebuild` | 17 个 accessory fields | 按 tick 从装备/效果来源建立当前派生能力；luck、movement、combat、环境和视觉结果按其实际 owner 提交。 | 多个效果域，整合边界 partial |

这些行为没有一个已被本报告证明完成迁移。特别是字段命名不能证明“progression flag”可存档，也不能证明“pet flag”是永久解锁而非本次召唤状态。

## State Ownership and Write Closure

**已读到的 Version4 写入路径：**

| 状态 | 直接证据 | Proposed owner 与闭合状态 |
|---|---|---|
| `maxMinions`、`numMinions`、`slotsMinions` | `Player.ResetEffects` 重置 `maxMinions` 基值，Player 更新路径有清理 minion counts；`Projectile.cs` 在 minion admission 读取 count/slots/limit，成功时递增 `numMinions` 并累加 `slotsMinions`。见 `Player.cs:10024`、`Player.cs:10446`、`Projectile.cs:14743`、`Projectile.cs:14744`、`Projectile.cs:14766`、`Projectile.cs:14767`。 | 同一容量不变量需要一个协调提交协议；因另有 Projectile 生命周期写入，唯一 P05 owner `unknown`，强制 `crossSubsystemOwner: integration-review`。 |
| fishing capability | `Player.ResetEffects` 将 fishingSkill 与多项钓鱼辅助效果重置，装备更新路径重新累计技能/设置能力。见 `Player.cs:6824`、`Player.cs:8301`、`Player.cs:10493`、`Player.cs:10498`。 | proposed 单一 rebuild 提交点管理基值和当 tick modifier；输入来源闭包 partial。不要由 Fishing Query 回写。 |
| pet flags | `Projectile.cs` 特定宠物 AI 读取 `Player.dead`/pet flag、设置 projectile `timeLeft`；部分分支在玩家死亡时直接清除 Player flag。见 `Projectile.cs:38510`、`Projectile.cs:38521`、`Projectile.cs:38851`。 | Projectile 与 Player 目前都有直接写/读关系，且不是全部 Pet 类型均检查完。旗标的解锁/启用/存活语义和唯一 writer 均 `unknown`。 |
| progression、unlock 与 event counter | P05 台账能确认字段声明。CPG 在所选 use-site 对 `anglerQuestsFinished` 返回 Write 和 Unknown，对 `usedAegisCrystal` 返回 Write 与 Unknown；`Player.cs` 初始化路径将一部分 consumed flags 设为 false。 | proposed typed commit commands；所有来源、重复事件、save/load、network projection 及奖励先后均 `unknown`。 |
| accessory fields | `Player.UpdateEquips` 与 `ResetEffects` 暗示 per-tick effect build/reset；CPG 对全部字段读写模式不闭合。 | proposed 派生 snapshot/rebuild 与各功能 owner 的显式输出；不能给整个组一个已经确认的单一 owner。 |
| mount/minecart | `Player.mount` 是 `Mount` 对象；`Mount.UpdateAfterEquips` / `UpdateEffects` 接收 Player；`Minecart.TrackCollision` 也接收 Player 并读其 Minecart settings。见 `Player.cs:1552`、`Mount.cs:2595`、`Mount.cs:4097`、`Minecart.cs:566`。 | P03/Movement 的车辆状态和本分区 Player effects 必须共用一个集成协议；`crossSubsystemOwner: integration-review`。 |

CPG representative use facts 中包含 `Unknown` 和 `ReadWrite`，且 `cartRampTime` 与 `mount` 使用结果 partial。没有用零命中、字段名相似或同一 partial class 将任何字段认定为“无其他读者”或“已有唯一写者”。被读取的 `Player.InternalSavePlayerFile`、`Serialize`、`Deserialize`、`FixLoadedData` 体为空或近似 stub（`Player.cs:26417`、`Player.cs:26418`、`Player.cs:26455`、`Player.cs:26458`），因此这份 Version4 快照不能支持 P05 的存档兼容结论。

## Boundary Role and Decision

| 能力边界 | 决定 | 理由与限制 |
|---|---|---|
| 一次性 consumed progress、unlock/preference、quest/event commit | `keep` 为一个进度 commit capability，按不同 typed command 保留各自校验与结果；不按字段拆 System。 | 共用 player progression 提交方向，但事件来源和副作用仍不同；只有 source adapters 可在授权后调用 commit owner。当前 target 文件是候选代码，不是已接入 System。 |
| Fishing snapshot rebuild/query | `separate` 于持久 progression commit；保留一个 rebuild System/已有 System API，并让 snapshot query 保持只读。 | 它按 tick 清零再聚合装备/buff，生命周期与 durable progress command 不同；杆/饵/世界液体及 bobber 不是 Player snapshot 的 owner。 |
| Minion capability rebuild 与容量计量 | 旗标重建 `separate` 于容量 admission/aggregation；容量的 Query 和 Command 不分成彼此独立的权威写者。 | 容量检查必须与投射物 admission/cleanup 协作。若先 Query 再独立 commit，存在状态被其他实体变化的窗口；精确原子边界待 integration review。 |
| Pet/companion capability 与实体生命期 | `separate` capability rebuild 和 projectile/entity synchronization；多个旗标组可在一个能力 System 内分区处理，不为 17 个库存组各设 System。 | Player flag 与 Projectile 的 heartbeat、死亡、销毁直接交织。定义映射、创建/销毁和持久解锁 owner 尚未确认。 |
| Minion damage high-water tracking | `separate` 于容量和 summon flag build，按 Combat/Projectile 的受控结果输入提交。 | damage 观测是时间累计事实，与 slot reservation/旗标重建不变量不同；实际更新事件入口 `unknown`。 |
| Accessory derived effects | `separate` 于 durable progression，按多个受影响功能输出能力 snapshot 或显式 handoff。 | 部分已实现 rebuild 不能证明其余 accessory effect 的唯一 owner；不能把 effect 输出投影回权威状态。 |
| Mount/minecart effects | 本分区 `partial`，不新设 final owner。 | `mount` 为嵌入式 Mount runtime 对象，轨道与坐骑直接修改 Player；所有权留给 Mount/Movement integration。 |

拒绝的替代方案：为 164 个字段或 17 个库存组分别创建 17 个 Systems；将所有 pet/summon/companion 状态合并到一个不可维护的大 Component；把只读 `RemainingMinionCapacityQuery` 当成 capacity commit 的最终授权；或依据文件顺序推导 tick scheduler 顺序。它们都会隐藏现有共享写入或增加转发层，而没有闭合真实不变量。

## System API and Legacy Behavior Mapping

下表是 proposed 概念 API 组合，不是要求签名一一复制，也不是已经存在的 production call path。

| Legacy entry / behavior | Proposed API composition | 兼容要求与状态 |
|---|---|---|
| `Main.DoUpdateInWorld` 遍历 active Player 并调用 `Player.Update(i)` | World/player tick coordinator 调用 player per-tick begin/rebuild APIs，再在规定阶段向其他 owner 发布只读 snapshots。 | 保留 active-player 范围与当前可观察顺序。具体新调度器、并行策略与 flush/barrier 为 `unknown`。 |
| `Player.ResetEffects`、`UpdateBuffs`、`UpdateEquips` | `BeginPlayerCapabilityTick` -> 按输入重建 `FishingCapabilitySnapshot`、minion/pet flags 与 accessory effect snapshots；Query 只读 snapshot。 | 不把 reset、每个 modifier 和 commit 混成一个纯 Query；同一 tick 的基值与 modifier 次序必须稳定。 |
| consumed upgrade、unlock/preference、quest/event progress 写入 | 经授权的 `CommitConsumedUpgrade`、`CommitUnlockProgress`、`RecordQuestEvent` typed commands -> 单一进度 commit owner -> persistence/network adapters。 | 保留无效输入、重复 token、状态 delta、奖励/事件顺序和返回状态；因旧输入 caller 与 persistence 证据不足，具体映射 `unknown`。 |
| `Projectile` minion admission 与 `Player` capacity counters | Capacity Query 用于显示/预判；最终 `TryAdmitMinion` 或等价协调 Command 在 spawn/kill 协议内重新校验、保留容量，再与 Projectile owner 成功/失败提交。 | 不允许只凭早先 Query 的结果超额生成；创建失败、kill、回滚与 counter 更新的原子性为 blocking decision。 |
| minion damage tracking fields | Combat/Projectile 发送已归属 Player、damage kind 与 source revision 的受控结果到 `RecordMinionDamageHighWater`；读取仅返回 snapshot。 | 事件幂等性、damage provenance、死亡/重置时机和显示者仍待源码/集成验证。 |
| pet/companion flags 及宠物 projectile AI | `RebuildPetCapabilities` 计算 Player capability；`SynchronizeOwnedPet`/生命周期命令由选定的 Projectile/Pet owner 执行。 | 旗标、解锁和活体 projectile 分开建模；跨世界、重连和死亡后行为需保持实测；最终 owner `integration-review`。 |
| Mount 与 minecart flags/state | 通过 Mount/Vehicle owner 的只读 state query 和经授权更新命令取得/提交状态；本分区仅输出 typed integration input。 | 不复制 Mount 对象、track collision 或玩家位置逻辑；typed boundary 与唯一写者由 P03/Movement 集成确定。 |
| accessory effects | `RebuildAccessoryEffects` 产出不可变 per-tick capability snapshot；Combat、Movement、Luck/Environment、Presentation 等 owner 分别消费，必要时以显式 command 更新其自有状态。 | 先证明 17 个字段的 reset/write/consumer 映射；不让 Projection 或 Query 隐藏写回。 |
| `Player.SavePlayer` / `Serialize` / `Deserialize` | 由 persistence Adapter 读写版本化 Player progression snapshot，恢复先校验再提交至各自权威 owner。 | 只是 future composition proposal。目标快照相关函数体为空，版本迁移字段和错误处理均 `unknown`。 |

不要为所有只读访问建独立 Query 类型。`GetFishingCapability`、`GetRemainingMinionCapacity` 等规则可保留在相应 owner 的 System API 或已有共享规则 Query；只有跨多个决策点复用且无可观察写入的规则才值得提取。Query 不授予绕过最终 admission/commit 的权限。

## Call and Dependency DAG

**当前源码中可确认的同步关系：**

```text
Main update (Main.cs:11354)
  -> Main.DoUpdateInWorld (Main.cs:11411)
     -> active player[i].Update(i) (Main.cs:11421)
        -> ResetEffects() (Player.cs:15207)
        -> UpdateProjectileCaches(i), UpdateBuffs(i) (Player.cs:15238-15239)
        -> UpdateEquips(i) (Player.cs:15296)
        -> UpdateMaxTurrets() in a later Player.Update path (Player.cs:15355)

Projectile minion admission
  -> read Player.numMinions / slotsMinions / maxMinions (Projectile.cs:14743-14744)
  -> on success increment Player.numMinions and slotsMinions (Projectile.cs:14766-14767)

Pet projectile AI
  -> read Player.dead and a selected pet flag
  -> some AI branches clear that Player flag on death
  -> keep the corresponding projectile alive while the flag is true
```

`Player.ResetEffects` sets the minion maximum baseline and clears fishing/equipment-derived effects; equipment/buff paths subsequently add modifiers. The direct source order above is a compatibility constraint, not proof of a parallel-safe or fully closed scheduler. The CPG call-site query for three named methods remained partial even though their direct calls are visible in source.

| Relation | Evidence/status | Migration constraint |
|---|---|---|
| `Main.DoUpdateInWorld -> Player.Update` | Source call in `Main.cs`; CPG `Player.Update` symbol lookup resolves but selected query is not a full inbound-call graph. | Re-establish active player set and failure/exception handling in the actual runtime coordinator. |
| `Player.Update -> ResetEffects -> UpdateBuffs/UpdateEquips` | Current source direct calls; `ResetEffects` call-site query complete for selected scope, other call-site queries partial. | `ResetEffects`/rebuild must precede consumers that require current tick state; no new order inferred from filenames. |
| `Player` capacity <-> Projectile admission | Direct Player reads and writes inside Projectile source; reset/maximum writes also in Player. | One invariant needs an explicit shared commit/admission protocol. P05 cannot name Player or Projectile as final cross-partition owner. |
| Player pet flags <-> pet Projectile AI | Direct reads and selected dead-path writes in Projectile; complete pet type set/callers not closed. | Capability availability and entity liveness must have a documented one-way command/query boundary; do not silently move flag writes. |
| Player mount/cart <-> Mount/Minecart | Source methods take and read/update shared Player runtime state. `mount` use query partial and budget-limited. | Cross-partition owner and update sequence remain `crossSubsystemOwner: integration-review`. |
| Progression/fishing/accessory -> save/network/event consumers | CPG has selected usage facts only; serialization methods are stubbed. | Edges, wire fields, save versions and commit visibility are `unknown`; do not claim a closed DAG. |

**Proposed DAG only:**

```text
authorized intent / event
  -> domain validation Query
  -> progression commit owner
  -> authoritative state
  -> persistence and network projections

tick begin/reset
  -> buff/equipment/content inputs
  -> fishing, minion, pet, accessory capability rebuilds
  -> immutable snapshots / read-only capability queries
  -> minion and pet admission/lifecycle coordination with Projectile owner
  -> owner-specific effects and one-way projections
```

上图中的跨 owner 依赖、barrier、buffer flush、事件注册顺序、网络可见时点和异常恢复尚未在目标 runtime 验证；所有该类边继续标 `proposed`/`unknown`。CPG 不展开 callee effects、事件订阅或动态调度，不能将索引关系画成闭合依赖图。

## Lifecycle and Side Effects

| 阶段 | 已知行为 | 未闭合部分 |
|---|---|---|
| create/load/activate | Version4 `Main.DoUpdateInWorld` 只对 active player 调用 `Player.Update`。Player 构造、连接和 ECS entity 初始化关系没有在本分区形成完整 caller 链。 | Entity 注册、session/slot 到 Player entity 映射、重连与恢复时点 `unknown`。 |
| per-tick reset/rebuild | `ResetEffects` 设置 minion baseline 并重置钓鱼/装备派生状态；buff/equipment 后续累加。 | 全部 164 字段的归零、保留、buff 输入和各方法异常路径没有完整闭合。 |
| minion spawn/despawn | Projectile 的 admission 检查 slot 与 limit；超限分支可 kill；成功分支写 Player counters。 | 同步命令、实体创建失败、跨 owner kill、重建时计数来源和多 Player/多世界隔离 unknown。 |
| pet/companion lifecycle | 若干 pet AI 以 `timeLeft` 延长 projectile 存活，且有死亡清除对应 flag 的分支。 | 其他 flag 类型、respawn 后是否恢复、重复命令、实体销毁回调、世界切换和 companion 关系清理 unknown。 |
| Mount/Minecart | `Mount.UpdateAfterEquips`、`UpdateEffects` 及 `Minecart.TrackCollision` 通过 Player 对象读写运行状态。 | 车辆状态的生命周期、与 Player Movement 的权威提交点、帧顺序和网络恢复交 integration review。 |
| persistence/network | `Player.SavePlayer` 有 wrapper，但 `InternalSavePlayerFile`、`Serialize`、`Deserialize` 和 `FixLoadedData` 是空体或 stub。 | P05 字段 save release/version gate、packet id、字段编码、加载校验、错误/回滚及发送顺序均 `unknown`。 |
| effects | 已读代码涉及 projectile 生存/kill、Player runtime counters 和 per-tick effect。 | 进度奖励、物品消耗、声音/视觉、网络和随机/时间依赖的完整副作用链未闭合。 |

因此不能把 P05 组件候选命名为“持久存档真相”，也不能把有 active Player tick 等同于单世界/单会话生命周期保证。

## Integration Handoff

下列 owner 结论全部保留给 `crossSubsystemOwner: integration-review`；本报告不替相邻分区定案。

| Seam | 必须共同确认的协议 |
|---|---|
| P03 Mount/Vehicle 与 Movement | `mount` runtime、cart flags、Mount/Minecart 对 Player 的写入、碰撞/移动提交顺序、typed snapshot 和 reset owner。 |
| ProjectileSimulation 与 P05 Minion/Pet | projectile create/kill、minion slot reserve/release、owner identity、pet heartbeat/death 清理、实体复制及回滚。 |
| Combat 与 Minion damage tracking | 两个 high-water 字段的权威来源、damage provenance、复用/幂等 token、重置和消费者。 |
| Items/Content/WorldProgression 与 Progression | consumed item 的授权与扣除、Biome Torches/Super Cart 解锁和偏好、Angler/Golfer/DD2 事件来源、奖励与进度 commit 的原子顺序。 |
| Buff/Equipment/Fishing 与 P05 capability rebuild | rods、bait、liquid/environment、equipment/modifier 和 bobber 的只读输入边界；refresh revision 与 tick visibility。 |
| Pet catalogs、WorldObject/Tile 与 Companion | pet 内容映射键、world-object pet资格、伙伴实体命令、tile/world identity 和清理责任。 |
| Network/WorldStorage/Player lifecycle | Player 快照版本、迁移校验、登录/重连/换世界/死亡恢复、packet 投影以及 commit 可见点。 |
| Luck/Movement/Combat/Environment 与 accessory effects | `brokenMirrorBadLuck`、boots/dash、whip/buff、magic cuffs、scope 等字段的实际 consumer、最终 effect owner 和单向投影。 |

SS14 `SharedResearchSystem` 只说明 `EntitySystem` 可以组合依赖与 API；不据此引入 Research 式注册表或事件，也不据此确认 Terraria component owner。

## Migration Behavior Contract

迁移验收应对照每个行为的**输入、状态差异、拒绝/错误、效果和顺序**，而不是比较字段数或 API 签名。下表均为后续验证计划，尚未执行。

| 行为 | 必须保留的旧/新观察向量 | 证据状态 |
|---|---|---|
| Consumed upgrade | 有效/无效/重复消费的 Player progression 状态、物品扣除、能力变化、奖励或事件；失败时无半提交。 | 输入来源、真实存档及效果 `unknown`。 |
| Unlock/preference | 解锁和 enabled preference 分别变化；生物群落火把/Super Cart 查询一致；旧库存 fallback、保存与网络 projection 对齐。 | 字段声明已确认，外部 preference 与持久化 unknown。 |
| Quest/event progress | Angler/Golfer/DD2 单次/重复/乱序输入、counter delta、reward callback 和 event order。 | 事件授权和提交先后 unknown。 |
| Fishing rebuild | 每 tick reset 基线、装备/buff 修正、所有 7 项 snapshot、下游 fishing eligibility；不带入旧 tick 值。 | 当前 source 有部分 reset/accumulation 证据，完整行为空间和 bobber integration partial。 |
| Minion capacity | 最大 slots、已有 minion 数/slots、接纳/拒绝、创建失败、销毁/重置和重复 delta 后 Player 与 Projectile 计数一致。 | 现状有 Player 与 Projectile 直接写入；原子提交 protocol unknown。 |
| Pet/companion | 各 flag 到 definition/entity 的映射、存活 refresh、死亡/销毁/重生/重连后的旗标与实体状态、重复同步无重复实体。 | 仅部分 Projectile 分支可见；全表映射和持久性 unknown。 |
| Vehicle/accessory effects | Mount/cart movement 和 collision、accessory reset/rebuild、受影响功能结果、网络/视觉投影及 tick 顺序。 | owner 与效果 consumer unknown/partial。 |
| Save/network | 新旧版本读取/写入、未知字段、拒绝/回滚、在线 replication 和重新登录观察结果。 | Version4 Player 序列化体被清空；暂无兼容结论。 |

迁移完成条件必须由实际 `src/NSSLC` 新 owner/API composition 被必需行为测试触达并通过；报告存在、代码可编译、verifier 文件存在或旧 facade 可调用都不够。当前没有可声称的行为等价或迁移成功。

## Evidence Gaps and Blocking Decisions

- Version4 P05 的 caller/writer closure 未由 API 或源码闭合。CPG 对关键调用有 partial/gap、字段访问有 `Unknown`；未扫完的动态入口、事件注册、alias 和外部写者保持 `unknown`。
- Player progression 字段是否持久、何时保存、如何版本化未知；当前 Version4 的 Player serialize/deserialize 实现已清空，不能推断恢复行为。
- Minion capacity 当前跨 Player reset/equipment 和 Projectile admission 修改；唯一 owner、reservation/rollback、失败时计数恢复及并行访问语义由 integration review 决策。
- Pet flags 是否表示永久解锁、当前召唤资格或活体跟随状态未知；现有 Projectile death branch 对部分 flag 直接清除，必须确认其与 respawn/save 的关系。
- Fishing/accessory inputs、更新 scheduler、snapshot refresh visibility、modifier stacking 顺序及 query 重复调用纯度尚未闭合。
- `mount` 的 CPG 使用结果达到 250 条 budget 并为 `Unknown`；`cartRampTime` query partial/zero-hit 不能作为不存在依赖的证明。车辆 owner 决策待 P03/Movement integration。
- 当前 NLTX 可见 progression/fishing/minion/accessory 候选实现缺少已定位的生产调度 caller；pet rebuild 和 persistence/network adapter 的当前接入路径未发现。静态搜索不是对源生成或外部 composition 的封闭性证明。
- 多世界隔离、重入、重复命令、登录/重连、换世界、异常恢复、协议兼容和删除旧 facade 均无本轮验证证据。

阻断后续“owner confirmed”结论的决定：Projectile 与 Player 哪一侧原子提交 capacity；pet flag 与实体生命周期的权威分界；P03/Mount 与 PlayerVehicleRuntime 状态边界；progression event/persistence/network 提交顺序；以及 Fishing/Accessory 派生 snapshot 的 tick 可见点。它们不能由 P05 单分区报告代替整合决策。

## Verification Plan

`verificationStatus: not-run`。本轮只做了文档、源码读取、文件 hash 和只读 CPG 查询；没有修改 `src/`、项目或测试，也没有运行 build、test 或 verifier。

后续获准实现后，verification 应至少包含：

1. 进度命令的有效/无效/重复/乱序输入、idempotency、奖励/事件顺序、save/load 与在线 projection。
2. Fishing 每 tick baseline/rebuild、装备/buff 叠加、重复 Query 稳定性、snapshot 失效和 bobber 接入。
3. Minion admission 的容量边界、重复 delta、并发/重入、Projectile create/kill/failure rollback、reset 与跨世界计数一致性。
4. 每类 pet/companion 的定义映射、flag/entity 同步、dead/despawn/respawn/reconnect/world unload、重复命令及不丢持久解锁。
5. Mount/Minecart movement/collision 行为、共享 Player 状态唯一提交和与 P03 的时序集成。
6. 17 个 accessory effects 的完整消费者映射、reset/rebuild 顺序、effect output owner 与预测/复制一致性。
7. 实际迁移项目的必需行为测试覆盖新 owner 和完整 API composition，再决定是否允许 legacy adapter 删除。

这些均为未执行计划；完成本报告只结算文档分区，不表示代码已迁移、编译通过、行为等价或迁移成功。
