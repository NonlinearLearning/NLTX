namespace Terraria.Player;

/// <summary>
/// Decoded type 118 fields. Slot selection and death/packet effects remain external.
/// </summary>
public readonly record struct PlayerDeathPacket118Input(
  byte DeclaredPlayerSlot,
  LegacyPlayerSlot AuthenticatedSenderSlot,
  bool HasAuthenticatedSender,
  PlayerLifecycleSystem.SpectatingNetworkMode NetworkMode,
  PlayerDeathReasonPacketInput DeathReason,
  short Damage,
  int HitDirection,
  bool IsPvp);
