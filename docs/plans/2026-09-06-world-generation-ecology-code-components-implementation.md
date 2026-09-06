# World Generation and Ecology Code Components Implementation Plan

## 需求

- 来源变更：`CR-2026-09-06-world-generation-ecology-code-implementation`
- 目标：把 `docs/design/2026-09-06-version4-world-generation-and-ecology-component-code-draft.md` 的 13 个 Component 适配并落到 `D:\TRbackup\NLTX\src`。
- 执行策略：`spec`
- 约束：不启动子代理；不修改 `dome`；保留现有未提交改动和旧 `*State` 类型。

## Phase 1：支持值类型

1. 不新增测试或 focused verifier，遵循用户明确的“不写测试”要求。
2. 新增 `WorldGenerationStage`、`GenerationPassDescriptor`、`PersistentEntityId`。

## Phase 2：世界生成核心组件

实现 `WorldDescriptorComponent`、`WorldGenerationRulesComponent`、`WorldGenerationLifecycleComponent`、`WorldGenerationPlanComponent`、`WorldGenerationPassStateComponent` 和 `WorldGenerationRandomStateComponent`。

## Phase 3：地形、生态、住房与交接组件

实现 `WorldTerrainStateComponent`、`BiomeEcologyStateComponent`、`EcologyScheduleComponent`、`HousingScanStateComponent`、`TownHousingAssignmentComponent`、`WorldLiquidHandoffComponent` 和 `ReadyTransitionComponent`。

## Phase 4：静态审计与构建

1. 对新增源码执行文件/命名空间/组件边界静态审计。
2. 检查活动 `dotnet.exe` / `csc.exe` 后，串行构建 `src/WorldSession/Terraria.WorldSession.csproj`。
3. 检查 `Build/bin/` 下的 WorldSession 产物，并记录命令、项目、退出码、warning/error 数量与输出路径。
4. 做只读文件清单与 git 状态审计；不提交 commit。

## 不变量覆盖

- 世界尺寸、生成坐标和层级顺序合法。
- Pass ID、版本、权重及禁用集合关系合法且防御性复制。
- Lifecycle 失败必须带失败类别；phase-level Ready 不等于完整 ready barrier。
- Active Pass、随机流、住房 assigned/homeless、Liquid stable 状态满足局部约束。
- Ready candidate 需要通过 Pass、Liquid、Housing、Persistence 和 publication revision 全部 facet。

## 未决边界

`WorldId`/`UniqueId`、`WorldEvil` canonical owner、住房稳定 identity、Liquid stable owner、Ready barrier owner 和 revision 类型体系继续标记为 `integration-review`；本次代码不将其误宣称为已完成迁移。

## 执行记录

- 按用户要求未新增或运行测试项目、focused verifier 或测试文件。
- 已完成 13 个 Component 与 3 个支持值类型的源码实现，目标目录为 `src/WorldSession/WorldGeneration/`。
- 最终命令：`& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments`，其中 `$dotnetArguments` 为 `build .\src\WorldSession\Terraria.WorldSession.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`。
- 最终结果：退出码 `0`；`0` warning；`0` error。
- 产物：`Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`。
