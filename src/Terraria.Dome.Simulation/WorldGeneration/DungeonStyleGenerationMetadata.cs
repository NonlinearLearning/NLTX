using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonStyleGenerationMetadata
{
  public static DungeonStyleGenerationMetadata Default =>
    new(-1, -1, false, DungeonRoomType.BiomeStructured);

  public DungeonStyleGenerationMetadata(
    int unbreakableWallProgressionTier,
    int liquidType,
    bool edgeDither,
    DungeonRoomType biomeRoomType)
  {
    if (unbreakableWallProgressionTier < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(unbreakableWallProgressionTier));
    }

    if (liquidType < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(liquidType));
    }

    if (!Enum.IsDefined(biomeRoomType))
    {
      throw new ArgumentOutOfRangeException(nameof(biomeRoomType));
    }

    UnbreakableWallProgressionTier = unbreakableWallProgressionTier;
    LiquidType = liquidType;
    EdgeDither = edgeDither;
    BiomeRoomType = biomeRoomType;
  }

  public int UnbreakableWallProgressionTier { get; }

  public int LiquidType { get; }

  public bool EdgeDither { get; }

  public DungeonRoomType BiomeRoomType { get; }
}
