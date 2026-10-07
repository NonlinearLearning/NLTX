# Arch 迁移长线 T5：宿主汇合、保存加载、性能与旧框架退出审计

文档 ID：DOC-2026-10-08-ARCH-TRACK-T5  
状态：active；这是执行合同，不是已完成报告  
对应批次：A6–A8  
依赖：T1–T4 的可接受交接；在前置未满足时只做准备和审计，不做不可逆删除  
主计划：[自定义 ECS 转向 Arch 执行计划](2026-10-07-custom-ecs-to-arch-execution-plan.md)

范围增强：[Arch 原生 API 覆盖审计](../reviews/audits/2026-10-08-arch-native-api-coverage-audit.md)，尤其 N1–N5 与最终完成条件；退出审计按机制覆盖新命名的重实现。

编译-only 限制：本线不运行宿主、WorldFile、切换、保存加载、rollback/retry、benchmark、性能
或 A8 行为审计；只编译受影响宿主/保存加载项目。删除旧框架仍需另行获准，不能由编译结果自动授权。

## 1. 目标

把 Arch World 接入实际 Simulation、NetworkServer、世界切换、保存/加载和失败恢复路径，
建立同输入的性能对照，并在所有前置线验收后审计旧框架是否真的退出生产默认路径。T5 是最后
汇合线，但不能用宿主 smoke、微基准或 PackageReference 存在冒充迁移完成。

## 2. 安全边界与停止条件

- 开始先读取根入口、progress、所有协作/架构/构建主题、主计划、研究文档和 T1–T4 交接；
  读取 pua skill，用 `/goal` 设置 T5 目标。
- 允许准备宿主适配的编译闭包和 source/project 文档；不运行 fixture、保存加载验证、benchmark
  或运行时审计。
- 在总验收者书面确认 T1–T4 通过前，不得删除 `EntityRuntime`、`ComponentStore`、旧编辑委托、
  `RuntimeEntityHandle` 或旧测试框架；A8 删除只能先出清单和隔离 commit。
- 不改存档格式定义，不把 Arch Entity 的 Id/WorldId/Version 写入协议或 WorldFile。
- 不执行全 solution Rebuild，不清理工作树，不覆盖既有用户变更。

## 3. 分批执行

### T5-B0：宿主与回滚场景盘点

1. 找到 Simulation、NetworkServer、LoadedWorldSession、WorldStorageRoot、Save/Load、Prepare/
   Commit/rollback 的真实装配入口；记录当前 source drift 和支持 manifest。
2. 设计每个场景的 World/token/registry/relationship/buffer ownership：正常加载、一 tick、退出、
   两次切换、取消、零 tick、候选 late-finalize failure、retry。
3. 先确认 T2–T4 的接口已进入本 worktree；缺失时只写适配缺口和最小隔离夹具，不复制旧协议。

### T5-B1：实际宿主汇合

1. 模拟宿主和 NetworkServer 默认装配只使用 Arch World；旧实现若仍存在，只能作为明确历史
   对照输入，不能同一活世界双写。
2. 世界切换创建新 World/token，旧请求、关系、槽位和 Entity 不解析到新会话。
3. 候选失败先撤销 UUID/关系/缓冲/订阅，再完整释放 World；原会话状态必须仍可用。
4. 保存通过领域值快照和现有 DTO/Codec/端口，不安装 Arch.Persistence 替换存档边界。
5. 真实 `.wld` 输入保持不变；所有输出复制到 `Build/diagnostics/ArchMigration/T5/`。
6. 消费 T1/T4 的 Arch.System 合同，用官方 Group 组织已迁移的周期系统；接线完整生命周期、
   零 tick 释放、异常与逐执行边界的会话失效检查。Kernel 继续负责线程、领域请求、发布门禁
   和 tick 提交，退出旧通用 phase 接口/排序框架。
7. 核对真实 Release 的关系/Events 程序集、编译符号、订阅清理和 World 隔离；不要只验 Debug
   夹具。世界单例组件与加载/保存投影读取同一权威 Arch 状态。

### T5-B2：性能测量与必要优化

1. 使用 A0 冻结的 Player 255、NPC 200、Projectile 1000 和实际组件组合/Spawn/Destroy/Add/Remove/
   Item transfer/完整 Simulation tick 输入。
2. 至少比较旧基线、Arch 单实体 Get/Set、Arch 批量 Query；记录 warm-up、耗时分位数、分配、
   GC、内存、archetype 数量和结构变化比例。
3. 按主计划预算记录：完整 tick p95、每 tick allocation、稳定存活内存；若 A0 尚未冻结，先
   记录“预算未冻结”并不得宣布性能通过。
4. 只做必要且可解释的优化（QueryDescription 缓存、减少重复 Add、合理初始组合、减少无谓
   快照分配）；不同时启用并行、PURE_ECS、SourceGenerator 或 chunk 调参。

### T5-B3：A8 退出审计与隔离删除准备

1. 审计生产项目引用、默认构造、模拟/网络装配、备选分支和测试夹具；区分 `分类参考`、历史
   文档、测试对照与 production compile。
2. 生成旧框架删除清单、依赖闭包、潜在遗漏和回滚点；先提交审计报告。
   清单还包括 IWorldSimulationTickPhase、通用关系图/反向字典、结构缓冲、世界状态容器及
   槽位/包装中的重复 ECS 职责。检查新命名的同等实现，不只匹配原类型名；未发现的机制
   标记检索范围，不凭零命中证明所有动态入口都不存在。
3. 只有总验收者另行确认删除范围后，才执行删除 commit；删除后只编译受影响项目，不运行测试
   或宿主场景，也不声称行为通过。

### T5-B4：编译结果交接

只编译受影响 Simulation、NetworkServer、WorldStorage 和宿主项目；不运行加载、tick、切换、
rollback/retry、保存/重载、取消、零 tick 或 benchmark。前置未完成时将未编译闭包标为
`partial/blocked`，不以 smoke 绕过依赖。

## 4. 完成条件

- 当前源码的真实 Simulation/NetworkServer/WorldFile 路径默认使用 Arch World。
- 世界切换、失败候选、rollback/retry、保存加载、取消和零 tick 退出本轮不运行，列为未验证风险。
- 性能报告与冻结预算本轮不生成；未启用优化和未覆盖路径具名。
- 旧通用 ECS 框架 production compile 声明/引用和同等重实现为零；合法领域身份、协议投影、
  业务状态与历史材料按职责列出。任何未退出的通用 ECS 例外都不能宣布全面迁移完成。
- 默认宿主/System 生命周期、世界单例、关系/事件 Release 清理及逐边界会话停止本轮不运行，
  只交接代码位置和未验证风险。
- A8 删除 commit（若获准）、编译记录、最终限制和回滚点交接；不提交测试/性能证据。

## 5. 失败切换与交接

宿主失败时先区分当前源码问题、输入/产物不匹配、World/token 泄漏、候选清理错误和历史支持
范围；记录完整错误与 50 行上下文，核对 source/DLL/PDB/input hash。连续两次同方向失败必须
切换到最小 fixture 或隔离 rollback probe，并按 pua skill 完成强制清单。性能未达预算时，不得
用微基准胜出掩盖完整 tick 回归。

交接必须包含受影响项目的编译命令和结果、source/project 文档、删除清单或删除 commit、
未覆盖能力、未运行说明、回滚步骤和总验收建议。最终 ledger 由总验收会话创建/更新，T5 不得
独自把整项迁移标为 complete。
