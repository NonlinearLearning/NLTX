# ECS 协作规则与调度声明设计

- 日期：2026-11-24
- 状态：implemented（规则集、加载器、调度器校验与生产规则文件已落地）
- 适用项目：`src/NSSLC/Terraria.Components.csproj`（`Terraria.Entity`）、`src/NSSLC.Server`
- 上游约束：[ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md)、[ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md) §5
- 相关设计：`2026-11-23-projectile-penetration-budget-linkage-design.md`（2026-11-24 删除，无单独继任文档）；
  穿透预算现由 [Projectile README](../../src/NSSLC/Projectile/README.md) 的「穿透预算」段与
  [投射物 P0–P4 CR](../cr/CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam.md) §3 的预算条目承载

## 1. 问题陈述

NSSLC 的系统顺序原本只有两种表达方式：

1. `EcsServerSystemRegistration` 的 `after` / `before` 边（显式，但覆盖率低）；
2. 注册集合里的书写位置（隐式，但实际承担了大部分排序）。

第 2 种是隐患：`EcsServer.BuildSchedule` 用 `PriorityQueue<int,int>`，以注册索引作为优先级与 tiebreak。**一次数组成分块移动就会静默改变执行顺序**，编译期无信号，运行期无告警，只有行为变化。

实测（2026-11-23）：

| 位置 | 注册数 | 声明 `after:` | 声明 `reason:` |
| --- | --- | --- | --- |
| `CombatSystemRegistration.cs` | 14 | 8 | 8 |
| `Program.cs` | 25 | 8 | 16 |
| 合计 | 39 | 16 | 24 |

23 条注册的顺序完全由数组下标决定。

同时存在一个反向缺口：**没有任何机制能表达"这条协作契约必须成立"**。例如"投射物伤害结果必须排在伤害提交之后"这种要求，只能写在注释里，无法被校验。

## 2. 目标与非目标

### 2.1 目标

- 让协作契约成为**可加载、可校验的数据**，而不是散落的注释与约定；
- 违规在**构造期**失败，而不是在某个 tick 表现为"系统跑在错误时点"；
- 保持既有调度语义不变：阶段顺序、依赖图、命令缓冲 barrier 都不动。

### 2.2 非目标

- **不改变顺序的决定权**。规则只能拒绝一个已排好的调度，不能参与排序。
- 不替代 `before`/`after` 边。规则是"注册必须证明的契约"，不是排序输入。
- 不引入新的调度器。Arch.System 的 `Group<T>` 仍是唯一执行器。

### 2.3 为什么规则不能参与排序

这是本设计的核心边界。若规则能影响排序，就会出现两个排序权威：

- 依赖图（`BuildSchedule`，可检测环、跨阶段、重复键）；
- 规则集（停留在文本，没有环检测能力）。

两者冲突时的仲裁没有明确定义，且规则文件写错会**改变**行为而不是**拒绝**行为——这比现状更危险。因此规则被定义为**后置断言**：读排序结果，只允许抛出异常。

## 3. 规则模型

### 3.1 四类规则

| 规则 | 文本形式 | 断言内容 |
| --- | --- | --- |
| `Before` | `before <先> <后>` | 前者的最终执行位置早于后者 |
| `RequiresPhase` | `phase <Phase> <键>` | 该键的注册落在指定阶段 |
| `RequiresDependency` | `requires-dependency <键>` | 该注册声明了至少一条 `before`/`after` 边 |
| `RequiresReason` | `requires-reason <键>` | 该注册的 `reason` 非空 |

`Before` 比较的是**全局执行位置**，因此跨阶段的 `before` 规则天然成立（阶段顺序是固定契约），不需要额外限制。跨阶段的 `after` **边**仍然非法——那是注册图的规则，未被本设计放宽。

### 3.2 类型

```csharp
public enum EcsCoordinationRuleKind : byte
{
  Before = 0,
  RequiresPhase = 1,
  RequiresDependency = 2,
  RequiresReason = 3
}

public readonly record struct EcsCoordinationRule
{
  public EcsCoordinationRuleKind Kind { get; }
  public string Key { get; }
  public string? OtherKey { get; }   // 仅 Before 使用
  public EcsServerPhase? Phase { get; }  // 仅 RequiresPhase 使用
  public string Text { get; }        // 规范化单行文本，用于诊断

  public static EcsCoordinationRule Before(string beforeKey, string afterKey);
  public static EcsCoordinationRule RequiresPhase(EcsServerPhase phase, string key);
  public static EcsCoordinationRule RequiresDependency(string key);
  public static EcsCoordinationRule RequiresReason(string key);
}

public sealed class EcsCoordinationRuleSet
{
  public static EcsCoordinationRuleSet Empty { get; }   // 接受一切
  public IReadOnlyList<EcsCoordinationRule> Rules { get; }
  public int Count { get; }
  public static EcsCoordinationRuleSet Create(IEnumerable<EcsCoordinationRule> rules);
  public static EcsCoordinationRuleSet FromRules(params EcsCoordinationRule[] rules);
}
```

`Text` 存在的理由是诊断：一条违规必须能打印出"是哪条规则、要求什么"，而不是只给键名。规则对象是 `readonly record struct`，无状态、可缓存、可比较。

### 3.3 加载格式

逐行 DSL，不引入序列化依赖：

```text
# 注释
before              combat.projectile.damage-result combat.projectile.tile-resolve
phase               Commit combat.projectile.tile-resolve
requires-dependency combat.projectile.tile-resolve
requires-reason     combat.projectile.tile-resolve
```

解析规则：

- 空行与 `#` 开头的行忽略；
- 关键字与参数以空白分隔，大小写敏感；
- 阶段名大小写不敏感（`Enum.TryParse(ignoreCase: true)`），但必须是已定义值；
- 键禁止包含空格、`#` 与首尾空白——否则无法在行内安全往返。

```csharp
public static class EcsCoordinationRuleLoader
{
  public static EcsCoordinationRuleSet Parse(string text);
}
```

**解析要么完整成功、要么抛错**，不返回部分应用的规则集。错误统一为 `EcsServerSchedulingException`，`Kind = CoordinationRuleInvalid`，消息带**一基行号**。

## 4. 调度器集成

### 4.1 校验位置

```csharp
// EcsServer 构造流程
IReadOnlyList<ScheduledRegistration> schedule = BuildSchedule(registrationList);
ValidateCoordinationRules(schedule, coordinationRules);   // ← 新增
World = world;
```

在 `BuildSchedule` 之后、构建阶段组之前。此时：

- 依赖图已通过环 / 跨阶段 / 重复键校验；
- 最终执行顺序已确定；
- 尚未调用任何系统的 `Initialize`。

因此违规**不会**留下半初始化的系统。

### 4.2 API

```csharp
public static EcsServer Create(
    ArchWorld world,
    IEnumerable<EcsServerSystemRegistration> registrations,
    EcsCoordinationRuleSet coordinationRules,   // 新增重载
    bool ownsWorld = false);

public IReadOnlyList<EcsCoordinationRule> CoordinationRules { get; }
```

原有 `Create(world, registrations, ownsWorld)` 保持语义不变，等价于传入 `EcsCoordinationRuleSet.Empty`。**规则是按需启用的**，存量调用点无需改动。

### 4.3 违规收集策略

一次性收集**全部**违规再抛，而不是遇到第一条就停：

```csharp
throw new EcsServerSchedulingException(
    EcsServerSchedulingFailureKind.CoordinationRuleViolation,
    $"The ECS schedule violates {violations.Count} coordination rule(s): " +
    string.Join(" | ", violations));
```

理由：协作契约的缺口通常成片出现（一次新增若干净了全部 `requires-reason`，会有十几条一起报）。逐条报会让修一个重建一次，成本乘以条数。

### 4.4 失败种类

`EcsServerSchedulingFailureKind` 追加两个成员（附加式，不改动既有取值）：

```csharp
CoordinationRuleInvalid = 4,     // 规则文本无法解析
CoordinationRuleViolation = 5,   // 调度不满足已加载的规则
```

## 5. 生产规则文件

位置：`src/NSSLC.Server/Registration/EcsCoordinationRules.rules`，随构建复制到输出目录。

加载点：`Program.cs` 的 `LoadCoordinationRules()`，在 `EcsServer.Create` 之前调用。

```csharp
using EcsServer server = EcsServer.Create(
    ecsWorld, registrations, LoadCoordinationRules());
```

文件缺失即启动失败：

```csharp
if (!File.Exists(path))
{
  throw new InvalidDataException(
      $"ECS coordination rules were not found at '{path}'. The rules file is copied to the " +
      "output directory at build time; a missing file means the deployment is incomplete.");
}
```

**为什么缺失要失败而不是静默退化为空规则集**：规则缺失意味着部署不完整，静默退化会让"以为有校验"变成"其实没有"，这比不校验更糟。

### 5.1 当前规则内容（23 条）

覆盖两条链：

```text
投射物：contact-capture → damage-request → damage-result → termination
Combat：protection-window → attack-request → damage-request → damage-result
```

外加 9 条 `phase` 归属、4 条 `requires-dependency`、4 条 `requires-reason`。

### 5.2 为什么没有全量收紧

43 个已声明键中，只有 4 个键被要求"必须声明依赖边"。原因是给其余键补边需要业务判断：哪些是真正的前驱、哪些只是恰好排在前面。**把这件事交给规则文件逐步收紧，比一次性猜出来更安全**——规则可以逐条加，每加一条都会立刻暴露不满足的注册。

## 6. 与 Arch ECS 的边界

Arch.System 的 `Group<T>` 只做一件事：按数组顺序对组内每个系统调用 `Update`。它没有调度器、没有依赖图。

| 职责 | 归属 |
| --- | --- |
| 计算执行顺序 | `EcsServer.BuildSchedule`（拓扑排序 + 环检测） |
| 校验协作契约 | `EcsServer.ValidateCoordinationRules` |
| 执行系统 | `Arch.System.Group<T>` |

**架构上不冲突，但有四处框架语义会咬人**，使用规则的人必须知道：

### 6.1 `BeforeUpdate` / `AfterUpdate` 是整组扫描

`EcsServer.Update` 的调用形态是：

```csharp
phaseGroup.Systems.BeforeUpdate(in context);   // 全组每个系统的 BeforeUpdate
phaseGroup.Systems.Update(in context);         // 再全组 Update
phaseGroup.Systems.AfterUpdate(in context);    // 再全组 AfterUpdate
```

因此重写 `BeforeUpdate` **不会**紧邻自己的 `Update` 执行，而是跑在全组所有 `Update` 之前。把"提交前校验"放在这里会读到错误状态。

**约定**：业务逻辑只放 `Update`，两个钩子保持空实现。

### 6.2 `GetArray` 不校验组件是否在查询里

Arch 文档原文："Returns the component array for a given component in an **unsafe manner**."

现存例子：`ProjectileMovementIntentSystem` 的 `WithAll` 只声明了 `ProjectileDispositionStateComponent` 与 `GroundingComponent`，回调却取 `ref MovementIntent`。它能跑，只因为 `ProjectileComposition.AddSpatialPrerequisites` 给每个投射物都加了该组件。

**约定**：回调里 `ref`/`out` 取到的每个组件必须在 `WithAll` 里显式声明。这条 Arch 不会替你检查。

### 6.3 实体迭代顺序不保证创建序

Arch 按 archetype/chunk 迭代。需要顺序的处理必须显式排序——`ProjectileTerminationCommitSystem` 按 `WorldScope → Id → Generation` 排序就是这个原因。

**约定**：顺序敏感逻辑必须显式排序，不得假设查询顺序。

### 6.4 结构变更延迟到 playback

`ProjectileLifecycleSystem.CommitTermination` 是范式：

```csharp
disposition = new ProjectileDispositionStateComponent(ProjectileDisposition.Terminated);  // 立刻
_registry.Remove(handle, out _);                                                          // 立刻
commands?.Destroy(entity);                                                                // 延迟
```

由此产生中间态：同一相位内后续系统看到的实体"还活着，但 `Disposition = Terminated`，handle 已无法解析"。

**约定**：判活必须看 `Disposition.IsActive`，不能只看 `world.IsAlive`。

### 6.5 legacy 注册通道

公开构造 `EcsServer(world, IEnumerable<ISystem<EcsServerTickContext>>)` 会把所有系统塞进 `Simulation`，赋予合成键 `legacy.{phase}.{index}`。`BuildSchedule` 明确拒绝把 legacy 注册作为依赖目标。该路径绕过声明模型，目前仅测试使用。

## 7. 验证

### 7.1 调度探针

`Test/Terraria.Components/Terraria.Entity.Scheduling`，10 → 18 条断言，全 PASS：

```text
PASS coordination-rules-accept-compliant-schedule | order=spawn>move rules=4
PASS coordination-rule-order-violation-is-rejected | CoordinationRuleViolation: order rule
PASS coordination-rule-missing-dependency-is-rejected | CoordinationRuleViolation: dependency rule
PASS coordination-rule-missing-reason-is-rejected | CoordinationRuleViolation: reason rule
PASS coordination-rule-phase-violation-is-rejected | CoordinationRuleViolation: phase rule
PASS coordination-rule-unknown-key-is-rejected | CoordinationRuleViolation: unknown key
PASS coordination-rule-document-parses | count=4
PASS coordination-rule-document-rejects-malformed-line | CoordinationRuleInvalid: ... line 1 ...
```

关键用例 `coordination-rules-accept-compliant-schedule` 故意把 `move` 写在 `spawn` 之前，靠 `after` 边得到 `spawn>move`——**证明顺序不由数组下标决定**。

### 7.2 构建与回归

| 检查 | 结果 |
| --- | --- |
| `Terraria.Components` 构建 | 0 警告 0 错误 |
| `NSSLC.Server` 构建 | 0 警告 0 错误 |
| 11 个组件验证项目 | 全部 PASS |
| 规则文件复制到输出 | `Build/bin/NSSLC.Server/Debug/net10.0/Registration/EcsCoordinationRules.rules`（2087 字节） |

### 7.3 真实组合根

以 `src/World/科研.wld` 启动服务器 20 秒：

```text
INFO ecs.server.ready listen=127.0.0.1:7777 world=8400x2400 spawn=4195,541
```

stderr 为空 → 23 条规则在真实注册集上全部通过。

### 7.4 实现过程中的两次自我纠错

| 错误 | 现象 | 修正 |
| --- | --- | --- |
| 测试用了 `Preparation → Simulation` 的 `after` 边 | `CrossPhaseDependency` 异常 | 改用同阶段用例（框架行为正确） |
| `Before("late","early")` 恰好与注册顺序一致 | 无违规，测试假通过 | 改为 `Before("early","late")` |

第二条尤其值得记录：**一条"应该失败"的用例实际通过，说明测试没测到东西**。

## 8. 未闭合

1. **是否把 `requires-dependency` / `requires-reason` 扩展到全部 43 个键。** 那要求把 23 条隐式顺序全部显式化，需要逐个判断真正的前驱，是真实的收紧。
2. **规则文件的位置与覆盖方式。** 当前固定在 `Registration/` 并随输出复制；若需要部署期外部覆盖，可加 `NSSLC_COORDINATION_RULES` 环境变量回退。
3. **CI 提前校验。** 现在规则与注册集的一致性只在服务器启动时暴露。若要提前，可让 `Terraria.Entity.Scheduling` 读取真实规则文件——但它不引用 `NSSLC.Server`，需要额外接线。
4. **legacy 通道的处置。** 建议标 `Obsolete` 或降为 `internal`，避免以后有人图省事绕过声明模型。

## 9. 设计取舍记录

| 决策 | 采用 | 替代方案 | 理由 |
| --- | --- | --- | --- |
| 规则能否影响排序 | 否，只做后置断言 | 规则参与排序 | 避免两个排序权威冲突，且规则文本无环检测能力 |
| 规则格式 | 逐行 DSL | JSON / YAML | 不引入序列化依赖，键无嵌套结构 |
| 违规报告 | 一次性收集全部 | 遇错即停 | 契约缺口通常成片出现 |
| 文件缺失 | 启动失败 | 退化为空规则集 | 静默退化会让"以为有校验"变成"其实没有" |
| 默认行为 | `Empty` 规则集 | 强制校验 | 存量 39 条注册未全声明边，强制会直接破坏现有组合根 |
| 规则的存放位置 | Server 组合根 | 各领域自述 | 顺序是跨领域的，领域无法知道自己相对别人的位置 |
