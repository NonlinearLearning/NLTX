using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.WorldModel;
using PipelineLiquidChangeCommand = Terraria.Dome.Simulation.Commands.LiquidChangeCommand;

namespace Terraria.Dome.Simulation.Liquid.Systems;

public sealed class LiquidCommitSystem
{
  public bool TryCommit(
    WorldGrid world,
    IReadOnlyCollection<PipelineLiquidChangeCommand> commands,
    LiquidDirtySectionComponent dirtySections,
    out LiquidCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(commands);
    ArgumentNullException.ThrowIfNull(dirtySections);
    HashSet<long> sequences = new();
    foreach (PipelineLiquidChangeCommand command in commands)
    {
      if (command.Sequence < 0 || !sequences.Add(command.Sequence) ||
          command.Type > (byte)LiquidType.Shimmer || !world.Contains(command.X, command.Y))
      {
        result = LiquidCommitResult.Failed("Liquid command failed validation.");
        return false;
      }
    }

    Dictionary<WorldSectionCoordinates, long> before = new();
    foreach (PipelineLiquidChangeCommand command in commands)
    {
      WorldSectionCoordinates section = world.GetSectionCoordinates(command.X, command.Y);
      before.TryAdd(section, world.GetSectionVersion(section));
    }

    world.CommitLiquidChanges([.. commands]);
    int applied = 0;
    foreach (PipelineLiquidChangeCommand command in commands)
    {
      applied++;
      WorldSectionCoordinates section = world.GetSectionCoordinates(command.X, command.Y);
      if (world.GetSectionVersion(section) > before[section])
      {
        dirtySections.Mark(section, world.GetSectionVersion(section));
      }
    }

    result = new LiquidCommitResult(true, applied, null);
    return true;
  }
}

public sealed record LiquidCommitResult(bool Succeeded, int AppliedCount, string? FailureReason)
{
  public static LiquidCommitResult Failed(string reason)
  {
    return new LiquidCommitResult(false, 0, reason);
  }
}
