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

LegacyPlayerSlot firstSlot = new(0);
LegacyPlayerSlot secondSlot = new(1);
PlayerInputCommand firstCommand = new(
  Player: firstSlot,
  ControlLeft: true,
  ControlRight: false,
  ControlUp: false,
  ControlDown: true,
  ControlJump: true,
  ControlTorch: false,
  ControlDash: true,
  ControlDownHold: false);
PlayerRawControlInputComponent firstInput = new();

Require(
  PlayerRawControlInputSystem.TryApply(
    firstCommand,
    isTargetActive: true,
    ref firstInput,
    out PlayerInputRejectionReason singleRejection) &&
  singleRejection == PlayerInputRejectionReason.None,
  "A valid active-player command must be accepted.");
Require(
  firstInput.ControlLeft && firstInput.ControlDown && firstInput.ControlJump &&
  firstInput.ControlDash,
  "Accepted commands must project their P10 control facts.");

PlayerRawControlInputComponent secondInput = new()
{
  ControlRight = true,
};
Dictionary<LegacyPlayerSlot, bool> activePlayers = new()
{
  [firstSlot] = true,
  [secondSlot] = false,
};
Dictionary<LegacyPlayerSlot, PlayerRawControlInputComponent> inputComponents = new()
{
  [firstSlot] = secondInput,
  [secondSlot] = new PlayerRawControlInputComponent(),
};
PlayerInputCommand secondCommand = firstCommand with { Player = secondSlot };

Require(
  !PlayerRawControlInputSystem.TryApplyBatch(
    new[] { firstCommand, secondCommand },
    activePlayers,
    inputComponents,
    out PlayerInputRejectionReason batchRejection) &&
  batchRejection == PlayerInputRejectionReason.InactivePlayer,
  "A batch containing an inactive player must be rejected.");
Require(
  inputComponents[firstSlot].ControlRight && !inputComponents[firstSlot].ControlLeft,
  "A rejected batch must not partially update an earlier player.");

activePlayers[secondSlot] = true;
Require(
  PlayerRawControlInputSystem.TryApplyBatch(
    new[] { firstCommand, secondCommand },
    activePlayers,
    inputComponents,
    out PlayerInputRejectionReason acceptedBatchRejection) &&
  acceptedBatchRejection == PlayerInputRejectionReason.None,
  "A valid batch must be accepted.");
Require(
  inputComponents[firstSlot].ControlLeft && inputComponents[secondSlot].ControlJump,
  "A valid batch must commit each player's controls.");

Console.WriteLine(
  "PASS: player input sync projection and raw control commits preserve accepted/rejected state");
