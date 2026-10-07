namespace Terraria.Player;

/// <summary>
/// Decoded type 16 fields. Session binding and byte decoding remain outside this type.
/// </summary>
public readonly record struct PlayerVitalPacket16Input(
  byte DeclaredPlayerSlot,
  int LocalPlayerSlot,
  LegacyPlayerSlot AuthenticatedSenderSlot,
  bool HasAuthenticatedSender,
  bool IsServerSideCharacter,
  PlayerLifecycleSystem.SpectatingNetworkMode NetworkMode,
  int StatLife,
  int StatLifeMax);
