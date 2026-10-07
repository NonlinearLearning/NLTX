using System.Numerics;
using Terraria.Npc;

var clearCollision = new ScriptedCollisionQuery(static (_, _) => false);
NpcGuideSourceProfileInput baseInput = new(
  TypeId: 22,
  NetId: 22,
  AiStyle: 7,
  Position: new Vector2(160f, 160f),
  Velocity: new Vector2(0.7f, 0f),
  State: new NpcGuideSourceProfileState(0f, 0f, 0f, 0f),
  DayTime: true,
  Raining: false,
  Eclipse: false,
  SlimeRain: false,
  IsStorming: false,
  WorldSurface: 100f,
  ServerAuthority: true,
  TownNpc: true,
  Homeless: false,
  InGoodRestingSpot: false,
  CurrentAreaOccupiedByPlayer: false,
  HomeAreaOccupiedByPlayer: false,
  HasHome: true,
  HomeTileX: 10,
  HomeTileY: 20,
  Width: 18,
  Height: 40,
  CollisionQuery: clearCollision,
  SittingCandidateAvailable: false);

Require(
  NpcGuideSourceProfile.CanHandle(22, 22, 7) &&
  !NpcGuideSourceProfile.CanHandle(22, 22, 3) &&
  !NpcGuideSourceProfile.CanHandle(20, 22, 7) &&
  !NpcGuideSourceProfile.CanHandle(22, 20, 7),
  "Guide source profile must require type=22, netID=22, and aiStyle=7.");

NpcGuideSourceProfileResult clear = NpcGuideSourceProfile.Evaluate(in baseInput);
Require(
  !clear.ReturnPressureActive &&
  !clear.HomeReturnEligible &&
  !clear.HomeTeleportRequested,
  "A clear daytime tick must not enter the town return path.");

Require(
  Evaluate(baseInput with { DayTime = false }).ReturnPressureActive &&
  Evaluate(baseInput with { Raining = true }).ReturnPressureActive &&
  Evaluate(baseInput with { Eclipse = true }).ReturnPressureActive &&
  Evaluate(baseInput with { SlimeRain = true }).ReturnPressureActive &&
  Evaluate(baseInput with { IsStorming = true }).ReturnPressureActive &&
  !Evaluate(baseInput with
  {
    IsStorming = true,
    Position = new Vector2(160f, 2_000f),
  }).ReturnPressureActive,
  "Guide return pressure must preserve night, rain, eclipse, slime-rain, and surface-storm rules.");

NpcGuideSourceProfileResult client = Evaluate(
  baseInput with { DayTime = false, ServerAuthority = false });
Require(
  client.ReturnPressureActive &&
  !client.HomeReturnEligible &&
  !client.HomeTeleportRequested,
  "The Guide home teleport gate must remain server authoritative.");

NpcGuideSourceProfileResult homeless = Evaluate(
  baseInput with { DayTime = false, Homeless = true });
Require(!homeless.HomeReturnEligible && !homeless.HomeTeleportRequested,
  "A homeless Guide must not request a home teleport.");

NpcGuideSourceProfileResult occupied = Evaluate(
  baseInput with
  {
    DayTime = false,
    CurrentAreaOccupiedByPlayer = true,
  });
Require(
  occupied.HomeReturnEligible &&
  !occupied.HomeTeleportRequested &&
  (occupied.Branches & NpcGuideSourceBranch.PlayerOccupancyBlocked) != 0,
  "Player occupancy must block the home teleport after eligibility is established.");

NpcGuideSourceProfileResult homeOccupied = Evaluate(
  baseInput with
  {
    DayTime = false,
    HomeAreaOccupiedByPlayer = true,
  });
Require(!homeOccupied.HomeTeleportRequested,
  "Player occupancy at the destination must block the home teleport.");

bool missingCollisionRejected = false;
try
{
  _ = Evaluate(baseInput with { DayTime = false, CollisionQuery = null });
}
catch (ArgumentNullException)
{
  missingCollisionRejected = true;
}
Require(missingCollisionRejected,
  "An eligible Guide home return with no collision query must reject the call contract instead of marking homeless.");

var leftCandidateCollision = new ScriptedCollisionQuery(
  static (tileX, tileY) => tileX == 11 && tileY is >= 17 and <= 19);
NpcGuideSourceProfileResult teleported = Evaluate(
  baseInput with
  {
    DayTime = false,
    CollisionQuery = leftCandidateCollision,
  });
Require(
    teleported.HomeTeleportSucceeded &&
  teleported.HomeTeleportCandidateOffset == -1 &&
  teleported.HomeTeleportPosition == new Vector2(9 * 16f + 8f - 9f, 20 * 16f - 40f - 0.1f) &&
  teleported.Velocity == Vector2.Zero &&
  teleported.ForceSittingRequested,
  "Guide home return must preserve the source 0,-1,+1 candidate order, geometry, zero velocity, and sitting request.");

NpcGuideSourceProfileResult centerTeleported = Evaluate(
  baseInput with { DayTime = false });
Require(centerTeleported.HomeTeleportCandidateOffset == 0,
  "Guide home return must prefer the source center candidate before lateral fallbacks.");

var rightCandidateCollision = new ScriptedCollisionQuery(
  static (tileX, tileY) => tileX == 9 && tileY is >= 17 and <= 19);
NpcGuideSourceProfileResult rightTeleported = Evaluate(
  baseInput with
  {
    DayTime = false,
    CollisionQuery = rightCandidateCollision,
  });
Require(rightTeleported.HomeTeleportCandidateOffset == 1,
  "Guide home return must use the +1 candidate after the center and -1 candidates fail.");

NpcGuideSourceProfileResult oddWidth = Evaluate(
  baseInput with
  {
    DayTime = false,
    Width = 19,
  });
Require(
  oddWidth.HomeTeleportPosition.X == 10 * 16f + 8f - 9,
  "Guide home return must use the source integer width/2 position formula for odd widths.");

var successEffects = new RecordingEffectPort();
NpcGuideSourceEffectApplicationResult successApplication =
  NpcGuideSourceProfile.ApplyEffectsAndObserve(in teleported, successEffects);
Require(
  successEffects.Events.SequenceEqual(["home-teleport", "network-sync", "force-sitting"]) &&
  successApplication.ForceSittingCommitted,
  "Successful Guide home return effects must preserve teleport, network, then the owner sitting capability call.");

NpcGuideSourceProfileResult alreadyHandled = Evaluate(
  baseInput with
  {
    DayTime = false,
    Position = teleported.Position,
    Velocity = teleported.Velocity,
    State = teleported.State,
    InGoodRestingSpot = true,
  });
Require(
  !alreadyHandled.HomeTeleportRequested &&
  !alreadyHandled.HomeReturnEligible,
  "A Guide already in a good resting spot must not repeat its teleport effect on the next tick.");

Require(
  NpcGuideGoodRestingSpotQuery.IsGoodRestingSpot(
    new NpcGuideGoodRestingSpotInput(22, 22, 7, false, 5f, false, 12, 18, 10, 20)) &&
  !NpcGuideGoodRestingSpotQuery.IsGoodRestingSpot(
    new NpcGuideGoodRestingSpotInput(22, 22, 7, false, 5f, false, 18, 20, 10, 20)) &&
  NpcGuideGoodRestingSpotQuery.IsGoodRestingSpot(
    new NpcGuideGoodRestingSpotInput(22, 22, 7, true, 0f, false, 10, 20, 10, 20)) &&
  !NpcGuideGoodRestingSpotQuery.IsGoodRestingSpot(
    new NpcGuideGoodRestingSpotInput(22, 22, 7, true, 0f, false, 11, 20, 10, 20)),
  "Guide good-resting-spot predicate must preserve the source night-sitting radius and daytime exact-match rules.");

var restingTiles = new ScriptedRestingTileQuery();
restingTiles.Set(10, 20, new NpcGuideRestingSpotTile(
  Active: true,
  SolidOrSlopedOrPlatform: true,
  CanBeSatOnForNpc: false,
  Type: 1,
  FrameY: 0));
restingTiles.Set(10, 20, new NpcGuideRestingSpotTile(
  Active: true,
  SolidOrSlopedOrPlatform: true,
  CanBeSatOnForNpc: true,
  Type: 15,
  FrameY: 1));
var restingOccupancy = new ScriptedRestingOccupancyQuery();
NpcGuideRestingSpotSearchInput alternateRestingInput = new(
  22,
  22,
  7,
  HomeTileX: 10,
  HomeTileY: 18,
  MyTileX: 10,
  MyTileY: 18,
  DayTime: false,
  Ai0: 0f,
  IsTownSlime: false,
  MaxTilesY: 100,
  TileQuery: restingTiles,
  OccupancyQuery: restingOccupancy);
NpcGuideRestingSpotSearchResult alternateRestingSpot =
  NpcGuideRestingSpotSearch.Find(in alternateRestingInput);
Require(
  alternateRestingSpot.FloorX == 10 &&
  alternateRestingSpot.FloorY == 21 &&
  alternateRestingSpot.UsedAlternateRestingSpot &&
  !alternateRestingSpot.AlternateRestingSpotOccupied,
  "Guide resting-spot search must descend to the solid floor, select the nearest seat, and apply the source frameY adjustment.");
restingOccupancy.SetOccupied(10, 21, true);
NpcGuideRestingSpotSearchInput occupiedRestingInput = alternateRestingInput;
NpcGuideRestingSpotSearchResult occupiedRestingSpot =
  NpcGuideRestingSpotSearch.Find(in occupiedRestingInput);
Require(
  occupiedRestingSpot.FloorX == 10 &&
  occupiedRestingSpot.FloorY == 20 &&
  !occupiedRestingSpot.UsedAlternateRestingSpot &&
  occupiedRestingSpot.AlternateRestingSpotOccupied,
  "Guide resting-spot search must fall back to the floor when the selected seat is occupied at call time.");

NpcGuideRestingSpotSearchInput sittingRestingInput = alternateRestingInput with
{
  MyTileX = 30,
  MyTileY = 30,
  Ai0 = 5f,
};
NpcGuideRestingSpotSearchResult sittingRestingSpot =
  NpcGuideRestingSpotSearch.Find(in sittingRestingInput);
Require(!sittingRestingSpot.UsedAlternateRestingSpot &&
  sittingRestingSpot.FloorX == 10 && sittingRestingSpot.FloorY == 20,
  "Guide resting-spot search must preserve the current floor when ai0=5.");

var safeWalkTiles = new ScriptedWalkTileQuery();
safeWalkTiles.Set(11, 10, new NpcGuideWalkTile(
  HasLiquid: false,
  IsLava: false,
  IsSolid: true));
NpcGuideWalkPredictionInput safeWalkInput = new(
  22,
  22,
  7,
  MyTileX: 10,
  HomeFloorX: 10,
  Direction: 1,
  IsTownCritter: false,
  IsLikeTownNpc: true,
  Ai1: 40f,
  CurrentlyDrowning: false,
  CanBreatheUnderWater: false,
  TileX: 11,
  TileY: 10,
  Width: 18,
  Height: 40,
  SearchAvoidedByNpc: true,
  StationaryFriendlyNpcAhead: false,
  LandingWouldDrown: false,
  TileQuery: safeWalkTiles);
NpcGuideWalkPredictionResult safeWalk = NpcGuideWalkPrediction.Evaluate(in safeWalkInput);
Require(!safeWalk.KeepWalking && !safeWalk.AvoidFalling &&
  (safeWalk.Branches & NpcGuideWalkPredictionBranch.SolidLanding) != 0,
  "Guide walk prediction must accept a solid landing inside home range.");

NpcGuideWalkPredictionResult outsideRange = EvaluateWalk(
  safeWalkInput with
  {
    MyTileX = 50,
    HomeFloorX = 10,
    Direction = -1,
    TileQuery = null,
  });
Require(!outsideRange.AvoidFalling &&
  (outsideRange.Branches & NpcGuideWalkPredictionBranch.OutsideHomeRange) != 0,
  "Guide walk prediction must stop fall avoidance when returning toward home from outside the ±35 range.");

NpcGuideWalkPredictionResult drowningWalk = EvaluateWalk(
  safeWalkInput with { CurrentlyDrowning = true });
Require(drowningWalk.KeepWalking &&
  (drowningWalk.Branches & NpcGuideWalkPredictionBranch.DrowningKeepWalking) != 0,
  "Guide walk prediction must keep walking while currently drowning.");

NpcGuideWalkPredictionResult crowdWalk = EvaluateWalk(
  safeWalkInput with
  {
    Ai1 = 10f,
    SearchAvoidedByNpc = false,
    StationaryFriendlyNpcAhead = true,
  });
Require(crowdWalk.KeepWalking &&
  (crowdWalk.Branches & NpcGuideWalkPredictionBranch.CrowdKeepWalking) != 0,
  "Guide walk prediction must preserve the source stationary-friendly-NPC crowd rule.");

var liquidLandingTiles = new ScriptedWalkTileQuery();
liquidLandingTiles.Set(11, 9, new NpcGuideWalkTile(true, false, false));
liquidLandingTiles.Set(11, 10, new NpcGuideWalkTile(false, false, true));
NpcGuideWalkPredictionResult drowningLanding = EvaluateWalk(
  safeWalkInput with
  {
    TileQuery = liquidLandingTiles,
    LandingWouldDrown = true,
  });
Require(drowningLanding.AvoidFalling &&
  (drowningLanding.Branches & NpcGuideWalkPredictionBranch.LandingDrownRisk) != 0,
  "Guide walk prediction must reject a solid landing whose liquid approach would drown the Guide.");

var lavaTiles = new ScriptedWalkTileQuery();
lavaTiles.Set(11, 9, new NpcGuideWalkTile(true, true, false));
NpcGuideWalkPredictionResult lavaWalk = EvaluateWalk(
  safeWalkInput with { TileQuery = lavaTiles });
Require(lavaWalk.AvoidFalling &&
  (lavaWalk.Branches & NpcGuideWalkPredictionBranch.LavaRisk) != 0,
  "Guide walk prediction must preserve the source lava risk branch.");

NpcGuideConversationInput conversationInput = new(
    22,
    22,
    7,
    new NpcGuideSourceProfileState(1f, 20f, 0f, 0f),
    Direction: 1,
    PlayerIsTalking: true,
    PlayerCenterX: 90f,
    NpcCenterX: 100f);
NpcGuideConversationResult conversation =
  NpcGuideConversationProfile.Evaluate(in conversationInput);
Require(conversation.State.Ai0 == 0f &&
  conversation.State.Ai1 == 300f &&
  conversation.State.LocalAi3 == 100f &&
  conversation.Direction == -1 &&
  conversation.NetworkUpdateRequested,
  "Guide conversation must reset ordinary state, face the player, and request synchronization.");

NpcGuideConversationInput specialConversationInput = new(
    22,
    22,
    7,
    new NpcGuideSourceProfileState(10f, 20f, 0f, 0f),
    Direction: 1,
    PlayerIsTalking: true,
    PlayerCenterX: 90f,
    NpcCenterX: 100f);
NpcGuideConversationResult specialConversation =
  NpcGuideConversationProfile.Evaluate(in specialConversationInput);
Require(specialConversation.State.Ai0 == 10f &&
  !specialConversation.NetworkUpdateRequested &&
  (specialConversation.Branches & NpcGuideConversationBranch.SpecialStatePreserved) != 0,
  "Guide conversation must preserve specialized ai0 conversation states.");

var dangerScan = NpcGuideDangerScan.Evaluate(
  new NpcGuideDangerScanInput(
    22,
    22,
    7,
    CenterX: 100f,
    PlayerTalking: false,
    AttackTypeRequiresExtendedRange: false,
    DangerDetectRange: 200f,
    Npcs: [
      new NpcGuideDangerNpcSnapshot(
        EntityId: 41,
        Active: true,
        CritterThatCanTurnOnPlayers: false,
        TypeId: 1,
        Friendly: false,
        Damage: 20,
        Stinky: false,
        NoTileCollide: true,
        IsSelf: false,
        CanBeChasedBy: true,
        Distance: 30f,
        CenterX: 70f,
        CanHit: true),
      new NpcGuideDangerNpcSnapshot(
        EntityId: 42,
        Active: true,
        CritterThatCanTurnOnPlayers: false,
        TypeId: 2,
        Friendly: false,
        Damage: 20,
        Stinky: true,
        NoTileCollide: true,
        IsSelf: false,
        CanBeChasedBy: true,
        Distance: 60f,
        CenterX: 160f,
        CanHit: true)],
    Players: []));
Require(dangerScan.DangerDetected &&
  dangerScan.DangerWithinBaseRange &&
  dangerScan.StinkyDanger &&
  dangerScan.LeftChaseEntityId == 41 &&
  dangerScan.RightChaseEntityId == 42,
  "Guide danger scan must retain danger flags and nearest left/right chase candidates.");

NpcGuideDangerScanResult extendedOnlyDanger = NpcGuideDangerScan.Evaluate(
  new NpcGuideDangerScanInput(
    22,
    22,
    7,
    CenterX: 100f,
    PlayerTalking: false,
    AttackTypeRequiresExtendedRange: true,
    DangerDetectRange: 200f,
    Npcs: [new NpcGuideDangerNpcSnapshot(
      EntityId: 43,
      Active: true,
      CritterThatCanTurnOnPlayers: false,
      TypeId: 3,
      Friendly: false,
      Damage: 20,
      Stinky: false,
      NoTileCollide: true,
      IsSelf: false,
      CanBeChasedBy: true,
      Distance: 220f,
      CenterX: 320f,
      CanHit: true)],
    Players: []));
Require(extendedOnlyDanger.DangerDetected &&
  !extendedOnlyDanger.DangerWithinBaseRange &&
  extendedOnlyDanger.LeftNearestOffset == -1f &&
  extendedOnlyDanger.RightNearestOffset == -1f,
  "Guide danger scan must preserve flag15 without promoting an extended-range-only target to flag16.");

NpcGuideDangerScanResult stinkyPlayerDanger = NpcGuideDangerScan.Evaluate(
  new NpcGuideDangerScanInput(
    22,
    22,
    7,
    CenterX: 100f,
    PlayerTalking: false,
    AttackTypeRequiresExtendedRange: false,
    DangerDetectRange: 200f,
    Npcs: [],
    Players: [new NpcGuideDangerPlayerSnapshot(7, true, false, true, 80f, 20f)]));
Require(stinkyPlayerDanger.DangerWithinBaseRange &&
  stinkyPlayerDanger.StinkyDanger &&
  stinkyPlayerDanger.LeftChaseEntityId == 7,
  "Guide danger scan must fall back to a nearby stinky player when no NPC is in base range.");

NpcGuideDangerProfileInput dangerInput = new(
  22,
  22,
  7,
  new NpcGuideSourceProfileState(0f, 0f, 0f, 0f),
  Direction: -1,
  ServerAuthority: true,
  InfectedSeed: false,
  PlayerTalking: false,
  ConversationPartnerEntityId: -1,
  ConversationPartnerActive: false,
  PrettySafeDistance: -1f,
  WalkAvoidFalling: false,
  dangerScan);
NpcGuideDangerProfileResult dangerWalk =
  NpcGuideDangerProfile.EvaluateWithRandom(
    in dangerInput,
    new QueueGuideRandomPort(7));
Require(dangerWalk.State.Ai0 == 1f &&
  dangerWalk.State.Ai1 == 127f &&
  dangerWalk.Direction == 1 &&
  dangerWalk.NetworkUpdateRequested,
  "Guide danger response must enter ai0=1, consume Next(120), and turn away from the nearest threat.");

NpcGuideDangerProfileInput aiEightInput = dangerInput with
{
  State = new NpcGuideSourceProfileState(8f, 0f, 0f, 0f),
  Direction = 1,
};
NpcGuideDangerProfileResult aiEightDanger = NpcGuideDangerProfile.EvaluateWithRandom(
  in aiEightInput,
  new QueueGuideRandomPort(12));
Require(aiEightDanger.State.Ai0 == 1f &&
  aiEightDanger.State.Ai1 == 312f &&
  (aiEightDanger.Branches & NpcGuideDangerProfileBranch.AiEightReturnedToWalk) != 0,
  "Guide ai0=8 danger response must use the source 300+Next(300) return timer.");

NpcGuideAiEightResult aiEightRefresh =
  NpcGuideAiEightProfile.EvaluateWithRandom(
    new NpcGuideAiEightInput(
      22,
      22,
      7,
      new NpcGuideSourceProfileState(8f, 60f, 0f, 0f),
      new Vector2(2f, 0f),
      DangerWithinBaseRange: true),
    new QueueGuideRandomPort(4));
Require(aiEightRefresh.State.Ai1 == 180f &&
  aiEightRefresh.Velocity.X == 1.6f &&
  aiEightRefresh.NetworkUpdateRequested &&
  (aiEightRefresh.Branches & NpcGuideAiEightBranch.DangerTimerRefreshed) != 0,
  "Guide ai0=8 continuation must damp velocity and refresh a nearly expired danger timer to 180.");

NpcGuideAiEightResult aiEightExpired =
  NpcGuideAiEightProfile.EvaluateWithRandom(
    new NpcGuideAiEightInput(
      22,
      22,
      7,
      new NpcGuideSourceProfileState(8f, 1f, 9f, 4f),
      new Vector2(-1f, 0f),
      DangerWithinBaseRange: false),
    new QueueGuideRandomPort(4, 5));
Require(aiEightExpired.State.Ai0 == 0f &&
  aiEightExpired.State.Ai1 == 64f &&
  aiEightExpired.State.Ai2 == 0f &&
  aiEightExpired.State.LocalAi3 == 35f &&
  aiEightExpired.Velocity.X == -0.8f &&
  (aiEightExpired.Branches & NpcGuideAiEightBranch.ExpiredToIdle) != 0,
  "Guide ai0=8 continuation must return to idle with the source 60+Next(60) and 30+Next(60) timers.");

NpcGuideAiEightResult nonAiEight =
  NpcGuideAiEightProfile.EvaluateWithRandom(
    new NpcGuideAiEightInput(
      22,
      22,
      7,
      new NpcGuideSourceProfileState(1f, 20f, 0f, 0f),
      new Vector2(3f, 0f),
      DangerWithinBaseRange: true),
    new QueueGuideRandomPort());
Require(nonAiEight.State.Ai0 == 1f &&
  nonAiEight.Velocity.X == 3f &&
  nonAiEight.Branches == NpcGuideAiEightBranch.None,
  "Guide ai0=8 continuation must leave non-ai0=8 states untouched.");

NpcGuideSittingResult sittingTick =
  NpcGuideSittingProfile.EvaluateWithRandom(
    new NpcGuideSittingInput(
      22,
      22,
      7,
      EntityId: 31,
      State: new NpcGuideSourceProfileState(5f, 20f, 4f, 2f),
      Velocity: new Vector2(0.5f, 0f),
      SittingTileX: 11,
      SittingTileY: 21,
      SittingTileIsChairOrBench: true),
    new QueueGuideRandomPort());
Require(sittingTick.State.Ai1 == 19f &&
  sittingTick.Velocity.X == 0.4f &&
  sittingTick.SittingRequest is NpcGuideSittingRequest sittingRequest &&
  sittingRequest.EntityId == 31 &&
  sittingRequest.TileX == 11 &&
  sittingRequest.TileY == 21,
  "Guide sitting tick must damp velocity, decrement ai1, and register the valid seat.");
var sittingEffects = new RecordingSittingEffectPort();
NpcGuideSittingProfile.ApplyEffects(in sittingTick, sittingEffects);
Require(sittingEffects.Events.SequenceEqual(["sitting"]),
  "Guide sitting registration must remain an explicit owner effect.");

NpcGuideSittingResult invalidSittingTick =
  NpcGuideSittingProfile.EvaluateWithRandom(
    new NpcGuideSittingInput(
      22,
      22,
      7,
      EntityId: 31,
      State: new NpcGuideSourceProfileState(5f, 1f, 4f, 2f),
      Velocity: new Vector2(-1f, 0f),
      SittingTileX: 11,
      SittingTileY: 21,
      SittingTileIsChairOrBench: false),
    new QueueGuideRandomPort(4, 5));
Require(invalidSittingTick.State.Ai0 == 0f &&
  invalidSittingTick.State.Ai1 == 64f &&
  invalidSittingTick.State.LocalAi3 == 35f &&
  invalidSittingTick.SittingRequest is null &&
  invalidSittingTick.NetworkUpdateRequested,
  "Guide sitting tick must expire immediately when the source seat is no longer chair or bench.");

NpcGuideConversationTickResult conversationTick =
  NpcGuideConversationTickProfile.EvaluateWithRandom(
    new NpcGuideConversationTickInput(
      22,
      22,
      7,
      new NpcGuideSourceProfileState(6f, 5f, 12f, 0f),
      new Vector2(1f, 0f),
      Direction: -1,
      ConversationTargetValid: true,
      NpcCenterX: 100f,
      TargetCenterX: 150f),
    new QueueGuideRandomPort());
Require(conversationTick.State.Ai1 == 4f &&
  conversationTick.Velocity.X == 0.8f &&
  conversationTick.Direction == 1 &&
  conversationTick.NetworkUpdateRequested &&
  (conversationTick.Branches & NpcGuideConversationTickBranch.FacingTarget) != 0,
  "Guide conversation tick must damp velocity, decrement ai1, and face a valid target.");

NpcGuideConversationTickResult expiredConversationTick =
  NpcGuideConversationTickProfile.EvaluateWithRandom(
    new NpcGuideConversationTickInput(
      22,
      22,
      7,
      new NpcGuideSourceProfileState(18f, 1f, 12f, 0f),
      new Vector2(-1f, 0f),
      Direction: 1,
      ConversationTargetValid: false,
      NpcCenterX: 100f,
      TargetCenterX: 150f),
    new QueueGuideRandomPort(4, 5));
Require(expiredConversationTick.State.Ai0 == 0f &&
  expiredConversationTick.State.Ai1 == 64f &&
  expiredConversationTick.State.LocalAi3 == 35f &&
  (expiredConversationTick.Branches & NpcGuideConversationTickBranch.LocalAi3Clamped) != 0 &&
  (expiredConversationTick.Branches & NpcGuideConversationTickBranch.ConversationTargetInvalid) != 0,
  "Guide conversation tick must clamp ai0=18 localAI3 and expire when its player target is invalid.");

NpcGuideConversationTickResult nonConversationTick =
  NpcGuideConversationTickProfile.EvaluateWithRandom(
    new NpcGuideConversationTickInput(
      22,
      22,
      7,
      new NpcGuideSourceProfileState(1f, 20f, 0f, 0f),
      new Vector2(3f, 0f),
      Direction: -1,
      ConversationTargetValid: true,
      NpcCenterX: 100f,
      TargetCenterX: 150f),
    new QueueGuideRandomPort());
Require(nonConversationTick.State.Ai0 == 1f &&
  nonConversationTick.Velocity.X == 3f &&
  nonConversationTick.Branches == NpcGuideConversationTickBranch.None,
  "Guide conversation tick must leave non-conversation states untouched.");

NpcGuideDangerProfileInput prettySafeInput = dangerInput with
{
  PrettySafeDistance = 20f,
};
NpcGuideDangerProfileResult prettySafeDanger =
  NpcGuideDangerProfile.Evaluate(in prettySafeInput);
Require(prettySafeDanger.State.Ai0 == 0f &&
  (prettySafeDanger.Branches & NpcGuideDangerProfileBranch.PrettySafeSuppressed) != 0,
  "Guide PrettySafe must suppress danger state entry when the nearest threat is farther away.");

NpcGuideDangerProfileInput partnerDangerInput = dangerInput with
{
  State = new NpcGuideSourceProfileState(3f, 0f, 0f, 0f),
  Direction = -1,
  ConversationPartnerEntityId = 41,
  ConversationPartnerActive = true,
};
var partnerDanger = NpcGuideDangerProfile.EvaluateWithRandom(
  in partnerDangerInput,
  new QueueGuideRandomPort(8, 9));
Require(partnerDanger.PartnerReset is NpcGuidePartnerStateResetRequest partnerReset &&
  partnerReset.EntityId == 41 &&
  partnerReset.State.Ai1 == 128f &&
  partnerDanger.State.Ai1 == 129f,
  "Guide danger response must expose partner reset before its own random walk timer.");
var dangerEffects = new RecordingDangerEffectPort();
NpcGuideDangerProfile.ApplyEffects(in partnerDanger, dangerEffects);
Require(dangerEffects.Events.SequenceEqual(["partner-reset", "network-sync"]),
  "Guide danger effects must apply partner reset before the Guide network update.");

NpcGuideAttackConfigurationResult normalAttack =
  NpcGuideAttackConfigurationProfile.Evaluate(
    new NpcGuideAttackConfigurationInput(
      22,
      22,
      7,
      HardMode: false,
      AttackTime: 30f,
      AttackTimer: 30f,
      LocalAi3: 0f,
      DamageScale: 1.5f,
      ServerAuthority: true));
Require(normalAttack.Configuration.ProjectileType == 1 &&
  normalAttack.Configuration.ProjectileSpeed == 10f &&
  normalAttack.Configuration.BaseDamage == 12 &&
  normalAttack.Configuration.AttackDamage == 18 &&
  normalAttack.Configuration.CooldownBase == 30 &&
  normalAttack.Configuration.CooldownRandomExclusive == 20 &&
  normalAttack.ProjectileSpawnWindowOpen &&
  normalAttack.FrameResetRequested,
  "Guide normal attack configuration must preserve projectile, timer, damage, speed, knockback, and spread inputs.");

var attackRandom = new QueueGuideRandomPort();
attackRandom.SetFloatValues(0.1f, -0.2f);
NpcGuideAttackConfigurationResult projectileAttack =
  NpcGuideAttackConfigurationProfile.EvaluateWithRandom(
    new NpcGuideAttackConfigurationInput(
      22,
      22,
      7,
      HardMode: false,
      AttackTime: 30f,
      AttackTimer: 10f,
      LocalAi3: 0f,
      DamageScale: 1f,
      ServerAuthority: true,
      Velocity: new Vector2(1f, 2f),
      NpcCenter: new Vector2(100f, 100f),
      SpriteDirection: 1,
      SelectedTargetAvailable: true,
      SelectedTargetCenter: new Vector2(180f, 100f)),
    attackRandom);
Require(projectileAttack.ProjectileSpawnRequested &&
  projectileAttack.ProjectileSpawn.ProjectileType == 1 &&
  projectileAttack.ProjectileSpawn.Damage == 12 &&
  projectileAttack.ProjectileSpawn.Position == new Vector2(116f, 98f) &&
  projectileAttack.ProjectileSpawn.Velocity.X > 9f &&
  projectileAttack.ProjectileSpawn.Velocity.Y < 0f &&
  projectileAttack.Velocity == new Vector2(0.8f, 2f),
  "Guide attack launch must preserve target aim, spawn geometry, spread port, damage, and horizontal damping.");
var attackEffects = new RecordingAttackEffectPort();
NpcGuideAttackConfigurationProfile.ApplyEffects(in projectileAttack, attackEffects);
Require(attackEffects.Events.SequenceEqual(["projectile"]),
  "Guide attack launch must expose projectile creation through the effect port.");

NpcGuideAttackConfigurationResult cycleAttack =
  NpcGuideAttackConfigurationProfile.EvaluateWithRandom(
    new NpcGuideAttackConfigurationInput(
      22,
      22,
      7,
      HardMode: false,
      AttackTime: 30f,
      AttackTimer: 1f,
      LocalAi3: 4f,
      DamageScale: 1f,
      ServerAuthority: false,
      LocalAi2: 8f,
      DangerWithinBaseRange: true),
    new QueueGuideRandomPort(3, 7));
Require(cycleAttack.CycleCompletionReached &&
  cycleAttack.NextState.Ai0 == 8f &&
  cycleAttack.NextState.Ai1 == 33f &&
  cycleAttack.NextState.LocalAi2 == 8f &&
  cycleAttack.NextState.LocalAi3 == 22f &&
  cycleAttack.NextLocalAi1 == 22f &&
  cycleAttack.NetworkUpdateRequested,
  "Guide attack cycle completion must preserve the source cooldown randomization and ai0=8 return state.");
var cycleEffects = new RecordingAttackEffectPort();
NpcGuideAttackConfigurationProfile.ApplyEffects(in cycleAttack, cycleEffects);
Require(cycleEffects.Events.SequenceEqual(["network-sync"]),
  "Guide attack cycle completion must request network synchronization after state randomization.");

NpcGuideDayMovementInput atHomeMovement = new(
  22,
  22,
  7,
  new NpcGuideSourceProfileState(0f, 0f, 0f, 0f),
  new Vector2(0.05f, 0f),
  Direction: 1,
  ServerAuthority: true,
  ReturnPressureActive: true,
  PlayerTalking: false,
  TownCritter: false,
  Homeless: false,
  InGoodRestingSpot: false,
  Stinky: false,
  DrownCollision: false,
  DungeonTile: false,
  MyTileX: 10,
  MyTileY: 20,
  HomeFloorX: 10,
  HomeFloorY: 20,
  Position: new Vector2(160f, 320f));
NpcGuideDayMovementResult atHomeResult =
  NpcGuideDayMovementProfile.EvaluateWithRandom(
    in atHomeMovement,
    new QueueGuideRandomPort());
Require(atHomeResult.Velocity.X == 0f &&
  atHomeResult.ForceSittingRequested &&
  (atHomeResult.Branches & NpcGuideDayMovementBranch.HomeFloorDamping) != 0,
  "Guide ai0=0 at the home floor must decelerate to zero and request ForceSitting.");
var atHomeEffects = new RecordingDayMovementEffectPort();
NpcGuideDayMovementProfile.ApplyEffects(in atHomeResult, atHomeEffects);
Require(atHomeEffects.Events.SequenceEqual(["force-sitting"]),
  "Guide day movement must expose ForceSitting through the existing owner effect port.");

NpcGuideDayMovementInput homeWalkInput = atHomeMovement with
{
  MyTileX = 20,
  Position = new Vector2(320f, 320f),
};
NpcGuideDayMovementResult homeWalkEntry =
  NpcGuideDayMovementProfile.EvaluateWithRandom(
    in homeWalkInput,
    new QueueGuideRandomPort(5));
Require(homeWalkEntry.State.Ai0 == 1f &&
  homeWalkEntry.State.Ai1 == 205f &&
  homeWalkEntry.Direction == -1 &&
  homeWalkEntry.NetworkUpdateRequested,
  "Guide ai0=0 away from home must enter ai0=1 with a 200+Next(200) timer and home-facing direction.");

NpcGuideDayMovementInput restingWalk = atHomeMovement with
{
  State = new NpcGuideSourceProfileState(1f, 40f, 0f, 0f),
  InGoodRestingSpot = true,
};
NpcGuideDayMovementResult restingWalkResult =
  NpcGuideDayMovementProfile.EvaluateWithRandom(
    in restingWalk,
    new QueueGuideRandomPort(3));
Require(restingWalkResult.State.Ai0 == 0f &&
  restingWalkResult.State.Ai1 == 203f &&
  restingWalkResult.State.LocalAi3 == 60f &&
  restingWalkResult.NetworkUpdateRequested,
  "Guide ai0=1 at a good resting spot must return to ai0=0 with the source timer reset.");

NpcGuideDayMovementInput expiringWalk = atHomeMovement with
{
  State = new NpcGuideSourceProfileState(1f, 1f, 0f, 0f),
  MyTileX = 10,
  Position = new Vector2(160f, 320f),
};
NpcGuideDayMovementResult expiringWalkResult =
  NpcGuideDayMovementProfile.EvaluateWithRandom(
    in expiringWalk,
    new QueueGuideRandomPort(4));
Require(expiringWalkResult.State.Ai0 == 0f &&
  expiringWalkResult.State.Ai1 == 304f &&
  expiringWalkResult.State.LocalAi3 == 60f &&
  expiringWalkResult.Velocity.X > 0f,
  "Guide ai0=1 timer expiry must return to ai0=0 and preserve the 300+Next(300) idle timer.");

NpcGuideDayMovementInput drowningWalkInput = expiringWalk with
{
  State = new NpcGuideSourceProfileState(1f, 20f, 0f, 0f),
  DrownCollision = true,
  Velocity = new Vector2(0f, 0f),
};
NpcGuideDayMovementResult drowningMovement =
  NpcGuideDayMovementProfile.EvaluateWithRandom(
    in drowningWalkInput,
    new QueueGuideRandomPort());
Require(drowningMovement.State.Ai1 == 20f &&
  drowningMovement.Velocity.X > 0f &&
  (drowningMovement.Branches & NpcGuideDayMovementBranch.DrownTimerPreserved) != 0,
  "Guide ai0=1 drowning state must preserve its timer while continuing horizontal movement.");

NpcGuideTraversalInput traversalBase = new(
  TypeId: 22,
  NetId: 22,
  AiStyle: 7,
  State: new NpcGuideSourceProfileState(1f, 50f, 0f, -1f),
  Position: new Vector2(160f, 320f),
  Velocity: new Vector2(1f, 0f),
  Direction: 1,
  ServerAuthority: true,
  ReturnPressureActive: true,
  DangerWithinBaseRange: false,
  Wet: false,
  DrownCollision: false,
  WillDrown: false,
  IsLikeTownNpc: true,
  LiquidDepthTiles: 0,
  SolidSupportCount: 3,
  AvoidFalling: false,
  KeepWalking: false,
  ThreeTilesAboveSolid: false,
  ThreeTileCollisionClear: false,
  TwoTilesAboveSolid: false,
  TwoTileCollisionClear: false,
  OneTileAboveSolid: false,
  OneTileCollisionClear: false,
  OneTileHasSlope: false,
  DoorCandidate: false,
  DoorType: 0,
  DoorTileX: 0,
  DoorTileY: 0,
  OpenDoorWithDirectionSucceeded: false,
  OpenDoorAgainstDirectionSucceeded: false,
  OpenTallGateSucceeded: false,
  CloseDoorPending: false,
  CloseDoorSucceeded: false,
  CloseTallGateSucceeded: false,
  OutsideClosingRange: false,
  IsGrounded: true);
NpcGuideTraversalInput jumpInput = traversalBase with
{
  ThreeTilesAboveSolid = true,
  ThreeTileCollisionClear = true,
};
NpcGuideTraversalResult jumpTraversal =
  NpcGuideTraversalProfile.EvaluateWithRandom(
    in jumpInput,
    new QueueGuideRandomPort());
Require(jumpTraversal.Velocity.Y == -6f &&
  jumpTraversal.State.LocalAi3 == jumpInput.Position.X &&
  (jumpTraversal.Branches & NpcGuideTraversalBranch.JumpThreeTiles) != 0 &&
  jumpTraversal.NetworkUpdateRequested,
  "Guide traversal must preserve the source three-tile jump velocity, landing marker, and network request.");

NpcGuideTraversalInput obstructionInput = traversalBase with
{
  DangerWithinBaseRange = true,
  ThreeTilesAboveSolid = true,
  ThreeTileCollisionClear = false,
};
NpcGuideTraversalResult obstructionTraversal =
  NpcGuideTraversalProfile.EvaluateWithRandom(
    in obstructionInput,
    new QueueGuideRandomPort());
Require(obstructionTraversal.State.Ai0 == 8f &&
  obstructionTraversal.State.Ai1 == 240f &&
  obstructionTraversal.Velocity.X == 0f &&
  (obstructionTraversal.Branches & NpcGuideTraversalBranch.ObstructionState) != 0,
  "Guide traversal must enter ai0=8 when a dangerous obstacle cannot be safely crossed.");

NpcGuideTraversalInput obstructionKeepWalkingInput = obstructionInput with
{
  KeepWalking = true,
};
NpcGuideTraversalResult obstructionKeepWalkingTraversal =
  NpcGuideTraversalProfile.EvaluateWithRandom(
    in obstructionKeepWalkingInput,
    new QueueGuideRandomPort());
Require(obstructionKeepWalkingTraversal.State.Ai1 == 240f,
  "Guide traversal obstruction state must suppress the keep-walking timer override.");

NpcGuideTraversalInput doorInput = traversalBase with
{
  State = new NpcGuideSourceProfileState(1f, 20f, 0f, -1f),
  DoorCandidate = true,
  DoorType = 10,
  DoorTileX = 12,
  DoorTileY = 19,
  OpenDoorWithDirectionSucceeded = true,
};
NpcGuideTraversalResult openDoorTraversal =
  NpcGuideTraversalProfile.EvaluateWithRandom(
    in doorInput,
    new QueueGuideRandomPort(0));
Require(openDoorTraversal.State.Ai1 == 100f &&
  openDoorTraversal.DoorEffect is NpcGuideDoorEffectRequest doorEffect &&
  doorEffect.Kind == NpcGuideDoorActionKind.OpenDoor &&
  doorEffect.Direction == 1,
  "Guide traversal must open a source door in the current direction and add 80 to ai1.");
var traversalEffects = new RecordingTraversalEffectPort();
NpcGuideTraversalProfile.ApplyEffects(in openDoorTraversal, traversalEffects);
Require(traversalEffects.Events.SequenceEqual(["door", "network-sync"]),
  "Guide traversal door effects must precede the network update request.");

NpcGuideTraversalInput drowningTraversalInput = traversalBase with
{
  Wet = true,
  DrownCollision = true,
  WillDrown = true,
  LiquidDepthTiles = 3,
  State = new NpcGuideSourceProfileState(1f, 20f, 0f, 0f),
};
NpcGuideTraversalResult drowningTraversal =
  NpcGuideTraversalProfile.EvaluateWithRandom(
    in drowningTraversalInput,
    new QueueGuideRandomPort());
Require(drowningTraversal.Velocity.Y < 0f &&
  drowningTraversal.State.LocalAi3 == drowningTraversalInput.Position.X &&
  (drowningTraversal.Branches & NpcGuideTraversalBranch.DrowningEscape) != 0,
  "Guide traversal must produce the source liquid-depth drowning escape impulse and landing marker.");

var noDoorRandom = new QueueGuideRandomPort(7);
NpcGuideTraversalInput noDoorInput = traversalBase with
{
  ReturnPressureActive = false,
};
_ = NpcGuideTraversalProfile.EvaluateWithRandom(in noDoorInput, noDoorRandom);
Require(noDoorRandom.Calls == 0,
  "Guide traversal must not consume the door roll when no door candidate exists.");

NpcGuideTraversalInput closeDoorInput = traversalBase with
{
  CloseDoorPending = true,
  CloseDoorSucceeded = true,
  DoorTileX = 12,
  DoorTileY = 19,
  OutsideClosingRange = true,
};
NpcGuideTraversalResult closeDoorTraversal =
  NpcGuideTraversalProfile.EvaluateWithRandom(
    in closeDoorInput,
    new QueueGuideRandomPort());
Require(closeDoorTraversal.CloseDoorConsumed &&
  closeDoorTraversal.CloseDoorEffect is NpcGuideDoorEffectRequest closeDoorEffect &&
  closeDoorEffect.Kind == NpcGuideDoorActionKind.CloseDoor,
  "Guide traversal must emit the source close-door request outside the closing range.");

NpcGuideTraversalInput tallGateInput = doorInput with
{
  OpenDoorWithDirectionSucceeded = false,
  OpenDoorAgainstDirectionSucceeded = false,
  OpenTallGateSucceeded = true,
};
NpcGuideTraversalResult tallGateTraversal =
  NpcGuideTraversalProfile.EvaluateWithRandom(
    in tallGateInput,
    new QueueGuideRandomPort(0));
Require(tallGateTraversal.DoorEffect is NpcGuideDoorEffectRequest tallGateEffect &&
  tallGateEffect.Kind == NpcGuideDoorActionKind.OpenTallGate &&
  tallGateTraversal.State.Ai1 == 100f,
  "Guide traversal must use the source tall-gate fallback after both door directions fail.");

NpcGuideTraversalInput avoidFallingInput = traversalBase with
{
  AvoidFalling = true,
  SolidSupportCount = 2,
};
NpcGuideTraversalResult avoidFallingTraversal =
  NpcGuideTraversalProfile.EvaluateWithRandom(
    in avoidFallingInput,
    new QueueGuideRandomPort(7));
Require(avoidFallingTraversal.State.Ai0 == 0f &&
  avoidFallingTraversal.State.Ai1 == 57f &&
  avoidFallingTraversal.State.LocalAi3 == -1f,
  "Guide traversal must preserve the source avoid-falling recovery state, timer, and later liquid-marker reset.");

NpcGuideAttackConfigurationResult hardmodeAttack =
  NpcGuideAttackConfigurationProfile.Evaluate(
    new NpcGuideAttackConfigurationInput(
      22,
      22,
      7,
      HardMode: true,
      AttackTime: 60f,
      AttackTimer: 20f,
      LocalAi3: 1f,
      DamageScale: 1f,
      ServerAuthority: false));
Require(hardmodeAttack.Configuration.ProjectileType == 2 &&
  hardmodeAttack.Configuration.BaseDamage == 18 &&
  hardmodeAttack.Configuration.CooldownBase == 15 &&
  hardmodeAttack.Configuration.CooldownRandomExclusive == 10 &&
  !hardmodeAttack.ProjectileSpawnWindowOpen,
  "Guide hardmode attack configuration must switch to projectile 2 and its shorter cooldown.");

var dialogueRandom = new QueueGuideRandomPort(1, 0);
NpcGuideDialogueResult chatter2 = NpcGuideDialogueProfile.EvaluateWithRandom(
  new NpcGuideDialogueInput(
    22,
    22,
    7,
    HasSpecialEventText: false,
    BloodMoon: false,
    LanternsUp: false,
    DownedMoonlord: false,
    Eclipse: false,
    SlimeRain: false,
    DayTime: true,
    HardMode: true,
    StinkyDanger: true),
  dialogueRandom);
Require(chatter2.Selection == NpcGuideDialogueSelection.HardmodeChatter2 &&
  dialogueRandom.Calls == 2,
  "Guide hardmode chatter must consume the source flag17 Next(8) checks in order.");
Require(
  NpcGuideDialogueProfile.EvaluateWithRandom(
    new NpcGuideDialogueInput(22, 22, 7, true, false, false, false, false, false, true, false, false),
    new QueueGuideRandomPort()).Selection == NpcGuideDialogueSelection.SpecialEventText,
  "Guide special event text must take precedence without consuming random state.");
Require(
  NpcGuideDialogueProfile.EvaluateWithRandom(
    new NpcGuideDialogueInput(22, 22, 7, false, true, false, false, false, false, true, false, false),
    new QueueGuideRandomPort(2)).Selection == NpcGuideDialogueSelection.BloodMoon172,
  "Guide blood-moon dialogue must use the source three-way random selection.");

var idleRandom = new QueueGuideRandomPort(0, 1, 1);
NpcGuideIdleProfileResult idleConversation = NpcGuideIdleProfile.EvaluateWithRandom(
  new NpcGuideIdleInput(
    22,
    22,
    7,
    new NpcGuideSourceProfileState(0f, 0f, 0f, 0f),
    Direction: 1,
    VelocityY: 0f,
    ServerAuthority: true,
    PlayerTalking: false,
    DangerWithinBaseRange: false,
    Wet: false,
    CanTalk: true,
    TownPet: false,
    SelfEntityId: 10,
    CenterX: 100f,
    NearbyNpcs: [new NpcGuideIdleTalkNpcSnapshot(11, true, true, false, false, false, 50f, 140f, true)]),
  idleRandom);
Require(idleConversation.State.Ai0 == 3f &&
  idleConversation.State.Ai1 == 840f &&
  idleConversation.PartnerConversation is not null,
  "Guide idle conversation must enter ai0=3 with source duration randomization and expose partner state.");

NpcGuideIdleProfileResult secondIdleConversation = NpcGuideIdleProfile.EvaluateWithRandom(
  new NpcGuideIdleInput(
    22,
    22,
    7,
    new NpcGuideSourceProfileState(0f, 0f, 0f, 0f),
    Direction: 1,
    VelocityY: 0f,
    ServerAuthority: true,
    PlayerTalking: false,
    DangerWithinBaseRange: false,
    Wet: false,
    CanTalk: true,
    TownPet: false,
    SelfEntityId: 10,
    CenterX: 100f,
    NearbyNpcs: [new NpcGuideIdleTalkNpcSnapshot(11, true, true, false, false, false, 50f, 140f, true)]),
  new QueueGuideRandomPort(1, 0, 0, 1, 2, 0));
Require(secondIdleConversation.State.Ai0 == 16f &&
  secondIdleConversation.State.LocalAi2 == 2f &&
  secondIdleConversation.State.LocalAi3 == 0f,
  "Guide second idle conversation must preserve ai0=16 and its two localAI random slots.");

NpcGuideIdleProfileResult genericIdle = NpcGuideIdleProfile.EvaluateWithRandom(
  new NpcGuideIdleInput(
    22,
    22,
    7,
    new NpcGuideSourceProfileState(0f, 0f, 0f, 0f),
    Direction: 1,
    VelocityY: 0f,
    ServerAuthority: true,
    PlayerTalking: false,
    DangerWithinBaseRange: false,
    Wet: false,
    CanTalk: false,
    TownPet: false,
    SelfEntityId: 10,
    CenterX: 100f,
    NearbyNpcs: []),
  new QueueGuideRandomPort(0));
Require(genericIdle.State.Ai0 == 2f &&
  (genericIdle.Branches & NpcGuideIdleProfileBranch.GenericIdle) != 0,
  "Guide generic idle branch must preserve the source ai0=2 transition.");

var blockedCollision = new ScriptedCollisionQuery(static (_, _) => true);
NpcGuideSourceProfileResult noPath = Evaluate(
  baseInput with
  {
    DayTime = false,
    CollisionQuery = blockedCollision,
  });
Require(
  noPath.HomeTeleportFailed &&
  noPath.HomelessUpdateRequested &&
  noPath.QuickFindHomeRequested &&
  noPath.FailureReason == NpcTaskFailureReason.NoPath,
  "A blocked Guide home must produce a NoPath result and delegate reassignment.");
var noPathEffects = new RecordingEffectPort();
NpcGuideSourceProfile.ApplyEffects(in noPath, noPathEffects);
Require(
  noPathEffects.Events.SequenceEqual(["mark-homeless", "quick-find-home"]),
  "A failed Guide home return must mark homeless before requesting QuickFindHome.");

var sittingRandom = new QueueGuideRandomPort(37);
var occupiedSittingOwner = new GuideForceSittingOwnerProbe(
  homeInWorld: true,
  floorTileActive: true,
  floorTileType: 15,
  floorTileFrameX: 1,
  floorTileFrameY: 0,
  currentAi0: 0f,
  occupiedBySittingNpc: true,
  occupiedBySittingPlayer: false,
  sittingRandom);
Require(!occupiedSittingOwner.TryCommit(teleported.ForceSittingRequest) &&
  occupiedSittingOwner.RandomCalls == 0 &&
  occupiedSittingOwner.Ai0 == 0f,
  "Guide force-sitting must recheck real-time NPC/player occupancy before consuming random state or writing ai0.");
occupiedSittingOwner.OccupiedBySittingNpc = false;
Require(occupiedSittingOwner.TryCommit(teleported.ForceSittingRequest) &&
  occupiedSittingOwner.RandomCalls == 1 &&
  occupiedSittingOwner.LastRandomMaxExclusive == 10800 &&
  occupiedSittingOwner.Ai0 == 5f &&
  occupiedSittingOwner.Ai1 == 937f &&
  occupiedSittingOwner.Direction == 1 &&
  occupiedSittingOwner.Position == new Vector2(10 * 16f + 8f + 2f, 20 * 16f) &&
  occupiedSittingOwner.Velocity == Vector2.Zero &&
  occupiedSittingOwner.LocalAi3 == 0f &&
  occupiedSittingOwner.NetworkUpdateRequested,
  "Guide force-sitting must consume 900+Next(10800) after occupancy checks and write ai0, ai1, position, velocity, localAI3, and netUpdate in source order.");

var alreadySittingOwner = new GuideForceSittingOwnerProbe(
  homeInWorld: true,
  floorTileActive: true,
  floorTileType: 497,
  floorTileFrameX: 0,
  floorTileFrameY: 0,
  currentAi0: 5f,
  occupiedBySittingNpc: false,
  occupiedBySittingPlayer: false,
  new QueueGuideRandomPort(1));
Require(!alreadySittingOwner.TryCommit(teleported.ForceSittingRequest) &&
  alreadySittingOwner.RandomCalls == 0,
  "Guide force-sitting must reject an already sitting ai0 state without consuming random state.");

var taskState = new NpcTaskStateComponent();
NpcTaskLifecycleResult entered = NpcTaskLifecycleSystem.Enter(
  taskState,
  NpcTaskKind.GuideReturnHome);
Require(entered.Accepted && entered.Phase == NpcTaskPhase.Running,
  "Guide return-home task composition must enter through the shared lifecycle owner.");
NpcTaskLifecycleResult completed = NpcTaskLifecycleSystem.Complete(
  taskState,
  NpcTaskKind.GuideReturnHome);
Require(completed.Accepted && completed.Phase == NpcTaskPhase.Completed,
  "A successful Guide return must complete through the shared task owner.");
NpcTaskLifecycleSystem.Enter(taskState, NpcTaskKind.GuideReturnHome);
NpcTaskLifecycleResult failed = NpcTaskLifecycleSystem.Fail(
  taskState,
  NpcTaskKind.GuideReturnHome,
  NpcTaskFailureReason.NoPath);
Require(failed.Accepted && failed.Phase == NpcTaskPhase.Failed &&
    failed.FailureReason == NpcTaskFailureReason.NoPath,
  "A failed Guide return must preserve NoPath in the shared task owner.");

Console.WriteLine(
  "PASS: Guide source identity, weather return pressure, server/home/player gates, " +
  "home candidate order, teleport effect order, no-path reassignment, repeat suppression, " +
  "good-resting-spot predicate and search, narrow force-sitting owner semantics, " +
  "walk prediction, day movement state, conversation state, danger scan and response, attack configuration, " +
  "random Guide dialogue and idle conversation, traversal, ai0=8 continuation, sitting tick, conversation tick, and shared task composition.");

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static NpcGuideSourceProfileResult Evaluate(NpcGuideSourceProfileInput input)
{
  return NpcGuideSourceProfile.Evaluate(in input);
}

static NpcGuideWalkPredictionResult EvaluateWalk(NpcGuideWalkPredictionInput input)
{
  return NpcGuideWalkPrediction.Evaluate(in input);
}

sealed class ScriptedCollisionQuery : INpcHomeReturnCollisionQuery
{
  private readonly Func<int, int, bool> _isSolid;

  public ScriptedCollisionQuery(Func<int, int, bool> isSolid)
  {
    _isSolid = isSolid;
  }

  public bool IsSolidTile(int tileX, int tileY)
  {
    return _isSolid(tileX, tileY);
  }
}

sealed class ScriptedRestingTileQuery : INpcGuideRestingSpotTileQuery
{
  private readonly Dictionary<(int X, int Y), NpcGuideRestingSpotTile> _tiles = [];

  public void Set(int tileX, int tileY, NpcGuideRestingSpotTile tile)
  {
    _tiles[(tileX, tileY)] = tile;
  }

  public NpcGuideRestingSpotTile ReadTile(int tileX, int tileY)
  {
    return _tiles.TryGetValue((tileX, tileY), out NpcGuideRestingSpotTile tile)
      ? tile
      : default;
  }
}

sealed class ScriptedRestingOccupancyQuery : INpcGuideRestingSpotOccupancyQuery
{
  private readonly HashSet<(int X, int Y)> _occupied = [];

  public void SetOccupied(int tileX, int tileY, bool occupied)
  {
    if (occupied)
    {
      _occupied.Add((tileX, tileY));
    }
    else
    {
      _occupied.Remove((tileX, tileY));
    }
  }

  public bool IsOccupiedBySittingTownNpc(int tileX, int tileY)
  {
    return _occupied.Contains((tileX, tileY));
  }
}

sealed class ScriptedWalkTileQuery : INpcGuideWalkTileQuery
{
  private readonly Dictionary<(int X, int Y), NpcGuideWalkTile> _tiles = [];

  public void Set(int tileX, int tileY, NpcGuideWalkTile tile)
  {
    _tiles[(tileX, tileY)] = tile;
  }

  public NpcGuideWalkTile ReadTile(int tileX, int tileY)
  {
    return _tiles.TryGetValue((tileX, tileY), out NpcGuideWalkTile tile)
      ? tile
      : default;
  }
}

sealed class RecordingEffectPort : INpcGuideSourceEffectPort
{
  public List<string> Events { get; } = [];

  public void ApplyHomeTeleport(Vector2 position, Vector2 velocity)
  {
    Events.Add("home-teleport");
  }

  public void RequestNetworkSync()
  {
    Events.Add("network-sync");
  }

  public bool TryForceSitting(in NpcGuideForceSittingRequest request)
  {
    Events.Add("force-sitting");
    LastSittingRequest = request;
    return true;
  }

  public NpcGuideForceSittingRequest LastSittingRequest { get; private set; }

  public void MarkHomeless()
  {
    Events.Add("mark-homeless");
  }

  public void RequestQuickFindHome()
  {
    Events.Add("quick-find-home");
  }
}

sealed class RecordingDangerEffectPort : INpcGuideDangerEffectPort
{
  public List<string> Events { get; } = [];

  public void RequestPartnerStateReset(
    in NpcGuidePartnerStateResetRequest request)
  {
    Events.Add("partner-reset");
  }

  public void RequestNetworkSync()
  {
    Events.Add("network-sync");
  }
}

sealed class RecordingAttackEffectPort : INpcGuideAttackEffectPort
{
  public List<string> Events { get; } = [];

  public void SpawnProjectile(in NpcGuideProjectileSpawnRequest request)
  {
    Events.Add("projectile");
  }

  public void RequestNetworkSync()
  {
    Events.Add("network-sync");
  }
}

sealed class RecordingDayMovementEffectPort : INpcGuideDayMovementEffectPort
{
  public List<string> Events { get; } = [];

  public bool TryForceSitting(in NpcGuideForceSittingRequest request)
  {
    Events.Add("force-sitting");
    return true;
  }

  public void RequestNetworkSync()
  {
    Events.Add("network-sync");
  }
}

sealed class RecordingSittingEffectPort : INpcGuideSittingEffectPort
{
  public List<string> Events { get; } = [];

  public void RegisterSitting(in NpcGuideSittingRequest request)
  {
    Events.Add("sitting");
  }

  public void RequestNetworkSync()
  {
    Events.Add("network-sync");
  }
}

sealed class RecordingTraversalEffectPort : INpcGuideTraversalEffectPort
{
  public List<string> Events { get; } = [];

  public void RequestDoor(in NpcGuideDoorEffectRequest request)
  {
    Events.Add("door");
  }

  public void RequestNetworkSync()
  {
    Events.Add("network-sync");
  }
}

sealed class QueueGuideRandomPort :
  INpcGuideDangerRandomPort,
  INpcGuideAiEightRandomPort,
  INpcGuideSittingRandomPort,
  INpcGuideConversationTickRandomPort,
  INpcGuideDialogueRandomPort,
  INpcGuideIdleRandomPort,
  INpcGuideAttackRandomPort,
  INpcGuideDayMovementRandomPort,
  INpcGuideTraversalRandomPort
{
  private readonly Queue<int> _values;
  private Queue<float> _floatValues = [];

  public QueueGuideRandomPort(params int[] values)
  {
    _values = new Queue<int>(values);
  }

  public int Calls { get; private set; }

  public int Next(int maxExclusive)
  {
    Calls++;
    LastMaxExclusive = maxExclusive;
    if (_values.Count == 0)
    {
      throw new InvalidOperationException("Guide sitting random fixture consumed too many values.");
    }

    return _values.Dequeue();
  }

  public int LastMaxExclusive { get; private set; }

  public void SetFloatValues(params float[] values)
  {
    _floatValues = new Queue<float>(values);
  }

  public float NextSingle(float minInclusive, float maxExclusive)
  {
    if (_floatValues.Count == 0)
    {
      return 0f;
    }

    return _floatValues.Dequeue();
  }
}

sealed class GuideForceSittingOwnerProbe
{
  private readonly bool _homeInWorld;
  private readonly bool _floorTileActive;
  private readonly int _floorTileType;
  private readonly int _floorTileFrameX;
  private readonly int _floorTileFrameY;
  private readonly QueueGuideRandomPort _random;

  public GuideForceSittingOwnerProbe(
    bool homeInWorld,
    bool floorTileActive,
    int floorTileType,
    int floorTileFrameX,
    int floorTileFrameY,
    float currentAi0,
    bool occupiedBySittingNpc,
    bool occupiedBySittingPlayer,
    QueueGuideRandomPort random)
  {
    _homeInWorld = homeInWorld;
    _floorTileActive = floorTileActive;
    _floorTileType = floorTileType;
    _floorTileFrameX = floorTileFrameX;
    _floorTileFrameY = floorTileFrameY;
    OccupiedBySittingNpc = occupiedBySittingNpc;
    OccupiedBySittingPlayer = occupiedBySittingPlayer;
    _random = random;
    Ai0 = currentAi0;
  }

  public bool OccupiedBySittingNpc { get; set; }

  public bool OccupiedBySittingPlayer { get; set; }

  public int RandomCalls => _random.Calls;

  public int LastRandomMaxExclusive => _random.LastMaxExclusive;

  public float Ai0 { get; private set; }

  public float Ai1 { get; private set; }

  public int Direction { get; private set; }

  public Vector2 Position { get; private set; }

  public Vector2 Velocity { get; private set; }

  public float LocalAi3 { get; private set; }

  public bool NetworkUpdateRequested { get; private set; }

  public bool TryCommit(NpcGuideForceSittingRequest request)
  {
    if (!_homeInWorld ||
        !_floorTileActive ||
        (_floorTileType != 15 && _floorTileType != 497) ||
        (_floorTileType == 15 && _floorTileFrameY >= 1080 && _floorTileFrameY <= 1098) ||
        Ai0 == 5f ||
        OccupiedBySittingNpc ||
        OccupiedBySittingPlayer)
    {
      return false;
    }

    int randomOffset = _random.Next(10800);
    Ai0 = 5f;
    Ai1 = 900 + randomOffset;
    Direction = _floorTileFrameX != 0 ? 1 : -1;
    Position = new Vector2(
      request.HomeFloorX * 16f + 8f + 2f * Direction,
      request.HomeFloorY * 16f);
    Velocity = Vector2.Zero;
    LocalAi3 = 0f;
    NetworkUpdateRequested = true;
    return true;
  }
}
