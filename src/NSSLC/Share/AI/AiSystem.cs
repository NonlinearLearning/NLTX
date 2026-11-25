using System;
using System.Collections.Generic;

namespace Terraria.Share.Ai;

/// <summary>Unified behavior registry and binding factory; owns no ECS entity or world.</summary>
public sealed class AiSystem<TIntent> where TIntent : unmanaged {
  private readonly Dictionary<Type, Func<AiBehavior<TIntent>>> _factories = new();

  public void Register<TBehavior>() where TBehavior : AiBehavior<TIntent>, new() {
    Register(static () => new TBehavior());
  }

  public void Register<TBehavior>(Func<TBehavior> factory) where TBehavior : AiBehavior<TIntent> {
    ArgumentNullException.ThrowIfNull(factory);
    if (!_factories.TryAdd(typeof(TBehavior), () => factory())) {
      throw new InvalidOperationException(
          $"AI behavior {typeof(TBehavior).FullName} is already registered.");
    }
  }

  /// <summary>Registers one catalog declaration; every bind creates a fresh runtime from it.</summary>
  public void Register(AiBehaviorDeclaration<TIntent> declaration) {
    ArgumentNullException.ThrowIfNull(declaration);
    Type type = declaration.BehaviorType;
    if (!_factories.TryAdd(type, declaration.CreateBehavior)) {
      throw new InvalidOperationException(
          $"AI behavior {type.FullName} is already registered.");
    }
  }

  public AiBinding<TIntent> Bind(in AiComponent component) {
    Type type = component.BehaviorType ??
        throw new ArgumentException("Declare an AI behavior with AiComponent.For<TBehavior>().",
                                    nameof(component));
    if (!_factories.TryGetValue(type, out Func<AiBehavior<TIntent>>? factory)) {
      throw new InvalidOperationException($"AI behavior {type.FullName} is not registered.");
    }
    AiBehavior<TIntent> behavior = factory() ??
        throw new InvalidOperationException("The AI factory returned null.");
    behavior.Claim();
    return new AiBinding<TIntent>(type, behavior);
  }

  /// <summary>Creates a binding from a validated domain-owned behavior factory.</summary>
  public AiBinding<TIntent> Bind(
      in AiComponent component,
      Func<AiBehavior<TIntent>> factory) {
    Type type = component.BehaviorType ??
        throw new ArgumentException("An AI behavior type is required.", nameof(component));
    ArgumentNullException.ThrowIfNull(factory);
    AiBehavior<TIntent> behavior = factory() ??
        throw new InvalidOperationException("The AI factory returned null.");
    if (behavior.GetType() != type) {
      AiOutput<TIntent> cleanup = new();
      behavior.Dispose(cleanup);
      throw new InvalidOperationException(
          $"The profile factory returned {behavior.GetType().FullName}, expected {type.FullName}.");
    }
    behavior.Claim();
    return new AiBinding<TIntent>(type, behavior);
  }

}
