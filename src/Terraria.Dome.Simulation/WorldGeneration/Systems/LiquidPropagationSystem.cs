using System;
using System.Collections.Generic;
using System.Linq;
using PipelineLiquidChangeCommand = Terraria.Dome.Simulation.Commands.LiquidChangeCommand;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class LiquidPropagationSystem
{
  public LiquidPropagationResult TryAppendCommands(
    WorldGridSnapshot snapshot,
    IReadOnlyCollection<LiquidDefinition> definitions,
    IReadOnlyCollection<LiquidMergeComponent> merges,
    List<LiquidWorkItemComponent> workItems,
    int budget,
    ref WorldGenerationStateComponent state,
    List<PipelineLiquidChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(merges);
    ArgumentNullException.ThrowIfNull(workItems);
    ArgumentNullException.ThrowIfNull(commands);
    if (budget < 0)
    {
      return LiquidPropagationResult.Failed("Liquid propagation budget cannot be negative.");
    }

    Dictionary<byte, LiquidDefinition> byType = definitions.ToDictionary(
      definition => definition.Type);
    foreach (LiquidWorkItemComponent workItem in workItems)
    {
      if (!byType.ContainsKey(workItem.LiquidType))
      {
        return LiquidPropagationResult.Failed("Liquid work item type was not defined.");
      }
    }

    HashSet<(int X, int Y, byte Type)> visited = new();
    int consumed = 0;
    while (consumed < budget && workItems.Count > 0)
    {
      workItems.Sort(static (first, second) =>
      {
        int sequenceComparison = first.Sequence.CompareTo(second.Sequence);
        if (sequenceComparison != 0)
        {
          return sequenceComparison;
        }

        int xComparison = first.X.CompareTo(second.X);
        return xComparison != 0 ? xComparison : first.Y.CompareTo(second.Y);
      });
      LiquidWorkItemComponent workItem = workItems[0];
      workItems.RemoveAt(0);
      if (!snapshot.Metadata.IsInside(workItem.X, workItem.Y) || workItem.Amount == 0 ||
          !visited.Add((workItem.X, workItem.Y, workItem.LiquidType)))
      {
        continue;
      }

      WorldTile current = snapshot.GetTile(workItem.X, workItem.Y);
      byte liquidType = workItem.LiquidType;
      if (current.LiquidAmount > 0 && current.LiquidType != liquidType)
      {
        LiquidMergeComponent? merge = null;
        foreach (LiquidMergeComponent candidate in merges)
        {
          if (candidate.Matches(current.LiquidType, liquidType))
          {
            merge = candidate;
            break;
          }
        }

        if (merge is null)
        {
          return LiquidPropagationResult.Failed("Liquid types could not be merged.");
        }

        liquidType = merge.Value.ResultType;
      }

      byte amount = Math.Min(workItem.Amount, byType[liquidType].MaxAmount);
      commands.Add(new PipelineLiquidChangeCommand(
        state.ReserveSequence(),
        workItem.X,
        workItem.Y,
        amount,
        liquidType));
      consumed++;
      byte nextAmount = (byte)(workItem.Amount / 2);
      if (nextAmount == 0)
      {
        continue;
      }

      foreach ((int x, int y) in GetNeighbors(workItem.X, workItem.Y))
      {
        if (snapshot.Metadata.IsInside(x, y) &&
            !visited.Contains((x, y, workItem.LiquidType)))
        {
          workItems.Add(new LiquidWorkItemComponent(
            x,
            y,
            workItem.LiquidType,
            nextAmount,
            state.ReserveSequence()));
        }
      }
    }

    return new LiquidPropagationResult(true, consumed, workItems.Count, null);
  }

  private static IEnumerable<(int X, int Y)> GetNeighbors(int x, int y)
  {
    yield return (x, y - 1);
    yield return (x - 1, y);
    yield return (x + 1, y);
    yield return (x, y + 1);
  }
}

public sealed record LiquidPropagationResult(
  bool Succeeded,
  int ConsumedWorkItems,
  int RemainingWorkItems,
  string? FailureReason)
{
  public static LiquidPropagationResult Failed(string reason)
  {
    return new LiquidPropagationResult(false, 0, 0, reason);
  }
}
