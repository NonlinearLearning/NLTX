namespace Terraria.WorldGeneration.Components;

public sealed class WorldGenerationLifecycleState
{
  public WorldPreparationState Phase;
  public WorldGenerationFailure Failure;
  public ulong GenerationRevision;

  public bool IsLoadingOrGenerating =>
    Phase == WorldPreparationState.Loading ||
    Phase == WorldPreparationState.Generating;
  public bool IsReady => Phase == WorldPreparationState.Ready;
  public bool CanUpdateSimulation => IsReady;
  public bool HasFailed => Phase == WorldPreparationState.Failed;
}
