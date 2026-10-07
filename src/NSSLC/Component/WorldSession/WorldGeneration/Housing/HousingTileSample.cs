namespace Terraria.WorldGeneration.Housing;

public readonly record struct HousingTileSample(
  bool IsActive,
  bool IsSolid,
  bool IsOpenGate,
  ushort WallType,
  bool IsHouseWall,
  bool HasTorch,
  bool HasDoor,
  bool HasChair,
  bool HasTable,
  bool IsStinkbug,
  bool IsEchoStinkbug)
{
  public int TileType { get; init; }

  public bool HasAnyWall => WallType != 0;
}
