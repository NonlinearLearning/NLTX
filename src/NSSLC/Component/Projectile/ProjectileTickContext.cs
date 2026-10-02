using System;

using Terraria.WorldStorage;

namespace Terraria.Projectile;

/// <summary>
/// Immutable context for one ordered projectile update substep.
/// </summary>
public readonly record struct ProjectileTickContext
{
  public ProjectileTickContext(
    ProjectileHandle handle,
    int slotIndex,
    int substepIndex,
    int substepCount)
  {
    if (slotIndex < 0 || substepIndex < 0 || substepCount <= 0 ||
      substepIndex >= substepCount)
    {
      throw new ArgumentOutOfRangeException();
    }

    if (handle.Slot.Value != slotIndex || handle.Generation == 0)
    {
      throw new ArgumentException(
        "The tick handle must identify the supplied slot and a live generation.",
        nameof(handle));
    }

    Handle = handle;
    SlotIndex = slotIndex;
    SubstepIndex = substepIndex;
    SubstepCount = substepCount;
  }

  public ProjectileHandle Handle { get; }

  public int SlotIndex { get; }

  public int SubstepIndex { get; }

  public int SubstepCount { get; }
}
