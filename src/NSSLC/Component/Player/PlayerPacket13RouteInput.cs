namespace Terraria.Player;

public readonly record struct PlayerPacket13RouteInput(
  byte DeclaredPlayerSlot,
  int LocalPlayerSlot,
  bool IsServerSideCharacter,
  LegacyPlayerSlot SenderPlayerSlot);
