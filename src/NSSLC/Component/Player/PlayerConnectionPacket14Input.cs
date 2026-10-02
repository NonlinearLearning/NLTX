namespace Terraria.Player;

/// <summary>
/// Decoded type 14 client update. Slot binding remains outside this adapter.
/// </summary>
public readonly record struct PlayerConnectionPacket14Input(
  int DeclaredPlayerSlot,
  bool IsActive,
  PlayerLifecycleSystem.SpectatingNetworkMode NetworkMode);
