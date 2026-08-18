using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Replication;

public sealed class ItemReplicationAssembler
{
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
      if (!session.VisibleSections.Contains(item.Section) || !session.ShouldSendItem(item))
      {
        continue;
      }

      frames.Add(TerrariaPacketCodec.EncodeItemReplication(item));
    }

    return frames;
  }
}
