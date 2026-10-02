namespace Terraria.WorldSession.Runtime;

public readonly record struct RuntimeLimitsDefinition
{
  public int OffLimitBorderTiles { get; }

  public RuntimeLimitsDefinition(int offLimitBorderTiles)
  {
    OffLimitBorderTiles = offLimitBorderTiles < 0
      ? throw new ArgumentOutOfRangeException(nameof(offLimitBorderTiles))
      : offLimitBorderTiles;
  }
}
