# WorldGenerationAndEcology Component Design

## 1. 设计元数据

```yaml
documentType: component-design
designScope: component-only
designStatus: decision-required
evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
subsystemId: WorldGenerationAndEcology
taskNumber: 18
sourceReport: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-world-generation-and-ecology-public-decomposition.md
sourcePrompt: D:\TRbackup\NLTX\docs\第一轮审查\设计\2026-09-06-version4-component-only-design-session-prompt.md
generatedAt: 2026-09-06
```

本设计以本会话唯一允许使用的研究报告为入口，并以 Version4 直接源码证据和当前 NLTX 类型为交叉依据。`designStatus: decision-required` 表示 Component 边界已经形成候选基线，但若干跨子系统 owner、实体范围和 ready barrier 组成仍不能从现有证据中裁决。

本文件的 `status` 取值含义如下：

- `existing`：当前 NLTX 已有相同或足以直接映射的状态类型；本文件只记录其 Component 归属，不声明已经完成 ECS 迁移。
- `partial`：当前 NLTX 有相关状态或值对象，但覆盖面、生命周期闭合或 authority 仍不完整。
- `proposed`：当前 NLTX 未发现可以直接作为该 Component 的完整现有类型；这是设计提案，不是代码变更。

## 2. 设计范围与排除范围

### 2.1 设计范围

本文件只定义 WorldGenerationAndEcology 子系统的 Component-only 形状：

- Component 的责任边界、字段、默认值和字段不变量；
- Component 的状态分类、生命周期和 Entity/World 范围；
- Component 之间的组合关系、ID 和关系字段；
- Version4 成员或状态簇到 Component 的候选归属；
- 当前 NLTX 类型覆盖、证据缺口和仍需裁决的 owner。

这里的 Component 是承载事实、身份、配置、进度、快照或可验证状态的最小状态单元。字段若暂时只是派生值、镜像或缓存，会在字段表中明确标注，不把它伪装成 authority。

### 2.2 排除范围

下列内容不在本文件范围内：

- System、Query、Command、Event、Adapter、Projection 或其他运行时结构；
- 生成 Pass 的执行算法、调度顺序、线程模型、批次协议和 I/O 实现；
- WorldGrid、Tile、TileEntity、Liquid solver 的实现或写入 API；
- 测试计划、迁移计划、文件移动、项目文件、源码实现或构建验证；
- Hardmode 转换的流程设计；它只在边界说明中保留为外部 progression owner；
- 把 Version4 的空壳入口、类型名或控制器存在性解释为行为等价实现。

## 3. 组件设计依据

### 3.1 直接研究报告

唯一研究报告是：

`D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-world-generation-and-ecology-public-decomposition.md`

报告元数据已确认为 `subsystemId: WorldGenerationAndEcology`、`taskNumber: 18`、`nltxStatus: partial`、`verificationStatus: not-run`。本文件不扫描或拼接 `docs\research` 中的其他报告。

### 3.2 Version4 证据簇

| 证据簇 | 直接位置 | 对 Component 设计的限制 |
| --- | --- | --- |
| 生成生命周期与 Pass 注册 | `D:\TRbackup\Version4\Terraria\WorldGen.cs:10108-10187`、`:10553` 起、`:9175-9176` | `GenerateWorld`、`Reset`、Pass 注册和清理存在，但核心 `AddGenerationPass` 重载为空，不能把注册表直接当成已验证执行行为。 |
| 生成编排 | `D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs:24-343` | 有 Controller/生成器编排形状，但 `RunPass` 位于 `:341-343` 且为空；因此 Plan、Pass cursor 和完成状态必须保持证据分级。 |
| 地形与世界描述 | `WorldGen.cs:10148-10187` 以及 `WorldGen.cs` 中生成后的地形事实 | World identity、surface/rock/spawn/Dungeon 等 canonical 字段与生成期地形事实不是同一责任，不能合并为巨型描述 Component。 |
| 住房扫描与分配 | `WorldGen.cs:5310-5773`、`Terraria.IO\WorldFile.cs:1938-1953` | 房间边界、评分、找房和加载后重新检查是不同状态簇；扫描临时状态不能直接等同 Town/NPC 分配结果。 |
| 生态与 Biome | `WorldGen.cs:9987-10017`、`WorldGen.cs:21654-21671` | `BiomeTileCheck` 是基于 Tile/Wall 的派生查询；感染计数、Biome 标签和更新调度不能无证据地共享同一个 authority。 |
| 液体 | `Liquid.cs:20-49`、`:1015-1094`、`:1190-1426`、`WorldFile.cs:658-787` | `numLiquid`、work set、settle 和加载后的标志清理属于液体运行期/交接事实；Component 只保留生成完成所需的 handoff 摘要。 |
| 持久化与完成通知 | `Terraria.IO\WorldFile.cs:1263-1467`、`WorldGen.cs:21704-21711` | WorldFile 元数据、生成完成通知和 ready barrier 不是同一个字段；持久化已提交必须成为 Ready 的独立 facet。 |
| Hardmode 边界 | `WorldGen.cs:26084-26127` | Hardmode 包含后台转换、`WorldFile.IOLock` 和主线程 follow-up，不纳入普通生成 Component 的生命周期。 |
| Tile 写入 | `WorldGen.cs:47823`、`:47843`、`:50970`、`:52561` | `Convert`、`PlaceTile`、`KillTile` 存在直接 Tile/Wall 写入和副作用；当前证据不足以声称 `WorldStorage.TileMapStore` 是全局唯一 authority。 |

### 3.3 当前 NLTX 证据

已确认的当前状态包括：

- `src\WorldSession\WorldGeneration\` 下已有 `WorldPreparationState`、`WorldGenerationLifecycleState`、`WorldDescriptorState`、`WorldRulesState`、`WorldEcologyScheduleState`、`TownHousingRegistry` 等类型；
- `dome\src\Terraria.Dome.Simulation\WorldGeneration\` 下已有 `WorldGenerationRequest`、`WorldGenerationStateComponent`、`WorldGenerationPassState`、`WorldGenerationRuntimeState`、`WorldGenerationRandomSnapshot`、`WorldGenerationCheckpoint`、`WorldGenerationTerrainState`、`WorldGenerationHousingState`、`WorldGenerationProgressionState`、`WorldGenerationServerState`、`LiquidPropagationSession`、`WorldGenerationTrace` 等类型；
- 当前 NLTX 有受限的 `WorldGenerationPipeline`、确定性随机状态、Tile/Liquid 局部提交和有限验证，但尚未证明实现了 Version4 的全量生成行为等价；
- `WorldGenerationServerState` 是混合状态，包含多个责任面，不能整体复制为一个巨型 Component；
- `WorldGenerationLifecycleState.IsReady` 与 server host 的 `IsReady` 都不能直接证明包含 Pass、Liquid、Housing、Persistence 的完整 ready barrier。

## 4. Version4 成员到 Component 归属表

下表是责任簇到 Component 的候选映射。它不是源代码成员移动清单；同一成员若同时产生配置、事实和进度，会拆分到不同 Component，派生值只保留其 canonical owner 或明确标注为镜像。

| Version4 成员/状态簇 | 候选 Component | 归属理由 | 证据状态 |
| --- | --- | --- | --- |
| `WorldGen.GenerateWorld` 的生成开始、清理、异常和 finally 生命周期 | `WorldGenerationLifecycleComponent`、`ReadyTransitionComponent` | 生命周期与最终可发布条件分别负责；不能由一个 `IsReady` 布尔值代替。 | `partial` |
| `WorldGen.Reset` 中的 `Manifest`、special seed、随机和结构初始状态 | `WorldDescriptorComponent`、`WorldGenerationRulesComponent`、`WorldGenerationRandomStateComponent` | 身份、规则和随机演进是不同稳定性与恢复边界。 | `partial` |
| `AddPasses` 注册的 Pass 描述和权重 | `WorldGenerationPlanComponent` | 计划是不可变输入/版本化集合，不等于当前 Pass cursor。 | `partial`；注册入口部分为空 |
| `WorldGenerator` controller、active pass 和 cursor 语义 | `WorldGenerationPassStateComponent` | 只保留当前阶段、游标、checkpoint/revision 和失败/暂停事实，不复制 controller。 | `partial`；`RunPass` 为空 |
| seed 相关随机值、生成期随机演进 | `WorldGenerationRulesComponent`、`WorldGenerationRandomStateComponent` | seed identity 属于规则/描述；可恢复的随机 cursor 属于生成状态。 | `partial` |
| `SurfaceLayer`、`RockLayer`、spawn、Dungeon、世界尺寸/边界 | `WorldDescriptorComponent` | 这些是世界 identity/geometry 的 canonical surface。 | `partial` |
| underworld、ocean、beach、Jungle/Snow/Desert 等生成期地形事实 | `WorldTerrainStateComponent` | 这些是依据生成结果计算出的 terrain facts，不重复拥有 descriptor identity。 | `partial` |
| `BiomeTileCheck`、感染/善恶计数、Biome 派生标签 | `BiomeEcologyStateComponent` | Tile/Wall 查询结果只能作为 revisioned facts、缓存或派生结果；不得直接冒充全局 authority。 | `partial` |
| 过地表/地下抽样、感染扩散开关、更新 rate/budget | `EcologyScheduleComponent` | 调度配置/预算与 Biome 事实、住房扫描游标职责不同。 | `partial` |
| `RoomNeeds`、`QuickFindHome`、房间边界和评分临时值 | `HousingScanStateComponent` | 扫描游标与候选房间是短生命周期状态，不是稳定 Town assignment。 | `partial` |
| Town NPC 房间结果、homeless 状态、加载后住房重新检查结果 | `TownHousingAssignmentComponent` | 分配结果需要稳定居民关系和 revision；当前整数 NPC key 是否足够尚未裁决。 | `partial` |
| `Liquid.numLiquid`、work set、`LiquidCheck` 和 settle 的生成期完成摘要 | `WorldLiquidHandoffComponent` | 只记录 ready 所需的交接/稳定性事实，不把 solver 内部集合或全局 liquid 数复制进 ECS Component。 | `partial` |
| `WorldGen.Finish`、WorldFile 提交、生成通知和可发布门槛 | `ReadyTransitionComponent` | ready 必须显式表达 Pass、Liquid、Housing、Persistence 等 facet，不能映射为 server host ready。 | `partial` |
| `StartHardmode`、后台转换、`WorldFile.IOLock` 和主线程 follow-up | 不单独归入本组 Component | 这是 WorldProgressionAndTransition 外部边界；普通生成 Component 只须避免与其共享 ownership。 | `partial` |

## 5. Component 定义

### 5.1 WorldDescriptorComponent

`status: existing`。当前主要映射为 `src\WorldSession\WorldGeneration\WorldDescriptorState.cs`；目标命名是责任归属，不表示需要立即新增同名代码文件。

#### 职责

持有世界身份、稳定几何尺寸和生成前后都可引用的 canonical 坐标。它不持有生成规则、当前 Pass、Biome 计数、住房扫描临时值或 ready 结果。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `WorldId` | `int` | `unset` | `existing`；`crossSubsystemOwner: integration-review` |
| `UniqueId` | `Guid` | `unset` | `existing`；`crossSubsystemOwner: integration-review` |
| `Name` | `string` | 空值或未加载 | `existing` |
| `Seed` | `int` | `unset` | `existing` |
| `SeedText` | `string` | 空值或未加载 | `existing` |
| `GeneratorVersion` | `ulong` | `unset` | `existing`；持久化 owner 未决 |
| `SizeX`、`SizeY` | `int` | `unset` | `existing` |
| `Bounds` | `WorldBounds` | 未设置 | `existing` |
| `SurfaceLayer`、`RockLayer` | `double` | `unset` | `existing` |
| `SpawnTileX`、`SpawnTileY` | `int` | `unset` | `existing`；`crossSubsystemOwner: integration-review` |
| `DungeonTileX`、`DungeonTileY` | `int` | `unset` | `existing` |
| `SectionCountX`、`SectionCountY` | `int` | 由尺寸和分段规则派生 | `existing`；不可独立写入 |
| `HasSurface` | `bool` | 由字段派生 | `existing`；不可独立写入 |

#### 字段不变量

- `SizeX`、`SizeY` 必须为正，`Bounds` 必须与尺寸一致；在 descriptor 尚未加载时允许整体为 `unset`，不能以零值伪装为合法小世界。
- `SpawnTile`、`DungeonTile` 必须落在 `Bounds` 内，或显式标记为未解析；坐标单位必须统一为 tile 坐标。
- `SurfaceLayer` 不得高于 `RockLayer`；无 surface 的世界必须通过 `IsNoSurface` 或显式 unset 语义表达，不能同时保留矛盾的正常 surface。
- `SectionCountX/Y` 和 `HasSurface` 是派生字段，不得形成第二个可写 authority。
- `WorldId` 与 `UniqueId` 的生命周期和持久化语义尚未由当前证据裁决，关系字段保留 `crossSubsystemOwner: integration-review`。

#### 生命周期

在世界身份解析/创建时初始化；在生成开始前冻结 identity 和 geometry；生成 Pass 可读取；WorldFile 读取/写入时作为 descriptor surface；若加载失败，保留失败前的 identity 事实但不得宣称 ready。

#### Entity/World 范围

每个 World Entity 一个。不得复制到每个 Tile、Room 或 NPC Entity；住房和生态只通过世界关系或明确居民关系引用它。

#### ID 与关系字段

`WorldId` 是兼容性/运行期整数候选，`UniqueId` 是稳定世界身份候选。`GenerationRevision` 不属于本 Component；若其他 Component 需要关联生成代，使用其自己的 revision/关系字段并由 `BD-COMP-06` 裁决共享类型。

#### 当前 NLTX 映射

`WorldDescriptorState` 已覆盖世界尺寸、边界、surface/rock 和坐标类事实的主要部分，因此标为 `existing`。当前证据不足以确认它是否已经拥有 Version4 WorldFile 中全部 `WorldId`、`UniqueId`、`GeneratorVersion` 和 Manifest 语义，故这些字段不作完整等价声明。

#### 证据

Version4 `WorldGen.Reset`：`D:\TRbackup\Version4\Terraria\WorldGen.cs:10148-10187`；WorldFile header、WorldId、UniqueId、Seed、GeneratorVersion、Hardmode、地形/生态状态和 Manifest：`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1263-1467`；当前映射：`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldDescriptorState.cs`。

### 5.2 WorldGenerationRulesComponent

`status: existing`。当前主要映射为 `src\WorldSession\WorldGeneration\WorldRulesState.cs` 和相关规则值类型。

#### 职责

持有决定一次世界生成输入与变体的规则事实，包括难度、seed variant、secret seed/world variant 和液体/感染相关开关。它不持有生成进度、Tile 结果、Hardmode 转换后的进度或 Ore tier progression。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `Difficulty` | `int` | `unset` | `existing` |
| `SeedVariant` | `string` | 空值 | `existing` |
| `WorldEvil` | `WorldEvilType` | `unresolved` | `partial`；与生态 owner 冲突 |
| `IsRemixWorld` | `bool` | `false` | `existing` |
| `IsEverythingWorld` | `bool` | `false` | `existing` |
| `IsNoTrapsWorld` | `bool` | `false` | `existing` |
| `IsDrunkWorld` | `bool` | `false` | `existing` |
| `IsGoodWorld` | `bool` | `false` | `existing` |
| `IsDontStarveWorld` | `bool` | `false` | `existing` |
| `IsNotTheBeesWorld` | `bool` | `false` | `existing` |
| `IsSkyblockWorld` | `bool` | `false` | `existing` |
| `IsNoSurfaceWorld` | `bool` | `false` | `existing` |
| `IsSurfaceDesertWorld` | `bool` | `false` | `partial` |
| `IsIceBiomeWorld` | `bool` | `false` | `partial` |
| `IsTenthAnniversaryWorld` | `bool` | `false` | `partial` |
| `NoInfection` | `bool` | `false` | `existing` |
| `ExtraLiquid` | `bool` | `false` | `partial` |
| `WorldIsFrozen` | `bool` | `false` | `partial` |
| `StartInHardmode` | `bool` | `false` | `partial`；progression boundary review |

#### 字段不变量

- 规则字段必须在生成计划开始前完成解析并在本次生成中保持稳定；未解析规则不得以默认 `false` 隐藏。
- secret seed/world variant flags 可以同时存在，互相排斥关系必须由明确的规则语义裁决，而不能由 Component 文件或顺序隐式决定。
- `WorldEvil` 的 canonical owner 在 Rules 与 Biome/Ecology 之间未决；两个 Component 可以暂存只读镜像，但只允许一个最终 authority。
- `StartInHardmode` 只描述创建输入；不等同于运行期 Hardmode 状态，不得在本 Component 中加入 `HardMode` 或 `OreTiers` 的运行期迁移状态。

#### 生命周期

由世界创建/加载输入解析；生成计划建立前冻结；所有生成期 Component 只读引用；持久化时作为规则元数据的一部分保存或投影；规则不因普通生态更新而改变。

#### Entity/World 范围

每个 World Entity 一个。不得附着到 Pass、Tile、Liquid work item 或居民 Entity。

#### ID 与关系字段

可通过 `WorldId`/`UniqueId` 关联 `WorldDescriptorComponent`，但不重复存储 descriptor identity。`Seed` 的 canonical owner 候选为 descriptor；`SeedVariant` 和 flags 属于 rules；共享 ID/版本类型由 `BD-COMP-06` 裁决。

#### 当前 NLTX 映射

`WorldRulesState`、`WorldSecretSeedFlags`、`WorldGameMode` 和 `WorldEvilType` 提供了主要规则覆盖，故标为 `existing`；Version4 的所有变体 flag、`ExtraLiquid`、冻结状态和创建时 Hardmode 语义尚未逐项闭合，逐字段使用 `existing`/`partial`。

#### 证据

Version4 `WorldGen.GenerateWorld` 与 `Reset`：`D:\TRbackup\Version4\Terraria\WorldGen.cs:10108-10187`；WorldFile 规则元数据：`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1263-1467`；当前映射：`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldRulesState.cs`、`WorldSecretSeedFlags.cs`、`WorldGameMode.cs`、`WorldEvilType.cs`。

### 5.3 WorldGenerationLifecycleComponent

`status: existing`。当前主要映射为 `src\WorldSession\WorldGeneration\WorldGenerationLifecycleState.cs`、`WorldPreparationState.cs` 和 `WorldGenerationFailure.cs`。

#### 职责

表达 World 的准备、加载、生成、失败和退出生成态；它是过程生命周期事实，不是最终发布 barrier。`ReadyTransitionComponent` 单独承载是否满足所有发布 facet。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `Phase` | `WorldPreparationState` | `NotStarted` 候选；需保留现有枚举语义 | `existing` |
| `Failure` | `WorldGenerationFailure` | `None` 候选 | `existing` |
| `GenerationRevision` | `ulong` | `0` | `partial` |
| `IsLoadingOrGenerating` | `bool` | 由 `Phase` 派生 | `existing`；不可独立写入 |
| `IsReady` | `bool` | 由生命周期与 Ready facets 派生 | `partial`；非独立 authority |
| `CanUpdateSimulation` | `bool` | 由 lifecycle/ready policy 派生 | `proposed` |
| `HasFailed` | `bool` | 由 `Failure` 派生 | `existing`；不可独立写入 |

#### 字段不变量

- `Phase` 必须与 `Failure` 的终态语义一致；失败时不能同时成为可更新、可发布状态。
- `GenerationRevision` 在一次世界生成代内稳定递增或保持，不能用 Pass cursor 代替。
- `IsReady` 不能仅由 `Phase` 推导为 true；它必须依赖 `ReadyTransitionComponent` 的完整 facet。
- 派生布尔值不得被外部 host 或单个 Pass 直接写入。

#### 生命周期

世界创建/加载时进入未开始或准备态；生成期间按状态推进；任何不可恢复失败进入失败态；只有 ready transition 通过后才可得出可更新/可发布结果；世界销毁或重新生成时清除当前代的过程状态。

#### Entity/World 范围

每个 World Entity 一个。server host 的 lifecycle 可以是外部观察者，但不是该 Component 的第二个 authority。

#### ID 与关系字段

`GenerationRevision` 关联同一 World Entity 上的 Plan、Pass、Random、Liquid 和 Ready 状态。`WorldId` 不应重复存储；revision 类型的共享 owner 标记为 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`WorldGenerationLifecycleState` 已提供 lifecycle 和 `IsReady` 相关表面，因此标为 `existing`。但完整 ready barrier 未闭合，`IsReady` 在目标设计中降级为派生/兼容表面，当前覆盖整体按 `partial` 处理。

#### 证据

Version4 `GenerateWorld` 的 flag、初始化、Pass 注册、finally 清理：`D:\TRbackup\Version4\Terraria\WorldGen.cs:10108-10147`；Version4 `Finish`：`WorldGen.cs:21704-21711`；当前映射：`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldGenerationLifecycleState.cs`、`WorldPreparationState.cs`、`WorldGenerationFailure.cs`。

### 5.4 WorldGenerationPlanComponent

`status: proposed`。当前存在 Pass/阶段相关类型，但没有证据证明已经形成不可变、版本化、可恢复的完整生成计划 Component。

#### 职责

持有一次生成代所使用的 Pass 描述集合、权重汇总和禁用项。它描述“要执行什么”，不描述“现在执行到哪里”，也不包含 Pass 的命令批次或 WorldGrid。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `GenerationId` | `long` | `0` 表示未建立 | `proposed`；`crossSubsystemOwner: integration-review` |
| `PlanVersion` | `int` | `0`/`unresolved` | `proposed`；版本 owner 未决 |
| `PassDescriptors` | `IReadOnlyList<GenerationPassDescriptor>` | 空集合 | `proposed` |
| `TotalWeight` | `double` | `0` | `proposed` |
| `DisabledPassIds` | `IReadOnlyList<string>` | 空集合 | `proposed` |

#### 字段不变量

- `GenerationId` 必须与 Lifecycle 的 `GenerationRevision` 具有明确一一关系，不能复用随机状态 cursor。
- `PassDescriptors` 的每个 ID 必须唯一；权重必须是有限非负值，`TotalWeight` 必须等于描述集合的可用权重和。
- 计划一旦进入 active generation，不得被当前 Pass 的进度写入；若计划变更，应产生新的 generation/plan revision。
- `GenerationPassDescriptor` 只作为字段值形状，不在本文件定义其执行行为；其 owner 和版本字段需进一步裁决。

#### 生命周期

由规则、生成器版本和世界尺寸等输入计算/加载；在第一 Pass 开始前冻结；整个 generation 代中只读；生成结束或失败后作为审计/恢复快照保留到其 owner 规定的边界。

#### Entity/World 范围

每个 World Entity 一个，每个 generation 代一个有效值。不得为每个 Pass 建立重复计划 Component。

#### ID 与关系字段

`GenerationId` 关联 Lifecycle、Pass、Random 和 Ready；`PassDescriptors` 中的 `PassId` 关联 `WorldGenerationPassStateComponent.ActivePassId`。这些字符串 ID、plan version 和 generation revision 的共享类型标记 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`WorldGenerationPipeline`、Pass 定义和 `WorldGenerationStage` 提供局部编排事实，但现有证据不足以证明完整 Pass 计划、版本和禁用集合已被集中持有，因此整体为 `proposed`，映射覆盖为 `partial`。

#### 证据

Version4 Pass 注册入口：`D:\TRbackup\Version4\Terraria\WorldGen.cs:10553` 起；`AddGenerationPass` 空重载：`WorldGen.cs:9175-9176`；Version4 `WorldGenerator`：`D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs:24-343`；当前候选映射：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationPipeline.cs`。

### 5.5 WorldGenerationPassStateComponent

`status: existing`。当前主要映射为 `dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationPassState.cs`。

#### 职责

持有当前 generation 的阶段、active Pass、可恢复游标、读快照 revision、checkpoint revision 和暂停/终止请求事实。它不保存计划全集、命令批次、Tile 网格或执行器对象。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `Stage` | `WorldGenerationStage` | `NotStarted` 候选 | `existing` |
| `ActivePassId` | `string?` | `null` | `existing` |
| `PassVersion` | `int` | `0`/`unresolved` | `partial` |
| `Cursor` | `long` | `0` | `existing` |
| `ReadSnapshotRevision` | `ulong?` | `null` | `partial` |
| `CheckpointRevision` | `ulong?` | `null` | `partial` |
| `FailureReason` | `string?` | `null` | `existing` |
| `PauseRequested` | `bool` | `false` | `partial` |
| `AbortRequested` | `bool` | `false` | `partial` |

#### 字段不变量

- `ActivePassId` 非空时必须能在 Plan 中解析到一个 Pass；无 active Pass 时 cursor 必须表达 stage 边界，而非遗留旧值。
- `Cursor` 只表示可恢复的逻辑位置，不能暗示某批 Tile 已提交或某个副作用已完成；其提交语义必须由外部证据定义。
- `CheckpointRevision` 和 `ReadSnapshotRevision` 必须引用同一 generation 代，不能跨代恢复。
- `PauseRequested`/`AbortRequested` 是状态事实或待处理意图的候选字段，不能据此推导 ready。

#### 生命周期

计划激活时清空 active cursor；进入每个 Pass 时设置 Pass ID、版本和 cursor；处理 checkpoint/失败/暂停时更新相应字段；Pass 结束后清理 active Pass 或转到下一个阶段；generation 结束时保留最终阶段并由 Ready 单独确认发布条件。

#### Entity/World 范围

每个 World Entity 一个；它描述该 World 当前 generation 的单一 active Pass，不应拆为每个 Tile 或每个 Pass Entity。

#### ID 与关系字段

通过 `GenerationId` 关联 Plan 和 Lifecycle，通过 `ActivePassId` 关联 `PassDescriptors`。`CheckpointRevision` 与 WorldGenerationCheckpoint 的关系字段 owner 未决，标记 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

存在同名 `WorldGenerationPassState` 和相关 stage snapshot，因此标为 `existing`。但 Version4 的实际 `RunPass` 为空，当前类型不能被描述为已经实现参考执行语义；`PassVersion`、snapshot 和 checkpoint 的全量契约仍为 `partial`。

#### 证据

Version4 `WorldGenerator.RunPass`：`D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs:341-343`；当前映射：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationPassState.cs`、`WorldGenerationStageSnapshot.cs`、`WorldGenerationCheckpoint.cs`。

### 5.6 WorldGenerationRandomStateComponent

`status: partial`。当前主要映射为 `dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationRandomSnapshot.cs` 和相关确定性随机状态；它不等于规则中的 seed identity。

#### 职责

持有 generation 中可恢复的随机流状态和其位置，用于使同一 generation 代的随机演进具有明确状态边界。它不持有世界 seed 的身份元数据，也不持有全局随机库对象。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `State` | `uint` | `0`/由 seed 初始化 | `existing` |
| `StreamVersion` | `int` | `0`/`unresolved` | `proposed` |
| `PassId` | `string?` | `null` | `partial` |
| `Cursor` | `long` | `0` | `partial` |

#### 字段不变量

- `State`、`PassId`、`Cursor` 必须属于同一个 `GenerationId`；generation 变化时不得隐式复用旧随机状态。
- seed、seed text 和 secret seed flags 不得在此重复成为 identity authority；此处只存可演进的 stream state。
- `StreamVersion` 未定义前不能宣称跨版本 replay 或恢复兼容；未知版本必须显式为 unresolved，而不是假设为零版本。
- `Cursor` 递增语义和 Pass 边界必须可观察，但不把随机调用次数外推为 Pass 完成。

#### 生命周期

生成初始化时由 descriptor/rules 建立；每个随机流使用边界更新；checkpoint 时作为同代快照保存；Pass 或 generation 结束时冻结/清除；普通生态更新不应悄然改变生成随机状态。

#### Entity/World 范围

每个 World Entity 当前 generation 一个。若未来证明一个 Pass 需要独立流，关系应通过显式 stream ID 表达，而不是复制整个 World Component；该选择目前未决。

#### ID 与关系字段

`PassId` 关联 Pass state；`Cursor` 关联 checkpoint；seed identity 关联 Descriptor/Rules。stream version 和 checkpoint revision 的共享 owner 标记 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

当前已有 `WorldGenerationRandomSnapshot` 和确定性随机相关状态，能够覆盖随机快照的一部分，故标为 `partial`。尚未确认流版本、cursor 与 Pass/generation revision 的完整闭合关系。

#### 证据

Version4 `WorldGen.Reset` 的随机初始化：`D:\TRbackup\Version4\Terraria\WorldGen.cs:10148-10187`；当前映射：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationRandomSnapshot.cs`。

### 5.7 WorldTerrainStateComponent

`status: existing`。当前主要映射为 `dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationTerrainState.cs`；它只拥有生成期 terrain facts，不重复拥有 descriptor identity。

#### 职责

持有生成过程计算出的地形层、海滩/海洋、地表和主要地貌定位事实。它不保存整个 WorldGrid，也不把 Tile 查询结果全部物化为 Component 字段。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `UnderworldLayerY` | `int` | `unset` | `existing` |
| `OceanLevelY` | `int` | `unset` | `existing` |
| `BeachDistance` | `int` | `unset` | `existing` |
| `BeachSandDepth` | `int` | `unset` | `existing` |
| `SurfaceOffset` | `double` | `unset` | `partial` |
| `JungleX`、`JungleY` | `int` | `unset` | `existing` |
| `SnowX`、`SnowY` | `int` | `unset` | `existing` |
| `DesertX`、`DesertY` | `int` | `unset` | `existing` |
| `SurfaceIsDesert` | `bool` | `false`/未计算 | `existing` |
| `SurfaceIsMushrooms` | `bool` | `false`/未计算 | `partial` |
| `SurfaceIsInSpace` | `bool` | `false`/未计算 | `partial` |
| `IsOceanAtSpawn` | `bool` | `false`/未计算 | `partial` |
| `IsBeachAtSpawn` | `bool` | `false`/未计算 | `partial` |
| `IsNoSurface` | `bool` | `false` | `partial`；规则镜像只读 |
| `IsRemix` | `bool` | `false` | `partial`；规则镜像只读 |
| `IsErrorWorld` | `bool` | `false` | `partial` |

#### 字段不变量

- 坐标字段必须在 `WorldDescriptorComponent.Bounds` 内，或在计算失败/尚未计算时保持 unset；不能以 `(0,0)` 代表有效地貌。
- `SurfaceIs*`、`IsOceanAtSpawn` 和 `IsBeachAtSpawn` 只有在对应扫描/计算完成后才可被读取为事实，并应带有相应 generation/terrain revision。
- `IsNoSurface`、`IsRemix` 与 Rules 中同名语义不能形成双写 authority；本 Component 中若保留，仅作为 terrain-side read-only mirror。
- 与 descriptor 重复的 `SurfaceLayer`、`RockLayer`、Spawn 和 Dungeon 字段不加入本 Component；兼容镜像不得独立更新。

#### 生命周期

生成开始时为空/未计算；相关地形 Pass 完成后逐项填充；后续验证可更新 revisioned facts；世界保存/加载时作为 terrain metadata 的一部分恢复；重新生成时清空上一代事实。

#### Entity/World 范围

每个 World Entity 一个。复杂地貌区域不因此创建一个 Component per biome；区域集合若未来需要独立实体，必须另行建模，不在此文件隐含。

#### ID 与关系字段

使用 World Entity 关系到 Descriptor、Rules 和 generation revision；地貌坐标使用 `TilePosition` 或明确的整数 tile 坐标。坐标值类型和 canonical owner 标记 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

存在同名 `WorldGenerationTerrainState`，并有 WorldSession 的 descriptor/bounds 值类型，故标为 `existing`。Version4 的每个地形事实是否已经逐项覆盖、其计算完成标记是否可靠，当前仍是 `partial`。

#### 证据

Version4 WorldFile 地形/生态元数据：`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1263-1467`；Version4 生成结束地形相关检查：`D:\TRbackup\Version4\Terraria\WorldGen.cs:21654-21671`；当前映射：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationTerrainState.cs`。

### 5.8 BiomeEcologyStateComponent

`status: proposed`。当前 NLTX 存在感染对齐、Tile presence、Biome 查询等分散事实，但没有证据证明一个 Component 已成为完整 Biome/Ecology authority。

#### 职责

持有经过明确 revision 标记的世界生态事实：Biome 标签集合、感染/善恶计数和 conversion 事实。它不拥有生态更新频率、扫描预算或住房状态；Tile/Wall 查询只能成为其输入、快照或派生缓存。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `BiomeRevision` | `ulong` | `0` | `proposed` |
| `BiomeTags` | `IReadOnlySet<string>` | 空集合，未完成时带 revision 语义 | `proposed`；语义 owner 未决 |
| `WorldIsInfected` | `bool` | `false`/未评估 | `partial` |
| `WorldEvil` | `WorldEvilType` | `unresolved` | `partial`；与 Rules 冲突 |
| `ConversionRevision` | `ulong` | `0` | `proposed` |
| `TotalEvil` | `int` | `0`/未统计 | `partial` |
| `TotalBlood` | `int` | `0`/未统计 | `partial` |
| `TotalGood` | `int` | `0`/未统计 | `partial` |
| `TotalSolid` | `int` | `0`/未统计 | `partial` |
| `TilePresenceRevision` | `ulong?` | `null` | `proposed` |

#### 字段不变量

- 所有计数必须声明统计范围、统计 revision 和未完成语义；零值不能无条件表示“世界没有该生态”。
- `BiomeTags` 是语义标签集合而非任意字符串缓存；标签 vocabulary 和 owner 尚未裁决，未定义标签不得作为稳定协议事实。
- `BiomeTileCheck` 的单点/局部结果不能直接覆盖世界级 `BiomeTags` 或感染计数。
- `WorldEvil` 只能在 Rules 或本 Component 中选择一个 canonical owner，另一处必须是只读镜像；该冲突列为 `BD-COMP-02`/`BD-COMP-06` 相关决定。

#### 生命周期

生成期在相关 terrain/生态事实建立后形成初始 revision；加载后从持久化或重新扫描恢复；感染/转换改变时递增相应 revision；未完成扫描期间保留旧 revision 与 stale/unknown 语义，不能直接发布为当前完整世界事实。

#### Entity/World 范围

每个 World Entity 一个。区域级 Biome 结果若未来需要独立索引，不在此 Component 中隐式创建区域列表；本组件只保留世界级摘要和明确 revision。

#### ID 与关系字段

通过 World Entity 关联 Terrain、Rules 和 EcologySchedule；`BiomeRevision`、`ConversionRevision`、`TilePresenceRevision` 的跨模块共享语义标记 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

当前有 `WorldInfectionAlignment*`、`TilePresenceScan*`、`BiomeSurfaceSystem` 和 WorldFile 相关事实，但未证实这些已汇聚为完整世界级 Biome/Ecology authority，故目标 Component 为 `proposed`、覆盖为 `partial`。

#### 证据

Version4 `BiomeTileCheck`：`D:\TRbackup\Version4\Terraria\WorldGen.cs:9987-10017`；Version4 生成结束液体/生态相关检查：`WorldGen.cs:21654-21671`；Version4 WorldFile 生态元数据：`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1263-1467`；当前候选映射：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldInfectionAlignment*`、`TilePresenceScan*` 和 `BiomeSurfaceSystem`。

### 5.9 EcologyScheduleComponent

`status: partial`。当前主要映射为 `src\WorldSession\WorldGeneration\WorldEcologyScheduleState.cs`，但住房游标不应继续与生态 schedule 混合。

#### 职责

持有生态更新是否允许、抽样位置/距离、世界更新 rate 和生态 mutation budget 等调度输入/预算事实。它不执行调度，不持有 Biome 计数或住房扫描游标。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `IsInfectionSpreadAllowed` | `bool` | `false` 直到规则解析 | `existing` |
| `OvergroundSampleX`、`OvergroundSampleY` | `int` | `unset` | `partial` |
| `UndergroundSampleX`、`UndergroundSampleY` | `int` | `unset` | `partial` |
| `WorldUpdateRate` | `int` | `unresolved` | `partial` |
| `EcologyMutationBudget` | `int` | `0`/未配置 | `proposed` |
| `ScheduleRevision` | `ulong` | `0` | `proposed` |

#### 字段不变量

- spread 允许性必须同时受 `NoInfection` 等 rules 约束；单独为 true 不能覆盖禁止规则。
- 抽样坐标必须与 Descriptor bounds 和 tile 坐标约定一致；未采样时保持 unset。
- rate/budget 必须是有限的非负配置；`0` 的含义必须区分“禁用”“未配置”和“无预算”。
- `ScheduleRevision` 变化只表示调度输入变化，不代表 Biome 事实已更新。

#### 生命周期

规则解析后建立；生成完成前可处于未启用/未配置态；生态更新周期开始前冻结当前调度 revision；配置变更产生新 revision；世界重新生成时重置。

#### Entity/World 范围

每个 World Entity 一个。抽样点是世界级调度数据，不为每个样本创建 Component；住房扫描相关 cursor 不放入这里。

#### ID 与关系字段

通过 World Entity 关联 Rules 和 BiomeEcologyState；`ScheduleRevision` 与 world update policy 的关系 owner 标记 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`WorldEcologyScheduleState` 已提供 schedule 主要表面，故标为 `partial` 而不是 `proposed`。现有状态中的 `TownHousingScanCursor` 与 `PrioritizedTownNpcType` 应迁出候选边界，且 mutation budget/revision 尚未证实。

#### 证据

当前映射：`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldEcologyScheduleState.cs`；Version4 的抽样、感染和生成后检查证据：`D:\TRbackup\Version4\Terraria\WorldGen.cs:21654-21671`、`WorldGen.cs:9987-10017`。

### 5.10 HousingScanStateComponent

`status: proposed`。当前有 `WorldGenerationHousingState`、房屋规则和值类型，但完整扫描状态到生成生命周期的闭合接入尚未证实。

#### 职责

持有一次住房扫描期间的游标、候选房间边界、评分、房间组成事实和失败信息。它是可丢弃/可恢复的扫描状态，不是 Town/NPC 的稳定房间分配集合。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `ScanRevision` | `ulong` | `0` | `proposed` |
| `Cursor` | `long` | `0` | `proposed` |
| `PrioritizedTownNpcType` | `int?` | `null` | `partial` |
| `RoomTiles` | `int` | `0` | `proposed` |
| `MaxRoomTiles` | `int` | `0`/未配置 | `proposed` |
| `MaxRoomSize` | `int` | `0`/未配置 | `proposed` |
| `RoomX1`、`RoomX2`、`RoomY1`、`RoomY2` | `int` | `unset` | `partial` |
| `BestX`、`BestY` | `int` | `unset` | `proposed` |
| `HighScore` | `int` | `unscored` | `proposed` |
| `CanSpawn` | `bool` | `false`/未评估 | `partial` |
| `HouseTile`、`RoomTorch`、`RoomDoor`、`RoomChair`、`RoomTable` | `bool` | `false`/未扫描 | `partial` |
| `RoomHasStinkbug`、`RoomHasEchoStinkbug` | `bool` | `false`/未扫描 | `proposed` |
| `CurrentlyTryingAlternateSpot` | `bool` | `false` | `proposed` |
| `SharedRoomX` | `int?` | `null` | `proposed` |
| `LastFoundHouse` | `TilePosition?` | `null` | `partial` |
| `FailureReason` | `string?` | `null` | `proposed` |

#### 字段不变量

- 房间边界必须满足 `X1 <= X2`、`Y1 <= Y2`，且在 World bounds 内；未开始扫描时允许整体 unset。
- `RoomTiles`、`MaxRoomTiles`、`MaxRoomSize` 必须非负；`HighScore` 的未评分状态不能与合法零分混淆。
- 房间组成布尔值只表示本次扫描已检查的事实；false 在未扫描时不是“明确不存在”。
- `PrioritizedTownNpcType` 只表示扫描目标候选，不是稳定居民身份；稳定 assignment 归另一个 Component。
- `ScanRevision` 必须递增或显式替换，不能在扫描中途静默复用旧结果。

#### 生命周期

住房检查开始时初始化；扫描每个候选房间时更新 cursor/边界/评分；成功时生成可被 assignment 采用的候选结果；失败时记录原因；扫描结束后可清除临时字段，但 revision/最后结果的保留期限需由 owner 决定。

#### Entity/World 范围

候选为每个 World Entity 一个，表示当前世界的一次 active housing scan。若未来证实多个并行扫描，必须用明确 scan ID 拆分；本文件不预设并行运行时结构。

#### ID 与关系字段

`ScanRevision` 关联 `TownHousingAssignmentComponent.SourceScanRevision`；`LastFoundHouse` 使用现有 `TilePosition` 值对象候选。NPC type 与稳定居民 ID 的关系未决，标记 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`dome\...\WorldGenerationHousingState.cs`、`TileHousingRuleSnapshot`、`TileHousingRuleQuery` 和 WorldSession 的住房类型覆盖一部分规则/状态；尚未证明 Version4 的完整 `RoomNeeds`、`QuickFindHome`、扫描生命周期和加载后重检均已闭合，因此目标为 `proposed`、覆盖为 `partial`。

#### 证据

Version4 房间边界、找房、评分和房间检查：`D:\TRbackup\Version4\Terraria\WorldGen.cs:5310-5773`；加载后住房重新检查、失败时 KickOut/homeless：`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1938-1953`；当前映射：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationHousingState.cs`、相关 `TileHousingRule*` 类型。

### 5.11 TownHousingAssignmentComponent

`status: proposed`。当前有 `TownHousingRegistry` 和 `TownHousingResidentKey(int NpcType)`，但稳定居民关系、owner 和实体范围尚未裁决。

#### 职责

持有 Town/居民到房间位置的稳定分配结果、无家可归集合以及该结果的 revision。它不保存扫描过程中的房间评分、候选边界或所有房间检查细节。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `AssignedRooms` | `IReadOnlyDictionary<PersistentEntityId, TilePosition>` 候选 | 空集合 | `proposed`；`crossSubsystemOwner: integration-review` |
| `HomelessResidents` | `IReadOnlySet<PersistentEntityId>` 候选 | 空集合 | `proposed`；`crossSubsystemOwner: integration-review` |
| `Revision` | `ulong` | `0` | `proposed` |
| `SourceScanRevision` | `ulong` | `0`/`null` 直到采用扫描结果 | `proposed` |

#### 字段不变量

- 同一稳定居民只能有一个 active assigned room，除非关系模型明确允许共享房间；共享规则不能由 dictionary 覆盖顺序决定。
- `AssignedRooms` 和 `HomelessResidents` 不得同时把同一居民标记为已分配和无家可归。
- `SourceScanRevision` 必须引用实际采用的 HousingScan revision；扫描失败或过期时不能发布为新分配。
- `PersistentEntityId` 不是当前 `TownHousingResidentKey(int NpcType)` 的无条件替换；在 owner 决定前两者只能通过兼容关系映射。

#### 生命周期

初始加载/生成时为空或由持久化结果恢复；HousingScan 成功后以新 revision 更新；加载后重新住房检查可能替换或清除关系并产生 homeless 结果；世界卸载时停止作为当前 assignment authority。

#### Entity/World 范围

候选一：每个 World Entity 一个，以居民 ID 到房间位置的集合表达；候选二：每个 Town/居民集合 Entity 一个，再由 World 关系汇总。当前只能确认它至少属于 WorldGenerationAndEcology 边界，具体 Entity 粒度是 `BD-COMP-03`。

#### ID 与关系字段

`AssignedRooms` 的 key 需要 `PersistentEntityId` 候选，value 使用 `TilePosition`；`SourceScanRevision` 关联 HousingScan。当前 NLTX 的 `TownHousingResidentKey(int NpcType)`、Town/NPC identity 和 persistence identity 是否一一对应，标记 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`TownHousingRegistry` 提供现有 registry 表面，`TownHousingResidentKey` 提供整数居民 key，因此覆盖为 `partial`；没有证据证明已经持有稳定 Entity 关系、assignment revision、homeless 集合和加载后重检闭环，目标 Component 标为 `proposed`。

#### 证据

Version4 `RoomNeeds`、`QuickFindHome`、房间评分和 `CheckRoom`：`D:\TRbackup\Version4\Terraria\WorldGen.cs:5310-5773`；WorldFile 加载后住房重检：`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1938-1953`；当前映射：`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\TownHousingRegistry.cs`、`TownHousingResidentKey.cs`。

### 5.12 WorldLiquidHandoffComponent

`status: proposed`。当前有 `LiquidPropagationSession`、Liquid work item 和局部 propagation 状态，但没有证据证明已形成生成完成与运行期液体 solver 之间的稳定 handoff authority。

#### 职责

持有生成阶段交给世界可发布条件所需的液体传播摘要：本次交接 revision、待处理数量、访问完成数量、稳定性、快照 revision 和失败信息。它不复制运行期 Liquid solver 的全局计数、work set、panic 标志或 quick-settle 实现。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `PropagationRevision` | `ulong` | `0` | `proposed`；`crossSubsystemOwner: integration-review` |
| `PendingWorkItemCount` | `int` | `0` | `partial` |
| `CompletedVisitCount` | `long` | `0` | `partial` |
| `Stable` | `bool` | `false` | `proposed` |
| `SnapshotRevision` | `ulong?` | `null` | `proposed` |
| `FailureReason` | `string?` | `null` | `proposed` |

#### 字段不变量

- `PendingWorkItemCount`、`CompletedVisitCount` 必须非负；`Stable: true` 只能在待处理集合为空且所需 liquid checks/settle 已满足时成立，具体条件需 owner 裁决。
- `Stable` 不能由 server host ready 或单次 `LiquidCheck` 直接覆盖。
- `PropagationRevision`/`SnapshotRevision` 必须与同一 generation 代关联；加载后的 settle 若改变事实，必须产生新 revision。
- 运行期 `numLiquid`、`quickSettle`、panic 和整个 solver 内部状态不属于该 Component 的字段。

#### 生命周期

生成液体传播开始时建立为未稳定；传播摘要随检查/settle 更新；达到明确 handoff 条件后标记 stable；WorldFile 加载后的 liquid settle 可能重新打开 revision；失败时记录原因；世界运行期可只读取交接结果而不把 solver 的所有内部状态放入本 Component。

#### Entity/World 范围

每个 World Entity 一个，表示该世界生成/加载代的液体交接状态。局部 work item 不在此 Component 中展开为世界级集合；其关系只以摘要字段表达。

#### ID 与关系字段

关联 Lifecycle、Pass 和 Ready 的 generation/propagation revision；与运行期 Liquid 状态的边界 owner 为 `crossSubsystemOwner: integration-review`，见 `BD-COMP-04`。

#### 当前 NLTX 映射

`LiquidPropagationSession` 和 `LiquidPropagationSystem` 提供局部运行/传播事实，故不是零覆盖；但现有状态是否能作为生成与运行期 handoff 的唯一 owner 未知，目标 Component 标为 `proposed`、覆盖为 `partial`。

#### 证据

Version4 Liquid 静态计数、运行期更新、work set、LiquidCheck 和合并：`D:\TRbackup\Version4\Terraria\Liquid.cs:20-49`、`:1015-1094`、`:1190-1426`；WorldFile 加载后 liquid settle 和 flag 清理：`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:658-787`；当前映射：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\LiquidPropagationSession.cs`。

### 5.13 ReadyTransitionComponent

`status: proposed`。当前有 World lifecycle ready 表面和 server host ready 表面，但没有证据证明存在完整、可审计的 World ready barrier Component。

#### 职责

持有世界从“生成/加载中”转为“允许发布/允许模拟”的必要 facet 结果：generation revision、Pass validation、Liquid stable、Housing complete、Persistence committed、publication revision 和失败原因。它不实现 barrier，不拥有 server host 状态，也不负责发通知。

#### 字段

| 字段 | 类型/形状 | 默认值 | 状态/owner |
| --- | --- | --- | --- |
| `GenerationRevision` | `ulong` | `0` | `proposed`；`crossSubsystemOwner: integration-review` |
| `PassesValidated` | `bool` | `false` | `proposed` |
| `LiquidStable` | `bool` | `false` | `proposed`；依赖 Liquid owner |
| `HousingComplete` | `bool` | `false` | `proposed`；依赖 Housing owner |
| `PersistenceCommitted` | `bool` | `false` | `proposed`；依赖 persistence owner |
| `PublicationRevision` | `ulong?` | `null` | `proposed` |
| `FailureReason` | `string?` | `null` | `proposed` |

#### 字段不变量

- `PublicationRevision` 非空前不能由该 Component 得出世界已发布；所有必需 facet 必须属于同一 `GenerationRevision`。
- 任一 facet 为 false、unknown 或 stale 时，`ReadyTransitionComponent` 不能被解释为 ready。
- `PersistenceCommitted` 只表示所需持久化提交已完成，不表示网络广播、host 启动或全部运行期状态已完成。
- `PassesValidated` 不能仅由 Pass 注册数量或 `RunPass` 入口存在性推导；Liquid/Housing 也不能由单一 host flag 替代。

#### 生命周期

generation 开始时清空；各独立状态簇完成后逐项更新；任何 facet 失效时撤销当前 publication candidate；持久化提交和最终验证完成后才可产生 publication revision；失败时保留失败原因并阻止 ready；重新生成时新建 generation revision。

#### Entity/World 范围

每个 World Entity 一个。server host、协议会话或网络连接只能观察/投影其状态，不作为同名 Component 的第二个 owner。

#### ID 与关系字段

通过 `GenerationRevision` 关联 Lifecycle、Plan/Pass、Liquid、Housing 和 persistence 结果；publication revision 的 owner、是否与 WorldFile revision 相同、以及与 server host ready 的关系由 `BD-COMP-05` 和 `BD-COMP-06` 裁决，并标 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`WorldGenerationLifecycleState.IsReady`、server host 的 `IsReady`、有限 generation validation 和 persistence 类型只能提供局部表面，不能证明完整 barrier，故目标 Component 标为 `proposed`、当前覆盖为 `partial`。

#### 证据

Version4 生成结束检查与通知：`D:\TRbackup\Version4\Terraria\WorldGen.cs:21654-21671`、`:21704-21711`；WorldFile header/metadata 提交：`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1263-1467`；当前映射候选：`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldGenerationLifecycleState.cs`、`dome\src\Terraria.Dome.Server\Startup\ServerHostState.cs`、相关 `WorldGenerationValidation`/persistence 状态。

## 6. Entity 与 Component 组合

### 6.1 World Entity 的候选完整组合

一个 World Entity 的候选组合如下：

`WorldDescriptorComponent` + `WorldGenerationRulesComponent` + `WorldGenerationLifecycleComponent` + `WorldGenerationPlanComponent` + `WorldGenerationPassStateComponent` + `WorldGenerationRandomStateComponent` + `WorldTerrainStateComponent` + `BiomeEcologyStateComponent` + `EcologyScheduleComponent` + `HousingScanStateComponent` + `TownHousingAssignmentComponent` + `WorldLiquidHandoffComponent` + `ReadyTransitionComponent`。

这只是 Component 组合关系，不定义任何执行顺序。所有生成代关联字段都必须通过 `GenerationId`/`GenerationRevision`/各自 revision 形成显式关系，不能依靠 Component 添加顺序、文件顺序或隐含全局变量。

### 6.2 组合阶段约束

| 状态阶段 | 必须存在或可解释的 Component 状态 | 组合含义 |
| --- | --- | --- |
| Descriptor/rules 未解析 | Descriptor 或 Rules 可为 unset，Lifecycle 不是 ready | 世界身份和输入尚未可供生成计划消费。 |
| Plan 已建立 | Descriptor、Rules、Lifecycle、Plan 存在；Pass/Random 可为未开始 | 已有生成代的静态输入，但没有 active Pass 完成事实。 |
| Generation active | Lifecycle、Plan、Pass、Random 存在；Terrain/生态/住房/液体可为部分 revision | 只表示过程存在，不表示可发布。 |
| Validation/handoff | Pass、Terrain、Biome、Housing、Liquid 各自有明确 revision；Ready facets 仍可为 false | 各状态簇的完成事实必须独立保留。 |
| Ready candidate | Ready 的全部 required facets 同一 generation revision 且无 failure | 允许形成 publication candidate；不能仅因 host ready 而跳过 facets。 |
| Failed | Lifecycle/Ready 保留 failure；未完成 Component 不得被解释为新鲜结果 | 失败信息优先于默认字段和旧 revision。 |

### 6.3 关系与范围限制

- Tile、Wall、Liquid work item、Room 候选和 NPC 居民不应因为存在这些字段就机械复制 13 个 World 级 Component。
- `HousingScanStateComponent` 的临时扫描值与 `TownHousingAssignmentComponent` 的稳定结果可以同时存在，但必须以 `SourceScanRevision` 表明是否采用了该扫描结果。
- `WorldLiquidHandoffComponent` 与运行期液体状态是交接关系，不是把运行期 solver 的全部字段嵌入 World Entity。
- server host 的 ready、WorldFile 的 metadata 和 World Entity 的 ReadyTransition 是三个不同关系面；最终 owner 需依赖 `BD-COMP-05`。

## 7. 组件拆分与合并决策

### 7.1 已作出的拆分

| 拆分 | 决策 | 理由 |
| --- | --- | --- |
| Descriptor 与 Rules | 拆分 | 世界是谁/有多大与世界如何生成/有哪些变体具有不同生命周期和持久化责任。 |
| Lifecycle 与 ReadyTransition | 拆分 | 过程状态的 `IsReady` 不能代替 Pass、Liquid、Housing、Persistence 完整 barrier。 |
| Plan、PassState 与 RandomState | 拆分 | 不可变计划、当前游标和随机演进是三个不同恢复与一致性边界。 |
| Descriptor 与 TerrainState | 拆分 | surface/rock/spawn/Dungeon 等 canonical geometry 不应与生成中计算的 ocean/beach/biome location facts 混合。 |
| BiomeEcology 与 EcologySchedule | 拆分 | 生态事实与更新 rate/budget/抽样调度的 owner 和 revision 语义不同。 |
| HousingScan 与 TownHousingAssignment | 拆分 | 扫描过程可丢弃、可重试；居民房间分配是稳定关系和持久化候选。 |
| LiquidHandoff 与运行期 Liquid 状态 | 拆分 | ready 所需交接摘要不应拥有 `numLiquid`、work set、quick settle 或 solver 内部状态。 |

### 7.2 保持为单一 Component 的理由

- `WorldDescriptorComponent` 仍把 identity 与稳定 geometry 作为一个 canonical surface；但它不吸收 rules、terrain facts 或 persistence 结果。
- `WorldGenerationLifecycleComponent` 仍把 Phase 与 Failure 作为过程生命周期整体；派生布尔值不成为额外可写 Component。
- `WorldGenerationPassStateComponent` 保留 active Pass 及其 cursor/失败/暂停事实；它不把每个 Pass 变为 Entity，因为当前证据只支持单一 active cursor。
- `ReadyTransitionComponent` 作为世界级 facet 汇总单元保留，但每个 facet 的来源 owner 必须显式关系化。

### 7.3 明确不做的合并

- 不把 `WorldGenerationServerState` 整体复制为 Component；它混合 server host、progression、ready、生成和协议边界。
- 不把 `WorldGenerationTrace`、`WorldGenerationCheckpoint` 和 `WorldGenerationRandomSnapshot` 直接拼成一个“GenerationStateComponent”；它们分别是诊断/快照/随机值对象候选。
- 不把 WorldGrid、Manifest、GenVars、TileMapStore 和 Liquid solver 作为 World Component 的字段集合。
- 不把 Hardmode 和 OreTiers 并入 `WorldGenerationRulesComponent`；Hardmode 转换保持在 WorldProgressionAndTransition 外部边界。

## 8. 不单独创建 Component 的对象

下列对象在本 Component-only 设计中保留为快照、值对象、外部状态或证据对象，不单独创建新的 Component：

| 对象/状态 | 不单独创建 Component 的原因 | 允许的关系 |
| --- | --- | --- |
| `WorldGen` | 它是 Version4 静态/过程聚合入口，不是稳定事实的单一 owner。 | 将其身份、规则、terrain、lifecycle 等不同状态映射到对应 Component。 |
| `WorldGenerator` controller | Controller 是编排概念；Component 只保存 Plan/Pass 的数据事实。 | `WorldGenerationPlanComponent` 和 `WorldGenerationPassStateComponent` 分别承载输入与 cursor。 |
| `GenVars` | 全局可变生成变量集合责任混杂，不能原样成为巨型 Component。 | 逐字段按 descriptor/rules/terrain/biome/housing 责任归属。 |
| `Manifest` | 更像生成/持久化快照或版本集合，不是一个独立世界行为状态。 | 由 Descriptor/Plan/Ready 的 revision/metadata 关系引用。 |
| `WorldGrid`、Tile、Wall、TileEntity | 是空间数据或外部存储事实；逐 Tile 组件化会扩大范围并遮蔽写入 authority。 | Terrain/Biome/Housing/Liquid 只保存摘要、坐标、revision 或关系。 |
| `WorldGenerationTrace` | 诊断/审计记录，不是业务 authority。 | 允许通过 generation revision 关联，但本文件不设计 trace Component。 |
| `WorldGenerationCheckpoint` | 快照/恢复载体，不等于当前 active state。 | 通过 Pass/Random/Plan 的 revision 关系引用。 |
| `WorldGenerationServerState` | 混合 server、progression、ready 和生成状态，整体复制会形成巨型组件。 | 拆分后只由明确 Component 持有其对应事实。 |
| `BiomeTileCheck`、`WaterCheck`、`RoomNeeds`、`QuickFindHome` | 查询/扫描过程或派生操作对象，不是稳定世界事实。 | 结果以 Biome、LiquidHandoff 或 HousingScan 的 revisioned 字段表达。 |
| `Liquid.numLiquid`、`quickSettle`、work set | Liquid solver 内部/运行期状态，不能作为生成 handoff 的全量模型。 | 只向 `WorldLiquidHandoffComponent` 提供摘要关系。 |
| `Hardmode`、ore tiers、后台转换状态 | 属于 WorldProgressionAndTransition 的外部生命周期，且有 `WorldFile.IOLock`/主线程边界。 | `WorldGenerationRulesComponent.StartInHardmode` 只保留创建输入候选。 |
| server host `IsReady` | Host 级观察/发布状态，不证明 World 级生成 facets 全部完成。 | 只能与 `ReadyTransitionComponent` 建立观察关系，不能取代它。 |

## 9. 当前 NLTX 组件覆盖

| 目标 Component | 当前 NLTX 类型/路径 | 覆盖判断 | 未覆盖或不能宣称的部分 |
| --- | --- | --- | --- |
| `WorldDescriptorComponent` | `src\WorldSession\WorldGeneration\WorldDescriptorState.cs`、`WorldBounds.cs` | `existing` | Version4 全部 WorldId/UniqueId/GeneratorVersion/Manifest owner 未闭合。 |
| `WorldGenerationRulesComponent` | `WorldRulesState.cs`、`WorldSecretSeedFlags.cs`、`WorldGameMode.cs`、`WorldEvilType.cs` | `existing` | 每个 Version4 flag、ExtraLiquid、冻结和 StartInHardmode 语义仍需逐项核对。 |
| `WorldGenerationLifecycleComponent` | `WorldGenerationLifecycleState.cs`、`WorldPreparationState.cs`、`WorldGenerationFailure.cs` | `existing` | 现有 `IsReady` 不能证明完整 barrier。 |
| `WorldGenerationPlanComponent` | `dome\...\WorldGenerationPipeline.cs`、Pass 相关类型 | `partial` 映射 | 不足以证明完整不可变计划、权重、版本和禁用集合。 |
| `WorldGenerationPassStateComponent` | `dome\...\WorldGenerationPassState.cs`、`WorldGenerationStageSnapshot.cs` | `existing` | 不足以证明 Version4 Pass 执行语义或提交完成含义。 |
| `WorldGenerationRandomStateComponent` | `dome\...\WorldGenerationRandomSnapshot.cs` | `partial` | stream version、cursor、checkpoint/generation 关联未闭合。 |
| `WorldTerrainStateComponent` | `dome\...\WorldGenerationTerrainState.cs`、WorldSession bounds/descriptor | `existing` | 事实覆盖与计算完成 revision 未逐项确认。 |
| `BiomeEcologyStateComponent` | `WorldInfectionAlignment*`、`TilePresenceScan*`、`BiomeSurfaceSystem` | `partial` | 没有已证实的世界级 Biome authority、标签词汇和统计 revision。 |
| `EcologyScheduleComponent` | `src\WorldSession\WorldGeneration\WorldEcologyScheduleState.cs` | `partial` | 当前混有住房相关 cursor，mutation budget/schedule revision 未证实。 |
| `HousingScanStateComponent` | `dome\...\WorldGenerationHousingState.cs`、`TileHousingRule*` | `partial` | 完整扫描临时状态、生成闭合和加载后重检接入未证实。 |
| `TownHousingAssignmentComponent` | `src\WorldSession\WorldGeneration\TownHousingRegistry.cs`、`TownHousingResidentKey.cs` | `partial` | 现有 int NPC key 是否等于稳定居民 identity、owner 和 entity 粒度未决。 |
| `WorldLiquidHandoffComponent` | `dome\...\LiquidPropagationSession.cs`、Liquid propagation 类型 | `partial` | 生成/运行期 handoff authority 和 stable 条件未证实。 |
| `ReadyTransitionComponent` | `WorldGenerationLifecycleState.IsReady`、`dome\...\ServerHostState.cs`、验证/持久化相关状态 | `partial` | 没有已证实的 Pass+Liquid+Housing+Persistence 完整 barrier。 |

当前 NLTX 的总体状态仍为 `partial`。已有类型说明设计覆盖面存在，但不等于 Version4 行为等价、全量成员覆盖、唯一写入 authority 或验证通过。

## 10. 组件级 evidence-gap

| Gap ID | 组件 | 缺失证据 | 对设计的影响 |
| --- | --- | --- | --- |
| `EG-COMP-01` | Plan/Pass | Version4 Pass 描述、权重、禁用规则与 `RunPass` 真实执行语义未全部闭合；关键入口存在空实现。 | `GenerationPassDescriptor`、PlanVersion、PassVersion 只能是 candidate/proposed，不能声明可执行等价。 |
| `EG-COMP-02` | Descriptor/Terrain | WorldId、UniqueId、section geometry、surface/rock/spawn/Dungeon 的唯一 canonical owner 和持久化边界未统一。 | 需要 `BD-COMP-01`、`BD-COMP-06`；暂不允许重复可写字段。 |
| `EG-COMP-03` | Rules/Biome | `WorldEvil` 是规则输入、生成结果还是生态 authority 的证据不一致。 | 两个 Component 的该字段标记 `partial`/`unresolved`，不得双写。 |
| `EG-COMP-04` | Biome/Ecology | `BiomeTileCheck`、感染统计、SceneMetrics 或 Alignment 结果的世界级范围、stale 语义和 revision 契约不足。 | `BiomeTags`、计数和 conversion revision 只能作为 proposed revisioned facts。 |
| `EG-COMP-05` | Housing | 完整 `RoomNeeds`/`QuickFindHome` 生命周期、扫描成功条件、生成后分配和加载后重检的闭合链未证实。 | Scan 与 Assignment 必须分离，`HousingComplete` 不能从 registry 存在性推导。 |
| `EG-COMP-06` | Housing identity | 当前 `TownHousingResidentKey(int NpcType)` 与稳定 persistent entity identity 的关系未知。 | `AssignedRooms` key 暂用 candidate，owner/范围进入 `BD-COMP-03`。 |
| `EG-COMP-07` | Liquid | Version4 生成结束 Liquid check、WorldFile 加载 settle、运行期 solver 的 stable handoff 条件未统一。 | Handoff 只能保存摘要；`Stable` 和 owner 进入 `BD-COMP-04`。 |
| `EG-COMP-08` | Ready | `WorldGenerationLifecycleState.IsReady`、server host `IsReady`、WorldFile commit、通知和完整 world barrier 的关系未证实。 | ReadyTransition 必须为 proposed；不得把 host ready 当作 world ready。 |
| `EG-COMP-09` | Tile authority | `Convert`、`PlaceTile`、`KillTile` 等直接写入与 `WorldStorage.TileMapStore` 的全局覆盖关系未证实。 | Terrain/生态/住房组件不能宣称拥有所有 Tile/TileEntity 写入。 |
| `EG-COMP-10` | Generation identity | generation revision、plan version、checkpoint revision、publication revision 是否同一版本体系未知。 | 共享 ID/版本类型进入 `BD-COMP-06`，各 Component 暂保本地 revision。 |
| `EG-COMP-11` | Hardmode boundary | `StartHardmode` 的创建输入与实际 Hardmode transition/ore tier 状态的跨边界契约未裁决。 | 普通生成 Component 不吸收 `HardMode`/`OreTiers`；只保留输入候选。 |
| `EG-COMP-12` | Persistence | WorldFile metadata、生成完成通知和 persistence committed 的最低完成条件未全部确认。 | Ready 的 `PersistenceCommitted` 保持独立 proposed facet。 |

## 11. 未决组件 owner

以下决定使用稳定 ID，直到有新的直接证据或架构裁决为止。每项的候选变化只讨论 Component ownership、字段和关系，不进入运行时实现。

### BD-COMP-01：WorldDescriptorComponent 的 identity/geometry owner

- 冲突字段：`WorldId`、`UniqueId`、`GeneratorVersion`、尺寸/section geometry、surface/rock/spawn/Dungeon 坐标可能同时出现在 WorldSession、WorldFile metadata、Dome server/world state 和 terrain state。
- 候选 owner：A）WorldSession `WorldDescriptorState` 作为唯一 World Entity Component；B）Dome/world persistence state 作为唯一 owner，WorldSession 只读镜像；C）identity 与 geometry 再拆成两个 Component。
- 对 Component 组成的影响：A 保持本设计的 `WorldDescriptorComponent` 单一 canonical surface；B 会增加跨边界只读关系并削弱 WorldSession 的直接 owner；C 会把本 Component 拆成 identity/geometry 两个 Component，并修改所有坐标关系字段。
- 当前不能裁决的原因：现有证据确认了 Version4 WorldFile 字段和 NLTX descriptor 类型，但没有证明运行期/持久化/协议三者的唯一写入入口及字段生命周期完全一致。
- 当前标记：`crossSubsystemOwner: integration-review`、`decision-required`。

### BD-COMP-02：Rules 中 HardMode/OreTiers 与 WorldProgressionAndTransition 的边界

- 冲突字段：`StartInHardmode`、运行期 `HardMode`、ore tier 状态以及 `WorldEvil` 可能被规则、世界描述或 progression 状态同时表达。
- 候选 owner：A）Rules 只拥有创建输入，Hardmode/OreTiers 全部外置；B）Rules 持有初始与当前值；C）另建 progression Component，并在 Rules 中保留只读 initial snapshot。
- 对 Component 组成的影响：A 是当前候选，保持 `WorldGenerationRulesComponent` 轻量；B 会把普通生成与 Hardmode transition 合并；C 会新增跨子系统关系和 initial/current 两组字段。
- 当前不能裁决的原因：Version4 `StartHardmode` 包含后台转换、I/O lock 和主线程 follow-up，当前证据不足以给出完整跨边界 contract；因此不将 `HardMode`/`OreTiers` 放入本设计。
- 当前标记：`crossSubsystemOwner: integration-review`、`decision-required`。

### BD-COMP-03：TownHousingAssignmentComponent 的 owner 和实体范围

- 冲突字段：Town housing registry、NPC/town domain、WorldSession 的 `TownHousingResidentKey(int NpcType)`、持久化居民 identity 都可能持有 assignment/homeless 结果。
- 候选 owner：A）World Entity 上的居民 ID 到房间的集合；B）Town/居民集合 Entity 上的 assignment Component，再由 World 建立汇总关系；C）继续以 `TownHousingResidentKey` 作为兼容 key，暂不引入 persistent ID。
- 对 Component 组成的影响：A 保持一个 World 级 `TownHousingAssignmentComponent`；B 会改变 Entity/Component 粒度并增加关系集合；C 字段类型保持 int，但不能证明跨加载/重命名/实体生命周期稳定。
- 当前不能裁决的原因：当前 NLTX 只确认了 int NPC key 和 registry 表面，Version4 加载后 `KickOut`/homeless 语义与稳定实体 identity 的一一对应尚未证实。
- 当前标记：`crossSubsystemOwner: integration-review`、`decision-required`。

### BD-COMP-04：WorldLiquidHandoffComponent 的 owner

- 冲突字段：Version4 `Liquid.numLiquid`、work set/settle、`LiquidPropagationSession`、运行期 liquid solver、WorldFile 加载 settle 都可能声明 liquid stable 或完成。
- 候选 owner：A）World Entity 的 handoff Component 只拥有摘要；B）Liquid domain 拥有完整 solver state，World Component 只读引用；C）生成与运行期各自拥有一个独立 handoff/solver Component。
- 对 Component 组成的影响：A 保持本设计的 6 字段摘要；B 会减少 World Component 字段但增加外部关系；C 会增加 generation handoff 与 runtime liquid boundary 的两个 Component，且需要清晰 revision 转换。
- 当前不能裁决的原因：直接源码同时显示生成结束检查、加载后 settle 和运行期 solver，尚没有统一的“stable”最低条件及唯一发布 owner。
- 当前标记：`crossSubsystemOwner: integration-review`、`decision-required`。

### BD-COMP-05：ReadyTransitionComponent 的 owner 和字段组成

- 冲突字段：lifecycle `IsReady`、server host `IsReady`、Pass validation、Liquid settle、Housing complete、WorldFile commit 和 Finish 通知分别提供 ready-like 表面。
- 候选 owner：A）World Entity 的 ReadyTransition 作为唯一 barrier facet 汇总；B）Lifecycle owner 只保留 phase，另由 persistence/publication domain owner；C）每个子域发布 facet，World 只保存只读聚合结果。
- 对 Component 组成的影响：A 保持本设计的 7 个字段；B 会删除或缩小 ReadyTransition；C 会增加若干 facet Component，并要求明确聚合关系；任何方案都不能把 server host flag 直接当 canonical world ready。
- 当前不能裁决的原因：现有证据确认了 Finish/通知、WorldFile 提交和 host ready 的不同位置，但未确认完整 world barrier 的必要 facet、发布时点和撤销规则。
- 当前标记：`crossSubsystemOwner: integration-review`、`decision-required`。

### BD-COMP-06：共享 ID、坐标和版本类型 owner

- 冲突字段：`WorldId`/`UniqueId`、`GenerationId`/`GenerationRevision`、Pass/plan version、checkpoint/publication revision、`TilePosition`/section 坐标和 NPC identity 在多个域内可能有相似但不等价类型。
- 候选 owner：A）WorldSession 提供共享值类型；B）Dome domain 提供共享值类型，WorldSession 适配；C）各 Component 保持局部类型，只通过明确映射关联。
- 对 Component 组成的影响：A 能简化 13 个 Component 的关系字段但提高公共类型耦合；B 会扩大 dome 对 WorldSession 的依赖；C 会减少共享耦合但增加映射字段和 unresolved 状态。
- 当前不能裁决的原因：研究证据足以确认字段名称和用途簇，但不足以确认它们是同一生命周期、同一持久化版本或同一坐标协议；不能仅凭同名字段合并。
- 当前标记：`crossSubsystemOwner: integration-review`、`decision-required`。

## 12. 最终 Component 清单

在当前证据和未决项未解决前，候选基线为以下 13 个 Component；这不是代码创建清单，也不是迁移顺序。

| # | Component | 当前状态 | Entity/World 范围 | 核心关系 |
| ---: | --- | --- | --- | --- |
| 1 | `WorldDescriptorComponent` | `existing` | 每 World 一个 | `WorldId`/`UniqueId`、geometry；`BD-COMP-01/06` |
| 2 | `WorldGenerationRulesComponent` | `existing` | 每 World 一个 | rules 与 descriptor；Hardmode 边界 `BD-COMP-02` |
| 3 | `WorldGenerationLifecycleComponent` | `existing` | 每 World 一个 | Phase/failure 与 generation revision |
| 4 | `WorldGenerationPlanComponent` | `proposed` | 每 generation 一个 World 级值 | Plan 与 Pass ID/version |
| 5 | `WorldGenerationPassStateComponent` | `existing` | 每 World 一个 active cursor | Plan、checkpoint、generation revision |
| 6 | `WorldGenerationRandomStateComponent` | `partial` | 每 generation 一个 World 级值 | seed identity 外置；Pass/cursor 关联 |
| 7 | `WorldTerrainStateComponent` | `existing` | 每 World 一个 | terrain facts 与 descriptor bounds |
| 8 | `BiomeEcologyStateComponent` | `proposed` | 每 World 一个 | Biome/conversion revision；`WorldEvil` owner 未决 |
| 9 | `EcologyScheduleComponent` | `partial` | 每 World 一个 | rules 与 Biome schedule revision |
| 10 | `HousingScanStateComponent` | `proposed` | 每 World 当前 scan 一个 | scan revision 与 assignment source |
| 11 | `TownHousingAssignmentComponent` | `proposed` | World 或 Town/居民集合，未决 | resident ID、room、homeless；`BD-COMP-03` |
| 12 | `WorldLiquidHandoffComponent` | `proposed` | 每 World 当前 generation 一个 | propagation revision 与 Liquid solver；`BD-COMP-04` |
| 13 | `ReadyTransitionComponent` | `proposed` | 每 World 一个 | generation facets 与 publication；`BD-COMP-05/06` |

清单中的 `existing`/`partial` 只表示当前 NLTX 的映射证据等级；清单中的 `proposed` 表示目标组件边界尚未以同名完整代码存在。任何 Component 的 `status` 都不能解释为已迁移、已接入或已验证。

## 13. 最终声明

本文件是 Component-only Design。

本文件不定义 System、Query、Command、Event、Adapter、Projection、
调度顺序、运行时实现、测试计划或迁移计划。

所有 status: proposed 的 Component 都只是设计提案。
本文件不声明代码已创建、已迁移、行为等价或验证通过。

`designStatus: decision-required`、`evidenceStatus: partial`、
`nltxStatus: partial` 和 `verificationStatus: not-run` 保持有效。尤其是：

- 当前已有 `WorldGenerationPipeline`、随机状态、局部 Tile/Liquid 提交和有限验证，不能被表述为“完全没有生成执行器”；
- 局部 Tile commit 不能被表述为所有 Tile/TileEntity 写者的全局唯一 authority；
- server host `IsReady` 不能被表述为完整 World ready barrier；
- Version4 参考源码中的空入口、控制器或类型存在性不能被表述为已经具备完整行为等价。
