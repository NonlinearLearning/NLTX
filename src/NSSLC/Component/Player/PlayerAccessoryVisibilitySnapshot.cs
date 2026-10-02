namespace Terraria.Player;

public readonly record struct PlayerAccessoryVisibilitySnapshot(
  ushort VisibilityMask,
  IReadOnlyList<bool> HiddenAccessories);
