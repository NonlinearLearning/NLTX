# P20 世界生成动作、条件、形状与结构规划 System 设计

## Document Metadata

~~~yaml
documentType: system-design
designStatus: proposed
implementationStatus: partial
executionStatus: partial
verificationStatus: not-run
historicalVerificationStatus: partial
migrationStatus: not-claimed
partitionId: P20
inputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P20-world-generation-actions-shapes.md
sourceRoot: D:\TRbackup\Version4
referenceRoot: D:\TRbackup\无任何删减通过编译
targetRoot: D:\TRbackup\NLTX\src\NSSLC
documentRevisionReviewedAt: 2026-10-01
relatedPartitions: P17, P19, P16, P01
deliveryMode: implementation-partial
thisTurnSourceModified: true
thisTurnBuild: focused-project-passed
thisTurnTest: focused-verifier-passed
thisTurnVerification: measured-partial
~~~

本文件是 P20 System 设计，不是实现结果，也不是行为等价或迁移成功证明。设计只覆盖权威 P20 的 12 个叶子组、115 个字段、4 个属性和 119 个成员。所有跨分区 owner、调度和提交协议在未达成集成决定前保持 crossSubsystemOwner: integration-review。

本次复核补充了完整参考源码定位、只读 CPG 结果，以及 NLTX 中 S06/S07 的局部实现记录；权威 P20 output report 的 `verificationStatus: not-run` 保持不变。前序局部实现/验证记录以 `historicalVerificationStatus: partial` 保留，本轮 focused 结果也不能替代真实迁移项目的行为验收。

## 1. 设计目标与非目标

### 目标

1. 把 Version4 世界生成的 action、condition、search、shape、modifier、culling、structure reservation 语义拆成可组合的 Query、Definition、Command、System、Adapter 和 Projection。
2. 保留旧入口的可观察结果、写集、不变量、顺序、错误、随机消耗、生命周期和重试语义；如果证据不足，保留 unknown，不以新类型名填空。
3. 让 P19 的生成执行边界可以消费 P20 的有序 action payload，让 P17 的 terrain/framing owner 成为最终写者，让 P16/P01 的生命周期和液体边界继续各自负责。
4. 把 StructureMap 的 eligibility、protected overlap、padding、reservation、reset 和 snapshot 语义集中到一个候选 owner，避免 CanPlace 与 AddProtectedStructure 被两个 System 分别实现而产生双写或竞态。

### 非目标

- 本设计不要求修改 Version4、参考项目、测试或项目文件；本轮已落地的 S06/S07 局部实现仅属于 `implementation-partial`，不改变完整迁移的验收门槛。
- 不按 12 个叶子组机械创建 12 个调度 System；纯 Definition/Query 不新增调度节点。
- 不把 SS14 或完整参考项目的实现直接当作 Version4 当前 checkout 的行为证明。
- 不决定 P17/P19/P16/P01 的最终 owner，不决定网络协议或存档版本，不把静态设计升级为 verified 或 migration-success。

## 2. 范围与成员映射

P20 的完整 119 项清单和来源序号以输入 report 为准。设计层将其压缩为以下行为边界，清单只用于审计，不代表每个成员都是独立状态 owner。

| 设计组 | 输入叶子 | 成员数 | proposed 边界 |
|---|---|---:|---|
| S01 | WorldTileMergeCullState | 8 | WorldTileMergeCullStateQuery，读取 revisioned neighborhood，返回 8 个 cull flags |
| S02 | WorldGenerationTileSetActions | 12 | tile command payload，交给 terrain owner commit |
| S03 | WorldGenerationWallMutationActions | 7 | wall command payload，交给 terrain/wall owner commit |
| S04 | WorldGenerationTilePlacementAndPaintActions | 5 | placement/paint command payload，保留组合操作的顺序与拒绝结果 |
| S05 | WorldGenerationLiquidAndNeighborActions | 3 | liquid/neighbor command payload，交给 P01/P17 约定的 owner |
| S06 | WorldGenerationTileScanAndControlActions | 7 | `WorldGenerationTileScanAndControlSystem` 的同步显式 sink/bounds 边界；完整旧 action chain 仍 partial |
| S07 | WorldGenerationTileFramingAndDebugActions | 3 | `WorldGenerationTileFramingAndDebugSystem` 的 framing port/diagnostic sink 边界；真实 owner 仍 partial |
| S08 | WorldGenerationConditionsAndSearches | 11 | explicit snapshot Query，保留 NOT_FOUND 兼容映射 |
| S09 | WorldGenerationShapeData | 10 | immutable shape Definition/Traversal Query |
| S10 | WorldGenerationShapeModifierState | 22 | immutable modifier Definition/Query，随机输入显式传入 |
| S11 | WorldGenerationTileWallConditionState | 20 | predicate Definition/Query，只读 tile snapshot |
| S12 | WorldStructurePlanningAndMasks | 11 | WorldStructureReservationSystem 候选 owner + snapshot Query |

12 组总计 119 项；字段和属性的逐项 declaring type::member 证据仍以 P20 System report 的 Complete 119-member scope 为唯一范围来源。

## 3. 证据基线

### 3.1 Version4 目标源码

目标源码目录 D:\TRbackup\Version4 没有 Git metadata。先前 report 记录了 WorldGen.cs SHA256 A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D、Actions.cs SHA256 3125F2FBF0DFC87491140E8E60D7BC7121D5D95665EEB87C1BD16A9464FA1E44。P20 相关的当前源码仍包含多个 default-return/no-op body，因此不能把空实现解释为无副作用。

### 3.2 完整参考项目

用户指定的完整参考项目是 D:\TRbackup\无任何删减通过编译。本轮只读其源码，未运行构建。关键文件 SHA256 如下，用于记录本次参考快照：

| 文件 | SHA256 |
|---|---|
| Terraria\WorldGen.cs | B9F7834CE1BC68C1DD9C656574A2272DB6F79E1407D934E1ADA33EDC3C930F82 |
| Terraria.WorldBuilding\Actions.cs | DD13D4F8986EB9DB5EB16EF89285CCCE91C90BC188B7F3D115AF93A29BA72CD1 |
| Terraria.WorldBuilding\GenAction.cs | A7009792C62A8AAEA746403CC91419B6574932F0F6CE35BEAFFF13B6C9CADDC0 |
| Terraria.WorldBuilding\GenShape.cs | D63294D6CAC9B1FC686817AD38AD79D0BE3FEEEBF607DD8DC6A5050CC2901CA7 |
| Terraria.WorldBuilding\StructureMap.cs | E2079D6564278C9FC72F754751CD5770363DA4567073FC0441FEC7586FF1F359 |
| Terraria.WorldBuilding\WorldGenRange.cs | FCE5D3C52663486EBD5B80925CAE859B90A3A99E5BD16CCDD7A45905238A402C |
| Terraria.WorldBuilding\WorldGenerator.cs | 56CA4E8F06B13368625CFB3C9B643DB757995B2F4BA1AC8043F9D0712AAB146F |
| Terraria.WorldBuilding\WorldGenSnapshot.cs | 6EC1EEEF313676D5EFC84AA639B044DCACE190B63304455501344BBFD4D7AB1B |
| Terraria.WorldBuilding\ModShapes.cs | 7F8AE18FEF27A983C4D2ADFC7BBEE2D77E4657F38D163B24EE4B739A7F31D65F |
| Terraria.WorldBuilding\Modifiers.cs | 8B9A6CE981EE2E457C5DC01E84D7D445929265624E1ACD97256547A2FD362A8B |
| Terraria.WorldBuilding\Conditions.cs | 4E426108B292A2BAAD960AB5CFF3ADD063D4734FE6A9980083EEAAA7333856DE |
| Terraria.WorldBuilding\Searches.cs | 6CABDF497570693544A555E65F58E43E0EC1F149A608DF72D0C3F0F9E1A37707 |
| Terraria.WorldBuilding\WorldUtils.cs | D6B86A2DCA50F4FC88109D12F0E8E3C7C359375D3E97FB2DEC0401F5DE06220E |

该完整项目补齐了当前 Version4 checkout 中缺失的若干方法体，可作为预期语义参考。它没有为当前 Version4 CPG 建立 snapshot identity，因此与目标的对应关系仍是 corroborated/partial，不是 confirmed。

本次源码复核的定位锚点（行号以该参考快照当前文件为准）如下：

| 参考行为 | 参考位置 | 直接观察到的约束 |
|---|---|---|
| action 链与相对输出 | `Terraria.WorldBuilding\GenAction.cs:15-27`、`:30-48` | `UnitApply` 先写 `OutputData`，再调用 `NextAction.Apply`；无 next 时返回 `true`；`IgnoreFailures` 改变 `Fail` 的返回策略 |
| shape traversal 与失败策略 | `Terraria.WorldBuilding\GenShape.cs:13-37` | `UnitApply` 先写 shape 输出，再调用 action；`Output` 与 `QuitOnFail` 是 operation-local 状态 |
| Gen/Find facade | `Terraria.WorldBuilding\WorldUtils.cs:39-63` | `Gen` 委托 `shape.Perform`；`Find` 将 `GenSearch.NOT_FOUND` 转换为 `false` |
| wall/paint action 分流 | `Terraria.WorldBuilding\Actions.cs:268-302,420-542,565-574` | `RemoveWall` 直接写 wall 0 后继续 `UnitApply`，与带 framing 语义的 `ClearWall` 独立；三个 `Set*Paint` 对 paint 0 调用 `Fail()`；三个独立 clear paint action 写入颜色 0 后继续 `UnitApply`；Rainbow action 依赖位置计算 |
| 结构资格与保留 | `Terraria.WorldBuilding\StructureMap.cs:19-109` | 锁内检查 bounds、padding、protected overlap 和 active tile；`AddProtectedStructure` 同时写普通/受保护列表；`Reset` 只清 protected 列表 |
| 范围缩放与随机上界 | `Terraria.WorldBuilding\WorldGenRange.cs:38-65` | `GetRandom` 调用 `Next(ScaledMinimum, ScaledMaximum + 1)`；面积、宽度和 None 使用不同缩放因子 |
| pass 执行边界 | `Terraria.WorldBuilding\WorldGenerator.cs:453-497` | `_controlLock` 下选择 pass、`RunPass`、追加结果并调用 `OnPassCompleted`；P20 不拥有 pass loop |

这些锚点只提升完整参考项目自身的可追溯性，不提升目标 Version4 的证据等级；目标缺失 body、版本差异、动态调用和运行时调度仍按 `unknown`/`partial` 处理。

### 3.3 只读 CPG 查询

查询通过 .agents\skills\ecs-system\tools\CpgEvidence.ps1，数据库为 D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite。manifest SHA256 为 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364，project fingerprint 为 521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B。本轮查询观察到：

- Find-CpgSymbols(WorldUtils.Gen) 返回 1 个 method symbol；在 `Terraria/WorldGen.cs` 限定范围的只读 call-site 查询返回 3 个 `internal-static-exact` confirmed call-site，但跨文件调用闭包仍未证明。
- WorldUtils.Find 返回两个 overload/符号，其中一个全范围 Find-CpgCallSites 返回 1 个 call-site；虚 dispatch 仍是 partial。
- StructureMap.CanPlace 返回两个 overload，其中一个全范围返回 1 个 call-site；AddProtectedStructure 的调用闭包仍未完全解析。
- WorldGenerator.GenerateWorld 的 inbound call-site 查询带 gap；RunPass 返回 1 个可解析 call-site。
- WorldGenRange.GetRandom 与 ScaleValue 的符号查询各返回 1 个 complete method symbol；按声明文件查询 call-site 时均为 partial、`NoMatchingFactInScannedScope`，因此不能解释为无 caller，也不能作为完整反向闭包。
- 本次复核使用 `SourcePath` 精确限定声明 shard：`WorldUtils.Gen`、`StructureMap.CanPlace`、`StructureMap.AddProtectedStructure`、`WorldGenerator.GenerateWorld`、`WorldGenerator.RunPass`、`WorldGenRange.GetRandom` 和 `WorldGenRange.ScaleValue` 均返回 `complete` symbol；`WorldUtils.Find` 在 `Terraria.WorldBuilding/WorldUtils.cs` 返回 2 个 `complete` symbols；`GenShape.UnitApply` 在其声明 shard 返回 `partial` 与 `NoMatchingFactInScannedScope`。这些结果只证明索引查询状态，不证明调用闭包或行为等价。
- 本次 S06/S07 入口复核中，`SetFrames` 和 `DebugDraw` 的声明均可定位，但在 `Terraria.WorldBuilding/Actions.cs` 的调用扫描均返回 `partial`、0 item、`NoMatchingFactInScannedScope`。零命中不解释为无调用；动态 dispatch、外部注册、渲染线程和 owner 生命周期仍为 `unknown`/`integration-review`。
- 本次 paint 入口复核中，`ClearTilePaint`、`ClearWallPaint`、`ClearTileAndWallPaint` 和 `SetTileAndWallRainbowPaint` 的 type 与 `Apply` surface 查询均为 `complete`；四个 `Apply` callable-facts 查询均为 `partial/CalleeEffectsNotExpanded`。这只确认目标声明和索引 surface 可定位，不确认 body、调用闭包或 owner wiring。
- 本次 wall 入口复核中，`RemoveWall` 的 type 与 `Apply` surface 查询为 `complete`，callable facts 为 `partial/CalleeEffectsNotExpanded`；参考直接写 wall 0，目标 owner/framing 关系仍未闭合。

CPG complete 只表示查询在索引预算内完成；virtual/interface/delegate/reflection、callee effects、alias、配置和运行时 scheduler 仍是 partial 或 unknown。CPG manifest 没有 SourceSnapshotId 或逐文件 source hash。

## 4. 从完整参考源码恢复的概念行为

以下内容是参考项目直接展示的行为约束，目标 Version4 当前 checkout 若缺少对应 body，实施时必须先做 source-diff 和 focused verifier。

### 4.1 Action chain 与 shape traversal

- GenAction.UnitApply 先把 (x - origin.X, y - origin.Y) 写入 OutputData（若非空），再调用 NextAction.Apply；没有 next action 时返回 true。
- GenAction.IgnoreFailures 将失败策略改为继续；Fail() 根据 _returnFalseOnFailure 返回失败或继续信号；Output(ShapeData) 保存输出对象并返回自身。
- GenShape.UnitApply 先把相对点写入 shape output，再调用 action.Apply；Output 与 QuitOnFail 是 operation-local configuration。
- WorldUtils.Gen 同时支持 (origin, shape, action) 和 GenShapeActionPair 两种入口，最终都调用 shape.Perform(origin, action)。
- 完整参考 `Terraria.WorldBuilding\Actions.cs` 的 S06 入口直接显示：`ContinueWrapper.Apply` 先调用被包裹 action 再执行 `UnitApply` 并忽略前者返回值（约 `21-27`）；`Count`/`Scanner` 先递增 sink 再执行 `UnitApply`（约 `39-63`）；`TileScanner` 仅在 active 且 type 命中时递增，再执行 `UnitApply`（约 `82-92`）；`Custom` 用非短路 OR 执行 callback 与 `UnitApply`（约 `147-153`）；`UpdateBounds` 更新 bounds 后执行 `UnitApply`（约 `352-359`）。
- 完整参考 `Terraria.WorldBuilding\Actions.cs` 的 S07 入口显示：`SetFrames` 调用 `WorldUtils.TileFrame` 后执行 `UnitApply`（约 `650-666`）；`DebugDraw` 使用 `SpriteBatch.Draw` 后执行 `UnitApply`（约 `361-380`）。这些是参考语义锚点，不是目标 Version4 的 confirmed runtime 闭包。
- 完整参考 `Terraria.WorldBuilding\Actions.cs:420-542` 显示：三个 `Set*Paint` 的 paint 0 分支调用 `Fail()`；`ClearTilePaint`、`ClearWallPaint`、`ClearTileAndWallPaint` 分别写 tile color 0、wall color 0、两个颜色 0，再调用 `UnitApply`；`SetTileAndWallRainbowPaint` 先按坐标取得 Rainbow paint ID。P20 只能把这些写集表达为 intent，不能在缺少 owner 和目标 body 证据时直接写 authority。
- 完整参考 `Terraria.WorldBuilding\Actions.cs:565-574` 的 `RemoveWall` 直接把 wall 写为 0 后执行 `UnitApply`，不携带 `ClearWall` 的 neighbor-framing 参数；因此 P20 保留两个独立 operation，避免把 framing 副作用错误附加到 direct remove。
- 参考 Actions 将清 tile、清 wall、set tile/wall、placement、paint、liquid、smoothing、scan、bounds 和 debug 组合在链式 action 中；具体 action 直接触碰 Main.tile、frame/wall frame、液体或图形对象。P20 新 API 必须把这些副作用变成显式 command/adapter 边界。

### 4.2 Conditions、searches、modifiers

- GenSearch 保存 _conditions 和 _requireAll，Check 按 all/any 规则调用条件；WorldUtils.Find 将 GenSearch.NOT_FOUND 映射为 false。
- P20 `WorldGenerationConditionsAndSearchesQuery` 通过 `SearchDefinition.RequireAll(bool)` 显式保留 all/any 模式；默认仍为 all，any 模式为空条件时返回 false，与参考 `Check` 的 `_requireAll` 终值一致。
- 参考 shape/modifier 类型以 ShapeData 点集合为输入，进行 outline、offset、expand、mask、dither、blotches、flip 等变换；这些变换可以设计成纯 Definition/Query，但随机输入、点顺序和失败策略必须显式化。
- 目标 Version4 的 Perform、Find、CheckValidity 或 modifier body 若仍为空，参考项目只能提供候选语义，不能替换目标版本的缺失事实。

### 4.3 StructureMap

完整参考项目的 StructureMap 行为包括：

1. _structures 和 _protectedStructures 是 JsonProperty 的列表，_lock 只保护 map 内部。
2. CanPlace 在锁内检查 world bounds；将 padding inflate 后检查 protected intersections；遍历 active tile 并按 validTiles[type] 判定。
3. AddStructure 只写 _structures；AddProtectedStructure 同时写 _structures 和 _protectedStructures；两者都在锁内 inflate。
4. Reset 在锁内清空 _protectedStructures，而不是同时清空普通 structures。
5. 这仍然没有证明调用方把 CanPlace 与后续 Add 操作作为一个原子事务。提议的 owner 必须把检查 + reserve + result 放进同一 commit 边界，或者明确接受 legacy race。

### 4.4 WorldGenRange 与 P19 交接

完整参考项目的 WorldGenRange：GetRandom 调用 random.Next(ScaledMinimum, ScaledMaximum + 1)；ScaleWith.WorldArea 使用 (Main.maxTilesX * Main.maxTilesY) / 5040000.0，WorldWidth 使用 Main.maxTilesX / 4200.0，None 使用 1.0，结果转换为 int。范围输入、随机流和 world dimensions 必须由 P19/P17 的明确 adapter 提供，P20 不得读取 ambient globals。

### 4.5 WorldGenerator 与 snapshot 只作为协作约束

完整参考项目的 WorldGenerator.GenerateWorld 在 _controlLock 下读取 PassResults.Count、选择 _currentPass、追加 RunPass 结果、调用 OnPassCompleted，再清空 _currentPass；暂停、abort、progress、snapshot 和 debug UI 都在同一执行闭环中。P19 report 已将这些职责提议给 lifecycle/pass/control Systems，P20 只提供 action/shape payload，不重新实现 pass loop。

WorldGenSnapshot 通过反射序列化 GenVars 的 public static fields/properties，保存 manifest、GenVars JSON 和 tile snapshot；恢复时会 reset WorldGen、恢复 manifest/GenVars/tile、停用 NPC。P20 的 structure state 是否进入持久化、由谁恢复、是否允许多 world，仍交给 P16/P17/P19 integration review。

## 5. 所有权与边界设计

### 5.1 唯一写者规则

| 状态/效果 | 提议 owner | P20 责任 | 当前证据 |
|---|---|---|---|
| tile/wall/paint/slope/half-block | P17 terrain owner | 产生有序 command，不直接写 component | reference action 有直接 tile/frame 写入；NLTX P20 command 无 consumer 证据 |
| liquid/neighbor state | P01/P17 integration owner | 产生 liquid intent，保留 neighbor order | P01 与 P17 边界未最终确认 |
| framing/cull revision | P17 framing owner + S01 Query | S01 只读计算 flags，S07 只产生 framing intent | NLTX cull Query 已有 explicit revision 输入，失效闭环未知 |
| pass cursor/result/progress | P19 pass execution owner | P20 不拥有 scheduler 或 pass results | P19 report 已提出 one ordered commit point |
| structure reservations | proposed P20 WorldStructureReservationSystem | atomic eligibility/reserve、snapshot、reset request | Version4 map logic可读；跨调用原子性未证明 |
| persistence/recovery | P16/P19 snapshot adapter + live-world owner | 提供可序列化 reservation projection | WorldGenSnapshot reference 已展示机制，目标绑定未知 |
| debug drawing/callbacks | adapter/presentation owner | S07 只发 intent，不持有 SpriteBatch | reference action 直接使用 graphics；线程/生命周期未知 |

任何共享 invariant 若需要两个 System 同时写，必须退回 owner 评审。Component 存在、文件归属或字段数量都不是 owner 证据。

### 5.2 Proposed API composition

~~~text
P19 pass input + immutable world/tile snapshot + explicit random input
  -> WorldGenerationConditionsAndSearchesQuery
  -> WorldGenerationShapeDataDefinitionQuery
  -> WorldGenerationShapeModifierStateDefinitionQuery
  -> ShapeTraversalQuery (ordered points, output data, quit/continue result)
  -> WorldGenerationActionExecutionSystem
  -> ordered tile/wall/paint/liquid/framing/debug commands
  -> P17/P01 owners and adapters

candidate rectangle + structure snapshot + tile validity snapshot
  -> StructurePlacementQuery
  -> WorldStructureReservationSystem (check + reserve + result)
  -> immutable StructureReservationSnapshot
  -> later generation queries / P19 pass continuation
~~~

### 5.3 API surface

**Queries/Definitions**

- ShapeTraversalQuery：输入 shape points、origin、action descriptors、failure policy、existing output policy；输出 ordered point result、output shape、first failure/continued status 和 consumed random facts。
- WorldGenerationConditionsAndSearchesQuery：输入 ITileSnapshotReader、bounds、conditions、search direction/range；输出 SearchResult，兼容 adapter 可映射为 NOT_FOUND/false。
- WorldGenerationShapeDataDefinitionQuery 与 WorldGenerationShapeModifierStateDefinitionQuery：继续使用不可变 point collections；禁止访问 global tile、clock、ambient random 或写回。
- WorldGenerationTileScanQuery：输入有序且不可变的 tile observations 与 requested tile IDs，输出 active tile counts、total matches 和兼容的 unknown-tile `-1` sentinel；不调用 count sink、custom callback 或 bounds reference。
- WorldGenerationTileScanAndControlSystem：同步执行 `Continue`、`Count`/`Scanner`、`TileScanner`、`Custom` 和 `UpdateBounds`；引用只在调用期间有效，异常由显式 policy 映射，系统不排队、不保留 callback，也不读取 ambient world。
- WorldGenerationTileFramingAndDebugSystem：同步执行 `SetFrames` 到显式 `IFramingPort`，或 `DebugDraw` 到 command 自带的 diagnostic sink；系统不持有 `SpriteBatch`，真实 framing/render owner 仍由集成端提供。
- WorldGenRangeQuery：输入 minimum、maximum、ScalingMode、world dimensions、explicit random stream；输出 scaled bounds 与 chosen value。
- StructurePlacementQuery：输入 candidate rectangle、padding、protected snapshot、world bounds、tile validity snapshot；输出 rejection reason 或可提交 reservation。
- WorldTileMergeCullStateQuery：保留 revision and missing-neighbor policy；只返回 8 个 flags，不更新 framing。

**Commands**

- 复用现有 WorldGenerationTileSetActionsCommand、WorldGenerationWallMutationActionsCommand、WorldGenerationTilePlacementAndPaintActionsCommand、WorldGenerationLiquidAndNeighborActionsCommand、WorldGenerationTileScanAndControlActionsCommand、WorldGenerationTileFramingAndDebugActionsCommand。
- 每个 command 必须带 generation scope、target/order identity 和 well-formed validation；callback、count sink、bounds reference 等引用型 payload 需要明确 lifetime、异常与取消语义。
- 新增 structure reservation command 只在 integration review 批准 owner 后实现；CanPlace 与 reserve 不得以两个可独立重入的方法暴露给并发 callers。

**Systems/Adapters/Projections**

- WorldGenerationActionExecutionSystem：按 P19 交付的 sequence 执行 Query 结果和 action payload，收集 first failure、continued/quit、取消状态和未提交 action，不直接拥有 terrain state；未提交 action 集合缺省为空，不得因兼容调用省略该参数而抛出运行时空值异常；有 generation scope 的结果必须拒绝属于其他 generation 的未提交 action；显式 `CancellationToken` 只在 P20 commit barrier 停止后续提交。
- WorldGenerationActionExecutionBatch：在 commit barrier 前保存已验证且复制的 generation-scoped action 顺序；`Prepare` 不触碰 owner port，`Commit` 才提交到显式 port。
- WorldGenerationActionCommitRouter：按 payload kind 将一个 action 路由到一个显式 owner port；六类 port 必须全部提供，router 不实现 P17/P01 的写入。
- WorldStructureReservationSystem：唯一提议的 P20 durable owner，负责 generation-scoped map、atomic check/reserve、protected membership reset、显式 generation discard 和 snapshot projection；reset 释放 protected membership 但保留 ordinary structure history，discard 才删除指定 generation 的全部记录，锁是 adapter 细节，不泄露到 domain API。
- LegacyWorldGenActionAdapter：在兼容阶段把旧 WorldUtils.Gen/Find/action chain 转为新组合；不复制不变量，不成为第二写者。当前只落地了 `WorldGenerationSearchCompatibilityAdapter` 的显式 snapshot `bool + out` 映射，未声称已接入旧静态入口。
- WorldStructureReservationSnapshotProjection、WorldGenerationActionResultProjection：复制为只读输出，不能回写 owner；execution result 与 projection 都必须防御性复制输入集合，action result projection 必须保留 execution 的 generation scope、取消状态和未提交后缀，旧兼容构造没有 scope 时保留 `unknown`。

## 6. 决策与被拒绝方案

- S01/S08-S11 keep as Query/Definition：输入和结果可以显式化，未发现独立调度生命周期；单独 System 只会转发。
- S02-S05 keep as command payloads：这些组描述 operation 参数与 effect intent，最终写者在相邻 P17/P01 owner；不能把 payload class 当 System。
- S06 partial：本轮已闭合同步 lifetime、显式 sink、bounds writable boundary、异常策略和 P20 action barrier 的取消后缀；旧 `GenAction` 的 NextAction 链、P19 scheduler cancellation、外部 callback 注册和 runtime caller 仍未闭合。
- S07 partial：本轮已拆出 framing port 与 diagnostic sink 执行边界；P17 framing owner、渲染线程约束、debug 生命周期和外部注册仍未闭合。
- S12 separate candidate：structure lists 有独立 invariant、锁和 reset，但其 world lifetime/persistence 仍未闭合，因此只提议一个 owner，不宣布最终 owner。
- 拒绝一个 P20 mega-System：它会同时拥有 terrain、liquid、framing、graphics、structure 和 pass state，违反单一写者和副作用隔离。
- 拒绝一个 leaf 一个 System：文件分组不能证明独立生命周期、phase 或 transaction；会增加调度耦合。
- 拒绝把 InMemoryStructureReservationAdapter 直接升级为 canonical owner：当前调用只展示 underground-desert larva projection，不能覆盖全局 StructureMap、Reset、snapshot 和 multi-world 生命周期。

## 7. 生命周期与集成契约

### Create/activate

P19 创建 generation scope 和 explicit snapshot/random input；S12 owner 在 scope 内创建空 reservation state。GenVars.structures = new StructureMap() 是 Version4 setup 事实，但 P20 新 owner 的构造、注册和多 world identity 仍需明确。

### Execute/commit

P19 提交有序 action sequence；S01/S08-S11 只读计算；S02-S07 产出 commands；P17/P01 owner commit terrain/liquid/framing。S12 将 eligibility 与 reserve 放在同一 owner operation，返回 accepted/rejected reason 和 immutable snapshot。

### Pause/retry/failure

P19 负责 pass pause/abort/retry barrier。P20 action result 必须携带 first failure、continued/quit 及 callback exception policy；S12 reservation reject 必须无部分写入。随机值只能来自 explicit stream，重试是否重放由 P19 决定。

### Reset/end/destroy

P16/P19 发起 generation reset/end；S12 清理 generation-scoped plans，不能清理其他 world；S01-S11 无持久 owner。取消/异常路径必须保证 pending command 不再提交，且 reservation snapshot 与 live owner 一致。

### Persistence/network

S12 是否进入 WorldGenSnapshot、保存格式、恢复版本、网络 projection 尚未决定；P20 只提供 serializable value projection。任何 snapshot restore 必须由 live-world owner 校验后 commit，不由 Projection 直接写入 terrain。

## 8. 验收观察向量

在实现任务获准后，旧 facade 与新组合必须在同一 world dimensions、generation ID、seed/random stream、tile snapshot 和 action order 下比较：

- point order、relative output data、OutputData/shape output、QuitOnFail、IgnoreFailures、NextAction 链和 action return；
- tile/wall/paint/liquid/slope/half-block/frame/neighbor-frame 的完整 delta 和顺序；
- condition/search all/any、bounds/fluff、NOT_FOUND、boundary reason；
- modifier point set、duplicate removal、random draw count、range scaling、retry replay；
- structure bounds、padding、protected overlap、valid tile、AddStructure/AddProtectedStructure、Reset、duplicate/reservation atomicity；
- callback invocation/count sink/bounds update、exception/cancellation、debug effect；
- pass barrier、command visibility、snapshot/restore/reset、multi-world isolation 和 network projection。

只有真实迁移项目的必需行为测试命中新 owner 和新组合并通过，才可把状态提升到 migration-success。报告、设计文档、编译或局部 verifier 都不满足该门槛。

## 9. Blocking Decisions and Evidence Gaps

1. CPG 与目标源码的 snapshot binding 缺失；完整参考项目虽补齐语义，但不是同一 snapshot。
2. Version4 当前 checkout 的 action/shape/search/condition/modifier/range body 不完整，必须在实现前按参考项目做逐文件 diff，并确认哪些差异是版本差异、哪些是清理产物。
3. GenAction.Apply 的 inbound closure、virtual Perform/Find 的动态目标、action registration、delegate/reflection/mod hooks 未闭合。
4. NLTX P20 command declarations 仍未发现既有 runtime consumer；本轮新增的 action execution owner 只接收显式 commit port，尚未接入 P19/P17/P01。
5. P17 terrain/framing/liquid owner、P19 sequence/random/barrier、P16 lifecycle/persistence、P01 liquid runtime owner 未签署 integration contract。
6. 本轮新增的 WorldStructureReservationSystem 已在 generation scope 内提供原子 check + reserve、protected overlap、padding、tile validity、snapshot、“只释放 protected membership、保留 ordinary history”的 reset 和显式 generation discard；与真实 StructureMap、P16/P19 snapshot 的完整对应关系仍未决定。
7. S06/S07 的本地同步执行、异常映射和引用 lifetime 已由显式系统契约覆盖；目标旧 action 的完整 callback/NextAction、线程、取消、渲染线程和 runtime caller 仍为 `unknown`/`integration-review`。
8. `WorldGenRangeQuery` 已将 `None`、`WorldArea`、`WorldWidth`、显式 `IGenerationRandomSource` 和 inclusive maximum 组合成只读 API；它没有证明目标 Version4 的所有 range 配置或随机重放契约。
9. `ShapeTraversalQuery` 已将不可变 shape 点、相对输出、`NextAction` 链、`QuitOnFail`、action output 和 callback exception policy 组合成只读 API；描述符回调必须是无副作用计算，真实 effect command 仍由 P20 execution/owner 负责。
10. `WorldGenerationActionCommitRouter` 已提供六路显式 owner port 的组合边界和 generation/sequence 透传；`WorldGenerationActionExecutionResult`/projection 现在保留 generation scope。它只完成单路由与结果边界，不证明任何 P17/P01 owner 已接入运行时。
11. 目标 Version4 的 `GenSearch.Check`/`RequireAll` CPG 查询仍为 `partial/NoMatchingFactInScannedScope`，因此 any 模式实现依据完整参考行为并保持 proposed；focused verifier 只证明显式 snapshot 上的局部结果。
12. `WorldGenerationShapeModifierStateDefinitionQuery` 已覆盖 scale、expand、offset、flip、rectangle mask、dither、radial dither 和 blotches 的显式输入边界；focused verifier 只证明给定输入下的纯点集结果。完整参考项目在每次 `Modifiers.*.Apply` 调用中消费 ambient random，当前 Query 没有证明每点 draw 次数、随机流顺序或 retry replay，因此这些事实继续标为 `unknown`。
13. `WorldGenerationTileScanQuery` 已把完整参考 `Actions.TileScanner.Apply/GetCount` 可确认的 active/type 计数逻辑收敛为显式 snapshot Query；目标 CPG 对 `TileScanner` 仍为 `partial/NoMatchingFactInScannedScope`，所以该 Query 不代表旧 action chain、sink lifetime、callback exception 或 runtime registration 已闭合。

在上述决定前，新增实现只能停留在显式输入、单一 owner 和 focused verification 的 partial 阶段，不能宣称已接入完整生产运行路径。

## 10. 当前状态

designStatus: proposed；implementationStatus: partial；executionStatus: partial；verificationStatus: not-run。本文件保留前序 P20 action execution、structure reservation、range/shape/modifier/tile-scan 记录，并补充 S06/S07 的显式执行系统与 P20 action barrier cancellation；本次仍未完成跨分区接入、完整行为等价或迁移成功。

## 11. 本轮实现补充（2026-10-01）

本轮进入局部实现阶段，补充 S06/S07 的显式同步执行系统、action commit 取消屏障、paint validation 与 focused 验证记录。以下字段描述本次交付动作，不覆盖文档中保留的前序局部实现记录：

~~~yaml
deliveryMode: implementation-partial
thisTurnSourceModified: true
thisTurnBuild: focused-project-passed
thisTurnTest: focused-verifier-passed
thisTurnVerification: measured-partial
migrationStatus: not-claimed
~~~

本次只读证据复核使用 `D:\TRbackup\NLTX\.agents\skills\ecs-system\tools\CpgEvidence.ps1`，数据库为 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`。结果如下：

| 入口 | 查询范围 | 结果 | 解释 |
|---|---|---|---|
| `WorldUtils.Gen` | `Terraria/WorldGen.cs` 调用点 | `complete`, 3 items | 仅是索引范围内的静态调用点，virtual `Perform`、外部注册和运行时调度仍是 `partial`/`unknown`。 |
| `StructureMap.CanPlace` 默认重载 | `Terraria/WorldGen.cs` 调用点 | `complete`, 1 item | 只闭合该重载的一个静态调用点。 |
| `StructureMap.CanPlace` `validTiles` 重载 | `Terraria/WorldGen.cs` 调用点 | `partial`, 0 item, `NoMatchingFactInScannedScope` | 零命中不是无调用证明。 |
| `StructureMap.AddProtectedStructure` | `Terraria/WorldGen.cs` 调用点 | `complete`, 2 items | 仍未闭合动态/外部 caller 和 CanPlace 后的原子关系。 |
| `WorldGenRange.GetRandom` | 声明文件调用点 | `partial`, 0 item, `NoMatchingFactInScannedScope` | random caller、draw 顺序与重放保持 `unknown`。 |
| `TileScanner` | `Terraria.WorldBuilding/Actions.cs` 声明文件调用点 | `partial`, 0 item, `NoMatchingFactInScannedScope` | 只读 Query 复用了参考源码可确认的计数规则，sink、callback 和生命周期保持 `unknown`。 |
| `SetFrames` | `Terraria.WorldBuilding/Actions.cs` 声明文件调用点 | `partial`, 0 item, `NoMatchingFactInScannedScope` | 只确认参考 framing 调用语义；目标 framing owner、线程约束和 runtime registration 保持 `unknown`。 |
| `DebugDraw` | `Terraria.WorldBuilding/Actions.cs` 声明文件调用点 | `partial`, 0 item, `NoMatchingFactInScannedScope` | 只确认参考 debug draw 直接效果；目标 diagnostic owner、生命周期和渲染线程约束保持 `unknown`。 |

本轮实现补充：

- `WorldGenerationActionExecutionSystem` 在 owner commit 前检查显式 `CancellationToken`，并保留 cancellation 时 generation-scoped、有序的未提交 action 后缀；owner 抛出 `OperationCanceledException` 时映射为 `CommitPortCancelled`，其他异常继续传播。
- `WorldGenerationActionExecutionResult` 与 `WorldGenerationActionResultProjection` 携带 `Cancelled`、`CancellationReason`，并保持 `Succeeded`、`CompletedAllActions` 与未提交后缀的一致性。
- `WorldGenerationTilePlacementAndPaintActionsCommand` 对 `SetTilePaint(0)`、`SetWallPaint(0)` 和 `SetTileAndWallPaint(0)` 均按完整参考 `Fail()` 语义拒绝；参考中的 `ClearTilePaint`、`ClearWallPaint` 和 `ClearTileAndWallPaint` 已作为独立 operation 纳入该 payload，但仍等待 terrain/placement owner 的 commit wiring。`SetTileAndWallRainbowPaint` 保持 `unknown`，没有伪造 `WorldGen.GetRainbowPaintIDForPosition`。
- `WorldGenerationWallMutationActionsCommand` 已增加独立 `RemoveWall` operation；它只表达 wall-zero intent，不携带 `ClearWall` 的 framing 选项，仍等待 terrain/wall owner 的 commit wiring。

完整参考源码 `D:\TRbackup\无任何删减通过编译` 仍只作为 corroborated/partial 语义来源：`GenAction.UnitApply` 的输出后 `NextAction` 顺序、`GenShape.UnitApply` 的 shape 输出、`WorldUtils.Gen/Find` 的 facade 映射、`StructureMap` 的锁内资格检查与双列表写入、以及 `WorldGenRange` 的缩放和 inclusive 上界均已在本设计的证据基线中定位。它们不能升级目标 `D:\TRbackup\Version4` 的缺失 body、CPG snapshot identity 或运行时行为证据。

本轮新增代码位于：

- `src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldGenerationTileScanAndControlSystem.cs`
- `src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldGenerationTileFramingAndDebugSystem.cs`
- `src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldGenerationActionExecutionSystem.cs`
- `src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldGenerationActionExecutionResult.cs`
- `src/NSSLC/Component/WorldSession/WorldGeneration/Adapters/WorldGenerationActionResultProjection.cs`
- `src/NSSLC/Component/WorldSession/WorldGeneration/Actions/WorldGenerationTilePlacementAndPaintActionsCommand.cs`
- `WorldGenerationTileScanAndControlActionsCommand.TileCountAccumulator.TryRecord` 的显式匹配结果。

本轮 focused verifier 源码包含：count/scanner sink 的单点递增、Continue callback 与 UnitApply 结果分离、active tile scan、writable bounds 更新、callback 异常策略、framing port、diagnostic sink、缺失 framing owner 拒绝、action cancellation barrier、owner cancellation exception、paint validation 和三个独立 clear operation 断言；本轮已重新运行并输出 `PASS: P20 core action and structure reservation checks`。上述证据只覆盖显式输入、同步 action barrier 和命令校验；不覆盖旧 `GenAction` 的完整 action chain、P19 调度取消、P17/P01 owner、渲染线程、snapshot 或 runtime registration。

本补充不改变权威 P20 report 的 `designStatus: proposed` 与 `verificationStatus: not-run`，也不把已有局部编译、focused verifier 或文档完成称为迁移成功。

2026-10-01 参考语义复核修正：完整参考 `Terraria.WorldBuilding/Actions.cs` 的
`SetTileAndWallPaint.Apply` 同样在 `paintID == 0` 时调用 `Fail()`；当前命令契约已拒绝三个
`Set*Paint(0)` 变体，并新增独立的 `ClearTilePaint`、`ClearWallPaint`、
`ClearTileAndWallPaint` operation。完整参考对应的清除动作分别写入 tile color 0、wall color 0，
以及两个颜色 0 后再调用 `UnitApply`；这些写入仍必须由 terrain/placement owner 执行，不能由 P20
command 自己写 authority。目标 CPG 对三个 clear 类型及 `Apply` surface 均可定位为
`complete`，但 callable facts 仍为 `partial`（`CalleeEffectsNotExpanded`）；Rainbow action 的
surface 同样为 `complete`、callable facts 为 `partial`，因此不实现其 random/position policy。
该记录保持 `designStatus: proposed`、`verificationStatus: not-run` 和
`migrationStatus: not-claimed`。
