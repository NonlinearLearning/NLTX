namespace Terraria.Player;

public sealed class PlayerRegenDelayStateComponent
{
  public float MaxRegenDelay { get; internal set; }

  internal void ResetForLifecycle()
  {
    MaxRegenDelay = 0f;
  }
}
