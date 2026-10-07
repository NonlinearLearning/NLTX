namespace Terraria.WorldStorage;

/// <summary>Evaluates liquid Logic Sensor state and its release countdown.</summary>
public static class TileEntityLiquidSensorSystem
{
  // These values are the persisted LogicCheckType byte values.
  private const byte WaterLogicCheck = 4;
  private const byte LavaLogicCheck = 5;
  private const byte HoneyLogicCheck = 6;
  private const byte LiquidLogicCheck = 7;
  private const byte WaterType = 0;
  private const byte LavaType = 1;
  private const byte HoneyType = 2;
  private const int LiquidReleaseDelayTicks = 15;

  public static bool Evaluate(
    byte logicCheck,
    TileCellState tile,
    bool isOn,
    int currentCountedData,
    out int countedData)
  {
    bool hasLiquid = tile.LiquidAmount > 0;
    bool state = logicCheck switch
    {
      WaterLogicCheck => hasLiquid && tile.LiquidType == WaterType,
      LavaLogicCheck => hasLiquid && tile.LiquidType == LavaType,
      HoneyLogicCheck => hasLiquid && tile.LiquidType == HoneyType,
      LiquidLogicCheck => hasLiquid,
      _ => throw new ArgumentOutOfRangeException(
        nameof(logicCheck),
        logicCheck,
        "Only liquid Logic Sensor checks can be evaluated here."),
    };

    countedData = currentCountedData;
    if (!state && isOn)
    {
      if (countedData == 0)
      {
        countedData = LiquidReleaseDelayTicks;
      }
      else if (countedData > 0)
      {
        countedData--;
      }

      state = countedData > 0;
    }

    return state;
  }
}
