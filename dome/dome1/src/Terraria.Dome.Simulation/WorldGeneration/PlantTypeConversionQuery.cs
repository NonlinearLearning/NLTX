namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PlantTypeConversionQuery
{
  private const int NormalGrassTileType = 2;
  private const int CorruptGrassTileType = 23;
  private const int JungleGrassTileType = 60;
  private const int MushroomGrassTileType = 70;
  private const int CrimsonGrassTileType = 199;
  private const int HallowedGrassTileType = 477;
  private const int AshGrassTileType = 633;
  private const int CorruptGrassAlternateTileType = 661;
  private const int CrimsonGrassAlternateTileType = 662;
  private const short LargePlantFrameX = 162;
  private const short GrassPlantFrameX = 126;
  private const short MushroomPlantFrameX = 144;
  private const short CrimsonMushroomPlantFrameX = 270;

  public static PlantTypeConversionResult Evaluate(int tileType, short frameX, int supportTileType)
  {
    bool isMushroom = IsMushroom(tileType, frameX);
    short convertedFrameX = NormalizeFrameX(tileType, frameX, supportTileType);
    int convertedTileType = ConvertTileType(tileType, ref convertedFrameX, supportTileType);
    return new PlantTypeConversionResult(convertedTileType, convertedFrameX, isMushroom);
  }

  public static bool IsBadTypeMatch(int supportTileType, int tileType)
  {
    return tileType switch
    {
      3 or 73 => supportTileType is not (NormalGrassTileType or HallowedGrassTileType or 78 or
        380 or 579),
      24 => supportTileType is not (CorruptGrassTileType or CorruptGrassAlternateTileType),
      61 or 74 => supportTileType is not (JungleGrassTileType or 226),
      71 => supportTileType != MushroomGrassTileType,
      110 or 113 => supportTileType is not (109 or 492),
      201 => supportTileType is not (CrimsonGrassTileType or CrimsonGrassAlternateTileType),
      637 => supportTileType != AshGrassTileType,
      _ => false
    };
  }

  private static int ConvertTileType(int tileType, ref short frameX, int supportTileType)
  {
    switch (supportTileType)
    {
      case CorruptGrassTileType:
      case CorruptGrassAlternateTileType:
        if (frameX >= LargePlantFrameX)
        {
          frameX = GrassPlantFrameX;
        }

        return 24;
      case NormalGrassTileType:
      case HallowedGrassTileType:
        return tileType == 113 ? 73 : 3;
      case 109:
      case 492:
        return tileType == 73 ? 113 : 110;
      case CrimsonGrassTileType:
      case CrimsonGrassAlternateTileType:
        return 201;
      case JungleGrassTileType:
      case 226:
        while (frameX > GrassPlantFrameX)
        {
          frameX -= GrassPlantFrameX;
        }

        return 61;
      case MushroomGrassTileType:
        while (frameX > 72)
        {
          frameX -= 72;
        }

        return 71;
      default:
        return tileType;
    }
  }

  private static bool IsMushroom(int tileType, short frameX)
  {
    return tileType switch
    {
      3 or 24 or 110 => frameX == MushroomPlantFrameX,
      201 => frameX == CrimsonMushroomPlantFrameX,
      _ => false
    };
  }

  private static short NormalizeFrameX(int tileType, short frameX, int supportTileType)
  {
    if ((tileType is 3 or 73) &&
        supportTileType is not (NormalGrassTileType or HallowedGrassTileType) &&
        frameX >= LargePlantFrameX)
    {
      return GrassPlantFrameX;
    }

    if (tileType == 74 && supportTileType is not (JungleGrassTileType or 226) &&
        frameX >= LargePlantFrameX)
    {
      return GrassPlantFrameX;
    }

    return frameX;
  }
}
