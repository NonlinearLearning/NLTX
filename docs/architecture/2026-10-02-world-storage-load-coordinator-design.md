# 世界存档协调器与临时载荷设计

文档 ID：DOC-2026-10-02-world-storage-load-coordinator-design  
逻辑域：architecture  
产物类型：design  
状态：draft  
范围：本地世界文件的完整载荷生命周期、Infrastructure 编解码、System 标准加载 API 的构建期收集与生成分派、ECS owner 提交及保存快照编排。  
证据入口：[WorldFile.cs](../../src/NSSLC.Infrastructure/WorldStorage/WorldFile/WorldFile.cs)、[WorldStorageRoot.cs](../../src/NSSLC/Component/WorldStorage/WorldStorageRoot.cs)、[WorldLoadLifecycleSystem.cs](../../src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldLoadLifecycleSystem.cs)、[WorldSavedOreTierLoadSystem.cs](../../src/NSSLC/Component/WorldSession/WorldSavedOreTierLoadSystem.cs)、[架构边界约束](../../Context/架构设计/ECS领域与基础设施架构边界.md)、[CR-2026-10-02](../cr/CR-2026-10-02-world-load-standard-api.md)。  
canonical 路径：docs/architecture/2026-10-02-world-storage-load-coordinator-design.md

System 加载协议和构建期发现规则由[标准加载 API 专项设计](2026-10-02-world-load-standard-api-design.md)细化；其分阶段实施门槛见[执行计划](execution/2026-10-02-world-load-standard-api-execution.md)。

## 1. 决策

一次世界加载尝试解码出的完整持久化数据由普通 .NET 协调器临时持有。协调器完成全局结构校验后，将局部输入交给相应 ECS 领域 owner；owner 才能验证领域不变量并更新自己的 Store。加载完成或失败清理后，协调器放弃载荷引用。

此方案的“整份”表示一个带强类型分段的对象图，拥有者是一次工作流中的协调器。它不要求所有字段塞进一个扁平类，也不表示同一份聚合对象要被逐个 System 共享。

每个需从世界存档恢复状态的 System 实现同一套标准加载 API，并声明其唯一 API 身份、所属 owner 和强类型输入 section。协调器的 API 目录由构建阶段收集声明后生成；加载阶段直接调用生成目录中的 API。不同 owner 可使用不同的输入和准备结果类型，但必须遵循相同的预检/准备、提交、结果和失败协议。

构建期收集与运行时执行必须分开：生成阶段发现和校验 API，运行时执行生成的直接调用代码，不扫描程序集、不使用反射，也不依赖逐个手写注册。生成器的具体实现方式仍待检查项目引用和构建工具链；不得引入外部库，生成源码须落在 Build/generated/。

保存/加载用例协调器归入已创建的 `NSSLC.Application`，不再在 Infrastructure 内驱动跨 owner 工作流，也不另建第二个 Application 或 feature-local `/WorldStorage` 项目。Application 定义用例所需的持久化/codec 端口并调用 NSSLC 公开领域 API；Infrastructure 实现这些端口并负责文件和格式效果。Application 不引用 Infrastructure 具体实现。

宿主组合根是另一项职责：它引用 Application 与具体 Infrastructure，实现端口绑定、协调器实例构造和 owner context 提供。`NSSLC.Application.csproj` 与 `NSSLC.Infrastructure.WorldStorage.csproj` 已建立，后者引用 Application 并实现本地文件端口；真实生产宿主及其项目引用仍需核对。不得因此让 NSSLC 领域反向依赖 Application 或 Infrastructure。

文件编解码使用 NSSLC.Infrastructure/WorldStorage/WorldFile/ 下现有格式代码的职责归属。其当前旧式实现会直接访问 Main、WorldGen 等状态，不能未经改造就视为纯解码器。拆分后的 codec 必须通过显式输入输出工作，不直接查找 ECS 状态或提交 Store。

## 2. 方案比较

| 方案 | 结果 | 决定 |
| --- | --- | --- |
| 解析器边读边写全局/Store | 文件格式、全局状态与多个 owner 交织；失败后难以判断已写入范围。 | 保留为待拆解的旧行为，不作为目标设计。 |
| 把整份数据放入 ECS Component 或 WorldStorageRoot | 临时 I/O 数据与权威运行时状态混合，加载重试和内存生命周期成为世界状态的一部分。 | 不采用。 |
| 协调器临时持有完整 DTO，再向 owner 分发窄输入 | 可以在提交前做整体验证，数据生命周期独立于 ECS；依赖边界在组合入口显式连接。 | 采用。 |
| 各 System 声明标准加载 API，构建阶段收集并生成调度目录 | API 发现与协调调用不依赖运行时扫描；输入按 owner 保持类型安全；构建期可以校验 API 契约。 | 采用。 |

## 3. 角色和责任

| 角色 | 责任 | 不负责 |
| --- | --- | --- |
| World file codec / Infrastructure | 实现 Application 定义的文件/codec 端口；负责文件格式、字节流、版本解读、压缩、文件错误及备份效果，从显式数据生成/消费 DTO。 | 决定跨 owner 用例顺序、查询全局 ECS、写 Store、决定玩法不变量。 |
| 持久化载荷 DTO | 以强类型分段表示格式映射后的数据；表达传输值，不含服务、行为、文件句柄或 Store 引用。 | 作为运行时 authority 或长期缓存。 |
| `NSSLC.Application` 用例协调器 | 每次尝试创建/持有完整载荷；执行全局校验，通过应用端口读取/写出文件，按 owner 调用受控输入，汇总结果并协调失败清理。 | 依赖具体 Infrastructure 类型、持有文件句柄超出工作流、成为静态单例、将整个 DTO 放入组件。 |
| ECS owner / 标准 System 加载 API | 声明本 owner 所需的强类型数据 section；预检/准备领域规则，提交时只修改 owner 自己的权威 Store；返回标准结果。 | 文件访问、格式版本判断、整份存档访问、其他 owner 的内部写入。 |
| Build-time API collector / generated catalog | 收集符合标准的领域 API 声明，校验唯一性、类型映射和依赖关系；把可编译的直接调用目录生成到 `NSSLC.Application` 消费程序集。 | 运行时状态修改、运行时反射发现、决定持久化字段语义、替代运行时依赖组合。 |
| Load lifecycle | 追踪当前加载/恢复阶段、重试次数、门状态及完成/失败结果。 | 保存或复用完整解码载荷。 |
| Host / composition root | 构造 Application 协调器和具体 Infrastructure 适配器；将适配器绑定到应用端口，提供 owner context，并连接加载生命周期。 | 承载文件格式细节、把依赖注册顺序伪装成 ECS 业务调度。 |

WorldStorageRoot 继续是运行时 Store 的组合入口。它不是临时 DTO 的存放点。领域快照和持久化 DTO 不因需要保存而变成组件。

类型归属遵循同一依赖方向：Application 拥有完整 `WorldPersistenceDocument` 和文件/codec 端口；各领域加载 API 消费的 owner section 类型属于 NSSLC 公开领域契约。文件格式 DTO 与 owner section 表示不同时，由 Application 工作流边界显式映射。Infrastructure 实现应用端口并生成/消费应用文档，不让领域 API 引用 Application 类型；这样避免 NSSLC 与 Application 形成项目循环引用。

## 4. 加载流程

~~~mermaid
sequenceDiagram
    participant Host as Host / Composition Root
    participant Life as WorldLoadLifecycle
    participant Coord as NSSLC.Application Coordinator
    participant Port as Application Codec Port
    participant Codec as Infrastructure Adapter
    participant Catalog as Generated API Catalog
    participant APIs as System Load APIs

    Host->>Life: 开始一次文件加载尝试
    Host->>Coord: 开始用例并提供 owner context
    Coord->>Port: 读取/解码请求
    Port->>Codec: 调用绑定的文件适配器
    Codec-->>Port: 格式结果
    Port-->>Coord: 完整 WorldPersistenceDocument
    Coord->>Coord: 全局结构/版本/边界校验
    Host->>Life: 解码成功后将加载门置为阻断状态
    Coord->>Catalog: 执行生成的准备/预检阶段
    loop 按生成目录的确定顺序
        Catalog->>APIs: PrepareLoad(强类型 owner section)
        APIs-->>Catalog: prepared / rejected
    end
    Coord->>Catalog: 全部准备成功后执行提交阶段
    loop 按声明依赖生成的确定顺序
        Catalog->>APIs: CommitLoad(owner 专属 prepared data)
        APIs->>APIs: 更新自己的权威 Store
        APIs-->>Catalog: committed / failed
    end
    Catalog-->>Coord: 聚合 API 结果
    Coord-->>Host: 加载用例结果
    Host->>Life: 执行既定加载后处理并结束加载门
    Host->>Life: 全部成功后完成尝试并通知已加载
~~~

### 4.1 载荷结构与所有权

使用描述性名称讨论对象契约：

~~~text
WorldPersistenceDocument
  WorldDescriptorData
  TileMapData
  EntitySlotData
  ContainerData
  SignData
  TileEntityData
  SectionData
  OtherFormatSections
~~~

这些名称是目标概念，不是当前已存在类型，也不是要求一比一新建类。最终字段清单必须逐个对应实际文件格式与 owner；并非所有运行时 Store 字段都应持久化。探索地图 .map 不在此对象图中。

协调器在当前尝试中持有载荷根引用。对每个 owner，只构造其允许的窄输入。提交后，Store 必须自行拥有稳定状态：可以防御性复制；也可以通过内部受控接口转移缓冲区所有权，但须保证协调器/DTO 不再修改该缓冲区、没有其它可变别名，并明确提交成功后的责任方。

### 4.2 标准 System 加载 API

加载 API 的协议统一，数据契约按 owner 强类型。概念形状如下；它描述契约，不承诺这些精确 C# 类型名：

~~~text
IWorldLoadApi<TOwnerContext, TSection, TPrepared>
  ApiId
  OwnerId
  SectionType
  SupportedFormatVersions
  SectionRequirement (required | optional)
  Dependencies
  PrepareLoad(TOwnerContext readOnlyContext, TSection section) -> Prepared<TPrepared> | Rejected(error)
  CommitLoad(TOwnerContext ownerContext, TPrepared prepared) -> Committed | Failed(error)
  DiscardPrepared(TPrepared prepared) -> release uncommitted staging resources
~~~

- TSection 是 codec 输出文档中的 owner 专属强类型 section。API 不接收 WorldPersistenceDocument 根对象，也不读取别的 owner 的 section。
- API 声明 section 所适用的世界文件版本和必需/可选策略。文件版本迁移和格式缺省值在分派前归一化；必需 section 缺失时整体加载在任何 owner 提交前失败，可选 section 以显式缺省值或 absence 输入表达。
- TOwnerContext 是该 API 所属 owner 的强类型运行时上下文，由组合入口提供并由生成目录按类型绑定；它不包含整个 WorldStorageRoot，也不暴露其他 owner 的可变 Store。
- PrepareLoad 在不改变权威 Store 的前提下完成 owner 校验及必要的暂存/规范化，并返回 owner 专属的准备结果。准备结果只在本次加载尝试内有效。
- 全部 API 准备成功后，协调器才进入提交阶段。CommitLoad 只改本 API 所属的权威 Store，并返回统一可判别结果。
- 准备阶段失败时，协调器通过 DiscardPrepared 释放所有已准备但未提交的数据。提交阶段失败可能已产生其他 owner 的提交；协调器不假定自动回滚，而是按世界级失败清理/重建策略处理。
- 有先后要求的 API 声明依赖关系。没有依赖关系的 API 按稳定 API ID 排列以保证生成结果可复现；不能依赖源文件枚举顺序表达业务先后。
- API 使用窄小、只读的 owner 依赖完成准备。提交 API 不提供访问其他 owner 内部 Store 的通用入口。

API 可以由 owner System 本身实现，也可以由该 System 暴露的专属加载端点实现；两者都必须出现在构建期收集结果中。所有声明必须被生成目录覆盖，不能只靠协调器手写调用。

### 4.3 校验与提交阶段

1. **文件与格式检查：**读取完整性、版本支持、字段结构和编码约束由 codec 负责。
2. **整体验证：**协调器验证跨字段关系、总量边界、索引引用和能在不依赖 Store 的情况下判断的结构约束。具体限值从旧格式和领域契约提取，不在本文猜定。
3. **加载门：**解码和整体校验成功后、owner 预检前，将世界置于加载状态，阻止模拟在预检和提交期间观察或改变半加载状态。
4. **领域预检：**生成目录调用每个 API 的 PrepareLoad，owner 在不写入权威 Store 的前提下检查输入，例如槽位范围、TileMap 尺寸、位置和类型关系。必须等全部 API 接受后才能开始提交。
5. **提交：**生成目录按已声明的依赖顺序调用 CommitLoad；不接受 codec 未校验的对象直接替换权威状态。运行时资源失败仍可能造成部分提交，需按失败语义清理或重建。

### 4.4 构建期收集与目录生成

构建期收集器读取参与组合的领域程序集中的标准 API 声明，并为协调器生成：

1. 所有 API 的唯一 ID、owner、上下文类型、section 类型、支持的文件版本、必需/可选策略和显式依赖信息。
2. 从解码文档对应 section 及组合入口提供的 owner 上下文，到 API 强类型输入的直接映射。
3. 确定、可复现的准备调用列表和提交调用列表。
4. 生成期诊断。至少覆盖 API ID 重复、接口签名不符合标准、section 或 owner 上下文没有映射或被多个 API 歧义消费、必需 section 无消费者、依赖引用缺失和依赖环。

生成代码负责直接分派；运行时协调器只持有本次文档、调用生成目录并汇总结果。禁止运行时反射扫描、程序集遍历或手工重复维护 API 清单。构建期收集器必须能观察组合入口所引用的各 System API；API 对组合入口不可访问时应给出构建诊断。

当前初步搜索未在 src/ 和 Build/ 发现已有 source generator/analyzer。实施前必须检查项目引用图和构建工具链，选定不新增外部库的生成方案，并将生成文件输出到 Build/generated/；具体 generator 项目/target 和跨程序集 manifest 方式未由现有证据决定。

### 4.5 Batch 命名语义

ChestLoadBatch 是说明“将一组 Chest 载入记录作为一次 owner 输入”时的示例名称，不是现存类型，也不是本设计要求新增的类型。Batch 只表达一批同类条目在一次 API 调用/提交边界内处理；它不意味着 ECS 命令缓冲、事务、跨 owner 原子性或后台队列。

如果接口接收的只是一个完整容器集合，名称可用 WorldContainerLoadInput、ContainerSnapshot 或项目现有词汇。只有确实存在多条同类记录的批量提交语义时，才选择 ...Batch。名称不能改变提交语义。

### 4.6 生命周期和重试

- DTO 与协调器都按单次尝试创建；不把载荷保存在 WorldLoadLifecycleComponent。
- 解码及整体校验成功后，将加载门置为阻断模拟状态；生成目录中所有 API 预检通过后才开始写入。全部 API 提交和加载后处理完成后才允许解除阻断并发出完成通知。
- 文件读取、解码或全局验证失败时，没有 owner 被触碰，可把明确失败结果交回既有重试/备份生命周期。
- owner 提交可能跨多个 Store。没有统一原子提交证据时，不能称为事务或可回滚。
- 如果某个 owner 在其他 owner 已提交后失败，则下一次尝试前必须执行整次失败世界状态的清理/重建，或者采用实现证明过的跨 owner 提交机制。未满足这一点时禁止自动重试并继续复用已部分提交的 Store。
- 尝试结束后，释放协调器、根载荷及仅用于转换的临时缓冲区引用；WorldLoadLifecycle 只保留生命周期元数据。

WorldLoadLifecycleSystem 当前表示主文件重试、备份检查/恢复、加载尝试和结果通知阶段，不负责文件解码或数据提交。`CompleteLoadAttempt(..., requiresWorldReset: true)` 和 `CancelRecovery(..., requiresWorldReset: true)` 为提交阶段部分写入或取消提供终止门，`WorldLoadLifecycleComponent.RequiresWorldReset` 在宿主完成清理并调用 `MarkWorldCleared()` 后清除。协调器应组合进该流程，而不是取代它或把流程阶段重复存入 DTO。

## 5. 保存流程

~~~mermaid
sequenceDiagram
    participant Host as Host / Composition Root
    participant Coord as NSSLC.Application Coordinator
    participant Owners as ECS Owners
    participant Port as Application Codec Port
    participant Codec as Infrastructure Adapter

    Host->>Coord: 开始保存
    Coord->>Owners: 在同一一致性边界请求持久化快照
    Owners-->>Coord: owner 专属不可变快照
    Coord->>Coord: 组装完整 WorldPersistenceDocument
    Coord->>Port: 编码并保存显式载荷
    Port->>Codec: 调用绑定的文件适配器
    Codec->>Codec: 写文件并执行既有备份策略
    Codec-->>Port: 保存结果
    Port-->>Coord: 保存结果
    Coord-->>Host: 明确成功或失败
~~~

所有 owner 快照必须对应同一世界状态边界。可以使用现有安全的暂停/调度屏障，或经确认的统一快照阶段；若只能依次读取且世界可能在读取间继续变化，就不能声称生成一致存档。具体屏障和线程语义目前未由相关源码证据闭合。

编码器只接收显式持久化 DTO（当前 Application 端口为 `WorldPersistenceDocument`）。它不接收文件路径或备份策略，也不通过 Main、静态单例或 ECS 查询补齐缺失字段。备份、临时文件和文件替换策略仍是协调器与 Infrastructure 文件端口的效果。

## 6. 依赖与类型落位

~~~text
Host / Composition Root
  -> NSSLC.Application (协调器、应用端口、生成目录)
  -> NSSLC.Infrastructure (端口实现)

NSSLC.Application -> NSSLC 公开领域 API
NSSLC.Infrastructure -> NSSLC.Application 端口/数据契约
NSSLC 领域 -> 不依赖 Application 或 Infrastructure
Infrastructure -> 文件系统和格式实现；不写 ECS 内部 Store
~~~

~~~mermaid
flowchart LR
    Systems[领域 System 声明标准加载 API] --> Collector[构建期收集与校验]
    Collector --> Generated[生成强类型 API 调用目录]
    Generated --> Coordinator[NSSLC.Application 协调器调用]
    Coordinator --> Sections[传入各 API 所需 section 与 owner 上下文]
    Sections --> Systems
~~~

`WorldPersistenceDocument` 根类型及 codec 端口由 Application 持有；owner API 所需的 `TSection` 由 NSSLC 领域公开契约持有。若磁盘格式 DTO 与 `TSection` 不同，Application 明确映射两者。Infrastructure 可依赖这些向内的公开数据契约，但 Application 不引用 Infrastructure 实现，领域项目也不引用 Application。

生成器收集的是可编译访问的 API 声明，不替代运行时依赖组合。组合入口仍负责提供世界文件 codec、各 API 所需的 owner 上下文和协调器实例；API 的发现和直接调用清单不由人工逐项注册。若 API 分布在多个程序集，构建方案须证明 Application 编译确实能读取这些声明并生成可访问调用。

`NSSLC.Application` 是这条流的用例归属，不再把协调器放在 Infrastructure 或 ECS 领域中。当前 src 中尚未确认同时引用 Application 与具体 Infrastructure 的生产宿主；宿主组合根的具体程序集仍须通过项目引用图确定。该边界不能通过让 Infrastructure 访问领域内部 Store 或让 ECS 反向引用 Application/Infrastructure 来绕过。

Application 现有 `WorldPersistenceDocument` 强类型分段载荷与解码/整体校验端口。`WorldLoadCoordinator` 在每次 Load 调用内读取字节、解码并持有文档、完成整体校验，再合并文档 section 与宿主 API/context 运行时绑定，通过 `IWorldLoadApiCatalog` 调用生成目录；成功 outcome 不保留文档引用，Commit 失败 outcome 标记 `RequiresWorldReset`。两参数 `Load` 默认返回 `WorldRecoveryStatus.Loaded`；备份替换成功后的宿主调用可通过三参数重载显式传入 `WorldRecoveryStatus.RecoveredFromBackup`，四参数重载还允许宿主在文件读取、解码、整体校验和目录分派边界传入 `CancellationToken`，协调器不根据路径猜测恢复来源，也不自行重试。`WorldLoadApiRuntimeBindingsBuilder` 可供组合根按 API 类型和 owner id 构造一次性运行时绑定快照。Infrastructure 的 `WorldFileStoreAdapter` 实现本地文件端口，`WorldStorageCoordinatorFactory` 为保存和加载都提供默认本地适配器及显式 `IWorldFileStore` 注入入口，便于组合根复用同一端口实例。协调流程已有一项 fixture 端到端正向验证。

这仍不是完整生产世界加载：当前 `WorldPersistenceDocument` 是通用 section 容器，没有从真实 `.wld` 格式到 owner section 的完整字段映射；正式项目已接入不依赖全局状态的 WorldFile decoder/validator/encoder，覆盖 header/environment/progression/quest/banner/boss-progression/party/sandstorm/defender-event/background/event/tree-tops/seasonal/npc-unlocks/time-policy/spawn、压缩 Tile payload、Chest/Sign/NPC、WeightedPressurePlates、TownManager、Bestiary、Footer，以及 TileEntity/CreativePowers 的有界原始 section。spawn section 读取额外出生点、dual-dungeons 标志和原始 manifest 文本；Tile payload 尚未展开为 owner tile cells，TileEntity/CreativePowers 仍由 owner 在 Prepare 阶段类型化解码。encoder 要求 Tile 与 Footer 存在并拒绝未知 section，以保持完整 pointer table 的 framing。`WorldFile/` 下旧代码仍直接访问 Main、WorldGen 等全局状态，未被正式项目编译。没有生产 System API 声明或生产 Host/lifecycle 调用工厂；加载门、提交后部分写入的世界级清理/重建仍须由真实宿主接入。正式项目不从 `分类参考` 编译或引用实现。

建议职责位置：

- src/NSSLC.Infrastructure/WorldStorage/WorldFile/：既有世界格式代码经解耦后的读写和文件效果。
- src/NSSLC/Component/WorldStorage/ 及具体领域目录：运行时权威 Store、快照读取和 owner 写入 API。
- 各领域 System 的标准加载 API 声明及其强类型持久化 section 输入。
- 构建期 collector/generator 属于 Build 工具边界；生成目录编译进 `NSSLC.Application`，并收集 Application 引用的所有领域 System API。
- `src/NSSLC.Application/`：普通 .NET 加载/保存用例协调器、应用端口和生成目录消费边界；不在 ECS namespace 下命名为领域 System。
- 实际组合/宿主程序集：实例化协调器与 Infrastructure 端口实现，提供 owner context 并调用用例；不承载文件格式实现。

这些是逻辑责任位置；不要求为没有真实依赖或调用入口的职责新建项目。

### 2026-10-03 WorldFile 后续区段加载边界

正式 decoder 已将完整 pointer table 中的 `TownManager` 和 `Bestiary` 映射为有界 DTO；`TileEntity` 与 `CreativePowers` 保留 section 原始载荷和数量/长度边界，避免依赖旧类型注册表。encoder 现在保留这些 section 的 framing 和原始载荷；owner 类型化展开、Tile 解压和生产生命周期接线继续作为后续阶段。

## 7. 数据和内存约束

- 持久化对象图只含明确存档数据；不含 file stream、平台客户端、组件引用、Store 或服务。
- 可变集合和数组不可从 DTO/owner API 泄漏到非所有者。
- TileMap 载荷可能很大，记录解析峰值、提交峰值及复制次数。不要为了“不可变”盲目重复复制全图。
- 缓冲区所有权转移必须是显式且单向的；一次提交失败后仍需明确由谁释放部分创建的 Store 数据。
- 完成、失败、取消和异常路径都释放暂存引用，不把它们缓存到全局变量、静态字段或生命周期组件。

## 8. 失败语义

| 阶段 | 可观察结果 | 是否已写入 owner |
| --- | --- | --- |
| 路径/文件读取失败 | 文件错误分类；依既有主文件/备份策略处理 | 否 |
| 格式或版本解码失败 | 明确格式错误；交由加载恢复流程决定能否重试 | 否 |
| 整体验证失败 | 拒绝载荷并报告字段/关系原因 | 否 |
| API 构建期收集失败 | 构建失败并指出重复/无效/不可访问/无映射的 API；不产生可运行的 API 调用目录 | 否 |
| API 准备/领域验证失败 | 报告 API ID、owner 和原因；协调器丢弃此前准备数据，不标记世界完成 | 否 |
| API 提交中异常 | 加载失败，不发送完成通知；禁止直接在不清理时重放 | 可能部分提交 |
| 文件保存失败 | 返回失败结果并保留符合旧语义的文件/备份状态 | 运行时 owner 不因编码失败而回滚 |

异常、取消、重复请求、保存结果未知和备份替换的精确兼容语义须在实现前对照旧代码梳理；本表不宣称旧行为已经完整迁移。

## 9. 当前源码证据与缺口

- [WorldFile.cs](../../src/NSSLC.Infrastructure/WorldStorage/WorldFile/WorldFile.cs) 是导入的旧式解析器；LoadWorld()、保存路径中可见 Main 等全局访问。因此它目前不是本设计中的纯 DTO codec。
- [WorldStorageRoot.cs](../../src/NSSLC/Component/WorldStorage/WorldStorageRoot.cs) 汇集运行时 Store，不能作为临时持久化载荷仓库。
- [WorldLoadLifecycleSystem.cs](../../src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldLoadLifecycleSystem.cs) 管理加载尝试和恢复阶段，不负责解析完整存档。
- [WorldSavedOreTierLoadSystem.cs](../../src/NSSLC/Component/WorldSession/WorldSavedOreTierLoadSystem.cs) 表示单一数据域的 read/repair/commit 形态，可作局部参照，不证明跨 owner 原子性。
- 当前多数 Store 尚无覆盖完整旧世界文件字段的批量载入契约；完整字段清单、生产调用入口和跨 owner 失败清理仍待实现证据。
- System 标准 API 契约、SDK Roslyn 构建期收集器、生成目录及 Application 通用载荷协调路径已有代码和 fixture 正向验证；Infrastructure 现在有不依赖全局状态的 WorldFile decoder/validator/encoder，覆盖完整 pointer table、压缩 Tile payload、实体/容器 section 和 Footer，但尚无完整 owner 字段映射、生产 System API 声明或生产 lifecycle 接线。schema 无消费者诊断和失败分支运行验证仍缺。

## 10. 分阶段实施建议

1. **格式盘点：**按旧 WorldFile 读写章节建立持久化字段到目标 owner 的映射，区分存档字段、派生索引和运行时暂态；不纳入探索地图 .map。
2. **DTO 与解码边界：**引入显式的临时持久化数据契约，让解码返回载荷而不是写入全局状态；确认不改变文件格式。
3. **标准 API：**按[专项设计](2026-10-02-world-load-standard-api-design.md)定义统一的 API 元数据和准备/提交协议，逐个 owner 声明强类型 section、唯一身份和必要依赖。
4. **构建期收集：**按[执行计划](execution/2026-10-02-world-load-standard-api-execution.md)确认无外部依赖的 generator/build-time collector，验证它能收集组合入口引用的全部 API，生成直接分派目录，并对重复、无效和缺失声明给出构建诊断。
5. **协调和恢复：**在 `NSSLC.Application` 实现普通用例协调器并调用生成目录；由实际宿主组合根提供端口实现和 owner context，接入整体校验、准备、提交、加载门、清理/重建、重试和完成通知。不得在领域层引入 Application 或 Infrastructure 依赖。
6. **保存切换：**按统一状态边界采集快照并由 codec 写出，保持备份和失败语义。
7. **行为验收：**比较旧/新格式输出与加载后 owner 状态，覆盖主文件、备份、损坏、版本、API 收集错误、owner 拒绝、部分提交失败和大世界内存峰值。未执行的验证明确标为未执行。

本文是设计，不是实施完成声明。保存/加载代码、重试和备份行为及 owner 字段覆盖都需在后续执行材料中独立验收。
