# WorldGen ECS 迁移剩余执行提案

> 状态：提案已完成（提前结案）；代码迁移仍为部分完成，采用约 40% reduced verifier
> 范围；完整测试、构建和 runtime differential 仍暂停。
>
> 前置提案：`docs/plans/2026-08-18-worldgen-ecs-migration-execution-proposal.md`。
>
> 不变量：旧 `WorldGen` 保留，且 `canRemoveLegacyWorldGen=false`。

## 1. 当前基线

当前迁移证据的静态计数如下：

```text
WorldGen methods:        684
Partial mappings:        227
Unmapped methods:         457
Unmapped fields:         233
Legacy removal allowed:  false
Validation scope:        reduced (32/81 WorldGeneration sections, 39.5%)
Builds/runtime diff:     paused
```

近期已建立的 Simulation 基础边界为：

```text
immutable snapshot
  -> pure query / system
  -> TileChangeCommand
  -> TileChangeCommitSystem
  -> immutable snapshot / protocol projection
```

`UpdateTileType` 与 `UpdateTileShape` 已能保留未声明变更的 `WorldTile` 状态。后续优先
收敛为可组合的事务化 Tile mutation，避免继续创建仅覆盖单字段的临时命令种类。

本轮新增 `CheckOnTable1x1` 的 partial pure mapping：`OnTable1x1ValidationQuery` 从 immutable
snapshot 计算普通 solid 支撑、平台侧连接和 type-78 bottom-slope 分支；`AnchorValid(Table)`、
`tileTable` registry、`KillTile` 及其宿主副作用仍 deferred。当前静态计数为 `184 Partial /
500 Unmapped`，删除门槛仍关闭。该记录属于早期切片基线；当前 inventory 已推进至
`227 Partial / 457 Unmapped`。

随后新增 `CheckSunflower` 的 partial pure mapping：`SunflowerValidationQuery` 从 immutable
snapshot 计算 2x4 footprint、frame 坐标、允许的 solid ground 类型和完整性结果；对象销毁、
掉落和 recursive framing 仍 deferred。该记录属于早期切片基线；当前 inventory 已推进至
`227 Partial / 457 Unmapped`，删除门槛仍关闭。

## 2. 阶段 A：当前静态收口

### 写集

- Modify: `docs/worldgen/worldgen-source-inventory.json`
- Modify: `docs/worldgen/worldgen-method-map.md`
- Modify: `docs/worldgen/worldgen-deletion-gate.json`
- Modify: `docs/worldgen/worldgen-parity-report.md`
- Modify: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
- Review: `src/Terraria.Dome.Simulation/WorldGeneration/MossColorQuery.cs`

### 动作

1. 核对 `MossColorQuery` 的 22 个 Tile ID 与默认 `-1`，以及 source line `67944` mapping。
2. 解析 `worldgen-source-inventory.json`，确认实际统计为 `227 Partial / 457 Unmapped`。
3. 核对 method map、deletion gate 和 parity report 与 inventory 一致。
4. 执行非 .NET 静态检查：JSON 解析、100 列限制、禁用依赖、`git diff --check`。
5. 保持 `canRemoveLegacyWorldGen=false`，不修改 legacy 编译入口。

### 完成条件

本阶段仅证明代码、inventory 与文档静态一致；不宣称编译成功、测试通过或行为等价。

当前阶段 A 静态收口已完成：`MossColorQuery` 的 22 个 Tile ID 与默认 `-1` 已复核；
inventory 为 `684` methods、`233` fields、`215 Partial / 469 Unmapped`；method map 的
684 个 line/name/status key 与 inventory 零不匹配；216 个带路径 mapping target 全部解析到
现有文件；JSON、Simulation 禁止依赖、WorldGeneration 风格和 `git diff --check` 均通过。
这些证据不改变阶段 G 的 build、verifier 和 runtime differential 暂停状态。

## 3. 阶段 B：TileFrame 计算边界

这是当前优先级最高的缺口。`TileMergeAttemptFrametest`、植物转换、Tile shape 更新和 rope
framing 都需要真实 frame 计算，不能把旧 frame 回写后伪装为已经重新 framing。

### 建议契约

```text
TileFrameRequest
  - X, Y
  - Source mutation kind
  - Snapshot before mutation
  - Pending tile mutations

TileFrameEvaluationResult
  - FrameX, FrameY
  - Optional shape normalization
  - Optional kill / replace intent
  - Affected neighbor coordinates
```

### 写集

- Create: `src/Terraria.Dome.Simulation/WorldGeneration/TileFrameRequest.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/TileFrameEvaluationResult.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/TileFrameEvaluationQuery.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/Systems/TileFrameCommandSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/World/Commands/TileFrameCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/TileMergeFrametestResult.cs`
- Modify: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`

### 动作

1. 建立不可变 request/result 契约。
2. 先支持非 frame-important、非液体、非树、非藤蔓、非多格对象的普通 solid Tile。
3. 只从 snapshot 加 pending command overlay 读取邻居；禁止直接读取或写入 `WorldGrid`。
4. 将 `TileMergeFrametestResult` 的 cardinal frame-work 转换为 `TileFrameRequest`。
5. 仅在 frame 已计算后生成 `TileFrameCommand`。
6. 固定提交顺序：

   ```text
   Tile mutations -> type/shape commit -> frame evaluation -> frame commit
   ```

### 排除范围

- `TileFrameImportant`
- 液体排队
- `CheckVines`
- `CheckCactus`
- 掉落物、实体、音效、网络、地图更新
- legacy 吞掉异常的 `try/catch` 行为

### 完成条件

同一 snapshot 和 pending mutation 集重复执行时，frame 结果、影响坐标和命令序列完全一致。

## 4. 阶段 C：Tile 连接与 framing 子族

在 TileFrame 契约稳定后，按复杂度处理相邻 Tile framing 行为。

### 第一批

- `GetTileMergeCulling`（已完成 bounded invisible-block culling；宿主可见性策略作为显式输入，
  不迁移 `Main.ShouldShowInvisibleBlocksAndWalls`。）
- `IsTreeType`（已完成显式 trunk registry membership query；registry ownership 仍由宿主提供。）
- `paintColor`（已完成固定 RGBA mapping；legacy Color 类型与 paint/effect 应用仍 deferred。）
- `coatingColor`（已完成固定 RGBA mapping；legacy Color 类型与 coating 应用仍 deferred。）
- `coatingColors`（已完成 fullbright/invisible block/wall 选择查询；legacy 可变 Color list 仍 deferred。）
- `SetForestBGSet`（已完成全部固定 style 的 immutable mountain/tree set mapping；caller-owned array mutation 与宿主背景应用仍 deferred。）
- `GetHollowTreeFoliageStyle`（已完成 hallow background style 到 foliage style 的 bounded mapping；背景状态与 TreeTops registry ownership 仍 deferred。）
- `InvalidTileForPilesOrSpeleothems`（已完成 world-margin、active 与显式 boulder registry 谓词；registry ownership 与破坏副作用仍 deferred。）
- `CheckVines`（已完成 support/slope 分类、replacement 和 kill intent；`SquareTileFrame`、`KillTile` 与宿主 mutation 仍 deferred。）
- `SquareTileFrame`（已完成九点 request topology/order；`resetFrame`、实际 frame 计算与 Tile mutation 仍 deferred。）
- `SquareWallFrame`（已完成九点 wall coordinate topology/order；`resetFrame`、`Framing.WallFrame` 与 wall mutation 仍 deferred。）
- `RangeFrame`（已完成 expanded rectangle coordinate topology/order；`MapUpdateQueue`、实际 tile/wall framing 与 mutation 仍 deferred。）
- `Cull`（已完成八邻域 cull-mask immutable application；legacy ref mutation 与 cache ownership 仍 deferred。）
- `IsConsideredTheSpawnArea`（已完成 remix、randomized/no-surface 与普通 surface 的显式输入谓词；Main/GenVars ownership 仍 deferred。）
- `GetTileTypeCountByCategory`（已完成五类固定 count formula；TileScanGroup ownership 与扫描/计数 mutation 仍 deferred。）
- `CountTileTypesInArea`（已完成 explicit snapshot rectangle active count vector；caller-owned array mutation 与 host scanning 仍 deferred。）
- `Housing_GetTestedRoomBounds`（已完成 fixed expansion/clamp pure calculation；room globals、housing scan 与 scheduling 仍 deferred。）
- `ScoreRoom_CanBeHomeSpot`（已完成 active type-379 rejection；room scoring、scan 与 NPC scheduling 仍 deferred。）
- `RoomNeeds`（已完成四类 required tile 的显式 set classification；room flags、registry ownership、scoring 与 scheduling 仍 deferred。）
- `Housing_CheckIfInRoom`（已完成显式 room-coordinate membership；mutable roomTiles ownership、scan 与 scheduling 仍 deferred。）
- 纯 tile merge / merge mask 分类
- `HandleRopeEndFraming` 的端点定位与 frame-work request
- 已迁移 `TileMergeAttemptFrametest` 的 frame-work 消费

### 第二批

- `CheckVines`
- `CheckCactus`
- `TileFrameCosmetic` 中可分离的 frame 选择逻辑
- rope、平台、特殊 Tile 的邻居连接规则

### 第三批

- `TileFrameImportant`
- 门、宝箱、家具、树、压力板、多格对象
- frame-important Tile 的 footprint 与对象所有权规则

每一批严格拆分：

```text
pure classification / frame evaluation
  -> command generation
  -> world commit
  -> object or entity side effects
  -> protocol projection
```

不得将 Tile Entity、掉落物、网络发包或 `Main.tile` 引回 Simulation。

## 5. 阶段 D：对象销毁与 Tile Entity 事务

`CheckFoodPlatter` 是后续最小的对象生命周期切片之一，但必须先建立对象事务边界。

### 目标形态

```text
ObjectDestructionEligibilityQuery
  -> ObjectDestructionCommand
  -> TileChangeCommand batch
  -> TileEntityRemovalCommand
  -> ItemDropCommand
  -> deterministic commit
  -> server / protocol projection
```

### 动作

1. 迁移 Food Platter 的支撑丢失销毁决策。
2. 将 Tile Entity 是否存在、是否携带物品作为显式 snapshot 输入。
3. 将 Tile kill、Tile Entity 移除、物品掉落组织为原子命令批次。

### 排除范围

- `Main.LocalPlayer.InterruptItemUsageIfOverTile`
- 本地 UI
- `NetMessage`
- 直接 `KillTile`
- legacy 全局 `destroyObject` 重入标志

### 完成条件

结构化对象只能完整销毁并产生完整批次，或零变更失败；不允许半删除。

## 6. 阶段 E：基础 Tile 操作事务收敛

当前已有 `Kill`、`Place`、`SetWall`、`SetInactive`、`UpdateTileType`、
`UpdateTileShape`。在 framing 完成后，应收敛为统一 mutation 模型。

### 目标契约

```text
TileMutationCommand
  - Sequence
  - Coordinates
  - Expected source state / conflict policy
  - Active, Type, Wall, Frame, Liquid, Shape, Actuation deltas
  - Preserve / clear semantics
  - Source domain
```

### 提交器职责

- 按明确优先级折叠同坐标命令或拒绝冲突。
- 保留未声明修改的 Tile 字段。
- 让 type、shape 与 frame 形成单个事务。
- 仅在提交成功后递增 section version。
- 失败不能留下局部状态。
- 协议层仅消费最终 section snapshot，不能观察内部命令顺序。

测试暂停期间不进行大规模命令重构；先在新 framing 切片中验证契约是否足够。

本轮已完成一个受限的合同收敛：`TileMutationProjection` 统一
`Kill`、`SetWall`、`SetInactive`、放置、类型更新和 shape 更新在 pending overlay
与 `TileChangeCommitSystem` 中的字段投影。`Kill` 现在明确区分
`PreserveLiquid=false` 的全量清除和 `PreserveLiquid=true` 的仅液体保留。完整
`TileMutationCommand`、冲突策略和协议提交边界仍未完成。

pending overlay 也已按命令 `Sequence` 统一排序后再投影，避免同一 mutation 集因
集合枚举顺序不同而产生不同 framing 结果。

本批次另完成 `Check2x2Style` 的纯验证前缀：2x2 原点、style band、frame-X、底部
支撑，以及类型 254 的允许支撑类型。对象删除、掉落、随机状态和递归 framing 仍保持
未映射。

同批次完成 `Check2x1` 的普通/桌类支撑验证；类型 185 的 pile 专用校验被显式返回为
deferred，未混入对象删除、掉落、声音或 framing 副作用。

本批次继续完成 `Check4x2` 的纯验证前缀：beds、picnic tables 和 bathtubs 的 4x2 原点、
style/frame 坐标和底部支撑均从 immutable snapshot 计算。`destroyObject`、`KillTile`、掉落
和递归 framing 仍保持排除，inventory 与 method map 标记为 `Partial`。

`HandleRopeEndFraming` 现在映射到已有的 `TileRopeEndFramingQuery.CreateRequests`，只负责从
immutable snapshot 找到 bounded rope endpoints 并生成 `RopeEnd` frame requests。由于 source 中
`HandleRopeEndFraming_Inner` 是空实现，递归处理、最终 rope frame 选择和所有宿主副作用继续明确
保持未映射。

`Check2x2` 现在增加通用 2x2 footprint、frame/style band 与支撑查询。`type 95/126` 的顶部
支撑可作为显式输入规则处理；boulder chest 保护、`type 132` 墙体调整、`type 652` 特殊布局、
销毁、掉落和递归 framing 均保持 deferred。

`Check3x2` 现在增加通用 3x2 footprint、frame/style band、平台特殊高度和底部支撑查询。
类型 186/187/488/704/705/26/695 的专用规则，以及销毁、掉落和递归 framing 均保持 deferred。

阶段 F 的 ore 入口也已收敛：`OrePlacementSystem` 不再直接生成可能覆盖现有 Tile 的 `Place`
命令，而是委托 `OrePlacementTransactionSystem` 完成 prepare、冲突检查、Ore stage 推进和
command append。准备失败时保持零变更。

`Check3x4` 增加通用 3x4 footprint、frame/style band 和底部支撑查询；对象专用掉落、销毁和
递归 framing 继续保持 deferred。

`Check5x4` 增加通用 5x4 footprint、frame/style band 和底部支撑查询；对象专用掉落、销毁和
递归 framing 继续保持 deferred。

`Check6x3` 增加通用 6x3 footprint、frame 坐标和底部支撑查询；对象专用掉落、销毁和递归
framing 继续保持 deferred。

`Check4x3Wall` 增加通用 4x3 wall-object footprint、frame/style、active/type 和 wall presence
查询；销毁和掉落继续保持 deferred。

`Check6x4Wall` 增加通用 6x4 wall-object footprint、frame/style、active/type 和 wall presence
查询；销毁和掉落继续保持 deferred。

`Check3x3` 增加通用 3x3 footprint、frame/style 和显式 top/bottom 支撑查询；生成阶段覆盖、
销毁、掉落和递归 framing 继续保持 deferred。

`Check3x5` 增加通用 3x5 footprint、frame/style 和底部支撑查询；类型专用掉落、销毁和递归
framing 继续保持 deferred。

`Check3x6` 增加通用 3x6 footprint、frame/style 和底部支撑查询；类型专用掉落、销毁和递归
framing 继续保持 deferred。

`Check3x3Wall` 增加通用 3x3 wall-object footprint、frame/style、active/type 和 wall presence
查询；销毁和掉落继续保持 deferred。

`Check4x4` 增加通用 4x4 footprint、frame/style 和底部支撑查询；类型专用掉落、销毁和递归
framing 继续保持 deferred。

`Check2x3Wall` 和 `Check3x2Wall` 增加通用 wall-object footprint、frame/style 和 wall presence
查询；销毁和掉落继续保持 deferred。

`Check2x5` 增加通用 2x5 footprint、frame/style 和底部支撑查询；类型专用掉落、销毁和递归
framing 继续保持 deferred。

`Check2xX` 增加 variable-height 2-column footprint、frame 坐标和 top/bottom 支撑查询；平台
桥接、锤击状态、类型专用掉落和销毁继续保持 deferred。

`Check1xX` 增加 type-dependent vertical footprint、frame-band 和 bottom-support 查询；
destroyObject、KillTile 与类型专用掉落继续保持 deferred。

`Check1x2` 增加两格竖直 footprint、frame-band、类型和 solid-or-platform 支撑查询；
类型 20 的随机 frame 修正、destroyObject、KillTile 与类型专用掉落继续保持 deferred。

`Check1x1` 增加单格对象的 bottom solid-support 查询；Abigail 花朵地面规则、boulder
分类与 KillTile 继续保持 deferred。

`CheckGolf1x1` 增加 frame 对齐和 bottom solid-support 查询；KillTile 继续保持 deferred。

`CheckLogicTiles` 增加 18 像素 frame 对齐和类型 419 对 419/420 支撑配对查询；
KillTile、wiring 与 logic activation side effects 继续保持 deferred。

`CheckAlch` 增加 style 0–6 的支撑类型集合、half-brick 拒绝和 lava-contact 查询；
style 5 液体驱动类型转换、NetMessage 同步与 KillTile 继续保持 deferred。

`CheckBanner` 增加三格竖直 footprint、frame-band 和悬挂支撑查询；destroyObject、
KillTile、banner 掉落映射与递归 framing 继续保持 deferred。

`CheckWeaponsRack` 增加 3x3 type-334 footprint、编码 frame 归一化和 wall backing 查询；
TileEntity inventory、掉落、destroyObject 与 KillTile 继续保持 deferred。

`CheckMan` / `CheckWoman` 增加 type-128/type-269 的 2x3 footprint、编码 frame 归一化和
bottom support 查询；destroyObject、KillTile 与 mannequin 掉落继续保持 deferred。

`Check1x2Top` 增加两格顶挂对象的 frame 和 platform、rope 或 solid 支撑查询；
TileEntity、destroyObject、KillTile 与类型专用掉落继续保持 deferred。

`CheckSign` 增加 2x2 footprint/frame、type-85 bottom support 和 oriented attachment 查询；
Sign.KillSign、TileEntity、destroyObject、KillTile 与递归 framing 继续保持 deferred。

`CheckBoulderChest` 增加 boulder 原点、上方坐标和 breakability/container protection 查询；
legacy break execution、宝箱状态变更与 network effects 继续保持 deferred。

`CheckChest` 增加 2x2 chest footprint、frame/type 和双列 bottom support 查询；
Chest.CanDestroyChest、Chest.DestroyChest、destroyObject、KillTile 与掉落映射继续保持 deferred。

`CheckTrapDoor` 增加 type-387 2x1、type-386 2x2 footprint/frame 和 solid-anchor 查询；
完整 CheckTileAnchors、destroyObject、KillTile、掉落与递归 framing 继续保持 deferred。

`CheckTileFrames` 增加 bounded footprint 的 active/type/frame 公式查询；
Framing.GetTileSafely 分配语义与销毁副作用继续保持 deferred。

`CheckTallGate` 基于 Simulation 既有的 type-388/type-389、1x5 tall-gate 契约增加 frame
和垂直 anchor 查询；动态 TileObjectData、destroyObject、KillTile、掉落与 framing 继续 deferred。

`CheckTileAnchors` / `AnchorValid` 增加 mode-based 边界 anchor 扫描和 snapshot attachment
谓词；Table、平台 frame 白名单、NotReallySolid/blockType 旧标志与分配语义继续 deferred。

`CheckStalactiteEcho` 增加 echo 高度、上下挂载方向、连续 frame 和 solid slope 支撑查询；
destroyObject、KillTile 与样式掉落继续保持 deferred。

`CheckStalactite` 增加 frame 分支方向、单/双格连续性和 slope 支撑查询；
UpdateStalagtiteStyle、InvalidTileForPilesOrSpeleothems、destroyObject 与 KillTile 继续 deferred。

`CheckXmasTree` 增加固定 4x8 type-171 footprint、legacy frame-pair 一致性和中部地面
支撑查询；destroyObject、KillTile 与圣诞树掉落继续保持 deferred。

`CheckCannon` 增加 4x3 footprint、style/frame band 和内部底部支撑查询；destroyObject、
KillTile 与 cannon 掉落映射继续保持 deferred。

`CheckMB` 增加 2x2 music-box footprint、style/frame band 和底部 solid 支撑查询；
table 分类、destroyObject、KillTile、掉落映射与递归 framing 继续保持 deferred。

`CheckFoodPlatter` 增加 bottom solid-support 查询；TileEntity 食物掉落、destroyObject 与
KillTile 继续保持 deferred。

`CheckBamboo` 增加 base/bamboo 支撑、上方 bamboo 关系和 frame-band 查询；随机 frame
归一化、NetMessage 与 KillTile 继续保持 deferred。

`BlockBelowMakesSandConvertIntoHardenedSand` / `BlockBelowMakesSandFall` 增加 below-tile
空隙、solid 与 type-165 例外查询；legacy tile registry flags 继续保持 deferred。

`CheckTileBreakability_HasReasonToReturnEarly` 映射到现有 breakability protection predicate；
完整 CheckTileBreakability 编排与 legacy registry ownership 继续保持 deferred。

`CheckTileBreakability2_ShouldTileSurvive` 增加 chest/dresser 原点和 container、display-doll、
hat-rack survival 查询；运行时实体破坏判断继续通过显式输入 deferred。

`CheckTorch` 增加 down/left/right/wall attachment 和 suggested frame 查询；tree/beam 特殊
集合、KillTile 与 frame mutation side effects 继续保持 deferred。

`CheckOasisPlant` 增加 3x2 plant frame 和显式 conversion-sand 支撑查询；销毁和递归 framing
继续保持 deferred。

`CheckJunglePlant` 增加 2x2/3x2 plant frame 和支撑查询，并保留 type-702 的 bottom-slope
规则；type-238 NPC spawn、掉落、销毁和递归 framing 继续保持 deferred。

`CheckUnderwaterPlant` 增加 bounded water/support/wall 检查和 frame-normalization 结果；
KillTile、网络、随机 frame 和 SquareTileFrame 继续保持 deferred。

`CheckCactus` 增加 cactus support、vertical height、side-branch attachment 和 supported sand
type 查询；KillTile、递归 framing、网络和掉落继续保持 deferred。

## 7. 阶段 F：世界生成核心行为族

Tile 和 framing 边界稳定后，按单一可回放 profile 推进世界生成核心逻辑。

建议顺序：

1. `TerrainBaseSystem`：令已有基础地形与 legacy profile 输入对齐。
2. `CaveCarvingSystem`：迁移一个 bounded cave profile，加入 protected footprint。
3. `BiomeSurfaceSystem`：先普通草地、沙地、丛林，不同时展开全部 secret seed。
4. `OrePlacementSystem`：一个矿物定义、固定深度带和明确冲突规则。
5. `StructurePlacementSystem`：一个小型 footprint 的 prepare / commit / rollback。
6. `TreePlacementSystem`：普通树根、树干、树冠，再特殊树。
7. `LiquidSourceSystem` / `LiquidPropagationSystem`：固定预算和可重放工作队列。
8. 最后处理完整 dungeon、房屋、宝箱、Hardmode、Meteor、Shimmer 与世界事件。

每个行为族必须遵循：

```text
legacy source mapping
  -> explicit immutable input
  -> pending verifier assertions
  -> pure query or system
  -> command batch
  -> commit boundary
  -> inventory mapping
  -> later runtime verification
```

WorldGeneration verifier supports an explicit `--reduced` smoke mode. The source currently
contains 81 independent `PASS` sections; the mode executes the first 32 (39.5%, rounded to
the requested 40% scope) and then exits with a summary. This mode is for fast feedback only
and does not replace the full verifier, which remains the default when `--reduced` is absent.
No test source is deleted or disabled; later migration slices remain covered by the full run.

## 8. 阶段 G：解除测试暂停后的验证顺序

只有在明确恢复测试后执行。

1. 运行 `Terraria.Dome.WorldGeneration.Verification`，优先暴露 Plant、shape、merge、moss 和
   framing 新切片的问题。
2. 运行 Simulation 项目 build，使用 `-p:UseSharedCompilation=false --no-restore`。
3. 运行 `WorldMechanics`、`WorldObjects`、`World.Protocol`、`World.Loopback` verifier。
4. 检查 forbidden references、source inventory 和 deletion gate。
5. 最后运行 legacy oracle differential / replay 对比。

绿色 verifier 只能证明已覆盖的 bounded mapping。即使未来这些测试通过，在大量入口和字段仍
未迁移且全世界差异存在时，也不能声称 `WorldGen` 已实现语义等价。

## 9. Legacy 删除门槛

旧 `WorldGen` 只能在以下条件同时满足后才允许考虑删除：

- 每个目标模式入口都有完整映射，或有批准的 OutOfScope 归属。
- legacy 所需字段、规则、随机流和运行时副作用均有替代边界。
- replay fingerprint 与目标 legacy oracle 对齐。
- section snapshot、Tile 扩展状态、液体、frame、对象状态与协议投影一致。
- 相关 build 和 verifier 均已通过。
- `worldgen-deletion-gate.json` 有新的实测证据，并将 `canRemoveLegacyWorldGen` 更新为 `true`。

当前不满足以上任一删除条件。阶段 B 的 bounded `TileFrame` 计算契约、snapshot pending
overlay、普通 solid frame evaluation 和 `TileFrameCommandSystem` 已落地，并由当前 verifier
源码中的 deterministic contract assertions 覆盖。它已解锁 merge frametest、rope end framing、
植物和 shape 更新的 command-only 前缀；完整 frame-important、液体、树、藤蔓、多格对象和
runtime parity 仍 deferred。阶段 C 当前已登记 `CheckVines`、`SquareTileFrame`、
`SquareWallFrame`、`RangeFrame`、`Cull` 以及若干显式输入的 spawn、category、housing 和
coating 纯查询；这些映射只覆盖分类、坐标 topology、request/command intent 或 immutable
result，不宣称宿主 framing、room scheduling 或对象副作用。下一项高价值工作是恢复允许的
focused verifier/build 验证，然后再决定是否扩大特殊连接规则边界。
