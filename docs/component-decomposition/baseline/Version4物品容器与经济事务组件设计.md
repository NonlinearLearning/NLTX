# Version4“物品、容器与经济事务”组件设计

## 1. 范围与约定

本文是《[Version4“物品、容器与经济事务”组件拆分报告](Version4物品容器与经济事务组件拆分报告.md)》
的状态模型落地稿。它只给出**每个 ECS 组件的字段和只读派生属性**，不定义 System、
Query、Command、网络 DTO、持久化 DTO、测试、执行顺序或迁移步骤。

字段按 `public-decomposition` 的状态所有权拆分：组件只保存一个稳定、同生命周期的权威
状态概念；跨实体转移、价格计算、掉落随机、网络复制和表现均不是组件字段。所有实体关系
均使用当前 `Terraria.Relationships.EntityReference`；`EntityReference.None` 表示无引用，
不以槽位索引或网络 ID 代替实体身份。

本文中的 `ContentId`、`DefinitionRevision`、`WorldTick`、`ContainerRevision`、
`TransactionId`、`AccountId` 和 `TileCoordinates` 是值对象/标识符的设计名，须在相应的
共享领域或基础设施边界定义，不能把外部 API 类型渗入 `Terraria.Items` 组件。`Guid.Empty`、
`0` 和 `EntityReference.None` 分别只表示其字段表明的“未分配/初始/无关系”状态。

## 2. 现有组件的目标字段

以下七个组件已存在于 `src/Items/`。本节定义其目标字段形状；除非后续迁移任务明确要求，
本文不修改现有 C# 文件。

### 2.1 `ItemDefinitionComponent`

**归属**：物品实体。**权威状态**：该实例所引用的只读内容定义。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `ContentId` | `int` | 无默认有效值 | 旧 `Item.type` 的内容目录键；只用于查找定义。 |
| `DefinitionRevision` | `int` | `0` | 创建实例时采用的定义版本；用于存档/兼容检查，不缓存定义字段。 |
| `HasDefinition` | `bool`（只读派生） | `false` | `ContentId > 0`。 |

不保存 `damage`、`value`、`maxStack`、`useTime`、装备槽或价格；它们属于 `ContentCatalog`
中的只读定义，避免内容热加载/版本变化时实例产生陈旧副本。

### 2.2 `ItemInstanceComponent`

**归属**：物品实体。**权威状态**：内容定义之外、可随该物品持久化的实例差异。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `PersistentInstanceId` | `Guid` | `Guid.Empty` | 存档层的稳定实例身份；不是 ECS 实体 ID。 |
| `PrefixId` | `int` | `0` | 前缀/修饰语标识；`0` 表示无前缀。 |
| `VariantId` | `int` | `0` | 内容变体标识；`0` 表示默认变体。 |
| `NameOverride` | `string?` | `null` | 玩家或规则指定的实例名称；`null` 使用内容定义名称。 |
| `DyeId` | `int` | `0` | 物品实例指定的染料/外观键；不保存渲染资源。 |
| `IsFavorited` | `bool` | `false` | 收藏保护标志。 |
| `HasPersistentIdentity` | `bool`（只读派生） | `false` | `PersistentInstanceId != Guid.Empty`。 |
| `HasNameOverride` | `bool`（只读派生） | `false` | `!string.IsNullOrWhiteSpace(NameOverride)`。 |

### 2.3 `StackableItemComponent`

**归属**：物品实体。**权威状态**：可合并物品的当前数量和由定义/变体决定的合并键。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Quantity` | `int` | `0` | 当前数量；`0` 仅允许在清空/销毁提交窗口内存在。 |
| `MaximumQuantity` | `int` | `1` | 本实例允许的最大数量，创建时从定义快照取得。 |
| `StackKey` | `ulong` | `0` | 内容、前缀、变体和不可合并实例差异计算的稳定键；不能以显示名称计算。 |
| `IsUnlimited` | `bool` | `false` | 特殊规则下消费不减少数量的权威标志。 |
| `RemainingCapacity` | `int`（只读派生） | `0` | `max(0, MaximumQuantity - Quantity)`。 |
| `IsEmpty` | `bool`（只读派生） | `true` | `Quantity <= 0`。 |
| `IsFull` | `bool`（只读派生） | `false` | `Quantity >= MaximumQuantity`。 |

`StackKey` 属于数量状态，是为了确保合并资格在内容版本变化后仍可明确判定；不保存其他
物品的实体引用，也不保存目标槽位或转移中的临时数量。

### 2.4 `InventoryComponent`

**归属**：玩家实体。**权威状态**：个人背包布局、所选槽位及槽位实例关系。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Slots` | `IReadOnlyList<EntityReference>` | 空列表 | 有序的普通背包槽位；每个元素是物品实体或 `None`。 |
| `SelectedSlot` | `int` | `-1` | 当前选中普通背包槽位；`-1` 表示没有可选槽。 |
| `TrashSlot` | `EntityReference` | `None` | 垃圾槽中的物品。 |
| `AmmoSlots` | `IReadOnlyList<EntityReference>` | 空列表 | 有序弹药槽位；与普通背包保持独立。 |
| `CoinSlots` | `IReadOnlyList<EntityReference>` | 空列表 | 有序钱币槽位；只是物品槽位而不是账户余额。 |
| `Revision` | `long` | `0` | 每次此布局提交成功后递增的乐观并发版本。 |
| `HasSelectedSlot` | `bool`（只读派生） | `false` | `SelectedSlot` 位于 `Slots` 范围内。 |
| `SelectedItem` | `EntityReference`（只读派生） | `None` | 有效 `SelectedSlot` 对应的物品实体，否则 `None`。 |

银行、箱子、商店和装备不纳入该组件；它们拥有独立的生命周期和访问规则。`Slots` 的具体
可变集合应是组件内部实现细节，对外只暴露只读快照/枚举。

### 2.5 `EquipmentComponent`

**归属**：玩家实体。**权威状态**：装备、外观和染料槽位中的物品关系及隐藏选择。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `FunctionalSlots` | `IReadOnlyDictionary<EquipmentSlot, EntityReference>` | 空字典 | 头部、身体、腿部、饰品等生效装备。 |
| `VanitySlots` | `IReadOnlyDictionary<EquipmentSlot, EntityReference>` | 空字典 | 仅外观覆盖装备。 |
| `DyeSlots` | `IReadOnlyDictionary<EquipmentSlot, EntityReference>` | 空字典 | 对应装备槽的染料物品。 |
| `HiddenSlots` | `IReadOnlySet<EquipmentSlot>` | 空集合 | 被玩家显式隐藏的外观槽。 |
| `Revision` | `long` | `0` | 装备关系提交版本。 |
| `IsHidden` | `bool`（索引只读属性） | `false` | `IsHidden[slot]` 判断槽位是否在 `HiddenSlots` 中。 |

不保存防御、移动速度、宠物或光源等最终效果；它们由装备效果计算从定义和此关系投影得到。

### 2.6 `HandsComponent`

**归属**：可持有物品的实体，通常是玩家。**权威状态**：左右手关系。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Primary` | `EntityReference` | `None` | 主手物品实体。 |
| `Secondary` | `EntityReference` | `None` | 副手物品实体。 |
| `HasPrimary` | `bool`（只读派生） | `false` | `!Primary.IsEmpty`。 |
| `HasSecondary` | `bool`（只读派生） | `false` | `!Secondary.IsEmpty`。 |

手持关系不等同于背包选择槽：选择槽属于 `InventoryComponent`，把选中物品移入手中是转移
事务的结果。

### 2.7 `WeaponComponent`

**归属**：具备武器能力的物品实体。**权威状态**：来自定义的静态武器能力快照。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Damage` | `int` | `0` | 基础伤害快照；最终伤害由战斗层计算。 |
| `Knockback` | `float` | `0` | 基础击退快照。 |
| `UseAnimationTicks` | `int` | `0` | 使用动画持续 tick；不表示当前使用进度。 |
| `UseTimeTicks` | `int` | `0` | 连续使用间隔 tick。 |
| `ProjectileContentId` | `int` | `0` | 使用时请求生成的投射物定义；`0` 表示没有。 |
| `ProjectileSpeed` | `float` | `0` | 投射物基础速度。 |
| `AmmoCategoryId` | `int` | `0` | 所需弹药类别；`0` 表示不需弹药。 |
| `ConsumesAmmo` | `bool` | `false` | 此能力是否会请求弹药消耗。 |
| `CanSpawnProjectile` | `bool`（只读派生） | `false` | `ProjectileContentId > 0`。 |

## 3. 物品使用与制作组件

### 3.1 `ItemUseComponent`

**归属**：正在被使用的物品实体。**权威状态**：使用生命周期，不保存输入设备或表现动画。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Phase` | `ItemUsePhase` | `Idle` | `Idle`、`Starting`、`Using`、`Channeling`、`Cooldown`、`Interrupted`。 |
| `StartedAtTick` | `long` | `0` | 本次使用开始的世界 tick。 |
| `CooldownUntilTick` | `long` | `0` | 下一次允许开始使用的世界 tick。 |
| `Owner` | `EntityReference` | `None` | 当前合法使用者；使用结束后清空。 |
| `TargetEntity` | `EntityReference` | `None` | 目标实体；没有实体目标时为 `None`。 |
| `TargetTile` | `TileCoordinates?` | `null` | 目标格坐标；没有 Tile 目标时为 `null`。 |
| `UseSequence` | `long` | `0` | 同一物品的递增使用序号，用于去重生成请求。 |
| `IsActive` | `bool`（只读派生） | `false` | `Phase` 为 `Starting`、`Using` 或 `Channeling`。 |
| `IsCoolingDown` | `bool`（只读派生） | `false` | `CooldownUntilTick > StartedAtTick` 且状态为 `Cooldown`。 |

### 3.2 `CraftingComponent`

**归属**：玩家实体。**权威状态**：一次已接受但尚未终结的制作请求及其材料版本快照。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `ActiveRecipeId` | `int` | `0` | 当前配方定义键；`0` 表示没有活动请求。 |
| `RequestedQuantity` | `int` | `0` | 请求制作数量。 |
| `AcceptedAtTick` | `long` | `0` | 权威侧接受请求的 tick。 |
| `MaterialInventoryRevision` | `long` | `0` | 计算资格时玩家库存 revision；提交前必须仍匹配。 |
| `ReservedMaterials` | `IReadOnlyList<CraftingMaterialReservation>` | 空列表 | 被本请求锁定的物品实体、槽位和数量；不是实际扣除结果。 |
| `CraftSequence` | `long` | `0` | 玩家制作请求递增序号。 |
| `HasActiveCraft` | `bool`（只读派生） | `false` | `ActiveRecipeId > 0 && RequestedQuantity > 0`。 |

`CraftingMaterialReservation` 是值对象，字段为 `Item` (`EntityReference`)、`SourceSlot` (`int`)、
`Quantity` (`int`) 和 `ExpectedStackKey` (`ulong`)。配方定义、环境资格和制作结果都不缓存于
组件中。

## 4. 容器与世界掉落组件

### 4.1 `ContainerCapacityComponent`

**归属**：箱子、银行、商店或其他容器实体。**权威状态**：容器类别和容量规则。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Kind` | `ContainerKind` | `Generic` | `Generic`、`Chest`、`Bank`、`VoidVault`、`Shop`、`TileEntityStorage`。 |
| `SlotCount` | `int` | `0` | 固定的有序槽位数量。 |
| `MaximumWeight` | `int?` | `null` | 可选重量上限；`null` 表示不按重量限制。 |
| `AllowsNestedContainers` | `bool` | `false` | 是否允许存入包含其他物品的容器实体。 |
| `IsValid` | `bool`（只读派生） | `false` | `SlotCount >= 0` 且重量上限未为负。 |

### 4.2 `ContainerContentsComponent`

**归属**：容器实体。**权威状态**：有序槽位中的物品关系及内容版本。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Slots` | `IReadOnlyList<EntityReference>` | 空列表 | 与 `SlotCount` 一一对应的物品实体或 `None`。 |
| `Revision` | `long` | `0` | 任何插入、移除、交换、合并或拆分提交后的版本。 |
| `LastMutationTick` | `long` | `0` | 最后一次成功内容变更的世界 tick。 |
| `OccupiedSlotCount` | `int`（只读派生） | `0` | 非空 `Slots` 的数量。 |
| `IsEmpty` | `bool`（只读派生） | `true` | `OccupiedSlotCount == 0`。 |
| `IsFull` | `bool`（只读派生） | `false` | `Slots` 非空且所有槽位都非空。 |

不含容器坐标、访问者、权限、商店价格或容器类别，防止内容写者与访问/世界写者被重新耦合。

### 4.3 `ContainerAccessComponent`

**归属**：容器实体。**权威状态**：访问约束和当前访问租约。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `TilePosition` | `TileCoordinates?` | `null` | 世界容器绑定的格坐标；个人银行可为 `null`。 |
| `IsLocked` | `bool` | `false` | 容器是否拒绝一般访问。 |
| `RequiredAccessFlags` | `ulong` | `0` | 权限位集合；`0` 表示无附加权限。 |
| `CurrentAccessor` | `EntityReference` | `None` | 当前打开/编辑该容器的玩家。 |
| `AccessLeaseUntilTick` | `long` | `0` | 当前访问租约截止 tick；到期即不再视为独占。 |
| `IsShared` | `bool` | `true` | 是否允许多个授权角色读取；写入仍由事务 revision 保护。 |
| `HasActiveAccessor` | `bool`（只读派生） | `false` | `CurrentAccessor` 非空且租约未失效。 |

### 4.4 `WorldItemComponent`

**归属**：世界掉落物品实体。**权威状态**：掉落位置、运动和可拾取性，不复制 Item 定义或堆叠字段。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Position` | `WorldPosition` | 原点 | 世界位置。 |
| `Velocity` | `WorldVector` | 零向量 | 世界运动速度。 |
| `SpawnedAtTick` | `long` | `0` | 出现在世界中的 tick。 |
| `DespawnAtTick` | `long?` | `null` | 过期 tick；`null` 表示当前规则下不自动过期。 |
| `IsInstanced` | `bool` | `false` | 是否为玩家独立掉落。 |
| `IsBeingGrabbed` | `bool` | `false` | 已通过拾取资格、等待转移提交的临时权威状态。 |
| `IsOnConveyor` | `bool` | `false` | 正被世界传送带规则控制。 |
| `Revision` | `long` | `0` | 拾取、合并、移动或生命周期提交版本。 |
| `HasExpired` | `bool`（只读派生） | `false` | 以当前显式 tick 比较 `DespawnAtTick`；不读取系统时钟。 |

`WorldPosition`、`WorldVector` 由世界/物理领域定义；不得改用 XNA 或渲染层向量类型作为组件
公共字段。

### 4.5 `WorldItemReservationComponent`

**归属**：世界掉落物品实体。**权威状态**：拾取归属、保护和忽略规则。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `ReservedFor` | `EntityReference` | `None` | 当前被保留给的玩家。 |
| `ReservationUntilTick` | `long` | `0` | 玩家独占保留的截止 tick。 |
| `IgnoreOwner` | `EntityReference` | `None` | 暂时不能重新获得该物品的实体。 |
| `IgnoreOwnerUntilTick` | `long` | `0` | 忽略期截止 tick。 |
| `NoGrabUntilTick` | `long` | `0` | 所有玩家均不能拾取的截止 tick。 |
| `EnemyPickupBlockedUntilTick` | `long` | `0` | 敌对实体不能拾取的截止 tick。 |
| `HasReservation` | `bool`（只读派生） | `false` | `ReservedFor` 非空且未超过 `ReservationUntilTick`。 |
| `CanBeGrabbed` | `bool`（只读派生） | `false` | 由当前显式 tick 与 `NoGrabUntilTick` 比较。 |

### 4.6 `TileEntityBindingComponent`

**归属**：TileEntity 或与其一一对应的容器实体。**权威状态**：世界 TileEntity 的绑定身份。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `TileEntityKind` | `int` | `0` | TileEntity 内容类型键。 |
| `TilePosition` | `TileCoordinates` | 原点 | 绑定的世界格坐标。 |
| `PersistentTileEntityId` | `Guid` | `Guid.Empty` | 存档中的稳定 TileEntity 身份。 |
| `IsBound` | `bool`（只读派生） | `false` | `TileEntityKind > 0 && PersistentTileEntityId != Guid.Empty`。 |

### 4.7 `ShopInventoryComponent`

**归属**：商店实体。**权威状态**：可售条目、数量和库存版本；它不是一般 Chest 内容。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Offers` | `IReadOnlyList<ShopOffer>` | 空列表 | 有序商品条目。 |
| `Revision` | `long` | `0` | 库存或报价集合变更版本。 |
| `RestockAtTick` | `long?` | `null` | 下一次补货 tick；`null` 表示不自动补货。 |
| `LastRestockTick` | `long` | `0` | 最后一次成功补货 tick。 |
| `IsRestockDue` | `bool`（只读派生） | `false` | 当前显式 tick 达到 `RestockAtTick`。 |

`ShopOffer` 是值对象，字段为 `OfferId` (`int`)、`ItemContentId` (`int`)、`VariantId` (`int`)、
`AvailableQuantity` (`int`)、`MaximumQuantity` (`int`)、`BasePrice` (`long`) 和
`CurrencyId` (`int`)。动态价格快照、买方和事务 ID 不持久化在条目内。

## 5. 经济组件

### 5.1 `CurrencyBalanceComponent`

**归属**：玩家钱包、银行账户或商店账户实体。**权威状态**：按货币类型聚合的余额和版本。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `AccountId` | `Guid` | `Guid.Empty` | 账户的持久化外部身份；不是持有实体 ID。 |
| `Balances` | `IReadOnlyDictionary<int, long>` | 空字典 | `CurrencyId → amount` 的权威余额；不得存负值。 |
| `Revision` | `long` | `0` | 每次余额结算成功后的版本。 |
| `LastTransactionId` | `Guid` | `Guid.Empty` | 最近一次已提交事务，用于快速重复检测；完整去重由账本负责。 |
| `IsInitialized` | `bool`（只读派生） | `false` | `AccountId != Guid.Empty`。 |
| `TotalCurrencyKinds` | `int`（只读派生） | `0` | `Balances` 中正余额货币种数。 |

### 5.2 `CommerceLedgerComponent`

**归属**：账户或专用账本实体。**权威状态**：为去重和恢复保留的最小交易终结记录。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Entries` | `IReadOnlyList<CommerceLedgerEntry>` | 空列表 | 有界的已终结购买、出售、存款和取款记录。 |
| `LastSequence` | `long` | `0` | 账本记录的单调递增序号。 |
| `RetentionFloorSequence` | `long` | `0` | 已安全压缩/移除记录之前的最小保留序号。 |
| `ContainsUnknownOutcome` | `bool`（只读派生） | `false` | 是否存在需要通过权威提交状态继续确认的记录。 |

`CommerceLedgerEntry` 是值对象，字段为 `TransactionId` (`Guid`)、`Sequence` (`long`)、
`Kind` (`CommerceTransactionKind`)、`State` (`CommerceTransactionState`)、`CommittedAtTick`
(`long`)、`CurrencyId` (`int`)、`Amount` (`long`) 和 `Counterparty` (`EntityReference`)。
账本不保存 UI 文本、网络确认包或可重新计算的价格明细。

## 6. 掉落来源组件

### 6.1 `LootSourceComponent`

**归属**：可以触发掉落结算的 NPC、Boss Bag、容器、世界对象或事件实体。
**权威状态**：源实体的掉落规则选择键和一次性结算标记。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `LootTableId` | `int` | `0` | `LootRuleCatalog` 的规则表键，通常对应内容类型。 |
| `SourceKind` | `LootSourceKind` | `Unknown` | `Npc`、`BossBag`、`Container`、`WorldEvent`、`TileEntity`。 |
| `HasResolvedLoot` | `bool` | `false` | 源实体是否已成功提交掉落，防止重复结算。 |
| `ResolutionSequence` | `long` | `0` | 当前/最后一次掉落结算序号。 |
| `ResolvedAtTick` | `long?` | `null` | 成功提交掉落的 tick。 |
| `CanResolveLoot` | `bool`（只读派生） | `false` | `LootTableId > 0 && !HasResolvedLoot`。 |

### 6.2 `LootAttributionComponent`

**归属**：掉落源实体。**权威状态**：结算所需的归属快照，不持有掉落结果。

| 字段/属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Killer` | `EntityReference` | `None` | 最后归因的击杀者/触发者。 |
| `EligibleRecipients` | `IReadOnlyList<EntityReference>` | 空列表 | 可参与个人掉落的玩家快照。 |
| `LuckOwner` | `EntityReference` | `None` | 用于规则计算的幸运值来源。 |
| `SourcePosition` | `WorldPosition` | 原点 | 生成掉落的世界位置快照。 |
| `AttributionRevision` | `long` | `0` | 归属计算更新版本。 |
| `HasEligibleRecipients` | `bool`（只读派生） | `false` | `EligibleRecipients.Count > 0`。 |

不要建立持久 `LootRollComponent`：随机结果是一次结算中的不可变值对象或 Command payload，而不
是持续实体状态。`LootRuleCatalog` 同样是内容目录，不附着在每个掉落源实体上。

## 7. 非组件状态的明确排除

以下数据与物品、容器或经济有关，但按生命周期和访问模式不应设计为 ECS 组件字段：

| 数据 | 应归属 | 排除原因 |
| --- | --- | --- |
| Item 默认伤害、价值、最大堆叠、使用方式、装备能力 | `ContentCatalog` 定义 | 多实例共享且加载后只读，复制到每个实例会导致陈旧/冲突。 |
| Recipe、RecipeGroup、掉落规则树、货币定义 | 各自 Catalog | 规则是内容输入，不随实体生命周期创建/销毁。 |
| `TransferPlan`、`PriceSnapshot`、`LootRollResult`、制作结果 | 纯计算结果/Command payload | 仅在单次提交期间有效；持久化会形成过期缓存。 |
| `TransactionId` 的完整去重索引 | 账本/持久化服务 | 跨实体、跨会话的恢复边界，不应复制到每个物品或容器。 |
| 网络包、客户端鼠标物品、Tooltip、动画、音频、粒子 | Adapter/Projection | 外部观察或表现状态，不能反向成为玩法权威。 |
| 当前 UI 打开的窗口、购物车、价格文本 | UI Projection | 可由权威快照重建；不应改变库存、余额或容器关系。 |

## 8. 组件组合约束

| 实体类别 | 必需组件 | 可选组件 | 禁止组合/说明 |
| --- | --- | --- | --- |
| 普通物品 | `ItemDefinitionComponent`、`ItemInstanceComponent` | `StackableItemComponent`、`WeaponComponent`、`ItemUseComponent` | 没有 `WorldItemComponent` 时不在世界中活动。 |
| 玩家 | `InventoryComponent`、`EquipmentComponent`、`HandsComponent` | `CraftingComponent`、`CurrencyBalanceComponent`、`CommerceLedgerComponent` | 玩家不拥有 `ContainerContentsComponent` 作为普通背包替代；背包布局已有独立所有权。 |
| 世界掉落 | 物品组件 + `WorldItemComponent` | `WorldItemReservationComponent` | 不持有 `ContainerContentsComponent` 或 `ShopInventoryComponent`。 |
| 箱子/银行 | `ContainerCapacityComponent`、`ContainerContentsComponent`、`ContainerAccessComponent` | `TileEntityBindingComponent`、`CurrencyBalanceComponent` | `ShopInventoryComponent` 仅用于商店，不与普通内容数组混作同一权威库存。 |
| 商店 | `ContainerCapacityComponent`、`ContainerAccessComponent`、`ShopInventoryComponent` | `CurrencyBalanceComponent`、`CommerceLedgerComponent` | 商品库存由 `ShopInventoryComponent` 所有；不要用 `ContainerContentsComponent` 伪装有限货架。 |
| 掉落源 | `LootSourceComponent` | `LootAttributionComponent` | 不存 `LootRollResult` 或已生成物品实体列表。 |

以上组合保留“物品数量、容器关系、访问规则、世界生命周期、余额、账本和掉落归属”各自的
唯一权威所有者，避免把 Version4 的 `Item`、`Player`、`Chest` 或 `WorldItem` 巨型对象重新
组合成一个新组件。
