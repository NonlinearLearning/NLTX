using Terraria.Player.Movement;
using Terraria.Player.Environment;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertClose(float expected, float actual, string message)
{
  if (MathF.Abs(expected - actual) > 0.00001f)
  {
    throw new InvalidOperationException(
      $"{message} Expected '{expected}', actual '{actual}'.");
  }
}

static PlayerTraversalPhysicsInput CreateInput(
  bool portalPhysicsEnabled = false,
  bool wet = false,
  bool downDash = false,
  bool shimmerWet = false,
  bool shimmering = false,
  bool honeyWet = false,
  bool merman = false,
  bool trident = false,
  bool lavaWet = false,
  bool controlUp = false,
  bool vortexDebuff = false)
{
  return new PlayerTraversalPhysicsInput(
    DefaultGravity: 0.4f,
    PortalPhysicsEnabled: portalPhysicsEnabled,
    Wet: wet,
    IsPerformingJumpDownDash: downDash,
    ShimmerWet: shimmerWet,
    Shimmering: shimmering,
    HoneyWet: honeyWet,
    Merman: merman,
    Trident: trident,
    LavaWet: lavaWet,
    ControlUp: controlUp,
    VortexDebuff: vortexDebuff);
}

var system = new PlayerTraversalPhysicsSystem();
PlayerTraversalPhysicsResult baseline = system.Resolve(CreateInput());
AssertClose(0.4f, baseline.Gravity,
  "The default frame should preserve explicit default gravity.");
AssertClose(10.01f, baseline.MaxFallSpeed,
  "The default frame should apply the per-frame fall-speed increment.");
AssertClose(3f, baseline.MaxRunSpeed,
  "The default frame should reset maximum run speed.");
AssertClose(0.08f, baseline.RunAcceleration,
  "The default frame should reset run acceleration.");
AssertClose(0.2f, baseline.RunSlowdown,
  "The default frame should reset run slowdown.");
Assert(baseline.JumpHeight == 15,
  "The default frame should reset jump height.");
AssertClose(5.01f, baseline.JumpSpeed,
  "The default frame should reset jump speed.");

PlayerTraversalPhysicsResult trident = system.Resolve(CreateInput(
  wet: true,
  trident: true,
  controlUp: true));
AssertClose(0.1f, trident.Gravity,
  "Trident ascent should override the normal wet gravity.");
AssertClose(2.01f, trident.MaxFallSpeed,
  "Trident ascent should override the normal wet fall-speed cap.");
Assert(trident.JumpHeight == 25,
  "Trident water traversal should set its jump-height baseline.");
AssertClose(5.51f, trident.JumpSpeed,
  "Trident water traversal should set its jump-speed baseline.");

PlayerTraversalPhysicsResult portalDownDash = system.Resolve(CreateInput(
  portalPhysicsEnabled: true,
  wet: true,
  downDash: true,
  vortexDebuff: true));
AssertClose(0f, portalDownDash.Gravity,
  "Vortex should override gravity after the wet down-dash adjustment.");
AssertClose(29.76f, portalDownDash.MaxFallSpeed,
  "Wet down-dash should scale the portal fall cap before the frame increment.");
Assert(portalDownDash.JumpHeight == 15,
  "Wet down-dash should not replace the reset jump-height baseline.");

var state = new PlayerMovementPhysicsStateComponent();
system.CommitMovementState(state, trident);
AssertClose(trident.Gravity, state.Gravity,
  "The commit should write gravity to the movement state component.");
AssertClose(trident.MaxFallSpeed, state.MaxFallSpeed,
  "The commit should write fall speed to the movement state component.");
AssertClose(trident.MaxRunSpeed, state.MaxRunSpeed,
  "The commit should write run speed to the movement state component.");
AssertClose(trident.RunAcceleration, state.RunAcceleration,
  "The commit should write acceleration to the movement state component.");
AssertClose(trident.RunSlowdown, state.RunSlowdown,
  "The commit should write slowdown to the movement state component.");

PlayerTraversalCapabilitySnapshot snapshot = new PlayerTraversalCapabilitySystem()
  .CreateSnapshot(
    state,
    new PlayerGravityAndWaterTraversalStateComponent(),
    new PlayerEnvironmentMobilityStateComponent());
AssertClose(trident.Gravity, snapshot.Gravity,
  "The traversal snapshot should expose committed gravity.");
AssertClose(trident.MaxFallSpeed, snapshot.MaxFallSpeed,
  "The traversal snapshot should expose the committed fall-speed cap.");
Assert(!snapshot.WaterWalk && !snapshot.Gills,
  "The traversal snapshot should expose the supplied gravity and mobility facts.");

Console.WriteLine(
  "PASS: player traversal physics, state commit and capability snapshot");
