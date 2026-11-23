# NpcSpawn 域改名为 EntitySpawn

文档 ID：CR-2026-11-23-ENTITY-SPAWN  
逻辑域：file-organization  
产物类型：decision、execution  
状态：completed  
日期：2026-11-23  
canonical 路径：`docs/cr/CR-2026-11-23-ENTITY-SPAWN.md`  
上游依据：[ECS 文件组织设计约束](../../Context/架构设计/ECS文件组织设计约束.md) §2.5、§3.1、§3.4、§6；
[组件命名设计约束](../../Context/架构设计/组件命名设计约束.md) §3、§4、§8.3；
[项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md) §5、§6  
前置：[CR-2026-11-23 NpcSpawn / Session / TileEntity 收进 World 语义子域](CR-2026-11-23-world-semantic-subdomains.md)

## 1. 授权与范围

用户要求：把 `NpcSpawn` 改为 `EntitySpawn`，同时修改该域下的文件命名和文档相关信息。

用户在三个分岔点选定：

| 分岔点 | 选定 |
| --- | --- |
| 命名空间 | 目录、文件、类型、命名空间四处一致，命名空间改为 `Terraria.EntitySpawn` |
| 附带身份 | 验证项目 `Terraria.NpcSpawn` 跟生产目录改名为 `Terraria.EntitySpawn`；注册键 `npc.spawn` 改为 `entity.spawn` |
| 文档范围 | 现状文档跟随改名，历史 CR / 审计 / 迁移台账保留旧名作为当时证据，另立本 CR |

本轮只做重命名：目录、12 个源码文件、类型名、命名空间、注册键、验证项目与现状文档。
刷怪规则、预算数值、区域算法、事实队列语义与系统调度顺序均不变。

## 2. 目录与文件对照

| 起点 | 目标 |
| --- | --- |
| `src/NSSLC/World/ComponentSystem/NpcSpawn/` | `src/NSSLC/World/ComponentSystem/EntitySpawn/` |
| `NpcSpawnSystem.cs` | `EntitySpawnSystem.cs` |
| `Component/NpcSpawnFactQueueComponent.cs` | `Component/EntitySpawnFactQueueComponent.cs` |
| `System/NpcSpawnArea.cs` | `System/EntitySpawnArea.cs` |
| `System/NpcSpawnAreaRules.cs` | `System/EntitySpawnAreaRules.cs` |
| `System/NpcSpawnBudget.cs` | `System/EntitySpawnBudget.cs` |
| `System/NpcSpawnBudgetRules.cs` | `System/EntitySpawnBudgetRules.cs` |
| `System/NpcSpawnCandidate.cs` | `System/EntitySpawnCandidate.cs` |
| `System/NpcSpawnedFact.cs` | `System/EntitySpawnedFact.cs` |
| `System/NpcSpawnEnvironment.cs` | `System/EntitySpawnEnvironment.cs` |
| `System/NpcSpawnFactQueue.cs` | `System/EntitySpawnFactQueue.cs` |
| `System/NpcSpawnRandom.cs` | `System/EntitySpawnRandom.cs` |
| `System/NpcSpawnTable.cs` | `System/EntitySpawnTable.cs` |

类型名与文件同步改名，维持 §2.5「一个核心公开类型对应一个同名 PascalCase 文件」：
`NpcSpawnSystem` → `EntitySpawnSystem`、`NpcSpawnedFact` → `EntitySpawnedFact`、
`NpcSpawnFactQueueComponent` → `EntitySpawnFactQueueComponent`，其余同构。

## 3. 命名空间与既有解耦事实

本轮是「变更目标明确要求同步调整」的情形，
[项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md) §5 允许把命名空间
作为独立的 API/依赖变更评估后一并调整，因此子域命名空间由 `Terraria.Npc` 改为
`Terraria.EntitySpawn`。

该调整改变了先前的既定解耦事实，随之收敛：

- `src/NSSLC/World/README.md` 的子域命名空间表，以及「命名空间不与目录同名」一句的主体
  收窄为 `Session/` 与 `TileEntity/`；
- `src/NSSLC/README.md` 的语义子域表；
- [Test 验证项目组织](../../Test/README.md) 第 3 条的「项目名跟生产目录」示例，改用仍然解耦的
  `TileEntity/` 与 `Share/ItemUse/`；
- [项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md) §5 的示例路径，
  改用仍然解耦的 `World/ComponentSystem/TileEntity/`。

`Terraria.Entity`（`Share/Entity/`）与 `Terraria.EntitySpawn` 是不同的命名空间，不会产生
类型名歧义；`EntitySpawnSystem` 通过文件级 `using Terraria.Npc;` 继续消费
`NpcBehaviorStateComponent`、`NpcAiRuntime` 等 NPC 域类型。

## 4. 引用更新

| 文件 | 处理 |
| --- | --- |
| `src/NSSLC.Server/Program.cs` | 新增 `using Terraria.EntitySpawn;`；刷怪表、环境、系统三处局部变量与类型引用改为 `entitySpawnTable` / `entitySpawnEnvironment` / `entitySpawnSystem`；事实队列创建与清理调用改名；注册键引用改 `EcsServerRegistrationKeys.EntitySpawn` |
| `src/NSSLC.Server/Registration/EcsServerRegistrationKeys.cs` | `NpcSpawn = "npc.spawn"` → `EntitySpawn = "entity.spawn"`；该键是服务器组合根身份，不是网络协议字段，无线上兼容负担 |
| `src/NSSLC.Network/NpcNetworkIdentitySystem.cs` | `using Terraria.Npc;` → `using Terraria.EntitySpawn;`；事实队列与事实类型引用改名 |
| `src/NSSLC/Terraria.Components.csproj` | 无需改动：`World/**/*.cs` 已覆盖新子域路径 |
| `Test/Terraria.Components/Terraria.NpcSpawn/` | 目录与 csproj 改名为 `Terraria.EntitySpawn`；`Program.cs` 类型引用、`using` 与测试定义 ID（`entity.spawn-test`）同步改名 |
| `src/NSSLC/World/README.md`、`src/NSSLC/README.md` | 目录树、命名空间表与链接文字跟随改名 |
| `Context/progress.md` | 基线条目改指 `EntitySpawn`，并新增本轮索引 |

不改动的相邻命名：

- `NpcSpawnUnlockFlags` / `UnlockedNpcSpawns`（`src/NSSLC.Infrastructure/WorldStorage/`）属于存档
  进度解锁标志，与本域无关；
- `ecs.npc.spawned` 是服务器诊断日志事件名，不是域身份；
- `NpcNetworkIdentitySystem`、`NpcBehaviorStateComponent` 等仍属 NPC 域，未随本域改名。

历史 CR、审计报告与迁移台账中的旧路径与旧名保留原样，按本 CR 第 2 节完成换算。

## 5. 验证

- `dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal`：退出码 0，
  0 警告，0 错误，产物 `Build/bin/Terraria.Components/Debug/net10.0/`。
- `dotnet msbuild src/NSSLC/Terraria.Components.csproj -getItem:Compile`：解析到 532 个 `.cs`
  编译项，其中 `EntitySpawn` 路径 12 个、`NpcSpawn` 路径 0 个，说明改名后的文件全部落在
  `World/**/*.cs` glob 内，`csproj` 无需改动。
- `dotnet build src/NSSLC.Server/NSSLC.Server.csproj --nologo --verbosity minimal`：退出码 0，
  0 错误；28 条警告全部来自被引用的 `src/NSSLC.Infrastructure/WorldGeneration`。以
  `-p:BuildProjectReferences=false` 单独编译本工程复核，为 0 警告、0 错误。
- `dotnet build Test/Terraria.Components/Terraria.EntitySpawn/Terraria.EntitySpawn.csproj`：
  退出码 0，0 警告，0 错误，产物 `Build/bin/Terraria.EntitySpawn/Debug/net10.0/`。
- `dotnet run --project Test/Terraria.Components/Terraria.EntitySpawn/Terraria.EntitySpawn.csproj
  --no-build --no-restore`：退出码 0，20 条检查全部 `PASS`、0 条 `FAIL`，末尾 1 行汇总
  `PASS`；刷怪预算、落点搜索、端到端 NPC 创建与网络槽位分配均与改名前的同名验证结果一致。
- 全仓搜索：`src/`、`Test/`、`Context/` 中除历史 CR 的标题与链接、以及与本域无关的
  `NpcSpawnUnlockFlags` 外，没有残留 `NpcSpawn` 生产标识符。

## 6. 回滚

```powershell
Move-Item src/NSSLC/World/ComponentSystem/EntitySpawn src/NSSLC/World/ComponentSystem/NpcSpawn
Move-Item Test/Terraria.Components/Terraria.EntitySpawn Test/Terraria.Components/Terraria.NpcSpawn
Rename-Item Test/Terraria.Components/Terraria.NpcSpawn/Terraria.EntitySpawn.csproj `
  Terraria.NpcSpawn.csproj
```

目录当前为未跟踪状态，因此用 `Move-Item` 而不是 `git mv`；若已提交，可改用等价的 `git mv`。
随后按第 2 节与第 4 节表格反向恢复文件名、类型名、命名空间、注册键与文档改动，并重建
`src/NSSLC.Server` 与验证项目。

## 7. 未覆盖与风险

- 域名为 `EntitySpawn`，但域内仍只包含 NPC 刷怪规则（`MaximumNpcs`、敌对阵营过滤、
  AI 装配）。这是命名先行、语义未泛化的遗留：等真正出现非 NPC 生成消费者时，再决定是扩展
  候选表还是把 NPC 专属规则拆回 `Npc/`，本轮不做预测性拆分。
- 注册键 `entity.spawn` 与既有 `npc.network.identity`、`npc.ai.intent` 不在同一命名族；若后续
  把 NPC 生成重新视为 NPC 域职责，该键需要与注册键族一起重新评估。
- 子域命名空间改变后，任何仓库外的消费方（例如未提交的分支或外部工具）若按
  `Terraria.Npc` 引用 `NpcSpawnSystem` 将无法编译；本仓库内已全部迁移。
- 工作区另有不属于本轮的未提交改动（`World/ComponentSystem/` 下 Session 与 TileEntity 的
  搬迁、`Share/ItemUse/`、`Share/Item`、`WorldItem/`、`Inventory/` 与相关文档），本轮未改写。
