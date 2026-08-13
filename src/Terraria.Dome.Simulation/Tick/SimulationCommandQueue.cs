using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;

namespace Terraria.Dome.Simulation.Tick;

internal sealed class SimulationCommandQueue
{
  private readonly List<DamageCommand> _damageCommands = new();
  private readonly List<DespawnEntityCommand> _despawnEntityCommands = new();
  private readonly List<SpawnProjectileCommand> _spawnProjectileCommands = new();

  public IReadOnlyList<DamageCommand> DamageCommands => _damageCommands;
  public IReadOnlyList<DespawnEntityCommand> DespawnEntityCommands => _despawnEntityCommands;
  public IReadOnlyList<SpawnProjectileCommand> SpawnProjectileCommands => _spawnProjectileCommands;

  public void Clear()
  {
    _damageCommands.Clear();
    _despawnEntityCommands.Clear();
    _spawnProjectileCommands.Clear();
  }

  public void Enqueue(DamageCommand command)
  {
    _damageCommands.Add(command);
  }

  public void Enqueue(DespawnEntityCommand command)
  {
    _despawnEntityCommands.Add(command);
  }

  public void Enqueue(SpawnProjectileCommand command)
  {
    _spawnProjectileCommands.Add(command);
  }
}
