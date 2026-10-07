namespace Terraria.WorldStorage;

/// <summary>Live Logic Sensor TileEntity capability state.</summary>
public struct TileEntityLogicSensorComponent
{
  public byte LogicCheck;

  public bool IsOn;

  public int CountedData;

  public TileEntityLogicSensorComponent(byte logicCheck, bool isOn, int countedData)
  {
    LogicCheck = logicCheck;
    IsOn = isOn;
    CountedData = countedData;
  }
}
