using Terraria.Player;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

PlayerInputSyncProjection emptyInput =
  PlayerInputSyncQuery.Evaluate(
    new PlayerInputSyncCacheInput(
      ControlLeft: false,
      ControlRight: false,
      ControlUp: false,
      ControlDown: false,
      ControlJump: false));
Require(!emptyInput.ControlLeft, "The sync projection must preserve left input.");
Require(!emptyInput.ControlRight, "The sync projection must preserve right input.");
Require(!emptyInput.ControlUp, "The sync projection must preserve up input.");
Require(!emptyInput.ControlDown, "The sync projection must preserve down input.");
Require(!emptyInput.ControlJump, "The sync projection must preserve jump input.");
Require(!emptyInput.PressingAnyInput, "Empty input must not report a pressed input.");

PlayerInputSyncProjection jumpInput =
  PlayerInputSyncQuery.Evaluate(
    new PlayerInputSyncCacheInput(
      ControlLeft: false,
      ControlRight: false,
      ControlUp: false,
      ControlDown: false,
      ControlJump: true));
Require(jumpInput.PressingAnyInput, "Jump alone must report a pressed input.");

PlayerInputSyncProjection directionalInput =
  PlayerInputSyncQuery.Evaluate(
    new PlayerInputSyncCacheInput(
      ControlLeft: true,
      ControlRight: false,
      ControlUp: false,
      ControlDown: false,
      ControlJump: false));
Require(directionalInput.PressingAnyInput, "Directional input must report a pressed input.");
Require(
  directionalInput ==
    PlayerInputSyncQuery.Evaluate(
      new PlayerInputSyncCacheInput(
        ControlLeft: true,
        ControlRight: false,
        ControlUp: false,
        ControlDown: false,
        ControlJump: false)),
  "The sync query must be deterministic for identical facts.");

Console.WriteLine("PASS: player input sync projection preserves Version4 cache semantics");
