using Terraria.Share.Ai.Goap.Core;
using Terraria.Share.Ai.Goap.Runtime;

namespace Terraria.Share.Ai.Examples;

public sealed class DefeatEnemyGoal : GoalBase {
  public DefeatEnemyGoal()
      : base(1f, new ValueCondition(new EnemyAliveKey(), Comparison.SmallerThanOrEqual, 0)) {
  }
}
