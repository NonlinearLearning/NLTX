# ECS 工程实践与迁移指导：Command、System、流程和证据化演进

**日期：** 2026-09-15  
**状态：** 研究结论与工程指导；已按 5 轮串行流程补充 SS14 System 拆分风格；不是 NLTX 运行时迁移完成声明  
**适用范围：** NLTX 的 ECS/domain 代码、`src/`、`src2/`、`dome/dome1/` 以及 Version4 成员迁移工作  
**证据口径：** `confirmed` 表示源码、提交或官方文档直接证明；`inferred` 表示由多个事实推导出的工程建议；`proposed` 表示尚未实施的目标设计；`unknown` 表示当前材料不能证明

本文是一次独立的现场研究稿。已有的短版草稿仍保留在
[`ecs-command-system工程实践与迁移指南.md`](ecs-command-system工程实践与迁移指南.md)；本文增加了可复核的 Git 提交、仓库内落地流程、迁移门禁和 Command/System 的细化边界。

## 1. 先给结论

ECS 迁移的最小可靠单位不是“把一个旧类拆成几个文件”，而是一个可以被验证的**行为切片**：

```text
旧行为入口
  -> 输入/网络/脚本适配
  -> Command（意图和必要的并发/幂等元数据）
  -> 校验/授权 Query
  -> 规则计算
  -> System/CommitSystem（唯一权威写入者）
  -> Snapshot/Projection/Effect
  -> 回放、差分、持久化和网络验证
```

建议把三个词固定成下面的工程含义：

| 概念 | 应该表达什么 | 不应该承担什么 |
| --- | --- | --- |
| Entity | 可寻址的身份和组合载体 | 玩法规则、网络连接、渲染对象本身 |
| Component | 某个稳定能力或生命周期状态的事实 | 跨域业务流程、I/O、隐藏的全局单例 |
| Query | 只读的资格、派生或查找规则 | 未声明的状态写入 |
| Command | 由边界提交的意图、请求或结构变化描述 | 直接持有 World、Socket、Renderer 或数据库连接 |
| System | 在明确阶段消费输入并计算/提交状态变化 | 通过反射或全局变量偷偷改变别的 owner 的状态 |
| Adapter/Projection | 将外部协议、存档、日志、UI 或网络映射到 ECS | 成为模拟状态的第二事实源 |

核心原则是：**Command 描述“要做什么”，System 决定“在什么条件下怎样改变权威状态”**。Command 不是所有方法调用的别名，System 也不是把旧 God class 改名后的新容器。

## 2. Command 的工程边界

### 2.1 哪些事情应该命名为 Command

以下情况通常适合 Command：

- 外部边界想请求模拟改变状态：玩家输入、网络包、脚本、管理员命令、加载恢复。
- 一个系统要把工作交给稍后的明确阶段，避免当前遍历时直接进行结构性变化。
- 一个跨域动作需要被排序、审计、重放、幂等处理或生成明确的拒绝结果。
- 一个写入者需要将“意图”和“实际提交”分开，例如 `Spawn`、`Damage`、`TileChange`、`Teleport`。

仅仅是纯函数的参数对象、查询结果或不可变快照，不必强行叫 Command。命名应帮助读者判断它是否会进入提交边界。

### 2.2 推荐的 Command 形状

Command 最少应包含业务载荷；涉及网络、并发、重放或跨 tick 时，再增加可审计元数据：

```csharp
public readonly record struct DamageCommand(
  int TargetEntityId,
  int SourceEntityId,
  int Amount,
  long Tick,
  long Sequence,
  long ExpectedRevision);
```

字段不应机械复制。按场景选择：

| 字段 | 何时需要 | 作用 |
| --- | --- | --- |
| Target/Entity | 目标不是由调用上下文唯一确定时 | 明确写入对象 |
| Source/Origin | 权限、仇恨、掉落、统计或审计依赖来源时 | 保留归因 |
| Tick/Phase | 固定 tick、延迟处理或回放时 | 绑定时序 |
| Sequence | 同 tick 多个写入可能竞争时 | 稳定排序 |
| Token/IdempotencyKey | 网络重试、恢复重放或重复投递时 | 防止重复应用 |
| ExpectedRevision | 需要乐观并发检查时 | 拒绝过期写入 |
| Payload | 业务参数 | 只放值语义数据，不放服务引用 |

Command 应为不可变值对象。NLTX 当前的 `UnlockPlayerProgressCommand`、`WorldTimeSkipCommand` 和 `TeleportCommitCommand` 都采用 `readonly record struct`，这是与本仓库 C# 风格相容的起点。

### 2.3 Command 的生命周期

一个可恢复的 Command 生命周期应能回答以下问题：

1. 谁产生它，来自哪个边界，属于哪个 tick？
2. 谁验证实体存在、权限、版本、数值范围和前置条件？
3. 谁按什么规则排序？同一 tick 是否依赖来源顺序？
4. 谁消费它？成功、拒绝、重复和暂缓分别是什么结果？
5. 失败是否可重试？重试会不会重复产生伤害、掉落、网络包或存档写入？
6. 处理结果如何进入回放、指标、日志、快照和测试？

推荐的处理状态至少区分：

```text
Accepted/Committed
RejectedInvalid
RejectedUnauthorized
RejectedStale
AlreadyApplied
Deferred/Retryable
FailedPermanent
```

不要用 `bool` 把这些语义压成一个成功/失败值。NLTX 的 `PlayerProgressionCommitSystem` 已经采用状态化结果，并对 token、重复应用、未知枚举和边界值进行分支处理；这是值得继续扩展的模式。

### 2.4 Command 队列和确定性

两种常见实现都可以成立：

1. **按类型分队列。** 适合领域边界清晰、每种 Command 有独立排序规则的项目。当前 `dome/dome1` 的 `SimulationCommandQueue` 就是按命令类型保存列表，并对 Shop、Liquid、Mechanism 等命令按 `Sequence` 排序。
2. **统一 envelope。** 适合跨域回放、统一审计和需要一个全局提交顺序的项目。必须保留类型、来源、tick、sequence 和 payload 编码版本。

无论哪种实现，都应明确：

- 同一阶段是否允许多个生产者并行追加；
- 排序键是否为 `(Tick, Phase, Priority, Sequence, Source)`；
- 消费时是快照枚举还是边消费边追加；
- 本 tick 产生的新 Command 是否只能进入下一阶段/下一 tick；
- 处理结束后何时清空，失败项如何保留；
- Command 是否能被序列化和重放。

不要依赖文件名、目录枚举、反射发现顺序或线程调度来定义 Command 顺序。

### 2.5 结构变化和普通字段写入要分开

修改现有组件字段值，和给实体添加/移除组件、创建/销毁实体，是不同风险等级的操作。后者会改变查询匹配和存储布局，通常需要延迟到明确的 flush/barrier。

Flecs 官方 Systems 文档明确说明：运行 pipeline 时，结构性 ECS 操作会先进入 staging command；sync point 才把命令合并到存储，使后续系统看到变化。该文档也说明 immediate system 会牺牲并行能力，并且遍历实体本身的操作仍有约束。迁移时应默认使用显式提交点，而不是为了“马上可见”到处使用 immediate。

## 3. System 的工程边界

### 3.1 一个 System 必须有可审查的契约

每个 System 至少在代码、设计文档或测试中固定以下信息：

| 契约项 | 示例问题 |
| --- | --- |
| Phase | 输入、规则、模拟、结构提交、投影还是表现？ |
| Read set | 读取哪些 Component、Definition、Snapshot、外部输入？ |
| Write set | 它是哪些状态的唯一 owner？ |
| Emit set | 产生哪些 Command、Event、Snapshot 或 Projection？ |
| Ordering | 依赖哪个阶段或哪个 barrier？ |
| Tick policy | 每 tick、固定间隔、事件触发还是一次性？ |
| Effect policy | 日志、时间、随机、网络、存档、UI 由什么 port 承接？ |
| Failure policy | 拒绝、重试、降级、回滚还是停止？ |

如果一个 System 既遍历玩家、修改 NPC、写网络包、读存档，又维护全局缓存，说明边界还没有形成；应先拆成 Query、规则计算、CommitSystem 和适配器。

### 3.2 System 的四种常见形状

1. **Pure rule/query：** 输入值和快照，输出资格、候选或决策；不写 ECS、不做 I/O。
2. **State transition/commit：** 验证 Command，修改一个明确 owner 的 Component/World state，返回结果。
3. **Structural commit：** 批量创建/销毁实体、添加/移除组件、提交 Tile/Liquid 等结构变化；必须有排序和 barrier。
4. **Projection/effect adapter：** 从已提交事实产生网络、存档、渲染、音频、日志或外部调用；不反向拥有模拟状态。

同一个领域可以有多个 System，但不要因为类型名不同就假定执行顺序。执行顺序应由显式阶段、依赖关系或调度器契约表达。

### 3.3 调度的正确默认值

Bevy 官方 ECS 入门文档展示了三个重要事实：System 是普通函数，Query 参数定义其访问范围；多个无依赖 System 默认可以并行；只有确实依赖结果时，才用 `.chain()` 约束顺序。Flecs 官方文档进一步把 pipeline、phase、读写冲突分析、sync point 和 tick source 都作为调度的一等概念。

因此 NLTX 应采用：

```text
显式 phase/barrier > 读写依赖 > 稳定排序键 > 文件/注册顺序
```

建议的高层阶段：

```text
Bootstrap
  -> Input/Ingest
  -> Normalize/Validate/Authorize
  -> Deterministic Simulation
  -> Structural Commit
  -> Derived Index / Snapshot
  -> Network / Persistence / Presentation Projection
  -> Trace / Metrics / Verification
```

阶段内可以并行，跨阶段必须通过数据契约或 barrier 连接。不要把“System 类按字母排序执行”当作调度器。

### 3.4 主要参考：Space Station 14 的 System 拆分风格

本节优先依据本机快照 C:/Users/shan/Downloads/ECS/space-station-14-master 的源码观察，
再用 RobustToolbox 官方 ECS 文档和 SS14 官方 GitHub 历史复核。快照目录没有保留 Git
元数据，因此快照只能证明当前代码形状；提交演进另以官方历史页为证据。

SS14 的可迁移规则不是“一个方法一个 System”，而是下面几条组合规则：

1. **先按领域/能力归属，再按稳定责任细分。** 典型路径是
   Content.Shared/Movement、Content.Server/Power、Content.Client/Movement；领域内部再
   使用 Components、Events、Systems 或 EntitySystems 表达边界。目录用于导航、ownership
   和编译依赖，不用于隐含运行时顺序。
2. **一个能力保留一个可识别的行为 owner。** PullingSystem 统一管理拉拽的资格判断、
   开始/停止转换、Joint、组件状态、消息和清理；它没有把每个动词机械拆成独立 System。
   这说明“是否仍然围绕同一状态不变量”比文件长度更适合作为拆分判据。
3. **Shared、Server、Client 按事实和副作用分层。** SharedFloorOcclusionSystem 维护
   碰撞集合和网络事实，Client 的 FloorOcclusionSystem 只把状态投影为 Sprite shader；
   SharedPowerNetSystem 提供跨端的电力语义，Server 的 PowerNetSystem 才拥有网络重连、
   求解器和服务器状态更新。共享类不是“所有逻辑都放进去”，而是只放两端必须一致的
   状态转换和事件契约。
4. **partial 只用于同一 owner 的稳定子责任。** AtmosphereSystem、GunSystem、
   SharedGunSystem、PathfindingSystem 等存在按 Gases、Commands、Ballistic、Magazine、
   AStar 等责任命名的 partial 文件。partial 不产生新的运行时 owner，也不能用来绕过
   原有的读写边界；如果子责任需要独立生命周期、独立调度或独立状态 owner，就应改为
   独立 System。
5. **事件订阅集中在初始化，周期更新有明确理由。** PullingSystem、CableSystem 和
   SharedBatterySystem 在 Initialize 中登记组件生命周期、交互和状态事件；只有需要
   周期推进、求解或冷却扫描的能力才实现 Update。Terasology 的官方代码也明确警告，
   不要让所有 System 每帧 Update，事件或延迟调度通常更合适。
6. **调度依赖写在 System 契约中。** PullingSystem 声明在 SharedPhysicsSystem
   之后更新，PowerNetSystem 声明在 NodeGroupSystem 之后更新。Valence 和 Veloren
   的对照实现进一步把 phase、SystemSet、before/after 和依赖名作为公开契约；NLTX
   应采用同样的显式表达，而不是依赖注册顺序。

可复核的 SS14 样本如下：

| 样本 | 代码事实 | 对 NLTX 的拆分启示 |
| --- | --- | --- |
| Content.Shared/Movement/Systems/SharedMoverController.cs、SharedMoverController.Input.cs、SharedMoverController.Relay.cs | 主类初始化跨端移动规则，Input 和 Relay 是同一类型的稳定 partial 子责任；Input 处理输入到状态，Relay 处理转发和生命周期清理 | 输入、转发和模拟可以分文件，但仍由同一能力 owner 维护不变量 |
| Content.Shared/Movement/Pulling/Systems/PullingSystem.cs | 一个能力系统集中订阅事件、检查 CanPull、执行 TryStartPull/TryStopPull，并维护 Joint、Dirty、消息和清理 | 不要按方法名过度拆分；先保证状态转换和失败路径闭合 |
| Content.Server/Power/EntitySystems/PowerNetSystem.cs | PowerNetSystem 统一处理网络重连、电池同步、solver Tick 和电力状态通知，并用 UpdatesAfter 表达 NodeGroupSystem 依赖 | 跨实体聚合能力需要单一权威 owner、阶段和重建/重连策略 |
| Content.Server/Power/EntitySystems/CableSystem.cs、CableSystem.Placer.cs | 电缆切割/锚定和放置器属于同一 CableSystem；放置器是稳定子责任，使用 partial 而非第二个 CablePlacerSystem | 只有当状态 owner 不变、生命周期一致时才使用 partial |
| Content.Shared/Movement/Systems/SharedFloorOcclusionSystem.cs、Content.Client/Movement/Systems/FloorOcclusionSystem.cs | Shared 维护碰撞集合和 Enabled 事实，Client 订阅状态并设置/移除 shader | 规则事实和表现副作用分离，Projection 不回写权威状态 |

因此，NLTX 采用 SS14 风格时的拆分决策顺序应是：

先问“谁拥有状态不变量”，再问“这段代码是共享事实、权威提交还是端专属效果”，
再问“是否有稳定且可命名的子责任”，最后才决定独立 System、partial 文件还是同文件私有方法。

### 3.5 五轮串行研究记录

本次按“本地主参考 -> 本地对照 -> 网络核验 -> 回看主参考”的顺序完成五轮，
每一轮的结果都作为下一轮的输入：

| 轮次 | 输入和检查 | 得到的可复核结论 |
| --- | --- | --- |
| 1. SS14 基线 | 统计快照中的 2073 个 System 文件：Content.Shared 926、Content.Server 717、Content.Client 426、Content.IntegrationTests 4；检查领域目录和 Components/Events/Systems/EntitySystems 层级 | System 数量本身不是拆分规则；能力域、端边界和生命周期才是第一层结构 |
| 2. SS14 样本 | 串行读取 Movement、Pulling、Power、Cable、FloorOcclusion 的主类、partial、Shared/Server/Client 配对和 Update/事件订阅 | 得到“能力 owner + 稳定 partial + Shared/端专属分层 + 显式 UpdatesAfter”的主风格 |
| 3. 次要项目 | 读取 Terasology ComponentSystemManager、UpdateSubscriberSystem，Valence 插件/SystemSet/Command 管线，Veloren System 的 Phase/Origin/依赖和指标 | 补充生命周期、事件优先、插件装配、读写 Query、阶段依赖和可观测性规则；不照搬语言 API |
| 4. 网络资料 | 读取 RobustToolbox ECS、Bevy ECS、Flecs Systems 官方页面，并查看 SS14 官方文件历史页 | 验证组件偏数据、System 持有行为、Query/读写影响调度、结构变化需要 staging/sync point、顺序必须显式 |
| 5. 反向验证 | 回到 SS14 快照统计 partial group 和 Shared/Server/Client 同名配对，并用官方历史页检查 SharedMoverController、CableSystem 的演进 | 规则得到互补：partial 不是自动拆分许可；维护性演进通常是小范围清理、能力补充和诊断改进，删除旧 owner 仍需独立门禁 |

这五轮只能证明“指导规则有源码和官方资料支持”，不能证明 NLTX 已完成任何具体运行时
迁移；NLTX 的每个切片仍需自己的读写盘点、差分 fixture 和 verifier。

## 4. 一个 ECS 工程的整体流程

### 4.1 启动与注册

启动阶段应完成：

- Component、Definition、原型和协议版本注册；
- Entity store、世界实体、静态 catalog 和随机流版本初始化；
- System graph、phase、tick source 和 effect port 组装；
- 迁移兼容层的开关、影子比较器和 trace sink 初始化。

注册失败应在启动时显式失败，而不是让运行时第一次查询时才出现空结果。

### 4.2 每个 tick 的数据流

```text
外部输入/网络包/脚本/恢复记录
  -> 入站 Adapter
  -> 规范化 Command + 来源/序号
  -> Validate/Authorize Query
  -> Pure rule calculation
  -> domain System
  -> ordered CommitSystem / structural flush
  -> immutable snapshot and derived query data
  -> network/persistence/presentation adapters
  -> deterministic trace and acceptance evidence
```

每个箭头都应有 owner。特别要防止：

- Protocol 直接修改 Simulation component；
- Projection 反向成为状态写入者；
- 旧 API facade 和新 System 双方都写同一状态；
- 日志、随机数、系统时钟或网络发送散落在规则循环中；
- 读取上一阶段尚未提交的结构变化。

### 4.3 快照和外部边界

快照不是“把所有字段复制一份”。它应说明：

- 是 authoritative state、derived state 还是 presentation state；
- 是否持久化、是否上网、是否只用于测试；
- revision、tick、随机流和版本如何传播；
- 恢复时由哪个 owner 重建，哪些字段必须重新计算。

网络、存档、日志、UI 和音频都应通过 Adapter/Projection/Port 接入。这样可以把纯规则测试和真实 I/O 测试分开，也能避免旧引擎全局对象进入 Simulation。

## 5. NLTX 当前基线和适用边界

### 5.1 已确认的仓库规则

本仓库已有的正式约束应优先于外部框架习惯：

- [`ECS文件组织设计约束.md`](../../Context/架构设计/ECS文件组织设计约束.md)：按游戏领域/能力组织；小领域保持扁平；一个核心公开类型对应同名 PascalCase 文件；目录移动不能暗改命名空间、公共 API 或运行时行为；不得靠文件顺序定义执行顺序。
- [`组件命名设计约束.md`](../../Context/架构设计/组件命名设计约束.md)：组件类型以 `Component` 结尾；运行时注册键、原型键、C# 类型和文件名保持稳定映射；能力优先于实体类别；不要为历史名称随意改公共身份。
- [`非函数式编码副作用隔离规范.md`](../../Context/约束/非函数式编码副作用隔离规范.md)：规则计算和 I/O、时钟、随机、日志、持久化、消息、UI 效果分离，并明确 effect 的 owner、顺序、失败和重试。
- [`AGENTS.md`](../../AGENTS.md)：迁移时记录源/目标路径和依赖影响；编译命令必须经过 `Build/Tools/Invoke-SerialDotnet.ps1`；输出必须位于 `Build/bin/`。

### 5.2 当前状态不能被误读成完成

当前 `Context/progress.md` 说明上下文已清理、没有 active migration plan/task，并明确这不是迁移完成、parity 或删除门禁声明。`docs/migration/ledgers/README.md` 说明 `Version4-member-migration-map.json` 是迁移账本事实源，但账本审计通过只说明结构化账本和引用一致，不说明运行时行为、网络、持久化或构建等价。

因此下列语句必须区分：

| 可以说 | 不能直接说 |
| --- | --- |
| 已建立组件边界、成员映射或设计草案 | Version4 的对应行为已经迁移 |
| 有 `Command`/`System` 源文件 | 已接入主循环且执行顺序正确 |
| focused verifier 通过 | 全量 parity 已通过 |
| ledger audit `ok=true` | 运行时和协议都已验证 |
| 有 compatibility adapter | 旧实现可以删除 |

### 5.3 当前代码中的可复用样本

| 本地样本 | 可复用经验 | 需要继续验证的边界 |
| --- | --- | --- |
| `src/Player/Progression/UnlockPlayerProgressCommand.cs` + `PlayerProgressionCommitSystem.cs` | Command 为值对象；System 负责 token 校验、枚举分派、幂等和状态化结果 | 生产者、tick/sequence、网络重复投递和持久化版本 |
| `src2/WorldSession/Runtime/WorldTimeSkipCommand.cs` + `WorldTimeSkipSystem.cs` | Command 与状态组件分开；System 提供请求、应用、冷却和 snapshot 消费入口 | 主调度阶段、跨世界生命周期、失败/重试策略 |
| `src2/SpatialMotionPhysics/TeleportCommitCommand.cs` + `TeleportCommitSystem.cs` | Commit System 的写集很窄，能独立测试 | EntityId 有效性、权限、空间约束和网络投影 |
| `dome/dome1/src/Terraria.Dome.Simulation/Tick/SimulationCommandQueue.cs` | 按类型收集、明确 `Count/Clear/Enqueue`，部分领域使用稳定 sequence 排序 | 消费顺序、跨类型全局顺序、失败项和跨 tick 生命周期 |
| `dome/dome1/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs` | 把多类 CommitSystem、snapshot 和协议边界集中到可追踪的模拟入口 | 入口仍然较大时应继续按阶段拆分并验证行为闭合 |

## 6. 迁移方法：按行为切片，不按旧类机械搬运

### 阶段 0：锁定基线

先记录：旧入口、所有读者、所有写者、生命周期、线程、序列化键、网络可见性、随机/时钟来源、外部副作用和代表性输入输出。为关键场景保存 golden trace 或 differential fixture。

基线清单必须能回答：

```text
谁写？什么时候写？写入后谁读？如果失败谁负责？重启后是否保留？网络端是否可见？
```

### 阶段 1：选择窄而完整的垂直切片

优先选择：

- 有单一权威 owner；
- 输入、规则、提交和验证都能闭合；
- 不同时跨越渲染、网络、存档和世界生成所有主流程；
- 能提供可重复的结果和低成本回滚开关。

不要以“字段数量最少”作为唯一标准。一个字段少但同时依赖全局随机、Tile、网络和存档的切片，风险可能高于一个字段较多但边界清晰的切片。

### 阶段 2：分配状态和责任

把旧成员先分类，不要立即写 Component：

| 旧成员类别 | 目标候选 | 判定问题 |
| --- | --- | --- |
| 权威持续事实 | Component/World state | 谁是唯一写者？是否需要持久化？ |
| 短期工作游标/预算 | Work state/Command buffer | 跨 tick 还是一次 System 调用？ |
| 稳定静态表 | Definition/Catalog | 是否有实体生命周期？是否会变化？ |
| 派生值 | Query/Snapshot/Projection | 是否可由权威事实重建？ |
| 外部输入 | Adapter -> Command | 是否来自协议、UI、脚本或恢复？ |
| 外部效果 | Effect port/Adapter | 失败和重试由谁拥有？ |
| 暂时无法闭合 | Compatibility/Deferred | 缺什么证据？何时重新审查？ |

### 阶段 3：先建立纯规则和验证

先写能脱离 World 的规则函数或窄 System：输入组件快照和 Command，返回结果、变更意图和 effect 描述。为边界值、重复命令、错误版本、实体死亡/销毁、tick 变化和随机种子写 focused verifier。

### 阶段 4：接入 Command 和唯一提交者

旧入口先转换为新 Command，Command 进入明确队列；由一个 owner System 提交到 Component。双写期间，旧结果仍是主结果，ECS 结果只做 shadow comparison；不要让两套实现都对外产生副作用。

推荐切换顺序：

```text
旧入口 -> Command（不改行为）
旧规则 -> ECS shadow rule（比较结果）
旧写入 -> ECS commit（仍保留 legacy fallback）
ECS projection -> 网络/存档/表现
关闭旧写入 -> 删除 facade/双写
```

每一步都需要一个可观测的差异分类：数值误差、顺序差异、默认值差异、生命周期差异、真实行为回归或证据不足。

### 阶段 5：闭合网络、存档和恢复

对于 authoritative slice，不能以本地 verifier 通过作为完成。至少验证：

- 网络输入能被规范化为同一个 Command；
- 重复 packet 不会重复提交；
- snapshot/protocol 映射不丢字段、不改变 owner；
- 存档版本和旧版本恢复有明确规则；
- 世界卸载、实体销毁、重连和恢复中的 Command 不会写入已失效目标。

### 阶段 6：切换、删除和性能验收

只有以下门禁都满足，才可删除旧实现：行为差分通过、网络/存档通过、生命周期通过、故障策略通过、指标可观测、性能基线不回归、回滚路径已验证。删除旧 facade 是最后一步，不是迁移的起点。

## 7. 推荐的 System/Command 组织方式

按领域优先，领域内部按真实边界细分：

```text
src/
  Player/
    Progression/
      UnlockPlayerProgressCommand.cs
      PlayerProgressionCommitSystem.cs
      PlayerUnlockProgressionLedgerComponent.cs
      ConsumedUpgradeEligibilityQuery.cs
  WorldSession/
    Runtime/
      WorldTimeSkipCommand.cs
      WorldTimeSkipSystem.cs
  WorldStorage/
    LiquidBufferCommand.cs
    LiquidBufferCommitSystem.cs
```

只有当规模、访问边界或独立验证单元足够稳定时，才引入 `Commands/`、`Systems/`、`Queries/` 子目录。不要为了视觉整齐建立全局 `Shared/Components/`、`Misc/` 或按技术名平铺的长期收容目录。

一个领域内的推荐依赖方向：

```text
Definition/Catalog
       |
       v
Query/Pure rule <- Component/Snapshot
       |
       v
Command -> CommitSystem -> authoritative Component
                              |
                              v
                 Snapshot/Projection/Effect Adapter
```

跨领域访问优先使用只读 Query、Command、Event、Snapshot 或 Port；不要直接读写邻域内部容器。

## 8. Git 提交历史中的工程经验

以下提交来自可复现的浅克隆历史。链接指向公开仓库的 commit 页面；提交本身比博客描述更适合用来观察边界、测试和迁移成本。

### 8.1 Bevy

| 提交 | 观察到的工程动作 | 对 NLTX 的启示 |
| --- | --- | --- |
| [`a140d84`](https://github.com/bevyengine/bevy/commit/a140d84f1078a88492627576fc0ffc66ceb389ad) | 移除 `RawCommandQueue`，同时修改 `Commands`、`CommandQueue`、`DeferredWorld`、`World` 和 `UnsafeWorldCell`；提交说明明确目标是减少 `unsafe`、简化理解并降低内存开销 | Command queue 是跨 World/deferred 生命周期的基础设施，不能只改一个 API 文件；应把所有权和 flush 语义作为整体迁移 |
| [`7fecf1c`](https://github.com/bevyengine/bevy/commit/7fecf1cd8b211ab16a3b9ad057ef56f967c3a400) | 将大实体切片重复检查从 O(N²) 改为 O(N)，并增加 Criterion benchmark 和不同规模结果 | Query/commit 优化必须带复杂度说明、代表性规模和 benchmark；不能只凭小场景体感 |
| [`61d7744`](https://github.com/bevyengine/bevy/commit/61d77447884110a37d8b4da132354bac3c089725) | 集中 `ComponentId` 构造，收紧底层字段可见性，并更新 engine/test/migration guide | 注册键和身份构造应集中到 owner，减少 magic number；接口迁移要带 migration guide 和全局引用更新 |
| [`b3fd9d7`](https://github.com/bevyengine/bevy/commit/b3fd9d783124128ac169704d0f4c0b8790daae78) | 给 `PipeSystem` 增加可配置名称，改变 engine、测试和诊断可见性 | System 应有稳定、可观测的诊断身份；复杂调度问题不能只靠类型名和日志文本猜测 |

### 8.2 Flecs

| 提交 | 观察到的工程动作 | 对 NLTX 的启示 |
| --- | --- | --- |
| [`e875d84`](https://github.com/SanderMertens/flecs/commit/e875d84512b0916b1e1f044d8e78785e56d7fba5) | 统一 C/C++ deferred set command 的构造路径 | 同一种结构变化应有一个规范化入口，避免不同边界生成语义略有不同的 Command |
| [`b9878ab`](https://github.com/SanderMertens/flecs/commit/b9878abed52470e1889794cfd967f378d3af1d3d) | JSON 恢复通过 deferred ECS removals 处理 | 恢复/加载也要遵循 ECS 的结构变化边界，不能绕过队列直接修改存储 |
| [`606abe3`](https://github.com/SanderMertens/flecs/commit/606abe3b1605260070b6f0fee9ebc8d3d95022a3) | 修复树形 query 过滤丢失表行的问题 | 查询过滤、层级关系和 observer 需要专门一致性测试；“查询能跑”不等于匹配集合正确 |
| [`2d7a5e7`](https://github.com/SanderMertens/flecs/commit/2d7a5e7a1237e70f66c88ce6544d89f39de07d48) | 新增 script event 模块，同时更新发行聚合、文档、示例和测试 | 新增 Command/Event 能力要同时更新模块注册、示例、测试和文档，避免只有源码路径没有可用流程 |

### 8.3 EnTT

| 提交 | 观察到的工程动作 | 对 NLTX 的启示 |
| --- | --- | --- |
| [`f982598`](https://github.com/skypjack/entt/commit/f982598fa5c741c883a38bd07fc0f3040ec087ca) | testbed 单独增加 `command_system.cpp/.h`，并更新 CMake 与 application | 最小可运行 ECS 切片通常从输入/Command/System 和装配入口一起落地，而不是只新增一个数据类型 |
| [`ca0cfcf`](https://github.com/skypjack/entt/commit/ca0cfcfc80b8219c0185927bf1a740b9e7acb072) | 为 testbed entity 绑定 velocity component | 先建立稳定数据事实，再让 System 消费它；组件应有清晰的能力语义 |
| [`fe6bbd5`](https://github.com/skypjack/entt/commit/fe6bbd5108b5fee749882d08848063aaa708fc19) | 增加最小 movement system | 以窄行为闭合切片，逐步扩大范围，比一次性拆旧 God class 更容易验证 |
| [`4a583eb`](https://github.com/skypjack/entt/commit/4a583ebec3cd5263063d2e0feb91e3393fb4a136) | 修复 resource comparison operator 的未定义行为 | Resource/handle/comparison/lifecycle 也属于 ECS 正确性边界，应进入测试和未定义行为检查 |

### 8.4 NLTX 自身的提交序列

当前仓库历史也提供了一个迁移过程样本：

| 提交 | 事实 | 经验 |
| --- | --- | --- |
| `1dbcf07` | 建立 Arch ECS dome 原型，同时加入 Command、Component、System、Snapshot、CommandQueue、验证项目和设计文档 | 原型应从第一天就带验证和边界，不要只搭运行时代码 |
| `456aac1` | NPC 迁移转入显式 pipeline，并增加 protocol、compatibility、server、verification 等边界 | 一旦跨网络/服务端，迁移单元必须包含协议和兼容层 |
| `b8a7c6f` | 增加 Flowstate 文档、focused verifiers、trace/比较脚本、多个领域 System/Command 调整 | 迁移过程本身需要可恢复的计划、证据和验证脚本 |
| `9d57780` | 按 Combat、Items、Npc、Physics、Player、Projectile 等领域建立组件边界 | 领域优先的目录和项目边界比全局 Components/Systems 更利于 ownership |
| `62054a6`、`eb87e17` | 设计并规划权威分区 session 和清理流程 | 大规模迁移需要可声明的分区、claim、验证和恢复机制；文档计划不等于运行时完成 |

## 9. 每个迁移切片的交付模板

每个 batch/partition 至少提交以下记录：

```markdown
# <领域>/<切片>

## Scope
- legacy source/type/member:
- target component/definition/query/command/system:
- excluded/deferred:

## Ownership
- authoritative state owner:
- command producers:
- command consumer:
- snapshot/projection owner:
- effect ports:

## Schedule
- phase:
- reads:
- writes:
- emits:
- barriers:
- tick/retry policy:

## Compatibility
- old entry points:
- import/export mapping:
- network/persistence version:
- rollback switch:

## Evidence
- source declaration and call graph:
- reader/writer inventory:
- differential fixture:
- verifier command and result:
- performance baseline:

## Exit gate
- [ ] no unowned writes
- [ ] deterministic order is explicit
- [ ] duplicate/stale commands are handled
- [ ] lifecycle and restore boundaries are tested
- [ ] effects are behind ports/adapters
- [ ] focused verification passes
- [ ] full affected-project verification passes
- [ ] deletion gate is separately approved
```

## 10. 验收和命令

### 10.1 代码审查门禁

- [ ] Command 是不可变值对象，字段只包含必要的值语义数据。
- [ ] System 的读集、写集、输出和阶段可定位。
- [ ] 每个权威 Component/World state 有唯一写者。
- [ ] 同 tick 顺序、跨 tick 行为、结构 flush 和失败/重试已定义。
- [ ] 网络、存档、随机、时钟、日志和 UI effect 都有明确 port/adapter。
- [ ] 目录按领域/能力归属；文件名、组件名、注册键和原型键一致。
- [ ] 迁移说明记录 source path、target path、dependency impact、回滚方式。
- [ ] 设计文档、账本状态和运行时验证状态没有混写。

### 10.2 行为验收门禁

- [ ] 新旧实现相同输入的结果差异已分类并达到允许阈值。
- [ ] 重复 Command、过期 revision、无效目标、实体销毁和世界卸载有测试。
- [ ] tick、sequence、随机流和事件/副作用顺序可重放。
- [ ] snapshot round-trip、旧版本恢复和网络投影已验证。
- [ ] 代表性规模有帧时间、分配、队列积压和查询耗时基线。
- [ ] 旧入口关闭后仍有可观测的回滚或故障报告路径。

### 10.3 SS14 风格的 System 拆分审查表

对一个待迁移的旧 System 或 God class，先逐项回答以下问题；任意一项答不上来，
先补证据，不要直接新增文件：

- [ ] 这个能力的状态不变量是什么？是否只有一个明确 owner？
- [ ] 代码属于 Shared 事实、Server 权威副作用、Client 表现副作用，还是测试/工具？
- [ ] 需要拆出的部分是否是稳定、可命名、可独立审查的子责任，而不是临时按方法长度切割？
- [ ] 使用 partial 后，类型、生命周期、读写集合和调度 owner 是否仍然只有一个？
- [ ] 如果拆成独立 System，是否已经定义独立的 Component/Query/Event/Command 接口和写入 owner？
- [ ] 事件订阅是否集中在初始化；周期 Update 是否有明确的 tick、预算、冷却或求解理由？
- [ ] UpdatesAfter、UpdatesBefore、phase、SystemSet 或等价调度依赖是否写在代码/装配处？
- [ ] 结构性变更是否经过显式队列、flush 或 barrier；是否避免在遍历期间隐式改变查询集合？
- [ ] Client/Projection 是否只消费已提交事实，不反向成为模拟状态写者？
- [ ] 子责任是否有 focused verifier、生命周期测试、网络/存档边界和性能证据？

推荐的审查结论格式：

~~~
System split decision: keep | partial | separate system
State owner:
Shared/server/client boundary:
Read set / write set:
Events and commands:
Schedule dependency:
Structural barrier:
Verification evidence:
Deferred questions:
~~~

### 10.4 NLTX 构建命令规则

任何可编译的 `dotnet` 命令都必须从仓库根目录通过串行包装器运行，并检查工作树中是否已有 `dotnet.exe`/`csc.exe`。示例：

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\path\AffectedProject.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
```

增量变更只构建受影响项目；先 restore/build，再用 `--no-build --no-restore` 运行 verifier。报告中记录精确命令、项目、退出码、warning/error 数量和 `Build/bin/` artifact 路径。本文只新增文档，没有执行编译，因此不能替代任何项目验证。

## 11. 研究来源和方法

### 11.1 主要参考：SS14 本地快照和官方资料

本轮研究的主要参考是本机快照：
C:/Users/shan/Downloads/ECS/space-station-14-master。重点读取了
Content.Shared/Movement、Content.Shared/Power、Content.Server/Power、
Content.Client/Movement 及其 Components、Events、Systems、EntitySystems 和 partial 文件。

SS14 的官方补充资料：

- [RobustToolbox ECS](https://docs.spacestation14.com/en/robust-toolbox/ecs.html)：
  组件以数据为主，行为放在 EntitySystem，系统通过事件订阅和公开 API 协作。
- [SharedMoverController 文件历史](https://github.com/space-wizards/space-station-14/commits/master/Content.Shared/Movement/Systems/SharedMoverController.cs)。
- [CableSystem 文件历史](https://github.com/space-wizards/space-station-14/commits/master/Content.Server/Power/EntitySystems/CableSystem.cs)。
  本次 GitHub REST API 受到 rate limit，因此用官方 history 页面核验文件级演进，并用
  raw.githubusercontent.com 核验官方源码入口；不把 REST API 失败当作历史不存在。

- Bevy 官方 ECS 入门：[ECS](https://bevy.org/learn/quick-start/getting-started/ecs/)。页面直接说明 Entity、Component、System、Query、`Commands`，并展示 System 默认可并行、需要时用 `.chain()` 固定顺序。
- Flecs 官方 Systems 文档：[Systems](https://www.flecs.dev/flecs/Systems.html)。页面直接说明 System 是 query + function，pipeline/phase/读写同步点、staging commands、immediate system、threading、interval/rate/tick source。
- Bevy API 入口：[Commands](https://docs.rs/bevy/latest/bevy/ecs/system/struct.Commands.html)。用于继续追查 Command API 与 World/deferred 语义。
- Flecs 官方仓库：[SanderMertens/flecs](https://github.com/SanderMertens/flecs)。提交历史通过浅克隆读取；GitHub 页面在本次环境中不稳定，因此提交事实以本地 Git 对象、SHA、作者日期和文件变更为准。
- Bevy 官方仓库：[bevyengine/bevy](https://github.com/bevyengine/bevy)。同上，提交事实以本地 Git 对象和 `git show` 输出为准。
- EnTT 官方仓库：[skypjack/entt](https://github.com/skypjack/entt)。同上，重点读取 testbed 的 Command/System 连续提交和 resource 修复提交。

### 11.2 次要参考：本地 ECS 项目

- Terasology：C:/Users/shan/Downloads/ECS/Terasology-develop；读取
  ComponentSystem.java、UpdateSubscriberSystem.java、ComponentSystemManager.java。
- Valence：C:/Users/shan/Downloads/ECS/valence-main；读取
  valence_command/src/manager.rs、handler.rs、valence_server/src/client.rs、
  movement.rs 和 valence_entity/src/lib.rs。
- Veloren：C:/Users/shan/Downloads/ECS/veloren-master；读取
  common/ecs/src/system.rs，观察 Phase、Origin、依赖和 CPU 指标契约。

次要项目只用于补充生命周期、插件装配、阶段/依赖和可观测性，不替代 SS14 的主要风格，
也不把其他语言的 API 直接移植到 NLTX。

### 11.3 网络补充资料

- Bevy 官方 ECS 入门：[ECS](https://bevy.org/learn/quick-start/getting-started/ecs/)。页面说明
  Entity、Component、System、Query 和 chain；无依赖 System 可并行，有依赖才约束顺序。
- Flecs 官方 Systems 文档：[Systems](https://www.flecs.dev/flecs/Systems.html)。页面说明
  System 是 query + function，并说明 pipeline/phase、读写分析、staging command、sync point、
  immediate system、threading、interval/rate/tick source。
- Bevy API 入口：[Commands](https://docs.rs/bevy/latest/bevy/ecs/system/struct.Commands.html)，
  用于追查 deferred World command 的 API 语义。
- Bevy、Flecs、EnTT 的官方仓库和提交样本仍保留在第 8 节，作为 Command、结构变更、
  Query 正确性、最小 movement slice 和 benchmark 的补充证据。

### 11.4 本仓库资料

- [`Context/progress.md`](../../Context/progress.md)：当前迁移上下文状态和“不是完成声明”的边界。
- [`docs/migration/ledgers/README.md`](../migration/ledgers/README.md)：Version4 账本事实源、audit 与 `migrated/verified/deferred` 的语义边界。
- [`docs/migration/assessments/迁移项目与既有项目能力评估报告.md`](../migration/assessments/迁移项目与既有项目能力评估报告.md)：既有项目能力和迁移限制。
- `dome/dome1/docs/flowstate/README.md`、`requirements.md`、`scope.md`：文档生命周期、manifest、证据和计划边界。
- `git log --all -- src src2 dome/dome1/src`：本仓库的 ECS 原型、领域边界、显式 pipeline 和 focused verification 演进。

### 11.5 研究限制

本机未配置 BRAVE_API_KEY，因此没有使用 Brave API；本次网络补充直接读取了可访问的官方 RobustToolbox、Bevy、Flecs、docs.rs 和 GitHub history 页面。Unity 旧版 docs.unity.cn/Packages/com.unity.entities@1.0/manual/concepts.html 页面返回 missing，因此本文没有把该页面作为证据，也没有用搜索摘要代替官方原文。外部框架机制不能直接证明 NLTX 的运行时行为，所有对 NLTX 的落地结论仍需按本仓库源码、账本和 verifier 重新证明。

## 12. 最终执行顺序

以后处理一个新的 ECS 迁移切片时，按下面顺序执行：

```text
1. 读取 progress/Flowstate/账本和适用约束
2. 固定旧入口、读写者、生命周期、协议/存档和副作用证据
3. 选择可闭合的领域行为切片
4. 定义 Component/Definition/Query/Command/System/Adapter ownership
5. 写纯规则和 focused verifier
6. 接入 Command queue 与唯一 CommitSystem
7. 通过显式 phase/barrier 接入主循环
8. 做 shadow/differential、snapshot/network/persistence 验证
9. 记录性能、失败、重试、回滚和删除门禁
10. 只有删除门禁通过后才清理 legacy facade
```

这套流程的目标不是把旧代码“看起来像 ECS”，而是让每个状态的权威性、每个 Command 的生命周期、每个 System 的读写集合和每个外部副作用都能够被源码、测试和提交历史共同解释。
