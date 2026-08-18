using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Wiring.Components;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class MechanismActivationSystem
{
  public IReadOnlyList<MechanismActivationCommand> Apply(
    IReadOnlyDictionary<int, MechanismComponent> mechanisms,
    IReadOnlyCollection<MechanismActivationCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(mechanisms);
    ArgumentNullException.ThrowIfNull(commands);
    List<MechanismActivationCommand> accepted = new();
    HashSet<(int MechanismId, long Sequence)> seen = new();
    foreach (MechanismActivationCommand command in commands)
    {
      if (!seen.Add((command.MechanismId, command.Sequence)) ||
          !mechanisms.TryGetValue(command.MechanismId, out MechanismComponent? mechanism) ||
          !mechanism.TryActivate(command.Sequence))
      {
        continue;
      }

      accepted.Add(command);
    }

    accepted.Sort(static (first, second) =>
    {
      int sequence = first.Sequence.CompareTo(second.Sequence);
      return sequence != 0 ? sequence : first.MechanismId.CompareTo(second.MechanismId);
    });
    return accepted;
  }
}
