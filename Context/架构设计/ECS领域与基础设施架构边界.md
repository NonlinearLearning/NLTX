# ECS 领域层与基础设施六边形架构边界

## 1. 文档状态

| 项目 | 决定 |
| --- | --- |
| 适用范围 | ECS 领域状态、世界持久化、文件访问、平台服务及其组合入口 |
| 设计状态 | 已确认 |
| 确认日期 | 2026-10-02 |
| 规范性质 | 架构设计；约束职责归属和依赖方向，不规定唯一框架或固定项目数量 |
| 关联约束 | [ECS 文件组织设计约束](ECS文件组织设计约束.md)、[副作用隔离规范（草案）](../约束/非函数式编码副作用隔离规范.md) |

## 2. 决定

ECS 只用于表达游戏领域的运行时实体/世界状态，以及围绕这些状态执行的领域行为。基础设施不采用 ECS 组织：文件、云存储、序列化、备份和平台能力使用普通 .NET 类型组织，并按六边形架构（Ports & Adapters）隔离外部效果。

本决定不要求把所有持久化相关代码放入一个新项目，也不要求所有应用代码变成纯函数。项目边界依据实际依赖、部署、测试或发布需求形成；在边界尚未稳定时，可在现有程序集内使用清楚命名的普通类。

## 3. 职责归属

| 层 | 负责内容 | 不负责内容 |
| --- | --- | --- |
| ECS 领域层 | 权威游戏状态、领域不变量、规则判断和受控状态提交；由领域 owner 捕获一致的只读快照 | 文件路径、文件句柄、云端客户端、字节流、备份目录、序列化器配置和平台异常 |
| 应用用例/协调层 | 接收保存或加载意图；编排快照采集、编码/解码、校验、存储调用、恢复与结果处理 | 直接访问操作系统文件 API；绕过领域 owner 修改组件状态 |
| 端口 | 声明用例所需的外部能力及稳定输入/结果契约，例如世界存储或文件存储能力 | 持有具体平台客户端或隐式引用 ECS 内部组件 |
| 基础设施适配器 | 实现端口；执行本地/云端文件访问、格式编解码及平台错误分类、资源释放 | 拥有游戏模拟规则或权威运行时状态 |
| 组合入口 | 创建并连接 ECS、用例、端口实现和宿主配置 | 将依赖注册顺序或文件枚举顺序当作业务调度规则 |

应用协调代码不属于 ECS，即使它会调用 ECS 的快照 API 或根据存储结果请求领域系统提交加载结果。`System` 一词保留给项目定义的 ECS 执行/行为单元；基础设施和应用类使用 `Coordinator`、`Workflow`、`Store`、`Codec`、`Adapter` 等能反映实际职责的名称。

## 4. 状态、快照与持久化边界

1. 只有运行时领域需要读取、修改或维护不变量的事实才建模为 ECS Component。是否要保存该事实，不决定它是否是 Component。
2. 持久化快照和 DTO 是跨层数据契约，不是 ECS Component。快照应是不可变或防御性复制的数据，只包含明确列入存档的字段，不暴露组件、可变集合或运行时服务。
3. 快照由权威领域 owner 在一致的状态边界捕获。编码器和文件适配器只能消费显式传入的快照/DTO，不能通过全局状态或 ECS 查询自行寻找存档数据。
4. 路径、云端选择、备份策略、事务阶段、锁、重试状态、序列化格式选项、字节缓冲和文件错误属于应用/基础设施数据。只有某项操作状态确实影响游戏规则或需要随实体/世界生命周期参与模拟时，才另行评估是否进入 ECS。
5. 加载数据须在应用边界完成格式解析和存档有效性检查，再由对应领域 owner 校验并提交到权威状态；适配器不得直接写组件或把未校验数据当作已提交状态。

## 5. 依赖与效果方向

依赖由外向内指向契约和领域能力，运行时具体组合由宿主完成：

```text
Host / Composition Root -> ECS Domain API + Application Use Case
Application Use Case -> Application-owned ports and persistence DTOs
Infrastructure Adapter -> implements Application-owned ports
```

基础设施可以依赖稳定的端口、持久化 DTO 和必要的领域公开契约，但不得依赖 ECS 的内部组件存储、系统调度或更新生命周期。ECS 领域层不依赖文件/云平台实现。端口由需要该能力的内层用例/契约定义，基础设施提供实现；只有当现有项目边界无法承载稳定契约时，才新增独立 Contracts/Application 项目。

保存流程的典型效果顺序为：领域快照捕获 -> 映射为持久化 DTO -> 编码 -> 经存储端口提交 -> 按策略回读/验证与备份 -> 返回明确结果。加载流程为：经存储端口读取 -> 解码/迁移 -> 应用级校验 -> 交给领域 owner 校验并提交 -> 在提交完成后发布相应领域通知。具体顺序、部分成功、超时结果未知、取消和恢复语义须由对应用例明确。

## 6. 目录与类型组织指导

按当前文件组织约束，领域代码仍先按领域归档；基础设施按外部能力和稳定职责组织，不建立 ECS 组件目录。下列路径是逻辑归属示意，不是当前源码路径清单：

```text
src/NSSLC/Component/WorldSession/       # 世界会话权威状态及领域快照值
src/NSSLC.Infrastructure/WorldStorage/  # 文件/云端适配器、格式 Codec 和平台实现
src/Server/Composition/                 # 宿主组合入口（按实际项目边界）
```

上面的路径是职责归属示意，不表示这些项目或空目录都应立即创建。

`src/NSSLC.Infrastructure/分类参考/` 是参考材料，不是生产实现目录；生产项目不得从该目录编译或引用实现。保存与恢复用例分别由 [WorldSaveCoordinator.cs](../../src/NSSLC.Application/WorldStorage/Persistence/WorldSaveCoordinator.cs) 和 [WorldLoadCoordinator.cs](../../src/NSSLC.Application/WorldStorage/Persistence/WorldLoadCoordinator.cs) 承载。正式的 [WorldFileStoreAdapter.cs](../../src/NSSLC.Infrastructure/WorldStorage/Platform/WorldFileStoreAdapter.cs) 实现 Application 文件端口并执行本地字节读写；[WorldStorageCoordinatorFactory.cs](../../src/NSSLC.Infrastructure/WorldStorage/WorldStorageCoordinatorFactory.cs) 将该适配器绑定到 Application 协调器，供宿主组合入口创建，并提供正式 WorldFile decoder/validator/encoder。`src/NSSLC.Infrastructure/WorldStorage/WorldFile/` 的正式 codec 不依赖全局状态：decoder/validator 接受 pointer-based `.wld` 版本 `88–326`，encoder 仍只写版本 `319`；二者共享 section pointer 边界和 header 前缀布局。decoder 按版本门保留压缩 Tile payload、Chest、Sign、NPC、TileEntity、WeightedPressurePlates、TownManager、Bestiary、CreativePowers 和 Footer 的 Application DTO 或有界原始载荷。WorldFile 319 的生成世界组合已通过 Application 加载 API，在 Prepare 阶段调用注入的 TileEntity codec 完成类型化解析，再交由领域 owner 提交；CreativePowers 当前接受标准新世界的空状态，非空载荷明确拒绝。旧全量 WorldFile 实现继续排除在正式项目编译之外。

### 后续 WorldFile 区段边界

正式 Infrastructure decoder 可把 `TownManager` 和 `Bestiary` 作为不可变 Application DTO 传递，把 `TileEntity` 和 `CreativePowers` 作为有界原始 section payload 传递。该载荷不得直接写入 ECS Store；对应 owner 必须在自己的 Prepare/Commit API 中完成类型化解码、领域校验和提交。WorldFile 319 的生成世界组合现已提供完整 27 个逻辑区段 API 与 bindings；地块压缩及生成结果 DTO 投影位于正式 WorldStorage 边界，其他版本不自动获得这些新增 owner API 的兼容性。生产 encoder 会保留这些区段的 framing 和原始载荷；它要求 Tile payload 与完整 Footer 存在，并拒绝未知 section，避免保存时静默丢失数据。

## 7. 拆分判断与验收

拆出应用或基础设施模块，应至少能指出一项真实收益：依赖方向清晰、外部效果可替换、保存/恢复工作流可独立验证，或平台实现可独立变化。若只是转发调用、复制类型或为模板补齐层次，则留在现有边界。

涉及持久化的设计或迁移应核对：

- [ ] ECS Component 只承载领域运行时状态，不承载文件和平台资源。
- [ ] 存档字段通过显式快照/DTO 传递，编码器没有隐藏 ECS 读取。
- [ ] 文件和云端 I/O 集中在适配器，错误、取消、结果未知及资源生命周期有明确语义。
- [ ] 用例协调顺序明确，失败恢复与重复执行行为可解释。
- [ ] 加载只通过领域 owner 校验并提交权威状态。
- [ ] 项目/目录边界基于实际依赖和规模，没有预建空层或空目录。
- [ ] 命名不会把普通应用/基础设施类误认为 ECS System。

本设计与 ECS 组件目录、命名空间、运行时行为和公共 API 变更相互独立；落实到源码时，仍须按根目录 `AGENTS.md` 读取相应约束并记录迁移影响。
