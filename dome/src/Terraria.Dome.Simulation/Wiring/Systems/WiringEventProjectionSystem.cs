using System.Collections.Generic;
using Terraria.Dome.Simulation.Wiring.Components;
using Terraria.Dome.Simulation.Wiring.Snapshots;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class WiringEventProjectionSystem
{
  public IReadOnlyList<MechanismSnapshot> CreateSnapshots(
    IReadOnlyDictionary<int, MechanismComponent> mechanisms)
  {
    List<MechanismSnapshot> snapshots = new(mechanisms.Count);
    foreach (KeyValuePair<int, MechanismComponent> entry in mechanisms)
    {
      MechanismComponent mechanism = entry.Value;
      snapshots.Add(new MechanismSnapshot(
        mechanism.MechanismId,
        mechanism.Type,
        mechanism.IsActive,
        mechanism.LastActivationSequence));
    }

    snapshots.Sort(static (first, second) => first.MechanismId.CompareTo(second.MechanismId));
    return snapshots;
  }
}
