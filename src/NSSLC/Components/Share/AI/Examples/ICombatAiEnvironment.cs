using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Examples;

/// <summary>
/// Action port. Unified AI implementations emit intents and read execution feedback.
/// </summary>
public interface ICombatAiEnvironment : IAgentEnvironment {
  void BeginAttack(ITarget target);
  bool HasAttackFinished(ITarget target);
  void CancelAttack();
}
