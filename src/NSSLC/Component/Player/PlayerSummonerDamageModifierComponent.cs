namespace Terraria.Player;

public sealed class PlayerSummonerDamageModifierComponent
{
  public float MinionDamage { get; internal set; } = 1f;

  public float MinionKb { get; internal set; }

  internal void ResetEffects()
  {
    MinionDamage = 1f;
    MinionKb = 0f;
  }
}
