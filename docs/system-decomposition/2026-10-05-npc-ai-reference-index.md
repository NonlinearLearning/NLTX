# NPC AI 参考源码索引

文档 ID：DOC-2026-10-05-NPC-AI-REFERENCE-INDEX  
逻辑域：system-decomposition  
产物类型：evidence  
状态：active  
范围：指定参考项目 NPC.AI 的全部分派入口与词法 helper 调用  
证据入口：D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs、Main.cs、Terraria.ID/NPCID.cs  
canonical 路径：docs/system-decomposition/2026-10-05-npc-ai-reference-index.md

## 证据口径

`NPC.cs` 共 97,097 行，`AI()` 位于 19997–43457。按该方法内顶层 `aiStyle == n`
分支与配对花括号抽取，确认 **128 个分支，连续 0–127，无重复和缺项**。
下表的类型示例来自 `SetDefaults` 中带 `type == 数字` 外层条件的赋值，并用 NPCID 常量命名；
它是候选类型线索，未解析特殊世界覆盖、继承默认值、类型集合条件和运行时风格改写。
“未得到示例”不表示分支不存在，也不表示该风格无用。

helper 列只列出分派分支直接调用的 `AI_*` 方法及声明行，不是完整传递调用闭包。
内联分支仍可能调用生成、伤害、碰撞等其他方法。**入口库存完整，不代表所有类型、分支、
效果、随机序列或联机行为已经迁移或证明等价。** 本文所有参考行为迁移状态均为未验证。
完整架构见 [AI 系统设计](2026-10-05-npc-ai-system-redesign.md)。

## 参考文件指纹

| 文件 | 行数 | SHA-256 |
| --- | ---: | --- |
| `Terraria/NPC.cs` | 97097 | `ed8aa2302730a9e046ef310e543204fba391bd35b1c9c0b213c8065dfbbe39f0` |
| `Terraria/Main.cs` | 67447 | `e24e61c9903bb7995f47e36c51edbe43481b643226861b717020877e7e22b63f` |
| `Terraria.ID/NPCID.cs` | 12529 | `e040b9cfffd57842c0099f11322ee5ec1dddd605743a25bae5415db31b0c025d` |

## 全部分派入口

行号绑定上面的文件指纹，范围包括分支条件至闭合括号。多个类型共享风格时只显示前三个示例。

| aiStyle | NPC.cs 分支行 | 直接 AI helper（声明行） | 默认类型候选示例 |
| ---: | --- | --- | --- |
| 0 | 20001–20120 | `AI_000_TransformBoundNPC` (45503) | BoundGoblin (105)、BoundWizard (106)、BoundMechanic (123) |
| 1 | 20121–20125 | `AI_001_Slimes` (61133) | BlueSlime (1)、MotherSlime (16)、LavaSlime (59) |
| 2 | 20126–20130 | `AI_002_FloatingEye` (53027) | DemonEye (2)、TheHungryII (116)、WanderingEye (133) |
| 3 | 20131–20135 | `AI_003_Fighters` (56637) | Zombie (3)、Skeleton (21)、GoblinPeon (26) |
| 4 | 20136–20985 | 内联 | EyeofCthulhu (4) |
| 5 | 20986–20990 | `AI_005_EaterOfSouls` (50966) | ServantofCthulhu (5)、EaterofSouls (6)、MeteorHead (23) |
| 6 | 20991–20995 | `AI_006_Worms` (51709) | DevourerHead (7)、DevourerBody (8)、DevourerTail (9) |
| 7 | 20996–21000 | `AI_007_TownEntities` (53740) | Merchant (17)、Nurse (18)、ArmsDealer (19) |
| 8 | 21001–21575 | `AI_AttemptToFindTeleportSpotNearBooks` (19166)<br>`AI_AttemptToFindTeleportSpot` (19092)<br>`AI_FindNearbyBook` (63141) | FireImp (24)、GoblinSorcerer (29)、DarkCaster (32) |
| 9 | 21576–21779 | 内联 | BurningSphere (25)、ChaosBall (30)、WaterSphere (33) |
| 10 | 21780–22130 | 内联 | CursedSkull (34)、GiantCursedSkull (289)、WaterBoltMimic (694) |
| 11 | 22131–22515 | 内联 | SkeletronHead (35)、DungeonGuardian (68) |
| 12 | 22516–22830 | 内联 | SkeletronHand (36) |
| 13 | 22831–23121 | 内联 | ManEater (43)、Snatcher (56)、Clinger (101) |
| 14 | 23122–23770 | 内联 | Harpy (48)、CaveBat (49)、JungleBat (51) |
| 15 | 23771–23775 | `AI_015_KingSlime` (43670) | KingSlime (50) |
| 16 | 23776–24305 | 内联 | Goldfish (55)、CorruptGoldfish (57)、Piranha (58) |
| 17 | 24306–24436 | 内联 | Vulture (61)、Raven (301) |
| 18 | 24437–24691 | 内联 | BlueJellyfish (63)、PinkJellyfish (64)、GreenJellyfish (103) |
| 19 | 24692–24822 | 内联 | Antlion (69) |
| 20 | 24823–24900 | 内联 | SpikeBall (70) |
| 21 | 24901–24952 | 内联 | BlazingWheel (72) |
| 22 | 24953–25542 | 内联 | Pixie (75)、Wraith (82)、Gastropod (122) |
| 23 | 25543–25622 | 内联 | CursedHammer (83)、EnchantedSword (84)、CrimsonAxe (179) |
| 24 | 25623–25847 | 内联 | Bird (74)、BirdBlue (297)、BirdRed (298) |
| 25 | 25848–25940 | 内联 | Mimic (85)、PresentMimic (341)、IceMimic (629) |
| 26 | 25941–25944 | `AI_026_Unicorns` (63201) | Unicorn (86)、Wolf (155)、HeadlessHorseman (315) |
| 27 | 25945–26367 | 内联 | WallofFlesh (113) |
| 28 | 26368–26525 | 内联 | WallofFleshEye (114) |
| 29 | 26526–26723 | 内联 | TheHungry (115) |
| 30 | 26724–27342 | 内联 | Retinazer (125) |
| 31 | 27343–27962 | 内联 | Spazmatism (126) |
| 32 | 27963–28281 | 内联 | SkeletronPrime (127) |
| 33 | 28282–28586 | 内联 | PrimeSaw (129) |
| 34 | 28587–28866 | 内联 | PrimeVice (130) |
| 35 | 28867–29102 | 内联 | PrimeCannon (128) |
| 36 | 29103–29337 | 内联 | PrimeLaser (131) |
| 37 | 29338–29341 | `AI_037_Destroyer` (50467) | TheDestroyer (134)、TheDestroyerBody (135)、TheDestroyerTail (136) |
| 38 | 29342–29483 | 内联 | SnowmanGangsta (143)、MisterStabby (144)、SnowBalla (145) |
| 39 | 29484–30010 | 内联 | GiantTortoise (153)、IceTortoise (154)、SolarSroller (417) |
| 40 | 30011–30243 | 内联 | WallCreeperWall (165)、JungleCreeperWall (237)、BlackRecluseWall (238) |
| 41 | 30244–30507 | 内联 | Herpling (174)、Derpling (177)、ChatteringTeethBomb (378) |
| 42 | 30508–30538 | 内联 | LostGirl (195) |
| 43 | 30539–31225 | 内联 | QueenBee (222) |
| 44 | 31226–31495 | 内联 | FlyingFish (224)、GiantFlyingAntlion (509)、FlyingAntlion (581) |
| 45 | 31496–31499 | `AI_045_Golem` (19677) | Golem (245) |
| 46 | 31500–31722 | 内联 | GolemHead (246) |
| 47 | 31723–31726 | `AI_047_GolemFist` (19399) | GolemFistLeft (247)、GolemFistRight (248) |
| 48 | 31727–31968 | 内联 | GolemHeadFree (249) |
| 49 | 31969–32033 | 内联 | AngryNimbus (250) |
| 50 | 32034–32099 | 内联 | FungiSpore (261)、Spore (265) |
| 51 | 32100–32464 | 内联 | Plantera (262) |
| 52 | 32465–32632 | 内联 | PlanterasHook (263) |
| 53 | 32633–32763 | 内联 | PlanterasTentacle (264) |
| 54 | 32764–33054 | 内联 | BrainofCthulhu (266) |
| 55 | 33055–33141 | 内联 | Creeper (267) |
| 56 | 33142–33163 | 内联 | DungeonSpirit (288) |
| 57 | 33164–33472 | 内联 | MourningWood (325)、Everscream (344) |
| 58 | 33473–33631 | 内联 | Pumpking (327) |
| 59 | 33632–33814 | 内联 | PumpkingBlade (328) |
| 60 | 33815–34128 | 内联 | IceQueen (345) |
| 61 | 34129–34381 | 内联 | SantaNK1 (346) |
| 62 | 34382–34434 | 内联 | ElfCopter (347) |
| 63 | 34435–34482 | 内联 | Flocko (352) |
| 64 | 34483–34712 | 内联 | Firefly (355)、LightningBug (358)、Lavafly (654) |
| 65 | 34713–34716 | `AI_065_Butterflies` (45517) | Butterfly (356)、GoldButterfly (444)、HellButterfly (653) |
| 66 | 34717–34812 | 内联 | Worm (357)、TruffleWorm (374)、GoldWorm (448) |
| 67 | 34813–35087 | 内联 | Snail (359)、GlowingSnail (360)、MagmaSnail (655) |
| 68 | 35088–35344 | 内联 | Duck2 (363)、DuckWhite2 (365)、Seagull2 (603) |
| 69 | 35345–35348 | `AI_069_DukeFishron` (49479) | DukeFishron (370) |
| 70 | 35349–35422 | 内联 | DetonatingBubble (371) |
| 71 | 35423–35544 | 内联 | Sharkron (372)、Sharkron2 (373) |
| 72 | 35545–35566 | 内联 | ForceBubble (384) |
| 73 | 35567–35678 | 内联 | MartianTurret (387) |
| 74 | 35679–35971 | 内联 | MartianDrone (388)、SolarCorite (418) |
| 75 | 35972–36548 | 内联 | ScutlixRider (390)、MartianSaucer (392)、MartianSaucerTurret (393) |
| 76 | 36549–37000 | 内联 | MartianSaucerCore (395) |
| 77 | 37001–37425 | 内联 | MoonLordCore (398) |
| 78 | 37426–37938 | 内联 | MoonLordHand (397) |
| 79 | 37939–38355 | 内联 | MoonLordHead (396) |
| 80 | 38356–38450 | 内联 | MartianProbe (399) |
| 81 | 38451–38894 | 内联 | MoonLordFreeEye (400) |
| 82 | 38895–39019 | 内联 | MoonLordLeechBlob (401) |
| 83 | 39020–39189 | 内联 | CultistTablet (437)、CultistDevote (438) |
| 84 | 39190–39193 | `AI_084_LunaticCultist` (65260) | CultistBoss (439)、CultistBossClone (440) |
| 85 | 39194–39487 | 内联 | StardustCellBig (405)、NebulaHeadcrab (421)、DeadlySphere (467) |
| 86 | 39488–39746 | 内联 | ShadowFlameApparition (472)、AncientCultistSquidhead (521) |
| 87 | 39747–40102 | `AI_87_BigMimic_FireStuffCannonBurst` (45420) | BigMimicCorruption (473)、BigMimicCrimson (474)、BigMimicHallow (475) |
| 88 | 40103–40633 | 内联 | Mothron (477) |
| 89 | 40634–40676 | 内联 | MothronEgg (478) |
| 90 | 40677–40912 | 内联 | MothronSpawn (479) |
| 91 | 40913–41097 | 内联 | GraniteFlyer (483) |
| 92 | 41098–41144 | 内联 | TargetDummy (488) |
| 93 | 41145–41255 | 内联 | PirateShip (491) |
| 94 | 41256–41671 | 内联 | LunarTowerVortex (422)、LunarTowerStardust (493)、LunarTowerNebula (507) |
| 95 | 41672–41719 | 内联 | StardustCellSmall (406) |
| 96 | 41720–41762 | 内联 | StardustJellyfishBig (407) |
| 97 | 41763–41912 | `AI_AttemptToFindTeleportSpot` (19092) | NebulaBrain (420) |
| 98 | 41913–42223 | 内联 | 未得到示例 |
| 99 | 42224–42290 | 内联 | SolarGoop (519) |
| 100 | 42291–42369 | 内联 | AncientLight (522) |
| 101 | 42370–42450 | 内联 | AncientDoom (523) |
| 102 | 42451–42847 | 内联 | SandElemental (541) |
| 103 | 42848–43033 | 内联 | SandShark (542)、SandsharkCorrupt (543)、SandsharkCrimson (544) |
| 104 | 43034–43037 | 内联 | DD2AttackerTest (547) |
| 105 | 43038–43289 | 内联 | DD2EterniaCrystal (548) |
| 106 | 43290–43372 | 内联 | DD2LanePortal (549) |
| 107 | 43373–43376 | `AI_107_ImprovedWalkers` (63766) | DD2GoblinT1 (552)、DD2GoblinT2 (553)、DD2GoblinT3 (554) |
| 108 | 43377–43380 | `AI_108_DivingFlyer` (66300) | DD2WyvernT1 (558)、DD2WyvernT2 (559)、DD2WyvernT3 (560) |
| 109 | 43381–43384 | `AI_109_DarkMage` (66705) | DD2DarkMageT1 (564)、DD2DarkMageT3 (565) |
| 110 | 43385–43388 | `AI_110_Betsy` (62670) | DD2Betsy (551) |
| 111 | 43389–43392 | `AI_111_DD2LightningBug` (67105) | DD2LightningBugT3 (578) |
| 112 | 43393–43396 | `AI_112_FairyCritter` (48779) | FairyCritterPink (583)、FairyCritterGreen (584)、FairyCritterBlue (585) |
| 113 | 43397–43400 | `AI_113_WindyBalloon` (48575) | WindyBalloon (594) |
| 114 | 43401–43404 | `AI_114_Dragonflies` (48400) | 未得到示例 |
| 115 | 43405–43408 | `AI_115_LadyBugs` (48262) | LadyBug (604)、GoldLadyBug (605)、Stinkbug (669) |
| 116 | 43409–43412 | `AI_116_WaterStriders` (48198) | WaterStrider (612)、GoldWaterStrider (613) |
| 117 | 43413–43416 | `AI_117_BloodNautilus` (47800) | BloodNautilus (618) |
| 118 | 43417–43420 | `AI_118_Seahorses` (47742) | Seahorse (626)、GoldSeahorse (627) |
| 119 | 43421–43424 | `AI_119_Dandelion` (47652) | Dandelion (628) |
| 120 | 43425–43428 | `AI_120_HallowBoss` (46601) | HallowBoss (636) |
| 121 | 43429–43432 | `AI_121_QueenSlime` (45835) | QueenSlimeBoss (657) |
| 122 | 43433–43436 | `AI_122_PirateGhost` (45459) | PirateGhost (662) |
| 123 | 43437–43440 | `AI_123_Deerclops` (44590) | Deerclops (668) |
| 124 | 43441–43444 | `AI_124_ElderSlimeChest` (44246) | 未得到示例 |
| 125 | 43445–43448 | `AI_125_ClumsySlimeBalloon` (44253) | BoundTownSlimePurple (686) |
| 126 | 43449–43452 | `AI_126_StatueMimic` (44000) | StatueMimic (690) |
| 127 | 43453–43456 | `AI_127_Pal` (43459) | PalworldCattivaDistressed (695)、PalworldFoxsparksDistressed (696) |

## 特殊事实与复现

- 风格 98 与 124 未在本次扫描中找到数值 `aiStyle = 98/124` 赋值；它们的分派入口实际存在。
- 风格 114 在 16838 赋值，但外层不是本次识别的数字类型等值条件；不能据此宣布类型覆盖完整。
- 风格 127 实际入口是 `AI_127_Pal`（43459）。`AI_127_Pal_SummonAttacker`（43575）
  在参考中只返回 0，且主流程直接调用 `NewNPC`。不把该占位 helper 当成已实现的召唤算法。
- `Main.cs:18113` 的槽位循环及 `NPC.cs:91880` 的更新入口，是 AI 之外调度设计的直接证据。

生成过程与机器可读结果保留在：

```text
python Build/diagnostics/NpcAiRedesign/index-reference.py
Build/diagnostics/NpcAiRedesign/reference-index.json
Build/diagnostics/NpcAiRedesign/reference-index.tsv
```

JSON 保留全部候选类型、默认赋值行和条件，以及直接 AI helper 的声明范围。
脚本只读参考源码，输出到 Build/diagnostics；上述生成物可重新生成，本文保留可评审的索引。
以后改变参考版本时必须重新计算指纹与行号，不复用本次行为证据。
