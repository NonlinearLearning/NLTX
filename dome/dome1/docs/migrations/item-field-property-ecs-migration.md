# Item.cs 字段与属性 ECS 迁移报告

**审查日期：** 2026-08-31  
**状态：** `PARTIAL`（字段归属已建立，完整行为等价未完成）  
**范围：** Version4 `Terraria/Item.cs` 的字段和属性声明  
**基线报告：** [`docs/research/2026-08-28-field-property-migration-comparison.md`](../research/2026-08-28-field-property-migration-comparison.md)  
**成员基线：** [`docs/migrations/item-ecs-member-mapping.md`](item-ecs-member-mapping.md)

**最近批次：** Batch GA（2026-08-31）为 source-resolved generated offers 增加
authoritative shop catalog/cache 原子刷新，并验证 stale offer、重复/未知 Item 拒绝和空结果
清空；NPC 条件驱动的动态 catalog 生成、完整交易 parity 和完整 Potion/Flask effect parity
仍为 `deferred`。

## 1. 结论

本报告把旧 `Item` God Object 中的字段和属性按 ECS 所有权重新组织为
`Definition -> Component -> System -> Snapshot -> Persistence/Protocol`。迁移目标不是把
148 个名字原样复制到新类，而是让每个成员的值语义、可变性、生命周期、序列化边界和拒绝
规则有明确 owner。

当前结论如下：

| 项目 | 结果 |
| --- | --- |
| 字段/属性声明基线 | 151（基线报告的词法声明计数） |
| 按本任务纳入迁移的成员 | 148 |
| 按 ID 语义排除的 Item.cs 成员 | 0 |
| 初始归属 | Definition 96、System 20、Component 3、Compatibility 8、Deferred 21 |
| 已有明确 ECS 运行时 | 定义注册/校验、库存与实例状态、世界物品生成/拾取/掉落、使用/装备/弹药、前缀/变体、快照/持久化/复制的部分路径 |
| 仍未等价 | 完整 `SetDefaults`/`ItemID.Sets`、完整 prefix/equipment/shop/shimmer、专用机制和 UI/渲染/音频字段 |
| 迁移判定 | `PARTIAL`；不能宣称旧 `Terraria.Item` 已被完全移除或实现字节级 parity |

基线中的 21 个 Deferred 成员中，`flaskTime`、`shopCustomPrice` 和 `shopSpecialCurrency`
三个字段行为族已经获得窄 owner：Batch FX 完成 `flaskTime` 的常量、八项 Item-to-Buff
mapping、兼容适配和服务器 Buff application；Batch FV/FW 完成普通货币覆盖与 Item-backed
special-currency settlement；Batch FY 再为成交后的 `OnPurchase` transient cleanup 提供 typed
receipt；Batch FZ 由 `ShopOfferCatalogSystem` 消费回执并更新注册 offer；Batch GA 再为已经
解析的 generated offers 提供全量校验后的 catalog 原子刷新。上述批次均为
`completed_partial`，因为完整 Flask effect、动态 catalog 生成、动态 OnPurchase reset、折扣/
价格调整、UI、协议和持久化仍未完成；因此当前仍 Deferred 的成员数保持为 20。

### 1.1 ID 排除边界

本任务沿用基线报告的语义排除规则：只排除实体/网络/世界/连接身份编号，例如
`Id`、`Uid`、`NetId`、`identity`、`whoAmI` 和明确的 `worldID`。Item.cs 中没有命中这类
声明，因此排除数为 0。

`type` 不属于排除项。它是内容类型键，不是实体身份；ECS 中由 `ItemDefinition.ItemType`
和 `ItemStack.ItemType` 承载。删除它会破坏定义查找、堆叠校验、库存复制和持久化恢复。

本报告只讨论字段和属性，不把 `SetDefaults`、`Prefix`、`Refresh` 等方法计入 148 个成员。
148 个成员是 `item-ecs-member-mapping.md` 生成的归一化字段/属性行；基线词法扫描中的表达式
体/静态缓存识别差异以及方法条目仍保留在成员映射表中，由 Item 执行计划跟踪，不在本报告
的字段/属性计数中重复计算。

## 2. ECS 所有权模型

旧字段按“是否属于某个物品实例”分层。静态、跨实例共享的值不可写入运行时组件；实例值
不可放回全局定义；副作用只能由系统在 tick 边界提交。

| 层 | 责任 | 当前类型/owner | 允许的内容 |
| --- | --- | --- | --- |
| Definition | 类型级、不可变规则 | `ItemDefinition`、`ItemIdentityDefinition`、`ItemUseDefinition`、`ItemCombatDefinition`、`ItemPlacementDefinition`、`ItemRecoveryDefinition`、`ItemEquipmentDefinition`、`ItemPrefixDefinition`、`ItemExtractinatorDefinition` | 尺寸、堆叠上限、价值、稀有度、使用/战斗/装备/放置/恢复规则、名称键和模式约束 |
| Component | 物品实例和世界生命周期 | `ItemStackComponent`、`ItemInstanceStateComponent`、`ItemWorldStateComponent`、`InventoryComponent` | 数量、前缀、变体、染色、收藏、名称覆盖、激活、拾取延迟、revision、保留玩家 |
| System | 纯规则计算和命令提交 | `ItemUseSystem`、`ItemEquipmentSystem`、`ItemAmmoConsumptionSystem`、`ItemPrefixSystem`、`ItemVariantSystem`、`WorldItemSpawnSystem`、`WorldItemPickupSystem`、`ItemPriceSystem`、`ShopOfferCatalogSystem` 等 | 使用事务、装备冲突、弹药扣除、前缀/变体变更、掉落、拾取、价格换算、商店回执消费、放置校验 |
| Snapshot | 只读跨层投影 | `ItemInstanceSnapshot`、`InventorySnapshot`、`EquipmentSnapshot`、`WorldItemSnapshot`、`ItemReplicationSnapshot` | Server 读取的稳定状态；不暴露 Arch Entity，不接受客户端反写 |
| Compatibility | 旧数据/协议边界 | `LegacyItemDefinitionAdapter`、`LegacyItemImportAdapter`、`LegacyItemDropAdapter` | 读取明确的旧数据记录并转换为定义、实例快照或创建命令；不得成为 Simulation 的全局状态 |
| Client/deferred | 客户端表现或尚无权威域 | Tooltip、颜色、音效、持有动画、Bestiary、商店特殊货币、DD2、sentry 等 | 保持延期；先建立独立 owner，再决定是否进入 ECS |

### 2.1 不变量

- `ItemType == 0` 或 `Quantity <= 0` 表示空槽/非活动 tombstone；空实例不能携带实例状态。
- 数量不得超过注册表中的 `StackLimit`；`UniqueStack`、前缀、变体和染色不兼容时不得合并。
- 定义对象是只读值；注册表拒绝重复/空类型、非法堆叠上限、未知依赖和矛盾规则。
- 世界物品 `Revision` 单调递增；Arch Entity、复制编号和持久化关联不混用。
- 一次成功使用最多扣除一次主物品和一次弹药；拒绝路径不得修改库存或世界物品。
- 客户端 `SyncItem`、装备确认和实例字段不能直接创建或覆盖服务端权威状态。
- UI/渲染/音频字段不得通过通用 `ItemData` 或 `Manager` 类型偷偷进入 Simulation。

## 3. 成员迁移总表

以下清单覆盖 `item-ecs-member-mapping.md` 的 148 个字段/属性行。`归属已建立` 只表示
目标 owner 已确定；只有表中明确写成 `运行时已验证` 才表示存在当前代码和验证器证据。

| 当前归属分组 | 成员数 | 当前判定 | 主要 owner | 下一步 |
| --- | ---: | --- | --- | --- |
| Definition | 97 | 归属已建立；尺寸、价值、稀有度、战斗/使用/装备/恢复/放置的一部分已运行时验证，Batch FX duration owner 为 `completed_partial` | `Items/Definitions`、`ItemDefinitionRegistry`、`ItemDefinitionCompiler`、`LegacyFlaskDefinitionRegistry` | 补齐默认值编译、静态表、全部引用和序列化字段 |
| System | 20 | 归属已建立；库存使用、弹药、装备、价格、前缀/变体和世界物品路径部分验证 | `Items/Systems`、`DomeSimulation` 命令提交 | 将剩余旧副作用改成确定性命令和提交后事件 |
| Component | 3 | `favorited`、`newAndShiny`、`active` 有明确组件/不变量；快照/持久化覆盖部分验证 | `ItemInstanceStateComponent`、`ItemWorldStateComponent`、`ItemStackComponent` | 补齐所有实例生命周期字段和 round-trip 证据 |
| Compatibility | 8 | 仅边界适配，不等于领域行为迁移 | `Items/Compatibility` | 保持零全局 legacy 引用；为每个适配输入建立版本化契约 |
| Deferred | 20 | 明确延期；其中 5 项是未建立的服务端行为域，其余是客户端表现 | Client projection 或新领域 owner | 先完成 owner/消费路径，再从 Deferred 移动到已验证 |

## 4. Definition 成员（97，含 Batch FX 当前 owner）

这些成员描述物品类型或不可变规则。迁移时应进入定义记录或定义注册表，不能在每个实例
重复保存。当前 `ItemDefinition` 已直接承载一部分通用标量，复杂族由嵌套定义记录承载。

### 4.1 基础、经济和身份元数据

`width`、`height`、`manaGrabRange`、`_nameOverride`、`luckPotionDuration1`、
`luckPotionDuration2`、`luckPotionDuration3`、`flaskTime`、`copper`、`silver`、`gold`、`platinum`、
`CommonMaxStack`、`potionDelay`、`questItem`、`headType`、`bodyType`、`legType`、`flame`、
`mech`、`tileWand`、`dye`、`fishingPole`、`bait`、`makeNPC`、`expertOnly`、`expert`、
`hairDye`、`paint`、`paintCoating`、`type`、`channel`、`accessory`、`stack`、`maxStack`。

目标映射：

- `type` -> `ItemDefinition.ItemType` / `ItemStack.ItemType`；`stack` -> `ItemStack.Quantity`。
- `maxStack`、`CommonMaxStack` -> `ItemDefinition.StackLimit`，由注册表在写入前校验。
- `flaskTime` -> `ItemDefinition.FlaskDurationTicks`；八项已确认的 Flask item-to-buff 关系由
  `LegacyFlaskDefinitionRegistry` 持有，并由 `ItemRecoveryDefinition` 投影到服务器 Buff
  application。该 owner 当前为 `completed_partial`，不代表完整 Flask effect parity。
- 名称覆盖、任务/材料/专家/唯一堆叠属性 -> `ItemIdentityDefinition` 或实例名称覆盖。
- `paint`、`paintCoating`、`dye` 不应伪装成定义与实例混合字段；实例值进入
  `ItemInstanceStateComponent`，类型级合法性留在 Definition。
- `headType`、`bodyType`、`legType` 等旧数组是静态表输入，不能直接复制成可变全局数组；
  需要版本化的定义注册表和缺失项拒绝规则。

### 4.2 放置、恢复和装备元数据

`damage`、`knockBack`、`healLife`、`healMana`、`potion`、`consumable`、`autoReuse`、
`tileBoost`、`alpha`、`scale`、`defense`、`headSlot`、`bodySlot`、`legSlot`、`handOnSlot`、
`handOffSlot`、`backSlot`、`frontSlot`、`shoeSlot`、`waistSlot`、`wingSlot`、`shieldSlot`、
`neckSlot`、`faceSlot`、`balloonSlot`、`beardSlot`、`voiceSlot`、`rare`、`ammo`、`notAmmo`、
`lifeRegen`、`manaIncrease`、`buyOnce`、`mana`、`noUseGraphic`、`value`、`buy`、`social`、
`vanity`、`material`、`buffType`、`buffTime`、`mountType`、`uniqueStack`、
`chlorophyteExtractinatorConsumable`、`crit`、`armorPenetration`、`bonusTagDamage`、`melee`、
`magic`、`ranged`、`summon`、`reuseDelay`、`hasVanityEffects`、`foodWidth`、`foodHeight`、
`WALL_PLACEMENT_USETIME`、`SlotsRemainingBeforeEmergencyStackingInMultiplayer`。

目标映射：

- Tile/Wall/样式/增益进入 `ItemPlacementDefinition`；放置动作由 `ItemPlacementSystem` 校验并
  产生 Tile 命令，不能由 Item 直接改世界。
- 生命、法力、Buff 类型/持续时间进入 `ItemRecoveryDefinition`，效果提交到玩家状态系统。
- `flaskTime` 的 72000 tick source constant 现在由 `ItemDefinition.FlaskDurationTicks` 统一
  消费；`LegacyItemDefinitionAdapter` 和默认 `DomeSimulation` definitions 不允许已知 Flask
  的 buff/duration mapping 漂移，`ItemUseSystem` 将该 duration 提交到
  `BuffCollectionComponent`。完整效果修正、目标过滤和跨层投影仍延期。
- 防御、生命恢复、Accessory/Vanity/Social 和槽位进入 `ItemEquipmentDefinition`；装备结果
  通过 `EquipmentSnapshot` 投影到协议。
- 伤害、击退、暴击、护甲穿透、伤害类别和弹药引用进入 `ItemCombatDefinition`；使用时间、
  动画、样式、冷却、channel、自动重复和射击引用进入 `ItemUseDefinition`。
- `chlorophyteExtractinatorConsumable` 已有 `ItemExtractinatorDefinition`/规则注册表切片，
  但不代表所有旧 Extractinator 入口均已 parity。
- `PaintOrCoating`、`OriginalRarity`、`OriginalDamage`、`OriginalDefense` 是派生属性：前者由
  实例染色/涂层状态计算，后三者从不可变 Definition 读取原始值；它们不能继续回查旧
  `ContentSamples.ItemsByType`。

## 5. System 成员（20）

这些成员表达行为选择、派生值或全局处理策略。它们不应作为可任意写入的公共字段留在
`Item` 实例中。

`isAShopItem`、`useStyle`、`useAnimation`、`useTime`、`pick`、`axe`、`hammer`、`createTile`、
`createWall`、`placeStyle`、`useTurn`、`shoot`、`shootSpeed`、`useAmmo`、`shootsEveryUse`、
`prefix`、`PickupReplacementTime`、`Variant`、`IsACoin`、`IsAir`。

迁移规则：

| 成员族 | ECS 处理 | 当前证据/缺口 |
| --- | --- | --- |
| 使用与放置 | `ItemUseSystem`、`ItemPlacementSystem` 读取 Definition 并产生命令 | 使用、放置、冷却和拒绝路径已有验证；完整旧使用样式仍非全量 parity |
| 工具强度 | `pick`、`axe`、`hammer` 进入工具能力 Definition，由 Tile 系统消费 | 需要逐项绑定旧 Tile 交互和有限值校验 |
| 射击与弹药 | `ItemUseDefinition`/`ItemCombatDefinition` + `ItemAmmoConsumptionSystem` | 弹药不足和一次性扣除已有验证；完整射击族与旧 `shoot` 分派仍有缺口 |
| 前缀与变体 | `ItemPrefixSystem`、`ItemVariantSystem`，实例只保存 `PrefixId`/`VariantId` | 变更命令存在；完整旧 prefix 表、重建默认值和 `Refresh` 仍延期 |
| 商店/拾取策略 | 交易系统和世界物品系统消费 typed policy | `PickupReplacementTime` 有 owner 方向；ordinary custom price 与 Item-backed special currency 已有窄交易 owner，完整商店和旧 slot/cache 语义未迁移 |
| 派生属性 | `IsACoin`、`IsAir`、`Variant` 由定义/实例状态计算 | 不能依赖旧 `ContentSamples` 或全局数组；需补齐定义注册覆盖 |

## 6. Component 成员（3）

| 旧成员 | 当前承载 | 判定 |
| --- | --- | --- |
| `favorited` | `ItemInstanceStateComponent.IsFavorited`，由库存/装备快照和持久化投影 | 运行时已验证；仍需完整账号/世界恢复 round-trip |
| `newAndShiny` | `ItemInstanceStateComponent.IsNewAndShiny`，由实例快照/协议投影 | 运行时已验证；不能把客户端展示含义扩大为服务器规则 |
| `active` | `ItemWorldStateComponent.IsActive` + `ItemStack.IsEmpty` 不变量 | 运行时已验证；世界物品使用单调 revision tombstone |

组件必须是小型值语义。名称覆盖、染色、前缀、变体等实例值不能塞回共享 Definition；
世界激活/拾取延迟/保留玩家不能写入 `ItemStack`。

## 7. Compatibility 成员（8）

`coinGrabRange`、`lifeGrabRange`、`treasureGrabRange`、`restorationDelay`、`eggnogDelay`、
`mushroomDelay`、`staff`、`claw`。

这些成员只允许作为旧数据/旧调用形状的边界输入。当前适配器是：

- `LegacyItemDefinitionAdapter`：明确的旧定义记录 -> `ItemDefinition`。
- `LegacyItemImportAdapter`：持久化旧物品记录 -> `ItemInstanceSnapshot`。
- `LegacyItemDropAdapter`：明确的旧掉落记录 -> `CreateWorldItemCommand`。

`staff` 和 `claw` 这类按类型索引的旧数组不能直接成为 Simulation 全局可变数组；必须转成
版本化定义注册表或在兼容层只读投影。兼容层不得引用 `Main.item`、`Player.inventory`、
`ContentSamples` 或 `ItemID.Sets` 来绕过权威校验。

## 8. Deferred 成员（20）

延期不是“删除字段”，而是保留明确的 owner 和准入条件。当前成员如下：

`goldCritterRarityColor`、`wornArmor`、`tooltipContext`、`tooltipSlot`、`holdStyle`、`color`、
`glowMask`、`UseSound`、`useSoundPitch`、`stringColor`、`ToolTip`、
`BestiaryNotes`、`noMelee`、`noWet`、`cartTrack`、`shopSpecialCurrency`、`shopCustomPrice`、
`DD2Summon`、`sentry`、`_phaseColors`。

### 8.1 客户端表现延期

`goldCritterRarityColor`、`wornArmor`、`tooltipContext`、`tooltipSlot`、`holdStyle`、`color`、
`glowMask`、`UseSound`、`useSoundPitch`、`stringColor`、`ToolTip`、`BestiaryNotes`、
`_phaseColors` 以及与 Tooltip/绘制相关的派生行为属于客户端投影。它们不能为了“字段覆盖”
进入 Simulation；准入条件是定义/快照已有稳定读取接口，并且客户端 owner 不反写权威状态。

### 8.2 尚未建立的服务端行为域

`flaskTime` 已有 source-backed Definition duration、Flask registry、兼容适配和服务器 Buff
consumer；完整 Potion/Flask effect modifiers、target filtering、damage/hit behavior、UI、协议
和持久化仍需独立效果域。`noMelee` 已有 item-animation 唤醒窄 owner，但完整玩家睡眠行为
仍需独立域；`cartTrack` 需要矿车/轨道
交互域；`shopSpecialCurrency` 与 `shopCustomPrice` 已有 Item/NPC 交易窄 owner，Batch FY
进一步为 authoritative settlement 增加 source-backed typed `OnPurchase` cleanup receipt，Batch FZ
由 `ShopOfferCatalogSystem` 在 tick commit 后消费回执并更新注册 offer；但完整
CustomCurrencyManager、动态 NPC catalog 生成、动态 OnPurchase reset、UI/协议/持久化和
slot/cache parity 仍需独立交易域；
`DD2Summon` 需要 DD2 世界事件域；`sentry` 需要持续炮台投射物生命周期域；`noWet` 需要明确的
湿润状态消费路径。

对仍在 Deferred 清单中的成员，在这些 owner 和消费路径建立前，不能把常量或布尔字段移到
通用 Definition 就算完成；Batch FX 的 `flaskTime` 仅完成上述窄效果链，不能扩大为完整
Potion/Flask parity。

## 9. 已确认的运行时证据

以下证据支持“部分迁移”而不是“全量完成”：

| 证据 | 结论 |
| --- | --- |
| `src/Terraria.Dome.Simulation/Items/ItemDefinition.cs` 及 `Items/Definitions/*` | 定义使用不可变 `record struct`；基础常量、尺寸/价值/稀有度、alpha/scale、使用、战斗、放置、恢复、装备、前缀和 Extractinator 已有明确类型 |
| `ItemDefinitionRegistry` / `ItemDefinitionCompiler` | 重复类型、空类型、堆叠上限、未知依赖、矛盾弹药、非法装备/放置/恢复/战斗元数据被拒绝 |
| `ItemInstanceStateComponent` / `ItemWorldStateComponent` / `ItemStack` | 实例与定义分离；空实例、激活、拾取保留、revision 和数量不变量有代码承载 |
| `InventoryComponent`、`InventoryTransferSystem`、`ItemUseSystem`、`ItemEquipmentSystem`、`ItemAmmoConsumptionSystem` | 库存转移、使用、装备、弹药和无副作用拒绝已有窄路径 |
| `WorldItemSpawnSystem`、`WorldItemPickupSystem`、`WorldItemStore` | 世界物品生成、位置有限值校验、拾取竞争、tombstone 和 PVS 投影已有窄路径 |
| `ItemPrefixSystem`、`ItemVariantSystem`、Extractinator 系统 | 前缀/变体/Extractinator 有切片实现；完整旧表和 `Refresh` 仍缺 |
| `ItemInstanceSnapshot`、`WorldItemSnapshot`、库存/装备复制和 `DomeStatePersistenceFormat` | 实例元数据、世界状态、库存装备和持久化有投影，但不是完整 Item.cs parity |
| `Build/evidence/item-ecs/legacy-item-reference-audit.txt` | Simulation 生产代码没有直接命中 `Terraria.Item`、`Main.item`、`Player.inventory`、`ContentSamples`、`ItemID.Sets` |
| `Build/evidence/item-ecs/deferred-member-audit.md` | 历史 Deferred 基线有逐项原因和 owner；Batch FX/FV/FW/FY/FZ 的当前窄 owner 由本报告与新证据覆盖，剩余 Deferred 数量为 20 |
| `Build/diagnostics/item-shop-special-currency-final-20260831/source-contract.txt`；`Build/diagnostics/item-shop-special-currency-final-rerun-20260831/summary.txt` | `shopSpecialCurrency` 的 legacy registry、Defender Medals Item-backed cap、特殊支付原子边界、满库存释放槽位和最终 rerun 证据 |
| `Build/diagnostics/item-flask-duration-final-20260831/source-contract.txt`；`summary.txt`；`gate-status.tsv`；`items-run.log`；当前树重跑 [`Build/diagnostics/item-flask-duration-final-20260831-rerun-01/`](../../Build/diagnostics/item-flask-duration-final-20260831-rerun-01/) | Batch FX 的 `flaskTime=72000` Definition owner、八项 item-to-buff mapping、adapter conflict rejection、默认 definitions 和服务器 Buff application；当前树重跑的构建/运行 gate exit 0 |
| `Build/diagnostics/item-shop-onpurchase-final-20260831-03/source-contract.txt`；`summary.txt`；`gate-status.tsv`；`items-run.log` | Batch FY 的 `Item.OnPurchase` custom-price guard、typed cleanup receipt、ordinary/special settlement 后发布边界；Items verifier `67 PASS`，Simulation、Definitions、Loopback 和 Server Release gates exit 0，拒绝与 queued 路径无 receipt |
| [`Build/diagnostics/item-shop-catalog-cleanup-final-20260831/`](../../Build/diagnostics/item-shop-catalog-cleanup-final-20260831/) | Batch FZ 的 catalog/cache receipt consumer、invalid identity rejection、tick 边界和同 tick 后续购买；Items verifier `69 PASS`，Simulation/Items/Loopback build exit `0`，Loopback serial rerun exit `0` |
| [`Build/diagnostics/item-shop-catalog-refresh-20260831-01/`](../../Build/diagnostics/item-shop-catalog-refresh-20260831-01/) | Batch GA 的 generated-offer catalog 原子刷新；Items verifier `70 PASS`，Items/Simulation/Definitions focused build/run exit `0`，非法/重复/未知 Item 输入保持旧 catalog，空结果清空 |

## 10. 迁移批次与验收

### P0：冻结定义契约

1. 从 `Item.cs` 提取默认值、可变性和引用关系，生成版本化 Definition 输入。
2. 让 `ItemDefinitionCompiler` 覆盖所有已纳入 Definition 的字段，并对数组/索引引用做缺失
   拒绝；禁止从 `ContentSamples` 运行时回查。
3. 验收：每个 Definition 成员都有默认值、owner、验证规则和序列化决定；注册表验证器通过。

### P1：闭环实例和行为

1. 补齐 `ItemUseStateComponent`、装备状态、所有权和使用快照中仍缺的字段。
2. 将 `SetDefaults`、prefix、variant、drop、shop、shimmer 的可迁移语义拆成纯规则和 typed
   command；命令在 tick 边界确定性提交。
3. 验收：使用/弹药/装备/放置/掉落/拾取/堆叠在重复 tick 和拒绝输入下无额外副作用。

### P2：持久化和协议投影

1. 为实例、库存、装备、世界物品和前缀/变体补齐 save/reload round-trip。
2. 验证 PVS、revision、tombstone、伪造客户端包拒绝和重连恢复。
3. 验收：Server 只从不可变快照构造协议帧，客户端输入不能反写 Item 权威状态。

### P3：延期 owner 资格审查

每个 Deferred 成员必须先有独立 owner、消费路径、快照/协议边界和回归验证，才允许从
`Deferred` 移到 `Definition`、`System` 或 `Component`。客户端表现字段不因存在同名定义就
自动转为服务器迁移。

## 11. 验证命令

从仓库根目录串行执行，避免共享编译输出竞争：

```powershell
dotnet run --project Test/Terraria.Dome.Items.Definitions.Verification/Terraria.Dome.Items.Definitions.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Items.Verification/Terraria.Dome.Items.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Items.Loopback.Verification/Terraria.Dome.Items.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test/Terraria.Dome.Persistence.Verification/Terraria.Dome.Persistence.Verification.csproj -c Release -p:UseSharedCompilation=false
rg -n --hidden -S "Terraria\.Item|Main\.item|Player\.inventory|ContentSamples|ItemID\.Sets" src/Terraria.Dome.Simulation
```

判定要求：

- 四个验证器均以退出码 0 完成，且记录警告/错误；
- legacy 引用查询在 Simulation 生产代码中无命中，适配器例外必须有清单记录；
- 失败时保留 `PARTIAL`，不能用单个绿色验证器替代完整成员 parity；
- 新增报告和证据只能进入 `docs/` 或 `Build/evidence/`，不能把编译产物写入 `src/`。

### 11.1 本次报告生成的验证记录

以下命令于 2026-08-28 从仓库根目录新鲜执行，均使用 `-p:UseSharedCompilation=false`、
`-p:MSBuildNodeReuse=false` 和 `-m:1`：

| 命令 | 退出码 | 关键输出/判定 |
| --- | ---: | --- |
| `Terraria.Dome.Items.Definitions.Verification` | 0 | `PASS: immutable item definition registry accepts valid data and rejects invalid references` |
| `Terraria.Dome.Items.Verification` | 0 | Item/Inventory/WorldItem/Use/Equipment/Drop/Prefix/Variant/Persistence 窄路径均输出 `PASS` |
| `Terraria.Dome.Items.Loopback.Verification` | 0 | `PASS: SyncItem is PVS-limited, server-owned and tombstone-replicated` |
| `Terraria.Dome.Persistence.Verification` | 1 | 在既有 `VerifyDeterministicBaseGeneration`（`Program.cs:1590`）失败：`Base generation did not create ground and clear spawn.`；不是 Item 断言，故整体仍为 `PARTIAL` |

这组结果证明 Item 相关窄路径可以重放，但不能证明完整持久化回归或全量旧 Item 行为等价。

### 11.1.1 Batch FX：`flaskTime` duration owner

Batch FX 先以 RED verifier 锁定缺口：Definitions 和 Items verifier 在缺少
`ItemDefinition.FlaskDurationTicks`、`LegacyFlaskDefinition` 与
`LegacyFlaskDefinitionRegistry` 时分别失败（两项 exit `1`）。GREEN 后的 source-backed
边界如下：

- `Item.cs:40` 的 `flaskTime = 72000` 由 `ItemDefinition.FlaskDurationTicks` 持有。
- `LegacyFlaskDefinitionRegistry` 固定八项映射：`1340 -> 71`、`1353 -> 73`、`1354 -> 74`、
  `1355 -> 75`、`1356 -> 76`、`1357 -> 77`、`1358 -> 78`、`1359 -> 79`。
- `LegacyItemDefinitionAdapter` 对已知 Flask 补齐 source buff、duration 和 consumable，并在
  Definition 创建前拒绝冲突输入；`DomeSimulation` 默认注册这些 definitions。
- `ItemUseSystem` 发布 `ItemUsedEvent` 并把相同 duration 写入 `BuffCollectionComponent`，
  因而 duration 不是只存在于常量或 verifier fixture 中。

证据入口为 [`Build/diagnostics/item-flask-duration-final-20260831/`](../../Build/diagnostics/item-flask-duration-final-20260831/)，
当前树重跑为 [`Build/diagnostics/item-flask-duration-final-20260831-rerun-01/`](../../Build/diagnostics/item-flask-duration-final-20260831-rerun-01/)。
重跑目录的 Simulation、Items Definitions、Items、Items Loopback 和 Server Release gate 均为
exit `0`；Items verifier 输出 Flask PASS，legacy/tab 扫描为预期无命中，`git diff --check`
为 `0`。首次聚合尝试的 Definitions 增量编译曾因缺少 Simulation ref assembly 报 `CS0006`，
直接串行重跑后已恢复为 `0`，不改变代码边界。完整 Flask effect modifiers、target filtering、damage/hit behavior、UI、协议、持久化
和完整 Potion/Flask parity 继续 `deferred`。

### 11.1.2 Batch FY：`OnPurchase` cleanup receipt owner

Batch FY 先以 RED verifier 固定成交后清理回执和 tick 发布缺口：在缺少
`ShopPurchaseReceipt`、`ShopPurchaseResult.Receipt` 与 `DomeSimulation` receipt publication
时，Items verifier build exit `1`。RED 证据为
`Build/diagnostics/item-shop-onpurchase-red-20260831/verifier-red-build.log`。

GREEN 后的 source-backed 边界如下：

- legacy `Item.cs:270-272` 规定 `shopSpecialCurrency` 的普通货币 sentinel `-1` 与 nullable
  `shopCustomPrice`；`Item.cs:49689-49698` 的 `OnPurchase` 只在
  `shopCustomPrice.HasValue` 时同时将 special currency 重置为 `-1`、custom price 清为
  `null`；`Item.cs:49700-49708` 的 `GetStoreValue()` 仍是 custom-or-base 解析。
- `ShopPurchaseReceipt` 是不可变 typed cleanup 投影。只有 authoritative offer 的
  `CustomPriceCopper.HasValue` 才把 `ResetCustomPrice` 与 `ResetSpecialCurrency` 同时置为
  `true`，并把 `OfferAfterCleanup` 的 custom price 设为 `null`、special currency 设为 `-1`。
  没有 custom price 的 special-currency offer 不产生 reset；receipt 不接受客户端价格、折扣
  或 `PriceAdjustment` 上下文。
- `ShopPurchaseSystem` 在 ordinary-currency 和 Item-backed special-currency 成交后返回 receipt；
  forged session、无效 NPC、未注册 offer、排队但尚未 tick commit 或其它拒绝路径不返回 receipt，
  也不改变库存或 `BuyOnce` 状态。`DomeSimulation.Shop.cs` 只在原子库存结算成功后发布，
  因而顺序保持 `Definition -> System -> command -> tick commit -> receipt`。

最终证据入口为 [`Build/diagnostics/item-shop-onpurchase-final-20260831-03/`](../../Build/diagnostics/item-shop-onpurchase-final-20260831-03/)。
该目录的 `gate-status.tsv` 记录 Simulation、Definitions、Items、Loopback、Server build/run
均为 exit `0`；Items verifier 的 `items-run.log` 输出 `67 PASS`。legacy-reference 与 tab
扫描为预期无命中（`rg` exit `1`），`git diff --check` exit `0`。本批只完成 typed
cleanup/receipt owner；动态 NPC catalog/cache mutation、动态 OnPurchase reset 的后续消费、
折扣与 `PriceAdjustment`、client/UI、协议、持久化、bank inventories 和完整 Terraria shop
parity 继续 `deferred`。

### 11.1.3 Batch FZ：成交回执驱动的 Shop catalog cleanup

Batch FZ 先以 TDD RED 固定消费缺口：直接引用 `ShopOfferCatalogSystem` 时 Items verifier
build 以 `CS0246` 失败；去掉 tick 接线后，运行时 verifier 在成功成交后仍观察到旧
`CustomPriceCopper`，以 exit `1` 失败。对应 RED 运行证据保留在本批工作记录中。

GREEN 后的 source-backed 边界如下：

- legacy `Item.cs:270-272` 的 `shopSpecialCurrency = -1` 与 nullable `shopCustomPrice`，以及
  `Item.cs:49689-49698` 的 custom-price guard，继续由 Batch FY 的 typed receipt 表达；本批
  不重新实现折扣、`PriceAdjustment` 或客户端价格输入。
- `ShopOfferCatalogSystem` 只接受当前注册 offer 的有效 receipt。custom-price receipt 的
  `OfferAfterCleanup` 必须保持 offer 身份、商品数量、基础价格和 `BuyOnce` 不变，并将
  `CustomPriceCopper` 清为 `null`、`SpecialCurrencyId` 清为 `-1`；身份不匹配的 receipt
  fail-closed 且不修改 catalog。无 custom price 的 receipt 是不改变 offer 的消费确认。
- `DomeSimulation.CommitShopPurchases` 在库存原子结算成功后消费 receipt，再发布 receipt
  事件；排队但未提交的命令、无效 session 和拒绝路径不触碰 catalog。由于 consumer 在同一
  tick 的 purchase loop 中更新 `_shopOffers`，后续命令读取清理后的 offer 并按基础价格结算。

最终证据入口为 [`Build/diagnostics/item-shop-catalog-cleanup-final-20260831/`](../../Build/diagnostics/item-shop-catalog-cleanup-final-20260831/)。
`items-run.log` 输出 `69 PASS` 且无 `FAIL`/`ERROR`；Simulation、Items verifier 和 Items
Loopback Release build 均 exit `0`。Loopback 首次运行命中既有世界物品 tombstone/PVS 瞬态断言，
串行 rerun exit `0`；legacy-reference/tab scan 为预期无命中（rg exit `1`），targeted
`git diff --check` exit `0`。本批仅完成 receipt 驱动的静态注册 offer cleanup；动态 NPC
catalog 生成、折扣/`PriceAdjustment`、client/UI、协议、持久化、bank inventories 和完整
Terraria shop parity 继续 `deferred`。

### 11.1.4 Batch GA：生成结果驱动的 Shop catalog 原子刷新

Batch GA 以 TDD RED 固定缺口：Items verifier 在 `DomeSimulation` 尚无
`TryRefreshGeneratedShopCatalog` 时构建失败，重跑 exit `1` 且仅有 4 个预期 `CS1061`。

GREEN 后的 source-backed 边界如下：

- legacy `Chest.SetupShop(int type)`（`Chest.cs:1403-1414`）先清空既有 item slots 再填充，
  `Main.OpenShop(int shopIndex)`（`Main.cs:40591-40599`）在打开商店时重新调用；本批把这一
  清空后填充的 cache replacement 边界建模为已解析 generated offers 的权威原子刷新。
- `ShopOfferCatalogSystem.ReplaceGeneratedOffers` 先校验所有 offer 和唯一 `OfferId`，成功
  后才清空并写入共享 `_shopOffers`；空生成结果清空 catalog，非法、重复或中途失败输入
  不产生部分写入。
- `DomeSimulation.TryRefreshGeneratedShopCatalog` 额外校验 authoritative `ItemDefinition`
  和 `StackLimit`，因此 purchase system 继续读取同一经过准入的 catalog。

最终证据入口为 [`Build/diagnostics/item-shop-catalog-refresh-20260831-01/`](../../Build/diagnostics/item-shop-catalog-refresh-20260831-01/)。
Items verifier `70 PASS`，Items/Simulation/Definitions focused build/run 均 exit `0`，0
warnings/errors；legacy-reference/tab scan 为预期无命中（rg exit `1`），targeted
`git diff --check` exit `0`。本批只完成 source-resolved generated offer 的 refresh/cache owner；
NPC 条件求值、shop slot/travel-shop、动态 NPC catalog 生成、动态 OnPurchase reset、折扣/
`PriceAdjustment`、client/UI、协议、持久化、bank inventories 和完整 Terraria shop parity
继续 `deferred`。

## 11.2 Flowstate 低上下文执行控制

本报告由 Flowstate `N4 -> N6` 管理；它是迁移边界和证据入口，不是代码实现计划。
后续续跑只读取本节、成员映射表、最新 checkpoint 和本批证据，不复制本报告全文。

| 节点 | 只保留的输入 | 产出/闸门 |
| --- | --- | --- |
| N4 迭代 | 一个行为族、一个 owner、一个验收句 | 私区 task；禁止顺手扩展到其他字段族 |
| N5 变更 | 新增字段/owner 或结论变化 | 记录影响；若改变权威边界则暂停并重新评审 |
| N6 验收 | 变更行、直接消费者、定向验证输出 | `verified`、`partial`、`deferred` 或 `not-run`，不得用归属代替行为证据 |

低上下文协议：每轮最多携带 6 条事实（目标、当前节点、写集、阻塞、下一步、证据路径），
详细成员仍以 `item-ecs-member-mapping.md` 为唯一索引。不得把 `progress.md`、Build 日志或
全量源码塞入上下文；需要细节时按成员或证据路径定点读取。

验证预算固定为完整回归的约 30%：每批只运行定义注册、Item 窄路径、协议/PVS 三类定向检查，
再执行一次 legacy 引用扫描。持久化全回归已知存在非 Item 的基线失败，保持 `PARTIAL`，不因
其它检查变绿而升级结论。任何新增证据必须写入 `Build/evidence/item-ecs/` 或本节对应文档。

当前 Flowstate checkpoint：`N6 / completed_with_partial_findings`；P0 基础定义、gathering、
召唤/坐骑、共享规则常量、Potion/Expert 身份、装备 ManaIncrease 消费、vanity-effects 元数据、Appearance 默认值、ReuseDelay 行为、Combat Flame/Mech 元数据、vanity 资格校验、Batch FX `flaskTime` duration owner、Batch FY `OnPurchase` cleanup receipt owner、Batch FZ catalog cleanup consumer 和 Batch GA generated-offer catalog refresh boundary 批次已实现并通过定向 verifier；视觉、完整 Flask effect、NPC 条件驱动的动态商店 catalog 生成和专用 projectile 效果仍 deferred，下一批继续按字段族补齐“默认值 -> owner ->
消费者 -> 快照/持久化 -> verifier”五段证据链。

## 12. 最终判定

> **Item.cs field/property ECS migration: PARTIAL.**
>
> 148 个非方法字段/属性已经有明确归属；其中 `flaskTime` 已形成
> `Definition -> Recovery -> ItemUseSystem -> BuffCollectionComponent` 的
> `completed_partial` 窄链，`OnPurchase` 已形成 authoritative settlement 后的 typed cleanup
> receipt，并由 `ShopOfferCatalogSystem` 在 tick commit 后更新注册 offer；Batch GA 又为已解析 generated offers 提供全量校验后的 catalog 原子刷新；Item.cs 按 ID 语义排除 0 项。当前运行时已形成
> 定义、实例组件、系统、快照和兼容适配器的骨架，并对库存、使用、装备、弹药、世界物品、
> Extractinator、持久化和协议投影完成若干窄路径验证。完整 `SetDefaults`/静态表、prefix 和
> variant 重建、商店/shimmer、专用行为以及 UI/渲染/音频仍未等价，因此不能宣称 Item ECS
> 迁移完成，也不能物理删除旧 Item 源码。
