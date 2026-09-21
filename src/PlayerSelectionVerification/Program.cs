using Terraria.Player;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

PlayerSelectionStateSnapshot ordinarySelection =
  PlayerSelectionQuery.Evaluate(
    new PlayerSelectionStateInput(
      Selected: 3,
      Hotbar: 2,
      Buffered: -1,
      Overridden: -1,
      IsUsingOrReusingItem: false,
      ItemTimeIsZero: true));

Require(
  ordinarySelection.CanChangeSelectedItemImmediately,
  "An idle player with zero item time may change selection immediately.");
Require(ordinarySelection.Selected == 3, "Selected must expose the committed selection.");
Require(ordinarySelection.Hotbar == 2, "Hotbar must expose the hotbar selection.");
Require(!ordinarySelection.HasActiveOverride, "An override of -1 must be inactive.");
Require(!ordinarySelection.HasBufferedChange, "A buffer of -1 must be inactive.");
Require(
  ordinarySelection.LastNonOverridenSelection == 3,
  "Without a buffer or override, the committed selection is last non-overridden.");

PlayerSelectionStateSnapshot overriddenSelection =
  PlayerSelectionQuery.Evaluate(
    new PlayerSelectionStateInput(
      Selected: 3,
      Hotbar: 2,
      Buffered: -1,
      Overridden: 8,
      IsUsingOrReusingItem: false,
      ItemTimeIsZero: true));
Require(overriddenSelection.HasActiveOverride, "A non-negative override must be visible.");
Require(
  overriddenSelection.LastNonOverridenSelection == -1,
  "An active override without a buffer has no non-overridden selection.");

PlayerSelectionStateSnapshot bufferedSelection =
  PlayerSelectionQuery.Evaluate(
    new PlayerSelectionStateInput(
      Selected: 3,
      Hotbar: 2,
      Buffered: 9,
      Overridden: 8,
      IsUsingOrReusingItem: true,
      ItemTimeIsZero: true));
Require(bufferedSelection.HasBufferedChange, "A non-negative buffer must be visible.");
Require(
  bufferedSelection.LastNonOverridenSelection == 9,
  "A buffered selection takes precedence for last non-overridden selection.");
Require(
  !bufferedSelection.CanChangeSelectedItemImmediately,
  "An active item use must block immediate selection changes.");

PlayerSelectionStateSnapshot timedSelection =
  PlayerSelectionQuery.Evaluate(
    new PlayerSelectionStateInput(
      Selected: 3,
      Hotbar: 2,
      Buffered: -1,
      Overridden: -1,
      IsUsingOrReusingItem: false,
      ItemTimeIsZero: false));
Require(
  !timedSelection.CanChangeSelectedItemImmediately,
  "Non-zero item time must block immediate selection changes.");
Require(
  bufferedSelection ==
    PlayerSelectionQuery.Evaluate(
      new PlayerSelectionStateInput(
        Selected: 3,
        Hotbar: 2,
        Buffered: 9,
        Overridden: 8,
        IsUsingOrReusingItem: true,
        ItemTimeIsZero: true)),
  "The selection query must be deterministic for identical facts.");

Console.WriteLine("PASS: player selection query preserves Version4 derived semantics");
