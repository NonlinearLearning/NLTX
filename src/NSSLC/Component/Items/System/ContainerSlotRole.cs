namespace Terraria.Items;

public readonly record struct ContainerSlotRole(string Name)
{
  public bool IsDefined => !string.IsNullOrWhiteSpace(Name);
}
