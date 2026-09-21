# Version4 内容定义目录组件字段与属性设计

## 1. 文档目的

本文承接《[Version4 内容定义目录组件化拆分报告](../../component-decomposition/baseline/Version4内容定义目录组件化拆分报告.md)》，
只定义“内容定义目录”相关组件、不可变定义记录、索引和投影的数据形状。

本文不定义 System、Query、Command 的算法，不定义测试，也不直接修改现有 C# 源码。所有字段
均按 `public-decomposition` 的规则划分：共同变更原因、共同生命周期和共同读写者放在同一
组件；运行时实例、世界坐标状态、网络临时状态和表现历史不进入内容定义组件。

源码事实基于 `D:\TRbackup\Version4`。用户请求中的 `D:\TRbackup\Version4参考` 在当前环境中
不存在。`C:\Users\shan\Downloads\ECS\space-station-14-master` 只作为“原型定义按领域
组织、运行时实体另存、消费者通过只读索引读取”的组织参考，不复制其领域命名和代码。

## 2. 统一字段约定

### 2.1 可变性

- 定义记录和已发布目录的字段全部为只读属性；实现可以使用 `sealed record`、不可变结构或
  等价的冻结对象。
- 集合属性使用 `ImmutableArray<T>`、`ImmutableDictionary<TKey, TValue>` 或等价的深度只读
  类型。`IReadOnlyList<T>` 仅可作为外部接口视图，不能把可变 `List<T>` 直接泄漏出去。
- 具有初始化阶段可变性的 builder 不属于下列运行时组件；builder 完成校验后才生成定义记录。
- 下文的“可空”表示该内容类型不适用或没有默认值，不表示运行时可任意写入 `null`。

### 2.2 标识字段

| 字段 | 类型 | 语义 |
| --- | --- | --- |
| `TypeId` | `int` | Terraria 内容类型 ID；只在对应内容域内解释 |
| `NetId` | `int` | NPC 或投射物协议/内容网络 ID；不等于实体槽位 |
| `PersistentId` | `string` | 可跨加载保持的内容名称/持久 ID；不等于账户 ID 或实体 UUID |
| `CatalogRevision` | `long` | 已发布内容快照版本；用于跨投影识别同一份定义 |
| `SourceKey` | `string` | 构建源或内容包的诊断键；不作为玩法身份 |

`whoAmI`、实体 UUID、ReplicationId、玩家账户 UUID、容器槽位和网络连接 ID 不在内容目录中。

### 2.3 类型别名

为避免旧框架类型渗透领域定义，以下字段使用稳定的领域值对象名称：

| 类型 | 含义 |
| --- | --- |
| `ContentId` | 已校验的整数内容 ID 值对象 |
| `ContentReference` | 指向另一个内容定义的 typed reference，包含内容域和 ID |
| `SoundReference` | 声音资源键及可选音高范围，不持有播放器或音频状态 |
| `ColorRgba` | 四通道颜色值，不依赖渲染对象 |
| `TileCoordinate` | 世界 Tile 坐标值对象；仅用于规则参数，不表示某坐标当前 Tile |
| `DropRuleReference` | 指向不可变掉落规则节点的引用 |

## 3. 目录根与发布快照

这些对象是进程级内容状态，不附着于 Player、NPC、Projectile 或其他实体。

### 3.1 `ContentCatalogSnapshot`

一次完整构建和校验后的不可变快照。它拥有各个定义目录的引用，但不拥有运行时实体实例。

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `CatalogRevision` | `long` | 单调递增的快照版本 |
| `SourceKey` | `string` | 本快照的构建源标识 |
| `Items` | `ItemDefinitionCatalog` | Item 定义索引 |
| `Npcs` | `NpcDefinitionCatalog` | NPC 定义索引，支持正/负 NetId |
| `Projectiles` | `ProjectileDefinitionCatalog` | Projectile 定义索引 |
| `Buffs` | `BuffDefinitionCatalog` | Buff 规则索引 |
| `Tiles` | `TileDefinitionCatalog` | Tile 定义索引 |
| `Walls` | `WallDefinitionCatalog` | Wall 定义索引 |
| `Recipes` | `RecipeDefinitionCatalog` | 配方定义索引；当前 Version4 证据为 `partial` |
| `RecipeGroups` | `RecipeGroupCatalog` | 配方材料组索引；当前 Version4 证据为 `partial` |
| `DropRules` | `DropRuleCatalog` | NPC/全局掉落规则索引 |
| `FishingDropRules` | `FishingDropRuleCatalog` | 钓鱼规则索引；完整解析链当前为 `partial` |
| `Identities` | `ContentIdentityCatalog` | Type/Net/Persistent ID 映射 |
| `DerivedIndexes` | `ContentDerivedIndexCatalog` | 由定义重算的玩法索引 |
| `PresentationIndexes` | `ContentPresentationIndex` | Bestiary、创意排序等单向投影 |
| `IsValidated` | `bool` | 只读校验结果；未通过校验的候选不得发布 |

### 3.2 `ContentCatalog`

运行时注入的只读根门面。它不保存第二份定义，也不提供写入属性。

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Snapshot` | `ContentCatalogSnapshot` | 当前完整快照 |
| `CatalogRevision` | `long` | 转发 `Snapshot.CatalogRevision` |
| `Items` | `IItemDefinitionQuery` | Item 窄查询视图 |
| `Npcs` | `INpcDefinitionQuery` | NPC 窄查询视图 |
| `Projectiles` | `IProjectileDefinitionQuery` | Projectile 窄查询视图 |
| `Buffs` | `IBuffDefinitionQuery` | Buff 窄查询视图 |
| `Tiles` | `ITileDefinitionQuery` | Tile 窄查询视图 |
| `Walls` | `IWallDefinitionQuery` | Wall 窄查询视图 |
| `RecipesAndDrops` | `IRecipeAndDropQuery` | 配方、RecipeGroup、掉落和钓鱼只读视图 |
| `Identities` | `IContentIdentityQuery` | 内容身份只读视图 |

### 3.3 `ContentIdentityCatalog`

身份索引独立于玩法定义，以免把多个 ID 键空间压扁成一个字段。

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `ItemPersistentIdByType` | `ImmutableDictionary<int, string>` | Item TypeId 到持久 ID |
| `ItemTypeByPersistentId` | `ImmutableDictionary<string, int>` | Item 持久 ID 到 TypeId |
| `NpcPersistentIdByNetId` | `ImmutableDictionary<int, string>` | NPC NetId 到持久 ID，包含负 NetId |
| `NpcNetIdByPersistentId` | `ImmutableDictionary<string, int>` | NPC 持久 ID 到 NetId |
| `NpcBestiaryCreditIdByNetId` | `ImmutableDictionary<int, string>` | Bestiary 归因 ID；只作为内容/投影索引 |
| `ProjectilePersistentIdByType` | `ImmutableDictionary<int, string>` | Projectile TypeId 到持久 ID，可为空 |
| `ProjectileTypeByPersistentId` | `ImmutableDictionary<string, int>` | Projectile 持久 ID 到 TypeId，可为空 |

### 3.4 `ContentDerivedIndexCatalog`

只保存能从冻结定义完全重算的索引，不保存本帧缓存或实体引用。

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `ProjectileHostileByType` | `ImmutableArray<bool>` | Projectile 是否具有 hostile 默认能力 |
| `ProjectileHookByType` | `ImmutableArray<bool>` | Projectile 是否具有 hook 默认能力 |
| `NpcNetIdsByType` | `ImmutableDictionary<int, ImmutableArray<int>>` | 一个 NPC Type 对应的 NetId 变体 |
| `ItemTypesByHeadSlot` | `ImmutableDictionary<int, int>` | 头部装备槽到 Item TypeId |
| `ItemTypesByBodySlot` | `ImmutableDictionary<int, int>` | 身体装备槽到 Item TypeId |
| `ItemTypesByLegSlot` | `ImmutableDictionary<int, int>` | 腿部装备槽到 Item TypeId |
| `TileTypesByCreatedWall` | `ImmutableDictionary<int, ImmutableArray<int>>` | 创建指定 Wall 的 Item 类型索引 |
| `TileTypesByCreatedTile` | `ImmutableDictionary<int, ImmutableArray<int>>` | 创建指定 Tile 的 Item 类型索引 |

### 3.5 `ContentPresentationIndex`

表现或菜单投影，不是玩法定义的权威来源。

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `NpcBestiarySortingIdByNetId` | `ImmutableDictionary<int, int>` | Bestiary 排序位置 |
| `NpcBestiaryRarityStarsByType` | `ImmutableDictionary<int, int>` | Bestiary 星级 |
| `ItemCreativeSortingByType` | `ImmutableDictionary<int, CreativeItemSortValue>` | 创意菜单分组和顺序 |
| `DyeShaderIdByItemType` | `ImmutableDictionary<int, int>` | 染料表现索引 |
| `ItemAnimationByType` | `ImmutableDictionary<int, AnimationDefinition>` | 绘制动画定义，不含当前帧计数器 |

### 3.6 类型定义目录容器

每个 Catalog 只拥有对应定义记录的只读索引和范围元数据。它们不缓存查询结果，不持有
`Item`、`NPC`、`Projectile` 的可变样本，也不保存 builder。

#### `ItemDefinitionCatalog`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `DefinitionsByType` | `ImmutableArray<ItemDefinition>` | 按连续 Item TypeId 索引的定义表 |
| `Count` | `int` | 可解析 Item 定义数量 |
| `MaximumTypeId` | `int` | 可接受的最大 Item TypeId |

#### `NpcDefinitionCatalog`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `DefinitionsByNetId` | `ImmutableDictionary<int, NpcDefinition>` | 按 NPC NetId 索引的定义表，包含负值变体 |
| `DefinitionsByType` | `ImmutableDictionary<int, NpcDefinition>` | 每个标准 NPC TypeId 的主定义 |
| `Count` | `int` | 可解析 NPC NetId 定义数量 |
| `MinimumNetId` | `int` | 已发布定义的最小 NPC NetId |
| `MaximumNetId` | `int` | 已发布定义的最大 NPC NetId |

#### `ProjectileDefinitionCatalog`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `DefinitionsByType` | `ImmutableArray<ProjectileDefinition>` | 按连续 Projectile TypeId 索引的定义表 |
| `Count` | `int` | 可解析 Projectile 定义数量 |
| `MaximumTypeId` | `int` | 可接受的最大 Projectile TypeId |

#### `BuffDefinitionCatalog`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `DefinitionsByType` | `ImmutableArray<BuffDefinition>` | 按连续 Buff TypeId 索引的定义表 |
| `Count` | `int` | 可解析 Buff 定义数量 |
| `MaximumTypeId` | `int` | 可接受的最大 Buff TypeId |

#### `TileDefinitionCatalog`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `DefinitionsByType` | `ImmutableArray<TileDefinition>` | 按连续 Tile TypeId 索引的定义表 |
| `Count` | `int` | 可解析 Tile 定义数量 |
| `MaximumTypeId` | `int` | 可接受的最大 Tile TypeId |

#### `WallDefinitionCatalog`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `DefinitionsByType` | `ImmutableArray<WallDefinition>` | 按连续 Wall TypeId 索引的定义表 |
| `Count` | `int` | 可解析 Wall 定义数量 |
| `MaximumTypeId` | `int` | 可接受的最大 Wall TypeId |

#### `RecipeDefinitionCatalog`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `DefinitionsByRecipeId` | `ImmutableDictionary<int, RecipeDefinition>` | 按 RecipeId 索引的冻结配方定义 |
| `RecipeIds` | `ImmutableArray<int>` | 保持内容发布顺序的 RecipeId 列表 |
| `Count` | `int` | 已发布配方数量 |
| `IsComplete` | `bool` | 当前 Recipe 链路是否具备完整来源证据；Version4 当前为 `false` |

#### `RecipeGroupCatalog`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `DefinitionsByGroupId` | `ImmutableDictionary<int, RecipeGroupDefinition>` | 按内部 GroupId 索引的定义表 |
| `GroupIdByFakeItemId` | `ImmutableDictionary<int, int>` | 旧兼容 FakeItemId 到 GroupId 的映射 |
| `Count` | `int` | 已发布 RecipeGroup 数量 |
| `IsComplete` | `bool` | 当前 RecipeGroup 链路是否具备完整来源证据；Version4 当前为 `false` |

#### `DropRuleCatalog`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `GlobalEntries` | `ImmutableArray<DropRuleCatalogEntry>` | 适用于所有 NPC 的规则条目 |
| `EntriesByNpcNetId` | `ImmutableDictionary<int, ImmutableArray<DropRuleCatalogEntry>>` | 按 NPC NetId 的规则条目 |
| `RuleDefinitionsById` | `ImmutableDictionary<string, DropRuleDefinition>` | 可复用规则节点定义 |
| `Count` | `int` | 全局和 NPC 规则条目总数 |

#### `FishingDropRuleCatalog`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Rules` | `ImmutableArray<FishingDropRuleDefinition>` | 已校验概率分母的钓鱼规则 |
| `RulesByRarity` | `ImmutableDictionary<FishingRarity, ImmutableArray<FishingDropRuleDefinition>>` | 按稀有度分组的派生索引 |
| `Count` | `int` | 已发布钓鱼规则数 |
| `IsResolutionPathConfirmed` | `bool` | 是否已确认从规则到产物提交的完整链路；Version4 当前为 `false` |

## 4. Item 定义组件

`ItemDefinition` 由以下能力组件组合而成。`stack`、`prefix`、`favorited`、当前持有人、选中
槽位和使用计时属于 Item 实例或 Player Inventory，不在定义中。

### 4.1 `ItemDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Identity` | `ItemIdentityDefinition` | TypeId 和 PersistentId |
| `Use` | `ItemUseDefinition` | 使用节奏和使用方式 |
| `Stack` | `ItemStackDefinition` | 默认最大堆叠和堆叠能力 |
| `Tool` | `ItemToolDefinition` | 镐、斧、锤和 Tile 加成 |
| `Placement` | `ItemPlacementDefinition` | 创建 Tile/Wall 和放置样式 |
| `Combat` | `ItemCombatDefinition` | 伤害、投射物、弹药和伤害标签 |
| `Effects` | `ItemEffectDefinition` | 治疗、Buff、药水和资源效果 |
| `Equipment` | `ItemEquipmentDefinition` | 装备槽和时装能力 |
| `Economy` | `ItemEconomyDefinition` | 价格、购买和货币 |
| `Presentation` | `ItemPresentationDefinition` | 稀有度、颜色、缩放等内容表现参数 |
| `Capabilities` | `ItemCapabilityDefinition` | material、mount、sentry 等布尔能力 |

### 4.2 `ItemIdentityDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `TypeId` | `int` | Item 类型 ID |
| `PersistentId` | `string` | Item 持久内容 ID |
| `SourceKey` | `string?` | 定义来源键 |

### 4.3 `ItemUseDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `HoldStyle` | `int` | 持握样式 |
| `UseStyle` | `int` | 使用样式 |
| `Channel` | `bool` | 是否持续通道使用 |
| `UseAnimationTicks` | `int` | 使用动画时长 |
| `UseTimeTicks` | `int` | 使用间隔 |
| `AutoReuse` | `bool` | 是否自动重复使用 |
| `UseTurn` | `bool` | 使用时是否允许转身 |
| `NoUseGraphic` | `bool` | 使用时隐藏物品图像 |
| `NoMeleeGraphic` | `bool` | 是否隐藏近战表现 |
| `ShootsEveryUse` | `bool` | 每次使用是否发射 |
| `ReuseDelayTicks` | `int` | 重复使用延迟 |
| `UseSound` | `SoundReference?` | 使用声音资源引用 |
| `UseSoundPitch` | `float` | 使用声音默认音高 |

### 4.4 `ItemStackDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `MaxStack` | `int` | 最大堆叠数量；实例当前数量不在此处 |
| `UniqueStack` | `bool` | 是否禁止与同类普通堆叠 |
| `IsMaterial` | `bool` | 是否属于材料 |
| `DefaultStack` | `int` | 新建实例的默认数量；若由生成命令覆盖则不改定义 |

### 4.5 `ItemToolDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `PickPower` | `int` | 镐力 |
| `AxePower` | `int` | 斧力 |
| `HammerPower` | `int` | 锤力 |
| `TileBoost` | `int` | Tile 操作范围加成 |

### 4.6 `ItemPlacementDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `CreateTileTypeId` | `int?` | 放置后创建的 Tile 类型 |
| `CreateWallTypeId` | `int?` | 放置后创建的 Wall 类型 |
| `PlaceStyle` | `int` | 放置样式 |
| `PlaceOnLiquid` | `bool` | 是否允许在液体环境放置 |

`PlaceOnLiquid` 只有在 Version4 的实际 Item 规则确认后才能发布；若无法从旧行为提取，保持
`null` 或移入单独的 placement rule projection，不以默认值猜测。

### 4.7 `ItemCombatDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Damage` | `int` | 默认伤害 |
| `KnockBack` | `float` | 默认击退 |
| `CritChance` | `int` | 默认暴击率 |
| `ArmorPenetration` | `int` | 护甲穿透 |
| `BonusTagDamage` | `int` | 标签伤害加成 |
| `ShootTypeId` | `int?` | 发射的 Projectile 类型 |
| `ShootSpeed` | `float` | 发射速度 |
| `AmmoTypeId` | `int?` | 消耗的弹药类型 |
| `UseAmmoTypeId` | `int?` | 使用时选择的弹药类别 |
| `IsNotAmmo` | `bool` | 是否明确不是弹药 |
| `IsMelee` | `bool` | 近战标签 |
| `IsRanged` | `bool` | 远程标签 |
| `IsMagic` | `bool` | 魔法标签 |
| `IsSummon` | `bool` | 召唤标签 |
| `IsSentry` | `bool` | 哨兵标签 |

### 4.8 `ItemEffectDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `HealLife` | `int` | 使用后生命恢复量 |
| `HealMana` | `int` | 使用后魔力恢复量 |
| `LifeRegeneration` | `int` | 生命再生效果 |
| `Mana` | `int` | 使用所需魔力 |
| `ManaIncrease` | `int` | 永久或基础魔力增加值 |
| `BuffTypeId` | `int?` | 使用后施加的 Buff 类型 |
| `BuffDurationTicks` | `int` | 默认 Buff 持续时间 |
| `IsPotion` | `bool` | 是否为药水 |
| `IsConsumable` | `bool` | 是否消耗品 |
| `NoWet` | `bool` | 是否不受湿润规则影响 |

### 4.9 `ItemEquipmentDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `IsAccessory` | `bool` | 是否为饰品 |
| `HeadSlot` | `int?` | 头部装备槽 ID |
| `BodySlot` | `int?` | 身体装备槽 ID |
| `LegSlot` | `int?` | 腿部装备槽 ID |
| `HandOnSlot` | `sbyte?` | 手持正面槽 |
| `HandOffSlot` | `sbyte?` | 手持背面槽 |
| `BackSlot` | `sbyte?` | 背部槽 |
| `FrontSlot` | `sbyte?` | 前部槽 |
| `ShoeSlot` | `sbyte?` | 鞋子槽 |
| `WaistSlot` | `sbyte?` | 腰部槽 |
| `WingSlot` | `sbyte?` | 翅膀槽 |
| `ShieldSlot` | `sbyte?` | 盾牌槽 |
| `NeckSlot` | `sbyte?` | 项链槽 |
| `FaceSlot` | `sbyte?` | 面部槽 |
| `BalloonSlot` | `sbyte?` | 气球槽 |
| `BeardSlot` | `sbyte?` | 胡须槽 |
| `VoiceSlot` | `sbyte?` | 声音槽 |
| `Defense` | `int` | 装备提供的基础防御 |
| `IsSocial` | `bool` | 是否为社交装备 |
| `IsVanity` | `bool` | 是否为时装 |
| `HasVanityEffects` | `bool` | 无视觉槽时仍具有时装效果 |

### 4.10 `ItemEconomyDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Value` | `int` | 基础价值 |
| `CanBuy` | `bool` | 是否可购买 |
| `BuyOnce` | `bool` | 是否只能购买一次 |
| `ShopSpecialCurrencyId` | `int?` | 特殊货币 ID |
| `ShopCustomPrice` | `int?` | 自定义商店价格 |

### 4.11 `ItemPresentationDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Rare` | `int` | 稀有度 |
| `Color` | `ColorRgba?` | 内容默认颜色 |
| `Alpha` | `int` | 默认透明度 |
| `GlowMaskId` | `short?` | 发光遮罩内容 ID |
| `Scale` | `float` | 默认显示缩放 |
| `StringColor` | `int?` | 旧字符串颜色键 |
| `BestiaryNotes` | `string?` | Bestiary 文本内容；不保存解锁状态 |

### 4.12 `ItemCapabilityDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `MountTypeId` | `int?` | 坐骑类型 |
| `CartTrack` | `bool` | 是否为矿车轨道能力 |
| `IsChlorophyteExtractinatorConsumable` | `bool` | 提取机消耗能力 |
| `IsDd2Summon` | `bool` | DD2 召唤能力 |
| `IsNewAndShiny` | `bool` | 内容特殊标签 |

## 5. NPC 定义组件

NPC 的默认属性与实例的 `life`、`target`、AI 数组、Buff、位置、槽位和网络同步状态分离。

### 5.1 `NpcDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Identity` | `NpcIdentityDefinition` | TypeId、NetId、PersistentId |
| `Combat` | `NpcCombatDefinition` | 默认生命、伤害、防御和战斗标签 |
| `Movement` | `NpcMovementDefinition` | 默认尺寸和介质移动参数 |
| `Spawn` | `NpcSpawnDefinition` | 刷新与捕获资格 |
| `Town` | `NpcTownDefinition` | 城镇/住房/城镇分类标签 |
| `Presentation` | `NpcPresentationDefinition` | 帧、稀有度和显示缩放 |
| `Capabilities` | `NpcCapabilityDefinition` | Boss、敌对、Critter 等能力 |

### 5.2 `NpcIdentityDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `TypeId` | `int` | NPC 类型 ID |
| `NetId` | `int` | NPC 网络/内容 ID，允许负值变体 |
| `PersistentId` | `string` | NPC 持久内容 ID |

### 5.3 `NpcCombatDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `LifeMax` | `int` | 新实例默认最大生命 |
| `Damage` | `int` | 默认接触/攻击伤害 |
| `Defense` | `int` | 默认防御；运行时最终防御不在此处更新 |
| `KnockBackResist` | `float` | 击退抗性 |
| `TakenDamageMultiplier` | `float` | 受到伤害倍率 |
| `LifeRegenDefault` | `int` | 默认生命再生输入 |
| `Rarity` | `int` | 内容战斗稀有度 |
| `Value` | `int` | NPC 默认价值 |

### 5.4 `NpcMovementDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Width` | `int` | 默认碰撞宽度 |
| `Height` | `int` | 默认碰撞高度 |
| `Scale` | `float` | 默认尺寸缩放 |
| `WaterMovementSpeed` | `float` | 水中移动速度 |
| `LavaMovementSpeed` | `float` | 熔岩中移动速度 |
| `HoneyMovementSpeed` | `float` | 蜂蜜中移动速度 |
| `GravityMultiplier` | `float?` | 重力倍率；未被旧行为确认时保持可空 |

### 5.5 `NpcSpawnDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `AiStyle` | `int` | 默认 AI 风格 ID |
| `CatchItemTypeId` | `int?` | 可捕获时对应的 Item 类型 |
| `CanBeCaught` | `bool` | 是否可捕获 |
| `SpawnGroup` | `int?` | 刷新分类或内容组 |
| `SpawnWeight` | `float` | 默认刷新权重 |
| `SpawnNeedsSyncing` | `bool` | 生成后是否需要同步的默认提示；不表示实例脏标记 |

### 5.6 `NpcTownDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `IsTownNpc` | `bool` | 是否为城镇 NPC |
| `IsLikeTownNpc` | `bool` | 是否按城镇 NPC 处理 |
| `CountsAsCritter` | `bool` | 是否计为 Critter |
| `TownNpcVariationCount` | `int` | 城镇变体数量 |
| `HousingPriority` | `int?` | 住房/Bestiary 城镇优先级 |
| `CanBeReplacedByOtherNpcs` | `bool` | 是否可被其他 NPC 替换 |

### 5.7 `NpcPresentationDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `FrameCount` | `int` | 动画帧数量 |
| `AltTextureId` | `int?` | 替代纹理内容 ID |
| `BestiaryCreditId` | `string?` | Bestiary 归因 ID |
| `BestiaryHidden` | `bool` | 是否隐藏 Bestiary 条目 |
| `TrailCacheLength` | `int` | 表现轨迹定义长度；不保存 `oldPos` |

### 5.8 `NpcCapabilityDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Friendly` | `bool` | 默认友好标签 |
| `Hostile` | `bool` | 默认敌对标签 |
| `IsBoss` | `bool` | 是否为 Boss |
| `IsImmuneToLava` | `bool` | 是否具有熔岩免疫能力 |
| `IsImmuneToWater` | `bool` | 是否具有水环境免疫能力 |
| `IsDungeon` | `bool` | 是否属于地牢分类 |

未能从 Version4 的定义入口确认的特殊能力不得用 `false` 充当“已确认没有该能力”；应保持
构建缺口或拆为独立的可选能力集合。

## 6. Projectile 定义组件

Projectile 定义只描述生成默认值和静态能力；`owner`、`identity`、`timeLeft`、`ai/localAI`、
命中免疫、`netUpdate` 和轨迹数组是实例/协议状态。

### 6.1 `ProjectileDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Identity` | `ProjectileIdentityDefinition` | TypeId 和持久 ID |
| `Geometry` | `ProjectileGeometryDefinition` | 默认宽高、缩放和碰撞形状 |
| `Behavior` | `ProjectileBehaviorDefinition` | AI 风格和更新频率 |
| `Combat` | `ProjectileCombatDefinition` | 默认伤害、敌我和击退 |
| `Penetration` | `ProjectilePenetrationDefinition` | 默认穿透和命中策略 |
| `Capabilities` | `ProjectileCapabilityDefinition` | hook、pet、minion、sentry 等标签 |
| `Presentation` | `ProjectilePresentationDefinition` | 帧、光照和绘制层定义 |

### 6.2 `ProjectileIdentityDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `TypeId` | `int` | Projectile 类型 ID |
| `PersistentId` | `string?` | 可选持久内容 ID |

### 6.3 `ProjectileGeometryDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Width` | `int` | 默认宽度 |
| `Height` | `int` | 默认高度 |
| `Scale` | `float` | 默认缩放 |
| `TileCollide` | `bool` | 是否与 Tile 碰撞 |
| `CorrectSlopeCollision` | `bool` | 是否使用斜坡修正 |
| `IgnoreWater` | `bool` | 是否忽略水 |
| `OwnerHitCheckDistance` | `float` | Owner 命中检查距离 |

### 6.4 `ProjectileBehaviorDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `AiStyle` | `int` | AI 风格 ID |
| `ExtraUpdates` | `int` | 每 Tick 额外更新次数 |
| `DefaultTimeLeft` | `int` | 新实例默认寿命；实例倒计时不在此处变化 |
| `NumUpdates` | `int` | 初始更新次数配置 |
| `DecidesManualFallThrough` | `bool` | 是否由行为决定穿平台 |
| `ShouldFallThrough` | `bool` | 默认穿平台策略 |

### 6.5 `ProjectileCombatDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Damage` | `int` | 默认伤害 |
| `KnockBack` | `float` | 默认击退 |
| `Friendly` | `bool` | 默认友好 |
| `Hostile` | `bool` | 默认敌对 |
| `Melee` | `bool` | 近战标签 |
| `Ranged` | `bool` | 远程标签 |
| `Magic` | `bool` | 魔法标签 |
| `ColdDamage` | `bool` | 冰冷伤害标签 |
| `Trap` | `bool` | 陷阱标签 |
| `NpcProjectile` | `bool` | NPC 发射标签 |

### 6.6 `ProjectilePenetrationDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `DefaultPenetrate` | `int` | 新实例默认剩余穿透 |
| `MaxPenetrate` | `int` | 最大穿透 |
| `StopsDealingDamageAfterPenetrateHits` | `bool` | 穿透命中后是否停止伤害 |
| `UsesLocalNpcImmunity` | `bool` | 是否使用本地 NPC 免疫 |
| `UsesIdStaticNpcImmunity` | `bool` | 是否使用按 ID 的静态免疫 |
| `AppliesImmunityTimeOnSingleHits` | `bool` | 单次命中是否施加免疫时间 |
| `LocalNpcHitCooldown` | `int` | 本地命中冷却默认值 |
| `IdStaticNpcHitCooldown` | `int` | 静态命中冷却默认值 |

### 6.7 `ProjectileCapabilityDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `IsArrow` | `bool` | 箭类标签 |
| `IsBobber` | `bool` | 鱼漂标签 |
| `IsCounterweight` | `bool` | 配重标签 |
| `IsSentry` | `bool` | 哨兵标签 |
| `IsMinion` | `bool` | 召唤物标签 |
| `MinionSlots` | `float` | 默认占用召唤容量 |
| `IsOwnerHitCheck` | `bool` | 是否依赖 Owner 命中检查 |
| `UsesOwnerMeleeHitCooldown` | `bool` | 是否使用 Owner 近战冷却 |
| `UsesOwnerLight` | `bool` | 是否使用 Owner 光照 |
| `NoEnchantments` | `bool` | 是否禁止附魔 |
| `NoEnchantmentVisuals` | `bool` | 是否禁止附魔表现 |

### 6.8 `ProjectilePresentationDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `FrameCount` | `int` | 动画帧数量 |
| `GlowMaskId` | `short?` | 发光遮罩 ID |
| `Light` | `float` | 默认光照强度 |
| `DrawLayer` | `int` | 默认绘制层 |
| `Hide` | `bool` | 默认隐藏表现 |

## 7. Buff 定义组件

Buff 定义描述一种 Buff 的规则；实体当前 Buff 持续时间、免疫倒计时和已应用来源属于
`StatusEffectsComponent` 或战斗状态。

### 7.1 `BuffDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Identity` | `BuffIdentityDefinition` | Buff ID 和持久键 |
| `Rules` | `BuffRuleDefinition` | 减益、PVP、持续与保存规则 |
| `Presentation` | `BuffPresentationDefinition` | 显示和本地化引用 |

### 7.2 `BuffIdentityDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `TypeId` | `int` | Buff 类型 ID |
| `PersistentId` | `string?` | 可选持久内容 ID |

### 7.3 `BuffRuleDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `IsDebuff` | `bool` | 是否为减益 |
| `AffectsPvp` | `bool` | 是否影响 PvP |
| `IsPersistent` | `bool` | 是否允许跨规则边界持续 |
| `NoSave` | `bool` | 是否禁止存档 |
| `NoTimeDisplay` | `bool` | 是否隐藏时间显示 |
| `ImmuneBuffTypeIds` | `ImmutableArray<int>` | 互相免疫的 Buff 类型 |
| `DefaultDurationTicks` | `int?` | 默认持续时间；实例时间不在此处更新 |

### 7.4 `BuffPresentationDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `DisplayNameKey` | `string?` | 本地化名称键 |
| `DescriptionKey` | `string?` | 本地化描述键 |
| `IconAssetKey` | `string?` | 图标资源键 |

## 8. Tile 与 Wall 定义组件

Tile/Wall 定义是按类型的规则，不是 `Main.tile[x, y]` 的当前内容。Tile 坐标、active、frame、
液体量、区段加载标记和临时固体覆盖属于世界存储或运行期状态。

### 8.1 `TileDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Identity` | `TileIdentityDefinition` | Tile 类型 ID |
| `Collision` | `TileCollisionDefinition` | 固体、平台和工具碰撞规则 |
| `Lighting` | `TileLightingDefinition` | 光照与遮光规则 |
| `Framing` | `TileFramingDefinition` | 帧和合并规则 |
| `Interaction` | `TileInteractionDefinition` | 容器、告示牌和交互标签 |
| `Environment` | `TileEnvironmentDefinition` | 液体、沙、火焰等环境规则 |

### 8.2 `TileIdentityDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `TypeId` | `int` | Tile 类型 ID |
| `PersistentId` | `string?` | 可选持久内容 ID |

### 8.3 `TileCollisionDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Solid` | `bool` | 基础固体属性 |
| `SolidTop` | `bool` | 是否为平台/顶部固体 |
| `Bouncy` | `bool` | 是否具有弹性 |
| `NoAttach` | `bool` | 是否禁止附着 |
| `NoFail` | `bool` | 碰撞/放置失败规则 |
| `Platform` | `bool` | 是否为平台 |
| `AxePowerRequired` | `int?` | 需要的斧力，可为空 |
| `PickPowerRequired` | `int?` | 需要的镐力，可为空 |
| `HammerPowerRequired` | `int?` | 需要的锤力，可为空 |

### 8.4 `TileLightingDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Lighted` | `bool` | 是否发光 |
| `BlockLight` | `bool` | 是否阻挡光线 |
| `NoSunLight` | `bool` | 是否阻挡阳光 |
| `Shine` | `int` | 默认闪烁/发光强度 |
| `Shine2` | `bool` | 第二种发光规则 |
| `GlowMaskId` | `short?` | 发光遮罩 ID |

### 8.5 `TileFramingDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `FrameImportant` | `bool` | 是否依赖独立帧 |
| `LargeFrameCount` | `byte` | 大型帧数量 |
| `MergeDirt` | `bool` | 是否与泥土合并 |
| `BlendAll` | `bool` | 是否与所有相邻 Tile 混合 |
| `MergeTypeIds` | `ImmutableArray<int>` | 可合并的 Tile 类型 |
| `Brick` | `bool` | 是否按砖块规则处理 |
| `Cracked` | `bool` | 是否为裂纹 Tile |

### 8.6 `TileInteractionDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Container` | `bool` | 是否为容器 Tile |
| `Sign` | `bool` | 是否为告示牌 Tile |
| `Table` | `bool` | 是否为桌类 Tile |
| `Rope` | `bool` | 是否为绳类 Tile |
| `NoSunLight` | `bool` | 交互/遮光复用标记；若与 Lighting 语义冲突则只保留一处权威 |
| `LavaDeath` | `bool` | 接触熔岩是否导致死亡 |
| `WaterDeath` | `bool` | 接触水是否导致死亡 |

### 8.7 `TileEnvironmentDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Sand` | `bool` | 是否为沙类环境 |
| `Flame` | `bool` | 是否为火焰类环境 |
| `ObsidianKill` | `bool` | 是否参与黑曜石杀伤规则 |
| `LiquidInteraction` | `TileLiquidInteractionKind` | 与液体的静态交互类别 |

### 8.8 `WallDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `TypeId` | `int` | Wall 类型 ID |
| `PersistentId` | `string?` | 可选持久内容 ID |
| `HouseWall` | `bool` | 是否可作为房屋墙 |
| `DungeonWall` | `bool` | 是否为地牢墙 |
| `Light` | `bool` | 是否具有墙体光照规则 |
| `BlendTypeId` | `int?` | 墙体混合类型 |
| `LargeFrameCount` | `byte` | 大型墙帧数量 |

### 8.9 `TileSolidityOverrideState`

这是运行期阶段状态，不属于 `TileDefinitionCatalog`。它用于承接当前 `Liquid`/WorldGen 对
`Main.tileSolid` 等表的临时改写。

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `ActiveOverrides` | `ImmutableDictionary<int, TileSolidityOverride>` | 当前阶段的 Tile 固体覆盖 |
| `OwnerPhase` | `TileOverridePhase` | 产生覆盖的系统阶段 |
| `ExpiresAtTick` | `long?` | 覆盖失效 Tick |
| `Revision` | `long` | 覆盖集合版本 |

`TileSolidityOverride` 值对象字段：

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `TileTypeId` | `int` | 被覆盖的 Tile 类型 |
| `Solid` | `bool` | 有效固体值 |
| `Reason` | `TileOverrideReason` | 液体、世界生成或其他明确原因 |

## 9. Recipe 与 RecipeGroup 定义组件

当前 Version4 在 `Main.Initialize_AlmostEverything` 中注释了 Recipe setup 调用，`Recipe.cs`
也没有闭合的完整实现。因此以下字段是目标数据形状，整体状态保持 `partial`，不得据此声称
当前运行时已经拥有完整配方目录。

### 9.1 `RecipeDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `RecipeId` | `int` | 配方稳定 ID |
| `Result` | `RecipeResultDefinition` | 产出 Item 和数量 |
| `Ingredients` | `ImmutableArray<RecipeIngredientDefinition>` | 输入材料 |
| `RequiredTileTypeIds` | `ImmutableArray<int>` | 所需工作站 Tile 类型 |
| `Conditions` | `ImmutableArray<RecipeConditionDefinition>` | 环境/世界条件 |
| `CustomShimmerResults` | `ImmutableArray<RecipeResultDefinition>` | 自定义微光结果 |
| `NeedHoney` | `bool` | 是否需要蜂蜜 |
| `NeedWater` | `bool` | 是否需要水 |
| `NeedLava` | `bool` | 是否需要熔岩 |
| `NeedTorchGodsFavor` | `bool` | 是否需要 Torch God 恩惠 |
| `Alchemy` | `bool` | 是否为炼金配方 |
| `NeedSnowBiome` | `bool` | 是否需要雪地环境 |
| `NeedGraveyardBiome` | `bool` | 是否需要墓地环境 |
| `NeedMechdusa` | `bool` | 是否需要 Mechdusa 条件 |
| `NotDecraftable` | `bool` | 是否禁止分解 |
| `Crimson` | `bool` | 是否需要猩红环境 |
| `Corruption` | `bool` | 是否需要腐化环境 |

### 9.2 `RecipeResultDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `ItemTypeId` | `int` | 产出 Item 类型 |
| `Stack` | `int` | 产出数量 |
| `PrefixId` | `byte?` | 固定前缀；随机前缀不写入定义 |

### 9.3 `RecipeIngredientDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `ItemTypeId` | `int?` | 直接材料 Item 类型 |
| `RecipeGroupId` | `int?` | 材料组 ID；与 `ItemTypeId` 二选一 |
| `Stack` | `int` | 需要数量 |

### 9.4 `RecipeGroupDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `GroupId` | `int` | 内部配方组 ID |
| `PersistentId` | `string?` | 可选持久键 |
| `DisplayNameKey` | `string?` | 组合材料显示文本键 |
| `ValidItemTypeIds` | `ImmutableArray<int>` | 可满足该组的 Item 类型 |
| `PreferredItemTypeId` | `int?` | 仅用于选择/显示的首选 Item |
| `DecraftItemTypeId` | `int?` | 分解时使用的 Item 类型 |
| `FakeItemId` | `int` | 兼容旧 `FakeItemIdOffset` 的内部引用值 |

## 10. 掉落与钓鱼定义组件

掉落目录保存规则图，不保存一次击杀的随机结果、已生成的 WorldItem 或实体引用。

### 10.1 `DropRuleCatalogEntry`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `NpcNetId` | `int?` | 目标 NPC NetId；为空表示全局规则 |
| `Rule` | `DropRuleDefinition` | 不可变规则树根 |
| `Priority` | `int` | 规则评估顺序 |
| `SourceKey` | `string` | 注册源 |

### 10.2 `DropRuleDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `RuleId` | `string` | 规则节点稳定键 |
| `Kind` | `DropRuleKind` | Common、OneFromOptions、Condition、Expert 等类别 |
| `ItemTypeIds` | `ImmutableArray<int>` | 可能产出的 Item 类型 |
| `ChanceNumerator` | `int` | 概率分子 |
| `ChanceDenominator` | `int` | 概率分母，必须为正 |
| `MinimumStack` | `int` | 最小数量 |
| `MaximumStack` | `int` | 最大数量 |
| `Conditions` | `ImmutableArray<DropConditionDefinition>` | 资格条件 |
| `SuccessChains` | `ImmutableArray<DropRuleReference>` | 成功后的链式规则 |
| `FailureChains` | `ImmutableArray<DropRuleReference>` | 失败后的链式规则 |
| `ExpertVariant` | `DropRuleReference?` | 专家模式规则 |
| `NormalVariant` | `DropRuleReference?` | 普通模式规则 |
| `LuckPolicy` | `DropLuckPolicy` | 幸运值影响规则 |

### 10.3 `DropConditionDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Kind` | `DropConditionKind` | 世界、难度、事件、Biome 或击杀来源条件 |
| `Parameter` | `string?` | 稳定参数键 |
| `IntegerValue` | `int?` | 整数参数 |
| `BooleanValue` | `bool?` | 布尔参数 |
| `ReferencedContentIds` | `ImmutableArray<ContentReference>` | 条件涉及的内容 ID |

### 10.4 `FishingDropRuleDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `RuleId` | `string` | 钓鱼规则键 |
| `Rarity` | `FishingRarity` | Common、Rare、Legendary 等 |
| `ChanceNumerator` | `int` | 概率分子 |
| `ChanceDenominator` | `int` | 概率分母，必须为正 |
| `ItemTypeIds` | `ImmutableArray<int>` | 可能产出的 Item 类型 |
| `Conditions` | `ImmutableArray<FishingConditionDefinition>` | 水域、高度、Biome、难度条件 |
| `IsQuestFish` | `bool` | 是否为任务鱼 |
| `StopFurtherRulesOnMatch` | `bool` | 命中后是否停止后续规则 |

### 10.5 `FishingConditionDefinition`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `Kind` | `FishingConditionKind` | 水量、高度、Biome、世界模式等类别 |
| `MinimumValue` | `float?` | 最小阈值 |
| `MaximumValue` | `float?` | 最大阈值 |
| `ContentTypeIds` | `ImmutableArray<int>` | 关联 Tile/Wall/Item 类型 |
| `Required` | `bool` | 是否为必要条件 |

## 11. 可选实体定义引用组件

内容目录本身不是实体组件，但实体若需要在 ECS 中引用某个定义，使用最小引用组件；它们不
复制定义字段，也不把目录对象作为可变引用暴露给系统。

### 11.1 `ItemDefinitionComponent`

用于 Item 实体指向内容定义。当前仓库已有同名组件，现有字段是 `Type`；设计上保持最小化。

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `TypeId` | `int` | Item Definition 的 TypeId |
| `CatalogRevision` | `long` | 创建该实例时采用的目录版本 |

`CatalogRevision` 如果会导致每个实例承担不必要的复制成本，可以只保存在实例快照/调试投影，
但不能把 `ItemDefinition` 对象引用放进实体组件。

### 11.2 `NpcDefinitionComponent`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `TypeId` | `int` | NPC 类型 ID |
| `NetId` | `int` | 创建该实例采用的 NPC NetId |
| `CatalogRevision` | `long` | 创建实例时的目录版本 |

### 11.3 `ProjectileDefinitionComponent`

用于 Projectile 实体指向定义。当前仓库已有 `Type`、`Friendly`、`Hostile`、`ExtraUpdates`；其中
后三项若作为运行时可变值，应迁移到专用运行时能力/行为组件，不能通过实体组件反向修改目录。

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `TypeId` | `int` | Projectile 类型 ID |
| `CatalogRevision` | `long` | 创建实例时的目录版本 |
| `Friendly` | `bool` | 生成时复制的运行时攻击阵营值 |
| `Hostile` | `bool` | 生成时复制的运行时攻击阵营值 |
| `ExtraUpdates` | `int` | 生成时复制的更新频率值 |

这里的 `Friendly`、`Hostile`、`ExtraUpdates` 是实例初始化后的值副本，不是 DefinitionCatalog
的权威字段；如果后续系统需要改变它们，写入 Projectile 运行时组件即可。

## 12. 不属于本设计的字段

下列字段即使与内容定义有关，也不能放进上述定义组件：

| 字段类别 | 示例 | 正确归属 |
| --- | --- | --- |
| 实例数量/变体 | `Item.stack`、`Item.prefix`、当前名称覆盖 | Item 实例/库存组件 |
| 实例生命与 AI | `NPC.life`、`NPC.target`、`NPC.ai[]`、`Projectile.ai[]` | NPC/Projectile 运行时组件 |
| 实例寿命与命中 | `Projectile.timeLeft`、`localNPCImmunity`、`numHits` | Projectile 生命周期/战斗组件 |
| 实体关系 | `Projectile.owner`、当前目标、容器持有人 | typed Entity Reference/关系组件 |
| 世界坐标状态 | `Main.tile[x,y]`、Tile frame、液体量、Chest 内容 | WorldStorage/TileMap/Container |
| 网络状态 | `identity`、`netUpdate`、`netSpam`、复制游标 | Network Projection/Adapter |
| 表现历史 | `oldPos`、`oldRot`、`oldSpriteDirection`、当前动画帧计数器 | Presentation Projection |
| 随机结果 | 某次掉落的 Item、数量、前缀 | Drop Resolution 输出/Spawn Command |
| 本帧或阶段覆盖 | `Main.tileSolid` 的临时变更 | `TileSolidityOverrideState` |

## 13. 字段命名和兼容映射

1. 新定义优先使用语义完整名称，例如 `UseAnimationTicks`、`CreateTileTypeId`、
   `DefaultTimeLeft`，不直接暴露旧字段的含混缩写。
2. 旧 `Item.type`、`NPC.netID`、`Projectile.type` 通过单向 Adapter 映射到 `TypeId`/`NetId`；
   适配器不把旧可变对象存入定义记录。
3. 旧数组如 `Main.tileSolid`、`Main.projFrames`、`Main.pvpBuff` 在构建时复制为对应的不可变
   定义集合；运行期覆盖必须进入单独的阶段状态。
4. `ContentSamples.ItemsByType`、`NpcsByNetId`、`ProjectilesByType` 只能作为 builder 的输入
   或兼容查询源；发布后的 ECS/领域系统只读取本设计的只读属性。
5. 当前仓库已有的 `ItemDefinitionComponent` 和 `ProjectileDefinitionComponent` 不应与
   `ItemDefinition`/`ProjectileDefinition` 混为同一层：前者是实体到定义的最小引用，后者是
   进程级不可变内容记录。

## 14. 设计状态

| 组件组 | 状态 | 说明 |
| --- | --- | --- |
| Item/NPC/Projectile/Buff/Tile/Wall 定义字段 | `confirmed` 形状 | 可由 Version4 `SetDefaults`、ID Sets、`ContentSamples` 和 Main 初始化路径提取；个别未确认字段标为可空或待适配 |
| 身份和派生索引 | `confirmed` 形状 | 对应 `ContentSamples` 双向映射和 `Main`/ID Sets 派生表 |
| NPC Drop 规则 | `confirmed` 形状 | 对应 `ItemDropDatabase` 注册/查找和 `ItemDropResolver` 链式规则 |
| Recipe/RecipeGroup | `partial` | setup 调用被注释，当前实现不足以证明完整运行时字段 |
| Fishing Drop | `partial` | 规则集合和概率校验可见，完整解析/提交链未闭合 |
| `TileSolidityOverrideState` | `partial` | 运行时写入事实已确认，覆盖所有写者和阶段仍需后续实现审计 |
| 可选实体定义引用组件 | 设计建议 | 仅描述最小引用字段，不改变当前生产代码 |
