# Arch 迁移长线 T1：当前基线与 Arch 2.1.0 原生 API 探针

文档 ID：DOC-2026-10-08-ARCH-TRACK-T1  
状态：active；这是执行合同，不是已完成报告  
对应批次：A0–A1  
协调文档：[并行长线协调](2026-10-08-arch-parallel-execution-coordination.md)  
主计划：[自定义 ECS 转向 Arch 执行计划](2026-10-07-custom-ecs-to-arch-execution-plan.md)

范围增强：[Arch 原生 API 覆盖审计](../reviews/audits/2026-10-08-arch-native-api-coverage-audit.md)，尤其 N1/N3 与能力采用边界。新增内容是待执行合同，不代表旧探针已覆盖。

编译-only 限制：本线不运行探针、测试、benchmark、冒烟或其他运行时验证；只编译受影响
项目并记录编译结果。API/关系/Events 行为仍属于未验证风险，不得从编译成功推导。

## 1. 目标

建立所有后续长线都能复核的当前源码基线，并在实际仓库 SDK、目标框架和输出约束下验证
Arch 2.1.0 的关键 API。T1 不负责把生产 ECS 切换到 Arch；它负责把“Arch 有什么行为、
NLTX 需要自己补什么协议、其他长线可安全依赖什么”变成可执行证据。

## 2. 前置读取与边界

先读取根 `AGENTS.md`、`Context/progress.md`、开发协作约束、构建约束、ECS 文件组织约束、
ECS Entity 约束、基础设施边界、组件命名约束、副作用隔离规范、C# 风格约束，以及主计划和
Arch 研究文档。必须读取 `C:\Users\shan\.agents\skills\pua\SKILL.md`，并在会话开头用
`/goal` 设定本长线目标。

允许：

- 新建或修改 `Test/Terraria.Arch.Verification/` 探针项目及其项目文件。
- 修改必要的测试/诊断脚本，使命令能把结果写到 `Build/diagnostics/ArchMigration/T1/`。
- 在本线文档中记录实际包资产、依赖、签名、版本和行为。

谨慎：

- 如果为了最小探针必须新增 `PackageReference`，只放到探针项目；不要先把 Arch 包扩散到
  所有生产项目。
- 不修改 `EntityRuntime`、生产组件、宿主装配或协议代码；发现问题时记录给 T2–T5。
- 不修改输入 world，不执行全 solution build，不清理既有工作树。

## 3. 分批执行

### T1-B0：基线快照与影响清单

1. 记录 `git status --short --branch`、当前 HEAD、`global.json`、相关项目文件和 .NET SDK。
2. 搜索生产、Test、参考目录中 `EntityRuntime`、`ComponentStore`、`RuntimeEntityHandle`、
   `EntityComponentSnapshot`、`Match`、`TryEdit`、`WorldStorageRoot`、身份 registry 和
   `LoadedWorldSession` 的引用；明确排除只读 `分类参考`。
3. 对每条引用记录 owner、读写集合、生命周期阶段、跨线程/排队边界和潜在 A2–A5 消费者。
4. 检查 A0 输入目录和现有报告；没有就记录“未建立”，不要伪造输入 hash。
5. 生成 `Build/diagnostics/ArchMigration/T1/baseline-inventory.json` 或等价 Markdown，
   记录 source hash、文件清单、已知失败、未接线路径和建议 owner。

### T1-B1：固定 Arch 包与最小 net10.0 探针

1. 建立 `Test/Terraria.Arch.Verification/`，目标框架和 SDK 遵循仓库现有策略。
2. 固定 `Arch 2.1.0`，记录实际 restore 后的传递依赖和资产选择；不能只引用研究文档中的
   `.nuspec` 结论。
3. 编写可重复探针，覆盖：`World.Create/Dispose/Destroy`、Entity `Id/WorldId/Version`、
   `IsAlive` 的边界、跨 World 误命中防护、组件 `Has/Get/TryGet/TryGetRef/Set/Add/Remove`、
   `QueryDescription` 的 All/Any/None/Exclusive、class/struct 实例隔离、ref 失效边界。
4. 编写独立 CommandBuffer 探针：`Create(ComponentType[])` 的暂存实体、分组回放、重复
   Set/Add 覆盖、已销毁目标、默认清理及异常后的部分效果；不要把它解释成事务。
5. 对线上文档与固定源码的差异逐项标记“文档漂移 / 以探针为准”，尤其是 namespace、
   `WithNone`、`Playback` 拼写和 EntityReference。

### T1-B2：编译门禁与交接

只针对探针项目执行 `dotnet build`（必要时先 restore）；不得运行探针或测试。报告命令、
项目、退出码、warning/error 数和 `Build/bin/` 输出路径。编译失败时保存第一条完整错误和
上下文，按 pua 要求切换排查方向；不以运行验证替代编译门禁。

### T1-B3：System 与官方关系/事件兼容性增补

1. 固定 `Arch.System 1.1.0`，验证 ISystem/BaseSystem/带名称 Group 的签名、注册顺序、
   嵌套、Initialize/BeforeUpdate/Update/AfterUpdate/Dispose；明确 Update 不自动调用前后钩子，
   Group 不提供业务依赖排序或并行 DAG。
2. 检查官方 Relationships 发布包与固定源码：NuGet 仅有 1.0.0/1.0.1，1.0.1 声明旧 Arch
   依赖；源码声明 2.1.0 不代表已发布。实际编译/运行确定兼容性，不降级核心或猜包版本。
3. 对需要关系自动清理的方案，验证 EVENTS、Debug/Release、Arch-Events 2.1.0 与扩展的
   实际依赖图。生产只解析一个兼容 Arch.dll；不要同时引入两套核心变体。
4. 必要时评估固定官方源码构建扩展，记录来源/许可证/hash/配置；不复制重写关系存储。
   source/target 销毁、移除重建和 World 释放的清理行为交接给 T3。
5. 本轮不运行新增能力探针；只记录涉及的项目和编译闭包。关系局部未决不改变编译结果，
   但必须作为未验证风险交接，不能把编译通过冒充关系通过。

## 4. 完成条件

- `Arch 2.1.0` 实际在仓库 SDK/net10.0 约束下 restore/build；依赖和资产可复核。
- 受影响探针项目编译通过或记录精确编译错误；不运行关键 API 探针。
- T2–T5 能拿到当前源码的编译闭包和未编译项目清单；行为边界作为后续风险记录。
- Arch.System、关系/Events 的运行时兼容性本轮保持未验证，不作为编译通过的附带结论。
- 生成 T1 交接报告，状态只能是 `done`、`partial` 或 `blocked`，不能用“基本完成”。

## 5. 失败与回退

包资产或签名不兼容时，先核对 SDK、资产选择、传递依赖和项目目标框架，再尝试最小替代
配置；不得把 Arch 版本随意升级。探针失败只回退探针项目和本线新增输出，不动生产路径。
生产引用发现与研究文档不符时，保留失败证据并把修复建议交给 T2–T5。

## 6. 交接给总验收会话

必须提交：探针代码/commit、baseline inventory、编译命令与结果、未编译清单、行为验证未执行
说明、T2–T5 的前置注意事项。交接报告放在 `Build/diagnostics/ArchMigration/T1/`，
必要时另写 `docs/research/` 事实增补，但不得把探针通过写成生产迁移通过。
