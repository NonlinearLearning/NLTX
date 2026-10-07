using Arch.Core;
using Arch.System;

internal static class Program
{
  private static readonly QueryDescription CounterQuery =
    new QueryDescription().WithAll<Counter>();

  private static void Main()
  {
    VerifySingleSystemHookOrder();
    VerifyGroupLifecycleAndNesting();
    VerifyExceptionAndDisposeBehavior();
    VerifyWorldBindingAndQueryBoundary();
    Console.WriteLine("PASS Arch.System 1.1.0 lifecycle and World-bound query probe");
  }

  private static void VerifySingleSystemHookOrder()
  {
    using World world = World.Create();
    var calls = new List<string>();
    var system = new TraceSystem("single", world, calls);
    int tick = 7;

    system.Initialize();
    system.BeforeUpdate(in tick);
    system.Update(in tick);
    system.AfterUpdate(in tick);
    system.Dispose();

    Require(calls.SequenceEqual(new[]
    {
      "single.Initialize",
      "single.BeforeUpdate",
      "single.Update",
      "single.AfterUpdate",
      "single.Dispose",
    }), "A single System must expose the five lifecycle hooks in caller order.");
    Require(ReferenceEquals(system.World, world),
      "BaseSystem must retain the World passed to its constructor.");
  }

  private static void VerifyGroupLifecycleAndNesting()
  {
    using World world = World.Create();
    var calls = new List<string>();
    var first = new TraceSystem("first", world, calls);
    var second = new TraceSystem("second", world, calls);
    var third = new TraceSystem("third", world, calls);
    var late = new TraceSystem("late", world, calls);
    var nested = new Group<int>("nested", second, third);
    var group = new Group<int>("root", first, nested);

    Require(calls.Count == 0, "Constructing or registering a System must not initialize it.");
    Require(group.Find<TraceSystem>().Count() == 3 &&
      ReferenceEquals(group.Get<TraceSystem>(), first),
      "Nested groups must expose registered systems through Find and Get.");

    group.Initialize();
    Require(calls.SequenceEqual(new[]
    {
      "first.Initialize",
      "second.Initialize",
      "third.Initialize",
    }), "Initialize must visit direct and nested systems in registration order.");

    calls.Clear();
    int tick = 11;
    group.Update(in tick);
    Require(calls.SequenceEqual(new[]
    {
      "first.Update",
      "second.Update",
      "third.Update",
    }), "Group.Update must call only Update in registration order.");

    calls.Clear();
    group.BeforeUpdate(in tick);
    Require(calls.SequenceEqual(new[]
    {
      "first.BeforeUpdate",
      "second.BeforeUpdate",
      "third.BeforeUpdate",
    }), "Group.BeforeUpdate must be called separately in registration order.");

    calls.Clear();
    group.AfterUpdate(in tick);
    Require(calls.SequenceEqual(new[]
    {
      "first.AfterUpdate",
      "second.AfterUpdate",
      "third.AfterUpdate",
    }), "Group.AfterUpdate must be called separately in registration order.");

    group.Add(late);
    Require(!calls.Contains("late.Initialize"),
      "Adding a System after Initialize must not initialize it automatically.");

    calls.Clear();
    group.Dispose();
    Require(calls.SequenceEqual(new[]
    {
      "first.Dispose",
      "second.Dispose",
      "third.Dispose",
      "late.Dispose",
    }), "Group.Dispose must visit direct and nested registrations in order.");

    calls.Clear();
    group.Dispose();
    Require(calls.SequenceEqual(new[]
    {
      "first.Dispose",
      "second.Dispose",
      "third.Dispose",
      "late.Dispose",
    }), "Group.Dispose must be treated as repeatable calls, not an idempotent lifecycle guard.");

    calls.Clear();
    var zeroTick = new Group<int>("zero-tick", new TraceSystem("zero", world, calls));
    zeroTick.Initialize();
    Require(calls.SequenceEqual(new[] { "zero.Initialize" }),
      "An initialized group with no tick must not run update hooks implicitly.");
    zeroTick.Dispose();
  }

  private static void VerifyExceptionAndDisposeBehavior()
  {
    using World world = World.Create();
    var updateCalls = new List<string>();
    var updateFailure = new InvalidOperationException("update failure");
    var failingUpdate = new TraceSystem(
      "failing-update", world, updateCalls, throwOn: "Update", updateFailure);
    var skippedUpdate = new TraceSystem("skipped-update", world, updateCalls);
    var updateGroup = new Group<int>("update-failure", failingUpdate, skippedUpdate);
    int tick = 1;
    Exception? observed = null;

    try
    {
      updateGroup.Update(in tick);
    }
    catch (Exception exception)
    {
      observed = exception;
    }

    Require(ReferenceEquals(observed, updateFailure),
      "Group.Update must propagate the original System exception.");
    Require(updateCalls.SequenceEqual(new[] { "failing-update.Update" }),
      "An Update exception must stop later registrations in that traversal.");

    var disposeCalls = new List<string>();
    var disposeFailure = new InvalidOperationException("dispose failure");
    var failingDispose = new TraceSystem(
      "failing-dispose", world, disposeCalls, throwOn: "Dispose", disposeFailure);
    var skippedDispose = new TraceSystem("skipped-dispose", world, disposeCalls);
    var disposeGroup = new Group<int>("dispose-failure", failingDispose, skippedDispose);
    observed = null;

    try
    {
      disposeGroup.Dispose();
    }
    catch (Exception exception)
    {
      observed = exception;
    }

    Require(ReferenceEquals(observed, disposeFailure),
      "Group.Dispose must propagate the original System exception.");
    Require(disposeCalls.SequenceEqual(new[] { "failing-dispose.Dispose" }),
      "A Dispose exception must stop later registrations from being disposed.");
  }

  private static void VerifyWorldBindingAndQueryBoundary()
  {
    using World firstWorld = World.Create();
    using World secondWorld = World.Create();
    Entity firstEntity = firstWorld.Create(new Counter(5));
    Entity secondEntity = secondWorld.Create(new Counter(50));
    var firstSystem = new CounterSystem(firstWorld);
    var secondSystem = new CounterSystem(secondWorld);

    Require(firstWorld.Id != secondWorld.Id &&
      firstEntity.WorldId == firstWorld.Id && secondEntity.WorldId == secondWorld.Id,
      "Entities must retain the WorldId of their owning World.");
    var mixedGroup = new Group<int>("mixed-worlds", firstSystem, secondSystem);
    Require(!ReferenceEquals(firstSystem.World, secondSystem.World),
      "Systems must retain their own World references.");

    int increment = 2;
    mixedGroup.Update(in increment);
    Require(firstWorld.Get<Counter>(firstEntity).Value == 7 &&
      secondWorld.Get<Counter>(secondEntity).Value == 52,
      "A group must not redirect a BaseSystem query away from its bound World.");

    var visited = new List<Entity>();
    firstWorld.Query(in CounterQuery, (Entity entity, ref Counter counter) =>
    {
      counter.Value++;
      visited.Add(entity);
    });
    Require(visited.Count == 1 && visited[0] == firstEntity,
      "A Query ref callback must update only matching entities in its World.");

    foreach (Entity entity in visited)
    {
      firstWorld.Add(entity, new StructuralTag());
    }

    Require(firstWorld.Has<StructuralTag>(firstEntity),
      "A structural component change must be available after Query completes.");
    ref Counter reacquired = ref firstWorld.Get<Counter>(firstEntity);
    reacquired.Value++;
    Require(reacquired.Value == 9 && secondWorld.Get<Counter>(secondEntity).Value == 52,
      "A component ref must be reacquired after the structural change and remain World-local.");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private sealed class TraceSystem : BaseSystem<World, int>
  {
    private readonly List<string> _calls;
    private readonly string _name;
    private readonly Exception? _exception;
    private readonly string? _throwOn;

    public TraceSystem(
      string name,
      World world,
      List<string> calls,
      string? throwOn = null,
      Exception? exception = null)
      : base(world)
    {
      _name = name;
      _calls = calls;
      _throwOn = throwOn;
      _exception = exception;
    }

    public override void Initialize()
    {
      Record("Initialize");
    }

    public override void BeforeUpdate(in int tick)
    {
      Record("BeforeUpdate");
    }

    public override void Update(in int tick)
    {
      Record("Update");
    }

    public override void AfterUpdate(in int tick)
    {
      Record("AfterUpdate");
    }

    public override void Dispose()
    {
      Record("Dispose");
    }

    private void Record(string hook)
    {
      _calls.Add($"{_name}.{hook}");
      if (_throwOn == hook)
      {
        throw _exception ?? new InvalidOperationException($"{_name} failed in {hook}.");
      }
    }
  }

  private sealed class CounterSystem : BaseSystem<World, int>
  {
    public CounterSystem(World world)
      : base(world)
    {
    }

    public override void Update(in int increment)
    {
      int amount = increment;
      World.Query(in CounterQuery, (Entity entity, ref Counter counter) =>
      {
        counter.Value += amount;
      });
    }
  }

  private struct Counter
  {
    public Counter(int value)
    {
      Value = value;
    }

    public int Value;
  }

  private struct StructuralTag
  {
  }
}
