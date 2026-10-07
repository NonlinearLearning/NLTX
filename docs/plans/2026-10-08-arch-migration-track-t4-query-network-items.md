# Arch 迁移长线 T4：原生查询、System 访问、网络重解析与物品提交保护

文档 ID：DOC-2026-10-08-ARCH-TRACK-T4  
状态：active；这是执行合同，不是已完成报告  
对应批次：A4–A5  
依赖：T2 的访问合同；消费 T3 的生命周期/关系合同  
主计划：[自定义 ECS 转向 Arch 执行计划](2026-10-07-custom-ecs-to-arch-execution-plan.md)

范围增强：[Arch 原生 API 覆盖审计](../reviews/audits/2026-10-08-arch-native-api-coverage-audit.md)，尤其 N1/N4/N5；周期 System 改为官方接口，并消费 T1 的 System 探针。

编译-only 限制：本线不运行 Query/ref、网络队列、物品冲突、RNG、序列化或 admission 验证；只
编译受影响 System、查询、网络和物品项目。

## 1. 目标

让领域 System 从旧 `Match/TryEdit/ComponentStore/RuntimeEntity` 访问迁移到 Arch 原生查询
和明确的领域 owner 协议；周期系统使用 Arch.System 的 ISystem，按需要使用 BaseSystem，
并向 T5 交接官方 Group 的注册与生命周期合同，同时保留显式业务顺序。网络、异步和排队请求不得捕获会
跨会话失效的裸 ID/ref；物品预留、拾取和转移不得因为删除通用快照 API 而失去过期提交保护。

## 2. 设计不变量

- QueryDescription 是筛选描述，不是生命周期、初始化或发布保证；查询命中后仍要检查领域状态。
- `TryGet<T>` 对 struct 是副本；写入必须使用短期 ref 或 Set，并遵守借用边界。
- Add/Remove/Destroy/创建和可能导致实体移动的结构变化前，结束所有相关 ref；变化后重新取得。
- 依赖旧槽位/RNG/同 tick 顺序的路径继续由显式 owner 调度；独立批处理才使用普通 Query。
- Arch CommandBuffer 只承担声明的结构提交，不替代领域命令队列、权限、epoch、事务或 FIFO。
- 排队请求保存业务意图、连接 epoch、世界 token、UUID/可解析 identity；消费时重新解析并校验。
- 物品冲突保护使用领域 revision/预留 token/提交时重新校验，不能保留一个泛化 revision API
  作为第二套权威组件状态。

## 3. 前置读取与范围

先读取根入口、progress、协作/构建约束、ECS 文件组织、ECS Entity、基础设施边界、副作用、
C# 风格、主计划、研究文档和 T2/T3 交接；读取 pua skill，用 `/goal` 设定目标。

主要范围：NPC/Player/Projectile/Items/Physics/Simulation 等真实 System、旧 Match/TryEdit
调用、网络 admission/owner/queue、`RuntimeItemRegistry`、`RuntimeWorldItemStore.NetworkSync`、
PlayerItemSpace 及相关 DTO/投影。不要把 Arch Entity 的整数直接写入协议或存档。

## 4. 分批执行

### T4-B0：访问与副作用盘点

1. 按 System 建立读/写组件集合、结构变化点、随机/时间/网络/存档副作用、执行阶段和顺序合同。
2. 标出长期持有 ref、跨 tick 捕获实体、async/queue 捕获旧句柄、把 struct 副本误当写入的路径。
3. 对每个旧通用 API 找真实业务不变量；能删除的框架细节不要在 T4 重建，必须保留的物品
   冲突保护则改成具名 owner 协议。

### T4-B1：原生查询与 System 访问

1. 缓存稳定 `QueryDescription`；先迁移无结构变化的纯读取/字段更新，再处理创建/删除/Add/Remove。
2. 对独立批处理使用 `World.Query`；对依赖 NPC slot、同 tick spawn、RNG 消费或平局选择的
   路径保留显式槽位/领域顺序，记录为什么不能直接使用 chunk 顺序。
3. 查询内收集带身份的结构意图，结束 query 后由 owner 校验并提交；需要立即可见的路径在
   明确安全边界提交，不能统一推迟到 tick 末。
4. 收窄或删除长期持有所有组件的 `RuntimeNpcEntity/RuntimePlayerEntity`；只保留值投影或
   明确边界视图，禁止第二份可写组件。
5. 周期更新对象实现 `ISystem<WorldSimulationTickContext>`，需要默认实现时采用
   `BaseSystem<World, WorldSimulationTickContext>`；退出 IWorldSimulationTickPhase，
   不重建通用 System 接口/组。普通领域计算方法按职责保留。
6. 交接 Initialize/BeforeUpdate/Update/AfterUpdate/Dispose、显式阶段顺序及可能切换会话的
   边界；宿主分组和检查接线归 T5，不能用整个 tick 一次 Group.Update 丢失中途停止语义。
7. 重复查询样板可使用官方 SourceGenerator；手写 World.Query 也是原生 API。删除旧多组件
   编辑/通用 Capture 层，不以新的泛型 Arch facade 延续它。

### T4-B2：网络/异步请求重解析

1. 入队保存连接 epoch、会话 token、UUID/协议 identity 和业务参数，不保存裸 Entity.Id 或
   可写 ref。
2. owner thread 排空时依次检查会话、epoch、WorldId、Version/alive、UUID、能力和生命周期，
   然后才读写 Arch 组件。
3. 跨世界、过期请求、旧会话、实体重建和终止中实体都要有明确拒绝原因；不把拒绝写成成功空操作。
4. 保持协议字段和 DTO 方向；Arch Entity 不进入 wire/persistence payload。

### T4-B3：物品提交保护

1. 把旧组件快照用途分为：真正物品提交冲突、普通同步读、诊断值快照；只为第一类建立领域协议。
2. 验证旧拾取计划、预留失效、数量改变、移除重建、转移失败、重复提交不会覆盖新状态或制造
   数量。
3. 明确预留 token 的签发、消费、释放、过期和世界切换失效；owner 负责所有副作用顺序。

### T4-B4：编译结果交接

只编译受影响 System、查询、网络和物品项目及引用闭包；上述行为场景全部不运行。记录编译
命令、退出码、warning/error 数、输出路径、未编译 gameplay 和未验证风险。

## 5. 完成条件

- 支持集内的 ECS 存储/访问/查询使用 Arch 原生 API，周期系统使用官方接口；旧通用访问包装退出。
- 没有跨结构变化/跨帧可写 ref；查询顺序依赖被显式建模。
- 排队请求按 token/epoch/UUID/WorldId/Version 重新解析；过期请求不会写新实例。
- 物品数量守恒与过期提交保护的 owner 已具名，但本轮不运行证据。
- 编译结果、未覆盖系统和未接线网络 gameplay 清晰交接；不提交测试结果。

## 6. 失败切换与交接

遇到编译错误，先读取完整错误和上下文，再从“收窄依赖闭包/最小隔离项目”的方向
修复，禁止只换 lambda 写法。网络/物品风险只记录 token、epoch、UUID、
预留状态、数量和关系 owner；不能只修最后一条异常。交接包括系统访问矩阵、请求重解析协议、
物品冲突合同、changed files/commit、证据和给 T5 的宿主注意事项。
