using System.Collections.Generic;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.StatusEffects.Snapshots;

namespace Terraria.Dome.Server.Replication;

public readonly record struct CombatReplicationBatch(
  IReadOnlyList<byte[]> Frames,
  IReadOnlyList<NpcReplicationSnapshot> Npcs,
  IReadOnlyList<ProjectileReplicationSnapshot> Projectiles)
{
  public IReadOnlyList<NpcProjectileReplicationSnapshot> NpcProjectiles { get; init; } = [];

  public IReadOnlyList<NpcStatusEffectStateSnapshot> NpcStatusEffects { get; init; } = [];
}
