# NLTX Entity Identity Context

本上下文定义实体身份相关的领域术语，区分服务器权威实体、协议索引、账户身份和兼容字段，避免把不同生命周期与作用域的标识误当成同一个主键。

## Language

**实体身份根（Entity Identity Root）**：服务器权威 ECS 实体在其有效生命周期内使用的唯一根身份，用于日志、诊断、内部稳定关联和跨系统追踪。当前规范名称为 `EntityUuid`。
_Avoid_: `whoAmI`、ReplicationId、账户 UUID、Projectile protocol identity 作为全局身份根

**Typed Projection**：从实体身份根派生或映射出的、带明确类型和作用域的外部标识视图，例如 `PlayerHandle`、`NpcHandle`、协议 identity 或 ReplicationId。它服务于特定边界，不能反向定义实体身份根。
_Avoid_: 无类型整数 ID、把所有 ID 统一成一个可互换字段

**身份适配器（Identity Adapter）**：在 ECS 权威身份与协议、兼容层、持久化或账户系统之间建立单向映射的边界模块；负责转换和校验，不拥有实体身份根。
_Avoid_: 在核心组件中直接承载协议槽位或账户归属

**权威身份创建流程（Authoritative Identity Creation）**：服务器在实体正式 spawn/commit 时生成并登记 `EntityUuid` 的流程。客户端输入、协议字段和外部存档只能请求或引用实体，不能指定、覆盖或复用该身份。
_Avoid_: 客户端生成的 UUID 直接成为服务器实体身份

**作用域标识（Scoped Identity）**：只在特定域、会话、连接、数组或协议版本内唯一的标识。作用域结束或改变后不可假定其值仍然稳定或全局唯一。
_Avoid_: 将局部索引宣称为持久化主键

**实体实例身份（Entity Instance Identity）**：绑定到一次权威 ECS 实体实例的身份；实例创建时生成，实例销毁后失效。重生、重建、重新发射或断线重连后重新创建的运行时实体均视为新的实例身份。
_Avoid_: 将运行时实例身份当作账户身份或跨会话持久化身份

**兼容索引（Compatibility Index）**：为 Terraria 旧 API 或数组式调用保留的 `whoAmI` 等位置索引，仅用于边界转换，不进入权威 ECS 状态。
_Avoid_: 兼容索引作为网络、账户或持久化 ID

**领域优先 ECS 布局（Domain-first ECS Layout）**：Entity ECS 文件先按游戏能力或规则领域
归类，再在领域内部按 `Components`、`Systems`、`Queries` 等职责分类。当前样本的领域
包括 `Entity`、`Movement`、`Physics` 和 `Liquid`；目录路径是导航和归属约束，不自动
决定 C# 命名空间。

**实体位置组件（LocationComponent）**：ECS 实体世界坐标的权威 X/Y 状态。它统一替代
`TransformComponent` 作为公共位置组件名称；查询和系统在基线修复后应只依赖
`LocationComponent`，不保留第二套公共位置类型。

**液体领域（Liquid Domain）**：承载液体接触状态及其后续查询、系统、事件和静态定义的
独立游戏能力领域。`LiquidComponent` 直接归属 `Liquid/Components/`；Player、NPC、
Projectile 的液体特有规则仍由各自领域系统处理。

**目录迁移前置门禁（Pre-migration Gate）**：Entity ECS 目录移动前必须先统一查询链的
`LocationComponent` 命名、补齐几何/空间查询的最小边界测试，并用仓库规定的串行脚本
验证基线项目可编译；目录迁移后重复同一验证，以区分基线修复与路径迁移问题。

**枚举朝向（Enumerated Direction）**：`DirectionComponent` 使用名为 `DirectionKind` 的
明确枚举表示水平朝向，不再以可任意赋值的裸 `int` 作为公共契约；规范值为 `Left`、
`None`、`Right`。表现层 `spriteDirection`、NPC 垂直方向和 Projectile 特殊方向继续保持
各自语义，不映射成同一个公共朝向字段。

**身份 Registry**：服务器运行时负责登记实体身份根、当前 ECS 实体和有效 typed projection 的目录。它只管理身份关系、冲突检测与生命周期清理，不拥有 Player、NPC 或 Projectile 的领域状态。
_Avoid_: 将 Registry 设计成承载所有实体字段的全局 Manager

**身份投影索引（Identity Projection Index）**：某一协议、连接、域或存储边界维护的 typed projection 到 `EntityUuid` 的映射。投影索引的唯一性和生命周期只在其声明作用域内成立。
_Avoid_: 用投影索引反向取代实体身份根

**协议实体引用（Protocol Entity Reference）**：网络消息中用于在明确协议版本和作用域内关联实体的 typed projection。它不是实体身份根，不能跨连接、跨会话或跨协议版本假定稳定。
_Avoid_: 在网络包中传输裸 `EntityUuid` 作为通用实体 ID

**持久化实体身份（Persistent Entity Identity）**：跨服务器重启、存档加载或世界会话仍需保持的业务对象身份。它与运行时实体实例身份分离，恢复时通过 Adapter 关联到新创建的 `EntityUuid`。
_Avoid_: 将运行时 UUID 直接当作存档主键

**Typed Handle**：绑定到特定实体域或索引作用域的强类型运行时引用，例如 `PlayerHandle` 或 `NpcHandle`。Handle 适合高频查找和协议兼容，但必须通过 Registry/Adapter 解析到 `EntityUuid`，不能与其他域的 Handle 或持久化身份互换。
_Avoid_: 用裸 `int` 表达跨域实体关系

**投影解析失败（Projection Resolution Failure）**：typed projection 因实体销毁、槽位复用、会话结束或作用域不匹配而无法解析到当前实体实例的显式结果。调用方必须处理失败，不得静默绑定到另一个实体。
_Avoid_: 失效 Handle 自动指向新占用同一槽位的实体

**预测实体身份（Prediction Entity Identity）**：仅由客户端预测或表现流程使用的临时标识。它不具有服务器权威性，不得写入 `EntityIdentityComponent`，也不得被当作账户、协议或持久化身份。
_Avoid_: 客户端预测 ID 冒充服务器 `EntityUuid`

**UUID 唯一性契约（UUID Uniqueness Contract）**：`EntityUuid` 是服务器生成的 128 位值；零值表示未分配且非法，类型和时间信息不从 UUID 推断，Registry 负责在运行时生命周期及其可观测历史中拒绝重复注册。
_Avoid_: 依赖 UUID 格式推断实体域或网络槽位

**单向身份投影迁移（One-way Identity Projection Migration）**：迁移期间由 Adapter 在旧字段、typed projection 与 `EntityUuid` 之间执行明确方向的转换；旧字段不再与新身份并列作为权威状态，也不允许隐式双向同步。
_Avoid_: 新旧身份字段同时可写且互相覆盖
