namespace Terraria.Content;

public sealed record WallDefinition(
  int TypeId,
  string? PersistentId,
  bool HouseWall,
  bool DungeonWall,
  bool Light,
  int? BlendTypeId)
{
  public byte LargeFrameCount { get; init; }
}
