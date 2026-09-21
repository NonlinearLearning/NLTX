using System;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Loot;

public sealed class LootTable
{
  private readonly WorldSeed _seed;

  public LootTable(WorldSeed seed)
  {
    _seed = seed;
  }

  public ItemStack RollDomeChaserNpcDrop(int npcReplicationId)
  {
    if (npcReplicationId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(npcReplicationId));
    }

    uint value = unchecked((uint)(_seed.Value ^ npcReplicationId * 1103515245));
    int quantity = 1 + (int)(value % 2U);
    return new ItemStack(1, quantity);
  }

  public ItemStack Roll(
    int lootTableId,
    int replicationId,
    ushort itemType,
    int minimumQuantity,
    int maximumQuantity)
  {
    if (lootTableId <= 0 || replicationId <= 0 || itemType == 0 || minimumQuantity <= 0 ||
        maximumQuantity < minimumQuantity)
    {
      throw new ArgumentOutOfRangeException(nameof(lootTableId));
    }

    uint value = unchecked((uint)(_seed.Value ^ replicationId * 1103515245 ^ lootTableId * 486187739));
    long quantityRange = (long)maximumQuantity - minimumQuantity + 1;
    int quantity = minimumQuantity + (int)(value % (ulong)quantityRange);
    return new ItemStack(itemType, quantity);
  }
}
