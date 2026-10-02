namespace Terraria.Npc;

public readonly record struct NpcDeathAnnouncementIntent(
  string LocalizationKey,
  byte Red,
  byte Green,
  byte Blue);
