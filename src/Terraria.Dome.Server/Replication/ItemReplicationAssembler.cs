using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Snapshots;

namespace Terraria.Dome.Server.Replication;

public sealed class ItemReplicationAssembler
{
  private readonly ItemDefinitionRegistry _itemDefinitions;

  public ItemReplicationAssembler(ItemDefinitionRegistry itemDefinitions)
  {
    _itemDefinitions = itemDefinitions ?? throw new ArgumentNullException(nameof(itemDefinitions));
  }

  public IReadOnlyList<byte[]> CollectFrames(
    SessionReplicationState session,
    IReadOnlyList<ItemReplicationSnapshot> items)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(items);
    List<byte[]> frames = new();
    for (int index = 0; index < items.Count; index++)
    {
      ItemReplicationSnapshot item = items[index];
      item.Validate();
      ItemReplicationAdmission.ValidateInstance(
        new ItemInstanceSnapshot(item.Stack, item.InstanceState),
        _itemDefinitions);
      if (!session.VisibleSections.Contains(item.Section) || !session.ShouldSendItem(item))
      {
        continue;
      }

      frames.Add(TerrariaPacketCodec.EncodeItemReplication(item));
    }

    return frames;
  }
}
