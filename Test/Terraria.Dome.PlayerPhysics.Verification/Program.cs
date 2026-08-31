using System;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Physics.Systems;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;

using DomeSimulation simulation = new(new WorldGrid(400, 300));
_ = simulation.WorldGrid.TrySetTile(20, 20, new WorldTile(IsActive: true, Type: 1));
PlayerHandle player = simulation.CreatePlayer(new SimulationVector(20, 23));
simulation.Tick(new SimulationInputBatch([]));
simulation.Tick(new SimulationInputBatch([]));
PlayerSnapshot snapshot = simulation.CreateSnapshot().FindPlayer(player);
if (snapshot.Position.Y != 21.0f || !snapshot.IsGrounded)
{
  throw new InvalidOperationException(
    $"Falling player penetrated a solid tile: y={snapshot.Position.Y:F2}.");
}

Console.WriteLine("PASS: falling players stop above solid tiles");

using DomeSimulation invertedGravitySimulation = new(new WorldGrid(400, 300));
PlayerHandle invertedGravityPlayer = invertedGravitySimulation.CreatePlayer(
  new SimulationVector(20.0f, 20.0f));
invertedGravitySimulation.SetPlayerGravityDirection(invertedGravityPlayer, -1.0f);
invertedGravitySimulation.Tick(new SimulationInputBatch());
PlayerSnapshot invertedGravitySnapshot =
  invertedGravitySimulation.CreateSnapshot().FindPlayer(invertedGravityPlayer);
if (invertedGravitySnapshot.Position.Y != 21.0f || invertedGravitySnapshot.Velocity.Y != 1.0f ||
    invertedGravitySnapshot.GravityDirection != -1.0f)
{
  throw new InvalidOperationException("Player gravity direction did not affect authoritative movement.");
}

Console.WriteLine("PASS: player gravity direction is simulation-owned");

VerifyTileDefinitionDrivenCollision();
VerifyHalfBrickCollisionGeometry();
VerifyTopSlopeContactGeometry();
VerifyPlayerTopSlopeComposition();
VerifyBottomSlopeDeferContract();
VerifyTileCollisionActivationContract();
VerifyCommittedActuatorInactiveCollision();
VerifyLegacyPlatformFallThroughContract();
VerifyPlayerInputControlsPlatformFallThrough();

static void VerifyTileDefinitionDrivenCollision()
{
  TileCollisionSystem system = new();
  ColliderComponent collider = new(1.0f, 1.0f);

  WorldGrid solidWorld = new(400, 300);
  _ = solidWorld.TrySetTile(4, 10, new WorldTile(IsActive: true, Type: 1));
  TransformComponent solidTransform = new(2.0f, 10.0f);
  VelocityComponent solidVelocity = new(3.0f, 0.0f);
  PhysicsStateComponent solidPhysics = default;
  system.MoveAndResolve(
    solidWorld,
    ref solidTransform,
    ref solidVelocity,
    ref solidPhysics,
    collider);
  if (solidTransform.X != 3.0f || solidVelocity.X != 0.0f)
  {
    throw new InvalidOperationException("A solid tile must block horizontal movement.");
  }

  WorldGrid slopedSolidWorld = new(400, 300);
  _ = slopedSolidWorld.TrySetTile(
    4,
    10,
    new WorldTile(IsActive: true, Type: 1, IsHalfBrick: true, Slope: 1));
  TransformComponent slopedSolidTransform = new(2.0f, 10.0f);
  VelocityComponent slopedSolidVelocity = new(3.0f, 0.0f);
  PhysicsStateComponent slopedSolidPhysics = default;
  system.MoveAndResolve(
    slopedSolidWorld,
    ref slopedSolidTransform,
    ref slopedSolidVelocity,
    ref slopedSolidPhysics,
    collider);
  if (slopedSolidTransform.X != 3.0f || slopedSolidVelocity.X != 0.0f)
  {
    throw new InvalidOperationException(
      "This slice must retain the existing whole-tile fallback for unported slope geometry.");
  }

  WorldGrid nonSolidWorld = new(400, 300);
  _ = nonSolidWorld.TrySetTile(4, 10, new WorldTile(IsActive: true, Type: 4));
  TransformComponent nonSolidTransform = new(2.0f, 10.0f);
  VelocityComponent nonSolidVelocity = new(3.0f, 0.0f);
  PhysicsStateComponent nonSolidPhysics = default;
  system.MoveAndResolve(
    nonSolidWorld,
    ref nonSolidTransform,
    ref nonSolidVelocity,
    ref nonSolidPhysics,
    collider);
  if (nonSolidTransform.X != 5.0f || nonSolidVelocity.X != 3.0f)
  {
    throw new InvalidOperationException("An active non-solid tile must not block movement.");
  }

  WorldGrid platformWorld = new(400, 300);
  _ = platformWorld.TrySetTile(4, 10, new WorldTile(IsActive: true, Type: 19));
  TransformComponent platformTransform = new(2.0f, 10.0f);
  VelocityComponent platformVelocity = new(3.0f, 0.0f);
  PhysicsStateComponent platformPhysics = default;
  system.MoveAndResolve(
    platformWorld,
    ref platformTransform,
    ref platformVelocity,
    ref platformPhysics,
    collider);
  if (platformTransform.X != 5.0f || platformVelocity.X != 3.0f)
  {
    throw new InvalidOperationException("A platform must not act as a horizontal wall.");
  }

  TransformComponent platformLandingTransform = new(4.0f, 12.0f);
  VelocityComponent platformLandingVelocity = new(0.0f, -3.0f);
  PhysicsStateComponent platformLandingPhysics = default;
  system.MoveAndResolve(
    platformWorld,
    ref platformLandingTransform,
    ref platformLandingVelocity,
    ref platformLandingPhysics,
    collider);
  if (platformLandingTransform.Y != 11.0f || platformLandingVelocity.Y != 0.0f ||
      !platformLandingPhysics.IsGrounded)
  {
    throw new InvalidOperationException(
      "A default falling collider must land on a platform top surface.");
  }

  TransformComponent platformUpwardTransform = new(4.0f, 8.0f);
  VelocityComponent platformUpwardVelocity = new(0.0f, 3.0f);
  PhysicsStateComponent platformUpwardPhysics = default;
  system.MoveAndResolve(
    platformWorld,
    ref platformUpwardTransform,
    ref platformUpwardVelocity,
    ref platformUpwardPhysics,
    collider);
  if (platformUpwardTransform.Y != 11.0f || platformUpwardVelocity.Y != 3.0f ||
      platformUpwardPhysics.IsGrounded)
  {
    throw new InvalidOperationException("A collider must pass upward through a platform.");
  }

  WorldGrid boundaryWorld = new(400, 300);
  TransformComponent boundaryTransform = new(0.0f, 10.0f);
  VelocityComponent boundaryVelocity = new(-1.0f, 0.0f);
  PhysicsStateComponent boundaryPhysics = default;
  system.MoveAndResolve(
    boundaryWorld,
    ref boundaryTransform,
    ref boundaryVelocity,
    ref boundaryPhysics,
    collider);
  if (boundaryTransform.X != 0.0f || boundaryVelocity.X != 0.0f)
  {
    throw new InvalidOperationException("World boundaries must remain blocking.");
  }

  WorldGrid unknownWorld = new(400, 300);
  _ = unknownWorld.TrySetTile(4, 10, new WorldTile(IsActive: true, Type: 753));
  TransformComponent unknownTransform = new(2.0f, 10.0f);
  VelocityComponent unknownVelocity = new(3.0f, 0.0f);
  PhysicsStateComponent unknownPhysics = new() { IsGrounded = true };
  try
  {
    system.MoveAndResolve(
      unknownWorld,
      ref unknownTransform,
      ref unknownVelocity,
      ref unknownPhysics,
      collider);
    throw new InvalidOperationException("An unknown active tile must be rejected.");
  }
  catch (InvalidOperationException exception)
    when (exception.Message != "An unknown active tile must be rejected.")
  {
    if (unknownTransform.X != 2.0f || unknownVelocity.X != 3.0f || !unknownPhysics.IsGrounded)
    {
      throw new InvalidOperationException("Unknown tile rejection must not partially resolve movement.");
    }
  }

  Console.WriteLine("PASS: tile collision distinguishes known and unknown tile definitions");
}

static void VerifyHalfBrickCollisionGeometry()
{
  TileCollisionSystem system = new();
  ColliderComponent collider = new(1.0f, 1.0f);
  WorldGrid world = new(400, 300);
  _ = world.TrySetTile(4, 10, new WorldTile(
    IsActive: true,
    Type: 1,
    IsHalfBrick: true));

  TransformComponent fallingTransform = new(4.0f, 12.0f);
  VelocityComponent fallingVelocity = new(0.0f, -3.0f);
  PhysicsStateComponent fallingPhysics = default;
  system.MoveAndResolve(
    world,
    ref fallingTransform,
    ref fallingVelocity,
    ref fallingPhysics,
    collider);
  if (fallingTransform.Y != 10.5f || fallingVelocity.Y != 0.0f ||
      !fallingPhysics.IsGrounded)
  {
    throw new InvalidOperationException(
      "A falling collider must land on the physical top of a half-brick.");
  }

  TransformComponent upperHalfTransform = new(2.0f, 10.5f);
  VelocityComponent upperHalfVelocity = new(3.0f, 0.0f);
  PhysicsStateComponent upperHalfPhysics = default;
  system.MoveAndResolve(
    world,
    ref upperHalfTransform,
    ref upperHalfVelocity,
    ref upperHalfPhysics,
    collider);
  if (upperHalfTransform.X != 5.0f || upperHalfVelocity.X != 3.0f)
  {
    throw new InvalidOperationException(
      "A collider overlapping only the upper half of a half-brick must pass horizontally.");
  }

  TransformComponent lowerHalfTransform = new(2.0f, 10.25f);
  VelocityComponent lowerHalfVelocity = new(3.0f, 0.0f);
  PhysicsStateComponent lowerHalfPhysics = default;
  system.MoveAndResolve(
    world,
    ref lowerHalfTransform,
    ref lowerHalfVelocity,
    ref lowerHalfPhysics,
    collider);
  if (lowerHalfTransform.X != 3.0f || lowerHalfVelocity.X != 0.0f)
  {
    throw new InvalidOperationException(
      "A collider overlapping the physical half-brick must be horizontally blocked.");
  }

  TransformComponent upwardTransform = new(4.0f, 9.8f);
  VelocityComponent upwardVelocity = new(0.0f, 1.0f);
  PhysicsStateComponent upwardPhysics = default;
  system.MoveAndResolve(
    world,
    ref upwardTransform,
    ref upwardVelocity,
    ref upwardPhysics,
    collider);
  if (upwardTransform.Y != 9.0f || upwardVelocity.Y != 0.0f || upwardPhysics.IsGrounded)
  {
    throw new InvalidOperationException(
      "A half-brick must retain its physical lower edge for upward collision.");
  }

  TransformComponent standingTransform = new(4.0f, 10.5f);
  VelocityComponent standingVelocity = new(0.0f, -0.1f);
  PhysicsStateComponent standingPhysics = default;
  system.MoveAndResolve(
    world,
    ref standingTransform,
    ref standingVelocity,
    ref standingPhysics,
    collider);
  if (standingTransform.Y != 10.5f || standingVelocity.Y != 0.0f ||
      !standingPhysics.IsGrounded)
  {
    throw new InvalidOperationException(
      "A collider aligned on a half-brick top must remain grounded.");
  }

  Console.WriteLine("PASS: half-bricks use lower-half runtime collision geometry");
}

static void VerifyTopSlopeContactGeometry()
{
  TileCollisionSystem system = new();
  TopSlopeContactSystem topSlopeSystem = new();
  ColliderComponent collider = new(1.0f, 2.0f);

  WorldGrid slopeOneWorld = new(400, 300);
  _ = slopeOneWorld.TrySetTile(4, 10, new WorldTile(IsActive: true, Type: 1, Slope: 1));
  TransformComponent slopeOneTransform = new(4.0f, 11.5f);
  VelocityComponent slopeOneVelocity = new(0.25f, -1.0f);
  PhysicsStateComponent slopeOnePhysics = default;
  system.MoveAndResolve(
    slopeOneWorld,
    ref slopeOneTransform,
    ref slopeOneVelocity,
    ref slopeOnePhysics,
    collider,
    deferTopSlopeCollision: true);
  topSlopeSystem.Resolve(
    slopeOneWorld,
    ref slopeOneTransform,
    ref slopeOneVelocity,
    ref slopeOnePhysics,
    collider);
  if (slopeOneTransform.X != 4.25f || slopeOneTransform.Y != 10.75f ||
      slopeOneVelocity.Y != 0.0f || !slopeOnePhysics.IsGrounded)
  {
    throw new InvalidOperationException(
      "Slope 1 must resolve a downward player contact against its diagonal top surface.");
  }

  WorldGrid slopeTwoWorld = new(400, 300);
  _ = slopeTwoWorld.TrySetTile(4, 10, new WorldTile(IsActive: true, Type: 1, Slope: 2));
  TransformComponent slopeTwoTransform = new(3.75f, 11.3f);
  VelocityComponent slopeTwoVelocity = new(-0.25f, -1.0f);
  PhysicsStateComponent slopeTwoPhysics = default;
  system.MoveAndResolve(
    slopeTwoWorld,
    ref slopeTwoTransform,
    ref slopeTwoVelocity,
    ref slopeTwoPhysics,
    collider,
    deferTopSlopeCollision: true);
  topSlopeSystem.Resolve(
    slopeTwoWorld,
    ref slopeTwoTransform,
    ref slopeTwoVelocity,
    ref slopeTwoPhysics,
    collider);
  if (slopeTwoTransform.X != 3.5f || slopeTwoTransform.Y != 10.5f ||
      slopeTwoVelocity.Y != 0.0f || !slopeTwoPhysics.IsGrounded)
  {
    throw new InvalidOperationException(
      "Slope 2 must resolve a downward player contact against its diagonal top surface.");
  }

  WorldGrid unknownWorld = new(400, 300);
  _ = unknownWorld.TrySetTile(4, 10, new WorldTile(IsActive: true, Type: 753, Slope: 1));
  TransformComponent unknownTransform = new(4.0f, 10.8f);
  VelocityComponent unknownVelocity = new(0.0f, -0.5f);
  PhysicsStateComponent unknownPhysics = new() { IsGrounded = true };
  try
  {
    topSlopeSystem.Resolve(
      unknownWorld,
      ref unknownTransform,
      ref unknownVelocity,
      ref unknownPhysics,
      collider);
    throw new InvalidOperationException("An unknown active top slope must be rejected.");
  }
  catch (InvalidOperationException exception)
    when (exception.Message != "An unknown active top slope must be rejected.")
  {
    if (unknownTransform.X != 4.0f || unknownTransform.Y != 10.8f ||
        unknownVelocity.Y != -0.5f || !unknownPhysics.IsGrounded)
    {
      throw new InvalidOperationException(
        "Unknown top-slope rejection must not partially resolve player physics.");
    }
  }

  Console.WriteLine("PASS: top slopes use player surface-contact geometry");
}

static void VerifyPlayerTopSlopeComposition()
{
  WorldGrid world = new(400, 300);
  _ = world.TrySetTile(22, 20, new WorldTile(IsActive: true, Type: 1, Slope: 1));
  using DomeSimulation simulation = new(world);
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(19.25f, 21.5f));

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: true,
    Jump: false,
    Fire: false)));

  PlayerSnapshot snapshot = simulation.CreateSnapshot().FindPlayer(player);
  if (snapshot.Position.X != 22.25f || snapshot.Position.Y != 20.75f ||
      !snapshot.IsGrounded)
  {
    throw new InvalidOperationException(
      "The player collision composition must resolve a slope top contact after ordinary movement.");
  }

  Console.WriteLine("PASS: player composition resolves top-slope contact after tile movement");
}

static void VerifyBottomSlopeDeferContract()
{
  if (!BottomSlopeCollisionRuleSystem.ShouldDeferWholeTileCollision(
        slope: 3,
        previousY: 10.0f,
        absoluteHorizontalVelocity: 0.25f,
        tileX: 4.0f,
        tileY: 10.0f,
        colliderLeft: 4.0f,
        colliderWidth: 1.0f))
  {
    throw new InvalidOperationException("Slope 3 must defer its whole-tile collision on the source path.");
  }

  if (!BottomSlopeCollisionRuleSystem.ShouldDeferWholeTileCollision(
        slope: 4,
        previousY: 10.0f,
        absoluteHorizontalVelocity: 0.25f,
        tileX: 4.0f,
        tileY: 10.0f,
        colliderLeft: 4.0f,
        colliderWidth: 1.0f))
  {
    throw new InvalidOperationException("Slope 4 must defer its whole-tile collision on the source path.");
  }

  if (BottomSlopeCollisionRuleSystem.ShouldDeferWholeTileCollision(
        slope: 3,
        previousY: 9.0f,
        absoluteHorizontalVelocity: 0.25f,
        tileX: 4.0f,
        tileY: 10.0f,
        colliderLeft: 4.0f,
        colliderWidth: 1.0f) ||
      BottomSlopeCollisionRuleSystem.ShouldDeferWholeTileCollision(
        slope: 0,
        previousY: 10.0f,
        absoluteHorizontalVelocity: 0.25f,
        tileX: 4.0f,
        tileY: 10.0f,
        colliderLeft: 4.0f,
        colliderWidth: 1.0f))
  {
    throw new InvalidOperationException("Bottom-slope defer must preserve its direction and slope guards.");
  }

  Console.WriteLine("PASS: bottom-slope whole-tile defer predicate is source-faithful and pure");
}

static void VerifyTileCollisionActivationContract()
{
  if (!TileCollisionActivationRuleSystem.ShouldParticipate(isActive: true, isInactive: false))
  {
    throw new InvalidOperationException("An active tile must enter the collision candidate path.");
  }

  if (TileCollisionActivationRuleSystem.ShouldParticipate(isActive: false, isInactive: false) ||
      TileCollisionActivationRuleSystem.ShouldParticipate(isActive: true, isInactive: true))
  {
    throw new InvalidOperationException(
      "Inactive or absent tiles must be excluded before collision geometry resolution.");
  }

  Console.WriteLine("PASS: tile active/inactive collision candidate rule is explicit and pure");
}

static void VerifyCommittedActuatorInactiveCollision()
{
  TileCollisionSystem system = new();
  ColliderComponent collider = new(1.0f, 1.0f);
  WorldGrid world = new(400, 300);
  WorldTile originalTile = new(
    IsActive: true,
    Type: 1,
    FrameX: 18,
    FrameY: 36,
    WallType: 4,
    IsActuated: true);
  _ = world.TrySetTile(4, 10, originalTile);

  TransformComponent blockedTransform = new(2.0f, 10.0f);
  VelocityComponent blockedVelocity = new(3.0f, 0.0f);
  PhysicsStateComponent blockedPhysics = default;
  system.MoveAndResolve(
    world,
    ref blockedTransform,
    ref blockedVelocity,
    ref blockedPhysics,
    collider);
  if (blockedTransform.X != 3.0f || blockedVelocity.X != 0.0f)
  {
    throw new InvalidOperationException(
      "An active actuator tile must initially block movement.");
  }

  world.EnqueueTileChange(new TileChangeCommand(
    Sequence: 1,
    X: 4,
    Y: 10,
    Kind: TileChangeKind.SetInactive,
    TileType: 1,
    IsInactive: true));
  world.CommitTileChanges();
  TransformComponent passThroughTransform = new(2.0f, 10.0f);
  VelocityComponent passThroughVelocity = new(3.0f, 0.0f);
  PhysicsStateComponent passThroughPhysics = default;
  system.MoveAndResolve(
    world,
    ref passThroughTransform,
    ref passThroughVelocity,
    ref passThroughPhysics,
    collider);
  if (passThroughTransform.X != 5.0f || passThroughVelocity.X != 3.0f)
  {
    throw new InvalidOperationException(
      "A committed inactive actuator tile must be excluded from collision.");
  }

  world.EnqueueTileChange(new TileChangeCommand(
    Sequence: 2,
    X: 4,
    Y: 10,
    Kind: TileChangeKind.SetInactive,
    TileType: 1,
    IsInactive: false));
  world.CommitTileChanges();
  TransformComponent restoredTransform = new(2.0f, 10.0f);
  VelocityComponent restoredVelocity = new(3.0f, 0.0f);
  PhysicsStateComponent restoredPhysics = default;
  system.MoveAndResolve(
    world,
    ref restoredTransform,
    ref restoredVelocity,
    ref restoredPhysics,
    collider);
  if (world.GetTile(4, 10) != originalTile || restoredTransform.X != 3.0f ||
      restoredVelocity.X != 0.0f)
  {
    throw new InvalidOperationException(
      "Reactivation must preserve the actuator tile and restore collision participation.");
  }

  Console.WriteLine("PASS: committed actuator inactive state controls runtime collision");
}

static void VerifyLegacyPlatformFallThroughContract()
{
  if (PlatformCollisionRuleSystem.ShouldCollideFromAbove(
        isPlatform: true,
        isProperTopFrame: true,
        fallThrough: true,
        fall2: false,
        legacyVelocityY: 0.5f))
  {
    throw new InvalidOperationException(
      "Legacy fall-through must skip a platform at low downward velocity.");
  }

  if (!PlatformCollisionRuleSystem.ShouldCollideFromAbove(
        isPlatform: true,
        isProperTopFrame: true,
        fallThrough: true,
        fall2: false,
        legacyVelocityY: 1.5f))
  {
    throw new InvalidOperationException(
      "Legacy fall-through must not skip a platform above the velocity threshold.");
  }

  if (PlatformCollisionRuleSystem.ShouldCollideFromAbove(
        isPlatform: true,
        isProperTopFrame: true,
        fallThrough: true,
        fall2: true,
        legacyVelocityY: 1.5f))
  {
    throw new InvalidOperationException("Legacy fall2 must preserve platform pass-through.");
  }

  if (PlatformCollisionRuleSystem.ShouldCollideFromAbove(
        isPlatform: false,
        isProperTopFrame: true,
        fallThrough: true,
        fall2: false,
        legacyVelocityY: 0.5f))
  {
    throw new InvalidOperationException("Non-platform tiles must not use top-surface skipping.");
  }

  if (PlatformCollisionRuleSystem.ShouldCollideFromAbove(
        isPlatform: true,
        isProperTopFrame: false,
        fallThrough: true,
        fall2: false,
        legacyVelocityY: 0.5f))
  {
    throw new InvalidOperationException("Non-top platform frames must not be treated as top surfaces.");
  }

  Console.WriteLine("PASS: legacy platform fall-through predicate is explicit and pure");
}

static void VerifyPlayerInputControlsPlatformFallThrough()
{
  WorldGrid world = new(400, 300);
  _ = world.TrySetTile(4, 10, new WorldTile(IsActive: true, Type: 19));
  using DomeSimulation simulation = new(world);
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(4.0f, 12.0f));

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    Down: true)));

  PlayerSnapshot snapshot = simulation.CreateSnapshot().FindPlayer(player);
  if (snapshot.Position.Y != 11.0f || snapshot.IsGrounded)
  {
    throw new InvalidOperationException(
      "A player Down input must pass through the platform top during the same tick.");
  }

  Console.WriteLine("PASS: player Down input reaches platform fall-through collision");
}
