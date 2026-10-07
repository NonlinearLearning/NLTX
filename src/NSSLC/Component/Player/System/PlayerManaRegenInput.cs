namespace Terraria.Player;

public readonly record struct PlayerManaRegenInput(
  bool IsConsideredStandingStill,
  bool IsGrappling,
  bool UsedArcaneCrystal,
  bool ManaRegenBuff);
