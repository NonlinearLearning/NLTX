namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ItemRecoveryDefinition(
  int Health = 0,
  int Mana = 0,
  ushort BuffType = 0,
  int BuffDurationTicks = 0,
  bool Consumable = true,
  int PotionDelayTicks = 0,
  int FlaskDurationTicks = 0,
  int FoodWidth = 22,
  int FoodHeight = 22,
  byte LuckPotionLevel = 0,
  ItemRecoveryDelayKind DelayKind = ItemRecoveryDelayKind.None);
