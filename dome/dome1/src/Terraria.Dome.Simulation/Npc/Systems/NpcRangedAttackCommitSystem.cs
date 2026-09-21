using System;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcRangedAttackCommitSystem
{
  public bool TryCommit(
    NpcRangedAttackCommand command,
    ProjectileDefinitionRegistry projectileDefinitions,
    out NpcProjectileSpawnRequest request)
  {
    request = default;
    ArgumentNullException.ThrowIfNull(projectileDefinitions);
    if (!command.SourceNpc.IsValid || command.Sequence < 0 || command.Damage <= 0 ||
        !IsFinite(command.Position) || !IsFinite(command.Velocity) ||
        !projectileDefinitions.TryGet(command.ProjectileType, out ProjectileDefinition definition) ||
        !definition.Hostile || definition.Friendly || definition.MaximumPenetration == 0 ||
        definition.PlayerDamagePolicy != PlayerDamagePolicy.HostileNonPvp)
    {
      return false;
    }

    request = new NpcProjectileSpawnRequest(
      command.SourceNpc,
      command.Sequence,
      definition,
      command.Damage,
      command.Position,
      command.Velocity);
    return true;
  }

  private static bool IsFinite(SimulationVector value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }
}
