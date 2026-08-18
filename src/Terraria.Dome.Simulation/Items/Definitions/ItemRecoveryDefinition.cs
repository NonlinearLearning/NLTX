namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ItemRecoveryDefinition(
  int Health = 0,
  int Mana = 0,
  ushort BuffType = 0,
  int BuffDurationTicks = 0,
  bool Consumable = true);
