using System.Numerics;
using Terraria.Player;
using Terraria.Player.Mount;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static MountDefinition CreateDefinition(
  int id,
  int flightTimeMax = 30,
  bool usesHover = true,
  bool isMinecart = false)
{
  MountGeometryDefinition geometry = new(
    textureWidth: 80,
    textureHeight: 80,
    xOffset: 0,
    yOffset: 0,
    bodyFrame: 0,
    playerHeadOffset: 0,
    heightBoost: 8,
    playerXOffset: 0,
    playerYOffsets: new[] { 1, 2 });
  MountFrameRange standing = new(10, 2, 1);
  MountFrameRange running = new(20, 3, 1);
  MountAnimationDefinition animation = new(
    TotalFrames: 32,
    Standing: standing,
    Running: running,
    InAir: new MountFrameRange(24, 1, 1),
    Flying: new MountFrameRange(25, 1, 1),
    Swimming: new MountFrameRange(26, 1, 1),
    Dashing: new MountFrameRange(27, 1, 1),
    Idle: new MountFrameRange(28, 1, 1),
    IdleFrameLoop: true);
  MountMovementDefinition movement = new(
    FlightTimeMax: flightTimeMax,
    UsesHover: usesHover,
    RunSpeed: 6f,
    DashSpeed: 8f,
    SwimSpeed: 4f,
    Acceleration: 0.1f,
    JumpSpeed: 5f,
    JumpHeight: 12,
    FallDamage: 1f,
    ExtraFall: 0,
    FatigueMax: 20f,
    ConstantJump: false,
    BlockExtraJumps: false,
    IsMinecart: isMinecart,
    CanRideMinecartTracks: isMinecart,
    CanUseWings: true,
    WalkingGraceTimeMax: 6,
    DismountsOnItemUse: true);
  MountAbilityDefinition ability = new(
    ChargeMax: 2,
    CooldownTicks: 10,
    DurationTicks: 4);
  MountPresentationDefinition presentation = new(
    LightColor: Vector3.One,
    EmitsLight: false,
    SpawnDust: 0,
    SpawnDustNoGravity: false);
  return new MountDefinition(
    id,
    buffType: 100 + id,
    geometry,
    animation,
    movement,
    ability,
    presentation);
}

MountDefinition definition = CreateDefinition(id: 0);
MountDefinitionCatalog catalog = new(new[] { definition });

var duplicateRejected = false;
try
{
  _ = new MountDefinitionCatalog(new[] { definition, CreateDefinition(id: 0) });
}
catch (ArgumentException)
{
  duplicateRejected = true;
}

Assert(duplicateRejected, "Catalog must reject duplicate mount IDs.");

int[] sourceOffsets = { 1, 2 };
MountGeometryDefinition copiedGeometry = new(
  textureWidth: 1,
  textureHeight: 1,
  xOffset: 0,
  yOffset: 0,
  bodyFrame: 0,
  playerHeadOffset: 0,
  heightBoost: 0,
  playerXOffset: 0,
  playerYOffsets: sourceOffsets);
sourceOffsets[0] = 99;
Assert(copiedGeometry.PlayerYOffsets[0] == 1,
  "Geometry offsets must be defensively copied.");

MountDrillConstantsDefinition drillConstants = MountDrillRulesQuery.Version4Constants;
Assert(drillConstants.DiodePointOne == new Vector2(36f, -6f) &&
  drillConstants.DiodePointTwo == new Vector2(36f, 8f) &&
  drillConstants.TextureWidth == 80 && drillConstants.PickPower == 210 &&
  drillConstants.PickTime == 1 && drillConstants.BeamsAtOnce == 2 &&
  drillConstants.MaxLength == 48f,
  "Drill constants must match the Version4 source values.");
MountSuperCartDefinition superCart = MountDrillRulesQuery.Version4SuperCart;
Assert(superCart.RunSpeed == 20f && superCart.DashSpeed == 20f &&
  superCart.Acceleration == 0.1f && superCart.JumpHeight == 15 &&
  superCart.JumpSpeed == 5.15f,
  "Super Cart constants must match the Version4 source values.");
MountSpecialVehicleCatalogDefinition specialVehicles =
  MountSpecialVehicleCatalogDefinition.Version4;
Assert(specialVehicles.ScutlixTextureSize == new Vector2(45f, 54f) &&
  specialVehicles.ScutlixBaseDamage == 50 &&
  specialVehicles.SantankTextureSize == new Vector2(23f, 2f) &&
  specialVehicles.ScutlixEyePositions.Count == 10 &&
  MountSpecialVehicleCatalogQuery.GetScutlixEyePosition(specialVehicles, 0) ==
    new Vector2(15f, -52f),
  "Scutlix eye positions must preserve the Version4 centered coordinates.");
MountEffectiveMovementSnapshot superCartMovement = MountSuperCartQuery.Project(
  definition.Movement,
  isUsingSuperCart: true);
Assert(superCartMovement.IsSuperCartOverride && superCartMovement.RunSpeed == 20f &&
  superCartMovement.JumpHeight == 15,
  "Super Cart projection must replace the source movement values when enabled.");

var frameState = new MountRuntimeFrameAndFlightStateComponent();
var abilityState = new MountFatigueAndAbilityStateComponent();
var variantState = new MountVariantStateComponent();
var drillState = new DrillMountRuntimeComponent();
var runtime = new MountRuntimeSystem(catalog);

MountActivationResult unknown = runtime.Activate(
  frameState,
  abilityState,
  variantState,
  drillState,
  new ContentId<MountDefinition>(99));
Assert(unknown.Kind == MountActivationResultKind.UnknownDefinition && !frameState.IsActive,
  "Unknown mount activation must fail without changing runtime state.");

ContentId<MountDefinition> mountType = new(0);
MountActivationResult activated = runtime.Activate(
  frameState,
  abilityState,
  variantState,
  drillState,
  mountType);
Assert(activated.Changed && frameState.IsActive && frameState.MountType == mountType,
  "Known mount activation must commit active state and type.");
Assert(frameState.Frame == 10 && abilityState.MaximumFatigue == 20f &&
  frameState.WalkingGraceRemainingTicks == 6,
  "Activation must seed standing frame, fatigue maximum and walking grace.");

MountActivationResult repeated = runtime.Activate(
  frameState,
  abilityState,
  variantState,
  drillState,
  mountType);
Assert(repeated.Kind == MountActivationResultKind.AlreadyActive,
  "Activating the same mount twice must be idempotent.");

MountFrameUpdateResult running = runtime.UpdateFrame(
  frameState,
  new MountFrameUpdateInput(
    MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Running,
    new Vector2(2f, 1f),
    IsDisplayDollOrInanimate: false,
    JustJumped: false,
    ControlDown: false));
Assert(running.Updated && running.Frame == 21 &&
  running.WalkingGraceRemainingTicks == 5,
  "Running frame update must select the running range and consume walking grace.");

MountFrameUpdateResult grounded = runtime.UpdateFrame(
  frameState,
  new MountFrameUpdateInput(
    MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Standing,
    Vector2.Zero,
    IsDisplayDollOrInanimate: false,
    JustJumped: false,
    ControlDown: false));
Assert(grounded.WalkingGraceRemainingTicks == 6,
  "Grounded frame update must restore walking grace.");

runtime.ResetFlightTime(frameState, horizontalVelocity: 2.5f);
Assert(frameState.FlightTimeRemainingTicks == 80,
  "Type zero flight reset must include the source velocity bonus.");
MountResourceTickResult flightTick = runtime.TickResources(
  frameState,
  abilityState,
  new MountResourceTickInput(
    RecoverFatigue: true,
    ConsumeFlightTime: true,
    ChargeAbility: false,
    SetAbilityActive: false,
    AbilityActiveValue: null));
Assert(flightTick.FlightAvailable && flightTick.FlightTimeRemainingTicks == 79,
  "Flight resource tick must consume one available flight tick.");

MountResourceTickResult chargeOne = runtime.TickResources(
  frameState,
  abilityState,
  new MountResourceTickInput(
    RecoverFatigue: false,
    ConsumeFlightTime: false,
    ChargeAbility: true,
    SetAbilityActive: false,
    AbilityActiveValue: null));
MountResourceTickResult chargeTwo = runtime.TickResources(
  frameState,
  abilityState,
  new MountResourceTickInput(
    RecoverFatigue: false,
    ConsumeFlightTime: false,
    ChargeAbility: true,
    SetAbilityActive: false,
    AbilityActiveValue: null));
Assert(chargeOne.IsAbilityActive == false && chargeOne.AbilityCharge == 1 &&
  chargeTwo.AbilityCharge == 2,
  "Ability charging must advance to the configured maximum.");
MountResourceTickResult released = runtime.TickResources(
  frameState,
  abilityState,
  new MountResourceTickInput(
    RecoverFatigue: false,
    ConsumeFlightTime: false,
    ChargeAbility: false,
    SetAbilityActive: true,
    AbilityActiveValue: true));
Assert(!abilityState.IsAbilityCharging && released.AbilityCharge == 1 &&
  released.AbilityCooldownRemainingTicks == 9 &&
  released.AbilityDurationRemainingTicks == 3 && released.IsAbilityActive,
  "Ability release must start cooldown/duration and apply the active flag.");

MountRuntimeSnapshot snapshot = MountRuntimeStateQuery.Snapshot(
  catalog,
  frameState,
  abilityState,
  variantState);
Assert(snapshot.IsActive && snapshot.CanFly && !snapshot.CanHover &&
  snapshot.RunSpeed == 6f && !snapshot.CanUseAbility,
  "Runtime snapshot must expose committed mobility and ability projections.");
MountRuntimeIdentityAndFrameSnapshot identitySnapshot =
  MountRuntimeIdentityAndFrameProjectionQuery.Snapshot(catalog, frameState);
Assert(identitySnapshot.HasDefinition && identitySnapshot.Frame == frameState.Frame &&
  identitySnapshot.HeightBoost == 8 && identitySnapshot.PlayerYOffsets.Count == 2,
  "Identity/frame projection must expose immutable definition geometry and frame state.");
MountRuntimeMobilityAndAbilitySnapshot mobilitySnapshot =
  MountRuntimeMobilityAndAbilityProjectionQuery.Snapshot(
    catalog,
    frameState,
    abilityState,
    variantState,
    isUsingSuperCart: false);
Assert(mobilitySnapshot.IsActive && mobilitySnapshot.CanFly &&
  mobilitySnapshot.Movement.RunSpeed == 6f &&
  mobilitySnapshot.Movement.IsSuperCartOverride == false &&
  !mobilitySnapshot.CanUseAbility,
  "Mobility/ability projection must remain a read-only committed view.");

MountQualificationResult unknownQualification = MountQualificationQuery.Evaluate(
  catalog,
  new ContentId<MountDefinition>(404),
  new MountQualificationInput(20, 42, true, false, false, false, false, false));
Assert(unknownQualification.Kind == MountQualificationResultKind.UnknownDefinition,
  "Qualification must reject an unknown definition.");
MountQualificationResult blockedSpace = MountQualificationQuery.Evaluate(
  catalog,
  mountType,
  new MountQualificationInput(20, 42, false, false, false, false, false, false));
Assert(blockedSpace.Kind == MountQualificationResultKind.BlockedBySpace &&
  blockedSpace.RequestedHeight == 50,
  "Qualification must report blocked space with the height boost.");
MountQualificationResult blockedWet = MountQualificationQuery.Evaluate(
  catalog,
  mountType,
  new MountQualificationInput(20, 42, true, true, false, false, false, false));
Assert(blockedWet.Kind == MountQualificationResultKind.BlockedByWetness,
  "Qualification must reject wet mounts.");
MountQualificationResult blockedGrapple = MountQualificationQuery.Evaluate(
  catalog,
  mountType,
  new MountQualificationInput(20, 42, true, false, false, false, true, false));
Assert(blockedGrapple.Kind == MountQualificationResultKind.BlockedByGrapple,
  "Qualification must reject grappling mounts without hook support.");
MountQualificationResult qualified = MountQualificationQuery.Evaluate(
  catalog,
  mountType,
  new MountQualificationInput(20, 42, true, false, false, false, false, false));
Assert(qualified.IsQualified, "A valid mount input must qualify.");

var drillSystem = new DrillMountSystem();
DrillMountRuntimeComponent.DrillTileTarget target = new(4, 5);
DrillBeamReservationResult invalidCooldown = drillSystem.TryReserveBeam(
  drillState,
  target,
  DrillMountRuntimeComponent.DrillBeamPurpose.Block,
  cooldownTicks: 0);
Assert(!invalidCooldown.Reserved, "A drill beam must reject a zero cooldown.");
DrillBeamReservationResult reserved = drillSystem.TryReserveBeam(
  drillState,
  target,
  DrillMountRuntimeComponent.DrillBeamPurpose.Block,
  cooldownTicks: 2);
Assert(reserved.Reserved, "A free drill beam must reserve a target.");
DrillBeamReservationResult duplicate = drillSystem.TryReserveBeam(
  drillState,
  target,
  DrillMountRuntimeComponent.DrillBeamPurpose.Block,
  cooldownTicks: 2);
Assert(!duplicate.Reserved, "A duplicate drill target must not reserve another beam.");
drillSystem.Tick(
  drillState,
  new DrillMountTickInput(0.5f, 0.25f, 0.1f));
Assert(drillState.BeamStates[reserved.BeamIndex].CooldownTicks == 1,
  "Drill tick must decrement beam cooldown.");
drillSystem.Tick(
  drillState,
  new DrillMountTickInput(1f, 0.5f, 0.1f));
Assert(drillState.BeamStates[reserved.BeamIndex].Target is null,
  "Drill target must clear when beam cooldown reaches zero.");
DrillBeamReservationResult reused = drillSystem.TryReserveBeam(
  drillState,
  target,
  DrillMountRuntimeComponent.DrillBeamPurpose.Block,
  cooldownTicks: 1);
Assert(reused.Reserved, "A cooled beam must accept the target again.");

MountDismountResult dismounted = runtime.Dismount(
  frameState,
  abilityState,
  variantState,
  drillState);
Assert(dismounted.Changed && !frameState.IsActive && frameState.MountType is null &&
  abilityState.MaximumFatigue == 0f && drillState.BeamStates.All(state => state.Target is null),
  "Dismount must clear the four owned runtime components.");

Console.WriteLine("PASS: mount catalog, lifecycle, frame, resources, qualification, and drill core");
