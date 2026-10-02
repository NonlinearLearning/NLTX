namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class GameplayEventFlagsComponent
{
  public bool DisableDontStarveDarknessDamage { get; private set; }

  public void SetDisableDontStarveDarknessDamage(bool disabled)
  {
    DisableDontStarveDarknessDamage = disabled;
  }
}
