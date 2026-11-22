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

Console.WriteLine();
Console.WriteLine(
    failures == 0
        ? "PASS: phase order, deterministic dependencies and the four rejected graphs hold."
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
