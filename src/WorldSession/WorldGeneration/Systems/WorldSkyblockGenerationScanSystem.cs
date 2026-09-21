using System;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Systems;

public static class WorldSkyblockGenerationScanSystem
{
  public static WorldSkyblockGenerationScanSnapshot Scan(
    WorldSkyblockGenerationScanComponent component,
    WorldSkyblockGenerationDimensions dimensions,
    ulong scanVersion,
    IWorldSkyblockGenerationGridReader gridReader)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(gridReader);
    component.BeginScan(scanVersion, dimensions.WorldTileCount);
    int maxX = Math.Max(40, dimensions.MaxTilesX - 40);
    int maxY = Math.Max(40, dimensions.MaxTilesY - 40);
    for (int x = 40; x < maxX; x++)
    {
      for (int y = 40; y < maxY; y++)
      {
        WorldSkyblockGenerationTileObservation observation = gridReader.Read(x, y);
        component.RecordTile(
          observation.TileType,
          observation.WallType,
          observation.IsActive);
      }
    }

    return component.CreateSnapshot();
  }
}
