using System.Numerics;
using Terraria.Player.Armor;
using Terraria.Player.Grapple;
using Terraria.Player.Jump;
using Terraria.Player.Mobility;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

var mobility = new PlayerMobilitySystem();
var jumps = new PlayerJumpAvailabilityComponent
{
  HasCloudOption = true,
  HasSandstormOption = false,
  CanJumpAgainSandstorm = true,
};
var execution = new PlayerJumpExecutionComponent
{
  IsPerformingDownDash = true,
};
mobility.RefreshDoubleJumps(jumps, execution);
Assert(jumps.CanJumpAgainCloud && jumps.CanJumpAgainSandstorm,
  "RefreshDoubleJumps restores available options and preserves other availability flags.");
Assert(!execution.IsPerformingDownDash,
  "RefreshDoubleJumps clears the down-dash execution marker.");

var flight = new PlayerFlightStateComponent { WingTime = 1f, WingTimeMax = 60 };
var rocket = new PlayerRocketStateComponent
{
  RocketTime = 1,
  RocketTimeMax = 7,
  RocketDelay = 12,
};
execution.IsPerformingDownDash = true;
mobility.RefreshMovementAbilities(flight, rocket, doubleJumps: false);
Assert(flight.WingTime == 60f && rocket.RocketTime == 7 && rocket.RocketDelay == 0,
  "RefreshMovementAbilities restores flight and rocket resources.");
Assert(execution.IsPerformingDownDash,
  "A refresh that skips double jumps does not rewrite jump execution state.");
mobility.RefreshMovementAbilities(
  flight,
  rocket,
  doubleJumps: true,
  availability: jumps,
  execution: execution);
Assert(!execution.IsPerformingDownDash,
  "A refresh that includes double jumps clears the down-dash execution marker.");

var jumpModifiers = new PlayerJumpMobilityModifiersComponent();
var jumpParameters = mobility.UpdateJumpParameters(
  jumpModifiers,
  new PlayerJumpParameterInput(
    15,
    5.01f,
    false,
    0,
    0f,
    true,
    true,
    true,
    true,
    true,
    true,
    false,
    false));
Assert(jumpParameters.JumpHeight == 28 &&
  MathF.Abs(jumpParameters.JumpSpeed - 12.71f) < 0.001f &&
  jumpModifiers.JumpSpeedBoost == 6f && jumpModifiers.ExtraFall == 25,
  "Non-mount jump modifiers accumulate and update jump parameters in source order.");

var mountedJumpModifiers = new PlayerJumpMobilityModifiersComponent();
var mountedJumpParameters = mobility.UpdateJumpParameters(
  mountedJumpModifiers,
  new PlayerJumpParameterInput(
    15,
    5.01f,
    true,
    30,
    10f,
    true,
    true,
    true,
    true,
    true,
    true,
    true,
    true));
Assert(mountedJumpParameters.JumpHeight == 0 && mountedJumpParameters.JumpSpeed == 1f &&
  mountedJumpModifiers.JumpSpeedBoost == 0f && mountedJumpModifiers.ExtraFall == 0,
  "Mount jump values bypass equipment modifiers before sticky and dazed reductions.");

Assert(mobility.CanMoveForwardOnRope(true, false),
  "An active rope tile without a solid collision allows forward movement.");
Assert(!mobility.CanMoveForwardOnRope(true, true) &&
  !mobility.CanMoveForwardOnRope(false, false),
  "Rope movement is blocked by collision or a missing active rope tile.");

PlayerGrappleProjectileSnapshot[] standardGrapples =
[
  new PlayerGrappleProjectileSnapshot(0, 2f, new Vector2(20f, 0f), 0, 0, 0f),
];
var standardForce = mobility.GetGrapplingForces(new PlayerGrappleForcesInput(
  standardGrapples,
  Vector2.Zero,
  Vector2.Zero,
  new Vector2(2f, 3f),
  false,
  false,
  false,
  false,
  1f));
Assert(standardForce.HasForceContributingGrapple &&
  standardForce.PreferredVelocity == new Vector2(11f, 0f),
  "Grapple force averages the anchor and clamps to the standard speed.");

PlayerGrappleProjectileSnapshot[] oddWidthGrapples =
[
  new PlayerGrappleProjectileSnapshot(0, 2f, new Vector2(5f, 0f), 1, 1, 0f),
];
var oddWidthForce = mobility.GetGrapplingForces(new PlayerGrappleForcesInput(
  oddWidthGrapples,
  Vector2.Zero,
  Vector2.Zero,
  Vector2.Zero,
  false,
  false,
  false,
  false,
  1f));
Assert(oddWidthForce.PreferredVelocity == new Vector2(5f, 0f),
  "Grapple centers retain the source integer half-width calculation.");

PlayerGrappleProjectileSnapshot[] slimeGrapples =
[
  new PlayerGrappleProjectileSnapshot(315, 2f, new Vector2(20f, 0f), 0, 0, 0f),
];
var slimeForce = mobility.GetGrapplingForces(new PlayerGrappleForcesInput(
  slimeGrapples,
  Vector2.Zero,
  Vector2.Zero,
  Vector2.Zero,
  false,
  false,
  false,
  false,
  1f));
Assert(slimeForce.PreferredVelocity == new Vector2(14f, 0f),
  "The slime grapple uses its special maximum pull speed.");

PlayerGrappleProjectileSnapshot[] flareGrapples =
[
  new PlayerGrappleProjectileSnapshot(446, 2f, Vector2.Zero, 0, 0, 0f),
];
var flareForce = mobility.GetGrapplingForces(new PlayerGrappleForcesInput(
  flareGrapples,
  Vector2.Zero,
  Vector2.Zero,
  Vector2.Zero,
  false,
  false,
  false,
  false,
  1f));
Assert(flareForce.PreferredVelocity == new Vector2(0f, -11f),
  "A zero solar-flare direction falls back upward before speed clamping.");

PlayerGrappleProjectileSnapshot[] gravityGrapples =
[
  new PlayerGrappleProjectileSnapshot(652, 2f, new Vector2(100f, 0f), 0, 0, 0f),
];
var gravityForce = mobility.GetGrapplingForces(new PlayerGrappleForcesInput(
  gravityGrapples,
  Vector2.Zero,
  Vector2.Zero,
  Vector2.Zero,
  false,
  true,
  false,
  false,
  1f));
Assert(gravityForce.PreferredVelocity == new Vector2(6f, 0f),
  "The gravity grapple adds the control projection to its pull target.");

PlayerGrappleProjectileSnapshot[] directionalGrapples =
[
  new PlayerGrappleProjectileSnapshot(865, 2f, Vector2.Zero, 0, 0, MathF.PI / 2f),
];
var directionalForce = mobility.GetGrapplingForces(new PlayerGrappleForcesInput(
  directionalGrapples,
  Vector2.Zero,
  Vector2.Zero,
  Vector2.Zero,
  false,
  false,
  false,
  false,
  1f));
Assert(directionalForce.PreferredDirection == 1 &&
  MathF.Abs(directionalForce.PreferredVelocity.X + 11f) < 0.001f,
  "Directional grapple returns a facing intent and a capped pull velocity.");

PlayerGrappleProjectileSnapshot[] inactiveGrapples =
[
  new PlayerGrappleProjectileSnapshot(0, 1f, new Vector2(20f, 0f), 0, 0, 0f),
];
var inactiveForce = mobility.GetGrapplingForces(new PlayerGrappleForcesInput(
  inactiveGrapples,
  Vector2.Zero,
  Vector2.Zero,
  new Vector2(2f, 3f),
  false,
  false,
  false,
  false,
  1f));
Assert(!inactiveForce.HasForceContributingGrapple &&
  inactiveForce.PreferredVelocity == new Vector2(2f, 3f),
  "Grapple force preserves current velocity when no projectile is latched.");

var armorSet = new PlayerArmorSetSystem();
var solar = new PlayerSolarArmorStateComponent
{
  IsSolarDashing = false,
  SolarDashConsumedFlare = true,
};
armorSet.BeginSolarDash(solar);
Assert(solar.IsSolarDashing && !solar.SolarDashConsumedFlare,
  "Solar dash start marks the dash active without marking a flare consumed.");

var nebula = new PlayerNebulaResourceStateComponent();
var nebulaResult = armorSet.UpdateNebulaBuff(
  nebula,
  new PlayerNebulaBuffUpdateInput(
    PlayerNebulaResourceKind.Life,
    173,
    175,
    2));
Assert(nebulaResult.ResourceLevel == 2 && nebula.LifeLevel == 2 &&
  nebulaResult.BuffType == 174 && nebulaResult.BuffTime == 480 &&
  nebulaResult.ChangedBuffSlot,
  "An expiring higher-tier Nebula buff steps down and refreshes its timer.");

Console.WriteLine("PASS: P07 mobility and armor-set isolated core");
