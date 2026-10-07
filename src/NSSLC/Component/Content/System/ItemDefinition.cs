namespace Terraria.Content;

public sealed record ItemDefinition(
  ItemIdentityDefinition Identity,
  ItemUseDefinition Use,
  ItemStackDefinition Stack,
  ItemCombatDefinition Combat,
  ItemPlacementDefinition Placement,
  ItemEffectDefinition Effects,
  ItemEquipmentDefinition Equipment,
  ItemEconomyDefinition Economy,
  ItemCapabilitiesDefinition Capabilities)
{
  public ItemToolDefinition Tool { get; init; } = new();

  public ItemPresentationDefinition Presentation { get; init; } = new();
}
