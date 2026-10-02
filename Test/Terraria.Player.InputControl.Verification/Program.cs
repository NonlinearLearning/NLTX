using Terraria.Player;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

var state = new PlayerInteractionLockStateComponent();
PlayerTileInteractionReleaseSystem.Advance(default, state);
Assert(state.ReleaseUseTile, "A clear tile frame must expose the released edge.");

PlayerTileInteractionReleaseSystem.ApplyGamepadTileReleaseLock(state);
Assert(!state.ReleaseUseTile && state.LockTileInteractionsTimer == 3,
  "Applying the gamepad lock must clear release and set its three-tick timer.");

for (int expectedTimer = 2; expectedTimer >= 0; expectedTimer--)
{
  PlayerTileInteractionReleaseSystem.Advance(default, state);
  Assert(!state.ReleaseUseTile && state.LockTileInteractionsTimer == expectedTimer,
    "The lock must suppress release while decrementing once per frame.");
}

PlayerTileInteractionReleaseSystem.Advance(default, state);
Assert(state.ReleaseUseTile && state.LockTileInteractionsTimer == 0,
  "Release becomes available on the frame after the lock timer reaches zero.");

PlayerTileInteractionReleaseSystem.Advance(
  new PlayerTileInteractionReleaseInput(TileInteractAttempted: true, MouseInterface: false),
  state);
Assert(!state.ReleaseUseTile, "A tile interaction attempt must suppress release.");

PlayerTileInteractionReleaseSystem.Advance(
  new PlayerTileInteractionReleaseInput(TileInteractAttempted: false, MouseInterface: true),
  state);
Assert(!state.ReleaseUseTile, "The mouse interface must suppress release for the frame.");

PlayerTileInteractionReleaseSystem.Advance(default, state);
Assert(state.ReleaseUseTile, "Release resumes after tile and UI suppression clear.");

PlayerChannelCancellationExpectation expectation =
  PlayerChannelCancellationAdapter.Begin(projectileType: 42);
Assert(
  expectation.ProjectileTypeExpected == 42 && expectation.ProjectileIndexExpected == 0,
  "Beginning an item channel must record its projectile type and reset the expected index.");

PlayerChannelCancellationExpectation trackedExpectation =
  PlayerChannelCancellationAdapter.Track(
    expectation,
    new PlayerChannelCancellationProjectileSnapshot(
      ProjectileType: 42,
      ProjectileIndex: 9,
      AiStyle: 0,
      Ai0: 0f));
Assert(
  trackedExpectation.ProjectileIndexExpected == 9,
  "A projectile of the expected type must update the expected index.");

PlayerChannelCancellationExpectation unchangedExpectation =
  PlayerChannelCancellationAdapter.Track(
    expectation,
    new PlayerChannelCancellationProjectileSnapshot(
      ProjectileType: 7,
      ProjectileIndex: 9,
      AiStyle: 0,
      Ai0: 0f));
Assert(
  unchangedExpectation == expectation,
  "An unrelated projectile must leave the expectation unchanged.");

Assert(
  PlayerChannelCancellationAdapter.Matches(
    trackedExpectation,
    new PlayerChannelCancellationProjectileSnapshot(
      ProjectileType: 42,
      ProjectileIndex: 9,
      AiStyle: 0,
      Ai0: 0f)),
  "The expected projectile type and index must match.");
Assert(
  PlayerChannelCancellationAdapter.Matches(
    trackedExpectation,
    new PlayerChannelCancellationProjectileSnapshot(
      ProjectileType: 7,
      ProjectileIndex: 9,
      AiStyle: 99,
      Ai0: -3f)),
  "The Version4 special channel marker must match any expectation.");
Assert(
  PlayerChannelCancellationAdapter.Reset() == default,
  "Starting a channel without an item must clear the projectile expectation.");

PlayerItemUseStartGateInput startUseInput = new(
  ControlUseItem: true,
  ReleaseUseItem: true,
  AnimationRemainingTicks: 0,
  ItemUseStyle: 1,
  HasBufferedSelectionChange: false);
Assert(
  PlayerItemUseExecutionSystem.ShouldEnterStartUseBranch(in startUseInput),
  "The Version4 start-use branch must accept a released, idle usable item without a buffered selection change.");
Assert(
  !PlayerItemUseExecutionSystem.ShouldEnterStartUseBranch(
    startUseInput with { ControlUseItem = false }),
  "The start-use branch must require item-use control.");
Assert(
  !PlayerItemUseExecutionSystem.ShouldEnterStartUseBranch(
    startUseInput with { ReleaseUseItem = false }),
  "The start-use branch must require the item-use release edge.");
Assert(
  !PlayerItemUseExecutionSystem.ShouldEnterStartUseBranch(
    startUseInput with { AnimationRemainingTicks = 1 }),
  "The start-use branch must require zero animation ticks.");
Assert(
  !PlayerItemUseExecutionSystem.ShouldEnterStartUseBranch(
    startUseInput with { ItemUseStyle = 0 }),
  "The start-use branch must reject items without a use style.");
Assert(
  !PlayerItemUseExecutionSystem.ShouldEnterStartUseBranch(
    startUseInput with { HasBufferedSelectionChange = true }),
  "A buffered selection change must suppress the start-use branch.");

PlayerItemUseChannelContinuationInput itemControlKeepsChannel = new(
  ControlUseItem: true,
  ControlUseTile: false,
  MountTypeEightActive: false,
  SelectedItemIsKite: false,
  HasBufferedSelectionChange: false);
Assert(
  PlayerItemUseExecutionSystem.ShouldKeepChanneling(in itemControlKeepsChannel),
  "Item-use control must preserve channeling without special mount or item rules.");

PlayerItemUseChannelContinuationInput releasedControlsEndChannel = new(
  ControlUseItem: false,
  ControlUseTile: false,
  MountTypeEightActive: false,
  SelectedItemIsKite: false,
  HasBufferedSelectionChange: false);
Assert(
  !PlayerItemUseExecutionSystem.ShouldKeepChanneling(in releasedControlsEndChannel),
  "Released controls must stop channeling without special mount or item rules.");

PlayerItemUseChannelContinuationInput tileControlKeepsMountedChannel = new(
  ControlUseItem: false,
  ControlUseTile: true,
  MountTypeEightActive: true,
  SelectedItemIsKite: false,
  HasBufferedSelectionChange: false);
Assert(
  PlayerItemUseExecutionSystem.ShouldKeepChanneling(in tileControlKeepsMountedChannel),
  "Type 8 mounts must keep channeling while either item or tile control is held.");

PlayerItemUseChannelContinuationInput kiteOverridesItemControl = new(
  ControlUseItem: true,
  ControlUseTile: false,
  MountTypeEightActive: true,
  SelectedItemIsKite: true,
  HasBufferedSelectionChange: false);
Assert(
  !PlayerItemUseExecutionSystem.ShouldKeepChanneling(in kiteOverridesItemControl),
  "Kite logic must override mount logic and require tile control.");

PlayerItemUseChannelContinuationInput kiteTileControlKeepsChannel =
  kiteOverridesItemControl with { ControlUseTile = true };
Assert(
  PlayerItemUseExecutionSystem.ShouldKeepChanneling(in kiteTileControlKeepsChannel),
  "Tile control must preserve channeling for a kite item.");

PlayerItemUseChannelContinuationInput bufferedChangeStopsChannel = new(
  ControlUseItem: true,
  ControlUseTile: true,
  MountTypeEightActive: true,
  SelectedItemIsKite: false,
  HasBufferedSelectionChange: true);
Assert(
  !PlayerItemUseExecutionSystem.ShouldKeepChanneling(in bufferedChangeStopsChannel),
  "A buffered selection change must override held controls and stop channeling.");

PlayerItemUseAnimationStepResult animationContinues =
  PlayerItemUseExecutionSystem.AdvanceAnimationFrame(
    new PlayerItemUseAnimationStepInput(
      AnimationRemainingTicks: 2,
      ReuseDelayRemainingTicks: 0,
      ControlUseItem: true,
      ReleaseUseItem: true,
      HasPendingReuse: false));
Assert(
  animationContinues.AnimationRemainingTicks == 1 && !animationContinues.HasPendingReuse,
  "An active animation must decrement once without scheduling reuse before it ends.");

PlayerItemUseAnimationStepResult animationEndsWithReuse =
  PlayerItemUseExecutionSystem.AdvanceAnimationFrame(
    new PlayerItemUseAnimationStepInput(
      AnimationRemainingTicks: 1,
      ReuseDelayRemainingTicks: 0,
      ControlUseItem: true,
      ReleaseUseItem: true,
      HasPendingReuse: false));
Assert(
  animationEndsWithReuse.AnimationRemainingTicks == 0 && animationEndsWithReuse.HasPendingReuse,
  "Ending animation with zero reuse delay and both controls must schedule pending reuse.");

PlayerItemUseAnimationStepResult animationEndsWithReuseDelay =
  PlayerItemUseExecutionSystem.AdvanceAnimationFrame(
    new PlayerItemUseAnimationStepInput(
      AnimationRemainingTicks: 1,
      ReuseDelayRemainingTicks: 1,
      ControlUseItem: true,
      ReleaseUseItem: true,
      HasPendingReuse: false));
Assert(
  !animationEndsWithReuseDelay.HasPendingReuse,
  "A nonzero reuse delay must prevent pending reuse from being scheduled.");

PlayerItemUseAnimationStepResult animationEndsWithoutReleasedInput =
  PlayerItemUseExecutionSystem.AdvanceAnimationFrame(
    new PlayerItemUseAnimationStepInput(
      AnimationRemainingTicks: 1,
      ReuseDelayRemainingTicks: 0,
      ControlUseItem: true,
      ReleaseUseItem: false,
      HasPendingReuse: false));
Assert(
  !animationEndsWithoutReleasedInput.HasPendingReuse,
  "Pending reuse must require both item-use controls at the animation end.");

PlayerItemUseReuseDelayResult reuseDelayStep =
  PlayerItemUseExecutionSystem.ApplyReuseDelay(
    new PlayerItemUseReuseDelayInput(ReuseDelayRemainingTicks: 5));
Assert(
  reuseDelayStep.AnimationRemainingTicks == 5 &&
    reuseDelayStep.ItemTimeRemainingTicks == 5 &&
    reuseDelayStep.ReuseDelayRemainingTicks == 0,
  "Applying reuse delay must set animation and item time from the delay and then clear it.");

PlayerItemUseAnimationStepResult inactiveAnimationPreservesReuse =
  PlayerItemUseExecutionSystem.AdvanceAnimationFrame(
    new PlayerItemUseAnimationStepInput(
      AnimationRemainingTicks: 0,
      ReuseDelayRemainingTicks: 0,
      ControlUseItem: true,
      ReleaseUseItem: true,
      HasPendingReuse: true));
Assert(
  inactiveAnimationPreservesReuse.AnimationRemainingTicks == 0 &&
    inactiveAnimationPreservesReuse.HasPendingReuse,
  "The animation step must not rewrite pending reuse when animation is inactive.");

PlayerItemUseFrameTailResult airItemFrameTail =
  PlayerItemUseExecutionSystem.CompleteFrameTail(
    new PlayerItemUseFrameTailInput(
      AnimationRemainingTicks: 0,
      ItemIsAir: true,
      ItemType: 42,
      HasPendingReuse: true,
      ControlUseItem: false,
      ItemTimeRemainingTicks: 2));
Assert(
  airItemFrameTail.ShouldTurnItemToAir &&
    !airItemFrameTail.HasPendingReuse &&
    airItemFrameTail.ReleaseUseItem &&
    airItemFrameTail.ItemTimeRemainingTicks == 1,
  "An empty non-default item at animation end must request cleanup, clear pending reuse, update release, and decrement item time.");

PlayerItemUseFrameTailResult defaultAirItemFrameTail =
  PlayerItemUseExecutionSystem.CompleteFrameTail(
    new PlayerItemUseFrameTailInput(
      AnimationRemainingTicks: 0,
      ItemIsAir: true,
      ItemType: 0,
      HasPendingReuse: true,
      ControlUseItem: true,
      ItemTimeRemainingTicks: 0));
Assert(
  !defaultAirItemFrameTail.ShouldTurnItemToAir &&
    defaultAirItemFrameTail.HasPendingReuse &&
    !defaultAirItemFrameTail.ReleaseUseItem &&
    defaultAirItemFrameTail.ItemTimeRemainingTicks == 0,
  "The default item must not request cleanup, release follows control, and zero item time remains zero.");

PlayerItemUseFrameTailResult occupiedItemFrameTail =
  PlayerItemUseExecutionSystem.CompleteFrameTail(
    new PlayerItemUseFrameTailInput(
      AnimationRemainingTicks: 0,
      ItemIsAir: false,
      ItemType: 42,
      HasPendingReuse: true,
      ControlUseItem: false,
      ItemTimeRemainingTicks: 1));
Assert(
  !occupiedItemFrameTail.ShouldTurnItemToAir &&
    occupiedItemFrameTail.HasPendingReuse &&
    occupiedItemFrameTail.ReleaseUseItem &&
    occupiedItemFrameTail.ItemTimeRemainingTicks == 0,
  "A non-empty item must retain pending reuse without requesting cleanup.");

PlayerItemUseFrameTailResult animatingAirItemFrameTail =
  PlayerItemUseExecutionSystem.CompleteFrameTail(
    new PlayerItemUseFrameTailInput(
      AnimationRemainingTicks: 1,
      ItemIsAir: true,
      ItemType: 42,
      HasPendingReuse: true,
      ControlUseItem: true,
      ItemTimeRemainingTicks: -1));
Assert(
  !animatingAirItemFrameTail.ShouldTurnItemToAir &&
    animatingAirItemFrameTail.HasPendingReuse &&
    !animatingAirItemFrameTail.ReleaseUseItem &&
    animatingAirItemFrameTail.ItemTimeRemainingTicks == -1,
  "Air-item cleanup waits for animation end and nonpositive item time remains unchanged.");

PlayerSetMatchResult headMaleWithoutSpecialMount =
  PlayerSetMatchQuery.Evaluate(
    new PlayerSetMatchInput(
      Head: 201,
      Body: 0,
      Legs: 0,
      ArmorSlotRequested: 0,
      Male: true,
      MountActive: false,
      MountType: 0,
      SomethingSpecial: false));
Assert(
  headMaleWithoutSpecialMount.MatchedSlot == 201 &&
    headMaleWithoutSpecialMount.Matched &&
    !headMaleWithoutSpecialMount.SomethingSpecial,
  "Head 201 must retain the male slot when the special mount is inactive.");

PlayerSetMatchResult headFemaleOnSpecialMount =
  PlayerSetMatchQuery.Evaluate(
    new PlayerSetMatchInput(
      Head: 201,
      Body: 0,
      Legs: 0,
      ArmorSlotRequested: 0,
      Male: false,
      MountActive: true,
      MountType: 54,
      SomethingSpecial: false));
Assert(
  headFemaleOnSpecialMount.MatchedSlot == 201,
  "Head 201 must retain slot 201 on mount type 54 regardless of gender.");

PlayerSetMatchResult body166 =
  PlayerSetMatchQuery.Evaluate(
    new PlayerSetMatchInput(
      Head: 0,
      Body: 166,
      Legs: 0,
      ArmorSlotRequested: 1,
      Male: true,
      MountActive: false,
      MountType: 0,
      SomethingSpecial: true));
Assert(
  body166.MatchedSlot == 119 &&
    body166.Matched &&
    !body166.SomethingSpecial,
  "Body 166 must map the male body and clear somethingSpecial on a match.");

PlayerSetMatchResult body81WithDefaultLegs =
  PlayerSetMatchQuery.Evaluate(
    new PlayerSetMatchInput(
      Head: 0,
      Body: 81,
      Legs: -1,
      ArmorSlotRequested: 1,
      Male: true,
      MountActive: false,
      MountType: 0,
      SomethingSpecial: false));
Assert(
  body81WithDefaultLegs.MatchedSlot == 169,
  "Body 81 must map only when the requested legs slot is -1 or 0.");

PlayerSetMatchResult body81WithOccupiedLegs =
  PlayerSetMatchQuery.Evaluate(
    new PlayerSetMatchInput(
      Head: 0,
      Body: 81,
      Legs: 1,
      ArmorSlotRequested: 1,
      Male: true,
      MountActive: false,
      MountType: 0,
      SomethingSpecial: true));
Assert(
  !body81WithOccupiedLegs.Matched &&
    body81WithOccupiedLegs.MatchedSlot == -1 &&
    body81WithOccupiedLegs.SomethingSpecial,
  "Body 81 must remain unmatched with occupied legs and preserve the input special flag.");

PlayerSetMatchResult maleLegs =
  PlayerSetMatchQuery.Evaluate(
    new PlayerSetMatchInput(
      Head: 0,
      Body: 0,
      Legs: 83,
      ArmorSlotRequested: 2,
      Male: true,
      MountActive: false,
      MountType: 0,
      SomethingSpecial: false));
Assert(
  maleLegs.MatchedSlot == 117,
  "Legs 83 must map to the male slot.");

PlayerSetMatchResult femaleLegs =
  PlayerSetMatchQuery.Evaluate(
    new PlayerSetMatchInput(
      Head: 0,
      Body: 0,
      Legs: 83,
      ArmorSlotRequested: 2,
      Male: false,
      MountActive: false,
      MountType: 0,
      SomethingSpecial: false));
Assert(
  !femaleLegs.Matched && femaleLegs.MatchedSlot == -1,
  "Legs 83 must remain unmatched for the female mapping.");

PlayerSetMatchCompositionResult playerFrameMatch =
  PlayerSetMatchCompositionQuery.Evaluate(
    new PlayerSetMatchCompositionInput(
      Head: 201,
      Body: 81,
      Legs: -1,
      Male: false,
      MountActive: false,
      MountType: 0));
Assert(
  playerFrameMatch.Head == 202 &&
    playerFrameMatch.Body == 81 &&
    playerFrameMatch.Legs == 169 &&
    playerFrameMatch.WearsRobe &&
    !playerFrameMatch.SomethingSpecial,
  "PlayerFrame matching must pass body-adjusted legs into the ordered legs and head matches while keeping both ref outputs distinct.");

PlayerPacket13RouteResult ignoredSelfEcho =
  PlayerPacket13RouteQuery.Evaluate(
    new PlayerPacket13RouteInput(
      DeclaredPlayerSlot: 7,
      LocalPlayerSlot: 7,
      IsServerSideCharacter: false,
      SenderPlayerSlot: new LegacyPlayerSlot(4)));
Assert(
  !ignoredSelfEcho.ShouldApply && ignoredSelfEcho.EffectivePlayerSlot is null,
  "Version4 must ignore a self-echoed Packet 13 outside server-side-character mode.");

PlayerPacket13RouteResult serverSideCharacterSelfUpdate =
  PlayerPacket13RouteQuery.Evaluate(
    new PlayerPacket13RouteInput(
      DeclaredPlayerSlot: 7,
      LocalPlayerSlot: 7,
      IsServerSideCharacter: true,
      SenderPlayerSlot: new LegacyPlayerSlot(4)));
Assert(
  serverSideCharacterSelfUpdate.ShouldApply &&
    serverSideCharacterSelfUpdate.EffectivePlayerSlot == new LegacyPlayerSlot(4),
  "Server-side-character mode must route an otherwise self-addressed packet to its sender slot.");

PlayerPacket13RouteResult spoofedDeclaredSlot =
  PlayerPacket13RouteQuery.Evaluate(
    new PlayerPacket13RouteInput(
      DeclaredPlayerSlot: 7,
      LocalPlayerSlot: 8,
      IsServerSideCharacter: false,
      SenderPlayerSlot: new LegacyPlayerSlot(4)));
Assert(
  spoofedDeclaredSlot.ShouldApply &&
    spoofedDeclaredSlot.EffectivePlayerSlot == new LegacyPlayerSlot(4),
  "Version4 must select whoAmI rather than the packet-declared player slot.");

Console.WriteLine("PASS: tile release, channel expectations, item-use gates, SetMatch, PlayerFrame composition, and Packet 13 routing preserve Version4 core rules");
