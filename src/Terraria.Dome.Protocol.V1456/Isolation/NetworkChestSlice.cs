using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public sealed class NetworkChestSlice
{
  private NetworkChestSlice(
    int chestId,
    int tileX,
    int tileY,
    IReadOnlyList<NetworkItemStackSlice> slots,
    long revision)
  {
    ChestId = chestId;
    TileX = tileX;
    TileY = tileY;
    Slots = slots;
    Revision = revision;
  }

  public int ChestId { get; }

  public int TileX { get; }

  public int TileY { get; }

  public IReadOnlyList<NetworkItemStackSlice> Slots { get; }

  public long Revision { get; }

  public static NetworkChestSlice From(ChestSnapshot snapshot)
  {
    NetworkItemStackSlice[] slots = new NetworkItemStackSlice[snapshot.Slots.Count];
    for (int index = 0; index < snapshot.Slots.Count; index++)
    {
      slots[index] = NetworkItemStackSlice.From(snapshot.Slots[index]);
    }

    return new NetworkChestSlice(
      snapshot.ChestId,
      snapshot.TileX,
      snapshot.TileY,
      slots,
      snapshot.Revision);
  }
}
