namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldSaveProjection
{
  public WorldSaveProjection(
    bool committed,
    IReadOnlyList<WorldSaveStage> stages,
    WorldStorageFailure failure)
  {
    ArgumentNullException.ThrowIfNull(stages);
    if (committed && failure.Kind != WorldStorageFailureKind.None)
    {
      throw new ArgumentException(
        "A committed save projection cannot contain a failure.",
        nameof(failure));
    }

    if (!committed && failure.Kind == WorldStorageFailureKind.None)
    {
      throw new ArgumentException(
        "A rejected save projection requires a failure.",
        nameof(failure));
    }

    Committed = committed;
    Stages = Array.AsReadOnly(stages.ToArray());
    Failure = failure;
  }

  public bool Committed { get; }

  public IReadOnlyList<WorldSaveStage> Stages { get; }

  public WorldStorageFailure Failure { get; }
}
