namespace Terraria.Player;

public readonly record struct PlayerRestNetworkApplyResult(
  PlayerRestActivity PreviousActivities,
  PlayerRestActivity AppliedActivities,
  bool ActivitiesChanged,
  bool SleepingStateChanged,
  bool PettingSizeChanged);
