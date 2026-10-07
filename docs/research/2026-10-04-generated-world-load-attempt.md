# 生成世界的正式加载尝试

> 这是修复前的历史诊断记录。下文“当前”指本次尝试时的源码状态。
> 随后的修复已接通完整保存加载链路，见
> [生成世界的完整持久化往返验证](2026-10-04-generated-world-persistence-roundtrip.md)。

日期：2026-10-04。范围：实际调用现有世界生成、WorldFile 编码和世界加载入口，
确定生成结果能否通过当前持久化链路加载。没有实现新的持久化格式或领域加载 API。

## 结论

**世界生成成功；当前不能完成生成世界的保存与加载。**

第一处阻塞是 `GeneratedWorld` 到 WorldFile 的压缩地块载荷转换尚未接通。
本轮对真实生成数据构造了明确标记为不完整的 Header、Chest、Footer DTO 投影；
正式 validator 和 encoder 均拒绝缺少 Tile payload 的 document，没有产生 `.wld`。

随后实际调用了 `WorldLoadCoordinator.Load`：预期存档路径返回 `Missing`。
另行读取上一轮生成的统计 JSON，文件读取成功，但正式 decoder 返回 `InvalidData`。
这两次调用都未进入 owner API 执行，也未提交任何世界状态。
没有对合法的生成世界 `.wld` 完成解码或加载，不能据此声称完整加载能力已经通过验证。

## 实际运行结果

### 生成

| 项目 | 本轮结果 |
| --- | --- |
| 种子 / 场景 | `12345` / Small / Corruption / Classic |
| 尺寸 | `4200 × 1200`，5,040,000 个地块位置 |
| 出生点 | `(2095, 232)` |
| 宝箱 / 非空物品槽 | `174` / `1128` |
| 活动 NPC 记录 | `2` |
| 完成 / 跳过的生成步骤 | `106` / `0` |
| 生成耗时 | `58.3359341` 秒 |

数据来自此次调用 `WorldCreation.Create(new WorldCreationRequest("12345"))`，
不是复用历史统计数。创建入口自身的出生区校验通过。

### 编码与加载

| 操作 | 实际结果 | 到达的边界 |
| --- | --- | --- |
| `WorldFileDocumentValidator.Validate(partialDocument)` | `InvalidData`：`The world document does not contain a present Tile payload section.` | DTO 校验 |
| `WorldStorageCoordinatorFactory.CreateSaveEncoder().Encode(partialDocument)` | `Succeeded=false`，`ByteCount=0`；`A complete WorldFile requires a present Tile payload section.` | 编码前置检查 |
| `WorldLoadCoordinator.Load(generated-12345.wld, bindings, ...)` | `Status=Missing`，一次读取尝试 | 文件不存在；没有成功读出字节 |
| `WorldLoadCoordinator.Load(generated-world-12345.json, bindings, ...)` | `Status=Failed`，`InvalidData`：`Unsupported world file version 537529723.` | 文件读取成功，WorldFile 版本检查拒绝 |

`generated-12345.wld` 是本轮准备写出的路径。由于编码失败，这个文件没有被创建；
对该路径调用 Load 是文件缺失的诊断，不是对有效存档的加载验证。

`generated-world-12345.json` 是已有生成统计报告，明确不是存档，也不包含完整地块。
`537529723` 是 decoder 把 JSON 开头四个字节作为 WorldFile 版本整数读取的结果，
不是生成器写出了错误的 `.wld` 版本。

两次 Load 的 observer 均未收到 `OnDocumentValidated`，`ApiExecution` 均为 null，
`CanPublishWorldLoaded=false`。没有执行 Prepare/Commit。

## 本轮使用的正式入口

- 生成：`src/NSSLC.Infrastructure/WorldGeneration/WorldCreation.cs`。
- 编码：`src/NSSLC.Infrastructure/WorldStorage/WorldFile/WorldFileDocumentEncoder.cs`。
- 加载组合：`src/NSSLC.Infrastructure/WorldStorage/WorldStorageCoordinatorFactory.cs`。
- 加载用例：`src/NSSLC.Application/WorldStorage/Persistence/WorldLoadCoordinator.cs`。
- 文件端口实现：`src/NSSLC.Infrastructure/WorldStorage/Platform/WorldFileStoreAdapter.cs`。
- decoder / validator：正式的 `WorldFileDocumentDecoder` / `WorldFileDocumentValidator`。

诊断宿主位于 `.agent-workplace/WorldGenerationLoadAttempt/`，引用正式生成库和正式
WorldStorage 项目。它没有替换 decoder、validator 或生成 catalog，没有构造虚假
Tile payload，没有编译参考目录或旧完整 `WorldFile.cs`，没有引用原 Terraria 游戏 DLL。
组合过程中引用的 `Terraria.WorldSession` 等程序集是仓库本地组件项目。
世界生成核心项目没有新增依赖。

## 后续仍需接通的部分

| 缺口 | 源码 / 运行证据 | 归属与建议顺序 |
| --- | --- | --- |
| 完整生成结果到持久化 DTO 的映射，特别是压缩 Tile payload | 本轮 validator / encoder 均因缺少该区段拒绝；当前 `WorldFileTilePayloadSection` 只接收已有压缩字节 | 在现有 WorldStorage 边界新增适配，先形成有效 `.wld`，保留地块、墙、液体、frame 和标志 |
| 各区段对应的正式加载 API | 本轮实际读取的 `GeneratedWorldLoadApiCatalog.Descriptors` 只有 `world-generation.tree-tops.load` | 由对应领域 owner 提供 Prepare/Commit，接通地块、宝箱、NPC、世界元数据和环境状态等区段 |
| 宿主完整 runtime bindings 和载入后内容比较 | 本轮只绑定已有树冠 API 和真实树冠 owner；未进入 API 执行 | 注册各 owner，保存后清除旧会话，加载到新会话，再比较尺寸、出生点、地块、宝箱和 NPC |

当前源码还包含 `WorldLoadApiCatalogValidation` 的 `UnconsumedSection` 校验：
document 中存在但没有注册 API 消费的区段会被拒绝。因此即使补齐 Tile 编码，
只有树冠 API 的现有 catalog 也不能加载完整 document。
这是对当前源码的静态结论；本轮的两次 Load 都在更早阶段失败，没有运行到该检查。

正式 document decoder 保留压缩 Tile 字节，不负责把它展开为领域地块。
写出有效 `.wld` 后，还需要地块 owner 在加载准备阶段完成解码和校验，
再通过 owner 提交；Infrastructure 不直接写 ECS Store。

## 构建和运行证据

在仓库根目录执行：

```powershell
dotnet restore .agent-workplace/WorldGenerationLoadAttempt/WorldGenerationLoadAttempt.csproj --verbosity minimal
dotnet build .agent-workplace/WorldGenerationLoadAttempt/WorldGenerationLoadAttempt.csproj --no-restore --verbosity minimal
dotnet run --project .agent-workplace/WorldGenerationLoadAttempt/WorldGenerationLoadAttempt.csproj --no-build --no-restore -- D:/TRbackup/NLTX/Build/diagnostics/WorldGenerationLoadAttempt/2026-10-04 Build/diagnostics/WorldGenerationCompile/generated-world-12345.json
```

运行宿主要求输出目录中不存在 `generated-12345.wld`，以避免旧存档影响本轮结果。

| 操作 | 项目 | 退出码 | 警告 / 错误 |
| --- | --- | ---: | --- |
| Restore | `WorldGenerationLoadAttempt.csproj` | `0` | 无 restore 错误 |
| 增量构建 | `WorldGenerationLoadAttempt.csproj` 及其项目依赖 | `0` | `0` / `0` |
| 手工生成 / 编码 / 加载尝试 | 同一宿主 | `2` | `2` 表示尝试已执行，完整保存加载未成功，不是编译失败 |

已确认实际构建产物：
`Build/bin/WorldGenerationLoadAttempt/Debug/net10.0/WorldGenerationLoadAttempt.dll`。
中间文件、源生成器输出、包缓存分别位于 `Build/obj/`、`Build/generated/`、`Build/packages/`。
没有构建 solution，没有使用 Rebuild，没有新增或运行测试套件。

证据目录：`Build/diagnostics/WorldGenerationLoadAttempt/2026-10-04/`。

- `restore.log` / `build.log`：还原和构建命令输出。
- `run.log`：106 个生成步骤及编码、加载返回值。
- `load-attempt.json`：结构化结果、实际 catalog、observer 状态。
- `run-exit-code.txt`：实际运行退出码。
- `build-properties.json`：宿主解析的输出位置。

本轮改动仅新增私有诊断宿主、这份报告，并追加世界生成 README 的加载尝试说明。
既有持久化、加载协调器和 catalog 校验的用户修改保持原样。
