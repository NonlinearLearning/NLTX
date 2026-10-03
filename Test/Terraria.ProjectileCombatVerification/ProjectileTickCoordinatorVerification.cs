using System;
using System.Collections.Generic;
using System.Numerics;

using Terraria.Content;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.WorldStorage;

internal static class ProjectileTickCoordinatorVerification
{
  public static void Run()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 31, extraUpdates: 2),
      CreateDefinition(typeId: 32, extraUpdates: 0),
      CreateDefinition(typeId: 33, extraUpdates: 2),
    });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 4);
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex());
    var context = new ProjectileDefinitionHydrationContext(
      catalogRevision: 1,
      npcCapacity: 4,
      playerCapacity: 4);

    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 31, center: new Vector2(10, 10)),
        definitions,
        context,
        out ProjectileHandle first),
      "The first projectile should be available to the ordered pass.");
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 32, center: new Vector2(20, 20)),
        definitions,
        context,
        out ProjectileHandle second),
      "The second projectile should be available to the ordered pass.");

    var adapter = new RecordingAdapter();
    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(2, result.ActiveProjectileCount, "Both active slots should be visited.");
    AssertEqual(0, result.SkippedInactiveCount, "No active slot should be skipped.");
    AssertEqual(4, result.UpdateStepCount, "ExtraUpdates should add ordered substeps.");
    AssertSequenceEqual(
      new[]
      {
        "pre",
        "slot-0-step-0-of-3",
        "slot-0-step-1-of-3",
        "slot-0-step-2-of-3",
        "slot-1-step-0-of-1",
        "post",
      },
      adapter.Events,
      "The pass must preserve pre, ascending slots, extra updates, and post order.");
    AssertEqual(first.Slot.Value, adapter.Handles[0].Slot.Value,
      "The first callback should retain the first local slot.");
    AssertEqual(second.Slot.Value, adapter.Handles[3].Slot.Value,
      "The second callback should retain the second local slot.");

    Assert(
      lifecycle.TryGet(first, out ProjectileEntityState? firstState) &&
      firstState is not null,
      "The first projectile should remain active after the first pass.");
    firstState!.HitImmunity.LocalNpcImmunityTicks[0] = 2;

    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 33, center: new Vector2(30, 30)),
        definitions,
        context,
        out ProjectileHandle expiring),
      "A short-lived projectile should be available for the next pass.");

    var secondAdapter = new RecordingAdapter();
    ProjectileTickResult secondResult =
      new ProjectileTickCoordinator(lifecycle).Tick(secondAdapter);

    AssertEqual(
      7,
      secondResult.UpdateStepCount,
      "The second pass should preserve each active projectile's extra-update substeps.");
    AssertEqual(
      1,
      firstState.HitImmunity.LocalNpcImmunityTicks[0],
      "Local immunity must advance once per regular Update, not once per extra substep.");
    Assert(
      !lifecycle.TryGet(expiring, out _),
      "A projectile reaching zero timeLeft must be released after its substeps.");

    VerifyAdapterTerminationStopsRemainingSubsteps();
  }

  private static void VerifyAdapterTerminationStopsRemainingSubsteps()
  {
    var definitions = new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(typeId: 34, extraUpdates: 2),
    });
    var slots = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      maximumCapacity: 1);
    var lifecycle = new ProjectileLifecycleSystem(slots, new ProjectileIdentityIndex());
    var context = new ProjectileDefinitionHydrationContext(
      catalogRevision: 1,
      npcCapacity: 1,
      playerCapacity: 1);
    Assert(
      lifecycle.TrySpawn(
        CreateSpawn(typeId: 34, center: new Vector2(40, 40)),
        definitions,
        context,
        out ProjectileHandle handle),
      "A projectile for adapter termination should be available.");

    var adapter = new TerminatingAdapter(lifecycle);
    ProjectileTickResult result = new ProjectileTickCoordinator(lifecycle).Tick(adapter);

    AssertEqual(1, result.ActiveProjectileCount,
      "The terminating projectile should be counted once before its callback.");
    AssertEqual(1, result.UpdateStepCount,
      "Termination during the first substep must stop remaining extra updates.");
    AssertEqual(1, adapter.UpdateCount,
      "The adapter must receive exactly one callback before lifecycle termination.");
    AssertSequenceEqual(
      new[] { "pre", "update", "post" },
      adapter.Events,
      "PostUpdateAllProjectiles must still run after adapter termination.");
    Assert(!lifecycle.TryGet(handle, out _),
      "Adapter termination must release the projectile before the next substep.");
  }

  private static ProjectileSpawnCommand CreateSpawn(int typeId, Vector2 center)
  {
    return new ProjectileSpawnCommand(
      typeId,
      new ProjectileOwnerReference(EntityReference.None, legacyOwnerSlot: 1),
      center,
      Vector2.Zero,
      damage: 1,
      originalDamage: 1,
      knockback: 0.0f);
  }

  private static ProjectileDefinition CreateDefinition(int typeId, int extraUpdates)
  {
    var penetration = new ProjectilePenetrationDefinition(1, 1, true)
    {
      UsesLocalNpcImmunity = true,
    };
    return new ProjectileDefinition(
      new ProjectileIdentityDefinition(typeId, null, NeedsUuid: false),
      new ProjectileGeometryDefinition(8, 8, 1.0f, true, false),
      new ProjectileBehaviorDefinition(
        1,
        extraUpdates,
        typeId == 33 ? 1 : 30),
      new ProjectileCombatDefinition(0, 0.0f, true, false),
      penetration,
      new ProjectileCapabilitiesDefinition(false, false, false, false),
      new ProjectilePresentationDefinition(1, 0.0f, false));
  }

  private sealed class RecordingAdapter : IProjectileTickAdapter
  {
    public List<string> Events { get; } = new();

    public List<ProjectileHandle> Handles { get; } = new();

    public void PreUpdateAllProjectiles()
    {
      Events.Add("pre");
    }

    public void UpdateProjectile(
      ProjectileTickContext context,
      ProjectileEntityState state)
    {
      Events.Add(
        $"slot-{context.SlotIndex}-step-{context.SubstepIndex}-of-{context.SubstepCount}");
      Handles.Add(context.Handle);
      AssertEqual(
        context.Handle.Slot.Value,
        state.Identity.SlotIndex,
        "The callback state must match the current lifecycle slot.");
    }

    public void PostUpdateAllProjectiles()
    {
      Events.Add("post");
    }
  }

  private sealed class TerminatingAdapter : IProjectileTickAdapter
  {
    private readonly ProjectileLifecycleSystem _lifecycle;

    public TerminatingAdapter(ProjectileLifecycleSystem lifecycle)
    {
      _lifecycle = lifecycle;
    }

    public List<string> Events { get; } = new();

    public int UpdateCount { get; private set; }

    public void PreUpdateAllProjectiles()
    {
      Events.Add("pre");
    }

    public void UpdateProjectile(
      ProjectileTickContext context,
      ProjectileEntityState state)
    {
      Events.Add("update");
      UpdateCount++;
      Assert(
        _lifecycle.TryTerminate(
          context.Handle,
          ProjectileEndReason.DestroyedByCollision),
        "The adapter should be able to submit a collision termination.");
    }

    public void PostUpdateAllProjectiles()
    {
      Events.Add("post");
    }
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void AssertEqual<T>(T expected, T actual, string message)
  {
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
      throw new InvalidOperationException(
        $"{message} Expected '{expected}', actual '{actual}'.");
    }
  }

  private static void AssertSequenceEqual(
    IReadOnlyList<string> expected,
    IReadOnlyList<string> actual,
    string message)
  {
    if (expected.Count != actual.Count)
    {
      throw new InvalidOperationException(
        $"{message} Expected count '{expected.Count}', actual '{actual.Count}'.");
    }

    for (int index = 0; index < expected.Count; index++)
    {
      if (expected[index] != actual[index])
      {
        throw new InvalidOperationException(
          $"{message} Difference at index {index}: expected '{expected[index]}', " +
          $"actual '{actual[index]}'.");
      }
    }
  }
}
