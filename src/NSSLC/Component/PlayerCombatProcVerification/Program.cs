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

var component = new PlayerCombatProcStateComponent();
var system = new PlayerCombatProcSystem(component);
var query = new PlayerCombatProcQuery();

AssertEqual(99999f, component.LifeSteal, "The life steal budget must preserve the source declaration default.");
AssertEqual(-1, component.EocHit, "The dash hit compatibility field must preserve the source sentinel.");

Guid firstEventId = Guid.Parse("20000000-0000-0000-0000-000000000001");
Guid sourceId = Guid.Parse("30000000-0000-0000-0000-000000000001");
var committedHit = new PlayerCommittedCombatHitEvent(
  EventId: firstEventId,
  SourceId: sourceId,
  Damage: 100,
  SourceRevision: 7,
  IsCommitted: true,
  Effects: new PlayerCombatProcHitEffects(
    GhostDamage: 120f,
    LifeStealCost: 12f,
    OnHitDodge: true,
    OnHitRegen: true,
    OnHitPetal: false,
    OnHitTitaniumStorm: true));

Assert(system.AcceptCommittedHit(committedHit), "A committed hit should update proc state.");
Assert(component.OnHitDodge, "Committed dodge proc should be recorded.");
Assert(component.OnHitRegen, "Committed regen proc should be recorded.");
Assert(component.OnHitTitaniumStorm, "Committed titanium proc should be recorded.");
AssertEqual(120f, component.GhostDmg, "Committed ghost damage should be recorded.");
AssertEqual(99987f, component.LifeSteal, "Committed life steal cost should reduce the budget.");
Assert(!system.AcceptCommittedHit(committedHit), "A duplicate committed hit must be ignored.");
AssertEqual(99987f, component.LifeSteal, "A duplicate hit must not spend life steal twice.");
AssertEqual(120f, component.GhostDmg, "A duplicate hit must not add ghost damage twice.");

var rejectedHit = committedHit with
{
  EventId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
  IsCommitted = false,
  Damage = 0
};
Assert(!system.AcceptCommittedHit(rejectedHit), "An attempted or zero-damage hit must not update proc state.");

system.AdvanceTick(expertMode: true);
Assert(component.GhostDmg < 120f, "Ghost damage should decay on a simulation tick.");
Assert(component.LifeSteal <= 70f, "Expert mode life steal must be capped at the source upper bound.");

system.BeginEocDash(2);
AssertEqual(2, component.EocDash, "A dash window should be explicitly started.");
system.CommitEocDashHit(17);
AssertEqual(17, component.EocHit, "A committed dash hit should preserve the compatibility target identifier.");
system.AdvanceTick(expertMode: false);
AssertEqual(1, component.EocDash, "A dash window should decrement deterministically.");
system.AdvanceTick(expertMode: false);
AssertEqual(0, component.EocDash, "A dash window should close at zero.");
AssertEqual(-1, component.EocHit, "Closing a dash window should clear its hit identifier.");

component.InfernoCounter = 179;
component.StarCloakCooldown = 2;
component.TitaniumStormCooldown = 1;
component.PetalTimer = 1;
component.BoneGloveTimer = 1;
component.HasTitaniumStormBuff = true;
system.AdvanceTick(expertMode: false);
AssertEqual(0, component.InfernoCounter, "Inferno counter should wrap at the source threshold.");
AssertEqual(1, component.StarCloakCooldown, "Star cloak cooldown should decrement.");
AssertEqual(0, component.TitaniumStormCooldown, "Titanium storm cooldown should decrement.");
AssertEqual(0, component.PetalTimer, "Petal timer should decrement.");
AssertEqual(0, component.BoneGloveTimer, "Bone glove timer should decrement.");

system.ResetEffects();
Assert(!component.OnHitDodge, "ResetEffects should clear transient dodge proc state.");
Assert(!component.OnHitRegen, "ResetEffects should clear transient regen proc state.");
Assert(!component.OnHitTitaniumStorm, "ResetEffects should clear transient titanium proc state.");
Assert(!component.HasTitaniumStormBuff, "ResetEffects should clear the buff-derived titanium state.");

PlayerCombatProcSnapshot beforeQuery = query.Snapshot(component);
AssertEqual(beforeQuery, query.Snapshot(component), "The proc query must not mutate component state.");

system.Reset();
AssertEqual(99999f, component.LifeSteal, "Reset should restore the source life steal declaration default.");
AssertEqual(-1, component.EocHit, "Reset should restore the dash hit sentinel.");
Assert(system.AcceptCommittedHit(committedHit), "Reset should start a new lifecycle event history.");
AssertEqual(99987f, component.LifeSteal, "A committed hit in a new lifecycle should update the reset state.");

Console.WriteLine("PASS: player combat proc committed-hit, timer and query semantics");
