using Terraria.Share.Ai.Goap.Core;

namespace Terraria.Share.Ai.Examples;

public sealed class AttackActionData : IActionData {
  public ITarget? Target { get; set; }
  public bool AttackStarted { get; set; }
}
