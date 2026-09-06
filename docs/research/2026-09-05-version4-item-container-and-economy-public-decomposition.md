# 17 — ItemContainerAndEconomy 独立只读审查与 ECS 设计报告

## 1. 报告元数据

| 项目 | 内容 |
| --- | --- |
| reviewId | version4-item-container-and-economy-public-decomposition |
| taskNumber | 17 |
| subsystemId | ItemContainerAndEconomy |
| layer | authoritative-simulation |
| currentNltxStatus | partial |
| verificationStatus | not-run |
| reportStatus | design-ready |
| generatedAt | 2026-09-05 |
| 输入文件 | D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-item-container-and-economy-public-decomposition.md |
| 报告性质 | 只读证据审查和 proposed ECS 边界设计 |

本报告只说明事实证据、状态所有权、依赖边界和未落地的 proposed 设计。没有创建或修改
.cs、.csproj、测试、组件、System、Query、Command、Adapter、Projection、迁移或运行时
文件，也没有宣称迁移完成、行为等价或 API 兼容。

## 2. 执行摘要

本轮结论是：ItemContainerAndEconomy 在 Version4 中是一个真实存在但未形成独立边界的责任面，
当前 NLTX 处于 partial。物品实例、堆叠、库存、装备、Chest、世界掉落、制作、商店、货币、
结果提交和网络/存档写入分散在 Item、Player、Chest、WorldItem、Recipe、CraftingRequests、
MessageBuffer、NetMessage 和 WorldFile 等不同责任面中。Version4 的调用链证明了这些对象之间
存在直接写入，而不是仅有数据引用。

最重要的设计判断如下：

1. ItemDefinition 与 ItemInstance 必须分离。Item.type、Item.stack、Item.prefix、Item.variant
   和实例扩展数据不能继续作为一个无边界的混合对象。
2. 堆叠数量只能有一个权威 owner。当前 ItemState.Stack 与 StackableItemComponent.Quantity/
   MaximumQuantity 重复表达数量，dome 的 ItemStack.Quantity 又形成第三个表示。
3. 容器内容只能有一个权威 owner。ContainerComponent.Contents 与
   ContainerContentsComponent.Slots 不能同时承担最终写入职责；Chest、Player 背包、银行和
   世界掉落必须通过统一的提交入口改变内容。
4. Reservation 不是扣除。制作、拾取和并发购买需要 reservation id、expected revision、
   expiration 和 commit/release 状态，不能用提前减少 stack 的方式模拟所有 reservation。
5. 货币余额与货币物品表示分离。金币 Item 仍可作为物品容器中的表示，但价格验证、扣款、找零、
   账本和退款需要一个权威 Commerce 提交路径。
6. Fishing、SpawnLifecycleAndLoot、PlayerGameplay、ContentCatalog、WorldStorage 和网络/
   存档层只提交请求、提供事实或输出投影；它们不能绕过 ItemContainerAndEconomy 直接修改
   inventory、Chest、bank、currency 或 item result。
7. 当前 dome 已有局部库存、世界掉落、商店购买和复制/持久化基础，但制作事务、跨容器原子
   提交、统一 reservation 生命周期和 Commerce ledger 仍未闭合，因此不能把当前目录或局部
   System 视为完整子系统。

本报告对于设计审查不阻断，标记为 reportStatus: design-ready；对于实际实现或迁移，
blocking-decision 为 implementation-blocked，必须先锁定单一权威状态和 focused verifier。

## 3. 子系统范围和不负责内容

### 3.1 负责范围

ItemContainerAndEconomy 的原始责任固定为：

- item instance；
- inventory 和 container contents 的事务；
- crafting；
- commerce，包括 shop offer、currency、purchase、sale、refund 和 ledger；
- item result commit，包括来自掉落、钓鱼、制作、购买或转换的结果进入目标容器。

装备关系在本报告中只覆盖“物品实例与装备槽的关系以及结果提交接口”；装备属性计算、玩家
能力、伤害和状态效果仍属于 PlayerGameplay、CombatAndStatus 或 integration-review。

### 3.2 不负责内容

下列对象和行为不在本报告中重新设计：

| 边界 | 本报告只接收或输出 | 本报告不拥有 |
| --- | --- | --- |
| ContentCatalog | ItemDefinition、RecipeDefinition、ShopOfferDefinition 的只读引用 | 内容注册表、配方规则的最终内容生产 |
| PlayerGameplay | 玩家、背包容器、装备槽的访问上下文 | 玩家移动、生命/魔力、输入状态和能力计算 |
| FishingAndCatchSimulation | item result request | 钓鱼资格、鱼获概率和钓鱼生命周期 |
| SpawnLifecycleAndLoot | LootSource 和 drop result request | NPC 掉落规则、随机、掉落源生命周期、世界掉落运动 |
| WorldStorage | Chest/TileEntity 的容器句柄和生命周期事件 | Chest 空间实体的创建、销毁、位置和世界结构 |
| CombatAndStatus | 消耗请求和使用结果 | 投射物、伤害、buff、状态机和战斗规则 |
| NetworkSessionAndSectionStreaming | 协议包和可见性上下文 | 客户端权威状态 |
| PersistenceAndRecovery | 快照读取/写入和版本迁移端口 | 运行时权威状态和事务裁决 |

固定的 19 个子系统清单不在本报告中改名、合并、拆分或扩展。跨边界 owner 未决的类型统一
标记为 crossSubsystemOwner: integration-review。

## 4. 证据优先级和来源角色

本报告使用以下证据优先级：

| 优先级 | 来源 | 角色 | 使用限制 |
| --- | --- | --- | --- |
| 1 | D:\TRbackup\Version4 | Version4 真实行为、写入根、调用链和生命周期 | 最高优先级；只对实际命中的私有实现下结论 |
| 2 | D:\TRbackup\无任何删减通过编译 | Version4 已存在文件的完整实现补证 | 标记 full-reference-supplemented；不能扩大 Version4 覆盖基线 |
| 3 | D:\TRbackup\tmodloader-api-docs-stable | tModLoader v2026.07 公开 API、网络、存档和扩展边界 | 不能替代 Version4 私有事务和调用顺序 |
| 4 | C:\Users\shan\Downloads\ECS\space-station-14-master | ECS 组件粒度、System/Query、事件/命令和投影组织参考 | 不能推断 Terraria 行为或复制领域语义 |
| 5 | D:\TRbackup\NLTX 当前源码及历史材料 | 当前实现映射和已有证据 | 仅用于判断 partial、重复状态和验证缺口 |

Version4、完整参考、tModLoader 和 SS14 目录均按只读方式检索。报告中的行号是本轮重新定位
后使用的证据锚点；外部 API 证据同时记录页面标题、版本和成员锚点。

## 5. Version4 事实证据

### 5.1 物品实例和使用路径

| 事实 | 证据 | 状态 | 读者/写者和副作用 |
| --- | --- | --- | --- |
| Item 混合保存 type、stack、maxStack、prefix、value、shopSpecialCurrency、shopCustomPrice 和大量装备/使用属性 | D:\TRbackup\Version4\Terraria\Item.cs:18-339；核心字段包括 type:122、stack:138、maxStack:140、value:248、shopSpecialCurrency:270、shopCustomPrice:272、prefix:286、active:317 | confirmed | 多个使用、装备、商店、网络和存档调用方读写；不是单一 ECS 组件的内聚概念 |
| Item.IsAir 由 type/stack 等实例状态派生 | D:\TRbackup\Version4\Terraria\Item.cs:354-363 | confirmed | 所有空物品判断都依赖该约定；迁移必须保持空槽与 inactive entity 语义 |
| Prefix 会同时改变统计、稀有度、价值和实例 prefix | D:\TRbackup\Version4\Terraria\Item.cs:375-498 | confirmed | Prefix 不是纯内容定义；它是实例状态变更并带有派生属性更新 |
| SetDefaults 重置实例并按 type/variant 填充内容属性和 stack | D:\TRbackup\Version4\Terraria\Item.cs:48120-48175；ResetStats 约:48390-48480 | confirmed | 与反序列化、生成、Refresh、TurnToAir 共同形成多个写入根 |
| NewItem 选择 Main.item 槽，清理旧槽，创建 WorldItem，设置 type/prefix/stack/位置/速度并广播 | D:\TRbackup\Version4\Terraria\Item.cs:48686-48797 | confirmed | 同时承担实例创建、世界掉落创建和网络副作用；应成为结果提交 Adapter 的被替换入口 |
| CanStack 只比较 type 和 prefix | D:\TRbackup\Version4\Terraria\Item.cs:48862-48878 | confirmed | 真实堆叠条件可能还受 ModItem、variant、uniqueStack 和容器规则影响，不能只复制此方法签名 |
| TurnToAir 清理 type、stack、prefix、dye、shoot 等字段 | D:\TRbackup\Version4\Terraria\Item.cs:48880-48899 | confirmed | 归零是实例清空，不应与 reservation release、entity deletion 或退款混为一谈 |
| Refresh 保存 stack/prefix/favorited 后重新 SetDefaults 和应用 prefix | D:\TRbackup\Version4\Terraria\Item.cs:48911-48925 | confirmed | 说明内容重建和实例状态恢复必须分层；直接复制 Item 会有状态丢失风险 |
| QuickMana 直接检查、应用资源效果、递减 item.stack，归零后 TurnToAir | D:\TRbackup\Version4\Terraria\Player.cs:3740-3771 | confirmed | 使用输入、资源效果和库存扣除混在一个 Player 路径中 |
| CountItem 和 ConsumeItem 遍历 inventory 并直接写 stack；ConsumeItem 可继续扣 bank4 | D:\TRbackup\Version4\Terraria\Player.cs:3986-4047 | confirmed | 没有统一跨容器原子事务；消费范围和提交顺序需要明确化 |
| ItemCheck 混合输入、可用性、魔力、药剂延迟、弹药、投射物、动画、表现和消耗 | D:\TRbackup\Version4\Terraria\Player.cs:23435-23590、25121-25454 | confirmed | PlayerGameplay 与 ItemContainerAndEconomy 之间必须改为请求/结果边界 |
| ApplyPotionDelay 与 ApplyLifeAndOrMana 在 Version4 为空体 | D:\TRbackup\Version4\Terraria\Player.cs:25163-25164 | confirmed | verification-gap；不能把完整参考实现写成 Version4 当前行为 |

### 5.2 容器、世界掉落和装备关系

| 事实 | 证据 | 状态 | 设计含义 |
| --- | --- | --- | --- |
| Player 直接持有 armor、dye、miscEquips、trashItem、inventory、bank、bank2、bank3、bank4 | D:\TRbackup\Version4\Terraria\Player.cs:1013-1095 | confirmed | 背包、装备、银行是多个容器域，不能以一个 PlayerInventoryComponent 重新聚合所有状态 |
| selectedItem 和 HeldItem 直接从 inventory 派生/访问 | D:\TRbackup\Version4\Terraria\Player.cs:2960-2962 | confirmed | 选中槽是关系或选择状态，不是 item definition |
| Chest 保存容量、item 数组、坐标、index、bankChest、name | D:\TRbackup\Version4\Terraria\Chest.cs:17-66 | confirmed | Chest 实体生命周期和容器内容是两个不同责任面 |
| Chest 创建、登记、删除、Resize、CreateBank、CreateShop 和空实例填充在同一类型中 | D:\TRbackup\Version4\Terraria\Chest.cs:68-150、459-557 | confirmed | WorldStorage 负责实体生命周期；ItemContainerAndEconomy 只负责内容提交 |
| Chest 销毁前检查内容是否为空 | D:\TRbackup\Version4\Terraria\Chest.cs:491-530 | confirmed | DestroyChest 需要读取容器快照，但不能直接绕过内容事务 |
| SetupShop 在 Version4 为空体 | D:\TRbackup\Version4\Terraria\Chest.cs:1222 | confirmed | 商店填充不能作为 Version4 当前已实现行为 |
| WorldItem 包含 inner Item、ownTime、reservation、noGrabDelay、instanced、ownIgnore、beingGrabbed、onConveyor 等状态 | D:\TRbackup\Version4\Terraria\WorldItem.cs:15-169 | confirmed | 世界掉落 payload、reservation、移动、寿命、拾取和表现混合 |
| 世界掉落合并直接写两个 Item.stack，归零时 TurnToAir 并发送网络消息 | D:\TRbackup\Version4\Terraria\WorldItem.cs:227-255 | confirmed | 合并必须变成提交命令；网络只能投影提交结果 |
| FindOwner 遍历玩家，调用 ItemSpace/CanPullItem，按距离和 hopper 设置 reservation | D:\TRbackup\Version4\Terraria\WorldItem.cs:257-348 | confirmed | reservation 不是 inventory 消费；所有权竞争需要显式状态机 |
| UpdateItem 同时更新寿命、重力、液体、闪耀、合并、敌人拾取阻止、位置、熔岩销毁和视觉效果 | D:\TRbackup\Version4\Terraria\WorldItem.cs:357-558 | confirmed | 移动/过期由 SpawnLifecycleAndLoot 或世界掉落系统负责，拾取结果交给 ItemCommitSystem |

### 5.3 配方、制作、商店和货币

| 事实 | 证据 | 状态 | 设计含义 |
| --- | --- | --- | --- |
| Recipe 保存 createItem、requiredItem、requiredTile、acceptedGroups、quick lookup、液体、生物群系、事件和拆解条件 | D:\TRbackup\Version4\Terraria\Recipe.cs:12-81 | confirmed | Recipe 是内容定义/资格输入，不是持续实体组件 |
| Recipe 构造时创建空 Item 实例 | D:\TRbackup\Version4\Terraria\Recipe.cs:90-97 | confirmed | 内容构造与实例构造耦合，迁移时必须用 definition ref 和 result request 分离 |
| RecipeGroup 保存 ValidItems、Items、DecraftItemId 和全局 recipeGroups，并提供 Add | D:\TRbackup\Version4\Terraria\RecipeGroup.cs:9-68 | confirmed | RecipeGroup 是静态互换规则，不是容器状态 |
| Version4 RemoteCraftRequest 包含 recipe、result、consumed、requested、quickCraft；网络 Deserialize 返回默认 false；存在 pending queue | D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:9-32 | confirmed | 当前 Version4 制作网络链未闭合；不能把完整参考实现当作当前事实 |
| 完整参考包含请求写入、服务端重新验证、局部消费、成功提交、失败退款和 pending 消费 | D:\TRbackup\无任何删减通过编译\Terraria.GameContent\CraftingRequests.cs:26-405 | full-reference-supplemented | 提供行为风险补证；暴露重复消费、超时未知、部分成功和退款时序风险 |
| Version4 CustomCurrencySystem 的计数、合并、付款、接受判断和价格接口为默认/空实现 | D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs:8-51 | confirmed | 当前货币执行链 partial，不能从类型存在推断交易闭合 |
| CustomCurrencyManager 只形成注册和识别基础，Version4 没有完整 BuyItem 结算路径 | D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencyManager.cs:14-45 | confirmed | Buy/Sell/Ledger 必须作为独立提交边界 |
| 完整参考提供货币计数、合并、付款、回滚缓存、接受判断、BuyItem 和价格输出 | D:\TRbackup\无任何删减通过编译\Terraria.GameContent.UI\CustomCurrencySystem.cs:8-296；CustomCurrencyManager.cs:8-164 | full-reference-supplemented | 只用于识别应测试的不变量和兼容行为 |
| ShopHelper.GetShoppingSettings 存在，ProcessMood 为空体；ItemShopSellbackHelper 只有 memo 数据 | D:\TRbackup\Version4\Terraria\ShopHelper.cs:49-66；ItemShopSellbackHelper.cs:5-24 | confirmed | 商店心情、出售和退款不是当前完整事务 |

### 5.4 掉落规则、网络和持久化

| 事实 | 证据 | 状态 | 设计含义 |
| --- | --- | --- | --- |
| CommonDrop 只声明 itemId、概率、数量范围和链式规则；TryDroppingItem 通过随机后调用 CommonCode.DropItemFromNPC | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\CommonDrop.cs:5-26、29-62 | confirmed | 掉落规则负责声明和计算结果，不应直接拥有库存内容 |
| ItemDropRule.Common、ByCondition、BossBag、NormalvsExpert 等构造规则对象 | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropRule.cs:3-145 | confirmed | 条件和选择属于 SpawnLifecycleAndLoot 输入，不是容器组件 |
| ItemDropDatabase 按 global 和 NPC net id 注册/索引规则，GetRulesForNPCID 合并规则列表 | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropDatabase.cs:7-105 | confirmed | 数据库是规则注册/查询；不是 item result commit |
| ItemDropResolver 取得 NPC 规则、逐条 ResolveRule、处理条件和 chained rules | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropResolver.cs:5-67 | confirmed | resolver 的输出应是 ItemResultCommitCommand 或世界掉落请求 |
| MessageBuffer 直接重建玩家/物品槽、WorldItem reservation/stack、Chest 内容，并调用 ConsumeItem/TurnToAir | D:\TRbackup\Version4\Terraria\MessageBuffer.cs:331-350、1090-1153、1493-1504、2540-2542、2648-2650 | confirmed | 网络接收器当前同时是解码器和权威状态写者，是迁移最高风险之一 |
| NetMessage 直接序列化普通物品、世界掉落 reservation、Chest、玩家物品和装备 | D:\TRbackup\Version4\Terraria\NetMessage.cs:201-212、643-676、866-888、1294、1470、1539、1626、1861-1868、2654-2670 | confirmed | 网络输出应改为 Projection；不能让序列化格式成为 authority |
| WorldFile 保存 Chest 坐标、名称、容量和每个 Item 的 stack/type/prefix，加载时重建 Item 并设置 stack/prefix | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1646-1718、2788-2840 | confirmed | 存档快照缺少新实例 ID、variant、reservation、ledger 和事务结果，需要版本化迁移 |
| WorldFile 的 LoadWorld/SaveWorld/Version2 生命周期明确 | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:658、878-957、1186-1194、1808-1826 | confirmed | Persistence Adapter 只能在快照边界读写，不能在运行时事务中直接改 Item |
| Main 初始化 CustomCurrencyManager、ItemDropSolver、ShopHelper、WorldItem 数组，并在更新链中调用 WorldItem | D:\TRbackup\Version4\Terraria\Main.cs:1021-1028、3336-3366、3431、3467-3468、11559-11569 | confirmed | 初始化和 tick 调度不能依赖文件顺序；需要显式 System 顺序 |

## 6. 完整参考补证

完整参考只补充 Version4 已命中的同名文件行为，不能改写 Version4 当前事实：

| 补证对象 | 读取内容 | 状态 | 允许用途 |
| --- | --- | --- | --- |
| Chest.SetupShop | 完整实现从 SetDefaults 填充商品，并按世界状态、玩家区域、事件和进度选择商品 | full-reference-supplemented | 设计 ShopInventory 和商品生成测试；不能称为 Version4 当前实现 |
| CraftingRequests | 远程请求、材料局部消费、服务端重新验证、成功提交、失败退款、pending 消费 | full-reference-supplemented | 设计 reservation、idempotency、rollback 和 timeout 测试 |
| CustomCurrencySystem/Manager | CountCurrency、CombineStacks、TryPurchasing、Accepts、BuyItem、GetPrices、回滚缓存 | full-reference-supplemented | 设计货币边界和 ledger；不能补齐 Version4 空实现 |
| 其他“无删减”文件 | 只用于核对 Version4 已存在类型的删减/空体 | existing-evidence | 不能扩大 ItemContainerAndEconomy 的 Version4 覆盖基线 |

完整参考揭示了一个不能隐藏的行为风险：远程制作可以先局部消费客户端材料，再由服务端
重新消费；如果请求超时、重复到达、服务端部分成功或退款失败，材料和结果可能不一致。
proposed 设计必须使用服务端 reservation、expected revision 和一次性 commit receipt，
不能复刻“双重消费后再退款”的隐式时序。

## 7. tModLoader 公开 API 交叉验证

### 7.1 检索记录

| 查询 | 来源和实际锚点 | 页面标题/版本 | 命中和消歧 | 结论 |
| --- | --- | --- | --- | --- |
| Item | D:\TRbackup\tmodloader-api-docs-stable\annotated.html:1059；类型页 D:\TRbackup\tmodloader-api-docs-stable\class_item.html | Item Class Reference；tModLoader v2026.07 | 由 annotated.html 的 href 精确取得 class_item.html | 公开 Item 同时暴露实例字段和生成/序列化相关方法，支持 definition/instance 分离判断 |
| Item.stack | D:\TRbackup\tmodloader-api-docs-stable\class_item.html#a87032523d0721bc622ae3d37452f78b8 | Item Class Reference；v2026.07 | 类型页内精确 stack 成员 | 当前 stack，maxStack 表示上限 |
| Item.maxStack | D:\TRbackup\tmodloader-api-docs-stable\class_item.html#a6e35aa31ca611fa855542433cc7adbc8 | Item Class Reference；v2026.07 | 类型页内精确 maxStack 成员 | 单个堆叠上限；默认 1 |
| Item.prefix | D:\TRbackup\tmodloader-api-docs-stable\class_item.html#ad19d05c738f67bd61c40e5b265d7f9a0 | Item Class Reference；v2026.07 | 类型页内精确 prefix 成员 | 当前实例前缀，默认 0，通过 Item.Prefix 设置 |
| Item.SetDefaults | D:\TRbackup\tmodloader-api-docs-stable\class_item.html#ac435cae572676b5ec8a8e26dac27ae11；另有重载 #aa1903fbd65a9a12fd6bd7e217ec00ae4 | Item Class Reference；v2026.07 | 以签名和重载区段消歧 | 从类型/variant 重建 Item 属性；不等于事务提交 |
| Item.NewItem | D:\TRbackup\tmodloader-api-docs-stable\class_item.html#a2a7567697940f3e104468c87b0559199、#a4a58d70dad8b5ad55610521b2b0d7924、#a9f40b070cfe44218b630d09346641cf8、#ace7df84ee9c39c1520cadd81a0fd86c2 | Item Class Reference；v2026.07 | 四个重载按参数签名消歧 | 公开世界物品生成入口，支持结果提交 Adapter 边界判断 |
| Chest | D:\TRbackup\tmodloader-api-docs-stable\annotated.html:366；类型页 D:\TRbackup\tmodloader-api-docs-stable\class_chest.html | Chest Class Reference；tModLoader v2026.07 | annotated.html href 精确命中 | Chest 表示非 Player inventory，包括 chest、portable storage 和 NPC shop |
| Chest lifecycle/shop | D:\TRbackup\tmodloader-api-docs-stable\class_chest.html#af43b708d6ba8a4550490ec034d49a7e2、#a44ce32eb9d46ee93f81fd89a04a1faed、#a1963f9c91e0b0381f0b362d540f93adf、#afb05eb9f1c36dc31ab70d7731222ab0f | Chest Class Reference；v2026.07 | 方法名和签名匹配 | 支持把注册/销毁/SetupShop 视作边界行为，而非单一 contents 组件 |
| Recipe | D:\TRbackup\tmodloader-api-docs-stable\annotated.html:1679-1682；类型页 D:\TRbackup\tmodloader-api-docs-stable\class_recipe.html:99-154 | Recipe Class Reference；tModLoader v2026.07 | 页面明确描述 Create、AddIngredient、AddTile、Register | Recipe 是 ingredients、tiles 和 result 的定义集合 |
| Recipe.AddIngredient/AddTile/Register/Create | D:\TRbackup\tmodloader-api-docs-stable\class_recipe.html#a32e3bff72441114d4cda94dd4cb05ea7、#a7e44a7659baf373b83f44c0fab3f5ac0、#adbbadf559d181a9ed34ce2082c6f8f9f、#a474a5953b0fbbdec4c68881277228d79 | Recipe Class Reference；v2026.07 | 实际成员锚点和签名匹配 | 只交叉验证内容注册、材料和站点边界 |
| ModItem | D:\TRbackup\tmodloader-api-docs-stable\annotated.html:1337-1338；类型页 D:\TRbackup\tmodloader-api-docs-stable\class_mod_item.html | ModItem Class Reference；tModLoader v2026.07 | 类型页和继承说明匹配 | 扩展点由 ModItem 提供，不应让外部扩展类型渗透核心 authority |
| ModItem.SetDefaults/CanStack/CanStackInWorld/ItemSpace/OnPickup | D:\TRbackup\tmodloader-api-docs-stable\class_mod_item.html#a10b52a61d301eca52d3b30c3c989d835、#abe74a706630de7e7c6f60dd5f6855f99、#a453b418e0a6eada1997713d3c2123ac6、#acfc46a2a397ebe4dc8b01308ec1d3126、#a6c3c5fc29f235f666e3a8fa350fc7272 | ModItem Class Reference；v2026.07 | 精确成员签名和说明匹配 | 公开堆叠/拾取钩子只能转换为策略输入或 adapter，不是客户端直接写入许可 |
| ModItem.LoadData/SaveData/NetReceive/NetSend | D:\TRbackup\tmodloader-api-docs-stable\class_mod_item.html#ac61c1bdd51166b263ebe536f5a9250a6、#ab4ee7330e00ebe6ca644570989a3e4b1、#a3d30fa657a2319543bdd619d2ad32f81、#a80e9898e70b923e0a2a9b0d643504e8e | ModItem Class Reference；v2026.07 | 精确成员签名和说明匹配 | 存档/网络扩展数据必须经 Adapter 进入实例投影 |
| GlobalItem.AddRecipes/LoadData/SaveData/NetReceive/NetSend | D:\TRbackup\tmodloader-api-docs-stable\class_global_item.html#a9ea8b1a34d268ff6a068ac2311f1c5cc、#a3c09aa7319828878f64e67b1049a5372、#a402e74c25445fdd28a55ec7a6969c258；无链接默认成员锚点 ac95971fb8c7195aa42bf476f12d0b4a3、a9a325b93998cbb5c268edf3961e6827f | GlobalItem Class Reference；v2026.07 | 由 Item 参数和方法区段消歧 | 全局扩展可参与注册/读写，但不能成为跨实体容器事务 owner |
| ModSystem.AddRecipes/LoadWorldData/SaveWorldData/NetReceive/NetSend | D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html#aed57ccad5d10597a1cd4f56ee3cdd226、#a12097aab73db65bd17b2bde505e1022d、#a926129e278ac9c460685bd2a326cd998、#a144ef598fa0b3bcc2a0ac6bd1c5467aa、#af9ebfea8b152b555b030265946cace70 | ModSystem Class Reference；v2026.07 | 方法区段和生命周期说明匹配 | 公开生命周期支持 Content/World/Network Adapter 分层 |
| ModPacket.Send | D:\TRbackup\tmodloader-api-docs-stable\annotated.html:1351；D:\TRbackup\tmodloader-api-docs-stable\class_mod_packet.html#a54393cf0ee63ef5c497a6850d4e8b688 | ModPacket Class Reference；v2026.07 | Send(int,int) 签名匹配 | 客户端调用发往服务器；服务端可广播、排除或定向，不能改变 authoritative commit 原则 |

### 7.2 交叉验证边界

tModLoader 公开文档确认了扩展、配方、Item/Chest、网络和存档的公开接口形状，但没有确认
Version4 私有 ItemCheck、CraftingRequests、Main.item、WorldItem 合并或 MessageBuffer 的完整
事务顺序。因此本报告中所有 Terraria 行为结论仍以 Version4 为准；公开 API 只用于确定
Adapter、注册和扩展的边界。

## 8. Space Station 14 最小相关 ECS 参考

SS14 只作为 ECS 组织参考。本轮实际读取的文件、类型和用途如下：

| 主题 | 实际读取文件和锚点 | 观察到的组织模式 | 仅可用于 |
| --- | --- | --- | --- |
| stack | C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Stacks\StackComponent.cs:7-38；SharedStackSystem.API.cs:104-145、234-265、298-331 | StackComponent 集中 type、Count、上限和有限的视觉状态；文档明确要求通过 SharedStackSystem.SetCount 写入；setter 负责 clamp、Dirty、事件和归零删除 | 支持“数量单一 owner + System 写入口” |
| inventory | Content.Shared\Inventory\InventoryComponent.cs:8-54；InventorySystem.Slots.cs:129-203；InventorySystem.Equip.cs:127-157 | InventoryComponent 保存模板、槽定义和 ContainerSlot；System 提供槽查询和装备资格/提交 | 支持把布局、槽位关系和装备操作分开 |
| item slot | Content.Shared\Containers\ItemSlot\ItemSlotsComponent.cs:10-40、53-114 | ItemSlotsComponent 只保存槽字典；ItemSlot 保存白名单、黑名单、锁、起始物品和表现配置；System 管理插入/弹出 | 支持 slot metadata 与 contents mutation 分离 |
| storage | Content.Shared\Storage\StorageComponent.cs:14-58、101-123；SharedStorageSystem.cs:1035-1185 | StorageComponent 保存网格、StoredItems、容量约束和访问配置；CanInsert 是资格判断，Insert 是写入路径 | 支持 Query/validation 与 System/commit 分离 |
| vending | Content.Shared\VendingMachines\Components\VendingMachineComponent.cs:7-46；VendingMachineInventoryEntry.cs:6-40；SharedVendingMachineSystem.cs:29-65、81-165；Content.Server\VendingMachines\VendingMachineSystem.cs:201-240 | 库存目录、库存类型、库存数量、网络状态、补货和出货由组件与 System 分开；服务器出货才生成实体并减少库存 | 支持商店库存与结果生成分开 |
| store | Content.Shared\Store\Components\StoreComponent.cs:9-102；SharedStoreSystem.cs:145-220 | StoreComponent 将 Balance、CurrencyWhitelist、目录、已购实体、已花费余额和退款策略声明为 store 状态；TryAddCurrency 负责验证后消费货币 | 支持 balance、offer、purchase tracking 和 refund state 的分离 |
| store purchase | Content.Server\Store\Systems\StoreSystem.Ui.cs:73-249、299-345；Content.Shared\Store\ListingPrototype.cs:125-219、365-378 | 服务器处理购买：确认 listing、条件、余额，扣款，生成结果/能力，记录购买和退款信息；同时存在 purchase event | 支持 server authoritative commit、receipt 和退款测试 |
| 网络/状态 | 上述组件的 NetworkedComponent、ComponentState 和 Store/Vending 状态投影 | 网络状态是组件快照/投影，系统负责修改权威状态 | 支持 Projection 不反写 authority |

本轮没有在 SS14 中找到与 Terraria Version4 的 ItemCheck、NPC drop rule 或旧式 Chest 存档格式
直接对应的行为证据；这些部分标记为无直接对应证据，继续以 Version4 为准。

## 9. 成员、字段、方法、读写者和生命周期盘点

下表按共同读写者、变更原因、生命周期和副作用分组，而不是按字段数量机械拆分。

| Version4 成员组 | 主要读者 | 主要写者 | 生命周期 | 状态类型 | 副作用 | 候选边界 | 状态 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Item.type/variant 和 SetDefaults | Player、Chest、WorldItem、Recipe、MessageBuffer、WorldFile | SetDefaults、netDefaults、Refresh、网络接收 | 创建、重建、加载 | content identity + derived instance data | 可能重置大量属性 | ItemDefinitionRef + ItemInstance | confirmed |
| Item.stack/maxStack | Player 消费、CanStack、WorldItem 合并、Chest/Net/WorldFile | ItemCheck、ConsumeItem、QuickMana、TryCombining、网络/存档恢复 | 创建、增减、归零 | authoritative quantity | 归零触发 TurnToAir、网络和删除 | ItemStackState + ItemCommitSystem | confirmed |
| Item.prefix 和 Prefix | 装备、堆叠、价值、网络、存档 | Prefix、Refresh、网络接收 | 创建、重铸、加载 | authoritative instance modifier | 修改统计和价格 | ItemInstance | confirmed |
| Item.value/shop 字段 | ShopHelper、货币、UI、Item value 查询 | SetDefaults、Prefix、shop setup、Mod hook | 内容初始化和商店生成 | definition-derived/offer snapshot | 影响价格和显示 | EconomyDefinition + CommerceOffer | partial |
| Player.inventory/bank/armor/dye | 物品使用、装备、QuickStack、网络、存档 | Player、Chest/MessageBuffer、CraftingRequests | 玩家加载到卸载 | authoritative container relation | 直接跨容器写入 | ContainerContents + EquipmentRelation | confirmed |
| Chest.item/maxItems/name/x/y | Chest lifecycle、UI、WorldFile、NetMessage | Create/Resize/Destroy、MessageBuffer、WorldFile | 世界初始化到保存 | container state + world storage metadata | 删除可能丢内容，网络可直接改内容 | WorldStorage boundary + ContainerContents | confirmed |
| WorldItem.inner/reservation/position | Main tick、玩家拾取、合并、网络 | NewItem、UpdateItem、FindOwner、MessageBuffer | spawn、update、pickup、despawn | payload + lifecycle + reservation | 运动、过期、网络、视觉 | WorldItem payload + integration-review lifecycle | confirmed |
| Recipe/RecipeGroup | Crafting eligibility、Guide、注册器 | 内容初始化和注册 | mod/content load | static definition | 读取内容和条件 | ContentCatalog definition | confirmed |
| RemoteCraftRequest/pending queue | 客户端、服务端、Net module | CraftingRequests | request、pending、commit/refund | transient transaction state | 网络、消费、退款 | CraftingTransaction | partial |
| CustomCurrencySystem/Manager | 商店 UI、购买、价格 | Include、BuyItem、currency hook | content load到交易 | definition + settlement | 消费、合并、回滚、显示 | CurrencyDefinition + CommerceLedger | partial |
| MessageBuffer/NetMessage item paths | Server/client state | packet decode/encode | 每次网络消息 | adapter + direct authority write | 直接改 Item/Chest/WorldItem | Network Adapter + Projection | confirmed |
| WorldFile Chest/Item paths | runtime state、world loader | save/load routines | world load/save | snapshot | 文件 I/O、版本兼容 | Persistence Adapter | confirmed |

### 9.1 访问模式结论

可观察到的访问模式可以归为六组：

1. definition readers：SetDefaults、Recipe、Shop、Prefix 分类和物品属性；
2. instance mutation：prefix、variant、favorited、name override、dye 和 stack；
3. container mutation：inventory、bank、armor、Chest、shop stock 和 world item；
4. transaction readers/writers：消费、制作、购买、出售、退款、合并、拾取；
5. external adapters：MessageBuffer、NetMessage、WorldFile、tModLoader hooks；
6. lifecycle/presentation：WorldItem movement、animation、UI、visual sync。

第 6 组不能回流到权威组件；第 5 组不能成为模拟层写者；第 1 组也不能携带第 2 组实例
状态。当前 Version4 将这六组混在大类中，是拆分的主要耦合来源。

## 10. 权威状态所有权表

| 状态 | 唯一建议 owner | 可写入口 | 只读使用者 | 当前风险 | owner 决策 |
| --- | --- | --- | --- | --- | --- |
| Item definition identity、stack limit、静态价格元数据 | ContentCatalog | definition registration/adapter | Item、Recipe、Shop、Loot | Item.SetDefaults 同时重建实例 | ContentCatalog |
| Item instance identity、prefix、variant、dye、favorited、name override | ItemContainerAndEconomy | ItemInstance command/commit | inventory、equipment、world item、network projection | ItemInstanceComponent 与 dome ItemInstanceStateComponent 分散 | ItemContainerAndEconomy |
| Item stack quantity | ItemContainerAndEconomy | ItemCommitSystem | merge、consume、pickup、craft、commerce | ItemState、StackableItemComponent、ItemStack 重复 | ItemContainerAndEconomy |
| Container layout/capacity | 容器所属能力 | container creation/configuration | access query、insert query | ContainerComponent 与 CapacityComponent 重复容量 | integration-review |
| Container contents | ItemContainerAndEconomy | ContainerTransfer/ItemCommitSystem | Player、Chest、WorldStorage、UI projection | Player/Chest/MessageBuffer 直接写数组 | ItemContainerAndEconomy |
| Container access/lease | WorldStorage 或 PlayerGameplay | ContainerAccessSystem | transfer/craft/shop eligibility | Chest open、坐标和访问混在网络路径 | integration-review |
| Equipment relation | PlayerGameplay 与 ItemContainerAndEconomy 交接 | EquipmentCommand + commit | stat systems、UI、network | armor/dye/miscEquip 直接位于 Player | integration-review |
| Craft reservation | ItemContainerAndEconomy | ReservationSystem | CraftingEligibilityQuery、CraftingCommitSystem | CraftingMaterialReservation 与 pending queue 分散 | ItemContainerAndEconomy |
| World pickup reservation | SpawnLifecycleAndLoot 与 ItemContainerAndEconomy 交接 | WorldItemReservationSystem | pickup eligibility、network | WorldItem reservation 与 dome ownership/reservation 重复 | integration-review |
| Shop offer/stock | Commerce | ShopOffer registration、stock commit | price query、UI projection | Chest.SetupShop、ShopInventory、ShopOffer 多套表示 | ItemContainerAndEconomy |
| Logical currency balance | Commerce | CommerceCommitSystem | price query、purchase/sale/refund | CustomCurrencySystem 空实现，dome 以 item stack 扫描余额 | ItemContainerAndEconomy |
| Currency item representation | ItemContainerAndEconomy | item commit / currency adapter | Commerce query | 金币实体与逻辑余额没有统一边界 | ItemContainerAndEconomy |
| Commerce ledger/receipt | Commerce | LedgerCommitSystem | refund、audit、network、persistence | CurrencyBalance/CommerceLedger 只有数据无执行链 | ItemContainerAndEconomy |
| Client inventory/chest/shop view | Network/Persistence Projection | snapshot emission | UI/client | MessageBuffer 可直接反写 authority | Network/Persistence |

### 10.1 ID 与关系模型

下列 ID 必须分开建模，不能用同一个整数或 Guid 兼任：

- runtime entity id：当前进程内 ECS 实体；
- persistent instance id：物品实例的稳定存档身份；
- container persistent id：Chest、bank 或其他持久容器身份；
- network id：协议同步身份；
- external content id：ItemDefinition、Recipe、Currency 或 Offer 的内容身份；
- replication id：世界掉落/客户端投影使用的同步身份；
- transaction id：Craft、Purchase、Sale、Pickup 或 ResultCommit 的幂等身份。

EntityReference 只能表示运行时关系或经过 Adapter 转换的引用，不能隐式代表持久化 ID、
网络 ID 或内容 ID。需要反向查找、占用组、reservation 扫描或内容归属时使用显式关系集合，
不要把所有 ID 堆进一个通用组件。

## 11. 当前 NLTX 映射

### 11.1 根 src/Items

当前根目录已有模型基础，但没有闭合的执行链，状态为 partial：

| 当前文件 | 已有内容 | 主要问题 |
| --- | --- | --- |
| src/Items/ItemDefinitionComponent.cs:3-21 | ContentId、DefinitionRevision 和兼容 Type | 与 ItemState.Type、dome ItemStack.ItemType 形成多套内容 ID |
| src/Items/ItemInstanceComponent.cs:5-31 | persistentInstanceId、prefix、variant、dye、favorited | 仅数据组件；未形成创建、变更、网络和存档提交链 |
| src/Items/ItemState.cs:3-6 | Type、Prefix、Stack、IsEmpty | 与 ItemDefinitionComponent、StackableItemComponent 重复 |
| src/Items/StackableItemComponent.cs:3-24 | Quantity、MaximumQuantity、StackKey、IsUnlimited | 与 ItemState.Stack 重复，且无统一 writer |
| src/Items/ContainerComponent.cs:5-15 | Capacity、Contents | 与 CapacityComponent、ContentsComponent 重复表示 |
| src/Items/ContainerCapacityComponent.cs:3-22 | Kind、SlotCount、MaximumWeight、nested | 未说明与 ContainerComponent.Capacity 的权威关系 |
| src/Items/ContainerContentsComponent.cs:6-42 | Slots、Revision、LastMutationTick | 有 revision 基础，但没有跨容器原子提交 |
| src/Items/ContainerAccessComponent.cs:5-32 | 坐标、锁、访问者、lease | 没有与所有容器操作统一集成 |
| src/Items/CraftingComponent.cs:5-34 | active recipe、requested、revision、sequence、reserved materials | reservation 只有数据，没有 reserve/commit/release/expire System |
| src/Items/ShopInventoryComponent.cs、ShopOffer.cs | offer、stock、price、currency | 没有交易 receipt、ledger、幂等和结果提交链 |
| src/Items/Commerce/* | CurrencyBalance、CommerceLedger 数据类型 | 没有统一权威结算执行链 |
| src/Items/WorldItemComponent.cs、WorldItemReservationComponent.cs | 位置、速度、寿命、reservation | 与 SpawnLifecycleAndLoot、dome ItemWorldState/ItemOwnership 重复 |

必须明确记录的重复状态：

1. ItemState.Stack 与 StackableItemComponent.Quantity/MaximumQuantity 重复表达堆叠；
2. ItemDefinitionComponent.Type、ItemState.Type、dome ItemStack.ItemType 形成多个内容 ID；
3. ContainerComponent.Capacity/Contents 与 ContainerCapacityComponent/ContainerContentsComponent
   重复表达容器权威状态；
4. CraftingMaterialReservation、WorldItemReservationComponent、dome ItemWorldStateComponent、
   dome ItemOwnershipComponent 分散表示 reservation/ownership；
5. 当前没有明确分离 runtime entity id、persistent instance id、network id 和 external id；
6. CurrencyBalanceComponent/CommerceLedgerComponent 只有数据组件，没有一致的结算执行链。

本轮不创建共享基础组件，不在 src/Items 下新增 generic Shared/Components/ 目录。

### 11.2 dome/src

dome 已有局部实现，但仍为 partial：

| 当前文件 | 证据 | 结论 |
| --- | --- | --- |
| dome/src/Terraria.Dome.Simulation/Items/ItemStack.cs:3-19 | ItemType、Quantity、Prefix、IsEmpty、WithQuantity | 有值对象基础，但不是全域唯一 stack owner |
| dome/src/Terraria.Dome.Simulation/Items/InventoryComponent.cs:6-130 | 40 槽、Revision、SelectedSlot、instance state、TryConsume、CanMerge | 单一 Inventory 内操作较完整，未覆盖 Player/Chest/world 的统一事务 |
| dome/src/Terraria.Dome.Simulation/Items/Systems/InventoryTransferSystem.cs:7-84 | TransferIntoSlot、definition/stack 校验和 SetSlot | 只实现向单个 slot 转移；不形成多容器原子提交 |
| dome/src/Terraria.Dome.Simulation/Items/Systems/InventoryCommandSystem.cs:6-163 | TryTransfer、TrySplit、TryMerge、revision 前置检查 | 主要是同一 InventoryComponent 内的命令 |
| dome/src/Terraria.Dome.Simulation/Items/WorldItemStore.cs:9-183 | Arch entity、ItemStackComponent、ItemInstanceState、ItemWorldState、ItemOwnership、输入校验 | 世界掉落基础存在，但与根 src/Items reservation/ownership 重复 |
| dome/src/Terraria.Dome.Simulation/Items/Systems/ShopPurchaseSystem.cs:31-400 | 普通/特殊货币购买、价格检查、revision、receipt | 商店单次购买有较强局部执行链，但没有统一 crafting/container commit 或持久 ledger |
| dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.Shop.cs:220-268 | queue、TryPurchase、PublishInventoryChanges、ApplyPurchaseReceipt | 有命令到 receipt 的路径，但只覆盖 Shop purchase |
| dome/src/Terraria.Dome.Simulation/Items/Systems/ItemPriceSystem.cs:5-84 | 金银铜铂换算、买卖价格、溢出检查 | 价格纯计算基础存在，未等于资金结算 |
| dome/src/Terraria.Dome.Server/Validation/ContainerInteractionValidator.cs:9-28 | 坐标、可见 section、距离检查 | 只验证打开容器，不验证 contents transaction |
| dome/src/Terraria.Dome.Server/Validation/ItemInteractionValidator.cs:24-42 | 玩家 active 和 selected slot 范围 | 只验证输入，不验证物品使用后的权威提交 |
| dome/src/Terraria.Dome.Protocol.V1456/Packets/Crafting* | CraftingRequest/RequiredItem/Response 数据包 | 有协议骨架；没有完整权威 crafting transaction lifecycle |

当前 Test/ 中没有 ItemContainerAndEconomy 专属 focused verifier。唯一相关命中
Test/Terraria.WorldInteraction.Components.Verification/Program.cs:7-48，只验证
StoredItemState 和 display doll/hat rack 的容量与空状态，不验证跨容器提交、制作、购买、退款、
reservation 或 network/persistence 投影。

## 12. Proposed ECS 拆分

以下所有类型、路径和签名均为 status: proposed，不代表当前文件已创建。

### 12.1 状态组件和值对象

| proposed 类型 | proposed 路径 | 唯一职责 | 必须拥有/不得拥有 | 状态 |
| --- | --- | --- | --- | --- |
| ItemDefinitionRef | src/Items/Definitions/ItemDefinitionRef.cs | 指向 ContentCatalog 的 external content id 和 definition revision | 只读内容引用；不得保存 quantity、reservation 或 runtime entity | proposed |
| ItemInstance | src/Items/Instances/ItemInstance.cs | persistent instance id、prefix、variant、dye、favorited、name override | 实例修改状态；不得保存 definition 的全部静态属性 | proposed |
| ItemStackState | src/Items/Instances/ItemStackState.cs | 一个实体的 quantity、stack limit ref、stack key 和 unlimited 语义 | quantity 唯一 owner；不得再由 ItemState.Stack 或容器副本写入 | proposed |
| InventoryLayout | src/Items/Containers/InventoryLayout.cs | 槽位顺序、热键/选择槽、装备槽定义 | 布局和关系元数据；不得拥有槽中物品 | proposed |
| ContainerCapacity | src/Items/Containers/ContainerCapacity.cs | 槽数、重量、嵌套策略和容量上限 | 容量规则；不得保存 contents | proposed |
| ContainerContents | src/Items/Containers/ContainerContents.cs | slot 到 item entity 的关系、revision、last mutation | contents 唯一 owner；不得拥有 item definition 或价格 | proposed |
| ContainerAccess | src/Items/Containers/ContainerAccess.cs | locked、access flags、current accessor、lease | 访问资格输入；不得直接消费物品 | proposed |
| EquipmentRelation | src/Items/Equipment/EquipmentRelation.cs | player 到 equipment slot 的 item entity 关系 | 只表达关系和 revision；stat 计算交给 PlayerGameplay/CombatAndStatus；crossSubsystemOwner: integration-review | proposed |
| WorldItemState | src/Items/WorldDrops/WorldItemState.cs | item payload 到世界掉落的绑定、spawn source、active/replication 状态 | 只保留交接所需世界状态；位置运动、过期、动画 owner 待 integration-review | proposed |
| WorldItemReservation | src/Items/WorldDrops/WorldItemReservation.cs | reserved player、reservation id、expires、ignore owner、pickup delay | reservation 状态；不得在 reserve 阶段减少 stack；crossSubsystemOwner: integration-review | proposed |
| CraftingState | src/Items/Crafting/CraftingState.cs | active transaction、recipe ref、requested quantity、expected revisions、phase | 事务状态；不得保存静态 RecipeDefinition 全量 | proposed |
| CraftingMaterialReservation | src/Items/Crafting/CraftingMaterialReservation.cs | transaction 到来源 container/slot/item/quantity 的 reservation 关系 | reservation 不等于扣除；必须可 release/expire | proposed |
| ShopInventory | src/Items/Commerce/ShopInventory.cs | offer ref、stock、shop revision、restock state | shop stock；不得拥有 buyer inventory | proposed |
| CommerceOffer | src/Items/Commerce/CommerceOffer.cs | price snapshot、currency kind、product result、buy once、offer revision | 交易输入快照；不得直接扣 currency | proposed |
| CurrencyBalance | src/Items/Commerce/CurrencyBalance.cs | account 的逻辑货币余额、currency revision、cap | logical balance；不得替代金币 Item 表示 | proposed |
| CommerceLedger | src/Items/Commerce/CommerceLedger.cs | transaction receipt、debit/credit、refund、status、idempotency key | 结算历史和不可重复提交依据；不得成为 UI cache | proposed |
| LootSource | src/Items/Results/LootSource.cs | 掉落来源、归属、随机结果的交接数据 | 只作为 cross-subsystem input；不得重新拥有掉落规则；crossSubsystemOwner: integration-review | proposed |

### 12.2 组件归并和旧状态处理

迁移后建议：

- ItemState 只保留兼容读取的过渡投影，不能继续与 ItemStackState 同时写 quantity；
- StackableItemComponent 的 Quantity/MaximumQuantity 必须停写，完成验证后删除或降为兼容
  Adapter 输入；
- ItemDefinitionComponent 统一改为 ItemDefinitionRef 的兼容映射，Type 只能作为 external
  content id 的旧别名；
- ContainerComponent 不再同时拥有 Capacity 和 Contents；若保留，只能是兼容 façade；
- WorldItemReservationComponent、dome ItemWorldStateComponent 和 ItemOwnershipComponent
  不能各自裁决 reservation；要么由 WorldItemReservation 统一，要么在 integration-review
  决定边界后保留显式的 lifecycle/ownership 分工；
- CurrencyBalance 和 CommerceLedger 必须由 Commerce System 写入，不能由 UI、packet decoder
  或 Item stack 扫描逻辑直接改写；
- 不创建 Shared/Components/ 作为跨域垃圾桶；共享值对象应归到其表达的 capability。

## 13. System、Query、Command、Adapter、Projection 边界

### 13.1 proposed 接口契约

| 模块 | proposed 接口/签名 | Implementation | Seam | Depth | Leverage | Locality | 状态 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Item definition registration | ItemDefinitionRegistry.Register(ItemDefinitionRef definition) | ContentCatalog Adapter | fake definition registry | medium | high | content load | proposed |
| Stack/merge query | bool EconomyQuery.CanMerge(ItemInstance source, ItemStackState sourceStack, ItemInstance target, ItemStackState targetStack, StackRules rules) | pure query | deterministic table cases | medium | high | item instance | proposed |
| Container eligibility | ContainerEligibilityQuery.Evaluate(ContainerAccess access, ContainerCapacity capacity, ContainerContents contents, ItemInstance item, ItemStackState stack) | pure query | no-write query fixture | medium | high | container | proposed |
| Item use/consume command | ItemConsumeCommand(entity, quantity, expectedRevision, operationId) | command ingress | malformed/duplicate command cases | shallow | high | input | proposed |
| Container transfer command | ContainerTransferCommand(sourceContainer, sourceSlot, targetContainer, targetSlot, quantity, expectedRevisions, operationId) | command ingress | fake commit port | deep | very-high | transaction | proposed |
| Reservation | ReservationSystem.TryReserve(ReservationRequest request, ReservationContext context) | authoritative System | fake clock and revisioned containers | deep | very-high | transaction | proposed |
| Item result commit | ItemCommitSystem.Commit(ItemResultCommitCommand command, ItemCommitContext context) | authoritative System | commit journal and receipt | deep | very-high | cross-container | proposed |
| Crafting eligibility | CraftingEligibilityQuery.Evaluate(CraftingRequest request, IReadOnlyList<ContainerSnapshot> sources, RecipeDefinition recipe) | pure query | recipe fixture and snapshot | medium | high | crafting read | proposed |
| Crafting commit | CraftingCommitSystem.Commit(CraftingTransaction transaction, CommitContext context) | authoritative System | reserve/commit/reject/release tests | deep | very-high | crafting | proposed |
| Commerce price | EconomyQuery.CalculatePrice(CommerceOffer offer, PriceContext context) | pure calculation | overflow/rounding fixture | medium | high | commerce read | proposed |
| Commerce commit | CommerceCommitSystem.Commit(CommerceTransaction transaction, CommerceContext context) | authoritative System | ledger and inventory fake ports | deep | very-high | commerce | proposed |
| Network input | NetworkItemAdapter.DecodeIntent(NetworkPacket packet) | protocol Adapter | packet codec fixture | medium | high | network boundary | proposed |
| Network output | InventoryProjection.Project(ContainerSnapshot snapshot, VisibilityContext visibility) | read-only Projection | golden snapshot | medium | high | network/UI | proposed |
| Persistence input/output | ItemPersistenceAdapter.Load/Save(ItemContainerSnapshot snapshot, PersistenceVersion version) | persistence Adapter | old/new fixture worlds | deep | very-high | persistence | proposed |

Query 只能读取快照、definition 和明确的 context，不能修改 component、revision、reservation、
ledger 或 entity。Projection 只能输出客户端/存档/审计视图，不能接受输出结果后反写模拟状态。
Adapter 负责外部 Item、Chest、MessageBuffer、WorldFile、tModLoader hook 和协议类型转换，
不能让外部类型渗透到核心事务接口。

### 13.2 Command 方向

允许的方向为：

external input -> Adapter -> Command -> validation/query -> reservation -> authoritative commit -> receipt/event -> Projection

不允许的方向为：

- packet decoder -> Item/Chest/Player 直接写字段；
- UI -> CurrencyBalance 或 ContainerContents 直接写字段；
- Loot resolver -> inventory/Chest 直接写字段；
- Query -> component、revision 或 ledger 写入；
- Projection -> simulation authority；
- refund -> 无 receipt 的盲目加回；
- reservation -> 直接减少 stack。

### 13.3 Item result commit

Loot、Fishing、Crafting、Shop、Extractinator 和转换逻辑均产生
ItemResultCommitCommand。该命令至少包含：

- operationId/transactionId；
- source kind 和 source id；
- target container id 或 world-drop target；
- ItemDefinitionRef；
- ItemInstance 变更或新实例策略；
- quantity；
- expected container revision；
- reservation/ownership token；
- failure policy；
- replay/idempotency key。

ItemCommitSystem 先验证 definition、数量、目标访问、容量、stack compatibility、expected
revision 和 token，再一次性修改 ContainerContents、ItemStackState 和必要的 ledger/receipt。
目标容器不能接受部分结果却没有明确的 remainder 或 reject 结果。

## 14. 调用方向和显式 System 顺序

以下顺序是 status: proposed 的局部顺序，不是最终全局调度裁决；跨子系统的最终 owner 和
顺序需要 integration-review。

1. ItemDefinitionRegistrationSystem：注册和校验 definition/recipe/offer/currency ref；
2. InputValidationSystem：校验 command、operationId、实体状态、范围和协议字段；
3. ContainerAccessSystem：校验 Chest/Player/bank 的访问 lease、可见性和权限；
4. EconomyQuery/CraftingEligibilityQuery：读取快照，计算资格、容量、价格和材料；
5. ReservationSystem：以 expected revision 建立 reservation，不改变可见 quantity；
6. ItemCommitSystem：提交跨容器 transfer、consume、pickup、merge 和 result；
7. CraftingCommitSystem 或 CommerceCommitSystem：提交制作或交易分支；
8. WorldItemCommitSystem：把世界掉落拾取、合并、销毁结果转换为统一 item commit；
9. LedgerCommitSystem：记录 debit/credit/refund/receipt/idempotency；
10. NetworkProjectionSystem/PersistenceProjectionSystem：输出 snapshot，不反写 authority。

必要的不变量：

- reservation 读取必须发生在 commit 前；
- 所有参与 commit 的 container revision 必须再次校验；
- ItemStackState 的 quantity 改变和 ContainerContents 的 slot 改变必须在同一 commit 边界；
- Commerce ledger 只有在实际 debit/credit 成功后确认 receipt；
- Projection 只能观察已提交 revision；
- 不依赖文件或目录顺序定义运行时顺序。

## 15. 制作、购买、出售、消费和回滚语义

### 15.1 消费成功/失败

消费命令先读取 source snapshot，验证数量、definition、使用资格、expected revision 和
operationId。成功时在一个 commit 中减少 ItemStackState，必要时清空空实例并更新
ContainerContents revision，然后发布 ItemConsumeReceipt。失败时不写 quantity，不调用
TurnToAir，不生成成功事件；如果此前已经建立 reservation，只执行 release/expire。

### 15.2 并发 reservation

同一 stack 的可用量应计算为：

available = committedQuantity - activeReservations

每个 reservation 保存 reservationId、transactionId、owner、quantity、source container、
source slot、expected revision、createdAt、expiresAt 和状态。相同 operationId 重放时返回
原 receipt；不同 transaction 不能超过可用量。commit 只接受仍有效且 revision 匹配的
reservation。reservation 过期释放占用，不产生隐式退款。

### 15.3 Crafting

制作流程为：

CraftingRequest -> eligibility snapshot -> reserve materials -> server commit -> consume reservation -> create result -> receipt

客户端可以提出请求或显示 pending projection，但不能把客户端局部扣除视作成功。服务端拒绝
时释放 reservation；服务端 commit 成功后才扣除材料并创建结果；结果容器不足时必须在验证
阶段拒绝，或明确把 remainder 作为独立的可恢复结果。不得复制完整参考中的重复消费后再
猜测退款时序。

### 15.4 Purchase/Sale

购买必须保存 price snapshot，而不是在扣款过程中重新读取可能已变化的价格。顺序为：

1. 验证 shop access、offer revision、stock、buyer container capacity 和 price；
2. 验证 logical balance 或 currency item representation；
3. 预留商品、资金和目标槽；
4. 一次 commit 完成 debit、stock decrement、item result commit；
5. 写入 CommerceLedger 和 receipt；
6. 输出 UI/network projection。

出售是反向流程，必须明确 item ownership、sell price snapshot、目标 currency、找零/堆叠
限制和 ledger credit。退款只能引用 receipt，且必须防止重复 refund。

### 15.5 金币边界

普通货币换算至少保持：

- copper = 1；
- silver = 100；
- gold = 10000；
- platinum = 1000000。

所有换算、堆叠拆分、找零、总价和卖价计算都必须先做非负校验和溢出校验。logical
CurrencyBalance 与 currency Item 的转换需要显式 adapter；不能因为扫描到 coin stack 就
直接覆盖逻辑余额。找零不足、目标槽不足、价格超出表示范围、货币 cap 超限和重复支付都
必须是可测试的拒绝结果。

### 15.6 回滚和幂等

事务失败时优先保证“未提交就无可见变化”。如果跨组件 commit 已开始，必须由同一
transaction journal 使用精确 inverse operation 回滚；禁止按当前数量盲目加回。每个 commit
返回 receipt，receipt 记录 operationId、before/after revision、debit/credit、result item
ids 和状态。重复 command 只能返回原结果，不能再次消费、发货或退款。

## 16. 持久化、网络和客户端 Projection

### 16.1 Persistence Adapter

Version4 的 Chest 存档只写 x、y、name、maxItems，以及每个槽的 stack/type/prefix。迁移
时应提供 status: proposed 的 Persistence Adapter，将旧字段解释为：

- type -> ItemDefinitionRef.externalContentId；
- stack -> ItemStackState.quantity；
- prefix -> ItemInstance.prefix；
- 槽位 -> ContainerContents；
- Chest x/y/name/capacity -> WorldStorage 的容器元数据；
- 缺失 persistent instance id -> 按 world/container/slot/legacy ordinal 生成并记录迁移诊断；
- 缺失 transaction/ledger -> 从旧快照开始新 revision，不伪造历史 receipt。

variant、dye、favorited、name override、reservation 和 ledger 不能从旧格式猜测；缺失时使用
明确的默认/unknown 状态并记录迁移结果。旧格式读入是兼容 Adapter，不能成为新的权威状态。

### 16.2 Network Adapter

MessageBuffer、NetMessage、CraftingRequests 和 tModLoader ModPacket 的外部包应拆成：

decode -> validate -> command 和 snapshot -> encode -> projection

服务端只在 command commit 成功后发出 inventory/chest/world-item/shop/ledger snapshot。
客户端收到 snapshot 时更新 Projection，不直接修改 authoritative ItemStack、Contents、
CurrencyBalance 或 CommerceLedger。Crafting response 必须包含 transactionId、accepted/
rejected、receipt 或明确失败原因；未知状态不能被客户端当作成功或自动退款。

### 16.3 客户端 Projection

InventoryProjection、ChestProjection、ShopProjection 和 CurrencyProjection 都是 status:
proposed 的只读输出模型。它们可以保存显示顺序、tooltip、价格文本、pending 状态和最近
receipt，但不能保存可被 commit 读取的权威 quantity。客户端预测如果保留，必须携带
predictionId 并在服务端 receipt/rollback 后替换，不能成为第二套 authority。

## 17. focused verifier 设计

当前没有本轮 focused verifier 实际结果，因此实际状态统一为 verificationStatus: not-run。
以下是实现前必须新增但本轮未创建的验证计划；所有名称均为 status: proposed。

| verifier | proposed 覆盖 | 关键断言 | 当前结果 |
| --- | --- | --- | --- |
| ItemStackInvariantVerifier | ItemDefinitionRef、ItemInstance、ItemStackState | quantity 非负、不超过 limit、空实例不带实例状态、prefix/variant 不影响 definition identity | not-run |
| ContainerTransactionVerifier | ContainerContents、跨 inventory/Chest/bank/world result | 单一 commit、expected revision、容量不足无部分写入、空槽归一化 | not-run |
| ReservationConcurrencyVerifier | CraftingMaterialReservation、WorldItemReservation | 同一 stack 不超卖、不同 transaction 竞争、expire/release、重复 operationId | not-run |
| CraftingTransactionVerifier | CraftingState、recipe eligibility、材料和结果 | 成功只扣一次、失败不扣、服务端 authority、结果不足 reject/remainder、timeout 未知 | not-run |
| CommerceSettlementVerifier | CommerceOffer、CurrencyBalance、CommerceLedger | price snapshot、普通/特殊货币、溢出/cap、debit/credit、receipt 幂等、退款一次 | not-run |
| ItemResultCommitVerifier | LootSource、fishing/craft/shop result | 所有 result 经过 ItemCommitSystem，目标容器不可用时明确 reject 或 remainder | not-run |
| NetworkProjectionVerifier | Network Adapter、inventory/chest/shop projection | 恶意客户端不能直接写 authority；快照 revision 单调；重复包不重复提交 | not-run |
| PersistenceMigrationVerifier | WorldFile old/new snapshot adapter | 旧 type/stack/prefix 可读取；新 ID 分离；坏数量、未知 type、缺字段可诊断 | not-run |

最小场景矩阵：

| 场景 | 初始状态 | 操作 | 预期 |
| --- | --- | --- | --- |
| 消费成功 | stack=5、revision=r | consume 2 | stack=3、revision=r+1、一个 receipt |
| 消费失败 | stack=1 | consume 2 | 无 quantity 变化、无 TurnToAir、明确 reject |
| 并发制作 | stack=5、两个 request 各 reserve 3 | reserve/commit | 只有一个可提交，剩余 2；另一个 release |
| reservation 过期 | active reservation | tick 超过 expiresAt | quantity 不变、占用释放、不可 commit |
| 跨容器 transfer | source 有 stack、target 有一空槽 | transfer | source/target/revision 同一 commit 更新 |
| 背包满的掉落 | target 满、loot 有结果 | ItemResultCommit | reject 或明确 remainder，不静默丢失 |
| 世界拾取竞争 | 同一 WorldItem、两个玩家 | 两个 pickup command | 只有一个 receipt，另一个 reject |
| 购买成功 | price snapshot、余额足够、目标有空间 | purchase | debit、stock、result、ledger 一致 |
| 购买失败 | 余额不足或 price revision 变更 | purchase | 无 debit、无 result、无 stock decrement |
| 找零溢出 | 大额 price/change | buy/sell | checked 拒绝或可表示的完整找零 |
| 重复退款 | 同一 receipt | refund 两次 | 只有一次 credit，第二次 idempotent/reject |
| 网络重放 | 同一 operationId | 重发 command | 返回原 receipt，不重复消费或发货 |
| 存档迁移 | 旧 Chest type/stack/prefix | load/save | 新快照可读，ID/revision 分离且诊断完整 |

## 18. 明确不拆分项

| 对象 | 不拆成一级子系统的原因 |
| --- | --- |
| 单个 Item 字段 | type、stack、prefix、value 等必须按共同访问和生命周期聚合到 definition、instance、stack 和 commerce 边界；逐字段拆分会制造无效状态和更多同步边 |
| 单个 Recipe | Recipe 是静态内容规则和资格输入；它不拥有持续变化的实体状态，不应成为运行时 System |
| 单个掉落规则 | CommonDrop 等是声明式策略；随机和条件属于 SpawnLifecycleAndLoot，输出才进入 ItemCommitSystem |
| 单个 Chest | Chest 是 WorldStorage 的一个容器实例；位置、生命周期、TileEntity 关系和 contents 不能把每个 Chest 变成新的子系统 |
| 单个金币实体 | 金币 Item 是货币表示，逻辑余额、价格、扣款和 ledger 是 Commerce 状态；单个 coin 不能拥有全局结算 |
| 单个商店 UI | UI 是 Network/Client Projection；它只能发 purchase intent 和显示 snapshot，不能拥有 offer、balance 或 commit |

## 19. 兼容策略和行为保持风险

### 19.1 分阶段兼容

建议的迁移顺序为：

1. 先建立 ItemDefinitionRef、ItemInstance 和 ItemStackState 的单一读写接口；
2. 将 Player.inventory、bank、armor、dye 和 Chest.item 适配为 ContainerContents；
3. 把 QuickMana、ConsumeItem、ItemCheck 消费分离为 command 和 commit；
4. 接入 WorldItem pickup/merge result commit；
5. 接入 Crafting reservation 和远程响应；
6. 接入 Commerce price、currency、ledger、purchase/sale/refund；
7. 最后切换 MessageBuffer、NetMessage、WorldFile 和客户端 projection。

旧 Item、Player、Chest 和 WorldItem 可在过渡期作为兼容 façade，但每个 façade 只能把写入
转发给一个 authority。禁止双写“旧字段 + 新组件”而没有 revision/一致性检查；兼容字段只
允许读或由 Adapter 单向同步。

### 19.2 行为保持风险

| 风险 | 证据/来源 | 影响 |
| --- | --- | --- |
| CanStack 仅比较 type/prefix，而 ModItem/variant/uniqueStack 可能改变资格 | Version4 Item.cs:48869-48878；tModLoader ModItem.CanStack/CanStackInWorld | 机械复制会导致错误合并或拒绝 |
| Item.IsAir、TurnToAir、SetDefaults 对空槽/归零/重建的语义不同 | Version4 Item.cs:354-363、48120-48175、48880-48899 | 空实体、空槽和删除时机可能改变 |
| Prefix 同时修改统计和价值 | Version4 Item.cs:375-498 | 只迁移 prefix 数字会漏掉派生属性刷新 |
| Player 消费路径跨 inventory 和 bank4 | Version4 Player.cs:3986-4047 | 单容器消费会改变可用材料和快捷使用行为 |
| WorldItem reservation、pickup、merge 和生命周期耦合 | Version4 WorldItem.cs:227-558 | 先提交拾取再更新生命周期可能产生重复或丢失 |
| CraftingRequests 完整参考和 Version4 空/默认实现不同 | Version4 CraftingRequests.cs:9-32；full reference:26-405 | 不能以补证行为直接替换当前行为 |
| CustomCurrency/SetupShop/ProcessMood 存在空体 | Version4 对应文件 | 商店和特殊货币行为应按 evidence-gap 处理 |
| 网络消息可直接写 authority | Version4 MessageBuffer.cs、NetMessage.cs | 恶意包、重复包和客户端预测会破坏一致性 |
| WorldFile 旧格式没有新实例和 ledger 信息 | Version4 WorldFile.cs:1646-1718、2788-2840 | 迁移不能伪造历史身份或交易记录 |
| 预期顺序可能由旧文件/数组访问隐式决定 | Version4 Main.cs、Player.cs、WorldItem.cs | ECS 迁移若不显式排序会改变结果 |

## 20. Evidence gap 与 blocking-decision

### 20.1 Evidence gap

| 缺口 | 当前状态 | 已查来源 | 后续要求 |
| --- | --- | --- | --- |
| Version4 ApplyPotionDelay/ApplyLifeAndOrMana 具体运行时行为 | missing in Version4 body；已有空体 | Version4 Player.cs、完整参考材料 | 由 PlayerGameplay/integration-review 单独裁定；本报告不补写 |
| Version4 CraftingRequests 网络 Deserialize 和完整事务闭合 | partial | Version4 CraftingRequests.cs:9-32；完整参考:26-405 | 实现前必须决定 server authority、pending、超时和退款协议 |
| Version4 CustomCurrency 完整付款/回滚 | partial | Version4 CustomCurrencySystem.cs:8-51；完整参考:8-296 | 实现前必须确认目标货币支持范围和溢出语义 |
| Version4 Chest.SetupShop 当前商品全集 | partial | Version4 Chest.cs:1222；完整参考 Chest.cs:1403+ | 只把完整参考作为补证；必须有内容目录输入 |
| 全量 inventory-like writers 的最终 owner | unresolved | Player、Chest、MessageBuffer、NetMessage、Crafting、dome 相关路径 | Integration Handoff 后锁定写入口并删除重复 writer |
| EquipmentRelation 与 WorldItem lifecycle 的跨子系统 owner | unresolved | Player、WorldItem、dome equipment/world state | 标记 crossSubsystemOwner: integration-review |
| 当前 NLTX ItemContainerAndEconomy focused verifier | missing | Test/Terraria.WorldInteraction.Components.Verification/Program.cs:7-48 | 实现前先建立第 17 节 verifier |
| 所有旧存档和网络版本的字段映射 | partial | WorldFile、NetMessage、dome V1456 compatibility | 需要版本化 fixture 和坏输入测试 |

### 20.2 Blocking-decision

| 决策 | 结论 |
| --- | --- |
| 是否阻断本次设计报告 | no-block-for-readonly-design；关键 Version4 权威状态、网络、存档和掉落交接已获得充分证据 |
| 是否允许直接开始迁移实现 | implementation-blocked；重复状态、无统一 commit、制作/货币空实现和 focused verifier 缺失尚未解决 |
| 是否允许把完整参考行为当作当前行为 | no；必须使用 full-reference-supplemented |
| 是否允许把 tModLoader/SS14 结论当作 Terraria 私有行为 | no；只能用于公开边界和 ECS 组织参考 |
| 是否允许确定跨子系统最终 owner | no；使用 crossSubsystemOwner: integration-review |
| 是否需要用户补充才能完成本报告 | no；实现前的 owner、协议和验证裁决交给 Integration Handoff |

## 21. Integration Handoff

### 21.1 交给 ContentCatalog

- 提供 ItemDefinitionRef、RecipeDefinition、RecipeGroup、CommerceOffer、CurrencyDefinition
  的稳定 external id 和 revision；
- 明确 definition stack limit、CanStack、uniqueStack、variant 和价格元数据；
- 不把运行时 quantity、reservation、ledger 放入静态内容。

### 21.2 交给 PlayerGameplay

- Player 只提供 actor、selected slot、访问上下文和能力/资源检查结果；
- QuickMana、ConsumeItem、ItemCheck 的库存写入改为 ItemConsumeCommand；
- armor、dye、miscEquips 的关系边界标记为 crossSubsystemOwner: integration-review；
- Player 不直接写 ItemStackState、ContainerContents 或 CurrencyBalance。

### 21.3 交给 FishingAndCatchSimulation

- 只提交带 source、owner、quantity、target policy 和 operationId 的 ItemResultCommitCommand；
- 鱼获概率、鱼竿状态和钓鱼生命周期不进入 ItemContainerAndEconomy；
- 结果进入背包、Chest 或世界掉落时都走同一个 commit 入口。

### 21.4 交给 SpawnLifecycleAndLoot

- ItemDropDatabase、ItemDropResolver、CommonDrop 保留为规则/解析责任；
- 解析结果转成 LootSource + ItemResultCommitCommand；
- 世界掉落的运动、过期、熔岩销毁和视觉更新由相邻责任面处理；
- 拾取、合并和 item payload 的最终内容提交必须得到 ItemContainerAndEconomy receipt。

### 21.5 交给 WorldStorage

- Chest 的坐标、TileEntity 绑定、创建、删除、Resize 和打开生命周期由 WorldStorage 提供；
- Chest contents 通过 ContainerContents handle 和 ContainerAccessContext 暴露；
- DestroyChest 只能读取 empty/evacuate 结果，不能直接清理 Item；
- bank/Chest 的持久化 id 和 world storage id 必须与 item persistent instance id 分离。

### 21.6 交给 NetworkSessionAndSectionStreaming

- MessageBuffer/NetMessage 只做 Decode Adapter 和 Encode Projection；
- 客户端包不得直接写 Player、Chest、WorldItem 或 Item stack；
- crafting、purchase、pickup、transfer 包必须有 operationId、expected revision 和明确结果；
- Projection 只发已提交 revision，服务端 receipt 是唯一成功依据。

### 21.7 交给 PersistenceAndRecovery

- WorldFile 旧 type/stack/prefix 通过迁移 Adapter 进入新 snapshot；
- 新快照版本化保存 instance id、container id、definition ref、quantity、prefix/variant、
  revision 和必要的 ledger/transaction 状态；
- 不为旧快照伪造未知 reservation 或历史 receipt；
- 坏数量、未知内容、重复 id 和超限容量必须产生诊断而非静默覆盖。

### 21.8 交给 CombatAndStatus

- Item use 只返回资源/消耗结果和 effect request；
- 投射物、伤害、buff 和状态效果不写 ContainerContents；
- 弹药消费通过 ItemConsumeCommand，不能由 projectile 或网络 handler 直接扣 stack。

### 21.9 固定交接摘要

```text
subsystemId: ItemContainerAndEconomy
taskNumber: 17
reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-item-container-and-economy-public-decomposition.md

evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run

confirmedOwners:
- ItemDefinition、ItemInstance、ItemStack、Container、Chest、WorldItem、Recipe、Commerce、货币、网络和存档边界的 Version4 事实已在本报告中分别定位；最终跨域 owner 仍交由整合会话裁决。

proposedTypes:
- proposed ItemDefinitionRef、ItemInstance、ItemStackState、ContainerContents、ContainerAccess
- proposed Reservation、CraftingState、CommerceOffer、CurrencyBalance、CommerceLedger
- proposed ItemConsumeCommand、ContainerTransferCommand、ItemCommitSystem、CraftingCommitSystem、CommerceCommitSystem
- proposed ItemPersistenceAdapter、InventoryProjection

sharedTypesForIntegrationReview:
- EntityId、PersistentEntityId、NetworkId、WorldSectionId、ItemResultCommitCommand、ContainerAccessContext、ItemContainerAndEconomyCommitPort

crossSubsystemReaders:
- PlayerGameplay、FishingAndCatchSimulation、SpawnLifecycleAndLoot、CombatAndStatus、WorldInteractionAndStructures

crossSubsystemWriters:
- ItemContainerAndEconomy 的 reservation、container、item-result 和 commerce commit；相邻系统只能提交受检 Command 或消费不可变 receipt/event。

evidenceGaps:
- 全量 inventory-like writers 的最终 owner、Version4 与当前 NLTX 的完整行为覆盖、旧存档迁移字段和网络包兼容矩阵仍需实现阶段验证。

blockingDecisions:
- 单一 quantity/container owner、reservation 与扣除的事务语义、货币余额与金币 Item 的边界，以及跨域 commit port 的最终 owner 尚未裁决。

verifierPlan:
- 建议新增 focused verifier 覆盖堆叠/合并、reservation、container transfer、crafting、commerce、loot result、网络/存档 round-trip 和重复提交幂等性；本轮不创建、不运行。
```

## 22. 本次验证状态和最终声明

本轮没有运行构建、恢复、测试、运行、发布、打包、MSBuild 或其他 compile-capable 命令，
也没有启动子代理。verificationStatus 统一为 not-run；历史材料若被引用，只能视为
existing-evidence，不能视为本轮验证结果。

报告结论仅为：

- Version4 事实证据已完成本子系统范围内的重新定位；
- 完整参考、tModLoader v2026.07 和 SS14 参考用途已明确隔离；
- 当前 NLTX 的 partial 状态、重复状态和执行链缺口已记录；
- proposed ECS 组件、System、Query、Command、Adapter、Projection、顺序和 verifier 计划
  已给出；
- 迁移实现仍被 implementation-blocked 条件约束；
- 本轮只新增本报告文件，未修改生产代码。

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。
