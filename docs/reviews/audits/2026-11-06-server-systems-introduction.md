# 服务器运算系统引入记录：刷怪 / 世界物品 / Tile 实体 / 物品使用

日期：2026-11-06
范围：`src/NSSLC/Components/{Npc,World,Player,Share/Item,Share/Inventory}`、`src/NSSLC.Network`、
`src/NSSLC.Server`、`Test/*`

本文记录四项 Version4 服务器运算能力在 NLTX ECS 中的落地形态、参考来源、验证证据与仍然存在的缺口。
文档分两类断言：**已构建/已运行**（有命令与输出）与**静态检视**（只读过代码，未执行）。

## 1. 世界物品 tick（下落 / 合并 / 拾取 / 消失）

| 文件 | 角色 |
| --- | --- |
| `Components/WorldItem/Model/WorldItemMotionProfile.cs` | 每种液体接触的 `(Gravity, MaximumFallSpeed, Wet)` 档案 |
| `Components/WorldItem/Solver/WorldItemMotionSolver.cs` | 逐 tick 积分与位移换算 |
| `Components/WorldItem/Solver/WorldItemMergeRules.cs` | 曼哈顿距离 ≤ 30 的合并判定与合入数量 |
| `Components/WorldItem/Solver/WorldItemPickupRules.cs` | 42 像素抓取范围与曼哈顿代价 |
| `Components/WorldItem/WorldItemTickSystem.cs` | 合并 → 运动 → 拾取三段，下落出界即退役 |
| `Components/World/Model/WorldTileArea.cs` | 瓦片矩形（含世界边界裁剪） |

参考常量（`Terraria.Item` 与 `Terraria.Main` 的物品下落路径）：空中重力 0.1、最大下落 7；
水中 0.08/5；蜂蜜 0.05/3；微光 0.065/4；水平阻尼 `×0.95`；速度死区 ±0.1；接触液体时位移 ×0.5；
物品碰撞盒 16×16；抓取距离 42；合并距离 30。**单位是像素/tick，不乘 deltaTime**，与参考一致。

守恒契约：拾取由世界物品域发出 `InventoryPickupRequest`，Inventory 域只报告实际接受数量，
World 域据此发出 `WorldItemRequestKind.Consume` 恰好删除同样的数量；合并走
`TryCombineStacks` 并按合入比例插值位置与速度。

## 2. NPC 刷怪

| 文件 | 角色 |
| --- | --- |
| `Components/NpcSpawn/Solver/NpcSpawnBudgetRules.cs` | `GetSpawnRate` 阶梯：难度/深度/夜晚/血月/日食缩放与上下限 |
| `Components/NpcSpawn/Solver/NpcSpawnAreaRules.cs` | 屏幕 1920×1080 → 120×67.5 瓦片，安全区 0.52、刷怪区 0.7，50 次尝试选点 |
| `Components/NpcSpawn/Solver/NpcSpawnRandom.cs` | 由世界种子与 tick 派生的 xorshift64* |
| `Components/NpcSpawn/NpcSpawnSystem.cs` | 每玩家预算 → 敌对上限 → 概率门 → 选点 → 可见性检查 → 生成 |
| `NSSLC.Network/NpcNetworkIdentitySystem.cs` | 为生成的 NPC 分配最低空闲网络槽位 `[0, 200)` |

关键参考常量：`defaultSpawnRate = 600`、`defaultMaxSpawns = 5`、`spawnSpaceX = 2`、
`spawnSpaceY = 3`、`MaximumAttempts = 50`；速率下限为 `600 × 0.1 = 60`，敌对上限 15。
生成时把底边对齐到目标瓦片、速度置零并挂 `AiComponent`，同时投递 `NpcSpawnedFact`。

## 3. Tile 实体（箱子 / 告示牌 / 传送塔）

| 文件 | 角色 |
| --- | --- |
| `Components/TileEntity/WorldTileEntityRecordDecoder.cs` | `world.tile-entities` 记录流解码 |
| `Components/TileEntity/WorldTileEntityStore.cs` | 以瓦片坐标索引箱子/告示牌、以持久化 id 索引 Tile 实体 |
| `Components/TileEntity/Model/WorldChestState.cs` | 40 槽箱子内容与"单会话持有"开箱所有权 |
| `Components/TileEntity/Model/WorldSignState.cs` | 文本归一到 `\n`，编辑超长拒绝、载入超长截断 |
| `Components/TileEntity/WorldTileEntityRequestSystem.cs` | 开箱/关箱/改文本意图的唯一写入者，逐条回执 |

记录格式按 `TileEntity.Read` / `ReadInner`：`byte kind`、`int id`、`short x`、`short y`，
随后按 kind 读载荷（训练假人 `int16 npc`；物品展示框/武器架/食物盘 `int16 type`、`byte prefix`、
`int16 stack`；逻辑感应器 `byte check` + `bool on`；传送塔与死细胞展示罐无载荷）。kind 编号取
`TileEntitiesManager.RegisterAll` 的注册顺序 0…10。

传送塔（kind 7）是 3×4 多瓦片实体，`TileWidth/TileHeight` 已按参考声明；**传送动作尚未接入**：
目标选择与落点写入需要 Movement 域的落点通道，本轮只做到注册、索引与尺寸。

不支持的四类（展示人偶、帽架、风筝锚、小动物锚）载荷是变长位集，解码器返回明确失败而不是
猜测长度 —— 猜错会让整流错位，宁可整世界加载失败也不要载入错位数据。

`ServerWorldLoader` 现在读取 `world.chests`、`world.signs`、`world.tile-entities` 三段，
缺段视为空表。元素到 `ItemId` 的映射走 `ItemIdRules.TryFromNetworkType`，未知类型保留位置、清空载荷。

## 4. 物品使用 / 弹药 / 通道

| 文件 | 角色 |
| --- | --- |
| `Components/ItemUse/Content/ItemUseDefinition.cs` | 每物品一条不可变使用档案 |
| `Components/ItemUse/Content/ItemUseDefinitionCatalog.cs` | 封存后只读的档案目录 |
| `Components/ItemUse/Content/ItemUseRules.cs` | 使用门闸、弹药判定与成本 |
| `Components/ItemUse/Player/PlayerItemUseSystem.cs` | `Player.ItemCheck` 的门闸与弹药段移植 |
| `Components/ItemUse/Player/Model/PlayerItemUseFact.cs` | 每次使用对外投递的事实 |

门闸以"下一次可用 tick"表达参考的 `itemAnimation` / `reuseDelay` 倒计时；`useTime` 是
像素/tick 直觉不变的最小间隔（至少 1）。远程武器每次射击按 `ItemUseRules.AmmoCost` 消耗 1 发
匹配 `useAmmo` 的弹药，先查后扣，扣失败即拒绝且不改门闸。`channel` 物品在使用期间保持通道标记，
`ReleaseUse` 清除；重复使用同一通道物品时事实带 `Continuous = true`。

**蓝量缺口**：ECS 尚无蓝量资源，`ManaCost` 只随档案传递、不扣减，参考的
`statMana >= item.mana` 检查未生效。示范物品表只登记 3 条（火把 / 铜弓 + 木箭 / 魔法飞弹），
全物品表导入需要物品表数据，属于后续工作。

## 接入与注册

`src/NSSLC.Server/Program.cs` 创建了四个世界级队列实体
（`WorldItemRequestQueueComponent`、`InventoryPickupRequestQueueComponent`、
`NpcSpawnFactQueueComponent`、`WorldTileEntityRequestQueueComponent` /
`WorldTileEntityResultQueueComponent`、`PlayerItemUseRequestQueueComponent` /
`PlayerItemUseFactQueueComponent`），并在 Simulation 阶段按
`WorldItemTickSystem` → `InventoryPickupRequestSystem` → `NpcSpawnSystem` →
`NpcNetworkIdentitySystem` → `WorldTileEntityRequestSystem` → `PlayerItemUseSystem` 顺序注册，
全部早于 Projection 阶段，保证同一 tick 的事实对网络投影可见。

## 验证证据

| 命令 | 结果 |
| --- | --- |
| `dotnet build src\NSSLC.Server\NSSLC.Server.csproj -v q --nologo` | 0 警告 0 错误 |
| `dotnet run --project Test\Terraria.WorldItem.Verification\...csproj` | PASS（运动、合并、拾取守恒、出界退役、集成） |
| `dotnet run --project Test\Terraria.Npc.SpawnEligibility.Verification\...csproj` | PASS（21 项：速率阶梯、选点、可见性、端到端生成、槽位分配） |
| `dotnet run --project Test\Terraria.WorldSession.TileEntity.Verification\...csproj` | PASS（22 项：记录解码、不支持 kind/截断失败、所有权、文本边界、请求回执） |
| `dotnet run --project Test\Terraria.PlayerItemUse.Verification\...csproj` | PASS（15 项：门闸、弹药消耗、通道、拒绝路径） |
| `dotnet Build\bin\NSSLC.Server\Debug\net10.0\NSSLC.Server.dll src\World\科研.wld` | 启动日志：`chests=493 signs=0 tileEntities=0 pylons=0`，随后 `ecs.server.ready` |

真实世界文件里 493 个箱子被正确解码；该世界没有告示牌与 Tile 实体记录，因此这两段的
真实数据路径由合成记录流验证覆盖（静态推导 + 单元运行），尚未用含传送塔的存档跑过。

## 未覆盖 / 待办

- 传送塔传送动作（目标选择 + 落点写入）；
- 展示人偶 / 帽架 / 风筝锚 / 小动物锚的解码；
- 蓝量资源与法术耗蓝；
- 全物品使用表导入（当前 3 条示范）；
- 刷怪表的生物群系分区、困难模式/血月/日食进度旗标、房屋背景墙检查；
- 箱子物品移动（存入/取出）与告示牌客户端同步消息。


## 目录重组：四个系统各自独立成域（2026-11-07）

四个系统原本散落在 `World` / `Npc` / `Player` / `Share/Item` 中，与各自领域的历史文件混放。
本轮把它们移入 `src/NSSLC/Components/` 下的四个独立领域目录，**文件内容与命名空间均未改动**
（依据 [ECS 文件组织设计约束](../../../Context/架构约束/ECS文件组织设计约束.md) §7「目录移动优先保持
命名空间与公共 API 不变」），因此这是纯目录迁移，没有运行时行为变化。

| 新领域目录 | 系统 | 文件数 | 移出位置 |
| --- | --- | --- | --- |
| `Components/WorldItem/` | 世界物品 tick | 14 | `Components/World/WorldItem*`、`World/Model/WorldItem*`、`World/Solver/WorldItem*`、`World/Contract/IWorldItem*` |
| `Components/TileEntity/` | 箱子 / 告示牌 / 传送塔 | 15 | `Components/World/WorldTileEntity*`、`World/Model/World{Chest,Sign}State.cs`、`World/Model/WorldTileEntity*` |
| `Components/NpcSpawn/` | NPC 刷怪 | 12 | `Components/Npc/NpcSpawn*`、`Npc/Model/NpcSpawn*`、`Npc/Solver/NpcSpawn*` |
| `Components/ItemUse/` | 物品使用 / 弹药 / 通道 | 13 | `Components/Player/PlayerItemUse*`、`Player/Model/PlayerItemUse*`、`Share/Item/ItemUse*`、`Share/Item/Model/ItemUseDefinition.cs`、`Share/Item/Solver/ItemUseRules.cs` |

每个目录内部保持原有 `Model/` / `Solver/` / `Contract/` 分层；`ItemUse/` 额外分成
`Content/`（`namespace Terraria.Item` 的共享内容：定义、目录、规则）与 `Player/`
（`namespace Terraria.Player` 的玩家侧门闸：系统、队列、状态、事实）。

命名空间决策：全部保持原命名空间（`Terraria.World` / `Terraria.Npc` / `Terraria.Player` /
`Terraria.Item`），因此没有任何 `using`、公共 API 或依赖方向变化。若将来要让目录名与命名空间
一一对应（`Terraria.WorldItem` 等），那是一次公共 API 变更，必须单独评估。

依赖影响：`Terraria.Components.csproj` 使用 `<Compile Include="Components/**/*.cs" />` 通配，
移动不改变编译集合；`Program.cs`、`NSSLC.Network` 与四个验证项目都只按命名空间引用类型，
不受目录位置影响。

保留在原处的相关文件（不属于这四项系统本身）：

- `Components/World/Model/WorldTileCoordinate.cs`、`WorldTileArea.cs`：世界几何值类型，被世界物品、
  刷怪与 Tile 实体共同使用，按能力归属留在世界域；
- `Components/Communication/SessionPlayer*.cs`：会话玩家投影，由 Communication 域持有；
- `Components/Share/Inventory/InventoryPickupRequest*.cs`：拾取请求通道由接收方 Inventory 域拥有，
  跨域输入边界不变；
- `src/NSSLC.Network/NpcNetworkIdentitySystem.cs`：网络槽位分配属于 Network 项目边界，未移动。

回滚方式：把上表四个目录中的文件按"移出位置"一列放回，删除四个空目录即可；因为没有内容改动，
回滚不需要重新验证语义，只需重跑构建。

移动后的验证（2026-11-07）：

| 命令 | 结果 |
| --- | --- |
| `dotnet build src\NSSLC.Server\NSSLC.Server.csproj -v q --nologo` | 0 错误（19 条警告全部来自未改动的 `NSSLC.WorldGeneration` 既存代码） |
| `dotnet run --project Test\Terraria.WorldItem.Verification` | PASS |
| `dotnet run --project Test\Terraria.Npc.SpawnEligibility.Verification` | PASS |
| `dotnet run --project Test\Terraria.WorldSession.TileEntity.Verification` | PASS |
| `dotnet run --project Test\Terraria.PlayerItemUse.Verification` | PASS |
