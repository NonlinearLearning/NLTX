using System;
using System.Collections.Generic;

namespace Terraria.Share.Attributes;

/// <summary>Explicit registry for immutable shared Buff definitions.</summary>
public sealed class BuffRegistry
{
  private readonly Dictionary<BuffEffectId, Buff> _definitions = [];

  public int Count => _definitions.Count;

  public void Register(Buff definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    if (!_definitions.TryAdd(definition.Effect, definition))
    {
      throw new InvalidOperationException(
          $"The Buff definition {definition.Effect.Value} is already registered.");
    }
  }

  public bool TryGet(BuffEffectId effect, out Buff definition)
  {
    return _definitions.TryGetValue(effect, out definition!);
  }
}
