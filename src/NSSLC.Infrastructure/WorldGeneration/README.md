# 世界生成

本目录是独立的 .NET 10 世界生成库，由 `NSSLC.WorldGeneration.csproj` 编译。
原 `Mocks/` 目录已更名为 `Runtime/`。生成所需的地块、随机数、规则表、多格物体、
液体、宝箱和轨道依赖已有实际实现，已实测生成完整标准世界。

算法主体从 `D:/TRbackup/无任何删减通过编译` 的完整参考源码恢复，补齐了此前
Version4 源码中被删空的方法，以及缺失的沙漠、洞穴房屋和地牢生成文件。
正式代码使用 `NSSLC.WorldGeneration.*` 命名空间；不引用、继承、加载或反射
Terraria 程序集，也没有 `CallTracker` 依赖。

## 目录

| 位置 | 职责 |
| --- | --- |
| `WorldCreation.cs` / `WorldCreationRequest.cs` | 接收参数、串行调用完整流程、校验出生区 |
| `GeneratedWorld.cs` / `GeneratedTile.cs` | 返回独立持有的地块缓冲、地块值拷贝和只读宝箱/NPC/步骤数据 |
| `WorldGen.cs` | 恢复的生成算法与步骤编排 |
| `WorldBuilding/` | 生成器、步骤、形状、搜索、生成变量和结构占用 |
| `Generation/` | 算法辅助类型和矿车轨道生成 |
| `Biomes/` | 地形、生物群系、洞穴房屋和沙漠结构生成 |
| `Dungeon/` | 地牢布局、房间、家具、陷阱和设置 |
| `Runtime/` | 创建阶段的内存状态、规则、碰撞、放置、液体、物品及宿主兼容类型 |
| `Runtime/Geometry/` | 本地向量、矩形、颜色和数学实现 |
| `EntitySources/` | 生成实体的来源上下文 |
| `Configuration/` | 内嵌生成配置 |
| `Snapshots/` | 单独授权保留的调试快照代码；创建 API 不使用磁盘快照 |

`Runtime/` 按普通 .NET 类型组织，没有引入 ECS 架构。它持有创建阶段的数据；
生成结果由调用方交给自己的世界 owner 校验和提交。此库不会直接写 ECS Store，
也不接管游戏运行时的权威状态。

## 创建 API

```csharp
using NSSLC.WorldGeneration;

GeneratedWorld world = WorldCreation.Create(
  new WorldCreationRequest(
    Seed: "12345",
    Size: GeneratedWorldSize.Small,
    Evil: GeneratedWorldEvil.Corruption,
    Difficulty: 0),
  passStarting: name => Console.WriteLine(name));

GeneratedTile tile = world.GetTile(world.SpawnX, world.SpawnY);
IReadOnlyList<GeneratedChest> chests = world.Chests;
```

尺寸为 Small `4200 × 1200`、Medium `6400 × 1800`、Large `8400 × 2400`。
邪恶类型可选 Random、Corruption、Crimson；难度取值为 `0..3`。
已识别的特殊种子模式会被拒绝，当前支持范围为标准世界。
数字和文本均可作为本地种子输入；文本使用本地稳定哈希，未验证与原游戏文本
种子映射相同。

通过 `WorldCreation.Create` 创建时，静态锁串行执行原算法，每次分配新的地块和
实体缓冲，重置世界状态及可变规则表。已锁定的多格物体定义仅初始化一次。
后续创建不修改先前返回的 `GeneratedWorld`；`GetTile` 返回值拷贝，宝箱、物品、
NPC 和步骤记录为只读结果。旧 `WorldGen` 入口保留用于源码兼容，调用方应使用
`WorldCreation.Create` 获取这些生命周期保证。

生成步骤异常向调用方传播，失败时不返回完整世界。可传入 `CancellationToken`，
取消在步骤开始时检查；单个长步骤内部没有立即中断保证。

## 构建与运行

命令在仓库根目录执行。首次使用需 restore，后续使用增量构建。

```powershell
dotnet restore src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj
dotnet build src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj --no-restore

# 单个小世界
dotnet run --project src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj --no-build --no-restore -- 12345 Build/diagnostics/WorldGenerationCompile/world-summary.json

# 同一进程连续创建并比较重复种子
dotnet run --project src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj --no-build --no-restore -- 12345,54321,12345 Build/diagnostics/WorldGenerationCompile/generated-world-repeat.json Small Corruption

# 中等大小的猩红世界，专家难度
dotnet run --project src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj --no-build --no-restore -- 67890 Build/diagnostics/WorldGenerationCompile/generated-world-crimson-medium.json Medium Crimson 1
```

参数依次为种子列表、可选统计报告路径、尺寸、邪恶类型、难度。
JSON 文件为统计和哈希报告，不包含完整地块载荷。
构建输出位于 `Build/bin/`，中间文件位于 `Build/obj/`，生成文件位于 `Build/generated/`。

## 实测记录（2026-10-03）

| 场景 | 尺寸 | 宝箱 | 非空物品槽 | 带前缀物品 | 含液体地块 | 完成步骤 |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| `12345`，腐化，经典 | 4200 × 1200 | 174 | 1128 | 88 | 315431 | 106 |
| `54321`，腐化，经典 | 4200 × 1200 | 178 | 1107 | 102 | 295653 | 106 |
| `67890`，猩红，专家 | 6400 × 1800 | 304 | 1922 | 153 | 596171 | 106 |

两条运行命令的退出码均为 `0`。各场景均没有跳过生成步骤，出生区、地块/墙编号、
宝箱坐标及物品编号/数量/前缀范围检查通过。连续创建顺序为
`12345 → 54321 → 12345`，两次 `12345` 的地块和宝箱 SHA256 相同；
重新读取第一次返回的世界，其地块 SHA256 没有变化。

证据位于 `Build/diagnostics/WorldGenerationCompile/` 的
`generated-world-repeat.json`、`generated-world-crimson-medium.json` 及对应日志。
这些记录验证了上述场景；Large、其余难度和全部种子组合尚未逐一运行。
没有新增测试项目，也没有构建整套 solution。

## 依赖与支持边界

- 唯一包依赖为 `Newtonsoft.Json 13.0.3`。几何计算使用本地实现，移除了 XNA、
  ReLogic 及旧平台程序集引用。
- 物品表包含 6147 个标准种子条目和 3230 个可放置地块映射；从参考源码的
  `SetDefaults` 控制流提取生成所需属性。物品前缀使用恢复的前缀表和随机算法。
- `Runtime` 仍保留部分旧游戏 API 的兼容类型。UI、音效、粒子、网络及运行中模拟
  不属于创建 API 的支持能力；没有实现完整的 Player/NPC/Item 游戏模拟。
  NPC 名称和进度文本使用本地 key，不包含原游戏本地化资源。
- 没有进行与原 Terraria 运行时逐块/逐物品一致的对照验证。
- 未复制完整 `Terraria.IO/WorldFile.cs`、世界持久化编解码或云存储适配器。
  `Runtime/WorldFile.cs` 的保存入口仍由宿主处理；加载入口转发给已注册的存储宿主，
  自身不执行 I/O。保存 `.wld`、加载存档、将结果转换为现有存储 DTO，由持久化边界承接。
- 快照文件保留在 `Snapshots/`。创建入口不创建、加载或恢复磁盘快照，调试控制器的
  相关操作明确拒绝执行；此次没有验证独立快照代码的完整运行语义。
- 原诊断项目现只引用正式项目。历史 `Build/diagnostics/WorldGenerationCompile/Source/`
  镜像不再参与编译。

## 完整保存与加载（2026-10-04）

`GeneratedWorldDocumentProjection.Create` 已在正式 `WorldStorage/Generation/` 边界
将生成结果转换为完整的 27 个逻辑存档区段。`WorldStorage/WorldFile/WorldFileTileCodec`
实现 WorldFile 319 的四级地块标志头和纵向游程压缩，输出
`WorldFileTilePayloadSection` 接受的载荷。世界生成库只捕获创建结果，不依赖存储项目。

保存复用 `WorldSaveCoordinator`、正式文件适配器和 WorldFile encoder；加载复用
`WorldLoadCoordinator`、正式 decoder/validator 和生成的 API catalog。
`WorldStorageCoordinatorFactory.CreateGeneratedWorldLoadBindings` 绑定全部 27 个 API，
将数据交由对应领域 owner 提交到新建且未发布的 `LoadedWorldSession`。
每个候选 session 持有自己的 `WorldLoadLifecycleComponent`；发布后该状态随 session 保留，
失败恢复则在 reset/丢弃该候选时结束。
`WorldTileMetricsComponent` 同样属于活动 session，包含扫描游标、累计窗口和逐 tile 类型的
临时计数数组；旧 `WorldGen.tileCounts` 访问器每次解析到当前活动 session 的数组，切换或清空
session 时不复用前一世界的计数。
完整 `LoadedWorldSession` 在发布后由 `WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession`
保留，使 Storage、TreeTops、住房注册和区段状态不会随加载 handler 返回而丢失；成功清空后释放该引用。
发布成功时，session 的 13 区 TreeTops 样式复制到 legacy `WorldGen.TreeTops`；`WorldGen.clearWorld()`
会在失败 reset 或正常清世界时清空该运行时缓存，避免样式跨 world 残留。
211 及以上版本的 TreeTops 区段为必需；缺失时加载会失败。加载 Prepare 会拒绝超过 13 项或不属于对应区域的样式值，不再静默截断；旧 `BinaryReader` 适配器
也先读取并校验整批值，再提交到 TreeTops state。Snow 区保留 legacy 可读值 `0..8、21、22、31、32、41、42`。
所有 Prepare 完成后才开始 Commit；Footer 在其他 API 提交成功后标记完成。

游戏宿主可在调用旧 `WorldFile.LoadWorld` 前调用
`WorldStorageCoordinatorFactory.RegisterGeneratedWorldLoadHandlerWithRuntimeTileMap` 注册加载入口；
它先将 Tile 缓冲和空间元数据提交到 legacy `Main`，再投影核心 session 状态并把
`WorldGen.TownManager` 绑定到候选 session 的 `TownHousingRegistryComponent`，最后调用宿主回调提交其余世界状态。
宿主参与首个解锁后更新的运行时状态应在该发布回调中提交；加载门仍保持升起，后续 settle 或 finalization 失败时由恢复流程执行 reset。
Chest 与 Sign 快照会在宿主回调前由 `LegacyWorldChestAndSignProjection` 恢复到 runtime 表；Bestiary
kill/sight/chat 存档状态会在核心 session 投影时原子写入 `Main.BestiaryTracker`，并随 `clearWorld()` reset。
加载仍需要宿主投影 NPC/TileEntity 等数据、实现 Bestiary gameplay tracking/network 效果，以及提供宿主专属的结算前清理和最终 NPC/世界初始化。
该方法使用现有 `WorldGen.clearWorld()` 处理失败 reset；该路径会重建 `TownManager`，清除失败候选绑定。
宿主须将同一 I/O gate 传给加载、变换和 snapshot-save coordinator；加载会在读取、恢复、发布和
settle 期间持有 gate。没有提供异常 observer 时，协调异常会向 `WorldFile.LoadWorld` 调用方传播；
提供 observer 时由 observer 接收。协调器以外的意外异常会释放未发布候选的加载门；如果发布已经开始，
入口先保持加载门并执行 `clearWorld()`，只有 reset 成功才释放加载门和活动 session 引用；reset 失败时继续
门控世界更新，并将原异常与 reset 异常一起报告。
`resetHostWorldState` 在 legacy reset 成功后、生命周期投影降低加载门前执行；宿主应在此清空随发布切换的自有运行时存储。
Tile 投影先构建隔离的 `Tile[,]` 再改写 legacy `Main`，只覆盖 Tile 与其空间元数据；宿主回调仍须
提交其他世界状态，并在成功发布前保持加载门和共享 I/O gate。
如果区段 Commit 失败但候选 session 尚未开始发布，恢复会丢弃候选并保留当前活动世界；只有
发布已开始且可能部分改写运行时状态时才执行全局 reset。
`LegacyWorldLoadRecoveryEffects` 执行 QuickWater、初始水检查和最多 100,000 次液体结算，
再更新 legacy `weatherCounter`、调用 `Cloud.resetClouds()`，然后执行宿主专属清理和最后一次水检查，
之后释放加载门；结算前会核对已发布会话尺寸与
legacy `Main.tile` 缓冲一致，并刷新 `WorldGen` 用于住房边界判断的尺寸兼容快照。
`WorldGen.serverLoadWorldCallBack()`、`clearWorld()`、`Reset()`、`GenerateWorld()`、
`WorldCreation.Create()`、`WorldGenerator.TryReset()` 和 `WorldGenSnapshot.Restore()` 共用 legacy
world lifecycle gate。load callback 串行执行；clear/reset/generation/snapshot 操作若遇到其他线程已持有
gate，会 fail fast，`TryReset()` 返回 `false`，其余入口抛 `InvalidOperationException`。恢复过程中同线程
调用 `clearWorld()` 等嵌套操作可重入。
`clearWorld()`、`Reset()`、WorldGenSnapshot restore 和 `WorldGenerator.TryReset()` 的 mutation 包装器会在
完整操作期间升起 `isGeneratingOrLoadingWorld`；正常结束恢复进入前的门状态，异常则保持门升起。
`GenerateWorld()` 另记录本次 `clearWorld()` 是否完整结束，避免其 `finally` 覆盖失败 reset 留下的保护状态。
`WorldGen.UpdateWorld()` 与 lifecycle 操作共用同一 monitor：生命周期操作期间跳过新 tick；已有 tick 运行时，
读档等待该 tick 结束，其他生命周期 mutation 按各自入口策略失败或返回 `false`。这样只串行化显式使用该门的
路径；同线程 tick 回调发起生命周期操作会被拒绝，加载 hook 内重入 update 会跳过。直接修改 legacy 静态
状态的宿主代码仍须接入同一生命周期边界。
仓库当前只有 `NSSLC.Tools.Simulation.Program` 注册该方法；它连接有限 NPC catalog 和 runtime
projection，并为 Simulation 不支持的结算前清理/最终化阶段提供 no-op callback。完整游戏/server
composition root 与全量 host effects 仍未接入。

异步地形变换可通过
`WorldStorageCoordinatorFactory.RegisterWorldTransformationHandler` 接入现有
`WorldTransformWorkCoordinator`。注册时由宿主持有 `WorldTransformTransactionComponent`、
创建一次 I/O gate，并将同一 gate 传给加载和 snapshot-save coordinator；调度器的主线程回调必须
进入真实宿主队列。legacy transform 状态在 I/O gate 释放后清零，再排入 follow-up。
若旧保存仍使用 `Terraria.IO.WorldFile.IOLock`，宿主须用该锁对象调用
`CreateWorldStorageIoGate(object)`；默认无参数 gate 只在传给同一组新 coordinator 时共享。
当前没有宿主注册调用方，当前项目也不编译旧 `Terraria.IO.WorldFile`，所以两条保存路径尚未
在仓库运行时真正合流；`initializeHardMode` 的实际地形效果也未实现。

工具支持以下完整往返命令。输出目录中已有 `generated.wld` 时拒绝覆盖。

```powershell
dotnet build src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj --no-restore
dotnet run --project src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj --no-build --no-restore -- --roundtrip Build/diagnostics/WorldGenerationRoundTrip/my-small-world 12345 Small Corruption 0
```

已实测 `12345 / Small / Corruption / 0` 和 `67890 / Medium / Crimson / 1`：
分别比较 5,040,000 / 11,520,000 个地块、174 / 304 个宝箱、6960 / 12160 个物品槽
及各 2 个 NPC，并比较出生点和已映射元数据，完整保存加载通过。
比较的是 `.wld` 实际持久化字段；非重要地块 frame、墙 framing 缓存和液体处理队列
等运行缓存不参与比较。

当前完整生成世界的加载组合支持 WorldFile 319。CreativePowers 接受标准新世界的
空载荷，非空内容明确拒绝。真实生成的上述两场景没有标牌或 TileEntity；另行通过
包含 1 个标牌、9 种空内容 TileEntity、额外出生点和 NPC 历史的诊断样例验证加载。
这不包含原 Terraria 程序加载验证，也不代表完整游戏模拟或服务器接入已经完成。

详细实现、区段 owner 表和证据见
[生成世界的完整持久化往返验证](../../../docs/research/2026-10-04-generated-world-persistence-roundtrip.md)。
修复前的失败尝试保留在
[生成世界的正式加载尝试](../../../docs/research/2026-10-04-generated-world-load-attempt.md)。
