using Terraria.Player.Combat;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

var effects = new PlayerArmorAndCombatEffectsComponent();
PlayerArmorAndCombatEffectsSystem.Rebuild(default, effects);
Assert(effects.Thorns == 0f &&
  !effects.TurtleArmor &&
  !effects.TurtleThorns &&
  !effects.CactusThorns &&
  !effects.SpiderArmor &&
  !effects.AnglerSetSpawnReduction &&
  !effects.VampireBurningInSunlight,
  "A default rebuild should clear the resolved armor and combat facts.");

PlayerArmorAndCombatEffectsSystem.Rebuild(
  new PlayerArmorAndCombatEffectsRebuildInput(
    ThornsBuffActive: true,
    DryadWardActive: false,
    TurtleArmorEquipped: false,
    TurtleSetBonusActive: false,
    CactusSetBonusActive: false,
    SpiderArmorEquipped: false,
    AnglerSetBonusActive: false),
  effects);
Assert(effects.Thorns == 1f,
  "The thorns buff should rebuild thorns to one.");

PlayerArmorAndCombatEffectsSystem.Rebuild(
  new PlayerArmorAndCombatEffectsRebuildInput(
    ThornsBuffActive: false,
    DryadWardActive: true,
    TurtleArmorEquipped: false,
    TurtleSetBonusActive: false,
    CactusSetBonusActive: false,
    SpiderArmorEquipped: false,
    AnglerSetBonusActive: false),
  effects);
Assert(effects.Thorns == 0.5f,
  "Dryad's Ward should contribute half thorns when no full thorns source is active.");

PlayerArmorAndCombatEffectsSystem.Rebuild(
  new PlayerArmorAndCombatEffectsRebuildInput(
    ThornsBuffActive: true,
    DryadWardActive: true,
    TurtleArmorEquipped: false,
    TurtleSetBonusActive: false,
    CactusSetBonusActive: false,
    SpiderArmorEquipped: false,
    AnglerSetBonusActive: false),
  effects);
Assert(effects.Thorns == 1f,
  "The thorns buff should keep the Dryad's Ward contribution capped at one.");

var armorSetFacts = new PlayerArmorAndCombatEffectsRebuildInput(
  ThornsBuffActive: false,
  DryadWardActive: false,
  TurtleArmorEquipped: true,
  TurtleSetBonusActive: true,
  CactusSetBonusActive: true,
  SpiderArmorEquipped: true,
  AnglerSetBonusActive: true);
PlayerArmorAndCombatEffectsSystem.Rebuild(armorSetFacts, effects);
PlayerArmorAndCombatEffectsSystem.Rebuild(armorSetFacts, effects);
Assert(effects.Thorns == 1f &&
  effects.TurtleArmor &&
  effects.TurtleThorns &&
  effects.CactusThorns &&
  effects.SpiderArmor &&
  effects.AnglerSetSpawnReduction &&
  !effects.VampireBurningInSunlight,
  "Armor facts should be replaced from the current frame without accumulating on repeat.");

Console.WriteLine("PASS: player armor and combat effect rebuild");
