using System;
using System.Collections.Generic;

namespace Terraria.Share.Ai;

/// <summary>
/// Composition-root catalog of AI behaviors keyed by implementation type. Seal it once the set is
/// complete: a sealed catalog rejects further additions, so the behavior set cannot drift while
/// bindings are being attached.
/// </summary>
public sealed class AiBehaviorCatalog<TIntent> where TIntent : unmanaged {
  private readonly Dictionary<Type, AiBehaviorDeclaration<TIntent>> _byType = [];
  private readonly List<AiBehaviorDeclaration<TIntent>> _ordered = [];

  public AiBehaviorCatalog() {
    Declarations = _ordered.AsReadOnly();
  }

  /// <summary>Declarations in addition order; a live view of the catalog contents.</summary>
  public IReadOnlyList<AiBehaviorDeclaration<TIntent>> Declarations { get; }

  public bool IsSealed { get; private set; }

  public AiBehaviorCatalog<TIntent> Add(AiBehaviorDeclaration<TIntent> declaration) {
    ArgumentNullException.ThrowIfNull(declaration);
    if (IsSealed) {
      throw new InvalidOperationException("The AI behavior catalog is sealed.");
    }
    if (!_byType.TryAdd(declaration.BehaviorType, declaration)) {
      throw new InvalidOperationException(
          $"AI behavior {declaration.BehaviorType.FullName} is declared twice.");
    }
    _ordered.Add(declaration);
    return this;
  }

  public void Seal() {
    IsSealed = true;
  }

  public AiBehaviorDeclaration<TIntent> Get(Type behaviorType) {
    ArgumentNullException.ThrowIfNull(behaviorType);
    if (!_byType.TryGetValue(behaviorType, out AiBehaviorDeclaration<TIntent>? declaration)) {
      throw new KeyNotFoundException($"AI behavior {behaviorType.FullName} is not declared.");
    }
    return declaration;
  }
}
