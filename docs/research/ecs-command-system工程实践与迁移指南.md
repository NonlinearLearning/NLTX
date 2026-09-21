# ECS Command / System 工程实践与迁移指南

本文面向把传统面向对象或“大循环”游戏代码迁移到 Entity Component System（ECS）的工程团队，重点回答三个问题：Command 应该放在哪里、System 如何组织、如何分阶段完成迁移并保持可验证性。

## 1. 先建立共同模型

ECS 将运行时状态拆成实体（Entity）、数据组件（Component）和处理逻辑（System）。实体只提供身份；组件保存可序列化、可查询的数据；System 通过查询读取和写入组件。Unity Entities 文档明确把组件描述为无行为的数据，并以 Query 选择匹配实体：[Unity Entities concepts](https://docs.unity.cn/Packages/com.unity.entities@1.0/manual/concepts.html)。Flecs 也将系统定义为对组件查询的处理器，并强调查询驱动的迭代：[Flecs Systems](https://www.flecs.dev/flecs/md_docs_2Systems.html)。

迁移时应把“意图”和“结果”分开：Command 是外部输入或跨系统请求（例如 Move、Damage、Spawn），它描述要做什么；System 在明确的阶段消费 Command，执行确定性计算并写回组件。Command 不应直接持有 World、渲染器或网络连接，I/O 由适配器完成。

## 2. Command 的边界与生命周期

推荐使用不可变命令记录：`Command { EntityId, Kind, Payload, IssuedTick, Source }`。输入、网络、脚本等适配器只负责产生命令并放入按 tick 排序的队列；Simulation Systems 在固定阶段批量读取；处理完成后记录成功、拒绝或重试原因。这样可以重放、审计和做客户端预测回滚。

命令处理应满足：

1. **校验与授权先行**：实体是否存在、来源是否有权操作、数值是否在协议范围内。
2. **确定性排序**：同一 tick 使用序号或来源优先级解决并发顺序，避免依赖线程调度。
3. **一次消费**：成功或明确失败后标记已处理；跨 tick 的工作使用显式状态组件。
4. **副作用隔离**：生成网络包、写存档、播放音效等动作输出到 Effect/Port 队列，由边界适配器执行。

## 3. System 的分层与调度

按能力和数据流划分系统，而不是按实体类型堆积巨型类。常见阶段为：Input/Command 收集 → 规则校验 → Simulation（移动、战斗、资源）→ 生命周期（Spawn/Destroy）→ 派生索引与事件 → Presentation/Networking。每个 System 应声明读写集合；读写冲突由调度器或代码评审发现。Bevy 的 Schedule/Systems 文档展示了以系统集合和依赖约束表达执行顺序的做法：[Bevy ECS schedules](https://bevyengine.org/learn/book/getting-started/ecs/)。EnTT 则提供组（group）和视图（view）以高效遍历匹配组件：[EnTT wiki](https://github.com/skypjack/entt/wiki/Entity-Component-System)。

系统代码保持“纯计算核心 + 薄适配层”：核心函数输入组件快照和命令，返回组件变更及效果；时钟、随机数、日志、持久化、消息发送通过接口注入。这样单元测试无需启动完整 World。

## 4. 从旧架构迁移的阶段

### 阶段 A：盘点与基线

建立成员/字段清单：旧类字段、所有写入点、生命周期、线程归属、序列化键和网络可见性。为关键场景建立回放或 golden test，记录 tick、实体数量、关键数值和副作用事件。先确认运行时真正加载的程序集和注册表，避免只依据过时源码。

### 阶段 B：抽取数据组件

先搬运低耦合、值语义明确的字段（位置、速度、生命、库存），保持旧 API 作为 facade。一个组件只表达一个稳定能力；不要为了“纯 ECS”一次性拆到无法理解。为每个组件定义默认值、添加/移除语义、序列化版本和注册键。

### 阶段 C：双写与影子系统

旧逻辑继续提供结果，ECS System 读取同一输入并影子计算；比较结果差异并分类（浮点误差、顺序差异、真实回归）。差异稳定后切换为 ECS 结果，保留回退开关和指标。

### 阶段 D：命令化入口

把 UI、网络包、脚本调用改为产生 Command；旧入口暂时转换成同样的 Command。禁止新代码直接修改核心组件，所有写入集中在拥有该字段的 System。

### 阶段 E：删除旧实现与性能验收

按依赖反向删除 facade 和双写路径。使用代表性世界规模测量帧时间、分配、查询耗时、命令积压和回放一致性；性能未达标时先检查查询范围、结构变化和缓存布局，再考虑并行。

## 5. 从开源项目提交历史提炼经验

阅读提交时关注“为什么改变边界”，而非只复制 API：Unity Entities 的包版本迁移记录可见其 [Entities changelog](https://docs.unity.cn/Packages/com.unity.entities@1.0/changelog/CHANGELOG.html)；Flecs 的架构和迁移在 [仓库提交历史](https://github.com/SanderMertens/flecs/commits/master/)；Bevy ECS 的调度、变更检测演进在 [提交历史](https://github.com/bevyengine/bevy/commits/main/crates/bevy_ecs/)；EnTT 的查询和存储优化在 [提交历史](https://github.com/skypjack/entt/commits/master/src/entt/)。建议每次迁移记录：旧模型、目标不变量、兼容层、验证命令、性能数据、回滚条件。

### 2026 年近期提交观察（截至 2026-09-12）

- **Bevy**：提交 `b3fd9d7` 为管道系统增加可配置名称，且保留零大小类型以支持缓存运行；说明系统包装器也需要可观测、稳定的诊断标识（[commit](https://github.com/bevyengine/bevy/commit/b3fd9d783124128ac169704d0f4c0b8790daae78)）。提交 `dca1c42` 修复 observer 初始化期间潜在释放自身导致的未定义行为，做法是先从组件中取出系统再初始化（[commit](https://github.com/bevyengine/bevy/commit/dca1c421a8cc8c091694c2f1874c4ea973f2d135)）。迁移启示：生命周期操作必须与借用/所有权边界分离，并为系统提供稳定名称。
- **Bevy 性能**：提交 `7fecf1c` 将大实体切片的重复检查从 O(N²) 改为 O(N)，同时加入 Criterion 基准（[commit](https://github.com/bevyengine/bevy/commit/7fecf1cd8b211ab16a3b9ad057ef56f967c3a400)）。迁移启示：每次查询重写都应带规模化基准和复杂度说明。
- **Flecs**：近期提交加入脚本事件模块并修复查询树丢行问题（[commits](https://github.com/SanderMertens/flecs/commits/master/)）。迁移启示：事件/Command 扩展要配套查询一致性测试，尤其关注层级关系和过滤条件。
- **EnTT**：提交 `4a583eb` 修复资源比较运算符未定义行为（[commit](https://github.com/skypjack/entt/commit/4a583ebec3cd5263063d2e0feb91e3393fb4a136)）。迁移启示：组件资源句柄、比较器和生命周期代码应纳入未定义行为检测与 sanitizer 测试。

## 6. 团队落地检查表

- 每个 System 有明确读/写组件集合、阶段和所有权。
- Command 可序列化、可重放，处理结果可观测。
- 组件命名、注册键和文件路径遵守项目约束，迁移记录源/目标路径及依赖影响。
- 关键场景具备确定性回放或差异测试；副作用有端口和失败策略。
- 增量构建、测试和性能基准均可从干净环境复现；切换开关和删除门槛写入计划。

### 参考资料（第一方）

- Unity Technologies, [Entities manual](https://docs.unity.cn/Packages/com.unity.entities@1.0/manual/concepts.html)
- Sander Mertens, [Flecs documentation](https://www.flecs.dev/flecs/)
- Bevy contributors, [Bevy ECS book](https://bevyengine.org/learn/book/getting-started/ecs/)
- skypjack, [EnTT repository and wiki](https://github.com/skypjack/entt/wiki/Entity-Component-System)
