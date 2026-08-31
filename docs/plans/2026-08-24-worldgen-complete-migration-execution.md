# WorldGen 完整迁移执行计划

> **For Codex:** REQUIRED SUB-SKILL: Use `executing-plans` to implement this plan task-by-task.
> 每个任务完成后必须刷新 `Build/diagnostics/` 证据；不得把局部验证的绿色结果表述为完整
> WorldGen parity。

**目标：** 将 Version4 `Terraria/WorldGen.cs` 中仍为 `Unmapped` 的世界生成责任迁移到
`Terraria.Dome.Simulation` 的快照、系统和命令提交流水线，使固定输入下的完整生成结果与旧基线
通过可重复的差分门禁，并最终允许移除旧 WorldGen 编译入口。

**架构：** `WorldGrid` 是 Simulation 的权威稠密 Tile 存储；WorldGen 系统只读取不可变快照
和冻结的生成请求，通过 `TileChangeCommand`、`LiquidChangeCommand`、结构命令和 frame 命令
表达意图。`TileChangeCommitSystem` 和 `LiquidChangeCommitSystem` 是唯一的提交边界；Server
和 Protocol 只消费提交后的不可变快照，不参与生成算法。

**技术栈：** .NET 10、C#、Arch ECS、Roslyn source inventory、V1456 world-section projection、
deterministic serial MSBuild、WLD v319 oracle/differential replay。

---

## 1. 当前基线和完成定义

### 1.1 当前事实基线

事实源是 `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs`。当前
`docs/worldgen/worldgen-source-inventory.json` 的机器可读统计为：

| 指标 | 当前值 |
| --- | ---: |
| Legacy 方法 | 684 |
| Legacy 字段 | 233 |
| `Partial` 方法 | 125 |
| `Unmapped` 方法 | 559 |
| `Unmapped` 字段 | 233 |
| Legacy 文件大小 | 1,901,533 bytes |
| Legacy 文件行数 | 73,355 |
| 主要全局引用 | `Main`、`Tile`、`Liquid`、`NetMessage` |
| 当前删除门禁 | `canRemoveLegacyWorldGen=false` |

当前完整差分基线记录在 `docs/worldgen/worldgen-deletion-gate.json` 和
`docs/worldgen/worldgen-parity-report.md`：

- 5,040,000 个 Tile 中有 3,190,404 个差异。
- Legacy 有 2,640,402 个 active Tile，当前 ECS fixture 有 3,671,879 个 active Tile。
- 扩展状态仍有至少 1,046,843 个 Tile 不一致。
- 当前 bounded verifier 可以通过，但完整 differential 仍失败。
- 旧 WorldGen 入口在所有目标方法映射、完整差分和删除审计通过前必须保留。

### 1.2 “完整迁移”定义

WorldGen 只有同时满足以下条件才可以标记为 `Complete`：

1. `worldgen-source-inventory.json` 中每个 ServerRelevant 方法和字段都有明确的
   `Complete`、`ReplacedWithEvidence` 或有审批的 `Excluded` 状态；不得留下无理由的
   `Unmapped`。
2. 每个责任族都有 Legacy source anchor、Simulation authority、命令/提交边界和执行验证器。
3. 默认种子、secret seed、world size、difficulty、hardmode、spawn 和 world metadata 都是
   显式输入，不从 `Main` 或旧静态字段隐式读取。
4. 生成结果在固定输入下可重放；连续执行和从 stage cursor 恢复的 snapshot fingerprint、
   section versions、Tile/Wall/Liquid 状态一致。
5. 完整 WLD oracle differential 不再有未解释差异；允许的投影差异必须写入差异分类表并有
   source-backed reason，不能用“兼容投影”掩盖缺失的服务器语义。
6. WorldGen 删除门禁、Simulation forbidden-dependency scan、完整相关验证器和串行 Release
   build 全部通过。

### 1.3 不计为完成的结果

- 仅新增同名静态 helper、把 God Object 按方法名拆成多个 God Object。
- 仅复制旧 `WorldGen.cs` 的方法体或保留 `Main.tile` 原地写入。
- 仅有 method catalog、协议字段或 API 声明，没有权威状态和执行证据。
- 仅通过局部 query 单测、编译或 bounded replay。
- 仅证明 WLD 导入可以保留旧 Tile；导入 parity 不等于生成 parity。
- 删除旧文件后再补验证，或用物理删除掩盖 `Unmapped` 责任。

## 2. 不可违反的边界

### 2.1 写入边界

生产实现只允许写入：

- `src/Terraria.Dome.Simulation/WorldGeneration/`
- 必要的 `src/Terraria.Dome.Simulation/World/`、`Commands/`、`Snapshots/` 适配点
- `Test/Terraria.Dome.WorldGeneration.Verification/` 和明确归属的 focused verifier
- `docs/worldgen/`、`docs/research/`、`Build/diagnostics/`

以下内容在删除门禁通过前只读：

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs`
- `D:\TRbackup\无任何删减通过编译\TerrariaServer.csproj` 及其旧源码
- 任何旧 `Main.cs`、`Tile.cs`、`Liquid.cs`、`NetMessage.cs` 运行时入口

### 2.2 Simulation 依赖边界

WorldGeneration 下的生产代码不得直接引用或访问：

- `Terraria.Main`、`Main.tile`、`Main.rand`、`Main.maxTilesX` 等旧全局状态。
- `NetMessage`、`MessageBuffer`、`RemoteClient`、网络发送副作用。
- 渲染、音效、客户端 UI、旧 `Liquid.QuickWater` 全局队列。
- 通过静态数组读取未冻结的 Tile/Wall registry。

需要兼容旧数据的字段必须先转换为 immutable definition、`WorldRuleSnapshot` 或显式输入
契约，再由 Simulation 使用。

### 2.3 随机和序列边界

- 所有随机行为使用带版本的 `GenerationRandomState`，以 `(seed, stream, stage, cursor)`
  定位，不使用共享全局 RNG。
- 命令序列、section 版本和 stage cursor 单调递增；溢出在入队前拒绝。
- 同一 Tick 内的冲突使用显式 `priority, source, sequence, x, y, kind` 排序，不能依赖
  Arch 枚举顺序、目录顺序或线程时序。

## 3. 冻结的公共契约

这些契约必须在任何大批量行为迁移前先落地，并由一个独立任务拥有。后续任务只能扩展字段，
不能各自发明另一套状态和提交语义。

### 3.1 生成请求

建议稳定形状：

```csharp
public sealed record WorldGenerationRequest(
  int Seed,
  WorldSeedVariant SeedVariant,
  int Width,
  int Height,
  int SpawnX,
  int SurfaceY,
  int RockLayerY,
  WorldDifficulty Difficulty,
  bool Hardmode,
  WorldRuleSnapshot Rules,
  int RandomStreamVersion);
```

实际类型名必须先检查当前代码；如果已有类型可表达同样契约，应扩展现有类型而不是创建
平行 request。所有字段都必须在生成开始前校验和冻结。

### 3.2 阶段和 cursor

阶段顺序必须由显式枚举和注册表维护：

```text
Prepare -> Terrain -> Cave -> Biome -> Ore -> Structure -> Tree
        -> Liquid -> Frame -> Validate -> Complete
```

cursor 至少包含：`GenerationId`、`Stage`、`SectionX`、`SectionY`、`NextSequence`、
`RandomStreamState`、`ProtectedFootprintRevision`。阶段只能前进，恢复不能重新执行已经提交的
命令。

### 3.3 命令和提交

所有生成输出都必须进入 typed command：

```text
TileChangeCommand
LiquidChangeCommand
StructurePlacementCommand
TileFrameCommand
WorldObjectPlacementCommand
```

每条命令必须携带来源、序列、坐标、预期 revision 或冲突策略。提交器负责边界检查、保护
footprint、冲突排序、Tile/Wall/Liquid 写入和 section version 更新。系统不得自行写入
`WorldGrid`。

### 3.4 快照和差分

差分必须在 immutable `WorldGridSnapshot` 上运行，至少比较：

```text
TileType, IsActive, FrameX, FrameY, WallType, LiquidKind, LiquidAmount,
Wire, Wire2, Wire3, Wire4, Actuated, HalfBrick, Slope,
TileColor, WallColor, InvisibleBlock, InvisibleWall
```

每个差异都要关联 `stage, source anchor, responsible system, reason`。没有分类原因的差异
视为失败。

## 4. 执行阶段和任务

每个 Task 都应独立提交；推荐提交消息使用 `feat(worldgen): ...`、`test(worldgen): ...`
或 `docs(worldgen): ...`。执行者不得把两个责任族合并成一个无法回滚的大提交。

### Task 0：建立清洁基线和证据目录

**目的：** 锁定旧源码 hash、当前 source inventory、当前 ECS replay、完整 differential 和
当前验证器状态，避免把历史结果当作新结果。

**Files:**

- Read: `docs/worldgen/worldgen-source-inventory.json`
- Read: `docs/worldgen/worldgen-deletion-gate.json`
- Read: `docs/worldgen/worldgen-parity-report.md`
- Test: `Test/Terraria.Dome.WorldGeneration.Verification/`
- Create: `Build/diagnostics/worldgen-complete/task-0-baseline/<timestamp>/`

**Steps:**

1. 生成新的 source inventory，记录 `WorldGen.cs` SHA-256、方法/字段数量和状态分布。
2. 运行 bounded WorldGeneration verifier，输出独立日志和结果 JSON。
3. 运行完整 4200 x 1200 oracle differential，输出 tile 和 extended-state mismatch。
4. 保存当前 `canRemoveLegacyWorldGen` 和阻断原因快照。
5. 将本次 artifact 路径写入 `docs/worldgen/worldgen-parity-report.md`，不覆盖历史证据。

**Commands:**

```powershell
dotnet run --project Test\Terraria.Dome.WorldGeneration.Verification\Terraria.Dome.WorldGeneration.Verification.csproj `
  -c Release -p:UseSharedCompilation=false `
  -- --evidence-dir "$pwd\Build\diagnostics\worldgen-complete\task-0-baseline\<timestamp>"
```

完整 differential 命令必须沿用 verifier 当前支持的 `--legacy-differential`、
`--legacy-oracle` 和相关绝对路径参数；实际命令从 verifier 的 `--help` 或当前运行脚本复制，
不要手写一套不同的比较器。

**验收：** 有新的 baseline artifact；若基线数字变化，先解释变化，再开始 Task 1。此任务
失败时不得修改生产代码。

### Task 1：冻结生成请求、阶段、随机流和 cursor

**目的：** 让后续系统拥有同一套输入和恢复语义，消除隐式 `Main` 状态。

**Files:**

- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationRequest.cs`
- Modify/Create: `src/Terraria.Dome.Simulation/WorldGeneration/GenerationRandomState.cs`
- Modify/Create: `src/Terraria.Dome.Simulation/WorldGeneration/GenerationCursor.cs`
- Modify/Create: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationStage.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs`
- Test: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`

**Steps:**

1. 先写 request 缺失字段、非法 bounds、非法层高、负 seed stream 和 cursor 回退的失败用例。
2. 运行 verifier，确认失败原因来自缺失契约而不是测试 fixture 错误。
3. 实现不可变 request 校验、阶段单调性和随机流版本化。
4. 实现 cursor 序列化/恢复，禁止恢复后重新提交已完成阶段的命令。
5. 验证同一 request 连续运行两次 fingerprint 一致。
6. 验证 Terrain checkpoint 后恢复 Cave，与 uninterrupted run 的 Tile/Wall/Liquid 状态一致。

**验收：** 所有生成系统都能从 request/cursor 得到输入；禁止读取旧全局字段；生成阶段和
随机流状态出现在 evidence JSON 中。

### Task 2：统一 Tile、Wall、Liquid 命令和唯一提交边界

**目的：** 先把所有后续行为的写入协议固定下来，避免每个系统直接修改 Grid。

**Files:**

- Modify: `src/Terraria.Dome.Simulation/Commands/TileChangeCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/World/Systems/LiquidChangeCommitSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/*Command*.cs`
- Test: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldMechanics.Verification/Program.cs`

**Steps:**

1. 写越界、重复 sequence、protected footprint 覆盖、expected revision 不匹配和非法液体
   合并的失败用例。
2. 运行红测试并记录现有提交器的行为差异。
3. 为 Tile、Wall、Liquid、Structure、Frame 增加统一 source/priority/sequence 字段。
4. 将排序和冲突策略集中在提交器，不允许系统本地排序后直接写入。
5. 验证提交原子性：一批结构命令失败时不得留下部分 Tile。
6. 验证 section version 只在实际状态变化时递增。

**验收：** 生成系统只输出命令；只有 commit system 可以变更 `WorldGrid`；现有协议 section
投影继续只读取 immutable snapshot。

### Task 3：地形基础层、边界和洞穴主链

**目的：** 让默认世界拥有与旧基线一致的地表、土层、岩层、底层和洞穴 carve 主结构。

**Legacy anchors:** `WorldGen.Initialize`、`TerrainPass`、`TileRunner`、`Caverer`、
`WavyCaverer`、`ChasmRunner` 及其 `Main`/`Tile` 访问点。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Systems/Terrain*System.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Systems/Cave*System.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyTileRunner*.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs`
- Test: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
- Evidence: `Build/diagnostics/worldgen-complete/task-3-terrain-cave/<timestamp>/`

**Steps:**

1. 为每个 source anchor 建立 method-map 行和最小输入 fixture。
2. 写 surface/rock/underworld layer 的边界和 protected spawn area 失败用例。
3. 写 cave runner 的 deterministic direction、distance、envelope 和 kill-frame 用例。
4. 实现只读 snapshot 查询、命令输出和固定 stage cursor。
5. 对同一个 seed 运行两次，比较 stage fingerprint 和命令序列。
6. 与 legacy oracle 做局部区域差分，按 `TileType/Active/Frame/Wall/Liquid` 分类。

**验收：** Terrain/Cave 不再依赖 `Main`、`Tile`、`NetMessage`；局部差分中的每个剩余差异
都有 source-backed 分类；不得宣称完整 parity。

### Task 4：Biome、secret seed 和世界规则输入

**目的：** 将地表生物群系、感染、雪地、沙漠、丛林、蘑菇、secret seed 等从旧全局开关
迁移为定义和规则快照。

**Legacy anchors:** `CheckInputForSecretSeed`、`InitializeSecretSeeds`、
`DoSurfaceIs*`、`DoNoSurface*`、`DoWorldIs*`、biome surface passes。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Definitions/*Biome*.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/*SecretSeed*.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Systems/Biome*System.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/WorldRuleSnapshot.cs`
- Test: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`

**Steps:**

1. 建立 secret-seed 输入规范化、候选匹配、非法组合和 unsupported 规则的失败测试。
2. 建立 biome profile 的 surface/rock/liquid/wall 输入 fixture。
3. 实现 profile-driven query 和命令发射；禁止直接读旧 `Main` flag。
4. 记录不迁移的客户端装饰、Color application 和旧加密/随机副作用。
5. 运行默认、secret-seed、hardmode、difficulty 矩阵并保存独立 fingerprints。

**验收：** 每个 secret seed 要么有完整行为证据，要么在 inventory 中明确为
`UnsupportedWithEvidence`；不能默默当成默认世界。

### Task 5：Ore、矿脉、宝石和资源分布

**目的：** 迁移矿物定义、深度带、矿脉传播和资源分布，同时固定旧随机流和冲突优先级。

**Legacy anchors:** `TileRunner` ore paths、`Spread.Gem`、ore tier selection、
world progression checks。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Definitions/Ore*.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Systems/Ore*System.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/GenerationRandomState.cs`
- Test: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`

**Steps:**

1. 写 ore tier、深度带、密度、world progression 和 seed 的参数边界测试。
2. 写矿脉越界、与保护结构冲突、重复 patch 和 sequence 稳定性测试。
3. 实现 definition registry、prepare result 和 deterministic command batch。
4. 比较 ore tile count、active tile、frame 和受保护区域的局部差分。
5. 将未覆盖的 rare/random override 写入方法 map，而不是留在代码注释中。

**验收：** 同 seed 的矿脉命令序列完全一致；不同 world rule 不会共享可变随机状态。

### Task 6：结构、地牢、房屋和多格对象

**目的：** 将结构放置从旧的直接 Tile 操作迁移为 definition + footprint + atomic commit。

**Legacy anchors:** dungeon、house、statue、teleporter、chest、tile-object placement，
以及 `Check*` 多格 footprint 方法族。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Definitions/Structure*.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Systems/Structure*System.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/*ValidationQuery.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldObjects/`
- Test: `Test/Terraria.Dome.Dungeon*.Verification/`
- Test: `Test/Terraria.Dome.WorldObjects.Verification/`
- Evidence: `Build/diagnostics/worldgen-complete/task-6-structures/<timestamp>/`

**Steps:**

1. 先按 footprint 尺寸族清点 method map：1x1、1x2、2x2、2xX、3xN、墙体对象和特殊结构。
2. 为每个尺寸族写 origin/frame/support/wall/bounds 的失败测试。
3. 实现 `StructureDefinition`、footprint、保护区域和预检结果。
4. 实现结构命令批次和原子提交；失败时输出明确 reason，不允许半结构。
5. 将 chest/sign/training dummy 等实体注册和持久化交给 WorldObjects owner，WorldGen 只
   发出 placement intent。
6. 运行 Dungeon、Room、WorldObjects 和 WorldGeneration verifier。

**验收：** 结构 placement 不直接写 Grid；多格对象的 footprint、entity identity、Tile/Wall
状态和 rollback 结果可重放。

### Task 7：树、植被、藤蔓和生长规则

**目的：** 迁移树根候选、树干/树冠、墙体、植被和藤蔓规则，区分一次性生成与运行时生长。

**Legacy anchors:** `GrowTreeSettings`、`AttemptToGrowTreeFromSapling`、tree frame、
vine/grass/plant propagation paths。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Definitions/Tree*.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Systems/Tree*System.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/*Tree*Query.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/*Plant*Query.cs`
- Test: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`

**Steps:**

1. 写 tree type registry、ground/wall suitability、height、canopy clearance 和 neighbor
   frame 的失败用例。
2. 写 cactus、bamboo、oasis/jungle/underwater plant、vine 的 support 和 bounds fixture。
3. 实现 tree/plant definitions、候选查询和 trunk/canopy command batch。
4. 将运行时生长放入单独 system，不得由一次性 generation pipeline 隐式触发。
5. 比较树干/树冠/墙体/plant 的局部差分，并记录随机选择的 stream/cursor。

**验收：** 树和植被不会覆盖 protected footprint；树生长、frame 和 drop side effect 的
未迁移部分有明确 inventory 状态。

### Task 8：Liquid 完整传播、合并和边界

**目的：** 将 `Liquid.QuickWater`、source、传播、合并和边界规则迁移为可暂停、可恢复、可限额
的工作队列。

**Legacy anchors:** `PlaceLiquid`、`EmptyLiquid`、`WaterCheck`、`QuickWater`、liquid merge
and world-generation liquid passes。

**Files:**

- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/LiquidPropagationSession.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Systems/LiquidPropagationSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/World/Systems/LiquidChangeCommitSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Liquid/Definitions/`
- Test: `Test/Terraria.Dome.Liquid.Verification/`
- Test: `Test/Terraria.Dome.Liquid.Loopback.Verification/`
- Evidence: `Build/diagnostics/worldgen-complete/task-8-liquid/<timestamp>/`

**Steps:**

1. 写 liquid type、amount、merge、boundary、source sequence overflow 的失败测试。
2. 写有限预算下的 pause/resume、去重、重放和 section version 测试。
3. 实现 source -> work item -> propagation -> `LiquidChangeCommand` -> commit 链。
4. 验证液体不会越过 world bounds、protected structure 或非法 Tile 状态。
5. 将 Liquid runtime update 与一次性 generation liquid pass 分开。
6. 运行单测、loopback 和 worldgen replay，对比 `LiquidKind/LiquidAmount` 差异。

**验收：** 无旧全局 liquid queue；同一 source/cursor 产生相同工作项和提交序列；重启恢复
不会重复传播或丢失已提交液体。

### Task 9：TileFrame、墙体 frame、斜坡和 extended state

**目的：** 关闭当前差分中大量 Frame、Wall、Slope、Wire、Actuated 等扩展状态差异。

**Legacy anchors:** `TileFrame`、`WallFrame`、`SquareTileFrame`、`SquareWallFrame`、
`ResetUVCache`、slope/half-brick/wire/actuator branches。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/Systems/TileFrameSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/WorldGeneration/*Frame*Query.cs`
- Modify: `src/Terraria.Dome.Simulation/World/WorldTile.cs`
- Modify: `src/Terraria.Dome.Simulation/Commands/`
- Test: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldMechanics.Verification/Program.cs`

**Steps:**

1. 建立 frame 请求的固定九点拓扑、范围遍历顺序和 source provenance 测试。
2. 建立 inactive sentinel、frame X/Y、wall、slope、half-brick、wire 和 actuator fixture。
3. 实现 mutation 后的 frame request 收集和独立 frame commit。
4. 只使用 immutable neighbor snapshot；禁止 frame system 读写可变 Tile 同步迭代。
5. 对完整 oracle 输出 extended-state mismatch 分类，并逐类归因。
6. 重新运行结构、树、液体和 WorldGen verifier，确保 frame 修复没有破坏主体 Tile。

**验收：** `FrameX/FrameY/WallType/Slope/Wire/Actuated` 的剩余差异均有分类；没有通过
“忽略字段”降低 mismatch。

### Task 10：世界运行时维护与生成后边界

**目的：** 将旧 `WorldGen.UpdateWorld`、草/墙/树增长、落体和天气相关职责与一次性生成
分离，明确哪些属于 Main Tick、哪些属于 WorldGen completion。

**Files:**

- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/World/Systems/*GrowthSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/World/WorldRuntimeSnapshot.cs`
- Modify: `docs/migrations/main-server-responsibility-ledger.md`
- Test: `Test/Terraria.Dome.WorldRules.Verification/`
- Test: `Test/Terraria.Dome.WorldClock.Verification/`

**Steps:**

1. 将 WorldGen inventory 中 `WorldRuntime` 域方法单独列出，不得为追求数字塞回 generation。
2. 为每个方法确定 owner：WorldGeneration、Simulation world runtime、Server tick 或
   ClientOnly/Excluded。
3. 为迁移到 runtime 的职责建立 snapshot/system/commit 测试。
4. 验证 generation 完成后 runtime system 不会重复执行一次性 pass。
5. 更新 inventory 的 `CurrentOwner`、`DeferredReason` 和 verifier 字段。

**验收：** WorldGen 完成边界和 Main Tick 完成边界清晰；没有循环依赖或隐式二次生成。

### Task 11：完整 oracle differential 和差异归因

**目的：** 从“局部 bounded verifier 通过”升级为全世界、全字段、可解释的旧基线差分。

**Files:**

- Modify: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
- Modify: `docs/worldgen/legacy-worldgen-differential.json`
- Modify: `docs/worldgen/legacy-worldgen-oracle.json`
- Modify: `docs/worldgen/worldgen-parity-report.md`
- Create: `docs/worldgen/worldgen-difference-classification.csv`
- Evidence: `Build/diagnostics/worldgen-complete/task-11-differential/<timestamp>/`

**Steps:**

1. 固定至少两次 Legacy oracle run，确认 WLD 输出 fingerprint 一致。
2. 用同一 seed、size、spawn、surface、rock layer、difficulty、hardmode 运行 ECS。
3. 比较全量 Tile 和 extended state，不使用只比较 active Tile 的缩减模式作为最终门禁。
4. 按 section、region、stage、field、source anchor、owner system 分类 mismatch。
5. 对每个非零差异生成可审计记录：`LegacyValue`、`EcsValue`、`Reason`、`Disposition`。
6. 只有 source-backed intentional projection 才能进入 `AcceptedDifference`；其余都是阻断项。

**验收：** differential 退出码为 0，或每个剩余差异有审批的 `AcceptedDifference`，且
`worldgen-deletion-gate.json` 明确记录原因和证据。

### Task 12：全量门禁、依赖扫描和旧入口删除评审

**目的：** 在任何物理删除前证明旧入口已经不再承担 ServerRelevant 责任。

**Files:**

- Modify: `docs/worldgen/worldgen-deletion-gate.json`
- Modify: `docs/migrations/version4-physical-deletion-ledger.csv`
- Modify: `docs/server-completion/completion-manifest.json`
- Create: `Build/diagnostics/worldgen-complete/task-12-deletion-gate/<timestamp>/`

**Steps:**

1. 重新生成 WorldGen method/field inventory，确认没有无理由 `Unmapped`。
2. 扫描 Simulation 对 `Main`、`Tile`、`Liquid.QuickWater`、`NetMessage` 等 forbidden
   dependency；输出文件和行号。
3. 运行 WorldGeneration、WorldMechanics、Liquid、Wiring、Dungeon、WorldObjects、WorldRules
   相关验证器。
4. 运行完整 solution Release build，使用 `-p:UseSharedCompilation=false`。
5. 运行 WLD import parity，确认旧世界导入仍为 0 mismatch；同时运行 generated-world
   differential，不能用 import parity 代替 generation parity。
6. 检查 `ServerRelevant deferred`、`Unknown`、`canRemoveLegacyWorldGen` 和所有证据路径。
7. 由评审者单独批准是否移除旧编译入口；本 Task 本身不执行删除。

**验收：** 只有以下条件全部满足，才允许另开一个删除提交：

- `canRemoveLegacyWorldGen=true`。
- `ServerRelevant deferred=0`，`Unknown=0`。
- 全量 differential 通过或所有差异都有正式 disposition。
- forbidden-dependency scan 为 0。
- 相关验证器和 serial Release build 退出码为 0。
- 删除后重新生成 solution/project evaluation，确认没有旧源码被隐式 glob 包含。

## 5. 验证矩阵

每个阶段都必须保留以下证据类别；不要只保留终端输出：

| 类别 | 证据 | 目的 |
| --- | --- | --- |
| Source | source hash、方法/字段 anchor、inventory JSON | 证明迁移对象没有漂移 |
| Contract | request、cursor、command schema、system order | 证明边界稳定 |
| Unit | query、policy、validator focused verifier | 证明局部公式和拒绝分支 |
| Replay | fingerprint、section versions、cursor restart | 证明确定性和恢复 |
| Differential | 全量 Tile/Wall/Liquid/extended state 比较 | 证明行为接近旧基线 |
| Boundary | forbidden dependency、project references、deletion ledger | 证明没有旧运行时泄漏 |
| Build | serial restore/build/test exit code、warnings/errors | 证明当前树可构建 |

推荐的串行验证命令：

```powershell
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false

dotnet run --project Test\Terraria.Dome.WorldGeneration.Verification\Terraria.Dome.WorldGeneration.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.DungeonBounds.Verification\Terraria.Dome.DungeonBounds.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.DungeonRoom.Verification\Terraria.Dome.DungeonRoom.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.Liquid.Verification\Terraria.Dome.Liquid.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.Liquid.Loopback.Verification\Terraria.Dome.Liquid.Loopback.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.WorldMechanics.Verification\Terraria.Dome.WorldMechanics.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.WorldRules.Verification\Terraria.Dome.WorldRules.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build
```

如果某个验证器不存在、项目路径发生变化或命令参数不再支持，先更新计划和证据索引，不要
静默跳过。所有输出写入 `Build/bin/`、`Build/obj/` 或 `Build/diagnostics/`，不要写入 `src/`
或项目文件旁边。

## 6. 进度状态和回滚规则

### 6.1 状态枚举

每个 method、field、stage 和责任族使用以下状态之一：

- `Unmapped`：没有当前 owner 或没有可靠映射。
- `Mapped`：已有目标位置，但没有完整行为证据。
- `Partial`：有明确 bounded behavior 和 verifier，但完整责任未覆盖。
- `Complete`：满足本计划的四列证据和差分门禁。
- `ReplacedWithEvidence`：明确由其他 Simulation/Server owner 取代，并有 source-backed
  replacement artifact。
- `ExcludedWithEvidence`：ClientOnly 或明确不在服务器分母内，并记录证据和理由。

不允许用 `Complete` 表示“代码已编译”，也不允许用 `Excluded` 隐藏 ServerRelevant 缺口。

### 6.2 回滚边界

- 只回滚当前 Task 的生产文件、测试文件和本 Task 新增的 evidence index。
- 不删除历史 `Build/diagnostics`，除非它是本 Task 明确创建且确认可重建的临时文件。
- 不恢复或覆盖其他并行方向的修改。
- 如果差分恶化，先恢复到上一个通过 baseline 的 commit，再保留失败 artifact 和原因。
- 禁止 `git reset --hard`、宽范围清理或物理删除旧基线。

## 7. 最终交付物

WorldGen 完整迁移结束时，必须有以下可审计文件：

- `docs/worldgen/worldgen-source-inventory.json`
- `docs/worldgen/worldgen-method-map.md`
- `docs/worldgen/worldgen-difference-classification.csv`
- `docs/worldgen/legacy-worldgen-oracle.json`
- `docs/worldgen/legacy-worldgen-differential.json`
- `docs/worldgen/worldgen-parity-report.md`
- `docs/worldgen/worldgen-deletion-gate.json`
- `docs/migrations/version4-physical-deletion-ledger.csv`
- `docs/server-completion/completion-manifest.json`
- 每个 Task 的 `Build/diagnostics/worldgen-complete/<task>/<timestamp>/`

最终报告必须同时给出：

1. 方法/字段状态统计。
2. 每个阶段的 fingerprint 和 cursor restart 结果。
3. 全量 differential 的 mismatch 总数和字段分布。
4. 所有 accepted/deferred/excluded 差异及理由。
5. forbidden dependency、构建、验证器和删除门禁退出码。
6. 删除旧入口前后的 project evaluation 和完整回归结果。

本计划完成前，项目状态保持 `IN_PROGRESS` 或 `PARTIAL`；不得因为 bounded verifier、WLD
import parity 或单次 Release build 通过而改成 `Complete`。
