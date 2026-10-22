# NLTX 组件定义

当前源码基线保留 [L0/L1 计划](docs/plans/2026-10-08-arch-l1-basic-game-components-execution-plan.md)
第 3.1 节列出的 68 个组件：6 个 L0、62 个 L1。

活动项目为 [Terraria.Components.csproj](src/NSSLC/Terraria.Components.csproj)，目标框架为
`net10.0`。项目只编译 `src/NSSLC/Component/` 中的组件及其字段必需的枚举、值类型，
没有项目引用或 NuGet 包引用。

```powershell
dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal
```

编译输出位于 `Build/bin/Terraria.Components/Debug/net10.0/`。

组件保留数据字段、常量和初始化值。原来的 ECS 系统、运行时存储、应用协调、游戏宿主、
网络接线、兼容别名及对应验证程序已退出当前活动源码。旧公共 API、程序集、解决方案
项目组合和运行行为不提供兼容保证。

用户明确保护 `src/NSSLC.Infrastructure/` 整棵目录：其中源码、项目、参考材料和已有修改
完整保留，不纳入上述删除范围，也不参与当前组件库的编译。该目录的旧项目可能继续引用
已经删除的项目；这次保留文件并不代表其原有项目仍可编译或运行。

完整组件路径见 [组件定义清单](docs/reviews/audits/2026-10-09-components-only-inventory.md)，
范围、验证证据和恢复方式见
[组件定义清理记录](docs/cr/CR-2026-10-09-components-only-cleanup.md)。
