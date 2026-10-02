namespace NLTX.PlayerInputGameplay.PressurePlates;

public readonly record struct PressurePlateCoordinate(int X, int Y);

public readonly record struct PressurePlateTransition(
  PressurePlateCoordinate Plate,
  int PlayerSlot,
  bool IsPressed);
