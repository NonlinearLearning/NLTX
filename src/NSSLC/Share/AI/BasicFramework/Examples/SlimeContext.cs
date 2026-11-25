using Terraria.Share.Ai.Htn.Contexts;

namespace Terraria.Share.Ai.Examples;

/// <summary>Per-slime sensor snapshot and one-tick intent; this is not an ECS component.</summary>
public sealed class SlimeContext : EnumContext<SlimeFact> {
  public float TargetDirection { get; set; }
  public bool WantsJump { get; set; }
  public float MoveDirection { get; set; }

  public void ClearIntent() {
    WantsJump = false;
    MoveDirection = 0f;
  }
}
