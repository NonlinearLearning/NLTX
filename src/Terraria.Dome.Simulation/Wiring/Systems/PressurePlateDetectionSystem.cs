using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Wiring.Components;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public readonly record struct WiringActorSnapshot(
  SimulationVector Position,
  bool IsPlayer);

public sealed class PressurePlateDetectionSystem
{
  public IReadOnlyList<MechanismActivationCommand> Detect(
    IReadOnlyList<PressurePlateComponent> plates,
    IReadOnlyList<WiringActorSnapshot> actors,
    long firstSequence)
  {
    ArgumentNullException.ThrowIfNull(plates);
    ArgumentNullException.ThrowIfNull(actors);
    if (firstSequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(firstSequence));
    }

    List<int> mechanismIds = new();
    for (int plateIndex = 0; plateIndex < plates.Count; plateIndex++)
    {
      PressurePlateComponent plate = plates[plateIndex];
      int radiusSquared = plate.Radius * plate.Radius;
      for (int actorIndex = 0; actorIndex < actors.Count; actorIndex++)
      {
        WiringActorSnapshot actor = actors[actorIndex];
        if (plate.RequiresPlayer && !actor.IsPlayer)
        {
          continue;
        }

        float deltaX = actor.Position.X - plate.X;
        float deltaY = actor.Position.Y - plate.Y;
        if (deltaX * deltaX + deltaY * deltaY <= radiusSquared)
        {
          mechanismIds.Add(plate.MechanismId);
          break;
        }
      }
    }

    WiringSequenceAllocator sequenceAllocator = new(firstSequence);
    if (!sequenceAllocator.TryReserve(mechanismIds.Count, out long sequence))
    {
      return [];
    }

    List<MechanismActivationCommand> activations = new(mechanismIds.Count);
    for (int index = 0; index < mechanismIds.Count; index++)
    {
      activations.Add(new MechanismActivationCommand(
        sequence + index,
        mechanismIds[index],
        MechanismActivationKind.Activate,
        mechanismIds[index]));
    }

    return activations;
  }
}
