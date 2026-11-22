# ECS 运行时边界收敛总计划

文档 ID：PLAN-2026-11-08-ECS-ARCHITECTURE-REMEDIATION
逻辑域：plans
产物类型：plan
状态：active
范围：NLTX 的 ECS Tick、网络输入、结构变更、系统调度、实体生命周期、World owner 与项目文件组织
证据入口：2026-11-08 ECS 参考项目静态审查；`src/NSSLC/Components/Share/Entity/EcsServer.cs`；`src/NSSLC.Server/Program.cs`；`src/NSSLC/Components/Communication/CommunicationSessionIngress.cs`；`src/NSSLC.Network/CommunicationHostRuntime.cs`
canonical 路径：`docs/plans/2026-11-08-ecs-architecture-remediation-master-plan.md`

## 1. 目标

把本轮架构审查中确认的边界偏离收敛到可验证的运行时契约：

1. Arch `World` 只由 ECS 主循环线程访问。TCP worker 只提交线程安全、不可变的原始 ingress 事件。
2. 结构变更通过宿主拥有的 CommandBuffer，在明确的提交 barrier 统一 playback。
3. phase 内的先后关系由可验证的依赖元数据表达，启动时拒绝缺失依赖和环。
4. 实体销毁同时清理 runtime registry、AI binding、网络 slot、session 关联和待处理事实。
5. `WorldRuntime` 和 `Terraria.Components` 的边界逐步收窄；不为尚未存在的持久化或 chunk 功能提前制造空抽象。
6. 清理活动项目中已经失效的 `Component/**/*.cs` 编译声明和相关旧路径引用。

“完成”必须以源码、测试、构建和运行证据共同证明；计划文件、编译成功或静态 grep 单独不能证明行为等价。

## 2. 当前基线与风险

| 项目 | 当前事实 | 状态 | 风险 |
| --- | --- | --- | --- |
| Tick 边界 | `EcsServer.Update` 按 `Preparation/Simulation/Commit/Projection` 执行，只有 `_updating` 重入保护 | confirmed | 外部线程仍可访问 World |
| 网络输入 | `CommunicationHostRuntime` 的 worker 回调进入 `CommunicationSessionIngress`，后者直接读写 Arch World 和普通 `Dictionary` | confirmed | P0/P1 并发与生命周期风险 |
| 结构变更 | `EcsServerCommandBuffer` 存在但没有接入主循环，创建/销毁/Add/Remove 仍散落 | confirmed | deferred boundary 失效 |
| 系统顺序 | `Program.BuildRegistrations` 依赖注册列表位置和注释 | confirmed | 重排或插入系统无法自动发现错误 |
| runtime identity | `EntityRuntimeRegistry` 有 world scope/generation，但服务器退出未显式清理 | partial | 悬挂映射与重用风险 |
| AI 生命周期 | `TryDetach` 存在，NPC 销毁路径没有完整接入 | partial | binding 与实体生命周期脱节 |
| NPC slot | generation 固定为 1；槽位耗尽分支在 drain 后直接返回 | confirmed | 旧 occupant 与事实丢失风险 |
| World owner | `WorldRuntime` 维护 revision、tile、liquid、mutation、空间端口和 projection 事实 | partial | 未来扩展时职责过度集中 |
| 项目配置 | `Terraria.Components.csproj` 仍包含不存在的 `Component/**/*.cs` | confirmed | 迁移残留和维护误导 |

## 3. 子计划与依赖

实现会按以下顺序启动新会话。每一批完成并通过自己的验收后，才能启动下一批；禁止多个会话同时修改共享工作树中的同一批源码。

| 顺序 | 子计划 | 优先级 | 主要代码边界 | 前置 |
| --- | --- | --- | --- | --- |
| 1 | [线程所有权与原始 ingress](2026-11-08-ecs-thread-ownership-ingress-plan.md) | P0/P1 | `NSSLC.Network`、`CommunicationSessionIngress`、Preparation 注册、Program 连接回调 | 无 |
| 2 | [CommandBuffer 与 Commit barrier](2026-11-08-ecs-command-buffer-commit-plan.md) | P0/P1 | `EcsServer`、`EcsServerTickContext`、结构变更调用点、实体定义和生命周期入口 | 1 |
| 3 | [phase 依赖验证](2026-11-08-ecs-scheduling-dependency-validation-plan.md) | P1 | `EcsServerSystemRegistration`、`EcsServer`、`Program.BuildRegistrations` | 2 |
| 4 | [实体、AI、网络 slot 生命周期](2026-11-08-ecs-entity-lifecycle-network-slot-plan.md) | P1/P2 | registry、NPC slot、AI adapter、session cleanup、shutdown | 1、2、3 |
| 5 | [职责收窄与项目配置清理](2026-11-08-ecs-domain-boundary-project-config-plan.md) | P2 | World contracts、`WorldRuntime`、活动 csproj、引用审计 | 4 |

子计划执行会保留独立的修改范围和验证记录。若某一批发现必须改变后续契约，先更新对应计划和依赖关系，再继续实现。

## 4. 共同不变量

- Arch `World`、Arch query、ECS component 和 entity lifetime 只在 ECS 主循环线程访问。
- 网络 worker 不保存或读取 ECS entity、session component、普通 session dictionary 或业务状态。
- 原始 ingress 事件不可变，至少带 `SessionEpoch`、事件顺序信息、接收时间/序号和明确的 open/packet/close 类型。
- `Preparation` 是外部输入进入 ECS 的唯一入口；`Projection` 只产生 outbound effect，不回写权威 gameplay 状态。
- 所有结构变更都有唯一 owner 和唯一 playback 边界；同一实体不能同时由直接 World API 和 CommandBuffer 修改生命周期。
- 现有四 phase 的可观察顺序保持不变；独立系统的稳定顺序保持兼容，依赖声明只约束必要关系。
- session、玩家实体、runtime handle、AI binding、NPC slot 和 generation 的清理必须幂等。
- 槽位耗尽不得静默丢弃已接收的 spawn fact；未处理事实必须保留、重试或变成明确可观测的拒绝。
- `WorldRuntime` 仍是当前权威 world owner；边界收窄不能复制一份权威 tile 或 mutation 状态。

## 5. 共同验证门槛

每个子计划完成时必须提供：

1. 受影响项目的增量构建命令、退出码、warning/error 计数和 `Build/bin/` 输出位置。
2. 与主风险相符的行为验证：并发输入、顺序、生命周期、结构提交或依赖图测试，不能只做编译。
3. 变更前后关键探针或协议输出的比较；若行为有意改变，写明旧语义、新语义和迁移原因。
4. 静态边界检查结果，包括直接 World 访问、直接结构变更、未声明依赖和旧路径引用。
5. 未覆盖项和 `confirmed/partial/unknown/proposed` 证据状态。

推荐的最终回归目标是：`Terraria.Components`、`NSSLC.Network`、`NSSLC.Server` 及受影响验证项目分别构建；通信生命周期、NPC spawn/slot、聊天、WorldItem、TileEntity、Inventory 和 Projectile 现有验证全部重新运行。不得使用 solution 全量重建替代受影响项目的增量验证。

## 6. 执行会话协议

- 每个新会话使用 `gpt-5.6-luna`，推理强度 `max`，从当前共享工作树读取本计划和对应子计划。
- 会话开始先重新检查 `AGENTS.md`、`Context/progress.md`、对应约束和 `git status`；不得恢复或覆盖无关用户修改。
- 会话只实施自己子计划的源码范围；发现跨计划契约时，记录并等待前置会话完成，不在同一工作树并行抢改。
- 不以“代码看起来合理”宣布完成；实现会话必须运行计划要求的验证并返回证据路径。
- 失败或部分完成时保留工作树和诊断，不使用破坏性 reset/checkout；由下一会话依据当前状态继续。

## 7. 暂不承诺的范围

本计划不承诺完整 Terraria gameplay parity，不恢复已删除的旧应用层，不复活 `NSSLC.Infrastructure/Network` 历史项目，不引入事件溯源、全局消息总线或 CQRS 类型层。World owner 的物理拆分只有在现有端口和新增功能需求证明必要时才实施。
