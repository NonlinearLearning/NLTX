using System;

namespace MinimalImplementation.Server.Player;

public readonly record struct PlayerVitals(
  short Life,
  short MaximumLife,
  short Mana,
  short MaximumMana)
{
  public const short MinimumMaximumLife = 20;

  public static PlayerVitals Initial => new(100, 100, 20, 20);

  public bool IsDead => Life <= 0;

  public int GetEffectiveMaximumLife(PlayerBuffs buffs) {
    ArgumentNullException.ThrowIfNull(buffs);
    const ushort lifeforceBuffType = 113;
    // Version4 ResetEffects / UpdateBuffs rounds the Lifeforce bonus down to whole hearts.
    return MaximumLife + (buffs.HasBuff(lifeforceBuffType) ? MaximumLife / 5 / 20 * 20 : 0);
  }

  public PlayerVitals WithLifeUpload(short life, short maximumLife)
  {
    return this with
    {
      Life = life,
      MaximumLife = Math.Max(MinimumMaximumLife, maximumLife)
    };
  }

  public PlayerVitals WithSpawnLife(PlayerBuffs buffs, bool pvpDeath) {
    ArgumentNullException.ThrowIfNull(buffs);
    if (pvpDeath) {
      return this with { Life = MaximumLife };
    }

    if (Life > 0) {
      return this;
    }

    int maximumLife = Math.Max(MaximumLife, GetEffectiveMaximumLife(buffs));
    return this with { Life = checked((short)Math.Max(100, maximumLife / 2)) };
  }

  public PlayerVitals WithHeal(int healAmount, int effectiveMaximumLife)
  {
    int maximumLife = Math.Clamp(effectiveMaximumLife, 0, short.MaxValue);
    int healedLife = Math.Min((int)Life + healAmount, maximumLife);
    return this with { Life = (short)Math.Max(healedLife, short.MinValue) };
  }

  public PlayerVitals WithManaUpload(short mana, short maximumMana) {
    return this with {
      Mana = mana,
      MaximumMana = maximumMana
    };
  }
}
