using System.Collections.ObjectModel;

namespace Terraria.NpcTownBestiary;

// status: proposed implementation
// crossSubsystemOwner: integration-review for the final key mode
public sealed class TownRoomRegistryComponent
{
  private readonly Dictionary<int, TownRoomTilePoint> _roomsByNpcType = new();

  public TownHousingKeyMode KeyMode => TownHousingKeyMode.Type;

  public ulong Revision { get; private set; }

  public int AssignedRoomCount => _roomsByNpcType.Count;

  public IReadOnlyDictionary<int, TownRoomTilePoint> RoomsByNpcType =>
    new ReadOnlyDictionary<int, TownRoomTilePoint>(_roomsByNpcType);

  internal bool TryAssign(int npcType, TownRoomTilePoint room, ulong expectedRevision)
  {
    if (expectedRevision != Revision)
    {
      return false;
    }

    _roomsByNpcType[npcType] = room;
    Revision++;
    return true;
  }

  internal bool TryEvict(int npcType, ulong expectedRevision)
  {
    if (expectedRevision != Revision || !_roomsByNpcType.Remove(npcType))
    {
      return false;
    }

    Revision++;
    return true;
  }

  internal void Restore(ulong revision, IEnumerable<KeyValuePair<int, TownRoomTilePoint>> rooms)
  {
    _roomsByNpcType.Clear();
    foreach (KeyValuePair<int, TownRoomTilePoint> room in rooms)
    {
      _roomsByNpcType[room.Key] = room.Value;
    }

    Revision = revision;
  }
}
