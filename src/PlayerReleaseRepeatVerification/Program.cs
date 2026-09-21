using Terraria.Player;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

PlayerReleaseAndRepeatStateComponent resetState =
  PlayerReleaseAndRepeatSystem.CreateResetState();

Require(resetState.ReleaseJump, "Reset must mark jump as released.");
Require(resetState.ReleaseUp, "Reset must mark up as released.");
Require(resetState.ReleaseUseItem, "Reset must mark item use as released.");
Require(resetState.ReleaseUseTile, "Reset must mark tile use as released.");
Require(resetState.ReleaseLeft, "Reset must mark left as released.");
Require(resetState.ReleaseRight, "Reset must mark right as released.");
Require(resetState.ReleaseDown, "Reset must mark down as released.");
Require(resetState.ReleaseDash, "Reset must mark dash as released.");
Require(
  !resetState.TryKeepingHoveringDown,
  "Reset must clear the downward hover intent.");
Require(
  !resetState.TryKeepingHoveringUp,
  "Reset must clear the upward hover intent.");
Require(resetState.LeftTimer == 0, "Reset must clear the left repeat timer.");
Require(resetState.RightTimer == 0, "Reset must clear the right repeat timer.");

PlayerReleaseAndRepeatStateComponent state = resetState;
PlayerReleaseAndRepeatSystem.Advance(
  new PlayerReleaseAndRepeatInput(
    ControlJump: true,
    ControlUp: true,
    ControlLeft: true,
    ControlRight: false,
    ControlDown: true,
    ControlDash: true,
    ReleaseUseItem: false,
    ReleaseUseTile: true,
    TryKeepingHoveringDown: true,
    TryKeepingHoveringUp: false),
  reset: false,
  ref state);

Require(!state.ReleaseJump, "Held jump must clear the jump release flag.");
Require(!state.ReleaseUp, "Held up must clear the up release flag.");
Require(!state.ReleaseLeft, "Held left must clear the left release flag.");
Require(state.ReleaseRight, "Released right must set the right release flag.");
Require(!state.ReleaseDown, "Held down must clear the down release flag.");
Require(!state.ReleaseDash, "Held dash must clear the dash release flag.");
Require(
  !state.ReleaseUseItem,
  "Item release must preserve the external item-use fact.");
Require(
  state.ReleaseUseTile,
  "Tile release must preserve the external tile-use fact.");
Require(
  state.TryKeepingHoveringDown,
  "Downward hover intent must be copied from the explicit input.");
Require(
  !state.TryKeepingHoveringUp,
  "Upward hover intent must be copied from the explicit input.");
Require(
  state.LeftTimer == PlayerReleaseAndRepeatSystem.DirectionRepeatWindowTicks,
  "A held direction from timer zero must open a full repeat window.");
Require(
  state.RightTimer == PlayerReleaseAndRepeatSystem.DirectionRepeatWindowTicks - 1,
  "A released direction must retain the post-release repeat timer.");

PlayerReleaseAndRepeatSystem.Advance(
  new PlayerReleaseAndRepeatInput(
    ControlJump: false,
    ControlUp: false,
    ControlLeft: true,
    ControlRight: true,
    ControlDown: false,
    ControlDash: false,
    ReleaseUseItem: true,
    ReleaseUseTile: false,
    TryKeepingHoveringDown: false,
    TryKeepingHoveringUp: true),
  reset: false,
  ref state);

Require(state.LeftTimer == 6, "A held left direction must decrement its timer.");
Require(state.RightTimer == 5, "A held right direction must decrement its timer.");
Require(state.ReleaseUseItem, "Item release must be read from the new input.");
Require(!state.ReleaseUseTile, "Tile release must be read from the new input.");

PlayerReleaseAndRepeatStateComponent oneTickRemainingState = resetState;
oneTickRemainingState.LeftTimer = 1;
oneTickRemainingState.RightTimer = 1;
PlayerReleaseAndRepeatSystem.Advance(
  new PlayerReleaseAndRepeatInput(
    ControlJump: true,
    ControlUp: true,
    ControlLeft: true,
    ControlRight: true,
    ControlDown: true,
    ControlDash: true,
    ReleaseUseItem: true,
    ReleaseUseTile: true,
    TryKeepingHoveringDown: false,
    TryKeepingHoveringUp: false),
  reset: false,
  ref oneTickRemainingState);

Require(
  oneTickRemainingState.LeftTimer == 0,
  "A timer at one must reach zero after one held tick.");
Require(
  oneTickRemainingState.RightTimer == 0,
  "A timer at one must reach zero after one held tick.");

PlayerReleaseAndRepeatStateComponent outOfRangeState = resetState;
outOfRangeState.LeftTimer = 99;
outOfRangeState.RightTimer = -1;
PlayerReleaseAndRepeatSystem.Advance(
  new PlayerReleaseAndRepeatInput(
    ControlJump: false,
    ControlUp: false,
    ControlLeft: true,
    ControlRight: true,
    ControlDown: false,
    ControlDash: false,
    ReleaseUseItem: true,
    ReleaseUseTile: true,
    TryKeepingHoveringDown: false,
    TryKeepingHoveringUp: false),
  reset: false,
  ref outOfRangeState);

Require(
  outOfRangeState.LeftTimer == PlayerReleaseAndRepeatSystem.DirectionRepeatWindowTicks - 1,
  "An oversized held timer must be clamped before decrementing.");
Require(
  outOfRangeState.RightTimer == PlayerReleaseAndRepeatSystem.DirectionRepeatWindowTicks,
  "A negative held timer must be clamped before reopening the window.");

PlayerReleaseAndRepeatStateComponent resetAfterInputState = outOfRangeState;
PlayerReleaseAndRepeatSystem.Advance(
  new PlayerReleaseAndRepeatInput(
    ControlJump: true,
    ControlUp: true,
    ControlLeft: true,
    ControlRight: true,
    ControlDown: true,
    ControlDash: true,
    ReleaseUseItem: false,
    ReleaseUseTile: false,
    TryKeepingHoveringDown: true,
    TryKeepingHoveringUp: true),
  reset: true,
  ref resetAfterInputState);

Require(
  resetAfterInputState.Equals(resetState),
  "Reset must discard the input snapshot and restore the complete reset state.");

Console.WriteLine(
  "PASS: release and repeat state preserves Version4 isolated edge and timer semantics");
