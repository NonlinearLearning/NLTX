# 68 个 L0/L1 组件定义清理记录

文档 ID：CR-2026-10-09-COMPONENTS-ONLY-CLEANUP  
逻辑域：change-control  
产物类型：decision、execution  
状态：completed-components-only  
日期：2026-10-09  
canonical 路径：`docs/cr/CR-2026-10-09-components-only-cleanup.md`

## 1. 授权与范围覆盖

用户要求以 [L0/L1 计划](../plans/2026-10-08-arch-l1-basic-game-components-execution-plan.md)
第 3.1 节的 6 个 L0 和 62 个 L1 类型为唯一组件清单，移除清单外组件、删除全部系统、
破坏全部兼容性、只保留组件定义。随后用户追加：保护 `NSSLC.Infrastructure` 不受删除影响。

这两条请求构成最终执行边界：

- `src/NSSLC.Infrastructure/` 整棵目录不删除、不移动、不修改，包含其中的系统、组件、
  参考代码及既有未提交修改。
- 保护范围外只保留 68 个组件定义，以及字段必需的 38 个枚举和值类型。
- 移除保护范围外的旧 ECS 运行时、全部 System、Query/Command 实现、关系存储、地图存储、
  内容目录、应用协调、平台接线、游戏工具宿主、代码生成器及旧验证项目。
- 历史文档、既有 `Build/diagnostics/` 证据、世界文件 `src/World/` 和任务开始前的
  `outputs/` 保留。没有执行 `git reset`、`git clean` 或目录递归删除。

本次要求覆盖原计划中“保留生产消费者、宿主、兼容投影和低等级混合模型”的门禁。
原计划第 3.2、4–10 节作为历史迁移设计留存，不再描述当前活动源码或本次验收要求。
这次完成的是组件定义清理，不是 Arch 运行迁移或玩法等价验收。

## 2. 最终代码与项目

活动项目为 `src/NSSLC/Terraria.Components.csproj`，显式编译 `Component/**/*.cs`。
`NSSLC.NonCommunicationSimulation.sln` 只包含此项目。项目目标为 `net10.0`，没有
`ProjectReference`、`PackageReference`，不会编译或引用保护目录。

组件字段与数据含义保留；组件内的状态推进、伤害结算、恢复、确认、提交、Tick、查询、
兼容构造函数和计算属性全部移除。数据容器改为公开字段；值组件需要字段初始化器时
只保留 C# 所要求的空无参构造函数。record 的隐式成员由编译器生成。

`EntityReference` 只保留 `EntityUuid` 与领域 scope，不再携带旧 ECS 的 `EntityRuntimeId`。
物品预留的旧 `RuntimeEntityId` 包装改为直接保存 `EntityReference`。保留的身份、槽位、
定义 ID、库存引用、免疫窗口和状态枚举都是数据定义，没有注册器、索引或兼容解析行为。
`NpcLegacySlotComponent` 是白名单内的组件，因此它的槽位与 generation 数据保留。

小目录按领域扁平化：共享 Entity 组件从 `Share/Entity/Components/` 移到 `Share/Entity/`；
玩家移动组件从 `Player/Movement/` 移到 `Player/`；NPC 网络意图从 `Npc/Network/` 移到
`Npc/`；世界加载状态从 `WorldSession/WorldGeneration/` 移到 `WorldSession/`。
组件命名与命名空间保留；配套定义从原 `System/` 目录归入所属领域。
逐类型的源路径、目标路径见 [组件定义清单](../reviews/audits/2026-10-09-components-only-inventory.md)。

## 3. 清理与保护结果

| 指标 | 执行前 | 执行后 |
| --- | ---: | ---: |
| 保护范围外的组件源码 | 197 | 68 |
| 保护范围外的 `*System.cs` | 129 | 0 |
| 活动组件库的定义源码 | — | 106：68 个组件、38 个配套定义 |
| 活动组件库项目引用 / 包引用 | — | 0 / 0 |
| 保护目录文件 | 2,232 | 2,232，路径与 SHA-256 完全一致 |

本次按逐文件清单实际移除 2,146 个文件。这个数字以任务开始时的工作树为基线，
不包含此前已经删除的文件；包含退出的源码、项目文件及其专属运行说明、脚本和配置。
全部删除路径在执行前解析为仓库内绝对路径，并再次拒绝保护目录、世界文件和范围外路径。

副作用：修改仓库源码及活动项目配置，不执行游戏模拟、网络发送、世界加载或存档写入。
没有保留旧运行入口或兼容外壳。保护目录保留的旧项目依赖可以悬空；未对这些项目进行
修复、构建或运行，也不声称原宿主仍可用。

## 4. 验证

构建命令：

```powershell
dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal
```

- 项目：`src/NSSLC/Terraria.Components.csproj`。
- 退出码：0；警告：0；错误：0。
- 产物：`Build/bin/Terraria.Components/Debug/net10.0/Terraria.Components.dll`，已确认存在。
- MSBuild 解析的 Compile 项为 106 个定义文件，且与源码清单完全相同。
- 输出、中间文件、生成文件和包路径分别在 `Build/bin/`、`Build/obj/`、
  `Build/generated/` 和 `Build/packages/`。
- 静态检查逐一核对 68 个组件与原计划第 3.1 节一致；清理范围外没有多余组件，
  清理范围内没有 System、行为方法、计算属性或旧 ECS 运行时引用。
- 保护目录全部文件的路径和 SHA-256 在执行前后完全一致。

证据位于 `Build/diagnostics/ComponentsOnly/2026-10-09/`：
`verification.json`、`cleanup-result.json`、`build.log`、`build-command.json` 和
`msbuild-properties.json`。这些是可再生或本地诊断产物，不提交为源码。

原验证程序依赖已删除的系统与宿主，本次随运行代码退出。没有运行旧玩法测试，
也没有把组件库构建成功描述为旧运行行为保持一致。

## 5. 恢复

删除前已把本次清理范围的 2,207 个文件按原路径保存到：

`Build/diagnostics/ComponentsOnly/2026-10-09/source-before-cleanup.zip`

该快照包括任务开始时的未提交修改和未跟踪代码；不覆盖保护目录、世界文件或历史证据。
SHA-256：`aaa86290bbda6824650fe59375448fd94d8a0cc3bb407af5b080bf2b127b8308`。

需要恢复时先核对当前改动，再按清单恢复快照文件、原项目和原解决方案，并删除本次新增的
组件库项目及新归位定义。快照恢复的是本次执行前的工作树内容，不等同于 HEAD，也不会
恢复在本次任务开始前就已删除的文件。
