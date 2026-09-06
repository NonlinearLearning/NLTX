using System;

namespace Terraria.Npc;

public sealed class NpcParentRelationComponent
{
  public NpcParentRelationComponent(
    NpcInstanceId parentInstanceId,
    NpcSlot parentLegacySlot,
    long attachedAtTick)
  {
    if (attachedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(attachedAtTick),
        "Attached tick cannot be negative.");
    }

    ParentInstanceId = parentInstanceId;
    ParentLegacySlot = parentLegacySlot;
    AttachedAtTick = attachedAtTick;
  }

  public NpcInstanceId ParentInstanceId { get; }

  public NpcSlot ParentLegacySlot { get; }

  public long AttachedAtTick { get; }

  public bool HasLegacyParentSlot => ParentLegacySlot.IsAssigned;
}
