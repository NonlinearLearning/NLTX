using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

internal sealed class PlayerInputApplySystem
{
  public void Apply(
    World world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    SimulationInputBatch inputBatch)
  {
    foreach (Entity entity in players.Values)
    {
      ref PlayerInputComponent input = ref world.Get<PlayerInputComponent>(entity);
      input = new PlayerInputComponent();
    }

    for (int index = 0; index < inputBatch.Inputs.Count; index++)
    {
      PlayerInput supplied = inputBatch.Inputs[index];
      if (!players.TryGetValue(supplied.Player, out Entity entity))
      {
        throw new ArgumentException("Input references an unknown player.", nameof(inputBatch));
      }

      ref PlayerInputComponent input = ref world.Get<PlayerInputComponent>(entity);
      input.MoveLeft = supplied.MoveLeft;
      input.MoveRight = supplied.MoveRight;
      input.Jump = supplied.Jump;
      input.Fire = supplied.Fire;
    }
  }
}
