namespace Terraria.Player;

public readonly record struct PlayerPacket13RouteResult(
  PlayerPacket13RouteDecision Decision,
  LegacyPlayerSlot? EffectivePlayerSlot);
