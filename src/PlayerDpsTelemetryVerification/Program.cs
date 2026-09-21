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

DateTimeOffset start = new(2026, 9, 12, 0, 0, 0, TimeSpan.Zero);
var clock = new VerificationClock(start.AddSeconds(10));
var component = new PlayerDpsTelemetryComponent();
var system = new PlayerDpsTelemetrySystem(component, clock);
var query = new PlayerDpsTelemetryQuery();

Guid firstEventId = Guid.Parse("10000000-0000-0000-0000-000000000001");
Guid secondEventId = Guid.Parse("10000000-0000-0000-0000-000000000002");

var firstEvent = new PlayerCommittedDamageEvent(
  firstEventId,
  Damage: 30,
  start,
  SourceRevision: 1);
Assert(system.AcceptCommittedDamage(firstEvent), "The first committed damage should start a window.");
Assert(component.DpsStarted, "The first committed damage should activate the window.");
AssertEqual(30, component.DpsDamage, "The first committed damage should be recorded.");
AssertEqual(start, component.DpsStart, "The window should start at the committed event time.");
AssertEqual(start, component.DpsEnd, "The end should equal the first committed event time.");
AssertEqual(start, component.DpsLastHit, "The last hit should equal the first committed event time.");

var componentBeforeQuery = (
  component.DpsStart,
  component.DpsEnd,
  component.DpsLastHit,
  component.DpsDamage,
  component.DpsStarted);
PlayerDpsTelemetrySnapshot earlySnapshot = query.Snapshot(component, start.AddSeconds(-1));
Assert(earlySnapshot.IsActive, "An early observation should still see the active window.");
AssertEqual(TimeSpan.Zero, earlySnapshot.Duration, "An observation before the start should have zero duration.");
AssertEqual(0d, earlySnapshot.DamagePerSecond, "An observation before the start should have zero DPS.");

PlayerDpsTelemetrySnapshot firstSnapshot = query.Snapshot(component, clock.UtcNow);
Assert(firstSnapshot.IsActive, "An open window should be active in its snapshot.");
AssertEqual(TimeSpan.FromSeconds(10), firstSnapshot.Duration, "The active duration should use the observation time.");
Assert(Math.Abs(firstSnapshot.DamagePerSecond - 3d) < 0.0001d, "The active DPS should use committed damage and duration.");
var componentAfterQuery = (
  component.DpsStart,
  component.DpsEnd,
  component.DpsLastHit,
  component.DpsDamage,
  component.DpsStarted);
AssertEqual(componentBeforeQuery, componentAfterQuery, "Query must not mutate the telemetry component.");
AssertEqual(0, clock.ReadCount, "Accept and query should not read the system clock.");

Assert(!system.AcceptCommittedDamage(firstEvent), "A duplicate event ID must be ignored.");
AssertEqual(30, component.DpsDamage, "A duplicate event must not increase damage.");

var secondEvent = new PlayerCommittedDamageEvent(
  secondEventId,
  Damage: 20,
  start.AddSeconds(4),
  SourceRevision: 2);
Assert(system.AcceptCommittedDamage(secondEvent), "A new committed event should accumulate.");
AssertEqual(50, component.DpsDamage, "Committed damage should accumulate.");
AssertEqual(start.AddSeconds(4), component.DpsLastHit, "The latest committed timestamp should be retained.");

var zeroDamageEvent = new PlayerCommittedDamageEvent(
  Guid.Parse("10000000-0000-0000-0000-000000000003"),
  Damage: 0,
  start.AddSeconds(5),
  SourceRevision: 3);
var negativeDamageEvent = zeroDamageEvent with { Damage = -1, EventId = Guid.Parse("10000000-0000-0000-0000-000000000004") };
var missingEventId = zeroDamageEvent with { EventId = Guid.Empty, Damage = 10 };
Assert(!system.AcceptCommittedDamage(zeroDamageEvent), "Zero damage must not enter telemetry.");
Assert(!system.AcceptCommittedDamage(negativeDamageEvent), "Negative damage must not enter telemetry.");
Assert(!system.AcceptCommittedDamage(missingEventId), "An event without a stable ID must not enter telemetry.");
AssertEqual(50, component.DpsDamage, "Invalid events must not mutate telemetry.");

clock.UtcNow = start.AddSeconds(12);
Assert(system.Stop(), "Stopping an active window should close it.");
AssertEqual(1, clock.ReadCount, "Stopping should read the injected clock once.");
PlayerDpsTelemetrySnapshot stoppedSnapshot = query.Snapshot(component, start.AddSeconds(1));
Assert(!stoppedSnapshot.IsActive, "A stopped window should not be active.");
AssertEqual(TimeSpan.FromSeconds(12), stoppedSnapshot.Duration, "A stopped window should retain its close time.");
AssertEqual(start.AddSeconds(12), stoppedSnapshot.EndedAt!.Value, "The stop time should be the window end.");
Assert(!system.AcceptCommittedDamage(firstEvent), "An event ID must remain deduplicated after stopping.");

system.Reset();
PlayerDpsTelemetrySnapshot resetSnapshot = query.Snapshot(component, clock.UtcNow);
Assert(!resetSnapshot.HasWindow, "Reset should remove the telemetry window.");
AssertEqual(0, resetSnapshot.Damage, "Reset should clear accumulated damage.");
Assert(!component.DpsStarted, "Reset should leave the component inactive.");
Assert(system.AcceptCommittedDamage(firstEvent), "Reset should clear the event replay set.");
Assert(!system.ReconcileCapability(true), "An available capability should not stop an inactive window.");
Assert(system.ReconcileCapability(false), "Removing the capability should stop an active window.");
Assert(!component.DpsStarted, "Capability removal should close the active window.");

var saturatedComponent = new PlayerDpsTelemetryComponent();
var saturatedSystem = new PlayerDpsTelemetrySystem(saturatedComponent, clock);
Assert(
  saturatedSystem.AcceptCommittedDamage(
    new PlayerCommittedDamageEvent(
      Guid.Parse("10000000-0000-0000-0000-000000000005"),
      int.MaxValue,
      start,
      5)),
  "The maximum valid damage event should be accepted.");
Assert(
  saturatedSystem.AcceptCommittedDamage(
    new PlayerCommittedDamageEvent(
      Guid.Parse("10000000-0000-0000-0000-000000000006"),
      1,
      start.AddSeconds(1),
      6)),
  "Damage after the maximum should still be accepted.");
AssertEqual(int.MaxValue, saturatedComponent.DpsDamage, "Damage accumulation should saturate at Int32.MaxValue.");

Console.WriteLine("PASS: player DPS telemetry committed-event lifecycle and pure query");

sealed class VerificationClock : IPlayerTelemetryClock
{
  public VerificationClock(DateTimeOffset utcNow)
  {
    UtcNow = utcNow;
  }

  public DateTimeOffset UtcNow { get; set; }

  public int ReadCount { get; private set; }

  DateTimeOffset IPlayerTelemetryClock.UtcNow
  {
    get
    {
      ReadCount++;
      return UtcNow;
    }
  }
}
