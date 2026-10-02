using Terraria.Projectile;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
  if (!EqualityComparer<T>.Default.Equals(expected, actual))
  {
    throw new InvalidOperationException(
      $"{message} Expected '{expected}', actual '{actual}'.");
  }
}

static void AssertThrows<TException>(Action action, string message)
  where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException(message);
}

if (args.Length == 1 && args[0] == "--identity-index")
{
  ProjectileIdentityIndexVerification.Run();
  Console.WriteLine("PASS: projectile identity index invariants");
  return;
}

if (args.Length == 1 && args[0] == "--lifecycle")
{
  ProjectileLifecycleVerification.Run();
  Console.WriteLine(
    "PASS: projectile lifecycle allocation, replacement, and termination invariants");
  return;
}

if (args.Length == 1 && args[0] == "--hydration")
{
  ProjectileDefinitionHydrationVerification.Run();
  Console.WriteLine("PASS: projectile definition hydration and lifecycle commit");
  return;
}

if (args.Length == 1 && args[0] == "--network")
{
  ProjectileNetworkApplyVerification.Run();
  Console.WriteLine("PASS: projectile network identity and packet apply invariants");
  return;
}

if (args.Length == 1 && args[0] == "--tick-coordinator")
{
  ProjectileTickCoordinatorVerification.Run();
  Console.WriteLine(
    "PASS: projectile ordered tick coordinator and extra-update invariants");
  return;
}

var definition = new ProjectileDefinitionComponent(
  projectileType: 12,
  behaviorKey: 7,
  friendlyDefault: true,
  hostileDefault: false,
  extraUpdates: 0,
  catalogRevision: 3);
var disposition = new ProjectileDispositionStateComponent();
ProjectileDispositionSystem.InitializeFromDefinition(ref disposition, definition);
Assert(disposition.Friendly, "Definition friendly default should initialize instance state.");
Assert(!disposition.Hostile, "Definition hostile default should initialize instance state.");

ProjectileDispositionSystem.SetHostile(ref disposition, true);
Assert(disposition.Friendly, "Hostile writer must not change friendly state.");
Assert(disposition.Hostile, "Hostile writer should update hostile state.");
ProjectileDispositionSystem.SetFriendly(ref disposition, false);
Assert(!disposition.Friendly, "Friendly writer should update friendly state.");
Assert(disposition.Hostile, "Friendly writer must not change hostile state.");
Assert(definition.FriendlyDefault, "Disposition writers must not mutate the definition value.");
Assert(!definition.HostileDefault, "Disposition writers must not mutate definition hostility.");

var finite = new ProjectilePenetrationStateComponent(
  remainingHits: 2,
  maximumHits: 2,
  stopsDealingDamageWhenDepleted: true);
Assert(ProjectilePenetrationSystem.CanDealDamage(finite), "Finite penetration should initially allow damage.");
Assert(ProjectilePenetrationSystem.CommitAcceptedHit(ref finite), "The first accepted hit should commit.");
AssertEqual(1, finite.RemainingHits, "The first finite hit should decrement remaining penetration.");
AssertEqual(1, finite.HitCount, "The first accepted hit should increment hit count.");
Assert(ProjectilePenetrationSystem.CommitAcceptedHit(ref finite), "The second accepted hit should commit.");
AssertEqual(0, finite.RemainingHits, "The second finite hit should exhaust penetration.");
AssertEqual(2, finite.HitCount, "Hit count should include both accepted hits.");
Assert(!ProjectilePenetrationSystem.CanDealDamage(finite), "An exhausted projectile must not deal damage.");
Assert(!ProjectilePenetrationSystem.CommitAcceptedHit(ref finite), "An exhausted hit must be rejected.");
AssertEqual(0, finite.RemainingHits, "A rejected exhausted hit must not change penetration.");
AssertEqual(2, finite.HitCount, "A rejected exhausted hit must not change hit count.");

var unlimited = new ProjectilePenetrationStateComponent(
  remainingHits: -1,
  maximumHits: -1);
Assert(unlimited.IsUnlimited, "The -1 penetration sentinel should be unlimited.");
Assert(ProjectilePenetrationSystem.CommitAcceptedHit(ref unlimited), "Unlimited penetration should accept a hit.");
AssertEqual(-1, unlimited.RemainingHits, "Unlimited penetration must preserve the -1 sentinel.");
AssertEqual(1, unlimited.HitCount, "Unlimited penetration should still count accepted hits.");

var reflection = new ProjectileReflectionStateComponent();
ProjectileReflectionSystem.MarkReflected(ref reflection);
Assert(reflection.Reflected, "Reflection commit should set the instance reflection result.");
ProjectileReflectionSystem.SetReflected(ref reflection, false);
Assert(!reflection.Reflected, "The explicit reflection writer should support a compatibility reset.");

AssertThrows<ArgumentOutOfRangeException>(
  () => new ProjectilePenetrationStateComponent(remainingHits: -2),
  "Penetration values below -1 must be rejected.");
AssertThrows<ArgumentOutOfRangeException>(
  () => new ProjectilePenetrationStateComponent(hitCount: -1),
  "Negative accepted-hit counts must be rejected.");
AssertThrows<ArgumentOutOfRangeException>(
  () => ProjectilePenetrationSystem.SetRemainingHits(ref finite, -2),
  "The penetration writer must reject values below -1.");

var policy = new ProjectileHitImmunityPolicyComponent(
  usesLocalNpcImmunity: true,
  localNpcCooldownTicks: 2,
  usesStaticNpcImmunity: false,
  staticNpcCooldownTicks: -1);
Assert(policy.WritesLocalNpcImmunity, "A configured local cooldown should write local state.");
Assert(!policy.UsesStaticNpcImmunityRegistry, "Static registry use should remain disabled by policy.");

var immunity = new ProjectileHitImmunityStateComponent(
  npcCapacity: 2,
  playerCapacity: 2);
ProjectileHitImmunitySystem.SetLocalNpcImmunity(ref immunity, 0, 2);
ProjectileHitImmunitySystem.SetLocalNpcImmunity(ref immunity, 1, -1);
ProjectileHitImmunitySystem.SetPlayerImmunity(ref immunity, 0, 2);
ProjectileHitImmunitySystem.SetRestrikeDelay(ref immunity, 2);
Assert(ProjectileHitImmunitySystem.IsLocalNpcImmune(immunity, 0), "Positive local cooldown should block an NPC hit.");
Assert(ProjectileHitImmunitySystem.IsLocalNpcImmune(immunity, 1), "-1 local immunity should remain blocking.");
Assert(ProjectileHitImmunitySystem.IsPlayerImmune(immunity, 0), "Positive player cooldown should block a player hit.");

ProjectileHitImmunitySystem.AdvanceTick(ref immunity, policy);
Assert(ProjectileHitImmunitySystem.IsLocalNpcImmune(immunity, 0), "A positive local cooldown should remain after one tick.");
Assert(ProjectileHitImmunitySystem.IsLocalNpcImmune(immunity, 1), "-1 local immunity must not decrement.");
Assert(ProjectileHitImmunitySystem.IsPlayerImmune(immunity, 0), "A positive player cooldown should remain after one tick.");
AssertEqual(1, immunity.RestrikeDelayTicks, "Restrike delay should decrement when positive.");

ProjectileHitImmunitySystem.AdvanceTick(ref immunity, policy);
Assert(!ProjectileHitImmunitySystem.IsLocalNpcImmune(immunity, 0), "A local cooldown should clear at zero.");
Assert(!ProjectileHitImmunitySystem.IsPlayerImmune(immunity, 0), "A player cooldown should clear at zero.");
AssertEqual(0, immunity.RestrikeDelayTicks, "Restrike delay should reach zero.");

ProjectileHitImmunitySystem.ResetLocalNpcImmunity(ref immunity);
ProjectileHitImmunitySystem.ResetPlayerImmunity(ref immunity);
Assert(!ProjectileHitImmunitySystem.IsLocalNpcImmune(immunity, 1), "Local reset should clear the -1 sentinel.");
Assert(!ProjectileHitImmunitySystem.IsPlayerImmune(immunity, 0), "Player reset should clear cooldowns.");

var disabledLocalPolicy = new ProjectileHitImmunityPolicyComponent(
  usesLocalNpcImmunity: false,
  localNpcCooldownTicks: -2);
ProjectileHitImmunitySystem.SetLocalNpcImmunity(ref immunity, 0, 2);
ProjectileHitImmunitySystem.AdvanceTick(ref immunity, disabledLocalPolicy);
Assert(ProjectileHitImmunitySystem.IsLocalNpcImmune(immunity, 0), "Disabled local policy must not tick local state.");

AssertThrows<ArgumentOutOfRangeException>(
  () => new ProjectileHitImmunityPolicyComponent(localNpcCooldownTicks: -3),
  "Local policy values below -2 must be rejected.");
AssertThrows<ArgumentOutOfRangeException>(
  () => new ProjectileHitImmunityStateComponent(npcCapacity: -1),
  "Negative NPC capacity must be rejected.");
AssertThrows<ArgumentOutOfRangeException>(
  () => ProjectileHitImmunitySystem.SetLocalNpcImmunity(ref immunity, 2, 1),
  "An out-of-range NPC index must be rejected.");
AssertThrows<ArgumentOutOfRangeException>(
  () => ProjectileHitImmunitySystem.SetPlayerImmunity(ref immunity, 0, -1),
  "Negative player cooldowns must be rejected.");

var trail = new ProjectileTrailCacheComponent(historyLength: 2);
ProjectileTrailCacheSystem.Record(ref trail, new System.Numerics.Vector2(1f, 2f), 0.5f, 1);
ProjectileTrailCacheSystem.Record(ref trail, new System.Numerics.Vector2(3f, 4f), 1.5f, -1);
AssertEqual(
  new System.Numerics.Vector2(3f, 4f),
  trail.OldPositions[0],
  "The newest trail position should be stored at index zero.");
AssertEqual(
  new System.Numerics.Vector2(1f, 2f),
  trail.OldPositions[1],
  "The previous trail position should shift to index one.");
AssertEqual(1.5f, trail.OldRotations[0], "The newest trail rotation should be stored.");
AssertEqual(-1, trail.OldSpriteDirections[0], "The newest sprite direction should be stored.");
trail.WhipPoints.Add(new System.Numerics.Vector2(5f, 6f));
ProjectileTrailCacheSystem.Reset(ref trail);
AssertEqual(System.Numerics.Vector2.Zero, trail.OldPositions[0], "Trail reset should clear positions.");
AssertEqual(0f, trail.OldRotations[0], "Trail reset should clear rotations.");
AssertEqual(0, trail.OldSpriteDirections[0], "Trail reset should clear sprite directions.");
AssertEqual(0, trail.WhipPoints.Count, "Trail reset should clear whip points.");
AssertThrows<ArgumentOutOfRangeException>(
  () => new ProjectileTrailCacheComponent(historyLength: -1),
  "Negative trail history length must be rejected.");

var collisionPolicy = new ProjectileCollisionPolicyComponent();
ProjectileCollisionPolicySystem.SetDecidesManualFallThrough(ref collisionPolicy, true);
ProjectileCollisionPolicySystem.SetShouldFallThrough(ref collisionPolicy, true);
Assert(
  ProjectileCollisionPolicySystem.CanFallThrough(collisionPolicy),
  "Fall-through should require both the decision policy and instance override.");
ProjectileCollisionPolicySystem.SetDecidesManualFallThrough(ref collisionPolicy, false);
Assert(
  !ProjectileCollisionPolicySystem.CanFallThrough(collisionPolicy),
  "Fall-through should be disabled when the decision policy is false.");
ProjectileCollisionPolicySystem.SetTileCollisionEnabled(ref collisionPolicy, false);
ProjectileCollisionPolicySystem.SetIgnoreWater(ref collisionPolicy, true);
ProjectileCollisionPolicySystem.SetCorrectSlopeCollision(ref collisionPolicy, true);
ProjectileCollisionPolicySystem.SetReflectsFromTiles(ref collisionPolicy, true);
ProjectileCollisionPolicySystem.SetOwnerHitCheck(ref collisionPolicy, true);
ProjectileCollisionPolicySystem.SetOwnerHitCheckDistance(ref collisionPolicy, 24.0f);
ProjectileCollisionPolicySystem.SetManualDirectionChange(ref collisionPolicy, true);
ProjectileCollisionPolicySystem.SetBouncePolicy(ref collisionPolicy, 2, 0.5f, 3.0f);
Assert(!collisionPolicy.TileCollisionEnabled, "Tile collision writer should update policy state.");
Assert(collisionPolicy.IgnoreWater, "Water policy writer should update policy state.");
Assert(collisionPolicy.CorrectSlopeCollision, "Slope policy writer should update policy state.");
Assert(collisionPolicy.ReflectsFromTiles, "Reflection policy writer should update policy state.");
Assert(collisionPolicy.OwnerHitCheck, "Owner-hit policy writer should update policy state.");
AssertEqual(24.0f, collisionPolicy.OwnerHitCheckDistance, "Owner-hit distance should be stored.");
Assert(collisionPolicy.ManualDirectionChange, "Direction policy writer should update policy state.");
AssertEqual(2, collisionPolicy.MaximumBounces, "Bounce count should be stored.");
AssertEqual(0.5f, collisionPolicy.BounceVelocityMultiplier, "Bounce multiplier should be stored.");
AssertEqual(3.0f, collisionPolicy.MinimumBounceSpeed, "Minimum bounce speed should be stored.");
AssertThrows<ArgumentOutOfRangeException>(
  () => new ProjectileCollisionPolicyComponent(ownerHitCheckDistance: float.NaN),
  "Non-finite owner-hit distance must be rejected.");
AssertThrows<ArgumentOutOfRangeException>(
  () => ProjectileCollisionPolicySystem.SetBouncePolicy(ref collisionPolicy, -1, 1.0f, 0.0f),
  "Negative bounce capacity must be rejected.");
AssertThrows<ArgumentOutOfRangeException>(
  () => ProjectileCollisionPolicySystem.SetOwnerHitCheckDistance(
    ref collisionPolicy,
    float.PositiveInfinity),
  "Non-finite owner-hit distance updates must be rejected.");

var network = new ProjectileNetworkStateComponent(playerCapacity: 2);
ProjectileNetworkStateSystem.RequestPrimaryUpdate(ref network);
Assert(network.SendRequested, "Primary network requests should mark send intent.");
Assert(
  ProjectileNetworkStateSystem.TryConsumeSendBudget(ref network),
  "A pending update should consume available send budget.");
AssertEqual(5, network.NetSpam, "A successful send should add the observed spam increment.");
Assert(!network.PrimaryUpdatePending, "A successful send should clear the primary request.");
Assert(!network.SecondaryUpdatePending, "A successful send should clear the secondary request.");
Assert(!network.SendRequested, "A successful send should clear send intent.");

network.NetSpam = ProjectileNetworkStateSystem.SendBudgetThreshold;
ProjectileNetworkStateSystem.RequestPrimaryUpdate(ref network);
Assert(
  !ProjectileNetworkStateSystem.TryConsumeSendBudget(ref network),
  "A saturated network budget should defer the update.");
Assert(!network.PrimaryUpdatePending, "A deferred request should leave primary state clear.");
Assert(network.SecondaryUpdatePending, "A deferred request should set secondary state.");
ProjectileNetworkStateSystem.AdvanceTick(ref network);
AssertEqual(59, network.NetSpam, "Network spam should decrement by one tick.");
Assert(
  ProjectileNetworkStateSystem.TryConsumeSendBudget(ref network),
  "A deferred update should send after one budget tick.");
AssertEqual(64, network.NetSpam, "The send should preserve the Version4 threshold-plus-increment behavior.");

ProjectileNetworkStateSystem.MarkSectionSyncSkipped(ref network, 1);
Assert(
  ProjectileNetworkStateSystem.IsSectionSyncSkipped(network, 1),
  "Section skip state should be readable after it is marked.");
ProjectileNetworkStateSystem.ClearSectionSyncSkipped(ref network, 1);
Assert(
  !ProjectileNetworkStateSystem.IsSectionSyncSkipped(network, 1),
  "Section skip state should clear only through the explicit send-boundary writer.");
AssertThrows<ArgumentOutOfRangeException>(
  () => ProjectileNetworkStateSystem.MarkSectionSyncSkipped(ref network, 2),
  "A section skip outside player capacity must be rejected.");
AssertThrows<ArgumentOutOfRangeException>(
  () => new ProjectileNetworkStateComponent(playerCapacity: -1),
  "Negative network player capacity must be rejected.");
AssertThrows<ArgumentOutOfRangeException>(
  () => new ProjectileNetworkStateComponent(netSpam: -1),
  "Negative network spam must be rejected.");
ProjectileNetworkStateSystem.Reset(ref network);
AssertEqual(0, network.NetSpam, "Network reset should clear spam state.");
Assert(
  !ProjectileNetworkStateSystem.IsSectionSyncSkipped(network, 1),
  "Network reset should clear section skip state.");

var animation = new ProjectileAnimationStateComponent(frame: 2, frameCounter: 1);
AssertEqual(2, animation.Frame, "Animation frame should preserve the initialized frame.");
AssertEqual(1, animation.FrameCounter, "Animation counter should preserve the initialized counter.");
Assert(
  !ProjectileAnimationStateSystem.AdvanceFrameCounter(ref animation, 3),
  "Animation should not advance before the configured counter limit.");
AssertEqual(2, animation.FrameCounter, "Animation counter should increment deterministically.");
Assert(
  ProjectileAnimationStateSystem.AdvanceFrameCounter(ref animation, 3),
  "Animation should signal a frame transition at the counter limit.");
AssertEqual(0, animation.FrameCounter, "Frame transition should reset the counter.");
ProjectileAnimationStateSystem.SetFrame(ref animation, 4);
ProjectileAnimationStateSystem.SetFrameCounter(ref animation, 2);
AssertEqual(4, animation.Frame, "Explicit frame writer should update animation state.");
AssertEqual(2, animation.FrameCounter, "Explicit counter writer should update animation state.");
ProjectileAnimationStateSystem.Reset(ref animation);
AssertEqual(0, animation.Frame, "Animation reset should clear the frame.");
AssertEqual(0, animation.FrameCounter, "Animation reset should clear the counter.");
AssertThrows<ArgumentOutOfRangeException>(
  () => new ProjectileAnimationStateComponent(frame: -1),
  "Negative animation frames must be rejected.");
AssertThrows<ArgumentOutOfRangeException>(
  () => ProjectileAnimationStateSystem.AdvanceFrameCounter(ref animation, 0),
  "Non-positive animation limits must be rejected.");

ProjectileIdentityIndexVerification.Run();

Console.WriteLine(
  "PASS: projectile disposition, penetration, immunity, trail, reflection, " +
  "collision policy, network, animation, and identity boundaries");
