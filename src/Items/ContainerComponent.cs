using Terraria.Relationships;

namespace Terraria.Items;

public sealed class ContainerComponent
{
  public ContainerComponent(int capacity, IReadOnlyList<EntityReference> contents)
  {
    Capacity = capacity;
    Contents = new List<EntityReference>(contents);
  }

  public int Capacity;
  public List<EntityReference> Contents;
}
