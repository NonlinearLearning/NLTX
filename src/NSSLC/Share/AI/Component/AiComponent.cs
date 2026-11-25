using System;

namespace Terraria.Share.Ai;

/// <summary>Entity declaration only; contains no planner, context, world or service.</summary>
public readonly record struct AiComponent {
  public Type? BehaviorType { get; }
  public bool Enabled { get; init; }

  private AiComponent(Type behaviorType) {
    BehaviorType = behaviorType;
    Enabled = true;
  }

  public static AiComponent For<TBehavior>() where TBehavior : IAiBehavior {
    return For(typeof(TBehavior));
  }

  public static AiComponent For(Type behaviorType) {
    ArgumentNullException.ThrowIfNull(behaviorType);
    if (!typeof(IAiBehavior).IsAssignableFrom(behaviorType)) {
      throw new ArgumentException(
          $"The type {behaviorType.FullName} does not implement IAiBehavior.",
          nameof(behaviorType));
    }

    return new AiComponent(behaviorType);
  }
}
