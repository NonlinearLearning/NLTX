using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

// 相位调度的可执行探针。
//
// 固定四件事：
//   1. 相位顺序是固定契约：Preparation → Simulation → Commit → Projection；
//   2. 合法依赖声明产生确定性的执行顺序，与注册书写顺序无关；
//   3. 四类非法图（重复键、缺失依赖、跨相位依赖、环）都必须被拒绝；
//   4. 同一系统实例不得注册两次，未声明键的旧式注册得到合成键。
int failures = 0;

void Check(string name, bool condition, string detail)
{
  Console.WriteLine($"{(condition ? "PASS" : "FAIL")} {name} | {detail}");
  if (!condition)
  {
    failures++;
  }
}

string Describe(EcsServerSchedulingFailureKind? kind, string message)
{
  return kind is null ? "no-exception" : $"{kind}: {message}";
}

EcsServerSchedulingFailureKind? CaptureException(ArchWorld world, EcsServerSystemRegistration[] registrations)
{
  try
  {
    _ = EcsServer.Create(world, registrations);
    return null;
  }
  catch (EcsServerSchedulingException exception)
  {
    return exception.Kind;
  }
}

EcsServerSchedulingFailureKind? CaptureExceptionWithRules(
    ArchWorld world,
    EcsServerSystemRegistration[] registrations,
    EcsCoordinationRuleSet rules)
{
  try
  {
    _ = EcsServer.Create(world, registrations, rules);
    return null;
  }
  catch (EcsServerSchedulingException exception)
  {
    return exception.Kind;
  }
}

using (ArchWorld world = ArchWorld.Create())
{
  // 合法图：后注册的系统通过 after 抢占前面的执行位置，注册书写顺序与执行顺序解耦。
  EcsServer server = EcsServer.Create(
      world,
      [
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "late",
              new NoopSystem(world)),
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "early",
              new NoopSystem(world),
              after: ["late"]),
          new EcsServerSystemRegistration(
              EcsServerPhase.Commit,
              "commit",
              new NoopSystem(world)),
          new EcsServerSystemRegistration(
              EcsServerPhase.Preparation,
              "prepare",
              new NoopSystem(world))
      ]);

  string order = string.Join(">", server.OrderedRegistrationKeys);
  Check(
      "phase-and-dependency-order",
      order == "prepare>late>early>commit",
      $"order={order}");

  EcsServer serverAgain = EcsServer.Create(
      world,
      [
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "late",
              new NoopSystem(world)),
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "early",
              new NoopSystem(world),
              after: ["late"])
      ]);
  Check(
      "schedule-is-stable-across-instances",
      string.Join(">", serverAgain.OrderedRegistrationKeys) == "late>early",
      $"order={string.Join(">", serverAgain.OrderedRegistrationKeys)}");
}

using (ArchWorld world = ArchWorld.Create())
{
  EcsServerSchedulingFailureKind? duplicate = CaptureException(
      world,
      [
          new EcsServerSystemRegistration(EcsServerPhase.Simulation, "same", new NoopSystem(world)),
          new EcsServerSystemRegistration(EcsServerPhase.Simulation, "same", new NoopSystem(world))
      ]);
  Check(
      "duplicate-key-is-rejected",
      duplicate == EcsServerSchedulingFailureKind.DuplicateKey,
      Describe(duplicate, "duplicate key"));
}

using (ArchWorld world = ArchWorld.Create())
{
  EcsServerSchedulingFailureKind? missing = CaptureException(
      world,
      [
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "only",
              new NoopSystem(world),
              after: ["absent"])
      ]);
  Check(
      "missing-dependency-is-rejected",
      missing == EcsServerSchedulingFailureKind.MissingDependency,
      Describe(missing, "missing dependency"));
}

using (ArchWorld world = ArchWorld.Create())
{
  EcsServerSchedulingFailureKind? crossPhase = CaptureException(
      world,
      [
          new EcsServerSystemRegistration(EcsServerPhase.Simulation, "sim", new NoopSystem(world)),
          new EcsServerSystemRegistration(
              EcsServerPhase.Projection,
              "projection",
              new NoopSystem(world),
              after: ["sim"])
      ]);
  Check(
      "cross-phase-dependency-is-rejected",
      crossPhase == EcsServerSchedulingFailureKind.CrossPhaseDependency,
      Describe(crossPhase, "cross phase"));
}

using (ArchWorld world = ArchWorld.Create())
{
  EcsServerSchedulingFailureKind? cycle = CaptureException(
      world,
      [
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "a",
              new NoopSystem(world),
              after: ["b"]),
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "b",
              new NoopSystem(world),
              after: ["a"])
      ]);
  Check(
      "cycle-is-rejected",
      cycle == EcsServerSchedulingFailureKind.Cycle,
      Describe(cycle, "cycle"));
}

using (ArchWorld world = ArchWorld.Create())
{
  NoopSystem shared = new(world);
  bool rejectedSharedInstance = false;
  try
  {
    _ = EcsServer.Create(
        world,
        [
            new EcsServerSystemRegistration(EcsServerPhase.Simulation, "first", shared),
            new EcsServerSystemRegistration(EcsServerPhase.Commit, "second", shared)
        ]);
  }
  catch (ArgumentException)
  {
    rejectedSharedInstance = true;
  }

  Check(
      "same-system-instance-is-rejected",
      rejectedSharedInstance,
      $"rejected={rejectedSharedInstance}");
}

using (ArchWorld world = ArchWorld.Create())
{
  // 旧式注册：无键，进入固定 Simulation 相位，并获得合成键。
  EcsServer legacy = EcsServer.Create(
      world,
      [
          new EcsServerSystemRegistration(EcsServerPhase.Simulation, new NoopSystem(world)),
          new EcsServerSystemRegistration(EcsServerPhase.Simulation, new NoopSystem(world))
      ]);
  IReadOnlyList<string> legacyKeys = legacy.OrderedRegistrationKeys;
  Check(
      "legacy-registrations-get-synthetic-keys",
      legacyKeys.Count == 2 &&
      legacyKeys.All(key => !string.IsNullOrWhiteSpace(key)) &&
      legacyKeys.Distinct(StringComparer.Ordinal).Count() == 2,
      $"keys=[{string.Join(",", legacyKeys)}]");

  bool legacyCannotBeDependencyTarget = false;
  try
  {
    _ = EcsServer.Create(
        world,
        [
            new EcsServerSystemRegistration(EcsServerPhase.Simulation, new NoopSystem(world)),
            new EcsServerSystemRegistration(
                EcsServerPhase.Simulation,
                "dependent",
                new NoopSystem(world),
                after: [legacyKeys[0]])
        ]);
  }
  catch (EcsServerSchedulingException exception)
      when (exception.Kind == EcsServerSchedulingFailureKind.MissingDependency)
  {
    legacyCannotBeDependencyTarget = true;
  }

  Check(
      "legacy-registration-cannot-be-dependency-target",
      legacyCannotBeDependencyTarget,
      $"rejected={legacyCannotBeDependencyTarget}");
}

using (ArchWorld world = ArchWorld.Create())
{
  EcsServer runnable = EcsServer.Create(
      world,
      [
          new EcsServerSystemRegistration(EcsServerPhase.Preparation, "prepare", new CountingSystem(world)),
          new EcsServerSystemRegistration(EcsServerPhase.Simulation, "sim", new CountingSystem(world)),
          new EcsServerSystemRegistration(EcsServerPhase.Commit, "commit", new CountingSystem(world)),
          new EcsServerSystemRegistration(EcsServerPhase.Projection, "projection", new CountingSystem(world))
      ]);
  runnable.Initialize();
  runnable.Update(0L, 1f / 60f);
  Check(
      "every-phase-runs-once-per-tick",
      runnable.LastTick == 0 &&
      CountingSystem.Ticks == 4 &&
      !runnable.IsFaulted,
      $"lastTick={runnable.LastTick} ticks={CountingSystem.Ticks} faulted={runnable.IsFaulted}");
}

// ---- 协作规则：调度器可以加载声明式规则，并在建服时拒绝不合规的调度 ----
using (ArchWorld world = ArchWorld.Create())
{
  EcsCoordinationRuleSet rules = EcsCoordinationRuleSet.FromRules(
      EcsCoordinationRule.Before("spawn", "move"),
      EcsCoordinationRule.RequiresPhase(EcsServerPhase.Simulation, "move"),
      EcsCoordinationRule.RequiresDependency("move"),
      EcsCoordinationRule.RequiresReason("move"));
  EcsServer governed = EcsServer.Create(
      world,
      [
          // 书写顺序故意反过来：规则与 after 边应当决定执行顺序，而不是数组下标。
          new EcsServerSystemRegistration(EcsServerPhase.Simulation, "move", new NoopSystem(world),
              after: ["spawn"], reason: "Movement reads the spawn result of the same tick."),
          new EcsServerSystemRegistration(EcsServerPhase.Simulation, "spawn", new NoopSystem(world),
              reason: "Spawn requests are recorded before simulation.")
      ],
      rules);
  string governedOrder = string.Join(">", governed.OrderedRegistrationKeys);
  Check(
      "coordination-rules-accept-compliant-schedule",
      governedOrder == "spawn>move" && governed.CoordinationRules.Count == 4,
      $"order={governedOrder} rules={governed.CoordinationRules.Count}");
}

using (ArchWorld world = ArchWorld.Create())
{
  // 规则要求 early 先于 late，而注册书写顺序是 late、early 且两者都没有声明依赖边：
  // 此时顺序只由数组下标决定，规则必须把这种"靠书写顺序成立"的调度判为违规。
  EcsCoordinationRuleSet rules = EcsCoordinationRuleSet.FromRules(
      EcsCoordinationRule.Before("early", "late"));
  EcsServerSchedulingFailureKind? violated = CaptureExceptionWithRules(
      world,
      [
          new EcsServerSystemRegistration(EcsServerPhase.Simulation, "late", new NoopSystem(world)),
          new EcsServerSystemRegistration(EcsServerPhase.Simulation, "early", new NoopSystem(world))
      ],
      rules);
  Check(
      "coordination-rule-order-violation-is-rejected",
      violated == EcsServerSchedulingFailureKind.CoordinationRuleViolation,
      Describe(violated, "order rule"));
}

using (ArchWorld world = ArchWorld.Create())
{
  EcsCoordinationRuleSet rules = EcsCoordinationRuleSet.FromRules(
      EcsCoordinationRule.RequiresDependency("drifting"));
  EcsServerSchedulingFailureKind? violated = CaptureExceptionWithRules(
      world,
      [new EcsServerSystemRegistration(EcsServerPhase.Simulation, "drifting", new NoopSystem(world))],
      rules);
  Check(
      "coordination-rule-missing-dependency-is-rejected",
      violated == EcsServerSchedulingFailureKind.CoordinationRuleViolation,
      Describe(violated, "dependency rule"));
}

using (ArchWorld world = ArchWorld.Create())
{
  EcsCoordinationRuleSet rules = EcsCoordinationRuleSet.FromRules(
      EcsCoordinationRule.RequiresReason("silent"));
  EcsServerSchedulingFailureKind? violated = CaptureExceptionWithRules(
      world,
      [
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation, "silent", new NoopSystem(world), after: [])
      ],
      rules);
  Check(
      "coordination-rule-missing-reason-is-rejected",
      violated == EcsServerSchedulingFailureKind.CoordinationRuleViolation,
      Describe(violated, "reason rule"));
}

using (ArchWorld world = ArchWorld.Create())
{
  EcsCoordinationRuleSet rules = EcsCoordinationRuleSet.FromRules(
      EcsCoordinationRule.RequiresPhase(EcsServerPhase.Commit, "misplaced"));
  EcsServerSchedulingFailureKind? violated = CaptureExceptionWithRules(
      world,
      [new EcsServerSystemRegistration(EcsServerPhase.Simulation, "misplaced", new NoopSystem(world))],
      rules);
  Check(
      "coordination-rule-phase-violation-is-rejected",
      violated == EcsServerSchedulingFailureKind.CoordinationRuleViolation,
      Describe(violated, "phase rule"));
}

using (ArchWorld world = ArchWorld.Create())
{
  EcsCoordinationRuleSet rules = EcsCoordinationRuleSet.FromRules(
      EcsCoordinationRule.RequiresReason("absent"));
  EcsServerSchedulingFailureKind? violated = CaptureExceptionWithRules(
      world,
      [new EcsServerSystemRegistration(EcsServerPhase.Simulation, "present", new NoopSystem(world))],
      rules);
  Check(
      "coordination-rule-unknown-key-is-rejected",
      violated == EcsServerSchedulingFailureKind.CoordinationRuleViolation,
      Describe(violated, "unknown key"));
}

// ---- 规则文本加载：逐行解析，错误带行号 ----
EcsCoordinationRuleSet parsed = EcsCoordinationRuleLoader.Parse(
    """
    # 协作契约：瓦片结算必须在伤害结果之后，因为它要消费已确认的回执
    before combat.projectile.damage-result combat.projectile.tile-resolve
    phase Commit combat.projectile.tile-resolve
    requires-dependency combat.projectile.tile-resolve
    requires-reason combat.projectile.tile-resolve
    """);
Check(
    "coordination-rule-document-parses",
    parsed.Count == 4 &&
    parsed.Rules[0].Kind == EcsCoordinationRuleKind.Before &&
    parsed.Rules[0].Key == "combat.projectile.damage-result" &&
    parsed.Rules[0].OtherKey == "combat.projectile.tile-resolve" &&
    parsed.Rules[1].Kind == EcsCoordinationRuleKind.RequiresPhase &&
    parsed.Rules[1].Phase == EcsServerPhase.Commit,
    $"count={parsed.Count}");

EcsServerSchedulingFailureKind? invalidRule = null;
string invalidRuleDetail = string.Empty;
try
{
  _ = EcsCoordinationRuleLoader.Parse("before only-one-key");
}
catch (EcsServerSchedulingException exception)
{
  invalidRule = exception.Kind;
  invalidRuleDetail = exception.Message;
}

Check(
    "coordination-rule-document-rejects-malformed-line",
    invalidRule == EcsServerSchedulingFailureKind.CoordinationRuleInvalid &&
    invalidRuleDetail.Contains("line 1", StringComparison.Ordinal),
    Describe(invalidRule, invalidRuleDetail));

Console.WriteLine();
Console.WriteLine(
    failures == 0
        ? "PASS: phase order, deterministic dependencies, rejected graphs and loaded " +
          "coordination rules hold."
        : $"FAIL: {failures} check(s) failed.");
return failures == 0 ? 0 : 1;
sealed class NoopSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  public NoopSystem(ArchWorld world) : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
  }
}

sealed class CountingSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  public CountingSystem(ArchWorld world) : base(world)
  {
  }

  public static int Ticks { get; set; }

  public override void Update(in EcsServerTickContext context)
  {
    Ticks++;
  }
}
