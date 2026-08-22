using System.Collections.Generic;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Replication;

public readonly record struct CombatReplicationBatch(
  IReadOnlyList<byte[]> Frames,
  IReadOnlyList<NpcReplicationSnapshot> Npcs,
  IReadOnlyList<ProjectileReplicationSnapshot> Projectiles);
