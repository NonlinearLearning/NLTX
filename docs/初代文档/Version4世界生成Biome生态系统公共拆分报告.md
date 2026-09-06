# Version4 世界生成、Biome 与生态系统公共拆分报告

> 范围：落实《[Version4 权威游戏模拟系统主要子系统](Version4权威游戏模拟系统主要子系统.md)》第 13 项：新世界创建/加载、地形和结构生成、Biome 查询、场景环境扫描、城镇住房与运行期生态扩散。
>
> 方法：本报告遵循 `public-decomposition`。它按状态所有权、共同读写者、生命周期和副作用划分 Component、System、Query、Command、Adapter 与 Projection；不是按 `WorldGen.cs` 的字段或目录机械拆文件。
>
> 结论状态：这是基于 Version4 真实源码的拆分设计与证据报告，不代表 NLTX 已完成迁移、编译、运行时回归或 Terraria 协议兼容。

## 1. 结论

`Terraria.WorldGen` 同时承担了五个性质不同的责任：生成生命周期、随机/Pass
执行、Tile/结构写入、运行期生态 Tick 和住房检查 scratch。它不应迁为一个
`WorldGenerationComponent` 或一个大 `WorldGenSystem`。

建议让 `WorldSession` 和 `WorldStorage` 并列成为世界根，采用以下边界：

```text
WorldSession
  ├─ WorldDescriptorState                 已生成世界的身份、尺寸、地层和锚点
  ├─ WorldRulesState                      种子、难度、秘密种子和长期规则
  ├─ WorldGenerationLifecycleState        Uninitialized/Loading/Generating/Ready/Failed
  └─ WorldEcologyScheduleState            运行期传播许可、采样游标和频率

WorldStorage
  ├─ TileMap / WallMap / LiquidMap         唯一的格子权威状态
  ├─ TownHousingRegistry                   NPC 类型/稳定身份到房屋位置的持久关系
  └─ TileEntity / EntitySlot stores

WorldGenerationAndBiomes
  ├─ GenerationDefinitionCatalog           只读 pass、结构和 Biome 定义
  ├─ TerrainGenerationSystem               创建期独占写 Tile 的有序作业
  ├─ BiomeRuleQuery                        Tile/Wall/规则/位置 -> BiomeResult
  ├─ SceneSensingSystem                    Tile/液体/实体快照 -> SceneSnapshot
  ├─ TownHousingSystem                     Room query 结果 -> registry 提交
  └─ WorldEcologySystem                    tick/RNG/规则 -> TileMutationCommand
```

关键不变量如下：

1. `TileMap` 是地形、墙和生态结果的唯一权威。生成和生态都只能提交
   `TileMutationCommand`，不能各自保留第二份 Tile 数组。
2. `BiomeResult` 和 `SceneSnapshot` 是从 TileMap、世界规则及位置导出的只读结果，
   不能持久化或反向写入游戏规则。
3. 生成、加载和正常世界 Tick 互斥。`Ready` 之前，实体更新、刷怪、住房重评估和生态
   均不得运行。
4. 住房“房间是否有效”是一次 Query；NPC/房屋占用关系是持久的 registry。两者不能合并。
5. 随机源、种子、pass 清单和取消结果必须是生成作业的显式输入/结果，不能隐藏在静态
   `Main.rand` 或 background `Task` 内。

## 2. 证据范围

### 2.1 目标文档与实际代码根

目标文档将该子系统定义为：`WorldGenerationAndBiomes`、
`TerrainGenerationSystem`、`BiomeRuleQuery`、`SceneSensing`、
`WorldEcologyAndSpread`，并特别要求将其和常规 Tick 分离
（[主要子系统文档](Version4权威游戏模拟系统主要子系统.md) 第 13 项）。

用户给出的 Version4 代码根实际为 `D:\TRbackup\Version4`；
`D:\TRbackup\Version4参考` 在当前工作区不存在。本报告对以下**直接拥有者、写入者
和边界适配器**作了逐类审计：

| 代码集合 | 数量 | 为什么直接相关 | 证据状态 |
| --- | ---: | --- | --- |
| `Terraria.WorldBuilding/*.cs` | 42 | Pass、形状、条件、结构保护、seed option、manifest、snapshot、generator controller | `confirmed/partial` |
| `Terraria.GameContent.Biomes/**/*.cs` | 26 | Terrain/Jungle pass 与洞穴、沙漠、地牢、微型 Biome 结构放置 | `partial` |
| `Terraria.GameContent.Generation/**/*.cs` | 21 | 轨道、形状、Legacy pass 及地牢布局/数据/约束 | `partial` |
| `Terraria/WorldGen.cs` | 1 | 生成创建/清理/执行、住房检查、生态 Tick、草/感染扩散和 Tile 写入 | `confirmed/partial` |
| `Terraria/SceneMetrics.cs`、`SceneMetricsScanSettings.cs`、`SceneState.cs` | 3 | 场景扫描输入、派生环境快照和表现消费者的分界 | `partial` |
| `Terraria.GameContent/TownRoomManager.cs` | 1 | 房屋占用关系、Save/Load、清理和快速查询 | `confirmed` |
| `Terraria.IO/WorldFile.cs` | 1 | 世界读取后的水体结算、住房恢复校验、TownManager 存档边界 | `confirmed` |
| `Terraria/Main.cs`、`Player.cs` | 2 | world-ready 门控及 `SceneMetrics.Scan` 的真实调用者 | `confirmed` |

“相关所有代码”在本报告中指上表的直接责任集合和其入口/持久化调用链，而不是按名称
扫描 Version4 全部约 1,200 文件。NPC、掉落、渲染、网络和 UI 中大量 Biome 消费者只读取
这些边界的结果；它们不是该系统状态的拥有者，故没有被错误收进本次组件写集。

### 2.2 Version4 关键事实

| 事实 | 真实代码证据 | 拆分含义 | 状态 |
| --- | --- | --- | --- |
| 创建和生成会设置 `generatingWorld` / `isGeneratingOrLoadingWorld`，创建 generator，清空世界、Reset、AddPasses、运行并在 `finally` 清理 flag | `Terraria/WorldGen.cs:6285-6305,10108-10147` | 生命周期必须是单一状态机；不能让生态和实体系统各检查不同 bool | `confirmed` |
| `Main.ShouldUpdateEntities` 要求 world 已准备且不在生成 | `Terraria/Main.cs:11400-11409` | `WorldReadyBarrier` 从同一生命周期状态派生 | `confirmed` |
| `WorldGen.AddPasses` 顺序注册 Terrain、Jungle、Desert、Dungeon 等 pass，但 `AddGenerationPass` 两个实现为空 | `Terraria/WorldGen.cs:10553-13960,9175-9176` | pass 分类与调用顺序可证；最终登记/排序实现不能声称已经闭合 | `partial` |
| `WorldGenerator` 持有 pass、当前 pass、暂停/abort、hash、snapshot 和 controller lock | `Terraria.WorldBuilding/WorldGenerator.cs:25-226` | 它是一次生成 job 的内部控制状态，不是世界运行时组件 | `confirmed/partial` |
| 配置按 `Biomes` / `Passes` 根节点创建 `MicroBiome` | `Terraria.WorldBuilding/WorldGenConfiguration.cs:9-38` | 定义数据应进入只读 `GenerationDefinitionCatalog`，不能和运行期 Tile 状态混合 | `confirmed` |
| `WorldGen.UpdateWorld` 先检查加载/生成，再处理机制、TileEntity、灾变、液体、住房与 over/underground 生态采样 | `Terraria/WorldGen.cs:59400-59524` | 运行期 ecology 是独立的有序 System，不能放入生成 pass | `confirmed` |
| `SpreadGrass` 读取边界、规则、Tile 邻域和随机数，直接改 Tile、frame、网络同步，并递归传播 | `Terraria/WorldGen.cs:62711-62815` | 资格判断、随机决策、Tile 写入和网络投影必须拆开 | `confirmed` |
| `CanEvilReplace` 只判定地牢 Tile/Wall 保护 | `Terraria/WorldGen.cs:63003-63020` | 应成为纯 `EcologyMutationEligibilityQuery` 的一部分 | `confirmed` |
| `SceneMetrics.Scan` 按 tick 与 scan center 失效，Reset 后扫描 Tile/NPC、聚合、计算 zone | `Terraria/SceneMetrics.cs:18-197` | 场景结果是带失效条件的派生快照，而非 WorldSession 权威字段 | `partial`，核心 helper 在此 Version4 为空 |
| `TownRoomManager` 保存 `npc type -> Point` 对和 `_hasRoom`，提供 Set/KickOut/Save/Load | `Terraria.GameContent/TownRoomManager.cs:9-142` | 持久占用关系单列 `TownHousingRegistry`；不和房间 flood-fill scratch 合并 | `confirmed` |
| `StartRoomCheck` / `CheckRoom` 使用 room bounds、stack、tile set 和家具 flag | `Terraria/WorldGen.cs:5310-5773` | 房间检查是无持久 scratch 的 `TownHousingQuery`，提交只能发生在验证后 | `partial` |
| Load 完成后会 QuickWater/WaterCheck，读取 TownManager 并重新检查 NPC 房屋；保存/加载各调用 TownManager 适配器 | `Terraria.IO/WorldFile.cs:658-781,1871-1952,3476-3489` | 加载后结算与住房修复是 lifecycle 后置阶段，二进制读取留在 Adapter | `confirmed` |

本地 tModLoader API 镜像 `D:\TRbackup\tmodloader-api-docs-stable` 的首页确认版本为
`tModLoader v2026.07`。其中 `class_world_gen.html` 确认公开 `CreateNewWorld`、
`GenerateWorld`、`UpdateWorld`、`SpreadGrass`、`StartRoomCheck` 及 `generatingWorld` 成员；
`class_town_room_manager.html` 确认 `HasRoom`、`SetRoom`、`KickOut`、`Save`、`Load`。
它只能补充公开成员存在和签名，不能证明 Version4 的私有写集、任务调度、WLD 字段顺序或网络
语义；这些结论均以上表 Version4 本地源码为准。

### 2.3 Space Station 14 只读参考

SS14 仅用于学习 ECS 组织边界，不能复制其 `Biome`、`Dungeon`、地图、网络或游戏规则。

| SS14 实际代码 | 观察到的模式 | 对本系统的可迁移结论 |
| --- | --- | --- |
| `Content.Shared/Parallax/Biomes/BiomeComponent.cs:9-81` | 配置、seed、layer 和已加载 chunk 是一个地图附着状态；修改 Tile/decals/entities 有独立追踪集合 | 若需要可恢复的世界生成缓存，应显式记录其所有者与失效条件，不能塞进玩家或普通 Tile 状态 |
| `Content.Shared/Parallax/Biomes/SharedBiomeSystem.cs:68-147` | 坐标、layer、seed 和 tile grid 输入经过只读方法求 Tile | `BiomeRuleQuery` 应接受显式 TileMap/规则/位置输入并返回结果，不修改世界 |
| `Content.Server/Parallax/BiomeSystem.cs:80-178` | MapInit、seed/template 初始化、加载范围和运行期缓存都在 System；component 不持有服务 | `TerrainGenerationSystem` 拥有 RNG/Tile writer 等依赖，状态组件只记录领域状态 |
| `Content.Shared/Procedural/Dungeon.cs:8-88` 与 `DungeonConfig.cs:7-50` | 生成得到的空间结果和只读 layer 配置是不同类型 | `StructurePlan` / `GenerationDefinitionCatalog` 不应与 TileMap 或进度条混合 |
| `Content.Server/Procedural/DungeonSystem.cs:29-251` | generation 是可取消 job；job queue、CancellationToken、外部服务和提交在 System | 生成作业句柄、线程与取消 token 留在 System/host adapter；`WorldGenerationLifecycleState` 只保存稳定 phase 与失败类别 |
| `Content.Server/Station/Components/StationBiomeComponent.cs:9-22` 与 `StationBiomeSystem.cs:10-30` | 小组件仅保存 template、seed、光照配置；System 在明确初始化事件运行 | 小且稳定的世界规则可以是状态；初始化时机是 System 责任 |
| `Content.Server/Procedural/RoomFillComponent.cs:9-41` 与 `RoomFillSystem.cs:10-48` | 房间选择配置和 MapInit 的结构写入分离；结束时删除一次性 marker | Version4 的房屋检查 scratch / 一次性生成 marker 不应成为长期住房 registry |

## 3. 成员归属与目标组件

### 3.1 状态与行为 ledger

| 成员簇 | 主要读者 | 主要写者 | 生命周期/频率 | 状态类型 | 目标归属 | 证据状态 |
| --- | --- | --- | --- | --- | --- | --- |
| 世界 ID、种子、尺寸、surface/rock、出生/地牢锚点 | 生成、Biome、生态、出生、存档 | Create/Load/Generation finish | create/load 到 unload | authority | `WorldDescriptorState` | `confirmed` |
| 秘密种子、难度、world evil、ore tier | pass、生态、刷怪、事件 | create/load/规则命令 | world lifetime | authority | `WorldRulesState` | `confirmed/partial` |
| phase、稳定 failure code | barrier、调度器、load/create | `WorldSessionLifecycleSystem` | load/generate/unload | lifecycle authority | `WorldGenerationLifecycleState` | `confirmed` |
| pass 清单、当前 pass、progress、snapshot、abort/lock | generation UI/controller | `WorldGenerator` | 仅生成 job | scratch / projection | `IWorldGenerationJob` 私有实现 | `partial` |
| shapes、conditions、structure definitions、Biome 配置 | Terrain pass / query | 内容加载 | 初始化后只读 | definition | `GenerationDefinitionCatalog` | `confirmed/partial` |
| `StructureMap` reservation 和建筑候选 | structure pass | terrain/structure generation | 仅生成 job | scratch authority | `StructureReservationState`，job 内部 | `partial` |
| Tile/Wall/Liquid、frame | collision、Biome、生态、存档、网络 | generation/ecology/tile interaction | world lifetime | authority | `WorldStorage`，不建逐 Tile ECS entity | `confirmed` |
| Tile/Wall/规则/位置到 biome 的资格 | spawn、drop、fishing、shop、scene consumer | 无，纯计算 | 按请求 | derived | `BiomeRuleQuery` | `partial` |
| `SceneMetrics` counts/zones/closest NPC | Player、Main、表现和资格消费者 | `SceneMetrics.Scan` | per tick/position | derived cache | `SceneSnapshot` + `SceneSensingSystem` | `partial` |
| 房间 flood-fill stack、bounds、家具标志 | 房屋资格检查 | `StartRoomCheck` | per query | scratch | `TownHousingQuery` 局部变量 | `partial` |
| NPC 到 home position 的占用关系 | town NPC、save/load | assign/kick/load | world lifetime | authority relation | `TownHousingRegistry` | `confirmed` |
| infection gate、sample cursor、world update rate、town NPC scheduling | ecology/town spawn scheduler | `WorldEcologySystem` | regular world tick | runtime authority | `WorldEcologyScheduleState` | `confirmed/partial` |
| 草、藤、树、感染、墙、植物和自然变化 | TileMap、replication | ecology system | sampled world tick | behavior/effect | `WorldEcologySystem` -> `TileMutationCommand` | `confirmed/partial` |
| WLD section layout、BinaryReader/Writer、post-load water settle | persistence host | `WorldFile` | I/O boundary | adapter/effect | `WorldGenerationPersistenceAdapter` | `confirmed` |
| progress text、sound、Cloud、scene visuals、network Tile square | UI/client/network | systems after commit | projection/effect | projection | progress/network/presentation projections | `confirmed/partial` |

### 3.2 推荐的 Component 与关联值类型

这些是目标结构，不表示已在 `src/` 中实现。每个公开 C# 类型应单独同名文件，放在领域目录；
本报告不建议创建泛化的 `Components/` 或 `Systems/` 收容目录。

| 类型 | 权威字段 | 不包含 | Interface / seam |
| --- | --- | --- | --- |
| `WorldGenerationLifecycleState` | `Phase`、稳定 `Failure`、`GenerationRevision` | `Task`、thread ID、`CancellationToken`、pass list、UI progress | `IWorldReadinessView`; phase-transition recorder |
| `WorldDescriptorState` | world identity、seed text、尺寸、边界、地层/出生/地牢锚点 | TileMap、实体/网络 ID、当前 job | immutable load/create input fixture |
| `WorldRulesState` | mode、hardmode、secret seed、world evil、长期 ore selections | `SceneMetrics`、天气、生态 cursor | `IWorldRulesView`; rule matrix fixture |
| `WorldEcologyScheduleState` | infection spread permit、over/underground sampling cursors、rate/budget、town scan cursor | Tile counts、scene result、玩家/NPC引用、RNG | fixed tick/RNG mutation-log fixture |
| `TownHousingRegistry` | stable NPC key 到 room position 的关系、occupied/revision | flood-fill stack、家具计数、NPC slot/network ID | `ITownHousingView`; assignment transaction fixture |
| `SceneSnapshot` | scan center、world/tile revision、computed counts/zones、scan tick | 可写 Tile、存档字段、presentation state | read-only `ISceneSnapshot`; invalidation fixture |
| `GenerationDefinitionCatalog` | versioned immutable pass/shape/structure/biome definitions | random state、world progress、TileMap | catalog fake / fixed definition fixture |
| `StructureReservationState` | generation job 内已保护/保留的范围 | Runtime ecology state、persistent tile data | `IStructureReservation`; overlapping placement fixture |

`SceneSnapshot` 应为短生命周期 projection/cache，不加入持久化的 WorldSession snapshot。若当前
WorldStorage 无世界 revision，先在 `TileMutationCommand` 提交点产生 revision，再让扫描缓存以
`(center, range, tileRevision, entityRevision)` 失效；没有此失效契约就不要缓存。

## 4. System、Query、Command 与 Adapter

| 模块 | 输入 -> 输出 | 唯一写集与副作用 | 不负责 |
| --- | --- | --- | --- |
| `WorldSessionLifecycleSystem` | create/load request -> `Ready`/`Failed` lifecycle transition | 唯一写 lifecycle；协调 reset、load、cleanup；发布生命周期事件 | Tile 算法、WLD bytes、NPC update |
| `TerrainGenerationSystem` | descriptor + rules + seed + catalog -> ordered `TileMutationCommand` / structure commands | 仅在 exclusive generation phase 写 WorldStorage；固定 RNG 流与 pass 顺序 | normal ecology tick、client progress UI |
| `BiomeRuleQuery` | TileMap read view + `WorldRulesState` + `TilePosition` -> `BiomeResult` | 纯函数，无写入、无 RNG、无 I/O | Tile mutation、scene cache |
| `SceneSensingSystem` | scan request + read snapshots -> `SceneSnapshot` | 计算/缓存派生度量；明确 cache 失效 | gameplay state、TileMap、scene visual |
| `TownHousingQuery` | TileMap + room position + content definitions -> `RoomQualification` | 纯 scan，工作内存仅在调用内存在 | registry assignment、NPC spawn |
| `TownHousingSystem` | qualification + assign/evict command -> registry change | 原子更新 `TownHousingRegistry`，发持久化 dirty event | 直接改 NPC AI、网络格式 |
| `WorldEcologySystem` | committed world/Tile views + schedule + RNG -> `TileMutationCommand[]` | 唯一写 ecology schedule；提出草、藤、树、感染等写请求 | 直接 `NetMessage`、场景 UI、普通 Tile 交互 |
| `TileMutationCommitSystem` | commands -> TileMap revision and committed mutation events | Tile/Wall/Liquid 的唯一写边界；每批检查冲突、frame、revision | 选择何时传播、网络 DTO 细节 |
| `WorldGenerationPersistenceAdapter` | `WorldSessionSnapshot` <-> WLD bytes | 所有文件 I/O、版本分支、temporary-save 恢复 | business rule、scene snapshot |
| `WorldGenerationNetworkProjection` | committed Tile/descriptor/lifecycle snapshot -> protocol DTO | 网络序列化和发送；入站只形成 validated command | 直接写 WorldSession 或 TileMap |

最小 public seam 建议如下：

```csharp
public interface IWorldGenerationJob
{
  WorldGenerationResult Run(
    in WorldGenerationRequest request,
    IGenerationDefinitionCatalog catalog,
    IWorldMutationSink mutations,
    CancellationToken cancellationToken);
}

public interface IBiomeRuleQuery
{
  BiomeResult Resolve(in BiomeQueryContext context);
}

public interface ITownHousingQuery
{
  RoomQualification Check(in RoomCheckRequest request);
}

public interface IWorldEcologyPlanner
{
  IReadOnlyList<TileMutationCommand> Plan(
    in EcologyTickContext context);
}
```

`IWorldEcologyPlanner` 与 `IBiomeRuleQuery` 应当是确定性计算。时钟、随机数、世界 Tile
read view、规则和预算作为 `Context` 的显式输入；提交 Tile、发送网络包、写存档和播放声音
留在外层 System/Adapter。

## 5. 调度与数据流

```text
Create / Load command
  -> WorldSessionLifecycleSystem (Loading / Generating)
  -> WorldGenerationPersistenceAdapter OR TerrainGenerationSystem
  -> TileMutationCommitSystem
  -> post-load water settlement + TownHousing repair
  -> WorldSessionLifecycleSystem (Ready)

Regular world tick, only when Ready
  -> WorldClock/Event systems provide committed rule inputs
  -> WorldEcologySystem -> TileMutationCommitSystem
  -> TownHousingQuery -> TownHousingSystem (when scheduled)
  -> SceneSensingSystem (derived snapshot; no gameplay writes)
  -> Persistence / Network / Presentation projections
```

必须显式声明以下顺序：

1. `TerrainGenerationSystem` 与所有 Tile/Liquid/生态写者互斥。
2. 生态计划读取**上一提交点**的 TileMap；所有生成的 Tile command 在同一 commit barrier
   提交，随后才能触发 frame、存档和网络 projection。
3. 住房有效性在 Tile 提交之后评估；资格 Query 成功不等于房屋已经分配。
4. `SceneSensingSystem` 位于 Tile commit 后，且任何结果只读。它绝不能以扫描结果直接改
   spawn、combat 或 Tile；需要玩法决策时由相应 System 调用同一 Query 再显式提交。
5. 正常实体/生态 Tick 在 `Generating`、`Loading`、`Failed`、`Unloading` 一律跳过。

## 6. 不拆分项与兼容策略

| 不拆分或暂缓项 | 原因 | 策略 |
| --- | --- | --- |
| 每一种 `MicroBiome` 一个长期 ECS component | 这些是生成 definition / algorithm，不是所有世界都持有的可变实例状态 | 保留为 catalog 记录和可替换 pass 实现 |
| 一个 Tile 一个 ECS entity | 世界网格稠密且现有 Tile 数组是存档/协议核心；会使身份、结构变更和性能恶化 | `TileMap` 作为专用 WorldStorage，使用块/区域 mutation |
| `SceneMetrics` 作为世界权威生态计数 | 它按中心和 tick 生成，且 Version4 的核心 helper 为空 | 只允许短期 derived snapshot，标记 `partial` |
| `WorldGenerator.Controller` 进入持久 component | 它含锁、pause、abort、snapshot 和 debug/UI 状态 | 封装在 job implementation；持久化只存已提交 WorldSession/TileMap |
| `TownRoomManager` 直接作为 NPC component | 它当前按 NPC type 维护世界关系并自行 Save/Load | 迁移为 `TownHousingRegistry`，明确稳定 ID 规则后才可变更键类型 |
| `AddGenerationPass` 的最终实现与排序 | 两个方法体在 Version4 为空，无法确认实际注册代码 | 先建立 pass-order trace verifier；未补证前保持 `partial`，不可声称字典/列表排序兼容 |

迁移必须避免“旧静态字段与新组件双写”。推荐顺序为：先建立只读 adapter 和 trace
对比，再逐个迁移 `Lifecycle -> Descriptor/Rules -> Generation job -> Ecology -> Housing ->
Scene snapshot -> persistence/network projection`。每批仅有一个权威写者；出现回归时回退该批
adapter，而不是让两套状态同步修补。

## 7. 风险与验证计划

| 风险 | 原因 | focused verifier |
| --- | --- | --- |
| 生成与 normal tick 重叠写 Tile | 当前生成使用静态 flag / background task | Loading/Generating 下 entity、spawn、ecology 均无 Tile mutation；success/fail/cancel 后 phase 唯一 |
| pass 顺序或 seed 漂移 | Version4 pass registration 实现未闭合 | 固定 seed 的 pass trace、每 pass Tile hash、manifest 前缀比较 |
| scene cache 陈旧 | scan 依赖 center/tick/Tile/NPC 数据 | 改 Tile、移动中心、跨 tick、NPC 改变各触发一次失效；无变化时复用 |
| 感染扩散越过保护规则 | `SpreadGrass` 混合资格、递归、写 Tile 和网络发送 | dungeon/cracked-wall/边界/Creative 禁止传播矩阵；命令日志只含允许坐标 |
| 房屋资格与占用关系分叉 | scratch 检查和 `TownRoomManager` 是不同生命周期 | 无效房屋不改变 registry；重复分配/驱逐/Save-Load 后双向关系一致 |
| WLD 读写破坏 | `WorldFile` 有版本化 section 和 post-load repair | known-world round trip、截断输入原子失败、load 后 QuickWater/housing repair 顺序 |
| 网络或表现反向写权威状态 | 旧 `WorldGen` 直接发送 Tile square，SceneState 消费 metrics | command handler 拒绝 client scene/progress 状态；projection golden frame 仅读取 committed snapshot |

本次交付是只读代码审计与文档编写，未修改生产 C#，因此没有运行 `dotnet` build 或 verifier。
上表列出实施后必须运行的 focused verifier；在其运行并记录 artifact 前，不得把该拆分称为
已验证的迁移。

## 8. 与现有报告的关系

本报告聚焦第 13 个主要子系统。世界规则、时钟、长期事件进度和网络/存档的更宽分析见
《[世界生成、Biome 与生态：世界会话及进度公共拆分审计报告](世界生成Biome生态与世界会话进度公共拆分审计报告.md)》；
其中已有组件字段草案见
《[世界生成、Biome 与生态：世界会话及进度组件设计](世界生成Biome生态与世界会话进度组件设计.md)》。本报告不改写这两份已有文件，也不将其未接线类型误报为生产实现。
