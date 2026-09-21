namespace Terraria.Player;

public readonly record struct PlayerAccessoryEffectRebuildInput(
  bool ExtraAccessory,
  int TankPet,
  int StringColor,
  int CounterWeight,
  int VanityCounterWeight,
  bool MagicString,
  bool YoyoString,
  bool YoyoGlove,
  float RapidAttackBonus,
  bool StressBall,
  bool StaffOfRegrowthBonus);
