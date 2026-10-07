using System;

using EntityEcs;

using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.Projectile;

/// <summary>
/// Immutable context for one ordered projectile update substep.
/// </summary>
public readonly record struct ProjectileTickContext
{
  public ProjectileTickContext(
    ProjectileHandle handle,
    RuntimeEntityHandle runtimeHandle,
    int slotIndex,
    int substepIndex,
    int initialSubstepCount,
    ProjectileWorldBounds? worldBounds = null)
  {
    if (slotIndex < 0 || substepIndex < 0)
    {
      throw new ArgumentOutOfRangeException();
    }

    if (handle.Slot.Value != slotIndex ||
        handle.Generation == 0 ||
        !runtimeHandle.IsAssigned)
    {
      throw new ArgumentException(
        "Tick handles must identify the supplied slot and a live runtime generation.",
        nameof(handle));
    }

    if (worldBounds.HasValue && !worldBounds.Value.IsValid)
    {
      throw new ArgumentException(
        "World bounds must be initialized by the validated constructor.",
        nameof(worldBounds));
    }

    Handle = handle;
    RuntimeHandle = runtimeHandle;
    SlotIndex = slotIndex;
    SubstepIndex = substepIndex;
    InitialSubstepCount = initialSubstepCount;
    WorldBounds = worldBounds;
  }

  public ProjectileHandle Handle { get; }

  /// <summary>
  /// Runtime identity captured for this substep. Resolve it again after any callback.
  /// </summary>
  public RuntimeEntityHandle RuntimeHandle { get; }

  public int SlotIndex { get; }

  public int SubstepIndex { get; }

  /// <summary>
  /// The count configured when this regular update began. AI can extend the
  /// loop by resetting NumUpdates, so SubstepIndex may exceed this value.
  /// </summary>
  public int InitialSubstepCount { get; }

  /// <summary>
  /// World edges captured by the caller for this pass, when available.
  /// </summary>
  public ProjectileWorldBounds? WorldBounds { get; }

  /// <summary>
  /// Compatibility alias for the count configured at the start of this update.
  /// It does not include any AI-driven loop extensions.
  /// </summary>
  public int SubstepCount => InitialSubstepCount;
}
