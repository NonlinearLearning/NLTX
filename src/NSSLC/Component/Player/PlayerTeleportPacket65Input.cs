namespace Terraria.Player;

/// <summary>
/// Decoded type 65 fields. Slot selection and packet/acknowledgement routing remain external.
/// </summary>
public readonly record struct PlayerTeleportPacket65Input(
  short DeclaredEntitySlot,
  int LocalPlayerSlot,
  LegacyPlayerSlot AuthenticatedSenderSlot,
  bool HasAuthenticatedSender,
  PlayerLifecycleSystem.SpectatingNetworkMode NetworkMode,
  byte TeleportFlags,
  WorldPosition Position,
  byte Style,
  int? ExtraInfo);
