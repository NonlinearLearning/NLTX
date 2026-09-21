using Terraria.Player;

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

var component = new PlayerBarrierAndRegenComponent();
var system = new PlayerBarrierSystem(component);
var projection = new PlayerBarrierFrameProjection();

Assert(!component.IceBarrier, "Ice barrier must start inactive.");
Assert(!component.PalladiumRegen, "Palladium regeneration must start inactive.");
AssertEqual((byte)0, component.IceBarrierFrame,
  "Ice barrier frame must start at zero.");
AssertEqual((byte)0, component.IceBarrierFrameCounter,
  "Ice barrier frame counter must start at zero.");

system.CommitCapabilities(new PlayerBarrierCapabilityInput(
  CurrentLife: 50,
  EffectiveLifeMaximum: 100,
  IceBarrierBuffActive: true,
  PalladiumRegen: true));
Assert(component.IceBarrier,
  "A valid half-life barrier capability should be active.");
Assert(component.PalladiumRegen,
  "Committed palladium capability should be preserved.");

system.AdvanceTick();
system.AdvanceTick();
AssertEqual((byte)2, component.IceBarrierFrameCounter,
  "Frame counter should advance once per active simulation tick.");
AssertEqual((byte)0, component.IceBarrierFrame,
  "Frame should not advance before the source threshold.");

system.AdvanceTick();
AssertEqual((byte)0, component.IceBarrierFrameCounter,
  "Frame counter should reset after exceeding two.");
AssertEqual((byte)1, component.IceBarrierFrame,
  "Frame should advance after three active ticks.");

for (int tick = 0; tick < 33; tick++)
{
  system.AdvanceTick();
}
AssertEqual((byte)0, component.IceBarrierFrame,
  "Ice barrier frame should wrap after twelve frames.");
AssertEqual((byte)0, component.IceBarrierFrameCounter,
  "Frame counter should remain bounded after repeated ticks.");

system.CommitCapabilities(new PlayerBarrierCapabilityInput(
  CurrentLife: 51,
  EffectiveLifeMaximum: 100,
  IceBarrierBuffActive: true,
  PalladiumRegen: true));
Assert(!component.IceBarrier,
  "Life above half must disable the ice barrier capability.");
Assert(component.PalladiumRegen,
  "Palladium regeneration must remain independently committed.");

system.CommitCapabilities(new PlayerBarrierCapabilityInput(
  CurrentLife: 50,
  EffectiveLifeMaximum: 100,
  IceBarrierBuffActive: false,
  PalladiumRegen: false));
Assert(!component.IceBarrier,
  "An absent barrier capability must disable the barrier.");
Assert(!component.PalladiumRegen,
  "An absent palladium capability must disable regeneration input.");

system.CommitCapabilities(new PlayerBarrierCapabilityInput(
  CurrentLife: 40,
  EffectiveLifeMaximum: 100,
  IceBarrierBuffActive: true,
  PalladiumRegen: true));
PlayerBarrierFrameSnapshot beforeProjection = projection.Snapshot(component);
PlayerBarrierFrameSnapshot afterProjection = projection.Snapshot(component);
AssertEqual(beforeProjection, afterProjection,
  "Barrier frame projection must be pure.");
Assert(component.IceBarrier,
  "Projection must not change barrier authority.");

system.ResetEffects();
Assert(!component.IceBarrier,
  "ResetEffects must clear the barrier authority.");
Assert(!component.PalladiumRegen,
  "ResetEffects must clear the regeneration capability.");

system.CommitCapabilities(new PlayerBarrierCapabilityInput(
  CurrentLife: 40,
  EffectiveLifeMaximum: 100,
  IceBarrierBuffActive: true,
  PalladiumRegen: true));
system.ResetForLifecycle();
Assert(!component.IceBarrier, "Lifecycle reset must clear the barrier.");
Assert(!component.PalladiumRegen,
  "Lifecycle reset must clear palladium regeneration.");
AssertEqual((byte)0, component.IceBarrierFrame,
  "Lifecycle reset must clear the presentation frame.");
AssertEqual((byte)0, component.IceBarrierFrameCounter,
  "Lifecycle reset must clear the frame counter.");

Console.WriteLine("PASS: player barrier threshold, regen capability and frame projection semantics");
