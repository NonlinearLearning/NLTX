using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcSegmentState(
  NpcHandle Handle,
  NpcSegmentComponent Segment,
  bool IsActive,
  bool IsDead);

public readonly record struct NpcSegmentValidationResult(bool IsValid, string FailureReason)
{
  public static NpcSegmentValidationResult Valid => new(true, string.Empty);
}

public sealed class NpcSegmentLifecycleSystem
{
  public NpcSegmentValidationResult Validate(IReadOnlyList<NpcSegmentState> segments)
  {
    ArgumentNullException.ThrowIfNull(segments);
    HashSet<NpcHandle> handles = new();
    Dictionary<NpcHandle, NpcSegmentState> byHandle = new();
    for (int index = 0; index < segments.Count; index++)
    {
      NpcSegmentState state = segments[index];
      if (!state.Handle.IsValid || !handles.Add(state.Handle))
      {
        return new(false, "Segment handles must be valid and unique.");
      }

      if (state.Segment.IsRoot && state.Segment.Root != state.Handle)
      {
        return new(false, "A root segment must point its root handle at itself.");
      }

      if (!state.Segment.IsRoot && !state.Segment.Parent.IsValid)
      {
        return new(false, "A non-root segment must reference a parent.");
      }

      byHandle.Add(state.Handle, state);
    }

    foreach (KeyValuePair<NpcHandle, NpcSegmentState> entry in byHandle)
    {
      NpcSegmentState state = entry.Value;
      if (!byHandle.TryGetValue(state.Segment.Root, out NpcSegmentState root) ||
          !root.Segment.IsRoot)
      {
        return new(false, "Segment root ownership is invalid.");
      }

      if (state.Segment.Parent.IsValid &&
          (!byHandle.TryGetValue(state.Segment.Parent, out NpcSegmentState parent) ||
           parent.Segment.Child != state.Handle))
      {
        return new(false, "Parent and child segment links must be reciprocal.");
      }

      if (state.Segment.Child.IsValid &&
          (!byHandle.TryGetValue(state.Segment.Child, out NpcSegmentState child) ||
           child.Segment.Parent != state.Handle))
      {
        return new(false, "Child and parent segment links must be reciprocal.");
      }
    }

    return NpcSegmentValidationResult.Valid;
  }

  public IReadOnlyList<NpcHandle> GetDeathOrder(IReadOnlyList<NpcSegmentState> segments)
  {
    NpcSegmentValidationResult validation = Validate(segments);
    if (!validation.IsValid)
    {
      throw new ArgumentException(validation.FailureReason, nameof(segments));
    }

    return segments
      .Where(segment => segment.IsActive || segment.IsDead)
      .OrderByDescending(segment => segment.Segment.SegmentIndex)
      .ThenByDescending(segment => segment.Handle.Value)
      .Select(segment => segment.Handle)
      .ToArray();
  }

  public IReadOnlyList<DespawnNpcCommand> GetWormFollowUpDespawns(
    IReadOnlyList<NpcSegmentState> segments,
    NpcHandle trigger,
    IReadOnlySet<NpcHandle> wormSegments)
  {
    ArgumentNullException.ThrowIfNull(segments);
    ArgumentNullException.ThrowIfNull(wormSegments);

    NpcSegmentValidationResult validation = Validate(segments);
    if (!validation.IsValid)
    {
      throw new ArgumentException(validation.FailureReason, nameof(segments));
    }

    Dictionary<NpcHandle, NpcSegmentState> byHandle = segments.ToDictionary(
      segment => segment.Handle,
      segment => segment);
    if (!byHandle.TryGetValue(trigger, out NpcSegmentState triggerState) ||
        (triggerState.IsActive && !triggerState.IsDead))
    {
      return Array.Empty<DespawnNpcCommand>();
    }

    List<DespawnNpcCommand> commands = new();
    HashSet<NpcHandle> visited = new();
    NpcHandle next = triggerState.Segment.Child;

    while (next.IsValid &&
           visited.Add(next) &&
           byHandle.TryGetValue(next, out NpcSegmentState state))
    {
      if (!wormSegments.Contains(next) || !state.IsActive)
      {
        break;
      }

      commands.Add(new DespawnNpcCommand(next, NpcDespawnReason.Killed));
      next = state.Segment.Child;
    }

    return commands;
  }
}
