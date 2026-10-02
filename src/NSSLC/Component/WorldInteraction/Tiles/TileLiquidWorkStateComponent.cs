namespace Terraria.WorldInteraction.Tiles;

public sealed class TileLiquidWorkStateComponent
{
  public bool IsCheckingLiquid { get; internal set; }

  public uint LastLiquidChangedRevision { get; internal set; }

  public bool ShouldSkipLiquid { get; internal set; }
}
