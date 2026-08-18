namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerControlIntent(
  byte PlayerSlot,
  bool MoveLeft,
  bool MoveRight,
  bool Jump,
  bool UseItem,
  bool FacingRight,
  byte SelectedItem);
