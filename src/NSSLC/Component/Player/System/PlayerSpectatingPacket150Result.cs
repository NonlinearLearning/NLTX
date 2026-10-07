namespace Terraria.Player;

public readonly record struct PlayerSpectatingPacket150Result(
  PlayerSpectatingPacket150Status Status,
  LegacyPlayerSlot? EffectivePlayerSlot,
  PlayerLifecycleSystem.SpectatingTargetResult TargetResult);
