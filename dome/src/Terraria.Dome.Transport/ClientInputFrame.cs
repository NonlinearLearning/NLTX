namespace Terraria.Dome.Transport;

public readonly record struct ClientInputFrame(
  bool MoveLeft = false,
  bool MoveRight = false,
  bool Jump = false,
  bool Fire = false);
