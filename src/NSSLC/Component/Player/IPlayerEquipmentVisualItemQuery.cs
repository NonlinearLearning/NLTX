namespace Terraria.Player;

// Item payload and ArmorID table ownership stay outside P09. This query exposes
// only the immutable fields required by the dye and visible-slot projections.
public interface IPlayerEquipmentVisualItemQuery
{
  bool TryGetItem(
    ItemEntityRef item,
    out PlayerEquipmentVisualItemSnapshot snapshot);
}
