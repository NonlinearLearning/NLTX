using System;

namespace Terraria.Player.Environment;

public static class PlayerShimmerTransitionSystem
{
  public static bool UpdateLocalTransition(
    in PlayerShimmerTransitionInput input,
    PlayerZoneAndEnvironmentStateComponent environment,
    IPlayerFaelingSpawnPort spawnPort)
  {
    ArgumentNullException.ThrowIfNull(environment);
    ArgumentNullException.ThrowIfNull(spawnPort);

    bool shouldSpawn = !environment.WasInShimmerZone && input.ZoneShimmer;
    if (shouldSpawn)
    {
      spawnPort.SpawnFaelings();
    }

    environment.WasInShimmerZone = input.ZoneShimmer;
    return shouldSpawn;
  }
}
