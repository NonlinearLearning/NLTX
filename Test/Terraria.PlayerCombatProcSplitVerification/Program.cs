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
var tickQuery = new PlayerCombatProcTickQuery();

AssertEqual(99999f, component.LifeSteal, "The life steal budget preserves the source default.");
AssertEqual(-1, component.EocHit, "The dash target preserves the source sentinel.");

var tickInput = new PlayerCombatProcTickInput(
  GhostDmg: 6.6666665f,
  LifeSteal: 79.7f,
  EocDash: 1,
  EocHit: 17,
  InfernoCounter: 179,
  StarCloakCooldown: 1,
  TitaniumStormCooldown: 1,
  PetalTimer: 1,
  BoneGloveTimer: 1,
  ExpertMode: false);
PlayerCombatProcTickResult tickResult = tickQuery.Advance(tickInput);

AssertEqual(tickInput, new PlayerCombatProcTickInput(
  GhostDmg: 6.6666665f,
  LifeSteal: 79.7f,
  EocDash: 1,
  EocHit: 17,
  InfernoCounter: 179,
  StarCloakCooldown: 1,
  TitaniumStormCooldown: 1,
  PetalTimer: 1,
  BoneGloveTimer: 1,
  ExpertMode: false), "The tick query cannot mutate its value input.");
AssertEqual(0f, tickResult.GhostDmg, "The tick query clamps ghost damage at zero.");
AssertEqual(80f, tickResult.LifeSteal, "The tick query clamps normal life steal.");
AssertEqual(0, tickResult.EocDash, "The tick query closes a one-tick dash window.");
AssertEqual(-1, tickResult.EocHit, "Closing a dash window clears the target sentinel.");
AssertEqual(0, tickResult.InfernoCounter, "The tick query wraps the inferno counter.");
AssertEqual(0, tickResult.StarCloakCooldown, "The tick query decrements cooldowns once.");
AssertEqual(0, tickResult.TitaniumStormCooldown, "The tick query does not underflow cooldowns.");
AssertEqual(0, tickResult.PetalTimer, "The tick query decrements petal time.");
AssertEqual(0, tickResult.BoneGloveTimer, "The tick query decrements glove time.");

Guid eventId = Guid.Parse("20000000-0000-0000-0000-000000000001");
Guid sourceId = Guid.Parse("30000000-0000-0000-0000-000000000001");
var committedHit = new PlayerCommittedCombatHitEvent(
  EventId: eventId,
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

Assert(system.AcceptCommittedHit(committedHit), "A committed hit updates proc state.");
Assert(!system.AcceptCommittedHit(committedHit), "A duplicate committed hit is ignored.");
AssertEqual(120f, component.GhostDmg, "A duplicate does not add ghost damage twice.");
AssertEqual(99987f, component.LifeSteal, "A duplicate does not spend life steal twice.");

system.AdvanceTick(expertMode: true);
Assert(component.GhostDmg < 120f, "A tick decays ghost damage.");
AssertEqual(70f, component.LifeSteal, "Expert recovery clamps the budget at the source cap.");

system.BeginEocDash(2);
Assert(system.CommitEocDashHit(17), "An open dash window accepts a target.");
system.AdvanceTick(expertMode: false);
system.AdvanceTick(expertMode: false);
AssertEqual(0, component.EocDash, "A dash window closes at zero.");
AssertEqual(-1, component.EocHit, "Closing a dash window clears the target sentinel.");

component.StarCloakCooldown = 2;
component.TitaniumStormCooldown = 1;
component.PetalTimer = 1;
component.BoneGloveTimer = 1;
component.HasTitaniumStormBuff = true;
system.AdvanceTick(expertMode: false);
AssertEqual(1, component.StarCloakCooldown, "A cooldown decrements once per tick.");
AssertEqual(0, component.TitaniumStormCooldown, "A cooldown does not underflow.");

system.ResetEffects();
Assert(!component.OnHitDodge, "An effect reset clears transient hit flags.");
Assert(!component.HasTitaniumStormBuff, "An effect reset clears derived buff state.");

PlayerCombatProcSnapshot beforeQuery = query.Snapshot(component);
AssertEqual(beforeQuery, query.Snapshot(component), "A proc query does not mutate state.");

system.Reset();
Assert(system.AcceptCommittedHit(committedHit), "A lifecycle reset clears replay history.");
AssertEqual(99987f, component.LifeSteal, "A reset restores the life steal budget before a new hit.");

Console.WriteLine("PASS: player combat proc split behavior");
