# AI 声明接口收口执行计划

**文档 ID**：PLAN-2026-11-25-AI-DECLARATION-INTERFACE
**逻辑域**：plans
**产物类型**：plan
**策略**：sequential（P0 → P1 为硬前置；P2 独立可验收；P3、P4 按 §3 的启动条件启动，不在本计划承诺期）
**状态**：implemented（P0–P4 已交付并通过第 8 节验证）
**上游依据**：
[ECS 协作规则与调度声明设计](../architecture/2026-11-24-ecs-coordination-rules-design.md)、
[AI 核心说明](../../src/NSSLC/Share/AI/README.md)、
[Entity 定义与组件协作](../../src/NSSLC/Share/Entity/README.md)、
[NPC AI 系统重新设计](../system-decomposition/2026-10-05-npc-ai-system-redesign.md)
**适用约束**：
[ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md)、
[ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md)、
[ECS Entity 组织设计约束](../architecture/ECSEntity组织设计约束.md)、
[组件命名设计约束](../../Context/架构约束/组件命名设计约束.md)、
[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md)、
[C# 风格约束](../../Context/项目约束/Google-CSharp-Style-Guide-约束.md)、
[构建与验证约束](../../Context/项目约束/构建与验证约束.md)、
[progress 信息保存约束](../../Context/项目约束/progress信息保存约束.md)
**范围**：`src/NSSLC/Share/AI/**`、`src/NSSLC/Npc/System/**`、`src/NSSLC/Share/Entity/System/**`、`src/NSSLC.Server/Program.cs` 的 AI 接线、`Test/Terraria.Components/Terraria.Npc.Ai/**`
**排除范围**：不改 `src/NSSLC.Infrastructure/**`；不动 `EcsServer.BuildSchedule` 与 `ValidateCoordinationRules`；不新增调度器；不引入事件总线；不改协议；不迁移 Projectile AI 链路（见 §2.2 D7、§10）

---

## 1. 执行目标

把「AI 声明」从**名义存在、无人消费**推进到**被机器消费、违约在建服期失败**。

判定口径四条，缺一条不算达成：

1. **声明被消费**：新增一个 AI 输入事实或一个行为时，只改声明一处；不存在「声明写了但没人读」的字段。
2. **失败前移**：缺事实、缺 handler、缺源组件这三类错误在**建服或定义构建期**失败，不再表现为 tick 内的 `KeyNotFoundException` / `ArgumentOutOfRangeException` / 每帧 `MissingComponents`。
3. **单写边界不变**：`Location`/`Velocity` 仍只有空间协调系统写；`Life` 仍只有战斗提交写；AI 仍只出意图，不新增任何跨域写。
4. **有证据**：每期给出构建命令、项目、退出码、警告/错误数、输出路径与验证 PASS 输出；不接受口头完成。

本计划**不追求**声明接口的最终形态。P3/P4 是明确推迟项，理由与启动条件见 §3。

## 2. 当前基线与证据等级

### 2.1 已成立（有代码证据）

| 能力 | 证据 |
| --- | --- |
| 值化输入契约 | `AiSnapshot` 内部 `Dictionary<Type, object>`，`With<T>`/`Get<T>` 约束 `where T : unmanaged`；`Get` 缺项抛 `KeyNotFoundException` |
| 需求声明 | `AiRequirements.Require<T>() where T : unmanaged`、`Components`、`FindMissing(snapshot)` |
| 行为契约 | `AiBehavior<TIntent>.CalculateCore(AiSnapshot, float, AiOutput<TIntent>)`；类注释禁止捕获 World、组件引用、I/O 与权威服务 |
| 运行期检查 | `AiBinding.Calculate` 分派 `BindingChanged` / `Disabled` / `MissingComponents` / `Calculated` |
| 行为工厂 | `AiSystem<TIntent>.Register<TBehavior>()` 与 `Register<TBehavior>(Func<TBehavior>)`，`Bind` 支持带参工厂（Projectile 已用） |
| 快照捕获 | `AiEntitySystem.Calculate(entity, in component, IAiSnapshotProvider, dt)` → `Capture(entity, binding.RequiredComponents)`；注释写死「快照不在调用后保留」 |
| 意图提交 | `NpcAiIntentAdapter.ApplyIntent`：5 个 kind → `NpcBehaviorStateComponent` / `MovementIntentRequestQueueComponent` / `CombatAttackIntentQueueComponent` |
| 定义期组合检查 | `EntityBuilder.Build()` 校验 `Require<T>` 的组件确实在定义里 |
| 调度契约校验 | `EcsServer` 构造期 `BuildSchedule` → `ValidateCoordinationRules`；4 类规则（`Before`/`RequiresPhase`/`RequiresDependency`/`RequiresReason`） |

### 2.2 断链（本计划要修的）

| # | 断点 | 证据 | 影响 |
| --- | --- | --- | --- |
| D1 | `EntityDefinition.AiSnapshotRequirements` **零消费者** | `git grep AiSnapshotRequirements` 仅命中 `Share/Entity/README.md:119`；`WithAi<TBehavior>(AiRequirements?)` 重载**无任何调用点** | 「声明输入需求」这条通道是死的，读者会误以为它在生效 |
| D2 | `NpcAiIntentAdapter` 查询取 `ref AiBehaviorSelectionComponent behavior` 但**从不使用** | `NpcAiIntentAdapter.cs:143` 是该标识符在方法体内唯一出现处 | NPC 侧 `BehaviorId/ProfileId` 选择完全不生效，与 Projectile 侧形成反向不一致 |
| D3 | `NpcAiSnapshotProvider.Capture` 用 4 个 `if (requiredComponents.Contains(typeof(...)))` 硬编码事实类型，并在同一类里跨 5 个域取数 | `NpcAiSnapshotProvider.cs:40-78`；目标选择扫描 `EntityFactionComponent`/`LifeComponent`/`LocationComponent`/`InventoryComponent`/`InvisibilityComponent` | 一个类知道五个域；违反 `ECS文件组织设计约束` §3.3「查询与其主要读取的能力领域放在一起」 |
| D4 | `AiRequirements` 在各行为内部以 `private static` 声明，与 `AiComponent`、`EntityDefinition` 无连接 | `SlimeAiBehavior.cs:15-19`、`GuideAiBehavior.cs:18-19` | 「AI 需要什么」散落在实现里，无法在组合根被检查，也无法被声明驱动 |
| D5 | `.rules` 中**无任何 AI 条目**；`npc.ai.intent` 注册无 `reason`、无 `after` 边 | `Registration/EcsCoordinationRules.rules` 全文仅覆盖投射物与 Combat；`Program.cs` 的 `NpcAiIntent` 注册三项元数据全空 | AI 的顺序靠 `movement.intent.apply` 的 `after` 反向保证，属「恰好排对」，无契约可验 |
| D6 | intent kind 与提交目标的对应关系**只存在于 switch 代码** | `NpcAiIntentAdapter.cs:200-252`；`default:` 抛 `ArgumentOutOfRangeException` | 新增 kind 忘加分支 → tick 内抛异常；声明侧无法校验 |
| D7 | Projectile AI 链路整体悬空（**本计划不修**） | `ProjectileAiRegistration` 零调用；`ProjectileSimulationSystem` 零生产调用；无 projectile `IAiSnapshotProvider` | 第二条 AI 链路不可用作抽象样本，直接决定 §3 D-2 的取舍 |

## 3. 决策记录

三条决策均已定稿；P3/P4 的「推迟」本身是决策内容，不是遗漏。

### D-1 顺序锚点：采用抽象角色，但**推迟到 P3**

**结论**：需要表达先后时，用 `AiAnchor` 角色枚举 + 宿主侧绑定表，不在声明里写注册键。锚点集合封顶 5 个：`InputApply`、`MovementCommit`、`SpatialIntegration`、`CombatAdmission`、`LifecycleCommit`。

**理由（按证据强度排序）**：

1. `EcsServerRegistrationKeys` 是 `internal`，位于 `NSSLC.Server` 程序集，`AssemblyInfo.cs` 仅对 `Terraria.Server.Lvcs` 开放 `InternalsVisibleTo`。写死键会使声明无法与行为同处 `src/NSSLC/Npc/System/**`。
2. 15 个验证项目中只有 2 个引用 `NSSLC.Server`；`Terraria.Npc.Ai`、`Terraria.Projectile`、`Terraria.Entity.Scheduling` 均只引用 `Terraria.Components`。写死键意味着 AI 的排序契约只能靠真实建服验证——协作规则设计 §8.3 已把同一问题记为未闭合项。
3. 协作规则设计 §9 决策表末行：「顺序是跨领域的，领域无法知道自己相对别人的位置」。写死键与该站位直接冲突。
4. `CombatSystemRegistration` 类注释：「Assembly wiring belongs to the server host, not to the component library」。

**推迟理由**：P1/P2 只声明「读什么、出什么」，不表达相对位置，锚点在此时无消费方。提前定义只能靠猜，且 5 个里预计 3 个用不上。

**防退化约束**（P3 启动时必须连同三条测试一起提）：

| 约束 | 检查 |
| --- | --- |
| 锚点是能力角色，不是位置别名 | 评审：锚点名不得含具体系统名、实体类型名，或为键名去点号 |
| 每个锚点恰好绑定一次 | 测试：`Enum.GetValues<AiAnchor>()` 与绑定表键集合全等 |
| 每个绑定键真实存在 | 测试：`boundKey ∈ EcsServer.OrderedRegistrationKeys` |
| 同一锚点不自相矛盾 | 生成边阶段校验：不得同时作为某声明的 `After` 终点与 `Before` 起点 |

**P3 实施记录（2026-11-25）**：按 §10 第 4 条在开工时复核锚点集合，最终只保留 2 个
（`InputApply`、`MovementCommit`），不是上限的 5 个。依据：Guide 与 Slime 两条声明需要的
唯一关系是「输入落成之后、移动意图装入之前」；另外 3 个锚点没有任何声明消费者，绑定它们只会
得到一条永不被解析的键，正好违反上表第 1 条的意图。四条防退化约束的去处：第 1 条是评审项，
由 `Terraria.Npc.Ai` 的 `anchor-ordering` 探针把「不含点号、不叫某某 System」断成可执行断言；
第 2、3 条由 `AiAnchorBindings.Create` 的完备性与宿主 `EnsureResolvable` 在建服期执行，
同一探针覆盖「少锚点 / 重复键 / 未定义锚点 / 空白键 / 键不在调度里」五种拒绝；
第 4 条由 `NpcAiCatalog.EnsureConsistent` 在生成边阶段拒绝「同一条注册被要求既在某键之前又在
某键之后」。

### D-2 门面：专用 `NpcAiModule`，**不做** `AiModule<TIntent>`

**结论**：通用部分收在「声明 + 目录 + 引擎」，门面保持域专用。等第二个域出现**真实** adapter 后再抽取。

**理由**：

1. 泛化需要 ≥2 个真实消费者验证抽象。NPC 已通，Projectile 无 provider、无 adapter、无注册（D7），其需求是推测的。
2. 域差异恰好集中在门面：provider、意图消费者形态（Arch System vs 普通类）、注册项数量三者都不可用泛型消除；通用门面只能剩「建 `AiSystem` + 注册行为 + 产注册项」很薄的一层，其余全靠 `Func<>` 注入，可读性低于两个专用门面。
3. 抽取成本低：`NpcAiModule` 定稿后，把公共部分提成泛型是纯机械重构，有验证兜底。**先专用后抽取**优于**先猜后修补**。

**启动条件（写死，不靠记忆）**：Projectile 出现真实 adapter，且两侧 `Create` 有 ≥3 个参数完全同构时，评估抽取 `AiModule<TIntent>`。

**门面的三条硬性质**（依据 `Share/AI/README.md:159-161`）：

| 性质 | 要求 |
| --- | --- |
| 构造期对象 | 建完退场，不在 `Update` 中出现 |
| 内部仍是独立系统 | 只聚合，不把 N 个系统合成一个大 `Update` |
| 不进组件 | 绝不把门面实例挂到实体上 |

**门面 ≠ 运行态所有者**：`AiEntitySystem` 内部持有 `Dictionary<TEntity, AiBinding<TIntent>>`，是每实体运行态。门面（`NpcAiModule`）与运行态所有者（`NpcAiRuntime`）拆成两个类型：前者进 `NSSLC.Server/Registration/`，后者留 `Npc/System/`。

### D-3 `Commits`：本轮**不声明**，表驱动单独立项

**结论**：P0–P3 不引入 `Commits` 字段。P4 若启动，则连同「switch → 表」一起做，不做只校验的中间态。

**理由**：要校验「提交到哪个目标」就必须让 kind→目标映射变成数据；而该映射当前就是 `ApplyIntent` 的 switch。因此「只校验」只有两种结局：退化为弱校验（只查 kind 有分支，仍靠并行维护的集合），或必然演变成表驱动。中间态必然漂移。

**P4 启动条件**：需要构造期拦截「声明说提交到 A、代码提交到 B」这类错误，或新增第 6 个 intent kind。

**可推迟的原因**：表驱动只改 `NpcAiIntentAdapter` 内部，不改变声明形状与目录落位，推迟不产生返工。

**P4 实施记录（2026-11-25）**：按「不做只校验的中间态」直接做表。`NpcAiIntentCommitTable`
成为 kind → 目标组件 → 写入函数的唯一映射，`ApplyIntent` 的 `switch` 与其 `default` 异常删除，
`Commits` 由 `TargetsOf(Emits)` 投影（声明里没有第二个字段）。P2 的 `NpcAiIntentDispatch`
（只有 kind 集合、没有目标）随之删除——两套 kind 列表正是本决策要避免的漂移。
提交目标的校验口径按实际组件族修正：允许「AI 自己的 `NpcBehaviorStateComponent`」或
`*RequestQueueComponent` / `*IntentQueueComponent`；计划里写的 `*RequestComponent` /
`*IntentComponent` 是这一族的简写，实际类型名是 `MovementIntentRequestQueueComponent` 与
`CombatAttackIntentQueueComponent`，照字面匹配只会得到一条无人能过的规则。

## 4. 批次与状态

| 批次 | 状态 | 前置 | 交付目标 |
| --- | --- | --- | --- |
| P0 死接缝清理 | **完成** | 本计划批准 | D1、D2 消除；不保留两条并行声明通道 |
| P1 输入声明化 | **完成** | P0 | D3、D4 消除；`Reads` 由声明驱动，provider 按域拆分 |
| P2 输出声明化 | **完成** | P1 | D6 部分消除；`Emits` 建服期失败（校验归属见第 8 节） |
| P3 顺序声明化 | **完成** | P2 + 真实排序需求出现 | D5 消除；锚点 + 绑定表 + 规则生成 |
| P4 提交表驱动 | **完成** | 满足 D-3 启动条件 | D6 完全消除；`Commits` 由表投影 |

P2 与 P1 可分离验收：P1 只动 provider 与声明目录，P2 才动 `NpcAiIntentAdapter` 的构造期校验。

## 5. P0 改动清单

| 动作 | 文件 | 说明 |
| --- | --- | --- |
| 删除 `AiSnapshotRequirements` 属性与其构造参数 | `src/NSSLC/Share/Entity/System/Entity.cs` | 依据：`git grep` 无调用点，仅 README 提及 |
| 删除 `WithAi<TBehavior>(AiRequirements?)` 重载 | `src/NSSLC/Share/Entity/System/EntityBuilder.cs` | 三个 `WithAi` 调用点均用无参重载；`Build()` 的 `Require<T>` 校验保留不动 |
| 更新示例，删除「在定义构建时加入 AI 需求」一段 | `src/NSSLC/Share/Entity/README.md` | 该段描述的能力不存在，留着会误导 |
| 从查询回调移除未使用的 `ref AiBehaviorSelectionComponent behavior` 参数 | `src/NSSLC/Npc/System/NpcAiIntentAdapter.cs` | 保留 `WithAll<AiBehaviorSelectionComponent>`（组合契约），只删回调参数 |

**P0 验收**

1. `dotnet build src/NSSLC/Terraria.Components.csproj` → 0 error / 0 warning，输出 `Build/bin/Terraria.Components/Debug/net10.0/`。
2. `dotnet build src/NSSLC.Server/NSSLC.Server.csproj` → 同上。
3. `dotnet run --project Test/Terraria.Components/Terraria.Entity.Definition --no-build --no-restore` 与 `Terraria.Npc.Ai` 全 PASS。
4. `git grep AiSnapshotRequirements -- src Test` 结果为空。

**P0 风险**：`Terraria.Entity.Definition` 若用到被删重载会在编译期暴露——这正是期望的失败方式，不是风险。

## 6. P1 改动清单

### 6.1 新增（通用，「声明 + 目录」）

`src/NSSLC/Share/AI/System/AiBehaviorDeclaration.cs`

```csharp
/// <summary>Immutable per-behavior declaration: what it reads, how it is constructed.</summary>
public sealed class AiBehaviorDeclaration<TIntent> where TIntent : unmanaged
{
  public Type BehaviorType { get; }
  public Func<AiBehavior<TIntent>> Factory { get; }
  public AiRequirements Reads { get; }
  public AiBehaviorDeclaration(Type behaviorType, Func<AiBehavior<TIntent>> factory, AiRequirements reads);
}
```

`src/NSSLC/Share/AI/System/AiBehaviorCatalog.cs`

```csharp
/// <summary>Immutable catalog keyed by behavior type; sealed after construction.</summary>
public sealed class AiBehaviorCatalog<TIntent> where TIntent : unmanaged
{
  public AiBehaviorDeclaration<TIntent> Get(Type behaviorType);   // 未注册即抛
  public IReadOnlyList<AiBehaviorDeclaration<TIntent>> Declarations { get; }
  public bool IsSealed { get; }
  public void Seal();
}
```

**键的选择**：P1 以 `Type` 为键，**不引入** `(AiBehaviorId, AiBehaviorProfileId)` 双键。依据：`AiComponent` 只携带 `BehaviorType`，`NpcAiRuntime.Attach` 与 `EntitySpawnSystem` 三处调用点都只传 `in AiComponent`；双键会迫使修改 `EntityDefinition` 与三个调用点，而收益要到 Projectile 带参行为真正接入时才出现。

### 6.2 新增（NPC 域）

`src/NSSLC/Npc/System/NpcAiDeclaration.cs`：Guide 与 Slime 两份声明，`Reads` 从行为内的 `private static AiRequirements` 搬到这里（行为侧读取改为由构造函数注入或由目录解析，二者择一，实施时记录）。

`src/NSSLC/Npc/System/NpcAiSnapshotContributors.cs`：按域拆成 Movement、Target、Life、World 四个 contributor，实现同一接口：

```csharp
public interface INpcAiSnapshotContributor
{
  /// <summary>Facts this contributor can project.</summary>
  IReadOnlyList<Type> FactTypes { get; }

  AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot);
}
```

### 6.3 修改

| 文件 | 改动 |
| --- | --- |
| `Npc/System/NpcAiSnapshotProvider.cs` | `Capture` 改为：按 `requiredComponents` 过滤 contributor 集合 → 逐个 `Contribute`；删除 4 个 `if`；`FindNearestFriendlyTarget` 迁入 Target contributor |
| `Npc/System/NpcAiRegistration.cs` | `NpcAiRuntime` 持有 `AiBehaviorCatalog<NpcAiIntent>`；`Register` 由目录驱动，替代两行手写 `_system.Register<T>()` |
| `Npc/System/SlimeAiBehavior.cs`、`GuideAiBehavior.cs` | `Requirements` 不再由行为自持，改由声明提供（保持 `CalculateCore` 签名不变） |

**不改**：`Program.cs` 注册块、`EcsCoordinationRules.rules`、`AiComponent`、`ApplyIntent`、`EntitySpawnSystem`。

### 6.4 P1 验收

| 检查 | 判定 |
| --- | --- |
| 构建 | `dotnet build src/NSSLC/Terraria.Components.csproj` 与 `src/NSSLC.Server/NSSLC.Server.csproj`：0 error / 0 warning，输出在 `Build/bin/` |
| 组件验证 | `Terraria.Npc.Ai`、`Terraria.Entity.Definition`、`Terraria.Entity.Scheduling`、`Terraria.Projectile`、`Terraria.Attributes`、`Terraria.EntitySpawn` 全 PASS（`--no-build --no-restore`） |
| 单一消费点 | `NpcAiSnapshotProvider.Capture` 中不存在 `typeof(NpcAi…Fact)` 字面量 |
| 行为等价 | Guide/Slime 在 `Terraria.Npc.Ai` 的意图序列与改动前逐条一致（该验证项目当前直接构造 adapter，可断言） |
| 真实启动 | 以 `src/World/科研.wld` 启动服务器 20 秒：`INFO ecs.server.ready`，stderr 为空（沿用协作规则设计 §7.3 口径） |

## 7. P2–P4 概要（P2 已交付，P3/P4 推迟）

| 批次 | 内容 | 启动条件 | 验收要点 |
| --- | --- | --- | --- |
| P2 | 声明承载 `Emits`（kind 集合）；`NpcAiIntentAdapter` 构造期校验每个 kind 有分支 | P1 完成（**已交付**） | 新探针：声明未覆盖的 kind → 建服失败；现有 `Terraria.Npc.Ai` 仍 PASS |
| P3 | `AiAnchor` + `AiAnchorBindings`；声明承载顺序锚点；生成 `after` 边与 `EcsCoordinationRule` | 已交付（2026-11-25） | 删除 `MovementIntentApply` 注册上 `after` 中的 `NpcAiIntent` 项后调度探针仍 PASS（`Program.cs` 的 `MovementIntentApply` 注册，以键名定位）；`.rules` 新增 AI 条目；§3 D-1 四条测试 |
| P4 | `ApplyIntent` 表驱动；`Commits` 从表投影 | 已交付（2026-11-25） | `Terraria.Npc.Ai` 行为等价；启动期校验提交目标是「AI 自己的行为状态」或 `*RequestQueueComponent`/`*IntentQueueComponent` |

## 8. 验证与证据

按 [构建与验证约束](../../Context/项目约束/构建与验证约束.md) 执行：只构建受影响项目；验证项目用 `--no-build --no-restore`；输出落在 `Build/bin/<ProjectName>/<Configuration>/net10.0/`（依据 `Directory.Build.props` 的 `BaseOutputPath`）。

每个批次必须记录：

| 字段 | 内容 |
| --- | --- |
| 命令 | 精确命令行与工作目录 |
| 结果 | 退出码、warning/error 计数 |
| 输出 | `Build/bin/` 下的实际路径 |
| 行为 | 验证项目 PASS 行，或真实启动的关键日志与 stderr 状态 |
| 未做 | 本批次未覆盖的检查项，单列为 unknown |

未实际执行的项一律不填「通过」。

### 8.1 本轮实测（2026-11-24，工作目录 `D:\TRbackup\NLTX`）

| 命令 | 退出码 | 警告 | 错误 | 产物 / 结果 |
| --- | --- | --- | --- | --- |
| `dotnet build src/NSSLC/Terraria.Components.csproj -v minimal` | 0 | 0 | 0 | `Build/bin/Terraria.Components/Debug/net10.0/Terraria.Components.dll` |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj -v minimal` | 0 | 19（全部位于 `NSSLC.Network`、`NSSLC.Infrastructure.WorldGeneration` 的既有文件，本轮改动文件 0 条） | 0 | `Build/bin/NSSLC.Server/Debug/net10.0/NSSLC.Server.dll` |
| `dotnet run --project Test/Terraria.Components/Terraria.Npc.Ai --no-build --no-restore` | 0 | — | — | 2 条 PASS / 0 FAIL；新增行 `PASS declaration-drives-inputs \| declared=2 facts=4 emits=3 unprojectable-rejected=True undispatchable-rejected=True`；末行 `PASS: Guide idle/wander/stop, Slime targeting/jump, AI intent, spatial movement and NPC wire projection.` |
| `dotnet run --project Test/Terraria.Components/Terraria.Entity.Definition --no-build --no-restore` | 0 | — | — | 7 条 PASS / 0 FAIL |
| `dotnet run --project Test/Terraria.Components/Terraria.Entity.Scheduling --no-build --no-restore` | 0 | — | — | 19 条 PASS / 0 FAIL |
| `dotnet run --project Test/Terraria.Components/Terraria.Attributes --no-build --no-restore` | 0 | — | — | 38 条 PASS / 0 FAIL |
| `dotnet run --project Test/Terraria.Components/Terraria.EntitySpawn --no-build --no-restore` | 0 | — | — | 22 条 PASS / 0 FAIL |
| `dotnet run --project Test/Terraria.Components/Terraria.Projectile --no-build --no-restore` | 0 | — | — | 87 条 PASS / 0 FAIL，末行 `PASS: projectile spawn produces a spatial-simulatable entity.` |
| `git grep "AiSnapshotRequirements" -- src Test` | 1（无匹配） | — | — | 空输出，D1/D2 的死接缝确已消失 |
| 真实启动：`NSSLC_WORLD_FILE=src/World/科研.wld dotnet run --project src/NSSLC.Server --no-build --no-restore`（22 s 后终止） | 0 | — | — | `INFO ecs.npc.spawned guide=world-spawn:4195,541 slime=world-spawn+3tiles:4198,541`；`INFO ecs.server.ready listen=127.0.0.1:7777 world=8400x2400 spawn=4195,541`；无异常行 |

六个验证项目合计 **175 条 PASS / 0 FAIL**。

**校验归属（P2 实施中的修正）**：`Emits` 的校验放在 `NpcAiIntentAdapter` 的构造里，而不是 `NpcAiRuntime`。
理由是适配器自己拥有那份 `switch` 分支表，只有它能判断「这个 kind 我能不能发」；
把校验放在运行时会让运行时依赖适配器的实现细节，且适配器可以不经运行时被单独构造（验证项目正是如此），
校验就会漏。现在 `NpcAiIntentAdapter` 新增 `NpcAiCatalog` 构造参数，做两件事：
`EnsureComplete()`（枚举成员与分支表对齐）与逐声明的 `EnsureDispatches(Emits, BehaviorType)`。

**本批次未做（unknown）**：
- P3 顺序锚点的探针（删除 `MovementIntentApply` 注册上 `after` 中的 `NpcAiIntent` 项后调度仍 PASS）——批次未开工。
- `Commits` 的运行时校验——P4 未开工。
- 客户端真实输入驱动 NPC AI 的全链路——本计划不含（见第 10 节）。

### 8.2 P3/P4 本轮实测（2026-11-25，工作目录 `D:\TRbackup\NLTX`）

| 命令 | 退出码 | 警告 | 错误 | 产物 / 结果 |
| --- | --- | --- | --- | --- |
| `dotnet build src/NSSLC/Terraria.Components.csproj -v quiet --nologo` | 0 | 0 | 0 | `Build/bin/Terraria.Components/Debug/net10.0/Terraria.Components.dll` |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj -v minimal --nologo` | 0 | 0 | 0 | `Build/bin/NSSLC.Server/Debug/net10.0/NSSLC.Server.dll` |
| `dotnet run --project Test/Terraria.Components/Terraria.Npc.Ai --no-build --no-restore` | 0 | — | — | 4 条 PASS / 0 FAIL；新增 `PASS anchor-ordering \| anchors=2 edges=2 rules=2 unbound-rejected=True duplicate-rejected=True contradiction-rejected=True`、`PASS declaration-edges \| after=1 before=1 generated-order-holds=True edge-dropped-changes-order=True`、`PASS commit-table \| kinds=5 targets=2 allowed=3 unhandled-rejected=True` |
| `Terraria.Entity.Definition` / `Entity.Scheduling` / `Attributes` / `EntitySpawn` / `Projectile`（`--no-build --no-restore`） | 全 0 | — | — | 7 / 19 / 38 / 22 / 87 条 PASS，0 FAIL |
| 真实启动：`NSSLC_WORLD_FILE=src/World/科研.wld dotnet run --project src/NSSLC.Server --no-build --no-restore`（20 s 后终止） | 0 | — | — | `INFO ecs.npc.spawned guide=world-spawn:4195,541 slime=world-spawn+3tiles:4198,541`；`INFO ecs.server.ready listen=127.0.0.1:7777 world=8400x2400 spawn=4195,541`；stderr 0 行 |

六个验证项目合计 **177 条 PASS / 0 FAIL**。P3 的「调度探针」不再靠手写 `after` 项：
`MovementIntentApply` 的 after 列表由 `NpcAiOrdering.AfterKeysOf` 生成，
`declaration-edges` 探针同时证明「边在 → 顺序对」「边删 → 顺序错」两个方向。

**本批次未做（unknown）**：
- 客户端真实输入驱动 NPC AI 全链路——本计划不含（第 10 节）。
- P3 的锚点集合只复核到「当前 2 个声明」，Projectile AI 链路接入后须重新复核（D7 未开工）。

## 9. 回滚

| 批次 | 回滚方式 | 数据影响 |
| --- | --- | --- |
| P0 | 恢复 `AiSnapshotRequirements` 与 `WithAi` 重载、恢复回调参数 | 无（无持久化字段） |
| P1 | 删除目录与声明，恢复 `NpcAiSnapshotProvider` 的四个 `if` | 无（声明不落盘、不进存档） |
| P2/P3/P4 | 各自独立提交，逐批回退 | 无 |

五个批次均不改变组件布局、协议或存档格式，因此不存在数据迁移。

## 10. 未闭合

1. **D7（Projectile AI 链路悬空）不在本计划**。它影响 §3 D-2 的抽取时机判断，但修复需要 provider + adapter + 注册 + 验证四件独立工作，应单独立项。
2. ~~**`AiBehaviorSelectionComponent` 在 NPC 路径的最终归属未定**~~ → **已定：保留为准入闸门**。
   P3 不需要运行期按 profile 查声明：目录键是行为类型，锚点是能力角色，两者都不读
   `BehaviorId/ProfileId`。该组件因此只承担「实体是否加入 NPC AI」的组合契约（查询过滤器），
   不再作为声明键的候选；代码里没有任何字段读它。
3. ~~**行为自持 `AiRequirements` 的收敛方式未定**~~ → **已定：构造函数注入**。`GuideAiBehavior(AiRequirements)` /
   `SlimeAiBehavior(AiRequirements)` 取代原来的 `private static AiRequirements Requirements`，读取契约只来自声明。
   选它而非「目录解析」的理由：目录解析在行为内部仍要查一次表，漂移只是被检查出来（运行期）；
   构造注入让行为拿不到第二份需求，漂移在结构上不可能（编译期即无入口）。
4. ~~**锚点集合数量待 P3 开工时复核**~~ → **已复核：只保留 2 个**（InputApply、MovementCommit）。
   5 个是上限估计；实测只有这两条关系有声明消费者，其余三个没有任何行为引用，绑定它们
   只会得到永不被解析的键。Projectile AI 链路（第 1 条）接入后须重新复核，届时新增锚点是
   枚举上的一次显式编辑。

## 11. 风险与规避

| 风险 | 规避 |
| --- | --- |
| P1 把 `Requirements` 从行为移出后，行为内部仍可能 `snapshot.Get<T>()` 未声明的事实 | 保留运行期 `FindMissing` 不变（双保险）；P2 再引入构造期校验 |
| 按域拆 contributor 后，一个事实被两个 contributor 声明 | `Capture` 构建期校验：事实类型在所有 contributor 中唯一出现 |
| `NpcAiModule`（P3）长成第二个 `NpcAiSnapshotProvider`（再次跨域） | 门面只碰目录、锚点绑定与注册项，禁止在门面内 `World.Query` |
| 文档与实现漂移 | 每批次完成后回写 §4 状态与 §8 证据；未执行不填通过 |

## 12. 设计取舍记录

| 决策 | 采用 | 替代方案 | 理由 |
| --- | --- | --- | --- |
| 顺序锚点 | 推迟到 P3，届时用抽象角色 | 现在写死键 | 键是 `internal`；库侧测试与声明都无法引用；与「领域不知自己位置」冲突 |
| 门面泛化 | 专用 `NpcAiModule` | 现在做 `AiModule<TIntent>` | 只有 1 个真实消费者；域差异集中在门面，泛型只能剩空壳 |
| `Commits` | P4 由提交表投影（不单独声明字段） | 先做弱校验 | 弱校验仍需并行维护 kind 集合，必然漂移；P4 已按此交付 |
| 目录键 | `Type` | `(AiBehaviorId, AiBehaviorProfileId)` 双键 | `AiComponent` 只带 `BehaviorType`；双键要改 `EntityDefinition` 与三个调用点，收益延后出现 |
| 声明载体 | 定义/目录层不可变对象 | 放进 `AiComponent` | 组件是每实体状态；携带工厂与需求会破坏复制语义与可查询性 |
| 校验位置 | 建服期 / 定义构建期 | tick 内异常 | 协作规则设计 §4.1：违规不留半初始化系统 |
| `Emits` 校验归属 | `NpcAiIntentAdapter` 构造 | `NpcAiRuntime` 构造 | 分支表由适配器拥有，只有它能判断「这个 kind 我发得了吗」；且适配器可被单独构造（验证项目如此），放在运行时会让这条路绕过校验 |
| 需求来源 | 声明经构造函数注入行为 | 行为内部查目录 | 注入让行为拿不到第二份需求，漂移结构上不可能；目录解析仍要运行期比对 |
