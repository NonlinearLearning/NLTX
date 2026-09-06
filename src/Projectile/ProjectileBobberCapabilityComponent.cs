namespace Terraria.Projectile;

public struct ProjectileBobberCapabilityComponent
{
  public ProjectileBobberCapabilityComponent(int bobberType = 0)
  {
    BobberType = bobberType;
  }

  public int BobberType;
}
