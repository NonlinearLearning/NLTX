using Terraria.Ai.Goap.Core;
using Terraria.Ai.Goap.Runtime;

namespace Terraria.Ai.Examples;

/// <summary>
/// GOAP example; the environment records attack requests, never applies damage here.
/// </summary>
public sealed class AttackAction : GoapActionBase<AttackActionData, EmptyActionProperties> {
  public AttackAction() : base(new ActionConfig {
    Name = "Attack target",
    StoppingDistance = 32f,
    Conditions = [new ValueCondition(new EnemyAliveKey(), Comparison.GreaterThan, 0)],
    Effects = [new Effect { WorldKey = new EnemyAliveKey(), Increase = false }]
  }) {
  }

  public override void BeforePerform(IAgent agent, AttackActionData data) {
    data.AttackStarted = true;
    ((ICombatAiEnvironment)agent.Environment).BeginAttack(data.Target!);
  }

  public override IActionRunState Perform(IAgent agent, AttackActionData data,
                                          IActionContext context) {
    bool finished = ((ICombatAiEnvironment)agent.Environment).HasAttackFinished(data.Target!);
    return finished ? ActionRunState.Completed : ActionRunState.Continue;
  }

  public override void End(IAgent agent, AttackActionData data) {
    if (data.AttackStarted) {
      ((ICombatAiEnvironment)agent.Environment).CancelAttack();
      data.AttackStarted = false;
    }
  }
}
