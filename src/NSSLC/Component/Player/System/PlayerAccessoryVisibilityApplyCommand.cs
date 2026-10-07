namespace Terraria.Player;

public readonly record struct PlayerAccessoryVisibilityApplyCommand(
  Guid CommandId,
  ushort VisibilityMask);
