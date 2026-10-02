using System.Numerics;
using Terraria.Player;

PlayerInteractionUiStateComponent state =
  PlayerInteractionUiStateSystem.CreateResetState();
Require(!state.CreativeInterface, "Reset must clear creative-interface state.");
Require(!state.MouseInterface, "Reset must clear mouse-interface state.");
Require(!state.LastMouseInterface, "Reset must clear previous mouse-interface state.");
Require(state.NoThrow == 0, "Reset must clear the no-throw counter.");

PlayerInteractionUiStateInput input = new(
  CreativeInterface: true,
  MouseInterface: true,
  LastMouseInterface: false,
  NoThrow: 3);
PlayerInteractionUiStateSystem.Advance(input, reset: false, ref state);
Require(state.CreativeInterface, "Advance must preserve creative-interface input.");
Require(state.MouseInterface, "Advance must preserve mouse-interface input.");
Require(!state.LastMouseInterface, "Advance must preserve previous mouse-interface input.");
Require(state.NoThrow == 3, "Advance must preserve the no-throw counter.");

PlayerInteractionUiStateSystem.Advance(
  new PlayerInteractionUiStateInput(
    CreativeInterface: true,
    MouseInterface: true,
    LastMouseInterface: true,
    NoThrow: 7),
  reset: true,
  ref state);
Require(!state.CreativeInterface, "Reset must not retain creative-interface input.");
Require(!state.MouseInterface, "Reset must not retain mouse-interface input.");
Require(!state.LastMouseInterface, "Reset must not retain previous mouse-interface input.");
Require(state.NoThrow == 0, "Reset must not retain the no-throw counter.");

PlayerItemUseIntentComponent itemUseIntent =
  PlayerItemUseIntentSystem.CreateResetState();
Require(!itemUseIntent.ControlUseItem, "Reset must clear item-use intent.");
Require(!itemUseIntent.ControlUseTile, "Reset must clear tile-use intent.");

PlayerItemUseIntentSystem.Advance(
  new PlayerItemUseIntentInput(ControlUseItem: true, ControlUseTile: true),
  reset: false,
  ref itemUseIntent);
Require(itemUseIntent.ControlUseItem, "Advance must preserve item-use intent.");
Require(itemUseIntent.ControlUseTile, "Advance must preserve tile-use intent.");

PlayerItemUseIntentSystem.Advance(
  new PlayerItemUseIntentInput(ControlUseItem: true, ControlUseTile: true),
  reset: true,
  ref itemUseIntent);
Require(!itemUseIntent.ControlUseItem, "Reset must discard item-use intent.");
Require(!itemUseIntent.ControlUseTile, "Reset must discard tile-use intent.");

PlayerChannelCancellationExpectation expectation =
  new(ProjectileTypeExpected: 42, ProjectileIndexExpected: 3);
PlayerChannelCancellationProjectileSnapshot matchingProjectile =
  new(ProjectileType: 42, ProjectileIndex: 3, AiStyle: 0, Ai0: 0f);
Require(
  PlayerChannelCancellationAdapter.Matches(expectation, matchingProjectile),
  "A matching projectile type and index must cancel the channel expectation.");

Require(
  !PlayerChannelCancellationAdapter.Matches(
    expectation,
    new PlayerChannelCancellationProjectileSnapshot(
      ProjectileType: 42,
      ProjectileIndex: 4,
      AiStyle: 0,
      Ai0: 0f)),
  "A mismatched projectile index must not cancel the channel expectation.");

PlayerChannelCancellationExpectation trackedExpectation =
  PlayerChannelCancellationAdapter.Track(
    expectation,
    new PlayerChannelCancellationProjectileSnapshot(
      ProjectileType: 42,
      ProjectileIndex: 9,
      AiStyle: 0,
      Ai0: 0f));
Require(
  trackedExpectation.ProjectileIndexExpected == 9,
  "A matching projectile type must update the expected projectile index.");

PlayerChannelCancellationExpectation unchangedExpectation =
  PlayerChannelCancellationAdapter.Track(
    expectation,
    new PlayerChannelCancellationProjectileSnapshot(
      ProjectileType: 7,
      ProjectileIndex: 9,
      AiStyle: 0,
      Ai0: 0f));
Require(
  unchangedExpectation == expectation,
  "A mismatched projectile type must not update the expectation.");

Require(
  PlayerChannelCancellationAdapter.Matches(
    expectation,
    new PlayerChannelCancellationProjectileSnapshot(
      ProjectileType: 7,
      ProjectileIndex: 9,
      AiStyle: 99,
      Ai0: -3f)),
  "The special projectile channel marker must match any expectation.");

PlayerInstantMovementAccumulatorComponent movementAccumulator =
  PlayerInstantMovementAccumulatorSystem.CreateFrameState();
Require(
  movementAccumulator.AccumulatedMovementThisFrame == Vector2.Zero,
  "A new movement frame must start with a zero accumulator.");

PlayerInstantMovementAccumulatorSystem.Accumulate(
  new PlayerInstantMovementAccumulatorInput(new Vector2(-1f, 0.25f)),
  ref movementAccumulator);
PlayerInstantMovementAccumulatorSystem.Accumulate(
  new PlayerInstantMovementAccumulatorInput(new Vector2(1f, 0.5f)),
  ref movementAccumulator);
Require(
  movementAccumulator.AccumulatedMovementThisFrame == new Vector2(0f, 0.75f),
  "Movement deltas must accumulate without changing their vector components.");

PlayerInstantMovementAccumulatorSystem.BeginFrame(ref movementAccumulator);
Require(
  movementAccumulator.AccumulatedMovementThisFrame == Vector2.Zero,
  "Beginning a new movement frame must clear the previous accumulator.");

PlayerStepSoundProjection defaultStepSound =
  PlayerStepSoundQuery.Calculate(new PlayerStepSoundInput(LegArmorId: 0));
Require(defaultStepSound.SoundType == 17, "Default step sound must preserve SoundType 17.");
Require(defaultStepSound.SoundStyle == -1, "Default step sound must preserve the disabled style.");
Require(
  defaultStepSound.IntendedCooldown == 9,
  "Default step sound must preserve the nine-tick cooldown.");

PlayerStepSoundProjection hermesStepSound =
  PlayerStepSoundQuery.Calculate(new PlayerStepSoundInput(LegArmorId: 140));
Require(hermesStepSound.SoundType == 2, "Special leg armor must use SoundType 2.");
Require(hermesStepSound.SoundStyle == 24, "Special leg armor must use SoundStyle 24.");
Require(
  hermesStepSound.IntendedCooldown == 6,
  "Special leg armor must use the six-tick cooldown.");

PlayerNavigationInstrumentComponent navigationInstruments =
  PlayerNavigationInstrumentSystem.CreateResetState();
Require(
  navigationInstruments.CompassLevel == 0,
  "Reset must clear compass capability.");
Require(
  navigationInstruments.WatchLevel == 0,
  "Reset must clear watch capability.");
Require(
  navigationInstruments.DepthMeterLevel == 0,
  "Reset must clear depth-meter capability.");
Require(
  !navigationInstruments.WeatherRadioEnabled,
  "Reset must clear weather-radio capability.");
Require(
  !navigationInstruments.CalendarEnabled,
  "Reset must clear calendar capability.");
Require(
  !navigationInstruments.StopwatchEnabled,
  "Reset must clear stopwatch capability.");

PlayerNavigationInstrumentSystem.ApplyAccessoryType(
  new PlayerNavigationInstrumentInput(AccessoryType: 15),
  ref navigationInstruments);
Require(
  navigationInstruments.WatchLevel == 1,
  "The first watch accessory must provide watch level one.");

PlayerNavigationInstrumentSystem.ApplyAccessoryType(
  new PlayerNavigationInstrumentInput(AccessoryType: 17),
  ref navigationInstruments);
Require(
  navigationInstruments.WatchLevel == 3,
  "The third watch accessory must provide watch level three.");

PlayerNavigationInstrumentSystem.BeginFrame(ref navigationInstruments);
PlayerNavigationInstrumentSystem.ApplyAccessoryType(
  new PlayerNavigationInstrumentInput(AccessoryType: 709),
  ref navigationInstruments);
Require(
  navigationInstruments.WatchLevel == 3,
  "The third watch accessory alias must provide watch level three.");

PlayerNavigationInstrumentSystem.ApplyAccessoryType(
  new PlayerNavigationInstrumentInput(AccessoryType: 395),
  ref navigationInstruments);
Require(
  navigationInstruments.CompassLevel == 1,
  "The combined navigation accessory must provide a compass.");
Require(
  navigationInstruments.DepthMeterLevel == 1,
  "The combined navigation accessory must provide a depth meter.");
Require(
  navigationInstruments.WatchLevel == 3,
  "The combined navigation accessory must provide the highest watch level.");

PlayerNavigationInstrumentSystem.ApplyAccessoryType(
  new PlayerNavigationInstrumentInput(AccessoryType: 3036),
  ref navigationInstruments);
Require(
  navigationInstruments.WeatherRadioEnabled,
  "The PDA accessory must provide a weather radio.");
Require(
  navigationInstruments.CalendarEnabled,
  "The PDA accessory must provide a calendar.");

PlayerNavigationInstrumentSystem.ApplyAccessoryType(
  new PlayerNavigationInstrumentInput(AccessoryType: 3099),
  ref navigationInstruments);
Require(
  navigationInstruments.StopwatchEnabled,
  "The stopwatch accessory must provide a stopwatch.");

PlayerNavigationInstrumentSystem.BeginFrame(ref navigationInstruments);
PlayerNavigationInstrumentSystem.ApplyAccessoryType(
  new PlayerNavigationInstrumentInput(AccessoryType: 3121),
  ref navigationInstruments);
Require(
  navigationInstruments.StopwatchEnabled,
  "The stopwatch composite accessory must provide a stopwatch.");
Require(
  !navigationInstruments.WeatherRadioEnabled &&
  !navigationInstruments.CalendarEnabled,
  "The stopwatch composite accessory must not provide unrelated instruments.");

PlayerNavigationInstrumentSystem.BeginFrame(ref navigationInstruments);
Require(
  navigationInstruments.WatchLevel == 0,
  "Beginning a new instrument frame must clear watch capability.");
Require(
  navigationInstruments.CompassLevel == 0,
  "Beginning a new instrument frame must clear compass capability.");
Require(
  navigationInstruments.DepthMeterLevel == 0,
  "Beginning a new instrument frame must clear depth-meter capability.");
Require(
  !navigationInstruments.WeatherRadioEnabled &&
  !navigationInstruments.CalendarEnabled &&
  !navigationInstruments.StopwatchEnabled,
  "Beginning a new instrument frame must clear boolean capabilities.");

PlayerDetectionInstrumentComponent detectionInstruments =
  PlayerDetectionInstrumentSystem.CreateResetState();
Require(
  !detectionInstruments.FishFinderEnabled &&
  !detectionInstruments.JarOfSoulsEnabled &&
  !detectionInstruments.ThirdEyeEnabled &&
  !detectionInstruments.OreFinderEnabled &&
  !detectionInstruments.CritterGuideEnabled &&
  !detectionInstruments.DreamCatcherEnabled,
  "Reset must clear detection instrument capabilities.");

PlayerDetectionInstrumentSystem.ApplyAccessoryType(
  new PlayerDetectionInstrumentInput(AccessoryType: 3120),
  ref detectionInstruments);
Require(
  detectionInstruments.FishFinderEnabled,
  "The fish-finder accessory must provide fish-finder capability.");

PlayerDetectionInstrumentSystem.BeginFrame(ref detectionInstruments);
PlayerDetectionInstrumentSystem.ApplyAccessoryType(
  new PlayerDetectionInstrumentInput(AccessoryType: 3122),
  ref detectionInstruments);
Require(
  detectionInstruments.ThirdEyeEnabled &&
  detectionInstruments.JarOfSoulsEnabled &&
  detectionInstruments.CritterGuideEnabled,
  "The detection composite accessory must provide its three detection capabilities.");
Require(
  !detectionInstruments.FishFinderEnabled &&
  !detectionInstruments.OreFinderEnabled &&
  !detectionInstruments.DreamCatcherEnabled,
  "The detection composite accessory must not provide unrelated capabilities.");

PlayerDetectionInstrumentSystem.BeginFrame(ref detectionInstruments);
PlayerDetectionInstrumentSystem.ApplyAccessoryType(
  new PlayerDetectionInstrumentInput(AccessoryType: 3121),
  ref detectionInstruments);
Require(
  detectionInstruments.OreFinderEnabled &&
  detectionInstruments.DreamCatcherEnabled,
  "The stopwatch composite accessory must provide ore and dream-catcher capabilities.");

PlayerDetectionInstrumentSystem.BeginFrame(ref detectionInstruments);
PlayerDetectionInstrumentSystem.ApplyAccessoryType(
  new PlayerDetectionInstrumentInput(AccessoryType: 3123),
  ref detectionInstruments);
Require(
  detectionInstruments.FishFinderEnabled &&
  detectionInstruments.ThirdEyeEnabled &&
  detectionInstruments.JarOfSoulsEnabled &&
  detectionInstruments.CritterGuideEnabled &&
  detectionInstruments.OreFinderEnabled &&
  detectionInstruments.DreamCatcherEnabled,
  "The phone composite accessory must provide all detection capabilities.");

PlayerDetectionInstrumentSystem.BeginFrame(ref detectionInstruments);
Require(
  !detectionInstruments.FishFinderEnabled &&
  !detectionInstruments.ThirdEyeEnabled &&
  !detectionInstruments.JarOfSoulsEnabled &&
  !detectionInstruments.OreFinderEnabled &&
  !detectionInstruments.CritterGuideEnabled &&
  !detectionInstruments.DreamCatcherEnabled,
  "Beginning a new detection frame must clear all capabilities.");

Require(PlayerNameDefinition.MaximumLength == 20, "Player names must retain the Version4 limit.");
Require(PlayerNameLengthQuery.IsWithinLimit(0), "An empty length is within the length limit.");
Require(PlayerNameLengthQuery.IsWithinLimit(20), "The maximum length is within the length limit.");
Require(!PlayerNameLengthQuery.IsWithinLimit(21), "Lengths above the maximum must be rejected.");
Require(!PlayerNameLengthQuery.IsWithinLimit(-1), "Negative lengths must be rejected.");

Require(
  PlayerItemReuseQuery.IsUsingOrReusingItem(
    new PlayerItemReuseSnapshot(
      ItemAnimationRemainingTicks: 0,
      ReuseDelayRemainingTicks: 0,
      IsChanneling: false,
      HasPendingReuse: true)),
  "A pending reuse is active when animation, delay and channel are clear.");
Require(
  !PlayerItemReuseQuery.IsUsingOrReusingItem(
    new PlayerItemReuseSnapshot(
      ItemAnimationRemainingTicks: 0,
      ReuseDelayRemainingTicks: 0,
      IsChanneling: false,
      HasPendingReuse: false)),
  "No pending reuse is inactive when animation, delay and channel are clear.");
Require(
  PlayerItemReuseQuery.IsUsingOrReusingItem(
    new PlayerItemReuseSnapshot(
      ItemAnimationRemainingTicks: 1,
      ReuseDelayRemainingTicks: 0,
      IsChanneling: false,
      HasPendingReuse: false)),
  "Active animation keeps item use active.");
Require(
  PlayerItemReuseQuery.IsUsingOrReusingItem(
    new PlayerItemReuseSnapshot(
      ItemAnimationRemainingTicks: 0,
      ReuseDelayRemainingTicks: 1,
      IsChanneling: false,
      HasPendingReuse: false)),
  "A reuse delay keeps item use active.");
Require(
  PlayerItemReuseQuery.IsUsingOrReusingItem(
    new PlayerItemReuseSnapshot(
      ItemAnimationRemainingTicks: 0,
      ReuseDelayRemainingTicks: 0,
      IsChanneling: true,
      HasPendingReuse: false)),
  "Channeling keeps item use active.");

PlayerNpcPressureContribution ordinaryContribution =
  PlayerNpcPressureQuery.Evaluate(
    new PlayerNpcPressureContributionInput(
      NpcType: 1,
      LifeMaximum: 100,
      ReleaseOwner: PlayerNpcPressureQuery.UnownedReleaseOwner,
      NpcSlotCost: 2f,
      IsSlimeRainActive: false,
      IsSlimeRainNpc: false,
      IsNpcActive: true,
      IsPlayerInActiveRange: true));
Require(ordinaryContribution.ShouldContribute, "An eligible NPC must contribute.");
Require(ordinaryContribution.SlotWeight == 2f, "An ordinary NPC must preserve its slot cost.");

PlayerNpcPressureContribution slimeContribution =
  PlayerNpcPressureQuery.Evaluate(
    new PlayerNpcPressureContributionInput(
      NpcType: 1,
      LifeMaximum: 100,
      ReleaseOwner: PlayerNpcPressureQuery.UnownedReleaseOwner,
      NpcSlotCost: 2f,
      IsSlimeRainActive: true,
      IsSlimeRainNpc: true,
      IsNpcActive: true,
      IsPlayerInActiveRange: true));
Require(slimeContribution.ShouldContribute, "A slime-rain NPC must contribute.");
Require(slimeContribution.SlotWeight == 1.3f, "Slime-rain NPCs must use the 0.65 multiplier.");

foreach (int excludedType in new[] { 25, 30, 33 })
{
  PlayerNpcPressureContribution excludedContribution =
    PlayerNpcPressureQuery.Evaluate(
      new PlayerNpcPressureContributionInput(
        NpcType: excludedType,
        LifeMaximum: 100,
        ReleaseOwner: PlayerNpcPressureQuery.UnownedReleaseOwner,
        NpcSlotCost: 2f,
        IsSlimeRainActive: false,
        IsSlimeRainNpc: false,
        IsNpcActive: true,
        IsPlayerInActiveRange: true));
  Require(!excludedContribution.ShouldContribute, "Excluded NPC types must not contribute.");
  Require(excludedContribution.SlotWeight == 0f, "Excluded NPC types must have zero contribution.");
}

Require(
  PlayerNpcPressureQuery.SumContributions(
    new[]
    {
      new PlayerNpcPressureContributionInput(
        1, 100, PlayerNpcPressureQuery.UnownedReleaseOwner, 1f, false, false, true, true),
      new PlayerNpcPressureContributionInput(
        2, 100, PlayerNpcPressureQuery.UnownedReleaseOwner, 2f, false, false, true, true),
      new PlayerNpcPressureContributionInput(
        3, 100, PlayerNpcPressureQuery.UnownedReleaseOwner, 4f, false, false, true, false),
    }) == 3f,
  "Pressure aggregation must sum only eligible NPC contributions.");

Console.WriteLine("PASS: player interaction state, reuse, name, and NPC pressure queries are valid");

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
