# Version4“物品、容器与经济事务”组件拆分报告

## 1. 结论摘要

本报告把 `docs/Version4权威游戏模拟系统主要子系统.md` 第 12 项定义的
“物品、容器与经济事务”拆成五条相互协作、但状态所有权不同的边界：

1. `ItemGameplay`：内容定义引用、物品实例、堆叠、装备和物品使用状态。
2. `WorldObjects`：世界掉落、箱子/银行/商店容器以及 TileEntity 绑定。
3. `InventoryTransferTransactions`：背包、容器、装备槽之间的资格判断和原子转移。
4. `CommerceTransactionSystem`：价格、货币、购买/出售、商店库存和账本事务。
5. `LootResolutionSystem`：掉落规则注册、条件/随机解析和生成掉落实体的提交。

当前 Version4 源码能够证明这些概念确实存在，但不能证明它们已经闭合实现。最重要的
缺口是：`Chest.SetupShop` 为空；`QuickStacking`、`NearbyChests` 和
`EmergencyStacking` 有多个空体；`CustomCurrencySystem` 的计数、支付和价格接口为空；
`CraftingRequests.NetCraftingRequestsModule.Deserialize` 返回默认 `false`，且没有消费/提交
路径。故本报告是组件化设计和证据盘点，不是“系统已实现”的验收结论。

## 2. 证据边界与检索记录

### 2.1 规范和目标文档

- 目标子系统：`docs/Version4权威游戏模拟系统主要子系统.md:42`，要求覆盖物品实例、堆叠、背包、装备、世界掉落、箱子、银行、商店、配方、制作、交易和掉落提交。
- 既有总设计：`docs/Version4权威游戏模拟系统拆分设计报告.md:320-356` 已提出
  `ItemDefinitionRef`、`ItemInstanceState`、`ItemStackState`、`InventoryLayoutState`、
  `EquipmentState`、`ItemUseState`、`WeaponAbilityState`、`CraftingState`、
  `LootRollState`，以及 `ContainerCapacityState`、`ContainerContentsState`、
  `ContainerAccessState`、`WorldItemState`、`TileEntityBindingState`。
- `public-decomposition` 要求先盘点成员、读者/写者、生命周期和副作用，再按访问模式选择
  Component、System、Query、Command、Adapter、Projection。其引用的
  `约束/公共拆分约束.md` 在当前 checkout 不存在，因此本报告同时遵循实际可读取的
  `Context/架构设计/ECS文件组织设计约束.md`、`Context/约束/Google-CSharp-Style-Guide-约束.md` 和
  `Context/约束/非函数式编码副作用隔离规范.md`。

### 2.2 Version4 目标代码（权威语义）

| 领域 | 真实文件与位置 | 证据结论 |
| --- | --- | --- |
| Item 定义/实例 | `D:\TRbackup\Version4\Terraria\Item.cs:122-248`、`:317-335`、`:48120-48170`、`:48862-48886` | `type`、`stack`、`maxStack`、`prefix`、`favorited`、使用/伤害/价值字段与 `active` 派生属性同处一个类；`SetDefaults`、`Clone`、`TurnToAir` 负责初始化和清空。权威字段 confirmed，职责混合 confirmed。 |
| 玩家背包/装备 | `D:\TRbackup\Version4\Terraria\Player.cs:1009-1095`、`:2960-2962`、`:3993-4040`、`:6890-6972` | `inventory[59]`、`armor`、`dye`、`miscEquips`、四个银行 Chest、选中槽和 `HeldItem` 直接归 Player；`ConsumeItem` 逐槽扣减并可能 `SetDefaults(0)`。状态 confirmed，原子事务 missing。 |
| 物品使用 | `D:\TRbackup\Version4\Terraria\Player.cs:23435-23574`、`:25165-25650` | `ItemCheck` 链同时做资格、资源/魔力支付、使用效果和 Projectile/实体请求；应拆为 Query + Command + System。边界 partial。 |
| Chest/银行/商店 | `D:\TRbackup\Version4\Terraria\Chest.cs:17-70`、`:80-140`、`:407-535`、`:1222` | `item[]`、`x/y`、`index`、`bankChest`、名称和静态坐标索引是独立世界生命周期；创建/销毁有实现，`SetupShop(int type) { }` 为空。容器状态 confirmed，商店填充 missing。 |
| WorldItem | `D:\TRbackup\Version4\Terraria\WorldItem.cs:15-170`、`:227-258`、`:357-550` | `inner Item` 是代理；保留玩家、拾取保护、寿命、位置、移动、合并、越界、熔岩销毁、网络发送都在同一类。实例代理和生命周期 confirmed，表现/网络混合 confirmed。 |
| 配方/配方组 | `D:\TRbackup\Version4\Terraria\Recipe.cs:1-98`、`RecipeGroup.cs:1-69` | 产物、材料、必需 Tile、液体/生物群系/事件条件和 RecipeGroup 均有字段；`Main.cs:3401-3414` 的配方组/配方数组初始化被注释，制作索引和远程提交未闭合。定义 partial，注册/资格 missing。 |
| 掉落 | `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropDatabase.cs:9-1293`、`ItemDropResolver.cs:1-67`、`ItemDropRule.cs:1-150` | 数据库按 NPC net id 和全局规则注册；Resolver 执行 `CanDrop`、嵌套规则和链式规则；规则类型覆盖普通、条件、专家/大师、选项、重掷等。规则解析 confirmed，生成实体提交需由外层 System 完成。 |
| 货币/交易 | `D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs:1-46`、`CustomCurrencySingleCoin.cs:1-27`、`CustomCurrencyManager.cs:1-46`、`Terraria.GameContent\ShopHelper.cs:1-67`、`ItemShopSellbackHelper.cs:1-24` | 货币注册和接受类型有骨架，计数/合并/支付/期望价格均为空；ShopHelper 只有价格调整输入，Sellback 只有 memo 状态。交易事务 missing/partial。 |
| 网络/远程制作 | `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:1-33`、`Terraria\MessageBuffer.cs:327-364`、`:1082-1182`、`:1472-1534` | 物品槽和 WorldItem 网络消息会直接重建/写入实例；远程制作请求仅排队字段，反序列化默认失败。网络是 Adapter/Projection，不得成为权威写入者。 |
| 权威槽位 | `D:\TRbackup\Version4\Terraria\Main.cs:934`、`:950`、`:982`、`:3334-3454` | 全局数组保存 401 个 WorldItem、8000 个 Chest、256 个 Player；这是迁移时必须由 `WorldStorage` 封装的旧权威入口。 |

### 2.3 SS14 只读组织参考

SS14 仅用于确认 ECS 粒度和 System/Component 边界，不用于复制代码、命名或领域语义。

| 参考文件 | 可迁移的组织事实 |
| --- | --- |
| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Stacks\StackComponent.cs:12-51` | 堆叠类型、数量、最大数量覆盖、无限标志和表现刷新标志是组件数据；数量变更由 `SharedStackSystem.SetCount/ReduceCount` 之类 System API 完成，而不是任意代码直接改字段。 |
| `...\Content.Shared\Stacks\SharedStackSystem.cs:36-131`、`:193-239` | 合并、拆分、消耗、网络状态和 UI 更新是行为边界；查询/变更入口集中，支持测试 seam。 |
| `...\Content.Shared\Inventory\InventorySystem.Equip.cs`、`InventorySystem.Slots.cs` | 装备槽定义、槽位枚举和装备/卸下操作与普通存储分开；槽位解析失败是可识别结果。 |
| `...\Content.Shared\Containers\ItemSlot\ItemSlotsComponent.cs:8-100` | ItemSlot 组件保存白/黑名单、锁定、插入/弹出策略和起始物品；插入资格由 ItemSlotsSystem 处理。 |
| `...\Content.Shared\Storage\StorageComponent.cs:18-168`、`...\Content.Shared\Storage\EntitySystems\SharedStorageSystem.cs:272-500` | 存储容量/交互配置是组件，插入、移除、嵌套 UI、可达性和转移由 Storage System 执行；容器实体关系不塞回 Item 组件。 |
| `...\Content.Shared\VendingMachines\Components\VendingMachineComponent.cs:8-49`、`...\Content.Server\VendingMachines\VendingMachineSystem.cs:93-239` | 机器库存、违禁库存、损坏/补货标志是组件；定价、扣库存、生成物品、更新 UI 和事件是 System 副作用。 |
| `...\Content.Server\Cargo\Systems\PricingSystem.cs:162-233`、`:320-331` | 估价可组合静态价格、堆叠价格、材料/溶液价格并递归容器；实现必须显式防循环容器和区分估价快照与交易结算。 |
| `...\Content.Server\Cargo\Systems\CargoSystem.Funds.cs:26-78` | 资金扣减、权限检查、生成货币堆和事件审计集中在资金 System；失败不能伪装成成功。 |

### 2.3.1 纳入范围的源码清单

“相关所有代码”按权威状态、规则、事务、生命周期、网络适配和持久化/表现投影的实际
调用边界收集，而不是按文件名包含所有带有 `Item` 的 UI、绘制、纹理和本地化文件。以下
清单是本报告已纳入分析范围的源文件族；`ItemDropRules` 目录中的每个规则实现均被作为
同一规则树边界处理。

| 范围 | Version4 文件 |
| --- | --- |
| 核心物品/实体/槽位 | `Terraria\Item.cs`、`Terraria\WorldItem.cs`、`Terraria\Player.cs`、`Terraria\Chest.cs`、`Terraria\Main.cs`、`Terraria\ShoppingSettings.cs`、`Terraria.ID\PlayerItemSlotID.cs`、`Terraria.ID\ItemID.cs`、`Terraria.ID\ItemSourceID.cs`、`Terraria.ID\CustomCurrencyID.cs`。 |
| 容器、快速堆叠和回购 | `Terraria.GameContent\PositionedChest.cs`、`NearbyChests.cs`、`QuickStacking.cs`、`EmergencyStacking.cs`、`ItemShopSellbackHelper.cs`、`FakeCursorItem.cs`。 |
| 商店与货币 | `Terraria.GameContent\ShopHelper.cs`、`Terraria.GameContent.UI\CustomCurrencyManager.cs`、`CustomCurrencySystem.cs`、`CustomCurrencySingleCoin.cs`。 |
| 配方与制作请求 | `Terraria\Recipe.cs`、`Terraria\RecipeGroup.cs`、`Terraria.GameContent\CraftingRequests.cs`。 |
| 掉落规则 | `Terraria.GameContent.ItemDropRules\ItemDropDatabase.cs`、`ItemDropResolver.cs`、`ItemDropRule.cs`、`DropAttemptInfo.cs`、`ItemDropAttemptResult.cs`、`ItemDropAttemptResultState.cs`、`CommonCode.cs`、`Conditions.cs`、`Chains.cs`，以及 `CommonDrop*`、`DropBasedOn*`、`DropLocalPerClientAndResetsNPCMoneyTo0`、`DropNothing`、`DropOneByOne`、`DropPerPlayerOnThePlayer`、`FromOptionsWithoutRepeatsDropRule`、`ItemDropWithConditionRule`、`LeadingConditionRule`、`MechBossSpawnersDropRule`、`OneFrom*`、`SlimeBodyItemDropRule`、`StatueMimicItemDropRule` 和相关接口。 |
| 生成来源/诊断 | `Terraria.DataStructures\EntitySource_DropAsItem.cs`、`EntitySource_OverfullChest.cs`、`PlayerGetItemLogger.cs`、`MinionSpawnFromInventoryItem.cs`。 |
| 网络适配 | `Terraria\MessageBuffer.cs`、`Terraria\NetMessage.cs`、`Terraria.Initializers\NetworkInitializer.cs`。 |

SS14 的纳入范围是 `Content.Shared\Stacks`、`Content.Shared\Inventory`、
`Content.Shared\Containers\ItemSlot`、`Content.Shared\Storage`、
`Content.Shared\VendingMachines\Components`、`Content.Server\Storage`、
`Content.Server\VendingMachines`、`Content.Server\Cargo\Systems\PricingSystem.cs` 和
`CargoSystem.Funds.cs`。UI 控件、贴图、音频以及与库存无权威交集的 Cargo/Storage 功能未
作为拆分依据；它们只在需确认投影边界时被识别为排除项。

### 2.4 tModLoader 离线 API 佐证

已按检索流程读取 `D:\TRbackup\tmodloader-api-docs-stable\index.html`，页眉显示
`tModLoader v2026.07`。类型索引明确列出：

- `class_item.html`：`Item Class Reference`；
- `class_chest.html`：Chest 描述为“非 Player inventory，例如 chests、portable storage 或 NPC shops”；
- `class_recipe.html`：Recipe 描述为 ingredients、tiles 和 resulting Item 的集合，并说明 `Create`、`AddIngredient`、`AddTile`、`Register` 公开 API；
- `class_player.html`：Player 类型页面存在。

该镜像用于确认公开类型/概念存在，字段读写和目标 Version4 的实际调用顺序仍以
`D:\TRbackup\Version4` 源码为准。成员级页面的动态锚点没有作为唯一证据使用，因此相关
外部佐证标记为 partial，而不是把文档索引误当作运行时实现。

## 3. 成员归属与状态清单

| 成员/概念 | 当前读者 | 当前写者 | 生命周期 | 状态类型 | 候选边界 | 证据状态 |
| --- | --- | --- | --- | --- | --- | --- |
| `Item.type`, `maxStack`, 使用/伤害/价值字段 | Item、Player、WorldItem、Shop/Drop 规则 | `SetDefaults*`、前缀/变体初始化 | 定义加载至实例销毁 | 内容定义引用 | `ItemDefinitionRef` | confirmed |
| `Item.prefix`、名称覆盖、染料/变体标记 | Player、WorldItem、网络槽位 | Prefix/网络重建/使用链 | 实例生命周期 | 权威实例状态 | `ItemInstanceState` | confirmed |
| `Item.stack` | Player、Chest、WorldItem、堆叠逻辑 | `ConsumeItem`、转移、WorldItem 合并、网络写入 | 槽位/掉落生命周期 | 权威数量 | `ItemStackState` | confirmed |
| `Player.inventory/armor/dye/miscEquips` | ItemCheck、装备效果、网络 | Player 方法、MessageBuffer | 玩家实体生命周期 | 关系/槽位 | `InventoryLayoutState`、`EquipmentState` | confirmed |
| `Chest.item[]`, `x/y/index/bankChest` | Chest 静态方法、Recipe、MessageBuffer | Create/Assign/Remove、网络 | 世界/银行/商店生命周期 | 容器权威状态 | `ContainerCapacityState`、`ContainerContentsState`、`ContainerAccessState` | confirmed |
| `WorldItem.playerIndexTheItemIsReservedFor`, `ownTime`, `timeLeft...` | FindOwner、Update、网络 | WorldItem.Update、MessageBuffer | 掉落至拾取/过期 | 拾取权限与寿命 | `WorldItemReservationState`、`WorldItemLifetimeState` | confirmed |
| `Recipe.requiredItem`, groups、Tile/biome/liquid 条件 | FindRecipes/制作入口 | Recipe 初始化/注册（当前初始化被注释） | 内容加载至世界会话 | 定义数据 | `RecipeDefinitionCatalog` | partial |
| `_globalEntries`, NPC rule lists、链式规则 | DropResolver | ItemDropDatabase 注册 | 内容加载至掉落结算 | 规则索引 | `LootRuleCatalog` | confirmed |
| `CustomCurrencySystem` 货币映射和 cap | 商店 UI/交易入口 | Register/Include/SetCurrencyCap；结算方法为空 | 内容加载至进程/世界 | 定义 + 结算依赖 | `CurrencyDefinitionCatalog`、`CurrencyBalanceState` | partial/missing |
| `CraftingRequests._pendingCrafts` | 网络模块（目前无消费） | Deserialize（当前返回 false） | 请求排队至确认/拒绝 | 临时请求 | `CraftRequestQueue`（非权威） | missing |

## 4. 组件与系统边界

### 4.1 `ItemGameplay`

组件应只保存持续状态，不执行跨实体行为。建议组件如下：

| 组件 | 最小字段 | 所有权与不变量 |
| --- | --- | --- |
| `ItemDefinitionRef` | `ContentId`, `DefinitionRevision` | 只引用 `ContentCatalog` 的只读定义；不能从实例反向修改定义。 |
| `ItemInstanceState` | `InstanceId`, `PrefixId`, `NameOverride`, `VariantId`, `Favorited` | 代表可持久化实例差异；实例 ID、持久化 ID、网络 ID 分开。 |
| `ItemStackState` | `Count`, `MaxCount`、可选 `StackKey` | `0` 只能表示空实例或待销毁；合并/拆分必须经 `StackMutationSystem`，不能让 Query 写数量。 |
| `InventoryLayoutState` | 槽位集合、选中槽、垃圾槽/弹药槽标志 | 只保存槽位关系和布局；不存商店价格或装备效果。 |
| `EquipmentState` | 装备槽到 Item 实体引用、隐藏/染料槽引用 | 穿戴关系是权威状态，效果由 `EquipmentEffectSystem` 派生。 |
| `ItemUseState` | `UsePhase`, `ElapsedTicks`, `Cooldown`, `Target` | 时间由 Tick 输入显式提供；不在组件内读取系统时钟。 |
| `WeaponAbilityState` | 武器输出快照、弹药定义引用、Projectile 请求序号 | 只产生 `SpawnProjectileCommand`；Projectile 实体由生命周期/WorldStorage 提交。 |
| `CraftingState` | 当前请求、锁定材料版本、结果预览 | 锁定不是扣除；只有事务提交成功后才减少材料。 |

建议接口：

```text
IItemDefinitionCatalog.Get(ContentId) -> ItemDefinition
IItemStackQuery.CanMerge(source, destination) -> StackMergeDecision
IItemUseEligibilityQuery.Evaluate(actor, item, context) -> EligibilityResult
IItemUseSystem.Apply(UseItemCommand, SimulationContext, CommandBuffer)
```

`IItemDefinitionCatalog` 和资格 Query 是纯/只读 seam；使用 System、堆叠 System 和装备
System 是唯一写入边界。`ItemCheck` 迁移时保持旧 API 作为 Adapter，内部转换成上述命令，
不得继续让 Item 实例访问 `Main.player` 或 `Main.item`。

### 4.2 `WorldObjects`

| 组件 | 最小字段 | 读写与边界 |
| --- | --- | --- |
| `ContainerCapacityState` | `ContainerKind`, `SlotCount`, `MaxWeight`（如有） | 只表示容量/槽位上限。 |
| `ContainerContentsState` | 有序槽位到 Item 实体引用、`Revision` | 唯一拥有容器内容；所有插入/移除递增 revision。 |
| `ContainerAccessState` | 坐标、锁定、当前访问者、权限、银行/商店标志 | 访问资格 Query 读取，Access System 写访问者和锁。 |
| `WorldItemState` | 世界坐标、速度、保留玩家、`KeepTime`、拾取保护、过期计时 | 不保存物品定义副本；通过 Item 实体引用组合。 |
| `TileEntityBindingState` | TileEntity 类型、坐标、持久化 ID | 与 TileMap/WorldStorage 绑定，不能把坐标注册逻辑放进 ContainerContents。 |
| `ShopInventoryState` | 商品定义引用、库存数量、商店 revision | 仅商店实体拥有库存；购买后由 Commerce 事务扣库存。 |

边界接口：

```text
IContainerQuery.CanInsert(actor, container, item, quantity) -> TransferEligibility
IContainerQuery.CanRemove(actor, container, slot, quantity) -> TransferEligibility
IWorldItemQuery.CanPickup(actor, worldItem) -> PickupEligibility
IWorldStorageCommands.Submit(SpawnWorldItemCommand | DespawnEntityCommand | ContainerMutationCommand)
```

`WorldItem.UpdateItem` 应拆成 `WorldItemMovementSystem`、`WorldItemReservationSystem`、
`WorldItemMergeSystem`、`WorldItemPickupSystem` 和 `WorldItemLifetimeSystem`。移动/视觉投影
不能直接决定拾取；拾取先过 Query，再提交转移命令。

### 4.3 `InventoryTransferTransactions`

这是跨组件不变量的唯一写入口，负责背包 ↔ 世界掉落、背包 ↔ Chest、装备 ↔ 背包、容器 ↔
容器、堆叠合并/拆分。建议命令：

```text
TransferItemCommand(source, destination, quantity, actor, expectedRevision)
SplitStackCommand(source, quantity, destinationHint, actor)
MergeStackCommand(source, destination, quantity, actor)
EquipItemCommand(actor, item, slot, expectedRevision)
UnequipItemCommand(actor, slot, destinationHint, expectedRevision)
PickupWorldItemCommand(actor, worldItem, expectedWorldItemRevision)
```

处理顺序固定为：

1. 读取并锁定 source/destination 的权威 revision；
2. 运行纯资格 Query（存在、距离、权限、锁定、容量、StackKey、数量）；
3. 计算 `TransferPlan`，但不写状态；
4. 在一个提交边界内扣 source、加 destination、清空空实例、更新 revision；
5. 发布 `ItemTransferredEvent`，再由网络/存档/表现投影读取事件或快照。

失败返回稳定错误码（`NotFound`、`Locked`、`OutOfRange`、`InsufficientQuantity`、
`CapacityExceeded`、`RevisionConflict`、`StackMismatch`），不能抛出第三方异常文本作为
业务结果。跨请求使用 `TransactionId` 去重；超时后结果未知时先查询 revision，不盲目重试。

### 4.4 `CommerceTransactionSystem`

将价格计算和结算分开：

| 边界 | 责任 |
| --- | --- |
| `PriceDefinition`/`CurrencyDefinition` | 内容目录中的基础价格、货币类型、上限和取整策略。 |
| `PriceQuery` | 读取商店库存、物品实例、NPC/世界规则，返回价格快照；可组合基础价、堆叠数量、关系/环境调整。不得扣钱。 |
| `FundsState` | 账户/钱包的货币余额、revision、货币类型；与 Item 堆叠分开，即便旧版以硬币 Item 表示。 |
| `PurchaseEligibilityQuery` | 商品存在、库存、价格快照未过期、账户余额、访问权限。 |
| `CommerceTransactionSystem` | 原子扣货币、扣商店库存、生成购买物品/转入容器、写入交易账本。 |
| `SellTransactionSystem` | 校验卖方持有权、转移物品、增加余额、更新回购 memo；失败时不得只完成半边。 |
| `CommerceLedgerProjection` | 输出审计/网络/存档记录，不能反写余额或库存。 |

建议命令：

```text
PurchaseCommand(actor, shop, offerId, quantity, priceRevision, transactionId)
SellCommand(actor, shop, sourceSlot, quantity, expectedItemRevision, transactionId)
DepositFundsCommand(account, currency, amount, source, transactionId)
WithdrawFundsCommand(account, currency, amount, destination, transactionId)
```

一次购买提交的顺序为“验证价格快照 → 锁定账户和商店 revision → 扣余额 → 扣库存 → 生成/转移
商品 → 写账本 → 发布事件”。任一步失败都返回拒绝并保持前置状态；如果远端确认丢失，使用
`TransactionId` 和账本查询确认，而不是再次扣款。

旧版 `CustomCurrencySystem.TryPurchasing` 的多数组参数应由 `CurrencyBalanceAdapter` 转换，
不能让新 System 依赖 UI 槽位数组；`ShopHelper` 的幸福度/环境价格调整应成为纯
`PriceModifierQuery`，UI 只消费 `PriceSnapshot`。

### 4.5 `LootResolutionSystem`

把已有规则树保留为定义/计算层：

| 模块 | 责任 |
| --- | --- |
| `LootRuleCatalog` | 按 NPC 内容 ID、全局规则和版本注册规则；初始化后只读。 |
| `LootEligibilityQuery` | 读取难度、事件、玩家、运气等显式输入；不生成实体。 |
| `LootRollQuery` | 在注入的随机源上执行 Common/Condition/Options/Reroll/Chain 规则，返回不可变 `LootRollResult`。 |
| `LootResolutionSystem` | 将结果转换为 `SpawnWorldItemCommand`、玩家直给或 Boss Bag 命令；负责数量、归属、来源和失败策略。 |
| `LootPersistenceProjection` | 保存规则版本、随机种子/审计摘要和已提交掉落，不成为下一次规则输入。 |

必须禁止 Query 直接调用 `Item.NewItem`、`NetMessage` 或写 Player/Chest。随机源、世界时间、
NPC 快照和参与玩家列表作为 `DropAttemptContext` 显式输入；重放时可使用同一 context 验证
结果。掉落提交与 NPC 销毁的先后关系应由 `DeathAndLootSystem` 显式规定：先完成 roll，再在同
一 CommandBuffer 中提交掉落与生命周期事件。

## 5. 模块拓扑与执行顺序

```text
ContentCatalog
  ├─ ItemDefinitionCatalog / RecipeCatalog / LootRuleCatalog / CurrencyDefinitionCatalog
  ↓
Intent + Eligibility Queries
  ├─ ItemUseEligibilityQuery
  ├─ ContainerAccessQuery / TransferEligibilityQuery
  ├─ PurchaseEligibilityQuery / PriceQuery
  └─ LootEligibilityQuery / LootRollQuery
  ↓
Commands
  ├─ Transfer / Equip / Pickup
  ├─ Purchase / Sell / Funds
  └─ SpawnWorldItem / Craft / Loot
  ↓
Authority Systems
  ├─ InventoryTransferTransactions
  ├─ CommerceTransactionSystem
  ├─ ItemUse / CraftingSystem
  └─ LootResolutionSystem + WorldItemLifetimeSystem
  ↓
WorldStorage commit (entity, container, tile-entity and world-item state)
  ↓
Network / Persistence / UI / Audio / Animation projections
```

在 `RuntimeComposition` 中建议使用以下显式顺序：

`Input/NetworkCommand → Intent/Eligibility Query → Price/Loot/Recipe pure calculation →
Transfer/Commerce/Crafting commit → Spawn/Despawn/Lifetime → Replication/Persistence →
Presentation`。

网络收包可以产生命令，但不能直接写 `Main.player`、`Main.item` 或 `Main.chest`。文件名、项目
文件顺序和目录枚举都不得承担顺序语义。

## 6. 对当前 NLTX `src/Items` 的落地建议

当前目录已有以下文件：`ItemDefinitionComponent.cs`、`StackableItemComponent.cs`、
`InventoryComponent.cs`、`ContainerComponent.cs`、`EquipmentComponent.cs`、
`EquipmentSlot.cs`、`HandsComponent.cs`、`WeaponComponent.cs`。它们是有价值的初始骨架，
但需要做以下调整：

| 当前文件 | 处理建议 |
| --- | --- |
| `ItemDefinitionComponent` | 保留为 `ItemDefinitionRef` 的最小实现；后续增加 definition revision/catalog seam，不把默认伤害/价格复制到实例。 |
| `StackableItemComponent` | 保留数量与最大数量，新增 StackKey/变更 API；把合并、拆分和消耗移到 `StackMutationSystem`。 |
| `InventoryComponent` | 保留槽位关系和选中槽；不要把装备、银行、商店或转移命令塞入组件。考虑把可变 `List` 的跨边界读取改为只读快照。 |
| `ContainerComponent` | 与 `InventoryComponent` 分开是正确方向；补充 `ContainerRevision` 和容量规则引用，禁止组件方法执行转移。 |
| `EquipmentComponent`/`EquipmentSlot` | 保留装备关系；增加隐藏/染料槽是否权威的明确字段，效果由单独 System 派生。 |
| `HandsComponent` | 保留为手持关系；拾取/交换走 Transfer Command。 |
| `WeaponComponent` | 保留静态武器能力数据；冷却、当前使用和 Projectile 请求移到 `ItemUseState`/`WeaponAbilityState`。 |
| 缺失模块 | 按领域优先新增 `src/Items/Transfer/`、`src/Items/Commerce/`、`src/Items/Loot/` 仅在达到独立测试/依赖边界时建立；小型定义和组件继续平铺 `src/Items/`。 |

建议目标布局（只创建有实际文件的目录）：

```text
src/Items/
  ItemDefinitionComponent.cs
  ItemInstanceComponent.cs
  StackableItemComponent.cs
  ItemUseComponent.cs
  InventoryComponent.cs
  EquipmentComponent.cs
  HandsComponent.cs
  WeaponComponent.cs
  ContainerComponent.cs
  Transfer/
    ItemTransferCommand.cs
    ItemTransferEligibilityQuery.cs
    InventoryTransferSystem.cs
  Commerce/
    CurrencyBalanceComponent.cs
    PriceSnapshot.cs
    CommerceTransactionSystem.cs
  Loot/
    LootRuleDefinition.cs
    LootRollQuery.cs
    LootResolutionSystem.cs
```

若 `Transfer`、`Commerce` 或 `Loot` 尚未有独立测试和稳定访问边界，应先放在 `Items` 根目录，
避免预建空的 `Components/Systems/Queries` 技术目录。每个新增核心公开类型使用同名
PascalCase 文件；命名空间不因目录移动自动改变。

## 7. 迁移批次、兼容和不拆分项

### 7.1 推荐批次

1. **批次 A：定义/实例**。建立 `ItemDefinitionRef`、`ItemInstanceState`、`ItemStackState`，
   加载旧 `Item` 的只读默认数据；验证 Clone/空物品/前缀保留。
2. **批次 B：容器与槽位**。封装 Player/Chest/WorldItem 的数组访问，建立 revision 和
   `WorldStorage` 命令入口；先保留单向 Legacy Adapter，禁止新旧字段双写。
3. **批次 C：转移事务**。迁移 `ConsumeItem`、WorldItem 拾取、Chest 放取、堆叠合并/拆分；
   先写 focused verifier 再替换调用点。
4. **批次 D：物品使用/制作**。拆 `ItemCheck`，恢复 RecipeCatalog 和远程请求的验证/提交；
   结果生成仍走 CommandBuffer。
5. **批次 E：经济**。先实现货币余额与价格快照，再接 Purchase/Sell 和商店库存；旧 UI 货币
   数组仅作为 Adapter 输入。
6. **批次 F：掉落**。保留规则树，新增纯 roll 结果和统一掉落提交；最后迁移网络/存档/表现投影。

### 7.2 兼容策略

- 旧 `Item`/`Chest`/`WorldItem` API 只能作为 Legacy Adapter；Adapter 的写入方向是
  `legacy input → typed command`，不能把新组件和旧字段同时当权威。
- 网络槽位、持久化 ID、实体引用和外部账户 ID 分开建模；快照/网络 DTO 不反向驱动权威状态。
- 空体和默认返回必须保留在证据表中，不能在迁移报告中标成“兼容实现”。

### 7.3 明确不拆分项

- 不把 `ItemDefinitionRef` 再按每个字段拆成十余个组件；默认定义共同由 Catalog 读取，
  生命周期一致。
- 不把每个货币面额做成实体组件；余额结算应是账户状态，只有世界中可拾取的硬币才是
  `ItemStackState` 实例。
- 不把 UI 购物车、Tooltip、动画、音频和网络包当成权威组件；它们属于 Projection/Adapter。
- 不把 Recipe 的每一个环境条件拆成实体组件；条件是 Definition + Query 输入。
- 不把 ContainerContents 和 ItemStack 合并；容器拥有关系，Item 拥有数量，二者的更新事务
  需要同一提交边界但不是同一状态所有者。

## 8. 副作用、失败与一致性登记

| 边界 | 读取 | 写入/效果 | 失败与重复语义 |
| --- | --- | --- | --- |
| Transfer System | 实体/容器 revision、位置、权限、StackKey | 源/目标槽位、实体生命周期、事件 | `RevisionConflict` 可安全重算；事务 ID 去重；部分成功禁止对外报告成功。 |
| Commerce System | 价格定义、账户余额、商店库存、世界规则 | 余额、库存、物品生成、账本/消息 | 扣款与发货必须同一权威提交；超时结果未知时查询账本，不能盲重试。 |
| Loot Resolution | NPC 快照、难度/事件、显式 RNG | 掉落 Spawn Command、掉落审计 | 规则 Query 可重放；提交至少一次时以事件/事务 ID 去重。 |
| WorldItem Lifetime | 时间 tick、位置、液体/Tile 查询 | 移动、合并、销毁、网络事件 | 时间是显式 tick；Despawn 与拾取竞争按 revision/提交顺序决定。 |
| Projections | 权威快照/事件 | 网络、存档、UI、日志 | 投影失败不能修改玩法状态；日志回调不得推进业务。 |

## 9. Focused verifier 计划与当前结果

建议每个批次先建立不依赖完整客户端/图形环境的验证器：

1. `ItemStackVerifier`：相同/不同 StackKey 合并、上限、拆分、空堆和前缀保留。
2. `TransferTransactionVerifier`：背包↔Chest、Chest↔Chest、WorldItem 拾取、装备/卸下、
   容量不足、锁定、距离和 revision 冲突；检查失败时源/目标完全不变。
3. `CommerceVerifier`：价格快照过期、余额不足、库存不足、购买/出售原子性、重复
   `TransactionId`、货币上限和结果未知恢复。
4. `RecipeVerifier`：RecipeGroup、Tile/液体/生物群系条件、材料锁定与提交后扣除；网络
   请求拒绝不能消耗材料。
5. `LootVerifier`：固定 RNG 重放、条件分支、嵌套链、数量范围、玩家归属和 Spawn Command。
6. `ProjectionVerifier`：网络/存档写入失败或 UI 不可见时，权威容器/余额/寿命不改变。

本次只新增文档，没有修改 C# 或项目文件，因此未运行 `dotnet restore/build/test`，也没有
新的 `Build/bin` 构建产物。以上 verifier 均为待实施计划，当前状态为 **未验证**；不能据此
宣称物品、容器或经济事务已经实现。

## 10. 风险清单

| 风险 | 影响 | 缓解 |
| --- | --- | --- |
| Player/Chest/WorldItem 旧数组与新组件双写 | 状态分叉、网络/存档不一致 | 先建立单向 Adapter，切换 authority 后删除旧写者；每批次记录唯一写入口。 |
| `ItemCheck` 拆分后顺序改变 | 消耗成功但效果失败，或重复生成 Projectile | 用 `UsePlan` 先计算，再按“资格→资源支付→效果→生成请求”提交；固定顺序 verifier。 |
| 商店价格读取容器递归形成循环 | 死循环或价格膨胀 | `PriceQuery` 维护访问栈/实体 revision，并提供不估价内容的显式标志。 |
| 网络确认丢失后重复购买/制作 | 重复扣款或重复产物 | `TransactionId`、账本查询和幂等提交；超时标记结果未知。 |
| WorldItem 拾取、合并、销毁竞争 | 丢物、复制物或错误归属 | 所有操作带 WorldItem/Container revision，统一由 WorldStorage CommandBuffer 提交。 |
| 仅参考 SS14 造成语义误植 | Terraria 规则改变 | SS14 证据只支持组织模式；字段语义和公开 API 以 Version4/tModLoader 证据为准。 |

## 11. 复核清单

- [x] 已阅读目标规格中的第 12 项及其总设计候选边界。
- [x] 已盘点 Version4 的 Item、Player、Chest、WorldItem、Recipe、Drop、Currency、Crafting 和网络入口。
- [x] 已只读检查 SS14 的 Stack、Inventory、ItemSlot、Storage、Vending、Pricing、Funds 边界。
- [x] 已区分权威状态、派生价格/查询、命令、适配器和投影。
- [x] 已标记空体、注释掉的初始化和缺失提交路径。
- [x] 已给出领域优先的组件文件布局、迁移顺序和 focused verifier 计划。
- [ ] 字段级读写者闭合、C# 实现迁移、编译和运行时回归：未完成，需后续批次执行。
