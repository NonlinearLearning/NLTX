using System;

namespace Terraria.WorldStorage;

public readonly record struct WorldLoadCommitResult
{
  private WorldLoadCommitResult(bool succeeded, WorldLoadApiFailure failure)
  {
    Succeeded = succeeded;
    Failure = failure;
  }

  public bool Succeeded { get; }

  public WorldLoadApiFailure Failure { get; }

  public static WorldLoadCommitResult Committed()
  {
    return new WorldLoadCommitResult(true, default);
  }

  public static WorldLoadCommitResult Rejected(WorldLoadApiFailure failure)
  {
    if (!failure.IsValid)
    {
      throw new ArgumentException("A rejected load result requires a valid failure.", nameof(failure));
    }

    return new WorldLoadCommitResult(false, failure);
  }
}
