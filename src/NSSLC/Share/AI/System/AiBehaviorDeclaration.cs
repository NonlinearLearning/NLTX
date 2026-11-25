using System;

namespace Terraria.Share.Ai;

/// <summary>
/// Immutable composition-root statement of one AI behavior: the implementation type, the factory
/// that creates a fresh runtime, and the value inputs that runtime reads.
/// </summary>
/// <remarks>
/// The factory receives <see cref="Reads"/> instead of closing over its own copy, so a runtime
/// cannot be built with a read contract different from the declared one. A declaration lives in
/// the composition root and never becomes component state.
/// </remarks>
public sealed class AiBehaviorDeclaration<TIntent> where TIntent : unmanaged {
  private readonly Func<AiRequirements, AiBehavior<TIntent>> _factory;

  public Type BehaviorType { get; }
  public AiRequirements Reads { get; }

  public AiBehaviorDeclaration(
      Type behaviorType,
      Func<AiRequirements, AiBehavior<TIntent>> factory,
      AiRequirements reads) {
    ArgumentNullException.ThrowIfNull(behaviorType);
    ArgumentNullException.ThrowIfNull(factory);
    ArgumentNullException.ThrowIfNull(reads);
    if (!typeof(AiBehavior<TIntent>).IsAssignableFrom(behaviorType)) {
      throw new ArgumentException(
          $"{behaviorType.FullName} is not an AI behavior for {typeof(TIntent).FullName}.",
          nameof(behaviorType));
    }
    BehaviorType = behaviorType;
    _factory = factory;
    Reads = reads;
  }

  /// <summary>Declares one behavior whose factory derives every runtime from the same reads.</summary>
  public static AiBehaviorDeclaration<TIntent> For<TBehavior>(
      Func<AiRequirements, TBehavior> factory,
      AiRequirements reads) where TBehavior : AiBehavior<TIntent> {
    ArgumentNullException.ThrowIfNull(factory);
    return new AiBehaviorDeclaration<TIntent>(
        typeof(TBehavior),
        requirements => factory(requirements),
        reads);
  }

  /// <summary>Creates one fresh runtime carrying this declaration's read contract.</summary>
  public AiBehavior<TIntent> CreateBehavior() {
    AiBehavior<TIntent> behavior = _factory(Reads) ??
        throw new InvalidOperationException(
            $"The factory for {BehaviorType.FullName} returned null.");
    if (behavior.GetType() != BehaviorType) {
      throw new InvalidOperationException(
          $"The declaration for {BehaviorType.FullName} created {behavior.GetType().FullName}.");
    }
    return behavior;
  }
}
