using System;
using System.Collections.Generic;

namespace Terraria.Npc;

// status: partial
// sourceMembers: playerInteraction, lastInteraction
// crossSubsystemOwner: interaction command owner integration-review
public sealed class NpcInteractionStateComponent
{
  public NpcInteractionStateComponent(
    IReadOnlyList<bool>? playerInteraction = null,
    int lastInteraction = -1)
  {
    PlayerInteraction = playerInteraction is null
      ? ReadOnlyMemory<bool>.Empty
      : new ReadOnlyMemory<bool>(new List<bool>(playerInteraction).ToArray());
    LastInteraction = lastInteraction;
  }

  public ReadOnlyMemory<bool> PlayerInteraction { get; }

  public int LastInteraction { get; }
}
