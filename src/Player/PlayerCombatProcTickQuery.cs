namespace Terraria.Player;

public sealed class PlayerCombatProcTickQuery
{
  private const float GhostDamagePerTick = 6.6666665f;
  private const int InfernoCounterLimit = 180;
  private const float ExpertLifeStealRecovery = 0.5f;
  private const float NormalLifeStealRecovery = 0.6f;
  private const float ExpertLifeStealLimit = 70f;
  private const float NormalLifeStealLimit = 80f;

  public PlayerCombatProcTickResult Advance(in PlayerCombatProcTickInput input)
  {
    float ghostDmg = MathF.Max(
      0f,
      input.GhostDmg - GhostDamagePerTick);

    float lifeStealLimit = input.ExpertMode
      ? ExpertLifeStealLimit
      : NormalLifeStealLimit;
    float recovery = input.ExpertMode
      ? ExpertLifeStealRecovery
      : NormalLifeStealRecovery;
    float lifeSteal = MathF.Min(
      lifeStealLimit,
      MathF.Max(0f, input.LifeSteal) + recovery);

    int eocDash = input.EocDash;
    int eocHit = input.EocHit;
    if (eocDash > 0)
    {
      eocDash--;
      if (eocDash == 0)
      {
        eocHit = -1;
      }
    }

    int infernoCounter = input.InfernoCounter + 1;
    if (infernoCounter >= InfernoCounterLimit)
    {
      infernoCounter = 0;
    }

    return new PlayerCombatProcTickResult(
      ghostDmg,
      lifeSteal,
      eocDash,
      eocHit,
      infernoCounter,
      Decrement(input.StarCloakCooldown),
      Decrement(input.TitaniumStormCooldown),
      Decrement(input.PetalTimer),
      Decrement(input.BoneGloveTimer));
  }

  private static int Decrement(int value)
  {
    return Math.Max(0, value - 1);
  }
}
