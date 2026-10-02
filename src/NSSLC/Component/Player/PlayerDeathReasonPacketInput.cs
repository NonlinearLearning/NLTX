namespace Terraria.Player;

/// <summary>
/// Optional source fields read by the type 118 death-reason payload.
/// </summary>
public readonly record struct PlayerDeathReasonPacketInput(
  short? SourcePlayerIndex,
  short? SourceNpcIndex,
  short? SourceProjectileLocalIndex,
  byte? SourceOtherIndex,
  short? SourceProjectileType,
  short? SourceItemType,
  byte? SourceItemPrefix,
  string? SourceCustomReason);
