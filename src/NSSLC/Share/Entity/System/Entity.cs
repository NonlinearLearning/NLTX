using System;
using System.Collections.Generic;
using Terraria.Share.Ai;

namespace Terraria.Share.Entities;

/// <summary>
/// Immutable business definition for an entity's initial component composition.
/// It does not own a runtime entity, Arch world or per-instance mutable state.
/// </summary>
public sealed class EntityDefinition {
  private readonly ComponentFactory[] _factories;
  private readonly Type[] _requiredComponents;

  public EntityId Id { get; }
  public IReadOnlyList<Type> ComponentTypes { get; }
  public IReadOnlyList<Type> RequiredComponents { get; }
  public AiComponent? Ai { get; }

  internal EntityDefinition(EntityId id, ComponentFactory[] factories,
                            Type[] requiredComponents, AiComponent? ai) {
    Id = id;
    _factories = factories;
    _requiredComponents = requiredComponents;
    ComponentTypes = Array.AsReadOnly(GetTypes(factories));
    RequiredComponents = Array.AsReadOnly(requiredComponents);
    Ai = ai;
  }

  public EntityComponentSet CreateComponents() {
    List<EntityComponentValue> values = new(_factories.Length);
    foreach (ComponentFactory factory in _factories) {
      values.Add(factory.Create());
    }
    return new EntityComponentSet(values);
  }

  public bool Provides(Type componentType) {
    ArgumentNullException.ThrowIfNull(componentType);
    foreach (Type provided in ComponentTypes) {
      if (provided == componentType) {
        return true;
      }
    }
    return false;
  }

  private static Type[] GetTypes(ComponentFactory[] factories) {
    Type[] types = new Type[factories.Length];
    for (int i = 0; i < factories.Length; i++) {
      types[i] = factories[i].ComponentType;
    }
    return types;
  }

  internal readonly struct ComponentFactory {
    private readonly Func<object> _factory;

    public Type ComponentType { get; }

    public ComponentFactory(Type componentType, Func<object> factory) {
      ComponentType = componentType;
      _factory = factory;
    }

    public EntityComponentValue Create() {
      object value = _factory() ??
          throw new InvalidOperationException(
              $"Factory for {ComponentType.FullName} returned null.");
      return new EntityComponentValue(ComponentType, value);
    }
  }
}
