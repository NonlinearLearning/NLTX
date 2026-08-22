using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcLifecycleResult(
  bool BecameInactive,
  bool IsDead,
  DespawnNpcCommand? DespawnCommand);

public sealed class NpcLifecycleSystem
{
  public NpcLifecycleResult Advance(
    ref NpcLifecycleComponent lifecycle,
    int currentHealth,
    bool isImmortal = false)
  {
    if (!lifecycle.IsActive)
    {
      return new(false, currentHealth <= 0, null);
    }

    if (isImmortal)
    {
      return new(false, false, null);
    }

    if (currentHealth <= 0)
    {
      lifecycle.IsActive = false;
      lifecycle.DespawnReason = NpcDespawnReason.Killed;
      return new(true, true, null);
    }

    if (lifecycle.TimeLeft > 0)
    {
      lifecycle.TimeLeft--;
    }

    if (lifecycle.TimeLeft <= 0)
    {
      lifecycle.IsActive = false;
      lifecycle.DespawnReason = NpcDespawnReason.TimedOut;
      return new(true, false, null);
    }

    return new(false, false, null);
  }
}
