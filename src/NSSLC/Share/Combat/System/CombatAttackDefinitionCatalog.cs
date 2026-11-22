using System;
using System.Collections.Generic;

namespace Terraria.Combat;

/// <summary>Immutable-after-sealing directory for attack content definitions.</summary>
public sealed class CombatAttackDefinitionCatalog
{
  private readonly Dictionary<CombatAttackDefinitionId, CombatAttackDefinition> _definitions = [];
  private readonly Dictionary<ulong, CombatAttackDefinition> _stableDefinitions = [];
  private bool _sealed;

  public void Register(CombatAttackDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    if (_sealed)
    {
      throw new InvalidOperationException("The attack definition catalog is sealed.");
    }

    if (!_definitions.TryAdd(definition.Id, definition))
    {
      throw new InvalidOperationException(
          $"Attack definition {definition.Id} is already registered.");
    }

    if (!_stableDefinitions.TryAdd(definition.Id.StableKey, definition))
    {
      _definitions.Remove(definition.Id);
      throw new InvalidOperationException(
          $"Attack definition stable key collides for {definition.Id}.");
    }
  }

  public void Seal()
  {
    _sealed = true;
  }

  public bool TryGet(CombatAttackDefinitionId id, out CombatAttackDefinition definition)
  {
    return _definitions.TryGetValue(id, out definition!);
  }

  public bool TryGet(ulong stableKey, out CombatAttackDefinition definition)
  {
    return _stableDefinitions.TryGetValue(stableKey, out definition!);
  }

  public CombatAttackDefinition Get(CombatAttackDefinitionId id)
  {
    if (!TryGet(id, out CombatAttackDefinition definition))
    {
      throw new KeyNotFoundException($"Attack definition {id} is not registered.");
    }

    return definition;
  }
}
