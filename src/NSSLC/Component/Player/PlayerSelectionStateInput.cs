namespace Terraria.Player;

public readonly record struct PlayerSelectionStateInput(
  int Selected,
  int Hotbar,
  int Buffered,
  int Overridden,
  bool IsUsingOrReusingItem,
  bool ItemTimeIsZero);
