namespace Terraria.Player;

public readonly record struct PlayerSpawnPacket12Result(
  PlayerSpawnPacket12Status Status,
  LegacyPlayerSlot? EffectivePlayerSlot,
  PlayerSpawnPacket12Context? SpawnContext,
  PlayerLifecycleSystem.SpawnCommitResult CommitResult);
