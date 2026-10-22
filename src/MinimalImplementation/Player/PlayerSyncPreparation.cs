namespace MinimalImplementation.Server.Player;

public sealed record PlayerSyncPreparation(
  bool Success,
  PlayerAppearance? Appearance,
  MinimalImplementation.Contracts.ProtocolErrorCode? ErrorCode,
  string? RejectionMessage)
{
  public bool IsPrepared => Success && Appearance is not null;
}
