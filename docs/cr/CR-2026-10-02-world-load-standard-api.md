# CR-2026-10-02：世界加载标准 API 与构建期自动调度

状态：accepted（文档范围）  
级别：major（影响核心加载流程和构建期 API 发现）  
提出方式：用户对话  
提出日期：2026-10-02  
批准与排期：用户于 2026-10-02 指示“更新正式文档”，随后要求“编写设计文档和执行文档”；用户现已明确授权实现基础代码，并暂缓生产 System 接入，详见后续记录。  
范围：世界存档加载 API 标准、System 自声明、构建期收集、生成调用目录、加载期自动调度及失败语义。

## 需求原文

> 如果要想协调器从世界中加载你需要的数据那么双方必须遵守一套标准api
>
> system实现此标准api加载自己需要的数据,协调器会在编译阶段收集这些api并在加载阶段自动调用

## 变更内容

每个需要从世界存档恢复状态的 System 实现标准加载 API，并声明自己消费的强类型数据 section、版本范围及必需/可选策略。协调器所需的 API 清单在构建阶段自动收集并生成直接调用目录；实际加载时，协调器按生成目录自动调用 API。API 统一生命周期、结果、依赖声明及错误处理语义，但每个 owner 保持自己的强类型输入与权威 Store 写入边界。

构建期发现不得由运行时反射或手工逐项注册代替。无效签名、重复身份、section 映射缺失/冲突、缺少消费者及依赖环应尽可能在构建期报告。

## 影响评估

- **数据格式：**不要求改变世界文件二进制格式。
- **领域 API：**每个持久化 owner/System 需要实现标准协议并声明数据 section 与 owner 上下文；加载分成无权威状态写入的准备/预检阶段和受控提交阶段。
- **协调器：**由静态逐项编排转为调用构建期生成的 API 目录。
- **构建：**需要选择能跨组合入口依赖程序集收集 API 的构建期生成机制，并验证输出位于 Build/generated/。
- **依赖：**不新增外部库；当前初步搜索没有发现仓库现成的 Roslyn source generator/analyzer。
- **失败处理：**准备阶段失败时丢弃已准备输入；提交阶段失败仍可能部分写入，必须执行世界级清理/重建后才能重试。
- **影响文档：**[世界存档加载与保存协调器 PRD](../plans/tasks/2026-10-02-world-storage-load-coordinator-prd.md)、[世界存档协调器与临时载荷设计](../architecture/2026-10-02-world-storage-load-coordinator-design.md)、[标准加载 API 专项设计](../architecture/2026-10-02-world-load-standard-api-design.md)、[标准加载 API 执行计划](../architecture/execution/2026-10-02-world-load-standard-api-execution.md)。

## 决定与边界

本 CR 接受上述加载 API 方向，并授权本次同步正式设计文档。级别由对核心加载流程及构建边界的影响判定为 major。

以下内容仍由实施前的方案核验确定，不在本次文档变更中预设实现已经存在：

- 具体采用 Roslyn source generator、其他构建期代码生成，还是仓库内的等价机制。
- API 在不同领域项目中的可见性、跨程序集发现和生成代码访问方式。
- 标准 API 的最终 C# 声明形式及准备数据的内存所有权实现。
- 生成顺序的依赖元数据和现有宿主组合程序集归属。

## 验收

- PRD 明确规定每个持久化 owner/System 声明并实现标准加载 API。
- 架构设计区分强类型 API 契约、构建期收集和运行时直接调用。
- API 通过强类型 section 和 owner 上下文取得自己所需数据/状态访问，不接收整份载荷根或通用 WorldStorageRoot。
- 设计禁止运行时反射发现和手工维护逐项 API 注册表。
- 文档保留无外部库约束，并把现成生成机制、跨项目收集和生成输出路径列为实现核验项。
- 本次只更新文档；不宣称 API、生成器或运行时加载已实现。

## 后续文档范围记录

用户后续要求原文：“编写设计文档和执行文档”。本次据此新增标准加载 API 专项设计与分阶段执行计划，并同步总体设计、PRD、CR 和文档 manifest。该范围补充只增加正式文档交付物，不改变已接受的加载 API 决策，不授权实现源码或构建工具；执行计划所有实施阶段保持 `planned`，验证保持 `not-run`。

## 实现范围变更记录

用户本轮要求原文：

> 根据上述两个文档执行计划实现代码,暂时系统不实现标准api,只进行10%的测试

本轮授权实现标准 API 契约与构建期收集/生成基础设施；生产 ECS System 暂不实现或声明该 API。允许加入不代表生产 owner 的生成器 fixture，用于验证构建期收集和生成直调。当前 NSSLC 项目图尚无组合 WorldFile 与 ECS owner 的宿主，因此真实世界载荷分派和生命周期接线不在本轮已确认实施边界内；不得新建独立 `NSSLC.Application/WorldStorage` 用例/端口层来绕过该缺口。

测试限制按用户补充的“只进行 10%”执行；本轮仅选择一项代表性的生成目录冒烟验证，不运行完整回归。生成器编译、受影响生产项目编译属于构建验证，单独记录，不扩大为测试套件。若用户随后给出不同的 10% 计量口径，则按回复调整测试清单。

本轮仍须遵守既有决定：不新增外部依赖；不运行时反射或手写逐 System 注册；不触碰探索地图 `.map`、Steam 云存档或已删除的本地/云端移动函数；加载提交失败不得标为成功。实施状态与未做的 System/宿主接入将在执行文档中如实更新。

## 后续应用层项目边界变更（2026-10-02）

### 需求原文

> 我已创建NSSLC.Application根据上述内容更新prd和设计文档

### 分级与本轮范围

- **建议级别：**重大。项目边界及 Application、Infrastructure、领域与 Host 的依赖图发生变化。
- **本轮授权：**用户明确要求同步 PRD 和设计文档；本轮据此更新相关正式文档和本 CR，不实施代码或生产加载接线。
- **变更决定：**`NSSLC.Application` 作为世界保存/加载用例层，承载协调器、应用端口及生成 API 目录的消费边界。该决定取代前文“不得新增 `NSSLC.Application/WorldStorage` 项目或用例/端口层”的限制。

### 目标依赖边界

- `NSSLC.Application` 依赖 NSSLC 领域公开 API 和应用端口；它不依赖 `NSSLC.Infrastructure` 的具体实现。
- `NSSLC.Infrastructure` 实现 Application 定义的文件/codec 端口并执行外部效果；它不决定跨 owner 的加载/保存用例，也不写 ECS Store。
- Host / Composition Root 引用 Application 与 Infrastructure，创建并绑定适配器、协调器和 owner context。
- NSSLC 领域不反向依赖 Application 或 Infrastructure。生成 API 目录由 Application 消费并编译；构建期 API 收集不负责运行时实例组合。

### 影响与证据状态

已同步材料：[世界存档加载与保存协调器 PRD](../plans/tasks/2026-10-02-world-storage-load-coordinator-prd.md)、[世界存档协调器设计](../architecture/2026-10-02-world-storage-load-coordinator-design.md)、[标准加载 API 设计](../architecture/2026-10-02-world-load-standard-api-design.md)、[执行计划](../architecture/execution/2026-10-02-world-load-standard-api-execution.md)。

在该阶段的工作树检查中，`src/NSSLC.Application/` 为空且未发现 `.csproj` 或项目引用，因此当时只记录了目标职责，没有把编译项目、端口实现、生成器接入或真实 Host 组合当作已验证事实。后续状态见本 CR 的“后续协调器代码迁移”记录。

## 本轮实施结果记录

- 已增加 `Terraria.WorldStorage` 标准加载契约及 SDK Roslyn source generator；生成器能扫描宿主编译与引用程序集并生成强类型直接调用 catalog，生成输出遵循 `Build/generated/`。
- 非生产跨程序集 fixture 正向验证通过，依赖构建报告 14 条既有 WorldStorage 字段警告、0 错误，生成器与 fixture 无警告；行为测试仅执行预先列出的 10 个等权场景中的第 1 项。具体命令、输出、诊断覆盖缺口和未接入范围见[执行记录](../architecture/execution/2026-10-02-world-load-standard-api-execution.md)。
- 本次不声明生产 System 已实现 API，不接入 WorldFile 协调器或 lifecycle；无消费者 section/schema 映射静态诊断仍待组合元数据方案。

## 后续协调器代码迁移（2026-10-02）

### 需求原文

> 将协调器相关的代码移动到NSSLC.Application

### 实施范围与依赖决定

- 新建 `src/NSSLC.Application/NSSLC.Application.csproj`，将字节级保存/恢复协调器及其命令、策略、结果和编码/校验契约移入 Application。
- Application 通过 `IWorldFileStore` 和应用层结果类型表达文件读写端口；正式 Infrastructure 的 `WorldFileStoreAdapter` 实现端口并执行本地文件系统 I/O。`分类参考` 目录不承载生产实现，也不被正式 Infrastructure 项目引用。Application 不引用 Infrastructure。
- `WorldStorageCoordinatorFactory.CreateSaveCoordinator()` 与 `CreateLoadCoordinator(decoder, validator, apiCatalog)` 提供由宿主调用的绑定入口；两者使用正式 `WorldFileStoreAdapter`，将 Application 协调器接到 `IWorldFileStore` 本地文件端口；加载目录由组合根显式注入。
- 当前源码中没有生产宿主引用或调用该工厂；因此已完成 Infrastructure 适配器与协调器的可组合接线，尚未完成游戏运行时保存/加载入口接入。
- 协调器命令和调用不暴露云存档选择参数；不改变现有 WorldFile 格式代码。
- 该迁移不表示已实现完整世界载荷解码、标准 API 目录分派、owner 提交或生产生命周期接线。

### WorldFile 接入边界

现有 `WorldFile.LoadWorld()` / `SaveWorld()` 直接访问 `Main`、`WorldGen` 等全局状态且没有显式路径/载荷参数。本次只把字节级协调器绑定到 `src/NSSLC.Infrastructure/WorldStorage` 的本地文件端口，不将这些全局副作用入口伪装成 Application codec；WorldFile 的显式输入/输出解耦和编解码端口接入仍是后续工作。

### 验收与证据状态

Application 项目和正式 `NSSLC.Infrastructure.WorldStorage.csproj` -> Application 端口引用已创建。使用 .NET SDK `10.0.400` 执行 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，退出码为 `0`，警告 `0`、错误 `0`。输出位于 `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` 和 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。仓库当前没有生产宿主引用或调用 `WorldStorageCoordinatorFactory`，故运行时入口接线仍未完成。既定 10% 行为验证通过原有正向生成目录场景（`1/10`），该场景不验证文件保存/恢复。Host 组合根及 Application 对领域 API 的引用关系仍未确认。

## 后续完整载荷与协调器分派实现（2026-10-02）

### 实施结果

- Application 新增通用 `WorldPersistenceDocument` 和唯一 `SectionId` 的强类型 section 容器，以及显式解码、整体校验端口。Coordinator 在一次 `Load` 调用中解码并临时持有该对象图，校验后构造绑定并调用 `IWorldLoadApiCatalog`；返回的 `WorldRecoveryOutcome` 不持有载荷。载荷没有放进组件、`WorldStorageRoot` 或生命周期状态。
- `NSSLC.Application` 编译 SDK Roslyn generator 并提供 Application 消费编译的生成 catalog wrapper；Infrastructure 工厂绑定本地字节文件适配器，并接收组合根提供的 `IWorldLoadApiCatalog`、decoder 与 validator。工厂不再固定选择可能为空的 Application catalog。生成 catalog 的 namespace 按消费程序集名区分。
- 新增 assembly-level `WorldPersistenceSectionSchemaAttribute`，由 API 消费编译声明所选 document codec 的规范化 section ID、强类型 DTO 和必需性。生成器实现 WLA007-WLA012，检查无效/重复 schema、API 缺失 schema、DTO 类型或 requirement 不匹配及必需 section 无消费者；诊断位于消费编译源码。本轮只构建匹配 schema 的正向 fixture，未执行负向诊断验证。
- Fixture verification host 通过 `TransitiveApis` 的传递项目引用获得两个独立 API assembly，在自身编译期生成 direct-call catalog，并通过 Infrastructure factory 注入 Application 协调器。唯一运行场景把 fixture 字节写入系统临时文件，由正式 `WorldFileStoreAdapter` 读取并解码 `fixture.section`，经整体校验、绑定预检、Prepare、Commit，验证 owner 状态、Optional Absent 输入及 `CommitAfter` 顺序，并检查加载成功结果；场景结束后清理临时文件。
- Application 消费程序集目前没有生产 System API 声明，因此其自身生成 catalog 为空；若调用方将此空 catalog 传给协调器，协调器会在开始 API 调用前以 `MissingApiDeclarations` 失败，不把单纯解码误报为世界加载完成。Infrastructure factory 现在要求组合根显式传入 catalog。Commit 阶段失败通过 outcome 的 `RequiresWorldReset` 暴露给外层，但实际 Host/lifecycle 清理与重建尚未接线。

### 验证

- 本轮用 SDK `10.0.400` 重跑验证宿主构建：`dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`，错误 `0`；宿主输出位于 `Build/bin/Terraria.WorldLoadApiGenerationVerification/Debug/net10.0/Terraria.WorldLoadApiGenerationVerification.dll`，Infrastructure 输出位于 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。新增项目引用后按需执行 `dotnet restore Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --verbosity minimal`，退出码 `0`。
- `dotnet run --project Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-build --no-restore`：退出码 `0`，输出 `World load API generation verification passed.`。该单一场景检查 optional section 在 Prepare 到 Commit 间保持 Absent，并验证 `CommitAfter` 顺序；场景数仍为 `1/10`。
- `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`：SDK `10.0.400`，退出码 `0`，警告 `0`，错误 `0`；Application 与 Infrastructure 输出均位于 `Build/bin/`。

### 未完成范围

本次实现的是通用强类型载荷容器和 API 调度边界，不是旧 `.wld` 的完整数据模型或编解码迁移。实际 WorldFile 字段/section/owner 映射、去除 `Main`/`WorldGen` 全局访问的 codec、生产 System 标准 API、生产宿主组合、加载门接入以及部分提交失败后的实际世界清理/重建仍未完成。执行计划的阶段 4 继续保持 `partial`。

## 加载尝试与恢复职责校正（2026-10-02）

静态对照总体设计第 4.6 节后，撤回了 `WorldLoadCoordinator` 自行循环重试和读写 `.bak` 的实现。当前 Application coordinator 对调用方指定的单一路径执行一次读取、解码、整体校验和生成目录调用；`WorldRecoveryOutcome.Attempts` 只表示本次调用的读取次数。协调器不再接受 `WorldRecoveryPolicy`，该策略类型已删除。

主文件重试、备份检查/恢复和加载阶段由 `WorldLoadLifecycleSystem` 决定。它目前只提供状态转换与动作结果，生产 Host 尚未调用 coordinator 或执行备份文件效果；因此 lifecycle 与 coordinator 的运行时接线仍未完成，不能把恢复流程记为已实现。此前 1/10 正向行为场景已通过；用户随后要求停止测试，本次只做静态核对与编译验证，不再运行行为测试。

## 继续实施：结果契约与备份来源标识（2026-10-03）

- `WorldLoadCoordinator` 保留两参数主文件加载入口，并新增带 `WorldRecoveryStatus` 的重载；宿主在备份替换成功后可以显式标记 `RecoveredFromBackup`，协调器不从路径名推断来源，也不重复执行生命周期的备份/重试职责。
- 自定义目录返回非法执行阶段时，Application 归一化为 Commit 失败以保留部分提交的重置要求，同时复制原始 `Exception` 和 `CleanupException`，保留取消与结果未知信息。
- `WorldLoadApiExecutionResult.Failed` 现在拒绝无效错误对象，与 Prepare/Commit 结果契约一致。
- 使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture 均成功；Infrastructure 依赖链报告 14 条 `Terraria.WorldStorage` 既有字段警告、0 错误，fixture 报告 0 警告、0 错误。没有运行行为测试，仍保持 `1/10` 预算；生产 System、真实 WorldFile codec 和 Host/lifecycle 接线仍按用户要求暂缓。
- `WorldStorageCoordinatorFactory` 现在为保存协调器提供显式 `IWorldFileStore` 注入重载，使组合根能够复用同一文件端口实例；无参数保存入口仍使用正式本地适配器。
- `WorldLoadCoordinator` 将目录描述符预检的枚举异常归一化为 Binding 失败；取消异常仍保留为 `Canceled`，且任何 API 调用前不会进入 Prepare。

### 保存端口结果契约归一化（2026-10-03）

静态检查发现保存协调器对编码器与文件端口返回的矛盾结果缺少统一处理。现已在 Application 边界拒绝“成功同时带失败”、成功读取却没有字节以及失败结果携带字节的编码/读取结果；编码器失败但无错误归一化为 `InvalidData`，文件端口失败但无错误归一化为 `IoFailure`，避免继续使用未验证字节或错误地执行备份轮换。正常失败分类和取消语义保持不变；未新增外部依赖、生产 System API 或行为测试。

验证命令：`dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal` 与 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，均退出码 `0`、警告 `0`、错误 `0`；输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。行为预算仍为 `1/10`。

### 零备份策略边界修正（2026-10-03）

`WorldSaveCoordinator` 现在在 `WorldBackupPolicy.BackupsToKeep=0` 时跳过备份轮换写入，避免无备份配置仍创建新的 `.bak`；主文件的编码、写入、读回和校验流程保持不变，已有旧备份不由本次保存清理。Infrastructure 与生成器 fixture 的串行增量编译均为退出码 `0`、警告 `0`、错误 `0`，没有运行行为测试。

### 加载读取结果契约归一化（2026-10-03）

`WorldLoadCoordinator` 现在拒绝文件端口返回的矛盾读取结果：成功带失败、成功无字节或失败携带字节均归一化为 `InvalidData`，失败无错误归一化为 `IoFailure`；正式适配器的正常错误分类和取消映射保持不变。Infrastructure 与生成器 fixture 串行增量编译均为退出码 `0`、警告 `0`、错误 `0`，没有运行行为测试。

### 聚合清理取消语义修正（2026-10-03）

`WorldLoadCoordinator` 现在识别 API 执行异常和 `DiscardPrepared` 清理异常中的取消，包括生成目录产生的 `AggregateException` 内层取消；这类结果保持 `WorldRecoveryStatus.Canceled`，Commit 阶段仍要求世界清理/重建。Infrastructure 与生成器 fixture 串行增量编译均为退出码 `0`、警告 `0`、错误 `0`，没有运行行为测试。

同一递归判定也用于文件读取和文档解码异常，聚合取消不会被降级为普通 I/O 或格式失败。

保存协调器的编码、读回校验和文件端口包装同步使用该判定，确保保存取消语义与加载一致。

### 空生成目录的显式拒绝（2026-10-03）

Application 当前尚无生产 `[WorldLoadApi]` 声明，因此生成器会为 Application 消费编译生成空 catalog。静态检查发现空 catalog 的直接调用原先会返回成功，可能掩盖“没有 API 声明”的组合错误。现已修改 `WorldLoadApiCatalogGenerator`：零声明目录的 `Execute` 生成 `Binding` 阶段 `MissingApiDeclarations` 失败；Application 协调器自身仍会在任何 API 调用前拒绝空 descriptor。该修正不引入生产 System API，也不改变参考目录、WorldFile codec 或 Host 尚未接入的范围。

使用 SDK `10.0.400` 串行编译 Infrastructure 和生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。已静态确认 Application 生成源码包含 `MissingApiDeclarations`。本次没有运行行为测试，行为预算仍为 `1/10`。

### 提交结果矛盾状态收紧（2026-10-03）

`WorldLoadCommitResult` 现在不再暴露可构造矛盾状态的两参数公开构造函数，仅允许通过 `Committed()` 或带有效错误的 `Rejected(...)` 创建结果。生成目录在处理提交结果时额外检查 `Failure.IsValid`；如果出现成功与失败同时存在的结果，按 Commit 失败处理并清理尚未提交的 prepared data。该修正只强化标准 API 结果契约，不引入生产 System API、真实 codec 或 Host 接线。

使用 SDK `10.0.400` 串行编译 Infrastructure 和生成器 fixture：Infrastructure 退出码 `0`、警告 `14`、错误 `0`（既有 `Terraria.WorldStorage` 字段警告）；fixture 退出码 `0`、警告 `0`、错误 `0`。本次没有运行行为测试，行为预算仍为 `1/10`。

### 失败阶段与提交结果不变量（2026-10-03）

`WorldLoadApiExecutionResult.Failed` 现在拒绝 `None` 或未定义的失败阶段，防止失败结果丢失生命周期位置。生成目录对提交结果的矛盾分支统一返回 `InvalidCommitResult`，避免把“成功同时带错误”作为 owner 的真实拒绝原因。该修正只强化契约和生成边界，不增加生产 System API。

使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture：Infrastructure 退出码 `0`、警告 `14`、错误 `0`；fixture 退出码 `0`、警告 `0`、错误 `0`。本次没有运行行为测试，行为预算仍为 `1/10`。

### 基础设施聚合取消分类一致化（2026-10-03）

`WorldFileStoreAdapter` 的异常分类现在递归检查 `AggregateException` 内层取消，与 Application 的取消语义一致。读写、删除、备份检查和恢复路径返回的聚合取消均归类为 `Canceled`，不再被包装为普通 I/O 失败。该修正不改变重试、备份选择或生产 Host 的未接入边界。

使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；Application、Infrastructure 和 fixture host 程序集输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次没有运行行为测试，行为预算仍为 `1/10`。

### 运行时目录失败身份保留（2026-10-03）

运行时目录校验现在返回具体失败 descriptor 的 `ApiId/OwnerId`：重复身份、重复 section、缺少依赖和依赖环均可定位到 API；依赖环只选择实际环成员。协调器将该身份传入 Binding 失败结果，便于宿主记录和恢复决策。使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；本次没有运行负向目录行为测试。

多个环成员时按 `ApiId` 字典序选择诊断身份，运行时错误不会受自定义目录枚举顺序影响。修正后 Infrastructure 与生成器 fixture 仍为退出码 `0`、警告 `0`、错误 `0`；没有运行行为测试。

### 取消恢复不触发重试（2026-10-03）

`WorldLoadLifecycleSystem` 新增 `CancelRecovery`，用于协作式取消当前主文件或备份恢复尝试。该方法仅接受六个活跃阶段：`PrimaryLoad`、`PrimaryRetry`、`BackupCheck`、`BackupRestore`、`BackupLoad`、`BackupRetry`；取消时设置 `LoadFailed`，直接转入 `Failed` 并返回 `ReportLoadFailure`，不再安排主文件或备份重试。`NotStarted`、`Completed`、`Failed` 等非活跃阶段会拒绝取消。该修正使生命周期取消语义与 Application coordinator 的 `Canceled` 结果保持边界一致，实际 Host 映射和行为验证仍未接入。

使用 SDK `10.0.400` 串行增量编译 `WorldSession`、Infrastructure 和生成器验证宿主，三次退出码均为 `0`；WorldSession 保留 `14` 条既有 `ColorRgba` 冲突警告，Infrastructure 与 fixture host 为 `0` 警告、`0` 错误。产物位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次没有运行 fixture，行为预算仍为 `1/10`；生产 System API、真实 WorldFile codec、Host/lifecycle 接线和取消路径行为测试继续保持未完成。

### 生命周期保留取消状态（2026-10-03）

`WorldLoadLifecycleComponent` 新增 `LoadCanceled` 属性。`CancelRecovery` 通过 `SetLoadCanceled` 保留取消身份，同时设置 `LoadFailed`、进入 `Failed` 并返回 `ReportLoadFailure`；后续普通 `SetLoadResult` 会清除该标记，避免生命周期复用旧取消状态。该变化只增强生命周期结果可观察性，不声明生产标准 API，也不接入尚未存在的 Host 调度。使用 SDK `10.0.400` 编译 `WorldSession`，退出码 `0`、警告 `14`、错误 `0`；本次没有运行生命周期行为测试，行为预算仍为 `1/10`。

### 包装异常取消分类一致化（2026-10-03）

新增 `WorldStorageExceptionClassifier`，使 Application 递归识别普通 `InnerException` 和 `AggregateException` 内层的 `OperationCanceledException`。文件端口抛出的 `FileNotFoundException`、权限、路径和 I/O 异常也会在 Application 边界映射为稳定的 `Missing`、`PermissionDenied`、`InvalidPath` 或 `IoFailure` 类别；Infrastructure 适配器同步递归检查包装异常。备份恢复主操作失败后的临时文件清理若发生取消，则结果改为 `Canceled`，不再沿用主异常类别。该修正只强化失败分类，不改变 coordinator 的单路径读取、重试职责或 API 接入范围。

使用 SDK `10.0.400` 编译 Infrastructure 和生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；产物位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次没有运行行为测试，行为预算仍为 `1/10`。

### 应用结果不变量收紧（2026-10-03）

`WorldRecoveryOutcome` 构造边界现在校验状态枚举、尝试次数和成功/失败与 `WorldLoadApiExecutionResult` 的一致性，拒绝成功带失败、成功缺少成功 API 执行，以及失败携带成功执行的矛盾结果。`WorldSaveProjection` 同样拒绝已提交但带失败或拒绝但无失败原因的状态。该修正防止组合入口绕过协调器时误报成功，不改变文件重试、备份选择或标准 API 暂缓范围。使用 SDK `10.0.400` 编译 Infrastructure 与生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；本次没有运行行为测试。

### 保存命令显式携带持久化文档（2026-10-03）

`WorldSaveCommand` 现在必须携带 `WorldPersistenceDocument`，`WorldSaveCoordinator` 在编码前拒绝默认命令或缺失文档，避免编码器通过隐式全局状态寻找保存数据。该变化仅闭合 Application 的显式输入边界；各 owner 快照的统一采集、格式字段映射和真实 codec 仍未实现。使用 SDK `10.0.400` 编译 Infrastructure 和生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；本次没有运行行为测试，行为预算仍为 `1/10`。

### 编码端口收窄为文档输入（2026-10-03）

`IWorldSaveEncoder.Encode` 现在只接收 `WorldPersistenceDocument`。`WorldSaveCoordinator` 保留路径、备份策略和文件效果编排，编码器无法再从端口参数取得路径或备份决策。该修正强化 Application 与 Infrastructure 的六边形边界，不改变保存文件格式或备份流程。使用 SDK `10.0.400` 编译 Infrastructure 与生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；本次没有运行行为测试。


### WorldFile 读取扩展至版本 326（2026-10-03）

#### 需求原文

> 后续扩展到版本 326

#### 范围与影响评估

- **级别建议：**中度。将正式 WorldFile decoder 和 validator 的可读版本范围从 `88–319` 扩展至 `88–326`；不得只更改版本门而忽略 320–326 的头部字段、区段边界或 Tile/实体负载变化。
- **实现范围：**仅扩展读取与验证。保存端继续明确写出版本 319；本次不把 326 版本文档重写成 319，也不宣称 326 保存兼容。
- **依据要求：**优先使用 326 的正式/可信格式源码；若不可得，需以真实版本 326 `.wld` 的完整区段消费与现有有界校验结果作为有限证据，并记录其无法证明的内容。
- **既有加载边界：**生产系统尚无标准 API 声明，故该 CR 只验收 326 数据进入已校验的 `WorldPersistenceDocument`。Coordinator 后续是否能完成 owner 分派，按其实际缺失 API 结果单独记录，不得将解码成功等同于世界已加载。
- **测试范围：**延续此前“只进行 10% 测试”的约束，只用一个代表性的真实 326 世界文件进行一次有界解码/验证尝试；不运行完整测试集，不读取探索地图 `.map`，不使用 Steam 云存档。

#### 验收

- decoder 与 validator 接受经格式边界校验的 320–326 文件，同时继续拒绝高于 326 的版本。
- 版本 319 编码行为不因读取上限扩展而变化。
- 用一个真实样本记录版本号、解码结果、验证结果和失败/未完成边界；不宣称未覆盖的 API 分派或 owner 状态提交已完成。
- 实现只落在正式 `src/NSSLC.Infrastructure/WorldStorage/WorldFile/`，不修改 `分类参考`。

#### 实施结果与验证（2026-10-03）

正式 decoder/validator 的可读上限现在是 326，写出上限仍明确为 319。version 323 起的两个 lightning seed 布尔字段进入 WorldFileSpawnSection 并按格式顺序读取；原始 World Manifest 文本继续保留。validator 允许游戏模式值 0–3，encoder 对 version 319 拒绝非默认的新字段，避免丢失数据。

格式依据：本机 Terraria 1.4.5.8 的 WorldFile 读写方法显示版本 >=323 读取/写入 moreLightningSeed 与 noLightningSeed，随后读取/写入 Manifest；公开 tModLoader 1.4.5 分支的 [WorldFile.cs.patch @78e43ac](https://github.com/tModLoader/tModLoader/blob/78e43ac6cf5f187453749da632a3cf8c58533651/patches/TerrariaNetCore/Terraria/IO/WorldFile.cs.patch#L4-L8) 显示 vanilla reader 的上限为 326。

使用 SDK 10.0.400 执行 dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal：退出码 0、警告 0、错误 0；Infrastructure 与 Application 程序集位于 Build/bin/。唯一真实样本尝试为 src/World/科研.wld（版本 326，SHA-256 47BF7220BA70D32C667ED84B22D316C564D46086CC78036278D3D77D7885027B）。Coordinator 已越过读取、解码和 validator，随后因生成目录没有生产 API 声明，在 Binding 阶段以 MissingApiDeclarations 停止；因此 codec 读取验证通过，但世界 owner 数据尚未提交，不能报告完整加载成功。只运行该一个代表场景，没有运行完整测试集。