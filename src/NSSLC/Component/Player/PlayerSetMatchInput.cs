namespace Terraria.Player;

public readonly record struct PlayerSetMatchInput(
  int Head,
  int Body,
  int Legs,
  int ArmorSlotRequested,
  bool Male,
  bool MountActive,
  int MountType,
  bool SomethingSpecial);
