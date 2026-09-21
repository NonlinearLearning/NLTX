using System;
using System.Collections.Generic;
using Terraria.Combat;
using Terraria.Npc;

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

static CombatContributorId Player(string accountUuid)
{
  return new CombatContributorId(CombatContributorKind.Player, accountUuid);
}

static CombatContributorId World()
{
  return new CombatContributorId(CombatContributorKind.World, null);
}

static HashSet<NpcTypeId> Active(params int[] typeValues)
{
  var types = new HashSet<NpcTypeId>();
  for (int index = 0; index < typeValues.Length; index++)
  {
    types.Add(new NpcTypeId(typeValues[index]));
  }

  return types;
}

static INpcDamageTrackingStrategy? CreateStrategy(NpcTypeId npcType)
{
  return npcType.Value switch
  {
    10 => new NpcDamageSingleTypeStrategy(npcType),
    20 or 21 => new NpcDamageCompositeStrategy(
      new NpcTypeId(20),
      new NpcTypeId(21)),
    _ => null,
  };
}

static NpcDamageTrackingSystem CreateSystem()
{
  return new NpcDamageTrackingSystem(CreateStrategy);
}

static NpcDamageTrackerSnapshot Only(
  NpcDamageTrackerSnapshot[] snapshots,
  string message)
{
  AssertEqual(1, snapshots.Length, message);
  return snapshots[0];
}

var singleType = new NpcTypeId(10);
var system = CreateSystem();
system.AdvanceTo(0, Active(10));

Assert(
  system.TryRecordAppliedDamage(singleType, Player("player-a"), 4, 0),
  "The first positive applied amount should create an active tracker.");
Assert(
  system.TryRecordAppliedDamage(singleType, Player("player-b"), 3, 0),
  "A second contributor should be recorded in the same encounter.");
Assert(
  system.TryRecordAppliedDamage(singleType, World(), 2, 0),
  "World damage should be recorded separately from player credit.");
Assert(
  system.TryRecordAppliedDamage(singleType, Player("player-a"), 6, 0),
  "A repeated contributor should accumulate without changing order.");

NpcDamageTrackerSnapshot first = Only(
  system.GetActiveSnapshots(),
  "The first hit should create exactly one active tracker.");
AssertEqual(3, first.Credits.Length, "Credit entries should be unique contributors.");
AssertEqual(
  Player("player-a"),
  first.Credits.Span[0].Contributor,
  "Credit order should preserve first appearance.");
AssertEqual(
  Player("player-b"),
  first.Credits.Span[1].Contributor,
  "The second player should retain second position.");
AssertEqual(
  World(),
  first.Credits.Span[2].Contributor,
  "World credit should retain its first appearance position.");
AssertEqual(10, first.Credits.Span[0].AppliedDamage, "Player damage should accumulate.");
AssertEqual(3, first.Credits.Span[1].AppliedDamage, "Second player damage should be exact.");
AssertEqual(2, first.Credits.Span[2].AppliedDamage, "World credit should be exact.");
AssertEqual(2, first.WorldDamage, "WorldDamage should equal accepted world damage.");
AssertEqual(
  Player("player-a"),
  first.LastContributor,
  "The last accepted contributor should be projected.");
Assert(!first.IsEmpty, "A positive credit makes the encounter non-empty.");
AssertEqual(0L, first.Duration, "Same-tick damage should have zero duration.");
AssertEqual(0L, first.TimeSinceLastHit, "Same-tick damage should have zero idle time.");

var externalCredits = first.Credits.ToArray();
externalCredits[0] = new DamageCreditEntry(Player("attacker"), 999);
NpcDamageTrackerSnapshot afterExternalMutation = Only(
  system.GetActiveSnapshots(),
  "Snapshot projection should remain available after caller mutation.");
AssertEqual(
  Player("player-a"),
  afterExternalMutation.Credits.Span[0].Contributor,
  "Mutating a copied credit array must not alter system state.");
AssertEqual(
  10,
  afterExternalMutation.Credits.Span[0].AppliedDamage,
  "Mutating a copied credit amount must not alter system state.");
var exposedSnapshots = system.GetActiveSnapshots();
Array.Clear(exposedSnapshots, 0, exposedSnapshots.Length);
AssertEqual(
  1,
  system.GetActiveSnapshots().Length,
  "Mutating the returned snapshot array must not alter registry state.");

Assert(
  !system.TryRecordAppliedDamage(singleType, Player("ignored"), 0, 0),
  "Zero applied damage should be rejected without state mutation.");
AssertEqual(1, system.ActiveTrackerCount, "Rejected zero damage must not create a tracker.");
AssertThrows<ArgumentException>(
  () => system.TryRecordAppliedDamage(
    singleType,
    new CombatContributorId(CombatContributorKind.Player, null),
    1,
    0),
  "Player credit without account provenance must be rejected.");
AssertThrows<ArgumentException>(
  () => system.TryRecordAppliedDamage(
    singleType,
    new CombatContributorId(CombatContributorKind.World, "not-world"),
    1,
    0),
  "World credit must not carry player account provenance.");

var failedCreationSystem = CreateSystem();
failedCreationSystem.AdvanceTo(0, Active(10));
AssertThrows<ArgumentException>(
  () => failedCreationSystem.TryRecordAppliedDamage(
    singleType,
    new CombatContributorId(CombatContributorKind.Player, null),
    1,
    0),
  "A failed first credit must report the invalid provenance.");
AssertEqual(
  0,
  failedCreationSystem.ActiveTrackerCount,
  "A failed first credit must not leave a ghost active tracker.");
Assert(
  failedCreationSystem.TryRecordAppliedDamage(singleType, Player("after-failure"), 1, 0),
  "A valid credit should remain possible after a failed first credit.");
AssertEqual(
  1UL,
  Only(
    failedCreationSystem.GetActiveSnapshots(),
    "The first successful credit should be the active tracker.").EncounterId,
  "A failed first credit must not consume encounter identity.");

system.AdvanceTo(12, Active(10));
NpcDamageTrackerSnapshot progressed = Only(
  system.GetActiveSnapshots(),
  "An active tracker should survive while its type remains active.");
AssertEqual(12L, progressed.TimeSinceLastHit, "Time should advance from the last hit.");
Assert(
  system.TryRecordAppliedDamage(singleType, Player("player-b"), 1, 12),
  "Damage at the current tick should be accepted.");
NpcDamageTrackerSnapshot laterHit = Only(
  system.GetActiveSnapshots(),
  "A later hit should remain in the same encounter.");
AssertEqual(12L, laterHit.Duration, "Duration should span the encounter start to last hit.");
AssertEqual(0L, laterHit.TimeSinceLastHit, "A current-tick hit resets idle time.");
system.AdvanceTo(13, Active(10));
AssertEqual(
  1L,
  Only(system.GetActiveSnapshots(), "The tracker should remain active.").TimeSinceLastHit,
  "Idle time should advance after the later hit.");

ulong firstEncounterId = first.EncounterId;
Assert(
  system.StopTracking(firstEncounterId, 13),
  "An active non-empty tracker should move to recent history when stopped.");
Assert(
  !system.StopTracking(firstEncounterId, 13),
  "Stopping the same encounter twice should be idempotent.");
AssertEqual(0, system.ActiveTrackerCount, "Stopped tracker should leave active registry.");
AssertEqual(1, system.RecentTrackerCount, "Stopped non-empty tracker should be recent.");
NpcDamageTrackerSnapshot recent = Only(
  system.GetRecentSnapshots(),
  "The stopped tracker should be visible in recent snapshots.");
Assert(recent.IsRecent && !recent.IsActive, "Recent lifecycle flags should be explicit.");
AssertEqual(EncounterCreditLifecycle.Closed, recent.Lifecycle, "Recent tracker should be closed.");

var emptySystem = CreateSystem();
emptySystem.AdvanceTo(0, Active(10));
NpcDamageEncounterTracker emptyTracker = emptySystem.StartTracking(
  singleType,
  new NpcDamageSingleTypeStrategy(singleType));
Assert(emptySystem.StopTracking(emptyTracker.EncounterId, 0), "Empty tracker should stop.");
AssertEqual(0, emptySystem.RecentTrackerCount, "Empty tracker should not enter recent history.");

var compositeSystem = CreateSystem();
var compositeHead = new NpcTypeId(20);
var compositeBody = new NpcTypeId(21);
compositeSystem.AdvanceTo(0, Active(20, 21));
Assert(
  compositeSystem.TryRecordAppliedDamage(compositeHead, Player("composite"), 5, 0),
  "Composite head damage should start one encounter.");
Assert(
  compositeSystem.TryRecordAppliedDamage(compositeBody, World(), 2, 0),
  "Composite member damage should reuse the same encounter.");
Assert(
  compositeSystem.MarkKilled(compositeHead, 0),
  "Killing a configured composite member should set killed state.");
NpcDamageTrackerSnapshot composite = Only(
  compositeSystem.GetActiveSnapshots(),
  "Composite members should project as one active tracker.");
Assert(composite.IsKilled, "Composite killed state should be visible in snapshots.");
compositeSystem.AdvanceTo(1, Active(21));
AssertEqual(
  1,
  compositeSystem.ActiveTrackerCount,
  "A composite tracker remains active while another member is active.");
Assert(
  !compositeSystem.MarkKilled(new NpcTypeId(99), 1),
  "An unrelated type should not mark a composite tracker killed.");
compositeSystem.AdvanceTo(2, Active());
AssertEqual(1, compositeSystem.RecentTrackerCount, "Composite tracker should close when all members end.");
Assert(
  Only(compositeSystem.GetRecentSnapshots(), "Composite recent snapshot expected.").IsKilled,
  "Killed state should survive active-to-recent transfer.");

var capSystem = CreateSystem();
var capIds = new List<ulong>();
for (int index = 0; index < 4; index++)
{
  capSystem.AdvanceTo(0, Active(10));
  Assert(
    capSystem.TryRecordAppliedDamage(singleType, Player($"cap-{index}"), 1, 0),
    "Each cap fixture should create an encounter.");
  ulong encounterId = Only(capSystem.GetActiveSnapshots(), "Cap encounter expected.").EncounterId;
  capIds.Add(encounterId);
  Assert(capSystem.StopTracking(encounterId, 0), "Each cap fixture should stop.");
}
AssertEqual(3, capSystem.RecentTrackerCount, "Recent history should be capped at three.");
var cappedIds = capSystem.GetRecentSnapshots();
AssertEqual(capIds[1], cappedIds[0].EncounterId, "The oldest recent tracker should be dropped first.");
AssertEqual(capIds[3], cappedIds[2].EncounterId, "Recent order should remain oldest to newest.");

var expirySystem = CreateSystem();
var expiryIds = new List<ulong>();
for (int index = 0; index < 2; index++)
{
  expirySystem.AdvanceTo(0, Active(10));
  Assert(
    expirySystem.TryRecordAppliedDamage(singleType, Player($"expiry-{index}"), 1, 0),
    "Each expiry fixture should create an encounter.");
  ulong encounterId = Only(expirySystem.GetActiveSnapshots(), "Expiry encounter expected.").EncounterId;
  expiryIds.Add(encounterId);
  Assert(expirySystem.StopTracking(encounterId, 0), "Each expiry fixture should stop.");
}
expirySystem.AdvanceTo(54000, Active());
AssertEqual(
  2,
  expirySystem.RecentTrackerCount,
  "The exact expiry threshold must retain more than one recent tracker.");
expirySystem.AdvanceTo(54001, Active());
AssertEqual(1, expirySystem.RecentTrackerCount, "Expiry must use a strict greater-than comparison.");
AssertEqual(
  expiryIds[1],
  Only(expirySystem.GetRecentSnapshots(), "One recent tracker must remain.").EncounterId,
  "Expiry should remove the oldest tracker first.");

expirySystem.Reset();
AssertEqual(0, expirySystem.ActiveTrackerCount, "Reset should clear active registry.");
AssertEqual(0, expirySystem.RecentTrackerCount, "Reset should clear recent registry.");
expirySystem.AdvanceTo(54001, Active(10));
Assert(
  expirySystem.TryRecordAppliedDamage(singleType, Player("after-reset"), 1, 54001),
  "The system should accept a new encounter after reset.");
AssertEqual(
  1UL,
  Only(expirySystem.GetActiveSnapshots(), "Reset should restart encounter identity allocation.").EncounterId,
  "Reset should restart world-local encounter identity allocation.");

var clockSystem = CreateSystem();
clockSystem.AdvanceTo(10, Active(10));
AssertThrows<ArgumentOutOfRangeException>(
  () => clockSystem.AdvanceTo(9, Active(10)),
  "A backwards clock value must be rejected.");
AssertThrows<InvalidOperationException>(
  () => clockSystem.TryRecordAppliedDamage(singleType, Player("future"), 1, 11),
  "Damage at a future uncommitted tick must be rejected.");

var firstWorld = CreateSystem();
var secondWorld = CreateSystem();
firstWorld.AdvanceTo(0, Active(10));
secondWorld.AdvanceTo(0, Active(10));
Assert(
  firstWorld.TryRecordAppliedDamage(singleType, Player("world-a"), 1, 0),
  "The first world should accept its own damage.");
AssertEqual(0, secondWorld.ActiveTrackerCount, "World instances must not share active registry state.");
Assert(
  secondWorld.TryRecordAppliedDamage(singleType, Player("world-b"), 1, 0),
  "The second world should independently accept its own damage.");
AssertEqual(
  1UL,
  Only(secondWorld.GetActiveSnapshots(), "The second world should have one tracker.").EncounterId,
  "Encounter IDs should be local to a world/session instance.");
firstWorld.Reset();
AssertEqual(1, secondWorld.ActiveTrackerCount, "Reset must not affect another world instance.");

Console.WriteLine("PASS: npc damage tracking system experiment");
