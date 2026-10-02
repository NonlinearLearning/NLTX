using Terraria.WorldGeneration.Dungeon.Bounds;
using Terraria.WorldGeneration.Dungeon.Halls;
using Terraria.WorldGeneration.Dungeon.Placement;
using Terraria.WorldGeneration.Dungeon.Rooms;

namespace Terraria.WorldGeneration.Dungeon.Generation;

public sealed class DungeonGenerationCollectionsComponent
{
  private readonly List<DungeonRoomDefinition> _rooms = new();
  private readonly List<DungeonHallDefinition> _halls = new();
  private readonly List<int> _featureIds = new();
  private readonly List<DungeonDoorPlacementRequest> _doors = new();
  private readonly List<DungeonPlatformPlacementRequest> _platforms = new();

  public IReadOnlyList<DungeonRoomDefinition> Rooms => _rooms;

  public IReadOnlyList<DungeonHallDefinition> Halls => _halls;

  public IReadOnlyList<int> FeatureIds => _featureIds;

  public IReadOnlyList<DungeonDoorPlacementRequest> Doors => _doors;

  public IReadOnlyList<DungeonPlatformPlacementRequest> Platforms => _platforms;

  public DungeonBoundsRectangle ProtectedDungeonBounds { get; private set; }

  public DungeonBoundsRectangle OuterProgressionBounds { get; private set; }

  public void SetBounds(
    DungeonBoundsRectangle protectedDungeonBounds,
    DungeonBoundsRectangle outerProgressionBounds)
  {
    ProtectedDungeonBounds = protectedDungeonBounds;
    OuterProgressionBounds = outerProgressionBounds;
  }

  public void AddRoom(DungeonRoomDefinition room)
  {
    ArgumentNullException.ThrowIfNull(room);
    _rooms.Add(room);
  }

  public void AddHall(DungeonHallDefinition hall)
  {
    ArgumentNullException.ThrowIfNull(hall);
    _halls.Add(hall);
  }

  public void AddFeature(int featureId)
  {
    if (featureId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(featureId));
    }

    _featureIds.Add(featureId);
  }

  public void AddDoor(DungeonDoorPlacementRequest door)
  {
    ArgumentNullException.ThrowIfNull(door);
    _doors.Add(door);
  }

  public void AddPlatform(DungeonPlatformPlacementRequest platform)
  {
    ArgumentNullException.ThrowIfNull(platform);
    _platforms.Add(platform);
  }

  public void ClearTransientWork()
  {
    _rooms.Clear();
    _halls.Clear();
    _featureIds.Clear();
    _doors.Clear();
    _platforms.Clear();
  }
}
