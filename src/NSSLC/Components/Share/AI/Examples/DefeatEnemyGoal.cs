using Terraria.Ai.Goap.Core;
using Terraria.Ai.Goap.Runtime;

namespace Terraria.Ai.Examples;

public sealed class DefeatEnemyGoal : GoalBase {
  public DefeatEnemyGoal()
      : base(1f, new ValueCondition(new EnemyAliveKey(), Comparison.SmallerThanOrEqual, 0)) {
  }
}
