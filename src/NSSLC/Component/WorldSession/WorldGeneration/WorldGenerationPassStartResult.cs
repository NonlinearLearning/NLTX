namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldGenerationPassStartResult(
  WorldGenerationPassStartStatus Status,
  WorldGenerationPassStateComponent State)
{
  public bool Accepted => true;

  public bool Started => Status == WorldGenerationPassStartStatus.Started;

  public bool Skipped => Status == WorldGenerationPassStartStatus.SkippedDisabled;
}
