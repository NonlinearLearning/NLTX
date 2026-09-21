# P18 世界秘密种子、Skyblock、选项与地貌定义 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 12 个叶子子系统，字段 86 条、属性 50 条、成员合计 136 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `WorldGenerationSecretSeedFlags` | `WorldGenerationAndEcology` | 10 | 0 | 10 | authoritative state/behavior |
| `WorldSecretSeedRegistryDefinitions` | `WorldGenerationAndEcology` | 6 | 0 | 6 | registry/projection |
| `WorldSecretSeedVisualAndSurfaceRules` | `WorldGenerationAndEcology` | 9 | 0 | 9 | definition/query |
| `WorldSecretSeedTerrainAndStructureRules` | `WorldGenerationAndEcology` | 11 | 0 | 11 | definition/query |
| `WorldSecretSeedProgressionAndInfectionRules` | `WorldGenerationAndEcology` | 12 | 0 | 12 | definition/query |
| `WorldSecretSeedSeasonalRules` | `WorldGenerationAndEcology` | 3 | 0 | 3 | definition/query |
| `WorldSecretSeedRuntimeRegistry` | `WorldGenerationAndEcology` | 2 | 1 | 3 | authoritative state/behavior |
| `WorldSecretSeedDerivedOptions` | `WorldGenerationAndEcology` | 0 | 2 | 2 | derived/query |
| `WorldSecretSeedDerivedVariations` | `WorldGenerationAndEcology` | 0 | 22 | 22 | derived/query |
| `WorldSkyblockGenerationRules` | `WorldGenerationAndEcology` | 11 | 3 | 14 | definition/query |
| `WorldSeedOptionCatalog` | `WorldGenerationAndEcology` | 1 | 21 | 22 | definition/query |
| `WorldLandmassAndTreeProfiles` | `WorldGenerationAndEcology` | 21 | 1 | 22 | definition/query |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `WorldGenerationSecretSeedFlags` | `WorldGenerationSecretSeedFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSecretSeedRegistryDefinitions` | `WorldSecretSeedRegistryDefinitionsProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSecretSeedVisualAndSurfaceRules` | `WorldSecretSeedVisualAndSurfaceRulesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSecretSeedTerrainAndStructureRules` | `WorldSecretSeedTerrainAndStructureRulesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSecretSeedProgressionAndInfectionRules` | `WorldSecretSeedProgressionAndInfectionRulesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSecretSeedSeasonalRules` | `WorldSecretSeedSeasonalRulesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSecretSeedRuntimeRegistry` | `WorldSecretSeedRuntimeRegistryComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSecretSeedDerivedOptions` | `WorldSecretSeedDerivedOptionsQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSecretSeedDerivedVariations` | `WorldSecretSeedDerivedVariationsQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSkyblockGenerationRules` | `WorldSkyblockGenerationRulesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSeedOptionCatalog` | `WorldSeedOptionCatalogDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldLandmassAndTreeProfiles` | `WorldLandmassAndTreeProfilesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

## 4. 分区内依赖方向

- 输入、网络命令或世界配置先进入意图/资格 Query，再由本分区的唯一 Owner System 通过 Command/CommitPort 写入权威 Component。
- `derived/query` 和 `definition/query` 只能读权威状态或定义；`registry/projection`、快照、复制和表现边界只做单向输出。
- 跨分区交接使用只读 Query、显式 Command、Event 或 Projection；Markdown 文件顺序不表达运行时执行顺序。

## 5. 拆分前证据缺口

- 源报告确认了成员、声明类型、来源路径、行列、字段/属性及候选细分归属，但没有闭合每个成员的完整读者、写者、创建/清理/持久化/网络生命周期。
- `confirmed` 仅表示 `source-inventory-confirmed`；组件是否需要拆成多个结构、是否为缓存或兼容投影，必须在迁移前补充 Version4 调用点和 focused verifier 证据。
- 外部 SS14 证据只用于组件/System/Query 的组织粒度；tModLoader `v2026.07` 只用于公开 API 边界交叉参考，不替代 Version4 私有语义证据。

## 6. 兼容策略与验证计划

- 兼容策略：先保持 Version4 原始声明、公共 API、命名空间和外部类型边界不变；每次只迁移一个叶子边界，并以 Adapter/Projection 保留旧读路径，确认新 Owner System 的写入闭合后再移除兼容层。
- 暂不拆分项：源报告已经按声明类型、生命周期或访问边界分开的叶子组不再按字段数量机械切块；`Actions`、`WorkItem` 和短生命周期参数保持 Command payload，定义/catalog/profile/rule 保持只读 Definition/Query。
- focused verifier：权威状态验证状态转换、唯一写者、重复 Command 和清理边界；纯 Query 验证确定性与无写回；Projection/Adapter 验证单向输出；所有成员迁移批次验证来源序号、声明行和原始类型闭包。
- 验证状态：以上是迁移前计划；本分区只完成成员库存逐行一致性检查，未执行 C# 编译、运行时行为、网络复制或持久化恢复测试。

## 7. 逐成员源码声明

### 正式父级子系统：`WorldGenerationAndEcology`
- 父级职责：沿用源报告正式父级 `WorldGenerationAndEcology`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 86；属性 50；合计 136；完整父级统计以源报告为准。

#### 4.20.12 细分子系统：`WorldGenerationSecretSeedFlags`

- 细分职责：特殊世界种子和生成模式启用旗标。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；世界规则命令提交模式状态。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2532 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4306 | 2 | remixWorldGen | bool | `public static bool remixWorldGen = false;` | `public static bool remixWorldGen = false;` |
| 2533 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4308 | 2 | everythingWorldGen | bool | `public static bool everythingWorldGen = false;` | `public static bool everythingWorldGen = false;` |
| 2534 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4310 | 2 | noTrapsWorldGen | bool | `public static bool noTrapsWorldGen = false;` | `public static bool noTrapsWorldGen = false;` |
| 2535 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4312 | 2 | drunkWorldGen | bool | `public static bool drunkWorldGen = false;` | `public static bool drunkWorldGen = false;` |
| 2536 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4314 | 2 | getGoodWorldGen | bool | `public static bool getGoodWorldGen = false;` | `public static bool getGoodWorldGen = false;` |
| 2537 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4316 | 2 | tenthAnniversaryWorldGen | bool | `public static bool tenthAnniversaryWorldGen = false;` | `public static bool tenthAnniversaryWorldGen = false;` |
| 2538 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4318 | 2 | dontStarveWorldGen | bool | `public static bool dontStarveWorldGen = false;` | `public static bool dontStarveWorldGen = false;` |
| 2539 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4320 | 2 | notTheBees | bool | `public static bool notTheBees = false;` | `public static bool notTheBees = false;` |
| 2540 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4322 | 2 | skyblockWorldGen | bool | `public static bool skyblockWorldGen = false;` | `public static bool skyblockWorldGen = false;` |
| 2541 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4324 | 2 | drunkWorldGenText | bool | `public static bool drunkWorldGenText = false;` | `public static bool drunkWorldGenText = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.29 细分子系统：`WorldSecretSeedRegistryDefinitions`

- 细分职责：秘密种子注册集合、文本元数据和解锁输入。
- 边界角色：`registry/projection`；最小 seam：SecretSeed Registry/Definition view；注册表只读枚举定义。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2330 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 340 | 3 | AllSecretSeeds | System.Collections.Generic.List<Terraria.WorldGen.SecretSeed> | `public static List<SecretSeed> AllSecretSeeds = new List<SecretSeed>();` | `public static List<SecretSeed> AllSecretSeeds = new List<SecretSeed>();` |
| 2366 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 412 | 3 | Localization | string | `public readonly string Localization;` | `public readonly string Localization;` |
| 2367 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 414 | 3 | _code | string | `private readonly string _code;` | `private readonly string _code;` |
| 2368 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 416 | 3 | _sound | Terraria.Audio.LegacySoundStyle | `private readonly LegacySoundStyle _sound;` | `private readonly LegacySoundStyle _sound;` |
| 2369 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 418 | 3 | _plaintext | string | `private string _plaintext;` | `private string _plaintext;` |
| 2370 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 420 | 3 | TextThatWasUsedToUnlock | string | `public string TextThatWasUsedToUnlock;` | `public string TextThatWasUsedToUnlock;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.30 细分子系统：`WorldSecretSeedVisualAndSurfaceRules`

- 细分职责：涂色、表面、空间、降雨和冻结世界规则。
- 边界角色：`definition/query`；最小 seam：WorldGen Definition/Query；规则输入不持有运行时启用状态。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2331 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 342 | 3 | paintEverythingGray | Terraria.WorldGen.SecretSeed | `public static SecretSeed paintEverythingGray = Register("SecretSeedDescription.paintEverythingGray", SoundID.MenuAccept, "2htOIVagY/7JFx7acMpyUR6D3qJDr/u+");` | `public static SecretSeed paintEverythingGray = Register("SecretSeedDescription.paintEverythingGray", SoundID.MenuAccept, "2htOIVagY/7JFx7acMpyUR6D3qJDr/u+");` |
| 2332 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 344 | 3 | paintEverythingNegative | Terraria.WorldGen.SecretSeed | `public static SecretSeed paintEverythingNegative = Register("SecretSeedDescription.paintEverythingNegative", SoundID.MenuAccept, "YJayFFSdWEl66+rlFoWJRNvBHJi8gHnx");` | `public static SecretSeed paintEverythingNegative = Register("SecretSeedDescription.paintEverythingNegative", SoundID.MenuAccept, "YJayFFSdWEl66+rlFoWJRNvBHJi8gHnx");` |
| 2333 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 346 | 3 | coatEverythingEcho | Terraria.WorldGen.SecretSeed | `public static SecretSeed coatEverythingEcho = Register("SecretSeedDescription.coatEverythingEcho", SoundID.MenuAccept, "5Czr2vSNyB9hJd1yob+TYo0qqH/5U2P9");` | `public static SecretSeed coatEverythingEcho = Register("SecretSeedDescription.coatEverythingEcho", SoundID.MenuAccept, "5Czr2vSNyB9hJd1yob+TYo0qqH/5U2P9");` |
| 2334 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 348 | 3 | coatEverythingIlluminant | Terraria.WorldGen.SecretSeed | `public static SecretSeed coatEverythingIlluminant = Register("SecretSeedDescription.coatEverythingIlluminant", SoundID.MenuAccept, "5YXhKErRZovhjJkrP9fptrVHbNc1oSSn");` | `public static SecretSeed coatEverythingIlluminant = Register("SecretSeedDescription.coatEverythingIlluminant", SoundID.MenuAccept, "5YXhKErRZovhjJkrP9fptrVHbNc1oSSn");` |
| 2335 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 350 | 3 | noSurface | Terraria.WorldGen.SecretSeed | `public static SecretSeed noSurface = Register("SecretSeedDescription.noSurface", SoundID.MenuAccept, "cptECrPRxYeNTULJULs4gVoKdRsf3c3n");` | `public static SecretSeed noSurface = Register("SecretSeedDescription.noSurface", SoundID.MenuAccept, "cptECrPRxYeNTULJULs4gVoKdRsf3c3n");` |
| 2340 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 360 | 3 | surfaceIsInSpace | Terraria.WorldGen.SecretSeed | `public static SecretSeed surfaceIsInSpace = Register("SecretSeedDescription.surfaceIsInSpace", SoundID.MenuAccept, "io2s6kMi4L7ZCDYZGP1Hc8nEWuYW4gp5");` | `public static SecretSeed surfaceIsInSpace = Register("SecretSeedDescription.surfaceIsInSpace", SoundID.MenuAccept, "io2s6kMi4L7ZCDYZGP1Hc8nEWuYW4gp5");` |
| 2341 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 362 | 3 | rainsForAYear | Terraria.WorldGen.SecretSeed | `public static SecretSeed rainsForAYear = Register("SecretSeedDescription.rainsForAYear", SoundID.MenuAccept, "xYBNU5Soje9VhQHNQXETDKbwlc+7XZau");` | `public static SecretSeed rainsForAYear = Register("SecretSeedDescription.rainsForAYear", SoundID.MenuAccept, "xYBNU5Soje9VhQHNQXETDKbwlc+7XZau");` |
| 2354 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 388 | 3 | rainbowStuff | Terraria.WorldGen.SecretSeed | `public static SecretSeed rainbowStuff = Register("SecretSeedDescription.rainbowStuff", SoundID.MenuAccept, "6lK0Tn4t2UlklesGiJ94617yKvk01ICB");` | `public static SecretSeed rainbowStuff = Register("SecretSeedDescription.rainbowStuff", SoundID.MenuAccept, "6lK0Tn4t2UlklesGiJ94617yKvk01ICB");` |
| 2359 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 398 | 3 | worldIsFrozen | Terraria.WorldGen.SecretSeed | `public static SecretSeed worldIsFrozen = Register("SecretSeedDescription.worldIsFrozen", SoundID.MenuAccept, "eH2IYQwQyOud0hyoTPaeVsqYlAP7MvbS");` | `public static SecretSeed worldIsFrozen = Register("SecretSeedDescription.worldIsFrozen", SoundID.MenuAccept, "eH2IYQwQyOud0hyoTPaeVsqYlAP7MvbS");` |

##### 属性（0）

无该类型成员记录。


#### 4.20.31 细分子系统：`WorldSecretSeedTerrainAndStructureRules`

- 细分职责：地形、洞穴、结构、液体和传送器世界规则。
- 边界角色：`definition/query`；最小 seam：WorldGen Definition/Query；生成 pass 只读消费规则。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2336 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 352 | 3 | extraLivingTrees | Terraria.WorldGen.SecretSeed | `public static SecretSeed extraLivingTrees = Register("SecretSeedDescription.extraLivingTrees", SoundID.MenuAccept, "QQN1FbxlHeUCXPZc51GYvn8G5GXOJcny");` | `public static SecretSeed extraLivingTrees = Register("SecretSeedDescription.extraLivingTrees", SoundID.MenuAccept, "QQN1FbxlHeUCXPZc51GYvn8G5GXOJcny");` |
| 2337 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 354 | 3 | extraFloatingIslands | Terraria.WorldGen.SecretSeed | `public static SecretSeed extraFloatingIslands = Register("SecretSeedDescription.extraFloatingIslands", SoundID.MenuAccept, "0ebq4RCzI3PVaUPOT0f6/+vkXEaoLz2U");` | `public static SecretSeed extraFloatingIslands = Register("SecretSeedDescription.extraFloatingIslands", SoundID.MenuAccept, "0ebq4RCzI3PVaUPOT0f6/+vkXEaoLz2U");` |
| 2342 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 364 | 3 | biggerAbandonedHouses | Terraria.WorldGen.SecretSeed | `public static SecretSeed biggerAbandonedHouses = Register("SecretSeedDescription.biggerAbandonedHouses", SoundID.MenuAccept, "vWb/t7nNF+tnjgr5VgY2hi0HcT1j3kvC");` | `public static SecretSeed biggerAbandonedHouses = Register("SecretSeedDescription.biggerAbandonedHouses", SoundID.MenuAccept, "vWb/t7nNF+tnjgr5VgY2hi0HcT1j3kvC");` |
| 2344 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 368 | 3 | addTeleporters | Terraria.WorldGen.SecretSeed | `public static SecretSeed addTeleporters = Register("SecretSeedDescription.addTeleporters", SoundID.MenuAccept, "+URq9gxzcyHxAXVqdwl1fz8wgPYYu0Wx");` | `public static SecretSeed addTeleporters = Register("SecretSeedDescription.addTeleporters", SoundID.MenuAccept, "+URq9gxzcyHxAXVqdwl1fz8wgPYYu0Wx");` |
| 2352 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 384 | 3 | noSpiderCaves | Terraria.WorldGen.SecretSeed | `public static SecretSeed noSpiderCaves = Register("SecretSeedDescription.noSpiderCaves", SoundID.MenuAccept, "SPlOdka0fv8wUovao6u3VB7ZS+IbcPDu");` | `public static SecretSeed noSpiderCaves = Register("SecretSeedDescription.noSpiderCaves", SoundID.MenuAccept, "SPlOdka0fv8wUovao6u3VB7ZS+IbcPDu");` |
| 2353 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 386 | 3 | actuallyNoTraps | Terraria.WorldGen.SecretSeed | `public static SecretSeed actuallyNoTraps = Register("SecretSeedDescription.actuallyNoTraps", SoundID.MenuAccept, "AoEz0g1XX0V/nJwcaN2RWwUf/6ghr9pT");` | `public static SecretSeed actuallyNoTraps = Register("SecretSeedDescription.actuallyNoTraps", SoundID.MenuAccept, "AoEz0g1XX0V/nJwcaN2RWwUf/6ghr9pT");` |
| 2355 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 390 | 3 | digExtraHoles | Terraria.WorldGen.SecretSeed | `public static SecretSeed digExtraHoles = Register("SecretSeedDescription.digExtraHoles", SoundID.MenuAccept, "MucLvCERZix3rfcwUH68HDtuFYukiTv9");` | `public static SecretSeed digExtraHoles = Register("SecretSeedDescription.digExtraHoles", SoundID.MenuAccept, "MucLvCERZix3rfcwUH68HDtuFYukiTv9");` |
| 2356 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 392 | 3 | roundLandmasses | Terraria.WorldGen.SecretSeed | `public static SecretSeed roundLandmasses = Register("SecretSeedDescription.roundLandmasses", SoundID.MenuAccept, "VSN8nV180t6PgabWDl4Uf55I1vu97JRD");` | `public static SecretSeed roundLandmasses = Register("SecretSeedDescription.roundLandmasses", SoundID.MenuAccept, "VSN8nV180t6PgabWDl4Uf55I1vu97JRD");` |
| 2357 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 394 | 3 | extraLiquid | Terraria.WorldGen.SecretSeed | `public static SecretSeed extraLiquid = Register("SecretSeedDescription.extraLiquid", SoundID.MenuAccept, "ZYO3rUjSeCaaBrCE8Bv0FBtkjigLMz90");` | `public static SecretSeed extraLiquid = Register("SecretSeedDescription.extraLiquid", SoundID.MenuAccept, "ZYO3rUjSeCaaBrCE8Bv0FBtkjigLMz90");` |
| 2358 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 396 | 3 | portalGunInChests | Terraria.WorldGen.SecretSeed | `public static SecretSeed portalGunInChests = Register("SecretSeedDescription.portalGunInChests", SoundID.MenuAccept, "ALdQZ+bxQA4VdfjVfdhO/sm9q3sZD9dJ");` | `public static SecretSeed portalGunInChests = Register("SecretSeedDescription.portalGunInChests", SoundID.MenuAccept, "ALdQZ+bxQA4VdfjVfdhO/sm9q3sZD9dJ");` |
| 2365 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 410 | 3 | dualDungeons | Terraria.WorldGen.SecretSeed | `public static SecretSeed dualDungeons = Register("SecretSeedDescription.dualDungeons", SoundID.MenuAccept, "ypBuvKpqKay//OvhG2COriSpGT7f4YY3");` | `public static SecretSeed dualDungeons = Register("SecretSeedDescription.dualDungeons", SoundID.MenuAccept, "ypBuvKpqKay//OvhG2COriSpGT7f4YY3");` |

##### 属性（0）

无该类型成员记录。


#### 4.20.32 细分子系统：`WorldSecretSeedProgressionAndInfectionRules`

- 细分职责：进度、感染、出生点、难度和队伍生成规则。
- 边界角色：`definition/query`；最小 seam：WorldGen Definition/Query；进度规则不反向拥有世界结果。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2338 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 356 | 3 | errorWorld | Terraria.WorldGen.SecretSeed | `public static SecretSeed errorWorld = Register("SecretSeedDescription.errorWorld", SoundID.MenuAccept, "GkviuS3QN0pyESRJdjIs6oC8s8hOhUXw");` | `public static SecretSeed errorWorld = Register("SecretSeedDescription.errorWorld", SoundID.MenuAccept, "GkviuS3QN0pyESRJdjIs6oC8s8hOhUXw");` |
| 2339 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 358 | 3 | graveyardBloodmoonStart | Terraria.WorldGen.SecretSeed | `public static SecretSeed graveyardBloodmoonStart = Register("SecretSeedDescription.graveyardBloodmoonStart", SoundID.MenuAccept, "N8G20sWOkIa7ZP0rS/jopLpe9180N6Tx");` | `public static SecretSeed graveyardBloodmoonStart = Register("SecretSeedDescription.graveyardBloodmoonStart", SoundID.MenuAccept, "N8G20sWOkIa7ZP0rS/jopLpe9180N6Tx");` |
| 2343 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 366 | 3 | randomSpawn | Terraria.WorldGen.SecretSeed | `public static SecretSeed randomSpawn = Register("SecretSeedDescription.randomSpawn", SoundID.MenuAccept, "zSwnCH9E121+S6VQdB0k20E7IPdtobls");` | `public static SecretSeed randomSpawn = Register("SecretSeedDescription.randomSpawn", SoundID.MenuAccept, "zSwnCH9E121+S6VQdB0k20E7IPdtobls");` |
| 2345 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 370 | 3 | startInHardmode | Terraria.WorldGen.SecretSeed | `public static SecretSeed startInHardmode = Register("SecretSeedDescription.startInHardmode", SoundID.MenuAccept, "6kX2PJe0FWt3i0fp0tVBh5jt84ozLXBo");` | `public static SecretSeed startInHardmode = Register("SecretSeedDescription.startInHardmode", SoundID.MenuAccept, "6kX2PJe0FWt3i0fp0tVBh5jt84ozLXBo");` |
| 2346 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 372 | 3 | noInfection | Terraria.WorldGen.SecretSeed | `public static SecretSeed noInfection = Register("SecretSeedDescription.noInfection", SoundID.MenuAccept, "m1gQVuUnIRW083pnfFdnN3DPsg1qFYHZ");` | `public static SecretSeed noInfection = Register("SecretSeedDescription.noInfection", SoundID.MenuAccept, "m1gQVuUnIRW083pnfFdnN3DPsg1qFYHZ");` |
| 2347 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 374 | 3 | hallowOnTheSurface | Terraria.WorldGen.SecretSeed | `public static SecretSeed hallowOnTheSurface = Register("SecretSeedDescription.hallowOnTheSurface", SoundID.MenuAccept, "KYvKIk2LK0oyNY86m+uPhKQ7QbzFmDsR");` | `public static SecretSeed hallowOnTheSurface = Register("SecretSeedDescription.hallowOnTheSurface", SoundID.MenuAccept, "KYvKIk2LK0oyNY86m+uPhKQ7QbzFmDsR");` |
| 2348 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 376 | 3 | worldIsInfected | Terraria.WorldGen.SecretSeed | `public static SecretSeed worldIsInfected = Register("SecretSeedDescription.worldIsInfected", SoundID.MenuAccept, "kbxnychxHNDcoyFHhxM9OJHRxis6mFF/");` | `public static SecretSeed worldIsInfected = Register("SecretSeedDescription.worldIsInfected", SoundID.MenuAccept, "kbxnychxHNDcoyFHhxM9OJHRxis6mFF/");` |
| 2349 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 378 | 3 | surfaceIsMushrooms | Terraria.WorldGen.SecretSeed | `public static SecretSeed surfaceIsMushrooms = Register("SecretSeedDescription.surfaceIsMushrooms", SoundID.MenuAccept, "e48+tRi5DqzRkBPk3yq9udBG/kaYOQaB");` | `public static SecretSeed surfaceIsMushrooms = Register("SecretSeedDescription.surfaceIsMushrooms", SoundID.MenuAccept, "e48+tRi5DqzRkBPk3yq9udBG/kaYOQaB");` |
| 2350 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 380 | 3 | surfaceIsDesert | Terraria.WorldGen.SecretSeed | `public static SecretSeed surfaceIsDesert = Register("SecretSeedDescription.surfaceIsDesert", SoundID.MenuAccept, "eyGmBQhQ9QnE7UsIib1QmnNRVBNmQtMi");` | `public static SecretSeed surfaceIsDesert = Register("SecretSeedDescription.surfaceIsDesert", SoundID.MenuAccept, "eyGmBQhQ9QnE7UsIib1QmnNRVBNmQtMi");` |
| 2351 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 382 | 3 | pooEverywhere | Terraria.WorldGen.SecretSeed | `public static SecretSeed pooEverywhere = Register("SecretSeedDescription.pooEverywhere", SoundID.MenuAccept, "Iubz1XcBvsfPjSZucIJ3hCDFFEpjG57w");` | `public static SecretSeed pooEverywhere = Register("SecretSeedDescription.pooEverywhere", SoundID.MenuAccept, "Iubz1XcBvsfPjSZucIJ3hCDFFEpjG57w");` |
| 2363 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 406 | 3 | vampirism | Terraria.WorldGen.SecretSeed | `public static SecretSeed vampirism = Register("SecretSeedDescription.vampirism", SoundID.MenuAccept, "4eijvDtfcSl66CDifYSVP3WBZm9OLBoW");` | `public static SecretSeed vampirism = Register("SecretSeedDescription.vampirism", SoundID.MenuAccept, "4eijvDtfcSl66CDifYSVP3WBZm9OLBoW");` |
| 2364 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 408 | 3 | teamBasedSpawns | Terraria.WorldGen.SecretSeed | `public static SecretSeed teamBasedSpawns = Register("SecretSeedDescription.teamBasedSpawns", SoundID.MenuAccept, "HnTdmrZ5OT1ldA3r0w3dCgrdLnJBtBSD");` | `public static SecretSeed teamBasedSpawns = Register("SecretSeedDescription.teamBasedSpawns", SoundID.MenuAccept, "HnTdmrZ5OT1ldA3r0w3dCgrdLnJBtBSD");` |

##### 属性（0）

无该类型成员记录。


#### 4.20.33 细分子系统：`WorldSecretSeedSeasonalRules`

- 细分职责：万圣节和圣诞节季节生成规则。
- 边界角色：`definition/query`；最小 seam：WorldGen Definition/Query；季节规则按生成阶段读取。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2360 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 400 | 3 | halloweenGen | Terraria.WorldGen.SecretSeed | `public static SecretSeed halloweenGen = Register("SecretSeedDescription.halloweenGen", SoundID.MenuAccept, "Z4Odmvd5lScy/KGXHUO2nvqA9l3KRvm8");` | `public static SecretSeed halloweenGen = Register("SecretSeedDescription.halloweenGen", SoundID.MenuAccept, "Z4Odmvd5lScy/KGXHUO2nvqA9l3KRvm8");` |
| 2361 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 402 | 3 | endlessHalloween | Terraria.WorldGen.SecretSeed | `public static SecretSeed endlessHalloween = Register("SecretSeedDescription.endlessHalloween", SoundID.MenuAccept, "KNSxbK83ZXH41aUhWLti9OFMxoMrCV1s");` | `public static SecretSeed endlessHalloween = Register("SecretSeedDescription.endlessHalloween", SoundID.MenuAccept, "KNSxbK83ZXH41aUhWLti9OFMxoMrCV1s");` |
| 2362 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 404 | 3 | endlessChristmas | Terraria.WorldGen.SecretSeed | `public static SecretSeed endlessChristmas = Register("SecretSeedDescription.endlessChristmas", SoundID.MenuAccept, "gkN386qfe3u1qqQDpGsUu3DsRkEBpD1R");` | `public static SecretSeed endlessChristmas = Register("SecretSeedDescription.endlessChristmas", SoundID.MenuAccept, "gkN386qfe3u1qqQDpGsUu3DsRkEBpD1R");` |

##### 属性（0）

无该类型成员记录。


#### 4.20.34 细分子系统：`WorldSecretSeedRuntimeRegistry`

- 细分职责：秘密种子启用计数和运行时启用状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；启用状态由种子注册系统集中维护。
- 成员文件数：1；声明类型数：1；字段：2；属性：1；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2371 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 422 | 3 | activeSecretSeedCount | int | `private static int activeSecretSeedCount = 0;` | `private static int activeSecretSeedCount = 0;` |
| 2372 | field | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 424 | 3 | _enabled | bool | `private bool _enabled;` | `private bool _enabled;` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2651 | property | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 426 | 3 | Enabled | bool | `public bool Enabled => _enabled;` | `public bool Enabled => _enabled;` |


#### 4.20.35 细分子系统：`WorldSecretSeedDerivedOptions`

- 细分职责：由秘密种子与世界规则派生的生成选项属性。
- 边界角色：`derived/query`；最小 seam：纯资格 Query；派生属性不得写回注册状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：2；合计：2。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2652 | property | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 428 | 3 | GenerateBiggerAbandonedHouses | bool | `public static bool GenerateBiggerAbandonedHouses { get { if (!biggerAbandonedHouses.Enabled) { if (errorWorld.Enabled) { return genRand.Next(3) == 0; } return false; } return true; } }` | `public static bool GenerateBiggerAbandonedHouses { get { if (!biggerAbandonedHouses.Enabled) { if (errorWorld.Enabled) { return genRand.Next(3) == 0; } return false; } return true; } }` |
| 2653 | property | Terraria.WorldGen.SecretSeed | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 444 | 3 | GenerateRainbowGlowsticks | bool | `public static bool GenerateRainbowGlowsticks { get { if (!rainbowStuff.Enabled) { return Main.tenthAnniversaryWorld; } return true; } }` | `public static bool GenerateRainbowGlowsticks { get { if (!rainbowStuff.Enabled) { return Main.tenthAnniversaryWorld; } return true; } }` |


#### 4.20.36 细分子系统：`WorldSecretSeedDerivedVariations`

- 细分职责：由秘密种子组合派生的变体和规则资格属性。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；不直接修改 SecretSeed 注册状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：22；合计：22。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（22）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2629 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 44 | 4 | paintEverythingGrayJustTheSurface | bool | `public static bool paintEverythingGrayJustTheSurface { get { if (paintEverythingGray.Enabled && !paintEverythingGrayJustTreasure) { if (!paintEverythingNegative.Enabled && !coatEverythingEcho.Enabled) { return coatEverythingIlluminant.Enabled; } return true; } return false; } }` | `public static bool paintEverythingGrayJustTheSurface { get { if (paintEverythingGray.Enabled && !paintEverythingGrayJustTreasure) { if (!paintEverythingNegative.Enabled && !coatEverythingEcho.Enabled) { return coatEverythingIlluminant.Enabled; } return true; } return false; } }` |
| 2630 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 60 | 4 | paintEverythingGrayJustTreasure | bool | `public static bool paintEverythingGrayJustTreasure { get { if (paintEverythingGray.Enabled) { return activeSecretSeedCount >= 4; } return false; } }` | `public static bool paintEverythingGrayJustTreasure { get { if (paintEverythingGray.Enabled) { return activeSecretSeedCount >= 4; } return false; } }` |
| 2631 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 72 | 4 | paintEverythingGrayUseWhite | bool | `public static bool paintEverythingGrayUseWhite { get { if (paintEverythingGray.Enabled) { return worldIsFrozen.Enabled; } return false; } }` | `public static bool paintEverythingGrayUseWhite { get { if (paintEverythingGray.Enabled) { return worldIsFrozen.Enabled; } return false; } }` |
| 2632 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 84 | 4 | paintEverythingNegativeJustUnderground | bool | `public static bool paintEverythingNegativeJustUnderground { get { if (paintEverythingNegative.Enabled && !paintEverythingNegativeJustSomeThings) { if (!paintEverythingGray.Enabled && !coatEverythingEcho.Enabled) { return coatEverythingIlluminant.Enabled; } return true; } return false; } }` | `public static bool paintEverythingNegativeJustUnderground { get { if (paintEverythingNegative.Enabled && !paintEverythingNegativeJustSomeThings) { if (!paintEverythingGray.Enabled && !coatEverythingEcho.Enabled) { return coatEverythingIlluminant.Enabled; } return true; } return false; } }` |
| 2633 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 100 | 4 | paintEverythingNegativeJustSomeThings | bool | `public static bool paintEverythingNegativeJustSomeThings { get { if (paintEverythingNegative.Enabled) { return activeSecretSeedCount >= 4; } return false; } }` | `public static bool paintEverythingNegativeJustSomeThings { get { if (paintEverythingNegative.Enabled) { return activeSecretSeedCount >= 4; } return false; } }` |
| 2634 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 112 | 4 | coatEverythingJustInnerBlocks | bool | `public static bool coatEverythingJustInnerBlocks { get { if (coatEverythingEcho.Enabled && !coatEverythingEchoJustSomeThings) { if (!paintEverythingGray.Enabled && !paintEverythingNegative.Enabled) { return activeSecretSeedCount >= 3; } return true; } return false; } }` | `public static bool coatEverythingJustInnerBlocks { get { if (coatEverythingEcho.Enabled && !coatEverythingEchoJustSomeThings) { if (!paintEverythingGray.Enabled && !paintEverythingNegative.Enabled) { return activeSecretSeedCount >= 3; } return true; } return false; } }` |
| 2635 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 128 | 4 | coatEverythingEchoJustSomeThings | bool | `public static bool coatEverythingEchoJustSomeThings { get { if (coatEverythingEcho.Enabled) { return activeSecretSeedCount >= 4; } return false; } }` | `public static bool coatEverythingEchoJustSomeThings { get { if (coatEverythingEcho.Enabled) { return activeSecretSeedCount >= 4; } return false; } }` |
| 2636 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 140 | 4 | coatEverythingIlluminantJustRandomSpots | bool | `public static bool coatEverythingIlluminantJustRandomSpots { get { if (!coatEverythingIlluminantJustSomeThings) { return coatEverythingEcho.Enabled; } return false; } }` | `public static bool coatEverythingIlluminantJustRandomSpots { get { if (!coatEverythingIlluminantJustSomeThings) { return coatEverythingEcho.Enabled; } return false; } }` |
| 2637 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 152 | 4 | coatEverythingIlluminantJustSomeThings | bool | `public static bool coatEverythingIlluminantJustSomeThings { get { if (coatEverythingEcho.Enabled) { if (activeSecretSeedCount < 3 && !paintEverythingGray.Enabled) { return paintEverythingNegative.Enabled; } return true; } return false; } }` | `public static bool coatEverythingIlluminantJustSomeThings { get { if (coatEverythingEcho.Enabled) { if (activeSecretSeedCount < 3 && !paintEverythingGray.Enabled) { return paintEverythingNegative.Enabled; } return true; } return false; } }` |
| 2638 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 168 | 4 | noSurfaceNoFloatingIslands | bool | `public static bool noSurfaceNoFloatingIslands { get { if (noSurface.Enabled && !errorWorld.Enabled) { return !extraFloatingIslands.Enabled; } return false; } }` | `public static bool noSurfaceNoFloatingIslands { get { if (noSurface.Enabled && !errorWorld.Enabled) { return !extraFloatingIslands.Enabled; } return false; } }` |
| 2639 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 180 | 4 | noSurfaceNoLivingTrees | bool | `public static bool noSurfaceNoLivingTrees { get { if (noSurface.Enabled && !errorWorld.Enabled) { return !extraLivingTrees.Enabled; } return false; } }` | `public static bool noSurfaceNoLivingTrees { get { if (noSurface.Enabled && !errorWorld.Enabled) { return !extraLivingTrees.Enabled; } return false; } }` |
| 2640 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 192 | 4 | noSurfaceNoPyramids | bool | `public static bool noSurfaceNoPyramids { get { if (noSurface.Enabled) { return !errorWorld.Enabled; } return false; } }` | `public static bool noSurfaceNoPyramids { get { if (noSurface.Enabled) { return !errorWorld.Enabled; } return false; } }` |
| 2641 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 204 | 4 | noSurfaceNoSwordShrines | bool | `public static bool noSurfaceNoSwordShrines { get { if (noSurface.Enabled) { return !errorWorld.Enabled; } return false; } }` | `public static bool noSurfaceNoSwordShrines { get { if (noSurface.Enabled) { return !errorWorld.Enabled; } return false; } }` |
| 2642 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 216 | 4 | extraLivingTreesReducedAmount | bool | `public static bool extraLivingTreesReducedAmount { get { if (extraLivingTrees.Enabled) { if (activeSecretSeedCount < 6) { return noSurface.Enabled; } return true; } return false; } }` | `public static bool extraLivingTreesReducedAmount { get { if (extraLivingTrees.Enabled) { if (activeSecretSeedCount < 6) { return noSurface.Enabled; } return true; } return false; } }` |
| 2643 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 232 | 4 | extraFloatingIslandsNormalAmount | bool | `public static bool extraFloatingIslandsNormalAmount { get { if (extraFloatingIslands.Enabled) { return Main.skyblockWorld; } return false; } }` | `public static bool extraFloatingIslandsNormalAmount { get { if (extraFloatingIslands.Enabled) { return Main.skyblockWorld; } return false; } }` |
| 2644 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 244 | 4 | extraFloatingIslandsReducedAmount | bool | `public static bool extraFloatingIslandsReducedAmount { get { if (!extraFloatingIslands.Enabled \|\| activeSecretSeedCount < 6) { return noSurface.Enabled; } return true; } }` | `public static bool extraFloatingIslandsReducedAmount { get { if (!extraFloatingIslands.Enabled \|\| activeSecretSeedCount < 6) { return noSurface.Enabled; } return true; } }` |
| 2645 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 256 | 4 | errorWorldBalancedChests | bool | `public static bool errorWorldBalancedChests { get { if (errorWorld.Enabled) { return activeSecretSeedCount >= 6; } return false; } }` | `public static bool errorWorldBalancedChests { get { if (errorWorld.Enabled) { return activeSecretSeedCount >= 6; } return false; } }` |
| 2646 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 268 | 4 | noSpiderCavesActuallyNoSpiderCaves | bool | `public static bool noSpiderCavesActuallyNoSpiderCaves { get { if (noSpiderCaves.Enabled) { return activeSecretSeedCount < 4; } return false; } }` | `public static bool noSpiderCavesActuallyNoSpiderCaves { get { if (noSpiderCaves.Enabled) { return activeSecretSeedCount < 4; } return false; } }` |
| 2647 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 280 | 4 | noSpiderCavesILiedMoreSpiderCaves | bool | `public static bool noSpiderCavesILiedMoreSpiderCaves { get { if (noSpiderCaves.Enabled) { return activeSecretSeedCount >= 4; } return false; } }` | `public static bool noSpiderCavesILiedMoreSpiderCaves { get { if (noSpiderCaves.Enabled) { return activeSecretSeedCount >= 4; } return false; } }` |
| 2648 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 292 | 4 | actuallyNoTrapsForRealIMeanIt | bool | `public static bool actuallyNoTrapsForRealIMeanIt { get { if (actuallyNoTraps.Enabled) { return activeSecretSeedCount < 4; } return false; } }` | `public static bool actuallyNoTrapsForRealIMeanIt { get { if (actuallyNoTraps.Enabled) { return activeSecretSeedCount < 4; } return false; } }` |
| 2649 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 304 | 4 | surfaceIsDesertNormalFunction | bool | `public static bool surfaceIsDesertNormalFunction { get { if (surfaceIsDesert.Enabled) { return !surfaceIsDesertSwapDesertAndSnowBiomes; } return false; } }` | `public static bool surfaceIsDesertNormalFunction { get { if (surfaceIsDesert.Enabled) { return !surfaceIsDesertSwapDesertAndSnowBiomes; } return false; } }` |
| 2650 | property | Terraria.WorldGen.SecretSeed.Variations | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 316 | 4 | surfaceIsDesertSwapDesertAndSnowBiomes | bool | `public static bool surfaceIsDesertSwapDesertAndSnowBiomes { get { if (surfaceIsDesert.Enabled) { return noSurface.Enabled; } return false; } }` | `public static bool surfaceIsDesertSwapDesertAndSnowBiomes { get { if (surfaceIsDesert.Enabled) { return noSurface.Enabled; } return false; } }` |


#### 4.20.37 细分子系统：`WorldSkyblockGenerationRules`

- 细分职责：Skyblock 世界生成限制、Tile/Wall 规则和生成拒绝属性。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Rule view；生成 System 通过 Query 消费。
- 成员文件数：1；声明类型数：1；字段：11；属性：3；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2373 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3143 | 3 | noAltars | bool | `public static bool noAltars = false;` | `public static bool noAltars = false;` |
| 2374 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3145 | 3 | noDungeon | bool | `public static bool noDungeon = false;` | `public static bool noDungeon = false;` |
| 2375 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3147 | 3 | noTemple | bool | `public static bool noTemple = false;` | `public static bool noTemple = false;` |
| 2376 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3149 | 3 | noHellstone | bool | `public static bool noHellstone = false;` | `public static bool noHellstone = false;` |
| 2377 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3151 | 3 | noFossils | bool | `public static bool noFossils = false;` | `public static bool noFossils = false;` |
| 2378 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3153 | 3 | noLifeCrystals | bool | `public static bool noLifeCrystals = false;` | `public static bool noLifeCrystals = false;` |
| 2379 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3155 | 3 | noHellforge | bool | `public static bool noHellforge = false;` | `public static bool noHellforge = false;` |
| 2380 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3157 | 3 | lowTiles | bool | `public static bool lowTiles = false;` | `public static bool lowTiles = false;` |
| 2381 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3159 | 3 | hasTile | bool[] | `public static bool[] hasTile = new bool[TileID.Count];` | `public static bool[] hasTile = new bool[TileID.Count];` |
| 2382 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3161 | 3 | hasWall | bool[] | `public static bool[] hasWall = new bool[WallID.Count];` | `public static bool[] hasWall = new bool[WallID.Count];` |
| 2383 | field | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3163 | 3 | currentActiveTiles | int | `public static int currentActiveTiles = 0;` | `public static int currentActiveTiles = 0;` |

##### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2654 | property | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3165 | 3 | denyFloatingIslands | bool | `public static bool denyFloatingIslands { get { if (skyblockWorldGen) { return !SecretSeed.extraFloatingIslands.Enabled; } return false; } }` | `public static bool denyFloatingIslands { get { if (skyblockWorldGen) { return !SecretSeed.extraFloatingIslands.Enabled; } return false; } }` |
| 2655 | property | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3177 | 3 | denyAllGeneration | bool | `public static bool denyAllGeneration => skyblockWorldGen;` | `public static bool denyAllGeneration => skyblockWorldGen;` |
| 2656 | property | Terraria.WorldGen.Skyblock | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3179 | 3 | denySomeGeneration | bool | `public static bool denySomeGeneration { get { if (skyblockWorldGen) { if (!SecretSeed.worldIsFrozen.Enabled && !SecretSeed.surfaceIsDesert.Enabled && !SecretSeed.surfaceIsMushrooms.Enabled && !SecretSeed.worldIsInfected.Enabled && !SecretSeed.hallowOnTheSurface.Enabled && !SecretSeed.noInfection.Enabled && !SecretSeed.extraFloatingIslands.Enabled && !SecretSeed.extraLiquid.Enabled) { return !SecretSeed.extraLivingTrees.Enabled; } return false; } return false; } }` | `public static bool denySomeGeneration { get { if (skyblockWorldGen) { if (!SecretSeed.worldIsFrozen.Enabled && !SecretSeed.surfaceIsDesert.Enabled && !SecretSeed.surfaceIsMushrooms.Enabled && !SecretSeed.worldIsInfected.Enabled && !SecretSeed.hallowOnTheSurface.Enabled && !SecretSeed.noInfection.Enabled && !SecretSeed.extraFloatingIslands.Enabled && !SecretSeed.extraLiquid.Enabled) { return !SecretSeed.extraLivingTrees.Enabled; } return false; } return false; } }` |


#### 4.20.46 细分子系统：`WorldSeedOptionCatalog`

- 细分职责：各类世界种子选项及其依赖定义。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；生成阶段通过资格 Query 消费。
- 成员文件数：10；声明类型数：10；字段：1；属性：21；合计：22。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2329 | field | Terraria.WorldBuilding.WorldSeedOption_Everything | Terraria.WorldBuilding/WorldSeedOption_Everything.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Everything.cs | 10 | 2 | _dependencies | System.Collections.Generic.List<Terraria.WorldBuilding.AWorldGenerationOption> | `protected List<AWorldGenerationOption> _dependencies;` | `protected List<AWorldGenerationOption> _dependencies;` |

##### 属性（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2608 | property | Terraria.WorldBuilding.WorldSeedOption_Anniversary | Terraria.WorldBuilding/WorldSeedOption_Anniversary.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Anniversary.cs | 5 | 2 | KeyName | string | `protected override string KeyName => "Seed_Celebration";` | `protected override string KeyName => "Seed_Celebration";` |
| 2609 | property | Terraria.WorldBuilding.WorldSeedOption_Anniversary | Terraria.WorldBuilding/WorldSeedOption_Anniversary.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Anniversary.cs | 7 | 2 | ServerConfigName | string | `public override string ServerConfigName => "celebration";` | `public override string ServerConfigName => "celebration";` |
| 2610 | property | Terraria.WorldBuilding.WorldSeedOption_DontStarve | Terraria.WorldBuilding/WorldSeedOption_DontStarve.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_DontStarve.cs | 5 | 2 | KeyName | string | `protected override string KeyName => "Seed_TheConstant";` | `protected override string KeyName => "Seed_TheConstant";` |
| 2611 | property | Terraria.WorldBuilding.WorldSeedOption_DontStarve | Terraria.WorldBuilding/WorldSeedOption_DontStarve.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_DontStarve.cs | 7 | 2 | ServerConfigName | string | `public override string ServerConfigName => "theconstant";` | `public override string ServerConfigName => "theconstant";` |
| 2612 | property | Terraria.WorldBuilding.WorldSeedOption_Drunk | Terraria.WorldBuilding/WorldSeedOption_Drunk.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Drunk.cs | 5 | 2 | KeyName | string | `protected override string KeyName => "Seed_Drunk";` | `protected override string KeyName => "Seed_Drunk";` |
| 2613 | property | Terraria.WorldBuilding.WorldSeedOption_Drunk | Terraria.WorldBuilding/WorldSeedOption_Drunk.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Drunk.cs | 7 | 2 | ServerConfigName | string | `public override string ServerConfigName => "drunk";` | `public override string ServerConfigName => "drunk";` |
| 2614 | property | Terraria.WorldBuilding.WorldSeedOption_Everything | Terraria.WorldBuilding/WorldSeedOption_Everything.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Everything.cs | 12 | 2 | KeyName | string | `protected override string KeyName => "Seed_Everything";` | `protected override string KeyName => "Seed_Everything";` |
| 2615 | property | Terraria.WorldBuilding.WorldSeedOption_Everything | Terraria.WorldBuilding/WorldSeedOption_Everything.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Everything.cs | 14 | 2 | ServerConfigName | string | `public override string ServerConfigName => "zenith";` | `public override string ServerConfigName => "zenith";` |
| 2616 | property | Terraria.WorldBuilding.WorldSeedOption_Everything | Terraria.WorldBuilding/WorldSeedOption_Everything.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Everything.cs | 16 | 2 | Dependencies | System.Collections.Generic.List<Terraria.WorldBuilding.AWorldGenerationOption> | `public List<AWorldGenerationOption> Dependencies { get { if (_dependencies == null) { _dependencies = new List<AWorldGenerationOption> { WorldGenerationOptions.Get<WorldSeedOption_Remix>(), WorldGenerationOptions.Get<WorldSeedOption_Drunk>(), WorldGenerationOptions.Get<WorldSeedOption_NotTheBees>(), WorldGenerationOptions.Get<WorldSeedOption_NoTraps>(), WorldGenerationOptions.Get<WorldSeedOption_DontStarve>(), WorldGenerationOptions.Get<WorldSeedOption_Anniversary>(), WorldGenerationOptions.Get<WorldSeedOption_ForTheWorthy>() }; } return _dependencies; } }` | `public List<AWorldGenerationOption> Dependencies { get { if (_dependencies == null) { _dependencies = new List<AWorldGenerationOption> { WorldGenerationOptions.Get<WorldSeedOption_Remix>(), WorldGenerationOptions.Get<WorldSeedOption_Drunk>(), WorldGenerationOptions.Get<WorldSeedOption_NotTheBees>(), WorldGenerationOptions.Get<WorldSeedOption_NoTraps>(), WorldGenerationOptions.Get<WorldSeedOption_DontStarve>(), WorldGenerationOptions.Get<WorldSeedOption_Anniversary>(), WorldGenerationOptions.Get<WorldSeedOption_ForTheWorthy>() }; } return _dependencies; } }` |
| 2617 | property | Terraria.WorldBuilding.WorldSeedOption_ForTheWorthy | Terraria.WorldBuilding/WorldSeedOption_ForTheWorthy.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_ForTheWorthy.cs | 5 | 2 | KeyName | string | `protected override string KeyName => "Seed_ForTheWorthy";` | `protected override string KeyName => "Seed_ForTheWorthy";` |
| 2618 | property | Terraria.WorldBuilding.WorldSeedOption_ForTheWorthy | Terraria.WorldBuilding/WorldSeedOption_ForTheWorthy.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_ForTheWorthy.cs | 7 | 2 | ServerConfigName | string | `public override string ServerConfigName => "fortheworthy";` | `public override string ServerConfigName => "fortheworthy";` |
| 2619 | property | Terraria.WorldBuilding.WorldSeedOption_Normal | Terraria.WorldBuilding/WorldSeedOption_Normal.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Normal.cs | 7 | 2 | KeyName | string | `protected override string KeyName => "Seed_Normal";` | `protected override string KeyName => "Seed_Normal";` |
| 2620 | property | Terraria.WorldBuilding.WorldSeedOption_Normal | Terraria.WorldBuilding/WorldSeedOption_Normal.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Normal.cs | 9 | 2 | ServerConfigName | string | `public override string ServerConfigName => null;` | `public override string ServerConfigName => null;` |
| 2621 | property | Terraria.WorldBuilding.WorldSeedOption_NoTraps | Terraria.WorldBuilding/WorldSeedOption_NoTraps.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_NoTraps.cs | 5 | 2 | KeyName | string | `protected override string KeyName => "Seed_NoTraps";` | `protected override string KeyName => "Seed_NoTraps";` |
| 2622 | property | Terraria.WorldBuilding.WorldSeedOption_NoTraps | Terraria.WorldBuilding/WorldSeedOption_NoTraps.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_NoTraps.cs | 7 | 2 | ServerConfigName | string | `public override string ServerConfigName => "notraps";` | `public override string ServerConfigName => "notraps";` |
| 2623 | property | Terraria.WorldBuilding.WorldSeedOption_NotTheBees | Terraria.WorldBuilding/WorldSeedOption_NotTheBees.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_NotTheBees.cs | 5 | 2 | KeyName | string | `protected override string KeyName => "Seed_NotTheBees";` | `protected override string KeyName => "Seed_NotTheBees";` |
| 2624 | property | Terraria.WorldBuilding.WorldSeedOption_NotTheBees | Terraria.WorldBuilding/WorldSeedOption_NotTheBees.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_NotTheBees.cs | 7 | 2 | ServerConfigName | string | `public override string ServerConfigName => "notthebees";` | `public override string ServerConfigName => "notthebees";` |
| 2625 | property | Terraria.WorldBuilding.WorldSeedOption_Remix | Terraria.WorldBuilding/WorldSeedOption_Remix.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Remix.cs | 5 | 2 | KeyName | string | `protected override string KeyName => "Seed_Remix";` | `protected override string KeyName => "Seed_Remix";` |
| 2626 | property | Terraria.WorldBuilding.WorldSeedOption_Remix | Terraria.WorldBuilding/WorldSeedOption_Remix.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Remix.cs | 7 | 2 | ServerConfigName | string | `public override string ServerConfigName => "remix";` | `public override string ServerConfigName => "remix";` |
| 2627 | property | Terraria.WorldBuilding.WorldSeedOption_Skyblock | Terraria.WorldBuilding/WorldSeedOption_Skyblock.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Skyblock.cs | 5 | 2 | KeyName | string | `protected override string KeyName => "Seed_Skyblock";` | `protected override string KeyName => "Seed_Skyblock";` |
| 2628 | property | Terraria.WorldBuilding.WorldSeedOption_Skyblock | Terraria.WorldBuilding/WorldSeedOption_Skyblock.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldSeedOption_Skyblock.cs | 7 | 2 | ServerConfigName | string | `public override string ServerConfigName => "skyblock";` | `public override string ServerConfigName => "skyblock";` |


#### 4.20.47 细分子系统：`WorldLandmassAndTreeProfiles`

- 细分职责：地貌形状、树木生长配置和树木 profile 定义。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Profile view；生成 pass 通过值读取。
- 成员文件数：2；声明类型数：3；字段：21；属性：1；合计：22。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2235 | field | Terraria.WorldBuilding.LandmassData | Terraria.WorldBuilding/LandmassData.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\LandmassData.cs | 7 | 2 | DataType | Terraria.WorldBuilding.LandmassDataType | `public LandmassDataType DataType;` | `public LandmassDataType DataType;` |
| 2236 | field | Terraria.WorldBuilding.LandmassData | Terraria.WorldBuilding/LandmassData.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\LandmassData.cs | 9 | 2 | Position | Vector2 | `public Vector2 Position;` | `public Vector2 Position;` |
| 2237 | field | Terraria.WorldBuilding.LandmassData | Terraria.WorldBuilding/LandmassData.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\LandmassData.cs | 11 | 2 | RadiusOrHalfSize | int | `public int RadiusOrHalfSize;` | `public int RadiusOrHalfSize;` |
| 2238 | field | Terraria.WorldBuilding.LandmassData | Terraria.WorldBuilding/LandmassData.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\LandmassData.cs | 13 | 2 | Style | int | `public int Style;` | `public int Style;` |
| 2391 | field | Terraria.WorldGen.GrowTreeSettings.Profiles | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3842 | 4 | GemTree_Ruby | Terraria.WorldGen.GrowTreeSettings | `public static GrowTreeSettings GemTree_Ruby = new GrowTreeSettings  			{  				GroundTest = GemTreeGroundTest,  				WallTest = GemTreeWallTest,  				TreeHeightMax = 12,  				TreeHeightMin = 7,  				TreeTileType = 587,  				TreeTopPaddingNeeded = 4,  				SaplingTileType = 590  			};` | `public static GrowTreeSettings GemTree_Ruby = new GrowTreeSettings { GroundTest = GemTreeGroundTest, WallTest = GemTreeWallTest, TreeHeightMax = 12, TreeHeightMin = 7, TreeTileType = 587, TreeTopPaddingNeeded = 4, SaplingTileType = 590 };` |
| 2392 | field | Terraria.WorldGen.GrowTreeSettings.Profiles | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3853 | 4 | GemTree_Diamond | Terraria.WorldGen.GrowTreeSettings | `public static GrowTreeSettings GemTree_Diamond = new GrowTreeSettings  			{  				GroundTest = GemTreeGroundTest,  				WallTest = GemTreeWallTest,  				TreeHeightMax = 12,  				TreeHeightMin = 7,  				TreeTileType = 588,  				TreeTopPaddingNeeded = 4,  				SaplingTileType = 590  			};` | `public static GrowTreeSettings GemTree_Diamond = new GrowTreeSettings { GroundTest = GemTreeGroundTest, WallTest = GemTreeWallTest, TreeHeightMax = 12, TreeHeightMin = 7, TreeTileType = 588, TreeTopPaddingNeeded = 4, SaplingTileType = 590 };` |
| 2393 | field | Terraria.WorldGen.GrowTreeSettings.Profiles | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3864 | 4 | GemTree_Topaz | Terraria.WorldGen.GrowTreeSettings | `public static GrowTreeSettings GemTree_Topaz = new GrowTreeSettings  			{  				GroundTest = GemTreeGroundTest,  				WallTest = GemTreeWallTest,  				TreeHeightMax = 12,  				TreeHeightMin = 7,  				TreeTileType = 583,  				TreeTopPaddingNeeded = 4,  				SaplingTileType = 590  			};` | `public static GrowTreeSettings GemTree_Topaz = new GrowTreeSettings { GroundTest = GemTreeGroundTest, WallTest = GemTreeWallTest, TreeHeightMax = 12, TreeHeightMin = 7, TreeTileType = 583, TreeTopPaddingNeeded = 4, SaplingTileType = 590 };` |
| 2394 | field | Terraria.WorldGen.GrowTreeSettings.Profiles | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3875 | 4 | GemTree_Amethyst | Terraria.WorldGen.GrowTreeSettings | `public static GrowTreeSettings GemTree_Amethyst = new GrowTreeSettings  			{  				GroundTest = GemTreeGroundTest,  				WallTest = GemTreeWallTest,  				TreeHeightMax = 12,  				TreeHeightMin = 7,  				TreeTileType = 584,  				TreeTopPaddingNeeded = 4,  				SaplingTileType = 590  			};` | `public static GrowTreeSettings GemTree_Amethyst = new GrowTreeSettings { GroundTest = GemTreeGroundTest, WallTest = GemTreeWallTest, TreeHeightMax = 12, TreeHeightMin = 7, TreeTileType = 584, TreeTopPaddingNeeded = 4, SaplingTileType = 590 };` |
| 2395 | field | Terraria.WorldGen.GrowTreeSettings.Profiles | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3886 | 4 | GemTree_Sapphire | Terraria.WorldGen.GrowTreeSettings | `public static GrowTreeSettings GemTree_Sapphire = new GrowTreeSettings  			{  				GroundTest = GemTreeGroundTest,  				WallTest = GemTreeWallTest,  				TreeHeightMax = 12,  				TreeHeightMin = 7,  				TreeTileType = 585,  				TreeTopPaddingNeeded = 4,  				SaplingTileType = 590  			};` | `public static GrowTreeSettings GemTree_Sapphire = new GrowTreeSettings { GroundTest = GemTreeGroundTest, WallTest = GemTreeWallTest, TreeHeightMax = 12, TreeHeightMin = 7, TreeTileType = 585, TreeTopPaddingNeeded = 4, SaplingTileType = 590 };` |
| 2396 | field | Terraria.WorldGen.GrowTreeSettings.Profiles | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3897 | 4 | GemTree_Emerald | Terraria.WorldGen.GrowTreeSettings | `public static GrowTreeSettings GemTree_Emerald = new GrowTreeSettings  			{  				GroundTest = GemTreeGroundTest,  				WallTest = GemTreeWallTest,  				TreeHeightMax = 12,  				TreeHeightMin = 7,  				TreeTileType = 586,  				TreeTopPaddingNeeded = 4,  				SaplingTileType = 590  			};` | `public static GrowTreeSettings GemTree_Emerald = new GrowTreeSettings { GroundTest = GemTreeGroundTest, WallTest = GemTreeWallTest, TreeHeightMax = 12, TreeHeightMin = 7, TreeTileType = 586, TreeTopPaddingNeeded = 4, SaplingTileType = 590 };` |
| 2397 | field | Terraria.WorldGen.GrowTreeSettings.Profiles | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3908 | 4 | GemTree_Amber | Terraria.WorldGen.GrowTreeSettings | `public static GrowTreeSettings GemTree_Amber = new GrowTreeSettings  			{  				GroundTest = GemTreeGroundTest,  				WallTest = GemTreeWallTest,  				TreeHeightMax = 12,  				TreeHeightMin = 7,  				TreeTileType = 589,  				TreeTopPaddingNeeded = 4,  				SaplingTileType = 590  			};` | `public static GrowTreeSettings GemTree_Amber = new GrowTreeSettings { GroundTest = GemTreeGroundTest, WallTest = GemTreeWallTest, TreeHeightMax = 12, TreeHeightMin = 7, TreeTileType = 589, TreeTopPaddingNeeded = 4, SaplingTileType = 590 };` |
| 2398 | field | Terraria.WorldGen.GrowTreeSettings.Profiles | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3919 | 4 | VanityTree_Sakura | Terraria.WorldGen.GrowTreeSettings | `public static GrowTreeSettings VanityTree_Sakura = new GrowTreeSettings  			{  				GroundTest = VanityTreeGroundTest,  				WallTest = DefaultTreeWallTest,  				TreeHeightMax = 12,  				TreeHeightMin = 7,  				TreeTileType = 596,  				TreeTopPaddingNeeded = 4,  				SaplingTileType = 595  			};` | `public static GrowTreeSettings VanityTree_Sakura = new GrowTreeSettings { GroundTest = VanityTreeGroundTest, WallTest = DefaultTreeWallTest, TreeHeightMax = 12, TreeHeightMin = 7, TreeTileType = 596, TreeTopPaddingNeeded = 4, SaplingTileType = 595 };` |
| 2399 | field | Terraria.WorldGen.GrowTreeSettings.Profiles | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3930 | 4 | VanityTree_Willow | Terraria.WorldGen.GrowTreeSettings | `public static GrowTreeSettings VanityTree_Willow = new GrowTreeSettings  			{  				GroundTest = VanityTreeGroundTest,  				WallTest = DefaultTreeWallTest,  				TreeHeightMax = 12,  				TreeHeightMin = 7,  				TreeTileType = 616,  				TreeTopPaddingNeeded = 4,  				SaplingTileType = 615  			};` | `public static GrowTreeSettings VanityTree_Willow = new GrowTreeSettings { GroundTest = VanityTreeGroundTest, WallTest = DefaultTreeWallTest, TreeHeightMax = 12, TreeHeightMin = 7, TreeTileType = 616, TreeTopPaddingNeeded = 4, SaplingTileType = 615 };` |
| 2400 | field | Terraria.WorldGen.GrowTreeSettings.Profiles | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3941 | 4 | Tree_Ash | Terraria.WorldGen.GrowTreeSettings | `public static GrowTreeSettings Tree_Ash = new GrowTreeSettings  			{  				GroundTest = AshTreeGroundTest,  				WallTest = DefaultTreeWallTest,  				TreeHeightMax = 12,  				TreeHeightMin = 7,  				TreeTileType = 634,  				TreeTopPaddingNeeded = 4,  				SaplingTileType = 20  			};` | `public static GrowTreeSettings Tree_Ash = new GrowTreeSettings { GroundTest = AshTreeGroundTest, WallTest = DefaultTreeWallTest, TreeHeightMax = 12, TreeHeightMin = 7, TreeTileType = 634, TreeTopPaddingNeeded = 4, SaplingTileType = 20 };` |
| 2401 | field | Terraria.WorldGen.GrowTreeSettings | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3994 | 3 | TreeTileType | ushort | `public ushort TreeTileType;` | `public ushort TreeTileType;` |
| 2402 | field | Terraria.WorldGen.GrowTreeSettings | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3996 | 3 | TreeHeightMin | int | `public int TreeHeightMin;` | `public int TreeHeightMin;` |
| 2403 | field | Terraria.WorldGen.GrowTreeSettings | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3998 | 3 | TreeHeightMax | int | `public int TreeHeightMax;` | `public int TreeHeightMax;` |
| 2404 | field | Terraria.WorldGen.GrowTreeSettings | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4000 | 3 | TreeTopPaddingNeeded | int | `public int TreeTopPaddingNeeded;` | `public int TreeTopPaddingNeeded;` |
| 2405 | field | Terraria.WorldGen.GrowTreeSettings | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4002 | 3 | GroundTest | Terraria.WorldGen.GrowTreeSettings.IsTileFitForTreeGroundTest | `public IsTileFitForTreeGroundTest GroundTest;` | `public IsTileFitForTreeGroundTest GroundTest;` |
| 2406 | field | Terraria.WorldGen.GrowTreeSettings | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4004 | 3 | WallTest | Terraria.WorldGen.GrowTreeSettings.IsWallTypeFitForTreeBack | `public IsWallTypeFitForTreeBack WallTest;` | `public IsWallTypeFitForTreeBack WallTest;` |
| 2407 | field | Terraria.WorldGen.GrowTreeSettings | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4006 | 3 | SaplingTileType | ushort | `public ushort SaplingTileType;` | `public ushort SaplingTileType;` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2584 | property | Terraria.WorldBuilding.LandmassData | Terraria.WorldBuilding/LandmassData.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\LandmassData.cs | 15 | 2 | Top | Vector2 | `public Vector2 Top { get { return Position - new Vector2(0f, RadiusOrHalfSize); } set { Position = value + new Vector2(0f, RadiusOrHalfSize); } }` | `public Vector2 Top { get { return Position - new Vector2(0f, RadiusOrHalfSize); } set { Position = value + new Vector2(0f, RadiusOrHalfSize); } }` |


## 8. 本分区自检

- 叶子子系统：12 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：86 / 50 / 136。
- 来源序号范围：2235..2656；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
