namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>A server-captured player position measured in world-tile units.</summary>
public readonly record struct QuickStackPlayerPosition(double TileX, double TileY)
{
  public bool IsValid => double.IsFinite(TileX)
    && double.IsFinite(TileY)
    && TileX is >= 0 and <= int.MaxValue
    && TileY is >= 0 and <= int.MaxValue;
}
