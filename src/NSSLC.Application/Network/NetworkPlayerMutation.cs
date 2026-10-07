namespace Terraria.Network;

public enum NetworkPlayerMutationStatus : byte
{
  Applied,
  NotFound,
  RejectedStage,
  RejectedSenderBinding,
  RejectedWorldRuntime,
  RejectedInvalidState,
}

public readonly record struct NetworkPlayerMutationResult(
  NetworkPlayerMutationStatus Status,
  NetworkPlayerSnapshot? Player)
{
  public bool Succeeded => Status == NetworkPlayerMutationStatus.Applied;
}
