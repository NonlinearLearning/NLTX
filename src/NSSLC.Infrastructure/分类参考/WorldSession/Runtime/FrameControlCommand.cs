namespace Terraria.WorldSession.Runtime;

public readonly record struct FrameControlCommand(
  bool GamePaused,
  bool MaxQueryEnabled);
