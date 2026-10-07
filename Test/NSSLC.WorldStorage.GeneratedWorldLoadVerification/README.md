# 生成世界的独立加载 API 验证

验证入口为正式 `WorldLoadCoordinator.Load(worldPath, bindings)`。
每条运行命令在新进程中创建空的 `LoadedWorldSession`，读取已存在的 `.wld`，
并通过完整的生成 catalog 与 owner bindings 加载数据。
验证入口不调用 `WorldCreation.Create`，不执行保存或覆盖源存档。

## 输入与断言

参数：`<existing.wld> <original-roundtrip.json> <new-load-report.json>`。
第二个参数为世界生成工具 `--roundtrip` 先前产生的成功报告，必须与存档对应。
第三个参数必须是新的报告文件路径。

检查内容：

- 加载成功、目标完成、API 执行成功；
- 世界身份、尺寸和出生点与生成报告一致；
- 遍历加载后的全部地块，计算同一持久化字段布局的 SHA256，与生成报告比较；
- 宝箱坐标、名称、全部物品槽以及 NPC 字段与源存档 DTO 逐项一致；
- 标牌、TileEntity、映射元数据及区段提交完整性一致；
- 各类记录数与生成报告一致；
- 再次加载到已有内容的目标被拒绝，重新比较数据仍一致；
- 源 `.wld` 在整个验证前后的 SHA256 一致。

项目链接现有工具的 `WorldPersistenceRoundTrip.cs` 与
`WorldRoundTripMetadataComparison.cs`，复用断言及元数据比较实现。
本项目入口只调用 `Require` 和 `Compare`，不调用往返工具的 `Run` 方法。
没有引入替代 loader、decoder、validator 或 owner mock。

## 构建与运行

在仓库根目录运行：

```powershell
dotnet restore Test/NSSLC.WorldStorage.GeneratedWorldLoadVerification/NSSLC.WorldStorage.GeneratedWorldLoadVerification.csproj --verbosity minimal
dotnet build Test/NSSLC.WorldStorage.GeneratedWorldLoadVerification/NSSLC.WorldStorage.GeneratedWorldLoadVerification.csproj --no-restore --verbosity minimal

dotnet run --project Test/NSSLC.WorldStorage.GeneratedWorldLoadVerification/NSSLC.WorldStorage.GeneratedWorldLoadVerification.csproj --no-build --no-restore -- Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/roundtrip.json Build/diagnostics/WorldLoadApiVerification/small-load.json
dotnet run --project Test/NSSLC.WorldStorage.GeneratedWorldLoadVerification/NSSLC.WorldStorage.GeneratedWorldLoadVerification.csproj --no-build --no-restore -- Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/roundtrip.json Build/diagnostics/WorldLoadApiVerification/medium-load.json
```

重跑时改用新的输出 JSON 路径。输入存档及生成报告由世界生成工具事先提供，
不提交进测试项目。构建输出集中在 `Build/bin/`，验证输出集中在 `Build/diagnostics/`。

## 2026-10-04 实测

| 场景 | 地块哈希覆盖位置 | 宝箱 | 物品槽 | NPC | API | 退出码 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Small 腐化经典，`12345` | 5,040,000 | 174 | 6960 | 2 | 27 | 0 |
| Medium 猩红专家，`67890` | 11,520,000 | 304 | 12160 | 2 | 27 | 0 |

两场景均为 `Loaded`，文件读取次数 `Attempts = 1`，目标初始为 fresh，
`ApiExecutionSucceeded` 与 `CanPublishWorldLoaded` 均为 true。
地块哈希、记录数、DTO 字段、元数据、重复加载拒绝和源文件不变检查全部通过。
此验证只提交到未发布的领域会话，不运行服务器的发布、液体推进或 NPC 游戏模拟。

Restore、增量构建、两次运行退出码均为 `0`。
构建 `0` 警告、`0` 错误；实际产物：
`Build/bin/NSSLC.WorldStorage.GeneratedWorldLoadVerification/Debug/net10.0/NSSLC.WorldStorage.GeneratedWorldLoadVerification.dll`。

运行证据位于 `Build/diagnostics/WorldLoadApiVerification/`：
`restore.log`、`build.log`、`small-run.log`、`medium-run.log`、对应退出码文件及
`small-load.json`、`medium-load.json`。
