using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns the generation lifecycle transition table for one generation session.
/// </summary>
public static class WorldGenerationLifecycleSystem
{
  public static WorldGenerationLifecycleComponent Create(long generationId)
  {
    return new WorldGenerationLifecycleComponent(generationId);
  }

  public static WorldGenerationLifecycleComponent BeginLoading(
    in WorldGenerationLifecycleComponent current,
    long expectedGenerationId)
  {
    return Transition(
      in current,
      expectedGenerationId,
      WorldPreparationState.Loading,
      WorldGenerationFailure.None);
  }

  public static WorldGenerationLifecycleComponent BeginGenerating(
    in WorldGenerationLifecycleComponent current,
    long expectedGenerationId)
  {
    return Transition(
      in current,
      expectedGenerationId,
      WorldPreparationState.Generating,
      WorldGenerationFailure.None);
  }

  public static WorldGenerationLifecycleComponent MarkReady(
    in WorldGenerationLifecycleComponent current,
    long expectedGenerationId)
  {
    return Transition(
      in current,
      expectedGenerationId,
      WorldPreparationState.Ready,
      WorldGenerationFailure.None);
  }

  public static WorldGenerationLifecycleComponent Fail(
    in WorldGenerationLifecycleComponent current,
    long expectedGenerationId,
    WorldGenerationFailure failure)
  {
    if (failure == WorldGenerationFailure.None)
    {
      throw new ArgumentException(
        "A failed generation must carry a failure category.",
        nameof(failure));
    }

    return Transition(
      in current,
      expectedGenerationId,
      WorldPreparationState.Failed,
      failure);
  }

  public static WorldGenerationLifecycleComponent BeginUnloading(
    in WorldGenerationLifecycleComponent current,
    long expectedGenerationId)
  {
    return Transition(
      in current,
      expectedGenerationId,
      WorldPreparationState.Unloading,
      WorldGenerationFailure.None);
  }

  public static WorldGenerationLifecycleComponent Reset(
    in WorldGenerationLifecycleComponent current,
    long expectedGenerationId)
  {
    EnsureGeneration(in current, expectedGenerationId);
    if (current.Phase != WorldPreparationState.Unloading)
    {
      throw new InvalidOperationException(
        "A generation can only reset after it enters the unloading phase.");
    }

    return Next(
      current.GenerationId,
      WorldPreparationState.Uninitialized,
      WorldGenerationFailure.None,
      current.GenerationRevision);
  }

  private static WorldGenerationLifecycleComponent Transition(
    in WorldGenerationLifecycleComponent current,
    long expectedGenerationId,
    WorldPreparationState nextPhase,
    WorldGenerationFailure failure)
  {
    EnsureGeneration(in current, expectedGenerationId);
    if (!IsAllowed(current.Phase, nextPhase))
    {
      throw new InvalidOperationException(
        $"The lifecycle cannot transition from {current.Phase} to {nextPhase}.");
    }

    return Next(
      current.GenerationId,
      nextPhase,
      failure,
      current.GenerationRevision);
  }

  private static bool IsAllowed(
    WorldPreparationState current,
    WorldPreparationState next)
  {
    return (current, next) switch
    {
      (WorldPreparationState.Uninitialized, WorldPreparationState.Loading) => true,
      (WorldPreparationState.Loading, WorldPreparationState.Generating) => true,
      (WorldPreparationState.Loading, WorldPreparationState.Failed) => true,
      (WorldPreparationState.Loading, WorldPreparationState.Unloading) => true,
      (WorldPreparationState.Generating, WorldPreparationState.Ready) => true,
      (WorldPreparationState.Generating, WorldPreparationState.Failed) => true,
      (WorldPreparationState.Generating, WorldPreparationState.Unloading) => true,
      (WorldPreparationState.Ready, WorldPreparationState.Unloading) => true,
      (WorldPreparationState.Failed, WorldPreparationState.Unloading) => true,
      _ => false
    };
  }

  private static WorldGenerationLifecycleComponent Next(
    long generationId,
    WorldPreparationState phase,
    WorldGenerationFailure failure,
    ulong currentRevision)
  {
    return new WorldGenerationLifecycleComponent(
      generationId,
      phase,
      failure,
      checked(currentRevision + 1));
  }

  private static void EnsureGeneration(
    in WorldGenerationLifecycleComponent current,
    long expectedGenerationId)
  {
    if (expectedGenerationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(expectedGenerationId));
    }

    if (current.GenerationId != expectedGenerationId)
    {
      throw new ArgumentException(
        "The lifecycle belongs to another generation.",
        nameof(expectedGenerationId));
    }
  }
}
