using System.Numerics;
using System.Linq;
using Terraria.Content;
using Terraria.Npc;

var system = new NpcAiSystem();
Terraria.NpcAi.Verification.NpcEyeOfCthulhuProfileVerification.Run();
Terraria.NpcAi.Verification.NpcServantOfCthulhuProfileVerification.Run();

// From the finite host's former center-based selection: measuring from the top selects slot 0.
NpcAiInput slime = CreateInput(1, 1, targets: [
  new(0, new NpcTargetGeometrySnapshot(new Vector2(4f, -10f), 20, 20),
      Vector2.Zero, true, false),
  new(1, new NpcTargetGeometrySnapshot(new Vector2(-2f, 0f), 20, 20),
      Vector2.Zero, true, false),
]);
NpcAiDecision slimeDecision = system.Evaluate(slime);
Require(slimeDecision.TargetSlot == 1 && slimeDecision.Action == -1,
    "Slime must choose the nearest player from its full center.");
Require(slimeDecision.Velocity == new Vector2(-2f, -5f),
    "Grounded slime must jump toward the selected player.");
Require(slime.State.State0 == 0f && system.Evaluate(slime) == slimeDecision,
    "Evaluation must preserve its input and be repeatable.");
NpcAiInput slimeWithLocalState = slime with {
  State = new NpcAiStateComponent(
      1,
      slime.State.State0,
      slime.State.State1,
      slime.State.State2,
      slime.State.State3,
      slime.State.Timer,
      localAi0: 11f,
      localAi1: 12f,
      localAi2: 13f,
      localAi3: 14f),
};
NpcAiDecision expectedCommittedSlimeDecision = system.Evaluate(slimeWithLocalState);
var committedBehaviorState = new NpcBehaviorStateComponent(1, 1, 0);
var committedLocalBehaviorState = new NpcLocalBehaviorStateComponent(
    [11f, 12f, 13f, 14f]);
NpcAiDecision committedSlimeDecision = system.EvaluateAndCommitFinite(
    slimeWithLocalState,
    committedBehaviorState,
    committedLocalBehaviorState);
Require(committedSlimeDecision == expectedCommittedSlimeDecision &&
    committedBehaviorState.Action == slimeDecision.Action &&
    committedBehaviorState.AuthoritativeAiSlots[0] == slimeDecision.State.State0 &&
    committedBehaviorState.AuthoritativeAiSlots[1] == slimeDecision.State.State1 &&
    committedBehaviorState.AuthoritativeAiSlots[2] == slimeDecision.State.State2 &&
    committedBehaviorState.AuthoritativeAiSlots[3] == slimeDecision.State.State3 &&
    committedLocalBehaviorState.LocalAiSlots[0] == 11f &&
    committedLocalBehaviorState.LocalAiSlots[1] == 12f &&
    committedLocalBehaviorState.LocalAiSlots[2] == 13f &&
    committedLocalBehaviorState.LocalAiSlots[3] == 14f,
    "The finite AI owner must commit action, AI slots, and instance local AI slots once.");

NpcAiInput tiedTargets = CreateInput(3, 3, targets: [
  new(9, new NpcTargetGeometrySnapshot(new Vector2(-2f, -1f), 20, 20),
      Vector2.Zero, true, false),
  new(2, new NpcTargetGeometrySnapshot(new Vector2(6f, -1f), 20, 20),
      Vector2.Zero, true, false),
]);
Require(system.Evaluate(tiedTargets).TargetSlot == 9,
    "Equal-distance targeting must preserve candidate order.");

NpcAiInput unavailableTargets = CreateInput(3, 3, targets: [
  new(0, new NpcTargetGeometrySnapshot(new Vector2(2f, -1f), 20, 20),
      Vector2.Zero, true, true),
  new(1, new NpcTargetGeometrySnapshot(new Vector2(2f, -1f), 20, 20),
      Vector2.Zero, false, false),
]);
NpcAiDecision idle = system.Evaluate(unavailableTargets);
Require(idle.TargetSlot == -1 && idle.Action == 0,
    "Dead and inactive players must be excluded.");

NpcAiInput eye = CreateInput(2, 2) with {
  State = new NpcAiStateComponent(2, 1f, 0f, -1f, 1f, 0),
  Targets = [new(
    0,
    new NpcTargetGeometrySnapshot(new Vector2(90f, 40f), 20, 20),
    Vector2.Zero,
    true,
    false)],
};
NpcAiDecision finalHoverTick = system.Evaluate(eye);
Require(finalHoverTick.Action == 0 && finalHoverTick.State.State0 == 0f,
    "Eye must finish the hover countdown before switching attacks.");
NpcAiDecision firstDiveTick = system.Evaluate(eye with { State = finalHoverTick.State });
Require(firstDiveTick.Action == 1 && firstDiveTick.State.State0 == 44f,
    "Eye must enter diving on the following tick.");

NpcAiInput guide = CreateInput(22, 0, town: true) with {
  Environment = new NpcAiEnvironmentSnapshot(false, true, true, new Vector2(100f, 9f)),
};
Require(system.Evaluate(guide).Action == 1,
    "Guide must walk toward an assigned home at night.");
Require(system.Evaluate(CreateInput(37, 0, town: true)).SkipMovement &&
    system.Evaluate(CreateInput(488, 0)).SkipMovement,
    "Old Man and training dummy must remain stationary in the finite profile.");

NpcTargetSelectionInputs normalTargets = CreateTargetInputs(
    NpcTargetSelectionStrategy.Normal,
    players: [
      CreatePlayerTarget(0, new Vector2(20f, 0f)),
      CreatePlayerTarget(1, new Vector2(10f, 10f)),
    ]);
NpcTargetSelectionStateComponent normalTargetState = new();
NpcTargetSelectionResult normalTarget = NpcTargetSelectionSystem.SelectAndCommit(
    in normalTargets,
    normalTargetState);
Require(normalTarget.TargetKind == NpcTargetKind.Player &&
    normalTarget.LegacyTargetIndex == 0 &&
    normalTarget.NetUpdateRequested &&
    normalTargetState.LegacyTargetIndex == 0,
    "Normal targeting must use Manhattan distance and preserve the first strict tie.");
NpcTargetSelectionInputs collidedNormalInputs = normalTargets with { CollideX = true };
Require(!NpcTargetSelectionSystem.Select(in collidedNormalInputs).NetUpdateRequested,
    "Normal target submission must defer netUpdate while collision flags block it.");

NpcTargetSelectionInputs wallOfFleshInputs = normalTargets with {
  Strategy = NpcTargetSelectionStrategy.WallOfFlesh,
  Players = [
    CreatePlayerTarget(0, new Vector2(4f, 0f), gross: false),
    CreatePlayerTarget(1, new Vector2(40f, 0f), gross: true),
  ],
};
NpcTargetSelectionResult wallOfFleshTarget = NpcTargetSelectionSystem.Select(
    in wallOfFleshInputs);
Require(wallOfFleshTarget.LegacyTargetIndex == 1,
    "Wall of Flesh targeting must reject players without gross.");

NpcTargetSelectionInputs upgradedNpcInputs = CreateTargetInputs(
    NpcTargetSelectionStrategy.Upgraded,
    players: [CreatePlayerTarget(0, new Vector2(100f, 0f))],
    npcs: [new(
      Slot: 12,
      TypeId: 548,
      Geometry: new NpcTargetGeometrySnapshot(new Vector2(4f, 0f), 20, 20),
      IsActive: true)]);
NpcTargetSelectionResult upgradedNpcTarget = NpcTargetSelectionSystem.Select(
    in upgradedNpcInputs);
Require(upgradedNpcTarget.TargetKind == NpcTargetKind.Npc &&
     upgradedNpcTarget.LegacyTargetIndex == 312 &&
    !upgradedNpcTarget.NetUpdateRequested,
    "Upgraded targeting must allow active 548 NPCs and preserve its immediate return path.");

NpcTargetSelectionInputs upgradedPetInputs = CreateTargetInputs(
    NpcTargetSelectionStrategy.Upgraded,
    players: [CreatePlayerTarget(
      0,
      new Vector2(100f, 0f),
      tankPet: new NpcTankPetTargetSnapshot(
        ProjectileSlot: 4,
        Geometry: new NpcTargetGeometrySnapshot(new Vector2(0f, 0f), 20, 20),
        CanHit: true))]);
NpcTargetSelectionResult upgradedPetTarget = NpcTargetSelectionSystem.Select(
    in upgradedPetInputs);
Require(upgradedPetTarget.TargetKind == NpcTargetKind.PlayerTankPet &&
    upgradedPetTarget.LegacyTargetIndex == 0 &&
    upgradedPetTarget.SecondaryLegacySlot == 4,
    "Upgraded targeting must keep the owner player index when a tank pet wins.");

NpcTargetSelectionInputs upgradedNoTargetInputs = CreateTargetInputs(
    NpcTargetSelectionStrategy.Upgraded,
    players: []);
NpcTargetSelectionResult upgradedNoTarget = NpcTargetSelectionSystem.Select(
    in upgradedNoTargetInputs);
Require(!upgradedNoTarget.HasTarget && !upgradedNoTarget.ShouldCommit,
    "Upgraded targeting must leave the submitted target unchanged when no candidate exists.");
NpcTargetSelectionStateComponent preservedTargetState = new();
_ = NpcTargetSelectionSystem.SelectAndCommit(in normalTargets, preservedTargetState);
_ = NpcTargetSelectionSystem.SelectAndCommit(in upgradedNoTargetInputs, preservedTargetState);
Require(preservedTargetState.LegacyTargetIndex == 0,
    "An upgraded no-candidate result must not clear the previously submitted target.");

NpcMovementTickStateComponent movementTickState = new();
movementTickState.BeginTick(
    new Vector2(4f, 8f),
    new Vector2(1f, -2f),
    wet: true,
    shimmerWet: false,
    honeyWet: true);
movementTickState.CommitPhysicsFlags(noGravity: true, noTileCollide: true);
movementTickState.CommitCollision(collideX: true, collideY: false);
Require(movementTickState.OldPosition == new Vector2(4f, 8f) &&
    movementTickState.OldVelocity == new Vector2(1f, -2f) &&
    movementTickState.Wet && movementTickState.HoneyWet &&
    movementTickState.NoGravity && movementTickState.NoTileCollide &&
    movementTickState.CollideX && !movementTickState.CollideY,
    "NPC movement tick state must preserve pre-AI facts and post-physics gates.");

bool rejected = false;
try {
  system.Evaluate(CreateInput(15, 1));
} catch (InvalidOperationException) {
  rejected = true;
}
Require(rejected, "A shared aiStyle must not silently enable unsupported NPC content.");

NpcBlueSlimeProfileInput sourceProfileInput = new(
    Position: Vector2.Zero,
    Velocity: Vector2.Zero,
    State: new NpcBlueSlimeProfileState(0f, 0f, 0f, 0f),
    Direction: 0,
    TargetSlot: 0,
    DayTime: true,
    IsDamaged: false,
    IsBelowSurface: false,
    SlimeRain: false,
    Wet: false,
    CollideX: false,
    CollideY: false,
    OldVelocity: Vector2.Zero,
    Gravity: 0.25f,
    IsClient: false,
    CanContainItems: true,
    Value: 25f,
    BaseDefense: 2);
Require(NpcBlueSlimeProfile.CanHandle(1, 1, 1) &&
    !NpcBlueSlimeProfile.CanHandle(16, 16, 1) &&
    !NpcBlueSlimeProfile.CanHandle(1, 1, 3),
    "Blue Slime source profile must require its concrete type, net id, and aiStyle identity.");
NpcBlueSlimeProfileResult sourceProfile = NpcBlueSlimeProfile.Evaluate(
    in sourceProfileInput);
Require(sourceProfile.ContainedItemGenerationRequested &&
    sourceProfile.TargetClosestRequested &&
    sourceProfile.NetUpdateRequested &&
    (sourceProfile.Branches & NpcBlueSlimeSourceBranch.ContainedItemGeneration) != 0 &&
    (sourceProfile.Branches & NpcBlueSlimeSourceBranch.TargetInitialization) != 0,
    "Blue Slime source profile must expose the server item-generation and target-init boundaries.");

NpcBlueSlimeProfileInput blueCooldownInput = sourceProfileInput with {
  State = new NpcBlueSlimeProfileState(-50f, 0f, 3f, 0f),
  Velocity = new Vector2(0f, -1f),
  Direction = 1,
  TargetSlot = 255,
  CanContainItems = false,
};
NpcBlueSlimeProfileResult blueCooldownTick1 =
  NpcBlueSlimeProfile.Evaluate(in blueCooldownInput);
NpcBlueSlimeProfileInput blueCooldownTick2Input = blueCooldownInput with {
  State = blueCooldownTick1.State,
};
NpcBlueSlimeProfileResult blueCooldownTick2 =
  NpcBlueSlimeProfile.Evaluate(in blueCooldownTick2Input);
NpcBlueSlimeProfileInput blueCooldownTick3Input = blueCooldownTick2Input with {
  State = blueCooldownTick2.State,
};
NpcBlueSlimeProfileResult blueCooldownTick3 =
  NpcBlueSlimeProfile.Evaluate(in blueCooldownTick3Input);
Require(blueCooldownTick1.State.Ai2 == 2f &&
    blueCooldownTick2.State.Ai2 == 1f &&
    blueCooldownTick3.State.Ai2 == 1f &&
    (blueCooldownTick1.Branches & NpcBlueSlimeSourceBranch.Ai2Cooldown) != 0 &&
    (blueCooldownTick2.Branches & NpcBlueSlimeSourceBranch.Ai2Cooldown) != 0 &&
    (blueCooldownTick3.Branches & NpcBlueSlimeSourceBranch.Ai2Cooldown) == 0,
    "Blue Slime ai[2] cooldown must advance 3→2→1 and hold at 1.");
NpcBlueSlimeProfileInput blueCooldownFrozenInput = blueCooldownInput with {
  State = new NpcBlueSlimeProfileState(-999f, 0f, 3f, 0f),
};
NpcBlueSlimeProfileResult blueCooldownFrozen =
  NpcBlueSlimeProfile.Evaluate(in blueCooldownFrozenInput);
Require(blueCooldownFrozen.State.Ai2 == 3f &&
    (blueCooldownFrozen.Branches & NpcBlueSlimeSourceBranch.Ai2Cooldown) == 0,
    "Blue Slime -999 sentinel must return before ai[2] cooldown decrement.");

NpcBlueSlimeProfileState cadenceState = new(-100f, 0f, 1f, 0f);
for (int tick = 0; tick < 99; tick++)
{
  NpcBlueSlimeProfileInput cadenceInput = sourceProfileInput with {
    State = cadenceState,
    Direction = 1,
    CanContainItems = false,
  };
  NpcBlueSlimeProfileResult cadence = NpcBlueSlimeProfile.Evaluate(
      in cadenceInput);
  cadenceState = cadence.State;
}
NpcBlueSlimeProfileInput firstJumpInput = sourceProfileInput with {
  State = cadenceState,
  Direction = 1,
  CanContainItems = false,
};
NpcBlueSlimeProfileResult firstSourceJump = NpcBlueSlimeProfile.Evaluate(
    in firstJumpInput);
Require(firstSourceJump.Velocity == new Vector2(2f, -6f) &&
    firstSourceJump.State.Ai0 == -1120f &&
    (firstSourceJump.Branches & NpcBlueSlimeSourceBranch.JumpImpulse) != 0,
    "Blue Slime source profile must preserve the normal grounded counter and first jump impulse.");

NpcBlueSlimeProfileInput dirtSlimeGroundedInput = sourceProfileInput with {
  State = new NpcBlueSlimeProfileState(-100f, 2f, 1f, 0f),
  Direction = 1,
  DayTime = true,
  CanContainItems = false,
};
NpcBlueSlimeProfileResult dirtSlimeGrounded = NpcBlueSlimeProfile.Evaluate(
    in dirtSlimeGroundedInput);
Require(dirtSlimeGrounded.State.Ai0 == -90f &&
    (dirtSlimeGrounded.Branches & NpcBlueSlimeSourceBranch.DirtSlimeGroundCounter) != 0,
    "A grounded Dirt Slime must advance ai[0] by nine before the same tick's grounded counter.");

NpcBlueSlimeProfileInput dirtSlimeFrozenInput = sourceProfileInput with {
  State = new NpcBlueSlimeProfileState(-999f, 0f, 1f, 0f),
  Direction = 1,
  DayTime = true,
  CanContainItems = false,
};
NpcBlueSlimeTypeOneSelectionResult dirtSlimeSelection =
  new(true, 2f, NetUpdateRequested: true);
NpcBlueSlimeProfileInput dirtSlimeSelectedInput =
  NpcBlueSlimeProfile.WithContainedItemSelection(
    in dirtSlimeFrozenInput,
    in dirtSlimeSelection);
NpcBlueSlimeProfileResult dirtSlimeFrozen = NpcBlueSlimeProfile.Evaluate(
    in dirtSlimeSelectedInput);
Require(!dirtSlimeFrozen.IsFrozen &&
    dirtSlimeSelectedInput.State.Ai1 == 2f &&
    dirtSlimeFrozen.State.Ai0 == -2120f &&
    dirtSlimeFrozen.Velocity == new Vector2(2f, -6f) &&
    (dirtSlimeFrozen.Branches & NpcBlueSlimeSourceBranch.DirtSlimeGroundCounter) != 0,
    "The Dirt Slime ai[1] branch must run before the -999 sentinel and can reach the same-tick jump phase.");

NpcBlueSlimeProfileInput specialGroundedSlimeInput = sourceProfileInput with {
  State = new NpcBlueSlimeProfileState(-100f, 3609f, 1f, 0f),
  Velocity = new Vector2(1f, 0f),
  Direction = -1,
};
NpcBlueSlimeProfileResult specialGroundedSlime =
  NpcBlueSlimeProfile.Evaluate(in specialGroundedSlimeInput);
Require(specialGroundedSlime.Velocity.X == 0.9f &&
    (specialGroundedSlime.Branches &
      NpcBlueSlimeSourceBranch.StoredAi1GroundAcceleration) != 0,
  "Blue Slime ai[1]=3609 must use the source grounded directional acceleration instead of ordinary friction.");

NpcBlueSlimeProfileInput stoneSlimeFallingInput = sourceProfileInput with {
  State = new NpcBlueSlimeProfileState(-50f, 3f, 1f, 0f),
  Direction = 1,
  TargetSlot = -1,
  Velocity = new Vector2(0f, 1.5f),
  CanContainItems = false,
};
NpcBlueSlimeProfileResult stoneSlimeFalling = NpcBlueSlimeProfile.Evaluate(
    in stoneSlimeFallingInput);
Require(MathF.Abs(stoneSlimeFalling.Velocity.Y - 2f) < 0.0001f &&
    (stoneSlimeFalling.Branches & NpcBlueSlimeSourceBranch.StoneSlimeGravity) != 0,
    "A falling Stone Slime must add twice the current NPC gravity before common movement.");

NpcBlueSlimeProfileInput stoneSlimeRisingInput = stoneSlimeFallingInput with {
  Velocity = new Vector2(0f, -1.5f),
};
NpcBlueSlimeProfileResult stoneSlimeRising = NpcBlueSlimeProfile.Evaluate(
    in stoneSlimeRisingInput);
Require(stoneSlimeRising.Velocity.Y == -1.5f &&
    (stoneSlimeRising.Branches & NpcBlueSlimeSourceBranch.StoneSlimeGravity) == 0,
    "A rising Stone Slime must not receive the source falling-only gravity increase.");

NpcBlueSlimeProfileInput stoneSlimeFrozenInput = stoneSlimeFallingInput with {
  State = new NpcBlueSlimeProfileState(-999f, 3f, 1f, 0f),
};
NpcBlueSlimeProfileResult stoneSlimeFrozen = NpcBlueSlimeProfile.Evaluate(
    in stoneSlimeFrozenInput);
Require(stoneSlimeFrozen.IsFrozen &&
    MathF.Abs(stoneSlimeFrozen.Velocity.Y - 2f) < 0.0001f &&
    (stoneSlimeFrozen.Branches & NpcBlueSlimeSourceBranch.StoneSlimeGravity) != 0,
    "Stone Slime gravity must be applied before the source -999 sentinel return.");

NpcBlueSlimeProfileInput cloudSlimeFallingInput = sourceProfileInput with {
  State = new NpcBlueSlimeProfileState(-50f, 751f, 1f, 0f),
  Direction = 1,
  TargetSlot = -1,
  Velocity = new Vector2(0f, 1.5f),
  CanContainItems = false,
};
NpcBlueSlimeProfileResult cloudSlimeFalling = NpcBlueSlimeProfile.Evaluate(
    in cloudSlimeFallingInput);
Require(MathF.Abs(cloudSlimeFalling.Velocity.Y - 1.35f) < 0.0001f &&
    (cloudSlimeFalling.Branches & NpcBlueSlimeSourceBranch.CloudSlimeGravity) != 0,
    "A moving Cloud Slime must subtract 0.6 times current NPC gravity before common movement.");

NpcBlueSlimeProfileInput cloudSlimeRisingInput = cloudSlimeFallingInput with {
  Velocity = new Vector2(0f, -1.5f),
};
NpcBlueSlimeProfileResult cloudSlimeRising = NpcBlueSlimeProfile.Evaluate(
    in cloudSlimeRisingInput);
Require(MathF.Abs(cloudSlimeRising.Velocity.Y + 1.65f) < 0.0001f &&
    (cloudSlimeRising.Branches & NpcBlueSlimeSourceBranch.CloudSlimeGravity) != 0,
    "A rising Cloud Slime must also receive the source nonzero-velocity gravity adjustment.");

NpcBlueSlimeProfileInput cloudSlimeStationaryInput = cloudSlimeFallingInput with {
  Velocity = Vector2.Zero,
};
NpcBlueSlimeProfileResult cloudSlimeStationary = NpcBlueSlimeProfile.Evaluate(
    in cloudSlimeStationaryInput);
Require(cloudSlimeStationary.Velocity.Y == 0f &&
    (cloudSlimeStationary.Branches & NpcBlueSlimeSourceBranch.CloudSlimeGravity) == 0,
    "A stationary Cloud Slime must skip the source nonzero-vertical-velocity gravity branch.");

NpcBlueSlimeProfileInput cloudSlimeFrozenInput = cloudSlimeFallingInput with {
  State = new NpcBlueSlimeProfileState(-999f, 751f, 1f, 0f),
};
NpcBlueSlimeProfileResult cloudSlimeFrozen = NpcBlueSlimeProfile.Evaluate(
    in cloudSlimeFrozenInput);
Require(cloudSlimeFrozen.IsFrozen &&
    MathF.Abs(cloudSlimeFrozen.Velocity.Y - 1.35f) < 0.0001f &&
    (cloudSlimeFrozen.Branches & NpcBlueSlimeSourceBranch.CloudSlimeGravity) != 0,
    "Cloud Slime gravity must be applied before the source -999 sentinel return.");

NpcBlueSlimeProfileInput activeJumpInput = sourceProfileInput with {
  State = new NpcBlueSlimeProfileState(-2f, 0f, 1f, 0f),
  Direction = 1,
  DayTime = false,
  CanContainItems = false,
};
NpcBlueSlimeProfileResult activeJump = NpcBlueSlimeProfile.Evaluate(
    in activeJumpInput);
Require(activeJump.Velocity == new Vector2(2f, -6f) &&
    (activeJump.Branches & NpcBlueSlimeSourceBranch.TargetReacquire) != 0,
    "Night or damaged Blue Slime cadence must accelerate and reacquire its target at the jump boundary.");

NpcBlueSlimeProfileInput wetInput = sourceProfileInput with {
  State = new NpcBlueSlimeProfileState(-50f, 0f, 1f, 0f),
  Direction = 1,
  Velocity = new Vector2(0f, 3f),
  Wet = true,
  CanContainItems = false,
};
NpcBlueSlimeProfileResult wetResult = NpcBlueSlimeProfile.Evaluate(in wetInput);
Require(MathF.Abs(wetResult.Velocity.Y - 2.2f) < 0.0001f &&
    (wetResult.Branches & NpcBlueSlimeSourceBranch.WetMovement) != 0,
    "Wet Blue Slime movement must apply the source downward damping and -4 cap.");

NpcBlueSlimeProfileInput frozenInput = sourceProfileInput with {
  State = new NpcBlueSlimeProfileState(-999f, 0f, 1f, 0f),
  Direction = 1,
  CanContainItems = false,
};
NpcBlueSlimeProfileResult frozen = NpcBlueSlimeProfile.Evaluate(
    in frozenInput);
Require(frozen.IsFrozen && frozen.Velocity == Vector2.Zero,
    "The source -999 sentinel must stop Blue Slime movement while preserving state.");

var effectPort = new RecordingBlueSlimeEffectPort();
NpcBlueSlimeProfile.ApplyEffects(in sourceProfile, isBallooned: false, effectPort);
Require(effectPort.Events.SequenceEqual([
      "generate-item",
      "network-sync",
      "target-reacquire",
    ]),
    "Blue Slime profile effects must preserve the initial source order: item, network sync, then target reacquire.");

NpcBlueSlimeProfileResult itemOnlyEffectResult = new(
  Velocity: Vector2.Zero,
  Defense: 2,
  State: new NpcBlueSlimeProfileState(0f, 0f, 0f, 0f),
  Direction: 1,
  AiAction: 0,
  NetUpdateRequested: false,
  TargetClosestRequested: true,
  ContainedItemGenerationRequested: true,
  Branches: NpcBlueSlimeSourceBranch.ContainedItemGeneration);
var itemOnlyEffectPort = new RecordingBlueSlimeEffectPort();
NpcBlueSlimeProfile.ApplyEffects(in itemOnlyEffectResult, isBallooned: false, itemOnlyEffectPort);
Require(itemOnlyEffectPort.Events.SequenceEqual([
      "generate-item",
      "network-sync",
      "target-reacquire",
    ]),
    "A contained-item state submission must request network sync before target reacquisition.");

var preselectedItemEffectPort = new RecordingBlueSlimeEffectPort();
preselectedItemEffectPort.Events.Add("generate-item");
NpcBlueSlimeProfile.ApplyEffects(
  in itemOnlyEffectResult,
  isBallooned: false,
  preselectedItemEffectPort,
  new NpcBlueSlimeTypeOneSelectionResult(true, 2f, true));
Require(preselectedItemEffectPort.Events.SequenceEqual([
      "generate-item",
      "network-sync",
      "target-reacquire",
    ]),
    "A preselected item must not be rolled twice and its sync must remain before target reacquisition.");

NpcBlueSlimeContainedItemInput normalItemInput = new(
    IsBallooned: false,
    LowTiles: false,
    MoonPhase: 0,
    InRockLayer: false,
    HardMode: false,
    NetMode: 0);
Require(NpcBlueSlimeContainedItemGenerator.Generate(
      in normalItemInput,
      new QueueBlueSlimeRandomPort(0, 0)) == 290,
    "Blue Slime item selector must preserve the normal category-zero random path.");
NpcBlueSlimeContainedItemInput balloonedItemInput = normalItemInput with { IsBallooned = true };
Require(NpcBlueSlimeContainedItemGenerator.Generate(
      in balloonedItemInput,
      new QueueBlueSlimeRandomPort(0, 0)) == 4367,
    "Blue Slime item selector must preserve the ballooned item path.");
NpcBlueSlimeContainedItemInput lowTilesItemInput = normalItemInput with { LowTiles = true };
Require(NpcBlueSlimeContainedItemGenerator.Generate(
      in lowTilesItemInput,
      new QueueBlueSlimeRandomPort(3, 0, 0, 2, 0)) == 73,
    "Blue Slime item selector must preserve low-tile random consumption before category three.");

NpcMotherSlimeProfileInput motherSlimeInput = new(
  TypeId: 16,
  NetId: 16,
  AiStyle: 1,
  Position: new Vector2(100f, 200f),
  Velocity: Vector2.Zero,
  State: new NpcMotherSlimeProfileState(0f, 0f, 0f, 0f),
  Direction: 1,
  TargetSlot: 255,
  DayTime: true,
  IsDamaged: false,
  IsBelowSurface: false,
  SlimeRain: false,
  Wet: false,
  CollideX: false,
  CollideY: false,
  OldVelocity: Vector2.Zero,
  SolidCollision: false);
Require(NpcMotherSlimeProfile.CanHandle(16, 16, 1) &&
    !NpcMotherSlimeProfile.CanHandle(1, 1, 1) &&
    !NpcMotherSlimeProfile.CanHandle(59, 59, 1) &&
    !NpcMotherSlimeProfile.CanHandle(16, 16, 3),
  "Mother Slime profile must accept only its type/net id/style tuple.");
Require(!NpcMotherSlimeProfile.SupportsContainedItemGeneration,
  "Mother Slime must honor SlimeCanContainItems[16] = false.");
NpcMotherSlimeProfileResult motherSlimeInitialization =
  NpcMotherSlimeProfile.Evaluate(in motherSlimeInput);
Require(motherSlimeInitialization.State.Ai0 == -99f &&
    motherSlimeInitialization.State.Ai2 == 1f &&
    motherSlimeInitialization.TargetClosestRequested &&
    motherSlimeInitialization.ContainedItemSelectionRejected,
  "Mother Slime must initialize ai[0]/ai[2], request its source target effect, and reject item selection.");
var motherSlimeInitializationEffects = new RecordingMotherSlimeEffectPort();
NpcMotherSlimeProfile.ApplyEffects(
  in motherSlimeInitialization,
  motherSlimeInitializationEffects);
Require(motherSlimeInitializationEffects.Events.SequenceEqual(["target-reacquire"]),
  "Mother Slime initialization must execute TargetClosest through the effect boundary.");

NpcMotherSlimeProfileInput motherSlimeCooldownInput = motherSlimeInput with
{
  State = new NpcMotherSlimeProfileState(-50f, 0f, 3f, 0f),
  Velocity = new Vector2(0f, -1f),
  TargetSlot = 255,
};
NpcMotherSlimeProfileResult motherSlimeCooldownTick1 =
  NpcMotherSlimeProfile.Evaluate(in motherSlimeCooldownInput);
NpcMotherSlimeProfileInput motherSlimeCooldownTick2Input =
  motherSlimeCooldownInput with { State = motherSlimeCooldownTick1.State };
NpcMotherSlimeProfileResult motherSlimeCooldownTick2 =
  NpcMotherSlimeProfile.Evaluate(in motherSlimeCooldownTick2Input);
NpcMotherSlimeProfileInput motherSlimeCooldownTick3Input =
  motherSlimeCooldownTick2Input with { State = motherSlimeCooldownTick2.State };
NpcMotherSlimeProfileResult motherSlimeCooldownTick3 =
  NpcMotherSlimeProfile.Evaluate(in motherSlimeCooldownTick3Input);
Require(motherSlimeCooldownTick1.State.Ai2 == 2f &&
    motherSlimeCooldownTick2.State.Ai2 == 1f &&
    motherSlimeCooldownTick3.State.Ai2 == 1f &&
    (motherSlimeCooldownTick1.Branches & NpcMotherSlimeSourceBranch.Ai2Cooldown) != 0 &&
    (motherSlimeCooldownTick2.Branches & NpcMotherSlimeSourceBranch.Ai2Cooldown) != 0 &&
    (motherSlimeCooldownTick3.Branches & NpcMotherSlimeSourceBranch.Ai2Cooldown) == 0,
  "Mother Slime ai[2] cooldown must advance 3→2→1 and hold at 1.");
NpcMotherSlimeProfileInput motherSlimeCooldownFrozenInput = motherSlimeCooldownInput with
{
  State = new NpcMotherSlimeProfileState(-999f, 0f, 3f, 0f),
};
NpcMotherSlimeProfileResult motherSlimeCooldownFrozen =
  NpcMotherSlimeProfile.Evaluate(in motherSlimeCooldownFrozenInput);
Require(motherSlimeCooldownFrozen.State.Ai2 == 3f &&
    (motherSlimeCooldownFrozen.Branches & NpcMotherSlimeSourceBranch.Ai2Cooldown) == 0,
  "Mother Slime -999 sentinel must return before ai[2] cooldown decrement.");

NpcMotherSlimeProfileInput motherSlimeWetInput = motherSlimeInput with
{
  State = new NpcMotherSlimeProfileState(-50f, 0f, 1f, 100f),
  Direction = 1,
  DayTime = false,
  Wet = true,
  Velocity = new Vector2(0f, -1f),
};
NpcMotherSlimeProfileResult motherSlimeWet = NpcMotherSlimeProfile.Evaluate(
  in motherSlimeWetInput);
Require(motherSlimeWet.Direction == -1 &&
    motherSlimeWet.State.Ai2 == 200f &&
    motherSlimeWet.Velocity.Y == -1.5f &&
    !motherSlimeWet.TargetClosestRequested,
  "Mother Slime wet rising at the stored ai[3] X must turn and suppress the later ai[2] target gate.");

NpcMotherSlimeProfileInput motherSlimeSolidCollisionInput = motherSlimeInput with
{
  State = new NpcMotherSlimeProfileState(-50f, 0f, 1f, 99f),
  CollideY = true,
  OldVelocity = new Vector2(0f, 4f),
  SolidCollision = true,
};
NpcMotherSlimeProfileResult motherSlimeSolidCollision =
  NpcMotherSlimeProfile.Evaluate(in motherSlimeSolidCollisionInput);
Require(motherSlimeSolidCollision.Position.X == 99f &&
    motherSlimeSolidCollision.Direction == -1 &&
    motherSlimeSolidCollision.State.Ai2 == 200f,
  "Mother Slime grounded SolidCollision correction must precede the ai[3] position turn check.");

NpcMotherSlimeProfileInput motherSlimeAirborneInput = motherSlimeInput with
{
  State = new NpcMotherSlimeProfileState(-50f, 0f, 1f, 50f),
  Velocity = new Vector2(0.2f, -1f),
  TargetSlot = 254,
  DayTime = false,
  CollideX = true,
};
NpcMotherSlimeProfileResult motherSlimeAirborne = NpcMotherSlimeProfile.Evaluate(
  in motherSlimeAirborneInput);
Require(motherSlimeAirborne.Position.X == 98.6f &&
    motherSlimeAirborne.Velocity.X == 0.4f &&
    (motherSlimeAirborne.Branches & NpcMotherSlimeSourceBranch.AirborneMovement) != 0,
  "Mother Slime airborne source path must apply its collideX nudge before acceleration.");

NpcMotherSlimeProfileInput motherSlimeJumpInput = motherSlimeInput with
{
  State = new NpcMotherSlimeProfileState(-1f, 0f, 1f, 50f),
  DayTime = false,
};
NpcMotherSlimeProfileResult motherSlimeJump = NpcMotherSlimeProfile.Evaluate(
  in motherSlimeJumpInput);
Require(motherSlimeJump.State.Ai0 == -1120f &&
    motherSlimeJump.Velocity == new Vector2(2f, -6f) &&
    motherSlimeJump.NetUpdateRequested &&
    motherSlimeJump.TargetClosestRequested,
  "Mother Slime first jump must use the source counter threshold, impulse, and effects.");
var motherSlimeJumpEffects = new RecordingMotherSlimeEffectPort();
NpcMotherSlimeProfile.ApplyEffects(in motherSlimeJump, motherSlimeJumpEffects);
Require(motherSlimeJumpEffects.Events.SequenceEqual(["network-sync", "target-reacquire"]),
  "Mother Slime jump must request network sync before TargetClosest.");

NpcMotherSlimeProfileInput motherSlimeThirdJumpInput = motherSlimeInput with
{
  State = new NpcMotherSlimeProfileState(-1502f, 0f, 1f, 50f),
  DayTime = false,
};
NpcMotherSlimeProfileResult motherSlimeThirdJump =
  NpcMotherSlimeProfile.Evaluate(in motherSlimeThirdJumpInput);
Require(motherSlimeThirdJump.State.Ai0 == -200f &&
    motherSlimeThirdJump.State.Ai3 == motherSlimeThirdJumpInput.Position.X &&
    motherSlimeThirdJump.Velocity == new Vector2(3f, -8f),
  "Mother Slime third jump phase must store its X origin and source impulse.");

NpcMotherSlimeProfileInput motherSlimeFrozenInput = motherSlimeInput with
{
  State = new NpcMotherSlimeProfileState(-999f, 0f, 0f, 0f),
  Wet = true,
  Velocity = new Vector2(0f, -1f),
};
NpcMotherSlimeProfileResult motherSlimeFrozen = NpcMotherSlimeProfile.Evaluate(
  in motherSlimeFrozenInput);
Require(motherSlimeFrozen.IsFrozen &&
    motherSlimeFrozen.State == motherSlimeFrozenInput.State &&
    motherSlimeFrozen.Velocity == motherSlimeFrozenInput.Velocity &&
    !motherSlimeFrozen.TargetClosestRequested,
  "Mother Slime -999 sentinel must return before wet movement or target effects.");

NpcBlueSlimeTypeOneSelectionInput surfaceSelectionInput = new(
  CurrentItemState: 0f,
  NetId: 1,
  NpcValue: 25f,
  PositionY: 1000f,
  CenterY: 1012f,
  WorldSurfaceTiles: 100d,
  PositionInRockLayer: false,
  CenterInRockLayer: false,
  NoTrapsWorld: false,
  GetGoodWorld: false,
  RemixWorld: false,
  VampireSeed: false,
  SlimeRain: false,
  GenuineParty: false,
  IsBallooned: false,
  IsSkyblockWorld: false,
  HardMode: false,
  MoonPhase: 0,
  NetMode: 0);
NpcBlueSlimeTypeOneSelectionResult surfaceSelection =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in surfaceSelectionInput,
    new QueueBlueSlimeRandomPort(1, 0));
Require(surfaceSelection.Attempted &&
    surfaceSelection.ItemState == 751f &&
    surfaceSelection.NetUpdateRequested,
    "The first Blue Slime item pass must retain the source pending sentinel until a surface item is selected.");

NpcBlueSlimeTypeOneSelectionInput undergroundSelectionInput =
  surfaceSelectionInput with {
    PositionY = 1800f,
    CenterY = 1812f,
    WorldSurfaceTiles = 100d,
  };
NpcBlueSlimeTypeOneSelectionResult undergroundSelection =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in undergroundSelectionInput,
    new QueueBlueSlimeRandomPort(0, 0, 0));
Require(undergroundSelection.Attempted && undergroundSelection.ItemState == 290f,
    "The Blue Slime entry must run the contained-item helper only after its source entry roll succeeds.");

NpcBlueSlimeTypeOneSelectionInput noTrapsSelectionInput = undergroundSelectionInput with {
  NoTrapsWorld = true,
};
NpcBlueSlimeTypeOneSelectionResult noTrapsSelection =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in noTrapsSelectionInput,
    new QueueBlueSlimeRandomPort(1, 1, 0));
Require(noTrapsSelection.ItemState == 539f,
    "The No Traps Blue Slime branch must use its source special-item chance after normal item rolls.");

NpcBlueSlimeTypeOneSelectionInput partySelectionInput = surfaceSelectionInput with {
  CenterY = 1000f,
  GenuineParty = true,
};
NpcBlueSlimeTypeOneSelectionResult partySelection =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in partySelectionInput,
    new QueueBlueSlimeRandomPort(0, 1));
Require(partySelection.ItemState == 3737f,
    "The daytime surface birthday-party branch must precede random contained-item rolls.");

NpcBlueSlimeTypeOneSelectionInput remixSelectionInput = surfaceSelectionInput with {
  RemixWorld = true,
};
NpcBlueSlimeTypeOneSelectionResult remixSelection =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in remixSelectionInput,
    new QueueBlueSlimeRandomPort(1, 1, 0));
Require(remixSelection.ItemState == 75f,
    "The Remix Blue Slime branch must retain its first-attempt-only item path.");

NpcBlueSlimeTypeOneSelectionInput skyblockLifeCrystalInput =
  undergroundSelectionInput with {
    IsSkyblockWorld = true,
    LowTiles = true,
    NoLifeCrystals = true,
    PositionInRockLayer = true,
  };
var skyblockLifeCrystalRandom = new QueueBlueSlimeRandomPort(0);
NpcBlueSlimeTypeOneSelectionResult skyblockLifeCrystalSelection =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in skyblockLifeCrystalInput,
    skyblockLifeCrystalRandom);
Require(skyblockLifeCrystalSelection.ItemState == 29f &&
    skyblockLifeCrystalRandom.Consumed == 1,
  "Skyblock low-tile Blue Slimes must roll the source life-crystal variant before other item branches.");

NpcBlueSlimeTypeOneSelectionInput skyblockVoiceItemInput =
  undergroundSelectionInput with {
    IsSkyblockWorld = true,
    LowTiles = true,
    PositionInRockLayer = false,
    AnyLifeCrystalSlime = true,
  };
var skyblockVoiceItemRandom = new QueueBlueSlimeRandomPort(0, 13);
NpcBlueSlimeTypeOneSelectionResult skyblockVoiceItemSelection =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in skyblockVoiceItemInput,
    skyblockVoiceItemRandom);
Require(skyblockVoiceItemSelection.ItemState == 5534f &&
    skyblockVoiceItemRandom.Consumed == 2,
  "Skyblock low-tile Blue Slimes must request the source voice item only after its 1-in-1000 roll succeeds.");

NpcBlueSlimeTypeOneSelectionInput skyblockHelperInput =
  undergroundSelectionInput with {
    IsSkyblockWorld = true,
    LowTiles = true,
    PositionInRockLayer = false,
  };
var skyblockHelperRandom = new QueueBlueSlimeRandomPort(1, 0, 3, 0, 1, 1, 1);
NpcBlueSlimeTypeOneSelectionResult skyblockHelperSelection =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in skyblockHelperInput,
    skyblockHelperRandom);
Require(skyblockHelperSelection.ItemState == 71f &&
    skyblockHelperRandom.Consumed == 7,
  "Skyblock item generation must pass LowTiles through to the helper and preserve its category random draws.");

 int[] skyblockNoItemRolls = Enumerable.Repeat(1, 28).ToArray();
var skyblockAttemptRandom = new QueueBlueSlimeRandomPort(skyblockNoItemRolls);
NpcBlueSlimeTypeOneSelectionInput skyblockAttemptInput =
  skyblockHelperInput with { SlimeRain = true };
NpcBlueSlimeTypeOneSelectionResult skyblockAttemptSelection =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in skyblockAttemptInput,
    skyblockAttemptRandom);
Require(skyblockAttemptSelection.Attempted &&
    skyblockAttemptSelection.ItemState == -1f &&
     skyblockAttemptRandom.Consumed == 28,
  "Skyblock plus Slime Rain must run seven source item attempts before leaving the pending sentinel.");

NpcBlueSlimeTypeOneSelectionInput clientSelectionInput =
  undergroundSelectionInput with { NetMode = 1 };
NpcBlueSlimeTypeOneSelectionResult clientSelection =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in clientSelectionInput,
    new QueueBlueSlimeRandomPort());
Require(!clientSelection.Attempted && !clientSelection.NetUpdateRequested,
  "Blue Slime contained-item selection must remain server-authoritative and consume no client random values.");

NpcBlueSlimeTypeOneSelectionInput alreadySelectedItemInput =
  surfaceSelectionInput with { CurrentItemState = 751f };
NpcBlueSlimeTypeOneSelectionResult alreadySelectedItem =
  NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
    in alreadySelectedItemInput,
    new QueueBlueSlimeRandomPort());
Require(!alreadySelectedItem.Attempted && alreadySelectedItem.ItemState == 751f,
    "A previously selected Blue Slime item state must not consume another random selection.");

NpcFloatingEyeProfileInput floatingEyeInput = new(
    TypeId: 2,
    NetId: 2,
    AiStyle: 2,
    Position: new Vector2(32f, 80f),
    Velocity: new Vector2(0f, 0f),
    OldVelocity: new Vector2(-4f, 3f),
    Width: 30,
    Height: 32,
    Scale: 1f,
    Direction: 1,
    DirectionY: 1,
    TargetSlot: 0,
    NoTileCollide: false,
    CollideX: true,
    CollideY: true,
    DayTime: false,
    ZoneGraveyard: false,
    WorldSurfacePixels: 1600f,
    Wet: false,
    DustRoll: 1);
Require(NpcFloatingEyeProfile.CanHandle(2, 2, 2) &&
    !NpcFloatingEyeProfile.CanHandle(116, 116, 2) &&
    !NpcFloatingEyeProfile.CanHandle(2, 2, 1),
    "Demon Eye source profile must require its concrete type, net id, and aiStyle identity.");
NpcFloatingEyeProfileResult collisionEye = NpcFloatingEyeProfile.Evaluate(
    in floatingEyeInput);
Require(collisionEye.NoGravity &&
    MathF.Abs(collisionEye.Velocity.X - 2.1f) < 0.0001f &&
    MathF.Abs(collisionEye.Velocity.Y + 1.49f) < 0.0001f &&
    (collisionEye.Branches & NpcFloatingEyeSourceBranch.CollisionBounce) != 0 &&
    collisionEye.TargetClosestRequested,
    "Demon Eye profile must apply source collision bounce before generic acceleration and target selection.");

NpcFloatingEyeProfileInput discouragedEyeInput = floatingEyeInput with {
  Position = new Vector2(32f, 80f),
  Velocity = new Vector2(-1f, 0f),
  OldVelocity = Vector2.Zero,
  CollideX = false,
  CollideY = false,
  DayTime = true,
  DustRoll = 1,
};
NpcFloatingEyeProfileResult discouragedEye = NpcFloatingEyeProfile.Evaluate(
    in discouragedEyeInput);
Require(discouragedEye.DespawnEncouragementRequested &&
    !discouragedEye.TargetClosestRequested &&
    discouragedEye.Direction == -1 &&
    discouragedEye.DirectionY == -1 &&
    (discouragedEye.Branches & NpcFloatingEyeSourceBranch.Discouraged) != 0,
    "Demon Eye discouragement must request despawn and set source directions without target reacquire.");

NpcFloatingEyeProfileInput wetEyeInput = floatingEyeInput with {
  Velocity = new Vector2(0f, 2f),
  OldVelocity = Vector2.Zero,
  CollideX = false,
  CollideY = false,
  DayTime = false,
  Wet = true,
  DustRoll = 0,
};
NpcFloatingEyeProfileResult wetEye = NpcFloatingEyeProfile.Evaluate(in wetEyeInput);
Require(MathF.Abs(wetEye.Velocity.Y - 1.4f) < 0.0001f &&
    wetEye.TargetClosestRequested &&
    wetEye.WetTargetClosestRequested &&
    wetEye.DustRequested &&
    (wetEye.Branches & NpcFloatingEyeSourceBranch.WetMovement) != 0,
    "Wet Demon Eye movement must damp positive vertical speed, apply the -0.5 source term, and reacquire after dust.");
var floatingEyeEffects = new RecordingFloatingEyeEffectPort();
NpcFloatingEyeProfile.ApplyEffects(in wetEyeInput, in wetEye, floatingEyeEffects);
Require(floatingEyeEffects.Events.SequenceEqual([
      "target-reacquire",
      "dust",
      "target-reacquire",
    ]),
    "Demon Eye effects must preserve target, dust, then wet target-reacquire order.");

var taskState = new NpcTaskStateComponent();
NpcTaskLifecycleResult enteredTask = NpcTaskLifecycleSystem.Enter(
  taskState,
  NpcTaskKind.GuideDayPatrol);
NpcTaskLifecycleResult reenteredTask = NpcTaskLifecycleSystem.Enter(
  taskState,
  NpcTaskKind.GuideDayPatrol);
Require(reenteredTask.Accepted && !reenteredTask.Changed &&
    reenteredTask.Cursor == enteredTask.Cursor,
    "Re-entering the active NPC task must be idempotent.");
NpcTaskLifecycleResult advancedTask = NpcTaskLifecycleSystem.Advance(
  taskState,
  NpcTaskKind.GuideDayPatrol);
Require(enteredTask.Accepted && enteredTask.Phase == NpcTaskPhase.Running &&
    advancedTask.Cursor == 1,
    "NPC task entry and cross-tick progress must commit a running cursor.");
NpcTaskLifecycleResult completedTask = NpcTaskLifecycleSystem.Complete(
  taskState,
  NpcTaskKind.GuideDayPatrol);
Require(completedTask.Accepted && completedTask.Phase == NpcTaskPhase.Completed,
    "NPC task completion must release the running phase without losing its cursor.");
NpcTaskLifecycleSystem.Enter(taskState, NpcTaskKind.GuideDayPatrol);
NpcTaskLifecycleResult replacedTask = NpcTaskLifecycleSystem.Enter(
  taskState,
  NpcTaskKind.GuideReturnHome);
Require(replacedTask.Accepted && replacedTask.Changed &&
    replacedTask.Kind == NpcTaskKind.GuideReturnHome &&
    replacedTask.Phase == NpcTaskPhase.Running,
    "Replacing a running NPC task must interrupt the old task before starting the new one.");
NpcTaskLifecycleResult failedTask = NpcTaskLifecycleSystem.Fail(
  taskState,
  NpcTaskKind.GuideReturnHome,
    NpcTaskFailureReason.NoPath);
Require(failedTask.Accepted && failedTask.Phase == NpcTaskPhase.Failed &&
    failedTask.FailureReason == NpcTaskFailureReason.NoPath,
    "NPC task failure must preserve an explicit reason.");
NpcTaskLifecycleSystem.Enter(taskState, NpcTaskKind.GuideDayPatrol);
NpcTaskLifecycleResult interruptedTask = NpcTaskLifecycleSystem.Interrupt(
    taskState,
    NpcTaskKind.GuideDayPatrol,
    NpcTaskFailureReason.TargetUnavailable);
Require(interruptedTask.Accepted && interruptedTask.Phase == NpcTaskPhase.Interrupted,
    "NPC task interruption must be observable and scoped to the active task.");
NpcTaskLifecycleResult resetTask = NpcTaskLifecycleSystem.Reset(taskState);
Require(resetTask.Accepted && resetTask.Phase == NpcTaskPhase.Idle &&
    resetTask.Kind == NpcTaskKind.None && resetTask.Cursor == 0 &&
    resetTask.FailureReason == NpcTaskFailureReason.None,
    "NPC task reset must clear the task kind, phase, cursor, and failure reason.");

var floatingEyeRandom = new QueueFloatingEyeRandomPort(0);
NpcFloatingEyeProfileResult randomizedEye = NpcFloatingEyeProfile.EvaluateWithRandom(
    in floatingEyeInput,
    floatingEyeRandom);
Require(randomizedEye.DustRequested && floatingEyeRandom.Consumed == 1,
    "Demon Eye random effects must consume exactly one source Next(40) roll per evaluation.");

NpcFighterProfileInput fighterInput = new(
    TypeId: 3,
    NetId: 3,
    AiStyle: 3,
    Position: Vector2.Zero,
    Velocity: Vector2.Zero,
    State: new NpcFighterProfileState(0f, 0f, 0f, 0f),
    Direction: 1,
    TargetSlot: 0,
    Scale: 1f,
    DayTime: false,
    IsBelowSurface: false,
    IsGrounded: true,
    JustHit: false);
Require(NpcFighterProfile.CanHandle(3, 3, 3) &&
    !NpcFighterProfile.CanHandle(16, 16, 1) &&
    !NpcFighterProfile.CanHandle(3, 3, 1),
    "Fighter source profile must require its concrete type, net id, and aiStyle identity.");
NpcFighterProfileResult fighterAcceleration = NpcFighterProfile.Evaluate(in fighterInput);
Require(fighterAcceleration.TargetClosestRequested &&
    MathF.Abs(fighterAcceleration.Velocity.X - 0.07f) < 0.0001f &&
    (fighterAcceleration.Branches & NpcFighterSourceBranch.HorizontalAcceleration) != 0,
    "Fighter source profile must request target selection and preserve the 0.07 grounded acceleration.");
NpcFighterProfileResult fighterIdleTick = NpcFighterProfile.Evaluate(
    fighterInput with { State = fighterInput.State with { Ai0 = 1f } });
Require(fighterIdleTick.Direction == -1 &&
    fighterIdleTick.State.Ai0 == 0f &&
    fighterIdleTick.NetUpdateRequested &&
    (fighterIdleTick.Branches & NpcFighterSourceBranch.IdleTurn) != 0,
    "Fighter source profile must turn after two grounded idle ticks and request synchronization.");
NpcFighterProfileResult fighterDay = NpcFighterProfile.Evaluate(
  fighterInput with { DayTime = true });
Require(fighterDay.DespawnEncouragementRequested &&
    fighterDay.TargetClosestRequested &&
    (fighterDay.Branches & NpcFighterSourceBranch.DaytimeDespawn) != 0,
    "Fighter source profile must preserve target reacquire while recording daytime surface despawn.");
NpcFighterProfileResult fighterHit = NpcFighterProfile.Evaluate(
  fighterInput with
  {
    State = fighterInput.State with { Ai0 = 1f },
    JustHit = true,
  });
Require(fighterHit.State.Ai0 == 0f && fighterHit.Direction == 1,
    "Fighter source profile must consume a real hit input before the idle-turn counter.");
NpcFighterProfileResult fighterTargetLevel = NpcFighterProfile.Evaluate(
  fighterInput with
  {
    Height = 42,
    TargetCenter = new Vector2(0f, 21f),
    TargetHeight = 42,
    TargetIsAvailable = true,
  });
Require(fighterTargetLevel.DirectionY == -1,
    "Fighter source profile must point upward when the target bottom matches the NPC bottom.");
NpcFighterProfileResult fighterDirectionState = NpcFighterProfile.Evaluate(
  fighterInput with { DirectionY = -1 });
Require(fighterDirectionState.DirectionY == -1,
    "Fighter source profile must preserve the existing vertical direction " +
    "when no target boundary overrides it.");
var fighterHitState = new NpcHitStateComponent();
Require(!fighterHitState.Consume(),
    "Fighter hit state must start clear before a strike is committed.");
fighterHitState.CommitHit();
Require(fighterHitState.Consume() && !fighterHitState.Consume(),
    "Fighter hit state must expose one consumable hit pulse per committed strike.");
NpcFighterProfileResult fighterScaled = NpcFighterProfile.Evaluate(
    fighterInput with { Scale = 0.5f, Velocity = new Vector2(2f, 0f) });
Require(MathF.Abs(fighterScaled.Velocity.X - 1.6f) < 0.0001f,
    "Fighter source profile must apply source scale to its speed cap and grounded damping.");

NpcFighterProfileResult fighterStep = NpcFighterProfile.Evaluate(
    fighterInput with
    {
      Traversal = new NpcFighterTraversalInput(
        SolidTileOneAhead: true,
        SolidTileTwoAhead: false,
        SolidTileThreeAhead: false,
        DoorAhead: false,
        DoorCanOpen: false,
        DoorTileX: 0,
        DoorTileY: 0,
        TargetAbove: false,
        TargetLineOfSight: false,
        ExpertMode: false),
    });
Require(fighterStep.JumpRequested &&
    MathF.Abs(fighterStep.JumpVelocityY + 6f) < 0.0001f &&
    (fighterStep.Branches & NpcFighterSourceBranch.ObstacleJump) != 0,
    "Fighter source profile must emit the source one-tile obstacle jump impulse.");

NpcFighterProfileResult fighterTallStep = NpcFighterProfile.Evaluate(
    fighterInput with
    {
      Traversal = new NpcFighterTraversalInput(
        SolidTileOneAhead: true,
        SolidTileTwoAhead: true,
        SolidTileThreeAhead: true,
        DoorAhead: false,
        DoorCanOpen: false,
        DoorTileX: 0,
        DoorTileY: 0,
        TargetAbove: false,
        TargetLineOfSight: false,
        ExpertMode: false),
    });
Require(fighterTallStep.JumpRequested &&
    MathF.Abs(fighterTallStep.JumpVelocityY + 8f) < 0.0001f,
    "Fighter source profile must raise the larger obstacle jump impulse.");

NpcFighterProfileResult fighterDoor = NpcFighterProfile.Evaluate(
    fighterInput with
    {
      State = fighterInput.State with { Ai2 = 59f },
      Traversal = new NpcFighterTraversalInput(
        SolidTileOneAhead: true,
        SolidTileTwoAhead: true,
        SolidTileThreeAhead: false,
        DoorAhead: true,
        DoorCanOpen: true,
        DoorTileX: 12,
        DoorTileY: 34,
        TargetAbove: false,
        TargetLineOfSight: false,
        ExpertMode: false),
    });
Require(fighterDoor.DoorOpenRequested &&
    fighterDoor.DoorTileX == 12 && fighterDoor.DoorTileY == 34 &&
    fighterDoor.DoorDirection == 1 &&
    fighterDoor.State.Ai2 == 0f &&
    (fighterDoor.Branches & NpcFighterSourceBranch.DoorInteraction) != 0,
    "Fighter source profile must request a door effect after the source wait threshold.");

NpcFighterProfileResult blockedDoor = NpcFighterProfile.Evaluate(
    fighterInput with
    {
      State = fighterInput.State with { Ai2 = 59f },
      Traversal = new NpcFighterTraversalInput(
        SolidTileOneAhead: true,
        SolidTileTwoAhead: false,
        SolidTileThreeAhead: false,
        DoorAhead: true,
        DoorCanOpen: false,
        DoorTileX: 12,
        DoorTileY: 34,
        TargetAbove: false,
        TargetLineOfSight: false,
        ExpertMode: false),
    });
Require(!blockedDoor.DoorOpenRequested && blockedDoor.State.Ai2 == 60f,
    "Fighter source profile must preserve the wait state when the door owner rejects opening.");

RecordingFighterEffectPort fighterEffects = new();
NpcFighterProfile.ApplyEffects(in fighterDoor, fighterEffects);
Require(fighterEffects.Events.SequenceEqual(["target-reacquire", "network-sync", "open-door"]),
    "Fighter effect order must keep target selection, synchronization, and door mutation observable.");

var homeReturnQuery = new ScriptedHomeReturnCollisionQuery((tileX, tileY) =>
  tileX == 11 && tileY is >= 17 and <= 19);
Require(NpcHomeReturnDestinationQuery.TryFindDestination(
      homeTileX: 10,
      homeTileY: 20,
      width: 20,
      height: 40,
      homeReturnQuery,
      out NpcHomeReturnDestination homeDestination) &&
    homeDestination.CandidateOffset == -1 &&
    homeDestination.Position == new Vector2(9 * 16f + 8f - 10f, 20 * 16f - 40f - 0.1f),
    "Guide home return must preserve the source 0,-1,+1 candidate order and destination geometry.");
var blockedHomeReturnQuery = new ScriptedHomeReturnCollisionQuery(static (_, _) => true);
Require(!NpcHomeReturnDestinationQuery.TryFindDestination(
      homeTileX: 10,
      homeTileY: 20,
      width: 20,
      height: 40,
      blockedHomeReturnQuery,
      out _),
    "Guide home return must reject a home when every candidate area is solid.");
var housingEffects = new NpcImmediateEffectStateComponent();
housingEffects.BeginTick();
housingEffects.CommitHousingRevalidationFailure();
housingEffects.CommitHousingRegistrySynchronization();
Require(housingEffects.HousingRevalidationFailed &&
    housingEffects.HousingRegistrySynchronized,
    "Housing revalidation effects must expose invalidation and registry synchronization.");
Console.WriteLine(
    "PASS: NPC AI center, ties, target gates, repeatability, phase boundary, home, task lifecycle, coverage, C1 Blue Slime, C1.3 Mother Slime, C2 Demon Eye, C3 Fighter, C4 Fighter traversal, D2 home return, D3 housing revalidation effects.");

static NpcAiInput CreateInput(
    int netId,
    int style,
    bool town = false,
    IReadOnlyList<NpcAiTargetSnapshot>? targets = null) {
  var definition = new NpcDefinition(netId, netId, $"npc:{netId}", 25, 7, 2, 24, 18,
      style, town, town, !town);
  return new NpcAiInput(definition, 0, Vector2.Zero, Vector2.Zero,
      new NpcAiStateComponent(style, 0f, 0f, 0f, 0f, 0),
      new NpcAiEnvironmentSnapshot(true, true, false, Vector2.Zero), targets ?? []);
}

static NpcTargetSelectionInputs CreateTargetInputs(
    NpcTargetSelectionStrategy strategy,
    IReadOnlyList<NpcPlayerTargetSnapshot> players,
    IReadOnlyList<NpcNpcTargetSnapshot>? npcs = null) {
  return new NpcTargetSelectionInputs(
      strategy,
      new NpcTargetGeometrySnapshot(Vector2.Zero, 20, 20),
      Direction: 1,
      DirectionY: 1,
      OldDirection: 0,
      OldDirectionY: 0,
      OldTarget: -1,
      CollideX: false,
      CollideY: false,
      Confused: false,
      Boss: false,
      FaceTarget: true,
      players,
      npcs ?? []);
}

static NpcPlayerTargetSnapshot CreatePlayerTarget(
    int slot,
    Vector2 position,
    bool gross = true,
    NpcTankPetTargetSnapshot? tankPet = null) {
  return new NpcPlayerTargetSnapshot(
      slot,
      new NpcTargetGeometrySnapshot(position, 20, 20),
      IsActive: true,
      IsDead: false,
      IsGhost: false,
      Aggro: 0,
      NoAggro: false,
      gross,
      ItemAnimation: 0,
      tankPet);
}

static void Require(bool condition, string message) {
  if (!condition) {
    throw new InvalidOperationException(message);
  }
}

sealed class RecordingBlueSlimeEffectPort : INpcBlueSlimeProfileEffectPort {
  public List<string> Events { get; } = [];

  public NpcBlueSlimeTypeOneSelectionResult GenerateContainedItem(bool isBallooned) {
    Events.Add("generate-item");
    return new NpcBlueSlimeTypeOneSelectionResult(
      Attempted: true,
      ItemState: 290f,
      NetUpdateRequested: true);
  }

  public void RequestNetworkSync() {
    Events.Add("network-sync");
  }

  public void RequestTargetReacquire() {
    Events.Add("target-reacquire");
  }
}

sealed class RecordingMotherSlimeEffectPort : INpcMotherSlimeProfileEffectPort {
  public List<string> Events { get; } = [];

  public void RequestNetworkSync() {
    Events.Add("network-sync");
  }

  public void RequestTargetReacquire() {
    Events.Add("target-reacquire");
  }
}

sealed class QueueBlueSlimeRandomPort : INpcBlueSlimeRandomPort {
  private readonly Queue<int> _values;

  public QueueBlueSlimeRandomPort(params int[] values) {
    _values = new Queue<int>(values);
  }

  public int Consumed { get; private set; }

  public int Next(int maxExclusive) {
    if (_values.Count == 0) {
      throw new InvalidOperationException("The Blue Slime random fixture consumed too few scripted values.");
    }

    Consumed++;
    return _values.Dequeue();
  }

  public int Next(int minInclusive, int maxExclusive) {
    return Next(maxExclusive - minInclusive) + minInclusive;
  }

  public int GetRandomVoiceItem() {
    return Next(14) switch {
      1 => 5500,
      2 => 5501,
      3 => 5502,
      4 => 5503,
      5 => 5504,
      6 => 5505,
      7 => 5506,
      8 => 5507,
      9 => 5508,
      10 => 5509,
      11 => 5484,
      12 => 5485,
      13 => 5534,
      _ => 5499,
    };
  }
}

sealed class RecordingFloatingEyeEffectPort : INpcFloatingEyeProfileEffectPort {
  public List<string> Events { get; } = [];

  public void EncourageDespawn(int ticks) {
    Events.Add("despawn");
  }

  public void RequestTargetReacquire() {
    Events.Add("target-reacquire");
  }

  public void SpawnDust(Vector2 position, int width, int height, Vector2 velocity) {
    Events.Add("dust");
  }
}

sealed class QueueFloatingEyeRandomPort : INpcFloatingEyeRandomPort {
  private readonly Queue<int> _values;

  public QueueFloatingEyeRandomPort(params int[] values) {
    _values = new Queue<int>(values);
  }

  public int Consumed { get; private set; }

  public int Next(int maxExclusive) {
    if (_values.Count == 0) {
      throw new InvalidOperationException("The Demon Eye random fixture consumed too few scripted values.");
    }

    Consumed++;
    return _values.Dequeue();
  }
}

sealed class RecordingFighterEffectPort : INpcFighterProfileEffectPort {
  public List<string> Events { get; } = [];

  public void EncourageDespawn(int ticks) {
    Events.Add("despawn");
  }

  public void RequestTargetReacquire() {
    Events.Add("target-reacquire");
  }

  public void RequestNetworkSync() {
    Events.Add("network-sync");
  }

  public void RequestOpenDoor(int tileX, int tileY, int direction) {
    Events.Add("open-door");
  }
}

sealed class ScriptedHomeReturnCollisionQuery : INpcHomeReturnCollisionQuery {
  private readonly Func<int, int, bool> _isSolid;

  public ScriptedHomeReturnCollisionQuery(Func<int, int, bool> isSolid) {
    _isSolid = isSolid;
  }

  public bool IsSolidTile(int tileX, int tileY) {
    return _isSolid(tileX, tileY);
  }
}
