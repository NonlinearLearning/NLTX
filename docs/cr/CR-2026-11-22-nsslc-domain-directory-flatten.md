# NSSLC 领域目录扁平化：移除 Components 与 Share 层

文档 ID：CR-2026-11-22-NSSLC-DOMAIN-FLATTEN  
逻辑域：file-organization  
产物类型：decision、execution  
状态：completed  
日期：2026-11-22  
canonical 路径：`docs/cr/CR-2026-11-22-nsslc-domain-directory-flatten.md`  
上游依据：[ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md) §2.1、§2.2、§4；
[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.1、§5

## 1. 授权与范围

用户要求把 `Share` 和 `Chat` 等域移到 `NSSLC` 下，并选择「全部领域都上移，删除
`Components` 层」的目标结构。本轮据此把 `src/NSSLC/Components/` 下的全部领域目录提升
到 `src/NSSLC/` 根层，并把 `Components/Share/` 拆成七个能力域；`Components/` 与
`Share/` 两个横向收容层不再存在。

本轮只移动目录和更新引用，不改任何 `.cs` 文件内容、命名空间、公共 API 或运行时行为。
`src/NSSLC.Infrastructure/` 未触碰。工作区中另有并发的
`src/NSSLC.Server/Program.cs` / `ServerTickPacing.cs` 修改（不属于本轮），本轮未触碰。

## 2. 目标结构

```text
src/NSSLC/
  AI/            Attributes/    Chat/          Combat/
  Communication/ Diagnostics/   Entity/        Inventory/
  Item/          ItemUse/       Npc/           NpcSpawn/
  Physics/       Player/        Projectile/    TileEntity/
  World/         WorldItem/
  Terraria.Components.csproj
  README.md
```

共 18 个领域目录。`src/NSSLC/Components/` 和 `src/NSSLC/Components/Share/` 已删除。

## 3. 移动清单

### 3.1 原 Components 下的一级领域

| 原路径 | 新路径 |
| --- | --- |
| `src/NSSLC/Components/Chat/` | `src/NSSLC/Chat/` |
| `src/NSSLC/Components/Communication/` | `src/NSSLC/Communication/` |
| `src/NSSLC/Components/Diagnostics/` | `src/NSSLC/Diagnostics/` |
| `src/NSSLC/Components/ItemUse/` | `src/NSSLC/ItemUse/` |
| `src/NSSLC/Components/Npc/` | `src/NSSLC/Npc/` |
| `src/NSSLC/Components/NpcSpawn/` | `src/NSSLC/NpcSpawn/` |
| `src/NSSLC/Components/Player/` | `src/NSSLC/Player/` |
| `src/NSSLC/Components/Projectile/` | `src/NSSLC/Projectile/` |
| `src/NSSLC/Components/TileEntity/` | `src/NSSLC/TileEntity/` |
| `src/NSSLC/Components/World/` | `src/NSSLC/World/` |
| `src/NSSLC/Components/WorldItem/` | `src/NSSLC/WorldItem/` |

### 3.2 Share 拆分出的能力域

| 原路径 | 新路径 |
| --- | --- |
| `src/NSSLC/Components/Share/AI/` | `src/NSSLC/AI/` |
| `src/NSSLC/Components/Share/Attributes/` | `src/NSSLC/Attributes/` |
| `src/NSSLC/Components/Share/Combat/` | `src/NSSLC/Combat/` |
| `src/NSSLC/Components/Share/Entity/` | `src/NSSLC/Entity/` |
| `src/NSSLC/Components/Share/Inventory/` | `src/NSSLC/Inventory/` |
| `src/NSSLC/Components/Share/Item/` | `src/NSSLC/Item/` |
| `src/NSSLC/Components/Share/Physics/` | `src/NSSLC/Physics/` |
| `src/NSSLC/Components/Share/README.md` | `src/NSSLC/README.md`（内容改写为项目领域入口） |
| `src/NSSLC/Components/Share/`（空壳） | 删除 |
| `src/NSSLC/Components/`（空壳） | 删除 |

共移动 592 个文件；其中 566 个 `.cs` 文件在移动前后路径集合一一对应。`git diff --cached`
对这 592 个文件全部识别为纯重命名（R100，0 增 0 删）；随后原 `Share/README.md` 改写成
`src/NSSLC/README.md`，内容修改不计入上述纯重命名。工作区另有不属于本轮的
`src/NSSLC.Server/Program.cs` 修改。

## 4. 结构调整理由

- ECS 文件组织约束 §2.1 要求「领域优先」，不推荐长期保留 `<代码根>/Components/` 这种
  横向技术层。上移后，`src/NSSLC/` 的第一层直接表达游戏领域。
- `Share/` 不是单一领域，而是七个能力域的收容层。ECS 文件组织约束 §4 要求共享组件按
  能力归属，禁止把 `Shared/Components/` 当作暂时无法归属的收容处；因此本轮将七个域
  提升为一级领域，`Share/` 不保留。
- 小领域保持扁平，未新增空的 `Components/`、`Systems/`、`Queries/` 子目录；现有
  `Attributes/Buff`、`Physics/Movement` 等稳定子域保持原状，避免把目录移动扩大成领域
  内部重构。

## 5. 引用与文档处理

- `src/NSSLC/Terraria.Components.csproj`：编译项从 `Components/**/*.cs` 改为 18 个领域
  的显式 `Compile Include`，不使用宽泛的 `**/*.cs`，避免把 `src/NSSLC/Build/` 下既存
  生成文件带入编译。
- `README.md`：更新活动项目说明、领域清单和构建输出路径。
- `src/NSSLC/README.md`：由原 `Share/README.md` 改写为项目级领域入口，修正原表中
  `Buff/`、`Movement/`、`Liquid/` 的失效链接，并说明 `Attributes/Buff`、
  `Physics/Movement`、`Physics/Liquid` 等稳定子域。
- `src/NSSLC/Attributes/README.md`、`src/NSSLC/Projectile/README.md`：按新目录深度修正
  指向 `docs/` 的相对链接。
- `Test/README.md`、`Context/项目约束/ECS系统输入边界约束.md`、
  `Context/架构约束/项目文件组织设计约束.md`：更新当前规则中的旧 `Components/` 路径。
- `docs/architecture/` 中仍描述当前实现的四份设计文档同步更新路径；历史审计、计划、
  旧 CR 中的路径作为当时证据保留，不逐条改写。旧路径按下表换算：

| 旧前缀 | 新前缀 |
| --- | --- |
| `src/NSSLC/Components/Share/AI/` | `src/NSSLC/AI/` |
| `src/NSSLC/Components/Share/Attributes/` | `src/NSSLC/Attributes/` |
| `src/NSSLC/Components/Share/Combat/` | `src/NSSLC/Combat/` |
| `src/NSSLC/Components/Share/Entity/` | `src/NSSLC/Entity/` |
| `src/NSSLC/Components/Share/Inventory/` | `src/NSSLC/Inventory/` |
| `src/NSSLC/Components/Share/Item/` | `src/NSSLC/Item/` |
| `src/NSSLC/Components/Share/Physics/` | `src/NSSLC/Physics/` |
| `src/NSSLC/Components/<领域>/` | `src/NSSLC/<领域>/` |

## 6. 验证

- `dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal`：
  退出码 0，0 警告，0 错误。
- `dotnet build src/NSSLC.Network/NSSLC.Network.csproj`：退出码 0，0 警告，0 错误。
- `dotnet build src/NSSLC.Server/NSSLC.Server.csproj`：首次构建因运行中的
  `NSSLC.Server`（PID 16916，`Build/bin/NSSLC.Server/... --manual-ai`）锁定输出 DLL
  而失败，错误均为 MSB3027/MSB3021 文件复制锁，不是编译错误。改用隔离输出根重跑：
  `dotnet build src/NSSLC.Server/NSSLC.Server.csproj -p:RepositoryBuildRoot=D:\TRbackup\NLTX\Build\verify\`，
  退出码 0，0 错误，19 条警告全部来自未修改的
  `src/NSSLC.Infrastructure/WorldGeneration`。随后工作区出现并发 `ServerTickPacing`
  修改，使用同一隔离输出根做最终重建：退出码 0，0 警告，0 错误（增量构建，复用前次
  WorldGeneration 产物）。
- 13 个验证项目逐个构建并运行：13/13 构建退出码 0、运行退出码 0，共 256 条 `PASS`、
  0 条 `FAIL`。日志与汇总位于
  `Build/diagnostics/domain-flatten-20261122/`（`test-summary.csv`）。测试在每个项目
  构建后用 `--no-build --no-restore` 运行，未加 `-t:Rebuild`。
- 移动前后 `.cs` 路径集合核对：HEAD 中 `src/NSSLC/Components/**` 为 566 个 `.cs`，
  当前 18 个领域合计 566 个 `.cs`，逐一映射后集合完全一致。
- `dotnet msbuild src/NSSLC/Terraria.Components.csproj -getItem:Compile` 解析到 566 个
  Compile 项，与移动前一致；`git diff --check` 退出码 0。

## 7. 回滚

源代码目录可用 HEAD `720a723f746dbc3d161f2f8c52e2ad7aaf418db6` 整体恢复：

```powershell
git checkout HEAD -- src/NSSLC
```

该命令会恢复 `src/NSSLC/Components/` 原树及 `Terraria.Components.csproj`，同时丢弃本轮
对 `src/NSSLC/README.md` 等文件的修改。文档改动按本 CR 的移动表反向移动即可；历史文档
未被改写，无需回滚。

## 8. 未覆盖与风险

- 运行中的 `NSSLC.Server`（PID 16916）未重启，仍使用启动时加载的旧程序集。本轮未改
  行为，若要用新构建验证，需要由用户重启服务器。
- `src/NSSLC/Build/` 是既存的生成物残留，未在本轮清理；显式 `Compile Include` 已将其
  排除。
- 历史审计、计划和旧 CR 中的旧路径保留原样，仅通过本 CR 的换算表定位；这是「历史证据
  不改写」的既定处理方式。
- 工作区存在不属于本轮的并发修改：`src/NSSLC.Server/Program.cs` 与未跟踪的
  `src/NSSLC.Server/ServerTickPacing.cs`（2026-11-22 21:57-21:58 写入）。本轮未触碰这些
  文件；最终隔离构建已验证两者与移动后的源码可一起编译（0 错误），但其行为验证不属于
  本 CR。
