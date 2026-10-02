using Terraria.Player.Environment;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

var environment = new PlayerZoneAndEnvironmentStateComponent();
var spawnPort = new RecordingFaelingSpawnPort(() =>
  Assert(!environment.WasInShimmerZone,
    "The spawn effect must run before the local shimmer latch is committed."));

bool spawned = PlayerShimmerTransitionSystem.UpdateLocalTransition(
  new PlayerShimmerTransitionInput(true),
  environment,
  spawnPort);
Assert(spawned, "Entering shimmer should request Faeling spawning.");
Assert(environment.WasInShimmerZone,
  "The local shimmer latch should commit after the spawn effect returns.");
Assert(spawnPort.CallCount == 1,
  "Entering shimmer should request one spawn effect.");

spawned = PlayerShimmerTransitionSystem.UpdateLocalTransition(
  new PlayerShimmerTransitionInput(true),
  environment,
  spawnPort);
Assert(!spawned, "A repeated in-shimmer update should not request another spawn.");
Assert(spawnPort.CallCount == 1,
  "A repeated in-shimmer update should not call the spawn effect.");

spawned = PlayerShimmerTransitionSystem.UpdateLocalTransition(
  new PlayerShimmerTransitionInput(false),
  environment,
  spawnPort);
Assert(!spawned, "Leaving shimmer should not request spawning.");
Assert(!environment.WasInShimmerZone,
  "Leaving shimmer should clear the local transition latch.");

spawned = PlayerShimmerTransitionSystem.UpdateLocalTransition(
  new PlayerShimmerTransitionInput(true),
  environment,
  spawnPort);
Assert(spawned, "Re-entering shimmer after leaving should request spawning.");
Assert(spawnPort.CallCount == 2,
  "A new false-to-true transition should call the spawn effect once.");

var failedEnvironment = new PlayerZoneAndEnvironmentStateComponent();
var failingPort = new RecordingFaelingSpawnPort(() =>
  throw new InvalidOperationException("Spawn failed."));
try
{
  PlayerShimmerTransitionSystem.UpdateLocalTransition(
    new PlayerShimmerTransitionInput(true),
    failedEnvironment,
    failingPort);
  throw new InvalidOperationException("The failing spawn effect should propagate.");
}
catch (InvalidOperationException exception) when (exception.Message == "Spawn failed.")
{
  Assert(!failedEnvironment.WasInShimmerZone,
    "A failed spawn effect must leave the local shimmer latch uncommitted.");
}

var immunityState = new PlayerZoneAndEnvironmentStateComponent();
Assert(PlayerEnvironmentBuffImmunitySystem.BeginTeleportImmunity(immunityState) == 4,
  "Teleport should start the Version4 environment buff immunity window.");
Assert(immunityState.EnvironmentBuffImmunityTimer == 4,
  "The teleport immunity window should commit to the environment state.");
Assert(PlayerEnvironmentBuffImmunitySystem.Tick(immunityState) == 3,
  "Each player tick should decrement environment immunity once.");
for (int i = 0; i < 3; i++)
{
  PlayerEnvironmentBuffImmunitySystem.Tick(immunityState);
}

Assert(immunityState.EnvironmentBuffImmunityTimer == 0,
  "The teleport immunity timer should reach zero after four ticks.");
Assert(PlayerEnvironmentBuffImmunitySystem.Tick(immunityState) == 0,
  "The environment immunity timer should not become negative.");

Console.WriteLine("PASS: player shimmer edge, effect order, exception timing and immunity timer");

sealed class RecordingFaelingSpawnPort(Action onSpawn) : IPlayerFaelingSpawnPort
{
  public int CallCount { get; private set; }

  public void SpawnFaelings()
  {
    CallCount++;
    onSpawn();
  }
}
