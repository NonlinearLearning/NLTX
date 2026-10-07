# Arch 原生 API 覆盖与自定义 ECS 退出审计

文档 ID：AUDIT-2026-10-08-ARCH-NATIVE-API-COVERAGE  
逻辑域：reviews / architecture / migration  
产物类型：static-audit / scope-enhancement  
日期：2026-10-08（Asia/Shanghai）  
状态：审计完成；增强项待实施和行为验证  
源码范围：`D:/TRbackup/NLTX` 当前主工作树；读取时 HEAD 为 `e2c686790ab6a4f14505ea914c27478f5959d061`  
关联：[主计划](../../plans/2026-10-07-custom-ecs-to-arch-execution-plan.md)、[并行执行合同](../../plans/2026-10-08-arch-parallel-execution-coordination.md)、[原始 API 研究](../../research/2026-10-07-arch-api-migration-research.md)

## 1. 用户要求与本次结论

用户原文：

> 所有涉及到ECS的方面全部使用arch的api,而不是自己造一套东西,检查哪些部分还需增强可以更多的使用arch并完全移除自己造的

原计划已经要求删除自定义实体存储、句柄、查询与组件编辑层，但仍允许自定义系统更新接口，并把所有扩展统一列为可选。新要求需要扩大完成条件：**Arch 提供的通用 ECS 机制直接使用其 API，禁止在外面重建另一套同等通用机制；项目自己的玩法规则继续由领域代码实现。**

最需要增强的是系统接口和组织方式、世界级权威组件、关系扩展的兼容性与清理、槽位/包装层的职责收窄，以及检查“换了名字的旧框架”的最终退出审计。原生 `World.Query` 和原生组件中的实体关系数据本身已经使用 Arch；不要求为了包数量把所有方法都改成 SourceGenerator 或安装与当前行为无关的扩展。

本次只修改审计和迁移合同，没有修改生产 C#、安装项目依赖、运行构建或行为测试。其他执行会话的结果以其交接与[执行总账](../../migration/ledgers/2026-10-07-arch-migration-execution-ledger.md)为准；本文不重新判定这些会话已经通过或失败。

## 2. 当前源码证据

在生产 `src` 下排除 `分类参考`、`obj`、`bin`，按完整类型词检索
`EntityRuntime|ComponentStore|RuntimeEntityHandle|EntityComponentSnapshot`，得到 **50 个不同 C# 文件**。这是这些词的直接影响面，未包含全部间接消费者；扩大到同前缀标识符会得到不同数量，不能混用统计口径。

同一范围内未检索到 `Arch.Core`、`Arch.System` 或 Arch 的 `PackageReference`。这只描述当前主工作树的文本观察，不证明其他隔离 worktree 没有进度，也不替代生产项目编译输入审计。

| 位置 | 已观察到的机制 | 结论 |
| --- | --- | --- |
| [EntityRuntime.cs](../../../src/NSSLC/Component/Share/Entity/System/EntityRuntime.cs) | 类型到组件表的字典、实体记录、generation、BorrowCount、Match | 自定义 ECS 核心，必须删除 |
| [ComponentStore.cs](../../../src/NSSLC/Component/Share/Entity/System/ComponentStore.cs) | 组件 cell、数组、attachment/data revision | 必须删除；真实物品冲突规则转为领域协议 |
| [ComponentAccess.cs](../../../src/NSSLC/Component/Share/Entity/System/ComponentAccess.cs) | 通用快照及多组件编辑/检查委托 | 必须删除，不重建转发层 |
| [RuntimeEntityHandle.cs](../../../src/NSSLC/Component/Relationships/System/RuntimeEntityHandle.cs) | runtime/index/generation 句柄 | 改成完整 Arch Entity |
| [WorldSimulationKernel.cs](../../../src/NSSLC.Application/Simulation/WorldSimulationKernel.cs) | 自定义 phase 数组、排序及 Execute 遍历 | 通用 System 接口/组织改用 Arch.System；保留业务检查 |
| [LoadedWorldSession.cs](../../../src/NSSLC.Application/WorldStorage/Loading/LoadedWorldSession.cs) | 独立 runtime 和独立世界状态对象 | 一个 Arch World，世界权威状态挂到单例实体 |
| [WorldSessionRestoreState.cs](../../../src/NSSLC/Component/WorldSession/System/WorldSessionRestoreState.cs) | 独立持有规则、时间、进度等状态 | 不再作为 Arch 外的长期权威状态容器 |
| [EntitySlotStore.cs](../../../src/NSSLC/Component/WorldStorage/System/EntitySlotStore.cs) | 通用状态数组、occupancy 和槽位 generation | 只允许承担有明确消费者的槽位投影 |
| [RuntimeNpcEntity.cs](../../../src/NSSLC.Tools.Simulation/RuntimeNpcEntity.cs)、[RuntimePlayerEntity.cs](../../../src/NSSLC.Tools.Simulation/RuntimePlayerEntity.cs) | 泛型 Capture/Edit 和聚合实体访问 | 删除通用访问职责，状态和更新直接走 Arch |

关系不是未使用的示意设计。`RuntimeNpcStore` 调用 `RuntimeNpcEntity.TryAttachParentRelation`、`TryCaptureParentRelation` 和 `TryDetachParentRelation`，后者同时维护 `NpcParentRelationComponent` 与 `EntityRelationState`。两份描述同一父子关系的数据需要确认唯一权威来源并移除冗余；类型中的 `status: proposed` 注释不能覆盖已存在的调用证据。其他关系类型、所有销毁路径及动态入口仍须由 T3 补齐。

`EntityLifecycleState` 此次仅查到声明，不能称为已经接入的生命周期引擎。`WorldNpcState` 是只读加载数据，物品槽位中的记录还包含复用冷却；不能把所有 `WorldEntityState` 派生类都认定为重复可写 ECS 组件。

## 3. 必须增强的迁移项

### N1：系统接口与组织直接采用 Arch.System

当前 `IWorldSimulationTickPhase` 定义 Phase/Execute，Kernel 将其排序后逐个调用。该通用系统契约退出，周期更新对象使用官方 `ISystem<WorldSimulationTickContext>`，需要 World 成员及默认生命周期实现时使用 `BaseSystem<World, WorldSimulationTickContext>`；注册与分组使用官方 `Group<T>`。不新增 `IEcsSystem`、`EcsScheduler`、自制 Group 或统一 Update 转发框架。

固定来源的 `Group<T>` 构造要求名称，按注册顺序执行，可嵌套。其 `Initialize`、`BeforeUpdate`、`Update`、`AfterUpdate` 是独立调用；调用 Update 不会自动执行前后钩子。它没有基于组件读写集的自动排序或并行 DAG。T1 必须验证这些行为，T4 实施领域 System，T5 完成宿主生命周期接线。

Kernel 继续负责 owner thread、网络/领域请求排空、世界 token、加载就绪、tick 提交、故障与取消。业务阶段枚举可保留用于显式顺序和 trace，不能继续作为第二套通用 System 接口。必须保留原来每个可能切换会话的执行边界后的失效检查：只在组内不会改变会话的路径直接调用整组 Update；必要时沿官方 Group 的枚举入口调用官方 System 生命周期并检查会话。不能把整个 tick 塞进一次 Group.Update 后丢掉中途停止语义。

纯计算、一次性领域操作或已有明确职责的方法可以保持普通 C# 方法；无需把每个名称含 System 的静态函数改成一个定时实例，也不能只套一层没有职责的框架适配类。

### N2：世界级权威组件进入 Arch World

`LoadedWorldSession._world` 当前持有 `WorldSessionRestoreState`，后者独立拥有 Descriptor、Rules、TimeWeather、Progression 等对象。候选加载、正式运行、保存快照都应读取同一份 Arch 权威状态。

T2 建立会话内世界单例实体，世界规则、时间、进度及其他运行时世界组件通过原生 Create/Get/Set/Query 访问；按实际领域边界挂组件，避免只把旧全量状态聚合包装成一个新的通用资源管理器。原恢复对象可以退出，或仅用于值快照/恢复输入，不能独立接受权威写入。生命周期门禁组件与外部发布流程按职责区分，不强制把文件、连接、发布事务都变成 ECS 数据。

现有 `IsFresh` 依赖 `EntityCount == 0`。引入必要单例后，必须改成“默认世界组件已就绪、尚无玩法实体/加载提交”的实际条件，不能让加载新鲜度检查永久失败。默认单例也不能提前使候选世界进入普通模拟。

Arch.LowLevel 的 `Resources<T>` 是带索引句柄的外部资源集合，不是 World 自带的 singleton API，不提供 Entity 的 world/version 语义；不能用它再建立一套世界实体身份。

### N3：关系优先使用官方扩展，并单独验证发布与清理

T1 增加官方 `Arch.Relationships` 兼容性探针；T3 对通用关系的新增、读取、替换、移除和反向清理优先使用已验证的官方 API。不得新建泛型关系图、双向关系字典或以新的关系 runtime 延续旧框架。

这里有明确的版本缺口，不能靠写 PackageReference 假定解决：

- 截至此次查询，NuGet `Arch.Relationships` 仅发布 1.0.0、1.0.1。
- 已只读检查 1.0.1 包内 nuspec，其声明的 Arch 依赖版本为 `1.2.6.5-alpha`；这不证明它与 2.1.0 二进制兼容，也不能仅凭旧依赖版本断言运行必然失败。
- 官方 Arch.Extended 提交 `18d1e4c1faec6c83335434969933601b346d19fc` 的关系项目声明自身版本 2.1.0、引用 Arch 2.1.0；这是源码信息，不能当作 NuGet 已发布 2.1.0。
- 该源码的 `HandleRelationshipCleanup/CleanupRelationships` 受 `#if EVENTS` 控制，项目仅在 Debug 配置直接定义 EVENTS。Release 的实际符号和清理 API 必须另外核对；仅在 NLTX 调用项目定义符号不会替扩展程序集生成这些方法。
- `Arch-Events 2.1.0` 已发布，与核心稳定版本记录同一 repository commit，并提供 `Arch.dll`。它是事件能力的核心变体；不能把普通 Arch 和 Arch-Events 当成两套可同时拥有的 World 实现。

T1 应先验证实际可取得的组合。若采用固定官方源码构建扩展，记录 commit、许可证、源码 hash、配置、EVENTS、依赖图及产物 hash；生产闭包只解析一份兼容 Arch 程序集，不在业务目录复制重写官方存储。不能为了旧关系包降级核心版本，也不能安装不存在的版本。

官方扩展的适用性尚未通过本仓库编译/运行探针。这个缺口只限制依赖它的关系接线，不阻止独立的核心存储、系统与世界组件迁移。对无需通用图机制的明确领域关系，可以在 Arch 组件中保存完整 Arch Entity，并由具名领域 owner 校验/清理；这属于领域数据与业务规则。任何保留方案都必须说明为什么无需官方通用关系 API，不能以此为理由重建通用框架。

关系验收至少涉及 source/target 销毁、同类型多关系、移除后重建、槽位复用、旧 token、World 释放和失败候选；所有尚未运行项具名列出。安装包本身不证明关系自动清理。

### N4：退出槽位表与实体包装中的重复 ECS 职责

Arch Entity 的 Version 负责运行时实体代次；删除 `RuntimeEntityHandle` 和实体记录中的另一套通用 generation。协议槽位、固定容量、旧 NPC 扫描顺序、Projectile identity 以及槽位复用冷却仍有业务消费者，不能用 Entity.Id 替代。

T2/T3 清点槽位表的 State、generation 和映射。最终槽位记录只能保存投影所需的完整 Arch Entity、协议信息、顺序及明确的复用控制；不能继续承载泛型组件表、通用实体有效性或第二份玩法状态。若保留槽位代次，必须证明其区分的是槽位绑定/请求代次，且仍以 Arch 存活校验作为实体有效性依据。

T4 删除 RuntimeNpcEntity/RuntimePlayerEntity 中通用 Capture/Edit/Has/Attach 路径及长期组件聚合。必要的边界视图、加载数据和只读值快照可以保留；系统字段更新使用短期原生 ref/Set，不能把旧所有访问方法机械改名成新的 Arch facade。

### N5：结构变化、查询与版本保护不再自建框架

实体 Create/Destroy、Add/Remove、Get/Set、筛选和批量更新直接使用 Arch 核心 API。已有旧存储/Match/Edit/通用 snapshot 删除要求继续有效，并覆盖新命名的等价实现。

需要延迟的存储结构变化使用已验证的 `Arch.Buffer.CommandBuffer`；安全且必须立即可见的路径直接调用原生结构 API。不得另建一个 `EcsCommandBuffer` 仿制 Add/Remove/Destroy。CommandBuffer 回放按 Create→Add→Set→Remove→Destroy 分组，不是 FIFO、事务或权限服务；网络请求队列、物品预留和 gameplay command 仍按原业务语义处理。

Arch 不提供旧组件 attachment/data revision、借用计数和深复制契约。删除这些通用协议后，确有消费者的物品过期提交保护、领域 revision 和防御复制必须保留；不为全部组件再维护全局版本表。

## 4. 其他 Arch 能力的采用边界

| 能力 | 本轮结论 | 退出/验证要求 |
| --- | --- | --- |
| Arch.Core 与 Arch.Buffer | 必选 | 全部存储、身份、访问、查询和适用结构缓冲直接采用 |
| Arch.System 1.1.0 | 新增必选 | 通用系统接口与组管理退出自制实现 |
| Arch.System.SourceGenerator 2.1.0 | 适合重复 query 样板时采用 | 手写 World.Query 也是原生 API；不另造查询生成器 |
| Arch.Relationships | 官方优先，兼容性有待探针 | 通用图机制不自建；领域关系数据按 N3 分类 |
| Arch 原生 ECS 事件 / Arch-Events | 需要 ECS 生命周期通知时验证采用 | Release 符号、重入、订阅释放、World 隔离需要证据 |
| Arch.EventBus | 有明确事件用途才采用 | 静态生成的 Hook/Unhook 不天然具有多世界作用域 |
| ParallelQuery / SharedJobScheduler | 有已测并行需求时采用 | 不自建并行查询框架；它们不提供业务系统自动 DAG |
| Arch.LowLevel.Resources | 实际外部资源集合才使用 | 不替代 Entity/世界 token/世界单例组件 |
| Arch.Persistence | 不替换现有 WorldFile 边界 | .wld、DTO、Codec、文件端口仍按既有行为验收 |
| PURE_ECS | 本轮不启用 | 多世界边界需要 WorldId 等信息 |

此次未定位到生产中已实现的通用 ECS EventBus 或泛型 World ResourceManager。平台事件、网络订阅、API bindings 字典不是同一机制，不能按名称批量删除。后续退出审计若发现新的通用 ECS 重实现，必须归入本次要求，而不能以本文没有列出类型为理由保留。

## 5. T1–T5 增量交付

| 长线 | 增强内容 | 新增交接 |
| --- | --- | --- |
| T1 | System 生命周期/Group；关系发布兼容；Events 配置；实际包/程序集闭包 | 官方能力与项目业务边界表、兼容性结论及未运行项 |
| T2 | 世界单例组件；IsFresh/候选加载；实体与槽位代次区分 | 世界组件及槽位权威状态合同 |
| T3 | 关系官方 API 接线；父子冗余状态退出；销毁/候选清理 | 关系来源/目标/清理矩阵与扩展适用性说明 |
| T4 | 周期系统实现官方接口；原生访问；包装和重复 query 样板退出 | System 注册/顺序/副作用矩阵及实际调用闭包 |
| T5 | 官方 System 生命周期和逐边界停止接线；Release 核验；全面退出审计 | 默认宿主证据、残留扫描及旧框架删除闭包 |

保留原执行合同的依赖顺序、测试预算和删除门禁。新要求没有授权本审计会话覆盖并行代码、停止其他会话、修改其任务目标或直接删除尚在使用的旧框架。已生成的旧交接只证明当时覆盖范围，不能自动满足新增 System/单例/关系条件。

## 6. 最终完成条件

1. 当前生产默认路径实际创建并访问 Arch World、完整 Arch Entity 和原生组件；注册了包不等于迁移完成。
2. 所有纳入支持集的通用 ECS 机制使用已验证的官方 API；没有第二套实体 allocator、组件 store/cell、query/edit/snapshot facade、System 接口/组、结构缓冲或通用关系图。
3. 世界权威组件和实体玩法组件只有一个 Arch 状态来源；外部槽位、DTO、快照及加载输入按职责分类，没有双写。
4. 宿主使用 Arch.System 的实际生命周期，已验证阶段顺序、同 tick 可见性、世界切换中途停止、异常及零 tick 退出；无跨帧/结构变化可写 ref。
5. 关系/事件的实际版本、构建配置与清理行为有证据；不将源码声明的版本冒充可安装版本。
6. 源码、项目引用、默认装配和测试迁移共同证明旧框架退出。历史文档和只读分类参考可保留；生产通用 ECS 残留不能通过“兼容”例外宣布全量完成。
7. 领域 UUID、世界 token、网络/连接 epoch、玩法生命周期、物品提交保护、协议槽位和 .wld 行为仍通过各自验收。Arch 没有提供的业务规则不伪装成已由框架解决。
8. 未测试、未接线及不兼容项保持 partial/unknown/not-run；不以静态审计、局部探针或构建成功声称生产迁移完成。

## 7. 官方来源与复核范围

- [Arch 2.1.0 固定源码](https://github.com/genaray/Arch/tree/04d52e7268eb6f376ca4841a0c204334120c5e9d)。
- [Arch.Extended 固定源码](https://github.com/genaray/Arch.Extended/tree/18d1e4c1faec6c83335434969933601b346d19fc)：Systems.cs、关系 csproj 与 WorldRelationshipExtensions.cs、Resources.cs、EventBus 生成器。
- [Arch.System 1.1.0](https://www.nuget.org/packages/Arch.System/1.1.0)及[SourceGenerator 2.1.0](https://www.nuget.org/packages/Arch.System.SourceGenerator/2.1.0)。
- [Relationships 发布版本索引](https://api.nuget.org/v3-flatcontainer/arch.relationships/index.json)与[1.0.1 包](https://api.nuget.org/v3-flatcontainer/arch.relationships/1.0.1/arch.relationships.1.0.1.nupkg)：只读内存检查 nuspec，未 restore/install。
- [Arch-Events 发布版本索引](https://api.nuget.org/v3-flatcontainer/arch-events/index.json)与[2.1.0 包](https://api.nuget.org/v3-flatcontainer/arch-events/2.1.0/arch-events.2.1.0.nupkg)：只读内存检查 nuspec、repository commit 与 Arch.dll 资产名称。

以上外部来源支持版本与 API 机制事实，不证明它们已在 NLTX net10.0 下通过兼容或业务测试。
