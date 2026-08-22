using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation.Items.Snapshots;

namespace Terraria.Dome.Server.Replication;

public sealed class InventoryReplicationAssembler
{
  public IReadOnlyList<byte[]> CollectFrames(byte playerSlot, InventorySnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    List<byte[]> frames = new(snapshot.Slots.Count);
    for (int slotId = 0; slotId < snapshot.Slots.Count; slotId++)
    {
      ItemInstanceSnapshot instance = snapshot.Slots[slotId];
      if (instance.State.PrefixId > byte.MaxValue)
      {
        throw new InvalidOperationException(
          "An item prefix cannot be represented by the V1456 equipment packet.");
      }

      frames.Add(TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
        playerSlot,
        slotId,
        instance.Stack.Quantity,
        (byte)instance.State.PrefixId,
        instance.Stack.ItemType,
        instance.State.IsFavorited,
        instance.State.IsNewAndShiny)));
    }

    return frames;
  }

  public IReadOnlyList<InventorySnapshot> Project(
    IReadOnlyList<InventorySnapshot> snapshots)
  {
    ArgumentNullException.ThrowIfNull(snapshots);
    return Array.AsReadOnly([.. snapshots]);
  }
}
