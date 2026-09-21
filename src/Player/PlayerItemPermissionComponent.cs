namespace Terraria.Player;

public sealed class PlayerItemPermissionComponent
{
  public bool IsAllowedToHoldItems { get; internal set; } = true;

  public bool AutoPaint { get; internal set; }

  public bool AutoActuator { get; internal set; }

  internal void ResetEffects()
  {
    IsAllowedToHoldItems = true;
    AutoPaint = false;
    AutoActuator = false;
  }
}
