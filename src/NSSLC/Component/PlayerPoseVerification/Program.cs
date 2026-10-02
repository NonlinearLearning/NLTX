using System.Numerics;
using Terraria.Player;

PlayerPoseAndAnimationStateComponent component = new();
PlayerPoseAndAnimationSystem system = new();

PlayerPoseInputSnapshot firstInput = new(
  Tick: new SimulationTick(5),
  HeadVelocityOverride: new Vector2(2.0f, 3.0f),
  BodyVelocityOverride: new Vector2(-4.0f, 5.0f),
  LegVelocityOverride: new Vector2(0.5f, -1.0f),
  FullRotation: 0.4f,
  FullRotationOrigin: new Vector2(8.0f, 9.0f),
  FartKartCloudDelay: -2,
  GfxOffY: 1.5f,
  StepSpeed: 2.0f);

PlayerPoseSnapshot firstSnapshot = system.Update(component, firstInput);
Require(firstSnapshot.Tick == new SimulationTick(5), "The pose tick must be preserved.");
Require(firstSnapshot.HeadPosition == new Vector2(2.0f, 3.0f),
  "The head position must advance from its input velocity.");
Require(firstSnapshot.BodyPosition == new Vector2(-4.0f, 5.0f),
  "The body position must advance from its input velocity.");
Require(firstSnapshot.LegPosition == new Vector2(0.5f, -1.0f),
  "The leg position must advance from its input velocity.");
Require(firstSnapshot.HeadRotation == 0.2f, "The head rotation must use horizontal velocity.");
Require(firstSnapshot.BodyRotation == -0.4f, "The body rotation must use horizontal velocity.");
Require(firstSnapshot.LegRotation == 0.05f, "The leg rotation must use horizontal velocity.");
Require(firstSnapshot.HeadVelocity == new Vector2(1.98f, 3.1f),
  "The head velocity must apply damping and vertical acceleration.");
Require(firstSnapshot.BodyVelocity == new Vector2(-3.96f, 5.1f),
  "The body velocity must apply damping and vertical acceleration.");
Require(firstSnapshot.LegVelocity == new Vector2(0.495f, -0.9f),
  "The leg velocity must apply damping and vertical acceleration.");
Require(firstSnapshot.FullRotation == 0.4f, "Full rotation must be preserved.");
Require(firstSnapshot.FullRotationOrigin == new Vector2(8.0f, 9.0f),
  "Full rotation origin must be preserved.");
Require(firstSnapshot.FartKartCloudDelay == 0, "The cloud delay must not remain negative.");
Require(firstSnapshot.GfxOffY == 1.5f, "The graphics offset must be preserved.");
Require(firstSnapshot.StepSpeed == 2.0f, "The step speed input must be preserved.");

PlayerPoseSnapshot heldSnapshot = system.Update(
  component,
  new PlayerPoseInputSnapshot(Tick: new SimulationTick(6), AdvancePose: false));
Require(heldSnapshot.HeadPosition == firstSnapshot.HeadPosition,
  "An early-return tick must preserve head position.");
Require(heldSnapshot.BodyPosition == firstSnapshot.BodyPosition,
  "An early-return tick must preserve body position.");
Require(heldSnapshot.LegPosition == firstSnapshot.LegPosition,
  "An early-return tick must preserve leg position.");
Require(heldSnapshot.HeadVelocity == firstSnapshot.HeadVelocity,
  "An early-return tick must preserve head velocity.");
Require(heldSnapshot.Tick == new SimulationTick(6), "An early-return tick must update the tick value.");

PlayerPoseSnapshot spawnReset = system.ResetForSpawn(component, new SimulationTick(7));
Require(spawnReset.HeadPosition == Vector2.Zero, "Spawn reset must clear head position.");
Require(spawnReset.BodyPosition == Vector2.Zero, "Spawn reset must clear body position.");
Require(spawnReset.LegPosition == Vector2.Zero, "Spawn reset must clear leg position.");
Require(spawnReset.HeadRotation == 0.0f, "Spawn reset must clear head rotation.");
Require(spawnReset.BodyRotation == 0.0f, "Spawn reset must clear body rotation.");
Require(spawnReset.LegRotation == 0.0f, "Spawn reset must clear leg rotation.");
Require(spawnReset.HeadVelocity == firstSnapshot.HeadVelocity,
  "Spawn reset must not claim ownership of velocity reset.");

PlayerPoseSnapshot teleportReset = system.ResetForTeleport(component, new SimulationTick(8));
Require(teleportReset.HeadPosition == Vector2.Zero, "Teleport reset must clear head position.");
Require(teleportReset.BodyPosition == Vector2.Zero, "Teleport reset must clear body position.");
Require(teleportReset.LegPosition == Vector2.Zero, "Teleport reset must clear leg position.");
Require(teleportReset.HeadRotation == 0.0f, "Teleport reset must clear head rotation.");
Require(teleportReset.BodyRotation == 0.0f, "Teleport reset must clear body rotation.");
Require(teleportReset.LegRotation == 0.0f, "Teleport reset must clear leg rotation.");

Console.WriteLine("PASS: player pose progression and reset");

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
