namespace Terraria.Player.Combat;

public readonly record struct PlayerArmorAndCombatEffectsRebuildInput(
  bool ThornsBuffActive,
  bool DryadWardActive,
  bool TurtleArmorEquipped,
  bool TurtleSetBonusActive,
  bool CactusSetBonusActive,
  bool SpiderArmorEquipped,
  bool AnglerSetBonusActive);
