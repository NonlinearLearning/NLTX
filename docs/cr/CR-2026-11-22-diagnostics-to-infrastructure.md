# 服务器诊断日志移入 NSSLC.Infrastructure

文档 ID：CR-2026-11-22-DIAGNOSTICS-TO-INFRASTRUCTURE  
逻辑域：file-organization、architecture  
产物类型：decision、execution  
状态：completed  
日期：2026-11-22  
canonical 路径：`docs/cr/CR-2026-11-22-diagnostics-to-infrastructure.md`  
上游依据：[项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md) §3.1、§5、§6；
[ECS 领域层与基础设施六边形架构边界](../../Context/架构设计/ECS领域与基础设施架构边界.md) §2、§3、§5；
[ECS 文件组织设计约束](../../Context/架构设计/ECS文件组织设计约束.md) §2.1

## 1. 授权与范围

用户要求「Diagnostics 移动到 NSSLC.Infrastructure」。本轮把 `src/NSSLC/Diagnostics/` 整体
移入 `src/NSSLC.Infrastructure/Diagnostics/`，并新增独立工程
`NSSLC.Infrastructure.Diagnostics.csproj` 承接编译。`Terraria.Diagnostics` 命名空间、公共
类型、方法体和日志输出格式全部不变；`Terraria.Components.csproj` 不再编译这些源文件。

`src/NSSLC.Infrastructure/` 是用户明确保护的目录。本轮只新增 `Diagnostics/` 子目录与一个新
工程，没有修改、修复或删除该目录中任何既有文件或项目；其余旧项目仍保持「可能引用已删除
项目」的原状。

## 2. 目标结构

```text
src/NSSLC.Infrastructure/
  Diagnostics/                     # 新增：日志端口、级别与宿主适配器
    NSSLC.Infrastructure.Diagnostics.csproj
    README.md
    ConsoleServerLogSink.cs
    IServerLogSink.cs
    ServerLog.cs
    ServerLogEntry.cs
    ServerLogger.cs
    ServerLogLevel.cs
  Network/  WorldData/  WorldGeneration/  WorldStorage/   # 未改动
```

`src/NSSLC/` 的 ECS 领域目录从 18 个减为 17 个，`Diagnostics` 不再是共享组件域。

## 3. 移动清单

| 原路径 | 新路径 |
| --- | --- |
| `src/NSSLC/Diagnostics/ConsoleServerLogSink.cs` | `src/NSSLC.Infrastructure/Diagnostics/ConsoleServerLogSink.cs` |
| `src/NSSLC/Diagnostics/IServerLogSink.cs` | `src/NSSLC.Infrastructure/Diagnostics/IServerLogSink.cs` |
| `src/NSSLC/Diagnostics/ServerLog.cs` | `src/NSSLC.Infrastructure/Diagnostics/ServerLog.cs` |
| `src/NSSLC/Diagnostics/ServerLogEntry.cs` | `src/NSSLC.Infrastructure/Diagnostics/ServerLogEntry.cs` |
| `src/NSSLC/Diagnostics/ServerLogger.cs` | `src/NSSLC.Infrastructure/Diagnostics/ServerLogger.cs` |
| `src/NSSLC/Diagnostics/ServerLogLevel.cs` | `src/NSSLC.Infrastructure/Diagnostics/ServerLogLevel.cs` |
| `src/NSSLC/Diagnostics/README.md` | `src/NSSLC.Infrastructure/Diagnostics/README.md`（移动后改写为基础设施工程说明） |

共 7 个文件。6 个 `.cs` 文件由 `git diff --cached --find-renames --summary` 识别为 100% 纯
重命名（R100，0 增 0 删）；`README.md` 先按原样移动，再改写内容。

## 4. 结构调整理由

- 日志是可观察性副作用，不属于 ECS 权威状态；原 `README.md` 已写明「不建立日志 Component
  或 ECS System」。既然不是领域状态，就不应长期留在共享组件程序集里。
- 共享组件程序集（`Terraria.Components`）只承载领域状态、规则与契约。日志端口带宿主效果
  （控制台写入、时间源），放在领域程序集里会让「引用领域」顺带获得平台副作用能力。
- 按六边形边界，日志出口属于基础设施适配器：端口与适配器同处一个不依赖任何项目的独立
  工程，由宿主和网络装配引用，ECS 领域层不再接触文件/控制台实现。
- 未借搬家顺手改命名空间或 API：项目文件组织约束 §5 要求目录移动优先保持行为、公共 API
  和命名空间不变，因此 `Terraria.Diagnostics` 原样保留。

## 5. 引用与文档处理

- `src/NSSLC/Terraria.Components.csproj`：删除 `<Compile Include="Diagnostics/**/*.cs" />`。
- `src/NSSLC.Infrastructure/Diagnostics/NSSLC.Infrastructure.Diagnostics.csproj`（新增）：
  `net10.0`、`Nullable`/`ImplicitUsings` 开启、`EnableDefaultCompileItems=false` 且显式
  `Compile Include="*.cs"`，与 `NSSLC.Infrastructure.Network.csproj` 的写法一致；
  `AssemblyName=NSSLC.Infrastructure.Diagnostics`、`RootNamespace=Terraria.Diagnostics`；
  无包引用、无项目引用。
- `src/NSSLC.Server/NSSLC.Server.csproj`：新增对该工程的 `ProjectReference`。
- `src/NSSLC.Network/NSSLC.Network.csproj`：新增对该工程的 `ProjectReference`，并在既有
  依赖方向注释后补一条例外说明——本工程只用它输出可观察性事件，不通过它读写领域状态。
- `src/NSSLC/README.md`：领域表移除 `Diagnostics` 行，扁平目录清单同步移除，并补一段说明
  日志已迁往基础设施侧、本表只列 ECS 领域。
- `README.md`：领域清单移除 `Diagnostics`；在受保护目录说明中补充唯一例外，即当前服务器与
  网络工程正式引用的 `Diagnostics/`。
- `src/NSSLC.Infrastructure/Diagnostics/README.md`：改写成工程说明，记录编译工程、迁移来源、
  保持不变的命名空间与依赖方向。
- 历史文档（`docs/cr/CR-2026-11-22-nsslc-domain-directory-flatten.md`、
  `docs/reviews/audits/2026-11-01-ecs-system-input-audit.md` 等）保持原样，作为当时证据。

## 6. 验证

| 命令 | 退出码 | 警告 | 错误 | 输出 |
| --- | ---: | ---: | ---: | --- |
| `dotnet build src/NSSLC.Infrastructure/Diagnostics/NSSLC.Infrastructure.Diagnostics.csproj --nologo --verbosity minimal` | 0 | 0 | 0 | `Build/bin/NSSLC.Infrastructure.Diagnostics/Debug/net10.0/` |
| `dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal` | 0 | 0 | 0 | `Build/bin/Terraria.Components/Debug/net10.0/` |
| `dotnet build src/NSSLC.Network/NSSLC.Network.csproj --nologo --verbosity minimal` | 0 | 0 | 0 | `Build/bin/NSSLC.Network/Debug/net10.0/` |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj --nologo --verbosity minimal` | 0 | 0 | 0 | `Build/bin/NSSLC.Server/Debug/net10.0/` |

补充核对：

- `dotnet msbuild ... -getItem:Compile`：新工程解析到 6 个 `Compile` 项，恰为移入的 6 个
  `.cs`；`Terraria.Components.csproj` 解析到 536 个 `Compile` 项，集合中不含任何
  `Diagnostics` 源文件。
- 程序集元数据抽查：`Terraria.Components.dll` 不再包含 `ConsoleServerLogSink`、
  `ServerLogEntry` 标识；`NSSLC.Infrastructure.Diagnostics.dll` 含 `Terraria.Diagnostics`
  命名空间；`NSSLC.Network.dll` 记录对 `NSSLC.Infrastructure.Diagnostics` 的引用；
  `Build/bin/NSSLC.Server/Debug/net10.0/` 已复制该 DLL。
- `git diff --check` 退出码 0；`git diff --cached --find-renames --summary` 对 7 个文件给出
  R100 纯重命名。

未运行：`Test/` 下的 13 个验证项目（没有任何验证工程引用 `Terraria.Diagnostics`，本轮不改
行为，仅换程序集归属）；未启动服务器进程，也未做客户端联调。

## 7. 回滚

本轮只改动 5 个文本位置（3 个 `.csproj`、2 个 `README.md`）和 1 个目录位置。回滚时逐条反向
编辑即可，**不要**对 `src/NSSLC.Server/NSSLC.Server.csproj`、`src/NSSLC.Network/NSSLC.Network.csproj`
使用 `git checkout`：它们还带有不属于本轮的并行修改。

```powershell
# 1. 移回源码（README 先删除，再用 HEAD 版本恢复）
git mv src/NSSLC.Infrastructure/Diagnostics/*.cs src/NSSLC/Diagnostics/
git rm --cached src/NSSLC.Infrastructure/Diagnostics/README.md
Remove-Item src/NSSLC.Infrastructure/Diagnostics/README.md
git checkout HEAD -- src/NSSLC/Diagnostics/README.md
# 2. 删除新增工程文件
Remove-Item src/NSSLC.Infrastructure/Diagnostics/NSSLC.Infrastructure.Diagnostics.csproj
# 3. 反向编辑 3 个 .csproj 与 2 个 README.md（见第 5 节清单）
```

## 8. 未覆盖与风险

- `NSSLC.Network`（网络领域程序集）现在引用一个基础设施工程。该引用只承载日志输出，已在
  工程注释和 README 记录；如果后续要求严格的单向依赖，应把日志端口下沉到独立 Contracts
  工程，而不是回退到领域程序集。
- `src/NSSLC.Infrastructure/` 其余旧项目依旧引用已删除的 `NSSLC.Application`、
  `NSSLC/Component/*` 等工程，仍不可编译；本轮按保护要求未修复。
- 迁移不改变日志文本格式与调用点，因此没有新增日志行为验证；服务器重启后的实际输出仍需
  下一次运行确认。
- 工作区存在不属于本轮的并行修改（`Communication` 迁出、`Program.cs`/`ServerTickPacing.cs`、
  服务器 GC 配置等），本轮未触碰；最终构建已包含这些改动且为 0 错误。
