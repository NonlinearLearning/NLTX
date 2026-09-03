using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Events;

namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcLootEmissionLedger
{
  private readonly Dictionary<NpcHandle, long> _lastEmittedTicks = new();

  public bool TryEmit(
    NpcLootSystem lootSystem,
    NpcDeathEvent death,
    out NpcLootCommand command)
  {
    ArgumentNullException.ThrowIfNull(lootSystem);
    command = default;
    if (!death.Npc.IsValid || death.Tick < 0 ||
        (_lastEmittedTicks.TryGetValue(death.Npc, out long lastTick) &&
         lastTick == death.Tick))
    {
      return false;
    }

    bool hadPreviousTick = _lastEmittedTicks.TryGetValue(death.Npc, out long previousTick);
    _lastEmittedTicks[death.Npc] = death.Tick;

    try
    {
      command = lootSystem.CreateDrop(death);
      if (!command.IsConsistent)
      {
        throw new InvalidOperationException(
          "NPC loot command does not preserve its source identity.");
      }

      return true;
    }
    catch
    {
      if (hadPreviousTick)
      {
        _lastEmittedTicks[death.Npc] = previousTick;
      }
      else
      {
        _lastEmittedTicks.Remove(death.Npc);
      }
      throw;
    }
  }
}
