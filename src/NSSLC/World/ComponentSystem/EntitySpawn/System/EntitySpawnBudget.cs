namespace Terraria.EntitySpawn;

/// <summary>Result of the Version4 <c>Spawner.GetSpawnRate</c> roll: divisor and nearby cap.</summary>
/// <remarks>
/// Every tick the spawner asks "may I roll?" with <c>Random.Next(SpawnRate) == 0</c> and caps the
/// nearby population at <see cref="MaximumSpawns"/>. Both numbers are per player, so they are
/// recomputed from that player's depth and the world flags.
/// </remarks>
public readonly record struct EntitySpawnBudget(int SpawnRate, int MaximumSpawns)
{
  public bool IsValid => SpawnRate > 0 && MaximumSpawns > 0;
}
