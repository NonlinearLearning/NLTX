namespace Terraria.Player;

public sealed class PlayerDamageMitigationInputComponent
{
  public float Endurance { get; internal set; }

  internal void ResetEffects()
  {
    Endurance = 0f;
  }
}
