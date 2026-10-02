namespace Terraria.ExternalPlatformBoundaries.Social.WeGame;

public readonly record struct WeGameIpcTransportStatus(
  bool IsOpen,
  bool IsBroken,
  int PendingFrameCount,
  int BufferSize,
  int Generation);
