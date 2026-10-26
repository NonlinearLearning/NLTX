using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Combat;

/// <summary>
/// Independent component values produced for one entity instance. The host adapts this set to Arch.
/// </summary>
public sealed class EntityComponentSet {
  private readonly Dictionary<Type, object> _values;
  private readonly ReadOnlyDictionary<Type, object> _readOnlyValues;
  private readonly ReadOnlyCollection<EntityComponentValue> _orderedValues;

  internal EntityComponentSet(IEnumerable<EntityComponentValue> values) {
    List<EntityComponentValue> orderedValues = new(values);
    _values = new Dictionary<Type, object>();
    foreach (EntityComponentValue value in orderedValues) {
      if (!_values.TryAdd(value.ComponentType, value.Value)) {
        throw new InvalidOperationException(
            $"Component {value.ComponentType.FullName} was produced more than once.");
      }
    }
    _readOnlyValues = new ReadOnlyDictionary<Type, object>(_values);
    _orderedValues = orderedValues.AsReadOnly();
  }

  public IReadOnlyCollection<Type> ComponentTypes => _readOnlyValues.Keys;

  public bool Contains<TComponent>() {
    return _readOnlyValues.ContainsKey(typeof(TComponent));
  }

  public bool TryGet<TComponent>(out TComponent component) {
    if (_readOnlyValues.TryGetValue(typeof(TComponent), out object? value) &&
        value is TComponent typed) {
      component = typed;
      return true;
    }
    component = default!;
    return false;
  }

  public TComponent Get<TComponent>() {
    if (!TryGet(out TComponent component)) {
      throw new KeyNotFoundException($"Entity component {typeof(TComponent).FullName} is missing.");
    }
    return component;
  }

  /// <summary>Returns a stable snapshot of the values for this spawn operation.</summary>
  public IReadOnlyList<EntityComponentValue> Values => _orderedValues;
}
