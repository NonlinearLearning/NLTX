# 世界存档加载与保存协调器 PRD

文档 ID：DOC-2026-10-02-world-storage-load-coordinator-prd  
逻辑域：plans  
产物类型：task (PRD)  
状态：draft  
范围：本地世界文件的加载、保存及其与 ECS 世界存储 owner 的交接；加载 API 的构建期收集和调度；不含探索地图侧车文件。  
设计与执行材料：[总体设计](../../architecture/2026-10-02-world-storage-load-coordinator-design.md)、[标准加载 API 设计](../../architecture/2026-10-02-world-load-standard-api-design.md)、[标准加载 API 执行计划](../../architecture/execution/2026-10-02-world-load-standard-api-execution.md)。  
证据入口：[WorldFile.cs](../../../src/NSSLC.Infrastructure/WorldStorage/WorldFile/WorldFile.cs)、[WorldLoadLifecycleSystem.cs](../../../src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldLoadLifecycleSystem.cs)、[WorldStorageRoot.cs](../../../src/NSSLC/Component/WorldStorage/WorldStorageRoot.cs)、[ECS 领域层与基础设施架构边界](../../../Context/架构设计/ECS领域与基础设施架构边界.md)、[CR-2026-10-02](../../cr/CR-2026-10-02-world-load-standard-api.md)。  
canonical 路径：docs/plans/tasks/2026-10-02-world-storage-load-coordinator-prd.md

## 1. 概述

世界文件包含多个领域 owner 所需的持久化数据。文件解析、存档有效性检查和 ECS 状态更新需要解耦：`NSSLC.Application` 承载普通 .NET 保存/加载用例协调器、应用所需的持久化/存储契约和生成 API 目录；Infrastructure 实现文件与格式端口并执行外部效果；NSSLC 领域公开各 owner 的强类型加载 API 并独占权威 Store 写入。宿主组合根创建具体适配器和协调器实例，再调用用例。

一次加载尝试由 Application 协调器临时持有完整解码结果。每个需要恢复持久化状态的 System 实现标准加载 API，并声明自己消费的强类型数据契约；构建阶段在能引用全部 API 且声明选定 codec schema 的消费编译中收集这些 API，优先在 Application 消费程序集生成目录，必要时由组合入口编译目录并通过 Application 端口注入，加载阶段自动执行。保存时，协调器反向汇集 owner 提供的持久化快照并交给文件格式端口写出。

完整存档不放入组件或 `WorldStorageRoot`。本 PRD 使用已创建的 `NSSLC.Application` 作为应用用例边界，不再新建第二个 Application 项目或 feature-local `/WorldStorage` 用例层。Application 依赖 NSSLC 的公开领域 API 与自身端口，不依赖 `NSSLC.Infrastructure` 具体实现；Infrastructure 实现 Application 定义的稳定契约；宿主组合根负责连接双方。

## 2. 目标

- 文件读写和格式编解码不直接修改或查询 ECS 全局状态。
- 完整解码载荷只在一次加载工作流中由协调器持有，且在 owner 提交完成或失败清理后释放引用。
- 每个领域 owner 只接收自己的载入数据，并通过受控入口校验、提交到权威 Store。
- 所有加载 API 遵循统一的生命周期、结果和失败语义；API 输入仍为 owner 专属强类型数据。
- 构建阶段自动发现和收集 System 加载 API，并生成确定性的调用目录；运行时不扫描类型或使用反射发现 API。
- 世界在所有必要 owner 提交及加载后处理完成前保持加载门处于阻断模拟的状态，不报告已加载。
- 保存数据来自明确的 owner 快照，文件编码不通过全局变量反向读取 ECS。
- Application 用例只依赖 NSSLC 的公开领域 API 和应用端口；基础设施实现端口，不能由 Infrastructure 决定跨 owner 的加载/保存流程。
- 宿主组合根集中构造 Application 协调器、Infrastructure 适配器和领域上下文；API 自动发现不替代实例组合。
- 保留现有世界文件格式和本地备份/恢复语义；任何格式差异须另行记录并验证。

## 3. 用户与使用场景

### US-001：加载一个世界

作为世界会话启动流程的维护者，我希望一次加载尝试能够先完整解码和检查世界数据，再将数据提交到对应 owner，以避免文件解析器直接操纵分散的运行时状态。

验收标准：

- [ ] 一次尝试产生一个完整的持久化载荷对象图；该对象图属于传输数据，不属于 ECS Component。
- [ ] 每个需要恢复数据的 System 声明唯一 API 身份及其强类型输入 section；协调器按生成目录自动调用，不维护手写的逐 System 调用列表。
- [ ] 每个 API 声明数据 section 对应的存档版本范围及必需/可选策略；缺失必需 section 在任何 owner 提交前报错，可选 section 使用显式缺省/absence 语义。
- [ ] 所有 owner API 遵循统一的预检/准备、提交、结果和清理协议；一个 API 只能更新其 owner 的权威状态。
- [ ] 构建期收集器能检查 API 签名、身份重复、输入 section schema 缺失/冲突、section DTO 类型错配、必需 section 无消费者、owner 上下文运行时绑定缺失和声明依赖错误；无效声明以编译错误或生成诊断报告。
- [ ] 生成调用次序稳定且可复现；需要先后关系的 API 明确声明依赖，不依赖源码枚举顺序或反射顺序。
- [ ] 运行时调用生成的直接分派代码，不做运行时类型扫描或反射发现。
- [ ] 文件、格式、头部及跨字段结构检查在任何 owner 状态提交前完成。
- [ ] TileMap、实体槽位、容器、标牌、TileEntity、区段等数据只交给实际拥有对应状态的 owner；范围以旧文件格式和当前 owner 为准。
- [ ] 所有 owner 在第一次状态写入前完成本领域输入预检；任一预检拒绝时不开始提交。
- [ ] owner 提交通过受控入口更新 Store；协调器不直接写组件字段或 Store 内部集合。
- [ ] 所有 owner 提交及规定的加载后处理成功后，才完成加载生命周期并发出世界已加载通知。
- [ ] 解码或整体校验失败时不得开始 owner 提交；重试和备份决策沿既有加载生命周期处理。
- [ ] 若 owner 提交期间失败且已发生部分写入，重试前必须清空/丢弃该次部分世界状态，或由已证明的事务性提交机制保证回滚；不得把同一载荷再次叠加提交。

### US-002：保存一个世界

作为世界存储维护者，我希望编码器消费一份显式构造的世界存档快照，以避免基础设施从全局 ECS 状态自行收集数据。

验收标准：

- [ ] 每类持久化数据由对应 owner 在明确的一致性边界提供快照。
- [ ] 协调器汇集完整快照后，才调用格式编码/文件写入。
- [ ] 快照只包含存档格式明确支持的持久化字段，不泄漏组件、Store、可变集合或运行时服务。
- [ ] 保存中途失败时对调用方返回明确失败结果；既有主文件/备份处理和格式兼容语义必须保持或有单独迁移决定。
- [ ] 取得多 owner 快照的时点一致性有明确保证，不能把不同模拟时刻的状态拼成一个世界文件。

### US-003：失败恢复不暴露半加载世界

作为世界生命周期维护者，我希望文件重试、备份恢复和 owner 提交失败有清晰边界，以免失败尝试的状态泄漏到下一次尝试。

验收标准：

- [ ] 每次主文件/备份尝试都有独立的临时载荷生命周期。
- [ ] 未通过全局校验的载荷不能进入提交阶段。
- [ ] 失败尝试不发布世界已加载通知。
- [ ] 进入下一次文件尝试前，系统能证明 owner 状态为空、已重建或已事务性回滚。
- [ ] 加载生命周期状态只保存阶段、重试及恢复所需信息，不持有整个持久化载荷。

## 4. 功能要求

| ID | 要求 |
| --- | --- |
| FR-01 | 读取/写入本地世界文件及格式编解码由 Infrastructure 内的普通 .NET 类型负责；它们实现 Application 定义的存储/codec 契约，不驱动跨 owner 用例。 |
| FR-02 | `NSSLC.Application` 中的普通协调器持有一次完整加载结果，负责编排整体校验、调用生成的 API 目录、汇总结果和处理失败。 |
| FR-03 | 每个持久化 owner/System 实现标准加载 API，并在编译期可发现；API 声明唯一身份、自己消费的数据 section、版本范围及必需/可选策略。 |
| FR-04 | 标准 API 使用统一协议，但输入、预备结果和 owner 行为保持强类型；API 不接收整份存档 DTO，不直接访问文件，不写其他 owner 的 Store。 |
| FR-05 | 构建期收集 NSSLC 领域 API，并在可见全部 API 的消费编译中生成确定性的直接调用目录；`NSSLC.Application` 是首选消费程序集，组合入口可在需要同时引用 codec schema 时生成并通过 Application 端口注入。运行时不得依赖反射或手工逐项注册。生成器发现重复身份、section schema 映射错误、无效契约或依赖环时必须报诊断；API 实例和 owner context 的可用性在 Prepare 前预检。 |
| FR-06 | 整份持久化载荷按领域分组；协调器保留完整对象图，并通过生成的类型化映射将 owner 专属 section 传给 API。 |
| FR-07 | 全部 owner 预检/准备成功后才开始提交；提交和准备结果必须可区分，且提交失败不得报告加载完成。 |
| FR-08 | 保存由各 owner 提供快照；协调器负责在格式写出前组装完整载荷。 |
| FR-09 | 读取、收集、校验、提交或保存失败都返回/记录可区分结果，不用静默默认值掩盖问题。 |
| FR-10 | 保持格式版本、默认值、压缩、备份、临时文件和错误行为的既有兼容性；行为差异需要单独列出。 |

## 5. 非目标与约束

- 不读取、生成或提交探索地图 .map 数据。
- 不包含 Steam 云存档、云端参数、云端字段、平台调用或 CopyToLocal、MoveToLocal、MoveToCloud 操作。
- 不新增外部库。
- 不通过运行时反射、扫描或手工维护的逐 System 注册表实现 API 收集；调用目录在构建阶段产生。
- 不允许生成代码写入 src/ 或项目目录；生成产物遵循仓库构建约束，放在 Build/generated/ 下。
- 不将持久化载荷、文件状态或协调器放入 Component、WorldStorageRoot 或 ECS 生命周期组件。
- 不再创建第二个 Application 项目或独立 `NSSLC.Application/WorldStorage` 子项目；保存/加载用例、所需端口和协调器归入现有 `NSSLC.Application`，不得为转发调用增加空抽象。
- `NSSLC.Application` 不引用 `NSSLC.Infrastructure` 的具体实现；Infrastructure 通过实现 Application 契约接入，宿主组合根持有具体引用。
- 不在本 PRD 中授权移除兼容入口、改变二进制格式或宣称迁移已完成。

## 6. 质量要求

- **内存：**大尺寸 Tile 数据在加载期间可能与目标运行时 Store 同时存在。实现应统计峰值并减少不必要复制；任何所有权转移都必须确保源 DTO 不再被使用且没有可变别名。
- **隔离：**协调器不使用静态全局载荷；同一进程内两个世界或两次尝试不能共享临时数据。
- **可诊断性：**报告文件阶段、格式/版本错误、整体校验错误、具体 owner 拒绝原因和提交阶段，不要求将完整存档内容写入日志。
- **构建期正确性：**无效或重复的加载 API 声明应在构建期被发现；相同输入和 API 声明产生相同目录和调用顺序。
- **依赖方向：**`NSSLC.Application -> NSSLC` 公开 API；`NSSLC.Infrastructure -> Application` 契约实现；Host 同时组合 Application 与 Infrastructure；领域层不得反向引用 Application 或 Infrastructure。
- **兼容性：**不因新对象形态改变原文件字段布局、字段顺序、压缩或缺省值。

## 7. 验收总表

- [ ] 完整数据在协调器一次工作流内持有，任何 ECS Component/根组件不保存该载荷。
- [ ] 编解码与 ECS 权威状态没有隐藏全局读写依赖。
- [ ] 每个持久化 owner 的标准加载 API 都被构建期收集，生成的调用目录覆盖所有必需 section，且没有重复或悬空映射；运行时缺失的必需 section 在提交前拒绝。
- [ ] 生成调用顺序有确定性；运行时不进行 API 发现或反射分派。
- [ ] API 生成机制没有新增外部库，生成输出留在规定目录。
- [ ] 所有待提交数据在整体校验后按 owner 分派。
- [ ] 加载门、owner 预检、提交、通知、失败清理和下一次重试的顺序定义完整。
- [ ] 保存快照来自各 owner，并有一致性边界。
- [ ] 内存峰值、缓冲区所有权和可变别名规则有实现证据。
- [ ] 已覆盖有效文件、损坏文件、格式版本不支持、无备份、备份成功、owner 校验拒绝、提交中断和保存失败。
- [ ] 世界文件往返及与旧格式的行为比较通过，且结果与未执行项分别记录。

## 8. 待实施前确认

- 盘点 WorldFile 的实际读写段落与各数据 owner 的完整字段映射，确认哪些结构持久化、哪些只是运行时索引。
- 核对 `NSSLC.Application` 项目引用 NSSLC 领域公开 API，Infrastructure 只向内依赖应用端口/数据契约，实际 Host 引用 Application 与 Infrastructure 并承担组合根职责；确认没有 Domain -> Application/Infrastructure 的反向依赖。
- 在能同时引用 API 声明并声明所选 codec schema 的消费编译中验证构建期收集器能看到传递引用 API，并能将生成目录编译到 `Build/generated/`；Application 是首选目标，组合入口是 codec schema 只在其引用闭包可见时的执行选项。
- 选择满足无外部依赖约束的构建期收集/代码生成机制，并证明它能够发现组合入口引用的各领域 API。
- 定义标准 API 的身份、owner 输入 section 映射、预检/提交结果、依赖排序、重复声明和生成诊断规则。
- 明确所有 owner 是否能在当前加载门内安全提交；对跨 owner 部分失败制定可执行的清理或回滚机制。
- 确认保存期间冻结/捕获所有 owner 快照的一致性边界。
- 确认旧文件备份和临时文件流程在新分层下的行为等价。

## 9. 证据边界

导入的 WorldFile.LoadWorld() 与保存代码仍包含对 Main、WorldGen 等全局状态的访问；它证明的是当前耦合现状，不证明解码载荷拆分已实现。WorldLoadLifecycleSystem 表达重试、备份与完成阶段，不是载荷容器。WorldSavedOreTierLoadSystem 仅是局部读取、修复、提交模式的例子，不能证明整份世界存档已经按 owner 迁移。

本次用户补充的正式要求是：每个 System 实现标准加载 API，协调器在编译阶段收集这些 API，并在加载阶段自动调用。仓库 src/ 与 Build/ 的初步检索未发现现有 Roslyn generator/analyzer；具体生成工具和跨项目 API 收集方式仍需实施前验证。

后续实施更新：`NSSLC.Application` 已建立通用 `WorldPersistenceDocument` 强类型 section 容器、解码与整体校验端口，并将其接入 `WorldLoadCoordinator`。协调器在单次调用内持有文档并通过生成目录分派；成功结果不保留载荷。Infrastructure 绑定正式本地字节存储与 Application 协调器。仍未实现真实 WorldFile 格式字段映射/解码、生产 owner API、生产 Host/lifecycle 接线和提交失败后的实际世界清理/重建；详见执行计划验证记录。
