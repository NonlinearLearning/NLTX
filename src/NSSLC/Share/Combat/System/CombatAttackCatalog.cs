using System;
using System.Collections.Generic;

namespace Terraria.Share.Combat;

/// <summary>Immutable-after-sealing directory for attack content definitions.</summary>
public sealed class CombatAttackCatalog
{
  private readonly Dictionary<CombatAttackId, CombatAttack> _definitions = [];
  private readonly Dictionary<ulong, CombatAttack> _stableDefinitions = [];
  private bool _sealed;

  public void Register(CombatAttack definition)
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

  public bool TryGet(CombatAttackId id, out CombatAttack definition)
  {
    return _definitions.TryGetValue(id, out definition!);
  }

  public bool TryGet(ulong stableKey, out CombatAttack definition)
  {
    return _stableDefinitions.TryGetValue(stableKey, out definition!);
  }

  public CombatAttack Get(CombatAttackId id)
  {
    if (!TryGet(id, out CombatAttack definition))
    {
      throw new KeyNotFoundException($"Attack definition {id} is not registered.");
    }

    return definition;
  }
}
