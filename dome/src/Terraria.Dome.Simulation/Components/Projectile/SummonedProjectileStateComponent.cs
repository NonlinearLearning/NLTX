namespace Terraria.Dome.Simulation.Components;

public struct SummonedProjectileStateComponent
{
  public SummonedProjectileStateComponent(
    SummonedProjectileKind kind,
    float slotCost,
    int ownerOrder = -1,
    bool persistsWhileOwnerAlive = false,
    SummonedOwnerEndPolicy ownerInactiveEndPolicy = SummonedOwnerEndPolicy.Despawn)
  {
    Kind = kind;
    SlotCost = slotCost;
    OwnerOrder = ownerOrder;
    PersistsWhileOwnerAlive = persistsWhileOwnerAlive;
    OwnerInactiveEndPolicy = ownerInactiveEndPolicy;
  }

  public SummonedProjectileKind Kind;
  public float SlotCost;
  public int OwnerOrder;
  public bool PersistsWhileOwnerAlive;
  public SummonedOwnerEndPolicy OwnerInactiveEndPolicy;
}
