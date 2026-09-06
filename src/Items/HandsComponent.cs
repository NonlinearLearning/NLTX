using Terraria.Relationships;

namespace Terraria.Items;

public struct HandsComponent
{
  public HandsComponent(EntityReference primary, EntityReference secondary)
  {
    Primary = primary;
    Secondary = secondary;
  }

  public EntityReference Primary;
  public EntityReference Secondary;

  public bool HasPrimary => !Primary.IsEmpty;
  public bool HasSecondary => !Secondary.IsEmpty;
}
