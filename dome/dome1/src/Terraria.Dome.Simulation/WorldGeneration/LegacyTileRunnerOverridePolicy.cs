namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerOverrideContext(
  int ExistingTileType,
  int TargetTileType,
  bool ExistingTileIsActive,
  bool TargetIsStone,
  bool TargetIsOre,
  bool ExistingTileCanBeCleared,
  bool IsInUndergroundDesert,
  int TileY,
  double WorldSurface,
  int SurfaceRandomOffset,
  bool Overwrite);

public static class LegacyTileRunnerOverridePolicy
{
  public static bool MustPreserveExistingTile(LegacyTileRunnerOverrideContext context)
  {
    if (!context.ExistingTileIsActive || !context.Overwrite)
    {
      return false;
    }

    bool preserve = context.TargetIsStone && context.ExistingTileType != 1;
    if (!context.ExistingTileCanBeCleared)
    {
      preserve = true;
    }

    switch (context.ExistingTileType)
    {
      case 53:
        if (context.TargetTileType == 59 && context.IsInUndergroundDesert)
        {
          preserve = true;
        }

        if (context.TargetTileType == 40 ||
            context.TileY < context.WorldSurface && context.TargetTileType != 59)
        {
          preserve = true;
        }

        break;
      case 45:
      case 147:
      case 189:
      case 190:
      case 196:
      case 460:
      case 717:
      case 718:
      case 719:
        preserve = true;
        break;
      case 396:
      case 397:
        preserve = !context.TargetIsOre;
        break;
      case 1:
        if (context.TargetTileType == 59 &&
            context.TileY < context.WorldSurface + context.SurfaceRandomOffset)
        {
          preserve = true;
        }

        break;
      case 367:
      case 368:
        if (context.TargetTileType == 59)
        {
          preserve = true;
        }

        break;
    }

    return preserve;
  }
}
