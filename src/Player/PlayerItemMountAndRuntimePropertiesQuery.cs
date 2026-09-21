using System.Numerics;

namespace Terraria.Player;

public static class PlayerItemMountAndRuntimePropertiesQuery
{
  private const int BreathingReedItemTypeId = 186;
  private const float SpectatingCameraVerticalOffset = -21.0f;

  public static PlayerItemMountAndRuntimePropertiesSnapshot Evaluate(
    in PlayerItemMountAndRuntimePropertiesInput input)
  {
    return new PlayerItemMountAndRuntimePropertiesSnapshot(
      input.MinionRestTargetPoint != Vector2.Zero,
      input.ItemTime == 0,
      input.ItemAnimation == input.ItemAnimationMax - 1,
      EvaluateUsingOrReusingItem(input),
      input.SceneMetrics,
      EvaluateSpectatingCameraPosition(input),
      input.MountActive &&
        input.MountIsSlime &&
        input.WetSlime > 0 &&
        !input.ControlJump,
      EvaluateBreathingReed(input),
      input.MountActive &&
        (input.MountIsCart || (input.MountCanGrindRails && input.OnTrack)),
      EvaluateMouthPosition(input),
      EvaluateHandPosition(input));
  }

  private static bool EvaluateUsingOrReusingItem(
    in PlayerItemMountAndRuntimePropertiesInput input)
  {
    if (input.ItemAnimation <= 0 &&
      input.ReuseDelay <= 0 &&
      !input.IsChanneling)
    {
      return input.HasPendingItemReuse;
    }

    return true;
  }

  private static Vector2 EvaluateSpectatingCameraPosition(
    in PlayerItemMountAndRuntimePropertiesInput input)
  {
    if (!input.SpectatingCameraTarget.HasValue)
    {
      return input.Position;
    }

    PlayerSpectatingCameraTargetSnapshot target = input.SpectatingCameraTarget.Value;
    return target.Bottom +
      new Vector2(0.0f, target.GfxOffY + SpectatingCameraVerticalOffset) +
      target.NetOffset;
  }

  private static bool EvaluateBreathingReed(
    in PlayerItemMountAndRuntimePropertiesInput input)
  {
    if (input.SelectedItemTypeId != BreathingReedItemTypeId)
    {
      return false;
    }

    return !input.MountActive || input.MountAllowsHeldItems;
  }

  private static Vector2? EvaluateMouthPosition(
    in PlayerItemMountAndRuntimePropertiesInput input)
  {
    return input.MountActive && input.MouthPositionOverride.HasValue
      ? input.MouthPositionOverride
      : input.MouthPositionFallback;
  }

  private static Vector2? EvaluateHandPosition(
    in PlayerItemMountAndRuntimePropertiesInput input)
  {
    return input.MountActive && input.HandPositionOverride.HasValue
      ? input.HandPositionOverride
      : input.HandPositionFallback;
  }
}
