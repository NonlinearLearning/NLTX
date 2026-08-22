# WorldGen ECS 迁移执行提案

> 状态：提案，未执行代码迁移。
>
> 事实源：`D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs`。
>
> 目标：将世界生成、地形、结构、树、矿物、Tile 改变和液体职责迁移到可验证的 ECS
> 组件/系统流水线，同时保留旧世界数据和 V1456 协议边界的兼容能力。

## 1. 结论先行

本次迁移不采用“把 `WorldGen` 按方法名拆成多个静态类”的方案。`WorldGen.cs` 当前是一个
约 73,356 行、1.90 MB 的 God Object，包含约 12,029 个 `Main.` 引用、607 个 `Tile` 引用和
53 个 `Liquid` 引用。直接按方法切文件会保留全局状态、随机数、网络写入和 Tile 原地修改的
隐式耦合，得到的是多个 God Object，而不是 ECS。

推荐的目标形态是：

```text
WorldGenerationRequest
  -> WorldGenerationStateComponent / WorldRuleSnapshot
  -> 固定顺序的生成系统
  -> TileChangeCommand / LiquidChangeCommand / StructurePlacementCommand
  -> TileChangeCommitSystem
  -> WorldGrid + section version
  -> immutable WorldGridSnapshot / WorldSectionSnapshot
  -> Server / Protocol 投影
```

其中：

- `WorldGrid` 仍是 Simulation 的权威稠密世界存储，不为每个 Tile 创建一个 ECS entity。
- ECS entity 表达生成任务、区域规则、结构放置请求、液体工作项等“有生命周期的意图或阶段
  状态”。
- 所有系统只能从只读快照读取，并通过命令写出结果；禁止系统直接调用 `Main.tile[x, y]`、
  `NetMessage`、音效、渲染或旧 `Liquid` 全局队列。
- `TileChangeCommitSystem` 是唯一把 Tile/墙体/液体变更提交到 `WorldGrid` 的系统。
- 只有在兼容验证通过后，才允许删除旧的 `WorldGen` 编译入口；Version4 目录在整个迁移期内
  只读保留，不在本提案中物理删除或覆盖。

## 2. 已核对的现状证据

### 2.1 Version4 事实源

已核对文件：

`D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs`

当前文件大小为 1,901,533 bytes，约 73,356 行，最后修改时间为 2026-08-11。文件中可以直接
观察到以下职责混合：

| 旧职责 | 事实源证据 | 迁移含义 |
| --- | --- | --- |
| Secret Seed 与生成配置 | `SecretSeed`、`FinalizeSecretSeeds` 等入口 | 变为不可变规则快照，不允许系统读写 `Main` 静态字段 |
| 地形/洞穴 | `TileRunner`、`Caverer`、`WavyCaverer`、`ChasmRunner` | 变为按区域执行的地形系统和确定性随机流 |
| 结构/地牢 | `Dungeon`、`IslandHouse`、`PlaceStatue`、传送器放置路径 | 变为结构定义 + 放置请求 + footprint 冲突检查 |
| 树/植被 | `GrowTreeSettings`、`AttemptToGrowTreeFromSapling`、草/藤蔓传播 | 变为树定义、候选点查询和树冠/树干命令 |
| 矿物/宝石 | `TileID.Sets.Ore`、`TileRunner`、`Spread.Gem` | 变为矿物定义注册表和确定性矿脉系统 |
| Tile 改变 | `KillTile`、`PlaceTile`、`ReplaceTile`、`SlopeTile`、`TileFrame` | 变为带来源/序号/冲突策略的 Tile 命令，并在单点提交 |
| 液体 | `PlaceLiquid`、`EmptyLiquid`、`Liquid.QuickWater`、`WaterCheck` | 变为液体源、传播工作项和边界受限的液体系统 |
| 世界运行时维护 | `UpdateWorld`、草/墙/树增长、落体和天气相关调用 | 与一次性生成分离；另立运行时环境系统，不混入生成系统 |

文件还包含大量 `Main.` 访问以及 `NetMessage.SendTileSquare` 等副作用。它们只能作为迁移
映射线索，不能作为新系统的依赖契约。

### 2.2 当前 NLTX 实现

当前可复用的世界边界位于：

- `src/Terraria.Dome.Simulation/World/WorldGrid.cs`
- `src/Terraria.Dome.Simulation/World/WorldTile.cs`
- `src/Terraria.Dome.Simulation/World/WorldGridSnapshot.cs`
- `src/Terraria.Dome.Simulation/Commands/TileChangeCommand.cs`
- `src/Terraria.Dome.Simulation/World/DefaultWorldEnvironmentConvergence.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationRequest.cs`

现有 `WorldGenerationPipeline` 只生成确定性地表/地面并清理出生点；现有
`DefaultWorldEnvironmentConvergence` 已证明可以按 tick 产生有限的 Tile/液体变化，但它是
验证投影，不代表已覆盖完整的 `WorldGen` 语义。

当前 `WorldGrid` 已经具备以下必须保留的边界：

- 4,200 x 1,200 等 V1456 世界尺寸可表达；section 为 200 x 150。
- Tile 状态通过 `WorldTile` 保存，section 版本独立递增。
- `TileChangeCommand` 进入队列后按 `Sequence, X, Y, Kind` 稳定提交。
- section/snapshot 是不可变复制，协议编码不直接观察可变世界。

## 3. 目标架构

### 3.1 组件分层

组件分为“世界单例组件”和“生成实体组件”。世界单例组件挂在一个
`WorldGenerationEntity` 上；生成实体用于可暂停、可重试、可分片的工作。不要把每一格 Tile
扩成一个 Arch entity，这会在 4,200 x 1,200 世界上制造约 500 万个实体，且不符合当前
`WorldGrid` 的 section 快照设计。

#### 世界单例组件

| 组件 | 关键字段 | 不变量 | 来源/用途 |
| --- | --- | --- | --- |
| `WorldGenerationStateComponent` | `GenerationId`、`Stage`、`NextSequence`、`IsComplete` | 同一生成 ID 只能向前推进阶段 | 替代 `WorldGen` 全局生成进度 |
| `WorldSeedComponent` | `Seed`、`SeedVariant`、`RandomStreamVersion` | 随机流版本固定后不可变 | 替代 `genRand` 和 secret seed 组合 |
| `WorldBoundsComponent` | `Width`、`Height`、section 尺寸 | 坐标和区域必须可验证 | 约束所有系统的读写范围 |
| `TerrainProfileComponent` | `SurfaceY`、`RockLayerY`、`UnderworldY`、生物群系带 | 所有高度带在生成开始前冻结 | 从 `Main.worldSurface` 等派生 |
| `WorldRuleSnapshotComponent` | difficulty、secret seeds、hardmode 等规则 | 系统只读 | 替代对 `Main` 静态规则的读取 |
| `GenerationCursorComponent` | 当前阶段、section 坐标、局部随机游标 | 重启后从 cursor 重放 | 支持分片生成和故障恢复 |
| `TileWriteBudgetComponent` | 当前 tick/阶段预算、拒绝计数 | 单阶段写入有上限 | 防止一个生成系统垄断 tick |

#### Tile/地形能力组件

这些组件不是“每 Tile 一个实体”，而是按 section/chunk 的稠密索引或阶段实体挂载。

| 组件 | 关键字段 | 用途 |
| --- | --- | --- |
| `TerrainLayerComponent` | 地表、土层、岩层、底层范围 | 记录地形系统当前要处理的层 |
| `CaveCarvingComponent` | 算法 ID、半径、密度、游标 | 描述洞穴/隧道 carve 工作 |
| `TileMutationIntentComponent` | 来源、优先级、冲突策略 | 让结构/矿物/树统一进入 Tile 命令阶段 |
| `TileFrameWorkComponent` | 待 frame 的 section/边界 | 把 frame 计算从写入动作中拆出 |
| `TileProtectionComponent` | 禁止覆盖的 footprint/标签 | 保护出生点、地牢、已提交结构 |

#### 结构、树和矿物组件

| 组件 | 关键字段 | 用途 |
| --- | --- | --- |
| `StructurePlacementComponent` | `StructureDefinitionId`、原点、旋转、种子 | 一个待放置结构的可重放请求 |
| `StructureFootprintComponent` | 矩形/多边形边界、占用策略 | 结构碰撞和越界检查 |
| `TreePlacementComponent` | 树型 ID、候选点、最小/最大高度 | 树干/树冠的确定性放置 |
| `TreeGrowthComponent` | 当前阶段、摇摆/成长计时 | 只用于世界生成后持续增长，不混入一次性放置 |
| `OreVeinComponent` | 矿物 ID、矿脉游标、密度、深度带 | 记录矿脉生成状态 |
| `BiomeSurfaceComponent` | 生物群系 ID、替换/墙体策略 | 地表生物群系和感染扩散的输入 |

#### 液体组件

| 组件 | 关键字段 | 不变量 |
| --- | --- | --- |
| `LiquidSourceComponent` | 坐标、液体类型、初始量、来源 | 来源只产生工作项，不直接改 Tile |
| `LiquidWorkItemComponent` | 坐标、邻居方向、剩余量、序号 | 每个工作项可去重、可限额、可重放 |
| `LiquidBoundaryComponent` | 区域边界、封闭/溢出规则 | 液体不能绕过世界边界或受保护结构 |
| `LiquidMergeComponent` | 两种液体及合并结果 | 合并规则来自定义表，不写死在传播循环 |

### 3.2 命令、事件和快照

按当前目录规范，建议增加以下文件；名称是执行提案中的稳定契约，具体字段需在对应切片
的红测试中冻结：

```text
src/Terraria.Dome.Simulation/
  WorldGeneration/
    Components/
      WorldGenerationStateComponent.cs
      WorldSeedComponent.cs
      WorldBoundsComponent.cs
      TerrainProfileComponent.cs
      WorldRuleSnapshotComponent.cs
      GenerationCursorComponent.cs
    Definitions/
      TerrainDefinition.cs
      OreDefinition.cs
      StructureDefinition.cs
      TreeDefinition.cs
      LiquidDefinition.cs
    Systems/
      WorldGenerationStageSystem.cs
      TerrainBaseSystem.cs
      CaveCarvingSystem.cs
      BiomeSurfaceSystem.cs
      OrePlacementSystem.cs
      StructurePlacementSystem.cs
      TreePlacementSystem.cs
      LiquidSourceSystem.cs
      LiquidPropagationSystem.cs
      TileFrameSystem.cs
      WorldGenerationValidationSystem.cs
  World/
    Components/
      TileMutationIntentComponent.cs
      TileProtectionComponent.cs
      LiquidSourceComponent.cs
      LiquidWorkItemComponent.cs
    Commands/
      LiquidChangeCommand.cs
      StructurePlacementCommand.cs
      TileFrameCommand.cs
    Systems/
      TileChangeCommitSystem.cs
      LiquidChangeCommitSystem.cs
```

现有 `World/WorldGrid.cs`、`WorldTile.cs`、`TileChangeCommand.cs` 不重命名，先通过适配保持
已有调用方和 V1456 section 投影稳定。`WorldGenerationPipeline` 在第一阶段保留为编排器，
后续只负责建立实体、调度阶段和返回最终 snapshot，不再持有各类生成算法。

事件只表达已发生的事实，例如 `TileChangedEvent`、`StructurePlacedEvent`、
`LiquidMergedEvent`。命令表达尚未提交的意图，不能用事件绕过提交系统。

## 4. 系统顺序与数据流

### 4.1 一次性世界生成顺序

系统注册必须显式写入 `WorldGenerationSystemOrder`，禁止依赖反射、目录枚举或类型名排序。

1. `WorldGenerationStageSystem`：验证 request、创建世界单例组件、初始化 generation ID 和
   cursor。
2. `TerrainBaseSystem`：生成海平面、土层、岩层和底层基础 Tile；只写
   `TileMutationIntentComponent` 或 `TileChangeCommand`。
3. `CaveCarvingSystem`：按 section 处理洞穴、隧道、空腔和受保护区域；不能覆盖已提交的
   结构保护 footprint。
4. `BiomeSurfaceSystem`：处理沙漠、雪地、丛林、感染、蘑菇等地表规则；不能直接调用旧
   `Main` 或 `TileID.Sets` 静态数组，统一读取 `BiomeSurfaceDefinition`。
5. `OrePlacementSystem`：按深度带和矿物定义放置矿脉；冲突时使用明确的优先级，不依赖遍历
   顺序。
6. `StructurePlacementSystem`：先计算 footprint，再提交结构 Tile、墙和关联对象；失败时
   输出结构失败事件和原因，不能半放置后静默继续。
7. `TreePlacementSystem`：先查树根候选点和空间，再生成树干/树冠/墙体命令；一次性世界
   生成中的树生长不得调用运行时 `UpdateWorld`。
8. `LiquidSourceSystem`：把水、熔岩、蜂蜜、微光等来源转换为液体工作项。
9. `LiquidPropagationSystem`：按固定优先队列和预算传播液体，生成液体变更命令；合并时
   产生显式 `LiquidMergeCommand` 或事件。
10. `TileFrameSystem`：对受影响 section 计算 frame/斜坡/墙体连接，输出 frame 命令；它
    只能读取“提交前快照 + 待提交变更”，不在遍历中修改原数组。
11. `TileChangeCommitSystem`：按 `Sequence, X, Y, Kind` 提交 Tile、墙体和液体变更，更新
    section version，清空命令缓冲。
12. `WorldGenerationValidationSystem`：检查越界、重叠 footprint、未消费工作项、非法液体
    量、随机游标和阶段完成状态，生成不可变 `WorldGridSnapshot`。

### 4.2 世界生成后的运行时更新

`WorldGen.UpdateWorld` 中的持续行为不应塞进一次性生成流水线，单独按 tick 调度：

```text
WorldRuleSnapshot
  -> GrassSpreadSystem / WallSpreadSystem / TreeGrowthSystem
  -> LiquidSourceSystem / LiquidPropagationSystem
  -> TileFrameSystem
  -> TileChangeCommitSystem
  -> EnvironmentChangeEvent
```

落体、天气、NPC 生成、事件 Boss、音效和网络广播分别属于 `WorldObjects`、`Npc`、环境或
Server 层；本提案只为它们留下事件/快照接口，不把它们重新塞回 `WorldGen` ECS 领域。

## 5. 旧职责到新系统的迁移映射

| 旧入口/模式 | 新组件/系统 | 第一阶段处理方式 | 完成条件 |
| --- | --- | --- | --- |
| `WorldGen.SecretSeed.*` | `WorldSeedComponent`、`WorldRuleSnapshotComponent` | 先建立 typed snapshot，保留旧 seed 解析适配器 | 同一 seed/variant 生成相同 fingerprint |
| 基础地表填充 | `TerrainBaseSystem` | 从现有 `WorldGenerationPipeline` 拆出 | 基础层 section 快照与旧 oracle 对齐 |
| `TileRunner`、`Caverer` | `CaveCarvingSystem`、`CaveCarvingComponent` | 先迁移单一 cave profile | 洞穴 mask、越界和 protected footprint 测试通过 |
| 生物群系地表变换 | `BiomeSurfaceSystem`、`BiomeSurfaceComponent` | 每个 biome 一个可重放 profile | 同一输入下变更序列稳定、无跨 biome 越界 |
| `TileID.Sets.Ore` 和矿脉逻辑 | `OreDefinition`、`OrePlacementSystem` | 先迁移 1 种矿物，再扩展 registry | 矿脉数量、深度带、冲突策略可测 |
| 地牢/房屋/传送器/雕像 | `StructureDefinition`、`StructurePlacementSystem` | 先迁移 footprint + 事务放置 | 放置成功或完全回滚，不出现半结构 |
| `GrowTreeSettings`、`AttemptToGrowTree...` | `TreeDefinition`、`TreePlacementSystem` | 先迁移普通树，再宝石/特殊树 | 树根、树高、树冠 footprint 与 frame 稳定 |
| `PlaceLiquid`、`EmptyLiquid` | `LiquidChangeCommand`、`LiquidChangeCommitSystem` | 将直接写 Tile 改成命令 | 量/类型/边界和版本递增正确 |
| `Liquid.QuickWater`、`WaterCheck` | `LiquidWorkItemComponent`、`LiquidPropagationSystem` | 先实现有预算的 BFS/优先队列 | 重放结果一致，不出现无限队列 |
| `KillTile`、`ReplaceTile`、`SlopeTile` | `TileMutationIntentComponent`、`TileChangeCommitSystem` | 先迁移纯状态变更 | 冲突、保护区和提交顺序有明确测试 |
| `TileFrame`、`SquareTileFrame` | `TileFrameSystem`、`TileFrameCommand` | 与 Tile 写入解耦 | frame 只依赖提交前快照和变更集 |
| `NetMessage.SendTileSquare` | Server/Protocol 投影 | 只消费 section snapshot | Simulation 无协议引用，V1456 帧序不变 |

## 6. 分阶段执行计划

每个阶段都必须形成“源码映射 -> 最小测试 -> 实现 -> 证据 -> 下一阶段”的闭环。阶段未通过时，
不得物理删除或禁用旧入口。

### 阶段 0：事实源清点与回放基线

**写集**

- Create: `docs/plans/2026-08-18-worldgen-ecs-migration-execution-proposal.md`（本文件）
- Create: `docs/worldgen/worldgen-source-inventory.json`
- Create: `docs/worldgen/worldgen-method-map.md`
- Create: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
- Create: `Test/Terraria.Dome.WorldGeneration.Verification/Terraria.Dome.WorldGeneration.Verification.csproj`

**动作**

1. 以 Roslyn 或编译器语法树扫描 `WorldGen.cs`，记录类型、方法、静态字段、`Main`/`Tile`/
   `Liquid`/`NetMessage` 引用和所属候选域。
2. 固定源文件大小、SHA-256、行数和扫描工具版本；不把物理删除目录的缺失文件当成迁移
   结果。
3. 为最小世界输入记录 `WorldGridSnapshot`、section version、Tile 变更序列和液体变更序列。

**验收门槛**

- inventory 中每个公共/内部入口都有 `Unmapped`、`Mapped` 或 `OutOfScope` 状态。
- `Unmapped` 允许存在，但必须列出下一阶段；不能声称“WorldGen 已完成迁移”。
- 事实源 hash 与回放输入写入证据文件，避免后续源文件漂移。

### 阶段 1：冻结世界输入和单一提交边界

**写集**

- Create: `src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldGenerationStateComponent.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldSeedComponent.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldBoundsComponent.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/Components/GenerationCursorComponent.cs`
- Create: `src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationRequest.cs`
- Modify: `src/Terraria.Dome.Simulation/World/WorldGrid.cs`
- Modify: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`

**动作**

1. 将 seed、规则、尺寸和 spawn 输入转成不可变 request/snapshot。
2. 让所有生成系统只能追加命令，不允许直接调用 `TrySetTile`；保留 `WorldGrid.TrySetTile`
   给内部提交器和已存在兼容调用方使用，逐步收紧可见性。
3. 将 Tile、墙体和液体提交排序规则写成稳定契约。

**验收门槛**

- 同一 request 重放两次，snapshot 和 command fingerprint 完全一致。
- 任意命令越界、重复序号、非法液体量都被拒绝。
- section version 只在提交成功后递增。

### 阶段 2：基础地形、洞穴和生物群系

**写集**

- Create: `TerrainDefinition.cs`、`TerrainProfileComponent.cs`
- Create: `TerrainBaseSystem.cs`、`CaveCarvingSystem.cs`、`BiomeSurfaceSystem.cs`
- Create: `CaveCarvingComponent.cs`、`BiomeSurfaceComponent.cs`
- Modify: `WorldGenerationPipeline.cs`
- Modify: `Test/Terraria.Dome.WorldMechanics.Verification/Program.cs`

**动作**

1. 把当前 `GenerateSurfaceAndGround` 改为 `TerrainBaseSystem` 的最小实现，保留现有出生点
   清理测试。
2. 用 profile/seed 驱动 cave carve，不先迁移所有 secret seed 分支。
3. 每个 profile 输出独立变更集和 section fingerprint，禁止系统间共享可变随机对象。

**验收门槛**

- 基础地形、出生点清理、洞穴越界和保护区域测试通过。
- 未支持的 biome/secret seed 明确返回 `UnsupportedRule`，不得静默使用默认地形。
- V1456 section 编码仍消费 immutable snapshot，协议项目不引用生成系统。

### 阶段 3：矿物、树和结构事务

**写集**

- Create: `OreDefinition.cs`、`OreVeinComponent.cs`、`OrePlacementSystem.cs`
- Create: `TreeDefinition.cs`、`TreePlacementComponent.cs`、`TreePlacementSystem.cs`
- Create: `StructureDefinition.cs`、`StructureFootprintComponent.cs`、
  `StructurePlacementComponent.cs`、`StructurePlacementSystem.cs`
- Create: `StructurePlacementCommand.cs`
- Modify: `WorldTile.cs`（仅在需要保留 frame/wall 语义时增量修改）
- Modify: `Test/Terraria.Dome.WorldObjects.Verification/Program.cs`

**动作**

1. 先迁移一个普通矿脉、一个普通树和一个最小房屋 footprint，验证定义、候选位置、冲突
   检查和事务提交。
2. 结构放置采用 prepare/commit 两步：prepare 只读检查，commit 一次性产生 Tile/墙/对象
   命令；失败时丢弃整批命令。
3. 树与矿物都必须经过 `TileProtectionComponent`，不得覆盖出生点、地牢或已提交结构。

**验收门槛**

- 结构放置只有“完整成功”或“无变更失败”两种结果。
- 同一 seed、定义版本和 origin 得到相同 footprint 和 Tile 序列。
- 树根/树冠、矿脉深度、结构占用区均有越界和冲突测试。

### 阶段 4：液体来源、传播和合并

**写集**

- Create: `LiquidDefinition.cs`、`LiquidSourceComponent.cs`、`LiquidWorkItemComponent.cs`
- Create: `LiquidSourceSystem.cs`、`LiquidPropagationSystem.cs`、`LiquidMergeComponent.cs`
- Create: `LiquidChangeCommand.cs`、`LiquidChangeCommitSystem.cs`
- Modify: `WorldGrid.cs`、`WorldTile.cs`
- Modify: `Test/Terraria.Dome.WorldMechanics.Loopback.Verification/Program.cs`

**动作**

1. 将 `PlaceLiquid`/`EmptyLiquid` 变成带来源和 sequence 的液体命令。
2. 用有界优先队列替代全局 `Liquid.QuickWater`；每 tick 消费固定预算，cursor 可持久化。
3. 将水/熔岩/蜂蜜/微光合并规则放入 `LiquidDefinition` 或规则表，合并结果通过事件/命令
   输出，不在邻居循环中直接改 Tile。

**验收门槛**

- 液体传播在相同输入和预算下可重放；队列不会无限增长。
- 世界边界、结构保护、非法类型和过量值均有拒绝测试。
- 液体改变只影响对应 section version，且不会绕过 Tile 提交系统。

### 阶段 5：Tile frame、运行时环境和兼容投影

**写集**

- Create: `TileFrameSystem.cs`、`TileFrameCommand.cs`
- Create: `WorldGenerationValidationSystem.cs`
- Modify: `DefaultWorldEnvironmentConvergence.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456` 中只读 section 编码入口
- Modify: `Test/Terraria.Dome.World.Protocol.Verification/Program.cs`

**动作**

1. 将 `TileFrame`/墙体连接/斜坡计算置于提交前的独立系统。
2. 将持续世界更新从一次性生成器剥离，采用 `WorldRuleSnapshot -> environment systems ->
   commands -> commit` 的 tick 流程。
3. 让 Server/Protocol 只消费 `WorldSectionSnapshot`，不把旧 `Main` 或 `WorldGen` 引用带入
   协议程序集。

**验收门槛**

- frame 计算不改变 Tile 内容，只产生 frame 命令。
- 世界生成完成后，运行时系统可以从 snapshot 继续推进，不重复执行一次性阶段。
- V1456 进入世界的 frame 顺序和 section 编码回归通过。

### 阶段 6：旧入口收缩、差分回放和删除闸门

**写集**

- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs`
- Modify: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
- Create: `docs/worldgen/worldgen-parity-report.md`
- Create: `docs/worldgen/worldgen-deletion-gate.json`

**动作**

1. 将旧 `WorldGen` 入口改成只读兼容适配器，适配器不得新增玩法逻辑。
2. 对固定 seed、world size、secret seed、spawn、biome 和液体边界执行差分回放。
3. 逐条关闭 inventory 中的 `Mapped` 项；未映射项维持显式失败，而不是猜测兼容行为。

**删除闸门**

只有以下条件全部成立时，才允许从编译图移除旧入口：

- inventory 中没有影响当前目标模式的 `Unmapped` 条目。
- 生成 snapshot、Tile command、液体 command 和 section fingerprint 在目标 oracle 上通过。
- 至少一次断电/重启 cursor 恢复回放成功。
- WorldMechanics、WorldObjects、World.Protocol 和 loopback 验证项目串行通过。
- `WorldGen.cs` 原始 hash、parity 报告和迁移版本已归档。
- Server/Protocol/Simulation 的引用扫描确认没有 `Terraria.WorldGen`、`Main.tile` 或旧
  `Liquid` 依赖泄漏。

## 7. 测试和证据策略

### 7.1 必须新增的验证类别

| 类别 | 最小断言 |
| --- | --- |
| 确定性 | 同一 request 的 snapshot、command、section fingerprint 一致 |
| 阶段顺序 | 乱序执行被拒绝；阶段只能单向推进 |
| 边界 | 负坐标、最大坐标、section 边界和空世界请求有明确结果 |
| 冲突 | 结构/树/矿物/洞穴覆盖冲突按优先级确定，不依赖 hash 遍历顺序 |
| 事务 | 结构或批量 Tile 失败时无部分提交 |
| 液体 | 传播预算、合并、边界、队列去重和恢复可重放 |
| 快照 | 提交前后 snapshot 不可变，section version 只因实际变更递增 |
| 协议 | V1456 section 编码不感知 ECS entity，只消费 immutable snapshot |
| 回归 | 现有 WorldMechanics、WorldObjects、World.Protocol、Loopback 验证全通过 |

### 7.2 建议命令

从仓库根目录串行运行，统一使用仓库指定 SDK 和 `UseSharedCompilation=false`：

```powershell
dotnet build src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj `
  -p:UseSharedCompilation=false

dotnet run --project Test/Terraria.Dome.WorldGeneration.Verification/ `
  -p:UseSharedCompilation=false

dotnet run --project Test/Terraria.Dome.WorldMechanics.Verification/ `
  -p:UseSharedCompilation=false

dotnet run --project Test/Terraria.Dome.WorldObjects.Verification/ `
  -p:UseSharedCompilation=false

dotnet run --project Test/Terraria.Dome.World.Protocol.Verification/ `
  -p:UseSharedCompilation=false

dotnet run --project Test/Terraria.Dome.World.Loopback.Verification/ `
  -p:UseSharedCompilation=false
```

执行实现阶段时，所有证据写入 `Build/` 或 `docs/worldgen/`，禁止写入 `src/` 旁边的临时
二进制、obj、测试结果或源生成输出。

### 7.3 代码扫描闸门

每个阶段完成后执行以下只读扫描：

```powershell
rg -n "Main\.tile|Main\.|NetMessage|Terraria\.Audio|Liquid\.QuickWater" `
  src/Terraria.Dome.Simulation/WorldGeneration `
  src/Terraria.Dome.Simulation/World

rg -n "WorldGen|Terraria\.WorldGen" `
  src/Terraria.Dome.Simulation `
  src/Terraria.Dome.Protocol.V1456 `
  src/Terraria.Dome.Server
```

允许扫描结果出现在兼容适配器或注释化 provenance 文件中；不允许出现在新生成系统的执行
路径中。

## 8. 风险、回滚和暂停条件

### 高风险项

- `WorldGen` 的静态随机流、secret seed 和全局字段之间存在未显式记录的顺序依赖。
- `Tile` 同时携带 active/type/wall/liquid/frame 等状态，当前 `WorldTile` 仍是精简模型，不能
  在没有字段 parity 的情况下声称完整兼容。
- 结构、树和矿物可能隐式依赖 Tile frame、墙体、实体对象或掉落物；只迁移 Tile 类型会产生
  半完成世界。
- 液体传播是长链状态机，若没有预算、去重和 cursor，会导致不可终止 tick 或不确定结果。
- Version4 目录标题包含“物理删除了某些文件”，因此部分旧依赖可能在事实源之外；缺失依赖
  必须标记为 `MissingSource`，不能用猜测补齐。

### 回滚边界

- 每个阶段以独立提交落地；失败只回滚当前阶段的新增组件/系统和测试，不触碰用户已有改动。
- 不删除 `WorldGrid`、section snapshot 或现有验证项目；它们是协议和世界边界的稳定契约。
- 旧 `WorldGen` 适配器在删除闸门前保持可编译，必要时可切回旧入口进行差分回放。

### 必须暂停的情况

- 发现目标行为依赖缺失的 Version4 源码，且没有可验证的 runtime/oracle 证据。
- 同一输入的旧实现或 oracle 本身不确定，无法区分迁移错误和源行为。
- 任何系统需要通过静态全局、协议对象或可变原始 Tile 数组绕过命令提交。
- 差分失败但无法定位到单一阶段；此时禁止扩大迁移范围，应先增加最小回放 fixture。

## 9. 交付物与完成定义

本提案完成不等于 `WorldGen` 已迁移。代码迁移完成的最低交付物为：

1. 分阶段组件/系统实现和明确的系统顺序注册。
2. `WorldGenerationRequest -> snapshot -> systems -> commands -> commit -> snapshot` 的可重放
   闭环。
3. 世界生成、Tile 改变和液体变更均有来源、序号、边界和失败原因。
4. 结构、树、矿物、洞穴和液体不再直接写旧 `Main`/`Liquid` 全局状态。
5. WorldGrid section version、V1456 section 编码和 Server 可见性回归通过。
6. source inventory、parity report、deletion gate 和所有验证命令的真实输出。
7. 未支持的 secret seed、结构或液体行为显式列出，不用“默认实现”伪装完成度。

建议的首个可交付切片是“阶段 0 + 阶段 1 + 阶段 2 的基础地形/单一洞穴 profile”。它能先
验证 ECS 边界、确定性命令和 section snapshot，而不把 1.90 MB 旧文件一次性搬进新架构。

