using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Actions;

/// <summary>
/// Describes one Version4 underwater-chest placement request at a recorded cave anchor.
/// </summary>
public readonly record struct OceanCaveTreasurePlacementCommand
{
  private OceanCaveTreasurePlacementCommand(
    long generationId,
    TilePosition anchor,
    int mainItemInChest,
    bool notNearOtherChests,
    int chestStyle,
    bool trySlope,
    ushort chestTileType)
  {
    GenerationId = generationId;
    Anchor = anchor;
    MainItemInChest = mainItemInChest;
    NotNearOtherChests = notNearOtherChests;
    ChestStyle = chestStyle;
    TrySlope = trySlope;
    ChestTileType = chestTileType;
  }

  public long GenerationId { get; }

  public TilePosition Anchor { get; }

  public int MainItemInChest { get; }

  public bool NotNearOtherChests { get; }

  public int ChestStyle { get; }

  public bool TrySlope { get; }

  public ushort ChestTileType { get; }

  public static OceanCaveTreasurePlacementCommand ForUnderwaterChest(
    long generationId,
    TilePosition anchor,
    int mainItemInChest)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (mainItemInChest < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(mainItemInChest));
    }

    return new OceanCaveTreasurePlacementCommand(
      generationId,
      anchor,
      mainItemInChest,
      notNearOtherChests: false,
      chestStyle: 17,
      trySlope: true,
      chestTileType: 0);
  }
}
