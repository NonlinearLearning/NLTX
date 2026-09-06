# ItemContainerAndEconomy Component Design

## 1. 设计元数据

| 项目 | 内容 |
|---|---|
| `subsystemId` | `ItemContainerAndEconomy` |
| `taskNumber` | `17` |
| `sourceReport` | `D:\TRbackup\NLTX\docs\research\2026-09-05-version4-item-container-and-economy-public-decomposition.md` |
| `outputDesignPath` | `D:\TRbackup\NLTX\docs\design\2026-09-05-version4-item-container-and-economy-component-design.md` |
| `designScope` | `component-only` |
| `designStatus` | `decision-required` |
| `evidenceStatus` | `partial` |
| `nltxStatus` | `partial` |
| `verificationStatus` | `not-run` |
| `selectionMethod` | 按语义内聚、共同读写者、生命周期、权威状态 owner、实体范围和跨域关系整理；不按字段数量机械拆分，也不把静态定义、单个实例或外部快照提升为 Component。 |

输入研究报告中的 `reportPath` 与实际存在的 `sourceReport` 一致，未发生路径
`evidence-mismatch`。本文件只新增这一份设计文档；所有目标路径、类型和字段形状均为
`status: proposed` 的设计记号，不能据此推断文件或运行时能力已经存在。

## 2. 设计范围与排除范围

本设计只确定 ItemContainerAndEconomy 的状态组件：物品实例、堆叠数量、容器布局与内容、
访问资格、装备关系、世界物品绑定与 reservation、制作状态、商店库存、逻辑货币余额和
结算账本。每个组件只回答“保存什么状态、属于什么实体范围、何时创建/清理、与哪些组件
共同存在”。

纳入的跨域事实只有确认组件归属所必需的最小信息：

- ContentCatalog 提供的外部内容引用；
- PlayerGameplay 与装备槽、选中槽和玩家容器的关系；
- WorldStorage 提供的 Chest/容器身份和访问上下文；
- SpawnLifecycleAndLoot、FishingAndCatchSimulation 提供的结果来源信息；
- NetworkSessionAndSectionStreaming、PersistenceAndRecovery 使用的 ID、revision 和只读快照字段。

以下内容不在本文件中设计：掉落规则、钓鱼资格、玩家能力和资源效果、世界物品运动、
Chest 的空间实体生命周期、内容注册、外部协议格式、存档文件格式、客户端显示，以及任何
行为执行、调度、验证计划或迁移步骤。它们只在组件字段的证据或 owner 冲突中被最小化引用。

## 3. 组件设计依据

### 3.1 证据优先级与直接事实

| 来源 | 实际证据 | 用途 | `evidenceStatus` |
|---|---|---|---|
| Version4 | `D:\TRbackup\Version4\Terraria\Item.cs:122-140,248,270-286,317-319` | 确认 `type`、`stack`、`maxStack`、价格相关字段、`prefix` 和实例空状态入口 | `confirmed` |
| Version4 | `D:\TRbackup\Version4\Terraria\Item.cs:354-363,375-498,48120-48175,48862-48899` | 确认空物品、prefix 修改、内容重建、堆叠判断和清空语义 | `confirmed` |
| Version4 | `D:\TRbackup\Version4\Terraria\Item.cs:48686-48797` | 确认世界物品结果包含类型、数量、prefix、位置和副作用；只用于字段归属 | `confirmed` |
| Version4 | `D:\TRbackup\Version4\Terraria\Player.cs:1013-1095,2960-2962` | 确认玩家装备、背包、银行和选中槽是多个容器/关系范围 | `confirmed` |
| Version4 | `D:\TRbackup\Version4\Terraria\Player.cs:3740-3771,3986-4047,23435-23590,25121-25164` | 确认消费、使用和空实现缺口；只用于数量 owner 与实例生命周期判断 | `partial` |
| Version4 | `D:\TRbackup\Version4\Terraria\Chest.cs:17-66,68-150,459-557,1222` | 确认 Chest 的容量、槽内容、坐标、银行属性、创建/清理和商店空体 | `confirmed` |
| Version4 | `D:\TRbackup\Version4\Terraria\WorldItem.cs:15-169,227-348,357-558` | 确认世界物品 payload、reservation、合并、寿命和表现状态混合 | `confirmed` |
| Version4 | `D:\TRbackup\Version4\Terraria\Recipe.cs:12-97`、`D:\TRbackup\Version4\Terraria\RecipeGroup.cs:9-68` | 确认 Recipe/RecipeGroup 是内容定义和互换规则，不是持续实体状态 | `confirmed` |
| Version4 / 完整参考补证 | `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:9-32`；`D:\TRbackup\无任何删减通过编译\Terraria.GameContent\CraftingRequests.cs:26-405` | 确认制作请求字段和实现缺口；完整参考仅补证同路径行为 | `partial` |
| Version4 / 完整参考补证 | `D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs:8-51`；`D:\TRbackup\无任何删减通过编译\Terraria.GameContent.UI\CustomCurrencySystem.cs:8-296` | 确认货币数据存在但 Version4 付款闭合不足 | `partial` |
| Version4 | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1646-1718,2788-2840` | 确认旧 Chest 快照只保存坐标、容量、槽位、type、stack、prefix | `confirmed` |
| Version4 | `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:331-350,1090-1153,1493-1504`；`D:\TRbackup\Version4\Terraria\NetMessage.cs:201-212,643-676,866-888` | 确认外部消息路径可直接触及 Item、Chest、WorldItem；只用于快照字段隔离 | `confirmed` |

### 3.2 公开 API 与 ECS 结构参考

`D:\TRbackup\tmodloader-api-docs-stable` 的本地版本为 `tModLoader v2026.07`。本轮采用实际
类型页和成员锚点作公开边界交叉验证：

| 页面与成员锚点 | 参考结论 | 限制 |
|---|---|---|
| `D:\TRbackup\tmodloader-api-docs-stable\class_item.html#a87032523d0721bc622ae3d37452f78b8`；`D:\TRbackup\tmodloader-api-docs-stable\class_item.html#a6e35aa31ca611fa855542433cc7adbc8`；`D:\TRbackup\tmodloader-api-docs-stable\class_item.html#ad19d05c738f67bd61c40e5b265d7f9a0` | `stack`、`maxStack` 和 `prefix` 是公开实例字段边界 | 不替代 Version4 私有写入路径 |
| `D:\TRbackup\tmodloader-api-docs-stable\class_item.html#ac435cae572676b5ec8a8e26dac27ae11`；`D:\TRbackup\tmodloader-api-docs-stable\class_item.html#a2a7567697940f3e104468c87b0559199` | 内容重建与世界物品生成入口可与实例状态分离 | 不确认事务顺序 |
| `D:\TRbackup\tmodloader-api-docs-stable\class_recipe.html:99-154`；成员 `D:\TRbackup\tmodloader-api-docs-stable\class_recipe.html#a32e3bff72441114d4cda94dd4cb05ea7`、`D:\TRbackup\tmodloader-api-docs-stable\class_recipe.html#a7e44a7659baf373b83f44c0fab3f5ac0`、`D:\TRbackup\tmodloader-api-docs-stable\class_recipe.html#adbbadf559d181a9ed34ce2082c6f8f9f` | 配方材料、站点和注册属于内容定义边界 | 不替代 Version4 制作状态 |
| `D:\TRbackup\tmodloader-api-docs-stable\class_mod_item.html#a10b52a61d301eca52d3b30c3c989d835`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_mod_item.html#abe74a706630de7e7c6f60dd5f6855f99`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_mod_item.html#a453b418e0a6eada1997713d3c2123ac6`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_mod_item.html#acfc46a2a397ebe4dc8b01308ec1d3126`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_mod_item.html#a6c3c5fc29f235f666e3a8fa350fc7272`；`D:\\TRbackup\\tmodloader-api-docs-stable\\class_mod_item.html#ac61c1bdd51166b263ebe536f5a9250a6`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_mod_item.html#ab4ee7330e00ebe6ca644570989a3e4b1`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_mod_item.html#a3d30fa657a2319543bdd619d2ad32f81`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_mod_item.html#a80e9898e70b923e0a2a9b0d643504e8e` | 扩展字段和堆叠资格需要通过边界转换进入实例状态；存档/网络扩展数据只能作为实例快照输入 | 外部扩展类型不拥有核心状态 |
| `D:\TRbackup\tmodloader-api-docs-stable\class_chest.html#af43b708d6ba8a4550490ec034d49a7e2`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_chest.html#a44ce32eb9d46ee93f81fd89a04a1faed`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_chest.html#a1963f9c91e0b0381f0b362d540f93adf`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_chest.html#afb05eb9f1c36dc31ab70d7731222ab0f`；`D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html#a12097aab73db65bd17b2bde505e1022d`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_chest.html#a926129e278ac9c460685bd2a326cd998`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_chest.html#a144ef598fa0b3bcc2a0ac6bd1c5467aa`、`D:\\TRbackup\\tmodloader-api-docs-stable\\class_chest.html#af9ebfea8b152b555b030265946cace70` | Chest 和公开存取/世界存取生命周期可作为边界参考 | 不替代私有 Chest 实现 |

Space Station 14 只作最小 ECS 粒度参考，实际读取包括：

- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Stacks\StackComponent.cs:7-38` 和 `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Stacks\SharedStackSystem.API.cs:104-145,234-265,298-331`：数量集中、边界归一化和写入所有权分开；
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Inventory\InventoryComponent.cs:8-54`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Inventory\InventorySystem.Slots.cs:129-203`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Inventory\InventorySystem.Equip.cs:127-157`：布局/槽位关系与装备资格分开；
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Containers\ItemSlot\ItemSlotsComponent.cs:10-40,53-114`：槽位元数据与槽内容分开；
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Storage\StorageComponent.cs:14-58,101-123`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Storage\EntitySystems\SharedStorageSystem.cs:1035-1185`：容量约束与内容状态分开；
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\VendingMachines\Components\VendingMachineComponent.cs:7-46`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\VendingMachines\VendingMachineInventoryEntry.cs:6-40`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Store\Components\StoreComponent.cs:9-102`：库存、价格、余额和购买记录保持不同状态范围；
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\VendingMachines\VendingMachineSystem.cs:201-240`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Store\SharedStoreSystem.cs:145-220`：只用于观察服务端权威状态与外部快照分离。

Space Station 14 无 Terraria ItemCheck、NPC 掉落规则或旧式 Chest 存档格式的直接对应证据；
这些边界只由 Version4 事实和 NLTX 项目约束决定。

### 3.3 ID 与关系字段记号

下列值对象全部为 `status: proposed`，不是新的基础 Component。凡被两个或以上子系统使用，
均标记 `crossSubsystemOwner: integration-review`，最终 owner 不在本文件裁决。

| 值对象 | 表达的身份 | 默认/缺省 | owner 状态 |
|---|---|---|---|
| `RuntimeEntityId` | 当前进程 ECS 实体 | 仅在实体已创建后存在 | `crossSubsystemOwner: integration-review` |
| `PersistentItemId` | 物品实例的持久身份 | 旧快照缺失；不得伪造历史值 | `crossSubsystemOwner: integration-review` |
| `PersistentContainerId` | Chest、银行或其他持久容器身份 | 创建时分配；旧格式缺失 | `crossSubsystemOwner: integration-review` |
| `NetworkId` | 协议同步身份 | 由网络边界分配 | `crossSubsystemOwner: integration-review` |
| `ExternalContentId` | Item、Recipe、Currency、Offer 的内容身份 | 必须由内容目录提供 | `crossSubsystemOwner: integration-review` |
| `ReplicationId` | 世界物品或快照同步身份 | 由同步边界提供 | `crossSubsystemOwner: integration-review` |
| `TransactionId` / `OperationId` | 制作、购买、出售、拾取和结果提交的幂等身份 | 未开始事务时不存在 | `crossSubsystemOwner: integration-review` |
| `ReservationId` | reservation 的稳定身份 | 未建立 reservation 时不存在 | `crossSubsystemOwner: integration-review` |

关系字段只表达一条明确关系：`ContainerContentsComponent` 保存容器槽到
`RuntimeEntityId` 的关系，`EquipmentRelationComponent` 保存玩家到装备槽的关系，
`WorldItemStateComponent` 保存世界掉落到物品实例的关系。持久化 ID、网络 ID、内容 ID 和
运行时实体 ID 不得通过同一个整数或 Guid 互相替代。

## 4. Version4 成员到 Component 归属表

下表只列出可直接支持组件字段归属的真实成员。`proposed Component` 列是设计目标，
不是现有类型声明；静态定义或外部快照不在此表中升格为 Component。

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|---|
| `type` | `Item` | 外部内容身份 | 权威引用 | 创建、重建、加载 | `ItemInstanceComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:122` |
| `stack` | `Item` / `WorldItem` | 堆叠数量 | 权威 | 创建、消费、合并、归零 | `ItemStackComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:138`; `D:\TRbackup\Version4\Terraria\WorldItem.cs:61-70` |
| `maxStack` | `Item` / `WorldItem` | 单堆上限 | 派生 | 内容重建、堆叠判断 | `ItemStackComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:140`; `D:\TRbackup\Version4\Terraria\WorldItem.cs:129` |
| `prefix` | `Item` | 实例修饰 | 权威 | 创建、重铸、加载 | `ItemInstanceComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:286,375-498` |
| `favorited` | `Item` / `WorldItem` | 实例收藏状态 | 权威 | 创建、背包更新、网络/存档 | `ItemInstanceComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:124`; `D:\TRbackup\Version4\Terraria\WorldItem.cs:97-107` |
| `Variant` | `Item` | 内容变体选择 | 权威引用/实例选择 | 内容重建、加载 | `ItemInstanceComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:48134-48142` |
| `dye`、`hairDye` | `Item` | 实例外观修饰 | 权威 | 创建、重建、清空 | `ItemInstanceComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:118-120,48889-48899` |
| `inventory`、`bank`-`bank4` | `Player` / `Chest` | 玩家和银行容器槽 | 权威关系 | 玩家加载到卸载 | `ContainerContentsComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Player.cs:1083-1095` |
| `armor`、`dye`、`miscEquips` | `Player` | 装备槽到物品的关系 | 权威关系 | 玩家加载到卸载 | `EquipmentRelationComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Player.cs:1013-1019` |
| `selectedItem` / `HeldItem` | `Player` | 选中槽关系 | 派生/兼容 | 玩家更新、投影 | `ContainerLayoutComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Player.cs:2960-2962` |
| `maxItems`、`item`、`x`、`y`、`bankChest`、`name` | `Chest` | 容量、槽、空间元数据 | 容器权威 + 外部元数据 | 创建、调整、删除、保存 | `ContainerCapacityComponent`、`ContainerContentsComponent`、`ContainerAccessComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\Chest.cs:38-52` |
| `playerIndexTheItemIsReservedFor`、`noGrabDelay`、`ownIgnore` | `WorldItem` | 世界掉落 reservation 与拾取限制 | 权威 reservation | 生成、竞争、释放、过期 | `WorldItemReservationComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:21-35,257-348` |
| `inner`、`active`、`timeSinceItemSpawned`、`beingGrabbed`、`onConveyor` | `WorldItem` | 物品 payload 与世界状态 | 权威/跨域 | 生成、更新、失活 | `WorldItemStateComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:17-47,357-558` |
| `recipe`、`requested`、`consumed`、`result` | `CraftingRequests.RemoteCraftRequest` | 制作请求和暂存结果 | 快照/临时 | 请求、挂起、结算或丢弃 | `CraftingStateComponent`、`CraftingReservationComponent` | `partial` | `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:11-22,30-32` |
| `shopSpecialCurrency`、`shopCustomPrice` | `Item` | 商店价格输入 | 派生/快照 | 内容重建、商店生成 | `ShopInventoryComponent` | `partial` | `D:\TRbackup\Version4\Terraria\Item.cs:268-272` |
| `CustomCurrencySystem` 的余额/付款成员 | `CustomCurrencySystem` | 货币计数、支付和回滚边界 | 权威结算候选 | 内容加载到交易 | `CurrencyBalanceComponent`、`CommerceLedgerComponent` | `partial` | `D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs:8-51` |
| `WorldFile` Chest Item 保存字段 | `WorldFile` | 旧存档槽快照 | 快照 | 世界保存/加载 | `ContainerContentsComponent`、`ItemInstanceComponent`、`ItemStackComponent` | `confirmed` | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1646-1718,2788-2840` |

## 5. Component 定义

以下每个目标组件均有独立的 `componentId`、实体范围和生命周期。`proposedPath` 明确标记
为 `status: proposed`；它不表示该文件已创建。字段表中的默认值若写为“创建时必需”或
“缺失”，是为了避免用未经证据支持的默认值伪造行为。

### 5.1 ItemInstanceComponent

#### 职责

保存单个物品实例相对于内容定义的持久身份和实例修饰，不保存堆叠数量、容器槽、价格结算
或世界运动状态。

```text
componentId: IC-01
name: ItemInstanceComponent
status: proposed
componentOwner: ItemContainerAndEconomy (candidate)
crossSubsystemOwner: integration-review
entityScope: 一个可被容器、装备或世界掉落引用的物品实体
lifecycle: 非空物品实体创建时建立；内容重建只更新引用相关字段；数量归零时随实例清空；旧快照加载时保留未知身份诊断
proposedPath: src/Items/Instances/ItemInstanceComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `definitionRef` | `ItemDefinitionRef` | 创建时必需；旧快照缺失时为未知引用 | 权威引用 | 只指向 `ExternalContentId` 和 definition revision，不携带数量 | `partial` | `D:\TRbackup\Version4\Terraria\Item.cs:122,48120-48142` |
| `persistentInstanceId` | `PersistentItemId` | 旧 Version4 快照没有该值；分配策略未定 | 权威身份 | 同一持久实例不能复用另一个物品的身份 | `missing` | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1646-1718,2788-2840` |
| `prefixId` | `int` | `0` | 权威 | prefix 更新不得改变 `definitionRef` | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:286,375-498` |
| `variantId` | `ExternalContentId` | 无变体或未知；不得由旧快照猜测 | 权威引用 | 变体必须属于 definition 的合法变体集合 | `partial` | `D:\TRbackup\Version4\Terraria\Item.cs:48134-48142` |
| `dyeId` | `int` | `0` | 权威 | 清空实例时必须同时清空实例外观状态 | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:48889-48899` |
| `favorited` | `bool` | `false` | 权威 | 只属于实例，不改变堆叠资格 | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:124` |
| `nameOverride` | `string?` | `null` | 权威 | 非空值只覆盖显示名称，不成为内容身份 | `partial` | `D:\TRbackup\Version4\Terraria\Item.cs:32,319`; 当前 `D:\TRbackup\NLTX\src\Items\ItemInstanceComponent.cs:5-31` |

#### 字段不变量

- `definitionRef`、`persistentInstanceId` 和 `prefixId` 的语义彼此独立；不能用 `type`、
  `prefix` 或 `ReplicationId` 兼任持久身份。
- 实例为空时不得保留 prefix、dye、名称覆盖或其他实例修饰；Version4 的 `IsAir` 和
  `TurnToAir` 语义必须分别保留。
- prefix 可能改变统计和价值，组件只保存实例修饰值，不把重算后的内容属性复制成另一套
  权威字段。

#### 生命周期

创建时必须获得内容引用；prefix、variant、dye、收藏和名称覆盖可在实例存在期间变化；
数量归零时进入空实例状态或由实体生命周期裁决清理。旧存档缺少 `PersistentItemId` 时
只能记录未决迁移状态，不能假设一个历史 ID。

#### Entity/World 范围

单个 item entity。一个 item entity 可以被 `ContainerContentsComponent`、
`EquipmentRelationComponent` 或 `WorldItemStateComponent` 引用，但同一时刻的归属关系必须
保持单一且可检查。

#### ID 与关系字段

`definitionRef` 使用 `ExternalContentId`；`persistentInstanceId` 使用
`PersistentItemId`；被其他实体引用时使用 `RuntimeEntityId`。三者的最终共享 owner 为
`integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\ItemInstanceComponent.cs:5-31` 已有 prefix、variant、dye、收藏和 Guid
字段，状态为 `partial`；缺少统一 definition 引用、实例写入闭环和旧快照身份策略。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria\Item.cs:122,124,286,354-363,375-498,48120-48175,48880-48899`。

### 5.2 ItemStackComponent

#### 职责

保存一个 item entity 的唯一权威堆叠数量；上限和堆叠资格来自内容引用或明确的兼容规则，
不在容器或实例组件中复制写入。

```text
componentId: IC-02
name: ItemStackComponent
status: proposed
componentOwner: ItemContainerAndEconomy (candidate)
crossSubsystemOwner: integration-review
entityScope: 单个 item entity，包括容器物品和世界掉落 payload
lifecycle: 非空实例创建时建立；消费/合并/拆分时更新；数量为零时与 ItemInstanceComponent 一起归空
proposedPath: src/Items/Instances/ItemStackComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `quantity` | `int` | 空实例为 `0`；非空创建必须显式提供 | 权威 | `0 <= quantity <= maximumQuantity`；数量变化与所属关系变化不能产生半写入 | `confirmed` | `D:\TRbackup\Version4\Terraria\Item.cs:138,48869-48878`; `D:\TRbackup\Version4\Terraria\Player.cs:3986-4047` |
| `maximumQuantity` | `int` | 由 definition 提供；无 definition 时未决 | 派生 | 不得由多个组件各自写入；必须为正数或由明确的无限语义表示 | `partial` | `D:\TRbackup\Version4\Terraria\Item.cs:140`; tModLoader `class_item.html#a6e35aa31ca611fa855542433cc7adbc8` |
| `stackKey` | `ulong` | 未建立堆叠键时为 `0` | 派生/缓存 | 只能辅助资格计算，不能取代 definition、prefix 或实例身份 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\StackableItemComponent.cs:3-24`；`D:\TRbackup\Version4\Terraria\Item.cs:48869-48878` 仅见 type/prefix |
| `isUnlimited` | `bool` | `false` 仅适用于明确声明的兼容值；普通物品不得默认无限 | 兼容 | 无限语义不能绕过非负、提交和容器容量约束 | `missing` | 当前 `D:\TRbackup\NLTX\src\Items\StackableItemComponent.cs:3-24`；Version4 无对应字段 |

#### 字段不变量

- `quantity` 是唯一数量 owner；`ItemState.Stack`、dome `ItemStack.Quantity` 和旧
  `StackableItemComponent.Quantity` 只能作为兼容读法，不能形成第二个写者。
- `maximumQuantity` 是内容派生值；如果 definition、variant、unique stack 或扩展资格会
  改变上限，必须明确其 revision，不能依靠 `stackKey` 的隐式同步。
- 清空数量不等于释放 reservation、删除实体或退款；这些状态不能由数量归零隐式推断。

#### 生命周期

创建、消费、合并、拆分和结果接收时维护数量；空实例必须同时满足 `quantity == 0` 与空
definition/实例语义。旧存档的 `stack` 只能进入此字段，不能生成历史 ledger 或 reservation。

#### Entity/World 范围

单个 item entity；容器总量不保存于此组件，世界掉落总量也不复制到 WorldItemState。

#### ID 与关系字段

组件自身不拥有容器 ID 或网络 ID；外部引用通过 `RuntimeEntityId`，内容上限通过
`ExternalContentId` 查找，跨域归属为 `integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\ItemState.cs:3-6`、`D:\TRbackup\NLTX\src\Items\StackableItemComponent.cs:3-24` 和 dome
`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\ItemStack.cs:3-19` 都保存 type/quantity/stack
语义，均为 `partial`；存在三套数量或内容表示，尚无单一 writer。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria\Item.cs:138-140,354-363,48862-48899`、
`D:\TRbackup\Version4\Terraria\Player.cs:3740-3771,3986-4047` 和 `D:\TRbackup\Version4\Terraria\WorldItem.cs:227-255`。

### 5.3 ContainerLayoutComponent

#### 职责

保存容器槽位的稳定布局、槽位角色和可选的选择关系；不保存槽中 item entity，不保存
容量数值，不负责物品内容。

```text
componentId: IC-03
name: ContainerLayoutComponent
status: proposed
componentOwner: ItemContainerAndEconomy (candidate; PlayerGameplay 对选中关系有交接责任)
crossSubsystemOwner: integration-review
entityScope: 玩家、银行、Chest、商店或其他具名容器实体
lifecycle: 容器创建时建立；布局变更时更新；容器删除时清理；选择关系缺失时保持明确的空选择
proposedPath: src/Items/Containers/ContainerLayoutComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `slotRoles` | `IReadOnlyList<ContainerSlotRole>` | 创建时必需；不设通用槽数量 | 权威布局 | 每个槽位有唯一顺序和角色；长度必须与内容槽集合一致 | `partial` | `D:\TRbackup\Version4\Terraria\Player.cs:1013-1095`; `D:\TRbackup\Version4\Terraria\Chest.cs:38-52` |
| `selectedSlot` | `SlotIndex?` | `null` 表示没有选择 | 派生/兼容 | 选择槽必须属于该容器允许的选择范围，不改变 contents | `partial` | `D:\TRbackup\Version4\Terraria\Player.cs:2960-2962`; 当前 `D:\TRbackup\NLTX\src\Items\InventoryComponent.cs:5-31` |
| `layoutRevision` | `long` | `0` 仅表示初始布局 revision | 权威版本 | 单调递增；不能以内容变化代替布局 revision | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ContainerContentsComponent.cs:6-42` 仅有内容 revision |

#### 字段不变量

- 布局、容量和内容分别维护；`slotRoles` 不得包含 item entity。
- `selectedSlot` 是关系/选择信息，不得被解释成 `ItemDefinitionRef` 或数量。
- 玩家热键槽、装备槽和 Chest 槽可以有不同角色，不能用一个固定数组长度作为全域约定。

#### 生命周期

容器建立时按容器种类提供布局；布局调整时增加 `layoutRevision`；容器销毁时布局和选择
关系一起失效，但不自动推断槽中物品已经安全转移。

#### Entity/World 范围

容器实体或持有容器关系的玩家实体；不直接覆盖 WorldStorage 的坐标和空间实体状态。

#### ID 与关系字段

槽位 ID 是容器内局部关系标识，不得与 `PersistentItemId` 或 `NetworkId` 复用。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\InventoryComponent.cs:5-31` 将 slots、selectedSlot、ammo 和 coin 混在一起，
状态为 `partial`；dome `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\InventoryComponent.cs:6-130` 有 40 槽和 selected slot，但尚未与
跨容器内容 owner 分离。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria\Player.cs:1013-1095,2960-2962` 和
`D:\TRbackup\Version4\Terraria\Chest.cs:17-66`。

### 5.4 ContainerCapacityComponent

#### 职责

保存容器的容量约束、容器类别和嵌套规则；不保存任何槽内容或 item entity 关系。

```text
componentId: IC-04
name: ContainerCapacityComponent
status: proposed
componentOwner: 容器所属能力 (candidate)
crossSubsystemOwner: integration-review
entityScope: 玩家背包、银行、Chest、商店和其他具名容器实体
lifecycle: 创建或加载时建立；Resize/规则变化时更新；容器清理时移除
proposedPath: src/Items/Containers/ContainerCapacityComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `kind` | `ContainerKind` | 创建时必需 | 权威 | 类别决定可用槽角色，但不能改变内容 owner | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ContainerCapacityComponent.cs:3-22` |
| `slotCount` | `int` | 创建时必需；不得假定全域固定值 | 权威 | `slotCount >= 0` 且等于布局/内容槽数 | `confirmed` | `D:\TRbackup\Version4\Terraria\Chest.cs:34-42`; `D:\TRbackup\Version4\Terraria\Player.cs:1013-1095` |
| `maximumWeight` | `long?` | `null` 表示未定义，不表示无限 | 权威约束 | 若有值则非负；重量规则不得悄悄写入 item quantity | `missing` | Version4 Chest/Player 证据未发现统一重量字段 |
| `allowsNestedContainers` | `bool` | `false` 仅作为当前设计保守值，最终规则未决 | 权威规则 | 禁止嵌套时任何内容关系都不能形成循环 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ContainerCapacityComponent.cs:3-22` |

#### 字段不变量

- `slotCount` 只能由容量 owner 写入；`ContainerComponent.Capacity` 不得与其双写。
- 容量不足是容器状态，不是对 item quantity 的静默截断。
- 嵌套规则必须防止容器关系循环，并保留跨 WorldStorage 的 owner 决策。

#### 生命周期

创建/加载时初始化，Resize 或内容规则改变时变更；删除前必须由外部容器生命周期确认内容
处理结果，不能因为容量组件消失而丢弃 contents。

#### Entity/World 范围

单个容器实体；Chest 的坐标、tile 绑定和世界索引不属于此组件。

#### ID 与关系字段

不保存 item ID；与 `PersistentContainerId` 的关系由 `ContainerAccessComponent` 或最终
整合 owner 裁决。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\ContainerCapacityComponent.cs:3-22` 已有 kind、slotCount、weight 和 nested
字段，状态为 `partial`；根 `D:\TRbackup\NLTX\src\Items\ContainerComponent.cs:5-15` 又重复保存 capacity。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria\Chest.cs:34-42,94-108` 和
`D:\TRbackup\Version4\Terraria\Player.cs:1083-1095`。

### 5.5 ContainerContentsComponent

#### 职责

保存槽位到 item entity 的唯一权威关系、内容 revision 和变更元数据；不保存 definition、
数量、价格或 access lease。

```text
componentId: IC-05
name: ContainerContentsComponent
status: proposed
componentOwner: ItemContainerAndEconomy (candidate)
crossSubsystemOwner: integration-review
entityScope: 具有布局和容量的容器实体
lifecycle: 容器创建/加载时建立；内容提交时更新 revision；容器清理时与内容结果一起结束
proposedPath: src/Items/Containers/ContainerContentsComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `slots` | `IReadOnlyList<RuntimeEntityId?>` | 与 layout 的槽位集合一起初始化 | 权威关系 | 长度等于 `ContainerCapacityComponent.slotCount`；同一 item 不得无意占据多个槽 | `partial` | `D:\TRbackup\Version4\Terraria\Chest.cs:38-43`; 当前 `D:\TRbackup\NLTX\src\Items\ContainerContentsComponent.cs:6-42` |
| `revision` | `long` | `0` 表示初始内容版本 | 权威版本 | 每次可见内容变更单调递增；跨容器变更不能只更新一侧 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ContainerContentsComponent.cs:6-42`；dome `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\InventoryComponent.cs:6-130` |
| `lastMutationTick` | `long` | 需要明确时钟来源；未定义值不得伪造为运行时 tick | 快照/诊断 | 只能记录已提交变化，不得作为数量 owner | `missing` | 当前组件有字段；Version4 未提供统一组件级 mutation tick |

#### 字段不变量

- `slots` 是容器内容唯一 owner；`ContainerComponent.Contents`、Player 数组、Chest.item
  和网络解码副本不能并行写入。
- `revision` 必须覆盖内容关系和同一提交中的数量变化，不能产生内容已变而 revision 未变的
  可见状态。
- 空槽只能引用空值；空 item entity 不得带有效实例或数量状态。

#### 生命周期

容器创建时按布局建立空槽或加载快照；转移、消费、合并、结果接收和恢复时更新 revision；
容器清理时只能在内容关系已明确释放、转移或拒绝后结束。

#### Entity/World 范围

单个容器实体；跨容器关系由多个组件共同参与，但每个容器仍有自己的 revision。

#### ID 与关系字段

槽中使用 `RuntimeEntityId`，容器身份使用 `PersistentContainerId`，网络快照使用
`NetworkId`；三者由 `integration-review` 统一处理。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\ContainerComponent.cs:5-15`、`D:\TRbackup\NLTX\src\Items\ContainerContentsComponent.cs:6-42`、
`D:\TRbackup\NLTX\src\Items\InventoryComponent.cs:5-31` 和 dome `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\InventoryComponent.cs:6-130` 均有内容表示，状态为
`partial`；其中 `ContainerComponent.Contents` 与目标唯一 owner 重复。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria\Player.cs:1083-1095`、
`D:\TRbackup\Version4\Terraria\Chest.cs:38-52,491-530`、`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1646-1718,2788-2840`。

### 5.6 ContainerAccessComponent

#### 职责

保存容器访问所需的持久身份、空间位置、锁定标志和短期访问关系；不直接保存或消费物品。

```text
componentId: IC-06
name: ContainerAccessComponent
status: proposed
componentOwner: WorldStorage 或 PlayerGameplay (candidate)
crossSubsystemOwner: integration-review
entityScope: Chest、银行、玩家专属容器和商店容器
lifecycle: 容器创建时建立；打开/关闭或 lease 变化时更新；容器删除时失效
proposedPath: src/Items/Containers/ContainerAccessComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `persistentContainerId` | `PersistentContainerId` | 创建时必需；旧格式缺失 | 权威身份 | 不得与 item instance ID 或 runtime entity ID 相同语义 | `missing` | `D:\TRbackup\Version4\Terraria\Chest.cs:44-50`; `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1646-1718` |
| `tilePosition` | `TileCoordinates?` | 非空间容器为 `null` | 权威元数据 | 有值时与 WorldStorage 的 tile entity 关系一致 | `confirmed` | `D:\TRbackup\Version4\Terraria\Chest.cs:44-47` |
| `isLocked` | `bool` | `false` | 权威资格输入 | 锁定不能直接改动 contents | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ContainerAccessComponent.cs:5-32`; Version4 以使用状态间接体现 |
| `requiredAccessFlags` | `ulong` | `0` | 权威资格输入 | 只描述访问所需资格，不描述玩家能力结果 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ContainerAccessComponent.cs:5-32` |
| `currentAccessor` | `RuntimeEntityId?` | `null` | 权威关系 | 访问者必须是有效实体；不得持久化为网络 ID | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ContainerAccessComponent.cs:5-32`; `D:\TRbackup\Version4\Terraria\Chest.cs:526-529` |
| `accessLeaseUntilTick` | `long?` | `null` 表示无 active lease | 权威短期状态 | 时钟和失效规则必须明确，过期不能隐式扣物品 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ContainerAccessComponent.cs:5-32` |
| `isShared` | `bool` | `true` | 权威规则 | 共享容器不能被误当作玩家个人容器 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ContainerAccessComponent.cs:5-32` |

#### 字段不变量

- access 只决定是否可访问，不拥有物品内容、数量、货币或 reservation。
- `tilePosition`、Chest index 和 `PersistentContainerId` 是不同层级的身份，不能互换。
- lease 失效只改变访问状态，不改变已经提交的容器 revision。

#### 生命周期

创建/加载时记录身份和位置；访问变化时更新 accessor/lease；WorldStorage 删除容器时使
访问关系失效。旧存档没有持久容器 ID 时保留缺口，不把坐标直接当作永久身份。

#### Entity/World 范围

空间 Chest、银行或其他容器实体；玩家的访问上下文是外部关系，不能内嵌成容器内容。

#### ID 与关系字段

使用 `PersistentContainerId`、`TileCoordinates` 和 `RuntimeEntityId`，不使用 `NetworkId`
替代权限身份；最终 owner 为 `integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\ContainerAccessComponent.cs:5-32` 已有位置、锁、flags、accessor 和 lease，
状态为 `partial`；dome `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Server\Validation\ContainerInteractionValidator.cs:9-28` 只覆盖打开条件，未闭合
contents 访问 owner。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria\Chest.cs:17-66,459-557` 和
`D:\TRbackup\Version4\Terraria\Player.cs:1089-1095`。

### 5.7 EquipmentRelationComponent

#### 职责

保存玩家/装备宿主到功能、外观和染色槽中 item entity 的关系；不保存装备属性、伤害、状态
效果或 item definition 全量。

```text
componentId: IC-07
name: EquipmentRelationComponent
status: proposed
componentOwner: PlayerGameplay 与 ItemContainerAndEconomy (candidate)
crossSubsystemOwner: integration-review
entityScope: 具有装备槽的玩家或展示实体
lifecycle: 宿主创建/加载时建立；装备/卸下/染色关系改变时更新；宿主清理时失效
proposedPath: src/Items/Equipment/EquipmentRelationComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `ownerEntity` | `RuntimeEntityId` | 创建时必需 | 权威关系 | 所有槽关系都属于同一个宿主实体 | `confirmed` | `D:\TRbackup\Version4\Terraria\Player.cs:1013-1019` |
| `functionalSlots` | `IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?>` | 空槽集合 | 权威关系 | 每个槽最多一个 item entity；同一 item 不能同时占据冲突槽 | `partial` | `D:\TRbackup\Version4\Terraria\Player.cs:1013-1019`; 当前 `D:\TRbackup\NLTX\src\Items\EquipmentComponent.cs:5-32` |
| `vanitySlots` | `IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?>` | 空槽集合 | 权威关系 | 外观关系不得代替功能关系 | `partial` | `D:\TRbackup\Version4\Terraria\Player.cs:1013-1019`; 当前 `D:\TRbackup\NLTX\src\Items\EquipmentComponent.cs:5-32` |
| `dyeSlots` | `IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?>` | 空槽集合 | 权威关系 | 染色槽不得成为功能物品的第二 owner | `partial` | `D:\TRbackup\Version4\Terraria\Player.cs:1015-1019`; 当前 `D:\TRbackup\NLTX\src\Items\EquipmentComponent.cs:5-32` |
| `hiddenSlots` | `IReadOnlySet<EquipmentSlot>` | 空集合 | 派生/表现 | 隐藏只影响关系的表现，不删除 item entity | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\EquipmentComponent.cs:5-32` |
| `revision` | `long` | `0` | 权威版本 | 关系变化单调递增；属性派生变化不应伪造关系变化 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\EquipmentComponent.cs:5-32` |

#### 字段不变量

- 装备关系与容器槽关系必须有明确的转移归属；同一 item 不得同时作为背包内容和功能装备
  的两个权威关系。
- 属性、战斗效果和玩家能力不进入该组件；它只保存 item entity 与槽的关系。
- `functionalSlots`、`vanitySlots` 和 `dyeSlots` 可分别为空，但互相冲突的槽位规则需由
  `integration-review` 裁决。

#### 生命周期

宿主加载时初始化，装备或卸下时更新 revision；宿主清理时先解除关系，再由 item/container
归属决定后续状态。旧数组读取只能通过兼容映射，不得双写数组与新关系。

#### Entity/World 范围

玩家或展示实体。装备属性和状态效果属于其他责任面，本设计不把它们并入组件。

#### ID 与关系字段

宿主和槽内引用使用 `RuntimeEntityId`；持久化时分别保存宿主 ID、槽位和 item ID，最终
共享 owner 为 `integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\EquipmentComponent.cs:5-32` 已有三类槽和 hidden 集合，状态为 `partial`；
它尚未与 `ContainerContentsComponent` 和 PlayerGameplay 的最终 owner 闭合。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria\Player.cs:1013-1019`，以及当前
`D:\TRbackup\NLTX\src\Items\EquipmentComponent.cs:5-32`。

### 5.8 WorldItemStateComponent

#### 职责

保存物品实例与世界掉落实体的绑定、世界存在状态、来源和同步身份。位置、运动、过期和
表现字段的最终 owner 仍需跨子系统裁决，不在此把所有世界行为重新聚合。

```text
componentId: IC-08
name: WorldItemStateComponent
status: proposed
componentOwner: ItemContainerAndEconomy (payload candidate)
crossSubsystemOwner: integration-review
entityScope: 世界掉落实体
lifecycle: 世界掉落生成时建立；拾取/合并/销毁或过期时更新；掉落实体失活时清理绑定
proposedPath: src/Items/WorldDrops/WorldItemStateComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `itemEntity` | `RuntimeEntityId` | 生成时必需 | 权威关系 | 世界掉落只能绑定一个物品实例 | `partial` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:17,47-70` |
| `spawnSource` | `LootSourceRef?` | 无来源时为未知，不得伪造 | 快照/交接 | 只记录来源引用，不拥有掉落规则 | `partial` | `D:\TRbackup\Version4\Terraria\Item.cs:48686-48797`; 研究报告 §5.4 |
| `active` | `bool` | 由生成结果明确提供 | 权威 | `active == false` 时不能作为可拾取 payload | `confirmed` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:47-47,357-376` |
| `replicationId` | `ReplicationId` | 生成时由同步边界提供 | 快照/外部身份 | 不得作为 item persistent identity | `partial` | 当前 `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\WorldItemStore.cs:9-183` |
| `worldPosition` | `WorldPosition` | 生成时必需；来源未提供时拒绝创建 | 权威/跨域 | 由世界空间 owner 维护；不改变 item quantity | `partial` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:357-558` |
| `spawnedAtTick` | `long` | 生成时必需 | 快照/生命周期 | 只记录世界掉落生命周期起点 | `confirmed` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:37,550-553` |
| `despawnAtTick` | `long?` | `null` 表示未提供统一过期时刻 | 生命周期 | 过期不得静默丢弃 item payload | `partial` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:410-413,531-557`; 当前 `D:\TRbackup\NLTX\src\Items\WorldItemComponent.cs:5-31` |
| `isInstanced` / `isBeingGrabbed` / `isOnConveyor` | `bool` | `false` | 跨域状态 | 不得改变 item 实例身份或容器内容 owner | `confirmed` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:29,39,41` |
| `revision` | `long` | `0` | 权威版本 | 绑定/世界可见状态变化单调递增 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\WorldItemComponent.cs:5-31`; Version4 无统一 revision |

#### 字段不变量

- 世界掉落只引用 item entity；不得在 `WorldItemStateComponent` 复制第二个数量字段。
- `replicationId`、`spawnSource` 和 `itemEntity` 属于不同身份层级。
- 位置、寿命、合并和表现字段与 item payload 的归属不同；无法确定 owner 时保持
  `integration-review`，不在本组件内扩张职责。

#### 生命周期

生成时绑定来源和 item entity；世界存在期间保留 active/生命周期元数据；拾取或合并成功后
解除世界绑定；过期或销毁必须有明确的 payload 结果，不能仅把 active 改成 false。

#### Entity/World 范围

一个世界掉落实体；不覆盖世界区域、NPC 掉落规则或物品的容器归属。

#### ID 与关系字段

`itemEntity` 用 `RuntimeEntityId`，来源使用 `LootSourceRef`，同步使用 `ReplicationId`；
全部跨域关系标记 `integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\WorldItemComponent.cs:5-31` 与 `D:\TRbackup\NLTX\src\Items\WorldItemReservationComponent.cs:5-32`、
dome `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\WorldItemStore.cs:9-183` 已有位置、stack、实例和 ownership 数据，状态为 `partial`；
运动、payload、reservation 和 ownership 仍有重复表示。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria\WorldItem.cs:15-169,227-255,357-558`。

### 5.9 WorldItemReservationComponent

#### 职责

保存世界掉落的拾取 reservation、忽略 owner、拾取延迟和过期信息；reservation 是占用关系，
不等于扣除数量或已经完成的拾取。

```text
componentId: IC-09
name: WorldItemReservationComponent
status: proposed
componentOwner: SpawnLifecycleAndLoot 与 ItemContainerAndEconomy (candidate)
crossSubsystemOwner: integration-review
entityScope: 世界掉落实体
lifecycle: 竞争时建立；提交成功后释放；超时、失效或拒绝时释放；世界掉落销毁时清理
proposedPath: src/Items/WorldDrops/WorldItemReservationComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `reservationId` | `ReservationId?` | 未 reservation 时为 `null` | 权威关系 | 每个 active reservation 有唯一身份 | `missing` | Version4 只有玩家索引和计时字段：`D:\TRbackup\Version4\Terraria\WorldItem.cs:21,33-35` |
| `reservedFor` | `RuntimeEntityId?` | `null` | 权威关系 | 非空时必须指向有效玩家/拾取实体 | `partial` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:257-348` |
| `reservationExpiresAt` | `long?` | `null`；旧计时语义未直接映射 | 权威生命周期 | 过期自动释放占用，不减少 quantity | `partial` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:33-35,329-346` |
| `ignoreOwner` | `RuntimeEntityId?` | `null` | 权威资格输入 | 忽略关系必须有明确失效时间 | `confirmed` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:31,268-286` |
| `ignoreOwnerUntilTick` | `long?` | `null` | 权威生命周期 | 到期后不得继续屏蔽 owner | `partial` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:402-409` |
| `noGrabUntilTick` | `long` | `0` | 权威资格输入 | 当前 tick 小于该值时不能拾取 | `confirmed` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:23,554-557` |
| `enemyPickupBlockedUntilTick` | `long` | `0` | 权威资格输入 | 只影响敌对拾取资格，不改变玩家 reservation | `confirmed` | `D:\TRbackup\Version4\Terraria\WorldItem.cs:35,514-525` |
| `reservationRevision` | `long` | `0` | 权威版本 | reservation 变化单调递增且与世界掉落 revision 可区分 | `missing` | Version4 未提供统一 reservation revision |

#### 字段不变量

- 可用数量不能通过把 reservation 直接写回 `ItemStackComponent.quantity` 来表示。
- 相同 `reservationId` 或 operation 只能对应一次提交结果；不同竞争者不能超过可用 payload。
- `reservedFor`、`ignoreOwner` 和 `ReplicationId` 不得共用一个 ID 类型。

#### 生命周期

根据世界掉落竞争建立 reservation；提交成功、拒绝、取消或超时释放；世界掉落被合并或
销毁时，所有 reservation 必须显式结束。Version4 的玩家索引和计时字段只能作为兼容输入。

#### Entity/World 范围

单个世界掉落实体；玩家权限和掉落来源是跨域关系，不内嵌玩家完整状态。

#### ID 与关系字段

使用 `ReservationId`、`RuntimeEntityId` 和独立的 `ReplicationId`；最终 owner 为
`integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\WorldItemReservationComponent.cs:5-32`、dome `ItemWorldStateComponent` 和
`ItemOwnershipComponent` 都表达 reservation/ownership，状态为 `partial`；尚无统一 reservation
生命周期和唯一 writer。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria\WorldItem.cs:21-35,257-348`。

### 5.10 CraftingStateComponent

#### 职责

保存一个制作事务的当前状态、Recipe 外部引用、请求数量、参与容器 revision 和结果状态；
不保存 Recipe 全量定义，也不把材料数量副本当作容器权威。

```text
componentId: IC-10
name: CraftingStateComponent
status: proposed
componentOwner: ItemContainerAndEconomy (candidate)
crossSubsystemOwner: integration-review
entityScope: 玩家或独立制作事务实体
lifecycle: 制作请求接受时建立；状态变化时更新；成功、拒绝、超时或取消后结束并清理临时引用
proposedPath: src/Items/Crafting/CraftingStateComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `transactionId` | `TransactionId?` | 未开始时为 `null` | 权威事务身份 | 同一请求重放不得生成第二个事务身份 | `partial` | `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:11-32`; 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\CraftingRequests.cs:26-405` |
| `recipeRef` | `RecipeDefinitionRef?` | 未选择配方时为 `null` | 权威引用 | 只引用内容定义，不保存静态 Recipe 全量 | `confirmed` | `D:\TRbackup\Version4\Terraria\Recipe.cs:12-97` |
| `requestedQuantity` | `int` | `0` 表示无 active craft | 权威 | 非 active 时必须为 `0`；正数不得超过内容允许范围 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\CraftingComponent.cs:5-34`; `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:17-21` |
| `expectedSourceRevisions` | `IReadOnlyDictionary<PersistentContainerId, long>` | 空集合 | 快照 | 提交前后参与容器 revision 必须可比较 | `missing` | 完整参考有请求/材料路径，但 Version4 未提供 revision 字段 |
| `acceptedAtTick` | `long?` | 未接受时为 `null` | 快照 | 只记录已接受状态，不作为超时成功依据 | `missing` | 当前 `D:\TRbackup\NLTX\src\Items\CraftingComponent.cs:5-34` 有近似字段；Version4 请求体无该字段 |
| `craftSequence` | `long` | `0` | 权威版本 | 同一 crafting owner 内单调递增 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\CraftingComponent.cs:5-34` |
| `phase` | `CraftingPhase` | 未定义；owner 裁决前不可伪造默认终态 | 权威状态 | 只允许定义好的状态，未知状态不得当作成功 | `missing` | `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:30-32` 只有 pending queue |

#### 字段不变量

- `recipeRef` 是外部内容引用；材料占用必须由 `CraftingReservationComponent` 表达，不能
  复制成两个数量 owner。
- `expectedSourceRevisions` 缺失或冲突时只能保持未决，不能以当前数量盲目继续。
- 超时结果未知时不能自动认定成功、失败或退款。

#### 生命周期

空闲状态不带 active transaction；请求接受后建立状态和 revision 快照；完成、拒绝、取消、
超时或结果不可提交时释放事务引用，具体结算 owner 仍需整合裁决。

#### Entity/World 范围

玩家制作上下文或独立 crafting transaction entity，不直接成为容器或 Recipe 实体。

#### ID 与关系字段

使用 `TransactionId`、`RecipeDefinitionRef` 和 `PersistentContainerId`；均为跨域候选，
`crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\CraftingComponent.cs:5-34` 已有 active recipe、requested、revision、sequence
和 reserved materials，但状态为 `partial`；没有完整 reservation 状态闭环。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:9-32`、
`D:\TRbackup\Version4\Terraria\Recipe.cs:12-97` 以及完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\CraftingRequests.cs:26-405`。

### 5.11 CraftingReservationComponent

#### 职责

保存制作事务对来源容器、槽位和 item entity 的暂时占用关系；不直接减少堆叠数量，不保存
Recipe 内容，也不把 reservation 当作成功结果。

```text
componentId: IC-11
name: CraftingReservationComponent
status: proposed
componentOwner: ItemContainerAndEconomy (candidate)
crossSubsystemOwner: integration-review
entityScope: 制作事务实体或来源容器上的 reservation 关系实体
lifecycle: 材料占用时建立；成功提交后转为已结算并清理；拒绝/取消/超时释放
proposedPath: src/Items/Crafting/CraftingReservationComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `reservationId` | `ReservationId` | 建立时必需 | 权威身份 | 在其生命周期内唯一 | `missing` | Version4 `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:11-32` 无该字段 |
| `transactionId` | `TransactionId` | 建立时必需 | 权威关系 | 必须指向一个 active `CraftingStateComponent` | `partial` | 完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\CraftingRequests.cs:26-405` |
| `sourceContainerId` | `PersistentContainerId` | 建立时必需 | 权威关系 | 与 source slot 所属容器匹配 | `missing` | Version4 请求只带 Item 列表，不带持久容器 ID |
| `sourceSlot` | `SlotIndex` | 建立时必需 | 权威关系 | 必须属于 source container layout | `partial` | `D:\TRbackup\Version4\Terraria\Player.cs:3986-4047`; `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:17-20` |
| `sourceItemEntity` | `RuntimeEntityId` | 建立时必需 | 权威关系 | 指向非空 item entity | `missing` | Version4 请求使用 Item 副本，不是 ECS entity 引用 |
| `quantity` | `int` | 建立时必需 | 权威占用 | 正数且不超过可用数量 | `partial` | 完整参考制作材料消费路径 `:26-405` |
| `expectedContainerRevision` | `long` | 建立时必需 | 快照 | 提交时必须再次匹配 | `missing` | Version4 无容器 revision 字段 |
| `createdAt` / `expiresAt` | `long?` | 创建时提供；未提供则状态未决 | 权威生命周期 | expires 不能早于 created；过期释放占用 | `missing` | Version4 pending queue `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:30-32` |
| `state` | `ReservationState` | 建立时必需；未建立时不存在 | 权威状态 | 只能从 active 到 committed/released/expired 等终态 | `missing` | Version4 没有 reservation state 枚举 |

#### 字段不变量

- reservation 数量只参与可用量计算，不从 `ItemStackComponent.quantity` 中提前扣除。
- 一个 source slot 的 active reservation 总量不能超过其 committed quantity。
- 事务重复到达只返回既有结果；不能因为重复 reservation 再次消费或退款。

#### 生命周期

建立后等待 revision 校验和最终结果；成功结算后消费占用并结束；失败、取消或过期释放
占用且保持可见数量不变。完整参考的局部消费/退款行为只作为风险证据，不作为当前行为。

#### Entity/World 范围

制作事务与容器/物品之间的关系状态；不成为静态 Recipe 的组成部分。

#### ID 与关系字段

使用 `ReservationId`、`TransactionId`、`PersistentContainerId`、`RuntimeEntityId`；
跨子系统 owner 为 `integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\CraftingMaterialReservation.cs:3-9` 只有 Item、source slot、quantity 和
stack key，状态为 `partial`；缺少 reservation identity、container revision、过期和终态。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:11-32`、
`D:\TRbackup\Version4\Terraria\Player.cs:3986-4047` 和完整参考 `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:26-405`。

### 5.12 ShopInventoryComponent

#### 职责

保存一个商店实体的商品快照、库存数量和商店 revision；商品价格和结果引用作为字段值对象
保存，不把买方容器或货币余额放进商店组件。

```text
componentId: IC-12
name: ShopInventoryComponent
status: proposed
componentOwner: ItemContainerAndEconomy (Commerce candidate)
crossSubsystemOwner: integration-review
entityScope: NPC 商店、旅行商店或其他商店实体
lifecycle: 商店创建/内容加载时建立；商品快照或库存变化时更新；商店关闭/销毁时清理
proposedPath: src/Items/Commerce/ShopInventoryComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `offers` | `IReadOnlyList<CommerceOffer>` | 空集合；商品全集必须由内容输入提供 | 快照/权威库存 | 每个 offer ID 唯一；商品数量不能绕过库存 revision | `partial` | `D:\TRbackup\Version4\Terraria\Chest.cs:1222`; 完整参考 `D:\TRbackup\Version4\Terraria\Chest.cs:1403+`; 当前 `D:\TRbackup\NLTX\src\Items\ShopInventoryComponent.cs:5-24` |
| `shopRevision` | `long` | `0` | 权威版本 | 商品或 stock 改变时单调递增 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ShopInventoryComponent.cs:5-24` |
| `restockAtTick` | `long?` | `null` | 权威生命周期 | 只有支持 restock 的商店才有值 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\ShopInventoryComponent.cs:5-24`; `D:\TRbackup\Version4\Terraria\Chest.cs:1222` 的 SetupShop 为空体 |
| `lastRestockTick` | `long?` | `null` | 快照 | 不能作为商品生成成功的依据 | `missing` | `D:\TRbackup\Version4\Terraria\Chest.cs:1222` 的 `SetupShop` 为空体，未确认统一 restock 状态 |

#### 字段不变量

- `CommerceOffer` 是商品快照值对象，不是买方余额或 ledger。
- `offers` 的 stock、price snapshot 和 offer revision 必须能被单独识别，不能让 Item 的
  静态 price 字段成为商店库存的第二个 owner。
- 商店商品结果进入玩家容器时仍必须遵循 item entity 和 contents 的唯一 owner 规则。

#### 生命周期

内容加载时形成商品快照；商店库存变化时更新 revision；商店清理时商品快照失效。`D:\TRbackup\Version4\Terraria\Chest.cs:1222` 的 `SetupShop` 空体使完整商品全集保持 `partial`，不得以完整参考直接提升状态。

#### Entity/World 范围

一个商店实体；NPC 心情、商店 UI 和玩家容器不属于该组件。

#### ID 与关系字段

`offerId` 和商品 content id 使用 `ExternalContentId`，商店实体使用 `RuntimeEntityId` 或
`PersistentContainerId`；跨域 owner 为 `integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\ShopInventoryComponent.cs:5-24` 与 `D:\TRbackup\NLTX\src\Items\ShopOffer.cs:3-11` 已有 offers、stock、
price 和 currency 字段，状态为 `partial`；缺少完整商品来源和一致结算记录。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria\Chest.cs:17-52,1222`、
`D:\TRbackup\Version4\Terraria\Item.cs:268-272` 和完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Chest.cs:1403+`。

### 5.13 CurrencyBalanceComponent

#### 职责

保存账户或玩家的逻辑货币余额、货币 revision 和上限约束；不把金币 Item 槽扫描结果直接
当作逻辑余额，也不保存商店商品库存。

```text
componentId: IC-13
name: CurrencyBalanceComponent
status: proposed
componentOwner: Commerce (candidate within ItemContainerAndEconomy)
crossSubsystemOwner: integration-review
entityScope: 玩家账户、角色账户或明确的经济账户实体
lifecycle: 账户创建/加载时建立；结算成功时更新；账户清理或合并时结束
proposedPath: src/Items/Commerce/CurrencyBalanceComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `accountId` | `ExternalAccountId` | 创建时必需；旧格式缺失 | 权威身份 | 一个账户 ID 不能隐式代表 item entity 或网络连接 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\Commerce\CurrencyBalanceComponent.cs:5-41` |
| `balances` | `IReadOnlyDictionary<ExternalContentId, long>` | 空集合 | 权威余额 | 每项非负；扣款不能使余额变负 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\Commerce\CurrencyBalanceComponent.cs:5-41`; `D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs:28-43` |
| `currencyCaps` | `IReadOnlyDictionary<ExternalContentId, long>` | 空集合；无 cap 不得推断无限 | 权威约束 | balance 不得超过相应 cap；cap 来源需可追踪 | `partial` | `D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs:12-25`; `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.UI\CustomCurrencySystem.cs:8-296` |
| `revision` | `long` | `0` | 权威版本 | 每次余额变化单调递增 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\Commerce\CurrencyBalanceComponent.cs:5-41` |
| `lastTransactionId` | `TransactionId?` | `null` | 快照/幂等线索 | 只能指向已记录的结算结果 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\Commerce\CurrencyBalanceComponent.cs:5-41`; Version4 无 ledger 字段 |

#### 字段不变量

- logical balance 与货币 Item representation 分离；扫描 coin stack 不能直接覆盖余额。
- 普通货币换算的单位和溢出语义必须明确；已有 `1/100/10000/1000000` 只作为当前 dome
  价格实现和 Version4 公开边界的证据，不代表所有特殊货币都采用同一单位。
- 结算失败不产生可见扣款；未知结果不能通过再次扣款或盲目加回解决。

#### 生命周期

账户加载时建立已知余额；成功结算后更新 revision；退款或补偿必须引用明确交易记录；旧
存档没有逻辑余额时保持缺失状态，不从 coin Item 无条件伪造历史余额。

#### Entity/World 范围

账户或玩家经济实体；货币 Item 仍由 `ItemInstanceComponent`、`ItemStackComponent` 和
容器内容关系表示。

#### ID 与关系字段

使用 `ExternalAccountId`、`ExternalContentId` 和 `TransactionId`；账户、内容和交易都是
跨子系统共享候选，`crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\Commerce\CurrencyBalanceComponent.cs:5-41` 已有账户、余额、revision 和
last transaction 字段，状态为 `partial`；dome `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\Systems\ShopPurchaseSystem.cs:31-400` 仅覆盖局部
普通/特殊货币购买路径。

#### 证据

主要事实来自 `D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs:8-51`、
`D:\TRbackup\Version4\Terraria\Item.cs:270-272` 和当前 NLTX 组件。

### 5.14 CommerceLedgerComponent

#### 职责

保存交易 receipt、借记/贷记、退款状态和幂等记录；不作为 UI 缓存，不直接保存商店目录或
玩家全部物品。

```text
componentId: IC-14
name: CommerceLedgerComponent
status: proposed
componentOwner: Commerce (candidate within ItemContainerAndEconomy)
crossSubsystemOwner: integration-review
entityScope: 账户、商店或明确的结算账本实体
lifecycle: 账本创建/加载时建立；每个已确认结算追加记录；保留策略变化时更新；实体清理时归档或结束
proposedPath: src/Items/Commerce/CommerceLedgerComponent.cs (status: proposed)
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | `evidenceStatus` | 证据 |
|---|---|---|---|---|---|---|
| `entries` | `IReadOnlyList<CommerceLedgerEntry>` | 空集合 | 权威历史 | 每条记录有唯一 transaction/operation identity 和明确状态 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\Commerce\CommerceLedgerComponent.cs:5-39`; Version4 CustomCurrency 为默认/空行为 |
| `lastSequence` | `long` | `0` | 权威版本 | 追加顺序单调递增；不能跳过已确认记录 | `partial` | 当前 `D:\TRbackup\NLTX\src\Items\Commerce\CommerceLedgerComponent.cs:5-39` |
| `retentionFloorSequence` | `long` | `0` | 权威保留元数据 | 不得大于 `lastSequence`；删除历史需留诊断边界 | `missing` | 当前组件有字段；Version4 无对应 ledger |
| `unknownOutcome` | `bool` | `false` | 派生 | 只能由 entry 状态派生，不能作为第二个结算状态 | `missing` | 当前 `ContainsUnknownOutcome` 为局部派生；Version4 无 ledger |

#### 字段不变量

- ledger 记录必须与 `CurrencyBalanceComponent.revision`、商品 stock revision 和结果 item
  identity 可关联，但不重复拥有这些状态。
- 同一 operation ID 的重复请求只能复用既有结果；重复退款不能再次 credit。
- 结果未知时保留 `unknownOutcome`，不能把未知当作成功或失败。

#### 生命周期

结算记录在明确结果后追加；未知、失败和退款保持可审计状态；保留下限变化不能伪造历史
receipt。旧存档没有账本时从新 revision 开始，不补造过去交易。

#### Entity/World 范围

账户或商店结算范围的账本实体；不把每笔交易或每个金币建成新的一级 Component。

#### ID 与关系字段

记录使用 `TransactionId`、`OperationId`、`ExternalAccountId` 和商品/货币的
`ExternalContentId`；均需要 `integration-review` 的共享 owner。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\Items\Commerce\CommerceLedgerComponent.cs:5-39` 已有 entries、sequence、保留下限
和未知状态派生，状态为 `partial`；缺少与余额、库存和结果身份的闭合关系。

#### 证据

主要事实来自当前 NLTX `D:\TRbackup\NLTX\src\Items\Commerce\CommerceLedgerComponent.cs:5-39`、Version4
`D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs:8-51` 和完整参考 `D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs:8-296`。

## 6. Entity 与 Component 组合

下表只描述状态共存关系，不描述行为执行或先后顺序。

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| 普通 item entity | `ItemInstanceComponent`、`ItemStackComponent` | `WorldItemStateComponent`、`EquipmentRelationComponent` 的关系引用 | 空 item 不得带有效 `ItemStackComponent` 或实例修饰 | 实例修饰与数量有不同不变量，但必须共同表达一个非空物品实体 |
| 玩家背包/银行容器 | `ContainerLayoutComponent`、`ContainerCapacityComponent`、`ContainerContentsComponent` | `ContainerAccessComponent` | 不得再以 `ContainerComponent` 同时持有 capacity 和 contents | 布局、容量、内容和访问的生命周期不同，玩家可拥有多个容器 |
| Chest/WorldStorage 容器 | `ContainerLayoutComponent`、`ContainerCapacityComponent`、`ContainerContentsComponent`、`ContainerAccessComponent` | `ShopInventoryComponent`（仅商店实体） | 不得把 Chest 空间元数据并入 contents | Chest 坐标/访问生命周期与内容 revision 不同 |
| 玩家装备宿主 | `EquipmentRelationComponent` | `ContainerLayoutComponent`（若装备槽需布局描述） | 与同一 item 的冲突容器归属必须互斥 | 装备槽只保存 item 关系，不拥有属性计算 |
| 世界掉落实体 | `WorldItemStateComponent`、被引用 item entity 的 `ItemInstanceComponent`、`ItemStackComponent` | `WorldItemReservationComponent` | active 世界掉落不得同时被标记为已提交装备/容器唯一归属 | 世界绑定、reservation 和 item payload 分开，避免世界状态复制数量 |
| 制作事务实体 | `CraftingStateComponent` | `CraftingReservationComponent` | 非 active crafting 不得保留 active reservation | 制作状态和材料占用拥有不同生命周期 |
| 商店实体 | `ShopInventoryComponent` | `ContainerAccessComponent` | 不得持有买方 `ContainerContentsComponent` 或 `CurrencyBalanceComponent` | 商店库存与买方资产必须隔离 |
| 经济账户实体 | `CurrencyBalanceComponent`、`CommerceLedgerComponent` | 无 | 不得把 coin item slot 作为 balance 的内嵌数组 | 余额是逻辑结算状态，金币 item 是另一种物品表示 |

组合约束的共同原则是：内容关系只在 `ContainerContentsComponent` 中出现，数量只在
`ItemStackComponent` 中出现，reservation 只在相应 reservation 组件中出现；任何兼容字段
都不能形成第二套权威写入。

## 7. 组件拆分与合并决策

### 7.1 必须拆分

| 决策 ID | 拆分对象 | 决策 | 依据 |
|---|---|---|---|
| `SPLIT-COMP-01` | definition 与 instance | 分开；definition 只通过 `ItemDefinitionRef` 引用，instance 保存 prefix、variant、dye、收藏和名称覆盖 | Version4 `SetDefaults` 会重建内容属性，而 `Prefix` 会改变实例统计和价值；生命周期和 owner 不同 |
| `SPLIT-COMP-02` | instance 与 stack | 分开；数量只有 `ItemStackComponent` owner | Player 消费、WorldItem 合并和 dome `ItemStack` 都直接改变数量；实例修饰不必随每次数量更新 |
| `SPLIT-COMP-03` | layout、capacity、contents | 分开；布局不带内容，容量不带槽关系 | Player、银行、Chest 和商店的槽范围、容量和内容生命周期不同；现有 `ContainerComponent` 重复表达 |
| `SPLIT-COMP-04` | contents 与 access | 分开；访问资格不拥有 item 关系 | Chest 的坐标/锁/访问者和槽内容可以独立失效，WorldStorage 需要不同 owner |
| `SPLIT-COMP-05` | item payload 与 world state/reservation | 分开；世界掉落不复制 item 数量 | WorldItem 同时包含 inner、reservation、寿命、移动和表现状态；合并/拾取竞争需要独立 reservation |
| `SPLIT-COMP-06` | crafting state 与 material reservation | 分开；制作状态不把材料占用数组当作扣除结果 | 请求状态、容器 revision 和材料占用具有不同生命周期；Version4/完整参考存在 pending、局部消费和退款风险 |
| `SPLIT-COMP-07` | shop stock、balance、ledger | 分开；商品、资产和审计历史不互相持有 | 商品 revision、账户余额 revision 和交易序列的更新原因不同 |
| `SPLIT-COMP-08` | equipment relation 与 item/contents | 分开；装备只保存关系 | Player 的 armor/dye/miscEquips 是不同关系范围，属性与战斗责任不进入本组件 |

### 7.2 可以合并或保留为语义单元

| 决策 ID | 对象 | 决策 | 依据 |
|---|---|---|---|
| `MERGE-COMP-01` | item 的 prefix、variant、dye、favorited、name override | 保留在 `ItemInstanceComponent` | 这些字段都随单个实例创建/清空，且共同表达实例修饰；拆成多个组件会制造空实例同步边 |
| `MERGE-COMP-02` | reservation 的身份、owner、过期和资格限制 | 各自保留在对应 reservation 组件内 | 它们共同维护 reservation 的有效性；但 crafting 与 world pickup 的生命周期不同，不能跨域合并成一个通用 reservation 组件 |
| `MERGE-COMP-03` | ledger 的 entries、sequence、保留下限 | 保留在 `CommerceLedgerComponent` | 共同表达账本历史与版本；`unknownOutcome` 仅作派生字段，不单独建组件 |
| `MERGE-COMP-04` | 容器槽关系和内容 revision | 保留在 `ContainerContentsComponent` | revision 保护的是同一内容关系；若分开会产生隐含同步要求 |

## 8. 不单独创建 Component 的对象

| 对象 | 设计状态 | 处理方式 | 不单独创建的理由 |
|---|---|---|---|
| `ItemDefinitionRef` | `proposed` | 作为 `ItemInstanceComponent.definitionRef` 的值对象 | 它是 ContentCatalog 的外部引用，不拥有持续实例状态、数量或 reservation |
| `CommerceOffer` | `proposed` | 作为 `ShopInventoryComponent.offers` 的商品快照值对象 | 单个 offer 没有独立容器范围；stock、price snapshot 和 offer revision 共同属于商店库存 |
| `RecipeDefinition`、`RecipeGroup` | `deferred` | 由 ContentCatalog 保存的静态定义引用 | Version4 `Recipe`/`RecipeGroup` 是内容规则，不是持续变化的实体状态 |
| `LootSourceRef` | `proposed` | 作为 `WorldItemStateComponent.spawnSource` 的交接值对象 | 掉落来源只提供来源身份，不重新拥有掉落规则、随机或世界掉落生命周期 |
| 单个 Item 实例 | `rejected` | 由 item entity 组合 `ItemInstanceComponent` 与 `ItemStackComponent` | 实例是实体状态，不是一个新的子系统级 Component 类型 |
| 单个 slot 或单个 stack | `rejected` | 作为 `ContainerContentsComponent` 的关系或 `ItemStackComponent` 的字段 | 按槽位/字段建组件会复制 owner，并制造大量结构变化 |
| 单个 Chest 或银行 | `rejected` | 作为容器实体组合布局、容量、内容和访问组件 | Chest 的空间身份和内容状态必须分开；单个 Chest 不应成为独立组件类别 |
| 单个金币 Item | `rejected` | 仍是普通 item entity；逻辑价值由 `CurrencyBalanceComponent` 表达 | coin stack 是货币表示，不是全局余额、价格或账本 owner |
| 单个 transaction、reservation 或 receipt | `rejected` | 作为相应组件中的值对象/关系记录 | 身份本身不能替代状态范围；只有持续关系和不可重复记录才进入相应组件 |
| 网络/存档/客户端快照记录 | `deferred` | 保持边界记录，字段映射到权威组件 | 快照可过期、可丢失或版本化，不能反向成为模拟 authority |
| `SelectedItem` | `deferred` | 作为 `ContainerLayoutComponent.selectedSlot` 的关系字段 | 选择槽没有独立实例生命周期，也不应复制 item identity |

## 9. 当前 NLTX 组件覆盖

本节的 `status` 只描述当前 NLTX 已有组件或数据模型覆盖，取值均为 `partial` 或
`existing`；它与第 5 节目标组件的 `status: proposed` 分开。当前整体仍为 `partial`，
因为局部数据存在不等于跨容器、reservation、货币和快照状态已经闭合。

| 当前路径 | 当前 `status` | 已有状态 | 目标映射 | 覆盖判断 | 证据 |
|---|---|---|---|---|---|
| `D:\TRbackup\NLTX\src\Items\ItemDefinitionComponent.cs:3-21` | `partial` | ContentId、definition revision、兼容 Type | `ItemInstanceComponent.definitionRef` | 内容 ID 基础存在，但与 `ItemState.Type`、dome `ItemStack.ItemType` 重复 | 当前文件；Version4 `D:\TRbackup\Version4\Terraria\Item.cs:122` |
| `D:\TRbackup\NLTX\src\Items\ItemInstanceComponent.cs:5-31` | `partial` | persistent Guid、prefix、variant、dye、收藏、名称覆盖 | `ItemInstanceComponent` | 实例字段较完整，但缺创建、变更和存档闭环 | 当前文件；Version4 `D:\TRbackup\Version4\Terraria\Item.cs:124,286` |
| `D:\TRbackup\NLTX\src\Items\ItemState.cs:3-6` | `partial` | Type、Prefix、Stack、IsEmpty | `ItemInstanceComponent` + `ItemStackComponent` | 与目标组件重复，不能继续作为数量 writer | 当前文件；Version4 `D:\TRbackup\Version4\Terraria\Item.cs:354-363` |
| `D:\TRbackup\NLTX\src\Items\StackableItemComponent.cs:3-24` | `partial` | Quantity、MaximumQuantity、StackKey、Unlimited | `ItemStackComponent` | 与 `ItemState.Stack` 重复，缺唯一 writer | 当前文件；Version4 `D:\TRbackup\Version4\Terraria\Item.cs:138-140` |
| `D:\TRbackup\NLTX\src\Items\ContainerComponent.cs:5-15` | `partial` | Capacity、Contents | `ContainerCapacityComponent` + `ContainerContentsComponent` | 同时持有两种状态，形成重复 authority | 当前文件；Version4 `D:\TRbackup\Version4\Terraria\Chest.cs:38-43` |
| `D:\TRbackup\NLTX\src\Items\ContainerCapacityComponent.cs:3-22` | `partial` | kind、slotCount、weight、nested | `ContainerCapacityComponent` | 有容量基础，但与旧 Capacity 未统一 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\ContainerContentsComponent.cs:6-42` | `partial` | slots、revision、lastMutationTick | `ContainerContentsComponent` | 已有 revision，但无跨容器原子一致性证明 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\ContainerAccessComponent.cs:5-32` | `partial` | 坐标、锁、flags、accessor、lease | `ContainerAccessComponent` | 访问数据存在，最终 owner 与 WorldStorage 未决 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\InventoryComponent.cs:5-31` | `partial` | slots、ammo、coin、trash、selected、revision | `ContainerLayoutComponent` + `ContainerContentsComponent` | 多种容器/关系混合，需拆成能力边界 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\EquipmentComponent.cs:5-32` | `partial` | functional、vanity、dye、hidden、revision | `EquipmentRelationComponent` | 槽关系基础存在，跨 Player owner 未决 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\CraftingComponent.cs:5-34` | `partial` | active recipe、requested、revision、sequence、reserved | `CraftingStateComponent` | 事务状态存在，但 reservation 只是数据列表 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\CraftingMaterialReservation.cs:3-9` | `partial` | item、source slot、quantity、stack key | `CraftingReservationComponent` | 缺 reservation ID、容器 revision、过期和终态 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\ShopInventoryComponent.cs:5-24` | `partial` | offers、revision、restock | `ShopInventoryComponent` | 商店局部状态存在，缺完整商品全集与结算关联 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\ShopOffer.cs:3-11` | `partial` | offer、item、variant、quantity、price、currency | `CommerceOffer` 值对象 | 不应单独成为 Component；缺价格快照和商品 revision 语义 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\Commerce\CurrencyBalanceComponent.cs:5-41` | `partial` | account、balances、revision、last transaction | `CurrencyBalanceComponent` | 余额数据存在，coin representation 与结算边界未闭合 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\Commerce\CommerceLedgerComponent.cs:5-39` | `partial` | entries、sequence、retention、unknown outcome | `CommerceLedgerComponent` | 账本数据存在，缺与余额/商品/结果的统一记录关系 | 当前文件 |
| `D:\TRbackup\NLTX\src\Items\WorldItemComponent.cs:5-31` | `partial` | position、velocity、寿命、instanced、grabbed、conveyor、revision | `WorldItemStateComponent` | 混入运动和表现状态，最终 owner 未决 | 当前文件；Version4 `D:\TRbackup\Version4\Terraria\WorldItem.cs:357-558` |
| `D:\TRbackup\NLTX\src\Items\WorldItemReservationComponent.cs:5-32` | `partial` | reserved、ignore owner、grab delay、enemy block | `WorldItemReservationComponent` | 与 dome ownership/world state 重复 | 当前文件；Version4 `D:\TRbackup\Version4\Terraria\WorldItem.cs:21-35,257-348` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\ItemStack.cs:3-19` | `partial` | ItemType、Quantity、Prefix、空状态 | `ItemStackComponent` + `ItemInstanceComponent` | 值对象层面可用，但不是全域唯一数量 owner | 当前文件 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\InventoryComponent.cs:6-130` | `partial` | 40 槽、revision、selected、instance state | `ContainerLayoutComponent` + `ContainerContentsComponent` | 单一 Inventory 局部有效，未覆盖 Player/Chest/world 统一关系 | 当前文件 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\WorldItemStore.cs:9-183` | `partial` | Arch entity、world item、stack、instance、ownership | `WorldItemStateComponent` + `WorldItemReservationComponent` | 世界掉落基础存在，ownership/reservation 重复 | 当前文件 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\Systems\ShopPurchaseSystem.cs:31-400` | `partial` | 局部普通/特殊货币购买、revision、receipt | `ShopInventoryComponent`、`CurrencyBalanceComponent`、`CommerceLedgerComponent` | 仅局部购买路径，不能代表全域结算已闭合 | 当前文件 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\Systems\ItemPriceSystem.cs:5-84` | `partial` | 金银铜铂换算、价格和溢出检查 | `CommerceOffer` / `CurrencyBalanceComponent` 的字段依据 | 纯价格基础不等于余额和账本结算 | 当前文件 |

已确认的重复状态如下：

1. `ItemState.Stack`、`StackableItemComponent.Quantity`、dome `ItemStack.Quantity` 重复表达数量；
2. `ItemDefinitionComponent.Type`、`ItemState.Type`、dome `ItemStack.ItemType` 重复表达内容 ID；
3. `ContainerComponent.Capacity/Contents` 与 capacity/contents 两个组件重复表达容器状态；
4. `CraftingMaterialReservation`、`WorldItemReservationComponent`、dome `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\Components\ItemWorldStateComponent.cs` 和 `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Items\Components\ItemOwnershipComponent.cs` 分散表达 reservation/ownership；
5. runtime entity ID、持久化实例 ID、网络 ID、replication ID 和 external content ID 尚未统一分离；
6. `CurrencyBalanceComponent` 与 `CommerceLedgerComponent` 有数据基础，但未形成完整结算闭环。

## 10. 组件级 evidence-gap

以下缺口不会阻止本文件交付，但会阻止把相应设计标为 `baseline`。`evidenceStatus` 使用
公共协议允许的状态；`evidence-gap` 是缺口类别，不是额外状态枚举。

| 缺口 ID | 影响组件 | `evidenceStatus` | 已知事实 | 未确认内容 |
|---|---|---|---|---|
| `EG-COMP-01` | `ItemInstanceComponent` | `missing` | Version4 Item 有 type/prefix/variant 等实例字段；旧 Chest 快照没有实例持久 ID | ID 分配、旧快照生成策略、玩家快照字段 |
| `EG-COMP-02` | `ItemStackComponent` | `partial` | Version4 `CanStack` 只比较 type/prefix；NLTX 有三套 quantity/type 表示 | variant、uniqueStack、扩展资格与 maximumQuantity 的最终 owner |
| `EG-COMP-03` | `ContainerLayoutComponent`、`ContainerCapacityComponent`、`ContainerContentsComponent` | `unresolved` | Player、bank、Chest、inventory 和 dome 已有局部容器数据 | 全量容器 writer、容量 owner、跨容器 revision 语义 |
| `EG-COMP-04` | `ContainerAccessComponent` | `unresolved` | Chest 坐标/生命周期与 contents 分开，但当前访问校验只覆盖打开 | persistent container ID 与 WorldStorage、Player access 的最终 owner |
| `EG-COMP-05` | `EquipmentRelationComponent` | `unresolved` | Player 有 armor/dye/miscEquips 数组，NLTX 有 EquipmentComponent | 装备关系与容器关系是否由 PlayerGameplay、ItemContainerAndEconomy 或整合层拥有 |
| `EG-COMP-06` | `WorldItemStateComponent`、`WorldItemReservationComponent` | `unresolved` | Version4 世界掉落同时持有 payload、reservation、寿命和表现 | 世界运动/销毁 owner、pickup reservation 与 item commit 的边界 |
| `EG-COMP-07` | `CraftingStateComponent`、`CraftingReservationComponent` | `partial` | Version4 有 pending queue，完整参考有局部消费/退款；两端行为不同 | server authority、超时未知、reservation 终态和结果不足语义 |
| `EG-COMP-08` | `ShopInventoryComponent`、`CurrencyBalanceComponent`、`CommerceLedgerComponent` | `partial` | Version4 SetupShop/CustomCurrency 存在空体；dome 有局部购买和价格逻辑 | 商品全集、特殊货币范围、金币 Item 与逻辑余额转换、ledger owner |
| `EG-COMP-09` | 所有跨容器组件 | `partial` | WorldFile 保存 type/stack/prefix；网络路径直接重建 Item/Chest/WorldItem | 所有旧存档/网络版本字段、未知结果和坏输入的诊断映射 |
| `EG-COMP-10` | 全部目标组件 | `missing` | `D:\TRbackup\NLTX\Test\Terraria.WorldInteraction.Components.Verification\Program.cs:7-48` 只覆盖有限存储对象 | 本子系统的跨容器、reservation、制作、货币和快照覆盖尚未存在 |

## 11. 未决组件 owner

以下事项会改变组件组成、字段 authority 或共享关系，故标记为 `decision-required`。每项
都保留候选方案，不在当前会话替整合会话宣布最终 owner。

### BD-COMP-01：数量与堆叠上限的 owner

- 冲突字段：`ItemState.Stack`、`StackableItemComponent.Quantity/MaximumQuantity`、dome `ItemStack.Quantity`、Version4 `Item.stack/maxStack`。
- 当前候选 owner：数量由 `ItemStackComponent` 保存；上限由 ContentCatalog 的 definition 引用派生。
- 方案 A：ItemContainerAndEconomy 独占 quantity，ContentCatalog 独占 stack limit。优点是实例
  和内容解耦；影响是需要统一扩展资格和 variant 规则。
- 方案 B：ContentCatalog 也保存运行时 stack state。影响是静态内容目录携带可变实体状态，
  会扩大共享写集，不符合当前组件拆分倾向。
- 方案 C：保留 legacy `ItemState` 为 façade。影响是必须严格禁止双写，并增加 revision
  一致性成本。
- 当前不能裁决：多个现有实现和跨 Player/WorldItem/Commerce 使用者仍读取不同表示；最终
  writer 需要整合会话确认。

### BD-COMP-02：容器 contents、capacity 与 access 的 owner

- 冲突字段：`ContainerComponent.Capacity/Contents`、`ContainerCapacityComponent`、
  `ContainerContentsComponent`、Player 数组、Chest.item、WorldStorage 容器身份。
- 当前候选 owner：contents 归 ItemContainerAndEconomy；capacity 归容器所属能力；access
  归 WorldStorage 或 PlayerGameplay。
- 方案 A：ItemContainerAndEconomy 拥有 contents，WorldStorage 只拥有 Chest 身份/空间，
  PlayerGameplay 提供玩家访问上下文。影响是跨域提交必须共享 container revision。
- 方案 B：WorldStorage 同时拥有 Chest contents。影响是库存、制作和商店需要依赖 WorldStorage，
  玩家背包又形成第二个 contents owner。
- 方案 C：按容器类型复制 owner。影响是 Player/Chest/bank/world 之间无法保证统一转移语义。
- 当前不能裁决：固定子系统之间的容器写入边界未由整合会话锁定。

### BD-COMP-03：装备关系的 owner

- 冲突字段：Player `armor/dye/miscEquips`、根 `EquipmentComponent`、dome loadout/equipment
  状态和 item/container 关系。
- 当前候选 owner：关系由 ItemContainerAndEconomy 保存，PlayerGameplay 拥有装备资格和玩家语义。
- 方案 A：关系归 ItemContainerAndEconomy，资格由 PlayerGameplay 提供。影响是装备转移和
  inventory contents 需要同一提交关系。
- 方案 B：关系归 PlayerGameplay。影响是 ItemContainerAndEconomy 只能通过交接引用访问，
  可能产生第二套槽关系。
- 方案 C：功能、外观、染色分别归不同 owner。影响是同一 item 冲突检查更复杂。
- 当前不能裁决：装备属性和物品归属跨 PlayerGameplay、CombatAndStatus 与当前子系统。

### BD-COMP-04：WorldItem 状态与 reservation 的 owner

- 冲突字段：Version4 `WorldItem.inner`、位置/寿命、`playerIndexTheItemIsReservedFor`、
  NLTX `WorldItemComponent`、`WorldItemReservationComponent`、dome `ItemWorldStateComponent`
  和 `ItemOwnershipComponent`。
- 当前候选 owner：payload/结果关系归 ItemContainerAndEconomy；运动/寿命归 SpawnLifecycleAndLoot；reservation 为交接状态。
- 方案 A：WorldItemState 只保存 payload 绑定和 active，运动/寿命全部留相邻责任面。影响是
  pickup 前后需要稳定的跨域引用。
- 方案 B：WorldItemState 同时保存位置/寿命。影响是世界掉落组件变大，但局部不变量较集中。
- 方案 C：reservation 完全归 SpawnLifecycleAndLoot。影响是 ItemContainerAndEconomy 无法
  单独确认 pickup commit 的占用资格。
- 当前不能裁决：世界掉落运动、拾取、合并和结果内容在 Version4 同一类中混合。

### BD-COMP-05：逻辑余额与金币 Item 的 owner

- 冲突字段：Version4 `Item.shopSpecialCurrency/shopCustomPrice`、CustomCurrencySystem 的
  货币计数、dome coin slots、`CurrencyBalanceComponent` 和 `CommerceLedgerComponent`。
- 当前候选 owner：逻辑余额/账本归 Commerce；coin Item 归 ItemContainerAndEconomy。
- 方案 A：逻辑余额唯一权威，coin Item 只作为显式表示。影响是需要明确转换和兼容快照策略。
- 方案 B：coin Item 唯一权威，余额按扫描派生。影响是购买、找零、特殊货币和幂等记录难以
  保证跨容器一致。
- 方案 C：普通货币使用 Item、特殊货币使用逻辑余额。影响是两套结算不变量和 UI 语义。
- 当前不能裁决：Version4 特殊货币大部分为空体，dome 只有局部普通/特殊购买证据。

### BD-COMP-06：共享 ID、transaction 与账本 owner

- 冲突字段：runtime entity、persistent item/container、network、replication、external
  content、transaction、operation 和 reservation ID。
- 当前候选 owner：各能力组件只引用稳定值对象，跨域定义和映射归 integration-review。
- 方案 A：由整合层定义共享值对象，各子系统只保存相应字段。影响是需要统一持久化/网络
  映射，但可以避免整数复用。
- 方案 B：沿用各模块本地整数。影响是兼容成本低，但无法证明跨域关系和幂等身份一致。
- 方案 C：以 network/replication ID 作为运行时唯一身份。影响是重连、存档恢复和客户端
  预测会把外部身份泄漏进权威状态。
- 当前不能裁决：现有 Version4 与 dome 同时使用数组索引、Guid、网络/复制字段，未形成统一
  共享身份协议。

## 12. 最终 Component 清单

该清单是本次 Component-only 设计的 14 个目标组件。所有行的 `status` 都是
`proposed`，不是当前 NLTX 已有能力；带 `integration-review` 的行不能视为最终跨域 owner
裁决。

| `componentId` | name | status | componentOwner | crossSubsystemOwner | entityScope | lifecycle | proposedPath |
|---|---|---|---|---|---|---|---|
| `IC-01` | `ItemInstanceComponent` | `proposed` | ItemContainerAndEconomy candidate | `integration-review` | item entity | 创建、实例修饰、清空/加载 | `src/Items/Instances/ItemInstanceComponent.cs`（`status: proposed`） |
| `IC-02` | `ItemStackComponent` | `proposed` | ItemContainerAndEconomy candidate | `integration-review` | item entity | 创建、消费、合并、拆分、归空 | `src/Items/Instances/ItemStackComponent.cs`（`status: proposed`） |
| `IC-03` | `ContainerLayoutComponent` | `proposed` | ItemContainerAndEconomy candidate / PlayerGameplay交接 | `integration-review` | container entity | 创建、布局变化、清理 | `src/Items/Containers/ContainerLayoutComponent.cs`（`status: proposed`） |
| `IC-04` | `ContainerCapacityComponent` | `proposed` | container capability candidate | `integration-review` | container entity | 创建、加载、Resize、清理 | `src/Items/Containers/ContainerCapacityComponent.cs`（`status: proposed`） |
| `IC-05` | `ContainerContentsComponent` | `proposed` | ItemContainerAndEconomy candidate | `integration-review` | container entity | 创建、内容变化、清理 | `src/Items/Containers/ContainerContentsComponent.cs`（`status: proposed`） |
| `IC-06` | `ContainerAccessComponent` | `proposed` | WorldStorage / PlayerGameplay candidate | `integration-review` | Chest、银行或玩家容器 | 创建、访问/lease变化、失效 | `src/Items/Containers/ContainerAccessComponent.cs`（`status: proposed`） |
| `IC-07` | `EquipmentRelationComponent` | `proposed` | PlayerGameplay / ItemContainerAndEconomy candidate | `integration-review` | player/equipment host | 加载、装备关系变化、清理 | `src/Items/Equipment/EquipmentRelationComponent.cs`（`status: proposed`） |
| `IC-08` | `WorldItemStateComponent` | `proposed` | ItemContainerAndEconomy payload candidate | `integration-review` | world-drop entity | 生成、世界存在、拾取/合并/销毁 | `src/Items/WorldDrops/WorldItemStateComponent.cs`（`status: proposed`） |
| `IC-09` | `WorldItemReservationComponent` | `proposed` | SpawnLifecycleAndLoot / ItemContainerAndEconomy candidate | `integration-review` | world-drop entity | 建立、提交/释放、过期、清理 | `src/Items/WorldDrops/WorldItemReservationComponent.cs`（`status: proposed`） |
| `IC-10` | `CraftingStateComponent` | `proposed` | ItemContainerAndEconomy candidate | `integration-review` | player/crafting transaction entity | 请求、状态变化、结束/清理 | `src/Items/Crafting/CraftingStateComponent.cs`（`status: proposed`） |
| `IC-11` | `CraftingReservationComponent` | `proposed` | ItemContainerAndEconomy candidate | `integration-review` | crafting/container relation | 建立、提交、释放/过期 | `src/Items/Crafting/CraftingReservationComponent.cs`（`status: proposed`） |
| `IC-12` | `ShopInventoryComponent` | `proposed` | Commerce candidate | `integration-review` | shop entity | 内容加载、stock变化、清理 | `src/Items/Commerce/ShopInventoryComponent.cs`（`status: proposed`） |
| `IC-13` | `CurrencyBalanceComponent` | `proposed` | Commerce candidate | `integration-review` | account/player economy entity | 加载、结算更新、归档 | `src/Items/Commerce/CurrencyBalanceComponent.cs`（`status: proposed`） |
| `IC-14` | `CommerceLedgerComponent` | `proposed` | Commerce candidate | `integration-review` | account/shop ledger entity | 创建、追加记录、保留/归档 | `src/Items/Commerce/CommerceLedgerComponent.cs`（`status: proposed`） |

## 13. 最终声明

本文件是 Component-only Design。
本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 `status: proposed` 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。
