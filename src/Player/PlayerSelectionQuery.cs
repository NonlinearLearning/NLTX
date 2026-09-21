namespace Terraria.Player;

public static class PlayerSelectionQuery
{
  public static PlayerSelectionStateSnapshot Evaluate(
    in PlayerSelectionStateInput input)
  {
    bool hasActiveOverride = input.Overridden >= 0;
    bool hasBufferedChange = input.Buffered >= 0;
    int lastNonOverridenSelection = hasBufferedChange
      ? input.Buffered
      : hasActiveOverride
        ? -1
        : input.Selected;

    return new PlayerSelectionStateSnapshot(
      input.Selected,
      input.Hotbar,
      input.Buffered,
      input.Overridden,
      !input.IsUsingOrReusingItem && input.ItemTimeIsZero,
      hasActiveOverride,
      hasBufferedChange,
      lastNonOverridenSelection);
  }
}
