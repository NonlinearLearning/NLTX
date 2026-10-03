# 世界存档 System 标准加载 API 执行计划

文档 ID：DOC-2026-10-02-world-load-standard-api-execution  
逻辑域：architecture  
产物类型：execution  
状态：draft  
executionStatus：in-progress  
implementationStatus：partial  
verificationStatus：partial  
范围：标准加载 API 契约、构建期 API 收集与强类型直接分派，以及加载生命周期接入。  
canonical 路径：docs/architecture/execution/2026-10-02-world-load-standard-api-execution.md

本文是分阶段执行计划，实施范围由[CR-2026-10-02](../../cr/CR-2026-10-02-world-load-standard-api.md)的最新记录约束。用户已授权本轮实现标准 API 契约和构建期收集/生成基础设施，但要求生产 System 暂不实现标准 API，测试仅进行 10%。因此本轮不迁移任何生产 owner，不虚报实际世界加载已自动分派。

后续架构边界更新：`NSSLC.Application` 已引用 WorldStorage 标准契约与构建期生成器；Application 侧现在有强类型 `WorldPersistenceDocument`、解码/整体校验端口、生成目录消费入口，以及在一次调用内解码、持有并分派该文档的加载协调器。协调器在执行前复制 descriptor 快照。Infrastructure 通过本地文件端口提供文件效果，并增加不依赖全局状态的 `WorldFileDocumentDecoder`、校验器和完整 pointer table `WorldFileDocumentEncoder`；decoder/encoder 共享 `.wld` header/environment/progression/quest/banner/boss-progression/party/sandstorm/defender-event/background/event/tree-tops/seasonal/npc-unlocks/time-policy/spawn 布局，并处理压缩 Tile payload、Chest/Sign/NPC/WeightedPressurePlates/TownManager/Bestiary/Footer，以及 TileEntity/CreativePowers 的有界原始载荷。它们不读取探索地图 `.map`，也不调用旧的 `Main`/`WorldGen` 全局解析器。生产 System API 声明和生产 lifecycle/Host 接线仍未实现。

此前依赖图调查仍表明 `NSSLC` 未加入根 `Terraria.Dome.sln`，而生成器目前由非生产 fixture 验证。阶段 0 现为部分完成：保留已验证的生成机制证据，补查 Application 消费程序集、Infrastructure 端口实现引用和真实 Host 组合关系后才能关闭。

本轮测试预算按一项代表性生成目录冒烟验证控制；受影响生产/生成器项目的编译作为构建验证单独记录，不运行完整测试集。若用户补充 10% 的计量口径，执行时更新实际抽样范围。

## 1. 范围与前置条件

本计划只覆盖加载 API 和构建自动分派。完整 WorldFile 解耦、保存快照一致性、全部字段迁移和旧行为等价属于上层工作项；它们是端到端验收所需依赖，不由 API 原型替代。

实施前须完成：

- 对照仓库根 `AGENTS.md` 读取当前进度、协作/变更约束、ECS 文件组织、领域与 Infrastructure 边界、C# 风格、副作用隔离及构建验证要求。
- 确认当前工作区已有用户变更，不回退或覆盖无关修改。
- 检查 `global.json`、所有相关 `.csproj`、`Directory.Build.*`、引用方向、SDK 版本和生成输出约定。
- 不新增外部库；构建生成源码留在 `Build/generated/`，中间文件不得进入源码项目目录。
- 对接阶段先确认 owner 持久化 section 清单和组合宿主，不让生成器反向引用 Infrastructure 内部实现。

## 2. 分阶段顺序

阶段状态使用 `planned`、`partial`、`done` 或 `blocked`。每阶段通过进入门槛并记录证据后，才能将状态改为 `done`；部分完成须列明已交付和未覆盖项；阻塞项写为 `blocked` 并给出可复现原因，不通过删去验收标准来解除。

| 阶段 | 工作内容 | 产物 | 进入下一阶段的门槛 | 状态 |
| --- | --- | --- | --- | --- |
| 0. 依赖图与生成机制调查 | 确认 Application 消费程序集、Host、Infrastructure 端口实现的项目引用；API 可见性；现有 MSBuild/SDK 生成能力、输出路径与 SDK 行为 | Application/Host 项目引用图；SDK Roslyn source generator 决策；Build/generated 输出约定 | 证明 API 消费编译能看到所有领域 API，Host 能组合 Application 与 Infrastructure，且依赖不反向指向领域 | `partial`（Application 目前只引用 WorldStorage 契约而未引用 owner API 项目；生产 Host 未接入；Infrastructure -> Application 端口方向已核实） |
| 1. API 合同冻结 | 定义 API/owner/section 身份、格式版本、必需/可选、强类型上下文、结果、清理与提交依赖 | `Terraria.WorldStorage` 契约类型 | 一项 API 可从声明明确判定所有输入、阶段和失败语义；没有隐式跨 owner 写入 | `done` |
| 2. 构建期收集原型 | 用最小 API 跨项目样例验证声明收集、传递引用、重复诊断、section/context 绑定和确定性生成 | Source generator；跨程序集 fixture；生成 catalog | 不扫描运行时类型、不使用运行时反射、不手写 API 目录；输出位于 `Build/generated/`；未引入外部依赖 | `partial`（双 API 传递发现、schema 正向映射、Optional Absent 和 `CommitAfter` 顺序已由唯一场景验证；两次增量构建生成字节一致；负向诊断尚未验证） |
| 3. 标准 API 与单 owner 试点 | 实现公共契约和一个真实 owner 的强类型 section、Prepare/Commit/Discard；先完成准备拒绝路径 | 一个可编译的 owner API；输入映射；生命周期边界说明 | Prepare 不写权威 Store；Commit 只写所属 Store；prepared data 在成功、拒绝和异常路径都可释放 | `planned`（生产 System 按用户要求暂缓） |
| 4. 生成目录及协调器接入 | 将准备/提交/清理直接调用目录生成到 API 消费编译；由 Application 协调器传入载荷 section 与 owner context，Host 提供生成目录、运行时绑定及生命周期接线 | Generated catalog；协调器调用路径；Host 组合图和调用序列证据 | 缺少必需 section 或 context 时，在任何 API 调用前失败；准备全成功后才提交；调用顺序确定 | `partial`（Application 协调器已接收 `IWorldLoadApiCatalog`、复制 descriptor 快照；Infrastructure factory 由组合根注入该目录，并提供 header decoder 默认入口；Application `GeneratedWorldLoadApiCatalog` 可接收 Host 编译生成的 descriptor 与执行方法组；fixture 改用此适配器并已编译；完整 WorldFile section、生产 System API 和 Host lifecycle 仍未接入） |
| 5. 持久化 owner 扩展 | 按 WorldFile 格式字段到 owner 的映射，逐 owner 增加 API 并从生成诊断修复缺漏 | owner 覆盖矩阵；每个 section 的映射和提交 owner 记录 | 所有必需 section 有且仅有正确消费者；非持久化、派生和暂态字段有排除理由 | `planned` |
| 6. 生命周期与恢复失败路径 | 接入现有主文件/备份尝试、加载门、成功通知和提交后清理/重建 | 生命周期调用图；失败分类和重试规则 | 准备失败无权威写入；提交失败不能报告成功；重试前部分世界状态已清理或重建 | `partial`（状态机拒绝在加载门仍升起时报告成功，并为备份检查/恢复失败提供终止转换；Infrastructure 本地适配器已实现检查 `.bak` 与临时文件替换/删除效果；Application coordinator 仍只处理单路径载荷/准备结果，生产源码没有调用 coordinator/factory 或备份效果的接线点；Host 通知和提交失败后的清理/重建仍未接入，新增恢复分支未做行为验证） |
| 7. 端到端验收与收尾 | 按构建验证约束执行受影响项目构建和定向行为验证；记录旧格式行为对比及未覆盖项 | 验证报告；风险与剩余工作；文档/manifest 更新 | PRD 加载验收项逐条有证据；生成输出没有进入源目录；所有未完成行为明确保留为未验收 | `partial`（仅完成构建和 1/10 冒烟） |

## 2.1 本轮执行边界

- **本轮已实现：**标准协议类型、构建期收集与部分诊断、确定性排序、生成的强类型直接调用目录，以及供非生产跨程序集 fixture 使用的绑定入口。
- **本轮不实现：**任何生产 System 的标准 API 实现/声明、生产 owner 字段映射、旧 WorldFile 的真实格式解码与字段迁移、生产 lifecycle/Host 接线。Application 的通用强类型文档、解码/整体校验端口和 generated catalog 协调路径纳入本轮。
- **测试范围：**一项最小正向生成目录冒烟，验证两个传递引用 API 被编译期发现、Optional Absent 输入和 `CommitAfter` 提交顺序。其他诊断路径、失败矩阵、全量 owner 和旧格式行为均未测；总行为场景仍为 1/10。
- **依赖边界：**生成器不得添加 NuGet/外部依赖；如果 SDK 自带 Roslyn 程序集引用在当前 SDK 上无法稳定构建，先停止并记录阻塞，不替换为运行时反射。
- **工作树边界：**当前 checkout 含有大量现存用户改动和文件迁移。本次只增改本任务直接涉及的文件，不切换/重置分支，不清理既有变更。

## 2.2 子代理并行执行批次

为缩短审计和实现时间，剩余工作按文件边界拆成三个可并行轨道；任何轨道都不得修改另一个轨道的正式源码目录。子代理只负责其轨道的静态核对和小范围修复，主代理负责依赖汇合、串行构建和文档收口。

| 轨道 | 子代理范围 | 并行交付物 | 汇合门槛 |
| --- | --- | --- | --- |
| A. WorldFile codec | `src/NSSLC.Infrastructure/WorldStorage/WorldFile/`；对照 Version4 参考源码检查 NPC、TileEntity、CreativePowers、Footer、pointer table 和 optional section | codec 修复或明确的未证实风险；不得修改 `分类参考`、`.map` 或 Steam 字段 | A 轨完成后，主代理串行构建 Application，再构建 Infrastructure |
| B. Application 协调器 | `src/NSSLC.Application/WorldStorage/Persistence/`；检查 descriptor 快照、绑定预检、Prepare/Commit/Discard、取消和清理失败归一化 | 协调器修复或不变量审计结果；保持公开 API 稳定 | B 轨完成后，主代理串行构建 Application，再构建 Infrastructure |
| C. 生成器与 Host 证据 | `Build/Tools/WorldLoadApiGenerator/`、非生产 fixture、依赖图和执行证据 | 生成器负向诊断/fixture 静态核对、Host 缺口和下一批次清单 | 不伪造生产 Host；生成器输出仍必须只进入 `Build/generated/` |

并行轨道完成后执行以下固定汇合顺序：

1. 检查工作区是否存在半成品或跨轨道修改，并运行 `git diff --check`。
2. 使用 SDK `10.0.400` 串行构建 `NSSLC.Application`。
3. 在 Application 成功后串行构建 `NSSLC.Infrastructure.WorldStorage`。
4. 仅在用户放宽测试预算时运行行为测试；当前继续保持 `1/10`，不运行完整测试集。
5. 更新本文件的阶段状态、命令、退出码、警告/错误数和输出路径。

该并行批次不能关闭阶段 3、5、6 或 7：生产 System API、真实 Host/lifecycle 接线、全量 owner 映射、失败矩阵和旧格式往返仍需后续独立批次。`WorldLoadApiCatalogGenerator` 继续保留为构建期唯一收集与强类型分派机制；没有生产 API 声明时，空目录返回 `MissingApiDeclarations` 是预期结果。

### CreativePowers 区段终止 framing（2026-10-03）

对照 `CreativePowerManager.SaveToWorld` 后补齐空区段边界：当前 WorldFile 即使没有持久化 CreativePower，也必须写出一个 `false` 记录终止符。正式 decoder 和 validator 现在拒绝零长度 CreativePowers payload；encoder 在文档缺少该可选 section 时写出合法的单字节终止 framing，而不会生成旧加载器无法读取的空区段。该修正仍只保留 opaque payload，不调用 CreativePowerManager，也不改变生产 System API 或行为测试预算。

CreativePowers opaque payload 还必须以 `false` 终止字节结束；codec 只检查 framing，不解释 power id 或扩展数据。

同批次补齐 TileEntity 的最小 framing 一致性：`EntityCount == 0` 时 opaque 记录必须为空，`EntityCount > 0` 时必须存在有界记录载荷。codec 不解析类型注册表或扩展字段，完整记录边界仍由未来 TileEntity owner 在 Prepare 阶段验证。

使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
```

两条命令均退出码 `0`、警告 `0`、错误 `0`；程序集输出位于 `Build/bin/NSSLC.Application/Debug/net10.0/` 和 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/`。本次没有运行行为测试，预算仍为 `1/10`。

## 3. 阶段执行细节

### 阶段 0：依赖图与生成机制调查

以 `NSSLC.Application` 作为协调器与生成目录的消费项目，已核实项目文件以及 Infrastructure 对 Application 文件端口的引用；仍需确认其对 NSSLC 领域公开 API 的引用和真实 Host 对双方的组合引用。不得让 Application 引用 Infrastructure 具体实现，也不得让领域反向依赖 Application。

对候选构建机制逐一记录：输入范围是否包括传递引用、是否能形成静态调用、如何绑定运行时实例和 owner context、如何报告 source location 诊断、如何稳定排序、生成文件如何落在 `Build/generated/`、是否需要新增包。若候选都不能满足要求，暂停阶段 2 并提出明确的架构差异，不改用运行时反射或手写注册表。

### 阶段 1：API 合同冻结

冻结的内容必须至少覆盖：

- 唯一且稳定的 `ApiId`、`OwnerId`、`SectionId`，以及它们的比较规则。
- `TSection` 与 `SectionId` 的验证方式；Optional absence 的显式表示。
- 文件版本支持范围与格式归一化责任。
- `TOwnerContext` 的来源、不可变/可变能力和不得越过的 owner 边界。
- Prepare 结果、Commit 结果、异常转译、Discard 幂等性和清理错误保留方式。
- `CommitAfter` 的有向边定义、拓扑排序和循环诊断。
- 一份诊断目录，至少含身份重复、签名错误、section 无映射/多映射、必需数据无消费者、上下文无映射、不可访问 API、缺失依赖及依赖环。

类型或元数据若无法通过生成器编译时检查，不得只留为注释约定；需形成诊断或显式设计偏差。

### 阶段 2：构建期收集原型

原型仅含两个独立领域程序集和一个组合宿主：一个有效 API、一个重复 ID、一个缺 section、一个 optional absent 输入，以及一条 commit dependency。至少重复执行两次相同构建，比较生成目录和排序，确认结果确定。

记录完整命令、SDK、退出码、警告/错误数和输出路径。原型不得改动生成文件以外的运行时代码，不得引入 NuGet 包，也不得把生成代码写进 `src/` 或 `.csproj` 所在目录。该原型只证明发现和生成可行，不证明全量 owner 或游戏加载行为。

### 阶段 3：标准 API 与单 owner 试点

选择一个字段映射和 Store 边界可明确的小型持久化 owner。先由该 owner 完成输入验证与准备数据，再由同一 owner 的 Commit 写入 Store。通过可观察状态快照确认 Prepare/拒绝不会改变 owner 状态，Discard 可释放暂存结果，Commit 失败被标记为可能部分写入。

试点不得让 coordinator、Infrastructure 或 catalog 直接写组件字段/Store 内部集合；不把 prepared payload 存入 ECS Component。试点结束后复核生成目录从声明到调用的完整可追踪性，再扩展更多 owner。

### 阶段 4：目录和协调器接入

协调器调用顺序固定为：完整解码与整体校验 -> 绑定预检 -> 全部 API Prepare -> 全部 API Commit -> 既有加载后处理 -> 完成通知。目录在绑定预检失败时不调用任何 API；Prepare 任一拒绝时逆序 Discard 已成功准备结果；Commit 失败则停止后续 API、清理尚未提交的准备值并进入世界级清理/重建。进入 Commit 的 API 必须自行消费或释放其准备值。

准备阶段按稳定 `ApiId` 顺序。提交阶段按 `CommitAfter` 拓扑排序，同层稳定排序。所有顺序写入构建证据并由生成输出验证，不依赖文件/程序集枚举顺序。

### 阶段 5：owner 扩展

按字段到 owner 的映射扩展，不按源码文件复制顺序扩展。每个 section 记录：格式来源、是否持久化、强类型 DTO、读取 owner、唯一提交 owner、缺省规则、文件版本范围、数量上限和行为验证范围。领域派生索引重建与持久化字段分开记录。

探索地图 `.map`、Steam 云存档以及已明确排除的云平台成员不得进入 API 或文档字段矩阵。批次命名仅表示真实输入形态，不改变事务语义。

### 阶段 6：失败恢复接入

分别覆盖解码/整体校验失败、API Prepare 拒绝、Prepare 抛错、Commit 失败、Discard 失败、加载后处理失败、主文件失败后备份成功、主备份均失败。每种情形都需记录加载门状态、受影响 owner、允许重试的条件和是否产生完成通知。

跨 owner 提交尚无原子性证据时，必须以一次性世界状态作业并在重试前重建为准。不得在已有部分写入的 Store 上只重放失败 API。

### 阶段 7：验收

验收按 PRD 和仓库构建约束执行，至少保留：受影响项目构建命令/退出码/警告错误数/输出目录；构建重复性和诊断结果；生成代码直接调用证据；owner 状态差异与提交顺序；正常加载、缺失必需 section、可选 absence、版本不支持、owner 拒绝、部分提交失败和重试前清理结果。

完整阶段要求的测试与行为验证仍是后续实施门槛。本轮只构建受影响项目，并将唯一运行行为场景限制为 1/10；行为验证未覆盖项须保持显式未完成。

### WorldFile TownManager/Bestiary/opaque section（2026-10-03）

正式 decoder 新增完整 pointer table 后续区段读取：TownManager 的 NPC 房间分配和 Bestiary 的 kill/sight/chat 记录进入 Application DTO；TileEntity 保留记录数量及有界原始记录载荷，CreativePowers 保留有界原始载荷。读取按版本门、section pointer、数量、字符串和坐标边界执行，不调用旧 `Main`、`WorldGen`、TileEntity 注册表或 CreativePowerManager。该阶段先只支持加载；后续 pointer table 保存增量已补齐对应 framing，供未来 owner 在 Prepare 阶段解码类型化内容。

使用 SDK `10.0.400` 串行编译 `NSSLC.Application` 与 `NSSLC.Infrastructure.WorldStorage`，两次退出码均为 `0`、警告 `0`、错误 `0`，输出位于 `Build/bin/`。本次没有运行新的行为测试，行为预算保持 `1/10`。

## 4. 完成定义

只有同时满足以下条件，才能把 `executionStatus` 从 `planned` 改为完成状态：

- 每个需要恢复持久化状态的 owner 都有可发现的标准 API 声明。
- 组合宿主构建期发现所有传递依赖 API，并生成直接、强类型调用目录。
- 无效映射、必需 section 缺失、依赖错误和身份冲突被构建期诊断；运行期缺失数据在任何写入前拒绝。
- 全部准备成功后才提交，owner 写边界唯一，部分提交失败后重试有清理/重建保证。
- 世界仅在提交及既定 load 后处理全部成功后解开加载门并通知完成。
- 无新增外部库，生成输出严格位于 `Build/generated/`，运行时不存在 API 扫描/反射发现。
- 旧格式、备份、错误分类、内存峰值和行为比较结果有独立证据；没有把代码编译通过等同于迁移验收。

## 5. 当前状态

当前状态：`executionStatus: in-progress`，`implementationStatus: partial`，`verificationStatus: partial`。

已交付：

- `src/NSSLC/Component/WorldStorage/` 中的标准契约基础和版本化绑定入口。
- 标准 API 的 `PrepareLoad` 契约现明确要求不得修改 section 输入、嵌套值、owner 权威状态或传入上下文；拒绝/抛异常前须释放未作为 prepared data 返回的本地暂存资源。`DiscardPrepared` 必须可重复调用，避免生成目录无法清理当前失败 API 未返回的句柄。
- `src/NSSLC.Application/WorldStorage/Persistence/` 中的恢复协调器会将存储字节交给显式解码端口，得到 `WorldPersistenceDocument`；文档持有版本和按唯一 `SectionId` 索引的强类型 section，协调器在同一次调用内执行整体校验并驱动 `IWorldLoadApiCatalog`。API 消费编译以 assembly-level `WorldPersistenceSectionSchemaAttribute` 声明所选 document schema，生成器据此检查 API section 映射并将诊断定位到消费编译源码。Infrastructure factory 绑定本地文件适配器并接收组合根提供的 `IWorldLoadApiCatalog`、decoder 与整体校验器；`GeneratedWorldLoadApiCatalog` 可包装组合 Host 编译生成的目录描述符和静态执行方法，Host 不必再为此手写接口转接类。文档不放入 ECS Component、`WorldStorageRoot` 或恢复结果；API 提交失败通过 outcome 标记，Commit 阶段失败要求外层清理/重建。
- 早期版本的 `WorldLoadCoordinator` 曾按 `WorldRecoveryPolicy` 重试主文件并直接读取、覆盖备份；该实现已在后续职责核对中撤回，因为它重复承担了 `WorldLoadLifecycleSystem` 的恢复阶段。当前 coordinator 每次 `Load` 只读取调用方传入的路径一次，解码、整体校验并执行目录，然后把单次结果交回调用方。它不读取 `.bak`、不改写/删除文件、不决定重试；`WorldRecoveryPolicy` 已移除。
- 当前保留的生成目录边界防御：coordinator 将目录描述符列表为空/无声明、目录执行返回 null 或目录访问/执行抛错转换为明确失败结果。用户确认唯一正向场景通过后要求停止测试，因此这些分支没有行为验证。
- 单次加载的取消语义：文件端口返回的取消结果、decoder/validator 抛出的 `OperationCanceledException` 或 API 执行异常均保留为 `Canceled` 并返回调用方；coordinator 不重试。备份替换及其取消恢复由生命周期/宿主效果执行承担，但目前没有生产 Host 接线，也未完成行为验证。
- `src/NSSLC.Infrastructure/WorldStorage/WorldFile/` 中导入的旧式全量格式代码仍未纳入正式项目编译；新增的 `WorldFileDocumentDecoder` 解析有界 header、environment、progression、quest、banner、boss-progression、party、sandstorm、defender-event、background、event、tree-tops、seasonal、npc-unlocks、time-policy 和 spawn 前缀，并产生对应 Application DTO；spawn section 读取额外出生点、dual-dungeons 标志和原始 manifest 文本，section pointers 足够时还读取压缩 Tile、Chest、Sign、NPC、WeightedPressurePlates、TownManager、Bestiary、Footer，以及 TileEntity/CreativePowers 的有界原始载荷。完整 WorldFile 字段映射和生产 System API 声明/实例、生产 Host 与 lifecycle 接线仍未实现。`分类参考` 不编译、不引用且不承载实现。
- `Build/Tools/WorldLoadApiGenerator/` 中基于 SDK Roslyn 的生成器。它收集消费编译输入及引用程序集，报告重复身份/API section、无效接口/属性组合、格式范围无效、schema 不存在/类型不符/重复/必需项无消费者/requirement 不匹配、缺失依赖和依赖环；环诊断筛出实际参与循环的 API，不将仅受循环阻塞的下游 API 报成环成员。WLA001 形状检查拒绝泛型外层 API、`object`/`dynamic`/可空/`Nullable<T>`/pointer/`void` 参数，以及 `WorldLoadApiBindings` 或其子类，避免生成不可访问调用或让 owner 越过 section 边界。生成器还产生描述符、整体绑定预检、强类型 Prepare/Commit 调用，以及 Prepare 或 Commit 失败时的待提交值清理；多个 `DiscardPrepared` 清理异常以 `AggregateException` 一并保留。若 API 返回默认的拒绝结果，生成目录会提供 `InvalidPrepareResult` 或 `InvalidCommitResult`，不向协调器暴露空错误码。负向诊断已实现但因停止测试要求尚未运行验证。
- `Test/Terraria.WorldLoadApiGenerationVerification.Api/`、`Test/Terraria.WorldLoadApiGenerationVerification.SecondaryApi/`、`Test/Terraria.WorldLoadApiGenerationVerification.TransitiveApis/` 与验证宿主构成非生产传递引用 fixture。生成源码输出在 `Build/generated/`。
- 未新增 NuGet 或其他外部依赖；没有生产 System 实现或声明标准 API，没有完整 WorldFile section 字段迁移、生产 owner API、Host 或 lifecycle 接线。通用完整载荷与 fixture 生成 API 的 coordinator 分派路径已实现，不代表生产游戏加载已接入。
- 已增加 `WorldFileHeaderSection`、`WorldFileEnvironmentSection`、`WorldFileProgressionSection`、`WorldFileQuestSection`、`WorldFileBannerSection`、`WorldFileBossProgressionSection`、`WorldFilePartySection`、`WorldFileSandstormSection`、`WorldFileDefenderEventSection`、`WorldFileBackgroundSection`、`WorldFileEventSection`、`WorldFileTreeTopsSection`、`WorldFileSeasonalSection`、`WorldFileNpcUnlockSection`、`WorldFileTimePolicySection`、`WorldFileDocumentDecoder`、`WorldFileDocumentValidator`、`WorldFileDocumentEncoder` 和工厂默认 codec 入口。该增量覆盖旧 `.wld` header 后的有界前缀 DTO 载荷，并在完整 pointer table 存在时增加 Tile/实体/世界集合的加载 DTO 或原始载荷；没有把这些数据当成完整世界加载，也没有引入 `.map`、Steam 云字段或旧全局状态依赖。

早期构建记录：`dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，曾报告 14 条既有 WorldStorage 字段的 `CS0649`/`CS0169` 警告、0 错误。后续增加 Application 引用并修正生成 namespace 后，最新相同项目构建为 0 警告、0 错误；详见下方端到端 fixture 构建记录。依赖输出位于 `Build/bin/`。

Application 协调器端到端 fixture 构建：SDK `10.0.400`，命令 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。宿主输出位于 `Build/bin/Terraria.WorldLoadApiGenerationVerification/Debug/net10.0/Terraria.WorldLoadApiGenerationVerification.dll`，Infrastructure 输出位于 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`；生成目录位于 `Build/generated/`，按消费程序集命名，Application 与 fixture 宿主的 catalog 不发生类型名冲突。

正式适配器及协调器绑定构建：SDK `10.0.400`，命令 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。该构建同时编译其 ProjectReference `src/NSSLC.Application/NSSLC.Application.csproj`。输出位于 `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` 和 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。正式项目显式编译 `Platform/WorldFileStoreAdapter.cs` 与 `WorldStorageCoordinatorFactory.cs`，不编译 `WorldFile/` 下仍耦合全局状态的格式代码，也不编译或引用 `分类参考`。仓库当前没有生产宿主引用或调用该工厂；构建证明项目与端口绑定可用，不证明游戏运行时已经接线。

恢复路径修订后构建：SDK `10.0.400`，命令 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。构建同时通过 Application 项目；输出仍位于 `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` 与 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。本次只做构建验证，没有运行恢复失败路径行为验证。

策略与目录异常边界修订后构建：SDK `10.0.400`，命令 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。构建通过 `Terraria.Relationships`、`Terraria.Items`、`Terraria.WorldStorage`、生成器、`NSSLC.Application` 和 `NSSLC.Infrastructure.WorldStorage`；Application 与 Infrastructure 输出位于 `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` 和 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。本次为编译验证，没有运行测试。

取消停止重试修订后构建：SDK `10.0.400`，命令 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。构建同时通过 `NSSLC.Application` 和 `NSSLC.Infrastructure.WorldStorage`，产物位于 `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` 和 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。这是编译验证；行为测试仍保持 `1/10`，取消路径未运行验证。

Prepare 清理责任契约修订后构建：SDK `10.0.400`，命令 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `14`、错误 `0`。警告为 `Terraria.WorldStorage` 中既有 `CS0649`/`CS0169` 未赋值或未使用字段；Application 和 Infrastructure 均成功编译，产物位于 `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` 与 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。本次编译验证接口契约源码，没有运行测试。

section 只读契约修订后构建：SDK `10.0.400`，命令 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `14`、错误 `0`。警告仍为 `Terraria.WorldStorage` 中相同的既有 `CS0649`/`CS0169` 字段；Application、Infrastructure 成功编译，产物仍在 `Build/bin/`。本次为编译验证，没有运行行为测试。

WLA006 环成员筛选修订后的 fixture 编译：SDK `10.0.400`，命令 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。该命令编译 generator、Application、Infrastructure、两个 API fixture、传递引用 assembly 与 fixture Host；Host 产物位于 `Build/bin/Terraria.WorldLoadApiGenerationVerification/Debug/net10.0/Terraria.WorldLoadApiGenerationVerification.dll`，生成源码仍位于 `Build/generated/`。这是编译验证，没有运行 fixture。

API 泛型外层形状检查修订后的 fixture 编译：SDK `10.0.400`，命令 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。Generator、Application、Infrastructure、API fixtures 与 Host 均成功编译，Host 产物位于 `Build/bin/Terraria.WorldLoadApiGenerationVerification/Debug/net10.0/Terraria.WorldLoadApiGenerationVerification.dll`。这是有效 fixture 的编译验证；未运行行为测试或新形状负向诊断验证。

标准 API 拒绝结果校验后的跨项目构建：SDK `10.0.400`，命令 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `14`、错误 `0`。警告均为 `Terraria.WorldStorage` 中既有未使用/未赋值字段的 `CS0169`/`CS0649`，未因本次变更新增错误；fixture、生成器、Application 和 Infrastructure 输出均位于 `Build/bin/`。这是生成代码编译验证，不是行为测试。

生成确定性核对：在前述 fixture build 成功后，将生成器源码时间戳推进以触发正常增量重编译，再执行相同 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`；第二次退出码 `0`，警告 `0`、错误 `0`。重编译前后 `Build/generated/NSSLC.Application/Debug/net10.0/WorldLoadApiGenerator/Terraria.WorldStorage.Generators.WorldLoadApiCatalogGenerator/WorldLoadApiCatalog.g.cs` 的 SHA-256 均为 `E68A18F3DEC18FE140F65B0A35B646A4DB5A4C1F25278E44DA887415FABA5D3E`；fixture Host 对应文件均为 `F18EEBBFB5E594921DAD0FFF9AE06BFCEDF7E2D04A58164EFA8821039DE3A6A4`。这证明当前有效输入下重复生成字节一致，不证明负向诊断或运行时失败语义。

本轮唯一行为场景：`dotnet run --project Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-build --no-restore`，此前退出码 `0`，输出 `World load API generation verification passed.`，按 10 个等权场景计为 `1/10`。验证宿主直接引用 `TransitiveApis`，由项目传递引用获得两个 API 程序集；generated catalog 在编译期发现并直接分派两个 API。该场景通过 Infrastructure factory 注入宿主生成的 catalog，由正式 `WorldFileStoreAdapter` 从系统临时文件读取 fixture 字节，再经过真实 Application `WorldLoadCoordinator` 的强类型解码、整体校验、绑定预检和 Prepare/Commit；断言 Optional section 以 Absent 传入且在 Commit 时仍为 absent，并断言准备按稳定 `ApiId` 排序、提交按 `CommitAfter` 排序。场景结束后删除临时文件；成功后更新 fixture owner 状态并允许报告加载成功。其余 9 项未运行。该场景在本轮恢复与无效拒绝结果处理变更前运行，没有重跑；当前代码改动尚无新增行为测试证据。生成源码位于 `Build/generated/Terraria.WorldLoadApiGenerationVerification/Debug/net10.0/WorldLoadApiGenerator/Terraria.WorldStorage.Generators.WorldLoadApiCatalogGenerator/WorldLoadApiCatalog.g.cs`；Application 生成文件位于相邻 `NSSLC.Application` 输出树。

未覆盖：重复身份和 section 的负向构建验证、无效版本和缺失/循环依赖诊断验证、必需 section 拒绝、Prepare 拒绝/异常清理、默认拒绝结果的运行时兜底、异常/空结果目录分支、主文件失败后由生命周期选择备份、备份恢复效果执行及其失败、Commit 失败与清理重建、缺失 owner binding 的运行验证、真实 owner 状态、实际 WorldFile 解码及真实文件读写。消费编译上的 section schema 输入与对应 WLA007-WLA012 诊断已经实现；负向诊断和恢复路径没有行为验证。生产接入前必须补齐这些证据；当前验证只证明通用载荷/协调路径与两项 fixture API 的成功场景。

### 加载尝试与恢复职责校正（2026-10-02）

静态对照设计第 4.6 节发现，早期 `WorldLoadCoordinator` 自行循环尝试并覆盖 `.bak`，与现有 `WorldLoadLifecycleSystem` 的主文件重试、备份检查/恢复状态重复。现已修正为：

- `WorldLoadCoordinator.Load(path, runtimeBindings)` 对指定路径执行一次读取、解码、整体校验及 API 目录调用；结果只描述本次调用，`Attempts` 为本次读取计数，不代表整个生命周期累计次数。
- coordinator 不含重试策略，不访问 `.bak`，也不改写或删除主文件；`WorldRecoveryPolicy` 已删除。主文件重试和备份恢复阶段由既有生命周期作出决策。
- 现有生命周期仍只是状态/动作模型；生产 Host 尚未把 coordinator 结果映射回 `CompleteLoadAttempt`，也未执行 `CheckBackup`、`RestoreBackupAndDelete` 等文件效果。因此端到端生命周期接线仍未完成，不能声称重试和备份已经在运行时工作。
- 唯一正向行为场景此前通过后，用户明确要求停止测试。本次及后续工作不再运行行为测试；本次只做静态检查和编译验证。

加载尝试职责修订后的编译验证：SDK `10.0.400`，执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。此命令仅编译 fixture 消费项目及其 ProjectReference，没有运行 fixture；成功编译 `NSSLC.Application`、`NSSLC.Infrastructure.WorldStorage` 和 fixture host，输出位于 `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll`、`Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll` 与 `Build/bin/Terraria.WorldLoadApiGenerationVerification/Debug/net10.0/Terraria.WorldLoadApiGenerationVerification.dll`。

### 生命周期失败转换补强（2026-10-03）

`WorldLoadLifecycleSystem.CompleteLoadAttempt` 现在要求成功完成前加载门已解除。新增 `FailBackupCheck` 与 `FailBackupRestore`：效果失败进入通用 `Failed` 终态并返回 `ReportLoadFailure`，不会把备份探测错误误报成“没有备份”，也不会在备份替换失败后继续加载。实际文件效果、Coordinator 结果到该状态机的 Host 映射及失败后的世界清理仍未接入；由于用户要求停止测试，本次没有运行恢复行为验证。

WorldSession 首次编译因缺少 `Build/obj/Terraria.WorldSession/project.assets.json` 以 `NETSDK1004` 失败。执行 `dotnet restore src/NSSLC/Component/WorldSession/Terraria.WorldSession.csproj --verbosity minimal` 后成功，再执行 `dotnet build src/NSSLC/Component/WorldSession/Terraria.WorldSession.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `14`、错误 `0`。14 条警告均为 `CS0436`，来自 WorldSession 对重复 `ColorRgba` 类型的编译选择；修改的 `WorldLoadLifecycleSystem.cs` 没有警告。程序集输出为 `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`。这是编译验证，不代表恢复路径行为已验证。

### 本地备份效果端（2026-10-03）

`WorldFileStoreAdapter.CheckBackupExists` 以只读文件句柄检查 `<worldPath>.bak`，不会为存在性探测加载整份世界文件；文件或父目录不存在时返回成功且 `backupExists=false`，其他 I/O 错误通过 `WorldStorageOperationResult` 分类。`RestoreBackupAndDelete` 先复制备份到同目录唯一临时文件，再替换主文件，最后移除备份；复制/替换失败会保留原主文件和备份，临时文件清理异常会附加到失败详情。备份删除失败时主文件可能已替换，操作返回失败，调用方不得继续加载；由于没有 Host 接线，这些结果当前尚无人映射到 `FailBackupCheck` / `FailBackupRestore`。

使用 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal` 编译，退出码 `0`，警告 `0`、错误 `0`。`NSSLC.Application` 与 `NSSLC.Infrastructure.WorldStorage` 输出位于 `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` 和 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。本次没有运行恢复行为测试。

### 强类型契约参数诊断补强（2026-10-03）

静态检查发现 WLA001 原先允许 `object`、`dynamic` 或顶层可空类型充当 owner context、section 和 prepared data 的类型参数，且允许将 `WorldLoadApiBindings` 作为 context，从而越过 owner 的 section 输入边界。生成器现将 `object`、`dynamic`、nullable reference、`Nullable<T>`、pointer、function pointer、`void` 以及 `WorldLoadApiBindings`/`WorldLoadApiRuntimeBindings` 子类判为无效 API 声明，并使用既有 WLA001 报错。另修正 WLA004 对显式 null `CommitAfter` 参数的处理：null 或非数组元数据会作为无效声明诊断，不按空依赖列表处理。未新增生产 API 或外部依赖。

使用 SDK `10.0.400` 执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。该命令只编译有效 fixture 和依赖项目，没有运行程序；生成器、Application、Infrastructure 和 fixture host 产物均在 `Build/bin/`。由于用户已要求停止测试，WLA001 新拒绝类型的负向诊断未运行验证，阶段 2 仍为 `partial`。

随后增加对共享 `WorldLoadApiBindings`/`WorldLoadApiRuntimeBindings` 根类型及其子类的拒绝后，再次执行相同 fixture `dotnet build`，退出码 `0`，警告 `0`、错误 `0`；generator、Application、Infrastructure 与 fixture host 均编译成功，输出仍在 `Build/bin/`。没有运行 fixture 或其他行为测试。

补充 null `CommitAfter` 元数据校验后再次执行相同编译命令，退出码 `0`，警告 `0`、错误 `0`；有效 fixture 和所有依赖项目均编译成功，生成器输出位于 `Build/bin/WorldLoadApiGenerator/Debug/net10.0/WorldLoadApiGenerator.dll`。本次未运行行为测试或无效元数据诊断验证。

生成目录清理失败聚合修订后的编译验证：SDK `10.0.400`，命令仍为 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。有效 fixture Host、生成器、Application 和 Infrastructure 均成功编译，产物位于 `Build/bin/`；没有运行 fixture 或行为测试。

生成代码多行格式修订后的编译验证（2026-10-03）：SDK `10.0.400`，命令 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。Generator、Application、Infrastructure 和 fixture host 均成功编译，程序集输出位于 `Build/bin/`，生成源码位于 `Build/generated/`；本次只编译，没有运行 fixture。当前 Application 的生成目录 descriptor 数量为 0，且 `src/` 下没有生产 `[WorldLoadApi]` 声明；标准 API 声明仅存在于验证 fixture。因此保留该编译期生成器是为了满足标准 API 的编译期收集与静态调用目录设计，不表示生产 System 已实现或接入标准 API。此前记录的生成哈希对应多行格式修订前版本；本次未做新的哈希稳定性比较。

组合 Host 目录桥接修订后的编译验证（2026-10-03）：`GeneratedWorldLoadApiCatalog` 新增构造函数，接收消费 Host 生成的 `Descriptors` 和 `Execute` 方法组，并复制 descriptor 列表；默认构造函数仍绑定 Application 自身生成的目录。验证 fixture 删除手写的 `IWorldLoadApiCatalog` 转接类型，改用此 Application 适配器。执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。Application、Infrastructure 和 fixture host 输出在 `Build/bin/`，生成源码仍在 `Build/generated/`。这是编译验证；没有运行 fixture，生产 Host 仍未创建或接线。

### 备份文件效果进入 Application 端口（2026-10-03）

`IWorldFileStore` 现在声明 `CheckBackupExists` 与 `RestoreBackupAndDelete`，使备份检查和恢复仍由生命周期决定时机，但宿主可通过 Application 所有的文件存储端口调用 Infrastructure 效果。`WorldFileStoreAdapter` 已有实现符合该接口；`WorldStorageCoordinatorFactory.CreateLoadCoordinator(IWorldFileStore, ...)` 可接收宿主持有的同一 store 实例，供 coordinator 读取主文件并由生命周期执行备份动作。协调器本身仍不选择 `.bak`、重试或替换文件。

使用 SDK `10.0.400` 编译 Infrastructure：`dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`；Application 与 Infrastructure 程序集输出在 `Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` 和 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`。随后编译验证 fixture：`dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`；fixture host 输出在 `Build/bin/Terraria.WorldLoadApiGenerationVerification/Debug/net10.0/Terraria.WorldLoadApiGenerationVerification.dll`。本次只编译，没有运行 fixture 或其他测试。端口现在具备备份文件能力，但生产 Host 尚未持有并调用该端口，也没有把这些结果映射到生命周期；真实 WorldFile codec 和生产 API 声明仍未实现，阶段 4、6、7 保持 `partial`。

### 生成目录诊断字面量转义修正（2026-10-03）

静态审查发现生成器把 API ID 原样拼入 UnsupportedFormatVersion 的生成消息；API ID 契约只要求非空，包含引号、反斜线或换行时会破坏生成的 C# 源码。现通过 Roslyn `SymbolDisplay.FormatLiteral` 对整条消息进行转义，与其它元数据字面量输出保持一致。执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，SDK `10.0.400`，退出码 `0`，警告 `0`、错误 `0`；generator、Application、Infrastructure 与 fixture host 输出均在 `Build/bin/`，生成源码输出仍在 `Build/generated/`。没有运行 fixture；含特殊字符 API ID 的生成结果尚未行为验证，阶段 2 仍为 `partial`。

### 目录执行异常按提交不确定处理（2026-10-03）

静态审查发现 `WorldLoadCoordinator` 原先把目录 `Execute` 抛出的异常归类为 `Binding`，使 `WorldRecoveryOutcome.RequiresWorldReset` 为 false；但自定义目录可能在异常前已提交部分 owner。现将描述符读取/计数异常保持为绑定失败（此时尚未调用 API），而目录执行抛异常、返回 null 或返回无效失败阶段时，归类为 `Commit`，从而保守要求世界状态清理/重建；`OperationCanceledException` 仍映射为 `Canceled`，同时保留重建要求。编译 `Test/Terraria.WorldLoadApiGenerationVerification`，SDK `10.0.400`，退出码 `0`，警告 `0`、错误 `0`，Application、Infrastructure、generator 和 fixture host 输出位于 `Build/bin/`。没有运行 fixture；新异常分支未做行为验证，阶段 6、7 仍为 `partial`。

### 绑定预检拒绝空实例（2026-10-03）

生成目录此前只检查 `TryGetApi` 和 `TryGetOwnerContext` 的布尔返回值；若实现错误地返回 `true` 并同时输出 null，缺陷会延迟到 API 调用时才暴露。现于任何 Prepare 调用前拒绝空 API 实例及空引用类型 owner context；值类型 owner context 不做 null 判断，以保留其默认值语义。构建后检查 `Build/generated/Terraria.WorldLoadApiGenerationVerification/Debug/net10.0/WorldLoadApiGenerator/Terraria.WorldStorage.Generators.WorldLoadApiCatalogGenerator/WorldLoadApiCatalog.g.cs`，确认两个 fixture API 的绑定预检都生成了 null 检查。执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，SDK `10.0.400`，退出码 `0`，警告 `0`、错误 `0`。本次只编译和静态检查生成源码，没有运行 fixture；空绑定拒绝和值类型 context 分支仍未做行为验证，阶段 4、7 保持 `partial`。

### 文件读取异常归一化（2026-10-03）

`WorldLoadCoordinator` 现在把文件端口抛出的 `OperationCanceledException` 转为 `Canceled`，其他意外异常转为 `Unknown`，使加载尝试仍以明确失败结果返回。它也拒绝 `Succeeded=true` 却同时带有失败类别的矛盾读取结果；正式 Infrastructure 适配器返回的细分文件错误分类不变。执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，SDK `10.0.400`，退出码 `0`，警告 `0`、错误 `0`；Application、Infrastructure、generator 和 fixture host 输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次没有运行 fixture；异常归一化分支仍未做行为验证，阶段 6、7 保持 `partial`。

### 生成命名空间转义程序集关键字（2026-10-03）

`GetCatalogNamespace` 现在将清洗后的程序集名作为 C# verbatim identifier 输出，避免诸如 `class` 这样的合法程序集名变成非法命名空间语法。执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，SDK `10.0.400`，退出码 `0`，警告 `0`、错误 `0`；Application、Infrastructure、generator 和 fixture host 输出均位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次只编译，没有运行 fixture；关键字程序集名分支尚无专门构建场景，阶段 2 仍为 `partial`。

### 描述符构造时校验协议不变量（2026-10-03）

`WorldLoadApiDescriptor` 构造函数现在与生成器保持一致，拒绝负格式版本、反向版本范围、未定义的 section requirement、空/重复依赖及自依赖，并继续防御性复制依赖列表。执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，SDK `10.0.400`，退出码 `0`、错误 `0`；共有 14 条 `Terraria.WorldStorage` 中未赋值/未使用字段的既有警告，修改的 descriptor 无新增警告。Application、Infrastructure、generator 和 fixture host 输出位于 `Build/bin/`。本次只编译，没有运行 fixture；不变量拒绝分支未做行为验证；阶段 1 的静态契约状态仍为 `done`，阶段 7 的整体验证仍为 `partial`。

### API 属性与 schema 属性约束一致化（2026-10-03）

`WorldLoadApiAttribute` 和 `WorldPersistenceSectionSchemaAttribute` 现在在构造阶段分别校验版本/依赖不变量以及 section 标识、类型和 requirement；它们与生成器的构建期检查和 `WorldLoadApiDescriptor` 的运行时检查保持一致。使用 SDK `10.0.400` 执行 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`；随后执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。产物位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次只编译，没有运行 fixture；新增构造拒绝分支未做行为验证。

### 缺失运行时绑定返回失败结果（2026-10-03）

`WorldLoadCoordinator.Load` 现在把运行时 API/context 绑定根对象为 null 的调用转换为 `InvalidData` 失败结果，并保持零次文件读取、零次 API 调用；此前的非空类型声明仍用于正常组合入口的编译期约束。使用 SDK `10.0.400` 执行 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`，Application 与 Infrastructure 输出位于 `Build/bin/`。本次没有运行测试；null 绑定分支尚无行为验证。

### 依赖边界与最终编译审计（2026-10-03）

静态审计确认 `NSSLC.Application` 只引用 `Terraria.WorldStorage` 和 SDK 生成器，`NSSLC.Infrastructure.WorldStorage` 只引用 Application 并显式编译文件适配器与组合工厂；正式项目没有引用 `分类参考`，也没有把未解耦的 `WorldFile/` 旧格式代码编入项目。生成输出仍位于 `Build/generated/`，正式程序集位于 `Build/bin/`。`WorldFile/` 中保留的 `MapFileName` 属于未编译的旧参考材料，不进入当前加载链；本设计继续不读取、生成或提交探索地图 `.map`。

最终增量编译：`dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，SDK `10.0.400`，退出码 `0`，警告 `14`、错误 `0`；警告均为 `Terraria.WorldStorage` 中既有的 `CS0649`/`CS0169` 字段。随后 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，退出码 `0`，警告 `0`、错误 `0`。本次没有运行测试或 fixture；行为预算仍保持既有 `1/10`，阶段 2、4、6、7 仍为 `partial`。

### Schema DTO 使用统一强类型校验（2026-10-03）

生成器的 assembly-level schema 检查现在复用 API 泛型参数的强类型规则，拒绝 `object`、`dynamic`、可空、`Nullable<T>`、指针、函数指针、`void` 以及 `WorldLoadApiBindings`/`WorldLoadApiRuntimeBindings` 子类作为 section DTO。这样 schema 不能绕过 owner 的窄输入边界或声明一个无法由有效 API 消费的弱类型 section。执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，SDK `10.0.400`，退出码 `0`，警告 `0`、错误 `0`；本次只编译，没有运行 fixture，负向 schema 诊断仍未运行验证。

### 生成契约拒绝 ref-like 类型（2026-10-03）

WLA001 的统一类型检查现在额外拒绝 ref-like 类型（例如 `Span<T>`），覆盖 API context、section 和 prepared data，避免它们进入生成目录中的泛型字段、局部变量或调用参数。执行 `dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal`，SDK `10.0.400`，退出码 `0`，警告 `0`、错误 `0`；本次只编译，没有运行 fixture，负向类型诊断仍未运行验证。

### Application 运行时目录描述符预检（2026-10-03）

新增 `WorldLoadApiCatalogValidation`，由 `WorldLoadCoordinator` 在调用生成目录前检查 descriptor 列表中的 null 项、重复 `ApiId`、重复 `SectionId`、未知 `CommitAfter` 依赖和依赖环。发现错误时返回 `Binding` 阶段失败，不执行任何 API；这为自定义组合目录保留了与构建期生成目录相同的运行时边界。执行 `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal`，SDK `10.0.400`，退出码 `0`，警告 `0`、错误 `0`；随后执行 fixture 编译，退出码 `0`，警告 `0`、错误 `0`。产物位于 `Build/bin/`，本次没有运行 fixture；非法目录分支未做行为验证。

### 取消结果独立状态化（2026-10-03）

`WorldRecoveryStatus` 新增 `Canceled`。`WorldLoadCoordinator` 现在将文件端口返回/抛出的取消、decoder/validator 取消以及目录执行异常映射出的取消结果返回为 `Status=Canceled`，不再只借助 `Status=Failed + Failure.Kind=Canceled` 表示；普通失败仍保持 `Failed`。Commit 阶段取消保留 `RequiresWorldReset`，供宿主决定清理/重建，coordinator 不重试。执行 Infrastructure 编译，SDK `10.0.400`，退出码 `0`，警告 `0`、错误 `0`；随后执行 fixture 编译，退出码 `0`，警告 `0`、错误 `0`。本次只编译，没有运行 fixture；取消分支仍未做行为验证。

### 备份恢复成功状态显式传入（2026-10-03）

`WorldLoadCoordinator` 新增三参数重载 `Load(path, runtimeBindings, successStatus)`，并保留两参数入口默认使用 `WorldRecoveryStatus.Loaded`。重载只接受 `Loaded` 和 `RecoveredFromBackup`；其他枚举值在读取文件前转换为 `InvalidData`，从而不会触碰文件或 owner API。备份恢复动作仍由生命周期/宿主通过 `IWorldFileStore.RestoreBackupAndDelete` 执行；替换成功后，宿主可将下一次加载标记为 `RecoveredFromBackup`，协调器不从路径或 `.bak` 文件名推断来源。使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

两次构建均退出码 `0`、警告 `0`、错误 `0`；程序集输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次没有运行 fixture 或其他行为测试，行为预算仍为既有 `1/10`，阶段 4、6、7 保持 `partial`。

### 非法目录结果保留取消原因与失败不变量（2026-10-03）

静态审查发现自定义 `IWorldLoadApiCatalog` 返回非法 `WorldLoadApiStage` 时，协调器会将结果归一化为 Commit 失败；如果原结果携带 `OperationCanceledException`，原先的归一化会丢失该异常，取消可能被误报为普通失败。现保留原结果的 `Exception` 和 `CleanupException`，同时保留 Commit 阶段的保守 `RequiresWorldReset` 语义。`WorldLoadApiExecutionResult.Failed` 也与 Prepare/Commit 结果一致，构造时拒绝无效 `WorldLoadApiFailure`，保证失败结果始终有稳定错误码和消息。使用 SDK `10.0.400` 串行编译 Infrastructure 和生成器 fixture：Infrastructure 退出码 `0`、14 条既有 `Terraria.WorldStorage` 字段警告、0 错误；fixture 退出码 `0`、0 警告、0 错误。输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次未运行行为测试，阶段 4、6、7 仍为 `partial`。

### 保存与加载复用文件端口（2026-10-03）

`WorldStorageCoordinatorFactory` 为 `CreateSaveCoordinator` 增加 `IWorldFileStore` 注入重载，并保留无参数入口创建正式 `WorldFileStoreAdapter`。这样组合根可以让保存和加载协调器共享同一个 Application 文件端口实例；工厂仍不持有 ECS Store、云端字段或参考目录实现。使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，两个项目均退出码 `0`、警告 `0`、错误 `0`；输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次未运行行为测试。

### 目录描述符预检异常归一化（2026-10-03）

`WorldLoadCoordinator` 现将 `WorldLoadApiCatalogValidation` 的枚举/索引异常纳入 Binding 异常边界；异常在任何 API Prepare 前转换为 `CatalogValidationException`。若异常为 `OperationCanceledException`，后续仍映射为 `WorldRecoveryStatus.Canceled`，不会被当作普通格式失败。使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，两个项目均退出码 `0`、警告 `0`、错误 `0`；本次未运行行为测试。

### 保存端口结果契约归一化（2026-10-03）

静态检查发现保存协调器对文件端口和编码器的矛盾结果缺少统一边界：例如返回 `Succeeded=true` 同时携带失败、成功读取却没有字节，或返回 `Succeeded=false` 却没有错误对象时，流程可能继续读取、写入或轮换备份。现将编码器、读取、写入和删除结果在 Application 边界归一化：成功结果带失败或缺少必要数据被拒绝为 `InvalidData`；编码器失败却缺少错误归一化为 `InvalidData`，文件端口失败却缺少错误归一化为 `IoFailure`；正常细分错误、取消和恢复顺序保持不变。该处理不引入重试，也不改变备份选择仍由生命周期负责的边界。

使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

两次构建均退出码 `0`、警告 `0`、错误 `0`；Application、Infrastructure、generator 和 fixture host 程序集输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次只做增量编译和静态检查，没有运行 fixture 或其他行为测试；行为预算仍为既有 `1/10`，阶段 4、6、7 保持 `partial`。

### 零备份策略边界修正（2026-10-03）

静态对照保存轮换流程发现，`WorldBackupPolicy.BackupsToKeep=0` 仍会把旧主文件写入 `.bak`，与零备份策略不符。`WorldSaveCoordinator.RotateBackups` 现会在备份数量为零时直接返回成功：主文件写入、读回和校验仍执行，但本次保存不会创建新的备份文件。该修正不改变加载协调器不选择或恢复 `.bak` 的职责边界，也不清理既有旧备份。

使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，两个项目均退出码 `0`、警告 `0`、错误 `0`；产物位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次没有运行行为测试，行为预算仍为 `1/10`。

### 加载读取结果契约归一化（2026-10-03）

`WorldLoadCoordinator` 现在与保存协调器使用一致的文件读取边界：文件端口返回成功但带失败、成功但无字节或失败却携带字节时，在进入解码前归一化为 `InvalidData`；失败但无错误时归一化为 `IoFailure`。真实适配器的 `Missing`、权限、I/O 和取消分类保持不变，协调器仍只读取调用方指定路径一次。

使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，两个项目均退出码 `0`、警告 `0`、错误 `0`；产物位于 `Build/bin/`，本次没有运行行为测试，行为预算仍为 `1/10`。

### 聚合清理取消语义修正（2026-10-03）

生成目录会将多个 `DiscardPrepared` 清理异常聚合保存。`WorldLoadCoordinator` 现递归检查主异常和清理异常中的 `OperationCanceledException`，包括 `AggregateException` 内层异常；只要加载 API 执行或清理边界观察到取消，结果就保持 `WorldRecoveryStatus.Canceled`，提交阶段仍保留世界级重置要求。使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；本次没有运行行为测试。

该递归取消判定同时覆盖文件端口读取和文档解码异常，避免聚合取消被降级为 `Unknown` 或 `InvalidData`。本次仅做增量编译，没有扩大行为验证范围。

保存协调器的编码、读回校验、读取、写入和删除包装也采用同一递归判定，聚合取消不会被降级为普通保存失败；本次仍只做编译验证。

### 空生成目录的显式拒绝（2026-10-03）

静态检查发现 Application 消费编译在当前没有生产 `[WorldLoadApi]` 声明时会生成空目录。此前该目录被直接调用时返回 `Completed`，会把“没有任何 API 可分派”误报为加载成功。`WorldLoadApiCatalogGenerator.AppendExecuteMethod` 现为零声明目录生成 `Binding` 阶段的 `MissingApiDeclarations` 失败；协调器已有的目录预检仍会在调用前拒绝空 descriptor。这样空目录既不能绕过生成目录直接使用，也不会改变本轮暂缓生产 System API 的范围。

使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

两次构建均退出码 `0`、警告 `0`、错误 `0`；Application、Infrastructure、generator 和 fixture host 产物位于 `Build/bin/`，生成源码位于 `Build/generated/`。静态检查 `Build/generated/NSSLC.Application/.../WorldLoadApiCatalog.g.cs` 已确认生成 `MissingApiDeclarations` 分支。本次没有运行 fixture 或其他行为测试，行为预算仍为既有 `1/10`，阶段 2、4、6、7 保持 `partial`。

### 提交结果矛盾状态收紧（2026-10-03）

静态审查发现 `WorldLoadCommitResult` 原来的公开位置构造方式允许 owner 返回“`Succeeded=true` 且 `Failure` 有效”的矛盾结果，生成目录会把它误当成成功并继续提交。现将两参数构造收为私有，仅保留 `Committed()` 与 `Rejected(failure)` 工厂；生成目录同时检查 `commitResult.Failure.IsValid`，任何矛盾状态都进入 Commit 失败路径并执行未提交 prepared data 的清理。该修正不改变 Commit owner 必须自行消费/释放输入的责任，也未新增生产 System API。

使用 SDK `10.0.400` 串行编译 Infrastructure 和生成器 fixture：Infrastructure 退出码 `0`、警告 `14`、错误 `0`（警告仍为 `Terraria.WorldStorage` 中既有 `CS0649`/`CS0169` 字段）；fixture 退出码 `0`、警告 `0`、错误 `0`。产物位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次只做编译和生成源码静态检查，没有运行行为 fixture，行为预算仍为 `1/10`。

### 失败阶段与提交结果不变量（2026-10-03）

`WorldLoadApiExecutionResult.Failed` 现在拒绝 `None` 或未定义的失败阶段，确保失败结果始终能定位到 Binding、Preparation 或 Commit。生成器对提交结果的矛盾分支统一生成 `InvalidCommitResult`，而不是沿用同时存在的 owner 错误码。使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，Infrastructure 退出码 `0`、警告 `14`、错误 `0`，fixture 退出码 `0`、警告 `0`、错误 `0`；没有运行行为 fixture，行为预算仍为 `1/10`。

### 基础设施聚合取消分类一致化（2026-10-03）

正式 `WorldFileStoreAdapter` 的异常分类现在递归检查 `AggregateException` 内层的 `OperationCanceledException`，与 Application 协调器和保存/加载包装保持一致。文件读取、写入、删除、备份探测和备份恢复遇到聚合取消时均返回 `Canceled`，不会降级为普通 I/O 失败；其他错误分类保持不变。Infrastructure 与生成器 fixture 的增量编译均退出码 `0`、警告 `0`、错误 `0`，Application、Infrastructure 和 fixture host 程序集输出位于 `Build/bin/`，生成源码位于 `Build/generated/`；本次没有运行行为 fixture。

### 运行时目录失败身份保留（2026-10-03）

`WorldLoadApiCatalogValidation` 现在在重复 `ApiId`、重复 `SectionId`、缺少 `CommitAfter` 依赖和依赖环时返回失败 descriptor 的 `ApiId/OwnerId`。依赖环选择实际环成员，不选择仅被环阻塞的下游 API；`WorldLoadCoordinator` 将这些身份带入 `WorldLoadApiExecutionResult`，使 Binding 失败可定位到具体 owner。使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；产物位于 `Build/bin/`，本次没有运行负向目录行为验证。

依赖环存在多个成员时，运行时校验按序比较 `ApiId`，选择字典序最小的实际环成员，避免自定义目录枚举顺序改变诊断身份。随后再次串行编译 Infrastructure 与 fixture，两个项目均退出码 `0`、警告 `0`、错误 `0`；没有运行行为测试。

### 取消恢复不触发重试（2026-10-03）

静态对照生命周期恢复规则后，`WorldLoadLifecycleSystem` 新增 `CancelRecovery`。该入口只接受 `PrimaryLoad`、`PrimaryRetry`、`BackupCheck`、`BackupRestore`、`BackupLoad` 和 `BackupRetry` 活跃阶段；它将加载结果标记为失败，直接进入 `Failed`，并返回 `ReportLoadFailure`，不会安排主文件或备份重试。非活跃阶段取消会被拒绝。这样协作式取消不会被 `CompleteLoadAttempt(loadFailed: true)` 误判为可恢复的普通加载失败。

使用 SDK `10.0.400` 完成受影响项目的增量编译：

```text
dotnet build src/NSSLC/Component/WorldSession/Terraria.WorldSession.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

三次构建均退出码 `0`。WorldSession 有 `14` 条既有 `ColorRgba` 冲突警告、`0` 错误；Infrastructure 和 fixture host 均为 `0` 警告、`0` 错误。程序集输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次只做静态检查和编译验证，没有运行 fixture；行为预算仍为 `1/10`，生命周期取消路径尚无行为测试证据，阶段 6、7 保持 `partial`。

### 生命周期保留取消状态（2026-10-03）

`WorldLoadLifecycleComponent` 新增只读 `LoadCanceled` 状态。`WorldLoadLifecycleSystem.CancelRecovery` 现在通过 `SetLoadCanceled` 记录取消，同时设置 `LoadFailed` 并直接进入 `Failed`；普通 `SetLoadResult` 会清除取消标记。这样生命周期状态能够区分协作式取消和普通失败，仍不安排任何主文件或备份重试，也不把完整载荷放入组件。使用 SDK `10.0.400` 编译 `WorldSession`，退出码 `0`、警告 `14`、错误 `0`；警告仍是既有 `ColorRgba` 冲突，程序集输出位于 `Build/bin/Terraria.WorldSession/Debug/net10.0/`。本次没有运行生命周期 fixture，阶段 6、7 保持 `partial`。

### 包装异常取消分类一致化（2026-10-03）

静态审查发现 Application 对普通 `InnerException` 包装的取消只检查外层异常和 `AggregateException`，可能将目录校验、解码器或文件端口的包装取消降级为 `Unknown` 或 `InvalidData`。新增应用层 `WorldStorageExceptionClassifier`，递归检查普通内部异常和聚合内部异常；文件端口抛出的常见缺失、权限、路径和 I/O 异常也在 Application 边界归一化为对应稳定类别。Infrastructure 的本地适配器同步递归检查普通内部异常；备份替换失败后的临时文件清理若被取消，现在保留 `Canceled` 类别。

使用 SDK `10.0.400` 串行增量编译：

```text
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

两次构建均退出码 `0`、警告 `0`、错误 `0`，Application、Infrastructure、generator 和 fixture host 产物位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次只做静态检查和编译验证，没有运行 fixture；行为预算仍为 `1/10`，阶段 4、6、7 保持 `partial`。

### 应用结果不变量收紧（2026-10-03）

`WorldRecoveryOutcome` 现在拒绝未定义状态、负尝试次数，以及“成功状态缺少成功 API 执行/仍带失败”或“失败状态携带成功 API 执行”的矛盾构造。`WorldSaveProjection` 同样拒绝已提交但带失败、或拒绝但没有失败原因的结果；两者仍由协调器在正常路径生成。使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`。本次只做编译和静态检查，没有运行行为测试，行为预算仍为 `1/10`。

### 保存命令显式携带持久化文档（2026-10-03）

静态对照保存流程发现 `WorldSaveCommand` 原先只有路径和备份策略，编码器没有显式快照输入，无法满足“编码器只消费 owner 快照/持久化 DTO”的边界。现将 `WorldPersistenceDocument` 加入保存命令构造参数；默认值构造的命令或缺失文档在进入编码器前返回 `InvalidData`。这一步只建立显式数据边界，不声称已经完成各 ECS owner 的快照采集、一致性屏障或真实 WorldFile 编码。

使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；产物位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次没有运行行为测试，行为预算仍为 `1/10`。

### 编码端口收窄为文档输入（2026-10-03）

保存编码端口 `IWorldSaveEncoder` 现在只接收 `WorldPersistenceDocument`，不再接收包含路径和备份策略的 `WorldSaveCommand`。路径、备份轮换和文件效果继续由 `WorldSaveCoordinator`/`IWorldFileStore` 负责，codec 只能处理显式持久化数据。使用 SDK `10.0.400` 串行编译 Infrastructure 与生成器 fixture，均退出码 `0`、警告 `0`、错误 `0`；本次没有运行行为测试。

### 宿主运行时绑定构建器（2026-10-03）

新增 `WorldLoadApiRuntimeBindingsBuilder`，供组合入口以显式泛型注册 API 实例和 owner context，
并在 `Build()` 时生成不可变的本次绑定快照。注册按 API 类型和 owner id 唯一匹配，重复注册立即
拒绝；绑定构建器不持有 `WorldPersistenceDocument`，不扫描程序集，也不改变生成目录的直接分派职责。
这一步只补齐宿主组合边界，仍未声明或实现任何生产 `[WorldLoadApi]`。

使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

三次构建均退出码 `0`、警告 `0`、错误 `0`；程序集输出位于 `Build/bin/`，生成源码位于
`Build/generated/`。本次只做静态检查和增量编译，没有运行 fixture 或其他行为测试，行为预算仍为
既有 `1/10`，阶段 4、6、7 保持 `partial`。

### 加载边界取消入口（2026-10-03）

`WorldLoadCoordinator` 新增带 `CancellationToken` 的 `Load` 重载，并在进入文件读取、解码、
整体校验和生成目录分派前后检查取消。协调器仍然只读取调用方指定的主文件一次，不选择或恢复
备份，不安排重试；取消结果统一为 `WorldRecoveryStatus.Canceled`。若 owner API 在 Commit 阶段
自行抛出取消异常，原有 `RequiresWorldReset` 语义继续保留，宿主仍必须清理或重建部分提交的世界。
文件端口、解码器和 owner API 的同步接口未被扩大，未引入异步或外部依赖。

使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

三次构建均退出码 `0`、警告 `0`、错误 `0`；程序集输出位于 `Build/bin/`，生成源码位于
`Build/generated/`。本次只做静态检查和增量编译，没有运行 fixture 或其他行为测试，行为预算仍为
既有 `1/10`，阶段 4、6、7 保持 `partial`。

### 生成目录阶段取消检查（2026-10-03）

`WorldLoadApiBindings` 增加本次调用的 `CancellationToken` 元数据，生成目录现在在绑定预检、每个
Prepare 和每个 Commit 调用前检查该令牌。Prepare 阶段观察到取消时仍通过既有生成清理函数逆序
丢弃已准备数据；Commit 阶段观察到取消时结果定位为 Commit，并继续要求世界级清理/重建。标准
`IWorldLoadApi<...>` 的强类型方法签名没有增加令牌参数，API 实例仍由宿主组合根提供。

使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC/Component/WorldStorage/Terraria.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

WorldStorage 退出码 `0`、14 条既有字段警告、0 错误；Application、Infrastructure 和 fixture
退出码均为 `0`、警告 `0`、错误 `0`。生成源码和程序集分别位于 `Build/generated/` 与
`Build/bin/`。静态检查确认 fixture 生成 catalog 包含绑定、Prepare 和 Commit 的取消检查；本次
没有运行 fixture 或其他行为测试，行为预算仍为既有 `1/10`，阶段 4、6、7 保持 `partial`。

### 构建期发现顺序稳定化（2026-10-03）

生成器的程序集、命名空间和嵌套类型遍历现在分别按程序集身份、元名称和类型元名称排序，
不再依赖 `HashSet` 或编译器自然枚举顺序。API 调用列表原有的 `ApiId`/依赖排序保持不变；
本次修正同时稳定了无效声明和重复身份诊断的发现顺序，不改变运行时分派，也不增加外部依赖。

使用 SDK `10.0.400` 编译生成器 fixture：退出码 `0`、警告 `0`、错误 `0`；程序集输出位于
`Build/bin/`，生成源码位于 `Build/generated/`。本次只做增量编译和生成源码静态检查，没有运行
fixture 或其他行为测试，行为预算仍为既有 `1/10`，阶段 2、4、7 保持 `partial`。

### 提交失败的生命周期重置门（2026-10-03）

`WorldLoadLifecycleSystem.CompleteLoadAttempt` 新增 `requiresWorldReset` 入口。协调器结果的
`RequiresWorldReset` 为真时，宿主可以将本次恢复直接转为终止失败，不再安排主文件或备份重试；
`WorldLoadLifecycleComponent.RequiresWorldReset` 记录待执行的世界级清理，调用
`MarkWorldCleared()` 后清除该标记。普通文件读取、解码和整体校验失败仍使用原有主文件重试顺序。
`CancelRecovery` 也支持记录取消后是否需要重置，从而覆盖 Commit 阶段取消的部分提交语义。

使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC/Component/WorldSession/Terraria.WorldSession.csproj --no-restore --verbosity minimal
dotnet restore Test/Terraria.WorldSession.P16LoadLifecycle.Verification/Terraria.WorldSession.P16LoadLifecycle.Verification.csproj
dotnet build Test/Terraria.WorldSession.P16LoadLifecycle.Verification/Terraria.WorldSession.P16LoadLifecycle.Verification.csproj --no-restore --verbosity minimal
```

WorldSession 退出码 `0`、14 条既有 `ColorRgba` 冲突警告、0 错误；生命周期验证项目 restore
后编译退出码 `0`、警告 `0`、错误 `0`。产物位于 `Build/bin/`。本次没有运行验证程序，行为
预算仍为既有 `1/10`，阶段 6、7 保持 `partial`。

### 有界 WorldFile header 解码与目录快照（2026-10-03）

`WorldFileDocumentDecoder` 已加入正式 Infrastructure 项目。它支持当前 pointer-based WorldFile
布局的版本 `88..319`，读取旧 `.wld` 的版本、World 文件元数据、section 指针、header 字段和版本条件字段，返回 Application 的不可变
`WorldFileHeaderSection`；读取过程始终限制在 header section 指针范围内，不调用旧 `WorldFile.cs`
中的 `Main`/`WorldGen` 全局状态，也不读取探索地图 `.map`。`WorldFileDocumentValidator` 检查
header section、世界边界、尺寸和游戏模式。`WorldStorageCoordinatorFactory.CreateLoadCoordinator`
新增默认 decoder/validator 入口，组合根仍需提供实际 catalog 和 owner 绑定。

`WorldLoadCoordinator` 在目录验证前复制 `IWorldLoadApiCatalog.Descriptors`，因此一次加载尝试
使用固定 descriptor 快照；目录列表为 null、索引异常或包含 null descriptor 时仍在 Binding 边界
失败。该快照不改变生成目录的直接调用，也不声称自定义 catalog 的执行实现已经具有事务性。

使用 SDK `10.0.400` 执行：

```text
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
```

两次构建均退出码 `0`、警告 `0`、错误 `0`；程序集输出位于 `Build/bin/`。本次只做静态检查和增量
编译，没有运行 decoder、fixture 或其他行为测试，行为预算仍为既有 `1/10`。完整 WorldFile
section、生产 owner API、lifecycle/Host 接线和旧格式往返验证仍未完成，阶段 4、5、6、7 保持
`partial` 或 `planned`。

随后重新编译 `Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj`
验证 descriptor 版本预检与生成 catalog 的引用闭包，退出码 `0`、警告 `0`、错误 `0`，宿主程序集
位于 `Build/bin/Terraria.WorldLoadApiGenerationVerification/Debug/net10.0/`；仍未运行该 fixture。

### 自定义 catalog 的格式版本预检（2026-10-03）

`WorldLoadApiCatalogValidation` 现在接收当前 `WorldPersistenceDocument.FormatVersion`，在
任何 API 的 Prepare/Commit 调用前检查每个 descriptor 的最小和最大格式版本。这样由组合根注入的
自定义 catalog 与生成 catalog 具有相同的 `UnsupportedFormatVersion` Binding 失败语义；失败结果
保留对应的 `ApiId` 和 `OwnerId`，不会让 API 通过自定义目录绕过版本声明。

使用 SDK `10.0.400` 增量编译 `NSSLC.Application` 与 `NSSLC.Infrastructure.WorldStorage`，两次
均退出码 `0`、警告 `0`、错误 `0`，输出位于 `Build/bin/`。本次没有运行行为测试，行为预算仍为
既有 `1/10`。

### 必需 section 统一 Binding 预检（2026-10-03）

`WorldLoadApiCatalogValidation` 现在同时接收当前文档的 `SectionIds`，在创建 API 实例、owner
context 或调用任何 Prepare/Commit 方法前，检查 `Required` descriptor 对应的 section 是否存在。
缺失时返回 `MissingRequiredSection` Binding 失败，并保留 descriptor 的 `ApiId` 与 `OwnerId`。
这条边界同时适用于宿主注入的自定义 catalog 和构建器生成的 catalog，不能通过自定义目录绕过
必需 section 声明；可选 section 仍由各 API 的 Prepare 逻辑决定如何处理。

使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

三次构建均退出码 `0`、警告 `0`、错误 `0`；Application、Infrastructure、generator 和 fixture
程序集输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次只做静态检查和增量编译，
没有运行 fixture 或其他行为测试，行为预算仍为既有 `1/10`。完整 WorldFile section、生产
owner API、Host/lifecycle 接线和负向行为验证仍未完成，阶段 4、5、6、7 保持 `partial` 或
`planned`。

### 旧版本种子标志语义对齐（2026-10-03）

静态对照旧 `WorldFile.LoadHeader` 发现，版本低于 `267` 时，旧格式没有单独存储 Zenith
标志，而是由 `DrunkWorld && RemixWorld` 推导；当该组合成立时，旧加载逻辑还会同步设置
`NoTrapsWorld=true`。`WorldFileDocumentDecoder` 现保留这一派生不变量，同时继续只写入
不可变 `WorldFileHeaderSection`，不调用 `Main` 或 `WorldGen`，也不读取 `.map`。

使用 SDK `10.0.400` 增量编译：

```text
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
```

构建退出码 `0`、警告 `0`、错误 `0`；Application、Infrastructure、generator 程序集均位于
`Build/bin/`。本次只做旧格式静态对照和增量编译，没有运行 decoder 或行为 fixture，行为预算
仍为既有 `1/10`。完整 WorldFile section、生产 owner API、Host/lifecycle 接线和真实旧格式
往返验证仍未完成，阶段 4、5、6、7 保持 `partial` 或 `planned`。

### 逐 API 绑定异常保留身份（2026-10-03）

生成目录的绑定预检现在分别保护 `TryGetApi`、`TryGetOwnerContext` 和
`TryGetSection<TSection>`。这些绑定实现若抛出异常，目录返回 `Binding` 阶段的
`ApiBindingException`、`OwnerContextBindingException` 或 `SectionBindingException`，并保留
当前 descriptor 的 `ApiId`、`OwnerId` 和原始异常；缺失值仍使用对应的 `Missing*` 失败码。
这样 section 类型不符或宿主绑定实现异常不会退化为无身份的通用 `BindingException`。

使用 SDK `10.0.400` 增量编译跨程序集 fixture：

```text
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

构建退出码 `0`、警告 `0`、错误 `0`；生成器、Application、Infrastructure 和 fixture 程序集
输出位于 `Build/bin/`，生成源码位于 `Build/generated/`。静态检查确认生成源码包含三类带
descriptor 身份的异常分支。本次没有运行 fixture 或其他行为测试，行为预算仍为既有 `1/10`；
负向绑定异常运行验证、完整 WorldFile section、生产 owner API 和 Host/lifecycle 接线仍未完成。

### 自定义目录与生成目录的版本预检顺序统一（2026-10-03）

Application 的 `WorldLoadApiCatalogValidation` 现在先检查 descriptor 的格式版本范围，再检查
`Required` section 是否存在，与生成目录的逐 API Binding 顺序一致。当两项同时无效时，自定义
catalog 和生成 catalog 都返回 `UnsupportedFormatVersion`；只有版本受支持时才继续报告
`MissingRequiredSection`。两种入口仍在任何 API 实例调用前完成全部目录级预检。

使用 SDK `10.0.400` 增量编译 Infrastructure（连带 Application）：

```text
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
```

构建退出码 `0`、警告 `0`、错误 `0`，程序集输出位于 `Build/bin/`。本次只做静态顺序对照和
增量编译，没有运行 fixture 或其他行为测试，行为预算仍为既有 `1/10`；负向版本/缺 section
行为验证、完整 WorldFile section、生产 owner API 和 Host/lifecycle 接线仍未完成。

随后重新编译跨程序集 fixture 以覆盖该 Application 侧变更：

```text
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

该构建退出码 `0`、警告 `0`、错误 `0`；fixture Host、Application、Infrastructure 和生成器
程序集输出均位于 `Build/bin/`，生成源码仍位于 `Build/generated/`。本次仍没有运行 fixture，
行为预算保持 `1/10`。

### WorldFile validator 版本边界闭合（2026-10-03）

`WorldFileDocumentValidator` 现在重复确认其负责的 pointer-based WorldFile 版本范围为
`88..319`。即使组合入口替换了 decoder，只要文档版本超出该范围，validator 也会在生成目录
和 owner API 调用前返回 `InvalidData`，不会让自定义 decoder 绕过 Infrastructure 的格式边界。
该检查不改变 Application 通用 validator 端口的可替换性，也不读取 `.map`。

使用 SDK `10.0.400` 增量编译 Infrastructure：

```text
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
```

构建退出码 `0`、警告 `0`、错误 `0`，Application 与 Infrastructure 程序集输出位于
`Build/bin/`。本次没有运行 decoder、validator 或行为 fixture；行为预算仍为既有 `1/10`。
完整 WorldFile section、旧格式往返和生产 owner/Host 接线仍未完成。

随后重新编译跨程序集 fixture 以覆盖 Infrastructure validator 变更：

```text
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

该构建退出码 `0`、警告 `0`、错误 `0`；Application、Infrastructure、generator 和 fixture
程序集输出均位于 `Build/bin/`，生成源码位于 `Build/generated/`。本次没有运行 fixture 或
其他行为测试，行为预算保持 `1/10`。

### 有界 WorldFile header/environment/progression/quest 编码器（2026-10-03）

Infrastructure 新增 `WorldFileDocumentEncoder`，实现 Application 的 `IWorldSaveEncoder`。
它只消费显式 `WorldPersistenceDocument`，仅支持当前 pointer-based WorldFile 版本 `319` 和
`world.header` 以及可选 `world.environment`/`world.progression`/`world.quests` section；会拒绝缺少 header、
未知 section、非法边界或不支持版本。编码器写入 WorldFile 元数据、两项 section 指针和有界
header 前缀字段，与正式 decoder 的读取布局一致；不读取
`Main`/`WorldGen`、不读取探索地图 `.map`，也不写入 Steam 云存档字段。工厂新增
`CreateSaveEncoder()`，组合根可以把它与 `WorldSaveCoordinator` 绑定。

使用 SDK `10.0.400` 增量编译：

```text
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
```

退出码 `0`、警告 `0`、错误 `0`；Application 与 Infrastructure 程序集输出位于 `Build/bin/`。
本次只做静态检查和增量编译，没有运行保存/加载 fixture，行为预算仍为既有 `1/10`。完整
WorldFile section 编解码、真实 owner 快照和 Host/lifecycle 接线仍未完成。

### WorldFile codec 格式边界集中（2026-10-03）

新增 `WorldFileFormatConstants`，由正式 decoder、validator 和 header encoder 共享版本范围、
元数据标识、文本长度及版本条件字段。这样 Infrastructure 的读取和写入边界不会因重复常量
漂移；旧全量 `WorldFile` 文件仍未加入正式项目编译。

随后使用 SDK `10.0.400` 增量编译同一 Infrastructure 项目，退出码 `0`、警告 `0`、错误 `0`，
输出仍位于 `Build/bin/`；没有运行新的行为测试，行为预算保持 `1/10`。

### WorldFile environment 前缀 section（2026-10-03）

新增 `WorldFileEnvironmentSection` 和 `world.environment` section。正式 decoder 在 header 的
身份/规则字段之后读取有限的环境前缀：背景样式、出生点、地表与岩层、时间/昼夜/月相、血月、
日食、地牢坐标和 Crimson 标志；所有值进入不可变持久化 DTO，不写入 ECS 或全局运行时。header
encoder 会编码该 section，缺少时使用显式空环境值；validator 会检查出生点、地牢坐标和地表/岩层
顺序。随后新增 `WorldFileProgressionSection` 和 `world.progression` section，按旧版本门读取
Boss/事件进度、入侵/降雨、矿层、背景样式和云层前缀；不执行旧加载器的派生修正或全局写入。
再新增 `WorldFileQuestSection` 和 `world.quests` section，按旧版本门读取 Angler 当日完成名单、
解锁标志、入侵起始规模和 Cultist 延迟。随后新增 `WorldFileBannerSection` 和
`world.banners` section，按参考实现读取 kill count 与版本 `>=289` 的 claimable count 数组，
并限制 `Int16` 数量范围。再新增 `WorldFileBossProgressionSection` 和
`world.boss-progression` section，按版本 `>=128` 读取快速到黎明标志，按版本 `>=131` 读取
Fishron、Martians、Ancient Cultist、Moon Lord 及节日 Boss 进度；版本更晚的 Party、DD2 和
其他 section 仍未迁移。随后新增 `WorldFilePartySection`、`WorldFileSandstormSection` 和
`WorldFileDefenderEventSection`，分别按版本 `>=170`、`>=174`、`>=178` 读取 Party、Sandstorm
和 DD2 进度前缀；再新增 `WorldFileBackgroundSection`、`WorldFileEventSection`、
`WorldFileTreeTopsSection` 和 `WorldFileSeasonalSection`，分别读取额外背景样式、Combat Book /
Lantern Night、TreeTops 变化值以及当日季节标志/矿层选择。随后新增
`WorldFileNpcUnlockSection` 和 `WorldFileTimePolicySection`，按版本门读取宠物、NPC/史莱姆解锁、
快速到黄昏、永久季节、种子、meteor/coin 计数和 `teamBasedSpawnsSeed`；随后新增 spawn section
读取 `ExtraSpawnPointManager` 的额外出生点、dual-dungeons 标志和原始 manifest 文本；完整 pointer
table 下还读取 Tile、Chest、Sign、NPC、WeightedPressurePlates、TownManager、Bestiary、Footer，
并保留 TileEntity/CreativePowers 的有界原始载荷。encoder 已补齐这些 section 的 framing 和
原始载荷写出，类型化内容仍由未来 owner 在 Prepare 阶段解码。

使用 SDK `10.0.400` 增量编译 Infrastructure 和跨程序集生成器 fixture：两个项目均退出码 `0`、
警告 `0`、错误 `0`，输出位于 `Build/bin/`。本次没有运行新的行为测试，行为预算仍为 `1/10`。

Progression/quest/banner/boss-progression/party/sandstorm/defender-event/background/event/tree-tops/seasonal/npc-unlocks/time-policy/spawn DTO 增量（含 `teamBasedSpawnsSeed`、dual-dungeons 标志和原始 manifest 文本）使用同一 Infrastructure 项目编译，退出码 `0`、警告 `0`、错误 `0`，
输出位于 `Build/bin/`。本次只做静态检查和增量编译，没有运行新的行为测试，行为预算仍为 `1/10`。

### WorldFile spawn/manifest 前缀（2026-10-03）

新增 `WorldFileExtraSpawnPoint` 和 `WorldFileSpawnSection`。正式 decoder 按版本门读取
`ExtraSpawnPointManager` 的 byte 数量及 `Int16` 坐标，读取版本 `>=304` 的 dual-dungeons 标志，
并在版本 `>=299` 时跳过旧布局中的兼容字段后保留 manifest 原始字符串；正式 encoder 只在
当前 version `319` 下写出对应的有界前缀，不依赖旧 JSON serializer。validator 限制出生点数量
和 manifest UTF-8 大小。此 section 仍是传输 DTO，不写入 ECS 或旧全局状态；encoder 会在完整
pointer table 中保留对应前缀。

使用 SDK `10.0.400` 增量编译：

```text
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

两次构建均退出码 `0`、警告 `0`、错误 `0`，输出位于 `Build/bin/`。本次只做静态检查和增量
编译，没有运行新的行为测试，行为预算仍为 `1/10`。

### WorldFile Chest/Sign 区段（2026-10-03）

正式 decoder 现在使用 pointer table 跳过压缩 Tile 区段，在存在完整 section pointers 时读取
Chest 和 Sign 区段。Chest 记录保留坐标、名称和有界 inventory 槽位（stack/type/prefix）；兼容
版本 `294` 前后的槽位计数布局。Sign 记录保留文本和坐标。两类数据只进入 Application DTO，
不调用 `Main`、`Chest`、`Sign` 或 ECS Store；数量、字符串长度、槽位数、坐标和 section 尾部都有
边界检查。encoder 现在与完整 pointer table 一起写出 Chest/Sign section，避免生成缺失后续
section 的正式 `.wld`。本次没有运行行为测试，预算仍为 `1/10`。

使用 SDK `10.0.400` 增量编译 `NSSLC.Application` 与 Infrastructure，均退出码 `0`、警告 `0`、
错误 `0`，输出位于 `Build/bin/`。

### WorldFile NPC 区段（2026-10-03）

正式 decoder 在存在 NPC section pointers 时继续跳过 Tile、Chest、Sign 区段并读取 NPC section：
保留 shimmered town NPC 标识、城镇 NPC 的名称/住宅/变体/位置字段，以及可保存 NPC 的类型和位置；
版本 `190` 前的 legacy 类型名、`213` 的变体标志、`268` 的 shimmered 列表和 `315` 的
`homelessDespawn` 均按旧布局读取。哨兵记录、记录数量、类型标识、有限浮点值、字符串和 section
尾部均有检查。DTO 不创建 `NPC` 实例，不写入 `Main.npc`，encoder 会按当前版本布局写出该
section。没有
运行行为测试，预算仍为 `1/10`。

### WorldFile Tile payload 区段（2026-10-03）

正式 decoder 在完整 pointer table 存在时读取 Tile section 的压缩字节和 frame-importance 位表，
并记录 header 中的世界尺寸。载荷由 `WorldFileTilePayloadSection` 持有，仍是不可变传输数据，
不会展开为 `Main.tile` 或其他全局对象；Tile owner 后续可在自己的 Prepare 阶段解压并校验。读取
使用 section pointer 边界，不读取探索地图 `.map`。encoder 会在 Tile payload 存在且 Footer
一致时保留压缩载荷和后续 section，行为测试仍停止在 `1/10`。

### WorldFile WeightedPressurePlates/Footer 区段（2026-10-03）

正式 decoder 在 version `>=170` 且 pointer table 足够时读取 WeightedPressurePlates 的坐标列表；
在完整 11 项 pointer table 存在时读取文件 Footer 的完成标志、世界名和 WorldId。两类数据分别
进入 `WorldFilePressurePlateSection` 与 `WorldFileFooterSection`，validator 检查坐标、数量、文本、
完成标志以及 Footer 与 Header 的身份一致性。TownManager、Bestiary、CreativePowers、TileEntity
等中间 section 仍不调用旧运行时注册表；encoder 会保留 DTO framing 和 opaque 原始载荷，
没有运行新的行为测试，预算仍为 `1/10`。

### WorldFile Boss progression 游标修正（2026-10-03）

对照参考 `WorldFile.LoadHeader` 后补齐版本 `>=140` 的九个 Lunar Tower 字段：四个塔的
`downed` 标志、四个塔的 `active` 标志和 `LunarApocalypseIsUp`。字段进入
`WorldFileBossProgressionSection`，decoder/encoder 使用同一版本门和顺序；旧版本仍以 `false`
作为缺省值。此前缺失这些字节会使真实 version `319` 存档在 Party 及后续 header section
处错位。另增加 header section 尾部消费检查，未识别的 header 字段会被拒绝而不会静默跳到
Tile pointer。此次只做静态对照和串行增量编译，不运行新的行为测试，预算保持 `1/10`。

### WorldFile header 指针与保存侧前缀校验（2026-10-03）

继续对照旧 `WorldFile.LoadWorld_Version2` 的游标语义，`WorldFileDocumentDecoder` 现在要求
header 起始指针严格等于重要性表读取后的当前位置。指针超前的文件会以 `InvalidData` 拒绝，
不会跳过未识别字节后继续构造持久化文档。该修改仍只在正式
`src/NSSLC.Infrastructure/WorldStorage/WorldFile/` 中生效，不读取 `.map`，也不调用旧全局
加载器。

`WorldFileDocumentEncoder` 补充与读取侧一致的前缀检查：环境 section 的出生点、地牢坐标、
地表/岩层顺序必须符合 header；progression 的浮点值必须有限；banner kill count、Party
cooldown、Lantern Night cooldown 和固定背景数量必须在有界范围内。
`WorldFileDocumentValidator` 同步拒绝负 banner kill count、Party cooldown 和 Lantern Night
cooldown。这样直接调用 encoder 时也不会生成随后 validator 无法接受的前缀数据；Tile、实体
和其他后续 section 的 DTO/opaque framing 由正式 encoder 保留，类型化 owner 解码仍未接入。

使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
```

两次构建均退出码 `0`、警告 `0`、错误 `0`；程序集输出位于 `Build/bin/NSSLC.Application/Debug/net10.0/`
和 `Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/`。本次只做静态检查和增量编译，
没有运行新的行为测试，行为预算仍为既有 `1/10`。完整 WorldFile section、生产 owner API、
Host/lifecycle 接线、旧格式往返和保存文件行为验证仍未完成，阶段 4、5、6、7 继续保持
`partial` 或 `planned`。

### WorldFile pointer table 尾部边界（2026-10-03）

继续检查 pointer table 的截断语义。参考 `WorldFile.LoadWorld_Version2` 会按固定索引读取
11 项指针，因此正式 decoder 对当前支持的版本要求指针表恰好为 11 项；少于或多于 11 项的
文件在读取 section 前以 `InvalidData` 拒绝。完整 11 项表仍由 Footer section 消费到文件末尾，
不会把未识别的后续字节或缺失的后续 section 静默当作成功加载。

使用 SDK `10.0.400` 串行编译 Application 与 Infrastructure，均退出码 `0`、警告 `0`、错误 `0`，
程序集输出仍位于 `Build/bin/`。本次只做静态检查和增量编译，没有运行新的行为测试，行为预算
保持 `1/10`；完整 section 映射、生产 owner/System API 和 Host/lifecycle 接线仍未完成。

### WorldFile 完整性校验与默认加载组合入口（2026-10-03）

`WorldFileDocumentValidator` 现在要求 decoded document 同时包含 Tile payload 和 Footer，并复核
Header 名称/Seed 文本、Tile 尺寸与有界压缩载荷，以及 Footer 完成标志和 Header 身份一致性。
NPC DTO 还要求 town NPC 只能位于 town 列表、saved NPC 只能位于 saved 列表，避免 encoder 忽略
错误列表标志而静默改变记录形态。`WorldStorageCoordinatorFactory.CreateLoadCoordinator()` 新增
默认组合入口，使用 `GeneratedWorldLoadApiCatalog`；当前没有生产 API 声明时，加载会明确返回
`MissingApiDeclarations`，不会回退到运行时扫描或把纯解码报告为成功。

边界复核后，Infrastructure 工厂的无参 `CreateLoadCoordinator()` 入口已移除：生成目录由宿主
组合根显式传入 `IWorldLoadApiCatalog`。这样避免把当前 Application 的空生成目录误当作生产目录；
上段记录保留为历史状态，现行入口以显式目录重载为准。

使用 SDK `10.0.400` 串行编译 Application 与 Infrastructure，均退出码 `0`、警告 `0`、错误 `0`，
输出位于 `Build/bin/`。本次没有运行新的行为测试，行为预算保持 `1/10`；生产 owner/System API
和 Host/lifecycle 接线仍未完成。

### WorldStorage 保存 read-back 校验入口（2026-10-03）

`WorldStorageCoordinatorFactory` 新增 `CreateSaveValidationQuery()`。该入口为
`WorldSaveCoordinator` 提供正式的 read-back 校验函数：保存后的字节先经
`WorldFileDocumentDecoder` 解码，再由 `WorldFileDocumentValidator` 做整体校验；解码失败或
校验失败均返回 `false`。同时提供接受 `IWorldPersistenceDocumentDecoder` 和
`IWorldPersistenceDocumentValidator` 的重载，保持保存 read-back 与 load 组合入口相同的可替换
端口边界。宿主仍需显式组合 `CreateSaveCoordinator()`、`CreateSaveEncoder()` 和该 query，工厂
不会读取备份、选择恢复路径或写入 ECS。该入口只复用现有正式 codec，不引入外部库、Steam 云
字段或探索地图数据。

使用 SDK `10.0.400` 串行编译 Application 与 Infrastructure，均退出码 `0`、警告 `0`、错误 `0`，
输出位于 `Build/bin/`；本次没有运行新的保存行为测试，行为预算保持 `1/10`。当时的保存
当时的 encoder 仍是有界前缀实现；后续已在下面的 pointer table 增量中补齐完整 section framing。

### WorldFile 完整 pointer table 保存对齐（2026-10-03）

继续对照 `WorldFile.SaveWorld_Version2` 的 11 项 pointer table，正式 encoder 现在要求
Tile payload 和 Footer 存在，并写出 header、Tile、Chest、Sign、NPC、TileEntity、压力板、
TownManager、Bestiary、CreativePowers 和 Footer 的完整 section framing。压缩 Tile 数据以及
TileEntity/CreativePowers 的原始载荷按 DTO 原样保留；Chest、Sign、NPC、TownManager 和 Bestiary
使用强类型 DTO 重写。未知 section、Footer 与 Header 身份不一致或 Tile 尺寸不匹配时保存直接
失败，避免静默丢失未识别字段。encoder 仍不读取全局状态、探索地图或外部库。

使用 SDK `10.0.400` 串行编译 Application 与 Infrastructure，均退出码 `0`、警告 `0`、错误 `0`，
输出位于 `Build/bin/`。本次没有运行新的行为测试，行为预算保持 `1/10`。生产 owner/System API、
真实 Host/lifecycle 接线和跨版本往返验证仍未验收。

### 加载期间目录描述符稳定性检查（2026-10-03）

`WorldLoadCoordinator` 在调用目录前仍使用本次加载的描述符快照；现在在目录成功返回后，
会再次读取目录描述符并逐项比较 API 身份、owner、section、格式版本、section requirement
和 `CommitAfter` 顺序。若自定义组合目录在执行期间改变了描述符，协调器返回
`CatalogChangedDuringLoad` 的 Commit 失败，并将 `RequiresWorldReset` 置为真，避免已经可能
提交部分 owner 状态时发布 `WorldLoaded`。描述符读取异常也被保留为 Commit 失败的原始异常；
生成目录使用不可变描述符列表，因此不改变现有生成路径。

使用 SDK `10.0.400` 串行编译 Application 与 Infrastructure：两个项目均退出码 `0`、警告 `0`、
错误 `0`，输出位于 `Build/bin/`。本次只做增量编译，没有运行行为测试，行为预算保持既有 `1/10`；
自定义目录并发变更分支仍未做行为验证。

### Tile 区段非空完整性检查（2026-10-03）

正式 WorldFile codec 现在把 Tile frame-importance 表和压缩 Tile payload 的空值视为格式错误：
decoder 在读取阶段拒绝零项重要性表和零长度 Tile 区段，validator 与 encoder 使用相同的非空
边界。这样损坏文件不会因为“section 存在”而进入 owner Prepare，也不会由保存侧写出随后无法
提供实际 Tile 数据的文件。该检查仍只针对 `.wld`，不读取或生成探索地图 `.map`。

使用 SDK `10.0.400` 串行编译 Application 与 Infrastructure：两个项目均退出码 `0`、警告 `0`、
错误 `0`，输出位于 `Build/bin/`。本次只做增量编译，没有运行行为测试，行为预算保持既有 `1/10`。

### WorldFile 本地 metadata section（2026-10-03）

正式 decoder 不再丢弃 WorldFile metadata：版本 `>=135` 的文件现在生成不可变
`WorldFileMetadataSection`，保留文件 `Revision` 和 `IsFavorite`；encoder 接收该 section 并
写回对应 metadata，缺少时使用 revision `1`、favorite `false` 的本地默认值。该 section 属于
文件格式元数据，不是 ECS 状态，不包含云端路径、Steam 字段或云端操作；validator/read-back
仍在 Application 边界完成。

使用 SDK `10.0.400` 串行编译 Application 与 Infrastructure，均退出码 `0`、警告 `0`、错误 `0`，
输出位于 `Build/bin/`。本次没有运行新的行为测试，行为预算仍为 `1/10`；完整旧格式往返和
生产 owner/System API 仍未验收。

### 本轮并行审计收口（2026-10-03）

协调器轨补充了目录描述符的防御性校验：在任何 owner API 调用前拒绝空或非法的
`ApiId`、`OwnerId`、`SectionId`、版本范围和 `Requirement`，并拒绝空依赖、重复依赖及
自引用的 `CommitAfter`。这些分支统一作为 Binding 失败返回，并保留失败 API/owner 身份；
没有修改公开标准 API、生产 System API 或行为测试预算。

WorldFile 轨使用仓库中的 version `319` 最小 `.wld` 样本做只读往返核对：正式 decoder 成功
解析，正式 validator 返回 `None`，encoder 回写后文件长度仍为 `15,202` 字节，输入和输出
SHA-256 均为
`72AC2E7AB6EAA47FEADA2911D8F9DA03AD217A509ADC0118595E350677483745`。该证据只覆盖该
最小样本的当前 codec 路径；旧版本、复杂存档、所有 owner 映射、生产生命周期接线和失败
行为矩阵仍未验收，不能替代完整 WorldFile 兼容性测试。

主代理按依赖顺序使用 SDK `10.0.400` 串行执行：

```text
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
```

两条命令均退出码 `0`、警告 `0`、错误 `0`。程序集输出位于
`Build/bin/NSSLC.Application/Debug/net10.0/NSSLC.Application.dll` 和
`Build/bin/NSSLC.Infrastructure.WorldStorage/Debug/net10.0/NSSLC.Infrastructure.WorldStorage.dll`；
生成源码仍位于 `Build/generated/`。本轮没有运行新的行为测试，行为预算保持 `1/10`，
执行计划状态继续为 `executionStatus: in-progress`、`implementationStatus: partial`、
`verificationStatus: partial`。

随后生成器/Host 轨收紧了组合入口：移除 `WorldStorageCoordinatorFactory` 的无参
`CreateLoadCoordinator()`，保留显式传入 `IWorldLoadApiCatalog`（以及可替换 decoder、
validator、file store）的重载。Infrastructure 不再把当前为空的 Application 生成目录
误当作生产目录，真实宿主必须在组合根提供 catalog。变更后按依赖顺序重新编译：

```text
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
```

三条命令均退出码 `0`、警告 `0`、错误 `0`，输出分别位于 `Build/bin/NSSLC.Application/`、
`Build/bin/NSSLC.Infrastructure.WorldStorage/` 和
`Build/bin/Terraria.WorldLoadApiGenerationVerification/`。fixture 仅作编译验证，未重新运行
行为场景；行为预算仍为 `1/10`，生产 System API 和真实 Host/lifecycle 接线仍按计划暂缓。

### 代码收口后的集中审核与定向验证（2026-10-03）

本轮先完成允许范围内的代码实现，再集中审核。审核覆盖 Application 协调器和保存边界、
生成器及其生成输出、Infrastructure WorldFile codec 和文件适配器、WorldSession 加载状态机，
并检查了项目引用方向、正式项目的显式编译清单、`Build/generated/` 输出位置以及
`分类参考` 隔离。审核确认：

- `NSSLC.Application` 只依赖 `Terraria.WorldStorage` 契约和 SDK 自带的生成器；
  `NSSLC.Infrastructure.WorldStorage` 只实现 Application 端口，不被 Application 反向引用。
- `WorldStorageCoordinatorFactory` 的加载入口必须显式接收 `IWorldLoadApiCatalog`，不会把空的
  Application 生成目录误当成生产目录；协调器单次只读调用方路径，不选择或改写 `.bak`。
- 正式 WorldFile 项目只编译解耦后的 decoder、validator、encoder、格式常量、文件适配器和
  工厂；旧全量 `WorldFile.cs` 及其辅助文件仍未纳入编译，`分类参考` 没有被编译或引用。
- 工作代码没有 Steam 云存档字段/参数/函数，也没有读取、生成或提交探索地图 `.map`；旧参考
  文件中的 `MapFileName` 不在正式编译链中。
- 生成源只出现在 `Build/generated/`，程序集只出现在 `Build/bin/`；`git diff --check` 无
  空白错误。

集中审核后使用 SDK `10.0.400` 串行执行以下构建：

```text
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC/Component/WorldSession/Terraria.WorldSession.csproj --no-restore --verbosity minimal
```

四条命令均退出码 `0`、警告 `0`、错误 `0`；输出分别位于 `Build/bin/NSSLC.Application/`、
`Build/bin/NSSLC.Infrastructure.WorldStorage/`、`Build/bin/Terraria.WorldLoadApiGenerationVerification/`
和 `Build/bin/Terraria.WorldSession/`。随后只运行既定的唯一行为场景：

```text
dotnet run --no-build --no-restore --project Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj
```

命令退出码 `0`，输出为 `World load API generation verification passed.`。本场景验证跨程序集
API 发现、Optional Absent、Prepare/Commit 顺序和协调器分派；行为预算保持 `1/10`，没有运行
完整测试集或新增行为场景。阶段 3、5 以及阶段 6 中的生产 owner、Host/lifecycle 接线和
恢复失败矩阵仍按用户约束暂缓，阶段 7 只完成本轮构建、静态审核和该定向验证。

随后仅修正正式 validator 中 Party section 条件的缩进，并重新编译受影响的
`NSSLC.Infrastructure.WorldStorage` 与 fixture。两条构建命令均退出码 `0`、警告 `0`、错误
`0`，输出仍位于 `Build/bin/`；重新运行同一 fixture 仍输出
`World load API generation verification passed.`，行为预算不变。

### 目标完成审计复核（2026-10-03）

按当前工作区重新执行计划中的受影响项目构建：

```text
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal
dotnet build Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj --no-restore --verbosity minimal
dotnet build src/NSSLC/Component/WorldSession/Terraria.WorldSession.csproj --no-restore --verbosity minimal
```

四条命令均退出码 `0`、警告 `0`、错误 `0`，程序集输出位于各自的 `Build/bin/` 子目录。
再次构建 fixture 后，生成文件
`Build/generated/Terraria.WorldLoadApiGenerationVerification/Debug/net10.0/WorldLoadApiGenerator/`
下的 `WorldLoadApiCatalog.g.cs` SHA-256 为
`AD3336C1650B291E7B17C04FA97F9E2C885632FF16D1B0A9241D3BBC1ACEA449`，重建前后相同，证明
当前输入下生成结果稳定。随后执行：

```text
dotnet run --no-build --no-restore --project Test/Terraria.WorldLoadApiGenerationVerification/Terraria.WorldLoadApiGenerationVerification.csproj
```

退出码为 `0`，输出为 `World load API generation verification passed.`。本轮行为测试仍只有
一个正向场景，即 `1/10`；负向诊断、生产 owner、生产 Host/lifecycle 和完整旧格式行为仍是
文档中明确保留的后续范围，而不是本轮暂缓限制之外的遗漏。


## WorldFile 版本 326 读取支持（2026-10-03）

- 正式 codec 将读写版本上限分开：decoder/validator 接受 88–326，encoder 仍只写 319。在 version 323 起，spawn/header 尾部按 WorldFile 顺序读取并保留 moreLightningSeed、noLightningSeed，随后读取既有 Manifest 字符串。版本 319 的 encoder 拒绝带有这两个新旗标的文档，避免无声丢弃字段。
- 格式依据包括本机 Terraria 1.4.5.8 的 Terraria.IO.WorldFile：LoadWorldFlags 在版本 >=323 读取上述两个布尔字段，再读取 WorldGen.Manifest；保存路径写入相同顺序。公开 tModLoader 1.4.5 分支也保留 vanilla WorldFile 的 num > 326 读取上限：[WorldFile.cs.patch @78e43ac](https://github.com/tModLoader/tModLoader/blob/78e43ac6cf5f187453749da632a3cf8c58533651/patches/TerrariaNetCore/Terraria/IO/WorldFile.cs.patch#L4-L8)。
- validator 的游戏模式上限调整到 3，以接受 Journey 模式；版本 209 起该字段保存整数模式值。
- SDK 10.0.400 命令 dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity minimal 退出码 0，警告 0、错误 0；程序集输出在 Build/bin/。
- 按 10% 范围，只用 src/World/科研.wld 做一条 coordinator 真实文件尝试。样本 SHA-256 为 47BF7220BA70D32C667ED84B22D316C564D46086CC78036278D3D77D7885027B。文件读取、版本解码、section 消费和整体校验均通过；Coordinator 到 Binding 阶段以 MissingApiDeclarations 失败。此状态符合当前生产系统没有标准 API 声明的边界，不表示 ECS owner 加载已完成。
- 未运行全量测试；没有读取探索地图 .map 或修改 分类参考。326 写入、owner API 声明与生产 Host 接线仍不在本次范围。