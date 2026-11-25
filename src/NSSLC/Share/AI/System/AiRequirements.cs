using System;
using System.Collections.Generic;

namespace Terraria.Share.Ai;

/// <summary>Immutable requirements; managed components must be projected to value inputs.</summary>
public sealed class AiRequirements {
  private readonly Type[] _types;

  public static AiRequirements Empty { get; } = new([]);
  public IReadOnlyList<Type> Components { get; }

  private AiRequirements(Type[] types) {
    _types = types;
    Components = Array.AsReadOnly(types);
  }

  public AiRequirements Require<TComponent>() where TComponent : unmanaged {
    Type type = typeof(TComponent);
    if (Array.IndexOf(_types, type) >= 0) {
      return this;
    }
    Type[] types = new Type[_types.Length + 1];
    _types.CopyTo(types, 0);
    types[^1] = type;
    return new AiRequirements(types);
  }

  public bool Contains<TComponent>() where TComponent : unmanaged {
    return Array.IndexOf(_types, typeof(TComponent)) >= 0;
  }

  internal Type[] FindMissing(AiSnapshot snapshot) {
    List<Type> missing = [];
    foreach (Type type in _types) {
      if (!snapshot.Contains(type)) {
        missing.Add(type);
      }
    }
    return missing.ToArray();
  }
}
