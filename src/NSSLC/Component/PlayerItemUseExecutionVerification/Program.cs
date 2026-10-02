using Terraria.Player;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

PlayerItemUseState continuingState = new()
{
  AnimationRemainingTicks = 5,
  AnimationDurationTicks = 8,
  UseRemainingTicks = 2,
  UseDurationTicks = 10,
  ReuseDelayRemainingTicks = 3,
  IsChanneling = true,
  HasPendingReuse = true,
};

PlayerItemUseExecutionSystem.ItemCheckEntryResult continueResult =
  PlayerItemUseExecutionSystem.BeginCheck(
    isCrowdControlled: false,
    continuingState);

Require(
  continueResult == PlayerItemUseExecutionSystem.ItemCheckEntryResult.Continue,
  "An uncontrolled player must continue into the ItemCheck body.");
Require(!continuingState.HasPendingReuse, "ItemCheck entry must clear pending reuse first.");
Require(continuingState.IsChanneling, "The non-controlled path must preserve channel state.");
Require(continuingState.AnimationRemainingTicks == 5, "The non-controlled path must preserve animation time.");
Require(continuingState.AnimationDurationTicks == 8, "The non-controlled path must preserve animation maximum.");

PlayerItemUseState controlledState = new()
{
  AnimationRemainingTicks = 5,
  AnimationDurationTicks = 8,
  UseRemainingTicks = 2,
  UseDurationTicks = 10,
  ReuseDelayRemainingTicks = 3,
  IsChanneling = true,
  HasPendingReuse = true,
};

PlayerItemUseExecutionSystem.ItemCheckEntryResult controlledResult =
  PlayerItemUseExecutionSystem.BeginCheck(
    isCrowdControlled: true,
    controlledState);

Require(
  controlledResult == PlayerItemUseExecutionSystem.ItemCheckEntryResult.ReturnedForCrowdControl,
  "Crowd control must return before the rest of ItemCheck executes.");
Require(!controlledState.HasPendingReuse, "Crowd control must clear pending reuse.");
Require(!controlledState.IsChanneling, "Crowd control must stop channeling.");
Require(controlledState.AnimationRemainingTicks == 0, "Crowd control must clear animation time.");
Require(controlledState.AnimationDurationTicks == 0, "Crowd control must clear animation maximum.");
Require(controlledState.UseRemainingTicks == 2, "The entry gate must not alter item-use time.");
Require(controlledState.UseDurationTicks == 10, "The entry gate must not alter item-use duration.");
Require(controlledState.ReuseDelayRemainingTicks == 3, "The entry gate must not alter reuse delay.");

Console.WriteLine(
  "PASS: ItemCheck entry gate preserves Version4 crowd-control early return ordering");
