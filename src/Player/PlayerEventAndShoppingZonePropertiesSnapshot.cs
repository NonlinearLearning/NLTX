namespace Terraria.Player;

public readonly record struct PlayerEventAndShoppingZonePropertiesSnapshot(
  bool ZoneOldOneArmy,
  bool ZoneLihzhardTemple,
  bool ZoneGraveyard,
  bool ZoneShadowCandle,
  bool ZoneShimmer,
  bool ShoppingZoneAnyBiome,
  bool ShoppingZoneBelowSurface);
