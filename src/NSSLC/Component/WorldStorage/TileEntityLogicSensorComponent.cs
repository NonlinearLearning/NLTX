namespace Terraria.WorldStorage;

/// <summary>Live Logic Sensor TileEntity capability state.</summary>
/// <remarks>
/// <para>职责：保存存储层逻辑感应器的检查种类、开关和计数数据。</para>
/// <para>拆分来源：Terraria.GameContent.Tile_Entities.TELogicSensor。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.Tile_Entities/TELogicSensor.cs。
/// </para>
/// <para>主要源成员：logicCheck（第 33 行）； On（第 35 行）； CountedData（第 37 行）。</para>
/// </remarks>
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
