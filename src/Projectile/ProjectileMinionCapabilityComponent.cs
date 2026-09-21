namespace Terraria.Projectile;

public struct ProjectileMinionCapabilityComponent
{
  public ProjectileMinionCapabilityComponent(
    float minionSlots = 0.0f,
    int minionPosition = 0,
    bool isMinion = false)
  {
    MinionSlots = minionSlots;
    MinionPosition = minionPosition;
    IsMinion = isMinion;
  }

  public float MinionSlots;
  public int MinionPosition;
  public bool IsMinion;
}
