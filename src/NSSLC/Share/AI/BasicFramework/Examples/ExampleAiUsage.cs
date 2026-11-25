namespace Terraria.Share.Ai.Examples;

/// <summary>Common entry points for both examples. No entity access or intent execution.</summary>
public static class ExampleAiUsage {
  public static AiSystem<ExampleAiIntent> CreateSystem() {
    AiSystem<ExampleAiIntent> system = new();
    system.Register<SlimeAiBehavior>();
    system.Register<AttackGoapAiBehavior>();
    return system;
  }

  public static AiResult<ExampleAiIntent> Calculate(AiBinding<ExampleAiIntent> binding,
      in AiComponent component, ExampleMovementComponent movement, ExampleTargetComponent target,
      ExampleAttackFeedbackComponent feedback, float deltaTime) {
    AiSnapshot snapshot = AiSnapshot.Empty
        .With(movement)
        .With(target)
        .With(feedback);
    return binding.Calculate(component, snapshot, deltaTime);
  }
}
