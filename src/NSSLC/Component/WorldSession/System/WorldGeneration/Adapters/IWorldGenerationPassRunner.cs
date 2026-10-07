using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Passes;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Runs one active pass synchronously and reports its run measurements.
/// </summary>
/// <remarks>
/// Implementations must finish before returning, invoke progress serially on the calling thread,
/// and must not retain the progress callback.
/// </remarks>
public interface IWorldGenerationPassRunner
{
  WorldGenerationPassRunOutput Execute(
    in WorldGenerationPassStateComponent activeState,
    GenerationPassDescriptor descriptor,
    WorldGenerationConfigurationSnapshot configuration,
    Action<string, double> reportProgress);
}
