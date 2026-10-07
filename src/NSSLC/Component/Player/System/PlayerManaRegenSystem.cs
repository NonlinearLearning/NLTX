namespace Terraria.Player;

public sealed class PlayerManaRegenSystem
{
  private const int FramesPerMana = 120;

  private readonly PlayerVitalStateComponent _vital;
  private readonly PlayerManaRegenStateComponent _state;
  private readonly PlayerManaRegenModifierComponent _modifiers;

  public PlayerManaRegenSystem(
    PlayerVitalStateComponent vital,
    PlayerManaRegenStateComponent state,
    PlayerManaRegenModifierComponent modifiers)
  {
    ArgumentNullException.ThrowIfNull(vital);
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(modifiers);

    _vital = vital;
    _state = state;
    _modifiers = modifiers;
  }

  public PlayerManaRegenResult Tick(in PlayerManaRegenInput input)
  {
    int maxMana = _vital.StatManaMax2;
    int manaBefore = _vital.StatMana;
    if (maxMana <= 0)
    {
      return PlayerManaRegenResult.Rejected(
        PlayerManaRegenFailureReason.InvalidManaCap,
        manaBefore);
    }

    if (!float.IsFinite(_state.ManaRegenDelay) ||
      !float.IsFinite(_modifiers.ManaRegenDelayBonus))
    {
      return PlayerManaRegenResult.Rejected(
        PlayerManaRegenFailureReason.InvalidRegenState,
        manaBefore);
    }

    _vital.StatMana = Math.Clamp(_vital.StatMana, 0, maxMana);
    float delay = Math.Max(0f, _state.ManaRegenDelay);
    if (delay > 0f)
    {
      delay -= 1f;
      delay -= _modifiers.ManaRegenDelayBonus;
      if (input.IsConsideredStandingStill || input.IsGrappling || input.ManaRegenBuff)
      {
        delay -= 1f;
      }

      if (input.UsedArcaneCrystal)
      {
        delay -= 0.05f;
      }
    }

    if (input.ManaRegenBuff && delay > 20f)
    {
      delay = 20f;
    }

    int manaRegen;
    if (delay <= 0f)
    {
      delay = 0f;
      manaRegen = maxMana / 3 + 1 + _modifiers.ManaRegenBonus;
      if (input.IsConsideredStandingStill || input.IsGrappling || input.ManaRegenBuff)
      {
        manaRegen += maxMana / 3;
      }

      if (input.UsedArcaneCrystal)
      {
        manaRegen += maxMana / 50;
      }

      float manaRatio = (float)_vital.StatMana / maxMana * 0.8f + 0.2f;
      if (input.ManaRegenBuff)
      {
        manaRatio = 1f;
      }

      manaRegen = (int)(manaRegen * manaRatio * 1.15f);
    }
    else
    {
      manaRegen = 0;
    }

    long regenCount = Math.Max(
      0,
      Math.Max(0, (long)_state.ManaRegenCount) + manaRegen);
    while (regenCount >= FramesPerMana)
    {
      regenCount -= FramesPerMana;
      if (_vital.StatMana < maxMana)
      {
        _vital.StatMana++;
      }
    }

    _vital.StatMana = Math.Min(_vital.StatMana, maxMana);
    _state.ManaRegen = manaRegen;
    _state.ManaRegenDelay = delay;
    _state.ManaRegenCount = (int)Math.Min(regenCount, int.MaxValue);
    return new PlayerManaRegenResult(
      Applied: true,
      ManaBefore: manaBefore,
      ManaAfter: _vital.StatMana,
      ManaRegen: manaRegen,
      ManaRegenCountAfter: _state.ManaRegenCount,
      ManaRegenDelayAfter: delay,
      FailureReason: PlayerManaRegenFailureReason.None);
  }
}
