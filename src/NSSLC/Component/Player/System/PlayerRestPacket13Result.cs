namespace Terraria.Player;

public readonly record struct PlayerRestPacket13Result(
  PlayerRestPacket13Status Status,
  LegacyPlayerSlot? EffectivePlayerSlot,
  PlayerRestNetworkApplyResult ApplyResult,
  bool ShouldBroadcast = false);
