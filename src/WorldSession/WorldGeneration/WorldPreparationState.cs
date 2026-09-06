namespace Terraria.WorldGeneration.Components;

public enum WorldPreparationState : byte
{
  Uninitialized,
  Loading,
  Generating,
  Ready,
  Failed,
  Unloading,
}
