namespace Terraria.Player;

public sealed class PlayerShimmerStateComponent
{
  public bool Shimmering { get; internal set; }

  public int TimeShimmering { get; internal set; }

  public float ShimmerTransparency { get; internal set; }

  // Stable state extracted from Version4 ShimmerUnstuckHelper; adapter behavior remains external.
  public int ShimmerUnstuckTimeLeft { get; internal set; }

  public bool ShimmerUnstuckProtectionActive { get; internal set; }

  internal void ResetForLifecycle()
  {
    Shimmering = false;
    TimeShimmering = 0;
    ShimmerTransparency = 0f;
    ShimmerUnstuckTimeLeft = 0;
    ShimmerUnstuckProtectionActive = false;
  }
}
