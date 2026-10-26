using System;
using System.Collections.Generic;

namespace Terraria.StatusEffects;

/// <summary>Explicit registry for immutable shared Buff definitions.</summary>
public sealed class BuffDefinitionRegistry
{
  private readonly Dictionary<BuffEffectId, BuffDefinition> _definitions = [];

  public int Count => _definitions.Count;

  public void Register(BuffDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    if (!_definitions.TryAdd(definition.Effect, definition))
    {
      throw new InvalidOperationException(
          $"The Buff definition {definition.Effect.Value} is already registered.");
    }
  }

  public bool TryGet(BuffEffectId effect, out BuffDefinition definition)
  {
    return _definitions.TryGetValue(effect, out definition!);
  }
}
