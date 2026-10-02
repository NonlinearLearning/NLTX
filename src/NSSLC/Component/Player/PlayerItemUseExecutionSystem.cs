namespace Terraria.Player;

public static class PlayerItemUseExecutionSystem
{
  public enum ItemCheckEntryResult
  {
    Continue,
    ReturnedForCrowdControl,
  }

  public static ItemCheckEntryResult BeginCheck(
    bool isCrowdControlled,
    PlayerItemUseState state)
  {
    state.HasPendingReuse = false;
    if (!isCrowdControlled)
    {
      return ItemCheckEntryResult.Continue;
    }

    state.IsChanneling = false;
    state.AnimationRemainingTicks = 0;
    state.AnimationDurationTicks = 0;
    return ItemCheckEntryResult.ReturnedForCrowdControl;
  }

  public static bool ShouldEnterStartUseBranch(
    in PlayerItemUseStartGateInput input)
  {
    return input.ControlUseItem &&
      input.ReleaseUseItem &&
      input.AnimationRemainingTicks == 0 &&
      input.ItemUseStyle != 0 &&
      !input.HasBufferedSelectionChange;
  }

  public static bool ShouldKeepChanneling(
    in PlayerItemUseChannelContinuationInput input)
  {
    bool shouldKeepChanneling = input.ControlUseItem;
    if (input.MountTypeEightActive)
    {
      shouldKeepChanneling = input.ControlUseItem || input.ControlUseTile;
    }

    if (input.SelectedItemIsKite)
    {
      shouldKeepChanneling = input.ControlUseTile;
    }

    if (input.HasBufferedSelectionChange)
    {
      shouldKeepChanneling = false;
    }

    return shouldKeepChanneling;
  }

  public static PlayerItemUseAnimationStepResult AdvanceAnimationFrame(
    in PlayerItemUseAnimationStepInput input)
  {
    if (input.AnimationRemainingTicks <= 0)
    {
      return new PlayerItemUseAnimationStepResult(
        input.AnimationRemainingTicks,
        input.HasPendingReuse);
    }

    int animationRemainingTicks = input.AnimationRemainingTicks - 1;
    bool hasPendingReuse = input.HasPendingReuse;
    if (animationRemainingTicks == 0 &&
      input.ReuseDelayRemainingTicks == 0 &&
      input.ControlUseItem &&
      input.ReleaseUseItem)
    {
      hasPendingReuse = true;
    }

    return new PlayerItemUseAnimationStepResult(
      animationRemainingTicks,
      hasPendingReuse);
  }

  public static PlayerItemUseReuseDelayResult ApplyReuseDelay(
    in PlayerItemUseReuseDelayInput input)
  {
    return new PlayerItemUseReuseDelayResult(
      AnimationRemainingTicks: input.ReuseDelayRemainingTicks,
      ItemTimeRemainingTicks: input.ReuseDelayRemainingTicks,
      ReuseDelayRemainingTicks: 0);
  }

  public static PlayerItemUseFrameTailResult CompleteFrameTail(
    in PlayerItemUseFrameTailInput input)
  {
    bool shouldTurnItemToAir =
      input.AnimationRemainingTicks == 0 &&
      input.ItemIsAir &&
      input.ItemType != 0;
    bool hasPendingReuse = shouldTurnItemToAir
      ? false
      : input.HasPendingReuse;
    int itemTimeRemainingTicks = input.ItemTimeRemainingTicks > 0
      ? input.ItemTimeRemainingTicks - 1
      : input.ItemTimeRemainingTicks;

    return new PlayerItemUseFrameTailResult(
      ShouldTurnItemToAir: shouldTurnItemToAir,
      HasPendingReuse: hasPendingReuse,
      ReleaseUseItem: !input.ControlUseItem,
      ItemTimeRemainingTicks: itemTimeRemainingTicks);
  }
}
