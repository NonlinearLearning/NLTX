using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class UnderwaterPlantValidationQuery
{
  private const int TileFrameWidth = 18;
  private const byte WaterLiquidType = 0;

  public static UnderwaterPlantValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    ushort plantType,
    int x,
    int y,
    bool ignoreSelf = true)
  {
    return Evaluate(
      snapshot,
      tileDefinitions,
      ConversionSandTileRegistry.RegisterDefaults(),
      plantType,
      x,
      y,
      ignoreSelf);
  }

  public static UnderwaterPlantValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    IReadOnlySet<ushort> conversionSandTileTypes,
    ushort plantType,
    int x,
    int y,
    bool ignoreSelf = true)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(conversionSandTileTypes);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile current = snapshot.GetTile(x, y);
    bool valid = ignoreSelf || !current.IsActive;
    for (int offsetY = 0; offsetY < 3; offsetY++)
    {
      int liquidY = y - offsetY;
      WorldTile liquidTile = snapshot.Metadata.IsInside(x, liquidY)
        ? snapshot.GetTile(x, liquidY)
        : default;
      valid &= liquidTile.LiquidAmount > 0 && liquidTile.LiquidType == WaterLiquidType;
    }

    WorldTile support = snapshot.Metadata.IsInside(x, y + 1)
      ? snapshot.GetTile(x, y + 1)
      : default;
    bool supportIsSand = support.IsActive && conversionSandTileTypes.Contains(support.Type);
    valid &= support.IsActive && TileStateQuery.IsSolid(support, tileDefinitions) &&
      (supportIsSand || support.Type == plantType);
    WorldTile plantTile = current;
    valid &= plantTile.WallType == 0 || plantTile.WallType is >= 63 and <= 69 or 80 or 81;

    bool above = snapshot.Metadata.IsInside(x, y - 1) &&
      snapshot.GetTile(x, y - 1).IsActive && snapshot.GetTile(x, y - 1).Type == plantType;
    bool below = snapshot.Metadata.IsInside(x, y + 1) &&
      snapshot.GetTile(x, y + 1).IsActive && snapshot.GetTile(x, y + 1).Type == plantType;
    short frameX = current.FrameX;
    if (above)
    {
      frameX = checked((short)(Math.Clamp(frameX / TileFrameWidth, 1, 7) * TileFrameWidth));
    }
    else if (below)
    {
      frameX = checked((short)(Math.Clamp(frameX / TileFrameWidth, 7, 12) * TileFrameWidth));
    }
    else
    {
      frameX = 0;
    }

    return new UnderwaterPlantValidationResult(
      valid,
      !valid,
      frameX,
      0,
      support.Type,
      supportIsSand);
  }
}
