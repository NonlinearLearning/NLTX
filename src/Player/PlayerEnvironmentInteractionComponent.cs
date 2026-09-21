namespace Terraria.Player;

public sealed class PlayerEnvironmentInteractionComponent
{
  public bool ShimmerImmune { get; internal set; }

  public float GravDir { get; internal set; } = 1f;

  internal void ResetEffects()
  {
    ShimmerImmune = false;
  }

  internal void ResetForLifecycle()
  {
    ShimmerImmune = false;
    GravDir = 1f;
  }
}
