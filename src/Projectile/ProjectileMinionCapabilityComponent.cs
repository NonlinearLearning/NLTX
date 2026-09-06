namespace Terraria.Projectile;

public struct ProjectileMinionCapabilityComponent
{
  public ProjectileMinionCapabilityComponent(
    float minionSlots = 0.0f,
    int minionPosition = 0)
  {
    MinionSlots = minionSlots;
    MinionPosition = minionPosition;
  }

  public float MinionSlots;
  public int MinionPosition;
}
