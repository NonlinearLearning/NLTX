namespace Terraria.Player;

public readonly record struct PlayerSetMatchCompositionInput(
  int Head,
  int Body,
  int Legs,
  bool Male,
  bool MountActive,
  int MountType);
