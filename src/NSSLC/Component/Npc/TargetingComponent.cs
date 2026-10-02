using Terraria.Relationships;

namespace Terraria.Npc;

public struct TargetingComponent
{
  public TargetingComponent(EntityReference target)
  {
    Target = target;
  }

  public EntityReference Target;
}
