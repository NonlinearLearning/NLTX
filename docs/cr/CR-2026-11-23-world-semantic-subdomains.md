# NpcSpawn / Session / TileEntity 收进 World 语义子域

文档 ID：CR-2026-11-23-WORLD-SEMANTIC-SUBDOMAINS  
逻辑域：file-organization  
产物类型：decision、execution  
状态：completed  
日期：2026-11-23  
canonical 路径：`docs/cr/CR-2026-11-23-world-semantic-subdomains.md`  
上游依据：[ECS 文件组织设计约束](../../Context/架构设计/ECS文件组织设计约束.md) §2.2、§3.1、§3.4、§6；
[项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md) §5、§6  
前置：[CR-2026-11-23 NSSLC 恢复 Share 收容层](CR-2026-11-23-nsslc-share-layer-restore.md)、
[CR-2026-11-22 NSSLC 领域内按 ECS 职责分目录](CR-2026-11-22-nsslc-ecs-role-directory-layout.md)

## 1. 授权与范围

用户要求：把 `NpcSpawn`、`Session`、`TileEntity` 三个领域移动到 `World`，并使用组件系统的
四类职责文件夹管理。用户在两种目录形态中选定「World 下建三个语义子域，各自带
`Component/`、`Command/`、`Query/`、`System/`」，不采用把三个域的文件平铺合并进 World
自有角色目录的方案。

本轮只移动目录、更新编译项与路径引用，不改任何 `.cs` 内容、命名空间、公共 API 或运行时
行为。

| 起点 | 目标 | 文件数 |
| --- | --- | --- |
| `src/NSSLC/NpcSpawn/` | `src/NSSLC/World/NpcSpawn/` | 12 |
| `src/NSSLC/Session/` | `src/NSSLC/World/Session/` | 3 |
| `src/NSSLC/TileEntity/` | `src/NSSLC/World/TileEntity/` | 15 |

合计 30 个文件，`git status` 全部识别为 R100 纯重命名，0 增 0 删。

## 2. 目标结构

```text
src/NSSLC/World/
  WorldInteractionSystem.cs       # World 自有主系统文件，留 World 根
  WorldTickSystem.cs
  README.md
  Component/                      # World 自有组件
  Query/                          # World 自有查询
  System/                         # World 自有规则、运行时与模型
  NpcSpawn/
    NpcSpawnSystem.cs             # 子域主系统文件，留子域根
    Component/  System/
  Session/                        # 无 *System.cs，保持扁平
    ISessionPlayerSource.cs  SessionEpoch.cs  SessionPlayer.cs
  TileEntity/
    WorldTileEntityRequestSystem.cs
    Command/  Component/  System/
```

## 3. 与既有约束的关系

[ECS 文件组织设计约束](../../Context/架构设计/ECS文件组织设计约束.md) §3.4 规定语义子域
（`Physics/Movement`、`Attributes/Buff` 等）视为独立领域、按同一规则再分，父领域根只放
父领域自己的 `*System.cs`。本轮把三个域作为 `World/` 下的语义子域落位，与该条文一致：
`World/` 根仍只有两个 `*System.cs` 与 `README.md`，子域各自保留自己的角色目录与主系统
文件。

§2.2「小目录扁平化」与 §3.4「没有 `*System.cs` 的目录不建这四类目录」决定了 `Session/`
保持扁平：它只有三个类型，没有系统，不为其模板完整性预建空角色目录。

[项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md) §5 规定目录移动优先
保持命名空间不变，本轮沿用：`World/NpcSpawn/` 仍是 `Terraria.Npc`，`World/Session/` 仍是
`Terraria.Session`，`World/TileEntity/` 仍是 `Terraria.World`。目录与命名空间不一致是既定
事实，不在移动时顺手修正。

语义依据：三个子域都读写同一份权威世界状态——刷怪按 tile 网格找地面并校验可见性、会话
投影为世界内的在线玩家、Tile 实体挂在世界坐标上——因此挂在 `World/` 下而不是继续散落在
项目根。

## 4. 引用更新

| 文件 | 处理 |
| --- | --- |
| `src/NSSLC/Terraria.Components.csproj` | 删除 `NpcSpawn/**/*.cs`、`Session/**/*.cs`、`TileEntity/**/*.cs` 三条编译项；三者已被既有的 `World/**/*.cs` 覆盖，不新增宽泛 glob |
| `src/NSSLC/README.md` | 项目根领域表删去三行；新增「`World/` 下的语义子域」小节，列出三个子域、内容与命名空间；扁平目录清单的 `Session` 改为 `World/Session` |
| `README.md` | 项目根领域清单删去三个名字，补一句说明它们是 `World/` 下的语义子域 |
| `src/NSSLC/World/README.md` | 「职责分层（2026-11-01）」改为「目录结构」，按四类职责目录重写目录树并补三个子域的角色与命名空间；「运算与更新的边界」中已不存在的 `Solver/`、`Model/`、`Contract/` 表述改指 `System/` 与 `Query/` |
| `Context/架构设计/项目文件组织设计约束.md` | §5 的目录与命名空间解耦示例路径改为 `src/NSSLC/World/NpcSpawn/` |
| `Test/README.md` | 第 3 条「项目名跟生产目录」的示例路径改为 `src/NSSLC/World/NpcSpawn/`、`src/NSSLC/World/TileEntity/` |

历史审计、计划、旧 CR 与迁移台账中的旧路径作为当时证据保留，不逐条改写。旧路径按下表
换算：

| 旧前缀 | 新前缀 |
| --- | --- |
| `src/NSSLC/NpcSpawn/` | `src/NSSLC/World/NpcSpawn/` |
| `src/NSSLC/Session/` | `src/NSSLC/World/Session/` |
| `src/NSSLC/TileEntity/` | `src/NSSLC/World/TileEntity/` |

`Test/` 下 14 个验证项目的 csproj 只引用 `src/NSSLC/Terraria.Components.csproj`，不含领域
路径，无需改动。验证项目名按 [Test 验证项目组织](../../Test/README.md) 第 3 条跟生产目录而
不跟命名空间，`Terraria.NpcSpawn` 与 `Terraria.TileEntity` 仍然指向正确的生产目录名，本轮
不改名。

## 5. 验证

- 移动：`git mv` 逐域执行，30 条重命名全部为 R100，工作区无内容修改。
- `dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal`：退出码 0，
  0 警告，0 错误，产物 `Build/bin/Terraria.Components/Debug/net10.0/`。
- `dotnet msbuild src/NSSLC/Terraria.Components.csproj -getItem:Compile` 解析到 536 个 `.cs`
  编译项；磁盘上（排除 `src/NSSLC/Build/`）同为 536 个 `.cs`，未编译文件 0 个，说明删除
  三条冗余编译项后没有文件落在编译 glob 之外。
- `dotnet build src/NSSLC.Server/NSSLC.Server.csproj --nologo --verbosity minimal`：退出码 0，
  0 错误，19 条警告全部来自未修改的 `src/NSSLC.Infrastructure/WorldGeneration`。
- 14 个验证项目逐个 `dotnet build` + `dotnet run --no-build --no-restore`：14/14 构建退出码 0、
  运行退出码 0，共 268 条 `PASS`、0 条 `FAIL`，汇总在
  `Build/diagnostics/world-subdomains-20261123/test-summary.csv`。

## 6. 回滚

```powershell
foreach ($d in 'NpcSpawn','Session','TileEntity') {
  git mv "src/NSSLC/World/$d" "src/NSSLC/$d"
}
```

随后在 `Terraria.Components.csproj` 恢复三条编译项，并按第 4 节表格反向恢复文档改动。

## 7. 未覆盖与风险

- 目录移动未改变命名空间，三个子域的目录名与命名空间继续不一致；这是 §5 要求的既定
  事实，不在本轮修正。
- `World/README.md` 第 2 节的运算表仍列 `WorldItemRules`，该类型实际位于
  `src/NSSLC/WorldItem/System/`，属本轮之前的既有失准，本轮未改。
- `Context/架构设计/ECS领域与基础设施架构边界.md` 的 `src/NSSLC/Component/WorldSession/`
  是文档自述的「逻辑归属示意，不是当前源码路径清单」，不随本轮换算。
- 工作区另有不属于本轮的未提交改动（`src/NSSLC.Network/` 三个文件、
  `src/NSSLC.Server/Program.cs`、`Test/Terraria.Components/Terraria.Npc.Ai/Program.cs`、
  `Context/progress.md`），以及并行任务正在进行的 `src/NSSLC/Share/AI/BasicFramework/`
  目录重排与其 README 改动；本轮未改写这些内容。
