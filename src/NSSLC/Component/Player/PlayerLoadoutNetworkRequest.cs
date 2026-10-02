namespace Terraria.Player;

public readonly record struct PlayerLoadoutNetworkRequest(
  Guid CommandId,
  int PacketPlayerIndex,
  int AuthorityPlayerIndex,
  int MainPlayerIndex,
  int TargetLoadoutIndex,
  bool UsingOrReusingItem,
  bool CCed,
  bool Dead,
  ushort VisibilityMask);
