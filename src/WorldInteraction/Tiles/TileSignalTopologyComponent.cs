namespace Terraria.WorldInteraction.Tiles;

public sealed class TileSignalTopologyComponent
{
  public bool HasWire1 { get; internal set; }
  public bool HasWire2 { get; internal set; }
  public bool HasWire3 { get; internal set; }
  public bool HasWire4 { get; internal set; }
  public bool HasActuator { get; internal set; }
  public bool IsActuated { get; internal set; }
}
