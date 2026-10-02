namespace Terraria.Player;

public sealed class PlayerBuildAndMiningSpeedComponent
{
  public float PickSpeed { get; internal set; } = 1f;

  public float WallSpeed { get; internal set; } = 1f;

  public float TileSpeed { get; internal set; } = 1f;

  internal void ResetEffects()
  {
    PickSpeed = 1f;
    WallSpeed = 1f;
    TileSpeed = 1f;
  }
}
