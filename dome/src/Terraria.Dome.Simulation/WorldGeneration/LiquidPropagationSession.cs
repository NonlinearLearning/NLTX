using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation.WorldGeneration.Systems;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class LiquidPropagationSession
{
  private readonly IReadOnlyList<LiquidDefinition> _definitions;
  private readonly IReadOnlyList<LiquidMergeComponent> _merges;
  private readonly WorldGrid _world;
  private readonly WorldMetadata _metadata;
  private readonly List<LiquidWorkItemComponent> _workItems;
  private readonly HashSet<LiquidPropagationVisit> _completedVisits;
  private WorldGenerationStateComponent _state;

  public LiquidPropagationSession(
    WorldGrid world,
    WorldMetadata metadata,
    IReadOnlyCollection<LiquidDefinition> definitions,
    IReadOnlyCollection<LiquidMergeComponent> merges,
    WorldGenerationStateComponent state,
    IReadOnlyCollection<LiquidWorkItemComponent> workItems,
    IReadOnlyCollection<LiquidPropagationVisit>? completedVisits = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(metadata);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(merges);
    ArgumentNullException.ThrowIfNull(workItems);
    if (world.Width != metadata.Width || world.Height != metadata.Height)
    {
      throw new ArgumentException(
        "World metadata dimensions must match the liquid propagation world.",
        nameof(metadata));
    }

    _world = world;
    _metadata = metadata;
    _definitions = Array.AsReadOnly(definitions.ToArray());
    _merges = Array.AsReadOnly(merges.ToArray());
    _state = state;
    _workItems = new List<LiquidWorkItemComponent>(workItems);
    _completedVisits = completedVisits is null
      ? new HashSet<LiquidPropagationVisit>()
      : new HashSet<LiquidPropagationVisit>(completedVisits);
  }

  public WorldGenerationStateComponent State => _state;

  public int PendingWorkItemCount => _workItems.Count;

  public static LiquidPropagationSession Restore(
    LiquidPropagationCheckpoint checkpoint,
    IReadOnlyCollection<LiquidDefinition> definitions,
    IReadOnlyCollection<LiquidMergeComponent> merges)
  {
    ArgumentNullException.ThrowIfNull(checkpoint);
    return new LiquidPropagationSession(
      checkpoint.RestoreWorld(),
      checkpoint.Snapshot.Metadata,
      definitions,
      merges,
      checkpoint.State,
      checkpoint.PendingWorkItems,
      checkpoint.CompletedVisits);
  }

  public LiquidPropagationAdvanceResult Advance(int budget)
  {
    if (budget < 0)
    {
      return LiquidPropagationAdvanceResult.Failed(
        "Liquid propagation budget cannot be negative.",
        _workItems.Count);
    }

    WorldGenerationStateComponent originalState = _state;
    List<LiquidWorkItemComponent> originalWorkItems = new(_workItems);
    List<Terraria.Dome.Simulation.Commands.LiquidChangeCommand> commands = new();
    LiquidPropagationResult propagationResult = new LiquidPropagationSystem().TryAppendCommands(
      _world.CreateSnapshot(_metadata),
      _definitions,
      _merges,
      _completedVisits,
      _workItems,
      budget,
      ref _state,
      commands);
    if (!propagationResult.Succeeded)
    {
      Restore(originalState, originalWorkItems);
      return LiquidPropagationAdvanceResult.Failed(
        propagationResult.FailureReason ?? "Liquid propagation failed.",
        _workItems.Count);
    }

    if (!new LiquidChangeCommitSystem().TryCommit(
          _world,
          commands,
          _definitions,
          out LiquidChangeCommitResult commitResult))
    {
      Restore(originalState, originalWorkItems);
      return LiquidPropagationAdvanceResult.Failed(
        commitResult.FailureReason ?? "Liquid propagation commit failed.",
        _workItems.Count);
    }

    MarkCompletedVisits(originalWorkItems);

    return new LiquidPropagationAdvanceResult(
      true,
      commitResult.AppliedCount,
      _workItems.Count,
      null);
  }

  public LiquidPropagationCheckpoint CreateCheckpoint()
  {
    return new LiquidPropagationCheckpoint(
      _world.CreateSnapshot(_metadata),
      _state,
      _workItems,
      _completedVisits);
  }

  public WorldGridSnapshot CreateSnapshot()
  {
    return _world.CreateSnapshot(_metadata);
  }

  private void Restore(
    WorldGenerationStateComponent state,
    IReadOnlyCollection<LiquidWorkItemComponent> workItems)
  {
    _state = state;
    _workItems.Clear();
    _workItems.AddRange(workItems);
  }

  private void MarkCompletedVisits(IReadOnlyCollection<LiquidWorkItemComponent> originalWorkItems)
  {
    HashSet<long> pendingSequences = new(_workItems.Select(workItem => workItem.Sequence));
    foreach (LiquidWorkItemComponent workItem in originalWorkItems)
    {
      if (!pendingSequences.Contains(workItem.Sequence))
      {
        _completedVisits.Add(new LiquidPropagationVisit(
          workItem.X,
          workItem.Y,
          workItem.LiquidType));
      }
    }
  }
}
