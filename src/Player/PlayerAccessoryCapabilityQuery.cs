namespace Terraria.Player;

public sealed class PlayerAccessoryCapabilityQuery
{
  public PlayerAccessoryCapabilitySnapshot Snapshot(
    PlayerAccessoryStringEffectComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    return new PlayerAccessoryCapabilitySnapshot(
      component.ExtraAccessorySlots,
      component.ExtraAccessory,
      component.TankPet,
      component.TankPetReset,
      component.StringColor,
      component.CounterWeight,
      component.VanityCounterWeight,
      component.MagicString,
      component.YoyoString,
      component.YoyoGlove,
      component.RapidAttackBonus,
      component.StressBall,
      component.StressBallPrevious,
      component.StaffOfRegrowthBonus);
  }

  public int SelectCounterWeightType(
    PlayerAccessoryStringEffectComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    return component.VanityCounterWeight != 0
      ? component.VanityCounterWeight
      : component.CounterWeight;
  }

  public bool HasYoyoCapability(
    PlayerAccessoryStringEffectComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    return component.YoyoGlove ||
      component.CounterWeight > 0 ||
      component.VanityCounterWeight > 0;
  }
}
