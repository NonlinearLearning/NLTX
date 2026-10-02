namespace Terraria.Player;

public sealed class PlayerStarCloakSourceRelationComponent
{
  public ItemEntityRef StarCloakItem { get; internal set; } = ItemEntityRef.None;

  public ItemEntityRef StarCloakItemManaCloakOverrideItem { get; internal set; } =
    ItemEntityRef.None;

  public ItemEntityRef StarCloakItemStarVeilOverrideItem { get; internal set; } =
    ItemEntityRef.None;

  public ItemEntityRef StarCloakItemBeeCloakOverrideItem { get; internal set; } =
    ItemEntityRef.None;

  internal void ResetEffects()
  {
    StarCloakItem = ItemEntityRef.None;
    StarCloakItemManaCloakOverrideItem = ItemEntityRef.None;
    StarCloakItemStarVeilOverrideItem = ItemEntityRef.None;
    StarCloakItemBeeCloakOverrideItem = ItemEntityRef.None;
  }
}
