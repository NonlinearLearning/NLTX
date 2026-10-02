namespace Terraria.Player;

public sealed class PlayerWhipCapabilityComponent
{
  public float WhipRangeMultiplier { get; internal set; } = 1f;

  public float WhipUseTimeMultiplier { get; internal set; } = 1f;

  internal void ResetEffects()
  {
    WhipRangeMultiplier = 1f;
    WhipUseTimeMultiplier = 1f;
  }
}
