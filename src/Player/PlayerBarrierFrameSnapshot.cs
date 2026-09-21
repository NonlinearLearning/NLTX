namespace Terraria.Player;

public readonly record struct PlayerBarrierFrameSnapshot(
  bool IceBarrier,
  byte IceBarrierFrame,
  byte IceBarrierFrameCounter);
