using System.Numerics;
using Terraria.Player.Environment;
using Terraria.Player.Luck;

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

var luckState = new PlayerLuckAndRescanStateComponent();
var environment = new PlayerEnvironmentDetectionAndSpawnStateComponent();
var port = new RecordingPlayerWallRescanPort(scanResult: true);
port.OnBroadcast = () =>
{
  Assert(environment.InsideUnbreakableWalls,
    "Broadcast should observe the committed inside state.");
  AssertEqual(20, luckState.UnbreakableWallScanCooldown,
    "Broadcast should observe the reset cooldown.");
  AssertEqual(Vector2.Zero, luckState.UnbreakableWallScanLastPosition,
    "Broadcast should observe the committed scan position.");
};
var forced = PlayerWallRescanSystem.Execute(
  new PlayerWallRescanInput(true, true, Vector2.Zero),
  luckState,
  environment,
  port);

Assert(forced.DidScan, "Force should run the wall scan.");
Assert(forced.Changed, "A changed inside state should be reported.");
AssertEqual(20, luckState.UnbreakableWallScanCooldown,
  "A completed scan should reset the cooldown.");
AssertEqual(1, port.ScanCount, "Force should run one scan.");
AssertEqual(1, port.BroadcastCount,
  "A changed inside state should broadcast once.");
AssertEqual("scan,broadcast", string.Join(",", port.Calls),
  "Broadcast should follow the scan and state commit.");

var skipped = PlayerWallRescanSystem.Execute(
  new PlayerWallRescanInput(true, false, Vector2.Zero),
  luckState,
  environment,
  port);
Assert(!skipped.DidScan, "A positive cooldown should defer a nearby scan.");
AssertEqual(19, luckState.UnbreakableWallScanCooldown,
  "A deferred scan should decrement the cooldown.");
AssertEqual(1, port.ScanCount,
  "A deferred scan should not read the world scan port.");

var seedDisabled = PlayerWallRescanSystem.Execute(
  new PlayerWallRescanInput(false, true, new Vector2(30f, 40f)),
  luckState,
  environment,
  port);
Assert(!seedDisabled.DidScan, "A non-dual seed should skip wall scanning.");
AssertEqual(19, luckState.UnbreakableWallScanCooldown,
  "The seed gate should preserve the cooldown.");
AssertEqual(Vector2.Zero, luckState.UnbreakableWallScanLastPosition,
  "The seed gate should preserve the last scan position.");

var boundaryPort = new RecordingPlayerWallRescanPort(scanResult: false);
var boundaryState = new PlayerLuckAndRescanStateComponent();
var boundaryEnvironment = new PlayerEnvironmentDetectionAndSpawnStateComponent();
PlayerWallRescanSystem.Execute(
  new PlayerWallRescanInput(true, true, Vector2.Zero),
  boundaryState,
  boundaryEnvironment,
  boundaryPort);
var distanceBoundary = PlayerWallRescanSystem.Execute(
  new PlayerWallRescanInput(true, false, new Vector2(256f, 0f)),
  boundaryState,
  boundaryEnvironment,
  boundaryPort);
Assert(distanceBoundary.DidScan,
  "Distance equal to the rescan threshold should trigger a scan.");
AssertEqual(2, boundaryPort.ScanCount,
  "The distance boundary should run a second scan despite cooldown.");
AssertEqual(0, boundaryPort.BroadcastCount,
  "An unchanged inside state should not broadcast.");

var expiredPort = new RecordingPlayerWallRescanPort(scanResult: false);
var expiredState = new PlayerLuckAndRescanStateComponent();
var expiredEnvironment = new PlayerEnvironmentDetectionAndSpawnStateComponent();
PlayerWallRescanSystem.Execute(
  new PlayerWallRescanInput(true, true, Vector2.Zero),
  expiredState,
  expiredEnvironment,
  expiredPort);
for (int i = 0; i < 19; i++)
{
  PlayerWallRescanSystem.Execute(
    new PlayerWallRescanInput(true, false, Vector2.Zero),
    expiredState,
    expiredEnvironment,
    expiredPort);
}

AssertEqual(1, expiredState.UnbreakableWallScanCooldown,
  "The scan should remain deferred until the cooldown reaches zero.");
var cooldownBoundary = PlayerWallRescanSystem.Execute(
  new PlayerWallRescanInput(true, false, Vector2.Zero),
  expiredState,
  expiredEnvironment,
  expiredPort);
Assert(cooldownBoundary.DidScan,
  "The scan should run when cooldown decrements to zero.");
AssertEqual(2, expiredPort.ScanCount,
  "Cooldown expiry should run one additional scan.");

Console.WriteLine(
  "PASS: player wall rescan seed, force, cooldown, distance and change effects");

sealed class RecordingPlayerWallRescanPort(bool scanResult) : IPlayerWallRescanPort
{
  public List<string> Calls { get; } = [];

  public Action? OnBroadcast { get; set; }

  public int ScanCount { get; private set; }

  public int BroadcastCount { get; private set; }

  public bool ScanInsideUnbreakableWalls(Vector2 playerCenter)
  {
    Calls.Add("scan");
    ScanCount++;
    return scanResult;
  }

  public void BroadcastChange()
  {
    Calls.Add("broadcast");
    BroadcastCount++;
    OnBroadcast?.Invoke();
  }
}
