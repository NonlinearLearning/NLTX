using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Snapshots;

namespace Terraria.Dome.Server.Replication;

public sealed class InventoryReplicationAssembler
{
  private readonly ItemDefinitionRegistry _itemDefinitions;

  public InventoryReplicationAssembler(ItemDefinitionRegistry itemDefinitions)
  {
    _itemDefinitions = itemDefinitions ?? throw new ArgumentNullException(nameof(itemDefinitions));
  }

  public IReadOnlyList<byte[]> CollectFrames(byte playerSlot, InventorySnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    List<byte[]> frames = new(snapshot.Slots.Count);
    for (int slotId = 0; slotId < snapshot.Slots.Count; slotId++)
    {
      ItemInstanceSnapshot instance = snapshot.Slots[slotId];
      ItemReplicationAdmission.ValidateInstance(instance, _itemDefinitions);

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
