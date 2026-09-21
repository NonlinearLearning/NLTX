using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldSaveProjection
{
  public WorldSaveProjection(
    bool committed,
    IReadOnlyList<WorldSaveStage> stages,
    FilePlatformFailure failure)
  {
    Committed = committed;
    Stages = Array.AsReadOnly(stages.ToArray());
    Failure = failure;
  }

  public bool Committed { get; }

  public IReadOnlyList<WorldSaveStage> Stages { get; }

  public FilePlatformFailure Failure { get; }
}
