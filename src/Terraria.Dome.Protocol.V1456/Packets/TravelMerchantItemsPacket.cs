using System;
using System.Collections.Generic;

namespace Terraria.Dome.Protocol.V1456.Packets;

public sealed class TravelMerchantItemsPacket
{
  public const int SlotCount = 40;

  public TravelMerchantItemsPacket(IReadOnlyList<short> itemIds)
  {
    ArgumentNullException.ThrowIfNull(itemIds);
    if (itemIds.Count != SlotCount)
    {
      throw new ArgumentOutOfRangeException(nameof(itemIds));
    }

    short[] copy = new short[SlotCount];
    for (int index = 0; index < SlotCount; index++)
    {
      copy[index] = itemIds[index];
    }

    ItemIds = copy;
  }

  public IReadOnlyList<short> ItemIds { get; }
}
