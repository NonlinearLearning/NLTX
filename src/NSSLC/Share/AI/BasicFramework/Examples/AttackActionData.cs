using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Examples;

public sealed class AttackActionData : IActionData {
  public ITarget? Target { get; set; }
  public bool AttackStarted { get; set; }
}
