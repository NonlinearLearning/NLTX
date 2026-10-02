using System;

namespace Terraria.Player;

/// <summary>
/// Decoded type 12 fields. Session binding and byte decoding remain outside this type.
/// </summary>
public readonly record struct PlayerSpawnPacket12Input(
  int DeclaredPlayerSlot,
  int AuthenticatedSenderSlot,
  bool HasAuthenticatedSender,
  PlayerLifecycleSystem.SpectatingNetworkMode NetworkMode,
  int SpawnX,
  int SpawnY,
  int RespawnRemainingTicks,
  int PveDeathCount,
  int PvpDeathCount,
  int TeamId,
  byte SpawnContextValue,
  bool IsLocalPlayer,
  bool MultiplayerBroadcast,
  long LastTimePlayerWasSavedBinary,
  DateTime CurrentUtcTime);
