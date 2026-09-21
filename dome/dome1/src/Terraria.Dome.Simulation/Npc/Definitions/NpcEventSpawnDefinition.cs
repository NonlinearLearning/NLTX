using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public readonly record struct NpcEventSpawnDefinition
{
  public NpcEventSpawnDefinition(
    int eventType,
    int npcDefinitionId,
    int maximumPerTick,
    float difficultyScale = 1.0f)
  {
    if (eventType is < 1 or > 4 || npcDefinitionId <= 0 || maximumPerTick <= 0 ||
        !float.IsFinite(difficultyScale) || difficultyScale <= 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(eventType));
    }

    EventType = eventType;
    NpcDefinitionId = npcDefinitionId;
    MaximumPerTick = maximumPerTick;
    DifficultyScale = difficultyScale;
  }

  public int EventType { get; }
  public int NpcDefinitionId { get; }
  public int MaximumPerTick { get; }
  public float DifficultyScale { get; }
}

public sealed class NpcEventSpawnTable
{
  private readonly IReadOnlyDictionary<int, IReadOnlyList<NpcEventSpawnDefinition>> _definitions;

  public NpcEventSpawnTable(IEnumerable<NpcEventSpawnDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    Dictionary<int, List<NpcEventSpawnDefinition>> grouped = new();
    foreach (NpcEventSpawnDefinition definition in definitions)
    {
      if (!grouped.TryGetValue(definition.EventType, out List<NpcEventSpawnDefinition>? entries))
      {
        entries = new List<NpcEventSpawnDefinition>();
        grouped.Add(definition.EventType, entries);
      }

      if (entries.Exists(entry => entry.NpcDefinitionId == definition.NpcDefinitionId))
      {
        throw new ArgumentException(
          $"NPC event definition {definition.NpcDefinitionId} is duplicated for event {definition.EventType}.",
          nameof(definitions));
      }

      entries.Add(definition);
    }

    Dictionary<int, IReadOnlyList<NpcEventSpawnDefinition>> snapshot = new();
    foreach (KeyValuePair<int, List<NpcEventSpawnDefinition>> pair in grouped)
    {
      snapshot.Add(pair.Key, pair.Value.AsReadOnly());
    }

    _definitions = snapshot;
  }

  public bool TryGet(int eventType, out IReadOnlyList<NpcEventSpawnDefinition> definitions)
  {
    return _definitions.TryGetValue(eventType, out definitions!);
  }
}
