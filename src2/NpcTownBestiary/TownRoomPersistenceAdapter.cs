using System.IO;

namespace Terraria.NpcTownBestiary;

public sealed class TownRoomPersistenceAdapter
{
  public void Save(TownRoomRegistryComponent registry, BinaryWriter writer)
  {
    ArgumentNullException.ThrowIfNull(registry);
    ArgumentNullException.ThrowIfNull(writer);
    writer.Write(registry.Revision);
    writer.Write(registry.AssignedRoomCount);
    foreach (KeyValuePair<int, TownRoomTilePoint> room in registry.RoomsByNpcType)
    {
      writer.Write(room.Key);
      writer.Write(room.Value.X);
      writer.Write(room.Value.Y);
    }
  }

  public TownRoomRegistryComponent Load(BinaryReader reader)
  {
    ArgumentNullException.ThrowIfNull(reader);
    ulong revision = reader.ReadUInt64();
    int count = reader.ReadInt32();
    if (count < 0)
    {
      throw new InvalidDataException("Town room count cannot be negative.");
    }

    var rooms = new List<KeyValuePair<int, TownRoomTilePoint>>(count);
    for (int i = 0; i < count; i++)
    {
      rooms.Add(new KeyValuePair<int, TownRoomTilePoint>(
        reader.ReadInt32(),
        new TownRoomTilePoint(reader.ReadInt32(), reader.ReadInt32())));
    }

    var registry = new TownRoomRegistryComponent();
    registry.Restore(revision, rooms);
    return registry;
  }
}
