namespace Terraria.NonAuthoritative.Persistence;

public enum WorldFileCommitStatus
{
  Committed,
  Rejected,
  OutcomeUnknown
}

public readonly record struct WorldFileCommitResult(
  WorldFileCommitStatus Status,
  WorldStorageFailure Failure)
{
  public static WorldFileCommitResult Committed =>
    new(WorldFileCommitStatus.Committed, WorldStorageFailure.None);

  public static WorldFileCommitResult Rejected(WorldStorageFailure failure)
  {
    return new(
      WorldFileCommitStatus.Rejected,
      failure.Kind == WorldStorageFailureKind.None
        ? WorldStorageFailure.Create(WorldStorageFailureKind.IoFailure)
        : failure);
  }

  public static WorldFileCommitResult Unknown(WorldStorageFailure failure)
  {
    return new(
      WorldFileCommitStatus.OutcomeUnknown,
      failure.Kind == WorldStorageFailureKind.None
        ? WorldStorageFailure.Create(WorldStorageFailureKind.Unknown)
        : failure);
  }
}
