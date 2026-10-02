using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public sealed class TownHousingRegistry
{
  public Dictionary<TownHousingResidentKey, TilePosition> AssignedRooms = new();
  public HashSet<TownHousingResidentKey> HomelessResidents = new();
  public ulong Revision;

  public int AssignedRoomCount => AssignedRooms.Count;
  public int HomelessResidentCount => HomelessResidents.Count;
  public bool IsEmpty =>
    AssignedRooms.Count == 0 && HomelessResidents.Count == 0;
}
