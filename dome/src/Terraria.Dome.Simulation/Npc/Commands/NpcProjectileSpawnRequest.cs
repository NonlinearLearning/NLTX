using Terraria.Dome.Simulation.Projectile.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Commands;

public readonly record struct NpcProjectileSpawnRequest(
  NpcHandle SourceNpc,
  long Sequence,
  ProjectileDefinition Definition,
  int Damage,
  SimulationVector Position,
  SimulationVector Velocity,
  ushort BannerIdToRespondTo = 0);
