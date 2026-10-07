using System;
using System.Collections.Generic;
using Arch.Buffer;
using Arch.Core;

namespace Terraria.Arch.Verification;

internal static class Program
{
  private static readonly (string Name, Action Run)[] Cases =
  [
    ("world.create-dispose", VerifyWorldCreateAndDispose),
    ("world.destroy", VerifyWorldDestroy),
    ("world.id-reuse", VerifyWorldIdReuse),
    ("world.clear", VerifyWorldClear),
    ("world.clear-id-reuse", VerifyWorldClearIdReuse),
    ("entity.id-version-reuse", VerifyEntityIdAndVersionReuse),
    ("entity.default-not-alive", VerifyDefaultEntityIsNotAlive),
    ("entity.destroyed-not-alive", VerifyDestroyedEntityIsNotAlive),
    ("entity.worldid-field", VerifyEntityWorldId),
    ("entity.equality-worldid", VerifyEntityEqualityIncludesWorldId),
    ("world.isalive-worldid-boundary", VerifyIsAliveDoesNotCheckWorldId),
    ("boundary.worldid-rejects-cross-world", VerifyWorldIdGuardRejectsCrossWorldEntity),
    ("boundary.token-rejects-retired-session", VerifyTokenGuardRejectsRetiredSession),
    ("component.has-present", VerifyHasPresentComponent),
    ("component.has-absent", VerifyHasAbsentComponent),
    ("component.get-ref", VerifyGetReturnsWritableRef),
    ("component.tryget-struct-copy", VerifyTryGetReturnsStructCopy),
    ("component.trygetref-present", VerifyTryGetRefWhenPresent),
    ("component.trygetref-absent", VerifyTryGetRefWhenAbsent),
    ("component.set-existing", VerifySetExistingComponent),
    ("component.add", VerifyAddComponent),
    ("component.remove", VerifyRemoveComponent),
    ("component.class-instance-isolation", VerifyClassInstancesAreIndependent),
    ("component.class-alias-is-preserved", VerifyClassAliasesAreNotDeepCopied),
    ("component.ref-reacquire-after-structure-change", VerifyRefIsReacquiredAfterStructureChange),
    ("component.struct-class-access-critical", VerifyCriticalComponentAccess),
    ("query.all-allows-extra", VerifyQueryAllAllowsExtraComponents),
    ("query.any", VerifyQueryAny),
    ("query.none", VerifyQueryNone),
    ("query.exclusive", VerifyQueryExclusive),
    ("query.composition-critical", VerifyCriticalQueryComposition),
    ("command-buffer.create-negative-id", VerifyBufferedCreateIsStaged),
    ("command-buffer.create-component-types", VerifyBufferedCreateComponentTypes),
    ("command-buffer.default-cleanup", VerifyPlaybackClearsByDefault),
    ("command-buffer.keep-commands", VerifyPlaybackCanRetainCommands),
    ("command-buffer.add-value-shares-set-group", VerifyAddRecordsValueInSetGroup),
    ("command-buffer.repeated-set", VerifyRepeatedSetUsesLastValue),
    ("command-buffer.repeated-add", VerifyRepeatedAddUsesLastValue),
    ("command-buffer.remove", VerifyBufferedRemove),
    ("command-buffer.destroy", VerifyBufferedDestroy),
    ("command-buffer.destroyed-target-observation", VerifyDestroyedTargetPlayback),
    ("command-buffer.exception-partial-effects", VerifyPlaybackKeepsEarlierEffectsOnFailure),
    ("command-buffer.set-missing-component", VerifyBufferedSetDoesNotAdd),
    ("command-buffer.create-and-groups-critical", VerifyCriticalCommandBufferBehavior)
  ];

  private static int Main(string[] args)
  {
    var selected = new List<string>();
    for (var index = 0; index < args.Length; index++)
    {
      if (args[index] != "--case" || index + 1 >= args.Length)
      {
        Console.Error.WriteLine("Usage: --case <case-name> [--case <case-name> ...]");
        return 2;
      }

      selected.Add(args[++index]);
    }

    if (selected.Count == 0)
    {
      Console.Error.WriteLine("At least one --case is required; the full matrix is never the default.");
      return 2;
    }

    foreach (var name in selected)
    {
      var testCase = Array.Find(Cases, candidate => candidate.Name == name);
      if (testCase.Run is null)
      {
        Console.Error.WriteLine($"Unknown case: {name}");
        return 2;
      }

      try
      {
        testCase.Run();
        Console.WriteLine($"PASS {name}");
      }
      catch (Exception exception)
      {
        Console.Error.WriteLine($"FAIL {name}: {exception}");
        return 1;
      }
    }

    Console.WriteLine($"Selected {selected.Count} of {Cases.Length} probe cases.");
    return 0;
  }

  private static void VerifyWorldCreateAndDispose()
  {
    var world = World.Create();
    var entity = world.Create();
    Assert(entity.WorldId == world.Id, "A created entity records its owning WorldId.");
    world.Dispose();
    world.Dispose();
  }

  private static void VerifyWorldDestroy()
  {
    var world = World.Create();
    World.Destroy(world);
  }

  private static void VerifyWorldIdReuse()
  {
    var first = World.Create();
    var firstId = first.Id;
    World.Destroy(first);

    var second = World.Create();
    try
    {
      Assert(second.Id == firstId, "Destroy releases the World id for immediate reuse.");
    }
    finally
    {
      World.Destroy(second);
    }
  }

  private static void VerifyWorldClear()
  {
    using var world = World.Create();
    var entity = world.Create();
    world.Clear();
    Assert(!world.IsAlive(entity), "Clear removes the existing entity from the World.");
  }

  private static void VerifyWorldClearIdReuse()
  {
    using var world = World.Create();
    var previous = world.Create();
    var worldId = world.Id;
    var sessionToken = Guid.NewGuid();

    world.Clear();
    var replacement = world.Create();

    Assert(world.Id == worldId, "Clear keeps the Arch World instance and id.");
    Assert(sessionToken != Guid.Empty, "The owner token is independent test state.");
    Console.WriteLine(
      $"OBSERVE clear-reuse: old={previous.Id}/{previous.Version}, " +
      $"new={replacement.Id}/{replacement.Version}, token={sessionToken}");
  }

  private static void VerifyEntityIdAndVersionReuse()
  {
    using var world = World.Create();
    var previous = world.Create();
    world.Destroy(previous);
    var replacement = world.Create();

    Assert(previous.Id == replacement.Id, "Destroy makes the entity id available for reuse.");
    Assert(previous.Version != replacement.Version, "Reuse advances the entity version.");
    Assert(!world.IsAlive(previous), "The previous entity version is no longer alive.");
  }

  private static void VerifyDefaultEntityIsNotAlive()
  {
    using var world = World.Create();
    Assert(!world.IsAlive(default), "The default Entity is not alive.");
  }

  private static void VerifyDestroyedEntityIsNotAlive()
  {
    using var world = World.Create();
    var entity = world.Create();
    world.Destroy(entity);
    Assert(!world.IsAlive(entity), "A destroyed Entity version is not alive.");
  }

  private static void VerifyEntityWorldId()
  {
    using var world = World.Create();
    var entity = world.Create();
    Assert(entity.WorldId == world.Id, "The Entity carries its owning WorldId.");
  }

  private static void VerifyEntityEqualityIncludesWorldId()
  {
    using var firstWorld = World.Create();
    using var secondWorld = World.Create();
    var first = firstWorld.Create();
    var second = secondWorld.Create();

    Assert(first.Id == second.Id, "Fresh worlds allocate the same first entity id.");
    Assert(first.Version == second.Version, "Fresh worlds allocate the same first version.");
    Assert(first.WorldId != second.WorldId, "The worlds have distinct ids.");
    Assert(first != second, "Entity equality includes WorldId.");
  }

  private static void VerifyIsAliveDoesNotCheckWorldId()
  {
    using var firstWorld = World.Create();
    using var secondWorld = World.Create();
    var first = firstWorld.Create();
    var second = secondWorld.Create();

    Assert(first.Id == second.Id && first.Version == second.Version, "The local ids collide.");
    Assert(first.WorldId != secondWorld.Id, "The Entity belongs to a different World.");
    Assert(secondWorld.IsAlive(first), "IsAlive checks the local id/version without WorldId.");
  }

  private static void VerifyWorldIdGuardRejectsCrossWorldEntity()
  {
    using var firstWorld = World.Create();
    using var secondWorld = World.Create();
    var foreign = firstWorld.Create();

    Assert(!HasWorldOwnership(secondWorld, foreign), "The caller rejects a foreign WorldId.");
  }

  private static void VerifyTokenGuardRejectsRetiredSession()
  {
    using var world = World.Create();
    var entity = world.Create();
    var oldToken = Guid.NewGuid();
    var currentToken = Guid.NewGuid();

    Assert(!HasSessionOwnership(world, entity, oldToken, currentToken), "A retired token is rejected.");
  }

  private static void VerifyHasPresentComponent()
  {
    using var world = World.Create();
    var entity = world.Create();
    world.Add(entity, new ProbePositionComponent(3));
    Assert(world.Has<ProbePositionComponent>(entity), "Has reports an attached component.");
  }

  private static void VerifyHasAbsentComponent()
  {
    using var world = World.Create();
    var entity = world.Create();
    Assert(!world.Has<ProbePositionComponent>(entity), "Has reports an absent component.");
  }

  private static void VerifyGetReturnsWritableRef()
  {
    using var world = World.Create();
    var entity = world.Create();
    world.Add(entity, new ProbePositionComponent(4));
    ref var component = ref world.Get<ProbePositionComponent>(entity);
    component.Value = 9;
    Assert(world.Get<ProbePositionComponent>(entity).Value == 9, "Get returns a writable component ref.");
  }

  private static void VerifyTryGetReturnsStructCopy()
  {
    using var world = World.Create();
    var entity = world.Create();
    world.Add(entity, new ProbePositionComponent(5));
    Assert(world.TryGet(entity, out ProbePositionComponent copy), "TryGet finds the struct component.");
    copy.Value = 17;
    Assert(world.Get<ProbePositionComponent>(entity).Value == 5, "Editing the out struct copy does not commit.");
  }

  private static void VerifyTryGetRefWhenPresent()
  {
    using var world = World.Create();
    var entity = world.Create();
    world.Add(entity, new ProbePositionComponent(2));
    ref var component = ref world.TryGetRef<ProbePositionComponent>(entity, out var exists);
    Assert(exists, "TryGetRef marks a present component.");
    component.Value = 8;
    Assert(world.Get<ProbePositionComponent>(entity).Value == 8, "The returned ref writes to the component.");
  }

  private static void VerifyTryGetRefWhenAbsent()
  {
    using var world = World.Create();
    var entity = world.Create();
    _ = world.TryGetRef<ProbePositionComponent>(entity, out var exists);
    Assert(!exists, "TryGetRef marks an absent component.");
  }

  private static void VerifySetExistingComponent()
  {
    using var world = World.Create();
    var entity = world.Create();
    world.Add(entity, new ProbePositionComponent(1));
    world.Set(entity, new ProbePositionComponent(6));
    Assert(world.Get<ProbePositionComponent>(entity).Value == 6, "Set replaces an existing component value.");
  }

  private static void VerifyAddComponent()
  {
    using var world = World.Create();
    var entity = world.Create();
    world.Add(entity, new ProbeVelocityComponent(7));
    Assert(world.Has<ProbeVelocityComponent>(entity), "Add changes the component composition.");
  }

  private static void VerifyRemoveComponent()
  {
    using var world = World.Create();
    var entity = world.Create();
    world.Add(entity, new ProbePositionComponent(1));
    world.Remove<ProbePositionComponent>(entity);
    Assert(!world.Has<ProbePositionComponent>(entity), "Remove removes the component composition.");
  }

  private static void VerifyClassInstancesAreIndependent()
  {
    using var world = World.Create();
    var first = world.Create();
    var second = world.Create();
    world.Add(first, new ProbeClassComponent(1));
    world.Add(second, new ProbeClassComponent(2));
    world.Get<ProbeClassComponent>(first).Value = 10;

    Assert(!ReferenceEquals(world.Get<ProbeClassComponent>(first), world.Get<ProbeClassComponent>(second)),
      "Separately supplied class component instances stay separate.");
    Assert(world.Get<ProbeClassComponent>(second).Value == 2, "Editing one entity leaves the other instance alone.");
  }

  private static void VerifyClassAliasesAreNotDeepCopied()
  {
    using var world = World.Create();
    var first = world.Create();
    var second = world.Create();
    var shared = new ProbeClassComponent(3);
    world.Add(first, shared);
    world.Add(second, shared);
    world.Get<ProbeClassComponent>(first).Value = 11;

    Assert(ReferenceEquals(world.Get<ProbeClassComponent>(first), world.Get<ProbeClassComponent>(second)),
      "Arch keeps the caller-supplied shared class reference.");
    Assert(world.Get<ProbeClassComponent>(second).Value == 11, "The shared class reference aliases across entities.");
  }

  private static void VerifyRefIsReacquiredAfterStructureChange()
  {
    using var world = World.Create();
    var entity = world.Create();
    world.Add(entity, new ProbePositionComponent(13));
    world.Add(entity, new ProbeVelocityComponent(1));
    ref var current = ref world.Get<ProbePositionComponent>(entity);
    Assert(current.Value == 13, "The component can be reacquired after a structural change.");
  }

  private static void VerifyCriticalComponentAccess()
  {
    using var world = World.Create();
    var first = world.Create();
    var second = world.Create();
    world.Add(first, new ProbePositionComponent(2));
    world.Add(first, new ProbeClassComponent(3));
    world.Add(second, new ProbeClassComponent(4));

    Assert(world.Has<ProbePositionComponent>(first), "Has sees the position component.");
    Assert(world.TryGet(first, out ProbePositionComponent copy), "TryGet returns the struct component.");
    copy.Value = 20;
    Assert(world.Get<ProbePositionComponent>(first).Value == 2, "TryGet returns a struct copy.");

    ref var position = ref world.TryGetRef<ProbePositionComponent>(first, out var exists);
    Assert(exists, "TryGetRef reports the component.");
    position.Value = 5;
    world.Add(first, new ProbeVelocityComponent(6));
    Assert(world.Get<ProbePositionComponent>(first).Value == 5, "The caller reacquires after structural change.");

    world.Set(first, new ProbePositionComponent(7));
    Assert(world.Get<ProbePositionComponent>(first).Value == 7, "Set updates the existing component.");
    world.Remove<ProbeVelocityComponent>(first);
    Assert(!world.Has<ProbeVelocityComponent>(first), "Remove changes the component signature.");
    world.Add(first, new ProbeVelocityComponent(8));
    Assert(world.Get<ProbeVelocityComponent>(first).Value == 8, "Add attaches a new component.");

    world.Get<ProbeClassComponent>(first).Value = 30;
    Assert(world.Get<ProbeClassComponent>(second).Value == 4, "Independent class instances are isolated.");
  }

  private static void VerifyQueryAllAllowsExtraComponents()
  {
    using var world = CreateQueryWorld();
    var query = new QueryDescription();
    query.WithAll<ProbePositionComponent, ProbeVelocityComponent>();
    Assert(world.CountEntities(query) == 2, "All requires both components and allows an extra Marker.");
  }

  private static void VerifyQueryAny()
  {
    using var world = CreateQueryWorld();
    var query = new QueryDescription();
    query.WithAny<ProbePositionComponent, ProbeVelocityComponent>();
    Assert(world.CountEntities(query) == 4, "Any requires at least one listed component.");
  }

  private static void VerifyQueryNone()
  {
    using var world = CreateQueryWorld();
    var query = new QueryDescription();
    query.WithNone<ProbeMarkerComponent, ProbeHealthComponent>();
    Assert(world.CountEntities(query) == 2, "None excludes entities with either listed component.");
  }

  private static void VerifyQueryExclusive()
  {
    using var world = CreateQueryWorld();
    var query = new QueryDescription();
    query.WithExclusive<ProbePositionComponent, ProbeVelocityComponent>();
    Assert(world.CountEntities(query) == 1, "Exclusive matches the exact component combination.");
  }

  private static void VerifyCriticalQueryComposition()
  {
    using var world = CreateQueryWorld();
    var all = new QueryDescription();
    all.WithAll<ProbePositionComponent, ProbeVelocityComponent>();
    var any = new QueryDescription();
    any.WithAny<ProbePositionComponent, ProbeVelocityComponent>();
    var none = new QueryDescription();
    none.WithNone<ProbeMarkerComponent, ProbeHealthComponent>();
    var exclusive = new QueryDescription();
    exclusive.WithExclusive<ProbePositionComponent, ProbeVelocityComponent>();

    Assert(world.CountEntities(all) == 2, "All includes entities with additional components.");
    Assert(world.CountEntities(any) == 4, "Any includes entities with either required component.");
    Assert(world.CountEntities(none) == 2, "None rejects either excluded component.");
    Assert(world.CountEntities(exclusive) == 1, "Exclusive requires an exact signature.");
  }

  private static void VerifyBufferedCreateIsStaged()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var temporary = buffer.Create(ComponentTypesForPositionAndVelocity());
    var beforePlayback = QueryForPositionAndVelocity();

    Assert(temporary.Id < 0, "CommandBuffer.Create returns a negative-id staged Entity.");
    Assert(world.CountEntities(beforePlayback) == 0, "The staged entity is not in the World before playback.");
  }

  private static void VerifyBufferedCreateComponentTypes()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    _ = buffer.Create(ComponentTypesForPositionAndVelocity());
    buffer.Playback(world);

    var query = QueryForPositionAndVelocity();
    Assert(world.CountEntities(query) == 1, "Playback creates the requested component signature.");
  }

  private static void VerifyPlaybackClearsByDefault()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    _ = buffer.Create(ComponentTypesForPositionAndVelocity());
    buffer.Playback(world);
    var count = world.CountEntities(QueryForPositionAndVelocity());
    Assert(buffer.Size == 0, "Playback clears commands by default.");
    buffer.Playback(world);
    Assert(world.CountEntities(QueryForPositionAndVelocity()) == count, "A second playback has no effect.");
  }

  private static void VerifyPlaybackCanRetainCommands()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    _ = buffer.Create(ComponentTypesForPositionAndVelocity());
    buffer.Playback(world, dispose: false);
    var firstCount = world.CountEntities(QueryForPositionAndVelocity());
    Assert(buffer.Size > 0, "Playback with dispose false retains the commands.");
    buffer.Playback(world);
    Assert(world.CountEntities(QueryForPositionAndVelocity()) == firstCount + 1,
      "Retained Create commands are replayed on the next Playback.");
  }

  private static void VerifyAddRecordsValueInSetGroup()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var entity = world.Create();
    buffer.Set(entity, new ProbePositionComponent(1));
    buffer.Add(entity, new ProbePositionComponent(2));
    buffer.Playback(world);

    Assert(world.Get<ProbePositionComponent>(entity).Value == 2,
      "Add records its component value in the same Set group, overwriting the earlier Set value.");
  }

  private static void VerifyRepeatedSetUsesLastValue()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var entity = world.Create();
    world.Add(entity, new ProbePositionComponent(0));
    buffer.Set(entity, new ProbePositionComponent(1));
    buffer.Set(entity, new ProbePositionComponent(2));
    buffer.Playback(world);
    Assert(world.Get<ProbePositionComponent>(entity).Value == 2, "The last buffered Set value wins.");
  }

  private static void VerifyRepeatedAddUsesLastValue()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var entity = world.Create();
    buffer.Add(entity, new ProbePositionComponent(1));
    buffer.Add(entity, new ProbePositionComponent(2));
    buffer.Playback(world);
    Assert(world.Get<ProbePositionComponent>(entity).Value == 2, "The last buffered Add value wins.");
  }

  private static void VerifyBufferedRemove()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var entity = world.Create();
    world.Add(entity, new ProbePositionComponent(1));
    buffer.Remove<ProbePositionComponent>(entity);
    buffer.Playback(world);
    Assert(!world.Has<ProbePositionComponent>(entity), "Playback applies buffered Remove.");
  }

  private static void VerifyBufferedDestroy()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var entity = world.Create();
    buffer.Destroy(entity);
    buffer.Playback(world);
    Assert(!world.IsAlive(entity), "Playback applies buffered Destroy.");
  }

  private static void VerifyDestroyedTargetPlayback()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var entity = world.Create();
    world.Add(entity, new ProbePositionComponent(1));
    world.Destroy(entity);
    buffer.Set(entity, new ProbePositionComponent(2));

    var exception = CaptureException(() => buffer.Playback(world));
    Console.WriteLine(exception is null
      ? "OBSERVE destroyed target: Playback returned without an exception."
      : $"OBSERVE destroyed target: Playback threw {exception.GetType().FullName}.");
    Assert(!world.IsAlive(entity), "The destroyed target remains not alive.");
  }

  private static void VerifyPlaybackKeepsEarlierEffectsOnFailure()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var addTarget = world.Create();
    var missingComponentTarget = world.Create();
    buffer.Set(missingComponentTarget, new ProbePositionComponent(9));
    buffer.Add(addTarget, new ProbePositionComponent(5));

    var exception = CaptureException(() => buffer.Playback(world));
    Assert(exception is not null, "Set to a missing component causes a playback exception.");
    Assert(world.Has<ProbePositionComponent>(addTarget), "The earlier Add remains applied after failure.");
    Assert(world.Get<ProbePositionComponent>(addTarget).Value == 5, "The partial Add value remains visible.");
  }

  private static void VerifyBufferedSetDoesNotAdd()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var entity = world.Create();
    buffer.Set(entity, new ProbePositionComponent(1));
    var exception = CaptureException(() => buffer.Playback(world));
    Assert(exception is not null, "Set does not add an absent component.");
    Assert(!world.Has<ProbePositionComponent>(entity), "Set failure leaves the absent component absent.");
  }

  private static void VerifyCriticalCommandBufferBehavior()
  {
    using var world = World.Create();
    using var createBuffer = new CommandBuffer();
    var staged = createBuffer.Create(ComponentTypesForPositionAndVelocity());

    Assert(staged.Id < 0, "Create returns a staged negative-id Entity.");
    Assert(world.CountEntities(QueryForPositionAndVelocity()) == 0, "Staged creation is not immediately visible.");
    createBuffer.Playback(world);
    Assert(world.CountEntities(QueryForPositionAndVelocity()) == 1, "Playback creates the staged component signature.");

    using var groupedBuffer = new CommandBuffer();
    var addTarget = world.Create();
    var setTargetWithoutComponent = world.Create();
    groupedBuffer.Set(setTargetWithoutComponent, new ProbePositionComponent(9));
    groupedBuffer.Add(addTarget, new ProbePositionComponent(5));

    var exception = CaptureException(() => groupedBuffer.Playback(world));
    Assert(exception is not null, "Set on a missing component fails after the Add group.");
    Assert(world.Has<ProbePositionComponent>(addTarget), "The earlier Add group remains applied.");
    Assert(world.Get<ProbePositionComponent>(addTarget).Value == 5,
      "The earlier grouped Add effect remains visible after the later Set failure.");

    using var valueBuffer = new CommandBuffer();
    var valueTarget = world.Create();
    valueBuffer.Set(valueTarget, new ProbePositionComponent(1));
    valueBuffer.Add(valueTarget, new ProbePositionComponent(2));
    valueBuffer.Playback(world);
    Assert(world.Get<ProbePositionComponent>(valueTarget).Value == 2,
      "Add shares the Set value slot and its later recorded value wins after the archetype move.");
  }

  private static World CreateQueryWorld()
  {
    var world = World.Create();
    var positionAndVelocity = world.Create();
    world.Add(positionAndVelocity, new ProbePositionComponent(1));
    world.Add(positionAndVelocity, new ProbeVelocityComponent(1));

    var positionOnly = world.Create();
    world.Add(positionOnly, new ProbePositionComponent(2));

    var velocityAndMarker = world.Create();
    world.Add(velocityAndMarker, new ProbeVelocityComponent(3));
    world.Add(velocityAndMarker, new ProbeMarkerComponent());

    var positionVelocityAndMarker = world.Create();
    world.Add(positionVelocityAndMarker, new ProbePositionComponent(4));
    world.Add(positionVelocityAndMarker, new ProbeVelocityComponent(4));
    world.Add(positionVelocityAndMarker, new ProbeMarkerComponent());
    return world;
  }

  private static ComponentType[] ComponentTypesForPositionAndVelocity()
  {
    return [typeof(ProbePositionComponent), typeof(ProbeVelocityComponent)];
  }

  private static QueryDescription QueryForPositionAndVelocity()
  {
    var query = new QueryDescription();
    query.WithAll<ProbePositionComponent, ProbeVelocityComponent>();
    return query;
  }

  private static bool HasWorldOwnership(World world, Entity entity)
  {
    return entity.WorldId == world.Id;
  }

  private static bool HasSessionOwnership(World world, Entity entity, Guid entityToken, Guid currentToken)
  {
    return entityToken == currentToken && HasWorldOwnership(world, entity) && world.IsAlive(entity);
  }

  private static Exception? CaptureException(Action action)
  {
    try
    {
      action();
      return null;
    }
    catch (Exception exception)
    {
      return exception;
    }
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}

internal struct ProbePositionComponent
{
  public ProbePositionComponent(int value)
  {
    Value = value;
  }

  public int Value;
}

internal struct ProbeVelocityComponent
{
  public ProbeVelocityComponent(int value)
  {
    Value = value;
  }

  public int Value;
}

internal struct ProbeMarkerComponent
{
}

internal struct ProbeHealthComponent
{
}

internal sealed class ProbeClassComponent
{
  public ProbeClassComponent(int value)
  {
    Value = value;
  }

  public int Value { get; set; }
}
