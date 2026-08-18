using System.Collections.Generic;
using Terraria.Dome.Simulation.Liquid.Snapshots;
using Terraria.Dome.Simulation.WorldModel;
using PipelineLiquidChangeCommand = Terraria.Dome.Simulation.Commands.LiquidChangeCommand;

namespace Terraria.Dome.Simulation.Liquid.Systems;

public sealed class LiquidReplicationSystem
{
  public IReadOnlyList<WorldLiquidSnapshot> CreateSnapshots(
    IReadOnlyCollection<PipelineLiquidChangeCommand> commands,
    long revision)
  {
    Dictionary<(int X, int Y), PipelineLiquidChangeCommand> latest = new();
    foreach (PipelineLiquidChangeCommand command in commands)
    {
      if (!latest.TryGetValue(
            (command.X, command.Y),
            out PipelineLiquidChangeCommand current) ||
          command.Sequence > current.Sequence)
      {
        latest[(command.X, command.Y)] = command;
      }
    }

    List<WorldLiquidSnapshot> snapshots = new(latest.Count);
    foreach (PipelineLiquidChangeCommand command in latest.Values)
    {
      snapshots.Add(new WorldLiquidSnapshot(command.X, command.Y, command.Amount, command.Type));
    }

    snapshots.Sort(static (first, second) =>
    {
      int x = first.X.CompareTo(second.X);
      return x != 0 ? x : first.Y.CompareTo(second.Y);
    });
    _ = revision;
    return snapshots;
  }

  public IReadOnlyList<LiquidReplicationSnapshot> CreateRevisionedSnapshots(
    IReadOnlyCollection<PipelineLiquidChangeCommand> commands,
    long revision)
  {
    IReadOnlyList<WorldLiquidSnapshot> snapshots = CreateSnapshots(commands, revision);
    List<LiquidReplicationSnapshot> result = new(snapshots.Count);
    for (int index = 0; index < snapshots.Count; index++)
    {
      WorldLiquidSnapshot snapshot = snapshots[index];
      result.Add(new LiquidReplicationSnapshot(
        snapshot.X,
        snapshot.Y,
        snapshot.Amount,
        snapshot.Type,
        revision));
    }

    return result;
  }
}
