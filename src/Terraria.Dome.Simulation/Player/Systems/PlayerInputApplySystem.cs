using System;
using System.Collections.Generic;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

internal sealed class PlayerInputApplySystem
{
  public void Apply(
    Arch.Core.World world,
    IReadOnlyDictionary<PlayerHandle, Entity> players,
    SimulationInputBatch inputBatch)
  {
    HashSet<PlayerHandle> seenPlayers = new();
    for (int index = 0; index < inputBatch.Inputs.Count; index++)
    {
      PlayerInput supplied = inputBatch.Inputs[index];
      if (!players.ContainsKey(supplied.Player))
      {
        throw new ArgumentException("Input references an unknown player.", nameof(inputBatch));
      }

      if (!seenPlayers.Add(supplied.Player))
      {
        throw new ArgumentException(
          "Input batch contains more than one input for the same player.",
          nameof(inputBatch));
      }
    }

    foreach (Entity entity in players.Values)
    {
      ref PlayerInputComponent input = ref world.Get<PlayerInputComponent>(entity);
      input = new PlayerInputComponent();
    }

    for (int index = 0; index < inputBatch.Inputs.Count; index++)
    {
      PlayerInput supplied = inputBatch.Inputs[index];
      Entity entity = players[supplied.Player];
      ref PlayerInputComponent input = ref world.Get<PlayerInputComponent>(entity);
      input.MoveLeft = supplied.MoveLeft;
      input.MoveRight = supplied.MoveRight;
      input.Jump = supplied.Jump;
      input.Fire = supplied.Fire;
      input.UseItem = supplied.UseItem;
      input.Facing = supplied.Facing;
    }
  }
}
