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

var component = new PlayerAccessoryStringEffectComponent();
var system = new PlayerAccessoryEffectRebuildSystem();
var query = new PlayerAccessoryCapabilityQuery();

AssertEqual(2, component.ExtraAccessorySlots, "The component must preserve the source declaration default.");
AssertEqual(-1, component.TankPet, "The component must preserve the source tank pet default.");

var input = new PlayerAccessoryEffectRebuildInput(
  ExtraAccessory: true,
  TankPet: 42,
  StringColor: 7,
  CounterWeight: 556,
  VanityCounterWeight: 561,
  MagicString: true,
  YoyoString: true,
  YoyoGlove: true,
  RapidAttackBonus: 0.02f,
  StressBall: true,
  StaffOfRegrowthBonus: true);
system.Rebuild(component, input);
system.ResetForTick(component, expertMode: true, gameMenu: false);
AssertEqual(1, component.ExtraAccessorySlots, "Expert mode should expose one extra accessory slot.");
Assert(Math.Abs(component.RapidAttackBonus - 0.015f) < 0.00001f, "ResetForTick should decay rapid attack bonus by one tick.");
Assert(!component.StressBall, "ResetForTick should clear the transient stress ball capability.");
Assert(!component.StaffOfRegrowthBonus, "ResetForTick should clear the transient staff capability.");
Assert(!component.YoyoGlove, "ResetForTick should clear transient yoyo glove state.");
AssertEqual(0, component.CounterWeight, "ResetForTick should clear the counterweight capability.");

system.Rebuild(component, input);
Assert(system.ConsumeStressBallEdge(component), "The first stress ball transition should be observable.");
Assert(!system.ConsumeStressBallEdge(component), "Repeating the same stress ball state must be idempotent.");
system.ResetForTick(component, expertMode: false, gameMenu: false);
AssertEqual(0, component.ExtraAccessorySlots, "Normal mode should not expose the extra slot.");
system.Rebuild(component, input with { StressBall = false });
Assert(system.ConsumeStressBallEdge(component), "Removing the stress ball capability should produce an edge.");

system.Rebuild(component, input with { CounterWeight = 556, VanityCounterWeight = 561 });
AssertEqual(561, query.SelectCounterWeightType(component), "Vanity counterweight must take precedence.");
system.Rebuild(component, input with { VanityCounterWeight = 0 });
AssertEqual(556, query.SelectCounterWeightType(component), "Regular counterweight must be the fallback.");
Assert(query.HasYoyoCapability(component), "A regular counterweight should expose yoyo capability.");

system.Rebuild(component, input with { TankPet = 42 });
Assert(!system.AdvanceTankPetReset(component), "The first tank pet reset pass should mark the reset.");
AssertEqual(42, component.TankPet, "The first tank pet reset pass must retain the pet relation.");
Assert(system.AdvanceTankPetReset(component), "The second tank pet reset pass should clear the relation.");
AssertEqual(-1, component.TankPet, "The cleared tank pet relation should use the source sentinel.");

system.Rebuild(component, input with { RapidAttackBonus = 0.01f });
PlayerAccessoryCapabilitySnapshot beforeQuery = query.Snapshot(component);
_ = query.Snapshot(component);
AssertEqual(beforeQuery, query.Snapshot(component), "The capability query must not mutate component state.");

system.ResetForSpawn(component);
AssertEqual(2, component.ExtraAccessorySlots, "Spawn reset should restore the declaration slot default.");
AssertEqual(-1, component.TankPet, "Spawn reset should clear the tank pet relation.");
AssertEqual(0f, component.RapidAttackBonus, "Spawn reset should clear transient rapid attack bonus.");
Assert(!component.StressBallPrevious, "Spawn reset should clear the stress edge memory.");

Console.WriteLine("PASS: player accessory string effects rebuild, reset, query and edge semantics");
