using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcCheckActiveWormSegmentPolicy
{
  public const int Version1456MaxNpcCount = 200;

  private const int WormAiStyle = 6;

  public static NpcCheckActiveWormSegmentDecision Evaluate(
    NpcCheckActiveWormSegmentInput input)
  {
    ValidateEnvelope(input);
    if (input.TriggerAiStyle != WormAiStyle)
    {
      return new(false, Array.Empty<DespawnNpcCommand>());
    }

    if (!float.IsFinite(input.TriggerNextSegment))
    {
      throw new ArgumentOutOfRangeException(nameof(input.TriggerNextSegment));
    }

    ValidateSegments(input);
    Dictionary<int, NpcCheckActiveWormSegmentState> segments = new(input.Segments.Count);
    for (int index = 0; index < input.Segments.Count; index++)
    {
      NpcCheckActiveWormSegmentState segment = input.Segments[index];
      if (!segments.TryAdd(segment.Handle.Value, segment))
      {
        throw new ArgumentException(
          "NPC CheckActive worm-segment handles must be unique.",
          nameof(input.Segments));
      }
    }

    List<DespawnNpcCommand> commands = new();
    HashSet<int> visited = new();
    if (!TryProjectSegmentIndex(input.TriggerNextSegment, out int next))
    {
      return new(true, Array.Empty<DespawnNpcCommand>());
    }

    while (next != input.Trigger.Value && next > 0 && next < input.MaxNpcCount &&
           visited.Add(next) &&
           segments.TryGetValue(next, out NpcCheckActiveWormSegmentState segment))
    {
      if (!segment.IsActive || segment.AiStyle != WormAiStyle)
      {
        break;
      }

      commands.Add(new DespawnNpcCommand(
        segment.Handle,
        NpcDespawnReason.SegmentRootRemoved));
      if (!TryProjectSegmentIndex(segment.NextSegment, out next))
      {
        break;
      }
    }

    return new(true, commands.ToArray());
  }

  private static void ValidateEnvelope(NpcCheckActiveWormSegmentInput input)
  {
    if (input.MaxNpcCount <= 0 || input.MaxNpcCount > Version1456MaxNpcCount)
    {
      throw new ArgumentOutOfRangeException(nameof(input.MaxNpcCount));
    }

    if (!input.Trigger.IsValid || input.Trigger.Value >= input.MaxNpcCount)
    {
      throw new ArgumentOutOfRangeException(nameof(input.Trigger));
    }

    if (input.TriggerAiStyle < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(input.TriggerAiStyle));
    }
  }

  private static void ValidateSegments(NpcCheckActiveWormSegmentInput input)
  {
    ArgumentNullException.ThrowIfNull(input.Segments);
    for (int index = 0; index < input.Segments.Count; index++)
    {
      NpcCheckActiveWormSegmentState segment = input.Segments[index];
      if (!segment.Handle.IsValid || segment.Handle.Value >= input.MaxNpcCount)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Segments));
      }

      if (segment.AiStyle < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Segments));
      }
    }
  }

  private static bool TryProjectSegmentIndex(float value, out int index)
  {
    index = 0;
    if (!float.IsFinite(value) || value < int.MinValue || value > int.MaxValue)
    {
      return false;
    }

    index = (int)value;
    return true;
  }
}
