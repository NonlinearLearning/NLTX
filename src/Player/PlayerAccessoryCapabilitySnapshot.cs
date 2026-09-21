namespace Terraria.Player;

public readonly record struct PlayerAccessoryCapabilitySnapshot(
  int ExtraAccessorySlots,
  bool ExtraAccessory,
  int TankPet,
  bool TankPetReset,
  int StringColor,
  int CounterWeight,
  int VanityCounterWeight,
  bool MagicString,
  bool YoyoString,
  bool YoyoGlove,
  float RapidAttackBonus,
  bool StressBall,
  bool StressBallPrevious,
  bool StaffOfRegrowthBonus);
