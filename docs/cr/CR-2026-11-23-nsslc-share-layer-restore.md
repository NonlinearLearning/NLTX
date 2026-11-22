# 恢复 NSSLC 的 Share 收容层：七个共享能力域收进 `src/NSSLC/Share/`

文档 ID：CR-2026-11-23-NSSLC-SHARE-LAYER-RESTORE  
逻辑域：file-organization  
产物类型：decision、execution  
状态：completed  
日期：2026-11-23  
canonical 路径：`docs/cr/CR-2026-11-23-nsslc-share-layer-restore.md`  
上游依据：[项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md) §3.1、§4、§5、§6；
[ECS 文件组织设计约束](../../Context/架构设计/ECS文件组织设计约束.md) §2.1、§2.3
前置：[CR-2026-11-22 NSSLC 领域目录扁平化](CR-2026-11-22-nsslc-domain-directory-flatten.md)、
[CR-2026-11-22 NSSLC 领域内按 ECS 职责分目录](CR-2026-11-22-nsslc-ecs-role-directory-layout.md)

## 1. 授权与范围

用户要求：检查 git 历史，把「对应 `Share` 下的文件夹」移动到
`D:\TRbackup\NLTX\src\NSSLC\Share`。

按 git 历史还原的清单来自 `5c67f3e^` 的 `src/NSSLC/Components/Share/`，共七个共享能力域：

| 原 Share 子目录 | 扁平化后位置（本轮起点） | 本轮目标路径 |
| --- | --- | --- |
| `Share/AI/` | `src/NSSLC/AI/` | `src/NSSLC/Share/AI/` |
| `Share/Attributes/` | `src/NSSLC/Attributes/` | `src/NSSLC/Share/Attributes/` |
| `Share/Combat/` | `src/NSSLC/Combat/` | `src/NSSLC/Share/Combat/` |
| `Share/Entity/` | `src/NSSLC/Entity/` | `src/NSSLC/Share/Entity/` |
| `Share/Inventory/` | `src/NSSLC/Inventory/` | `src/NSSLC/Share/Inventory/` |
| `Share/Item/` | `src/NSSLC/Item/` | `src/NSSLC/Share/Item/` |
| `Share/Physics/` | `src/NSSLC/Physics/` | `src/NSSLC/Share/Physics/` |

不在范围的目录：`Chat`、`ItemUse`、`Npc`、`NpcSpawn`、`Player`、`Projectile`、`Session`、
`TileEntity`、`World`、`WorldItem`。这些域在 git 历史中从未位于 `Share/` 下，而是
`Components/<领域>/`；`Communication` 已在 2026-11-22 迁入 `src/NSSLC.Network/`，
`Diagnostics` 已迁入 `src/NSSLC.Infrastructure/`，两者都不回到组件库。

`Share/README.md` 不回迁：它已在扁平化轮次改写为项目级入口 `src/NSSLC/README.md`，
继续承担整个组件项目的领域导航。本轮只移动文件夹，不新增 `Share/README.md`。

本轮只移动目录与更新引用，不改任何 `.cs` 文件内容、命名空间、公共 API 或运行时行为。

## 2. 目标结构

```text
src/NSSLC/
  Share/
    AI/            Attributes/    Combat/        Entity/
    Inventory/     Item/          Physics/
  Chat/            ItemUse/       Npc/           NpcSpawn/
  Player/          Projectile/    Session/       TileEntity/
  World/           WorldItem/
  Terraria.Components.csproj
  README.md
```

共享能力域收在 `Share/`，端别领域直挂项目根。七个域内部结构保持不变：`Component/`、
`Command/`、`Query/`、`System/` 四类职责目录与领域根主系统文件都随目录整体移动，不对
领域内部再做重排。

## 3. 与既有约束的关系

[项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md) §3.1 规定共享代码放在
`Share` 或等价的共享项目边界内，§4 给出 `src/Shared/<领域>/` 的端别扩展形态。本轮把七个
端别无关的能力域收进 `Share/`，与该条文一致，并恢复了扁平化轮次删掉的那层边界。

[ECS 文件组织设计约束](../../Context/架构设计/ECS文件组织设计约束.md) §2.1 的示例树描述的是
`<代码根>/` 下的领域第一层，不含横向收容层。本轮引入的 `Share/` 不是横向职责切分（不是
`Components/`、`Systems/`、`Queries/`、`Events/` 这类按文件职责划分的目录）：它下面仍然是
领域目录，领域优先原则未被削弱。§2.3「按需引入子目录」同样满足——`Share/` 下有七个真实
领域和 376 个文件，不是为模板完整性预建的空壳。

因此本轮不修改上述两份约束文档的正文，只通过本 CR 记录这层边界的适用范围与撤销条件。

## 4. 引用更新

| 文件 | 处理 |
| --- | --- |
| `src/NSSLC/Terraria.Components.csproj` | 七个域改为 `Share/<领域>/**/*.cs` 显式编译项；其余领域保持原路径，仍不使用宽泛 `**/*.cs`，继续排除 `src/NSSLC/Build/` |
| `src/NSSLC/README.md` | 目录表拆为 `Share/` 下共享能力域与项目根领域两张表；子域路径、`Goap`/`Htn`/`Examples` 说明与正文域路径同步加 `Share/` 前缀；补注通信域已迁至 `NSSLC.Network/Communication`（原 `Communication/` 链接在扁平化后即已失效，本轮一并修正） |
| `README.md` | 领域清单按 `Share/` 与项目根两层重写 |
| `src/NSSLC/Share/Attributes/README.md` | 目录深度加一，`../../../docs/` 改为 `../../../../docs/` |
| `docs/architecture/2026-10-23-attribute-system-design.md` | 实现位置加 `Share/` 前缀 |
| `docs/architecture/2026-10-23-gravity-attribute-system-design.md` | 实现位置加 `Share/` 前缀 |
| `docs/architecture/2026-10-23-spatial-simulation-component-system.md` | 共享代码位置改指 `src/NSSLC/Share/` |
| `docs/architecture/2026-10-23-nsslc-arch-ecs-organizer.md` | 五处 `EcsServer` 实现文件路径加 `Share/` 前缀 |

历史审计、计划、旧 CR 与迁移台账中的旧路径作为当时证据保留，不逐条改写；旧路径按下表换算：

| 旧前缀 | 新前缀 |
| --- | --- |
| `src/NSSLC/AI/` | `src/NSSLC/Share/AI/` |
| `src/NSSLC/Attributes/` | `src/NSSLC/Share/Attributes/` |
| `src/NSSLC/Combat/` | `src/NSSLC/Share/Combat/` |
| `src/NSSLC/Entity/` | `src/NSSLC/Share/Entity/` |
| `src/NSSLC/Inventory/` | `src/NSSLC/Share/Inventory/` |
| `src/NSSLC/Item/` | `src/NSSLC/Share/Item/` |
| `src/NSSLC/Physics/` | `src/NSSLC/Share/Physics/` |

`Test/` 下 14 个验证项目的 csproj 只引用 `src/NSSLC/Terraria.Components.csproj`，不含领域
路径，无需改动。

## 5. 验证

- 移动：`git mv` 逐域执行，`git status` 记录 375 条纯重命名（R100）加 1 条「重命名且内容
  已修改」（`Physics/Liquid/LiquidSimulation.cs` 的工作区既有未提交改动随目录一起移动，
  未被改写）。合计 376 个文件。
- `dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal`：退出码 0，
  0 警告，0 错误，产物 `Build/bin/Terraria.Components/Debug/net10.0/`。
- `dotnet msbuild src/NSSLC/Terraria.Components.csproj -getItem:Compile` 解析到 536 个 `.cs`
  编译项；磁盘上（排除 `src/NSSLC/Build/`）同为 536 个 `.cs`，未编译文件 0 个，说明移动后
  没有文件落在编译 glob 之外。536 与扁平化 CR 记录的 566 之差来自本轮之前已发生的文件
  变化，不是本次移动造成。
- `dotnet build src/NSSLC.Server/NSSLC.Server.csproj --nologo --verbosity minimal`：退出码 0，
  0 错误，28 条警告全部来自未修改的 `src/NSSLC.Infrastructure/WorldGeneration`。
- 14 个验证项目逐个 `dotnet build` + `dotnet run --no-build --no-restore`：14/14 构建退出码 0、
  运行退出码 0，共 268 条 `PASS`、0 条 `FAIL`，汇总在
  `Build/diagnostics/share-layer-restore-20261123/test-summary.csv`。
- 链接检查：对本轮改动的 6 个 `.md` 与 `src/NSSLC/Share/` 下全部 `.md` 做相对链接解析，断链 0。

## 6. 回滚

```powershell
foreach ($d in 'AI','Attributes','Combat','Entity','Inventory','Item','Physics') {
  git mv "src/NSSLC/Share/$d" "src/NSSLC/$d"
}
```

随后把 `Terraria.Components.csproj` 的七个编译项改回不带 `Share/` 前缀的路径，并按第 4 节
表格反向恢复文档改动。`src/NSSLC/Share/` 变空后删除即可。

## 7. 未覆盖与风险

- 目录移动未改变命名空间：`src/NSSLC/Share/Physics/Movement/` 仍是 `Terraria.Movement`，
  `Share/Physics/Liquid/` 仍是 `Terraria.Liquid`，`Share/NpcSpawn` 一类目录与命名空间不一致
  的既有事实全部保留，不在本轮顺手修正。
- `src/NSSLC/Build/` 仍是既存的生成物残留，本轮未清理；显式 `Compile Include` 继续将其排除。
- 工作区另有不属于本轮的未提交修改（`src/NSSLC.Network/` 四个文件、
  `src/NSSLC.Server/Program.cs`、`src/NSSLC/Share/Physics/Liquid/LiquidSimulation.cs`、
  未跟踪的 `docs/未实现的优化思路/`），本轮未改写其内容。服务器构建验证表明它们与移动后的
  源码可一起编译，但其行为验证不属于本 CR。
