namespace Terraria.Player;

public readonly record struct PlayerDamageMitigationInput(
  int DamageAmount,
  int Defense,
  float Endurance,
  bool Critical);
