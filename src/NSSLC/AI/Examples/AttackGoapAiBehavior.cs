namespace Terraria.Ai.Examples;

/// <summary>
/// The existing GOAP goal/action example bound to the same API as SlimeAiBehavior.
/// </summary>
public sealed class AttackGoapAiBehavior
    : GoapAiBehavior<ExampleCombatAiEnvironment, ExampleAiIntent> {
  private static readonly AiRequirements _requirements = AiRequirements.Empty
      .Require<ExampleMovementComponent>()
      .Require<ExampleTargetComponent>()
      .Require<ExampleAttackFeedbackComponent>();

  public AttackGoapAiBehavior()
      : base(_requirements, new ExampleCombatAiEnvironment(), [new AttackAction()],
             new DefeatEnemyGoal()) {
  }
}
