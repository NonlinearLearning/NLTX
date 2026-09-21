namespace Terraria.Player;

public readonly record struct PlayerInteractionUiStateInput(
  bool CreativeInterface,
  bool MouseInterface,
  bool LastMouseInterface,
  int NoThrow);
