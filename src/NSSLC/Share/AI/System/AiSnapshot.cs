using System;
using System.Collections.Generic;

namespace Terraria.Ai;

/// <summary>Immutable copies of explicit inputs, indexed by their exact component type.</summary>
public sealed class AiSnapshot {
  private readonly Dictionary<Type, object> _components;

  public static AiSnapshot Empty { get; } = new(new Dictionary<Type, object>());

  private AiSnapshot(Dictionary<Type, object> components) {
    _components = components;
  }

  public AiSnapshot With<TComponent>(TComponent component) where TComponent : unmanaged {
    Dictionary<Type, object> components = new(_components) {
      [typeof(TComponent)] = component
    };
    return new AiSnapshot(components);
  }

  public bool Contains(Type componentType) {
    ArgumentNullException.ThrowIfNull(componentType);
    return _components.ContainsKey(componentType);
  }

  public bool TryGet<TComponent>(out TComponent component) where TComponent : unmanaged {
    if (_components.TryGetValue(typeof(TComponent), out object? value)) {
      component = (TComponent)value;
      return true;
    }
    component = default;
    return false;
  }

  public TComponent Get<TComponent>() where TComponent : unmanaged {
    if (!TryGet(out TComponent component)) {
      throw new KeyNotFoundException($"AI input is missing {typeof(TComponent).FullName}.");
    }
    return component;
  }
}
