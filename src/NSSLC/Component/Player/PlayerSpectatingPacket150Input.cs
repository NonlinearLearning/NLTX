namespace Terraria.Player;

/// <summary>
/// Decoded type 150 data plus the transport identity and resolved target facts.
/// </summary>
public readonly record struct PlayerSpectatingPacket150Input(
  int DeclaredPlayerSlot,
  int TargetPlayerSlot,
  int AuthenticatedSenderSlot,
  bool HasAuthenticatedSender,
  PlayerLifecycleSystem.SpectatingNetworkMode NetworkMode,
  bool IsLocalPlayer,
  int TargetWhoAmI,
  bool TargetIsActive,
  bool TargetIsDead,
  int TargetDeadElapsedTicks);
