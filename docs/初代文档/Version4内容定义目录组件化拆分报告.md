# Version4 内容定义目录组件化拆分报告

## 1. 结论与范围

本报告把《[Version4 权威游戏模拟系统主要子系统](Version4权威游戏模拟系统主要子系统.md)》
中的“内容定义目录”落实为一组有明确状态所有权的目录、构建、查询和实例化边界。目标是让
`ContentCatalog` 成为**进程级、初始化后不可变的定义快照**，向模拟提供按 ID 的只读规则，
但不保存任何世界、玩家、NPC、投射物或物品实例状态。

本报告最初是只读源码研究；当前实现仅保留 `src/Content` 的不可变定义、目录、身份/派生/表现
索引与 Tile 临时覆盖状态组件，以及 Item/NPC/Projectile/Buff/Tile/Wall/Recipe/Drop/Fishing 的静态
数据记录。构建/校验入口、查询、实例初始化器、掉落解析和专用 verifier 均未保留。Version4 legacy
adapter、完整 Recipe/Fishing 端到端运行时仍未迁移。

当前静态目录组件还包括 `ItemToolDefinition`、`ItemPresentationDefinition`、
`NpcPresentationDefinition`、`BuffIdentityDefinition`/`BuffRuleDefinition`/`BuffPresentationDefinition`、
Tile 的 collision/lighting/framing/interaction/environment 记录以及 Projectile 的几何、行为、战斗、
穿透、能力和表现扩展字段。它们均为冻结定义的组成值，不承载实体的当前生命、位置、AI、寿命、
网络标记或表现帧计数器。

掉落、Recipe 与 Fishing 当前仅表达不可变规则和条件数据；资格判断、随机抽样、规则链执行和
世界实例写入均不在本次组件交付中。

请求给出的 `D:\TRbackup\Version4参考` 在本机不存在。实际发现并阅读的目标源码为
`D:\TRbackup\Version4`；以下所有 Version4 行为结论均只基于该目录。SS14 目录
`C:\Users\shan\Downloads\ECS\space-station-14-master` 仅用来学习“定义按领域分布、消费者经
原型查询读取、实体运行时状态另存”的组织模式，绝不复制其类型、命名或领域规则。

### 1.1 设计判定

1. `ContentCatalog` 不是 ECS 实体组件：其生命周期是进程内容加载到卸载，而不是某个实体的
   spawn/despawn；将它挂到实体会制造无意义的复制、持久化与网络身份问题。
2. 定义记录也不应暴露为现有 `Item`、`NPC` 或 `Projectile` 可变对象。`SetDefaults` 同时写入
   默认规则和实例复位数据，直接返回这些样本会使调用者能污染全局“定义”。
3. 每种内容类型采用紧凑整数 ID 的只读索引作为第一阶段内部实现，外侧只接触类型化查询。
   这是兼容 Version4 现有数组和固定 ID 协议的低风险路径；不应先把所有条目改造成对象图。
4. 内容加载可以有可变 builder；发布给模拟的 snapshot 不可以。动态内容或重载若需要支持，
   必须构造、校验并原子替换一份完整 snapshot，不能在 tick 中修改已发布目录。
5. “用定义初始化实例”是显式 System/Factory 的职责。它输出初始化命令或实例状态；目录只读，
   不拥有槽位、位置、生命、stack、AI、寿命、随机结果或网络同步位。

## 2. 证据范围与状态

### 2.1 读过的本地来源

| 来源 | 用途 | 关键证据 |
| --- | --- | --- |
| `docs/Version4权威游戏模拟系统主要子系统.md` | 确认系统边界：定义目录只读、运行时实例分离 | 34、71-74、94 行 |
| `D:\TRbackup\Version4\Terraria\Main.cs` | 内容初始化顺序、全局 ID 表、掉落/钓鱼数据库装载 | 1015-1021、3278、3324-3417、3507-3571、5209 起 |
| `D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs` | Item/NPC/Projectile 样本、持久 ID 映射及其重建 | 833-904、937-962 行 |
| `D:\TRbackup\Version4\Terraria.ID\{Item,NPC,Projectile,Buff,Tile,Wall}ID.cs`、`SetFactory.cs` | 按内容 ID 构造的类型 Sets、后置装配和紧凑表缓冲工厂 | `ItemID.cs:43-45,1398-1404`；`TileID.cs:8,141,425-427`；`SetFactory.cs:6,20-177` |
| `D:\TRbackup\Version4\Terraria\Item.cs` | 定义字段与实例字段混合、`SetDefaults` 的复位行为 | 122-303、317-339、48120-48172 行 |
| `D:\TRbackup\Version4\Terraria\NPC.cs` | `SetDefaults` 复位实例缓冲/状态；自然生成入口 | 185-253、8133-8204、5152-5197 行 |
| `D:\TRbackup\Version4\Terraria\Projectile.cs` | 默认值和 AI、免疫、网络、寿命等实例数据混合 | 90-238、444-540 行 |
| `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\*.cs` | 规则注册、只读按 NPC 查找和结果解析链 | `ItemDropDatabase.cs:7-160`、`ItemDropResolver.cs:5-66` |
| `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\*.cs` | 鱼类掉落规则集合、装载和校验 | `FishDropRuleList.cs:6-26`、`GameContentFishDropPopulator.cs:12-32` |
| `D:\TRbackup\Version4\Terraria\Recipe.cs`、`RecipeGroup.cs` | Recipe/Group 当前可见定义形状及未闭合的装载链 | `Recipe.cs:12-97`、`RecipeGroup.cs:9-68` |
| `D:\TRbackup\tmodloader-api-docs-stable\class_content_samples.html` | 交叉核对公开 API 对 `ContentSamples` 可变样本及可访问时机的说明；不用于推断 Version4 私有调用顺序 | `tModLoader v2026.07`，96-97、187-188 行明确样本仅作 reference、不得修改，且应在 `Mod.PostSetupContent` 或之后访问 |
| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared` | 仅确认原型定义和消费者查询的组织模式 | `StatusEffectPrototype.cs:6-9`；`Store/SharedStoreSystem.Listings.cs` 的 prototype-to-runtime clone 边界；`Prototypes/EntityPrototypeHelpers.cs:10-37` 的 `TryIndex` 只读查询 |

SS14 checkout 中未包含 `Robust.Shared.Prototypes` 的实现源码，因此本报告没有把未实际读取的
`IPrototypeManager` 装载/热重载细节作为证据。上述 SS14 引用只支持一般组织模式，不支持
Terraria 语义或本设计的字段归属。

### 2.2 证据状态规则

- `confirmed`：在 Version4 中同时找到声明/构建写者与至少一个真实读取或消费点。
- `partial`：找到了部分定义或消费者，但完整加载、生命周期或执行路径缺失。
- `missing`：未据此新建权威状态；本报告列入缺口而不猜测。

### 2.3 相关代码闭包及停止条件

“相关所有代码”在本报告中指能直接构建、保存、查询、初始化或临时改写内容定义的**可审计代码闭包**，而不是把整个 Terraria/SS14 checkout 都标记为内容目录。检索以 `ContentSamples`、`SetDefaults`、`SetFactory`、`*.Sets`、`ItemDropDatabase`、`FishDropsDB`、`RecipeGroup`、`tileSolid` 和 `PostSetupContent` 为种子；命中后再沿初始化和消费者边界阅读。

| 闭包 | 已读 Version4 源码 | 读/写关系及报告处理 |
| --- | --- | --- |
| 启动与发布根 | `Terraria/Main.cs:3324-3417` | 构造/发布 Item drop、Fish drop、投射物 capability 与 `ContentSamples` 后处理；`Recipe.SetupRecipeGroups/SetupRecipes` 在此版本被注释，因此 Recipe 只为 `partial`。 |
| 类型表与身份样本 | `Terraria.ID/{Item,NPC,Projectile,Buff,Tile,Wall}ID.cs`、`SetFactory.cs`、`ContentSamples.cs:833-962` | ID Sets 为 builder 输入；`ContentSamples.Initialize` 创建可变的 `Item`/`NPC`/`Projectile` 样本和持久 ID 映射，之后的 Refresh/排序重建证明不能把原对象暴露为冻结定义。 |
| 定义-实例混合点 | `Terraria/Item.cs`、`NPC.cs`、`Projectile.cs` 的字段与 `SetDefaults` 路径 | 同一初始化入口同时设置类型默认值和实例复位状态；由受控 legacy adapter 提取 definition，`DefinitionTo*Initializer` 负责新实例，不能把该入口直接公开为 Catalog API。 |
| Tile/Wall/Buff/投射物消费者 | `Terraria/Main.cs`、`Collision.cs`、`Liquid.cs`、`WorldGen.cs`、`Framing.cs`、`Wiring.cs`、`Chest.cs`、`TileObject.cs` | 这些消费者读取 `Main` 类型表；`WorldGen.cs` 多处暂时写 `Main.tileSolid`，故基础 Tile definition 与世界/阶段 override 必须拆开。这里没有把每个调用点当成新的目录成员。 |
| NPC 掉落规则图 | `Terraria.GameContent.ItemDropRules` 下全部 38 个 `.cs`，特别是 `ItemDropDatabase.cs:7-165`、`ItemDropResolver.cs:5-66` | 数据库在启动期注册规则，resolver 先按 `npc.netID` 读规则，再判断条件、抽样并解析链；规则图保持一个不可变目录项，资格、随机和实例生成另拆为 Query/System/Command。 |
| 钓鱼与配方 | `Terraria.GameContent.FishDropRules` 下全部 9 个 `.cs`；`Terraria/Recipe.cs`、`RecipeGroup.cs` | 已确认钓鱼规则装载和 Recipe/Group 数据形状；未闭合到最终产物提交的运行时链路，所以两者不进入 `confirmed` 迁移切片。 |

SS14 只读对照的闭包限于 `Content.Shared/StatusEffect/StatusEffectPrototype.cs`、`Content.Shared/Prototypes/EntityPrototypeHelpers.cs` 和 Store 的 prototype、component、system、UI 与 localisation consumer 文件。它们显示 `IPrototype` 定义、`ProtoId<T>` 引用和 `IPrototypeManager.TryIndex/Index` 消费边界；不包含 `Robust.Shared.Prototypes` 的实现，故不推断加载、验证、缓存或热重载行为。

停止条件是：某个文件若仅因使用内容 ID 而未构建、保存、查询或改写上述定义/规则，则按其所属 gameplay、network 或 presentation 领域保留；它不是 `ContentCatalog` 的成员。对于仍缺完整加载或执行证据的 Recipe 和 fishing，维持 `partial`，不以名称补全字段或流程。

## 3. 现有成员盘点与归属

下表按共同读写者、生命周期和状态语义划分，而不是按照旧类文件或字段数量拆分。

| 现有成员或路径 | 当前读者/写者与生命周期 | 状态类型 | 候选归属 | 状态 |
| --- | --- | --- | --- | --- |
| `Main.tileSolid`、`tileSolidTop`、`tileFrameImportant`、`tileMerge`、`tileLighted`、`tileContainer`、`tileSign` | `Initialize_TileAndNPCData1/2` 写；`Collision`、`Liquid`、世界交互和内容辅助逻辑读；进程内容初始化后长期存在，但 `Liquid` 会临时改动 `tileSolid` | 类型规则；现有表中混入临时 override | `TileDefinitionCatalog`，另建 `TileSolidityOverrideState` 给液体/生成阶段 | confirmed，override 分离为 partial |
| `Main.wallHouse`、`wallDungeon`、`wallLight`、`wallBlend` | Tile/房屋/生成类查询；初始化时写 | Wall 类型规则 | `WallDefinitionCatalog` | confirmed |
| `Main.projFrames`、`projHostile`、`projHook` 及 `ProjectileID.Sets` | 初始化以 `Projectile.SetDefaults` 和 ID Sets 填充；投射物行为和表现读取 | 投射物类型规则/派生索引 | `ProjectileDefinitionCatalog` + `ProjectileCapabilityIndex` | confirmed |
| `Main.pvpBuff`、`persistentBuff`、`debuff`、`buffNoSave` 等 | 内容初始化后由 Buff、战斗、存档路径读取 | Buff 类型规则 | `BuffDefinitionCatalog` | confirmed |
| `ItemID/NPCID/ProjectileID/BuffID/TileID/WallID.Sets` 与 `SetFactory` | 静态 ID Set 在内容初始化/`PostSetupContent` 中装配；定义构建和规则查询读取 | 内容类型标签与紧凑构建缓冲 | 各类型 DefinitionCatalog 的 builder 输入；不将 `Sets` 作为对外可写 API | confirmed |
| `ContentSamples.ItemsByType`，其 Item 样本；Item 持久 ID 双向字典 | `ContentSamples.Initialize` 写；`Item.Original*`、`ItemID`、whip 逻辑等读 | 定义样本与内容身份索引；样本自身可变 | `ItemDefinitionCatalog`、`ContentIdentityCatalog`；不暴露 Item | confirmed |
| `ContentSamples.NpcsByNetId`，NPC 持久 ID、Bestiary ID/星级索引 | `Initialize` 写；drop database、城镇、Bestiary、NPC 转换读取 | NPC 定义样本、网络/持久 ID 映射、派生表现索引 | `NpcDefinitionCatalog`、`ContentIdentityCatalog`、`BestiaryDefinitionProjection` | confirmed；Bestiary 是 projection |
| `ContentSamples.ProjectilesByType` | `Initialize` 写；投射物规则消费者读 | 投射物定义样本 | `ProjectileDefinitionCatalog` | confirmed |
| `Item.SetDefaults` / `NPC.SetDefaults` / `Projectile.SetDefaults` | 样本构建和每个新实例初始化调用；会复位 stack、AI、免疫、位置、寿命、网络标志、轨迹缓冲等 | 定义提取入口和实例初始化混合 | `CatalogBuildSystem` 读取结果；`DefinitionToInstanceInitializer` 写实例 | confirmed，禁止直接转发 |
| `Item.type/use*/damage/defense/maxStack/...` | Default 初始化写；使用、装备、战斗、库存读取；`stack`、`prefix` 等仍会按实例变化 | 定义与实例混合 | `ItemDefinition`；`ItemStackState`/`ItemInstanceState` 留在 ItemGameplay | confirmed |
| `NPC` 默认 life/damage/defense/aiStyle/frame/catchability 与 life、AI、buff、位置、网络字段 | 同一个 `SetDefaults` 写入；生成/AI/战斗/网络读写 | 定义与实例混合 | `NpcDefinition`；运行时留 `Npc*State` | confirmed |
| `Projectile` 默认尺寸、enemy/friendly、aiStyle、能力标签与 `owner`、`timeLeft`、`ai/localAI`、immunity、identity、netUpdate | `SetDefaults` 和运行时行为/网络都写 | 定义与实例混合 | `ProjectileDefinition`；运行时留 `ProjectileBehavior/Combat/Network` 状态 | confirmed |
| `ItemDropDatabase` 的全局/NPC rule lists 和 `Populate` | 程序启动期注册；`ItemDropResolver.TryDropping` 获取规则、再解析 rule chain | 不可变规则图；当前 API 在加载后仍公开 Register/Remove | `DropRuleCatalog` + 仅加载阶段可见的 builder | confirmed；freeze 边界为 partial |
| `FishDropRuleList._rules` 与 `GameContentFishDropPopulator.Populate` | 启动期 `Add` + chance 校验；未在已读范围找到完整 runtime resolver | 钓鱼规则集合 | `FishingDropRuleCatalog` | partial |
| `Recipe.createItem/requiredItem/requiredTile/conditions`、`RecipeGroup.recipeGroups` | 结构可读；初始化调用 `SetupRecipeGroups/SetupRecipes` 在 `Main.cs:3401,3414` 被注释，`Recipe.cs` 大片为空实现 | 配方定义；链路未完整加载 | `RecipeDefinitionCatalog`、`RecipeGroupCatalog` | partial |
| Bestiary 排序、创意排序、染料 shader ID、手册分组 | 内容样本和定义产生的 UI/展示索引 | 派生表现/投影，不是玩法规则根 | `ContentPresentationIndex`，由投影构建 | confirmed，排除权威玩法目录 |
| `Main.item/npc/projectile/player`、Tile map、Chest、world item | 世界或实体槽位随世界/实体生命周期变动 | 权威运行时实例/世界存储 | `WorldStorage`、领域实例状态 | confirmed exclusion |

### 3.1 关键不变量

- **定义不含可变实例引用。** 不允许 `ItemDefinition` 持有可修改的 `Item`；可变数组、集合和
  引用型规则树需要在 build 阶段深拷贝或冻结为只读结构。
- **实例不回写定义。** `stack`、`prefix`、`life`、`ai`、`localAI`、`timeLeft`、`owner`、位置、
  免疫计时和任何网络标记都不能通过引用别名写回 catalog。
- **身份概念分开。** Type ID、NPC net ID、持久 string ID、实体槽位和实体身份不可互换。Catalog
  只负责内容 type/net/persistent 的映射；`EntitySlotStore<T>` 仍负责实例槽位。
- **世界临时 override 不能改 definition。** 现有 `Liquid` 与 update 路径会改 `Main.tileSolid`。
  迁移后应由 `TileSolidityOverrideState` 和只读 `EffectiveTileCollisionQuery` 覆盖基础 Tile 定义，
  并在阶段结束撤销，而不能写入 `TileDefinitionCatalog`。

## 4. 目标模块拆分

目录示意遵循“领域优先、小目录扁平化”。这些是建议的目标文件归属，而不是本次创建的文件：

```text
src/
  Content/
    ContentCatalog.cs
    ContentCatalogSnapshot.cs
    ContentCatalogBuildSystem.cs
    ContentCatalogValidationQuery.cs
    ContentIdentityCatalog.cs
    ContentIdentityQuery.cs
    ItemDefinitionCatalog.cs
    ItemDefinition.cs
    NpcDefinitionCatalog.cs
    NpcDefinition.cs
    ProjectileDefinitionCatalog.cs
    ProjectileDefinition.cs
    BuffDefinitionCatalog.cs
    BuffDefinition.cs
    TileDefinitionCatalog.cs
    TileDefinition.cs
    WallDefinitionCatalog.cs
    WallDefinition.cs
    RecipeDefinitionCatalog.cs
    RecipeDefinition.cs
    RecipeGroupCatalog.cs
    DropRuleCatalog.cs
    FishingDropRuleCatalog.cs
    ContentDerivedIndexCatalog.cs
  ItemGameplay/
    DefinitionToItemInstanceInitializer.cs
  Npc/
    DefinitionToNpcInstanceInitializer.cs
  Projectile/
    DefinitionToProjectileInstanceInitializer.cs
  Loot/
    DropEligibilityQuery.cs
    DropResolutionSystem.cs
  Physics/
    EffectiveTileCollisionQuery.cs
  WorldGeneration/
    TileSolidityOverrideState.cs
```

只有 `Content/` 里确实出现足够多的稳定定义类型时才保留该领域目录；初始迁移不预建
`Components/`、`Systems/`、`Queries/` 等纯技术子目录。每个公开核心类型各占同名 PascalCase
文件。`DefinitionTo*Initializer` 按它写入的实体能力放到 Item/Npc/Projectile 领域，不把所有
跨域行为回收进 catalog。

### 4.1 `ContentCatalogSnapshot` 与根门面

**拥有的权威状态**：一次成功内容加载产生的不可变 `ContentCatalogSnapshot`，包含各类型
catalog、内容身份映射和可由定义完全重算的派生索引。

**不拥有**：当前世界、内容包下载、文件 IO、实体槽位、玩家存档、网络包、随机源和任意
`Item/NPC/Projectile` 运行时对象。

最小接口：

```csharp
public interface IContentCatalog
{
  IContentIdentityQuery Identities { get; }
  IItemDefinitionQuery Items { get; }
  INpcDefinitionQuery Npcs { get; }
  IProjectileDefinitionQuery Projectiles { get; }
  ITileDefinitionQuery Tiles { get; }
  IWallDefinitionQuery Walls { get; }
  IBuffDefinitionQuery Buffs { get; }
  IRecipeAndDropQuery RecipesAndDrops { get; }
}
```

`ContentCatalog` 的唯一替换 seam 是 `IContentCatalog`；模拟系统依赖具体、窄的 `I*DefinitionQuery`
而不是整个根对象。根门面深度高、复用率高，但调用局部性通过子查询维持。

### 4.2 类型定义目录

以下目录都将 ID 映射到只读 definition record。可选择数组作紧凑 ID 范围的内部存储、字典作负
net ID 或稀疏 ID 的内部存储，但不得把集合直接以可变类型公开。

| 模块 | 权威定义 | 主要消费者 | 明确排除 |
| --- | --- | --- | --- |
| `ItemDefinitionCatalog` | use style/time、伤害、defense、工具强度、place/shoot、弹药、装备槽、max stack、默认 buff、价格和类型能力 | Item use、装备、制作、投射物产生、商店资格 | stack、prefix、收藏、当前持有人、实例名称覆盖、使用计时 |
| `NpcDefinitionCatalog` | 默认生命/伤害/防御、尺寸、frame、AI style、捕获/城镇/敌对等能力与 type/net/persistent ID | Spawn、AI 初始化、战斗、Bestiary 投影、drop rule lookup | 当前 life、target、AI 进度、buff、位置、spawn sync、实体 slot |
| `ProjectileDefinitionCatalog` | 默认几何、friendly/hostile、AI style、默认寿命、穿透、碰撞、frame、pet/hook/minion 等能力 | Projectile spawn、behavior、combat qualification、表现投影 | owner、identity、timeLeft、ai/localAI、local immunity、netUpdate、轨迹历史 |
| `BuffDefinitionCatalog` | debuff、PVP、持久化、免疫、显示及保存规则 | Buff application、combat、persistence eligibility | 实体当前 buff 时间、免疫倒计时 |
| `TileDefinitionCatalog` | solid/top、frame-important、light、merge、container/sign、tool、platform/attachment 和基础碰撞能力 | Collision、Liquid、world generation、tile placement/framing | 某坐标 Tile、液体量、frame、临时 solid override、section 状态 |
| `WallDefinitionCatalog` | house/dungeon/light/blend 等 Wall 规则 | Housing、lighting、world generation | 某世界坐标的 wall 实例 |

定义 record 必须表达“与 type 共同变更”的属性。一个只被网络、渲染或本帧效果读取的字段应
建立 projection，而不是为了字段完整度塞入玩法定义。

### 4.3 内容身份与派生索引

`ContentIdentityCatalog` 是独立模块，提供 `TryGetItemType(persistentId, out itemType)`、
`TryGetNpcNetId(persistentId, out netId)` 和逆向查询。它不把 `NPC.netID`、`whoAmI` 或实体 ID
压缩成单一 ID。其构建依据是 `ContentSamples.Initialize` 中的 Item/NPC 双向字典。

`ContentDerivedIndexCatalog` 只收纳可由已冻结定义重建的索引，例如投射物 hostile/hook 能力、
物品装备槽反查、NPC 按 type 的 net ID 变体、创意排序/Bestiary 排序所需的中间数据。它的读者
若是 UI、shader 或 Bestiary，应拆入 `ContentPresentationIndex` projection，不能使表现排序成为
权威玩法状态。

### 4.4 配方、掉落和钓鱼规则

`RecipeDefinitionCatalog` 与 `RecipeGroupCatalog` 负责配方输入、工作站、环境条件、结果和 item
group 的冻结定义；`CraftEligibilityQuery` 根据库存快照、世界规则和工作站条件判断资格；
`CraftResolutionSystem` 才可发出扣料和生成命令。由于目前 Version4 在启动函数中注释了
`Recipe.SetupRecipeGroups` 与 `Recipe.SetupRecipes`，并且 `Recipe.cs` 未提供完整实现，本模块
不可投入权威运行时，状态为 `partial`。

`DropRuleCatalog` 负责按 NPC net ID 和全局规则取回不可变规则树，覆盖当前
`ItemDropDatabase.GetRulesForNPCID` 的读取职责。`DropEligibilityQuery` 必须把 world rule、NPC
快照、击杀来源、难度和其他条件作为显式输入并只返回资格；`DropResolutionSystem` 取得显式
`IRandomSource` 后选择结果，并向 `WorldStorage` 追加 `SpawnItemCommand`。不能让规则树、随机数
或 resolver 直接成为 WorldItem 的所有者。

`FishingDropRuleCatalog` 只保存通过 `ChanceDenominator > 0` 校验的规则。其对应的资格/随机/产物
提交同样要独立于 catalog；完整 Version4 运行时解析器未在本次已读范围闭合，因此应先以
`partial` 防止迁移时丢失钓鱼语义。

### 4.5 构建、校验与冻结

`ContentCatalogBuildSystem` 是启动阶段的唯一写者。它将当前以下分散流程收口到本地 builder：

```text
Content source / legacy SetDefaults / ID Sets / rule populators
  -> typed mutable builders
  -> ContentCatalogValidationQuery
  -> immutable ContentCatalogSnapshot
  -> ContentCatalog published once
  -> WorldSession ready barrier opens
```

构建 System 可以调用受控 legacy adapter 从 `SetDefaults` 提取数据，但 adapter 必须在每次提取时
创建新的临时实例，立即复制所需字段，且不把临时对象返回给调用者。发布后从 DI/组合根仅注入
只读 `IContentCatalog`。若校验失败，旧 snapshot 保持可用或世界就绪屏障拒绝开启；不得发布半
个 catalog。

`ContentCatalogValidationQuery` 不改状态，至少验证：每个 ID 有可解析定义；持久 ID 双向映射
一一对应；负 NPC net ID 映射不丢失；Item/Projectile 引用的目标 ID 存在；Tile/Wall ID 在各表
范围内；drop rule 的 NPC 和 Item 引用可解析；recipe group 引用可解析；所有对外数组/集合在
发布后不能写入。

### 4.6 实例化、临时覆盖与投影

`DefinitionToItemInstanceInitializer`、`DefinitionToNpcInstanceInitializer` 和
`DefinitionToProjectileInstanceInitializer` 读取 definition，按 spawn command 的显式输入创建各自
运行时状态。顺序为“选择 definition -> 初始化实例 state -> 应用 spawn 参数 -> 通过
`WorldStorage` 提交槽位/实体”，而不是先从 `Main` 样本取得可变对象再分散改写。

`EffectiveTileCollisionQuery` 读取 `TileDefinitionCatalog` 和阶段受控的
`TileSolidityOverrideState`，计算有效固体性。这样保留现有 Liquid/WorldGen 的必要临时语义，
同时阻断对内容定义表的运行时写入。

Bestiary、creative 排序、Dye shader ID、网络序列化和 save DTO 都是 Projection/Adapter。它们可以
订阅 catalog 发布事件生成只读快照，却不得反向写入 catalog 或通过表象状态改变玩法定义。

## 5. 依赖方向和执行顺序

```text
startup source / legacy adapter / rule populators
  -> ContentCatalogBuildSystem
  -> ContentCatalogValidationQuery
  -> frozen IContentCatalog
  -> WorldUpdateBarrier opens

IContentCatalog + WorldSession snapshot + WorldStorage read view
  -> pure eligibility queries
  -> Item/NPC/Projectile/Craft/Drop systems
  -> Spawn, inventory, tile, buff, and lifecycle commands
  -> WorldStorage commit barrier
  -> network / save / UI projections
```

运行 tick 中的依赖方向为 `ContentCatalog -> rule/query -> System -> Command -> WorldStorage`。Catalog
不依赖 `WorldStorage`，不会写 command，不发送网络包，不序列化存档，也不读取时钟或随机源。

显式调度约束：内容加载和完整校验必须先于 WorldSession ready；实例初始化必须先于实体进入
AI/physics/combat；drop/craft 资格先于随机解析与命令提交；projection 必须在提交后读取稳定快照。
文件、项目或注册顺序不能隐式表达这些依赖。

## 6. 接口契约与测试 seam

| 模块 | 输入 | 输出 | effect seam | 测试 seam |
| --- | --- | --- | --- | --- |
| `IItemDefinitionQuery` 等 | Type/Net/Persistent ID | immutable definition 或 `false` | 无副作用 | 为每种 ID 和未知 ID 做 lookup；修改返回值不能影响下次 lookup |
| `ContentCatalogBuildSystem` | 内容 source、legacy adapter、ID Set source | build candidate | 仅 startup 构建与发布 port | 小 ID fixture；构建失败时不发布；相同输入产生同 snapshot |
| `ContentCatalogValidationQuery` | candidate snapshot | validation result | 纯 | 漏项、重复 persistent ID、悬空引用、无效 chance 皆可复现 |
| `DefinitionTo*Initializer` | definition、typed spawn request | initialized runtime state/command | command buffer 写入 | 初始化后修改实例不影响 definition；spawn override 只改实例 |
| `CraftEligibilityQuery` | recipe definition、inventory/world snapshot | eligible/rejection reason | 纯 | 缺材料、缺工作站、环境不符、成功路径 |
| `DropEligibilityQuery` | rule、death/world snapshot | eligible/rejection reason | 纯 | difficulty/seed/event 边界；不调用随机或生成 item |
| `DropResolutionSystem` | eligible rule、`IRandomSource`、command buffer | drop commands | 随机与 command 明确注入 | 固定 random 序列产生固定命令；失败没有世界写入 |
| `EffectiveTileCollisionQuery` | tile definition、override state、tile state | collision policy | 纯 | override 生命周期内外有效性；基础定义不可变 |

为避免行为漂移，第一批 verifier 应以 legacy 行为为 oracle：针对选择的 Item、NPC、Projectile、
Tile、Wall、Buff 和 drop rule ID，比对 legacy 受控临时实例提取值与冻结 definition；再验证
实例化后本地更改没有污染 catalog。由于 Recipe/Fishing 链路未闭合，对这两类只能先写加载/验证
测试，不应声称端到端玩法等价。

## 7. 建议迁移切片和兼容策略

1. 建立只读 `IContentCatalog`、`ContentCatalogSnapshot` 和 validator，不改变任何旧 `Main.*` 写者。
   用 legacy adapter 构造快照，并用 focused parity verifier 固定代表性 ID。
2. 先迁移 Tile/Wall/Buff/Projectile capability 等纯读取查询。保留现有 `Main.*` 作为单向兼容
   adapter 的数据来源，禁止 catalog 与 `Main.*` 双向同步。
3. 迁移 Item/NPC/Projectile 的实例创建入口。新 initializer 成为唯一新路径；旧
   `SetDefaults` 仅留在 adapter/兼容入口，不能让游戏 tick 直接以样本充当定义。
4. 将 `ItemDropDatabase` 的注册阶段转为 builder，将 `ItemDropResolver` 拆为资格、随机解析和
   command 提交。先对固定随机序列建立覆盖，再迁移。
5. 补全 Recipe、RecipeGroup 和 Fish resolver 的真实加载/消费者证据后，才把其目录标记为
   `confirmed` 并接入权威 crafting/fishing。不要把当前 `partial` 结构发布为可用玩法。
6. 最后迁移 Bestiary、creative、shader、save/network DTO 到投影层。删除或改写旧入口前，确认
   没有消费者持有可变 definition 引用。

兼容期只能存在一个权威定义写者：要么 legacy 初始化构建 snapshot，要么新 builder 构建 snapshot。
禁止新旧目录相互回写。任何动态内容重载必须走完整 build/validate/publish 事务，并由明确的
世界暂停或版本屏障保证不跨 tick 观察到混合内容版本。

## 8. 暂不拆分与风险

| 项目 | 决定 | 理由与解除条件 |
| --- | --- | --- |
| 紧凑 ID 数组 | 不改为对象图 | 现有代码和协议广泛按 ID 索引；只先收口只读接口。等查询及序列化边界稳定后再评估内部存储。 |
| `SetDefaults` 的大 switch | 不按 switch case 拆成海量类型组件 | 先由 adapter 提取 definition 和 initializer 分离实例复位；每个 type 的特殊行为仍应按真正的共同读写者拆分。 |
| `ItemDropRule` 条件/链式规则树 | 暂保持一个不可变 rule graph | 链接顺序和条件语义本身构成不变量。只拆读取、资格、随机和提交边界。 |
| Bestiary/creative/shader 索引 | 不纳入玩法权威 definition | 它们是从内容或表现需求派生的 projection，应当单向读取。 |
| Recipe 运行时 | 不实施 | 当前源码的 setup 调用被注释且 `Recipe.cs` 没有闭合实现，缺少完整生命周期证据。 |
| Fish runtime resolver | 不实施 | 已确认规则构建与 chance 校验，未确认完整解析到产物提交的路径。 |

主要风险是 legacy 静态表在初始化后仍被写入：已直接观察到 `Liquid` 和 update 路径会改变
`Main.tileSolid`。若未经 override 设计就把该数组冻结，会改变液体、世界生成或碰撞行为；若保持
全局可写，又会破坏 catalog 的只读不变量。迁移必须先通过 `EffectiveTileCollisionQuery` 显式建模
这一例外。另一个风险是 `ContentSamples` 将 mutable entity 对象作为样本公开，任何迁移若不深拷贝
或冻结 record，都会产生跨实体和跨系统的定义污染。

## 9. 本次验证结果

- 已完成：读取目标设计、Version4 真实源码、SS14 可访问的原型定义与消费者代码；建立字段/规则
  所有权、生命周期和调用方向。
- 已执行：`src/Content/Terraria.Content.csproj` 与 `Test/Terraria.Content.Catalog.Verification`
  的串行编译运行；产物位于 `Build/bin/`。验证覆盖目录查询、身份映射、冻结发布、引用校验、
  Tile override、builder source seam 和定义到实例初始化。
- 未执行：Version4 legacy adapter 的真实提取、Recipe/Fishing 端到端解析、完整服务器运行时回归；
  这些仍受原始源码证据中的 `partial` 状态约束。
- 报告结论：Item、NPC、Projectile、Tile、Wall、Buff、NPC drop 的定义/消费者证据可支持上述
  组件边界；Recipe 与 fishing runtime 必须保持 `partial`，直到补齐明确的加载和执行证据。
