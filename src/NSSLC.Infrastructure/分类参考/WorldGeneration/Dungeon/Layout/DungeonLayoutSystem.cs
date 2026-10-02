using Terraria.WorldGeneration.Dungeon.Bounds;
using Terraria.WorldGeneration.Dungeon.Rooms;

namespace Terraria.WorldGeneration.Dungeon.Layout;

public sealed class DungeonLayoutSystem
{
  public RoomEntry AddRoom(
    DungeonLayoutGraphWorkState graph,
    int entryId,
    DungeonRoomDefinition room,
    float progressAlongSnake)
  {
    ArgumentNullException.ThrowIfNull(graph);
    var entry = new RoomEntry(entryId, room, progressAlongSnake);
    graph.AddRoom(entry);
    return entry;
  }

  public HallLine ConnectRooms(
    DungeonLayoutGraphWorkState graph,
    int lineId,
    int sourceEntryId,
    int targetEntryId,
    DungeonTilePoint sourcePoint,
    DungeonTilePoint targetPoint)
  {
    ArgumentNullException.ThrowIfNull(graph);
    if (!graph.Rooms.ContainsKey(sourceEntryId) || !graph.Rooms.ContainsKey(targetEntryId))
    {
      throw new InvalidOperationException(
        "Both hall-line endpoints must be registered room entries.");
    }

    var hallLine = new HallLine(
      lineId,
      sourceEntryId,
      targetEntryId,
      sourcePoint,
      targetPoint);
    graph.AddHall(hallLine);
    graph.Rooms[sourceEntryId].AddForwardLink(targetEntryId);
    graph.Rooms[targetEntryId].AddBackLink(sourceEntryId);
    return hallLine;
  }

  public bool IsWithinProtectedBounds(
    DungeonBoundsRectangle protectedBounds,
    DungeonTilePoint point)
  {
    return protectedBounds.Contains(point);
  }
}
