namespace Terraria.WorldSession.NpcProgression.LunarTower;

public readonly record struct LunarTowerPresenceSnapshot(
  bool SolarActive,
  bool VortexActive,
  bool NebulaActive,
  bool StardustActive,
  bool MoonLordActive);
