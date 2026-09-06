using System;
using Terraria.Relationships;

namespace Terraria.Npc;

public sealed class NpcTargetComponent
{
  public NpcTargetComponent(
    NpcTargetKind targetKind = NpcTargetKind.None,
    EntityReference? targetReference = null,
    int legacyTargetIndex = -1,
    int previousLegacyTargetIndex = -1)
  {
    if (legacyTargetIndex < -1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(legacyTargetIndex),
        "Legacy target index must be -1 or a non-negative index.");
    }

    if (previousLegacyTargetIndex < -1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(previousLegacyTargetIndex),
        "Previous legacy target index must be -1 or a non-negative index.");
    }

    if (targetKind == NpcTargetKind.None && targetReference.HasValue)
    {
      throw new ArgumentException(
        "A target reference cannot be supplied when the target kind is None.",
        nameof(targetReference));
    }

    TargetKind = targetKind;
    TargetReference = targetReference;
    LegacyTargetIndex = legacyTargetIndex;
    PreviousLegacyTargetIndex = previousLegacyTargetIndex;
  }

  // Compatibility constructor for the existing selection timestamp API.
  public NpcTargetComponent(
    NpcTargetKind kind,
    EntityReference? targetEntity,
    long selectedAtTick)
    : this(kind, targetEntity, -1, -1)
  {
    SelectedAtTick = selectedAtTick;
  }

  public EntityReference? TargetReference { get; }

  public NpcTargetKind TargetKind { get; }

  public int LegacyTargetIndex { get; }

  public int PreviousLegacyTargetIndex { get; }

  public long SelectedAtTick { get; }

  // Compatibility aliases retained while callers move to the split API.
  public NpcTargetKind Kind => TargetKind;

  public EntityReference? TargetEntity => TargetReference;

  public bool HasTarget =>
    TargetKind != NpcTargetKind.None && TargetReference.HasValue;
}
