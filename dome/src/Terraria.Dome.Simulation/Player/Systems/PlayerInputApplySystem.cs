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

      if (supplied.Facing < -1 || supplied.Facing > 1)
      {
        throw new ArgumentException(
          "Input facing must be -1, 0 or 1.",
          nameof(inputBatch));
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
      ref ControlInputComponent input = ref world.Get<ControlInputComponent>(entity);
      bool previousUseItem = input.UseItem;
      bool previousUseTile = input.UseTile;
      bool previousDash = input.Dash;
      input = new ControlInputComponent();
      input.UseItemJustReleased = previousUseItem;
      input.UseTileJustReleased = previousUseTile;
      input.DashJustReleased = previousDash;
    }

    for (int index = 0; index < inputBatch.Inputs.Count; index++)
    {
      PlayerInput supplied = inputBatch.Inputs[index];
      Entity entity = players[supplied.Player];
      ref ControlInputComponent input = ref world.Get<ControlInputComponent>(entity);
      bool previousDash = input.DashJustReleased;
      bool previousUseTile = input.UseTileJustReleased;
      bool previousUseItem = input.UseItemJustReleased;
      input.Down = supplied.Down;
      input.Dash = supplied.Dash;
      input.DashJustPressed = supplied.Dash && !previousDash;
      input.DashJustReleased = !supplied.Dash && previousDash;
      input.MoveLeft = supplied.MoveLeft;
      input.MoveRight = supplied.MoveRight;
      input.Jump = supplied.Jump;
      input.Fire = supplied.Fire;
      input.UseItem = supplied.UseItem;
      input.Up = supplied.Up;
      input.UseTile = supplied.UseTile;
      input.UseTileJustPressed = supplied.UseTile && !previousUseTile;
      input.UseTileJustReleased = !supplied.UseTile && previousUseTile;
      input.UseItemJustPressed = supplied.UseItem && !previousUseItem;
      input.UseItemJustReleased = !supplied.UseItem && previousUseItem;
      input.Facing = supplied.Facing;
    }
  }
}
