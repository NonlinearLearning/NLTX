using Terraria.WorldGeneration.Actions;

namespace Terraria.WorldGeneration.Adapters;

public interface IWorldGenerationActionCommitPort
{
  WorldGenerationActionCommitResult Commit(in WorldGenerationAction action);
}
