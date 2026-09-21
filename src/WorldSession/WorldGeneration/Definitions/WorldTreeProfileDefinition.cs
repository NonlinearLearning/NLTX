using System;

namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldTreeProfileDefinition
{
  public WorldTreeProfileDefinition(
    WorldTreeProfileId id,
    ushort treeTileType,
    ushort saplingTileType,
    int treeHeightMin,
    int treeHeightMax,
    int treeTopPaddingNeeded,
    WorldTreeGroundPredicateId groundPredicate,
    WorldTreeWallPredicateId wallPredicate)
  {
    if (!Enum.IsDefined(id))
    {
      throw new ArgumentOutOfRangeException(nameof(id));
    }

    if (treeHeightMin <= 0 || treeHeightMax < treeHeightMin)
    {
      throw new ArgumentOutOfRangeException(nameof(treeHeightMin));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(treeTopPaddingNeeded);
    if (!Enum.IsDefined(groundPredicate))
    {
      throw new ArgumentOutOfRangeException(nameof(groundPredicate));
    }

    if (!Enum.IsDefined(wallPredicate))
    {
      throw new ArgumentOutOfRangeException(nameof(wallPredicate));
    }

    Id = id;
    TreeTileType = treeTileType;
    SaplingTileType = saplingTileType;
    TreeHeightMin = treeHeightMin;
    TreeHeightMax = treeHeightMax;
    TreeTopPaddingNeeded = treeTopPaddingNeeded;
    GroundPredicate = groundPredicate;
    WallPredicate = wallPredicate;
  }

  public WorldTreeProfileId Id { get; }

  public ushort TreeTileType { get; }

  public ushort SaplingTileType { get; }

  public int TreeHeightMin { get; }

  public int TreeHeightMax { get; }

  public int TreeTopPaddingNeeded { get; }

  public WorldTreeGroundPredicateId GroundPredicate { get; }

  public WorldTreeWallPredicateId WallPredicate { get; }
}
