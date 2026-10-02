namespace Terraria.Projectile;

public struct ProjectileBobberCapabilityComponent
{
  public ProjectileBobberCapabilityComponent(
    int bobberType = 0,
    bool isBobber = false)
  {
    BobberType = bobberType;
    IsBobber = isBobber;
  }

  public int BobberType;
  public bool IsBobber;
}
