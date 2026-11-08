# 哪些类型可以放进 `src/NSSLC/Components/World`

```text
文档 ID：DOC-2026-11-06-world-domain-placement-review
逻辑域：reviews
产物类型：design
状态：active
范围：src/NSSLC/Components/World/**（判定时 37 个 .cs，执行后 38 个）
      src/NSSLC.Infrastructure/WorldData/**（判定时 58 个 .cs，执行后 57 个）
      src/NSSLC/Components/Share/Physics/Liquid/LiquidBlockingRules.cs（对照物）
证据入口：src/NSSLC/Components/World/WorldTileStorageComponent.cs
      src/NSSLC/Components/World/Model/WorldTileState.cs
      src/NSSLC/Components/World/Model/WorldTileCoordinate.cs
      src/NSSLC/Components/World/Model/WorldFacts.cs（执行后位置）
      src/NSSLC/Components/Share/Physics/Liquid/LiquidBlockingRules.cs:16
canonical 路径：docs/reviews/audits/2026-11-06-world-domain-placement-review.md
```

本文回答一个问题：**除了 `Components/World` 原有的 37 个文件，还有哪些类型应该进来。**
结论：只有 `WorldFacts` 一项可以明确放入；§5 已执行该迁移，其余候选要么是编解码器、
要么是持久化 DTO，按本仓库约束必须留在基础设施。

## 1. 判定规则

规则不是我定的，按仓库既有约束抽取，后面每条判定都引用编号：

| 编号 | 规则 | 出处 |
| --- | --- | --- |
| R1 | ECS 领域层承载权威游戏状态、领域不变量、规则判断与受控提交 | [ECS 领域与基础设施架构边界](../../../Context/架构设计/ECS领域与基础设施架构边界.md) §3 |
| R2 | 持久化快照与 DTO 是**跨层数据契约，不是 ECS Component** | 同上 §4.2 |
| R3 | 文件路径、字节流、编解码器与平台异常属于基础设施适配器 | 同上 §3、§5 |
| R4 | 目录先表达领域能力，子目录按需引入，禁止预建空目录/未用 API | [ECS 文件组织设计约束](../../../Context/架构设计/ECS文件组织设计约束.md) §2.1、§2.3 |
| R5 | 一个公开类型对应一个同名 PascalCase 文件 | 同上 §2.5 |
| R6 | 依赖由外向内：基础设施可依赖领域公开契约，领域不依赖基础设施 | 边界文档 §5 |

## 2. 两侧现状

### 2.1 `src/NSSLC/Components/World`（判定时 37 文件 / 执行后 38 文件，`Terraria.World`）

| 分组 | 文件数 | 代表类型 |
| --- | --- | --- |
| 根目录 | 12 | `WorldRuntime`、`WorldTickSystem`、`WorldInteractionSystem`、`WorldItemStore`、`WorldRuntimeStateComponent`、`WorldTileStorageComponent` |
| `Contract/` | 5 | `IWorldTileGrid`、`WorldInteractionWorld`、`IWorldProjectionSource`、`IWorldItemQuery` |
| `Model/` | 12 → 13 | `WorldTileState`、`WorldTileCoordinate`、`WorldMutation`、`WorldItemState`、`WorldTimeAdvance`、**`WorldFacts`（§5 迁入）** |
| `Solver/` | 8 | `WorldTileTransitionRules`、`WorldTileDropRules`、`WorldTileMotionSolver`、`WorldTimeAdvanceSolver` |

域内已经存在同名概念，这是后面判定重复项的依据：
`WorldTileState`（16 字段 tile 值）、`WorldTileCoordinate`（坐标）、
`Terraria.Liquid.LiquidContactFlags`（液体种类）、`LiquidBlockingRules`（tile 阻挡表）。

### 2.2 `src/NSSLC.Infrastructure/WorldData`（判定时 58 文件 / 执行后 57 文件，4 个命名空间）

| 分组 | 文件数 | 内容 |
| --- | --- | --- |
| `Persistence/` | 12 | 5 个端口（`IWorldPersistenceDocumentDecoder` 等）+ 7 个结果/失败类型 |
| `Persistence/WorldData/` | 37 | `.wld` 各 section 的 DTO + `WorldPersistenceDocument` |
| `WorldStorage/` | 5 | `TileCoordinate`、`TileEntityId/TypeId`、`TileEntitySnapshot`、`WorldLoadSection` |
| `Items/` | 1 | `ItemState` |
| `Tile/` | 3 → 2 | `WorldTile`、`WorldFileTilePayloadDecoder`（`WorldFacts` 已迁出） |

### 2.3 其他可能的候选池（本次只做否决记录）

| 候选池 | 代表类型 | 判定 | 理由 |
| --- | --- | --- | --- |
| `NSSLC.Infrastructure/WorldStorage/WorldFile/` | `WorldFileDocumentDecoder`、`WorldFileTileCodec`、`WorldFileDocumentEncoder` | ✗ | 编解码器（R3），与 §3 第 7 行同类 |
| `NSSLC.Infrastructure/WorldGeneration/Runtime/` | `Tile`、`Liquid`、`TileEntity`、`TileObjectData` | ✗ | Version4 世界生成移植层，不是 ECS 运行时权威状态（R1/R4） |
| `NSSLC/Components/Communication/` | `TerrariaProtocolAdmissionHandler`（tile 帧重要性编解码） | ✗ | 协议适配，属通信域（R3） |
| `NSSLC/Components/Share/Physics/Liquid/` | `LiquidSimulation`、`LiquidBlockingRules`、`LiquidContactFlags` | ✗（不搬） | 液体是独立领域；只有其中的 tile 事实表应与 `WorldFacts` 合并（§4.2） |
| `NSSLC.Server/` | `ServerWorldLoader` | ✗ | 宿主组装与文件 I/O（R3） |

## 3. 判定总表

| # | 类型 | 现位置 | 判定 | 依据 |
| --- | --- | --- | --- | --- |
| 1 | `WorldFacts` | `WorldData/Tile/WorldFacts.cs` | ✅ **已放入**（§5） | R1：tile 类型事实（solid / solidTop / frameImportant / saveSlopes）是领域知识，不是文件格式知识 |
| 2 | `WorldTile` | `WorldData/Tile/WorldTile.cs` | ◐ 先重构再谈 | R1+§4.1：域内已有 `WorldTileState`，并存会形成两套 tile 值类型 |
| 3 | `TileWireMask` | `WorldData/Tile/WorldTile.cs` | ◐ 先重构再谈 | 域内 `WorldTileState.Wires` 用 `byte`，迁入需同步改字段类型 |
| 4 | `LiquidType` | `WorldData/Tile/WorldTile.cs` | ✗ 不迁，应删除 | R1+§2.5：与域内 `LiquidContactFlags` 语义重复，两套枚举必然漂移 |
| 5 | `TileCoordinate` | `WorldData/WorldStorage/TileCoordinate.cs` | ✗ 不迁 | R2+R6：DTO 被领域类型签名会反向耦合；域内已有 `WorldTileCoordinate` |
| 6 | `WorldTileDelta` / `WorldTilePatch` | `WorldData/Tile/WorldTile.cs` | ✗ 暂不迁 | R4：全仓库零消费者（Packet 20 patch 未实现），迁入即预建未用 API |
| 7 | `WorldFileTilePayloadDecoder` | `WorldData/Tile/` | ✗ 留在基础设施 | R3：字节流 → 行的解码器 |
| 8 | `Persistence/` 12 + `WorldData/` 37 | `WorldData/Persistence/**` | ✗ 留在基础设施 | R2：跨层数据契约 |
| 9 | `WorldLoadSection<T>` | `WorldData/WorldStorage/` | ✗ 留在基础设施 | R2：持久化区段可选性表达 |
| 10 | `TileEntitySnapshot` / `TileEntityId` / `TileEntityTypeId` | `WorldData/WorldStorage/` | ✗ 留在基础设施 | R2；且域内没有 TileEntity 领域，迁入需先有权威 owner（R4） |
| 11 | `ItemState` | `WorldData/Items/` | ✗ 留在基础设施 | R2：存档物品值；与 `Terraria.Item` 域的运行时物品不是同一概念 |

## 4. 唯一明确可放入：`WorldFacts`

### 4.1 为什么是它

`WorldFacts` 的内容全部是 **tile 类型的游戏属性**，来源是 Version4 的
`Main.Initialize_TileAndNPCData`（solid、solidTop、frameImportant、saveSlopes、
compression batching、liquidMergeReplaceable），不是 `.wld` 文件格式知识：
文件格式只决定"这些表怎么落字节"，不决定"哪种 tile 是实心"。

### 4.2 域内已经有一份，而且完全一致

`LiquidBlockingRules`（`Components/Share/Physics/Liquid`，118 行）自带
`SolidTiles`（324 条）与 `SolidTopTiles`（84 条）查找表，注释写明"从 world tile facts 抬出来的"。
本次逐条比对 `WorldFacts.SolidTileIds`/`SolidTopTileIds` 与它的两份集合：

```text
WorldFacts Solid=324   LiquidBlockingRules Solid=324   差异 0 条
WorldFacts SolidTop=84 LiquidBlockingRules SolidTop=84  差异 0 条
```

确认表内容完全相同，例外清单（tile 138/484/546/664/711–716）与判定公式
（`Active && !Inactive && Solid && !SolidTop`）也相同；唯一差异是查找表长度
（`WorldFacts.TileCount = 753` vs `LiquidBlockingRules` 的 1024）。

**也就是说，同一份游戏事实现在有两个物理副本。** 这正是"应该放进 World"的最强理由：
不合并它们，任何一次 Version4 tile 变更都只改到其中一份。

### 4.3 `WorldFacts` 的死成员

| 成员 | 外部消费者（迁移前） | 处置 |
| --- | --- | --- |
| `TileCount` / `WallCount` | 解码器（边界校验） | 保留 |
| `FrameImportant` / `SaveSlopes` | 解码器 | 保留 |
| `IsValidTileType` | 解码器 | 保留 |
| `Solid` / `SolidTop` | **0**（域内用 `LiquidBlockingRules`） | 保留并成为唯一副本（§5 已执行） |
| `BlocksLiquid(WorldTile)` / `AllowsLiquidMerge(WorldTile)` | **0** | 删除（签名还引用基础设施的 `WorldTile`，域内无法编译） |
| `Fingerprint` / `ComputeFingerprint` | **0** | 删除 |
| `CopyFrameImportantForProtocolInputs` / `CopyCompressionBatchingForProtocolInputs` | **0** | 删除 |
| `AllowsSaveCompressionBatching` / `LiquidMergeReplaceable` | **0** | 降为 `internal` 保留：它们是 Version4 事实表而非派生数据，删除会让后续实现重新推导 |

## 5. 已执行的迁移（2026-11-06）

| 步骤 | 动作 | 结果 |
| --- | --- | --- |
| 1 | `WorldFacts` 移到 `src/NSSLC/Components/World/Model/WorldFacts.cs`，命名空间 `Terraria.World` | 完成 |
| 2 | `LiquidBlockingRules` 删除自带的 324/84 两份 tile 表与两个查找数组，改为 `Facts.Solid`/`Facts.SolidTop`，边界由 `WorldTileState.Type < WorldFacts.TileCount` 守卫 | 完成（`LiquidBlockingRules.cs` 118 → 73 行） |
| 3 | `WorldData` csproj 增加 `..\..\NSSLC\Terraria.Components.csproj` 引用（基础设施 → 领域，R6 允许；`Terraria.Components` 不引用任何项目，无环） | 完成 |
| 4 | 删除 §4.3 表中列出的死成员，两张大事实表降为 `internal` | 完成 |
| 5 | 验证 | 见 §5.1 |

### 5.1 验证记录

```text
dotnet build src\NSSLC.Server\NSSLC.Server.csproj -v minimal   ->  已成功生成，0 错误，exit=0
dotnet Build\bin\NSSLC.Server\Debug\net10.0\NSSLC.Server.dll "D:\TRbackup\NLTX\src\World\科研.wld"
  ecs.protocol.world_cache_ready sections=672 compressedBytes=8402777 elapsedMs=3434
  ecs.server.ready listen=127.0.0.1:7777 world=8400x2400 spawn=4195,541
```

冒烟输出的 `sections`、`compressedBytes`、`spawn` 与迁移前逐字相同，说明 tile 载荷解码与出生点选择未变。

**行为等价依据**：`LiquidBlockingRules` 原来的两张表与 `WorldFacts` 对应表逐条比对差异为 0（§4.2），
例外清单与判定公式相同，表内最大 tile id 为 750 < 753，因此把查找边界从 1024 收到 `TileCount`
不改变任何可达输入的结果。

**回滚方式**：还原 `LiquidBlockingRules.cs`、把 `WorldFacts.cs` 移回 `WorldData/Tile/` 并删除
`WorldData` 对 `Terraria.Components` 的引用即可，三步互相独立。


## 6. 不建议做的事

- **不要**把 `WorldFileTilePayloadDecoder` 或任何 `Persistence/*` 搬进 `Components/World`：
  那会把"读 `.wld` 字节"变成领域职责（R2/R3）。
- **不要**为了让 `TileCoordinate` 统一而让 37 个 DTO 依赖域内类型（R6 允许依赖，但
  持久化契约一旦引用领域类型，领域演进会连带改存档契约）。
- **不要**在域内预建 `TileEntity` 领域或 `Data/`、`Utils/` 之类的目录（R4）。
- **不要**改名域内现有的 `Contract/Model/Solver` 子目录：37 文件规模下它们有实际导航价值，
  改名会波及全部 `using`，收益不足。

## 7. 未验证事项

- `WorldFacts` 与 `LiquidBlockingRules` 的比对覆盖 solid/solidTop 两张表与例外清单/公式；
  `FrameImportant`、`SaveSlopes` 未与 Version4 原表逐条核对（只核对了两副本之间的一致性）。
- 保留为 `internal` 的 `AllowsSaveCompressionBatching`、`LiquidMergeReplaceable` 两张大表
  仍无消费者；未来接入存档压缩或液体反应时才应接线，不应提前暴露为 public。
- 本次未做行为差分（无 Version4 对照运行），等价性依据是"表逐条相同 + 判据相同 + 边界收缩不影响
  可达输入"，不是运行期对比结论。
