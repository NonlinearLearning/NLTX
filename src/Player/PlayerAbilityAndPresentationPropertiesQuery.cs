namespace Terraria.Player;

public static class PlayerAbilityAndPresentationPropertiesQuery
{
  public static PlayerAbilityAndPresentationPropertiesSnapshot Evaluate(
    in PlayerAbilityAndPresentationPropertiesInput input)
  {
    bool canUseDownDashAbilities = !input.IsPerformingJumpDownDash;
    bool mountFishronSpecial = EvaluateMountFishronSpecial(input);

    return new PlayerAbilityAndPresentationPropertiesSnapshot(
      input.UnlockedBiomeTorches && input.BiomeTorchPreferenceEnabled,
      input.UnlockedSuperCart && input.EnabledSuperCart,
      (input.RangedDamage / input.RangedMultDamage + input.ArrowDamageAdditiveStack) *
        input.RangedMultDamage * input.ArrowDamage,
      input.RangedDamage * input.BulletDamage,
      input.RangedDamage * input.RocketDamage,
      canUseDownDashAbilities,
      !input.IsMerman && canUseDownDashAbilities,
      input.IsInvisible &&
        input.ItemAnimation == 0 &&
        !input.IsDisplayDollOrInanimate &&
        !input.IsHatRackDoll,
      input.TalkNpc,
      input.IsSitting || input.IsSleeping,
      input.PortalPhysicsRemainingTicks > 0 && !input.MountActive,
      mountFishronSpecial);
  }

  private static bool EvaluateMountFishronSpecial(
    in PlayerAbilityAndPresentationPropertiesInput input)
  {
    bool canUseSpecial = input.Life >= input.MaximumLife / 2 &&
      (!input.IsWet || input.IsLavaWet || input.IsHoneyWet) &&
      !input.IsDripping &&
      !(input.MountFishronSpecialCounter > 0.0f);

    if (!canUseSpecial)
    {
      return true;
    }

    return input.IsRaining && input.IsInPlaceWithWind;
  }
}
