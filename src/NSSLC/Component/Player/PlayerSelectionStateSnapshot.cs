namespace Terraria.Player;

public readonly record struct PlayerSelectionStateSnapshot(
  int Selected,
  int Hotbar,
  int Buffered,
  int Overridden,
  bool CanChangeSelectedItemImmediately,
  bool HasActiveOverride,
  bool HasBufferedChange,
  int LastNonOverridenSelection);
