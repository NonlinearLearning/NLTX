namespace Terraria.Player;

public readonly record struct PlayerLoadoutSwitchCommand(
  Guid CommandId,
  int TargetLoadoutIndex,
  int PlayerIndex,
  int MainPlayerIndex,
  bool UsingOrReusingItem,
  bool CCed,
  bool Dead);
