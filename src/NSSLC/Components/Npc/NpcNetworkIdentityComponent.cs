using System;

namespace Terraria.Npc;

/// <summary>
/// Identifies the client-side slot used to project one authoritative NPC.
/// </summary>
/// <remarks>
/// The network slot and generation are a protocol projection only. Entity identity and all NPC
/// gameplay state remain authoritative in the ECS entity.
/// </remarks>
public readonly record struct NpcNetworkIdentityComponent
{
  public NpcNetworkIdentityComponent(byte slot, byte generation, short netId)
  {
    if (generation == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generation));
    }

    if (netId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(netId));
    }

    Slot = slot;
    Generation = generation;
    NetId = netId;
  }

  /// <summary>Client NPC array slot; it is not the ECS entity identity.</summary>
  public byte Slot { get; }

  /// <summary>Client-side generation used to replace a stale slot occupant.</summary>
  public byte Generation { get; }

  /// <summary>Terraria NPC net id used by the client to construct the visual NPC.</summary>
  public short NetId { get; }
}
