using System;

namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldGenerationLifecycleComponent
{
  public WorldGenerationLifecycleComponent(
    long generationId,
    WorldPreparationState phase = WorldPreparationState.Uninitialized,
    WorldGenerationFailure failure = WorldGenerationFailure.None,
    ulong generationRevision = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (!Enum.IsDefined(phase))
    {
      throw new ArgumentOutOfRangeException(nameof(phase));
    }

    if (!Enum.IsDefined(failure))
    {
      throw new ArgumentOutOfRangeException(nameof(failure));
    }

    if (phase == WorldPreparationState.Failed &&
        failure == WorldGenerationFailure.None)
    {
      throw new ArgumentException(
        "A failed lifecycle must carry a failure category.",
        nameof(failure));
    }

    GenerationId = generationId;
    Phase = phase;
    Failure = failure;
    GenerationRevision = generationRevision;
  }

  public long GenerationId { get; }

  public WorldPreparationState Phase { get; }

  public WorldGenerationFailure Failure { get; }

  public ulong GenerationRevision { get; }

  public bool IsLoadingOrGenerating =>
    Phase == WorldPreparationState.Loading ||
    Phase == WorldPreparationState.Generating;

  public bool IsPhaseReady =>
    Phase == WorldPreparationState.Ready &&
    Failure == WorldGenerationFailure.None;

  public bool IsReady => IsPhaseReady;

  public bool HasFailed =>
    Phase == WorldPreparationState.Failed ||
    Failure != WorldGenerationFailure.None;
}
