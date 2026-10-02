namespace Terraria.WorldInteraction.TileEntities;

public sealed class LogicSensorComponent
{
  public LogicCheckType CheckType { get; internal set; }
  public bool IsOn { get; internal set; }
  public int CountedData { get; internal set; }
}
