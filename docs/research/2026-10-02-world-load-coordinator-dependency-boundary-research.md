# 世界加载协调与依赖组合边界研究

文档 ID：DOC-2026-10-02-world-load-coordinator-dependency-boundary-research  
逻辑域：research  
产物类型：research  
状态：draft  
范围：核对世界加载工作流协调、Infrastructure 适配器与宿主组合根的职责和依赖方向。  
证据入口：[ECS 领域与基础设施边界](../../Context/架构设计/ECS领域与基础设施架构边界.md)、[世界存档协调器设计](../architecture/2026-10-02-world-storage-load-coordinator-design.md)、[WorldLoadLifecycleSystem.cs](../../src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldLoadLifecycleSystem.cs)、[WorldSavedOreTierLoadSystem.cs](../../src/NSSLC/Component/WorldSession/WorldSavedOreTierLoadSystem.cs)、[WorldFile.cs](../../src/NSSLC.Infrastructure/WorldStorage/WorldFile/WorldFile.cs)。  
canonical 路径：docs/research/2026-10-02-world-load-coordinator-dependency-boundary-research.md

## 1. 结论

世界加载的整体工作流不应由 `NSSLC.Infrastructure` 驱动。Infrastructure 负责文件、平台访问和格式适配；跨多个领域 owner 的加载流程应由应用用例协调代码组织；具体依赖实例由最高层宿主的组合根创建并连接。

需要区分两个经常都被称为“协调层”的职责：

1. **用例协调器**负责一次加载的顺序和结果：读取/解码、整体检查、向领域 owner 分发窄输入、汇总提交结果以及失败恢复。它依赖领域公开 API 与存储/codec 抽象，不应依赖具体文件适配器。
2. **宿主组合根（Composition Root）**负责构造对象图，把具体 Infrastructure 适配器绑定到用例所需的抽象，并提供领域上下文。它可以引用具体 Infrastructure 和 NSSLC 项目，是两者在运行时被组合的位置。

因此，若“整体协调层”的意思是由一个更外层入口同时引用 `NSSLC.Infrastructure` 和 NSSLC，这个方向成立；若意思是让两个项目都依赖一个新的协调层，则不成立。典型依赖关系为：

```text
Host / Composition Root -> Application Coordinator
Host / Composition Root -> Infrastructure Adapter
Application Coordinator -> NSSLC public domain APIs
Application Coordinator -> storage/codec port
Infrastructure Adapter -> storage/codec port implementation
NSSLC Domain -> no dependency on Infrastructure
```

协调器是否要成为独立 `NSSLC.Application` 项目，不能仅凭六边形架构决定。若现有宿主边界可以容纳这项用例，而且没有复用、独立测试或部署方面的真实收益，不必为层次完整而新建项目。若协调器需要在多个宿主复用，或需要与具体平台适配器独立测试，再把用例及其端口放到稳定的应用边界。

## 2. 外部资料

### 2.1 Microsoft：Clean Architecture 与依赖方向

Microsoft 的[常见应用架构](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)说明，Application Core 定义抽象，Infrastructure 提供实现；UI 与 Infrastructure 都依赖核心，通常不必彼此依赖。具体实现通过依赖注入接到核心抽象上。该资料支持“宿主组合双方、基础设施实现内层契约”，不支持把跨层用例流程放进基础设施项目。

Microsoft 的[微服务基础设施持久化层设计](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)也将持久化抽象放在领域模型侧、将仓储实现放在 Infrastructure 侧，并指出这种安排可避免应用层直接依赖基础设施实现。

Microsoft 的[应用层实现指南](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/microservice-application-layer-implementation-web-api)把应用层描述为用例协调位置：处理器组织命令与领域模型，基础设施服务通过依赖注入提供。应用层可以与宿主程序集同处，也可以单独成项目；这说明层次职责并不强制对应独立程序集。

### 2.2 原始架构说明：用例协调与组合根

Robert C. Martin 的[Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)将 Use Cases 描述为应用专属规则，并指出它们编排数据进出实体。这对应世界加载中的跨 owner 流程协调，不意味着文件适配器拥有该流程。

Mark Seemann 的[Composition Root](https://blog.ploeh.dk/2011/07/28/CompositionRoot/)将组合根作为组合对象图的唯一入口，并建议它位于应用入口附近，而不是各类库内部。这对应由游戏宿主、服务器入口或其他实际启动方装配 Infrastructure 与领域能力。

## 3. 与本仓库世界加载设计的对应

- [ECS 领域与基础设施边界](../../Context/架构设计/ECS领域与基础设施架构边界.md)已规定：应用协调层编排快照、编解码、校验、存储调用和结果；Infrastructure 执行文件/平台效果；组合入口创建并连接领域、用例和适配器。该约束不要求固定项目数量。
- [世界存档协调器设计](../architecture/2026-10-02-world-storage-load-coordinator-design.md)选择协调器临时持有完整载荷并按 owner 分发；同时明确不新增独立 `NSSLC.Application/WorldStorage` 子系统，协调器应落在实际宿主/组合边界，待确认真实项目归属。
- `WorldLoadLifecycleSystem` 管理加载尝试、重试、备份恢复和完成通知，不负责文件解码及多 owner 数据提交。因此，世界加载用例协调器应接入其生命周期，而不是把文件格式和完整载荷责任放入该 ECS System。
- `WorldSavedOreTierLoadSystem.Load` 接收 `IWorldFileSavedOreTierAdapter`，展示了领域加载行为依赖适配器契约的局部形态；它不能证明跨 owner 的完整加载已具备原子性。
- 当前 `WorldFile.cs` 对 `Main`、`WorldGen`、路径及运行时状态有大量直接访问，是导入的旧式加载实现，不是已经解耦的纯 codec。目标协调器设计不能把它未经改造地当成满足端口契约的 DTO 解码器。
- 本次检查没有在源码项目引用中确认一个明确同时组合世界文件代码与 `Terraria.WorldStorage` / `Terraria.WorldSession` 的宿主项目。因此可以确定职责方向，但协调器的最终程序集归属仍需由真实启动/调用入口和项目引用图决定。

## 4. 建议

1. 保留 `WorldLoadLifecycleSystem` 的生命周期职责，并让宿主入口在既有阶段调用一次普通 .NET 世界加载协调器。
2. 将加载顺序、全局校验、API 分发和跨 owner 失败语义放在应用用例/组合边界；不要由 Infrastructure 决定哪些领域 owner 被加载以及何时提交。
3. 让文件 codec 与平台存储只执行显式输入输出及外部效果。若协调器处于独立 Application 项目，Infrastructure 实现应用定义的端口；若暂不拆 Application，则先把协调器放到真实宿主组合处，避免领域程序集引用 Infrastructure。
4. 把具体类型注册和实例创建集中在宿主组合根。Infrastructure 可以提供实现和便捷注册扩展，但扩展不应暗中启动/驱动世界加载业务流程。
5. 项目拆分以前先检查真实项目引用、生产入口和测试替换需求。只有当独立应用项目实际改善依赖方向、工作流测试或复用时才创建它；本地已确认的设计也明确反对只为补齐架构模板而建空层。

## 5. 边界与限制

Microsoft 文档主要用 Web API、仓储和数据库说明这些架构规则。它们能支持依赖反转、应用协调与依赖注入组合的原则，但不能替代本仓库对 Terraria 加载生命周期、System API 可访问性、多个 Store 的部分提交和重建语义的验证。当前世界加载协调器文档仍是设计草案，不表示相关 API、codec 或宿主调用已经实现。

Brave Search skill 在当前环境没有配置 `BRAVE_API_KEY`，无法用该服务完成搜索请求。本次通过直接访问 Microsoft Learn 及架构原作者页面进行核验，返回页面均可访问；引用只用于校验原则，不作为本仓库实现完成的证据。

## 6. 来源

- Microsoft Learn, [Common web application architectures](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- Microsoft Learn, [Designing the infrastructure persistence layer](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- Microsoft Learn, [Implementing the microservice application layer using the Web API](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/microservice-application-layer-implementation-web-api)
- Robert C. Martin, [The Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- Mark Seemann, [Composition Root](https://blog.ploeh.dk/2011/07/28/CompositionRoot/)
