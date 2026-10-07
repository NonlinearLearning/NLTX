# 生成世界的完整持久化往返验证

日期：2026-10-04。范围：标准生成世界的完整 DTO 投影、WorldFile 319 地块压缩、
区段加载 API、宿主绑定及新世界会话的数据比较。

## 结论

**已修复此前的三个阻塞，并实际完成两个真实生成世界的保存、加载和数据比较。**

保存使用正式 `WorldSaveCoordinator`、WorldFile encoder 和文件适配器；加载使用正式
`WorldLoadCoordinator`、decoder、validator、生成 catalog 和完整 bindings。
目标为新建且未发布的 `LoadedWorldSession`，没有复用生成器的静态状态或原始地块缓冲。

最终运行记录如下。两条命令的退出码均为 `0`；两场景的 `LoadStatus` 均为 `Loaded`，
`CanPublishWorldLoaded` 为 `true`，执行 27 个区段 API。

| 场景 | 尺寸 | 地块逐格比较 | 宝箱 | 全部物品槽 | 非空槽 | NPC | 出生点 |
| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |
| `12345 / Small / Corruption / 0` | 4200 × 1200 | 5,040,000 | 174 | 6960 | 1128 | 2 | (2095, 232) |
| `67890 / Medium / Crimson / 1` | 6400 × 1800 | 11,520,000 | 304 | 12160 | 1922 | 2 | (3196, 352) |

两场景也比较了世界身份、尺寸、环境、时间、矿石、进度、NPC 解锁、树冠、
出生配置、住房、图鉴、压力板、文件元数据和区段提交完整性。
真实生成场景中标牌、TileEntity 均为 0；其非空加载另有诊断样例，见下文。

## 实现与职责

| 位置 | 本次职责 |
| --- | --- |
| `WorldGeneration/GeneratedWorld.cs`、`GeneratedWorldSettings.cs` | 在生成锁内捕获只读地块、宝箱、NPC、标牌、TileEntity 身份、规则表、Manifest 和元数据 |
| `WorldStorage/Generation/GeneratedWorldDocumentProjection.cs` | 将显式生成结果投影为完整 `WorldPersistenceDocument` |
| `WorldStorage/WorldFile/WorldFileTileCodec.cs` | 将 `GeneratedTile` 编码为压缩 Tile payload；解码到不可变领域快照 |
| `WorldStorage/WorldFile/WorldFileTileEntityCodec.cs` | 编码生成期空内容实体；类型化解析 319 的 0–8 号实体记录 |
| `NSSLC.Application/WorldStorage/Loading/` | 区段 Prepare/Commit/Discard、领域 owner 调用及未发布会话组合 |
| `NSSLC/Component/WorldStorage/` | 地块、宝箱、NPC、标牌、TileEntity 和压力板的受控状态提交 |
| `NSSLC/Component/WorldSession/` | 世界描述、规则、时间天气、进度、外观、里程碑、季节政策和 NPC 历史提交 |
| `WorldStorage/WorldStorageCoordinatorFactory.cs` | 注入 codec，绑定所有区段 API 与实际目标 owner |
| `NSSLC.Tools.WorldGeneration/WorldPersistenceRoundTrip.cs` | 实际生成、保存、加载及逐项比较；写运行报告 |

上述 `WorldStorage/` 路径均位于正式 `src/NSSLC.Infrastructure/`。
投影和地块压缩没有放回世界生成算法或参考目录。
WorldStorage 项目引用独立 WorldGeneration 项目；生成库不反向依赖 WorldStorage 或
Application，也不引用、继承、加载或反射原 Terraria 游戏程序集。

领域 owner 使用稳定的快照/状态契约；codec 不直接写 ECS Store。
额外出生点归 `WorldDescriptorState`，已变化的城镇 NPC 编号归领域 NPC 历史，
世界里程碑和永久季节政策分别归对应组件。文件版本、frame importance、归档元数据
及加载完成标记留在应用组合中。

## 地块压缩与比较语义

Tile codec 按列遍历，实现 WorldFile 319 的四级标志头及纵向 RLE。
支持 byte 和 int16 游程长度，并消费生成结果捕获的 frame importance 和压缩规则表。

保存与比较覆盖：

- 活动状态、地块类型、墙类型、重要地块 frame；
- 液体数量及水、熔岩、蜂蜜、微光类型；
- 四种线路、半砖、斜坡、actuator、inactive 状态；
- 地块/墙涂料、隐形及 fullbright 状态。

`Normalize` 统一源值和解码值的持久化语义。非活动地块残留 type/frame、非重要地块
frame、墙 framing 缓存及液体处理队列标志不属于 `.wld` 保存内容。空物品槽同样只保存
空槽状态。因此“比较一致”表示持久化字段一致，不表示所有原始运行缓存字节一致。

codec 拒绝截断载荷、跨列游程、保留游程标志、无效类型/涂料/形状和尾随字节。
解码上限为 25,000,000 个地块，压缩载荷上限为 64 MiB。

最终地块 SHA256：

| 场景 | 持久化地块 SHA256 |
| --- | --- |
| Small | `DD81D76FE2C6413CC613715C721576124AC7D20E1769AB7DDBBCB1B7A5BA0D3B` |
| Medium | `E2EB0FC5EB5B78CF1351AC09962000591724C592621A39337E17AD39AE1BCC89` |

## 27 个逻辑区段与加载 owner

以下是应用 DTO 的逻辑区段。它们由正式 encoder 写入 WorldFile 的物理布局，
不表示新增了 27 个物理 section pointer。

| 逻辑区段 | 加载 API | 提交目标/职责 |
| --- | --- | --- |
| `world.header` | `world.header.load` | 世界描述与规则；归档时间 |
| `world.metadata` | `world.metadata.load` | 归档 Revision/Favorite |
| `world.environment` | `world.environment.load` | 环境、主要出生点、地牢、层高、时间 |
| `world.progression` | `world.progression.load` | 进度、里程碑、入侵、矿石和天气 |
| `world.quests` | `world.quests.load` | NPC 任务/营救历史及事件计时 |
| `world.banners` | `world.banners.load` | 击杀与可领取旗帜计数 |
| `world.boss-progression` | `world.boss-progression.load` | Boss、入侵、月球事件及快进时间 |
| `world.party` | `world.party.load` | 世界派对状态 |
| `world.sandstorm` | `world.sandstorm.load` | 沙尘暴状态 |
| `world.defender-event` | `world.defender-event.load` | 酒馆 NPC 与 DD2 进度 |
| `world.backgrounds` | `world.backgrounds.load` | 额外背景样式 |
| `world.event-flags` | `world.event-flags.load` | 战斗书及灯笼夜状态 |
| `world.seasonal` | `world.seasonal.load` | 当日季节事件及基础矿石 |
| `world.npc-unlocks` | `world.npc-unlocks.load` | 城镇 NPC 解锁及相关进度 |
| `world.time-policy` | `world.time-policy.load` | 时间、永久季节政策和对应规则 |
| `world.tree-tops` | `world-generation.tree-tops.load` | 复用已有树冠 owner API |
| `world.spawn-points` | `world.spawn-points.load` | 额外出生点、出生规则、Manifest |
| `world.tile-payload` | `world.tiles.load` | 解压快照交给地块 owner |
| `world.chests` | `world.chests.load` | 宝箱位置、名称、物品槽 |
| `world.signs` | `world.signs.load` | 标牌位置与文本 |
| `world.npcs` | `world.npcs.load` | NPC slot store 与变化历史 |
| `world.tile-entities` | `world.tile-entities.load` | 类型化 TileEntity 快照交给 owner |
| `world.pressure-plates` | `world.pressure-plates.load` | 压力板登记 |
| `world.town-manager` | `world.town-manager.load` | 复用城镇住房登记 owner |
| `world.bestiary` | `world.bestiary.load` | 图鉴击杀、看到/交谈历史 |
| `world.creative-powers` | `world.creative-powers.load` | 校验标准新世界的空状态；拒绝非空载荷 |
| `world.footer` | `world.footer.load` | 在其他 API 成功后核对身份、标记完成 |

全部逻辑区段有 schema 和唯一消费 API，继续保留 `UnconsumedSection` 拒绝规则。
Header、Tile payload 和 Footer 为必需区段；其他 API 支持区段缺失的初始化状态。

## 生命周期与失败行为

1. 宿主新建未发布的目标，并通过 factory 注册完整 bindings。
2. 正式文件适配器读取 `.wld`；decoder/validator 检查格式、区段和跨区段身份/坐标。
3. 所有 API Prepare 完成；地块和 TileEntity 在此阶段取得类型化快照。
4. 根据显式 `CommitAfter` 依赖调用领域 owner 提交，Footer 最后标记完成。
5. 调用方只在完整成功后发布目标。完整加载失败的未发布目标应丢弃。

Prepare 不修改领域状态；当前暂存均为托管不可变数据，没有需主动释放的租借资源。
若 Commit 已开始后失败，协调器的 reset/recovery 语义仍适用；本次没有声称多 owner
提交具有数据库式回滚能力。已有内容的目标会在 Prepare 阶段被拒绝。

WorldFile 319 的非空 TileEntity 在正式 validator 中结合 Header 尺寸校验锚点，
避免到 owner Commit 才发现越界。旧版本的有界原始区段保留策略继续有效。

## 补充诊断

私有宿主：`.agent-workplace/WorldStorageLoadProbes/WorldStorageLoadProbes.csproj`。
使用正式 decoder、encoder、文件适配器、validator、catalog 和 loader，
没有替换持久化实现。最终运行退出码为 `0`，14 项检查全部通过。

| 检查 | 实测结果 |
| --- | --- |
| 四级 Tile header 的所有持久化标志 | 手写独立载荷正确解码；包括高位类型、重要 frame、微光、涂料、线路和 coating |
| int16 游程长度 | 513 格空列正确展开 |
| TileEntity 越界、截断 | 正式加载拒绝，绑定目标未修改 |
| 地块跨列、截断、保留游程标志、未知类型、尾随字节 | Prepare 拒绝，绑定目标未修改 |
| 重复宝箱坐标 | Prepare 拒绝，绑定目标未修改 |
| 非空未知 CreativePower | Prepare 拒绝，绑定目标未修改 |
| 非空 owner 区段 | 9 种空内容 TileEntity 的编号/类型/锚点/空物品、1 个标牌、1 个额外出生点及 1 个 NPC 历史编号一致 |
| 再次加载到已有内容的目标 | Prepare 拒绝，原 WorldId 和地块 mutation revision 未变 |
| 未提供 bindings | Binding 阶段拒绝，未要求世界 reset |

非空样例是对真实生成 `.wld` 的显式区段替换，用来验证对应 API。
这些样例不表示生成器自然产生了相应对象，也不证明所有含物品 TileEntity 的组合均已验证。

## 命令与证据

命令在仓库根目录执行。首次 restore 以已有缓存和项目依赖为准；本轮只做受影响项目
增量构建，没有构建 solution、没有使用 Rebuild，也没有新增或运行测试套件。

```powershell
dotnet build src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj --no-restore --verbosity minimal
dotnet build .agent-workplace/WorldStorageLoadProbes/WorldStorageLoadProbes.csproj --no-restore --verbosity minimal

dotnet run --project src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj --no-build --no-restore -- --roundtrip Build/diagnostics/WorldGenerationRoundTrip/12345-small-final 12345 Small Corruption 0
dotnet run --project src/NSSLC.Tools.WorldGeneration/NSSLC.Tools.WorldGeneration.csproj --no-build --no-restore -- --roundtrip Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final 67890 Medium Crimson 1
dotnet run --project .agent-workplace/WorldStorageLoadProbes/WorldStorageLoadProbes.csproj --no-build --no-restore -- Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld Build/diagnostics/WorldGenerationRoundTrip/probes-final
```

工具要求新输出目录，重跑时改用新目录，避免覆盖已有 `generated.wld`。
诊断证据根为 `Build/diagnostics/WorldGenerationRoundTrip/`：

- `12345-small-final/generated.wld`、`roundtrip.json`；
- `67890-medium-final/generated.wld`、`roundtrip.json`；
- `run-12345-small-final.log`、`run-67890-medium-final.log` 和对应退出码文件；
- `probes-final/probes.json`、`probe-run-final.log` 和退出码文件；
- `tool-build-final.log`、`probe-build-final.log` 和构建退出码文件。

最终增量构建退出码均为 `0`，错误数均为 `0`。正式工具构建有 `1` 个警告，私有诊断宿主
构建为 `0` 警告。工具的警告为现有 `WorldLoadRecoveryCoordinator.cs` 中的 `CS8714`
可空泛型约束警告；不属于本次直接往返所调用的恢复路径。
这些计数对应上述最终增量命令；此前触发依赖重编译时也存在原生成源码及领域项目的
既有警告，未将增量结果解释为整个仓库没有警告。
构建产物已确认位于：

- `Build/bin/NSSLC.Tools.WorldGeneration/Debug/net10.0/NSSLC.Tools.WorldGeneration.dll`；
- `Build/bin/WorldStorageLoadProbes/Debug/net10.0/WorldStorageLoadProbes.dll`。

最终 Small 存档 2,984,821 字节，Medium 存档 6,639,182 字节。
分别实测生成 63.2405 / 125.4391 秒，总计 77.4738 / 152.1046 秒；这是当前机器的
运行记录。存档还包含新生成的身份和时间，文件字节数不用于地块确定性比较。

## 支持范围

### 后续独立加载 API 测试

同日补充了独立进程加载测试。直接调用 `WorldLoadCoordinator.Load` 读取上述两份
`*-final/generated.wld`，不重新生成世界。每场景加载 27 个 API，地块 SHA256 与生成
报告一致，出生点和记录数一致，宝箱、NPC、标牌、TileEntity 与存档 DTO 逐字段检查，
并复用元数据比较。再次加载被拒绝后重新比较仍一致；源存档 SHA256 未变。

两次测试退出码均为 `0`，独立验证项目增量构建 `0` 警告、`0` 错误。
可重复命令及验证入口见
[独立加载 API 验证](../../Test/NSSLC.WorldStorage.GeneratedWorldLoadVerification/README.md)。
本次证据另存于 `Build/diagnostics/WorldLoadApiVerification/` 的
`small-load.json`、`medium-load.json`、运行日志及退出码文件。

### 当前边界

- 此完整加载组合当前支持 WorldFile **319**；底层 document decoder 的 88–326
  读取范围不等于这些新 owner API 对所有版本均已适配。
- 标准新世界中生成器未维护的动态服务状态采用明确的新世界默认值，例如空 Party、
  Banner、Bestiary、CreativePowers 及 CultistDelay 86400。
- CreativePowers 非空数据明确拒绝。生成期 TileEntity 快照当前保存身份并编码空内容；
  本次没有验证含物品/动态内容 TileEntity 的全面往返。
- 本次运行 Small 腐化经典和 Medium 猩红专家；Large、其他难度和全部种子组合未逐一运行。
- 目标是仓库自己的新世界会话。没有运行原 Terraria 程序验证 `.wld`，没有验证完整
  游戏模拟、联机服务器发布或云存储路径。

修复前证据保留在
[生成世界的正式加载尝试](2026-10-04-generated-world-load-attempt.md)。
世界生成目录的使用说明见
[WorldGeneration README](../../src/NSSLC.Infrastructure/WorldGeneration/README.md)。
