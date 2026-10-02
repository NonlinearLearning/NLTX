namespace Terraria.WorldInteraction.TileEntities;

public sealed class LeashedEntityAnchorComponent
{
  public int ItemType { get; internal set; }
  public bool HasItem => ItemType > 0;
}
