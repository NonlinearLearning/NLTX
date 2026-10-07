namespace Terraria.Player;

public readonly record struct PlayerRestPacket13Input(
  byte DeclaredPlayerSlot,
  int LocalPlayerSlot,
  LegacyPlayerSlot AuthenticatedSenderSlot,
  bool HasAuthenticatedSender,
  bool IsServerSideCharacter,
  PlayerLifecycleSystem.SpectatingNetworkMode NetworkMode,
  bool IsPetting,
  bool IsPetSmall,
  bool IsSitting,
  bool IsSleeping,
  bool CanRelayToPeers = false);
