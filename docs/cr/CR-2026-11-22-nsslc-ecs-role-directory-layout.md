# NSSLC 领域内按 ECS 职责分目录：Component / Command / Query / System

文档 ID：CR-2026-11-22-NSSLC-ECS-ROLE-DIRECTORIES  
逻辑域：file-organization  
产物类型：decision、execution  
状态：completed  
日期：2026-11-22  
canonical 路径：`docs/cr/CR-2026-11-22-nsslc-ecs-role-directory-layout.md`  
上游依据：[ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md) §3.2、§3.3、§3.4、§6；
[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §5
前置：[CR-2026-11-22 NSSLC 领域目录扁平化](CR-2026-11-22-nsslc-domain-directory-flatten.md)

## 1. 授权与范围

用户要求：在 `src/NSSLC/` 下移除 `Model` 这类职责文件夹命名风格，忽略 `Communication`
（该域由并行任务迁出，本轮不触碰）；按 [ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md)
分为 `Component`、`Command`、`Query` 三类归档；系统主文件留在领域根目录、不做子目录
管理，系统的附属小文件统一放进 `System/`。用户同时确认了范围与口径：

- 范围：全 `NSSLC` 领域统一，不只含 `Model/` 的 6 个领域。
- `System/` 收纳所有非 `Component`/`Command`/`Query` 的附属类型。
- 第三方移植库（`AI/Goap`、`AI/Htn`）与示例集（`AI/Examples`）保持原样。
- 没有 `*System.cs` 的目录保持扁平。

本轮只移动文件：不改任何 `.cs` 内容、命名空间、公共 API 或运行时行为。`git diff --cached
--summary` 对 567 条重命名全部识别为 R100，0 增 0 删。

## 2. 目标结构与归档规则

```text
src/NSSLC/<领域>/
  <Xxx>System.cs          # 主系统文件，留领域根
  README.md
  Component/              # 挂在实体或世界上的持续状态
  Command/                # 请求、意图、请求/命令队列及其种类标识
  Query/                  # 只读查询接口与只读端口
  System/                 # 规则、求解器、运行时与存储服务、定义与编目、事实/结果/快照、枚举与值对象
```

判定口径：

| 判定 | 目录 |
| --- | --- |
| 文件名以 `System.cs` 或 `System.<片段>.cs` 结尾（含 `InventorySystem.Entities.cs`） | 领域根 |
| 以 `Component.cs` 结尾 | `Component/` |
| `*Request`、`*RequestKind`、`*RequestQueue`、`*Intent`、`*IntentKind`、`*Mutation`、`*Command*` | `Command/` |
| 只读查询接口与只读端口（`I*Reader`、`I*Grid`、`I*Query`、`I*Source`、`I*Resolver`、`I*SnapshotProvider`） | `Query/` |
| 其余（规则、求解器、Store/Runtime、定义/编目/注册表、事实/结果/快照、枚举与值对象） | `System/` |

语义子域视为独立领域，各自再分四类：`Attributes/Buff`、`Physics/CollisionGeometry`、
`Physics/ContactFact`、`Physics/Movement`、`Physics/SpatialSimulation`、`ItemUse/Player`。
父领域根只放父领域自己的 `*System.cs`。

保持扁平（无 `*System.cs`）：`Diagnostics`、`Item`、`Session`、`Physics`、
`Physics/Liquid`、`Physics/SpatialQuery`、`ItemUse/Content`、`Attributes/Contribution`。
保持来源结构：`AI/Examples`、`AI/Goap`、`AI/Htn`。

## 3. 移动清单

共移动 311 个 `.cs`，删除 12 个已空的 `Model/`、`Solver/`、`Contract/` 目录。

| 领域 | 根 | Component | Command | Query | System | 合计 |
| --- | --- | --- | --- | --- | --- | --- |
| `AI` | 2 | 2 | 0 | 1 | 14 | 19 |
| `Attributes` | 3 | 3 | 1 | 0 | 16 | 23 |
| `Attributes/Buff` | 6 | 9 | 0 | 0 | 12 | 27 |
| `Chat` | 1 | 3 | 4 | 0 | 11 | 19 |
| `Combat` | 10 | 7 | 6 | 0 | 15 | 38 |
| `Entity` | 2 | 5 | 1 | 2 | 19 | 29 |
| `Inventory` | 4 | 3 | 2 | 0 | 5 | 14 |
| `ItemUse/Player` | 1 | 3 | 3 | 0 | 3 | 10 |
| `Npc` | 1 | 1 | 2 | 0 | 11 | 15 |
| `NpcSpawn` | 1 | 1 | 0 | 0 | 10 | 12 |
| `Physics/CollisionGeometry` | 1 | 3 | 0 | 0 | 2 | 6 |
| `Physics/ContactFact` | 1 | 1 | 0 | 0 | 2 | 4 |
| `Physics/Movement` | 3 | 9 | 3 | 0 | 7 | 22 |
| `Physics/SpatialSimulation` | 2 | 1 | 0 | 0 | 6 | 9 |
| `Player` | 3 | 3 | 3 | 0 | 1 | 10 |
| `Projectile` | 9 | 15 | 6 | 0 | 19 | 49 |
| `TileEntity` | 1 | 2 | 3 | 0 | 9 | 15 |
| `World` | 2 | 5 | 0 | 3 | 21 | 31 |
| `WorldItem` | 2 | 1 | 2 | 2 | 7 | 14 |
| **合计** | **55** | **77** | **36** | **8** | **190** | **366** |

### 3.1 旧职责目录的去向

| 旧目录 | 去向 |
| --- | --- |
| `Chat/Model`、`ItemUse/Player/Model`、`NpcSpawn/Model`、`TileEntity/Model`、`World/Model`、`WorldItem/Model` | 按类型语义拆到 `Component/`、`Command/`、`System/`；目录删除 |
| `Chat/Solver`、`NpcSpawn/Solver`、`World/Solver`、`WorldItem/Solver` | `System/`；目录删除 |
| `World/Contract`、`WorldItem/Contract` | 只读端口进 `Query/`；`WorldInteractionWorld`（带 `TryBreakTile`/`TryPlaceTile` 的写端口）进 `System/`；目录删除 |

`World/Model/WorldMutation.cs` 初版按 `*Mutation` 判定放进 `World/Command/`，读正文后发现它是
"已提交、待投影的变更记录"，不是系统输入，随后移到 `World/System/`，删除空的
`World/Command/`。`World` 因此没有 `Command/` 目录：它的写入口是
`WorldInteractionRequestComponent`（组件），不是命令类型。
| `ItemUse/Content` | 无系统，保持扁平不动 |

### 3.2 目录与命名空间

所有文件仍使用原命名空间：`src/NSSLC/World/**` 仍是 `namespace Terraria.World`，
`src/NSSLC/Physics/Movement/**` 仍是 `Terraria.Movement`，`NpcSpawn/` 仍是
`Terraria.Npc`，`ItemUse/` 仍是 `Terraria.Player`。目录移动没有改变命名空间，符合
[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §5。

## 4. 结构调整理由

- 原 `Model/`、`Solver/`、`Contract/` 是**职责词收容目录**：`Model/` 同时装着组件、
  请求、结果、事实、枚举和值对象，读目录无法回答"这个类型在 ECS 里扮演什么角色"。
  按 `Component`/`Command`/`Query`/`System` 归位后，角色从路径可读。
- 主系统文件留在领域根、附属文件进 `System/`，让"域里有哪些系统"在一层目录内可数；
  附属规则不再与系统文件混排。
- 领域仍是最外层归属，`Component/` 等只出现在领域内部，没有形成
  `<代码根>/Components/` 那种横向切散；`AI/Goap`、`AI/Htn` 等移植库保持来源结构，
  便于比对上游与重新导入。
- 无系统的小目录保持扁平，避免为模板完整性造出只有一个文件的职责目录。

## 5. 文档处理

- `Context/架构约束/ECS文件组织设计约束.md`：§3.4 由"不强制规定统一目录名称"改为固定的
  四类职责目录规则；§2.1 增加"禁止的是代码根层横向切分"的澄清；§3.2、§3.3 的示例改为
  新布局；§6 验收清单新增两条。
- `src/NSSLC/README.md`：新增"领域内目录"一节，写明四类目录、扁平目录清单和移植库例外。
- `docs/architecture/2026-10-23-nsslc-arch-ecs-organizer.md`：`EcsServer*` 的路径更新为
  `src/NSSLC/Entity/System/`、`src/NSSLC/Entity/Command/`。
- `Context/` 与 `docs/architecture/` 之外的历史审计、计划、迁移台账中的旧路径作为当时
  证据保留，不逐条改写；换算方式见 §2 与 §3.1。

## 6. 验证

- `dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal`：
  退出码 0，0 警告，0 错误（移动后、修复三处 `using` 之后各复验一次）。
- `git diff --cached -M --summary -- src/NSSLC`：567 条 rename 全部 100% 相似，0 增 0 删；
  其中 567 条含前置扁平化 CR 的 256 条纯重命名。**唯一的内容改动**是下面三处必需修复。
- 内容改动 3 处：并行任务把 `SessionEpoch` 从 `Terraria.Communication` 移到
  `Terraria.Session` 后，这三个文件仍 `using Terraria.Communication;`，该命名空间已不存在。
  本轮把 `Inventory/Command/InventoryPickupRequest.cs`、
  `TileEntity/Command/WorldTileEntityRequest.cs`、`TileEntity/System/WorldTileEntityResult.cs`
  的 `using Terraria.Communication;` 改为 `using Terraria.Session;`，其余内容不变。
- 14 个验证项目里，9 个与 `NSSLC.Network` 无关的项目逐个构建并运行通过：
  `Terraria.Attributes`、`Terraria.Chat`、`Terraria.Combat.ProtectionWindow`、
  `Terraria.Entity.Definition`、`Terraria.Entity.Scheduling`、`Terraria.Inventory`、
  `Terraria.ItemUse`、`Terraria.Projectile`、`Terraria.TileEntity`。
- 其余 5 个（`Terraria.Network.OutboundOrdering`、`Terraria.Server.TickCadence`、
  `Terraria.Npc.Ai`、`Terraria.NpcSpawn`、`Terraria.WorldItem`）构建失败，报错位置全部在
  `src/NSSLC.Network/**`（`InboundRawFrame`、`ConnectionClosed` 缺失，
  `ICommunicationOutboundPort` 新增 `BeginTickBatch`/`SealTickBatch` 未实现），是本轮之外的
  并行重构；这 5 个项目在并行重构收口前无法作为本轮的验收证据。
- `.cs` 计数：移动前受影响 19 个目录共 366 个 `.cs`（311 个非系统文件 + 55 个
  `*System.cs`），移动后合计仍为 366，无遗漏、无重复。

## 7. 回滚

本轮无内容改动，回滚只需把路径改回：

```powershell
# 按 §2 的判定表反向移动：Component/、Command/、Query/、System/ 下的文件回到领域根或原
# Model/、Solver/、Contract/ 目录，然后删除空的四类目录
git checkout -- Context/架构约束/ECS文件组织设计约束.md src/NSSLC/README.md `
  docs/architecture/2026-10-23-nsslc-arch-ecs-organizer.md
```

不使用 `git checkout HEAD -- src/NSSLC`：工作区存在不属于本轮的并行改动（`Communication`
迁出为 `Session` 与 `src/NSSLC.Network/Communication/**`、`Terraria.Components.csproj`
编译项），整体还原会一并丢弃它们。

## 8. 未覆盖与风险

- `Communication` 域按用户要求不纳入本轮：该目录在 22:53–22:55 已被并行任务迁出
  `src/NSSLC/`，`Terraria.Components.csproj` 的 `Compile Include` 也由该任务改为
  `Session/**`。`src/NSSLC/README.md` 的领域表仍留 `Communication` 行，属于并行任务的
  收尾范围，本轮未改。
- `AI/Goap`、`AI/Htn`、`AI/Examples` 未按四类拆分，是显式例外；若日后要统一，需要先
  决定移植库的重新导入策略。
- 扁平目录（`Diagnostics`、`Item`、`Session`、`Physics/Liquid`、`Physics/SpatialQuery`、
  `ItemUse/Content`、`Attributes/Contribution`）在出现第一个 `*System.cs` 时需要补建四类
  目录并迁移，本轮不预建。
- 目录深度增加一层，旧文档中的相对链接若指向被移动文件需按 §3.1 换算；`Context/` 与
  `docs/architecture/` 之外的文档未批量改写。
- 并行任务仍在改写 `src/NSSLC.Network`、`src/NSSLC.Server` 与测试项目；本轮构建与验证
  结果只对 2026-11-22 23:0x 的工作区状态有效，若并行改动继续，需要重跑上述验证。
- 并行任务在 23:06–23:08 把本轮移动过的三个文件（见 §6）压成单行，导致 `///` 注释把类型
  声明整行吞掉、`InventoryPickupRequest` 等类型消失并产生 20 条 CS0246。本轮用索引版本
  恢复行结构后只保留它的 `using` 语义修复，重新编译通过。若并行任务再次压平这些文件，
  需要再次恢复行结构。
