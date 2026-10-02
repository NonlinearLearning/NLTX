namespace Terraria.Player.Presentation;

public sealed class PlayerCompositeArmStateComponent
{
  public PlayerCompositeArmSnapshot FrontArm { get; private set; }

  public PlayerCompositeArmSnapshot BackArm { get; private set; }

  internal void SetFrontArm(PlayerCompositeArmSnapshot arm)
  {
    FrontArm = arm;
  }

  internal void SetBackArm(PlayerCompositeArmSnapshot arm)
  {
    BackArm = arm;
  }

  internal void Reset()
  {
    FrontArm = default;
    BackArm = default;
  }

  public PlayerCompositeArmsSnapshot ToSnapshot()
  {
    return new PlayerCompositeArmsSnapshot(FrontArm, BackArm);
  }
}
