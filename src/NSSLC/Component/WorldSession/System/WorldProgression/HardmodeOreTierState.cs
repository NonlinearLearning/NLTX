namespace Terraria.WorldProgression.Components;

public readonly record struct HardmodeOreTierState(
  int CobaltTileType,
  int MythrilTileType,
  int AdamantiteTileType)
{
  public static HardmodeOreTierState Uninitialized { get; } = new(-1, -1, -1);
}
