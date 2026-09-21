namespace Terraria.Player;

public sealed class PlayerAccessoryEffectRebuildSystem
{
  private const float RapidAttackBonusDecay = 0.005f;

  public void Rebuild(
    PlayerAccessoryStringEffectComponent component,
    in PlayerAccessoryEffectRebuildInput input)
  {
    ArgumentNullException.ThrowIfNull(component);

    component.ExtraAccessory = input.ExtraAccessory;
    component.TankPet = input.TankPet;
    component.StringColor = input.StringColor;
    component.CounterWeight = input.CounterWeight;
    component.VanityCounterWeight = input.VanityCounterWeight;
    component.MagicString = input.MagicString;
    component.YoyoString = input.YoyoString;
    component.YoyoGlove = input.YoyoGlove;
    component.RapidAttackBonus = NormalizeRapidAttackBonus(input.RapidAttackBonus);
    component.StressBall = input.StressBall;
    component.StaffOfRegrowthBonus = input.StaffOfRegrowthBonus;
  }

  public void ResetForTick(
    PlayerAccessoryStringEffectComponent component,
    bool expertMode,
    bool gameMenu)
  {
    ArgumentNullException.ThrowIfNull(component);

    component.RapidAttackBonus = MathF.Max(
      0f,
      component.RapidAttackBonus - RapidAttackBonusDecay);
    component.ExtraAccessorySlots = component.ExtraAccessory &&
      (expertMode || gameMenu)
      ? 1
      : 0;

    component.StressBall = false;
    component.StaffOfRegrowthBonus = false;
    component.YoyoGlove = false;
    component.MagicString = false;
    component.CounterWeight = 0;
    component.VanityCounterWeight = 0;
    component.StringColor = 0;
    component.YoyoString = false;
  }

  public bool ConsumeStressBallEdge(
    PlayerAccessoryStringEffectComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    if (component.StressBall == component.StressBallPrevious)
    {
      return false;
    }

    component.StressBallPrevious = component.StressBall;
    return true;
  }

  public bool AdvanceTankPetReset(
    PlayerAccessoryStringEffectComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    if (component.TankPet < 0)
    {
      return false;
    }

    if (!component.TankPetReset)
    {
      component.TankPetReset = true;
      return false;
    }

    component.TankPet = -1;
    return true;
  }

  public void ResetForSpawn(PlayerAccessoryStringEffectComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    component.ExtraAccessorySlots = 2;
    component.ExtraAccessory = false;
    component.TankPet = -1;
    component.TankPetReset = false;
    component.StringColor = 0;
    component.CounterWeight = 0;
    component.VanityCounterWeight = 0;
    component.MagicString = false;
    component.YoyoString = false;
    component.YoyoGlove = false;
    component.RapidAttackBonus = 0f;
    component.StressBall = false;
    component.StressBallPrevious = false;
    component.StaffOfRegrowthBonus = false;
  }

  private static float NormalizeRapidAttackBonus(float value)
  {
    return float.IsFinite(value) && value > 0f ? value : 0f;
  }
}
