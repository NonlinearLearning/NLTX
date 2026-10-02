namespace Terraria.Player;

public readonly record struct PlayerConnectionPacket14Result(
  PlayerConnectionPacket14Status Status,
  LegacyPlayerSlot? EffectivePlayerSlot,
  PlayerLifecycleSystem.ConnectionTransition Transition);
