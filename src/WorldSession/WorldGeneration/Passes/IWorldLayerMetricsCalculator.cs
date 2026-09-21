using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Passes;

public interface IWorldLayerMetricsCalculator
{
  WorldLayerMetricsSnapshot Calculate(
    in WorldLayerMetricsCalculationInput input,
    IGenerationRandomSource random);
}
