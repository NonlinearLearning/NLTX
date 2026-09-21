namespace Terraria.Player;

public sealed class PlayerMovementSpeedModifierComponent
{
  public float MoveSpeed { get; internal set; } = 1f;

  internal void ResetEffects()
  {
    MoveSpeed = 1f;
  }
}
