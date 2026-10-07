namespace Terraria.Player;

public readonly record struct PlayerMaleChangeCommand(
  bool HasChange,
  int SkinVariant);
