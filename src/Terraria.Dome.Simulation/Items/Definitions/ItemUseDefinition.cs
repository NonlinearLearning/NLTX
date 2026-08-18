namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ItemUseDefinition(
  int UseTime = 0,
  int UseAnimation = 0,
  int UseStyle = 0,
  bool Channel = false,
  bool AutoReuse = false,
  bool UseTurn = false,
  bool Consumable = false,
  int HealthRestore = 0,
  int ManaRestore = 0,
  int ManaCost = 0,
  int CooldownTicks = 0,
  ushort ShootType = 0,
  float ShootSpeed = 0,
  ushort AmmoType = 0,
  bool ConsumesAmmo = false);
