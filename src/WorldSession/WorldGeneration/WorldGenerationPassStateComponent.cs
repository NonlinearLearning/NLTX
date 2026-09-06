using System;

namespace Terraria.WorldGeneration.Components;

public struct WorldGenerationPassStateComponent
{
  public WorldGenerationPassStateComponent(
    long generationId,
    WorldGenerationStage stage = WorldGenerationStage.Created,
    string? activePassId = null,
    int passVersion = 0,
    long cursor = 0,
    ulong? readSnapshotRevision = null,
    ulong? checkpointRevision = null,
    string? failureReason = null,
    bool pauseRequested = false,
    bool abortRequested = false)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (!Enum.IsDefined(stage))
    {
      throw new ArgumentOutOfRangeException(nameof(stage));
    }

    if (cursor < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cursor));
    }

    if (passVersion < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(passVersion));
    }

    if (activePassId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(activePassId);
    }

    if (activePassId is null && passVersion != 0)
    {
      throw new ArgumentException(
        "An inactive pass cannot carry a pass version.",
        nameof(passVersion));
    }

    if (activePassId is not null && passVersion <= 0)
    {
      throw new ArgumentException(
        "An active pass must carry a positive pass version.",
        nameof(passVersion));
    }

    GenerationId = generationId;
    Stage = stage;
    ActivePassId = activePassId;
    PassVersion = passVersion;
    Cursor = cursor;
    ReadSnapshotRevision = readSnapshotRevision;
    CheckpointRevision = checkpointRevision;
    FailureReason = failureReason;
    PauseRequested = pauseRequested;
    AbortRequested = abortRequested;
  }

  public long GenerationId;

  public WorldGenerationStage Stage;

  public string? ActivePassId;

  public int PassVersion;

  public long Cursor;

  public ulong? ReadSnapshotRevision;

  public ulong? CheckpointRevision;

  public string? FailureReason;

  public bool PauseRequested;

  public bool AbortRequested;

  public bool HasActivePass => ActivePassId is not null;

  public bool HasFailure => !string.IsNullOrWhiteSpace(FailureReason);
}
